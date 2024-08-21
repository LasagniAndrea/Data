#region using directives

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
//
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

//
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Shared;
using EfsML.v30.Doc;  
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using FpML.v44.Assetdef;
using FixML.Enum;
using FixML.v50SP1.Enum;
//
#endregion using directives

namespace EfsML.v30.MarginRequirement
{
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("marginRequirement", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Margin Requirement")]
    public partial class MarginRequirement : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("clearingOrganizationPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [ControlGUI(Name = "Clearing Organization", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference clearingOrganizationPartyReference;

        [System.Xml.Serialization.XmlElementAttribute("marginRequirementOfficePartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [ControlGUI(Name = "Margin Requirement Office", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference marginRequirementOfficePartyReference;


        [System.Xml.Serialization.XmlElementAttribute("entityPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [ControlGUI(Name = "Entity", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference entityPartyReference;

        [System.Xml.Serialization.XmlElementAttribute("timing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        [ControlGUI(Name = "Timing", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public SettlSessIDEnum timing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool isGrossMarginSpecified;

        [System.Xml.Serialization.XmlElementAttribute("isGrossMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [ControlGUI(Name = "Gross Margining", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Boolean isGrossMargin;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialMarginMethodSpecified;

        [System.Xml.Serialization.XmlElementAttribute("initialMarginMethod", typeof(InitialMarginMethodEnum), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        // PM 20160404 [22116] initialMarginMethod devient un array
        // EG 20230808 [26454] New Gestion Full/FpML InitialMarginMethod
        public InitialMarginMethodEnum[] InitialMarginMethod
        {
            set
            {
                efs_initialMarginMethodSpecified = ArrFunc.IsFilled(value);
                if (ArrFunc.IsFilled(value))
                {
                    efs_initialMarginMethod = new EFS_EnumArray[] { };
                    List<EFS_EnumArray> lst = new List<EFS_EnumArray>();
                    value.ToList().ForEach(item => lst.Add(new EFS_EnumArray(ReflectionTools.ConvertEnumToString<InitialMarginMethodEnum>(item))));
                    efs_initialMarginMethod = lst.ToArray();
                }
            }
            get
            {
                InitialMarginMethodEnum[] ret = null;
                if (ArrFunc.IsFilled(efs_initialMarginMethod))
                {
                    List<InitialMarginMethodEnum> lst = new List<InitialMarginMethodEnum>();
                    efs_initialMarginMethod.ToList().ForEach(item => {
                        Nullable<InitialMarginMethodEnum> _enum = ReflectionTools.ConvertStringToEnumOrNullable<InitialMarginMethodEnum>(item.Value);
                        if (_enum.HasValue)
                            lst.Add(_enum.Value);
                    });
                    if (lst.Count > 0)
                        ret = lst.ToArray();
                }
                efs_initialMarginMethodSpecified = (null != ret);
                return ret;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_initialMarginMethodSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Margin Methods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial Margin Method", IsMaster = false, IsMasterVisible = false, IsChild = false, MinItem = 0)]
        public EFS_EnumArray[] efs_initialMarginMethod;

        [System.Xml.Serialization.XmlElementAttribute("payment", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment", IsClonable = true, IsChild = true)]
        public SimplePayment[] payment;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingStockSpecified;

        /// <summary>
        ///  Liste des positions actions déposées et utilisées pour la réduction des postions ETD Short futures et Short call
        /// </summary>
        [System.Xml.Serialization.XmlArray(ElementName = "underlyingStocks", Order = 8)]
        [System.Xml.Serialization.XmlArrayItemAttribute("underlyingStock")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)] // FI 20160613 [22256] GLOP (prévoir l'affichage de cet élément)
        public UnderlyingStock[] underlyingStock;

        #endregion Members
    }
    

    /// <summary>
    /// Actions déposées et utilisées pour réduire les positions ETD Short Future et Short Call Future
    /// </summary>
    public partial class UnderlyingStock
    {
        /// <summary>
        /// Id non significatif (POSEQUSECURITY.IDPOSEQUSECURITY)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;

        /* otcmlId pertmet d'accéder à l'acteur comme au book, je laisse bookId uniquement parce que déjà suffisamment significatif 
        /// <summary>
        /// Représente l'acteur déposant (côté ENTITY) ou le dépositaire (côté CLEARER)
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("actorId")]
        public ActorId actorId;
        */

        /// <summary>
        /// Représente le book déposant (côté ENTITY) ou le dépositaire (côté CLEARER)
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("bookId")]
        public BookId bookId;


        /// <summary>
        ///  Représente l'asset Equity
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("equity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityAsset equity;


        /// <summary>
        /// Qté disponibles  (POSEQUSECURITY.QTY)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("qtyAvailable")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        // EG 20170127 Qty Long To Decimal
        public decimal qtyAvailable;

        /// <summary>
        /// Qté utilisée pour couvrir des positions ETD Future
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("qtyUsedFut")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public decimal qtyUsedFut;

        /// <summary>
        /// Qté utilisée pour couvrir des positions ETD Option
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("qtyUsedOpt")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public decimal qtyUsedOpt;
    }

    /// <summary>
    ///  Liste des Actions déposées et utilisées pour réduire les positions ETD Short Future et Short Call Future
    /// </summary>
    public partial class UnderlyingStocks
    {
        /// <summary>
        ///  Liste de positions actions 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("underlyingStock", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public UnderlyingStock[] underlyingStock;
    }

}
