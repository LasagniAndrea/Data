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
        /// Repr�sente le niveau de s�v�rit� de l'erreur
        /// </summary>
        private ErrorLogSeverityEnum _SeverityEnum;
        /// <summary>
        /// Repr�sente m�thode o� Spheres� a d�rect� l'erreur
        /// </summary>
        private string _Method = string.Empty;
        /// <summary>
        /// Repr�sente l'objet qui est � l'origine de l'erreur
        /// </summary>
        private string _Source = string.Empty;
        /// <summary>
        /// Repr�sente le Message d'erreur
        /// </summary>
        private string _Message = string.Empty;
        /// <summary>
        /// Repr�sente la session Spheres� qui a g�n�r� l'erreur
        /// </summary>
        private AppSession _session = null; 
        /// <summary>
        /// Repr�sente la page demand�e
        /// </summary>
        private string _URL = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        private DateTime _DtError;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Repr�sente la m�thode o� Spheres� a d�rect� l'erreur
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }
        /// <summary>
        /// Repr�sente le Message d'erreur
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        /// <summary>
        /// Repr�sente l'objet qui est � l'origine de l'erreur
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }
        /// <summary>
        /// Repr�sente le niveau de s�v�rit� de l'erreur
        /// </summary>
        public ErrorLogSeverityEnum Severity
        {
            get { return _SeverityEnum; }
            set { _SeverityEnum = value; }
        }
        /// <summary>
        /// Repr�sente l'instance Spheres� qui a g�n�r� l'erreur
        /// </summary>
        public AppSession Session
        {
            get { return _session; }
            set { _session = value; }
        }
        /// <summary>
        /// Repr�sente la page demand�e
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
        /// FI 20200910 [XXXXX] Appel � ExceptionTools.GetMessageExtended
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
        /// FI 20200910 [XXXXX] Appel � ExceptionTools.GetMessageExtended
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
