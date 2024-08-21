#region using directives
using EfsML.Business;
using EfsML.Interface;

using FpML.Interface;

#endregion using directives

// EG 20140702 VarianceSwapTion Removed
namespace FpML.v44.VarianceSwaps
{
    #region VarianceLeg
    // EG 20140702 Upd Interface
    public partial class VarianceLeg : IVarianceLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_VarianceLeg efs_VarianceLeg;
		#endregion Members

		#region IReturnSwapLeg Members
		IReference IReturnSwapLeg.PayerPartyReference 
        {
            set { this.payerPartyReference = (FpML.v44.Shared.PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
		IReference IReturnSwapLeg.ReceiverPartyReference 
        {
            set { this.receiverPartyReference = (FpML.v44.Shared.PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; } 
        }
        IReference IReturnSwapLeg.CreateReference
        {
            get { return new FpML.v44.Shared.PartyOrAccountReference(); }
        }
        IMoney IReturnSwapLeg.CreateMoney
        {
            get { return new FpML.v44.Shared.Money(); }
        }
        string IReturnSwapLeg.LegEventCode
        {
            get
            {
                return string.Empty;
            }
        }
        string IReturnSwapLeg.LegEventType
        {
            get
            {
                return string.Empty ;
            }
        }
		#endregion IReturnSwapLeg Members
	}
	#endregion VarianceLeg
	#region VarianceSwap
	public partial class VarianceSwap : IProduct,IDeclarativeProvision
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
	#endregion VarianceSwap
}
