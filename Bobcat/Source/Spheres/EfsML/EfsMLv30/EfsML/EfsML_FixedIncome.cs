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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class BondPrice : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("deliveryPrice", Order = 1)]
		[ControlGUI(Name = "Bond clean price", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Decimal cleanPrice;
		[System.Xml.Serialization.XmlElementAttribute("cleanOfAccruedInterest", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Clean of accrued interest")]
		public EFS_Boolean cleanOfAccruedInterest;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool accrualsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("accruals", Order = 3)]
		[ControlGUI(Name = "accruals")]
		public EFS_Decimal accruals;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool dirtyPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dirtyPrice", Order = 4)]
		[ControlGUI(Name = "Dirty price")]
		public EFS_Decimal dirtyPrice;
		#endregion Members
	}
	#endregion BondPrice

	#region BondTransaction
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute("bondTransaction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class BondTransaction : AbstractUnitTransaction
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("price", Order = 1)]
		[ControlGUI(Name = "Price", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public BondPrice price;
		[System.Xml.Serialization.XmlElementAttribute("future", Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Identification of the bond", IsVisible = false)]
		public Bond bond;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Identification of the bond")]
		public bool FillBalise;

		#endregion Members
	}
	#endregion BondTransaction
}
