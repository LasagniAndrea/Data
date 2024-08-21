using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace EFS.Common.Log
{

    /// <summary>
    /// SpheresTraceSource
    /// </summary>
    /// PM 20160104 [POC MUREX] Add
    /// EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresTraceSource : TraceSource
    {
        #region Members
        /// <summary>
        /// Durée limite pour exécution d'une requête (valeur en seconde)
        /// <para>Au dela de cette durée, un warning est inscrit dans le journal</para>
        /// <para>si la valeur -1 => aucun warning, valeur par défaut 120 secondes</para>
        /// </summary>
        private static readonly int m_SqlDurationLimit = 120;

        /// <summary>
        /// Nom de l'attibut commutateur personnalisé dédié à l'inscription d'un warning dans la trace lorsque la durée (secondes) spécifiée est dépassé
        /// </summary>
        private const string m_SqlDurationLimitAttrib = "sqlDurationLimit";
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la durée au dela de laquelle une insruction sql doit générer un warning dans la trace
        /// <para>-1 aucune limite</para>
        /// </summary>
        public int SqlDurationLimit
        {
            get
            {
                int ret = m_SqlDurationLimit;
                if (StrFunc.IsFilled(this.Attributes[m_SqlDurationLimitAttrib]))
                {
                    ret = Convert.ToInt32(this.Attributes[m_SqlDurationLimitAttrib]);
                }
                return ret;
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Initialise une nouvelle instance de la classe SpheresTraceSource
        /// </summary>
        /// <param name="pName">Nom de la source</param>
        public SpheresTraceSource(string pName) :
            base(pName)
        {
        }
      
        #endregion Constructors

        #region Methods
        /// <summary>
        ///  Retourne les attributs de commutateur personnalisés définis dans le fichier de configuration de l'application.
        /// </summary>
        /// <returns></returns>
        protected override string[] GetSupportedAttributes()
        {

            return new string[] { m_SqlDurationLimitAttrib };
        }

        /// <summary>
        ///  Retourne le(s) fichier(s) de trace associés à la source
        /// </summary>
        /// <returns></returns>
        public string[] GetTraceFile()
        {
            string[] ret = null;

            TraceListener[] traceListernerArray = new TraceListener[this.Listeners.Count];
            this.Listeners.CopyTo(traceListernerArray, 0);

            IEnumerable<TextWriterTraceListener> traceLst = traceListernerArray.Where(x => x is TextWriterTraceListener).Select(x => x as TextWriterTraceListener);
            if (traceLst.Count() > 0)
            {
                ret = (from item in traceLst.Where(x => (null != GetFilePathFromTraceListener(x)))
                       select GetFilePathFromTraceListener(item)).ToArray();
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        private static string GetFilePathFromTraceListener(TextWriterTraceListener listener)
        {
            // Check if the listener's Writer is a StreamWriter
            if (listener.Writer is StreamWriter streamWriter)
            {
                // Access the BaseStream property of StreamWriter to get the underlying FileStream
                if (streamWriter.BaseStream is FileStream fileStream)
                {
                    // Retrieve the file path from the FileStream
                    return fileStream.Name;
                }
            }

            // If the underlying Writer is not a StreamWriter or FileStream, return null or handle accordingly
            return null;
        }


        #endregion Methods
    }

}
