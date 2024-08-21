#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.ClosingReopeningPositions;
using System;
using System.Collections.Generic;
#endregion Using Directives

namespace EFS.Process
{
    #region NormMsgFactory_CLOSINGREOPENINGINTEGRATE
    /// <summary>
    /// Construction Messages de type : NORMMSGFACTORY
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType : CLOSINGREOPENINGINTEGRATE</para>
    ///<para>  ● id          : Id de la table ACTIONREQUEST</para>
    ///<para>  ● identifier  : Identifiant de la demande d'action dans la table ACTIONREQUEST</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée : les caractéristiques de l'action
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Données générales</para>
    ///<para>  ● Données liées au(x) scope(sq) concerné(s)</para>
    ///<para>  ● Données liées à la méthode de closing et de réouverture</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Le service NormMsgFactory ne prépare pas un message pour un autre service applicatif,  
    /// il se charge directement de l'intégration ou de la modification dans la table ACTIONREQUEST.
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Recherche de l'action (table ACTIONREQUEST)</para>
    ///<para>  ● recherche et implémentation des identifiants (MARCHE, CONTRATS, ...)</para>
    ///<para>  ● Création/MAJ (table ACTIONREQUEST)</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    /// EG 20230901 [WI702] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public class NormMsgFactory_CLOSINGREOPENINGINTEGRATE : NormMsgFactoryBase
    {
        #region Members
        private ClosingReopeningAction m_ClosingReopeningAction;
        private ClosingReopeningAction m_ClosingReopeningActionIntegrated;
        #endregion Members
        #region Constructors
        public NormMsgFactory_CLOSINGREOPENINGINTEGRATE(NormMsgFactoryProcess pNormMsgFactoryProcess) : base(pNormMsgFactoryProcess)
        {
        }
        #endregion Constructors
        #region Methods
        #region Generate
        public override Cst.ErrLevel Generate()
        {
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 8120), 0,
                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

            // Log : Lectures de paramètres et Alimentation de la classe ClosingReopeningAction
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8121), 1,
                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

            int idA = Convert.ToInt32(m_NormMsgFactoryProcess.MQueue.header.requester.idA);

            // Chargement de la classe
            Cst.ErrLevel ret = SetClosingReopeningActionClass();
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                ARQInfo _arqInfo = new ARQInfo(idA, m_NormMsgFactoryProcess.MQueue.id.ToString());
                // Recherche de l'existence dans la table ACTIONREQUEST sur la base de l'IDENTIFIER
                bool isExist = m_ClosingReopeningAction.Exist(_arqInfo, ARQTools.ARQWhereMode.IDENTIFIER);
                if (isExist)
                {
                    m_ClosingReopeningActionIntegrated = new ClosingReopeningAction(m_NormMsgFactoryProcess.Cs)
                    {
                        identifier = m_ClosingReopeningAction.identifier,
                        identifierSpecified = m_ClosingReopeningAction.identifierSpecified
                    };
                    m_ClosingReopeningActionIntegrated.Load(_arqInfo, ARQTools.ARQWhereMode.IDENTIFIER);

                    // Un ligne existe dans la table avec le statut incomplet : on procède à la compléter avec
                    // les données présentes dans le message NormMsgFactory
                    if (m_ClosingReopeningActionIntegrated.readystate == ARQTools.ActionRequestReadyStateEnum.TOCOMPLETE)
                    {
                        m_ClosingReopeningAction.readystate = ARQTools.ActionRequestReadyStateEnum.REGULAR;
                        m_ClosingReopeningAction.IdARQ = m_ClosingReopeningActionIntegrated.IdARQ;
                    }
                    else
                    {
                        // Une action existe déjà avec cet identifiant la ise à jour rejetée.
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 8135), 2,
                            new LogParam(LogTools.IdentifierAndId(m_ClosingReopeningAction.identifier, m_ClosingReopeningAction.IdARQ)),
                            new LogParam(LogTools.IdentifierAndId(m_ClosingReopeningActionIntegrated.identifier, m_ClosingReopeningActionIntegrated.IdARQ))));

                        ret = Cst.ErrLevel.NOTHINGTODO;
                    }
                }
                else
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8023), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8024), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                    m_ClosingReopeningAction.readystate = ARQTools.ActionRequestReadyStateEnum.REGULAR;
                    // Alimentation des données manquante dans la classe (via Parameters dans BuildInfo)
                    ret = m_ClosingReopeningAction.ConstructNormMsgFactoryMessage();

                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        // Ecriture dans la table
                        ret = m_ClosingReopeningAction.Write(_arqInfo);
                    }
                }
            }
            return ret;
        }
        #endregion Generate

        #region SetClosingReopeningActionClass
        /// <summary>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// Alimentation de la classe ClosingReopeningAction via les paramètres présents 
        /// dans le fichier de type NORMMSGFACTORY
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20230901 [WI702] New : ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected Cst.ErrLevel SetClosingReopeningActionClass()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                m_ClosingReopeningAction = new ClosingReopeningAction(m_NormMsgFactoryProcess.Cs)
                {
                    identifier = BuildingInfo.identifier,
                    identifierSpecified = BuildingInfo.identifierSpecified
                };
                List<string> _unavailableParameters = SetNormMsgFactoryParameters(BuildingInfo.parameters);
                if (0 < _unavailableParameters.Count)
                {
                    // Paramètre obligatoire manquant dans le message
                    string _value = string.Empty;
                    _unavailableParameters.ForEach(item => _value += item + " - ");
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8030), 2,
                        new LogParam(_value + "(ClosingReopening)"),
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                    ret = Cst.ErrLevel.DATANOTFOUND;
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] AddCriticalException
                m_NormMsgFactoryProcess.ProcessState.AddCriticalException(ex);

                // RD 20171013 [23506] Log exception                        
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));

                ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
        }
        #endregion SetClosingReopeningActionClass


        #region SetNormMsgFactoryParameters
        /// EG 20230901 [WI702] New : ClosingReopeningPosition - Delisting action - NormMsgFactory
        private List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = new List<string>();
            _unavailableParameters.AddRange(m_ClosingReopeningAction.SetNormMsgFactoryParameters(m_NormMsgFactoryProcess.Cs, pMQueueParameters));
            return _unavailableParameters;
        }
        #endregion SetNormMsgFactoryParameters

        #endregion Methods
    }
    #endregion NormMsgFactory_CLOSINGREOPENINGINTEGRATE
}