#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.v30.Security;
using EfsML.v30.Security.Shared;
using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.Enum;
/*using FpML.v44.Mktenv;*/
using FpML.v44.Shared;
using FpML.v44.Option.Shared;

#endregion using directives


namespace FpML.v44.BondOption
{
    #region BondOption
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //[System.Xml.Serialization.XmlRootAttribute("bondOption", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("bondOption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Bond Option")]
    /// EG 20150422 [20513] BANCAPERTA new EFSmL-3-0
    public partial class BondOption : OptionBaseExtended
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("strike", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public BondOptionStrike strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public EFS_RadioChoice underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bond", IsVisible = true)]
        public Bond underlyerBond;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerConvertibleBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "ConvertibleBond", IsVisible = true)]
        public ConvertibleBond underlyerConvertibleBond;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool securityAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityAsset", typeof(SecurityAsset), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Debenture Security")]
        public SecurityAsset securityAsset;
        #endregion Members
    }
    #endregion BondOption
    #region BondOptionStrike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class BondOptionStrike : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        public EFS_RadioChoice typeStrike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikeReferenceSwapCurveSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceSwapCurve", typeof(ReferenceSwapCurve))]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference to a Swap curve", IsVisible = true)]
        public ReferenceSwapCurve typeStrikeReferenceSwapCurve;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("price", typeof(OptionStrike))]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "OptionStrike", IsVisible = true)]
        public OptionStrike typeStrikePrice;
        #endregion Members
    }
    #endregion BondOptionStrike

    #region MakeWholeAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MakeWholeAmount : SwapCurveValuation
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interpolationMethodSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interpolation method")]
        public InterpolationMethod interpolationMethod;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Early call date")]
        public IdentifiedDate earlyCallDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Early call date")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion MakeWholeAmount

    #region ReferenceSwapCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReferenceSwapCurve : ItemGUI
    {
        #region Members
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Swap curve valuation")]
        public SwapCurveValuation swapUnwindValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool makeWholeAmountSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Swap curve valuation")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Make whole amount")]
        public MakeWholeAmount makeWholeAmount;
        #endregion Members
    }
    #endregion ReferenceSwapCurve

    #region SwapCurveValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MakeWholeAmount))]
    public partial class SwapCurveValuation : ItemGUI
    {
        #region Members
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark floating rate index")]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexTenorSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark floating rate index")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tenor")]
        public Interval indexTenor;
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Spread")]
        public EFS_Decimal spread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side of the measure")]
        public QuotationSideEnum side;
        #endregion Members
    }
    #endregion SwapCurveValuation
}
