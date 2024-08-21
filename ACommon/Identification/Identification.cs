using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ACommon
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISpheresIdentification
    {
        #region Accessors
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        //
        string Identifier { set; get; }
        //
        string Displayname { set; get; }
        //
        bool DescriptionSpecified { set; get; }
        string Description { set; get; }
        //
        bool ExtllinkSpecified { set; get; }
        string Extllink { set; get; }
        #endregion Accessors
    }

    /// <summary>
    /// Représente l'identification d'un élément dans Spheres®
    /// <para>L'identification contient: un id, un identifier, un displayname, une decription et un extllink</para>
    /// </summary>
    // PL 20171020 [23490] add Timezone GLOP
    public class SpheresIdentification : ISpheresIdentification
    {
        #region Members
        private string _otcmlId;

        private string _identifier;
        private string _lastIdentifier;
        private string _displayname;

        private bool _descriptionSpecified;
        private string _description;

        private bool _timezoneSpecified;
        private string _timezone;

        private bool _extllinkSpecified;
        private string _extllink;
        #endregion Members

        #region Properties
        public int OTCmlId
        {
            get { return Convert.ToInt32(OtcmlId); }
            set { OtcmlId = value.ToString(); }
        }

        public string OtcmlId
        {
            get { return _otcmlId; }
            set { _otcmlId = value; }
        }
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
        // RD 20150807 Add
        public string LastIdentifier
        {
            get { return _lastIdentifier; }
            set { _lastIdentifier = value; }
        }

        public string Displayname
        {
            get { return _displayname; }
            set { _displayname = value; }
        }

        public bool TimezoneSpecified
        {
            get { return _timezoneSpecified; }
            set { _timezoneSpecified = value; }
        }

        public string Timezone
        {
            get { return _timezone; }
            set
            {
                _timezone = value;
                TimezoneSpecified = StrFunc.IsFilled(Timezone);
            }
        }

        public bool DescriptionSpecified
        {
            get { return _descriptionSpecified; }
            set { _descriptionSpecified = value; }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                DescriptionSpecified = StrFunc.IsFilled(Description);
            }
        }

        public bool ExtllinkSpecified
        {
            get { return _extllinkSpecified; }
            set { _extllinkSpecified = value; }
        }
        public string Extllink
        {
            get { return _extllink; }
            set
            {
                _extllink = value;
                ExtllinkSpecified = StrFunc.IsFilled(Extllink);
            }
        }
        #endregion

        #region Constructors
        public SpheresIdentification()
        { }

        public SpheresIdentification(int pId, string pIdentifier)
        {
            OTCmlId = pId;
            _identifier = pIdentifier;
        }
        public SpheresIdentification(string pIdentifier, string pDisplayname, string pDescription, string pExtllink)
        {
            _identifier = pIdentifier;
            _displayname = pDisplayname;
            _description = pDescription;
            _extllink = pExtllink;
        }
        public SpheresIdentification(string pIdentifier, string pDisplayname, string pDescription, string pTimezone, string pExtllink)
        {
            _identifier = pIdentifier;
            _displayname = pDisplayname;
            _description = pDescription;
            _timezone = pTimezone;
            _extllink = pExtllink;
        }
        #endregion
    }

    /// <summary>
    /// Classe dédiée aux acteur
    /// couche supplémentaire à SpheresIdentification
    /// actuellement seul le BusinessCenter est remonté dans cette classe.
    /// </summary>
    /// EG 20210419 [XXXXX] New Implementation Nouvelle class pour Acteur (container des données propre à la table ACTOR) - ici BusinessCenter
    public class ActorIdentification : SpheresIdentification
    {
        #region Members
        private string _businessCenter;
        private bool _businessCenterSpecified;
        #endregion Members

        #region Properties
        public string BusinessCenter
        {
            get { return _businessCenter; }
            set { _businessCenter = value; }
        }
        public bool BusinessCenterSpecified
        {
            get { return _businessCenterSpecified; }
            set { _businessCenterSpecified = value; }
        }
        #endregion

        #region Constructors
        public ActorIdentification()
        { }

        public ActorIdentification(int pId, string pIdentifier, string pBusinessCenter) : base(pId, pIdentifier)
        {
            _businessCenter = pBusinessCenter;
            _businessCenterSpecified = StrFunc.IsFilled(pBusinessCenter);
        }
        public ActorIdentification(string pIdentifier, string pDisplayname, string pDescription, string pExtllink, string pBusinessCenter) : base(pIdentifier, pDisplayname, pDescription, pExtllink)
        {
            _businessCenter = pBusinessCenter;
            _businessCenterSpecified = StrFunc.IsFilled(pBusinessCenter);
        }
        public ActorIdentification(string pIdentifier, string pDisplayname, string pDescription, string pTimezone, string pExtllink, string pBusinessCenter) : base(pIdentifier, pDisplayname, pDescription, pTimezone, pExtllink)
        {
            _businessCenter = pBusinessCenter;
            _businessCenterSpecified = StrFunc.IsFilled(pBusinessCenter);
        }
        #endregion
    }


    /// <summary>
    /// Représente un Id (sa valeur (<see cref="id"/>), son type (<see cref="idIdent"/>), l'identifier associé (<see cref="idIdentifier"/>))
    /// </summary>
     [Serializable]
    public class IdData
    {
        #region Members
        /// <summary>
        ///  Représente un id non significatif  
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public int id;
        /// <summary>
        ///  Identification de l'id (EX IOTASK)
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string idIdent;
        /// <summary>
        /// Identifier associé
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string idIdentifier;
        #endregion Members

        #region Constructors
        public IdData()
        {
        }
        public IdData(int pIdData, string pIdDataIdentifier, string pIdDataIdent)
        {
            id = pIdData;
            idIdentifier = pIdDataIdentifier;
            idIdent = pIdDataIdent;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Représente un Id (sa valeur (<see cref="id"/>) et diverses informations complémentaires (<see cref="idInfos"/>))
    /// </summary>
    [Serializable]
    public class IdInfo
    {
        #region Members
        /// <summary>
        /// Représente un id non significatif  
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public int id;

        /// <summary>
        /// Représente des informations complémentaires
        /// </summary>
        [System.Xml.Serialization.XmlArray()]
        public DictionaryEntry[] idInfos;
        #endregion

        #region Constructors
        public IdInfo() { }
        #endregion

    }

}
