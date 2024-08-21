#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;


#endregion Using Directives

namespace EFS.TradeInformation
{
    #region TradeActionBase
    /// <summary>
    /// 
    /// </summary>
    public class TradeActionBase
    {
        /// <summary>
        /// Représente la quantité Max pour l'action
        /// </summary>
        public enum MaxQuantityEnum
        {
            /// <summary>
            /// La quantité ne peut dépasser la quantité disponible
            /// </summary>
            AvailableQuantity,
            /// <summary>
            /// La quantité ne peut dépasser la quantité initiale
            /// </summary>
            InitialQuantity,
            /// <summary>
            /// La quantité en position
            /// </summary>
            // EG 20151102 [21465] New
            PosQuantity,
        }

        #region Members
        /// <summary>
        /// Représente l'ID du trade 
        /// </summary>
        // EG 20151102 [21465] New
        public int idT;
        /// <summary>
        /// Représente le trade qui subit l'action
        /// </summary>
        public SpheresIdentification tradeIdentification;
        /// <summary>
        /// Représente la date de l'action
        /// </summary>
        public EFS_Date date;
        /// <summary>
        /// Représente la quantité impliquée dans l'action
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal quantity;
        /// <summary>
        /// Représente la quantité initiale
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal initialQuantity;
        /// <summary>
        /// Représente la quantité disponible (=la quantité en position) 
        /// <para>Ne peut être supérieure à la quantité initiale</para>
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal availableQuantity;

        public bool noteSpecified;
        /// <summary>
        /// Représente une note 
        /// </summary>
        public string note;


        /// <summary>
        /// Si true les frais initiaux sont restitués sur la base de la quantité corrigé|transférée
        /// <para>Même si l'on parle de frais, ce sont en fait tous les opps qui sont restitués/para>
        /// </summary>
        public EFS_Boolean isFeeRestitution;

        /// <summary>
        /// Si true les frais de garde (SafekeepingPayment sont restitués sur la base de la quantité corrigée|transférée
        /// </summary>
        public EFS_Boolean isReversalSafekeeping;

        public bool otherPartyPaymentSpecified = false;
        /// <summary>
        /// Représente les frais 
        /// </summary>
        [System.Xml.Serialization.XmlElement("otherPartyPayment")]
        public IPayment[] otherPartyPayment = null;

        #endregion

        #region properties
        // EG 20151102 [21465] New
        public DateTime DtBusiness
        {
            get {return date.DateValue;}
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual MaxQuantityEnum MaxQuantity
        {
            get { return MaxQuantityEnum.AvailableQuantity; }
        }
        #endregion

        #region TradeActionBase
        // EG 20150907 [21317] New
        // FI 20161005 [XXXXX] Modify
        // FI 20170116 [21916] Modify
        // EG 20230929 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
        public TradeActionBase(string pCS, TradeInput pTradeInput, Cst.Capture.ModeEnum pMode)
        {
            tradeIdentification = pTradeInput.Identification;
            DateTime dtBusiness = pTradeInput.GetDefaultDateAction(pCS,  pMode);
            date = new EFS_Date(DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtISODate));

            // FI 20170116 [21916] Call pTradeInput.DataDocument.currentProduct.RptSide()
            //RptSideProductContainer rptSideProduct = pTradeInput.DataDocument.currentProduct.RptSide(pTradeInput.CS, true);
            RptSideProductContainer rptSideProduct = pTradeInput.DataDocument.CurrentProduct.RptSide();

            // FI 20161005 [XXXXX] Add  NotImplementedException
            if (null == rptSideProduct)
                throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", pTradeInput.DataDocument.CurrentProduct.ProductBase.ToString()));

            // EG 20151102 [21465] New
            // Id du trade
            idT = pTradeInput.IdT;
            //Quantité initiale
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            initialQuantity = new EFS_Decimal(Convert.ToDecimal(rptSideProduct.Qty));
            //Quantité dispo
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20151125 [21465][20979] Refactoring Nullable<int> for idPR
            Nullable<int> idPR = null;
            if (pMode == Cst.Capture.ModeEnum.PositionCancelation)
            {
                // EG 20170127 Qty Long To Decimal
                // EG 20151125 [21465][20979] Refactoring
                // EG 20230928 [WI715] [26497]
                idPR = PosKeepingTools.GetExistingKeyPosRequest(pCS, null, Cst.PosRequestTypeEnum.PositionCancelation,
                    tradeIdentification.OTCmlId, dtBusiness, out _, out _, out note, out _, null);
            }
            // EG 20170127 Qty Long To Decimal
            availableQuantity = new EFS_Decimal(PosKeepingTools.GetAvailableQuantity(pCS, dtBusiness, tradeIdentification.OTCmlId, idPR));

            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            quantity = new EFS_Decimal(availableQuantity.DecValue);
        }
        public TradeActionBase() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public TradeActionBase(SpheresIdentification pTradeIdentification, decimal pInitialQuantity, decimal pAvailableQuantity, DateTime pDate, string pNotes)
        {
            tradeIdentification = pTradeIdentification;
            date = new EFS_Date(DtFunc.DateTimeToString(pDate, DtFunc.FmtISODate));
            initialQuantity = new EFS_Decimal(pInitialQuantity);
            availableQuantity = new EFS_Decimal(pAvailableQuantity);
            quantity = new EFS_Decimal(pAvailableQuantity);
            note = pNotes;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Alimente le membre otherPartyPaymentSource (repésentatif de restitution de frais)
        /// <para>Les frais sont restitués au prorata de la quantité transférée / corrigée</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20150916 [21367] Modify
        // EG 20180205 [23769] Upd EFS_TradeLibray constructor call  (substitution to the static class EFS_CURRENT)  
        public void CalcFeeRestitution(string pCS)
        {
            this.otherPartyPayment = null;
            this.otherPartyPaymentSpecified = false;

            if (isFeeRestitution.BoolValue)
            {
                EFS_TradeLibrary tradeLib = new EFS_TradeLibrary(pCS, null, tradeIdentification.OTCmlId);
                this.otherPartyPaymentSpecified = tradeLib.DataDocument.OtherPartyPaymentSpecified;
                if (this.otherPartyPaymentSpecified)
                    this.otherPartyPayment = (IPayment[])tradeLib.DataDocument.OtherPartyPayment.Clone();
         
                if (otherPartyPaymentSpecified)
                {
                    EFS_Cash cash;
                    // EG 20160404 Migration vs2013
                    // #warning PL 20130910 Vérifier si "initialQuantity" tient bien compte d'éventuelles action correctives précédentes.
                    decimal coef = quantity.DecValue / initialQuantity.DecValue;

                    for (int i = 0; i < ArrFunc.Count(otherPartyPayment); i++)
                    {

                        //Reverse the Payer and the Receiver
                        string tmp = otherPartyPayment[i].PayerPartyReference.HRef;
                        otherPartyPayment[i].PayerPartyReference.HRef = otherPartyPayment[i].ReceiverPartyReference.HRef;
                        otherPartyPayment[i].ReceiverPartyReference.HRef = tmp;

                        //Set paymentDateDate
                        // FI 20150916 [21367] Il faut conserver la date initiale présente sur le trade si  l'action se produit avant la date des frais   
                        // (Cas d'un transfert ou d'une correction effectuée sur un trade EquitySecurityTransaction avant la date de règelement du trade (et des frais))
                        DateTime dtOpp = otherPartyPayment[i].PaymentDate.UnadjustedDate.DateValue;
                        if (this.date.DateValue.CompareTo(dtOpp) > 0)
                            dtOpp = this.date.DateValue;
                        
                        otherPartyPayment[i].PaymentDateSpecified = true;
                        otherPartyPayment[i].PaymentDate.UnadjustedDate.DateValue = dtOpp;

                        ISpheresIdSchemeId spheresId_FormulaValue1;
                        if (Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedPeriodCharacteristicsScheme))
                        {
                            #region Cas spécifique des barèmes avec tranches ET périodes pour l'assiette (ex. Barèmes dégressifs)
                            //Step 1: Recherche du nombre de tranche
                            int lastBraket = 1;
                            for (int j = 2; j <= 999; j++)
                            {
                                lastBraket = j;
                                if (!Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket" + lastBraket.ToString()))
                                {
                                    lastBraket--;
                                    break;
                                }
                            }
                            //Step 2: Maj des données
                            decimal basisValue, paymentAmount, newBasisValue, newPaymentAmount, newTotalPaymentAmount;
                            string data;

                            paymentAmount = newTotalPaymentAmount = 0;

                            decimal remainingQuantity = quantity.DecValue;
                            for (int j = lastBraket; j >= 1; j--)
                            {
                                //PL 20141017 TBD spheresId_FormulaValue2
                                spheresId_FormulaValue1 = otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket" + j.ToString());

                                //Lecture de l'assiette sur la tranche (ex. 50000 dans le cas de [EUR 3.00][Unit: FixedAmount][Basis: Quantity Value: 50000.00][Amount: 150000.00])
                                data = spheresId_FormulaValue1.Value.Split(new string[] { " Value:" }, StringSplitOptions.None)[1];
                                data = data.Split(']')[0].Trim();
                                basisValue = DecFunc.DecValueFromInvariantCulture(data);
                                if (remainingQuantity > 0)
                                {
                                    newBasisValue = System.Math.Min(remainingQuantity, basisValue);
                                    remainingQuantity = System.Math.Max(0, remainingQuantity - basisValue);

                                    //Lecture du montant sur la tranche (ex. 150000 dans le cas de [EUR 3.00][Unit: FixedAmount][Basis: Quantity Value: 50000.00][Amount: 150000.00])
                                    data = spheresId_FormulaValue1.Value.Split(new string[] { "[Amount:" }, StringSplitOptions.None)[1];
                                    data = data.Split(']')[0].Trim();
                                    paymentAmount = DecFunc.DecValueFromInvariantCulture(data);

                                    if (newBasisValue == basisValue)
                                    {
                                        newPaymentAmount = paymentAmount;
                                    }
                                    else
                                    {
                                        //Recalcul uniquement s'il y a nouvelle assiette
                                        cash = new EFS_Cash(CSTools.SetCacheOn(pCS), paymentAmount * (newBasisValue / basisValue), otherPartyPayment[i].PaymentAmount.Currency);
                                        newPaymentAmount = cash.AmountRounded;
                                    }
                                }
                                else
                                {
                                    newBasisValue = 0;
                                    newPaymentAmount = 0;
                                }

                                //Maj uniquement s'il y a recalcul (donc tranche impactée partiellement ou totalement)
                                if ((remainingQuantity == 0) || (newBasisValue == 0))
                                {
                                    //Maj de l'assiette de calcul pour la tranche
                                    spheresId_FormulaValue1.Value = spheresId_FormulaValue1.Value.Replace(" Value: " + StrFunc.FmtDecimalToInvariantCulture(basisValue) + "]",
                                                                              " Value: " + StrFunc.FmtDecimalToInvariantCulture(newBasisValue) + "]");
                                    //Maj du montant de frais pour la tranche
                                    spheresId_FormulaValue1.Value = spheresId_FormulaValue1.Value.Replace("[Amount: " + StrFunc.FmtDecimalToInvariantCulture(paymentAmount) + "]",
                                                                              "[Amount: " + StrFunc.FmtDecimalToInvariantCulture(newPaymentAmount) + "]");
                                }
                                //Cumul des nouveaux montants des tranches
                                newTotalPaymentAmount += newPaymentAmount;
                            }

                            //Recacul du "coef", pour le recalcul des éventuelles taxes, sur la base du ratio du montant après/avant
                            coef = newTotalPaymentAmount / otherPartyPayment[i].PaymentAmount.Amount.DecValue;

                            //Set Amount
                            otherPartyPayment[i].PaymentAmount.Amount.DecValue = newTotalPaymentAmount;
                            #endregion
                        }
                        else
                        {
                            //Set Amount
                            cash = new EFS_Cash(CSTools.SetCacheOn(pCS), otherPartyPayment[i].PaymentAmount.Amount.DecValue * coef, otherPartyPayment[i].PaymentAmount.Currency);
                            otherPartyPayment[i].PaymentAmount.Amount.DecValue = cash.AmountRounded;
                        }
                        // RD 20120125 PL 20130910 Gestion du cas des frais barèmes disposant de tranches (_Bracket1)
                        // Mise à jour de l'assiette de calcul avec la quantité corrigée/transférée (Uniquement dans le cas d'une assiette de base de type Quantité)
                        string dataFeeSchedFormulaValue1 = string.Empty;
                        string dataFeeSchedFormulaValue2 = string.Empty; //PL 20141017
                        spheresId_FormulaValue1 = null;
                        ISpheresIdSchemeId spheresId_FormulaValue2 = null;
                        if (Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme))
                        {
                            spheresId_FormulaValue1 = otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme);
                            spheresId_FormulaValue2 = otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme);
                        }
                        else if (Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket" + "1"))
                        {
                            spheresId_FormulaValue1 = otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket" + "1");
                            spheresId_FormulaValue2 = otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme + "_Bracket" + "1");
                        }
                        if (spheresId_FormulaValue1 != null)
                            dataFeeSchedFormulaValue1 = spheresId_FormulaValue1.Value;
                        if (spheresId_FormulaValue2 != null)
                            dataFeeSchedFormulaValue2 = spheresId_FormulaValue2.Value;
                        //PL 20141017 
                        if (dataFeeSchedFormulaValue1.Contains("[Basis: " + Cst.AssessmentBasisEnum.Quantity))
                        {
                            if (Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme))
                                otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme).Value = StrFunc.FmtDecimalToInvariantCulture(quantity.DecValue);
                        }
                        if (dataFeeSchedFormulaValue2.Contains("[Basis: " + Cst.AssessmentBasisEnum.Quantity))
                        {
                            if (Tools.IsPaymentSourceScheme(otherPartyPayment[i], Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme))
                                otherPartyPayment[i].PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme).Value = StrFunc.FmtDecimalToInvariantCulture(quantity.DecValue);
                        }

