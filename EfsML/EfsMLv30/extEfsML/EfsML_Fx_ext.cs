#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;


using EfsML.Interface;

using FpML.Enum;

using FpML.v44.Enum;
using FpML.v44.Fx;
using FpML.v44.Shared;

#endregion using directives

namespace EfsML.v30.Fx
{
	#region AssetOrNothing
	public partial class AssetOrNothing : IAssetOrNothing
	{
		#region Constructors
		public AssetOrNothing() { }
		#endregion Constructors

		#region IAssetOrNothing Members
		bool IAssetOrNothing.CurrencyReferenceSpecified {get { return this.currencyReferenceSpecified;}}
		string IAssetOrNothing.CurrencyReference {get { return this.currencyReference.Value;}}
		bool IAssetOrNothing.GapSpecified {get { return this.gapSpecified;}}
		decimal IAssetOrNothing.Gap {get { return this.gap.DecValue;}}
		#endregion IAssetOrNothing Members
	}
	#endregion AssetOrNothing
	#region AverageStrikeOption
	public partial class AverageStrikeOption : IAverageStrikeOption
	{
		#region Constructors
		public AverageStrikeOption() { }
		#endregion Constructors

		#region IAverageStrikeOption Members
		SettlementTypeEnum IAverageStrikeOption.SettlementType 
		{
			set { this.settlementType = value; } 
			get { return this.settlementType; } 
		}
		#endregion IAverageStrikeOption Members
	}
	#endregion AverageStrikeOption

	#region CappedCallOrFlooredPut
	public partial class CappedCallOrFlooredPut : ICappedCallOrFlooredPut
	{
		#region Constructors
		public CappedCallOrFlooredPut()
		{
			typeFxCapBarrier = new Empty();
			typeFxFloorBarrier = new Empty();
		}
		#endregion Constructors

		#region ICappedCallOrFlooredPut Members
		bool ICappedCallOrFlooredPut.TypeFxCapBarrierSpecified{get { return this.typeFxCapBarrierSpecified; }}
		bool ICappedCallOrFlooredPut.TypeFxFloorBarrierSpecified {get { return this.typeFxFloorBarrierSpecified; } }
		PayoutEnum ICappedCallOrFlooredPut.PayoutStyle { get { return this.payoutStyle; } }
		#endregion ICappedCallOrFlooredPut Members
	}
	#endregion CappedCallOrFlooredPut

    #region PayoutPeriod
    public partial class PayoutPeriod : IPayoutPeriod
    {
        #region Constructors
        public PayoutPeriod() { }
        #endregion Constructors

		#region IPayoutPeriod Members
		PeriodEnum IPayoutPeriod.Period {get { return this.period; }}
		int IPayoutPeriod.PeriodMultiplier{get { return this.periodMultiplier.IntValue; }}
		decimal IPayoutPeriod.Percentage{get { return this.percentage.DecValue; } }
		#endregion IPayoutPeriod Members
	}
    #endregion PayoutPeriod
}
