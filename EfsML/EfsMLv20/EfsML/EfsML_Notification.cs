#region using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;

using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Notification; 

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Doc;
#endregion using directives



namespace EfsML.v20.Notification
{

    
    /// <summary>
    /// Modelisation d'un message de confirmation
    /// Contient un header et .....
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("confirmationMessage", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class NotificationDocument
    {
        #region Members
        //Version du document
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
        //
        [System.Xml.Serialization.XmlElementAttribute("header", Order = 1)]
        public NotificationHeader header;
        //
        [System.Xml.Serialization.XmlElementAttribute("dataDocument", Order = 2)]
        public DataDocument dataDocument;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notepadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notepad", Order = 3)]
        public CDATA[] notepad;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("events", Order = 4)]
        public Events events;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hierarchicalEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("events", Order = 5)]
        public HierarchicalEvents hierarchicalEvents;
        #endregion Members
    }
    

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationHeader
    {
        #region Members
        /// <summary>
        /// IdMCO => Id unique du message 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("confirmationMessageId", Order = 1)]
        public NotificationId[] confirmationMessageId;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("notificationConfirmationSystemIds", Order = 2)]
        public NotificationConfirmationSystemIds notificationConfirmationSystemIds;
        
        /// <summary>
        /// Date Generation
        /// </summary>
        /// FI 20140808 [20549] creationTimestamp est de type EFS_DateTimeUTC
        [System.Xml.Serialization.XmlElementAttribute("creationTimestamp", Order = 3)]
        public EFS_DateTimeUTC creationTimestamp;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousCreationTimestampSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        [System.Xml.Serialization.XmlElementAttribute("previousCreationTimestamp", Order = 4)]
        public PreviousDateTimeUTC[] previousCreationTimestamp;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Order = 5)]
        public EFS_Date valueDate;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueDate2Specified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("valueDate2", Order = 6)]
        public EFS_Date valueDate2;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sendBy", Order = 7)]
        public NotificationRouting sendBy;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sendTo", Order = 8)]
        public NotificationRouting[] sendTo;
        
        /// <summary>
        /// /
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool copyToSpecified;
        /// <summary>
        /// /
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("copyTo", Order = 9)]
        public NotificationRoutingCopyTo[] copyTo;
        
        /// <summary>
        /// /
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool softApplicationSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("softApplication", Order = 10)]
        public SoftApplication softApplication;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportCurrencySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingCurrency", Order = 11)]
        public Currency reportCurrency;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportFeeDisplaySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingFeeDisplay", Order = 12)]
        public string reportFeeDisplay;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportSettingsSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportSettings", Order = 13)]
        public ReportSettings reportSettings;
        #endregion Members
    }
    

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRouting : RoutingPartyReference
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool partyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyRelativeTo", Order = 1)]
        public PartyReference partyReference;
    }
    

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRoutingCopyTo : NotificationRouting
    {
        [System.Xml.Serialization.XmlAttributeAttribute("bcc", DataType = "boolean")]
        public bool isBcc;
    }
    

}