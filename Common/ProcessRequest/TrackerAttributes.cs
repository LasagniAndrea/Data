using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.Common.MQueue;


namespace EFS.Process
{

    /// <summary>
    ///  Spheres Attributs nécessaires à l'alimentation du tracker (<see cref="Tracker.IdData"/>, <see cref="Tracker.Data"/>, <see cref="Tracker.Ack"/>)
    /// </summary>
    public class TrackerAttributes
    {
        #region Members
        /// <summary>
        /// process
        /// </summary>
        public Cst.ProcessTypeEnum process;
        /// <summary>
        /// source optionnelle de la demande: 
        /// IdMenu : Action diverses
        /// RequestType : Position
        /// Cst.Capture.ModeEnum : Saisie d'une négociation
        /// </summary>
        public string caller;
        /// <summary>
        /// dictionnaire d'infos diverses pour alimenter le tracker
        /// IDDATA, IDDATAIDENT, IDDATAIDENTIFIER
        /// DATA1, DATA2, DATA3, DATA4, DATA5
        /// </summary>
        public List<DictionaryEntry> info;
        /// <summary>
        /// Type de Product (sert à déterminer le groupe d'appartenance de la ligne du tracker)
        /// </summary>
        public string gProduct;
        /// <summary>
        /// Informations pour la gestion des accusés de traitement
        /// </summary>
        public TrackerAcknowledgmentInfo acknowledgment;
        #endregion Members

        #region Constructors
        public TrackerAttributes() { }
        #endregion Constructors

        #region Method
        /// <summary>
        ///  Permet l'alimentation de <see cref="Tracker.IdData"/>
        /// </summary>
        /// <returns></returns>
        public IdData BuildTrackerIdData()
        {
            IdData ret = new IdData();
            if ((null != info) && (0 < info.Count))
            {
                if (info.Exists(match => match.Key.ToString() == "IDDATA"))
                    ret.id = Convert.ToInt32(info.Find(match => match.Key.ToString() == "IDDATA").Value);
                if (info.Exists(match => match.Key.ToString() == "IDDATAIDENT"))
                    ret.idIdent = Convert.ToString(info.Find(match => match.Key.ToString() == "IDDATAIDENT").Value);
                if (info.Exists(match => match.Key.ToString() == "IDDATAIDENTIFIER"))
                    ret.idIdentifier = Convert.ToString(info.Find(match => match.Key.ToString() == "IDDATAIDENTIFIER").Value);
            }

            return ret;
        }
        /// <summary>
        ///  Permet l'alimentation de <see cref="Tracker.Data"/>
        /// </summary>
        /// <returns></returns>
        public TrackerData BuildTrackerData()
        {

            Tuple<string, int> sys = GetSystemMsg(gProduct, process, caller, info);


            TrackerData ret = new TrackerData()
            {
                sysCode = sys.Item1,
                sysNumber = sys.Item2
            };

            if (ArrFunc.IsFilled(info))
            {
                if (info.Exists(match => match.Key.ToString() == "DATA1"))
                    ret.data1 = Convert.ToString(info.Find(match => match.Key.ToString() == "DATA1").Value);
                if (info.Exists(match => match.Key.ToString() == "DATA2"))
                    ret.data2 = Convert.ToString(info.Find(match => match.Key.ToString() == "DATA2").Value);
                if (info.Exists(match => match.Key.ToString() == "DATA3"))
                    ret.data3 = Convert.ToString(info.Find(match => match.Key.ToString() == "DATA3").Value);
                if (info.Exists(match => match.Key.ToString() == "DATA4"))
                    ret.data4 = Convert.ToString(info.Find(match => match.Key.ToString() == "DATA4").Value);
                if (info.Exists(match => match.Key.ToString() == "DATA5"))
                    ret.data5 = Convert.ToString(info.Find(match => match.Key.ToString() == "DATA5").Value);
            }

            return ret;
        }

        /// <summary>
        ///  Permet l'alimentation de <see cref="TrackerBase.Group"/>
        /// </summary>
        /// <returns></returns>
        /// FI 20230109 [XXXXX] Refactoring
        public Cst.GroupTrackerEnum BuildTrackerGroup()
        {
            Cst.GroupTrackerEnum ret = Cst.GroupTrackerEnum.EXT;
            Boolean isInvoicing = (Cst.ProductGProduct_ADM == gProduct);
            if (isInvoicing)
            {
                ret = Cst.GroupTrackerEnum.INV;
            }
            else
            {
                Cst.TrackerSystemMsgAttribute att = ReflectionTools.GetAttribute<Cst.TrackerSystemMsgAttribute>(process);
                if (null != att)

                    ret = att.Group;
            }
            return ret;
        }



