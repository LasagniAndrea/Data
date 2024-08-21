using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeInformation;
using System;
using System.Data;

namespace EFS.SpheresRiskPerformance
{
    // EG 20180205 [23769] New
    internal class RiskTemplateInfo
    {
        #region Members
        public CaptureSessionInfo SessionInfo { get; set; }
        public InputUser InputUser { get; set; }
        public int IdTTemplate { get; set; }
        public string ScreenTemplate { get; private set; }
        #endregion Members
        #region Constructor
        public RiskTemplateInfo(IdMenu.Menu pIdMenu, string pProduct, RiskPerformanceProcessBase pProcess)
        {
            User user = new User(pProcess.Session.IdA, null, RoleActor.SYSADMIN);

            //CaptureSessionInfo
            SessionInfo = new CaptureSessionInfo
            {
                user = user,
                session = pProcess.Session,
                licence = pProcess.License,
                idProcess_L = pProcess.IdProcess,
                idTracker_L = pProcess.Tracker.IdTRK_L
            };


            string idMenu = IdMenu.GetIdMenu(pIdMenu);
            InputUser = new InputUser(idMenu, user);
            InputUser.InitializeFromMenu(CSTools.SetCacheOn(pProcess.Cs));
            InputUser.InitCaptureMode();

            SQL_Instrument sqlInstrument = new SQL_Instrument(pProcess.Cs, pProduct, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);
            if (false == sqlInstrument.LoadTable(new string[] { "IDI,IDENTIFIER" }))
                throw new NotSupportedException(StrFunc.AppendFormat("Instrument {0} not found", pProduct));

            SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(sqlInstrument.Id);

            StringData[] data = searchInstrumentGUI.GetDefault(pProcess.Cs, false);
            if (ArrFunc.IsEmpty(data))
                throw new NotSupportedException(StrFunc.AppendFormat("Screen or template not found for Instrument {0}", sqlInstrument.Identifier));
            ScreenTemplate = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;

            string templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;

            IdTTemplate = TradeRDBMSTools.GetTradeIdT(pProcess.Cs, templateIdentifier);
            if (0 == IdTTemplate)
                throw new NotSupportedException("Trade Source not found");
        }
        #endregion Constructor
    }

    internal sealed class RiskPerformanceTools
    {

        /// <summary>
        ///  Record Trade In database
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureGen"></param>
        /// <param name="pRecordSettings"></param>
        /// <param name="pCaptureSession"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pAddCriticalException"></param>
        /// <param name="pLogDetail"></param>
        /// <param name="pAttach"></param>
        /// <returns></returns>
        /// FI 20120801 Add Method (Il faudrait sortir cette méthode pour qu'elle soit appelée par IO (importation de trade)
        /// Cette méthode est utilisée par le 
        /// - le calcul du risk
        /// - la cashBalance
        /// - le calcul des intérêts
        /// EG 20140204 [19586] Add ProcessBase parameter (gestion TimeOut/DeadLock)
        /// FI 20140930 [XXXXX] Modification de signature  add pIdMenu
        /// FI 20170404 [23039] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        internal static TradeCommonCaptureGen.ErrorLevel RecordTradeRisk (
            string pCS, IDbTransaction pDbTransaction,
            TradeRiskCaptureGen pCaptureGen,TradeRecordSettings pRecordSettings,
            CaptureSessionInfo pCaptureSession,Cst.Capture.ModeEnum pCaptureMode,
            string pIdMenu,
            SetErrorWarning pSetErrorWarning, AddCriticalException pAddCriticalException,
            LogLevelDetail pLogDetail, AddAttachedDoc pAttach)
        {
            TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            string identifier = string.Empty;
            
            TradeCommonCaptureGenException errExc = null;
            try
            {
                int idT = 0;
                
                // FI 20231123 [WI748] Mise en place d'un try Multiple
                try
                {
                    TryMultiple tryMultiple = new TryMultiple(pCS, "RecordTradeRisk_CheckAndRecord", $"Record Trade Risk")
                    {
                        SetErrorWarning = pSetErrorWarning,
                        LogRankOrder = 3,
                        ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                    };
                    
                    tryMultiple.Exec(() =>
                    {
                        pCaptureGen.CheckAndRecord(pCS, pDbTransaction, pIdMenu, pCaptureMode, pCaptureSession, pRecordSettings,
                        ref identifier, ref idT, out _, out _);
                    });
                }
                catch (TryMultipleException ex)
                {
                    if (ex.IsFromTargetInvocationException)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Error, ex.Message, 4));
                        throw ex.ActionException;
                    }
                    else
                        throw;
                }

                SpheresIdentification identification = TradeRDBMSTools.GetTradeIdentification(pCS, pDbTransaction, idT);
                pCaptureGen.TradeCommonInput.Identification = identification;

            }
            catch (TradeCommonCaptureGenException ex)
            {
                // FI 20200623 [XXXXX] AddException
                pAddCriticalException(ex);
                
                errExc = ex;
                lRet = errExc.ErrLevel;
            }
            catch (Exception) { throw; }//Error non gérée
            
            //Si une exception se produit après l'enregistrement du trade => cela veut dire que le trade est correctement rentré en base, on est en succès
            if ((null != errExc) && TradeCaptureGen.IsRecordInSuccess(errExc.ErrLevel))
                lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            
            #region Ecrire du fichier XML ds temporary et dans attached doc si erreur ou log en mode full
            if ((TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet) || (pLogDetail >= LogLevelDetail.LEVEL4))
            {
                bool isError = (TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet);
                string filename = string.Empty;
                string folder = string.Empty;
                try
                {
                    // FI 20170404 [23039] paramètre underlying, trader
                    string msg = pCaptureGen.GetResultMsgAfterCheckAndRecord(pCS, errExc, pCaptureMode, identifier, null, null, true, out string msgDet);
                    if (StrFunc.IsFilled(msgDet))
                        msg = msg + Cst.CrLf + msgDet;
                    //
                    if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == lRet)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.None, msg, 2));
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] AddException
                        pAddCriticalException.Invoke(errExc);
                        
                        Logger.Log(new LoggerData(errExc));
                        Logger.Log(new LoggerData(LogLevelEnum.Error, msg));
                    }
                    
                    try
                    {
                        pCaptureGen.WriteTradeXMLOnTemporary(pCaptureSession.session, out folder, out filename);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("<b>ERROR on writing tradeXML on temporary folder</b>", ex);
                    }


                    try
                    {
                        byte[] data = FileTools.ReadFileToBytes(folder + @"\" + filename);
                        pAttach.Invoke(pCS, pCaptureSession.idProcess_L.Value, pCaptureSession.user.IdA, data, filename, "xml");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("<b>ERROR on writing tradeXML in AttachedDoc table</b>", ex);
                    }
                }
                catch (Exception ex)
                {
                    // FI 20200623 [XXXXX] AddException
                    pAddCriticalException.Invoke(ex);

                    // FI 20200623 [XXXXX] SetErrorWarning
                    pSetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                }
            }
            #endregion
            
            return lRet;
        }
    }
}
