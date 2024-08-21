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
    #region EFS_BondInitialValuationChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_BondInitialValuationChoiceType
    {
        nboption,
        notional,
        underlyer,
    }
    #endregion EFS_BondInitialValuationChoiceType

    #region EFS_OptionBaseChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_OptionBaseChoiceType
    {
        asianFeatures,
        automaticExercise,
        barrierFeatures,
        exerciseDates,
        initialValuation,
        knockFeatures,
    }
    #endregion EFS_OptionBaseChoiceType

    #region Product
    #region EFS_EventMatrixBondOption
    public class EFS_EventMatrixBondOption : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bondOptionPremium", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("bondOptionType", typeof(EFS_EventMatrixBondOptionType))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixBondOption() { productName = "BondOption"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixBondOption
    #endregion Product

    #region EFS_EventMatrixBondOptionType
    public class EFS_EventMatrixBondOptionType : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialValuation", typeof(EFS_EventMatrixBondInitialValuation))]
        [System.Xml.Serialization.XmlElementAttribute("asianFeatures", typeof(EFS_EventMatrixValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("barrierFeatures", typeof(EFS_EventMatrixValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("knockFeatures", typeof(EFS_EventMatrixValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_OptionBaseChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixBondOptionType() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityOptionType

    #region EFS_EventMatrixBondInitialValuation
    public class EFS_EventMatrixBondInitialValuation : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nboption", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("underlyer", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("notional", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_BondInitialValuationChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixBondInitialValuation() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixBondInitialValuation
    #region EFS_EventMatrixValuationDates
    public class EFS_EventMatrixValuationDates : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("underlyerValuationDates", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixValuationDates() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixValuationDates
}
