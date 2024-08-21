#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using System.Reflection;
#endregion using directives


namespace EfsML.v30.Option.Shared
{
    #region ExtendedBarrier
    public partial class ExtendedBarrier : IExtendedBarrier, IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IBarrier Members
		bool IExtendedBarrier.BarrierCapSpecified
		{
			set { this.barrierCapSpecified = value; }
			get { return this.barrierCapSpecified; }
		}
		IExtendedTriggerEvent IExtendedBarrier.BarrierCap
		{
			set { this.barrierCap = (ExtendedTriggerEvent)value; }
			get { return this.barrierCap; }
		}
		bool IExtendedBarrier.BarrierFloorSpecified
		{
			set { this.barrierFloorSpecified = value; }
			get { return this.barrierFloorSpecified; }
		}
		IExtendedTriggerEvent IExtendedBarrier.BarrierFloor
		{
			set { this.barrierFloor = (ExtendedTriggerEvent)value; }
			get { return this.barrierFloor; }
		}
		#endregion IBarrier Members
	}
	#endregion ExtendedBarrier
	#region ExtendedTriggerEvent
	public partial class ExtendedTriggerEvent : IExtendedTriggerEvent
	{
		#region ITriggerEvent Members
		bool ITriggerEvent.ScheduleSpecified
		{
			set { this.scheduleSpecified = value; }
			get { return this.scheduleSpecified; }
		}
		IAveragingSchedule[] ITriggerEvent.Schedule
		{
			set { this.schedule = (AveragingSchedule[])value; }
			get { return this.schedule; }
		}
		bool ITriggerEvent.TriggerDatesSpecified
		{
			set { this.triggerDatesSpecified = value; }
			get { return this.triggerDatesSpecified; }
		}
		IDateList ITriggerEvent.TriggerDates
		{
			set { this.triggerDates = (DateList)value; }
			get { return this.triggerDates; }
		}
		ITrigger ITriggerEvent.Trigger
		{
			set { this.trigger = (Trigger)value; }
			get { return this.trigger; }
		}
		bool ITriggerEvent.FeaturePaymentSpecified
		{
			set { this.featurePaymentSpecified = value; }
			get { return this.featurePaymentSpecified; }
		}
		IFeaturePayment ITriggerEvent.FeaturePayment
		{
			set { this.featurePayment = (FeaturePayment)value; }
			get { return this.featurePayment; }
		}
		#endregion ITriggerEvent Members

		#region IExtendedTriggerEvent Members
		bool IExtendedTriggerEvent.TriggerPerComponentSpecified
		{
			set {this.triggerPerComponentSpecified = value;}
			get { return this.triggerPerComponentSpecified; }
		}
		EFS_Boolean IExtendedTriggerEvent.TriggerPerComponent
		{
			set { this.triggerPerComponent = value; }
			get { return this.triggerPerComponent; }
		}
		bool IExtendedTriggerEvent.TriggerHitOperatorSpecified
		{
			set { this.triggerHitOperatorSpecified = value; }
			get { return this.triggerHitOperatorSpecified; }
		}
		TriggerHitOperatorEnum IExtendedTriggerEvent.TriggerHitOperator
		{
			set { this.triggerHitOperator = value; }
			get { return this.triggerHitOperator; }
		}
		#endregion IExtendedTriggerEvent Members
	}
	#endregion ExtendedTriggerEvent
}
