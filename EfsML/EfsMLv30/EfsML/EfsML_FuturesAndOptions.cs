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


namespace EfsML.v30.FuturesAndOptions
{
	#region FutureTransaction
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute("futureTransaction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class FutureTransaction : AbstractUnitTransaction
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("deliveryPrice", Order = 1)]
		[ControlGUI(Name = "Delivery price", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Decimal deliveryPrice;
		[System.Xml.Serialization.XmlElementAttribute("future", Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Identification of the future", IsVisible = false)]
		public Future future;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Identification of the future")]
		public bool FillBalise;

		#endregion Members
	}
	#endregion FutureTransaction

}