        /// <summary>
        /// Alimentation du code message SYSNUMBER
        /// </summary>
        /// FI 20170313 [22225] Modify
        // EG 20180525 [23979] IRQ Processing
        private static Tuple<string, int> GetSystemMsg(string gProduct, Cst.ProcessTypeEnum pProcess, string pCaller, List<DictionaryEntry> pListInfo)
        {
            bool isObserver = (pCaller == "ISOBSERVER");
            string sysCode = "TRK";
            int sysNumber = GetTrackerSysNumber<Cst.ProcessTypeEnum>(pProcess.ToString(), isObserver ? Cst.TrackerSysNumberType.Observer : Cst.TrackerSysNumberType.Regular);

            if ((false == isObserver) && StrFunc.IsFilled(pCaller))
            {
                if (Enum.IsDefined(typeof(Cst.Capture.ModeEnum), pCaller))
                {
                    /// FI 20190722 [XXXXX] Call GetTrackerSysNumberByCaptureMode
                    sysNumber = GetTrackerSysNumberByCaptureMode(pCaller, gProduct);
                }
                else if (Enum.IsDefined(typeof(IdMenu.Menu), pCaller) || IdMenu.ConvertToMenu(pCaller).HasValue)
                {
                    int sysNumberMenu = GetTrackerSysNumberByIdMenu(pCaller, pListInfo);
                    if (sysNumberMenu > 0)
                        sysNumber = sysNumberMenu;
                }
                else if (Enum.IsDefined(typeof(Cst.In_Out), pCaller))
                {
                    sysNumber = GetTrackerSysNumber<Cst.In_Out>(pCaller);
                }
                else if (Enum.IsDefined(typeof(Cst.ProcessTypeEnum), pCaller))
                {
                    Boolean isOK = true;
                    if ((pProcess == Cst.ProcessTypeEnum.NORMMSGFACTORY) && (false == ExistSysNumberNMF<Cst.ProcessTypeEnum>(pCaller)))
                        isOK = false;

                    if (isOK)
                    {
                        // Pour NORMMSGFACTORY
                        // On rentre ici s'il existe un message tracker personnalisé pour le ProcessTypeEnum (pCaller) 
                        // Sans message tracker personnalisé Spheres conserve le message classique de NORMMSGFACTORY
                        sysNumber = GetTrackerSysNumber<Cst.ProcessTypeEnum>(gProduct, pProcess, pCaller);
                    }
                }
                else if (Enum.IsDefined(typeof(Cst.PosRequestTypeEnum), pCaller))
                {
                    Boolean isOK = true;
                    if ((pProcess == Cst.ProcessTypeEnum.NORMMSGFACTORY) && (false == ExistSysNumberNMF<Cst.PosRequestTypeEnum>(pCaller)))
                        isOK = false;

                    if (isOK)
                    {
                        // Pour NORMMSGFACTORY
                        // On rentre ici s'il existe un message tracker personnalisé pour le PosRequestType (pCaller) 
                        // Sans message tracker personnalisé Spheres conserve le message classique de NORMMSGFACTORY
                        sysNumber = GetTrackerSysNumber<Cst.PosRequestTypeEnum>(gProduct, pProcess, pCaller);
                    }
                }
            }

            return new Tuple<string, int>(sysCode, sysNumber);
        }

        /// <summary>
        /// Recherche SYSNUMBER d'un Menu
        /// </summary>
        /// FI 20190722 [XXXXX] static method
        private static int GetTrackerSysNumberByIdMenu(string pValue, List<DictionaryEntry> pListInfo)
        {
            int ret = 0;
            Cst.TrackerSysNumberType trkType = Cst.TrackerSysNumberType.Regular;

            if ((ArrFunc.IsFilled(pListInfo)) &&
                (pListInfo.Exists(match => match.Key.ToString() == "PRODUCTNAME")) &&
                (pListInfo.Find(match => match.Key.ToString() == "PRODUCTNAME").Value.ToString() == Cst.ProductInvoiceSettlement))
                trkType = Cst.TrackerSysNumberType.TradeAdmin;

            Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(pValue);
            if (menu.HasValue)
                ret = GetTrackerSysNumber<IdMenu.Menu>(menu.Value.ToString(), trkType);

            return ret;
        }

        /// <summary>
        /// Recherche SYSNUMBER d'un Cst.Capture.ModeEnum
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pGProduct"></param>

