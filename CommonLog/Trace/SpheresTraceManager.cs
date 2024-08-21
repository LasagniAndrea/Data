using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace EFS.Common.Log
{
    /// <summary>
    /// SpheresTraceManager: Gestion des traces nommées "SpheresTrace" et  "SpheresTraceTime"
    /// </summary>
    /// PM 20160104 [POC MUREX] Add
    public class SpheresTraceManager : TraceManagerBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private static readonly string _SpheresTraceName = "SpheresTrace";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180314 [XXXXX] Add
        private static readonly string _SpheresTraceTimeName = "SpheresTraceTime";
        #endregion Members

        #region Accessors

        /// <summary>
        /// Trace nommée <see cref="_SpheresTraceName"/>. Trace de l'application (informations, warnings, erreurs , etc..)
        /// </summary>
        public SpheresTraceSource SpheresTrace
        {
            get;
            private set;
        }

        /// <summary>
        /// Trace nommée <see cref="_SpheresTraceTimeName"/>. Trace des durées de traitements
        /// </summary>
        /// FI 20180314 [XXXXX] Add
        public SpheresTraceSource SpheresTraceTime
        {
            get;
            private set;
        }

        private Dictionary<string, TraceTimeItem> dicTraceTime;

        /// <summary>
        /// Retourne true si <see cref="SpheresTrace"/> est disponible  
        /// </summary>
        /// FI 20200406 [XXXXX] Add
        public Boolean IsSpheresTraceAvailable
        {
            get { return (null != SpheresTrace); }

        }

        /// <summary>
        /// Retourne true si <see cref="SpheresTraceTime"/> est disponible  
        /// </summary>
        /// FI 20200406 [XXXXX] Add
        public Boolean IsSpheresTracetimeAvailable
        {
            get { return (null != SpheresTraceTime); }

        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileNameSuffixe">suffixe appliqué au nom de fichier de trace</param>
        public SpheresTraceManager(string pFileNameSuffixe) :
            base(pFileNameSuffixe)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileNameSuffixe">suffixe appliqué au nom de fichier de trace</param>
        /// <param name="pGetFilePath"></param>
        public SpheresTraceManager(string pFileNameSuffixe, GetFilepath pGetFilePath) :
            base(pFileNameSuffixe, pGetFilePath)
        {

        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180314 [XXXXX] Modify
        public void Flush()
        {
            if (null != SpheresTrace)
                SpheresTrace.Flush();

            // FI 20180314 [XXXXX] Add
            if (null != SpheresTraceTime)
                SpheresTraceTime.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180314 [XXXXX] Modify
        public void Close()
        {
            if (null != SpheresTrace)
            {
                SpheresTrace.Flush();
                SpheresTrace.Close();
                SpheresTrace = null;
            }

            // FI 20180314 [XXXXX] Add
            if (null != SpheresTraceTime)
            {
                SpheresTraceTime.Flush();
                SpheresTraceTime.Close();
                SpheresTraceTime = null;
            }
        }

        /// <summary>
        /// Mise en place des traces <see cref="SpheresTrace"/> et <see cref="SpheresTraceTime"/>
        /// <para>Sauvegarde des traces précédentes. Suppression des anciennes traces de plus de 30 jours</para>
        /// </summary>
        /// FI 20180314 [XXXXX] Modify
        public void NewTrace()
        {
            // Close file if already exists
            this.Close();

            PrepareTrace();

            ConfigurationSection diagnosticsSection = (ConfigurationSection)ConfigurationManager.GetSection("system.diagnostics");
            if (null != diagnosticsSection)
            {
                ConfigurationElementCollection sharedListeners = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sharedListeners"].Value;
                ConfigurationElementCollection sources = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sources"].Value;

                if (((null != sharedListeners) && (sharedListeners.Count > 0)) || ((null != sources) && (sources.Count > 0)))
                {
                    // Create new file
                    SpheresTrace = InitializeTrace(SpheresTraceManager._SpheresTraceName);

                    // Create new file
                    // FI 20180314 [XXXXX] add SpheresTraceTime
                    SpheresTraceTime = InitializeTrace(SpheresTraceManager._SpheresTraceTimeName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTraceName"></param>
        /// <returns></returns>
        private SpheresTraceSource InitializeTrace(string pTraceName)
        {
            // Create new file
            SpheresTraceSource tradeSource = new SpheresTraceSource(pTraceName);
            
            if (SourceLevels.Off != tradeSource.Switch.Level)
            {
                ModifyListenersFile(tradeSource);
            }
            else
            {
                tradeSource = null;
            }
            return tradeSource;
        }


        /// <summary>
        /// Alimente <see cref="SpheresTrace"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pMessage"></param>
        public virtual void TraceInformation(object pSource, string pMessage)
        {
            Trace(pSource, new TraceEvent(TraceEventType.Information, 0, pMessage));
        }

        /// <summary>
        /// Alimente <see cref="SpheresTrace"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pMessage"></param>
        public virtual void TraceVerbose(object pSource, string pMessage)
        {
            Trace(pSource, new TraceEvent(TraceEventType.Verbose, 0, pMessage));
        }

        /// <summary>
        /// Alimente <see cref="SpheresTrace"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pMessage"></param>
        /// FI 20190719 [XXXXX] Add
        public virtual void TraceWarning(object pSource, string pMessage)
        {
            Trace(pSource, new TraceEvent(TraceEventType.Warning, 0, pMessage));
        }

        /// <summary>
        /// Alimente <see cref="SpheresTrace"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pMessage"></param>
        /// FI 20190705 [XXXXX] Add
        public virtual void TraceError(object pSource, string pMessage)
        {
            Trace(pSource, new TraceEvent(TraceEventType.Error, 0, pMessage));
        }

        /// <summary>
        /// Alimente <see cref="SpheresTrace"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTraceEvent"></param>
        /// FI 20170215 [XXXXX] Modify
        /// FI 20180330 [XXXXX] Modify
        public virtual void Trace(object pSource, TraceEvent pTraceEvent)
        {
            if (null != this.SpheresTrace)
            {
                lock (this.SpheresTrace)
                {
                    string objectSource = GetObjectSourceName(pSource);
                    string threadName = ThreadingTools.GetThreadName();

                    string messageEventType = pTraceEvent.EventType.ToString();
                    string message = pTraceEvent.EventData;
                    string[] data = { threadName, objectSource, message };

                    this.SpheresTrace.TraceData(pTraceEvent.EventType, pTraceEvent.EventId, data);
                }
            }
        }


        /// <summary>
        /// Initie dans la trace des durée de traitement l'étape {pSetp} pour la clé {pKey}
        /// </summary>
        /// <param name="pStep">Exemple: UTICALCULATION</param>
        /// <param name="pKey">Exemple: (IDT:20200)</param>
        /// <param name="pDescription"></param>
        /// FI 20180314 [XXXXX] Add
        public virtual void TraceTimeBegin(string pStep, string pKey, string pDescription)
        {
            if (this.SpheresTraceTime != null)
            {
                lock (this.SpheresTraceTime)
                {
                    try
                    {
                        if (pStep.Contains('['))
                        {
                            throw new ArgumentException("Char '[' is not allowed");
                        }
                        if (null == dicTraceTime)
                        {
                            dicTraceTime = new Dictionary<string, TraceTimeItem>();
                        }
                        string key = StrFunc.AppendFormat("{0}[@key={1}]", pStep, pKey);
                        if (false == dicTraceTime.ContainsKey(key))
                        {
                            dicTraceTime.Add(key, new TraceTimeItem() { dtStart = DateTime.Now, description = pDescription });
                        }
                        else
                        {
                            dicTraceTime[key].dtStart = DateTime.Now;
                            dicTraceTime[key].dtEnd = DateTime.MinValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        // FI 20200910 [XXXXX] Appel à MessageExtended
                        Trace(this, new TraceEvent(TraceEventType.Error, SpheresExceptionParser.GetSpheresException("Exception during TraceTimeBegin", ex).MessageExtended));
                    }
                }
            }
        }

        /// <summary>
        /// Initie dans la trace des durée de traitement l'étape {pSetp} pour la clé {pKey}
        /// </summary>
        /// <param name="pStep">Exemple: UTICALCULATION</param>
        /// <param name="pKey">Exemple: (IDT:20200)</param>
        /// FI 20180314 [XXXXX] Add
        public virtual void TraceTimeBegin(string pStep, string pKey)
        {
            TraceTimeBegin(pStep, pKey, String.Empty);
        }

        /// <summary>
        /// Insère dans la trace des durée de traitement la durée de l'étape {pSetp} pour la clé {pKey}
        /// </summary>
        /// <param name="pStep">Exemple: UTICALCULATION</param>
        /// <param name="pKey">Exemple: (IDT:20200)</param>
        /// FI 20180314 [XXXXX] Add
        public virtual void TraceTimeEnd(string pStep, string pKey)
        {
            if (this.SpheresTraceTime != null)
            {
                lock (this.SpheresTraceTime)
                {
                    try
                    {
                        string key = StrFunc.AppendFormat("{0}[@key={1}]", pStep, pKey);

                        if (null == dicTraceTime)
                        {
                            throw new NullReferenceException("dictionnary dicAudit is null");
                        }
                        if (false == dicTraceTime.ContainsKey(key))
                        {
                            throw new NullReferenceException(StrFunc.AppendFormat("key: {0} doestn't exists", key));
                        }
                        dicTraceTime[key].dtEnd = DateTime.Now;

                        TimeSpan timeSpan = dicTraceTime[key].GetTimeSpan();
                        if (StrFunc.IsFilled(dicTraceTime[key].description))
                        {
                            SpheresTraceTime.TraceEvent(TraceEventType.Verbose, 0, "STEP:{0},DESC:{1},KEY:{2},DET DURATION:{3}", pStep, dicTraceTime[key], pKey, timeSpan.ToString());
                        }
                        else
                        {
                            SpheresTraceTime.TraceEvent(TraceEventType.Verbose, 0, "STEP:{0},KEY:{1},DET DURATION:{2}", pStep, pKey, timeSpan.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        // FI 20200910 [XXXXX] Appel à MessageExtended
                        Trace(this, new TraceEvent(TraceEventType.Error, SpheresExceptionParser.GetSpheresException("Exception during TraceTimeEnd", ex).MessageExtended));
                    }
                }
            }
        }

        /// <summary>
        /// <para>Insère dans la trade des durée de traitement les statistiques pour l'étape {pStep} (TOT, SUM, MIN, MAX)</para>
        /// <para>TOT => Temps total constaté pour l'étape {pStep}</para>
        /// <para>SUM => somme des temps constaté unitairement pour l'étape {pStep} (SUM=TOT si traitement synchrone)</para>
        /// <para>MIN => Temps minimum constaté sur l'étape {pStep}</para>
        /// <para>MAX => Temps maximum constaté pour l'étape {pStep}</para>
        /// </summary>
        /// <param name="pStep"></param>
        /// <param name="pDescription"></param>
        /// FI 20180314 [XXXXX] Add
        public virtual void TraceTimeSummary(string pStep, string pDescription)
        {
            if (this.SpheresTraceTime != null)
            {
                lock (this.SpheresTraceTime)
                {
                    try
                    {
                        List<string> lstKey = new List<string>();

                        if (null != dicTraceTime)
                        {
                            lstKey = (from item in dicTraceTime.Keys.Where(x => x.StartsWith(StrFunc.AppendFormat("{0}[", pStep)))
                                      select item).ToList();
                        }

                        if (lstKey.Count > 0)
                        {
                            List<string> unvalidKey = (from item in lstKey.Where(x => dicTraceTime[x].dtStart == DateTime.MinValue ||
                                                                                     dicTraceTime[x].dtEnd == DateTime.MinValue)
                                                       select item).ToList();

                            if (unvalidKey.Count > 0)
                            {
                                throw new Exception(StrFunc.AppendFormat("key:{0} => dtStart or dtEnd is empty", unvalidKey.First()));
                            }
                            /* TOT */
                            DateTime dtMin = (from item in lstKey
                                              select dicTraceTime[item].dtStart).Min();
                            DateTime dtMax = (from item in lstKey
                                              select dicTraceTime[item].dtEnd).Max();
                            TimeSpan timeSpan = (dtMax - dtMin);
                            if (StrFunc.IsFilled(pDescription))
                            {
                                SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},DESC:{1},TOT DURATION:{2}", pStep, pDescription, timeSpan.ToString());
                            }
                            else
                            {
                                SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},TOT DURATION:{1}", pStep, timeSpan.ToString());
                            }
                            if (lstKey.Count > 1)
                            {
                                /* SUM */
                                TimeSpan sum = (from item in lstKey
                                                select dicTraceTime[item].GetTimeSpan()).Aggregate(new TimeSpan(0), (p, v) => p.Add(v));
                                if (StrFunc.IsFilled(pDescription))
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},DESC:{1},SUM DURATION:{2},", pStep, pDescription, sum.ToString());
                                }
                                else
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},SUM DURATION:{1}", pStep, sum.ToString());
                                }
                                /* AVG */
                                double avgTicks = (from item in lstKey
                                                   select dicTraceTime[item].GetTimeSpan().Ticks).Average();

                                TimeSpan avg = new TimeSpan((long)avgTicks);
                                if (StrFunc.IsFilled(pDescription))
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},DESC:{1},AVG DURATION:{2},", pStep, pDescription, avg.ToString());
                                }
                                else
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},AVG DURATION:{1}", pStep, avg.ToString());
                                }
                                /* MIN */
                                TimeSpan min = (from item in lstKey
                                                select dicTraceTime[item].GetTimeSpan()).Min();
                                if (StrFunc.IsFilled(pDescription))
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},DESC:{1},MIN DURATION:{2}", pStep, pDescription, min.ToString());
                                }
                                else
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},MIN DURATION:{1}", pStep, min.ToString());
                                }
                                /* MAX */
                                TimeSpan max = (from item in lstKey
                                                select dicTraceTime[item].GetTimeSpan()).Max();
                                if (StrFunc.IsFilled(pDescription))
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},DESC:{1},MAX DURATION:{2},", pStep, pDescription, max.ToString());
                                }
                                else
                                {
                                    SpheresTraceTime.TraceEvent(TraceEventType.Information, 0, "STEP:{0},MAX DURATION:{1}", pStep, max.ToString());
                                }
                            }

                            /*Remove Entries in dictionnay */
                            foreach (string item in lstKey)
                            {
                                dicTraceTime.Remove(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // FI 20200910 [XXXXX] Appel à MessageExtended
                        Trace(this, new TraceEvent(TraceEventType.Error, SpheresExceptionParser.GetSpheresException("Exception during TraceTimeSummary", ex).MessageExtended));
                    }
                }
            }
        }

        /// <summary>
        /// <para>Insère dans la trade des durée de traitement les statistiques des étapes présentes (TOT, SUM, MIN, MAX)</para>
        /// </summary>
        /// <param name="pDescription"></param>
        public virtual void TraceTimeSummaryAll(string pDescription)
        {
            if (this.SpheresTraceTime != null)
            {
                lock (this.SpheresTraceTime)
                {
                    try
                    {
                        List<string> lstKey = new List<string>();

                        if (null != dicTraceTime)
                        {
                            lstKey = (from item in dicTraceTime.OrderBy(x => x.Value.dtStart)
                                      select StrFunc.Before(item.Key, "[")).Distinct().ToList();
                        }

                        foreach (string item in lstKey)
                        {
                            TraceTimeSummary(item, pDescription);
                        }
                    }
                    catch (Exception ex)
                    {
                        // FI 20200910 [XXXXX] Appel à MessageExtended
                        Trace(this, new TraceEvent(TraceEventType.Error, SpheresExceptionParser.GetSpheresException("Exception during TraceTimeSummaryAll", ex).MessageExtended));
                    }
                }
            }
        }

        /// <summary>
        /// <para>Insère dans la trace des durée de traitement les statistiques pour l'étape {pStep} (TOT, SUM, MIN, MAX)</para>
        /// <para>TOT => Temps total constaté pour l'étape {pStep}</para>
        /// <para>SUM => somme des temps constaté unitairement pour l'étape {pStep} (SUM=TOT si traitement synchrone)</para>
        /// <para>MIN => Temps minimum constaté sur l'étape {pStep}</para>
        /// <para>MAX => Temps maximum constaté pour l'étape {pStep}</para>
        /// </summary>
        /// <param name="pStep"></param>
        /// FI 20180314 [XXXXX] Add
        public virtual void TraceTimeSummary(string pStep)
        {
            TraceTimeSummary(pStep, string.Empty);
        }

        /// <summary>
        ///  Purge 
        /// </summary>
        /// FI 20180314 [XXXXX] Add
        public void TraceTimeReset()
        {
            dicTraceTime = null;
        }


        #endregion Methods
    }
}