                        #region TAX
                        if (otherPartyPayment[i].TaxSpecified)
                        {
                            for (int j = 0; j < ArrFunc.Count(otherPartyPayment[i].Tax); j++)
                            {
                                foreach (ITaxSchedule taxDetail in otherPartyPayment[i].Tax[j].TaxDetail)
                                {
                                    if (taxDetail.TaxAmountSpecified)
                                    {
                                        cash = new EFS_Cash(CSTools.SetCacheOn(pCS), taxDetail.TaxAmount.Amount.Amount.DecValue * coef, taxDetail.TaxAmount.Amount.Currency);
                                        taxDetail.TaxAmount.Amount.Amount.DecValue = cash.AmountRounded;
                                        //accountingAmount
                                        if (taxDetail.TaxAmount.AccountingAmountSpecified)
                                        {
                                            cash = new EFS_Cash(CSTools.SetCacheOn(pCS), taxDetail.TaxAmount.AccountingAmount.Amount.DecValue * coef, taxDetail.TaxAmount.AccountingAmount.Currency);
                                            taxDetail.TaxAmount.AccountingAmount.Amount.DecValue = cash.AmountRounded;
                                        }
                                        //issueAmount
                                        if (taxDetail.TaxAmount.IssueAmountSpecified)
                                        {
                                            cash = new EFS_Cash(CSTools.SetCacheOn(pCS), taxDetail.TaxAmount.IssueAmount.Amount.DecValue * coef, taxDetail.TaxAmount.IssueAmount.Currency);
                                            taxDetail.TaxAmount.IssueAmount.Amount.DecValue = cash.AmountRounded;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion TAX
                    }
                }
            }
        }
        #endregion Methods
    }
    #endregion TradeActionBase
    #region TradePositionCancelation
    /// <summary>
    /// Représente une correction de quantité
    /// </summary>
    public class TradePositionCancelation : TradeActionBase
    {
        #region Members
        #endregion

        #region Constructors
        public TradePositionCancelation() { }
        public TradePositionCancelation(SpheresIdentification pTradeIdentification, int pInitialQuantity, int pAvailableQuantity, DateTime pDate, string pNotes) :
            base(pTradeIdentification, pInitialQuantity, pAvailableQuantity, pDate, pNotes)
        {
            isFeeRestitution = new EFS_Boolean(false);
        }
        public TradePositionCancelation(string pCS, TradeInput pTradeInput) :
            base(pCS, pTradeInput, Cst.Capture.ModeEnum.PositionCancelation)
        {
            isFeeRestitution = new EFS_Boolean(false);
        }
        #endregion Constructors
    }
    #endregion TradePositionCancelation

