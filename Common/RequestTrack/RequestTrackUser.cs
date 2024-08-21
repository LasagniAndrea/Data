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
    /// Représente un utilisateur
    /// <para>Contient notamment les acteurs parents</para>
    /// </summary>
    public class RequestTrackUser : RequestTrackActor
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool parentSpecified;

        [System.Xml.Serialization.XmlArray("parents")]
        [System.Xml.Serialization.XmlArrayItem("parent")]
        public RequestTrackActor[] parent;
    }

}