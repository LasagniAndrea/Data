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
    #region EFS_EventMatrixExchangeTradedDerivativeStreamChoiceType
    // EG 20091221 Choice items on EFS_EventMatrixExchangeTradedDerivativeCategory
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_EventMatrixExchangeTradedDerivativeStreamChoiceType
    {
        premium,
        nominalQtyStep,
        nominalQtyVariation,
        linkedProductClosingAmounts,
        exerciseDates,
    }
    #endregion EFS_EventMatrixExchangeTradedDerivativeStreamChoiceType
    #region EFS_EventMatrixPremiumChoiceType
    // EG 20210812 [25173] Calcul de la prime sur Trade avec TrdTyp = Merge ou VolumeWeightedAverageTrade
    // Nouveaux éléments pour gestion des nouveaux événements PRT (Theoretical premium amount) et PCR (Cash residual amount)
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_EventMatrixPremiumChoiceType
    {
        premiumTheoretical,
        premiumCashResidual,
    }
    #endregion EFS_EventMatrixPremiumChoiceType
    #region EFS_EventMatrixExchangeTradedDerivative
    // EG 20091221 EFS_EventMatrixExchangeTradedDerivative matrix
    public class EFS_EventMatrixExchangeTradedDerivative : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivativeStream", typeof(EFS_EventMatrixExchangeTradedDerivativeStream))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixExchangeTradedDerivative() { productName = "ExchangeTradedDerivative"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixExchangeTradedDerivative
    #region EFS_EventMatrixExchangeTradedDerivativeStream
    // EG 20091221 EFS_EventMatrixExchangeTradedDerivativeStream matrix
    // EG 20210812 [25173] Calcul de la prime sur Trade avec TrdTyp = Merge ou VolumeWeightedAverageTrade
    public class EFS_EventMatrixExchangeTradedDerivativeStream : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPremium))]
        [System.Xml.Serialization.XmlElementAttribute("nominalQtyStep", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("nominalQtyVariation", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventMatrixExchangeTradedDerivativeStreamChoiceType[] itemsElementName;

        public EFS_EventMatrixExchangeTradedDerivativeStream() { }
    }
    #endregion EFS_EventMatrixExchangeTradedDerivativeStream

    // EG 20210812 [25173] Calcul de la prime sur Trade avec TrdTyp = Merge ou VolumeWeightedAverageTrade
    // Nouveaux éléments pour gestion des nouveaux événements PRT (Theoretical premium amount) et PCR (Cash residual amount)
    public class EFS_EventMatrixPremium : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("premiumTheoretical", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("premiumCashResidual", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventMatrixPremiumChoiceType[] itemsElementName;

        public EFS_EventMatrixPremium() { }
    }
}