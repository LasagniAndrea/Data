#region using directives
using EfsML.Interface;
using FpML.Interface;
#endregion using directives

// EG 20140702 DividendSwapTransactionSupplementOption removed
namespace FpML.v44.DividendSwaps
{
    #region DividendSwapTransactionSupplement
    public partial class DividendSwapTransactionSupplement : IProduct,IDeclarativeProvision
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
	#endregion DividendSwapTransactionSupplement
}
