#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.v30.Doc;
using EfsML.v30.MiFIDII_Extended;
using EfsML.v30.Settlement;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;

#endregion using directives

namespace FpML.v44.Doc
{
    #region DataDocument
    public partial class DataDocument : IDataDocument
    {
        #region Accessors
        #region CurrentTrade
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Trade FirsTrade
        {
            get { return (Trade)trade.GetValue(0); }
        }
        #endregion CurrentTrade
        #region TradeDate
        /// <summary>
        /// Obtient la date de transaction du 1er trade
        /// </summary>
        public DateTime TradeDate
        {
            get { return FirsTrade.tradeHeader.tradeDate.DateValue; }
        }
        #endregion TradeDate
        #endregion accessors
        #region Constructors
        public DataDocument()
        {
            trade = new Trade[1] { new Trade() };
            party = new Party[2] { new Party(), new Party() };
            portfolioSpecified = false; 
            portfolio = new Portfolio[1] { new Portfolio() };
        }
        #endregion Constructors
        #region IDataDocument Members
        // EG 20171016 [23509] Upd
        IParty[] IDataDocument.Party
        {
            set {
                //this.party = (Party[])value; 
                this.party = new Party[] { };
                if (ArrFunc.IsFilled(value))
                    this.party = value.Cast<Party>().ToArray();
            }
            get { return this.party; }
        }
        ITrade[] IDataDocument.Trade
        {
            set { this.trade = (Trade[])value; }
            get { return (ITrade[])this.trade; }
        }
        ITrade IDataDocument.FirstTrade
        {
            set { this.trade[0] = (Trade)value; }
            get { return (ITrade)this.trade[0]; }
        }
        object IDataDocument.Item { get { return this; } }

        
        #endregion IDataDocument Members

        #region IDocument Members
        DocumentVersionEnum IDocument.Version { get { return this.version; } }
        Type IDataDocument.GetTypeParty() { return this.party.GetType().GetElementType(); }
        #endregion IDocument Members
    }
	#endregion DataDocument
	#region Document
    // EG 20140702 Add efs_actualBuild (12)
	public abstract partial class Document : IDocument
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int efs_actualBuild=12;
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("actualBuild", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int ActualBuild
        {
            set { ; }
            get { return efs_actualBuild; }
        }
        #endregion Accessors
        #region Constructors
        public Document()
        {
        }
        #endregion Constructors
        #region IDocument Members
        DocumentVersionEnum IDocument.Version { get { return this.version; } }
		#endregion IDocument Members
	}
	#endregion Document

    #region ExecutionDateTime
    /// EG 20170918 [23342] New FpML extensions for MiFID I (use since MiFID II)
    /// EG 20170926 [22374] Upd Scheme
    public partial class ExecutionDateTime : IScheme
    {
        #region Constructors
        public ExecutionDateTime()
        {
            this._date = new EFS_DateTimeOffset();
            this.executionDateTimeScheme = "http://www.iana.org/time-zones";
        }
        #endregion Constructors


        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }

        string IScheme.Scheme
        {
            set { this.executionDateTimeScheme = value; }
            get { return this.executionDateTimeScheme; }
        }
    }
    #endregion ExecutionDateTime

	#region LinkId
	public partial class LinkId : ItemGUI,IEFS_Array,ILinkId
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ILinkId Members
		string ILinkId.LinkIdScheme
		{
			get { return this.LinkIdScheme; }
			set { this.LinkIdScheme = value; }
		}
		string ILinkId.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
        decimal ILinkId.Factor
        {
            get { return this.factor.DecValue; }
            set { this.factor = new EFS_Decimal(value); }
        }
		string ILinkId.Id
		{
			get { return this.Id; }
			set { this.Id = value; }
		}
        string ILinkId.StrFactor
        {
            get { return this.Factor; }
            set { this.Factor = value; }
        }
        #endregion ILinkId Members
    }
    #endregion LinkId

    #region PartyRole
    public partial class PartyRole : IPartyRole
    {
        #region IPartyRole Membres
        bool IPartyRole.AccountSpecified
        {
            get { return this.partyRoleAccountSpecified; }
            set { this.partyRoleAccountSpecified = value; }
        }
        IReference IPartyRole.Account
        {
            get { return this.partyRoleAccount; }
            set { this.partyRoleAccount = (AccountReference)  value; }
        }

        bool IPartyRole.PartySpecified
        {
            get { return this.partyRolePartySpecified; }
            set { this.partyRolePartySpecified = value; }
        }
        IReference IPartyRole.Party
        {
            get { return this.partyRoleParty; }
            set { this.partyRoleParty = (PartyReference) value; }
        }
        #endregion
    }
    #endregion
    #region PartyTradeIdentifier
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
    public partial class PartyTradeIdentifier : IEFS_Array, IPartyTradeIdentifier
	{
		#region Constructors
		public PartyTradeIdentifier()
		{
			bookId = new BookId();
			localClassDerv = new LocalClassDerv();
			iasClassDerv = new IASClassDerv();
            hedgeClassDerv = new HedgeClassDerv();
            hedgeFolder = new HedgeFolder();
            hedgeFactor = new HedgeFactor();
			fxClass = new FxClass();
			localClassNDrv = new LocalClassNDrv();
			iasClassNDrv = new IASClassNDrv();
            hedgeClassNDrv = new HedgeClassNDrv();
			linkId = new LinkId[1] { new LinkId() };
		}
		#endregion Constructors
		#region Methods
		#region GetLinkIdFromScheme
		public LinkId GetLinkIdFromScheme(string pScheme)
		{
			LinkId ret = null;
			if (ArrFunc.IsFilled(linkId))
			{
				for (int i = 0; i < linkId.Length; i++)
				{
					if (linkId[i].LinkIdScheme == pScheme)
					{
						ret = linkId[i];
						break;
					}
				}
			}
			return ret;
		}
		#endregion GetLinkIdFromScheme
		#region GetLinkIdWithNoScheme
		public LinkId GetLinkIdWithNoScheme()
		{
			LinkId ret = null;
			if (ArrFunc.IsFilled(linkId))
			{
				for (int i = 0; i < linkId.Length; i++)
				{
					if (StrFunc.IsEmpty(linkId[i].LinkIdScheme))
					{
						ret = linkId[i];
						break;
					}
				}
			}
			return ret;
		}
		#endregion GetLinkIdWithNoScheme
		#endregion Methods

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IPartyTradeIdentifier Members
		IReference IPartyTradeIdentifier.PartyReference 
		{
			set { this.partyReference = (PartyReference)value; }
			get { return this.partyReference; } 
		}
		bool IPartyTradeIdentifier.BookIdSpecified 
		{ 
			set { this.bookIdSpecified = value; } 
			get { return this.bookIdSpecified; } 
		}
		IBookId IPartyTradeIdentifier.BookId 
		{
			get { return this.bookId; } 
		}
		bool IPartyTradeIdentifier.LinkIdSpecified
		{
			set { this.linkIdSpecified = value; }
			get { return this.linkIdSpecified; }
		}
        ILinkId[] IPartyTradeIdentifier.LinkId
        {
            set { this.linkId = (LinkId[])value; }
            get { return this.linkId; }
        }
		bool IPartyTradeIdentifier.LocalClassDervSpecified 
		{ 
			set { this.localClassDervSpecified = value; } 
			get { return this.localClassDervSpecified; } 
		}
		IScheme IPartyTradeIdentifier.LocalClassDerv 
		{
			set { this.localClassDerv = (LocalClassDerv)value; }
			get { return this.localClassDerv; } 
		}
		bool IPartyTradeIdentifier.LocalClassNDrvSpecified
		{
			set { this.localClassNDrvSpecified = value; }
			get { return this.localClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.LocalClassNDrv 
		{
			set { this.localClassNDrv = (LocalClassNDrv)value; } 
			get { return this.localClassNDrv; } 
		}
		bool IPartyTradeIdentifier.IasClassDervSpecified
		{
			set { this.iasClassDervSpecified = value; }
			get { return this.iasClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.IasClassDerv 
		{
			set { this.iasClassDerv = (IASClassDerv)value; } 
			get { return this.iasClassDerv; } 
		}
		bool IPartyTradeIdentifier.IasClassNDrvSpecified
		{
			set { this.iasClassNDrvSpecified = value; }
			get { return this.iasClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.IasClassNDrv 
		{
			set { this.iasClassNDrv = (IASClassNDrv)value; } 
			get { return this.iasClassNDrv; } 
		}
		bool IPartyTradeIdentifier.HedgeClassDervSpecified
		{
			set { this.hedgeClassDervSpecified = value; }
			get { return this.hedgeClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.HedgeClassDerv 
		{
			set { this.hedgeClassDerv = (HedgeClassDerv)value; } 
			get { return this.hedgeClassDerv; } 
		}
		bool IPartyTradeIdentifier.HedgeClassNDrvSpecified
		{
			set { this.hedgeClassNDrvSpecified = value; }
			get { return this.hedgeClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.HedgeClassNDrv 
		{ 
			set { this.hedgeClassNDrv = (HedgeClassNDrv)value; } 
			get { return this.hedgeClassNDrv; } 
		}
		bool IPartyTradeIdentifier.FxClassSpecified
		{
			set { this.fxClassSpecified = value; }
			get { return this.fxClassSpecified; }
		}
		IScheme IPartyTradeIdentifier.FxClass 
		{ 
			set { this.fxClass = (FxClass)value; } 
			get { return this.fxClass; } 
		}
        //
        bool IPartyTradeIdentifier.TradeIdSpecified
        {
            set { this.tradeTradeIdSpecified = true; }
            get { return this.tradeTradeIdSpecified; }
        }
        // EG 20240227 [WI858] Update interfacte signature (ITradeId)
        ITradeId[] IPartyTradeIdentifier.TradeId 
		{
            get
            {
                return (ITradeId[])this.tradeTradeId;
            } 
		}
        //
        bool IPartyTradeIdentifier.VersionedTradeIdSpecified
        {
            set { this.tradeVersionedTradeIdSpecified = true; }
            get { return this.tradeVersionedTradeIdSpecified ; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
        IVersionedSchemeId[] IPartyTradeIdentifier.VersionedTradeId
        {
            get
            {
                return this.tradeVersionedTradeId;
            }
        }

		ILinkId IPartyTradeIdentifier.GetLinkIdFromScheme(string pScheme) { return this.GetLinkIdFromScheme(pScheme); }
		ILinkId IPartyTradeIdentifier.GetLinkIdWithNoScheme() { return this.GetLinkIdWithNoScheme(); }
		#endregion IPartyTradeIdentifier Members
		#region ITradeIdentifier Members
        string ITradeIdentifier.GetTradeIdMemberName() { return "tradeTradeId"; }
        ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		void ITradeIdentifier.RemoveTradeIdFromScheme(string pScheme) { this.RemoveTradeIdFromScheme(pScheme); }
		#endregion ITradeIdentifier Members
	}
	#endregion PartyTradeIdentifier
	#region PartyTradeInformation
    // EG 20170918 [23342] initialize elements to constructor
    // FI 20170928 [23452] Modify
    // EG 20171025 [23509] executionDateTimeOffset, orderEntered, timestamps
    public partial class PartyTradeInformation : IEFS_Array, IPartyTradeInformation
    {
        #region Constructors
        public PartyTradeInformation()
        {
            partyReference = new PartyReference();

            categorySpecified = false;
            category = new TradeCategory[] { new TradeCategory() };

            traderSpecified = false;
            trader = new Trader[] { new Trader() };

            salesSpecified = false;
            sales = new Trader[] { new Trader() };

            executionDateTimeSpecified = false;
            executionDateTime = new ExecutionDateTime();

            // FI 20170928 [23452]
            timestampsSpecified = false;
            timestamps = new TradeProcessingTimestamps();

            // FI 20170928 [23452]
            relatedPartySpecified = false;
            relatedParty = new RelatedParty[] { new RelatedParty() };

            // FI 20170928 [23452]
            relatedPersonSpecified = false;
            relatedPerson = new RelatedPerson[] { new RelatedPerson() };

            // FI 20170928 [23452]
            algorithmSpecified = false;
            algorithm = new Algorithm[] { new Algorithm() };

            // FI 20170928 [23452]
            categorySpecified = false;
            category = new TradeCategory[] { new TradeCategory() };

            shortSaleSpecified = false;
            shortSale = new ShortSale();

            tradingWaiverSpecified = false;
            tradingWaiver = new TradingWaiver[] { new TradingWaiver() };

            otcClassificationSpecified = false;
            otcClassification = new OtcClassification[] { new OtcClassification() };

            isCommodityHedgeSpecified = false;
            isCommodityHedge = new EFS_Boolean(false);

            isSecuritiesFinancingSpecified = false;
            isSecuritiesFinancing = new EFS_Boolean(false);
        }
        #endregion Constructors
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IPartyTradeInformation Members
        // EG 20171031 [23509] Upd
        public object Item { get { return this; } }
        string IPartyTradeInformation.PartyReference
        {
            set { this.partyReference.href = value; }
            get { return this.partyReference.href; }
        }
        bool IPartyTradeInformation.TraderSpecified
        {
            get { return this.traderSpecified; }
            set { this.traderSpecified = value; }
        }
        ITrader[] IPartyTradeInformation.Trader
        {
            get { return this.trader; }
            set { this.trader = (Trader[])value; }
        }

        bool IPartyTradeInformation.SalesSpecified
        {
            get { return this.salesSpecified; }
            set { this.salesSpecified = value; }
        }
        ITrader[] IPartyTradeInformation.Sales
        {
            get { return this.sales; }
            set { this.sales = (Trader[])value; }
        }

        bool IPartyTradeInformation.ExecutionDateTimeSpecified
        {
            get { return this.executionDateTimeSpecified; }
            set { this.executionDateTimeSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IScheme IPartyTradeInformation.ExecutionDateTime
        {
            get { return this.executionDateTime; }
            set 
            {
                if (value is SchemeData)
                    this.executionDateTime = new ExecutionDateTime() { executionDateTimeScheme = value.Scheme, Value = value.Value };
                else
                    this.executionDateTime = (ExecutionDateTime)value; 
            }
        }
        Nullable<DateTimeOffset> IPartyTradeInformation.ExecutionDateTimeOffset
        {
            get {return Tz.Tools.ToDateTimeOffset(this.executionDateTime.Value);}
        }
        bool IPartyTradeInformation.OrderEnteredSpecified
        {
            get { return timestampsSpecified && timestamps.OrderEnteredSpecified; }
        }
        bool IPartyTradeInformation.TimestampsSpecified
        {
            get { return timestampsSpecified; }
            set { timestampsSpecified = value; }
        }
        ITradeProcessingTimestamps IPartyTradeInformation.Timestamps
        {
            get { return timestamps; }
            set { timestamps = (TradeProcessingTimestamps)value; }
        }
        bool IPartyTradeInformation.BrokerPartyReferenceSpecified
        {
            get { return this.brokerPartyReferenceSpecified; }
            set { this.brokerPartyReferenceSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IReference[] IPartyTradeInformation.BrokerPartyReference
        {
            get { return this.brokerPartyReference; }
            set 
            {
                //this.brokerPartyReference = (ArrayPartyReference[])value;
                this.brokerPartyReference = new ArrayPartyReference[] { };
                if (ArrFunc.IsFilled(value))
                    this.brokerPartyReference = value.Cast<ArrayPartyReference>().ToArray();
                this.brokerPartyReferenceSpecified = ArrFunc.IsFilled(this.brokerPartyReference);
            }
        }


        bool IPartyTradeInformation.RelatedPartySpecified
        {
            get { return relatedPartySpecified; }
            set { relatedPartySpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IRelatedParty[] IPartyTradeInformation.RelatedParty // FI 20170928 [23452] add
        {
            get { return relatedParty; }
            set 
            {
                this.relatedParty = new RelatedParty[]{};
                if (ArrFunc.IsFilled(value))
                    this.relatedParty = value.Cast<RelatedParty>().ToArray();
                this.relatedPartySpecified = ArrFunc.IsFilled(this.relatedParty);
            }
        }

        bool IPartyTradeInformation.RelatedPersonSpecified
        {
            get { return relatedPersonSpecified; }
            set { relatedPersonSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IRelatedPerson[] IPartyTradeInformation.RelatedPerson  // FI 20170928 [23452] add
        {
            get { return relatedPerson; }
            set 
            {
                this.relatedPerson = new RelatedPerson[] { };
                if (ArrFunc.IsFilled(value))
                    this.relatedPerson = value.Cast<RelatedPerson>().ToArray();
                this.relatedPersonSpecified = ArrFunc.IsFilled(this.relatedPerson);
            }
        }

        bool IPartyTradeInformation.AlgorithmSpecified
        {
            get { return algorithmSpecified; }
            set { algorithmSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IAlgorithm[] IPartyTradeInformation.Algorithm  // FI 20170928 [23452] add
        {
            get { return this.algorithm; }
            set 
            {
                this.algorithm = new Algorithm[] { };
                if (ArrFunc.IsFilled(value))
                    this.algorithm = value.Cast<Algorithm>().ToArray();
                this.algorithmSpecified = ArrFunc.IsFilled(this.algorithm);
            }
        }

        bool IPartyTradeInformation.CategorySpecified
        {
            get { return categorySpecified; }
            set { categorySpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IScheme[] IPartyTradeInformation.Category    // FI 20170928 [23452] add
        {
            get { return this.category; }
            set
            {
                this.category = new TradeCategory[] {};
                if (ArrFunc.IsFilled(value))
                {
                    if (0 < value.OfType<SchemeData>().Count())
                        this.category = (from item in value 
                                         select new TradeCategory(){categoryScheme = item.Scheme, Value = item.Value}).ToArray();
                    else
                        this.category = value.Cast<TradeCategory>().ToArray(); 
                }
                this.categorySpecified = ArrFunc.IsFilled(this.category);
            }
        }


        bool IPartyTradeInformation.TradingWaiverSpecified
        {
            get { return tradingWaiverSpecified; }
            set { tradingWaiverSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IScheme[] IPartyTradeInformation.TradingWaiver    // FI 20170928 [23452] add
        {
            get { return this.tradingWaiver; }
            set
            {
                this.tradingWaiver = new TradingWaiver[] { };
                if (ArrFunc.IsFilled(value))
                {
                    if (0 < value.OfType<SchemeData>().Count())
                        this.tradingWaiver = (from item in value 
                                              select new TradingWaiver() { tradingWaiverScheme = item.Scheme, Value = item.Value }).ToArray();
                    else
                        this.tradingWaiver = value.Cast<TradingWaiver>().ToArray();
                }
                this.tradingWaiverSpecified = ArrFunc.IsFilled(this.tradingWaiver);
            }
        }

        bool IPartyTradeInformation.ShortSaleSpecified
        {
            get { return shortSaleSpecified; }
            set { shortSaleSpecified = value; }
        }
        IScheme IPartyTradeInformation.ShortSale  
            // FI 20170928 [23452] add
        {
            get { return this.shortSale; }
            set { shortSale = new ShortSale { shortSaleScheme = value.Scheme, Value = value.Value }; }
        }


        bool IPartyTradeInformation.OtcClassificationSpecified
        {
            get { return otcClassificationSpecified; }
            set { otcClassificationSpecified = value; }
        }
        // EG 20171016 [23509] Upd
        IScheme[] IPartyTradeInformation.OtcClassification    // FI 20170928 [23452] add
        {
            get { return this.otcClassification; }
            set
            {
                this.otcClassification = new OtcClassification[] { };
                if (ArrFunc.IsFilled(value))
                {
                    if (0 < value.OfType<SchemeData>().Count())
                        this.otcClassification = (from item in value
                                                  select new OtcClassification() { otcClassificationScheme = item.Scheme, Value = item.Value }).ToArray();
                    else
                        this.otcClassification = value.Cast<OtcClassification>().ToArray();
                }
                this.otcClassificationSpecified = ArrFunc.IsFilled(this.otcClassification);
            }
        }

        Boolean IPartyTradeInformation.IsCommodityHedgeSpecified
        {
            get { return isCommodityHedgeSpecified; }
            set { isCommodityHedgeSpecified = value; }
        }

        Boolean IPartyTradeInformation.IsCommodityHedge // FI 20170928 [23452] add
        {
            get { return isCommodityHedge.BoolValue; }
            set { isCommodityHedge = new EFS_Boolean(value); }
        }



        Boolean IPartyTradeInformation.IsSecuritiesFinancingSpecified
        {
            get { return isSecuritiesFinancingSpecified; }
            set { isSecuritiesFinancingSpecified = value; }
        }

        Boolean IPartyTradeInformation.IsSecuritiesFinancing // FI 20170928 [23452] add
        {
            get { return isSecuritiesFinancing.BoolValue; }
            set { isSecuritiesFinancing = new EFS_Boolean(value); }
        }

        #endregion IPartyTradeInformation Members
    }
	#endregion PartyTradeInformation

	#region Strategy
	public partial class Strategy : IStrategy
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return (IProduct[])((Strategy)this).Item; } }
		#endregion IProduct Members

		#region IStrategy Members
		//IProduct[] IStrategy.items { get { return (IProduct[])this.Item; } }
        object[] IStrategy.SubProduct
        {
            get { return this.Item; }
            set { Item = (Product[])value; }
        }
        //
        bool IStrategy.PremiumProductReferenceSpecified
        {
            get { return premiumProductReferenceSpecified; }
            set { premiumProductReferenceSpecified = value; }
        }
        IReference IStrategy.PremiumProductReference
        {
            get { return (IReference)premiumProductReference; }
            set { premiumProductReference = (ProductReference)value; }
        }
        //
        bool IStrategy.MainProductReferenceSpecified
        {
            get { return mainProductReferenceSpecified; }
            set { mainProductReferenceSpecified = value; }
        }
        IReference IStrategy.MainProductReference
        {
            get { return (IReference)mainProductReference; }
            set { mainProductReference = (ProductReference)value; }
        }
        //
        #region indexor
        IProduct IStrategy.this[int pIndex]
        {
            get { return (IProduct)this.Item[pIndex]; }

        }
        #endregion
        #endregion IStrategy Members

    }
	#endregion Strategy

	#region Trade
    public partial class Trade : IEFS_Array, ITrade, IProduct
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedTradeDate
        {
            get
            {

                return tradeHeader.AdjustedTradeDate;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TradeDate
        {
            get
            {

                return tradeHeader.TradeDate;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object Extends
        {
            get
            {
                return extends;
            }

        }
        #endregion Accessors

        #region Constructors
        public Trade()
        {
            tradeHeader = new TradeHeader();
            calculationAgent = new CalculationAgent();
            calculationAgentBusinessCenter = new BusinessCenter();
            documentation = new Documentation();
            governingLaw = new GoverningLaw();
            extends = new TradeExtends();
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

        #region ITrade Members
        ITradeHeader ITrade.TradeHeader
        {
            get { return (ITradeHeader)this.tradeHeader; }
            set { this.tradeHeader = (TradeHeader)value; }
        }
        IProduct ITrade.Product
        {
            get { return (IProduct)this.product; }
            set { this.product = (Product)value; }  
        }
        bool ITrade.OtherPartyPaymentSpecified
        {
            set { this.otherPartyPaymentSpecified = value; }
            get { return this.otherPartyPaymentSpecified; }
        }
        // EG 20101020 Ticket:17185
        IPayment[] ITrade.OtherPartyPayment
        {
            set 
            {
                this.otherPartyPayment = null;
                if (ArrFunc.IsFilled(value))
                {
                    this.otherPartyPayment = value.Cast<Payment>().ToArray();
                }
                //this.otherPartyPayment = (Payment[])value; 
            }
            get { return this.otherPartyPayment; }
        }
        EFS_Events ITrade.ProductEvents
        {
            set { this.product.efs_Events = value; }
            get { return this.product.efs_Events; }
        }
        bool ITrade.SettlementInputSpecified
        {
            set { this.settlementInputSpecified = value; }
            get { return this.settlementInputSpecified; }
        }
        ISettlementInput[] ITrade.SettlementInput { get { return this.settlementInput; } }
        bool ITrade.BrokerPartyReferenceSpecified
        {
            set { this.brokerPartyReferenceSpecified = value; }
            get { return this.brokerPartyReferenceSpecified; }
        }
        // EG 20171016 [23509] Upd
        IReference[] ITrade.BrokerPartyReference
        {
            set 
            {
                //this.brokerPartyReference = (ArrayPartyReference[])value;
                this.brokerPartyReference = new ArrayPartyReference[] { };
                if (ArrFunc.IsFilled(value))
                    this.brokerPartyReference = value.Cast<ArrayPartyReference>().ToArray();
                this.brokerPartyReferenceSpecified = ArrFunc.IsFilled(this.brokerPartyReference);
            }
            get { return this.brokerPartyReference; }
        }
        bool ITrade.NettingInformationInputSpecified
        {
            set { this.nettingInformationInputSpecified = value; }
            get { return this.nettingInformationInputSpecified; }
        }
        INettingInformationInput ITrade.NettingInformationInput
        {
            set { this.nettingInformationInput = (NettingInformationInput)value; }
            get { return this.nettingInformationInput; }
        }
        bool ITrade.CalculationAgentSpecified
        {
            set { this.calculationAgentSpecified = value; }
            get { return this.calculationAgentSpecified; }
        }
        ICalculationAgent ITrade.CalculationAgent
        {
            set { this.calculationAgent = (CalculationAgent)value; }
            get { return this.calculationAgent; }
        }
        DateTime ITrade.AdjustedTradeDate { get { return this.AdjustedTradeDate.DateValue; } }
        bool ITrade.GoverningLawSpecified
        {
            set { this.governingLawSpecified = value; }
            get { return this.governingLawSpecified; }
        }
        IScheme ITrade.GoverningLaw
        {
            set { this.governingLaw = (GoverningLaw)value; }
            get { return this.governingLaw; }
        }
        bool ITrade.TradeSideSpecified
        {
            set { this.tradeSideSpecified = value; }
            get { return this.tradeSideSpecified; }
        }
        // EG 20171016 [23509] Upd
        ITradeSide[] ITrade.TradeSide
        {
            set 
            {
                this.tradeSide = null;
                if (ArrFunc.IsFilled(value))
                {
                    //        tradeSide = new TradeSide[value.Length];
                    //        value.CopyTo(tradeSide, 0);
                    this.tradeSide = value.Cast<TradeSide>().ToArray(); 
                }
                this.tradeSideSpecified = ArrFunc.IsFilled(this.tradeSide);
            }
            get
            {
                return this.tradeSide;
            }
        }
        bool ITrade.DocumentationSpecified
        {
            get { return this.documentationSpecified; }
            set { this.documentationSpecified = value; }
        }
        IDocumentation ITrade.Documentation
        {
            get { return this.documentation; }
            set { this.documentation = (Documentation)value; }
        }
        bool ITrade.TradeIntentionSpecified
        {
            get { return this.tradeIntentionSpecified; }
            set { this.tradeIntentionSpecified = value; }
        }
        ITradeIntention ITrade.TradeIntention
        {
            get { return this.tradeIntention; }
            set { this.tradeIntention = (TradeIntention)value; }
        }

        /// FI 20130627 [18745] add
        bool ITrade.TradeIdSpecified
        {
            get { return this.tradeIdSpecified; }
            set { this.tradeIdSpecified = value; }
        }
        
        /// <summary>
        /// Représente l'identifier du trade dans Spheres®
        /// <para>Renseigné dans le cadre de la messagerie</para>
        /// </summary>
        /// FI 20130627 [18745] add
        string ITrade.TradeId
        {
            get { return this.tradeId; }
            set { this.tradeId = value; }
        }


        IDocumentation ITrade.CreateDocumentation()
        {
            return new Documentation();
        }
        IParty ITrade.CreateParty()
        {
            return new Party();
        }
        ISettlementChain ITrade.CreateSettlementChain()
        {
            return new SettlementChain();
        }
        IIssiItemsRoutingActorsInfo ITrade.CreateIssiItemsRoutingActorsInfo(string pConnectionString, int pIdIssi, IssiItem[] pIssiItem)
        {
            return new IssiItemsRoutingActorsInfo(pConnectionString, pIdIssi, pIssiItem);
        }
        bool ITrade.ExtendsSpecified { get { return this.extendsSpecified; } set { this.extendsSpecified = value; } }
        ITradeExtends ITrade.Extends { get { return this.extends; } }
        // EG 20101020 Ticket:17185
        void ITrade.SetOtherPartyPayment(ArrayList pOtherPartyPayment)
        {
            otherPartyPayment = (Payment[])pOtherPartyPayment.ToArray(typeof(Payment));
        }

        #endregion ITrade Members

        #region IProduct Members
        object IProduct.Product { get { return this.product; } }
        IProductBase IProduct.ProductBase { get { return this.product; } }
        #endregion IProduct Members
    }
	#endregion Trade
    #region TradeDate
    // EG 20140702 Add businessDate
    public partial class TradeDate : ITradeDate
    {
        #region Accessors
        #region TimeStamp
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime DtTimeStamp
        {
            get
            {
                return DtFunc.AddTimeToDate(DateValue, new DtFunc().StringToDateTime(TimeStamp));
            }
        }
        #endregion TimeStamp
        #endregion Accessors

        #region constructor
        public TradeDate()
        {
            efs_id = new EFS_Id();
        }
        #endregion

        #region ITradeDate Members
        string ITradeDate.Efs_id
        {
            set { this.efs_id.Value = value; }
            get { return this.efs_id.Value; }
        }
        string ITradeDate.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        DateTime ITradeDate.DateValue { get { return this.DateValue; } }
        DateTime ITradeDate.DateTimeValue { get { return this.DtTimeStamp; } }
        string ITradeDate.TimeStampHHMMSS
        {
            set { this.TimeStamp = value; }
            get { return this.TimeStamp; }
        }
        DateTime ITradeDate.TimeStamp
        {
            set { this.TimeStamp = value.ToLongTimeString(); }
            get { return this.DtTimeStamp; }
        }
        string ITradeDate.BusinessDate
        {
            set { this.BizDt = value; }
            get { return this.BizDt; }
        }
        // EG 20171016 [23509] Unused
        //DateTime ITradeDate.BusinessDate
        //{
        //    get { return StrFunc.IsFilled(bizDt) ? this.efs_BizDt.DateValue : this.DateValue; }
        //    set { this.efs_BizDt = new EFS_Date(); this.efs_BizDt.DateValue = value; }
        //}
        #endregion ITradeDate Members
    }
    #endregion TradeDate
	#region TradeHeader
    // EG 20171025 [23509] Add clearedDate
	public partial class TradeHeader : ITradeHeader
	{
		#region Accessors
		#region AdjustedTradeDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Date AdjustedTradeDate
		{
			get { return new EFS_Date(tradeDate.Value); }
		}
		#endregion AdjustedTradeDate
		#region TradeDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_EventDate TradeDate
		{
			get { return new EFS_EventDate(tradeDate.DateValue, tradeDate.DateValue); }
		}
		#endregion TradeDate
		#endregion Accessors
		#region Constructors
		public TradeHeader()
		{
			partyTradeIdentifier = new PartyTradeIdentifier[1] { new PartyTradeIdentifier() };
			partyTradeInformation = new PartyTradeInformation[1] { new PartyTradeInformation() };
			tradeDate = new TradeDate();
            clearedDate = new IdentifiedDate();
		}
		#endregion Constructors

		#region ITradeHeader Members
        // EG 20171016 [23509] Upd
        IPartyTradeIdentifier[] ITradeHeader.PartyTradeIdentifier 
        {
            get { return this.partyTradeIdentifier; }
            set 
            {
                //this.partyTradeIdentifier = (PartyTradeIdentifier[])value;
                this.partyTradeIdentifier = new PartyTradeIdentifier[] { };
                if (ArrFunc.IsFilled(value))
                    this.partyTradeIdentifier = value.Cast<PartyTradeIdentifier>().ToArray();
            }
            
        }
		bool ITradeHeader.PartyTradeInformationSpecified 
		{ 
			set { this.partyTradeInformationSpecified = value; } 
			get { return this.partyTradeInformationSpecified; } 
		}
        // EG 20171016 [23509] Upd
		IPartyTradeInformation[] ITradeHeader.PartyTradeInformation 
		{ 
            set 
            {
                //this.partyTradeInformation = (PartyTradeInformation[])value;
                this.partyTradeInformation = new PartyTradeInformation[] { };
                if (ArrFunc.IsFilled(value))
                    this.partyTradeInformation = value.Cast<PartyTradeInformation>().ToArray();
                this.partyTradeInformationSpecified = ArrFunc.IsFilled(this.partyTradeInformation);
            }
			get { return this.partyTradeInformation; } 
		}
		ITradeDate ITradeHeader.TradeDate {get { return this.tradeDate; }}
		DateTime ITradeHeader.AdjustedTradeDate { get { return this.AdjustedTradeDate.DateValue; } }

        bool ITradeHeader.ClearedDateSpecified
        {
            set { this.clearedDateSpecified = value; }
            get { return this.clearedDateSpecified; }
        }
        IAdjustedDate ITradeHeader.ClearedDate
        {
            set { this.clearedDate = (IdentifiedDate)value; }
            get { return this.clearedDate; }
        }
		#endregion ITradeHeader Members

	}
    #endregion TradeHeader
    #region TradeId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
            // EG 20240227 [WI858] Update interfacte signature (ITradeId)
    public partial class TradeId : IEFS_Array, IComparable, ITradeId
	{
		#region Constructors
		public TradeId()
		{
			tradeIdScheme = string.Empty;
			Value = string.Empty;
		}
		#endregion Constructors

		#region IComparable Members
		#region CompareTo
		public int CompareTo(object pObj)
		{
            if (pObj is TradeId objTradeId)
            {
                int ret = 0;
                //
                if (0 == ret)
                {
                    if (objTradeId.tradeId != tradeId)
                        ret = -1;
                }
                //
                if (0 == ret)
                {
                    if (objTradeId.id != id)
                        ret = -1;
                }
                //
                if (0 == ret)
                {
                    if (objTradeId.Value != Value)
                        ret = -1;
                }
                return ret;
            }
            throw new ArgumentException("object is not a TradeId");
		}
		#endregion CompareTo
		#endregion IComparable Members
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ISchemeId Members
		string ISchemeId.Id 
		{ 
			set { this.id = value; } 
			get { return this.id; }
		}
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set { this.source = value; }
            get { return this.source; }
        }
        #endregion ISchemeId Members
        /// <summary>
        /// Les TradeId sont considérés comme non relatifs à un actor pour
        /// les schemes : 
        /// - Spheres_TradeIdMarketTransactionIdScheme
        /// - Cst.Spheres_TradeIdTvticScheme
        /// </summary>
        bool ITradeId.IsRelativeToActor
        {
            get
            {
                return (this.tradeIdScheme != Cst.Spheres_TradeIdMarketTransactionIdScheme) &&
                       (this.tradeIdScheme != Cst.Spheres_TradeIdTvticScheme);
            }
        }
        #region IScheme Members
        string IScheme.Scheme
		{
			set { this.tradeIdScheme = value; }
			get { return this.tradeIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion TradeId
	#region TradeIdentifier
	public partial class TradeIdentifier : ITradeIdentifier
	{
		#region Constructors
		public TradeIdentifier()
		{
			partyReference			= new PartyReference();
			tradeTradeId			= new TradeId[1] { new TradeId() };
			tradeVersionedTradeId	= new VersionedTradeId[1] { new VersionedTradeId() };
		}
		#endregion Constructors
		
        #region Methods
        #region GetTradeIdMemberName
        public static string GetTradeIdMemberName()
        {
            return "tradeTradeId";
        }
        #endregion
        #region GetTradeIdFromScheme
        public TradeId GetTradeIdFromScheme(string pScheme)
		{
			TradeId ret = null;
			if (ArrFunc.IsFilled(tradeTradeId))
			{
				for (int i = 0; i < tradeTradeId.Length; i++)
				{
                    if (tradeTradeId[i].tradeIdScheme == pScheme)
					{
						ret = tradeTradeId[i];
						break;
					}
				}
			}
			return ret;
		}
		#endregion GetTradeIdFromScheme
		#region GetTradeIdWithNoScheme
		public TradeId GetTradeIdWithNoScheme()
		{
			TradeId ret = null;
			if (ArrFunc.IsFilled(tradeTradeId))
			{
				for (int i = 0; i < tradeTradeId.Length; i++)
				{
					if (StrFunc.IsEmpty(tradeTradeId[i].tradeIdScheme))
					{
						ret = tradeTradeId[i];
						break;
					}
				}
			}
			return ret;
		}
        #endregion GetTradeIdWithNoScheme
        #region RemoveTradeIdFromScheme
        // EG 20240227 [WI855] Upd GetTradeIdMemberName() instead of "tradeId"
        public void RemoveTradeIdFromScheme(string pScheme)
		{
			if (ArrFunc.IsFilled(tradeTradeId))
			{
				for (int i = tradeTradeId.Length - 1; -1 < i; i--)
				{
					if (tradeTradeId[i].tradeIdScheme == pScheme)
						ReflectionTools.RemoveItemInArray(this, GetTradeIdMemberName(), i);
				}
			}
		}
        /// <summary>
        /// Suppression d'un tradeId pour un scheme donné
        /// </summary>
        /// <param name="pScheme"></param>
        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        public void RemoveTradeId(string pScheme)
        {
            if (null != tradeTradeId)
                tradeTradeId = tradeTradeId.Where(item => (item.tradeIdScheme != pScheme)).ToArray();
            tradeTradeIdSpecified = ArrFunc.IsFilled(tradeTradeId);
        }

        #endregion RemoveTradeIdFromScheme
        /// <summary>
        /// Ajout d'un tradeId pour un scheme donné
        /// </summary>
        /// <param name="pScheme"></param>
        /// <param name="pValue"></param>
        // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
        public void SetTradeId(string pScheme, string pValue)
        {
            bool isMustBeApplied = (null == tradeTradeId) || (0 == tradeTradeId.Where(item => (item.tradeIdScheme == pScheme) && item.Value.Equals(pValue)).Count());
            if (isMustBeApplied)
            {
                TradeId newTradeId = new TradeId
                {
                    tradeIdScheme = pScheme,
                    Value = pValue
                };

                if (ArrFunc.IsFilled(tradeTradeId))
                    tradeTradeId = tradeTradeId.Where(item => item.tradeIdScheme != pScheme).ToArray();

                if (ArrFunc.IsFilled(tradeTradeId))
                    tradeTradeId = (from item in tradeTradeId select (TradeId)item).Concat(new TradeId[] { newTradeId }).ToArray();
                else
                    tradeTradeId= new TradeId[] { newTradeId };
            }
        }

        #endregion Methods

        #region ITradeIdentifier Members
        string ITradeIdentifier.GetTradeIdMemberName() { return GetTradeIdMemberName(); } 
        ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		#endregion ITradeIdentifier Members
	}
	#endregion TradeIdentifier
    #region TradeSide
    public partial class TradeSide : ITradeSide, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion Accessors
        #region constructor
        public TradeSide()
        {
            creditor = new PartyRole();
        }
        #endregion
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
        #region ITradeSide Membres
        bool ITradeSide.ConfirmerSpecified
        {
            get { return this.confirmerSpecified; }
            set { this.confirmerSpecified = value; }
        }
        IPartyRole ITradeSide.Confirmer
        {
            get { return (IPartyRole)this.confirmer; }
            set { this.confirmer = (PartyRole)value; }
        }
        bool ITradeSide.SettlerSpecified
        {
            get { return this.settlerSpecified; }
            set { this.settlerSpecified = value; }
        }
        IPartyRole ITradeSide.Settler
        {
            get { return (IPartyRole)this.settler; }
            set { this.settler = (PartyRole)value; }
        }
        IPartyRole ITradeSide.Creditor
        {
            get { return (IPartyRole)this.creditor; }
            set { this.creditor = (PartyRole)value; }
        }
        #endregion
    }
    #endregion TradeSide
    
    #region Trader
    public partial class Trader : ItemGUI,IEFS_Array,ITrader
	{
		#region Accessors
		#region OTCmlId
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get { return (StrFunc.IsFilled(otcmlId) ? Convert.ToInt32(otcmlId) : 0); }
			set { otcmlId = value.ToString(); }
		}
		#endregion OTCmlId
		#endregion Accessors
		#region Constructors
		public Trader() 
		{ 
			Value = string.Empty;
            // 20090610 RD 
            // pour avoir toujours une valeur pour l'accesseur "this.factor" et ainsi il sera sérialisé même avec une valeur à zero 
            factor = new EFS_Decimal(0);
		}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
        // EG 20170918 [23342] Add optionalItem
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ITrader Members
		string ITrader.Identifier
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		decimal ITrader.Factor
		{
			get { return this.factor.DecValue; }
			set { this.factor = new EFS_Decimal(value); }
		}
        string ITrader.StrFactor
        {
            set { this.Factor = value; }
            get { return this.Factor; }
        }
		string ITrader.Name
		{
			set { this.Name = value; }
			get { return this.Name; }
		}
		string ITrader.OtcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
		int ITrader.OTCmlId
		{
			set { this.OTCmlId = value; }
			get { return this.OTCmlId; }
		}
        string ITrader.Scheme
        {
            set { this.traderScheme = value; }
            get { return this.traderScheme; }
        }
		#endregion ITrader Members
    }
    #endregion Trader

    #region VersionedTradeId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class VersionedTradeId : IVersionedSchemeId
	{
		#region Constructors
		public VersionedTradeId()
		{
			effectiveDate = new IdentifiedDate();
		}
		#endregion Constructors

        #region ISchemeId Members
		string ISchemeId.Id 
		{
			set { this.tradeId.id = value;} 
			get { return this.tradeId.id;}
		}
        string ISchemeId.Source
        {
            set { this.tradeId.source = value; }
            get { return this.tradeId.source; }
        }

        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.Scheme
		{
			set { this.tradeId.tradeIdScheme = value; }
			get { return this.tradeId.tradeIdScheme; }
		}
		string IScheme.Value
		{
			set { this.tradeId.Value = value; }
			get { return this.tradeId.Value; }
		}
        #endregion IScheme Members
        #region IVersionedSchemeId Members
        IAdjustedDate IVersionedSchemeId.EffectiveDate
        {
            set { this.effectiveDate = (IdentifiedDate)value; }
            get { return this.effectiveDate; }
        }
        bool  IVersionedSchemeId.EffectiveDateSpecified
        {
            set { this.effectiveDateSpecified = value; }
            get { return this.effectiveDateSpecified; }
        }
        #endregion IVersionedSchemeId Members
    }
	#endregion VersionedTradeId
}
namespace FpML.v44.Doc.ToDefine
{
    #region Event
    public abstract partial class Event : IEvent
	{
		#region IEvent Members
		ISchemeId[] IEvent.EventId
		{
			set {this.eventId = (EventId[])value;}
			get {return this.eventId;}
		}
		#endregion IEvent Members
	}
    #endregion Event
    #region EventId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class EventId : ISchemeId,IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ISchemeId Members
		string ISchemeId.Id
		{
			set { this.id = value; }
			get { return this.id; }
		}
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.Scheme
		{
			set { this.eventIdScheme = value; }
			get { return this.eventIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion EventId

	#region Increase
	public partial class Increase
	{
		#region Constructors
		public Increase()
		{
			detailIncreaseInNotionalAmount		= new Money();
			detailOutstandingNotionalAmount		= new Money();
			detailIncreaseInNumberOfOptions		= new EFS_Decimal();
			detailOutstandingNumberOfOptions	= new EFS_Decimal();
		}
		#endregion Constructors
	}
	#endregion Increase
}
