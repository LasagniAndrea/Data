#region Using Directives
using EFS.ACommon;
using EfsML.Enum;
using System;
using System.Collections.Generic;
#endregion Using Directives

// RD 20120809 [18070] Optimisation
namespace EFS.Process.EarGen
{
    #region public class Pre_EarAmounts
    public class Pre_EarAmounts
    {
        #region Members
        public string IdMarketEnv;
        // Currencies
        public string IdFCU;
        public string IdACU;
        public string IdCU1;
        public string IdCU2;
        // Dates
        public DateTime DtEar;
        public DateTime DtEvent;
        public DateTime DtTransact;
        public DateTime DtPRSDate;
        public DateTime DtValue;
        // IsDateToProcess
        public bool IsDtEventToProcess;
        public bool IsDtTransactToProcess;
        public bool IsDtValueToProcess;
        public bool IsDtPRSDateToProcess;
        public bool IsDtEarToProcess;
        /// <summary>
        /// Montant payé
        /// </summary>
        public decimal Paid;
        /// <summary>
        /// Montant reçu
        /// </summary>
        public decimal Received;
        //
        public Pre_EarAmount FCUAmount;
        //
        public Pre_EarAmount ACUEarDateAmount;
        public Pre_EarAmount ACUEventDateAmount;
        public Pre_EarAmount ACUTransactDateAmount;
        public Pre_EarAmount ACUPRSDateAmount;
        public Pre_EarAmount ACUValueDateAmount;
        //
        public Pre_EarAmount CU1EarDateAmount;
        public Pre_EarAmount CU1EventDateAmount;
        public Pre_EarAmount CU1TransactDateAmount;
        public Pre_EarAmount CU1PRSDateAmount;
        public Pre_EarAmount CU1ValueDateAmount;
        //
        public Pre_EarAmount CU2EarDateAmount;
        public Pre_EarAmount CU2EventDateAmount;
        public Pre_EarAmount CU2TransactDateAmount;
        public Pre_EarAmount CU2PRSDateAmount;
        public Pre_EarAmount CU2ValueDateAmount;
        #endregion Members
        #region Constructors
        public Pre_EarAmounts()
        {
            IsDtEventToProcess = true;
            IsDtTransactToProcess = true;
            IsDtValueToProcess = true;
            IsDtPRSDateToProcess = true;
            IsDtEarToProcess = true;
        }
        public Pre_EarAmounts(DateTime pDtTransact, DateTime pDtEar)
            : this()
        {
            DtEar = pDtEar;
            DtTransact = pDtTransact;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Duplicate this object
        /// </summary>
        public Pre_EarAmounts Clone()
        {
            return (Pre_EarAmounts)this.MemberwiseClone();
        }
        /// <summary>
        /// Alimente Paid et Received
        /// </summary>
        /// <param name="pPaidAmount"></param>
        /// <param name="pReceivedAmount"></param>
        public void SetCounterValuesData(decimal pPaidAmount, decimal pReceivedAmount)
        {
            Paid = pPaidAmount;
            Received = pReceivedAmount;
        }
        /// <summary>
        /// Alimente IdFCU, IdACU, IdCU1, IdCU2, DtPRSDate, DtValue
        /// </summary>
        /// <param name="pIdFCU"></param>
        /// <param name="pIdACU"></param>
        /// <param name="pIdCU1"></param>
        /// <param name="pIdCU2"></param>
        /// <param name="pDtPRSDate"></param>
        /// <param name="pDtValue"></param>
        public void SetCounterValuesData(string pIdFCU, string pIdACU, string pIdCU1, string pIdCU2, DateTime pDtPRSDate, DateTime pDtValue)
        {
            IdFCU = pIdFCU;
            IdACU = pIdACU;
            IdCU1 = pIdCU1;
            IdCU2 = pIdCU2;
            //
            DtPRSDate = pDtPRSDate;
            DtValue = pDtValue;
        }
        /// <summary>
        /// Almente IsDtEventToProcess, IsDtTransactToProcess, IsDtPRSDateToProcess,IsDtEarToProcess
        /// </summary>
        /// <param name="pIsDtEventToProcess"></param>
        /// <param name="pIsDtTransactToProcess"></param>
        /// <param name="pIsDtValueToProcess"></param>
        /// <param name="pIsDtPRSDateToProcess"></param>
        /// <param name="pIsDtEarToProcess"></param>
        public void SetCounterValuesData(bool pIsDtEventToProcess, bool pIsDtTransactToProcess, bool pIsDtValueToProcess, bool pIsDtPRSDateToProcess, bool pIsDtEarToProcess)
        {
            IsDtEventToProcess = pIsDtEventToProcess;
            IsDtTransactToProcess = pIsDtTransactToProcess;
            IsDtValueToProcess = pIsDtValueToProcess;
            IsDtPRSDateToProcess = pIsDtPRSDateToProcess;
            IsDtEarToProcess = pIsDtEarToProcess;
        }
        
        /// <summary>
        /// Reset Amounts, Currencies and Dates
        /// </summary>
        public void ResetAmounts()
        {            
            IdMarketEnv = null;

            IdFCU = string.Empty;
            IdACU = string.Empty;
            IdCU1 = string.Empty;
            IdCU2 = string.Empty;
            
            DtPRSDate = DateTime.MinValue;
            DtValue = DateTime.MinValue;
            DtEvent = DateTime.MinValue;
            
            Paid = 0;
            Received = 0;

            FCUAmount = null;
            
            ACUEarDateAmount = null;
            ACUEventDateAmount = null;
            ACUTransactDateAmount = null;
            ACUPRSDateAmount = null;
            ACUValueDateAmount = null;
            
            CU1EarDateAmount = null;
            CU1EventDateAmount = null;
            CU1TransactDateAmount = null;
            CU1PRSDateAmount = null;
            CU1ValueDateAmount = null;
            
            CU2EarDateAmount = null;
            CU2EventDateAmount = null;
            CU2TransactDateAmount = null;
            CU2PRSDateAmount = null;
            CU2ValueDateAmount = null;
        }
        /// <summary>
        /// Retourne la devise IdACU ou IdCU1 ou IdCU2 ou IdFCU en fonction de {pExchangeType}
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <returns></returns>
        public string GetExchangeTypeIDC(ExchangeTypeEnum pExchangeType)
        {
            string ret;
            //
            switch (pExchangeType)
            {
                case ExchangeTypeEnum.ACU_EARDATE:
                case ExchangeTypeEnum.ACU_EVENTDATE:
                case ExchangeTypeEnum.ACU_TRANSACTDATE:
                case ExchangeTypeEnum.ACU_PRSDATE:
                case ExchangeTypeEnum.ACU_VALUEDATE:
                    ret = IdACU;
                    break;
                case ExchangeTypeEnum.CU1_EARDATE:
                case ExchangeTypeEnum.CU1_EVENTDATE:
                case ExchangeTypeEnum.CU1_TRANSACTDATE:
                case ExchangeTypeEnum.CU1_PRSDATE:
                case ExchangeTypeEnum.CU1_VALUEDATE:
                    ret = IdCU1;
                    break;
                case ExchangeTypeEnum.CU2_EARDATE:
                case ExchangeTypeEnum.CU2_EVENTDATE:
                case ExchangeTypeEnum.CU2_TRANSACTDATE:
                case ExchangeTypeEnum.CU2_PRSDATE:
                case ExchangeTypeEnum.CU2_VALUEDATE:
                    ret = IdCU2;
                    break;
                default:
                    ret = IdFCU;
                    break;
            }
            //
            return ret;
        }
        /// <summary>
        /// Retourne la date DtEar ou DtEvent ou DtTransact ou DtPRSDate ou DtValue en fonction de {pExchangeType}
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <returns></returns>
        public DateTime GetExchangeTypeDate(ExchangeTypeEnum pExchangeType)
        {
            DateTime ret;
            //
            switch (pExchangeType)
            {
                case ExchangeTypeEnum.ACU_EARDATE:
                case ExchangeTypeEnum.CU1_EARDATE:
                case ExchangeTypeEnum.CU2_EARDATE:
                    ret = DtEar;
                    break;
                case ExchangeTypeEnum.ACU_EVENTDATE:
                case ExchangeTypeEnum.CU1_EVENTDATE:
                case ExchangeTypeEnum.CU2_EVENTDATE:
                    ret = DtEvent;
                    break;
                case ExchangeTypeEnum.ACU_TRANSACTDATE:
                case ExchangeTypeEnum.CU1_TRANSACTDATE:
                case ExchangeTypeEnum.CU2_TRANSACTDATE:
                    ret = DtTransact;
                    break;
                case ExchangeTypeEnum.ACU_PRSDATE:
                case ExchangeTypeEnum.CU1_PRSDATE:
                case ExchangeTypeEnum.CU2_PRSDATE:
                    ret = DtPRSDate;
                    break;
                case ExchangeTypeEnum.ACU_VALUEDATE:
                case ExchangeTypeEnum.CU1_VALUEDATE:
                case ExchangeTypeEnum.CU2_VALUEDATE:
                    ret = DtValue;
                    break;
                default:
                    ret = DateTime.Today;
                    break;
            }
            //
            return ret;
        }
        /// <summary>
        /// Retourne IsDtEarToProcess ou IsDtEventToProcess ou IsDtTransactToProcess ou IsDtPRSDateToProcess ou IsDtValueToProcess en fonction de {pExchangeType}
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <returns></returns>
        public bool IsExchangeTypeDateToProcess(ExchangeTypeEnum pExchangeType)
        {
            bool ret;
            //
            switch (pExchangeType)
            {
                case ExchangeTypeEnum.ACU_EARDATE:
                case ExchangeTypeEnum.CU1_EARDATE:
                case ExchangeTypeEnum.CU2_EARDATE:
                    ret = IsDtEarToProcess;
                    break;
                case ExchangeTypeEnum.ACU_EVENTDATE:
                case ExchangeTypeEnum.CU1_EVENTDATE:
                case ExchangeTypeEnum.CU2_EVENTDATE:
                    ret = IsDtEventToProcess;
                    break;
                case ExchangeTypeEnum.ACU_TRANSACTDATE:
                case ExchangeTypeEnum.CU1_TRANSACTDATE:
                case ExchangeTypeEnum.CU2_TRANSACTDATE:
                    ret = IsDtTransactToProcess;
                    break;
                case ExchangeTypeEnum.ACU_PRSDATE:
                case ExchangeTypeEnum.CU1_PRSDATE:
                case ExchangeTypeEnum.CU2_PRSDATE:
                    ret = IsDtPRSDateToProcess;
                    break;
                case ExchangeTypeEnum.ACU_VALUEDATE:
                case ExchangeTypeEnum.CU1_VALUEDATE:
                case ExchangeTypeEnum.CU2_VALUEDATE:
                    ret = IsDtValueToProcess;
                    break;
                default:
                    ret = false;
                    break;
            }
            //
            return ret;
        }
        #endregion Methods
    }
    #endregion Pre_EarAmounts
    #region public class Pre_EarAmount
    public class Pre_EarAmount : IComparable
    {
        #region Members
        private string _exchangeType;
        private string _idC;
        private DateTime _exchangeDate;
        private decimal _paid;
        private decimal _received;
        private DateTime _fxDate;
        //
        private int _idQuote_H;
        private string _idMarketEnv;
        private string _idValScenario;
        private int _idAsset;
        private decimal _fxValue;
        //
        private int _idQuote_H2;
        private string _idMarketEnv2;
        private string _idValScenario2;
        private int _idAsset2;
        private decimal _fxValue2;
        //
        private ProcessStateTools.StatusEnum _idStProcess;
        #endregion Members
        #region Accessors
        public string ExchangeType
        {
            get { return _exchangeType; }
            set { _exchangeType = value; }
        }
        public string IdC
        {
            get { return _idC; }
            set { _idC = value; }
        }
        public DateTime ExchangeDate
        {
            get { return _exchangeDate; }
            set { _exchangeDate = value; }
        }
        public decimal Paid
        {
            get { return _paid; }
            set { _paid = value; }
        }
        public decimal Received
        {
            get { return _received; }
            set { _received = value; }
        }
        public DateTime FxDate
        {
            get { return _fxDate; }
            set { _fxDate = value; }
        }
        //
        public int IdQuote_H
        {
            get { return _idQuote_H; }
            set { _idQuote_H = value; }
        }
        public string IdMarketEnv
        {
            get { return _idMarketEnv; }
            set { _idMarketEnv = value; }
        }
        public string IdValScenario
        {
            get { return _idValScenario; }
            set { _idValScenario = value; }
        }
        public int IdAsset
        {
            get { return _idAsset; }
            set { _idAsset = value; }
        }
        public decimal FxValue
        {
            get { return _fxValue; }
            set { _fxValue = value; }
        }
        //
        public int IdQuote_H2
        {
            get { return _idQuote_H2; }
            set { _idQuote_H2 = value; }
        }
        public string IdMarketEnv2
        {
            get { return _idMarketEnv2; }
            set { _idMarketEnv2 = value; }
        }
        public string IdValScenario2
        {
            get { return _idValScenario2; }
            set { _idValScenario2 = value; }
        }
        public int IdAsset2
        {
            get { return _idAsset2; }
            set { _idAsset2 = value; }
        }
        public decimal FxValue2
        {
            get { return _fxValue2; }
            set { _fxValue2 = value; }
        }
        //
        public ProcessStateTools.StatusEnum IdStProcess
        {
            get { return _idStProcess; }
            set { _idStProcess = value; }
        }
        #endregion Accessors
        #region Constructors
        public Pre_EarAmount() { }
        public Pre_EarAmount(string pExchangeType, string pIdC)
        {
            _exchangeType = pExchangeType;
            _idC = pIdC;
        }
        public Pre_EarAmount(string pExchangeType, string pIdC, decimal pPaidAmount, decimal pReceivedAmount, ProcessStateTools.StatusEnum pIdStProcess)
            : this(pExchangeType, pIdC)
        {
            _paid = pPaidAmount;
            _received = pReceivedAmount;
            _idStProcess = pIdStProcess;
        }
        public Pre_EarAmount(string pExchangeType, string pIdC, decimal pPaidAmount, decimal pReceivedAmount, ProcessStateTools.StatusEnum pIdStProcess,
            int pIdQuote_H)
            : this(pExchangeType, pIdC, pPaidAmount, pReceivedAmount, pIdStProcess)
        {
            _idQuote_H = pIdQuote_H;
        }
        public Pre_EarAmount(string pExchangeType, string pIdC, decimal pPaidAmount, decimal pReceivedAmount, ProcessStateTools.StatusEnum pIdStProcess,
            int pIdQuote_H, int pIdQuote_H2)
            : this(pExchangeType, pIdC, pPaidAmount, pReceivedAmount, pIdStProcess, pIdQuote_H)
        {
            _idQuote_H2 = pIdQuote_H2;
        }
        public Pre_EarAmount(string pExchangeType, string pIdC, DateTime pFxDate, decimal pPaidAmount, decimal pReceivedAmount, ProcessStateTools.StatusEnum pIdStProcess,
            int pIdQuote_H, string pIdMarketEnv, string pIdValScenario, int pIdAsset, decimal pFxValue)
            : this(pExchangeType, pIdC, pPaidAmount, pReceivedAmount, pIdStProcess, pIdQuote_H)
        {
            _idMarketEnv = pIdMarketEnv;
            _idValScenario = pIdValScenario;
            _idAsset = pIdAsset;
            _fxDate = pFxDate;
            _fxValue = pFxValue;
        }
        #endregion Constructors
        #region CompareTo
        public int CompareTo(object obj)
        {
            if (obj is Pre_EarAmount item)
            {
                if ((_exchangeType == item.ExchangeType) && (_idC == item.IdC))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarAmount");
        }
        #endregion CompareTo
    }
    #endregion Pre_EarAmount
    #region public class Pre_EarActorInfo
    public class Pre_EarActorInfo
    {
        #region Members
        public int IdB;
        public string IdB_Identifier;
        public int IdAEntity;
        public string IdAEntity_Identifier;
        public string IdCAccount;
        public string IdMarketEnv;
        #endregion Members
    }
    #endregion public class Pre_EarActorInfo
    #region public class Pre_EarBook
    public class Pre_EarBook : Pre_EarActorInfo
    {
        #region Members
        public int IdEAR;
        
