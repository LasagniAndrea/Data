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
using EfsML.v30.Ird;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.LoanDeposit
{
	#region LoanDeposit
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute("loanDeposit", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class LoanDeposit : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("loanDepositStream", Order = 1)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Loan & Deposit Stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2, IsCopyPasteItem = true)]
		[BookMarkGUI(Name = "S", IsVisible = true)]
		public LoanDepositStream[] loanDepositStream;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool earlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
		public EarlyTerminationProvision earlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cancelableProvision", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cancelable Provision")]
		public CancelableProvision cancelableProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("extendibleProvision", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extendible Provision")]
		public ExtendibleProvision extendibleProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool additionalPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("additionalPayment", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
		[BookMarkGUI(IsVisible = false)]
		public Payment[] additionalPayment;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stepUpProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Step-Up Provision")]
		public StepUpProvision stepUpProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Implicit Provisions")]
		public ImplicitProvision implicitProvision;
		#endregion Members

	}
	#endregion LoanDeposit
	#region LoanDepositStream
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2007/EFSmL-3-0")]
	public partial class LoanDepositStream : InterestRateStreamBase	{}
    #endregion LoanDepositStream
}
