#region using directives
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.Enum;

using EfsML.v30.Security.Shared;
using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.v44.Shared;
#endregion using directives

namespace EfsML.v30.Security
{
    #region BuyAndSellBack
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("buyAndSellBack", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Buy and Sell Back")]
    public partial class BuyAndSellBack : SaleAndRepurchaseAgreement
    {
    }
    #endregion BuyAndSellBack
    #region DebtSecurityTransaction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("debtSecurityTransaction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Debt Security Transaction")]
    /// EG 20190730 New Attrbutes (TrdType, TrdSubType and SecondaryTrdType
    public partial class DebtSecurityTransaction : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrTradeSideReference buyerPartyReference;
        //
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrTradeSideReference sellerPartyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Security asset", Color = MethodsGUI.ColorEnum.Red)]
        public EFS_RadioChoice item;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemSecurityAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityAsset", typeof(SecurityAsset), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Asset", IsVisible = true)]
        public SecurityAsset itemSecurityAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemSecurityAssetReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityAssetReference", typeof(SecurityAssetReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference to", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.AssetReference)]
        public SecurityAssetReference itemSecurityAssetReference;


        [System.Xml.Serialization.XmlElementAttribute("quantity", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity", IsVisible = false)]
        public OrderQuantity quantity;
        //
        [System.Xml.Serialization.XmlElementAttribute("price", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Price", IsVisible = false)]
        public OrderPrice price;
        //
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Price")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount", IsVisible = false, Color = MethodsGUI.ColorEnum.Orange)]
        public Payment grossAmount;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Type")]
        [DictionaryGUI(Tag = "828", Anchor = "TrdType")]
        public TrdTypeEnum trdType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdTypeSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Sub Type")]
        [DictionaryGUI(Tag = "829", Anchor = "TrdSubType")]
        public TrdSubTypeEnum trdSubType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trdSubTypeSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Trade Type")]
        [DictionaryGUI(Tag = "855", Anchor = "SecondaryTrdType")]
        public SecondaryTrdTypeEnum secondaryTrdType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryTrdTypeSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;

        #endregion Members
    }
    #endregion DebtSecurityTransaction
    #region Repo
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("repo", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class Repo : SaleAndRepurchaseAgreement
    {
    }
    #endregion Repo
    #region SaleAndRepurchaseAgreement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BuyAndSellBack))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Repo))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SecurityLending))]
    public partial class SaleAndRepurchaseAgreement : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("duration", Order = 1)]
        public RepoDurationEnum duration;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool noticePeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("noticePeriod", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notice period")]
        public AdjustableOffset noticePeriod;
        //
        [System.Xml.Serialization.XmlElementAttribute("cashStream", Order = 3)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, IsCopyPasteItem = true, Color = MethodsGUI.ColorEnum.Green)]
        //[BookMarkGUI(Name = "S", IsVisible = true)]
        public CashStream[] cashStream;
        //
        [System.Xml.Serialization.XmlElementAttribute("spotLeg", Order = 4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot leg", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, IsCopyPasteItem = true,Color = MethodsGUI.ColorEnum.Orange)]
        public SecurityLeg[] spotLeg;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forwardLeg", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forward legs")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forward leg", IsClonable = true, IsChild = true, Color = MethodsGUI.ColorEnum.Orange)]
        public SecurityLeg[] forwardLeg;
        #endregion Members
    }
    #endregion SaleAndRepurchaseAgreement
    #region SecurityLending
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("securityLending", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class SecurityLending : SaleAndRepurchaseAgreement
    {
    }
    #endregion SecurityLending
    #region SecurityLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityLeg : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotLegReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotLegReference", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot leg reference")]
        public SecurityLegReference spotLegReference;
        //
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security transaction")]
        public DebtSecurityTransaction debtSecurityTransaction;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marginSpecified;
        [System.Xml.Serialization.XmlElementAttribute("margin", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security transaction")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Margin")]
        public Margin margin;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SecurityLeg)]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
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
        #endregion Members
    }
    #endregion SecurityLeg
}
