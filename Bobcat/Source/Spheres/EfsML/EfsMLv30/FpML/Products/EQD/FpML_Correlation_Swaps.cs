#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Eq.Shared;
using FpML.v44.Option.Shared;
#endregion using directives

// EG 20140702 New build FpML4.4 CorrelationSwapOption removed
namespace FpML.v44.CorrelationSwaps
{
    #region CorrelationAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CorrelationAmount : CalculatedAmount
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("correlation", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Correlation", IsVisible = false)]
        public Correlation correlation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Correlation")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion CorrelationAmount
    #region CorrelationLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CorrelationLeg : DirectionalLegUnderlyerValuation
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount", IsVisible = false)]
        public CorrelationAmount amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        public bool FillBalise3;
    }
    #endregion CorrelationLeg
    #region CorrelationSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("correlationSwap", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Correlation Swap")]
    public partial class CorrelationSwap : NettedSwapBase
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("correlationLeg", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg", IsVisible = false)]
        public CorrelationLeg correlationLeg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Leg")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion CorrelationSwap
}
