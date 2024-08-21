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

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.FixedIncome
{
	#region BondPrice
	public partial class BondPrice : IBondPrice
	{
		#region IBondPrice Members
		EFS_Decimal IBondPrice.CleanPrice
		{
			set { this.cleanPrice = value;}
			get { return this.cleanPrice; }
		}
		EFS_Boolean IBondPrice.CleanOfAccruedInterest
		{
			set { this.cleanOfAccruedInterest = value; }
			get { return this.cleanOfAccruedInterest; }
		}
		bool IBondPrice.AccrualsSpecified
		{
			set { this.accrualsSpecified = value; }
			get { return this.accrualsSpecified; }
		}
		EFS_Decimal IBondPrice.Accruals
		{
			set { this.accruals = value; }
			get { return this.accruals; }
		}
		bool IBondPrice.DirtyPriceSpecified
		{
			set { this.dirtyPriceSpecified = value; }
			get { return this.dirtyPriceSpecified; }
		}
		EFS_Decimal IBondPrice.DirtyPrice
		{
			set { this.dirtyPrice = value; }
			get { return this.dirtyPrice; }
		}
		#endregion IBondPrice Members
	}
	#endregion BondPrice 

	#region BondTransaction
	public partial class BondTransaction : IProduct, IBondTransaction
	{
		#region IBondTransaction Members
		IBondPrice IBondTransaction.Price
		{
			set { this.price = (BondPrice)value; }
			get { return this.price; }
		}
		IBond IBondTransaction.Bond
		{
			set { this.bond = (Bond)value; }
			get { return this.bond; }
		}
		#endregion IBondTransaction Members
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
	#endregion BondTransaction
}
