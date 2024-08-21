using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Permission;
using EFS.Status;
using EfsML.Enum;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EFS.Tuning
{
    #region public Enum
    /// <summary>
    /// classe de tuning (Pour effectuer ou pour ignorer)
    /// </summary>
    /// FI 20190524 [23912] Add
    public enum TuningInputClassEnum
    {
        /// <summary>
        /// Tuning pour ignorer une action ou un traitement   
        /// </summary>
        Ignore,

        /// <summary>
        /// Tuning pour activer une action ou un traitement   
        /// </summary>
        Process
    }

    /// <summary>
    /// Type de tuning (Pour effectuer ou pour ignorer)
    /// <para>ITP : Input Trade for Process</para>
    /// <para>ITI : Input Trade for Ignore</para>
    /// <para>IEP : Input Event for Process</para>
    /// <para>IEI : Input Event for Ignore</para>
    /// </summary>
    public enum TuningInputTypeEnum
    {
        /// <summary>
        /// Input Trade for Process
        /// </summary>
        ITP,
        /// <summary>
        /// Input Trade for Ignore
        /// </summary>
        ITI,
        /// <summary>
        /// Input Event for Process
        /// </summary>
        IEP,
        /// <summary>
        /// Input Event for Ignore
        /// </summary>
        IEI,
    }
    public enum TuningOutputTypeEnum
    {
        /// <summary>
        /// Output Trade on Success
        /// </summary>
        OTS,
        /// <summary>
        /// Output Trade on Error
        /// </summary>
        OTE,
        /// <summary>
        /// Output Event on Success
        /// </summary>
        OES,
        /// <summary>
        ///  Output Event on Error
        /// </summary>
        OEE,
    }
    #endregion Enum

    /// <summary>
    /// La classe "TuningInputBase" est destinée à recevoir et manipuler une partie des données (les données "Input") issues d'une table TUNING
    /// eg: Table: ACTIONTUNING Colonnes: CTRLUSERTRADE, MINSTCHECK, CTRLMINSTCHECK, ..., CTRLCNFMSGGEN ...
    /// </summary>
    public abstract class TuningInputBase
    {
        #region Members
        /// <summary>
        /// Enregistrement SQL du tuning
        /// </summary>
        protected DataRow _dr;
        /// <summary>
        /// Suffixe pour les colonnes SQL  
        /// <para>Utiliser pour ProcessTuning (eg: ITI, ITP,...IEI)</para>
        /// </summary>
        protected string _suffix = string.Empty;
        /// <summary>
        /// Type de tuning
        /// </summary>
        /// FI 20190524 [23912] Add
        protected TuningInputClassEnum _class = TuningInputClassEnum.Process;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient true s'il existe un paramétrage 
        /// </summary>
        protected bool IsDrSpecified
        {
            get { return (_dr != null); }
        }

        /// <summary>
        /// Obtient true s'il existe un paramétrage renseigné
        /// </summary>
        public bool ExistDataSetting
        {
            get
            {

                //Check s'il existe un statut d'ACTIVATION de paramétré
                bool ret = IdStatusActivationSpecified;

                if (!ret)
                {
                    //Check s'il existe au moins un statut USER (CHECK ou MATCH) de paramétré
                    foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)))
                    {
                        if (StatusTools.IsStatusUser(statusEnum))
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                MinMaxTuningEnum minMaxTuning = ((i == 0) ? MinMaxTuningEnum.MIN : MinMaxTuningEnum.MAX);
                                ret |= IsStatusMinMaxSpecified(statusEnum, minMaxTuning);
                            }
                        }
                    }
                }

                if (!ret)
                {
                    //Check s'il existe au moins un état de PROCESS (SIGEN, ACCOUNTGEN, ...) de paramétré
                    foreach (Cst.ProcessTypeEnum processTypeEnum in Enum.GetValues(typeof(Cst.ProcessTypeEnum)))
                    {
                        if (IsControlRunProcessSpecified(processTypeEnum))
                        {
                            ret = true;
                            break;
                        }
                    }
                }

                return ret;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// FI 20190524 [23912] Add 
        public TuningInputBase(DataRow pDr)
            : this(pDr, null)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        ///<param name="pType"></param>
        /// FI 20190524 [23912] Chgt de signature 
        public TuningInputBase(DataRow pDr, Nullable<TuningInputTypeEnum> pType)
        {
            _dr = pDr;

            _class = TuningInputClassEnum.Process;
            if (pType.HasValue)
            {
                _class = TuningTools.GetTuningClass(pType.Value);
                _suffix = StrFunc.AppendFormat("_{0}", pType.Value);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        ///Les "virtual methods" sont présentes pour mettre à disposition des valeur par défaut pour les colonnes inexistantes 
        ///dans certaines tables Tuning (eg: ACTIONTUNING.IDSTACTIVATION). Elles sont "overrided" dans les classes principales.
        protected virtual Cst.CtrlLastUserEnum CtrlUserTrade
        {
            get { return Cst.CtrlLastUserEnum.INDIFFERENT; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool IdStatusActivationSpecified
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual Cst.StatusActivation IdStatusActivation
        {
            get { return Cst.StatusActivation.REGULAR; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStatus"></param>
        /// <param name="pLastIda"></param>
        /// <param name="pCurrentIda"></param>
        /// <param name="pMsgControl"></param>
        /// <returns></returns>
        /// FI 20161114 [RATP] Modify
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public virtual ErrorLevelTuningEnum ScanTradeCompatibility(string pCS, int pIdT, CommonStatus pStatus, int pLastTradeIda, int pCurrentIda, out string pMsgControl)
        {
            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;
            pMsgControl = string.Empty;

            if (IsDrSpecified)
            {
                #region Compatibilité des statuts du Trade
                switch (CtrlUserTrade)
                {
                    case Cst.CtrlLastUserEnum.DIFFERENT:
                        if (pLastTradeIda == pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERTRADE;
                            pMsgControl += Ressource.GetString("Msg_UserTradeDifferent_NotOk") + Cst.CrLf;
                        }
                        break;
                    case Cst.CtrlLastUserEnum.IDENTIC:
                        if (pLastTradeIda != pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERTRADE;
                            pMsgControl += Ressource.GetString("Msg_UserTradeIdentic_NotOk") + Cst.CrLf;
                        }
                        break;
                }

                if (IdStatusActivationSpecified)
                {
                    if (pStatus.stActivation.CurrentSt != IdStatusActivation.ToString())
                    {
                        errLevel = ErrorLevelTuningEnum.ERR_ACTIVATION;
                        pMsgControl += Ressource.GetString("Msg_StatusActivation_" + _class + "_NotOk") + Cst.CrLf;
                    }
                }

                // statuts Users pour lesquels un poids min ou max a été renseigné
                foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                    .Where(x => StatusTools.IsStatusUser(x)))
                {
                    foreach (MinMaxTuningEnum item in Enum.GetValues(typeof(MinMaxTuningEnum)).Cast<MinMaxTuningEnum>()
                        .Where(x => IsStatusMinMaxSpecified(statusEnum, x)))
                    {
                        int stValue = GetStatusMinMaxValue(statusEnum, item);
                        Cst.CtrlStatusEnum ctrlContext = CtrlStatusMinMaxValue(statusEnum, item);

                        errLevel = pStatus.ScanUserStatusCompatibility(statusEnum, item, stValue, ctrlContext);

                        if (ErrorLevelTuningEnum.SUCCESS != errLevel)
                            pMsgControl += Ressource.GetString("Msg_" + statusEnum.ToString() + "_" + item.ToString() + "_" + _class + "_NotOk") + Cst.CrLf;
                    }
                }
                #endregion

                #region Compatibilité des process subient par les évènements du Trade
                // FI 20161114 [RATP] exclusion de  ESRSTDGEN et ESRNETGEN (Le test s'effectue sur ESR)
                foreach (Cst.ProcessTypeEnum processTypeEnum in Enum.GetValues(typeof(Cst.ProcessTypeEnum)).Cast<Cst.ProcessTypeEnum>()
                            .Where(x => IsControlRunProcessSpecified(x) && (x != Cst.ProcessTypeEnum.ESRSTDGEN) && (x != Cst.ProcessTypeEnum.ESRNETGEN)))
                {
                    Cst.CtrlSendingEnum ctrlSending = GetControlRunProcessValue(processTypeEnum);

                    errLevel = TuningTools.ScanTradeEventProcess(pCS, pIdT, processTypeEnum, ctrlSending);
                    if (errLevel != ErrorLevelTuningEnum.SUCCESS)
                    {

                        if (processTypeEnum == Cst.ProcessTypeEnum.CMGEN)
                            pMsgControl += Ressource.GetString("Msg_NOTIFICATIONMSGGEN" + "_" + ctrlSending.ToString() + "_" + _class + "_NotOk") + Cst.CrLf;
                        else
                            pMsgControl += Ressource.GetString("Msg_" + processTypeEnum.ToString() + "_" + ctrlSending.ToString() + "_" + _class + "_NotOk") + Cst.CrLf;
                    }
                }
                #endregion
            }

            if (StrFunc.IsFilled(pMsgControl))
            {
                //Il existe au moins un problème de cohérence
                errLevel = ErrorLevelTuningEnum.ERROR;
            }

            return errLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdE"></param>
        /// <param name="pStatus"></param>
        /// <param name="pMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl 
        public virtual ErrorLevelTuningEnum ScanEventCompatibility(string pCS, int pIdE, CommonStatus pStatus, out string pMsgControl)
        {
            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;
            pMsgControl = string.Empty;

            if (IsDrSpecified)
            {
                #region Compatibilité des statuts de l'événement

                if (IdStatusActivationSpecified)
                {
                    if (pStatus.stActivation.CurrentSt != IdStatusActivation.ToString())
                    {
                        errLevel = ErrorLevelTuningEnum.ERR_ACTIVATION;
                        pMsgControl += Ressource.GetString("Msg_StatusActivation_" + _class + "_NotOk") + Cst.CrLf;
                    }
                }

                foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                    .Where(x => StatusTools.IsStatusUser(x)))
                {
                    foreach (MinMaxTuningEnum item in Enum.GetValues(typeof(MinMaxTuningEnum)).Cast<MinMaxTuningEnum>()
                       .Where(x => IsStatusMinMaxSpecified(statusEnum, x)))
                    {
                        int stValue = GetStatusMinMaxValue(statusEnum, item);
                        Cst.CtrlStatusEnum ctrlContext = CtrlStatusMinMaxValue(statusEnum, item);
                        errLevel = pStatus.ScanUserStatusCompatibility(statusEnum, item, stValue, ctrlContext);
                        if (ErrorLevelTuningEnum.SUCCESS != errLevel)
                            pMsgControl += Ressource.GetString("Msg_" + statusEnum.ToString() + "_" + item.ToString() + "_" + _class + "_NotOk") + Cst.CrLf;
                    }
                }
                #endregion

                #region Compatibilité en fonction des process subis par les évènements du Trade
                // FI 20161114 [RATP] exclusion de  ESRSTDGEN et ESRNETGEN (Le test s'effectue sur ESR)
                foreach (Cst.ProcessTypeEnum processTypeEnum in Enum.GetValues(typeof(Cst.ProcessTypeEnum)).Cast<Cst.ProcessTypeEnum>()
                    .Where(x => IsControlRunProcessSpecified(x) && (x != Cst.ProcessTypeEnum.ESRSTDGEN) && (x != Cst.ProcessTypeEnum.ESRNETGEN)))
                {
                    Cst.CtrlSendingEnum ctrlSending = GetControlRunProcessValue(processTypeEnum);

                    errLevel = TuningTools.ScanEventProcess(pCS, pIdE, processTypeEnum, ctrlSending);
                    if (errLevel != ErrorLevelTuningEnum.SUCCESS)
                        pMsgControl += Ressource.GetString("Msg_" + processTypeEnum.ToString() + "_" + ctrlSending.ToString() + "_" + _class + "_NotOk") + Cst.CrLf;
                }
                #endregion
            }

            if (StrFunc.IsFilled(pMsgControl))
            {
                //Il existe au moins un problème de cohérence
                errLevel = ErrorLevelTuningEnum.ERROR;
            }

            return errLevel;
        }

        /// <summary>
        /// Retourne true si la colonne existe {pStatusEnum}+{pMinMaxTuning} existe et est renseignée
        /// <para>Exemple : MINSTCHECK</para>
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <param name="pMinMaxTuning"></param>
        /// <returns></returns>
        protected bool IsStatusMinMaxSpecified(StatusEnum pStatusEnum, MinMaxTuningEnum pMinMaxTuning)
        {
            bool ret = false;
            try
            {
                if (IsDrSpecified)
                {

                    string columnName = pMinMaxTuning.ToString() + StatusTools.GetTableNameStatusUser(pStatusEnum);//eg: MIN + STCHECK
                    ret = (_dr[columnName + _suffix] != Convert.DBNull);
                }
            }
            catch
            { }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <param name="pMinMaxTuning"></param>
        /// <returns></returns>
        protected int GetStatusMinMaxValue(StatusEnum pStatusEnum, MinMaxTuningEnum pMinMaxTuning)
        {
            int ret = 0;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = pMinMaxTuning.ToString() + StatusTools.GetTableNameStatusUser(pStatusEnum);//eg: MIN + STCHECK
                    ret = Convert.ToInt32(_dr[columnName + _suffix]);
                }
            }
            catch
            { }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <param name="pMinMaxTuning"></param>
        /// <returns></returns>
        protected Cst.CtrlStatusEnum CtrlStatusMinMaxValue(StatusEnum pStatusEnum, MinMaxTuningEnum pMinMaxTuning)
        {
            Cst.CtrlStatusEnum ret = Cst.CtrlStatusEnum.EVERY;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = "CTRL" + pMinMaxTuning.ToString() + StatusTools.GetTableNameStatusUser(pStatusEnum);//eg: CTRL + MIN + STCHECK
                    ret = (Cst.CtrlStatusEnum)System.Enum.Parse(typeof(Cst.CtrlStatusEnum), _dr[columnName + _suffix].ToString());
                }
            }
            catch
            { }
            return ret;
        }

        /// <summary>
        /// Retourne true si le paramétrage pour le process {pProcessTypeEnum} est différent de INDIFFERENT
        /// </summary>
        /// <param name="pProcessTypeEnum"></param>
        /// <returns></returns>
        protected bool IsControlRunProcessSpecified(Cst.ProcessTypeEnum pProcessTypeEnum)
        {
            bool ret = false;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = GetProcessColumn(pProcessTypeEnum);
                    if (_dr.Table.Columns.Contains(columnName + _suffix))
                    {
                        ret = (_dr[columnName + _suffix].ToString() != Cst.CtrlSendingEnum.INDIFFERENT.ToString());
                    }
                }
            }
            catch
            {
                // EG 20160404 Migration vs2013
                // #warning Avant chaque sortie de release verifier ici la necessite d'ajouter de nouvelles colonnes
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessTypeEnum"></param>
        /// <returns></returns>
        protected Cst.CtrlSendingEnum GetControlRunProcessValue(Cst.ProcessTypeEnum pProcessTypeEnum)
        {
            Cst.CtrlSendingEnum ret = Cst.CtrlSendingEnum.INDIFFERENT;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = GetProcessColumn(pProcessTypeEnum);
                    ret = (Cst.CtrlSendingEnum)System.Enum.Parse(typeof(Cst.CtrlSendingEnum), _dr[columnName + _suffix].ToString());
                }
            }
            catch
            {
                // EG 20160404 Migration vs2013
                // #warning Avant chaque sortie de release verifier ici la necessite d'ajouter de nouvelles colonnes
            }
            return ret;
        }

        /// <summary>
        /// Retourne le nom de colonne qui se rapporte au process {pProcessTypeEnum}
        /// </summary>
        /// <param name="pProcessTypeEnum"></param>
        /// <returns></returns>
        /// FI 20161114 [RATP] Modify
        private string GetProcessColumn(Cst.ProcessTypeEnum pProcessTypeEnum)
        {
            switch (pProcessTypeEnum)
            {
                case Cst.ProcessTypeEnum.RIMGEN:
                    //FI 20110927 Il n'existe pas de colonne CTRLRIMGEN
                    //(la colonne CNFMSGGEN représente la messagerie de notification en général (Conf + avis d'opéré, EOD)
                    pProcessTypeEnum = Cst.ProcessTypeEnum.CMGEN;
                    break;
                case Cst.ProcessTypeEnum.ESRNETGEN:
                case Cst.ProcessTypeEnum.ESRSTDGEN:
                    // FI 20161114 [RATP] Modify 
                    pProcessTypeEnum = Cst.ProcessTypeEnum.ESRGEN;
                    break;
            }
            return "CTRL" + pProcessTypeEnum.ToString();
        }

    }

    /// <summary>
    /// La classe "TuningOutputBase" est destinée à recevoir et manipuler une partie des données (les données "Output") issues d'une table TUNING
    /// eg: Table: PROCESSTUNING Colonnes: IDSTACTIVATION_OTS, IDSTPRIORITY_OTS, ..., SENDMESSAGE_PPS, ...
    /// </summary>
    public abstract class TuningOutputBase
    {
        protected DataRow _dr;
        protected string _suffix = string.Empty;
        //
        #region Constructors
        public TuningOutputBase(DataRow pDr, string pSuffix)
        {
            _dr = pDr;
            _suffix = (StrFunc.IsFilled(pSuffix) ? "_" + pSuffix : string.Empty); //Utiliser pour ProcessTuning (eg: ITI, ITP)
        }
        #endregion
        //
        #region Private methods
        //PL 20120824 Tuning 
        private bool SetSendMessage(string pSendMessageName, ref Cst.ProcessTypeEnum opSendMessage)
        {
            bool ret = false;
            if (_dr[pSendMessageName] != Convert.DBNull)
            {
                opSendMessage = (Cst.ProcessTypeEnum)System.Enum.Parse(typeof(Cst.ProcessTypeEnum), _dr[pSendMessageName].ToString());
                ret = (opSendMessage != Cst.ProcessTypeEnum.NA);
            }
            return ret;
        }
        #endregion
        #region Public methods
        public bool IdStActivationSpecified
        {
            get
            {
                bool ret = false;
                try
                {
                    if (IsDrSpecified)
                        ret = (_dr["IDSTACTIVATION" + _suffix] != Convert.DBNull);
                }
                catch
                { }
                return ret;
            }
        }
        public Cst.StatusActivation IdStActivation
        {
            get
            {
                Cst.StatusActivation ret = Cst.StatusActivation.REGULAR;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.StatusActivation)System.Enum.Parse(typeof(Cst.StatusActivation), _dr["IDSTACTIVATION" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        public bool IdStPrioritySpecified
        {
            get
            {
                bool ret = false;
                try
                {
                    if (IsDrSpecified)
                        ret = (_dr["IDSTPRIORITY" + _suffix] != Convert.DBNull);
                }
                catch
                { }
                return ret;
            }
        }
        public Cst.StatusPriority IdStPriority
        {
            get
            {
                Cst.StatusPriority ret = Cst.StatusPriority.REGULAR;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.StatusPriority)System.Enum.Parse(typeof(Cst.StatusPriority), _dr["IDSTPRIORITY" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        //
        public bool IdStSpecified(StatusEnum pStatusEnum)
        {
            bool ret = false;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = StatusTools.GetColumnNameStatusUser(pStatusEnum);//eg: IDSTCHECK
                    ret = (_dr[columnName + _suffix] != Convert.DBNull);
                }
            }
            catch
            { }
            return ret;
        }
        public string IdSt(StatusEnum pStatusEnum)
        {
            string ret = string.Empty;
            try
            {
                if (IsDrSpecified)
                {
                    string columnName = StatusTools.GetColumnNameStatusUser(pStatusEnum);//eg: IDSTCHECK
                    ret = _dr[columnName + _suffix].ToString();
                }
            }
            catch
            { }
            return ret;
        }

        //PL 20120824 Tuning 
        public bool SendMessage(ref Cst.ProcessTypeEnum[] opSendMessageTypeArray)
        {
            //NB: On arrête dès que l'on trouve un paramétrage "vide".
            bool ret = false;

            if (IsDrSpecified)
            {
                if (SetSendMessage("SENDMESSAGE_PP" + _suffix.Substring(3, 1) + "1", ref opSendMessageTypeArray[0]))
                {
                    ret = true;
                    if (SetSendMessage("SENDMESSAGE_PP" + _suffix.Substring(3, 1) + "2", ref opSendMessageTypeArray[1]))
                    {
                        SetSendMessage("SENDMESSAGE_PP" + _suffix.Substring(3, 1) + "3", ref opSendMessageTypeArray[2]);
                    }
                }
            }

            return ret;
        }

        protected bool IsDrSpecified
        {
            get { return (_dr != null); }
        }
        #endregion
    }

    /// <summary>
    /// La classe "TuningBase" est destinée à recevoir et exploiter les données issues d'une table TUNING
    /// eg: Table: ACTIONTUNING Colonnes: CTRLUSERTRADE, MINSTCHECK, CTRLMINSTCHECK, ..., CTRLCNFMSGGEN, ...
    /// </summary>
    // EG 20180205 [23769] Add dbTransaction  
    public abstract class TuningBase
    {
        #region Membre
        protected DataRow _dr;
        protected int _idI;
        protected IDbTransaction _dbTransaction;
        #endregion Membre

        #region Accessor
        /// <summary>
        /// 
        /// </summary>
        public bool DrSpecified
        {
            get { return (null != _dr); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataRow Dr
        {
            get { return _dr; }
        }
        #endregion

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public TuningBase(int pIdI) : this(pIdI, null) { }
        // EG 20180205 [23769] Add dbTransaction  
        public TuningBase(int pIdI, IDbTransaction pDbTransaction)
        {
            _idI = pIdI;
            _dbTransaction = pDbTransaction;
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        public virtual void SetDrTuning(string pCS)
        {
            _dr = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        protected static string GetSubSqlTuning_GINSTR(string pCS, string pAliasTable, Nullable<int> pIdGInstr)
        {
            string ret = pAliasTable + ".IDGINSTR";

            if (pIdGInstr == null)
            {
                ret += SQLCst.IS_NULL;
            }
            else
            {
                ret += " in (" + Cst.CrLf;

                ret += SQLCst.SELECT + "gi.IDGINSTR";
                ret += SQLCst.FROM_DBO + Cst.OTCml_TBL.GINSTR.ToString() + " gi " + Cst.CrLf;

                ret += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GINSTRROLE.ToString() + " gir on gir.IDGINSTR=gi.IDGINSTR" + Cst.CrLf;
                ret += SQLCst.AND + "gir.IDROLEGINSTR=" + DataHelper.SQLString(RoleGInstr.TUNING.ToString()) + Cst.CrLf;
                ret += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gir");

                ret += SQLCst.WHERE + "gi.IDGINSTR=" + pIdGInstr.ToString() + Cst.CrLf;
                ret += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gi") + Cst.CrLf;

                ret += ")";
            }
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class TuningUserBase : TuningBase
    {
        #region Members
        protected int _idA;
        protected ActorAncestor _actorAncestor;
        #endregion Members

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public TuningUserBase(int pIdI, int pIdA, ActorAncestor pActorAncestor)
            : this(pIdI, pIdA, pActorAncestor, null) { }
        // EG 20180205 [23769] Add dbTransaction  
        public TuningUserBase(int pIdI, int pIdA, ActorAncestor pActorAncestor, IDbTransaction pDbTransaction)
            : base(pIdI, pDbTransaction)
        {
            _idA = pIdA;
            _actorAncestor = pActorAncestor;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public override void SetDrTuning(string pCS)
        {

            #region Declare variable
            DataSet dataSet = null;
            DataRow dataRow = null;
            #endregion Declare

            //Exist GINSTR => Permet de réduire la complexité des query 
            // Si l'instrument n'appartient pas un un groupe les restrictions se feront directement avec IDGINSTR is null 
            int? idGInstr = InstrTools.GetIdGInstr(pCS, _dbTransaction, _idI, RoleGInstr.TUNING);
            //
            #region Recherche des defaults paramétrés dans ACTIONTUNING
            // Methode => Recherche sur Acteur Courant + Groupe Instrument 
            // sinon      Recherche sur Acteur Courant + Groupe Instrument "null"
            // sinon      Recherche sur les Parents    + Groupe Instrument 
            // sinon      Recherche sur les Parents    + Groupe Instrument "null" et ..... (Boucle sue les parents)
            // sinon   => Recherche sur Acteur "null"  + Groupe Instrument 	
            // sinon      Recherche sur Acteur "null"  + Groupe Instrument "null"
            // sinon   => Recuperation de la valeur issue du Template
            bool isFound = false;
            bool isContinue = true;

            int level = -1;
            string sql;
            //
            while (isContinue && !isFound)
            {
                level += 1;

                string listIdAncestor = string.Empty;
                if (null != _actorAncestor)
                    listIdAncestor = _actorAncestor.GetListIdA_ActorByLevel(level);

                isContinue = StrFunc.IsFilled(listIdAncestor);

                if (isContinue)
                {
                    sql = GetSqlTuning(pCS, listIdAncestor, idGInstr);
                    dataSet = DataHelper.ExecuteDataset(pCS, _dbTransaction, CommandType.Text, sql);
                    isFound = (dataSet.Tables[0].Rows.Count > 0);
                }
            }
            #endregion

            #region Ultime recherche sans tenir compte de l'actor IDA
            if (!isFound)
            {
                sql = GetSqlTuning(pCS, null, idGInstr);
                dataSet = DataHelper.ExecuteDataset(pCS, _dbTransaction, CommandType.Text, sql);
                isFound = (dataSet.Tables[0].Rows.Count > 0);
            }
            #endregion

            if (isFound)
                dataRow = dataSet.Tables[0].Rows[0];

            _dr = dataRow;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="listIdAncestor"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        protected virtual string GetSqlTuning(string pCS, string listIdAncestor, Nullable<int> pIdGInstr)
        {
            return "Virtual GetSqlTuning";
        }
    }

    /// <summary>
    /// La classe "ActionTuningInput" est destinée à recevoir et manipuler une partie des données (les données "Input") issues de la table ACTIONTUNING
    /// On y retrouve des override pour les noms de colonnes spécifiques à la table ACTIONTUNING
    /// eg: CTRLUSERTRADE, ...
    /// </summary>
    public class ActionTuningInput : TuningInputBase
    {
        #region Constructors
        public ActionTuningInput(DataRow pDr) : base(pDr) { 
        }

        #endregion

        #region  public Cst.CtrlLastUserEnum
        protected override Cst.CtrlLastUserEnum CtrlUserTrade
        {
            get
            {
                Cst.CtrlLastUserEnum ret = base.CtrlUserTrade;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.CtrlLastUserEnum)System.Enum.Parse(typeof(Cst.CtrlLastUserEnum), _dr["CTRLUSERTRADE" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        #endregion  Cst.CtrlLastUserEnum
    }

    /// <summary>
    /// La classe "ActionTuning" est destinée à recevoir et exploiter les données issues de la table ACTIONTUNING
    /// eg: Colonnes: CTRLUSERTRADE, MINSTCHECK, CTRLMINSTCHECK, ..., CTRLCNFMSGGEN, ...
    /// </summary>
    public class ActionTuning : TuningUserBase
    {
        #region Members
        private readonly int _idPermission;       //Action is an permission in MENUs (OTC_INP_TRD,OTC_INP_TRD_ABN,OTC_INP_TRD_BAR_TRG,etc.....)
        private readonly ActionTuningInput _actionTuningInput;
        #endregion Members

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public ActionTuning(string pCS, int pIdI, int pIdPermission, int pIdA, ActorAncestor pActorAncestor)
            : this(pCS, null, pIdI, pIdPermission, pIdA, pActorAncestor) { }
        // EG 20180205 [23769] Add dbTransaction  
        public ActionTuning(string pCS, IDbTransaction pDbTransaction, int pIdI, int pIdPermission, int pIdA, ActorAncestor pActorAncestor)
            : base(pIdI, pIdA, pActorAncestor, pDbTransaction)
        {
            _idPermission = pIdPermission;
            SetDrTuning(CSTools.SetCacheOn(pCS));
            _actionTuningInput = new ActionTuningInput(_dr);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pListIdA"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        protected override string GetSqlTuning(string pCS, string pListIdA, Nullable<int> pIdGInstr)
        {

            StrBuilder sql = new StrBuilder();
            int firstIndex = 0;

            if (IntFunc.IsEmptyOrZero(pIdGInstr))
                firstIndex = 1;

            for (int i = firstIndex; i < 2; i++)
            {
                if (i > firstIndex)
                    sql += SQLCst.UNIONALL + Cst.CrLf;

                //PL 20111021 Add all GetSQLDataDtEnabled() 
                sql += SQLCst.SELECT + i.ToString() + " as ORDER_GINSTR, " + Cst.CrLf;
                sql += "IDACTIONTUNING, IDPERMISSION, IDGINSTR, IDA, CTRLUSERTRADE, " + Cst.CrLf;
                sql += "MINSTCHECK, CTRLMINSTCHECK, MAXSTCHECK, CTRLMAXSTCHECK, MINSTMATCH, CTRLMINSTMATCH, MAXSTMATCH, " + Cst.CrLf;
                sql += "CTRLMAXSTMATCH, CTRLCNFMSGGEN, CTRLESRGEN, CTRLEARGEN, " + Cst.CrLf;
                sql += "IDSTCHECK, IDSTMATCH";
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTIONTUNING.ToString() + " at" + Cst.CrLf;

                SQLWhere sqlWhere = new SQLWhere("at.IDPERMISSION=" + _idPermission.ToString());
                if (StrFunc.IsFilled(pListIdA))
                    sqlWhere.Append("at.IDA in  (" + pListIdA + ")", SQLCst.AND, true);
                else
                    sqlWhere.Append("at.IDA" + SQLCst.IS_NULL, SQLCst.AND, true);

                sqlWhere.Append(GetSubSqlTuning_GINSTR(pCS, "at", (i == 0 ? pIdGInstr : null)), SQLCst.AND, true);

                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "at"), SQLCst.AND, true);

                sql += sqlWhere + Cst.CrLf;
            }
            sql += SQLCst.ORDERBY + "ORDER_GINSTR";
            return sql.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public ErrorLevelTuningEnum ScanCompatibility(string pCS, int pIdT, TradeStatus pTradeStatus, int pTradeLastIda, out string pMsgControl)
        {
            ErrorLevelTuningEnum errorLevelTuning = _actionTuningInput.ScanTradeCompatibility(pCS, pIdT, pTradeStatus, pTradeLastIda, _idA, out pMsgControl);
            return errorLevelTuning;
        }
    }

    /// <summary>
    /// La classe "ActionTuningInput" est destinée à recevoir et manipuler une partie des données (les données "Input") issues de la table ACTIONTUNING
    /// On y retrouve des override pour les noms de colonnes spécifiques à la table ACTIONTUNING
    /// eg: CTRLUSERTRADE, ...
    /// </summary>
    public class StCheckTuningInput : TuningInputBase
    {
        #region Constructors
        public StCheckTuningInput(DataRow pDr)
            : base(pDr)
        {
        }
        #endregion

        #region  protected override ctrlUserTrade
        protected override Cst.CtrlLastUserEnum CtrlUserTrade
        {
            get
            {
                Cst.CtrlLastUserEnum ret = base.CtrlUserTrade;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.CtrlLastUserEnum)System.Enum.Parse(typeof(Cst.CtrlLastUserEnum), _dr["CTRLUSERTRADE" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        #endregion  Cst.CtrlLastUserEnum
        #region protected ctrlUserStatus
        protected Cst.CtrlLastUserEnum CtrlUserStatus
        {
            get
            {
                Cst.CtrlLastUserEnum ret = Cst.CtrlLastUserEnum.INDIFFERENT;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.CtrlLastUserEnum)System.Enum.Parse(typeof(Cst.CtrlLastUserEnum), _dr["CTRLUSERSTCHECK" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        #endregion  Cst.CtrlLastUserEnum

        #region public override ScanTradeCompatibility
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStatus"></param>
        /// <param name="pLastTradeIda"></param>
        /// <param name="pCurrentIda"></param>
        /// <param name="pMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public override ErrorLevelTuningEnum ScanTradeCompatibility(string pCS, int pIdT, CommonStatus pStatus, int pLastTradeIda, int pCurrentIda, out string pMsgControl)
        {
            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;
            base.ScanTradeCompatibility(pCS, pIdT, pStatus, pLastTradeIda, pCurrentIda, out  pMsgControl);

            if (IsDrSpecified)
            {
                switch (CtrlUserStatus)
                {
                    case Cst.CtrlLastUserEnum.DIFFERENT:
                        if (pStatus.GetUser(StatusEnum.StatusCheck) == pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERSTATUS;
                            pMsgControl += Ressource.GetString("Msg_UserStatusDifferent_NotOk") + Cst.CrLf;
                        }
                        break;
                    case Cst.CtrlLastUserEnum.IDENTIC:
                        if (pStatus.GetUser(StatusEnum.StatusCheck) != pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERSTATUS;
                            pMsgControl += Ressource.GetString("Msg_UserStatusIdentic_NotOk") + Cst.CrLf;
                        }
                        break;
                }
            }

            if (StrFunc.IsFilled(pMsgControl))
            {
                //Il existe au moins un problème de cohérence
                errLevel = ErrorLevelTuningEnum.ERROR;
            }
            return errLevel;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class StCheckTuning : TuningUserBase
    {
        #region Members
        private readonly string _stCheck;
        private readonly StCheckTuningInput _stCheckTuningInput;
        #endregion Members

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public StCheckTuning(string pCS, int pIdI, string pStCheck, int pIdA, ActorAncestor pActorAncestor)
            : this(pCS, null, pIdI, pStCheck, pIdA, pActorAncestor) { }
        // EG 20180205 [23769] Add dbTransaction  
        public StCheckTuning(string pCS, IDbTransaction pDbTransaction, int pIdI, string pStCheck, int pIdA, ActorAncestor pActorAncestor)
            : base(pIdI, pIdA, pActorAncestor, pDbTransaction)
        {
            _stCheck = pStCheck;
            SetDrTuning(CSTools.SetCacheOn(pCS));
            _stCheckTuningInput = new StCheckTuningInput(_dr);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        public ErrorLevelTuningEnum ScanCompatibility(string pCS, int pIdT, CommonStatus pStatus, int pTradeLastIda, out string opMsgControl)
        {
            return _stCheckTuningInput.ScanTradeCompatibility(pCS, pIdT, pStatus, pTradeLastIda, _idA, out opMsgControl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pListIdA"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        protected override string GetSqlTuning(string pCS, string pListIdA, Nullable<int> pIdGInstr)
        {

            StrBuilder sql = new StrBuilder();
            int firstIndex = 0;

            if (IntFunc.IsEmptyOrZero(pIdGInstr))
                firstIndex = 1;

            for (int i = firstIndex; i < 2; i++)
            {
                if (i > firstIndex)
                    sql += SQLCst.UNIONALL + Cst.CrLf;

                //PL 20111021 Add all GetSQLDataDtEnabled() 
                sql += SQLCst.SELECT + i.ToString() + " as ORDER_GINSTR," + Cst.CrLf;
                sql += "IDSTCHECKTUNING, IDSTCHECK, IDGINSTR, IDA, CTRLUSERTRADE, CTRLUSERSTCHECK, " + Cst.CrLf;
                sql += "MINSTCHECK, CTRLMINSTCHECK, MAXSTCHECK, CTRLMAXSTCHECK, MINSTMATCH, CTRLMINSTMATCH, MAXSTMATCH, " + Cst.CrLf;
                sql += "CTRLMAXSTMATCH, CTRLCNFMSGGEN, CTRLESRGEN, CTRLEARGEN" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.STCHECKTUNING.ToString() + " at" + Cst.CrLf;

                SQLWhere sqlWhere = new SQLWhere("at.IDSTCHECK=" + DataHelper.SQLString(_stCheck));
                if (StrFunc.IsFilled(pListIdA))
                    sqlWhere.Append("at.IDA in  (" + pListIdA + ")", SQLCst.AND, true);
                else
                    sqlWhere.Append("at.IDA" + SQLCst.IS_NULL, SQLCst.AND, true);

                sqlWhere.Append(GetSubSqlTuning_GINSTR(pCS, "at", (i == 0 ? pIdGInstr : null)), SQLCst.AND, true);

                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "at"), SQLCst.AND, true);

                sql += sqlWhere + Cst.CrLf;
            }
            sql += SQLCst.ORDERBY + "ORDER_GINSTR";
            return sql.ToString();

        }
    }

    /// <summary>
    /// La classe "ActionTuningInput" est destinée à recevoir et manipuler une partie des données (les données "Input") issues de la table ACTIONTUNING
    /// On y retrouve des override pour les noms de colonnes spécifiques à la table ACTIONTUNING
    /// eg: CTRLUSERTRADE, ...
    /// </summary>
    public class StMatchTuningInput : TuningInputBase
    {
        #region Constructors
        public StMatchTuningInput(DataRow pDr)
            : base(pDr)
        {
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected override Cst.CtrlLastUserEnum CtrlUserTrade
        {
            get
            {
                Cst.CtrlLastUserEnum ret = base.CtrlUserTrade;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.CtrlLastUserEnum)System.Enum.Parse(typeof(Cst.CtrlLastUserEnum), _dr["CTRLUSERTRADE" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected Cst.CtrlLastUserEnum CtrlUserStatus
        {
            get
            {
                Cst.CtrlLastUserEnum ret = Cst.CtrlLastUserEnum.INDIFFERENT;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.CtrlLastUserEnum)System.Enum.Parse(typeof(Cst.CtrlLastUserEnum), _dr["CTRLUSERSTMATCH" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStatus"></param>
        /// <param name="pTradeIda"></param>
        /// <param name="pCurrentIda"></param>
        /// <param name="pMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public override ErrorLevelTuningEnum ScanTradeCompatibility(string pCS, int pIdT, CommonStatus pStatus, int pTradeIda, int pCurrentIda, out string pMsgControl)
        {
            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;
            base.ScanTradeCompatibility(pCS, pIdT, pStatus, pTradeIda, pCurrentIda, out pMsgControl);

            if (IsDrSpecified)
            {

                switch (CtrlUserStatus)
                {
                    case Cst.CtrlLastUserEnum.DIFFERENT:
                        if (pStatus.GetUser(StatusEnum.StatusMatch) == pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERSTATUS;
                            pMsgControl += Ressource.GetString("Msg_UserStatusDifferent_NotOk") + Cst.CrLf;
                        }
                        break;
                    case Cst.CtrlLastUserEnum.IDENTIC:
                        if (pStatus.GetUser(StatusEnum.StatusMatch) != pCurrentIda)
                        {
                            errLevel = ErrorLevelTuningEnum.ERR_USERSTATUS;
                            pMsgControl += Ressource.GetString("Msg_UserStatusIdentic_NotOk") + Cst.CrLf;
                        }
                        break;
                }
            }

            if (StrFunc.IsFilled(pMsgControl))
            {
                //Il existe au moins un problème de cohérence
                errLevel = ErrorLevelTuningEnum.ERROR;
            }
            return errLevel;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StMatchTuning : TuningUserBase
    {
        #region Members
        private readonly string _stMatch;
        private readonly StMatchTuningInput _stMatchTuningInput;
        #endregion Members

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public StMatchTuning(string pCS, int pIdI, string pStMatch, int pIdA, ActorAncestor pActorAncestor)
            : this(pCS, null, pIdI, pStMatch, pIdA, pActorAncestor) { }
        // EG 20180205 [23769] Add dbTransaction  
        public StMatchTuning(string pCS, IDbTransaction pDbTransaction, int pIdI, string pStMatch, int pIdA, ActorAncestor pActorAncestor)
            : base(pIdI, pIdA, pActorAncestor, pDbTransaction)
        {
            _stMatch = pStMatch;
            SetDrTuning(CSTools.SetCacheOn(pCS));
            _stMatchTuningInput = new StMatchTuningInput(_dr);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl 
        public ErrorLevelTuningEnum ScanCompatibility(string pCS, int pIdT, CommonStatus pStatus, int pTradeLastIda, out string pMsgControl)
        {
            return _stMatchTuningInput.ScanTradeCompatibility(pCS, pIdT, pStatus, pTradeLastIda, _idA, out pMsgControl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pListIdA"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        protected override string GetSqlTuning(string pCS, string pListIdA, Nullable<int> pIdGInstr)
        {

            StrBuilder sql = new StrBuilder();
            int firstIndex = 0;

            if (IntFunc.IsEmptyOrZero(pIdGInstr))
                firstIndex = 1;

            for (int i = firstIndex; i < 2; i++)
            {
                if (i > firstIndex)
                    sql += SQLCst.UNIONALL + Cst.CrLf;

                //PL 20111021 Add all GetSQLDataDtEnabled() 
                sql += SQLCst.SELECT + i.ToString() + " as ORDER_GINSTR," + Cst.CrLf;
                sql += "IDSTMATCHTUNING, IDSTMATCH, IDGINSTR, IDA, CTRLUSERTRADE, CTRLUSERSTMATCH, " + Cst.CrLf;
                sql += "MINSTCHECK, CTRLMINSTCHECK, MAXSTCHECK, CTRLMAXSTCHECK, MINSTMATCH, CTRLMINSTMATCH, MAXSTMATCH, CTRLMAXSTMATCH, " + Cst.CrLf;
                sql += "CTRLCNFMSGGEN, CTRLESRGEN, CTRLEARGEN" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.STMATCHTUNING.ToString() + " at" + Cst.CrLf;

                SQLWhere sqlWhere = new SQLWhere("at.IDSTMATCH=" + DataHelper.SQLString(_stMatch));
                if (StrFunc.IsFilled(pListIdA))
                    sqlWhere.Append("at.IDA in  (" + pListIdA + ")", SQLCst.AND, true);
                else
                    sqlWhere.Append("at.IDA" + SQLCst.IS_NULL, SQLCst.AND, true);

                sqlWhere.Append(GetSubSqlTuning_GINSTR(pCS, "at", (i == 0 ? pIdGInstr : null)), SQLCst.AND, true);

                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "at"), SQLCst.AND, true);

                sql += sqlWhere;
            }
            sql += SQLCst.ORDERBY + "ORDER_GINSTR";
            return sql.ToString();
        }
    }

    /// <summary>
    /// La classe "ProcessTuningInput" est destinée à recevoir et manipuler une partie des données (les données "Input") issues de la table PROCESSTUNING
    /// On y retrouve des override pour les noms de colonnes spécifiques à la table ACTIONTUNING
    /// eg: IDSTACTIVATION, ...
    /// </summary>
    public class ProcessTuningInput : TuningInputBase
    {
        #region Constructors
        public ProcessTuningInput(DataRow pDr, TuningInputTypeEnum pTuningInputType)
            : base(pDr, pTuningInputType)
        {
        }
        #endregion

        #region protected override idStatusActivationSpecified
        protected override bool IdStatusActivationSpecified
        {
            get
            {
                bool ret = false;
                try
                {
                    if (IsDrSpecified)
                        ret = (_dr["IDSTACTIVATION" + _suffix] != Convert.DBNull);
                }
                catch
                { }
                return ret;
            }
        }
        #endregion
        #region protected override idStatusActivation
        protected override Cst.StatusActivation IdStatusActivation
        {
            get
            {
                Cst.StatusActivation ret = base.IdStatusActivation;
                try
                {
                    if (IsDrSpecified)
                        ret = (Cst.StatusActivation)System.Enum.Parse(typeof(Cst.StatusActivation), _dr["IDSTACTIVATION" + _suffix].ToString());
                }
                catch
                { }
                return ret;
            }
        }
        #endregion
    }

    /// <summary>
    /// La classe "ProcessTuningOutput" est destinée à recevoir et manipuler une partie des données (les données "Output") issues de la table PROCESSTUNING
    /// On y retrouve des "override" pour les noms de colonnes spécifiques à la table PROCESSTUNING
    /// eg: [none]
    /// </summary>
    public class ProcessTuningOutput : TuningOutputBase
    {
        #region Constructors
        public ProcessTuningOutput(DataRow pDr, TuningOutputTypeEnum pTuningOutputType)
            : base(pDr, pTuningOutputType.ToString())
        { }
        #endregion
    }

    /// <summary>
    /// La classe "ProcessTuning" est destinée à recevoir et exploiter les données issues de la table PROCESSTUNING
    /// eg: Colonnes: IDSTACTIVATION_ITI, IDSTCHECK_ITI, ..., , IDSTACTIVATION_ITP, ..., SENDMESSAGE_PPS, ...
    /// </summary>
    public class ProcessTuning : TuningBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190524 [23912] Add
        private enum ScanType
        {
            Trade, 
            Event
        }
        
        #region Members
        private readonly Cst.ProcessTypeEnum m_ProcessType;
        private readonly string m_ProcessName;
        private readonly string m_HostName;

        private readonly ProcessTuningInput processTuningInputTradeIgnore;
        private readonly ProcessTuningInput processTuningInputTradeProcess;
        private readonly ProcessTuningInput processTuningInputEventIgnore;
        private readonly ProcessTuningInput processTuningInputEventProcess;

        private readonly ProcessTuningOutput processTuningOutputTradeSuccess;
        private readonly ProcessTuningOutput processTuningOutputTradeError;
        private readonly ProcessTuningOutput processTuningOutputEventSuccess;
        private readonly ProcessTuningOutput processTuningOutputEventError;
        #endregion

        #region accessor
        public LogLevelDetail LogDetailEnum
        {
            get
            {
                LogLevelDetail ret = LogLevelDetail.LEVEL3;
                
                if (DrSpecified)
                {
                    string data = Convert.ToString(Dr["LOGLEVEL"]);
                    if (Enum.IsDefined(typeof(LogLevelDetail), data))
                    {
                        ret = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), data, true);
                    }
                    else
                    {
                        //Pour compatibilité ascendante, si besoin... 
                        switch (data)
                        {
                            case "FULL":
                                ret = LogLevelDetail.LEVEL4;
                                break;
                            case "NONE":
                                ret = LogLevelDetail.LEVEL2;
                                break;
                            default:
                                ret = LogLevelDetail.LEVEL3;
                                break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public ProcessTuning(string pCS, int pIdI, Cst.ProcessTypeEnum pProcessType, string pProcessName, string pHostName)
            : this(pCS, null, pIdI, pProcessType, pProcessName, pHostName) { }
        // EG 20180205 [23769] Add dbTransaction  
        public ProcessTuning(string pCS, IDbTransaction pDbTransaction, int pIdI, Cst.ProcessTypeEnum pProcessType, string pProcessName, string pHostName)
            : base(pIdI, pDbTransaction)
        {

            m_ProcessType = pProcessType;
            m_ProcessName = pProcessName;
            m_HostName = pHostName;

            SetDrTuning(CSTools.SetCacheOn(pCS));

            processTuningInputTradeIgnore = new ProcessTuningInput(_dr, TuningInputTypeEnum.ITI);
            processTuningInputTradeProcess = new ProcessTuningInput(_dr, TuningInputTypeEnum.ITP);
            processTuningInputEventIgnore = new ProcessTuningInput(_dr, TuningInputTypeEnum.IEI);
            processTuningInputEventProcess = new ProcessTuningInput(_dr, TuningInputTypeEnum.IEP);

            processTuningOutputTradeSuccess = new ProcessTuningOutput(_dr, TuningOutputTypeEnum.OTS);
            processTuningOutputTradeError = new ProcessTuningOutput(_dr, TuningOutputTypeEnum.OTE);
            processTuningOutputEventSuccess = new ProcessTuningOutput(_dr, TuningOutputTypeEnum.OES);
            processTuningOutputEventError = new ProcessTuningOutput(_dr, TuningOutputTypeEnum.OEE);
        }
        #endregion


        /// <summary>
        /// Get an Array of SendMessage by reference
        /// </summary>
        /// <param name="pTuningOutput"></param>
        /// <param name="opSendMessageTypeArray"></param>
        //PL 20120824 Tuning 
        public bool Get_SendMessage(TuningOutputTypeEnum pTuningOutput, ref Cst.ProcessTypeEnum[] opSendMessageTypeArray)
        {
            bool ret = false;
            ProcessTuningOutput processTuningOutput = GetProcessTuningOutput(pTuningOutput);

            if (processTuningOutput != null)
            {
                //ret = true;
                //opSendMessageTypeArray = processTuningOutput.SendMessage;
                ret = processTuningOutput.SendMessage(ref opSendMessageTypeArray);
            }
            return ret;
        }

        /// <summary>
        /// Return an output StActivation
        /// </summary>
        /// <param name="pTuningOutput"></param>
        /// <returns></returns>
        public Cst.StatusActivation Get_StActivation(TuningOutputTypeEnum pTuningOutput)
        {
            Cst.StatusActivation ret = Cst.StatusActivation.REGULAR;
            //
            ProcessTuningOutput processTuningOutput = GetProcessTuningOutput(pTuningOutput);
            //
            if (processTuningOutput != null)
            {
                if (processTuningOutput.IdStActivationSpecified)
                    ret = processTuningOutput.IdStActivation;
            }
            return ret;
        }

        /// <summary>
        /// Write in RDBMS output Status
        /// </summary>
        /// <param name="pTuningOutput"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        public Cst.ErrLevel WriteStatus(string pCS, IDbTransaction pDbTransaction, TuningOutputTypeEnum pTuningOutput, int pId, int pIdA)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            bool isNewTransac = (null == pDbTransaction);
            IDbTransaction dbTransaction; 
            
            if (isNewTransac)
                dbTransaction = DataHelper.BeginTran(pCS);
            else
                dbTransaction = pDbTransaction;
            
            try
            {
                // FI 20200820 [25468] Dates systemes en UTC
                string sqlDtSys = DataHelper.SQLGetDate(pCS,true);
                ProcessTuningOutput processTuningOutput = GetProcessTuningOutput(pTuningOutput);
                //
                string columnIdName = null;
                string tableNameStSys = null;
                Cst.OTCml_TBL prefixTableNameStUser = Cst.OTCml_TBL.SYSTEM_L;
                //
                switch (pTuningOutput)
                {
                    case TuningOutputTypeEnum.OTS:
                    case TuningOutputTypeEnum.OTE:
                        tableNameStSys = Cst.OTCml_TBL.TRADE.ToString();
                        prefixTableNameStUser = Cst.OTCml_TBL.TRADE;
                        columnIdName = "IDT";
                        break;
                    case TuningOutputTypeEnum.OES:
                    case TuningOutputTypeEnum.OEE:
                        tableNameStSys = Cst.OTCml_TBL.EVENT.ToString();
                        prefixTableNameStUser = Cst.OTCml_TBL.EVENT;
                        columnIdName = "IDE";
                        break;
                }
                //
                #region TRADE Or EVENT
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    if (processTuningOutput.IdStActivationSpecified || processTuningOutput.IdStPrioritySpecified)
                    {
                        string sql = SQLCst.UPDATE_DBO + tableNameStSys + SQLCst.SET;
                        string where = SQLCst.WHERE + columnIdName + " = " + pId.ToString();
                        string separator = string.Empty;
                        //
                        if (processTuningOutput.IdStActivationSpecified)
                        {
                            sql += separator + "IDSTACTIVATION=" + DataHelper.SQLString(processTuningOutput.IdStActivation.ToString());
                            separator = ",";
                            sql += separator + "DTSTACTIVATION=" + sqlDtSys;
                            sql += separator + "IDASTACTIVATION=" + pIdA.ToString();
                        }
                        if (processTuningOutput.IdStPrioritySpecified)
                        {
                            sql += separator + "IDSTPRIORITY=" + DataHelper.SQLString(processTuningOutput.IdStPriority.ToString());
                            separator = ",";
                            sql += separator + "DTSTPRIORITY=" + sqlDtSys;
                            sql += separator + "IDASTPRIORITY=" + pIdA.ToString();
                        }
                        //
                        sql += Cst.CrLf + where;
                        int nRow = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sql);
                        switch (nRow)
                        {
                            case 1:
                                ret = Cst.ErrLevel.SUCCESS;
                                break;
                            case 0:
                                ret = Cst.ErrLevel.DATANOTFOUND;
                                break;
                            default:
                                ret = Cst.ErrLevel.MULTIDATAFOUND;
                                break;
                        }
                    }
                }
                #endregion
                //
                #region TRADESTCHECK Or EVENTSTCHECK or TRADESTMATCH or EVENTSTMATCH
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)))
                    {
                        if (StatusTools.IsStatusUser(statusEnum))
                        {
                            if (processTuningOutput.IdStSpecified(statusEnum) && (ret == Cst.ErrLevel.SUCCESS))
                            {
                                string tableName = StatusTools.GetTableNameStatusUser(statusEnum, prefixTableNameStUser).ToString();
                                string columnName = StatusTools.GetColumnNameStatusUser(statusEnum);
                                string stValue = DataHelper.SQLString(processTuningOutput.IdSt(statusEnum));
                                //
                                string sqlDel = SQLCst.DELETE_DBO + tableName + Cst.CrLf;
                                sqlDel += SQLCst.WHERE + columnIdName + " = " + pId.ToString();
                                sqlDel += SQLCst.AND + columnName + " = " + stValue;
                                //
                                string sqlIns = SQLCst.INSERT_INTO_DBO + tableName;
                                sqlIns += "(" + columnIdName + "," + columnName + ",DTINS,IDAINS)";
                                sqlIns += " values ";
                                sqlIns += "(" + pId.ToString() + "," + stValue + "," + sqlDtSys + "," + pIdA.ToString() + ")";
                                //
                                int nRow = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlDel + Cst.CrLf + sqlIns);
                                if (nRow > 0)
                                    ret = Cst.ErrLevel.SUCCESS;
                                else
                                    ret = Cst.ErrLevel.DATANOTFOUND;
                            }
                        }
                    }
                }
                #endregion
                //
            }
            // FI 20200915 SpheresException2 impossible
            //catch (SpheresException2) { throw; }
            catch (Exception ex)
            {
                ret = Cst.ErrLevel.FAILURE;
                if (isNewTransac)
                    DataHelper.RollbackTran(dbTransaction);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if (isNewTransac && (ret == Cst.ErrLevel.SUCCESS))
                    DataHelper.CommitTran(dbTransaction);
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public override void SetDrTuning(string pCS)
        {
            _dr = null;
            // Exist GINSTR => Permet de réduire la complexité des query 
            // Si l'instrument n'appartient pas un un groupe les restrictions se feront directement avec IDGINSTR is null 
            Nullable<int> idGInstr = InstrTools.GetIdGInstr(pCS, _dbTransaction, _idI, RoleGInstr.TUNING);

            //Recherche des defaults paramétrés dans PROCESSTUNING 
            QueryParameters sql = GetSqlProcessTuning(pCS, idGInstr);

            DataSet dataSet = DataHelper.ExecuteDataset(pCS, _dbTransaction, CommandType.Text, sql.Query, sql.Parameters.GetArrayDbParameter());
            if (dataSet.Tables[0].Rows.Count > 0)
                _dr = dataSet.Tables[0].Rows[0];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdGInstr"></param>
        /// <returns></returns>
        /// FI 20120711 tuning => la fonction retourne un QueryParameters
        private QueryParameters GetSqlProcessTuning(string pCS, Nullable<int> pIdGInstr)
        {
            StrBuilder sql = new StrBuilder();
            int firstIndex = 0;

            if (IntFunc.IsEmptyOrZero(pIdGInstr))
                firstIndex = 1;

            for (int i = firstIndex; i < 2; i++)
            {
                if (i > firstIndex)
                    sql += SQLCst.UNIONALL + Cst.CrLf;

                //PL 20111021 Add all GetSQLDataDtEnabled() 
                sql += SQLCst.SELECT + i.ToString() + " as ORDER_GINSTR," + Cst.CrLf;
                sql += "IDPROCESSTUNING, PROCESS, IDGINSTR, PROCESSNAME, HOSTNAME, LOGLEVEL," + Cst.CrLf;
                //
                sql += "IDSTACTIVATION_ITI, MINSTCHECK_ITI, CTRLMINSTCHECK_ITI, MAXSTCHECK_ITI, CTRLMAXSTCHECK_ITI," + Cst.CrLf;
                sql += "MINSTMATCH_ITI, CTRLMINSTMATCH_ITI, MAXSTMATCH_ITI, CTRLMAXSTMATCH_ITI,CTRLCNFMSGGEN_ITI," + Cst.CrLf;
                sql += "CTRLESRGEN_ITI,CTRLEARGEN_ITI,CTRLACCOUNTGEN_ITI, CTRLSIGEN_ITI," + Cst.CrLf;
                //
                sql += "IDSTACTIVATION_IEI, MINSTCHECK_IEI, CTRLMINSTCHECK_IEI, MAXSTCHECK_IEI, CTRLMAXSTCHECK_IEI," + Cst.CrLf;
                sql += "MINSTMATCH_IEI, CTRLMINSTMATCH_IEI, MAXSTMATCH_IEI,CTRLMAXSTMATCH_IEI,CTRLCNFMSGGEN_IEI," + Cst.CrLf;
                sql += "CTRLESRGEN_IEI,CTRLEARGEN_IEI,CTRLACCOUNTGEN_IEI, CTRLSIGEN_IEI," + Cst.CrLf;
                //
                sql += "IDSTACTIVATION_ITP, MINSTCHECK_ITP, CTRLMINSTCHECK_ITP, MAXSTCHECK_ITP, CTRLMAXSTCHECK_ITP," + Cst.CrLf;
                sql += "MINSTMATCH_ITP, CTRLMINSTMATCH_ITP, MAXSTMATCH_ITP,CTRLMAXSTMATCH_ITP,CTRLCNFMSGGEN_ITP," + Cst.CrLf;
                sql += "CTRLESRGEN_ITP,CTRLEARGEN_ITP,CTRLACCOUNTGEN_ITP, CTRLSIGEN_ITP," + Cst.CrLf;
                //
                sql += "IDSTACTIVATION_IEP, MINSTCHECK_IEP, CTRLMINSTCHECK_IEP, MAXSTCHECK_IEP, CTRLMAXSTCHECK_IEP," + Cst.CrLf;
                sql += "MINSTMATCH_IEP, CTRLMINSTMATCH_IEP, MAXSTMATCH_IEP,CTRLMAXSTMATCH_IEP,CTRLCNFMSGGEN_IEP," + Cst.CrLf;
                sql += "CTRLESRGEN_IEP,CTRLEARGEN_IEP,CTRLACCOUNTGEN_IEP, CTRLSIGEN_IEP," + Cst.CrLf;
                //
                sql += "IDSTACTIVATION_OTS, IDSTPRIORITY_OTS, IDSTCHECK_OTS, IDSTMATCH_OTS," + Cst.CrLf;
                sql += "IDSTACTIVATION_OES, null as IDSTPRIORITY_OES, IDSTCHECK_OES, IDSTMATCH_OES," + Cst.CrLf;
                sql += "SENDMESSAGE_PPS1, SENDMESSAGE_PPS2, SENDMESSAGE_PPS3," + Cst.CrLf; // RD 20091221 [16803]
                sql += "IDSTACTIVATION_OTE, IDSTPRIORITY_OTE, IDSTCHECK_OTE, IDSTMATCH_OTE," + Cst.CrLf;
                sql += "IDSTACTIVATION_OEE, null as IDSTPRIORITY_OEE, IDSTCHECK_OEE, IDSTMATCH_OEE," + Cst.CrLf;
                sql += "SENDMESSAGE_PPE1, SENDMESSAGE_PPE2, SENDMESSAGE_PPE3, ISMOVEMESSAGE_PPE" + Cst.CrLf; // RD 20091221 [16803]

                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESSTUNING.ToString() + " pt" + Cst.CrLf;

                SQLWhere sqlWhere = new SQLWhere("pt.PROCESS=@PROCESS");
                if (StrFunc.IsFilled(m_ProcessName))
                    sqlWhere.Append("(pt.PROCESSNAME=@PROCESSNAME"
                        + SQLCst.OR + "pt.PROCESSNAME" + SQLCst.IS_NULL + ")", SQLCst.AND, true);
                else
                    sqlWhere.Append("pt.PROCESSNAME" + SQLCst.IS_NULL, SQLCst.AND, true);
                if (StrFunc.IsFilled(m_HostName))
                    sqlWhere.Append("(pt.HOSTNAME=@HOSTNAME"
                        + SQLCst.OR + "pt.HOSTNAME" + SQLCst.IS_NULL + ")", SQLCst.AND, true);
                else
                    sqlWhere.Append("pt.HOSTNAME" + SQLCst.IS_NULL, SQLCst.AND, true);

                sqlWhere.Append(GetSubSqlTuning_GINSTR(pCS, "pt", (i == 0 ? pIdGInstr : null)), SQLCst.AND, true);

                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "pt"), SQLCst.AND, true);

                sql += sqlWhere + Cst.CrLf;
            }
            sql += SQLCst.ORDERBY + "ORDER_GINSTR,PROCESSNAME" + SQLCst.DESC + ",HOSTNAME" + SQLCst.DESC;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.HOSTNAME), m_HostName);
            dp.Add(new DataParameter(pCS, "PROCESSNAME", DbType.AnsiString, 64), m_ProcessName);
            dp.Add(new DataParameter(pCS, "PROCESS", DbType.AnsiString, 64), m_ProcessType);

            QueryParameters ret = new QueryParameters(pCS, sql.ToString(), dp);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTuningOutput"></param>
        /// <returns></returns>
        public ProcessTuningOutput GetProcessTuningOutput(TuningOutputTypeEnum pTuningOutput)
        {
            ProcessTuningOutput processTuningOutput = null;
            switch (pTuningOutput)
            {
                case TuningOutputTypeEnum.OTS:
                    processTuningOutput = this.processTuningOutputTradeSuccess;
                    break;
                case TuningOutputTypeEnum.OES:
                    processTuningOutput = this.processTuningOutputEventSuccess;
                    break;
                case TuningOutputTypeEnum.OTE:
                    processTuningOutput = this.processTuningOutputTradeError;
                    break;
                case TuningOutputTypeEnum.OEE:
                    processTuningOutput = this.processTuningOutputEventError;
                    break;
            }
            return processTuningOutput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pCommonStatus"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Refactoring
        public Cst.ErrLevel ScanTradeCompatibility(string pCS, int pId, CommonStatus pCommonStatus, out  string opMsgControl)
        {
            Pair<ProcessTuningInput, ProcessTuningInput> processTuningInputTrade =
                new Pair<ProcessTuningInput, ProcessTuningInput>(processTuningInputTradeIgnore, processTuningInputTradeProcess);

            return ScanCompatibility(processTuningInputTrade, ScanType.Trade,  pCS, pId, pCommonStatus, out opMsgControl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pCommonStatus"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Refactoring
        public Cst.ErrLevel ScanEventCompatibility(string pCS, int pId, CommonStatus pCommonStatus, out  string opMsgControl)
        {
            Pair<ProcessTuningInput, ProcessTuningInput> processTuningInputEvent =
                new Pair<ProcessTuningInput, ProcessTuningInput>(processTuningInputEventIgnore, processTuningInputEventProcess);

            return ScanCompatibility(processTuningInputEvent, ScanType.Event, pCS, pId, pCommonStatus, out opMsgControl);
        }


        /// <summary>
        /// ScanCompatibility: Scan si compatible avec le référentiel PROCESSTUNING
        ///   Step 1: S'il existe un paramétrage "Ignore" on vérifie si celui-ci est compatible avec le paramétrage
        ///           Si oui, on retourne "ErrLevel.DATAIGNORE"
        ///           Si non, on passe à l'étape 2
        ///   Step 2: S'il existe un paramétrage "Process" on vérifie si celui-ci est compatible avec le paramétrage
        ///           Si oui, on retourne "ErrLevel.SUCCESS"
        ///           Si non, on retourne "ErrLevel.DATAUNMATCH"
        ///   Step 3: S'il n'existe aucun paramétrage 
        ///			  On retourne "ErrLevel.SUCCESS"
        /// </summary>
        /// FI 20190524 [23912] Add
        // EG 20190605 [23912] Upd Correction Retour du Status Cst.ErrLevel
        private static Cst.ErrLevel ScanCompatibility<T>(T processTuningInput, ScanType ptype, string pCS, int pId, CommonStatus pCommonStatus, out  string opMsgControl)
                where T : Pair<ProcessTuningInput, ProcessTuningInput>
        {
            opMsgControl = string.Empty;
            if (null == processTuningInput.First)
                throw new InvalidProgramException("processTuningInputIgnore is null");

            if (null == processTuningInput.Second)
                throw new InvalidProgramException("processTuningInputProcess is null");

            switch (ptype)
            {
                case ScanType.Trade:
                case ScanType.Event:
                    break;
                default:
                    throw new InvalidProgramException(ptype.ToString() + " is not supported");
            }

            ProcessTuningInput processTuningInputIgnore = processTuningInput.First;
            ProcessTuningInput processTuningInputProcess = processTuningInput.Second;

            Cst.ErrLevel errLevel;
            if (processTuningInputIgnore.ExistDataSetting || processTuningInputProcess.ExistDataSetting)
            {

                errLevel = Cst.ErrLevel.TUNING_UNMATCH;

                //Le traitement doit-il etre ignoré ?
                if (processTuningInputIgnore.ExistDataSetting)
                {
                    ErrorLevelTuningEnum errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
                    switch (ptype)
                    {
                        case ScanType.Trade:
                            errorLevelTuning = processTuningInputIgnore.ScanTradeCompatibility(pCS, pId, pCommonStatus, 0, 0, out opMsgControl);
                            break;
                        case ScanType.Event:
                            errorLevelTuning = processTuningInputIgnore.ScanEventCompatibility(pCS, pId, pCommonStatus, out opMsgControl);
                            break;
                    }
                    if (errorLevelTuning == ErrorLevelTuningEnum.SUCCESS)
                        errLevel = Cst.ErrLevel.TUNING_IGNORE;
                }

                //Si le traitement n'est pas ignoré alors Le traitement doit-il être exécuté ?
                // EG 20190605 [23912] Upd 
                if (errLevel == Cst.ErrLevel.TUNING_UNMATCH)
                {
                    if (processTuningInputProcess.ExistDataSetting)
                    {
                        string msgControl = string.Empty;
                        ErrorLevelTuningEnum errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
                        switch (ptype)
                        {
                            case ScanType.Trade:
                                errorLevelTuning = processTuningInputProcess.ScanTradeCompatibility(pCS, pId, pCommonStatus, 0, 0, out msgControl);
                                break;
                            case ScanType.Event:
                                errorLevelTuning = processTuningInputProcess.ScanEventCompatibility(pCS, pId, pCommonStatus, out msgControl);
                                break;
                        }

                        if (StrFunc.IsFilled(opMsgControl))
                            opMsgControl += Cst.CrLf;
                        opMsgControl += msgControl;

                        if (errorLevelTuning == ErrorLevelTuningEnum.SUCCESS)
                            errLevel = Cst.ErrLevel.SUCCESS;
                    }
                    else
                    {
                        errLevel = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            else
            {
                errLevel = Cst.ErrLevel.SUCCESS;
            }
            return errLevel;
        }


    }

    /// <summary>
    /// La classe "TuningTools" est une classe "sealed" contenant diverses méthodes "static" destinées à la gestion des infos de TUNING 
    /// eg: ScanEventProcess()
    /// </summary>
    public sealed class TuningTools
    {
        #region public ScanAllEventProcess
        public static ErrorLevelTuningEnum ScanTradeEventProcess(string pCS, int pIdT, Cst.ProcessTypeEnum pProcessType, Cst.CtrlSendingEnum pCtrlSending)
        {
            Cst.ProcessTypeEnum[] processTypeEnum;
            if (pProcessType == Cst.ProcessTypeEnum.ESRGEN)
                processTypeEnum = new Cst.ProcessTypeEnum[] { Cst.ProcessTypeEnum.ESRSTDGEN, Cst.ProcessTypeEnum.ESRNETGEN };
            else
                processTypeEnum = new Cst.ProcessTypeEnum[] { pProcessType };
            return ScanTradeEventProcess(pCS, pIdT, processTypeEnum, pCtrlSending);
        }
        public static ErrorLevelTuningEnum ScanTradeEventProcess(string pCS, int pIdT, Cst.ProcessTypeEnum[] pProcessType, Cst.CtrlSendingEnum pCtrlSending)
        {

            ErrorLevelTuningEnum errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
            //
            switch (pCtrlSending)
            {
                case Cst.CtrlSendingEnum.NOSENDING:
                case Cst.CtrlSendingEnum.SENDING:
                    bool isFound = TradeRDBMSTools.ExistEventProcess(pCS, pIdT, pProcessType);
                    switch (pCtrlSending)
                    {
                        case Cst.CtrlSendingEnum.NOSENDING:
                            if (isFound)
                                errorLevelTuning = ErrorLevelTuningEnum.ERROR;
                            break;
                        case Cst.CtrlSendingEnum.SENDING:
                            if (!isFound)
                                errorLevelTuning = ErrorLevelTuningEnum.ERROR;
                            break;
                    }
                    break;
                case Cst.CtrlSendingEnum.INDIFFERENT:
                default:
                    errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
                    break;
            }
            //
            return errorLevelTuning;

        }
        #endregion ScanAllEventProcess

        #region public ScanEventProcess
        public static ErrorLevelTuningEnum ScanEventProcess(string pCS, int pIdE, Cst.ProcessTypeEnum pProcessType, Cst.CtrlSendingEnum pCtrlSending)
        {
            Cst.ProcessTypeEnum[] processTypeEnum;
            if (pProcessType == Cst.ProcessTypeEnum.ESRGEN)
                processTypeEnum = new Cst.ProcessTypeEnum[] { Cst.ProcessTypeEnum.ESRSTDGEN, Cst.ProcessTypeEnum.ESRNETGEN };
            else
                processTypeEnum = new Cst.ProcessTypeEnum[] { pProcessType };
            return ScanEventProcess(pCS, pIdE, processTypeEnum, pCtrlSending);
        }

        public static ErrorLevelTuningEnum ScanEventProcess(string pCS, int pIdE, Cst.ProcessTypeEnum[] pProcessType, Cst.CtrlSendingEnum pCtrlSending)
        {

            ErrorLevelTuningEnum errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
            //
            switch (pCtrlSending)
            {
                case Cst.CtrlSendingEnum.NOSENDING:
                case Cst.CtrlSendingEnum.SENDING:
                    bool isFound = EventRDBMSTools.ExistEventProcess(pCS, pIdE, pProcessType);
                    switch (pCtrlSending)
                    {
                        case Cst.CtrlSendingEnum.NOSENDING:
                            if (isFound)
                                errorLevelTuning = ErrorLevelTuningEnum.ERROR;
                            break;
                        case Cst.CtrlSendingEnum.SENDING:
                            if (!isFound)
                                errorLevelTuning = ErrorLevelTuningEnum.ERROR;
                            break;
                    }
                    break;
                case Cst.CtrlSendingEnum.INDIFFERENT:
                default:
                    errorLevelTuning = ErrorLevelTuningEnum.SUCCESS;
                    break;
            }
            //
            return errorLevelTuning;

        }
        #endregion ScanEventProcess

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdPermission"></param>
        /// <param name="pIdA"></param>
        /// <param name="pActorAncestor"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Add dbTransaction  
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsActionAllowed(string pCS, int pIdT, int pIdI, int pIdPermission, int pIdA, ActorAncestor pActorAncestor, out string opMsgControl)
        {
            return IsActionAllowed(pCS, null, pIdT, pIdI, pIdPermission, pIdA, pActorAncestor, out opMsgControl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdPermission"></param>
        /// <param name="pIdA"></param>
        /// <param name="pActorAncestor"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Add dbTransaction  
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static bool IsActionAllowed(string pCS, IDbTransaction pDbTransaction, int pIdT, int pIdI, int pIdPermission, int pIdA, ActorAncestor pActorAncestor, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            //Note: S'il n'existe aucun paramétrage, on admet que l'action est autorisée !
            bool ret = true;
            //
            //				//Note: Si la permission à contrôler n'est pas une InputAction, on retourne une erreur 
            //				if (false == PermissionTools.IsInputAction(pPermission))
            //					throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Permission paramameter is not an InputAction");
            //
            //Load ACTIONTUNING
            ActionTuning actionTuning = new ActionTuning(CSTools.SetCacheOn(pCS), pDbTransaction, pIdI, pIdPermission, pIdA, pActorAncestor);
            //
            if (actionTuning.DrSpecified)
            {
                //Load des statuts du trade
                TradeStatus tradeStatus = new TradeStatus();
                tradeStatus.Initialize(pCS, pDbTransaction, pIdT);
                //Load de la dernière modifications opérées sur le trade (dernier USER) 
                // EG 20240123 [WI816] Trade input: Implemented a new action to modify uninvoiced fees - GUI
                SQL_LastTrade_L sqlLastTrade_L = new SQL_LastTrade_L(pCS, pIdT, 
                    new PermissionEnum[] { PermissionEnum.Create, PermissionEnum.Modify, PermissionEnum.ModifyPostEvts, PermissionEnum.ModifyFeesUninvoiced, PermissionEnum.Remove })
                {
                    DbTransaction = pDbTransaction
                };

                ret = IsActionAllowed(pCS, pIdT, actionTuning, tradeStatus, sqlLastTrade_L.IdA, out  opMsgControl);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pActionTuning"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsActionAllowed(string pCS, int pIdT, ActionTuning pActionTuning, TradeStatus pTradeStatus, int pTradeLastIda, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            //Note: S'il n'existe aucun paramétrage, on admet que l'action est autorisée !
            bool ret = true;
            if (pActionTuning.DrSpecified)
            {
                //Contrôle de la bonne compatibilité
                ErrorLevelTuningEnum errLevel = pActionTuning.ScanCompatibility(pCS, pIdT, pTradeStatus, pTradeLastIda, out opMsgControl);
                ret = (ErrorLevelTuningEnum.SUCCESS == errLevel);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStCheckTuning"></param>
        /// <param name="pStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsStatusCheckAllowed(string pCS, int pIdT, StCheckTuning pStCheckTuning, CommonStatus pStatus, int pTradeLastIda, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            //Note: S'il n'existe aucun paramétrage, on admet que l'action est autorisée !
            bool ret = true;

            if (pStCheckTuning.DrSpecified)
            {
                //Contrôle de la bonne compatibilité
                ErrorLevelTuningEnum errLevel = pStCheckTuning.ScanCompatibility(pCS, pIdT, pStatus, pTradeLastIda, out opMsgControl);
                ret = (ErrorLevelTuningEnum.SUCCESS == errLevel);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdI"></param>
        /// <param name="pStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="pIdA"></param>
        /// <param name="pActorAncestor"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsStatusCheckAllowed(string pCS, int pIdT, int pIdI, CommonStatus pStatus, int pTradeLastIda, int pIdA, ActorAncestor pActorAncestor, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            bool ret = true;

            StatusCollection stChecks = pStatus.GetStUsersCollection(StatusEnum.StatusCheck);

            foreach (Status.Status item in stChecks.Status.Where(x => x.IsModify && BoolFunc.IsTrue(x.NewValue)))
            {
                string status = item.NewSt;

                StCheckTuning stCheck = new StCheckTuning(pCS, pIdI, status, pIdA, pActorAncestor);
                if (false == IsStatusCheckAllowed(pCS, pIdT, stCheck, pStatus, pTradeLastIda, out string msgControl))
                {
                    ret = false;
                    if (StrFunc.IsFilled(opMsgControl))
                        opMsgControl += Cst.CrLf;
                    opMsgControl += msgControl;
                }
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStMatchTuning"></param>
        /// <param name="pStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsStatusMatchAllowed(string pCS, int pIdT, StMatchTuning pStMatchTuning, CommonStatus pStatus, int pTradeLastIda, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            //Note: S'il n'existe aucun paramétrage, on admet que l'action est autorisée !
            bool ret = true;

            if (pStMatchTuning.DrSpecified)
            {
                //Contrôle de la bonne compatibilité
                ErrorLevelTuningEnum errLevel = pStMatchTuning.ScanCompatibility(pCS, pIdT, pStatus, pTradeLastIda, out opMsgControl);
                ret = (ErrorLevelTuningEnum.SUCCESS == errLevel);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdI"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeLastIda"></param>
        /// <param name="pIdA"></param>
        /// <param name="pActorAncestor"></param>
        /// <param name="opMsgControl"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Chgt de signature out à la place de ref pour pMsgControl
        public static bool IsStatusMatchAllowed(string pCS, int pIdT, int pIdI, CommonStatus pTradeStatus, int pTradeLastIda, int pIdA, ActorAncestor pActorAncestor, out string opMsgControl)
        {
            opMsgControl = string.Empty;
            bool ret = true;

            StatusCollection stMatchs = pTradeStatus.GetStUsersCollection(StatusEnum.StatusMatch);
            if (null != stMatchs)
            {
                for (int i = 0; i < stMatchs.Status.Length; i++)
                {
                    if ((stMatchs.Status[i].IsModify) && BoolFunc.IsTrue((stMatchs.Status[i].NewValue)))
                    {
                        string status = stMatchs.Status[i].NewSt;
                        StMatchTuning stMatch = new StMatchTuning(pCS, pIdI, status, pIdA, pActorAncestor);
                        if (false == IsStatusMatchAllowed(pCS, pIdT, stMatch, pTradeStatus, pTradeLastIda, out string msgControl))
                        {
                            ret = false;
                            if (StrFunc.IsFilled(opMsgControl))
                                opMsgControl += Cst.CrLf;
                            opMsgControl += msgControl;

                        }
                    }
                }
            }
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTuningInputType"></param>
        /// <returns></returns>
        /// FI 20190524 [23912] Add
        public static TuningInputClassEnum GetTuningClass(TuningInputTypeEnum pTuningInputType)
        {
            switch (pTuningInputType)
            {
                case TuningInputTypeEnum.IEI:
                case TuningInputTypeEnum.ITI:
                    return TuningInputClassEnum.Ignore;

                case TuningInputTypeEnum.IEP:
                case TuningInputTypeEnum.ITP:
                    return TuningInputClassEnum.Process;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Value : {0} is not implemented", pTuningInputType));
            }
        }

    }
}