        public DateTime DtEvent;
        /// <summary>
        /// Date d'annulation (renseigné si IdStActivation = REMOVED)
        /// </summary>
        public DateTime DtRemoved;
        /// <summary>
        /// Valeurs possibles REMOVED ou REGULAR
        /// </summary>
        public Cst.StatusActivation IdStActivation;

        public List<Pre_EarDet> EarDets;
        public string PartyIdentifier;
        #endregion Members


        #region Constructors
        public Pre_EarBook(int pIdB, string pIdB_Identifier, int pIdAEntity, string pIdAEntity_Identifier,
            string pIdCAccount, string pIdMarketEnv, DateTime pDtEvent, string pPartyIdentifier)
            : this(pIdB, pIdB_Identifier, pIdAEntity, pIdAEntity_Identifier,
            pIdCAccount, pIdMarketEnv, Cst.StatusActivation.REGULAR, pDtEvent, DateTime.MinValue, pPartyIdentifier) { }

        public Pre_EarBook(Pre_EarBook pEarBook, Cst.StatusActivation pIdStActivation, DateTime pDtEvent, DateTime pDtRemoved)
            : this(pEarBook.IdB, pEarBook.IdB_Identifier, pEarBook.IdAEntity, pEarBook.IdAEntity_Identifier,
            pEarBook.IdCAccount, pEarBook.IdMarketEnv, pIdStActivation, pDtEvent, pDtRemoved, pEarBook.PartyIdentifier) { }

