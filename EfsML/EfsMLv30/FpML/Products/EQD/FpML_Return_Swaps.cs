#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Doc;
using FpML.v44.Eq.Shared;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.ReturnSwaps
{
    #region EquitySwapTransactionSupplement
    /// <summary>
    /// A type for defining Equity Swap Transaction Supplement
    /// </summary>
    // EG 20140702 New build FpML4.4 Add relevantJuridiction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("equitySwapTransactionSupplement", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class EquitySwapTransactionSupplement : ReturnSwapBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mutualEarlyTerminationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mutualEarlyTermination", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "mutualEarlyTermination")]
        public EFS_Boolean mutualEarlyTermination;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExchangeIndexAnnexFallbackSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multipleExchangeIndexAnnexFallback", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "multipleExchangeIndexAnnexFallback")]
        public EFS_Boolean multipleExchangeIndexAnnexFallback;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localJurisdictionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("localJurisdiction", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Local jurisdiction")]
        public Country localJurisdiction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantJurisdictionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relevantJurisdiction", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Relevant jurisdiction")]
        public Country relevantJurisdiction;
        #endregion Members
    }
    #endregion EquitySwapTransactionSupplement
}
