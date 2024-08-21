#region Using Directives
using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;

using EFS.ApplicationBlocks.Data;
using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using EfsML.v20;
using EfsML.EventMatrix;
using EfsML.Notification;
using EfsML.v20.Notification;
using EfsML.Settlement;
using EfsML.v20.Settlement;
using EfsML.v20.Settlement.Message;

using FpML.Enum;
using FixML.Enum;
using FixML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Eqd;
using FpML.v42.Eqs;
using FpML.v42.Fx;
using FpML.v42.Ird;
using FpML.v42.Cd;
using FpML.Interface;

#endregion Using Directives
namespace FpML.v42.Shared
{
    #region Address
    public partial class Address : ICloneable, IAddress
    {
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Address clone = new Address();
            clone.city = new EFS_String(this.city.Value);
            clone.country = (Country)this.country.Clone();
            clone.postalCode = new EFS_String(this.postalCode.Value);
            clone.state = new EFS_String(this.state.Value);
            clone.streetAddress = new StreetAddress();
            clone.streetAddress.streetLine = new EFS_StringArray[this.streetAddress.streetLine.Length];
            for (int i = 0; i < this.streetAddress.streetLine.Length; i++)
                clone.streetAddress.streetLine[i] = new EFS_StringArray(this.streetAddress.streetLine[i].Value);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IAddress Membres
        IStreetAddress IAddress.streetAddress
        {
            get { return this.streetAddress; }
            set { streetAddress = (StreetAddress)value; }
        }
        EFS_String IAddress.city
        {
            get { return this.city; }
            set { this.city = value; }
        }
        EFS_String IAddress.state
        {
            get { return this.state; }
            set { this.state = value; }
        }
        IScheme IAddress.country
        {
            get { return this.country; }
            set { this.country = (Country)value; }
        }
        EFS_String IAddress.postalCode
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
        public AdjustableDate()
        {
            unadjustedDate = new IdentifierDate();
            dateAdjustments = new BusinessDayAdjustments();
        }
        #endregion Constructors

        #region IAdjustableDate Members
        IAdjustedDate IAdjustableDate.unadjustedDate
        {
            set { this.unadjustedDate = (IdentifierDate)value; }
            get { return this.unadjustedDate; }
        }
        IBusinessDayAdjustments IAdjustableDate.dateAdjustments
        {
            set { this.dateAdjustments = (BusinessDayAdjustments)value; }
            get { return this.dateAdjustments; }
        }
        string IAdjustableDate.efs_id
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
            AdjustableDate clone = new AdjustableDate();
            clone.unadjustedDate = (IdentifierDate)this.unadjustedDate.Clone();
            clone.dateAdjustments = (BusinessDayAdjustments)this.dateAdjustments.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion AdjustableDate
    #region AdjustableDates
    public partial class AdjustableDates : IAdjustableDates
    {
        #region Constructors
        public AdjustableDates()
        {
            unadjustedDate = new unadjustedDate[] { new unadjustedDate() };
        }
        #endregion Constructors
        #region IAdjustableDates Members
        IAdjustedDate[] IAdjustableDates.unadjustedDate
        {
            get { return (IAdjustedDate[])this.unadjustedDate; }
            set { this.unadjustedDate = (unadjustedDate[])value; }
        }
        IBusinessDayAdjustments IAdjustableDates.dateAdjustments
        {
            get { return this.dateAdjustments; }
        }
        DateTime IAdjustableDates.this[int pIndex]
        {
            get { return this.unadjustedDate[pIndex].dtValue.DateValue; }
        }
        #endregion IAdjustableDates Members
    }
    #endregion AdjustableDates
    #region AdjustableOrRelativeDate
    public partial class AdjustableOrRelativeDate : IAdjustableOrRelativeDate
    {
        #region Constructors
        public AdjustableOrRelativeDate()
        {
            adjustableOrRelativeDateRelativeDate = new RelativeDateOffset();
            adjustableOrRelativeDateAdjustableDate = new AdjustableDate();
        }
        #endregion Constructors

