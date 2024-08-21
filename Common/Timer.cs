using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EFS.ACommon;

namespace EFS.Common
{
    /// <summary>
    /// Spheres Timer class
    /// </summary>
    public class Timer
    {
        DateTime m_Start;

        internal DateTime Start
        {
            get { return m_Start; }
            private set { m_Start = value; }
        }

        DateTime m_End;

        internal DateTime End
        {
            get { return m_End; }
            private set { m_End = value; }
        }

        internal Timer()
        {
            Start = DateTime.Now;
        }

        private TimeSpan GetElapsedTime()
        {
            End = DateTime.Now;

            TimeSpan span = End - Start;

            return span;
        }

        /// <summary>
        /// get the elapsed time
        /// </summary>
        /// <param name="pFormat">output string format, can be null</param>
        /// <returns>the elapsed time</returns>
        public string GetElapsedTime(string pFormat)
        {
            TimeSpan span = this.GetElapsedTime();

            string timeElapsed;
            switch (pFormat)
            {
                case "hms":

                    if (span.Hours > 0)
                        timeElapsed = String.Format("{0}h {1}mn", span.Hours.ToString(), StrFunc.IntegerPadding(span.Minutes, 2));
                    else
                        timeElapsed = String.Format("{0}mn {1}s", span.Minutes.ToString(), StrFunc.IntegerPadding(span.Seconds, 2));
                    break;

                case "h":

                    timeElapsed = String.Format("{0}h", span.TotalHours);

                    break;

                case "m":

                    timeElapsed = String.Format("{0}m", span.TotalMinutes);

                    break;

                case "s":

                    timeElapsed = String.Format("{0}s", span.TotalSeconds);

                    break;

                case "sec":

                    timeElapsed = String.Format("{0:N0},{1} sec.",
                        span.Days * 24 * 60 * 60 + span.Hours * 60 * 60 + span.Minutes * 60 + span.Seconds,
                        StrFunc.IntegerPadding(span.Milliseconds, 3));

                    break;

                default:

                    timeElapsed = String.Format("{0}:{1}:{2},{3}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);

                    break;
            }

            return timeElapsed;
        }

        internal void Reset()
        {
            this.Start = DateTime.Now;
        }

    }

    /// <summary>
    /// Spheres Timer collection
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    // EG 20180425 Analyse du code Correction [CA2237]
    [ComVisible(false)]
    [Serializable]
    public class TimerCollection : Dictionary<string, Timer>
    {

        /// <summary>
        /// Create a new timer and save it inside the collection
        /// </summary>
        /// <param name="pName">the identifier of the new timer</param>
        /// <returns>true, when the timer is successfull created</returns>
        public bool CreateTimer(string pName)
        {
            return CreateTimer(pName, false);
        }
        public bool CreateTimer(string pName, bool pIsReset)
        {
            bool created = false;

            if (!String.IsNullOrEmpty(pName) && !this.ContainsKey(pName))
            {
                Timer newTimer = new Timer();

                this.Add(pName, newTimer);

                created = true;
            }

            if (pIsReset)
                ResetTimer(pName);

            return created;
        }

        /// <summary>
        /// Reset the timer with the given identifier
        /// </summary>
        /// <param name="pName">timer identifier</param>
        /// <returns>true when timer successfull reset</returns>
        public bool ResetTimer(string pName)
        {
            bool reset = false;

            if (!String.IsNullOrEmpty(pName) && this.ContainsKey(pName))
            {
                Timer timerToReset = this[pName];

                timerToReset.Reset();

                reset = true;
            }

            return reset;
        }

        /// <summary>
        /// get the elapsed time from the "pName" timer 
        /// </summary>
        /// <param name="pName">timer identifier</param>
        /// <param name="pOperation">label of the operation</param>
        /// <param name="pFormat">output timespan string format</param>
        /// <returns>the string: "Operation [{pOperation}] completed in {timer[{pName}]}."</returns>
        public string GetElapsedTime(string pName, string pOperation, string pFormat)
        {
            string timeElapsed = Cst.None;

            if (!String.IsNullOrEmpty(pName) && this.ContainsKey(pName))
            {
                Timer timerToReset = this[pName];

                timeElapsed = string.Format("[{0}] completed in {1}.", pOperation, timerToReset.GetElapsedTime(pFormat));
            }

            return timeElapsed;
        }
        /// <summary>
        /// get the elapsed time from the "pName" timer 
        /// </summary>
        /// <param name="pName">timer identifier</param>
        /// <param name="pFormat">output timespan string format</param>
        /// <returns></returns>
        public string GetElapsedTime(string pName, string pFormat)
        {
            string timeElapsed = Cst.None;
            //
            if (!String.IsNullOrEmpty(pName) && this.ContainsKey(pName))
            {
                Timer timerToReset = this[pName];
                timeElapsed = timerToReset.GetElapsedTime(pFormat);
            }
            //
            return timeElapsed;
        }

        public string GetElapsedTime(string pName)
        {
            return GetElapsedTime(pName, false);
        }
        public string GetElapsedTime(string pName, bool pIsReset)
        {
            return GetElapsedTime(pName, "default", pIsReset);
        }
        public string GetElapsedTime(string pName, string pFormat, bool pIsReset)
        {
            string timeElapsed = Cst.None;

            if (!String.IsNullOrEmpty(pName) && this.ContainsKey(pName))
            {
                Timer timerToReset = this[pName];

                timeElapsed = string.Format("{0} : {1}", timerToReset.GetElapsedTime(pFormat), pName);

                if (pIsReset)
                    ResetTimer(pName);
            }

            return timeElapsed;
        }
    }
}