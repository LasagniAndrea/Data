#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Ccis Trades de marché, Référentiel Titre, Trade Déposit
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customCaptureInfos", IsNullable = false)]
    public sealed class TradeCustomCaptureInfos : TradeCommonCustomCaptureInfos, ICloneable
    {
        #region CCst
        public new sealed class CCst
        {
            public CCst() { }
            #region Constants
            //A
            public const string Prefix_additionalPayment = "additionalPayment";
            //
            //B
            public const string Prefix_bulletPayment = "bulletPayment";
            public const string Prefix_bsb = "bsb";
            public const string Prefix_buyAndSellBack = "buyAndSellBack"; //vocation => doit Remplacer bsb
            // EG 20150410 [20513] BANCAPERTA
            public const string Prefix_bondOption = "bondOption";
            //C
            public const string Prefix_cancelableProvision = "cancelable";
            public const string Prefix_cashStream = "cashStream";
            public const string Prefix_capFloor = "capFloor";
            public const string Prefix_capFloorStream = "capFloorStream";
            public const string Prefix_classification = "classification";

            public const string Prefix_commoditySpot = "commoditySpot";
            public const string Prefix_correctionOfQuantity = "correctionOfQuantity";
            //
            //D
            public const string Prefix_debtSecurity = "debtSecurity";
            public const string Prefix_debtSecurityStream = "debtSecurityStream";
            public const string Prefix_debtSecurityTransaction = "debtSecurityTransaction";
            public const string Prefix_deliveryQuantity = "deliveryQuantity";
            public const string Prefix_deliveryConditions = "deliveryConditions";
            public const string Prefix_deliveryPeriods = "deliveryPeriods";
            //
            //E
            public const string Prefix_earlyTerminationProvision = "earlyTermination";
            public const string Prefix_electricityPhysicalLeg = "electricityPhysicalLeg";
            // EG 20221201 [25639] [WI484] New
            public const string Prefix_environmentalPhysicalLeg = "environmentalPhysicalLeg";
            public const string Prefix_environmentalProductionFeatures = "productionFeatures";
            public const string Prefix_environmentalProductionDevice = "device";
            public const string Prefix_endTime = "endTime";
            public const string Prefix_eqd = "eqd";
            public const string Prefix_eqOpt = "eqOpt";
            public const string Prefix_equityOption = "equityOption";//vocation => doit Remplacer eqOpt
            // EG 20140526 New
            //public const string Prefix_eqs = "eqs";
            //public const string Prefix_equitySwap = "equitySwap";//vocation => doit Remplacer eqs
            //public const string Prefix_equ = "equ";

            public const string Prefix_equityExercice = "equityExercise";
            public const string Prefix_equityPremium = "equityPremium";
            public const string Prefix_exchangedCurrency1 = "exchangedCurrency1";
            public const string Prefix_exchangedCurrency2 = "exchangedCurrency2";
            public const string Prefix_exchangeRate = "exchangeRate";
            public const string Prefix_exchangeTradedDerivative = "exchangeTradedDerivative";
            public const string Prefix_equitySecurityTransaction = "equitySecurityTransaction";            
            public const string Prefix_extendibleProvision = "extendible";
            //
            //F
            public const string Prefix_finalStub = "finalStub";
            public const string Prefix_fixedLeg = "fixedLeg";
            public const string Prefix_fixing = "fixing";
            public const string Prefix_fixTradeCaptureReport = "FIXML_TrdCapRpt";
            public const string Prefix_fixInstrument = "Instrmt";
            public const string Prefix_forwardLeg = "forwardLeg";
            public const string Prefix_fra = "fra";
            public const string Prefix_fx = "fx";
            public const string Prefix_fxSingleLeg = "fxSingleLeg";//vocation => doit Remplacer Prefix_fx
            public const string Prefix_fxAmericanTrigger = "fxAmericanTrigger";
            public const string Prefix_fxAveRateOpt = "fxAveRateOpt";
            public const string Prefix_fxAverageRateOption = "fxAverageRateOption";//vocation => doit Remplacer Prefix_fxAveRateOpt
            public const string Prefix_fxBarrier = "fxBarrier";
            public const string Prefix_fxBarOpt = "fxBarOpt";
            public const string Prefix_fxBarrierOption = "fxBarrierOption"; //vocation => doit Remplacer Prefix_fxBarOpt
            public const string Prefix_fxDigOpt = "fxDigOpt";
            public const string Prefix_fxDigitalOption = "fxDigitalOption";//vocation => doit Remplacer Prefix_fxDigOpt
            public const string Prefix_fxEuropeanTrigger = "fxEuropeanTrigger";
            public const string Prefix_fxOpt = "fxOpt";
            public const string Prefix_fxSimpleOption = "fxSimpleOption";//vocation => doit Remplacer Prefix_fxOpt
            public const string Prefix_fxOptionPremium = "fxOptionPremium";
            public const string Prefix_fxSwap = "fxSwap";
            //
            //G
            public const string Prefix_grossAmount = "grossAmount";
            public const string Prefix_gasPhysicalLeg = "gasPhysicalLeg";
            //            
            //I
            public const string Prefix_initialStub = "initialStub";
            public const string Prefix_instrumentId = "instrumentId";
            // EG 20140526 New
            //public const string Prefix_int = "int";
            public const string Prefix_interestLeg = "interestLeg";
            //
            //L
            public const string Prefix_loanDeposit = "loanDeposit";
            public const string Prefix_loanDepositStream = "loanDepositStream";
            public const string Prefix_localization = "localization";
            //
            public const string Prefix_marginRequirement = "marginRequirement";
            //N
            public const string Prefix_nettingInfo = "nettingInformation";
            //
            //O
            public const string Prefix_orderPrice = "orderPrice";
            public const string Prefix_orderQuantity = "orderQuantity";
            public const string Prefix_otherPartyPayment = "otherPartyPayment";
            //
            //P
            public const string Prefix_payment = "payment";
            public const string Prefix_physicalQuantity = "physicalQuantity";
            public const string Prefix_physicalLeg = "physicalLeg";
            public const string Prefix_premium = "premium";
            public const string Prefix_PositionTransfer = "positionTransfer";
            //
            //R
            public const string Prefix_rate = "rate";
            public const string Prefix_RemoveAllocation = "removeAllocation";
            public const string Prefix_repo = "repo";
            // EG 20140526 New
            public const string Prefix_returnSwap = "returnSwap";
            public const string Prefix_returnLeg = "returnLeg";
            public const string Prefix_RptSide = "RptSide";
            //
            //S
            public const string Prefix_security = "security";
            public const string Prefix_securityAsset = "securityAsset";
            public const string Prefix_settlementInput = "settlementInput";
            public const string Prefix_settlementPeriods = "settlementPeriods";

            public const string Prefix_singleUnderlyer = "singleUnderlyer";
            public const string Prefix_stubCalculationPeriodAmount = "stubCalculationPeriodAmount";
            public const string Prefix_spotLeg = "spotLeg";
            public const string Prefix_startTime = "startTime";
            public const string Prefix_strategy = "strategy";
            public const string Prefix_supplyStartTime = "supplyStartTime";
            public const string Prefix_supplyEndTime = "supplyEndTime";
            public const string Prefix_swap = "swap";
            public const string Prefix_swapStream = "swapStream";
            public const string Prefix_swaption = "swaption";
            public const string Prefix_swaptionPremium = "premium";
            public const string Prefix_swaptionExerciseProcedure = "exerciseProcedure";
            
            //
            //T
            public const string Prefix_tradeExtends = "tradeExtends";
            public const string Prefix_termDeposit = "termDeposit";

            // EG 20151102 [21465]
            public const string Prefix_denOption = "denOption";
            //
            //U
            public const string Prefix_underlyer = "underlyer";
            public const string Prefix_underlyingDelivery = "underlyingDelivery";
            //
            //
            //
            //
            //
            public const string CALCULATION_PERIOD_DATES_REFERENCE = "calculationPeriodDates";
            public const string RESET_DATES_REFERENCE = "resetDates";
            public const string PAYMENT_DATES_REFERENCE = "paymentDates";
            public const string EFFECTIVEDATE_REFERENCE = "effectiveDate";
            public const string TEMINATIONDATE_REFERENCE = "terminationDate";
            //
            public const string INITIALVALUE_REFERENCE = "initialValue";
            public const string NOTIONALSCHEDULE_REFERENCE = "notionalSchedule";
            public const string PRINCIPAL_REFERENCE = "principal";
            public const string EXCHANGECURRENCY1_REFERENCE = "exchangeCurrency1";
            public const string EXCHANGECURRENCY2_REFERENCE = "exchangeCurrency2";
            public const string NOTIONAL_REFERENCE = "notional";
            public const string CALLCURRENCYAMOUNT_REFERENCE = "callCurrencyAmount";
            public const string PUTCURRENCYAMOUNT_REFERENCE = "putCurrencyAmount";
            //
            public const string EFFECTIVE_BUSINESS_CENTERS_REFERENCE = "effectiveBusinessCenters";
            public const string TERMINATION_BUSINESS_CENTERS_REFERENCE = "terminationBusinessCenters";
            public const string FIRSTPERIODSTART_BUSINESS_CENTERS_REFERENCE = "firstPeriodStartBusinessCenters";
            public const string RESET_BUSINESS_CENTERS_REFERENCE = "resetBusinessCenters";
            public const string PERIOD_BUSINESS_CENTERS_REFERENCE = "periodBusinessCenters";
            public const string PAYMENT_BUSINESS_CENTERS_REFERENCE = "paymentBusinessCenters";
            //
            public const string EXPIRATION_BUSINESS_CENTERS_REFERENCE = "expirationBusinessCenters";
            public const string EXERCICE_BUSINESS_CENTERS_REFERENCE = "exerciceBusinessCenters";
            public const string COMMENCEMENT_BUSINESS_CENTERS_REFERENCE = "commencementBusinessCenters";
            #endregion Constants
        }
        #endregion CCst
        
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public CciTradeBase CciTrade
        {
            get { return (CciTradeBase)CciContainer; }
            set { CciContainer = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override TradeCommonInput TradeCommonInput
        {
            get { return (TradeCommonInput)Obj; }
        }
        #endregion Accessors
        //
        #region Constructor
        public TradeCustomCaptureInfos()
            : base()
        {
        }
        public TradeCustomCaptureInfos(string pCS, TradeCommonInput pTradeCommonInput) :
            this(pCS, pTradeCommonInput, null, string.Empty , true)
        {
        }
        public TradeCustomCaptureInfos(string pCS, TradeCommonInput pTradeCommonInput, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
            : base(pCS, pTradeCommonInput, pUser, pSessionId, pIsGetDefaultOnInitializeCci)
        {
        }
        #endregion Constructors
        //
        #region Methods
        #region CloneGlobalCci
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pCciSource"></param>
        /// <param name="pCciContainer"></param>
        public string CloneGlobalCci(string key, CustomCaptureInfo pCciSource, IContainerCci pCciContainer)
        {
            //Ex  S'il existe Irs_calculationPeriodDates_effectiveDate => 
            //    et que pCciContainer correspond à cciStream[1] alors un cci Irs1_calculationPeriodDates_effectiveDate sera généré
            //			
            string ret = string.Empty;
            if (false == Contains(pCciContainer.CciClientId(key)))
            {
                CustomCaptureInfo cci = (CustomCaptureInfo)pCciSource.Clone(CustomCaptureInfo.CloneMode.CciAttribut);
                cci.ClientId = pCciSource.ClientId_Prefix + pCciContainer.CciClientId(key);
                ret = cci.ClientId;
                Add(cci);
            }
            return ret; ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pCciSource"></param>
        /// <param name="pCciProduct"></param>
        /// <returns></returns>
        public string CloneGlobalCci(string key, CustomCaptureInfo pCciSource, CciProductBase pCciProduct)
        {
            string ret = string.Empty;
            //Ex  S'il existe fxopt=> 
            //    et que pCciContainer correspond à cciStream[1] alors un cci Irs1_calculationPeriodDates_effectiveDate sera généré
            //			
            if (false == Contains(pCciProduct.CciClientId(key)))
            {
                CustomCaptureInfo cci = (CustomCaptureInfo)pCciSource.Clone(CustomCaptureInfo.CloneMode.CciAttribut);
                cci.ClientId = pCciSource.ClientId_Prefix + pCciProduct.CciClientId(key);
                ret = cci.ClientId;
                //
                Add(cci);
            }
            return ret;
        }
        #endregion CloneGlobalCci
        //

        /// <summary>
        /// 1/ Déversement des CCI sur l'IHM
        /// 2/ Mise à Disabled de certains contrôles
        /// 3/ Reload de certaines DDL
        /// </summary>
        /// <param name="pPage"></param>
        public override void Dump_ToGUI(CciPageBase pPage)
        {
            base.Dump_ToGUI(pPage);
            CciTrade.DumpSpecific_ToGUI(pPage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        public override void InitializeCci( CustomCaptureInfo pCci, SQL_Table pSql_Table, string pData)
        {
            base.InitializeCci(pCci, pSql_Table, pData);
        }

        /// <summary>
        /// Synchronize les différents pointeurs du dataDocument existants dans les cciContainers  
        /// </summary>
        // EG 20100208 Add ProductContainer
        // EG 20171109 [23509] Upd Test IsCashPayment
        public override void InitializeCciContainer()
        {
            //[NewProduct] Code à enrichir lors de l'ajout d'un nouveau produit
            DataDocumentContainer document = TradeCommonInput.DataDocument;
            //
            CciTrade = null;
            if (null != document)
            {

                ProductContainer product = document.CurrentProduct;
                if (product.IsCashPayment)
                {
                    CciTrade = new CciTradeRisk(this);
                    CciTrade.cciProduct = new CciProductCashPayment((CciTradeRisk)CciTrade, (IBulletPayment)CciTrade.CurrentTrade.Product, string.Empty);
                }
                else if (product.IsBulletPayment)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductBulletPayment((CciTrade)CciTrade, (IBulletPayment)CciTrade.CurrentTrade.Product, string.Empty);
                }
                else if (product.IsBuyAndSellBack)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductBuyAndSellBack((CciTrade)CciTrade, (IBuyAndSellBack)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_bsb);
                }
                else if (product.IsCapFloor)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductCapFloor((CciTrade)CciTrade, (ICapFloor)CciTrade.CurrentTrade.Product, string.Empty);
                }
                else if (product.IsDebtSecurity)
                {
                    CciTrade = new CciTradeDebtSecurity(this);
                    CciTrade.cciProduct = new CciProductDebtSecurity((CciTradeDebtSecurity)CciTrade, (IDebtSecurity)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_debtSecurity);
                }
                else if (product.IsDebtSecurityTransaction)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductDebtSecurityTransaction((CciTrade)CciTrade, (IDebtSecurityTransaction)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_debtSecurityTransaction);
                }
                else if (product.IsBondOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductDebtSecurityOption((CciTrade)CciTrade, (IDebtSecurityOption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_bondOption);
                }
                else if (product.IsEquityOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductEquityOption((CciTrade)CciTrade, (IEquityOption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_eqOpt);
                }
                // EG 20140526 New
                else if (product.IsReturnSwap)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductReturnSwap((CciTrade)CciTrade, (IReturnSwap)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_returnSwap);
                }
                else if (product.IsExchangeTradedDerivative)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductExchangeTradedDerivative((CciTrade)CciTrade, (IExchangeTradedDerivative)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_exchangeTradedDerivative);
                }
                else if (product.IsEquitySecurityTransaction)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductEquitySecurityTransaction((CciTrade)CciTrade, (IEquitySecurityTransaction)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_equitySecurityTransaction);
                }
                else if (product.IsFra)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFra((CciTrade)CciTrade, (IFra)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fra);
                }
                else if (product.IsFxAverageRateOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFxAverageRateOption((CciTrade)CciTrade, (IFxAverageRateOption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fxAveRateOpt);
                }
                else if (product.IsFxBarrierOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFxBarrierOption((CciTrade)CciTrade, (IFxBarrierOption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fxBarOpt);
                }
                else if (product.IsFxDigitalOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFXDigitalOption((CciTrade)CciTrade, (IFxDigitalOption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fxDigOpt);
                }
                else if (product.IsFxLeg)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFX((CciTrade)CciTrade, (IFxLeg)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fx);
                }
                else if (product.IsFxSimpleOption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFXOptionLeg((CciTrade)CciTrade, (IFxOptionLeg)CciTrade.CurrentTrade.Product, CciProductFXOptionLeg.FxProduct.FxOptionLeg, TradeCustomCaptureInfos.CCst.Prefix_fxOpt);
                }
                else if (product.IsFxSwap)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductFX((CciTrade)CciTrade, (IFxSwap)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_fx);
                }
                else if (product.IsFxTermDeposit)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductTermDeposit((CciTrade)CciTrade, (ITermDeposit)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_termDeposit);
                }
                else if (product.IsLoanDeposit)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductLoanDeposit((CciTrade)CciTrade, (ILoanDeposit)CciTrade.CurrentTrade.Product, string.Empty);
                }
                else if (product.IsRepo)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductRepo((CciTrade)CciTrade, (IRepo)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_repo);
                }
                else if (product.IsSwap)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductSwap((CciTrade)CciTrade, (ISwap)CciTrade.CurrentTrade.Product, string.Empty);
                }
                else if (product.IsSwaption)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductSwaption((CciTrade)CciTrade, (ISwaption)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_swaption);
                }
                else if (product.IsStrategy)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductStrategy((CciTrade)CciTrade, (IStrategy)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_strategy);
                }
                else if (product.IsMarginRequirement)
                {
                    CciTrade = new CciTradeRisk(this);
                    CciTrade.cciProduct = new CciProductMarginRequirement((CciTradeRisk)CciTrade, (IMarginRequirement)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_marginRequirement);
                }
                else if (product.IsCommoditySpot)
                {
                    CciTrade = new CciTrade(this);
                    CciTrade.cciProduct = new CciProductCommoditySpot((CciTrade)CciTrade, (ICommoditySpot)CciTrade.CurrentTrade.Product, TradeCustomCaptureInfos.CCst.Prefix_commoditySpot);
                }

                if (null != CciTrade)
                {
                    if (null != CciTrade.cciProduct)
                        CciTrade.cciProduct.SetProduct(CciTrade.CurrentTrade.Product);

                    CciTrade.Initialize(CciTrade.PrefixHeader);
                }
                //else
                //Les Product non gérées par la saisie light, accessibles depuis la full uniquement passe ici
                //throw new SpheresException("CustomCaptureInfos.InitializeCciContainer", "Product is unknown");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeCapture"></param>
        public override void InitializeDocument(Cst.Capture.ModeEnum pModeCapture)
        {
            if (CciContainerSpecified)
            {
                if ((false == IsPreserveData) && Cst.Capture.IsModeNewOrDuplicateOrReflect(pModeCapture))
                    CciTrade.Initialize_Document();
                //
                base.InitializeDocument(pModeCapture);
            }
        }

        /// <summary>
        /// Retourne true si la donnée pValue existe sur un des Emetteurs
        /// <para>Valable uniquement sur les opérations sur titre</para>
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public override bool IsValueUseInIssuer(string pValue)
        {
            //20091002 FI Add this function
            bool ret = false;
            ArrayList alCciSecurityAsset = new ArrayList();
            //
            if (CciTrade.DataDocument.CurrentProduct.IsDebtSecurityTransaction)
            {
                CciSecurityAsset cciSecurityAsset = ((CciProductDebtSecurityTransaction)((CciTrade)CciTrade).cciProduct).CciSecurityAsset;
                alCciSecurityAsset.Add(cciSecurityAsset);
            }
            else if (CciTrade.DataDocument.CurrentProduct.ProductBase.IsSaleAndRepurchaseAgreement)
            {
                CciProductSaleAndRepurchaseAgreement cciSandR = (CciProductSaleAndRepurchaseAgreement)CciTrade.cciProduct;

                for (int i = 0; i < ArrFunc.Count(cciSandR.CciSpotLeg); i++)
                {
                    if (cciSandR.CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset != null)
                        alCciSecurityAsset.Add(cciSandR.CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset);
                }
                //
                for (int i = 0; i < ArrFunc.Count(cciSandR.CciForwardLeg); i++)
                {
                    if (cciSandR.CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset != null)
                        alCciSecurityAsset.Add(cciSandR.CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset);
                }
            }
            //
            if (ArrFunc.IsFilled(alCciSecurityAsset))
            {
                CciSecurityAsset[] cciSecurityAsset = (CciSecurityAsset[])alCciSecurityAsset.ToArray(typeof(CciSecurityAsset));
                for (int i = 0; i < ArrFunc.Count(cciSecurityAsset); i++)
                {
                    CustomCaptureInfo cci = cciSecurityAsset[i].Cci(CciSecurityAsset.CciEnum.issuer);
                    ret = (cci.NewValue == pValue);
                    if (ret)
                        break;
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// Non utilisé pour l'instant
        /// </summary>
        public override void UpdCaptureFromCaptureMode()
        {
        }

        /// <summary>
        /// Retourne le clientId qui permet l'accès à un element du dataDocument via reflection
        /// <para>Exemple sur les swap</para>
        /// <para>Obtient "trade1_swap_swapStream" pour accéder aux streams d'un swap lorsque clientId vaut "swapStream" (valeur dans les descriptifs XML des écrans swap)</para>
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public override string ShiftClientIdToDocumentElement(string pClientId)
        {

            string ret = pClientId;
            //
            ProductContainer product = TradeCommonInput.DataDocument.CurrentProduct;
            //
            if (pClientId.StartsWith("tradeHeader_party"))
            {
                ret = pClientId.Replace("tradeHeader_party", "party");
                if (ret.Contains("_broker"))
                    ret = ret.Replace("_broker", "_brokerPartyReference");
            }
            //
            if (pClientId.StartsWith("tradeHeader_broker"))
                ret = pClientId.Replace("tradeHeader_broker", "trade1_brokerPartyReference");
            string prefix;
            //
            if (product.IsBulletPayment)
            {
                prefix = CCst.Prefix_bulletPayment;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsBuyAndSellBack)
            {
                if (pClientId.StartsWith(CCst.Prefix_bsb))
                    ret = "trade1_" + CCst.Prefix_buyAndSellBack + pClientId.Substring(CCst.Prefix_bsb.Length);
                else if (pClientId.StartsWith(CCst.Prefix_buyAndSellBack)) //On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_buyAndSellBack + pClientId.Substring(CCst.Prefix_buyAndSellBack.Length);
            }
            else if (product.IsCapFloor)
            {
                if (pClientId.StartsWith(CCst.Prefix_capFloorStream))
                    ret = "trade1_" + CCst.Prefix_capFloor + "_" + CCst.Prefix_capFloorStream + pClientId.Substring(CCst.Prefix_capFloorStream.Length);
                else if (pClientId.StartsWith(CCst.Prefix_additionalPayment))
                    ret = "trade1_" + CCst.Prefix_capFloor + "_" + CCst.Prefix_additionalPayment + pClientId.Substring(CCst.Prefix_additionalPayment.Length);
                else if (pClientId.StartsWith(CCst.Prefix_premium))
                    ret = "trade1_" + CCst.Prefix_capFloor + "_" + CCst.Prefix_premium + pClientId.Substring(CCst.Prefix_premium.Length);
                else if (pClientId.StartsWith(CCst.Prefix_capFloor)) //On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_capFloor + pClientId.Substring(CCst.Prefix_capFloor.Length);
            }
            else if (product.IsDebtSecurity)
            {
                prefix = CCst.Prefix_debtSecurity;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsDebtSecurityTransaction)
            {
                prefix = CCst.Prefix_debtSecurityTransaction;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsBondOption)
            {
                prefix = CCst.Prefix_bondOption;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsEquityOption)
            {
                if (pClientId.StartsWith(CCst.Prefix_eqOpt))
                    ret = "trade1_" + CCst.Prefix_equityOption + pClientId.Substring(CCst.Prefix_eqOpt.Length);
                else if (pClientId.StartsWith(CCst.Prefix_equityOption))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_equityOption + pClientId.Substring(CCst.Prefix_equityOption.Length);
            }
            else if (product.IsReturnSwap)
            {
                //EG 20140526 New
                if (pClientId.StartsWith(CCst.Prefix_returnSwap))
                    ret = "trade1_" + CCst.Prefix_returnSwap + pClientId.Substring(CCst.Prefix_returnSwap.Length);
            }
            else if (product.IsExchangeTradedDerivative)
            {
                prefix = CCst.Prefix_exchangeTradedDerivative;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsFra)
            {
                prefix = CCst.Prefix_fra;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsFxAverageRateOption)
            {
                if (pClientId.StartsWith(CCst.Prefix_fxAveRateOpt))
                    ret = "trade1_" + CCst.Prefix_fxAverageRateOption + pClientId.Substring(CCst.Prefix_fxAveRateOpt.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxAverageRateOption))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxAverageRateOption + pClientId.Substring(CCst.Prefix_fxAverageRateOption.Length);
            }
            else if (product.IsFxBarrierOption)
            {
                if (pClientId.StartsWith(CCst.Prefix_fxBarOpt))
                    ret = "trade1_" + CCst.Prefix_fxBarrierOption + pClientId.Substring(CCst.Prefix_fxBarOpt.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxBarrierOption))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxBarrierOption + pClientId.Substring(CCst.Prefix_fxBarrierOption.Length);
            }
            else if (product.IsFxDigitalOption)
            {
                if (pClientId.StartsWith(CCst.Prefix_fxDigOpt))
                    ret = "trade1_" + CCst.Prefix_fxDigitalOption + pClientId.Substring(CCst.Prefix_fxDigOpt.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxDigitalOption))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxDigitalOption + pClientId.Substring(CCst.Prefix_fxDigitalOption.Length);
            }
            else if (product.IsFxSimpleOption)
            {
                if (pClientId.StartsWith(CCst.Prefix_fxOpt))
                    ret = "trade1_" + CCst.Prefix_fxSimpleOption + pClientId.Substring(CCst.Prefix_fxOpt.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxSimpleOption))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxSimpleOption + pClientId.Substring(CCst.Prefix_fxSimpleOption.Length);
            }
            else if (product.IsFxLeg)
            {
                if (pClientId.StartsWith(CCst.Prefix_fx))
                    ret = "trade1_" + CCst.Prefix_fxSingleLeg + pClientId.Substring(CCst.Prefix_fx.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxSingleLeg))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxSingleLeg + pClientId.Substring(CCst.Prefix_fxSingleLeg.Length);
            }
            else if (product.IsFxSwap)
            {
                if (pClientId.StartsWith(CCst.Prefix_fx))
                    ret = "trade1_" + CCst.Prefix_fxSwap + "_" + CCst.Prefix_fxSingleLeg + pClientId.Substring(CCst.Prefix_fx.Length);
                else if (pClientId.StartsWith(CCst.Prefix_fxSwap))//On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_fxSwap + pClientId.Substring(CCst.Prefix_fxSwap.Length);
            }
            else if (product.IsLoanDeposit)
            {
                if (pClientId.StartsWith(CCst.Prefix_loanDepositStream))
                    ret = "trade1_" + CCst.Prefix_loanDeposit + "_" + CCst.Prefix_loanDepositStream + pClientId.Substring(CCst.Prefix_loanDepositStream.Length);
                else if (pClientId.StartsWith(CCst.Prefix_additionalPayment))
                    ret = "trade1_" + CCst.Prefix_loanDeposit + "_" + CCst.Prefix_additionalPayment + pClientId.Substring(CCst.Prefix_additionalPayment.Length);
                else if (pClientId.StartsWith(CCst.Prefix_loanDeposit)) //On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_loanDeposit + pClientId.Substring(CCst.Prefix_loanDeposit.Length);
            }
            else if (product.IsRepo)
            {
                prefix = CCst.Prefix_repo;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsSwap)
            {
                if (pClientId.StartsWith(CCst.Prefix_swapStream))
                    ret = "trade1_" + CCst.Prefix_swap + "_" + CCst.Prefix_swapStream + pClientId.Substring(CCst.Prefix_swapStream.Length);
                else if (pClientId.StartsWith(CCst.Prefix_additionalPayment))
                    ret = "trade1_" + CCst.Prefix_swap + "_" + CCst.Prefix_additionalPayment + pClientId.Substring(CCst.Prefix_additionalPayment.Length);
                else if (pClientId.StartsWith(CCst.Prefix_swap)) //On passera ici dans le futur
                    ret = "trade1_" + CCst.Prefix_swap + pClientId.Substring(CCst.Prefix_swap.Length);
            }
            else if (product.IsSwaption)
            {
                prefix = CCst.Prefix_swaption;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsFxTermDeposit)
            {
                prefix = CCst.Prefix_termDeposit;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            else if (product.IsStrategy)
            {
                if (pClientId.StartsWith(CCst.Prefix_fxSimpleOption))
                    ret = "trade1_" + CCst.Prefix_strategy + "_" + CCst.Prefix_fxSimpleOption + pClientId.Substring(CCst.Prefix_fxSimpleOption.Length);
                else if (pClientId.StartsWith(CCst.Prefix_equityOption))
                    ret = "trade1_" + CCst.Prefix_strategy + "_" + CCst.Prefix_equityOption + pClientId.Substring(CCst.Prefix_equityOption.Length);
                else if (pClientId.StartsWith(CCst.Prefix_strategy))
                    ret = "trade1_" + CCst.Prefix_strategy + pClientId.Substring(CCst.Prefix_strategy.Length);
            }
            else if (product.IsMarginRequirement)
            {
                prefix = CCst.Prefix_marginRequirement;
                if (pClientId.StartsWith(prefix))
                    ret = "trade1_" + prefix + pClientId.Substring(prefix.Length);
            }
            //
            if (pClientId.StartsWith(CCst.Prefix_otherPartyPayment))
                ret = "trade1_" + CCst.Prefix_otherPartyPayment + pClientId.Substring(CCst.Prefix_otherPartyPayment.Length);
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientIdIndex"></param>
        /// <param name="pIndex"></param>
        public void Dump_Index_ToDocument(string pCciClientIdIndex, IFloatingRateIndex pIndex)
        {

            bool isLoaded = false;
            CustomCaptureInfo cci = null;
            SQL_AssetIndex sql_Index = null;

            if (Contains(pCciClientIdIndex) && (null != pIndex))
            {
                cci = this[pCciClientIdIndex];

                //Initialisation
                cci.Sql_Table = null;
                cci.ErrorMsg = string.Empty;
                pIndex.OTCmlId = 0;
                pIndex.Value = string.Empty;
                //
                if (StrFunc.IsFilled(cci.NewValue))
                {
                    string dataToFind = cci.NewValue;
                    sql_Index = new SQL_AssetIndex(CSTools.SetCacheOn(CS), SQL_AssetIndex.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                    isLoaded = sql_Index.IsLoaded && (sql_Index.RowsCount == 1);
                    //
                    if (false == isLoaded)
                    {
                        dataToFind = cci.NewValue.Replace(" ", "%") + "%";
                        sql_Index = new SQL_AssetIndex(CSTools.SetCacheOn(CS), SQL_AssetIndex.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        isLoaded = sql_Index.IsLoaded && (sql_Index.RowsCount == 1);
                    }
                }
            }
            if (isLoaded)
            {
                cci.NewValue = sql_Index.Identifier;
                cci.Sql_Table = sql_Index;
                //
                pIndex.OTCmlId = sql_Index.Id;
                pIndex.Value = sql_Index.Identifier;
            }
            else
            {
                cci.ErrorMsg = cci.IsEmpty && cci.IsMandatory ? Ressource.GetString("Msg_AssetNotFound") : string.Empty;
                cci.Sql_Table = null;
                pIndex.OTCmlId = 0;
                pIndex.Value = string.Empty;
            }
        }

        /// <summary>
        /// Alimente {pBda} à partir du cci {pClientId_bdc} 
        /// </summary>
        /// <param name="pBda"></param>
        /// <param name="pClientId_bdc">Cci qui représente la BusinessDayConvention</param>
        /// <param name="pBusinessCenters_Id">Id pour l'élément pBda.businessCentersDefine</param>
        /// <param name="pCciScanBCs"></param>
        public void DumpBDC_ToDocument(IBusinessDayAdjustments pBda, string pClientId_bdc, string pBusinessCenters_Id, CciBC pCciScanBCs)
        {
            bool isOk = Contains(pClientId_bdc) && (null != pBda);
            if (isOk)
            {
                CustomCaptureInfo cciBdc = this[pClientId_bdc];
                BusinessDayConventionEnum newBdc = BusinessDayConventionEnum.NotApplicable;
                BusinessDayConventionEnum lastBdc = BusinessDayConventionEnum.NotApplicable;

                if (StrFunc.IsFilled(cciBdc.NewValue))
                    newBdc = StringToEnum.BusinessDayConvention(cciBdc.NewValue);
                if (StrFunc.IsFilled(cciBdc.LastValue))
                    lastBdc = StringToEnum.BusinessDayConvention(cciBdc.LastValue);

                pBda.BusinessDayConvention = newBdc;

                bool isNewWithBcs = (false == Tools.IsBdcNoneOrNA(newBdc));
                bool isLastWithBcs = (false == Tools.IsBdcNoneOrNA(lastBdc));
                if ((isNewWithBcs != isLastWithBcs)			 // Passage de NA ou None Vers une aure convention
                    || (newBdc == lastBdc)				     //newBdc == lastBdc indique que l'on est ici suite à un changement de Currency ou de Payer/Receiver
                    || (isNewWithBcs && (false == pBda.BusinessCentersDefineSpecified) && (false == pBda.BusinessCentersReferenceSpecified)))
                {

                    #region  SetBusinessCentersInBDA or RelativeDateOffset
                    //
                    IBusinessCenters bcs = pBda.BusinessCentersDefine;

                    // RD 20200825 [25451] Pour CommoditySpot, il n y'a pas de BDC (il est à NONE) alors ne pas écraser le BC existant (il s'agit du BC du marché)
                    if (CciTrade.Product.IsCommoditySpot == false || isNewWithBcs)
                    {
                        IBusinessCenter[] bc = null;
                        if (isNewWithBcs)
                            bc = pCciScanBCs.GetBusinessCenters();
                        bcs.BusinessCenter = bc;
                        if (StrFunc.IsEmpty(bcs.Id))
                            bcs.Id = pBusinessCenters_Id;

                        bool isExistBCs = ArrFunc.IsFilled(bc);
                        pBda.BusinessCentersNoneSpecified = (false == isExistBCs);
                        pBda.BusinessCentersReferenceSpecified = false;
                        pBda.BusinessCentersDefineSpecified = (isExistBCs);
                    }
                    #endregion
                    //
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientIdFloatingRateIndex"></param>
        /// <param name="pObjParent"></param>
        /// <param name="pfloatingRateIndex"></param>
        /// <param name="pIndexTenor"></param>
        public void Dump_FloatingRateIndex_ToDocument(string pCciClientIdFloatingRateIndex, object pObjParent, IFloatingRateIndex pfloatingRateIndex, IInterval pIndexTenor)
        {
            DumpFloatingRateIndex_ToDocument(pCciClientIdFloatingRateIndex, null, pObjParent, pfloatingRateIndex, pIndexTenor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientIdFloatingRateIndex"></param>
        /// <param name="pDefaultPeriod"></param>
        /// <param name="pObjParent"></param>
        /// <param name="pfloatingRateIndex"></param>
        /// <param name="pIndexTenor"></param>
        public void DumpFloatingRateIndex_ToDocument(string pCciClientIdFloatingRateIndex, IInterval pDefaultPeriod,
            object pObjParent, IFloatingRateIndex pfloatingRateIndex, IInterval pIndexTenor)
        {
            bool isLoaded = false;
            bool isFound = false;
            CustomCaptureInfo cci = null;
            SQL_AssetRateIndex sql_RateIndex = null;
            FieldInfo fld = null;
            //
            if (Contains(pCciClientIdFloatingRateIndex) && (null != pfloatingRateIndex))
            {
                cci = this[pCciClientIdFloatingRateIndex];
                string dataToFind = string.Empty;
                //
                //Initialisation
                SQL_AssetRateIndex.IDType IDTypeSearch = SQL_AssetRateIndex.IDType.Asset_Identifier;
                cci.Sql_Table = null;
                cci.ErrorMsg = string.Empty;
                pfloatingRateIndex.OTCmlId = 0;
                pfloatingRateIndex.Value = string.Empty;
                //
                if (null != pObjParent)
                    fld = pObjParent.GetType().GetField("indexTenorSpecified");
                //
                if (StrFunc.IsFilled(cci.NewValue))
                {
                    string search_asset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_rateindex",
                                                typeof(System.String), IDTypeSearch.ToString());
                    string[] aSearch_asset = search_asset.Split(";".ToCharArray());
                    //Tip: Utile pour trouver "AUD-LIBOR-BBA/11M" à partir de "LIBOR 11" via "AUD LIBOR 11"
                    int searchCount = aSearch_asset.Length;
                    for (int k = 0; k < searchCount; k++)
                    {
                        try { IDTypeSearch = (SQL_AssetRateIndex.IDType)Enum.Parse(typeof(SQL_AssetRateIndex.IDType), aSearch_asset[k], true); }
                        catch { continue; }

                        for (int i = 0; i < 3; i++)
                        {
                            dataToFind = cci.NewValue;
                            if (i == 1)
                                dataToFind = cci.NewValue.Replace(" ", "%") + "%";
                            else if (i == 2)//20090618 PL Newness
                                dataToFind = "%" + cci.NewValue.Replace(" ", "%") + "%";

                            sql_RateIndex = new SQL_AssetRateIndex(CSTools.SetCacheOn(CS), IDTypeSearch, dataToFind,
                                SQL_Table.ScanDataDtEnabledEnum.Yes)
                            {
                                MaxRows = 2 //NB: Afin de retourner au max 2 lignes
                            };

                            isLoaded = sql_RateIndex.IsLoaded;
                            int rowsCount = sql_RateIndex.RowsCount;
                            isFound = isLoaded && (rowsCount == 1);
                            if (isLoaded)
                                break;
                        }
                        if (isLoaded)
                            break;
                    }
                }
                //
                #region Search with Tenor
                //if ((false == isLoaded) && false == RateTools.IsFloatingRateWithTenor(dataToFind))
                if ((!isFound) && (!RateTools.IsFloatingRateWithTenor(dataToFind)))
                {
                    //Un Rate avec tenor a été trouvé, mais aucun tenor de saisi --> on recherche avec la frequence pour default tenor
                    //if ( (sql_RateIndex.Asset_RateIndexWithTenor) && (false == RateTools.IsFloatingRateWithTenor(dataToFindInitial)) && (pDefaultPeriod.periodMultiplier.IntValue >0) )
                    if ((null != pDefaultPeriod) && (pDefaultPeriod.PeriodMultiplier.IntValue > 0))
                    {
                        string periodMultiplier = pDefaultPeriod.PeriodMultiplier.IntValue.ToString();
                        string period = pDefaultPeriod.Period.ToString();
                        SQL_AssetRateIndex tmp_sql_RateIndex = new SQL_AssetRateIndex(CSTools.SetCacheOn(CS), IDTypeSearch, RateTools.GetFloatingRateWithTenor(dataToFind, periodMultiplier, period),
                                                    SQL_Table.ScanDataDtEnabledEnum.Yes);
                        isFound = tmp_sql_RateIndex.IsLoaded && (tmp_sql_RateIndex.RowsCount == 1);
                        if (isFound)
                        {
                            sql_RateIndex = tmp_sql_RateIndex;
                        }
                        else
                        {
                            //20090514 PL Use PeriodTofrequency.Translate()
                            bool isNewSearch = PeriodTofrequency.Translate(ref periodMultiplier, ref period, true);
                            if (isNewSearch)
                            {
                                tmp_sql_RateIndex = new SQL_AssetRateIndex(CSTools.SetCacheOn(CS), IDTypeSearch, RateTools.GetFloatingRateWithTenor(dataToFind, periodMultiplier, period),
                                                    SQL_Table.ScanDataDtEnabledEnum.Yes);
                                isFound = tmp_sql_RateIndex.IsLoaded && (tmp_sql_RateIndex.RowsCount == 1);
                                if (isFound)
                                    sql_RateIndex = tmp_sql_RateIndex;
                            }
                        }
                    }
                }
                #endregion Search with Tenor
            }
            if (isFound)
            {
                cci.NewValue = sql_RateIndex.Identifier;
                cci.Sql_Table = sql_RateIndex;
                //
                pfloatingRateIndex.OTCmlId = sql_RateIndex.IdAsset;
                pfloatingRateIndex.Value = sql_RateIndex.Idx_IdIsda;
                //		
                if (null != pIndexTenor)  // There is no pfloatingRateCalculation with Fra product
                {
                    if (null != fld)
                        fld.SetValue(pObjParent, sql_RateIndex.Asset_RateIndexWithTenor);

                    if (sql_RateIndex.Asset_RateIndexWithTenor)
                    {
                        pIndexTenor.Period = StringToEnum.Period(sql_RateIndex.Asset_Period_Tenor);
                        pIndexTenor.PeriodMultiplier = new EFS_Integer(sql_RateIndex.Asset_PeriodMltp_Tenor.ToString());
                    }
                }
            }
            else
            {
                // 20090511 RD
                if (cci.IsFilled)
                    cci.ErrorMsg = Ressource.GetString("Msg_IndexNotFound");
                else
                    cci.ErrorMsg = cci.IsMandatory ? Ressource.GetString("Msg_IndexNotFound") : string.Empty;
                //
                cci.Sql_Table = null;
                pfloatingRateIndex.OTCmlId = 0;
                pfloatingRateIndex.Value = string.Empty;
                if (null != fld)
                    fld.SetValue(pObjParent, false);
            }

        }

        /// <summary>
        /// Set a Offset (ie: rateCutOffDaysOffset on a CapFloor, ...)
        /// <para>Retourne true s'il existe un paramétrage d'offset sur l'asset</para>
        /// </summary>
        /// <param name="pO"></param>
        /// <param name="pSql_Rate"></param>
        public bool DumpOffset_ToDocument(IOffset pOffset, SQL_AssetRateIndex pSql_RateIndex, string pRadical)
        {
            string tmp = pSql_RateIndex.FirstRow["Idx_PERIODMLTP" + pRadical + "OFFSET"].ToString();
            //isSpecified = StrFunc.IsEmpty(tmp) && (0 < Convert.ToInt32(tmp));
            //20080619 PL/EPL Correction
            bool ret = StrFunc.IsFilled(tmp) && (0 != Convert.ToInt32(tmp));
            if (ret)
            {
                // DayType
                tmp = pSql_RateIndex.FirstRow["Idx_DAYTYPE" + pRadical + "OFFSET"].ToString();
                if (System.Enum.IsDefined(typeof(DayTypeEnum), tmp))
                {
                    pOffset.DayTypeSpecified = true;
                    pOffset.DayType = StringToEnum.DayType(tmp);
                }
                else
                {
                    pOffset.DayTypeSpecified = false;
                }

                // Frequency
                tmp = pSql_RateIndex.FirstRow["Idx_PERIOD" + pRadical + "OFFSET"].ToString();
                if (System.Enum.IsDefined(typeof(PeriodEnum), tmp))
                {
                    pOffset.Period = StringToEnum.Period(tmp);
                    pOffset.PeriodMultiplier.Value = pSql_RateIndex.FirstRow["Idx_PERIODMLTP" + pRadical + "OFFSET"].ToString();
                }
            }
            return ret;
        }

        /// <summary>
        /// Set a RelativeDateOffset (ie: fixingDateOffset on a Fra, fixingDates on a CapFloor, ...)
        /// </summary>
        /// <param name="pRDO"></param>
        /// <param name="pSql_Rate"></param>
        /// <param name="pDateRelativeTo_Href"></param>
        /// <param name="pBusinessCenters_Id"></param>
        public void DumpRelativeDateOffset_ToDocument(IRelativeDateOffset pRDO, SQL_AssetRateIndex pSql_RateIndex, string pDateRelativeTo_Href, string pBusinessCenters_Id)
        {
            //Set BC from FloatingRate repository
            int countBc = 1;
            if (StrFunc.IsFilled(pSql_RateIndex.Idx_BusinessCenterAdditional))
            {
                if (pSql_RateIndex.Idx_BusinessCenter != pSql_RateIndex.Idx_BusinessCenterAdditional)
                    countBc++;
            }
            //
            //Set BDC from FloatingRate repository
            pRDO.BusinessDayConvention = pSql_RateIndex.FpML_Enum_BusinessDayConvention_Payment;
            IBusinessDayAdjustments bda;
            if (1 < countBc)
                bda = pRDO.CreateBusinessDayAdjustments(pSql_RateIndex.FpML_Enum_BusinessDayConvention_Payment, pSql_RateIndex.Idx_BusinessCenter, pSql_RateIndex.Idx_BusinessCenterAdditional);
            else
                bda = pRDO.CreateBusinessDayAdjustments(pSql_RateIndex.FpML_Enum_BusinessDayConvention_Payment, pSql_RateIndex.Idx_BusinessCenter);

            pRDO.BusinessCentersReferenceSpecified = false;
            pRDO.BusinessCentersNoneSpecified = false;
            pRDO.BusinessCentersDefineSpecified = true;
            pRDO.BusinessCentersDefine = bda.BusinessCentersDefine;
            if (StrFunc.IsFilled(pBusinessCenters_Id) && StrFunc.IsFilled(pRDO.BusinessCentersDefine.Id))
                pRDO.BusinessCentersDefine.Id = pBusinessCenters_Id;
            //                    
            //Set dateRelativeTo to EffectiveDate
            pRDO.DateRelativeToValue = pDateRelativeTo_Href;
            //
            //Set fixingDateOffset from FloatingRate repository
            pRDO.DayTypeSpecified = false;
            if (StrFunc.IsFilled(pSql_RateIndex.Idx_DayTypeFixingOffset))
            {
                pRDO.DayTypeSpecified = true;
                pRDO.DayType = pSql_RateIndex.FpML_Enum_DayTypeFixingOffset;
            }
            //
            pRDO.PeriodMultiplier.Value = 0.ToString();
            pRDO.Period = PeriodEnum.D;
            if (StrFunc.IsFilled(pSql_RateIndex.Idx_PeriodFixingOffset))
            {
                pRDO.PeriodMultiplier.Value = pSql_RateIndex.Idx_PeriodMlptFixingOffset.ToString();
                pRDO.Period = pSql_RateIndex.FpML_Enum_PeriodFixingOffset;
            }
            //Nothing dat in repository --> NONE (20060309 PL Ligne ajoutée pour palier au FOLLOWING qui venait par défaut)
            pRDO.BusinessDayConvention = BusinessDayConventionEnum.NONE;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxTriggerOrFxBarrierObject"></param>
        /// <param name="psql_asset"></param>
        public void DumpTriggerOrBarrierFromAssetFxRate(object pFxTriggerOrFxBarrierObject, SQL_AssetFxRate psql_asset)
        {
            object obj = pFxTriggerOrFxBarrierObject;
            SQL_AssetFxRate sql_asset = psql_asset;
            if (null != obj && (null != sql_asset))
            {
                #region quotedCurrencyPair
                FieldInfo fld = obj.GetType().GetField("quotedCurrencyPair");
                IQuotedCurrencyPair qc = (IQuotedCurrencyPair)fld.GetValue(obj);
                if (null != qc)
                {
                    qc.Currency1 = sql_asset.QCP_Cur1;
                    qc.Currency2 = sql_asset.QCP_Cur2;
                    qc.QuoteBasis = sql_asset.QCP_QuoteBasisEnum;
                }
                #endregion quotedCurrencyPair
                #region informationSource Item 0
                fld = obj.GetType().GetField("informationSource");
                IInformationSource[] infos = (IInformationSource[])fld.GetValue(obj);
                //
                if (null == infos)
                {
                    infos = TradeCommonInput.CurrentTrade.Product.ProductBase.CreateInformationSources();
                    fld.SetValue(obj, infos);
                }
                //
                infos[0].OTCmlId = sql_asset.Id;
                infos[0].RateSource.Value = sql_asset.PrimaryRateSrc;
                infos[0].AssetFxRateId.Value = sql_asset.Identifier;

                //
                infos[0].RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcPage);
                if (infos[0].RateSourcePageSpecified)
                    infos[0].CreateRateSourcePage(sql_asset.PrimaryRateSrcPage);
                //
                infos[0].RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcHead);
                if (infos[0].RateSourcePageHeadingSpecified)
                    infos[0].RateSourcePageHeading = sql_asset.PrimaryRateSrcHead;
                #endregion informationSource
                #region InformationSource Item 1

                if (StrFunc.IsFilled(sql_asset.SecondaryRateSrc))
                {
                    if (infos.Length == 1)
                    {
                        ReflectionTools.AddItemInArray(obj, "informationSource", 1);
                        fld = obj.GetType().GetField("informationSource");
                        infos = (IInformationSource[])fld.GetValue(obj);
                    }
                    infos[1].RateSource.Value = sql_asset.SecondaryRateSrc;
                    infos[1].RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcPage);
                    if (infos[1].RateSourcePageSpecified)
                        infos[0].CreateRateSourcePage(sql_asset.PrimaryRateSrcPage);
                    //
                    infos[1].RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcHead);
                    if (infos[1].RateSourcePageHeadingSpecified)
                        infos[1].RateSourcePageHeading = sql_asset.SecondaryRateSrcHead;
                }
                #endregion InformationSource Item 1
            }

        }

        /// <summary>
        /// Retourne un array de CustomCaptureInfo tels que newValue != OldValue. Les ccis retenus sont nécessairement autres que ceux présents dans les ADP et OPP  
        /// </summary>
        /// <returns></returns>
        // EG 20151102 [21465] 
        public CustomCaptureInfo[] GetCciOtherThanFeeChanged(TradeInput.FeeTarget pFeeTarget)
        {

            CustomCaptureInfo[] ret = null;
            
            ArrayList al = null;
            CustomCaptureInfo[] cci = GetCciHasChanged().ToArray(); 
            
            if (ArrFunc.IsFilled(cci))
            {
                IContainerCci[] cciOPP;
                IContainerCci[] cciADP = null;
                //
                if (pFeeTarget == TradeInput.FeeTarget.trade)
                    cciOPP = ((CciTrade) CciTrade).cciOtherPartyPayment;
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                else if (pFeeTarget == TradeInput.FeeTarget.denOption)
                    // EG 20151102 [21465]
                    cciOPP = ((CciTrade)CciTrade).cciDenOption.CciOtherPartyPayment;
                else if (pFeeTarget == TradeInput.FeeTarget.none)
                    cciOPP = null;
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("FeeTarget {0} is not implemented", pFeeTarget.ToString()));
                //
                if (pFeeTarget == TradeInput.FeeTarget.trade)
                {
                    if (CciTrade.Product.IsLoanDeposit)
                    {
                        #region IsLoanDeposit
                        CciProductLoanDeposit cciLoanDeposit = (CciProductLoanDeposit)CciTrade.cciProduct;
                        cciADP = cciLoanDeposit.CciAdditionalPayment;
                        #endregion  
                    }
                    else if ((CciTrade.Product.IsSwap) || CciTrade.Product.IsSwaption)
                    {
                        CciProductSwap cciSwap;

                        #region IsSwap or IsSwaption
                        if (CciTrade.Product.IsSwap)
                            cciSwap = (CciProductSwap)CciTrade.cciProduct;
                        else
                            cciSwap = ((CciProductSwaption)CciTrade.cciProduct).CciSwap;
                        //
                        cciADP = cciSwap.CciAdditionalPayment;
                        #endregion
                    }
                    else if (CciTrade.Product.IsCapFloor)
                    {
                        #region IsCapFloor
                        CciProductCapFloor cciCapFloor = (CciProductCapFloor)CciTrade.cciProduct;
                        cciADP = cciCapFloor.CciAdditionalPayment;
                        #endregion
                    }
                    else if (CciTrade.Product.IsReturnSwap)
                    {
                        // EG 20140526 New
                        #region IsReturnSwap
                        #endregion
                    }
                    else if (CciTrade.Product.IsStrategy)
                    {
                        ArrayList alAdd = new ArrayList();
                        CciProductStrategy cciProductStrategy = (CciProductStrategy)CciTrade.cciProduct;
                        for (int i = 0; i < ArrFunc.Count(cciProductStrategy.CciSubProduct); i++)
                        {
                            CciProductBase cciProductItem = cciProductStrategy.CciSubProduct[i];
                            //
                            if (cciProductItem.Product.IsCapFloor)
                                alAdd.AddRange(((CciProductCapFloor)cciProductItem).CciAdditionalPayment);
                            else if (cciProductItem.Product.IsSwap)
                                alAdd.AddRange(((CciProductSwap)cciProductItem).CciAdditionalPayment);
                            else if (cciProductItem.Product.IsSwaption)
                                alAdd.AddRange(((CciProductSwaption)cciProductItem).CciSwap.CciAdditionalPayment);
                            else if (cciProductItem.Product.IsLoanDeposit)
                                alAdd.AddRange(((CciProductLoanDeposit)cciProductItem).CciAdditionalPayment);
                            else if (cciProductItem.Product.IsReturnSwap)
                                alAdd.AddRange(((CciProductReturnSwap)cciProductItem).CciAdditionalPayment);
                        }
                        if (ArrFunc.IsFilled(alAdd))
                            cciADP = (IContainerCci[])alAdd.ToArray(typeof(IContainerCci));
                    }
                    else if (
                            (CciTrade.Product.IsBulletPayment) ||
                            (CciTrade.Product.IsFra) ||
                            (CciTrade.Product.IsEquityOption) ||
                            (CciTrade.Product.IsFx) ||
                            (CciTrade.Product.IsFxTermDeposit) ||
                            (CciTrade.Product.IsFxOption) ||
                            (CciTrade.Product.IsDebtSecurityTransaction) ||
                            (CciTrade.Product.IsRepo) ||
                            (CciTrade.Product.IsBuyAndSellBack) ||
                            (CciTrade.Product.IsExchangeTradedDerivative) ||
                            (CciTrade.Product.IsEquitySecurityTransaction) ||
                            (CciTrade.Product.IsCommoditySpot)
                            )
                    {
                        //FI 20120709 [18002] add IsEquitySecurityTransaction dans le if
                        //
                        //NADA
                    }
                    else
                    {
                        throw new NotImplementedException("Current product is not managed, please contact EFS");
                    }
                }
                //
                al = new ArrayList(cci);
                for (int i = ArrFunc.Count(al) - 1; i >= 0; i--)
                {
                    bool isPaymentCci = false;
                    //
                    // OPP
                    if (false == isPaymentCci)
                    {
                        if (ArrFunc.IsFilled(cciOPP))
                        {
                            foreach (IContainerCci payment in cciOPP)
                            {
                                if (payment.IsCciOfContainer(cci[i].ClientId_WithoutPrefix))
                                    isPaymentCci = true;
                            }
                        }
                    }
                    // ADP   
                    if (false == isPaymentCci)
                    {
                        //
                        if (ArrFunc.IsFilled(cciADP))
                        {
                            foreach (IContainerCci payment in cciADP)
                            {
                                if (payment.IsCciOfContainer(cci[i].ClientId_WithoutPrefix))
                                    isPaymentCci = true;
                            }
                        }
                    }
                    if (isPaymentCci)
                        al.RemoveAt(i);
                }
            }
            //
            if (ArrFunc.Count(al) > 0)
                ret = (CustomCaptureInfo[])al.ToArray(typeof(CustomCaptureInfo));
            //
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientAssetFxRate"></param>
        /// <param name="pCciClientCu1"></param>
        /// <param name="pCciClientCu2"></param>
        public void InitializeCciAssetFxRate(string pCciClientAssetFxRate, string pCciClientCu1, string pCciClientCu2)
        {

            if (Contains(pCciClientAssetFxRate) && Contains(pCciClientCu1) && Contains(pCciClientCu2))
            {
                //
                if (this[pCciClientCu1].IsFilledValue && this[pCciClientCu2].IsFilledValue)
                {
                    KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
                    {
                        IdC1 = this[pCciClientCu1].NewValue,
                        IdC2 = this[pCciClientCu2].NewValue
                    };
                    int idAsset = keyAssetFxRate.GetIdAsset(CSTools.SetCacheOn(CS));
                    if (0 < idAsset)
                    {
                        SQL_AssetFxRate sql_asset = new SQL_AssetFxRate(CSTools.SetCacheOn(CS), idAsset, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        //sql_asset.LoadTable(new string[] { "IDENTIFIER" });
                        //20081030 PL Pb alias 
                        sql_asset.LoadTable();
                        if (sql_asset.IsLoaded)
                            SetNewValue(pCciClientAssetFxRate, sql_asset.Identifier);
                    }
                }
            }

        }

        /// <summary>
        /// Initialisation de la reference Notionel pour calcul des Primes et des additionnal payment et OtherpartyPayment 
        /// </summary>
        /// <param name="pCciStream"></param>
        /// <param name="pCciPremium"></param>
        /// <param name="pCciAdditionalPayment"></param>
        /// <param name="pCciOtherPartyPayment"></param>
        /// <param name="pCci"></param>
        public void InitializePaymentPaymentQuoteRelativeTo(string pId,
            CciPayment[] pCciPremium,
            CciPayment[] pCciAdditionalPayment,
            CciPayment[] pCciOtherPartyPayment)
        {


            if (StrFunc.IsFilled(pId))
            {
                IPayment payment;

                // Premium
                for (int i = 0; i < ArrFunc.Count(pCciPremium); i++)
                {
                    payment = (IPayment)pCciPremium[i].Payment;
                    //
                    if ((pCciPremium[i].ExistPaymentQuote) && (StrFunc.IsEmpty(payment.PaymentQuote.PaymentRelativeTo.HRef)))
                        SetNewValue(pCciPremium[i].CciClientId(CciPayment.CciEnumPayment.paymentQuote_paymentRelativeTo), pId);
                }
                // Additional 
                for (int i = 0; i < ArrFunc.Count(pCciAdditionalPayment); i++)
                {
                    payment = (IPayment)pCciAdditionalPayment[i].Payment;
                    //
                    if ((pCciAdditionalPayment[i].ExistPaymentQuote) && (StrFunc.IsEmpty(payment.PaymentQuote.PaymentRelativeTo.HRef)))
                        SetNewValue(pCciAdditionalPayment[i].CciClientId(CciPayment.CciEnumPayment.paymentQuote_paymentRelativeTo), pId);
                }
                //
                // OtherPartyPayment
                for (int i = 0; i < ArrFunc.Count(pCciOtherPartyPayment); i++)
                {
                    payment = (IPayment)pCciOtherPartyPayment[i].Payment;
                    //
                    if ((pCciOtherPartyPayment[i].ExistPaymentQuote) && (StrFunc.IsEmpty(payment.PaymentQuote.PaymentRelativeTo.HRef)))
                        SetNewValue(pCciOtherPartyPayment[i].CciClientId(CciPayment.CciEnumPayment.paymentQuote_paymentRelativeTo), pId);
                }
            }



        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciStream"></param>
        /// <param name="pCciPremium"></param>
        /// <param name="pCciAdditionalPayment"></param>
        /// <param name="pCciOtherPartyPayment"></param>
        public void InitializePaymentPaymentQuoteRelativeTo(CciStream pCciStream,
            CciPayment[] pCciPremium,
            CciPayment[] pCciAdditionalPayment,
            CciPayment[] pCciOtherPartyPayment)
        {
            string id = string.Empty;
            if (pCciStream.Irs.CalculationPeriodAmount.CalculationSpecified)
            {
                if (pCciStream.Irs.CalculationPeriodAmount.Calculation.NotionalSpecified)
                    id = pCciStream.Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Id;
            }
            //
            InitializePaymentPaymentQuoteRelativeTo(id, pCciPremium, pCciAdditionalPayment, pCciOtherPartyPayment);

        }

        /// <summary>
        /// Initialisation du Cci  DayCountFraction en fonction du taux saisie 
        /// </summary>
        public void ProcessInitialize_DCF(string pCciClientIdDayCountFraction, string pCciClientIdFloatingRate)
        {
            if (this.Contains(pCciClientIdFloatingRate) && this.Contains(pCciClientIdDayCountFraction))
            {
                SQL_AssetRateIndex sql_RateIndex = (SQL_AssetRateIndex)this[pCciClientIdFloatingRate].Sql_Table;
                if (null != sql_RateIndex)
                {
                    EFS_DayCountFraction dcf = new EFS_DayCountFraction();
                    if (StrFunc.IsFilled(sql_RateIndex.Idx_DayCountFraction))
                    {
                        dcf.DayCountFraction_FpML = sql_RateIndex.Idx_DayCountFraction;
                        this[pCciClientIdDayCountFraction].NewValue = dcf.DayCountFraction;
                    }
                }
            }

        }

        /// <summary>
        /// prepropositions des éléments d'un CciSecurity en fonction de l'émetteur
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCciSecurity"></param>
        public void ProcessInitialize_Issuer(CustomCaptureInfo pCci, CciSecurity pCciSecurity)
        {
            SQL_Actor sql_Actor = (SQL_Actor)pCci.Sql_Table;
            if (null != sql_Actor)
            {
                string idCountry = sql_Actor.IdCountryResidence;
                //20090713 FI du test suivant
                if (null != pCciSecurity.cciLocalization)
                {
                    // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                    SetNewValue(pCciSecurity.cciLocalization.CciClientId(CciLocalization.CciEnum.countryOfIssue), idCountry, /*true*/ false);
                }

                SQL_Country sql_Country = new SQL_Country(CSTools.SetCacheOn(CS), idCountry);
                if (sql_Country.IsLoaded)
                {
                    SetNewValue(pCciSecurity.CciClientId(CciUnderlyingAsset.CciEnum.currency), sql_Country.IDC, /*true*/ false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pDataInput"></param>
        /// <returns></returns>
        protected override string InterceptInput(CustomCaptureInfo pCci, string pDataInput)
        {
            string dataInput = pDataInput;
            /* FI 20220601 [XXXXX] Mise en commentaire, l'interprétation des échéances est déja réalisé  dans CciFixInstrument
            if ((null != CciTrade) && (null != CciTrade.cciProduct))
            {
                CciProductExchangeTradedDerivative cciEtd = null;
                //
                if (CciTrade.cciProduct.product.isExchangeTradedDerivative)
                {
                    if (CciTrade.cciProduct.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        cciEtd = (CciProductExchangeTradedDerivative)CciTrade.cciProduct;
                    }
                }
                else if (CciTrade.cciProduct.product.isStrategy)
                {
                    CciProductStrategy cciStrategy = (CciProductStrategy)CciTrade.cciProduct;
                    if (ArrFunc.IsFilled(cciStrategy.cciSubProduct))
                    {
                        for (int i = 0; i < ArrFunc.Count(cciStrategy.cciSubProduct) - 1; i++)
                        {
                            CciProductExchangeTradedDerivative cciProductEtd = cciStrategy.cciSubProduct[i] as CciProductExchangeTradedDerivative;
                            if ((null != cciProductEtd) && (cciProductEtd.IsCciOfContainer(pCci.ClientId_WithoutPrefix)))
                            {
                                cciEtd = cciProductEtd;
                                break;
                            }
                        }
                    }
                }
                //
                if (null != cciEtd)
                {
                    if (cciEtd.cciFixTradeCaptureReport.cciFixInstrument.IsCci(CciFixInstrument.CciEnum.MMY, pCci))
                    {
                        IExchangeTradedDerivative etd = (IExchangeTradedDerivative)cciEtd.product.product;
                        IFixInstrument fixInstrumentClone = (IFixInstrument)ReflectionTools.Clone(etd.tradeCaptureReport.Instrument, ReflectionTools.CloneStyle.CloneField);
                        
                        FixInstrumentContainer fixInstrument = new FixInstrumentContainer(fixInstrumentClone);
                        fixInstrument.SetMaturityMonthYear(CSTools.SetCacheOn(CS), fmtETDMaturityInput, pDataInput);
                        dataInput = fixInstrument.MaturityMonthYear;
                    }
                }
            }
            */
            return base.InterceptInput(pCci, dataInput);
        
        }


        // 20120711 MF Ticket 18006 added opQualifTrdType in order to use the Replace command with the old way
        /// <summary>
        /// Obtient une signature et un style de qualification qui caractérise le trade. 
        /// Cette méthode ne gère pas plus que 3 qualifications (First and Second and Third).  
        /// </summary>
        /// <param name="opQualifFirst">caption of the first qualification label</param>
        /// <param name="opForeColorFirst">text color of the first qualification label</param>
        /// <param name="opBackColorFirst">background color of the first qualification label</param>
        /// <param name="opQualifSecond">caption of the second qualification label</param>
        /// <param name="opForeColorSecond">text color of the second qualification label</param>
        /// <param name="opBackColorSecond">background color of the second qualification label</param>
        /// <param name="opQualifThird">caption of the third qualification label</param>
        /// <param name="opForeColorThird">text color of the third qualification label</param>
        /// <param name="opBackColorThird">background color of the third qualification label</param>
        /// <returns>the drawing activation flag, when true then at least one qualification will be drawn</returns>
        protected override bool DisplayNameInstrumentStyle(
            out string opQualifFirst, out string opForeColorFirst, out string opBackColorFirst,
            out string opQualifSecond, out string opForeColorSecond, out string opBackColorSecond,
            out string opQualifThird, out string opForeColorThird, out string opBackColorThird)
        {
            //PL 20120524 New feature
            //PL 20120529 Refactoring for EquitySecurityTransaction
            bool ret = false;

            // 20120711 MF Ticket 18006 START
            opQualifFirst = null;
            opQualifSecond = null;
            opQualifThird = null;

            opForeColorFirst = "transparent";
            opBackColorFirst = "transparent";
            opForeColorSecond = "transparent";
            opBackColorSecond = "transparent";
            opForeColorThird = "transparent";
            opBackColorThird = "transparent";

            // 20120711 MF Ticket 18006 END

            if (CciTrade.TradeCommonInput.IsTradeFound)
            {
                ExchangeTradedContainer exchangeTraded = null;
                ProductContainer productContainer = CciTrade.TradeCommonInput.DataDocument.CurrentProduct;

                if (productContainer.IsExchangeTradedDerivative)
                {
                    exchangeTraded = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productContainer.Product, CciTrade.TradeCommonInput.DataDocument);
                }
                else if (productContainer.IsEquitySecurityTransaction)
                {
                    exchangeTraded = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)productContainer.Product);
                }

                if (exchangeTraded != null)
                {
                    // 20120711 MF Ticket 18006 START

                    ret = GetQualifForExchTradedInstrument(CS,
                        exchangeTraded.TradeCaptureReport.TrdTypeSpecified, exchangeTraded.TradeCaptureReport.TrdType,
                        exchangeTraded.TradeCaptureReport.SecondaryTrdTypeSpecified, exchangeTraded.TradeCaptureReport.SecondaryTrdType,
                        exchangeTraded.TradeCaptureReport.SecSubTyp,
                        out opQualifFirst, out ExtendEnumValue extendEnumFirstValue,
                        out opQualifSecond, out ExtendEnumValue extendEnumSecond,
                        out opQualifThird, out ExtendEnumValue extendEnumThird
                    );

                    if (extendEnumFirstValue != null)
                    {
                        if (StrFunc.IsFilled(extendEnumFirstValue.ForeColor))
                            opForeColorFirst = extendEnumFirstValue.ForeColor;
                        if (StrFunc.IsFilled(extendEnumFirstValue.BackColor))
                            opBackColorFirst = extendEnumFirstValue.BackColor;
                    }

                    if (extendEnumSecond != null)
                    {
                        if (StrFunc.IsFilled(extendEnumSecond.ForeColor))
                            opForeColorSecond = extendEnumSecond.ForeColor;
                        if (StrFunc.IsFilled(extendEnumSecond.BackColor))
                            opBackColorSecond = extendEnumSecond.BackColor;
                    }

                    if (extendEnumThird != null)
                    {
                        if (StrFunc.IsFilled(extendEnumThird.ForeColor))
                            opForeColorThird = extendEnumThird.ForeColor;
                        if (StrFunc.IsFilled(extendEnumThird.BackColor))
                            opBackColorThird = extendEnumThird.BackColor;
                    }

                }
            }
            return ret;
        }

        /// <summary>
        /// Get the string and the styles 
        /// for the FutureExchangeTradedDerivativeBlock/OptionExchangeTradedDerivativeBlock/EquitySecurityTransactionBlock banners
        /// </summary>
        /// <remarks>20120711 MF Ticket 18006</remarks>
        /// <param name="pTrdTypeSpecified">TrdTyp specified flag, true when a valid value has been inputed</param>
        /// <param name="pTrdType">TrdTyp value, use only when pTrdTypeSpecified is true</param>
        /// <param name="pSecondaryTrdTypeSpecified">TrdTyp2 specified flag, true when a valid value has been inputed</param>
        /// <param name="pSecondaryTrdType">TrdTyp2 value, use only when pSecondaryTrdTypeSpecified is true</param>
        /// <param name="opEnumTrdTypValue">output value containing the enum value containing the wanted TrdTyp style (can be null)</param>
        /// <param name="opEnumSecondaryTrdTypValue">output value containing the enum value containing 
        /// the wanted SecondaryTrdTyp style (can be null)</param>
        /// <param name="opQualifTrdType">output value containing the TrdTyp text shown on the banner, used also to replace
        /// on the banner the old value (can be null)</param>
        /// <param name="opQualifSecondaryTrdTyp">output value containing the SecondaryTrdTyp text shown on the banner</param>
        /// <returns>draw activation flag for banner of the future/option/equity report block, true when the banner has to be updated
        /// with the TrdTyp and/or the SecondaryTrTyp values</returns>
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        /// EG 20211220 [XXXXX] La fonction retourne toujours true.
        private static bool GetQualifForExchTradedInstrument(string cs, 
            bool pTrdTypeSpecified, TrdTypeEnum pTrdType,
            bool pSecondaryTrdTypeSpecified, SecondaryTrdTypeEnum pSecondaryTrdType,
            string pStrategy,
            out string opQualifTrdType, out ExtendEnumValue opEnumTrdTypValue,
            out string opQualifSecondaryTrdTyp, out ExtendEnumValue opEnumSecondaryTrdTypValue,
            out string opQualifStrategy, out ExtendEnumValue opEnumStrategyValue)
        {
            opQualifTrdType = null;
            opEnumTrdTypValue = null;
            opQualifSecondaryTrdTyp = null;
            opEnumSecondaryTrdTypValue = null;
            opQualifStrategy = null;
            opEnumStrategyValue = null;

            // this "if" block is the same that we use inside of DisplayNameInstrumentStyle, the block is replied just in case 
            // GetEnumForInstrumentStyle will be called outside of DisplayNameInstrumentStyle
            bool validTrdType = pTrdTypeSpecified && pTrdType != TrdTypeEnum.RegularTrade;

            if (validTrdType)
            {
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnum enumTrdTyp = ExtendEnumsTools.ListEnumsSchemes["TrdTypeEnum"];
                ExtendEnum enumTrdTyp = DataEnabledEnumHelper.GetDataEnum(cs, "TrdTypeEnum");

                opEnumTrdTypValue = enumTrdTyp[pTrdType.ToString()];
                opQualifTrdType = pTrdType.ToString();
            }

            bool validSecondaryTrdType = pSecondaryTrdTypeSpecified && pSecondaryTrdType != SecondaryTrdTypeEnum.RegularTrade;

            if (validSecondaryTrdType)
            {
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnum enumSecondaryTrdTyp = ExtendEnumsTools.ListEnumsSchemes["TrdTypeEnum"];
                ExtendEnum enumSecondaryTrdTyp = DataEnabledEnumHelper.GetDataEnum(cs, "TrdTypeEnum");
                opEnumSecondaryTrdTypValue = enumSecondaryTrdTyp[pSecondaryTrdType.ToString()];
                opQualifSecondaryTrdTyp = pSecondaryTrdType.ToString();
            }

            bool validStrategy = StrFunc.IsFilled(pStrategy);

            if (validStrategy)
            {
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnum enumStrategy = ExtendEnumsTools.ListEnumsSchemes["StrategyTypeScheme"];
                ExtendEnum enumStrategy = DataEnabledEnumHelper.GetDataEnum(cs, "StrategyTypeScheme");
                opEnumStrategyValue = enumStrategy[pStrategy];
                opQualifStrategy = Ressource.GetString(string.Format("strategytype_{0}", opEnumStrategyValue.Value));
            }

            return true; // validTrdType || validSecondaryTrdType || validStrategy;
        }

        #region SetFundingAndMargin
        /// <summary>
        ///  Recherche du taux de refinancement et des marges. Alimentation du datadocument dans la foulée. 
        ///  <para>Cette méthode alimente le flag IsToSynchronizeWithDocument avec la valeur true</para>
        /// </summary>
        /// EG 20150306 [POC-BERKELEY] : Add EquitySecurityTransaction
        /// EG 20170510 [23153] Upd
        public void SetFundingAndMargin(string pCS)
        {
            string errMsg = null;
            ProcessStateTools.StatusEnum errStatus = ProcessStateTools.StatusEnum.NA;
            Exception exception = null;
            bool isOk = false;

            // Le code suivant est plus générique mais génère un ordre SQL alors FI préfère le code suivant
            //Pair<Cst.UnderlyingAsset, int> asset = TradeCommonInput.DataDocument.currentProduct.GetUnderlyingAsset(pCS, null);
            //if ((null != asset) && asset.Second > 0)
            //{

            //    FeeRequest feeRequest = new FeeRequest(CS, (TradeInput)this.TradeCommonInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
            //    if (TradeCaptureGen.SetFundingAndMargin(feeRequest, ref errMsg, ref errStatus, ref exception))
            //    {
            //        //Pour forcer la réinitialisation et afficher les données issues des barèmes.
            //        IsToSynchronizeWithDocument = true;
            //    }
            //}

            if (TradeCommonInput.DataDocument.CurrentProduct.IsReturnSwap)
            {

                IReturnSwap returnSwap = (IReturnSwap)TradeCommonInput.DataDocument.CurrentProduct.Product;
                ReturnSwapContainer returnSwapContainer = new ReturnSwapContainer(returnSwap, TradeCommonInput.DataDocument);
                /// EG 20170510 [23153]
                //returnSwapContainer.SetMainReturnLeg(string.Empty, null);
                IReturnLegMainUnderlyer returnMainUnderlyer = returnSwapContainer.MainReturnLeg.Second;
                isOk = (null != returnMainUnderlyer) && (returnMainUnderlyer.OTCmlId > 0);
            }
            else if (TradeCommonInput.DataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                IEquitySecurityTransaction equitySecurityTransaction = (IEquitySecurityTransaction)TradeCommonInput.DataDocument.CurrentProduct.Product;
                EquitySecurityTransactionContainer equitySecurityTransactionContainer = new EquitySecurityTransactionContainer(equitySecurityTransaction, TradeCommonInput.DataDocument);
                isOk = (equitySecurityTransactionContainer.SecurityId > 0);
            }

            if (isOk)
            {
                FeeRequest feeRequest = new FeeRequest(pCS, null, (TradeInput)this.TradeCommonInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
                if (TradeCaptureGen.SetFundingAndMargin(feeRequest, ref errMsg, ref errStatus, ref exception))
                {
                    //Pour forcer la réinitialisation et afficher les données issues des barèmes.
                    IsToSynchronizeWithDocument = true;
                }
            }
        }
        #endregion SetFundingAndMargin



        /// <summary>
        ///  Alimente un label dans le header de {pTableH}
        ///  <para>Si le contrôle label n'existe pas il est ajouté</para>
        /// </summary>
        /// <param name="pTogglePanel"></param>
        /// <param name="pControlId">I</param>
        /// <param name="pData"></param>
        /// <param name="pStyle">Style element for the label</param>
        /// FI 20200117 [25167] Add Method
        /// FI 20200120 [25167] Refactoring
        /// FI 20200121 [25167] Refactoring
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        /// EG 20211220 [XXXXX] Suppression des espaces dans la propriété Text (géré par padding dans CSS)
        public static void SetLinkInfoInTogglePanel(WCTogglePanel pTogglePanel, string pControlId, string pData, string pStyle)
        {
            if (null == pTogglePanel)
                throw new ArgumentNullException("pTableH is null;");

            string id = StrFunc.AppendFormat("{0}_{1}_{2}", pTogglePanel.ID, WCTogglePanel.CssHeadLinkInfo, pControlId);

            if (!(pTogglePanel.ControlHeader.FindControl(id) is Label lbl))
            {
                //Ajout d'un contrôle label dans le header de pTableH  s'il n'existe pas
                lbl = new Label
                {
                    ID = id,
                    CssClass = EFSCssClass.CssClassEnum.txtCaptureConsult.ToString(),
                };

                if (StrFunc.IsFilled(pStyle))
                    lbl.Attributes["style"] = pStyle;

                pTogglePanel.AddContentHeaderLinkInfo(lbl);
            }

            lbl.Text = string.Empty;
            if (StrFunc.IsFilled(pData))
                lbl.Text = pData;
        }


        /// <summary>
        ///  Obtient l'Id attendu d'un tableH enfant de produit
        ///  <para>Retourne string.Empty si le produit n'est pas géré</para>
        /// </summary>
        /// <param name="cciProduct">représente </param>
        /// <param name="pSuffix">Suffix attentu dans discriptif CCIML (fichier xml)</param>
        /// <returns></returns>
        /// FI 20200117 [25167] Add Method
        public string GetIdTableHProduct(string pSuffix)
        {
            if (null == CciTrade)
                throw new NullReferenceException("CciTrade is null");

            string prefix;
            if (CciTrade.cciProduct.Product.IsStrategy)
                prefix = ((CciProductStrategy)CciTrade.cciProduct).CciProductGlogal.Prefix;
            else
                prefix = CciTrade.cciProduct.Prefix;

            string ret = StrFunc.AppendFormat("{0}{1}{2}", CciPageDesign.PrefixTableHID, prefix, pSuffix);

            return ret;
        }

        #endregion Methods
        //
        #region ICloneable Members
        public object Clone()
        {
            return Clone(CustomCaptureInfo.CloneMode.CciAll);
        }
        // EG 20180425 Analyse du code Correction [CA2214]
        public object Clone(CustomCaptureInfo.CloneMode pCloneMode)
        {
            TradeCustomCaptureInfos clone = new TradeCustomCaptureInfos(this.CS, this.TradeCommonInput, this.User, this.SessionId, IsGetDefaultOnInitializeCci)
            {
                FmtETDMaturityInput = this.FmtETDMaturityInput
            };
            clone.InitializeCciContainer();

            foreach (CustomCaptureInfo cci in this)
                clone.Add((CustomCaptureInfo)cci.Clone(pCloneMode));

            return clone;
        }
        #endregion ICloneable Members
    }
 
}
