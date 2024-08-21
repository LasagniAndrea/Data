#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Matching.Status.ToDefine;
using FpML.v44.Msg.ToDefine;
using FpML.v44.PostTrade.Confirmation.ToDefine;
using FpML.v44.PostTrade.Execution.ToDefine;
using FpML.v44.PostTrade.Negotiation.ToDefine;
using FpML.v44.Shared;

#endregion using directives


namespace FpML.v44.PostTrade.ToDefine
{
	#region PartialTerminationAmount
	public partial class PartialTerminationAmount
	{
		#region Constructors
		public PartialTerminationAmount()
		{
			detailDecreaseInNotionalAmount		= new Money();
			detailOutstandingNotionalAmount		= new Money();
			detailDecreaseInNumberOfOptions		= new EFS_Decimal();
			detailOutstandingNumberOfOptions	= new EFS_Decimal();
		}
		#endregion Constructors
	}
	#endregion PartialTerminationAmount

	#region Termination
	public partial class Termination
	{
		#region Constructors
		public Termination()
		{
			typePartial = new PartialTerminationAmount();
			typeFull	= new Empty();
		}
		#endregion Constructors
	}
	#endregion Termination

}
