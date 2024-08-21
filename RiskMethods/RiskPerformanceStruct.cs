using System;
//
using EFS.Process;
using EFS.SpheresRiskPerformance.Enum;
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Parameters collection for the risk margin evaluation
    /// </summary>
    /// <remarks>
    /// This collection is built parsing the input request received from the service mqueue
    /// </remarks>
    /// FI 20141126 [20526] Modify
    public struct RiskPerformanceProcessInfo
    {
        /// <summary>
        /// Clearing House internal Id
        /// </summary>
        public int CssId;
        public string CssIdentifier;

        /// <summary>
        /// Delete the previous evaluation
        /// </summary>
        public bool Reset;

        /// <summary>
        /// Evaluation mode
        /// </summary>
        public RiskEvaluationMode Mode;

        /// <summary>
        /// Risk Timing
        /// </summary>
        public SettlSessIDEnum Timing;

        /// <summary>
        /// internal id of the entity for which the risk evaluation request is generated
        /// </summary>
        public int Entity;
        /// <summary>
        /// internal identifier of the entity for which the risk evaluation request is generated
        /// </summary>
        /// FI 20141126 [20526] Add
        public string EntityIdentifier;

        /// <summary>
        /// Date business of the posted request
        /// </summary>
        public DateTime DtBusiness;

        /// <summary>
        /// Hours/minutes, depending by the SettlSessIDEnum value - of the posted request
        /// </summary>
        public TimeSpan TimeBusiness;

        /// <summary>
        /// request identifier (from table POSREQUEST) - can be null
        /// </summary>
        public int? IdPr;

        /// <summary>
        /// internal id of the actor MARGINREQOFFICE for which the risk evaluation request is generated 
        /// </summary>
        /// <value>is optional, can be null</value>
        public Nullable<int> MarginReqOfficeChild;

        /// <summary>
        /// Horaire de prise en compte de la position pour un calcul Intra-Day
        /// </summary>
        //PM 20140214 [19493] New
        public TimeSpan PositionTime;

        /// <summary>
        /// Horaire des prix et paramètres de calcul à considérer pour un calcul Intra-Day
        /// </summary>
        //PM 20140214 [19493] New
        public TimeSpan RiskDataTime;

        /// <summary>
        /// Software à l'origine de la demande calcul
        /// </summary>
        //PM 20141028 [9700] New (Eurex Prisma for Eurosys Futures)
        public string SoftwareRequester;

        /// <summary>
        /// Process du calcul
        /// </summary>
        // PM 20180219 [23824] New
        public ProcessBase Process;
    }
}