    #region TradeDenOptionAsset
    // EG 20151102 [21465] New
    public class TradeDenOptionAsset
    {
        #region Members
        public decimal strikePrice;
        public string callPut;
        public Nullable<DateTime> maturity;
        public ExerciseStyleEnum exerciseStyle = default;
        public string exerciseType;
        public SettlMethodEnum settlementMethodEnum = default;
        public string settlementMethod;
        public string physicalFactor;
        public int daysToExpire;
        public TradeDenOptionUnderlyer underlyer;
        #endregion Members

        #region Accessors
        #region IsQuoteLoaded
        /// <summary>
        /// Obtient true s'il existe une cotation pour le sous-jacent
        /// </summary>
        public bool IsQuoteLoaded
        {
            get {return (null != underlyer) && underlyer.IsQuoteLoaded;}
        }
        #endregion IsQuoteLoaded
        #endregion Accessors

        #region Contructors
        public TradeDenOptionAsset()
        {
            underlyer = new TradeDenOptionUnderlyer();
        }
        #endregion Contructors

        #region Methods
        #region GetSettlementExtValue
        private string GetSettlementExtValue(string pCS, string pSettlementMethod)
        {
            string value = String.Empty;
            string extValue = String.Empty;
            string extAttrb = String.Empty;

            EnumTools.GetEnumInfos(pCS, "SettltMethodEnum", pSettlementMethod, ref value, ref extValue, ref extAttrb);

            return extValue;
        }
        #endregion GetSettlementExtValue
        #region Initialized
        public void Initialized(string pCS, ExchangeTradedDerivativeContainer pETD, DateTime pDtBusiness)
        {
            // StrikePrice
            strikePrice = pETD.AssetETD.StrikePrice;
            // CallPut
            callPut = pETD.AssetETD.PutCall_EnglishString;
            // Maturity
            maturity = pETD.AssetETD.Maturity_MaturityDate;
            // American|european
            exerciseStyle = StringToEnum.ConvertToExerciseStyleEnum(pETD.DerivativeContract.ExerciseStyle);
            exerciseType = Enum.GetName(typeof(ExerciseStyleEnum), exerciseStyle);
            // Settlement method
            settlementMethodEnum = pETD.SettlementMethod;
            settlementMethod = GetSettlementExtValue(pCS, pETD.DerivativeContract.SettlementMethod);

            // Factor
            physicalFactor = Cst.NotAvailable;
            if ((settlementMethodEnum == SettlMethodEnum.PhysicalSettlement) && pETD.AssetETD.Factor.HasValue)
                physicalFactor = Convert.ToString(pETD.AssetETD.Factor.Value);
            try
            {
                if (maturity.HasValue)
                {
                    // Days to expire
                    TimeSpan? delta = maturity.Value.Subtract(pDtBusiness);
                    if (delta.HasValue)
                        daysToExpire = delta.Value.Days;
                }
            }
            catch (ArgumentOutOfRangeException) { }


            // Initialisation du sous-jacent
            underlyer.Initialized(pCS, pETD, pDtBusiness);
        }
        #endregion Initialized
        #endregion Methods
    }
    #endregion TradeDenOptionAsset
    #region TradeDenOptionAction
    // EG 20151102 [21465] New
    // EG 20170127 Qty Long To Decimal
    public class TradeDenOptionAction
    {
        #region Members
        public long idT;
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal initialQuantity;
        public EFS_Decimal positionQuantity;
        public EFS_Decimal availableQuantity;
        public EFS_Integer idPR;
        public int nbIdPR;
        public EFS_Decimal denQuantity;
        public EFS_Decimal inputQuantity;
        public EFS_String denUser;
        public EFS_DateTime denDate;
        public EFS_Boolean denIsRemove;
        public EFS_String denStatus;
        public EFS_String denRequestMode;
        public Cst.PosRequestTypeEnum denPosRequestType;
        public EFS_String denRequestType;
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Contructors
        public TradeDenOptionAction()
        {
        }
        // EG 20170127 Qty Long To Decimal
        public TradeDenOptionAction(decimal pInitialQuantity, decimal pPositionQuantity, decimal pAvailableQuantity)
        {
            // EG 20170127 Qty Long To Decimal
            initialQuantity = new EFS_Decimal(pInitialQuantity);
            positionQuantity = new EFS_Decimal(pPositionQuantity);
            availableQuantity = new EFS_Decimal(pAvailableQuantity);
        }
        #endregion Contructors

        #region Methods
        #endregion Methods
    }
    #endregion TradeDenOptionAction
    #region TradeDenOptionUnderlyer
    // EG 20151102 [21465] New
    public class TradeDenOptionUnderlyer
    {
        #region Members
        /// <summary>
        /// Catégorie du sous-jacent
        /// </summary>
        public Cst.UnderlyingAsset underlyingAsset = default;
        /// <summary>
        /// Libellé Catégorie du sous-jacent
        /// </summary>
        public string assetCategory;
        /// <summary>
        /// Identifiant du sous-jacent
        /// </summary>
        public string identifier;
        /// <summary>
        /// Id du sous-jacent
        /// </summary>
        public int idAsset;
        /// <summary>
        /// DerivativeContract : Sous-jacent (si future)
        /// </summary>
        public SQL_DerivativeContract sqlDerivativeContract = null;
        /// <summary>
        /// Asset : Sous-jacent
        /// </summary>
        public SQL_AssetBase sqlAsset = null;
        /// <summary>
        /// Cours du sous-jacent
        /// </summary>
        public decimal quoteValue;
        /// <summary>
        /// Timing du cours du sous-jacent
        /// </summary>
        public string quoteTiming;
        /// <summary>
        /// Date du cours du sous-jacent
        /// </summary>
        public DateTime quoteTime;
        /// <summary>
        /// Source du cours sous-jacent
        /// </summary>
        public string quoteSource;

        private SQL_Quote underlyingQuotation = null;
        #endregion Members

        #region Accessors
        #region IsQuoteLoaded
        /// <summary>
        /// Obtient true s'il existe une cotation pour le sous-jacent
        /// </summary>
        public bool IsQuoteLoaded
        {
            get {return (null != underlyingQuotation) && underlyingQuotation.IsLoaded;}
        }
        #endregion IsQuoteLoaded
        #endregion Accessors

        #region Contructors
        public TradeDenOptionUnderlyer()
        {
        }
        #endregion Contructors

