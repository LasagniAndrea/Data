#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Interface;

using FpML.Interface;

using FpML.v44.Eq.Shared;
using FpML.v44.Option.Shared;
#endregion using directives

// EG 20140702 CorrelationSwapOption removed
namespace FpML.v44.CorrelationSwaps
{
	#region CorrelationSwap
	public partial class CorrelationSwap : IProduct,IDeclarativeProvision
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion CorrelationSwap
}