        /// FI 20190722 [XXXXX] Add Method (pas d'usage de pListInfo pour l'instant, en place en vue d'un prochain usage)
        private static int GetTrackerSysNumberByCaptureMode(string pValue, string pGProduct)
        {
            Cst.TrackerSysNumberType trackerSysNumberType;
            switch (pGProduct)
            {
                case Cst.ProductGProduct_ASSET:
                    // si création, modification, etc  d'un asset => alors c'est forcément un titre
                    trackerSysNumberType = Cst.TrackerSysNumberType.TradeDebtSec;
                    break;
                case Cst.ProductGProduct_ADM:
                    trackerSysNumberType = Cst.TrackerSysNumberType.TradeAdmin;
                    break;
                default:
                    trackerSysNumberType = Cst.TrackerSysNumberType.Default;
                    break;
            }
            int ret = GetTrackerSysNumber<Cst.Capture.ModeEnum>(pValue, trackerSysNumberType);

            return ret;
        }
        /// <summary>
        /// Recherche SYSNUMBER sur la base d'une valeur (pValue) d'enum (T)
        /// </summary>
        // EG 20190214 Correction messages Tracker pour NormMsgFactory
        private static int GetTrackerSysNumber<T>(string gProduct, Cst.ProcessTypeEnum pProcess, string pValue) where T : struct
        {
            Cst.TrackerSysNumberType trkType = Cst.TrackerSysNumberType.Regular;
            if (pProcess == Cst.ProcessTypeEnum.NORMMSGFACTORY)
                trkType = Cst.TrackerSysNumberType.NormMsgFactory;
            else if (pProcess == Cst.ProcessTypeEnum.IRQ)
                trkType = Cst.TrackerSysNumberType.IRQRequest;
            else if (gProduct == Cst.ProductGProduct_MTM)
                trkType = Cst.TrackerSysNumberType.MarkToMarket;
            return GetTrackerSysNumber<T>(pValue, trkType);
        }
        /// <summary>
        /// Recherche SYSNUMBER sur la base d'une valeur (pValue) d'enum (T)
        /// </summary>
        /// EG 20190214 Correction messages Tracker pour NormMsgFactory
        /// FI 20190722 [XXXXX] static method
        private static int GetTrackerSysNumber<T>(string pValue, Cst.TrackerSysNumberType pTrackerSysNumberType = Cst.TrackerSysNumberType.Regular) where T : struct
        {
            int ret = 0;
            Cst.TrackerSystemMsgAttribute attribute = ReflectionTools.GetAttribute<Cst.TrackerSystemMsgAttribute>(typeof(T), pValue);
            if (null != attribute)
            {
                switch (pTrackerSysNumberType)
                {
                    case Cst.TrackerSysNumberType.Observer:
                        ret = attribute.SysNumber_ObserverMode;
                        break;
                    case Cst.TrackerSysNumberType.Regular:
                        ret = attribute.SysNumber;
                        break;
                    case Cst.TrackerSysNumberType.TradeAdmin:
                        ret = attribute.SysNumber_TradeAdmin;
                        if (0 == ret)
                            ret = GetTrackerSysNumber<T>(pValue); // FI 20190722 [XXXXX] lecture du regular si pas ressource type Admin
                        break;
                    case Cst.TrackerSysNumberType.TradeDebtSec: // FI 20190722 [XXXXX] Add
                        ret = attribute.SysNumber_TradeDebtSec;
                        if (0 == ret)
                            ret = GetTrackerSysNumber<T>(pValue);
                        break;
                    case Cst.TrackerSysNumberType.MarkToMarket:
                        ret = attribute.SysNumber_MTM;
                        break;
                    case Cst.TrackerSysNumberType.IRQRequest:
                        ret = attribute.SysNumber_IRQRequest;
                        if (0 == ret)
                            ret = ReflectionTools.GetAttribute<Cst.TrackerSystemMsgAttribute>(Cst.ProcessTypeEnum.IRQ).SysNumber;
                        break;
                    case Cst.TrackerSysNumberType.NormMsgFactory:
                        ret = attribute.SysNumber_NMF;
                        if (0 == ret)
                            ret = GetTrackerSysNumber<T>(pValue);
                        break;
                    case Cst.TrackerSysNumberType.Default:
                    default:
                        ret = attribute.SysNumber_Default;
                        if (0 == ret)
                            ret = GetTrackerSysNumber<T>(pValue);
                        break;
                }
            }
            return ret;
        }
        #endregion Method
        #region Static Method

