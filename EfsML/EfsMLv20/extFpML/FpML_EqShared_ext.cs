#region Using Directives
using System;
using System.Reflection;
using System.Xml.Serialization;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Enum;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Asset;
using FpML.v42.Shared;
#endregion Using Directives
namespace FpML.v42.EqShared
{
    #region Composite
    public partial class Composite : IComposite
    {
        #region Constructors
        public Composite()
        {
        }
        #endregion Constructors

        #region IComposite Members
        IRelativeDateOffset IComposite.relativeDate
        {
            set { this.relativeDate = (RelativeDateOffset)value; }
            get { return this.relativeDate; }
        }
        bool IComposite.relativeDateSpecified
        {
            set { this.relativeDateSpecified = value; }
            get { return this.relativeDateSpecified; }
        }
        EFS_MultiLineString IComposite.determinationMethod
        {
            set { this.determinationMethod = value; }
            get { return this.determinationMethod; }
        }
        bool IComposite.determinationMethodSpecified
        {
            set { this.determinationMethodSpecified = value; }
            get { return this.determinationMethodSpecified; }
        }
        IFxSpotRateSource IComposite.fxSpotRateSource
        {
            set { fxSpotRateSource = (FxSpotRateSource)value; }
            get { return this.fxSpotRateSource; }
        }

        bool IComposite.fxSpotRateSourceSpecified
        {
            set { this.fxSpotRateSourceSpecified = value; }
            get { return this.fxSpotRateSourceSpecified; }
        }
        #endregion IComposite Members

    }
    #endregion Composite
	#region EquityValuation
    // EG 20140702 Upd Interface
	public partial class EquityValuation : IEquityValuation
	{
		#region IEquityValuation Members
		bool IEquityValuation.valuationDateSpecified
		{
            set { this.valuationDateSpecified=value; }
			get { return this.valuationDateSpecified; }
		}
		IAdjustableDateOrRelativeDateSequence IEquityValuation.valuationDate
		{
			get { return this.valuationDate; }
		}
		bool IEquityValuation.valuationDatesSpecified
		{
			set { this.valuationDatesSpecified = value; }
			get { return this.valuationDatesSpecified; }
		}
		IAdjustableRelativeOrPeriodicDates IEquityValuation.valuationDates
		{
			set { this.valuationDates =(AdjustableRelativeOrPeriodicDates) value; }
			get { return this.valuationDates; }
		}
		bool IEquityValuation.valuationTimeTypeSpecified
		{
			get { return this.valuationTimeTypeSpecified; }
		}
		TimeTypeEnum IEquityValuation.valuationTimeType
		{
			get { return this.valuationTimeType; }
		}
		bool IEquityValuation.valuationTimeSpecified
		{
			get { return this.valuationTimeSpecified; }
		}
		IBusinessCenterTime IEquityValuation.valuationTime
		{
			get { return this.valuationTime; }
		}
		bool IEquityValuation.futuresPriceValuationSpecified
		{
			get { return this.futuresPriceValuationSpecified; }
		}
		EFS_Boolean IEquityValuation.futuresPriceValuation
		{
			get { return this.futuresPriceValuation; }
		}
		bool IEquityValuation.optionsPriceValuationSpecified
		{
			get { return this.optionsPriceValuationSpecified; }
		}
		EFS_Boolean IEquityValuation.optionsPriceValuation
		{
			get { return this.optionsPriceValuation; }
		}
		#endregion IEquityValuation Members
	}
	#endregion EquityValuation
	#region EquityValuationDate
    // EG 20140702 Upd Interface
	public partial class EquityValuationDate : IAdjustableDateOrRelativeDateSequence
	{
		#region Constructors
		public EquityValuationDate()
		{
			equityValuationDateAdjustableDate = new AdjustableDate();
			equityValuationDateRelativeDateSequence = new RelativeDateSequence();
		}
		#endregion Constructors

		#region IAdjustableDateOrRelativeDateSequence Members
		bool IAdjustableDateOrRelativeDateSequence.adjustableDateSpecified
		{
			set { this.equityValuationDateAdjustableDateSpecified = value; }
			get { return this.equityValuationDateAdjustableDateSpecified; }
		}
		IAdjustableDate IAdjustableDateOrRelativeDateSequence.adjustableDate 
		{
			get { return this.equityValuationDateAdjustableDate; } 
		}
		bool IAdjustableDateOrRelativeDateSequence.relativeDateSequenceSpecified 
		{
            set { this.equityValuationDateRelativeDateSequenceSpecified = value; }
            get { return this.equityValuationDateRelativeDateSequenceSpecified; }
        }
		IRelativeDateSequence IAdjustableDateOrRelativeDateSequence.relativeDateSequence 
		{
			get { return this.equityValuationDateRelativeDateSequence; }
		}
		#endregion IAdjustableDateOrRelativeDateSequence Members
	}
	#endregion EquityValuationDate
	#region ExtraordinaryEvents
	public partial class ExtraordinaryEvents : IExtraordinaryEvents
	{
		#region Constructors
		public ExtraordinaryEvents()
		{
			itemFailureToDeliver = new EFS_Boolean();
			itemAdditionalDisruptionEvents = new AdditionalDisruptionEvents();
		}
		#endregion Constructors

		#region IExtraordinaryEvents Members
		bool IExtraordinaryEvents.mergerEventsSpecified
		{
			get { return this.mergerEventsSpecified;}
		}
		#endregion IExtraordinaryEvents Members
	}
	#endregion ExtraordinaryEvents

	#region FxFeature
	public partial class FxFeature : IFxFeature
	{
		#region Constructors
		public FxFeature()
		{
			referenceCurrency = new IdentifiedCurrency();
			fxFeatureQuanto = new Quanto();
			fxFeatureComposite = new Composite();
		}
		#endregion Constructors

		#region IFxFeature Members
        ISchemeId IFxFeature.referenceCurrency
        {
            set { this.referenceCurrency = (IdentifiedCurrency)value; }
            get { return this.referenceCurrency; }
        }
        bool IFxFeature.fxFeatureCompositeSpecified
        {
            set { this.fxFeatureCompositeSpecified = value; }
            get { return this.fxFeatureCompositeSpecified; }
        }
        IComposite IFxFeature.fxFeatureComposite
        {
            set { this.fxFeatureComposite = (Composite)value; }
            get { return this.fxFeatureComposite; }
        }
        bool IFxFeature.fxFeatureQuantoSpecified
        {
            set { this.fxFeatureQuantoSpecified = value; }
            get { return this.fxFeatureQuantoSpecified; }
        }
        IQuanto IFxFeature.fxFeatureQuanto
        {
            set { this.fxFeatureQuanto = (Quanto)value; }
            get { return this.fxFeatureQuanto; }
        }

        bool IFxFeature.fxFeatureCrossCurrencySpecified
        {
            set { ;}
            get { return false; }
        }
        IComposite IFxFeature.fxFeatureCrossCurrency
        {
            set { ;}
            get { return null; }
        }
		#endregion IFxFeature Members
	}
	#endregion FxFeature

	#region Quanto
	public partial class Quanto : IQuanto
	{
		#region IQuanto Members
		IFxRate[] IQuanto.CreateFxRate { get {return new FxRate[1] { new FxRate() }; } }
		IFxRate[] IQuanto.fxRate
		{
			set { this.fxRate= (FxRate[])value; }
			get { return this.fxRate; }
		}
		#endregion IQuanto Members
	}
	#endregion Quanto

}
