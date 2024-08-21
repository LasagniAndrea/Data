#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Gestion Interruption d'un traitement
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType    : IRQ</para>
    ///<para>  ● posRequestType : Valeur de PosRequestTypeEnum (EOD, CLOSINGDAY...), ProcessTypeEnum, IOElementType</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● IDTRK_L        : Identifiant du tracker lié au traitement à interrompre</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    // EG 20180509 New
    public class NormMsgFactory_IRQ : NormMsgFactoryBase
    {
        #region Members
        private readonly IRQRequester _irqRequester;
        #endregion Members
        #region Constructors
        public NormMsgFactory_IRQ(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
            // initialisation d'un IRQRequester
            _irqRequester = new IRQRequester(m_NormMsgFactoryProcess.Cs, BuildingInfo.id);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Création d'une sémaphore de demande d'interruption de traitement
        /// </summary>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Generate()
        {
            Cst.IRQLevel ret = Cst.IRQLevel.REJECTED;

            
            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 9900), 0,
                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                new LogParam(_irqRequester.RequestedBy),
                new LogParam(_irqRequester.RequestedAt)));

            string processType = BuildingInfo.parameters["PROCESS"].ExValue;

            // La sémaphore est créée si elle n'existe pas encore et que le traitment à
            // interrompre n'est pas encore terminé (ReadyState = ACTIVE|REQUESTED)
            switch (_irqRequester.ReadyState)
            {
                case ProcessStateTools.ReadyStateEnum.ACTIVE:
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9902), 1,
                        new LogParam(processType),
                        new LogParam(_irqRequester.RequestedBy),
                        new LogParam(_irqRequester.RequestedAt),
                        new LogParam(_irqRequester.AppName),
                        new LogParam(_irqRequester.AppVersion),
                        new LogParam(_irqRequester.HostName)));

                    ret = IRQTools.CreateNamedSemaphore(m_NormMsgFactoryProcess.Tracker, m_NormMsgFactoryProcess.ProcessState.SetErrorWarning, _irqRequester);
                    break;
                case ProcessStateTools.ReadyStateEnum.REQUESTED:
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9903), 1,
                        new LogParam(processType),
                        new LogParam(_irqRequester.RequestedBy),
                        new LogParam(_irqRequester.RequestedAt)));

                    ret = IRQTools.CreateNamedSemaphore(m_NormMsgFactoryProcess.Tracker, m_NormMsgFactoryProcess.ProcessState.SetErrorWarning, _irqRequester);
                    break;
                case ProcessStateTools.ReadyStateEnum.TERMINATED:
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9904), 1,
                        new LogParam(processType),
                        new LogParam(_irqRequester.RequestedBy),
                        new LogParam(_irqRequester.RequestedAt),
                        new LogParam(_irqRequester.Status),
                        new LogParam(_irqRequester.AppName),
                        new LogParam(_irqRequester.AppVersion),
                        new LogParam(_irqRequester.HostName)));
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9907), 1));
                    break;
            }
            if (ret == Cst.IRQLevel.EXECUTED)
            {
                // Message informatif : La sémaphore est créée, la demande d'interruption est donc activée
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9905), 1));
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9910), 1));
            }

            return (ret == Cst.IRQLevel.EXECUTED) ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.FAILUREWARNING;
        }
        public override Cst.ErrLevel SendFinalMessages()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Methods
    }
}