        /// <summary>
        /// Alimentation des données DATA1..DATA5 pour la ligne du tracker
        /// </summary>
        /// <returns></returns>
        /// EG 20130607 [18740] Add RemoveCAExecuted
        /// EG 20140826 Add InvoicingValidation|InvoicingCancellation
        /// FI 20150108 [XXXXX] Modify
        /// FI 20170314 [22225] Modify
        /// FI 20170327 [23004] Modify  
        // EG 20180525 [23979] IRQ Processing
        // EG 20190214 Correction messages Tracker pour NormMsgFactory
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static List<DictionaryEntry> BuildInfo(MQueueBase pQueue)
        {
            List<DictionaryEntry> lstData;
            if (pQueue is PosKeepingRequestMQueue _queue)
            {
                lstData = new List<DictionaryEntry>();
                switch (_queue.requestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                    case Cst.PosRequestTypeEnum.PositionCancelation:
                    case Cst.PosRequestTypeEnum.PositionTransfer:
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    case Cst.PosRequestTypeEnum.RemoveAllocation:
                    case Cst.PosRequestTypeEnum.ClearingSpecific:
                    case Cst.PosRequestTypeEnum.UnClearing:
                    case Cst.PosRequestTypeEnum.ClearingBulk:
                    case Cst.PosRequestTypeEnum.UpdateEntry:
                        // FI 20170327 [23004] Aliemntation de lstData 
                        lstData.Add(new DictionaryEntry("DATA1", _queue.GetIdInfoEntry("ENTITY").Value));
                        lstData.Add(new DictionaryEntry("DATA2", _queue.GetIdInfoEntry("CSSCUSTODIAN").Value));
                        lstData.Add(new DictionaryEntry("DATA3", _queue.GetIdInfoEntry("MARKET").Value));
                        lstData.Add(new DictionaryEntry("DATA4", _queue.GetIdInfoEntry("CONTRACTNAME").Value));
                        lstData.Add(new DictionaryEntry("DATA5", _queue.GetIdInfoEntry("DTBUSINESS").Value));
                        break;
                    case Cst.PosRequestTypeEnum.ClosingDay:
                    case Cst.PosRequestTypeEnum.EndOfDay:
                    case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                        // FI 20150108 [XXXXX] Use parameter CSSCUTODIAN instead of CLEARINGHOUSE    
                        lstData.Add(new DictionaryEntry("DATA1", _queue.parameters["ENTITY"].ExValue));
                        lstData.Add(new DictionaryEntry("DATA2", _queue.parameters["CSSCUSTODIAN"].ExValue));
                        lstData.Add(new DictionaryEntry("DATA3", _queue.parameters["DTBUSINESS"].Value));
                        break;
                    case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                        // FI 20150108 [XXXXX] Use parameter CSSCUTODIAN instead of CLEARINGHOUSE    
                        lstData.Add(new DictionaryEntry("DATA1", _queue.parameters["ENTITY"].ExValue));
                        lstData.Add(new DictionaryEntry("DATA2", _queue.parameters["CSSCUSTODIAN"].ExValue));
                        if (null != _queue.parameters["MARKET"])
                            lstData.Add(new DictionaryEntry("DATA3", _queue.parameters["MARKET"].Value));
                        lstData.Add(new DictionaryEntry("DATA4", _queue.parameters["DTBUSINESS"].Value));
                        if (null != _queue.parameters["ISSIMUL"])
                            lstData.Add(new DictionaryEntry("DATA5", _queue.parameters["ISSIMUL"].Value));
                        break;
                    case Cst.PosRequestTypeEnum.RemoveCAExecuted:
                        // FI 20150108 [XXXXX] Use parameter CSSCUTODIAN instead of CLEARINGHOUSE    
                        lstData.Add(new DictionaryEntry("DATA1", _queue.parameters["CSSCUSTODIAN"].ExValue));
                        lstData.Add(new DictionaryEntry("DATA2", _queue.parameters["MARKET"].Value));
                        lstData.Add(new DictionaryEntry("DATA3", _queue.parameters["CORPORATEACTION"].ExValue));
                        lstData.Add(new DictionaryEntry("DATA4", _queue.parameters["DTBUSINESS"].Value));
                        if (null != _queue.parameters["ISSIMUL"])
                            lstData.Add(new DictionaryEntry("DATA5", _queue.parameters["ISSIMUL"].Value));
                        break;
                    default:
                        break;
                }
                return lstData;
            }
            else if (pQueue is IOMQueue _queue1)
            {
                lstData = new List<DictionaryEntry>();
                // FI 20130924 [] test _queue.idInfo != null
                // Si la tâche est inconnue (Mauvais paramétrage de la Gateway par exemple)
                if (_queue1.idInfo != null)
                    lstData.Add(new DictionaryEntry("IDDATA", _queue1.idInfo.id));
                lstData.Add(new DictionaryEntry("IDDATAIDENT", _queue1.GetStringValueIdInfoByKey("ident")));
                lstData.Add(new DictionaryEntry("IDDATAIDENTIFIER", _queue1.GetStringValueIdInfoByKey("identifier")));
                lstData.Add(new DictionaryEntry("DATA1", _queue1.GetStringValueIdInfoByKey("displayName")));
            }
            else if (pQueue is NormMsgFactoryMQueue _queue2)
            {
                lstData = new List<DictionaryEntry>();

                MQueueparameters parameters = _queue2.buildingInfo.parameters;

                switch (_queue2.buildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE:
                        lstData.Add(new DictionaryEntry("IDDATA", _queue2.id));
                        if (parameters["REFNOTICE"].nameSpecified)
                            lstData.Add(new DictionaryEntry("IDDATAIDENTIFIER", parameters["REFNOTICE"].Value));
                        lstData.Add(new DictionaryEntry("IDDATAIDENT", Cst.OTCml_TBL.CORPOACTIONISSUE.ToString()));
                        lstData.AddRange(BuildInfo(_queue2.buildingInfo.processType, parameters));
                        break;
                    case Cst.ProcessTypeEnum.IRQ:
                        lstData.Add(new DictionaryEntry("IDDATA", _queue2.id));
                        lstData.Add(new DictionaryEntry("IDDATAIDENT", Cst.OTCml_TBL.TRACKER_L.ToString()));
                        lstData.AddRange(BuildInfo(_queue2.buildingInfo.processType, parameters));
                        break;
                    case Cst.ProcessTypeEnum.RIMGEN:
                        lstData.Add(new DictionaryEntry("DATA1", parameters["ENTITY"].Value));
                        lstData.Add(new DictionaryEntry("DATA2", parameters["CNFTYPE"].Value));
                        break;
                    case Cst.ProcessTypeEnum.CASHBALANCE:
                        lstData.Add(new DictionaryEntry("DATA1", parameters["ENTITY"].Value));
                        lstData.Add(new DictionaryEntry("DATA2", parameters["TIMING"].Value));
                        break;
                    case Cst.ProcessTypeEnum.POSKEEPREQUEST:
                        // FI 20230109 [XXXXX] Test sur ExistSysNumberNMF
                        if (ExistSysNumberNMF<Cst.PosRequestTypeEnum>(_queue2.buildingInfo.posRequestType.ToString()))
                        {
                            // Pour NORMMSGFACTORY
                            // On rentre ici s'il existe un message tracker personnalisé pour le posRequestType
                            // Dans ce cas, le message personalisé affiche nécessairement les 2 paramètres obligatoires
                            lstData.Add(new DictionaryEntry("DATA1", parameters["ENTITY"].Value));
                            lstData.Add(new DictionaryEntry("DATA2", parameters["CSSCUSTODIAN"].Value));
                        }
                        else
                        {
                            // On rentre ici s'il n'existe pas un message tracker personnalisé pour le posRequestType  (à ce jour UPDENTRY)
                            lstData = BuildInfoNormsgFactoryDefault(_queue2);
                        }
                        break;
                    default:
                        lstData = BuildInfoNormsgFactoryDefault(_queue2);
                        break;
                }
            }
            else if (pQueue is TradeActionGenMQueue _queue3)
            {
                if (ArrFunc.IsEmpty(_queue3.item))
                    throw new NullReferenceException("item is empty");

                if (_queue3.item[0].tradeActionCode == TradeActionCode.TradeActionCodeEnum.FeesCalculation)
                    lstData = BuildInfo(Cst.ProcessTypeEnum.FEESCALCULATION, pQueue.parameters);
                else if (_queue3.item[0].tradeActionCode == TradeActionCode.TradeActionCodeEnum.FeesEventGenUninvoiced)
                    lstData = BuildInfo(Cst.ProcessTypeEnum.FEESUNINVOICED, pQueue.parameters);
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Action (id:{0}) is not implemented", _queue3.item[0].tradeActionCode.ToString()));
            }
            else
            {
                lstData = BuildInfo(pQueue.ProcessType, pQueue.parameters);
            }

            // FI 20180328 [23871] Call RemoveHTMLEntities
            List<DictionaryEntry> ret = RemoveHTMLEntities(lstData);

            return ret;
        }

