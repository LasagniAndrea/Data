#region using directives
using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EFS.EFSTools;



using EfsML.DynamicData;

using EfsML.Enum;

using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.DebtSecurity;
using EfsML.v30.Shared;
using EfsML.v30.Ird;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives

namespace EfsML.v30.DebtSecurityTransaction
{
    #region DebtSecurityTransaction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("DebtSecurityTransaction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
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
        [System.Xml.Serialization.XmlElementAttribute("securityAsset", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security Asset", IsVisible = false)]
        public SecurityAsset securityAsset;
        //
        [System.Xml.Serialization.XmlElementAttribute("quantity", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security Asset")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity", IsVisible = false)]
        public OrderQuantity quantity;
        //
        [System.Xml.Serialization.XmlElementAttribute("price", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quantity")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Price", IsVisible = false)]
        public OrderPrice price;
        //
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Price")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount", IsVisible = false)]
        public Payment grossAmount;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross Amount")]
        public bool FillBalise;
        //
        #endregion Members
    }
    #endregion DebtSecurityTransaction

    #region Security
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityAsset : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("securityId", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "securityId")]
        public EFS_String securityId;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool securityNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityName", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security name")]
        public EFS_String securityName;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool securityDescriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityDescription", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security description")]
        public EFS_String securityDescription;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool securityIssueDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityIssueDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue date")]
        public EFS_Date securityIssueDate;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool debtSecuritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security characteristics")]
        public DebtSecurity.DebtSecurity debtSecurity;     

        #endregion Members
    }
    #endregion Security

    #region OrderPrice
    public partial class OrderPrice
    {
        [System.Xml.Serialization.XmlElementAttribute("priceUnits", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [ControlGUI(Name = "value")]
        public PriceQuoteUnits priceUnits;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cleanPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cleanPrice", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clean price")]
        public EFS_Decimal cleanPrice;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dirtyPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dirtyPrice", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dirty price")]
        public EFS_Decimal dirtyPrice;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest rate")]
        public EFS_Decimal accruedInterestRate;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestAmount", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest amount")]
        public Money accruedInterestAmount;
    }
    #endregion

    #region OrderQuantity
    public partial class OrderQuantity 
    {
        [System.Xml.Serialization.XmlElementAttribute("quantityType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Quantity type")]
        public OrderQuantityTypeEnum  quantityType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("numberOfUnits", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of units")]
        public EFS_Decimal numberOfUnits;

        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        public Money notionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional")]
        public bool FillBalise;
    }
    #endregion OrderQuantity

}
