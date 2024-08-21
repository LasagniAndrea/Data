#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;

using EFS.Status;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;


#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Class charge de l'alimentation du dataDocument via les ccis (contexte trade de marché execution,allocation)
    /// </summary>
    // EG 20180514 [23812] Report
    public class TradeInput : TradeCommonInput
    {
        /// <summary>
        /// Représente les élements qui contiennent des otherPartyPayments ou des additionnalPayments  
        /// <para>Ces derniers sont potentiellement alimentés à partir des barèmes.</para>
        /// </summary>
        public enum FeeTarget
        {
            /// <summary>
            /// Représente un trade
            /// </summary>
            trade,
            /// <summary>
            /// Représente l'exercice ou l'assignation ou l'abandon
            /// <para>Il n'existe pas de ADP sur cet élément</para>
            /// </summary>
            denOption,
            /// <summary>
            /// Représente un transfert de portefeuille
            /// </summary>
            positionTransfer,
            /// <summary>
            /// il n'existe pas des otherPartyPayments ou des additionnalPayments
            /// </summary>
            none
        }
        /// <param name="pIsResetOnlyFeesFromSchedule">Si true: Suppression uniquement des additionalPayment et otherPartyPayment issus d'un barèmes de frais</param>
        /// <param name="pisResetEmptyPayment">Si true: Suppression uniquement des additionalPayment et otherPartyPayment non renseignés</param>

        /// <summary>
        /// Représente les modes de Reset des otherPartyPayments ou des additionnalPayments  
        /// </summary>
        // EG 20201009 Ajout d'un FlagsAttribute pour simplification d'utilisation du mode.
        [Flags]
        public enum ClearFeeMode
        {
            Corrected = 0x1,
            FromSchedule = 0x2,
            Manual = 0x4,
            MissingData = 0x8,
            All = Corrected + FromSchedule + Manual + MissingData,
        }

        #region Members
        private bool _isTradeCancelable;
        private bool _isTradeExtendible;
        private bool _isTradeMandatoryEarlyTermination;
        private bool _isTradeOptionalEarlyTermination;
        private bool _isTradeStepUp;

        /// <summary>
        /// Représente les directives pour les frais de garde (Safekeeping fees)
        /// <para>Ce membre est alimenté uniquement lors du traitement EOD</para>
        /// </summary>
        // EG 20150708 [21103] New
        [System.Xml.Serialization.XmlElement("safeKeepingAction")]
        public SafekeepingAction safekeepingAction;

        /// <summary>
        /// Représente les directives pour les exercices, assignations etc sur ETD [Alloc uniquement]
        /// <para>Ce membre est alimenté uniquement lors d'une action exercice, assignation, abandon etc</para>
        /// </summary>
        // EG 20151102 [21465] Upd
        [System.Xml.Serialization.XmlElement("denOption")]
        public TradeDenOption tradeDenOption;


        /// <summary>
        /// Représente les directives de correction de position [Alloc uniquement]
        /// <para>Ce membre est alimenté uniquement lors d'une correction de position</para>
        /// </summary>
        public TradePositionCancelation positionCancel;

        /// <summary>
        /// Représente les directives de transfert de position [Alloc uniquement]
        /// <para>Ce membre est alimenté uniquement lors d'un transfert de position</para>
        /// </summary>
        public TradePositionTransfer positionTransfer;

        /// <summary>
        /// Représente les directives d'annulation de position [Alloc uniquement]
        /// <para>Ce membre est alimenté uniquement lors d'une annulation</para>
        /// </summary>
        public TradeRemoveAllocation removeAllocation;

        /// <summary>
        /// Représente les directives de split de trades [Alloc uniquement]
        /// <para>Ce membre est alimenté uniquement lors d'un split</para>
        /// </summary>
        public TradeSplit split;

        /// <summary>
        /// Représente les directives de livraison du sosu jacent de trades [ETD Alloc uniquement de type Future]
        /// <para>Ce membre est alimenté uniquement lors d'une action de livraison</para>
        /// </summary>
        public TradeUnderlyingDelivery underlyingDelivery;

        public TradeFxOptionEarlyTermination fxOptionEarlyTermination;
        /// <summary>
        /// Représente les lignes de frais des trades de marché déjà facturés
        /// </summary>
        /// <para>Ce membre est alimenté uniquement lors d'une action de modification des frais non facturés</para>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public IEnumerable<TradeFeeInvoiced> feesAlreadyInvoiced;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient ou définit la collection de cci
        /// </summary>
        public new TradeCustomCaptureInfos CustomCaptureInfos
        {
            set { base.CustomCaptureInfos = value; }
            get { return (TradeCustomCaptureInfos)base.CustomCaptureInfos; }
        }

        #region Provisions
        #region IsTradeExtendible
        /// <summary>
        /// Obtient true si le trade est extensible
        /// <para>S'il existe un eventType EXT</para>
        /// </summary>
        public bool IsTradeExtendible
        {
            get { return _isTradeExtendible; }
        }
        #endregion IsTradeExtendible
        #region IsTradeCancelable
        /// <summary>
        /// Obtient true si le trade est extensible
        /// <para>S'il existe un eventType CAN</para>
        /// </summary>
        public bool IsTradeCancelable
        {
            get { return _isTradeCancelable; }
        }
        #endregion IsTradeCancelable
        #region IsTradeMandatoryEarlyTermination
        /// <summary>
        /// Obtient true si le trade est extensible
        /// <para>S'il existe un eventType MET</para>
        /// </summary>
        public bool IsTradeMandatoryEarlyTermination
        {
            get { return _isTradeMandatoryEarlyTermination; }
        }
        #endregion IsTradeMandatoryEarlyTermination
        #region IsTradeStepUp
        public bool IsTradeStepUp
        {
            get { return _isTradeStepUp; }
        }
        #endregion IsTradeStepUp
        #region IsTradeOptionalEarlyTermination
        public bool IsTradeOptionalEarlyTermination
        {
            get { return _isTradeOptionalEarlyTermination; }
        }
        #endregion IsTradeOptionalEarlyTermination
        #region IsTradeTerminationAuthorized
        public bool IsTradeTerminationAuthorized
        {
            get { return IsTradeCancelable || IsTradeMandatoryEarlyTermination || IsTradeOptionalEarlyTermination; }
        }
        #endregion IsTradeTerminationAuthorized
        #endregion Provisions

        /// <summary>
        /// Obtient true si l'abandon est permis sur le trade
        /// <para>Remarque: Si le trade est une vente il ne pourra être abandonné que si l'acheteur abandonne</para>
        /// </summary>
        /// EG 20140708 Add Test isEQD
        /// EG 20150423 [20513] BANCAPERTA
        public bool IsAbandonAuthorized
        {
            get
            {

                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                //
                if (ret)
                {
                    ret = DataDocument.CurrentProduct.IsFxAverageRateOption ||
                        DataDocument.CurrentProduct.IsFxBarrierOption ||
                        DataDocument.CurrentProduct.IsFxSimpleOption ||
                        DataDocument.CurrentProduct.IsEQD || DataDocument.CurrentProduct.IsBondOption ||
                        (this.IsTradeFoundAndAllocation && Tools.IsExchangeTradedDerivativeOption(DataDocument.CurrentProduct.Product));
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si l'assignation est permise sur le trade
        /// </summary>
        public bool IsAssignmentAuthorized
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                //
                if (ret)
                {
                    ret = this.IsETDandAllocation && Tools.IsExchangeTradedDerivativeOption(DataDocument.CurrentProduct.Product);
                    if (ret)
                    {
                        ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)DataDocument.CurrentProduct.Product, DataDocument);
                        ret = etd.IsDealerBuyerOrSeller(BuyerSellerEnum.SELLER);
                    }
                }
                //
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si la correction de quantité est permise sur le trade
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        public bool IsPositionCancelationEvents
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                    //ret = IsTradeFoundAndAllocation && (false == Product.productBase.IsCommoditySpot); // FI 20161214 [21916]  pas de tenu de postion sur CommoditySpot
                    ret = IsTradeFoundAndAllocation && m_SQLInstrument.IsFungible; // FI 20170116 [21916]  Règle plus large
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si la transfert de quantité est permise sur le trade
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        public bool IsPositionTransferEvents
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                    //ret = IsTradeFoundAndAllocation && (false == Product.productBase.IsCommoditySpot); // FI 20161214 [21916]  pas de tenu de postion sur CommoditySpot
                    ret = IsTradeFoundAndAllocation && m_SQLInstrument.IsFungible; // FI 20170116 [21916]  Règle plus large
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si le Splitting est permis sur le trade
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        public bool IsTradeSplitting
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                    //ret = IsTradeFoundAndAllocation && (false == Product.productBase.IsCommoditySpot); // FI 20161214 [21916]  pas de tenu de postion sur CommoditySpot
                    ret = IsTradeFoundAndAllocation && m_SQLInstrument.IsFungible; // FI 20170116 [21916]  Règle plus large
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si la clôture spécifique est permise sur le trade
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        public bool IsClearingSpecificEvents
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                    //ret = IsTradeFoundAndAllocation && (false == Product.productBase.IsCommoditySpot); // FI 20161214 [21916]  pas de tenu de postion sur CommoditySpot
                    ret = IsTradeFoundAndAllocation && m_SQLInstrument.IsFungible; // FI 20170116 [21916]  Règle plus large
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si la livraison du sous-jacent est permise sur le trade
        /// </summary>
        public bool IsUnderlyingDeliveryAuthorized
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                {
                    ret = (this.IsTradeFoundAndAllocation && Tools.IsExchangeTradedDerivativeFuture(DataDocument.CurrentProduct.Product));
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20140708 Add Test isEQD
        /// EG 20150423 [20513] BANCAPERTA
        public bool IsBarrierTriggerAuthorized
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                {
                    ret = DataDocument.CurrentProduct.IsFxBarrierOption || DataDocument.CurrentProduct.IsFxDigitalOption ||
                        DataDocument.CurrentProduct.IsEQD || DataDocument.CurrentProduct.IsBondOption;
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsFixingCustomerAuthorized
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                if (ret)
                {
                    ret = DataDocument.CurrentProduct.IsFixingCalculationAgent;
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si l'exercice est permis sur le trade
        /// <para>FxAverageRateOption ,FxBarrierOption,FxSimpleOption ou Achat d'un contrat option </para>
        /// </summary>
        /// EG 20140708 Add Test IsEQD
        /// EG 20150423 [20513] BANCAPERTA 
        public bool IsExerciseAuthorized
        {
            get
            {
                bool ret = (TradeStatus.stEnvironment.NewSt != Cst.StatusEnvironment.TEMPLATE.ToString());
                //
                if (ret)
                {
                    if (DataDocument.CurrentProduct.IsFxAverageRateOption ||
                        DataDocument.CurrentProduct.IsFxBarrierOption ||
                        DataDocument.CurrentProduct.IsFxSimpleOption ||
                        DataDocument.CurrentProduct.IsEQD || DataDocument.CurrentProduct.IsBondOption)
                    {
                        ret = true;
                    }
                    else if (IsAllocation && Tools.IsExchangeTradedDerivativeOption(DataDocument.CurrentProduct.Product))
                    {
                        ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)DataDocument.CurrentProduct.Product, DataDocument);
                        ret = etd.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER);
                    }
                    else
                    {
                        ret = false;
                    }
                }
                //
                return ret;
            }
        }
        #endregion Accessors

        #region Constructor
        public TradeInput() : base() { }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Comparaison entre les événements de frais déjà facturés et 
        /// une ligne de frais dans le trade (IPayment)
        /// Si non trouvé => frais modifiable
        /// </summary>
        /// <param name="pPayment"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        // EG 20240210 [WI816] Trade input: Correctifs
        public bool PaymentIsUninvoiced(IPayment pPayment)
        {
            bool ret = true;
            if (StrFunc.IsFilled(pPayment.PayerPartyReference.HRef) && StrFunc.IsFilled(pPayment.ReceiverPartyReference.HRef))
            {
                (int idA, int? idB) payer = (DataDocument.GetOTCmlId_Party(pPayment.PayerPartyReference.HRef).Value, DataDocument.GetOTCmlId_Book(pPayment.PayerPartyReference.HRef));
                (int idA, int? idB) receiver = (DataDocument.GetOTCmlId_Party(pPayment.ReceiverPartyReference.HRef).Value, DataDocument.GetOTCmlId_Book(pPayment.ReceiverPartyReference.HRef));
                (DateTime date, decimal amount, string currency) amount = (pPayment.PaymentDate.UnadjustedDate.DateValue, pPayment.PaymentAmount.Amount.DecValue, pPayment.PaymentCurrency);

                ret = (null == feesAlreadyInvoiced.FirstOrDefault(item =>
                    (item.payer == payer) && (item.receiver == receiver) && (item.payment == amount) &&
                    ((item.status.HasValue && pPayment.PaymentSource.StatusSpecified && (item.status.Value == pPayment.PaymentSource.Status)) ||
                    ((false == item.status.HasValue) && (false == pPayment.PaymentSource.StatusSpecified))) &&
                    (item.eventType == pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeEventTypeScheme).Value)
                ));
            }
            return ret;
        }

        /// <summary>
        /// Alimente les données non renseignées avec des valeurs par défaut
        /// <para>Alimente notamment les données obligatoire vis à vis  du XSD</para>
        /// </summary>
        public override void SetDefaultValue(string pCS, IDbTransaction pDbTransaction)
        {
            //
            if (this.Product.ProductBase.IsRepo)
            {
                IRepo repo = (IRepo)this.Product.Product;
                ICalculationPeriodDates calculationPeriodDates = repo.CashStream[repo.CashStream.Length - 1].CalculationPeriodDates;

                //alimentation de cashStream avec datemax si non renseigné Max 9999-12-31
                #region add TerminationDate
                bool isSetTerminationDate = (false == calculationPeriodDates.TerminationDateRelativeSpecified);
                if (isSetTerminationDate)
                {
                    isSetTerminationDate = (
                        (false == calculationPeriodDates.TerminationDateAdjustableSpecified) ||
                        calculationPeriodDates.TerminationDateAdjustableSpecified && DtFunc.IsDateTimeEmpty(calculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue));
                }
                //
                //
                if (isSetTerminationDate)
                {
                    //FI 20091223 [16471]  appel à CreateAdjustableDate
                    calculationPeriodDates.TerminationDateAdjustableSpecified = true;
                    calculationPeriodDates.TerminationDateAdjustable = Product.ProductBase.CreateAdjustableDate(DateTime.MaxValue, BusinessDayConventionEnum.NONE, null);
                }
                #endregion add TerminationDate
                //
                #region add forwardLeg
                //
                bool isSetForwardLeg = (RepoDurationEnum.Open != repo.Duration);
                isSetForwardLeg = isSetForwardLeg && (false == repo.ForwardLegSpecified);
                isSetForwardLeg = isSetForwardLeg && (calculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue != DateTime.MaxValue);
                //                        
                if (isSetForwardLeg)
                {
                    // Génération des forwardLeg à l'identique des spotLeg (sauf sens)
                    if (false == repo.ForwardLegSpecified)
                    {
                        ArrayList al = new ArrayList();
                        for (int i = 0; i < ArrFunc.Count(repo.SpotLeg); i++)
                        {
                            ISecurityLeg clone = (ISecurityLeg)ReflectionTools.Clone(repo.SpotLeg[i], ReflectionTools.CloneStyle.CloneField);
                            //
                            string hRefTmp = clone.DebtSecurityTransaction.BuyerPartyReference.HRef;
                            clone.DebtSecurityTransaction.BuyerPartyReference.HRef = clone.DebtSecurityTransaction.SellerPartyReference.HRef;
                            clone.DebtSecurityTransaction.SellerPartyReference.HRef = hRefTmp;
                            //
                            hRefTmp = clone.DebtSecurityTransaction.GrossAmount.PayerPartyReference.HRef;
                            clone.DebtSecurityTransaction.GrossAmount.PayerPartyReference.HRef = clone.DebtSecurityTransaction.GrossAmount.ReceiverPartyReference.HRef;
                            clone.DebtSecurityTransaction.GrossAmount.ReceiverPartyReference.HRef = hRefTmp;
                            //
                            string idAsset = string.Empty;
                            if (repo.SpotLeg[i].DebtSecurityTransaction.SecurityAssetSpecified)
                                idAsset = repo.SpotLeg[i].DebtSecurityTransaction.SecurityAsset.Id;

                            else if (repo.SpotLeg[i].DebtSecurityTransaction.SecurityAssetReferenceSpecified)
                                idAsset = repo.SpotLeg[i].DebtSecurityTransaction.SecurityAssetReference.HRef;
                            //
                            if (StrFunc.IsFilled(idAsset))
                            {
                                clone.DebtSecurityTransaction.SecurityAssetSpecified = false;
                                clone.DebtSecurityTransaction.SecurityAssetReferenceSpecified = true;
                                clone.DebtSecurityTransaction.SecurityAssetReference.HRef = idAsset;
                            }
                            //
                            clone.SpotLegReferenceSpecified = StrFunc.IsFilled(repo.SpotLeg[i].Id);
                            if (clone.SpotLegReferenceSpecified)
                            {
                                clone.SpotLegReference.HRef = repo.SpotLeg[i].Id;
                                clone.Id = null;
                            }
                            //
                            al.Add(clone);
                        }
                        if (ArrFunc.IsFilled(al))
                        {
                            repo.ForwardLegSpecified = true;
                            //Ce code ne marche pas bizarrement
                            //ISecurityLeg[] forwardLeg = (ISecurityLeg[])al.ToArray(typeof(ISecurityLeg));
                            //repo.forwardLeg = forwardLeg;
                            repo.ForwardLeg = Product.ProductBase.CreateSecurityLegs(al.Count);
                            for (int i = 0; i < al.Count; i++)
                            {
                                repo.ForwardLeg[i].Id = TradeCustomCaptureInfos.CCst.Prefix_forwardLeg + (i + 1).ToString();
                                repo.ForwardLeg[i] = (ISecurityLeg)al[i];
                            }
                        }
                    }
                }
                #endregion
                //
                #region set PrincipalExchanges on cashStream
                for (int i = 0; i < ArrFunc.Count(repo.CashStream); i++)
                {
                    if (false == repo.CashStream[i].PrincipalExchangesSpecified)
                    {
                        repo.CashStream[i].PrincipalExchangesSpecified = true;

                        repo.CashStream[i].PrincipalExchanges.InitialExchange.BoolValue = true;
                        repo.CashStream[i].PrincipalExchanges.IntermediateExchange.BoolValue = true;
                        repo.CashStream[i].PrincipalExchanges.FinalExchange.BoolValue = true;
                    }
                }
                #endregion
                //
            }
            else if (this.Product.ProductBase.IsBuyAndSellBack)
            {
                IBuyAndSellBack buyAndSellBack = (IBuyAndSellBack)this.Product.Product;
                //
                #region set PrincipalExchanges on cashStream
                for (int i = 0; i < ArrFunc.Count(buyAndSellBack.CashStream); i++)
                {
                    if (false == buyAndSellBack.CashStream[i].PrincipalExchangesSpecified)
                    {
                        buyAndSellBack.CashStream[i].PrincipalExchangesSpecified = true;
                        buyAndSellBack.CashStream[i].PrincipalExchanges.InitialExchange.BoolValue = false;
                        buyAndSellBack.CashStream[i].PrincipalExchanges.IntermediateExchange.BoolValue = false;
                        buyAndSellBack.CashStream[i].PrincipalExchanges.FinalExchange.BoolValue = false;
                    }
                }
                #endregion
            }

            AddBookForOtherPartyPayment(CSTools.SetCacheOn(pCS), pDbTransaction);

            // FI 20180328 [23871] Call SetPaymentId
            // En cas de modification de trade sans cght notoire de mettre à jour les Id sur les frais
            if (CurrentTrade.OtherPartyPaymentSpecified)
                Tools.SetPaymentId(CurrentTrade.OtherPartyPayment, "OPP");

            base.SetDefaultValue(pCS, pDbTransaction);

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        public void SetFlagProvision(string pCS)
        {
            //FDA/PL 20120202 Add try/catch and dr.Close()
            if (IsTradeFound)
            {
                ResetFlagProvision();

                #region Provision authorization
                using (IDataReader dr = TradeRDBMSTools.IsTradeProvision(pCS, m_SQLTrade.IdT))
                {
                    while (dr.Read())
                    {
                        string eventType = dr["EVENTTYPE"].ToString();
                        if (EventTypeFunc.IsCancelableProvision(eventType))
                            _isTradeCancelable = true;
                        else if (EventTypeFunc.IsExtendibleProvision(eventType))
                            _isTradeExtendible = true;
                        else if (EventTypeFunc.IsMandatoryEarlyTerminationProvision(eventType))
                            _isTradeMandatoryEarlyTermination = true;
                        else if (EventTypeFunc.IsOptionalEarlyTerminationProvision(eventType))
                            _isTradeOptionalEarlyTermination = true;
                        else if (EventTypeFunc.IsStepUpProvision(eventType))
                            _isTradeStepUp = true;
                    }
                }
                #endregion Provision authorization
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeIdentifier"></param>
        public override void InitializeSqlTrade(string pCS,IDbTransaction pDbTransaction, string pTradeIdentifier)
        {
            InitializeSqlTrade(pCS, pDbTransaction, pTradeIdentifier, SQL_TableWithID.IDType.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            m_SQLTrade = new SQL_TradeTransaction(pCS, pIdType, pId)
            {
                DbTransaction = pDbTransaction
            };
        }

        /// <summary>
        /// Définit une nouvelle instance des ccis
        /// <para>
        /// Synchronize des pointeurs existants dans les CciContainers avec les éléments du dataDocument
        /// </para>
        /// </summary>
        /// <param name="User"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modification de signature
        // EG 20180425 Analyse du code Correction [CA2214]
        public override void InitializeCustomCaptureInfos(string pCS, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
        {
            CustomCaptureInfos = new TradeCustomCaptureInfos(pCS, this, pUser, pSessionId, pIsGetDefaultOnInitializeCci);
            CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        /// Initialise le membre associée à une action particulière (remove, correctionOfQuantity, exercice, etc...)   
        /// </summary>
        /// <param name="pMode"></param>
        /// EG 20140731 Refactoring (RptSideContainer)
        // EG 20180514 [23812] Report
        public override void InitializeForAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            if (Cst.Capture.ModeEnum.PositionCancelation == pMode)
            {
                positionCancel = new TradePositionCancelation(pCS,this);
            }
            else if (Cst.Capture.ModeEnum.PositionTransfer == pMode)
            {
                positionTransfer = new TradePositionTransfer(pCS,this);
            }
            else if (Cst.Capture.ModeEnum.OptionAbandon == pMode || Cst.Capture.ModeEnum.OptionAssignment == pMode || Cst.Capture.ModeEnum.OptionExercise == pMode)
            {
                // EG 20151102 [21465] Upd
                tradeDenOption = new TradeDenOption();
                DateTime dtBusiness = GetDefaultDateAction(pCS, pMode);
                // EG 20151019 [21112] Add null for pDbTransaction parameter
                tradeDenOption.Initialized(pCS, null, Identification.OTCmlId, dtBusiness, (IExchangeTradedDerivative)Product.Product, pMode, DataDocument,
                    Cst.DenOptionActionType.newRemaining);
            }
            else if (Cst.Capture.ModeEnum.RemoveAllocation == pMode)
            {
                removeAllocation = new TradeRemoveAllocation(pCS,this);
            }
            else if (Cst.Capture.ModeEnum.TradeSplitting == pMode)
            {
                split = new TradeSplit(pCS,this);
            }
            // PM 20130822 [17949] Livraison Matif
            else if (Cst.Capture.ModeEnum.UnderlyingDelivery == pMode)
            {
                DateTime dtBusiness = GetDefaultDateAction(pCS, pMode);
                underlyingDelivery = new TradeUnderlyingDelivery(Identification, dtBusiness);
                underlyingDelivery.Init(pCS, Identification.OTCmlId, (IExchangeTradedDerivative)Product.Product, DataDocument);
            }
            else if (Cst.Capture.ModeEnum.FxOptionEarlyTermination == pMode) // FI 20180221 [23803] Add 
            {
                DateTime dtAction = DateTime.Now;

                this.fxOptionEarlyTermination = new TradeFxOptionEarlyTermination()
                {
                    actionDate = new EFS.GUI.Interface.EFS_DateTime(DtFunc.DateTimeToStringISO(dtAction)),
                    valueDate = new EFS.GUI.Interface.EFS_Date(DtFunc.DateTimeToStringDateISO(dtAction)),
                    cashSettlement = new FpML.v44.Shared.Money(),
                    settlementDate = new EFS.GUI.Interface.EFS_Date(DtFunc.DateTimeToStringDateISO(dtAction)),
                    payerPartyReference = new FpML.v44.Shared.PartyReference(),
                    receiverPartyReference = new FpML.v44.Shared.PartyReference(),
                };

                if (((IFxOptionBase)this.Product.Product.ProductBase).FxOptionPremiumSpecified)
                {
                    IFxOptionPremium[] premium = ((IFxOptionBase)this.Product.Product.ProductBase).FxOptionPremium;
                    this.fxOptionEarlyTermination.payerPartyReference.HRef = premium[0].ReceiverPartyReference.HRef;
                    this.fxOptionEarlyTermination.receiverPartyReference.HRef = premium[0].PayerPartyReference.HRef;
                    this.fxOptionEarlyTermination.cashSettlement.Currency = premium[0].PremiumAmount.Currency;
                }
            }
            base.InitializeForAction(pCS, pMode);
        }
        /// <summary>
        /// Chargment des frais déjà facturés pour un trade
        /// Stockage dans liste feeAlreadyinvoiced
        /// Remarque pour Une facture annulée 
        /// - tous les événements de frais de la facture ont le STATUT : DEACTIV
        /// - les liens entre les événements de frais sur les trade de marché utilisés dans la facture sont supprimés (IDE_SOURCE = NULL)
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        // EG 20240210 [WI816] Trade input: Correctifs
        public override void InitializeInvoicedFees(string pCS, int pIdT)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            dp.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.OtherPartyPayment);
            dp.Add(new DataParameter(pCS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Invoiced);

            string sqlSelect = @"select ev.IDT, ev.IDE, ev.IDA_PAY, ev.IDB_PAY, ev.IDA_REC, ev.IDB_REC,
            ev.EVENTTYPE, ev.DTSTARTUNADJ, ev.VALORISATION, ev.UNIT, 
            ef.STATUS, ef.IDFEEMATRIX, ef.IDFEE, ef.IDFEESCHEDULE, ef.ISFEEINVOICING
            from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            inner join dbo.EVENTFEE ef on (ef.IDE = ev.IDE)
            inner join dbo.EVENT evinv on (evinv.IDE_SOURCE = ev.IDE)
            where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ec.EVENTCLASS = @EVENTCLASS)
            order by ev.IDE";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                feesAlreadyInvoiced = 
                from item in DataReaderExtension.DataReaderMapToList(dr)
                select 
                new TradeFeeInvoiced()
                {
                    idE = Convert.ToInt32(item["IDE"].Value),
                    payer = (Convert.ToInt32(item["IDA_PAY"].Value), Convert.ToInt32(item["IDB_PAY"].Value)),
                    receiver = (Convert.ToInt32(item["IDA_REC"].Value), Convert.ToInt32(item["IDB_REC"].Value)),
                    payment = (Convert.ToDateTime(item["DTSTARTUNADJ"].Value), Convert.ToDecimal(item["VALORISATION"].Value), Convert.ToString(item["UNIT"].Value)),
                    status = ReflectionTools.ConvertStringToEnumOrNullable<SpheresSourceStatusEnum>(Convert.ToString(item["STATUS"].Value)),
                    eventType = Convert.ToString(item["EVENTTYPE"].Value),
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20151102 [21465] Upd
        public override void Clear()
        {
            underlyingDelivery = null;
            tradeDenOption = null;
            positionCancel = null;
            base.Clear();
        }

        /// <summary>
        /// Retourne la date à pré-proposée  en fonction de l'action (exercide, abandon , correction,..)
        /// </summary>
        /// <param name="pMode"></param>
        /// <returns></returns>
        /// EG 20120607 Date action = MAX(DTBUSINESS,CLEARINGBUSINESSDATE)
        // EG 20180514 [23812] Report 
        public override DateTime GetDefaultDateAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            DateTime ret;
            if (Cst.Capture.ModeEnum.PositionCancelation == pMode ||
                Cst.Capture.ModeEnum.OptionAssignment == pMode ||
                Cst.Capture.ModeEnum.OptionAbandon == pMode ||
                Cst.Capture.ModeEnum.OptionExercise == pMode ||
                Cst.Capture.ModeEnum.PositionTransfer == pMode ||
                Cst.Capture.ModeEnum.RemoveAllocation == pMode ||
                Cst.Capture.ModeEnum.TradeSplitting == pMode ||
                Cst.Capture.ModeEnum.UnderlyingDelivery == pMode || // PM 20130822 [17949] Livraison Matif
                Cst.Capture.ModeEnum.ClearingSpecific == pMode ||
                Cst.Capture.ModeEnum.FxOptionEarlyTermination == pMode)
            {
                //FI 20121113[18224] Lecture de CurrentBusinessDate
                ret = CurrentBusinessDate;
                if (ret == DateTime.MinValue)
                    ret = OTCmlHelper.GetDateBusiness(pCS);
                else if (ret < ClearingBusinessDate)
                    ret = ClearingBusinessDate;
            }
            else
            {
                ret = base.GetDefaultDateAction(pCS,pMode);
            }
            return ret;

        }

        /// <summary>
        /// Retourne une nouvelle instance de la classe chargée d'alimenter postRequest
        /// <para>La classe instanciée est fonction du type d'action (Abandon, Exercice, Correction, etc...)</para>
        /// </summary>
        /// <param name="pMode">  </param>
        /// <returns></returns>
        // EG 20150716 [21103] Add isReversalSafekeeping
        // EG 20170223 Add SQLProduct.GroupProduct
        public IPosRequest NewPostRequest(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pMode)
        {
            IPosRequest postRequest = null;

            SQL_EntityMarket sqlEntityMarket = GetSql_EntityMarket(pCS, pDbTransaction);
            int idEM = sqlEntityMarket.IdEM;
            int idA_Entity = sqlEntityMarket.IdA;
            int idA_CSS = sqlEntityMarket.IdA_CSS;
            Nullable<int> idA_Custodian = sqlEntityMarket.IdA_CUSTODIAN;

            if (null != positionCancel && pMode == Cst.Capture.ModeEnum.PositionCancelation)
            {
                // EG 20150907 [21317] InitialQty
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                // EG 20170223 Add SQLProduct.GroupProduct
                IPosRequestCorrection postRequestCorrection = (IPosRequestCorrection)PosKeepingTools.CreatePosRequestCorrection(
                    Product.ProductBase,
                    SettlSessIDEnum.Intraday,
                    SQLTrade.IdT,
                    SQLTrade.Identifier,
                    idA_Entity,
                    idA_CSS,
                    idA_Custodian,
                    idEM,
                    positionCancel.date.DateValue,
                    positionCancel.initialQuantity.DecValue,
                    positionCancel.availableQuantity.DecValue,
                    positionCancel.quantity.DecValue,
                    positionCancel.otherPartyPaymentSpecified ? positionCancel.otherPartyPayment : null,
                    positionCancel.isReversalSafekeeping, positionCancel.note, SQLProduct.GroupProduct);
                postRequest = postRequestCorrection;
            }
            else if (null != positionTransfer && pMode == Cst.Capture.ModeEnum.PositionTransfer)
            {
                // EG 20150716 [21103] Add isReversalSafekeeping
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                // EG 20170223 Add SQLProduct.GroupProduct
                IPosRequestTransfer postRequestTransfer = (IPosRequestTransfer)PosKeepingTools.CreatePosRequestTransfer(
                    pCS,
                    Product.ProductBase,
                    positionTransfer.tradeIdentification.OTCmlId,
                    positionTransfer.tradeIdentification.Identifier,
                    idA_Entity,
                    idA_CSS,
                    idA_Custodian,
                    idEM,
                    positionTransfer.date.DateValue,
                    positionTransfer.initialQuantity.DecValue,
                    positionTransfer.availableQuantity.DecValue,
                    positionTransfer.quantity.DecValue,
                    positionTransfer.otherPartyPayment, positionTransfer.note, this.Identification.OTCmlId, positionTransfer.isReversalSafekeeping,
                    SQLProduct.GroupProduct);

                postRequest = postRequestTransfer;
            }
            else if (null != removeAllocation && Cst.Capture.IsModeRemoveAllocation(pMode))
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                // EG 20170223 Add SQLProduct.GroupProduct
                IPosRequestRemoveAlloc postRequestRemoveAlloc = (IPosRequestRemoveAlloc)PosKeepingTools.CreatePosRequestRemoveAlloc(
                    pCS,
                    Product.ProductBase,
                    removeAllocation.tradeIdentification.OTCmlId,
                    removeAllocation.tradeIdentification.Identifier,
                    idA_Entity, idA_CSS, idA_Custodian, idEM,
                    removeAllocation.date.DateValue,
                    removeAllocation.initialQuantity.DecValue,
                    removeAllocation.availableQuantity.DecValue,
                    removeAllocation.note, SQLProduct.GroupProduct);
                //
                postRequest = postRequestRemoveAlloc;
            }
            else if (null != split && Cst.Capture.IsModeTradeSplitting(pMode))
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                // EG 20170223 Add SQLProduct.GroupProduct
                IPosRequestSplit posRequestSplit = (IPosRequestSplit)PosKeepingTools.CreatePosRequestSplit(
                    pCS,
                    Product.ProductBase,
                    split.tradeIdentification.OTCmlId,
                    split.tradeIdentification.Identifier,
                    idA_Entity, idA_CSS, idA_Custodian, idEM,
                    split.date.DateValue,
                    split.initialQuantity.DecValue,
                    split.note,
                    split.AlNewTrades, SQLProduct.GroupProduct);

                postRequest = posRequestSplit;
            }
            return postRequest;
        }
        // EG 20151102 [21465] New
        // EG 20170223 Add SQLProduct.GroupProduct
        public List<IPosRequest> NewPostRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pPosRequestType, bool pIsCallByService)
        {
            List<IPosRequest> posRequest = new List<IPosRequest>();

            SQL_EntityMarket sqlEntityMarket = GetSql_EntityMarket(pCS, pDbTransaction);
            int idEM = sqlEntityMarket.IdEM;
            int idA_Entity = sqlEntityMarket.IdA;
            int idA_CSS = sqlEntityMarket.IdA_CSS;
            Nullable<int> idA_Custodian = sqlEntityMarket.IdA_CUSTODIAN;

            if (null != tradeDenOption)
            {
                QuoteTimingEnum quoteTiming = default;
                if (Enum.IsDefined(typeof(QuoteTimingEnum), tradeDenOption.asset.underlyer.quoteTiming))
                    quoteTiming = (QuoteTimingEnum)Enum.Parse(typeof(QuoteTimingEnum), tradeDenOption.asset.underlyer.quoteTiming);

                IPosRequestOption posRequestOption = null;
                switch (tradeDenOption.denOptionActionType)
                {
                    case Cst.DenOptionActionType.@new:
                    case Cst.DenOptionActionType.newRemaining:
                        // EG 20170223 Add SQLProduct.GroupProduct
                        posRequestOption = (IPosRequestOption)PosKeepingTools.CreatePosRequestOption(pCS,
                            Product.ProductBase,
                            tradeDenOption.posRequestType,
                            tradeDenOption.PosRequestMode,
                            SQLTrade.IdT,
                            SQLTrade.Identifier,
                            idA_Entity,
                            idA_CSS,
                            idA_Custodian,
                            idEM,
                            tradeDenOption.date.DateValue,
                            // EG 20170127 Qty Long To Decimal
                            tradeDenOption.quantity.DecValue,
                            tradeDenOption.availableQuantity.DecValue,
                            tradeDenOption.asset.strikePrice,
                            tradeDenOption.asset.underlyer.underlyingAsset,
                            tradeDenOption.asset.underlyer.idAsset,
                            tradeDenOption.asset.underlyer.identifier,
                            quoteTiming,
                            tradeDenOption.asset.underlyer.quoteValue,
                            tradeDenOption.asset.underlyer.quoteTime,
                            tradeDenOption.asset.underlyer.quoteSource,
                            tradeDenOption.otherPartyPaymentSpecified ? tradeDenOption.otherPartyPayment : null,
                            tradeDenOption.note, null, SQLProduct.GroupProduct);

                        // Si et seulement si demande en provenance d'un dénouement manuel via WEB
                        if (false == pIsCallByService)
                        {
                            // si 1 seule ligne de dénouement alors on la modifie
                            if (1 == tradeDenOption.ActionLengthByRequestType(pPosRequestType))
                            {
                                // Si quantité restante alors : Qty à dénouer = Qty available
                                // EG 20170127 Qty Long To Decimal
                                if (tradeDenOption.denOptionActionType == Cst.DenOptionActionType.newRemaining)
                                    posRequestOption.Qty = tradeDenOption.availableQuantity.DecValue;
                                // On  récupère l'IDPR de l'unique dénouement
                                posRequestOption.IdPR = tradeDenOption.IdPR_ForModifiedAction(pPosRequestType);
                            }
                        }
                        posRequest.Add(posRequestOption);
                        break;
                    case Cst.DenOptionActionType.remove:
                        if (ArrFunc.IsFilled(tradeDenOption.action))
                        {
                            tradeDenOption.action.ToList().ForEach(item =>
                            {
                                if (item.denIsRemove.BoolValue)
                                {
                                    // EG 20170223 Add SQLProduct.GroupProduct
                                    posRequestOption = (IPosRequestOption)PosKeepingTools.CreatePosRequestOption(pCS,
                                        Product.ProductBase,
                                        tradeDenOption.posRequestType,
                                        tradeDenOption.PosRequestMode,
                                        SQLTrade.IdT,
                                        SQLTrade.Identifier,
                                        idA_Entity,
                                        idA_CSS,
                                        idA_Custodian,
                                        idEM,
                                        tradeDenOption.date.DateValue,
                                        // EG 20170127 Qty Long To Decimal
                                        item.inputQuantity.DecValue,
                                        tradeDenOption.availableQuantity.DecValue,
                                        tradeDenOption.asset.strikePrice,
                                        tradeDenOption.asset.underlyer.underlyingAsset,
                                        tradeDenOption.asset.underlyer.idAsset,
                                        tradeDenOption.asset.underlyer.identifier,
                                        quoteTiming,
                                        tradeDenOption.asset.underlyer.quoteValue,
                                        tradeDenOption.asset.underlyer.quoteTime,
                                        tradeDenOption.asset.underlyer.quoteSource,
                                        tradeDenOption.otherPartyPaymentSpecified ? tradeDenOption.otherPartyPayment : null,
                                        tradeDenOption.note, null, SQLProduct.GroupProduct);
                                    posRequestOption.IdPR = item.idPR.IntValue;
                                    posRequest.Add(posRequestOption);
                                }
                            });
                        }
                        break;
                }
            }
            return posRequest;
        }

        /// <summary>
        ///  Nouvelle instance de SQL_EntityMarket
        /// </summary>
        /// <returns></returns>
        /// EG 20151102 [21465] New
        /// FI 20161005 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private SQL_EntityMarket GetSql_EntityMarket(String pCS , IDbTransaction pDbTransaction)
        {
            string cs = CSTools.SetCacheOn(pCS);
            int idEntity = DataDocument.GetFirstEntity(cs);
            if (idEntity <= 0)
                throw new Exception("No entity found in dataDocument");

            DataDocument.CurrentProduct.GetMarket(pCS, pDbTransaction, out SQL_Market sqlMarket);
            if ((sqlMarket == null))
                throw new Exception("No market found in dataDocument");

            RptSideProductContainer rptSideProduct = Product.RptSide(pCS, pDbTransaction, IsAllocation);
            // FI 20161005 [XXXXX] Add NotImplementedException
            if (null == rptSideProduct)
                throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", Product.ProductBase.ToString()));


            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(cs, pDbTransaction, idEntity, sqlMarket.Id, rptSideProduct.IdA_Custodian)
            {
                DbTransaction = pDbTransaction
            };
            bool isFoundEntityMarket = sqlEntityMarket.LoadTable(new string[] { "IDEM, IDA_CUSTODIAN, ety.IDA, mk.IDA" });
            if ((false == isFoundEntityMarket))
                throw new Exception("No EntityMarket Found");

            return sqlEntityMarket;
        }
        /// <summary>
        /// Retourne la ou les origines des frais présents sur un élément (trade,exeAssAbnOption, etc..) 
        /// </summary>
        /// <param name="pFeeTarget">Représente l'élément qui contient des frais</param>
        /// <returns>NoFee,FeeFromManualInput,FeeFromCalculateBySchedule,FeeFromManualInputAndCalculateBySchedule</returns>
        // EG 20150708 [21103] Upd
        // EG 20151102 [21465] Upd
        public Cst.OriginOfFeeEnum GetOriginOfFee(FeeTarget pFeeTarget)
        {
            bool isExist = false;
            bool isExistManualInput = false;
            bool isExistCalculateBySchedule = false;
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade != pFeeTarget) & (FeeTarget.denOption != pFeeTarget) & (FeeTarget.none != pFeeTarget))
                throw new NotImplementedException(StrFunc.AppendFormat("Target Element {0} is not implemented", pFeeTarget.ToString()));
            //
            #region OPP
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade == pFeeTarget) || (FeeTarget.denOption == pFeeTarget))
            {
                IPayment[] otherPartyPayment = null;
                bool otherPartyPaymentSpecified = false;
                if (FeeTarget.trade == pFeeTarget)
                {
                    // EG 20150708 [21103]
                    if (null != safekeepingAction)
                    {
                        otherPartyPayment = safekeepingAction.payment;
                        otherPartyPaymentSpecified = safekeepingAction.paymentSpecified;
                    }
                    else
                    {
                        otherPartyPayment = CurrentTrade.OtherPartyPayment;
                        otherPartyPaymentSpecified = CurrentTrade.OtherPartyPaymentSpecified;
                    }
                }
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                else if (FeeTarget.denOption == pFeeTarget)
                {
                    // EG 20151102 [21465] Upd
                    otherPartyPayment = tradeDenOption.otherPartyPayment;
                    otherPartyPaymentSpecified = tradeDenOption.otherPartyPaymentSpecified;
                }

                if (ArrFunc.IsFilled(otherPartyPayment))
                {
                    isExist = otherPartyPaymentSpecified;
                    if (isExist)
                    {
                        foreach (IPayment payment in otherPartyPayment)
                        {
                            if (StrFunc.IsFilled(payment.PayerPartyReference.HRef))
                            {
                                if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme))
                                    isExistCalculateBySchedule = true;
                                else
                                    isExistManualInput = true;
                            }
                        }
                    }
                }
            }
            #endregion OPP
            //
            #region ADP
            //Seuls les trades on des ADP
            if (FeeTarget.trade == pFeeTarget)
            {
                IPayment[] additionalPayment = null;
                if (Product.IsLoanDeposit)
                {
                    #region IsLoanDeposit
                    isExist = isExist || ((ILoanDeposit)CurrentTrade.Product).AdditionalPaymentSpecified;
                    additionalPayment = ((ILoanDeposit)CurrentTrade.Product).AdditionalPayment;
                    #endregion IsLoanDeposit
                }
                else if ((Product.IsSwap) || Product.IsSwaption)
                {
                    ISwap swap;

                    #region IsSwap or IsSwaption
                    if (Product.IsSwap)
                        swap = (ISwap)Product.Product;
                    else
                        swap = ((ISwaption)Product.Product).Swap;
                    //
                    isExist = isExist || swap.AdditionalPaymentSpecified;
                    additionalPayment = swap.AdditionalPayment;
                    #endregion IsSwap or IsSwaption
                }
                else if (Product.IsCapFloor)
                {
                    #region IsCapFloor
                    isExist = isExist || ((ICapFloor)Product.Product).AdditionalPaymentSpecified;
                    additionalPayment = ((ICapFloor)CurrentTrade.Product).AdditionalPayment;
                    #endregion IsCapFloor
                }
                else if (Product.IsReturnSwap)
                {
                    #region IsReturnSwap
                    isExist = isExist || ((IReturnSwap)Product.Product).AdditionalPaymentSpecified;
                    IReturnSwapAdditionalPayment[] returnSwapPayment = ((IReturnSwap)Product.Product).AdditionalPayment;

                    if ((ArrFunc.IsFilled(returnSwapPayment)) && isExist)
                    {
                    }
                    #endregion IsReturnSwap
                }
                else if (Product.IsStrategy)
                {
                    #region IsStrategy
                    ArrayList al = new ArrayList();
                    StrategyContainer strategyContainer = (StrategyContainer)Product;
                    ProductContainer[] subProduct = strategyContainer.GetSubProduct();
                    for (int i = 0; i < ArrFunc.Count(subProduct); i++)
                    {
                        if (subProduct[i].IsLoanDeposit)
                        {
                            if (((ILoanDeposit)subProduct[i].Product).AdditionalPaymentSpecified)
                                al.AddRange((((ILoanDeposit)subProduct[i].Product).AdditionalPayment));
                        }
                        else if (subProduct[i].IsSwap)
                        {
                            if (((ISwap)subProduct[i].Product).AdditionalPaymentSpecified)
                                al.AddRange((((ISwap)subProduct[i].Product).AdditionalPayment));
                        }
                        else if (subProduct[i].IsSwaption)
                        {
                            if (((ISwaption)subProduct[i].Product).Swap.AdditionalPaymentSpecified)
                                al.AddRange((((ISwaption)subProduct[i].Product).Swap.AdditionalPayment));
                        }
                        else if (subProduct[i].IsCapFloor)
                        {
                            if (((ICapFloor)subProduct[i].Product).AdditionalPaymentSpecified)
                                al.AddRange((((ICapFloor)subProduct[i].Product).AdditionalPayment));
                        }
                    }
                    if ((ArrFunc.IsFilled(al)))
                        additionalPayment = (IPayment[])al.ToArray(typeof(IPayment));
                    #endregion IsStrategy
                }
                //
                if (isExist && ArrFunc.IsFilled(additionalPayment))
                {
                    foreach (IPayment payment in additionalPayment)
                    {
                        if (StrFunc.IsFilled(payment.PayerPartyReference.HRef))
                        {
                            if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme))
                                isExistCalculateBySchedule = true;
                            else
                                isExistManualInput = true;
                        }
                    }
                }
            }
            #endregion ADP
            //
            if (isExistManualInput && isExistCalculateBySchedule)
                return Cst.OriginOfFeeEnum.FeeFromManualInputAndCalculateBySchedule;
            else if (isExistCalculateBySchedule)
                return Cst.OriginOfFeeEnum.FeeFromCalculateBySchedule;
            else if (isExistManualInput)
                return Cst.OriginOfFeeEnum.FeeFromManualInput;
            //
            return Cst.OriginOfFeeEnum.NoFee;

        }


        /// <summary>
        /// Suppression des additionalPayment et des otherPartyPayment d'un élément (trade, exeAssAbnOption, ...)
        /// </summary>
        /// <param name="pTarget">Elément contenant les frais</param>
        /// <param name="pMode">Mode de suppression</param>
        // EG 20150708 [21103]
        public void ClearFee(FeeTarget pTarget, ClearFeeMode pMode)
        {
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade != pTarget) & (FeeTarget.denOption != pTarget) & (FeeTarget.none != pTarget))
                throw new NotImplementedException(StrFunc.AppendFormat("Target element is not implemented! [{0}]", pTarget.ToString()));

            #region OPP
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade == pTarget) || (FeeTarget.denOption == pTarget))
            {
                IPayment[] otherPartyPayment = null;

                if (FeeTarget.trade == pTarget)
                {
                    // EG 20150708 [21103] New
                    if (null != safekeepingAction)
                        otherPartyPayment = safekeepingAction.payment;
                    else
                        otherPartyPayment = CurrentTrade.OtherPartyPayment;
                }
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                else if (FeeTarget.denOption == pTarget)
                {
                    otherPartyPayment = tradeDenOption.otherPartyPayment;
                }

                if (ArrFunc.IsFilled(otherPartyPayment))
                {
                    for (int i = otherPartyPayment.Length - 1; -1 < i; i--)
                    {
                        if (IsRemoveForClearFee(pMode, otherPartyPayment[i]))
                        {
                            if (FeeTarget.trade == pTarget)
                            {
                                // EG 20150708 [21103] upd
                                ReflectionTools.AddOrRemoveItemInArray(CurrentTrade, (null != safekeepingAction) ? "payment" : "otherPartyPayment", true, i);
                            }
                            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                            else if ((FeeTarget.denOption == pTarget))
                            {
                                ReflectionTools.AddOrRemoveItemInArray(tradeDenOption, "otherPartyPayment", true, i);
                            }
                        }
                    }
                }
            }
            #endregion OPP

            #region ADP
            if (FeeTarget.trade == pTarget)
            {
                ProductContainer product = null;
                if (CurrentTrade.Product.ProductBase.IsLoanDeposit)
                {
                    product = new ProductContainer(CurrentTrade.Product);
                }
                else if ((CurrentTrade.Product.ProductBase.IsSwap) || CurrentTrade.Product.ProductBase.IsSwaption)
                {
                    if (CurrentTrade.Product.ProductBase.IsSwap)
                    {
                        product = new ProductContainer(CurrentTrade.Product);
                    }
                    else if (CurrentTrade.Product.ProductBase.IsSwaption)
                    {
                        product = new ProductContainer((IProduct)((ISwaption)CurrentTrade.Product).Swap);
                    }
                }
                else if (CurrentTrade.Product.ProductBase.IsCapFloor)
                {
                    product = new ProductContainer(CurrentTrade.Product);
                }

                if ((null != product) && ArrFunc.IsFilled(product.AdditionalPayment))
                {
                    for (int i = product.AdditionalPayment.Length - 1; -1 < i; i--)
                    {
                        IPayment[] additionalPayment = product.AdditionalPayment;

                        if (IsRemoveForClearFee(pMode, additionalPayment[i]))
                        {
                            ReflectionTools.AddOrRemoveItemInArray(product.Product, "additionalPayment", true, i);
                        }
                    }
                }
            }
            #endregion ADP
        }
        /// EG 20201009 Changement de nom pour enum ClearFeeMode
        private bool IsRemoveForClearFee(ClearFeeMode pMode, IPayment pPayment)
        {
            bool isRemove = (pMode == ClearFeeMode.All);
            if (false == isRemove)
            {
                if (ClearFeeMode.FromSchedule == (pMode & ClearFeeMode.FromSchedule))
                    isRemove = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeScheduleScheme);

                if (false == isRemove)
                {
                    if (ClearFeeMode.MissingData == (pMode & ClearFeeMode.MissingData))
                    {
                       isRemove = (false == CaptureTools.IsDocumentElementInCapture(pPayment));
                        if (false == isRemove)
                        {
                            if (ClearFeeMode.Corrected == (pMode & ClearFeeMode.Corrected))
                                isRemove = (pPayment.PaymentSourceSpecified && pPayment.PaymentSource.StatusSpecified && (pPayment.PaymentSource.Status == SpheresSourceStatusEnum.Corrected));

                            if (false == isRemove)
                                isRemove = (ClearFeeMode.Manual == (pMode & ClearFeeMode.Manual));
                        }
                    }
                }
            }
            return isRemove;
        }
        /// <summary>
        /// Ajout, à partir de frais issus du référentiel Conditions/Barèmes, d'éléments additionalPayments et/ou otherPartyPayments.
        /// </summary>
        /// <param name="pFeeTarget">Représente l'élément qui va accepter les frais</param>
        /// <param name="pFees">Array qui contient les frais issus des barèmes</param>
        /// <returns>Message d'information sur les opérations effectuées ou sur les erreurs rencontrées</returns>
        public string SetFee(FeeTarget pFeeTarget, FeeResponse[] pFees)
        {
            return SetFee(pFeeTarget, pFees, false, false);
        }
        
        /// <summary>
        /// Ajout, à partir de frais issus du référentiel Conditions/Barèmes, d'éléments additionalPayments et/ou otherPartyPayments.
        /// </summary>
        /// <param name="pFeeTarget">Représente l'élément qui va accepter les frais</param>
        /// <param name="pFees">Array qui contient les frais issus des barèmes</param>
        /// <param name="pIsWithAuditMsg"></param>
        /// <param name="pIsWithDiscardMsg"></param>
        /// <returns>Message d'information sur les opérations effectuées ou sur les erreurs rencontrées</returns>
        /// EG 20150708 [21103] Upd
        public string SetFee(FeeTarget pFeeTarget, FeeResponse[] pFees, bool _, bool pIsWithAuditMsg)
        {
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade != pFeeTarget) & (FeeTarget.denOption != pFeeTarget) & (FeeTarget.none != pFeeTarget))
                throw new NotImplementedException(StrFunc.AppendFormat("Target Element {0} is not implemented", pFeeTarget.ToString()));

            string auditMsg = string.Empty;
            //string discardMsg = string.Empty;
            string errorMsg = string.Empty;
            string infoMsg = string.Empty;

            foreach (FeeResponse fee in pFees)
            {
                if (fee.AuditMessageSpecified && pIsWithAuditMsg)
                {
                    auditMsg += fee.AuditMessage + Cst.CrLf2;
                }

                if (fee.ErrorMessageSpecified)
                {
                    errorMsg += "- " + fee.ErrorMessage + Cst.CrLf2;
                }
                //else if (fee.DiscardMessageSpecified && pIsWithDiscardMsg)
                //{
                //    discardMsg += "- " + fee.DiscardMessage + Cst.CrLf2;
                //}
                else if (fee.PaymentSpecified)
                {
                    bool isMsgSet = false;
                    foreach (IPayment feePayment in fee.Payment)
                    {
                        IPayment payment = null;

                        if (fee.InfoMessageSpecified && !isMsgSet)
                        {
                            isMsgSet = true;
                            infoMsg += "- " + fee.InfoMessage + Cst.CrLf2;
                        }

                        // EG 20150708 [21103]
                        #region Create FpML Payment Object (OPP|ADP|SKP)
                        switch (fee.EventCode)
                        {
                            case EventCodeEnum.OPP:
                                #region OtherPartyPayment
                                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                                if (pFeeTarget == FeeTarget.denOption)
                                {
                                    // EG 20151102 [21465] Upd
                                    ReflectionTools.AddItemInArray(tradeDenOption, "otherPartyPayment", ArrFunc.Count(tradeDenOption.otherPartyPayment));
                                    tradeDenOption.otherPartyPaymentSpecified = true;
                                    // FI 20180328 [23871] call Tools.SetPaymentId
                                    Tools.SetPaymentId(tradeDenOption.otherPartyPayment, fee.EventCode.ToString()); 
                                    
                                    payment = tradeDenOption.otherPartyPayment[tradeDenOption.otherPartyPayment.Length - 1];
                                }
                                else if (pFeeTarget == FeeTarget.trade)
                                {
                                    payment = DataDocument.AddOtherPartyPayment();
                                    // FI 20180328 [23871] call Tools.SetPaymentId
                                    Tools.SetPaymentId(DataDocument.CurrentTrade.OtherPartyPayment, fee.EventCode.ToString()); 
                                    //DataDocument.currentTrade.otherPartyPaymentSpecified = ArrFunc.IsFilled(DataDocument.currentTrade.otherPartyPayment);
                                }
                                #endregion OtherPartyPayment
                                break;
                            case EventCodeEnum.ADP:
                                #region AdditionalPayment
                                if (pFeeTarget == FeeTarget.trade)
                                {
                                    ITrade trade = CurrentTrade;
                                    #region IsADP
                                    if (Product.IsLoanDeposit)
                                    {
                                        #region IsLoanDeposit
                                        ILoanDeposit loanDeposit = (ILoanDeposit)trade.Product;
                                        
                                        ReflectionTools.AddItemInArray(loanDeposit, "additionalPayment", ArrFunc.Count(loanDeposit.AdditionalPayment));
                                        loanDeposit.AdditionalPaymentSpecified = true;
                                        // FI 20180328 [23871] call Tools.SetPaymentId
                                        Tools.SetPaymentId(loanDeposit.AdditionalPayment, fee.EventCode.ToString()); 
                                        
                                        payment = loanDeposit.AdditionalPayment[loanDeposit.AdditionalPayment.Length - 1];
                                        #endregion
                                    }
                                    else if ((Product.IsSwap) || Product.IsSwaption)
                                    {
                                        #region IsSwap or IsSwaption
                                        ISwap swap = null;
                                        if (Product.IsSwap)
                                            swap = (ISwap)trade.Product;
                                        else
                                            swap = ((ISwaption)trade.Product).Swap;
                                        
                                        ReflectionTools.AddItemInArray(swap, "additionalPayment", ArrFunc.Count(swap.AdditionalPayment));
                                        swap.AdditionalPaymentSpecified = true;
                                        // FI 20180328 [23871] call Tools.SetPaymentId
                                        Tools.SetPaymentId(swap.AdditionalPayment, fee.EventCode.ToString()); 

                                        payment = swap.AdditionalPayment[swap.AdditionalPayment.Length - 1];
                                        #endregion
                                    }
                                    else if (Product.IsCapFloor)
                                    {
                                        #region IsCapFloor
                                        ICapFloor capFloor = (ICapFloor)trade.Product;
                                        
                                        ReflectionTools.AddItemInArray(capFloor, "additionalPayment", ArrFunc.Count(capFloor.AdditionalPayment));
                                        capFloor.AdditionalPaymentSpecified = true;
                                        // FI 20180328 [23871] call Tools.SetPaymentId
                                        Tools.SetPaymentId(capFloor.AdditionalPayment, fee.EventCode.ToString()); 

                                        payment = capFloor.AdditionalPayment[capFloor.AdditionalPayment.Length - 1];
                                        #endregion
                                    }
                                    else if (Product.IsReturnSwap)
                                    {
                                        #region IsReturnSwap
                                        IReturnSwap eqs = (IReturnSwap)trade.Product;

                                        ReflectionTools.AddItemInArray(eqs, "additionalPayment", ArrFunc.Count(eqs.AdditionalPayment));
                                        eqs.AdditionalPaymentSpecified = true;
                                        
                                        IReturnSwapAdditionalPayment returnSwapPayment = eqs.AdditionalPayment[eqs.AdditionalPayment.Length - 1];
                                        returnSwapPayment.PayerPartyReference.HRef = feePayment.PayerPartyReference.HRef;
                                        returnSwapPayment.ReceiverPartyReference.HRef = feePayment.ReceiverPartyReference.HRef;

                                        //PL 20141003 Set paymentAmountSpecified
                                        returnSwapPayment.AdditionalPaymentAmount.PaymentAmountSpecified = true;
                                        returnSwapPayment.AdditionalPaymentAmount.PaymentAmount.Amount = feePayment.PaymentAmount.Amount;
                                        returnSwapPayment.AdditionalPaymentAmount.PaymentAmount.Currency = feePayment.PaymentAmount.Currency;

                                        returnSwapPayment.AdditionalPaymentDate.AdjustableDateSpecified = feePayment.PaymentDateSpecified;
                                        returnSwapPayment.AdditionalPaymentDate.AdjustableDate.UnadjustedDate = feePayment.PaymentDate.UnadjustedDate;
                                        returnSwapPayment.AdditionalPaymentDate.AdjustableDate.DateAdjustments = feePayment.PaymentDate.DateAdjustments;

                                        returnSwapPayment.PaymentTypeSpecified = feePayment.PaymentTypeSpecified;
                                        returnSwapPayment.PaymentType = feePayment.PaymentType;

                                        
                                        #endregion
                                    }
                                    else
                                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                            Ressource.GetString2("Msg_Fees_IncorrectADP", trade.Product.ProductBase.ProductName), new string[] { });
                                    #endregion
                                }
                                #endregion AdditionalPayment
                                break;
                            case EventCodeEnum.SKP:
                                // EG 20150708 [SKP]
                                #region SafekeepingPayment
                                ReflectionTools.AddItemInArray(safekeepingAction, "payment", ArrFunc.Count(safekeepingAction.payment));
                                payment = safekeepingAction.payment[safekeepingAction.payment.Length - 1];
                                safekeepingAction.paymentSpecified = true;
                                #endregion SafekeepingPayment
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat(" Fee Event code {0} is not implemented", fee.EventCode.ToString()));
                            // EG 20160404 Migration vs2013
                            //break;
                        }
                        #endregion Create FpML Payment Object (OPP|ADP|SKP)

                        if (null != payment)
                        {
                            #region Set payment
                            payment.PayerPartyReference.HRef = feePayment.PayerPartyReference.HRef;
                            payment.ReceiverPartyReference.HRef = feePayment.ReceiverPartyReference.HRef;

                            payment.PaymentAmount.Amount = feePayment.PaymentAmount.Amount;
                            payment.PaymentAmount.Currency = feePayment.PaymentAmount.Currency;

                            payment.PaymentDateSpecified = feePayment.PaymentDateSpecified;
                            payment.PaymentDate.UnadjustedDate = feePayment.PaymentDate.UnadjustedDate;
                            payment.PaymentDate.DateAdjustments = feePayment.PaymentDate.DateAdjustments;

                            payment.PaymentTypeSpecified = feePayment.PaymentTypeSpecified;
                            payment.PaymentType = feePayment.PaymentType;

                            payment.PaymentSourceSpecified = feePayment.PaymentSourceSpecified;
                            payment.PaymentSource = feePayment.PaymentSource;
                            #endregion
                        }
                    }
                }
            }

            string retMsg = string.Empty;
            if (StrFunc.IsFilled(infoMsg))
            {
                retMsg += Ressource.GetString("Msg_Fees_InfoFee") + Cst.CrLf2 + infoMsg + Cst.CrLf2;
            }
            if (StrFunc.IsFilled(errorMsg))
            {
                retMsg += Ressource.GetString("Msg_Fees_ErrorFee") + Cst.CrLf2 + errorMsg + Cst.CrLf2;
            }
            //if (StrFunc.IsFilled(discardMsg))
            //{
            //    retMsg += Ressource.GetString("Msg_Fees_DiscardFee") + Cst.CrLf2 + discardMsg + Cst.CrLf2;
            //}
            if (StrFunc.IsFilled(auditMsg))
            {
                retMsg += Ressource.GetString("Track") + Cst.CrLf2 + auditMsg + Cst.CrLf2;
            }
            // EG 20150729 New
            if (StrFunc.IsEmpty(retMsg))
            {
                retMsg = Ressource.GetString("Msg_Fees_NoFee"); //Res: "Aucun frais !"
            }

            return retMsg;
        }

        /// <summary>
        /// Suppression d'éléments additionalPayments et/ou otherPartyPayments existants, puis recalcul et ajout, 
        /// à partir de frais issus du référentiel Conditions/Barèmes, d'éléments additionalPayments et/ou otherPartyPayments.
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pAction"></param>
        /// FI 20161206 [22092] Add pAction
        // EG 20180307 [23769] Gestion dbTransaction
        public void RecalculFeeAndTax(string pCS, IDbTransaction pDbTransaction, string pAction)
        {
            //Purge
            ClearFee(TradeInput.FeeTarget.trade, TradeInput.ClearFeeMode.All);

            //Calcul
            FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(pCS), pDbTransaction, this, pAction);
            FeeProcessing fees = new FeeProcessing(feeRequest);
            
            fees.Calc(CSTools.SetCacheOn(pCS), pDbTransaction);

            if (ArrFunc.IsFilled(fees.FeeResponse))
            {
                SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse);
                ProcessFeeTax(pCS, pDbTransaction, TradeInput.FeeTarget.trade, feeRequest.DtReference);
            }
        }


        /// <summary>
        /// Suppression d'éléments additionalPayments et/ou otherPartyPayments existants, puis recalcul et ajout, 
        /// à partir de frais issus du référentiel Conditions/Barèmes, d'éléments additionalPayments et/ou otherPartyPayments.
        /// <para>Action : IdMenu.GetIdMenu(IdMenu.Menu.InputTrade) </para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public void RecalculFeeAndTax(string pCS, IDbTransaction pDbTransaction)
        {
            RecalculFeeAndTax(pCS, pDbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
        }



        /// <summary>
        /// Identifie les frais "similaires" entre ceux saisis manuellement et ceux issus du référentiel Conditions/Barèmes,
        /// afin de substituer le montant de ceux issus du référentiel par celui ceux saisis manuellement.
        /// <para>
        /// NB: On considère comme frais "similaires" les frais de mêmes: Payer/Receiver/Currency/PaymentType/PaymentDate
        /// </para>
        /// </summary>
        /// <returns></returns>
        public string SubstituteFeeSchedule()
        {
            string errorMsg = string.Empty;
            string infoMsg = string.Empty;

            //WARNING: On ne traite pour l'instant que des OPP.            
            if (DataDocument.CurrentTrade.OtherPartyPaymentSpecified)
            {
                IEnumerable<IPayment> opps_manual = from opp in DataDocument.CurrentTrade.OtherPartyPayment
                                                    where ((!opp.PaymentSourceSpecified) || (opp.PaymentSourceSpecified && (!opp.PaymentSource.StatusSpecified)))
                                                       && opp.PaymentTypeSpecified && opp.PaymentDateSpecified
                                                    select opp;

                IEnumerable<IPayment> opps_schedule = from opp in DataDocument.CurrentTrade.OtherPartyPayment
                                                      where (opp.PaymentSourceSpecified && opp.PaymentSource.StatusSpecified)
                                                         && opp.PaymentTypeSpecified && opp.PaymentDateSpecified
                                                      select opp;

                foreach (IPayment opp_manual in opps_manual)
                {
                    IEnumerable<IPayment> opp_schedule = from opp in opps_schedule
                                                         where opp.PayerPartyReference.HRef == opp_manual.PayerPartyReference.HRef
                                                            && opp.ReceiverPartyReference.HRef == opp_manual.ReceiverPartyReference.HRef
                                                            && opp.PaymentCurrency == opp_manual.PaymentCurrency
                                                            && opp.PaymentType.Value == opp_manual.PaymentType.Value
                                                            && opp.PaymentDate.UnadjustedDate.DateValue == opp_manual.PaymentDate.UnadjustedDate.DateValue
                                                         select opp;
                    int opp_schedule_Count = opp_schedule.Count();
                    IPayment opp_schedule_forced = null;
                    if (opp_schedule_Count > 0)
                    {
                        opp_schedule_forced = opp_schedule.First();

                        infoMsg += String.Format(@"- OPP \ {0}: {1} - {2}",
                                    opp_schedule_forced.PaymentType.Value,
                                    opp_schedule_forced.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).Value,
                                    opp_schedule_forced.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).Value) + Cst.CrLf + "    ";
                    }
                    switch (opp_schedule_Count)
                    {
                        case 0: //Aucun frais similaire trouvé issu du référentiel Conditions/Barèmes 
                            break;
                        case 1: //Un frais similaire trouvé issu du référentiel Conditions/Barèmes 
                            //PL 20141017
                            infoMsg += Ressource.GetString2("Msg_SubstituteFeeInformation",
                                        DtFunc.DateTimeToString(opp_schedule_forced.PaymentDate.UnadjustedDate.DateValue, DtFunc.FmtShortDate),
                                        opp_schedule_forced.PayerPartyReference.HRef,
                                        opp_schedule_forced.ReceiverPartyReference.HRef,
                                        StrFunc.FmtMoneyToCurrentCulture(opp_manual.PaymentAmount.Amount.DecValue, opp_schedule_forced.PaymentCurrency),
                                        StrFunc.FmtMoneyToCurrentCulture(opp_schedule_forced.PaymentAmount.Amount.DecValue, opp_schedule_forced.PaymentCurrency));

                            //Forçage du montant issu du référentiel Conditions/Barèmes
                            opp_schedule_forced.PaymentAmount.Amount.DecValue = opp_manual.PaymentAmount.Amount.DecValue;
                            opp_schedule_forced.PaymentSource.Status = SpheresSourceStatusEnum.Forced;

                            //Suppression du montant saisi manuellement. 
                            //NB: Pour celà, on initialise à "null" le Payer afin que l'élément Payment soit purgé ultérieurement par CleanUp().
                            opp_manual.PayerPartyReference.HRef = null;
                            opp_manual.ReceiverPartyReference.HRef = null;
                            opp_manual.PaymentAmount.Amount.DecValue = 0;
                            break;
                        default: //Plusieurs frais similaires trouvés issus du référentiel Conditions/Barèmes 
                            //WARNING: Aucune substitution opérée !
                            errorMsg += Ressource.GetString2("Msg_NoSubstituteFeeInformation",
                                        DtFunc.DateTimeToString(opp_schedule_forced.PaymentDate.UnadjustedDate.DateValue, DtFunc.FmtShortDate),
                                        opp_schedule_forced.PayerPartyReference.HRef,
                                        opp_schedule_forced.ReceiverPartyReference.HRef,
                                        opp_schedule_forced.PaymentCurrency);
                            break;
                    }
                }
            }

            string retMsg = string.Empty;
            if (StrFunc.IsFilled(infoMsg))
                retMsg += Ressource.GetString("Msg_Fees_InfoFee") + Cst.CrLf2 + infoMsg + Cst.CrLf2;
            if (StrFunc.IsFilled(errorMsg))
                retMsg += Ressource.GetString("Msg_Fees_ErrorFee") + Cst.CrLf2 + errorMsg + Cst.CrLf2;

            return retMsg;
        }

        /// <summary>
        /// Compare les éléments additionalPayments et/ou otherPartyPayments à partir de frais issus d'un barème, 
        /// avec les éléments additionalPayments et/ou otherPartyPayments existants sur le trade.
        /// </summary>
        /// <param name="pFees">Array qui contient les frais issus des barèmes</param>
        /// <param name="pRetMsg">Message d'information sur les opérations effectuées ou sur les erreurs rencontrées</param>
        /// <returns>True s'il n'existe pas de différence</returns>
        /// FI 20140814 [20286] Modify
        public bool CompareFee(FeeResponse[] pFees, out string pRetMsg)
        {
            bool isOk = true;
            pRetMsg = string.Empty;

            string errorMsg = string.Empty;
            string infoMsg = string.Empty;

            List<IPayment> tradePaymentOPP = new List<IPayment>();
            List<IPayment> tradePaymentADP = new List<IPayment>();
            List<IReturnSwapAdditionalPayment> tradeRSPaymentADP = new List<IReturnSwapAdditionalPayment>();

            if (DataDocument.CurrentTrade.OtherPartyPaymentSpecified)
                tradePaymentOPP.AddRange(DataDocument.CurrentTrade.OtherPartyPayment);

            if (Product.IsLoanDeposit)
            {
                #region IsLoanDeposit
                ILoanDeposit loanDeposit = (ILoanDeposit)CurrentTrade.Product;

                if (loanDeposit.AdditionalPaymentSpecified)
                    tradePaymentADP.AddRange(loanDeposit.AdditionalPayment);
                #endregion
            }
            else if ((Product.IsSwap) || Product.IsSwaption)
            {
                #region IsSwap or IsSwaption
                ISwap swap = null;
                if (Product.IsSwap)
                    swap = (ISwap)CurrentTrade.Product;
                else
                    swap = ((ISwaption)CurrentTrade.Product).Swap;

                if (swap.AdditionalPaymentSpecified)
                    tradePaymentADP.AddRange(swap.AdditionalPayment);
                #endregion
            }
            else if (Product.IsCapFloor)
            {
                #region IsCapFloor
                ICapFloor capFloor = (ICapFloor)CurrentTrade.Product;

                if (capFloor.AdditionalPaymentSpecified)
                    tradePaymentADP.AddRange(capFloor.AdditionalPayment);
                #endregion
            }
            else if (Product.IsReturnSwap)
            {
                #region IsReturnSwap
                IReturnSwap eqs = (IReturnSwap)CurrentTrade.Product;

                if (eqs.AdditionalPaymentSpecified)
                    tradeRSPaymentADP.AddRange(eqs.AdditionalPayment);

                // NB: sur le IReturnSwapAdditionalPayment, il n'existe pas de PaymentSource
                #endregion
            }

            int countTradePaymentOPP = tradePaymentOPP.Select(payment => Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme)).Count();
            int countTradePaymentADP = tradePaymentADP.Select(payment => Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme)).Count();

            int countPaymentOPP = 0;
            int countPaymentADP = 0;

            // Pas de frais re-calculés
            // Des frais calculés existent sur le trade
            if ((ArrFunc.IsEmpty(pFees)) && (countTradePaymentOPP != 0 || countTradePaymentADP != 0))
            {
                isOk = false;
                infoMsg = Ressource.GetString("Msg_Fees_NoFee"); //Res: "Aucun frais !"
            }

            if (isOk && ArrFunc.IsFilled(pFees))
            {
                foreach (FeeResponse fee in pFees)
                {
                    // Au moins un Payment est re-calculé
                    // Par contre il n'existe pas sur le trade
                    if (fee.ErrorMessageSpecified)
                    {
                        errorMsg += "- " + fee.ErrorMessage + Cst.CrLf + Cst.CrLf;
                    }
                    else if (fee.PaymentSpecified)
                    {
                        bool isMsgSet = false;
                        foreach (IPayment feePayment in fee.Payment)
                        {
                            if (fee.InfoMessageSpecified && !isMsgSet)
                            {
                                isMsgSet = true;
                                infoMsg += "- " + fee.InfoMessage + Cst.CrLf + Cst.CrLf;
                            }

                            if (isOk)
                            {
                                if (EventCodeFunc.IsOtherPartyPayment(fee.EventCode.ToString()))
                                {
                                    // Le Payment OPP est re-calculé
                                    // Par contre il n'existe pas sur le trade

                                    // FI 20140814 [20286] 
                                    // Réécrirure de la requête Linq pour ne pas planter lorsque les frais du trade sont issus d'une saisie manuelle
                                    // Les données OTCml_RepositoryFeeScheduleScheme et OTCml_RepositoryFeeMatrixScheme ne sont pas présentes dans les frais saisie mauellement
                                    // => Spheres® n'en tient pas compte dans ce contrôle
                                    countPaymentOPP++;
                                    isOk = tradePaymentOPP.Exists(payment =>
                                        (payment.PayerPartyReference.HRef == feePayment.PayerPartyReference.HRef) &&
                                        (payment.ReceiverPartyReference.HRef == feePayment.ReceiverPartyReference.HRef) &&

                                        (payment.PaymentAmount.Amount.DecValue == feePayment.PaymentAmount.Amount.DecValue) &&
                                        (payment.PaymentAmount.Currency == feePayment.PaymentAmount.Currency) &&

                                        (payment.PaymentDateSpecified == feePayment.PaymentDateSpecified) &&
                                        (payment.PaymentDate.UnadjustedDate.DateValue == feePayment.PaymentDate.UnadjustedDate.DateValue) &&

                                        (payment.PaymentTypeSpecified == feePayment.PaymentTypeSpecified) &&
                                        (payment.PaymentType.Value == feePayment.PaymentType.Value) &&

                                        (payment.PaymentSourceSpecified == feePayment.PaymentSourceSpecified) &&
                                        (
                                            (payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value ==
                                                        feePayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value)
                                        )
                                        //&&
                                        //(
                                        //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme) != null)
                                        //    &&
                                        //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId ==
                                        //                feePayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId)
                                        //)
                                        //&&
                                        //(
                                        //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme) != null)
                                        //    &&
                                        //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId ==
                                        //      feePayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId))
                                        );
                                }
                                else if (EventCodeFunc.IsAdditionalPayment(fee.EventCode.ToString()))
                                {
                                    // Le Payment ADP est re-calculé
                                    // Par contre il n'existe pas sur le trade
                                    countPaymentADP++;

                                    if (Product.IsReturnSwap)
                                    {
                                        isOk = tradeRSPaymentADP.Exists(returnSwapPayment =>
                                            (returnSwapPayment.PayerPartyReference.HRef == feePayment.PayerPartyReference.HRef) &&
                                            (returnSwapPayment.ReceiverPartyReference.HRef == feePayment.ReceiverPartyReference.HRef) &&

                                            (returnSwapPayment.AdditionalPaymentAmount.PaymentAmount.Amount.DecValue == feePayment.PaymentAmount.Amount.DecValue) &&
                                            (returnSwapPayment.AdditionalPaymentAmount.PaymentAmount.Currency == feePayment.PaymentAmount.Currency) &&

                                            (returnSwapPayment.AdditionalPaymentDate.AdjustableDateSpecified == feePayment.PaymentDateSpecified) &&
                                            (returnSwapPayment.AdditionalPaymentDate.AdjustableDate.UnadjustedDate.DateValue == feePayment.PaymentDate.UnadjustedDate.DateValue) &&

                                            (returnSwapPayment.PaymentTypeSpecified == feePayment.PaymentTypeSpecified) &&
                                            (returnSwapPayment.PaymentType.Value == feePayment.PaymentType.Value));
                                    }
                                    else
                                    {
                                        // FI 20140814 [20286] 
                                        // Réécrirure de la requête Linq pour ne pas planter lorsque les frais du trade sont issus d'une saisie manuelle
                                        // Les données OTCml_RepositoryFeeScheduleScheme et OTCml_RepositoryFeeMatrixScheme et OTCml_RepositoryFeeInvoicingScheme ne sont pas présents dans les frais saisie manuellement
                                        // => Spheres® n'en tient pas compte dans ce contrôle
                                        isOk = tradePaymentADP.Exists(payment =>
                                            (payment.PayerPartyReference.HRef == feePayment.PayerPartyReference.HRef) &&
                                            (payment.ReceiverPartyReference.HRef == feePayment.ReceiverPartyReference.HRef) &&

                                            (payment.PaymentAmount.Amount.DecValue == feePayment.PaymentAmount.Amount.DecValue) &&
                                            (payment.PaymentAmount.Currency == feePayment.PaymentAmount.Currency) &&

                                            (payment.PaymentDateSpecified == feePayment.PaymentDateSpecified) &&
                                            (payment.PaymentDate.UnadjustedDate.DateValue == feePayment.PaymentDate.UnadjustedDate.DateValue) &&

                                            (payment.PaymentTypeSpecified == feePayment.PaymentTypeSpecified) &&
                                            (payment.PaymentType.Value == feePayment.PaymentType.Value)
                                            //&&
                                            //(payment.paymentSourceSpecified == feePayment.paymentSourceSpecified)
                                            //&&
                                            //(
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme) != null)
                                            //    &&
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value ==
                                            //                feePayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value)
                                            //)
                                            //&&
                                            //(
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme) != null)
                                            //    &&
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId ==
                                            //                feePayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId)
                                            //)
                                            //&&
                                            //(
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme) != null)
                                            //    &&
                                            //    (payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId ==
                                            //      feePayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId))
                                            );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (isOk)
            {
                // Il existe un Payment OPP déjà calculé sur le trade
                // Par contre il n'est pas re-calculé
                if (tradePaymentOPP.Select(payment => Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme)).Count() != countPaymentOPP)
                    isOk = false;
            }

            if (isOk)
            {
                if (Product.IsReturnSwap)
                {
                    // Sur le IReturnSwapAdditionalPayment, il n'existe pas de PaymentSource, donc on ne peut pas savoir s'il est calculé ou pas.
                }
                else
                {
                    // Il existe un Payment ADP déjà calculé sur le trade
                    // Par contre il n'est pas re-calculé
                    if (tradePaymentADP.Select(payment => Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme)).Count() != countPaymentADP)
                        isOk = false;
                }
            }

            if (false == isOk)
            {
                if (StrFunc.IsFilled(infoMsg))
                    pRetMsg += Ressource.GetString("Msg_Fees_InfoFeeCheck") + Cst.CrLf + Cst.CrLf + infoMsg + Cst.CrLf + Cst.CrLf;
                if (StrFunc.IsFilled(errorMsg))
                    pRetMsg += Ressource.GetString("Msg_Fees_ErrorFee") + Cst.CrLf + Cst.CrLf + errorMsg + Cst.CrLf + Cst.CrLf;
            }
            //
            return isOk;
        }

        /// <summary>
        /// Retourne "true" s'il doit y avoir calcule et injection des frais issus du référentiel Conditions/Barèmes (comportement normal).
        /// </summary>
        /// EG 20190613 Upd Test ClosingReopeningPosition
        public bool IsApplyFeeCalculation(FeeTarget pFeeTarget)
        {
            bool ret = false;
            if (pFeeTarget == FeeTarget.trade)
            {
                ret = true;
                // RD 20130205 [18388] Ne pas calculer des frais automatiques pour les Futures livrés suite à Exercice/Assignation d'Option
                //Spheres® n'injecte pas les frais:
                // 1- si le trade n'est pas un "trade de marché", mais un "trade ouverture de position".
                // 2- ou bien c'est un trade généré à la suite d'un Exercice/Assignation d'Option
                // PM 20130215 [18414] : Ne pas calculer des frais automatiques pour les position issu de Cascading
                // 3- ou bien c'est un trade généré à la suite d'un Cascading
                // EG 20130607
                // 4- Ne pas calculer des frais automatiques pour les trades ajustés suite à CA
                // EG 20180613
                // 5- Ne pas calculer des frais automatiques pour les trades fermés et réouvert suite à CLOSINGREOPENINGPOSITION
                if (IsETDandAllocation)
                {
                    ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)Product.Product, DataDocument);
                    if (etd.IsPositionOpening || etd.IsOptionExercise || etd.IsCascading || etd.IsTradeCAAdjusted || etd.IsClosingReopeningPosition)
                        ret = false;
                }
                else if (IsESEandAllocation)
                {
                    EquitySecurityTransactionContainer ese = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)Product.Product, DataDocument);
                    if (ese.IsClosingReopeningPosition)
                        ret = false;
                }
               //FI 20110426 [17423] Pas de frais sur les factures
                else if (SQLProduct.IsAdministrativeProduct)
                {
                    ret = false;
                }
                // Pas de frais sur les trades risk
                else if (SQLProduct.IsRiskProduct)
                {
                    ret = false;
                }
            }
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            else if (pFeeTarget == FeeTarget.denOption)
            {
                ret = true;
                // RD 20160519 [22173] Ne pas calculer des frais automatiques pour "annulation des traitements du jour"                
                if (tradeDenOption != null && tradeDenOption.denOptionActionType == Cst.DenOptionActionType.remove)
                    ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Calcul les taxes sur les frais, avec comme date de référence {pDtReference}
        /// </summary>
        /// <param name="pFeeTarget"></param>
        /// <param name="pDtReference"></param>
        /// <returns></returns>
        // EG 20150708 [21103] Upd
        public bool ProcessFeeTax(string pCS, IDbTransaction pDbTransaction, FeeTarget pFeeTarget, DateTime pDtReference)
        {
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade != pFeeTarget) & (FeeTarget.denOption != pFeeTarget) & (FeeTarget.none != pFeeTarget))
                throw new NotImplementedException(StrFunc.AppendFormat("Target Element {0} is not implemented", pFeeTarget.ToString()));
            //
            bool isOk = true;
            if (FeeTarget.trade == pFeeTarget)
            {
                #region AdditionalPayment
                IProductBase product = Product.ProductBase;
                IPayment[] additionalPayment = null;
                if (product.IsLoanDeposit)
                {
                    ILoanDeposit loanDeposit = (ILoanDeposit)product;
                    if (loanDeposit.AdditionalPaymentSpecified)
                        additionalPayment = loanDeposit.AdditionalPayment;
                }
                else if (product.IsSwap)
                {
                    ISwap swap = (ISwap)CurrentTrade.Product;
                    if (swap.AdditionalPaymentSpecified)
                        additionalPayment = swap.AdditionalPayment;
                }
                else if (product.IsSwaption)
                {
                    ISwap swap = ((ISwaption)CurrentTrade.Product).Swap;
                    if (swap.AdditionalPaymentSpecified)
                        additionalPayment = swap.AdditionalPayment;
                }
                else if (product.IsCapFloor)
                {
                    ICapFloor capFloor = (ICapFloor)CurrentTrade.Product;
                    if (capFloor.AdditionalPaymentSpecified)
                        additionalPayment = capFloor.AdditionalPayment;
                }
                //else if (product.IsReturnSwap)
                //{
                //    IReturnSwap eqs = (IReturnSwap)TradeInput.CurrentTrade.product;
                //    if (eqs.additionalPaymentSpecified)
                //        additionalPayment = eqs.additionalPayment;
                //}
                if (null != additionalPayment)
                {
                    foreach (IPayment payment in additionalPayment)
                        isOk = isOk && SetFeeTax(pCS, pDbTransaction, payment, pDtReference);
                }
                #endregion AdditionalPayment
            }
            // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
            if ((FeeTarget.trade == pFeeTarget) || (FeeTarget.denOption == pFeeTarget))
            {
                IPayment[] otherPartyPayment = null;
                bool otherPartyPaymentSpecified = false;

                if (FeeTarget.trade == pFeeTarget)
                {
                    // EG 20150708 [21103] New
                    if (null != safekeepingAction)
                    {
                        otherPartyPayment = safekeepingAction.payment;
                        otherPartyPaymentSpecified = safekeepingAction.paymentSpecified;
                    }
                    else
                    {
                        otherPartyPayment = CurrentTrade.OtherPartyPayment;
                        otherPartyPaymentSpecified = CurrentTrade.OtherPartyPaymentSpecified;
                    }
                }
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                else if (FeeTarget.denOption == pFeeTarget)
                {
                    otherPartyPayment = tradeDenOption.otherPartyPayment;
                    otherPartyPaymentSpecified = tradeDenOption.otherPartyPaymentSpecified;
                }
                #region OtherPartyPayment
                if (otherPartyPaymentSpecified)
                {
                    foreach (IPayment payment in otherPartyPayment)
                        isOk = isOk && SetFeeTax(pCS, pDbTransaction, payment, pDtReference);
                }
                #endregion OtherPartyPayment
            }
            return isOk;

        }

        /// <summary>
        /// Calcul les taxes sur {pPayment} avec comme date de référence {pDtReference}
        /// </summary>
        /// <param name="pPayment"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> for idAReceiver|idAPayer
        // EG 20180307 [23769] Gestion dbTransaction
        private bool SetFeeTax(string pCS , IDbTransaction pDbTransacton, IPayment pPayment, DateTime pDtReference)
        {
            bool isOk = true;

            // RD 20130205 [18389] Utiliser la bonne date de référence, selon s'il s'agit d'un ETD ou pas, d'une action sur trade ou pas.
            DateTime dtTransac = (DtFunc.IsDateTimeFilled(pDtReference) ? pDtReference : DataDocument.TradeDate);
            IProductBase productBase = (IProductBase)DataDocument.CurrentProduct.ProductBase;
            Nullable<int> idAReceiver = DataDocument.GetOTCmlId_Party(pPayment.ReceiverPartyReference.HRef);
            ISpheresIdSchemeId spheresFeeEventTypeScheme = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeEventTypeScheme);
            if (null != spheresFeeEventTypeScheme)
            {
                bool isTaxApplied = true;
                #region TaxApplication
                ISpheresIdSchemeId spheresFeeTaxApplication = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeTaxApplicationScheme);
                if ((null != spheresFeeTaxApplication) && StrFunc.IsFilled(spheresFeeTaxApplication.Value))
                {
                    // EG Tester la condition (lorsqu'elle sera gérée)
                    TaxApplicationEnum taxApplication = (TaxApplicationEnum)Enum.Parse(typeof(TaxApplicationEnum), spheresFeeTaxApplication.Value, true);
                    isTaxApplied = (taxApplication == TaxApplicationEnum.Always);
                }
                #endregion TaxApplication

                if (isTaxApplied)
                {
                    #region isTaxAmountCalculated
                    //WARNING: Pas de calcul de taxe sur le trade, lors de frais destinés à être facturés au sein d'une facture (EventClass=INV).
                    //         Sur de tels frais, s'il y a lieu, les taxes seront calculées lors de la génération de la facture.
                    //         Cependant, on insère tout de même sur le trade toutes les informations relatives aux taxes, afin que celles-ci puissent être calculés lors de la génération de la facture.
                    bool isTaxAmountCalculated = false;
                    if (pPayment.PaymentSourceSpecified)
                    {
                        ISpheresIdSchemeId spheresIdScheme = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme);
                        isTaxAmountCalculated = (null == spheresIdScheme) || BoolFunc.IsFalse(spheresIdScheme.Value);
                    }
                    #endregion isTaxAmountCalculated

                    #region Query Tax
                    DataParameters dp = new DataParameters();
                    dp.Add(new DataParameter(pCS, "EVENTTYPE", DbType.String, SQLCst.UT_EVENT_LEN), spheresFeeEventTypeScheme.Value);
                    dp.Add(new DataParameter(pCS, "DTTRANSAC", DbType.Date), dtTransac);
                    dp.Add(new DataParameter(pCS, "IDA_RECEIVER", DbType.Int32), idAReceiver ?? Convert.DBNull);

                    StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                    sqlSelect += "tx.IDTAX,tx.IDENTIFIER,tx.DISPLAYNAME" + Cst.CrLf;
                    sqlSelect += ",txd.IDTAXDET,txd.IDENTIFIER as IDENTIFIERDET,txd.IDCOUNTRY,txd.TAXTYPE,txd.TAXRATE,txd.EVENTTYPE" + Cst.CrLf;
                    //PL 20150317 Calcul des événements de taxes, même lorsque celle-ci n'est pas relative au pays du collecteur. Dans ce cas génération uniquement des événements REC.
                    sqlSelect += ",case when a_collect.IDCOUNTRYTAX=txd.IDCOUNTRY then 1 else 0 end as ISCOLLECTED" + Cst.CrLf;
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TAX.ToString() + " tx" + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TAXEVENT.ToString() + " txe on (txe.IDTAX=tx.IDTAX) and (txe.EVENTTYPE=@EVENTTYPE)";
                    sqlSelect += " and (" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "txe", "@DTTRANSAC") + ")" + Cst.CrLf;
                    //Restriction aux seules taxes collectées par le Receiver
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORTAX.ToString() + " atx on (atx.IDTAX=tx.IDTAX) and (atx.IDA=@IDA_RECEIVER)";
                    sqlSelect += " and (" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "atx", "@DTTRANSAC") + ")" + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_collect on (a_collect.IDA=atx.IDA)" + Cst.CrLf;
                    //Restriction aux seules taxes en vigueur dans le pays fiscal du Receiver                    
                    //sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TAXDET.ToString() + " txd on (txd.IDTAX=tx.IDTAX) and (txd.IDCOUNTRY=a_collect.IDCOUNTRYTAX)";
                    //PL 20150317 Calcul des événements de taxes, même lorsque celle-ci n'est pas relative au pays du collecteur. Dans ce cas génération uniquement des événements REC.
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TAXDET.ToString() + " txd on (txd.IDTAX=tx.IDTAX)";
                    sqlSelect += " and (" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "txd", "@DTTRANSAC") + ")" + Cst.CrLf;

                    if (isTaxAmountCalculated)
                    {
                        //Restriction aux seules taxes en vigueur dans le pays fiscal du Payer, dans le cas de frais "non réglés" dans le cadre d'une facture (EventClass=STL)
                        //NB: Dans le cas de frais "réglés" dans le cadre d'une facture (EventClass=INV), le payeur est le payeur de la facture et les condtions relatives à ce derner 
                        //    sont définies sur le référnetiel "Règles de facturation". 
                        Nullable<int> idAPayer = DataDocument.GetOTCmlId_Party(pPayment.PayerPartyReference.HRef);
                        // EG 20150706 [21021]
                        dp.Add(new DataParameter(pCS, "IDA_PAYER", DbType.Int32), idAPayer ?? Convert.DBNull);
                        sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_liable on (a_liable.IDCOUNTRYTAX=txd.IDCOUNTRY) and (a_liable.IDA=@IDA_PAYER)" + Cst.CrLf;
                    }
                    sqlSelect += SQLCst.WHERE + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "tx", "@DTTRANSAC") + ")";
                    sqlSelect += SQLCst.ORDERBY + "tx.IDTAX";
                    #endregion Query Tax

                    #region Filling Tax
                    QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);
                    //DataSet ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(CS), CommandType.Text, sqlSelect);
                    DataSet ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(pCS), pDbTransacton, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    if ((null != ds) && (0 < ds.Tables[0].Rows.Count))
                    {
                        int nbRow = ds.Tables[0].Rows.Count;
                        ArrayList aTax = new ArrayList();
                        ArrayList aTaxDetail = new ArrayList();
                        int prevIdTax = 0;
                        ITax tax = null;
                        for (int i = 0; i < nbRow; i++)
                        {
                            DataRow row = ds.Tables[0].Rows[i];
                            if ((0 != prevIdTax) && (prevIdTax != Convert.ToInt32(row["IDTAX"])))
                            {
                                tax.TaxDetail = (ITaxSchedule[])aTaxDetail.ToArray(aTaxDetail[0].GetType());
                                aTax.Add(tax);
                                aTaxDetail = new ArrayList();
                            }
                            if (prevIdTax != Convert.ToInt32(row["IDTAX"]))
                            {
                                #region Tax Group
                                tax = pPayment.CreateTax;
                                #region Source Tax Group
                                tax.TaxSource = pPayment.CreateSpheresSource;
                                tax.TaxSource.SpheresId = productBase.CreateSpheresId(2);
                                tax.TaxSource.SpheresId[0].Scheme = Cst.OTCml_RepositoryTaxScheme;
                                tax.TaxSource.SpheresId[0].OTCmlId = Convert.ToInt32(row["IDTAX"]);
                                tax.TaxSource.SpheresId[0].Value = row["IDENTIFIER"].ToString();
                                // EventType
                                tax.TaxSource.SpheresId[1].Scheme = Cst.OTCml_RepositoryFeeEventTypeScheme;
                                tax.TaxSource.SpheresId[1].Value = spheresFeeEventTypeScheme.Value;
                                #endregion Source Tax Group
                                #endregion Tax Group
                            }

                            #region Tax Details
                            ITaxSchedule taxSchedule = pPayment.CreateTaxSchedule;
                            taxSchedule.TaxAmountSpecified = isTaxAmountCalculated;
                            #region Amount calculation
                            if (taxSchedule.TaxAmountSpecified)
                            {
                                decimal taxRate = Convert.ToDecimal(row["TAXRATE"]);
                                //FI 20110914 Pas d'arrondi sur les taxes, afin de disposer du maximum de décimales significatives et ainsi pas perdre de centimes lors  
                                //EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(CS), pPayment.paymentAmount.amount.DecValue * taxRate, pPayment.paymentAmount.currency);
                                decimal taxAmount = pPayment.PaymentAmount.Amount.DecValue * taxRate;
                                taxSchedule.TaxAmount = taxSchedule.CreateTripleInvoiceAmounts;
                                taxSchedule.TaxAmount.Amount = productBase.CreateMoney(taxAmount, pPayment.PaymentAmount.Currency);
                                // RD 20110930 / issueAmount ne sert que pour la facturation
                                //taxSchedule.taxAmount.issueAmount = productBase.CreateMoney(taxAmount, pPayment.paymentAmount.currency);
                            }
                            #endregion Amount calculation

                            #region Source Tax Details
                            ISpheresSource source = pPayment.CreateSpheresSource;
                            source.SpheresId = productBase.CreateSpheresId(6);
                            // Identifier TAXDET
                            source.SpheresId[0].Scheme = Cst.OTCml_RepositoryTaxDetailScheme;
                            source.SpheresId[0].OTCmlId = Convert.ToInt32(row["IDTAXDET"]);
                            source.SpheresId[0].Value = row["IDENTIFIERDET"].ToString();
                            // Country
                            source.SpheresId[1].Scheme = Cst.OTCml_RepositoryTaxDetailCountryScheme;
                            source.SpheresId[1].Value = row["IDCOUNTRY"].ToString();
                            // Type
                            source.SpheresId[2].Scheme = Cst.OTCml_RepositoryTaxDetailTypeScheme;
                            source.SpheresId[2].Value = row["TAXTYPE"].ToString();
                            // Rate
                            source.SpheresId[3].Scheme = Cst.OTCml_RepositoryTaxDetailRateScheme;
                            source.SpheresId[3].Value = StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(row["TAXRATE"]));
                            // EventType for TAX
                            source.SpheresId[4].Scheme = Cst.OTCml_RepositoryTaxDetailEventTypeScheme;
                            source.SpheresId[4].Value = row["EVENTTYPE"].ToString();
                            // Indicator for TAX Collected (PL 20150317)
                            source.SpheresId[5].Scheme = Cst.OTCml_RepositoryTaxDetailCollected;
                            source.SpheresId[5].Value = BoolFunc.IsTrue(row["ISCOLLECTED"]) ? "true" : "false";
                            #endregion Source Tax Details

                            taxSchedule.TaxSource = source;
                            aTaxDetail.Add(taxSchedule);
                            #endregion Tax Details

                            prevIdTax = Convert.ToInt32(row["IDTAX"]);
                        }
                        if (0 < aTaxDetail.Count)
                        {
                            tax.TaxDetail = (ITaxSchedule[])aTaxDetail.ToArray(aTaxDetail[0].GetType());
                            aTax.Add(tax);
                        }
                        pPayment.TaxSpecified = (0 < aTax.Count);
                        if (pPayment.TaxSpecified)
                            pPayment.Tax = (ITax[])aTax.ToArray(aTax[0].GetType());
                    }
                    else
                    {
                        // RD 20170111 [22748] Purge des taxes
                        pPayment.TaxSpecified = false;
                        pPayment.Tax = null;
                    }
                    #endregion Filling Tax
                }
            }
            return isOk;
        }

        private void ResetFlagProvision()
        {
            _isTradeCancelable = false;
            _isTradeExtendible = false;
            _isTradeMandatoryEarlyTermination = false;
            _isTradeOptionalEarlyTermination = false;
            _isTradeStepUp = false;
        }

        /// <summary>
        /// Ajoute un book sur la party qui représente l'entité, si elle est ni acheteur, ni vendeur, et qu'elle prend part à un opp 
        /// <para>Le book est celui par défaut rattaché à l'entité (colonne ENTITY.IDB_OPP)</para>
        /// </summary>
        /// <param name="pParty"></param>
        ///FI 20100623 [17064]    
        // EG 20180307 [23769] Gestion dbTransaction
        private void AddBookForOtherPartyPayment(string pCS, IDbTransaction pDbTrancation)
        {
            IPartyTradeIdentifier partyTradeIdentifier = null;
            ActorRoleCollection actorRoleActor = DataDocument.GetActorRole(pCS, pDbTrancation);
            //
            foreach (IParty party in DataDocument.Party)
            {
                // RD [19593] Pour les trade ETD Allocation, ajouter systématiquement un book sur la party qui représente l'entité: 
                // - si elle est ni acheteur, ni vendeur, 
                // - qu'elle prend part OU PAS à un opp
                // Ainsi à la saisie des frais (OPP) durant la vie du trade (Exercice, ...) il existera toujours un book de rattacher à l'Entity

                bool isAddBook = (
                            (false == actorRoleActor.IsActorRole(party.OTCmlId, RoleActor.COUNTERPARTY))
                            && (actorRoleActor.IsActorRole(party.OTCmlId, RoleActor.BROKER))
                            && (actorRoleActor.IsActorRole(party.OTCmlId, RoleActor.ENTITY))
                            );

                if (isAddBook)
                {
                    if (false == this.IsAllocation)
                    {
                        isAddBook = false;

                        if (DataDocument.OtherPartyPaymentSpecified)
                        {
                            foreach (IPayment payment in DataDocument.OtherPartyPayment)
                            {
                                isAddBook = (payment.PayerPartyReference.HRef == party.Id) || (payment.ReceiverPartyReference.HRef == party.Id);
                                if (isAddBook)
                                    break;
                            }
                        }
                    }
                }

                if (isAddBook)
                {
                    partyTradeIdentifier = DataDocument.GetPartyTradeIdentifier(party.Id);
                    if (null != partyTradeIdentifier)
                        isAddBook = (false == partyTradeIdentifier.BookIdSpecified);
                }
                //
                if (isAddBook)
                {
                    //Obtention de l'entité
                    // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude sur signature ActorAncestor
                    ActorAncestor ac = new ActorAncestor(pCS, pDbTrancation, party.OTCmlId, null, null, false);
                    int idAEntity = ac.GetFirstRelation(RoleActor.ENTITY);
                    //
                    //Obtention du Book 
                    SQL_Book sqlBook = null;
                    SQL_Entity sqlEntity = new SQL_Entity(pCS, idAEntity, SQL_Table.ScanDataDtEnabledEnum.No)
                    {
                        DbTransaction = pDbTrancation
                    };
                    if ((sqlEntity.IsLoaded) && sqlEntity.IdBookOpp > 0)
                    {
                        sqlBook = new SQL_Book(pCS, sqlEntity.IdBookOpp, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            DbTransaction = pDbTrancation
                        };
                        if (false == sqlBook.IsLoaded)
                            sqlBook = null;
                    }
                    //                        
                    if (null != sqlBook)
                    {
                        if (null == partyTradeIdentifier)
                            partyTradeIdentifier = DataDocument.AddPartyTradeIndentifier(party.Id);
                        //
                        Tools.SetBookId(partyTradeIdentifier.BookId, sqlBook);
                        partyTradeIdentifier.BookIdSpecified = true;
                    }
                }
                //}
            }
            //}
        }

        // EG 20180307 [23769] Gestion dbTransaction
        protected override bool CheckIsTradeFound(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
            bool isTemplate = TradeRDBMSTools.IsTradeTemplate(pCS, pDbTransaction, pId, pIdType);
            if (isTemplate)
                stEnv = Cst.StatusEnvironment.TEMPLATE;

            string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);
            SQL_TradeTransaction sqlTrade = new SQL_TradeTransaction(pCS, pIdType, sqlTradeId,
                                                stEnv, SQL_Table.RestrictEnum.Yes,
                                                pUser, pSessionId, m_SQLTrade.RestrictProduct)
            {
                DbTransaction = pDbTransaction
            };
            return sqlTrade.IsFound;
        }

        /// <summary>
        ///  Retourne true si Spheres® peut peut procéder aux calculs UTI
        ///  <para>Retourne true si ALLOC, si l'entité est REGULATORYOFFICE</para>
        /// </summary>
        /// <param name="pIdARegulatoryOfficeRelativeToEntity">Retoune l'acteur REGULATORYOFFICE vis à vis de l'entité</param>
        /// <returns></returns>
        /// FI 20140605 [20049] désormais l'UTI est calculé si activité maison 
        // EG 20180307 [23769] Gestion dbTransaction
        public Boolean IsCalcUTIAvailable(string pCS, IDbTransaction pDbTransaction, out int pIdARegulatoryOfficeRelativeToEntity)
        {
            pIdARegulatoryOfficeRelativeToEntity = 0;
            Boolean isOk = this.IsTradeFoundAndAllocation;

            if (isOk)
            {
                int idAEntity = DataDocument.GetFirstEntity(CSTools.SetCacheOn(pCS), pDbTransaction);
                isOk = (idAEntity > 0);
                if (isOk)
                {
                    // L'entité doit être RegulatoryOffice => Les UTI/PUTI sont calculés par Spheres® uniquement si l'entité a le rôle RegulatoryOffice
                    pIdARegulatoryOfficeRelativeToEntity = RegulatoryTools.GetActorRegulatoryOffice(CSTools.SetCacheOn(pCS), pDbTransaction, idAEntity);
                    isOk = (pIdARegulatoryOfficeRelativeToEntity > 0);
                }
            }
            // FI 20140605 [20049] désormais l'UTI est calculé si activité maison 
            // => Mise en commentaire du pavé suivant
            // Pas de calcul de l'UTI lorsque le dealer n'est pas un client et que son entité est membre (activé maison)
            //if (isOk)
            //{
            //    ExchangeTradedDerivativeContainer exchangeTradedDerivative = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)DataDocument.currentProduct.productBase);

            //    IFixParty dealer = exchangeTradedDerivative.GetDealer();
            //    IParty dealerParty = null;
            //    if (null != dealer)
            //        dealerParty = DataDocument.GetParty(dealer.PartyId.href);

            //    IFixParty clearer = exchangeTradedDerivative.GetClearer();
            //    IParty clearerParty = null;
            //    if (null != clearer)
            //        clearerParty = DataDocument.GetParty(clearer.PartyId.href);

            //    isOk = (null != dealerParty) && (null != clearerParty);

            //    if (isOk)
            //    {
            //        // Pas de calcul de l'UTI lorsque le dealer n'est pas un client et que son entité est membre (activé maison)
            //        ActorRoleCollection actorRoleCol = DataDocument.GetActorRole(CSTools.SetCacheOn(CS));
            //        Boolean isDealerClient = actorRoleCol.IsActorRole(dealerParty.OTCmlId, RoleActor.CLIENT);
            //        if ((!isDealerClient) && (clearer.PartyRole == PartyRoleEnum.ClearingOrganization))
            //        {
            //            isOk = false;
            //        }
            //    }
            //}

            return isOk;
        }

        /// <summary>
        ///  Retourne true si Spheres® peut peut procéder aux calculs UTI
        ///  <para>Retourne true si ALLOC, si l'entité est REGULATORYOFFICE</para>
        /// </summary>
        /// <returns></returns>
        public Boolean IsCalcUTIAvailable(string pCS, IDbTransaction pDbTransaction)
        {
            return IsCalcUTIAvailable(pCS, pDbTransaction, out _);
        }

        /// <summary>
        /// Calcul de l'UTI Dealer ou Clearer d'un trade ALLOC et alimentation de l'élément tradId.
        /// <para>Cette méthode fait appel à IsCalcUTIAvailable avant de procéder à l'évaluation</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSide"></param>
        /// <param name="pTradeIdentification">Identification de trade sous Spheres (doit être renseigné uniquement si l'identification du trade est connue)</param>
        /// FI 20140218 [19631] add Method
        /// FI 20140307 [19689] add pDbTransaction
        /// FI 20140623 [20125] upd Mise à jour du datadocument uniquement si rule != UTIRule.NONE 
        /// EG 20140526 Replace exchangeTradedDerivative by exchangeTraded
        /// FI 20140919 [XXXXX] Modify (Refatoring pour appel aux méthodes UTITools.CalcUTIDealer et UTITools.CalcUTIClearer)
        /// EG 20180307 [23769] Gestion dbTransaction
        /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajout paramètre UTIRule pour alimentation de la colonne SOURCE dans TRADEID). 
        public void CalcAndSetTradeUTI(String pCS,  IDbTransaction pDbTransaction, TypeSideAllocation pSide, SpheresIdentification pTradeIdentification)
        {
            if (false == this.IsTradeFoundAndAllocation)
                throw new Exception(StrFunc.AppendFormat("Business status: {0} is not ALLOC", this.TradeStatus.stBusiness.NewSt));

            Boolean isOk = IsCalcUTIAvailable(pCS, pDbTransaction);

            if (isOk)
            {
                UTIComponents UTIComponents = UTITools.InitUTIComponentsFromDataDocument(CSTools.SetCacheOn(pCS), pDbTransaction, DataDocument, pTradeIdentification);

                IParty party = null;
                string uti = string.Empty;
                UTIRule rule = default;
                switch (pSide)
                {
                    case TypeSideAllocation.Dealer:
                        if (UTIComponents.Dealer_Actor_id > 0)
                        {
                            party = DataDocument.GetParty(UTIComponents.Dealer_Actor_id.ToString(), PartyInfoEnum.OTCmlId);
                            if (null != party)
                                UTITools.CalcUTIDealer(CSTools.SetCacheOn(pCS), pDbTransaction, UTIComponents, UTIType.UTI, out uti, out rule);
                        }
                        break;
                    case TypeSideAllocation.Clearer:
                        if (UTIComponents.Clearer_Actor_Id > 0)
                        {
                            party = DataDocument.GetParty(UTIComponents.Clearer_Actor_Id.ToString(), PartyInfoEnum.OTCmlId);
                            if (null != party)
                                UTITools.CalcUTIClearer(CSTools.SetCacheOn(pCS), pDbTransaction, UTIComponents, UTIType.UTI, out uti, out rule);
                        }
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Side: {0} not implemented", pSide.ToString()));
                }

                if (null != party)
                {
                    if (rule != UTIRule.NONE)
                        DataDocument.SetUTI(party.Id, uti, rule.ToString());
                }
            }
        }


        /// <summary>
        /// Retourne le PUTI Dealer ou Clearer d'un Trade ALLOC
        /// <para>Cette méthode fait appel à IsCalcUTIAvailable avant de procéder à l'évaluation</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSide"></param>
        /// <param name="pSpheresIdentification">identifiants du trade courant </param>
        /// <param name="posUti"> id non significatif de la position et idT du trade ayant ouvert la position</param>
        /// <param name="opUTI">Retourne le code UTI</param>
        /// <param name="opRule">Retourne la règle retenue par Spheresª</param>
        /// <returns></returns>
        /// FI 20140623 [20125] add parameter opRule
        /// FI 20140919 [XXXXX] Modify (Refatoring pour appel aux méthodes UTITools.CalcUTIDealer et UTITools.CalcUTIClearer)
        /// EG 20180307 [23769] Gestion dbTransaction
        public void CalcPositionUTI(string pCS, IDbTransaction pDbTransaction, TypeSideAllocation pSide, SpheresIdentification pSpheresIdentification, (int idPosUTI, int idTOpening) posUti, out string opUTI, out UTIRule opRule)
        {
            opUTI = string.Empty;
            opRule = default;

            bool isOk = IsCalcUTIAvailable(pCS,  pDbTransaction);
            if (isOk)
            {
                UTIComponents UTIComponents = UTITools.InitUTIComponentsFromDataDocument(CSTools.SetCacheOn(pCS), pDbTransaction, this.DataDocument, pSpheresIdentification);
                // FI 20240627 [WI983]
                UTIComponents.InitPUTIComponents(CSTools.SetCacheOn(pCS), pDbTransaction, posUti.idPosUTI, posUti.idTOpening);

                switch (pSide)
                {
                    case TypeSideAllocation.Dealer:
                        UTITools.CalcUTIDealer(CSTools.SetCacheOn(pCS), pDbTransaction, UTIComponents, UTIType.PUTI, out opUTI, out opRule);
                        break;
                    case TypeSideAllocation.Clearer:
                        UTITools.CalcUTIClearer(CSTools.SetCacheOn(pCS), pDbTransaction, UTIComponents, UTIType.PUTI, out opUTI, out opRule);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Side: {0} not implemented", pSide.ToString()));
                }
            }
        }

        /// <summary>
        /// Les tradeId spécifiques aux UTI sont mis à string.Empty
        /// </summary>
        /// FI 20140206 [19564] add Method 
        public void ClearUTI()
        {
            IPartyTradeIdentifier[] partyTradeIdentifier = this.DataDocument.PartyTradeIdentifier;
            for (int i = 0; i < ArrFunc.Count(partyTradeIdentifier); i++)
            {
                if (partyTradeIdentifier[i].TradeIdSpecified)
                {
                    for (int j = 0; j < ArrFunc.Count(partyTradeIdentifier[i].TradeId); j++)
                    {
                        if (partyTradeIdentifier[i].TradeId[j].Scheme == Cst.OTCml_TradeIdUTISpheresScheme)
                            partyTradeIdentifier[i].TradeId[j].Value = string.Empty;
                    }
                }
            }
        }


        /// <summary>
        ///  Retourne les parties assujetties à déclation MiFIR
        /// <para>Retourne un énumérateur vide si aucune partie </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        /// EG 20171031 [23509] Upd
        public IEnumerable<IParty> GetPartyMiFIR(string pCS)
        {
            DataDocumentContainer doc = DataDocument;

            IEnumerable<IParty> ret = new IParty[] { };
            if (IsAllocation)
            {
                //Si alloc => considération uniquement de dealer
                RptSideProductContainer rptSide = doc.CurrentProduct.RptSide(pCS, IsAllocation);
                if (null == rptSide)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented", doc.CurrentProduct.ProductBase.ProductName));

                IFixParty fixParty = rptSide.GetDealer();
                if (null != fixParty)
                {
                    IParty party = doc.GetParty(fixParty.PartyId.href);
                    if (null != party)
                        ret = new IParty[] { party };
                }
                //if (null == fixParty)
                //    throw new NullReferenceException("Dealer not found");

                //IParty party = doc.GetParty(fixParty.PartyId.href);
                //if (null == party)
                //    throw new NotImplementedException(StrFunc.AppendFormat("party :(Id:{0}) not found", fixParty.PartyId.href));

                //ret = new IParty[] { party };
            }
            else
            {
                //Si exécution => Considération des contreparties
                ret = DataDocument.Party.Where(x => DataDocument.IsPartyCounterParty(x)).ToArray();
            }

            // Ne sont pas considérer les contrepaties Externes
            ActorRoleCollection colRole = doc.GetActorRole(pCS);
            ret = from item in
                      ret.Where(x => colRole.IsActorRole(x.OTCmlId, RoleActor.CLIENT) || colRole.IsActorRole(x.OTCmlId, RoleActor.ENTITY))
                  select item;

            return ret;
        }

        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public class TradeInputGUI : TradeCommonInputGUI
    {
        #region Accessors
        /// <summary>
        ///  Retourne l'identifiant du regroupement des produits 
        /// </summary>
        public override Cst.SQLCookieGrpElement GrpElement
        {
            get
            {
                return Cst.SQLCookieGrpElement.SelProduct;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSqlAlias"></param>
        /// <returns></returns>
        public override string GetSQLRestrictProduct(string pSqlAlias)
        {
            return pSqlAlias + ".GPRODUCT" + StrFunc.AppendFormat(" not in ({0},{1},{2})",
                DataHelper.SQLString(Cst.ProductGProduct_ADM),
                DataHelper.SQLString(Cst.ProductGProduct_ASSET),
                DataHelper.SQLString(Cst.ProductGProduct_RISK));

        }

        #endregion Accessors

        #region Constructors
        public TradeInputGUI(string pIdMenu, User pUser, string pXMLFilePath)
            : base(pIdMenu, pUser, pXMLFilePath)
        {
        }
        #endregion Constructors
    }
}