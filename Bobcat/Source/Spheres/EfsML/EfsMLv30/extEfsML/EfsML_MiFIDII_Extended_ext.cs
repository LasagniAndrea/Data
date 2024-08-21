#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion using directives

namespace EfsML.v30.MiFIDII_Extended
{
    #region ActionType
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ActionType : IScheme
    {
        #region Constructors
        public ActionType()
        {
            actionTypeScheme = "http://www.fpml.org/coding-scheme/action-type";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.actionTypeScheme = value; }
            get { return this.actionTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ActionType

    #region Algorithm
    // EG 20170918 [23342] New FpML extensions for MiFID II
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Modify
    public partial class Algorithm : IEFS_Array, IAlgorithm, ISpheresId
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add constructor
        public Algorithm()
        {
            this.role = new AlgorithmRole();
            this.name = new EFS_String(); 
        }
        
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

        #region IAlgorithm // FI 20170928 [23452]
        string IAlgorithm.Name
        {
            get
            {
                return name.Value;
            }
            set
            {
                name = new EFS_String(value);
            }
        }
        IScheme IAlgorithm.Role
        {
            get
            {
                return role;
            }
            set
            {
                role = new AlgorithmRole()
                {
                    Value = value.Value,
                    algorithmRoleScheme = value.Scheme
                };
            }
        }
        #endregion

        #region ISpheresId // FI 20170928 [23452] Implémentation de ISpheresId
        string ISpheresId.OtcmlId
        {
            get
            {
                return this.otcmlId;
            }
            set
            {
                this.otcmlId = value;
            }
        }
        int ISpheresId.OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion
    }
    #endregion Algorithm

    #region AlgorithmRole
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class AlgorithmRole : IScheme
    {
        #region Constructors
        public AlgorithmRole()
        {
            algorithmRoleScheme = "http://www.fpml.org/coding-scheme/algorithm-role";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.algorithmRoleScheme = value; }
            get { return this.algorithmRoleScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion AlgorithmRole

    #region BusinessUnit
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class BusinessUnit : IEFS_Array
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion BusinessUnit

    #region BusinessUnitReference
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class BusinessUnitReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion BusinessUnitReference

    #region ContactInformation
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ContactInformation
    {
    }
    #endregion ContactInformation

    #region CreditRating
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class CreditRating : IScheme, IEFS_Array
    {
        #region Constructors
        public CreditRating() : this(string.Empty) { }
        public CreditRating(string pValue)
        {
            creditRatingScheme = "http://www.fpml.org/coding-scheme/external/moodys";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.creditRatingScheme = value; }
            get { return this.creditRatingScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

    }
    #endregion CreditRating

    #region CurrencyPairClassification
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class CurrencyPairClassification : ISchemeId
    {
        #region Constructors
        public CurrencyPairClassification()
        {
            currencyPairClassificationScheme = "http://www.fpml.org/coding-scheme/esma-currency-pair-classification";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.currencyPairClassificationScheme = value; }
            get { return this.currencyPairClassificationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

        string ISchemeId.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
    }
    #endregion CurrencyPairClassification

    #region EntityClassification
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class EntityClassification : IScheme
    {
        #region Constructors
        public EntityClassification()
        {
            entityClassificationScheme = "http://www.fpml.org/coding-scheme/entity-classification";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.entityClassificationScheme = value; }
            get { return this.entityClassificationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion EntityClassification

    #region IndustryClassification
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class IndustryClassification : IScheme, IEFS_Array
    {
        #region Constructors
        public IndustryClassification() : this(string.Empty) { }
        public IndustryClassification(string pValue)
        {
            industryClassificationScheme = "http://www.fpml.org/coding-scheme/regulatory-corporate-sector";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.industryClassificationScheme = value; }
            get { return this.industryClassificationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

    }
    #endregion IndustryClassification

    #region NotionalReportingType
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class NotionalReportingType : IScheme
    {
        #region Constructors
        public NotionalReportingType()
        {
            notionalTypeScheme = "http://www.fpml.org/coding-scheme/notional-type";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.notionalTypeScheme = value; }
            get { return this.notionalTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion NotionalReportingType

    #region OrganizationCharacteristic
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class OrganizationCharacteristic : IScheme, IEFS_Array
    {
        #region Constructors
        public OrganizationCharacteristic() : this(string.Empty) { }
        public OrganizationCharacteristic(string pValue)
        {
            organizationCharacteristicScheme = "http://www.fpml.org/coding-scheme/organization-characteristic";
            Value = pValue;
        }
        #endregion Constructors
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.organizationCharacteristicScheme = value; }
            get { return this.organizationCharacteristicScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

    }
    #endregion OrganizationCharacteristic

    #region OrganizationType
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class OrganizationType : IScheme
    {
        #region Constructors
        public OrganizationType()
        {
            organizationTypeScheme = "http://www.fpml.org/coding-scheme/organization-type";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.organizationTypeScheme = value; }
            get { return this.organizationTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion OrganizationType

    #region OtcClassification
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class OtcClassification : IScheme, IEFS_Array
    {
        #region Constructors
        public OtcClassification() : this(string.Empty) { }
        public OtcClassification(string pValue)
        {
            otcClassificationScheme = "http://www.fpml.org/coding-scheme/esma-mifir-otc-classification";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.otcClassificationScheme = value; }
            get { return this.otcClassificationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

    }
    #endregion OtcClassification

    #region PartyRelationshipType
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class PartyRelationshipType : ISchemeId
    {
        #region Constructors
        public PartyRelationshipType()
        {
            partyRelationshipTypeScheme = "http://www.fpml.org/coding-scheme/party-relationship-type";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.partyRelationshipTypeScheme = value; }
            get { return this.partyRelationshipTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

        string ISchemeId.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
    }
    #endregion PartyRelationshipType

    #region PartyRoleCode
    // EG 20170918 [23342] New FpML extensions for MiFID II (= PartyRole v.5.1 vs PartyRole v4.4)
    // EG 20171016 [23342] Upd partyRoleScheme
    public partial class PartyRoleCode : IScheme
    {
        #region Constructors
        public PartyRoleCode()
        {
            partyRoleScheme = "http://www.fpml.org/coding-scheme/party-role";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.partyRoleScheme = value; }
            get { return this.partyRoleScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion PartyRoleCode

    #region PartyRoleType
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class PartyRoleType : IScheme
    {
        #region Constructors
        public PartyRoleType()
        {
            partyRoleTypeScheme = "http://www.fpml.org/coding-scheme/party-role-type";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.partyRoleTypeScheme = value; }
            get { return this.partyRoleTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion PartyRoleType

    #region Person
    
    /// <summary>
    /// 
    /// </summary>
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// FI 20170928 [23452] Modify (Iperson)
    public partial class Person : IPerson, ISpheresId, IEFS_Array
    {
        #region Constructors
        public Person()
        {
            itemInitial = new EFS_StringArray[] { new EFS_StringArray() };
            itemMiddleName = new EFS_StringArray[] { new EFS_StringArray() };
            itemNone = new Empty();
            firstName = new EFS_String();
            surname = new EFS_String();
            personId = new PersonId[] { new PersonId() };
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

        #region ISpheresId // FI 20170928 [23452]
        string ISpheresId.OtcmlId
        {
            get
            {
                return this.otcmlId;
            }
            set
            {
                this.otcmlId = value;
            }
        }
        int ISpheresId.OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion

        #region IPerson Membres // FI 20170928 [23452]
        bool IPerson.FirstNameSpecified
        {
            get
            {
                return this.firstNameSpecified;
            }
            set
            {
                this.firstNameSpecified = value;
            }
        }
        string IPerson.FirstName
        {
            get
            {
                return this.firstName.Value;
            }
            set
            {
                firstName = new EFS_String(value);
            }
        }

        bool IPerson.SurnameSpecified
        {
            get
            {
                return this.surnameSpecified;
            }
            set
            {
                this.surnameSpecified = value;
            }
        }
        string IPerson.Surname
        {
            get
            {
                return this.surname.Value;
            }
            set
            {
                surname = new EFS_String(value);
            }
        }


        bool IPerson.PersonIdSpecified
        {
            get
            {
                return this.personIdSpecified;
            }
            set
            {
                this.personIdSpecified = value;
            }
        }
        // EG 20171016 [23452] Upd
        IScheme[] IPerson.PersonId
        {
            get { return personId; }
            set
            {
                //this.personId = null;
                //if (ArrFunc.IsFilled(value))
                //{
                //    this.personId = (from item in value
                //                     select new PersonId()
                //                     {
                //                         personIdScheme = item.scheme,
                //                         Value = item.Value
                //                     }).ToArray();
                //}
                if (0 < value.OfType<SchemeData>().Count())
                {
                    this.personId = (from item in value
                                     select new PersonId() {personIdScheme = item.Scheme, Value = item.Value}).ToArray();
                }
                else
                {
                    this.personId = value.Cast<PersonId>().ToArray();
                }
            }
        }


        string IPerson.Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }
        #endregion

        
    }
    #endregion Person

    #region PersonId
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class PersonId : IScheme, IEFS_Array
    {
        #region Constructors
        public PersonId() : this(string.Empty) { }
        public PersonId(string pValue)
        {
            personIdScheme = "http://www.euro-finance-systems.fr/otcml/personid";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.personIdScheme = value; }
            get { return this.personIdScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion PersonId

    #region PersonReference
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class PersonReference : ICloneable, IReference
    {
        #region Constructor
        public PersonReference() { }
        public PersonReference(string pPersonReference)
        {
            this.href = pPersonReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            PersonReference clone = new PersonReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion PersonReference

    #region PersonRole
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class PersonRole : IScheme
    {
        #region Constructors
        public PersonRole()
        {
            personRoleScheme = "http://www.fpml.org/coding-scheme/person-role";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.personRoleScheme = value; }
            get { return this.personRoleScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion PersonRole

    #region Region
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class Region : IScheme, IEFS_Array
    {
        #region Constructors
        public Region() : this(string.Empty) { }
        public Region(string pValue)
        {
            regionScheme = "http://www.fpml.org/coding-scheme/region";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.regionScheme = value; }
            get { return this.regionScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray

    }
    #endregion Region

    #region RegulatorId
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class RegulatorId : IScheme
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.regulatorIdScheme = value; }
            get { return this.regulatorIdScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion RegulatorId

    #region RelatedParty
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // FI 20170928 [23452] Modify
    public partial class RelatedParty : ItemGUI, IEFS_Array, IRelatedParty
    {
        #region Constructors
        public RelatedParty()
        {
            this.accountReference = new AccountReference();
            this.partyReference = new PartyReference();
            this.role = new PartyRoleCode();
            this.type = new PartyRoleType();
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

        #region IRelatedParty 
        // FI 20170928 [23452] add
        IReference IRelatedParty.PartyReference
        {
            get
            {
                return partyReference;
            }
            set
            {
                partyReference = new PartyReference(value.HRef);
            }
        }
        bool IRelatedParty.AccountReferenceSpecified
        {
            get
            {
                return this.accountReferenceSpecified;
            }
            set
            {
                this.accountReferenceSpecified = value;
            }
        }
        IReference IRelatedParty.AccountReference
        {
            get
            {
                return this.accountReference;
            }
            set
            {
                this.accountReference = new AccountReference() { href = value.HRef};
            }
        }
        // EG 20171016 [23342] Upd
        IScheme IRelatedParty.Role
        {
            get
            {
                return role;
            }
            set
            {
                role = new PartyRoleCode()
                {
                    Value = value.Value,
                    partyRoleScheme = value.Scheme
                };
            }
        }
        IScheme IRelatedParty.Type
        {
            get
            {
                return type;
            }
            set
            {
                type = new PartyRoleType()
                {
                    Value = value.Value,
                    partyRoleTypeScheme = value.Scheme
                };
            }
        }
        #endregion IRelatedParty
    }
    #endregion RelatedParty

    #region ReportingBoolean
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ReportingBoolean : IScheme
    {
        #region Constructors
        public ReportingBoolean()
        {
            reportingBooleanScheme = " http://www.fpml.org/coding-scheme/esma-reporting-boolean-2-0.xml";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.reportingBooleanScheme = value; }
            get { return this.reportingBooleanScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ReportingBoolean

    #region ReportingPurpose
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ReportingPurpose : IScheme, IEFS_Array
    {
        #region Constructors
        public ReportingPurpose() : this(string.Empty) { }
        public ReportingPurpose(string pValue)
        {
            reportingPurposeScheme = "http://www.fpml.org/coding-scheme/reporting-purpose";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.reportingPurposeScheme = value; }
            get { return this.reportingPurposeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion ReportingPurpose

    #region RelatedPerson
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // FI 20170928 [23452] Modify (IRelatedPerson)
    public partial class RelatedPerson : ItemGUI, IEFS_Array, IRelatedPerson
    {
        #region Constructors
        public RelatedPerson()
        {
            this.personReference = new PersonReference();
            this.role = new PersonRole();
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

        #region IRelatedPerson // FI 20170928 [23452]
        IReference IRelatedPerson.PersonReference
        {
            get
            {
                return personReference;
            }
            set
            {
                personReference = new PersonReference(value.HRef);
            }
        }
        IScheme IRelatedPerson.Role
        {
            get
            {
                return this.role;
            }
            set
            {
                new PersonRole()
                {
                    Value = value.Value,
                    personRoleScheme = value.Scheme
                };
            }
        }
        #endregion
    }
    #endregion RelatedPerson

    #region ReportingRegime
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ReportingRegime : IEFS_Array
    {
        #region Constructors
        public ReportingRegime()
        {
            itemEntityClassification = new EntityClassification();
            itemPartyEntityClassification = new PartyEntityClassification();
            itemNone = new Empty();
        }
        #endregion Constructors

        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion ReportingRegime

    #region ReportingRegimeName
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ReportingRegimeName : IScheme
    {
        #region Constructors
        public ReportingRegimeName()
        {
            reportingRegimeNameScheme = "http://www.fpml.org/coding-scheme/reporting-regime";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.reportingRegimeNameScheme = value; }
            get { return this.reportingRegimeNameScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ReportingRegimeName

    #region ReportingRole
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class ReportingRole : ISchemeId
    {
        #region Constructors
        public ReportingRole()
        {
            reportingRoleScheme = "http://www.fpml.org/coding-scheme/reporting-role";
            Value = string.Empty;
        }
        #endregion Constructors

        #region ISchemeId Members
        string IScheme.Scheme
        {
            set { this.reportingRoleScheme = value; }
            get { return this.reportingRoleScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        string ISchemeId.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
    }
    #endregion ReportingRole

    #region ShortSale
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class ShortSale : IScheme
    {
        #region Constructors
        public ShortSale()
        {
            shortSaleScheme = "http://www.fpml.org/coding-scheme/esma-mifir-short-sale";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.shortSaleScheme = value; }
            get { return this.shortSaleScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ShortSale

    #region SupervisoryBody
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class SupervisoryBody : IScheme
    {
        #region Constructors
        public SupervisoryBody()
        {
            supervisoryBodyScheme = "http://www.fpml.org/coding-scheme/supervisory-body";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.supervisoryBodyScheme = value; }
            get { return this.supervisoryBodyScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion SupervisorBody

    #region SupervisorRegistration
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class SupervisorRegistration : IEFS_Array
    {
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion SupervisorRegistration

    #region TelephoneNumber
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TelephoneNumber : IEFS_Array
    {
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion TelephoneNumber

    #region TradeCategory
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TradeCategory : IScheme, IEFS_Array
    {
        #region Constructors
        public TradeCategory() : this(string.Empty) { }
        public TradeCategory(string pValue)
        {
            categoryScheme = "http://www.fpml.org/coding-scheme/org-type-category";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.categoryScheme = value; }
            get { return this.categoryScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion TradeCategory

    #region TradeTimestamp
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TradeTimestamp : IEFS_Array
    {
        #region Constructors
        public TradeTimestamp() : this(string.Empty) { }
        public TradeTimestamp(string pValue)
        {
            Value = pValue;
        }
        #endregion Constructors

        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value

        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
    }
    #endregion TradeTimestamp

    #region TimestampTypeScheme
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TimestampTypeScheme : IScheme
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.timestampScheme = value; }
            get { return this.timestampScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion TimestampTypeScheme

    #region TradeProcessingTimestamps
    // EG 20170918 [23342] New FpML extensions for MiFID II
    // EG 20171025 [23509] Add ITradeProcessingTimestamps
    // EG 20171031 [23509] Upd
    public partial class TradeProcessingTimestamps : ITradeProcessingTimestamps
    {
        #region Constructors
        public TradeProcessingTimestamps()
        {
            // FI 20170928 [23452] new instance for  _orderEntered and timestamp 
            _orderEntered = new EFS_DateTimeOffset();
            timestamp = new TradeTimestamp[] { new TradeTimestamp() };
        }
        #endregion Constructors

        #region ITradeProcessingTimestamps 
        // FI 20170928 [23452] Add
        bool ITradeProcessingTimestamps.OrderEnteredSpecified
        {
            get {return OrderEnteredSpecified;}
            set {OrderEnteredSpecified = value;}
        }

        Nullable<DateTimeOffset> ITradeProcessingTimestamps.OrderEnteredDateTimeOffset
        {
            get {return Tz.Tools.ToDateTimeOffset(this.OrderEntered);}
        }

        string ITradeProcessingTimestamps.OrderEntered
        {
            get {return OrderEntered;}
            set {OrderEntered = value;}
        }
        #endregion
    }
    #endregion TradeProcessingTimestamps

    #region TradingWaiver
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TradingWaiver : IScheme, IEFS_Array
    {
        #region Constructors
        public TradingWaiver() : this(string.Empty) { }
        public TradingWaiver(string pValue)
        {
            tradingWaiverScheme = "http://www.fpml.org/coding-scheme/esma-mifir-trading-waiver";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.tradingWaiverScheme = value; }
            get { return this.tradingWaiverScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray

    }
    #endregion TradingWaiver

    #region TransactionCharacteristic
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class TransactionCharacteristic : IScheme, IEFS_Array
    {
        #region Constructors
        public TransactionCharacteristic() : this(string.Empty) { }
        public TransactionCharacteristic(string pValue)
        {
            transactionCharacteristicScheme = "http://www.fpml.org/coding-scheme/transaction-characteristic";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.transactionCharacteristicScheme = value; }
            get { return this.transactionCharacteristicScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray

    }
    #endregion TransactionCharacteristic

    #region Unit
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public partial class Unit : IScheme
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.unitScheme = value; }
            get { return this.unitScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion Unit

}
