#region Using Directives
using EFS.ACommon;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Interface;
using FpML.Interface;
using System;

#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciProductBuyAndSellBack
    /// <summary>
    /// Description résumée de CciTradeBuyAndSellBack.
    /// </summary>
    public class CciProductBuyAndSellBack : CciProductSaleAndRepurchaseAgreement
    {
        #region constructor
        public CciProductBuyAndSellBack(CciTrade pCciTrade, IBuyAndSellBack pBuyAndSellBack, string pPrefix)
            : this(pCciTrade, pBuyAndSellBack, pPrefix, -1) { }
        public CciProductBuyAndSellBack(CciTrade pCciTrade, IBuyAndSellBack pBuyAndSellBack, string pPrefix, int pNumber)
            : base(pCciTrade, (ISaleAndRepurchaseAgreement)pBuyAndSellBack, pPrefix, pNumber)
        {
          
        }
        #endregion constructor
        //
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
           
                base.ProcessInitialize(pCci);
                //
                for (int i = 0; i < SpotLegLength; i++)
                {
                    CciDebtSecurityTransaction cciDebtSecurityTransaction = CciSpotLeg[i].cciDebtSecurityTransaction;
                    //
                    if ((cciDebtSecurityTransaction.cciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci)) && (null != pCci.Sql_Table))
                    {
                        ISecurityAsset securityAsset = cciDebtSecurityTransaction.DebtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                        if (null != securityAsset)
                        {
                            DateTime date = new SecurityAssetContainer(securityAsset).CalcPaymentDate(CciTrade.CSCacheOn, CciTrade.DataDocument.TradeDate);
                            cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.date).NewValue = DtFunc.DateTimeToStringDateISO(date);
                        }
                    }
                }
                //
                // La formule est valable uniquement dans le cas ou on a:
                // 1 seul CashStream
                // 1 seul SpotLeg
                // 1 seul ForwardLeg
                //
                if (SpotLegLength == 1 && ForwardLegLength == 1 && CashStreamLength == 1)
                    Calc(pCci);
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciCashStream[0].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue.ToString()), true, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciCashStream[0].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), true, TypeData.TypeDataEnum.@string);
        }
        
        
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();
            //
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    if (CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_dayCountFraction, cci))
                    {
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.High);// Pour le Recalcul des grossAmount ( spot et Forward)
                    }
                }
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {
            base.SetProduct(pProduct);
        }

        //        
        #region private Calc
        /// <summary>
        /// Calcul de grossAmount du spotLeg , du grossAmount du forwardLeg , et du taux
        /// </summary>
        /// <param name="pCci"></param>
        private void Calc(CustomCaptureInfo pCci)
        {
            CciCompare ccic = GetCciToCalc(pCci);
            //
            if (null != ccic)
            {
                decimal result;
                switch (ccic.key)
                {
                    case "spotGrossAmount":
                        result = RepurchaseAgreementContainer.CalcSpotGrossAmount();
                        if ((result) > decimal.Zero)
                            CciSpotLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(result);
                        break;
                    case "forwardGrossAmount":
                        result = RepurchaseAgreementContainer.CalcForwardGrossAmount();
                        if ((result) > decimal.Zero)
                            CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(result);
                        break;
                    case "cashStreamRate":
                        result = RepurchaseAgreementContainer.CalcCashStreamRate();
                        if ((result) > decimal.Zero)
                            CciCashStream[0].Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue).NewValue = StrFunc.FmtDecimalToInvariantCulture(result);
                        break;
                }
            }
        }
        #endregion private Calc
        #region private GetCciToCalc
        /// <summary>
        /// Retoune le cciCompare à calculer
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        private CciCompare GetCciToCalc(CustomCaptureInfo pCci)
        {
            CciCompare ret = null;
            CciCompare[] ccic = null;
            //  
            if (CciCashStream[0].IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue, pCci) ||
                CciCashStream[0].IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_dayCountFraction, pCci) ||
                CciCashStream[0].IsCci(CciStream.CciEnum.calculationPeriodDates_terminationDate, pCci) ||
                CciCashStream[0].IsCci(CciStream.CciEnum.calculationPeriodDates_effectiveDate, pCci))
            {
                // A chaque modif du taux , du DCF, effectiveMinDate ou terminationMaxDate, si tout est déjà renseigné on calcule le grossAmount du forwardLeg
                // sinon la donnée non renseignée entre le grossAmount du forwardLeg et le grossAmount du spotLeg
                // 
                if (CaptureTools.IsInputFilled(RepurchaseAgreement))
                {
                    ccic = new CciCompare[] { new CciCompare("forwardGrossAmount", CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                }
                else
                {
                    ccic = new CciCompare[] {new CciCompare("forwardGrossAmount", CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1),
                                         new CciCompare("spotGrossAmount" ,CciSpotLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount),2)};
                }
            }
            else if (CciSpotLeg[0].cciDebtSecurityTransaction.cciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
            {
                // A chaque modif du grossAmount du spotLeg, si tout est déjà renseigné on calcule le grossAmount du forwardLeg
                // sinon la donnée non renseignée entre le grossAmount du forwardLeg et le taux
                // 
                if (CaptureTools.IsInputFilled(RepurchaseAgreement))
                {
                    ccic = new CciCompare[] { new CciCompare("forwardGrossAmount", CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                }
                else
                {
                    ccic = new CciCompare[] {new CciCompare("forwardGrossAmount", CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1),
                                         new CciCompare("cashStreamRate" ,CciCashStream[0].Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue),2)};
                }
            }
            else if (CciForwardLeg[0].cciDebtSecurityTransaction.cciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
            {
                // A chaque modif du grossAmount du forwardLeg, si tout est déjà renseigné on ne calcule rien
                // sinon la donnée non renseignée entre le taux et le grossAmount du spotLeg
                // 
                if (false == CaptureTools.IsInputFilled(RepurchaseAgreement))
                {
                    ccic = new CciCompare[] {new CciCompare("cashStreamRate", CciCashStream[0].Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue), 1),
                                         new CciCompare("spotGrossAmount" ,CciSpotLeg[0].cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.amount),2)};
                }
            }
            //
            if (ArrFunc.IsFilled(ccic))
            {
                Array.Sort(ccic);
                ret = ccic[0];
            }
            //
            return ret;

        }
        #endregion
    
        #endregion
    }
    #endregion
}
