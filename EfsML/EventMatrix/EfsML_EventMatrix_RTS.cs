#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives

namespace EfsML.EventMatrix
{
    #region Choice Matrix
    #region EFS_EventMatrixReturnLegValuationChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    // EG 20231127 [WI755] Implementation Return Swap : returnSwapAmount instead of equityAmount
    public enum EFS_EventMatrixReturnLegValuationChoiceType
    {
        returnSwapAmount,
        marginrequirement,
    }
    #endregion EFS_EventMatrixReturnLegValuationChoiceType
    #endregion Choice Matrix


    #region Product
    #region EFS_EventMatrixReturnSwap
    public class EFS_EventMatrixReturnSwap : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("returnLeg", typeof(EFS_EventMatrixReturnLeg))]
        [System.Xml.Serialization.XmlElementAttribute("interestLeg", typeof(EFS_EventMatrixInterestLeg))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixReturnSwap() { productName = "ReturnSwap"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixReturnSwap
    #endregion Product

    #region EFS_EventMatrixInterestLeg
    public class EFS_EventMatrixInterestLeg : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixInterestLegPaymentDates))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixInterestLeg() { }
    }
    #endregion EFS_EventMatrixInterestLeg

    #region EFS_EventMatrixInterestLegPaymentDates
    public class EFS_EventMatrixInterestLegPaymentDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("returnSwapAmountStep", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("resetDates", typeof(EFS_EventMatrixResetDates))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixInterestLegPaymentDates() { }
    }
    #endregion EFS_EventMatrixInterestLegPaymentDates

    #region EFS_EventMatrixReturnLeg
    // EG 20231127 [WI755] Implementation Return Swap : Add paymentDates
    public class EFS_EventMatrixReturnLeg : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("underlyer", typeof(EFS_EventMatrixReturnLegUnderlyer))]
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixReturnLegPaymentDates))]
        [System.Xml.Serialization.XmlElementAttribute("valuation", typeof(EFS_EventMatrixReturnLegValuation))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixReturnLeg() { }
    }
    #endregion EFS_EventMatrixReturnLeg
    #region EFS_EventMatrixReturnLegValuation
    public class EFS_EventMatrixReturnLegValuation : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("basketComponent", typeof(EFS_EventMatrixBasketComponent))]
        public EFS_EventMatrixGroup[] group;
        public EFS_EventMatrixReturnLegValuation() { }
    }
    #endregion EFS_EventMatrixReturnLegValuation
    #region EFS_EventMatrixReturnLegUnderlyer
    public class EFS_EventMatrixReturnLegUnderlyer : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("basketComponent", typeof(EFS_EventMatrixBasketComponent))]
        public EFS_EventMatrixGroup[] group;
        public EFS_EventMatrixReturnLegUnderlyer() { }
    }
    #endregion EFS_EventMatrixReturnLegUnderlyer

    // EG 20231127 [WI755] Implementation Return Swap : New
    #region EFS_EventMatrixReturnLegPaymentDates
    public class EFS_EventMatrixReturnLegPaymentDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixReturnLegPaymentDates() { }
    }
    #endregion EFS_EventMatrixReturnLegPaymentDates

    #region EFS_EventMatrixBasketComponent
    public class EFS_EventMatrixBasketComponent : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;
        public EFS_EventMatrixBasketComponent() { }
    }
    #endregion EFS_EventMatrixBasketComponent

}
