#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.Enum;

using FpML.v44.Enum;
using FpML.v44.Fx;
using FpML.v44.Shared;

#endregion using directives

namespace EfsML.v30.Fx
{
    #region AssetOrNothing
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetOrNothing : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyReference", typeof(Currency), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currencyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("gap", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Gap", Width = 75)]
        public EFS_Decimal gap;
        #endregion Members
    }
    #endregion AssetOrNothing
    #region AverageStrikeOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AverageStrikeOption : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 1)]
        [ControlGUI(Name = "Settlement Type")]
        public SettlementTypeEnum settlementType;
        #endregion Members
    }
    #endregion AverageStrikeOption

    #region CappedCallOrFlooredPut
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CappedCallOrFlooredPut : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Type")]
        public EFS_RadioChoice type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFxCapBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxCapBarrier", typeof(Empty), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty typeFxCapBarrier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFxFloorBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFloorBarrier", typeof(Empty), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty typeFxFloorBarrier;

        [System.Xml.Serialization.XmlElementAttribute("payoutStyle", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Payout style")]
        public PayoutEnum payoutStyle;
        #endregion Members
    }
    #endregion CappedCallOrFlooredPut

    #region FxOptionLegBarrier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxBarrierOption))]
    public class FxOptionLegBarrier : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementTermsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement Terms")]
        public FxCashSettlement cashSettlementTerms;
        [System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=8)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=10)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotedAsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedAs", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted As")]
        public QuotedAs quotedAs;
        #endregion Members

        #region Accessors
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
        {
            get {return buyerPartyReference.href; }
        }
        #endregion BuyerPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
        {
            get {return sellerPartyReference.href; }
        }
        #endregion SellerPartyReference
        #endregion Accessors
        #region Constructors
        public FxOptionLegBarrier()
        {
            fxStrikePrice = new FxStrikePrice();
        }
        #endregion Constructors
    }
    #endregion FxOptionLegBarrier

    #region PayoutPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PayoutPeriod : Interval
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("percentage", typeof(EFS_Decimal), Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Percentage of payout", Width = 75)]
        public EFS_Decimal percentage;
        #endregion Members
    }
    #endregion PayoutPeriod
}