        #region Members
        #region Initialized
        public void Initialized(string pCS, ExchangeTradedDerivativeContainer pETD, DateTime pDtBusiness)
        {
            // Asset category
            underlyingAsset = Cst.ConvertToUnderlyingAsset(pETD.DerivativeContract.AssetCategory);
            assetCategory = Ressource.GetString(underlyingAsset.ToString(), true);

            // Underlyer Contract and Asset
            if (((Cst.UnderlyingAsset.Future == underlyingAsset) || (Cst.UnderlyingAsset.ExchangeTradedContract == underlyingAsset)) &&
                (0 < pETD.DerivativeContract.IdDcUnl))
            {
                sqlDerivativeContract = new SQL_DerivativeContract(pCS, pETD.DerivativeContract.IdDcUnl);
                if (sqlDerivativeContract.IsLoaded)
                    assetCategory = String.Concat(assetCategory, Cst.Space, sqlDerivativeContract.ContractSymbol);
            }

            // Underlyer asset id
            identifier = Cst.NotAvailable;
            idAsset = pETD.GetIdAssetUnderlyer();
            if (0 < idAsset)
            {
                sqlAsset = AssetTools.NewSQLAsset(pCS, underlyingAsset, idAsset);
                if (this.sqlAsset.IsLoaded)
                    identifier = sqlAsset.Identifier;

                // Underlyer Quotation
                QuotationSideEnum quoteSideRequest = QuotationSideEnum.OfficialClose;
                if (DtFunc.IsDateTimeFilled(pETD.AssetETD.Maturity_MaturityDate) && (0 == pETD.AssetETD.Maturity_MaturityDate.CompareTo(pDtBusiness)))
                    quoteSideRequest = QuotationSideEnum.OfficialSettlement;
                SetQuotation(pCS, pETD.ProductBase, pDtBusiness, quoteSideRequest);
            }
        }
        #endregion Initialized
        #region SetQuotation
        /// <summary>
        /// Alimente la quotation du sous-jacent
        /// </summary>
        private void SetQuotation(string pCS, IProductBase pProduct, DateTime pDtBusiness, QuotationSideEnum pQuoteSide)
        {
            quoteValue = 0;
            quoteTime = pDtBusiness;
            quoteSource = Cst.NotAvailable;
            quoteTiming = Cst.NotAvailable;

            if (0 < idAsset)
            {
                KeyQuote keyQuote = new KeyQuote(pCS, pDtBusiness, null, null, pQuoteSide, QuoteTimingEnum.Close)
                {
                    TimeOperator = "<="
                };
                QuoteEnum quoteEnum = AssetTools.ConvertUnderlyingAssetToQuoteEnum(underlyingAsset);

                underlyingQuotation = AssetTools.GetQuote(pCS, pProduct, keyQuote, quoteEnum, idAsset, SQL_Quote.OfficialSettlementBehaviorEnum.OfficialAll,
                    out  quoteValue, out quoteTime, out  quoteSource, out quoteTiming);
            }
        }
        #endregion SetQuotation
        #endregion Members
    }
    #endregion TradeDenOptionUnderlyer
    #region TradeDenOption
    /// <summary>
    /// Représente un dénouement d'option sur ExchangeTradedDerivative
    /// </summary>
    // EG 20151102 [21465] New Remplace ExeAssAbnOption
    // EG 20170127 Qty Long To Decimal
    public class TradeDenOption : TradeActionBase
    {
        #region Members
        // EG 20151102 [21465] Quantité en position
        /// <summary>
        /// Quantité en position
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal positionQuantity;
        /// <summary>
        /// Représente le mode de l'action (ITD|EOD)
        /// </summary>
        public EFS_String requestMode;
        /// <summary>
        /// Représente le type de laction (ABN|NEX|NAS|ASS|EXE)
        /// </summary>
        public Cst.PosRequestTypeEnum posRequestType = default;
        /// <summary>
        /// Caractéristique de l'actif source
        /// </summary>
        public TradeDenOptionAsset asset;

        public EFS_Boolean abandonRemaining;
        public EFS_String moneyPosition;
        public MoneyPositionEnum moneyPositionEnum = default;

        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal prevQuantity;

        public SettlSessIDEnum prevRequestMode;

        /// <summary>
        /// Représente le mode de dénouement
        /// </summary>
        public Cst.DenOptionActionType denOptionActionType;


        public EFS_String actionTitle;
        /// <summary>
        /// Liste des actions de dénouement du jour déjà présentes 
        /// </summary>
        public TradeDenOptionAction[] action;
        /// <summary>
        /// Frais associés à l'action
        /// </summary>
        [System.Xml.Serialization.XmlElement("otherPartyPayment")]
        public new Payment[] otherPartyPayment = null;

        private bool initialized = false;

        #endregion Members

        #region Accessors
        #region IsInitialized
        /// <summary>
        /// get the initialisation status of the object
        /// </summary>
        public bool IsInitialized
        {
            get { return initialized; }
        }
        #endregion IsInitialized
        #region IsActionCanBePerformed
        /// <summary>
        /// Obtient true si l'abandon, l'assignation ou l'exercice peut être exécuté en fonction de :
        /// la date de journée de bourse
        /// du type d'exercice
        /// de la date d'échéance de l'actif
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public bool IsActionCanBePerformed
        {
            get
            {
                bool ret = IsInitialized;
                if (ret)
                {
                    //Si la date de maturity est inconnue, l'action est autorisée (pas de contrôle)
                    // RD 20160315 [21995] Ajouter DtFunc.IsDateTimeFilled
                    if (asset.maturity != null && asset.maturity.HasValue && DtFunc.IsDateTimeFilled(asset.maturity.Value))
                        ret = (asset.maturity.Value >= this.date.DateValue);

                    if (ret)
                    {
                        switch (posRequestType)
                        {
                            case Cst.PosRequestTypeEnum.OptionExercise:
                            case Cst.PosRequestTypeEnum.OptionAssignment:
                                // FI 20181017 [24218] Assignation anticipée d'une position Option de type Européenne non autorisée avant l'échéance
                                // RD 20160315 [21995] Ajouter DtFunc.IsDateTimeFilled
                                if ((ExerciseStyleEnum.European == asset.exerciseStyle) && asset.maturity.HasValue && DtFunc.IsDateTimeFilled(asset.maturity.Value))
                                    ret = (asset.maturity.Value == this.date.DateValue);
                                break;
                            case Cst.PosRequestTypeEnum.OptionAbandon:
                            case Cst.PosRequestTypeEnum.OptionNotExercised:
                            case Cst.PosRequestTypeEnum.OptionNotAssigned:
                                break;
                            default:
                                throw new InvalidOperationException(StrFunc.AppendFormat("{0} is not implemented", posRequestType.ToString()));
                        }
                    }
                }
                return ret;
            }
        }
        #endregion IsActionCanBePerformed
        #region IsQuoteLoaded
        /// <summary>
        /// Obtient true s'il existe une cotation pour le sous-jacent
        /// </summary>
        public bool IsQuoteLoaded
        {
            get {return IsInitialized && (null != asset) && asset.IsQuoteLoaded;}
        }
        #endregion IsQuoteLoaded
        #region PosRequestMode
        /// <summary>
        /// Obtient ou définit le Mode (Intraday, EndOfDay)
        /// </summary>
        public SettlSessIDEnum PosRequestMode
        {
            get {return (SettlSessIDEnum)ReflectionTools.EnumParse(new SettlSessIDEnum(), requestMode.Value);}
            set {requestMode.Value = value.ToString();}
        }
        #endregion PosRequestMode
        #region SqlAsset
        /// <summary>
        /// Obtient l'asset du DC 
        /// </summary>
        public SQL_AssetBase UnderlyerSqlAsset
        {
            get {return asset.underlyer.sqlAsset;}
        }
        #endregion UnderlyerSqlAsset
        #region SqlDerivativeContract
        /// <summary>
        /// Obtient le DC du ss jacent lorsque ce dernier est un Future 
        /// </summary>
        public SQL_DerivativeContract SqlUnderlyingDC
        {
            get {return asset.underlyer.sqlDerivativeContract;}
        }
        #endregion SqlUnderlyingDC

        #endregion Accessors

        #region Constructors
        public TradeDenOption()
        {
        }
        #endregion Contructors

        #region Methods
        #region ActionLengthByRequestType
        public int ActionLengthByRequestType(Cst.PosRequestTypeEnum pPosRequestType)
        {
            int length = 0;
            if (ArrFunc.IsFilled(action))
            {
                action.ToList().ForEach(item =>
                {
                    if (item.denPosRequestType == pPosRequestType)
                        length++;
                });
            }
            return length;
        }
        #endregion ActionLengthByRequestType
        #region ConvertToPosRequest
        /// <summary>
        /// Converti le mode de saisie de l'action de dénouement en mode de demande de traitement de dénouement
        /// Cst.Capture.ModeEnum => Cst.PosRequestTypeEnum
        /// </summary>
        /// <param name="pMode"></param>
        /// <returns></returns>
        private Cst.PosRequestTypeEnum ConvertToPosRequest(Cst.Capture.ModeEnum pMode)
        {
            Cst.PosRequestTypeEnum ret = default;
            if (System.Enum.IsDefined(typeof(Cst.PosRequestTypeEnum), pMode.ToString()))
                ret = (Cst.PosRequestTypeEnum)System.Enum.Parse(typeof(Cst.PosRequestTypeEnum), pMode.ToString());
            return ret;
        }

        #endregion ConvertToPosRequest
        #region GetUniqueAction
        private TradeDenOptionAction GetUniqueAction(Cst.PosRequestTypeEnum pPosRequestType)
        {
            TradeDenOptionAction uniqueAction = null;
            if (1 == ActionLengthByRequestType(pPosRequestType))
                uniqueAction = action.ToList().Find(item => item.denPosRequestType == pPosRequestType);
            return uniqueAction;
        }