        public Pre_EarBook(int pIdB, string pIdB_Identifier, int pIdAEntity, string pIdAEntity_Identifier,
            string pIdCAccount, string pIdMarketEnv, Cst.StatusActivation pIdStActivation, DateTime pDtEvent, DateTime pDtRemoved, string pPartyIdentifier)
        {
            EarDets = new List<Pre_EarDet>();

            IdB = pIdB;
            IdB_Identifier = pIdB_Identifier;
            IdAEntity = pIdAEntity;
            IdAEntity_Identifier = pIdAEntity_Identifier;
            IdCAccount = pIdCAccount;
            IdMarketEnv = pIdMarketEnv;
            IdStActivation = pIdStActivation;
            DtEvent = pDtEvent;
            DtRemoved = pDtRemoved;
            PartyIdentifier = pPartyIdentifier;
        }
        #endregion Constructors        
        #region Methods        
        #endregion Methods
    }
    #endregion Pre_EarBook
   
    #region Public class Pre_EarBase
    public class Pre_EarBase : IComparable
    {
        #region Members
        
        
        public string earCode;
        public string eventCode;
        public string eventType;
        public string eventClass;
        /// <summary>
        /// Liste des montants
        /// </summary>
        public List<Pre_EarAmount> earAmounts;
        #endregion Members

