#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;


using FpML.v44.Shared;
using FpML.v44.Eq.Shared;
using FpML.v44.Option.Shared;
#endregion using directives

// EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
namespace FpML.v44.DividendSwaps
{
    #region DividendLeg
    /// <summary>
    /// Floating payment leg of a dividend swap
    /// </summary>
    // EG 20140702 New build FpML4.4 Add specialDividends
    // EG 20140702 New build FpML4.4 Add materialDividend
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DividendLeg : DirectionalLegUnderlyer
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool declaredCashDividendPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("declaredCashDividendPercentage", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Cash dividend percentage")]
        public EFS_Decimal declaredCashDividendPercentage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool declaredCashEquivalentDividendPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("declaredCashEquivalentDividendPercentage", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Cash equivalent dividend percentage")]
        public EFS_Decimal declaredCashEquivalentDividendPercentage;
        [System.Xml.Serialization.XmlElementAttribute("dividendPeriod", Order = 3)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend period payment")]
        public DividendPeriodPayment[] dividendPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specialDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("specialDividends", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Special dividends")]
        public EFS_Boolean specialDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nonCashDividendSpecified;
        [System.Xml.Serialization.XmlElementAttribute("materialDividend", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Material dividend")]
        public EFS_Boolean materialDividend;
        #endregion Members
    }
    #endregion DividendLeg
    #region DividendPeriodPayment
    /// <summary>
    /// A time bounded dividend period, with fixed strike and a dividend payment date per period.
    /// </summary>
    // EG 20140702 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DividendPeriodPayment : DividendPeriod
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fixedStrike", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Fixed strike")]
        public EFS_Decimal fixedStrike;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date", IsVisible = false)]
        public AdjustableOrRelativeDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Valuation date")]
        public AdjustableOrRelativeDate valuationDate;
        #endregion Members
    }
    #endregion DividendPeriodPayment
    #region DividendSwapTransactionSupplement
    // EG 20140702 New build FpML4.4 Add EquityUnderlyerProvision.model
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("dividendSwapTransactionSupplement", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Dividend Swap Transaction Supplement")]
    public partial class DividendSwapTransactionSupplement : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("dividendLeg", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend leg", IsVisible = false)]
        public DividendLeg dividendLeg;
		[System.Xml.Serialization.XmlElementAttribute("fixedLeg", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend leg")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed payment leg", IsVisible = false)]
        public FixedPaymentLeg fixedLeg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExchangeIndexAnnexFallbackSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleExchangeIndexAnnexFallback", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed payment leg")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "multipleExchangeIndexAnnexFallback")]
        public EFS_Boolean multipleExchangeIndexAnnexFallback;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localJurisdictionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localJurisdiction", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Local jurisdiction")]
        public Country localJurisdiction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantJurisdictionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relevantJurisdiction", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Relevant jurisdiction")]
        public Country relevantJurisdiction;
        #endregion Members
    }
    #endregion DividendSwapTransactionSupplement
    #region FixedPaymentAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FixedPaymentAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Payment amount")]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 2)]
        public RelativeDateOffset paymentDate;
        #endregion Members
    }
    #endregion FixedPaymentAmount
    #region FixedPaymentLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FixedPaymentLeg : DirectionalLeg
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPayment",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed payments")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed payment")]
        public FixedPaymentAmount[] fixedPayment;
        #endregion Members
    }
    #endregion FixedPaymentLeg
}