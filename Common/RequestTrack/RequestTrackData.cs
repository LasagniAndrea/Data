using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue; 
using EFS.Actor;



namespace EFS.Common.Log
{

    /// <summary>
    ///  Représente les données présente dans un RequestTrackDocument
    /// </summary>
    public class RequestTrackData
    {
        /// <summary>
        ///  Regroupement des données
        /// </summary>
        [System.Xml.Serialization.XmlElement("group", IsNullable = false)]
        public RequestTrackDataGrp[] group;

        /// <summary>
        /// 
        /// </summary>
        public RequestTrackData()
        {

        }

    }

    /// <summary>
    /// Représente une regroupement
    /// </summary>
    public class RequestTrackDataGrp
    {
        /// <summary>
        /// Identifiant du regroupement
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("n")]
        public string identifier;

        /// <summary>
        /// Représente les couples Books du regroupement
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean bookSpecified;
        [System.Xml.Serialization.XmlArrayItem("book", IsNullable = false)]
        public RequestTrackBook[] book;


        /// <summary>
        /// Représente les acteurs du regroupement
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean actorSpecified;
        [System.Xml.Serialization.XmlArrayItem("actor", IsNullable = false)]
        public RequestTrackActor[] actor;

    }

}