using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Linq;
using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.Common;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.EventMatrix;
using EfsML.Interface;
using EfsML.Settlement; 

using EfsML.v20;
using EfsML.v20.Settlement;
using EfsML.v20.Settlement.Message;

using FixML.v44;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Cd;
using FpML.v42.Ird;
using FpML.v42.Fx;
using FpML.v42.Eqd;
using FpML.v42.Eqs;
using FpML.v42.Msg;
using FpML.v42.Shared;

using Tz = EFS.TimeZone;



namespace FpML.v42.Doc
{
    #region AccountReference
    public partial class AccountReference : IReference 
    {
        #region IReference Members
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion
    
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
            //
            portfolioSpecified = false;
            portfolio = new Portfolio[1] { new Portfolio() };
        }
		#endregion Constructors
		#region IDataDocument Members
        // PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
		IParty[] IDataDocument.party 
		{
			set { 
                //this.party = (Party[])value; 
                this.party = new Party[] { };
                if (ArrFunc.IsFilled(value))
                    this.party = value.Cast<Party>().ToArray();
            }
			get { return this.party; }
		}
        ITrade[] IDataDocument.trade
        {
            set { this.trade = (Trade[])value; }
            get { return this.trade; }
        }
		ITrade IDataDocument.firstTrade 
		{
			set { this.trade[0] = (Trade)value; } 
			get { return (ITrade)this.trade[0]; } 
		}
        object IDataDocument.item
        {
            get { return this; }
        }
        #region methods
        Type IDataDocument.GetTypeParty() {  return this.party.GetType().GetElementType(); }
		
        #endregion
        #endregion IDataDocument Members

