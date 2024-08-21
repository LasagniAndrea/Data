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
    /// Représente une requête http
    /// </summary>
    public class RequestTrackWebRequest
    {
        /// <summary>
        ///  date de la demande http
        /// </summary>
        [XmlAttribute("timestamp")]
        public string timestamp;

        /// <summary>
        ///  Identification unique de la page web (guid:global unique identifier)
        /// </summary>
        [XmlAttribute("guid")]
        public string guid;

        /// <summary>
        ///  Représente la session dans laquelle la requête http est effectuée
        /// </summary>
        [XmlElement("session", IsNullable = false)]
        public RequestTrackWebSession session;

        /// <summary>
        ///  URL de  requête Http
        /// </summary>
        [XmlElement("url", IsNullable = false)]
        public CDATA url;

        /// <summary>
        /// 
        /// </summary>
        public RequestTrackWebRequest()
        {
            session = new RequestTrackWebSession();
            url = new CDATA();
        }

    }

    /// <summary>
    /// Représente une session 
    /// </summary>
    public class RequestTrackWebSession
    {
        /// <summary>
        ///  Identification unique de la session web
        /// </summary>
        [XmlAttribute("guid")]
        public string guid;
        /// <summary>
        ///  Contexte Machine dans lequel est exécutée la session web 
        /// </summary>
        [XmlAttribute("hostname")]
        public string hostname;

    }



}
