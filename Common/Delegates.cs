using System;

using EFS.ACommon;
using EFS.Common.Log;
using EFS.Process;
using EFS.ApplicationBlocks.Data;
using EFS.LoggerClient.LoggerService;

namespace EFS.Common
{
    /// <summary>
    /// Delegate method implementing log recording
    /// </summary>
    /// <param name="logLevel">Niveau de log</param>
    /// <param name="pMessage">Log message, not null</param>
    /// <param name="pDatas">Additional datas</param>
    /// FI 20240111 [WI793] Add
    public delegate void LogAddDetail(LogLevelEnum logLevel, string pMessage, int pRankOrder = 0,  string[] pDatas = default);

    /// <summary>
    /// Delegate method implementing doc log recording
    /// </summary>
    /// <param name="pCS"></param>
    /// <param name="pIdProcess"></param>
    /// <param name="pIdA"></param>
    /// <param name="pData"></param>
    /// <param name="pName"></param>
    /// <param name="pDocType"></param>
    public delegate void AddAttachedDoc(string pCS, int pIdProcess, int pIdA, byte[] pData, string pName, string pDocType);

    /// <summary>
    /// Delegue qui permet de mettre à jour le status du process appelant en error ou warning
    /// </summary>
    /// <param name="pStatus"></param>
    /// FI 20200623 [XXXXX] Add
    public delegate void SetErrorWarning(ProcessStateTools.StatusEnum pStatus);

    /// <summary>
    /// Delegue qui permet d'ajouter une exception au process appelant  (le process appelant en fait ce qu'il en veut)
    /// </summary>
    /// <param name="pEx"></param>
    /// FI 20200623 [XXXXX] Add
    public delegate void AddCriticalException(Exception pEx);

}
