#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Doc;
using FpML.v44.Riskdef;
using FpML.v44.ValuationResults.ToDefine;
#endregion using directives

namespace FpML.v44.Main
{
    #region ValuationDocument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationDocument : DataDocument
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketSpecified;
        [System.Xml.Serialization.XmlElementAttribute("market",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Markets")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Market", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Market[] market;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationSetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationSet",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "ValuationSets")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "ValuationSet", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public ValuationSet[] valuationSet;
        #endregion Members
    }
    #endregion ValuationDocument
}
