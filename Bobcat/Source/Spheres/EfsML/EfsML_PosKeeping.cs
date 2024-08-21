#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;
            
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.DynamicData;

#endregion Using Directives
namespace EfsML.Business
{
    public sealed class PosKeepingTools
    {
        #region Constructors
        public PosKeepingTools() { }
        #endregion Constructors

        #region ManualBulkMessage
        public Cst.ErrLevelMessage ManualBulkMessage(string pCS, DataTable pDataTable, MQueueparameter[] pParameters, params string[] pDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevelMessage errMessage = null;
            string msgReturn = string.Empty;

            //string methodName = "PosKeepingTools.SendManualBulkMessage";
            try
            {
                int idA_Dealer = 0;
                int idB_Dealer = 0;
                int idA_EntityDealer = 0;
                int idA_Clearer = 0;
                int idB_Clearer = 0;
                int idA_EntityClearer = 0;
                int idAsset = 0;
                int clearing_Qty = 0;
                ArrayList aItems = new ArrayList();
                PosKeepingBulk[] items = null;
                foreach (DataRow row in pDataTable.Rows)
                {
                    clearing_Qty = Convert.ToInt32(row["CLEARINGQTY"]);
                    if (0 < clearing_Qty)
                    {
                        idA_Dealer = Convert.ToInt32(row["IDA_DEALER"]);
                        if (false == Convert.IsDBNull(row["IDA_ENTITYDEALER"]))
                            idA_EntityDealer = Convert.ToInt32(row["IDA_ENTITYDEALER"]);
                        idB_Dealer = Convert.ToInt32(row["IDB_DEALER"]);

                        idA_Clearer = Convert.ToInt32(row["IDA_CLEARER"]);
                        if (false == Convert.IsDBNull(row["IDA_ENTITYCLEARER"]))
                            idA_EntityClearer = Convert.ToInt32(row["IDA_ENTITYCLEARER"]);
                        if (false == Convert.IsDBNull(row["IDB_CLEARER"]))
                            idB_Clearer = Convert.ToInt32(row["IDB_CLEARER"]);

                        idAsset = Convert.ToInt32(row["IDASSET"]);
                        aItems.Add(new PosKeepingBulk(idAsset, clearing_Qty, idA_Dealer, idB_Dealer, idA_EntityDealer, idA_Clearer, idB_Clearer, idA_EntityClearer));
                    }
                }
                if (0 < aItems.Count)
                {
                    items = (PosKeepingBulk[])aItems.ToArray(typeof(PosKeepingBulk));
                    errLevel = SendManualBulkMessage(pCS, pParameters, items, ref msgReturn, pDatas);
                }
                else
                {
                    errLevel = Cst.ErrLevel.DATANOTFOUND;
                    msgReturn = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                msgReturn = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                msgReturn += ex.Message + Cst.CrLf;
                if (null != ex.InnerException)
                    msgReturn += ex.InnerException.Message;
            }
            finally
            {
                errMessage = new Cst.ErrLevelMessage(errLevel, msgReturn);
            }
            return errMessage;
        }
        #endregion ManualBulkMessage

        #region SendManualBulkMessage
        private static Cst.ErrLevel SendManualBulkMessage(string pCS, MQueueparameter[] pParameters, PosKeepingBulk[] pItems, ref string pMsgReturn, string[] pDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string msgReturn = "Msg_PROCESS_GENERATE_MONO_DATA";
            string msgParameter = string.Empty;
            try
            {
                
                MQueueTaskInfo taskInfo = new MQueueTaskInfo();
                MQueueIdInfo[] idInfo = new MQueueIdInfo[1] { new MQueueIdInfo() };
                idInfo[0].idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_FUT) };

                MQueueBase mQueue = new PosKeepingManualBulkMQueue(pCS, pItems, idInfo[0], new MQueueparameters(pParameters), null);
                msgParameter = Ressource.GetString2("Msg_PosKeepingManbualBulkMessage",
                                DtFunc.DateTimeToString(mQueue.GetMasterDate(), DtFunc.FmtShortDate), pDatas[0]);
                taskInfo.mQueue = new MQueueBase[] { mQueue };
                //
                taskInfo.process = Cst.ProcessTypeEnum.POSKEEPMANBULK;
                taskInfo.connectionString = pCS;
                taskInfo.idInfo = idInfo;
                taskInfo.appInstance = SessionTools.GetAppInstance();
                bool isThreadPool = (bool)SystemSettings.GetAppSettings("ThreadPool", typeof(System.Boolean), true);
                if (isThreadPool)
                {
                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                    taskInfo.handle = ThreadPool.RegisterWaitForSingleObject(autoResetEvent,
                        new WaitOrTimerCallback(MQueueTools.SendMultiple), taskInfo, 1000, false);
                    Thread.Sleep(1000);
                    autoResetEvent.Set();
                }
                else
                {
                    MQueueTools.SendMultiple(taskInfo, false);
                }
                int nbSend = 0;
                if (null != taskInfo.mQueue)
                    nbSend = ArrFunc.Count(taskInfo.mQueue);
                //
                msgReturn = Ressource.GetString2(msgReturn, nbSend.ToString(), msgParameter);
            }
            catch (OTCmlException otcmlException)
            {
                errLevel = Cst.ErrLevel.FAILURE;
            }
            catch (Exception ex)
            {
                errLevel = Cst.ErrLevel.FAILURE;
            }
            pMsgReturn = msgReturn;
            return errLevel;
        }
        #endregion SendManualBulkMessage
    }
}
