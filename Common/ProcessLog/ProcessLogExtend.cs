using EFS.ACommon;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.Common.Log
{
    /// <summary>
    ///  Gestion de l'écriture des logs via un <see cref="ProcessLog"/> ou un <see cref="LoggerScope"/>
    /// </summary>
    /// FI 20240111 [WI793] Add
    public class ProcessLogExtend
    {

        private readonly ProcessLog _processLog;
        private readonly LoggerScope _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processLog">null accepté</param>
        /// <param name="logger">null accepté</param>
        public ProcessLogExtend(ProcessLog processLog, LoggerScope logger)
        {
            _processLog = processLog;
            _logger = logger;
        }

        /// <summary>
        /// Ajoute un log (avec eciture dans la foulée  appel à <see cref="ProcessLog.SQLWriteDetail"/> ou  <see cref="LoggerScope.Write(pIsForced:true)"/> )
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <param name="pMessage">message de type <see cref="String"/> ou de type <see cref="SysMsgCode"/></param>
        /// <param name="pRankOrder"></param>
        /// <param name="pData"></param>
        public void LogAddDetail<T>(LogLevelEnum pLogLevel, T pMessage, int pRankOrder = 0, string[] pData = null)
        {
            if (pMessage == null)
                throw new ArgumentNullException($"{nameof(pMessage)} is null");

            if (false == (pMessage.GetType().Equals(typeof(string)) || (pMessage.GetType().Equals(typeof(SysMsgCode)))))
                throw new ArgumentException($"{nameof(pMessage)} type ({pMessage.GetType()}) is not valid");

            if (null != _processLog)
            {
                string message = string.Empty;
                if (typeof(T).Equals(typeof(string)))
                    message = pMessage as string;
                else if (typeof(T).Equals(typeof(SysMsgCode)))
                    message = (pMessage as SysMsgCode).MessageCode;

                List<string> loginInfo = new List<string> { message };
                if (ArrFunc.IsFilled(pData))
                    loginInfo.AddRange(pData);

                _processLog.AddDetail(loginInfo.ToArray(), pLogLevel);
                _processLog.SQLWriteDetail();
            }
            else if (null != _logger)
            {
                if (typeof(T).Equals(typeof(string)))
                    _logger.Log(new LoggerData(pLogLevel, pMessage as string, pRankOrder, LoggerTools.LogParamFromString(pData)), true);
                else if (typeof(T).Equals(typeof(SysMsgCode)))
                    _logger.Log(new LoggerData(pLogLevel, pMessage as SysMsgCode, pRankOrder, LoggerTools.LogParamFromString(pData)), true);

                _logger.Write(pIsForced: true);
            }
        }
    }
}
