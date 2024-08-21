using System;
//
using EFS.ACommon;

namespace EFS.Common.Log
{
    /// <summary>
    /// Summary description for ErrorBlock.
    /// </summary>
    public class ErrorBlock
    {
        #region Members
        /// <summary>
        /// Représente le niveau de sévérité de l'erreur
        /// </summary>
        private ErrorLogSeverityEnum _SeverityEnum;
        /// <summary>
        /// Représente méthode où Spheres® a dérecté l'erreur
        /// </summary>
        private string _Method = string.Empty;
        /// <summary>
        /// Représente l'objet qui est à l'origine de l'erreur
        /// </summary>
        private string _Source = string.Empty;
        /// <summary>
        /// Représente le Message d'erreur
        /// </summary>
        private string _Message = string.Empty;
        /// <summary>
        /// Représente la session Spheres® qui a généré l'erreur
        /// </summary>
        private AppSession _session = null; 
        /// <summary>
        /// Représente la page demandée
        /// </summary>
        private string _URL = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        private DateTime _DtError;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Représente la méthode où Spheres® a dérecté l'erreur
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }
        /// <summary>
        /// Représente le Message d'erreur
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        /// <summary>
        /// Représente l'objet qui est à l'origine de l'erreur
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }
        /// <summary>
        /// Représente le niveau de sévérité de l'erreur
        /// </summary>
        public ErrorLogSeverityEnum Severity
        {
            get { return _SeverityEnum; }
            set { _SeverityEnum = value; }
        }
        /// <summary>
        /// Représente l'instance Spheres® qui a généré l'erreur
        /// </summary>
        public AppSession Session
        {
            get { return _session; }
            set { _session = value; }
        }
        /// <summary>
        /// Représente la page demandée
        /// </summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }
        /// <summary>
        /// Time Error Occurred
        /// </summary>
        public DateTime DtError
        {
            get { return _DtError; }
            set { _DtError = value; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pSession"></param>
        /// <param name="pURL"></param>
        /// FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
        public ErrorBlock(Exception pEx, AppSession pSession, string pURL)
            : this(ExceptionTools.GetTargetException(pEx), ErrorLogSeverityEnum.HIGH, pEx.Source, ExceptionTools.GetMessageAndStackExtended(pEx), pSession, pURL)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pSession"></param>
        /// <param name="pURL"></param>
        /// FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
        public ErrorBlock(SpheresException2 pEx, AppSession pSession, string pURL)
            : this(pEx.Method, ErrorLogSeverityEnum.HIGH, pEx.Source, ExceptionTools.GetMessageAndStackExtended(pEx), pSession, pURL)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pSeverityEnum"></param>
        /// <param name="pSource"></param>
        /// <param name="pMessage"></param>
        /// <param name="pSession"></param>
        /// <param name="pURL"></param>
        public ErrorBlock(string pMethod, ErrorLogSeverityEnum pSeverityEnum, string pSource, string pMessage, AppSession pSession, string pURL)
        {
            _Method = pMethod;
            _SeverityEnum = pSeverityEnum;
            _Source = pSource;
            _Message = pMessage;
            _session = pSession;
            _URL = pURL;
            // FI 20200813 [XXXXX] UTC
            _DtError = SystemTools.GetOSDateSys().ToUniversalTime();
        }
        #endregion Constructors
    }
}
