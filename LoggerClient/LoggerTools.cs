using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using EFS.ACommon;

namespace EFS.LoggerClient.LoggerService
{
     /// <summary>
    /// Classe de méthodes utiles pour le Logger
    /// </summary>
    public static class LoggerTools
    {
        #region Methods
        /// <summary>
        /// Obtient un LogLevel en fonction du statut d'un process
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pDefaultLogLevel">Valeur de retour par defaut</param>
        /// <returns></returns>
        // PM 20200909 [XXXXX] Ajout paramètre pDefaultLogLevel (valeur de retour par defaut)
        // EG 20220221 [XXXXX] Add LogLevel for IRQ status
        public static LogLevelEnum StatusToLogLevelEnum(EFS.ACommon.ProcessStateTools.StatusEnum pStatus, LogLevelEnum pDefaultLogLevel = LogLevelEnum.Info)
        {
            LogLevelEnum logLevel;
            switch (pStatus)
            {
                case EFS.ACommon.ProcessStateTools.StatusEnum.ERROR:
                    logLevel = LogLevelEnum.Error;
                    break;
                case EFS.ACommon.ProcessStateTools.StatusEnum.WARNING:
                case EFS.ACommon.ProcessStateTools.StatusEnum.IRQ:
                    logLevel = LogLevelEnum.Warning;
                    break;
                default:
                    logLevel = pDefaultLogLevel;
                    break;
            }
            return logLevel;
        }

        /// <summary>
        /// Obtient un LogLevel en fonction du statut d'un process
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static LogLevelEnum StatusToLogLevelEnum(EFS.SpheresRiskPerformance.CashBalance.ControlEODLogStatus pStatus)
        {
            LogLevelEnum logLevel;
            switch (pStatus)
            {
                case EFS.SpheresRiskPerformance.CashBalance.ControlEODLogStatus.ERROR:
                    logLevel = LogLevelEnum.Error;
                    break;
                case EFS.SpheresRiskPerformance.CashBalance.ControlEODLogStatus.WARNING:
                    logLevel = LogLevelEnum.Warning;
                    break;
                case EFS.SpheresRiskPerformance.CashBalance.ControlEODLogStatus.INFO:
                default:
                    logLevel = LogLevelEnum.Info;
                    break;
            }
            return logLevel;
        }

        /// <summary>
        /// Construit un ensemble de LogParam à partir d'un ensemble de string
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static IEnumerable<LogParam> LogParamFromString(IEnumerable<string> pData)
        {
            IEnumerable<LogParam> logParam;
            if (pData != default(string[]))
            {
                logParam = pData.Select(s => new LogParam(s));
            }
            else
            {
                logParam = default;
            }
            return logParam;
        }
        #endregion Methods
    }

