#region using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.MiFIDII_Extended;
using EfsML.v30.Doc;
using EfsML.v30.Security;
using EfsML.v30.Security.Shared;
using EfsML.v30.Fix;
using EfsML.v30.FixedIncome;
using EfsML.v30.FuturesAndOptions;
using EfsML.v30.Invoice;
using EfsML.v30.Ird;
using EfsML.v30.CommodityDerivative;

using EfsML.v30.LoanDeposit;

using EfsML.v30.Notification;

using EfsML.v30.MarginRequirement;
using EfsML.v30.Settlement;
using EfsML.v30.Settlement.Message;
using EfsML.v30.Shared;
using EfsML.v30.PosRequest;
using EfsML.v30.CashBalance;
using EfsML.v30.CashBalanceInterest;

using FpML.Enum;

using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.BondOption;
using FpML.v44.CorrelationSwaps;
using FpML.v44.Cd;
using FpML.v44.DividendSwaps;
using FpML.v44.Doc;
using FpML.v44.Eq;
using FpML.v44.Eqd;
using FpML.v44.Eq.Shared;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Mktenv.ToDefine;
using FpML.v44.Option.Shared;
using FpML.v44.ValuationResults;
using FpML.v44.ReturnSwaps;
using FpML.v44.Riskdef;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.VarianceSwaps;

using FixML.Interface;
using FixML.Enum;
using FixML.v50SP1;

#endregion using directives


namespace FpML.v44.Shared
{
    #region AccountReference
    public partial class AccountReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion

    #region Address
    public partial class Address : ICloneable, IAddress
    {
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Address clone = new Address
            {
                city = new EFS_String(this.city.Value),
                country = (Country)this.country.Clone(),
                postalCode = new EFS_String(this.postalCode.Value),
                state = new EFS_String(this.state.Value),
                streetAddress = new StreetAddress()
            };
            clone.streetAddress.streetLine = new EFS_StringArray[this.streetAddress.streetLine.Length];
            for (int i = 0; i < this.streetAddress.streetLine.Length; i++)
                clone.streetAddress.streetLine[i] = new EFS_StringArray(this.streetAddress.streetLine[i].Value);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IAddress Membres
        IStreetAddress IAddress.StreetAddress
        {
            get { return this.streetAddress; }
            set { streetAddress = (StreetAddress)value; }
        }
        EFS_String IAddress.City
        {
            get { return this.city; }
            set { this.city = value; }
        }
        EFS_String IAddress.State
        {
            get { return this.state; }
            set { this.state = value; }
        }
        IScheme IAddress.Country
        {
            get { return this.country; }
            set { this.country = (Country)value; }
        }
        EFS_String IAddress.PostalCode
        {
            get { return this.postalCode; }
            set { this.postalCode = value; }
        }
        #endregion
    }
    #endregion Address

    #region AdjustableDate
    public partial class AdjustableDate : ICloneable, IAdjustableDate
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
        #region Constructors
        // EG 20221201 [25639] [WI482] Upd
        public AdjustableDate()
        {
            unadjustedDate = new IdentifiedDate();
            dateAdjustments = new BusinessDayAdjustments();
            efs_id = new EFS_Id();
        }
        #endregion Constructors

        #region IAdjustableDate Members
        IAdjustedDate IAdjustableDate.UnadjustedDate
        {
            set { this.unadjustedDate = (IdentifiedDate)value; }
            get { return this.unadjustedDate; }
        }
        IBusinessDayAdjustments IAdjustableDate.DateAdjustments
        {
            set { this.dateAdjustments = (BusinessDayAdjustments)value; }
            get { return this.dateAdjustments; }
        }
        string IAdjustableDate.Efs_id
        {
            set { this.efs_id.Value = value; }
            get { return this.efs_id.Value; }
        }
        object IAdjustableDate.Clone() { return this.Clone(); }
        IAdjustedDate IAdjustableDate.GetAdjustedDate() { return (IAdjustedDate)this.unadjustedDate.Clone(); }
        #endregion IAdjustableDate Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            AdjustableDate clone = new AdjustableDate
            {
                unadjustedDate = (IdentifiedDate)this.unadjustedDate.Clone(),
                dateAdjustments = (BusinessDayAdjustments)this.dateAdjustments.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion AdjustableDate
    #region AdjustableDates
    public partial class AdjustableDates : IAdjustableDates
    {
        #region IAdjustableDates Members
        IAdjustedDate[] IAdjustableDates.UnadjustedDate
        {
            get { return this.unadjustedDate; }
            set { this.unadjustedDate = (IdentifiedDate[])value; }
        }
        IBusinessDayAdjustments IAdjustableDates.DateAdjustments { get { return (IBusinessDayAdjustments)this.dateAdjustments; } }
        DateTime IAdjustableDates.this[int pIndex] { get { return this.unadjustedDate[pIndex].DateValue; } }
        #endregion IAdjustableDates Members
    }
    #endregion AdjustableDates
    #region AdjustableOrRelativeAndAdjustedDate
    public partial class AdjustableOrRelativeAndAdjustedDate : IAdjustableOrRelativeAndAdjustedDate
    {
        #region Constructors
        public AdjustableOrRelativeAndAdjustedDate()
        {
            adjustableOrRelativeDateRelativeDate = new RelativeDateOffset();
            adjustableOrRelativeDateAdjustableDate = new AdjustableDate();
            adjustedDate = new IdentifiedDate();
        }
        #endregion Constructors

        #region IAdjustableOrRelativeAndAdjustedDate Members
        bool IAdjustableOrRelativeAndAdjustedDate.AdjustedDateSpecified
        {
            set { this.adjustedDateSpecified = value; }
            get { return this.adjustedDateSpecified; }
        }
        IAdjustedDate IAdjustableOrRelativeAndAdjustedDate.AdjustedDate
        {
            set { this.adjustedDate = (IdentifiedDate)value; }
            get { return this.adjustedDate; }
        }

        #endregion
        #region IAdjustableOrRelativeDate Members
        bool IAdjustableOrRelativeDate.AdjustableDateSpecified
        {
            set { this.adjustableOrRelativeDateAdjustableDateSpecified = value; }
            get { return this.adjustableOrRelativeDateAdjustableDateSpecified; }
        }
        IAdjustableDate IAdjustableOrRelativeDate.AdjustableDate
        {
            set { this.adjustableOrRelativeDateAdjustableDate = (AdjustableDate)value; }
            get { return this.adjustableOrRelativeDateAdjustableDate; }
        }
        bool IAdjustableOrRelativeDate.RelativeDateSpecified
        {
            set { this.adjustableOrRelativeDateRelativeDateSpecified = value; }
            get { return this.adjustableOrRelativeDateRelativeDateSpecified; }
        }
        IRelativeDateOffset IAdjustableOrRelativeDate.RelativeDate
        {
            set { this.adjustableOrRelativeDateRelativeDate = (RelativeDateOffset)value; }
            get { return this.adjustableOrRelativeDateRelativeDate; }
        }
        IAdjustableDate IAdjustableOrRelativeDate.CreateAdjustableDate
        {
            get { return new AdjustableDate(); }
        }
        IRelativeDateOffset IAdjustableOrRelativeDate.CreateRelativeDate
        {
            get { return new RelativeDateOffset(); }
        }
        #endregion IAdjustableOrRelativeDate Members
    }
    #endregion AdjustableOrRelativeAndAdjustedDate
    #region AdjustableOrRelativeDate
    public partial class AdjustableOrRelativeDate : IAdjustableOrRelativeDate
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
        #region Constructors
        public AdjustableOrRelativeDate()
        {
            adjustableOrRelativeDateRelativeDate = new RelativeDateOffset();
            adjustableOrRelativeDateAdjustableDate = new AdjustableDate();
        }
        #endregion Constructors

        #region IAdjustableOrRelativeDate Members
        bool IAdjustableOrRelativeDate.AdjustableDateSpecified
        {
            set { this.adjustableOrRelativeDateAdjustableDateSpecified = value; }
            get { return this.adjustableOrRelativeDateAdjustableDateSpecified; }
        }
        IAdjustableDate IAdjustableOrRelativeDate.AdjustableDate
        {
            set { this.adjustableOrRelativeDateAdjustableDate = (AdjustableDate)value; }
            get { return this.adjustableOrRelativeDateAdjustableDate; }
        }
        bool IAdjustableOrRelativeDate.RelativeDateSpecified
        {
            set { this.adjustableOrRelativeDateRelativeDateSpecified = value; }
            get { return this.adjustableOrRelativeDateRelativeDateSpecified; }
        }
        IRelativeDateOffset IAdjustableOrRelativeDate.RelativeDate
        {
            set { this.adjustableOrRelativeDateRelativeDate = (RelativeDateOffset)value; }
            get { return this.adjustableOrRelativeDateRelativeDate; }
        }
        IAdjustableDate IAdjustableOrRelativeDate.CreateAdjustableDate
        {
            get { return new AdjustableDate(); }
        }
        IRelativeDateOffset IAdjustableOrRelativeDate.CreateRelativeDate
        {
            get { return new RelativeDateOffset(); }
        }
        #endregion IAdjustableOrRelativeDate Members
    }
    #endregion AdjustableOrRelativeDate
    #region AdjustableOrRelativeDates
    public partial class AdjustableOrRelativeDates : IAdjustableOrRelativeDates
    {
        #region Constructors
        public AdjustableOrRelativeDates()
        {
            adjustableOrRelativeDatesRelativeDates = new RelativeDates();
            adjustableOrRelativeDatesAdjustableDates = new AdjustableDates();
        }
        #endregion Constructors

