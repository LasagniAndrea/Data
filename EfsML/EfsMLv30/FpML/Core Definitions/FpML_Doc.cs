#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.v30.CashBalance;
using EfsML.v30.CashBalanceInterest;
using EfsML.v30.CommodityDerivative;
using EfsML.v30.Doc;
using EfsML.v30.Fix;
using EfsML.v30.FuturesAndOptions;
using EfsML.v30.Invoice;
using EfsML.v30.LoanDeposit;
using EfsML.v30.MarginRequirement;
using EfsML.v30.MiFIDII_Extended;
using EfsML.v30.Security;
using EfsML.v30.Security.Shared;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.v44.Cd;
using FpML.v44.CorrelationSwaps;
using FpML.v44.CreditEvent.Notification.ToDefine;
using FpML.v44.DividendSwaps;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Enum;
using FpML.v44.Eq.Shared;
using FpML.v44.Eqd;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Option.Shared;
using FpML.v44.PostTrade.ToDefine;
using FpML.v44.ReturnSwaps;
using FpML.v44.Shared;
using FpML.v44.VarianceSwaps;
using System;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Doc
{
    #region AllocationTradeIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AllocationTradeIdentifier : PartyTradeIdentifier
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool blockTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("blockTradeId", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Block trade Id")]
        public PartyTradeIdentifier blockTradeId;
        #endregion Members
    }
    #endregion AllocationTradeIdentifier

    #region BlockTradeIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BlockTradeIdentifier : PartyTradeIdentifier
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocationTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allocationTradeId", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Allocated trade Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocated trade Id", IsClonable = true, IsChild = true)]
        public PartyTradeIdentifier[] allocationTradeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool blockTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("blockTradeId", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Block trade Id")]
        public PartyTradeIdentifier blockTradeId;
        #endregion Members
    }
    #endregion BlockTradeIdentifier

    #region Contract
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //FI 20130701 [18745] Add EfsML.v30.Notification.CashBalanceReport
    // EG 20140702 New build FpML4.4 VarianceSwapTransactionSupplement added
    public class Contract
    {
        #region Members
        [System.Xml.Serialization.XmlElement("header", typeof(ContractHeader), Namespace = "http://www.fpml.org/2007/FpML-4-4")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Contract Header", IsVisible = false)]
        public ContractHeader header;
        [System.Xml.Serialization.XmlElement("brokerEquityOption", typeof(BrokerEquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("bulletPayment", typeof(BulletPayment), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("capFloor", typeof(CapFloor), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("cashBalance", typeof(CashBalance), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("cashBalanceInterest", typeof(CashBalanceInterest), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("cashBalanceReport", typeof(EfsML.v30.Notification.CashBalanceReport), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("creditDefaultSwap", typeof(CreditDefaultSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("debtSecurity", typeof(DebtSecurity), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("debtSecurityTransaction", typeof(DebtSecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("equityForward", typeof(EquityForward), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("equityOption", typeof(EquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("returnSwap", typeof(ReturnSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("equitySecurityTransaction", typeof(EquitySecurityTransaction), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("exchangeTradedDerivative", typeof(ExchangeTradedDerivative), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("fra", typeof(Fra), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxAverageRateOption", typeof(FxAverageRateOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxBarrierOption", typeof(FxBarrierOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxDigitalOption", typeof(FxDigitalOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxSingleLeg", typeof(FxLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxSimpleOption", typeof(FxOptionLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("fxSwap", typeof(FxSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("invoice", typeof(Invoice), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("invoiceSettlement", typeof(InvoiceSettlement), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("loanDeposit", typeof(LoanDeposit), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("marginRequirement", typeof(MarginRequirement), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("repo", typeof(Repo), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElement("swap", typeof(Swap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("swaption", typeof(Swaption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("strategy", typeof(Strategy), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElement("termDeposit", typeof(TermDeposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwapTransactionSupplement", typeof(VarianceSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Contract Header")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product", IsVisible = false, IsGroup = false, IsProduct = true)]
        [BookMarkGUI(IsVisible = true)]
        public Product product;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool otherPartyPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("otherPartyPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Other Party Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other Party Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] otherPartyPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgent", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent")]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentBusinessCenterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentBusinessCenter", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent Business Center")]
        public BusinessCenter calculationAgentBusinessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool collateralSpecified;
        [System.Xml.Serialization.XmlElementAttribute("collateral", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Collateral")]
        public Collateral collateral;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("documentation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Documentation")]
        public Documentation documentation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool governingLawSpecified;
        [System.Xml.Serialization.XmlElementAttribute("governingLaw", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Governing Law")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public GoverningLaw governingLaw;
        #endregion Members
    }
    #endregion Contract
    #region ContractHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractHeader
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Identifiers")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Identifier", IsClonable = true, IsChild = true)]
        public ContractIdentifier[] identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("information", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Information")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Information", IsClonable = true, IsChild = true)]
        public ContractInformation[] information;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Date", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractDate", Order = 3)]
        public TradeDate contractDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contract Date")]
        public bool FillBalise;
        #endregion Members
        #region Constructors
        public ContractHeader()
        {
            identifier = new ContractIdentifier[1] { new ContractIdentifier() };
            information = new ContractInformation[1] { new ContractInformation() };
            contractDate = new TradeDate();
        }
        #endregion Constructors
    }
    #endregion ContractHeader
    #region ContractId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string contractIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
        #region Accessors
        #region id
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (null == efs_id)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion id
        #endregion Accessors
    }
    #endregion ContractId
    #region ContractIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public class ContractIdentifier : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Name = "party", LblWidth = 50)]
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Id")]
        public EFS_RadioChoice contract;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractContractIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractId", typeof(ContractId), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Contract Ids", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Id")]
        public ContractId[] contractContractId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractVersionedContractIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedContractId", typeof(VersionedContractId), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Versioned contract Ids", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Versioned contract Id")]
        public VersionedContractId[] contractVersionedContractId;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
        #region Constructors
        public ContractIdentifier()
        {
            contractContractId = new ContractId[1] { new ContractId() };
            contractVersionedContractId = new VersionedContractId[1] { new VersionedContractId() };
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion ContractIdentifier
    #region ContractInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractInformation : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Name = "party", LblWidth = 50)]
        public PartyReference partyReference;
        #endregion Members
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion ContractIdentifier
    #region ContractReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractReference
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Contract identifier", IsMaster = true)]
        public ContractIdentifier[] identifier;
        #endregion Members
    }
    #endregion ContractReference

    #region DataDocument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("FpML", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    // EG 20170918 [23342] Upd party ArrayDivGUI
    public partial class DataDocument : Document
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool validationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("validation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Validations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Validation", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Validation[] validation;
        [System.Xml.Serialization.XmlElementAttribute("trade", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade", IsMaster = true, IsMasterVisible = true, MinItem = 1, MaxItem = 1)]
        public Trade[] trade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool portfolioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("portfolio", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        public Portfolio[] portfolio;
        [System.Xml.Serialization.XmlElementAttribute("event", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        public ToDefine.Event[] @event;
        [System.Xml.Serialization.XmlElementAttribute("party", Order = 5)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Parties", IsMaster = true, IsChild = true, MinItem = 2, Color = MethodsGUI.ColorEnum.BlueLight)]
        public Party[] party;
        #endregion Members
    }
    #endregion DataDocument
    #region Document
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	/*
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AcceptQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationAmended))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmendmentConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeCashflows))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractIncreased))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractFullTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractPartialTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractReferenceMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNotification))]
	*/
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DataDocument))]
	/*
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IncreaseConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Message))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRejected))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotificationMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationNotificationMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationRequestMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationResponseMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAcknowledged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsMatchResults))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAcceptanceConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAlreadyExpired))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteUpdated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAllocation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAmendmentConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestIncreaseConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuoteResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTerminationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPortfolio))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestValuationReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadySubmitted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyTerminated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmended))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsMatchResult))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeErrorResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMismatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNotFound))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeUnmatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TerminationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationDocument))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReport))]
	*/
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EfsDocument))]
    [System.Xml.Serialization.XmlRootAttribute("FpML", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
	public abstract partial class Document
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public DocumentVersionEnum version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool expectedBuildSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public int expectedBuild;
        #endregion Members
    }
    #endregion Document

    #region ExecutionDateTime
    /// EG 20170918 [23342] New FpML extensions for MiFID I (use since MiFID II)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
    public partial class ExecutionDateTime : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string executionDateTimeScheme;
        //public EFS_DateTimeOffset Value;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            set { _date = new EFS_DateTimeOffset(value); }
            get {return (null != _date) ? _date.GetValue : string.Empty;}
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValueSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(Value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _dateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Execution date", LblWidth = 130)]
        public EFS_DateTimeOffset _date;

        #endregion Members
    }
    #endregion ExecutionDateTime

    #region LinkId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class LinkId
    {
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute("linkIdScheme", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string LinkIdScheme
		{
			set { linkIdScheme = new EFS_String(value); }
			get
			{
				if (linkIdScheme == null)
					return null;
				else
					return linkIdScheme.Value;
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
		public string Factor
		{
			set { factor = new EFS_Decimal(value); }
			get
			{
				if (null == factor)
					return null;
				return factor.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
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
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "Scheme", Width = 200)]
		public EFS_String linkIdScheme;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Name = "value", Width = 150)]
		public EFS_String Identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "factor", Width = 100)]
		public EFS_Decimal factor;
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

    #region PartyRole
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
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
        [System.Xml.Serialization.XmlElementAttribute("party", typeof(PartyReference),Order = 2)]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationTradeIdentifier))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BlockTradeIdentifier))]
    public partial class PartyTradeIdentifier : TradeIdentifier
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool linkIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("linkId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Link Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Link Id", IsClonable = true)]
        public LinkId[] linkId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bookIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bookId", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Book Id")]
        public BookId bookId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localClassDerv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocalClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public LocalClassDerv localClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iasClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("iasClassDerv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "IASClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IASClassDerv iasClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeClassDerv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeClassDerv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeClassDerv hedgeClassDerv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeFolderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeFolder", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeFolder")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeFolder hedgeFolder;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeFactor")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeFactor hedgeFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxClassSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxClass", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FxClass")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public FxClass fxClass;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localClassNDrv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LocalClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public LocalClassNDrv localClassNDrv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool iasClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("iasClassNDrv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "IASClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IASClassNDrv iasClassNDrv;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hedgeClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hedgeClassNDrv", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "HedgeClassNDrv")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public HedgeClassNDrv hedgeClassNDrv;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:PartyTradeIdentifier";
        #endregion Members
    }
    #endregion PartyTradeIdentifier
    #region PartyTradeIdentifiers
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PartyTradeIdentifiers
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Trade Identifier", IsClonable = true)]
        public PartyTradeIdentifier[] partyTradeIdentifier;
        #endregion Members
    }
    #endregion PartyTradeIdentifiers
    #region PartyTradeInformation
    // EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyTradeInformation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Party", LblWidth = 50)]
        public PartyReference partyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool traderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("trader", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Traders")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trader", IsClonable = true)]
        public Trader[] trader;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool executionDateTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("executionDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Execution date")]
        public ExecutionDateTime executionDateTime;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool salesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sales", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Sales")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sale", IsClonable = true)]
        public Trader[] sales;
        //
        //20080930 FI 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool brokerPartyReferenceSpecified;
        //20080930 FI 
        [System.Xml.Serialization.XmlElementAttribute("brokerPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Broker Party Reference")]
        public ArrayPartyReference[] brokerPartyReference;

        // EG 20170918 [23342]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedPartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedParty", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Related party")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related party")]
        public RelatedParty[] relatedParty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedPersonSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedPerson", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related person")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related person")]
        public RelatedPerson[] relatedPerson;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool algorithmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("algorithm", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Algorithm")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Algorithm")]
        public Algorithm[] algorithm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool categorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("category", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Category")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Category")]
        public TradeCategory[] category;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timestampsSpecified;

        [System.Xml.Serialization.XmlElementAttribute("timestamps", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Timestamps")]
        public TradeProcessingTimestamps timestamps;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intentToClearSpecified;
        [System.Xml.Serialization.XmlElementAttribute("intentToClear", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Intent to clear")]
        public EFS_Boolean intentToClear;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reportingRegimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("reportingRegime", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reporting regime")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reporting regime", IsChild = true)]
        public ReportingRegime[] reportingRegime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool isSecuritiesFinancingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isSecuritiesFinancing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Securities financing")]
        public EFS_Boolean isSecuritiesFinancing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool otcClassificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("otcClassification", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "OTC classification")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "OTC classification")]
        public OtcClassification[] otcClassification;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradingWaiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradingWaiver", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trading waiver")]
        public TradingWaiver[] tradingWaiver;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool shortSaleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("shortSale", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Short sale")]
        public ShortSale shortSale;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool isCommodityHedgeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isCommodityHedge", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commodity hedge")]
        public EFS_Boolean isCommodityHedge;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool isDisputedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isDisputed", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Disputed")]
        public EFS_Boolean isDisputed;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
        Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:PartyTradeInformation";
        #endregion Members
    }
    #endregion PartyTradeInformation
    
    #region PaymentDetail
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PaymentDetail
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDatePaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", typeof(AdjustableOrRelativeDate), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payment date", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public AdjustableOrRelativeDate paymentDatePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateAdjustablePaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", typeof(AdjustableDate2), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable payment date", IsVisible = true)]
        public AdjustableDate2 paymentDateAdjustablePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateAdjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", typeof(EFS_Date), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjusted payment date", IsVisible = true)]
        public EFS_Date paymentDateAdjustedPaymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment amount")]
        public Money paymentAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentRuleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentRule", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment rule")]
        public PaymentRule paymentRule;
        #endregion Members
        #region Constructors
        public PaymentDetail()
        {
            paymentDatePaymentDate = new AdjustableOrRelativeDate();
            paymentDateAdjustablePaymentDate = new AdjustableDate2();
            paymentDateAdjustedPaymentDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion PaymentDetail
    #region PaymentRule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PercentageRule))]
    public abstract class PaymentRule { }
    #endregion PaymentRule

    #region QueryParameterId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QueryParameterId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string queryParameterIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.QueryParameterId)]
        public EFS_Id efs_id;
        #endregion Members
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
    }
    #endregion QueryParameterId
    #region QueryParameterOperator
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QueryParameterOperator : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string queryParameterOperatorScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.QueryParameterOperator)]
        public EFS_Id efs_id;
        #endregion Members
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
        #region Constructors
        public QueryParameterOperator()
        {
            queryParameterOperatorScheme = "http://www.fpml.org/coding-scheme/query-parameter-operator-1-0";
        }
        #endregion Constructors
    }
    #endregion QueryParameterOperator

    #region Strategy
    // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapOption removed
    // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapTransactionSupplement added
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("strategy", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class Strategy : Product
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumProductReferenceSpecified;
        //PL 20100323 Add Namespace = "http://www.fpml.org/2007/FpML-4-4", 
        [System.Xml.Serialization.XmlElementAttribute("premiumProductReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Premium product reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public ProductReference premiumProductReference;
        [System.Xml.Serialization.XmlElementAttribute("additionalInvoice", typeof(AdditionalInvoice), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("bondOption", typeof(BondOption.BondOption), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("brokerEquityOption", typeof(BrokerEquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(BulletPayment),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("buyAndSellBack", typeof(BuyAndSellBack), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(CapFloor),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]

        [System.Xml.Serialization.XmlElementAttribute("commoditySpot", typeof(CommoditySpot), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("commoditySwap", typeof(CommoditySwap), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]

        [System.Xml.Serialization.XmlElementAttribute("correlationSwap", typeof(CorrelationSwap),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("correlationSwapOption", typeof(CorrelationSwapOption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", typeof(CreditDefaultSwap),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwapOption", typeof(CreditDefaultSwapOption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditNote", typeof(CreditNote), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", typeof(DebtSecurity), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", typeof(DebtSecurityTransaction), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplement", typeof(DividendSwapTransactionSupplement),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplementOption", typeof(DividendSwapTransactionSupplementOption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeBase", typeof(EquityDerivativeBase),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeLongFormBase", typeof(EquityDerivativeLongFormBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeShortFormBase", typeof(EquityDerivativeShortFormBase),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityForward", typeof(EquityForward),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4",Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransaction", typeof(EquitySecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", typeof(ExchangeTradedDerivative), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(Fra), Namespace = "http://www.fpml.org/2007/FpML-4-4",Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(FxAverageRateOption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(FxBarrierOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(FxDigitalOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption",typeof(FxOptionLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(FxLeg),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(FxSwap),Namespace = "http://www.fpml.org/2007/FpML-4-4",  Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("futureTransaction", typeof(FutureTransaction), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("invoice", typeof(Invoice), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("invoiceSettlement", typeof(InvoiceSettlement), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("nettedSwapBase", typeof(NettedSwapBase),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("optionBase", typeof(OptionBase),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("optionBaseExtended", typeof(OptionBaseExtended),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("repo", typeof(Repo), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwap", typeof(ReturnSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwapBase", typeof(ReturnSwapBase),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("securityLending", typeof(SecurityLending), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(Strategy),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(Swap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(Swaption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(TermDeposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwap", typeof(VarianceSwap),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("varianceSwapOption", typeof(VarianceSwapOption),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwapTransactionSupplement", typeof(VarianceSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product", IsClonable = false, IsChild = true, IsVariableArray = false, IsProduct = true)]
        public Product[] Item;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mainProductReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mainProductReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Main product reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public ProductReference mainProductReference;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
            Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Strategy";

        #endregion Members
        
        #region Constructors
        public Strategy()
        {
            premiumProductReference = new ProductReference(); 
            mainProductReference = new ProductReference();
        }
        #endregion Constructors

    }
    #endregion Strategy

    #region Trade
    //FI 20130701 [18745] Add EfsML.v30.Notification.CashBalanceReport
    // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapOption removed
    // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapTransactionSupplement added
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Trade
    {
        #region Members
        [System.Xml.Serialization.XmlElement("tradeHeader", typeof(TradeHeader), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade Header", IsVisible = false, Color = MethodsGUI.ColorEnum.BlueLight)]
        public TradeHeader tradeHeader;
        [System.Xml.Serialization.XmlElementAttribute("additionalInvoice", typeof(AdditionalInvoice), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("bondOption", typeof(BondOption.BondOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("brokerEquityOption", typeof(BrokerEquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(BulletPayment), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("buyAndSellBack", typeof(BuyAndSellBack), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(CapFloor), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", typeof(CashBalance), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceReport", typeof(EfsML.v30.Notification.CashBalanceReport), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceInterest", typeof(CashBalanceInterest), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("commoditySpot", typeof(CommoditySpot), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("commoditySwap", typeof(CommoditySwap), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("correlationSwap", typeof(CorrelationSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("correlationSwapOption", typeof(CorrelationSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", typeof(CreditDefaultSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwapOption", typeof(CreditDefaultSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("creditNote", typeof(CreditNote), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", typeof(DebtSecurity), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", typeof(DebtSecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplement", typeof(DividendSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplementOption", typeof(DividendSwapTransactionSupplementOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeBase", typeof(EquityDerivativeBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeLongFormBase", typeof(EquityDerivativeLongFormBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeShortFormBase", typeof(EquityDerivativeShortFormBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityForward", typeof(EquityForward), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransaction", typeof(EquitySecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", typeof(ExchangeTradedDerivative), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(Fra), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("futureTransaction", typeof(FutureTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(FxAverageRateOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(FxBarrierOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(FxDigitalOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(FxOptionLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(FxLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(FxSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("invoice", typeof(Invoice), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("invoiceSettlement", typeof(InvoiceSettlement), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("loanDeposit", typeof(LoanDeposit), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", typeof(MarginRequirement), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("nettedSwapBase", typeof(NettedSwapBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("optionBase", typeof(OptionBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("optionBaseExtended", typeof(OptionBaseExtended), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("repo", typeof(Repo), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwap", typeof(ReturnSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwapBase", typeof(ReturnSwapBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("securityLending", typeof(SecurityLending), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(Strategy), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(Swap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(Swaption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(TermDeposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwap", typeof(VarianceSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        //[System.Xml.Serialization.XmlElementAttribute("varianceSwapOption", typeof(VarianceSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwapTransactionSupplement", typeof(VarianceSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]

        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade Header")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product", IsVisible = false, IsGroup = false, IsProduct = true, Color = MethodsGUI.ColorEnum.Blue)]
        [BookMarkGUI(IsVisible = true)]
        [DictionaryGUI(Page = "ExecutionReport")]
        public Product product;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool otherPartyPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("otherPartyPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Product")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Other Party Payment", Color = MethodsGUI.ColorEnum.Violet)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other Party Payment", IsClonable = true, IsChild = true, Color = MethodsGUI.ColorEnum.Violet)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] otherPartyPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool brokerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("brokerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Broker Party References")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Broker Party Reference", MinItem = 0)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public ArrayPartyReference[] brokerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgent", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent")]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentBusinessCenterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentBusinessCenter", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent Business Center")]
        public BusinessCenter calculationAgentBusinessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool collateralSpecified;
        [System.Xml.Serialization.XmlElementAttribute("collateral", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Collateral")]
        public Collateral collateral;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("documentation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Documentation")]
        public Documentation documentation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool governingLawSpecified;
        [System.Xml.Serialization.XmlElementAttribute("governingLaw", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Governing Law")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public GoverningLaw governingLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allocations", IsNullable = false, Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Allocations")]
        public Allocations allocations;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeSide", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade Side")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Side", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public TradeSide[] tradeSide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeIntentionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeIntention", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Trade intention")]
        public TradeIntention tradeIntention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInputSpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementInput", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Settlement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public SettlementInput[] settlementInput;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nettingInformationInputSpecified;

        [System.Xml.Serialization.XmlElementAttribute("nettingInformationInput", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Netting")]
        public NettingInformationInput nettingInformationInput;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendsSpecified;

        [System.Xml.Serialization.XmlElementAttribute("tradeExtends", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extends", Color = MethodsGUI.ColorEnum.Gray)]
        public TradeExtends extends;

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
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0", DataType = "normalizedString")]
        public string tradeId;


        #endregion Members
    }
    #endregion Trade
    #region TradeDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20140702 New BizDt
    // EG 20171028 [23509] Upd Hide timestamp and bizdt dans Full FpML (CreateControlGUI)
    public partial class TradeDate : IdentifiedDate
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("timeStamp", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string TimeStamp
        {
            set { efs_timeStamp = new EFS_Time(value); }
            get { return efs_timeStamp?.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        //[ControlGUI(Name = "TimeStamp")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Time efs_timeStamp;

        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string BizDt
        {
            set
            {
                efs_BizDt = new EFS_Date(value);
            }
            get
            {
                if (efs_BizDt == null)
                    return null;
                else
                    return efs_BizDt.Value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsLabel = true, Name = "Business date", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Date efs_BizDt;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:TradeDate";
        #endregion Members
    }
    #endregion TradeDate
    #region TradeHeader
    // EG 20170918 [23342] New FpML extensions for MiFID II
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20171003 [23452] Add tradeDateTime 
    public partial class TradeHeader
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Identifiers")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Identifier", IsClonable = true, IsMaster = true, IsChild = true)]
        public PartyTradeIdentifier[] partyTradeIdentifier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyTradeInformationSpecified;

        [System.Xml.Serialization.XmlElementAttribute("partyTradeInformation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Information")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Information", IsClonable = true, IsChild = true)]
        public PartyTradeInformation[] partyTradeInformation;

        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("tradeDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        public TradeDate tradeDate;

        // EG 20170822 [23342]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool productSummarySpecified;
        [System.Xml.Serialization.XmlElementAttribute("productSummary", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Product summary")]
        public ProductSummary productSummary;

        // EG 20170804 [23342]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clearedDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("clearedDate", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cleared date")]
        public IdentifiedDate clearedDate;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:TradeHeader";

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cleared Date")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion TradeHeader
    #region TradeId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Extension FpML de tradeId - add source)
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
        [ControlGUI(Name = "Id", Width = 50)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        [ControlGUI(Name = "Source", Width = 50)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public string source;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance", Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string type = "efs:TradeId";
        #endregion Members
    }
    #endregion TradeId
    #region TradeIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationTradeIdentifier))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BlockTradeIdentifier))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartyTradeIdentifier))]
    // EG 20170918 [23342] Upd
    public partial class TradeIdentifier
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Name = "party", LblWidth = 50)]
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Id")]
        public EFS_RadioChoice trade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeId", typeof(TradeId), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Trade Ids", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Id")]
        public TradeId[] tradeTradeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeVersionedTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedTradeId", typeof(VersionedTradeId), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Versioned trade Ids", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Versioned trade Id")]
        public VersionedTradeId[] tradeVersionedTradeId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion TradeIdentifier
    #region Trader
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
		[System.Xml.Serialization.XmlAttributeAttribute("name",Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string Name
		{
			set { name = new EFS_String(value); }
			get
			{
				if (name == null)
					return null;
				else
					return name.Value;
			}
		}
		[System.Xml.Serialization.XmlAttributeAttribute("factor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string Factor
		{
			set { factor = new EFS_Decimal(value); }
			get
			{
				if (null == factor)
					return null;
				return factor.Value;
			}
		}
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Name = "Identifier", Width = 150)]
		public EFS_String Identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "name", Width = 200)]
		public EFS_String name;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "factor", Width = 100, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Decimal factor;
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
    #region TradeSide
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
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
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "confirmer")]
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
        [System.Xml.Serialization.XmlElementAttribute("accountant", Order = 9)]
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

    #region VersionedContractId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class VersionedContractId : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("contractId", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Id", IsVisible = false)]
        public ContractId contractId;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Id")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
        public EFS_NonNegativeInteger version;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version effective Date")]
        public IdentifiedDate effectiveDate;
    }
    #endregion VersionedContractId
    #region VersionedTradeId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class VersionedTradeId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("tradeId", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Id", IsVisible = false)]
        public TradeId tradeId;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Id")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
        public EFS_NonNegativeInteger version;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version effective Date")]
        public IdentifiedDate effectiveDate;
		#endregion Members
	}
    #endregion VersionedTradeId
}



namespace FpML.v44.Doc.ToDefine
{
    #region Allocation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Allocation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("allocationTradeId", Order = 1)]
        public PartyTradeIdentifier allocationTradeId;
        [System.Xml.Serialization.XmlElementAttribute("partyReference", typeof(PartyReference), Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("accountReference", typeof(AccountReference), Order = 2)]
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute("allocatedNotional", typeof(Money), Order = 3)]
        [System.Xml.Serialization.XmlElementAttribute("allocatedFraction", typeof(System.Decimal), Order = 3)]
        public object Item1;
        [System.Xml.Serialization.XmlElementAttribute("collateral",Order = 4)]
        public Collateral collateral;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool creditChargeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditChargeAmount", Order = 5)]
        public Money creditChargeAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool approvalsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("approval", Order = 6)]
        public Approval[] approvals;
        [System.Xml.Serialization.XmlElementAttribute("masterConfirmationDate", Order = 7)]
        public DateTime masterConfirmationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool masterConfirmationDateSpecified;
        #endregion Members
    }
    #endregion Allocation
    #region Allocations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Allocations
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("allocation", Order = 1)]
        public Allocation[] allocation;
        #endregion Members
    }
    #endregion Allocations
    #region Amendment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Amendment : Event
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("trade", Order = 1)]
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("amendmentTradeDate",DataType = "date", Order = 2)]
        public System.DateTime amendmentTradeDate;
        [System.Xml.Serialization.XmlElementAttribute("amendmentEffectiveDate",DataType = "date", Order = 3)]
        public System.DateTime amendmentEffectiveDate;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 4)]
        public Payment payment;
        #endregion Members
    }
    #endregion Amendment
    #region Approval
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Approval
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString", Order = 1)]
        public string type;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString", Order = 2)]
        public string status;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool approverSpecified;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString", Order = 3)]
        public string approver;
        #endregion Members
    }
    #endregion Approval
    #region Approvals
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Approvals
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("approval", Order = 1)]
        public Approval[] approval;
        #endregion Members
    }
    #endregion Approvals

    #region BestFitTrade
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BestFitTrade
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier", Order = 1)]
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("differences", Order = 2)]
        public TradeDifference[] differences;
        #endregion Members
    }
    #endregion BestFitTrade

    #region ChangeContract
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ChangeContractSize))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractTermination))]
    public abstract class ChangeContract
    {
        [System.Xml.Serialization.XmlElementAttribute("contractReference", Order = 1)]
        public ContractReference contractReference;
        [System.Xml.Serialization.XmlElementAttribute("date", Order = 2)]
        public EFS_Date date;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 3)]
        public EFS_Date effectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 4)]
        public Payment payment;
    }
    #endregion ChangeContract
    #region ChangeContractSize
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ChangeContractSize : ChangeContract
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("changeInNotionalAmount;", Order = 1)]
        public Money changeInNotionalAmount;
        [System.Xml.Serialization.XmlElementAttribute("outstandingNotionalAmount", Order = 2)]
        public Money outstandingNotionalAmount;
        [System.Xml.Serialization.XmlElementAttribute("changeInNumberOfOptions", Order = 3)]
        public EFS_Decimal changeInNumberOfOptions;
        [System.Xml.Serialization.XmlElementAttribute("outstandingNumberOfOptions", Order = 4)]
        public EFS_Decimal outstandingNumberOfOptions;
        [System.Xml.Serialization.XmlElementAttribute("changeInNumberOfUnits", Order = 5)]
        public EFS_Decimal changeInNumberOfUnits;
        [System.Xml.Serialization.XmlElementAttribute("outstandingNumberOfUnits", Order = 6)]
        public EFS_Decimal outstandingNumberOfUnits;
        #endregion Members
    }
    #endregion ChangeContractSize
    #region Collateral
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Collateral
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("independentAmount", Order = 1)]
        public IndependentAmount independentAmount;
        #endregion Members
    }
    #endregion Collateral
    #region ContractNovation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractNovation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("oldContract", typeof(Contract),Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("oldContractReference", typeof(ContractIdentifier[]),Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("newContract", typeof(Contract),Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("newContractReference", typeof(ContractIdentifier[]),Order = 1)]
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute("transferor", Order = 5)]
        public PartyReference transferor;
        [System.Xml.Serialization.XmlElementAttribute("transferee", Order = 6)]
        public PartyReference transferee;
        [System.Xml.Serialization.XmlElementAttribute("remainingParty", Order = 7)]
        public PartyReference remainingParty;
        [System.Xml.Serialization.XmlElementAttribute("otherRemainingParty", Order = 8)]
        public PartyReference otherRemainingParty;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date",Order = 9)]
        public System.DateTime novationDate;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date", Order = 10)]
        public System.DateTime novationContractDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool novationContractDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("novatedNumberOfUnits", typeof(System.Decimal), Order = 11)]
        [System.Xml.Serialization.XmlElementAttribute("novatedNumberOfOptions", typeof(System.Decimal), Order = 11)]
        [System.Xml.Serialization.XmlElementAttribute("novatedAmount", typeof(Money), Order = 11)]
        public object Item1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fullFirstCalculationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullFirstCalculationPeriod", Order = 14)]
        public bool fullFirstCalculationPeriod;
        [System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate", Order = 15)]
        public FirstPeriodStartDate[] firstPeriodStartDate;
        [System.Xml.Serialization.XmlElementAttribute("nonReliance", Order = 16)]
        public Empty nonReliance;
        [System.Xml.Serialization.XmlElementAttribute("creditDerivativesNotices", Order = 17)]
        public CreditDerivativesNotices creditDerivativesNotices;
        [System.Xml.Serialization.XmlElementAttribute("contractualDefinitions", Order = 18)]
        public ContractualDefinitions[] contractualDefinitions;
        [System.Xml.Serialization.XmlElementAttribute("contractualTermsSupplement", Order = 19)]
        public ContractualTermsSupplement[] contractualTermsSupplement;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 20)]
        public Payment payment;
        #endregion Members
    }
    #endregion ContractNovation
    #region ContractTermination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractTermination : ChangeContract { }
    #endregion ContractTermination
    #region CreditDerivativesNotices
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CreditDerivativesNotices
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("creditEvent", Order = 1)]
        public bool creditEvent;
        [System.Xml.Serialization.XmlElementAttribute("publiclyAvailableInformation", Order = 2)]
        public bool publiclyAvailableInformation;
        [System.Xml.Serialization.XmlElementAttribute("physicalSettlement", Order = 3)]
        public bool physicalSettlement;
        #endregion Members
    }
    #endregion CreditDerivativesNotices

    #region Event
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Increase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Termination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Novation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Amendment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNoticeDocument))]
    public abstract partial class Event
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("eventId",Order = 1)]
        public EventId[] eventId;
        #endregion Members
    }
    #endregion Event
    #region EventId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EventId : ItemGUI
    {
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue eventId;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Scheme", Width = 350)]
		public string eventIdScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "Id", Width = 50)]
		public string id;
		#endregion Members
    }
    #endregion EventId

    #region FirstPeriodStartDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FirstPeriodStartDate
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "date")]
        public System.DateTime Value;
        #endregion Members
    }
    #endregion FirstPeriodStartDate

    #region Increase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Increase : Event
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("trade",Order = 1)]
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier",  Order = 2)]
        public PartyTradeIdentifier[] tradeReference;
        [System.Xml.Serialization.XmlElementAttribute("increaseTradeDate", Order = 3)]
        public System.DateTime increaseTradeDate;
        [System.Xml.Serialization.XmlElementAttribute("increaseEffectiveDate", Order = 4)]
        public System.DateTime increaseEffectiveDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Increase informations")]
		public EFS_RadioChoice detail;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailIncreaseInNotionalAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("increaseInNotionalAmount", typeof(Money), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money detailIncreaseInNotionalAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailOutstandingNotionalAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("outstandingNotionalAmount", typeof(Money), Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money detailOutstandingNotionalAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailIncreaseInNumberOfOptionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("increaseInNumberOfOptions", typeof(EFS_Decimal), Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal detailIncreaseInNumberOfOptions;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailOutstandingNumberOfOptionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("outstandingNumberOfOptions", typeof(EFS_Decimal), Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal detailOutstandingNumberOfOptions;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 9)]
        public Payment payment;
        #endregion Members
    }
    #endregion Increase
    #region IndependentAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
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

    #region PartyPortFolioName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
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
    }
    #endregion PartyPortFolioName
    #region PercentageRule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PercentageRule : PaymentRule
    {
        [System.Xml.Serialization.XmlElementAttribute("paymentPercent", Order = 1)]
        public System.Decimal paymentPercent;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmountReference", Order = 2)]
        public NotionalAmountReference notionalAmountReference;
    }
    #endregion PercentageRule
    #region PortFolio
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QueryPortfolio))]
    public class Portfolio
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyPortfolioName", Order = 1)]
        public PartyPortfolioName partyPortfolioName;
        [System.Xml.Serialization.XmlElementAttribute("tradeId", Order = 2)]
        public TradeId[] tradeId;
        [System.Xml.Serialization.XmlElementAttribute("portfolio", Order = 3)]
        public Portfolio[] portfolio;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PortFolio)]
        public EFS_Id efs_id;
        #endregion Members
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
    }
    #endregion PortFolio
    #region PortFolioName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PortFolioName
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PortFolio)]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string portfolioNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
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
    }
    #endregion PortFolioName

    #region QueryParameter
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QueryParameter
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("queryParameterId", Order = 1)]
        public QueryParameterId queryParameterId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString", Order = 2)]
        public string queryParameterValue;
        [System.Xml.Serialization.XmlElementAttribute("queryParameterOperator", Order = 3)]
        public QueryParameterOperator queryParameterOperator;
        #endregion Members
    }
    #endregion QueryParameter
    #region QueryPortfolio
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QueryPortfolio : Portfolio
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("queryParameter", Order = 1)]
        public QueryParameter[] queryParameter;
        #endregion Members
    }
    #endregion QueryPortfolio

    #region TradeDifference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeDifference
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("differenceType", Order = 1)]
        public DifferenceTypeEnum differenceType;
        [System.Xml.Serialization.XmlElementAttribute("differenceSeverity", Order = 2)]
        public DifferenceSeverityEnum differenceSeverity;
        [System.Xml.Serialization.XmlElementAttribute("element", Order = 3)]
        public string element;
        [System.Xml.Serialization.XmlElementAttribute("basePath", Order = 4)]
        public string basePath;
        [System.Xml.Serialization.XmlElementAttribute("baseValue", Order = 5)]
        public string baseValue;
        [System.Xml.Serialization.XmlElementAttribute("otherPath", Order = 6)]
        public string otherPath;
        [System.Xml.Serialization.XmlElementAttribute("otherValue", Order = 7)]
        public string otherValue;
        [System.Xml.Serialization.XmlElementAttribute("missingElement", Order = 8)]
        public string[] missingElement;
        [System.Xml.Serialization.XmlElementAttribute("extraElement", Order = 9)]
        public string[] extraElement;
        [System.Xml.Serialization.XmlElementAttribute("message", Order = 10)]
        public string message;
        #endregion Members
    }
    #endregion TradeDifference

    #region Validation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Validation
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string validationScheme;
    }
    #endregion Validation
}
