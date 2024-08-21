#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Security.Shared;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Linq;
using System.Reflection;
#endregion using directives

namespace EfsML.v30.Security
{
    #region BuyAndSellBack
    public partial class BuyAndSellBack : IBuyAndSellBack
    {
        #region Accessors
        #region Stream
        public CashStream[] Stream
        {
            get { return this.cashStream;}
        }
        #endregion Stream
        #endregion Accessors
        #region Constructor
        public BuyAndSellBack()
        {
        }
        #endregion Constructor

        #region ISaleAndRepurchaseAgreement Members
        RepoDurationEnum ISaleAndRepurchaseAgreement.Duration
        {
            set { this.duration = value; }
            get { return this.duration; }
        }
        bool ISaleAndRepurchaseAgreement.NoticePeriodSpecified
        {
            set { this.noticePeriodSpecified = value; }
            get { return this.noticePeriodSpecified; }
        }
        IAdjustableOffset ISaleAndRepurchaseAgreement.NoticePeriod
        {
            set { this.noticePeriod = (AdjustableOffset)value; }
            get { return this.noticePeriod; }
        }
        ICashStream[] ISaleAndRepurchaseAgreement.CashStream
        {
            set { this.cashStream = (CashStream[])value; }
            get { return this.cashStream; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.SpotLeg
        {
            set { this.spotLeg = (SecurityLeg[])value; }
            get { return this.spotLeg; }
        }
        bool ISaleAndRepurchaseAgreement.ForwardLegSpecified
        {
            set { this.forwardLegSpecified = value; }
            get { return this.forwardLegSpecified; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.ForwardLeg
        {
            set { this.forwardLeg = (SecurityLeg[])value; }
            get { return this.forwardLeg; }
        }
        EFS_SaleAndRepurchaseAgreement ISaleAndRepurchaseAgreement.Efs_SaleAndRepurchaseAgreement
        {
            set { this.efs_SaleAndRepurchaseAgreement = value; }
            get { return this.efs_SaleAndRepurchaseAgreement; }
        }
        EFS_EventDate ISaleAndRepurchaseAgreement.MaxTerminationDate { get { return MaxTerminationDate; } }
        #endregion ISaleAndRepurchaseAgreement Members
    }
    #endregion BuyAndSellBack

    #region DebtSecurityTransaction
    // EG 20171031 [23509] Upd
    public partial class DebtSecurityTransaction : IProduct, IProductBase, IDebtSecurityTransaction
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_DebtSecurityTransactionAmounts efs_DebtSecurityTransactionAmounts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_DebtSecurityTransactionStream efs_DebtSecurityTransactionStream;
        /// <summary>
        /// Représente le titre obtenu à partir de <see cref="itemSecurityAsset"/> ou de <see cref="itemSecurityAssetReference"/>
        /// </summary>
        /// FI 20230615 [XXXX] Add 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SecurityAsset SecurityAssetResolved;

        // Virtuel
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeIdSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ExchangeId exchangeId;
        #endregion Members
        #region Accessors
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get { return efs_DebtSecurityTransactionAmounts.AdjustedClearingBusinessDate; }
        }
        #endregion AdjustedClearingBusinessDate

        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get 
            {
                return efs_DebtSecurityTransactionAmounts.ClearingBusinessDate; 
            }
        }
        #endregion ClearingBusinessDate
        #region BuyerPartyReference
        public string BuyerPartyReference
        {
            get { return buyerPartyReference.href; }
        }
        #endregion BuyerPartyReference
        #region DebtSecurity
        /// <summary>
        /// Obtient les caractéristiques du titre obtenu à partir de <see cref="itemSecurityAsset"/> ou de <see cref="itemSecurityAssetReference"/>
        /// <para>Attention valeur null possible</para>
        /// </summary>
        /// FI 20230615 [XXXX] use SecurityAssetResolved
        public DebtSecurity DebtSecurity
        {
            get
            {
                if (null == SecurityAssetResolved)
                    throw new NullReferenceException($"{nameof(SecurityAssetResolved)} is null.");

                DebtSecurity ret = null;
                if (SecurityAssetResolved.debtSecuritySpecified)
                    ret = SecurityAssetResolved.debtSecurity;

                return ret;
            }
        }
        #endregion DebtSecurity

        #region IssuerPartyReference
        public string IssuerPartyReference
        {
            get
            {
                string issuerPartyReference = null;
                IReference reference = ((IDebtSecurityTransaction)this).IssuerPartyReference;
                if (null != reference)
                    issuerPartyReference = reference.HRef;
                return issuerPartyReference;
            }
        }
        #endregion IssuerPartyReference
        
        public bool IsDiscount
        {
            get
            {
                bool ret = false;
                if (null != DebtSecurity)
                    ret = DebtSecurity.IsDiscount;
                return ret;
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        public bool IsNotDiscount
        {
            get { return (false == IsDiscount); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date()
                };
                dtEffective.unadjustedDate.DateValue = DateTime.MinValue;
                dtEffective.adjustedDate = new EFS_Date
                {
                    DateValue = DateTime.MinValue
                };
                foreach (EFS_DebtSecurityStream stream in efs_DebtSecurityTransactionStream.debtSecurityStream)
                {
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(stream.EffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = stream.EffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = stream.EffectiveDate.adjustedDate.DateValue;
                    }
                }
                return dtEffective;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (EFS_DebtSecurityStream stream in efs_DebtSecurityTransactionStream.debtSecurityStream)
                {
                    if (0 < stream.TerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = stream.TerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = stream.TerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        
        
        #region SellerPartyReference
        public string SellerPartyReference
        {
            get { return sellerPartyReference.href; }
        }
        #endregion SellerPartyReference
        #region Stream
        /// <summary>
        /// Obtient les caractéristiques Stream du titre obtenu à partir de <see cref="itemSecurityAsset"/> ou de <see cref="itemSecurityAssetReference"/>
        /// <para>Attention valeur null possible</para>
        /// </summary>
        public DebtSecurityStream[] Stream
        {
            get
            {
                if (null != DebtSecurity)
                    return DebtSecurity.Stream;
                else
                    return null;
            }
        }
        #endregion Stream
        #region ValueDate
        public EFS_EventDate ValueDate
        {
            get { return efs_DebtSecurityTransactionAmounts.grossAmount.PaymentDate; }
        }
        #endregion ValueDate
        #endregion Accessors
        #region Constructor
        public DebtSecurityTransaction()
        {
            buyerPartyReference = new PartyOrTradeSideReference();
            sellerPartyReference = new PartyOrTradeSideReference();
            itemSecurityAsset = new SecurityAsset();
            itemSecurityAssetReference = new SecurityAssetReference();
            quantity = new OrderQuantity();
            price = new OrderPrice();
            grossAmount = new Payment();
        }
        #endregion Constructor
        #region IDebtSecurityTransaction Members

        // EG 20151130 New
        // EG 20171031 [23509] Upd
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190730 New parameter pStatus for (new EFS_DebtSecurityTransactionAmounts)
        void IDebtSecurityTransaction.SetStreams(string pCS, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatus)
        {
            efs_DebtSecurityTransactionAmounts = new EFS_DebtSecurityTransactionAmounts(pCS, this, pDataDocument, pStatus)
            {
                StatusBusiness = pStatus
            };
            efs_DebtSecurityTransactionStream = new EFS_DebtSecurityTransactionStream(pCS, this, pDataDocument, pStatus);
        }

        /// <summary>
        /// Recherche de l'asset security dans <paramref name="dataDocument"/> lorsque nécessaire afin d'utiliser ses propriétés (exemple <seealso cref="SecurityAssetOTCmlId"/>).
        /// </summary>
        /// <param name="dataDocument"></param>
        /// FI 20230615 [XXXX] Add ResolveSecurityAsset
        void IDebtSecurityTransaction.ResolveSecurityAsset(DataDocumentContainer dataDocument)
        {
            SecurityAssetResolved = null;

            if (itemSecurityAssetSpecified)
                SecurityAssetResolved = itemSecurityAsset;
            else if (itemSecurityAssetReferenceSpecified)
            {
                object obj = ReflectionTools.GetObjectById(dataDocument.DataDocument.Item, itemSecurityAssetReference.href);
                if (null != obj)
                    SecurityAssetResolved = (SecurityAsset)obj;
            }
        }
        

        IReference IDebtSecurityTransaction.BuyerPartyReference
        {
            set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.buyerPartyReference; }
        }
        IReference IDebtSecurityTransaction.SellerPartyReference
        {
            set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.sellerPartyReference; }
        }
        IReference IDebtSecurityTransaction.IssuerPartyReference
        {
            get 
            {
                IReference reference = null;
                DebtSecurity debtSecurity = DebtSecurity;
                if ((null != debtSecurity) && ArrFunc.IsFilled(debtSecurity.Stream))
                    reference = DebtSecurity.Stream[0].payerPartyReference;
                return reference;
            }
        }
        bool IDebtSecurityTransaction.SecurityAssetSpecified
        {
            set { this.itemSecurityAssetSpecified = value; }
            get { return this.itemSecurityAssetSpecified; }
        }
        ISecurityAsset IDebtSecurityTransaction.SecurityAsset
        {
            set { this.itemSecurityAsset = (SecurityAsset)value; }
            get { return this.itemSecurityAsset; }
        }
        bool IDebtSecurityTransaction.SecurityAssetReferenceSpecified
        {
            set { this.itemSecurityAssetReferenceSpecified = value; }
            get { return this.itemSecurityAssetReferenceSpecified; }
        }
        IReference IDebtSecurityTransaction.SecurityAssetReference
        {
            set { this.itemSecurityAssetReference = (SecurityAssetReference)value; }
            get { return this.itemSecurityAssetReference; }
        }
        IOrderQuantity IDebtSecurityTransaction.Quantity
        {
            set { this.quantity = (OrderQuantity)value; }
            get { return this.quantity; }
        }

        IOrderPrice IDebtSecurityTransaction.Price
        {
            set { this.price = (OrderPrice)value; }
            get { return this.price; }
        }

        IPayment IDebtSecurityTransaction.GrossAmount
        {
            set { this.grossAmount = (Payment)value; }
            get { return this.grossAmount; }
        }
        IDebtSecurity IDebtSecurityTransaction.DebtSecurity
        {
            get { return DebtSecurity; }
        }
        EFS_DebtSecurityTransactionAmounts IDebtSecurityTransaction.Efs_DebtSecurityTransactionAmounts
        {
            set { efs_DebtSecurityTransactionAmounts = value; }
            get { return efs_DebtSecurityTransactionAmounts; }
        }
        EFS_DebtSecurityTransactionStream IDebtSecurityTransaction.Efs_DebtSecurityTransactionStream
        {
            set { efs_DebtSecurityTransactionStream = value; }
            get { return efs_DebtSecurityTransactionStream; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI [XXXXX] 20230616 use SecurityAssetResolved
        int IDebtSecurityTransaction.SecurityAssetOTCmlId 
        {
            get
            {
                if (null == SecurityAssetResolved)
                    throw new NullReferenceException($"{nameof(SecurityAssetResolved)} is null.");

                return SecurityAssetResolved.OTCmlId;
            }

        }
        EFS_EventDate IDebtSecurityTransaction.MaxTerminationDate {get { return MaxTerminationDate; }}
        
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] IDebtSecurityTransaction.RptSide
        {
            set { this.RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return this.RptSide; }
        }
        EFS_Asset IDebtSecurityTransaction.Efs_Asset(string pCS)
        {
            EFS_Asset _efs_Asset = null;
            int _id = ((IDebtSecurityTransaction) this).SecurityAssetOTCmlId;
            if (0 < _id)
            {
                SQL_AssetDebtSecurity sql_AssetDebtSecurity = new SQL_AssetDebtSecurity(CSTools.SetCacheOn(pCS), _id);
                if (sql_AssetDebtSecurity.IsLoaded)
                {
                    _efs_Asset = new EFS_Asset
                    {
                        idAsset = sql_AssetDebtSecurity.Id,
                        description = sql_AssetDebtSecurity.Description,
                        IdMarket = sql_AssetDebtSecurity.IdM,
                        IdMarketFIXML_SecurityExchange = sql_AssetDebtSecurity.Market_FIXML_SecurityExchange,
                        IdMarketIdentifier = sql_AssetDebtSecurity.Market_Identifier,
                        IdMarketISO10383_ALPHA4 = sql_AssetDebtSecurity.Market_ISO10383_ALPHA4,
                        assetCategory = sql_AssetDebtSecurity.AssetCategory.Value
                    };

                    string schemeId = EnumTools.GenerateScheme(pCS, Cst.OTCmL_SecurityIdSourceScheme, "SecurityIDSourceEnum", "ISIN", true);
                    if ((null != DebtSecurity) &&  (null != DebtSecurity.security) && (null != DebtSecurity.security.instrumentId))
                    {
                        IScheme  scheme = Tools.GetScheme((IScheme[])DebtSecurity.security.instrumentId, schemeId);
                        if (null != scheme)
                            _efs_Asset.isinCode = scheme.Value;
                    }
                }
            }
            return _efs_Asset;
        }

        // EG 20150624 [21151] New 
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuantity"></param>
        void IDebtSecurityTransaction.InitPositionTransfer(decimal pQuantity)
        {
            if (null == DebtSecurity)
                throw new NullReferenceException($"{nameof(DebtSecurity)} is null");
            
            if (DebtSecurity.security.numberOfIssuedSecuritiesSpecified)
            {
                DebtSecurity.security.numberOfIssuedSecuritiesSpecified = true;
                DebtSecurity.security.numberOfIssuedSecurities = new EFS_Decimal(pQuantity);
            }
        }

        bool IDebtSecurityTransaction.ExchangeIdSpecified
        {
            set { this.exchangeIdSpecified = value; }
            get { return this.exchangeIdSpecified; }
        }
        ISpheresIdScheme IDebtSecurityTransaction.ExchangeId
        {
            set { this.exchangeId = (ExchangeId)value; }
            get { return this.exchangeId; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDebtSecurityTransactionContainer"></param>
        /// <param name="pPreviousCouponDate"></param>
        /// <returns></returns>
        /// EG 20190823 [FIXEDINCOME] New Alimentation des dates pour une nouvelle période d'intérêts (Cas DebtSecurity perpetual)
        ICalculationPeriodDates IDebtSecurityTransaction.CalcNextInterestPeriodDates(string pCS, DebtSecurityTransactionContainer pDebtSecurityTransactionContainer, DateTime pPreviousCouponDate)
        {
            ICalculationPeriodDates calculationPeriodDates = null;
            if (null == DebtSecurity)
                throw new NullReferenceException($"{nameof(DebtSecurity)} is null");

            IDebtSecurityStream stream = DebtSecurity.Stream.FirstOrDefault();
            if (stream != default(IDebtSecurityStream))
            {
                calculationPeriodDates = stream.CalcNextInterestPeriodDates(pCS, pDebtSecurityTransactionContainer, pPreviousCouponDate);
                if (null != calculationPeriodDates)
                {
                    if (DebtSecurity.security.calculationRulesSpecified &&
                        DebtSecurity.security.calculationRules.fullCouponCalculationRulesSpecified)
                    {
                        FullCouponCalculationRules fullCouponCalculationRules = DebtSecurity.security.calculationRules.fullCouponCalculationRules;
                        if (fullCouponCalculationRules.recordDateSpecified)
                            stream.SetRecordAndExDates(pCS, 
                                pDebtSecurityTransactionContainer.DataDocument, DividendDateReferenceEnum.RecordDate, fullCouponCalculationRules.recordDate);
                        if (fullCouponCalculationRules.exDateSpecified)
                            stream.SetRecordAndExDates(pCS, 
                                pDebtSecurityTransactionContainer.DataDocument, DividendDateReferenceEnum.ExDate, fullCouponCalculationRules.exDate);
                    }
                }
            }
            return calculationPeriodDates;
        }
        #endregion IDebtSecurityTransaction Members
        #region ITradeTypeReport Members
        // EG 20190730 New interface
        bool ITradeTypeReport.TrdTypeSpecified
        {
            set { this.trdTypeSpecified = value; }
            get { return this.trdTypeSpecified; }
        }
        TrdTypeEnum ITradeTypeReport.TrdType
        {
            set { this.trdType = (TrdTypeEnum)value; }
            get { return this.trdType; }
        }

        bool ITradeTypeReport.TrdSubTypeSpecified
        {
            set { this.trdSubTypeSpecified = value; }
            get { return this.trdSubTypeSpecified; }
        }
        TrdSubTypeEnum ITradeTypeReport.TrdSubType
        {
            set { this.trdSubType = (TrdSubTypeEnum)value; }
            get { return this.trdSubType; }
        }

        bool ITradeTypeReport.SecondaryTrdTypeSpecified
        {
            set { this.secondaryTrdTypeSpecified = value; }
            get { return this.secondaryTrdTypeSpecified; }
        }
        SecondaryTrdTypeEnum ITradeTypeReport.SecondaryTrdType
        {
            set { this.secondaryTrdType = (SecondaryTrdTypeEnum)value; }
            get { return this.secondaryTrdType; }
        }
        #endregion ITradeTypeReport Members
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
    }
    #endregion DebtSecurityTransaction

    #region Repo
    public partial class Repo : IRepo
    {
        #region Accessors
        #region Stream
        public CashStream[] Stream
        {
            get { return this.cashStream; }
        }
        #endregion Stream
        #endregion Accessors

        #region Constructor
        public Repo()
        {

        }
        #endregion Constructor
        #region ISaleAndRepurchaseAgreement Members
        RepoDurationEnum ISaleAndRepurchaseAgreement.Duration
        {
            set { this.duration = value; }
            get { return this.duration; }
        }
        bool ISaleAndRepurchaseAgreement.NoticePeriodSpecified
        {
            set { this.noticePeriodSpecified = value; }
            get { return this.noticePeriodSpecified; }
        }
        IAdjustableOffset ISaleAndRepurchaseAgreement.NoticePeriod
        {
            set { this.noticePeriod = (AdjustableOffset)value; }
            get { return this.noticePeriod; }
        }
        ICashStream[] ISaleAndRepurchaseAgreement.CashStream
        {
            set { this.cashStream = (CashStream[])value; }
            get { return this.cashStream; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.SpotLeg
        {
            set { this.spotLeg = (SecurityLeg[])value; }
            get { return this.spotLeg; }
        }
        bool ISaleAndRepurchaseAgreement.ForwardLegSpecified
        {
            set { this.forwardLegSpecified = value; }
            get { return this.forwardLegSpecified; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.ForwardLeg
        {
            set { this.forwardLeg = (SecurityLeg[])value; }
            get { return this.forwardLeg; }
        }
        EFS_SaleAndRepurchaseAgreement ISaleAndRepurchaseAgreement.Efs_SaleAndRepurchaseAgreement
        {
            set { this.efs_SaleAndRepurchaseAgreement = value; }
            get { return this.efs_SaleAndRepurchaseAgreement; }
        }
        #endregion ISaleAndRepurchaseAgreement Members
    }
    #endregion Repo

    #region SaleAndRepurchaseAgreement
    public partial class SaleAndRepurchaseAgreement : IProduct, IProductBase, ISaleAndRepurchaseAgreement
    {
        #region Accessors
        #region MinEffectiveDate
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in cashStream)
                {
                    EFS_EventDate streamEffectiveDate = stream.EffectiveDate;
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(streamEffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = streamEffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = streamEffectiveDate.adjustedDate.DateValue;
                    }
                }
                return dtEffective;
            }
        }
        #endregion MinEffectiveDate
        #region MaxTerminationDate
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in cashStream)
                {
                    EFS_EventDate streamTerminationDate = stream.TerminationDate;
                    if (0 < streamTerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = streamTerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = streamTerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        #endregion MaxTerminationDate
        #endregion Accessors
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SaleAndRepurchaseAgreement efs_SaleAndRepurchaseAgreement;
        #endregion Members
        #region Constructor
        public SaleAndRepurchaseAgreement()
        {
            this.cashStream = new CashStream[1] { new CashStream() };
            this.spotLeg = new SecurityLeg[1] { new SecurityLeg() };
            this.forwardLeg = new SecurityLeg[1] { new SecurityLeg() };
        }
        #endregion Constructor
        #region Methods
        #region DebtSecurityStream
        public IInterestRateStream DebtSecurityStream(int pStreamNo)
        {
            int nbCashStream = cashStream.Length;
            int nbSpotLeg = spotLeg.Length;
            int leg = System.Math.DivRem(pStreamNo, 1000, out int remainder);

            // Nous sommes sur un stream = CashStream
            if ((0 == leg) && (remainder <= nbCashStream))
                return (IInterestRateStream)cashStream[remainder - 1];

            if (leg < nbCashStream)
                return (IInterestRateStream)cashStream[leg - 1];
            else
            {
                // Nous sommes sur un stream sur SecurityLeg (SpotLeg || ForwardLeg)
                SecurityLeg securityLeg = null;
                if (leg <= (nbSpotLeg + spotLeg.Length))
                    securityLeg = spotLeg[leg - nbCashStream - 1];
                else if (forwardLegSpecified)
                    securityLeg = forwardLeg[leg - nbSpotLeg - nbCashStream - 1];

                if (null != securityLeg)
                {
                    if (null == securityLeg.debtSecurityTransaction.DebtSecurity)
                        throw new NullReferenceException($"{nameof(securityLeg.debtSecurityTransaction.DebtSecurity)} is null");

                    return (IInterestRateStream)securityLeg.debtSecurityTransaction.DebtSecurity.debtSecurityStream[remainder - 1];
                }
            }
            return null;
        }
        #endregion DebtSecurityStream
        #endregion Methods
        #region ISaleAndRepurchaseAgreement Members
        RepoDurationEnum ISaleAndRepurchaseAgreement.Duration
        {
            set { this.duration = value; }
            get { return this.duration; }
        }
        bool ISaleAndRepurchaseAgreement.NoticePeriodSpecified
        {
            set { this.noticePeriodSpecified = value; }
            get { return this.noticePeriodSpecified; }
        }
        IAdjustableOffset ISaleAndRepurchaseAgreement.NoticePeriod
        {
            set { this.noticePeriod = (AdjustableOffset)value; }
            get { return this.noticePeriod; }
        }
        ICashStream[] ISaleAndRepurchaseAgreement.CashStream
        {
            set { this.cashStream = (CashStream[])value; }
            get { return this.cashStream; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.SpotLeg
        {
            set { this.spotLeg = (SecurityLeg[])value; }
            get { return this.spotLeg; }
        }
        bool ISaleAndRepurchaseAgreement.ForwardLegSpecified
        {
            set { this.forwardLegSpecified = value; }
            get { return this.forwardLegSpecified; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.ForwardLeg
        {
            set { this.forwardLeg = (SecurityLeg[])value; }
            get { return this.forwardLeg; }
        }
        EFS_SaleAndRepurchaseAgreement ISaleAndRepurchaseAgreement.Efs_SaleAndRepurchaseAgreement
        {
            set { this.efs_SaleAndRepurchaseAgreement = value; }
            get { return this.efs_SaleAndRepurchaseAgreement; }
        }
        #endregion ISaleAndRepurchaseAgreement Members
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
    }
    #endregion SaleAndRepurchaseAgreement
    #region SecurityLending
    public partial class SecurityLending : ISecurityLending
    {
        #region Constructor
        public SecurityLending()
        {
        }
        #endregion Constructor
        #region ISaleAndRepurchaseAgreement Members
        RepoDurationEnum ISaleAndRepurchaseAgreement.Duration
        {
            set { this.duration = value; }
            get { return this.duration; }
        }
        bool ISaleAndRepurchaseAgreement.NoticePeriodSpecified
        {
            set { this.noticePeriodSpecified = value; }
            get { return this.noticePeriodSpecified; }
        }
        IAdjustableOffset ISaleAndRepurchaseAgreement.NoticePeriod
        {
            set { this.noticePeriod = (AdjustableOffset)value; }
            get { return this.noticePeriod; }
        }
        ICashStream[] ISaleAndRepurchaseAgreement.CashStream
        {
            set { this.cashStream = (CashStream[])value; }
            get { return this.cashStream; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.SpotLeg
        {
            set { this.spotLeg = (SecurityLeg[])value; }
            get { return this.spotLeg; }
        }
        bool ISaleAndRepurchaseAgreement.ForwardLegSpecified
        {
            set { this.forwardLegSpecified = value; }
            get { return this.forwardLegSpecified; }
        }
        ISecurityLeg[] ISaleAndRepurchaseAgreement.ForwardLeg
        {
            set { this.forwardLeg = (SecurityLeg[])value; }
            get { return this.forwardLeg; }
        }
        #endregion ISaleAndRepurchaseAgreement Members
    }
    #endregion SecurityLending
    #region SecurityLeg
    public partial class SecurityLeg : ISecurityLeg, IEFS_Array
    {
        #region Constructor
        public SecurityLeg()
        {
            debtSecurityTransaction = new DebtSecurityTransaction();
            spotLegReference = new SecurityLegReference();
            margin = new Margin();
        }
        #endregion Constructor
        #region ISecurityLeg Members
        bool ISecurityLeg.SpotLegReferenceSpecified
        {
            set { this.spotLegReferenceSpecified = value; }
            get { return this.spotLegReferenceSpecified; }
        }
        IReference ISecurityLeg.SpotLegReference
        {
            set { this.spotLegReference = (SecurityLegReference)value; }
            get { return this.spotLegReference; }
        }
        IDebtSecurityTransaction ISecurityLeg.DebtSecurityTransaction
        {
            set { this.debtSecurityTransaction = (DebtSecurityTransaction)value; }
            get { return this.debtSecurityTransaction; }
        }
        bool ISecurityLeg.MarginSpecified
        {
            set { this.marginSpecified = value; }
            get { return this.marginSpecified; }
        }
        IMargin ISecurityLeg.Margin
        {
            set { this.margin = (Margin)value; }
            get { return this.margin; }
        }
        String ISecurityLeg.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion ISecurityLeg Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion SecurityLeg
}
