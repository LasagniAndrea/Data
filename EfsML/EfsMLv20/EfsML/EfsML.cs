#region Using Directives
using System;
using System.Text;
using System.Reflection;
using System.Data;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;


using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.ComplexControls;

using EfsML;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Doc;
using FpML.v42.Enum;
using FpML.v42.EqShared;
using FpML.v42.Eqs;
using FpML.v42.Fx;
using FpML.v42.Shared;
using FixML.v44;


#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision
namespace EfsML.v20
{

    #region AssetFxRateId
    public partial class AssetFxRateId : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate ref.", Width = 300)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public string Value;
        public string otcmlId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
		}
		#endregion Members
    }
    #endregion AssetFxRateId
    #region AssetOrNothing
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class AssetOrNothing : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyReference", typeof(Currency), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currencyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gapSpecified;
		[System.Xml.Serialization.XmlElementAttribute("gap", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Gap", Width = 75)]
        public EFS_Decimal gap;
		#endregion Members
    }
    #endregion AssetOrNothing
    #region AverageStrikeOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class AverageStrikeOption : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 1)]
		[ControlGUI(Name = "Settlement Type")]
        public SettlementTypeEnum settlementType;
		#endregion Members
	}
	#endregion AverageStrikeOption

	#region BookId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class BookId : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string bookIdScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlIgnore()]
        public bool bookNameSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("bookName", DataType = "normalizedString")]
        public string bookName;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
		#endregion Members
	}
    #endregion BookId

    #region CappedCallOrFlooredPut
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CappedCallOrFlooredPut : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Type")]
        public EFS_RadioChoice type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFxCapBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxCapBarrier", typeof(Empty),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty typeFxCapBarrier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFxFloorBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFloorBarrier", typeof(Empty),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty typeFxFloorBarrier;

		[System.Xml.Serialization.XmlElementAttribute("payoutStyle", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Payout style")]
        public PayoutEnum payoutStyle;
		#endregion Members
	}
    #endregion CappedCallOrFlooredPut

    #region CustomerSettlementPayment
    public partial class CustomerSettlementPayment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency",Order=1)]
        [ControlGUI(Name = "currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amount",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "amount")]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("rate",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate", IsVisible = true)]
        public ExchangeRate rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
        public bool FillBalise;
		#endregion Members
    }
    #endregion CustomerSettlementPayment

    #region Css
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class Css : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
		#endregion Members
	}
    #endregion Css
    #region CssCriteria
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CssCriteria : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice cssCriteria;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssCriteriaCssSpecified;
        [System.Xml.Serialization.XmlElementAttribute("css", typeof(Css),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Css", IsVisible = true)]
        public Css cssCriteriaCss;
        //		
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssCriteriaCssInfoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssInfo", typeof(CssInfo),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "CssInfo", IsVisible = true)]
        public CssInfo cssCriteriaCssInfo;
		#endregion Members
	}
    #endregion CssCriteria
    #region CssInfo
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CssInfo : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssCountrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssCountry",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country", Width = 350)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Country cssCountry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssType",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssType cssType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssSystemTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssSystemType",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "System type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssSystemType cssSystemType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssSettlementTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssSettlementType",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssSettlementType cssSettlementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssPaymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssPaymentType",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssPaymentType cssPaymentType;
		#endregion Members
    }
    #endregion CssInfo
    #region CssPaymentType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CssPaymentType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cssPaymentTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion CssPaymentType
    #region CssSettlementType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CssSettlementType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cssSettlementTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion CssSettlementType
    #region CssSystemType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	public partial class CssSystemType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cssSystemTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
	}
    #endregion CssSystemType
    #region CssType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class CssType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cssTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion CssType

    #region EfsDocument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("EfsML", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class EfsDocument : DataDocument
	{
		#region Members
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        public string efs_schemaLocation;
		
		#endregion Members
    }
    #endregion EfsDocument
    #region EfsSettlementInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class EfsSettlementInformation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice information;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationStandardSpecified;

        [System.Xml.Serialization.XmlElementAttribute("standardSettlementStyle", typeof(StandardSettlementStyleEnum),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Style")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Standard", IsVisible = true)]
        public StandardSettlementStyleEnum informationStandard;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationInstructionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", typeof(EfsSettlementInstruction),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Instruction", IsVisible = true)]
        public EfsSettlementInstruction informationInstruction;

        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        //[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //public EfsMLDocumentVersionEnum EfsMLversion;

		#endregion Members
    }
    #endregion EfsSettlementInformation
    #region EfsSettlementInstruction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("EfsSettlementInstruction", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class EfsSettlementInstruction : SettlementInstruction
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementMethodInformation",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Method Information")]
        public Routing settlementMethodInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool investorInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("investorInformation",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Investor Information")]
        public Routing investorInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool originatorInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("originatorInformation",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Originator Information")]
        public Routing originatorInformation;
        /// <summary>
        /// ISSIDatabase (used by SIGENPROCESS)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idssiDb;  
        /// <summary>
        /// IDISSI used In case of internalDatabase (used by SIGENPROCESS)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idIssi;	  
        
        #endregion Members
	}
    #endregion EfsSettlementInstruction

    #region EventCode
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	public partial class EventCode : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string eventCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion EventCode
    #region EventCodes
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class EventCodes : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool productReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("productReference",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Product reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public ProductReference productReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool StreamIdSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stream number", Width = 400)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [System.Xml.Serialization.XmlElementAttribute("streamId",Order=2)]
        public EFS_NonNegativeInteger streamId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventCodeSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event code", Width = 400)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [System.Xml.Serialization.XmlElementAttribute("eventCode",Order=3)]
        public EventCode eventCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventType",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event type", Width = 400)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EventType eventType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementDate",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement date ")]
        public EFS_Date settlementDate;
		#endregion Members
    }
    #endregion EventCodes
    #region EventCodesSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class EventCodesSchedule : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("eventCodes",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event Codes", IsClonable = true, IsChild = true, MinItem = 1)]
        public EventCodes[] eventCodes;
		#endregion Members
	}
    #endregion EventCodesSchedule
    #region EventType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class EventType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string eventTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion EventType

    #region FixML : Product
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("FIXML", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class FIXML : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("ExecRpt",Order=1)]
        [DictionaryGUI(Page = "ExecutionReport")]
        public ExecutionReport_message execReport;
		#endregion Members
	}
    #endregion FixML : Product

    #region FxOptionLegBarrier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxBarrierOption))]
    public class FxOptionLegBarrier : Product
    {
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
		[System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
		[System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementTermsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement Terms")]
        public FxCashSettlement cashSettlementTerms;
		[System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 8)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
		[System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
		[System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 10)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotedAsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("quotedAs", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted As")]
        public QuotedAs quotedAs;

        #region Accessors
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
        {
            get {return buyerPartyReference.href;}
        }
        #endregion BuyerPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
        {
            get { return sellerPartyReference.href; }
        }
        #endregion SellerPartyReference
        #endregion Accessors
        #region Constructors
        public FxOptionLegBarrier()
        {
            fxStrikePrice = new FxStrikePrice();
        }
        #endregion Constructors
    }
    #endregion FxOptionLegBarrier
    #region FxClass
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class FxClass : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string fxClassScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion FxClass
    #region FlowContext
    /// <summary>
    /// Class qui permet de définir un context
    /// La méthode IsMatchWith vérifie que l'évent est compatible avec le context
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class FlowContext : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyContextSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partyContext", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party context")]
        public PartyPayerReceiverReference[] partyContext;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventCodesScheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("eventCodesSchedule", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event codes schedule")]
        public EventCodesSchedule eventCodesSchedule;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSecuritiesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSecurities", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Securities")]
        public CashSecuritiesEnum cashSecurities;
		#endregion Members
    }
    #endregion FlowContext

    #region HedgeClassDerv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class HedgeClassDerv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string hedgeClassDervScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion HedgeClassDerv
    #region HedgeClassNDrv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class HedgeClassNDrv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string hedgeClassNDrvScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion HedgeClassNDrv

    #region HedgeFolder
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class HedgeFolder : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string hedgeFolderScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion HedgeFolder
    #region HedgeFactor
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class HedgeFactor : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string hedgeFactorScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion HedgeFactor

    #region IASClassDerv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class IASClassDerv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string iasClassDervScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion IASClassDerv
    #region IASClassNDrv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class IASClassNDrv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string iasClassNDrvScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion IASClassNDrv

    #region KnownAmountSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class KnownAmountSchedule : AmountSchedule
	{
		#region Members
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Initial Value")]
		[System.Xml.Serialization.XmlElementAttribute("notionalValue", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 1)]
        public Money notionalValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalValueSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 2)]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:KnownAmountSchedule";
		#endregion Members
    }
    #endregion KnownAmountSchedule

    #region LocalClassDerv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class LocalClassDerv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string localClassDervScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion LocalClassDerv
    #region LocalClassNDrv
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class LocalClassNDrv : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string localClassNDrvScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion LocalClassNDrv

    #region NettingDesignation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class NettingDesignation : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
		#endregion Members
    }
    #endregion NettingDesignation
    #region NettingInformationInput
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class NettingInformationInput : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("nettingMethod", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 1)]
        [ControlGUI(Name = "netting Method", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 250)]
        public NettingMethodEnum nettingMethod;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nettingDesignationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("nettingDesignation", Namespace = "http://www.efs.org/2005/EFSmL-2-0", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Netting Designation")]
        public NettingDesignation nettingDesignation;
		#endregion Members
    }
    #endregion NettingInformationInput

    #region PartyPayerReceiverReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class PartyPayerReceiverReference : ItemGUI, IEFS_Array
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party")]
        public EFS_RadioChoice partyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferencePayerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", typeof(PartyReference), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payer", IsVisible = true)]
        public PartyReference partyReferencePayer;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferenceReceiverSpecified;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Receiver", IsVisible = true)]
        public PartyReference partyReferenceReceiver;
        //
    }
    #endregion PartyPayerReceiverReference

    #region PaymentQuote
    public partial class PaymentQuote : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool percentageRateFractionSpecified;

		[System.Xml.Serialization.XmlElementAttribute("percentageRateFraction", typeof(EFS_String), Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rate Fraction")]
        public EFS_String percentageRateFraction;

		[System.Xml.Serialization.XmlElementAttribute("percentageRate", typeof(EFS_Decimal), Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rate")]
        public EFS_Decimal percentageRate;

		[System.Xml.Serialization.XmlElementAttribute("paymentRelativeTo", typeof(Reference), Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative to")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
		public Reference paymentRelativeTo;
		#endregion Members
	}
    #endregion PaymentQuote

    #region PayoutPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class PayoutPeriod : Interval
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("percentage", typeof(EFS_Decimal), Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Percentage of payout", Width = 75)]
        public EFS_Decimal percentage;
		#endregion Members
	}
    #endregion PayoutPeriod
    
    


    #region SettlementInformationInput
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SettlementInformationInput : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party")]
        public EFS_RadioChoice partyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferencePayerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", typeof(PartyReference), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payer", IsVisible = true)]
        public PartyReference partyReferencePayer;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferenceReceiverSpecified;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Receiver", IsVisible = true)]
        public PartyReference partyReferenceReceiver;
        //
		[System.Xml.Serialization.XmlElementAttribute("settlementInformation", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Information", IsVisible = true)]
        public EfsSettlementInformation settlementInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventCodesScheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("eventCodesSchedule", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Information")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Event codes schedule")]
        public EventCodesSchedule eventCodesSchedule;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssCriteriaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cssCriteria", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Css details")]
        public CssCriteria cssCriteria;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ssiCriteriaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("ssiCriteria", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Ssi details")]
        public SsiCriteria ssiCriteria;
		#endregion Members
	}
	#endregion SettlementInformationInput
	#region SettlementInput
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SettlementInput : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementContext", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Context", IsVisible = true)]
        public FlowContext settlementContext;
		[System.Xml.Serialization.XmlElementAttribute("settlementInputInfo", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Context")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Information", IsVisible = true)]
        public SettlementInputInfo settlementInputInfo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Information")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion SettlementInput
    #region SettlementInputInfo
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SettlementInputInfo
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementInformation", Order = 1)]
        public EfsSettlementInformation settlementInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssCriteriaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cssCriteria", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Css details")]
        public CssCriteria cssCriteria;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ssiCriteriaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("ssiCriteria", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Ssi details")]
        public SsiCriteria ssiCriteria;
		#endregion Members
	}
	#endregion SettlementInputInfo
	#region SsiCriteria
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SsiCriteria : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ssicountrySpecified;
		[System.Xml.Serialization.XmlElementAttribute("cssCountry", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country", Width = 350)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Country ssiCountry;
		#endregion Members
    }
    #endregion SsiCriteria

    #region SoftApplication
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SoftApplication
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("name", Order = 1)]
        public EFS_String name;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 2)]
        public EFS_String version;
        #endregion Members
    }
    #endregion SoftApplication

    #region SpheresId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SpheresId
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue spheresId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "Scheme", Width = 400)]
        public string scheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Id", Width = 50)]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        #endregion Members
    }
    #endregion SpheresId
    #region TradeExtend
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class TradeExtend
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue spheresId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "Scheme", Width = 400)]
        public string scheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Id", Width = 50)]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hRefSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("href", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string hRef;
        #endregion Members
    }
    #endregion TradeExtend
    #region SpheresSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class SpheresSource : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool statusSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "status")]
        [System.Xml.Serialization.XmlElementAttribute("status", Order = 1)]
        public SpheresSourceStatusEnum status;
        [System.Xml.Serialization.XmlElementAttribute("spheresId", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spheres Id")]
        public SpheresId[] spheresId;
        #endregion Members
    }
    #endregion SpheresSource

    #region StreamExtension
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class StreamExtension : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knockFeatures", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock Features")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock Feature", IsClonable = true, IsChild = true)]
        public KnockFeature[] knockFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerFeatures", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger Features")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger Feature", IsClonable = true, IsChild = true)]
        public TriggerFeature[] triggerFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averageFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averageFeatures", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average Features")]
        public AverageFeature averageFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexedRateFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexedRateFeatures", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Rate Features")]
        public IndexedRateFeature indexedRateFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxFeatures", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Features")]
        public FxFeature fxFeatures;
		#endregion Members
        //		
        #region Constructors
        public StreamExtension()
        {
        }
        #endregion Constructors
    }
    #endregion StreamExtension

    #region AmericanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class AmericanTrigger : ItemGUI
	{
		#region Members
		[ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TouchConditionEnum touchCondition;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerUnderlyer", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] triggerUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerBasketTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerUnderlyerBasketType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum triggerUnderlyerBasketType;

		[System.Xml.Serialization.XmlElementAttribute("trigger", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        public bool FillBalise;
		#endregion Members
		#region Constructors
		public AmericanTrigger() { }
        #endregion Constructors
    }
    #endregion AmericanTrigger
    #region CorridorBarriers
    public class CorridorBarriers : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("barrierUpSchedule", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Up", IsVisible = true)]
        public Schedule barrierUpSchedule;
		[System.Xml.Serialization.XmlElementAttribute("barrierDownSchedule", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down", IsVisible = true)]
        public Schedule barrierDownSchedule;
		[System.Xml.Serialization.XmlElementAttribute("corridorType", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Type")]
        public CorridorTypeEnum corridorType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("nbMaxValues", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nb Max Values")]
        public NbMaxValues nbMaxValues;
        #endregion Members
        #region Constructors
        public CorridorBarriers() { }
        #endregion Constructors
    }
    #endregion CorridorBarriers
    #region EuropeanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class EuropeanTrigger : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("triggerCondition", Order = 1)]
        [ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TriggerConditionEnum triggerCondition;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerUnderlyer",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] triggerUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerBasketTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerUnderlyerBasketType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum triggerUnderlyerBasketType;
		[System.Xml.Serialization.XmlElementAttribute("trigger", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        public bool FillBalise;
		#endregion Members
        #region Constructors
        public EuropeanTrigger() { }
        #endregion Constructors
    }
    #endregion EuropeanTrigger

    #region AverageFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class AverageFeature : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeFactorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strikeFactor", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike factor")]
        public EFS_Decimal strikeFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging period out")]
        public FeatureAveragingPeriod averagingPeriodOut;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Type", Width = 200)]
        public AveragingTypeEnum averagingType;
		#endregion Members
        #region Constructors
        public AverageFeature(){}
        #endregion Constructors
    }
    #endregion AverageFeature

    #region FeatureAveragingPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class FeatureAveragingPeriod : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("schedule",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedules")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public EquitySchedule[] schedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingDateTimesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingDateTimes",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Dates")]
        public DateTimeList averagingDateTimes;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weekNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("weekNumber", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Week number")]
        public EFS_Decimal weekNumber;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingOnPreviousPeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingOnPreviousPeriod", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging on previous period")]
        public EFS_Boolean averagingOnPreviousPeriod;

		[System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Market Disruption", Width = 200)]
        public MarketDisruptionEnum marketDisruption;
		#endregion Members
        #region Constructors
        public FeatureAveragingPeriod(){}
        #endregion Constructors
    }
    #endregion FeatureAveragingPeriod
    #region FxFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class FxFeature : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;
		[System.Xml.Serialization.XmlElementAttribute("referenceCurrency", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Currency", Width = 75)]
        public IdentifiedCurrency referenceCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotFxRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spotFxRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Composite")]
        public Composite spotFxRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool prefixedFxRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("prefixedFxRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Quanto")]
        public Quanto prefixedFxRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotFxRateSpreadSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spotFxRateSpread", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot FxRate spread")]
        public EFS_Decimal spotFxRateSpread;
		#endregion Members
		#region Constructors
		public FxFeature(){}
        #endregion Constructors
    }
    #endregion FxFeature
	#region ImplicitProvision
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	public partial class ImplicitProvision : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cancelableProvision", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Cancelable Provision")]
		public Empty cancelableProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool mandatoryEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Mandatory Early Termination Provision")]
		public Empty mandatoryEarlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationProvision", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Optional Early Termination Provision")]
		public Empty optionalEarlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("extendibleProvision", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Extendible Provision")]
		public Empty extendibleProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stepUpProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Step Up Provision")]
		public Empty stepUpProvision;
		#endregion Members
	}
	#endregion ImplicitProvision
    #region IndexedRateFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class IndexedRateFeature : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice featureType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureTypeIndexSpecified;
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(UnderlyerIndex),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public UnderlyerIndex featureTypeIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureTypeQuotedCurrencyPairSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", typeof(QuotedCurrencyPair),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public QuotedCurrencyPair featureTypeQuotedCurrencyPair;

		[System.Xml.Serialization.XmlElementAttribute("indexationPercentage", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Indexation percentage")]
        public EFS_Decimal indexationPercentage;
		[System.Xml.Serialization.XmlElementAttribute("indexationFormula", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Indexation formula", Width = 200)]
        public IndexationFormulaEnum indexationFormula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging period out")]
        public FeatureAveragingPeriod averagingPeriodOut;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingType", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Type", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingTypeEnum averagingType;
		#endregion Members
        #region Constructors
        public IndexedRateFeature()
        {
            featureTypeIndex = new UnderlyerIndex();
            featureTypeQuotedCurrencyPair = new QuotedCurrencyPair();
        }
        #endregion Constructors
    }
    #endregion IndexedRateFeature

    #region KnockFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class KnockFeature : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockUnderlyer",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyers")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] knockUnderlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerBasketTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knockUnderlyerBasketType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum knockUnderlyerBasketType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice knockType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockTypeTriggerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("trigger", typeof(Trigger),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Trigger", IsVisible = true)]
        public Trigger knockTypeTrigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockTypeCorridorBarriersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("corridorBarriers", typeof(CorridorBarriers),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Corridor Barriers", IsVisible = true)]
        public CorridorBarriers knockTypeCorridorBarriers;
		[System.Xml.Serialization.XmlElementAttribute("knockCondition", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Condition")]
        public KnockConditionEnum knockCondition;
		[System.Xml.Serialization.XmlElementAttribute("knockEffectType", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Effect Type")]
        public KnockEffectTypeEnum knockEffectType;

        [System.Xml.Serialization.XmlElementAttribute("knockObservation",Order=8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation", IsClonable = true, IsMaster = true, IsChild = true)]
        public KnockObservation[] knockObservation;
		#endregion Members
        #region Constructors
        public KnockFeature()
        {
            knockTypeTrigger = new Trigger();
            knockTypeCorridorBarriers = new CorridorBarriers();
        }
        #endregion Constructors

        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion KnockFeature
    #region KnockObservation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class KnockObservation : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("observationType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Type")]
        public KnockObservationTypeEnum observationType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationPeriod",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Periods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period", IsClonable = true, IsChild = true)]
        public BusinessDateRange[] observationPeriod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationDates",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Dates")]
        public DateList observationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationSchedule",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Schedules")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Schedule", IsClonable = true, IsChild = true)]
        public EquitySchedule[] observationSchedule;
		#endregion Members
        #region Constructors
        public KnockObservation(){}
        #endregion Constructors

        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion KnockObservation
    #region KnockUnderlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class KnockUnderlyer : ItemGUI, IEFS_Array
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public EFS_RadioChoice knockUnderlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerQuotedCurrencyPairSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", typeof(QuotedCurrencyPair),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quoted Currency Pair", IsVisible = true)]
        public QuotedCurrencyPair knockUnderlyerQuotedCurrencyPair;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRate", typeof(UnderlyerFloatingRate),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate", IsVisible = true)]
        public UnderlyerFloatingRate knockUnderlyerFloatingRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerIndexSpecified;
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(UnderlyerIndex),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Index", IsVisible = true)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public UnderlyerIndex knockUnderlyerIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerSecurityCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityCode", typeof(UnderlyerSecurityCode),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Security Code", IsVisible = true)]
        [ControlGUI(Name = "Code", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public UnderlyerSecurityCode knockUnderlyerSecurityCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty knockUnderlyerNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockInformationSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knockInformationSource", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Source")]
        public InformationSource knockInformationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixingTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixingTime", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        public BusinessCenterTime fixingTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool associatedStrikeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("associatedStrike", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Associated Strike")]
        public EFS_Decimal associatedStrike;
		#endregion Members
        #region Constructors
        public KnockUnderlyer()
        {
            knockUnderlyerQuotedCurrencyPair = new QuotedCurrencyPair();
            knockUnderlyerFloatingRate = new UnderlyerFloatingRate();
            knockUnderlyerSecurityCode = new UnderlyerSecurityCode();
            knockUnderlyerIndex = new UnderlyerIndex();
            knockUnderlyerNone = new Empty();
        }
        #endregion Constructors

        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion KnockUnderlyer
    #region NbMaxValues
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class NbMaxValues : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesInSpecified;
		[System.Xml.Serialization.XmlElementAttribute("nbMaxValuesIn", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "In")]
        public EFS_Integer nbMaxValuesIn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesOutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("nbMaxValuesOut", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Out")]
        public EFS_Integer nbMaxValuesOut;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("nbMaxValuesType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NbMaxValuesTypeEnum nbMaxValuesType;
        #endregion Members
        #region Constructors
        public NbMaxValues() { }
        #endregion Constructors
    }
    #endregion NbMaxValues

    #region RoutingCreateElement
    public partial class RoutingCreateElement : IRoutingCreateElement
    {
    }
    #endregion
    #region RoutingPartyReference
    public partial class RoutingPartyReference : Routing
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hrefSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion

    #region StepUpProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class StepUpProvision : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice stepupExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise stepupExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise stepupExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise stepupExerciseEuropean;
		#endregion Members
	}
    #endregion StepUpProvision

    #region TriggerEffect
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class TriggerEffect : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("triggerEffectType", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Effect Type")]
        public KnockEffectTypeEnum triggerEffectType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPayoutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerPayout", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payout")]
        public TriggerPayout triggerPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPrincipalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerPrincipal", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Principal")]
        public TriggerPrincipal triggerPrincipal;
		#endregion Members
		#region Constructors
		public TriggerEffect()
        {
        }
        #endregion Constructors
    }
    #endregion TriggerEffect
    #region TriggerFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class TriggerFeature : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500)]
        public EFS_String description;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice trigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanTrigger", typeof(AmericanTrigger),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American", IsVisible = true)]
        public AmericanTrigger triggerAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanTrigger", typeof(EuropeanTrigger),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European", IsVisible = true)]
        public EuropeanTrigger triggerEuropean;

        [System.Xml.Serialization.XmlElementAttribute("triggerObservation",Order=4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation", IsClonable = true, IsMaster = true, IsChild = true)]
        public KnockObservation[] observation;

        [System.Xml.Serialization.XmlElementAttribute("triggerEffect",Order=5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effect", IsVisible = false)]
        public TriggerEffect effect;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effect")]
        public bool FillBalise;
		#endregion Members
        #region Constructors
        public TriggerFeature()
        {
            triggerAmerican = new AmericanTrigger();
            triggerEuropean = new EuropeanTrigger();
        }
        #endregion Constructors

        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Methods
    }
    #endregion TriggerFeature
    #region TriggerPayout
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class TriggerPayout : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentValueSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePaymentValue", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Feature")]
        public FeaturePayment featurePaymentValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureRateValueSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featureRateValue", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate")]
        public EFS_Decimal featureRateValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureSpreadValueSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featureSpreadValue", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread")]
        public EFS_Decimal featureSpreadValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureMultiplierValueSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featureMultiplierValue", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier")]
        public EFS_Decimal featureMultiplierValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePayoutFormulaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePayoutFormula", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula featurePayoutFormula;
		#endregion Members
		#region Constructors
		public TriggerPayout(){}
        #endregion Constructors
    }
    #endregion TriggerPayout
    #region TriggerPrincipal
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class TriggerPrincipal : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureNotionalPercentageSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featureNotionalPercentage", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional percentage")]
        public EFS_Decimal featureNotionalPercentage;
		[System.Xml.Serialization.XmlElementAttribute("featurePrincipalExchange", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Principal Exchange")]
        public PrincipalExchangeEnum featurePrincipalExchange;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeType", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Principal Exchange Type")]
        public PrincipalExchangeTypeEnum principalExchangeType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalPaymentDualCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("principalPaymentDualCurrency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Payment Dual Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency principalPaymentDualCurrency;
		#endregion Members
		#region Constructors
		public TriggerPrincipal(){}
        #endregion Constructors
    }
    #endregion TriggerPrincipal
    #region UnderlyerFloatingRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class UnderlyerFloatingRate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Order = 1)]
		[ControlGUI(Name = "Rate", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexTenorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
		#endregion Members
		#region Constructors
		public UnderlyerFloatingRate() { }
        #endregion Constructors
    }
    #endregion UnderlyerFloatingRate
    #region UnderlyerIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class UnderlyerIndex : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string indexScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Index)]
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
        #region Constructors
        public UnderlyerIndex()
        {
            indexScheme = "http://www.efs.org/spec/2005/index-2-0";
        }
        #endregion Constructors
    }
    #endregion UnderlyerIndex
    #region UnderlyerSecurityCode
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class UnderlyerSecurityCode : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string securityCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SecurityCode)]
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
        #region Constructors
        public UnderlyerSecurityCode()
        {
            securityCodeScheme = "http://www.efs.org/spec/2005/securityCode-2-0";
        }
        #endregion Constructors
    }
    #endregion UnderlyerSecurityCode
}
