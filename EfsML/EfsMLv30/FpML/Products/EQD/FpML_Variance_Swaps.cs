#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Shared;
using FpML.v44.Eq.Shared;
using FpML.v44.Option.Shared;
#endregion using directives

// EG 20140702 New build FpML4.4 VarianceSwapOption removed
namespace FpML.v44.VarianceSwaps
{
    #region VarianceAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class VarianceAmount : CalculatedAmount
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("variance", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance", IsVisible = false)]
        public Variance variance;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion VarianceAmount
    #region VarianceLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class VarianceLeg : DirectionalLegUnderlyerValuation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance amount", IsVisible = false)]
        public VarianceAmount amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance amount")]
        public bool FillBalise3;
        #endregion Members
    }
    #endregion VarianceLeg
    #region VarianceSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("varianceSwap", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class VarianceSwap : NettedSwapBase
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("varianceLeg", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance leg", IsVisible = false)]
        public VarianceLeg varianceLeg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Variance leg")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion VarianceSwap

    #region VarianceSwapTransactionSupplement
    /// <summary>
    /// 
    /// </summary>
    // EG 20140702 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("varianceSwapTransactionSupplement", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class VarianceSwapTransactionSupplement : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("returnLeg", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Variance Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Variance Leg", IsClonable = true, IsChild = true)]
        public VarianceLeg[] varianceLeg;
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
    #endregion VarianceSwapTransactionSupplement

}
