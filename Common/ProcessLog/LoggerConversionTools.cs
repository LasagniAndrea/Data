using System.Collections.Generic;
//
using EFS.ACommon;
using EFS.LoggerClient.LoggerService;

namespace EFS.Common.Log
{
    /// <summary>
    /// 
    /// </summary>
    // PM 20200102 [XXXXX] New Log
    public class LoggerConversionTools
    {
        #region Methods
        /// <summary>
        /// Conversion de <see cref="LogLevelDetail"/> en L<see cref="LogLevelEnum"/>
        /// </summary>
        /// <param name="logDetailEnum"></param>
        /// <returns></returns>
        public static LogLevelEnum DetailEnumToLogLevelEnum(LogLevelDetail logDetailEnum)
        {
            LogLevelEnum logLevel = LogLevelEnum.Trace;
            switch (logDetailEnum)
            {
                case LogLevelDetail.LEVEL1:
                    logLevel = LogLevelEnum.Error;
                    break;
                case LogLevelDetail.LEVEL2:   
                    logLevel = LogLevelEnum.Warning;
                    break;
                case LogLevelDetail.LEVEL3:
                    logLevel = LogLevelEnum.Info;
                    break;
                case LogLevelDetail.LEVEL4:
                    logLevel = LogLevelEnum.Debug;
                    break;
                case LogLevelDetail.LEVEL5:   
                    logLevel = LogLevelEnum.Trace;
                    break;
            }
            return logLevel;
        }

        /// <summary>
        /// Conversion de <see cref="ProcessLogInfo"/> en <see cref="LoggerData"/>
        /// </summary>
        /// <param name="pLogInfo"></param>
        /// <returns></returns>
        public static LoggerData ProcessLogInfoToLoggerData(ProcessLogInfo pLogInfo)
        {
            LoggerData loggerData = default;
            if (pLogInfo != default(ProcessLogInfo))
            {
                List<LogParam> logParam = new List<LogParam>();
                if (StrFunc.IsFilled(pLogInfo.idDataIdent) || (pLogInfo.idData > 0))
                {
                    logParam.Add(new LogParam(pLogInfo.idData, default, pLogInfo.idDataIdent, Cst.LoggerParameterLink.IDDATA));
                }
                if (StrFunc.IsFilled(pLogInfo.queueMessage))
                {
                    logParam.Add(new LogParam(0, pLogInfo.queueMessage, default, Cst.LoggerParameterLink.QUEUEMSG));
                }
                #region pLogInfo.data
                if (StrFunc.IsFilled(pLogInfo.data1))
                {
                    logParam.Add(new LogParam(pLogInfo.data1));
                }
                if (StrFunc.IsFilled(pLogInfo.data2))
                {
                    logParam.Add(new LogParam(pLogInfo.data2));
                }
                if (StrFunc.IsFilled(pLogInfo.data3))
                {
                    logParam.Add(new LogParam(pLogInfo.data3));
                }
                if (StrFunc.IsFilled(pLogInfo.data4))
                {
                    logParam.Add(new LogParam(pLogInfo.data4));
                }
                if (StrFunc.IsFilled(pLogInfo.data5))
                {
                    logParam.Add(new LogParam(pLogInfo.data5));
                }
                if (StrFunc.IsFilled(pLogInfo.data6))
                {
                    logParam.Add(new LogParam(pLogInfo.data6));
                }
                if (StrFunc.IsFilled(pLogInfo.data7))
                {
                    logParam.Add(new LogParam(pLogInfo.data7));
                }
                if (StrFunc.IsFilled(pLogInfo.data8))
                {
                    logParam.Add(new LogParam(pLogInfo.data8));
                }
                if (StrFunc.IsFilled(pLogInfo.data9))
                {
                    logParam.Add(new LogParam(pLogInfo.data9));
                }
                if (StrFunc.IsFilled(pLogInfo.data10))
                {
                    logParam.Add(new LogParam(pLogInfo.data1));
                }
                #endregion pLogInfo.data
                if (pLogInfo.SysMsgCode != default(SysMsgCode))
                {
                    loggerData = new LoggerData(StatusToLogLevelEnum(pLogInfo.status), pLogInfo.SysMsgCode, pLogInfo.levelOrder, logParam);
                }
                else
                {
                    loggerData = new LoggerData(StatusToLogLevelEnum(pLogInfo.status), pLogInfo.message, pLogInfo.levelOrder, logParam);
                }
            }
            return loggerData;
        }

        /// <summary>
        /// Conversion d'un Status sous forme de string en LogLevelEnum
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        // PM 20210915 [XXXXX] AzDO 201 : La méthode devient public
        public static LogLevelEnum StatusToLogLevelEnum(string pStatus)
        {
            LogLevelEnum logLevel = LogLevelEnum.Info;
            switch (pStatus)
            {
                case "ERROR":
                    logLevel = LogLevelEnum.Error;
                    break;
                case "WARNING":
                    logLevel = LogLevelEnum.Warning;
                    break;
                case "NA":
                    logLevel = LogLevelEnum.Debug;
                    break;
            }
            return logLevel;
        }
        #endregion Methods
    }
}