        #region IAdjustableOrRelativeDate Members
        bool IAdjustableOrRelativeDate.adjustableDateSpecified
        {
            set { this.adjustableOrRelativeDateAdjustableDateSpecified = value; }
            get { return this.adjustableOrRelativeDateAdjustableDateSpecified; }
        }
        IAdjustableDate IAdjustableOrRelativeDate.adjustableDate
        {
            set { this.adjustableOrRelativeDateAdjustableDate = (AdjustableDate)value; }
            get { return this.adjustableOrRelativeDateAdjustableDate; }
        }
        bool IAdjustableOrRelativeDate.relativeDateSpecified
        {
            set { this.adjustableOrRelativeDateRelativeDateSpecified = value; }
            get { return this.adjustableOrRelativeDateRelativeDateSpecified; }
        }
        IRelativeDateOffset IAdjustableOrRelativeDate.relativeDate
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
        bool IAdjustableOrRelativeDates.adjustableDatesSpecified
        {
            set { this.adjustableOrRelativeDatesAdjustableDatesSpecified = value; }
            get { return this.adjustableOrRelativeDatesAdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableOrRelativeDates.adjustableDates
        {
            set { this.adjustableOrRelativeDatesAdjustableDates = (AdjustableDates)value; }
            get { return this.adjustableOrRelativeDatesAdjustableDates; }
        }
        bool IAdjustableOrRelativeDates.relativeDatesSpecified
        {
            set { this.adjustableOrRelativeDatesRelativeDatesSpecified = value; }
            get { return this.adjustableOrRelativeDatesRelativeDatesSpecified; }
        }
        IRelativeDates IAdjustableOrRelativeDates.relativeDates
        {
            set { this.adjustableOrRelativeDatesRelativeDates = (RelativeDates)value; }
            get { return this.adjustableOrRelativeDatesRelativeDates; }
        }
        #endregion IAdjustableOrRelativeDates Members
    }
    #endregion AdjustableOrRelativeDates
    #region AdjustableRelativeOrPeriodicDates
    // EG 20140702 Upd Interface
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
        bool IAdjustableRelativeOrPeriodicDates.adjustableDatesSpecified
        {
            get { return this.adjustableRelativeOrPeriodicAdjustableDatesSpecified; }
        }
        IAdjustableDates IAdjustableRelativeOrPeriodicDates.adjustableDates
        {
            get { return this.adjustableRelativeOrPeriodicAdjustableDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates.periodicDatesSpecified
        {
            set { this.adjustableRelativeOrPeriodicPeriodicDatesSpecified = value; }
            get { return this.adjustableRelativeOrPeriodicPeriodicDatesSpecified; }
        }
        IPeriodicDates IAdjustableRelativeOrPeriodicDates.periodicDates
        {
            set { this.adjustableRelativeOrPeriodicPeriodicDates = (PeriodicDates)value; }
            get { return this.adjustableRelativeOrPeriodicPeriodicDates; }
        }
        bool IAdjustableRelativeOrPeriodicDates.relativeDateSequenceSpecified
        {
            set { this.adjustableRelativeOrPeriodicRelativeDateSequenceSpecified = value; }
            get { return this.adjustableRelativeOrPeriodicRelativeDateSequenceSpecified; }
        }
        IRelativeDateSequence IAdjustableRelativeOrPeriodicDates.relativeDateSequence
        {
            get { return this.adjustableRelativeOrPeriodicRelativeDateSequence; }
        }
        #endregion IAdjustableRelativeOrPeriodicDates Members

    }
    #endregion AdjustableRelativeOrPeriodicDates
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
        IAdjustableOrRelativeDate IAmericanExercise.commencementDate { get { return this.commencementDate; } }
        IAdjustableOrRelativeDate IAmericanExercise.expirationDate { get { return this.expirationDate; } }
        bool IAmericanExercise.relevantUnderlyingDateSpecified
        {
            set { this.relevantUnderlyingDateSpecified = value; }
            get { return this.relevantUnderlyingDateSpecified; }
        }
        IAdjustableOrRelativeDates IAmericanExercise.relevantUnderlyingDate
        {
            set { this.relevantUnderlyingDate = (AdjustableOrRelativeDates)value; }
            get { return this.relevantUnderlyingDate; }
        }
        bool IAmericanExercise.latestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime IAmericanExercise.latestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        bool IAmericanExercise.exerciseFeeScheduleSpecified
        {
            set { this.exerciseFeeScheduleSpecified = value; }
            get { return this.exerciseFeeScheduleSpecified; }
        }
        IExerciseFeeSchedule IAmericanExercise.exerciseFeeSchedule
        {
            get { return this.exerciseFeeSchedule; }
        }
        bool IAmericanExercise.multipleExerciseSpecified
        {
            set { this.multipleExerciseSpecified = value; }
            get { return this.multipleExerciseSpecified; }
        }

        IMultipleExercise IAmericanExercise.multipleExercise
        {
            set { this.multipleExercise = (MultipleExercise)value; }
            get { return this.multipleExercise; }
        }
        IMultipleExercise IAmericanExercise.CreateMultipleExercise()
        {
            MultipleExercise multiple = new MultipleExercise();
            multiple.integralMultipleAmountSpecified = false;
            multiple.maximumNotionalAmountSpecified = false;
            return multiple;
        }

        #endregion IAmericanExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.earliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }
        IBusinessCenterTime IExerciseBase.expirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime();
                bct.businessCenter = new BusinessCenter();
                bct.hourMinuteTime = new HourMinuteTime();
                return bct;
            }
        }
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            int nbNotionalReference = pNotionalReference.Length;
            ExerciseNotionalReference[] exerciseNotionalReference = null;
            if (0 < nbNotionalReference)
            {
                exerciseNotionalReference = new ExerciseNotionalReference[nbNotionalReference];
                for (int i = 0; i < nbNotionalReference; i++)
                {
                    exerciseNotionalReference[i] = new ExerciseNotionalReference();
                    ((IReference)exerciseNotionalReference[i]).hRef = pNotionalReference[i];
                }
            }
            return exerciseNotionalReference;
        }
        #endregion IExerciseBase Members
    }
    #endregion AmericanExercise
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
        ICurrency IAmountSchedule.currency { get { return this.currency; } }
        #endregion IAmountSchedule Members
    }
    #endregion AmountSchedule
    #region ArrayPartyReference
    public partial class ArrayPartyReference : IEFS_Array, IReference
    {
        #region Constructor
        public ArrayPartyReference() { }
        public ArrayPartyReference(string pPartyReference)
        {
            this.href = pPartyReference;
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
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members

    }
    #endregion ArrayPartyReference
    #region AutomaticExercise
    public partial class AutomaticExercise : IAutomaticExercise
    {
        #region IAutomaticExercise Members
        decimal IAutomaticExercise.thresholdRate { get { return this.thresholdRate.DecValue; } }
        #endregion IAutomaticExercise Members
    }
    #endregion AutomaticExercise

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
        IAdjustableOrRelativeDates IBermudaExercise.bermudaExerciseDates { get { return this.bermudaExerciseDates; } }
        bool IBermudaExercise.relevantUnderlyingDateSpecified { get { return this.relevantUnderlyingDateSpecified; } }
        IAdjustableOrRelativeDates IBermudaExercise.relevantUnderlyingDate { get { return this.relevantUnderlyingDate; } }
        bool IBermudaExercise.latestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime IBermudaExercise.latestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        bool IBermudaExercise.exerciseFeeScheduleSpecified
        {
            get { return this.exerciseFeeScheduleSpecified; }
        }
        IExerciseFeeSchedule IBermudaExercise.exerciseFeeSchedule
        {
            get { return this.exerciseFeeSchedule; }
        }
        bool IBermudaExercise.multipleExerciseSpecified
        {
            set { this.multipleExerciseSpecified = value; }
            get { return this.multipleExerciseSpecified; }
        }
        IMultipleExercise IBermudaExercise.multipleExercise
        {
            set { this.multipleExercise = (MultipleExercise)value; }
            get { return this.multipleExercise; }
        }
        IMultipleExercise IBermudaExercise.CreateMultipleExercise()
        {
            MultipleExercise multiple = new MultipleExercise();
            multiple.integralMultipleAmountSpecified = false;
            multiple.maximumNotionalAmountSpecified = false;
            return multiple;
        }
        #endregion IBermudaExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.earliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }
        IBusinessCenterTime IExerciseBase.expirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime();
                bct.businessCenter = new BusinessCenter();
                bct.hourMinuteTime = new HourMinuteTime();
                return bct;
            }
        }
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            int nbNotionalReference = pNotionalReference.Length;
            ExerciseNotionalReference[] exerciseNotionalReference = null;
            if (0 < nbNotionalReference)
            {
                exerciseNotionalReference = new ExerciseNotionalReference[nbNotionalReference];
                for (int i = 0; i < nbNotionalReference; i++)
                {
                    exerciseNotionalReference[i] = new ExerciseNotionalReference();
                    ((IReference)exerciseNotionalReference[i]).hRef = pNotionalReference[i];
                }
            }
            return exerciseNotionalReference;
        }
        #endregion IExerciseBase Members
    }
    #endregion BermudaExercise
    #region BusinessCenter
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class BusinessCenter : IEFS_Array, ICloneable, IBusinessCenter
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
        #region Constructors
        public BusinessCenter() : this(string.Empty) { }
        public BusinessCenter(string pValue)
        {
            businessCenterScheme = "http://www.fpml.org/coding-scheme/business-center-2-0";
            Value = pValue;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object _Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        #region IBusinessCenter Members
        string IBusinessCenter.businessCenterScheme
        {
            set { this.businessCenterScheme = value; }
            get { return this.businessCenterScheme; }
        }
        string IBusinessCenter.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        /// EG 20150422 [20513] BANCAPERTA New 
        string IBusinessCenter.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        /// EG 20150422 [20513] BANCAPERTA New 
        EFS_Id IBusinessCenter.efs_id { get { return this.efs_id; } }
        #endregion IBusinessCenter Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            BusinessCenter clone = new BusinessCenter();
            clone.businessCenterScheme = this.businessCenterScheme;
            clone.Value = this.Value;
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
        public void Add(BusinessCenter pBc)
        {
            ArrayList aList = null;
            //
            if (null != businessCenter)
                aList = new ArrayList(businessCenter);
            else
                aList = new ArrayList();
            //
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
        #endregion
        #endregion Methods

        #region IBusinessCenters Members
        IBusinessCenter[] IBusinessCenters.businessCenter
        {
            set { this.businessCenter = (BusinessCenter[])value; }
            get { return this.businessCenter; }
        }
        object IBusinessCenters.Clone() { return this.Clone(); }
        IBusinessDayAdjustments IBusinessCenters.GetBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments();
            bda.businessDayConvention = pBusinessDayConvention;
            bda.businessCentersDefineSpecified = true;
            bda.businessCentersDefine = this;
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
        /// <param name="pCs"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC">devises au format ISO4217_ALPHA3</param>
        /// <param name="pIdM"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters IBusinessCenters.LoadBusinessCenters(string pCs, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM)
        {
            DataRowCollection rows = Tools.LoadBusinessCenter(pCs, pDbTransaction, pIdA, pIdC, pIdM);
            if ((null != rows) && (0 < rows.Count))
            {
                BusinessCenter[] businessCenter = new BusinessCenter[rows.Count];
                int i = 0;
                foreach (DataRow row in rows)
                {
                    if (0 < row[0].ToString().Length)
                    {
                        businessCenter[i] = new BusinessCenter();
                        businessCenter[i].Value = row[0].ToString();
                        i++;
                    }
                }
                return new BusinessCenters(businessCenter);
            }
            return null;
        }
        string IBusinessCenters.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        EFS_Id IBusinessCenters.efs_id { get { return this.efs_id; } }
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
            BusinessCentersReference clone = new BusinessCentersReference();
            clone.href = this.href;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IReference Members
        string IReference.hRef
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
            BusinessCenterTime clone = new BusinessCenterTime();
            clone.hourMinuteTime = new HourMinuteTime(this.hourMinuteTime.Value);
            clone.businessCenter = (BusinessCenter)this.businessCenter.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IBusinessCenterTime Members
        IHourMinuteTime IBusinessCenterTime.hourMinuteTime
        {
            get { return this.hourMinuteTime; }
        }
        IBusinessCenter IBusinessCenterTime.businessCenter
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
                BusinessDayAdjustments businessDayAdjustments = new BusinessDayAdjustments();
                businessDayAdjustments.businessCentersDefine = this.businessCentersDefine;
                businessDayAdjustments.businessCentersDefineSpecified = this.businessCentersDefineSpecified;
                businessDayAdjustments.businessCentersReference = this.businessCentersReference;
                businessDayAdjustments.businessCentersReferenceSpecified = this.businessCentersReferenceSpecified;
                businessDayAdjustments.businessCentersNone = this.businessCentersNone;
                businessDayAdjustments.businessCentersNoneSpecified = this.businessCentersNoneSpecified;
                businessDayAdjustments.businessDayConvention = this.businessDayConvention;
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
            BusinessDateRange clone = new BusinessDateRange();
            clone.unadjustedFirstDate = new EFS_Date(this.unadjustedFirstDate.Value);
            clone.unadjustedLastDate = new EFS_Date(this.unadjustedLastDate.Value);
            clone.businessDayConvention = this.businessDayConvention;
            //	
            clone.businessCentersDefineSpecified = this.businessCentersDefineSpecified;
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
        #region IBusinessDateRange Members
        IBusinessDayAdjustments IBusinessDateRange.GetAdjustments { get { return this.GetAdjustments; } }
        EFS_Date IBusinessDateRange.unadjustedFirstDate
        {
            set { this.unadjustedFirstDate = value; }
            get { return this.unadjustedFirstDate; }
        }
        EFS_Date IBusinessDateRange.unadjustedLastDate
        {
            set { this.unadjustedLastDate = value; }
            get { return this.unadjustedLastDate; }
        }
        #endregion IBusinessDateRange Members
        #region IBusinessDayAdjustments Membres
        // EG 20190115 [24361]
        bool IBusinessDayAdjustments.isSettlementOfHolidayDeliveryConvention
        {
            set { }
            get { return false; }
        }
        BusinessDayConventionEnum IBusinessDayAdjustments.businessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        bool IBusinessDayAdjustments.businessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IBusinessDayAdjustments.businessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IBusinessDayAdjustments.businessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IBusinessDayAdjustments.businessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IBusinessDayAdjustments.businessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IBusinessDayAdjustments.businessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IBusinessDayAdjustments.businessCentersReferenceValue
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
            if ((null != pBusinessCenters) && (ArrFunc.IsFilled(pBusinessCenters.businessCenter)))
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
            BusinessDayAdjustments clone = new BusinessDayAdjustments();
            clone.businessDayConvention = this.businessDayConvention;
            //	
            clone.businessCentersDefineSpecified = this.businessCentersDefineSpecified;
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
        // EG 20190115 [24361]
        bool IBusinessDayAdjustments.isSettlementOfHolidayDeliveryConvention
        {
            set { }
            get { return false; }
        }
        BusinessDayConventionEnum IBusinessDayAdjustments.businessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        bool IBusinessDayAdjustments.businessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IBusinessDayAdjustments.businessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IBusinessDayAdjustments.businessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IBusinessDayAdjustments.businessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IBusinessDayAdjustments.businessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IBusinessDayAdjustments.businessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IBusinessDayAdjustments.businessCentersReferenceValue
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
        bool ICalculationAgent.partyReferenceSpecified
        {
            set { this.calculationAgentPartyReferenceSpecified = value; }
            get { return this.calculationAgentPartyReferenceSpecified; }
        }
        IReference[] ICalculationAgent.partyReference
        {
            set { this.calculationAgentPartyReference = (ArrayPartyReference[])value; }
            get { return this.calculationAgentPartyReference; }
        }
        bool ICalculationAgent.partySpecified
        {
            set { this.calculationAgentPartySpecified = value; }
            get { return this.calculationAgentPartySpecified; }
        }
        CalculationAgentPartyEnum ICalculationAgent.party
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
        PeriodEnum ICalculationPeriodFrequency.period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer ICalculationPeriodFrequency.periodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval ICalculationPeriodFrequency.interval { get { return (IInterval)this; } }
        RollConventionEnum ICalculationPeriodFrequency.rollConvention
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
            clearanceSystemIdScheme = "http://www.fpml.org/spec/2002/clearance-system-1-0";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.scheme
        {
            set { this.clearanceSystemIdScheme = value; }
            get { return this.clearanceSystemIdScheme; }
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
            Country clone = new Country();
            clone.countryScheme = this.countryScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

        #region IScheme Members
        string IScheme.scheme
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
        public static object _Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Currency clone = new Currency();
            clone.currencyScheme = this.currencyScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region ICurrency Members
        string ICurrency.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        string ICurrency.currencyScheme
        {
            set { this.currencyScheme = value; }
            get { return this.currencyScheme; }
        }
        #endregion ICurrency Members
    }
    #endregion Currency

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
        object[] IDateList.date { get { return this.date; } }
        DateTime IDateList.this[int pIndex] { get { return this.date[pIndex].DateValue; } }
        #endregion IDateList Members
    }
    #endregion DateList
    #region DateOffset
    // EG 20140702 Upd Interface
    public partial class DateOffset : IEFS_Array, IDateOffset
    {
        #region IDateOffset Members
        PeriodEnum IDateOffset.period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IDateOffset.periodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        bool IDateOffset.dayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IDateOffset.dayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        BusinessDayConventionEnum IDateOffset.businessDayConvention
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
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion DateReference
    #region Documentation
    public partial class Documentation : IDocumentation
    {
        #region IDocumentation Membres
        bool IDocumentation.masterAgreementSpecified
        {
            get { return this.masterAgreementSpecified; }
            set { masterAgreementSpecified = value; }
        }
        IMasterAgreement IDocumentation.masterAgreement
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

    #region DividendConditions
    // EG 20140702 Upd Interface
    public partial class DividendConditions : IDividendConditions
    {
        #region IDividendConditions Members
        bool IDividendConditions.dividendEntitlementSpecified
        {
            set { this.dividendEntitlementSpecified = value; }
            get { return this.dividendEntitlementSpecified; }
        }
        DividendEntitlementEnum IDividendConditions.dividendEntitlement
        {
            set { this.dividendEntitlement = value; }
            get { return this.dividendEntitlement; }
        }
        bool IDividendConditions.dividendAmountSpecified
        {
            set { this.dividendAmountSpecified = value; }
            get { return this.dividendAmountSpecified; }
        }
        DividendAmountTypeEnum IDividendConditions.dividendAmount
        {
            set { this.dividendAmount = value; }
            get { return this.dividendAmount; }
        }
        bool IDividendConditions.dividendPeriodSpecified
        {
            set { this.dividendPeriodSpecified = value; }
            get { return this.dividendPeriodSpecified; }
        }
        DividendPeriodEnum IDividendConditions.dividendPeriod
        {
            set { this.dividendPeriod = value; }
            get { return this.dividendPeriod; }
        }
        bool IDividendConditions.dividendPaymentDateSpecified
        {
            set { this.dividendPaymentDateSpecified = value; }
            get { return this.dividendPaymentDateSpecified; }
        }
        IDividendPaymentDate IDividendConditions.dividendPaymentDate
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
        }
        #endregion Constructors

        #region IDividendPaymentDate Members
        bool IDividendPaymentDate.dividendDateReferenceSpecified
        {
            set { this.paymentDateDividendDateReferenceSpecified = value; }
            get { return this.paymentDateDividendDateReferenceSpecified; }
        }
        DividendDateReferenceEnum IDividendPaymentDate.dividendDateReference
        {
            set { this.paymentDateDividendDateReference = value; }
            get { return this.paymentDateDividendDateReference; }
        }
        bool IDividendPaymentDate.adjustableDateSpecified
        {
            get { return this.paymentDateAdjustableDateSpecified; }
        }
        IAdjustableDate IDividendPaymentDate.adjustableDate
        {
            get { return this.paymentDateAdjustableDate; }
        }
        bool IDividendPaymentDate.offsetSpecified
        {
            get { return false; }
        }
        IOffset IDividendPaymentDate.offset
        {
            get { return null; }
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
        object IEmpty.empty
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
        IAdjustableOrRelativeDate IEuropeanExercise.expirationDate { get { return this.expirationDate; } }
        bool IEuropeanExercise.relevantUnderlyingDateSpecified { get { return this.relevantUnderlyingDateSpecified; } }
        IAdjustableOrRelativeDates IEuropeanExercise.relevantUnderlyingDate { get { return this.relevantUnderlyingDate; } }
        bool IEuropeanExercise.exerciseFeeSpecified
        {
            get { return this.exerciseFeeSpecified; }
        }
        IExerciseFee IEuropeanExercise.exerciseFee
        {
            get { return this.exerciseFee; }
        }
        bool IEuropeanExercise.partialExerciseSpecified
        {
            set { this.partialExerciseSpecified = value; }
            get { return this.partialExerciseSpecified; }
        }
        IPartialExercise IEuropeanExercise.partialExercise
        {
            get { return this.partialExercise; }
        }
        IPartialExercise IEuropeanExercise.CreatePartialExercise()
        {
            MultipleExercise multiple = new MultipleExercise();
            multiple.integralMultipleAmountSpecified = false;
            multiple.maximumNotionalAmountSpecified = false;
            return multiple;
        }
        #endregion IEuropeanExercise Members
        #region IExerciseBase Members
        IBusinessCenterTime IExerciseBase.earliestExerciseTime
        {
            set { this.earliestExerciseTime = (BusinessCenterTime)value; }
            get { return this.earliestExerciseTime; }
        }
        IBusinessCenterTime IExerciseBase.expirationTime
        {
            set { this.expirationTime = (BusinessCenterTime)value; }
            get { return this.expirationTime; }
        }
        IBusinessCenterTime IExerciseBase.CreateBusinessCenterTime
        {
            get
            {
                BusinessCenterTime bct = new BusinessCenterTime();
                bct.businessCenter = new BusinessCenter();
                bct.hourMinuteTime = new HourMinuteTime();
                return bct;
            }
        }
        IReference[] IExerciseBase.CreateNotionalReference(string[] pNotionalReference)
        {
            int nbNotionalReference = pNotionalReference.Length;
            ExerciseNotionalReference[] exerciseNotionalReference = null;
            if (0 < nbNotionalReference)
            {
                exerciseNotionalReference = new ExerciseNotionalReference[nbNotionalReference];
                for (int i = 0; i < nbNotionalReference; i++)
                {
                    exerciseNotionalReference[i] = new ExerciseNotionalReference();
                    ((IReference)exerciseNotionalReference[i]).hRef = pNotionalReference[i];
                }
            }
            return exerciseNotionalReference;
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
        string IScheme.scheme
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


        #region ISpheresId Members
        string ISpheresId.otcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
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

        #region IExerciseId Members
        string IExerciseId.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        #endregion

        #region IExercise Members
        EFS_ExerciseDates IExercise.efs_ExerciseDates
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
        bool IExerciseFee.feeAmountSpecified
        {
            set { this.typeFeeAmountSpecified = value; }
            get { return this.typeFeeAmountSpecified; }
        }
        EFS_Decimal IExerciseFee.feeAmount
        {
            set { this.typeFeeAmount = value; }
            get { return this.typeFeeAmount; }
        }
        bool IExerciseFee.feeRateSpecified
        {
            set { this.typeFeeRateSpecified = value; }
            get { return this.typeFeeRateSpecified; }
        }
        EFS_Decimal IExerciseFee.feeRate
        {
            set { this.typeFeeRate = value; }
            get { return this.typeFeeRate; }
        }
        #endregion IExerciseFee Members
        #region IExerciseFeeBase Members
        IReference IExerciseFeeBase.payerPartyReference
        {
            get { return this.payerPartyReference; }
        }
        IReference IExerciseFeeBase.receiverPartyReference
        {
            get { return this.receiverPartyReference; }
        }
        IReference IExerciseFeeBase.notionalReference
        {
            get { return this.notionalReference; }
        }
        IRelativeDateOffset IExerciseFeeBase.feePaymentDate
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
        bool IExerciseFeeSchedule.feeAmountSpecified
        {
            get { return this.typeFeeAmountSpecified; }
        }
        IAmountSchedule IExerciseFeeSchedule.feeAmount
        {
            get { return this.typeFeeAmount; }
        }
        bool IExerciseFeeSchedule.feeRateSpecified
        {
            get { return this.typeFeeRateSpecified; }
        }

        ISchedule IExerciseFeeSchedule.feeRate
        {
            get { return this.typeFeeRate; }
        }
        #endregion IExerciseFeeSchedule Members
        #region IExerciseFeeBase Members
        IReference IExerciseFeeBase.payerPartyReference
        {
            get { return this.payerPartyReference; }
        }
        IReference IExerciseFeeBase.receiverPartyReference
        {
            get { return this.receiverPartyReference; }
        }
        IReference IExerciseFeeBase.notionalReference
        {
            get { return this.notionalReference; }
        }
        IRelativeDateOffset IExerciseFeeBase.feePaymentDate
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
        IReference IExerciseNotice.partyReference
        {
            set { this.partyReference = (PartyReference)value; }
            get { return this.partyReference; }
        }
        bool IExerciseNotice.exerciseNoticePartyReferenceSpecified
        {
            set { this.exerciseNoticePartyReferenceSpecified = value; }
            get { return this.exerciseNoticePartyReferenceSpecified; }
        }
        IReference IExerciseNotice.exerciseNoticePartyReference
        {
            set { this.exerciseNoticePartyReference = (PartyReference)value; }
            get { return this.exerciseNoticePartyReference; }
        }
        IBusinessCenter IExerciseNotice.businessCenter
        {
            set { this.businessCenter = (BusinessCenter)value; }
            get { return this.businessCenter; }
        }
        #endregion IExerciseNotice Members
    }
    #endregion ExerciseNotice
    #region ExerciseNotionalReference
    public partial class ExerciseNotionalReference : IEFS_Array, IReference
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IReference Members
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }

        #endregion IReference Members
    }
    #endregion ExerciseNotionalReference
    #region ExerciseProcedure
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class ExerciseProcedure : IExerciseProcedure
    {
        #region Constructors
        public ExerciseProcedure()
        {
            exerciseProcedureAutomatic = new AutomaticExercise();
            exerciseProcedureManual = new ManualExercise();
            followUpConfirmationSpecified = true;
        }
        #endregion Constructors

        #region IExerciseProcedure Members
        bool IExerciseProcedure.exerciseProcedureAutomaticSpecified { get { return this.exerciseProcedureAutomaticSpecified; } }
        IAutomaticExercise IExerciseProcedure.exerciseProcedureAutomatic { get { return this.exerciseProcedureAutomatic; } }
        bool IExerciseProcedure.exerciseProcedureManualSpecified { get { return this.exerciseProcedureManualSpecified; } }
        IManualExercise IExerciseProcedure.exerciseProcedureManual { get { return this.exerciseProcedureManual; } }
        /// EG 20150422 [20513] BANCAPERTA New 
        bool IExerciseProcedure.followUpConfirmationSpecified { get { return true; } }
        bool IExerciseProcedure.followUpConfirmation { get { return this.followUpConfirmation.BoolValue; } }
        #endregion IExerciseProcedure Members
    }
    #endregion ExerciseProcedure

    #region FloatingRate
    public partial class FloatingRate : IFloatingRate
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
        #region Constructors
        public FloatingRate()
        {
            floatingRateIndex = new FloatingRateIndex();
            floatingRateMultiplierSchedule = new Schedule();
            spreadSchedule = new Schedule();
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
        bool IFloatingRate.capRateScheduleSpecified
        {
            set { this.capRateScheduleSpecified = value; }
            get { return this.capRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.capRateSchedule
        {
            set { this.capRateSchedule = (StrikeSchedule[])value; }
            get { return this.capRateSchedule; }
        }
        bool IFloatingRate.floorRateScheduleSpecified
        {
            set { this.floorRateScheduleSpecified = value; }
            get { return this.floorRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.floorRateSchedule
        {
            set { this.floorRateSchedule = (StrikeSchedule[])value; }
            get { return this.floorRateSchedule; }
        }
        IFloatingRateIndex IFloatingRate.floatingRateIndex { get { return this.floatingRateIndex; } }
        bool IFloatingRate.spreadScheduleSpecified
        {
            set { this.spreadScheduleSpecified = value; }
            get { return this.spreadScheduleSpecified; }
        }
        ISchedule IFloatingRate.spreadSchedule
        {
            set { this.spreadSchedule = (Schedule)value; }
            get { return this.spreadSchedule; }
        }
        // EG 2050309 POC - BERKELEY New
        ISpreadSchedule[] IFloatingRate.lstSpreadSchedule
        {
            set {  }
            get {return null; }
        }
        bool IFloatingRate.floatingRateMultiplierScheduleSpecified
        {
            set { this.floatingRateMultiplierScheduleSpecified = value; }
            get { return this.floatingRateMultiplierScheduleSpecified; }
        }
        ISchedule IFloatingRate.floatingRateMultiplierSchedule { get { return this.floatingRateMultiplierSchedule; } }
        bool IFloatingRate.rateTreatmentSpecified { get { return this.rateTreatmentSpecified; } }
        RateTreatmentEnum IFloatingRate.rateTreatment { get { return this.rateTreatment; } }
        ISchedule IFloatingRate.CreateSchedule()
        {
            this.spreadSchedule = new Schedule();
            return spreadSchedule;
        }
        ISpreadSchedule IFloatingRate.CreateSpreadSchedule()
        {
            return null;
        }
        IStrikeSchedule[] IFloatingRate.CreateStrikeSchedule(int pDim)
        {
            StrikeSchedule[] ret = new StrikeSchedule[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new StrikeSchedule();
            return ret;
        }
        bool IFloatingRate.indexTenorSpecified
        {
            set { this.indexTenorSpecified = value; }
            get { return this.indexTenorSpecified; }
        }
        IInterval IFloatingRate.indexTenor
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
        bool IFloatingRateCalculation.initialRateSpecified
        {
            set { this.initialRateSpecified = value; }
            get { return this.initialRateSpecified; }
        }
        EFS_Decimal IFloatingRateCalculation.initialRate
        {
            set { this.initialRate = value; }
            get { return this.initialRate; }
        }

        bool IFloatingRateCalculation.finalRateRoundingSpecified { get { return this.finalRateRoundingSpecified; } }
        IRounding IFloatingRateCalculation.finalRateRounding { get { return this.finalRateRounding; } }
        bool IFloatingRateCalculation.averagingMethodSpecified { get { return this.averagingMethodSpecified; } }
        AveragingMethodEnum IFloatingRateCalculation.averagingMethod { get { return this.averagingMethod; } }
        bool IFloatingRateCalculation.negativeInterestRateTreatmentSpecified
        {
            set { this.negativeInterestRateTreatmentSpecified = value; }
            get { return this.negativeInterestRateTreatmentSpecified; }
        }
        NegativeInterestRateTreatmentEnum IFloatingRateCalculation.negativeInterestRateTreatment
        {
            set { this.negativeInterestRateTreatment = value; }
            get { return this.negativeInterestRateTreatment; }
        }
        ISpreadSchedule[] IFloatingRateCalculation.CreateSpreadSchedules(ISpreadSchedule[] pSpreadSchedules)
        {
            return null;
        }
        #endregion IFloatingRateCalculation Members

        #region IFloatingRate Members
        bool IFloatingRate.capRateScheduleSpecified
        {
            set { this.capRateScheduleSpecified = value; }
            get { return this.capRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.capRateSchedule
        {
            set { this.capRateSchedule = (StrikeSchedule[])value; }
            get { return this.capRateSchedule; }
        }
        bool IFloatingRate.floorRateScheduleSpecified
        {
            set { this.floorRateScheduleSpecified = value; }
            get { return this.floorRateScheduleSpecified; }
        }
        IStrikeSchedule[] IFloatingRate.floorRateSchedule
        {
            set { this.floorRateSchedule = (StrikeSchedule[])value; }
            get { return this.floorRateSchedule; }
        }
        IFloatingRateIndex IFloatingRate.floatingRateIndex { get { return this.floatingRateIndex; } }
        bool IFloatingRate.spreadScheduleSpecified
        {
            set { this.spreadScheduleSpecified = value; }
            get { return this.spreadScheduleSpecified; }
        }
        ISchedule IFloatingRate.spreadSchedule
        {
            set { this.spreadSchedule = (Schedule)value; }
            get { return this.spreadSchedule; }
        }
        // EG 2050309 POC - BERKELEY New
        ISpreadSchedule[] IFloatingRate.lstSpreadSchedule
        {
            set {}
            get { return null; }
        }

        bool IFloatingRate.floatingRateMultiplierScheduleSpecified
        {
            set { this.floatingRateMultiplierScheduleSpecified = value; }
            get { return this.floatingRateMultiplierScheduleSpecified; }
        }
        ISchedule IFloatingRate.floatingRateMultiplierSchedule { get { return this.floatingRateMultiplierSchedule; } }
        bool IFloatingRate.rateTreatmentSpecified { get { return this.rateTreatmentSpecified; } }
        RateTreatmentEnum IFloatingRate.rateTreatment { get { return this.rateTreatment; } }
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
            floatingRateIndexScheme = "http://www.fpml.org/ext/isda-2000-definitions";
        }
        #endregion Constructors

        #region IFloatingRateIndex Members
        int IFloatingRateIndex.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        bool IFloatingRateIndex.hrefSpecified
        {
            set { ; }
            get { return false; }
        }
        string IFloatingRateIndex.href
        {
            set { }
            get { return null; }
        }
        string IFloatingRateIndex.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IFloatingRateIndex Members
    }
    #endregion FloatingRateIndex
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
        ICurrency IFxCashSettlement.settlementCurrency { get { return this.settlementCurrency; } }
        IFxFixing[] IFxCashSettlement.fixing { get { return this.fixing; } }
        //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
        bool IFxCashSettlement.customerSettlementRateSpecified
        {
            set { this.customerSettlementRateSpecified = value; }
            get { return this.customerSettlementRateSpecified; }
        }
        bool IFxCashSettlement.calculationAgentSettlementRateSpecified
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public FxFixing FixingRate
        {
            get
            {
                 return this; 
                
            }
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
            FxFixing clone = new FxFixing();
            clone.primaryRateSource = (InformationSource)this.primaryRateSource.Clone();
            clone.secondaryRateSourceSpecified = this.secondaryRateSourceSpecified;
            if (clone.secondaryRateSourceSpecified)
                clone.secondaryRateSource = (InformationSource)this.secondaryRateSource.Clone();
            clone.fixingTime = (BusinessCenterTime)this.fixingTime.Clone();
            clone.quotedCurrencyPair = (QuotedCurrencyPair)this.quotedCurrencyPair.Clone();
            clone.fixingDate = new EFS_Date(this.fixingDate.Value);
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
        IQuotedCurrencyPair IFxFixing.quotedCurrencyPair
        {
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
            get { return this.quotedCurrencyPair; }
        }
        IInformationSource IFxFixing.primaryRateSource
        {
            set { this.primaryRateSource = (InformationSource)value; }
            get { return this.primaryRateSource; }
        }
        bool IFxFixing.secondaryRateSourceSpecified
        {
            set { this.secondaryRateSourceSpecified = value; }
            get { return this.secondaryRateSourceSpecified; }
        }
        IInformationSource IFxFixing.secondaryRateSource
        {
            set { this.secondaryRateSource = (InformationSource)value; }
            get { return this.secondaryRateSource; }
        }
        EFS_Date IFxFixing.fixingDate
        {
            set { this.fixingDate = value; }
            get { return this.fixingDate; }
        }
        IBusinessCenterTime IFxFixing.fixingTime
        {
            set { this.fixingTime = (BusinessCenterTime)value; }
            get { return this.fixingTime; }
        }
        IInformationSource IFxFixing.CreateInformationSource()
        {

            InformationSource informationSource = new InformationSource();
            informationSource.rateSource = new InformationProvider();
            return informationSource;

        }
        IBusinessCenterTime IFxFixing.CreateBusinessCenterTime()
        {
            BusinessCenterTime bct = new BusinessCenterTime();
            bct.businessCenter = new BusinessCenter();
            bct.hourMinuteTime = new HourMinuteTime();
            return bct;
        }
        IBusinessCenterTime IFxFixing.CreateBusinessCenterTime(IBusinessCenterTime pBusinessCenterTime)
        {
            BusinessCenterTime bct = new BusinessCenterTime();
            bct.businessCenter = new BusinessCenter(pBusinessCenterTime.businessCenter.Value);
            bct.hourMinuteTime = new HourMinuteTime(pBusinessCenterTime.hourMinuteTime.Value);
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
        IQuotedCurrencyPair IFxRate.quotedCurrencyPair
        {
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
            get { return this.quotedCurrencyPair; }
        }
        EFS_Decimal IFxRate.rate
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
        IInformationSource IFxSpotRateSource.primaryRateSource
        {
            set { this.primaryRateSource = (InformationSource)value; }
            get { return this.primaryRateSource; }
        }
        bool IFxSpotRateSource.secondaryRateSourceSpecified
        {
            set { this.secondaryRateSourceSpecified = value; }
            get { return this.secondaryRateSourceSpecified; }
        }
        IInformationSource IFxSpotRateSource.secondaryRateSource
        {
            set { this.secondaryRateSource = (InformationSource)value; }
            get { return this.secondaryRateSource; }
        }
        IBusinessCenterTime IFxSpotRateSource.fixingTime
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
            if (fixing.secondaryRateSourceSpecified)
                fixing.secondaryRateSource = (InformationSource)this.secondaryRateSource.Clone();
            fixing.fixingDate = new EFS_Date();
            fixing.fixingDate.DateValue = pFixingDate;
            fixing.fixingTime = this.fixingTime;
            return fixing;
        }
        IInformationSource IFxSpotRateSource.CreateInformationSource
        {
            get
            {
                InformationSource informationSource = new InformationSource();
                informationSource.rateSource = new InformationProvider();
                return informationSource;
            }
        }
        #endregion IFxSpotRateSource Members
    }
    #endregion FxSpotRateSource

    #region GoverningLaw
    public partial class GoverningLaw : IScheme
    {
        #region Constructors
        public GoverningLaw()
        {
            governingLawScheme = "http://www.fpml.org/spec/2002/governing-law-1-0";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.scheme
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
    }
    #endregion GoverningLaw

    #region HourMinuteTime
    public partial class HourMinuteTime : IHourMinuteTime
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.DateTime TimeValue
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
        public IdentifiedCurrency()
        {
            currencyScheme = "http://www.fpml.org/ext/iso4217-2001-08-15";
        }
        #endregion Constructors

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
    #region IdentifierDate
    public partial class IdentifierDate : ICloneable, IAdjustedDate
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.DateTime DateValue
        {
            get { return new DtFunc().StringToDateTime(this.Value, DtFunc.FmtISODate); }
            set { this.Value = DtFunc.DateTimeToString(value, DtFunc.FmtISODate); }
        }
        #endregion Accessors

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
        string IAdjustedDate.id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        #endregion IAdjustedDate Members
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            IdentifierDate clone = new IdentifierDate();
            clone.DateValue = this.DateValue;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion IdentifierDate
    #region InformationProvider
    public partial class InformationProvider : ICloneable, IScheme
    {
        #region Constructors
        public InformationProvider()
        {
            informationProviderScheme = "http://www.fpml.org/spec/2003/information-provider-2-0";
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            InformationProvider clone = new InformationProvider();
            clone.informationProviderScheme = this.informationProviderScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.scheme
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
        #endregion
        #endregion Methods

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            InformationSource clone = new InformationSource();
            clone.assetFxRateId = (AssetFxRateId)this.assetFxRateId.Clone();
            clone.rateSource = (InformationProvider)this.rateSource.Clone();
            clone.rateSourcePageSpecified = this.rateSourcePageSpecified;
            clone.rateSourcePageHeadingSpecified = this.rateSourcePageHeadingSpecified;
            clone.otcmlId = this.otcmlId;
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
        IAssetFxRateId IInformationSource.assetFxRateId
        {
            get { return this.assetFxRateId; }
        }
        int IInformationSource.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        IScheme IInformationSource.rateSource
        {
            set { this.rateSource = (InformationProvider)value; }
            get { return this.rateSource; }
        }
        bool IInformationSource.rateSourcePageSpecified
        {
            set { this.rateSourcePageSpecified = value; }
            get { return this.rateSourcePageSpecified; }
        }
        IScheme IInformationSource.rateSourcePage
        {
            set { this.rateSourcePage = (RateSourcePage)value; }
            get { return this.rateSourcePage; }
        }
        bool IInformationSource.rateSourcePageHeadingSpecified
        {
            set { this.rateSourcePageHeadingSpecified = value; }
            get { return this.rateSourcePageHeadingSpecified; }
        }
        string IInformationSource.rateSourcePageHeading
        {
            set { this.rateSourcePageHeading = new EFS_String(value); }
            get { return this.rateSourcePageHeading.Value; }
        }
        void IInformationSource.CreateRateSourcePage(string pRateSourcePage)
        {
            this.rateSourcePage = new RateSourcePage();
            this.rateSourcePage.Value = pRateSourcePage;
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
        string IScheme.scheme
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
    }
    #endregion InstrumentId

    #region InterestAccrualsMethod
    // EG 20140702 Upd Interface
    // EG 20170510 [23153] Add SetAsset 
    public partial class InterestAccrualsMethod : IInterestAccrualsMethod
    {
        // Variable de travail
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_AssetBase _sql_Asset;

        #region Constructors
        public InterestAccrualsMethod()
        {
            interestAccrualsMethodFloatingRate = new FloatingRateCalculation();
            interestAccrualsMethodFixedRate = new EFS_Decimal();
        }
        #endregion Constructors

        #region IInterestAccrualsMethod Members
        bool IInterestAccrualsMethod.floatingRateSpecified
        {
            set { this.interestAccrualsMethodFloatingRateSpecified = value; }
            get { return this.interestAccrualsMethodFloatingRateSpecified; }
        }
        IFloatingRateCalculation IInterestAccrualsMethod.floatingRate
        {
            set { this.interestAccrualsMethodFloatingRate = (FloatingRateCalculation)value; }
            get { return this.interestAccrualsMethodFloatingRate; }
        }
        bool IInterestAccrualsMethod.fixedRateSpecified
        {
            set { this.interestAccrualsMethodFixedRateSpecified = value; }
            get { return this.interestAccrualsMethodFixedRateSpecified; }
        }
        EFS_Decimal IInterestAccrualsMethod.fixedRate
        {
            set { this.interestAccrualsMethodFixedRate = value; }
            get { return this.interestAccrualsMethodFixedRate; }
        }
        bool IInterestAccrualsMethod.sqlAssetSpecified
        {
            get { return (null != this._sql_Asset); }
        }
        SQL_AssetBase IInterestAccrualsMethod.sqlAsset
        {
            set { this._sql_Asset = value; }
            get { return this._sql_Asset; }
        }
        // EG 20170510 [23153]
        void IInterestAccrualsMethod.SetAsset(string pCS, IDbTransaction pDbTransaction)
        {
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
        #endregion

        #region ICloneable members
        public object Clone()
        {
            IntermediaryInformation clone = new IntermediaryInformation();
            clone = (IntermediaryInformation)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable members

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
        #endregion Methods
        #region IRouting Membres
        bool IRouting.routingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.routingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.routingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.routingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.routingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.routingIdsAndExplicitDetails
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
        EFS_PosInteger IIntermediaryInformation.intermediarySequenceNumber
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
            Interval clone = new Interval();
            clone.periodMultiplier = (EFS_Integer)this.periodMultiplier.Clone();
            clone.period = this.period;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IComparable Members
        #region CompareTo
        /// <summary>
        /// Retourne 0 si la periode et le multiplier sont identiques, -1 sinon 
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            int ret = -1;
            Interval interval = pObj as Interval;
            if ((null != pObj) && (period == interval.period) && (periodMultiplier.IntValue == interval.periodMultiplier.IntValue))
                ret = 0;
            return ret;
        }
        #endregion CompareTo
        #endregion IComparable Members
        #region IInterval Members
        PeriodEnum IInterval.period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IInterval.periodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod) { return new Interval(pPeriod.ToString(), pMultiplier); }
        IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision) { return new Rounding(pRoundingDirection, pPrecision); }
        int IInterval.CompareTo(object obj) { return this.CompareTo(obj); }
        #endregion IInterval Members
    }
    #endregion Interval

    #region MasterAgreement
    public partial class MasterAgreement : IMasterAgreement
    {

        #region IMasterAgreement Membres
        IScheme IMasterAgreement.masterAgreementType
        {
            get { return this.masterAgreementType; }
            set { this.masterAgreementType = (MasterAgreementType)value; }
        }

        bool IMasterAgreement.masterAgreementDateSpecified
        {
            get { return this.masterAgreementDateSpecified; }
            set { this.masterAgreementDateSpecified = value; }
        }
        EFS_Date IMasterAgreement.masterAgreementDate
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
        string IScheme.scheme
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



    #region ManualExercise
    public partial class ManualExercise : IManualExercise
    {
        #region IManualExercise Members
        bool IManualExercise.exerciseNoticeSpecified { get { return this.exerciseNoticeSpecified; } }
        bool IManualExercise.fallbackExerciseSpecified { get { return this.fallbackExerciseSpecified; } }
        bool IManualExercise.fallbackExercise { get { return this.fallbackExercise.BoolValue; } }
        #endregion IManualExercise Members
    }
    #endregion ManualExercise
    #region Money
    public partial class Money : ICloneable, IMoney
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
            Money clone = new Money();
            clone.currency = (Currency)this.currency.Clone();
            clone.amount = new EFS_Decimal(amount.DecValue);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IMoney Members
        #region Accessors
        EFS_Decimal IMoney.amount
        {
            set { this.amount = value; }
            get { return this.amount; }
        }
        string IMoney.currency
        {
            set { this.currency.Value = value; }
            get { return this.currency.Value; }
        }
        IOffset IMoney.DefaultOffsetPreSettlement { get { return new Offset(PeriodEnum.D, -2, DayTypeEnum.Business); } }
        IOffset IMoney.DefaultOffsetUsanceDelaySettlement { get { return new Offset(PeriodEnum.D, 2, DayTypeEnum.Business); } }
        ICurrency IMoney.Currency
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
    }
    #endregion Money
    #region MultipleExercise
    public partial class MultipleExercise : IMultipleExercise
    {
        #region IMultipleExercise Members
        bool IMultipleExercise.maximumNumberOfOptionsSpecified
        {
            get { return false; }
        }
        EFS_PosInteger IMultipleExercise.maximumNumberOfOptions
        {
            get { return null; }
        }
        bool IMultipleExercise.maximumNotionalAmountSpecified
        {
            get { return this.maximumNotionalAmountSpecified; }
        }
        EFS_Decimal IMultipleExercise.maximumNotionalAmount
        {
            get { return this.maximumNotionalAmount; }
        }
        #endregion IMultipleExercise Members
        #region IPartialExercise Members
        bool IPartialExercise.notionalReferenceSpecified
        {
            set { this.notionalReferenceSpecified = value; }
            get { return this.notionalReferenceSpecified; }
        }
        IReference[] IPartialExercise.notionalReference
        {
            set { this.notionalReference = (ExerciseNotionalReference[])value; }
            get { return this.notionalReference; }
        }
        bool IPartialExercise.integralMultipleAmountSpecified
        {
            set { this.integralMultipleAmountSpecified = value; }
            get { return this.integralMultipleAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.integralMultipleAmount
        {
            set { this.integralMultipleAmount = value; }
            get { return this.integralMultipleAmount; }
        }
        bool IPartialExercise.minimumNumberOfOptionsSpecified
        {
            set { ; }
            get { return false; }
        }
        EFS_PosInteger IPartialExercise.minimumNumberOfOptions
        {
            set { ; }
            get { return null; }
        }
        bool IPartialExercise.minimumNotionalAmountSpecified
        {
            set { ; }
            get { return true; }
        }
        EFS_Decimal IPartialExercise.minimumNotionalAmount
        {
            set { this.minimumNotionalAmount = value; }
            get { return this.minimumNotionalAmount; }
        }
        #endregion IPartialExercise Members
    }
    #endregion MultipleExercise

    #region NotionalReference
    public partial class NotionalReference : IReference
    {
        #region Constructors
        public NotionalReference()
        {
            href = string.Empty;
        }
        #endregion Constructors

        #region IReference Members
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion NotionalReference

    #region Offset
    public partial class Offset : ICloneable, IOffset
    {
        #region Accessors
        #region Id
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
        #endregion Id
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
            period = (PeriodEnum)StringToEnum.Period(pPeriod);
            dayType = (DayTypeEnum)StringToEnum.DayType(pDayType);
            dayTypeSpecified = true;
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            Offset clone = new Offset();
            clone.periodMultiplier = (EFS_Integer)this.periodMultiplier.Clone();
            clone.period = this.period;
            clone.dayTypeSpecified = this.dayTypeSpecified;
            clone.dayType = this.dayType;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IOffset Members
        bool IOffset.dayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IOffset.dayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        /// <summary>
        /// Retourne les business Centers associées au devises {pCurrencies}
        ///  <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrencies"></param>
        /// <returns></returns>
        /// FI 20131118 [19118] usage de ArrFunc.IsFilled
        // EG 20180307 [23769] Gestion dbTransaction
        IBusinessCenters IOffset.GetBusinessCentersCurrency(string pConnectionString, IDbTransaction pDbTransaction, params string[] pCurrencies)
        {
            BusinessCenters ret = null;
            DataRowCollection rows = Tools.LoadBusinessCenter(pConnectionString, pDbTransaction, pCurrencies, null, null);
            if (ArrFunc.IsFilled(rows))
            //if (0 < rows.Count)
            {
                BusinessCenter[] businessCenter = new BusinessCenter[rows.Count];
                int i = 0;
                foreach (DataRow row in rows)
                {
                    if (0 < row[0].ToString().Length)
                    {
                        businessCenter[i] = new BusinessCenter();
                        businessCenter[i].Value = row[0].ToString();
                        i++;
                    }
                }
                ret = new BusinessCenters(businessCenter);
            }
            return ret;
        }
        IBusinessDayAdjustments IOffset.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments();
            bda.businessDayConvention = pBusinessDayConvention;
            if (ArrFunc.IsFilled(pIdBC))
            {
                BusinessCenters bcs = new BusinessCenters();
                foreach (string idBC in pIdBC)
                {
                    if (StrFunc.IsFilled(idBC))
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
        PeriodEnum IInterval.period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IInterval.periodMultiplier
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
            return ((IInterval)this).CompareTo(obj);
        }
        #endregion
    }
    #endregion Offset

    #region PartialExercise
    public partial class PartialExercise : IPartialExercise
    {
        #region IPartialExercise Members
        bool IPartialExercise.notionalReferenceSpecified
        {
            set { this.notionalReferenceSpecified = value; }
            get { return this.notionalReferenceSpecified; }
        }
        IReference[] IPartialExercise.notionalReference
        {
            set { this.notionalReference = (ExerciseNotionalReference[])value; }
            get { return this.notionalReference; }
        }
        bool IPartialExercise.integralMultipleAmountSpecified
        {
            set { this.integralMultipleAmountSpecified = value; }
            get { return this.integralMultipleAmountSpecified; }
        }
        EFS_Decimal IPartialExercise.integralMultipleAmount
        {
            set { this.integralMultipleAmount = value; }
            get { return this.integralMultipleAmount; }
        }
        bool IPartialExercise.minimumNumberOfOptionsSpecified
        {
            set { ; }
            get { return false; }
        }
        EFS_PosInteger IPartialExercise.minimumNumberOfOptions
        {
            set { ; }
            get { return null; }
        }
        bool IPartialExercise.minimumNotionalAmountSpecified
        {
            set { ; }
            get { return true; }
        }
        EFS_Decimal IPartialExercise.minimumNotionalAmount
        {
            set { this.minimumNotionalAmount = value; }
            get { return this.minimumNotionalAmount; }
        }
        #endregion IPartialExercise Members
    }
    #endregion PartialExercise
    #region PartyReference
    public partial class PartyReference : IReference
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
            PartyReference clone = new PartyReference();
            clone.href = this.href;
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.hRef
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
        #region Accessors
        #region AdjustedPaymentDate
        // EG 20180423 Analyse du code Correction [CA2200]        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get{ return this.efs_Payment.AdjustedPaymentDate; }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get{ return this.efs_Payment.AdjustedPreSettlementDate; }
        }
        #endregion AdjustedPreSettlementDate
        #region ExpirationDate
        // EG 20180423 Analyse du code Correction [CA2200]
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
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeRate ExchangeRate
        {
            get{ return efs_Payment.ExchangeRate; }
        }
        #endregion ExchangeRate
        #region Invoicing_AdjustedPaymentDate
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date Invoicing_AdjustedPaymentDate
        {
            get{ return this.efs_Payment.Invoicing_AdjustedPaymentDate; }
        }
        #endregion Invoicing_AdjustedPaymentDate
        #region PayerPartyReference
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get{ return this.payerPartyReference.href; }
        }
        #endregion PayerPartyReference
        #region PaymentAmount
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PaymentAmount
        {
            get{ return this.efs_Payment.paymentAmount.amount; }
        }
        #endregion PaymentAmount
        #region PaymentQuote
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentQuote PaymentQuote
        {
            get{ return efs_Payment.PaymentQuote; }
        }
        #endregion PaymentQuote
        #region PaymentSource
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentSource PaymentSource
        {
            get{ return efs_Payment.PaymentSource; }
        }
        #endregion PaymentSource
        #region PaymentCurrency
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PaymentCurrency
        {
            get
            {
                if (this.efs_Payment == null)
                {
                    return this.paymentAmount.currency.Value;
                }
                else
                {
                    return this.efs_Payment.paymentAmount.currency;
                }
            }
        }
        #endregion PaymentCurrency
        #region PaymentDate
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get{ return this.efs_Payment.PaymentDate; }
        }
        #endregion PaymentDate
        #region PaymentSettlementCurrency
        // EG 20180423 Analyse du code Correction [CA2200]
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
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get{ return receiverPartyReference.href; }
        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        public Payment()
        {
            adjustedPaymentDate = new EFS_Date();
            payerPartyReference = new PartyReference();
            receiverPartyReference = new PartyReference();
            settlementInformation = new SettlementInformation();
            paymentDate = new AdjustableDate();
            paymentType = new PaymentType();
            paymentAmount = new Money();
            paymentSource = new SpheresSource();
        }
        #endregion Constructors
        #region Methods
        #region PaymentType
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PaymentType(string pEventCode)
        {
            try
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
                    // NORMALEMENT ON NE VAS PLUS DANS CE PAVE
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
            catch (Exception) { throw; }
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
        IReference IPayment.payerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyReference)value; }
        }
        IReference IPayment.receiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyReference)value; }
        }
        IMoney IPayment.paymentAmount
        {
            get { return this.paymentAmount; }
        }
        bool IPayment.paymentDateSpecified
        {
            set { this.paymentDateSpecified = value; }
            get { return this.paymentDateSpecified; }
        }
        // EG 20101020 Ticket:17185
        IAdjustableDate IPayment.paymentDate
        {
            set { this.paymentDate = (AdjustableDate)value; }
            get { return this.paymentDate; }
        }
        bool IPayment.adjustedPaymentDateSpecified
        {
            set { this.adjustedPaymentDateSpecified = value; }
            get { return this.adjustedPaymentDateSpecified; }
        }
        DateTime IPayment.adjustedPaymentDate
        {
            set { this.adjustedPaymentDate.DateValue = value; }
            get { return this.adjustedPaymentDate.DateValue; }
        }
        bool IPayment.paymentQuoteSpecified
        {
            set { this.paymentQuoteSpecified = value; }
            get { return this.paymentQuoteSpecified; }
        }
        IPaymentQuote IPayment.paymentQuote
        {
            set { this.paymentQuote = (PaymentQuote)value; }
            get { return this.paymentQuote; }
        }
        bool IPayment.customerSettlementPaymentSpecified
        {
            set { this.customerSettlementPaymentSpecified = value; }
            get { return this.customerSettlementPaymentSpecified; }
        }
        ICustomerSettlementPayment IPayment.customerSettlementPayment
        {
            set { this.customerSettlementPayment = (CustomerSettlementPayment)value; }
            get { return this.customerSettlementPayment; }
        }
        bool IPayment.paymentSourceSpecified
        {
            set { this.paymentSourceSpecified = value; }
            get { return this.paymentSourceSpecified; }
        }
        ISpheresSource IPayment.paymentSource
        {
            set { this.paymentSource = (SpheresSource)value; }
            get { return this.paymentSource; }
        }
        bool IPayment.taxSpecified
        {
            set { }
            get { return false; }
        }
        ITax[] IPayment.tax
        {
            set { }
            get { return null; }
        }
        string IPayment.PaymentCurrency
        {
            //KKK set { this.PaymentCurrency = value; }
            get { return this.PaymentCurrency; }
        }
        string IPayment.PaymentSettlementCurrency { get { return this.PaymentSettlementCurrency; } }
        IAdjustedDate IPayment.CreateAdjustedDate(DateTime pAdjustedDate)
        {
            IdentifierDate adjustedDate = new IdentifierDate();
            adjustedDate.DateValue = pAdjustedDate;
            return adjustedDate;
        }
        IAdjustedDate IPayment.CreateAdjustedDate(string pAdjustedDate)
        {
            IdentifierDate adjustedDate = new IdentifierDate();
            adjustedDate.Value = pAdjustedDate;
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
                    notionalAmount = new Money(notionalReference.notionalStepSchedule.initialValue.DecValue, notionalReference.notionalStepSchedule.currency.Value);
                }
            }

            return notionalAmount;
        }
        EFS_Payment IPayment.efs_Payment
        {
            get { return this.efs_Payment; }
            set { efs_Payment = value; }
        }
        bool IPayment.settlementInformationSpecified
        {
            set { this.settlementInformationSpecified = value; }
            get { return this.settlementInformationSpecified; }
        }
        ISettlementInformation IPayment.settlementInformation
        {
            get { return this.settlementInformation; }
        }
        ICustomerSettlementPayment IPayment.CreateCustomerSettlementPayment { get { return new CustomerSettlementPayment(); } }
        bool IPayment.paymentTypeSpecified
        {
            set { this.paymentTypeSpecified = value; }
            get { return this.paymentTypeSpecified; }
        }

        IScheme IPayment.paymentType
        {
            set { this.paymentType = (PaymentType)value; }
            get { return this.paymentType; }
        }
        IPaymentQuote IPayment.CreatePaymentQuote { get { return new PaymentQuote(); } }
        IReference IPayment.CreateReference { get { return new Reference(); } }
        string IPayment.GetPaymentType(string pEventCode)
        {
            return this.PaymentType(pEventCode);
        }
        ITax IPayment.CreateTax { get { return null; } }
        ITaxSchedule IPayment.CreateTaxSchedule { get { return null; } }
        ISpheresSource IPayment.CreateSpheresSource { get { return new SpheresSource(); } }
        IPayment IPayment.DeleteMatchPayment(ArrayList pPayment) { return null; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180328 [23871] Add 
        string IPayment.id
        {
            set { ; }
            get { return null; }
        }

        #endregion IPayment Members
    }
    #endregion Payment
    #region PaymentCurrency
    public partial class PaymentCurrency : IPaymentCurrency
    {
        #region Constructors
        public PaymentCurrency()
        {
            paymentCurrencyReference = new EFS_Href();
            paymentCurrencyCurrency = new Currency();
            paymentCurrencyDeterminationMethod = new EFS_MultiLineString();
        }
        #endregion Constructors

        #region IPaymentCurrency Members
        bool IPaymentCurrency.referenceSpecified
        {
            get { return this.paymentCurrencyReferenceSpecified; }
        }
        string IPaymentCurrency.reference
        {
            get { return this.paymentCurrencyReference.Value; }
        }
        bool IPaymentCurrency.currencySpecified
        {
            get { return this.paymentCurrencyCurrencySpecified; }
        }
        ICurrency IPaymentCurrency.currency
        {
            get { return this.paymentCurrencyCurrency; }
        }
        bool IPaymentCurrency.currencydeterminationMethodSpecified
        {
            get { return this.paymentCurrencyDeterminationMethodSpecified; }
        }
        string IPaymentCurrency.currencyDeterminationMethodValue
        {
            get { return this.paymentCurrencyDeterminationMethod.Value; }
        }
        IScheme IPaymentCurrency.currencyDeterminationMethod
        {
            get { return null; }
        }
        #endregion IPaymentCurrency Members
    }
    #endregion PaymentCurrency
    #region PaymentType
    public partial class PaymentType : IScheme
    {
        #region IScheme Members
        string IScheme.scheme
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
        IAdjustableOrRelativeDate IPeriodicDates.calculationStartDate
        {
            get { return this.calculationStartDate; }
        }
        bool IPeriodicDates.calculationEndDateSpecified
        {
            get { return this.calculationEndDateSpecified; }
        }
        IAdjustableOrRelativeDate IPeriodicDates.calculationEndDate
        {
            get { return this.calculationEndDate; }
        }
        ICalculationPeriodFrequency IPeriodicDates.calculationPeriodFrequency
        {
            get { return this.calculationPeriodFrequency; }
        }
        IBusinessDayAdjustments IPeriodicDates.calculationPeriodDatesAdjustments
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

        #region IPrincipalExchanges Members
        EFS_Boolean IPrincipalExchanges.initialExchange
        {
            set { this.initialExchange = value; }
            get { return this.initialExchange; }
        }
        EFS_Boolean IPrincipalExchanges.finalExchange
        {
            set { this.finalExchange = value; }
            get { return this.finalExchange; }
        }
        EFS_Boolean IPrincipalExchanges.intermediateExchange
        {
            set { this.intermediateExchange = value; }
            get { return this.intermediateExchange; }
        }
        #endregion IPrincipalExchanges Members
    }
    #endregion PrincipalExchanges
    #region Product
    // EG 20140702 Upd Interface
    // EG 20150317 [POC] Add IsFungible|IsMargining|IsMarginingAndNotFungible
    // EG 20150422 [20513] BANCAPERTA New 
    // EG 20171025 [23509] CreateTradeProcessingTimestamps
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
        bool IProductBase.IsDebtSecurity { get { return false; } }
        bool IProductBase.IsDebtSecurityTransaction { get { return false; } }
        bool IProductBase.IsEquityForward { get { return this.GetType().Equals(typeof(EquityForward)); } }
        bool IProductBase.IsEquityOption { get { return this.GetType().Equals(typeof(EquityOption)); } }
        bool IProductBase.IsEquityOptionTransactionSupplement { get { return this.GetType().Equals(typeof(EquityOptionTransactionSupplement)); } }
        bool IProductBase.IsEquitySecurityTransaction { get { return false; } }
        bool IProductBase.IsExchangeTradedDerivative { get { return false; } }
        bool IProductBase.IsFra { get { return this.GetType().Equals(typeof(Fra)); } }
        bool IProductBase.IsFutureTransaction { get { return false; } }
        bool IProductBase.IsFxAverageRateOption { get { return this.GetType().Equals(typeof(FxAverageRateOption)); } }
        bool IProductBase.IsFxBarrierOption { get { return this.GetType().Equals(typeof(FxBarrierOption)); } }
        bool IProductBase.IsFxDigitalOption { get { return this.GetType().Equals(typeof(FxDigitalOption)); } }
        bool IProductBase.IsFxLeg { get { return this.GetType().Equals(typeof(FxLeg)); } }
        bool IProductBase.IsFxOptionLeg { get { return this.GetType().Equals(typeof(FxOptionLeg)); } }
        bool IProductBase.IsFxSwap { get { return this.GetType().Equals(typeof(FxSwap)); } }
        bool IProductBase.IsFxTermDeposit { get { return this.GetType().Equals(typeof(TermDeposit)); } }
        bool IProductBase.IsInvoice { get { return false; } }
        bool IProductBase.IsInvoiceSettlement { get { return false; } }
        bool IProductBase.IsAdditionalInvoice { get { return false; } }
        bool IProductBase.IsCreditNote { get { return false; } }
        bool IProductBase.IsLoanDeposit { get { return false; } }
        bool IProductBase.IsReturnSwap { get { return this.GetType().Equals(typeof(EquitySwap)); } }
        bool IProductBase.IsEquitySwapTransactionSupplement { get { return this.GetType().Equals(typeof(EquitySwapTransactionSupplement)); } }
        bool IProductBase.IsSwap { get { return this.GetType().Equals(typeof(Swap)); } }
        bool IProductBase.IsSwaption { get { return this.GetType().Equals(typeof(Swaption)); } }
        bool IProductBase.IsSaleAndRepurchaseAgreement { get { return false; } }
        bool IProductBase.IsRepo { get { return false; } }
        bool IProductBase.IsSecurityLending { get { return false; } }
        bool IProductBase.IsBuyAndSellBack { get { return false; } }
        bool IProductBase.IsBondTransaction { get { return false; } }
        bool IProductBase.IsMarginRequirement { get { return false; } }
        bool IProductBase.IsCashBalance { get { return false; } }
        bool IProductBase.IsCashBalanceInterest { get { return false; } }
        // EG 20131118 New
        bool IProductBase.IsCashPayment { get { return false; } }
        bool IProductBase.IsVarianceSwapTransactionSupplement { get { return false; } }
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
                       ((IProductBase)this).IsFra || ((IProductBase)this).IsSwap || ((IProductBase)this).IsSwaption;
            }
        }
        bool IProductBase.IsADM
        {
            get
            {
                return false;
            }
        }
        bool IProductBase.IsASSET
        {
            get
            {
                return false;
            }
        }
        bool IProductBase.IsSEC
        {
            get
            {
                return false;
            }
        }
        bool IProductBase.IsSecurityTransaction
        {
            get
            {
                return false;
            }
        }
        bool IProductBase.IsLSD
        {
            get
            {
                return false;
            }
        }

        bool IProductBase.IsRISK
        {
            get
            {
                return false;
            }
        }
        // EG 20150410 [20513] BANCAPERTA
        bool IProductBase.IsBondOption
        {
            get
            {
                return false;
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
        bool IProductBase.IsESE
        {
            get
            {
                return false;
            }
        }
        bool IProductBase.IsDSE
        {
            get
            {
                return false;
            }
        }
        /// EG 20150317 [POC] New
        bool IProductBase.IsFungible(string pCS)
        {
            return false;
        }
        /// EG 20150317 [POC] New
        bool IProductBase.IsMargining(string pCS)
        {
            return false;
        }
        /// EG 20150317 [POC] New
        bool IProductBase.IsMarginingAndNotFungible(string pCS)
        {
            return false;
        }


        //
        string IProductBase.ProductName { get { return this.GetType().Name; } }
        IProductType IProductBase.productType
        {
            set { this.productType = (ProductType)value; }
            get { return this.productType; }
        }
        string IProductBase.id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        Type IProductBase.TypeofLoadDepositStream { get { return null; } }
        Type IProductBase.TypeofStream { get { return new InterestRateStream().GetType(); } }
        Type IProductBase.TypeofFxOptionPremium { get { return new FxOptionPremium().GetType(); } }
        Type IProductBase.TypeofPayment { get { return new Payment().GetType(); } }
        Type IProductBase.TypeofPaymentDates { get { return new PaymentDates().GetType(); } }
        Type IProductBase.TypeofSchedule { get { return new Schedule().GetType(); } }
        Type IProductBase.TypeofEquityPremium { get { return new EquityPremium().GetType(); } }
        Type IProductBase.TypeofReturnLeg { get { return new EquityLeg().GetType(); } }
        Type IProductBase.TypeofInterestLeg { get { return new InterestLeg().GetType(); } }
        Type IProductBase.TypeofCurrency { get { return new Currency().GetType(); } }
        Type IProductBase.TypeofBusinessCenter { get { return new BusinessCenter().GetType(); } }
        Type IProductBase.TypeofPhysicalLeg { get { return null; } }
        Type IProductBase.TypeofGasPhysicalLeg { get { return null; } }
        Type IProductBase.TypeofElectricityPhysicalLeg { get { return null; } }
        Type IProductBase.TypeofFinancialLeg { get { return null; } }
        Type IProductBase.TypeofFixedPriceSpotLeg { get { return null; } }
        Type IProductBase.TypeofFixedPriceLeg { get { return null; } }
        Type IProductBase.TypeofEnvironmentalPhysicalLeg { get { return null; } }


        IImplicitProvision IProductBase.implicitProvision
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).implicitProvision;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).implicitProvision;
                else
                    return null;

            }
            set
            {
                ImplicitProvision implicitProvision = (ImplicitProvision)value;
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).implicitProvision = implicitProvision;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).implicitProvision = implicitProvision;
            }
        }
        bool IProductBase.implicitProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).implicitProvisionSpecified;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).implicitProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).implicitProvisionSpecified = value;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).implicitProvisionSpecified = value;
            }
        }
        bool IProductBase.implicitEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.implicitProvisionSpecified &&
                       (productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified ||
                        productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified);
            }
        }
        bool IProductBase.implicitCancelableProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.implicitProvisionSpecified &&
                       productBase.implicitProvision.cancelableProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.implicitProvisionSpecified)
                    {
                        productBase.implicitProvisionSpecified = value;
                        productBase.implicitProvision = new ImplicitProvision();
                    }
                    productBase.implicitProvision.cancelableProvisionSpecified = value;
                    productBase.implicitProvision.cancelableProvision = new Empty();
                }
                else if (productBase.implicitProvisionSpecified)
                {
                    productBase.implicitProvision.cancelableProvisionSpecified = value;
                    productBase.implicitProvisionSpecified = productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.extendibleProvisionSpecified ||
                                                             productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.stepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.implicitExtendibleProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.implicitProvisionSpecified &&
                       productBase.implicitProvision.extendibleProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.implicitProvisionSpecified)
                    {
                        productBase.implicitProvisionSpecified = value;
                        productBase.implicitProvision = new ImplicitProvision();
                    }
                    productBase.implicitProvision.extendibleProvisionSpecified = value;
                    productBase.implicitProvision.extendibleProvision = new Empty();
                }
                else if (productBase.implicitProvisionSpecified)
                {
                    productBase.implicitProvision.extendibleProvisionSpecified = value;
                    productBase.implicitProvisionSpecified = productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.cancelableProvisionSpecified ||
                                                             productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.stepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.implicitMandatoryEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.implicitProvisionSpecified &&
                       productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.implicitProvisionSpecified)
                    {
                        productBase.implicitProvisionSpecified = value;
                        productBase.implicitProvision = new ImplicitProvision();
                    }
                    productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified = value;
                    productBase.implicitProvision.mandatoryEarlyTerminationProvision = new Empty();
                }
                else if (productBase.implicitProvisionSpecified)
                {
                    productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified = value;
                    productBase.implicitProvisionSpecified = productBase.implicitProvision.cancelableProvisionSpecified ||
                                                             productBase.implicitProvision.extendibleProvisionSpecified ||
                                                             productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.stepUpProvisionSpecified;
                }
            }
        }
        bool IProductBase.implicitOptionalEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                return productBase.implicitProvisionSpecified &&
                       productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (value)
                {
                    if (false == productBase.implicitProvisionSpecified)
                    {
                        productBase.implicitProvisionSpecified = value;
                        productBase.implicitProvision = new ImplicitProvision();
                    }
                    productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified = value;
                    productBase.implicitProvision.optionalEarlyTerminationProvision = new Empty();
                }
                else if (productBase.implicitProvisionSpecified)
                {
                    productBase.implicitProvision.optionalEarlyTerminationProvisionSpecified = value;
                    productBase.implicitProvisionSpecified = productBase.implicitProvision.cancelableProvisionSpecified ||
                                                             productBase.implicitProvision.extendibleProvisionSpecified ||
                                                             productBase.implicitProvision.mandatoryEarlyTerminationProvisionSpecified ||
                                                             productBase.implicitProvision.stepUpProvisionSpecified;
                }
            }
        }

        bool IProductBase.earlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).earlyTerminationProvisionSpecified;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).earlyTerminationProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).earlyTerminationProvisionSpecified = value;
                else if (productBase.IsCapFloor)
                    ((CapFloor)productBase).earlyTerminationProvisionSpecified = value;
            }
        }
        IEarlyTerminationProvision IProductBase.earlyTerminationProvision
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).earlyTerminationProvision;
                else if (productBase.IsCapFloor)
                    return ((CapFloor)productBase).earlyTerminationProvision;
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
            }
        }
        bool IProductBase.cancelableProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).cancelableProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).cancelableProvisionSpecified = value;
            }

        }
        ICancelableProvision IProductBase.cancelableProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).cancelableProvision = (CancelableProvision)value;
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).cancelableProvision;
                return null;
            }
        }
        bool IProductBase.extendibleProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).extendibleProvisionSpecified;
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).extendibleProvisionSpecified = value;
            }

        }
        IExtendibleProvision IProductBase.extendibleProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    ((Swap)productBase).extendibleProvision = (ExtendibleProvision)value;
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.IsSwap)
                    return ((Swap)productBase).extendibleProvision;
                return null;
            }
        }
        bool IProductBase.optionalEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified;
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptionalSpecified = value;
                }
            }

        }
        IOptionalEarlyTermination IProductBase.optionalEarlyTerminationProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    OptionalEarlyTermination optionalEarlyTermination = (OptionalEarlyTermination)value;
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptional = optionalEarlyTermination;
                }
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.optionalEarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationOptional;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationOptional;
                }
                return null;
            }
        }
        bool IProductBase.mandatoryEarlyTerminationProvisionSpecified
        {
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified;
                }
                return false;
            }
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified = value;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatorySpecified = value;
                }
            }
        }
        IMandatoryEarlyTermination IProductBase.mandatoryEarlyTerminationProvision
        {
            set
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.earlyTerminationProvisionSpecified)
                {
                    MandatoryEarlyTermination mandatoryEarlyTermination = (MandatoryEarlyTermination)value;
                    if (productBase.IsSwap)
                        ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatory = mandatoryEarlyTermination;
                    else if (productBase.IsCapFloor)
                        ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatory = mandatoryEarlyTermination;
                }
            }
            get
            {
                IProductBase productBase = (IProductBase)this;
                if (productBase.mandatoryEarlyTerminationProvisionSpecified)
                {
                    if (productBase.IsSwap)
                        return ((Swap)productBase).earlyTerminationProvision.earlyTerminationMandatory;
                    else if (productBase.IsCapFloor)
                        return ((CapFloor)productBase).earlyTerminationProvision.earlyTerminationMandatory;
                }
                return null;
            }
        }

        IPartyRole IProductBase.CreatePartyRole() { return new PartyRole(); }
        ICurrency IProductBase.CreateCurrency(string pCurrency) { return new Currency(pCurrency); }
        IDebtSecurity IProductBase.CreateDebtSecurity() { return null; }
        IDebtSecurityStream[] IProductBase.CreateDebtSecurityStreams(int pDim) { return null; }
        IDebtSecurityTransaction IProductBase.CreateDebtSecurityTransaction() { return null; }
        IFxOptionLeg IProductBase.CreateFxOptionLeg() { return new FxOptionLeg(); }
        IExchangeTradedDerivative IProductBase.CreateExchangeTradedDerivative() { return null; }
        IEquitySecurityTransaction IProductBase.CreateEquitySecurityTransaction() { return null; }
        IReturnSwap IProductBase.CreateReturnSwap() { return null; }
        IPayment IProductBase.CreatePayment() { return new Payment(); }
        IPriceUnits IProductBase.CreatePriceUnits() { return null; }
        IMoney IProductBase.CreateMoney() { return new Money(); }
        IMoney IProductBase.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        IBusinessDayAdjustments IProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments();
            bda.businessDayConvention = pBusinessDayConvention;
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
        IAdjustableOffset IProductBase.CreateAdjustableOffset() { return null; }
        IRelativeDateOffset IProductBase.CreateRelativeDateOffset() { return new RelativeDateOffset(); }
        IAdjustableDate IProductBase.CreateAdjustableDate(DateTime pDate, BusinessDayConventionEnum pBusinessDayConvention, IBusinessCenters pBusinessCenters)
        {
            AdjustableDate adjustableDate = new AdjustableDate();
            adjustableDate.unadjustedDate = new IdentifierDate();
            adjustableDate.unadjustedDate.DateValue = pDate;
            adjustableDate.dateAdjustments = new BusinessDayAdjustments(pBusinessDayConvention, pBusinessCenters);
            return adjustableDate;
        }
        IRelativeDates IProductBase.CreateRelativeDates() { return new RelativeDates(); }
        IBusinessDateRange IProductBase.CreateBusinessDateRange() { return new BusinessDateRange(); }
        IInterval IProductBase.CreateInterval(PeriodEnum pPeriod, int pMultiplier) { return new Interval(pPeriod.ToString(), pMultiplier); }
        ICalculationPeriodFrequency IProductBase.CreateFrequency(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention)
        {
            return new CalculationPeriodFrequency(pPeriod, pPeriodMultiplier, pRollConvention);
        }
        IInterval[] IProductBase.CreateIntervals() { return new Interval[1] { new Interval() }; }
        IFxAverageRateOption IProductBase.CreateFxAverageRateOption() { return new FxAverageRateOption(); }
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
            PartyReference reference = new PartyReference();
            reference.href = pReference;
            return reference;
        }
        IReference IProductBase.CreatePartyReference(string pReference)
        {
            PartyReference reference = new PartyReference();
            reference.href = pReference;
            return reference;
        }
        IReference IProductBase.CreatePartyOrTradeSideReference(string pReference)
        {
            PartyReference reference = new PartyReference();
            reference.href = pReference;
            return reference;
        }
        IReference[] IProductBase.CreateArrayPartyReference(string pPartyReference)
        {
            return new ArrayPartyReference[1] { new ArrayPartyReference(pPartyReference) };
        }
        /// EG 20150422 [20513] BANCAPERTA New 
        IReference IProductBase.CreateBusinessCentersReference(string pReference)
        {
            BusinessCentersReference reference = new BusinessCentersReference();
            reference.href = pReference;
            return reference;
        }
        ISimplePayment IProductBase.CreateSimplePayment() { return null; }
        ISwap IProductBase.CreateSwap() { return new Swap(); }
        ISecurity IProductBase.CreateSecurity() { return null; }
        ISecurityAsset IProductBase.CreateSecurityAsset() { return null; }
        ISecurityLeg IProductBase.CreateSecurityLeg() { return null; }
        ISecurityLeg[] IProductBase.CreateSecurityLegs(int pDim) { return null; }
        IRoutingCreateElement IProductBase.CreateRoutingCreateElement() { return new RoutingCreateElement(); }
        ISettlementMessagePartyPayment IProductBase.CreateSettlementMessagePartyPayment() { return new SettlementMessagePartyPayment(); }
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters IProductBase.LoadBusinessCenters(string pConnectionString, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM)
        {
            BusinessCenters businessCenters = new BusinessCenters();
            return ((IBusinessCenters)businessCenters).LoadBusinessCenters(pConnectionString, pDbTransaction, pIdA, pIdC, pIdM);
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
                businessCenters.businessCenter = (BusinessCenter[])aBusinessCenters.ToArray(typeof(BusinessCenter));
            }
            return businessCenters;

        }
        void IProductBase.SetProductType(string pId, string pIdentifier)
        {
            productType = new ProductType();
            productType.productTypeScheme = Cst.OTCml_ProductTypeScheme;
            productType.otcmlId = pId;
            productType.Value = pIdentifier;

        }
        void IProductBase.SetId(int pInstrumentNo)
        {

            id = Cst.FpML_InstrumentNo + pInstrumentNo.ToString();

        }
        IAdjustedDate IProductBase.CreateAdjustedDate(DateTime pAdjustedDate)
        {
            IdentifierDate adjustedDate = new IdentifierDate();
            adjustedDate.DateValue = pAdjustedDate;
            return adjustedDate;
        }
        IAdjustedDate IProductBase.CreateAdjustedDate(string pAdjustedDate)
        {
            IdentifierDate adjustedDate = new IdentifierDate();
            adjustedDate.Value = pAdjustedDate;
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
            Trader trader = new Trader();
            trader.traderScheme = Cst.OTCml_ActorIdentifierScheme;
            return trader;
        }
        IInterestRateStream[] IProductBase.CreateInterestRateStream(int pDim)
        {
            InterestRateStream[] ret = new InterestRateStream[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new InterestRateStream();
            return ret;
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
            EarlyTerminationProvision earlyTerminationProvision = new EarlyTerminationProvision();
            earlyTerminationProvision.earlyTerminationOptionalSpecified = false;
            earlyTerminationProvision.earlyTerminationMandatorySpecified = true;
            earlyTerminationProvision.earlyTerminationMandatory = new MandatoryEarlyTermination();
            return earlyTerminationProvision;
        }
        IEarlyTerminationProvision IProductBase.CreateOptionalEarlyTermination()
        {
            EarlyTerminationProvision earlyTerminationProvision = new EarlyTerminationProvision();
            earlyTerminationProvision.earlyTerminationMandatorySpecified = false;
            earlyTerminationProvision.earlyTerminationOptionalSpecified = true;
            earlyTerminationProvision.earlyTerminationOptional = new OptionalEarlyTermination();
            return earlyTerminationProvision;
        }
        IBusinessCenterTime IProductBase.CreateBusinessCenterTime(DateTime pTime, string pBusinessCenter)
        {
            BusinessCenterTime businessCenterTime = new BusinessCenterTime();
            businessCenterTime.hourMinuteTime = new HourMinuteTime(DtFunc.DateTimeToString(pTime, DtFunc.FmtISOTime));
            businessCenterTime.businessCenter = new BusinessCenter(pBusinessCenter);
            return businessCenterTime;
        }
        IQuotedCurrencyPair IProductBase.CreateQuotedCurrencyPair(string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasis)
        {
            return new QuotedCurrencyPair(pIdC1, pIdC1, pQuoteBasis);
        }
        INetInvoiceAmounts IProductBase.CreateNetInvoiceAmounts()
        {
            return null;
        }
        INetInvoiceAmounts IProductBase.CreateNetInvoiceAmounts(decimal pAmount, string pAmountCurrency,
            decimal pIssueAmount, string pIssueAmountCurrency, decimal pAccountingAmount, string pAccountingAmountCurrency)
        {
            return null;
        }
        ITradeIntention IProductBase.CreateTradeIntention()
        {
            return null;
        }
        IPosRequest IProductBase.CreatePosRequest()
        {
            return null;
        }
        IPosRequest IProductBase.CreatePosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_CSS, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestOption IProductBase.CreatePosRequestOption()
        {
            return null;
        }
        IPosRequestPositionOption IProductBase.CreatePosRequestPositionOption()
        {
            return null;
        }
        IPosRequestClearingEOD IProductBase.CreatePosRequestClearingEOD()
        {
            return null;
        }
        IPosRequestClearingBLK IProductBase.CreatePosRequestClearingBLK()
        {
            return null;
        }
        IPosRequestClearingSPEC IProductBase.CreatePosRequestClearingSPEC()
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestOption IProductBase.CreatePosRequestOption(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        IPosRequestTransfer IProductBase.CreatePosRequestTransfer()
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestTransfer IProductBase.CreatePosRequestTransfer(DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        IPosRequestRemoveAlloc IProductBase.CreatePosRequestRemoveAlloc()
        {
            return null;
        }
        // EG 20170127 Qty Long To Decimal
        IPosRequestRemoveAlloc IProductBase.CreatePosRequestRemoveAlloc(DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        IPosRequestSplit IProductBase.CreatePosRequestSplit()
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestSplit IProductBase.CreatePosRequestSplit(DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        /// EG 20130607 [18740] Add RemoveCAExecuted
        IPosRequestRemoveCAExecuted IProductBase.CreatePosRequestRemoveCAExecuted()
        {
            return null;
        }
        IPosRequestRemoveCAExecuted IProductBase.CreatePosRequestRemoveCAExecuted(int pIdA_CssCustodian, int pIdM, int pIdCE, DateTime pDtBusiness, bool pIsCustodian)
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingEOD IProductBase.CreatePosRequestClearingEOD(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingBLK IProductBase.CreatePosRequestClearingBLK(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingSPEC IProductBase.CreatePosRequestClearingSPEC(SettlSessIDEnum pRequestMode, DateTime pDtBusiness,
            int pIdT, decimal pQty, IPosKeepingClearingTrade[] pTradesTarget)
        {
            return null;
        }
        IPosKeepingData IProductBase.CreatePosKeepingData()
        {
            return null;
        }
        IPosKeepingKey IProductBase.CreatePosKeepingKey()
        {
            return null;
        }
        IPosKeepingClearingTrade IProductBase.CreatePosKeepingClearingTrade()
        {
            return null;
        }
        IPosKeepingMarket IProductBase.CreatePosKeepingMarket()
        {
            return null;
        }
        EfsML.v30.PosRequest.PosKeepingAsset IProductBase.CreatePosKeepingAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset)
        {
            return null;
        }
        IPosRequestKeyIdentifier IProductBase.CreatePosRequestKeyIdentifier()
        {
            return null;
        }
        IPosRequestCorrection IProductBase.CreatePosRequestCorrection()
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestCorrection IProductBase.CreatePosRequestCorrection(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        IPosRequestDetCorrection IProductBase.CreatePosRequestDetCorrection()
        {
            return null;
        }
        IPosRequestDetUnderlyer IProductBase.CreatePosRequestDetUnderlyer()
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestEntry IProductBase.CreatePosRequestEntry(DateTime pDtBusiness, decimal pQty)
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64)
        // EG 20170127 Qty Long To Decimal
        IPosRequestUnclearing IProductBase.CreatePosRequestUnclearing(DateTime pDtMarket, int pIdPR, int pIdPADET, Cst.PosRequestTypeEnum pRequestType,
            int pIdT, decimal pQty, DateTime pDtBusiness, int pIdT_Closing, string pClosing_Identifier, decimal pClosingQty)
        {
            return null;
        }
        IPosRequestUpdateEntry IProductBase.CreatePosRequestUpdateEntry(SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestUnclearing IProductBase.CreatePosRequestUnclearing()
        {
            return null;
        }
        IPosRequestUpdateEntry IProductBase.CreatePosRequestUpdateEntry()
        {
            return null;
        }
        IPosRequestCascadingShifting IProductBase.CreatePosRequestCascadingShifting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestCascadingShifting IProductBase.CreatePosRequestCascadingShifting()
        {
            return null;
        }
        IPosRequestMaturityOffsetting IProductBase.CreatePosRequestMaturityOffsetting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestMaturityOffsetting IProductBase.CreatePosRequestMaturityOffsetting()
        {
            return null;
        }
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery IProductBase.CreatePosRequestPhysicalPeriodicDelivery(DateTime pDtBusiness)
        {
            return null;
        }
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery IProductBase.CreatePosRequestPhysicalPeriodicDelivery()
        {
            return null;
        }
        
        // EG 20150317 [POC] Add pIdEM
        IPosRequestEOD IProductBase.CreatePosRequestEOD(int pIdA_entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
        {
            return null;
        }
        IPosRequestEOD IProductBase.CreatePosRequestEOD()
        {
            return null;
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD(int pIdA_entity, int pIdA_CssCustodian, DateTime pDtBusiness, bool pIsCustodian)
        {
            return null;
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD(int pIdA_entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM,bool pIsCustodian)
        {
            return null;
        }
        IPosRequestREMOVEEOD IProductBase.CreatePosRequestREMOVEEOD()
        {
            return null;
        }
        // EG 20150317 [POC] Add IdEM
        IPosRequestClosingDAY IProductBase.CreatePosRequestClosingDAY(int pIdA_entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
        {
            return null;
        }
        IPosRequestClosingDAY IProductBase.CreatePosRequestClosingDAY()
        {
            return null;
        }
        IPosRequestClosingDayControl IProductBase.CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestClosingDayControl IProductBase.CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return null;
        }
        IFixInstrument IProductBase.CreateFixInstrument() { return null; }
        IPosRequestPositionDocument IProductBase.CreatePosRequestPositionDocument() { return null; }
        IPosRequestDetPositionOption IProductBase.CreatePosRequestDetPositionOption() { return null; }
        IPosRequestCorporateAction IProductBase.CreatePosRequestCorporateAction(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            return null;
        }
        IPosRequestCorporateAction IProductBase.CreatePosRequestCorporateAction()
        {
            return null;
        }
        INettingInformationInput IProductBase.CreateNettingInformationInput() { return new NettingInformationInput(); }
        IParty IProductBase.CreateParty() { return new Party(); }
        IBookId IProductBase.CreateBookId() { return new BookId(); }
        IUnitQuantity IProductBase.CreateUnitQuantity() { return null; }
        // EG 20170206 [22787] New
        IPrevailingTime IProductBase.CreatePrevailingTime() { return null; }
        ICalculationPeriodFrequency IProductBase.CreateCalculationPeriodFrequency() { return null; }
        // FI 20170928 [23452] add
        IPerson IProductBase.CreatePerson() { return null; }
        // FI 20170928 [23452] add
        IRelatedPerson IProductBase.CreateRelatedPerson() { return null; }
        // FI 20170928 [23452] add
        IAlgorithm IProductBase.CreateAlgorithm() { return null; }
        // FI 20170928 [23452] add
        IRelatedParty IProductBase.CreateRelatedParty() { return null; }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateTradeCategory() { return null; }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateTradingWaiver() { return null; }
        // FI 20170928 [23452] add
        IScheme IProductBase.CreateOtcClassification() { return null; }
        // EG 20171016 [23509] New
        IZonedDateTime IProductBase.CreateZonedDateTime() { return null; }
        // EG 20171025 [23509] New
        ITradeProcessingTimestamps IProductBase.CreateTradeProcessingTimestamps()
        {
            // FI 20190405 [XXXXX] BANCAPERTA 8.1
            return new EfsML.v30.MiFIDII_Extended.TradeProcessingTimestamps();
        }

        #region IProductBase Members
        bool IProductBase.IsCommoditySwap { get { return false; } }
        bool IProductBase.IsCommoditySpot { get { return false; } }
        bool IProductBase.IsCommodityDerivative { get { return false; } }

        bool IProductBase.primaryAssetClassSpecified
        {
            set { ; }
            get { return false; }
        }

        IScheme IProductBase.primaryAssetClass
        {
            set { ; }
            get { return null; }
        }

        bool IProductBase.secondaryAssetClassSpecified
        {
            set { ; }
            get { return false; }
        }

        IScheme[] IProductBase.secondaryAssetClass
        {
            set { ; }
            get { return null; }
        }
        #endregion

        #endregion IProductBase Members
    }
    #endregion Product
    #region ProductReference
    public partial class ProductReference : IReference
    {
        #region IReference Members
        string IReference.hRef
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
        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId
        #endregion Accessors

        #region ISpheresId Members
        string ISpheresId.otcmlId
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
        string IScheme.scheme
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
            currency1 = new Currency();
            currency1.Value = pCurrency1;
            currency2 = new Currency();
            currency2.Value = pCurrency2;
            quoteBasis = pQuoteBasis;
        }
        public QuotedCurrencyPair(string pCurrency1, string pCurrency2, string pQuoteBasis)
        {
            currency1 = new Currency();
            currency1.Value = pCurrency1;
            currency2 = new Currency();
            currency2.Value = pCurrency2;
            quoteBasis = (QuoteBasisEnum)StringToEnum.Parse(pQuoteBasis, QuoteBasisEnum.Currency1PerCurrency2);
        }
        #endregion Constructors

        #region ICloneable Members
        public object Clone()
        {
            QuotedCurrencyPair clone = new QuotedCurrencyPair();
            clone.currency1 = (Currency)this.currency1.Clone();
            clone.currency2 = (Currency)this.currency2.Clone();
            clone.quoteBasis = this.quoteBasis;
            return clone;
        }
        #endregion ICloneable Members
        #region IQuotedCurrencyPair Members
        string IQuotedCurrencyPair.currency1
        {
            set { this.currency1.Value = value; }
            get { return this.currency1.Value; }
        }
        string IQuotedCurrencyPair.currency1Scheme
        {
            set { this.currency1.currencyScheme = value; }
            get { return this.currency1.currencyScheme; }
        }
        string IQuotedCurrencyPair.currency2
        {
            set { this.currency2.Value = value; }
            get { return this.currency2.Value; }
        }
        string IQuotedCurrencyPair.currency2Scheme
        {
            set { this.currency2.currencyScheme = value; }
            get { return this.currency2.currencyScheme; }
        }
        QuoteBasisEnum IQuotedCurrencyPair.quoteBasis
        {
            set { this.quoteBasis = value; }
            get { return this.quoteBasis; }
        }
        #endregion IQuotedCurrencyPair Members
    }
    #endregion QuotedCurrencyPair

    #region RateSourcePage
    public partial class RateSourcePage : ICloneable, IScheme
    {
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RateSourcePage clone = new RateSourcePage();
            clone.rateSourcePageScheme = this.rateSourcePageScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.scheme
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
    #region Reference
    public partial class Reference : IReference
    {
        #region IReference Members
        string IReference.hRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion Reference
    #region ReferenceAmount
    public partial class ReferenceAmount : IScheme
    {
        #region IScheme Members
        string IScheme.scheme
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
    public partial class RelativeDateOffset : IRelativeDateOffset
    {
        #region Accessors
        #region DateRelativeToValue
        public string DateRelativeToValue
        {
            get
            {
                return dateRelativeTo.href;
            }
        }
        #endregion DateRelativeToValue
        #region GetAdjustments
        public BusinessDayAdjustments GetAdjustments
        {
            get
            {
                BusinessDayAdjustments businessDayAdjustments = new BusinessDayAdjustments();
                businessDayAdjustments.businessCentersDefine = this.businessCentersDefine;
                businessDayAdjustments.businessCentersDefineSpecified = this.businessCentersDefineSpecified;
                businessDayAdjustments.businessCentersReference = this.businessCentersReference;
                businessDayAdjustments.businessCentersReferenceSpecified = this.businessCentersReferenceSpecified;
                businessDayAdjustments.businessCentersNone = this.businessCentersNone;
                businessDayAdjustments.businessCentersNoneSpecified = this.businessCentersNoneSpecified;
                businessDayAdjustments.businessDayConvention = this.businessDayConvention;
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
                offset.period = this.period;
                offset.periodMultiplier = this.periodMultiplier;
                offset.dayType = this.dayType;
                offset.dayTypeSpecified = this.dayTypeSpecified;
                return offset;
            }
        }
        #endregion GetOffset
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
            businessCentersNoneSpecified = true;
            dateRelativeTo = new DateRelativeTo();
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
        bool IRelativeDateOffset.businessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IRelativeDateOffset.businessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }

        bool IRelativeDateOffset.businessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }

        bool IRelativeDateOffset.businessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }

        IBusinessCenters IRelativeDateOffset.businessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }

        BusinessDayConventionEnum IRelativeDateOffset.businessDayConvention
        {
            set { this.businessDayConvention = value; }
            get { return this.businessDayConvention; }
        }
        #endregion IRelativeDateOffset Members
        #region IOffset Members
        bool IOffset.dayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IOffset.dayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
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

        EFS_Integer IInterval.periodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }

        PeriodEnum IInterval.period
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
            int ret = 0;
            Interval intervale = new Interval(this.period, this.periodMultiplier.IntValue);
            IOffset offsetObj = (IOffset)obj;
            Interval intervaleObj = new Interval(offsetObj.period, offsetObj.periodMultiplier.IntValue);
            ret = ((IInterval)intervale).CompareTo(intervaleObj);

            return ret;
        }
        #endregion IInterval Members
    }
    #endregion RelativeDateOffset
    #region RelativeDates
    public partial class RelativeDates : IRelativeDates
    {
        #region IRelativeDateOffset Membres
        IBusinessDayAdjustments IRelativeDates.GetAdjustments { get { return this.GetAdjustments; } }
        IOffset IRelativeDates.GetOffset { get { return this.GetOffset; } }
        bool IRelativeDates.scheduleBoundsSpecified { get { return this.scheduleBoundsSpecified; } }
        DateTime IRelativeDates.scheduleBoundsUnadjustedFirstDate { get { return this.scheduleBounds.unadjustedFirstDate.DateValue; } }
        DateTime IRelativeDates.scheduleBoundsUnadjustedLastDate { get { return this.scheduleBounds.unadjustedLastDate.DateValue; } }
        bool IRelativeDates.periodSkipSpecified { get { return this.periodSkipSpecified; } }
        int IRelativeDates.periodSkip { get { return this.periodSkip.IntValue; } }
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
        #region IRelativeDateSequence Members
        IBusinessDayAdjustments IRelativeDateSequence.GetAdjustments { get { return null; } }
        IDateOffset[] IRelativeDateSequence.dateOffset
        {
            set { this.dateOffset = (DateOffset[])value; }
            get { return this.dateOffset; }
        }
        string IRelativeDateSequence.DateRelativeToValue
        {
            set { this.dateRelativeTo.href = value; }
            get { return this.dateRelativeTo.href; }
        }
        bool IRelativeDateSequence.businessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }

        bool IRelativeDateSequence.businessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }

        IBusinessCenters IRelativeDateSequence.businessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        IReference IRelativeDateSequence.businessCentersReference
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
            RequiredIdentifierDate clone = new RequiredIdentifierDate();
            clone.DateValue = this.DateValue;
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
        string IRequiredIdentifierDate.id
        {
            set { this.id = value; }
            get { return this.id; }
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
        IInterval IResetFrequency.interval { get { return (IInterval)this; } }
        bool IResetFrequency.weeklyRollConventionSpecified
        {
            set { this.weeklyRollConventionSpecified = value; }
            get { return this.weeklyRollConventionSpecified; }
        }
        WeeklyRollConventionEnum IResetFrequency.weeklyRollConvention
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
            precision = new EFS_PosInteger();
            precision.IntValue = pPrecision;
        }
        public Rounding(string pRoundingDirection, int pPrecision)
        {
            if (System.Enum.IsDefined(typeof(RoundingDirectionEnum), pRoundingDirection))
                roundingDirection = (RoundingDirectionEnum)System.Enum.Parse(typeof(RoundingDirectionEnum), pRoundingDirection, true);
            else
                roundingDirection = RoundingDirectionEnum.Nearest;
            precision = new EFS_PosInteger();
            precision.IntValue = pPrecision;
        }
        #endregion Constructors

        #region IRounding Members
        RoundingDirectionEnum IRounding.roundingDirection { get { return this.roundingDirection; } }
        int IRounding.precision { get { return this.precision.IntValue; } }
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

        #region ICloneable members
        public object Clone()
        {
            Routing clone = new Routing();
            clone = (Routing)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable members
        #region IRouting Membres
        bool IRouting.routingIdsSpecified
        {
            get { return this.routingIdsSpecified; }
            set { routingIdsSpecified = value; }
        }
        IRoutingIds IRouting.routingIds
        {
            get { return this.routingIds; }
            set { this.routingIds = (RoutingIds)value; }
        }
        bool IRouting.routingExplicitDetailsSpecified
        {
            get { return this.routingExplicitDetailsSpecified; }
            set { routingExplicitDetailsSpecified = value; }
        }
        IRoutingExplicitDetails IRouting.routingExplicitDetails
        {
            get { return this.routingExplicitDetails; }
            set { routingExplicitDetails = (RoutingExplicitDetails)value; }
        }
        bool IRouting.routingIdsAndExplicitDetailsSpecified
        {
            get { return this.routingIdsAndExplicitDetailsSpecified; }
            set { this.routingIdsAndExplicitDetailsSpecified = value; }
        }
        IRoutingIdsAndExplicitDetails IRouting.routingIdsAndExplicitDetails
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
        #region Constructors
        public RoutingExplicitDetails() { }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RoutingExplicitDetails clone = new RoutingExplicitDetails();
            clone.routingAccountNumberSpecified = this.routingAccountNumberSpecified;
            if (clone.routingAccountNumberSpecified)
                clone.routingAccountNumber = new EFS_String(this.routingAccountNumber.Value);
            clone.routingAddressSpecified = this.routingAddressSpecified;
            if (clone.routingAddressSpecified)
                clone.routingAddress = (Address)this.routingAddress.Clone();
            clone.routingName = new EFS_String(this.routingName.Value);
            clone.routingReferenceTextSpecified = this.routingReferenceTextSpecified;
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
        string IRoutingExplicitDetails.routingName { get { return this.routingName.Value; } }
        bool IRoutingExplicitDetails.routingAccountNumberSpecified { get { return this.routingAccountNumberSpecified; } }
        string IRoutingExplicitDetails.routingAccountNumber { get { return this.routingAccountNumber.Value; } }
        #endregion IRoutingExplicitDetails Members
    }
    #endregion RoutingExplicitDetails
    #region RoutingId
    public partial class RoutingId : IEFS_Array, IRoutingId
    {
        #region Constructors
        public RoutingId()
        {
            routingIdCodeScheme = "http://www.fpml.org/coding-scheme/routing-id-code-1-0";
            Value = string.Empty;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object _Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
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
        string IRoutingId.routingIdCodeScheme
        {
            set { this.routingIdCodeScheme = value; }
            get { return this.routingIdCodeScheme; }
        }
        string IRoutingId.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IRoutingId
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
            RoutingIds clone = new RoutingIds();
            clone.routingId = (RoutingId[])this.routingId.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members

        #region IRoutingIds Members
        IRoutingId[] IRoutingIds.routingId
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

        #region Constructors
        public RoutingIdsAndExplicitDetails() { }
        #endregion Constructors
        //
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            RoutingIdsAndExplicitDetails clone = new RoutingIdsAndExplicitDetails();
            clone.routingAccountNumberSpecified = this.routingAccountNumberSpecified;
            if (clone.routingAccountNumberSpecified)
                clone.routingAccountNumber = new EFS_String(this.routingAccountNumber.Value);
            clone.routingAddressSpecified = this.routingAddressSpecified;
            if (clone.routingAddressSpecified)
                clone.routingAddress = (Address)this.routingAddress.Clone();
            clone.routingName = new EFS_String(this.routingName.Value);
            clone.routingReferenceTextSpecified = this.routingReferenceTextSpecified;
            if (clone.routingReferenceTextSpecified)
            {
                clone.routingReferenceText = new EFS_StringArray[this.routingReferenceText.Length];
                for (int i = 0; i < this.routingReferenceText.Length; i++)
                    clone.routingReferenceText[i] = new EFS_StringArray(this.routingReferenceText[i].Value);
            }
            clone.routingIds = (RoutingIds[])this.routingIds.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        //
        #region IRoutingIdsAndExplicitDetails Members
        IRoutingIds[] IRoutingIdsAndExplicitDetails.routingIds
        {
            get { return this.routingIds; }
            set { routingIds = (RoutingIds[])value; }
        }
        EFS_String IRoutingIdsAndExplicitDetails.routingName
        {
            get { return this.routingName; }
            set { this.routingName = value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.routingAddressSpecified
        {
            get { return this.routingAddressSpecified; }
            set { this.routingAddressSpecified = value; }
        }
        IAddress IRoutingIdsAndExplicitDetails.routingAddress
        {
            get { return this.routingAddress; }
            set { routingAddress = (Address)value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.routingAccountNumberSpecified
        {
            get { return this.routingAccountNumberSpecified; }
            set { this.routingAccountNumberSpecified = value; }
        }
        EFS_String IRoutingIdsAndExplicitDetails.routingAccountNumber
        {
            get { return this.routingAccountNumber; }
            set { this.routingAccountNumber = value; }
        }
        //
        bool IRoutingIdsAndExplicitDetails.routingReferenceTextSpecified
        {
            get { return this.routingReferenceTextSpecified; }
            set { this.routingReferenceTextSpecified = value; }
        }
        EFS_StringArray[] IRoutingIdsAndExplicitDetails.routingReferenceText
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
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
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
            ret = Cst.ErrLevel.SUCCESS;

            return ret;
        }
        #endregion CalcAdjustableSteps
        #region GetStepDatesValue
        public DateTime[] GetStepDatesValue()
        {
            DateTime[] ret = null;
            ArrayList lst = new ArrayList();

            if (ArrFunc.IsFilled(step))
            {
                for (int i = 0; i < step.Length; i++)
                    lst.Add(step[i].stepDate.DateValue);
            }
            //
            ret = (DateTime[])lst.ToArray(typeof(DateTime));
            //
            return ret;
        }
        #endregion
        #endregion Methods

        #region ISchedule Members
        #region Accessors
        EFS_Decimal ISchedule.initialValue
        {
            set { this.initialValue = value; }
            get { return this.initialValue; }
        }
        bool ISchedule.stepSpecified { get { return this.stepSpecified; } }
        IStep[] ISchedule.step
        {
            get { return this.step; }
        }
        EFS_Step[] ISchedule.efs_Steps { get { return this.efs_Steps; } }
        bool ISchedule.IsStepCalculated { get { return this.IsStepCalculated; } }
        DateTime[] ISchedule.GetStepDatesValue { get { return this.GetStepDatesValue(); } }
        string ISchedule.id
        {
            set { this.id = value; }
            get { return this.id; }
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
        bool ISettlementInformation.standardSpecified
        {
            set { this.informationStandardSpecified = value; }
            get { return this.informationStandardSpecified; }
        }
        StandardSettlementStyleEnum ISettlementInformation.standard
        {
            set { this.informationStandard = value; }
            get { return this.informationStandard; }
        }
        bool ISettlementInformation.instructionSpecified
        {
            set { this.informationInstructionSpecified = value; }
            get { return this.informationInstructionSpecified; }
        }
        ISettlementInstruction ISettlementInformation.instruction
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
            SettlementInstruction clone = new SettlementInstruction();
            clone = (SettlementInstruction)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion ICloneable Members

        #region ISettlementInstruction Members
        bool ISettlementInstruction.settlementMethodSpecified
        {
            get { return this.settlementMethodSpecified; }
        }
        IScheme ISettlementInstruction.settlementMethod
        {
            get { return (IScheme)this.settlementMethod; }
        }
        bool ISettlementInstruction.correspondentInformationSpecified
        {
            get { return this.correspondentInformationSpecified; }
        }

        IRouting ISettlementInstruction.correspondentInformation
        {
            get { return this.correspondentInformation; }
        }
        bool ISettlementInstruction.intermediaryInformationSpecified
        {
            get { return this.intermediaryInformationSpecified; }
        }

        IRouting[] ISettlementInstruction.intermediaryInformation
        {
            get { return this.intermediaryInformation; }
        }

        IRouting ISettlementInstruction.beneficiary
        {
            get { return this.beneficiary; }
        }
        IEfsSettlementInstruction ISettlementInstruction.CreateEfsSettlementInstruction()
        {
            return new EfsSettlementInstruction();
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
            SettlementMethod ret = new SettlementMethod();
            ret.settlementMethodScheme = settlementMethodScheme;
            ret.Value = Value;
            return ret;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.scheme
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

    #region Step
    public partial class Step : IEFS_Array, IStep
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
        EFS_Date IStep.stepDate
        {
            get { return this.stepDate; }
        }
        EFS_Decimal IStep.stepValue
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
        EFS_StringArray[] IStreetAddress.streetLine
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
            payOutNone = new Empty();
            payOutAmount = new PayOutAmountSchedule();
            payOutRate = new Schedule();
        }
        #endregion Constructors
        #region Methods
        #region _buyer
        public static object _buyer(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _buyer
        #region _seller
        public static object _seller(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
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
        bool IStrikeSchedule.buyerSpecified
        {
            set { this.buyerSpecified = value; }
            get { return this.buyerSpecified; }
        }
        PayerReceiverEnum IStrikeSchedule.buyer
        {
            set { this.buyer.Value = value; }
            get { return this.buyer.Value; }
        }
        bool IStrikeSchedule.sellerSpecified
        {
            set { this.sellerSpecified = value; }
            get { return this.sellerSpecified; }
        }
        PayerReceiverEnum IStrikeSchedule.seller
        {
            set { this.seller.Value = value; }
            get { return this.seller.Value; }
        }
        #endregion IStrikeSchedule Members
    }
    #endregion StrikeSchedule

    #region UnadjustedDate
    public partial class unadjustedDate : IEFS_Array, IAdjustedDate
    {
        #region Accessors
        // 20071003 EG : Ticket 15800
        #region Value
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            set { dtValue = new EFS_Date(value); }
            get
            {
                if (dtValue == null)
                    return null;
                else
                    return dtValue.Value;
            }
        }
        #endregion Value
        #region Id
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
        #endregion Value
        #endregion Accessors
        #region Constructors
        public unadjustedDate() { }
        #endregion Constructors

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
            set { this.dtValue.DateValue = value; }
            get { return this.dtValue.DateValue; }
        }
        string IAdjustedDate.id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        #endregion IAdjustedDate Members
    }
    #endregion UnadjustedDate

}
