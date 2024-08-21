#region using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;

using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Doc;
#endregion using directives

namespace EfsML.v20.Settlement
{
    #region IssisCriteria
    [System.Xml.Serialization.XmlRootAttribute("ISSIS", Namespace = "", IsNullable = true)]
	public partial class IssisCriteria : Issis 
	{
		#region Members
		//Context Pour le tri 
        //private CssCriteria _cssCriteria;
        //private SsiCriteria _ssiCriteria;
		#endregion Members
	}
	#endregion Issis

    #region IssiItemsRoutingActorsInfo
    [System.Xml.Serialization.XmlRootAttribute("ISSIITEMS", Namespace = "", IsNullable = true)]
    public partial class IssiItemsRoutingActorsInfo : IssiItems
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private SettlementRoutingActorsBuilder issiActorsInfo;
        #endregion Members
    }
	#endregion IssiItems

	#region SettlementChain
	/// <summary>
	/// Description résumée de SettlementChain.
	/// </summary>
	public partial class SettlementChain 
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementChainItems", Order = 1)]
		private SettlementChainItem[] settlementChainItem;
		public int cssLink;
		#endregion Members
	}
	#endregion SettlementChain
	#region SettlementChainItem
	/// <summary>
	/// Item of a setlementChain (Item Payer or Item Receiver)
	/// </summary>
	public partial class SettlementChainItem
	{
		#region Members
		public SiModeEnum siMode;
		public int idIssi;        //uniquement si siMode = Oissi ,  Fissi ou   Dissi
		public EfsSettlementInstruction settlementInstructions;
		public int idASettlementOffice;  //(Payer's or Receiver's Settlement Office) use for Load SSI for example 
		#endregion Members
	}
	#endregion SettlementChainItem
}