        /// <summary>
        /// Retourne dictionnaire de String. Les clés alimentées sont DATA1..DATA5
        /// </summary>
        /// <param name="pProcess"></param>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        /// FI 20161108 [RATP] Modify
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190214 Gestion DTBUSINESS pour CASHBALANCE
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        public static List<DictionaryEntry> BuildInfo(Cst.ProcessTypeEnum pProcess, MQueueparameters pParameters)
        {
            List<DictionaryEntry> lstData = new List<DictionaryEntry>();
            switch (pProcess)
            {
                case Cst.ProcessTypeEnum.IRQ:
                    if ((null != pParameters["DATA1"]) && StrFunc.IsFilled(pParameters["DATA1"].Value))
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATA1"].Value));
                    if ((null != pParameters["DATA2"]) && StrFunc.IsFilled(pParameters["DATA2"].Value))
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["DATA2"].Value));
                    if ((null != pParameters["DATA3"]) && StrFunc.IsFilled(pParameters["DATA3"].Value))
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["DATA3"].Value));
                    if ((null != pParameters["DATA4"]) && StrFunc.IsFilled(pParameters["DATA4"].Value))
                        lstData.Add(new DictionaryEntry("DATA4", pParameters["DATA4"].Value));
                    if ((null != pParameters["DATA5"]) && StrFunc.IsFilled(pParameters["DATA5"].Value))
                        lstData.Add(new DictionaryEntry("DATA5", pParameters["DATA5"].Value));
                    break;
                case Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE:
                    lstData.Add(new DictionaryEntry("DATA1", pParameters["CAMARKET"].Value + " (" + pParameters["CAMARKET"].ExValue + ")"));
                    lstData.Add(new DictionaryEntry("DATA2", pParameters["PUBDATE"].Value));
                    if (pParameters["CETYPE"].ExValueSpecified)
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["CETYPE"].ExValue));
                    else
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["CETYPE"].Value));
                    lstData.Add(new DictionaryEntry("DATA4", pParameters["ADJMETHOD"].Value));
                    if (null != pParameters["UNLCODE"])
                        lstData.Add(new DictionaryEntry("DATA5", pParameters["UNLCODE"].Value));
                    else if (null != pParameters["UNLIDENTIFIER"])
                        lstData.Add(new DictionaryEntry("DATA5", pParameters["UNLIDENTIFIER"].Value));
                    else if (null != pParameters["UNLSYMBOL"])
                        lstData.Add(new DictionaryEntry("DATA5", pParameters["UNLSYMBOL"].Value));
                    break;
                case Cst.ProcessTypeEnum.MSOGEN:
                    if (null != pParameters["DTMSO"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DTMSO"].Value));
                    break;
                case Cst.ProcessTypeEnum.EVENTSGEN:
                case Cst.ProcessTypeEnum.EVENTSVAL:
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["ENTITY"].ExValue));
                    break;
                case Cst.ProcessTypeEnum.ACCOUNTGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    break;
                case Cst.ProcessTypeEnum.ACCRUALSGEN:
                case Cst.ProcessTypeEnum.LINEARDEPGEN:
                case Cst.ProcessTypeEnum.MTMGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    break;
                case Cst.ProcessTypeEnum.SIGEN:
                case Cst.ProcessTypeEnum.CIGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    if (null != pParameters["ISTOEND"])
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["ISTOEND"].Value));
                    break;

                case Cst.ProcessTypeEnum.CMGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ISMODEREGENERATE"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ISMODEREGENERATE"].Value));
                    break;
                case Cst.ProcessTypeEnum.RIMGEN:
                    if (null != pParameters["DTBUSINESS"])
                    {
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DTBUSINESS"].Value));
                        if (null != pParameters["ENTITY"])
                            lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                        if (null != pParameters["CNFTYPE"])
                        {
                            if (Cst.DDLVALUE_ALL == pParameters["CNFTYPE"].Value)
                                lstData.Add(new DictionaryEntry("DATA3", pParameters["CNFTYPE"].Value));
                            else
                                lstData.Add(new DictionaryEntry("DATA3", pParameters["CNFTYPE"].ExValueSpecified ? pParameters["CNFTYPE"].ExValue : pParameters["CNFTYPE"].Value));
                        }
                        if (null != pParameters["GMARKET"])
                            lstData.Add(new DictionaryEntry("DATA4",
                                pParameters["GMARKET"].ExValueSpecified ? pParameters["GMARKET"].ExValue : "-"));
                        else
                            lstData.Add(new DictionaryEntry("DATA4", "-"));

                        if (null != pParameters["MARKET"])
                        {
                            if (Cst.DDLVALUE_ALL == pParameters["MARKET"].Value)
                                lstData.Add(new DictionaryEntry("DATA5", pParameters["MARKET"].Value));
                            else
                                lstData.Add(new DictionaryEntry("DATA5", pParameters["MARKET"].ExValueSpecified ? pParameters["MARKET"].ExValue : pParameters["MARKET"].Value));
                        }
                        else
                            lstData.Add(new DictionaryEntry("DATA5", "-"));
                    }
                    else
                    {
                        if ((null != pParameters["DATE1"]) && (null != pParameters["DATE2"]))
                        {
                            string betweenDates = pParameters["DATE1"].Value;
                            if (false == betweenDates.Equals(pParameters["DATE2"].Value))
                                betweenDates = String.Format("[{0},{1}]", pParameters["DATE1"].Value, pParameters["DATE2"].Value);
                            lstData.Add(new DictionaryEntry("DATA1", betweenDates));
                        }

                        if (null != pParameters["ENTITY"])
                            lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));

                        if (null != pParameters["CNFTYPE"])
                        {
                            if (Cst.DDLVALUE_ALL == pParameters["CNFTYPE"].Value)
                                lstData.Add(new DictionaryEntry("DATA3", pParameters["CNFTYPE"].Value));
                            else
                                lstData.Add(new DictionaryEntry("DATA3", pParameters["CNFTYPE"].ExValueSpecified ? pParameters["CNFTYPE"].ExValue : pParameters["CNFTYPE"].Value));
                        }

                        if (null != pParameters["GMARKET"])
                            lstData.Add(new DictionaryEntry("DATA4",
                                StrFunc.IsFilled(pParameters["GMARKET"].ExValue) ? pParameters["GMARKET"].ExValue : "-"));
                        else
                            lstData.Add(new DictionaryEntry("DATA4", "-"));

                        if (null != pParameters["MARKET"])
                        {
                            if (Cst.DDLVALUE_ALL == pParameters["MARKET"].Value)
                                lstData.Add(new DictionaryEntry("DATA5", pParameters["MARKET"].Value));
                            else
                                lstData.Add(new DictionaryEntry("DATA5", pParameters["MARKET"].ExValueSpecified ? pParameters["MARKET"].ExValue : pParameters["MARKET"].Value));
                        }
                        else
                            lstData.Add(new DictionaryEntry("DATA5", "-"));
                    }
                    break;
                case Cst.ProcessTypeEnum.RMGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    break;
                case Cst.ProcessTypeEnum.INVOICINGGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    if (null != pParameters["ISSIMUL"])
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["ISSIMUL"].Value));
                    break;
                case Cst.ProcessTypeEnum.INVVALIDSIMUL:
                case Cst.ProcessTypeEnum.INVCANCELSIMUL:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    break;
                case Cst.ProcessTypeEnum.FEESCALCULATION:
                    // FI 20180328 [23871] Alimentation de DATA1,...DATA5
                    //DATA1
                    if ((null != pParameters["DATE1"]) && (null != pParameters["DATE2"]))
                    {
                        string betweenDates = pParameters["DATE1"].Value;
                        if (false == betweenDates.Equals(pParameters["DATE2"].Value))
                            betweenDates = String.Format("[{0},{1}]", pParameters["DATE1"].Value, pParameters["DATE2"].Value);
                        lstData.Add(new DictionaryEntry("DATA1", betweenDates));
                    }
                    //DATA2
                    lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    //DATA3
                    lstData.Add(new DictionaryEntry("DATA3", pParameters["CSSCUSTODIAN"].ExValue));
                    //DATA4
                    string data4 = string.Format("{0} / {1} / {2} / {3}",
                         pParameters["FEESCALCULATIONMODE"].ExValue,
                         pParameters["FEE"].ExValue,
                         pParameters["FEESCHEDULE"].ExValue,
                         pParameters["FEEMATRIX"].ExValue);

                    lstData.Add(new DictionaryEntry("DATA4", data4));
                    //DATA5
                    string data5 = string.Format("{0} / {1}",
                        pParameters["ISMANFEES_PRESERVED"].Value,
                        pParameters["ISFORCEDFEES_PRESERVED"].Value);
                    lstData.Add(new DictionaryEntry("DATA5", data5));
                    break;
                case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                    if (null != pParameters["ENTITY"])
                    {
                        if ("-1" == pParameters["ENTITY"].Value)
                            lstData.Add(new DictionaryEntry("DATA1", Cst.DDLVALUE_ALL));
                        else
                            lstData.Add(new DictionaryEntry("DATA1", pParameters["ENTITY"].ExValue));
                    }
                    if (null != pParameters["TIMING"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["TIMING"].Value));
                    if (null != pParameters["ISSIMUL"])
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["ISSIMUL"].Value));
                    if (null != pParameters["ISRESET"])
                        lstData.Add(new DictionaryEntry("DATA4", pParameters["ISRESET"].Value));
                    break;
                case Cst.ProcessTypeEnum.CASHBALANCE:
                    if (null != pParameters["ENTITY"])
                    {
                        if ("-1" == pParameters["ENTITY"].Value)
                            lstData.Add(new DictionaryEntry("DATA1", Cst.DDLVALUE_ALL));
                        else
                            lstData.Add(new DictionaryEntry("DATA1", pParameters["ENTITY"].ExValue));
                    }
                    // DATA2 is used for DtBusiness
                    if (null != pParameters["DTBUSINESS"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["DTBUSINESS"].Value));

                    if (null != pParameters["TIMING"])
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["TIMING"].Value));
                    break;
                case Cst.ProcessTypeEnum.CASHINTEREST:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    break;
                case Cst.ProcessTypeEnum.EARGEN:
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    if (null != pParameters["ENTITY"])
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["ENTITY"].ExValue));
                    if (null != pParameters["CLASS"])
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["CLASS"].Value));
                    break;
                case Cst.ProcessTypeEnum.ESRSTDGEN: // FI 20161108
                case Cst.ProcessTypeEnum.ESRNETGEN: // FI 20161108 GLOP (il faudra vérifier si Ok)
                    if (null != pParameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATE1"].Value));
                    break;
                // JN 20190528 - Mise à jour d'une règle d'échéance sur dérivés listés
                case Cst.ProcessTypeEnum.MATURITYRULEUPDATE:
                    if ((null != pParameters["IDDATA"]) && StrFunc.IsFilled(pParameters["IDDATA"].Value))
                        lstData.Add(new DictionaryEntry("IDDATA", pParameters["IDDATA"].Value));
                    if ((null != pParameters["IDDATAIDENT"]) && StrFunc.IsFilled(pParameters["IDDATAIDENT"].Value))
                        lstData.Add(new DictionaryEntry("IDDATAIDENT", pParameters["IDDATAIDENT"].Value));
                    if ((null != pParameters["IDDATAIDENTIFIER"]) && StrFunc.IsFilled(pParameters["IDDATAIDENTIFIER"].Value))
                        lstData.Add(new DictionaryEntry("IDDATAIDENTIFIER", pParameters["IDDATAIDENTIFIER"].Value));
                    if ((null != pParameters["DATA1"]) && StrFunc.IsFilled(pParameters["DATA1"].Value))
                        lstData.Add(new DictionaryEntry("DATA1", pParameters["DATA1"].Value));
                    if ((null != pParameters["DATA2"]) && StrFunc.IsFilled(pParameters["DATA2"].Value))
                        lstData.Add(new DictionaryEntry("DATA2", pParameters["DATA2"].Value));
                    if ((null != pParameters["DATA3"]) && StrFunc.IsFilled(pParameters["DATA3"].Value))
                        lstData.Add(new DictionaryEntry("DATA3", pParameters["DATA3"].Value));
                    if ((null != pParameters["DATA4"]) && StrFunc.IsFilled(pParameters["DATA4"].Value))
                        lstData.Add(new DictionaryEntry("DATA4", pParameters["DATA4"].Value));
                    if ((null != pParameters["DATA5"]) && StrFunc.IsFilled(pParameters["DATA5"].Value))
                        lstData.Add(new DictionaryEntry("DATA5", pParameters["DATA5"].Value));
                    break;
            }

            // FI 20180328 [23871] Call RemoveHTMLEntities
            List<DictionaryEntry> ret = RemoveHTMLEntities(lstData);
            return ret;
        }


        /// <summary>
        ///  Supprime les caractères réservés du HTML &lt; et &gt; puisque le tracker est affiché sur l'application Web
        /// </summary>
        /// <param name="pDic"></param>
        /// <returns></returns>
        /// FI 20180328 [23871] Add method
        private static List<DictionaryEntry> RemoveHTMLEntities(List<DictionaryEntry> pDic)
        {
            if (null == pDic)
                throw new ArgumentNullException("pDic argument is null");

            // FI 20200916 [XXXXX] test supplémentaire sur (null == item.Value) pour ne pas planter sur cette instruction linq
            List<DictionaryEntry> ret = (from item in pDic
                                         select new DictionaryEntry(item.Key,
                                             ((null == item.Value) || StrFunc.IsEmpty(item.Value.ToString())) ? string.Empty :
                                                       item.Value.ToString().Replace("<", string.Empty).Replace(">", string.Empty))).ToList();

            return ret;
        }

        /// <summary>
        ///  Retourne true si l'attribut <see cref="Cst.TrackerSystemMsgAttribute"/> associé à la valeur <paramref name="pValue"/> de type <typeparamref name="T"/> définit une valeur pour <see cref="Cst.TrackerSystemMsgAttribute.SysNumber_NMF"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20230109 [XXXXX] Add
        private static Boolean ExistSysNumberNMF<T>(string pValue)
        {
            Cst.TrackerSystemMsgAttribute attribute = ReflectionTools.GetAttribute<Cst.TrackerSystemMsgAttribute>(typeof(T), pValue);
            if ((null != attribute) && attribute.SysNumber_NMF > 0)
                return true;
            
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// FI 20230109 [XXXXX] Add
        private static List<DictionaryEntry> BuildInfoNormsgFactoryDefault(NormMsgFactoryMQueue queue)
        {
            List<DictionaryEntry> lstData = new List<DictionaryEntry>();

            string extlId = Cst.NotAvailable;
            if ((null != queue.acknowledgment) && StrFunc.IsFilled(queue.acknowledgment.extlId))
                extlId = queue.acknowledgment.extlId;

            lstData.Add(new DictionaryEntry("DATA1", extlId));
            if (queue.buildingInfo.posRequestTypeSpecified && (Cst.PosRequestTypeEnum.None != queue.buildingInfo.posRequestType))
                lstData.Add(new DictionaryEntry("DATA2", queue.buildingInfo.posRequestType));
            else if (queue.buildingInfo.processType != Cst.ProcessTypeEnum.IO)
                lstData.Add(new DictionaryEntry("DATA2", queue.buildingInfo.processType));

            return lstData;
        }

        #endregion


    }
}
