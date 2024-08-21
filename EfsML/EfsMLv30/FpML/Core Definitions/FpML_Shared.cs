#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.v30.CashBalanceInterest;
using EfsML.v30.CommodityDerivative;
using EfsML.v30.Fix;
using EfsML.v30.FuturesAndOptions;
using EfsML.v30.Invoice;
using EfsML.v30.LoanDeposit;
using EfsML.v30.MiFIDII_Extended;
using EfsML.v30.Security;
using EfsML.v30.Security.Shared;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.v44.Assetdef;
using FpML.v44.Cd;
using FpML.v44.CorrelationSwaps;
using FpML.v44.DividendSwaps;
using FpML.v44.Doc;
using FpML.v44.Eq.Shared;
using FpML.v44.Eqd;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Mktenv.ToDefine;
using FpML.v44.Option.Shared;
using FpML.v44.ReturnSwaps;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.VarianceSwaps;
using System;
using System.Reflection;
#endregion using directives


namespace FpML.v44.Shared
{
    #region Account
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Account
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("accountId",Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Id", IsMaster = true, IsChild = false)]
        public AccountId[] accountId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountName",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account name", IsMaster = true)]
        public EFS_StringArray[] accountName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountBeneficiarySpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountBeneficiary", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account beneficiary")]
        public PartyReference accountBeneficiary;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
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
    #endregion Account
    #region AccountId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AccountId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string accountIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion AccountId
    #region AccountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AccountReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion AccountReference
    #region Address
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Address : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("streetAddress", Order = 1)]
		[ControlGUI(Name = "Adress", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
		public StreetAddress streetAddress;
        [System.Xml.Serialization.XmlElementAttribute("city", Order = 2)]
        [ControlGUI(Name = "City", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
        public EFS_String city;
        [System.Xml.Serialization.XmlElementAttribute("state", Order = 3)]
        [ControlGUI(Name = "State", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
        public EFS_String state;
        [System.Xml.Serialization.XmlElementAttribute("country", Order = 4)]
        [ControlGUI(Name = "Country", LblWidth = 60, Width = 350)]
        public Country country;
        [System.Xml.Serialization.XmlElementAttribute("postalCode", Order = 5)]
        [ControlGUI(Name = "Postal code", Width = 100)]
        public EFS_String postalCode;
        #endregion Members
    }
    #endregion Address
    #region AdjustableDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("unadjustedDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public IdentifiedDate unadjustedDate;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Date Adjustments", IsVisible = false, IsCopyPaste = true)]
        public BusinessDayAdjustments dateAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion AdjustableDate
    #region AdjustableDate2
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdjustableDate2 : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("unadjustedDate", Order = 1)]
        public IdentifiedDate unadjustedDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Date adjustment")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", typeof(BusinessDayAdjustments), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessDayAdjustments itemDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustmentsReference", typeof(BusinessDayAdjustmentsReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessDayAdjustments)]
        public BusinessDayAdjustmentsReference itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
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
        public AdjustableDate2()
        {
            unadjustedDate = new IdentifiedDate();
            itemDefine = new BusinessDayAdjustments();
            itemReference = new BusinessDayAdjustmentsReference();
        }
        #endregion Constructors
    }
    #endregion AdjustableDate2
    #region AdjustableDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 Upd GUI
    public partial class AdjustableDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedDate", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unadjusted Dates", IsMaster = true, IsMasterVisible = true)]
        public IdentifiedDate[] unadjustedDate;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjustment", IsVisible = false)]
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 2)]
        public BusinessDayAdjustments dateAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjustment")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public EFS_Id efs_id;
        //public bool FillBalise;
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
    #endregion AdjustableDates
    #region AdjustableDatesOrRelativeDateOffset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 Upd GUI
    public partial class AdjustableDatesOrRelativeDateOffset : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice dtType_;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtType_AdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates dtType_AdjustableDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtType_RelativeDateOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true)]
        public RelativeDateOffset dtType_RelativeDateOffset;
        #endregion Members
    }
    #endregion AdjustableDatesOrRelativeDateOffset
    #region AdjustableOrRelativeAndAdjustedDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableOrRelativeAndAdjustedDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate adjustableOrRelativeDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateRelativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
        public RelativeDateOffset adjustableOrRelativeDateRelativeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted date")]
        public IdentifiedDate adjustedDate;
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
    #endregion AdjustableOrRelativeAndAdjustedDate
    #region AdjustableOrRelativeDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableOrRelativeDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate adjustableOrRelativeDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateRelativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
        public RelativeDateOffset adjustableOrRelativeDateRelativeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion AdjustableOrRelativeDate
    #region AdjustableOrRelativeDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableOrRelativeDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDatesAdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates adjustableOrRelativeDatesAdjustableDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDatesRelativeDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDates", typeof(RelativeDates), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Dates", IsVisible = true)]
        public RelativeDates adjustableOrRelativeDatesRelativeDates;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion AdjustableOrRelativeDates
    #region AdjustableRelativeOrPeriodicDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableRelativeOrPeriodicDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableRelativeOrPeriodic;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicAdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates adjustableRelativeOrPeriodicAdjustableDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicRelativeDateSequenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDateSequence", typeof(RelativeDateSequence), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date Sequence", IsVisible = true)]
        public RelativeDateSequence adjustableRelativeOrPeriodicRelativeDateSequence;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicPeriodicDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodicDates", typeof(PeriodicDates), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periodic Dates", IsVisible = true)]
        public PeriodicDates adjustableRelativeOrPeriodicPeriodicDates;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion AdjustableRelativeOrPeriodicDates
    #region AdjustableRelativeOrPeriodicDates2
    /// <summary>
    ///  A type giving the choice between defining a series of dates as an explicit list of dates together 
    ///  with applicable adjustments, or as relative to some other series of (anchor) dates, 
    ///  or as a set of factors to specify periodic occurences. 
    /// </summary>
    // EG 20140702 New build FpML4.4 New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableRelativeOrPeriodicDates2 : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice dtType_;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtType_AdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates dtType_AdjustableDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtType_RelativeDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDates", typeof(RelativeDates), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Dates", IsVisible = true)]
        public RelativeDates dtType_RelativeDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dtType_PeriodicDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodicDates", typeof(PeriodicDates), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periodic Dates", IsVisible = true)]
        public PeriodicDates dtType_PeriodicDates;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion AdjustableRelativeOrPeriodicDates2
    #region AdjustedRelativeDateOffset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustedRelativeDateOffset : RelativeDateOffset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relativeDateAdjustmentsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDateAdjustments", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative date Adjustments")]
        public BusinessDayAdjustments relativeDateAdjustments;
        #endregion Members
    }
    #endregion AdjustedRelativeDateOffset
    #region AmericanExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("americanExercise", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class AmericanExercise : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("commencementDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commencement Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate commencementDate;
        [System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commencement Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date")]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
        [System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Latest Exercise Time")]
        public BusinessCenterTime latestExerciseTime;
        [System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleExercise", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Multiple Exercise", IsCopyPaste = true)]
        public MultipleExercise multipleExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseFeeSchedule", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee Schedule", IsCopyPaste = true)]
        public ExerciseFeeSchedule exerciseFeeSchedule;
        #endregion Members
    }
    #endregion AmericanExercise
    #region AmountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AmountReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion AmountReference
    #region AmountSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("notionalStepSchedule", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class AmountSchedule : Schedule
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Currency", Width = 70, LineFeed = MethodsGUI.LineFeedEnum.BeforeAndAfter)]
        public Currency currency;
        #endregion Members
    }
    #endregion AmountSchedule
    #region ArrayPartyReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ArrayPartyReference
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public EFS_Href efs_href;
        #endregion Members
    }
    #endregion ArrayPartyReference
    #region AutomaticExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AutomaticExercise
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("thresholdRate", Order = 1)]
		[ControlGUI(Name = "thresholdRate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal thresholdRate;
		#endregion Members
	}
    #endregion AutomaticExercise

    #region Beneficiary
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Beneficiary : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Rounting")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiaryPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("beneficiaryPartyReference", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference beneficiaryPartyReference;
        #endregion Members
    }
    #endregion Beneficiary
    #region BermudaExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("bermudaExercise", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class BermudaExercise : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bermudaExerciseDates", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Bermuda Exercise Dates", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDates bermudaExerciseDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Bermuda Exercise Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date", IsCopyPaste = true)]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
        [System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Latest Exercise Time")]
        public BusinessCenterTime latestExerciseTime;
        [System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleExercise", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Multiple Exercise", IsCopyPaste = true)]
        public MultipleExercise multipleExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseFeeSchedule", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee Schedule", IsCopyPaste = true)]
        public ExerciseFeeSchedule exerciseFeeSchedule;
        #endregion Members
    }
    #endregion BermudaExercise
    #region BrokerConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BrokerConfirmation : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("brokerConfirmationType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 350)]
        public BrokerConfirmationType brokerConfirmationType;
    }
    #endregion BrokerConfirmation
    #region BrokerConfirmationType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BrokerConfirmationType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string brokerConfirmationTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Constructors
        public BrokerConfirmationType()
        {
            brokerConfirmationTypeScheme = "http://www.fpml.org/coding-scheme/broker-confirmation-type-3-2";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            BrokerConfirmationType clone = new BrokerConfirmationType
            {
                brokerConfirmationTypeScheme = this.brokerConfirmationTypeScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion BrokerConfirmationType
    #region BusinessCenter
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    /// EG 20150422 [20513] BANCAPERTA ReferenceGUI = BusinessCenters
    public partial class BusinessCenter : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string businessCenterScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 300)]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessCenter)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessCenters)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion BusinessCenter
    #region BusinessCenterTime
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class BusinessCenterTime : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Time", Width = 100, IsSpaceBefore = false, Regex = EFSRegex.TypeRegex.RegexLongTime)]
        public HourMinuteTime hourMinuteTime;
        [ControlGUI(Name = "Business center", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public BusinessCenter businessCenter;
        #endregion Members
    }
    #endregion BusinessCenterTime
    #region BusinessCenters
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("businessCenters", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class BusinessCenters
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Center")]
        public BusinessCenter[] businessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessCenters)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion BusinessCenters
    #region BusinessCentersReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class BusinessCentersReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        [ControlGUI(IsLabel = false, Name = "value", Width = 200)]
        public string href;
        #endregion Members
    }
    #endregion BusinessCentersReference
    #region BusinessDateRange
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class BusinessDateRange : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("unadjustedFirstDate", Order = 1)]
        [ControlGUI(Name = "firstDate")]
        public EFS_Date unadjustedFirstDate;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedLastDate", Order = 2)]
        [ControlGUI(Name = "lastDate")]
        public EFS_Date unadjustedLastDate;
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 3)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;
        #endregion Members
    }
    #endregion BusinessDateRange
    #region BusinessDayAdjustments
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class BusinessDayAdjustments : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 1)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers", IsCopyPaste = true)]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessDayAdjustments)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion BusinessDayAdjustments
    #region BusinessDayAdjustmentsReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BusinessDayAdjustmentsReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        [ControlGUI(IsLabel = false, Name = "value", Width = 200)]
        public string href;
        #endregion Members
    }
    #endregion BusinessDayAdjustmentsReference

    #region CalculationAgent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CalculationAgent : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice calculationAgent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentPartyReference", typeof(ArrayPartyReference), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party Reference", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public ArrayPartyReference[] calculationAgentPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentPartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentParty", typeof(CalculationAgentPartyEnum), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public CalculationAgentPartyEnum calculationAgentParty;
        #endregion Members
    }
    #endregion CalculationAgent
    #region CalculationPeriodFrequency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CalculationPeriodFrequency : Interval
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("rollConvention", Order = 1)]
        [ControlGUI(Name = "Roll", Width = 190)]
        public RollConventionEnum rollConvention;
        #endregion Members
    }
    #endregion CalculationPeriodFrequency
    #region CashSettlementReferenceBanks
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashSettlementReferenceBanks
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceBank", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Bank")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Bank", IsClonable = true)]
        public ReferenceBank[] referenceBank;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    #endregion CashSettlementReferenceBanks
    #region CashflowType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cashflowTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Constructors
        public CashflowType()
        {
            cashflowTypeScheme = "http://www.fpml.org/coding-scheme/cashflow-type-2-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            CashflowType clone = new CashflowType
            {
                cashflowTypeScheme = this.cashflowTypeScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion CashflowType
    #region ClearanceSystem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ClearanceSystem : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string clearanceSystemScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ClearanceSystem
    #region ContractualDefinitions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractualDefinitions : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue contractualDefinitions;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string contractualDefinitionsScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Constructors
        public ContractualDefinitions()
        {
            contractualDefinitionsScheme = "http://www.fpml.org/coding-scheme/contractual-definitions-3-2";
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
    #endregion ContractualDefinitions
    #region ContractualMatrix
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractualMatrix : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("matrixType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 300)]
        public MatrixType matrixType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publicationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("publicationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publication Date")]
        public EFS_Date publicationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matrixTermSpecified;
        [System.Xml.Serialization.XmlElementAttribute("matrixTerm", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Term", Width = 350)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public MatrixTerm matrixTerm;
        #endregion Members
        #region Constructors
        public ContractualMatrix()
        {
            publicationDate = new EFS_Date();
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
    #endregion ContractualMatrix
    #region ContractualSupplement
    // EG 20140702 New build FpML4.4 contractualSupplementScheme (New version)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractualSupplement
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue contractualSupplement;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string contractualSupplementScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Constructors
        public ContractualSupplement()
        {
            contractualSupplementScheme = "http://www.fpml.org/coding-scheme/contractual-supplement-6-7";
        }
        #endregion Constructors
    }
    #endregion ContractualSupplement
    #region ContractualTermsSupplement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractualTermsSupplement : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        public ContractualSupplement @type;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publicationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("publicationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publication Date")]
        public EFS_Date publicationDate;
        #endregion Members
		#region Constructors
		public ContractualTermsSupplement()
		{
			publicationDate = new EFS_Date();
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
    #endregion ContractualTermsSupplement
    #region CorrespondentInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CorrespondentInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Rounting")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool correspondentPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("correspondentPartyReference", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference correspondentPartyReference;
        #endregion Members
    }
    #endregion CorrespondentInformation
    #region Country
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Country : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string countryScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
    }
    #endregion Country
    #region CreditSeniority
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CreditSeniority : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string creditSeniorityScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = true, Name = "value")]
        public string Value;
        #endregion Members
    }
    #endregion CreditSeniority
    #region CreditSupportAgreement
    // EG 20140702 New build FpML4.4 New 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CreditSupportAgreement : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("brokerConfirmationType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 350)]
        public BrokerConfirmationType brokerConfirmationType;
        [System.Xml.Serialization.XmlElementAttribute("stepDate", Order = 2)]
        [ControlGUI(Name = "Date", LblWidth = 75)]
        public EFS_Date date;

    }
    #endregion CreditSupportAgreement
    #region CreditSupportAgreementType
    // EG 20140526 New build FpML4.4 New 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CreditSupportAgreementType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string creditSupportAgreementTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Constructors
        public CreditSupportAgreementType()
        {
            creditSupportAgreementTypeScheme = "http://www.fpml.org/coding-scheme/credit-support-agreement-type-1-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            CreditSupportAgreementType clone = new CreditSupportAgreementType
            {
                creditSupportAgreementTypeScheme = this.creditSupportAgreementTypeScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion CreditSupportAgreementType

    #region Currency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(IdentifiedCurrency))]
    //public partial class Currency : SchemeGUI //ItemGUI
    public partial class Currency : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string currencyScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
    }
    #endregion Currency

    #region DateList
    public partial class DateList : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("date", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date", IsChild = false)]
        public EFS_DateArray[] date;
        #endregion Members
    }
    #endregion DateList
    #region DateOffset
    // EG 20140702 New build FpML4.4 sequence DEPRECATED
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DateOffset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [ControlGUI(Name = "dayType")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 4)]
        [ControlGUI(Name = "convention", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public BusinessDayConventionEnum businessDayConvention;
        #endregion Members
    }
    #endregion DateOffset
    #region DateRange
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BusinessDateRange))]
    public class DateRange : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("unadjustedFirstDate", Order = 1)]
        [ControlGUI(Name = "firstDate")]
        public EFS_Date unadjustedFirstDate;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedLastDate", Order = 2)]
        [ControlGUI(Name = "lastDate")]
        public EFS_Date unadjustedLastDate;
        #endregion Members
    }
    #endregion DateRange
    #region DateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DateReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion DateReference
    #region DateTimeList
    public partial class DateTimeList : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date", IsChild = false)]
        public EFS_DateTimeArray[] dateTime;
        #endregion Members
    }
    #endregion DateTimeList
    #region DayCountFraction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DayCountFraction : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string dayCountFractionScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members

        #region Constructors
        public DayCountFraction()
        {
            dayCountFractionScheme = "http://www.fpml.org/coding-scheme/day-count-fraction-2-1";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            DayCountFraction clone = new DayCountFraction
            {
                dayCountFractionScheme = this.dayCountFractionScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion DayCountFraction
    #region DeterminationMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DeterminationMethod : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string determinationMethodScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
    }
    #endregion DeterminationMethod
    #region DividendConditions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DividendConditions : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendReinvestmentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendReinvestment",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Reinvestment")]
        public EFS_Boolean dividendReinvestment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendEntitlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendEntitlement", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Entitlement")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendEntitlementEnum dividendEntitlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendAmount", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend amount")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendAmountTypeEnum dividendAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Payment Date")]
        public DividendPaymentDate dividendPaymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice dividendPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodEffectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPeriodEffectiveDate", typeof(DateReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative to Effective date", IsVisible = true)]
        public DateReference dividendPeriodEffectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPeriod", typeof(DividendPeriodEnum), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Period", IsVisible = true)]
        public DividendPeriodEnum dividendPeriodPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPeriodEndDate", typeof(DateReference), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative to End date", IsVisible = true)]
        public DateReference dividendPeriodEndDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extraOrdinaryDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extraOrdinaryDividends", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "ExtraOrdinary Party reference")]
        public PartyReference extraOrdinaryDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excessDividendAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("excessDividendAmount", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Gross Cash Dividend per Share")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendAmountTypeEnum excessDividendAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Currency amount")]
        public EFS_RadioChoice currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", typeof(Currency), Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 75)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency", IsVisible = true)]
        public Currency currencyCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(DeterminationMethod), Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Method", IsVisible = true)]
        public DeterminationMethod currencyDeterminationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyCurrencyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyReference", typeof(IdentifiedCurrencyReference), Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Method", IsVisible = true)]
        public IdentifiedCurrencyReference currencyCurrencyReference;
        [System.Xml.Serialization.XmlElementAttribute("paymentCurrency", Order = 13)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Currency", IsVisible = true)]
        public PaymentCurrency paymentCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendFxTriggerDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendFxTriggerDate", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Currency")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Fx Trigger Date")]
        public DividendPaymentDate dividendFxTriggerDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestAccrualsMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestAccrualsMethod", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Interest Accruals Method")]
        public InterestAccrualsCompoundingMethod interestAccrualsMethod;
        #endregion Members
    }
    #endregion DividendConditions
    #region DividendPaymentDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DividendPaymentDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice paymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateDividendDateReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendDateReference", typeof(DividendDateReferenceEnum), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Date Reference", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public DividendDateReferenceEnum paymentDateDividendDateReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate paymentDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDateOffset", typeof(Offset), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Offset", IsVisible = true)]
        public Offset paymentDateOffset;
        #endregion Members
    }
    #endregion DividendPaymentDate
    #region Documentation
    // EG 20140702 New build FpML4.4 creditSupportAgreement replace creditSupportComplement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Documentation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterAgreementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterAgreement", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Master Agreement")]
        public MasterAgreement masterAgreement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice documentation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty documentationNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationMasterConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterConfirmation", typeof(MasterConfirmation), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Master Confirmation", IsVisible = true)]
        public MasterConfirmation documentationMasterConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationBrokerConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("brokerConfirmation", typeof(BrokerConfirmation), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Broker Confirmation", IsVisible = true)]
        public BrokerConfirmation documentationBrokerConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualDefinitionsSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual Definitions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ContractualDefinitions", IsClonable = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractualDefinitions", Order = 4)]
        public ContractualDefinitions[] contractualDefinitions;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualSupplementTermsSupplementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractualTermsSupplement", typeof(ContractualTermsSupplement), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual terms supplement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contractual terms supplement")]
        public ContractualTermsSupplement[] contractualSupplementTermsSupplement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualMatrixSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual Matrix")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ContractualMatrix", IsClonable = true, IsChild = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractualMatrix", Order = 6)]
        public ContractualMatrix[] contractualMatrix;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditSupportAgreementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditSupportAgreement", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Credit Support Agreement", Width = 600)]
        public CreditSupportAgreement creditSupportAgreement;
        #endregion Members
        #region Constructors
        public Documentation()
        {
            documentationNone = new Empty();
            documentationMasterConfirmation = new MasterConfirmation();
            documentationBrokerConfirmation = new BrokerConfirmation();
        }
        #endregion Constructors
    }
    #endregion Documentation

    #region Empty
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Empty : EmptyGUI
    {
    }
    #endregion Empty
    #region EntityId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EntityId : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue entityId;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string entityIdScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Constructors
        public EntityId() : this(string.Empty) { }
        public EntityId(string pValue)
        {
            Value          = pValue;
			entityIdScheme = "http://www.fpml.org/spec/2003/entity-id-RED-1-0";
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
    #endregion EntityId
    #region EntityName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EntityName : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue entityName;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string entityNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Constructors
        public EntityName()
        {
            entityNameScheme = "http://www.fpml.org/spec/2003/entity-name-RED-1-0";
        }
        #endregion Constructors
    }
    #endregion EntityName
    #region EuropeanExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("europeanExercise", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class EuropeanExercise : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date", IsCopyPaste = true)]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
        [System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
        [System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partialExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partialExercise", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Partial Exercise", IsCopyPaste = true)]
        public PartialExercise partialExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseFee", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee", IsCopyPaste = true)]
        public ExerciseFee exerciseFee;
        #endregion Members
    }
    #endregion EuropeanExercise
    #region ExchangeId
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeId : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue exchangeId;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string exchangeIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 250)]
        public string Value;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeId";

        #endregion Members
    }
    #endregion ExchangeId
    #region Exercise
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmericanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BermudaExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAmericanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityBermudaExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityEuropeanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EuropeanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SharedAmericanExercise))]
    public partial class Exercise : ItemGUI
    {
        #region Members
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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Exercise
    #region ExerciseFee
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("exerciseFee", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class ExerciseFee : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer", LblWidth = 120)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 3)]
        [ControlGUI(Name = "NotionalReference", LblWidth = 100, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 170)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public ScheduleReference notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee")]
        public EFS_RadioChoice typeFee;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeAmount", typeof(EFS_Decimal), Order = 4)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal typeFeeAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeRate", typeof(EFS_Decimal), Order = 5)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75, Regex = EFSRegex.TypeRegex.RegexFixedRateExtend)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal typeFeeRate;

        [System.Xml.Serialization.XmlElementAttribute("feePaymentDate", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset feePaymentDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion ExerciseFee
    #region ExerciseFeeSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ExerciseFeeSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer", LblWidth = 287)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 3)]
        [ControlGUI(Name = "NotionalReference", LblWidth = 267, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 170)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public ScheduleReference notionalReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee", IsCopyPaste = true)]
        public EFS_RadioChoice typeFee;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeAmountSchedule", typeof(AmountSchedule), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AmountSchedule typeFeeAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeRateSchedule", typeof(Schedule), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Schedule typeFeeRate;
        [System.Xml.Serialization.XmlElementAttribute("feePaymentDate", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date", IsVisible = true, IsCopyPaste = true)]
        [ControlGUI(IsPrimary = false, Name = "offset", LblWidth = 233)]
        public RelativeDateOffset feePaymentDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion ExerciseFeeSchedule
    #region ExerciseNotice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ExerciseNotice : ItemGUI 
    {
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Name = "partyReference")]
		public PartyReference partyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseNoticePartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseNoticePartyReference", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Notice Party Reference")]
		public PartyReference exerciseNoticePartyReference;
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 3)]
		[ControlGUI(IsPrimary = false, Name = "business Center")]
		public BusinessCenter businessCenter;
		#endregion Members
    }
    #endregion ExerciseNotice
    #region ExerciseProcedure
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    /// EG 20150422 [20513] BANCAPERTA new followUpConfirmationSpecified
    public partial class ExerciseProcedure : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice exerciseProcedure;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseProcedureAutomaticSpecified;
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(AutomaticExercise), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Automatic Exercise", IsVisible = true)]
        public AutomaticExercise exerciseProcedureAutomatic;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseProcedureManualSpecified;
        [System.Xml.Serialization.XmlElementAttribute("manualExercise", typeof(ManualExercise), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Manual Exercise", IsVisible = true)]
        public ManualExercise exerciseProcedureManual;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool followUpConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("followUpConfirmation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "followUpConfirmation")]
        public EFS_Boolean followUpConfirmation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool limitedRightToConfirmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("limitedRightToConfirm", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "limitedRightToConfirm")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public EFS_Boolean limitedRightToConfirm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool splitTicketSpecified;
        [System.Xml.Serialization.XmlElementAttribute("splitTicket", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "splitTicket")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public EFS_Boolean splitTicket;
        #endregion Members
    }
    #endregion ExerciseProcedure

    #region FloatingRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FloatingRateCalculation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InflationRateCalculation))]
    public partial class FloatingRate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Order = 1)]
        [ControlGUI(Name = "Rate", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexTenorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation rules", IsVisible = false, IsGroup = true)]
        public bool FillBalise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floatingRateMultiplierScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateMultiplierSchedule", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FloatingRate Multiplier Schedule", IsCopyPaste = true)]
        public Schedule floatingRateMultiplierSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spreadSchedule", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public SpreadSchedule[] spreadSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateTreatmentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateTreatment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Treatment")]
        public RateTreatmentEnum rateTreatment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capRateScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("capRateSchedule", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap Rate Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap Rate Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public StrikeSchedule[] capRateSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floorRateScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floorRateSchedule", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor Rate Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor Rate Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public StrikeSchedule[] floorRateSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation rules")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.RateIndex)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion FloatingRate
    #region FloatingRateCalculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InflationRateCalculation))]
    [System.Xml.Serialization.XmlRootAttribute("floatingRateCalculation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Floating Rate Calculation")]
    public partial class FloatingRateCalculation : FloatingRate
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialRate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "initialRate", Regex = EFSRegex.TypeRegex.RegexFixedRate)]
        public EFS_Decimal initialRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]

        public bool finalRateRoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("finalRateRounding", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "finalRateRounding")]
        public Rounding finalRateRounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingMethod", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "averagingMethod")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingMethodEnum averagingMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool negativeInterestRateTreatmentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("negativeInterestRateTreatment", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "negativeInterestRateTreatment")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NegativeInterestRateTreatmentEnum negativeInterestRateTreatment;
        #endregion Members
    }
    #endregion FloatingRateCalculation
    #region FloatingRateIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FloatingRateIndex : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string floatingRateIndexScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hrefSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("href", DataType = "normalizedString")]
        public string href;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FloatingRateIndex";
        #endregion Members
    }
    #endregion FloatingRateIndex
    #region ForecastRateIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ForecastRateIndex : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Order = 1)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
        #endregion Members
    }
    #endregion ForecastRateIndex
    #region Formula
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Formula : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaDescriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formulaDescription", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description")]
        public EFS_MultiLineString formulaDescription;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mathSpecified;
        [System.Xml.Serialization.XmlElementAttribute("math", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Math")]
        public Math math;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaComponentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formulaComponent", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula Components")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula Components", IsClonable = true, IsChild = true, MinItem = 0)]
        public FormulaComponent[] formulaComponent;
        #endregion Members
    }
    #endregion Formula
    #region FormulaComponent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FormulaComponent
    {                       
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("componentDescription", Order = 1)]
        [ControlGUI(Name = "Description", LblWidth = 100)]
        public EFS_MultiLineString componentDescription;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formula", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula formula;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_nameSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Name", Width = 500)]
        public EFS_Attribute efs_name;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_hrefSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Formula)]
        public EFS_Href efs_href;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name
        {
            set
            {
                efs_name = new EFS_Attribute(value);
                efs_nameSpecified = StrFunc.IsFilled(value);
            }
            get
            {
                if (efs_name == null)
                    return null;
                else
                    return efs_name.Value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute("href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set
            {
                efs_href = new EFS_Href(value);
                efs_hrefSpecified = StrFunc.IsFilled(value);
            }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }
        #endregion Accessors
    }
    #endregion FormulaComponent
    #region FxCashSettlement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FxCashSettlement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(Currency), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Settlement Currency", Width = 75)]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlElementAttribute("fixing", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing", IsClonable = true, IsChild = true)]
        public FxFixing[] fixing;
        #endregion Members
		#region Members
        //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool customerSettlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("customerSettlementRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "customer Settlement Rate")]
        public Empty customerSettlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSettlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentSettlementRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "calculation Agent Settlement Rate")]
        public Empty calculationAgentSettlementRate;
        #endregion Members
    }
    #endregion FxCashSettlement
    #region FxFixing
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxFixing : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted Currency Pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("fixingDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted Currency Pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Fixing Date")]
        public EFS_Date fixingDate;
        #endregion Members
    }
    #endregion FxFixing
    #region FxRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeRate))]
    public partial class FxRate
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 1)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [ControlGUI(Name = "rate", Regex = EFSRegex.TypeRegex.RegexFxRateExtend)]
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 2)]
        public EFS_Decimal rate;
        #endregion Members
    }
    #endregion FxRate
    #region FxSpotRateSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxFixing))]
    public partial class FxSpotRateSource : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Primary Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Primary Rate Source")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion FxSpotRateSource

    #region GoverningLaw
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20170918 [23342] Change SchemeGUI to ItemGUI
    public partial class GoverningLaw : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string governingLawScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion GoverningLaw

    #region HourMinuteTime
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class HourMinuteTime : StringGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion HourMinuteTime

    #region IdentifiedCurrency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class IdentifiedCurrency : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string currencyScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion IdentifiedCurrency
    #region IdentifiedCurrencyReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class IdentifiedCurrencyReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion IdentifiedCurrencyReference
    #region IdentifiedDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	public partial class IdentifiedDate : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        public string Value
		{
            set
            {
                efs_Value = new EFS_Date(value);
            }
			get
			{
				if (efs_Value == null)
					return null;
				else
					return efs_Value.Value;
			}
		}
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = true, Name = "value", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		public EFS_Date efs_Value;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion IdentifiedDate
    #region IdentifiedPayerReceiver
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IdentifiedPayerReceiver
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 85)]
        public PayerReceiverEnum Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PayerReceiver)]
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
    #endregion IdentifiedPayerReceiver
    #region InformationProvider
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class InformationProvider : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string informationProviderScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion InformationProvider
    #region InformationSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InformationSource : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate ref.", Width = 300)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public AssetFxRateId assetFxRateId;

		[System.Xml.Serialization.XmlElementAttribute("rateSource",typeof(InformationProvider), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "rateSource", Width = 400)]
        public InformationProvider rateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourcePageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateSourcePage", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rateSourcePage", Width = 600)]
        public RateSourcePage rateSourcePage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourcePageHeadingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateSourcePageHeading", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "heading", Width = 600)]
        public EFS_String rateSourcePageHeading;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:InformationSource";
        #endregion Members
    }
    #endregion InformationSource
    #region InstrumentId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class InstrumentId
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue instrumentId;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string instrumentIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 300)]
        public string Value;
        #endregion Members
    }
    #endregion InstrumentId
    #region InterestAccrualsCompoundingMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestAccrualsCompoundingMethod))]
    public class InterestAccrualsCompoundingMethod : InterestAccrualsMethod
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compoundingMethod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Compounding Method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CompoundingMethodEnum compoundingMethod;
        #endregion Members
    }
    #endregion InterestAccrualsCompoundingMethod
    #region InterestAccrualsMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestAccrualsCompoundingMethod))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestCalculation))]
    public partial class InterestAccrualsMethod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Rate")]
        public EFS_RadioChoice rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate Calculation", IsVisible = false)]
        public FloatingRateCalculation rateFloatingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed Rate", IsVisible = false)]
        [ControlGUI(IsPrimary = false, Name = "value", Regex = EFSRegex.TypeRegex.RegexFixedRateExtend)]
        public EFS_Decimal rateFixedRate;
        #endregion Members
    }
    #endregion InterestAccrualsMethod
    #region InterpolationMethod
    // EG 20140702 New build FpML4.4 Move from Fpml MktEnv
    // EG 20140702 New build FpML4.4 new version of scheme
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class InterpolationMethod : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string interpolationMethodScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion InterpolationMethod

    #region IntermediaryInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class IntermediaryInformation
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Routing")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;
        [System.Xml.Serialization.XmlElementAttribute("intermediarySequenceNumber", Order = 4)]
        [ControlGUI(Name = "Intermediary Sequence Number", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger intermediarySequenceNumber;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intermediaryPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("intermediaryPartyReference", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference intermediaryPartyReference;

        #endregion Members
    }
    #endregion IntermediaryInformation
    #region Interval
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AdjustedRelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CalculationPeriodFrequency))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxFixingDate))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Offset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResetFrequency))]
    public partial class Interval : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "Period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Interval

    #region LegId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 New
    public partial class LegId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string legIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public LegId()
        {
            legIdScheme = "http://www.efs.org/spec/2005/legId-1-0";
        }
        #endregion Constructors
    }
    #endregion LegId

    #region Leg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwapLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwapLegUnderlyer))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestRateStream))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixedPaymentLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLegUnderlyer))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLegUnderlyerValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FeeLeg))]
    public abstract class Leg : ItemGUI 
    {
    }
    #endregion Leg
    #region LegIdentifier
    // EG 20140702 New build FpML4.4 New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class LegIdentifier : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("legId", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "LegId")]
        public LegId legId;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
        public EFS_PosInteger version;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective date")]
        public IdentifiedDate effectiveDate;
    }
    #endregion LegIdentifier

    #region LegalEntity
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class LegalEntity : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityName", typeof(EntityName), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Name")]
        public EntityName entityName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityId", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Id", IsChild = false)]
        public EntityId[] entityId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Entity)]
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
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion LegalEntity
    #region LegalEntityReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class LegalEntityReference : Reference, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "value", Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Entity)]
        public EFS_Href efs_href;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }
        #endregion Accessors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion LegalEntityReference

    #region MainPublication
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MainPublication : SchemeTextGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string mainPublicationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MainPublication()
        {
            mainPublicationScheme = "http://www.fpml.org/coding-scheme/inflation-main-publication-1-0";
        }
        #endregion Constructors
    }
    #endregion MainPublication
    #region ManualExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ManualExercise
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseNoticeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseNotice", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
        public ExerciseNotice exerciseNotice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fallbackExercise", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "fallbackExercise")]
        public EFS_Boolean fallbackExercise;
        #endregion Members
    }
    #endregion ManualExercise
    #region MasterAgreement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MasterAgreement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 150)]
        public MasterAgreementType masterAgreementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterAgreementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Master Agreement Date")]
        public EFS_Date masterAgreementDate;
        #endregion Members
        #region Constructors
        public MasterAgreement()
        {
            masterAgreementDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion MasterAgreement
    #region MasterAgreementType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MasterAgreementType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string masterAgreementTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MasterAgreementType()
        {
            masterAgreementTypeScheme = "http://www.fpml.org/spec/2002/master-agreement-type-1-0";
        }
        #endregion Constructors
    }
    #endregion MasterAgreementType
    #region MasterConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MasterConfirmation : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Type", Width = 350)]
        public MasterConfirmationType masterConfirmationType;
        [ControlGUI(IsPrimary = false, Name = "Date")]
        public EFS_Date masterConfirmationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterConfirmationAnnexDateSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Master Confirmation Annex Date")]
        public EFS_Date masterConfirmationAnnexDate;
        #endregion Members
        #region Constructors
        public MasterConfirmation()
        {
            masterConfirmationAnnexDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion MasterConfirmation
    #region MasterConfirmationType
    // EG 20140702 New build FpML4.4 masterConfirmationTypeScheme new version
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MasterConfirmationType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string masterConfirmationTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MasterConfirmationType()
        {
            masterConfirmationTypeScheme = "http://www.fpml.org/coding-scheme/master-confirmation-type-5-7";
        }
        #endregion Constructors
    }
    #endregion MasterConfirmationType
    #region Math
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Math : NodeGUI
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any;
    }
    #endregion Math
    #region MatrixTerm
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 New build FpML4.4 new version of scheme
    public class MatrixTerm : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string matrixTermScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MatrixTerm()
        {
            matrixTermScheme = "http://www.fpml.org/coding-scheme/credit-matrix-transaction-type-3-1";
        }
        #endregion Constructors
    }
    #endregion MatrixTerm
    #region MatrixType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MatrixType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string matrixTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MatrixType()
        {
            matrixTypeScheme = "http://www.fpml.org/coding-scheme/matrix-type-1-0";
        }
        #endregion Constructors
    }
    #endregion MatrixType
    #region MimeType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MimeType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string mimeTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion MimeType
    #region Money
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxOptionPayout))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CalculationAmount))]
    public partial class Money : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(IsLabel = false, Name = null, Width = 60)]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = " ", Width = 150)]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Money
    #region MultipleExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MultipleExercise : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Reference", IsMaster = true)]
        public ScheduleReference[] notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool integralMultipleAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("integralMultipleAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Integral Multiple Amount")]
        public EFS_Decimal integralMultipleAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum")]
        public EFS_RadioChoice minimum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumNumberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumNumberOfOptions", typeof(EFS_PosInteger), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger minimumNumberOfOptions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumNotionalAmount", typeof(EFS_Decimal), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal minimumNotionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum")]
        public EFS_RadioChoice maximum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty maximumNone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNumberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumNumberOfOptions", typeof(EFS_PosInteger), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger maximumNumberOfOptions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumNotionalAmount", typeof(EFS_Decimal), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal maximumNotionalAmount;
        #endregion Members
    }
    #endregion MultipleExercise

    #region NotionalAmountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NotionalAmountReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
        #region Constructors
        public NotionalAmountReference()
        {
            href = string.Empty;
        }
        #endregion Constructors
    }
    #endregion NotionalAmountReference

    #region Offset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AdjustedRelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxFixingDate))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    public partial class Offset : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
        [ControlGUI(Name = "dayType")]
        [System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
        #endregion Members
    }
    #endregion Offset

    #region PartialExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PartialExercise : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Reference", IsMaster = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public ScheduleReference[] notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool integralMultipleAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("integralMultipleAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Integral Multiple Amount")]
        public EFS_Decimal integralMultipleAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum")]
        public EFS_RadioChoice minimum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumNumberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumNumberOfOptions", typeof(EFS_PosInteger), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger minimumNumberOfOptions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumNotionalAmount", typeof(EFS_Decimal), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal minimumNotionalAmount;
        #endregion Members
    }
    #endregion PartialExercise
    
    #region Party
    /// EG 20170918 [23342] New FpML extensions for MiFID II
    /// EG 20170922 [22374] New The party's time zone.
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Party
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(IsLabel = true, Name = "Party", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Id", MinItem = 1)]
        public PartyId[] partyId;
        [System.Xml.Serialization.XmlElementAttribute("partyName", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        public string partyName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("account", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account")]
        public Account[] account;

        // EG 20170918 [23342]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool classificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("classification", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Industry classification")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Industry classification")]
        public IndustryClassification[] classification;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditRatingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditRating", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit rating")]
        public CreditRating[] creditRating;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool countrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("country", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Country")]
        public Country country;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool regionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("region", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Region")]
        public Region[] region;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool jurisdictionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("jurisdiction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Jurisdiction")]
        public GoverningLaw[] jurisdiction;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool organizationTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("organizationType", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Organization type")]
        public OrganizationType organizationType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contactInfoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contactInfo", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contact information")]
        public ContactInformation contactInfo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessUnitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessUnit", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business unit")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business unit")]
        public BusinessUnit[] businessUnit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool personSpecified;
        [System.Xml.Serialization.XmlElementAttribute("person", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Person")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Person")]
        public Person[] person;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        // EG 20170922 [22374]
        [System.Xml.Serialization.XmlAttributeAttribute("tzdbId", DataType = "normalizedString")]
        public string tzdbid;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Party";
        #endregion Members
    }
    #endregion Party
    #region PartyId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    /// EG 20170918 [23342] change PartyIdGUI to ItemGUI
    public partial class PartyId : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string partyIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PartyId
    #region PartyOrAccountReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PartyOrAccountReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PartyOrAccountReference
    #region PartyOrTradeSideReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PartyOrTradeSideReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PartyOrTradeSideReference
    #region PartyReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PartyReference : HrefGUI
    {
        #region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PartyReference
    
    #region Payment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [MainTitleGUI(Title = "Payment")]
    public partial class Payment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Receiver")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After, IsCopyPaste = true)]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        public AdjustableDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public IdentifiedDate adjustedPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentType", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment type")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public PaymentType paymentType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInformation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement information")]
        public SettlementInformation settlementInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountFactor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discount factor")]
        public EFS_Decimal discountFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool presentValueAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("presentValueAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Present value", IsCopyPaste = true)]
        public Money presentValueAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_hrefSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Formula)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute("href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentQuote", Order = 10)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension", IsVisible = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Percentage")]
		public PaymentQuote paymentQuote;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool customerSettlementPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("customerSettlementPayment", Order = 11)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer settlement payment")]
		public CustomerSettlementPayment customerSettlementPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentSource",Order=12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        public SpheresSource paymentSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool taxSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
