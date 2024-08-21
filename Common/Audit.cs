using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using EFS.ACommon;

namespace EFS.Common
{
    /// <summary>
    /// Class qui permet auditer le temps d'exécution d'un process 
    /// </summary>
    [System.Xml.Serialization.XmlRoot()]
    public class AuditTime : DatetimeProfiler
    {
        #region members
        private readonly List<AuditTimeStep> stepList;
        #endregion

        #region properties
        [System.Xml.Serialization.XmlAttribute("totalDuration")]
        public string pTotalDuration; //Durée de tous les Step réunis
        [System.Xml.Serialization.XmlAttribute("warningLimit")]
        public int warningLimit = 10; //Valeur du pourcentage du temps d'execution des Step avant WARNING        

        /// <summary>
        /// Liste des étapes enregistrées
        /// </summary>
        [System.Xml.Serialization.XmlElement()]
        public AuditTimeStep[] Step
        {
            get
            {
                AuditTimeStep[] ret = null;
                if (ArrFunc.IsFilled(stepList))
                {
                    ret = new AuditTimeStep[stepList.Count];
                    int i = 0;
                    foreach (AuditTimeStep pair in stepList)
                    {
                        ret[i] = pair;
                        i++;
                    }
                }
                return ret;
            }
            set
            {
                stepList.Clear();
                stepList.AddRange(value);
            }
        }
        #endregion

        #region constructor
        public AuditTime()
            : base(DateTime.Now)
        {
            stepList = new List<AuditTimeStep>();
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// Insertion du Step courant dans la liste
        /// </summary>
        public static void SubStepLevel(List<AuditTimeStep> stepList, string pMethod, Nullable<bool> pIsBegin, string pInformation, string currentTime)
        {
            bool done = false;
            AuditTimeStep lastATS = stepList.Last();
            _ = new List<AuditTimeStep>();

            //Si Dernier Step non terminé
            if (!lastATS.isCompleted)
            {
                if (lastATS.StepSpecified) //S'il a une liste d'enfant
                {
                    _ = lastATS.Step;
                }
                else //S'il n'a pas de la liste d'enfant
                {
                    lastATS.StepSpecified = true;
                    lastATS.Step = new List<AuditTimeStep>
                    {
                        new AuditTimeStep(pMethod, (bool)pIsBegin, pInformation, currentTime)
                    }; //On lui en crée une et on ajoute le Step en cours
                    done = true;
                }
            }
            else //Dernier Step terminé, on ajoute au même niveau
            {
                stepList.Add(new AuditTimeStep(pMethod, (bool)pIsBegin, pInformation, currentTime));
                done = true;
            }

            if (!done) //Traitement des "enfants des enfants"...
            {
                SubStepLevel(lastATS.Step, pMethod, (bool)pIsBegin, pInformation, currentTime); //Appel recursif pour chercher dans les enfants
            }
        }

        /// <summary>
        /// Calcul du temps d'exécution (duration) des Step enfants
        /// </summary>
        public static bool SubStepExecution(AuditTimeStep ats, string pMethod, TimeSpan currentTime)
        {
            bool done = false;
            // AuditTimeStep courant
            if (ats.method == pMethod && ats.duration == null)
            {
                ats.duration = (currentTime - TimeSpan.Parse(ats.start)).ToString();
                ats.isCompleted = true;
                done = true;
            }
            //Si un AuditTimeStep enfant (Step) est présent
            else if (ats.StepSpecified)
            {
                List<AuditTimeStep> lsTemp = new List<AuditTimeStep>();

                foreach (var child in ats.Step)
                {
                    if (child != null)
                    {
                        //Step enfant
                        if (child.method == pMethod && child.duration == null)
                        {
                            child.duration = (currentTime - TimeSpan.Parse(child.start)).ToString();
                            child.isCompleted = true;
                            done = true;
                            break;
                        }
                        //Step introuvable, les Step enfants sont stockés dans une liste temporaire
                        else
                        {
                            lsTemp.Add(child);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //Step introuvable, on cherche au niveau inférieur
                if (!done)
                {
                    foreach (AuditTimeStep child in lsTemp)
                    {
                        done = SubStepExecution(child, pMethod, currentTime);
                    }
                }
            }
            return done;
        }


        public void StartAuditTime()
        {
            Start(DateTime.Now);
        }

        /// <summary>
        /// Ajoute une étape à l'audit.
        /// </summary>
        /// <param name="pLabel"></param>
        public void AddStep(string pMethod)
        {
            string currentTime = GetTimeSpan().ToString();
            //On identifie s'il s'agit d'un Begin/End via le 1er mot de la méthode.
            Nullable<bool> isBegin = null;

            if (pMethod.ToLower().StartsWith("begin") || pMethod.ToLower().StartsWith("start"))
            {
                isBegin = true;
                pMethod = pMethod.Remove(0, 5).TrimStart(' ');
            }
            else if (pMethod.ToLower().StartsWith("end"))
            {
                isBegin = false;
                pMethod = pMethod.Remove(0, 3).TrimStart(' ');
            }

            AddStep(pMethod, isBegin, null, currentTime);
        }
        public void AddStep(string pMethod, Nullable<bool> pIsBegin, string pInformation, string currentTime)
        {
            if (currentTime == null)
            {
                currentTime = GetTimeSpan().ToString();
            }

            if (pIsBegin.HasValue && (stepList.Count > 0)) //Step de niveau 1 ou plus dans la Liste
            {
                #region StepBegin
                if ((bool)pIsBegin)
                {
                    if (stepList.Last().isCompleted) //Si dernier Step de premier niveau terminé, on insére celui ci au même niveau
                    {
                        stepList.Add(new AuditTimeStep(pMethod, pIsBegin, pInformation, currentTime));
                    }
                    else //Step de niveau inférieur...
                    {
                        SubStepLevel(stepList, pMethod, (bool)pIsBegin, pInformation, currentTime);
                    }
                }
                #endregion StepBegin

                #region StepEnd
                else //Step End : Calcul du temps d'execution...
                {
                    bool done = false;

                    //Recherche de l'item "Begin" correspond
                    //Step de premier niveau
                    foreach (AuditTimeStep ats in stepList)
                    {
                        if (ats.method == pMethod && ats.duration == null)
                        {
                            ats.duration = (TimeSpan.Parse(currentTime) - TimeSpan.Parse(ats.start)).ToString();
                            ats.isCompleted = true;
                            done = true;
                            break;
                        }
                    }
                    //Step de niveau inférieur
                    if (!done)
                    {
                        foreach (AuditTimeStep ats in stepList)
                        {
                            bool did = SubStepExecution(ats, pMethod, TimeSpan.Parse(currentTime));
                            if (did)
                            {
                                break;
                            }
                        }
                    }
                }
                #endregion StepEnd
            }

            else //Premier Step
            {
                //Insertion du Premier Step
                stepList.Add(new AuditTimeStep(pMethod, pIsBegin, pInformation, currentTime));
            }
        }

        /// <summary>
        /// Retourne une représentation XML de l'audit
        /// </summary>
        /// <returns></returns>
        public StringBuilder Serialize()
        {
            StringWriterWithEncoding writer = null;
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AuditTime));
                StringBuilder sb = new StringBuilder();
                writer = new StringWriterWithEncoding(sb, Encoding.UTF8);
                xmlSerializer.Serialize(writer, this, ns);
                return sb;
            }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
        }

        /// <summary>
        /// Ecriture dans la sortie du debuggueur de la représentation XML de  l'audit
        /// </summary>
        public void WriteDebug()
        {
            StringBuilder sb = Serialize();
            Debug.Write(sb.ToString());
        }

        /// <summary>
        /// Ecriture dans un fichier de la représentation XML de  l'audit
        /// </summary>
        public void WriteFile(string pPath)
        {
            //Calcul du temps d'exécution de tous les Step & du pourcentage de chaque Step
            TimeSpan totalDuration = TimeSpan.Zero;
            AuditTimeStep lastStep = stepList.Last();
            if (lastStep.isCompleted)
            {
                foreach (AuditTimeStep ats in stepList)
                {
                    totalDuration += TimeSpan.Parse(ats.duration);
                }
                pTotalDuration = totalDuration.ToString();
                CalculPercentDuration(totalDuration.TotalSeconds, stepList);
            }

            //Serialisation
            StringBuilder sb = Serialize();
            FileTools.WriteStringToFile(sb.ToString(), pPath);
        }

        /// <summary>
        /// Calcul du Pourcentage du Temps d'Execution
        /// </summary>
        public void CalculPercentDuration(double totalDuration, List<AuditTimeStep> lsTemp)
        {
            double childDuration = 0;
            foreach (AuditTimeStep child in lsTemp)
            {
                //BD 20130305
                if (child.duration != null)
                {
                    childDuration = (TimeSpan.Parse(child.duration)).TotalSeconds;
                }

                double percent = Math.Round((100 * childDuration / totalDuration), 2, MidpointRounding.AwayFromZero);
                if (percent > warningLimit) //Pourcentage superieure à warningLimit
                {
                    child.percent = percent + "% !WARNING!";
                }
                else //Inférieur ou égal à warningLimit
                {
                    child.percent = percent + "%";
                }

                if (child.StepSpecified) //Si ce Step a des enfants, on s'en occupe aussi
                {
                    CalculPercentDuration(totalDuration, child.Step);
                }
            }
        }
        #endregion Methods

    }

