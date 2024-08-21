#region Using Directives
using System;
using System.Reflection;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;

using EfsML.Enum;

using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Asset;
using FpML.v42.Eqs;
using FpML.v42.EqShared;
using FpML.v42.Shared;
#endregion Using Directives

namespace FpML.v42.Eqd
{
	#region BrokerEquityOption
	public partial class BrokerEquityOption : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion BrokerEquityOption
	#region EquityForward
	public partial class EquityForward : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion EquityForward
	#region EquityOption
	public partial class EquityOption : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion EquityOption
	#region EquityOptionTransactionSupplement
	public partial class EquityOptionTransactionSupplement : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion EquityOptionTransactionSupplement
}