//        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tax")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tax", IsChild = true, IsMaster =true, IsMasterVisible = true, MinItem=0 )]
        public Tax[] tax;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension")]
        public bool FillBalise;

		#endregion Members
    }
    #endregion Payment
    #region PaymentCurrency
    public partial class PaymentCurrency : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice paymentCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty paymentCurrencyNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", typeof(Currency),Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 75)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency", IsVisible = true)]
        public Currency paymentCurrencyCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(DeterminationMethod),Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 300)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        public DeterminationMethod paymentCurrencyDeterminationMethod;
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
        [System.Xml.Serialization.XmlAttributeAttribute("href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20231127 [WI755] Implementation Return Swap : Suppression efs_hRefSpecified
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Href efs_href;
        #endregion Members
    }
    #endregion PaymentCurrency
    #region PaymentType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PaymentType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string paymentTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion PaymentType
    #region PeriodicDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 Upd GUI
    public partial class PeriodicDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationStartDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Start Date", IsVisible = false)]
        public AdjustableOrRelativeDate calculationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationEndDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Start Date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation End Date")]
        public AdjustableOrRelativeDate calculationEndDate;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency", IsVisible = false)]
        public CalculationPeriodFrequency calculationPeriodFrequency;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesAdjustments", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustment", IsVisible = false)]
        public BusinessDayAdjustments calculationPeriodDatesAdjustments;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustment")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion PeriodicDates
    #region PositiveMoney
    /// <summary>
    ///  A type defining a positive money amount 
    /// </summary>
    // EG 20140702 New build FpML4.4 New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PositiveMoney : Money
    {
        #region Constructors
        public PositiveMoney()
            : base()
        {
        }
        public PositiveMoney(Decimal pAmount, string pCur)
            : base(pAmount, pCur)
        {
        }
        #endregion Constructors
    }
    #endregion PositiveMoney
    #region PricingStructure
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VolatilityRepresentation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxCurve))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditCurve))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(YieldCurve))]
    public abstract class PricingStructure
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString",Order = 1)]
        public string name;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 2)]
        public Currency currency;
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
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion PricingStructure
    #region PricingStructureReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingStructureReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PricingStructureReference
    #region PrincipalExchanges
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PrincipalExchanges : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialExchange", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Initial Exchange")]
        public EFS_Boolean initialExchange;
        [System.Xml.Serialization.XmlElementAttribute("finalExchange", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Final Exchange")]
        public EFS_Boolean finalExchange;
        [System.Xml.Serialization.XmlElementAttribute("intermediateExchange", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Intermediate Exchange")]
        public EFS_Boolean intermediateExchange;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion PrincipalExchanges
    #region Product
    //FI 20130701 [18745] Add EfsML.v30.Notification.CashBalanceReport
    // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapOption removed
    // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapTransactionSupplement added
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AdditionalInvoice))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(BondOption.BondOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(BulletPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BuyAndSellBack))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CapFloor))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EfsML.v30.CashBalance.CashBalance))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EfsML.v30.Notification.CashBalanceReport ))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CashBalanceInterest))]

    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommoditySpot))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommoditySwap))]

    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationSwap))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationSwapOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditDefaultSwap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditDefaultSwapOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditNote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DebtSecurity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DebtSecurityTransaction))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendSwapTransactionSupplement))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendSwapTransactionSupplementOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeBase))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeLongFormBase))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeShortFormBase))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySecurityTransaction))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedDerivative))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Fra))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FutureTransaction))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxAverageRateOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxBarrierOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxDigitalOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxLeg))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxOptionLeg))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FxSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Invoice))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoiceSettlement))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(LoanDeposit))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(NettedSwapBase))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(OptionBase))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(OptionBaseExtended))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Repo))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwapBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SecurityLending))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Strategy))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Swap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Swaption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(TermDeposit))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceSwapTransactionSupplement))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceSwapOption))]
    public partial class Product : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("productType",Order=3)]
        public ProductType[] productType;
        [System.Xml.Serialization.XmlElementAttribute("productId",Order=4)]
        public ProductId[] productId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLocked = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Product
    #region ProductId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ProductId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string productIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
    }
    #endregion ProductId
    #region ProductReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ProductReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion ProductReference
    #region ProductType
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ProductType 
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string productTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion ProductType

    #region QuotedCurrencyPair
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class QuotedCurrencyPair : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency1", Order = 1)]
        [ControlGUI(Name = "currency1", LblWidth = 80, Width = 75)]
        public Currency currency1;
        [System.Xml.Serialization.XmlElementAttribute("currency2", Order = 2)]
        [ControlGUI(Name = "currency2", LblWidth = 80, Width = 75)]
        public Currency currency2;
        [System.Xml.Serialization.XmlElementAttribute("quoteBasis", Order = 3)]
        [ControlGUI(Name = "quoteBasis", LblWidth = 80, Width = 190, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public QuoteBasisEnum quoteBasis;
        #endregion Members
    }
    #endregion QuotedCurrencyPair

    #region Rate (Unused)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FloatingRate))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FloatingRateCalculation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InflationRateCalculation))]
    public abstract class Rate
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion Rate
    #region RateObservation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RateObservation
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Reset date")]
        public EFS_Date resetDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedFixingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedFixingDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Adjusted fixing date")]
        public EFS_Date adjustedFixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observedRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Observed rate")]
        public EFS_Decimal observedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool treatedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("treatedRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Treated rate")]
        public EFS_Decimal treatedRate;
        [System.Xml.Serialization.XmlElementAttribute("observationWeight", Order = 5)]
        [ControlGUI(Name = "Observation weight", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger observationWeight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateReference", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Rate reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Rate)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.RateIndex)]
        public RateReference rateReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forecastRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forecastRate", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forecast rate")]
        public EFS_Decimal forecastRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool treatedForecastRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("treatedForecastRate", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Treated forecast rate")]
        public EFS_Decimal treatedForecastRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    #endregion RateObservation
    #region RateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RateReference : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }
    #endregion RateReference
    #region RateSourcePage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RateSourcePage : SchemeTextGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string rateSourcePageScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion RateSourcePage
    #region Reference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotionalAmountReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestCalculationReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelevantUnderlyingDateReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BusinessCentersReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SpreadScheduleReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PricingStructureReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LegalEntityReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResetDatesReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartyReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PricingDataPointCoordinateReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixedRateReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartyOrAccountReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationScenarioReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PricingParameterDerivativeReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MarketReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ScheduleReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IdentifiedCurrencyReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CalculationPeriodDatesReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ProductReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AssetOrTermPointOrPricingStructureReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AccountReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestRateStreamReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AnyAssetReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BusinessDayAdjustmentsReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventsReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartyOrTradeSideReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SettlementTermsReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PaymentDatesReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestLegCalculationPeriodDatesReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmountReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AssetReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ProtectionTermsReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DateReference))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoiceTradeReference))]
    public abstract class Reference { }
    #endregion Reference
    #region ReferenceAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReferenceAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue referenceAmount;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string referenceAmountScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        #endregion Members
    }
    #endregion ReferenceAmount
    #region ReferenceBank
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferenceBank : ReferenceBankGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceBankId", Order = 1)]
        [ControlGUI(Name = "referenceBank", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ReferenceBankId referenceBankId;
        [System.Xml.Serialization.XmlElementAttribute("referenceBankName", Order = 2)]
        public string referenceBankName;
        #endregion Members
        #region Constructors
        public ReferenceBank()
        {
            referenceBankId = new ReferenceBankId();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #region _referenceBankId
        public static object INIT_referenceBankId(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ReferenceBankControl(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _referenceBankId
        #endregion Methods
    }
    #endregion ReferenceBank
    #region ReferenceBankId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferenceBankId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string referenceBankIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion ReferenceBankId
    #region RelativeDateOffset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AdjustedRelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    public partial class RelativeDateOffset : ItemGUI
    {
        #region Members
        // Interval
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier" , Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20150907 [21317] Add ReferenceGUI
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        // Offset
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [ControlGUI(Name = "dayType", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
        // Convention
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 4)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;
        // Business Centers Choice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "dateRelativeTo", Width = 210)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateReference dateRelativeTo;
        #endregion Members
    }
    #endregion RelativeDateOffset
    #region RelativeDateSequence
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RelativeDateSequence : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", Order = 1)]
        [ControlGUI(Name = "dateRelativeTo", Width = 210)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateReference dateRelativeTo;
        [System.Xml.Serialization.XmlElementAttribute("dateOffset", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date Offset", IsClonable = true, IsMaster = true, IsMasterVisible = true)]
        public DateOffset[] dateOffset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BusinessCentersReference businessCentersReference;
        #endregion Members
    }
    #endregion RelativeDateSequence
    #region RelativeDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RelativeDates : RelativeDateOffset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodSkipSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodSkip", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "periodSkip", Width = 70)]
        public EFS_PosInteger periodSkip;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleBoundsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("scheduleBounds", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "scheduleBounds")]
        public DateRange scheduleBounds;
        #endregion Members
    }
    #endregion RelativeDates
    #region RequiredIdentifierDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RequiredIdentifierDate : DateCalendarGUI
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsLabel = true, Name = "value")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion RequiredIdentifierDate
    #region ResetFrequency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ResetFrequency : Interval
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weeklyRollConventionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("weeklyRollConvention", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "WeeklyRoll", Width = 150)]
        public WeeklyRollConventionEnum weeklyRollConvention;
        #endregion Members
    }
    #endregion ResetFrequency
    #region Rounding
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Rounding : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("roundingDirection", Order = 1)]
        [ControlGUI(Name = "direction", Width = 80)]
        public RoundingDirectionEnum roundingDirection;
        [System.Xml.Serialization.XmlElementAttribute("precision", Order = 2)]
        [ControlGUI(Name = "precision", LblWidth = 62, Width = 25)]
        public EFS_PosInteger precision;
        #endregion Members
    }
    #endregion Rounding
    #region Routing
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	public partial class Routing : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Rounting")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;
        #endregion Members
    }
    #endregion Routing
    #region RoutingExplicitDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RoutingExplicitDetails
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("routingName", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Name")]
        public EFS_String routingName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAddressSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingAddress", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Address")]
        public Address routingAddress;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAccountNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingAccountNumber", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Number")]
        public EFS_String routingAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingReferenceTextSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingReferenceText", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text", IsMaster = true)]
        public EFS_StringArray[] routingReferenceText;
        #endregion Members
    }
    #endregion RoutingExplicitDetails
    #region RoutingId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RoutingId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string routingIdCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = "value", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public string Value;
        #endregion Members
    }
    #endregion RoutingId
    #region RoutingIds
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RoutingIds
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("routingId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Routing Id", IsClonable = true)]
        public RoutingId[] routingId;
        #endregion Members
    }
    #endregion RoutingIds
    #region RoutingIdsAndExplicitDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RoutingIdsAndExplicitDetails
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds), Order = 1)]
        [ControlGUI(Name = "Routing Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Routing Ids", IsMaster = true)]
        public RoutingIds[] routingIds;
        [System.Xml.Serialization.XmlElementAttribute("routingName", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Explicit Details", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Name")]
        public EFS_String routingName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAddressSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingAddress", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Address")]
        public Address routingAddress;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAccountNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingAccountNumber", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Number")]
        public EFS_String routingAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingReferenceTextSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingReferenceText", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text", IsMaster = true)]
        public EFS_StringArray[] routingReferenceText;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Explicit Details")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion RoutingIdsAndExplicitDetails

    #region Schedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmountSchedule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(StrikeSchedule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SpreadSchedule))]
    public partial class Schedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialValue", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Initial Value")]
        public EFS_Decimal initialValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepSpecified;
        [System.Xml.Serialization.XmlElementAttribute("step", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Step", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Step", IsClonable = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public Step[] step;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        [ControlGUI(IsLabel = false, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Schedule
    #region ScheduleReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ScheduleReference 
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute("href",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }                           
        #endregion Members
    }
    #endregion ScheduleReference
    #region SettlementInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SettlementInformation : ItemGUI
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
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", typeof(SettlementInstruction), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Instruction", IsVisible = true)]
        public SettlementInstruction informationInstruction;
        #endregion Members
    }
    #endregion SettlementInformation
    #region SettlementInstruction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SettlementInstruction 
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementMethod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public SettlementMethod settlementMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool correspondentInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("correspondentInformation", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Correspondent Information")]
        public CorrespondentInformation correspondentInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intermediaryInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("intermediaryInformation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Intermediary Information")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Intermediary Information", IsClonable = true, IsChild = true)]
        public IntermediaryInformation[] intermediaryInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiaryBankSpecified;
        [System.Xml.Serialization.XmlElementAttribute("beneficiaryBank", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary Bank")]
        public Routing beneficiaryBank;
        [System.Xml.Serialization.XmlElementAttribute("beneficiary", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary", IsVisible = false)]
        public Beneficiary beneficiary;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool depositoryPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("depositoryPartyReference", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference depositoryPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool splitSettlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("splitSettlement", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Split Settlement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Split Settlement", IsClonable = true, IsChild = true)]
        public SplitSettlement[] splitSettlement;
        #endregion Members
    }
    #endregion SettlementInstruction
    #region SettlementMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SettlementMethod : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementMethodScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
	}
    #endregion SettlementMethod
    #region SettlementPriceSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SettlementPriceSource : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementPriceSourceScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion SettlementPriceSource
    #region SettlementRateSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SettlementRateSource : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Settlement Rate Source")]
        public EFS_RadioChoice source;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sourceInformationSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("informationSource", typeof(InformationSource), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Information Source", IsVisible = true)]
        public InformationSource sourceInformationSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sourceCashSettlementReferenceBanksSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementReferenceBanks", typeof(CashSettlementReferenceBanks), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Cash Settlement ReferenceBanks", IsVisible = true)]
        public CashSettlementReferenceBanks sourceCashSettlementReferenceBanks;
        #endregion Members
        #region Constructors
        public SettlementRateSource()
        {
            sourceInformationSource = new InformationSource();
            sourceCashSettlementReferenceBanks = new CashSettlementReferenceBanks();
        }
        #endregion Constructors
    }
    #endregion SettlementRateSource
    #region SharedAmericanExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityBermudaExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAmericanExercise))]
    public partial class SharedAmericanExercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("commencementDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commencement date", IsVisible = false)]
        public AdjustableOrRelativeDate commencementDate;
        [System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commencement date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date", IsVisible = false)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "latest Exercise Time")]
        [ControlGUI(Name = "value")]
        public BusinessCenterTime latestExerciseTime;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion SharedAmericanExercise
    #region SimplePayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ClassifiedPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Premium))]
    public partial class SimplePayment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date", IsVisible = false)]
        public AdjustableOrRelativeAndAdjustedDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion SimplePayment
    #region SplitSettlement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SplitSettlement : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("splitSettlementAmount", Order = 1)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money splitSettlementAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiaryBankSpecified;
        [System.Xml.Serialization.XmlElementAttribute("beneficiaryBank", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary Bank")]
        public Routing beneficiaryBank;
        [System.Xml.Serialization.XmlElementAttribute("beneficiary", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary", IsVisible = false)]
        public Routing beneficiary;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion SplitSettlement
    #region SpreadSchedule
    public partial class SpreadSchedule : Schedule 
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        public SpreadScheduleType type;
        #endregion Members
    }
    #endregion SpreadSchedule
    #region SpreadScheduleReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SpreadScheduleReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion SpreadScheduleReference
    #region SpreadScheduleType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SpreadScheduleType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string spreadScheduleTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion SpreadScheduleType
    #region Step
    // 20080523 EG Ticket : 16221
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Step
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("stepDate", Order = 1)]
        [ControlGUI(Name = "Step Date", LblWidth = 75)]
        public EFS_Date stepDate;
        [System.Xml.Serialization.XmlElementAttribute("stepValue", Order = 2)]
        [ControlGUI(Name = "Value", LblWidth = 45)]
        public EFS_Decimal stepValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public EFS_Id efs_id;
        #endregion Members
     }
    #endregion Step
	#region StreetAddress
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	public partial class StreetAddress : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("streetLine", Order = 1)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Street line", IsClonable = true, IsMaster = true, IsChild = false)]
		public EFS_StringArray[] streetLine;
		#endregion Members
	}
	#endregion StreetAddress
    #region Strike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Strike : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("strikeRate", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Strike rate")]
        public EFS_Decimal strikeRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("buyer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Buyer")]
        public IdentifiedPayerReceiver buyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("seller", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seller")]
        public IdentifiedPayerReceiver seller;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    #endregion Strike
    #region StrikeSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StrikeSchedule : Schedule
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("buyer", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Buyer")]
        public IdentifiedPayerReceiver buyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("seller", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seller")]
        public IdentifiedPayerReceiver seller;
        #endregion Members
    }
    #endregion StrikeSchedule
    #region Stub (included StubValue)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Stub : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stub Type")]
        public EFS_RadioChoice stubType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubTypeFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRate", typeof(FloatingRate), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Floating Rate")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Floating Rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public FloatingRate[] stubTypeFloatingRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubTypeFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubRate", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "rate", Width = 100, Regex = EFSRegex.TypeRegex.RegexFixedRate)]
        public EFS_Decimal stubTypeFixedRate;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubTypeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubAmount", typeof(Money), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "amount")]
        public Money stubTypeAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubStartDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Start Date")]
        public AdjustableOrRelativeDate stubStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubEndDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "End Date")]
        public AdjustableOrRelativeDate stubEndDate;
        #endregion Members
    }
    #endregion Stub (included StubValue)

    // *************************************************
    // Contenu Provisions déplacée de IRD vers SHARED
    // pour partage avec les FXOptions
    // EG 20180514 [23812] Report
    // *************************************************

    #region EarlyTerminationProvision
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EarlyTerminationProvision : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Type")]
        public EFS_RadioChoice earlyTermination;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationMandatorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTermination", typeof(MandatoryEarlyTermination), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Mandatory Early Termination", IsVisible = true)]
        public MandatoryEarlyTermination earlyTerminationMandatory;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationOptionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionalEarlyTermination", typeof(OptionalEarlyTermination), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Optional Early Termination", IsVisible = true)]
        public OptionalEarlyTermination earlyTerminationOptional;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatoryEarlyTerminationDateTenorSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period after trade date of the mandatory early termination date")]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationDateTenor", typeof(Interval), Order = 3)]
        public Interval mandatoryEarlyTerminationDateTenor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionalEarlyTerminationParametersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationParameters", typeof(ExercisePeriod), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Optional Early Termination Parameters")]
        public ExercisePeriod optionalEarlyTerminationParameters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion EarlyTerminationProvision

    #region CashPriceMethod
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CashPriceMethod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementReferenceBanksSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementReferenceBanks", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Banks")]
        public CashSettlementReferenceBanks cashSettlementReferenceBanks;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementCurrency", Order = 2)]
        [ControlGUI(Name = "currency", LblWidth = 115)]
        public Currency cashSettlementCurrency;
        [System.Xml.Serialization.XmlElementAttribute("quotationRateType", Order = 3)]
        [ControlGUI(Name = "quotationRateType", LblWidth = 115)]
        public QuotationRateTypeEnum quotationRateType;
        #endregion Members
    }
    #endregion CashPriceMethod
    #region CashSettlement
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("cashSettlement", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class CashSettlement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementValuationTime", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Time", IsVisible = false)]
        public BusinessCenterTime cashSettlementValuationTime;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementValuationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset cashSettlementValuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementPaymentDate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Date")]
        public CashSettlementPaymentDate cashSettlementPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementMethod", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Method")]
        public EFS_DropDownChoice cashSettlementMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodcashPriceMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashPriceMethod", typeof(CashPriceMethod), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "cashPriceMethod", IsVisible = true)]
        public CashPriceMethod cashSettlementMethodcashPriceMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodcashPriceAlternateMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashPriceAlternateMethod", typeof(CashPriceMethod), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "cashPriceAlternateMethod", IsVisible = true)]
        public CashPriceMethod cashSettlementMethodcashPriceAlternateMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodparYieldCurveAdjustedMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parYieldCurveAdjustedMethod", typeof(YieldCurveMethod), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "parYieldCurveAdjustedMethod", IsVisible = true)]
        public YieldCurveMethod cashSettlementMethodparYieldCurveAdjustedMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("zeroCouponYieldAdjustedMethod", typeof(YieldCurveMethod), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "zeroCouponYieldAdjustedMethod", IsVisible = true)]
        public YieldCurveMethod cashSettlementMethodzeroCouponYieldAdjustedMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodparYieldCurveUnadjustedMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parYieldCurveUnadjustedMethod", typeof(YieldCurveMethod), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "parYieldCurveUnadjustedMethod", IsVisible = true)]
        public YieldCurveMethod cashSettlementMethodparYieldCurveUnadjustedMethod;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CashSettlement
    #region CashSettlementPaymentDate
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("cashSettlementPaymentDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class CashSettlementPaymentDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payment Date")]
        public EFS_RadioChoice paymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateAdjustablesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates paymentDateAdjustables;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateRelativeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
        public RelativeDateOffset paymentDateRelative;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateBusinessDateRangeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDateRange", typeof(BusinessDateRange), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Business Date Range", IsVisible = true)]
        public BusinessDateRange paymentDateBusinessDateRange;

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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CashSettlementPaymentDate

    #region EarlyTerminationEvent
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EarlyTerminationEvent : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
        [ControlGUI(Name = "adjusted Exercise Date")]
        public EFS_Date adjustedExerciseDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 2)]
        [ControlGUI(Name = "adjusted Early Termination Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date adjustedEarlyTerminationDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementValuationDate", Order = 3)]
        [ControlGUI(Name = "adjusted CashSettlement valuation date")]
        public EFS_Date adjustedCashSettlementValuationDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementPaymentDate", Order = 4)]
        [ControlGUI(Name = "adjusted CashSettlement payment date")]
        public EFS_Date adjustedCashSettlementPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedExerciseFeePaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseFeePaymentDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "adjusted Exercise Fee payment date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date adjustedExerciseFeePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
        public EarlyTerminationEvent()
        {
            adjustedExerciseFeePaymentDate = new EFS_Date();
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
    #endregion EarlyTerminationEvent
    #region ExercisePeriod
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ExercisePeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("earliestExerciseDateTenor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Earliest exercise date tenor", LblWidth = 180)]
        public Interval earliestExerciseDateTenor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Exercise frequency", LblWidth = 180)]
        public Interval exerciseFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    #endregion ExercisePeriod
    #region MandatoryEarlyTermination
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MandatoryEarlyTermination
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableDate mandatoryEarlyTerminationDate;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgent", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Agent", IsVisible = false)]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Agent")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement", IsVisible = false)]
        public CashSettlement cashSettlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mandatoryEarlyTerminationAdjustedDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationAdjustedDates", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted Dates")]
        public MandatoryEarlyTerminationAdjustedDates mandatoryEarlyTerminationAdjustedDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion MandatoryEarlyTermination
    #region MandatoryEarlyTerminationAdjustedDates
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MandatoryEarlyTerminationAdjustedDates
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 1)]
        [ControlGUI(Name = "EarlyTerminationDate")]
        public EFS_Date adjustedEarlyTerminationDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementValuationDate", Order = 2)]
        [ControlGUI(Name = "CashSettlementValuationDate")]
        public EFS_Date adjustedCashSettlementValuationDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementPaymentDate", Order = 3)]
        [ControlGUI(Name = "CashSettlementPaymentDate")]
        public EFS_Date adjustedCashSettlementPaymentDate;
        #endregion Members
    }
    #endregion MandatoryEarlyTerminationAdjustedDates
    #region OptionalEarlyTermination
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class OptionalEarlyTermination : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool singlePartyOptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singlePartyOption", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party Option")]
        public SinglePartyOption singlePartyOption;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice optionalEarlyTerminationExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionalEarlyTerminationExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise optionalEarlyTerminationExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionalEarlyTerminationExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise optionalEarlyTerminationExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionalEarlyTerminationExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise optionalEarlyTerminationExerciseEuropean;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseNoticeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseNotice", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Notice", IsClonable = true, IsChild = true)]
        public ExerciseNotice[] exerciseNotice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool followUpConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("followUpConfirmation", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "followUpConfirmation")]
        public EFS_Boolean followUpConfirmation;

        [System.Xml.Serialization.XmlElementAttribute("calculationAgent", Order = 7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Agent", IsVisible = false)]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Agent")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement", IsVisible = false)]
        public CashSettlement cashSettlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionalEarlyTerminationAdjustedDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationAdjustedDates", Order = 9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Optional Early Termination Adjusted Dates")]
        public OptionalEarlyTerminationAdjustedDates optionalEarlyTerminationAdjustedDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool isImplicitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isImplicit", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Implicit termination")]
        public EFS_Boolean isImplicit;
        #endregion Members
    }
    #endregion OptionalEarlyTermination
    #region OptionalEarlyTerminationAdjustedDates
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class OptionalEarlyTerminationAdjustedDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationEvent", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 11)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true)]
        public EarlyTerminationEvent[] earlyTerminationEvent;
        #endregion Members
    }
    #endregion OptionalEarlyTerminationAdjustedDates
    #region SinglePartyOption
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SinglePartyOption : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [ControlGUI(Name = "Seller")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrTradeSideReference sellerPartyReference;
        #endregion Members
    }
    #endregion SinglePartyOption
    #region YieldCurveMethod
    // EG 20180514 [23812] Report En provenance de FpML_Ird
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class YieldCurveMethod
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementRateSource", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "settlementRateSource")]
        public SettlementRateSource settlementRateSource;
        [System.Xml.Serialization.XmlElementAttribute("quotationRateType", Order = 2)]
        [ControlGUI(Name = "quotationRateType", LblWidth = 115)]
        public QuotationRateTypeEnum quotationRateType;
        #endregion Members
    }
    #endregion YieldCurveMethod
}