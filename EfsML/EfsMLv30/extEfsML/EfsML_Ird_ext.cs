#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Enum;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
#endregion using directives

namespace EfsML.v30.Ird
{
	#region KnownAmountSchedule
	public partial class KnownAmountSchedule : IKnownAmountSchedule
	{
		#region Constructors
		public KnownAmountSchedule() { }
		#endregion Constructors

		#region IKnownAmountSchedule Members

		bool IKnownAmountSchedule.NotionalValueSpecified
		{
			get { return this.notionalValueSpecified; }
		}
		IMoney IKnownAmountSchedule.NotionalValue
		{
			get { return this.notionalValue; }
		}
		bool IKnownAmountSchedule.DayCountFractionSpecified
		{
			get { return this.dayCountFractionSpecified; }
            set { this.dayCountFractionSpecified = value; }
		}
		DayCountFractionEnum IKnownAmountSchedule.DayCountFraction
		{
			get { return this.dayCountFraction; }
            set { this.dayCountFraction = value; }
		}
		#endregion IKnownAmountSchedule Members
	}
	#endregion KnownAmountSchedule

	#region StepUpProvision
	public partial class StepUpProvision : IStepUpProvision
	{
		#region Accessors
		#region EFS_Exercise
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object EFS_Exercise
		{
			get
			{
				if (stepupExerciseAmericanSpecified)
					return stepupExerciseAmerican;
				else if (stepupExerciseBermudaSpecified)
					return stepupExerciseBermuda;
				else if (stepupExerciseEuropeanSpecified)
					return stepupExerciseEuropean;
				else
					return null;
			}
		}
		#endregion EFS_Exercise
		#endregion Accessors
		#region Constructors
		public StepUpProvision()
		{
			stepupExerciseAmerican = new AmericanExercise();
			stepupExerciseBermuda = new BermudaExercise();
			stepupExerciseEuropean = new EuropeanExercise();
		}
		#endregion Constructors

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle
		{
			get
			{
				if (this.stepupExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.stepupExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		ICashSettlement IProvision.CashSettlement 
		{
			set { ;}
			get { return null; } 
		}
		#endregion IProvision Members
		#region IStepUpProvision Members
		EFS_ExerciseDates IStepUpProvision.Efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion IStepUpProvision Members

	}
	#endregion StepUpProvision
}
