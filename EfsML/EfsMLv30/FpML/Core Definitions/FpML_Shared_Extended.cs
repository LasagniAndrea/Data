#region using directives
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.v30.CommodityDerivative;
using FpML.Interface;

#endregion using directives

namespace FpML.v44.Shared
{
    #region AssetClass
    /// <summary>
    /// Extended of Product complexType (FpML v.5.9)
    /// </summary>
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AssetClass : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string assetClassScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Constructors
        public AssetClass()
        {
            assetClassScheme = "http://www.fpml.org/coding-scheme/asset-class";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            AssetClass clone = new AssetClass
            {
                assetClassScheme = this.assetClassScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone

        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.assetClassScheme; }
            set { this.assetClassScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members

        #endregion Methods
    }
    #endregion AssetClass

    #region Product
    /// <summary>
    /// Extended of Product complexType (FpML v.5.9)
    /// </summary>
    /// EG 20161122 New Commodity Derivative
    public partial class Product 
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryAssetClassSpecified;

        [System.Xml.Serialization.XmlElementAttribute("primaryAssetClass", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "PrimaryAssetClass")]
        public AssetClass primaryAssetClass;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryAssetClassSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Secondary Asset Class")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "SecondaryAssetClass", IsClonable = true)]
        [System.Xml.Serialization.XmlElementAttribute("secondaryAssetClass", Order = 2)]
        public AssetClass[] secondaryAssetClass;
        #endregion Members

        #region IProductBase Members
        bool IProductBase.IsCommoditySpot { get { return this.GetType().Equals(typeof(CommoditySpot)); } }
        bool IProductBase.IsCommoditySwap { get { return this.GetType().Equals(typeof(CommoditySwap)); } }
        bool IProductBase.IsCommodityDerivative { get { return ((IProductBase)this).IsCommoditySwap || ((IProductBase)this).IsCommoditySpot; } }

        bool IProductBase.PrimaryAssetClassSpecified
        {
            set { this.primaryAssetClassSpecified = value; }
            get { return this.primaryAssetClassSpecified; }
        }

        IScheme IProductBase.PrimaryAssetClass
        {
            set { this.primaryAssetClass = (AssetClass)value; }
            get { return this.primaryAssetClass; }
        }

        bool IProductBase.SecondaryAssetClassSpecified
        {
            set { this.secondaryAssetClassSpecified = value; }
            get { return this.secondaryAssetClassSpecified; }
        }

        IScheme[] IProductBase.SecondaryAssetClass
        {
            set { this.secondaryAssetClass= (AssetClass[])value; }
            get { return this.secondaryAssetClass; }
        }
        #endregion
    }
    #endregion Product

}