        #endregion GetUniqueAction
        #region IdPR_ForModifiedAction
        public int IdPR_ForModifiedAction(Cst.PosRequestTypeEnum pPosRequestType)
        {
            int idPR = 0;
            TradeDenOptionAction uniqueAction = GetUniqueAction(pPosRequestType);
            if (null != uniqueAction)
                idPR = uniqueAction.idPR.IntValue;
            return idPR;
        }
        #endregion IdPR_ForModifiedAction
        #region GetPreviousDenQty
        // EG 20170127 Qty Long To Decimal
        public decimal GetPreviousDenQty(Cst.PosRequestTypeEnum pPosRequestType)
        {
            decimal qty = 0;
            TradeDenOptionAction uniqueAction = GetUniqueAction(pPosRequestType);
            if (null != uniqueAction)
                qty = uniqueAction.denQuantity.DecValue;
            return qty;
        }
        #endregion GetPreviousDenQty


        #region SetUnderlyer
        /// <summary>
        /// Alimente les données du sous-jacent
        /// </summary>
        private void SetUnderlyer(string pCS, IExchangeTradedDerivative pProduct, DataDocumentContainer pDataDocument)
        {
            ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer(pCS, pProduct, pDataDocument);
            asset = new TradeDenOptionAsset();
            asset.Initialized(pCS, etd, DtBusiness);

            moneyPosition = new EFS_String(Cst.NotAvailable);
            moneyPositionEnum = default;

            if ((null != asset.underlyer) && (0 < asset.underlyer.idAsset))
            {
                if (null != asset.underlyer.sqlDerivativeContract)
                    asset.strikePrice = ExchangeTradedDerivativeTools.ConvertStrikeToUnderlyerBase(asset.strikePrice, asset.underlyer.underlyingAsset,
                        asset.underlyer.sqlDerivativeContract.InstrumentNum, asset.underlyer.sqlDerivativeContract.InstrumentDen);

                PutOrCallEnum putOrCallEnum = (PutOrCallEnum)ReflectionTools.EnumParse(new PutOrCallEnum(), asset.callPut);
                moneyPositionEnum = PosKeepingTools.GetMoneyPositionEnum(putOrCallEnum, asset.strikePrice, asset.underlyer.quoteValue);
                moneyPosition.Value = PosKeepingTools.GetMoneyPosition(moneyPositionEnum);
            }
        }
        #endregion SetUnderlyer
        #region Initialized
        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pIdT">Trade Id</param>
        /// <param name="pDtBusiness">Current clearing date</param>
        /// <param name="pProduct">The current trade product</param>
        /// <param name="pBaseProduct">The current trade product base, for SQL_Quote use</param>
        /// <param name="pMode">Current mode de consultation</param>
        public void Initialized(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pDtBusiness, 
            IExchangeTradedDerivative pProduct, Cst.Capture.ModeEnum pMode, DataDocumentContainer pDataDocument,
            Cst.DenOptionActionType pDenOptionActionType)
        {
            this.idT = pIdT;
            this.date = new EFS_Date
            {
                DateValue = pDtBusiness
            };
            this.abandonRemaining = new EFS_Boolean();
            this.note = string.Empty;
            this.posRequestType = ConvertToPosRequest(pMode);
            this.requestMode = new EFS_String(ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.Intraday));

            SetUnderlyer(pCS, pProduct, pDataDocument);

            denOptionActionType = pDenOptionActionType;
            InitQuantities(pCS, pDbTransaction, pIdT, denOptionActionType);

