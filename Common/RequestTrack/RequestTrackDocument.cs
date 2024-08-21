using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


using EfsML.Enum;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Actor;



namespace EFS.Common.Log
{

    /// <summary>
    /// Représente la journalisation d'une action utilisateur
    /// </summary>
    [XmlRootAttribute("requestTrack", IsNullable = false)]
    public class RequestTrackDocument
    {
        /// <summary>
        /// version du docuement
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum version;

        /// <summary>
        /// base de données
        /// </summary>
        [XmlElement("database", IsNullable = false)]
        public RequestTrackDatabase database;

        /// <summary>
        /// Requête Http
        /// </summary>
        [XmlElement("request", IsNullable = false)]
        public RequestTrackWebRequest request;

        /// <summary>
        /// user
        /// </summary>
        [XmlElement("user", IsNullable = false)]
        public RequestTrackUser user;

        /// <summary>
        /// action utilisateur
        /// </summary>
        [XmlElement("action", IsNullable = false)]
        public RequestTrackAction action;


        /// <summary>
        /// Donnée exposées dans le document
        /// </summary>
        [XmlIgnoreAttribute()]
        public Boolean dataSpecified;
        [XmlElement("data", IsNullable = false)]
        public RequestTrackData data;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20141021 [20350] Modify
        public RequestTrackDocument()
        {
            // FI 20141021 [20350] avec le ticket 20350, le document est en version 2.0
            version = EfsMLDocumentVersionEnum.Version20;
            database = new RequestTrackDatabase();
            data = new RequestTrackData();
            request = new RequestTrackWebRequest();
        }
        #endregion constructor
    }

    /// <summary>
    /// Représente une base de données
    /// </summary>
    public class RequestTrackDatabase
    {
        [XmlAttribute("cs")]
        public string cs;
    }

    /// <summary>
    /// Représente les éléments de base pour identifier un élément
    /// </summary>
    public class RequestTrackRepository
    {
        [XmlIgnore]
        public Boolean idSpecified;
        /// <summary>
        ///  Id non significatif (IDA,IDB, etc...)
        ///  <para>Certains référentiels ne possèdent pas d'id non significatif</para>
        /// </summary>
        [XmlAttribute("sid")]
        public int id;

        /// <summary>
        /// Identifier
        /// </summary>
        [XmlAttribute("n")]
        public string identifier;

        /// <summary>
        /// Display Name
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("dn")]
        public string displayName;

        [XmlIgnore]
        public Boolean descriptionSpecified;
        /// <summary>
        /// Description
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("desc")]
        public string description;

        [XmlIgnore]
        public Boolean extl1Specified;
        /// <summary>
        /// External Id
        /// </summary>
        [XmlAttribute("extl1")]
        public string extl1;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public Boolean extl2Specified;
        /// <summary>
        /// 2nd External Id
        /// </summary>
        [XmlAttribute("extl2")]
        public string extl2;

    }

    /// <summary>
    /// Représente un acteur (id, identifier, displayName, description, addr1, addr2, extl1, extl2)
    /// </summary>
    public class RequestTrackActor : RequestTrackRepository
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Boolean addr1Specified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("addr1")]
        public string addr1;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Boolean addr2Specified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("addr2")]
        public string addr2;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Boolean roleSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("role")]
        public string role;

    }

    /// <summary>
    /// Représente un book
    /// </summary>
    public class RequestTrackBook : RequestTrackRepository
    {
        /// <summary>
        /// Actor propriétaire au book
        /// </summary>
        [XmlElement("ownerRef", IsNullable = false)]
        public RequestTrackActorReference ownerRef;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Boolean actorRefSpecified;
        /// <summary>
        /// Actor rattaché au book
        /// </summary>
        [XmlElement("actorRef", IsNullable = true)]
        public RequestTrackActorReference actorRef;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestTrackBook()
        {
            ownerRef = new RequestTrackActorReference();
            actorRef = new RequestTrackActorReference();
        }
    }

    /// <summary>
    /// Reference to an actor
    /// </summary>
    public class RequestTrackActorReference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string href;
    }

}
