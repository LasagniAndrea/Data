#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.v30.Shared;

using FpML.Enum;
using FpML.v44.Shared;
using FpML.v44.Option.Shared;
using FpML.v44.Assetdef;
 
#endregion using directives


namespace EfsML.v30.AssetDef
{
    #region IdentifiedFxRate
    /// <summary>
    /// Représente un taux de change 
    /// </summary>
    /// FI 20110824 [CashBalance] add IdentifiedFxRate
    public partial class IdentifiedFxRate : FxRate 
    {
        /// <summary>
        /// Identifiant unique de la cotation dans un document
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.FxRate)]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        /// <summary>
        /// Représente l'Id non significatif de la cotation 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

    }
    #endregion

    #region ShortAsset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("asset", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20140826 Add ShortAsset : UnderlyingAsset à la place de ShortAsset : Asset
    public partial class ShortAsset : UnderlyingAsset
    {
        //20091022 FI [add underlying instrument in invoice] 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool instrumentSpecified;
        /// <summary>
        /// Représente l'instrument/product rattaché à l'asset
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("instrument", Order = 1)]
        [ControlGUI(Name = "instrument", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ProductType instrument;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalAmountSpecified;
        
        /// <summary>
        /// Représente le notional de l'asset
        /// </summary>
        /// FI 20091223 [16471] add notionalAmount
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 2)]
        [ControlGUI(Name = "notionalAmount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money notionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodNumberOfDaysSpecified;
        
        
        /// <summary>
        /// Durée de l'asset exprimée en nombre de jours (base Exact/360)
        /// </summary>
        /// FI 20091223 [16471] add periodNumberOfDays
        [System.Xml.Serialization.XmlElementAttribute("periodNumberOfDays", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Integer periodNumberOfDays;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string type = "efs:ShortAsset";
    }
    #endregion

    #region UnderlyerCappedFlooredPrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class UnderlyerCappedFlooredPrice : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
		public EFS_RadioChoice cappedFloored;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedFlooredPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("price", typeof(EFS_Decimal),Order=1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal cappedFlooredPrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedFlooredPriceRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceRelativeTo", typeof(PriceReference), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
		public PriceReference cappedFlooredPriceRelativeTo;
		#endregion Members
	}
	#endregion UnderlyerCappedFlooredPrice

    #region FxRateReference
    /// <summary>
    /// Représente une reference vers un taux de change 
    /// </summary>
    /// FI 20110824 [CashBalance] add FxRateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FxRateReference : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }
    #endregion



}
