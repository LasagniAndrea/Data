#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_ExchangeTradedDerivative
    // EG 20140120 Report 3.7
    public class EFS_ExchangeTradedDerivative
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        private readonly Nullable<int> m_IdT;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public DateTime tradeDate;
        public DateTime clearingBusinessDate;
        public DateTime maturityDate;
        // EG 20140120 Report 3.7
        public DateTime maturityDateSys;
        public CfiCodeCategoryEnum category;
        public bool isOption;
        public DerivativeExerciseStyleEnum exerciseStyle;
        public PutOrCallEnum putOrCall;
        public bool premiumSpecified;
        public EFS_ETDPremium premium;
        public IMoney nominal;
        public decimal quantity;
        public bool exerciseDatesSpecified;
        public EFS_FxExerciseDates exerciseDates;
        public bool isPositionOpening;
        public bool isTradeCAAdjusted;
        public bool isPositionKeeping;

        private EFS_Asset m_AssetETD;
        #endregion Members
        //
        #region Accessors
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = clearingBusinessDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedClearingBusinessDate
        #region AdjustedMaturityDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedMaturityDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = maturityDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedMaturityDate
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get { return nominal.Amount; }
        }
        #endregion Amount
        #region AssetETD
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset AssetETD
        {
            get { return m_AssetETD; }
        }
        #endregion AssetETD
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return new EFS_EventDate(clearingBusinessDate.Date, clearingBusinessDate.Date); }
        }
        #endregion ClearingBusinessDate
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get { return nominal.Currency; }
        }
        #endregion Currency
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BuyerPartyReference
        {
            get
            {
                return buyerPartyReference.HRef;
            }
        }
        #endregion BuyerPartyReference
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region EventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string EventCode
        {
            get
            {
                string eventCode = EventCodeFunc.FutureExchangeTradedDerivative;
                if (CfiCodeCategoryEnum.Option == category)
                {
                    if (DerivativeExerciseStyleEnum.American == exerciseStyle)
                        eventCode = EventCodeFunc.AmericanExchangeTradedDerivativeOption;
                    else if (DerivativeExerciseStyleEnum.European == exerciseStyle)
                        eventCode = EventCodeFunc.EuropeanExchangeTradedDerivativeOption;
                }
                return eventCode;
            }
        }
        #endregion EventCode
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string EventType
        {
            get
            {
                string eventType = EventTypeFunc.Future;
                if (CfiCodeCategoryEnum.Option == category)
                {
                    if (PutOrCallEnum.Call == putOrCall)
                        eventType = EventTypeFunc.Call;
                    else if (PutOrCallEnum.Put == putOrCall)
                        eventType = EventTypeFunc.Put;
                }
                return eventType;
            }
        }
        #endregion EventType
        #region IsNotPositionOpeningAndNotPositionKeeping
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20130610 Add Test isTradeCAAdjusted
        public bool IsNotPositionOpeningAndIsPositionKeeping
        {
            get
            {
                return (false == isPositionOpening) && (false == isTradeCAAdjusted) && isPositionKeeping;
            }
        }
        #endregion IsNotPositionOpening

        #region MaturityDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate MaturityDate
        {
            get { return new EFS_EventDate(maturityDate.Date, maturityDate.Date); }
        }
        #endregion MaturityDate
        #region Quantity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Quantity
        {
            get { return new EFS_Decimal(quantity); }
        }
        #endregion Quantity
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SellerPartyReference
        {
            get
            {
                return sellerPartyReference.HRef;
            }
        }
        #endregion SellerPartyReference
        #endregion Accessors
        //
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExchangeTradedDerivative(string pConnectionString, DataDocumentContainer pDataDocument, Nullable<int> pIdT)
        {
            m_IdT = pIdT;
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }

        // EG 2015124 [POC-MUREX] Add pDbTransaction
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExchangeTradedDerivative(string pConnectionString, IExchangeTradedDerivative pExchangeTradedDerivative, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness, Nullable<int> pIdT)
            : this(pConnectionString, null, pExchangeTradedDerivative, pDataDocument, pStatusBusiness, pIdT)
        {
        }
        // EG 2015124 [POC-MUREX] New
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExchangeTradedDerivative(string pConnectionString, IDbTransaction pDbTransaction, IExchangeTradedDerivative pExchangeTradedDerivative, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness, Nullable<int> pIdT)
            : this(pConnectionString, pDataDocument, pIdT)
        {
            Calc(pDbTransaction, pExchangeTradedDerivative, pStatusBusiness);
        }
        #endregion Constructors
        //
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeTradedDerivative"></param>
        /// <param name="pDataDocument"></param>
        // EG 20140904 Add AssetCategory
        // EG 2015124 [POC-MUREX] Add pDbTransaction
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        private void Calc(IDbTransaction pDbTransaction, IExchangeTradedDerivative pExchangeTradedDerivative, Cst.StatusBusiness pStatusBusiness)
        {
            // EG 2015124 [POC-MUREX] Add pDbTransaction
            ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer = new ExchangeTradedDerivativeContainer(m_Cs, pDbTransaction, pExchangeTradedDerivative, m_DataDocument);
            // Buyer / Seller
            IFixParty buyer = exchangeTradedDerivativeContainer.GetBuyer();
            IFixParty seller = exchangeTradedDerivativeContainer.GetSeller();
            if (null == buyer)
                throw new NotSupportedException("buyer is not Found");
            if (null == seller)
                throw new NotSupportedException("seller is not Found");
            //
            isPositionOpening = exchangeTradedDerivativeContainer.IsPositionOpening;
            isTradeCAAdjusted = exchangeTradedDerivativeContainer.IsTradeCAAdjusted;

            isPositionKeeping = false;
            if (pStatusBusiness == Cst.StatusBusiness.ALLOC)
                isPositionKeeping = exchangeTradedDerivativeContainer.IsPosKeepingOnBookDealer(m_Cs, pDbTransaction);
            //
            buyerPartyReference = buyer.PartyId;
            sellerPartyReference = seller.PartyId;
            //
            category = exchangeTradedDerivativeContainer.Category.Value;
            isOption = exchangeTradedDerivativeContainer.IsOption;
            // Trade, Business and Maturity Dates
            tradeDate = exchangeTradedDerivativeContainer.TradeDate;
            clearingBusinessDate = exchangeTradedDerivativeContainer.ClearingBusinessDate;
            //
            maturityDate = exchangeTradedDerivativeContainer.MaturityDate;
            // EG 20140121 Homologuation
            maturityDateSys = exchangeTradedDerivativeContainer.MaturityDateSys;
            // RD 20100524 [17004] / Use of MaxDate (only Date)
            if (DtFunc.IsDateTimeEmpty(maturityDate))
            {
                maturityDate = DateTime.MaxValue.Date;
            }
            else
            {
                // PM 20130822 [18404] Décalage de 0 jour business preceding pour que la date d'échéance soit une Business Date
                IBusinessDayAdjustments maturityBDA = m_DataDocument.CurrentProduct.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, exchangeTradedDerivativeContainer.AssetETD.Market_IDBC);
                // PM 20141027 [20419] DayTypeEnum.Business => DayTypeEnum.ExchangeBusiness
                IOffset maturityOffset = m_DataDocument.CurrentProduct.ProductBase.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.ExchangeBusiness);
                maturityDate = Tools.ApplyOffset(m_Cs, maturityDate, maturityOffset, maturityBDA, m_DataDocument);
            }
            //
            nominal = exchangeTradedDerivativeContainer.Nominal;
            quantity = exchangeTradedDerivativeContainer.Qty;
            //
            #region Option
            if (isOption)
            {
                #region ExerciseStyle
                exerciseStyle = exchangeTradedDerivativeContainer.ExerciseStyle;
                #endregion ExerciseStyle
                #region PutOrCall
                putOrCall = exchangeTradedDerivativeContainer.PutOrCall;
                #endregion PutOrCall
                #region Premium
                // EG 20130723 (18754) Prime toujours présente sur option (avec IsPayment à false pour REC/STL dans le cas de FuturesStyleMarkToMarket
                premiumSpecified = exchangeTradedDerivativeContainer.IsPremiumStyle || exchangeTradedDerivativeContainer.IsFuturesStyleMarkToMarket;
                if (premiumSpecified)
                    premium = new EFS_ETDPremium(m_Cs, pDbTransaction,  exchangeTradedDerivativeContainer, m_IdT);
                #endregion Premium
                #region ExerciseDates
                exerciseDatesSpecified = true;
                SettlMethodEnum settlMethod = exchangeTradedDerivativeContainer.SettlementMethod;
                if (DerivativeExerciseStyleEnum.American == exerciseStyle)
                {
                    exerciseDates = new EFS_FxExerciseDates(EventTypeFunc.American, clearingBusinessDate, maturityDate, settlMethod);
                }
                else if (DerivativeExerciseStyleEnum.European == exerciseStyle)
                {
                    exerciseDates = new EFS_FxExerciseDates(EventTypeFunc.European, maturityDate, maturityDate, settlMethod);
                }
                else if (DerivativeExerciseStyleEnum.Bermuda == exerciseStyle)
                {
                    exerciseDatesSpecified = false;
                }
                #endregion ExerciseDates
            }
            #endregion Option
            //
            #region Asset
            m_AssetETD = new EFS_Asset
            {
                //FI 20110817 [17548] m_AssetETD.idC est alimenté avec AssetETD.IdC
                idC = exchangeTradedDerivativeContainer.AssetETD.IdC,
                IdMarket = exchangeTradedDerivativeContainer.IdMarket,
                idAsset = exchangeTradedDerivativeContainer.AssetETD.Id,
                assetCategory = exchangeTradedDerivativeContainer.AssetETD.AssetCategory,
                assetSymbol = exchangeTradedDerivativeContainer.AssetETD.AssetSymbol,
                isinCode = exchangeTradedDerivativeContainer.AssetETD.ISINCode,
                contractIdentifier = exchangeTradedDerivativeContainer.AssetETD.Identifier,
                contractDisplayName = exchangeTradedDerivativeContainer.AssetETD.DisplayName,
                contractSymbol = exchangeTradedDerivativeContainer.AssetETD.DrvContract_Symbol,
                category = category,
                putOrCall = putOrCall,
                contractMultiplier = new EFS_Decimal(exchangeTradedDerivativeContainer.AssetETD.ContractMultiplier),
                // RD 20100524 [17004] / Use of MaxDate 
                maturityDate = maturityDate,
                // EG 20140120 Report 3.7
                maturityDateSys = maturityDateSys,
                // RD 20100524 / Ajout du membre lastTradingDay
                lastTradingDay = exchangeTradedDerivativeContainer.AssetETD.Maturity_LastTradingDay,
                deliveryDate = exchangeTradedDerivativeContainer.AssetETD.Maturity_DeliveryDate,
                nominalValue = exchangeTradedDerivativeContainer.AssetETD.DrvContract_NominalValue
            };
            if (isOption)
                m_AssetETD.strikePrice = new EFS_Decimal(exchangeTradedDerivativeContainer.AssetETD.StrikePrice);
            #endregion Asset
            //
            m_ErrLevel = Cst.ErrLevel.SUCCESS;

        }

        #endregion Methods
    }
    #endregion EFS_ExchangeTradedDerivative

    #region EFS_ETDPremium
    public class EFS_ETDPremium
    {
        #region Members
        private readonly string m_Cs;
        private readonly Nullable<int> m_IdT;
        public IMoney premiumAmount;
        public EFS_ETDPremiumCalculation premiumCalculation;
        // EG 20210812 [25173] Calcul de la prime sur Trade avec TrdTyp = Merge ou VolumeWeightedAverageTrade
        // Contient les montants théorique et le cash résiduel
        public EFS_ETDPremiumAdditionalAmount premiumAdditionalAmount;
        /// <summary>
        /// Représente la date de règlement (==> Date de compensation + 1JO) 
        /// </summary>
        public DateTime paymentDate;
        /// <summary>
        /// Représente la date obtenue après application du délai d'usance à la date de règlement
        /// <para>Elle est équivalente à paymentDate si l'application du delai d'usance n'est pas nécessaire</para> 
        /// </summary>
        public DateTime paymentDateDelay;
        /// <summary>
        ///  
        /// </summary>
        public bool usanceDelaySettlementSpecified;
        public EFS_UsanceDelaySettlement usanceDelaySettlement;
        public bool isPositionOpening;
        public bool isTradeCAAdjusted;
        // EG 20190613 [24683] New
        public bool isPositionClosingReopening;
        // EG 20130723 (18754)
        public FuturesValuationMethodEnum valuationMethod;
        #endregion Members
        //
        #region Accessors
        #region AdjustedPaymentDate
        /// <summary>
        /// Obtient paymentDateDelay
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = paymentDate
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedPaymentDate
        //
        #region EventType
        /// <summary>
        /// Obtient la valeur EVENTTYPE en fonction du type de trade (TRADE de marché = PRM, Insertion de Position = TradeOpening = HPR)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        //EG 20130610 Add isTradeCAAdjusted
        // EG 20190613 Add isPositionClosingReopening
        // RD 20200213 [25192] Remove isPositionClosingReopening
        // EG 202208016 [XXXXX] Modification TRDTYPE de réouverture sur ClosingReopeningPosition (Réouverture = PositionOpening)
        public string EventType
        {
            get
            {
                // RD 20200213 [25192] Supprimer isPositionClosingReopening du test 
                // - pour pouvoir calculer un LPP/PRM à la place d'un LPP/HPR dans le cas dun transfert de position
                // - isPositionClosingReopening concerne aussi le transfert de Position
                // 
                // Remarque:
                // --------
                // - ce changement a été opéré dans le cadre du chantier Fermeture/Ouverture de Position (Ticket 24683)
                // - ce chantier a été arrété, il sera rouvert probablement 
                // - en attendant, l'évolution a été annulée
                //
                //if (isPositionOpening || isTradeCAAdjusted || isPositionClosingReopening)
                if (isPositionOpening || isTradeCAAdjusted)
                    return EfsML.Enum.Tools.EventTypeFunc.HistoricalPremium;
                else
                    return EfsML.Enum.Tools.EventTypeFunc.Premium;
            }
        }
        #endregion EventType
        //
        #region PremiumAmount
        /// <summary>
        /// Obtient le montant du premiumAmount
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PremiumAmount
        {
            get
            {

                return this.premiumAmount.Amount;

            }
        }
        #endregion PremiumAmount
        #region PremiumCurrency
        /// <summary>
        /// Obtient la devise du premiumAmount
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PremiumCurrency
        {
            get
            {
                return this.premiumAmount.Currency;

            }
        }
        #endregion PremiumCurrency
        #region PaymentDate
        /// <summary>
        /// Obtient paymentDate
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return new EFS_EventDate(paymentDate.Date, paymentDate.Date);

            }
        }
        #endregion PaymentDate
        #region PremiumCalculation
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ETDPremiumCalculation PremiumCalculation
        {
            get
            {
                return premiumCalculation;

            }
        }
        #endregion PremiumCalculation
        #region IsPayment
        /// <summary>
        /// IsPayment=false sur STL si FuturesStyleMarkToMark sinon true.
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsPayment
        {
            get
            {
                bool isPayment = true;
                if (valuationMethod == FuturesValuationMethodEnum.FuturesStyleMarkToMarket)
                    isPayment = false;
                return isPayment;
            }
        }
        #endregion IsPayment
        #endregion Accessors
        #region Constructors
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_ETDPremium(string pCs, ExchangeTradedDerivativeContainer pExchangeTradedDerivativeContainer) : 
            this(pCs,null,  pExchangeTradedDerivativeContainer, null) { }
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_ETDPremium(string pCs, IDbTransaction pDbTransaction, ExchangeTradedDerivativeContainer pExchangeTradedDerivativeContainer) :
            this(pCs, pDbTransaction, pExchangeTradedDerivativeContainer, null) { }
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_ETDPremium(string pCs, ExchangeTradedDerivativeContainer pExchangeTradedDerivativeContainer, Nullable<int> pIdT) :
            this(pCs, null, pExchangeTradedDerivativeContainer, pIdT) { }
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_ETDPremium(string pCs, IDbTransaction pDbTransaction, ExchangeTradedDerivativeContainer pExchangeTradedDerivativeContainer, Nullable<int> pIdT)
        {
            m_Cs = pCs;
            m_IdT = pIdT;
            Calc(pDbTransaction, pExchangeTradedDerivativeContainer);
        }
        #endregion Constructors
        #region Methods
        #region RestitutionAmount
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal RestitutionPremiumAmount(decimal pQty)
        {
            return premiumCalculation.price100 * pQty * premiumCalculation.multiplier;
        }
        #endregion RestitutionPremiumAmount
        #region Calc
        /// <revision>
        ///     <version>2.4.0</version><author>PL</author><date>20100223</date>
        ///     <comment>
        ///     Use from captureFees
        ///     </comment>
        /// </revision>
        //PM 20140507 [19970][19259] Utilisation de PriceQuotedCurrency au lieu de PriceCurrency
        //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
        //PM 20140806 [20273][20106] Arrondi de la prime unitaire
        // EG 20150302 [19885][20797] Réécriture de la query pour calcul de la prime
        // EG 20150914 [20797] Réécriture de la query pour calcul de la prime dans le cas de CA multiple
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Test isPositionClosingReopening
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20210812 [25173] Calcul de la prime sur Trade avec TrdTyp = MergeTrade ou VolumeWeightedAverageTrade
        private void Calc(IDbTransaction pDbTransaction, ExchangeTradedDerivativeContainer pExchangeTradedDerivativeContainer)
        {

            #region Premium calculation
            // EG 20130723 (18754)
            valuationMethod = pExchangeTradedDerivativeContainer.ValuationMethod;
            // Price / Qty / ContractMultiplier
            decimal price = pExchangeTradedDerivativeContainer.Price;
            decimal price100 = pExchangeTradedDerivativeContainer.Price100;
            // EG/PL/CC change Decimal to integer
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pExchangeTradedDerivativeContainer.Qty);
            //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
            //decimal multiplier = pExchangeTradedDerivativeContainer.ContractMultiplier;
            decimal multiplier = pExchangeTradedDerivativeContainer.ContractMultiplierQuotedCurrency;
            //PM 20140806 [20273][20106] Calcul de la Prime unitaire arrondie avant de la multiplier par la quantité
            //decimal amount = price100 * qty * multiplier;
            //PM 20150129 [20737][20754] Modification du calcul unitaire
            // FI 20190801 [24821] Application de l'arrondi sur la prime
            //decimal amount = pExchangeTradedDerivativeContainer.CashFlowValorization(price100, 0, multiplier, qty, true);
            // EG 20210812 [25173] Montants de prime
            Pair<decimal, decimal> amount = pExchangeTradedDerivativeContainer.PremiumValorization(price100, multiplier, qty, true);

            IProductBase productBase = ((IProduct)pExchangeTradedDerivativeContainer.ExchangeTradedDerivative).ProductBase;
            isPositionOpening = pExchangeTradedDerivativeContainer.IsPositionOpening;
            isTradeCAAdjusted = pExchangeTradedDerivativeContainer.IsTradeCAAdjusted;
            isPositionClosingReopening = pExchangeTradedDerivativeContainer.IsClosingReopeningPosition;
            if (isTradeCAAdjusted && m_IdT.HasValue)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(m_Cs, "IDTEX", DbType.Int32), m_IdT);
                parameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTBUSINESS), pExchangeTradedDerivativeContainer.ClearingBusinessDate); // FI 20201006 [XXXXX] DbType.Date

                // EG 20150914 [20797] 
                // Les données QTY, MULTIPLIER et PRICE sont lues dans EVENTDET sur la ligne d'événement UMG du jour du trade CUM (EXDATE-1)
                string sqlSelect = @"select evdcum.DAILYQUANTITY as QTY, evdcum.PRICE, evdcum.PRICE100, evdcum.CONTRACTMULTIPLIER
                from dbo.TRADELINK tl 
                /* TRADE : TRADE CUM = IDT_B */
                inner join dbo.TRADE trcum on (trcum.IDT = tl.IDT_B) 
                /* TRADE : TRADE EX = IDT_A */
                inner join dbo.TRADE trex on (trex.IDT = tl.IDT_A) 
                /* ENTITYMARKET : (Date Cum = DTMARKETPREV) */
                inner join dbo.ENTITYMARKET emex on (emex.IDM = trex.IDM) and (emex.IDA = trex.IDA_ENTITY)
                /* EVENT : Unrealized margin du jour (avant CA) = trcum.IDT */
                inner join dbo.EVENT evcum on (evcum.IDT = trcum.IDT) and (evcum.EVENTTYPE = 'UMG') and 
                (evcum.DTSTARTADJ = case when emex.DTMARKET = @DTBUSINESS then emex.DTMARKETPREV else emex.DTMARKET end)
                inner join dbo.EVENTDET evdcum on (evdcum.IDE = evcum.IDE)
                where (tl.IDT_A = @IDTEX) and (tl.LINK = 'PositionAfterCorporateAction')" + Cst.CrLf;
                QueryParameters qryParameters = new QueryParameters(m_Cs, sqlSelect, parameters);

                using (IDataReader dr = DataHelper.ExecuteReader(m_Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (dr.Read())
                    {
                        price = Convert.ToDecimal(dr["PRICE"]);
                        price100 = Convert.ToDecimal(dr["PRICE100"]);
                        multiplier = Convert.ToDecimal(dr["CONTRACTMULTIPLIER"]);
                        // EG 20170127 Qty Long To Decimal
                        qty = Convert.ToDecimal(dr["QTY"]);
                        // FI 20190801 [24821] Application de l'arrondi sur la prime
                        // EG 20210812 [25173] Montants de prime
                        amount.First = pExchangeTradedDerivativeContainer.CashFlowValorization(price100, 0, multiplier, qty, true);
                        amount.Second = amount.First;
                    }
                }
            }
            //PM 20140507 [19970][19259] Utilisation de PriceQuotedCurrency au lieu de PriceCurrency
            //premiumAmount = productBase.CreateMoney(amount, pExchangeTradedDerivativeContainer.PriceCurrency);
            // EG 20210812 [25173] Montants de prime
            premiumAmount = productBase.CreateMoney(amount.Second, pExchangeTradedDerivativeContainer.PriceQuotedCurrency);
            premiumCalculation = new EFS_ETDPremiumCalculation(price, price100, qty, multiplier);
            #endregion Premium calculation
            //
            #region PaymentDate
            // RD 20110429 
            // Création d'une méthode commune GetNextBusinessDate()
            // La date de paiement est égale à la date de compensation + 1JO sur le BC du Market du DC
            DateTime dtDefaultDate = pExchangeTradedDerivativeContainer.GetNextBusinessDate(m_Cs, pDbTransaction);
            if (DtFunc.IsDateTimeFilled(dtDefaultDate))
                paymentDate = dtDefaultDate;
            //
            #endregion PaymentDate

            // EG 20210812 [25173] Montants de prime (Théorique et Cash résiduel)
            if (amount.First != amount.Second)
            {
                premiumAdditionalAmount = new EFS_ETDPremiumAdditionalAmount(productBase, amount, pExchangeTradedDerivativeContainer.PriceQuotedCurrency);
            }

        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_ETDPremium
    #region EFS_ETDPremiumCalculation
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    public class EFS_ETDPremiumCalculation
    {
        #region Members
        public decimal price;
        public decimal price100;
        // EG/PL/CC change Decimal to integer
        // EG 20170127 Qty Long To Decimal
        public decimal qty;
        public decimal multiplier;
        #endregion Members
        #region Constructors
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public EFS_ETDPremiumCalculation(decimal pPrice, decimal pPrice100, decimal pQty, decimal pMultiplier)
        {
            price = pPrice;
            price100 = pPrice100;
            qty = pQty;
            multiplier = pMultiplier;
        }
        #endregion Constructors
    }
    #endregion EFS_ETDPremiumCalculation
    #region EFS_ETDPremiumAdditionalAmount
    /// <summary>
    /// Classe de stockage des montants informatifs de calcul de prime pour des trades 
    /// avec TrdTyp = MergeTrade ou VolumeWeightedAverageTrade
    /// 
    /// - Montant théorique 
    /// - Montant résiduel
    /// 
    /// Ils sont utilisés pour alimenter les événements enfants de la prime lorsque Prime théorique est
    /// différente de la prime réelle.
    /// </summary>
    // EG 20210812 [25173] New
    public class EFS_ETDPremiumAdditionalAmount
    {
        public IMoney theoreticalAmount;
        public IMoney cashResidualAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal TheoreticalAmount
        {
            get {return this.theoreticalAmount.Amount;}
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string TheoreticalAmountCurrency
        {
            get {return this.theoreticalAmount.Currency;}
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal CashResidualAmount
        {
            get { return this.cashResidualAmount.Amount; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CashResidualAmountCurrency
        {
            get { return this.cashResidualAmount.Currency; }
        }

        #region Constructors
        public EFS_ETDPremiumAdditionalAmount(IProductBase pProductBase, Pair<decimal,decimal> pAmount, string pPriceQuotedCurrency)
        {
            cashResidualAmount = pProductBase.CreateMoney(pAmount.Second - pAmount.First, pPriceQuotedCurrency);
            theoreticalAmount = pProductBase.CreateMoney(pAmount.First, pPriceQuotedCurrency);
        }
        #endregion Constructors
    }
    #endregion EFS_ETDPremiumAdditionalAmount

    #region EFS_UsanceDelaySettlement
    /// New Rules to determine the date of Settlement events with Usance Delay
    public class EFS_UsanceDelaySettlement : EFS_AdjustedSettlement, ICloneable
    {
        #region Accessors
        #region AdjustedUsanceDelaySettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedUsanceDelaySettlementDate
        {
            get
            {

                if (DtFunc.IsDateTimeFilled(m_OffsetSettlementDate))
                {
                    string dt = DtFunc.DateTimeToString(m_OffsetSettlementDate, DtFunc.FmtISODate);
                    return new EFS_Date(dt);
                }
                else
                    return null;

            }
        }
        #endregion AdjustedUsanceDelaySettlementDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, EFS_Date pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU2, pOffset, pMethod, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, DateTime pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_UsanceDelaySettlement(string pCS, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
            : base(pCS, null, pSettlementDate, pCU1, pCU2, pOffset, pMethod, pDataDocument) { }
        #endregion Constructors
        #region ICloneable
        public object Clone()
        {
            EFS_UsanceDelaySettlement clone = new EFS_UsanceDelaySettlement(this.m_Cs, this.m_SettlementDate, this.m_CU1, this.m_CU2, this.m_Offset, this.m_Method.ToString(), this.m_DataDocument);
            return clone;
        }
        #endregion ICloneable

    }
    #endregion EFS_UsanceDelaySettlement
}