using EFS.ACommon;
using EFS.Common.MQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.Process
{
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class ProcessBase
    {

        /// <summary>
        /// Permet l'écriture dans de Tracker
        /// </summary>
        public Tracker Tracker
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient true si l'alimentation du tracker est disponible
        /// <para>Retourné généralement true, sauf lorsque l'initialisation du tracker a planté ou si l'initialisation du tracker n'a pas eu lieu du fait d'une exception en amont</para>
        /// </summary>
        /// FI 20201013 [XXXXX] Add
        public Boolean IsTrackerAvailable
        {
            get
            {
                return (Tracker != null) && (Tracker.IdTRK_L > 0);
            }
        }


        /// <summary>
        ///  Creation de <see cref="Tracker"/>
        ///  <para>création d'un nouvel enregistrement dans la table TRACKER_L ou Mise à jour d'un enregistrement</para>
        /// <para>La création d'un nouvel enregistrement s'effectue lorsque la demande de traitement ne fait pas suite à une demande web (Exemple Message issu d'une gateway)</para>
        /// </summary>
        private void SetTracker()
        {
            try
            {
                if (MQueue.header.requesterSpecified && MQueue.header.requester.idTRKSpecified)
                {
                    Tracker = new Tracker(Cs)
                    {
                        IdTRK_L = MQueue.header.requester.idTRK
                    };
                    Tracker.Select();

                    Tracker.ReadyState = ProcessStateTools.ReadyStateActiveEnum;
                    Tracker.Status = ProcessState.Status;
                    if (MQueue.header.requester.entitySpecified)
                        Tracker.Request.Session.IdA_Entity = Convert.ToInt32(MQueue.header.requester.entity.otcmlId);
                    Tracker.Update(Session);
                }
                else
                {
                    // Aucune ligne dans le TRACKER : INSERTION
                    InsertNewTracker();
                }
            }
            catch (Exception) // FI 20201013 [XXXXX]
            {
                Tracker = null; // Ainsi la property IsTrackerAvailable  retourne false;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InsertNewTracker()
        {
            InsertNewTracker(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPostedMsg"></param>
        private void InsertNewTracker(int pPostedMsg)
        {
            TrackerAttributes trackerAtt = BuildTrackerAttributes();

            Tracker = new Tracker(Cs)
            {
                ProcessRequested = MQueue.ProcessType,
                ReadyState = ProcessStateTools.ReadyStateActiveEnum,
                Status = ProcessState.Status,
                Group = trackerAtt.BuildTrackerGroup(),
                IdData = new IdData(MQueue.id, MQueue.identifier, DataIdent),
                Data = trackerAtt.BuildTrackerData()
            };
            // FI 20120129 [18252]
            // Les traitements I/O rentre dans le group EXT du tracker
            if (ProcessType == Cst.ProcessTypeEnum.IO)
            {
                if ((MQueue as IOMQueue).IsGatewayMqueue)
                    Tracker.Group = Cst.GroupTrackerEnum.EXT;
            }

            if (null != trackerAtt.acknowledgment)
                Tracker.Ack = trackerAtt.acknowledgment;

            Tracker.Insert(Session, pPostedMsg);

            MQueue.header.requester = new MQueueRequester(Tracker.IdTRK_L, Tracker.Request.Session, Tracker.Request.DtRequest);
            MQueue.header.requesterSpecified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20190214 Correction messages Tracker pour NormMsgFactory
        private TrackerAttributes BuildTrackerAttributes()
        {
            TrackerAttributes ret = new TrackerAttributes()
            {
                process = MQueue.ProcessType,
                gProduct = MQueue.GetStringValueIdInfoByKey("GPRODUCT")
            };
            if (MQueue is PosKeepingRequestMQueue posKeepingRequestMQueue)
                ret.caller = posKeepingRequestMQueue.requestType.ToString();
            else if (MQueue is IOMQueue ioMqueue)
                ret.caller = ioMqueue.GetStringValueIdInfoByKey("IN_OUT");
            else if (MQueue is NormMsgFactoryMQueue normMsgFactoryMQueue)
            {
                if (normMsgFactoryMQueue.buildingInfo.posRequestTypeSpecified && (Cst.PosRequestTypeEnum.None != normMsgFactoryMQueue.buildingInfo.posRequestType))
                    ret.caller = normMsgFactoryMQueue.buildingInfo.posRequestType.ToString();
                else
                    ret.caller = normMsgFactoryMQueue.buildingInfo.processType.ToString();

                if (null != normMsgFactoryMQueue.acknowledgment)
                {
                    ret.acknowledgment = new TrackerAcknowledgmentInfo()
                    {
                        extlId = normMsgFactoryMQueue.acknowledgment.extlId,
                        schedules = normMsgFactoryMQueue.acknowledgment.schedules
                    };
                }
            }
            else if (MQueue is ConfirmationMsgGenMQueue)
                ret.caller = "TODO";
            else if (MQueue is InvoicingGenMQueue)
                ret.caller = "TODO";
            else if (MQueue is QuotationHandlingMQueue)
                ret.caller = "TODO";

            ret.info = TrackerAttributes.BuildInfo(MQueue);

            return ret;
        }

        /// <summary>
        /// <para>
        /// - Mise à jour du statut du tracker
        /// </para>
        /// <para>
        /// - Génération des accusés de réception lorsque la demande de traitement a totalement été traité
        /// </para>
        /// </summary>
        /// FI 20160412 [XXXXX] Add
        /// EG 20180525 [23979] IRQ Processing
        private void TrackerFinalize()
        {
            if (IsTrackerAvailable) // FI 20201013 [XXXXX] Test sur IsLogAvailbale
            {
                try
                {
                    if (ProcessStateTools.IsStatusTerminated(ProcessState.Status))
                    {
                        Tracker.SetCounter(ProcessState.Status, ProcessState.PostedSubMsg, Session);
                        if (ProcessStateTools.IsReadyStateTerminated(Tracker.ReadyState))
                            Tracker.AckGenerate(ProcessType);
                    }
                }
                catch (Exception ex)
                {
                    AppInstance.AppTraceManager.TraceError(this, $"An Exception occured in tracker Finalize. Exception: {ex.GetType()}:{ex.Message}");
                    throw;
                }
            }
        }

    }
}
