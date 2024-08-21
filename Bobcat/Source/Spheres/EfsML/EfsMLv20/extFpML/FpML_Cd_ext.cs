#region Using Directives
using System;
using System.Reflection;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Enum;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Asset;
using FpML.v42.Eqs;
using FpML.v42.Shared;
#endregion Using Directives
namespace FpML.v42.Cd
{
	#region CreditDefaultSwap
	public partial class CreditDefaultSwap : IProduct
	{
		#region Constructors
		public CreditDefaultSwap()
		{
			itemCash = new CashSettlementTerms();
			itemPhysical = new PhysicalSettlementTerms();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion CreditDefaultSwap
}
