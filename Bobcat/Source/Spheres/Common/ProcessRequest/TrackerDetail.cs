using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Acknowledgment;

namespace EFS.Process
{
    /// <summary>
    /// 
    /// </summary>
    public class TrackerData
    {
        #region Members
        public string sysCode;
        public int sysNumber;
        public string data1;
        public string data2;
        public string data3;
        public string data4;
        public string data5;
        #endregion Members
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public TrackerData() { }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }
    
    /// <summary>
    /// Informations pour la gestion des accusés de traitement
    /// </summary>
    [Serializable]
    [XmlRootAttribute("acknowledgment", IsNullable = false)]
    public class TrackerAcknowledgmentInfo : AcknowledgmentInfo
    {

        [XmlIgnoreAttribute()]
        public bool idInfoSpecified;
        /// <summary>
        ///  Représente une partie du Message queue à l'origine du  traitement de manière à pourvoir le restituer dans l'accusé de traitement
        ///  <para>Remarque : à ce jour seuls les accusée de reception de tye <seealso cref="AckWebSessionSchedule"/> utilise idInfo</para>
        /// </summary>
        [XmlElementAttribute(ElementName = "idInfo")]
        public IdInfo idInfo;

        /// <summary>
        /// 
        /// </summary>
        public TrackerAcknowledgmentInfo() : base()
        { }


        /// <summary>
        ///  Mise en place d'un accusé de traitement de type <see cref="AckWebSessionSchedule"/>
        /// </summary>
        /// <param name="pIdInfo"></param>
        public void SetAckWebSessionSchedule(IdInfo pIdInfo)
        {
            idInfoSpecified = (null != pIdInfo);
            if (idInfoSpecified)
                idInfo = pIdInfo;

            SetAckWebSessionSchedule();
        }

        /// <summary>
        ///  Mise en place d'un accusé de traitement de type <see cref="AckWebSessionSchedule"/>
        /// </summary>
        public void SetAckWebSessionSchedule()
        {
            schedules = new AckSchedules(
                        new AckWebSessionSchedule()
                        {
                            responseSpecified = true,
                            response = MOMSettings.LoadMOMSettings(Cst.ProcessTypeEnum.RESPONSE)
                        }
                    );
        }
    }


}
