using EFS.ACommon;
using EFS.Process;
using System;

namespace EFS.Common.IO.Interface
{
    // EG 20190114 Add detail to ProcessLog Refactoring
    // RD 20190718 Add appInstance
    public interface IIOTaskLaunching
    {
        #region Accessors
        int IdProcess { get; }
        string Cs { get; }
        int UserId { get; }
        string RequesterHostName { get; }
        int RequesterIdA { get; }
        DateTime RequesterDate { get; }
        string RequesterSessionId { get; }
        IOTask IoTask { get; }
        AppSession Session { get; }
        #endregion Accessors

        #region Methods
        void CheckFolderFromFilePath(string pPathFileName, double pTimeout, int pLevelOrder);
        string GetFilepath(string pFilePath);
        string GetTemporaryDirectory(AppSession.AddFolderSessionId pAddFolderSession);
        
        /// <summary>
        /// Ajoute l'exception dans le process appelant 
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20200629 [XXXXX] Add AddException
        void AddCriticalException(Exception pEx);
        /// <summary>
        /// Mise à jour du status du process appelant en error ou warning
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// FI 20200629 [XXXXX] Add AddException
        void SetErrorWarning(ProcessStateTools.StatusEnum pStatusEnum);
        #endregion Methods
    }
}
