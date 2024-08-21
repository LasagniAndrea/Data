#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v20;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Ird;
using FpML.v42.Shared;
using FpML.v42.Asset;
using FpML.v42.EqShared;

#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision
namespace FpML.v42.Eqs
{
    #region AdditionalPaymentAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdditionalPaymentAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("formula", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula formula;
		#endregion Members
	}
    #endregion AdditionalPaymentAmount
    #region AssetSwapLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityLeg))]
    public partial class AssetSwapLeg
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
		[ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string legIdentifier
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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion AssetSwapLeg

    #region EquityAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityVarianceAmount))]
    public partial class EquityAmount : LegAmount
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Cash Settlement")]
        public EFS_Boolean cashSettlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeDividendsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionsExchangeDividends", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Dividends")]
        public EFS_Boolean optionsExchangeDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalDividendsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("additionalDividends", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "additional Dividends")]
        public EFS_Boolean additionalDividends;
		#endregion Members
	}
    #endregion EquityAmount
    #region EquityLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equityLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class EquityLeg : AssetSwapLeg
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;
		[System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;
		[System.Xml.Serialization.XmlElementAttribute("underlyer", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        public Underlyer underlyer;
		[System.Xml.Serialization.XmlElementAttribute("valuation", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuations", IsVisible = false)]
        public EquitySwapValuation valuation;
		[System.Xml.Serialization.XmlElementAttribute("notional", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuations")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional", IsVisible = false)]
        public EquitySwapNotional notional;
		[System.Xml.Serialization.XmlElementAttribute("equityAmount", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Amount", IsVisible = false)]
        public EquityAmount equityAmount;
		[System.Xml.Serialization.XmlElementAttribute("return", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Return conditions", IsVisible = false)]
        public Return @return;
		[System.Xml.Serialization.XmlElementAttribute("notionalAdjustments", Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Return conditions")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Notional Adjustments")]
        public NotionalAdjustmentEnum notionalAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX feature")]
        public EqShared.FxFeature fxFeature;
		#endregion Members
    }
    #endregion EquityLeg
    #region EquityPaymentDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquityPaymentDates
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityPaymentDatesInterimSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityPaymentDatesInterim", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Payment Dates Interim")]
        public AdjustableOrRelativeDates equityPaymentDatesInterim;
		[System.Xml.Serialization.XmlElementAttribute("equityPaymentDateFinal", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Payment Final Date", IsVisible = false)]
        public AdjustableOrRelativeDate equityPaymentDateFinal;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Payment Final Date")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentDates)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion EquityPaymentDates

    #region EquitySwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equitySwap", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class EquitySwap : EquitySwapBase
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeFeatures", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Principal Exchanges Features")]
        public PrincipalExchangeFeatures principalExchangeFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public EquitySwapAdditionalPayment[] additionalPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTermination",Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Early Termination", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public EquitySwapEarlyTerminationType[] earlyTermination;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extraordinaryEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extraordinaryEvents", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extraordinary Events")]
        public ExtraordinaryEvents extraordinaryEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PrincipalExchangeFeatures efs_InitialPrincipalExchangeFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PrincipalExchangeFeatures efs_PrincipalExchangeFeatures;
		#endregion Members
    }
    #endregion EquitySwap
    #region EquitySwapAdditionalPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquitySwapAdditionalPayment
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("additionalPaymentAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Amount", IsVisible = false)]
        public AdditionalPaymentAmount additionalPaymentAmount;
		[System.Xml.Serialization.XmlElementAttribute("additionalPaymentDate", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Date", IsVisible = false)]
        public AdjustableOrRelativeDate additionalPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentType", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public PaymentType paymentType;
		#endregion Members
    }
    #endregion EquitySwapAdditionalPayment
    #region EquitySwapBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwap))]
    public partial class EquitySwapBase : Product
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerPartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Buyer")]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerPartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Seller")]
        public PartyReference sellerPartyReference;
        /*
        [System.Xml.Serialization.XmlElementAttribute("interestLeg", typeof(InterestLeg))]
        [System.Xml.Serialization.XmlElementAttribute("varianceLeg", typeof(VarianceLeg))]
        [System.Xml.Serialization.XmlElementAttribute("equityLeg", typeof(EquityLeg))]
        [ArrayDivGUI(Level=MethodsGUI.LevelEnum.First,Name="Asset Swap Leg",IsClonable=true,IsMaster=true,
             IsMasterVisible=true,IsChild=true,MinItem=1)]
        public AssetSwapLeg[] assetSwapLeg;
        */
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool returnLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityLeg",Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Equity Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Leg", IsClonable = true, IsChild = true)]
        public EquityLeg[] returnLeg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestLeg", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Interest Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Interest Leg", IsClonable = true, IsChild = true)]
        public InterestLeg[] interestLeg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool varianceLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("varianceLeg", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Variance Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Variance Leg", IsClonable = true, IsChild = true)]
        public VarianceLeg[] varianceLeg;
		#endregion Members
    }
    #endregion EquitySwapBase
    #region EquitySwapEarlyTerminationType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquitySwapEarlyTerminationType
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Party Reference")]
        public PartyReference partyReference;
		[System.Xml.Serialization.XmlElementAttribute("startingDate", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting Date", IsVisible = false)]
        public StartingDate startingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting Date")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion EquitySwapEarlyTerminationType
    #region EquitySwapNotional
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquitySwapNotional : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice equitySwapNotional;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equitySwapNotionalDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString equitySwapNotionalDeterminationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equitySwapNotionalNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", typeof(Money),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Money equitySwapNotionalNotionalAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equitySwapNotionalAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(Reference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative To", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 300)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public Reference equitySwapNotionalAmountRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion EquitySwapNotional
    #region EquitySwapTransactionSupplement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equitySwapTransactionSupplement", Namespace = "http://www.fpml.org/2005/FpML-4-2",IsNullable = false)]
    public partial class EquitySwapTransactionSupplement : EquitySwapBase
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mutualEarlyTerminationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mutualEarlyTermination", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "mutualEarlyTermination")]
        public EFS_Boolean mutualEarlyTermination;
		#endregion Members
	}
    #endregion EquitySwapTransactionSupplement
    #region EquitySwapValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquitySwapValuation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("initialPrice", typeof(EquitySwapValuationPrice),Order=1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Price", IsVisible = false)]
        public EquitySwapValuationPrice initialPrice;
		[System.Xml.Serialization.XmlElementAttribute("equityNotionalReset", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Equity Notional Reset")]
        public EFS_Boolean equityNotionalReset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationPriceInterimSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationPriceInterim", typeof(EquitySwapValuationPrice),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interim Price")]
        public EquitySwapValuationPrice valuationPriceInterim;
        [System.Xml.Serialization.XmlElementAttribute("valuationPriceFinal", typeof(EquitySwapValuationPrice),Order=4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final Price", IsVisible = false)]
        public EquitySwapValuationPrice valuationPriceFinal;
		[System.Xml.Serialization.XmlElementAttribute("equityPaymentDates", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final Price")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates", IsVisible = false)]
        public EquityPaymentDates equityPaymentDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion EquitySwapValuation
    #region EquitySwapValuationPrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquitySwapValuationPrice : Price
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityValuationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityValuation", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity valuation")]
        public EquityValuation equityValuation;
		#endregion Members
	}
    #endregion EquitySwapValuationPrice
    #region EquityVarianceAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityVarianceAmount : EquityAmount
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementPaymentDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CashSettlement Payment Date")]
        public AdjustableOrRelativeDate cashSettlementPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Start Date")]
        public EFS_Date observationStartDate;
		#endregion Members
		#region Constructors
		public EquityVarianceAmount()
        {
            observationStartDate = new EFS_Date();
		}
		#endregion Constructors
	}
    #endregion EquityVarianceAmount

    #region Formula
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Formula : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaDescriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("formulaDescription", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description")]
        public EFS_MultiLineString formulaDescription;
		[System.Xml.Serialization.XmlElementAttribute("math", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Math", IsVisible = false)]
        public Math math;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaComponentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formulaComponent",Order=3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Math")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula Components")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula Components", IsClonable = true, IsChild = true, MinItem = 0)]
        public FormulaComponent[] formulaComponent;
		#endregion Members
	}
    #endregion Formula
    #region FormulaComponent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FormulaComponent : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("componentDescription", Order = 1)]
        [ControlGUI(Name = "Description", LblWidth = 100)]
        public EFS_MultiLineString componentDescription;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("formula", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula formula;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_nameSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Name", Width = 500)]
        public EFS_Attribute efs_name;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_hrefSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Formula)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            set
            {
                efs_name = new EFS_Attribute(value);
                efs_nameSpecified = StrFunc.IsFilled(value);
            }
            get
            {
                if (efs_name == null)
                    return null;
                else
                    return efs_name.Value;
            }
        }


        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set
            {
                efs_href = new EFS_Href(value);
                efs_hrefSpecified = StrFunc.IsFilled(value);
            }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
		}
		#endregion Members

		#region IEFS_Array Interface Methods
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,
            ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Interface Methods
    }
    #endregion FormulaComponent

    #region InterestCalculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class InterestCalculation : InterestAccrualsMethod
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Day Count Fraction")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
	}
    #endregion InterestCalculation

    #region InterestLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("interestLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class InterestLeg : AssetSwapLeg
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("interestLegCalculationPeriodDates", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates", IsVisible = false)]
        public InterestLegCalculationPeriodDates interestLegCalculationPeriodDates;
		[System.Xml.Serialization.XmlElementAttribute("notional", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional", IsVisible = false)]
        public EquitySwapNotional notional;
		[System.Xml.Serialization.XmlElementAttribute("interestAmount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Amount", IsVisible = false)]
        public LegAmount interestAmount;
		[System.Xml.Serialization.XmlElementAttribute("interestCalculation", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Calculation", IsVisible = false)]
        public InterestCalculation interestCalculation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubCalculationPeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubCalculationPeriod", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Calculation")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Calculation", IsVisible = false)]
        public StubCalculationPeriod stubCalculationPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Calculation")]
        public bool FillBalise;
		#endregion Members
    }
    #endregion InterestLeg
    #region InterestLegCalculationPeriodDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class InterestLegCalculationPeriodDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;
		[System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;
		[System.Xml.Serialization.XmlElementAttribute("interestLegResetDates", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates", IsVisible = false)]
        public InterestLegResetDates interestLegResetDates;
		[System.Xml.Serialization.XmlElementAttribute("interestLegPaymentDates", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates", IsVisible = false)]
        public AdjustableOrRelativeDates interestLegPaymentDates;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion InterestLegCalculationPeriodDates
    #region InterestLegResetDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class InterestLegResetDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "CalculationPeriodDates Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference calculationPeriodDatesReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice resetDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesResetRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetRelativeTo", typeof(ResetRelativeToEnum),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reset Relative To", IsVisible = true)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public ResetRelativeToEnum resetDatesResetRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesResetFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetFrequency", typeof(ResetFrequency),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reset Frequency", IsVisible = true)]
        public ResetFrequency resetDatesResetFrequency;
		#endregion Members
	}
    #endregion InterestLegResetDates

    #region LegAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAmount))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityVarianceAmount))]
    public partial class LegAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentCurrency", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Currency")]
        public PaymentCurrency paymentCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice legAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountReferenceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceAmount", typeof(ReferenceAmount),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference", IsVisible = true)]
        public ReferenceAmount legAmountReferenceAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formula", typeof(Formula),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Formula", IsVisible = true)]
        public Formula legAmountFormula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountEncodedDescriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("encodedDescription", typeof(System.Byte[]), DataType = "base64Binary",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Encoded Description", IsVisible = true)]
        public System.Byte[] legAmountEncodedDescription;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountVarianceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("variance", typeof(Variance),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Variance", IsVisible = true)]
        public Variance legAmountVariance;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationDates", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Dates")]
        public AdjustableRelativeOrPeriodicDates calculationDates;
		#endregion Members
	}
    #endregion LegAmount

    #region PrincipalExchangeAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PrincipalExchangeAmount : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Principal Exchange Amount")]
        public EFS_RadioChoice exchange;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(Reference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        public Reference exchangeAmountRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString exchangeDeterminationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangePrincipalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalAmount", typeof(Money),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Principal Amount", IsVisible = true)]
        public Money exchangePrincipalAmount;
		#endregion Members
	}
    #endregion PrincipalExchangeAmount
    #region PrincipalExchangeDescriptions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PrincipalExchangeDescriptions
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeAmount", Order = 3)]
        public PrincipalExchangeAmount principalExchangeAmount;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeDate", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Exchange Date", IsVisible = false)]
        public AdjustableOrRelativeDate principalExchangeDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Exchange Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
    }
    #endregion PrincipalExchangeDescriptions
    #region PrincipalExchangeFeatures
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PrincipalExchangeFeatures : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("principalExchanges", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchange", IsVisible = false)]
        public PrincipalExchanges principalExchanges;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeDescriptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeDescriptions",Order=2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchange")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Descriptions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Description", IsChild = true)]
        public PrincipalExchangeDescriptions[] principalExchangeDescriptions;
		#endregion Members
	}
    #endregion PrincipalExchangeFeatures

    #region Return
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Return
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("returnType", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Type")]
        public ReturnTypeEnum returnType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendConditionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendConditions", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend Conditions")]
        public DividendConditions dividendConditions;
		#endregion Members
	}
    #endregion Return

    #region StartingDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class StartingDate
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice startingDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startingDateDateRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", typeof(DateRelativeTo),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Date Relative To", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateRelativeTo startingDateDateRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startingDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AdjustableDate startingDateAdjustableDate;
		#endregion Members
	}
    #endregion StartingDate
    #region StubCalculationPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class StubCalculationPeriod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialStubSpecified;
		[System.Xml.Serialization.XmlElementAttribute("initialStub", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Stub")]
        public Stub initialStub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool finalStubSpecified;
		[System.Xml.Serialization.XmlElementAttribute("finalStub", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final Stub")]
        public Stub finalStub;
		#endregion Members
	}
    #endregion StubCalculationPeriod

    #region Variance
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Variance : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Level")]
        public EFS_RadioChoice level;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool levelInitialLevelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialLevel", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Initial", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public EFS_Decimal levelInitialLevel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool levelClosingLevelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("closingLevel", typeof(EFS_Boolean),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Closing", IsVisible = true)]
        [ControlGUI(Name = "yes")]
        public EFS_Boolean levelClosingLevel;

		[System.Xml.Serialization.XmlElementAttribute("varianceAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public Money varianceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Strike Price")]
        public EFS_RadioChoice strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikePriceVarianceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("varianceStrikePrice", typeof(EFS_Decimal),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Variance Strike Price", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public EFS_Decimal strikePriceVariance;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikePriceVolatilitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("volatilityStrikePrice", typeof(EFS_Decimal),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Volatility Strike Price", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public EFS_Decimal strikePriceVolatility;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expectedNSpecified;
		[System.Xml.Serialization.XmlElementAttribute("expectedN", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "expectedN", Width = 60)]
        public EFS_Integer expectedN;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool varianceCapSpecified;
		[System.Xml.Serialization.XmlElementAttribute("varianceCap", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "varianceCap")]
        public EFS_Boolean varianceCap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedVarianceCapSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedVarianceCap", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "unadjustedVarianceCap")]
        public EFS_Decimal unadjustedVarianceCap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeTradedContractNearestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exchangeTradedContractNearest", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "ExchangeTradedContract")]
        public ExchangeTradedContract exchangeTradedContractNearest;
		#endregion Members
		#region Constructors
		public Variance()
        {
            levelInitialLevel = new EFS_Decimal();
            levelClosingLevel = new EFS_Boolean();
            strikePriceVariance = new EFS_Decimal();
            strikePriceVolatility = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion Variance
    #region VarianceLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("varianceLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class VarianceLeg : AssetSwapLeg, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("underlyer", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying component", IsVisible = false)]
        public Underlyer underlyer;
		[System.Xml.Serialization.XmlElementAttribute("equityValuation", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying component")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation", IsVisible = false)]
        public EquityValuation equityValuation;
		[System.Xml.Serialization.XmlElementAttribute("equityAmount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Variance Amount", IsVisible = false)]
        public EquityVarianceAmount equityAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Variance Amount")]
        public bool FillBalise;
		#endregion Members
    }
    #endregion VarianceLeg

    #region Math
    // EG 20160404 Migration vs2013
    // #warning Pb ValidationRequest
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Math : NodeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any;
		#endregion Members
    }
    #endregion Math
}
