#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Enum;

using FpML.Enum;

using FpML.v44.Shared;
using FpML.v44.Option.Shared;
#endregion using directives


namespace EfsML.v30.Option.Shared
{
	#region ExtendedBarrier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class ExtendedBarrier : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool barrierCapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("barrierCap", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap : Trigger level approached from beneath")]
		public ExtendedTriggerEvent barrierCap;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool barrierFloorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("barrierFloor", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor : Trigger level approached from above")]
		public ExtendedTriggerEvent barrierFloor;
		#endregion Members
	}
	#endregion ExtendedBarrier
	#region ExtendedTriggerEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class ExtendedTriggerEvent : TriggerEvent
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool triggerPerComponentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerPerComponent", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger per component")]
		public EFS_Boolean triggerPerComponent;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool triggerHitOperatorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerHitOperator", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger hit operator")]
		public TriggerHitOperatorEnum triggerHitOperator;
		#endregion Members
	}
	#endregion ExtendedTriggerEvent
}
