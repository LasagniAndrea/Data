using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common;
using EFS.Import;
using EFS.TradeInformation;
using EFS.GUI.CCI;
using EFS.ApplicationBlocks.Data;
using EFS.Status;

using EfsML.Interface;
using EfsML.DynamicData;

using FpML.Interface;

namespace EFS.TradeInformation.Import
{
    /// <summary>
    /// Caractéristiques du trade à importer
    /// </summary>
    /// FI 20120226 add class TradeImportInput
    public class TradeImportInput
    {
        #region members
        /// <summary>
        /// Représente les statuts associé au trade
        /// </summary>
        private TradeStatus _tradeStatus;

        /// <summary>
        /// Représente les données sous forme d'un collection de cci
        /// </summary>
        private TradeCommonCustomCaptureInfos _customCaptureInfos;

        /// <summary>
        /// 
        /// </summary>
        private FullCustomCaptureInfo[] _fullCustomCaptureInfos;
        #endregion members

        #region accessors
        /// <summary>
        /// Représente les données sous forme d'un collection de cci
        /// </summary>
        [System.Xml.Serialization.XmlArray("customCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("customCaptureInfo", typeof(CustomCaptureInfoDynamicData))]
        public TradeCommonCustomCaptureInfos CustomCaptureInfos
        {
            set { _customCaptureInfos = value; }
            get { return _customCaptureInfos; }
        }


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FullCustomCaptureInfosSpecified;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlArray("fullCustomCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("fullCustomCaptureInfo", typeof(FullCustomCaptureInfo))]
        public FullCustomCaptureInfo[] FullCustomCaptureInfos
        {
            set { _fullCustomCaptureInfos = value; }
            get { return _fullCustomCaptureInfos; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TradeStatusSpecified;

        /// <summary>
        /// Représente le statut avec lequel le tradedoit être importé
        /// </summary>
        [System.Xml.Serialization.XmlElement("tradeStatus", typeof(TradeStatus))]
        public TradeStatus TradeStatus
        {
            set { _tradeStatus = value; }
            get { return _tradeStatus; }
        }
        #endregion accessors

    }
    
}