    /// <summary>
    /// Données pour SYSTEMMSG
    /// </summary>
    public partial class SysMsgCode
    {
        #region Accessors
        /// <summary>
        /// Codes pour SYSTEMMSG sous forme de string pour compatibilité
        /// </summary>
        public string MessageCode
        { get { return String.Format("{0}-{1:D5}", SysCode.ToString(), SysNumber); } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pSysCode">Code du message</param>
        /// <param name="pSysNumber">Numéro du message</param>
        public SysMsgCode(SysCodeEnum pSysCode, int pSysNumber)
        {
            SysCode = pSysCode;
            SysNumber = pSysNumber;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Données d'un élément du journal
    /// </summary>
    public partial class LoggerData
    {
        #region Members
        private readonly List<LogParam> m_ParamList = new List<LogParam>();
        #endregion Members

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        public LoggerData()
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLogLevel">Niveau d'importance du log</param>
        /// <param name="pSysMsg">SysCode et SysNumber de SYSTEMMSG</param>
        /// <param name="pRankOrder">Niveau hiérarchique du log</param>
        /// <param name="pData">Paramètre du message de log</param>
        public LoggerData(LogLevelEnum pLogLevel, SysMsgCode pSysMsg, int pRankOrder = 0, IEnumerable<LogParam> pData = default)
        {
            LogLevel = pLogLevel;
            SysMsg = pSysMsg;
            RankOrder = pRankOrder;
            if (pData != default(IEnumerable<LogParam>))
            {
                Parameters = pData.ToList();
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLogLevel">Niveau d'importance du log</param>
        /// <param name="pSysMsg">SysCode et SysNumber de SYSTEMMSG</param>
        /// <param name="pRankOrder">Niveau hiérarchique du log</param>
        /// <param name="pData">Paramètre du message de log</param>
        public LoggerData(LogLevelEnum pLogLevel, SysMsgCode pSysMsg, int pRankOrder = 0, params LogParam[] pData)
        {
            LogLevel = pLogLevel;
            SysMsg = pSysMsg;
            RankOrder = pRankOrder;
            if (pData != default(LogParam[]))
            {
                Parameters = pData.ToList();
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLogLevel">Niveau d'importance du log</param>
        /// <param name="pMessage">Message du log</param>
        /// <param name="pRankOrder">>Niveau hiérarchique du log</param>
        /// <param name="pData">Paramètre du message de log</param>
        public LoggerData(LogLevelEnum pLogLevel, string pMessage, int pRankOrder = 0, IEnumerable<LogParam> pData = default)
        {
            LogLevel = pLogLevel;
            Message = pMessage;
            RankOrder = pRankOrder;
            if (pData != default(IEnumerable<LogParam>))
            {
                Parameters = pData.ToList();
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLogLevel">Niveau d'importance du log</param>
        /// <param name="pMessage">Message du log</param>
        /// <param name="pRankOrder">>Niveau hiérarchique du log</param>
        /// <param name="pData">Paramètre du message de log</param>
        public LoggerData(LogLevelEnum pLogLevel, string pMessage, int pRankOrder = 0, params LogParam[] pData)
        {
            LogLevel = pLogLevel;
            Message = pMessage;
            RankOrder = pRankOrder;
            if (pData != default(LogParam[]))
            {
                Parameters = pData.ToList();
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pSpheresEx">Exception</param>
        /// <param name="pData">Paramètre du message de log</param>
        /// FI 20220719 [XXXXX] Refactoring (Alimentation possible de Message et de SysMsg)
        public LoggerData(SpheresException2 pSpheresEx, params LogParam[] pData)
        {
            /* FI 20220719 : Explications 
                Si pSpheresEx.Message contient un code (ie "SYS-03469") (cas d'une exception gérée)   
                    => Le log de Spheres affiche le message correspondant au code 
                    => La trace de Spheres affiche le message correspondant au code + le Message complet de l'exception pour audit (propriété MessageExtended)
                
                sinon pSpheresEx.Message contient un message (cas d'une exception non gérée)   
                    => Le log de Spheres affiche le message complet de l'exception 
                    => La trace de Spheres affiche le Message complet de l'exception pour audit (propriété MessageExtended)
            */

            if (pSpheresEx != default(EFS.ACommon.SpheresException2))
            {
                LogLevel = LoggerTools.StatusToLogLevelEnum(pSpheresEx.ProcessState.Status);

                Message = pSpheresEx.MessageExtended;

                RankOrder = pSpheresEx.LevelOrder;

                if (ArrFunc.IsFilled(pSpheresEx.ParamData))
                    Parameters = (pSpheresEx.ParamData).Where(x => StrFunc.IsFilled(x)).Select(s => new LogParam(s)).ToList();

                if ((pData != default(LogParam[])) && (pData.Count() > 0))
                    Parameters = Parameters.Concat(pData).ToList();

                // PM 20210503 [XXXXX] Gestion message SysCode passé en ajout dans la string Message 
                Match matchCode = Cst.Regex_CodeNumber.Match(pSpheresEx.Message);
                if (matchCode.Success)
                {
                    string code = matchCode.Groups[1].Value;
                    if (code != "ORA")
                    {
                        int number = Convert.ToInt32(matchCode.Groups[2].Value);
                        Enum.TryParse<SysCodeEnum>(code, out SysCodeEnum sysCode);

                        SysMsg = new SysMsgCode(sysCode, number);
                    }
                }
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajout de paramètres à l'élément du journal
        /// </summary>
        /// <param name="pParam"></param>
        public void AddParam(LogParam pParam)
        {
            if (pParam != default(LogParam))
            {
                m_ParamList.Add(pParam);
                Parameters = m_ParamList.ToList();
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Données concernant les Identifiants internes de référentiels ou paramètres de messages de journaux
    /// </summary>
    public partial class LogParam
    {
        #region Constructors
        /// <summary>
        /// Création d'un paramètre texte de log
        /// </summary>
        /// <param name="pData"></param>
        public LogParam(object pData) : this(pData != default ? pData.ToString() : default) { }

        /// <summary>
        /// Création d'un paramètre texte de log
        /// </summary>
        /// <param name="pData"></param>
        public LogParam(string pData)
        {
            IsReferentialIdentifier = false;
            Data = pData;
            // PM 20201126 [XXXXX] Autoriser les paramètres chaine vide
            //DataSpecified = (pData != default) && (pData.Length > 0);
            DataSpecified = (pData != default);
        }
        
        /// <summary>
        /// Création d'un paramètre de log pour un élément d'un référentiel
        /// </summary>
        /// <param name="pId">Identifiant interne dans le référentiel</param>
        /// <param name="pReferential">Nom du référentiel</param>
        public LogParam(int pId, string pReferential) : this(pId, default, pReferential, default(string)) { }

        /// <summary>
        /// Création d'un paramètre de log pour un élément d'un référentiel
        /// </summary>
        /// <param name="pId">Identifiant interne dans le référentiel</param>
        /// <param name="pIdentifier">Identifiant dans le référentiel</param>
        /// <param name="pReferential">Nom du référentiel</param>
        public LogParam(int pId, string pIdentifier, string pReferential) : this(pId, pIdentifier, pReferential, default(string)) { }

        /// <summary>
        /// Création d'un paramètre de log pour un élément d'un référentiel
        /// </summary>
        /// <param name="pId">Identifiant interne dans le référentiel</param>
        /// <param name="pIdentifier">Identifiant dans le référentiel</param>
        /// <param name="pReferential">Nom du référentiel</param>
        /// <param name="pLink">Lien vers le référentiel ou mot clé</param>
        public LogParam(int pId, string pIdentifier, string pReferential, Cst.LoggerParameterLink pLink) : this(pId, pIdentifier, pReferential, pLink.ToString()) { }

        /// <summary>
        /// Création d'un paramètre de log pour un élément d'un référentiel
        /// </summary>
        /// <param name="pId">Identifiant interne dans le référentiel</param>
        /// <param name="pIdentifier">Identifiant dans le référentiel</param>
        /// <param name="pReferential">Nom du référentiel</param>
        /// <param name="pLink">Lien vers le référentiel ou mot clé</param>
        public LogParam(int pId, string pIdentifier, string pReferential, string pLink)
        {
            IsReferentialIdentifier = true;
            InternalId = pId;
            Identifier = pIdentifier;
            Referential = pReferential;
            Link = pLink;
            DataSpecified = false;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Données d'identification d'un ensemble de log
    /// </summary>
    public partial class LogScope : IEquatable<LogScope>
    {
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        public LogScope()
        {
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdTRK_L"></param>
        /// <param name="pIdProcess"></param>
        public LogScope(string pProcessType, string pConnectionString, int pIdTRK_L = 0, int pIdProcess = 0) : this()
        {
            LogScopeId = Guid.NewGuid();
            SubLogScopeId = LogScopeId;
            ProcessType = pProcessType;
            InitialProcessType = pProcessType;
            ConnectionString = pConnectionString;
            IdTRK_L = pIdTRK_L;
            IdPROCESS_L = pIdProcess;
            IdStProcess = ProcessStateTools.StatusEnum.NONE.ToString();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        ///  Méthode Equals d'objet
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public override bool Equals(object pObj)
        {
            if ((pObj == null) || (GetType() != pObj.GetType()))
            {
                return false;
            }
            else
            {
                LogScope objAsLogScope = pObj as LogScope;
                return Equals(objAsLogScope);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return LogScopeId.GetHashCode();
        }

        /// <summary>
        /// Opérateur égal de LogScope
        /// </summary>
        /// <param name="pObjA"></param>
        /// <param name="pObjB"></param>
        /// <returns></returns>
        public static bool operator== (LogScope pObjA, LogScope pObjB)
        {
            if ((object)pObjA == default)
            {
                return (object)pObjB == default;
            }
            else
            {
                return pObjA.Equals(pObjB);
            }
        }

        /// <summary>
        /// Opérateur différent de LogScope
        /// </summary>
        /// <param name="pObjA"></param>
        /// <param name="pObjB"></param>
        /// <returns></returns>
        public static bool operator !=(LogScope pObjA, LogScope pObjB)
        {
            if ((object)pObjA == default)
            {
                return !((object)pObjB == default);
            }
            else
            {
                return !pObjA.Equals(pObjB);
            } 
        }
        
        /// <summary>
        /// Méthode Equals de LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public bool Equals(LogScope pScope)
        {
            if ((object)pScope == default)
            {
                return false;
            }
            else
            {
                return (LogScopeId.Equals(pScope.LogScopeId));
            }
        }
        #endregion Methods
    }
}
