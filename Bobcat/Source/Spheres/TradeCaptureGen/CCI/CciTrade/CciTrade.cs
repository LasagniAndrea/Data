#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Book;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Enum.MiFIDII_Extended;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Concerne les trades de marché
    /// </summary>
    // EG 20180514 [23812] Report
    public class CciTrade : CciTradeBase
    {

        /// <summary>
        /// Liste de marché. 
        /// <para>Pour l'instant seul le 1er élément est renseigné.Il contient le Venue (la plateforme)</para>  
        /// </summary>
        /// FI 20170928 [23452] Add cciMarket
        public CciMarketParty[] cciMarket;

        /// <summary>
        ///  Liste des frais
        /// </summary>
        /// FI 20170928 [23452] Add cciMarket
        public CciPayment[] cciOtherPartyPayment;

        /// <summary>
        /// 
        /// </summary>
        public CciNettingInformation cciNettingInformation;

        /// <summary>
        /// 
        /// </summary>
        public CciSettlementInput[] cciSettlementInput;

        /// <summary>
        /// 
        /// </summary>
        public CciPositionCancelation cciPositionCancelation;

        /// <summary>
        /// class managing the common controls for the position transfer
        /// </summary>
        public CciPositionTransfer cciPositionTransfer;

        /// <summary>
        /// class managing the common controls for the exercise/assignation/abandon 
        /// </summary>
        // EG 20151102 [21465] New cciDenOption instead of cciExeAssAbnOption 
        public CciDenOption cciDenOption;

        /// <summary>
        /// class managing the common controls for the remove allocation
        /// </summary>
        public CciRemoveAllocation cciRemoveAllocation;

        /// <summary>
        /// class managing the common controls for the underlying delivery
        /// </summary>
        public CciUnderlyingDelivery cciUnderlyingDelivery;

        public CciTradeFxOptionEarlyTermination cciTradeFxOptionEarlyTermination; 
        /// <summary>
        /// Obtient TradeInput
        /// </summary>
        public TradeInput TradeInput
        {
            get { return (TradeInput)TradeCommonInput; }
        }


        #region CciMarketParty
        // EG 20171031 [23509] New
        public override CciMarketParty CciFacilityParty
        {
            get { return cciMarket[0]; }
        }
        #endregion CciMarketParty

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add
        public int MarketLength
        {
            get { return cciMarket.Length; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add
        public int OtherPartyPaymentLength
        {
            get { return ArrFunc.IsFilled(cciOtherPartyPayment) ? cciOtherPartyPayment.Length : 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SettlementInputLength
        {
            get { return ArrFunc.IsFilled(cciSettlementInput) ? cciSettlementInput.Length : 0; }
        }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCcis"></param>
        /// EG 20180514 [23812] Report
        public CciTrade(TradeCustomCaptureInfos pCcis)
            : base(pCcis)
        {

            TradeInput tradeInput = (TradeInput)Ccis.TradeCommonInput;
            // FI 20180619 [XXXXX] Il y a tjs une plateforme sur les trades de marché
            // => Par défaut initialisation de cciMarket avec 1 item
            // => nécessaire lors de l'appel de la méthode IsControlModeConsultPositionTransferSpecific
            cciMarket = new CciMarketParty[] { new CciMarketParty(this, 1, "tradeHeader" + CustomObject.KEY_SEPARATOR) };

            if (null != tradeInput.positionCancel)
                cciPositionCancelation = new CciPositionCancelation(this, tradeInput.positionCancel, TradeCustomCaptureInfos.CCst.Prefix_correctionOfQuantity);

            if (null != tradeInput.tradeDenOption)
                cciDenOption = new CciDenOption(this, tradeInput.tradeDenOption, TradeCustomCaptureInfos.CCst.Prefix_denOption);

            if (null != tradeInput.positionTransfer)
                cciPositionTransfer = new CciPositionTransfer(this, tradeInput.positionTransfer, TradeCustomCaptureInfos.CCst.Prefix_PositionTransfer);

            if (null != tradeInput.removeAllocation)
                cciRemoveAllocation = new CciRemoveAllocation(this, tradeInput.removeAllocation, TradeCustomCaptureInfos.CCst.Prefix_RemoveAllocation);

            if (null != tradeInput.underlyingDelivery)
                cciUnderlyingDelivery = new CciUnderlyingDelivery(this, tradeInput.underlyingDelivery, TradeCustomCaptureInfos.CCst.Prefix_underlyingDelivery);

            if (null != tradeInput.fxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination = new CciTradeFxOptionEarlyTermination(this, tradeInput.fxOptionEarlyTermination, "FxOptionEarlyTermination");

            //Initialize(PrefixHeader);
        }
        #endregion constructor



        /// <summary>
        /// 
        /// </summary>
        /// FI 20171003 [23464] Add Method
        protected override void LastDump_ToDocument()
        {
            DataDocument.UpdateDecisionMaker(CSCacheOn);
            base.LastDump_ToDocument();
        }

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].AddCciSystem();

            for (int i = 0; i < OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].AddCciSystem();

            if (null != cciNettingInformation)
                cciNettingInformation.AddCciSystem();

            for (int i = 0; i < SettlementInputLength; i++)
                cciSettlementInput[i].AddCciSystem();

            if (null != cciPositionCancelation)
                cciPositionCancelation.AddCciSystem();

            if (null != cciPositionTransfer)
                cciPositionTransfer.AddCciSystem();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.AddCciSystem();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.AddCciSystem();

            if (null != cciDenOption)
                cciDenOption.AddCciSystem();

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180514 [23812] Report
        public override void Initialize_FromCci()
        {
            base.Initialize_FromCci();

            InitializeMarket_FromCci();

            InitializeOtherPartyPayment_FromCci();

            InitializeNettingInformation_FromCci();

            InitializeSettlementInput_FromCci();

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.Initialize_FromCci();

            if (null != cciPositionCancelation)
                cciPositionCancelation.Initialize_FromCci();

            if (null != cciPositionTransfer)
                cciPositionTransfer.Initialize_FromCci();

            if (null != cciDenOption)
                cciDenOption.Initialize_FromCci();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.Initialize_FromCci();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.Initialize_FromCci();

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180514 [23812] Report
        public override void CleanUp()
        {
            base.CleanUp();

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.CleanUp();

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].CleanUp();

            CleanUpOtherPartyPayment();

            CleanUpSettlementInput();

            CleanUpNettingInfo();

            if (null != cciPositionCancelation)
                cciPositionCancelation.CleanUp();

            if (null != cciPositionTransfer)
                cciPositionTransfer.CleanUp();

            if (null != cciDenOption)
                cciDenOption.CleanUp();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.CleanUp();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.CleanUp();

        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20180514 [23812] Report
        public override void Dump_ToDocument()
        {
            // FI 20240416 [WI902] cciMarket[i].Dump_ToDocument() doit être exécuté avant base.Dump_ToDocument()
            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].Dump_ToDocument();


            base.Dump_ToDocument();

            
            //OtherPartyPayment
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].Dump_ToDocument();
            CurrentTrade.OtherPartyPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.OtherPartyPaymentSpecified, cciOtherPartyPayment);

            //NettingInformation
            if (null != cciNettingInformation)
                cciNettingInformation.Dump_ToDocument();


            //SettlementInput
            for (int i = 0; i < this.SettlementInputLength; i++)
                cciSettlementInput[i].Dump_ToDocument();
            CurrentTrade.SettlementInputSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.SettlementInputSpecified, cciSettlementInput);

            if (null != cciTradeRemove)
                cciTradeRemove.Dump_ToDocument();

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.Dump_ToDocument();

            //
            if (null != cciPositionCancelation)
                cciPositionCancelation.Dump_ToDocument();
            //
            if (null != cciPositionTransfer)
                cciPositionTransfer.Dump_ToDocument();

            // EG 20151102 [21465]
            if (null != cciDenOption)
                cciDenOption.Dump_ToDocument();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.Dump_ToDocument();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.Dump_ToDocument();


        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].Initialize_Document();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180514 [23812] Report
        public override void Initialize_FromDocument()
        {
            base.Initialize_FromDocument();

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].Initialize_FromDocument();

            for (int i = 0; i < OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].Initialize_FromDocument();

            if (null != cciNettingInformation)
                cciNettingInformation.Initialize_FromDocument();

            for (int i = 0; i < SettlementInputLength; i++)
                cciSettlementInput[i].Initialize_FromDocument();

            if (null != cciPositionCancelation)
                cciPositionCancelation.Initialize_FromDocument();

            if (null != cciPositionTransfer)
                cciPositionTransfer.Initialize_FromDocument();

            if (null != cciDenOption)
                cciDenOption.Initialize_FromDocument();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.Initialize_FromDocument();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.Initialize_FromDocument();

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.Initialize_FromDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20171128 [XXXXX] Modify
        // EG 20180514 [23812] Report
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].ProcessInitialize(pCci);

            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                cciOtherPartyPayment[i].ProcessInitialize(pCci);
            }

            if (null != cciNettingInformation)
                cciNettingInformation.ProcessInitialize(pCci);

            for (int i = 0; i < SettlementInputLength; i++)
                cciSettlementInput[i].ProcessInitialize(pCci);

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.ProcessInitialize(pCci);

            if (null != cciPositionCancelation)
                cciPositionCancelation.ProcessInitialize(pCci);

            if (null != cciPositionTransfer)
                cciPositionTransfer.ProcessInitialize(pCci);

            if (null != cciDenOption)
                cciDenOption.ProcessInitialize(pCci);

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.ProcessInitialize(pCci);

            Boolean isInstrMiFir = BoolFunc.IsFalse(TradeInput.SQLInstrument.GetFirstRowColumnValue("ESMAMIFIREXEMPT"));
            if (isInstrMiFir)
            {
                // FI 20171128 [XXXXX] isInitMiFIR lors du chgt de sens (ajout test sur IsClientId_PayerOrReceiver)
                Boolean isInitMiFIR = ((cciParty.Where(x => (x.IsCci(CciTradeParty.CciEnum.actor, pCci) ||
                                                            x.IsCci(CciTradeParty.CciEnum.book, pCci))).Count() > 0)
                                       ||
                                        ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                                       );

                if (isInitMiFIR)
                {
                    string partyId = string.Empty;
                    CciTradeParty cciPartyItem = cciParty.Where(x => (x.IsCci(CciTradeParty.CciEnum.actor, pCci) ||
                                                                        x.IsCci(CciTradeParty.CciEnum.book, pCci))).FirstOrDefault();

                    if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                        cciPartyItem = cciParty.Where(x => x.GetPartyId(false) == pCci.NewValue).FirstOrDefault();

                    if (null != cciPartyItem)
                    {
                        partyId = cciPartyItem.GetPartyId();
                        if (StrFunc.IsFilled(partyId))
                            InitializeMiFIR(partyId);
                    }
                }

                ProcessInializeTraderSalesMiFIR(pCci);
            }
        }

        #region public override IsClientId_PayerOrReceiver
        // EG 20180514 [23812] Report
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = base.IsClientId_PayerOrReceiver(pCci);

            if ((!isOk) && (null != cciDenOption))
                isOk = cciDenOption.IsClientId_PayerOrReceiver(pCci);

            if ((!isOk) && (null != cciPositionCancelation))
                isOk = cciPositionCancelation.IsClientId_PayerOrReceiver(pCci);

            if ((!isOk) && (null != cciPositionTransfer))
                isOk = cciPositionTransfer.IsClientId_PayerOrReceiver(pCci);

            if ((!isOk) && (null != cciRemoveAllocation))
                isOk = cciRemoveAllocation.IsClientId_PayerOrReceiver(pCci);

            if (false == isOk)
                isOk = IsClientId_OtherPartyPaymentPayerReceiver(pCci);

            if (!isOk)
                isOk = IsClientId_SettlementInformationsPayerReceiver(pCci);

            if ((!isOk) && (null != cciTradeFxOptionEarlyTermination))
                isOk = cciTradeFxOptionEarlyTermination.IsClientId_PayerOrReceiver(pCci);

            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver

        /// <summary>
        /// 
        /// </summary>
        // EG 20180514 [23812] Report
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.RefreshCciEnabled();

            RefreshCciEnabledMiFiR();

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].RefreshCciEnabled();

            // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                cciOtherPartyPayment[i].RefreshCciEnabled();
                // Lectures des frais déjà facturés pour gestion Enabled/Disabled
                if (Cst.Capture.IsModeUpdateFeesUninvoiced(Ccis.CaptureMode))
                    CciTools.SetCciContainer(cciOtherPartyPayment[i], "CciEnumPayment", "IsEnabled",
                        (this.TradeCommonInput as TradeInput).PaymentIsUninvoiced(cciOtherPartyPayment[i].Payment as IPayment));
            }

            if (null != cciPositionCancelation)
                cciPositionCancelation.RefreshCciEnabled();

            if (null != cciPositionTransfer)
                cciPositionTransfer.RefreshCciEnabled();

            if (null != cciDenOption)
                cciDenOption.RefreshCciEnabled();

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.RefreshCciEnabled();

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.RefreshCciEnabled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20180514 [23812] Report
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].SetDisplay(pCci);

            for (int i = 0; i < OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].SetDisplay(pCci);

            for (int i = 0; i < SettlementInputLength; i++)
                cciSettlementInput[i].SetDisplay(pCci);

            if (null != cciPositionCancelation)
                cciPositionCancelation.SetDisplay(pCci);

            if (null != cciPositionTransfer)
                cciPositionTransfer.SetDisplay(pCci);

            if (null != cciDenOption)
                cciDenOption.SetDisplay(pCci);

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.SetDisplay(pCci);

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.SetDisplay(pCci);

            if (null != cciTradeFxOptionEarlyTermination)
                cciTradeFxOptionEarlyTermination.SetDisplay(pCci);

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pOccurs"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public override bool RemoveLastEmptyItemInDocumentArray(string pPrefix, int pOccurs, string pParentClientId, int pParentOccurs)
        {
            bool ret;

            if (TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment == pPrefix)
                ret = RemoveLastItemInOtherPartyPaymentArray(pPrefix, true);
            else
                ret = base.RemoveLastEmptyItemInDocumentArray(pPrefix, pOccurs, pParentClientId, pParentOccurs);

            if (null != cciPositionCancelation)
                cciPositionCancelation.RemoveLastItemInArray(pPrefix);

            if (null != cciPositionTransfer)
                cciPositionTransfer.RemoveLastItemInArray(pPrefix);

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.RemoveLastItemInArray(pPrefix);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public override void RemoveLastItemInArray(string pPrefix)
        {
            base.RemoveLastItemInArray(pPrefix);
            RemoveLastItemInOtherPartyPaymentArray(pPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            base.DumpSpecific_ToGUI(pPage);

            for (int i = 0; i < MarketLength; i++)
                cciMarket[i].DumpSpecific_ToGUI(pPage);

            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                // FI 20180427 [XXXXX] Appel systématique à DumpSpecific_ToGUI
                //if (cciOtherPartyPayment[i].IsSpecified)
                cciOtherPartyPayment[i].DumpSpecific_ToGUI(pPage);
            }

            // EG 20151102 [21465]
            if (cciDenOption != null)
                cciDenOption.DumpSpecific_ToGUI(pPage);

            if (null != cciPositionCancelation)
                cciPositionCancelation.DumpSpecific_ToGUI(pPage);

            if (null != cciPositionTransfer)
                cciPositionTransfer.DumpSpecific_ToGUI(pPage);

            if (null != cciRemoveAllocation)
                cciRemoveAllocation.DumpSpecific_ToGUI(pPage);

            if (null != cciUnderlyingDelivery)
                cciUnderlyingDelivery.DumpSpecific_ToGUI(pPage);

        }


        #region IContainerCciQuoteBasis Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = base.GetBaseCurrency(pCci);

            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    ret = cciOtherPartyPayment[i].GetBaseCurrency(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }

            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = base.GetCurrency1(pCci);

            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    ret = cciOtherPartyPayment[i].GetCurrency1(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }

            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = base.GetCurrency2(pCci);
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    ret = cciOtherPartyPayment[i].GetCurrency2(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            return ret;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool ret = base.IsClientId_QuoteBasis(pCci);
            if (ret == false)
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    ret = cciOtherPartyPayment[i].IsClientId_QuoteBasis(pCci);
                    if (ret)
                        break;
                }
            }

            return ret;
        }
        #endregion IContainerCciQuoteBasis Members

        #region IContainerCciPayerReceiver Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiverOtherPartyPayment(string pLastValue, string pNewValue)
        {
            base.SynchronizePayerReceiverOtherPartyPayment(pLastValue, pNewValue);
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion

        #region ITradeGetInfoButton Members
        #region SetButtonReferential
        // EG 20190730 Upd (TrdType for DebtSecurityTransaction)
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

            base.SetButtonReferential(pCci, pCo);

            #region SettlementInformations Css
            for (int i = 0; i < SettlementInputLength; i++)
            {
                if (cciSettlementInput[i].IsCci(CciSettlementInput.CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss, pCci))
                {
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = Cst.OTCml_TBL.CSS.ToString();
                    //
                    pCo.SqlColumn = "a1_IDENTIFIER";
                    pCo.ClientIdForSqlColumn = pCo.GetCtrlClientId(CustomObject.ControlEnum.textbox);
                    break;
                }
            }
            #endregion SettlementInformations Css

            #region NettingMethod
            if (null != cciNettingInformation)
            {
                if (cciNettingInformation.IsCci(CciNettingInformation.CciEnum.nettingDesignation, pCci))
                {
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = Cst.OTCml_TBL.NETDESIGNATION.ToString();
                    pCo.SqlColumn = "IDENTIFIER";
                    pCo.ClientIdForSqlColumn = pCo.GetCtrlClientId(CustomObject.ControlEnum.textbox);
                    //
                    int Ida_1 = GetActorIda(CciTradeParty.PartyType.party, 0);
                    int Ida_2 = GetActorIda(CciTradeParty.PartyType.party, 1);
                    //
                    pCo.Condition = "RestrictActor";
                    pCo.Param = new string[] { Ida_1.ToString(), Ida_2.ToString() };
                }

            }
            #endregion NettingMethod

            // FI 20180502 [23926] Add (Substitution de barème)
            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                if (cciOtherPartyPayment[i].IsCci(CciPayment.CciEnumPayment.paymentSource_feeSchedule, pCci))
                {
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = Cst.OTCml_TBL.FEESCHEDULE.ToString();
                    pCo.Fk = (null != pCci.Sql_Table) ? ((SQL_FeeSchedule)pCci.Sql_Table).IdFee.ToString() : null;
                    pCo.Condition = "TRADE_INPUT";

                    FeeRequest feeRequest = new FeeRequest(CSCacheOn, null, ((TradeInput)Ccis.Obj), IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));

                    List<StringDynamicData> lstSDD = new List<StringDynamicData>();

                    //DTREFERENCE
                    StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DTREFERENCE", DtFunc.DateTimeToStringDateISO(feeRequest.DtReference))
                    {
                        dataformat = DtFunc.FmtISODate
                    };
                    lstSDD.Add(dt);

                    //ACTION
                    StringDynamicData idPerMission = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "IDPERMISSION1", feeRequest.IdPermission[0].ToString());
                    lstSDD.Add(idPerMission);
                    string sIdPerMission2  = "N/A";
                    if (ArrFunc.Count(feeRequest.IdPermission)>=2)
                        sIdPerMission2 = feeRequest.IdPermission[1].ToString();
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "IDPERMISSION2", sIdPerMission2));


                    //IDSTBUSINESS
                    StringDynamicData stBusiness = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "IDSTBUSINESS", feeRequest.TradeInput.TradeStatus.stBusiness.NewSt);
                    lstSDD.Add(stBusiness);

                    //IDI
                    StringDynamicData idI = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI", feeRequest.TradeInput.Product.IdI.ToString());
                    lstSDD.Add(idI);
                    
                    //IDC & IDM
                    if (null != feeRequest.Contract)
                    {
                        StringDynamicData category = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "CONTRACTCATEGORY", feeRequest.Contract.First.ToString());
                        lstSDD.Add(category);
                        StringDynamicData idC = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDCONTRACT", feeRequest.Contract.Second.Id.ToString());
                        lstSDD.Add(idC);
                    }
                    if (null != feeRequest.SqlMarket)
                        lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDM", feeRequest.SqlMarket.Id.ToString()));

                    //IDI_UNL 
                    string sIdIUnl = "0";
                    if (feeRequest.Product.GetUnderlyingAssetIdI().HasValue)
                        sIdIUnl = feeRequest.Product.GetUnderlyingAssetIdI().Value.ToString();
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI_UNL", sIdIUnl));

                    //TRDTYPE (ETD et DST uniquement)
                    string trdType = "N/A";
                    if (feeRequest.TradeInput.Product.GetTrdType().HasValue)
                        trdType = feeRequest.TradeInput.Product.GetTrdType().Value.ToString();
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "TRDTYPE", trdType));

                    //TRADABLETYPE (COMS uniquement)
                    string tradableType = "N/A";
                    if (null != feeRequest.Contract && feeRequest.Contract.First == Cst.ContractCategory.CommodityContract)
                        tradableType = ((SQL_CommodityContract)feeRequest.Contract.Second).TradableType;
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "TRADABLETYPE", tradableType));

                    //ASSETCATEGORY (ETD uniquement)
                    string assetCategory = "N/A";
                    if (null != feeRequest.Contract && feeRequest.Contract.First == Cst.ContractCategory.DerivativeContract)
                        assetCategory = ((SQL_DerivativeContract)feeRequest.Contract.Second).AssetCategory;
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "ASSETCATEGORY", assetCategory));

                    int IDA_PAY = 0;
                    string hRef = ((IPayment)cciOtherPartyPayment[i].Payment).PayerPartyReference.HRef;
                    IParty party= null;
                    if (StrFunc.IsFilled(hRef))
                    {
                        party = this.DataDocument.GetParty(hRef, PartyInfoEnum.id);
                        //FI 20190122 [24286] add test if (null != party)
                        if (null != party)
                            IDA_PAY = party.OTCmlId;
                    }
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.integer.ToString() , "IDA_PAY", IDA_PAY.ToString()));

                    
                    int IDA_REC = 0;
                    hRef = ((IPayment)cciOtherPartyPayment[i].Payment).ReceiverPartyReference.HRef;
                    party = null;
                    if (StrFunc.IsFilled(hRef))
                    {
                        party = this.DataDocument.GetParty(hRef, PartyInfoEnum.id);
                        if (null != party)
                            IDA_REC = party.OTCmlId;
                    }
                    lstSDD.Add(new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDA_REC", IDA_REC.ToString()));


                    pCo.DynamicArgument = (from item in lstSDD
                                           select item.Serialize()).ToArray();
                    break;
                }
            }
        }
        #endregion SetButtonReferential
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = base.SetButtonZoom(pCci, pCo, ref  pIsSpecified, ref pIsEnabled);
            //

            if (false == isOk)
            {
                #region OtherPartyPayment
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    isOk = cciOtherPartyPayment[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                    if (isOk)
                    {
                        pCo.Object = "otherPartyPayment";
                        pCo.Element = "settlementInformation";
                        pCo.OccurenceValue = i + 1;
                        pIsSpecified = cciOtherPartyPayment[i].IsSettlementInfoSpecified;
                        pIsEnabled = cciOtherPartyPayment[i].IsSettlementInstructionSpecified;
                        break;
                    }
                }
                #endregion OtherPartyPayment
            }
            //			
            if (false == isOk)
            {
                #region SettlementInput
                for (int i = 0; i < SettlementInputLength; i++)
                {
                    isOk = cciSettlementInput[i].IsCci(CciSettlementInput.CciEnum.details, pCci);
                    if (isOk)
                    {
                        pCo.Object = "trade";
                        pCo.Element = "settlementInput";
                        pCo.OccurenceValue = i + 1;
                        pIsSpecified = CurrentTrade.SettlementInputSpecified;
                        pIsEnabled = true;
                        break;
                    }
                }
                #endregion
            }

            // EG 20151102 [21465]
            if ((false == isOk) && (null != cciDenOption))
                isOk = cciDenOption.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;


        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_OtherPartyPaymentPayerReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = base.IsClientId_OtherPartyPaymentPayerReceiver(pCci);
            if (false == isOk)
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    isOk = cciOtherPartyPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }

            if (false == isOk)
            {
                // EG 20151102 [21465]
                if (null != this.cciDenOption)
                {
                    for (int i = 0; i < ArrFunc.Count(cciDenOption.CciOtherPartyPayment); i++)
                    {
                        isOk = cciDenOption.CciOtherPartyPayment[i].IsClientId_PayerOrReceiver(pCci);
                        if (isOk)
                            break;
                    }
                }

            }

            return isOk;
        }


        /// <summary>
        /// 
        /// </summary>
        public override void SetClientIdDefaultReceiverOtherPartyPayment()
        {
            SetClientIdDefaultReceiverToOtherPartyPayment(cciOtherPartyPayment);
        }


        /// <summary>
        /// Initialize cciMarket
        /// </summary>
        /// FI 20170928 [23452] Add
        private void InitializeMarket_FromCci()
        {

            bool isOk = true;
            int index = -1;

            CciMarketParty cciPartyMarket;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                cciPartyMarket = new CciMarketParty(this, index + 1, "tradeHeader" + CustomObject.KEY_SEPARATOR);
                isOk = (Ccis.Contains(cciPartyMarket.CciClientId(CciMarketParty.CciEnum.executionDateTime)));
                if (isOk)
                    lst.Add(cciPartyMarket);
                else
                    isOk = false;
            }

            cciMarket = null;
            cciMarket = (CciMarketParty[])lst.ToArray(typeof(CciMarketParty));

            for (int i = 0; i < this.MarketLength; i++)
                cciMarket[i].Initialize_FromCci();

        }

        /// <summary>
        /// Modifie l'ordre des parties présentes dans le dataDocument
        /// <para>Dans l'ordre, cciTradeparty[0] puis cciTradeparty[1] puis les brokers puis les autres</para>
        /// </summary>
        /// FI 20170928 [23452] Modify
        public override void SetPartyInOrder()
        {

            Hashtable ht = new Hashtable();
            ArrayList al = new ArrayList();


            bool isStTemplateOrMissing = TradeCommonInput.TradeStatus.IsStEnvironment_Template
                        || TradeCommonInput.TradeStatus.IsStActivation_Missing;
            //
            for (int i = 0; i < PartyLength; i++)
            {
                bool addParty = cciParty[i].IsSpecified;
                if (false == addParty)
                {
                    // 20090921 RD / Pour ne pas vérifier la validité de l'acteur pour les templates
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Ajouter la Party si le Trade est un Template ou bien en statut Missing
                    addParty = isStTemplateOrMissing;
                }
                //
                if (addParty)
                {
                    string id = cciParty[i].GetPartyId(isStTemplateOrMissing);
                    if (false == ht.ContainsKey(id) && (null != DataDocument.GetParty(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }
            //
            for (int i = 0; i < BrokerLength; i++)
            {
                bool addParty = cciBroker[i].IsSpecified;
                if (false == addParty)
                {
                    // 20090921 FI / Pour ne pas vérifier la validité de l'acteur pour les templates
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Ajouter la Party si le Trade est un Template ou bien en statut Missing
                    addParty = isStTemplateOrMissing;
                }
                //
                if (addParty)
                {
                    string id = cciBroker[i].GetPartyId(isStTemplateOrMissing);
                    if (false == ht.ContainsKey(id) && (null != DataDocument.GetParty(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }
            //
            // FI 20170928 [23452] Add Market
            for (int i = 0; i < MarketLength; i++)
            {
                bool addParty = cciMarket[i].IsSpecified;
                if (false == addParty)
                {
                    addParty = isStTemplateOrMissing;
                }
                //
                if (addParty)
                {
                    string id = cciMarket[i].GetPartyId(isStTemplateOrMissing);
                    if (false == ht.ContainsKey(id) && (null != DataDocument.GetParty(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }



            //Ajout des parties présente dans le doc (partie et broker)
            if (ArrFunc.IsFilled(DataDocument.Party))
            {
                for (int i = 0; i < DataDocument.Party.Length; i++)
                {
                    string id = DataDocument.Party[i].Id;
                    //20090414 PL Gestion du cas où id == null
                    if ((id != null) && (!ht.ContainsKey(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }
            //
            DataDocument.Party = (IParty[])al.ToArray((DataDocument.DataDocument.GetTypeParty()));

        }

        /// <summary>
        /// Obtient le 1er trader avec rôle EXECUTION existant parmi les {pPartyTradeInformation} 
        /// <para></para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPartyTradeInformation"></param>
        /// <returns></returns>
        private static ITrader GetFirstTraderExecution(string pCS, IEnumerable<IPartyTradeInformation> pPartyTradeInformation)
        {
            ITrader ret = null;

            foreach (IPartyTradeInformation item in pPartyTradeInformation)
            {
                if (item.TraderSpecified)
                {
                    ITrader trader = item.Trader.Where(x =>
                                    ActorTools.IsActorWithRole(pCS, x.OTCmlId, RoleActor.EXECUTION)).FirstOrDefault();
                    if (null != trader)
                    {
                        ret = trader;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Pré-propostion des InvestmentDecision Within Firm et Execution Within Firm post saisi d'un trader ou d'un sales
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20171031 [23509] Upd
        private void ProcessInializeTraderSalesMiFIR(CustomCaptureInfo pCci)
        {

            #region Ecrans dans lesquels les gestionnaire(s) et autres intervenant(s) sont enfants des contreparties
            // Init si trader/sales est modifié (trader/sales de la party ou de l'intervenant( qui est enfant de la partie))
            // Si trader/sales de la party 1 => Init de la party1 si assujettie à MiFIR
            // Si trader/sales de la party 2 => Init de la party2 si assujettie à MiFIR
            Regex regex = new Regex(@"tradeHeader_party(\d)[_\w]*_(trader|sales)\d_identifier$");
            Match match = regex.Match(pCci.ClientId_WithoutPrefix);
            if (match.Success)
            {
                int partyNumber = IntFunc.IntValue(match.Groups[1].Value);
                IParty partyItem = cciParty[partyNumber - 1].GetParty();

                IEnumerable<IParty> partyMiFIR = this.TradeInput.GetPartyMiFIR(CSCacheOn);
                Boolean isEnabledMiFid = (partyItem != null) && (null != partyMiFIR) && (partyMiFIR.Where(x => (x.Id == partyItem.Id)).Count() > 0);

                if (isEnabledMiFid)
                    this.InitInvestmentDecisionExecutionWithinFirm(partyNumber, match.Groups[2].Value);
            }
            #endregion

            #region Ecrans dans dans lesquels les gestionnaire(s) et autres intervenant(s) sont indépendants
            // Init si trader est modifié  (trader de l'intervenant broker qui est entité)
            // => Dans ce cas Init des parties 1 et 2 si assujettie à MiFIR
            regex = new Regex(@"tradeHeader_broker(\d)_trader\d_identifier$");
            match = regex.Match(pCci.ClientId_WithoutPrefix);
            if (match.Success)
            {
                int brokerNumber = IntFunc.IntValue(match.Groups[1].Value);
                CciTradeParty cciBrokerItem = cciBroker[brokerNumber - 1];
                if (null == cciBrokerItem)
                    throw new NullReferenceException(StrFunc.AppendFormat("cciBroker (number:{0}) is null", brokerNumber.ToString()));

                IEnumerable<IParty> partyMiFIR = this.TradeInput.GetPartyMiFIR(CSCacheOn);
                if (null != partyMiFIR)
                {
                    for (int i = 0; i < ArrFunc.Count(cciParty); i++)
                    {
                        IParty partyItem = cciParty[i].GetParty();
                        Boolean isEnabledMiFid = (partyItem != null) && (partyMiFIR.Where(x => (x.Id == partyItem.Id)).Count() > 0);
                        if (isEnabledMiFid && (cciBrokerItem.GetActorIda() == BookTools.GetEntityBook(CSCacheOn, cciParty[i].GetBookIdB())))
                        {
                            this.InitInvestmentDecisionExecutionWithinFirm(i + 1, "trader");
                        }
                    }
                }
            }
            #endregion

        }

        /// <summary>
        ///  Initialisation des InvestmentDecision Within Firm et Execution Within Firm de la party 1 ou party2 post alimentation d'un trader ou d'un sales
        /// </summary>
        /// <param name="pPartyNumber">1 pour la 1er partie, 2 pour 2ème party</param>
        /// <param name="pTraderOrSales">valeurs attentues "trader" ou "sales" </param>
        private void InitInvestmentDecisionExecutionWithinFirm(int pPartyNumber, string _)
        {
            if ((pPartyNumber == 1) || (pPartyNumber == 2))
            {
                CciTradeParty cciParty = this.cciParty[pPartyNumber - 1];
                if (null == cciParty)
                    throw new NullReferenceException(StrFunc.AppendFormat("Party : {0} not exists", pPartyNumber.ToString()));

                CustomCaptureInfo cciInvest = cciParty.Cci(CciTradeParty.CciEnum.partyTradeInformation_investmentDecisionWithinFirm);
                if (null != cciInvest)
                    cciInvest.NewValue = string.Empty;

                CustomCaptureInfo cciExec = cciParty.Cci(CciTradeParty.CciEnum.partyTradeInformation_executionWithinFirm);
                if (null != cciExec)
                    cciExec.NewValue = string.Empty;


                IParty party = cciParty.GetParty();
                IPartyTradeInformation partyTradeInformation = cciParty.GetPartyTradeInformation();

                if (null != party && party.OTCmlId > 0 && null != partyTradeInformation)
                {
                    IPartyTradeInformation entitypartyTradeInformation = null;

                    Nullable<int> idB = DataDocument.GetOTCmlId_Book(party.Id);
                    Nullable<int> idAEntity = idB.HasValue ? BookTools.GetEntityBook(CSCacheOn, idB) : new Nullable<int>();
                    if (idAEntity.HasValue)
                    {
                        IParty partyEntity = DataDocument.GetParty(idAEntity.ToString(), PartyInfoEnum.OTCmlId);
                        if (null != partyEntity)
                            entitypartyTradeInformation = DataDocument.GetPartyTradeInformation(partyEntity.Id);
                    }

                    bool isClientUnderDecisionMaker = BookTools.IsCounterPartyClientUnderDecisionMaker(CSCacheOn, party.OTCmlId, idB);
                    bool isClient = BookTools.IsCounterPartyClient(CSCacheOn, party.OTCmlId, idB);

                    if (isClientUnderDecisionMaker)
                    {
                        if (null != cciInvest) // investmentDecisionWithinFirm
                        {
                            if (partyTradeInformation.SalesSpecified)
                            {
                                ITrader sales = partyTradeInformation.Sales.Where(x =>
                                                ActorTools.IsActorWithRole(CSCacheOn, x.OTCmlId, RoleActor.INVESTDECISION)).FirstOrDefault();

                                if (null != sales)
                                    cciInvest.NewValue = sales.Identifier;
                            }
                        }

                        if (null != cciExec) // executionWithinFirm
                        {
                            if (null != entitypartyTradeInformation)
                            {
                                if (entitypartyTradeInformation.TraderSpecified)
                                {
                                    ITrader trader = entitypartyTradeInformation.Trader.Where(x =>
                                                    ActorTools.IsActorWithRole(CSCacheOn, x.OTCmlId, RoleActor.EXECUTION)).FirstOrDefault();

                                    if (null != trader)
                                        cciExec.NewValue = trader.Identifier;
                                }
                            }
                        }
                    }
                    else if (isClient)
                    {
                        if (null != cciInvest) // investmentDecisionWithinFirm
                            cciInvest.NewValue = String.Empty;

                        if (null != cciExec) // executionWithinFirm
                        {
                            List<IPartyTradeInformation> lst = new List<IPartyTradeInformation>();
                            if (null != entitypartyTradeInformation)
                                lst.Add(entitypartyTradeInformation);
                            lst.Add(partyTradeInformation);

                            ITrader trader = GetFirstTraderExecution(CSCacheOn, lst);
                            if (null != trader)
                                cciExec.NewValue = trader.Identifier;
                        }
                    }
                    else // HOUSE
                    {
                        if (null != cciInvest) // investmentDecisionWithinFirm
                        {
                            if (partyTradeInformation.SalesSpecified)
                            {
                                ITrader sales = partyTradeInformation.Sales.Where(x =>
                                                ActorTools.IsActorWithRole(CSCacheOn, x.OTCmlId, RoleActor.INVESTDECISION)).FirstOrDefault();

                                if (null != sales)
                                    cciInvest.NewValue = sales.Identifier;
                            }
                        }
                        if (null != cciExec) // executionWithinFirm
                        {
                            List<IPartyTradeInformation> lst = new List<IPartyTradeInformation>();
                            if (null != entitypartyTradeInformation)
                                lst.Add(entitypartyTradeInformation);
                            lst.Add(partyTradeInformation);

                            ITrader trader = GetFirstTraderExecution(CSCacheOn, lst);
                            if (null != trader)
                                cciExec.NewValue = trader.Identifier;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CleanUpOtherPartyPayment()
        {
            if (ArrFunc.IsFilled(cciOtherPartyPayment))
            {
                for (int i = 0; i < cciOtherPartyPayment.Length; i++)
                {
                    cciOtherPartyPayment[i].CleanUp();
                }
            }

            if (ArrFunc.IsFilled(CurrentTrade.OtherPartyPayment))
            {
                for (int i = CurrentTrade.OtherPartyPayment.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(CurrentTrade.OtherPartyPayment[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(CurrentTrade, "otherPartyPayment", i);
                }
            }
            CurrentTrade.OtherPartyPaymentSpecified = ArrFunc.IsFilled(CurrentTrade.OtherPartyPayment);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compléments
        private void InitializeOtherPartyPayment_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified = CurrentTrade.OtherPartyPaymentSpecified;
            string clientIdDefaultReceiver = string.Empty;
            if (0 < cciParty.Length)
                clientIdDefaultReceiver = cciParty[0].CciClientId(CciTradeParty.CciEnum.actor); // Voir SetDefaultRec  			
            // FI 20190626 [XXXXX] Sur DebtSecurityTransaction et EquitySecurityTransaction 
            //Spheres alimente par défaut la date de payment de l'opp avec la date du GrossAmoun (Cette date pouvant elle même être initialisée à partir de Dtexcution + plus application d'un potentiel offset présent  sur le titre)
            string clientIdDefaultDate = string.Empty;
            if (DataDocument.CurrentProduct.IsDebtSecurityTransaction)
            {
                CciProductDebtSecurityTransaction cciProductDebtSecurity = (CciProductDebtSecurityTransaction)this.cciProduct;
                clientIdDefaultDate = cciProductDebtSecurity.CciGrossAmount.CciClientId(CciPayment.CciEnum.date); 
            }
            else if (DataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                CciProductEquitySecurityTransaction cciProductEquitySecurityTransaction = (CciProductEquitySecurityTransaction)this.cciProduct;
                clientIdDefaultDate = cciProductEquitySecurityTransaction.CciGrossAmount.CciClientId(CciPayment.CciEnum.date); 
            }

            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                //
                CciPayment otherPartyPayment = new CciPayment(this, index + 1, null, CciPayment.PaymentTypeEnum.Payment, TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment,
                    "Brokerage", clientIdDefaultReceiver, string.Empty, CciClientIdMainCurrency, clientIdDefaultDate);
                //
                isOk = Ccis.Contains(otherPartyPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(CurrentTrade.OtherPartyPayment) || (index == CurrentTrade.OtherPartyPayment.Length))
                        ReflectionTools.AddItemInArray(CurrentTrade, "otherPartyPayment", index);
                    otherPartyPayment.Payment = CurrentTrade.OtherPartyPayment[index];
                    //
                    lst.Add(otherPartyPayment);
                }
            }
            //
            cciOtherPartyPayment = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < this.OtherPartyPaymentLength; i++)
                cciOtherPartyPayment[i].Initialize_FromCci();
            //			
            CurrentTrade.OtherPartyPaymentSpecified = SaveSpecified;
            //

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <returns></returns>
        private bool RemoveLastItemInOtherPartyPaymentArray(string pPrefix)
        {
            return RemoveLastItemInOtherPartyPaymentArray(pPrefix, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pIsEmpty"></param>
        /// <returns></returns>
        private bool RemoveLastItemInOtherPartyPaymentArray(string pPrefix, bool pIsEmpty)
        {

            bool isOk = true;
            if (pPrefix == TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment)
            {
                int posArray = this.OtherPartyPaymentLength - 1;
                bool isToRemove = true;
                if (pIsEmpty)
                    isToRemove = StrFunc.IsEmpty(CurrentTrade.OtherPartyPayment[posArray].PayerPartyReference.HRef);
                //
                if (isToRemove)
                {
                    Ccis.RemoveCciOf(cciOtherPartyPayment[posArray]);
                    ReflectionTools.RemoveItemInArray(this, "cciOtherPartyPayments", posArray);
                    ReflectionTools.RemoveItemInArray(CurrentTrade, "otherPartyPayment", posArray);
                }
                else
                    isOk = false;
            }
            return isOk;

        }


        /// <summary>
        /// 
        /// </summary>
        private void InitializeNettingInformation_FromCci()
        {

            CciNettingInformation cciNetInfo = new CciNettingInformation(this, TradeCustomCaptureInfos.CCst.Prefix_nettingInfo, null);
            bool isOk = Ccis.Contains(cciNetInfo.CciClientId(CciNettingInformation.CciEnum.nettingMethod));
            //
            if (isOk)
            {
                cciNettingInformation = cciNetInfo;
                //
                if (false == CurrentTrade.NettingInformationInputSpecified)
                    CurrentTrade.NettingInformationInput = DataDocument.CurrentProduct.ProductBase.CreateNettingInformationInput();
                cciNettingInformation.NettingInformationInput = CurrentTrade.NettingInformationInput;
                cciNettingInformation.Initialize_FromCci();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void CleanUpSettlementInput()
        {

            if (ArrFunc.IsFilled(cciSettlementInput))
            {
                for (int i = 0; i < cciSettlementInput.Length; i++)
                    cciSettlementInput[i].CleanUp();
            }

            if (ArrFunc.IsFilled(CurrentTrade.SettlementInput))
            {
                for (int i = CurrentTrade.SettlementInput.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(CurrentTrade.SettlementInput[i]))
                        ReflectionTools.RemoveItemInArray(CurrentTrade, "settlementInput", i);
                }
            }
            CurrentTrade.SettlementInputSpecified = ArrFunc.IsFilled(CurrentTrade.SettlementInput);

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeSettlementInput_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified = CurrentTrade.SettlementInputSpecified;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                //
                CciSettlementInput cciSettlementInputItem = new CciSettlementInput(this, index + 1, TradeCustomCaptureInfos.CCst.Prefix_settlementInput, null);
                //
                isOk = Ccis.Contains(cciSettlementInputItem.CciClientId(CciSettlementInput.CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss));
                //
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(CurrentTrade.SettlementInput) || (index == CurrentTrade.SettlementInput.Length))
                        ReflectionTools.AddItemInArray(CurrentTrade, "settlementInput", index);
                    cciSettlementInputItem.SettlementInput = CurrentTrade.SettlementInput[index];
                    //
                    lst.Add(cciSettlementInputItem);
                }
            }
            //
            // Alimenations de settlementInformations
            cciSettlementInput = (CciSettlementInput[])lst.ToArray(typeof(CciSettlementInput));
            for (int i = 0; i < SettlementInputLength; i++)
                cciSettlementInput[i].Initialize_FromCci();
            //
            CurrentTrade.SettlementInputSpecified = SaveSpecified;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_SettlementInformationsPayerReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            for (int i = 0; i < SettlementInputLength; i++)
            {
                isOk = cciSettlementInput[i].IsClientId_PayerOrReceiver(pCci);
                if (isOk)
                    break;
            }
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        private void CleanUpNettingInfo()
        {
            if (CurrentTrade.NettingInformationInputSpecified)
                CurrentTrade.NettingInformationInputSpecified = CaptureTools.IsDocumentElementValid(CurrentTrade.NettingInformationInput);

        }

        /// <summary>
        /// Définit le clearer et/ou le Executing broker
        /// <para>Cette méthode s'applique uniquement sur les allocations</para>
        /// <param name="pbOnlyIfIsEmpty">si true, l'alimentation depuis CLEARINGTEMLATE se fait uniquement si la donnée n'est pas renseignée</param>
        /// </summary>
        /// FI 20140805 [XXXXX] Modify
        /// EG 20150331 [POC] FxLeg|FxOptionLeg
        public void SetCciClearerOrBrokerFromClearingTemplate(Boolean pbOnlyIfIsEmpty)
        {
            if (false == TradeCommonInput.IsAllocation)
                throw new NotSupportedException(StrFunc.AppendFormat("Method SetCciClearerOrBrokerFromClearingTemplate is not supported, this trade is not an allocation"));


            RptSideProductContainer rptSideContainer = this.TradeCommonInput.Product.RptSide();

            // FI 20140805 [XXXXX] 
            ClearingTemplates clearingTemplates = new ClearingTemplates();
            clearingTemplates.Load(CSCacheOn, TradeCommonInput.DataDocument, rptSideContainer, SQL_Table.ScanDataDtEnabledEnum.Yes);

            bool isOk = ArrFunc.IsFilled(clearingTemplates.clearingTemplate);
            if (isOk)
            {
                ClearingTemplate clearingTemplateFind = clearingTemplates.clearingTemplate[0];

                //CLEARER
                string cciClientId = cciParty[1].CciClientId(CciTradeParty.CciEnum.actor);
                Ccis.SetNewValue(cciClientId, clearingTemplateFind.clearerIdentifier, pbOnlyIfIsEmpty);
                cciClientId = cciParty[1].CciClientId(CciTradeParty.CciEnum.book);
                Ccis.SetNewValue(cciClientId, clearingTemplateFind.bookClearerIdentifier, pbOnlyIfIsEmpty);

                //BROKER
                if (StrFunc.IsFilled(clearingTemplateFind.brokerIdentifier) && ArrFunc.IsEmpty(cciParty[1].cciBroker))
                    throw new Exception("Collection ccis doesn't contains item for cciParty[1].cciBroker");

                if (StrFunc.IsFilled(clearingTemplateFind.brokerIdentifier))
                {
                    cciClientId = cciParty[1].cciBroker[0].CciClientId(CciTradeParty.CciEnum.actor);
                    if (false == Ccis.Contains(cciClientId))
                        throw new Exception(StrFunc.AppendFormat("Collection ccis doesn't contains {0}", cciClientId));

                    Ccis.SetNewValue(cciClientId, clearingTemplateFind.brokerIdentifier, pbOnlyIfIsEmpty);
                }
                if (StrFunc.IsFilled(clearingTemplateFind.bookBrokerIdentifier))
                {
                    cciClientId = cciParty[1].cciBroker[0].CciClientId(CciTradeParty.CciEnum.book);
                    if (false == Ccis.Contains(cciClientId))
                        throw new Exception(StrFunc.AppendFormat("Collection ccis doesn't contains {0}", cciClientId));

                    Ccis.SetNewValue(cciClientId, clearingTemplateFind.bookBrokerIdentifier, pbOnlyIfIsEmpty);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20171031 [23509] Upd 
        private void RefreshCciEnabledMiFiR()
        {
            IEnumerable<IParty> partyMiFIR = this.TradeInput.GetPartyMiFIR(CSCacheOn);
            foreach (CciTradeParty item in cciParty)
            {
                IParty partyItem = item.GetParty();

                Boolean isEnabledMiFid = false;
                if (null != partyMiFIR)
                    isEnabledMiFid = (partyItem != null) && (partyMiFIR.Where(x => (x.Id == partyItem.Id)).Count() > 0);

                foreach (CciTradeParty.CciEnum itemcci in CciTools.GetCciEnum<CciTradeParty.CciEnum>("MiFIR"))
                    Ccis.Set(item.CciClientId(itemcci), "IsEnabled", isEnabledMiFid);

                if (isEnabledMiFid)
                {
                    CustomCaptureInfo cci = null;
                    IEnumerable<string> countryOfEEA = CountryTools.GetCountryOfUnion(CSCacheOn, CountryTools.CountryUnion.EEA);

                    // Trading Capacity 
                    // Enabled si party assujetie à MiFIR

                    // Waiver Indicator 
                    // Enabled si party assujetie à MiFIR
                    //         si facility !=XXXX et  facility != XOFF et facility au sein de l'EEA)
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_tradingWaiver1);
                    if (null != cci)
                    {
                        IParty facility = DataDocument.Party.Where(x => DataDocumentContainer.IsPartyFacility(x)).FirstOrDefault();
                        cci.IsEnabled = (facility != null) && (facility.PartyId != MarketTools.XXXX) && (facility.PartyId != MarketTools.XOFF);
                        if (cci.IsEnabled)
                        {
                            SQL_Market sqlMarket = new SQL_Market(CSCacheOn, facility.OTCmlId);
                            if (false == sqlMarket.LoadTable(new string[] { "IDCOUNTRY" }))
                                throw new NullReferenceException(StrFunc.AppendFormat("Market (IdM:{0}) not found", facility.OTCmlId));
                            cci.IsEnabled = countryOfEEA.Contains(sqlMarket.IdCountry);
                        }
                    }

                    // Short selling indicator 
                    // Enabled si party assujetie à MiFIR
                    //         si seller
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_shortSale);
                    if (null != cci)
                        cci.IsEnabled = DataDocument.CurrentProduct.IsPartyBuyerOrSeller(partyItem, BuyerSellerEnum.SELLER);

                    // Commodity derivative indicator 
                    // Enabled si party assujetie à MiFIR
                    //         si sur les dérivés sur matières premières
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_isCommodityHedge);
                    if (null != cci)
                    {
                        cci.IsEnabled = false;
                        if (null != TradeInput.SQLInstrument.GetFirstRowColumnValue("ESMACLASSIFICATION"))
                        {
                            string colValue = TradeInput.SQLInstrument.GetFirstRowColumnValue("ESMACLASSIFICATION").ToString();

                            EsmaProductClassificationScheme value =
                                    ReflectionTools.ConvertStringToEnum<EsmaProductClassificationScheme>(colValue);

                            cci.IsEnabled = (value.ToString().StartsWith("Commodity_"));
                        }
                        else
                        {
                            TradeInput.DataDocument.CurrentProduct.GetDerivativeContract(CSCacheOn, null, out SQL_DerivativeContract sqlDc);
                            if (null != sqlDc)
                                cci.IsEnabled = (sqlDc.UnderlyingGroup == "C");
                        }
                    }

                    // Securities financing transaction indicator)
                    // Enabled si party assujetie à MiFIR
                    //         si INSTRUMENT.ESMASECURITIESFIN non reseigné
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_isSecuritiesFinancing);
                    if (null != cci)
                        cci.IsEnabled = (null == TradeInput.SQLInstrument.GetFirstRowColumnValue("ESMASECURITIESFIN"));


                    // OTC post-Trade indicateor 
                    // Enabled si party assujetie à MiFIR
                    //         si marché XXXX ou XOFF
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_otcClassification1);
                    if (null != cci && cci.IsEnabled)
                    {
                        IParty facility = partyMiFIR.Where(x => DataDocumentContainer.IsPartyFacility(x)).FirstOrDefault();
                        cci.IsEnabled = (facility != null) && ((facility.PartyId == MarketTools.XXXX) || (facility.PartyId == MarketTools.XOFF));
                    }
                }
            }

        }

        /// <summary>
        ///  Pré-propostion des indicateurs MiFIR post saisi d'un marché, d'un Acteur ou d'un book  ou d'un sens
        /// </summary>
        // EG 20171031 [23509] Upd
        private void InitializeMiFIR(string partyId)
        {
            ActorRoleCollection colRole = DataDocument.GetActorRole(CSCacheOn);
            IEnumerable<IParty> partyMiFIR = this.TradeInput.GetPartyMiFIR(CSCacheOn);

            foreach (CciTradeParty item in cciParty.Where (x=> !StrFunc.IsFilled(partyId) || x.GetPartyId()== partyId))
            {
                IParty partyItem = item.GetParty();

                //1er étape Purge des cci MiFIR
                IEnumerable<CciTradeParty.CciEnum> cciEnumMiFIR = CciTools.GetCciEnum<CciTradeParty.CciEnum>("MiFIR");
                foreach (CciTradeParty.CciEnum itemcci in cciEnumMiFIR)
                {
                    CustomCaptureInfo cci = item.Cci(itemcci);
                    if (null != cci)
                        cci.Reset();
                }

                //pré-propostions si la party est assujettie à déclaration 
                Boolean isEnabledMiFid = false;
                if (null != partyMiFIR)
                    isEnabledMiFid = (partyItem != null) && (partyMiFIR.Where(x => (x.Id == partyItem.Id)).Count() > 0);

                if (isEnabledMiFid)
                {
                    // Trading Capacity 
                    CustomCaptureInfo cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_tradingCapacity);
                    if (null != cci)
                    {
                        //Activité client => AOTC et Activité Maison => DEAL
                        if (colRole.IsActorRole(partyItem.OTCmlId, RoleActor.CLIENT))
                            cci.NewValue = TradingCapacityScheme.AOTC.ToString();
                        else if (colRole.IsActorRole(partyItem.OTCmlId, RoleActor.ENTITY))
                            cci.NewValue = TradingCapacityScheme.DEAL.ToString();
                    }

                    // Waiver Indicator
                    // Aucune pré-proposition => newValue = string.Empty
                    // Short selling indicator
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_shortSale);
                    if (null != cci)
                    {
                        if (DataDocument.IsPartyBuyerOrSeller(partyItem, BuyerSellerEnum.SELLER))
                            cci.NewValue = ShortSaleScheme.UNDI.ToString();
                    }


                    // Commodity derivative indicator
                    // Aucune pré-proposition => newValue = string.Empty

                    // Securities financing transaction indicator 
                    // Pre-proposition en fonction de ESMASECURITIESFIN
                    cci = item.Cci(CciTradeParty.CciEnum.partyTradeInformation_isSecuritiesFinancing);
                    if (null != cci)
                    {
                        object colValue = TradeInput.SQLInstrument.GetFirstRowColumnValue("ESMASECURITIESFIN");

                        if ((colValue == null) || BoolFunc.IsFalse(colValue))
                        {
                            cci.NewValue = "false";
                        }
                        else if (BoolFunc.IsTrue(colValue))
                        {
                            cci.NewValue = "true";
                        }
                    }

                    // OTC post-Trade indicator 
                    // Aucune pré-proposition => newValue = string.Empty
                }
            }
        }

        
    }
}