        #region IDocument Members
        DocumentVersionEnum IDocument.version {get { return this.version;} }
		#endregion IDocument Members
	}
	#endregion DataDocument

	#region LinkId
	public partial class LinkId : ItemGUI,IEFS_Array ,ILinkId
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
		string ILinkId.linkIdScheme
		{
			get { return this.linkIdScheme; }
			set { this.linkIdScheme = value; }
		}
		string ILinkId.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
        decimal ILinkId.Factor
        {
            get { return this.Factor.DecValue; }
            set { this.Factor = new EFS_Decimal(value); }
        }
		string ILinkId.id
		{
			get { return this.id; }
			set { this.id = value; }
		}
		#endregion LinkId Members
	}
	#endregion LinkId

	#region Party
    /// EG 20170926 [22374] New The party's time zone.
    /// <summary>
    /// 
    /// </summary>
    // FI 20170928 [23452] Modify
    // PL 20180618 Harmonisation avec code FpML.v4.4
	public partial class Party : IEFS_Array, IParty
	{
		#region Accessors
		#region PartyId
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public IScheme PartyId
		{
            //set { this.partyId = (PartyId)value; }
            //get { return this.partyId; }
            set { this.partyId[0] = (PartyId)value; }
            get { return this.partyId[0]; }
        }
		#endregion PartyId
        #region PartyIds
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IScheme[] partyIds
        {
            //set { this.partyId = ((PartyId[])value)[0]; }
            //get { return (new PartyId[1] { this.partyId }); }
            set { this.partyId = (PartyId[])value; }
            get { return this.partyId; }
        }
        #endregion PartyIds
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
		public Party()
		{
            //partyId = new PartyId();
            partyId = new PartyId[1] { new PartyId() };
		}
		#endregion Constructors
		#region Methods
		#region RemoveReference
		public void RemoveReference(FullConstructor pFullCtor)
		{
			pFullCtor.LoadEnumObjectReference("PartyReference", this.otcmlId, null);
		}
		#endregion RemoveReference
		#endregion Methods

		#region IEFS_Array Members
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion IEFS_Array Members
		#region IParty Members
		string IParty.partyId 
		{
            //set { this.partyId.Value = value; }
            //get { return this.partyId.Value; }
            set { this.partyId[0].Value = value; }
            get { return this.partyId[0].Value; }
        }
		string IParty.partyName
		{
			set { this.partyName = value; }
			get { return this.partyName; }
		}
		string IParty.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
        string IParty.otcmlId { get { return this.otcmlId; } }
		int IParty.OTCmlId 
		{ 
			set { this.OTCmlId = value; } 
			get { return this.OTCmlId; } 
		}
        string IParty.tzdbid
        {
            set { ; }
            get { return string.Empty; }
        }
        // FI 20170928 [23452] add Person
        bool IParty.personSpecified
        {
            get { return false; }
            set { ;}
        }
        IPerson[] IParty.person
        {
            get { return null; }
            set { ;}
        }
		#endregion IParty Members
	}
	#endregion Party
	#region PartyId
	//public partial class PartyId : ICloneable,IScheme
    public partial class PartyId : ICloneable, IScheme, IEFS_Array
	{
		#region Constructors
		public PartyId()
		{
			//partyIdScheme = "http://www.fpml.org/ext/iso9362";
            //20090416 PL Use OTCml_ActorIdentifierScheme
            partyIdScheme = Cst.OTCml_ActorIdentifierScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			PartyId clone = new PartyId();
			clone.partyIdScheme = this.partyIdScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			set { this.partyIdScheme = value; }
			get { return this.partyIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members

        #region Methods
        #region _Value
        // EG 20170918 [23342] Upd
        public static object _Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new EFS.GUI.ComplexControls.PartyId(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods
    }
	#endregion PartyId

    #region PartyRole
    public partial class PartyRole : IPartyRole 
    {
        #region IPartyRole Membres
        bool IPartyRole.accountSpecified
        {
            get { return this.partyRoleAccountSpecified; }
            set { this.partyRoleAccountSpecified = value; }
        }
        IReference IPartyRole.account
        {
            get {  return this.partyRoleAccount; }
            set { this.partyRoleAccount = (AccountReference)value; }
        }

        bool IPartyRole.partySpecified
        {
            get { return this.partyRolePartySpecified; }
            set { this.partyRolePartySpecified = value; }
        }
        IReference IPartyRole.party
        {
            get { return this.partyRoleParty; }
            set { this.partyRoleParty = (PartyReference)value; }
        }
        #endregion
    }
    #endregion

    #region PartyTradeIdentifier
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
    public partial class PartyTradeIdentifier : IEFS_Array,IPartyTradeIdentifier
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
					if (linkId[i].linkIdScheme == pScheme)
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
					if (StrFunc.IsEmpty(linkId[i].linkIdScheme))
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
		IReference IPartyTradeIdentifier.partyReference 
		{ 
			set { this.partyReference = (PartyReference) value; } 
			get { return this.partyReference; } 
		}
		bool IPartyTradeIdentifier.bookIdSpecified
		{
			set { this.bookIdSpecified = value; }
			get { return this.bookIdSpecified; }
		}
		IBookId IPartyTradeIdentifier.bookId { get { return this.bookId; } }
		bool IPartyTradeIdentifier.linkIdSpecified
		{
			get { return this.linkIdSpecified; }
			set { this.linkIdSpecified = value; }
		}
        ILinkId[] IPartyTradeIdentifier.linkId
        {
            set { this.linkId = (LinkId[])value; }
            get { return this.linkId; }
        }
		bool IPartyTradeIdentifier.localClassDervSpecified
		{
			set { this.localClassDervSpecified = value; }
			get { return this.localClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.localClassDerv
		{
			set { this.localClassDerv = (LocalClassDerv)value; }
			get { return this.localClassDerv; }
		}
		bool IPartyTradeIdentifier.localClassNDrvSpecified
		{
			set { this.localClassNDrvSpecified = value; }
			get { return this.localClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.localClassNDrv
		{
			set { this.localClassNDrv = (LocalClassNDrv)value; }
			get { return this.localClassNDrv; }
		}
		bool IPartyTradeIdentifier.iasClassDervSpecified
		{
			set { this.iasClassDervSpecified = value; }
			get { return this.iasClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.iasClassDerv
		{
			set { this.iasClassDerv = (IASClassDerv)value; }
			get { return this.iasClassDerv; }
		}
		bool IPartyTradeIdentifier.iasClassNDrvSpecified
		{
			set { this.iasClassNDrvSpecified = value; }
			get { return this.iasClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.iasClassNDrv
		{
			set { this.iasClassNDrv = (IASClassNDrv)value; }
			get { return this.iasClassNDrv; }
		}
		bool IPartyTradeIdentifier.hedgeClassDervSpecified
		{
			set { this.hedgeClassDervSpecified = value; }
			get { return this.hedgeClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.hedgeClassDerv
		{
			set { this.hedgeClassDerv = (HedgeClassDerv)value; }
			get { return this.hedgeClassDerv; }
		}
		bool IPartyTradeIdentifier.hedgeClassNDrvSpecified
		{
			set { this.hedgeClassNDrvSpecified = value; }
			get { return this.hedgeClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.hedgeClassNDrv
		{
			set { this.hedgeClassNDrv = (HedgeClassNDrv)value; }
			get { return this.hedgeClassNDrv; }
		}
		bool IPartyTradeIdentifier.fxClassSpecified
		{
			set { this.fxClassSpecified = value; }
			get { return this.fxClassSpecified; }
		}
		IScheme IPartyTradeIdentifier.fxClass
		{
			set { this.fxClass = (FxClass)value; }
			get { return this.fxClass; }
		}
        // en 4.2 tradeId est obligatoire
        bool IPartyTradeIdentifier.versionedTradeIdSpecified  
        {
            set { ; }
            get { return false; }
        }
        IVersionedSchemeId[] IPartyTradeIdentifier.versionedTradeId  { get { return (IVersionedSchemeId[])null; } }

        // en 4.2 tradeId est obligatoire
        bool IPartyTradeIdentifier.tradeIdSpecified
        {
            set { ; }
            get { return true; }
        }
        ISchemeId[] IPartyTradeIdentifier.tradeId { get { return this.tradeId; } }


		ILinkId IPartyTradeIdentifier.GetLinkIdFromScheme(string pScheme) { return this.GetLinkIdFromScheme(pScheme); }
		ILinkId IPartyTradeIdentifier.GetLinkIdWithNoScheme() { return this.GetLinkIdWithNoScheme(); }
		#endregion IPartyTradeIdentifier Members
		#region ITradeIdentifier Members
		ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		void ITradeIdentifier.RemoveTradeIdFromScheme(string pScheme){this.RemoveTradeIdFromScheme(pScheme);}
		#endregion ITradeIdentifier Members
	}
	#endregion PartyTradeIdentifier
	#region PartyTradeInformation
    // EG 20171025 [23509] Add executionDateTimeOffset, orderEntered, timestamps
    // EG 20171031 [23509] Upd
    public partial class PartyTradeInformation : IEFS_Array, IPartyTradeInformation
    {
        #region Constructors
        public PartyTradeInformation()
        {
            partyReference = new PartyReference();
            // FI 20190405 [XXXXX] BANCAPERTA 8.1
            executionDateTimeSpecified = false;
            executionDateTime = new FpML.v44.Doc.ExecutionDateTime();

            // FI 20190405 [XXXXX] BANCAPERTA 8.1
            timestampsSpecified = false;
            timestamps = new EfsML.v30.MiFIDII_Extended.TradeProcessingTimestamps();
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
        public object item { get { return this; } }

        string IPartyTradeInformation.partyReference
        {
            set { this.partyReference.href = value; }
            get { return this.partyReference.href; }
        }
        //
        bool IPartyTradeInformation.traderSpecified
        {
            get { return this.traderSpecified; }
            set { this.traderSpecified = value; }
        }
        ITrader[] IPartyTradeInformation.trader
        {
            get { return this.trader; }
            set { this.trader = (Trader[])value; }
        }
        bool IPartyTradeInformation.salesSpecified
        {
            get { return this.salesSpecified; }
            set { this.salesSpecified = value; }
        }
        ITrader[] IPartyTradeInformation.sales
        {
            get { return this.sales; }
            set { this.sales = (Trader[])value; }
        }
        //
        bool IPartyTradeInformation.brokerPartyReferenceSpecified
        {
            get { return brokerPartyReferenceSpecified; }
            set { brokerPartyReferenceSpecified = value; }
        }
        // PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
        IReference[] IPartyTradeInformation.brokerPartyReference
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

        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        bool IPartyTradeInformation.executionDateTimeSpecified
        {
            get { return this.executionDateTimeSpecified; }
            set { this.executionDateTimeSpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        IScheme IPartyTradeInformation.executionDateTime
        {
            get { return this.executionDateTime; }
            set
            {
                if (value is SchemeData)
                    this.executionDateTime = new FpML.v44.Doc.ExecutionDateTime() { executionDateTimeScheme = value.scheme, Value = value.Value };
                else
                    this.executionDateTime = (FpML.v44.Doc.ExecutionDateTime)value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        Nullable<DateTimeOffset> IPartyTradeInformation.executionDateTimeOffset
        {
            get { return Tz.Tools.ToDateTimeOffset(this.executionDateTime.Value); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        bool IPartyTradeInformation.orderEnteredSpecified
        {
            get { return timestampsSpecified && timestamps.orderEnteredSpecified; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        bool IPartyTradeInformation.timestampsSpecified
        {
            get { return timestampsSpecified; }
            set { timestampsSpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1 
        ITradeProcessingTimestamps IPartyTradeInformation.timestamps
        {
            get { return timestamps; }
            set { timestamps = (EfsML.v30.MiFIDII_Extended.TradeProcessingTimestamps)value; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.relatedPartySpecified
        {
            get { return false; }
            set { ; }
        }
        IRelatedParty[] IPartyTradeInformation.relatedParty
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.relatedPersonSpecified
        {
            get { return false; }
            set { ; }
        }
        IRelatedPerson[] IPartyTradeInformation.relatedPerson
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.algorithmSpecified
        {
            get { return false; }
            set { ; }
        }
        IAlgorithm[] IPartyTradeInformation.algorithm
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.categorySpecified
        {
            get { return false; }
            set { ; }
        }
        IScheme[] IPartyTradeInformation.category
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.tradingWaiverSpecified
        {
            get { return false; }
            set { ; }
        }
        IScheme[] IPartyTradeInformation.tradingWaiver
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.shortSaleSpecified
        {
            get { return false; }
            set { ; }
        }
        IScheme IPartyTradeInformation.shortSale
        {
            get { return null; }
            set { ; }
        }

        // FI 20170928 [23452] add
        bool IPartyTradeInformation.otcClassificationSpecified
        {
            get { return false; }
            set { ; }
        }
        IScheme[] IPartyTradeInformation.otcClassification
        {
            get { return null; }
            set { ; }
        }

        Boolean IPartyTradeInformation.isCommodityHedgeSpecified
        {
            get { return false; }
            set { ; }
        }
        Boolean IPartyTradeInformation.isCommodityHedge
        {
            get { return false; }
            set { ; }
        }


        Boolean IPartyTradeInformation.isSecuritiesFinancingSpecified
        {
            get { return false; }
            set { ; }
        }
        Boolean IPartyTradeInformation.isSecuritiesFinancing
        {
            get { return false; }
            set { ; }
        }


        
        #endregion IPartyTradeInformation Members
    }
	#endregion PartyTradeInformation

	#region Strategy
	public partial class Strategy : IStrategy
	{
		#region IStrategy Members
        //IProduct[] IStrategy.items { get { return (IProduct[])this.Item; } }
        object[] IStrategy.subProduct
        {
            get { return this.Item; }
            set { this.Item = (Product[])value; }
        }
        //
        bool IStrategy.premiumProductReferenceSpecified
        {
            get { return premiumProductReferenceSpecified; }
            set { premiumProductReferenceSpecified = value; }
        }
        IReference IStrategy.premiumProductReference
        {
            get { return (IReference)premiumProductReference; }
            set { premiumProductReference = (ProductReference)value; }
        }
        //
        bool IStrategy.mainProductReferenceSpecified
        {
            get { return false; }
            set { }
        }
        IReference IStrategy.mainProductReference
        {
            get { return null; }
            set { ; }
        }
        //
        IProduct IStrategy.this[int pIndex] { get { return (IProduct)this.Item[pIndex]; } }
        #endregion IStrategy Members
        //       		
        #region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		#endregion IProduct Members
	}
	#endregion Strategy

	#region Trade
    public partial class Trade : IEFS_Array, ITrade, IProduct
    {
        #region Accessors
        #region AdjustedTradeDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedTradeDate
        {
            get
            {
                return tradeHeader.AdjustedTradeDate;

            }
        }
        #endregion AdjustedTradeDate
        #region TradeDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TradeDate
        {
            get
            {
                return tradeHeader.TradeDate;

            }
        }
        #endregion TradeDate
        #endregion Accessors
        
        
        
        
        #region Constructors
        public Trade()
        {
            tradeHeader = new TradeHeader();
            calculationAgent = new CalculationAgent();
            calculationAgentBusinessCenter = new BusinessCenter();
            documentation = new Documentation();
            governingLaw = new GoverningLaw();
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
        ITradeHeader ITrade.tradeHeader
        {
            get { return (ITradeHeader)this.tradeHeader; }
            set { this.tradeHeader = (TradeHeader)value; }
        }
        IProduct ITrade.product
        {
            get { return (IProduct)this.product; }
            set { this.product = (Product)value; }  
        }
        bool ITrade.otherPartyPaymentSpecified
        {
            set { this.otherPartyPaymentSpecified = value; }
            get { return this.otherPartyPaymentSpecified; }
        }
        // EG 20101020 Ticket:17185
        IPayment[] ITrade.otherPartyPayment 
        {
            set { this.otherPartyPayment = (Payment[])value; }
            get { return this.otherPartyPayment; } 
        }
        EFS_Events ITrade.productEvents
        {
            set { this.product.efs_Events = value; }
            get { return this.product.efs_Events; }
        }
        bool ITrade.settlementInputSpecified
        {
            set { this.settlementInputSpecified = value; }
            get { return this.settlementInputSpecified; }
        }
        ISettlementInput[] ITrade.settlementInput { get { return this.settlementInput; } }
        bool ITrade.brokerPartyReferenceSpecified
        {
            set { this.brokerPartyReferenceSpecified = value; }
            get { return this.brokerPartyReferenceSpecified; }
        }
        // PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
        IReference[] ITrade.brokerPartyReference
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
        bool ITrade.nettingInformationInputSpecified
        {
            set { this.nettingInformationInputSpecified = value; }
            get { return this.nettingInformationInputSpecified; }
        }
        INettingInformationInput ITrade.nettingInformationInput
        {
            set { this.nettingInformationInput = (NettingInformationInput)value; }
            get { return this.nettingInformationInput; }
        }
        bool ITrade.calculationAgentSpecified
        {
            set { this.calculationAgentSpecified = value; }
            get { return this.calculationAgentSpecified; }
        }
        ICalculationAgent ITrade.calculationAgent
        {
            set { this.calculationAgent = (CalculationAgent)value; }
            get { return this.calculationAgent; }
        }
        DateTime ITrade.AdjustedTradeDate { get { return this.AdjustedTradeDate.DateValue; } }
        bool ITrade.governingLawSpecified
        {
            set { this.governingLawSpecified = value; }
            get { return this.governingLawSpecified; }
        }
        IScheme ITrade.governingLaw
        {
            set { this.governingLaw = (GoverningLaw)value; }
            get { return this.governingLaw; }
        }
        bool ITrade.tradeSideSpecified
        {
            set { this.tradeSideSpecified = value; }
            get { return this.tradeSideSpecified; }
        }
        // PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
        ITradeSide[] ITrade.tradeSide
        {
            set
            {
                tradeSide = null;
                if (ArrFunc.IsFilled(value))
                {
                    //tradeSide = new TradeSide[value.Length];
                    //value.CopyTo(tradeSide, 0);
                    this.tradeSide = value.Cast<TradeSide>().ToArray(); 
                }
            }
            get
            {
                return this.tradeSide;
            }
        }
        bool ITrade.documentationSpecified
        {
            get { return this.documentationSpecified; }
            set { this.documentationSpecified = value; }
        }
        IDocumentation ITrade.documentation
        {
            get { return this.documentation; }
            set { this.documentation = (Documentation)value; }
        }
        bool ITrade.tradeIntentionSpecified
        {
            get { return false; }
            set { }
        }
        ITradeIntention ITrade.tradeIntention
        {
            get { return null; }
            set { }
        }

        bool ITrade.tradeIdSpecified
        {
            get { return this.tradeIdSpecified; }
            set { this.tradeIdSpecified = value; }
        }
        /// <summary>
        /// Représente l'identifier du trade dans Spheres®
        /// <para>Renseigné dans le cadre de la messagerie</para>
        /// </summary>
        /// FI 20130627 [18745] add
        string ITrade.tradeId
        {
            get { return this.tradeId; }
            set { this.tradeId = value; }
        }


        IParty ITrade.CreateParty()
        {
            return new Party();
        }
        IDocumentation ITrade.CreateDocumentation()
        {
            return new Documentation();
        }
        ISettlementChain ITrade.CreateSettlementChain()
        {
            return new SettlementChain();
        }
        
        IIssiItemsRoutingActorsInfo ITrade.CreateIssiItemsRoutingActorsInfo(string pConnectionString, int pIdIssi, IssiItem[] pIssiItem)
        {
            return new IssiItemsRoutingActorsInfo(pConnectionString, pIdIssi, pIssiItem);
        }
        bool ITrade.extendsSpecified { get { return false; } set { } }
        ITradeExtends ITrade.extends { get { return null; } }
        // EG 20101020 Ticket:17185
        void ITrade.SetOtherPartyPayment(ArrayList pOtherPartyPayment)
        {
            otherPartyPayment = (Payment[])pOtherPartyPayment.ToArray(typeof(Payment));
        }

        #endregion ITrade Members
        #region IProduct Members
        object IProduct.product { get { return this.product; } }
        IProductBase IProduct.productBase { get { return this.product; } }
        //IProduct[] IProduct.ProductsStrategy { get { return (IProduct[])((Strategy)this.product).Item; } }
        #endregion IProduct Members
    }
	#endregion Trade
	#region TradeDate
    /// EG 20140702 Upd Interface
    /// EG 20171016 [23509] 
    public partial class TradeDate : ITradeDate
    {
        #region Accessors
        #region TimeStamp
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime TimeStamp
        {
            get
            {
                return DtFunc.AddTimeToDate(DateValue, new DtFunc().StringToDateTime(timeStamp));
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
        string ITradeDate.efs_id
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
        DateTime ITradeDate.DateTimeValue { get { return this.TimeStamp; } }
        string ITradeDate.timeStamp
        {
            set { this.timeStamp = value; }
            get { return this.timeStamp; }
        }
        DateTime ITradeDate.TimeStamp
        {
            set { this.timeStamp = value.ToShortTimeString(); }
            get { return this.TimeStamp; }
        }
        string ITradeDate.businessDate
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        // EG 20171016 [23509] Unused
        //DateTime ITradeDate.BusinessDate 
        //{ 
        //    get { return this.DateValue; }
        //    set { ;}
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
		}
		#endregion Constructors

		#region ITradeHeader Members
        // PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
        IPartyTradeIdentifier[] ITradeHeader.partyTradeIdentifier 
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
		bool ITradeHeader.partyTradeInformationSpecified 
		{ 
			set { this.partyTradeInformationSpecified = value; } 
			get { return this.partyTradeInformationSpecified; } 
		}
		// PL 20180618 from FpML.v44 of EG 20171016 [23509] Upd
        IPartyTradeInformation[] ITradeHeader.partyTradeInformation 
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
		ITradeDate ITradeHeader.tradeDate { get { return this.tradeDate; } }
		DateTime ITradeHeader.adjustedTradeDate { get { return this.AdjustedTradeDate.DateValue; } }
        /// EG 20171003 [23452]
        bool ITradeHeader.clearedDateSpecified
        {
            get { return false; }
            set { ; }
        }
        IAdjustedDate ITradeHeader.clearedDate
        {
            get { return null; }
            set { ; }
        }
        #endregion ITradeHeader Members
	}
    #endregion TradeHeader
    #region TradeId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class TradeId : IEFS_Array, IComparable, ISchemeId
	{
		#region Constructors
		public TradeId()
		{
			tradeIdScheme = string.Empty;
			Value = string.Empty;
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
		#region IComparable Members
		#region CompareTo
        public int CompareTo(object pObj)
		{
            TradeId objTradeId = pObj as TradeId;
            if (null != objTradeId)
			{
				int ret = 0;
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
		#region ISchemeId Members
		string ISchemeId.id 
		{
			set { this.id = value; }
			get { return this.id; }
		}
        /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.scheme
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
			partyReference = new PartyReference();
			tradeId = new TradeId[1] { new TradeId() };
		}
		#endregion Constructors
		#region Methods
        #region GetTradeIdFromScheme
        public TradeId GetTradeIdFromScheme(string pScheme)
		{
			TradeId ret = null;
			if (ArrFunc.IsFilled(tradeId))
			{
				for (int i = 0; i < tradeId.Length; i++)
				{
					if (tradeId[i].tradeIdScheme == pScheme)
					{
						ret = tradeId[i];
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
			if (ArrFunc.IsFilled(tradeId))
			{
				for (int i = 0; i < tradeId.Length; i++)
				{
					if (StrFunc.IsEmpty(tradeId[i].tradeIdScheme))
					{
						ret = tradeId[i];
						break;
					}
				}
			}
			return ret;
		}
		#endregion GetTradeIdWithNoScheme
		#region RemoveTradeIdFromScheme
		public void RemoveTradeIdFromScheme(string pScheme)
		{
			if (ArrFunc.IsFilled(tradeId))
			{
				for (int i = tradeId.Length - 1; -1 < i; i--)
				{
					if (tradeId[i].tradeIdScheme == pScheme)
						ReflectionTools.RemoveItemInArray(this, "tradeId", i);
				}
			}
		}
		#endregion RemoveTradeIdFromScheme
		#endregion Methods

		#region ITradeIdentifier Members
		string ITradeIdentifier.GetTradeIdMemberName() { return "tradeId"; }
        ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		void ITradeIdentifier.RemoveTradeIdFromScheme(string pScheme) { this.RemoveTradeIdFromScheme(pScheme); }
		#endregion ITradeIdentifier Members

	}
	#endregion TradeIdentifier
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
		#region ITrader Members
		string ITrader.identifier 
		{
			set { this.Value = value;} 
			get { return this.Value; } 
		}
		decimal ITrader.Factor
		{
			get { return this.Factor.DecValue; }
			set { this.Factor = new EFS_Decimal(value); }
		}
        string ITrader.factor
        {
            set { this.factor = value; }
            get { return this.factor; }
        }
        string ITrader.name
		{
			set { this.name = value; }
			get { return this.name; }
		}
		string ITrader.otcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
		int ITrader.OTCmlId
		{
			set { this.OTCmlId = value; }
			get { return this.OTCmlId; }
		}
        string ITrader.scheme
        {
            set { this.traderScheme = value; }
            get { return this.traderScheme; }
        }
		#endregion ITrader Members
	}
	#endregion Trader

    #region TradeSide
    public partial class TradeSide : ITradeSide , IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id
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

        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray

        #region ITradeSide Membres
        bool ITradeSide.confirmerSpecified
        {
            get { return this.confirmerSpecified; }
            set { this.confirmerSpecified = value; }
        }
        IPartyRole ITradeSide.confirmer
        {
            get { return (IPartyRole)this.confirmer; }
            set { this.confirmer = (PartyRole)value; }
        }
        bool ITradeSide.settlerSpecified
        {
            get { return this.settlerSpecified; }
            set { this.settlerSpecified = value; }
        }
        IPartyRole ITradeSide.settler
        {
            get { return (IPartyRole)this.settler; }
            set { this.settler = (PartyRole)value; }
        }
        IPartyRole ITradeSide.creditor
        {
            get { return (IPartyRole)this.creditor; }
            set { this.creditor = (PartyRole)value; }
        }
        #endregion
    }
    #endregion TradeSide

}
