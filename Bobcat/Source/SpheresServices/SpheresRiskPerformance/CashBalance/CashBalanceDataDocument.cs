using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;


using EFS.GUI.Interface;
using EFS.Process;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.TradeInformation;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;

using EfsML.v30.AssetDef;
using EfsML.v30.CashBalance;
using EfsML.v30.Fix;
using EfsML.v30.Shared;

using FixML.Enum;
using FixML.v50SP1.Enum;

using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

using Tz = EFS.TimeZone;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// 
    /// </summary>
    public class CBDataDocument
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuotedCurrencyPair"></param>
        /// <returns></returns>
        // PM 20190701 [24761] private to public
        public static string GetFxRateId(QuotedCurrencyPair pQuotedCurrencyPair)
        {
            string ret;
            if (pQuotedCurrencyPair.quoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                ret = pQuotedCurrencyPair.currency1.Value + "per" + pQuotedCurrencyPair.currency2.Value;
            else
                ret = pQuotedCurrencyPair.currency2.Value + "per" + pQuotedCurrencyPair.currency1.Value;
            return ret;
        }

        /// <summary>
        /// Retourne un ExchangeCashPosition en fonction d'un money dont le montant est signé
        /// </summary>
        /// <param name="pMoney">le montant est signé</param>
        /// <param name="pExAmount">le montant est signé et en devise de contravaleur</param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pIsCBOWithMGCCTRVal">contrevaleur uniquement si true</param>
        /// <returns></returns>
        private static ExchangeCashPosition GetExchangeCashPosition(Money pMoney, CBExAmount pExAmount,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            string pDateHRef, bool pIsCBOWithMGCCTRVal)
        {
            CashPosition cashPosition = GetCashPosition(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
            ExchangeCashPosition ret = new ExchangeCashPosition(cashPosition);
            //
            if (pIsCBOWithMGCCTRVal && (pExAmount != null))
            {
                ret.exchangeAmountSpecified = true;
                ret.exchangeAmount = new Money(System.Math.Abs(pExAmount.CurrencyAmount.Amount.DecValue), pExAmount.CurrencyAmount.Currency);

                List<FxRateReference> rateList = new List<FxRateReference>();
                FxRateReference rate;

                foreach (CBQuote quote in pExAmount.Quote)
                {
                    rate = new FxRateReference
                    {
                        href = GetFxRateId(quote.Rate.quotedCurrencyPair)
                    };
                    rateList.Add(rate);
                }

                ret.exchangeFxRateReferenceSpecified = (rateList.Count > 0);
                if (ret.exchangeFxRateReferenceSpecified)
                    ret.exchangeFxRateReference = rateList.ToArray();
            }

            return ret;
        }

        #region GetCashPosition
        /// <summary>
        /// Renseigne un CashPosition en fonction d'un Money dont le montant est signé
        /// </summary>
        /// <param name="pMoney">le montant est signé</param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <param name="pCashPosition">CashPosition renseigné</param>
        private static void GetCashPosition(Money pMoney,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            string pDateHRef, Nullable<DateTime> pDate, CashPosition pCashPosition)
        {
            if (pCashPosition != default(CashPosition))
            {
                GetPayerReceiver(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, out string payer, out string receiver);
                pCashPosition.payerPartyReference = new PartyOrAccountReference(payer);
                pCashPosition.receiverPartyReference = new PartyOrAccountReference(receiver);

                pCashPosition.amount = (Money)pMoney.Clone();
                pCashPosition.amount.amount.DecValue = System.Math.Abs(pCashPosition.amount.amount.DecValue);

                if (StrFunc.IsFilled(pDateHRef))
                {
                    pCashPosition.dateReferenceSpecified = true;
                    pCashPosition.dateReference = new DateReference
                    {
                        href = pDateHRef
                    };
                }
                else if (pDate.HasValue && pDate.Value != DateTime.MinValue)
                {
                    pCashPosition.dateDefineSpecified = true;
                    pCashPosition.dateDefine = new IdentifiedDate
                    {
                        DateValue = pDate.Value
                    };
                }
                else
                {
                    throw new ArgumentException("pDateHRef and  pDate are not specified");
                }
            }
        }
        /// <summary>
        /// Retourne un CashPosition en fonction d'un money dont le montant est signé
        /// </summary>
        /// <param name="pMoney">le montant est signé</param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        private static CashPosition GetCashPosition(Money pMoney,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            string pDateHRef, Nullable<DateTime> pDate)
        {
            CashPosition ret = new CashPosition();
            //
            GetCashPosition(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, ret);
            //
            return ret;
        }
        #endregion GetCashPosition

        /// <summary>
        /// Retourne le payer et le receiver en fonction du signe du montant
        /// <para>si le montant est positif, le payer est pActor1Id et le receiver est pActor2Id </para>
        /// <para>si le montant est négatif, le payer est pActor2Id et le receiver est pActor1Id </para>
        /// <para>si le montant est à zéro, les payer et receiver sont fonction de pIsZeroToPayeByActor1</para>
        /// </summary>
        /// <param name="pMoney"></param>
        /// <param name="pActor1Id"></param>
        /// <param name="pActor2Id"></param>
        /// <param name="pIsZeroToPayeByActor1"></param>
        /// <param name="pPayer"></param>
        /// <param name="pReceiver"></param>
        private static void GetPayerReceiver(Money pMoney, string pActor1Id, string pActor2Id, bool pIsZeroToPayeByActor1,
            out string pPayer, out string pReceiver)
        {
            if (pMoney == null || pMoney.Amount.DecValue == 0)
            {
                // Montant à zéro, le payeur dépond de la nature du montant
                if (pIsZeroToPayeByActor1)
                {
                    pPayer = pActor1Id;
                    pReceiver = pActor2Id;
                }
                else
                {
                    pPayer = pActor2Id;
                    pReceiver = pActor1Id;
                }
            }
            else if (pMoney.Amount.DecValue > 0)
            {
                pPayer = pActor1Id;
                pReceiver = pActor2Id;
            }
            else
            {
                pPayer = pActor2Id;
                pReceiver = pActor1Id;
            }
        }

        /// <summary>
        /// Retourne Debit,Crédit en fonction du signe du montant
        /// <para>Lorsque le montant est négatif alors {pActorId} paye et l'entité reçoit</para>
        /// <para>Lorsque le montant est positif alors {pActorId} reçoit et l'entité paye</para>
        /// </summary>
        /// <param name="pMoney">montant signé</param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <returns></returns>
        private static CrDrEnum GetCrDrEnum(Money pMoney, string pEntityId, string pActorId, bool pIsZeroToPayeByEntity)
        {
            GetPayerReceiver(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, out string payer, out _);
            CrDrEnum ret;
            if (payer == pActorId)
                ret = CrDrEnum.DR;
            else
                ret = CrDrEnum.CR;
            return ret;
        }

        /// <summary>
        /// Retourne un SimplePayment en fonction d'un money dont le montant est signé
        /// </summary>
        /// <param name="pMoney"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <returns></returns>
        private static SimplePayment GetSimplePayment(
            Money pMoney,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            DateTime pDtBusiness, IBusinessCenters pBusinessCenters)
        {
            SimplePayment payment = new SimplePayment();
            //payer, receiver                
            GetPayerReceiver(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, out string payer, out string receiver);
            payment.payerPartyReference.href = payer;
            payment.receiverPartyReference.href = receiver;
            //
            //Money
            payment.paymentAmount = (Money)pMoney.Clone();
            payment.paymentAmount.amount.DecValue = System.Math.Abs(payment.paymentAmount.amount.DecValue);
            //
            payment.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified = true;
            payment.paymentDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate.DateValue = pDtBusiness.AddDays(1);
            payment.paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments = new BusinessDayAdjustments();
            //
            if (pBusinessCenters != null)
            {
                payment.paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersDefineSpecified = ArrFunc.IsFilled(pBusinessCenters.BusinessCenter);
                payment.paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersDefine = (BusinessCenters)pBusinessCenters;
            }
            else
                payment.paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersDefineSpecified = false;
            //    
            payment.paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.FOLLOWING;
            //
            return payment;
        }

        /// <summary>
        /// Retourne un tableau de Money non signés en fonction d'une liste de Money {pMoney} dont le montant est signé
        /// </summary>
        /// <param name="pMoney"></param>
        /// <returns></returns>
        private static Money[] GetMoney(List<Money> pMoney)
        {
            return (from money in pMoney
                    select new Money(System.Math.Abs(money.amount.DecValue), money.Currency)
                    ).ToList().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdC"></param>
        /// <param name="pBusinessCenters"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20140910 [20066][20185] Add pCacheData
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment[] GetFees(string pCS, IDbTransaction pDbTransaction, 
            IProductBase pProductBase, string pIdC,
            CBTradeInfo pTradeInfo, DateTime pDtBusiness, IBusinessCenters pBusinessCenters, CBCache pCacheData)
        {
            string entityPartyId = pTradeInfo.SqlEntity.XmlId;
            string actorPartyId = pTradeInfo.SqlActor.XmlId;

            // PM 20150709 [21103] GetFeesPaymentTypes remplacé par CashFlowOPP_FlowCur.GetPaymentType
            List<string> idCPaymentTypes = pTradeInfo.CashFlowOPP_FlowCur.GetPaymentType(pIdC);
            DetailedContractPayment[] ret;
            if (idCPaymentTypes.Count() == 0)
            {
                // PM 20141218 Déplacé dans une nouvelle méthode
                ////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //// Payment
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////                    
                //ret = new DetailedContractPayment[1];
                //ret[0] = new DetailedContractPayment();
                //ret[0].paymentTypeSpecified = false;
                ////
                ////                
                //string payer, receiver = null;
                //GetPayerReceiver(null, entityPartyId, actorPartyId, pTradeInfo.IsClearer, out payer, out receiver);
                //ret[0].payerPartyReference.href = payer;
                //ret[0].receiverPartyReference.href = receiver;
                ////
                //ret[0].paymentAmount = new Money(0, pIdC);
                //ret[0].paymentDateSpecified = true;
                //ret[0].paymentDate.unadjustedDate.DateValue = pDtBusiness.AddDays(1);
                //ret[0].paymentDate.dateAdjustments.businessCentersDefineSpecified =
                //    ((pBusinessCenters != null) && ArrFunc.IsFilled(pBusinessCenters.businessCenter));
                //ret[0].paymentDate.dateAdjustments.businessCentersDefine = (BusinessCenters)pBusinessCenters;
                //ret[0].paymentDate.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.FOLLOWING;
                ////
                ////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //// Payment - Tax
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////
                //ret[0].taxSpecified = false;
                ret = GetNoFeePayment(entityPartyId, actorPartyId, pTradeInfo.IsClearer, pIdC, pDtBusiness, pBusinessCenters);
            }
            else
            {
                ret = new DetailedContractPayment[idCPaymentTypes.Count()];
                //
                for (int i = 0; i < idCPaymentTypes.Count(); i++)
                {
                    // PM 20141218 Déplacé dans une nouvelle méthode
                    ret[i] = GetDetailedContractPayment(pCS, pDbTransaction, pProductBase, pIdC, false,
                        pTradeInfo, pDtBusiness, pBusinessCenters, idCPaymentTypes[i], pTradeInfo.CashFlowOPP_FlowCur.CashFlows);
                    ////
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //// Payment
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////                    
                    //ret[i] = new DetailedContractPayment();
                    //ret[i].paymentTypeSpecified = true;
                    //ret[i].paymentType.Value = idCPaymentTypes[i];
                    ////
                    //Tools.InitializePaymentSource(ret[i], pProductBase);
                    //Tools.SetPaymentSource(CSTools.SetCacheOn(pCS), ret[i], false);
                    ////
                    //List<Money> listMoney =
                    //    (from moneyByIDC in
                    //         // Cumuler les différents montants en HT
                    //         (from fee in pTradeInfo.CashFlowOPP_FlowCur
                    //          where fee.PaymentType == idCPaymentTypes[i] && fee.IdTaxSpecified == false
                    //          from money in fee.CurrencyAmount
                    //          where money.Currency == pIdC
                    //          select money).GroupBy(money1 => money1.Currency)
                    //     select new Money((from money2 in moneyByIDC select money2.amount.DecValue).Sum(), pIdC)).ToList();
                    ////

                    //Money moneyPayment = GetMoneyIdC(listMoney, pIdC);

                    //string payer, receiver = null;
                    //GetPayerReceiver(moneyPayment, entityPartyId, actorPartyId, pTradeInfo.IsClearer, out payer, out receiver);
                    //ret[i].payerPartyReference.href = payer;
                    //ret[i].receiverPartyReference.href = receiver;

                    //ret[i].paymentAmount = (Money)moneyPayment.Clone();
                    //ret[i].paymentAmount.amount.DecValue = System.Math.Abs(ret[i].paymentAmount.amount.DecValue);

                    ////
                    //ret[i].paymentDateSpecified = true;
                    //ret[i].paymentDate.unadjustedDate.DateValue = pDtBusiness.AddDays(1);
                    //ret[i].paymentDate.dateAdjustments.businessCentersDefineSpecified =
                    //    ((pBusinessCenters != null) && ArrFunc.IsFilled(pBusinessCenters.businessCenter));
                    //ret[i].paymentDate.dateAdjustments.businessCentersDefine = (BusinessCenters)pBusinessCenters;
                    //ret[i].paymentDate.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.FOLLOWING;

                    //Set detail par DC
                    //PM 20140910 [20066][20185] Add pCacheData
                    SetContractPaymentDetail(ret[i], pTradeInfo.CashFlowOPP_FlowCur.CashFlows.ToList(), pIdC, idCPaymentTypes[i],
                        entityPartyId, actorPartyId, pTradeInfo.IsClearer, pCacheData);

                    ////Add montant des taxes
                    //SetFeeTax(pCS, ret[i], pTradeInfo.CashFlowOPP_FlowCur, pIdC, idCPaymentTypes[i], pProductBase);
                }
            }
            return ret;
        }

        /// <summary>
        /// Fournit un tableau de DetailedContractPayment pour les flux Safe Keeping Payment en devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdC"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20150709 [21103] New
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment[] GetSafekeeping(string pCS, IDbTransaction pDbTransaction, IProductBase pProductBase, string pIdC,
            CBTradeInfo pTradeInfo, DateTime pDtBusiness, IBusinessCenters pBusinessCenters, CBCache pCacheData)
        {
            DetailedContractPayment[] ret = null;
            if ((pTradeInfo != null) && (pTradeInfo.CashFlowSKP_FlowCur != null) && (pTradeInfo.CashFlowSKP_FlowCur.CashFlowsByCurrency(pIdC).Count() > 0))
            {
                List<string> idCPaymentTypes = pTradeInfo.CashFlowSKP_FlowCur.GetPaymentType(pIdC);
                //
                string entityPartyId = pTradeInfo.SqlEntity.XmlId;
                string actorPartyId = pTradeInfo.SqlActor.XmlId;
                //
                if (idCPaymentTypes.Count() == 0)
                {
                    ret = GetNoFeePayment(entityPartyId, actorPartyId, pTradeInfo.IsClearer, pIdC, pDtBusiness, pBusinessCenters);
                }
                else
                {
                    ret = new DetailedContractPayment[idCPaymentTypes.Count()];
                    //
                    for (int i = 0; i < idCPaymentTypes.Count(); i++)
                    {
                        ret[i] = GetDetailedContractPayment(pCS, pDbTransaction, pProductBase, pIdC, false, pTradeInfo, pDtBusiness, pBusinessCenters, idCPaymentTypes[i], pTradeInfo.CashFlowSKP_FlowCur.CashFlows);
                        SetContractPaymentDetail(ret[i], pTradeInfo.CashFlowSKP_FlowCur.CashFlows.ToList(), pIdC, idCPaymentTypes[i], 
                            entityPartyId, actorPartyId, pTradeInfo.IsClearer, pCacheData);
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdC"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <returns></returns>
        // PM 20141218 Ajout méthode GetExchangeFees pour la contrevaleur par PaymentType
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment[] GetExchangeFees(string pCS, IDbTransaction pDbTransaction, IProductBase pProductBase, string pIdC,
            CBTradeInfo pTradeInfo, DateTime pDtBusiness, IBusinessCenters pBusinessCenters)
        {
            //DetailedContractPayment[] ret = null;

            IEnumerable<CBDetCashFlows> cashFlowOPP;
            if ((pTradeInfo != null) && (pTradeInfo.CtrValTradeInfo != null) && (pTradeInfo.CtrValTradeInfo.CBFlowsFeesAndTax != null))
            {
                cashFlowOPP = pTradeInfo.CtrValTradeInfo.CBFlowsFeesAndTax.CashFlows;
            }
            else
            {
                cashFlowOPP = new List<CBDetCashFlows>();
            }
            return GetExchangeDetailedContractPayment(pCS, pDbTransaction, pProductBase, pIdC, pTradeInfo, cashFlowOPP, pDtBusiness, pBusinessCenters);
        }

        /// <summary>
        /// Fournit un tableau de DetailedContractPayment pour les flux Safe Keeping Payment en contre valeur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdC"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <returns></returns>
        /// PM 20150709 [21103] New
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment[] GetExchangeSafekeeping(string pCS, IDbTransaction pDbTransaction, IProductBase pProductBase, 
            string pIdC, CBTradeInfo pTradeInfo, DateTime pDtBusiness, IBusinessCenters pBusinessCenters)
        {
            DetailedContractPayment[] detailedContractPayment = null;
            if ((pTradeInfo != null) && (pTradeInfo.CtrValTradeInfo != null) && (pTradeInfo.CtrValTradeInfo.CBFlowsSafeKeepingPayment != null))
            {
                IEnumerable<CBDetCashFlows> cashFlow = pTradeInfo.CtrValTradeInfo.CBFlowsSafeKeepingPayment.CashFlows;
                if ((cashFlow != null) && (cashFlow.Count() > 0))
                {
                    detailedContractPayment = GetExchangeDetailedContractPayment(pCS, pDbTransaction, pProductBase, pIdC, pTradeInfo, cashFlow, 
                        pDtBusiness, pBusinessCenters);
                }
            }
            return detailedContractPayment;
        }

        /// <summary>
        /// Fournit un tableau de DetailedContractPayment pour le détail des flux Safe Keeping Payment en contre valeur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdC"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pCashFlow"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <returns></returns>
        /// PM 20150709 [21103] New
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment[] GetExchangeDetailedContractPayment(string pCS, IDbTransaction pDbTransaction,
            IProductBase pProductBase, string pIdC, CBTradeInfo pTradeInfo, IEnumerable<CBDetCashFlows> pCashFlow, 
            DateTime pDtBusiness, IBusinessCenters pBusinessCenters)
        {
            DetailedContractPayment[] ret = null;
            if ((pTradeInfo != null) && (pCashFlow != null))
            {
                string entityPartyId = pTradeInfo.SqlEntity.XmlId;
                string actorPartyId = pTradeInfo.SqlActor.XmlId;

                List<string> idCPaymentTypes = (
                    from fee in pCashFlow
                    select fee.PaymentType).Distinct().ToList();

                if (idCPaymentTypes.Count() == 0)
                {
                    ret = GetNoFeePayment(entityPartyId, actorPartyId, pTradeInfo.IsClearer, pIdC, pDtBusiness, pBusinessCenters);
                }
                else
                {
                    ret = new DetailedContractPayment[idCPaymentTypes.Count()];
                    for (int i = 0; i < idCPaymentTypes.Count(); i++)
                    {
                        ret[i] = GetDetailedContractPayment(pCS, pDbTransaction, pProductBase, pIdC, true, pTradeInfo, pDtBusiness, pBusinessCenters, idCPaymentTypes[i], pCashFlow);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEntityPartyId"></param>
        /// <param name="pActorPartyId"></param>
        /// <param name="pIsClearer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <returns></returns>
        // PM 20141218 Ajout méthode pour partage du code
        private static DetailedContractPayment[] GetNoFeePayment(string pEntityPartyId, string pActorPartyId, bool pIsClearer,
             string pIdC, DateTime pDtBusiness, IBusinessCenters pBusinessCenters)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Payment
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            GetPayerReceiver(null, pEntityPartyId, pActorPartyId, pIsClearer, out string payer, out string receiver);
            DetailedContractPayment[] ret = new DetailedContractPayment[1]
            {
                new DetailedContractPayment()
                {
                    paymentTypeSpecified = false,
                    paymentAmount = new Money(0, pIdC),
                    paymentDateSpecified = true,
                    payerPartyReference = new PartyOrAccountReference(payer),
                    receiverPartyReference = new PartyOrAccountReference(receiver),
                    paymentDate = new AdjustableDate()
                    {
                        unadjustedDate = new IdentifiedDate()
                        {
                            DateValue = pDtBusiness.AddDays(1),
                        },
                        dateAdjustments = new BusinessDayAdjustments()
                        {
                            businessCentersDefineSpecified = pBusinessCenters != null && ArrFunc.IsFilled(pBusinessCenters.BusinessCenter),
                            businessCentersDefine = (BusinessCenters)pBusinessCenters,
                            businessDayConvention = BusinessDayConventionEnum.FOLLOWING
                        }
                    },
                    // Payment - Tax
                    taxSpecified = false
                }
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <param name="pIdCPaymentTypes"></param>
        /// <param name="pCashFlow"></param>
        /// <returns></returns>
        /// PM 20141218 Ajout méthode pour partage du code
        /// PM 20150709 [21103] Renommage GetFeeTaxPayment en GetDetailedContractPayment
        // EG 20180205 [23769] Add dbTransaction  
        private static DetailedContractPayment GetDetailedContractPayment(string pCS, IDbTransaction pDbTransaction, IProductBase pProductBase, 
            string pIdC, bool pIsCtrVal, CBTradeInfo pTradeInfo, DateTime pDtBusiness, IBusinessCenters pBusinessCenters,
            string pIdCPaymentTypes, IEnumerable<CBDetCashFlows> pCashFlow)
        {
            DetailedContractPayment ret = new DetailedContractPayment
            {
                paymentTypeSpecified = true,
                paymentType = new PaymentType()
                {
                    Value = pIdCPaymentTypes
                }
            };

            Tools.InitializePaymentSource(ret, pProductBase);
            Tools.SetPaymentSource(CSTools.SetCacheOn(pCS), pDbTransaction, ret, false);

            List<Money> listMoney;
            if (pIsCtrVal)
            {
                listMoney =
                    (from moneyByIDC in
                         // Cumuler les différents montants en HT
                         (from fee in pCashFlow
                          where fee.PaymentType == pIdCPaymentTypes && fee.IdTaxSpecified == false
                          from money in fee.CtrValAmount
                          where money.Currency == pIdC
                          select money).GroupBy(money1 => money1.Currency)
                     select new Money((from money2 in moneyByIDC select money2.amount.DecValue).Sum(), pIdC)).ToList();
            }
            else
            {
                listMoney =
                    (from moneyByIDC in
                         // Cumuler les différents montants en HT
                         (from fee in pCashFlow
                          where fee.PaymentType == pIdCPaymentTypes && fee.IdTaxSpecified == false
                          from money in fee.CurrencyAmount
                          where money.Currency == pIdC
                          select money).GroupBy(money1 => money1.Currency)
                     select new Money((from money2 in moneyByIDC select money2.amount.DecValue).Sum(), pIdC)).ToList();
            }
            //
            Money moneyPayment = GetMoneyIdC(listMoney, pIdC);

            string entityPartyId = pTradeInfo.SqlEntity.XmlId;
            string actorPartyId = pTradeInfo.SqlActor.XmlId;
            GetPayerReceiver(moneyPayment, entityPartyId, actorPartyId, pTradeInfo.IsClearer, out string payer, out string receiver);
            ret.payerPartyReference.href = payer;
            ret.receiverPartyReference.href = receiver;

            ret.paymentAmount = (Money)moneyPayment.Clone();
            ret.paymentAmount.amount.DecValue = System.Math.Abs(ret.paymentAmount.amount.DecValue);

            //
            ret.paymentDateSpecified = true;
            ret.paymentDate.unadjustedDate.DateValue = pDtBusiness.AddDays(1);
            ret.paymentDate.dateAdjustments.businessCentersDefineSpecified =
                ((pBusinessCenters != null) && ArrFunc.IsFilled(pBusinessCenters.BusinessCenter));
            ret.paymentDate.dateAdjustments.businessCentersDefine = (BusinessCenters)pBusinessCenters;
            ret.paymentDate.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.FOLLOWING;

            //Add montant des taxes
            SetFeeTax(pCS, pDbTransaction, ret, pCashFlow, pIdC, pIsCtrVal, pIdCPaymentTypes, pProductBase);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeInfo"></param>
        /// <returns></returns>
        ///PM 20140919 [20066][20185] Ajout gestion méthode UK
        private static IdentifiedFxRate[] GetFxRate(CBTradeInfo pTradeInfo)
        {
            IdentifiedFxRate[] fxRates = null;
            //PM 20140919 [20066][20185] Ajout gestion méthode UK
            //if (pTradeInfo.IsCBOWithMGCCTRVal)
            //{
            //List<CBQuote> quotesAll =
            //    (from quote in
            //         ((from deposit in pTradeInfo.FlowDeposit
            //           from exAmount in deposit.Deposit_ExCTRValCur
            //           from quote in exAmount.Quote
            //           select quote)
            //         .Union(from exAmount in pTradeInfo.CashAvailable_ExCTRValCur
            //                from quote in exAmount.Quote
            //                select quote)
            //         .Union(from collateral in pTradeInfo.FlowCollateral
            //                where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
            //                from exAmount in collateral.Amount_MGCCTRVal.AvailableGross_Ex
            //                from quote in exAmount.Quote
            //                select quote))
            //     select quote).Distinct(new CBQuoteComparer()).ToList();
            //    //
            //    List<EfsML.v30.AssetDef.IdentifiedFxRate> identifiedFxRates = new List<EfsML.v30.AssetDef.IdentifiedFxRate>();
            //    //
            //    EfsML.v30.AssetDef.IdentifiedFxRate identifiedFxRate = null;
            //    //
            //    foreach (CBQuote quote in quotesAll)
            //    {
            //        identifiedFxRate = new EfsML.v30.AssetDef.IdentifiedFxRate();
            //        identifiedFxRate.quotedCurrencyPair = quote.Rate.quotedCurrencyPair;
            //        identifiedFxRate.rate = quote.Rate.rate;
            //        identifiedFxRate.efs_id = new EFS_Id();
            //        identifiedFxRate.efs_id.Value =
            //            GetFxRateId(quote.Rate.quotedCurrencyPair);
            //        //
            //        identifiedFxRate.OTCmlId = quote.IdQuote;
            //        //
            //        identifiedFxRates.Add(identifiedFxRate);
            //    }
            //    // 
            //    fxRates = (EfsML.v30.AssetDef.IdentifiedFxRate[])identifiedFxRates.ToArray();
            //}
            //else if (pTradeInfo.IsCBOWithMGCCollatCTRVal)
            //{
            //    List<CBQuote> quotesAll =
            //        (from quote in
            //             ((from collateral in pTradeInfo.FlowCollateral
            //                where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
            //                from exAmount in collateral.Amount_MGCCollatCTRVal.AvailableGross_Ex
            //                from quote in exAmount.Quote
            //                select quote))
            //         select quote).Distinct(new CBQuoteComparer()).ToList();
            //    //
            //    List<EfsML.v30.AssetDef.IdentifiedFxRate> identifiedFxRates = new List<EfsML.v30.AssetDef.IdentifiedFxRate>();
            //    //
            //    EfsML.v30.AssetDef.IdentifiedFxRate identifiedFxRate = null;
            //    //
            //    foreach (CBQuote quote in quotesAll)
            //    {
            //        identifiedFxRate = new EfsML.v30.AssetDef.IdentifiedFxRate();
            //        identifiedFxRate.quotedCurrencyPair = quote.Rate.quotedCurrencyPair;
            //        identifiedFxRate.rate = quote.Rate.rate;
            //        identifiedFxRate.efs_id = new EFS_Id();
            //        identifiedFxRate.efs_id.Value =
            //            GetFxRateId(quote.Rate.quotedCurrencyPair);
            //        //
            //        identifiedFxRate.OTCmlId = quote.IdQuote;
            //        //
            //        identifiedFxRates.Add(identifiedFxRate);
            //    }
            //    // 
            //    fxRates = (EfsML.v30.AssetDef.IdentifiedFxRate[])identifiedFxRates.ToArray();
            //}
            if (pTradeInfo.IsCBOWithMGCCTRVal
                || pTradeInfo.IsCBOWithMGCCollatCTRVal
                || (pTradeInfo.CbCalcMethod == CashBalanceCalculationMethodEnum.CSBUK))
            {
                List<CBQuote> quotesAll;
                if (pTradeInfo.CbCalcMethod == CashBalanceCalculationMethodEnum.CSBUK)
                {
                    quotesAll = pTradeInfo.CtrValTradeInfo.Quote;
                }
                else if (pTradeInfo.IsCBOWithMGCCTRVal)
                {
                    quotesAll =
                        (
                        from quote in
                            (from deposit in pTradeInfo.FlowDeposit
                             from exAmount in deposit.Deposit_ExCTRValCur
                             from quote in exAmount.Quote
                             select quote
                            ).Concat(from exAmount in pTradeInfo.CashAvailable_ExCTRValCur
                                     where (exAmount.Quote != null)
                                     from quote in exAmount.Quote
                                     select quote
                            ).Concat(from collateral in pTradeInfo.FlowCollateral
                                     where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                                     && (collateral.Amount_MGCCTRVal.AvailableGross_Ex != null)
                                     from exAmount in collateral.Amount_MGCCTRVal.AvailableGross_Ex
                                     from quote in exAmount.Quote
                                     select quote)
                        select quote
                        ).Distinct(new CBQuoteComparer()).ToList();
                }
                else if (pTradeInfo.IsCBOWithMGCCollatCTRVal)
                {
                    quotesAll =
                        (
                        from collateral in pTradeInfo.FlowCollateral
                        where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                        && (collateral.Amount_MGCCollatCTRVal.AvailableGross_Ex != null)
                        from exAmount in collateral.Amount_MGCCollatCTRVal.AvailableGross_Ex
                        from quote in exAmount.Quote
                        select quote
                        ).Distinct(new CBQuoteComparer()).ToList();
                }
                else
                {
                    quotesAll = new List<CBQuote>();
                }

                List<IdentifiedFxRate> identifiedFxRates = new List<IdentifiedFxRate>();
                IdentifiedFxRate identifiedFxRate = null;

                foreach (CBQuote quote in quotesAll)
                {
                    identifiedFxRate = new IdentifiedFxRate
                    {
                        quotedCurrencyPair = quote.Rate.quotedCurrencyPair,
                        rate = quote.Rate.rate,
                        efs_id = new EFS_Id(GetFxRateId(quote.Rate.quotedCurrencyPair)),
                        OTCmlId = quote.IdQuote
                    };
                    identifiedFxRates.Add(identifiedFxRate);
                }
                fxRates = identifiedFxRates.ToArray();
            }
            return fxRates;
        }

        /// <summary>
        /// Alimente le DataDocument avec:
        /// <para>- les différents parties ({pSqlActor}, {pSqlBook},{pSqlEntity})</para>
        /// <para>- la date de compensation m_CBHierarchy.DtBusiness</para>
        /// <para>- les différents montants dans  {pTradeInfo}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// FI 20120725 [18009] modifications diverses
        /// PM 20140901 [20066][20185] Gestion méthode UK
        /// PM 20140909 [20066][20185] Add pCacheData
        /// FI 20161027 [22151] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public static void SetDataDocument(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDoc, CBTradeInfo pTradeInfo, CBCache pCacheData)
        {
            // PM 20171117 [23509] Ajout dtOrderEntered à l'image de ce qui est fait pour les trades margin requirement
            // On recupère ORDERENTERED éventuel du trade si déjà créé
            Nullable<DateTimeOffset> dtOrderEntered = pDataDoc.GetOrderEnteredDateTimeOffset();

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Ajout des parties
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            //Suppression des parties existantes
            pDataDoc.RemoveParty();
            //
            IParty partyActor = pDataDoc.AddParty(pTradeInfo.SqlActor);
            IPartyTradeIdentifier partyActorTradeIdentifier = pDataDoc.AddPartyTradeIndentifier(partyActor.Id);
            Tools.SetBookId(partyActorTradeIdentifier.BookId, pTradeInfo.SqlBook);
            partyActorTradeIdentifier.BookIdSpecified = true;
            //                    
            IParty partyEntity = pDataDoc.AddParty(pTradeInfo.SqlEntity);
            //
            // PM 20171117 [23509] Ajout gestion partyTradeInformationEntity à l'image de ce qui est fait pour les trades margin requirement
            if (null != partyEntity)
            {
                IPartyTradeInformation partyTradeInformationEntity = pDataDoc.AddPartyTradeInformation(partyEntity.Id);
                if (null != partyTradeInformationEntity)
                {
                    partyTradeInformationEntity.Timestamps = pDataDoc.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                    if (false == dtOrderEntered.HasValue)
                    {
                        DateTime dtSys = OTCmlHelper.GetDateSys(pTradeInfo.SqlActor.CS);
                        dtOrderEntered = Tz.Tools.FromTimeZone(dtSys, Tz.Tools.UniversalTimeZone);
                    }
                    partyTradeInformationEntity.Timestamps.OrderEntered = Tz.Tools.ToString(dtOrderEntered);
                    partyTradeInformationEntity.Timestamps.OrderEnteredSpecified = true;
                    partyTradeInformationEntity.TimestampsSpecified = true;
                }
            }
            // FI 20161027 [22151] Add CSSCUSTODIAN Party existing pTradeInfo.endOfDayStatus
            if (null != pTradeInfo.endOfDayStatus)
            {
                IEnumerable<int> idACssCustodian = from item in pTradeInfo.endOfDayStatus.cssCustodianStatus
                                                   select item.idACssCustodian;
                foreach (int item in idACssCustodian)
                {
                    SQL_Actor sqlactor = new SQL_Actor(CSTools.SetCacheOn(pCS), item)
                    {
                        DbTransaction = pDbTransaction
                    };
                    sqlactor.LoadTable();
                    pDataDoc.AddParty(sqlactor);
                }
            }


            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Trade Date / Cleared Date / TradeDateTime
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            ITradeHeader tradeHeader = pDataDoc.TradeHeader;
            tradeHeader.TradeDate.TimeStampHHMMSS = null;
            // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness
            pDataDoc.SetClearedDate(pTradeInfo.DtBusiness);
            tradeHeader.ClearedDate.Id = "DtBusiness";
            tradeHeader.TradeDate.Value = pDataDoc.TradeHeader.ClearedDate.Value;
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            EfsML.v30.CashBalance.CashBalance cashBalance = (EfsML.v30.CashBalance.CashBalance)pDataDoc.CurrentProduct.Product;
            cashBalance.cashBalanceOfficePartyReference = new PartyReference(pTradeInfo.SqlActor.XmlId);
            cashBalance.entityPartyReference = new PartyReference(pTradeInfo.SqlEntity.XmlId);
            cashBalance.timing = pTradeInfo.Timing;
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Fx rate
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            cashBalance.fxRate = GetFxRate(pTradeInfo);
            cashBalance.fxRateSpecified = ArrFunc.IsFilled(cashBalance.fxRate);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Exchange cash balance stream
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            SetExchangeStream(pCS, pDbTransaction, pDataDoc, pTradeInfo, pCacheData);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            SetStream(pCS, pDbTransaction, pDataDoc, pTradeInfo, pCacheData);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Settings
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            cashBalance.settings = new CashBalanceSettings
            {
                cashBalanceOfficePartyReference = new PartyReference(partyActor.Id),
                scope = pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.Scope_CBO,
                exchangeCurrency = new Currency(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.ExchangeIDC),
                exchangeCurrencySpecified = StrFunc.IsFilled(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.ExchangeIDC),
                useAvailableCash = new EFS_Boolean(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.IsUseAvailableCash),
                cashAndCollateral = pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.CashAndCollateral,
                managementBalance = new EFS_Boolean(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.IsManagementBalance),
                marginCallCalculationMethod = pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.MgcCalcMethod,
                cashBalanceMethod = pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.CbCalcMethod,
                cashBalanceMethodSpecified = true
            };
            if (StrFunc.IsFilled(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.CbIDC))
            {
                cashBalance.settings.cashBalanceCurrency = new Currency(pTradeInfo.PartyTradeInfo.ActorCBO.BusinessAttribute.CbIDC);
                cashBalance.settings.cashBalanceCurrencySpecified = true;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - EndOFDay
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // FI 20161027 [22151] 
            cashBalance.endOfDayStatusSpecified = (null != pTradeInfo.endOfDayStatus);
            if (cashBalance.endOfDayStatusSpecified)
                cashBalance.endOfDayStatus = pTradeInfo.endOfDayStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pCashBalanceStream"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDeposit_FlowCur">contient le cumul des deposits par devise</param>
        /// <param name="pDeposit_ExCTRValCur"></param>
        // EG 20180205 [23769] Add dbTransaction  
        private static void SetMarginRequirement(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDoc, CashBalanceStream pCashBalanceStream,
            CBTradeInfo pTradeInfo, List<Money> pDeposit_FlowCur, List<CBExAmount> pDeposit_ExCTRValCur)
        {
            string entityPartyId = pTradeInfo.SqlEntity.XmlId;
            string actorPartyId = pTradeInfo.SqlActor.XmlId;
            string idC = pCashBalanceStream.currency.Value;
            //
            Money depositFcu = GetMoneyIdC(pDeposit_FlowCur, idC);

            CBExAmount depositExChange = null;
            if (null != pDeposit_ExCTRValCur)
                depositExChange = pDeposit_ExCTRValCur.Find(match => match.FlowCurrency == idC);
            //
            // Montant global (somme des deposits par devise idC)
            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id 
            //ExchangeCashPosition exchCashPos =
            //        GetExchangeCashPosition(depositFcu, depositExChange, entityPartyId, actorPartyId, pTradeInfo.IsClearer,
            //             pDataDoc.tradeHeader.tradeDate.efs_id, pTradeInfo.IsCBOWithMGCCTRVal);
            ExchangeCashPosition exchCashPos =
                    GetExchangeCashPosition(depositFcu, depositExChange, entityPartyId, actorPartyId, pTradeInfo.IsClearer,
                         pDataDoc.TradeHeader.ClearedDate.Id, pTradeInfo.IsCBOWithMGCCTRVal);

            pCashBalanceStream.marginRequirement = new CssExchangeCashPosition(exchCashPos);

            #region detail
            // Montant global (somme des deposit par couple (devise,chambre))
            List<CssAmount> lstCssAmountDetail = new List<CssAmount>();
            IEnumerable<int> lstIdAcss = (from flowdeposit in pTradeInfo.FlowDeposit select flowdeposit.Ida_Css).Distinct();
            foreach (int idACss in lstIdAcss)
            {
                decimal marginRequirement =
                     (from deposit in pTradeInfo.FlowDeposit
                      where deposit.Ida_Css == idACss
                      from money in deposit.CurrencyAmount
                      where money.Currency == idC
                      select money.amount.DecValue).Sum();

                if (marginRequirement != 0)
                {
                    Money marginRequirementMoney = new Money(marginRequirement, idC);
                    CssAmount detail = new CssAmount();

                    //PM 20150402 [POC] Ne pas alimenter le CSS lorsqu'il n'existe pas
                    if (idACss > 0)
                    {

                        SQL_Actor sqlActorCss = new SQL_Actor(CSTools.SetCacheOn(pCS), idACss)
                        {
                            DbTransaction = pDbTransaction
                        };
                        sqlActorCss.LoadTable();
                        pDataDoc.AddParty(sqlActorCss);

                        detail.cssHref = sqlActorCss.XmlId;
                        detail.cssHrefSpecified = true;
                    }
                    detail.Amt = System.Math.Abs(marginRequirementMoney.amount.DecValue);
                    detail.AmtSide = GetCrDrEnum(marginRequirementMoney, entityPartyId, actorPartyId, pTradeInfo.IsClearer);
                    // FI 20141208 [XXXXX] alimentation de AmtSideSpecified
                    detail.AmtSideSpecified = (detail.AmtSide > 0);

                    lstCssAmountDetail.Add(detail);
                }
            }

            pCashBalanceStream.marginRequirement.detailSpecified = ArrFunc.IsFilled(lstCssAmountDetail);
            if (pCashBalanceStream.marginRequirement.detailSpecified)
                pCashBalanceStream.marginRequirement.detail = lstCssAmountDetail.ToArray();
            #endregion
        }

        /// <summary>
        /// Retourne un ContractSimplePayment en fonction d'un money dont le montant est signé
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMoney"></param>
        /// <param name="pCBDetCashFlows">les détail cashFlows toutes devises et Derivative Contract</param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        ///PM 20140910 [20066][20185] Add pCacheData
        // EG 20180205 [23769] Del pCS
        private static ContractSimplePayment GetContractSimplePayment(Money pMoney, List<CBDetCashFlows> pCBDetCashFlows,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            DateTime pDtBusiness, IBusinessCenters pBusinessCenters, CBCache pCacheData)
        {
            //Montant cumulé sur la devise pIdC
            string idC = pMoney.Currency;

            SimplePayment payment = GetSimplePayment(pMoney, pEntityId, pActorId, pIsZeroToPayeByEntity, pDtBusiness, pBusinessCenters);
            ContractSimplePayment ret = new ContractSimplePayment(payment);

            #region détail des montants
            ret.detail = GetContractAmounts(pCBDetCashFlows, idC, false, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
            ret.detailSpecified = ((ret.detail != null) && (ret.detail.Count() > 0));
            #endregion
            //
            return ret;
        }

        /// <summary>
        ///  alimente {pCashBalanceStream}.previousMarginConstituent
        /// </summary>
        /// <param name="pCashBalanceStream"></param>
        /// <param name="pTradeInfo"></param>
        private static void SetPreviousMarginConstituent(CashBalanceStream pCashBalanceStream, CBTradeInfo pTradeInfo)
        {
            string idC = pCashBalanceStream.currency.Value;
            string entityPartyId = pTradeInfo.SqlEntity.XmlId;
            string actorPartyId = pTradeInfo.SqlActor.XmlId;

            pCashBalanceStream.previousMarginConstituent = new PreviousMarginConstituent();
            pCashBalanceStream.previousMarginConstituentSpecified = true;
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Margin requirement
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //                
            Money prevDepositIdC = GetMoneyIdC(pTradeInfo.PrevDeposit_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.marginRequirement =
                GetCashPosition(prevDepositIdC, entityPartyId, actorPartyId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Cash available
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //                
            Money prevCashAvailableIdC = GetMoneyIdC(pTradeInfo.PrevCashAvailable_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.cashAvailable =
                GetCashPosition(prevCashAvailableIdC, entityPartyId, actorPartyId, pTradeInfo.IsNotClearer, string.Empty, pTradeInfo.DtBusinessPrev);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Cash used
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //     
            Money prevCashUsedIdC = GetMoneyIdC(pTradeInfo.PrevCashUsed_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.cashUsed =
                GetCashPosition(prevCashUsedIdC, entityPartyId, actorPartyId, pTradeInfo.IsNotClearer, string.Empty, pTradeInfo.DtBusinessPrev);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Collateral available
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //                
            Money prevCollatAvailableIDC = GetMoneyIdC(pTradeInfo.PrevCollatAvailable_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.collateralAvailable =
                GetCashPosition(prevCollatAvailableIDC, entityPartyId, actorPartyId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Collateral used
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //                
            Money prevCollatUsedIdC = GetMoneyIdC(pTradeInfo.PrevCollatUsed_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.collateralUsed =
                GetCashPosition(prevCollatUsedIdC, entityPartyId, actorPartyId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance - Cash balance stream - Previous Margin requirement constituent - Uncovered Margin requirement
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //                
            Money prevPrevDefectDepositIdC = GetMoneyIdC(pTradeInfo.PrevDefectDeposit_FlowCur, idC);
            pCashBalanceStream.previousMarginConstituent.uncoveredMarginRequirement =
                GetCashPosition(prevPrevDefectDepositIdC, entityPartyId, actorPartyId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);

        }

        /// <summary>
        /// Retourne le 1er money dans la liste tel que la devise vaut {pIdC}
        /// <para>s'il n'existe aucun money dans la devise retourne un new Money avec montant à zéro</para>
        /// </summary>
        /// <param name="pLstMoney"></param>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        ///PM 20140910 [20066][20185] Vérifier que la liste n'est pas null 
        private static Money GetMoneyIdC(List<Money> pLstMoney, string pIdC)
        {
            Money ret = null;
            //Money moneyIdC = pLstMoney.Find(money => money.Currency == pIdC);
            //if (null != moneyIdC)
            //{
            //    ret = (Money)moneyIdC.Clone();
            //}
            //else
            //{
            //    ret = new Money(0, pIdC);
            //}
            if (null != pLstMoney)
            {
                Money moneyIdC = pLstMoney.Find(money => money.Currency == pIdC);
                if (null != moneyIdC)
                {
                    ret = (Money)moneyIdC.Clone();
                }
            }
            if (null == ret)
            {
                ret = new Money(0, pIdC);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pCacheData"></param>
        /// PM 20140909 [20066][20185] Add pCacheData
        // EG 20180205 [23769] Add dbTransaction  
        private static void SetStream(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDoc, CBTradeInfo pTradeInfo, CBCache pCacheData)
        {
            EfsML.v30.CashBalance.CashBalance cashBalance = (EfsML.v30.CashBalance.CashBalance)pDataDoc.CurrentProduct.Product;
            IProductBase productBase = (IProductBase)cashBalance;
            ITradeHeader tradeHeader = pDataDoc.TradeHeader;

            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
            string businessDateId = tradeHeader.ClearedDate.Id;

            string entityId = pTradeInfo.SqlEntity.XmlId;
            string actorId = pTradeInfo.SqlActor.XmlId;

            //FI 20120725 [18009] appel à CalcCashFlows
            List<Money> cashFlows_FlowCur = pTradeInfo.CalcCashFlows();

            // RD 20170524 [23180] Mémoriser la devise du premier cashBalanceStream si elle exite
            string oldFirstCurrency = string.Empty;
            if (cashBalance != null && ArrFunc.IsFilled(cashBalance.cashBalanceStream) && cashBalance.cashBalanceStream[0].currency != null)
                oldFirstCurrency = cashBalance.cashBalanceStream[0].currency.Value;

            cashBalance.cashBalanceStream = new CashBalanceStream[pTradeInfo.CurrencyPriority.Count];

            List<Money> collateralAvailable_FlowCur = null;
            List<Money> collateralAvailable_MGCCollatCTRVal = null;
            List<CBCollateralConstituent> collateralAvailableConstituent_MGCCollatCTRVal = null;
            List<Money> collateralUsed_Method = null;

            #region Couverture
            // Montants utilisés pour toutes les méthodes
            collateralAvailable_FlowCur =
                (from moneyByIDC in
                     (from collateral in pTradeInfo.FlowCollateral
                      where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                      && collateral.IsAllowed_AtLeastOneCSS
                      from money in collateral.CollatAvailable_FlowCur
                      select money).GroupBy(money => money.Currency)
                 select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();


            if (pTradeInfo.IsCBOWithMGCCollatCur)
            {
                #region Déposit et couverture en devise
                collateralUsed_Method =
                    (from moneyByIDC in
                         (from collateral in pTradeInfo.FlowCollateral
                          where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                          && collateral.IsAllowed_AtLeastOneCSS
                          from money in collateral.CollatUsed_FlowCur
                          select money).GroupBy(money => money.Currency)
                     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();
                #endregion
            }
            else if (pTradeInfo.IsCBOWithMGCCollatCTRVal)
            {
                #region Déposit en devise et couverture en contrevaleur
                collateralAvailable_MGCCollatCTRVal =
                    (from moneyByIDC in
                         (from collateral in pTradeInfo.FlowCollateral
                          where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                          && collateral.IsAllowed_AtLeastOneCSS
                          from money in collateral.Amount_MGCCollatCTRVal.Available
                          select money).GroupBy(money => money.Currency)
                     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

                collateralUsed_Method =
                    (from moneyByIDC in
                         (from collateral in pTradeInfo.FlowCollateral
                          where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                          && collateral.IsAllowed_AtLeastOneCSS
                          from money in collateral.Amount_MGCCollatCTRVal.Used
                          select money).GroupBy(money => money.Currency)
                     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

                collateralAvailableConstituent_MGCCollatCTRVal =
                    (from constituentByIDC in
                         (from collateral in pTradeInfo.FlowCollateral
                          where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                          && collateral.IsAllowed_AtLeastOneCSS
                          from constituent in collateral.Amount_MGCCollatCTRVal.AvailableConstituent
                          select constituent).GroupBy(constituent => constituent.Currency)
                     select new CBCollateralConstituent(constituentByIDC.Key,
                         (from moneyByIDC in
                              (from constituent in constituentByIDC
                               from money in constituent.AlreadyUsed
                               select money).GroupBy(constituent => constituent.Currency)
                          select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList())
                      ).ToList();
                #endregion
            }
            else if (pTradeInfo.IsCBOWithMGCCTRVal)
            {
                #region Déposit et couverture en contravaleur
                // TODO
                #endregion
            }

            List<CBExAmount> collateralAvailable_ExCTRValCur = null;
            if (pTradeInfo.IsCBOWithMGCCTRVal)
            {
                collateralAvailable_ExCTRValCur =
                    (from cbExAmountByIDC in
                         (from collateral in pTradeInfo.FlowCollateral
                          where collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash
                          && collateral.IsAllowed_AtLeastOneCSS
                          from cbExAmount in collateral.Amount_MGCCTRVal.AvailableGross_Ex
                          select cbExAmount).GroupBy(cbExAmount => cbExAmount.FlowCurrency)
                     select new CBExAmount(cbExAmountByIDC.Key,
                     new Money((from cbExAmount in cbExAmountByIDC select cbExAmount.CurrencyAmount.amount.DecValue).Sum(),
                     (from cbExAmount in cbExAmountByIDC select cbExAmount.CurrencyAmount.Currency).First()),
                     (from cbExAmount in cbExAmountByIDC
                      from quote in cbExAmount.Quote
                      select quote).ToList())).ToList();
            }
            #endregion

            List<Money> deposit_FlowCur = pTradeInfo.CalcDeposit_FlowCur();
            List<CBExAmount> deposit_ExCTRValCur = pTradeInfo.CalcDeposit_ExCTRValCur();

            for (int i = 0; i < pTradeInfo.CurrencyPriority.Count; i++)
            {
                string idC = pTradeInfo.CurrencyPriority[i].Currency;

                // RD 20170524 [23180] Utiliser la devise du premier cashBalanceStream si elle exite
                if (StrFunc.IsEmpty(idC))
                    idC = oldFirstCurrency;

                IBusinessCenters currencyBCS = productBase.LoadBusinessCenters(CSTools.SetCacheOn(pCS), pDbTransaction,
                    null, new string[] { idC }, null);
                //
                cashBalance.cashBalanceStream[i] = new CashBalanceStream();
                CashBalanceStream cashBalanceStream = cashBalance.cashBalanceStream[i];
                cashBalanceStream.currency = new Currency(idC);
                //
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Previous Margin requirement constituent
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // [TODO RD: A revoir previousMarginConstituent pour cashBalanceStream]
                SetPreviousMarginConstituent(cashBalanceStream, pTradeInfo);
                //
                #region Margin requirement
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Margin requirement
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                SetMarginRequirement(pCS, pDbTransaction, pDataDoc, cashBalanceStream, pTradeInfo, deposit_FlowCur, deposit_ExCTRValCur);
                //
                #endregion Margin requirement
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                CashAvailable cashAvailable = new CashAvailable();
                //
                Money cashAvailableIdc = GetMoneyIdC(pTradeInfo.CashAvailable_FlowCur, idC);
                CBExAmount cashAvailableExchange = null;
                if (null != pTradeInfo.CashAvailable_ExCTRValCur)
                    cashAvailableExchange = pTradeInfo.CashAvailable_ExCTRValCur.Find(match => match.FlowCurrency == idC);

                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                //ExchangeCashPosition exchangecashPosition = GetExchangeCashPosition(cashAvailableIdc, cashAvailableExchange,
                //        entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, pTradeInfo.IsCBOWithMGCCTRVal);
                ExchangeCashPosition exchangecashPosition = GetExchangeCashPosition(cashAvailableIdc, cashAvailableExchange,
                        entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, pTradeInfo.IsCBOWithMGCCTRVal);
                //
                cashBalanceStream.cashAvailable = new CashAvailable(exchangecashPosition)
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Cash available - constituent
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    constituentSpecified = true,
                    constituent = new CashAvailableConstituent()
                };
                #region Cash balance payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash balance payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                //PM 20140912 [20066][20185] Gestion méthode UK (ETD & CFD)
                //Money cashBalancePayment = GetMoneyIdC(pTradeInfo.Payment_FlowCur, idC);
                //cashBalanceStream.cashAvailable.constituent.cashBalancePayment = new CashBalancePayment();
                //GetCashPosition(cashBalancePayment, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)cashBalanceStream.cashAvailable.constituent.cashBalancePayment);
                switch (pTradeInfo.CbCalcMethod)
                {
                    case CashBalanceCalculationMethodEnum.CSBUK:
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //cashBalanceStream.cashAvailable.constituent.cashBalancePayment = GetCashBalancePayment(pTradeInfo.PaymentStl_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                        cashBalanceStream.cashAvailable.constituent.cashBalancePayment = GetCashBalancePayment(pTradeInfo.PaymentStl_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                        break;
                    case CashBalanceCalculationMethodEnum.CSBDEFAULT:
                    default:
                        Money cashBalancePayment = GetMoneyIdC(pTradeInfo.Payment_FlowCur, idC);
                        cashBalanceStream.cashAvailable.constituent.cashBalancePayment = new CashBalancePayment();
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //GetCashPosition(cashBalancePayment, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)cashBalanceStream.cashAvailable.constituent.cashBalancePayment);
                        GetCashPosition(cashBalancePayment, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, (CashPosition)cashBalanceStream.cashAvailable.constituent.cashBalancePayment);
                        cashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashDepositSpecified = false;
                        cashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashWithdrawalSpecified = false;
                        break;
                }
                #endregion Cash balance payment
                #region Previous cash balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Previous cash balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //                
                Money previousCashBalance = GetMoneyIdC(pTradeInfo.PrevCashBalance_FlowCur, idC);
                cashBalanceStream.cashAvailable.constituent.previousCashBalance =
                    GetCashPosition(previousCashBalance, entityId, actorId, pTradeInfo.IsNotClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                //
                #endregion Previous cash balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //        
                Money cashFlows = GetMoneyIdC(cashFlows_FlowCur, idC);
                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                //CashPosition cashPosition = GetCashPosition(cashFlows, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                CashPosition cashPosition = GetCashPosition(cashFlows, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                //
                cashBalanceStream.cashAvailable.constituent.cashFlows = new CashFlows(cashPosition)
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    constituent = new CashFlowsConstituent()
                };

                #region Variation Margin
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent - Variation margin
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  
                Money variationMargin = pTradeInfo.CashFlowVMG_FlowCur.CalcSumMoneyIdC(idC);
                //PM 20140910 [20066][20185] Add pCacheData
                cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.variationMargin =
                    GetContractSimplePayment(variationMargin, pTradeInfo.CashFlowVMG_FlowCur.CashFlows.ToList(),
                        entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                //
                #endregion Variation Margin
                #region Premium
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent - Premium
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  
                Money premium = pTradeInfo.CashFlowPRM_FlowCur.CalcSumMoneyIdC(idC);
                //PM 20140910 [20066][20185] Add pCacheData
                cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.premium =
                    GetContractSimplePayment(premium, pTradeInfo.CashFlowPRM_FlowCur.CashFlows.ToList(),
                        entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                //
                #endregion Premium
                #region Cash Settlement
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent - Cash Settlement
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.cashSettlement =
                    GetContractSimplePaymentConstituent(pTradeInfo.CashFlowSCU_FlowCur, idC, entityId, actorId, pTradeInfo.IsClearer,
                    pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                //
                #endregion Cash Settlement
                #region Fees
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent - Fees
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                //PM 20140910 [20066][20185] Add pCacheData
                cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.fee =
                GetFees(pCS, pDbTransaction, productBase, idC, pTradeInfo, pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                //
                #endregion Fees
                #region Safekeeping
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - Constituent - Cash flows - Constituent - Safekeeping
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                // PM 20150709 [21103] Add safekeeping
                DetailedContractPayment[] safekeeping = GetSafekeeping(pCS, pDbTransaction, productBase, idC, pTradeInfo, pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                if ((safekeeping != null) && (safekeeping.Count() > 0))
                {
                    cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.safekeeping = safekeeping;
                    cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.safekeepingSpecified = true;
                }
                //
                #endregion Safekeeping
                #region Equalisation Payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash available - constituent - Cash flows - constituent - Equalisation Payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                // PM 20170911 [23408] Add Equalisation Payment
                cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.equalisationPaymentSpecified = ((pTradeInfo.CashFlowEQP_FlowCur != default(CBFlows)) && (pTradeInfo.CashFlowEQP_FlowCur.Flows.Count() > 0));
                if (cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.equalisationPaymentSpecified)
                {
                    Money equalisationPayment = pTradeInfo.CashFlowEQP_FlowCur.CalcSumMoneyIdC(idC);
                    cashBalanceStream.cashAvailable.constituent.cashFlows.constituent.equalisationPayment =
                        GetContractSimplePayment(equalisationPayment, pTradeInfo.CashFlowEQP_FlowCur.CashFlows.ToList(),
                            entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS, pCacheData);
                }            
                //
                #endregion Equalisation Payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Collateral, Collateral available
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                // FI 20160530 [21885] Alimentation de collateral
                cashBalanceStream.collateral = GetPosCollateral(pCS, pDbTransaction, idC, entityId, actorId, pTradeInfo, pDataDoc, pCacheData);
                cashBalanceStream.collateralSpecified = ArrFunc.IsFilled(cashBalanceStream.collateral);

                Money collateralAvailableIdC = null;

                if (pTradeInfo.IsCBOWithMGCCollatCur)
                    collateralAvailableIdC = GetMoneyIdC(collateralAvailable_FlowCur, idC);
                else if (pTradeInfo.IsCBOWithMGCCollatCTRVal)
                    collateralAvailableIdC = GetMoneyIdC(collateralAvailable_MGCCollatCTRVal, idC);
                else if (pTradeInfo.IsCBOWithMGCCTRVal)
                {
                    // TODO
                }

                CBExAmount collateralAvailableExchange = null;
                if (null != collateralAvailable_ExCTRValCur)
                    collateralAvailable_ExCTRValCur.Find(match => match.FlowCurrency == idC);

                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                //exchangecashPosition = GetExchangeCashPosition(collateralAvailableIdC, collateralAvailableExchange,
                //        entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, pTradeInfo.IsCBOWithMGCCTRVal);
                exchangecashPosition = GetExchangeCashPosition(collateralAvailableIdC, collateralAvailableExchange,
                        entityId, actorId, pTradeInfo.IsClearer, businessDateId, pTradeInfo.IsCBOWithMGCCTRVal);

                cashBalanceStream.collateralAvailable = new CollateralAvailable(exchangecashPosition);

                #region Déposit en devise et couverture en contrevaleur
                if (pTradeInfo.IsCBOWithMGCCollatCTRVal)
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Collateral available - constituent
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    cashBalanceStream.collateralAvailable.constituentSpecified = true;
                    cashBalanceStream.collateralAvailable.constituent = new CollateralAvailableConstituent
                    {

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Cash balance stream - Collateral available - constituent - collateral
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        collateral = GetMoney(collateralAvailable_FlowCur),

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Cash balance stream - Collateral available - constituent - collateralAlreadyUsed
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        collateralAlreadyUsed = GetMoney(
                        (from constituent in collateralAvailableConstituent_MGCCollatCTRVal
                         where constituent.Currency == idC
                         from money in constituent.AlreadyUsed
                         select money).ToList())
                    };

                    cashBalanceStream.collateralAvailable.constituentSpecified = (cashBalanceStream.collateralAvailable.constituent.collateralAlreadyUsed.Length > 0);
                }
                #endregion Déposit en devise et couverture en contrevaleur
                #region Déposit et couverture en contrevaleur
                if (false == pTradeInfo.IsCBOWithMGCCTRVal)
                {
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - cash used 
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    Money cashUsed = GetMoneyIdC(pTradeInfo.CashUsed_FlowCur, idC);
                    cashBalanceStream.cashUsedSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.cashUsed = GetCashPosition(cashUsed, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.cashUsed = GetCashPosition(cashUsed, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Collateral used
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    Money collateralUsed = GetMoneyIdC(collateralUsed_Method, idC);
                    cashBalanceStream.collateralUsedSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.collateralUsed = GetCashPosition(collateralUsed, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.collateralUsed = GetCashPosition(collateralUsed, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Uncovered Margin requirement
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    Money uncoveredMarginRequirement = GetMoneyIdC(pTradeInfo.GlobalDefectDeposits_FlowCur, idC);
                    cashBalanceStream.uncoveredMarginRequirementSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.uncoveredMarginRequirement = GetCashPosition(uncoveredMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.uncoveredMarginRequirement = GetCashPosition(uncoveredMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Cash balance stream - Margin call
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    cashBalanceStream.marginCallSpecified = true;
                    //
                    // Appel = Payer est pSqlActor
                    // Restitution = Payer est pSqlEntity
                    Money marginCall = GetMoneyIdC(pTradeInfo.MarginCall_FlowCur, idC);
                    cashBalanceStream.marginCall =
                        GetSimplePayment(marginCall, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS);
                }
                #endregion Déposit et couverture en contrevaleur
                #region Cash Balance
                //
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Cash Balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                Money cashBalanceIdC = GetMoneyIdC(pTradeInfo.CashBalance_FlowCur, idC);
                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                //cashBalanceStream.cashBalance = GetCashPosition(cashBalanceIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                cashBalanceStream.cashBalance = GetCashPosition(cashBalanceIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                #endregion Cash Balance
                #region Realized Margin
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Realized Margin
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                cashBalanceStream.realizedMarginSpecified = pTradeInfo.CashFlowRMG_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.realizedMarginSpecified)
                {
                    // PM 20150616 [21124] Ajout pIsCtrVal à false
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.realizedMargin = GetMarginConstituent(pTradeInfo.CashFlowRMG_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                    cashBalanceStream.realizedMargin = GetMarginConstituent(pTradeInfo.CashFlowRMG_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                }
                #endregion Realized Margin
                #region Unrealized Margin
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Unrealized Margin
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                cashBalanceStream.unrealizedMarginSpecified = pTradeInfo.CashFlowUMG_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.unrealizedMarginSpecified)
                {
                    // PM 20150616 [21124] Ajout pIsCtrVal à false
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.unrealizedMargin = GetMarginConstituent(pTradeInfo.CashFlowUMG_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                    cashBalanceStream.unrealizedMargin = GetMarginConstituent(pTradeInfo.CashFlowUMG_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                }
                #endregion Unrealized Margin
                #region Liquidating Value
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Liquidating Value
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //
                //PM 20140909 [20066][20185] Gestion méthode UK (ETD & CFD)
                //cashBalanceStream.liquidatingValueSpecified = true;
                //Money liquidatingValueIdC = GetMoneyIdC(pTradeInfo.OtherFlowLOV_FlowCur, idC);

                //cashBalanceStream.liquidatingValue =
                //GetCashPosition(liquidatingValueIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                cashBalanceStream.liquidatingValueSpecified = pTradeInfo.CashFlowLOV_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.liquidatingValueSpecified)
                {
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.liquidatingValue = GetOptionLiquidatingValue(pTradeInfo.CashFlowLOV_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.liquidatingValue = GetOptionLiquidatingValue(pTradeInfo.CashFlowLOV_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion Liquidating Value
                #region Market Value
                // PM 20150616 [21124] Add marketValue
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Market Value
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                cashBalanceStream.marketValueSpecified = pTradeInfo.CashFlowMKV_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.marketValueSpecified)
                {
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.marketValue = GetDetailedCashPosition(pTradeInfo.CashFlowMKV_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                    cashBalanceStream.marketValue = GetDetailedCashPosition(pTradeInfo.CashFlowMKV_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null, pCacheData);
                }
                #endregion
                #region Funding
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Funding
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140909 [20066][20185] Add funding
                cashBalanceStream.fundingSpecified = pTradeInfo.CashFlowFDA_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.fundingSpecified)
                {
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.funding = GetDetailedCashPosition(pTradeInfo.CashFlowFDA_FlowCur, idC, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                    cashBalanceStream.funding = GetDetailedCashPosition(pTradeInfo.CashFlowFDA_FlowCur, idC, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                }
                #endregion
                #region Borrowing
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Borrowing
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20150323 [POC] Add Borrowing
                cashBalanceStream.borrowingSpecified = pTradeInfo.CashFlowBWA_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.borrowingSpecified)
                {
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.borrowing = GetDetailedCashPosition(pTradeInfo.CashFlowBWA_FlowCur, idC, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                    cashBalanceStream.borrowing = GetDetailedCashPosition(pTradeInfo.CashFlowBWA_FlowCur, idC, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                }
                #endregion
                #region Unsettled
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Unsettled
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20150318 [POC] Add Unsettled
                cashBalanceStream.unsettledCashSpecified = pTradeInfo.CashFlowUST_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.unsettledCashSpecified)
                {
                    // PM 20150616 [21124] Ajout pIsCtrVal à false
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.unsettledCash = GetDetailedCashPosition(pTradeInfo.CashFlowUST_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, true, pCacheData);
                    cashBalanceStream.unsettledCash = GetDetailedCashPosition(pTradeInfo.CashFlowUST_FlowCur, idC, false, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, true, pCacheData);
                }
                #endregion
                #region Forward Cash Payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Forward Cash Payment
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140912 [20066][20185] Add forwardCashPayment
                cashBalanceStream.forwardCashPaymentSpecified = pTradeInfo.ForwardPaymentStl_FlowCur.IsFilledCurrencyFlows(idC);
                if (cashBalanceStream.forwardCashPaymentSpecified)
                {
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.forwardCashPayment = GetCashBalancePayment(pTradeInfo.ForwardPaymentStl_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.forwardCashPayment = GetCashBalancePayment(pTradeInfo.ForwardPaymentStl_FlowCur, idC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion Forward Cash Payment
                #region Equity Balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Equity Balance
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140910 [20066][20185] Add equityBalance
                if (pTradeInfo.EquityBalance_FlowCur != null)
                {
                    Money equityBalanceIdC = GetMoneyIdC(pTradeInfo.EquityBalance_FlowCur, idC);
                    cashBalanceStream.equityBalanceSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.equityBalance = GetCashPosition(equityBalanceIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.equityBalance = GetCashPosition(equityBalanceIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion
                #region Equity Balance With Forward Cash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Equity Balance With Forward Cash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140910 [20066][20185] Add equityBalanceWithForwardCash
                if (pTradeInfo.EquityBalanceForwardCash_FlowCur != null)
                {
                    Money equityBalanceForwardCashIdC = GetMoneyIdC(pTradeInfo.EquityBalanceForwardCash_FlowCur, idC);
                    cashBalanceStream.equityBalanceWithForwardCashSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.equityBalanceWithForwardCash = GetCashPosition(equityBalanceForwardCashIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.equityBalanceWithForwardCash = GetCashPosition(equityBalanceForwardCashIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion
                #region Total Account Value
                // PM 20150616 [21124] Add totalAccountValue
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Total Account Value
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (pTradeInfo.TotalAccountValue_FlowCur != null)
                {
                    Money totalAccountValueIdC = GetMoneyIdC(pTradeInfo.TotalAccountValue_FlowCur, idC);
                    cashBalanceStream.totalAccountValueSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.totalAccountValue = GetCashPosition(totalAccountValueIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.totalAccountValue = GetCashPosition(totalAccountValueIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion
                #region Excess Deficit
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Excess Deficit
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140910 [20066][20185] Add excessDeficit
                if (pTradeInfo.ExcessDeficit_FlowCur != null)
                {
                    Money excessDeficitIdC = GetMoneyIdC(pTradeInfo.ExcessDeficit_FlowCur, idC);
                    cashBalanceStream.excessDeficitSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.excessDeficit = GetCashPosition(excessDeficitIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.excessDeficit = GetCashPosition(excessDeficitIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion
                #region Excess Deficit With Forward Cash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Cash Balance - Cash balance stream - Excess Deficit With Forward Cash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //PM 20140910 [20066][20185] Add excessDeficitWithForwardCash
                if (pTradeInfo.ExcessDeficitForwardCash_FlowCur != null)
                {
                    Money excessDeficitForwardCashIdC = GetMoneyIdC(pTradeInfo.ExcessDeficitForwardCash_FlowCur, idC);
                    cashBalanceStream.excessDeficitWithForwardCashSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalanceStream.excessDeficitWithForwardCash = GetCashPosition(excessDeficitForwardCashIdC, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalanceStream.excessDeficitWithForwardCash = GetCashPosition(excessDeficitForwardCashIdC, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pCacheData"></param>
        /// PM 20150616 [21124] Add pCacheData
        // EG 20180205 [23769] Add dbTransaction  
        private static void SetExchangeStream(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDoc, CBTradeInfo pTradeInfo, CBCache pCacheData)
        {
            EfsML.v30.CashBalance.CashBalance cashBalance = (EfsML.v30.CashBalance.CashBalance)pDataDoc.CurrentProduct.Product;
            IProductBase productBase = (IProductBase)cashBalance;
            ITradeHeader tradeHeader = pDataDoc.TradeHeader;

            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
            string businessDateId = tradeHeader.ClearedDate.Id;

            string entityId = pTradeInfo.SqlEntity.XmlId;
            string actorId = pTradeInfo.SqlActor.XmlId;

            cashBalance.exchangeCashBalanceStreamSpecified = false;

            //if (pTradeInfo.IsCBOWithMGCCTRVal)
            if (((pTradeInfo.CbCalcMethod == CashBalanceCalculationMethodEnum.CSBUK) && (StrFunc.IsFilled(pTradeInfo.CashBalanceIDC)))
                || pTradeInfo.IsCBOWithMGCCTRVal)
            {
                cashBalance.exchangeCashBalanceStreamSpecified = true;
                cashBalance.exchangeCashBalanceStream = new ExchangeCashBalanceStream();
                //
                if (pTradeInfo.CbCalcMethod == CashBalanceCalculationMethodEnum.CSBUK)
                {
                    #region CSBUK
                    cashBalance.exchangeCashBalanceStream.currency = new Currency(pTradeInfo.CashBalanceIDC);
                    IBusinessCenters currencyBCS = productBase.LoadBusinessCenters(CSTools.SetCacheOn(pCS), pDbTransaction,
                        null, new string[] { pTradeInfo.CashBalanceIDC }, null);
                    if (pTradeInfo.CtrValTradeInfo != null)
                    {
                        CBCtrValTradeInfo ctrValTradeInfo = pTradeInfo.CtrValTradeInfo;
                        ExchangeCashBalanceStream exchStream = cashBalance.exchangeCashBalanceStream;
                        #region Margin Requirement
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Margin requirement
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.marginRequirementSpecified = (ctrValTradeInfo.MarginRequirement != null);
                        if (exchStream.marginRequirementSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.marginRequirement = GetCashPosition(ctrValTradeInfo.MarginRequirement, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.marginRequirement = GetCashPosition(ctrValTradeInfo.MarginRequirement, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion

                        #region Cash Available / Cash Payment / Previous Cash Balance / Cash Flows
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.cashAvailableSpecified = true;
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //CashPosition cashAvailablePosition = GetCashPosition(ctrValTradeInfo.CashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                        CashPosition cashAvailablePosition = GetCashPosition(ctrValTradeInfo.CashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                        CashBalancePayment cashBalancePayment = new CashBalancePayment();
                        exchStream.cashAvailable = new CashAvailable(cashAvailablePosition)
                        {
                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Cash Balance - Exchange cash balance stream - Cash Available - Constituent
                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            constituentSpecified = true,
                            constituent = new CashAvailableConstituent()
                            {
                                cashBalancePayment = cashBalancePayment
                            }
                        };
                        #region Cash Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Balance Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Payment Global
                        Money cashPayment = ctrValTradeInfo.CashPayment;
                        if (null == cashPayment)
                        {
                            cashPayment = new Money(0, ctrValTradeInfo.Currency);
                        }
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //GetCashPosition(cashPayment, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, cashBalancePayment);
                        GetCashPosition(cashPayment, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, cashBalancePayment);
                        // Cash Payment Deposit
                        cashBalancePayment.cashDepositSpecified = (ctrValTradeInfo.CashPaymentDeposit != null);
                        if (cashBalancePayment.cashDepositSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(ctrValTradeInfo.CashPaymentDeposit, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)cashBalancePayment.cashDeposit);
                            GetCashPosition(ctrValTradeInfo.CashPaymentDeposit, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, cashBalancePayment.cashDeposit);
                        }
                        // Cash Payment Withdrawal
                        cashBalancePayment.cashWithdrawalSpecified = (ctrValTradeInfo.CashPaymentWithdrawal != null);
                        if (cashBalancePayment.cashWithdrawalSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(ctrValTradeInfo.CashPaymentWithdrawal, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)cashBalancePayment.cashWithdrawal);
                            GetCashPosition(ctrValTradeInfo.CashPaymentWithdrawal, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null, cashBalancePayment.cashWithdrawal);
                        }
                        #endregion
                        #region Previous Cash Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Previous Cash Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        Money previousCashBalance = ctrValTradeInfo.PreviousCashBalance;
                        if (null == previousCashBalance)
                        {
                            previousCashBalance = new Money(0, ctrValTradeInfo.Currency);
                        }
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //exchStream.cashAvailable.constituent.previousCashBalance = GetCashPosition(previousCashBalance, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                        exchStream.cashAvailable.constituent.previousCashBalance = GetCashPosition(previousCashBalance, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        #endregion

                        #region Cash Flows
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                        //CashPosition cashFlowsPosition = GetCashPosition(ctrValTradeInfo.CashFlows, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                        CashPosition cashFlowsPosition = GetCashPosition(ctrValTradeInfo.CashFlows, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);

                        #region Variation Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows - Variation Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        SimplePayment variationMarginPayment = GetSimplePayment(ctrValTradeInfo.VariationMargin, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS);
                        #endregion

                        #region Premium
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows - Premium
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        SimplePayment premiumPayment = GetSimplePayment(ctrValTradeInfo.Premium, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS);
                        #endregion

                        #region Cash Settlement
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows - Cash Settlement
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        SimplePayment cashSettlementPayment = GetSimplePayment(ctrValTradeInfo.CashSettlement, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS);

                        List<OptionMarginConstituent> cashSettlementOption = new List<OptionMarginConstituent>();
                        if ((ctrValTradeInfo.CashSettlementOptionPremium != null) || (ctrValTradeInfo.CashSettlementOptionMarkToMarket != null))
                        {
                            // Option Premium
                            if (ctrValTradeInfo.CashSettlementOptionPremium != null)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //CashPosition optCash = GetCashPosition(ctrValTradeInfo.CashSettlementOptionPremium, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                                CashPosition optCash = GetCashPosition(ctrValTradeInfo.CashSettlementOptionPremium, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                                OptionMarginConstituent optionMargin = new OptionMarginConstituent(optCash)
                                {
                                    valuationMethod = FuturesValuationMethodEnum.PremiumStyle
                                };
                                cashSettlementOption.Add(optionMargin);
                            }
                            // Option Mark To Market
                            if (ctrValTradeInfo.CashSettlementOptionMarkToMarket != null)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //CashPosition optCash = GetCashPosition(ctrValTradeInfo.CashSettlementOptionMarkToMarket, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                                CashPosition optCash = GetCashPosition(ctrValTradeInfo.CashSettlementOptionMarkToMarket, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                                OptionMarginConstituent optionMargin = new OptionMarginConstituent(optCash)
                                {
                                    valuationMethod = FuturesValuationMethodEnum.FuturesStyleMarkToMarket
                                };
                                cashSettlementOption.Add(optionMargin);
                            }
                        }

                        CashPosition cashPositionOther = null;
                        if (ctrValTradeInfo.CashSettlementOther != null)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //cashSettlementConstituent.other = GetCashPosition(ctrValTradeInfo.CashSettlementOther, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            cashPositionOther = GetCashPosition(ctrValTradeInfo.CashSettlementOther, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }


                        //PM 20150320 [POC] Changement de type de cashSettlement
                        //exchStream.cashAvailable.constituent.cashFlows.constituent.cashSettlement = new ContractSimplePayment(cashSettlement);
                        ContractSimplePaymentConstituent cashSettlementConstituent = new ContractSimplePaymentConstituent(cashSettlementPayment)
                        {
                            optionSpecified = (cashSettlementOption.Count > 0),
                            option = (cashSettlementOption.Count > 0) ? cashSettlementOption.ToArray():null,

                            otherSpecified = (cashPositionOther != null),
                            other = cashPositionOther
                        };
                        #endregion

                        #region Fee
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows - Constituent - Fee
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        DetailedContractPayment[] fees = GetExchangeFees(pCS, pDbTransaction, productBase, pTradeInfo.CashBalanceIDC, pTradeInfo, pTradeInfo.DtBusiness, currencyBCS);
                        #endregion Fee

                        #region SafeKeeping
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash available - Constituent - Cash flows - Constituent - Safekeeping
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // PM 20150709 [21103] Add safekeeping
                        DetailedContractPayment[] safekeeping = GetExchangeSafekeeping(pCS, pDbTransaction, productBase, pTradeInfo.CashBalanceIDC, pTradeInfo, pTradeInfo.DtBusiness, currencyBCS);
                        #endregion Safekeeping

                        #region Equalisation Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Available - Constituent - Cash Flows - Equalisation Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // PM 20170911 [23408] Add Equalisation Payment
                        SimplePayment equalisationPayment = null;
                        if (ctrValTradeInfo.EqualisationPayment != default)
                            equalisationPayment = GetSimplePayment(ctrValTradeInfo.EqualisationPayment, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, currencyBCS);
                        #endregion Equalisation Payment

                        exchStream.cashAvailable.constituent.cashFlows = new CashFlows(cashFlowsPosition)
                        {
                            constituent = new CashFlowsConstituent()
                            {
                                variationMargin = new ContractSimplePayment(variationMarginPayment),
                                premium = new ContractSimplePayment(premiumPayment),
                                cashSettlement = cashSettlementConstituent,
                                fee = fees,
                                safekeepingSpecified = (safekeeping != null) && (safekeeping.Count() > 0),
                                safekeeping = (safekeeping != null) && (safekeeping.Count() > 0) ? safekeeping:null,
                                equalisationPaymentSpecified = (ctrValTradeInfo.EqualisationPayment != default),
                                equalisationPayment = (ctrValTradeInfo.EqualisationPayment != default) ? new ContractSimplePayment(equalisationPayment) : null,
                            }
                        };
                        #endregion
                        #endregion

                        #region Collateral Available
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Collateral Available
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.collateralAvailableSpecified = (ctrValTradeInfo.CollateralAvailable != null);
                        if (exchStream.collateralAvailableSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.collateralAvailable = GetCashPosition(ctrValTradeInfo.CollateralAvailable, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.collateralAvailable = GetCashPosition(ctrValTradeInfo.CollateralAvailable, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Cash Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Cash Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.cashBalanceSpecified = (ctrValTradeInfo.CashBalance != null);
                        if (exchStream.cashBalanceSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.cashBalance = GetCashPosition(ctrValTradeInfo.CashBalance, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.cashBalance = GetCashPosition(ctrValTradeInfo.CashBalance, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Realized Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Realized Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.realizedMarginSpecified = ctrValTradeInfo.CBFlowsRealizedMargin.IsFilledCounterValueFlows();
                        if (exchStream.realizedMarginSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.realizedMargin = GetMarginConstituent(ctrValTradeInfo.CBFlowsRealizedMargin, null, true, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                            exchStream.realizedMargin = GetMarginConstituent(ctrValTradeInfo.CBFlowsRealizedMargin, null, true, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                        }
                        #endregion
                        #region Unrealized Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Unrealized Margin
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.unrealizedMarginSpecified = ctrValTradeInfo.CBFlowsUnrealizedMargin.IsFilledCounterValueFlows();
                        if (exchStream.unrealizedMarginSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.unrealizedMargin = GetMarginConstituent(ctrValTradeInfo.CBFlowsUnrealizedMargin, null, true, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, pCacheData);
                            exchStream.unrealizedMargin = GetMarginConstituent(ctrValTradeInfo.CBFlowsUnrealizedMargin, null, true, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, pCacheData);
                        }
                        #endregion
                        #region Liquidating Value
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Liquidating Value
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.liquidatingValueSpecified = ((ctrValTradeInfo.OptionValue != null) || (ctrValTradeInfo.LongOptionValue != null) || (ctrValTradeInfo.ShortOptionValue != null));
                        if (exchStream.liquidatingValueSpecified)
                        {
                            OptionLiquidatingValue liquidatingValue = new OptionLiquidatingValue();
                            exchStream.liquidatingValue = liquidatingValue;
                            // Option Value
                            Money optionValue = ctrValTradeInfo.OptionValue;
                            if (null == optionValue)
                            {
                                optionValue = new Money(0, ctrValTradeInfo.Currency);
                            }
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(optionValue, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, liquidatingValue);
                            GetCashPosition(optionValue, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, liquidatingValue);
                            // Long Option Value
                            liquidatingValue.longOptionValueSpecified = (ctrValTradeInfo.LongOptionValue != null);
                            if (liquidatingValue.longOptionValueSpecified)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //liquidatingValue.longOptionValue = GetCashPosition(ctrValTradeInfo.LongOptionValue, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                                liquidatingValue.longOptionValue = GetCashPosition(ctrValTradeInfo.LongOptionValue, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                            }
                            // Short Option Value
                            liquidatingValue.shortOptionValueSpecified = (ctrValTradeInfo.ShortOptionValue != null);
                            if (liquidatingValue.shortOptionValueSpecified)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //liquidatingValue.shortOptionValue = GetCashPosition(ctrValTradeInfo.ShortOptionValue, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                                liquidatingValue.shortOptionValue = GetCashPosition(ctrValTradeInfo.ShortOptionValue, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                            }
                        }
                        #endregion
                        #region Market Value
                        // PM 20150616 [21124] Add marketValue
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Market Value
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.marketValueSpecified = (ctrValTradeInfo.MarketValue != null);
                        if (exchStream.marketValueSpecified)
                        {
                            exchStream.marketValue = new DetailedCashPosition();
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(ctrValTradeInfo.MarketValue, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, exchStream.marketValue);
                            GetCashPosition(ctrValTradeInfo.MarketValue, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, exchStream.marketValue);
                        }
                        #endregion
                        #region Funding
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Funding
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.fundingSpecified = (ctrValTradeInfo.Funding != null);
                        if (exchStream.fundingSpecified)
                        {
                            exchStream.funding = new DetailedCashPosition();
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(ctrValTradeInfo.Funding, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, exchStream.funding);
                            GetCashPosition(ctrValTradeInfo.Funding, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, exchStream.funding);
                        }
                        #endregion
                        #region Borrowing
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Borrowing
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // PM 20150323 [POC] Add Borrowing
                        exchStream.borrowingSpecified = (ctrValTradeInfo.Borrowing != null);
                        if (exchStream.borrowingSpecified)
                        {
                            exchStream.borrowing = new DetailedCashPosition();
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(ctrValTradeInfo.Borrowing, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, exchStream.borrowing);
                            GetCashPosition(ctrValTradeInfo.Borrowing, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, exchStream.borrowing);
                        }
                        #endregion
                        #region Unsettled Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Unsettled Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.unsettledCashSpecified = ctrValTradeInfo.CBFlowsUnsettledCash.IsFilledCounterValueFlows();
                        if (exchStream.unsettledCashSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.unsettledCash = GetDetailedCashPosition(ctrValTradeInfo.CBFlowsUnsettledCash, null, true, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, true, pCacheData);
                            exchStream.unsettledCash = GetDetailedCashPosition(ctrValTradeInfo.CBFlowsUnsettledCash, null, true, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, true, pCacheData);
                        }
                        #endregion
                        #region Forward Cash Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Forward Cash Payment
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.forwardCashPaymentSpecified = ((ctrValTradeInfo.CashForward != null) || (ctrValTradeInfo.CashForwardDeposit != null) || (ctrValTradeInfo.CashForwardWithdrawal != null));
                        if (exchStream.forwardCashPaymentSpecified)
                        {
                            CashBalancePayment payment = new CashBalancePayment();
                            exchStream.forwardCashPayment = payment;
                            // Montant Global
                            Money cashForward = ctrValTradeInfo.CashForward;
                            if (null == cashForward)
                            {
                                cashForward = new Money(0, ctrValTradeInfo.Currency);
                            }
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //GetCashPosition(cashForward, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)payment);
                            GetCashPosition(cashForward, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, (CashPosition)payment);
                            // Deposit
                            payment.cashDepositSpecified = (ctrValTradeInfo.CashForwardDeposit != null);
                            if (payment.cashDepositSpecified)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //GetCashPosition(ctrValTradeInfo.CashForwardDeposit, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)payment.cashDeposit);
                                GetCashPosition(ctrValTradeInfo.CashForwardDeposit, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null, (CashPosition)payment.cashDeposit);
                            }
                            //Withdrawal
                            payment.cashWithdrawalSpecified = (ctrValTradeInfo.CashForwardWithdrawal != null);
                            if (payment.cashWithdrawalSpecified)
                            {
                                // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                                //GetCashPosition(ctrValTradeInfo.CashForwardWithdrawal, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null, (CashPosition)payment.cashWithdrawal);
                                GetCashPosition(ctrValTradeInfo.CashForwardWithdrawal, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null, (CashPosition)payment.cashWithdrawal);
                            }
                        }
                        #endregion
                        #region Equity Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Equity Balance
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.equityBalanceSpecified = (ctrValTradeInfo.EquityBalance != null);
                        if (exchStream.equityBalanceSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.equityBalance = GetCashPosition(ctrValTradeInfo.EquityBalance, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.equityBalance = GetCashPosition(ctrValTradeInfo.EquityBalance, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Equity Balance With Forward Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Equity Balance With Forward Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.equityBalanceWithForwardCashSpecified = (ctrValTradeInfo.EquityBalanceWithForwardCash != null);
                        if (exchStream.equityBalanceWithForwardCashSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.equityBalanceWithForwardCash = GetCashPosition(ctrValTradeInfo.EquityBalanceWithForwardCash, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.equityBalanceWithForwardCash = GetCashPosition(ctrValTradeInfo.EquityBalanceWithForwardCash, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Total Account Value
                        // PM 20150616 [21124] Add totalAccountValue
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Total Account Value
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.totalAccountValueSpecified = (ctrValTradeInfo.TotalAccountValue != null);
                        if (exchStream.totalAccountValueSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.totalAccountValue = GetCashPosition(ctrValTradeInfo.TotalAccountValue, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.totalAccountValue = GetCashPosition(ctrValTradeInfo.TotalAccountValue, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Excess Deficit
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Excess Deficit
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.excessDeficitSpecified = (ctrValTradeInfo.ExcessDeficit != null);
                        if (exchStream.excessDeficitSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.excessDeficit = GetCashPosition(ctrValTradeInfo.ExcessDeficit, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.excessDeficit = GetCashPosition(ctrValTradeInfo.ExcessDeficit, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                        #region Excess Deficit With Forward Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Cash Balance - Exchange cash balance stream - Excess Deficit With Forward Cash
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        exchStream.excessDeficitWithForwardCashSpecified = (ctrValTradeInfo.ExcessDeficitWithForwardCash != null);
                        if (exchStream.excessDeficitWithForwardCashSpecified)
                        {
                            // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                            //exchStream.excessDeficitWithForwardCash = GetCashPosition(ctrValTradeInfo.ExcessDeficitWithForwardCash, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                            exchStream.excessDeficitWithForwardCash = GetCashPosition(ctrValTradeInfo.ExcessDeficitWithForwardCash, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    cashBalance.exchangeCashBalanceStream.currency = new Currency(pTradeInfo.ExchangeIDC);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    // [TODO RD: A revoir previousMarginConstituent pour exchangeCashBalanceStream]
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituentSpecified = true;
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent = new PreviousMarginConstituent();
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Margin requirement
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                
                    Money previousMarginRequirement = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.marginRequirement =
                        GetCashPosition(previousMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Cash available
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                
                    Money previousCashAvailable = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.cashAvailable =
                        GetCashPosition(previousCashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Cash used
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                
                    Money previousCashUsed = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.cashUsed =
                        GetCashPosition(previousCashUsed, entityId, actorId, pTradeInfo.IsNotClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Collateral available
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                
                    Money previousCollateralAvailable = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.collateralAvailable =
                        GetCashPosition(previousCollateralAvailable, entityId, actorId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Collateral used
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  
                    Money previousCollateralUsed = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.collateralUsed =
                        GetCashPosition(previousCollateralUsed, entityId, actorId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent - Uncovered Margin requirement
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                
                    Money previousUncoveredMarginRequirement = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.previousMarginConstituent.uncoveredMarginRequirement =
                        GetCashPosition(previousUncoveredMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, string.Empty, pTradeInfo.DtBusinessPrev);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Margin requirement
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    // Déposit payé (à couvrir par le Dealer) dans le cas du Dealer
                    // Déposit reçu (à couvrir par le Dealer en face) dans le cas du Clearer
                    //
                    //PM 20140917 Réécriture pour éviter les plantages
                    //Money marginRequirement =
                    //    (from moneyByIDC in
                    //         (from deposit in pTradeInfo.FlowDeposit
                    //          select deposit.Deposit_MGCCTRVal).GroupBy(money => money.Currency)
                    //     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).First();
                    Money marginRequirement =
                        (
                        from deposit in pTradeInfo.FlowDeposit
                        where (deposit.Deposit_MGCCTRVal != null)
                        group deposit.Deposit_MGCCTRVal by deposit.Deposit_MGCCTRVal.Currency
                            into moneyByIDC
                            select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                        ).FirstOrDefault();

                    if (null == marginRequirement)
                        marginRequirement = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.marginRequirementSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalance.exchangeCashBalanceStream.marginRequirement = GetCashPosition(marginRequirement, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalance.exchangeCashBalanceStream.marginRequirement = GetCashPosition(marginRequirement, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Uncovered Margin requirement
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    // Déposit payé (à couvrir par le Dealer) dans le cas du Dealer
                    // Déposit reçu (à couvrir par le Dealer en face) dans le cas du Clearer
                    Money uncoveredMarginRequirement = pTradeInfo.GlobalDefectDeposits_CTRValCur;
                    if (null == uncoveredMarginRequirement)
                        uncoveredMarginRequirement = new Money(0, pTradeInfo.ExchangeIDC);
                    cashBalance.exchangeCashBalanceStream.uncoveredMarginRequirementSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalance.exchangeCashBalanceStream.uncoveredMarginRequirement = GetCashPosition(uncoveredMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalance.exchangeCashBalanceStream.uncoveredMarginRequirement = GetCashPosition(uncoveredMarginRequirement, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Cash available
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    Money cashAvailable = pTradeInfo.GlobalDefectDeposits_CTRValCur;
                    if (null == cashAvailable)
                        cashAvailable = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.cashAvailableSpecified = true;
                    //PM 20140918 [20066][20185] Changement du type de cashAvailable de CashPosition vers CashAvailable
                    //cashBalance.exchangeCashBalanceStream.cashAvailable =
                    //    GetCashPosition(cashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, tradeHearder.tradeDate.efs_id, null);
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //CashPosition cashAvailablePosition = GetCashPosition(cashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    CashPosition cashAvailablePosition = GetCashPosition(cashAvailable, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                    cashBalance.exchangeCashBalanceStream.cashAvailable = new CashAvailable(cashAvailablePosition)
                    {
                        constituentSpecified = false,
                        constituent = null
                    };
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Collateral available
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    //PM 20140917 Réécriture pour éviter les plantages
                    //Money collateralAvailable =
                    //    (from moneyByIDC in
                    //         (from collateral in pTradeInfo.FlowCollateral
                    //          where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                    //          && (collateral.IsAllowed_AtLeastOneCSS)
                    //          select collateral.Amount_MGCCTRVal.Available).GroupBy(money => money.Currency)
                    //     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).First();
                    Money collateralAvailable =
                        (
                        from collateral in pTradeInfo.FlowCollateral
                        where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                        && (collateral.IsAllowed_AtLeastOneCSS)
                        && (collateral.Amount_MGCCTRVal.Available != null)
                        group collateral.Amount_MGCCTRVal.Available by collateral.Amount_MGCCTRVal.Available.Currency
                            into moneyByIDC
                            select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                        ).FirstOrDefault();

                    if (null == collateralAvailable)
                        collateralAvailable = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.collateralAvailableSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalance.exchangeCashBalanceStream.collateralAvailable = GetCashPosition(collateralAvailable, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalance.exchangeCashBalanceStream.collateralAvailable = GetCashPosition(collateralAvailable, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - cash used 
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    Money cashUsed = pTradeInfo.CashUsed_CTRValCur;
                    if (null == cashUsed)
                        cashUsed = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.cashUsedSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalance.exchangeCashBalanceStream.cashUsed = GetCashPosition(cashUsed, entityId, actorId, pTradeInfo.IsNotClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalance.exchangeCashBalanceStream.cashUsed = GetCashPosition(cashUsed, entityId, actorId, pTradeInfo.IsNotClearer, businessDateId, null);
                    //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Collateral used
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    //PM 20140917 Réécriture pour éviter les plantages
                    //Money collateralUsed =
                    //    (from moneyByIDC in
                    //         (from collateral in pTradeInfo.FlowCollateral
                    //          where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                    //          && (collateral.IsAllowed_AtLeastOneCSS)
                    //          select collateral.Amount_MGCCTRVal.Used).GroupBy(money => money.Currency)
                    //     select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).First();
                    Money collateralUsed =
                        (
                        from collateral in pTradeInfo.FlowCollateral
                        where (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)
                        && (collateral.IsAllowed_AtLeastOneCSS)
                        && (collateral.Amount_MGCCTRVal.Used != null)
                        group collateral.Amount_MGCCTRVal.Used by collateral.Amount_MGCCTRVal.Used.Currency
                            into moneyByIDC
                            select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                        ).FirstOrDefault();
                    //         
                    if (null == collateralUsed)
                        collateralUsed = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.collateralUsedSpecified = true;
                    // PM 20171117 [23509] Ajout de businessDateId = clearedDate.id pour remplacer tradeHeader.tradeDate.efs_id
                    //cashBalance.exchangeCashBalanceStream.collateralUsed = GetCashPosition(collateralUsed, entityId, actorId, pTradeInfo.IsClearer, tradeHeader.tradeDate.efs_id, null);
                    cashBalance.exchangeCashBalanceStream.collateralUsed = GetCashPosition(collateralUsed, entityId, actorId, pTradeInfo.IsClearer, businessDateId, null);

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Cash Balance - Exchange cash balance stream - Margin call
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //
                    // Appel = Payer est pSqlActor
                    // Restitution = Payer est pSqlEntity
                    IBusinessCenters bcs = productBase.LoadBusinessCenters(CSTools.SetCacheOn(pCS), pDbTransaction, 
                        null, new string[] { pTradeInfo.ExchangeIDC }, null);

                    Money marginCall = pTradeInfo.MarginCall_CTRValCur;
                    if (null == marginCall)
                        marginCall = new Money(0, pTradeInfo.ExchangeIDC);

                    cashBalance.exchangeCashBalanceStream.marginCallSpecified = true;
                    cashBalance.exchangeCashBalanceStream.marginCall =
                        GetSimplePayment(marginCall, entityId, actorId, pTradeInfo.IsClearer, pTradeInfo.DtBusiness, bcs);
                }
            }
        }

        /// <summary>
        /// Alimente l'élement detail de {pPayment}
        /// <para>ce dernier contient les montant HT et les taxes par DC</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPayment"></param>
        /// <param name="pCashFlow_FlowCur"></param>
        /// <param name="pIdC"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pEntityPartyId"></param>
        /// <param name="pActorPartyId"></param>
        /// <param name="pIsClearer"></param>
        /// FI 20120802 add SetFeeDetail
        /// PM 20140910 [20066][20185] Add pCacheData
        /// PM 20150709 [21103] Renommage de SetFeeDetail en SetContractPaymentDetail
        // EG 20180205 [23769] Del pCS
        private static void SetContractPaymentDetail(DetailedContractPayment pPayment, List<CBDetCashFlows> pCashFlow_FlowCur, 
            string pIdC, string pPaymentType, string pEntityPartyId, string pActorPartyId, bool pIsClearer, CBCache pCacheData)
        {
            //Detail des montants HT par DerivativeContract ou Asset
            List<ContractAmountAndTax> lst = new List<ContractAmountAndTax>();
            // Recherche de la somme des montants des frais et des tax pour le type de payment pPaymentType et la devise pIdC
            // par AssetCategory et IdDC ou IdAsset
            var flowById = from flow in pCashFlow_FlowCur
                           where (flow.PaymentType == pPaymentType)
                           from flowMoney in flow.CurrencyAmount
                           where (flowMoney.Currency == pIdC)
                           group flow by new { flow.AssetCategory, OTCmlId = (flow.IdDC != 0) ? flow.IdDC : flow.IdAsset }
                               into groupedFlow
                               select new
                               {
                                   groupedFlow.Key.AssetCategory,
                                   groupedFlow.Key.OTCmlId,
                                   FeeAmount = (from curFlow in groupedFlow
                                                where (curFlow.IdTaxSpecified == false)
                                                from money in curFlow.CurrencyAmount
                                                select money.amount.DecValue).Sum(),
                                   FeeTax = (from curFlow in groupedFlow
                                             where (curFlow.IdTaxSpecified == true)
                                             from money in curFlow.CurrencyAmount
                                             select money.amount.DecValue).Sum(),
                               };
            //
            foreach (var id in flowById)
            {
                //Somme des montants HT (=>fee.IdTaxSpecified == false) par DC
                decimal feeAmountByDC = id.FeeAmount;

                //Somme des taxes  (=>fee.IdTaxSpecified == true) par DC
                decimal feeTaxByDC = id.FeeTax;

                if (feeAmountByDC != 0 || feeTaxByDC != 0)
                {
                    Money feeByDCMoney = new Money(feeAmountByDC, pIdC);
                    ContractAmountAndTax detail = new ContractAmountAndTax();
                    //PM 20150324 [POC] AssetCategory devient nullable
                    if (id.AssetCategory.HasValue)
                    {
                        if (null != pCacheData)
                        {
                            //PM 20150324 [POC] AssetCategory devient nullable
                            //CBAssetInfo assetInfo = pCacheData.GetAssetInfo(id.AssetCategory, id.OTCmlId);
                            CBAssetInfo assetInfo = pCacheData.GetContractAssetInfo(id.AssetCategory.Value, id.OTCmlId);
                            if (null != assetInfo)
                            {
                                detail.OTCmlId = id.OTCmlId;
                                detail.otcmlIdSpecified = true;
                                detail.Sym = assetInfo.Sym;
                                detail.SymSpecified = StrFunc.IsFilled(assetInfo.Sym);
                                detail.Exch = assetInfo.Exch;
                                detail.ExchSpecified = StrFunc.IsFilled(assetInfo.Exch);
                            }
                        }

                        //PM 20150324 [POC] AssetCategory devient nullable
                        //detail.assetCategory = id.AssetCategory;
                        detail.assetCategory = id.AssetCategory.Value;
                        detail.assetCategorySpecified = true;
                    }
                    detail.Amt = System.Math.Abs(feeByDCMoney.amount.DecValue);
                    detail.AmtSide = GetCrDrEnum(feeByDCMoney, pEntityPartyId, pActorPartyId, pIsClearer);
                    // FI 20141208 [XXXXX] alimentation de AmtSideSpecified
                    detail.AmtSideSpecified = (detail.Amt > 0);

                    detail.taxSpecified = (feeTaxByDC != 0);
                    if (detail.taxSpecified)
                    {
                        Money feeTaxByDCMoney = new Money(feeTaxByDC, pIdC);
                        detail.tax.Amt = System.Math.Abs(feeTaxByDCMoney.amount.DecValue);
                        detail.tax.AmtSide = GetCrDrEnum(feeTaxByDCMoney, pEntityPartyId, pActorPartyId, pIsClearer);
                        // FI 20141208 [XXXXX] alimentation de AmtSideSpecified
                        detail.tax.AmtSideSpecified = (detail.tax.AmtSide > 0);

                    }
                    lst.Add(detail);
                }
            }
            pPayment.detailSpecified = ArrFunc.IsFilled(lst);
            if (pPayment.detailSpecified)
                pPayment.detail = lst.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPayment"></param>
        /// <param name="pCashFlow_FlowCur"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pProductBase"></param>
        /// FI 20120803 add SetFeeTax
        /// PM 20141218 ajout pIsCtrVal
        // EG 20180205 [23769] Add dbTransaction  
        private static void SetFeeTax(string pCS, IDbTransaction pDbTransaction, DetailedContractPayment pPayment,
            IEnumerable<CBDetCashFlows> pCashFlow_FlowCur, string pIdC, bool pIsCtrVal, string pPaymentType, IProductBase pProductBase)
        {
            IEnumerable<CBDetCashFlows> feeTax =
                from fee in pCashFlow_FlowCur
                where (fee.PaymentType == pPaymentType)
                && (fee.IdTaxSpecified == true)
                && (pIsCtrVal || (fee.CurrencyAmount.Find(match => match.Currency == pIdC) != null))
                select fee;
            //
            pPayment.taxSpecified = false;
            //
            List<int> lstIdTax = (from fee in feeTax select fee.IdTax).Distinct().ToList();
            //
            if (lstIdTax.Count() > 0)
            {
                pPayment.taxSpecified = true;
                pPayment.tax = new Tax[lstIdTax.Count()];
                //
                for (int i = 0; i < lstIdTax.Count(); i++)
                {
                    SQL_Tax sqlTax = new SQL_Tax(CSTools.SetCacheOn(pCS), lstIdTax[i])
                    {
                        DbTransaction = pDbTransaction
                    };
                    sqlTax.LoadTable(new string[] { "IDTAX,IDENTIFIER" });
                    //
                    pPayment.tax[i] = new Tax();
                    Tax taxGrp = pPayment.tax[i];
                    //
                    // Tax Source
                    taxGrp.taxSource.statusSpecified = false;
                    taxGrp.taxSource.spheresId = (SpheresId[])pProductBase.CreateSpheresId(2);
                    //
                    // Tax Group
                    taxGrp.taxSource.spheresId[0].scheme = Cst.OTCml_RepositoryTaxScheme;
                    taxGrp.taxSource.spheresId[0].OTCmlId = sqlTax.Id;
                    taxGrp.taxSource.spheresId[0].Value = sqlTax.Identifier;
                    //
                    // EventType
                    taxGrp.taxSource.spheresId[1].scheme = Cst.OTCml_RepositoryFeeEventTypeScheme;
                    taxGrp.taxSource.spheresId[1].Value = pPaymentType;
                    //
                    // Tax Details
                    List<int> lstIdTaxDet = (
                        from fee in feeTax
                        where (fee.IdTax == sqlTax.Id)
                        select fee.IdTaxDet).Distinct().ToList();
                    //
                    taxGrp.taxDetail = new TaxSchedule[lstIdTaxDet.Count()];
                    //
                    for (int j = 0; j < lstIdTaxDet.Count(); j++)
                    {
                        SQL_TaxDet sqlTaxDet = new SQL_TaxDet(CSTools.SetCacheOn(pCS), lstIdTaxDet[j])
                        {
                            DbTransaction = pDbTransaction
                        };
                        sqlTaxDet.LoadTable(new string[] { "IDTAX,IDTAXDET,IDENTIFIER,EVENTTYPE" });

                        //Somme des taxes dont le taux est sqlTaxDet.Id et la devise {pIdC} appliquées aux opp tel que PaymentType = {pPaymentType}
                        List<Money> listMoney;
                        if (pIsCtrVal)
                        {
                            listMoney =
                            (from moneyByIDC in
                                 // Cumuler les Taxes à l'image des frais
                                 (from fee in feeTax
                                  where (fee.IdTax == sqlTax.Id)
                                  && (fee.IdTaxDet == sqlTaxDet.Id)
                                  from money in fee.CtrValAmount
                                  where money.Currency == pIdC
                                  select money).GroupBy(money1 => money1.Currency)
                             select new Money((from money2 in moneyByIDC select money2.amount.DecValue).Sum(), pIdC)).ToList();
                        }
                        else
                        {
                            listMoney =
                            (from moneyByIDC in
                                 // Cumuler les Taxes à l'image des frais
                                 (from fee in feeTax
                                  where (fee.IdTax == sqlTax.Id)
                                  && (fee.IdTaxDet == sqlTaxDet.Id)
                                  && (fee.CurrencyAmount.Find(match => match.Currency == pIdC) != null)
                                  from money in fee.CurrencyAmount
                                  where money.Currency == pIdC
                                  select money).GroupBy(money1 => money1.Currency)
                             select new Money((from money2 in moneyByIDC select money2.amount.DecValue).Sum(), pIdC)).ToList();
                        }
                        //
                        taxGrp.taxDetail[j] = new TaxSchedule();
                        //
                        ITaxSchedule taxSchedule = taxGrp.taxDetail[j];
                        //
                        // Amount calculation
                        taxSchedule.TaxAmountSpecified = true;
                        taxSchedule.TaxAmount = taxSchedule.CreateTripleInvoiceAmounts;
                        taxSchedule.TaxAmount.Amount =
                            (Money)pProductBase.CreateMoney(System.Math.Abs(listMoney.First().Amount.DecValue), pIdC);
                        //
                        // Source Tax Details
                        taxGrp.taxDetail[j].taxSource = new SpheresSource();
                        ISpheresSource source = taxGrp.taxDetail[j].taxSource;
                        source.SpheresId = pProductBase.CreateSpheresId(5);
                        //
                        // Identifier TAXDET
                        source.SpheresId[0].Scheme = Cst.OTCml_RepositoryTaxDetailScheme;
                        source.SpheresId[0].OTCmlId = sqlTaxDet.Id;
                        source.SpheresId[0].Value = sqlTaxDet.Identifier;
                        //
                        // Country
                        source.SpheresId[1].Scheme = Cst.OTCml_RepositoryTaxDetailCountryScheme;
                        source.SpheresId[1].Value =
                            (from fee in feeTax
                             where (fee.IdTax == sqlTax.Id)
                             && (fee.IdTaxDet == sqlTaxDet.Id)
                             select fee.TaxCountry).First();
                        //
                        // Type
                        source.SpheresId[2].Scheme = Cst.OTCml_RepositoryTaxDetailTypeScheme;
                        source.SpheresId[2].Value =
                            (from fee in feeTax
                             where (fee.IdTax == sqlTax.Id)
                             && (fee.IdTaxDet == sqlTaxDet.Id)
                             select fee.TaxType).First();
                        //
                        // Rate
                        source.SpheresId[3].Scheme = Cst.OTCml_RepositoryTaxDetailRateScheme;
                        source.SpheresId[3].Value = StrFunc.FmtDecimalToInvariantCulture(
                            (from fee in feeTax
                             where (fee.IdTax == sqlTax.Id)
                             && (fee.IdTaxDet == sqlTaxDet.Id)
                             select fee.TaxRate).First()
                             );
                        //
                        // EventType
                        source.SpheresId[4].Scheme = Cst.OTCml_RepositoryTaxDetailEventTypeScheme;
                        source.SpheresId[4].Value = sqlTaxDet.EventType;
                    }
                }
            }
        }

        /// <summary>
        /// Construction d'un objet MarginConstituent
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20140908 [20066][20185] New
        /// PM 20150616 [21124] Add pIsCtrVal
        private static MarginConstituent GetMarginConstituent(CBFlows pFlows, string pIdC, bool pIsCtrVal, string pEntityId, string pActorId,
            bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate, CBCache pCacheData)
        {
            MarginConstituent ret = new MarginConstituent();
            //
            if (pFlows != null)
            {
                #region montant global
                Money moneyIdC = pFlows.CalcSumAmount(pIdC, pIsCtrVal);
                if (moneyIdC != null)
                {
                    ret.globalAmountSpecified = true;
                    ret.globalAmount = GetCashPosition(moneyIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate);
                }
                #endregion
                #region montant future
                //List<Money> futMoneyList = (
                //    from flow in pFlows.CashFlows
                //    where ((flow.Category.HasValue) && (flow.Category.Value == CfiCodeCategoryEnum.Future))
                //    from money in flow.CurrencyAmount
                //    where (money != null) && (money.Currency == pIdC)
                //    group money by money.Currency
                //        into moneyByIDC
                //        select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                //    ).ToList();

                // Prendre les flux Futures
                IEnumerable<CBDetCashFlows> futFlows = pFlows.CashFlows.Where(f => (f.Category.HasValue && (f.Category.Value == CfiCodeCategoryEnum.Future)));
                Money futMoneyIdC = CBFlowTools.CalcSumAmount(futFlows, pIdC, pIsCtrVal);
                if ((futMoneyIdC != null) && (futMoneyIdC.Amount.DecValue != 0))
                {
                    ret.futureSpecified = true;
                    ret.future = GetCashPosition(futMoneyIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
                }
                #endregion
                #region montants option
                List<OptionMarginConstituent> optionMarginList = new List<OptionMarginConstituent>();
                OptionMarginConstituent optMtMMargin = GetOptionMarginConstituent(pFlows, pIdC, pIsCtrVal, FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate);
                if (optMtMMargin != null)
                {
                    optionMarginList.Add(optMtMMargin);
                }
                OptionMarginConstituent optPremMargin = GetOptionMarginConstituent(pFlows, pIdC, pIsCtrVal, FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate);
                if (optPremMargin != null)
                {
                    optionMarginList.Add(optPremMargin);
                }
                ret.optionSpecified = (optionMarginList.Count > 0);
                if (ret.optionSpecified)
                {
                    ret.option = optionMarginList.ToArray();
                }
                #endregion
                #region montant autres (non ETD)
                // PM 20150616 [21124] Ajout détail par AssetCategory pour les montants OTC
                //List<Money> otherMoneyList = (
                //    from flow in pFlows.CashFlows
                //    where (flow.Category.HasValue == false)
                //    from money in flow.CurrencyAmount
                //    where (money != null) && (money.Currency == pIdC)
                //    group money by money.Currency
                //        into moneyByIDC
                //        select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                //    ).ToList();
                //Money otherMoneyIdC = GetMoneyIdC(otherMoneyList, pIdC);
                //if (otherMoneyIdC.Amount.DecValue != 0)
                //{
                //    ret.otherSpecified = true;
                //    ret.other = GetCashPosition(otherMoneyIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
                //}
                ret.other = GetOTCMarginConstituent(pFlows, pIdC, pIsCtrVal, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
                ret.otherSpecified = ((ret.other != null) && (ret.other.Count() > 0));
                #endregion
                #region détail des montants
                ret.detail = GetContractAmounts(pFlows, pIdC, false, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
                ret.detailSpecified = ((ret.detail != null) && (ret.detail.Count() > 0));
                #endregion
            }
            //
            return ret;
        }
        /// <summary>
        /// Construction d'un objet OptionMarginConstituent
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pValuationMethod"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// PM 20140908 [20066][20185] New
        /// PM 20150616 [21124] Add parameter pIsCtrVal
        private static OptionMarginConstituent GetOptionMarginConstituent(CBFlows pFlows, string pIdC, bool pIsCtrVal, FuturesValuationMethodEnum pValuationMethod,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate)
        {
            OptionMarginConstituent ret = null;
            if (pFlows != null)
            {
                // PM 20150616 [21124] Add parameter pIsCtrVal
                //List<Money> optMoney = (
                //    from flow in pFlows.CashFlows
                //    where (flow.Category.HasValue && (flow.Category.Value == CfiCodeCategoryEnum.Option)
                //    && flow.FutValuationMethod.HasValue && (flow.FutValuationMethod.Value == pValuationMethod))
                //    from money in flow.CurrencyAmount
                //    where (money != null) && (money.Currency == pIdC)
                //    group money by money.Currency
                //        into moneyByIDC
                //        select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                //    ).ToList();

                // Prendre les flux Options
                IEnumerable<CBDetCashFlows> optFlows = pFlows.CashFlows.Where(f => (f.Category.HasValue && (f.Category.Value == CfiCodeCategoryEnum.Option)
                    && f.FutValuationMethod.HasValue && (f.FutValuationMethod.Value == pValuationMethod)));
                Money optIdC = CBFlowTools.CalcSumAmount(optFlows, pIdC, pIsCtrVal);
                if ((optIdC != null) && (optIdC.Amount.DecValue != 0))
                {
                    CashPosition optCash = GetCashPosition(optIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate);
                    ret = new OptionMarginConstituent(optCash)
                    {
                        valuationMethod = pValuationMethod
                    };
                }
            }
            return ret;
        }

        /// <summary>
        /// Construction d'un array d'objets OTCMarginConstituent
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        private static AssetMarginConstituent[] GetOTCMarginConstituent(CBFlows pFlows, string pIdC, bool pIsCtrVal,
            string pEntityId, string pActorId, bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate)
        {
            List<AssetMarginConstituent> ret = new List<AssetMarginConstituent>();
            if (pFlows != null)
            {
                // Prendre les flux OTC
                IEnumerable<CBDetCashFlows> otcFlows = pFlows.CashFlows.Where(f => f.Category.HasValue == false);
                // Prendre les flux en devise pIdC ou en Contrevaleur
                // RD 20151112 [21551] a.CtrValAmount est une liste vide (non null) pour les montants à zéro.
                //IEnumerable<CBDetCashFlows> cashFlows = (pIsCtrVal ? otcFlows.Where( a => a.CtrValAmount != default) :
                IEnumerable<CBDetCashFlows> cashFlows = (pIsCtrVal ? otcFlows.Where(a => ArrFunc.IsFilled(a.CtrValAmount)) :
                    from flow in otcFlows
                    from flowMoney in flow.CurrencyAmount
                    where (flowMoney != null) && (flowMoney.Currency == pIdC)
                    select flow);

                // S'il existe des flux
                if (cashFlows.Count() > 0)
                {
                    // Construction du montant par AssetCategory
                    var moneyByAssetCategory =
                        from flow in cashFlows
                        group flow by flow.AssetCategory
                            into groupedFlow
                            select new
                            {
                                AssetCategory = groupedFlow.Key,
                                Amount = new Money(
                                    (pIsCtrVal
                                    ? (from curFlow in groupedFlow
                                       from money in curFlow.CtrValAmount
                                       select money.amount.DecValue).Sum()
                                    : (from curFlow in groupedFlow
                                       from money in curFlow.CurrencyAmount
                                       select money.amount.DecValue).Sum()),
                                    (pIsCtrVal ? groupedFlow.FirstOrDefault().CtrValAmount.FirstOrDefault().Currency : pIdC)
                                    ),
                            };

                    foreach (var id in moneyByAssetCategory)
                    {
                        if (id.Amount.Amount.DecValue != 0)
                        {
                            CashPosition optCash = GetCashPosition(id.Amount, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate);
                            AssetMarginConstituent otcAmount = new AssetMarginConstituent(optCash)
                            {
                                assetCategorySpecified = id.AssetCategory.HasValue
                            };
                            if (otcAmount.assetCategorySpecified)
                                otcAmount.assetCategory = id.AssetCategory.Value;
                            ret.Add(otcAmount);
                        }
                    }
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Construction d'un objet ContractAmount
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] Add parameter pIsCtrVal
        private static ContractAmount[] GetContractAmounts(IEnumerable<CBDetCashFlows> pFlows, string pIdC, bool pIsCtrVal, string pEntityId, string pActorId, bool pIsZeroToPayeByEntity, CBCache pCacheData)
        {
            //Detail des montants par DerivativeContract ou Asset
            List<ContractAmount> lstOfContractAmount = new List<ContractAmount>();
            // Recherche de la somme des montants des flux pour la devise pIdC par AssetCategory et IdDC ou IdAsset
            //var flowById = from flow in pFlows
            //           from flowMoney in flow.CurrencyAmount
            //           where (flowMoney.Currency == pIdC)
            //           group flow by new
            //           {
            //               flow.AssetCategory,
            //               OTCmlId = ((flow.IdDC != 0) ? flow.IdDC : flow.IdAsset),
            //           }
            //               into groupedFlow
            //               select new
            //               {
            //                   AssetCategory = groupedFlow.Key.AssetCategory,
            //                   OTCmlId = groupedFlow.Key.OTCmlId,
            //                   Amount = (from curFlow in groupedFlow
            //                             from money in curFlow.CurrencyAmount
            //                             select money.amount.DecValue).Sum(),
            //               };

            // Prendre les flux en devise pIdC ou en Contrevaleur
            // RD 20151105 [21527] a.CtrValAmount est une liste vide (non null) pour les montants à zéro.
            //IEnumerable<CBDetCashFlows> cashFlows = (pIsCtrVal ? pFlows.Where( a => a.CtrValAmount != default) :
            IEnumerable<CBDetCashFlows> cashFlows = (pIsCtrVal ? pFlows.Where(a => ArrFunc.IsFilled(a.CtrValAmount)) :
                from flow in pFlows
                from flowMoney in flow.CurrencyAmount
                where (flowMoney != null) && (flowMoney.Currency == pIdC)
                select flow);

            // S'il existe des flux
            if (cashFlows.Count() > 0)
            {
                // Grouper les flux par AssetCategory et Id (IdDC ou IdAsset)
                var flowById = from flow in cashFlows
                               group flow by new
                               {
                                   flow.AssetCategory,
                                   OTCmlId = ((flow.IdDC != 0) ? flow.IdDC : flow.IdAsset),
                               }
                                   into groupedFlow
                                   select new
                                   {
                                       groupedFlow.Key.AssetCategory,
                                       groupedFlow.Key.OTCmlId,
                                       Flows = groupedFlow,
                                       Amount = (pIsCtrVal
                                       ? (from curFlow in groupedFlow
                                          from money in curFlow.CtrValAmount
                                          select money.amount.DecValue).Sum()
                                       : (from curFlow in groupedFlow
                                          from money in curFlow.CurrencyAmount
                                          select money.amount.DecValue).Sum()),
                                       IdC = (pIsCtrVal ? groupedFlow.FirstOrDefault().CtrValAmount.FirstOrDefault().Currency : pIdC),
                                   };

                foreach (var id in flowById)
                {
                    if (id.Amount != 0)
                    {
                        Money moneyAmount = new Money(id.Amount, id.IdC);
                        ContractAmount detail = new ContractAmount();
                        //PM 20150324 [POC] AssetCategory devient nullable
                        if (id.AssetCategory.HasValue)
                        {
                            if (null != pCacheData)
                            {
                                //PM 20150324 [POC] AssetCategory devient nullable
                                //CBAssetInfo assetInfo = pCacheData.GetAssetInfo(id.AssetCategory, id.OTCmlId);
                                CBAssetInfo assetInfo = pCacheData.GetContractAssetInfo(id.AssetCategory.Value, id.OTCmlId);
                                if (null != assetInfo)
                                {
                                    detail.OTCmlId = id.OTCmlId;
                                    detail.otcmlIdSpecified = true;
                                    detail.Sym = assetInfo.Sym;
                                    detail.SymSpecified = StrFunc.IsFilled(assetInfo.Sym);
                                    detail.Exch = assetInfo.Exch;
                                    detail.ExchSpecified = StrFunc.IsFilled(assetInfo.Exch);
                                }
                            }
                            //PM 20150324 [POC] AssetCategory devient nullable
                            //detail.assetCategory = id.AssetCategory;
                            detail.assetCategory = id.AssetCategory.Value;
                            detail.assetCategorySpecified = true;
                        }
                        detail.Amt = System.Math.Abs(moneyAmount.amount.DecValue);
                        detail.AmtSide = GetCrDrEnum(moneyAmount, pEntityId, pActorId, pIsZeroToPayeByEntity);
                        // FI 20141208 [XXXXX] alimentation de AmtSideSpecified
                        detail.AmtSideSpecified = (detail.Amt > 0);
                        //
                        lstOfContractAmount.Add(detail);
                    }
                }
            }
            return lstOfContractAmount.ToArray();
        }
        /// <summary>
        /// Construction d'un objet ContractAmount
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] Add parameter pIsCtrVal
        private static ContractAmount[] GetContractAmounts(CBFlows pFlows, string pIdC, bool pIsCtrVal, string pEntityId, string pActorId, bool pIsZeroToPayeByEntity, CBCache pCacheData)
        {
            if (pFlows != null)
            {
                return GetContractAmounts(pFlows.CashFlows, pIdC, pIsCtrVal, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Construction d'un objet OptionLiquidatingValue
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdc"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        ///PM 20140908 [20066][20185] New
        private static OptionLiquidatingValue GetOptionLiquidatingValue(CBFlows pFlows, string pIdC, string pEntityId, string pActorId,
            bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate)
        {
            OptionLiquidatingValue ret = new OptionLiquidatingValue();
            if (pFlows != null)
            {
                // Montant Global
                GetCashPosition(pFlows.CalcSumMoneyIdC(pIdC), pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, ret);
                //
                #region Long Option Value
                // PM 20150409 [POC] Ajout des LOV des options OTC
                //List<Money> optLongMoney = (
                //        from flow in pFlows.CashFlows
                //        where (flow.Category.HasValue && (flow.Category.Value == CfiCodeCategoryEnum.Option)
                //              && flow.Side.HasValue && (flow.Side.Value == SideEnum.Buy))
                //        from money in flow.CurrencyAmount
                //        where (money != null) && (money.Currency == pIdC)
                //        group money by money.Currency into moneyIdC
                //        select new Money( moneyIdC.Sum( m => m.Amount.DecValue ), moneyIdC.Key)
                //        ).ToList();
                List<Money> optLongMoney = (
                        from flow in pFlows.CashFlows
                        where ((flow.CategorySpecified && (flow.Category.Value == CfiCodeCategoryEnum.Option))
                            || (flow.CategorySpecified == false)) // OTC
                              && flow.Side.HasValue && (flow.Side.Value == SideEnum.Buy)
                        from money in flow.CurrencyAmount
                        where (money != null) && (money.Currency == pIdC)
                        group money by money.Currency into moneyIdC
                        select new Money(moneyIdC.Sum(m => m.Amount.DecValue), moneyIdC.Key)
                        ).ToList();

                if (optLongMoney.Count > 0)
                {
                    Money optLongIdC = GetMoneyIdC(optLongMoney, pIdC);
                    ret.longOptionValue = GetCashPosition(optLongIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
                    ret.longOptionValueSpecified = true;
                }
                #endregion
                #region Short Option Value
                // PM 20150409 [POC] Ajout des LOV des options OTC
                //List<Money> optShortMoney = (
                //        from flow in pFlows.CashFlows
                //        where (flow.Category.HasValue && (flow.Category.Value == CfiCodeCategoryEnum.Option)
                //            && flow.Side.HasValue && (flow.Side.Value == SideEnum.Sell))
                //        from money in flow.CurrencyAmount
                //        where (money != null) && (money.Currency == pIdC)
                //        group money by money.Currency into moneyIdC
                //        select new Money(moneyIdC.Sum(m => m.Amount.DecValue), moneyIdC.Key)
                //        ).ToList();
                List<Money> optShortMoney = (
                        from flow in pFlows.CashFlows
                        where ((flow.CategorySpecified && (flow.Category.Value == CfiCodeCategoryEnum.Option))
                            || (flow.CategorySpecified == false)) // OTC
                            && flow.Side.HasValue && (flow.Side.Value == SideEnum.Sell)
                        from money in flow.CurrencyAmount
                        where (money != null) && (money.Currency == pIdC)
                        group money by money.Currency into moneyIdC
                        select new Money(moneyIdC.Sum(m => m.Amount.DecValue), moneyIdC.Key)
                        ).ToList();

                if (optShortMoney.Count > 0)
                {
                    Money optShortIdC = GetMoneyIdC(optShortMoney, pIdC);
                    ret.shortOptionValue = GetCashPosition(optShortIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, null);
                    ret.shortOptionValueSpecified = true;
                }
                #endregion
            }
            return ret;
        }
        /// <summary>
        /// Construction d'un objet DetailedCashPosition
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20140910 [20066][20185] New
        private static DetailedCashPosition GetDetailedCashPosition(CBFlows pFlows, string pIdC, string pEntityId, string pActorId,
            bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate, CBCache pCacheData)
        {
            return GetDetailedCashPosition(pFlows, pIdC, false, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, false, pCacheData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <param name="pIsWithDateDetail"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] Add parameter pIsWithDateDetail
        /// PM 20150616 [21124] Add parameter pIsCtrVal
        private static DetailedCashPosition GetDetailedCashPosition(CBFlows pFlows, string pIdC, bool pIsCtrVal, string pEntityId, string pActorId,
            bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate, bool pIsWithDateDetail, CBCache pCacheData)
        {
            DetailedCashPosition ret = new DetailedCashPosition();
            //
            if (pFlows != null)
            {
                // Montant Global
                // PM 20150616 [21124] Gestion de la contrevaleur
                //GetCashPosition(pFlows.CalcSumMoneyIdC(pIdC), pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, ret);
                Money sumAmount = (pIsCtrVal ? pFlows.CalcSumMoneyCtrVal() : pFlows.CalcSumMoneyIdC(pIdC));
                GetCashPosition(sumAmount, pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, ret);
                //
                // PM 20150616 [21124] Alimentation de dateDetail
                if (pIsWithDateDetail)
                {
                    // Prendre les flux en devise pIdC ou en Contrevaleur
                    IEnumerable<CBDetCashFlows> cashFlows = (pIsCtrVal ? pFlows.CashFlows : pFlows.CashFlowsByCurrency(pIdC));

                    if (cashFlows.Count() > 0)
                    {
                        // Construire des DetailedDateAmount par date de flux
                        ret.dateDetail = (
                            from flow in cashFlows
                            group flow by flow.DtValue
                                into groupedFlow
                                select new DetailedDateAmount
                                {
                                    ValueDate = groupedFlow.Key,
                                }).ToArray();
                        ret.dateDetailSpecified = ((ret.detail != null) && (ret.detail.Count() > 0));

                        // Alimenter les autres membre de chaque DetailedDateAmount
                        foreach (DetailedDateAmount dateAmount in ret.dateDetail)
                        {
                            IEnumerable<CBDetCashFlows> flows = cashFlows.Where(f => f.DtValue == dateAmount.ValueDate);
                            IEnumerable<Money> moneyToSum;
                            if (pIsCtrVal)
                            {
                                moneyToSum = from curFlow in flows
                                             where curFlow.CtrValAmount != default
                                             from money in curFlow.CtrValAmount
                                             select money;
                            }
                            else
                            {
                                moneyToSum = from curFlow in flows
                                             from money in curFlow.CurrencyAmount
                                             select money;
                            }
                            Money moneyAmount = CBFlowTools.CalcSumByIdC(moneyToSum).FirstOrDefault();
                            //
                            // RD 20171107 [23541] Add test
                            //dateAmount.Amt = System.Math.Abs(moneyAmount.amount.DecValue);
                            if (moneyAmount != null)
                                dateAmount.Amt = System.Math.Abs(moneyAmount.amount.DecValue);
                            else
                                dateAmount.Amt = 0;

                            dateAmount.AmtSideSpecified = (dateAmount.Amt > 0);
                            if (dateAmount.AmtSideSpecified)
                            {
                                dateAmount.AmtSide = GetCrDrEnum(moneyAmount, pEntityId, pActorId, pIsZeroToPayeByEntity);
                            }
                            //
                            dateAmount.detail = GetContractAmounts(flows, pIdC, pIsCtrVal, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
                            dateAmount.detailSpecified = ((dateAmount.detail != null) && (dateAmount.detail.Count() > 0));
                        }
                    }
                }
                else
                {
                    #region détail des montants
                    ret.detail = GetContractAmounts(pFlows, pIdC, pIsCtrVal, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
                    ret.detailSpecified = ((ret.detail != null) && (ret.detail.Count() > 0));
                    #endregion
                }
            }
            //
            return ret;
        }
        /// <summary>
        /// Construction d'un objet CashBalancePayment
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDateHRef"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        ///PM 20140912 [20066][20185] New
        private static CashBalancePayment GetCashBalancePayment(CBFlows pFlows, string pIdC, string pEntityId, string pActorId,
             bool pIsZeroToPayeByEntity, string pDateHRef, Nullable<DateTime> pDate)
        {
            CashBalancePayment ret = new CashBalancePayment();
            if (pFlows != null)
            {
                // Montant Global
                GetCashPosition(pFlows.CalcSumMoneyIdC(pIdC), pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, (CashPosition)ret);
                // Deposit
                GetCashPosition(pFlows.CalcSumNegativeMoneyIdC(pIdC), pEntityId, pActorId, pIsZeroToPayeByEntity, pDateHRef, pDate, (CashPosition)ret.cashDeposit);
                ret.cashDepositSpecified = (ret.cashDeposit.amount.Amount.DecValue != 0);
                //Withdrawal
                GetCashPosition(pFlows.CalcSumPositiveMoneyIdC(pIdC), pEntityId, pActorId, !pIsZeroToPayeByEntity, pDateHRef, pDate, (CashPosition)ret.cashWithdrawal);
                ret.cashWithdrawalSpecified = (ret.cashWithdrawal.amount.Amount.DecValue != 0);
            }
            return ret;
        }

        /// <summary>
        /// Construction d'un ContractSimplePaymentConstituent
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pActorId"></param>
        /// <param name="pIsZeroToPayeByEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pBusinessCenters"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        //PM 201850319 [POC] New
        private static ContractSimplePaymentConstituent GetContractSimplePaymentConstituent(
            CBFlows pFlows, string pIdC, string pEntityId, string pActorId, bool pIsZeroToPayeByEntity,
            DateTime pDtBusiness, IBusinessCenters pBusinessCenters, CBCache pCacheData)
        {
            Money amount = pFlows.CalcSumMoneyIdC(pIdC);
            SimplePayment payment = GetSimplePayment(amount, pEntityId, pActorId, pIsZeroToPayeByEntity, pDtBusiness, pBusinessCenters);
            ContractSimplePaymentConstituent ret = new ContractSimplePaymentConstituent(payment);

            #region détail des montants
            ret.detail = GetContractAmounts(pFlows, pIdC, false, pEntityId, pActorId, pIsZeroToPayeByEntity, pCacheData);
            ret.detailSpecified = ((ret.detail != null) && (ret.detail.Count() > 0));
            #endregion
            #region montants option
            List<OptionMarginConstituent> optionMarginList = new List<OptionMarginConstituent>();
            OptionMarginConstituent optMtMMargin = GetOptionMarginConstituent(pFlows, pIdC, false, FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket, pEntityId, pActorId, pIsZeroToPayeByEntity, null, pDtBusiness);
            if (optMtMMargin != null)
            {
                optionMarginList.Add(optMtMMargin);
            }
            OptionMarginConstituent optPremMargin = GetOptionMarginConstituent(pFlows, pIdC, false, FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle, pEntityId, pActorId, pIsZeroToPayeByEntity, null, pDtBusiness);
            if (optPremMargin != null)
            {
                optionMarginList.Add(optPremMargin);
            }
            ret.optionSpecified = (optionMarginList.Count > 0);
            if (ret.optionSpecified)
            {
                ret.option = optionMarginList.ToArray();
            }
            #endregion
            #region montant autres (non ETD)
            List<Money> otherMoneyList = (
                from flow in pFlows.CashFlows
                where (flow.Category.HasValue == false)
                from money in flow.CurrencyAmount
                where (money != null) && (money.Currency == pIdC)
                group money by money.Currency
                    into moneyByIDC
                    select new Money(moneyByIDC.Sum(m => m.amount.DecValue), moneyByIDC.Key)
                ).ToList();
            Money otherMoneyIdC = GetMoneyIdC(otherMoneyList, pIdC);
            if (otherMoneyIdC.Amount.DecValue != 0)
            {
                ret.otherSpecified = true;
                ret.other = GetCashPosition(otherMoneyIdC, pEntityId, pActorId, pIsZeroToPayeByEntity, null, pDtBusiness);
            }
            #endregion
            //
            return ret;
        }


        /// <summary>
        /// Retourne le détail des collateraux disponibles sur une devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdC">Devise</param>
        /// <param name="entityPartyId">Entité</param>
        /// <param name="actorPartyId">Acteur</param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pCacheData"></param>
        /// <returns></returns>
        /// FI 20160530 [21885] add Method
        // EG 20180205 [23769] Add dbTransaction  
        private static PosCollateral[] GetPosCollateral(string pCS, IDbTransaction pDbTransaction,
            string pIdC, string entityPartyId, string actorPartyId, CBTradeInfo pTradeInfo, DataDocumentContainer pDataDoc, CBCache pCacheData)
        {
            PosCollateral[] ret = null;

            IEnumerable<CBDetCollateral> collateralDet =
                               from item in pTradeInfo.FlowCollateral.Where( x=> x.CollatAvailable_FlowCur.Exists( y=> y.Currency == pIdC))       
                               where item.CollateralCategory != CollateralCategoryEnum.AvailableCash
                               && item.IsAllowed_AtLeastOneCSS 
                               select item;

            if (collateralDet.Count() > 0)
            {
                var collateralDetIdC = (from item in collateralDet
                                        from flow in item.CollatAvailable_FlowCur
                                        where flow.Currency == pIdC
                                        select item);

                if (collateralDetIdC.Count() > 0)
                {
                    ret = new PosCollateral[collateralDetIdC.Count()];
                    int i = 0;
                    foreach (CBDetCollateral collateralDetItem in collateralDetIdC)
                    {
                        ret[i] = new PosCollateral();
                        PosCollateral posCollateral = ret[i];
                        //
                        posCollateral.OTCmlId = collateralDetItem.idPoscollateral;

                        /*
                        //ACTOR
                        SQL_Actor  actor = new SQL_Actor(CSTools.SetCacheOn(pCS), collateralDetItem.IDA);
                        posCollateral.actorId.Value = actor.Identifier;
                        posCollateral.actorId.actorIdScheme = null;   // pas de scheme (cet attribut est non obligatoire) => cela allège la flux et facilite la lecture du flux XML
                        posCollateral.actorId.OTCmlId = actor.Id;
                        posCollateral.actorId.actorNameSpecified = false;
                        */

                        //Book
                        SQL_Book book = new SQL_Book(CSTools.SetCacheOn(pCS), collateralDetItem.IDB)
                        {
                            DbTransaction = pDbTransaction
                        };
                        posCollateral.bookId.Value  = book.Identifier;
                        posCollateral.bookId.bookIdScheme = null;   // pas de scheme (cet attribut est non obligatoire) => cela allège la flux et facilite la lecture du flux XML
                        posCollateral.bookId.OTCmlId = book.Id;
                        posCollateral.bookId.bookNameSpecified = false;

                        //Asset
                        Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), collateralDetItem.AssetCategory);
                        if (assetCategory == Cst.UnderlyingAsset.ExchangeTradedContract)
                            throw new InvalidOperationException(StrFunc.AppendFormat("Asset Category (Id:{0}) not valid for collateral", assetCategory));

                        CBAssetInfo assetInfo = pCacheData.GetContractAssetInfo(assetCategory, collateralDetItem.IdAsset);
                        if (null == assetInfo)
                            throw new NullReferenceException(StrFunc.AppendFormat("assetInfo is null for asset (Id:{0} ;Category:{1}", collateralDetItem.IdAsset, assetCategory));

                        posCollateral.asset = new ContractAsset()
                        {
                            otcmlId = assetInfo.OTCmlId.ToString(),
                            otcmlIdSpecified = true,
                            assetCategory = assetInfo.AssetCategory,
                            assetCategorySpecified = true,
                            Sym = assetInfo.Sym,
                            SymSpecified = StrFunc.IsFilled(assetInfo.Sym)
                        };

                        //MarketValue
                        decimal amont =
                                (from flow in collateralDetItem.CollatAvailable_FlowCur
                                 select flow.Amount).Sum(x => x.DecValue);

                        // Logique inversée => Le déposant paye == Le déposant crédite son compte 
                        CrDrEnum side = GetCrDrEnum(new Money(amont, pIdC), entityPartyId, actorPartyId, pTradeInfo.IsClearer);
                        if (CrDrEnum.CR == side)
                            side = CrDrEnum.DR;
                        else
                            side = CrDrEnum.CR;

                        posCollateral.valuation = new PosCollateralValuation()
                        {
                            OTCmlId = collateralDetItem.idPoscollateralVal,
                            Amt = System.Math.Abs(amont),
                            AmtSideSpecified = (amont != decimal.Zero),
                            AmtSide = side,
                            //AmtSide = GetCrDrEnum(new Money(amont, pIdC), entityPartyId, actorPartyId, pTradeInfo.IsClearer),
                            QtySpecified = collateralDetItem.Qty.HasValue,
                            Qty = collateralDetItem.Qty ?? 0
                        };

                        var collateralDetIdCCssHaircut = from item in collateralDetItem.collateralEnv
                                                         where item.IsCollateralAllowed
                                                         select new
                                                         {
                                                             item.CssId,
                                                             item.Haircut
                                                         };
                        //Haircut
                        posCollateral.haircut = new CssValue[collateralDetIdCCssHaircut.Count()];
                        int j = 0;
                        foreach (var haircut in collateralDetIdCCssHaircut)
                        {
                            string cssHRef = string.Empty;
                            if (haircut.CssId.HasValue)
                            {
                                SQL_Actor sqlActorCss = new SQL_Actor(CSTools.SetCacheOn(pCS), haircut.CssId.Value)
                                {
                                    DbTransaction = pDbTransaction
                                };
                                sqlActorCss.LoadTable();
                                pDataDoc.AddParty(sqlActorCss);
                                cssHRef = pDataDoc.GetParty(haircut.CssId.Value.ToString(), PartyInfoEnum.OTCmlId).Id;
                            }

                            posCollateral.haircut[j] = new CssValue()
                            {
                                cssHrefSpecified = StrFunc.IsFilled(cssHRef),
                                cssHref = StrFunc.IsFilled(cssHRef) ? cssHRef : string.Empty,
                                Value = haircut.Haircut.HasValue ? haircut.Haircut.Value / 100 : decimal.Zero
                            };
                            j++;
                        }
                        i++;

                        //si le collateral s'applique à une seule chambre et que l'instruction retenue n'est pas spécifique à une chambre
                        //on stipule ici que le collateral s'applique unique ment à cette chambre. 
                        if ((collateralDetItem.Ida_CssSpecified) && (false == posCollateral.haircut[0].cssHrefSpecified))
                        {
                            SQL_Actor sqlActorCss = new SQL_Actor(CSTools.SetCacheOn(pCS), collateralDetItem.Ida_Css)
                            {
                                DbTransaction = pDbTransaction
                            };
                            sqlActorCss.LoadTable();
                            pDataDoc.AddParty(sqlActorCss);
                            string cssHRef = pDataDoc.GetParty(collateralDetItem.Ida_Css.ToString(), PartyInfoEnum.OTCmlId).Id;
                            posCollateral.haircut[0].cssHrefSpecified = true;
                            posCollateral.haircut[0].cssHref = cssHRef;
                        }
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataDocA"></param>
        /// <param name="pDataDocB"></param>
        /// <returns></returns>
        // PM 20190701 [24761] New
        public static bool IsSameDataDocument(DataDocumentContainer pDataDocA, DataDocumentContainer pDataDocB)
        {
            EfsML.v30.CashBalance.CashBalance cashBalanceA = (EfsML.v30.CashBalance.CashBalance)pDataDocA.CurrentProduct.Product;
            EfsML.v30.CashBalance.CashBalance cashBalanceB = (EfsML.v30.CashBalance.CashBalance)pDataDocB.CurrentProduct.Product;
            //
            // Cash Balance
            bool isSame = (cashBalanceA.cashBalanceOfficePartyReference.href == cashBalanceB.cashBalanceOfficePartyReference.href);
            isSame = isSame && (cashBalanceA.entityPartyReference.href == cashBalanceB.entityPartyReference.href);
            isSame = isSame && (cashBalanceA.timing == cashBalanceB.timing);
            //
            // Cash Balance - Fx rate
            isSame = isSame && (cashBalanceA.fxRateSpecified == cashBalanceB.fxRateSpecified);
            if (isSame && cashBalanceA.fxRateSpecified && cashBalanceB.fxRateSpecified)
            {
                isSame = CashBalanceComparer.IdentifiedFxRateComparer.ArrayEquals(cashBalanceA.fxRate, cashBalanceB.fxRate);
            }
            //
            // Cash Balance - Settings
            isSame = isSame && CashBalanceComparer.CashBalanceSettingsComparer.Equals(cashBalanceA.settings, cashBalanceB.settings);
            //
            // Cash Balance - End Of Day Status
            isSame = isSame && (cashBalanceA.endOfDayStatusSpecified == cashBalanceB.endOfDayStatusSpecified);
            if (isSame && cashBalanceA.endOfDayStatusSpecified && cashBalanceB.endOfDayStatusSpecified)
            {
                isSame = (cashBalanceA.endOfDayStatus == cashBalanceB.endOfDayStatus) || ((cashBalanceA.endOfDayStatus != default(EndOfDayStatus)) && (cashBalanceB.endOfDayStatus != default(EndOfDayStatus)));
                if (isSame && (cashBalanceA.endOfDayStatus != default(EndOfDayStatus)) && (cashBalanceB.endOfDayStatus != default(EndOfDayStatus)))
                {
                    isSame = CashBalanceComparer.CssCustodianStatusComparer.ArrayEquals(cashBalanceA.endOfDayStatus.cssCustodianStatus, cashBalanceB.endOfDayStatus.cssCustodianStatus);
                }
            }
            // Cash Balance - Exchange Stream
            isSame = isSame && (cashBalanceA.exchangeCashBalanceStreamSpecified == cashBalanceB.exchangeCashBalanceStreamSpecified);
            if (isSame && cashBalanceA.exchangeCashBalanceStreamSpecified && cashBalanceB.exchangeCashBalanceStreamSpecified)
            {
                isSame = CashBalanceComparer.ExchangeCashBalanceStreamComparer.Equals(cashBalanceA.exchangeCashBalanceStream, cashBalanceB.exchangeCashBalanceStream);
            }
            //
            // Cash Balance - Streams
            // PM 20201202 [25592] Correction test
            isSame = isSame && CashBalanceComparer.CashBalanceStreamComparer.ArrayEquals(cashBalanceA.cashBalanceStream, cashBalanceB.cashBalanceStream);
            //
            return isSame;
        }
    }
}