            initialized = true;
        }
        #endregion Init
        #region InitShortForm
        // EG 20170127 Qty Long To Decimal
        public void InitShortForm(DateTime pDtBusiness, decimal pQty)
        {
            this.date = new EFS_Date
            {
                DateValue = pDtBusiness
            };
            this.quantity = new EFS_Decimal(pQty);
        }
        #endregion InitShortForm
        #region InitQuantities
        /// <summary>
        /// Initialisation des quantités (initiale, en position, disponibles, dénouées du jour) 
        /// </summary>
        // EG 20151102 [21465] New
        // EG 20160314 Add @REQUESTTYPE = @REQUESTTYPE dans 2nde Query
        // FI 20160407 [22064] Modify
        // RD 20161031 [22570] Modify
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public void InitQuantities(string pCS, IDbTransaction pDbTransaction, int pIdT, Cst.DenOptionActionType _)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(posRequestType));
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), DtBusiness);

            string sqlSelect = @"select tr.IDT, tr.QTY, 
            tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY,0) as POSQTY, 
            tr.QTY - isnull(pab.AVAILABLEQTY,0) - isnull(pas.AVAILABLEQTY,0) as AVAILABLEQTY
            from dbo.TRADE tr
          
            /* On exclue les dénouements de même type du jour pour calculer la quantité available */
            left outer join 
            (   
	            select pad.IDT_BUY as IDT, sum(pad.QTY) as QTY, sum(case when (pa.DTBUSINESS = @DTBUSINESS) and (pr.REQUESTTYPE = @REQUESTTYPE) then 0 else pad.QTY end) as AVAILABLEQTY
	            from dbo.POSACTION pa
	            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
	            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA)
	            where (pa.DTBUSINESS <= @DTBUSINESS) and (pad.DTCAN is null or (pad.DTCAN > @DTBUSINESS))
	            group by pad.IDT_BUY
            ) pab  on (pab.IDT = tr.IDT)

            left outer join 
            (   
	            select pad.IDT_SELL as IDT, sum(pad.QTY) as QTY, sum(case when (pa.DTBUSINESS = @DTBUSINESS)  and (pr.REQUESTTYPE = @REQUESTTYPE) then 0 else pad.QTY end) as AVAILABLEQTY
	            from dbo.POSACTION pa
	            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
	            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA)
	            where (pa.DTBUSINESS <= @DTBUSINESS) and (pad.DTCAN is null or (pad.DTCAN > @DTBUSINESS))
	            group by pad.IDT_SELL
            ) pas  on (pas.IDT = tr.IDT)
            where (tr.IDT = @IDT);" + Cst.CrLf;


            sqlSelect += @"select isnull(pr.QTY,0) as DENQTY, pr.IDPR, ac.IDENTIFIER as DENUSER, isnull(pr.DTUPD,pr.DTINS) as DENDATE,
            pr.REQUESTMODE as DENREQUESTMODE, pr.STATUS as DENSTATUS, pr.REQUESTTYPE as DENREQUESTTYPE
            from dbo.TRADE tr
            inner join POSREQUEST pr on (pr.IDT = tr.IDT) and (pr.STATUS in ('PENDING', 'WARNING', 'SUCCESS')) and (pr.DTBUSINESS = @DTBUSINESS) 
            and (@REQUESTTYPE = @REQUESTTYPE)
            and (pr.REQUESTTYPE in ('ASS','ABN','NEX','NAS','EXE'))
            inner join ACTOR ac on (ac.IDA = isnull(pr.IDAUPD,pr.IDAINS)) 
            where (tr.IDT = @IDT)
            order by DENDATE desc;" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
            DataSet ds;
            if (null != pDbTransaction)
            {
                ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            else
            {
                // PL 20180312 WARNING: Use Read Commited !
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParameters, 480);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, 480);
            }

            if (null != ds)
            {
                ArrayList aDenOptionAction = new ArrayList();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    // EG 20170127 Qty Long To Decimal
                    initialQuantity = new EFS_Decimal(Convert.ToDecimal(row["QTY"]));
                    positionQuantity = new EFS_Decimal(Convert.ToDecimal(row["POSQTY"]));
                    availableQuantity = new EFS_Decimal(Convert.ToDecimal(row["AVAILABLEQTY"]));
                    quantity = new EFS_Decimal(positionQuantity.DecValue);
                    prevQuantity = 0;
                    foreach (DataRow row2 in ds.Tables[1].Rows)
                    {
                        // EG 20170127 Qty Long To Decimal
                        decimal denQuantity = Convert.ToDecimal(row2["DENQTY"]);

                        //FI/EG 20160407 [22064] Reecriture de la condition isAdded 
                        //bool isAdded = ((requestMode == SettlSessIDEnum.Intraday) && (0 < denQuantity)) || (requestMode == SettlSessIDEnum.EndOfDay);
                        //Annulation des actions du jour (autre que annulation) en succès ou 
                        //Annulation des actions EOD en attente 
                        // RD 20161031 [22570] Annulation des actions de dénouement avec le statut Warning
                        // bool isAdded = (0 < denQuantity) && LevelStatusTools.IsStatusSuccess(row2["DENSTATUS"].ToString());
                        bool isAdded = (0 < denQuantity) && 
                            (LevelStatusTools.IsStatusSuccess(row2["DENSTATUS"].ToString()) || LevelStatusTools.IsLevelWarning(row2["DENSTATUS"].ToString()));
                        isAdded |= LevelStatusTools.IsStatusPending(row2["DENSTATUS"].ToString());
                        
                        if (isAdded)
                        {
                            // EG 20170127 Qty Long To Decimal
                            TradeDenOptionAction _denOptionAction = new TradeDenOptionAction(initialQuantity.DecValue, positionQuantity.DecValue, availableQuantity.DecValue)
                            {
                                idPR = new EFS_Integer(Convert.ToInt32(row2["IDPR"])),
                                // EG 20170127 Qty Long To Decimal
                                denQuantity = new EFS_Decimal(Convert.ToDecimal(row2["DENQTY"])),
                                denUser = new EFS_String(Convert.ToString(row2["DENUSER"])),
                                denDate = new EFS_DateTime()
                            };
                            // FI 20200820 [25468] Date system en UTC
                            _denOptionAction.denDate.DateTimeValue = DateTime.SpecifyKind(Convert.ToDateTime(row2["DENDATE"]), DateTimeKind.Utc);
                            _denOptionAction.denIsRemove = new EFS_Boolean(); 
                            _denOptionAction.denStatus = new EFS_String(Ressource.GetString(Convert.ToString(row2["DENSTATUS"])));
                            _denOptionAction.denRequestMode = new EFS_String(Ressource.GetString(Convert.ToString(row2["DENREQUESTMODE"])));
                            _denOptionAction.denPosRequestType = (Cst.PosRequestTypeEnum) ReflectionTools.EnumParse(new Cst.PosRequestTypeEnum(), Convert.ToString(row2["DENREQUESTTYPE"]));
                            _denOptionAction.denRequestType = new EFS_String(Ressource.GetString(Convert.ToString(_denOptionAction.denPosRequestType)));
                            _denOptionAction.inputQuantity = new EFS_Decimal(0);
                            aDenOptionAction.Add(_denOptionAction);
                            // EG 20170127 Qty Long To Decimal
                            prevQuantity += _denOptionAction.denQuantity.DecValue;
                        }
                    }
                }
                if (0 < aDenOptionAction.Count)
                    action = (TradeDenOptionAction[])aDenOptionAction.ToArray(typeof(TradeDenOptionAction));
            }
        }
        #endregion InitQuantities
        #endregion Methods
    }
    #endregion TradeDenOption

    #region SafekeepingAction
    /// <summary>
    /// Class containing the business data to perform an safekeeping calculation (EOD processing)
    /// </summary>
    // EG 20150708 [SKP] New
    // EG 20190308 Upd [VCL migration] errReadOfficialClose
    public class SafekeepingAction
    {
        #region Members
        /// <summary>
        /// Représente la quantité disponible en date de settlement
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal quantity;
        /// <summary>
        /// Représente la DTBUSINESS courante
        /// </summary>
        public EFS_Date dtBusiness;
        /// <summary>
        /// Représente la date fin de période pour le calcul du SKP (DTBUSINESS)
        /// </summary>
        public EFS_Date dtStartPeriod;
        /// <summary>
        /// Représente la date fin de période pour le calcul du SKP (DTBUSINESSNEXT)
        /// </summary>
        public EFS_Date dtEndPeriod;
        /// <summary>
        /// Représente le prix de clôture de l'actif
        /// </summary>
        public Quote closingPrice;

        public bool marketValueSpecified = false;
        /// <summary>
        /// Représente le MarketValue du jour
        /// </summary>
        public IMoney marketValue;
        public decimal marketValueQty;
        public bool marketValueDataSpecified = false;
        public MarketValueData marketValueData;

        public bool paymentSpecified = false;
        /// <summary>
        /// Frais associés à l'action
        /// </summary>
        [System.Xml.Serialization.XmlElement("payment")]
        public FpML.v44.Shared.Payment[] payment = null;
        /// <summary>
        /// Sauvegarde Message Erreur cotation
        /// </summary>
        public bool errReadOfficialCloseSpecified = false;
        public SystemMSGInfo errReadOfficialClose;
        #endregion
        #region Accessors
        public bool IsErrorQuote
        {
            get 
            {
                return (errReadOfficialCloseSpecified || (null == closingPrice) || (false == closingPrice.valueSpecified)) && (0 < quantity.DecValue);
            }
        }
        #endregion Accessors
        #region Constructor
        /// <summary>   
        /// Alimentation des données nécessaires au calcul de frais de garde 
        /// </summary>
        public SafekeepingAction()
        {
        }
        #endregion Constructor
    }
    #endregion SafekeepingAction
    #region TradePositionTransfer
    /// <summary>
    /// Class containing the business data to perform a transfer of related to an allocation
    /// </summary>
    public class TradePositionTransfer : TradeActionBase
    {
        #region Members
        public IMoney InitialAccruedInterestAmount { get; private set; }
        public IMoney InitialGrossAmount { get; private set; }
        #endregion Members
        #region accessors

        public override TradeActionBase.MaxQuantityEnum MaxQuantity
        {
            get { return TradeActionBase.MaxQuantityEnum.InitialQuantity; }
        }
        #endregion

        #region Constructors
        public TradePositionTransfer() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public TradePositionTransfer(SpheresIdentification pTradeIdentification, decimal pInitialQty, decimal pAvailableQuantity, DateTime pDate, string pNotes) :
            base(pTradeIdentification, pInitialQty, pAvailableQuantity, pDate, pNotes)
        {
            isFeeRestitution = new EFS_Boolean(false);
        }
        public TradePositionTransfer(string pCS, TradeInput pTradeInput) :
            base(pCS, pTradeInput, Cst.Capture.ModeEnum.PositionTransfer)
        {
            isFeeRestitution = new EFS_Boolean(false);
            if (pTradeInput.DataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                IEquitySecurityTransaction est = pTradeInput.DataDocument.CurrentProduct.Product as IEquitySecurityTransaction;
                // GAM Source
                InitialGrossAmount = est.GrossAmount.PaymentAmount;
            }
            else if (pTradeInput.DataDocument.CurrentProduct.IsDebtSecurityTransaction)
            {
                IDebtSecurityTransaction dst = pTradeInput.DataDocument.CurrentProduct.Product as IDebtSecurityTransaction;
                // GAM Source
                InitialGrossAmount = dst.GrossAmount.PaymentAmount;
                if (dst.Price.AccruedInterestAmountSpecified)
                    InitialAccruedInterestAmount = dst.Price.AccruedInterestAmount.Clone();
            }
        }
        #endregion Constructors

        #region Method

        #endregion
    }
    #endregion TradePositionTransfer
    #region TradeRemoveAllocation
    /// <summary>
    /// Class containing the business data to perform an annulation of allocation
    /// </summary>
    public class TradeRemoveAllocation : TradeActionBase
    {
        #region accessors
        public override TradeActionBase.MaxQuantityEnum MaxQuantity
        {
            get
            {
                return TradeActionBase.MaxQuantityEnum.InitialQuantity;
            }
        }
        #endregion

        #region Constructors
        public TradeRemoveAllocation() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public TradeRemoveAllocation(SpheresIdentification pTradeIdentification, decimal pInitialQty, decimal pAvailableQuantity, DateTime pDate, string pNotes) :
            base(pTradeIdentification, pInitialQty, pAvailableQuantity, pDate, pNotes)
        {
        }
        public TradeRemoveAllocation(string pCS, TradeInput pTradeInput) :
            base(pCS, pTradeInput, Cst.Capture.ModeEnum.RemoveAllocation)
        {
        }
        #endregion Constructors
    }
    #endregion TradeRemoveAllocation
    #region TradeSplit
    /// <summary>
    /// Class containing the business data to perform a split
    /// </summary>
    public class TradeSplit : TradeActionBase
    {
        #region Accessors
        //public override TradeETDActionBase.MaxQuantityEnum maxQuanity
        //{
        //    get { return TradeETDActionBase.MaxQuantityEnum.InitialQuantity; }
        //}
        public ArrayList AlNewTrades
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public TradeSplit() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public TradeSplit(SpheresIdentification pTradeIdentification, decimal pInitialQty, DateTime pDate) :
            base(pTradeIdentification, pInitialQty, pInitialQty, pDate, null)
        {
            AlNewTrades = new ArrayList();
        }
        public TradeSplit(string pCS, TradeInput pTradeInput) :
            base(pCS, pTradeInput, Cst.Capture.ModeEnum.TradeSplitting)
        {
            AlNewTrades = new ArrayList();
        }
        #endregion Constructors
        #region Methods
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public void AddNewTrade(string pIdStActivation, string pActorIdentifier, int pIdA, string pBookIdentifier, int pIdB, decimal pQty, string pPosEfct)
        {
            EfsML.v30.PosRequest.SplitNewTrade newTrade = new EfsML.v30.PosRequest.SplitNewTrade(pIdStActivation, pActorIdentifier, pIdA, pBookIdentifier, pIdB, pQty, pPosEfct);
            AlNewTrades.Add(newTrade);
        }
        #endregion Methods

    }
    #endregion TradeSplit
    #region TradeUnderlyingDelivery
    /// <summary>
    /// Class containing the business data to perform a underlying Delivery (Future only)
    /// </summary>
    public class TradeUnderlyingDelivery
    {
        #region Members
        /// <summary>
        /// Représente le trade qui subit l'action
        /// </summary>
        public SpheresIdentification tradeIdentification;
        /// <summary>
        /// Représente la quantité impliquée dans l'action (qté MOF)
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal quantity;
        /// <summary>
        /// Date de l'échéance
        /// </summary>
        public EFS_Date maturityDate;
        /// <summary>
        /// Méthode de livraison
        /// </summary>
        public EFS_String settlMethod;
        public SettlMethodEnum settlementMethodEnum = default;
        /// <summary>
        /// Factor
        /// </summary>
        public EFS_String physicalFactor;
        /// <summary>
        /// Underlying asset category
        /// </summary>
        public EFS_String unlAssetCategory;
        /// <summary>
        /// Date de la nouvelle l'étape de livraison
        /// </summary>
        public EFS_Date date;
        /// <summary>
        /// Nouvelle étape de livraison
        /// </summary>
        public string deliveryStep;
        /// <summary>
        /// Etape de livraison actuelle
        /// </summary>
        public string currentDeliveryStep;

        private readonly DateTime m_CurrentBusinessDate = DateTime.MinValue;
        private readonly Dictionary<string, DateTime> m_StepDates = null;
        private int m_idEventMOF = 0;
        private UnderlyingDeliveryStepEnum m_CurrentStepEnum = UnderlyingDeliveryStepEnum.NA;
        #endregion Members

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpheresIdentification"></param>
        /// <param name="pDate"></param>
        public TradeUnderlyingDelivery(SpheresIdentification pSpheresIdentification, DateTime pDate)
        {
            m_StepDates = new Dictionary<string, DateTime>();
            m_CurrentBusinessDate = pDate;
            tradeIdentification = pSpheresIdentification;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Init the object
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pIdT">Trade Id</param>
        /// <param name="pProduct">The current trade product</param>
        /// <param name="pBaseProduct">The current trade product base, for SQL_Quote use</param>
        public void Init(string pCS, int pIdT, IExchangeTradedDerivative pProduct, DataDocumentContainer pDataDocument)
        {
            // Building the container of the product
            ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer(pCS, pProduct, pDataDocument);

            // Underlying Asset category
            string unlAssetCategory = etd.DerivativeContract.AssetCategory;

            // Settlement method
            SettlMethodEnum settlementMethodEnum = etd.SettlementMethod;
            string extCode = String.Empty;
            string extValue = String.Empty;
            string extAttrb = String.Empty;
            EnumTools.GetEnumInfos(pCS, "SettltMethodEnum", etd.DerivativeContract.SettlementMethod, ref extCode, ref extValue, ref extAttrb);
            string settlement = Ressource.GetString(extValue, true);

            // Factor
            string factor = Cst.NotAvailable;
            if ((settlementMethodEnum == SettlMethodEnum.PhysicalSettlement) && etd.AssetETD.Factor.HasValue)
            {
                factor = Convert.ToString(etd.AssetETD.Factor.Value);
            }

            /////////////////////////////
            // INIT THE BUSINESS CLASS //
            /////////////////////////////
            this.settlMethod = new EFS_String(settlement);
            this.settlementMethodEnum = settlementMethodEnum;
            this.physicalFactor = new EFS_String(factor);
            this.unlAssetCategory = new EFS_String(unlAssetCategory);
            // quantity
            GetMaturityOffSettingInfo(pCS, pIdT);
            m_CurrentStepEnum = CurrentDeliveryStep();
            currentDeliveryStep = Ressource.GetString(m_CurrentStepEnum.ToString(), true);
            // date
            if ((m_StepDates != default(Dictionary<string, DateTime>))
                && (m_StepDates.Count > 0))
            {
                // Recherche de la date la plus grande des étapes actuelles de livraison
                DateTime dtStep = m_StepDates.Max(step => step.Value);
                // Si la date de compensation courante est supérieure à la la plus grande date des étapes actuelles de livraison
                if (m_CurrentBusinessDate > dtStep)
                {
                    dtStep = m_CurrentBusinessDate;
                }
                date = new EFS_Date(DtFunc.DateTimeToString(dtStep, DtFunc.FmtISODate));
            }
        }

        /// <summary>
        /// Recherche de l'étape actuelle de livraison
        /// </summary>
        /// <returns></returns>
        private UnderlyingDeliveryStepEnum CurrentDeliveryStep()
        {
            UnderlyingDeliveryStepEnum currentIMStep = UnderlyingDeliveryStepEnum.NA;
            if ((m_StepDates != default(Dictionary<string, DateTime>)) && (m_StepDates.Count > 0))
            {
                if (m_StepDates.TryGetValue(EventClassFunc.PhysicalSettlement, out _))
                {
                    // Recherche date de fin de la pré-livraison
                    if (m_StepDates.TryGetValue(EventClassFunc.DeliveryDelay, out DateTime currentStepDate))
                    {
                        currentIMStep = UnderlyingDeliveryStepEnum.PreDelivery;
                        if (currentStepDate <= m_CurrentBusinessDate) // La pré-livraison est dépassée
                        {
                            // Passage en phase de livraison
                            currentIMStep = UnderlyingDeliveryStepEnum.Delivery;
                            //
                            // Recherche date de fin de la livraison avec facturation en 2 étapes
                            if (m_StepDates.TryGetValue(EventClassFunc.PreSettlement, out currentStepDate))
                            {
                                if (currentStepDate <= m_CurrentBusinessDate) // La livraison avec facturation en 2 étapes est dépassée
                                {
                                    currentIMStep = UnderlyingDeliveryStepEnum.DeliveredWithInvoicingInTwoSteps;
                                }
                            }
                            // Recherche date de fin de la livraison
                            if (m_StepDates.TryGetValue(EventClassFunc.Settlement, out currentStepDate))
                            {
                                if (currentStepDate <= m_CurrentBusinessDate) // La date finale de livraison est dépassée
                                {
                                    currentIMStep = UnderlyingDeliveryStepEnum.Delivered;
                                }
                            }
                        }
                    }
                }
            }
            return currentIMStep;
        }

        /// <summary>
        /// Lecture des informations sur la maturity offseting
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pIdT">Trade Id</param>
        // EG 20190926 Upd Refactoring Cst.PosRequestTypeEnum
        private void GetMaturityOffSettingInfo(string pCS, int pIdT)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            #region sql Select
            string sqlSelect = SQLCst.SELECT + "e.IDE, e.VALORISATION as QTY, ec.EVENTCLASS, ec.DTEVENT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec " + SQLCst.ON + " (ec.IDE = e.IDE)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(e.EVENTCODE = " + DataHelper.SQLString(EventCodeFunc.MaturityOffsettingFuture) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = " + DataHelper.SQLString(Cst.StatusActivation.REGULAR.ToString()) + ")" + Cst.CrLf;
            #endregion sql Select
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dataParameters);
            IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (dr.Read())
            {
                m_idEventMOF = Convert.ToInt32(dr["IDE"]);
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                quantity = new EFS_Decimal(Convert.ToDecimal(dr["QTY"]));
                do
                {
                    string eventClass = Convert.ToString(dr["EVENTCLASS"]);
                    DateTime dtEvent = Convert.ToDateTime(dr["DTEVENT"]);
                    m_StepDates.Add(eventClass, dtEvent);
                }
                while (dr.Read());
            }
            else
            {
                // EG 20170127 Qty Long To Decimal
                quantity = new EFS_Decimal(0);
            }
        }

        /// <summary>
        /// Ecriture de la nouvelle étape de livraison
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <returns></returns>
        public Cst.ErrLevel RecordDeliveryStep(string pCS)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            EventClass evtClass = new EventClass
            {
                idE = m_idEventMOF,
                dtEvent = date
            };
            // Date de l'EventClass
            if (m_CurrentBusinessDate > date.DateValue)
            {
                evtClass.dtEvent = new EFS_Date(DtFunc.DateTimeToString(m_CurrentBusinessDate, DtFunc.FmtISODate));
            }
            if (m_StepDates.TryGetValue(EventClassFunc.DeliveryDelay, out DateTime existingDate))
            {
                if (existingDate > date.DateValue)
                {
                    evtClass.dtEvent = new EFS_Date(DtFunc.DateTimeToString(existingDate, DtFunc.FmtISODate));
                }
            }
            // Code de l'EventClass en fonction de l'étape de livraison
            if (deliveryStep == UnderlyingDeliveryStepEnum.Delivered.ToString())
            {
                evtClass.code = EventClassFunc.Settlement;
                if (m_StepDates.TryGetValue(EventClassFunc.PreSettlement, out existingDate))
                {
                    if (existingDate > date.DateValue)
                    {
                        evtClass.dtEvent = new EFS_Date(DtFunc.DateTimeToString(existingDate, DtFunc.FmtISODate));
                    }
                }
            }
            else if (deliveryStep == UnderlyingDeliveryStepEnum.DeliveredWithInvoicingInTwoSteps.ToString())
            {
                evtClass.code = EventClassFunc.PreSettlement;
                if (m_StepDates.TryGetValue(EventClassFunc.Settlement, out existingDate))
                {
                    if (existingDate < date.DateValue)
                    {
                        evtClass.dtEvent = new EFS_Date(DtFunc.DateTimeToString(existingDate, DtFunc.FmtISODate));
                    }
                }
            }
            else
            {
                errLevel = Cst.ErrLevel.DATAREJECTED;
            }
            if (errLevel == Cst.ErrLevel.SUCCESS)
            {
                if (0 < DateTime.Compare(evtClass.dtEvent.DateValue, OTCmlHelper.GetDateSys(pCS).Date))
                {
                    evtClass.dtEventForced = evtClass.dtEvent;
                }
                else
                {
                    evtClass.dtEventForced = new EFS_Date(DtFunc.DateTimeToString(OTCmlHelper.GetDateSys(pCS).Date, DtFunc.FmtISODate));
                }
                evtClass.isPayment = new EFS_Boolean(false);
                // Maj ou Insert EventClass
                if (m_StepDates.TryGetValue(evtClass.code, out DateTime previousDtEvent))
                {
                    errLevel = UpdateEventClass(pCS, evtClass, previousDtEvent);
                }
                else
                {
                    errLevel = InsertEventClass(pCS, evtClass);
                }
            }
            return errLevel;
        }

        /// <summary>
        /// Insert un nouvel Event Class sur un événement existant
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pEvtClass">EventClass contenant les données à insérer</param>
        /// <returns></returns>
        private Cst.ErrLevel InsertEventClass(string pCS, EventClass pEvtClass)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDE), pEvtClass.idE);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCLASS), pEvtClass.code);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), pEvtClass.dtEvent.DateTimeValue);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENTFORCED), pEvtClass.dtEventForced.DateTimeValue);
            dataParameters.Add(new DataParameter(pCS, "ISPAYMENT", DbType.Boolean), pEvtClass.isPayment.BoolValue);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXTLLINK), null);
            #region sql Select
            string sqlSelect = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + Cst.CrLf;
            sqlSelect += "( IDE, EVENTCLASS, DTEVENT, DTEVENTFORCED, ISPAYMENT, EXTLLINK )" + Cst.CrLf;
            sqlSelect += SQLCst.VALUES + Cst.CrLf;
            sqlSelect += "(@IDE, @EVENTCLASS, @DTEVENT, @DTEVENTFORCED, @ISPAYMENT, @EXTLLINK)";
            #endregion sql Select
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dataParameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (rowAffected != 1)
            {
                errLevel = Cst.ErrLevel.SQL_ERROR;
            }
            return errLevel;
        }
        /// <summary>
        /// Mise à jour d'un nouvel Event Class existant sur un événement existant
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pEvtClass">EventClass contenant les données à mettre à jour</param>
        /// <param name="pPreviousDtEvent">Date de l'EventClass à mettre à jour</param>
        /// <returns></returns>
        private Cst.ErrLevel UpdateEventClass(string pCS, EventClass pEvtClass, DateTime pPreviousDtEvent)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDE), pEvtClass.idE);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCLASS), pEvtClass.code);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), pEvtClass.dtEvent.DateTimeValue);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENTFORCED), pEvtClass.dtEventForced.DateTimeValue);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pPreviousDtEvent);
            #region sql Select
            string sqlSelect = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.SET + "DTEVENT = @DTEVENT, DTEVENTFORCED = @DTEVENTFORCED" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(IDE = @IDE)" + SQLCst.AND + "(EVENTCLASS = @EVENTCLASS)" + SQLCst.AND + "(DTEVENT = @DT)";
            #endregion sql Select
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dataParameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (rowAffected != 1)
            {
                errLevel = Cst.ErrLevel.SQL_ERROR;
            }
            return errLevel;
        }
        #endregion Methods
    }
    #endregion TradeUnderlyingDelivery

    // EG 20180514 [23812] Report
    public class TradeFxOptionEarlyTermination
    {
        #region Members

        public EFS_DateTime actionDate;
        public EFS_Date valueDate;

        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney cashSettlement;
        public EFS_Date settlementDate;

        public bool noteSpecified;
        public string note;


        public Pair<int, Nullable<int>> IdPayer
        {
            private set;
            get;
        }

        public Pair<int, Nullable<int>> IdReceiver
        {
            private set;
            get;
        }





        public void SetPayerReceiver(DataDocumentContainer pDoc)
        {
            //Payer
            IParty payer = pDoc.GetParty(payerPartyReference.HRef);
            IBookId bookPayer = pDoc.GetBookId(payerPartyReference.HRef);
            IdPayer = new Pair<int, int?>(payer.OTCmlId, (null != bookPayer) ? bookPayer.OTCmlId : new Nullable<int>());

            //Receiver
            IParty receiver = pDoc.GetParty(receiverPartyReference.HRef);
            IBookId bookReceiver = pDoc.GetBookId(receiverPartyReference.HRef);
            IdReceiver = new Pair<int, int?>(receiver.OTCmlId, (null != bookReceiver) ? bookReceiver.OTCmlId : new Nullable<int>());

        }



        #endregion Members

    }

    /// <summary>
    /// Classe de réception des éléments de frais déjà facturés (EVT) sur un trade
    /// Utilisé pour comparaison et "matchage" avec ligne de frais dans OtherPartyPayment du trade
    /// </summary>
    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
    // EG 20240210 [WI816] Trade input: Correctifs (status = Nullable<SpheresSourceStatusEnum>)
    public class TradeFeeInvoiced
    {
        public int idE;
        public (int idA, Nullable<int> idB) payer;
        public (int idA, Nullable<int> idB) receiver;
        public (DateTime date, decimal amount, string currency) payment;
        public Nullable<SpheresSourceStatusEnum> status;
        public string eventType;

        public TradeFeeInvoiced() { }
    }
}