using System;

namespace EFS.Common.Log
{
    /// <summary>
    /// Niveau de sévérité de l'erreur
    /// </summary>
    public enum ErrorLogSeverityEnum
    {
        VERY_LOW = 1,
        LOW = 2,
        MEDIUM = 3,
        HIGH = 4,
        VERY_HIGH = 5
    };

    /// <summary>
    /// Données de l'erreur
    /// </summary>
    public class ErrorLogInfo
    {
        #region Members
        /// <summary>
        /// Représente le niveau de sévérité de l'erreur
        /// </summary>
        private ErrorLogSeverityEnum m_SeverityEnum;
        /// <summary>
        /// Représente méthode où Spheres® a dérecté l'erreur
        /// </summary>
        private string m_Method = string.Empty;
        /// <summary>
        /// Représente l'objet qui est à l'origine de l'erreur
        /// </summary>
        private string m_Source = string.Empty;
        /// <summary>
        /// Représente le Message d'erreur
        /// </summary>
        private string m_Message = string.Empty;
        /// <summary>
        /// Représente la page demandée
        /// </summary>
        private string m_URL = string.Empty;
        /// <summary>
        /// Horaire de génération de l'erreur
        /// </summary>
        private DateTime m_DtError;
        /// <summary>
        /// Nom de l'application associé à l'instance
        /// </summary>
        private string m_AppName = string.Empty;
        /// <summary>
        /// N° de version de l'application 
        /// </summary>
        private string m_AppVersion = string.Empty;
        /// <summary>
        /// Identifiant de l'acteur qui a lancé l'instance
        /// </summary>
        private string m_IdA_Identifier = string.Empty;
        /// <summary>
        /// Uniquement sur application web
        /// </summary>
        private string m_BrowserInfo = string.Empty;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Représente la méthode où Spheres® a dérecté l'erreur
        /// </summary>
        public string Method
        {
            get { return m_Method; }
            set { m_Method = value; }
        }
        /// <summary>
        /// Représente le Message d'erreur
        /// </summary>
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }
        /// <summary>
        /// Représente l'objet qui est à l'origine de l'erreur
        /// </summary>
        public string Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        }
        /// <summary>
        /// Représente le niveau de sévérité de l'erreur
        /// </summary>
        public ErrorLogSeverityEnum Severity
        {
            get { return m_SeverityEnum; }
            set { m_SeverityEnum = value; }
        }
        /// <summary>
        /// Représente la page demandée
        /// </summary>
        public string URL
        {
            get { return m_URL; }
            set { m_URL = value; }
        }
        /// <summary>
        /// Horaire de génération de l'erreur
        /// </summary>
        public DateTime DtError
        {
            get { return m_DtError; }
            set { m_DtError = value; }
        }
        /// <summary>
        /// Nom de l'application associé à l'instance
        /// </summary>
        public string AppName
        {
            get { return m_AppName; }
            set { m_AppName = value; }
        }
        /// <summary>
        /// N° de version de l'application 
        /// </summary>
        public string AppVersion
        {
            get { return m_AppVersion; }
            set { m_AppVersion = value; }
        }
        /// <summary>
        /// Identifiant de l'acteur qui a lancé l'instance
        /// </summary>
        public string IdA_Identifier
        {
            get { return m_IdA_Identifier; }
            set { m_IdA_Identifier = value; }
        }
        /// <summary>
        /// Uniquement sur application web
        /// </summary>
        public string BrowserInfo
        {
            get { return m_BrowserInfo; }
            set { m_BrowserInfo = value; }
        }
        /// <summary>
        /// Nom de machine qui exécute l'instance
        /// </summary>
        public string HostName
        {
            get { return System.Environment.MachineName; }
        }
        #endregion Accessors
    }
}