    /// <summary>
    /// Représente une étape lors d'un audit de procédure
    /// </summary>
    public class AuditTimeStep
    {
        const string BEGIN = "begin", END = "end";

        #region members
        /// <summary>
        /// Temps écoulé depuis depuis le début de l'audit
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("start")]
        public string start;
        /// <summary>
        /// Duration en pourcentage
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("percent")]
        public string percent;
        /// <summary>
        /// Durée réel d'exécution
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("duration")]
        public string duration;
        /// <summary>
        /// Nom de baptême de l'étape
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("method")]
        public string method;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string step;
        [System.Xml.Serialization.XmlIgnore()]
        private string m_information;
        [System.Xml.Serialization.XmlIgnore()]
        public bool InformationSpecified;
        [System.Xml.Serialization.XmlElement()]
        public XmlCDataSection Information
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return doc.CreateCDataSection(m_information);
            }
            set
            {
                m_information = value.Value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute()]
        public List<AuditTimeStep> Step;
        [System.Xml.Serialization.XmlIgnore()]
        public bool isCompleted;
        [System.Xml.Serialization.XmlIgnore()]
        public bool StepSpecified;
        #endregion

        #region constructor
        public AuditTimeStep() { } //Nécessaire à la sérialisation
        public AuditTimeStep(string pMethod, Nullable<bool> pIsBegin, string pInformation, string pTimeSpan)
        {
            isCompleted = false;

            start = pTimeSpan;

            if (pIsBegin.HasValue)
            {
                step = (bool)pIsBegin ? BEGIN : END;
            }
            else
            {
                step = "n/a";
            }

            method = pMethod;

            InformationSpecified = !String.IsNullOrEmpty(pInformation);
            m_information = pInformation;
        }
        #endregion

        #region accessors
        public bool IsBegin
        {
            get { return (this.step == BEGIN); }
        }
        public bool IsEnd
        {
            get { return (this.step == END); }
        }
        #endregion accessors
    }
}
