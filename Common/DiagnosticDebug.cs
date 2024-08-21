using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon; 

namespace EFS.Common
{
    /// <summary>
    /// Outils de diagnostique
    /// </summary>
    public class DiagnosticDebug
    {
        /// <summary>
        /// Activer la trace des différentes étapes du traitement
        /// </summary>
        public bool IsTraceSteps = true;
        private static TimerCollection m_Timers;

        public DiagnosticDebug(bool pIsTraceSteps)
        {
            IsTraceSteps = pIsTraceSteps;
            m_Timers = new TimerCollection();
        }

        /// <summary>
        /// Début de la trace, avec création d'un timer
        /// </summary>
        /// <param name="pName"></param>
        public void Start(string pName)
        {
            Start(pName, string.Empty);
        }
        /// <summary>
        /// Début de la trace, avec création d'un timer et écriture d'un Header avec le nom de l'étape
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pHeader"></param>
        public void Start(string pName, string pHeader)
        {
            if (IsTraceSteps)
            {
                m_Timers.CreateTimer(pName, true);

                if (StrFunc.IsFilled(pHeader))
                    Write(pName + pHeader);
            }
        }
        /// <summary>
        /// Fin de la trace, avec écriture du temps écoulé
        /// </summary>
        /// <param name="pName"></param>
        public void End(string pName)
        {
            End(pName, string.Empty);
        }
        /// <summary>
        /// Fin de la trace, avec écriture du temps écoulé et écriture d'un footer
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pFooter"></param>
        public void End(string pName, string pFooter)
        {
            if (IsTraceSteps)
            {
                System.Diagnostics.Debug.WriteLine(m_Timers.GetElapsedTime(pName, "sec", false));

                if (StrFunc.IsFilled(pFooter))
                    Write(pFooter);
            }
        }
        /// <summary>
        /// Ecriture d'un commentaire
        /// </summary>
        /// <param name="pComment"></param>
        public void Write(string pComment)
        {
            if (IsTraceSteps)
                System.Diagnostics.Debug.WriteLine(pComment);
        }
    }
}
