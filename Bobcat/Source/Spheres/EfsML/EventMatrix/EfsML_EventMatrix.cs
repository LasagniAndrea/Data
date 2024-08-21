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
    #region General
    #region EFS_EventMatrix
    /// <summary>
    /// <newpara><b>Description :</b> The EFS_EventMatrix element forms the root for any conforming Events referential 
    /// instance document</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>parameters (zero or one occurrence; of the type EFS_EventMatrixParameter)</newpara>
    /// <newpara>trade (exactly one occurrence; of the type EFS_EventMatrixTrade)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlRootAttribute("EventMatrix", IsNullable = false)]
    public class EFS_EventMatrix
    {
        [System.Xml.Serialization.XmlElementAttribute("parameter")]
        public EFS_EventMatrixParameter[] parameters;

        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public EFS_EventMatrixTrade trade;
    }
    #endregion EFS_EventMatrix
    #region EFS_EventMatrixConditional
    public class EFS_EventMatrixConditional : EFS_EventMatrixField
    {
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
    }
    #endregion EFS_EventMatrixConditional
    #region EFS_EventMatrixField
    /// <summary>
    /// <newpara><b>Description :</b></newpara>
    /// <newpara><b>Contents :</b>The base element type which all items of EFS_EventsMatrixItem extend</newpara>
    /// <newpara>declaringType (zero or one occurrence; of the type xsd:string) name of EFS or FpML class who declare Value.</newpara>
    /// <newpara>Value (exactly one occurrence; of the type xsd:string) the name of method or property or constant to apply.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixKey</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixSubItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    public class EFS_EventMatrixField
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool declaringTypeSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string declaringType;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        /// <summary>
        /// 
        /// </summary>
        public object result;
        
        #region constructor
        public EFS_EventMatrixField() { }
        #endregion
    }
    #endregion EFS_EventMatrixField
    #region EFS_EventMatrixFieldReference
    /// <summary>
    /// <newpara><b>Description :</b></newpara>
    /// <newpara><b>Contents :</b>The base element type which all items of EFS_EventsMatrixItem extend</newpara>
    /// <newpara>hRef (exactly one occurrence; of the type xsd:string) the id of parameter defined before in the document</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixKey</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixSubItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    public class EFS_EventMatrixFieldReference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string hRef;

        public EFS_EventMatrixFieldReference() { }
    }
    #endregion EFS_EventMatrixFieldReference
    #region EFS_EventMatrixGroup
    /// <summary>
    /// <newpara><b>Description :</b> The events definitions for a specific group.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>parameters (zero or more occurrence; of the type EFS_EventMatrixParameter)</newpara>
    /// <newpara>itemGroup (exactly one occurrence; of the type EFS_EventMatrixItem) 
    /// Define the informative event elements for this group</newpara>
    /// <newpara>item (zero or more occurrence; of the type EFS_EventMatrixItem)</newpara>
    /// <newpara>keyGroup (exactly one occurrence; of the type EFS_EventMatrixGroupKey) Define the key for this group</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixTrade</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixCapFloorStream</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixSwapStream</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixProduct</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixTrade</newpara>
    ///</remarks>
    public class EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("parameter")]
        public EFS_EventMatrixParameter[] parameters;
        [System.Xml.Serialization.XmlElementAttribute("keyGroup")]
        public EFS_EventMatrixGroupKey keyGroup;
        [System.Xml.Serialization.XmlElementAttribute("itemGroup")]
        public EFS_EventMatrixItem itemGroup;
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public EFS_EventMatrixItem[] item;

        #region constructor
        public EFS_EventMatrixGroup() { }
        #endregion constructor
    }
    #endregion EFS_EventMatrixGroup
    #region EFS_EventMatrixGroupKey
    /// <summary>
    /// <newpara><b>Description :</b> the key for a events group.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>key (zero or one occurrence; of the type xsd:string) prefix key of the group</newpara>
    /// <newpara>name (zero or one occurrence; of the type xsd:string) name of the group</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixGroup</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    public class EFS_EventMatrixGroupKey
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string key;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string name;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string result;
    }
    #endregion EFS_EventMatrixGroupKey
    #region EFS_EventMatrixItem
    /// <summary>
    /// <newpara><b>Description :</b> The event matrix element</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>parameters (zero or one occurrence; of the type EFS_EventMatrixParameter)</newpara>
    /// <newpara>occurence (zero or one occurrence; of the type EFS_EventMatrixOccurence) EFS or FpML complex type element, 
    /// for where events definition will occur</newpara>
    /// <newpara>key (exactly one occurrence; of the type EFS_EventMatrixKey) the key event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>startDate (exactly one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the Start Period event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>startDateReference (exactly one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the Start Period event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>endDate (exactly one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the End Period event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>endDateReference (exactly one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the End Period event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>payer (zero or one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the payer event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>payerReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the payer event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>receiver (zero or one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the receiver event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>receiverReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the receiver event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>valorisation (zero or one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the valorisation event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>valorisationReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the valorisation event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>unit (zero or one occurrence; of the type EFS_EventMatrixField)
    /// the element (method or property or constant) to retrieve the unit event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>unitReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
    /// the element reference (parameter) to retrieve the unit event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>subItem (one or more occurrence; of the type EFS_EventMatrixSubItem) the sub-event element definition.
    /// </newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    public class EFS_EventMatrixItem
    {
        #region Members
        #region Event
        [System.Xml.Serialization.XmlElementAttribute("startDate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("startDateReference", typeof(EFS_EventMatrixFieldReference))]
        public object startDate;
        [System.Xml.Serialization.XmlElementAttribute("endDate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("endDateReference", typeof(EFS_EventMatrixFieldReference))]
        public object endDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool payerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payer", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("payerReference", typeof(EFS_EventMatrixFieldReference))]
        public object payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool receiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiver", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("receiverReference", typeof(EFS_EventMatrixFieldReference))]
        public object receiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valorisationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valorisation", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("valorisationReference", typeof(EFS_EventMatrixFieldReference))]
        public object valorisation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitType", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("unitTypeReference", typeof(EFS_EventMatrixFieldReference))]
        public object unitType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unit", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("unitReference", typeof(EFS_EventMatrixFieldReference))]
        public object unit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStCalculSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idStCalcul", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("idStCalculReference", typeof(EFS_EventMatrixFieldReference))]
        public object idStCalcul;
        #endregion Event
        #region EventAsset & EventDet
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool itemDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("item_Details")]
        public EFS_EventMatrixItemDetails itemDetails;
        #endregion EventAsset & EventDet
        #region EventClass
        [System.Xml.Serialization.XmlElementAttribute("subItem")]
        public EFS_EventMatrixSubItem[] subItem;
        [System.Xml.Serialization.XmlElementAttribute("subItems")]
        public EFS_EventMatrixSubItems[] subItems;
        #endregion EventClass
        #region Others
        [System.Xml.Serialization.XmlElementAttribute("parameter")]
        public EFS_EventMatrixParameter[] parameters;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool occurenceSpecified;
        public EFS_EventMatrixOccurence occurence;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("conditional", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("conditionalReference", typeof(EFS_EventMatrixFieldReference))]
        public object conditional;
        [System.Xml.Serialization.XmlElementAttribute("key")]
        public EFS_EventMatrixKey key;
        #endregion Others
        #endregion Members
        #region Indexors
        public EFS_EventMatrixParameter this[string pId]
        {
            get
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (pId == parameters[i].Id)
                        return parameters[i];
                }
                return null;
            }
        }
        #endregion Indexors
        #region Constructors
        public EFS_EventMatrixItem() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixItem
    #region EFS_EventMatrixItemDetails
    // 20081030 EG Newness PaymentSource
    // 20091222 EG Newness etdPremium
    public class EFS_EventMatrixItemDetails
    {
        #region Members
        #region Assets
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("asset", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("assetReference", typeof(EFS_EventMatrixFieldReference))]
        public object asset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool asset2Specified;
        [System.Xml.Serialization.XmlElementAttribute("asset2", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("asset2Reference", typeof(EFS_EventMatrixFieldReference))]
        public object asset2;
        #endregion Assets
        #region COMD Details
        public bool fixedPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPrice", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("fixedPriceReference", typeof(EFS_EventMatrixFieldReference))]
        public object fixedPrice;
        public bool deliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryDate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("deliveryDateReference", typeof(EFS_EventMatrixFieldReference))]
        public object deliveryDate;
        #endregion COMD Details
        #region FX Details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentQuote", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("paymentQuoteReference", typeof(EFS_EventMatrixFieldReference))]
        public object paymentQuote;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("exchangeRateReference", typeof(EFS_EventMatrixFieldReference))]
        public object exchangeRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sideRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("sideRateReference", typeof(EFS_EventMatrixFieldReference))]
        public object sideRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixingRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("fixingReference", typeof(EFS_EventMatrixFieldReference))]
        public object fixingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("settlementRateReference", typeof(EFS_EventMatrixFieldReference))]
        public object settlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool currencyPairSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyPair", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("currencyPairReference", typeof(EFS_EventMatrixFieldReference))]
        public object currencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool triggerRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("triggerRateReference", typeof(EFS_EventMatrixFieldReference))]
        public object triggerRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("strikePriceReference", typeof(EFS_EventMatrixFieldReference))]
        public object strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool premiumQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premiumQuote", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("premiumQuoteReference", typeof(EFS_EventMatrixFieldReference))]
        public object premiumQuote;
        public bool triggerEventSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerEvent", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("triggerEventReference", typeof(EFS_EventMatrixFieldReference))]
        public object triggerEvent;

        #endregion FX Details
        #region IRD Details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("fixedRateReference", typeof(EFS_EventMatrixFieldReference))]
        public object fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("dayCountFractionReference", typeof(EFS_EventMatrixFieldReference))]
        public object dayCountFraction;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool strikeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strike", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("strikeReference", typeof(EFS_EventMatrixFieldReference))]
		public object strike;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool spreadSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spread", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("spreadReference", typeof(EFS_EventMatrixFieldReference))]
		public object spread;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool multiplierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multiplier", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("multiplierReference", typeof(EFS_EventMatrixFieldReference))]
		public object multiplier;
		#endregion IRD Details
        #region ADM Details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool invoicingAmountBaseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("invoicingAmountBase", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("invoicingAmountBaseReference", typeof(EFS_EventMatrixFieldReference))]
        public object invoicingAmountBase;
        #endregion ADM Details
        #region ETD Details
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool etdPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("etdPremium", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("etdPremiumReference", typeof(EFS_EventMatrixFieldReference))]
        public object etdPremium;
        #endregion ETD Details
        #region PaymentSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentSource", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("paymentSourceReference", typeof(EFS_EventMatrixFieldReference))]
        public object paymentSource;
        #endregion PaymentSource
        #region TaxSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("taxSource", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("taxSourceReference", typeof(EFS_EventMatrixFieldReference))]
        public object taxSource;
        #endregion TaxSource
        #endregion Members
        #region Constructors
        public EFS_EventMatrixItemDetails() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixItemDetails
    #region EFS_EventMatrixKey
    /// <summary>
    /// <newpara><b>Description :</b> The key Event.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>category (exactly one occurrence; of the type EFS_EventField) 
    /// the element (method or property or constant) to retrieve the category event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>categoryReference (exactly one occurrence; of the type EFS_EventFieldReference)
    /// the element reference (parameter) to retrieve the category event.</newpara>
    /// <newpara>Either</newpara> 
    /// <newpara>relativeTo (exactly one occurrence; of the type EFS_EventField)
    /// the element (method or property or constant) to retrieve the relativeTo event.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>relativeToReference (exactly one occurrence; of the type EFS_EventFieldReference)
    /// the element reference (parameter) to retrieve the relativeTo event.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis 
    public class EFS_EventMatrixKey : ICloneable
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("eventCode", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("eventCodeReference", typeof(EFS_EventMatrixFieldReference))]
        public object eventCode;
        [System.Xml.Serialization.XmlElementAttribute("eventType", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("eventTypeReference", typeof(EFS_EventMatrixFieldReference))]
        public object eventType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idE_SourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idE_Source", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("idE_SourceReference", typeof(EFS_EventMatrixFieldReference))]
        public object idE_Source;
        #endregion Members
		#region Constructors
		public EFS_EventMatrixKey() { }
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
        {
            EFS_EventMatrixKey clone = new EFS_EventMatrixKey
            {
                eventCode = this.eventCode,
                eventType = this.eventType,
                idE_Source = this.idE_Source
            };
            return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
	}
    #endregion EFS_EventMatrixKey
    #region EFS_EventMatrixOccurence
    /// <summary>
    /// <newpara><b>Description :</b> The occurence element (FpML or EFS element reference) 
    /// for a group or item event matrix declaration</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>to (exactly one occurrence; of the type EventOccurenceEnum) The type of occurence Loop.
    /// All (All elements), AllExceptFirst (All elements except first), AllExceptLast (All elements except last),
    /// AllExceptFirstAndLast (All elements except first and last), First (First element only), Last (Last element only),
    /// Unique (for no array),
    /// </newpara>
    /// <newpara>field (zero or one occurrence; of the type xsd:string) a child's element of element declared in Value.</newpara>
    /// <newpara>field (zero or one occurrence; of the type xsd:boolean) true if element declared is optinal.</newpara>
    /// <newpara>Value (exactly one occurrence; of the type xsd:string) a element of EFS or FpML trade.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    // EG 20231127 [WI755] Implementation Return Swap : Add to2 (currently unused)
    public class EFS_EventMatrixOccurence
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string to;
        // Occurence utilisée en cas de présence de fields(Array) et Value(non array => to2)
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string to2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool positionSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string position;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string field;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "boolean")]
        public bool isOptional;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "boolean")]
        public bool isFieldOptional;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public EFS_EventMatrixOccurence() { }
		#endregion Constructors
	}
    #endregion EFS_EventMatrixOccurence
    #region EFS_EventMatrixParameter
    /// <summary>
    /// <newpara><b>Description :</b> The list of event's parameters</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type 
    /// EFS_EventMatrixField)</newpara>
    /// <newpara>id (exactly one occurrence; of the type xsd:string) key of the parameter</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EFS_EventMatrixGroup</newpara>
    ///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
    ///<newpara>• Complex type: EFS_EventMatrix</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    public class EFS_EventMatrixParameter : EFS_EventMatrixField
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string hRef;
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
    }
    #endregion EFS_EventMatrixParameter
    #region EFS_EventMatrixSubItems
    public class EFS_EventMatrixSubItems
    {
        [System.Xml.Serialization.XmlElementAttribute("subItem")]
        public EFS_EventMatrixSubItem[] subItem;

        #region Constructors
        public EFS_EventMatrixSubItems() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixSubItems
    #region EFS_EventMatrixSubItem
    public class EFS_EventMatrixSubItem
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("conditional", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("conditionalReference", typeof(EFS_EventMatrixFieldReference))]
        public object conditional;

        [System.Xml.Serialization.XmlElementAttribute("eventClass", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("eventClassReference", typeof(EFS_EventMatrixFieldReference))]
        public object eventClass;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventDate", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("eventDateReference", typeof(EFS_EventMatrixFieldReference))]
        public object eventDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isPayment", typeof(EFS_EventMatrixField))]
        [System.Xml.Serialization.XmlElementAttribute("isPaymentReference", typeof(EFS_EventMatrixFieldReference))]
        public object isPayment;

        #region Constructors
        public EFS_EventMatrixSubItem() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixSubItem
    #endregion General

    #region Trade / Strategy / Product
    #region EFS_EventMatrixProduct
    // EG 20091221 Add IncludeAttribute EFS_EventMatrixExchangeTradedDerivative
    // PM 20120824 [18058] Add XmlIncludeAttribute EFS_EventMatrixCashBalanceInterest
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixAdditionalInvoice))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixBondOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixBulletPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixBuyAndSellBack))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCashBalance))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCashBalanceInterest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCapFloor))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCommoditySpot))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCreditNote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixDebtSecurity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixDebtSecurityTransaction))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixEquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixEquitySecurityTransaction))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixExchangeTradedDerivative))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFra))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxAverageRateOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxBarrierOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxDigitalOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxOptionLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixMarginRequirement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixInvoice))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixInvoiceSettlement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixReturnSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixRepo))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixStrategy))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixSwaption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixTermDeposit))]
    public class EFS_EventMatrixProduct : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string productName;
    }

    #endregion EFS_EventMatrixProduct
    #region EFS_EventMatrixStrategy
    // EG 20091221 Add ElementAttribute EFS_EventMatrixExchangeTradedDerivative
    public class EFS_EventMatrixStrategy : EFS_EventMatrixProduct
    {
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(EFS_EventMatrixBulletPayment))]
        [System.Xml.Serialization.XmlElementAttribute("buyAndSellBack", typeof(EFS_EventMatrixBuyAndSellBack))]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(EFS_EventMatrixCapFloor))]
        [System.Xml.Serialization.XmlElementAttribute("commoditySpot", typeof(EFS_EventMatrixCommoditySpot))]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", typeof(EFS_EventMatrixDebtSecurity))]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", typeof(EFS_EventMatrixDebtSecurityTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EFS_EventMatrixEquityOption))]
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransaction", typeof(EFS_EventMatrixEquitySecurityTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", typeof(EFS_EventMatrixExchangeTradedDerivative))]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(EFS_EventMatrixFra))]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(EFS_EventMatrixFxAverageRateOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(EFS_EventMatrixFxBarrierOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(EFS_EventMatrixFxDigitalOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(EFS_EventMatrixFxOptionLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(EFS_EventMatrixFxLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(EFS_EventMatrixFxSwap))]
        [System.Xml.Serialization.XmlElementAttribute("loanDeposit", typeof(EFS_EventMatrixLoanDeposit))]
        [System.Xml.Serialization.XmlElementAttribute("repo", typeof(EFS_EventMatrixRepo))]
        [System.Xml.Serialization.XmlElementAttribute("returnSwap", typeof(EFS_EventMatrixReturnSwap))]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(EFS_EventMatrixStrategy))]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(EFS_EventMatrixSwap))]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(EFS_EventMatrixSwaption))]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(EFS_EventMatrixTermDeposit))]
        public EFS_EventMatrixGroup[] group;
		#endregion Members
		#region Constructors
		public EFS_EventMatrixStrategy(){productName = "Strategy"; }
		#endregion Constructors
	}
    #endregion EFS_EventMatrixStrategy
    #region EFS_EventMatrixTrade
    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixTrade : EFS_EventMatrixGroup
    {
        // PM 20120824 [18058] Add XmlElementAttribute EFS_EventMatrixCashBalanceInterest
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("additionalInvoice", typeof(EFS_EventMatrixAdditionalInvoice))]
        [System.Xml.Serialization.XmlElementAttribute("creditNote", typeof(EFS_EventMatrixCreditNote))]
        [System.Xml.Serialization.XmlElementAttribute("bondOption", typeof(EFS_EventMatrixBondOption))]
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(EFS_EventMatrixBulletPayment))]
        [System.Xml.Serialization.XmlElementAttribute("buyAndSellBack", typeof(EFS_EventMatrixBuyAndSellBack))]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(EFS_EventMatrixCapFloor))]
        [System.Xml.Serialization.XmlElementAttribute("commoditySpot", typeof(EFS_EventMatrixCommoditySpot))]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", typeof(EFS_EventMatrixDebtSecurity))]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", typeof(EFS_EventMatrixDebtSecurityTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EFS_EventMatrixEquityOption))]
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransaction", typeof(EFS_EventMatrixEquitySecurityTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", typeof(EFS_EventMatrixExchangeTradedDerivative))]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(EFS_EventMatrixFra))]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(EFS_EventMatrixFxAverageRateOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(EFS_EventMatrixFxBarrierOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(EFS_EventMatrixFxDigitalOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(EFS_EventMatrixFxOptionLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(EFS_EventMatrixFxLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(EFS_EventMatrixFxSwap))]
        [System.Xml.Serialization.XmlElementAttribute("invoice", typeof(EFS_EventMatrixInvoice))]
        [System.Xml.Serialization.XmlElementAttribute("invoiceSettlement", typeof(EFS_EventMatrixInvoiceSettlement))]
        [System.Xml.Serialization.XmlElementAttribute("loanDeposit", typeof(EFS_EventMatrixLoanDeposit))]
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", typeof(EFS_EventMatrixMarginRequirement))]
        [System.Xml.Serialization.XmlElementAttribute("returnSwap", typeof(EFS_EventMatrixReturnSwap))]
        [System.Xml.Serialization.XmlElementAttribute("repo", typeof(EFS_EventMatrixRepo))]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(EFS_EventMatrixStrategy))]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(EFS_EventMatrixSwap))]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(EFS_EventMatrixSwaption))]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(EFS_EventMatrixTermDeposit))]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", typeof(EFS_EventMatrixCashBalance))]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceInterest", typeof(EFS_EventMatrixCashBalanceInterest))]
        [System.Xml.Serialization.XmlElementAttribute("otherPartyPayment", typeof(EFS_EventMatrixPayment))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
    }
    #endregion EFS_EventMatrixTrade
    #endregion Trade / Strategy / Product
}
