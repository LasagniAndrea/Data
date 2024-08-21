#region using directives
using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

 

using EfsML.Enum;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Shared;
using EfsML.v30.Repository; 

using FpML.Enum;
using FpML.Interface;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.Doc
{

    #region ActorId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ActorId : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string actorIdScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlIgnore()]
        public bool actorNameSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("actorName", DataType = "normalizedString")]
        public string actorName;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion ActorId

    #region BookId
    /// <summary>
    /// Représente un book
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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

    #region Css
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
        [System.Xml.Serialization.XmlElementAttribute("cssInfo", typeof(CssInfo), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "CssInfo", IsVisible = true)]
        public CssInfo cssCriteriaCssInfo;
        #endregion Members
    }
    #endregion CssCriteria
    #region CssInfo
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
        [System.Xml.Serialization.XmlElementAttribute("cssType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssType cssType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssSystemTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssSystemType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "System type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssSystemType cssSystemType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssSettlementTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssSettlementType", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssSettlementType cssSettlementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cssPaymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cssPaymentType", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CssPaymentType cssPaymentType;
        #endregion Members
    }
    #endregion CssInfo
    #region CssPaymentType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("EfsML", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class EfsDocument : DataDocument
    {
        #region Members
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string efs_schemaLocation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool repositorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("repository", Order = 1)]
        public EfsML.v30.Repository.Repository repository;		
        #endregion Members
    }
    #endregion EfsDocument
    
    #region EfsSettlementInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EfsSettlementInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice information;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationStandardSpecified;

        [System.Xml.Serialization.XmlElementAttribute("standardSettlementStyle", typeof(StandardSettlementStyleEnum), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Style")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Standard", IsVisible = true)]
        public StandardSettlementStyleEnum informationStandard;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationInstructionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", typeof(EfsSettlementInstruction), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Instruction", IsVisible = true)]
        public EfsSettlementInstruction informationInstruction;
        #endregion Members
    }
    #endregion EfsSettlementInformation
    #region EfsSettlementInstruction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("EfsSettlementInstruction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class EfsSettlementInstruction : SettlementInstruction
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementMethodInformation", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Method Information")]
        public Routing settlementMethodInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool investorInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("investorInformation", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Investor Information")]
        public Routing investorInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool originatorInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("originatorInformation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Originator Information")]
        public Routing originatorInformation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idssiDb;  // ISSIDatabase (used by SIGENPROCESS)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idIssi;	  // IDISSI used In case of internalDatabase (used by SIGENPROCESS)
        #endregion Members
    }
    #endregion EfsSettlementInstruction
    #region EventCode
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EventCodes : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool productReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("productReference", Order = 1)]
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
        [System.Xml.Serialization.XmlElementAttribute("streamId", Order = 2)]
        public EFS_NonNegativeInteger streamId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventCodeSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event code", Width = 400)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [System.Xml.Serialization.XmlElementAttribute("eventCode", Order = 3)]
        public EventCode eventCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eventTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventType", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event type", Width = 400)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EventType eventType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement date ")]
        public EFS_Date settlementDate;
        #endregion Members
    }
    #endregion EventCodes
    #region EventCodesSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EventCodesSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("eventCodes", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event Codes", IsClonable = true, IsChild = true, MinItem = 1)]
        public EventCodes[] eventCodes;
        #endregion Members
    }
    #endregion EventCodesSchedule

    #region FlowContext
    /// <summary>
    /// Class qui permet de définir un context
    /// La méthode IsMatchWith vérifie que l'évent est compatible avec le context
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
        //
        #endregion Members
    }
    #endregion FlowContext
    #region FxClass
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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

	#region HedgeClassDerv
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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

	#region LocalClassDerv
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class NettingInformationInput : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nettingMethod", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [ControlGUI(Name = "netting Method", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 250)]
        public NettingMethodEnum nettingMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nettingDesignationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nettingDesignation", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Netting Designation")]
        public NettingDesignation nettingDesignation;
        #endregion Members
    }
    #endregion NettingInformationInput

    
    #region SettlementInformationInput
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SettlementInformationInput : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party")]
        public EFS_RadioChoice partyReference;
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
    #endregion SettlementInformations
    #region SettlementInput
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SettlementInput : ItemGUI 
    {
        #region Members
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Context", IsVisible = true)]
        public FlowContext settlementContext;
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SsiCriteria : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ssicountrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("ssiCountry", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country", Width = 350)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Country ssiCountry;
        #endregion Members
    }
    #endregion SsiCriteria

    #region TradeIntention
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeIntention : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initiator", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "initiator")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "initiator", IsMaster=true,MinItem=1,MaxItem=2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public ArrayPartyReference[] initiator;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("reactor", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "reactor")]
        public PartyReference reactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion TradeIntention

}