#region using directives
using EFS.GUI.Interface;
using EfsML.Interface;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.FuturesAndOptions
{
    #region FutureTransaction
    public partial class FutureTransaction : IProduct, IFutureTransaction
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFutureTransaction Members
		EFS_Decimal IFutureTransaction.DeliveryPrice
		{
			set { this.deliveryPrice = value; }
			get { return this.deliveryPrice; }
		}
		IFuture IFutureTransaction.Future
		{
			set { this.future = (Future)value; }
			get { return this.future; }
		}
		#endregion IFutureTransaction Members
		#region IAbstractUnitTransaction Members
		EFS_Decimal IAbstractUnitTransaction.NumberOfUnits
		{
			set { this.numberOfUnits = value; }
			get { return this.numberOfUnits; }
		}
		IMoney IAbstractUnitTransaction.UnitPrice
		{
			set { this.unitPrice = (Money)value; }
			get { return this.unitPrice; }
		}
		#endregion IAbstractUnitTransaction Members
		#region IAbstractTransaction Members
		IReference IAbstractTransaction.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IAbstractTransaction.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		#endregion IAbstractTransaction Members
	}
	#endregion FutureTransaction
}
