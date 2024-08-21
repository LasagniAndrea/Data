#region using directives
using System;
using System.Linq;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Shared;
using FpML.v44.Assetdef;
using FpML.Interface;

using EfsML.Enum;
using FpML.Enum;

using FixML.v50SP1;
#endregion using directives

using System.Collections.Generic;
using EFS.ACommon;

namespace EfsML.v30.CommodityDerivative
{
    #region ArraySettlementPeriodsReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ArraySettlementPeriodsReference
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SettlementPeriods)]
        public EFS_Href efs_href;
        #endregion Members
    }
    #endregion ArraySettlementPeriodsReference

    #region AveragePriceLeg
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("averagePriceLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AveragePriceLeg : CommoditySwapLeg
    {
    }
    #endregion AveragePriceLeg

    #region CalculationPeriodsDatesReference
    /// EG 20161122 New Commodity Derivative
    public partial class CalculationPeriodsDatesReference : HrefGUI, ICloneable, IReference
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
        #region Constructor
        public CalculationPeriodsDatesReference() { }
        public CalculationPeriodsDatesReference(string pReference)
        {
            this.href = pReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            CalculationPeriodsDatesReference clone = new CalculationPeriodsDatesReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion CalculationPeriodsDatesReference
    #region CalculationPeriodsReference
    /// EG 20161122 New Commodity Derivative
    public partial class CalculationPeriodsReference : HrefGUI, ICloneable, IReference
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
        #region Constructor
        public CalculationPeriodsReference() { }
        public CalculationPeriodsReference(string pReference)
        {
            this.href = pReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            CalculationPeriodsReference clone = new CalculationPeriodsReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion CalculationPeriodsReference
    #region CalculationPeriodsScheduleReference
    /// EG 20161122 New Commodity Derivative
    public partial class CalculationPeriodsScheduleReference : HrefGUI, ICloneable, IReference
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
        #region Constructor
        public CalculationPeriodsScheduleReference() { }
        public CalculationPeriodsScheduleReference(string pReference)
        {
            this.href = pReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            CalculationPeriodsReference clone = new CalculationPeriodsReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion CalculationPeriodsScheduleReference
    #region Commodity
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("commodity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class Commodity
    {
        #region IdentifiedAsset / Asset
        [System.Xml.Serialization.XmlElementAttribute("instrumentId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyingAsset", IsVisible = false, IsGroup = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Id", IsClonable = true, IsMaster = true)]
        public InstrumentId[] instrumentId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description", Width = 600)]
        public EFS_String description;
        #endregion IdentifiedAsset / Asset

        //<xsd:group name="CommodityProduct.model">
        //<xsd:sequence>
        //<xsd:group minOccurs="0" ref="CommodityReferencePriceFramework.model"/>
        //<xsd:element name="specifiedPrice" type="SpecifiedPriceEnum"/>
        //<xsd:sequence minOccurs="0">
        //<xsd:choice>
        //<xsd:choice>
        //<xsd:element fpml-annotation:deprecated="true" fpml-annotation:deprecatedReason="Enumerated representation of deliveryDates is deprecate in favor of a parametric representation. 
        //Rationale: There is a need to track all the possible nearby contracts used for pricing. The 'DeliveryDatesEnum' list can grow significantly. Use instead 'deliveryNearby' component 
        //that contain a deliveryNearbyMultiplier (e.g. 0, 1, 2, 3, ...) and a deliveryNearbyType (e.g. NearByMonth, NearByWeek, etc.)." name="deliveryDates" type="DeliveryDatesEnum"/>
        //<xsd:element name="deliveryNearby" type="DeliveryNearby"/>
        //</xsd:choice>
        //<xsd:element name="deliveryDate" type="AdjustableDate"/>
        //<xsd:element name="deliveryDateYearMonth" type="xsd:gYearMonth"/>
        //</xsd:choice>
        //<xsd:element minOccurs="0" name="deliveryDateRollConvention" type="Offset"/>
        //<xsd:element minOccurs="0" name="deliveryDateExpirationConvention" type="Offset"/>
        //</xsd:sequence>
        //<xsd:element minOccurs="0" name="multiplier" type="PositiveDecimal"/>
        //</xsd:sequence>
        //</xsd:group>

        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance", Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Commodity";
        #endregion Members
    }
    #endregion Commodity
    #region Commodity
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("commodityAsset", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CommodityAsset : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:CommodityAsset";
        #endregion Members
    }
    #endregion Commodity
    #region CommodityBusinessCalendar
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityBusinessCalendar : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string commodityBusinessCalendarScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public CommodityBusinessCalendar()
        {
            commodityBusinessCalendarScheme = "http://www.fpml.org/coding-scheme/commodity-business-calendar";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.commodityBusinessCalendarScheme = value; }
            get { return this.commodityBusinessCalendarScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CommodityBusinessCalendar
    #region CommodityCalculationPeriodsSchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityCalculationPeriodsSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlElementAttribute("balanceOfFirstPeriod", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Balance of First period")]
        public EFS_Boolean balanceOfFirstPeriod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public EFS_Id efs_id;
        #endregion Members

        #region Accessors
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
        #endregion Accessors
    }
    #endregion CommodityCalculationPeriodsSchedule
    #region CommodityDeliveryPeriods
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryPeriods", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CommodityDeliveryPeriods : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Delivery periods")]
        public EFS_RadioChoice delivery;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryPeriodsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periods", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates deliveryPeriods;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryPeriodsScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodsSchedule", typeof(CommodityCalculationPeriodsSchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        public CommodityCalculationPeriodsSchedule deliveryPeriodsSchedule;

        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryCalculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference deliveryCalculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryCalculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference deliveryCalculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryCalculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference deliveryCalculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommodityDeliveryPeriods
    #region CommodityDeliveryPoint
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityDeliveryPoint : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string deliveryPointScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members

        #region Constructors
        public CommodityDeliveryPoint()
        {
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.deliveryPointScheme = value; }
            get { return this.deliveryPointScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CommodityDeliveryPoint
    #region CommodityFixedPriceSchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommodityFixedPriceSchedule
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Step")]
        public EFS_RadioChoice step;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepFixedPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPriceStep", typeof(FixedPrice), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed price", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public FixedPrice[] stepFixedPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepWorldscaleRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("worldscaleRateStep", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Worlscale rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public EFS_Decimal[] stepWorldscaleRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepContractRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractRateStep", typeof(NonNegativeMoney), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public NonNegativeMoney[] stepContractRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepSettlementPeriodsPriceScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsPriceSchedule", typeof(CommoditySettlementPeriodsPriceSchedule), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed price", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommoditySettlementPeriodsPriceSchedule[] stepSettlementPeriodsPriceSchedule;
        #endregion Members
        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer
    }
    #endregion CommodityFixedPriceSchedule
    #region CommodityFx
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommodityFx
    {
        #region Members
        //<xsd:complexType name="CommodityFx">
        //<xsd:sequence>
        //<xsd:element name="primaryRateSource" type="InformationSource"/>
        //<xsd:element minOccurs="0" name="secondaryRateSource" type="InformationSource"/>
        //<xsd:element minOccurs="0" name="fxType" type="CommodityFxType"/>
        //<xsd:element minOccurs="0" name="averagingMethod" type="AveragingMethodEnum"/>
        //<xsd:choice minOccurs="0">
        //<xsd:element name="fixingTime" type="BusinessCenterTime"/>
        //<xsd:sequence>
        //<xsd:choice>
        //<xsd:element maxOccurs="unbounded" name="fxObservationDates" type="AdjustableDates"/>
        //<xsd:sequence>
        //<xsd:sequence minOccurs="0">
        //<xsd:group ref="Days.model"/>
        //<xsd:group minOccurs="0" ref="LagOrReference.model"/>
        //</xsd:sequence>
        //<xsd:group ref="CommodityCalculationPeriodsPointer.model"/>
        //</xsd:sequence>
        //</xsd:choice>
        //<xsd:element minOccurs="0" name="fixingTime" type="BusinessCenterTime"/>
        //</xsd:sequence>
        //</xsd:choice>
        //</xsd:sequence>
        //</xsd:complexType>
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingMethod", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Averaging method")]
        public AveragingMethodEnum averagingMethod;
        #endregion Members
    }
    #endregion CommodityFx
    #region CommodityHub
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityHub : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Party")]
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference partyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountReferenceSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account")]
        [System.Xml.Serialization.XmlElementAttribute("accountReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference accountReference;

        [System.Xml.Serialization.XmlElementAttribute("hubCode", Order = 3)]
        [ControlGUI(Name = "Hub code")]
        public CommodityHubCode hubCode;

        #endregion Members

    }
    #endregion CommodityHub
    #region CommodityHubCode
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityHubCode : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string hubCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public CommodityHubCode()
        {
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.hubCodeScheme = value; }
            get { return this.hubCodeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CommodityHubCode
    #region CommodityMarketDisruption
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("commodityMarketDisruption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CommodityMarketDisruption
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Market disruption")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMarketDisruptionEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("marketDisruptionEvents", typeof(MarketDisruptionEventsEnum), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Events", IsVisible = true)]
        public MarketDisruptionEventsEnum itemMarketDisruptionEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMarketDisruptionEventSpecified;
        [System.Xml.Serialization.XmlElementAttribute("marketDisruptionEvent", typeof(MarketDisruptionEvent), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Event", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public MarketDisruptionEvent[] itemMarketDisruptionEvent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalMarketDisruptionEventSpecified;

        [System.Xml.Serialization.XmlElementAttribute("additionalMarketDisruptionEvent", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Additional Market Disruption Event")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public MarketDisruptionEvent[] additionalMarketDisruptionEvent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Disruption fallback")]
        public EFS_RadioChoice item2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool item2DisruptionFallbacksSpecified;
        [System.Xml.Serialization.XmlElementAttribute("disruptionFallbacks", typeof(DisruptionFallbacksEnum), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Events", IsVisible = true)]
        public DisruptionFallbacksEnum item2DisruptionFallbacks;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool item2DisruptionFallbackSpecified;
        [System.Xml.Serialization.XmlElementAttribute("disruptionFallback", typeof(SequencedDisruptionFallback), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Event", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1)]
        public SequencedDisruptionFallback[] item2DisruptionFallback;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackReferencePriceSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Fallback Reference price")]
        [System.Xml.Serialization.XmlElementAttribute("fallbackReferencePrice", Order = 6)]
        public Underlyer fallbackReferencePrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNumberOfDaysOfDisruptionSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Maximum number of days of disruption")]
        [System.Xml.Serialization.XmlElementAttribute("maximumNumberOfDaysOfDisruption", Order = 7)]
        public EFS_NonNegativeInteger maximumNumberOfDaysOfDisruption;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceMaterialityPercentageSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Price materiality percentage")]
        [System.Xml.Serialization.XmlElementAttribute("priceMaterialityPercentage", Order = 8)]
        public EFS_Decimal priceMaterialityPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumFuturesContractsSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Minimum futures contracts")]
        [System.Xml.Serialization.XmlElementAttribute("minimumFuturesContracts", Order = 9)]
        public EFS_PosInteger minimumFuturesContracts;

        #endregion Members
    }
    #endregion CommodityMarketDisruption
    #region CommodityNotionalQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityNotionalQuantity : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quantityUnit", Order = 1)]
        [ControlGUI(Name = "Unit")]
        public QuantityUnit quantityUnit;

        [System.Xml.Serialization.XmlElementAttribute("quantityFrequency", Order = 2)]
        [ControlGUI(Name = "Frequency")]
        public CommodityQuantityFrequency quantityFrequency;

        [System.Xml.Serialization.XmlElementAttribute("quantity", Order = 3)]
        [ControlGUI(Name = "Quantity")]
        public EFS_Decimal quantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DeliveryQuantity)]
        public EFS_Id efs_id;
        #endregion Members

        #region Accessors
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
        #endregion Accessors
    }
    #endregion CommodityNotionalQuantity
    #region CommodityNotionalQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityNotionalQuantitySchedule
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Notional quantity")]
        public EFS_RadioChoice nq;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalStepSpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalStep", typeof(CommodityNotionalQuantity), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional step", IsVisible = true)]
        public CommodityNotionalQuantity nqNotionalStep;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqSettlementPeriodsNotionalQuantityScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsNotionalQuantitySchedule", typeof(CommoditySettlementPeriodsNotionalQuantitySchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods notional quantity schedule", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommoditySettlementPeriodsNotionalQuantitySchedule[] nqSettlementPeriodsNotionalQuantitySchedule;

        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommodityNotionalQuantitySchedule
    #region CommodityPricingDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommodityPricingDates
    {
        #region Members
        //<xsd:complexType name="CommodityPricingDates">
        //<xsd:sequence>
        //<xsd:group ref="CommodityCalculationPeriodsPointer.model"/>
        //<xsd:choice>
            //<xsd:sequence>
                //<xsd:element minOccurs="0" name="lag" type="Lag"/>
                //<xsd:choice>
                    //<xsd:sequence>
                        //<xsd:group ref="Days.model"/>
                        //<xsd:element minOccurs="0" name="businessCalendar" type="CommodityBusinessCalendar"/>
                        //<xsd:element minOccurs="0" name="calendarSource" type="CalendarSourceEnum"/>
                    //</xsd:sequence>
                    //<xsd:element maxOccurs="unbounded" name="settlementPeriods" type="SettlementPeriods"/>
                    //<xsd:element maxOccurs="unbounded" name="settlementPeriodsReference" type="SettlementPeriodsReference"/>
                //</xsd:choice>
            //</xsd:sequence>
            //<xsd:element maxOccurs="unbounded" name="pricingDates" type="AdjustableDates"/>
        //</xsd:choice>
        //</xsd:sequence>
        //<xsd:attribute name="id" type="xsd:ID"/>
        //</xsd:complexType>
        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lagSpecified;

        [System.Xml.Serialization.XmlElementAttribute("lag", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Lags")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Lag", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public Lag[] lag;


        #region Members CommodityCalculationPeriodsPointer
        #endregion Members CommodityCalculationPeriodsPointer
        #endregion Members
    }
    #endregion CommodityPricingDates
    #region CommoditySpread
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommoditySpread : Money
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadConversionFactorySpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Transmission contingency")]
        [System.Xml.Serialization.XmlElementAttribute("spreadConversionFactor", Order = 1)]
        public EFS_Decimal spreadConversionFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadUnitSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Spread unit")]
        [System.Xml.Serialization.XmlElementAttribute("spreadUnit", Order = 2)]
        public QuantityUnit spreadUnit;
        #endregion Members
    }
    #endregion CommoditySpread
    #region CommoditySpreadSchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommoditySpreadSchedule
    {
        #region Members
        [ControlGUI(IsLabel = true, Name = "Spread steps", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("spreadStep", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "step", MinItem = 1)]
        public CommoditySpread[] spreadStep;

        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer
        #endregion Members
    }
    #endregion CommoditySpreadSchedule
    #region CommodityPhysicalQuantity
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommodityPhysicalQuantity : CommodityPhysicalQuantityBase
    {
    }
    #endregion CommodityPhysicalQuantity
    #region CommodityPhysicalQuantityBase
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommodityPhysicalQuantity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ElectricityPhysicalQuantity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GasPhysicalQuantity))]
    public partial class CommodityPhysicalQuantityBase : ItemGUI
    {
        #region Members
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DeliveryQuantity)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommodityPhysicalQuantityBase
    #region CommodityQuantityFrequency
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityQuantityFrequency : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string quantityFrequencyScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public CommodityQuantityFrequency()
        {
            quantityFrequencyScheme = "http://www.fpml.org/coding-scheme/commodity-quantity-frequency";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.quantityFrequencyScheme = value; }
            get { return this.quantityFrequencyScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CommodityQuantityFrequency
    #region CommodityPayRelativeToEvent
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("marketDisruptionEvent", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CommodityPayRelativeToEvent : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string commodityPayRelativeToEventScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public CommodityPayRelativeToEvent()
        {
            commodityPayRelativeToEventScheme = "http://www.fpml.org/coding-scheme/commodity-pay-relative-to-event";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.commodityPayRelativeToEventScheme = value; }
            get { return this.commodityPayRelativeToEventScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion CommodityPayRelativeToEvent
    #region CommodityPhysicalQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityPhysicalQuantitySchedule
    {
        #region Members
        [ControlGUI(IsLabel = true, Name = "Quantity", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("quantityStep", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "step", MinItem = 1)]
        public CommodityNotionalQuantity[] quantityStep;

        [ControlGUI(Name = "Delivery period reference")]
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriodsReference", typeof(CalculationPeriodsReference), Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference deliveryPeriodsReference;

        [ControlGUI(Name = "Delivery periods schedule reference")]
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 3)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference deliveryPeriodsScheduleReference;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommodityPhysicalQuantitySchedule
    #region CommodityRelativePaymentDates
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommodityRelativePaymentDates
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payment")]
        public EFS_RadioChoice rt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rtPayRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payRelativeTo", typeof(CommodityPayRelativeToEnum), Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative to", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CommodityPayRelativeToEnum rtRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rtPayRelativeToEventSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payRelativeToEvent", typeof(CommodityPayRelativeToEvent), Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative to event", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CommodityPayRelativeToEvent rtRelativeToEvent;

        #region Members CommodityCalculationPeriodsPointer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriodsPointer

        [System.Xml.Serialization.XmlElementAttribute("paymentDaysOffset", Order = 6)]
        [ControlGUI(Name = "Payment days offset")]
        public DateOffset paymentDaysOffset;

        #region BusinessCentersOrReference.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;
        #endregion BusinessCentersOrReference.model

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommodityRelativePaymentDates
    #region CommoditySettlementPeriodsNotionalQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommoditySettlementPeriodsNotionalQuantitySchedule
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsNotionalQuantityStep", Order = 1)]
        [ControlGUI(IsLabel = true, Name = "Settlement periods notional quantity", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference notional quantity step", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsNotionalQuantityStep;

        [ControlGUI(IsLabel = true, Name = "Settlement periods", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion CommoditySettlementPeriodsNotionalQuantitySchedule
    #region CommoditySettlementPeriodsNotionalQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommoditySettlementPeriodsNotionalQuantity : CommodityNotionalQuantity
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion CommoditySettlementPeriodsNotionalQuantity
    #region CommoditySettlementPeriodsPriceSchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CommoditySettlementPeriodsPriceSchedule
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsPriceStep", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "step", MinItem = 1)]
        public FixedPrice[] settlementPeriodsPriceStep;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion CommoditySettlementPeriodsPriceSchedule

    #region CommoditySpot
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("commoditySpot", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20221201 [25639] [WI482] Add
    public partial class CommoditySpot : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", typeof(AdjustableOrRelativeDate), Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;

        [System.Xml.Serialization.XmlElementAttribute("terminationDate", typeof(AdjustableOrRelativeDate), Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;

        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(Currency), Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Settlement Currency", Width = 75)]
        public Currency settlementCurrency;

        [System.Xml.Serialization.XmlElementAttribute("fixedLeg", typeof(FixedPriceSpotLeg), Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot price leg", IsVisible = false, Color = MethodsGUI.ColorEnum.Green)]
        public FixedPriceSpotLeg fixedLeg;

        [System.Xml.Serialization.XmlElementAttribute("coalPhysicalLeg", typeof(CoalPhysicalLeg), Order = 5)]
        [System.Xml.Serialization.XmlElementAttribute("electricityPhysicalLeg", typeof(ElectricityPhysicalLeg), Order = 5)]
        [System.Xml.Serialization.XmlElementAttribute("environmentalPhysicalLeg", typeof(EnvironmentalPhysicalLeg), Order = 5)]
        [System.Xml.Serialization.XmlElementAttribute("gasPhysicalLeg", typeof(GasPhysicalLeg), Order = 5)]
        [System.Xml.Serialization.XmlElementAttribute("oilPhysicalLeg", typeof(OilPhysicalLeg), Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot price leg")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Physical leg", IsVisible = false, Color = MethodsGUI.ColorEnum.Green)]
        public PhysicalSwapLeg commodityPhysicalLeg;

        #region TrdCapRptSideGrp_Block[] RptSide
        // FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlElementAttribute("RptSide", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Physical leg")]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Mandatory)]
        //[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Sides")]
        //[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Side", IsMaster = true, IsChild = true, MinItem = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public TrdCapRptSideGrp_Block[] RptSide;
        #endregion TrdCapRptSideGrp_Block[] RptSide

        /*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Physical leg")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commonPricingSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Common pricing")]
        [System.Xml.Serialization.XmlElementAttribute("commonPricing", Order = 8)]
        public EFS_Boolean commonPricing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketDisruptionSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Market disruption")]
        [System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 9)]
        public CommodityMarketDisruption marketDisruption;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDisruptionSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Settlement disruption")]
        [System.Xml.Serialization.XmlElementAttribute("settlementDisruption", Order = 10)]
        public CommodityBullionSettlementDisruptionEnum settlementDisruption;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool roundingSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Settlement disruption")]
        [System.Xml.Serialization.XmlElementAttribute("settlementDisruption", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 11)]
        public Rounding rounding;
        */

        #region IdM (Représente le marché )
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idM;
        #endregion IdM

        #endregion Members
    }
    #endregion CommoditySpot

    #region CommoditySwap
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("commoditySwap", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CommoditySwap : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", typeof(AdjustableOrRelativeDate), Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;

        [System.Xml.Serialization.XmlElementAttribute("terminationDate", typeof(AdjustableOrRelativeDate), Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;

        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(Currency), Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Settlement Currency", Width = 75)]
        public Currency settlementCurrency;

        /*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type Date")]
        public EFS_RadioChoice leg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCommoditySwapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("coalPhysicalLeg", typeof(CoalPhysicalLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("electricityPhysicalLeg", typeof(ElectricityPhysicalLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("environmentPhysicalLeg", typeof(EnvironmentalPhysicalLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("fixedLeg", typeof(FixedPriceLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("floatingLeg", typeof(FloatingPriceLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("gasPhysicalLeg", typeof(GasPhysicalLeg), Order = 6)]
        [System.Xml.Serialization.XmlElementAttribute("oilPhysicalLeg", typeof(OilPhysicalLeg), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commodity Swap Legs", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1)]
        public CommoditySwapLeg[] legCommoditySwap;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legWeatherSpecified;
        [System.Xml.Serialization.XmlElementAttribute("weatherLeg", typeof(WeatherLeg), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Weather Legs", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2, MaxItem = 2)]
        public WeatherLeg[] legWeather;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commonPricingSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Common pricing")]
        [System.Xml.Serialization.XmlElementAttribute("commonPricing", Order = 8)]
        public EFS_Boolean commonPricing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketDisruptionSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Market disruption")]
        [System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 9)]
        public CommodityMarketDisruption marketDisruption;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDisruptionSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Settlement disruption")]
        [System.Xml.Serialization.XmlElementAttribute("settlementDisruption", Order = 10)]
        public CommodityBullionSettlementDisruptionEnum settlementDisruption;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool roundingSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Settlement disruption")]
        [System.Xml.Serialization.XmlElementAttribute("settlementDisruption", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 11)]
        public Rounding rounding;
        */
        #endregion Members
    }
    #endregion CommoditySwap
    #region CommoditySwapLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AveragePriceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FinancialSwapLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NonPeriodicFixedPriceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PhysicalSwapLeg))]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public abstract class CommoditySwapLeg : ItemGUI
    {
        #region Members
        //[System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        //public string id
        //{
        //    set { efs_id = new EFS_Id(value); }
        //    get
        //    {
        //        if (efs_id == null)
        //            return null;
        //        else
        //            return efs_id.Value;
        //    }
        //}
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CommodityLeg)]
        //public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CommoditySwapLeg
    #region CoalPhysicalLeg
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("coalPhysicalLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class CoalPhysicalLeg : PhysicalSwapLeg
    {
    }
    #endregion CoalPhysicalLeg

    #region DayOfWeekScheme
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("applicableDay", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class DayOfWeekScheme : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public DayOfWeekScheme()
        {
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { ; }
            get { return null; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion DayOfWeekScheme
    #region DisruptionFallback
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("disruptionFallback", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class DisruptionFallback : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string commodityMarketDisruptionFallbackScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public DisruptionFallback()
        {
            commodityMarketDisruptionFallbackScheme = "http://www.fpml.org/coding-scheme/commodity-market-disruption-fallback";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.commodityMarketDisruptionFallbackScheme = value; }
            get { return this.commodityMarketDisruptionFallbackScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion DisruptionFallback

    #region EEPParameters
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("eEPParameters", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20221201 [25639] [WI484] Partial
    public partial class EEPParameters
    {
        #region Members
        [ControlGUI(Name = "Applicable")]
        [System.Xml.Serialization.XmlElementAttribute("eEPApplicable", Order = 1)]
        public EFS_Boolean eEPApplicable;
        [ControlGUI(Name = "Risk period")]
        [System.Xml.Serialization.XmlElementAttribute("riskPeriod", Order = 2)]
        public EEPRiskPeriod riskPeriod;
        [ControlGUI(Name = "Equivalent applicable")]
        [System.Xml.Serialization.XmlElementAttribute("equivalentApplicable", Order = 3)]
        public EFS_Boolean equivalentApplicable;
        [ControlGUI(Name = "Penalty applicable")]
        [System.Xml.Serialization.XmlElementAttribute("penaltyApplicable", Order = 4)]
        public EFS_Boolean penaltyApplicable;
        #endregion Members
    }
    #endregion EEPParameters
    #region EEPRiskPeriod
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("riskPeriod", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class EEPRiskPeriod
    {
        #region Members
        [ControlGUI(Name = "Start date")]
        [System.Xml.Serialization.XmlElementAttribute("startDate", Order = 1)]
        public EFS_Date startDate;
        [ControlGUI(Name = "End date")]
        [System.Xml.Serialization.XmlElementAttribute("endDate", Order = 2)]
        public EFS_Date endDate;
        #endregion Members
    }
    #endregion EEPRiskPeriod

    #region ElectricityDelivery
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("electricityDelivery", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityDelivery : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Delivery")]
        public EFS_RadioChoice ed;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool edDeliveryPointSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryPoint", typeof(ElectricityDeliveryPoint), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Delivery point", IsVisible = true)]
        public ElectricityDeliveryPoint edDeliveryPoint;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool edDeliveryZoneSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryZone", typeof(CommodityDeliveryPoint), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Delivery zone", IsVisible = true)]
        public CommodityDeliveryPoint edDeliveryZone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryTypeSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Delivery type")]
        [System.Xml.Serialization.XmlElementAttribute("deliverytype", Order = 3)]
        public ElectricityDeliveryType deliveryType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transmissionContingencySpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transmission contingency")]
        [System.Xml.Serialization.XmlElementAttribute("transmissionContingency", Order = 4)]
        public ElectricityTransmissionContingency transmissionContingency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interconnectionPointSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interconnection point")]
        [System.Xml.Serialization.XmlElementAttribute("interconnectionPoint", Order = 5)]
        public InterconnectionPoint interconnectionPoint;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool electingPartyReferenceSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Electing party reference")]
        [System.Xml.Serialization.XmlElementAttribute("electingPartyReference", Order = 6)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference electingPartyReference;

        #endregion Members
    }
    #endregion ElectricityDelivery
    #region ElectricityDeliveryFirm
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityDeliveryFirm
    {
        #region Members
        [ControlGUI(Name = "Force majeure")]
        [System.Xml.Serialization.XmlElementAttribute("forceMajeure", Order = 1)]
        public EFS_Boolean forceMajeure;
        #endregion Members
    }
    #endregion ElectricityDeliveryFirm
    #region ElectricityDeliveryPoint
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityDeliveryPoint : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string deliveryPointScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.deliveryPointScheme = value; }
            get { return this.deliveryPointScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ElectricityDeliveryPoint
    #region ElectricityDeliverySystemFirm
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityDeliverySystemFirm
    {
        #region Members
        [ControlGUI(Name = "Applicable")]
        [System.Xml.Serialization.XmlElementAttribute("applicable", Order = 1)]
        public EFS_Boolean applicable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool systemSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "System")]
        [System.Xml.Serialization.XmlElementAttribute("system", typeof(CommodityDeliveryPoint), Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public CommodityDeliveryPoint system;
        #endregion Members
    }
    #endregion ElectricityDeliverySystemFirm
    #region ElectricityDeliveryType
    /// EG 20161122 New Commodity 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryType", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityDeliveryType : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Delivery type")]
        public EFS_RadioChoice type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFirmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firm", typeof(ElectricityDeliveryFirm), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Firm", IsVisible = true)]
        public ElectricityDeliveryFirm typeFirm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeNonFirmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nonFirm", typeof(EFS_Boolean), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Non firm", IsVisible = true)]
        public EFS_Boolean typeNonFirm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeSystemFirmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firm", typeof(ElectricityDeliverySystemFirm), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "System Firm", IsVisible = true)]
        public ElectricityDeliverySystemFirm typeSystemFirm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeUnitFirmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitFirm", typeof(ElectricityDeliveryUnitFirm), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Unit firm", IsVisible = true)]
        public ElectricityDeliveryUnitFirm typeUnitFirm;
        #endregion Members

    }
    #endregion ElectricityDeliveryType
    #region ElectricityDeliveryUnitFirm
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityDeliveryUnitFirm
    {
        #region Members
        [ControlGUI(Name = "Applicable")]
        [System.Xml.Serialization.XmlElementAttribute("applicable", Order = 1)]
        public EFS_Boolean applicable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool generationAssetSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "GenerationAsset")]
        [System.Xml.Serialization.XmlElementAttribute("generationAsset", typeof(CommodityDeliveryPoint), Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public CommodityDeliveryPoint generationAsset;
        #endregion Members
    }
    #endregion ElectricityDeliveryUnitFirm
    #region ElectricityPhysicalDeliveryQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityPhysicalDeliveryQuantity : CommodityNotionalQuantity
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion ElectricityPhysicalDeliveryQuantity
    #region ElectricityPhysicalDeliveryQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityPhysicalDeliveryQuantitySchedule : CommodityPhysicalQuantitySchedule
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion ElectricityPhysicalDeliveryQuantitySchedule
    /// EG 20221201 [25639] [WI484] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnvironmentalProductionFeatures
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool technologySpecified;

        [System.Xml.Serialization.XmlElementAttribute("technology", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Technology")]
        public CommodityTechnologyTypeEnum technology;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool regionSpecified;

        [System.Xml.Serialization.XmlElementAttribute("region", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Region")]
        public EnvironmentalProductionRegion region;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deviceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("device", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Device")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Device", IsClonable = true, IsChild = true)]
        public EnvironmentalProductionDevice[] device;
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnvironmentalProductionRegion : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string regionScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("device", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class EnvironmentalProductionDevice : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string deviceScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }

    #region ElectricityPhysicalLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("electricityPhysicalLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityPhysicalLeg : PhysicalSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriods", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery periods", IsVisible = false)]
        public CommodityDeliveryPeriods deliveryPeriods;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriods", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery periods")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement period", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public SettlementPeriods[] settlementPeriods;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementPeriodsScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsSchedule", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods schedule")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "schedule", IsClonable = true, IsChild = true, MinItem = 0)]
        public SettlementPeriodsSchedule[] settlementPeriodsSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool loadTypeSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Load type")]
        [System.Xml.Serialization.XmlElementAttribute("loadType", Order = 4)]
        public LoadTypeEnum loadType;

        [System.Xml.Serialization.XmlElementAttribute("electricity", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Electricity specifications", IsVisible = false)]
        public ElectricityProduct electricity;

        [System.Xml.Serialization.XmlElementAttribute("deliveryConditions", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Electricity specifications")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Electricity conditions", IsVisible = false)]
        public ElectricityDelivery deliveryConditions;

        [System.Xml.Serialization.XmlElementAttribute("deliveryQuantity", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Electricity conditions")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery quantity", IsVisible = false)]
        public ElectricityPhysicalQuantity deliveryQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery quantity")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion ElectricityPhysicalLeg
    #region ElectricityPhysicalQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryQuantity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityPhysicalQuantity : CommodityPhysicalQuantityBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("physicalQuantity", typeof(ElectricityPhysicalDeliveryQuantity), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "quantity", IsClonable = true, IsMaster = false, IsChild = true)]
        public ElectricityPhysicalDeliveryQuantity[] itemPhysicalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalQuantityScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("physicalQuantitySchedule", typeof(ElectricityPhysicalDeliveryQuantitySchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "quantity", IsClonable = true, IsMaster = false, IsChild = true)]
        public ElectricityPhysicalDeliveryQuantitySchedule[] itemPhysicalQuantitySchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalPhysicalQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("totalPhysicalQuantity", typeof(UnitQuantity), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total physical quantity")]
        public UnitQuantity totalPhysicalQuantity;
        #endregion Members

    }
    #endregion ElectricityPhysicalQuantity
    #region ElectricityProduct
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("electricity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Type")]
        public ElectricityProductTypeEnum typeElectricity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool voltageSpecified;

        [System.Xml.Serialization.XmlElementAttribute("voltage", typeof(EFS_PositiveDecimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Voltage")]
        public EFS_PositiveDecimal voltage;
        #endregion Members
    }
    #endregion ElectricityProduct
    #region ElectricityTransmissionContingency
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("transmissionContingency", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class ElectricityTransmissionContingency : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Type", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "contingency")]
        public ElectricityTransmissionContingencyType contingency;

        [System.Xml.Serialization.XmlElementAttribute("contingentParty", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contingent party reference", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2, MaxItem = 2)]
        public PartyReference[] contingentParty;
        #endregion Members
    }
    #endregion ElectricityTransmissionContingency
    #region ElectricityTransmissionContingencyType
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ElectricityTransmissionContingencyType : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string electricityTransmissionContingencyScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public ElectricityTransmissionContingencyType()
        {
            electricityTransmissionContingencyScheme = "http://www.fpml.org/coding-scheme/electricity-transmission-contingency";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.electricityTransmissionContingencyScheme = value; }
            get { return this.electricityTransmissionContingencyScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion ElectricityTransmissionContingencyType

    #region EnvironmentalProductApplicableLaw
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnvironmentalProductApplicableLaw : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string environmentalProductApplicableLawScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public EnvironmentalProductApplicableLaw()
        {
            environmentalProductApplicableLawScheme = "http://www.fpml.org/coding-scheme/governing-law";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.environmentalProductApplicableLawScheme = value; }
            get { return this.environmentalProductApplicableLawScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion EnvironmentalProductApplicableLaw
    #region EnvironmentalPhysicalLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("environmentalPhysicalLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20221201 [25639] [WI484] Partial + ProductionFeatures
    public partial class EnvironmentalPhysicalLeg : PhysicalSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("numberOfAllowances", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of allowances")]
        public UnitQuantity numberOfAllowances;

        [System.Xml.Serialization.XmlElementAttribute("environmental", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Environmental")]
        public EnvironmentalProduct environmental;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool abandonmentOfSchemeSpecified;

        [System.Xml.Serialization.XmlElementAttribute("abandonmentOfScheme", typeof(EnvironmentalAbandonmentOfSchemeEnum), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Voltage")]
        public EnvironmentalAbandonmentOfSchemeEnum abandonmentOfScheme;

        [System.Xml.Serialization.XmlElementAttribute("deliveryDate", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Delivery date")]
        public AdjustableOrRelativeDate deliveryDate;

        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Payment date")]
        public DateOffset paymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToDeliverApplicableSpecified;

        [System.Xml.Serialization.XmlElementAttribute("failureToDeliverApplicable", typeof(EFS_Boolean), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Voltage")]
        public EFS_Boolean failureToDeliverApplicable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool eEPParametersSpecified;

        [System.Xml.Serialization.XmlElementAttribute("eEPParameters", typeof(EEPParameters), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "EEP parameters")]
        public EEPParameters eEPParameters;
        #endregion Members

        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool productionFeaturesSpecified;

        [System.Xml.Serialization.XmlElementAttribute("productionFeatures", typeof(EnvironmentalProductionFeatures), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 10)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Production Features", IsVisible = false)]
        public EnvironmentalProductionFeatures productionFeatures;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Production Features")]
        public bool FillBalise2;
        #endregion Members

    }
    #endregion EnvironmentalPhysicalLeg
    #region EnvironmentalProduct
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("environmental", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20221201 [25639] [WI484] Partial
    public partial class EnvironmentalProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("productType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Type")]
        public EnvironmentalProductTypeEnum productType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compliancePeriodSpecified;

        [System.Xml.Serialization.XmlElementAttribute("compliancePeriod", typeof(EnvironmentalProductComplaincePeriod), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Compliance period")]
        public EnvironmentalProductComplaincePeriod compliancePeriod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool vintageSpecified;

        [System.Xml.Serialization.XmlElementAttribute("vintage", typeof(EFS_Integer), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Vintage", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 0)]
        public EFS_Integer[] vintage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applicableLawSpecified;

        [System.Xml.Serialization.XmlElementAttribute("applicableLaw", typeof(EnvironmentalProductApplicableLaw), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Applicable law")]
        public EnvironmentalProductApplicableLaw applicableLaw;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trackingSystemSpecified;

        [System.Xml.Serialization.XmlElementAttribute("trackingSystem", typeof(EnvironmentalTrackingSystem), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Tracking system")]
        public EnvironmentalTrackingSystem trackingSystem;
        #endregion Members
    }
    #endregion EnvironmentalProduct
    #region EnvironmentalProductComplaincePeriod
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20221201 [25639] [WI484] Partial + EFS_String
    public partial class EnvironmentalProductComplaincePeriod
    {
        #region Members
        [ControlGUI(Name = "Start year")]
        //[System.Xml.Serialization.XmlElementAttribute("startYear", DataType = "gYear", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("startYear", Order = 1)]
        public EFS_String startYear;
        [ControlGUI(Name = "End year")]
        //[System.Xml.Serialization.XmlElementAttribute("endYear", DataType = "gYear", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("endYear", Order = 2)]
        public EFS_String endYear;
        #endregion Members
    }
    #endregion EnvironmentalProductComplaincePeriod
    #region EnvironmentalTrackingSystem
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnvironmentalTrackingSystem : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string commodityEnvironmentalTrackingSystemScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public EnvironmentalTrackingSystem()
        {
            commodityEnvironmentalTrackingSystemScheme = "http://www.fpml.org/coding-scheme/commodity-environmental-tracking-system";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.commodityEnvironmentalTrackingSystemScheme = value; }
            get { return this.commodityEnvironmentalTrackingSystemScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion EnvironmentalTrackingSystem

    #region FinancialSwapLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AveragePriceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixedPriceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixedPriceSpotLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FloatingPriceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(WeatherLeg))]
    public abstract partial class FinancialSwapLeg : CommoditySwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", typeof(PartyOrAccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Payer")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payerAccountReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("payerAccountReference", typeof(AccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Payer account")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference payerAccountReference;

        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", typeof(PartyOrAccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool receiverAccountReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("receiverAccountReference", typeof(AccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Receiver account", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference receiverAccountReference;
        #endregion Members
    }
    #endregion FinancialSwapLeg

    #region FixedPrice
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fixedPrice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class FixedPrice : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("price", Order = 1)]
        [ControlGUI(Name = "Price")]
        public EFS_Decimal price;

        [System.Xml.Serialization.XmlElementAttribute("priceCurrency", Order = 2)]
        [ControlGUI(Name = "Currency")]
        public Currency priceCurrency;

        [System.Xml.Serialization.XmlElementAttribute("priceUnit", Order = 3)]
        [ControlGUI(Name = "Unit", LineFeed=MethodsGUI.LineFeedEnum.After)]
        public QuantityUnit priceUnit;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion FixedPrice
    #region FixedPriceLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fixedLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class FixedPriceLeg : FinancialSwapLeg
    {
        #region Members CommodityCalculationPeriods.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates", IsVisible = true)]
        public AdjustableDates calculationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriods", typeof(AdjustableDates), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods", IsVisible = true)]
        public AdjustableDates calculationPeriods;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsSchedule", typeof(CommodityCalculationPeriodsSchedule), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        public CommodityCalculationPeriodsSchedule calculationPeriodsSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference calculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference calculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodsDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference calculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriods.model

        #region Members CommodityFixedPrice.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Price")]
        public EFS_RadioChoice price;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceFixedPriceScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPriceSchedule", typeof(CommodityFixedPriceSchedule), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed price Schedule", IsVisible = true)]
        public CommodityFixedPriceSchedule priceFixedPriceSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceFixedPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPrice", typeof(FixedPrice), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed price", IsVisible = true)]
        public FixedPrice priceFixedPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceWorldscaleRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("worldscaleRate", typeof(EFS_Decimal), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Contract rate", IsVisible = true)]
        public EFS_Decimal priceWorldscaleRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceContractRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractRate", typeof(NonNegativeMoney), Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Contract rate", IsVisible = true)]
        public NonNegativeMoney priceContractRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceSettlementPeriodsPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsPrice", typeof(SettlementPeriodsFixedPrice), Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods fixed price", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public SettlementPeriodsFixedPrice[] priceSettlementPeriodsPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalPriceSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Total price")]
        [System.Xml.Serialization.XmlElementAttribute("totalPrice", Order = 12)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public NonNegativeMoney totalPrice;
        #endregion Members CommodityFixedPrice.model

        #region Members CommodityNotionalQuantity.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Notional quantity")]
        public EFS_RadioChoice nq;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantityScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantitySchedule", typeof(CommodityNotionalQuantitySchedule), Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity schedule", IsVisible = true)]
        public CommodityNotionalQuantitySchedule nqNotionalQuantitySchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantity", typeof(CommodityNotionalQuantity), Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity", IsVisible = true)]
        public CommodityNotionalQuantity nqNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqSettlementPeriodsNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsNotionalQuantity", typeof(CommoditySettlementPeriodsNotionalQuantity), Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods notional quantity", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommoditySettlementPeriodsNotionalQuantity[] nqSettlementPeriodsNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqQuantityReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("quantityReference", typeof(QuantityReference), Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quantity reference", IsVisible = true)]
        public QuantityReference nqQuantityReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("totalNotionalQuantity", typeof(EFS_Decimal), Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Total notional quantity")]
        public EFS_Decimal totalNotionalQuantity;
        #endregion Members CommodityNotionalQuantity.model

        #region Members CommodityPaymentDates.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payment dates")]
        public EFS_RadioChoice payment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentRelativePaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativePaymentDates", typeof(CommodityRelativePaymentDates), Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative payment dates", IsVisible = true)]
        public CommodityRelativePaymentDates paymentRelativePaymentDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(AdjustableDatesOrRelativeDateOffset), Order = 19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payment dates", IsVisible = true)]
        public AdjustableDatesOrRelativeDateOffset paymentPaymentDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentMasterAgreementPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementPaymentDates", typeof(EFS_Boolean), Order = 20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Master agreement payment dates", IsVisible = true)]
        public EFS_Boolean paymentMasterAgreementPaymentDates;
        #endregion Members CommodityPaymentDates.model

        #region Members CommodityFreightFlatRate.model
        [System.Xml.Serialization.XmlElementAttribute("flatRate", Order = 21)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Flate rate")]
        public FlatRateEnum flatRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool flatRateAmountSpecified;

        [System.Xml.Serialization.XmlElementAttribute("flatRateAmount", typeof(NonNegativeMoney), Order = 22)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Flate rate amount")]
        public NonNegativeMoney flatRateAmount;
        #endregion Members CommodityFreightFlatRate.model
    }
    #endregion FixedPriceLeg
    #region FixedPriceSpotLeg
    /// EG 20161122 New Commodity Derivative
    /// FI 20170116 [21916] Modify (commodityPayment devient grossAmount)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fixedLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class FixedPriceSpotLeg : FinancialSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fixedPrice", Order = 7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot price", IsVisible = true)]
        public FixedPrice fixedPrice;
        #endregion Members

        #region Members CommodityNotionalQuantity.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Notional quantity")]
        public EFS_RadioChoice nq;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantityScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantitySchedule", typeof(CommodityNotionalQuantitySchedule), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity schedule", IsVisible = true)]
        public CommodityNotionalQuantitySchedule nqNotionalQuantitySchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantity", typeof(CommodityNotionalQuantity), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity", IsVisible = true)]
        public CommodityNotionalQuantity nqNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqSettlementPeriodsNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsNotionalQuantity", typeof(CommoditySettlementPeriodsNotionalQuantity), Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods notional quantity", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommoditySettlementPeriodsNotionalQuantity[] nqSettlementPeriodsNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqQuantityReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("quantityReference", typeof(QuantityReference), Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quantity reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DeliveryQuantity)]
        public QuantityReference nqQuantityReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("totalNotionalQuantity", typeof(EFS_Decimal), Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total notional quantity")]
        public EFS_Decimal totalNotionalQuantity;
        #endregion Members CommodityNotionalQuantity.model

        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Gross Amount", Color = MethodsGUI.ColorEnum.Orange)]
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", Order = 16)]
        public Payment grossAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commodity payment")]
        public bool FillBalise;

        #region Members ID
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CommodityLeg)]
        public EFS_Id efs_id;
        #endregion Members ID
    }
    #endregion FixedPriceSpotLeg
    #region FloatingLegCalculation
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("floatingLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class FloatingLegCalculation : FinancialSwapLeg
    {
        [System.Xml.Serialization.XmlElementAttribute("pricingDates", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Pricing dates")]
        public CommodityPricingDates pricingDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingMethodSpecified;

        [System.Xml.Serialization.XmlElementAttribute("averagingMethod", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Averaging method")]
        public AveragingMethodEnum averagingMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool conversionFactorSpecified;

        [System.Xml.Serialization.XmlElementAttribute("conversionFactor", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Averaging method")]
        public EFS_Decimal conversionFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool roundingSpecified;

        [System.Xml.Serialization.XmlElementAttribute("rounding", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Rounding")]
        public Rounding rounding;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread")]
        public EFS_RadioChoice spread;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty spreadNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spread", typeof(CommoditySpread), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CommoditySpread spreadSpread;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpreadScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("spreadSchedule", typeof(CommoditySpreadSchedule), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commodity Swap Legs", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1)]
        public CommoditySpreadSchedule[] spreadSpreadSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpreadPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spread", typeof(EFS_Decimal), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spreadSpreadPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxSpecified;

        [System.Xml.Serialization.XmlElementAttribute("fx", Order = 8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Fx")]
        public CommodityFx fx;
    }
    #endregion FloatingLegCalculation
    #region FloatingPriceLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("floatingLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class FloatingPriceLeg : FinancialSwapLeg
    {
        #region Members CommodityCalculationPeriods.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationDates", typeof(AdjustableDates), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates", IsVisible = true)]
        public AdjustableDates itemCalculationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationPeriodsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriods", typeof(AdjustableDates), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods", IsVisible = true)]
        public AdjustableDates itemCalculationPeriods;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationPeriodsScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsSchedule", typeof(CommodityCalculationPeriodsSchedule), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        public CommodityCalculationPeriodsSchedule itemCalculationPeriodsSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsReference", typeof(CalculationPeriodsReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference itemCalculationPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference itemCalculationPeriodsScheduleReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCalculationDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodsDatesReference", typeof(CalculationPeriodsDatesReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dates reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public CalculationPeriodsDatesReference itemCalculationPeriodsDatesReference;
        #endregion Members CommodityCalculationPeriods.model

        [System.Xml.Serialization.XmlElementAttribute("commodity", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Commodity")]
        public Commodity commodity;

        #region Members CommodityNotionalQuantity.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Notional quantity")]
        public EFS_RadioChoice nq;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantityScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantitySchedule", typeof(CommodityNotionalQuantitySchedule), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity schedule", IsVisible = true)]
        public CommodityNotionalQuantitySchedule nqNotionalQuantitySchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("notionalQuantity", typeof(CommodityNotionalQuantity), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional quantity", IsVisible = true)]
        public CommodityNotionalQuantity nqNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqSettlementPeriodsNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsNotionalQuantity", typeof(CommoditySettlementPeriodsNotionalQuantity), Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement periods notional quantity", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommoditySettlementPeriodsNotionalQuantity[] nqSettlementPeriodsNotionalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nqQuantityReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("quantityReference", typeof(QuantityReference), Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quantity reference", IsVisible = true)]
        public QuantityReference nqQuantityReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalNotionalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("totalNotionalQuantity", typeof(EFS_Decimal), Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Total notional quantity")]
        public EFS_Decimal totalNotionalQuantity;
        #endregion Members CommodityNotionalQuantity.model

        [System.Xml.Serialization.XmlElementAttribute("calculation", Order = 13)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Type")]
        public FloatingLegCalculation calculation;

        #region Members CommodityPaymentDates.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payment dates")]
        public EFS_RadioChoice payment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentRelativePaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativePaymentDates", typeof(CommodityRelativePaymentDates), Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative payment dates", IsVisible = true)]
        public CommodityRelativePaymentDates paymentRelativePaymentDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(AdjustableDatesOrRelativeDateOffset), Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payment dates", IsVisible = true)]
        public AdjustableDatesOrRelativeDateOffset paymentPaymentDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentMasterAgreementPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementPaymentDates", typeof(EFS_Boolean), Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Master agreement payment dates", IsVisible = true)]
        public EFS_Boolean paymentMasterAgreementPaymentDates;
        #endregion Members CommodityPaymentDates.model

        #region Members CommodityFreightFlatRate.model
        [System.Xml.Serialization.XmlElementAttribute("flatRate", Order = 17)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Flate rate")]
        public FlatRateEnum flatRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool flatRateAmountSpecified;

        [System.Xml.Serialization.XmlElementAttribute("flatRateAmount", typeof(NonNegativeMoney), Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Flate rate amount")]
        public NonNegativeMoney flatRateAmount;
        #endregion Members CommodityFreightFlatRate.model
    }
    #endregion FloatingPriceLeg

    #region GasDelivery
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryConditions", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasDelivery : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Delivery point")]
        public EFS_RadioChoice gd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gdDeliveryPointSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryPoint", typeof(GasDeliveryPoint), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Point", IsVisible = true)]
        public GasDeliveryPoint gdDeliveryPoint;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gdEntryPointSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entryPoint", typeof(CommodityDeliveryPoint), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Entry point", IsVisible = true)]
        public CommodityDeliveryPoint gdEntryPoint;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gdWithdrawalPointSpecified;
        [System.Xml.Serialization.XmlElementAttribute("withdrawalPoint", typeof(CommodityDeliveryPoint), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Withdrawal point", IsVisible = true)]
        public CommodityDeliveryPoint gdWithdrawalPoint;

        [System.Xml.Serialization.XmlElementAttribute("deliveryType", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed,Name = "Type")]
        public CommodityDeliveryTypeEnum deliveryType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interconnectionPointSpecified;

        [System.Xml.Serialization.XmlElementAttribute("interconnectionPoint", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interconnection point")]
        public InterconnectionPoint interconnectionPoint;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerHubSpecified;

        [System.Xml.Serialization.XmlElementAttribute("buyerHub", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Buyer hub")]
        public CommodityHub buyerHub;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerHubSpecified;

        [System.Xml.Serialization.XmlElementAttribute("sellerHub", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seller hub")]
        public CommodityHub sellerHub;
        #endregion Members
    }
    #endregion GasDelivery
    #region GasDeliveryPeriods
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryPeriods", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasDeliveryPeriods : CommodityDeliveryPeriods
    {
        #region Members

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool supplyStartTimeSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supply start time")]
        [System.Xml.Serialization.XmlElementAttribute("supplyStartTime", Order = 1)]
        public PrevailingTime supplyStartTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool supplyEndTimeSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Supply end time")]
        [System.Xml.Serialization.XmlElementAttribute("supplyEndTime", Order = 2)]
        public PrevailingTime supplyEndTime;
        #endregion Members
    }
    #endregion GasDeliveryPeriods
    #region GasDeliveryPoint
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryPoint", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasDeliveryPoint : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "deliveryPointScheme", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string deliveryPointECCScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsLabel = true, Name = "xxx", Width = 75)]
        public string Value;
        #endregion Members
    }
    #endregion GasDeliveryPoint
    #region GasProduct
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("gas", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasProduct : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public GasProductTypeEnum typeGas;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others characteristics")]
        public EFS_RadioChoice complement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool complementNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty complementNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool complementCalorificValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calorificValue", typeof(EFS_NonNegativeDecimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Calorific value", IsVisible = true)]
        public EFS_NonNegativeDecimal complementCalorificValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool complementQualitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("quality", typeof(GasQuality), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quality", IsVisible = true)]
        public GasQuality complementQuality;
        #endregion Members
    }
    #endregion GasProduct
    #region GasPhysicalLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("gasPhysicalLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasPhysicalLeg : PhysicalSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriods", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery periods", IsVisible = false)]
        public GasDeliveryPeriods deliveryPeriods;
        [System.Xml.Serialization.XmlElementAttribute("gas", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery periods")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gas specifications", IsVisible = false)]
        public GasProduct gas;
        [System.Xml.Serialization.XmlElementAttribute("deliveryConditions", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gas specifications")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery conditions", IsVisible = false)]
        public GasDelivery deliveryConditions;
        [System.Xml.Serialization.XmlElementAttribute("deliveryQuantity", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery conditions")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery quantity", IsVisible = false)]
        public GasPhysicalQuantity deliveryQuantity;
        #endregion Members

        #region Members ID
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Delivery quantity")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CommodityLeg)]
        public EFS_Id efs_id;
        #endregion Members ID
    }
    #endregion GasPhysicalLeg
    #region GasPhysicalQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deliveryQuantity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class GasPhysicalQuantity : CommodityPhysicalQuantityBase
    {
        #region Members
        #region Members CommodityFixedPhysicalQuantity.model
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Delivery quantity")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("physicalQuantity", typeof(CommodityNotionalQuantity), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "quantity", IsClonable = true, IsMaster = false, IsMasterVisible = false, IsChild = true)]
        public CommodityNotionalQuantity[] itemPhysicalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalQuantityScheduleSpecified;

        [System.Xml.Serialization.XmlElementAttribute("physicalQuantitySchedule", typeof(CommodityPhysicalQuantitySchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "quantity", IsClonable = true, IsMaster = false, IsMasterVisible = false, IsChild = true)]
        public CommodityPhysicalQuantitySchedule[] itemPhysicalQuantitySchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalPhysicalQuantitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("totalPhysicalQuantity", typeof(UnitQuantity), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total physical quantity")]
        public UnitQuantity totalPhysicalQuantity;
        #endregion CommodityFixedPhysicalQuantity.model

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minPhysicalQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("minPhysicalQuantity", typeof(CommodityNotionalQuantity), Order = 4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Min physical quantity", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommodityNotionalQuantity[] minPhysicalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maxPhysicalQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maxPhysicalQuantity", typeof(CommodityNotionalQuantity), Order = 5)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Max physical quantity", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public CommodityNotionalQuantity[] maxPhysicalQuantity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool electingPartySpecified;

        [System.Xml.Serialization.XmlElementAttribute("electingParty", typeof(PartyReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Electing party reference")]
        public PartyReference electingParty;
        #endregion Members
    }
    #endregion GasPhysicalQuantity
    #region GasQuality
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class GasQuality : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string gasQualityScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public GasQuality()
        {
            gasQualityScheme = "http://www.fpml.org/coding-scheme/commodity-gas-quality";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.gasQualityScheme = value; }
            get { return this.gasQualityScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion GasQuality

    #region InterconnectionPoint
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("interconnectionPoint", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class InterconnectionPoint : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string interconnectionPointScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public InterconnectionPoint()
        {
            interconnectionPointScheme = "http://www.fpml.org/coding-scheme/external/eic-codes";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.interconnectionPointScheme = value; }
            get { return this.interconnectionPointScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion InterconnectionPoint

    #region Lag
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class Lag
    {
        #region Members
        [ControlGUI(Name = "duration")]
        [System.Xml.Serialization.XmlElementAttribute("lagDuration", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public Interval lagDuration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstObservationDateOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstObservationDateOffset", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First observation date offset")]
        public Interval firstObservationDateOffset;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Lag

    #region MarketDisruptionEvent
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("marketDisruptionEvent", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class MarketDisruptionEvent : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string commodityMarketDisruptionScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public MarketDisruptionEvent()
        {
            commodityMarketDisruptionScheme = "http://www.fpml.org/coding-scheme/commodity-market-disruption";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.commodityMarketDisruptionScheme = value; }
            get { return this.commodityMarketDisruptionScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion MarketDisruptionEvent

    #region NonNegativeMoney
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class NonNegativeMoney : Money
    {
        #region Constructors
        public NonNegativeMoney()
            : base()
        {
        }
        public NonNegativeMoney(Decimal pAmount, string pCur)
            : base(pAmount, pCur)
        {
        }
        #endregion Constructors
    }
    #endregion NonNegativeMoney
    #region NonPeriodicFixedPriceLeg
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("nonPeriodicFixedPriceLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class NonPeriodicFixedPriceLeg : CommoditySwapLeg
    {
    }
    #endregion NonPeriodicFixedPriceLeg

    #region OilPhysicalLeg
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("oilPhysicalLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class OilPhysicalLeg : PhysicalSwapLeg
    {
    }
    #endregion OilPhysicalLeg

    #region OffsetPrevailingTime
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OffsetPrevailingTime : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("time", Order = 1)]
        //[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Time")]
        public PrevailingTime time;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool offsetSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Offset")]
        //[System.Xml.Serialization.XmlElementAttribute("offset", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("offset", typeof(Offset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        public Offset offset;
        #endregion Members
    }
    #endregion OffsetPrevailingTime

    #region PhysicalSwapLeg
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GasPhysicalLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OilPhysicalLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ElectricityPhysicalLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CoalPhysicalLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EnvironmentalPhysicalLeg))]
    public abstract partial class PhysicalSwapLeg : CommoditySwapLeg
    {
        #region Members
        [ControlGUI(Name = "Payer")]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", typeof(PartyOrAccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payerAccountReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("payerAccountReference", typeof(AccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Payer account")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference payerAccountReference;

        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", typeof(PartyOrAccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool receiverAccountReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("receiverAccountReference", typeof(AccountReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Receiver account", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Account)]
        public AccountReference receiverAccountReference;

        #endregion Members

        #region Members Asset commodity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commodityAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("commodityAsset", typeof(CommodityAsset), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset")]
        public CommodityAsset commodityAsset;
        #endregion Members Asset commodity
    }
    #endregion PhysicalSwapLeg
    #region PrevailingTime
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PrevailingTime : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("hourMinuteTime", typeof(HourMinuteTime), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [ControlGUI(Name = "Time")]
        public HourMinuteTime hourMinuteTime;

        [System.Xml.Serialization.XmlElementAttribute("location", Order = 2)]
        [ControlGUI(Name = "Timezone")]
        public TimezoneLocation location;
        #endregion Members
    }
    #endregion PrevailingTime

    #region QuantityReference
    /// EG 20161122 New Commodity Derivative
    public partial class QuantityReference : HrefGUI, ICloneable, IReference
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
        #region Constructor
        public QuantityReference() { }
        public QuantityReference(string pReference)
        {
            this.href = pReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            QuantityReference clone = new QuantityReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion QuantityReference
    #region QuantityUnit
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class QuantityUnit : SchemeGUI, IScheme
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string quantityUnitScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members

        #region Constructors
        public QuantityUnit()
        {
            quantityUnitScheme = "http://www.fpml.org/coding-scheme/price-quote-units";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.quantityUnitScheme = value; }
            get { return this.quantityUnitScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion QuantityUnit

    #region SequencedDisruptionFallback
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("sequencedDisruptionFallback", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class SequencedDisruptionFallback
    {
        [ControlGUI(Name = "Minimum futures contracts")]
        [System.Xml.Serialization.XmlElementAttribute("fallback", Order = 1)]
        public DisruptionFallback fallback;

        [ControlGUI(Name = "Minimum futures contracts")]
        [System.Xml.Serialization.XmlElementAttribute("sequence", Order = 2)]
        public EFS_PosInteger sequence;
    }
    #endregion SequencedDisruptionFallback
    #region SettlementPeriods
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("settlementPeriods", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20221201 [25639] [WI484] Upd Gestion Array d'enums
    public partial class SettlementPeriods : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("duration", Order = 1)]
        [ControlGUI(Name = "Duration")]
        public SettlementPeriodDurationEnum duration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool applicableDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("applicableDay", typeof(DayOfWeekExtEnum), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        //public DayOfWeekExtEnum[] applicableDay;
        public DayOfWeekExtEnum[] ApplicableDay
        {
            set
            {
                if (ArrFunc.IsFilled(value))
                {
                    efs_applicableDay = new EFS_StringArray[]{ };
                    List<EFS_StringArray> lst = new List<EFS_StringArray>();
                    value.ToList().ForEach(item => lst.Add(new EFS_StringArray(item.ToString())));
                    efs_applicableDay = lst.ToArray();
                }
            }
            get 
            {
                DayOfWeekExtEnum[] ret = null;
                if (ArrFunc.IsFilled(efs_applicableDay))
                {
                    List<DayOfWeekExtEnum> lst = new List<DayOfWeekExtEnum>();
                    efs_applicableDay.ToList().ForEach(item => lst.Add(ReflectionTools.ConvertStringToEnum<DayOfWeekExtEnum>(item.Value)));
                    return lst.ToArray();
                }
                return ret;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Applicable days", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MaxItem = 7)]
        public EFS_StringArray[] efs_applicableDay;

        [System.Xml.Serialization.XmlElementAttribute("startTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Start time", IsVisible = false)]
        public OffsetPrevailingTime startTime;

        [System.Xml.Serialization.XmlElementAttribute("endTime", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Start time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "End time", IsVisible = false)]
        public OffsetPrevailingTime endTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timeDurationSpecified;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "End time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time duration")]
        [System.Xml.Serialization.XmlElementAttribute("timeDuration", Order = 5)]
        public EFS_Time timeDuration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business calendar")]
        public EFS_RadioChoice calendar;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calendarNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty calendarNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calendarExcludeHolidaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("excludeHolidays", typeof(CommodityBusinessCalendar), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Exclude holidays", IsVisible = false)]
        public CommodityBusinessCalendar calendarExcludeHolidays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calendarIncludeHolidaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("includeHolidays", typeof(CommodityBusinessCalendar), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Include holidays", IsVisible = false)]
        public CommodityBusinessCalendar calendarIncludeHolidays;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SettlementPeriods)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion SettlementPeriods
    #region SettlementPeriodsFixedPrice
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("settlementPeriodsPrice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class SettlementPeriodsFixedPrice : FixedPrice
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion SettlementPeriodsFixedPrice

    #region SettlementPeriodsReference
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")] 
    public partial class SettlementPeriodsReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SettlementPeriods)]
        public string href;
        #endregion Members
    }
    #endregion SettlementPeriodsReference
    #region SettlementPeriodsSchedule
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SettlementPeriodsSchedule //: ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsStep", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Steps")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Step", IsMaster = true, IsMasterVisible = false , IsChild = true, MinItem = 1)]
        public SettlementPeriodsStep[] settlementPeriodsStep;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Calculation")]
        public EFS_RadioChoice dc;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dcPeriodsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriodsReference", typeof(CalculationPeriodsReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriods)]
        public CalculationPeriodsReference dcPeriodsReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dcPeriodsScheduleReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriodsScheduleReference", typeof(CalculationPeriodsScheduleReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periods schedule", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodsSchedule)]
        public CalculationPeriodsScheduleReference dcPeriodsScheduleReference;
        #endregion Members
    }
    #endregion SettlementPeriodsSchedule
    #region SettlementPeriodsStep
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("settlementPeriodsStep", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class SettlementPeriodsStep : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementPeriodsReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement periods reference", IsMaster = true, IsChild = true, MinItem = 1)]
        public ArraySettlementPeriodsReference[] settlementPeriodsReference;
        #endregion Members
    }
    #endregion SettlementPeriodsStep

    #region TimezoneLocation
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TimezoneLocation : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string timezoneLocationScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion TimeZoneLocation

    #region UnitQuantity
    /// EG 20161122 New Commodity Derivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("numberOfAllowances", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class UnitQuantity : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quantityUnit", Order = 1)]
        [ControlGUI(Name = "Unit")]
        public QuantityUnit quantityUnit;
        [System.Xml.Serialization.XmlElementAttribute("quantity", Order = 2)]
        [ControlGUI(Name = "Quantity")]
        public EFS_NonNegativeDecimal quantity;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion UnitQuantity

    #region WeatherLeg
    /// EG 20161122 New Commodity Derivative : NOT IN SCOPE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("weatherLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class WeatherLeg : FinancialSwapLeg
    {
    }
    #endregion WeatherLeg
}

