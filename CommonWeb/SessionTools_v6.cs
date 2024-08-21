using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;

//20071212 FI Ticket 16012 => Migration Asp2.0


// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{

    /// <summary>
    /// Classe qui gère les variables session
    /// <para>Attention cette classe s'appuie sur HttpContext.Current</para>
    /// <para>Elle doit être utilisée uniquement lors d'une réponse à une requête Http</para>
    /// </summary>
    public sealed partial class SessionTools
    {
        public static Dictionary<string, QueryParameters> CachedWSLstQuery
        {
            get
            {
                Dictionary<string, QueryParameters> ret = null;
                if (IsSessionAvailable)
                {
                    ret = (HttpSessionStateTools.Get(SessionTools.SessionState, "WSLstQuery")) as Dictionary<string, QueryParameters>;
                }
                return ret;
            }

            set
            {
                HttpSessionStateTools.Set(SessionState, "WSLstQuery", value);
            }
        }
        public static Dictionary<string, List<DataTypeAhead>> CachedWSLstData
        {
            get
            {
                Dictionary<string, List<DataTypeAhead>> ret = null;
                if (IsSessionAvailable)
                {
                    ret = (HttpSessionStateTools.Get(SessionTools.SessionState, "WSLstData")) as Dictionary<string, List<DataTypeAhead>>;
                }
                return ret;
            }

            set
            {
                HttpSessionStateTools.Set(SessionState, "WSLstData", value);
            }
        }
        /// <summary>
        /// Retourne l'historique des consultations TRACKER
        /// <para>Retourne ToDay, 1D, 7D, 1M, 3M ou Beyond</para>
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <returns></returns>
        public static string GetTrackerInfo(Cst.SQLCookieElement pSQLCookieElement, string pDefaultValue)
        {
            string element = pSQLCookieElement.ToString();
            string tmpvalue;
            try
            {
                tmpvalue = (String)SessionState[Cst.SQLCookieGrpElement.Tracker + "_" + element];
                if (StrFunc.IsEmpty(tmpvalue))
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.Tracker, element, out tmpvalue);
                    SessionState[element] = tmpvalue;
                }
            }
            catch { tmpvalue = pDefaultValue; }

            if (StrFunc.IsEmpty(tmpvalue))
                tmpvalue = pDefaultValue;
            return tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pValue"></param>
        public static void SetTrackerInfo(Cst.SQLCookieElement pSQLCookieElement, string pValue, string pDefaultValue)
        {
            string element = pSQLCookieElement.ToString();
            string tmpvalue;
            try
            {
                AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Tracker, element, pValue);
                tmpvalue = pValue;
            }
            catch { tmpvalue = pDefaultValue; }
            SessionState[element] = tmpvalue;
        }

        /// <summary>
        /// Obitient ou définit l'historique du tracker
        /// </summary>
        public static string TrackerDisplayMode
        {
            get { return GetTrackerInfo(Cst.SQLCookieElement.TrackerDisplayMode, "Group"); }
            set { SetTrackerInfo(Cst.SQLCookieElement.TrackerDisplayMode, value, "Group"); }
        }


        public static Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> TrackerDisplayValues
        {
            get
            {
                string element = Cst.SQLCookieElement.TrackerDisplayValues.ToString();
                Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> dic = null;
                object itemSession = HttpSessionStateTools.Get(SessionState, element);
                if (null != itemSession)
                    dic = itemSession as Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>>;
                if ((null == dic) || (0 == dic.Count))
                {
                    dic = TrackerTools.ReadTrackerDisplayValueFromCookie(SessionTools.CS);
                    if (IsSessionAvailable)
                        SessionState[element] = dic;

                }
                else
                {
                    dic = itemSession as Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>>;
                }
                return dic;
            }
            set
            {
                TrackerTools.SaveTrackerDisplayValueToCookie(value);
                HttpSessionStateTools.Set(SessionState, Cst.SQLCookieElement.TrackerDisplayValues.ToString(), value);
            }
        }

    }
}
