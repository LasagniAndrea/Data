//
using EFS.ACommon;
using System;
using System.Diagnostics;

namespace EFS.Common.Log
{


    /// <summary>
    /// Trace Event
    /// </summary>
    /// PM 20160104 [POC MUREX] Add
    public class TraceEvent
    {
        #region Properties
        /// <summary>
        /// Type d'évènement
        /// </summary>
        public TraceEventType EventType { get; set; }

        /// <summary>
        /// Id de l'évènement
        /// </summary>
        public Int16 EventId { get; set; }

        /// <summary>
        /// Message associé à l'évènement
        /// </summary>
        public string EventData { get; set; }
        #endregion Properties

        #region  Conctructors
        public TraceEvent() : this(TraceEventType.Information, 0, null) { }
        public TraceEvent(TraceEventType pType, string pData) : this(pType, 0, pData) { }
        public TraceEvent(TraceEventType pType, Int16 pId, string pData)
        {
            EventType = pType;
            EventId = pId;

            //PL 20160524 Newness for SQL
            if (StrFunc.IsFilled(pData) && pData.StartsWith("SQL:"))
            {
                EventData = "SQL:" + Cst.CrLf;
                EventData += "-- -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+" + Cst.CrLf;
                EventData += pData.Substring(4).Trim() + Cst.CrLf;
                EventData += "-- -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+";
            }
            else
            {
                EventData = pData;
            }
        }
        #endregion Conctructors
    }

    /// FI 20180314 [XXXXX] Add
    public class TraceTimeItem
    {
        public TraceTimeItem()
        { }

        /// <summary>
        /// 
        /// </summary>
        public DateTime dtStart;

        /// <summary>
        /// 
        /// </summary>
        public DateTime dtEnd;

        /// <summary>
        /// 
        /// </summary>
        public String description;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetTimeSpan()
        {
            return dtEnd - dtStart;
        }
    }
}
