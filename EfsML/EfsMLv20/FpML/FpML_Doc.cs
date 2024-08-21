#region Using Directives
using System;
using System.Reflection;
using System.Threading;
using System.Globalization;
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

using EfsML.v20;

using FixML.v44;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Cd;
using FpML.v42.Ird;
using FpML.v42.Fx;
using FpML.v42.Eqd;
using FpML.v42.Eqs;
using FpML.v42.Msg;
using FpML.v42.Shared;
#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision
namespace FpML.v42.Doc
{
    #region Account
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Account
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("accountId",Order = 1)]
        public AccountId[] accountId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool accountNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountName",Order = 2)]
        public string[] accountName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool accountBeneficiarySpecified;
		[System.Xml.Serialization.XmlElementAttribute("accountBeneficiary", Order = 3)]
        public PartyReference accountBeneficiary;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public EFS_Id efs_id;
		#endregion Members
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
    }
    #endregion Account
    #region AccountId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AccountId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string accountIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion AccountId
    #region AccountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AccountReference : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }
    #endregion AccountReference
    #region Allocation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Allocation
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("allocationTradeId", Order = 1)]
        public PartyTradeIdentifier allocationTradeId;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Reference")]
		public EFS_RadioChoice reference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool referencePartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partyReference", typeof(PartyReference), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public PartyReference referencePartyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool referenceAccountReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("accountReference", typeof(AccountReference), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public AccountReference referenceAccountReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Allocated")]
		public EFS_RadioChoice allocated;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool allocatedNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("allocatedNotional", typeof(Money), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money allocatedNotional;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool allocatedFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("allocatedFraction", typeof(EFS_Decimal), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal allocatedFraction;

		[System.Xml.Serialization.XmlElementAttribute("collateral", Order = 6)]
        public Collateral collateral;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool creditChargeAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("creditChargeAmount", Order = 7)]
        public Money creditChargeAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool approvalsSpecified;
        [System.Xml.Serialization.XmlArrayItemAttribute("approval", IsNullable = false)]
        public Approval[] approvals;
		#endregion Members
		#region Constructors
		public Allocation()
		{
			referencePartyReference		= new PartyReference();
			referenceAccountReference	= new AccountReference();
			allocatedFraction			= new EFS_Decimal();
			allocatedNotional			= new Money();
		}
		#endregion Constructors
	}
    #endregion Allocation
    #region AllocationTradeIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AllocationTradeIdentifier : PartyTradeIdentifier
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("blockTradeId", Order = 1)]
        public PartyTradeIdentifier blockTradeId;
		#endregion Members
	}
    #endregion AllocationTradeIdentifier
    #region Approval
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Approval : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("type",Order=1)]
        public EFS_String type;
        [System.Xml.Serialization.XmlElementAttribute("status",Order=2)]
        public EFS_String status;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool approverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("approver",Order=3)]
        public EFS_String approver;
		#endregion Members
	}
    #endregion Approval

    #region BlockTradeIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class BlockTradeIdentifier : PartyTradeIdentifier
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("allocationTradeId",Order = 1)]
        public PartyTradeIdentifier[] allocationTradeId;
		[System.Xml.Serialization.XmlElementAttribute("blockTradeId", Order = 2)]
        public PartyTradeIdentifier blockTradeId;
		#endregion Members
	}
    #endregion BlockTradeIdentifier

    #region Collateral
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Collateral
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("independentAmount", Order = 1)]
        public IndependentAmount independentAmount;
		#endregion Members
	}
    #endregion Collateral

    #region DataDocument
    /// <summary><newpara></newpara>
    /// <newpara><b>Description :</b> A type defining a content model that is backwards compatible with older 
    /// FpML releases and which can be used to contain sets of data without expressing any processing intention.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type Document)</newpara>
    /// <newpara><b>• The abstract base type from which all FpML compliant messages and documents must be derived.</b></newpara>
    /// <newpara>validation (zero or more occurrences; of the type Validation)</newpara> 
    /// <newpara>trade (zero or more occurences; of the type Trade). The root element in an FpML trade document.</newpara>
    /// <newpara>portfolio (zero or more occurences; of the type Portfolio).
    /// An arbitary grouping of trade references (and possibly other portfolios).</newpara>
    /// <newpara>party (zero or more occurences; of the type Party). The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    /// <remarks>
    /// <newpara><b>Used by :</b></newpara>
    /// <newpara><b>Derived Types :</b></newpara>
    /// <newpara><b>Substituted by :</b></newpara>
    /// </remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("FpML", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class DataDocument : Document
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("validation", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        public Validation[] validation;
		[System.Xml.Serialization.XmlElementAttribute("trade",Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade", IsMaster = true, IsMasterVisible = true, MinItem = 1, MaxItem = 1)]
        public Trade[] trade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool portfolioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("portfolio", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        public Portfolio[] portfolio;
        [System.Xml.Serialization.XmlElementAttribute("party",Order=4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Parties", IsMaster = true, MinItem = 2)]
        public Party[] party;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeSide", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "TradeSide")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "TradeSide", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public TradeSide[] tradeSide;
        #endregion Members
    }
    #endregion DataDocument

    #region IndependentAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndependentAmount
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        public PartyReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("paymentDetail", Order = 3)]
        public PaymentDetail[] paymentDetail;
		#endregion Members
	}
    #endregion IndependentAmount

    #region LinkId
    /// <summary>
    /// <newpara><b>Description :</b> The data type used for link identifiers.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by 
    /// the type xsd:string)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: PartyTradeIdentifier</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class LinkId
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string linkIdScheme
		{
			set { LinkIdScheme = new EFS_String(value); }
			get
			{
				if (LinkIdScheme == null)
					return null;
				else
					return LinkIdScheme.Value;
			}
		}
		[System.Xml.Serialization.XmlTextAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string Value
		{
			set { Identifier = new EFS_String(value); }
			get
			{
				if (Identifier == null)
					return null;
				else
					return Identifier.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string factor
		{
			set	{Factor = new EFS_Decimal(value);}
			get
			{
				if (null == Factor)
					return null;
				return Factor.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
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
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "Scheme", Width = 200)]
		public EFS_String LinkIdScheme;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Name = "value", Width = 150)]
		public EFS_String Identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "factor", Width = 100)]
		public EFS_Decimal Factor;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Id efs_id;

		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string type = "efs:LinkId";
		#endregion Members
    }
    #endregion LinkId

    #region NotionalAmountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class NotionalAmountReference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion NotionalAmountReference

    #region Party
    /// <summary><newpara></newpara>
    /// <newpara><b>Description :</b> A type defining party identifier information.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyId (exactly one occurence; with locally defined content) A party identifier, e.g. a S.W.I.F.T. bank identifier
    /// code (BIC).</newpara>
    /// <newpara>partyName (zero or one occurence; of the type xsd:string) The name of the party. A free format string. FpML	
    /// does not define usage rules for this element.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: AcceptQuote</newpara>
    ///<newpara>• Complex type: CancelTradeConfirmation</newpara>
    ///<newpara>• Complex type: CancelTradeMatch</newpara>
    ///<newpara>• Complex type: ConfirmationCancelled</newpara>
    ///<newpara>• Complex type: ConfirmTrade</newpara>
    ///<newpara>• Complex type: DataDocument</newpara>
    ///<newpara>• Complex type: ModifyTradeConfirmation</newpara>
    ///<newpara>• Complex type: ModifyTradeMatch</newpara>
    ///<newpara>• Complex type: QuoteAcceptanceConfirmed</newpara>
    ///<newpara>• Complex type: QuoteUpdated</newpara>
    ///<newpara>• Complex type: RequestQuote</newpara>
    ///<newpara>• Complex type: RequestQuoteResponse</newpara>
    ///<newpara>• Complex type: RequestTradeConfirmation</newpara>
    ///<newpara>• Complex type: RequestTradeMatch</newpara>
    ///<newpara>• Complex type: RequestTradeStatus</newpara>
    ///<newpara>• Complex type: TradeAffirmation</newpara>
    ///<newpara>• Complex type: TradeAffirmed</newpara>
    ///<newpara>• Complex type: TradeAlleged</newpara>
    ///<newpara>• Complex type: TradeAlreadyMatched</newpara>
    ///<newpara>• Complex type: TradeAlreadySubmitted</newpara>
    ///<newpara>• Complex type: TradeAmended</newpara>
    ///<newpara>• Complex type: TradeConfirmed</newpara>
    ///<newpara>• Complex type: TradeCreated</newpara>
    ///<newpara>• Complex type: TradeMatched</newpara>
    ///<newpara>• Complex type: TradeMismatched</newpara>
    ///<newpara>• Complex type: TradeNotFound</newpara>
    ///<newpara>• Complex type: TradeStatus</newpara>
    ///<newpara>• Complex type: TradeUnmatched</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class Party 
	{
		#region Members
        // PL 20180618 Harmonisation avec code FpML.v4.4
        [System.Xml.Serialization.XmlElementAttribute("partyId", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]
        [ControlGUI(IsLabel = true, Name = "Party", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Id", MinItem = 1)]
        public PartyId[] partyId;
        //public PartyId partyId;
        [System.Xml.Serialization.XmlElementAttribute("partyName", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
        public string partyName;
        [System.Xml.Serialization.XmlElementAttribute("account", Order = 3)]
        public Account[] account;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Party";
		#endregion Members
    }
    #endregion Party
    #region PartyId
    /// <summary>
    /// <newpara><b>Description :</b> The data type used for party identifiers.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type 
    /// xsd:normalizedString)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: MessageHeader</newpara>
    ///<newpara>• Complex type: NotificationMessageHeader</newpara>
    ///<newpara>• Complex type: Party</newpara>
    ///<newpara>• Complex type: RequestMessageHeader</newpara>
    ///<newpara>• Complex type: ResponseMessageHeader</newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PartyId : PartyIdGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string partyIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion PartyId
    #region PartyPortFolioName
    /// <summary>
    /// <newpara><b>Description :</b> A type to represent a portfolio name for a particular party.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyReference (exactly one occurence; with locally defined content) A pointer style reference to a party</newpara>
    /// <newpara>identifier defined elsewhere in the document. The party referenced has allocated the trade identifier.</newpara>
    /// <newpara>portfolioName (one or more occurences; with locally defined content) The name of a portfolio.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Portfolio</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PartyPortfolioName
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlElementAttribute("portfolioName", Order = 2)]
        public PortFolioName[] portfolioName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PartyPortFolio)]
        public EFS_Id efs_id;
		#endregion Members
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
    }
    #endregion PartyPortFolioName
    #region PartyRole
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PartyRole : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference to")]
        public EFS_RadioChoice partyRole;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyRoleAccountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("account", typeof(AccountReference),Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference partyRoleAccount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyRolePartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("party", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference partyRoleParty;
		#endregion Members
		
        #region Constructors
		public PartyRole()
        {
            partyRoleAccount = new AccountReference();
            partyRoleParty = new PartyReference();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion PartyRole
    #region PartyTradeIdentifier
    /// <summary>
    /// <newpara><b>Description :</b> A type defining one or more trade identifiers allocated to the trade by a party. 
    /// A link identifier allows the trade to be associated with other related trades, e.g. trades forming part of 
    /// a larger structured transaction. It is expected that for external communication of trade there will be only one 
    /// tradeId sent in the document per party.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type 
    /// TradeIdentifier)</newpara>
    /// <newpara>• A type defining a trade identifier issued by the indicated party.</newpara>
    /// <newpara>linkId (zero or more occurences; with locally defined content)A link identifier allowing the trade to be
    /// associated with other related trades, e.g. the linkId may contain a tradeId for an associated trade or several
    /// related trades may be given the same linkId. FpML does not define the domain values associated with this
    /// element. Note that the domain values for this element are not strictly an enumerated list.</newpara>
    ///</summary>
    ///<remarks>
    /// <newpara><b>Used by :</b></newpara>
    /// <newpara>• Complex type: CancelTradeConfirmation</newpara>
    /// <newpara>• Complex type: CancelTradeMatch</newpara>
    /// <newpara>• Complex type: ConfirmTrade</newpara>
    /// <newpara>• Complex type: TradeHeader</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class PartyTradeIdentifier : TradeIdentifier
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool linkIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("linkId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Link Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Link Id", IsClonable = true)]
        public LinkId[] linkId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bookIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bookId", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Book Id")]
        public BookId bookId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localClassDerv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocalClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public LocalClassDerv localClassDerv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iasClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("iasClassDerv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "IASClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IASClassDerv iasClassDerv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeClassDerv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeClassDerv hedgeClassDerv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxClassSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxClass", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FxClass")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public FxClass fxClass;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localClassNDrv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocalClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public LocalClassNDrv localClassNDrv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iasClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("iasClassNDrv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "IASClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IASClassNDrv iasClassNDrv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeClassNDrv", Namespace = "http://www.efs.org/2005/EFSmL-2-0",Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeClassNDrv hedgeClassNDrv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeFolderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeFolder", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeFolder")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeFolder hedgeFolder;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeFactor", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeFactor")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeFactor hedgeFactor;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:PartyTradeIdentifier";
		#endregion Members
    }
    #endregion PartyTradeIdentifier
    #region PartyTradeInformation
    /// <summary>
    /// <newpara><b>Description :</b></newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyReference (exactly one occurrence; of the type PartyReference).</newpara>
    /// <newpara>trader (zero or more occurrences; of the type xsd:normalizedString)</newpara>
    ///</summary>
    ///<remarks>
    /// <newpara><b>Used by :</b></newpara>
    /// <newpara>• Complex type: TradeHeader</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class PartyTradeInformation
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]
        [ControlGUI(Name = "Party", LblWidth = 50)]
        public PartyReference partyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool traderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("trader", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trader", IsClonable = true, IsMaster = true)]
        public Trader[] trader;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool salesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sales", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 3)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sales", IsClonable = true, IsMaster = true)]
        public Trader[] sales;
        //
        //20080930 FI 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool brokerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("brokerPartyReference", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Broker Party Reference")]
        public ArrayPartyReference[] brokerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool executionDateTimeSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1
        [System.Xml.Serialization.XmlElementAttribute("executionDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-2", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution date")]
        public FpML.v44.Doc.ExecutionDateTime executionDateTime;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timestampsSpecified;
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190405 [XXXXX] BANCAPERTA 8.1
        [System.Xml.Serialization.XmlElementAttribute("timestamps", Namespace = "http://www.efs.org/2007/EFSmL-2-0", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Timestamps")]
        public EfsML.v30.MiFIDII_Extended.TradeProcessingTimestamps timestamps;


        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
        Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:PartyTradeInformation";
		#endregion Members
    }
    #endregion PartyTradeInformation
    #region PaymentDetail
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PaymentDetail
    {
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Payment date")]
		public EFS_RadioChoice paymentDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDateAdjustedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", typeof(EFS_Date), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Date paymentDateAdjusted;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDateAdjustableSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", typeof(AdjustableDate2), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public AdjustableDate2 paymentDateAdjustable;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Payment")]
		public EFS_RadioChoice payment;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentRuleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentRule", typeof(PercentageRule), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public PercentageRule paymentRule;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", typeof(AdjustableDate2), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money paymentAmount;
		#endregion Members
		#region Constructors
		public PaymentDetail()
		{
			paymentDateAdjusted		= new EFS_Date();
			paymentDateAdjustable	= new AdjustableDate2();
			paymentRule				= new PercentageRule();
			paymentAmount			= new Money();
		}
		#endregion Constructors
	}
    #endregion PaymentDetail
    #region PaymentRule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PercentageRule))]
    public abstract class PaymentRule
    {
    }
    #endregion PaymentRule
    #region PercentageRule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class PercentageRule : PaymentRule
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("paymentPercent", Order = 1)]
		public EFS_Decimal paymentPercent;
		[System.Xml.Serialization.XmlElementAttribute("notionalAmountReference", Order = 2)]
        public NotionalAmountReference notionalAmountReference;
		#endregion Members
	}
    #endregion PercentageRule
    #region PortFolio
    /// <summary>
    /// <newpara><b>Description :</b> A type representing an arbitary grouping of trade references.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyPortfolioName (zero or one occurence; of the type PartyPortfolioName) The name of the portfolio
    /// together with the party that gave the name.</newpara>
    /// <newpara>tradeId (zero or more occurrences; of the type TradeId)</newpara>
    /// <newpara>portfolio (zero or more occurences; of the type Portfolio) An arbitary grouping of trade references (and
    /// possibly other portfolios).</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: DataDocument</newpara>
    ///<newpara>• Complex type: Portfolio</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Portfolio
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyPortfolioName", Order = 1)]
        public PartyPortfolioName partyPortfolioName;
        [System.Xml.Serialization.XmlElementAttribute("tradeId",Order = 2)]
        public TradeId[] tradeId;
        [System.Xml.Serialization.XmlElementAttribute("portfolio", Order = 3)]
        public Portfolio[] portfolio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PortFolio)]
        public EFS_Id efs_id;
		#endregion Members
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
    }
    #endregion PortFolio
    #region PortFolioName
    /// <summary>
    /// <newpara><b>Description :</b> The data type used for portfolio names.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: PartyPortfolioName</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PortFolioName
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PortFolio)]
        public EFS_Id efs_id;
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
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string portfolioNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion PortFolioName

    #region QueryParameter
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QueryParameter
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("queryParameterId",Order = 1)]
		public QueryParameterId queryParameterId;
		[System.Xml.Serialization.XmlElementAttribute("queryParameterValue", Order = 2)]
        public EFS_String queryParameterValue;
		[System.Xml.Serialization.XmlElementAttribute("queryParameterOperator", Order = 3)]
        public QueryParameterOperator queryParameterOperator;
		#endregion Members
	}
    #endregion QueryParameter
    #region QueryParameterId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QueryParameterId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string queryParameterIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.QueryParameterId)]
        public EFS_Id efs_id;
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
    }
    #endregion QueryParameterId
    #region QueryParameterOperator
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QueryParameterOperator
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string queryParameterOperatorScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.QueryParameterOperator)]
        public EFS_Id efs_id;
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

        public QueryParameterOperator()
        {
            queryParameterOperatorScheme = "http://www.fpml.org/coding-scheme/query-parameter-operator-1-0";
        }
    }
    #endregion QueryParameterOperator
    #region QueryPortfolio
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QueryPortfolio : Portfolio
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("queryParameter", Order = 1)]
        public QueryParameter[] queryParameter;
		#endregion Members
	}
    #endregion QueryPortfolio

    #region Strategy
    /// <summary>
    /// <newpara><b>Description :</b> A type defining a group of products making up a single trade.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
    /// <newpara> • The base type which all FpML products extend.</newpara>
    /// <newpara>premiumProductReference (zero or one occurence; with locally defined content)</newpara>
    /// <newpara>product (one or more occurences; of the type Product) An abstract element used as a place holder for the
    /// substituting product elements.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Element: strategy</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("strategy", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class Strategy : Product
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumProductReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premiumProductReference", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Premium product reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public ProductReference premiumProductReference;
		[System.Xml.Serialization.XmlElementAttribute("brokerEquityOption", typeof(BrokerEquityOption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(BulletPayment), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(CapFloor), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", typeof(CreditDefaultSwap), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("equityForward", typeof(EquityForward), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EquityOption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("equitySwap", typeof(EquitySwap), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fra", typeof(Fra), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(FxAverageRateOption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(FxBarrierOption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(FxDigitalOption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(FxOptionLeg), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(FxLeg), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(FxSwap), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("strategy", typeof(Strategy), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("swap", typeof(Swap), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("swaption", typeof(Swaption), Order = 2)]
		[System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(TermDeposit), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("FIXML", typeof(EfsML.v20.FIXML), Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order=2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product", IsClonable = false, IsChild = true, IsVariableArray = false, IsProduct = true)]
        public Product[] Item;
		#endregion Members
    }
    #endregion Strategy

    #region Trade
    /// <summary>
    /// <newpara><b>Description :</b> A type definiting an FpML trade.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>tradeHeader (exactly one occurence; of the type TradeHeader) The information on the trade which is not
    /// product specific, e.g. trade date.</newpara>
    /// <newpara>product (exactly one occurence; of the type Product) An abstract element used as a place holder for the
    /// substituting product elements.</newpara>
    /// <newpara>otherPartyPayment (zero or more occurences; of the type Payment) Other fees or additional payments
    /// associated with the trade, e.g. broker commissions, where one or more of the parties involved are not principal
    /// parties involved in the trade.</newpara>
    /// <newpara>calculationAgent (zero or one occurence; of the type CalculationAgent) The ISDA Calculation Agent
    /// responsible for performing duties associated with an optional early termination.</newpara>
    /// <newpara>calculationAgentBusinessCenter (zero or one occurrence; of the type BusinessCenter) The city in which the
    /// office through which ISDA Calculation Agent is acting for purposes of the transaction is located.</newpara>
    /// <newpara>documentation (zero or one occurence; of the type Documentation)Defines the definitions that govern the
    /// document and should include the year and type of definitions referenced, along with any relevant
    /// documentation (such as master agreement) and the date it was signed.</newpara>
    /// <newpara>governingLaw (zero or one occurence; with locally defined content) TBA</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: AcceptQuote</newpara>
    ///<newpara>• Complex type: DataDocument</newpara>
    ///<newpara>• Complex type: ModifyTradeConfirmation</newpara>
    ///<newpara>• Complex type: ModifyTradeMatch</newpara>
    ///<newpara>• Complex type: QuoteAcceptanceConfirmed</newpara>
    ///<newpara>• Complex type: RequestTradeConfirmation</newpara>
    ///<newpara>• Complex type: RequestTradeMatch</newpara>
    ///<newpara>• Complex type: TradeAffirmation</newpara>
    ///<newpara>• Complex type: TradeAmended</newpara>
    ///<newpara>• Complex type: TradeConfirmed</newpara>
    ///<newpara>• Complex type: TradeCreated</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class Trade
	{
		#region Members
		[System.Xml.Serialization.XmlElement("tradeHeader", typeof(TradeHeader), Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade Header", IsVisible = false)]
        public TradeHeader tradeHeader;
		[System.Xml.Serialization.XmlElement("brokerEquityOption", typeof(BrokerEquityOption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("bulletPayment", typeof(BulletPayment), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("capFloor", typeof(CapFloor), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("creditDefaultSwap", typeof(CreditDefaultSwap), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("equityForward", typeof(EquityForward), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("equityOption", typeof(EquityOption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("equitySwap", typeof(EquitySwap), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fra", typeof(Fra), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxAverageRateOption", typeof(FxAverageRateOption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxBarrierOption", typeof(FxBarrierOption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxDigitalOption", typeof(FxDigitalOption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxSingleLeg", typeof(FxLeg), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxSimpleOption", typeof(FxOptionLeg), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("fxSwap", typeof(FxSwap), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("swap", typeof(Swap), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("swaption", typeof(Swaption), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("strategy", typeof(Strategy), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("termDeposit", typeof(TermDeposit), Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[System.Xml.Serialization.XmlElement("FIXML", typeof(EfsML.v20.FIXML), Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade Header")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product", IsVisible = false, IsGroup = false, IsProduct = true)]
        [BookMarkGUI(IsVisible = true)]
        [DictionaryGUI(Page = "ExecutionReport")]
        public Product product;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool otherPartyPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("otherPartyPayment", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Other Party Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other Party Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] otherPartyPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool brokerPartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("brokerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Broker Party Reference")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Broker Party Reference", MinItem = 0)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public ArrayPartyReference[] brokerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationAgent", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent")]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentBusinessCenterSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationAgentBusinessCenter", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent Business Center")]
        public BusinessCenter calculationAgentBusinessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("documentation", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Documentation")]
        public Documentation documentation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool governingLawSpecified;
		[System.Xml.Serialization.XmlElementAttribute("governingLaw", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Governing Law")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public GoverningLaw governingLaw;
        /*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
        public bool allocationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allocations", IsNullable=false,Namespace="http://www.fpml.org/2005/FpML-4-2")]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level=MethodsGUI.LevelEnum.First,Name="Allocations")]
        [ArrayDivGUI(Level=MethodsGUI.LevelEnum.Intermediary,Name="Allocation",MinItem=0)]
        public Allocation[] allocations;
        */
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
        public bool tradeSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeSide", Namespace="http://www.fpml.org/2005/FpML-4-2", Order = 9)]  
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level=MethodsGUI.LevelEnum.First,Name="Trade Side")]
        [ArrayDivGUI(Level=MethodsGUI.LevelEnum.Intermediary,Name="Trade Side",IsClonable=true,IsChild=true)]
        [BookMarkGUI(IsVisible=false)]
        public TradeSide[] tradeSide;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInputSpecified;

		[System.Xml.Serialization.XmlElementAttribute("settlementInput", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Settlement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public SettlementInput[] settlementInput;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nettingInformationInputSpecified;

		[System.Xml.Serialization.XmlElementAttribute("nettingInformationInput", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Netting")]
        public NettingInformationInput nettingInformationInput;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Trade)]
        public EFS_Id efs_id;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Trade";

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean tradeIdSpecified;
        /// <summary>
        /// Représente l'identifier du trade dans Spheres®
        /// <para>Utiliser notamment par la messagerie où l'élément tradeHeader peut-être à null (gain de place)</para>
        /// </summary>
        /// FI 20130627 [18745] add
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0", DataType = "normalizedString")]
        public string tradeId;


		#endregion Members
		#region id
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
		#endregion id
    }
    #endregion Trade
    #region TradeDate
    /// <summary>
    /// <newpara><b>Description :</b> The type used to define the trade date for a transaction.</newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type xsd:date)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: TradeHeader</newpara>
    ///<newpara><b>Substituted Types : RequiredIdentifierDate</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class TradeDate : IdentifierDate
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string timeStamp
        {
            set { efs_timeStamp = new EFS_Time(value); }
            get { return (null != efs_timeStamp) ? efs_timeStamp.Value : null; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "TimeStamp")]
        public EFS_Time efs_timeStamp;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:TradeDate";
		#endregion Members
    }
    #endregion TradeDate
    #region TradeHeader
    /// <summary><newpara></newpara>
    /// <newpara><b>Description :</b> A type defining trade related information which is not product specific.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyTradeIdentifier (one or more occurences; of the type PartyTradeIdentifier) The trade reference
    /// identifier(s) allocated to the trade by the parties involved.</newpara>
    /// <newpara>tradeDate (exactly one occurence; of the type TradeDate) The trade date.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Trade</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class TradeHeader
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Identifier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Identifier", IsClonable = true, IsMaster = true, IsChild = true)]
        public PartyTradeIdentifier[] partyTradeIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyTradeInformationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partyTradeInformation", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Information")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Information", IsClonable = true, IsChild = true)]
        public PartyTradeInformation[] partyTradeInformation;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date", IsVisible = true)]
		[System.Xml.Serialization.XmlElementAttribute("tradeDate", Order = 3)]
        public TradeDate tradeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date")]
        public bool FillBalise;
		#endregion Members
    }
    #endregion TradeHeader
    #region Trader
    /// <summary>
    /// <newpara><b>Description :</b> The data type used for trader identifiers.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: PartyTradeInformation</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class Trader
	{
		#region Members
        [System.Xml.Serialization.XmlTextAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string Value
		{
			set { Identifier = new EFS_String(value); }
			get
			{
				if (Identifier == null)
					return null;
				else
					return Identifier.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string name
		{
			set { Name = new EFS_String(value); }
			get
			{
				if (Name == null)
					return null;
				else
					return Name.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string factor
		{
			set { Factor = new EFS_Decimal(value); }
			get
			{
				if (null == Factor)
					return null;
				return Factor.Value;
			}
		}
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Name = "Identifier", Width = 150)]
		public EFS_String Identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "name", Width = 200)]
		public EFS_String Name;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "factor", Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Decimal Factor;
		[System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute("traderScheme", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string traderScheme;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string type = "efs:Trader";
		#endregion Members
    }
    #endregion Trader
    #region TradeId
    /// <summary><newpara></newpara>
    /// <newpara><b>Description :</b> A trade reference identifier allocated by a party. 
    /// FpML does not define the domain values associated with this
    /// element. Note that the domain values for this element are not strictly an enumerated list.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Portfolio</newpara>
    ///<newpara>• Complex type: TradeIdentifier</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class TradeId
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue tradeId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "Scheme", Width = 400)]
        public string tradeIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Id", Width = 50)]
        public string id;
		#endregion Members
	}
    #endregion TradeId
    #region TradeIdentifier
    /// <summary>
    /// <newpara><b>Description :</b> A type defining a trade identifier issued by the indicated party.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partyReference (exactly one occurence; with locally defined content) A pointer style reference to a party
    /// identifier defined elsewhere in the document. The party referenced has allocated the trade identifier.</newpara>
    /// <newpara>tradeId (one or more occurrences; of the type TradeId)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: PartyTradeIdentifier</newpara>
    ///<newpara>• Complex type: BestFitTrade</newpara>
    ///<newpara>• Complex type: ConfirmationCancelled</newpara>
    ///<newpara>• Complex type: RequestTradeStatus</newpara>
    ///<newpara>• Complex type: TradeAffirmed</newpara>
    ///<newpara>• Complex type: TradeAlleged</newpara>
    ///<newpara>• Complex type: TradeAlreadyMatched</newpara>
    ///<newpara>• Complex type: TradeAlreadySubmitted</newpara>
    ///<newpara>• Complex type: TradeMatched</newpara>
    ///<newpara>• Complex type: TradeMismatched</newpara>
    ///<newpara>• Complex type: TradeNotFound</newpara>
    ///<newpara>• Complex type: TradeStatusItem</newpara>
    ///<newpara>• Complex type: TradeUnmatched</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///<newpara>• Complex type: PartyTradeIdentifier</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationTradeIdentifier))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BlockTradeIdentifier))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartyTradeIdentifier))]
    public partial class TradeIdentifier
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyReference",Order = 1)]
		[ControlGUI(Name = "party", LblWidth = 50)]
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlElementAttribute("tradeId",Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Id", IsClonable = true, IsMaster = true)]
        public TradeId[] tradeId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
		#endregion Members
	}
    #endregion TradeIdentifier
    #region TradeSide
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class TradeSide 
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ordererSpecified;
		[System.Xml.Serialization.XmlElementAttribute("orderer", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Orderer")]
        public PartyRole orderer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool introducerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("introducer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Introducer")]
        public PartyRole introducer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool executorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("executor", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Executor")]
        public PartyRole executor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool confirmerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("confirmer", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Confirmer")]
        public PartyRole confirmer;
		[System.Xml.Serialization.XmlElementAttribute("creditor", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Creditor", IsVisible = true)]
        public PartyRole creditor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculaterSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculater", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Creditor")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculater")]
        public PartyRole calculater;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settler", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settler")]
        public PartyRole settler;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiarySpecified;
		[System.Xml.Serialization.XmlElementAttribute("beneficiary", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary")]
        public PartyRole beneficiary;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountantSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountant",Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accountant")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accountant", IsClonable = true, IsChild = true)]
        public PartyRole[] accountant;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.TradeSide)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion TradeSide

    #region Validation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Validation
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string validationScheme;
    }
    #endregion Validation
}
