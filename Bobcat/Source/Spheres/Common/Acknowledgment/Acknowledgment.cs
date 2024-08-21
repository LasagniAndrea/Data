using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;

using EFS.ACommon;
using EfsML.Enum;

namespace EFS.Common.Acknowledgment
{
    /// <summary>
    /// Informations concernant la gestion des accusés de traitement
    /// </summary>
    [Serializable]
    [XmlRootAttribute("acknowledgment", IsNullable = false)]
    public class AcknowledgmentInfo
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(Form = XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion = EfsMLDocumentVersionEnum.Version30;

        /// <summary>
        /// Identifiant externe du traitement 
        /// </summary>
        [XmlElementAttribute(ElementName = "extlId")]
        public string extlId;

        /// <summary>
        /// liste des accusés de traitement 
        /// </summary>
        [XmlElementAttribute(ElementName = "schedules")]
        public AckSchedules schedules;
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public AcknowledgmentInfo()
        {
            schedules = new AckSchedules();
        }
        #endregion Constructors
    }
}

