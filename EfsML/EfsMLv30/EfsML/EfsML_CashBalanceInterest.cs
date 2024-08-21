using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
//
using EFS.GUI;
using EFS.GUI.Attributes;
//
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Shared;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Ird;
using FpML.v44.Shared;

namespace EfsML.v30.CashBalanceInterest
{

    /// <summary>
    /// Cash Balance Interest(modeled as an FpML:Product)
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("cashBalanceInterest", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Cash Balance Interest")]
    public partial class CashBalanceInterest : Product
    {
        #region Members
        /// <summary>
        /// Type de montant sur lequel portent les intérêts
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("interestAmountType", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Interest Amount Type")]
        public InterestAmountTypeEnum interestAmountType;

        /// <summary>
        /// Représente l'entité
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("entityPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [ControlGUI(Name = "Entity", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference entityPartyReference;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceInterestStream", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash Balance Interest Stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1, IsCopyPasteItem = true, Color = MethodsGUI.ColorEnum.Green)]
		[BookMarkGUI(Name = "S", IsVisible = true)]
        public CashBalanceInterestStream[] cashBalanceInterestStream;
        #endregion Members

    }
    #region CashBalanceInterestStream
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalanceInterestStream : InterestRateStreamBase
    {
        [System.Xml.Serialization.XmlElementAttribute("threshold", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Minimum Threshold")]
        public Money minimumThreshold;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool minimumThresholdSpecified;
    }
    #endregion CashBalanceInterestStream
}