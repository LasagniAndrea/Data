#region using directives
using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.v30.Shared;
using FixML.v50SP1;

using FpML.v44.Shared;
using FpML.v44.Assetdef;
#endregion using directives

namespace EfsML.v30.Fix
{
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySecurityTransaction))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedDerivative))]
    /// EG 20150624 [21151] New
    public abstract partial class ExchangeTradedFIXML : Product
    {
        #region CfiCodeCategoryEnum category
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool categorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("Category", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "CFI Category")]
        public CfiCodeCategoryEnum category;
        #endregion CfiCodeCategoryEnum category

        #region FIXML fixML
        [System.Xml.Serialization.XmlElementAttribute("FIXML", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        //[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "FIXML")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "FIXML", IsVisible = true, Color = MethodsGUI.ColorEnum.Green)]
        public FIXML fixML;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "FIXML")]
        public bool FillBalise;
        #endregion FIXML fixML
    }

    #region ExchangeTradedDerivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("exchangeTradedDerivative", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20150624 [21151] New ExchangeTradedFIXML
    public partial class ExchangeTradedDerivative : ExchangeTradedFIXML
    {
    }
    #endregion ExchangeTradedDerivative

    #region EquitySecurityTransaction
    /// <summary>
    ///  Produit EquitySecurityTransaction
    /// </summary>
    /// EG 20150306 [POC-BERKELEY] : Add marginRatio
    /// EG 20150624 [21151] New ExchangeTradedFIXML
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("equitySecurityTransaction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Equity Security Transaction")]
    public partial class EquitySecurityTransaction : ExchangeTradedFIXML
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount", IsVisible = false, Color = MethodsGUI.ColorEnum.Orange)]
        public Payment grossAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marginRatioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Margin ratio")]
        public MarginRatio marginRatio;
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool FillBalise;
        #endregion Members
    }
    #endregion EquitySecurityTransaction

}
