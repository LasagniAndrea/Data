#region using directives
using System;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.Enum;
using FpML.Enum;
using FpML.v44.Shared;
using Tz = EFS.TimeZone;
#endregion using directives

namespace EfsML.v30.MiFIDII_Extended
{
    #region ActionType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// <summary>
    /// Reports a regulator-specific code for the action associated with this submission. Used, for example, to report the ESMA action type.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ActionType
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string actionTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ActionType

    #region AdmissionToTrading
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// <summary>
    /// Information about whether and when a product was admitted to trading on a facility.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class AdmissionToTrading : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Requested")]
        [System.Xml.Serialization.XmlElementAttribute("requested", Order = 1)]
        /// Whether the issuer of the financial instrument has requested or approved the trading or 
        /// admission to trading of their financial instruments on a trading venue.
        public EFS_Boolean requested;

        [System.Xml.Serialization.XmlElementAttribute("requestDate", Order = 2)]
        /// Date and time of the request for admission to trading on the trading venue.
        public string RequestDate
        {
            set { _requestDate = new EFS_DateTimeOffset(value); }
            get { return RequestDateSpecified ? _requestDate.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RequestDateSpecified
        {
            get { return (null != _requestDate) && _requestDate.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _requestDateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Request date", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_DateTimeOffset _requestDate;

        [System.Xml.Serialization.XmlElementAttribute("approvalDate", Order = 3)]
        /// Date and time the issuer has approved admission to trading or trading in its financial instruments on a trading venue.
        public string ApprovalDate
        {
            set { _requestDate = new EFS_DateTimeOffset(value); }
            get { return ApprovalDateSpecified ? _approvalDate.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApprovalDateSpecified
        {
            get { return (null != _approvalDate) && _approvalDate.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _approvalDateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Approval date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_DateTimeOffset _approvalDate;

        [System.Xml.Serialization.XmlElementAttribute("admissionDate", Order = 4)]
        /// Date and time of the admission to trading on the trading venue or the date and time when the instrument was first traded or an order or quote was first received by the trading venue.
        public string AdmissionDate
        {
            set { _admissionDate = new EFS_DateTimeOffset(value); }
            get { return AdmissionDateSpecified ? _admissionDate.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AdmissionDateSpecified
        {
            get { return (null != _admissionDate) && _admissionDate.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _admissionDateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Admission date")]
        public EFS_DateTimeOffset _admissionDate;

        [System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 5)]
        /// Date and time when the financial instrument ceases to be traded or to be admitted to trading on the trading venue.
        public string TerminationDate
        {
            set { _terminationDate = new EFS_DateTimeOffset(value); }
            get { return TerminationDateSpecified ? _terminationDate.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TerminationDateSpecified
        {
            get { return (null != _terminationDate) && _terminationDate.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _terminationDateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Termination date")]
        public EFS_DateTimeOffset _terminationDate;
        #endregion Members
    }
    #endregion AdmissionToTrading

    #region Algorithm
    /// <summary>
    /// Provides information about an algorithm that executed or otherwise participated in this trade
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// FI 20170928 [23452] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Algorithm
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("name", Order = 1)]
        [ControlGUI(Name = "Name", Width = 500)]
        /// The name of the algorithm.
        public EFS_String name;
        [System.Xml.Serialization.XmlElementAttribute("role", Order = 2)]
        [ControlGUI(Name = "Role", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// The category of the function of the algorithm. 
        /// The related individual performs the role specified in this field for the base party. 
        /// For example, if the role is "Trader", the related person acts or acted as the base party's trader.
        public AlgorithmRole role;

        // FI 20170928 [23452] ajout de otcmlId
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        #endregion Members
    }
    #endregion Algorithm

    #region AlgorithmRole
    /// <summary>
    /// A type describing a role played by an algorithm in one or more transactions. 
    /// Examples include roles such as TradingDecision, RoutingDecision. This can be extended to provide custom roles.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AlgorithmRole : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string algorithmRoleScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion AlgorithmRole

    #region BusinessUnit
    /// <summary>
    /// A type that represents information about a unit within an organization.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BusinessUnit : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("name", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Name")]
        /// A name used to describe the organization unit
        public EFS_String name;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessUnitIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessUnitId", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business unit Id")]
        /// An identifier used to uniquely identify organization unit
        public Unit businessUnitId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contactInfoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contactInfo", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contact information")]
        /// Information on how to contact the unit using various means.
        public ContactInformation contactInfo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool countrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("country", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country")]
        /// The ISO 3166 standard code for the country where the individual works.
        public Country country;
        #endregion Members
    }
    #endregion BusinessUnit

    #region BusinessUnitReference
    /// <summary>
    /// Reference to an organizational unit.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BusinessUnitReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion BusinessUnitReference

    #region ContactInformation
    /// <summary>
    /// A type that represents how to contact an individual or organization.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContactInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool telephoneSpecified;
        [System.Xml.Serialization.XmlElementAttribute("telephone", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Telephones")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Telephones")]
        /// A telephonic contact.
        public TelephoneNumber[] telephone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool emailSpecified;
        [System.Xml.Serialization.XmlElementAttribute("email", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Emails")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Emails")]
        /// An address on an electronic mail or messaging system .
        public EFS_StringArray[] email;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool addressSpecified;
        [System.Xml.Serialization.XmlElementAttribute("address", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Address")]
        /// A postal or street address.
        public Address address;
        #endregion Members
    }
    #endregion ContactInformation

    #region CreditRating
    /// <summary>
    /// The party's credit rating.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CreditRating : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string creditRatingScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion CreditRating

    #region CurrencyPairClassification
    /// <summary>
    /// A type containing a code representing the risk classification of a currency pair, as specified by a regulator.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CurrencyPairClassification : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string currencyPairClassificationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion CurrencyPairClassification

    #region EntityClassification
    /// <summary>
    /// A type describing the entity of a party, for example Financial, NonFinancial etc.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EntityClassification : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string entityClassificationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion EntityClassification

    #region FacilityExecutionExceptionDeclaration
    /// <summary>
    /// Provides supporting evidence when a party invoked exception to not execute the trade on facility such as SEF and DCM 
    /// even though the particular product is mandated to execute on a SEF.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FacilityExecutionExceptionDeclaration : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reasonSpecified;
        [System.Xml.Serialization.XmlElementAttribute("reason", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reason")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reason")]
        /// Reason for not executing the trade on SEF or other facility.
        public EFS_StringArray[] reason;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool organizationCharacteristicSpecified;
        [System.Xml.Serialization.XmlElementAttribute("organizationCharacteristic", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Organization characteristic")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Organization characteristic")]
        /// Allows the organization to specify which categories or characteristics apply to it for end-user exception determination. 
        /// Examples include "FinancialEntity", "CaptiveFinanceUnit", "BoardOfDirectorsApproval".
        public OrganizationCharacteristic[] organizationCharacteristic;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transactionCharacteristicSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transactionCharacteristic", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transaction characteristic")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transaction characteristic")]
        /// Allows the relevant transaction level categories or characteristics to be recorded for end-user exception determination. 
        /// Examples include "BoardOfDirectorsApproval", "HedgesCommercialRisk".
        public TransactionCharacteristic[] transactionCharacteristic;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool supervisorRegistrationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("supervisorRegistration", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supervisor registration")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supervisor registration")]
        /// Allows the organization to specify which if any relevant regulators it is registered with, and if so their identification number. 
        /// For example, it could specify that it is SEC registered and provide its Central Index Key.
        public SupervisorRegistration[] supervisorRegistration;
        #endregion Members
    }
    #endregion FacilityExecutionExceptionDeclaration

    #region IndustryClassification
    /// <summary>
    /// A party's industry sector classification.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class IndustryClassification : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string industryClassificationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion IndustryClassification

    #region NotionalReportingType
    /// <summary>
    /// How a notional is to be reported for this reporting regime. E.g. for ESMA EMIR, it would be Nominal or Monetary Amount
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class NotionalReportingType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string notionalTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion NotionalReportingType

    #region OrganizationCharacteristic
    /// <summary>
    /// A characteristic of an organization used in declaring an end-user exception.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OrganizationCharacteristic : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string organizationCharacteristicScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion OrganizationCharacteristic

    #region OrganizationType
    /// <summary>
    /// A code that describes what type of role an organization plays, for example a SwapsDealer, a Major Swaps Participant, or Other
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OrganizationType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string organizationTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion OrganizationType

    #region OtcClassification
    /// <summary>
    /// Indicator as to the type of transaction in accordance with Articles 20(3)(a) and 21(5)(a) of Regulation (EU) 600/2014.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OtcClassification : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string otcClassificationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion OtcClassification

    #region PartyEntityClassification
    /// <summary>
    /// Indicates the category or classification or business role of the organization referenced by the partyTradeInformation 
    /// with respect to this reporting regime, for example Financial, NonFinancial etc.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyEntityClassification : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        /// A pointer style reference to a party identifier defined elsewhere in the document. 
        /// The party referenced has the classification in the associated "entityClassification" element below.
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlElementAttribute("entityClassification", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity classification")]
        /// Indicates the category or classification or business role of the organization referenced by the partyTradeInformation 
        /// with respect to this reporting regime, for example Financial, NonFinancial etc.
        public EntityClassification entityClassification;
        #endregion Members
    }
    #endregion PartyEntityClassification

    #region PartyRelationshipType
    /// <summary>
    /// A type containing a code representing how two parties are related, e.g. Affiliated, Intragroup, None.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyRelationshipType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string partyRelationshipTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion PartyRelationshipType

    #region PartyRoleCode
    /// <summary>
    /// A type describing a role played by a party in one or more transactions. 
    /// Examples include roles such as guarantor, custodian, confirmation service provider, etc. 
    /// This can be extended to provide custom roles.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II (= PartyRole v.5.1 vs PartyRole v4.4)
    /// EG 20171016 [23342] Upd partyRoleScheme
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyRoleCode : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("partyRoleScheme", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string partyRoleScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PartyRoleCode

    #region PartyRoleType
    /// <summary>
    /// A type refining the role a role played by a party in one or more transactions. 
    /// Examples include "AllPositions" and "SomePositions" for Guarantor. This can be extended to provide custom types.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyRoleType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string partyRoleTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PartyRoleType

    #region Person
    /// <summary>
    /// A type that represents information about a person connected with a trade or business process.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Person
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool honorificSpecified;
        [System.Xml.Serialization.XmlElementAttribute("honorific", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Honorific")]
        public EFS_String honorific;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstName", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First name", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_String firstName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "add")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMiddleNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("middleName", typeof(EFS_StringArray), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Middle name")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Middle name")]
        public EFS_StringArray[] itemMiddleName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemInitialSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initial", typeof(EFS_StringArray), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial")]
        public EFS_StringArray[] itemInitial;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool surnameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("surname", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Surname")]
        public EFS_String surname;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool suffixSpecified;
        [System.Xml.Serialization.XmlElementAttribute("suffix", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Suffix", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_String suffix;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool personIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("personId", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Person id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Person id")]
        public PersonId[] personId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessUnitReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("businessUnitReference", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business unit reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessUnit)]
        /// The unit for which the indvidual works.
        public BusinessUnitReference businessUnitReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contactInfoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contactInfo", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contact information")]
        /// Information on how to contact the individual using various means.
        public ContactInformation contactInfo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateOfBirthSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateOfBirth", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Date of birthday")]
        /// The birth date of the person, e.g. 1970-01-01
        public EFS_Date dateOfBirth;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool countrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("country", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Country")]
        /// The ISO 3166 standard code for the country where the individual works.
        public Country country;

        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion Person

    #region PersonId
    /// <summary>
    /// An identifier used to identify an individual person.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PersonId : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string personIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PersonId

    #region PersonReference
    /// <summary>
    /// Reference to an individual.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PersonReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PersonReference

    #region PersonRole
    /// <summary>
    /// A type describing a role played by a person in one or more transactions. 
    /// Examples include roles such as Trader, Broker, MiddleOffice, Legal, etc. 
    /// This can be extended to provide custom roles.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PersonRole : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string personRoleScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PersonRole

    #region ProductSummary
    /// <summary>
    /// Summary information about the product that was traded. This is intended primarily for trade reporting by TRs.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ProductSummary : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("finalSettlementDate", Order = 1)]
        public string FinalSettlementDate
        {
            set { _finalSettlementDate = new EFS_DateTimeOffset(value); }
            get { return FinalSettlementDateSpecified ? _finalSettlementDate.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FinalSettlementDateSpecified
        {
            get { return (null != _finalSettlementDate) && _finalSettlementDate.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _finalSettlementDateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Final settlement date")]
        /// Settlement date
        public EFS_DateTimeOffset _finalSettlementDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Settlement type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// Shows how the transaction is to be settled when it is exercised.
        public SettlementTypeExtendedEnum settlementType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool senioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("seniority", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Repayment precedence of a debt instrument", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// The repayment precedence of a debt instrument.
        public CreditSeniority seniority;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexFactor", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(IsLabel = false, Level = MethodsGUI.LevelEnum.None, Name = "Index factor", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal indexFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool admissionToTradingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("admissionToTrading", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Admission to trading")]
        /// Information about whether and when a product was admitted to trading on a facility.
        public AdmissionToTrading admissionToTrading;
        #endregion Members
    }
    #endregion ProductSummary

    #region Region
    /// <summary>
    /// A code that describes the world region of a counterparty. 
    /// For example, NorthAmerica, Europe, Asia.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Region : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string regionScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion Region

    #region RegulatorId
    /// <summary>
    /// An ID assigned by a regulator to an organization registered with it. 
    /// (NOTE: should this just by represented by an alternate party ID?)
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RegulatorId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string regulatorIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion RegulatorId

    #region RelatedParty
    /// <summary>
    /// This may be used to identify one or more parties that perform a role within the transaction. 
    /// If this is within a partyTradeInformation block, the related party performs the role with respect to 
    /// the party identifie by the "partyReference" in the partyTradeInformation block.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RelatedParty : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Name = "Party")]
        /// Reference to a party.
        public PartyReference partyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountReference", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AccountReference accountReference;

        [System.Xml.Serialization.XmlElementAttribute("role", Order = 3)]
        [ControlGUI(Name = "Role")]
        /// The category of the relationship. The related party performs the role specified in this field for the base party. 
        /// For example, if the role is "Guarantor", the related party acts as a guarantor for the base party.
        public PartyRoleCode role;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// Additional definition refining the type of relationship. 
        /// For example, if the "role" is Guarantor, this element may be used to specify whether all positions are guaranteed, 
        /// or only a subset of them.
        public PartyRoleType type;
        #endregion Members
    }
    #endregion RelatedParty

    #region ReportingBoolean
    /// <summary>
    /// How a Boolean value is to be reported for this regulator. 
    /// Typically "true" or "false", but for ESMA "X" is also allowed, to indicate not supplied.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReportingBoolean : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string reportingBooleanScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ReportingBoolean

    #region ReportingPurpose
    /// <summary>
    /// A value that explains the reason or purpose that information is being reported. 
    /// Examples might include RealTimePublic reporting, PrimaryEconomicTerms reporting, Confirmation reporting, or Snapshot reporting.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReportingPurpose : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string reportingPurposeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ReportingPurpose

    #region RelatedPerson
    /// <summary>
    /// Provides information about a person that executed or supports this trade
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RelatedPerson : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("personReference", Order = 1)]
        [ControlGUI(Name = "person", LblWidth = 50)]
        /// The individual person that is related to this.
        public PersonReference personReference;
        [System.Xml.Serialization.XmlElementAttribute("role", Order = 2)]
        [ControlGUI(Name = "Role", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// The category of the relationship. The related individual performs the role specified in this field for the base party. 
        /// For example, if the role is "Trader", the related person acts acts or acted as the base party's trader.
        public PersonRole role;
        #endregion Members
    }
    #endregion RelatedPerson

    #region ReportingRegime
    /// <summary>
    /// Provides information about how the information in this message is applicable to a regulatory reporting process.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReportingRegime
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("name", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Name")]
        /// Identifies the reporting regime under which this data is reported. For example, Dodd-Frank, MiFID, HongKongOTCDRepository, ODRF
        public ReportingRegimeName name;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool supervisorRegistrationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("supervisorRegistration", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supervisor registration")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supervisor registration")]
        /// Identifies the specific regulator or other supervisory body for which this data is produced. 
        /// For example, CFTC, SEC, UKFSA, ODRF, SFC, ESMA.
        public SupervisorRegistration[] supervisorRegistration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reportingRoleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("reportingRole", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reporting role")]
        /// Identifies the role of this party in reporting this trade for this regulator; roles could include ReportingParty and Voluntary reporting.
        public ReportingRole reportingRole;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool reportingPurposeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("reportingPurpose", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reporting purpose")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reporting purpose")]
        /// The reason this message is being sent, for example Snapshot, PET, Confirmation, RealTimePublic.
        public ReportingPurpose[] reportingPurpose;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatorilyClearableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatorilyClearable", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mandatorily clearable")]
        /// Whether the particular trade type in question is required by this regulator to be cleared.
        public ReportingBoolean mandatorilyClearable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatoryFacilityExecutionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatoryFacilityExecution", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mandatory facility execution")]
        /// Whether the particular product must be executed on a SEF or DCM. See to Dodd-Frank section 723(a)(8).
        public EFS_Boolean mandatoryFacilityExecution;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatoryFacilityExecutionExceptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatoryFacilityExecutionException", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mandatory facility execution exception")]
        /// Specifies whether the party invoked exception to not execute the trade on facility such as SEF and DCM 
        /// even though the particular product is mandated to execute on a SEF.
        public bool mandatoryFacilityExecutionException;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatoryFacilityExecutionExceptionDeclarationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatoryFacilityExecutionExceptionDeclaration", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mandatory facility execution exception declaration")]
        /// Provides supporting evidence when a party invoked exception to not execute the trade on facility such as SEF and DCM 
        /// even though the particular product is mandated to execute on a SEF.
        public FacilityExecutionExceptionDeclaration mandatoryFacilityExecutionExceptionDeclaration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exceedsClearingThresholdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exceedsClearingThreshold", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exceeds clearing threshold")]
        /// Indicates whether the counterparty exceeds the volume threshold above which trades are required to be cleared.
        public EFS_Boolean exceedsClearingThreshold;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Classification")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemEntityClassificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityClassification", typeof(EntityClassification), Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity")]
        /// Indicates the category or classification or business role of the organization referenced by the partyTradeInformation 
        /// with respect to this reporting regime, for example Financial, NonFinancial etc.
        public EntityClassification itemEntityClassification;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPartyEntityClassificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyEntityClassification", typeof(PartyEntityClassification), Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsLabel = false, Level = MethodsGUI.LevelEnum.End)]
        /// Indicates the category or classification or business role of a trade party with respect to this reporting regime, 
        /// for example Financial, NonFinancial, Dealer, Non-Dealer, LocalParty, etc.
        public PartyEntityClassification itemPartyEntityClassification;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradePartyRelationshipTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradePartyRelationshipType", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade party relationship type")]
        /// Indicates how the parties to the trade (the counterparties) are related to each other with respect to this reporting regime, 
        /// e.g. Affiliated, Intragroup, etc..
        public PartyRelationshipType tradePartyRelationshipType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool actionTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("actionType", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Action type")]
        /// Reports a regulator-specific code for the action associated with this submission. Used, for example, to report the ESMA action type.
        public ActionType actionType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool preEnactmentTradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("preEnactmentTrade", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pre enactment trade")]
        /// Reports that this trade was executed prior to the enactment of the relevant reporting regulation.
        public EFS_Boolean preEnactmentTrade;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalType", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional type")]
        /// How the notional amount should be reported for the reporting regime. 
        /// For example, for ESMA MiFIR it would be Nominal or MonetaryAmount.
        public NotionalReportingType notionalType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyPairClassificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyPairClassification", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency pair classification")]
        /// Reports a regulator-specific code classifying the currency pair in the trade into risk categories such as Major Currencies or Emerging Markets.
        public CurrencyPairClassification currencyPairClassification;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transmissionOfOrderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transmissionOfOrder", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transmission of order")]
        public EFS_Boolean transmissionOfOrder;
        #endregion Members
    }
    #endregion ReportingRegime

    #region ReportingRegimeName
    /// <summary>
    /// An identifier of an reporting regime or format used for regulatory reporting, 
    /// for example DoddFrankAct, MiFID, HongKongOTCDRepository, etc.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReportingRegimeName : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string reportingRegimeNameScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ReportingRegimeName

    #region ReportingRole
    /// <summary>
    /// A type containing a code representing the role of a party in a report, 
    /// e.g. the originator, the recipient, the counterparty, etc. 
    /// This is used to clarify which participant's information is being reported.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReportingRole : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string reportingRoleScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion ReportingRole

    #region ShortSale
    /// <summary>
    /// A short sale concluded by an investment firm on its own behalf or on behalf of a client, as described in Article 11.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ShortSale : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string shortSaleScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ShortSale

    #region SupervisorBody
    /// <summary>
    /// An identifier of an organization that supervises or regulates trading activity, e.g. CFTC, SEC, FSA, ODRF, etc.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SupervisoryBody : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string supervisoryBodyScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion SupervisorBody

    #region SupervisorRegistration
    /// <summary>
    /// Provides information about a regulator or other supervisory body that an organization is registered with.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SupervisorRegistration
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("supervisoryBody", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Supervisory body")]
        /// The regulator or other supervisory body the organization is registered with (e.g. SEC).
        public SupervisoryBody supervisoryBody;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool registrationNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("timestamp", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Registration number")]
        /// The ID assigned by the regulator (e.g. SEC's Central Index Key).
        public RegulatorId registrationNumber;
        #endregion Members
    }
    #endregion SupervisorRegistration

    #region TelephoneNumber
    /// <summary>
    /// A type that represents a telephonic contact.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TelephoneNumber : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        [ControlGUI(Name = "Type")]
        /// The type of telephone number (work, personal, mobile).
        public TelephoneTypeEnum type;
        [System.Xml.Serialization.XmlElementAttribute("number", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number", LineFeed = MethodsGUI.LineFeedEnum.After)]
        /// A telephonic contact.
        public EFS_String number;
        #endregion Members
    }
    #endregion TelephoneNumber

    #region TradeCategory
    /// <summary>
    /// A scheme used to categorize positions.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeCategory : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string categoryScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion TradeCategory

    #region TradeTimestamp
    /// <summary>
    /// A generic trade timestamp
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeTimestamp : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        [ControlGUI(Name = "type", LblWidth = 50)]
        /// The type or meaning of a timestamp.
        public TimestampTypeScheme type;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_DateTimeOffset _value;
        [System.Xml.Serialization.XmlElementAttribute("value", Order = 2)]
        public string Value
        {
            set { _value = new EFS_DateTimeOffset(value); }
            get { return (null != _value) && _value.DateTimeValue.HasValue ? _value.ISODateTimeValue : string.Empty; }
        }

        #endregion Members
    }
    #endregion TradeTimestamp

    #region TimestampTypeScheme
    /// <summary>
    /// The type or meaning of a timestamp.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TimestampTypeScheme : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string timestampScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion TimestampTypeScheme

    #region TradeProcessingTimestamps
    /// <summary>
    /// Allows timing information about when a trade was processed and reported to be recorded.
    /// </summary>
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // EG 20171025 [23509] Upd 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeProcessingTimestamps : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("orderEntered", Order = 1)]
        /// When an order was first generated, as recorded for the first time when it was first entered by a person 
        /// or generated by a trading algorithm (i.e., the first record of the order).
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string OrderEntered
        {
            set { _orderEntered = new EFS_DateTimeOffset(value); }
            get { return (null != _orderEntered) ? _orderEntered.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrderEnteredSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _orderEnteredSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "First record of the order", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _orderEntered;


        [System.Xml.Serialization.XmlElementAttribute("orderSubmitted", Order = 2)]
        /// The time when an order is submitted by a market participant to an execution facility, 
        /// as recorded based on the timestamp of the message that was sent by the participant. 
        /// If the participant records this time (i.e. it is in the participant's party trade information), 
        /// it will be the time the message was sent. 
        /// If the execution facility records this time (i.e. it is in the facility's party trade information), 
        /// it will be the time the message was received.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string OrderSubmitted
        {
            set { _orderSubmitted = new EFS_DateTimeOffset(value);}
            //get { return orderSubmittedSpecified ? _orderSubmitted.ISODateTimeValue : string.Empty; }
            get { return (null != _orderSubmitted) ? _orderSubmitted.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrderSubmittedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(OrderSubmitted); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _orderSubmittedSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Order submitted", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _orderSubmitted;

        [System.Xml.Serialization.XmlElementAttribute("publiclyReported", Order = 3)]
        /// When the public report of this was created or received by this party. 
        /// If the participant records this time (i.e. it is in the participant's party trade information), 
        /// it will be the time the message was sent. If the execution records this time (i.e. it is in the facility's party trade information), 
        /// it will be the time the message was received.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string PubliclyReported
        {
            set { _publiclyReported = new EFS_DateTimeOffset(value); }
            get { return (null != _publiclyReported) ? _publiclyReported.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PubliclyReportedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(PubliclyReported); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _publiclyReportedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Publicly reported", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _publiclyReported;

        [System.Xml.Serialization.XmlElementAttribute("publicReportAccepted", Order = 4)]
        /// When the public report of this was most recently corrected or corrections were sent or received by this party.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string PublicReportAccepted
        {
            set { _publicReportAccepted = new EFS_DateTimeOffset(value); }
            get { return (null != _publicReportAccepted) ? _publicReportAccepted.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PublicReportAcceptedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(PublicReportAccepted); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _publicReportAcceptedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Public report accepted", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _publicReportAccepted;

        [System.Xml.Serialization.XmlElementAttribute("publicReportUpdated", Order = 5)]
        /// When the public report of this was first accepted for submission to a regulator.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string PublicReportUpdated
        {
            set { _publicReportUpdated = new EFS_DateTimeOffset(value); }
            get { return (null != _publicReportUpdated) ? _publicReportUpdated.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PublicReportUpdatedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(PublicReportUpdated); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _publicReportUpdatedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Public report updated", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _publicReportUpdated;

        [System.Xml.Serialization.XmlElementAttribute("nonpubliclyReported", Order = 6)]
        /// When the non-public report of this was created or received by this party.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string NonpubliclyReported
        {
            set { _nonpubliclyReported = new EFS_DateTimeOffset(value); }
            get { return (null != _nonpubliclyReported) ? _nonpubliclyReported.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NonpubliclyReportedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(NonpubliclyReported); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _nonpubliclyReportedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Non publicly reported", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _nonpubliclyReported;

        [System.Xml.Serialization.XmlElementAttribute("nonpublicReportAccepted", Order = 7)]
        /// When the non-public report of this was first accepted for submission to a regulator.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string NonpublicReportAccepted
        {
            set { _nonpublicReportAccepted = new EFS_DateTimeOffset(value); }
            get { return (null != _nonpublicReportAccepted) ? _nonpublicReportAccepted.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NonpublicReportAcceptedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(NonpublicReportAccepted); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _nonpublicReportAcceptedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Non public report accepted", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _nonpublicReportAccepted;

        [System.Xml.Serialization.XmlElementAttribute("nonpublicReportUpdated", Order = 8)]
        /// When the non-public report of this was most recently corrected or corrections were received by this party.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string NonpublicReportUpdated
        {
            set { _nonpublicReportUpdated = new EFS_DateTimeOffset(value); }
            get { return (null != _nonpublicReportUpdated) ? _nonpublicReportUpdated.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NonpublicReportUpdatedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(NonpublicReportUpdated); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _nonpublicReportUpdatedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Non public report updated", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _nonpublicReportUpdated;

        [System.Xml.Serialization.XmlElementAttribute("submittedForConfirmation", Order = 9)]
        /// When this trade was supplied to a confirmation service or counterparty for confirmation.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string SubmittedForConfirmation
        {
            set { _submittedForConfirmation = new EFS_DateTimeOffset(value); }
            get { return (null != _submittedForConfirmation) ? _submittedForConfirmation.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubmittedForConfirmationSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(SubmittedForConfirmation); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _submittedForConfirmationSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Submitted for confirmation", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _submittedForConfirmation;

        [System.Xml.Serialization.XmlElementAttribute("updatedForConfirmation", Order = 10)]
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string UpdatedForConfirmation
        {
            set { _updatedForConfirmation = new EFS_DateTimeOffset(value); }
            get { return (null != _updatedForConfirmation) ? _updatedForConfirmation.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UpdatedForConfirmationSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(SubmittedForConfirmation); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _updatedForConfirmationSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Updated for confirmation", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _updatedForConfirmation;

        [System.Xml.Serialization.XmlElementAttribute("confirmed", Order = 11)]
        /// When this trade was confirmed.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string Confirmed
        {
            set { _confirmed = new EFS_DateTimeOffset(value); }
            get { return (null != _confirmed) ? _confirmed.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConfirmedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(Confirmed); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _confirmedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Confirmed trade", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _confirmed;

        [System.Xml.Serialization.XmlElementAttribute("submittedForClearing", Order = 12)]
        /// When this trade was supplied to a clearing service for clearing.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string SubmittedForClearing
        {
            set { _submittedForClearing = new EFS_DateTimeOffset(value); }
            get { return (null != _submittedForClearing) ? _submittedForClearing.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubmittedForClearingSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(SubmittedForClearing); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _submittedForClearingSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Submitted for clearing", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _submittedForClearing;

        [System.Xml.Serialization.XmlElementAttribute("updatedForClearing", Order = 13)]
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string UpdatedForClearing
        {
            set { _updatedForClearing = new EFS_DateTimeOffset(value); }
            get { return (null != _updatedForClearing) ? _updatedForClearing.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UpdatedForClearingSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(UpdatedForClearing); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _updatedForClearingSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Updated for clearing", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _updatedForClearing;

        [System.Xml.Serialization.XmlElementAttribute("cleared", Order = 14)]
        /// When this trade was cleared.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string Cleared
        {
            set { _cleared = new EFS_DateTimeOffset(value); }
            get { return (null != _cleared) ? _cleared.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ClearedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(Cleared); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _clearedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Cleared trade", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _cleared;

        [System.Xml.Serialization.XmlElementAttribute("allocationsSubmitted", Order = 15)]
        /// When allocations for this trade were submitted or received by this party.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string AllocationsSubmitted
        {
            set { _allocationsSubmitted = new EFS_DateTimeOffset(value); }
            get { return (null != _allocationsSubmitted) ? _allocationsSubmitted.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocationsSubmittedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(AllocationsSubmitted); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _allocationsSubmittedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Submitted allocations", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _allocationsSubmitted;

        [System.Xml.Serialization.XmlElementAttribute("allocationsUpdated", Order = 16)]
        /// When allocations for this trade were most recently corrected.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string AllocationsUpdated
        {
            set { _allocationsUpdated = new EFS_DateTimeOffset(value); }
            get { return (null != _allocationsUpdated) ? _allocationsUpdated.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocationsUpdatedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(AllocationsUpdated); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _allocationsUpdatedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Updated allocations", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _allocationsUpdated;

        [System.Xml.Serialization.XmlElementAttribute("allocationsCompleted", Order = 17)]
        /// When allocations for this trade were completely processed.
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public string AllocationsCompleted
        {
            set { _allocationsCompleted = new EFS_DateTimeOffset(value); }
            get { return (null != _allocationsCompleted) ? _allocationsCompleted.GetValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllocationsCompletedSpecified
        {
            get { return EFS.ACommon.StrFunc.IsFilled(AllocationsCompleted); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _allocationsCompletedSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Completed allocations", LblWidth = 150, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = false)]
        public EFS_DateTimeOffset _allocationsCompleted;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timestampSpecified;
        [System.Xml.Serialization.XmlElementAttribute("timestamp", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other timestamps")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Other timestamps")]
        /// Other timestamps for this trade. This is provisional in Recordkeeping and Transparency view and may be reviewed in a subsequent draft.
        public TradeTimestamp[] timestamp;
        #endregion Members
    }
    #endregion TradeProcessingTimestamps

    #region TradingWaiver
    /// <summary>
    /// Indication as to whether the transaction was executed under a pre-trade waiver in accordance with Articles 4 and 9 of Regulation (EU) 600/2014.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradingWaiver : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string tradingWaiverScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion TradingWaiver

    #region TransactionCharacteristic
    /// <summary>
    /// A characteristic of a transaction used in declaring an end-user exception.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TransactionCharacteristic : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string transactionCharacteristicScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion TransactionCharacteristic

    #region Unit
    /// <summary>
    /// A type used to record information about a unit, subdivision, desk, or other similar business entity.
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Unit : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string unitScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion Unit

}
