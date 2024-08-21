using EFS.ACommon;
using EFS.SpheresService;
using System;
using System.IO;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace EFS.Common.MQueue
{


    /// <summary>
    /// 
    /// </summary>
    public class MQueueTools
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pPath"></param>
        /// <returns></returns>
        private static string GetFileName(MQueueBase pMQueue, string pPath)
        {
            int ida_Entity = 0;

            if (pMQueue.header.requesterSpecified && pMQueue.header.requester.entitySpecified)
                ida_Entity = Convert.ToInt32(pMQueue.header.requester.entity.otcmlId);

            string folder = ServiceTools.GetQueueSuffix(pMQueue.ConnectionString, Cst.Process.GetService(pMQueue.ProcessType), ida_Entity);

            StringBuilder fileName = new StringBuilder();
            fileName.AppendFormat(@"{0}\{1}\{2}.xml", pPath, folder, pMQueue.header.messageQueueName);

            return fileName.ToString();
        }

        #region GetMQueueByProcess
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessTypeEnum"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        /// EG 20230918 [WI702] Closing / Reopening module : NormMsgFactory
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static MQueueBase GetMQueueByProcess(Cst.ProcessTypeEnum pProcessTypeEnum)
        {
            MQueueBase mQueue;
            switch (pProcessTypeEnum)
            {
                case Cst.ProcessTypeEnum.ACCOUNTGEN:
                    mQueue = new AccountGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.ACCRUALSGEN:
                    mQueue = new AccrualsGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.CASHBALANCE:
                    mQueue = new RiskPerformanceMQueue(pProcessTypeEnum);
                    break;
                case Cst.ProcessTypeEnum.CASHINTEREST:
                    mQueue = new CashBalanceInterestMQueue();
                    break;
                case Cst.ProcessTypeEnum.COLLATERALVAL:
                    mQueue = new CollateralValMQueue();
                    break;
                case Cst.ProcessTypeEnum.CIGEN:
                    mQueue = new ConfirmationInstrGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.CMGEN:
                    mQueue = new ConfirmationMsgGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.CMGEN_IO:
                    mQueue = new ConfirmationMsgGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.EARGEN:
                    mQueue = new EarGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.ESRGEN:
                case Cst.ProcessTypeEnum.ESRNETGEN:
                    mQueue = new ESRNetGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.ESRSTDGEN:
                    mQueue = new ESRStandardGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.EVENTSGEN:
                    mQueue = new EventsGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.EVENTSVAL:
                    mQueue = new EventsValMQueue();
                    break;
                case Cst.ProcessTypeEnum.FEESCALCULATION:
                case Cst.ProcessTypeEnum.FEESUNINVOICED:
                    // FI 20170306 [22225] new TradeActionMQueue 
                    TradeActionMQueue tradeActionMQueue = new TradeActionMQueue
                    {
                        tradeActionCode = (pProcessTypeEnum == Cst.ProcessTypeEnum.FEESCALCULATION)?
                        TradeActionCode.TradeActionCodeEnum.FeesCalculation: TradeActionCode.TradeActionCodeEnum.FeesEventGenUninvoiced
                    };
                    mQueue = new TradeActionGenMQueue();
                    ((TradeActionGenMQueue)mQueue).item = new TradeActionMQueue[] { tradeActionMQueue };
                    break;
                case Cst.ProcessTypeEnum.GATEBCS:
                    mQueue = new GateBCSMQueue();
                    break;
                case Cst.ProcessTypeEnum.INVOICINGGEN:
                    mQueue = new InvoicingGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.INVCANCELSIMUL:
                    mQueue = new InvoicingCancelationSimulationGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.INVVALIDSIMUL:
                    mQueue = new InvoicingValidationSimulationGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.IO:
                    mQueue = new IOMQueue();
                    break;
                case Cst.ProcessTypeEnum.LINEARDEPGEN:
                    mQueue = new LinearDepGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.MSOGEN:
                    mQueue = new MSOGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.MTMGEN:
                    mQueue = new MarkToMarketGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.POSKEEPENTRY:
                    mQueue = new PosKeepingEntryMQueue();
                    break;
                case Cst.ProcessTypeEnum.POSKEEPREQUEST:
                    mQueue = new PosKeepingRequestMQueue();
                    break;
                case Cst.ProcessTypeEnum.QUOTHANDLING:
                    mQueue = new QuotationHandlingMQueue();
                    break;
                case Cst.ProcessTypeEnum.RESPONSE:
                    mQueue = new ResponseRequestMQueue();
                    break;
                case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                    mQueue = new RiskPerformanceMQueue(pProcessTypeEnum);
                    break;
                case Cst.ProcessTypeEnum.SHELL:
                    mQueue = new ShellMQueue();
                    break;
                case Cst.ProcessTypeEnum.SIGEN:
                    mQueue = new SettlementInstrGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.TRADEACTGEN:
                    mQueue = new TradeActionGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.RIMGEN:
                    mQueue = new ReportInstrMsgGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.RMGEN:
                    mQueue = new ReportMsgGenMQueue();
                    break;
                case Cst.ProcessTypeEnum.NORMMSGFACTORY:
                case Cst.ProcessTypeEnum.IRQ:
                case Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE:
                case Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE:
                    mQueue = new NormMsgFactoryMQueue();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pProcessTypeEnum.ToString()));
            }
            return mQueue;
        }
        #endregion GetMQueueByProcess
        #region GetMQueuePriority
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMqueue"></param>
        /// <returns></returns>
        private static MessagePriority GetMQueuePriority(MQueueBase pMqueue)
        {
            MessagePriority ret = MessagePriority.Normal;
            if (pMqueue.header.messageQueueNameSpecified && pMqueue.IsIdT && pMqueue.idSpecified)
            {
                try
                {
                    string[] msg = pMqueue.header.messageQueueName.Split("_".ToCharArray());
                    if (ArrFunc.IsFilled(msg) && (MQueueBase.POSITION_STATUS_PRIORITY <= msg.Length))
                    {
                        string priority = msg[MQueueBase.POSITION_STATUS_PRIORITY];
                        priority = priority.Replace("X", string.Empty);
                        //
                        Cst.StatusPriority statusPriority = (Cst.StatusPriority)System.Enum.Parse(typeof(Cst.StatusPriority), priority);
                        switch (statusPriority)
                        {
                            case Cst.StatusPriority.HIGH:
                                ret = MessagePriority.High;
                                break;
                            case Cst.StatusPriority.REGULAR:
                                ret = MessagePriority.Normal;
                                break;
                            case Cst.StatusPriority.LOW:
                                ret = MessagePriority.Low;
                                break;
                        }
                    }
                }
                catch
                {
                    ret = MessagePriority.Normal;
                }
            }
            return ret;
        }
        #endregion GetMQueuePriority
        #region ReadFromFile
        /// <summary>
        ///  Charge un MQueueBase à partir d'un fichier 
        /// </summary>
        /// <param name="pFileName">Nom du fichier</param>
        /// <returns></returns>
        /// FI 20160128 [XXXXX] Reecriture de la méthode  
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        public static MQueueBase ReadFromFile(string pFileName)
        {
            //PL 20100928 Add for() and isError [NB: Pour (essayer) de gérer le cas où parfois le fichier est incomplet pour la lecture...)
            MQueueBase ret = null;
            for (int step = 0; step <= 1; step++)
            {
                bool isError = false;
                try
                {
                    using (FileStream fs = new FileStream(pFileName, FileMode.Open))
                    {
                        ret = MQueueTools.ReadFromMessage(new Message() { BodyStream = fs });
                    }
                }
                catch (Exception)
                {
                    if (step == 0)
                        isError = true;
                    else
                        throw;
                }
                if (!isError)
                    break;
            }
            return ret;
        }
        #endregion ReadFromFile
        #region ReadFromMessage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <returns></returns>
        public static MQueueBase ReadFromMessage(Message pMessage)
        {
            // FI 20201110 [XXXXX] Simplification de la lecture
            // Il n'y a plus plusieurs tentatives avec Mode DeCrypt ou non. Désormais la méthode Read applique automatiquement le décryptage si nécessaire    
            MQueueBase ret = (MQueueBase)(new MQueueFormatter().Read(pMessage));
            return ret;
        }
        #endregion ReadFromMessage
        #region Send
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessTypeEnum"></param>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        /// <param name="pParameters"></param>
        /// <param name="pRequester"></param>
        /// <param name="pSendInfo"></param>
        public static void Send(Cst.ProcessTypeEnum pProcessTypeEnum, string pSource, IdInfo pId, MQueueparameters pParameters, MQueueRequester pRequester, MQueueSendInfo pSendInfo)
        {
            MQueueBase mQueue = GetMQueueByProcess(pProcessTypeEnum);
            if (null != mQueue)
            {
                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = pSource,
                    parameters = pParameters,
                    requester = pRequester,
                    id = pId.id,
                    idInfo = pId
                };
                mQueue.Set(mQueueAttributes);
                Send(mQueue, pSendInfo);
            }
        }


        /// <summary>
        /// Envoi le message vers la queue adéquate
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pMqueueSendInfo"></param>
        public static void Send(MQueueBase pMQueue, MQueueSendInfo pMQueueSendInfo)
        {
            //Timeout: 0.1 --> Permet de n'effectuer qu'une seule tentative, car les tentatives ont lieu toutes les 0.5sec
            double timeout = 0.1;
            int attemps = 0;
            Send(pMQueue, pMQueueSendInfo, timeout, ref attemps);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pMQueueSendInfo"></param>
        /// <param name="pTimeout"></param>
        /// <param name="opAttemps"></param>
        public static void Send(MQueueBase pMQueue, MQueueSendInfo pMQueueSendInfo, double pTimeout, ref int opAttemps)
        {
            if ((null == pMQueueSendInfo) || (false == pMQueueSendInfo.IsInfoValid))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "MQueueSendInfo not defined",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MOM_UNKNOWN));

            Cst.MOM.MOMEnum MOMEnum = pMQueueSendInfo.MOMSetting.MOMType;
            string MOMPath = pMQueueSendInfo.MOMSetting.MOMPath;
            bool isRecoverable = pMQueueSendInfo.MOMSetting.MOMRecoverable;
            bool isEncrypt = pMQueueSendInfo.MOMSetting.MOMEncrypt;


            /* FI 20201001 [XXXXX] Mise en commentaire
            bool isNoEncryptForced = false;
//#if (DEBUG)
//            isNoEncryptForced = true;// En mode DEBUG, pas de cryptage de la CS
//#endif
            if (false == isEncrypt)
            {
                if (pMQueue.header.connectionString.IsCrypt)
                {
                    string MOMCmptLevel = (string)SystemSettings.GetAppSettings("MOMCmptLevel", typeof(string), string.Empty);
                    //PL 20120620 Nouveau principe: Cryptage systématique, sauf directive contraire (MOMCmptLevel) dans le fichier de config (TRIM 11069 EFS-Thread-Item:13)
                    //PL 20151007 Un bug, depuis la v2.6, faisait que le NON cryptage était systématique.
                    if ((!String.IsNullOrEmpty(MOMCmptLevel)) && ((MOMCmptLevel == "4.5") || (MOMCmptLevel == "2.5.0")))
                    {
                        isNoEncryptForced = true;
                    }
                    if (isNoEncryptForced)
                    {
                        //Niveau de Compatibilité v2.5.0/v4.5 (ou mode DEBUG) --> Pas de cryptage de la CS
                        string cs_uncrypted = pMQueue.ConnectionString;
                        pMQueue.header.connectionString.cryptSpecified = true;
                        pMQueue.header.connectionString.crypt = false;
                        pMQueue.header.ConnectionString = cs_uncrypted;
                    }
                }
            }
            */
            /* FI 20201001 [XXXXX] Nouvele version */
            if (false == isEncrypt)
            {
                //PL 20120620 Nouveau principe: Cryptage systématique, sauf directive contraire (MOMCmptLevel) dans le fichier de config (TRIM 11069 EFS-Thread-Item:13)
                Boolean isEncryptCS = true;
                string MOMCmptLevel = (string)SystemSettings.GetAppSettings("MOMCmptLevel", typeof(string), string.Empty);
                if ((StrFunc.IsFilled(MOMCmptLevel)) && ((MOMCmptLevel == "4.5") || (MOMCmptLevel == "2.5.0")))
                    isEncryptCS = false;

                pMQueue.header.connectionString.cryptSpecified = true;
                pMQueue.header.connectionString.crypt = isEncryptCS;
                pMQueue.header.BuildConnectionStringValue();
            }
            else
            {
                pMQueue.header.BuildConnectionStringValue();
            }

            switch (MOMEnum)
            {
                case Cst.MOM.MOMEnum.FileWatcher:
                    SendFile2(pMQueue, MOMPath, isEncrypt);
                    break;
                case Cst.MOM.MOMEnum.MSMQ:
                    opAttemps = 0;
                    SendQueue(pMQueue, MOMPath, isRecoverable, isEncrypt, string.Empty, pTimeout, ref opAttemps);
                    break;
                default:
                    break;
            }
        }
        #endregion Send

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pPath"></param>
        /// <param name="pIsEncrypt"></param>
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// EG 20180425 Analyse du code Correction [CA2202]
        /// FI 20230427 [XXXXX] En cas de IOException, cette dernière exception est ajoutée en inner exception our garder trace de l'exception source
        public static void SendFile2(MQueueBase pMQueue, string pPath, bool pIsEncrypt)
        {

            // PM 20100512
            // Résolution du problème de fichier en cours d'utilisation par un autre processus, lorsque deux processus 
            // testant en même temps l'existance d'un fichier message et essaient de le créer "presque" en même temps.
            DatetimeProfiler dtProfiler = null;
            bool isFileExist;
            do
            {
                String fileName = null;
                IOException iOException = null;

                //Si dtProfiler!=null, génération d'un nouveau nom de fichier, via le nouveau timestamp courant
                if (pMQueue.SetMessageQueueName(dtProfiler != null))
                {
                    fileName = GetFileName(pMQueue, pPath);
                    isFileExist = File.Exists(fileName.ToString());
                }
                else
                {
                    //Tip: On considère ici le fichier existant afin de produire une erreur au delà de 10 sec.
                    //     On ne doit en théorie jamais rentrer dans ce "else".
                    isFileExist = true;
                }

                if (!isFileExist)
                {

                    try
                    {
                        using (FileStream fs = new FileStream(fileName.ToString(), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                //Construct Message
                                MQueueFormatter mqFormatter = new MQueueFormatter
                                {
                                    isEncrypt = pIsEncrypt
                                };

                                Message msg = new Message();
                                mqFormatter.Write(msg, pMQueue);

                                using (StreamReader sr = new StreamReader(msg.BodyStream))
                                {
                                    //Write Message on Filewatcher
                                    sw.Write(sr.ReadToEnd());
                                }
                            }
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Répertoire invalide
                        throw;
                    }
                    catch (IOException ex)
                    {
                        //IOException: On considère qu'un fichier de même nom a été créé par un autre processus.
                        isFileExist = true;
                        iOException = ex;
                    }

                }

                if (isFileExist)
                {
                    if (dtProfiler == null)
                    {
                        //Initialisation de dtProfiler pour limiter les tentatives à 10 sec.
                        dtProfiler = new DatetimeProfiler(DateTime.Now);
                    }
                    else if (dtProfiler.GetTimeSpan().Seconds >= 10)
                    {
                        //Au delà de 10 sec. de tentative, on génère une erreur "timeout"
                        throw new Exception($"Timeout expired! Unable to send mqueue file. {fileName}: File already exist or incorrect name.", iOException);
                    }
                }
            }
            while (isFileExist);
        }

        #region SendQueue
        public static void SendQueue(MQueueBase pMQueue, string pPath, bool pIsRecoverable, bool pIsEncrypt)
        {
            SendQueue(pMQueue, pPath, string.Empty, pIsRecoverable, pIsEncrypt, string.Empty);
        }
        public static void SendQueue(MQueueBase pMQueue, string pPath, string pSuffix, bool pIsRecoverable,
        bool pIsEncrypt, string pLabel)
        {
            //Timeout: 0.1 --> Permet de n'effectuer qu'une seule tentative, car les tentatives ont lieu toutes les 0.5sec
            double timeout = 0.1;
            int attemps = 0;
            SendQueue(pMQueue, pPath, pSuffix, pIsRecoverable, pIsEncrypt, pLabel, timeout, ref attemps);
        }
        public static void SendQueue(MQueueBase pMQueue, string pPath, bool pIsRecoverable,
        bool pIsEncrypt, string pLabel, double pTimeout, ref int opAttemps)
        {
            SendQueue(pMQueue, pPath, string.Empty, pIsRecoverable, pIsEncrypt, pLabel, pTimeout, ref opAttemps);
        }
        public static void SendQueue(MQueueBase pMQueue, string pPath, string pSuffix, bool pIsRecoverable,
        bool pIsEncrypt, string pLabel, double pTimeout, ref int opAttemps)
        {
            pMQueue.SetMessageQueueName(false);

            MQueueFormatter formatter = new MQueueFormatter
            {
                isEncrypt = pIsEncrypt
            };

            //generation de message
            Message msg = new Message(pMQueue, formatter)
            {
                Label = StrFunc.IsFilled(pLabel) ? pLabel : pMQueue.header.messageQueueName,
                Priority = GetMQueuePriority(pMQueue),
                //PL 20101020 FDA 20101020 WARNING: Management of Recoverable property
                Recoverable = pIsRecoverable
            };

            //envoi du message
            // RD 20130402 [18549] 
            if (StrFunc.IsEmpty(pSuffix))
            {
                int ida_Entity = 0;
                if (pMQueue.header.requesterSpecified && pMQueue.header.requester.entitySpecified)
                    ida_Entity = Convert.ToInt32(pMQueue.header.requester.entity.otcmlId);

                pSuffix = ServiceTools.GetQueueSuffix(pMQueue.ConnectionString, Cst.Process.GetService(pMQueue.ProcessType), ida_Entity);
            }

            string pathProcess = pPath + pSuffix;
            MessageQueue mq = MQueueTools.GetMsMQueue(pathProcess, pTimeout, out opAttemps);
            mq.Send(msg);
        }
        #endregion SendQueue


        #region GetMsMQueue_NoError
        /// <summary>
        /// Retourne une objet MessageQueue gérant la queue.
        /// <para>Spheres® tente éventuellement plusieurs tentatives</para>
        /// </summary>
        /// <param name="pPath">Path de la queue</param>
        /// <param name="pTimeout">Timeout en seconde (0 pour timeout infini)</param>
        /// <returns>MessageQueue: Null si la queue est injoignable</returns>
        public static MessageQueue GetMsMQueue_NoError(string pPath, double pTimeOut)
        {
            if (pTimeOut <= 0)
                pTimeOut = 60;
            return GetMsMQueue(false, pPath, pTimeOut, out _);
        }
        #endregion GetMsMQueue_NoError
        #region GetMsMQueue
        /// <summary>
        /// Retourne une objet MessageQueue gérant la queue
        /// <para>Spheres® tente éventuellement plusieurs tentatives</para>
        /// </summary>
        /// <param name="pPath">Path de la queue</param>
        /// <param name="pTimeout">Timeout en seconde (0 pour timeout infini)</param>
        /// <param name="opAttemps">Nombre de tentatives effectuées (en cas de connexion en succès)</param>
        /// <returns>MessageQueue</returns>
        /// <exception cref="SpheresException2 [MOM_PATH_ERROR] si la queue n'est pas accessible"/>
        public static MessageQueue GetMsMQueue(string pPath, double pTimeOut, out int opAttemps)
        {
            return GetMsMQueue(true, pPath, pTimeOut, out opAttemps);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsThrowError"></param>
        /// <param name="pPath"></param>
        /// <param name="pTimeOut"></param>
        /// <param name="opAttemps"></param>
        /// <returns></returns>
        private static MessageQueue GetMsMQueue(bool pIsThrowError, string pPath, double pTimeOut, out int opAttemps)
        {
            MessageQueue messageQueue = null;

            Exception lastException = null;
            bool isMOMExist = false;

            //WARNING: Sur une queue "Remote" le type d'adresse "FormatName" ne fonctionne pas en mode DEBUG, le compte windows ne dispose pas des droits nécessaires sur le "Remote serveur".
            string path = pPath;

            double timeOut = pTimeOut;
            bool isWarning = false;
            bool isWarningRecorded = false;
            int count = 0;
            DatetimeProfiler dtProfiler = new DatetimeProfiler(DateTime.Now);
            #region while isMOMExist / timeout
            while (((timeOut <= 0) || (dtProfiler.GetTimeSpan().TotalSeconds.CompareTo(timeOut) == -1))
                && (!isMOMExist)
                && (count < Int32.MaxValue))
            {
                try
                {
                    //NB: Avec une adresse de type "FormatName", le seul moyen de renseigner le path, 
                    //    c'est au moment de l'instanciation de l'objet (la propriété "path" ne le permet pas).
                    count++;

                    if (count > 1)
                        Thread.Sleep(500); //Pause de 0.5 sec.

                    messageQueue = new MessageQueue(path);

                    //Vérification du bon fonctionnement de la queue, par généreration d'une erreur lors du MoveNext() 
                    // FI 20221230 [XXXXX] usage de using
                    using (MessageEnumerator messageEnum = messageQueue.GetMessageEnumerator2())
                    {
                        messageEnum.MoveNext();
                    }

                    isMOMExist = true;

                    if (isWarningRecorded)
                        isWarning = false;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    //isWarning = (count == 1) || ((_eventLogLevel == ErrorManager.DetailEnum.FULL) && ((count % 10) == 0));
                    isWarning = (count == 1) || (true && ((count % 10) == 0));
                }
                finally
                {
                    if (isWarning)
                    {
                        //Tentative infructeuse, écriture d'un message Warning dans le journal Windows
                        isWarningRecorded = true;
                        isWarning = false;

                        string warningMsg = @"MSMQ is unreachable" + Cst.CrLf;
                        warningMsg += @"- Contact your system administrator to make sure there are no problem with the network or its configurations." + Cst.CrLf;
                        warningMsg += @"[Path: " + path + "]" + Cst.CrLf;
                        warningMsg += @"[Failed attempts to access: " + count.ToString() + "]" + Cst.CrLf;
                        //WriteEventLog(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.MOM_PATH_ERROR, "ActivateService", warningMsg, null, true);
                    }
                }
            }
            #endregion

            if (isMOMExist)
            {
                #region isMOMExist --> WriteEventLog()
                //Tentative fructeuse, écriture d'un message Information dans le journal Windows
                if (isWarningRecorded)
                {
                    //N tentative(s) infructeuse(s), écriture d'un message Information dans le journal Windows
                    string infoMsg = @"MSMQ has opened successfully" + Cst.CrLf;
                    infoMsg += @"[Path: " + path + "]" + Cst.CrLf;
                    infoMsg += @"[Attempts to access: " + count.ToString() + "]";
                    //WriteEventLog(ProcessStateTools.StatusNoneEnum, Cst.ErrLevel.CONNECTED, "ActivateService", infoMsg, null, true);
                }
                #endregion
            }
            else
            {
                #region !isMOMExist --> throw new SpheresException()
                if (pIsThrowError)
                {
                    string errorMsg = @"Message queuing service (MSMQ) does not exist or is unreachable" + Cst.CrLf;
                    errorMsg += @"[Path: " + path + "]" + Cst.CrLf;
                    errorMsg += @"[Failed attempts to access: " + count.ToString() + "]" + Cst.CrLf;
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errorMsg,
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MOM_PATH_ERROR), lastException);
                }
                else
                {
                    messageQueue = null;
                }
                #endregion
            }

            opAttemps = count;
            return messageQueue;
        }
        #endregion GetMsMQueue

        /// <summary>
        ///  Retourne les attributs de subsitutions en fonction du type de message Queue
        ///  <para>Retourne null s'il n'existe pas d'attributs de substitution</para>
        /// </summary>
        /// <param name="pMQueueType"></param>
        /// FI 20140519 [19923] add Method
        public static XmlAttributeOverrides GetXmlAttributeOverrides(Type pMQueueType)
        {
            XmlAttributeOverrides xmlAttributeOverrides = null;

            // sur RequestTrackMqueue, les MQueueparameter ont des attributs diffirents pour que le flux xml final soit cohérent
            if (pMQueueType.Equals(typeof(RequestTrackMqueue)))
            {
                xmlAttributeOverrides = new XmlAttributeOverrides();

                XmlAttributes xmlAttributes = new XmlAttributes
                {
                    //n instead of name
                    XmlAttribute = new XmlAttributeAttribute("n")
                };
                xmlAttributeOverrides.Add(typeof(MQueueparameter), "name", xmlAttributes);
                //dn instead of name
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("dn")
                };
                xmlAttributeOverrides.Add(typeof(MQueueparameter), "displayname", xmlAttributes);
                //type instead of dataType
                xmlAttributes = new XmlAttributes
                {
                    XmlAttribute = new XmlAttributeAttribute("type")
                };
                xmlAttributeOverrides.Add(typeof(MQueueparameter), "dataType", xmlAttributes);
            }

            return xmlAttributeOverrides;
        }


        /// <summary>
        /// Ajoute le paramètre ISCREATEDBY_NORMMSGFACTORY dans le message {pQueue}
        /// </summary>
        /// <param name="pQueue"></param>
        /// FI 20240118 [WI814] Add Method
        public static void AddFactoryFlagParameter(MQueueBase pQueue)
        {
            if (false == pQueue.parametersSpecified)
                pQueue.parameters = new MQueueparameters();

            MQueueparameter parameter = new MQueueparameter(MQueueBase.PARAM_ISCREATEDBY_NORMMSGFACTORY, TypeData.TypeDataEnum.@bool);
            parameter.SetValue(true);

            pQueue.parameters.Add(parameter);
            pQueue.parametersSpecified = true;
        }

        /// <summary>
        /// Ajoute le paramètre PARAM_ISCREATEDBY_GATEWAY dans le message {pQueue}
        /// </summary>
        /// <param name="pQueue"></param>
        /// FI 20240118 [WI814] Add Method
        public static void AddGatewayFlagParameter(IOMQueue pQueue)
        {
            if (false == pQueue.parametersSpecified)
                pQueue.parameters = new MQueueparameters();

            MQueueparameter parameter = new MQueueparameter(IOMQueue.PARAM_ISCREATEDBY_GATEWAY, TypeData.TypeDataEnum.@bool);
            parameter.SetValue(true);

            pQueue.parameters.Add(parameter);
            pQueue.parametersSpecified = true;
        }

    }


    /// <summary>
    /// Représente les propriétes qui définissent l'envoi de message à travers un MOM 
    /// </summary>
    public class MQueueSendInfo
    {
        
        #region Accessors
        /// <summary>
        /// Obtient true si les propriétés du MOM sont valides (MOM et MOMPath correctement définis)
        /// </summary>
        public bool IsInfoValid
        {
            get { return MOMSetting.IsInfoValid; }
        }
        /// <summary>
        /// Obtient ou définit les proporiétés du MOM à la reception du message
        /// </summary>
        public MOMSettings MOMSetting
        {
            get;
            set;
        }
        #endregion Accessors

        #region Constructors
        public MQueueSendInfo()
        {
            
        }
        #endregion Constructors
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20140519 [19923] possibilité d'attribuer des substitutions de serialization
    public class MQueueFormatter : IMessageFormatter, ICloneable
    {
        #region Members
        /// <summary>
        ///  Crypt on writing
        /// </summary>
        public bool isEncrypt;
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public MQueueFormatter()
        {
            isEncrypt = false;
        }
        #endregion Constructors

        #region IMessageFormatter Members

        public Boolean CanRead(Message message)
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// FI 20140519 [19923] prise en compte des substitutions de serialization
        /// EG 20180423 Analyse du code Correction [CA2202]
        /// FI 20200608 [XXXXX] using syntax
        public object Read(Message message)
        {
            string text = null;
            using (Stream stream = message.BodyStream)
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                    // FI 20201110 [XXXXX] si le text n'est pas en XML => Decrypt pour obtenir un message qui en théorie devrait être en XML 
                    // Il n'y a plus lecture de la property isEncrypt utilisée désormais uniquement pour l'écrirure
                    //if (isEncrypt)
                    if (false == StrFunc.IsXML(text))
                        text = Cryptography.Decrypt(text);
                }
            }

            Type tMQueue = GetTypeMessageQueue(text);
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(tMQueue, text)
            {
                // FI 20140519 [19923]  
                XmlAttributeOverrides = MQueueTools.GetXmlAttributeOverrides(tMQueue)
            };
            MQueueBase ret = (MQueueBase)CacheSerializer.Deserialize(serializeInfo);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        /// FI 20140519 [19923] prise en compte des substitutions de serialization
        public void Write(Message message, object obj)
        {
            /// FI 20140519 [19923] add  xmlAttributeOverrides
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(obj.GetType(), obj)
            {
                XmlAttributeOverrides = MQueueTools.GetXmlAttributeOverrides(obj.GetType())
            };

            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());

            string text = sb.ToString();
            if (isEncrypt)
                text = Cryptography.Encrypt(text);
            byte[] buff = Encoding.UTF8.GetBytes(text);
            message.BodyStream = new MemoryStream(buff);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// FI 20140519 [19923] prise en compte des substitutions de de serialization
        public Stream Write(object obj)
        {

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(MQueueBase), obj)
            {
                /// FI 20140519 [19923]  
                XmlAttributeOverrides = MQueueTools.GetXmlAttributeOverrides(obj.GetType())
            };

            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());

            string text = sb.ToString();
            if (isEncrypt)
                text = Cryptography.Encrypt(text);
            
            byte[] buff = Encoding.UTF8.GetBytes(text);
            Stream stream = new MemoryStream(buff);
            return stream;
        }

        #endregion IMessageFormatter Members
        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public object Clone()
        {
            MQueueFormatter clone = new MQueueFormatter
            {
                isEncrypt = isEncrypt
            };
            return clone;
        }

        #endregion ICloneable Members

        private static Type GetTypeMessageQueue(string pMessage)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(pMessage);
            string rootDocument = xmlDocument.DocumentElement.Name;

            MQueueBase mQueueBase = new MQueueBase();
            object[] attributes = mQueueBase.GetType().GetCustomAttributes(typeof(XmlIncludeAttribute), true);
            if (0 < attributes.Length)
            {
                foreach (object attribute in attributes)
                {
                    XmlIncludeAttribute includeAttribute = (XmlIncludeAttribute)attribute;
                    object[] rootAttributes = includeAttribute.Type.GetCustomAttributes(typeof(XmlRootAttribute), true);
                    if (0 < rootAttributes.Length)
                    {
                        foreach (object rootAttribute in rootAttributes)
                        {
                            if (rootDocument == ((XmlRootAttribute)rootAttribute).ElementName)
                                return includeAttribute.Type;
                        }
                    }
                }
            }
            return null;
        }


    }
    

}