        #region Constructors
        public Pre_EarBase() 
        {
            earAmounts = new List<Pre_EarAmount>();
        }
        #endregion Constructors
        
        #region Accessors
        public virtual bool IsAgAmountsWithCounterValue
        {
            get
            {
                return false;
            }
        }
        #endregion
        #region Indexors
        public Pre_EarAmount this[string pExchangeType, string pIdC]
        {
            get { return earAmounts.Find(amount => amount.ExchangeType == pExchangeType && amount.IdC == pIdC); }
        }
        #endregion Indexors
        #region Methods
        #region Add
        /// <summary>
        /// Ajoute un pEarAmount s'il n'existe pas 
        /// </summary>
        /// <param name="pEarAmount"></param>
        public void Add(Pre_EarAmount pEarAmount)
        {
            if ((pEarAmount != null) && earAmounts.Find(amount => amount.CompareTo(pEarAmount) == 0) == null)
                earAmounts.Add(pEarAmount);
        }
        #endregion Add        
        #region CompareTo
        public virtual int CompareTo(object obj) { return -1; }
        #endregion CompareTo
        #endregion Methods

    }
    #endregion Public class Pre_EarBase
    #region public class Pre_EarCalc
    public class Pre_EarCalc : Pre_EarBase
    {
        #region Members
        public int idEARCalc;
        public string calcType;
        public DateTime dtAccount;
        public string agFunc;
        public string agAmounts;
        public int sequenceNo;
        /// <summary>
        /// Liste des évènements utilisés
        /// </summary>
        public int[] earCalcEvent;
        #endregion Members
        #region Accessors
        public override bool IsAgAmountsWithCounterValue
        {
            get
            {
                string[] agList;
                if (agAmounts.IndexOf(@"&") > 0)
                    agList = agAmounts.Split('&');
                else
                    agList = agAmounts.Split('|');
                //
                string[] cv = agList[0].Split(';');

                if (cv.Length > 3 && StrFunc.IsFilled(cv[3]))
                    return true;
                else
                {
                    if (agList.Length > 1 && StrFunc.IsFilled(agList[1]))
                    {
                        string[] cv2 = agList[1].Split(';');

                        if (cv2.Length > 3 && StrFunc.IsFilled(cv2[3]))
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
        }
        #endregion
        #region Constructors
        public Pre_EarCalc(string pEarCode, string pCalcType, string pEventClass, DateTime pDtAccount,
             string pAgFunc, string pAgAmounts, int pSequenceNo, int[] pEarCalcEvent)
            : base()
        {
            earCode = pEarCode;
            calcType = pCalcType;
            eventClass = pEventClass;
            dtAccount = pDtAccount;

            agFunc = pAgFunc;
            agAmounts = pAgAmounts;
            sequenceNo = pSequenceNo;
            earCalcEvent = pEarCalcEvent;
        }
        #endregion Constructors
        #region Methods
        #region CompareTo
        /// <summary>
        /// clé constituée de earCode,eventClass,calcType,dtAccount
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int CompareTo(object obj)
        {
            if (obj is Pre_EarCalc item)
            {
                if ((earCode == item.earCode) && (eventClass == item.eventClass) &&
                    (calcType == item.calcType) && (dtAccount == item.dtAccount))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarCalc");
        }
        #endregion CompareTo
        #endregion Methods
    }
    #endregion Pre_EarDay
    #region public class Pre_EarCommon
    public class Pre_EarCommon : Pre_EarBase
    {
        #region Members
        public int idEARCommon;

        public int idEARDay;
        public DateTime dtEvent;
        #endregion Members
        #region Constructors
        public Pre_EarCommon(int pIdEARDay, string pEarCode, string pEventCode, string pEventType, string pEventClass, DateTime pDtEvent)
            : base()
        {
            idEARDay = pIdEARDay;
            earCode = pEarCode;
            eventCode = pEventCode;
            eventType = pEventType;
            eventClass = pEventClass;
            dtEvent = pDtEvent;
        }
        #endregion Constructors
        #region Methods
        #region CompareTo
        public override int CompareTo(object obj)
        {
            if (obj is Pre_EarCommon item)
            {
                if ((earCode == item.earCode) && (eventCode == item.eventCode) &&
                    (eventClass == item.eventClass) && (eventType == item.eventType) &&
                    (idEARDay == item.idEARDay))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarCommon");

        }
        #endregion CompareTo
        #endregion Methods
    }
    #endregion Pre_EarCommon
    #region public class Pre_EarDay
    public class Pre_EarDay : Pre_EarBase
    {
        #region Members
        
        public int idEARDay;
        
        /// <summary>
        /// Identifiant non significatif de l'EVENT
        /// </summary>
        public int idE;
        
        /// <summary>
        /// Identifiant non significatif de l'EVENTCLASS
        /// </summary>
        public int idEC;
        
        /// <summary>
        /// Date associée à l'EVENTCLASS
        /// </summary>
        public DateTime dtEvent;
        #endregion Members

        #region Constructors
        public Pre_EarDay(string pEarCode, string pEventCode, string pEventType, string pEventClass, int pIdEC, int pIdE, DateTime pDtEvent)
            : base()
        {
            earCode = pEarCode;
            eventCode = pEventCode;
            eventType = pEventType;
            eventClass = pEventClass;
            idEC = pIdEC;
            idE = pIdE;
            dtEvent = pDtEvent;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int CompareTo(object obj)
        {
            if (obj is Pre_EarDay item)
            {
                if ((earCode == item.earCode) && (eventCode == item.eventCode) &&
                    (eventClass == item.eventClass) && (eventType == item.eventType) &&
                    (idEC == item.idEC))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarDay");

        }
        
        #endregion Methods
    }
    #endregion Pre_EarDay
    #region public class Pre_EarNom
    public class Pre_EarNom : Pre_EarBase
    {
        #region Members
        public int idEARNom;
        public DateTime dtAccount;
        public int[] earNomEvent;
        #endregion Members
        #region Constructors
        public Pre_EarNom(string pEventCode, string pEventType, string pEventClass, DateTime pDtAccount, int[] pEarNomEvent)
            : base()
        {
            eventCode = pEventCode;
            eventType = pEventType;
            eventClass = pEventClass;
            dtAccount = pDtAccount;
            earNomEvent = pEarNomEvent;
        }
        #endregion Constructors
        #region Methods
        #region CompareTo
        public override int CompareTo(object obj)
        {
            if (obj is Pre_EarNom item)
            {
                if ((eventCode == item.eventCode) && (eventClass == item.eventClass) &&
                    (eventType == item.eventType) && (dtAccount == item.dtAccount))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarNom");

        }
        #endregion CompareTo
        #endregion Methods
    }
    #endregion Pre_EarNom
    
    #region public class Pre_EarDet
    /// <summary>
    /// 
    /// </summary>
    public class Pre_EarDet
    {
        #region Members
        public int instrumentNo;
        public int streamNo;

        public List<Pre_EarDay> EarDays;
        public List<Pre_EarCommon> EarCommons;
        public List<Pre_EarCalc> EarCalcs;
        public List<Pre_EarNom> EarNoms;
        #endregion Members
        #region Constructors
        public Pre_EarDet() 
        {
            EarDays = new List<Pre_EarDay>();
            EarNoms = new List<Pre_EarNom>();
            EarCommons = new List<Pre_EarCommon>();
            EarCalcs = new List<Pre_EarCalc>();
        }
        public Pre_EarDet(int pInstrumentNo, int pStreamNo)
            : this()
        {
            instrumentNo = pInstrumentNo;
            streamNo = pStreamNo;
        }
        #endregion Constructors

        #region Indexors
        /* FI 20151027 [21513] Mise en commentaire cet indexor n'a plus de sens 
        /// <summary>
        /// Recherche dans EarCalcs
        /// </summary>
        /// <param name="pEarCode"></param>
        /// <param name="pCalcType"></param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtAccount"></param>
        /// <returns></returns>
        public Pre_EarCalc this[string pEarCode, string pCalcType, string pEventClass, DateTime pDtAccount]
        {
            get
            {
                return EarCalcs.Find(earCalc => (earCalc.earCode == pEarCode) &&
                    (earCalc.calcType == pCalcType) &&
                    (earCalc.eventClass == pEventClass) &&
                    (earCalc.dtAccount == pDtAccount));
            }
        }
        */

        /// <summary>
        /// Recherche dans EarCommons
        /// </summary>
        /// <param name="pIdEARDay"></param>
        /// <param name="pEarCode"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pEventClass"></param>
        /// <returns></returns>
        public Pre_EarCommon this[int pIdEARDay, string pEarCode, string pEventCode, string pEventType, string pEventClass]
        {
            get
            {
                return EarCommons.Find(earCommon => (earCommon.idEARDay == pIdEARDay) &&
                    (earCommon.earCode == pEarCode) &&
                    (earCommon.eventCode == pEventCode) &&
                    (earCommon.eventClass == pEventClass) &&
                    (earCommon.eventType == pEventType));
            }
        }
        /// <summary>
        /// Recherche dans EarDays
        /// </summary>
        /// <param name="pEarCode"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pEventClass"></param>
        /// <param name="pIdEC"></param>
        /// <returns></returns>
        public Pre_EarDay this[string pEarCode, string pEventCode, string pEventType, string pEventClass, int pIdEC]
        {
            get
            {
                return EarDays.Find(earDay => (earDay.earCode == pEarCode) &&
                    (earDay.eventCode == pEventCode) &&
                    (earDay.eventClass == pEventClass) &&
                    (earDay.eventType == pEventType) &&
                    (earDay.idEC == pIdEC));
            }
        }
        /// <summary>
        /// Recherche dans EarNoms
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pDtAccount"></param>
        /// <returns></returns>
        public Pre_EarNom this[string pEventCode, string pEventType, DateTime pDtAccount]
        {
            get
            {
                return EarNoms.Find(earNom => (earNom.eventCode == pEventCode) &&
                    (earNom.eventType == pEventType) &&
                    (earNom.dtAccount == pDtAccount));
            }
        }
        #endregion Indexors
        #region Methods
        public int CompareTo(object obj)
        {
            if (obj is Pre_EarDet item)
            {
                if ((instrumentNo == item.instrumentNo) && (streamNo == item.streamNo))
                    return 0;
                return -1;
            }
            throw new ArgumentException("object is not a Pre_EarDet");
        }
        #endregion Methods
    }
    #endregion Pre_EarDet

    #region public class CounterValueData
    public class CounterValueData
    {
        #region Members
        public string IdMarketEnv;
        // Currencies
        public string IdFCU;
        public string IdACU;
        public string IdCU1;
        public string IdCU2;
        // Dates
        public DateTime DtEar;
        public DateTime DtEvent;
        public DateTime DtTransact;
        public DateTime DtValue;
        #endregion Members
        #region Constructors
        public CounterValueData() { }
        public CounterValueData(DateTime pDtTransact, /*DateTime pDtValue,*/ DateTime pDtEar)
        {
            InitCurrencies();
            //
            DtEar = pDtEar;
            DtEvent = DateTime.MinValue;
            DtTransact = pDtTransact;
            DtValue = DateTime.MinValue;//pDtValue;
        }
        #endregion Constructors
        #region Methods
        public void InitCurrencies()
        {
            InitCurrencies(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue);
        }
        public void InitCurrencies(string pIdFCU, string pIdACU, string pIdCU1, string pIdCU2, DateTime pDtValue)
        {
            IdFCU = pIdFCU;
            IdACU = pIdACU;
            IdCU1 = pIdCU1;
            IdCU2 = pIdCU2;
            //
            DtValue = pDtValue;
        }
        //
        public string GetExchangeTypeIDC(ExchangeTypeEnum pExchangeType)
        {
            string exchangeTypeIDC;
            //
            switch (pExchangeType)
            {
                case ExchangeTypeEnum.ACU_EARDATE:
                case ExchangeTypeEnum.ACU_EVENTDATE:
                case ExchangeTypeEnum.ACU_TRANSACTDATE:
                case ExchangeTypeEnum.ACU_VALUEDATE:
                    exchangeTypeIDC = IdACU;
                    break;
                case ExchangeTypeEnum.CU1_EARDATE:
                case ExchangeTypeEnum.CU1_EVENTDATE:
                case ExchangeTypeEnum.CU1_TRANSACTDATE:
                case ExchangeTypeEnum.CU1_VALUEDATE:
                    exchangeTypeIDC = IdCU1;
                    break;
                case ExchangeTypeEnum.CU2_EARDATE:
                case ExchangeTypeEnum.CU2_EVENTDATE:
                case ExchangeTypeEnum.CU2_TRANSACTDATE:
                case ExchangeTypeEnum.CU2_VALUEDATE:
                    exchangeTypeIDC = IdCU2;
                    break;
                default:
                    exchangeTypeIDC = IdFCU;
                    break;
            }
            //
            return exchangeTypeIDC;
        }
        public DateTime GetExchangeTypeDate(ExchangeTypeEnum pExchangeType)
        {
            DateTime exchangeTypeDate;
            //
            switch (pExchangeType)
            {
                case ExchangeTypeEnum.ACU_EARDATE:
                case ExchangeTypeEnum.CU1_EARDATE:
                case ExchangeTypeEnum.CU2_EARDATE:
                    exchangeTypeDate = DtEar;
                    break;
                case ExchangeTypeEnum.ACU_EVENTDATE:
                case ExchangeTypeEnum.CU1_EVENTDATE:
                case ExchangeTypeEnum.CU2_EVENTDATE:
                    exchangeTypeDate = DtEvent;
                    break;
                case ExchangeTypeEnum.ACU_TRANSACTDATE:
                case ExchangeTypeEnum.CU1_TRANSACTDATE:
                case ExchangeTypeEnum.CU2_TRANSACTDATE:
                    exchangeTypeDate = DtTransact;
                    break;
                case ExchangeTypeEnum.ACU_VALUEDATE:
                case ExchangeTypeEnum.CU1_VALUEDATE:
                case ExchangeTypeEnum.CU2_VALUEDATE:
                    exchangeTypeDate = DtValue;
                    break;
                default:
                    exchangeTypeDate = DateTime.Today;
                    break;
            }
            //
            return exchangeTypeDate;
        }
        #endregion
    }
    #endregion public class CounterValueData
}