        #region IAdjustableOrRelativeDates Members
        bool IAdjustableOrRelativeDates.AdjustableDatesSpecified
        {
            set { this.adjustableOrRelativeDatesAdjustableDatesSpecified = value; }
            get { return this.adjustableOrRelativeDatesAdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableOrRelativeDates.AdjustableDates
        {
            set { this.adjustableOrRelativeDatesAdjustableDates = (AdjustableDates)value; }
            get { return this.adjustableOrRelativeDatesAdjustableDates; }
        }
        bool IAdjustableOrRelativeDates.RelativeDatesSpecified
        {
            set { this.adjustableOrRelativeDatesRelativeDatesSpecified = value; }
            get { return this.adjustableOrRelativeDatesRelativeDatesSpecified; }
        }
        IRelativeDates IAdjustableOrRelativeDates.RelativeDates
        {
            set { this.adjustableOrRelativeDatesRelativeDates = (RelativeDates)value; }
            get { return this.adjustableOrRelativeDatesRelativeDates; }
        }
        #endregion IAdjustableOrRelativeDates Members
    }
    #endregion AdjustableOrRelativeDates
    #region AdjustableDatesOrRelativeDateOffset
    // EG 20140702 New
    public partial class AdjustableDatesOrRelativeDateOffset : IAdjustableDatesOrRelativeDateOffset
    {
        #region Constructors
        public AdjustableDatesOrRelativeDateOffset()
        {
            dtType_AdjustableDates = new AdjustableDates();
            dtType_RelativeDateOffset = new RelativeDateOffset();
        }
        #endregion Constructors
        #region IAdjustableRelativeOrPeriodicDates Members
        bool IAdjustableDatesOrRelativeDateOffset.AdjustableDatesSpecified
        {
            set { this.dtType_AdjustableDatesSpecified = value; }
            get { return this.dtType_AdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableDatesOrRelativeDateOffset.AdjustableDates
        {
            set { this.dtType_AdjustableDates = (AdjustableDates)value; }
            get { return this.dtType_AdjustableDates; }
        }
        bool IAdjustableDatesOrRelativeDateOffset.RelativeDateOffsetSpecified
        {
            set { this.dtType_RelativeDateOffsetSpecified = value; }
            get { return this.dtType_RelativeDateOffsetSpecified; }
        }
        IRelativeDateOffset IAdjustableDatesOrRelativeDateOffset.RelativeDateOffset
        {
            set { this.dtType_RelativeDateOffset = (RelativeDateOffset)value; }
            get { return this.dtType_RelativeDateOffset; }
        }
        #endregion IAdjustableRelativeOrPeriodicDates Members
    }
    #endregion AdjustableDatesOrRelativeDateOffset
    #region AdjustableRelativeOrPeriodicDates
    public partial class AdjustableRelativeOrPeriodicDates : IAdjustableRelativeOrPeriodicDates
    {
        #region Constructors
        public AdjustableRelativeOrPeriodicDates()
        {
            adjustableRelativeOrPeriodicAdjustableDates = new AdjustableDates();
            adjustableRelativeOrPeriodicRelativeDateSequence = new RelativeDateSequence();
            adjustableRelativeOrPeriodicPeriodicDates = new PeriodicDates();
        }
        #endregion Constructors

        #region IAdjustableRelativeOrPeriodicDates Members
        bool IAdjustableRelativeOrPeriodicDates.AdjustableDatesSpecified
        {
            get { return this.adjustableRelativeOrPeriodicAdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableRelativeOrPeriodicDates.AdjustableDates
        {
            get { return this.adjustableRelativeOrPeriodicAdjustableDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates.PeriodicDatesSpecified
        {
            set { this.adjustableRelativeOrPeriodicPeriodicDatesSpecified = value; }
            get { return this.adjustableRelativeOrPeriodicPeriodicDatesSpecified; }
        }
        IPeriodicDates IAdjustableRelativeOrPeriodicDates.PeriodicDates
        {
            set { this.adjustableRelativeOrPeriodicPeriodicDates = (PeriodicDates)value; }
            get { return this.adjustableRelativeOrPeriodicPeriodicDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates.RelativeDateSequenceSpecified
        {
            set { this.adjustableRelativeOrPeriodicRelativeDateSequenceSpecified = value; }
            get { return this.adjustableRelativeOrPeriodicRelativeDateSequenceSpecified; }
        }
        IRelativeDateSequence IAdjustableRelativeOrPeriodicDates.RelativeDateSequence
        {
            get { return this.adjustableRelativeOrPeriodicRelativeDateSequence; }
        }
        #endregion IAdjustableRelativeOrPeriodicDates Members
    }
    #endregion AdjustableRelativeOrPeriodicDates
    #region AdjustableRelativeOrPeriodicDates2
    // EG 20140702 New
    public partial class AdjustableRelativeOrPeriodicDates2 : IAdjustableRelativeOrPeriodicDates2
    {
        #region Constructors
        public AdjustableRelativeOrPeriodicDates2()
        {
            dtType_AdjustableDates = new AdjustableDates();
            dtType_RelativeDates = new RelativeDates();
            dtType_PeriodicDates = new PeriodicDates();
        }
        #endregion Constructors

        #region IAdjustableRelativeOrPeriodicDates2 Members
        bool IAdjustableRelativeOrPeriodicDates2.AdjustableDatesSpecified
        {
            set { this.dtType_AdjustableDatesSpecified = value; }
            get { return this.dtType_AdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableRelativeOrPeriodicDates2.AdjustableDates
        {
            set { this.dtType_AdjustableDates = (AdjustableDates)value; }
            get { return this.dtType_AdjustableDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates2.PeriodicDatesSpecified
        {
            set { this.dtType_PeriodicDatesSpecified = value; }
            get { return this.dtType_PeriodicDatesSpecified; }
        }
        IPeriodicDates IAdjustableRelativeOrPeriodicDates2.PeriodicDates
        {
            set { this.dtType_PeriodicDates = (PeriodicDates)value; }
            get { return this.dtType_PeriodicDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates2.RelativeDatesSpecified
        {
            set { this.dtType_RelativeDatesSpecified = value; }
            get { return this.dtType_RelativeDatesSpecified; }
        }
        IRelativeDates IAdjustableRelativeOrPeriodicDates2.RelativeDates
        {
            set { this.dtType_RelativeDates = (RelativeDates)value; }
            get { return this.dtType_RelativeDates; }
        }
        #endregion IAdjustableRelativeOrPeriodicDates2 Members
    }
    #endregion AdjustableRelativeOrPeriodicDates2
    #region AdjustedRelativeDateOffset
    public partial class AdjustedRelativeDateOffset : IAdjustedRelativeDateOffset
    {
        #region IRelativeDateOffset Members
        string IRelativeDateOffset.DateRelativeToValue
        {
            set { this.dateRelativeTo.href = value; }
            get { return this.dateRelativeTo.href; }
        }
        IOffset IRelativeDateOffset.GetOffset { get { return this.GetOffset; } }
        IBusinessDayAdjustments IRelativeDateOffset.GetAdjustments { get { return this.GetAdjustments; } }
        #endregion IRelativeDateOffset Members
    }
    #endregion AdjustedRelativeDateOffset
    #region AmericanExercise
    public partial class AmericanExercise : IAmericanExercise
    {
        #region Constructors
        public AmericanExercise()
        {
            relevantUnderlyingDate = new AdjustableOrRelativeDates();
            commencementDate = new AdjustableOrRelativeDate();
            expirationDate = new AdjustableOrRelativeDate();
        }
        #endregion Constructors

        #region IAmericanExercise Members
        IAdjustableOrRelativeDate IAmericanExercise.CommencementDate { get { return this.commencementDate; } }
        IAdjustableOrRelativeDate IAmericanExercise.ExpirationDate { get { return this.expirationDate; } }
        bool IAmericanExercise.RelevantUnderlyingDateSpecified
        {
            set { this.relevantUnderlyingDateSpecified = value; }
            get { return this.relevantUnderlyingDateSpecified; }
        }
        IAdjustableOrRelativeDates IAmericanExercise.RelevantUnderlyingDate
        {
            set { this.relevantUnderlyingDate = (AdjustableOrRelativeDates)value; }
            get { return this.relevantUnderlyingDate; }
        }
        bool IAmericanExercise.LatestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime IAmericanExercise.LatestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        bool IAmericanExercise.ExerciseFeeScheduleSpecified
        {
            set { this.exerciseFeeScheduleSpecified = value; }
            get { return this.exerciseFeeScheduleSpecified; }
        }
        IExerciseFeeSchedule IAmericanExercise.ExerciseFeeSchedule
        {
            get { return this.exerciseFeeSchedule; }
        }
        bool IAmericanExercise.MultipleExerciseSpecified
        {
            set { this.multipleExerciseSpecified = value; }
            get { return this.multipleExerciseSpecified; }
        }

        IMultipleExercise IAmericanExercise.MultipleExercise
        {
            set { this.multipleExercise = (MultipleExercise)value; }
            get { return this.multipleExercise; }
        }
        IMultipleExercise IAmericanExercise.CreateMultipleExercise()
        {
            MultipleExercise multiple = new MultipleExercise
            {
                integralMultipleAmountSpecified = false,
                minimumNotionalAmountSpecified = true,
                minimumNumberOfOptionsSpecified = false,
                maximumNoneSpecified = true,
                maximumNumberOfOptionsSpecified = false,
                maximumNotionalAmountSpecified = false
            };
            return multiple;
        }
        #endregion IAmericanExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.EarliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }

        IBusinessCenterTime IExerciseBase.ExpirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(),
                    hourMinuteTime = new HourMinuteTime()
                };
                return bct;
            }
        }
        // EG 20180514 [23812] Report
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            ScheduleReference[] scheduleReference = null;
            if (ArrFunc.IsFilled(pNotionalReference))
            {
                int nbNotionalReference = pNotionalReference.Length;
                if (0 < nbNotionalReference)
                {
                    scheduleReference = new ScheduleReference[nbNotionalReference];
                    for (int i = 0; i < nbNotionalReference; i++)
                    {
                        scheduleReference[i] = new ScheduleReference();
                        ((IReference)scheduleReference[i]).HRef = pNotionalReference[i];
                    }
                }
            }
            return scheduleReference;
        }
        #endregion IExerciseBase Members
    }
    #endregion AmericanExercise
    #region AmountReference
    public partial class AmountReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion AmountReference
    #region AmountSchedule
    public partial class AmountSchedule : IAmountSchedule
    {
        #region Constructors
        public AmountSchedule()
        {
            currency = new Currency();
        }
        #endregion Constructors

        #region IAmountSchedule Members
        ICurrency IAmountSchedule.Currency { get { return this.currency; } }
        #endregion IAmountSchedule Members
    }
    #endregion AmountSchedule
    #region ArrayPartyReference
    public partial class ArrayPartyReference : IEFS_Array, IReference
    {
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
        #region Constructor
        public ArrayPartyReference() { }
        public ArrayPartyReference(string pPartyReference)
        {
            this.Href = pPartyReference;
        }
        #endregion Constructor
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
        #region IReference Members
        string IReference.HRef
        {
            set { this.Href = value; }
            get { return this.Href; }
        }
        #endregion IReference Members
    }
    #endregion ArrayPartyReference
    #region AutomaticExercise
    public partial class AutomaticExercise : IAutomaticExercise
    {
        #region IAutomaticExercise Members
        decimal IAutomaticExercise.ThresholdRate { get { return this.thresholdRate.DecValue; } }
        #endregion IAutomaticExercise Members
    }
    #endregion AutomaticExercise

    #region Beneficiary
    public partial class Beneficiary : ICloneable, IRouting
    {
        #region Constructors
        public Beneficiary()
        {
            routingIds = new RoutingIds();
            routingExplicitDetails = new RoutingExplicitDetails();
            routingIdsAndExplicitDetails = new RoutingIdsAndExplicitDetails();
        }
        #endregion Constructors
        #region Methods
        #region GetRoutingAccountNumber
        public string GetRoutingAccountNumber()
        {
            return SettlementTools.GetRoutingAccountNumber((IRouting)this);
        }
        #endregion GetRoutingAccountNumber
        #endregion Methods

        #region ICloneable Members
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public object Clone()
        {
            Beneficiary clone = (Beneficiary)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable Members
        #region IRouting Membres
        bool IRouting.RoutingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.RoutingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.RoutingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.RoutingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.RoutingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.RoutingIdsAndExplicitDetails
        {
            get { return this.routingIdsAndExplicitDetails; }
            set { this.routingIdsAndExplicitDetails = (RoutingIdsAndExplicitDetails)value; }
        }
        IRouting IRouting.Clone()
        {
            return (IRouting)this.Clone();
        }
        #endregion IRouting Membres
    }
    #endregion Beneficiary
    #region BermudaExercise
    public partial class BermudaExercise : IBermudaExercise
    {
        #region Constructors
        public BermudaExercise()
        {
            relevantUnderlyingDate = new AdjustableOrRelativeDates();
        }
        #endregion Constructors

        #region IBermudaExercise Members
        IAdjustableOrRelativeDates IBermudaExercise.BermudaExerciseDates { get { return this.bermudaExerciseDates; } }
        bool IBermudaExercise.RelevantUnderlyingDateSpecified { get { return this.relevantUnderlyingDateSpecified; } }
        IAdjustableOrRelativeDates IBermudaExercise.RelevantUnderlyingDate { get { return this.relevantUnderlyingDate; } }
        bool IBermudaExercise.LatestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime IBermudaExercise.LatestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        bool IBermudaExercise.ExerciseFeeScheduleSpecified
        {
            get { return this.exerciseFeeScheduleSpecified; }
        }
        IExerciseFeeSchedule IBermudaExercise.ExerciseFeeSchedule
        {
            get { return this.exerciseFeeSchedule; }
        }
        bool IBermudaExercise.MultipleExerciseSpecified
        {
            set { this.multipleExerciseSpecified = value; }
            get { return this.multipleExerciseSpecified; }
        }
        IMultipleExercise IBermudaExercise.MultipleExercise
        {
            set { this.multipleExercise = (MultipleExercise)value; }
            get { return this.multipleExercise; }
        }
        IMultipleExercise IBermudaExercise.CreateMultipleExercise()
        {
            MultipleExercise multiple = new MultipleExercise
            {
                integralMultipleAmountSpecified = false,
                minimumNotionalAmountSpecified = true,
                minimumNumberOfOptionsSpecified = false,
                maximumNoneSpecified = true,
                maximumNumberOfOptionsSpecified = false,
                maximumNotionalAmountSpecified = false
            };
            return multiple;
        }
        #endregion IBermudaExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.EarliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }

        IBusinessCenterTime IExerciseBase.ExpirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(),
                    hourMinuteTime = new HourMinuteTime()
                };
                return bct;
            }
        }
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            int nbNotionalReference = pNotionalReference.Length;
            ScheduleReference[] scheduleReference = null;
            if (0 < nbNotionalReference)
            {
                scheduleReference = new ScheduleReference[nbNotionalReference];
                for (int i = 0; i < nbNotionalReference; i++)
                {
                    scheduleReference[i] = new ScheduleReference();
                    ((IReference)scheduleReference[i]).HRef = pNotionalReference[i];
                }
            }
            return scheduleReference;
        }
        #endregion IExerciseBase Members
    }
    #endregion BermudaExercise
    #region BusinessCenter
    // EG 20140702 New build FpML4.4
    // EG 20150422 [20513] BANCAPERTA New id|efs_id
    public partial class BusinessCenter : IEFS_Array, ICloneable, IBusinessCenter
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
        #region Constructors
        public BusinessCenter() : this(string.Empty) { }
        public BusinessCenter(string pValue)
        {
            businessCenterScheme = "http://www.fpml.org/coding-scheme/business-center-6-5";
            Value = pValue;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        #region IBusinessCenter Members
        string IBusinessCenter.BusinessCenterScheme
        {
            set { this.businessCenterScheme = value; }
            get { return this.businessCenterScheme; }
        }

        string IBusinessCenter.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        string IBusinessCenter.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        EFS_Id IBusinessCenter.Efs_id { get { return this.efs_id; } }
        #endregion IBusinessCenter Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessCenter clone = new BusinessCenter
            {
                businessCenterScheme = this.businessCenterScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion BusinessCenter
    #region BusinessCenters
    public partial class BusinessCenters : ICloneable, IBusinessCenters
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
        #region Constructors
        public BusinessCenters() { }
        public BusinessCenters(BusinessCenter[] pBc)
        {
            if (ArrFunc.IsFilled(pBc))
            {
                for (int i = 0; i < pBc.Length; i++)
                {
                    Add(pBc[i]);
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region Add
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public void Add(BusinessCenter pBc)
        {
            ArrayList aList;
            if (null != businessCenter)
                aList = new ArrayList(businessCenter);
            else
                aList = new ArrayList();
            aList.Add((BusinessCenter)pBc);
            businessCenter = (BusinessCenter[])aList.ToArray(typeof(BusinessCenter));
        }
        #endregion Add
        #region Contains
        public bool Contains(string pValue)
        {
            bool ret = false;
            //
            if (ArrFunc.IsFilled(businessCenter))
            {
                for (int i = 0; i < businessCenter.Length; i++)
                {
                    if (businessCenter[i].Value == pValue)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion Contains
        #endregion Methods

        #region IBusinessCenters Members
        IBusinessCenter[] IBusinessCenters.BusinessCenter
        {
            set { this.businessCenter = (BusinessCenter[])value; }
            get { return this.businessCenter; }
        }
        object IBusinessCenters.Clone() { return this.Clone(); }
        IBusinessDayAdjustments IBusinessCenters.GetBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments
            {
                businessDayConvention = pBusinessDayConvention,
                businessCentersDefineSpecified = true,
                businessCentersDefine = this
            };
            return bda;
        }
        IInterval IBusinessCenters.GetInterval(string pPeriod, int pPeriodMultiplier)
        {
            return (IInterval)new Interval(pPeriod, pPeriodMultiplier);
        }
        /// <summary>
        /// Retourne les business Centers associées aux acteurs, devises, marchés
        ///  <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC">Devise au format ISO4217_ALPHA3</param>
        /// <param name="pIdM"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters IBusinessCenters.LoadBusinessCenters(string pConnectionString, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM)
        {
            DataRowCollection rows = Tools.LoadBusinessCenter(pConnectionString, pDbTransaction, pIdA, pIdC, pIdM);
            if ((null != rows) && 0 < rows.Count)
            {
                BusinessCenter[] businessCenter = new BusinessCenter[rows.Count];
                int i = 0;
                foreach (DataRow row in rows)
                {
                    if (0 < row[0].ToString().Length)
                    {
                        businessCenter[i] = new BusinessCenter
                        {
                            Value = row[0].ToString()
                        };
                        i++;
                    }
                }
                return new BusinessCenters(businessCenter);
            }
            return null;
        }
        string IBusinessCenters.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        EFS_Id IBusinessCenters.Efs_id { get { return this.efs_id; } }
        #endregion IBusinessCenters Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessCenters clone = new BusinessCenters();
            if (this.businessCenter != null)
                clone.businessCenter = (BusinessCenter[])this.businessCenter.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion BusinessCenters
    #region BusinessCentersReference
    public partial class BusinessCentersReference : ICloneable, IReference
    {
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessCentersReference clone = new BusinessCentersReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion BusinessCentersReference
    #region BusinessCenterTime
    public partial class BusinessCenterTime : ICloneable, IBusinessCenterTime
    {
        #region Constructors
        public BusinessCenterTime()
        {
            hourMinuteTime = new HourMinuteTime();
            businessCenter = new BusinessCenter();
        }
        #endregion Constructors
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessCenterTime clone = new BusinessCenterTime
            {
                hourMinuteTime = new HourMinuteTime(this.hourMinuteTime.Value),
                businessCenter = (BusinessCenter)this.businessCenter.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IBusinessCenterTime Members
        IHourMinuteTime IBusinessCenterTime.HourMinuteTime
        {
            get { return this.hourMinuteTime; }
        }
        IBusinessCenter IBusinessCenterTime.BusinessCenter
        {
            get { return this.businessCenter; }
        }
        #endregion IBusinessCenterTime Members
    }
    #endregion BusinessCenterTime
    #region BusinessDateRange
    // EG 20190115 [24361] Add isSettlementOfHolidayDeliveryConvention (Migration financial settlement for BoM Products)
    public partial class BusinessDateRange : IEFS_Array, ICloneable, IBusinessDateRange
    {
        #region Constructors
        public BusinessDateRange()
        {
            businessDayConvention = new BusinessDayConventionEnum();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
            businessCentersNone = new Empty();
        }
        #endregion Constructors
        #region Methods
        #region GetAdjustments
        public BusinessDayAdjustments GetAdjustments
        {
            get
            {
                BusinessDayAdjustments businessDayAdjustments = new BusinessDayAdjustments
                {
                    businessCentersDefine = this.businessCentersDefine,
                    businessCentersDefineSpecified = this.businessCentersDefineSpecified,
                    businessCentersReference = this.businessCentersReference,
                    businessCentersReferenceSpecified = this.businessCentersReferenceSpecified,
                    businessCentersNone = this.businessCentersNone,
                    businessCentersNoneSpecified = this.businessCentersNoneSpecified,
                    businessDayConvention = this.businessDayConvention
                };
                return businessDayAdjustments;
            }
        }
        #endregion GetAdjustments
        #region ResetIdSibling
        public void ResetIdSibling()
        {
            if (businessCentersNoneSpecified)
                businessCentersDefine.efs_id.Value = null;
        }
        #endregion ResetIdSibling
        #endregion Methods

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
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessDateRange clone = new BusinessDateRange
            {
                unadjustedFirstDate = new EFS_Date(this.unadjustedFirstDate.Value),
                unadjustedLastDate = new EFS_Date(this.unadjustedLastDate.Value),
                businessDayConvention = this.businessDayConvention,
                businessCentersDefineSpecified = this.businessCentersDefineSpecified
            };
            if (null != this.businessCentersDefine)
                clone.businessCentersDefine = (BusinessCenters)this.businessCentersDefine.Clone();

            clone.businessCentersNoneSpecified = this.businessCentersNoneSpecified;
            if (null != this.businessCentersNone)
                clone.businessCentersNone = (Empty)this.businessCentersNone.Clone();

            clone.businessCentersReferenceSpecified = this.businessCentersReferenceSpecified;
            if (null != this.businessCentersReference)
                clone.businessCentersReference = (BusinessCentersReference)this.businessCentersReference.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IBusinessDateRange Members
        IBusinessDayAdjustments IBusinessDateRange.GetAdjustments { get { return this.GetAdjustments; } }
        // EG 20190115 [24361] Add isSettlementOfHolidayDeliveryConvention
        //bool IBusinessDayAdjustments.isSettlementOfHolidayDeliveryConvention
        //{
        //    set { }
        //    get { return false; }
        //}
        EFS_Date IBusinessDateRange.UnadjustedFirstDate
        {
            set { this.unadjustedFirstDate = value; }
            get { return this.unadjustedFirstDate; }
        }
        EFS_Date IBusinessDateRange.UnadjustedLastDate
        {
            set { this.unadjustedLastDate = value; }
            get { return this.unadjustedLastDate; }
        }
        #endregion IBusinessDateRange Members
        #region IBusinessDayAdjustments Membres
        // EG 20190115 [24361]
        bool IBusinessDayAdjustments.IsSettlementOfHolidayDeliveryConvention
        {
            set { }
            get { return false; }
        }
        BusinessDayConventionEnum IBusinessDayAdjustments.BusinessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        bool IBusinessDayAdjustments.BusinessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IBusinessDayAdjustments.BusinessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IBusinessDayAdjustments.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IBusinessDayAdjustments.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IBusinessDayAdjustments.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IBusinessDayAdjustments.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IBusinessDayAdjustments.BusinessCentersReferenceValue
        {
            get
            {
                if (this.businessCentersReferenceSpecified)
                    return this.businessCentersReference.href;
                return string.Empty;
            }
        }
        object IBusinessDayAdjustments.Clone() { return this.Clone(); }
        IOffset IBusinessDayAdjustments.DefaultOffsetPreSettlement { get { return new Offset(PeriodEnum.D, -2, DayTypeEnum.Business); } }
        IAdjustableDate IBusinessDayAdjustments.CreateAdjustableDate(DateTime pUnadjustedDate)
        {
            AdjustableDate adjustableDate = new AdjustableDate();
            adjustableDate.unadjustedDate.DateValue = pUnadjustedDate;
            adjustableDate.dateAdjustments = (BusinessDayAdjustments)this.Clone();
            return adjustableDate;
        }
        IOffset IBusinessDayAdjustments.CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType)
        {
            return new Offset(pPeriod, pMultiplier, pDayType);
        }
        #endregion IBusinessDayAdjustments Members
    }
    #endregion BusinessDateRange
    #region BusinessDayAdjustments
    // EG 20190115 [24361] Add isSettlementOfHolidayDeliveryConvention (Migration financial settlement for BoM Products)
    public partial class BusinessDayAdjustments : ICloneable, IBusinessDayAdjustments
    {
        // EG 20190115 [24361]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private bool isSettlementOfHolidayDeliveryConvention;

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
        public BusinessDayAdjustments()
        {
            businessDayConvention = new BusinessDayConventionEnum();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
            businessCentersNone = new Empty();
        }
        public BusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, IBusinessCenters pBusinessCenters)
        {
            businessDayConvention = pBusinessDayConvention;
            businessCentersNone = new Empty();
            businessCentersNoneSpecified = true;
            if ((null != pBusinessCenters) && (ArrFunc.IsFilled(pBusinessCenters.BusinessCenter)))
            {
                businessCentersDefineSpecified = true;
                businessCentersDefine = (BusinessCenters)pBusinessCenters.Clone();
            }
        }
        #endregion Constructors
        #region Methods
        #region ProcessReference_IdSibling
        public void ProcessReference_IdSibling(FieldInfo pFldPrevious, FieldInfo pFldNext, FullConstructor pFullCtor)
        {
            string oldValue = null;
            string newValue = null;
            if (pFldPrevious.FieldType.Equals(typeof(BusinessCenters)))
                oldValue = businessCentersDefine.efs_id.Value;
            if (pFldNext.FieldType.Equals(typeof(BusinessCenters)))
                newValue = businessCentersDefine.efs_id.Value;
            pFullCtor.LoadEnumObjectReference("BusinessCentersReference", oldValue, newValue);
        }
        #endregion ProcessReference_IdSibling
        #endregion Methods

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessDayAdjustments clone = new BusinessDayAdjustments
            {
                businessDayConvention = this.businessDayConvention,
                businessCentersDefineSpecified = this.businessCentersDefineSpecified
            };
            if (null != this.businessCentersDefine)
                clone.businessCentersDefine = (BusinessCenters)this.businessCentersDefine.Clone();
            //
            clone.businessCentersNoneSpecified = this.businessCentersNoneSpecified;
            if (null != this.businessCentersNone)
                clone.businessCentersNone = (Empty)this.businessCentersNone.Clone();
            //
            clone.businessCentersReferenceSpecified = this.businessCentersReferenceSpecified;
            if (null != this.businessCentersReference)
                clone.businessCentersReference = (BusinessCentersReference)this.businessCentersReference.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IBusinessDayAdjustments Members
        // EG 20190115 [24361] Add isSettlementOfHolidayDeliveryConvention
        bool IBusinessDayAdjustments.IsSettlementOfHolidayDeliveryConvention
        {
            set { this.isSettlementOfHolidayDeliveryConvention = value; }
            get { return this.isSettlementOfHolidayDeliveryConvention; }
        }
        BusinessDayConventionEnum IBusinessDayAdjustments.BusinessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        bool IBusinessDayAdjustments.BusinessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IBusinessDayAdjustments.BusinessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IBusinessDayAdjustments.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IBusinessDayAdjustments.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IBusinessDayAdjustments.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IBusinessDayAdjustments.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IBusinessDayAdjustments.BusinessCentersReferenceValue
        {
            get
            {
                if (this.businessCentersReferenceSpecified)
                    return this.businessCentersReference.href;
                return string.Empty;
            }
        }
        object IBusinessDayAdjustments.Clone() { return this.Clone(); }
        IOffset IBusinessDayAdjustments.DefaultOffsetPreSettlement { get { return new Offset(PeriodEnum.D, -2, DayTypeEnum.Business); } }
        IAdjustableDate IBusinessDayAdjustments.CreateAdjustableDate(DateTime pUnadjustedDate)
        {
            AdjustableDate adjustableDate = new AdjustableDate();
            adjustableDate.unadjustedDate.DateValue = pUnadjustedDate;
            adjustableDate.dateAdjustments = (BusinessDayAdjustments)this.Clone();
            return adjustableDate;
        }
        IOffset IBusinessDayAdjustments.CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType)
        {
            return new Offset(pPeriod, pMultiplier, pDayType);
        }
        #endregion IBusinessDayAdjustments Members
    }
    #endregion BusinessDayAdjustments

    #region CalculationAgent
    public partial class CalculationAgent : ICalculationAgent
    {
        #region Constructors
        public CalculationAgent()
        {
            calculationAgentPartyReference = new ArrayPartyReference[1] { new ArrayPartyReference() };
        }
        #endregion Constructors

        #region ICalculationAgent Members
        bool ICalculationAgent.PartyReferenceSpecified
        {
            set { this.calculationAgentPartyReferenceSpecified = value; }
            get { return this.calculationAgentPartyReferenceSpecified; }
        }
        IReference[] ICalculationAgent.PartyReference
        {
            set { this.calculationAgentPartyReference = (ArrayPartyReference[])value; }
            get { return this.calculationAgentPartyReference; }
        }
        bool ICalculationAgent.PartySpecified
        {
            set { this.calculationAgentPartySpecified = value; }
            get { return this.calculationAgentPartySpecified; }
        }
        CalculationAgentPartyEnum ICalculationAgent.Party
        {
            set { this.calculationAgentParty = value; }
            get { return this.calculationAgentParty; }
        }
        #endregion ICalculationAgent Members
    }
    #endregion CalculationAgent
    #region CalculationPeriodFrequency
    public partial class CalculationPeriodFrequency : ICalculationPeriodFrequency
    {
        #region Constructors
        public CalculationPeriodFrequency()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
            rollConvention = new RollConventionEnum();
        }
        public CalculationPeriodFrequency(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention)
        {
            periodMultiplier = new EFS_Integer(pPeriodMultiplier);
            period = pPeriod;
            rollConvention = pRollConvention;
        }
        #endregion Constructors

        #region ICalculationPeriodFrequency Members
        PeriodEnum ICalculationPeriodFrequency.Period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer ICalculationPeriodFrequency.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval ICalculationPeriodFrequency.Interval { get { return (IInterval)this; } }
        RollConventionEnum ICalculationPeriodFrequency.RollConvention
        {
            set { this.rollConvention = value; }
            get { return this.rollConvention; }
        }
        #endregion ICalculationPeriodFrequency Members
    }
    #endregion CalculationPeriodFrequency
    #region ClearanceSystem
    public partial class ClearanceSystem : IScheme
    {
        #region Constructors
        public ClearanceSystem() : this(string.Empty) { }
        public ClearanceSystem(string pValue)
        {
            Value = pValue;
            clearanceSystemScheme = "http://www.fpml.org/coding-scheme/clearance-system-1-0";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.clearanceSystemScheme = value; }
            get { return this.clearanceSystemScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ClearanceSystem
    #region Country
    public partial class Country : ICloneable, IScheme
    {
        #region Constructors
        public Country()
        {
            countryScheme = "http://www.fpml.org/ext/iso3166";
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Country clone = new Country
            {
                countryScheme = this.countryScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.countryScheme; }
            set { this.countryScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members
    }
    #endregion Country
    #region CreditSeniority
    public partial class CreditSeniority : IScheme
    {
        #region Constructors
        public CreditSeniority()
        {
            creditSeniorityScheme = "http://www.fpml.org/coding-scheme/credit-seniority-1-0";
        }
        #endregion Constructors
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.creditSeniorityScheme = value; }
            get { return this.creditSeniorityScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CreditSeniority

    #region Currency
    public partial class Currency : IEFS_Array, ICloneable, ICurrency
    {
        #region Constructors
        public Currency() : this(string.Empty) { }
        public Currency(string pCur)
        {
            currencyScheme = "http://www.fpml.org/ext/iso4217-2001-08-15";
            Value = pCur;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Currency clone = new Currency
            {
                currencyScheme = this.currencyScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
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
        #region ICurrency Members
        string ICurrency.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        string ICurrency.CurrencyScheme
        {
            set { this.currencyScheme = value; }
            get { return this.currencyScheme; }
        }

        #endregion ICurrency Members
    }
    #endregion Currency
    #region CorrespondentInformation
    public partial class CorrespondentInformation : ICloneable, IRouting
    {
        #region Constructors
        public CorrespondentInformation()
        {
            routingIds = new RoutingIds();
            routingExplicitDetails = new RoutingExplicitDetails();
            routingIdsAndExplicitDetails = new RoutingIdsAndExplicitDetails();
        }
        #endregion Constructors
        #region Methods

        #region GetRoutingAccountNumber
        public string GetRoutingAccountNumber()
        {
            return SettlementTools.GetRoutingAccountNumber((IRouting)this);
        }
        #endregion GetRoutingAccountNumber
        #endregion Methods

        #region ICloneable Members
        public object Clone()
        {
            CorrespondentInformation clone = (CorrespondentInformation)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable Members
        #region IRouting Membres
        bool IRouting.RoutingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.RoutingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.RoutingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.RoutingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.RoutingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.RoutingIdsAndExplicitDetails
        {
            get { return this.routingIdsAndExplicitDetails; }
            set { this.routingIdsAndExplicitDetails = (RoutingIdsAndExplicitDetails)value; }
        }
        IRouting IRouting.Clone()
        {
            return (IRouting)this.Clone();
        }
        #endregion IRouting Membres
    }
    #endregion CorrespondentInformation

    #region DateList
    public partial class DateList : IDateList
    {
        #region Methods
        #region DisplayArray
        public static object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods

        #region IDateList Members
        object[] IDateList.Date { get { return this.date; } }
        DateTime IDateList.this[int pIndex] { get { return this.date[pIndex].DateValue; } }
        #endregion IDateList Members
    }
    #endregion DateList
    #region DateOffset
    // EG 20140702 Upd Interface
    // EG 20221201 [25639] [WI482] Add constructeur
    public partial class DateOffset : IEFS_Array, IDateOffset
    {
        public DateOffset()
        {
            periodMultiplier = new EFS_Integer();
        }
        #region IDateOffset Members
        PeriodEnum IDateOffset.Period 
        { 
            set { this.period = value; }
            get { return this.period; } 
        }
        EFS_Integer IDateOffset.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        bool IDateOffset.DayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IDateOffset.DayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        BusinessDayConventionEnum IDateOffset.BusinessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        IOffset IDateOffset.Offset
        {
            get
            {
                Offset offset = new Offset(this.period, this.periodMultiplier.IntValue, this.dayType);
                return (IOffset)offset;
            }
        }
        IBusinessDayAdjustments IDateOffset.BusinessDayAdjustments(IBusinessCenters pBusinessCenters)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments(this.businessDayConvention, pBusinessCenters);
            return (IBusinessDayAdjustments)bda;
        }
        #endregion IDateOffset Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion DateOffset
    #region DateReference
    public partial class DateReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion DateReference
    #region DateTimeList
    public partial class DateTimeList : IEFS_Array, IDateTimeList
    {
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
        #region IDateTimeList Members
        EFS_DateTimeArray[] IDateTimeList.DateTime
        {
            set { this.dateTime = (EFS_DateTimeArray[])value; }
            get { return this.dateTime; }
        }
        #endregion IDateTimeList Members
    }
    #endregion DateTimeList

    #region Documentation
    public partial class Documentation : IDocumentation
    {
        #region IDocumentation Membres
        bool IDocumentation.MasterAgreementSpecified
        {
            get { return this.masterAgreementSpecified; }
            set { masterAgreementSpecified = value; }
        }
        IMasterAgreement IDocumentation.MasterAgreement
        {
            get { return this.masterAgreement; }
            set { this.masterAgreement = (MasterAgreement)value; }
        }
        IMasterAgreement IDocumentation.CreateMasterAgreement()
        {
            return new MasterAgreement();
        }
        #endregion
    }
    #endregion
    #region DeterminationMethod
    public partial class DeterminationMethod : IScheme
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.determinationMethodScheme = value; }
            get { return this.determinationMethodScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion DeterminationMethod

    #region DividendConditions
    // EG 20140702 Upd Interface
    public partial class DividendConditions : IDividendConditions
    {
        #region Constructors
        public DividendConditions()
        {
            dividendPeriodEffectiveDate = new DateReference();
            dividendPeriodEndDate = new DateReference();

            currencyCurrency = new Currency();
            currencyDeterminationMethod = new DeterminationMethod();
            currencyCurrencyReference = new IdentifiedCurrencyReference();
        }
        #endregion Constructors

        #region IDividendConditions Members
        bool IDividendConditions.DividendEntitlementSpecified
        {
            set { this.dividendEntitlementSpecified = value; }
            get { return this.dividendEntitlementSpecified; }
        }
        DividendEntitlementEnum IDividendConditions.DividendEntitlement
        {
            set { this.dividendEntitlement = value; }
            get { return this.dividendEntitlement; }
        }
        bool IDividendConditions.DividendAmountSpecified
        {
            set { this.dividendAmountSpecified = value; }
            get { return this.dividendAmountSpecified; }
        }
        bool IDividendConditions.DividendPeriodSpecified
        {
            set { this.dividendPeriodPeriodSpecified = value; }
            get { return this.dividendPeriodPeriodSpecified; }
        }
        DividendPeriodEnum IDividendConditions.DividendPeriod
        {
            set { this.dividendPeriodPeriod = value; }
            get { return this.dividendPeriodPeriod; }
        }
        DividendAmountTypeEnum IDividendConditions.DividendAmount
        {
            set { this.dividendAmount = value; }
            get { return this.dividendAmount; }
        }
        bool IDividendConditions.DividendPaymentDateSpecified
        {
            set { this.dividendPaymentDateSpecified = value; }
            get { return this.dividendPaymentDateSpecified; }
        }
        IDividendPaymentDate IDividendConditions.DividendPaymentDate
        {
            set { this.dividendPaymentDate = (DividendPaymentDate)value; }
            get { return this.dividendPaymentDate; }
        }
        #endregion IDividendConditions Members
    }
    #endregion DividendConditions
    #region DividendPaymentDate
    public partial class DividendPaymentDate : IDividendPaymentDate
    {
        #region Constructors
        public DividendPaymentDate()
        {
            paymentDateAdjustableDate = new AdjustableDate();
            paymentDateOffset = new Offset();
        }
        #endregion Constructors

        #region IDividendPaymentDate Members
        bool IDividendPaymentDate.DividendDateReferenceSpecified
        {
            set { this.paymentDateDividendDateReferenceSpecified = value; }
            get { return this.paymentDateDividendDateReferenceSpecified; }
        }
        DividendDateReferenceEnum IDividendPaymentDate.DividendDateReference
        {
            set { this.paymentDateDividendDateReference = value; }
            get { return this.paymentDateDividendDateReference; }
        }
        bool IDividendPaymentDate.AdjustableDateSpecified
        {
            get { return this.paymentDateAdjustableDateSpecified; }
        }
        IAdjustableDate IDividendPaymentDate.AdjustableDate
        {
            get { return this.paymentDateAdjustableDate; }
        }
        bool IDividendPaymentDate.OffsetSpecified
        {
            get { return this.paymentDateOffsetSpecified; }
        }
        IOffset IDividendPaymentDate.Offset
        {
            get { return this.paymentDateOffset; }
        }
        #endregion IDividendPaymentDate Members
    }
    #endregion DividendPaymentDate

    #region Empty
    public partial class Empty : ICloneable, IEmpty
    {
        #region ICloneable members
        #region Clone
        public object Clone()
        {
            Empty clone = new Empty();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable members
        #region IEmpty Members
        object IEmpty.Empty
        {
            get { return this; }
        }
        #endregion IEmpty Members
    }
    #endregion Empty

    #region EuropeanExercise
    public partial class EuropeanExercise : IEuropeanExercise
    {
        #region Constructors
        public EuropeanExercise()
        {
            relevantUnderlyingDate = new AdjustableOrRelativeDates();
        }
        #endregion Constructors

        #region IEuropeanExercise Members
        IAdjustableOrRelativeDate IEuropeanExercise.ExpirationDate { get { return this.expirationDate; } }
        bool IEuropeanExercise.RelevantUnderlyingDateSpecified { get { return this.relevantUnderlyingDateSpecified; } }
        IAdjustableOrRelativeDates IEuropeanExercise.RelevantUnderlyingDate { get { return this.relevantUnderlyingDate; } }
        bool IEuropeanExercise.ExerciseFeeSpecified
        {
            get { return this.exerciseFeeSpecified; }
        }
        IExerciseFee IEuropeanExercise.ExerciseFee
        {
            get { return this.exerciseFee; }
        }
        bool IEuropeanExercise.PartialExerciseSpecified
        {
            set { this.partialExerciseSpecified = value; }
            get { return this.partialExerciseSpecified; }
        }
        IPartialExercise IEuropeanExercise.PartialExercise
        {
            get { return this.partialExercise; }
        }
        IPartialExercise IEuropeanExercise.CreatePartialExercise()
        {
            PartialExercise partial = new PartialExercise
            {
                integralMultipleAmountSpecified = false,
                minimumNotionalAmountSpecified = true,
                minimumNumberOfOptionsSpecified = false
            };
            return partial;
        }
        #endregion IEuropeanExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.EarliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }

        IBusinessCenterTime IExerciseBase.ExpirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(),
                    hourMinuteTime = new HourMinuteTime()
                };
                return bct;
            }
        }
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            int nbNotionalReference = pNotionalReference.Length;
            ScheduleReference[] scheduleReference = null;
            if (0 < nbNotionalReference)
            {
                scheduleReference = new ScheduleReference[nbNotionalReference];
                for (int i = 0; i < nbNotionalReference; i++)
                {
                    scheduleReference[i] = new ScheduleReference();
                    ((IReference)scheduleReference[i]).HRef = pNotionalReference[i];
                }
            }
            return scheduleReference;
        }
        #endregion IExerciseBase Members
    }
    #endregion EuropeanExercise
    #region ExchangeId
    public partial class ExchangeId : IEFS_Array, ISpheresIdScheme
    {
        #region accessor
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion
        #region Constructors
        public ExchangeId() : this(string.Empty) { }
        public ExchangeId(string pValue)
        {
            Value = pValue;
            exchangeIdScheme = "http://www.fpml.org/spec/2002/exchange-id-MIC-1-0";
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
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.exchangeIdScheme = value; }
            get { return this.exchangeIdScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
        #region ISpheresId Membres
        int ISpheresId.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        string ISpheresId.OtcmlId
        {
            get { return otcmlId; }
            set { otcmlId = value; }
        }
        #endregion
    }
    #endregion ExchangeId
    #region Exercise
    public partial class Exercise : IExercise
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExerciseDates efs_ExerciseDates;
        #endregion Members

        #region IExerciseId
        string IExerciseId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion

        #region IExercise Members
        EFS_ExerciseDates IExercise.Efs_ExerciseDates
        {
            get { return this.efs_ExerciseDates; }
            set { this.efs_ExerciseDates = value; }
        }
        #endregion IExercise Members
    }
    #endregion Exercise
    #region ExerciseFee
    public partial class ExerciseFee : IExerciseFee
    {
        #region Constructors
        public ExerciseFee()
        {
            typeFeeAmount = new EFS_Decimal();
            typeFeeRate = new EFS_Decimal();
        }
        #endregion Constructors

        #region IExerciseFee Members
        bool IExerciseFee.FeeAmountSpecified
        {
            set { this.typeFeeAmountSpecified = value; }
            get { return this.typeFeeAmountSpecified; }
        }
        EFS_Decimal IExerciseFee.FeeAmount
        {
            set { this.typeFeeAmount = value; }
            get { return this.typeFeeAmount; }
        }
        bool IExerciseFee.FeeRateSpecified
        {
            set { this.typeFeeRateSpecified = value; }
            get { return this.typeFeeRateSpecified; }
        }
        EFS_Decimal IExerciseFee.FeeRate
        {
            set { this.typeFeeRate = value; }
            get { return this.typeFeeRate; }
        }
        #endregion IExerciseFee Members
        #region IExerciseFeeBase Members
        IReference IExerciseFeeBase.PayerPartyReference
        {
            get { return this.payerPartyReference; }
        }
        IReference IExerciseFeeBase.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
        }
        IReference IExerciseFeeBase.NotionalReference
        {
            get { return this.notionalReference; }
        }
        IRelativeDateOffset IExerciseFeeBase.FeePaymentDate
        {
            set { this.feePaymentDate = (RelativeDateOffset)value; }
            get { return this.feePaymentDate; }
        }
        #endregion IExerciseFeeBase Members
    }
    #endregion ExerciseFee
    #region ExerciseFeeSchedule
    public partial class ExerciseFeeSchedule : IExerciseFeeSchedule
    {
        #region Constructors
        public ExerciseFeeSchedule()
        {
            typeFeeAmount = new AmountSchedule();
            typeFeeRate = new Schedule();
        }
        #endregion Constructors

        #region IExerciseFeeSchedule Members
        bool IExerciseFeeSchedule.FeeAmountSpecified
        {
            get { return this.typeFeeAmountSpecified; }
        }
        IAmountSchedule IExerciseFeeSchedule.FeeAmount
        {
            get { return this.typeFeeAmount; }
        }
        bool IExerciseFeeSchedule.FeeRateSpecified
        {
            get { return this.typeFeeRateSpecified; }
        }
        ISchedule IExerciseFeeSchedule.FeeRate
        {
            get { return this.typeFeeRate; }
        }
        #endregion IExerciseFeeSchedule Members
        #region IExerciseFeeBase Members
        IReference IExerciseFeeBase.PayerPartyReference
        {
            get { return this.payerPartyReference; }
        }
        IReference IExerciseFeeBase.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
        }
        IReference IExerciseFeeBase.NotionalReference
        {
            get { return this.notionalReference; }
        }
        IRelativeDateOffset IExerciseFeeBase.FeePaymentDate
        {
            set { this.feePaymentDate = (RelativeDateOffset)value; }
            get { return this.feePaymentDate; }
        }
        #endregion IExerciseFeeBase Members
    }
    #endregion ExerciseFeeSchedule
    #region ExerciseNotice
    public partial class ExerciseNotice : IEFS_Array, IExerciseNotice
    {
        #region Constructors
        public ExerciseNotice()
        {
            partyReference = new PartyReference();
            exerciseNoticePartyReference = new PartyReference();
            businessCenter = new BusinessCenter();
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
        #region IExerciseNotice Members
        IReference IExerciseNotice.PartyReference
        {
            set { this.partyReference = (PartyReference)value; }
            get { return this.partyReference; }
        }
        bool IExerciseNotice.ExerciseNoticePartyReferenceSpecified
        {
            set { this.exerciseNoticePartyReferenceSpecified = value; }
            get { return this.exerciseNoticePartyReferenceSpecified; }
        }
        IReference IExerciseNotice.ExerciseNoticePartyReference
        {
            set { this.exerciseNoticePartyReference = (PartyReference)value; }
            get { return this.exerciseNoticePartyReference; }
        }
        IBusinessCenter IExerciseNotice.BusinessCenter
        {
            set { this.businessCenter = (BusinessCenter)value; }
            get { return this.businessCenter; }
        }
        #endregion IExerciseNotice Members
    }
    #endregion ExerciseNotice

    #region ExerciseProcedure
    // EG 20150422 [20513] BANCAPERTA followUpConfirmationSpecified (true)
    public partial class ExerciseProcedure : IExerciseProcedure
    {
        #region Constructors
        public ExerciseProcedure()
        {
            exerciseProcedureAutomatic = new AutomaticExercise();
            exerciseProcedureManual = new ManualExercise();
            followUpConfirmationSpecified = true;
            followUpConfirmation = new EFS_Boolean(false);
        }
        #endregion Constructors

        #region IExerciseProcedure Members
        bool IExerciseProcedure.ExerciseProcedureAutomaticSpecified { get { return this.exerciseProcedureAutomaticSpecified; } }
        IAutomaticExercise IExerciseProcedure.ExerciseProcedureAutomatic { get { return this.exerciseProcedureAutomatic; } }
        bool IExerciseProcedure.ExerciseProcedureManualSpecified { get { return this.exerciseProcedureManualSpecified; } }
        IManualExercise IExerciseProcedure.ExerciseProcedureManual { get { return this.exerciseProcedureManual; } }
        bool IExerciseProcedure.FollowUpConfirmationSpecified { get { return true; } }
        bool IExerciseProcedure.FollowUpConfirmation { get { return this.followUpConfirmation.BoolValue; } }
        #endregion IExerciseProcedure Members
    }
    #endregion ExerciseProcedure

    #region FloatingRate
    public partial class FloatingRate : IFloatingRate
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
        #region Constructors
        public FloatingRate()
        {
            floatingRateIndex = new FloatingRateIndex();
            floatingRateMultiplierSchedule = new Schedule();
            spreadSchedule = new SpreadSchedule[1] { new SpreadSchedule() };
            indexTenor = new Interval();
        }
        #endregion Constructors
        #region Methods
        #region GetSqlAssetRateIndex
        public SQL_AssetRateIndex GetSqlAssetRateIndex(string pCs)
        {
            SQL_AssetRateIndex ret = null;
            if (null != floatingRateIndex)
            {
                int idFloatingRateIndex = floatingRateIndex.OTCmlId;
                if (idFloatingRateIndex > 0)
                {
                    ret = new SQL_AssetRateIndex(pCs, SQL_AssetRateIndex.IDType.IDASSET, idFloatingRateIndex);
                    ret.LoadTable();
                }
            }
            return ret;
        }
        #endregion GetSqlAssetRateIndex
        #endregion Methods

        #region IFloatingRate Members
        bool IFloatingRate.CapRateScheduleSpecified
        {
            set { this.capRateScheduleSpecified = value; }
            get { return this.capRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.CapRateSchedule
        {
            set { this.capRateSchedule = (StrikeSchedule[])value; }
            get { return this.capRateSchedule; }
        }
        bool IFloatingRate.FloorRateScheduleSpecified
        {
            set { this.floorRateScheduleSpecified = value; }
            get { return this.floorRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.FloorRateSchedule
        {
            set { this.floorRateSchedule = (StrikeSchedule[])value; }
            get { return this.floorRateSchedule; }
        }
        IFloatingRateIndex IFloatingRate.FloatingRateIndex { get { return this.floatingRateIndex; } }
        bool IFloatingRate.SpreadScheduleSpecified
        {
            set { this.spreadScheduleSpecified = value; }
            get { return this.spreadScheduleSpecified; }
        }
        ISchedule IFloatingRate.SpreadSchedule
        {
            set
            {
                if ((null != this.spreadSchedule) && (0 < this.spreadSchedule.Length))
                    this.spreadSchedule[0] = (SpreadSchedule)value;
            }
            get
            {
                if (this.spreadScheduleSpecified && (0 < this.spreadSchedule.Length))
                    return this.spreadSchedule[0];
                else
                    return null;
            }
        }
        // EG 20150309 POC - BERKELEY New
        ISpreadSchedule[] IFloatingRate.LstSpreadSchedule
        {
            set {this.spreadSchedule = (SpreadSchedule[])value;}
            get {return this.spreadSchedule;}
        }
        bool IFloatingRate.FloatingRateMultiplierScheduleSpecified
        {
            set { this.floatingRateMultiplierScheduleSpecified = value; }
            get { return this.floatingRateMultiplierScheduleSpecified; }
        }
        ISchedule IFloatingRate.FloatingRateMultiplierSchedule { get { return this.floatingRateMultiplierSchedule; } }
        bool IFloatingRate.RateTreatmentSpecified { get { return this.rateTreatmentSpecified; } }
        RateTreatmentEnum IFloatingRate.RateTreatment { get { return this.rateTreatment; } }
        ISchedule IFloatingRate.CreateSchedule()
        {
            this.spreadSchedule = new SpreadSchedule[1] { new SpreadSchedule() };
            return spreadSchedule[0];
        }

        ISpreadSchedule IFloatingRate.CreateSpreadSchedule()
        {
            this.spreadSchedule = new SpreadSchedule[1] { new SpreadSchedule() };
            return spreadSchedule[0];
        }
        IStrikeSchedule[] IFloatingRate.CreateStrikeSchedule(int pDim)
        {
            StrikeSchedule[] ret = new StrikeSchedule[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new StrikeSchedule();
            return ret;
        }
        bool IFloatingRate.IndexTenorSpecified
        {
            set { this.indexTenorSpecified = value; }
            get { return this.indexTenorSpecified; }
        }
        IInterval IFloatingRate.IndexTenor
        {
            set { this.indexTenor = (Interval)value; }
            get { return this.indexTenor; }
        }
        SQL_AssetRateIndex IFloatingRate.GetSqlAssetRateIndex(string pCs) { return this.GetSqlAssetRateIndex(pCs); }
        #endregion IFloatingRate Members

    }
    #endregion FloatingRate
    #region FloatingRateCalculation
    public partial class FloatingRateCalculation : IFloatingRateCalculation
    {
        #region Constructors
        public FloatingRateCalculation() { }
        #endregion Constructors

        #region IFloatingRateCalculation Members
        // EG 20161116 Gestion InitialRate (RATP)
        bool IFloatingRateCalculation.InitialRateSpecified
        {
            set { this.initialRateSpecified = value; }
            get { return this.initialRateSpecified; }
        }
        EFS_Decimal IFloatingRateCalculation.InitialRate 
        {
            set { this.initialRate = value; }
            get { return this.initialRate; } 
        }

        bool IFloatingRateCalculation.FinalRateRoundingSpecified { get { return this.finalRateRoundingSpecified; } }
        IRounding IFloatingRateCalculation.FinalRateRounding { get { return this.finalRateRounding; } }
        bool IFloatingRateCalculation.AveragingMethodSpecified { get { return this.averagingMethodSpecified; } }
        AveragingMethodEnum IFloatingRateCalculation.AveragingMethod { get { return this.averagingMethod; } }
        bool IFloatingRateCalculation.NegativeInterestRateTreatmentSpecified 
        {
            set { this.negativeInterestRateTreatmentSpecified = value; }
            get { return this.negativeInterestRateTreatmentSpecified; } 
        }
        NegativeInterestRateTreatmentEnum IFloatingRateCalculation.NegativeInterestRateTreatment 
        {
            set { this.negativeInterestRateTreatment = value; }
            get { return this.negativeInterestRateTreatment; } 
        }
        ISpreadSchedule[] IFloatingRateCalculation.CreateSpreadSchedules(ISpreadSchedule[] pSpreadSchedules)
        {
            SpreadSchedule[] spreadSchedules = new SpreadSchedule[pSpreadSchedules.Length];
            for (int i = 0; i < pSpreadSchedules.Length; i++)
            {
                spreadSchedules[i] = (SpreadSchedule)pSpreadSchedules[i];
            }
            return spreadSchedules;
        }
        #endregion IFloatingRateCalculation Members
        #region IFloatingRate Members
        bool IFloatingRate.IndexTenorSpecified
        {
            set { this.indexTenorSpecified = value; }
            get { return this.indexTenorSpecified; }
        }
        IInterval IFloatingRate.IndexTenor
        {
            set { this.indexTenor = (Interval)value; }
            get { return this.indexTenor; }
        }
        bool IFloatingRate.CapRateScheduleSpecified
        {
            set { this.capRateScheduleSpecified = value; }
            get { return this.capRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.CapRateSchedule
        {
            set { this.capRateSchedule = (StrikeSchedule[])value; }
            get { return this.capRateSchedule; }
        }
        bool IFloatingRate.FloorRateScheduleSpecified
        {
            set { this.floorRateScheduleSpecified = value; }
            get { return this.floorRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.FloorRateSchedule
        {
            set { this.floorRateSchedule = (StrikeSchedule[])value; }
            get { return this.floorRateSchedule; }
        }
        IFloatingRateIndex IFloatingRate.FloatingRateIndex { get { return this.floatingRateIndex; } }
        bool IFloatingRate.FloatingRateMultiplierScheduleSpecified
        {
            set { this.floatingRateMultiplierScheduleSpecified = value; }
            get { return this.floatingRateMultiplierScheduleSpecified; }
        }
        ISchedule IFloatingRate.FloatingRateMultiplierSchedule { get { return this.floatingRateMultiplierSchedule; } }
        bool IFloatingRate.SpreadScheduleSpecified
        {
            set { this.spreadScheduleSpecified = value; }
            get { return this.spreadScheduleSpecified; }
        }
        /// <summary>
        /// 
        /// </summary>
        ISchedule IFloatingRate.SpreadSchedule
        {
            set
            {
                if ((null != this.spreadSchedule) && (0 < ArrFunc.Count(this.spreadSchedule)))
                    this.spreadSchedule[0] = (SpreadSchedule)value;
            }
            get
            {
                if (this.spreadScheduleSpecified && (0 < ArrFunc.Count(this.spreadSchedule)))
                    return this.spreadSchedule[0];
                else
                    return null;
            }
        }
        ISpreadSchedule[] IFloatingRate.LstSpreadSchedule
        {
            set {this.spreadSchedule = (SpreadSchedule[])value;}
            get {return this.spreadSchedule;}
        }

        bool IFloatingRate.RateTreatmentSpecified { get { return this.rateTreatmentSpecified; } }
        RateTreatmentEnum IFloatingRate.RateTreatment { get { return this.rateTreatment; } }
        IStrikeSchedule[] IFloatingRate.CreateStrikeSchedule(int pDim)
        {
            StrikeSchedule[] ret = new StrikeSchedule[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new StrikeSchedule();
            return ret;
        }
        #endregion IFloatingRate Members
    }
    #endregion FloatingRateCalculation
    #region FloatingRateIndex
    public partial class FloatingRateIndex : IFloatingRateIndex
    {
        #region Constructors
        public FloatingRateIndex()
        {
            floatingRateIndexScheme = "http://www.fpml.org/coding-scheme/floating-rate-index-2-0";
        }
        #endregion Constructors

        #region IFloatingRateIndex Members
        int IFloatingRateIndex.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        bool IFloatingRateIndex.HrefSpecified
        {
            set { this.hrefSpecified = value; }
            get { return this.hrefSpecified; }
        }
        string IFloatingRateIndex.Href
        {
            get { return this.href; }
            set { this.href = value; }
        }
        string IFloatingRateIndex.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IFloatingRateIndex Members
    }
    #endregion FloatingRateIndex
    #region Formula
    public partial class Formula : IFormula
    {
        #region IFormula Members
        bool IFormula.FormulaDescriptionSpecified
        {
            set { this.formulaDescriptionSpecified = value; }
            get { return this.formulaDescriptionSpecified; }
        }
        EFS_MultiLineString IFormula.FormulaDescription
        {
            set { this.formulaDescription = value; }
            get { return this.formulaDescription; }
        }
        bool IFormula.MathSpecified
        {
            set { this.mathSpecified = value; }
            get { return this.mathSpecified; }
        }
        IMath IFormula.Math
        {
            set { this.math = (Math)value; }
            get { return this.math; }
        }
        bool IFormula.FormulaComponentSpecified
        {
            set { this.formulaComponentSpecified = value; }
            get { return this.formulaComponentSpecified; }
        }
        IFormulaComponent[] IFormula.FormulaComponent
        {
            set { this.formulaComponent = (FormulaComponent[])value; }
            get { return this.formulaComponent; }
        }
        #endregion IFormula Members
    }
    #endregion Formula
    #region FormulaComponent
    public partial class FormulaComponent : IEFS_Array, IFormulaComponent
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,
            ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

        #region IFormulaComponent Members
        EFS_MultiLineString IFormulaComponent.ComponentDescription
        {
            set { this.componentDescription = value; }
            get { return this.componentDescription; }
        }
        bool IFormulaComponent.FormulaSpecified
        {
            set { this.formulaSpecified = value; }
            get { return this.formulaSpecified; }
        }
        IFormula IFormulaComponent.Formula
        {
            set { this.formula = (Formula)value; }
            get { return this.formula; }
        }
        string IFormulaComponent.Name
        {
            set { this.Name = value; }
            get { return this.Name; }
        }
        string IFormulaComponent.Href
        {
            set { this.Href = value; }
            get { return this.Href; }
        }
        #endregion IFormulaComponent Members
    }
    #endregion FormulaComponent
    #region FxCashSettlement
    public partial class FxCashSettlement : IFxCashSettlement
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FxCashSettlement";
        #endregion Members
        #region Constructors
        public FxCashSettlement()
        {
            settlementCurrency = new Currency();
        }
        #endregion Constructors

        #region IFxCashSettlement Members
        ICurrency IFxCashSettlement.SettlementCurrency { get { return this.settlementCurrency; } }
        IFxFixing[] IFxCashSettlement.Fixing { get { return this.fixing; } }
        //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
        bool IFxCashSettlement.CustomerSettlementRateSpecified
        {
            set { this.customerSettlementRateSpecified = value; }
            get { return this.customerSettlementRateSpecified; }
        }
        bool IFxCashSettlement.CalculationAgentSettlementRateSpecified
        {
            set { this.calculationAgentSettlementRateSpecified = value; }
            get { return this.calculationAgentSettlementRateSpecified; }
        }
        void IFxCashSettlement.Initialize()
        {
            this.settlementCurrency = new Currency();
            this.fixing = new FxFixing[1] { new FxFixing() };
        }
        #endregion IFxCashSettlement Members
    }
    #endregion FxCashSettlement
    #region FxFixing
    public partial class FxFixing : IEFS_Array, ICloneable, IFxFixing
    {
        #region Accessors
        #region FixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date FixingDate
        {
            get
            {
                 return new EFS_Date(fixingDate.Value); 
            }
        }
        #endregion AdjustedPaymentDate
        #region FixingRate
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public FxFixing FixingRate
        {
            get{ return this; }
        }
        #endregion FixingRate
        #endregion Accessors
        #region Constructors
        public FxFixing()
        {
            primaryRateSource = new InformationSource();
            quotedCurrencyPair = new QuotedCurrencyPair();
            fixingTime = new BusinessCenterTime();
            fixingDate = new EFS_Date();
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            FxFixing clone = new FxFixing
            {
                primaryRateSource = (InformationSource)this.primaryRateSource.Clone(),
                secondaryRateSourceSpecified = this.secondaryRateSourceSpecified,
                fixingTime = (BusinessCenterTime)this.fixingTime.Clone(),
                quotedCurrencyPair = (QuotedCurrencyPair)this.quotedCurrencyPair.Clone(),
                fixingDate = new EFS_Date(this.fixingDate.Value)
            };
            if (clone.secondaryRateSourceSpecified)
                clone.secondaryRateSource = (InformationSource)this.secondaryRateSource.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IFxFixing Members
        IQuotedCurrencyPair IFxFixing.QuotedCurrencyPair
        {
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
            get { return this.quotedCurrencyPair; }
        }
        IInformationSource IFxFixing.PrimaryRateSource
        {
            set { this.primaryRateSource = (InformationSource)value; }
            get { return this.primaryRateSource; }
        }
        bool IFxFixing.SecondaryRateSourceSpecified
        {
            set { this.secondaryRateSourceSpecified = value; }
            get { return this.secondaryRateSourceSpecified; }
        }
        IInformationSource IFxFixing.SecondaryRateSource
        {
            set { this.secondaryRateSource = (InformationSource)value; }
            get { return this.secondaryRateSource; }
        }
        EFS_Date IFxFixing.FixingDate
        {
            set { this.fixingDate = value; }
            get { return this.fixingDate; }
        }
        IBusinessCenterTime IFxFixing.FixingTime
        {
            set { this.fixingTime = (BusinessCenterTime)value; }
            get { return this.fixingTime; }
        }
        IInformationSource IFxFixing.CreateInformationSource()
        {
            InformationSource informationSource = new InformationSource
            {
                rateSource = new InformationProvider()
            };
            return informationSource;
        }
        IBusinessCenterTime IFxFixing.CreateBusinessCenterTime()
        {
            BusinessCenterTime bct = new BusinessCenterTime
            {
                businessCenter = new BusinessCenter(),
                hourMinuteTime = new HourMinuteTime()
            };
            return bct;
        }
        IBusinessCenterTime IFxFixing.CreateBusinessCenterTime(IBusinessCenterTime pBusinessCenterTime)
        {
            BusinessCenterTime bct = new BusinessCenterTime
            {
                businessCenter = new BusinessCenter(pBusinessCenterTime.BusinessCenter.Value),
                hourMinuteTime = new HourMinuteTime(pBusinessCenterTime.HourMinuteTime.Value)
            };
            return bct;
        }
        IQuotedCurrencyPair IFxFixing.CreateQuotedCurrencyPair(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis)
        {
            return new QuotedCurrencyPair(pCurrency1, pCurrency2, pQuoteBasis);
        }
        IFxFixing IFxFixing.Clone() { return (IFxFixing)this.Clone(); }
        #endregion IFxFixing Members
    }
    #endregion FxFixing
    #region FxRate
    public partial class FxRate : IEFS_Array, IFxRate
    {
        #region Constructors
        public FxRate()
        {
            rate = new EFS_Decimal();
            quotedCurrencyPair = new QuotedCurrencyPair();
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
        #region IFxRate Members
        IQuotedCurrencyPair IFxRate.QuotedCurrencyPair
        {
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
            get { return this.quotedCurrencyPair; }
        }
        EFS_Decimal IFxRate.Rate
        {
            set { this.rate = value; }
            get { return this.rate; }
        }
        #endregion IFxRate Members
    }
    #endregion FxRate
    #region FxSpotRateSource
    public partial class FxSpotRateSource : IFxSpotRateSource
    {
        #region Constructors
        public FxSpotRateSource()
        {
            secondaryRateSource = new InformationSource();
        }
        #endregion Constructors

        #region IFxSpotRateSource Members
        IInformationSource IFxSpotRateSource.PrimaryRateSource
        {
            set { this.primaryRateSource = (InformationSource)value; }
            get { return this.primaryRateSource; }
        }
        bool IFxSpotRateSource.SecondaryRateSourceSpecified
        {
            set { this.secondaryRateSourceSpecified = value; }
            get { return this.secondaryRateSourceSpecified; }
        }
        IInformationSource IFxSpotRateSource.SecondaryRateSource
        {
            set { this.secondaryRateSource = (InformationSource)value; }
            get { return this.secondaryRateSource; }
        }
        IBusinessCenterTime IFxSpotRateSource.FixingTime
        {
            set { this.fixingTime = (BusinessCenterTime)value; }
            get { return this.fixingTime; }
        }
        IMoney IFxSpotRateSource.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        IFxFixing IFxSpotRateSource.CreateFxFixing(string pCurrency1, string pCurrency2, DateTime pFixingDate)
        {
            FxFixing fixing = new FxFixing();
            fixing.primaryRateSource.OTCmlId = 0;
            fixing.primaryRateSource = (InformationSource)this.primaryRateSource.Clone();
            fixing.quotedCurrencyPair = new QuotedCurrencyPair(pCurrency1, pCurrency2, QuoteBasisEnum.Currency1PerCurrency2);
            fixing.secondaryRateSourceSpecified = this.secondaryRateSourceSpecified;
            fixing.fixingTime = this.fixingTime;
            fixing.fixingDate = new EFS_Date
            {
                DateValue = pFixingDate
            };
            if (fixing.secondaryRateSourceSpecified)
                fixing.secondaryRateSource = (InformationSource)this.secondaryRateSource.Clone();
            return fixing;
        }
        IInformationSource IFxSpotRateSource.CreateInformationSource
        {
            get
            {
                InformationSource informationSource = new InformationSource
                {
                    rateSource = new InformationProvider()
                };
                return informationSource;
            }
        }
        #endregion IFxSpotRateSource Members
    }
    #endregion FxSpotRateSource

    #region GoverningLaw
    // EG 20170918 [23342] Add IEFS_Array, new constructor
    public partial class GoverningLaw : IScheme, IEFS_Array
    {
        #region Constructors
        public GoverningLaw() : this(string.Empty) { }
        public GoverningLaw(string pValue)
        {
            governingLawScheme = "http://www.fpml.org/spec/2002/governing-law-1-0";
            Value = pValue;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.governingLawScheme; }
            set { this.governingLawScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members

        #region _Value
        // EG 20170918 [23342] New
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members

    }
    #endregion GoverningLaw

    #region HourMinuteTime
    public partial class HourMinuteTime : IHourMinuteTime
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime TimeValue
        {
            get { return new DtFunc().StringToDateTime(this.Value, DtFunc.FmtISOTime); }
            set { this.Value = DtFunc.DateTimeToString(value, DtFunc.FmtISOTime); }
        }
        #endregion Accessors
        #region Constructors
        public HourMinuteTime() { }
        public HourMinuteTime(string pValue) { this.Value = pValue; }
        #endregion Constructors

        #region IHourMinuteTime Members
        string IHourMinuteTime.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        DateTime IHourMinuteTime.TimeValue
        {
            set { this.TimeValue = value; }
            get { return this.TimeValue; }
        }
        #endregion IHourMinuteTime Members
    }
    #endregion HourMinuteTime

    #region IdentifiedCurrency
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class IdentifiedCurrency : ISchemeId
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
        #region Constructors
        public IdentifiedCurrency()
        {
            currencyScheme = "http://www.fpml.org/ext/iso4217-2001-08-15";
        }
        #endregion Constructors

        #region ISchemeId Members
        string ISchemeId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.currencyScheme = value; }
            get { return this.currencyScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion IdentifiedCurrency
    #region IdentifiedCurrencyReference
    public partial class IdentifiedCurrencyReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion IdentifiedCurrencyReference

    #region IdentifiedDate
    public partial class IdentifiedDate : ICloneable, IAdjustedDate, IEFS_Array
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.DateTime DateValue
        {
            get { return new DtFunc().StringToDateTime(this.Value, DtFunc.FmtISODate); }
            set { this.Value = DtFunc.DateTimeToString(value, DtFunc.FmtISODate); }
        }
        #endregion Accessors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IAdjustedDate Members
        string IAdjustedDate.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        DateTime IAdjustedDate.DateValue
        {
            set { this.DateValue = value; }
            get { return this.DateValue; }
        }
        string IAdjustedDate.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        #endregion IAdjustedDate Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            IdentifiedDate clone = new IdentifiedDate
            {
                DateValue = this.DateValue
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion IdentifiedDate
    #region InformationProvider
    public partial class InformationProvider : ICloneable, IScheme
    {
        #region Constructors
        public InformationProvider()
        {
            informationProviderScheme = "http://www.fpml.org/coding-scheme/information-provider-2-0";
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            InformationProvider clone = new InformationProvider
            {
                informationProviderScheme = this.informationProviderScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.informationProviderScheme = value; }
            get { return this.informationProviderScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

    }
    #endregion InformationProvider
    #region InformationSource
    public partial class InformationSource : ICloneable, IEFS_Array, IInformationSource
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set
            {
                otcmlId = value.ToString();
                assetFxRateId.OTCmlId = value;
            }
        }
        #endregion Accessors
        #region Constructors
        public InformationSource()
        {
            rateSource = new InformationProvider();
            assetFxRateId = new AssetFxRateId();
        }
        #endregion Constructors
        #region Methods
        #region SetAssetFxRateId
        public void SetAssetFxRateId(int pIdAsset, string pIdentifier)
        {
            if (null != assetFxRateId)
                assetFxRateId = new AssetFxRateId();
            assetFxRateId.OTCmlId = pIdAsset;
            assetFxRateId.Value = pIdentifier;
        }
        #endregion SetAssetFxRateId
        #endregion Methods

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            InformationSource clone = new InformationSource
            {
                assetFxRateId = (AssetFxRateId)this.assetFxRateId.Clone(),
                rateSource = (InformationProvider)this.rateSource.Clone(),
                rateSourcePageSpecified = this.rateSourcePageSpecified,
                rateSourcePageHeadingSpecified = this.rateSourcePageHeadingSpecified,
                otcmlId = this.otcmlId
            };
            if (clone.rateSourcePageSpecified)
                clone.rateSourcePage = (RateSourcePage)this.rateSourcePage.Clone();
            if (clone.rateSourcePageHeadingSpecified)
                clone.rateSourcePageHeading = new EFS_String(this.rateSourcePageHeading.Value);

            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IInformationSource Members
        IAssetFxRateId IInformationSource.AssetFxRateId
        {
            get { return this.assetFxRateId; }
        }
        int IInformationSource.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        IScheme IInformationSource.RateSource
        {
            set { this.rateSource = (InformationProvider)value; }
            get { return this.rateSource; }
        }
        bool IInformationSource.RateSourcePageSpecified
        {
            set { this.rateSourcePageSpecified = value; }
            get { return this.rateSourcePageSpecified; }
        }
        IScheme IInformationSource.RateSourcePage
        {
            set { this.rateSourcePage = (RateSourcePage)value; }
            get { return this.rateSourcePage; }
        }
        bool IInformationSource.RateSourcePageHeadingSpecified
        {
            set { this.rateSourcePageHeadingSpecified = value; }
            get { return this.rateSourcePageHeadingSpecified; }
        }
        string IInformationSource.RateSourcePageHeading
        {
            set { this.rateSourcePageHeading = new EFS_String(value); }
            get { return this.rateSourcePageHeading.Value; }
        }
        void IInformationSource.CreateRateSourcePage(string pRateSourcePage)
        {
            this.rateSourcePage = new RateSourcePage
            {
                Value = pRateSourcePage
            };
        }
        void IInformationSource.SetAssetFxRateId(int pIdAsset, string pIdentifier) { this.SetAssetFxRateId(pIdAsset, pIdentifier); }
        #endregion IInformationSource Members
    }
    #endregion InformationSource
    #region InstrumentId
    public partial class InstrumentId : IEFS_Array, IScheme
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.instrumentIdScheme = value; }
            get { return this.instrumentIdScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

        #region constructor
        public InstrumentId()
        {
            this.instrumentIdScheme = "http://www.euro-finance-systems.fr/otcml/instrumentId";
        }
        #endregion IScheme Members

    }
    #endregion InstrumentId
    #region InterestAccrualsMethod
    // EG 20140702 Upd Interface
    // EG 20140702 Add _sql_Asset
    // EG 20170510 [23153] Add SetAsset 
    public partial class InterestAccrualsMethod : IInterestAccrualsMethod
    {
        // Variable de travail
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_AssetBase _sql_Asset;

        #region Constructors
        public InterestAccrualsMethod()
        {
            rateFloatingRate = new FloatingRateCalculation();
            rateFixedRate = new EFS_Decimal();
        }
        #endregion Constructors

        #region IInterestAccrualsMethod Members
        bool IInterestAccrualsMethod.FloatingRateSpecified
        {
            set { this.rateFloatingRateSpecified = value; }
            get { return this.rateFloatingRateSpecified; }
        }
        IFloatingRateCalculation IInterestAccrualsMethod.FloatingRate
        {
            set { this.rateFloatingRate = (FloatingRateCalculation)value; }
            get { return this.rateFloatingRate; }
        }
        bool IInterestAccrualsMethod.FixedRateSpecified
        {
            set { this.rateFixedRateSpecified = value; }
            get { return this.rateFixedRateSpecified; }
        }
        EFS_Decimal IInterestAccrualsMethod.FixedRate
        {
            set { this.rateFixedRate = value; }
            get { return this.rateFixedRate; }
        }
        bool IInterestAccrualsMethod.SqlAssetSpecified
        {
            get { return (null != this._sql_Asset); }
        }
        SQL_AssetBase IInterestAccrualsMethod.SqlAsset
        {
            set { this._sql_Asset = value; }
            get { return this._sql_Asset; }
        }
        // EG 20170510 [23153]
        void IInterestAccrualsMethod.SetAsset(string pCS, IDbTransaction pDbTransaction)
        {
            if (rateFloatingRateSpecified)
            {
                this._sql_Asset = new SQL_AssetRateIndex(pCS, SQL_AssetRateIndex.IDType.IDASSET, rateFloatingRate.floatingRateIndex.OTCmlId);
                if (null != pDbTransaction)
                    this._sql_Asset.DbTransaction = pDbTransaction;
                this._sql_Asset.LoadTable();
            }
        }

        #endregion IInterestAccrualsMethod Members
    }
    #endregion InterestAccrualsMethod

    #region IntermediaryInformation
    public partial class IntermediaryInformation : IEFS_Array, IIntermediaryInformation, ICloneable
    {
        #region Constructors
        public IntermediaryInformation()
        {
            routingIds = new RoutingIds();
            routingExplicitDetails = new RoutingExplicitDetails();
            routingIdsAndExplicitDetails = new RoutingIdsAndExplicitDetails();
        }
        #endregion Constructors
        #region Methods

        #region GetRoutingAccountNumber
        public string GetRoutingAccountNumber()
        {
            return SettlementTools.GetRoutingAccountNumber((IRouting)this);
        }
        #endregion GetRoutingAccountNumber
        #endregion Methods

        #region ICloneable members
        public object Clone()
        {
            IntermediaryInformation clone = (IntermediaryInformation)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable members

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
        #region IRouting Membres
        bool IRouting.RoutingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.RoutingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.RoutingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.RoutingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.RoutingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.RoutingIdsAndExplicitDetails
        {
            get { return this.routingIdsAndExplicitDetails; }
            set { this.routingIdsAndExplicitDetails = (RoutingIdsAndExplicitDetails)value; }
        }
        IRouting IRouting.Clone()
        {
            return (IRouting)this.Clone();
        }
        #endregion IRouting Membres
        #region IIntermediaryInformation Members
        EFS_PosInteger IIntermediaryInformation.IntermediarySequenceNumber
        {
            set { this.intermediarySequenceNumber = value; }
            get { return this.intermediarySequenceNumber; }
        }
        #endregion IIntermediaryInformation Members
    }
    #endregion IntermediaryInformation
    #region Interval
    public partial class Interval : ICloneable, IComparable, IInterval
    {
        #region Accessors
        #region Id
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
        #endregion Id
        #region Period
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Period
        {
            get { return period.ToString(); }
        }
        #endregion Period
        #region PeriodMultiplier
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int PeriodMultiplier
        {
            get { return periodMultiplier.IntValue; }
        }
        #endregion PeriodMultiplier
        #endregion Accessors
        #region Constructors
        public Interval()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
        }

        public Interval(PeriodEnum pPeriod, int pPeriodMultiplier)
        {
            periodMultiplier = new EFS_Integer(pPeriodMultiplier);
            period = pPeriod;
        }

        public Interval(string pPeriod, int pPeriodMultiplier)
        {
            periodMultiplier = new EFS_Integer(pPeriodMultiplier);
            period = StringToEnum.Period(pPeriod);
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Interval clone = new Interval
            {
                periodMultiplier = (EFS_Integer)this.periodMultiplier.Clone(),
                period = this.period
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IComparable Members
        #region CompareTo
        /// <summary>
        /// Retourne 0 si la periode et le multiplier sont identiques, -1 sinon 
        /// </summary>
        public int CompareTo(object obj)
        {
            int ret = -1;
            Interval interval = (Interval)obj;
            if ((period == interval.period) && (periodMultiplier.IntValue == interval.periodMultiplier.IntValue))
                ret = 0;
            return ret;
        }
        #endregion CompareTo
        #endregion IComparable Members
        #region IInterval Members
        PeriodEnum IInterval.Period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IInterval.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod) { return new Interval(pPeriod.ToString(), pMultiplier); }
        IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision) { return new Rounding(pRoundingDirection, pPrecision); }
        int IInterval.CompareTo(object obj)
        {
            return this.CompareTo(obj);
        }
        #endregion IInterval Members
    }
    #endregion Interval
    #region InterpolationMethod
    public partial class InterpolationMethod : IScheme
    {
        #region Constructors
        public InterpolationMethod()
        {
            interpolationMethodScheme = "http://www.fpml.org/coding-scheme/interpolation-method-1-1";
        }
        #endregion Constructors

        #region IScheme Membres

        string IScheme.Scheme
        {
            set { this.interpolationMethodScheme = value; }
            get { return this.interpolationMethodScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }

        #endregion
    }
    #endregion InterpolationMethod

    #region ManualExercise
    public partial class ManualExercise : IManualExercise
    {
        #region IManualExercise Members
        bool IManualExercise.ExerciseNoticeSpecified { get { return this.exerciseNoticeSpecified; } }
        bool IManualExercise.FallbackExerciseSpecified { get { return this.fallbackExerciseSpecified; } }
        bool IManualExercise.FallbackExercise { get { return this.fallbackExercise.BoolValue; } }
        #endregion IManualExercise Members
    }
    #endregion ManualExercise
    #region MasterAgreement
    public partial class MasterAgreement : IMasterAgreement
    {

        #region IMasterAgreement Membres
        IScheme IMasterAgreement.MasterAgreementType
        {
            get { return this.masterAgreementType; }
            set { this.masterAgreementType = (MasterAgreementType)value; }
        }

        bool IMasterAgreement.MasterAgreementDateSpecified
        {
            get { return this.masterAgreementDateSpecified; }
            set { this.masterAgreementDateSpecified = value; }
        }
        EFS_Date IMasterAgreement.MasterAgreementDate
        {
            get { return this.masterAgreementDate; }
            set { this.masterAgreementDate = value; }
        }
        IScheme IMasterAgreement.CreateMasterAgreementType()
        {
            return new MasterAgreementType();
        }
        #endregion
    }
    #endregion
    #region MasterAgreementType
    public partial class MasterAgreementType : IScheme
    {
        #region IScheme Membres
        string IScheme.Scheme
        {
            get { return this.masterAgreementTypeScheme; }
            set { this.masterAgreementTypeScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion
    }
    #endregion
    #region Math
    public partial class Math : IMath
    {
        #region IMath Members
        System.Xml.XmlNode[] IMath.Any
        {
            set { this.Any = value; }
            get { return this.Any; }
        }
        #endregion IMath Members
    }
    #endregion Math


    #region Money
    public partial class Money : ICloneable, IMoney, IEFS_Array
    {
        #region Accessors
        #region Amount
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get{ return amount; }
        }
        #endregion Amount
        #region Currency
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get{ return currency.Value; }
        }
        #endregion Currency
        #region id
        [System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
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
        #endregion id
        #endregion Accessors
        #region Constructors
        public Money()
        {
            amount = new EFS_Decimal();
            currency = new Currency();
        }
        public Money(Decimal pAmount, string pCur)
        {
            amount = new EFS_Decimal(pAmount);
            currency = new Currency(pCur);

        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Money clone = new Money
            {
                currency = (Currency)this.currency.Clone(),
                amount = new EFS_Decimal(amount.DecValue)
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IMoney Members
        #region Accessors
        EFS_Decimal IMoney.Amount
        {
            set { this.amount = value; }
            get { return this.amount; }
        }
        string IMoney.Currency
        {
            set { this.currency.Value = value; }
            get { return this.currency.Value; }
        }
        IOffset IMoney.DefaultOffsetPreSettlement { get { return new Offset(PeriodEnum.D, -2, DayTypeEnum.Business); } }
        IOffset IMoney.DefaultOffsetUsanceDelaySettlement { get { return new Offset(PeriodEnum.D, 2, DayTypeEnum.Business); } }
        ICurrency IMoney.GetCurrency 
        {
            get { return this.currency; } 
        }
        #endregion Accessors
        #region Methods
        IOffset IMoney.CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType)
        {
            return new Offset(pPeriod, pMultiplier, pDayType);
        }
        IMoney IMoney.Clone() { return (IMoney)this.Clone(); }
        #endregion Methods
        #endregion IMoney Members

        #region IEFS_Array Membres

        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }

        #endregion
    }
    #endregion Money
    #region MultipleExercise
    public partial class MultipleExercise : IMultipleExercise
    {
        #region Constrcutors
        public MultipleExercise()
        {
            minimumNumberOfOptions = new EFS_PosInteger();
            minimumNotionalAmount = new EFS_Decimal();
            maximumNone = new Empty();
            maximumNotionalAmount = new EFS_Decimal();
            maximumNumberOfOptions = new EFS_PosInteger();
        }
        #endregion Constructors
        #region IMultipleExercise Members
        bool IMultipleExercise.MaximumNumberOfOptionsSpecified
        {
            get { return this.maximumNumberOfOptionsSpecified; }
        }
        EFS_PosInteger IMultipleExercise.MaximumNumberOfOptions
        {
            get { return this.maximumNumberOfOptions; }
        }
        bool IMultipleExercise.MaximumNotionalAmountSpecified
        {
            get { return this.maximumNotionalAmountSpecified; }
        }
        EFS_Decimal IMultipleExercise.MaximumNotionalAmount
        {
            get { return this.maximumNotionalAmount; }
        }
        #endregion IMultipleExercise Members
        #region IPartialExercise Members
        bool IPartialExercise.NotionalReferenceSpecified
        {
            set { this.notionalReferenceSpecified = value; }
            get { return this.notionalReferenceSpecified; }
        }
        IReference[] IPartialExercise.NotionalReference
        {
            set { this.notionalReference = (ScheduleReference[])value; }
            get { return this.notionalReference; }
        }
        bool IPartialExercise.IntegralMultipleAmountSpecified
        {
            set { this.integralMultipleAmountSpecified = value; }
            get { return this.integralMultipleAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.IntegralMultipleAmount
        {
            set { this.integralMultipleAmount = value; }
            get { return this.integralMultipleAmount; }
        }
        bool IPartialExercise.MinimumNumberOfOptionsSpecified
        {
            set { this.minimumNumberOfOptionsSpecified = value; }
            get { return this.minimumNumberOfOptionsSpecified; }
        }
        EFS_PosInteger IPartialExercise.MinimumNumberOfOptions
        {
            set { this.minimumNumberOfOptions = value; }
            get { return this.minimumNumberOfOptions; }
        }
        bool IPartialExercise.MinimumNotionalAmountSpecified
        {
            set { this.minimumNotionalAmountSpecified = value; }
            get { return this.minimumNotionalAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.MinimumNotionalAmount
        {
            set { this.minimumNotionalAmount = value; }
            get { return this.minimumNotionalAmount; }
        }
        #endregion IPartialExercise Members

    }
    #endregion MultipleExercise

    #region Offset
    public partial class Offset : ICloneable, IOffset
    {
        #region Accessors
        #region id
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
        #endregion id
        #endregion Accessors
        #region Constructors
        public Offset()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
            dayType = new DayTypeEnum();
        }

        public Offset(PeriodEnum pPeriod, int pPeriodMultiplier, DayTypeEnum pDayType)
        {
            periodMultiplier = new EFS_Integer(pPeriodMultiplier);
            period = pPeriod;
            dayType = pDayType;
            dayTypeSpecified = true;
        }

        public Offset(string pPeriod, int pPeriodMultiplier, string pDayType)
        {
            periodMultiplier = new EFS_Integer(pPeriodMultiplier);
            period = StringToEnum.Period(pPeriod);
            dayType = StringToEnum.DayType(pDayType);
            dayTypeSpecified = true;
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Offset clone = new Offset
            {
                periodMultiplier = (EFS_Integer)this.periodMultiplier.Clone(),
                period = this.period,
                dayTypeSpecified = this.dayTypeSpecified,
                dayType = this.dayType
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IOffset Members
        bool IOffset.DayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IOffset.DayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        /// <summary>
        /// Retourne les business Centers associées au devises {pCurrencies}
        ///  <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrencies">Devises au formats ISO4217_ALPHA3</param>
        /// <returns></returns>
        /// FI 20131118 [19118] usage de ArrFunc.IsFilled
        // EG 20180307 [23769] Gestion dbTransaction
        IBusinessCenters IOffset.GetBusinessCentersCurrency(string pCS, IDbTransaction pDbTransaction, params string[] pCurrencies)
        {
            BusinessCenters businessCenters = null;
            DataRowCollection rows = Tools.LoadBusinessCenter(pCS, pDbTransaction, null, pCurrencies, null);
            
            //if (0 < rows.Count)
            if (ArrFunc.IsFilled(rows))  
            {
                BusinessCenter[] businessCenter = new BusinessCenter[rows.Count];
                int i = 0;
                foreach (DataRow row in rows)
                {
                    if (0 < row[0].ToString().Length)
                    {
                        businessCenter[i] = new BusinessCenter
                        {
                            Value = row[0].ToString()
                        };
                        i++;
                    }
                }
                businessCenters = new BusinessCenters(businessCenter);
            }
            return businessCenters;
        }
        IBusinessDayAdjustments IOffset.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments
            {
                businessDayConvention = pBusinessDayConvention
            };
            if (ArrFunc.IsFilled(pIdBC))
            {
                BusinessCenters bcs = new BusinessCenters();
                foreach (string idBC in pIdBC)
                {
                    bcs.Add(new BusinessCenter(idBC));
                }
                if (0 < bcs.businessCenter.Length)
                {
                    bda.businessCentersDefineSpecified = true;
                    bda.businessCentersDefine = bcs;
                }
            }
            return bda;
        }
        #endregion IOffset Members
        #region IInterval Members
        PeriodEnum IInterval.Period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IInterval.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod)
        {
            return new Interval(pPeriod.ToString(), pMultiplier);
        }
        IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision)
        {
            return ((IInterval)this).GetRounding(pRoundingDirection, pPrecision);
        }
        int IInterval.CompareTo(object obj)
        {
            Interval intervale = new Interval(this.period, this.periodMultiplier.IntValue);
            IOffset offsetObj = (IOffset)obj;
            Interval intervaleObj = new Interval(offsetObj.Period, offsetObj.PeriodMultiplier.IntValue);
            int ret = ((IInterval)intervale).CompareTo(intervaleObj);
            return ret;
        }
        #endregion
    }
    #endregion Offset

    #region PartialExercise
    public partial class PartialExercise : IPartialExercise
    {
        #region IPartialExercise Members
        bool IPartialExercise.NotionalReferenceSpecified
        {
            set { this.notionalReferenceSpecified = value; }
            get { return this.notionalReferenceSpecified; }
        }
        IReference[] IPartialExercise.NotionalReference
        {
            set { this.notionalReference = (ScheduleReference[])value; }
            get { return this.notionalReference; }
        }
        bool IPartialExercise.IntegralMultipleAmountSpecified
        {
            set { this.integralMultipleAmountSpecified = value; }
            get { return this.integralMultipleAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.IntegralMultipleAmount
        {
            set { this.integralMultipleAmount = value; }
            get { return this.integralMultipleAmount; }
        }
        bool IPartialExercise.MinimumNumberOfOptionsSpecified
        {
            set { this.minimumNumberOfOptionsSpecified = value; }
            get { return this.minimumNumberOfOptionsSpecified; }
        }
        EFS_PosInteger IPartialExercise.MinimumNumberOfOptions
        {
            set { this.minimumNumberOfOptions = value; }
            get { return this.minimumNumberOfOptions; }
        }
        bool IPartialExercise.MinimumNotionalAmountSpecified
        {
            set { this.minimumNotionalAmountSpecified = value; }
            get { return this.minimumNotionalAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.MinimumNotionalAmount
        {
            set { this.minimumNotionalAmount = value; }
            get { return this.minimumNotionalAmount; }
        }
        #endregion IPartialExercise Members
    }
    #endregion PartialExercise
    #region Party
    /// EG 20170922 [22374] New The party's time zone.
    /// EG 20170926 [22374] Add tzdbid
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Modify
    public partial class Party : IEFS_Array, IParty
    {
        #region Accessors
        #region PartyId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IScheme PartyId
        {
            set { this.partyId[0] = (PartyId)value; }
            get { return this.partyId[0]; }
        }
        #endregion PartyId
        #region PartyIds
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IScheme[] PartyIds
        {
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
        string IParty.PartyId
        {
            set { this.partyId[0].Value = value; }
            get { return this.partyId[0].Value; }
        }
        string IParty.PartyName
        {
            set { this.partyName = value; }
            get { return this.partyName; }
        }
        string IParty.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        string IParty.OtcmlId { get { return this.otcmlId; } }
        int IParty.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        string IParty.Tzdbid
        {
            set { this.tzdbid = value; }
            get { return this.tzdbid; }
        }

        Boolean IParty.PersonSpecified
        {
            set { this.personSpecified = value; }
            get { return this.personSpecified; }
        }
        // EG 20171016 [23509] Upd
        IPerson[] IParty.Person //FI 20170928 [23452]
        {
            //set
            //{
            //    person = null;
            //    if (ArrFunc.IsFilled(value))
            //    {
            //        person = (from item in value
            //                  select new EfsML.v30.MiFIDII_Extended.Person()
            //                  {
            //                      firstNameSpecified = item.firstNameSpecified,
            //                      firstName = (item.firstNameSpecified) ? new EFS_String(item.firstName) : new EFS_String(),
            //                      surnameSpecified = item.surnameSpecified,
            //                      surname = (item.surnameSpecified) ? new EFS_String(item.surname) : new EFS_String(),
            //                      personIdSpecified = item.personIdSpecified,
            //                      personId = (item.personIdSpecified) ? (from itemPerson in item.personId
            //                                                             select new PersonId()
            //                                                             {
            //                                                                 personIdScheme = itemPerson.scheme,
            //                                                                 Value = itemPerson.Value
            //                                                             }).ToArray() : new PersonId[] { new PersonId() },
            //                      id = item.id,
            //                      otcmlId = item.otcmlId
            //                  }).ToArray();
            //    }
            //}
            set {this.person = value.Cast<Person>().ToArray();}
            get { return this.person; }
        }

        #endregion IParty Members
    }
    #endregion Party
    #region PartyId
    public partial class PartyId : ICloneable, IScheme, IEFS_Array
    {
        #region Constructors
        public PartyId()
        {
            partyIdScheme = Cst.OTCml_ActorIdentifierScheme;
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            PartyId clone = new PartyId
            {
                partyIdScheme = this.partyIdScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new EFS.GUI.ComplexControls.PartyId(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

    }
    #endregion PartyId

    #region PartyOrAccountReference
    public partial class PartyOrAccountReference : ICloneable, IReference
    {
        #region Constructor
        public PartyOrAccountReference() { }
        public PartyOrAccountReference(string pPartyReference)
        {
            this.href = pPartyReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            PartyOrAccountReference clone = new PartyOrAccountReference
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
    #endregion PartyOrAccountReference
    #region PartyOrTradeSideReference
    public partial class PartyOrTradeSideReference : ICloneable, IReference
    {
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            PartyOrTradeSideReference clone = new PartyOrTradeSideReference { href = this.href };
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
    #endregion PartyOrTradeSideReference
    #region PartyReference
    public partial class PartyReference : ICloneable, IReference
    {
        #region Constructor
        public PartyReference() { }
        public PartyReference(string pPartyReference)
        {
            this.href = pPartyReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            PartyReference clone = new PartyReference { href = this.href };
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
    #endregion PartyReference
    #region Payment
    public partial class Payment : IEFS_Array, IPayment
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Payment efs_Payment;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Payment";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180328 [23871] Add Id
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180328 [23871] Add Id
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

        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                return this.efs_Payment.AdjustedPaymentDate;
            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {
                return this.efs_Payment.AdjustedPreSettlementDate;
            }
        }
        #endregion AdjustedPreSettlementDate
        #region ExpirationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpirationDate
        {
            get
            {
                if (null != efs_Payment.expirationDate)
                    return efs_Payment.expirationDate;
                else
                    return PaymentDate;
            }
        }

        #endregion ExpirationDate
        #region ExchangeRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeRate ExchangeRate
        {
            get
            {
                return efs_Payment.ExchangeRate;
            }
        }
        #endregion ExchangeRate
        #region Invoicing_AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date Invoicing_AdjustedPaymentDate
        {
            get
            {
                return this.efs_Payment.Invoicing_AdjustedPaymentDate;
            }
        }
        #endregion Invoicing_AdjustedPaymentDate
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.href;
            }
        }
        #endregion PayerPartyReference
        #region PaymentAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PaymentAmount
        {
            get
            {
                return this.efs_Payment.paymentAmount.Amount;
            }
        }
        #endregion PaymentAmount
        #region PaymentQuote
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentQuote PaymentQuote
        {
            get
            {
                return efs_Payment.PaymentQuote;
            }
        }
        #endregion PaymentQuote
        #region PaymentSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentSource PaymentSource
        {
            get
            {
                return efs_Payment.PaymentSource;
            }
        }
        #endregion PaymentSource

        #region PaymentCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PaymentCurrency
        {
            get
            {
                if (this.efs_Payment == null)
                    return this.paymentAmount.currency.Value;
                else
                    return this.efs_Payment.paymentAmount.Currency;

            }
        }
        #endregion PaymentCurrency
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return this.efs_Payment.PaymentDate;
            }
        }
        #endregion PaymentDate
        #region PaymentSettlementCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PaymentSettlementCurrency
        {
            get
            {
                if (customerSettlementPaymentSpecified)
                    return customerSettlementPayment.currency.Value;
                else
                    return paymentAmount.currency.Value;

            }
        }
        #endregion PaymentSettlementCurrency
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.href;
            }

        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        public Payment()
        {
            adjustedPaymentDate = new IdentifiedDate();
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
            settlementInformation = new SettlementInformation();
            paymentDate = new AdjustableDate();
            paymentType = new PaymentType();
            paymentAmount = new Money();
            paymentSource = new SpheresSource();
        }
        #endregion Constructors
        #region Methods
        #region PaymentType
        public string PaymentType(string pEventCode)
        {

            // RD 20100708/ FI 20100708
            //efs_Payment n'est valorisé que dans la génération des évènements
            //ailleurs il est null
            if ((null != efs_Payment) && efs_Payment.paymentSourceSpecified)
            {
                return efs_Payment.paymentSource.eventType;
            }
            else if (paymentSourceSpecified)
            {
                EFS_PaymentSource paymentSource = new EFS_PaymentSource(this);
                return paymentSource.eventType;
            }
            else
            {
                // NORMALEMENT ON NE PASSE PLUS DANS CE PAVE
                // PL 20150126 Et bien si, on passe encore dans ce pavé, notamment dans le cas des "Cash-Payment".
                if (paymentTypeSpecified)
                {
                    if (paymentType.Value == "Brokerage")
                        return EventTypeFunc.Brokerage.ToString();
                    else if (paymentType.Value == "Fee")
                        return EventTypeFunc.Fee.ToString();
                    else if (paymentType.Value == "Cash")
                        return EventTypeFunc.CashSettlement.ToString();
                    else if (3 <= paymentType.Value.Length)
                        return paymentType.Value.Substring(0, 3).ToUpper();
                    else
                        return paymentType.Value.ToUpper();
                }
                else if (EventCodeFunc.IsAdditionalPayment(pEventCode))
                    return EventTypeFunc.Fee;
                else if (EventCodeFunc.IsOtherPartyPayment(pEventCode))
                    return EventTypeFunc.Brokerage;
                else
                    return EventTypeFunc.CashSettlement.ToString();
            }

        }
        #endregion PaymentType
        #endregion Methods

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

        #region IPayment Members
        IReference IPayment.PayerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
        }
        IReference IPayment.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        }
        IMoney IPayment.PaymentAmount
        {
            get { return this.paymentAmount; }
        }
        bool IPayment.PaymentDateSpecified
        {
            set { this.paymentDateSpecified = value; }
            get { return this.paymentDateSpecified; }
        }
        // EG 20101020 Ticket:17185
        IAdjustableDate IPayment.PaymentDate
        {
            set { this.paymentDate = (AdjustableDate)value; }
            get { return this.paymentDate; }
        }
        bool IPayment.AdjustedPaymentDateSpecified
        {
            set { this.adjustedPaymentDateSpecified = value; }
            get { return this.adjustedPaymentDateSpecified; }
        }
        DateTime IPayment.AdjustedPaymentDate
        {
            set { this.adjustedPaymentDate.DateValue = value; }
            get { return this.adjustedPaymentDate.DateValue; }
        }
        bool IPayment.PaymentQuoteSpecified
        {
            set { this.paymentQuoteSpecified = value; }
            get { return this.paymentQuoteSpecified; }
        }
        IPaymentQuote IPayment.PaymentQuote
        {
            set { this.paymentQuote = (PaymentQuote)value; }
            get { return this.paymentQuote; }
        }
        bool IPayment.CustomerSettlementPaymentSpecified
        {
            set { this.customerSettlementPaymentSpecified = value; }
            get { return this.customerSettlementPaymentSpecified; }
        }
        ICustomerSettlementPayment IPayment.CustomerSettlementPayment
        {
            set { this.customerSettlementPayment = (CustomerSettlementPayment)value; }
            get { return this.customerSettlementPayment; }
        }
        bool IPayment.PaymentSourceSpecified
        {
            set { this.paymentSourceSpecified = value; }
            get { return this.paymentSourceSpecified; }
        }
        ISpheresSource IPayment.PaymentSource
        {
            set { this.paymentSource = (SpheresSource)value; }
            get { return this.paymentSource; }
        }
        bool IPayment.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        ITax[] IPayment.Tax
        {
            set { this.tax = (Tax[])value; }
            get { return this.tax; }
        }
        string IPayment.PaymentCurrency
        {
            get { return this.PaymentCurrency; }
        }
        string IPayment.PaymentSettlementCurrency { get { return this.PaymentSettlementCurrency; } }
        IAdjustedDate IPayment.CreateAdjustedDate(DateTime pAdjustedDate)
        {
            IdentifiedDate adjustedDate = new IdentifiedDate
            {
                DateValue = pAdjustedDate
            };
            return adjustedDate;
        }
        IAdjustedDate IPayment.CreateAdjustedDate(string pAdjustedDate)
        {
            IdentifiedDate adjustedDate = new IdentifiedDate
            {
                Value = pAdjustedDate
            };
            return adjustedDate;
        }
        IMoney IPayment.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        IMoney IPayment.GetNotionalAmountReference(object pNotionalReference)
        {
            IMoney notionalAmount = null;

            if (null != pNotionalReference)
            {
                Type tNotionalReference = pNotionalReference.GetType();
                if (tNotionalReference.Equals(typeof(KnownAmountSchedule)))
                {
                    KnownAmountSchedule notionalReference = (KnownAmountSchedule)pNotionalReference;
                    notionalAmount = new Money(notionalReference.initialValue.DecValue, notionalReference.currency.Value);
                }
                else if (tNotionalReference.Equals(typeof(AmountSchedule)))
                {
                    AmountSchedule notionalReference = (AmountSchedule)pNotionalReference;
                    notionalAmount = new Money(notionalReference.initialValue.DecValue, notionalReference.currency.Value);
                }
                else if (tNotionalReference.Equals(typeof(Notional)))
                {
                    Notional notionalReference = (Notional)pNotionalReference;
                    notionalAmount = new Money(notionalReference.notionalStepSchedule.initialValue.DecValue,
                        notionalReference.notionalStepSchedule.currency.Value);
                }
            }

            return notionalAmount;
        }
        EFS_Payment IPayment.Efs_Payment
        {
            get { return this.efs_Payment; }
            set { efs_Payment = value; }
        }
        bool IPayment.SettlementInformationSpecified
        {
            set { this.settlementInformationSpecified = value; }
            get { return this.settlementInformationSpecified; }
        }
        ISettlementInformation IPayment.SettlementInformation
        {
            get { return this.settlementInformation; }
        }
        ICustomerSettlementPayment IPayment.CreateCustomerSettlementPayment { get { return new CustomerSettlementPayment(); } }
        bool IPayment.PaymentTypeSpecified
        {
            set { this.paymentTypeSpecified = value; }
            get { return this.paymentTypeSpecified; }
        }
        IScheme IPayment.PaymentType
        {
            set { this.paymentType = (PaymentType)value; }
            get { return this.paymentType; }
        }
        IPaymentQuote IPayment.CreatePaymentQuote { get { return new PaymentQuote(); } }
        IReference IPayment.CreateReference { get { return new AmountReference(); } }
        string IPayment.GetPaymentType(string pEventCode)
        {
            return this.PaymentType(pEventCode);
        }
        ITax IPayment.CreateTax { get { return new Tax(); } }
        ITaxSchedule IPayment.CreateTaxSchedule { get { return new TaxSchedule(); } }
        ISpheresSource IPayment.CreateSpheresSource { get { return new SpheresSource(); } }
        /// <summary>
        /// La ligne de frais Forcée matche t-elle avec une ligne de frais recalculée issu d'un barème ???
        /// Quels éléments sont des critères de matching
        /// </summary>
        /// <param name="pPayment"></param>
        IPayment IPayment.DeleteMatchPayment(ArrayList pPayment)
        {
            IPayment _matchPayment = null;
            foreach (IPayment _payment in pPayment)
            {
                bool isMatch = (payerPartyReference.href == _payment.PayerPartyReference.HRef);
                isMatch &= (receiverPartyReference.href == _payment.ReceiverPartyReference.HRef);
                isMatch &= (paymentTypeSpecified == _payment.PaymentTypeSpecified);
                isMatch &= (paymentType.Value == _payment.PaymentType.Value);
                isMatch &= (paymentAmount.Currency == _payment.PaymentAmount.Currency);
                isMatch &= (paymentDateSpecified == _payment.PaymentDateSpecified);
                isMatch &= (paymentDate.unadjustedDate.DateValue == _payment.PaymentDate.UnadjustedDate.DateValue);

                isMatch &= paymentSourceSpecified;
                if (isMatch)
                {
                    ISpheresSource _source = paymentSource as ISpheresSource;
                    #region IdFeeMatrix
                    if (Tools.IsPaymentSourceScheme(this, Cst.OTCml_RepositoryFeeMatrixScheme) &&
                        Tools.IsPaymentSourceScheme(_payment, Cst.OTCml_RepositoryFeeMatrixScheme)
                        )
                    {
                        isMatch &= (_source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId ==
                                   _payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId);
                    }
                    #endregion IdFeeMatrix
                    #region IdFee
                    if (Tools.IsPaymentSourceScheme(this, Cst.OTCml_RepositoryFeeScheme) &&
                        Tools.IsPaymentSourceScheme(_payment, Cst.OTCml_RepositoryFeeScheme)
                        )
                    {
                        isMatch &= (_source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId ==
                                   _payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId);
                    }
                    #endregion IdFee
                    #region IdFeeSchedule
                    if (Tools.IsPaymentSourceScheme(this, Cst.OTCml_RepositoryFeeScheme) &&
                        Tools.IsPaymentSourceScheme(_payment, Cst.OTCml_RepositoryFeeScheme)
                        )
                    {
                        isMatch &= (_source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId ==
                                   _payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId);
                    }
                    #endregion IdFeeSchedule
                }
                if (isMatch)
                {
                    _matchPayment = _payment;
                    break;
                }
            }
            return _matchPayment;
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180328 [23871] Add 
        string IPayment.Id
        {
            set { Id = value; }
            get { return Id; }
        }
        #endregion IPayment Members
    }
    #endregion Payment
    #region PaymentCurrency
    // EG 20231127 [WI755] Implementation Return Swap : DEL referenceSpecified + UPD Property Reference
    public partial class PaymentCurrency : IPaymentCurrency
    {
        #region Constructors
        public PaymentCurrency()
        {
            paymentCurrencyNone = new Empty();
            paymentCurrencyCurrency = new Currency();
            paymentCurrencyDeterminationMethod = new DeterminationMethod();
        }
        #endregion Constructors

        #region IPaymentCurrency Members
        string IPaymentCurrency.Reference
        {
            get { return this.Href; }
        }
        bool IPaymentCurrency.CurrencySpecified
        {
            get { return this.paymentCurrencyCurrencySpecified; }
        }
        ICurrency IPaymentCurrency.Currency
        {
            get { return this.paymentCurrencyCurrency; }
        }
        bool IPaymentCurrency.CurrencydeterminationMethodSpecified
        {
            get { return this.paymentCurrencyDeterminationMethodSpecified; }
        }
        string IPaymentCurrency.CurrencyDeterminationMethodValue
        {
            get { return this.paymentCurrencyDeterminationMethod.Value; }
        }
        IScheme IPaymentCurrency.CurrencyDeterminationMethod
        {
            get { return this.paymentCurrencyDeterminationMethod; }
        }
        #endregion IPaymentCurrency Members

    }
    #endregion PaymentCurrency
    #region PaymentType
    public partial class PaymentType : IEFS_Array, IScheme
    {
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
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.paymentTypeScheme = value; }
            get { return this.paymentTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion PaymentType
    #region PeriodicDates
    public partial class PeriodicDates : IPeriodicDates
    {
        #region Constructors
        public PeriodicDates()
        {
            calculationStartDate = new AdjustableOrRelativeDate();
            calculationPeriodFrequency = new CalculationPeriodFrequency();
            calculationPeriodDatesAdjustments = new BusinessDayAdjustments();
        }
        #endregion Constructors

        #region IPeriodicDates Members
        IAdjustableOrRelativeDate IPeriodicDates.CalculationStartDate
        {
            get { return this.calculationStartDate; }
        }
        bool IPeriodicDates.CalculationEndDateSpecified
        {
            get { return this.calculationEndDateSpecified; }
        }
        IAdjustableOrRelativeDate IPeriodicDates.CalculationEndDate
        {
            get { return this.calculationEndDate; }
        }
        ICalculationPeriodFrequency IPeriodicDates.CalculationPeriodFrequency
        {
            get { return this.calculationPeriodFrequency; }
        }
        IBusinessDayAdjustments IPeriodicDates.CalculationPeriodDatesAdjustments
        {
            get { return this.calculationPeriodDatesAdjustments; }
        }
        #endregion IPeriodicDates Members
    }
    #endregion PeriodicDates
    #region PrincipalExchanges
    public partial class PrincipalExchanges : IPrincipalExchanges
    {
        #region constructor
        public PrincipalExchanges()
        {
            this.initialExchange = new EFS_Boolean();
            this.intermediateExchange = new EFS_Boolean();
            this.finalExchange = new EFS_Boolean();
        }
        #endregion constructor

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

        #region IPrincipalExchanges Members
        EFS_Boolean IPrincipalExchanges.InitialExchange
        {
            set { this.initialExchange = value; }
            get { return this.initialExchange; }
        }
        EFS_Boolean IPrincipalExchanges.FinalExchange
        {
            set { this.finalExchange = value; }
            get { return this.finalExchange; }
        }
        EFS_Boolean IPrincipalExchanges.IntermediateExchange
        {
            set { this.intermediateExchange = value; }
            get { return this.intermediateExchange; }
        }
        #endregion IPrincipalExchanges Members

    }
    #endregion PrincipalExchanges
    #region Product
    // EG 20140702 Upd Interface
    // EG 20150302 Add FxRateAsset on method CreatePosKeepingAsset (CFD Forex)
    // EG 20150317 [POC] Add IsFungible|IsMargining|IsMarginingAndNotFungible
    // EG 20150422 [20513] BANCAPERTA New CreateBusinessCentersReference
    // EG 20171025 [23509] CreateTradeProcessingTimestamps
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public partial class Product : IEFS_Array, IProductBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Events efs_Events;
        #endregion Members
        #region Accessors
        #region EventGroupName
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public static string EventGroupName
        {
            get { return "Product"; }
        }
        #endregion EventGroupName
        #region id
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
        #endregion id
        #endregion Accessors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IProductBase Members
        bool IProductBase.IsStrategy { get { return this.GetType().Equals(typeof(Strategy)); } }
        bool IProductBase.IsBrokerEquityOption { get { return this.GetType().Equals(typeof(BrokerEquityOption)); } }
        bool IProductBase.IsBulletPayment { get { return this.GetType().Equals(typeof(BulletPayment)); } }
        bool IProductBase.IsCapFloor { get { return this.GetType().Equals(typeof(CapFloor)); } }
        bool IProductBase.IsDebtSecurity { get { return this.GetType().Equals(typeof(DebtSecurity)); } }
        bool IProductBase.IsDebtSecurityTransaction { get { return this.GetType().Equals(typeof(DebtSecurityTransaction)); } }
        bool IProductBase.IsEquityForward { get { return this.GetType().Equals(typeof(EquityForward)); } }
        bool IProductBase.IsEquityOption { get { return this.GetType().Equals(typeof(EquityOption)); } }
        bool IProductBase.IsEquityOptionTransactionSupplement { get { return this.GetType().Equals(typeof(EquityOptionTransactionSupplement)); } }
        /// <summary>
        /// Obtient true si le type est  EquitySecurityTransaction
        /// </summary>
        bool IProductBase.IsEquitySecurityTransaction { get { return this.GetType().Equals(typeof(EquitySecurityTransaction)); } }
        /// <summary>
        /// Obtient true si le type est  ExchangeTradedDerivative
        /// </summary>
        bool IProductBase.IsExchangeTradedDerivative { get { return this.GetType().Equals(typeof(ExchangeTradedDerivative)); } }
        bool IProductBase.IsFra { get { return this.GetType().Equals(typeof(Fra)); } }
        bool IProductBase.IsFutureTransaction { get { return this.GetType().Equals(typeof(FutureTransaction)); } }
        bool IProductBase.IsFxAverageRateOption { get { return this.GetType().Equals(typeof(FxAverageRateOption)); } }
        bool IProductBase.IsFxBarrierOption { get { return this.GetType().Equals(typeof(FxBarrierOption)); } }
        bool IProductBase.IsFxDigitalOption { get { return this.GetType().Equals(typeof(FxDigitalOption)); } }
        bool IProductBase.IsFxLeg { get { return this.GetType().Equals(typeof(FxLeg)); } }
        bool IProductBase.IsFxSwap { get { return this.GetType().Equals(typeof(FxSwap)); } }
        bool IProductBase.IsFxTermDeposit { get { return this.GetType().Equals(typeof(TermDeposit)); } }
        bool IProductBase.IsFxOptionLeg { get { return this.GetType().Equals(typeof(FxOptionLeg)); } }
        bool IProductBase.IsInvoice { get { return this.GetType().Equals(typeof(Invoice)); } }
        bool IProductBase.IsInvoiceSettlement { get { return this.GetType().Equals(typeof(InvoiceSettlement)); } }
        bool IProductBase.IsAdditionalInvoice { get { return this.GetType().Equals(typeof(AdditionalInvoice)); } }
        bool IProductBase.IsCreditNote { get { return this.GetType().Equals(typeof(CreditNote)); } }
        bool IProductBase.IsLoanDeposit { get { return this.GetType().Equals(typeof(LoanDeposit)); } }
        bool IProductBase.IsReturnSwap { get { return this.GetType().Equals(typeof(ReturnSwap)); } }
        bool IProductBase.IsEquitySwapTransactionSupplement { get { return this.GetType().Equals(typeof(EquitySwapTransactionSupplement)); } }
        bool IProductBase.IsVarianceSwapTransactionSupplement { get { return this.GetType().Equals(typeof(VarianceSwapTransactionSupplement)); } }
        bool IProductBase.IsSwap { get { return this.GetType().Equals(typeof(Swap)); } }
        bool IProductBase.IsSwaption { get { return this.GetType().Equals(typeof(Swaption)); } }
        bool IProductBase.IsSaleAndRepurchaseAgreement { get { return this.GetType().BaseType.Equals(typeof(SaleAndRepurchaseAgreement)); } }
        bool IProductBase.IsRepo { get { return this.GetType().Equals(typeof(Repo)); } }
        bool IProductBase.IsSecurityLending { get { return this.GetType().Equals(typeof(SecurityLending)); } }
        bool IProductBase.IsBuyAndSellBack { get { return this.GetType().Equals(typeof(BuyAndSellBack)); } }
        bool IProductBase.IsBondTransaction { get { return this.GetType().Equals(typeof(BondTransaction)); } }
        bool IProductBase.IsMarginRequirement { get { return this.GetType().Equals(typeof(MarginRequirement)); } }
        bool IProductBase.IsCashBalance { get { return this.GetType().Equals(typeof(CashBalance)); } }
        bool IProductBase.IsCashBalanceInterest { get { return this.GetType().Equals(typeof(CashBalanceInterest)); } }
        // EG 20131118 New
        bool IProductBase.IsCashPayment
        {
            get
            {
                return this.GetType().Equals(typeof(BulletPayment)) && (((ISpheresIdScheme)((IProductBase)this).ProductType).Value == "cashPayment");
            }
        }
        //
        bool IProductBase.IsADM
        {
            get
            {
                return ((IProductBase)this).IsInvoice || ((IProductBase)this).IsAdditionalInvoice ||
                       ((IProductBase)this).IsCreditNote || ((IProductBase)this).IsInvoiceSettlement;
            }
        }
        /// <summary>
        /// Pour l'instant Il n'existe que DebtSecurity (représentatif d'un titre)
        /// </summary>
        bool IProductBase.IsASSET
        {
            get
            {
                return ((IProductBase)this).IsDebtSecurity;
            }
        }
        bool IProductBase.IsEQD
        {
            get
            {
                return ((IProductBase)this).IsBrokerEquityOption || ((IProductBase)this).IsEquityForward ||
                       ((IProductBase)this).IsEquityOption || ((IProductBase)this).IsEquityOptionTransactionSupplement;
            }
        }
        /// <summary>
        /// Obtient true si IsFxAverageRateOption ou IsFxBarrierOption ou IsFxDigitalOption ou IsFxOptionLeg
        /// </summary>
        bool IProductBase.IsFxOption
        {
            get
            {
                return ((IProductBase)this).IsFxAverageRateOption || ((IProductBase)this).IsFxBarrierOption ||
                       ((IProductBase)this).IsFxDigitalOption || ((IProductBase)this).IsFxOptionLeg;
            }
        }
        /// <summary>
        /// Obtient true si IsFxOption ou IsFxSwap ou IsFxLeg
        /// </summary>
        bool IProductBase.IsFx
        {
            get
            {
                return ((IProductBase)this).IsFxOption || ((IProductBase)this).IsFxSwap || ((IProductBase)this).IsFxLeg;
            }
        }
        /// <summary>
        /// Obtient true si IsBulletPayment ou IsCapFloor ou IsFra ou IsSwap ou IsSwaption
        /// </summary>
        bool IProductBase.IsIRD
        {
            get
            {
                return ((IProductBase)this).IsBulletPayment || ((IProductBase)this).IsCapFloor ||
                       ((IProductBase)this).IsFra || ((IProductBase)this).IsSwap || ((IProductBase)this).IsSwaption ||
                       ((IProductBase)this).IsLoanDeposit;
            }
        }
        /// <summary>
        /// Obtient true si le produit représente un titre de créance ou une négociation  sur titres de créance
        /// </summary>
        bool IProductBase.IsSEC
        {
            get
            {
                return ((IProductBase)this).IsDebtSecurity ||
                       ((IProductBase)this).IsDebtSecurityTransaction ||
                       ((IProductBase)this).IsSaleAndRepurchaseAgreement;
            }
        }
        /// <summary>
        /// Retourne true si le produit repésente une négociation sur titre de créance 
        /// </summary>
        bool IProductBase.IsSecurityTransaction
        {
            get
            {
                return (((IProductBase)this).IsSEC & (false == ((IProductBase)this).IsASSET));
            }
        }
        bool IProductBase.IsLSD
        {
            get
            {
                return ((IProductBase)this).IsExchangeTradedDerivative;
            }
        }
        /// <summary>
        ///  Equivalent à IsEquitySecurityTransaction  
        /// </summary>
        bool IProductBase.IsESE
        {
            get
            {
                return ((IProductBase)this).IsEquitySecurityTransaction;
            }
        }
        /// <summary>
        /// Retourne true 
        /// </summary>
        bool IProductBase.IsDSE
        {
            get
            {
                return ((IProductBase)this).IsSaleAndRepurchaseAgreement || ((IProductBase)this).IsCapFloor ||
                       ((IProductBase)this).IsRepo || ((IProductBase)this).IsSecurityLending || ((IProductBase)this).IsBuyAndSellBack ||
                       ((IProductBase)this).IsDebtSecurity || ((IProductBase)this).IsDebtSecurityTransaction;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20171204 [23509] Add IsCashPayment
        bool IProductBase.IsRISK
        {
            get
            {
                return ((IProductBase)this).IsMarginRequirement ||
                    ((IProductBase)this).IsCashBalance ||
                    ((IProductBase)this).IsCashPayment ||
                    ((IProductBase)this).IsCashBalanceInterest;

            }
        }
        // EG 20150410 [20513] BANCAPERTA
        bool IProductBase.IsBondOption
        {
            get
            {
                return this.GetType().Equals(typeof(BondOption.BondOption));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20150317 [POC] New
        /// FI 20170116 [21916] Modify
        bool IProductBase.IsFungible(string pCS)
        {
            ProductContainer _product = new ProductContainer((IProduct)this);
            _ = _product.GetInstrument(pCS, out SQL_Instrument _sql_Instrument);
            return _sql_Instrument.IsFungible;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20150317 [POC] New
        bool IProductBase.IsMargining(string pCS)
        {
            ProductContainer _product = new ProductContainer((IProduct)this);
            _ = _product.GetInstrument(pCS, out SQL_Instrument _sql_Instrument);
            return _sql_Instrument.IsMargining;
        }
        /// EG 20150317 [POC] New
        /// FI 20170116 [21916] Modify
        bool IProductBase.IsMarginingAndNotFungible(string pCS)
        {
            ProductContainer _product = new ProductContainer((IProduct)this);
            _ = _product.GetInstrument(pCS, out SQL_Instrument _sql_Instrument);
            return (_sql_Instrument.IsMargining) && (!_sql_Instrument.IsFungible);
        }
        /// <summary>
        /// Obtient le nom du System.Type de l'instance actuelle
        /// </summary>
        string IProductBase.ProductName
        {
            get { return this.GetType().Name; }
        }
        /// <summary>
        /// 
        /// </summary>
        IProductType IProductBase.ProductType
        {
            get
            {
                IProductType ret = null;
                if (ArrFunc.IsFilled(productType))
                    ret = (IProductType)this.productType[0];
                return ret;
            }
            set { this.productType = new ProductType[1] { (ProductType)value }; }
        }
        /// <summary>
        /// 
        /// </summary>
        string IProductBase.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        Type IProductBase.TypeofStream { get { return new InterestRateStream().GetType(); } }
        Type IProductBase.TypeofLoadDepositStream { get { return new LoanDepositStream().GetType(); } }
        Type IProductBase.TypeofFxOptionPremium { get { return new FxOptionPremium().GetType(); } }
        Type IProductBase.TypeofPayment { get { return new Payment().GetType(); } }
        Type IProductBase.TypeofPaymentDates { get { return new PaymentDates().GetType(); } }
        Type IProductBase.TypeofSchedule { get { return new Schedule().GetType(); } }
        Type IProductBase.TypeofEquityPremium { get { return new EquityPremium().GetType(); } }
        Type IProductBase.TypeofReturnLeg { get { return new ReturnLeg().GetType(); } }
        Type IProductBase.TypeofInterestLeg { get { return new InterestLeg().GetType(); } }
        Type IProductBase.TypeofCurrency { get { return new Currency().GetType(); } }
        Type IProductBase.TypeofBusinessCenter { get { return new BusinessCenter().GetType(); } }
        Type IProductBase.TypeofPhysicalLeg { get { return typeof(PhysicalSwapLeg); } }
        Type IProductBase.TypeofGasPhysicalLeg { get { return new GasPhysicalLeg().GetType(); } }
        Type IProductBase.TypeofElectricityPhysicalLeg { get { return new ElectricityPhysicalLeg().GetType(); } }
        Type IProductBase.TypeofFinancialLeg { get { return typeof(FinancialSwapLeg); } }
        Type IProductBase.TypeofFixedPriceSpotLeg { get { return new FixedPriceSpotLeg().GetType(); } }
        Type IProductBase.TypeofFixedPriceLeg { get { return new FixedPriceLeg().GetType(); } }
        Type IProductBase.TypeofEnvironmentalPhysicalLeg { get { return new EnvironmentalPhysicalLeg().GetType(); } }


        // EG 20180514 [23812] Report
        IImplicitProvision IProductBase.ImplicitProvision
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).implicitProvision;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).implicitProvision;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).implicitProvision;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        return ((FxAverageRateOption)productBase).implicitProvision;
                    else if (productBase.IsFxBarrierOption)
                        return ((FxBarrierOption)productBase).implicitProvision;
                    else if (productBase.IsFxDigitalOption)
                        return ((FxDigitalOption)productBase).implicitProvision;
                    else if (productBase.IsFxOptionLeg)
                        return ((FxOptionLeg)productBase).implicitProvision;
                }
                return null;

            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                ImplicitProvision implicitProvision = (ImplicitProvision)value;
                if (productBase.IsSwap)
                    ((Swap)productBase).implicitProvision = implicitProvision;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).implicitProvision = implicitProvision;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).implicitProvision = implicitProvision;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        ((FxAverageRateOption)productBase).implicitProvision = implicitProvision;
                    else if (productBase.IsFxBarrierOption)
                        ((FxBarrierOption)productBase).implicitProvision = implicitProvision;
                    else if (productBase.IsFxDigitalOption)
                        ((FxDigitalOption)productBase).implicitProvision = implicitProvision;
                    else if (productBase.IsFxOptionLeg)
                        ((FxOptionLeg)productBase).implicitProvision = implicitProvision;
                }
            }
        }
        // EG 20180514 [23812] Report
        bool IProductBase.ImplicitProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).implicitProvisionSpecified;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).implicitProvisionSpecified;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).implicitProvisionSpecified;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        return ((FxAverageRateOption)productBase).implicitProvisionSpecified;
                    else if (productBase.IsFxBarrierOption)
                        return ((FxBarrierOption)productBase).implicitProvisionSpecified;
                    else if (productBase.IsFxDigitalOption)
                        return ((FxDigitalOption)productBase).implicitProvisionSpecified;
                    else if (productBase.IsFxOptionLeg)
                        return ((FxOptionLeg)productBase).implicitProvisionSpecified;
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).implicitProvisionSpecified = value;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).implicitProvisionSpecified = value;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).implicitProvisionSpecified = value;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        ((FxAverageRateOption)productBase).implicitProvisionSpecified = value;
                    else if (productBase.IsFxBarrierOption)
                        ((FxBarrierOption)productBase).implicitProvisionSpecified = value;
                    else if (productBase.IsFxDigitalOption)
                        ((FxDigitalOption)productBase).implicitProvisionSpecified = value;
                    else if (productBase.IsFxOptionLeg)
                        ((FxOptionLeg)productBase).implicitProvisionSpecified = value;
                }

            }
        }
        bool IProductBase.ImplicitEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.ImplicitProvisionSpecified &&
                       (productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified ||
                        productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified);
            }
        }
        bool IProductBase.ImplicitCancelableProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.ImplicitProvisionSpecified &&
                       productBase.ImplicitProvision.CancelableProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.ImplicitProvisionSpecified)
                    {
                        productBase.ImplicitProvisionSpecified = value;
                        productBase.ImplicitProvision = new ImplicitProvision();
                    }
                    productBase.ImplicitProvision.CancelableProvisionSpecified = value;
                    productBase.ImplicitProvision.CancelableProvision = new Empty();
                }
                else if (productBase.ImplicitProvisionSpecified)
                {
                    productBase.ImplicitProvision.CancelableProvisionSpecified = value;
                    productBase.ImplicitProvisionSpecified = productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.ExtendibleProvisionSpecified ||
                                                             productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.StepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.ImplicitExtendibleProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.ImplicitProvisionSpecified &&
                       productBase.ImplicitProvision.ExtendibleProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.ImplicitProvisionSpecified)
                    {
                        productBase.ImplicitProvisionSpecified = value;
                        productBase.ImplicitProvision = new ImplicitProvision();
                    }
                    productBase.ImplicitProvision.ExtendibleProvisionSpecified = value;
                    productBase.ImplicitProvision.ExtendibleProvision = new Empty();
                }
                else if (productBase.ImplicitProvisionSpecified)
                {
                    productBase.ImplicitProvision.ExtendibleProvisionSpecified = value;
                    productBase.ImplicitProvisionSpecified = productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.CancelableProvisionSpecified ||
                                                             productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.StepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.ImplicitOptionalEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.ImplicitProvisionSpecified &&
                       productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.ImplicitProvisionSpecified)
                    {
                        productBase.ImplicitProvisionSpecified = value;
                        productBase.ImplicitProvision = new ImplicitProvision();
                    }
                    productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified = value;
                    productBase.ImplicitProvision.OptionalEarlyTerminationProvision = new Empty();
                }
                else if (productBase.ImplicitProvisionSpecified)
                {
                    productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified = value;
                    productBase.ImplicitProvisionSpecified = productBase.ImplicitProvision.CancelableProvisionSpecified ||
                                                             productBase.ImplicitProvision.ExtendibleProvisionSpecified ||
                                                             productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.StepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.ImplicitMandatoryEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.ImplicitProvisionSpecified &&
                       productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.ImplicitProvisionSpecified)
                    {
                        productBase.ImplicitProvisionSpecified = value;
                        productBase.ImplicitProvision = new ImplicitProvision();
                    }
                    productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified = value;
                    productBase.ImplicitProvision.MandatoryEarlyTerminationProvision = new Empty();
                }
                else if (productBase.ImplicitProvisionSpecified)
                {
                    productBase.ImplicitProvision.MandatoryEarlyTerminationProvisionSpecified = value;
                    productBase.ImplicitProvisionSpecified = productBase.ImplicitProvision.CancelableProvisionSpecified ||
                                                             productBase.ImplicitProvision.ExtendibleProvisionSpecified ||
                                                             productBase.ImplicitProvision.OptionalEarlyTerminationProvisionSpecified ||
                                                             productBase.ImplicitProvision.StepUpProvisionSpecified;
                }
            }
        }

        // EG 20180514 [23812] Report
        bool IProductBase.EarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).earlyTerminationProvisionSpecified;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).earlyTerminationProvisionSpecified;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).earlyTerminationProvisionSpecified;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        return ((FxAverageRateOption)productBase).earlyTerminationProvisionSpecified;
                    else if (productBase.IsFxBarrierOption)
                        return ((FxBarrierOption)productBase).earlyTerminationProvisionSpecified;
                    else if (productBase.IsFxDigitalOption)
                        return ((FxDigitalOption)productBase).earlyTerminationProvisionSpecified;
                    else if (productBase.IsFxOptionLeg)
                        return ((FxOptionLeg)productBase).earlyTerminationProvisionSpecified;
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).earlyTerminationProvisionSpecified = value;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).earlyTerminationProvisionSpecified = value;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).earlyTerminationProvisionSpecified = value;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        ((FxAverageRateOption)productBase).earlyTerminationProvisionSpecified = value;
                    else if (productBase.IsFxBarrierOption)
                        ((FxBarrierOption)productBase).earlyTerminationProvisionSpecified = value;
                    else if (productBase.IsFxDigitalOption)
                        ((FxDigitalOption)productBase).earlyTerminationProvisionSpecified = value;
                    else if (productBase.IsFxOptionLeg)
                        ((FxOptionLeg)productBase).earlyTerminationProvisionSpecified = value;
                }
            }
        }
        // EG 20180514 [23812] Report
        IEarlyTerminationProvision IProductBase.EarlyTerminationProvision
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).earlyTerminationProvision;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).earlyTerminationProvision;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).earlyTerminationProvision;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        return ((FxAverageRateOption)productBase).earlyTerminationProvision;
                    else if (productBase.IsFxBarrierOption)
                        return ((FxBarrierOption)productBase).earlyTerminationProvision;
                    else if (productBase.IsFxDigitalOption)
                        return ((FxDigitalOption)productBase).earlyTerminationProvision;
                    else if (productBase.IsFxOptionLeg)
                        return ((FxOptionLeg)productBase).earlyTerminationProvision;
                }
                return null;
            }
            set
            {
                EarlyTerminationProvision earlyTerminationProvision = (EarlyTerminationProvision)value;
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).earlyTerminationProvision = earlyTerminationProvision;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).earlyTerminationProvision = earlyTerminationProvision;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).earlyTerminationProvision = earlyTerminationProvision;
                else if (productBase.IsFxOption)
                {
                    if (productBase.IsFxAverageRateOption)
                        ((FxAverageRateOption)productBase).earlyTerminationProvision = earlyTerminationProvision;
                    else if (productBase.IsFxBarrierOption)
                        ((FxBarrierOption)productBase).earlyTerminationProvision = earlyTerminationProvision;
                    else if (productBase.IsFxDigitalOption)
                        ((FxDigitalOption)productBase).earlyTerminationProvision = earlyTerminationProvision;
                    else if (productBase.IsFxOptionLeg)
                        ((FxOptionLeg)productBase).earlyTerminationProvision = earlyTerminationProvision;
                }
            }
        }
        bool IProductBase.CancelableProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).cancelableProvisionSpecified;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).cancelableProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).cancelableProvisionSpecified = value;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).cancelableProvisionSpecified = value;
            }

        }
        ICancelableProvision IProductBase.CancelableProvision
        {
            set
            {
                CancelableProvision cancelableProvision = (CancelableProvision)value;
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).cancelableProvision = cancelableProvision;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).cancelableProvision = cancelableProvision;
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).cancelableProvision;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).cancelableProvision;
                return null;
            }
        }
        bool IProductBase.ExtendibleProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).extendibleProvisionSpecified;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).extendibleProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).extendibleProvisionSpecified = value;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).extendibleProvisionSpecified = value;
            }

        }
        IExtendibleProvision IProductBase.ExtendibleProvision
        {
            set
            {
                ExtendibleProvision extendibleProvision = (ExtendibleProvision)value;
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).extendibleProvision = extendibleProvision;
                else if (productBase.IsLoanDeposit)
                    ((LoanDeposit)productBase).extendibleProvision = extendibleProvision;
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).extendibleProvision;
                else if (productBase.IsLoanDeposit)
                    return ((LoanDeposit)productBase).extendibleProvision;
                return null;
            }
        }
        // EG 20180514 [23812] Report
        bool IProductBase.OptionalEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                    else if (productBase.IsLoanDeposit)
                        return ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                    else if (productBase.IsFxOption)
                    {
                        if (productBase.IsFxAverageRateOption)
                            return ((FxAverageRateOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                        else if (productBase.IsFxBarrierOption)
                            return ((FxBarrierOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                        else if (productBase.IsFxDigitalOption)
                            return ((FxDigitalOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                        else if (productBase.IsFxOptionLeg)
                            return ((FxOptionLeg)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                    }
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                    else if (productBase.IsLoanDeposit)
                        ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                    else if (productBase.IsFxOption)
                    {
                        if (productBase.IsFxAverageRateOption)
                            ((FxAverageRateOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                        else if (productBase.IsFxBarrierOption)
                            ((FxBarrierOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                        else if (productBase.IsFxDigitalOption)
                            ((FxDigitalOption)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                        else if (productBase.IsFxOptionLeg)
                            ((FxOptionLeg)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                    }
                }
            }
        }
        // EG 20180514 [23812] Report
        IOptionalEarlyTermination IProductBase.OptionalEarlyTerminationProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    OptionalEarlyTermination optionalEarlyTermination = (OptionalEarlyTermination)value;
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                    else if (productBase.IsLoanDeposit)
                        ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                    else if (productBase.IsFxOption)
                    {
                        if (productBase.IsFxAverageRateOption)
                            ((FxAverageRateOption)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                        else if (productBase.IsFxBarrierOption)
                            ((FxBarrierOption)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                        else if (productBase.IsFxDigitalOption)
                            ((FxDigitalOption)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                        else if (productBase.IsFxOptionLeg)
                            ((FxOptionLeg)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                    }
                }
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.OptionalEarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptional;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptional;
                    else if (productBase.IsLoanDeposit)
                        return ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationOptional;
                    else if (productBase.IsFxOption)
                    {
                        if (productBase.IsFxAverageRateOption)
                            return ((FxAverageRateOption)productBase).earlyTerminationProvision.earlyTerminationOptional;
                        else if (productBase.IsFxBarrierOption)
                            return ((FxBarrierOption)productBase).earlyTerminationProvision.earlyTerminationOptional;
                        else if (productBase.IsFxDigitalOption)
                            return ((FxDigitalOption)productBase).earlyTerminationProvision.earlyTerminationOptional;
                        else if (productBase.IsFxOptionLeg)
                            return ((FxOptionLeg)productBase).earlyTerminationProvision.earlyTerminationOptional;
                    }
                }
                return null;
            }
        }
        bool IProductBase.MandatoryEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified;
                    else if (productBase.IsLoanDeposit)
                        return ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified;
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified = value;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified = value;
                    else if (productBase.IsLoanDeposit)
                        ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified = value;
                }
            }
        }
        IMandatoryEarlyTermination IProductBase.MandatoryEarlyTerminationProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.EarlyTerminationProvisionSpecified)
                {
                    MandatoryEarlyTermination mandatoryEarlyTermination = (MandatoryEarlyTermination)value;
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatory = mandatoryEarlyTermination;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatory = mandatoryEarlyTermination;
                    else if (productBase.IsLoanDeposit)
                        ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationMandatory = mandatoryEarlyTermination;
                }
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.MandatoryEarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatory;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatory;
                    else if (productBase.IsLoanDeposit)
                        return ((LoanDeposit)productBase).earlyTerminationProvision.earlyTerminationMandatory;
                }
                return null;
            }
        }

        IPartyRole IProductBase.CreatePartyRole() { return new PartyRole(); }
        ICurrency IProductBase.CreateCurrency(string pCurrency) { return new Currency(pCurrency); }
        IDebtSecurity IProductBase.CreateDebtSecurity() { return new DebtSecurity(); }
        IDebtSecurityStream[] IProductBase.CreateDebtSecurityStreams(int pDim)
        {
            DebtSecurityStream[] ret = new DebtSecurityStream[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new DebtSecurityStream();
            return ret;
        }
        IDebtSecurityTransaction IProductBase.CreateDebtSecurityTransaction() { return new DebtSecurityTransaction(); }
        IFxOptionLeg IProductBase.CreateFxOptionLeg() { return new FxOptionLeg(); }
        IExchangeTradedDerivative IProductBase.CreateExchangeTradedDerivative() { return new ExchangeTradedDerivative(); }
        IReturnSwap IProductBase.CreateReturnSwap() { return new ReturnSwap(); }
        IEquitySecurityTransaction IProductBase.CreateEquitySecurityTransaction() { return new EquitySecurityTransaction(); }
        IPayment IProductBase.CreatePayment() { return new Payment(); }
        IPriceUnits IProductBase.CreatePriceUnits() { return new PriceUnits(); }

        IMoney IProductBase.CreateMoney() { return new Money(); }
        IMoney IProductBase.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        IBusinessDayAdjustments IProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments
            {
                businessDayConvention = pBusinessDayConvention
            };
            if (ArrFunc.IsFilled(pIdBC))
            {
                BusinessCenters bcs = new BusinessCenters();
                foreach (string idBC in pIdBC)
                {
                    bcs.Add(new BusinessCenter(idBC));
                }
                if (0 < bcs.businessCenter.Length)
                {
                    bda.businessCentersDefineSpecified = true;
                    bda.businessCentersDefine = bcs;
                }
            }
            else
            {
                bda.businessCentersNoneSpecified = true;
                bda.businessCentersNone = new Empty();
            }
            return bda;
        }
        IOffset IProductBase.CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType) { return new Offset(pPeriod, pMultiplier, pDayType); }
        IAdjustableOffset IProductBase.CreateAdjustableOffset() { return new AdjustableOffset(); }
        IRelativeDateOffset IProductBase.CreateRelativeDateOffset() { return new RelativeDateOffset(); }
        IAdjustableDate IProductBase.CreateAdjustableDate(DateTime pDate, BusinessDayConventionEnum pBusinessDayConvention, IBusinessCenters pBusinessCenters)
        {
            AdjustableDate adjustableDate = new AdjustableDate
            {
                unadjustedDate = new IdentifiedDate(),
                dateAdjustments = new BusinessDayAdjustments(pBusinessDayConvention, pBusinessCenters)
            };
            adjustableDate.unadjustedDate.DateValue = pDate;
            return adjustableDate;
        }
        IRelativeDates IProductBase.CreateRelativeDates() { return new RelativeDates(); }
        IBusinessDateRange IProductBase.CreateBusinessDateRange() { return new BusinessDateRange(); }
        IInterval IProductBase.CreateInterval(PeriodEnum pPeriod, int pMultiplier) { return new Interval(pPeriod.ToString(), pMultiplier); }
        ICalculationPeriodFrequency IProductBase.CreateFrequency(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention)
        {
            return new CalculationPeriodFrequency(pPeriod, pPeriodMultiplier, pRollConvention);
        }
        IInterestRateStream[] IProductBase.CreateInterestRateStream(int pDim)
        {
            InterestRateStream[] ret = new InterestRateStream[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new InterestRateStream();
            return ret;
        }
        IInterval[] IProductBase.CreateIntervals() { return new Interval[1] { new Interval() }; }
        IFxAverageRateOption IProductBase.CreateFxAverageRateOption()
        {
            return new FxAverageRateOption();
        }
        IFxLeg[] IProductBase.CreateFxLegs(int pDim)
        {
            FxLeg[] ret = new FxLeg[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new FxLeg();
            return ret;
        }
        IInformationSource[] IProductBase.CreateInformationSources() { return new InformationSource[1] { new InformationSource() }; }
        IFxFixing IProductBase.CreateFxFixing() { return new FxFixing(); }
        IExpiryDateTime IProductBase.CreateExpiryDateTime() { return new ExpiryDateTime(); }
        IFxRate IProductBase.CreateFxRate() { return new FxRate(); }
        IFxStrikePrice IProductBase.CreateStrikePrice() { return new FxStrikePrice(); }
        IExerciseFee IProductBase.CreateExerciseFee() { return new ExerciseFee(); }
        IExerciseProcedure IProductBase.CreateExerciseProcedure() { return new ExerciseProcedure(); }
        ICashSettlement IProductBase.CreateCashSettlement() { return new CashSettlement(); }
        ICalculationAgent IProductBase.CreateCalculationAgent() { return new CalculationAgent(); }
        IReference IProductBase.CreatePartyOrAccountReference(string pReference)
        {
            PartyOrAccountReference reference = new PartyOrAccountReference
            {
                href = pReference
            };
            return reference;
        }
        IReference IProductBase.CreatePartyReference(string pReference)
        {
            PartyReference reference = new PartyReference
            {
                href = pReference
            };
            return reference;
        }

        IReference IProductBase.CreatePartyOrTradeSideReference(string pReference)
        {
            PartyOrTradeSideReference reference = new PartyOrTradeSideReference
            {
                href = pReference
            };
            return reference;
        }
        IReference[] IProductBase.CreateArrayPartyReference(string pPartyReference)
        {
            return new ArrayPartyReference[1] { new ArrayPartyReference(pPartyReference) };
        }
        IReference IProductBase.CreateBusinessCentersReference(string pReference)
        {
            BusinessCentersReference reference = new BusinessCentersReference
            {
                href = pReference
            };
            return reference;
        }
        ISimplePayment IProductBase.CreateSimplePayment() { return new SimplePayment(); }
        ISwap IProductBase.CreateSwap() { return new Swap(); }
        ISecurity IProductBase.CreateSecurity() { return new Security(); }
        ISecurityAsset IProductBase.CreateSecurityAsset() { return new SecurityAsset(); }
        ISecurityLeg IProductBase.CreateSecurityLeg() { return new SecurityLeg(); }
        ISecurityLeg[] IProductBase.CreateSecurityLegs(int pDim)
        {
            SecurityLeg[] ret = new SecurityLeg[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new SecurityLeg();
            return ret;
        }

        ISettlementMessagePartyPayment IProductBase.CreateSettlementMessagePartyPayment() { return new SettlementMessagePartyPayment(); }
        IRoutingCreateElement IProductBase.CreateRoutingCreateElement() { return new RoutingCreateElement(); }
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters IProductBase.LoadBusinessCenters(string pConnectionString, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM)
        {
            BusinessCenters businessCenters = new BusinessCenters();
            return ((IBusinessCenters)businessCenters).LoadBusinessCenters(pConnectionString, pDbTransaction, pIdA, pIdC, pIdM);
        }
        void IProductBase.SetProductType(string pId, string pIdentifier)
        {
            //Store IdI to OTCmlId and IDENTIFIER to productType
            productType = new ProductType[1] { new ProductType() };
            productType[0].productTypeScheme = Cst.OTCml_ProductTypeScheme;
            productType[0].otcmlId = pId;
            productType[0].Value = pIdentifier;
        }
        void IProductBase.SetId(int pInstrumentNo)
        {
            Id = Cst.FpML_InstrumentNo + pInstrumentNo.ToString();

        }
        IBusinessCenters IProductBase.CreateBusinessCenters(params string[] pIdBCs)
        {

            IBusinessCenters businessCenters = null;
            if (null != pIdBCs)
            {
                ArrayList aBusinessCenters = new ArrayList();
                foreach (string idBC in pIdBCs)
                {
                    if (StrFunc.IsFilled(idBC))
                        aBusinessCenters.Add(new BusinessCenter(idBC));
                }
                businessCenters = new BusinessCenters();
                businessCenters.BusinessCenter = (BusinessCenter[])aBusinessCenters.ToArray(typeof(BusinessCenter));
            }
            return businessCenters;

        }
        IAdjustedDate IProductBase.CreateAdjustedDate(DateTime pAdjustedDate)
        {
            IdentifiedDate adjustedDate = new IdentifiedDate
            {
                DateValue = pAdjustedDate
            };
            return adjustedDate;
        }
        IAdjustedDate IProductBase.CreateAdjustedDate(string pAdjustedDate)
        {
            IdentifiedDate adjustedDate = new IdentifiedDate
            {
                Value = pAdjustedDate
            };
            return adjustedDate;
        }
        ISettlementMessageDocument IProductBase.CreateSettlementMessageDocument()
        {
            return new SettlementMessageDocument();
        }
        INotificationDocument IProductBase.CreateConfirmationMessageDocument()
        {
            return new NotificationDocument();
        }
        ISpheresIdSchemeId[] IProductBase.CreateSpheresId(int pDim)
        {
            SpheresId[] spheresId = new SpheresId[pDim];
            for (int i = 0; i < pDim; i++)
            {
                spheresId[i] = new SpheresId();
            }
            return spheresId;
        }
        ITrader IProductBase.CreateTrader()
        {
            Trader trader = new Trader
            {
                traderScheme = Cst.OTCml_ActorIdentifierScheme
            };
            return trader;
        }
        IImplicitProvision IProductBase.CreateImplicitProvision()
        {
            return new ImplicitProvision();
        }
        IEmpty IProductBase.CreateImplicitProvisionItem()
        {
            return new Empty();
        }
        ICancelableProvision IProductBase.CreateCancelableProvision()
        {
            CancelableProvision cancelableprovision = new CancelableProvision();
            return cancelableprovision;
        }
        IExtendibleProvision IProductBase.CreateExtendibleProvision()
        {
            ExtendibleProvision extendibleProvision = new ExtendibleProvision();
            return extendibleProvision;
        }
        IEarlyTerminationProvision IProductBase.CreateMandatoryEarlyTermination()
        {
            EarlyTerminationProvision earlyTerminationProvision = new EarlyTerminationProvision
            {
                earlyTerminationOptionalSpecified = false,
                earlyTerminationMandatorySpecified = true,
                earlyTerminationMandatory = new MandatoryEarlyTermination()
            };
            return earlyTerminationProvision;
        }

        // EG 20180514 [23812] Report
        IEarlyTerminationProvision IProductBase.CreateOptionalEarlyTermination()
        {
            EarlyTerminationProvision earlyTerminationProvision = new EarlyTerminationProvision
            {
                earlyTerminationMandatorySpecified = false,
                earlyTerminationOptionalSpecified = true,
                earlyTerminationOptional = new OptionalEarlyTermination()
            };
            return earlyTerminationProvision;
        }
        IBusinessCenterTime IProductBase.CreateBusinessCenterTime(DateTime pTime, string pBusinessCenter)
        {
            BusinessCenterTime businessCenterTime = new BusinessCenterTime
            {
                hourMinuteTime = new HourMinuteTime(DtFunc.DateTimeToString(pTime, DtFunc.FmtISOTime)),
                businessCenter = new BusinessCenter(pBusinessCenter)
            };
            return businessCenterTime;
        }
        IQuotedCurrencyPair IProductBase.CreateQuotedCurrencyPair(string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasis)
        {
            return new QuotedCurrencyPair(pIdC1, pIdC1, pQuoteBasis);
        }
        INetInvoiceAmounts IProductBase.CreateNetInvoiceAmounts()
        {
            NetInvoiceAmounts netInvoiceAmounts = new NetInvoiceAmounts();
            return netInvoiceAmounts;
        }
        INetInvoiceAmounts IProductBase.CreateNetInvoiceAmounts(decimal pAmount, string pAmountCurrency,
            decimal pIssueAmount, string pIssueAmountCurrency, decimal pAccountingAmount, string pAccountingAmountCurrency)
        {
            NetInvoiceAmounts netInvoiceAmounts = new NetInvoiceAmounts
            {
                amount = new Money(pAmount, pAmountCurrency),
                issueAmount = new Money(pIssueAmount, pIssueAmountCurrency),
                accountingAmount = new Money(pAccountingAmount, pAccountingAmountCurrency)
            };
            return netInvoiceAmounts;
        }
        ITradeIntention IProductBase.CreateTradeIntention()
        {
            return new TradeIntention();
        }
        IPosRequest IProductBase.CreatePosRequest()
        {
            return new PosRequest();
        }
        IPosRequest IProductBase.CreatePosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return new PosRequestGroupLevel(pRequestType, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness);
        }
        IPosRequestOption IProductBase.CreatePosRequestOption()
        {
            return new PosRequestOption();
        }
        IPosRequestPositionOption IProductBase.CreatePosRequestPositionOption()
        {
            return new PosRequestPositionOption();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestOption IProductBase.CreatePosRequestOption(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestOption(pRequestType, pRequestMode, pDtBusiness, pQty);
        }
        IPosRequestClearingEOD IProductBase.CreatePosRequestClearingEOD()
        {
            return new PosRequestClearingEOD();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingEOD IProductBase.CreatePosRequestClearingEOD(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestClearingEOD(pRequestMode, pDtBusiness, pQty);
        }
        IPosRequestClearingBLK IProductBase.CreatePosRequestClearingBLK()
        {
            return new PosRequestClearingBLK();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingBLK IProductBase.CreatePosRequestClearingBLK(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestClearingBLK(pRequestMode, pDtBusiness, pQty);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingSPEC IProductBase.CreatePosRequestClearingSPEC(SettlSessIDEnum pRequestMode, DateTime pDtBusiness,
            int pIdT, decimal pQty, IPosKeepingClearingTrade[] pTradesTarget)
        {
            return new PosRequestClearingSPEC(pRequestMode, pDtBusiness, pIdT, pQty, pTradesTarget);
        }
        IPosRequestClearingSPEC IProductBase.CreatePosRequestClearingSPEC()
        {
            return new PosRequestClearingSPEC();
        }
        IPosRequestCorrection IProductBase.CreatePosRequestCorrection()
        {
            return new PosRequestCorrection();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestCorrection IProductBase.CreatePosRequestCorrection(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestCorrection(pRequestMode, pDtBusiness, pQty);
        }
        IPosRequestTransfer IProductBase.CreatePosRequestTransfer()
        {
            return new PosRequestTransfer();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestTransfer IProductBase.CreatePosRequestTransfer(DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestTransfer(pDtBusiness, pQty);
        }
        IPosRequestRemoveAlloc IProductBase.CreatePosRequestRemoveAlloc()
        {
            return new PosRequestRemoveAlloc();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestRemoveAlloc IProductBase.CreatePosRequestRemoveAlloc(DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestRemoveAlloc(pDtBusiness, pQty);
        }
        IPosRequestSplit IProductBase.CreatePosRequestSplit()
        {
            return new PosRequestSplit();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestSplit IProductBase.CreatePosRequestSplit(DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestSplit(pDtBusiness, pQty);
        }
        /// EG 20130607 [18740] Add RemoveCAExecuted
        IPosRequestRemoveCAExecuted IProductBase.CreatePosRequestRemoveCAExecuted()
        {
            return new PosRequestRemoveCAExecuted();
        }
        IPosRequestRemoveCAExecuted IProductBase.CreatePosRequestRemoveCAExecuted(int pIdA_CssCustodian, int pIdM, int pIdCE, DateTime pDtBusiness, bool pIsCustodian)
        {
            return new PosRequestRemoveCAExecuted(pIdA_CssCustodian, pIdM, pIdCE, pDtBusiness, pIsCustodian);
        }
        IPosRequestDetCorrection IProductBase.CreatePosRequestDetCorrection()
        {
            return new PosRequestDetCorrection();
        }
        IPosRequestDetUnderlyer IProductBase.CreatePosRequestDetUnderlyer()
        {
            return new PosRequestDetUnderlyer();
        }
        IPosKeepingData IProductBase.CreatePosKeepingData()
        {
            return new PosKeepingData();
        }
        IPosKeepingKey IProductBase.CreatePosKeepingKey()
        {
            return new PosKeepingKey();
        }
        IPosKeepingClearingTrade IProductBase.CreatePosKeepingClearingTrade()
        {
            return new PosKeepingClearingTrade();
        }
        IPosKeepingMarket IProductBase.CreatePosKeepingMarket()
        {
            return new PosKeepingMarket();
        }
        /// EG 20150302 Add FxRateAsset test (CFD Forex)
        PosKeepingAsset IProductBase.CreatePosKeepingAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset)
        {
            PosKeepingAsset _posKeepingAsset = null;
            if (pUnderlyingAsset.HasValue)
            {
                switch (pUnderlyingAsset.Value)
                {
                    case Cst.UnderlyingAsset.Bond:
                    case Cst.UnderlyingAsset.ConvertibleBond:
                        _posKeepingAsset = new PosKeepingAsset_BOND();
                        break;
                    case Cst.UnderlyingAsset.EquityAsset:
                        _posKeepingAsset = new PosKeepingAsset_EQUITY();
                        break;
                    case Cst.UnderlyingAsset.ExchangeTradedContract:
                        _posKeepingAsset = new PosKeepingAsset_ETD();
                        break;
                    case Cst.UnderlyingAsset.Index:
                        _posKeepingAsset = new PosKeepingAsset_INDEX();
                        break;
                    case Cst.UnderlyingAsset.RateIndex:
                        _posKeepingAsset = new PosKeepingAsset_RATEINDEX();
                        break;
                    case Cst.UnderlyingAsset.FxRateAsset:
                        _posKeepingAsset = new PosKeepingAsset_FXRATE();
                        break;
                    case Cst.UnderlyingAsset.Commodity:
                        _posKeepingAsset = new PosKeepingAsset_COMS();
                        break;
                }
            }
            return _posKeepingAsset;
        }
        IPosRequestKeyIdentifier IProductBase.CreatePosRequestKeyIdentifier()
        {
            return new PosRequestKeyIdentifier();
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestEntry IProductBase.CreatePosRequestEntry(DateTime pDtBusiness, decimal pQty)
        {
            return new PosRequestEntry(pDtBusiness, pQty);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestUnclearing IProductBase.CreatePosRequestUnclearing(DateTime pDtMarket, int pIdPR, int pIdPADET, Cst.PosRequestTypeEnum pRequestType,
            decimal pQty, DateTime pDtBusiness, int pIdT_Closing, string pClosing_Identifier, decimal pClosingQty)
        {
            return new PosRequestUnclearing(pDtMarket, pQty, pIdPR, pIdPADET, pRequestType, pDtBusiness, pIdT_Closing, pClosing_Identifier, pClosingQty);
        }
        IPosRequestUpdateEntry IProductBase.CreatePosRequestUpdateEntry(SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return new PosRequestUpdateEntry(pRequestMode, pDtBusiness);
        }
        IPosRequestUnclearing IProductBase.CreatePosRequestUnclearing()
        {
            return new PosRequestUnclearing();
        }
        IPosRequestUpdateEntry IProductBase.CreatePosRequestUpdateEntry()
        {
            return new PosRequestUpdateEntry();
        }
        // PM 20130218 [18414]
        IPosRequestCascadingShifting IProductBase.CreatePosRequestCascadingShifting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return new PosRequestCascadingShifting(pRequestType, pRequestMode, pDtBusiness);
        }
        IPosRequestCascadingShifting IProductBase.CreatePosRequestCascadingShifting()
        {
            return new PosRequestCascadingShifting();
        }
        IPosRequestMaturityOffsetting IProductBase.CreatePosRequestMaturityOffsetting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return new PosRequestMaturityOffsetting(pRequestType, pRequestMode, pDtBusiness);
        }
        IPosRequestMaturityOffsetting IProductBase.CreatePosRequestMaturityOffsetting()
        {
            return new PosRequestMaturityOffsetting();
        }
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery IProductBase.CreatePosRequestPhysicalPeriodicDelivery(DateTime pDtBusiness)
        {
            return new PosRequestPhysicalPeriodicDelivery(pDtBusiness);
        }
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery IProductBase.CreatePosRequestPhysicalPeriodicDelivery()
        {
            return new PosRequestPhysicalPeriodicDelivery();
        }

        IPosRequestEOD IProductBase.CreatePosRequestEOD()
        {
            return new PosRequestEOD();
        }
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Add Cst.PosRequestTypeEnum pPosRequestType parameter)
        IPosRequestEOD IProductBase.CreatePosRequestEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian, Cst.PosRequestTypeEnum pPosRequestTypeEnum)
        {
            return new PosRequestEOD(pIdA_Entity, pIdA_CssCustodian, pDtBusiness, pIdEM, pIsCustodian, pPosRequestTypeEnum);
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD()
        {
            return new PosRequestREMOVEEOD();
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, bool pIsCustodian)
        {
            return new PosRequestREMOVEEOD(pIdA_Entity, pIdA_CssCustodian, pDtBusiness, pIsCustodian);
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
        {
            return new PosRequestREMOVEEOD(pIdA_Entity, pIdA_CssCustodian, pDtBusiness, pIdEM, pIsCustodian);
        }
        IPosRequestClosingDAY IProductBase.CreatePosRequestClosingDAY()
        {
            return new PosRequestClosingDAY();
        }
        // EG 20150313 [POC] Add pIdEM
        IPosRequestClosingDAY IProductBase.CreatePosRequestClosingDAY(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
        {
            return new PosRequestClosingDAY(pIdA_Entity, pIdA_CssCustodian, pDtBusiness, pIdEM, pIsCustodian);
        }
        IPosRequestClosingDayControl IProductBase.CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, DateTime pDtBusiness)
        {
            return new PosRequestClosingDayControl(pRequestType, pIdA_Entity, pIdA_Css, pIdA_Custodian, pDtBusiness);
        }
        IPosRequestClosingDayControl IProductBase.CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return new PosRequestClosingDayControl(pRequestType, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness);
        }
        IFixInstrument IProductBase.CreateFixInstrument()
        {
            return new InstrumentBlock();
        }

        IPosRequestPositionDocument IProductBase.CreatePosRequestPositionDocument()
        {
            return new PosRequestPositionDocument();
        }

        IPosRequestDetPositionOption IProductBase.CreatePosRequestDetPositionOption()
        {
            return new PosRequestDetPositionOption();
        }
        IPosRequestCorporateAction IProductBase.CreatePosRequestCorporateAction(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_CSS, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return new PosRequestCorporateAction(pRequestType, pIdA_Entity, pIdA_CSS, pIdA_Custodian, pIdEM, pDtBusiness);
        }
        IPosRequestCorporateAction IProductBase.CreatePosRequestCorporateAction()
        {
            return new PosRequestCorporateAction();
        }
        INettingInformationInput IProductBase.CreateNettingInformationInput() { return new NettingInformationInput(); }
        IParty IProductBase.CreateParty() { return new Party(); }
        IBookId IProductBase.CreateBookId() { return new BookId(); }
        IUnitQuantity IProductBase.CreateUnitQuantity() { return new UnitQuantity(); }
        // EG 20170206 [22787] New
        IPrevailingTime IProductBase.CreatePrevailingTime() { return new PrevailingTime(); }
        ICalculationPeriodFrequency IProductBase.CreateCalculationPeriodFrequency() { return new CalculationPeriodFrequency(); }
        // FI 20170928 [23452] add
        IPerson IProductBase.CreatePerson() { return new Person(); }
        // FI 20170928 [23452] add
        IRelatedPerson IProductBase.CreateRelatedPerson() { return new RelatedPerson(); }
        // FI 20170928 [23452] add
        IAlgorithm IProductBase.CreateAlgorithm() { return new Algorithm(); }
        // FI 20170928 [23452] add
        IRelatedParty IProductBase.CreateRelatedParty() { return new RelatedParty(); }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateTradeCategory() { return new TradeCategory(); }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateTradingWaiver() { return new TradingWaiver(); }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateOtcClassification() { return new OtcClassification(); }
        // EG 20171016 [23509] New
        IZonedDateTime IProductBase.CreateZonedDateTime() { return new ZonedDateTime(); }
        // EG 20171028 [23509] New
        ITradeProcessingTimestamps IProductBase.CreateTradeProcessingTimestamps() { return new TradeProcessingTimestamps(); }
        #endregion
    }
    #endregion Product
    #region ProductReference
    public partial class ProductReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion ProductReference
    #region ProductType
    /// <summary>
    /// 
    /// </summary>
    /// FI 20140218 [20275] ProductType hérite de IProductType 
    public partial class ProductType : IProductType
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ProductType";
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public ProductType() { }
        public ProductType(string pIdentifier, int pOTCmlId)
        {
            OTCmlId = pOTCmlId;
            productTypeScheme = " http://www.euro-finance-systems.fr/otcml/producttype";
            Value = pIdentifier;
        }
        #endregion Constructors
        #region ISpheresId Members
        string ISpheresId.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        #endregion ISpheresId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.productTypeScheme; }
            set { this.productTypeScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members
        
    }
    #endregion ProductType

    #region QuotedCurrencyPair
    public partial class QuotedCurrencyPair : ICloneable, IQuotedCurrencyPair
    {
        #region Constructors
        public QuotedCurrencyPair()
        {
            currency1 = new Currency();
            currency2 = new Currency();
            quoteBasis = new QuoteBasisEnum();
        }
        public QuotedCurrencyPair(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis)
        {
            currency1 = new Currency
            {
                Value = pCurrency1
            };
            currency2 = new Currency
            {
                Value = pCurrency2
            };
            quoteBasis = pQuoteBasis;
        }
        public QuotedCurrencyPair(string pCurrency1, string pCurrency2, string pQuoteBasis)
        {
            currency1 = new Currency
            {
                Value = pCurrency1
            };
            currency2 = new Currency
            {
                Value = pCurrency2
            };
            quoteBasis = (QuoteBasisEnum)StringToEnum.Parse(pQuoteBasis, QuoteBasisEnum.Currency1PerCurrency2);
        }
        #endregion Constructors

        #region ICloneable Members
        public object Clone()
        {
            QuotedCurrencyPair clone = new QuotedCurrencyPair
            {
                currency1 = (Currency)this.currency1.Clone(),
                currency2 = (Currency)this.currency2.Clone(),
                quoteBasis = this.quoteBasis
            };
            return clone;
        }
        #endregion ICloneable Members
        #region IQuotedCurrencyPair Members
        string IQuotedCurrencyPair.Currency1
        {
            set { this.currency1.Value = value; }
            get { return this.currency1.Value; }
        }
        string IQuotedCurrencyPair.Currency1Scheme
        {
            set { this.currency1.currencyScheme = value; }
            get { return this.currency1.currencyScheme; }
        }


        string IQuotedCurrencyPair.Currency2
        {
            set { this.currency2.Value = value; }
            get { return this.currency2.Value; }
        }
        string IQuotedCurrencyPair.Currency2Scheme
        {
            set { this.currency2.currencyScheme = value; }
            get { return this.currency2.currencyScheme; }
        }

        QuoteBasisEnum IQuotedCurrencyPair.QuoteBasis
        {
            set { this.quoteBasis = value; }
            get { return this.quoteBasis; }
        }
        #endregion IQuotedCurrencyPair Members
    }
    #endregion QuotedCurrencyPair

    #region RateReference
    public partial class RateReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion RateReference

    #region RateSourcePage
    public partial class RateSourcePage : ICloneable, IScheme
    {
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RateSourcePage clone = new RateSourcePage
            {
                rateSourcePageScheme = this.rateSourcePageScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.rateSourcePageScheme = value; }
            get { return this.rateSourcePageScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion RateSourcePage
    #region ReferenceAmount
    public partial class ReferenceAmount : IScheme
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.referenceAmountScheme = value; }
            get { return this.referenceAmountScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ReferenceAmount
    #region RelativeDateOffset
    public partial class RelativeDateOffset : IRelativeDateOffset, ICloneable
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
        #region DateRelativeToValue
        public string DateRelativeToValue
        {
            get
            {
                return dateRelativeTo.href.ToString();
            }
        }
        #endregion DateRelativeToValue
        #region GetAdjustments
        public BusinessDayAdjustments GetAdjustments
        {
            get
            {
                BusinessDayAdjustments businessDayAdjustments = new BusinessDayAdjustments
                {
                    businessCentersDefine = this.businessCentersDefine,
                    businessCentersDefineSpecified = this.businessCentersDefineSpecified,
                    businessCentersReference = this.businessCentersReference,
                    businessCentersReferenceSpecified = this.businessCentersReferenceSpecified,
                    businessCentersNone = this.businessCentersNone,
                    businessCentersNoneSpecified = this.businessCentersNoneSpecified,
                    businessDayConvention = this.businessDayConvention
                };
                return businessDayAdjustments;
            }
        }
        #endregion GetAdjustments
        #region GetOffset
        public Offset GetOffset
        {
            get
            {
                Offset offset = new Offset
                {
                    period = this.period,
                    periodMultiplier = this.periodMultiplier,
                    dayType = this.dayType,
                    dayTypeSpecified = this.dayTypeSpecified
                };
                return offset;
            }
        }
        #endregion GetOffset
        #endregion Accessors
        #region Constructors
        public RelativeDateOffset()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
            dayType = new DayTypeEnum();
            businessDayConvention = new BusinessDayConventionEnum();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
            businessCentersNone = new Empty();
            dateRelativeTo = new DateReference();
        }
        #endregion Constructors

        #region IRelativeDateOffset Members
        IBusinessDayAdjustments IRelativeDateOffset.GetAdjustments { get { return this.GetAdjustments; } }
        IOffset IRelativeDateOffset.GetOffset { get { return this.GetOffset; } }
        string IRelativeDateOffset.DateRelativeToValue
        {
            set { this.dateRelativeTo.href = value; }
            get { return this.dateRelativeTo.href; }
        }
        bool IRelativeDateOffset.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }

        bool IRelativeDateOffset.BusinessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }

        bool IRelativeDateOffset.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }

        IBusinessCenters IRelativeDateOffset.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        IReference IRelativeDateOffset.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        BusinessDayConventionEnum IRelativeDateOffset.BusinessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        #endregion IRelativeDateOffset Members
        #region IOffset Members
        bool IOffset.DayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IOffset.DayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        /// <summary>
        /// Retourne les business Centers associées au devises {pCurrencies}
        /// <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrencies">Devise au format ISO4217_ALPHA3</param>
        /// <returns></returns>
        /// FI 20131118 [19118] Add Commentaire pour présiner que la méthode peut retourner la valeur null
        // EG 20180307 [23769] Gestion dbTransaction
        IBusinessCenters IOffset.GetBusinessCentersCurrency(string pConnectionString, IDbTransaction pDbTransaction, params string[] pCurrencies)
        {
            return ((IOffset)GetOffset).GetBusinessCentersCurrency(pConnectionString, pDbTransaction, pCurrencies);
        }
        IBusinessDayAdjustments IOffset.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            return ((IOffset)GetOffset).CreateBusinessDayAdjustments(pBusinessDayConvention, pIdBC);
        }
        #endregion IOffset Members
        #region IInterval Members

        EFS_Integer IInterval.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }

        PeriodEnum IInterval.Period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod)
        {
            this.period = pPeriod;
            this.periodMultiplier = new EFS_Integer(pMultiplier);
            return ((IInterval)this);
            //return ((IInterval)this).GetInterval(pMultiplier,pPeriod);
        }

        IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision)
        {
            return ((IInterval)this).GetRounding(pRoundingDirection, pPrecision);
        }
        int IInterval.CompareTo(object obj)
        {
            return ((IInterval)this).CompareTo(obj);
        }
        #endregion IInterval Members

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RelativeDateOffset clone = new RelativeDateOffset
            {
                businessCentersNoneSpecified = businessCentersNoneSpecified,
                businessCentersNone = (Empty)businessCentersNone.Clone(),
                businessCentersDefineSpecified = businessCentersDefineSpecified,
                businessCentersDefine = (BusinessCenters)businessCentersDefine.Clone(),
                businessCentersReferenceSpecified = businessCentersReferenceSpecified,
                businessCentersReference = (BusinessCentersReference)businessCentersReference.Clone(),
                businessDayConvention = businessDayConvention,
                dateRelativeTo = dateRelativeTo,
                dayTypeSpecified = dayTypeSpecified,
                dayType = dayType,
                periodMultiplier = new EFS_Integer(periodMultiplier.IntValue),
                period = period
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

    }
    #endregion RelativeDateOffset
    #region RelativeDates
    public partial class RelativeDates : IRelativeDates
    {
        #region IRelativeDateOffset Membres
        IOffset IRelativeDates.GetOffset { get { return this.GetOffset; } }
        IBusinessDayAdjustments IRelativeDates.GetAdjustments { get { return this.GetAdjustments; } }
        bool IRelativeDates.ScheduleBoundsSpecified { get { return this.scheduleBoundsSpecified; } }
        DateTime IRelativeDates.ScheduleBoundsUnadjustedFirstDate { get { return this.scheduleBounds.unadjustedFirstDate.DateValue; } }
        DateTime IRelativeDates.ScheduleBoundsUnadjustedLastDate { get { return this.scheduleBounds.unadjustedLastDate.DateValue; } }
        bool IRelativeDates.PeriodSkipSpecified { get { return this.periodSkipSpecified; } }
        int IRelativeDates.PeriodSkip { get { return this.periodSkip.IntValue; } }
        #endregion IRelativeDateOffset Membres
    }
    #endregion RelativeDates
    #region RelativeDateSequence
    // EG 20140702 Upd Interface
    public partial class RelativeDateSequence : IRelativeDateSequence
    {
        #region Constructors
        public RelativeDateSequence()
        {
            businessCentersDefine = new BusinessCenters();
            businessCentersReference = new BusinessCentersReference();
        }
        #endregion Constructors
        #region Methods
        #region DateRelativeToValue
        public string DateRelativeToValue
        {
            get
            {
                return dateRelativeTo.href.ToString();
            }
        }
        #endregion DateRelativeToValue
        #region GetAdjustments
        public BusinessDayAdjustments GetAdjustments
        {
            get
            {
                BusinessDayAdjustments businessDayAdjustments = new BusinessDayAdjustments
                {
                    businessCentersDefine = this.businessCentersDefine,
                    businessCentersDefineSpecified = this.businessCentersDefineSpecified,
                    businessCentersReference = this.businessCentersReference,
                    businessCentersReferenceSpecified = this.businessCentersReferenceSpecified,
                    businessDayConvention = BusinessDayConventionEnum.NotApplicable
                };
                return businessDayAdjustments;
            }
        }
        #endregion GetAdjustments
        #region GetOffset
        public Offset GetOffset
        {
            get
            {
                Offset offset = new Offset();
                if (ArrFunc.IsFilled(this.dateOffset))
                {
                    offset.period = this.dateOffset[0].period;
                    offset.periodMultiplier = this.dateOffset[0].periodMultiplier;
                    offset.dayType = this.dateOffset[0].dayType;
                    offset.dayTypeSpecified = this.dateOffset[0].dayTypeSpecified;
                }
                return offset;
            }
        }
        #endregion GetOffset

        #endregion Methods
        #region IRelativeDateSequence Members
        IBusinessDayAdjustments IRelativeDateSequence.GetAdjustments { get { return this.GetAdjustments; } }
        IDateOffset[] IRelativeDateSequence.DateOffset 
        { 
            set { this.dateOffset = (DateOffset[]) value; } 
            get { return this.dateOffset; } 
        }

        string IRelativeDateSequence.DateRelativeToValue
        {
            set { this.dateRelativeTo.href = value; }
            get { return this.dateRelativeTo.href; }
        }
        bool IRelativeDateSequence.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }

        bool IRelativeDateSequence.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }

        IBusinessCenters IRelativeDateSequence.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        IReference IRelativeDateSequence.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        #endregion IRelativeDateSequence Members
    }
    #endregion RelativeDateSequence
    #region RequiredIdentifierDate
    public partial class RequiredIdentifierDate : ICloneable, IRequiredIdentifierDate
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.DateTime DateValue
        {
            get { return new DtFunc().StringToDateTime(this.Value, DtFunc.FmtISODate); }
            set { this.Value = DtFunc.DateTimeToString(value, DtFunc.FmtISODate); }
        }
        #endregion Accessors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RequiredIdentifierDate clone = new RequiredIdentifierDate
            {
                DateValue = this.DateValue
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IRequiredIdentifierDate Members
        DateTime IRequiredIdentifierDate.DateValue
        {
            set { this.DateValue = value; }
            get { return this.DateValue; }
        }
        string IRequiredIdentifierDate.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        string IRequiredIdentifierDate.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion IRequiredIdentifierDate Members
    }
    #endregion RequiredIdentifierDate
    #region ResetFrequency
    public partial class ResetFrequency : IResetFrequency
    {
        #region Constructors
        public ResetFrequency()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
            weeklyRollConvention = new WeeklyRollConventionEnum();
        }
        #endregion Constructors

        #region IResetFrequency Members
        IInterval IResetFrequency.Interval { get { return (IInterval)this; } }
        bool IResetFrequency.WeeklyRollConventionSpecified
        {
            set { this.weeklyRollConventionSpecified = value; }
            get { return this.weeklyRollConventionSpecified; }
        }
        WeeklyRollConventionEnum IResetFrequency.WeeklyRollConvention
        {
            set { this.weeklyRollConvention = value; }
            get { return this.weeklyRollConvention; }
        }
        #endregion IResetFrequency Members
    }
    #endregion ResetFrequency
    #region Rounding
    public partial class Rounding : IRounding
    {
        #region Constructors
        public Rounding() { }
        public Rounding(RoundingDirectionEnum pRoundingDirection, int pPrecision)
        {
            roundingDirection = pRoundingDirection;
            precision = new EFS_PosInteger
            {
                IntValue = pPrecision
            };
        }
        public Rounding(string pRoundingDirection, int pPrecision)
        {
            if (System.Enum.IsDefined(typeof(RoundingDirectionEnum), pRoundingDirection))
                roundingDirection = (RoundingDirectionEnum)System.Enum.Parse(typeof(RoundingDirectionEnum), pRoundingDirection, true);
            else
                roundingDirection = RoundingDirectionEnum.Nearest;
            precision = new EFS_PosInteger
            {
                IntValue = pPrecision
            };
        }
        #endregion Constructors

        #region IRounding Members
        RoundingDirectionEnum IRounding.RoundingDirection { get { return this.roundingDirection; } }
        int IRounding.Precision { get { return this.precision.IntValue; } }
        #endregion IRounding Members
    }
    #endregion Rounding
    #region Routing
    public partial class Routing : ICloneable, IRouting
    {
        #region Constructors
        public Routing()
        {
            routingIds = new RoutingIds();
            routingExplicitDetails = new RoutingExplicitDetails();
            routingIdsAndExplicitDetails = new RoutingIdsAndExplicitDetails();
        }
        #endregion Constructors
        #region Methods
        #region GetRoutingAccountNumber
        public string GetRoutingAccountNumber()
        {
            return SettlementTools.GetRoutingAccountNumber((IRouting)this);
        }
        #endregion GetRoutingAccountNumber
        #endregion Methods

        #region ICloneable Members
        public object Clone()
        {
            Routing clone = (Routing)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable Members
        #region IRouting Membres
        bool IRouting.RoutingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.RoutingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.RoutingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.RoutingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.RoutingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.RoutingIdsAndExplicitDetails
        {
            get { return this.routingIdsAndExplicitDetails; }
            set { this.routingIdsAndExplicitDetails = (RoutingIdsAndExplicitDetails)value; }
        }
        IRouting IRouting.Clone()
        {
            return (IRouting)this.Clone();
        }
        #endregion IRouting Membres
    }
    #endregion Routing
    #region RoutingExplicitDetails
    public partial class RoutingExplicitDetails : ICloneable, IRoutingExplicitDetails
    {
        #region Constructor
        public RoutingExplicitDetails() { }
        #endregion Constructor

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RoutingExplicitDetails clone = new RoutingExplicitDetails
            {
                routingAccountNumberSpecified = this.routingAccountNumberSpecified,
                routingName = new EFS_String(this.routingName.Value),
                routingReferenceTextSpecified = this.routingReferenceTextSpecified,
                routingAddressSpecified = this.routingAddressSpecified
            };
            if (clone.routingAccountNumberSpecified)
                clone.routingAccountNumber = new EFS_String(this.routingAccountNumber.Value);
            if (clone.routingAddressSpecified)
                clone.routingAddress = (Address)this.routingAddress.Clone();
            if (clone.routingReferenceTextSpecified)
            {
                clone.routingReferenceText = new EFS_StringArray[this.routingReferenceText.Length];
                for (int i = 0; i < this.routingReferenceText.Length; i++)
                    clone.routingReferenceText[i] = new EFS_StringArray(this.routingReferenceText[i].Value);
            }
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IRoutingExplicitDetails Members
        string IRoutingExplicitDetails.RoutingName { get { return this.routingName.Value; } }
        bool IRoutingExplicitDetails.RoutingAccountNumberSpecified { get { return this.routingAccountNumberSpecified; } }
        string IRoutingExplicitDetails.RoutingAccountNumber { get { return this.routingAccountNumber.Value; } }
        #endregion IRoutingExplicitDetails Members
    }
    #endregion RoutingExplicitDetails
    #region RoutingId
    public partial class RoutingId : IEFS_Array, IRoutingId
    {
        #region Constructors
        public RoutingId()
        {
            routingIdCodeScheme = "http://www.fpml.org/ext/iso9362";
            Value = string.Empty;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new SchemeText(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
        #region IRoutingId Members
        string IRoutingId.RoutingIdCodeScheme
        {
            set { this.routingIdCodeScheme = value; }
            get { return this.routingIdCodeScheme; }
        }
        string IRoutingId.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IRoutingId Members
    }
    #endregion RoutingId
    #region RoutingIds
    public partial class RoutingIds : IEFS_Array, ICloneable, IRoutingIds
    {
        #region Constructors
        public RoutingIds()
        {
            //routingId = null;
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RoutingIds clone = new RoutingIds
            {
                routingId = (RoutingId[])this.routingId.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        //
        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
        //
        #region IRoutingIds Members
        IRoutingId[] IRoutingIds.RoutingId
        {
            get { return this.routingId; }
            set { this.routingId = (RoutingId[])value; }
        }
        void IRoutingIds.SetRoutingId(ArrayList pRoutingId)
        {
            routingId = (RoutingId[])pRoutingId.ToArray(typeof(RoutingId));
        }
        #endregion IRoutingIds Members
    }
    #endregion RoutingIds
    #region RoutingIdsAndExplicitDetails
    public partial class RoutingIdsAndExplicitDetails : ICloneable, IRoutingIdsAndExplicitDetails
    {
        #region Constructor
        public RoutingIdsAndExplicitDetails() { }
        #endregion Constructor
        //
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RoutingIdsAndExplicitDetails clone = new RoutingIdsAndExplicitDetails
            {
                routingAccountNumberSpecified = this.routingAccountNumberSpecified,
                routingAddressSpecified = this.routingAddressSpecified,
                routingName = new EFS_String(this.routingName.Value),
                routingReferenceTextSpecified = this.routingReferenceTextSpecified,
                routingIds = (RoutingIds[])this.routingIds.Clone()
            };
            if (clone.routingAccountNumberSpecified)
                clone.routingAccountNumber = new EFS_String(this.routingAccountNumber.Value);
            if (clone.routingAddressSpecified)
                clone.routingAddress = (Address)this.routingAddress.Clone();
            if (clone.routingReferenceTextSpecified)
            {
                clone.routingReferenceText = new EFS_StringArray[this.routingReferenceText.Length];
                for (int i = 0; i < this.routingReferenceText.Length; i++)
                    clone.routingReferenceText[i] = new EFS_StringArray(this.routingReferenceText[i].Value);
            }
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        //
        #region IRoutingIdsAndExplicitDetails Members
        IRoutingIds[] IRoutingIdsAndExplicitDetails.RoutingIds
        {
            get { return this.routingIds; }
            set { routingIds = (RoutingIds[])value; }
        }
        EFS_String IRoutingIdsAndExplicitDetails.RoutingName
        {
            get { return this.routingName; }
            set { this.routingName = value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.RoutingAddressSpecified
        {
            get { return this.routingAddressSpecified; }
            set { this.routingAddressSpecified = value; }
        }
        IAddress IRoutingIdsAndExplicitDetails.RoutingAddress
        {
            get { return this.routingAddress; }
            set { routingAddress = (Address)value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.RoutingAccountNumberSpecified
        {
            get { return this.routingAccountNumberSpecified; }
            set { this.routingAccountNumberSpecified = value; }
        }
        EFS_String IRoutingIdsAndExplicitDetails.RoutingAccountNumber
        {
            get { return this.routingAccountNumber; }
            set { this.routingAccountNumber = value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.RoutingReferenceTextSpecified
        {
            get { return this.routingReferenceTextSpecified; }
            set { this.routingReferenceTextSpecified = value; }
        }
        EFS_StringArray[] IRoutingIdsAndExplicitDetails.RoutingReferenceText
        {
            get { return this.routingReferenceText; }
            set { this.routingReferenceText = value; }
        }
        //
        #endregion IRoutingIdsAndExplicitDetails Members
    }
    #endregion RoutingIdsAndExplicitDetails

    #region Schedule
    public partial class Schedule : ISchedule
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Step[] efs_Steps;
        #endregion Members
        #region Accessors
        #region IsStepCalculated
        public bool IsStepCalculated
        {
            get { return stepSpecified && (null != efs_Steps) && (0 < efs_Steps.Length); }
        }
        #endregion IsStepCalculated
        #region id
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
        #endregion id
        #endregion Accessors
        #region Constructors
        public Schedule()
        {
            step = new Step[] { new Step() };
            initialValue = new EFS_Decimal();
        }
        #endregion Constructors
        #region Methods
        #region CalcAdjustableSteps
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel CalcAdjustableSteps(string pConnectionString, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            if (stepSpecified && (0 < step.Length))
            {
                ArrayList aStep = new ArrayList();
                foreach (Step item in step)
                {
                    EFS_Step efs_Step = new EFS_Step(pConnectionString, item.stepDate.DateValue, item.stepValue.DecValue, pBusinessDayAdjustments, pDataDocument);
                    aStep.Add(efs_Step);
                }
                if (0 < aStep.Count)
                    efs_Steps = (EFS_Step[])aStep.ToArray(typeof(EFS_Step));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalcAdjustableSteps
        #region GetStepDatesValue
        public DateTime[] GetStepDatesValue()
        {
            ArrayList lst = new ArrayList();
            if (ArrFunc.IsFilled(step))
            {
                for (int i = 0; i < step.Length; i++)
                    lst.Add(step[i].stepDate.DateValue);
            }
            DateTime[] ret = (DateTime[])lst.ToArray(typeof(DateTime));
            return ret;
        }
        #endregion
        #endregion Methods

        #region ISchedule Members
        #region Accessors
        EFS_Decimal ISchedule.InitialValue
        {
            set { this.initialValue = value; }
            get { return this.initialValue; }
        }
        bool ISchedule.StepSpecified { get { return this.stepSpecified; } }
        IStep[] ISchedule.Step
        {
            get { return this.step; }
        }
        EFS_Step[] ISchedule.Efs_Steps { get { return this.efs_Steps; } }
        bool ISchedule.IsStepCalculated { get { return this.IsStepCalculated; } }
        DateTime[] ISchedule.GetStepDatesValue { get { return this.GetStepDatesValue(); } }
        string ISchedule.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion Accessors
        #region Methods
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel ISchedule.CalcAdjustableSteps(string pCs, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            return this.CalcAdjustableSteps(pCs, pBusinessDayAdjustments, pDataDocument);
        }
        #endregion Methods
        #endregion ISchedule Members
    }
    #endregion Schedule
    #region ScheduleReference
    public partial class ScheduleReference : IReference, IEFS_Array
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.Href = value; }
            get { return this.Href; }
        }
        #endregion IReference Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

    }
    #endregion ScheduleReference

    #region SettlementInformation
    public partial class SettlementInformation : ISettlementInformation
    {
        #region Constructors
        public SettlementInformation()
        {
            informationInstruction = new SettlementInstruction();
        }
        #endregion Constructors

        #region ISettlementInformation Members
        bool ISettlementInformation.StandardSpecified
        {
            set { this.informationStandardSpecified = value; }
            get { return this.informationStandardSpecified; }
        }
        StandardSettlementStyleEnum ISettlementInformation.Standard
        {
            set { this.informationStandard = value; }
            get { return this.informationStandard; }
        }
        bool ISettlementInformation.InstructionSpecified
        {
            set { this.informationInstructionSpecified = value; }
            get { return this.informationInstructionSpecified; }
        }
        ISettlementInstruction ISettlementInformation.Instruction
        {
            set { this.informationInstruction = (SettlementInstruction)value; }
            get { return this.informationInstruction; }
        }
        #endregion ISettlementInformation Members
    }
    #endregion SettlementInformation
    #region SettlementInstruction
    public partial class SettlementInstruction : ICloneable, ISettlementInstruction
    {
        #region Constructor
        public SettlementInstruction() { }
        #endregion Constructor

        #region ICloneable Members
        public object Clone()
        {
            SettlementInstruction clone = (SettlementInstruction)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable Members
        #region ISettlementInstruction Members
        bool ISettlementInstruction.SettlementMethodSpecified
        {
            get { return this.settlementMethodSpecified; }
        }
        IScheme ISettlementInstruction.SettlementMethod
        {
            get { return (IScheme)this.settlementMethod; }
        }
        bool ISettlementInstruction.CorrespondentInformationSpecified
        {
            get { return this.correspondentInformationSpecified; }
        }
        IRouting ISettlementInstruction.CorrespondentInformation
        {
            get { return this.correspondentInformation; }
        }
        bool ISettlementInstruction.IntermediaryInformationSpecified
        {
            get { return this.intermediaryInformationSpecified; }
        }
        IRouting[] ISettlementInstruction.IntermediaryInformation
        {
            get { return this.intermediaryInformation; }
        }
        IRouting ISettlementInstruction.Beneficiary
        {
            get { return this.beneficiary; }
        }
        IEfsSettlementInstruction ISettlementInstruction.CreateEfsSettlementInstruction()
        {
            return (IEfsSettlementInstruction)new EfsSettlementInformation();
        }
        IEfsSettlementInstruction[] ISettlementInstruction.CreateEfsSettlementInstructions()
        {
            return new EfsSettlementInstruction[] { new EfsSettlementInstruction() };
        }
        IEfsSettlementInstruction[] ISettlementInstruction.CreateEfsSettlementInstructions(IEfsSettlementInstruction pEfsSettlementInstruction)
        {
            return new EfsSettlementInstruction[] { (EfsSettlementInstruction)pEfsSettlementInstruction };
        }
        #endregion ISettlementInstruction Members
    }
    #endregion SettlementInstruction
    #region SettlementMethod
    public partial class SettlementMethod : ICloneable, IScheme
    {
        #region Constructors
        public SettlementMethod()
        {
            settlementMethodScheme = "http://www.fpml.org/coding-scheme/settlement-method-1-0";
        }
        #endregion Constructors
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            SettlementMethod ret = new SettlementMethod
            {
                settlementMethodScheme = settlementMethodScheme,
                Value = Value
            };
            return ret;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.settlementMethodScheme = value; }
            get { return this.settlementMethodScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

    }
    #endregion SettlementMethod
    #region SettlementPriceSource
    public partial class SettlementPriceSource : IScheme
    {
        #region Constructors
        public SettlementPriceSource()
        {
            settlementPriceSourceScheme = "http://www.fpml.org/coding-scheme/settlement-price-source-1-0";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.settlementPriceSourceScheme = value; }
            get { return this.settlementPriceSourceScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion SettlementPriceSource

    #region SharedAmericanExercise
    public partial class SharedAmericanExercise : ISharedAmericanExercise
    {
        #region ISharedAmericanExercise Members
        IAdjustableOrRelativeDate ISharedAmericanExercise.CommencementDate
        {
            set { this.commencementDate = (AdjustableOrRelativeDate)value; }
            get { return this.commencementDate; }
        }
        IAdjustableOrRelativeDate ISharedAmericanExercise.ExpirationDate
        {
            set { this.expirationDate = (AdjustableOrRelativeDate)value; }
            get { return this.expirationDate; }
        }
        bool ISharedAmericanExercise.LatestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime ISharedAmericanExercise.LatestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        #endregion
        #region IExerciseId Members
        string IExerciseId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion IExerciseBase Members
    }
    #endregion SharedAmericanExercise
    #region SimplePayment
    public partial class SimplePayment : ISimplePayment, IEFS_Array
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SimplePayment efs_simplePayment;

        #region Constructors
        public SimplePayment()
        {
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
            paymentAmount = new Money();
            paymentDate = new AdjustableOrRelativeAndAdjustedDate();
        }
        #endregion Constructors

        #region ISimplePayment Membres
        IReference ISimplePayment.PayerPartyReference
        {
            get { return this.payerPartyReference; }
            set { payerPartyReference = (PartyOrAccountReference)value; }
        }
        IReference ISimplePayment.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { receiverPartyReference = (PartyOrAccountReference)value; }
        }
        IAdjustableOrRelativeAndAdjustedDate ISimplePayment.PaymentDate
        {
            get { return paymentDate; }
            set { paymentDate = (AdjustableOrRelativeAndAdjustedDate)value; }
        }
        IMoney ISimplePayment.PaymentAmount
        {
            get { return this.paymentAmount; }
            set { this.paymentAmount = (Money)value; }
        }
        #endregion

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

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDoc"></param>
        public void SetEfsSimplePayment(string pCS, DataDocumentContainer pDoc)
        {
            efs_simplePayment = new EFS_SimplePayment(pCS, pDoc, this);
        }
        #endregion

    }
    #endregion

    #region SpreadSchedule
    public partial class SpreadSchedule : IEFS_Array, ISpreadSchedule
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

        #region ISpreadSchedule Members
        ISpreadScheduleType ISpreadSchedule.Type 
        { 
            get { return this.type; }
        }
        void ISpreadSchedule.CreateSpreadScheduleType(string pValue)
        {
            this.type = new SpreadScheduleType(pValue);
        }
        #endregion ISpreadSchedule Members
    }
    #endregion SpreadSchedule
    #region SpreadScheduleType
    public partial class SpreadScheduleType : ISpreadScheduleType
    {
        #region Constructors
        public SpreadScheduleType(string pValue)
        {
            spreadScheduleTypeScheme = "http://www.fpml.org/coding-scheme/spread-schedule-type-1-0";
            this.Value = pValue;
        }
        public SpreadScheduleType()
        {
            spreadScheduleTypeScheme = "http://www.fpml.org/coding-scheme/spread-schedule-type-1-0";
        }
        #endregion Constructors
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            SpreadScheduleType ret = new SpreadScheduleType
            {
                spreadScheduleTypeScheme = spreadScheduleTypeScheme,
                Value = Value
            };
            return ret;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region ISpreadScheduleType Members
        // EG 20150309 POC - BERKELEY New
        string IScheme.Value 
        {
            set { this.Value = value; }
            get { return this.Value; } 
        }
        string IScheme.Scheme
        {
            set { this.spreadScheduleTypeScheme = value; }
            get { return this.spreadScheduleTypeScheme; }
        }
        //string ISpreadScheduleType.Value { get { return this.Value; } }
        #endregion ISpreadScheduleType Members
    }
    #endregion SpreadScheduleType
    #region Step
    public partial class Step : IEFS_Array, IStep
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
        #region Constructors
        public Step()
        {
            stepDate = new EFS_Date();
            stepValue = new EFS_Decimal();
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

        #region IStep Members
        EFS_Date IStep.StepDate
        {
            get { return this.stepDate; }
        }
        EFS_Decimal IStep.StepValue
        {
            get { return this.stepValue; }
        }
        #endregion IStep Members
    }
    #endregion Step
    #region StreetAddress
    public partial class StreetAddress : IStreetAddress
    {
        #region IStreetAddress Members
        EFS_StringArray[] IStreetAddress.StreetLine
        {
            get { return this.streetLine; }
            set { this.streetLine = value; }
        }
        #endregion IStreetAddress Members
    }
    #endregion StreetAddress
    #region StrikeSchedule
    public partial class StrikeSchedule : IEFS_Array, IStrikeSchedule
    {
        #region Constructors
        public StrikeSchedule()
        {
            buyer = new IdentifiedPayerReceiver();
            seller = new IdentifiedPayerReceiver();
        }
        #endregion Constructors
        #region Methods
        #region _buyer
        public static object INIT_buyer(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _buyer
        #region _seller
        public static object INIT_seller(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _seller
        #endregion Methods

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IStrikeSchedule Members
        bool IStrikeSchedule.BuyerSpecified
        {
            set { this.buyerSpecified = value; }
            get { return this.buyerSpecified; }
        }
        PayerReceiverEnum IStrikeSchedule.Buyer
        {
            set { this.buyer.Value = value; }
            get { return this.buyer.Value; }
        }
        bool IStrikeSchedule.SellerSpecified
        {
            set { this.sellerSpecified = value; }
            get { return this.sellerSpecified; }
        }
        PayerReceiverEnum IStrikeSchedule.Seller
        {
            set { this.seller.Value = value; }
            get { return this.seller.Value; }
        }
        #endregion IStrikeSchedule Members

    }
    #endregion StrikeSchedule
    #region Stub (included StubValue)
    public partial class Stub : IStub
    {
        #region Constructors
        public Stub()
        {
            stubTypeFloatingRate = new FloatingRate[1] { new FloatingRate() };
            stubTypeFixedRate = new EFS_Decimal();
            stubTypeAmount = new Money();
        }
        #endregion Constructors

        #region IStub Members
        bool IStub.StubTypeFloatingRateSpecified
        {
            set { this.stubTypeFloatingRateSpecified = value; }
            get { return this.stubTypeFloatingRateSpecified; }
        }
        IFloatingRate[] IStub.StubTypeFloatingRate
        {
            set { this.stubTypeFloatingRate = (FloatingRate[])value; }
            get { return this.stubTypeFloatingRate; }
        }
        bool IStub.StubTypeFixedRateSpecified
        {
            set { this.stubTypeFixedRateSpecified = value; }
            get { return this.stubTypeFixedRateSpecified; }
        }
        EFS_Decimal IStub.StubTypeFixedRate
        {
            set { this.stubTypeFixedRate = value; }
            get { return this.stubTypeFixedRate; }
        }
        bool IStub.StubTypeAmountSpecified
        {
            set { this.stubTypeAmountSpecified = value; }
            get { return this.stubTypeAmountSpecified; }
        }
        IMoney IStub.StubTypeAmount
        {
            set { this.stubTypeAmount = (Money)value; }
            get { return this.stubTypeAmount; }
        }
        bool IStub.StubStartDateSpecified { get { return this.stubStartDateSpecified; } }
        IAdjustableOrRelativeDate IStub.StubStartDate { get { return this.stubStartDate; } }
        bool IStub.StubEndDateSpecified { get { return this.stubEndDateSpecified; } }
        IAdjustableOrRelativeDate IStub.StubEndDate { get { return this.stubEndDate; } }
        IFloatingRate[] IStub.CreateFloatingRate { get { return new FloatingRate[2] { new FloatingRate(), new FloatingRate() }; } }
        IMoney IStub.CreateMoney { get { return new Money(); } }
        #endregion IStub Members
    }
    #endregion Stub (included StubValue)

    // EG 20180514 [23812] Report
    // *************************************************
    // Contenu Provisions déplacée de IRD vers SHARED
    // pour partage avec les FXOptions
    // EG 20180103
    // *************************************************

    #region EarlyTerminationProvision
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext    
    public partial class EarlyTerminationProvision : IEarlyTerminationProvision
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
        #region Constructors
        public EarlyTerminationProvision()
        {
            earlyTerminationMandatory = new MandatoryEarlyTermination();
            mandatoryEarlyTerminationDateTenor = new Interval();
            earlyTerminationOptional = new OptionalEarlyTermination();
            optionalEarlyTerminationParameters = new ExercisePeriod();
        }
        #endregion Constructors

        #region IEarlyTerminationProvision Members
        bool IEarlyTerminationProvision.MandatorySpecified { get { return this.earlyTerminationMandatorySpecified; } }
        IMandatoryEarlyTermination IEarlyTerminationProvision.Mandatory { get { return this.earlyTerminationMandatory; } }
        bool IEarlyTerminationProvision.OptionalSpecified { get { return this.earlyTerminationOptionalSpecified; } }
        IOptionalEarlyTermination IEarlyTerminationProvision.Optional { get { return this.earlyTerminationOptional; } }
        #endregion IEarlyTerminationProvision Members
    }
    #endregion EarlyTerminationProvision

    #region CashPriceMethod
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class CashPriceMethod : ICashPriceMethod
    {
        #region Constructors
        public CashPriceMethod()
        {
            cashSettlementReferenceBanks = new CashSettlementReferenceBanks();
            //cashSettlementReferenceBanksSpecified = false;
            cashSettlementCurrency = new Currency();
        }
        #endregion Constructors
        #region ICashPriceMethod Members
        bool ICashPriceMethod.CashSettlementReferenceBanksSpecified
        {
            set { this.cashSettlementReferenceBanksSpecified = value; }
            get { return this.cashSettlementReferenceBanksSpecified; }
        }
        ICurrency ICashPriceMethod.CashSettlementCurrency
        {
            set { this.cashSettlementCurrency = (Currency)value; }
            get { return this.cashSettlementCurrency; }
        }
        QuotationRateTypeEnum ICashPriceMethod.QuotationRateType
        {
            set { this.quotationRateType = value; }
            get { return this.quotationRateType; }
        }
        #endregion ICashPriceMethod Members
    }
    #endregion CashPriceMethod

    #region CashSettlement
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class CashSettlement : ICashSettlement
    {
        #region Constructors
        public CashSettlement()
        {
            cashSettlementMethodcashPriceMethod = new CashPriceMethod();
            cashSettlementMethodcashPriceAlternateMethod = new CashPriceMethod();
            cashSettlementMethodparYieldCurveAdjustedMethod = new YieldCurveMethod();
            cashSettlementMethodparYieldCurveUnadjustedMethod = new YieldCurveMethod();
            cashSettlementMethodzeroCouponYieldAdjustedMethod = new YieldCurveMethod();
        }
        #endregion Constructors

        #region ICashSettlement Members
        IBusinessCenterTime ICashSettlement.ValuationTime
        {
            set { this.cashSettlementValuationTime = (BusinessCenterTime)value; }
            get { return this.cashSettlementValuationTime; }
        }
        IRelativeDateOffset ICashSettlement.ValuationDate
        {
            set { this.cashSettlementValuationDate = (RelativeDateOffset)value; }
            get { return this.cashSettlementValuationDate; }
        }
        bool ICashSettlement.PaymentDateSpecified
        {
            set { this.cashSettlementPaymentDateSpecified = value; }
            get { return this.cashSettlementPaymentDateSpecified; }
        }
        ICashSettlementPaymentDate ICashSettlement.PaymentDate
        {
            set { this.cashSettlementPaymentDate = (CashSettlementPaymentDate)value; }
            get { return this.cashSettlementPaymentDate; }
        }
        ICashSettlementPaymentDate ICashSettlement.CreatePaymentDate
        {
            get { return new CashSettlementPaymentDate(); }
        }
        ICashPriceMethod ICashSettlement.CreateCashPriceMethod(string pCurrency, QuotationRateTypeEnum pQuotationRateType)
        {
            CashPriceMethod cashPriceMethod = new CashPriceMethod
            {
                cashSettlementReferenceBanksSpecified = false,
                cashSettlementCurrency = new Currency(pCurrency),
                quotationRateType = pQuotationRateType
            };
            return cashPriceMethod;
        }
        string ICashSettlement.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        bool ICashSettlement.CashPriceMethodSpecified
        {
            set { this.cashSettlementMethodcashPriceMethodSpecified = value; }
            get { return this.cashSettlementMethodcashPriceMethodSpecified; }
        }
        ICashPriceMethod ICashSettlement.CashPriceMethod
        {
            set { this.cashSettlementMethodcashPriceMethod = (CashPriceMethod)value; }
            get { return this.cashSettlementMethodcashPriceMethod; }
        }
        bool ICashSettlement.CashPriceAlternateMethodSpecified
        {
            set { this.cashSettlementMethodcashPriceAlternateMethodSpecified = value; }
            get { return this.cashSettlementMethodcashPriceAlternateMethodSpecified; }
        }
        ICashPriceMethod ICashSettlement.CashPriceAlternateMethod
        {
            set { this.cashSettlementMethodcashPriceAlternateMethod = (CashPriceMethod)value; }
            get { return this.cashSettlementMethodcashPriceAlternateMethod; }
        }
        bool ICashSettlement.ParYieldCurveAdjustedMethodSpecified
        {
            set { this.cashSettlementMethodparYieldCurveAdjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodparYieldCurveAdjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.ParYieldCurveAdjustedMethod
        {
            set { this.cashSettlementMethodparYieldCurveAdjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodparYieldCurveAdjustedMethod; }
        }
        bool ICashSettlement.ZeroCouponYieldAdjustedMethodSpecified
        {
            set { this.cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.ZeroCouponYieldAdjustedMethod
        {
            set { this.cashSettlementMethodzeroCouponYieldAdjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodzeroCouponYieldAdjustedMethod; }
        }
        bool ICashSettlement.ParYieldCurveUnadjustedMethodSpecified
        {
            set { this.cashSettlementMethodparYieldCurveUnadjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodparYieldCurveUnadjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.ParYieldCurveUnadjustedMethod
        {
            set { this.cashSettlementMethodparYieldCurveUnadjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodparYieldCurveUnadjustedMethod; }
        }
        #endregion ICashSettlement Members
    }
    #endregion CashSettlement
    #region CashSettlementPaymentDate
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class CashSettlementPaymentDate : ICashSettlementPaymentDate
    {
        #region Constructors
        public CashSettlementPaymentDate()
        {
            paymentDateAdjustables = new AdjustableDates();
            paymentDateRelative = new RelativeDateOffset();
            paymentDateBusinessDateRange = new BusinessDateRange();
        }
        #endregion Constructors

        #region ICashSettlementPaymentDate Members
        bool ICashSettlementPaymentDate.AdjustableDatesSpecified
        {
            set { this.paymentDateAdjustablesSpecified = value; }
            get { return this.paymentDateAdjustablesSpecified; }
        }
        IAdjustableDates ICashSettlementPaymentDate.AdjustableDates
        {
            set { this.paymentDateAdjustables = (AdjustableDates)value; }
            get { return this.paymentDateAdjustables; }
        }
        bool ICashSettlementPaymentDate.BusinessDateRangeSpecified
        {
            set { this.paymentDateBusinessDateRangeSpecified = value; }
            get { return this.paymentDateBusinessDateRangeSpecified; }
        }
        IBusinessDateRange ICashSettlementPaymentDate.BusinessDateRange
        {
            set { this.paymentDateBusinessDateRange = (BusinessDateRange)value; }
            get { return this.paymentDateBusinessDateRange; }
        }
        bool ICashSettlementPaymentDate.RelativeDateSpecified
        {
            set { this.paymentDateRelativeSpecified = value; }
            get { return this.paymentDateRelativeSpecified; }
        }
        IRelativeDateOffset ICashSettlementPaymentDate.RelativeDate
        {
            set { this.paymentDateRelative = (RelativeDateOffset)value; }
            get { return this.paymentDateRelative; }
        }
        string ICashSettlementPaymentDate.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion ICashSettlementPaymentDate Members
    }
    #endregion CashSettlementPaymentDate

    #region MandatoryEarlyTermination
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class MandatoryEarlyTermination : IProvision, IMandatoryEarlyTermination
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_MandatoryEarlyTerminationDates efs_MandatoryEarlyTerminationDates;
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

        #region IProvision Members
        ExerciseStyleEnum IProvision.GetStyle { get { return ExerciseStyleEnum.European; } }
        ICashSettlement IProvision.CashSettlement
        {
            set { this.cashSettlement = (CashSettlement)value; }
            get { return this.cashSettlement; }
        }
        #endregion IProvision Members
        #region IMandatoryEarlyTermination Members
        IAdjustableDate IMandatoryEarlyTermination.MandatoryEarlyTerminationDate { get { return this.mandatoryEarlyTerminationDate; } }
        ICashSettlement IMandatoryEarlyTermination.CashSettlement { get { return this.cashSettlement; } }
        bool IMandatoryEarlyTermination.AdjustedDatesSpecified { get { return this.mandatoryEarlyTerminationAdjustedDatesSpecified; } }
        IMandatoryEarlyTerminationAdjustedDates IMandatoryEarlyTermination.AdjustedDates { get { return this.mandatoryEarlyTerminationAdjustedDates; } }
        EFS_MandatoryEarlyTerminationDates IMandatoryEarlyTermination.Efs_MandatoryEarlyTerminationDates
        {
            get { return this.efs_MandatoryEarlyTerminationDates; }
            set { this.efs_MandatoryEarlyTerminationDates = value; }
        }
        #endregion IMandatoryEarlyTermination Members


    }
    #endregion MandatoryEarlyTermination
    #region MandatoryEarlyTerminationAdjustedDates
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class MandatoryEarlyTerminationAdjustedDates : IMandatoryEarlyTerminationAdjustedDates
    {
        #region IMandatoryEarlyTerminationAdjustedDates Members
        DateTime IMandatoryEarlyTerminationAdjustedDates.AdjustedEarlyTerminationDate { get { return this.adjustedEarlyTerminationDate.DateValue; } }
        DateTime IMandatoryEarlyTerminationAdjustedDates.AdjustedCashSettlementPaymentDate { get { return this.adjustedCashSettlementPaymentDate.DateValue; } }
        DateTime IMandatoryEarlyTerminationAdjustedDates.AdjustedCashSettlementValuationDate { get { return this.adjustedCashSettlementValuationDate.DateValue; } }
        #endregion IMandatoryEarlyTerminationAdjustedDates Members
    }
    #endregion MandatoryEarlyTerminationAdjustedDates
    #region OptionalEarlyTermination
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class OptionalEarlyTermination : IOptionalEarlyTermination
    {
        #region Accessors
        #region EFS_Exercise
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object EFS_Exercise
        {
            get
            {
                if (optionalEarlyTerminationExerciseAmericanSpecified)
                    return optionalEarlyTerminationExerciseAmerican;
                else if (optionalEarlyTerminationExerciseBermudaSpecified)
                    return optionalEarlyTerminationExerciseBermuda;
                else if (optionalEarlyTerminationExerciseEuropeanSpecified)
                    return optionalEarlyTerminationExerciseEuropean;
                else
                    return null;
            }
        }
        #endregion EFS_Exercise
        #endregion Accessors
        #region Constructors
        public OptionalEarlyTermination()
        {
            optionalEarlyTerminationExerciseAmerican = new AmericanExercise();
            optionalEarlyTerminationExerciseBermuda = new BermudaExercise();
            optionalEarlyTerminationExerciseEuropean = new EuropeanExercise();
        }
        #endregion Constructors

        #region IProvision Members
        ExerciseStyleEnum IProvision.GetStyle
        {
            get
            {
                if (this.optionalEarlyTerminationExerciseAmericanSpecified)
                    return ExerciseStyleEnum.American;
                else if (this.optionalEarlyTerminationExerciseBermudaSpecified)
                    return ExerciseStyleEnum.Bermuda;

                return ExerciseStyleEnum.European;
            }
        }
        ICashSettlement IProvision.CashSettlement
        {
            set { this.cashSettlement = (CashSettlement)value; }
            get { return this.cashSettlement; }
        }
        #endregion IProvision Members
        #region IOptionalEarlyTermination Members
        ICalculationAgent IOptionalEarlyTermination.CalculationAgent
        {
            set { this.calculationAgent = (CalculationAgent)value; }
            get { return this.calculationAgent; }
        }
        EFS_ExerciseDates IOptionalEarlyTermination.Efs_ExerciseDates
        {
            get { return this.efs_ExerciseDates; }
            set { this.efs_ExerciseDates = value; }
        }
        #endregion IOptionalEarlyTermination Members
        #region IExerciseProvision Members
        bool IExerciseProvision.AmericanSpecified
        {
            get { return this.optionalEarlyTerminationExerciseAmericanSpecified; }
            set { this.optionalEarlyTerminationExerciseAmericanSpecified = value; }
        }
        IAmericanExercise IExerciseProvision.American
        {
            get { return this.optionalEarlyTerminationExerciseAmerican; }
            set { this.optionalEarlyTerminationExerciseAmerican = (AmericanExercise)value; }
        }
        bool IExerciseProvision.BermudaSpecified
        {
            get { return this.optionalEarlyTerminationExerciseBermudaSpecified; }
            set { this.optionalEarlyTerminationExerciseBermudaSpecified = value; }
        }
        IBermudaExercise IExerciseProvision.Bermuda
        {
            get { return this.optionalEarlyTerminationExerciseBermuda; }
            set { this.optionalEarlyTerminationExerciseBermuda = (BermudaExercise)value; }
        }
        bool IExerciseProvision.EuropeanSpecified
        {
            get { return this.optionalEarlyTerminationExerciseEuropeanSpecified; }
            set { this.optionalEarlyTerminationExerciseEuropeanSpecified = value; }
        }
        IEuropeanExercise IExerciseProvision.European
        {
            get { return this.optionalEarlyTerminationExerciseEuropean; }
            set { this.optionalEarlyTerminationExerciseEuropean = (EuropeanExercise)value; }
        }
        bool IExerciseProvision.ExerciseNoticeSpecified
        {
            get { return this.exerciseNoticeSpecified; }
            set { this.exerciseNoticeSpecified = value; }
        }
        IExerciseNotice[] IExerciseProvision.ExerciseNotice
        {
            get { return this.exerciseNotice; }
            set { this.exerciseNotice = (ExerciseNotice[])value; }
        }
        bool IExerciseProvision.FollowUpConfirmation
        {
            set { this.followUpConfirmation = new EFS_Boolean(value); }
            get { return this.followUpConfirmation.BoolValue; }
        }
        IAmericanExercise IExerciseProvision.CreateAmerican
        {
            get { return new AmericanExercise(); }
        }
        #endregion IExerciseProvision Members
    }
    #endregion OptionalEarlyTermination

    #region YieldCurveMethod
    // EG 20180514 [23812] Report En provenance de FpML_Ird_Ext
    public partial class YieldCurveMethod : IYieldCurveMethod
    {
        #region Constructors
        public YieldCurveMethod()
        {
            settlementRateSource = new SettlementRateSource();
        }
        #endregion Constructors
        #region IYieldCurveMethod Members
        bool IYieldCurveMethod.SettlementRateSourceSpecified
        {
            get { return this.settlementRateSourceSpecified; }
            set { this.settlementRateSourceSpecified = value; }
        }
        QuotationRateTypeEnum IYieldCurveMethod.QuotationRateType
        {
            get { return this.quotationRateType; }
            set { this.quotationRateType = value; }
        }
        #endregion IYieldCurveMethod Members

    }
    #endregion YieldCurveMethod

}
