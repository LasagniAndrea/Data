#region Using Directives
using System;
using System.Linq; 
using System.Drawing;
using System.Text;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic; 

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.Actor;
#endregion Using Directives


namespace EFS.Status
{
    #region Enum
    /// <summary>
    /// 
    /// </summary>
    /// FI 20161124 [22634] Add
    public enum Mode
    {
        Event,
        Trade,
    }

    public enum ErrorLevelTuningEnum
    {
        SUCCESS,
        ERROR,
        ERR_ACTIVATION,
        ERR_USERTRADE,
        ERR_USERSTCHECK,
        ERR_USERSTMATCH,
        ERR_USERSTATUS,
        ERR_MINCHECK,
        ERR_MAXCHECK,
        ERR_MINMATCH,
        ERR_MAXMATCH,
        ERR_CNFMSGGEN,
        ERR_ESRGEN,
        ERR_EARGEN,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum MinMaxTuningEnum
    {
        /// <summary>
        /// Poids minimum attendu
        /// </summary>
        MIN,
        /// <summary>
        /// Poids maximum attendu
        /// </summary>
        MAX
    }
    /// <summary>
    /// StatusEnvironment, StatusBusiness, StatusActivation, StatusPriority, StatusCheck, StatusMatch, StatusUsedBy
    /// </summary>
    /// FI 20161124 [22634] Attention il ne faut pas toucher à l'ordre
    public enum StatusEnum
    {
        /// <summary>
        /// REGULAR, SIMULATION, TEMPLATE
        /// </summary>
        StatusEnvironment,
        /// <summary>
        /// ALLOCATION, EXECUTION, PRE-TRADE
        /// </summary>
        StatusBusiness,
        /// <summary>
        /// REGULAR, INCOMPLETE, MISSING, DEACTIVE
        /// </summary>
        StatusActivation,
        /// <summary>
        /// REGULAR, HIGH, LOW
        /// </summary>
        StatusPriority,
        /// <summary>
        /// Statut USER CHECK
        /// </summary>
        StatusCheck,
        /// <summary>
        /// Statut USER MATCH
        /// </summary>
        StatusMatch,
        /// <summary>
        /// 
        /// </summary>
        StatusUsedBy
    }

    /// <summary>
    /// Current , New
    /// </summary>
    public enum StatusCurrentNew
    {
        Current,
        New
    }

    #endregion Enum

    /// <summary>
    /// Représente un statut
    /// </summary>
    public class Status : ICloneable
    {

        #region members
        /// <summary>
        /// type de statut
        /// </summary>
        private readonly StatusEnum _statusType;
        /// <summary>
        /// Liste des valeurs possibles du status
        /// </summary>
        private ExtendEnum _extendEnum;
        /// <summary>
        /// Obtient et définit un flag associé à la présence du initiale du statut (1 si statut présent,0 si non présent)
        /// <para>Les statuts système sont tjs présents</para>
        /// <para>Les statuts user sont optionels</para>
        /// </summary>
        private int _currentValue;
        /// <summary>
        /// Obtient et définit un flag associé à la présence du statut (1 si statut présent,0 si non présent)
        /// <para>Les statuts système sont tjs présents</para>
        /// <para>Les statuts user sont optionels</para>
        /// </summary>
        private int _newValue;
        /// <summary>
        /// statut initial 
        /// </summary>
        private string _currentSt;
        /// <summary>
        /// statut saisi
        /// </summary>
        private string _newSt;
        /// <summary>
        /// Date d'effet initiale
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        private Nullable<DateTime> _currentDtEffect;
        /// <summary>
        /// Date d'effet saisie
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        private Nullable<DateTime> _newDtEffect;
        /// <summary>
        /// Note initiale
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        private String _currentNote;
        /// <summary>
        /// Note saisie
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        private String _newNote;

        #endregion

        #region Accessors
        /// <summary>
        /// Obtient ou définit la valeur initiale du statut  
        /// </summary>
        public string CurrentSt
        {
            get { return _currentSt; }
            set
            {
                _currentSt = value;
                _newSt = value;
            }
        }
        /// <summary>
        /// Obtient ou définit la valeur du statut  
        /// </summary>
        public string NewSt
        {
            get { return _newSt; }
            set
            {
                _newSt = value;
                IsModify = (NewSt != CurrentSt);
            }
        }
        /// <summary>
        /// Obtient et définit un flag associé à la présence du initiale du statut (1 si statut présent,0 si non présent)
        /// <para>Les statuts système sont tjs présents</para>
        /// </summary>
        public int CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                _newValue = value;
            }
        }
        /// <summary>
        /// Obtient et définit un flag associé à la présencedu statut (1 si statut présent,0 si non présent)
        /// <para>Les statuts système sont tjs présents</para>
        /// </summary>
        public int NewValue
        {
            get { return _newValue; }
            set
            {
                _newValue = value;
                IsModify = (NewValue != CurrentValue);
            }
        }

        /// <summary>
        /// Obtient le type du statut
        /// </summary>
        public StatusEnum StatusType
        {
            get { return _statusType; }
        }

        /// <summary>
        /// Obtient et définit la date d'effet initiale
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        public Nullable<DateTime> CurrentDtEffect
        {
            get { return _currentDtEffect; }
            set
            {
                _currentDtEffect = value;
                _newDtEffect = value;
            }
        }
        /// <summary>
        /// Obtient et définit la date d'effet
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        public Nullable<DateTime> NewDtEffect
        {
            get { return _newDtEffect; }
            set { _newDtEffect = value; }
        }


        /// <summary>
        /// Obtient et définit la note initiale
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        public String CurrentNote
        {
            get { return _currentNote; }
            set
            {
                _currentNote = value;
                _newNote = value;
            }
        }
        /// <summary>
        /// Obtient et définit la note
        /// <para>Donnée présente uniquement sur les statuts user (Check ou Match)</para>
        /// </summary>
        /// FI 20140728 [20255] add 
        public string NewNote
        {
            get { return _newNote; }
            set { _newNote = value; }
        }


        /// <summary>
        /// Obtient true si le statut changé
        /// </summary>
        public bool IsModify
        {
            set;
            get;
        }
        /// <summary>
        /// Obtient ou définit la date système initiale du statut (date système)
        /// </summary>
        public DateTime CurrentDtSys
        {
            set;
            get;
        }
        /// <summary>
        /// Obtient l'utilisateur initial du statut 
        /// </summary>
        public int CurrentIdA
        {
            set;
            get;
        }
        /// <summary>
        /// Obtient ou définit un libellé complémentaire initial du statut (exclusivement sur StUsedBy)
        /// </summary>
        public string CurrentLibSt
        {
            set;
            get;
        }
        /// <summary>
        /// Obtient ou définit le poids initial du statut
        /// </summary>
        public int CurrentWeight
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définit le titre de l'infoBulle sur la valeur du statut
        /// </summary>
        /// EG 20160119 Refactoring Header
        public string TooltipTitle
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définit une infoBulle sur la valeur du statut
        /// </summary>
        /// FI 20140728 [20255] add property
        public string Tooltip
        {
            set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public ExtendEnum ExtendEnum
        {

            get { return _extendEnum; }

            set
            {
                _extendEnum = value;
                if (StatusTools.IsStatusSystem(_statusType))
                {
                    for (int i = 0; i < _extendEnum.item.Length; i++)
                    {
                        _extendEnum.item[i].ExtValue = Ressource.GetString(_extendEnum.item[i].Value, _extendEnum.item[i].ExtValue);
                    }
                }
            }
        }
        /// <summary>
        /// Obtient l'enum associé à la valeur initiale du statut 
        /// </summary>
        public ExtendEnumValue CurrentStExtend
        {
            get
            {
                return _extendEnum.GetExtendEnumValueByValue(this.CurrentSt);
            }
        }
        /// <summary>
        /// Obtient l'enum associé à la valeur du statut 
        /// </summary>
        public ExtendEnumValue NewStExtend
        {
            get
            {
                return _extendEnum.GetExtendEnumValueByValue(this.NewSt);
            }
        }

        /// <summary>
        /// Obtient true si NewSt != CurrentSt
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return (NewSt != CurrentSt);
            }
        }


        #endregion Accessors

        #region Constructor
        public Status() { }
        public Status(StatusEnum pStatus)
        {
            this._statusType = pStatus;
            this.CurrentSt = string.Empty;
            this.Tooltip = string.Empty;
        }
        #endregion Constructor


        /// <summary>
        /// Alimente les propriétés Tooltip et TooltipTitle
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="auditTimestampInfo">pour piloter l'affichage des horodatages</param>
        /// FI 20140728 [20255] Rename methode en InitializeTooltip
        /// EG 20160119 Refactoring Header
        /// FI 20200820 [25468] Add parameter pAuditTimestampZone et pCollaborator
        /// EG 20220210 [25939] Amélioration du Log sur une demande d'annulation rejetée pour cause de Lock exclusif sur un autre traitement
        public void SetTooltip(string pCS, AuditTimestampInfo auditTimestampInfo)
        {
            //string tooltip = StatusTools.LibStatusType(this._statusType) + ":";
            string tooltip = string.Empty;

            if (StatusTools.IsStatusSystem(this._statusType))
            {
                string Label;
                // New            
                if (this.IsModify)
                {
                    Label = Ressource.GetString("New", true);
                    tooltip = Label + ":" + Cst.Tab + this.NewStExtend.ExtValue + " - " + this.NewStExtend.Documentation + Cst.CrLf;
                }

                // Current
                Label = Ressource.GetString(this.IsModify ? "Previous" : "Current", true);
                tooltip += Label + ":" + Cst.Tab + this.CurrentStExtend.ExtValue + " - " + this.CurrentStExtend.Documentation;
                if ((null != this.CurrentLibSt) && StrFunc.IsFilled(this.CurrentLibSt))
                {
                    string resLabel = Ressource.GetString(this.CurrentLibSt, true);
                    tooltip += Cst.CrLf + "  " + Ressource.GetString("PROCESS") + ":" + Cst.Tab + this.CurrentLibSt + (this.CurrentLibSt == resLabel ? string.Empty : " - " + resLabel);
                }
            }
            else if (StatusTools.IsStatusUser(this._statusType)) // Spheres® affiche les valeurs en vigueur (utilisation des membres new) 
            {
                // New
                string Label = Ressource.GetString("Current", true); //tips label current => c'est normal => Spheres® affiche les données en vigueur 
                tooltip = Label + ":" + Cst.Tab + this.NewStExtend.ExtValue + " - " + this.NewStExtend.Documentation;

                if (this.NewDtEffect.HasValue)
                    tooltip += Cst.CrLf + "  " + Ressource.GetString("RelevantDate", true) + ":" + Cst.Tab + this.NewDtEffect.Value.ToLongDateString();

                if (StrFunc.IsFilled(this.NewNote))
                    tooltip += Cst.CrLf + "  " + Ressource.GetString("NOTE", true) + ":" + Cst.Tab + Cst.Tab + this.NewNote;
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", this._statusType.ToString()));
            }


            Boolean isAddCurrentInfo = StatusTools.IsStatusSystem(this._statusType) ||
                                       ((StatusTools.IsStatusUser(this._statusType) && (false == this.IsModify)));
            if (isAddCurrentInfo)
            {
                if (this.CurrentIdA != 0)
                {
                    //20090720 FI Add SetCacheOn
                    SQL_Actor sql_Actor = new SQL_Actor(CSTools.SetCacheOn(pCS), this.CurrentIdA);
                    if (sql_Actor.IsLoaded)
                        tooltip += Cst.CrLf + "  " + Ressource.GetString("Collaborator", true) + ":" + Cst.Tab + sql_Actor.Identifier;
                }

                if (this.CurrentDtSys != Convert.ToDateTime(null))
                {
                    // FI 20200820 [25468] Appel à DtFuncExtended.DisplayDateSys
                    tooltip += Cst.CrLf + "  " + Ressource.GetString("Date", true) + ":" + Cst.Tab + Cst.Tab +
                        DtFuncExtended.DisplayTimestampUTC(CurrentDtSys, auditTimestampInfo);
                }
            }

            this.TooltipTitle = StatusTools.LibStatusType(this._statusType);
            this.Tooltip = tooltip;
        }

        /// <summary>
        /// NewValue = CurrentValue
        /// </summary>
        /// FI 20140728 [20255] Modify
        public void ResetNewValueFromCurrent()
        {
            NewValue = CurrentValue;
            //FI 20140728 [20255] IsModify est positionné à False
            IsModify = false;
        }

        /// <summary>
        /// NewValue = 0
        /// </summary>
        public void ResetNewValue()
        {
            NewValue = 0;
        }


        #region Membres de ICloneable
        public object Clone()
        {
            Status clone = new Status(_statusType)
            {
                ExtendEnum = ExtendEnum,

                CurrentValue = CurrentValue,
                NewValue = NewValue,

                CurrentSt = CurrentSt,
                NewSt = NewSt,

                CurrentDtEffect = CurrentDtEffect,
                NewDtEffect = NewDtEffect,

                IsModify = IsModify,
                CurrentDtSys = CurrentDtSys,
                CurrentIdA = CurrentIdA,
                CurrentWeight = CurrentWeight,

                Tooltip = Tooltip
            };

            return clone;
        }
        #endregion Membres de ICloneable

    }

    /// <summary>
    /// <para>Liste des status Match (Repository STMATCH)</para>
    /// <para>Liste des status Check (Repository STCHECK)</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("STWEIGHTS", Namespace = "", IsNullable = false)]
    public class StWeightCollection
    {
        /// <summary>
        /// Type MATCH,CHECK
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StatusEnum Statut;

        [System.Xml.Serialization.XmlElementAttribute("STWEIGHT")]
        public StWeight[] StWeight;

        #region indexor
        /// <summary>
        ///  Obtient le statut {pIdSt}
        /// </summary>
        /// <param name="pIdSt"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify
        public StWeight this[string pIdSt]
        {
            get
            {
                StWeight ret = null;
                for (int i = 0; i < StWeight.Length; i++)
                {
                    if (StWeight[i].IdSt == pIdSt)
                    {
                        ret = StWeight[i];
                        break;// FI 20140708 [20179] add Break
                    }
                }
                return ret;
            }
        }
        #endregion indexor

        #region Method
        /// <summary>
        /// Chargement des statuts Match ou Checks
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public StWeightCollection LoadWeight(string pCS)
        {
            return LoadWeight(pCS, null);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public StWeightCollection LoadWeight(string pCS, IDbTransaction pDbTransaction)
        {
            Cst.OTCml_TBL table = StatusTools.GetTableNameStatusUser(Statut);
            string columnId = StatusTools.GetColumnNameStatusUser(Statut);

            //PL 20111021 Add all GetSQLDataDtEnabled() 
            // FI 20140708 [20179] add column CUSTOMVALUE
            string SQLQuery = SQLCst.SELECT + columnId + " as IDST,WEIGHT,CUSTOMVALUE" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + table + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, table) + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "WEIGHT";

            //FI 20121113 [18224] add CacheOn
            DataSet dsResult = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, SQLQuery);
            dsResult.DataSetName = "STWEIGHTS";
            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "STWEIGHT";

            string serializerResult = new DatasetSerializer(dsResult).Serialize();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(StWeightCollection), serializerResult);
            StWeightCollection stWeights = (StWeightCollection)CacheSerializer.Deserialize(serializeInfo);
            return stWeights;
        }
        #endregion Method
    }

    /// <summary>
    /// 
    /// </summary>
    public class StMatchWeightCollection : StWeightCollection
    {
        public StMatchWeightCollection()
        {
            base.Statut = StatusEnum.StatusMatch;
        }
    }

    /// <summary>
    /// Collection de statuts de type CHECK
    /// </summary>
    public class StCheckWeightCollection : StWeightCollection
    {
        /// <summary>
        /// 
        /// </summary>
        public StCheckWeightCollection()
        {
            base.Statut = StatusEnum.StatusCheck;
        }
    }

    /// <summary>
    /// Représente un statut et son poids
    /// </summary>
    /// FI 20140708 [20179] add CUSTOMVALUE
    public class StWeight
    {
        /// <summary>
        /// Identifiant du poids
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDST")]
        public string IdSt;

        /// <summary>
        /// Valeur du poids
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("WEIGHT")]
        public int Weight;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("CUSTOMVALUE")]
        public String CustomValue;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class StatusTools
    {
        /// <summary>
        /// Obtient true si le statut est un statut user  (StatusCheck,StatusMatch)
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public static bool IsStatusUser(StatusEnum pStatusEnum)
        {
            bool isStatusSystem = (pStatusEnum == StatusEnum.StatusEnvironment)
                || (pStatusEnum == StatusEnum.StatusBusiness)
                || (pStatusEnum == StatusEnum.StatusActivation)
                || (pStatusEnum == StatusEnum.StatusPriority)
                || (pStatusEnum == StatusEnum.StatusUsedBy);
            return !isStatusSystem;
        }

        /// <summary>
        /// Obtient true si le statut est un statut système (StatusEnvironment, StatusBusiness,StatusActivation,StatusPriority,StatusUsedBy)
        /// </summary>
        public static bool IsStatusSystem(StatusEnum pStatusEnum)
        {
            return !StatusTools.IsStatusUser(pStatusEnum);
        }

        /// <summary>
        ///  Retourne les types de statuts disponibles 
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        /// FI 20161124 [22634] Add Method
        public static StatusEnum[] GetAvailableStatus(Mode pMode)
        {
            StatusEnum[] ret = null;

            switch (pMode)
            {
                case Mode.Trade:
                    ret = (from item in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                           select item).ToArray();
                    break;
                case Mode.Event:
                    List<StatusEnum> lst = new List<StatusEnum>
                    {
                        StatusEnum.StatusActivation
                    };
                    lst.AddRange(from item in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>().Where(x => IsStatusUser(x))
                                 select item);
                    ret = lst.ToArray();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("table :{0} is not implemeted", pMode.ToString()));
            }

            return ret;
        }


        /// <summary>
        /// Retourne TRADESTCHECK ou TRADESTCHECK
        /// </summary>
        public static Cst.OTCml_TBL GetTradeTableNameStatusUser(StatusEnum pStatusEnum)
        {
            return (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), "TRADE" + GetTableNameStatusUser(pStatusEnum), true);
        }
        /// <summary>
        /// Retourne EVENTSTMATCH ou EVENTSTCHECK
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public static Cst.OTCml_TBL GetEventTableNameStatusUser(StatusEnum pStatusEnum)
        {
            return (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), "EVENT" + GetTableNameStatusUser(pStatusEnum), true);
        }
        /// <summary>
        /// Retourne le nom de la table repository associé à {pStatusEnum} 
        /// <para>Exemple StatusMatch => STMATCH</para>
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public static Cst.OTCml_TBL GetTableNameStatusUser(StatusEnum pStatusEnum)
        {
            return (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pStatusEnum.ToString().Replace("Status", "ST"), true);
        }
        /// <summary>
        /// Retourne TRADESTMATCH ou TRADESTCHECK ou EVENTSTMATCH ou EVENTSTCHECK 
        /// </summary>
        /// <param name="pStatusEnum">StatusCheck ou StatusMatch</param>
        /// <param name="pTableName">TRADE ou EVENT</param>
        /// <returns></returns>
        public static Cst.OTCml_TBL GetTableNameStatusUser(StatusEnum pStatusEnum, Cst.OTCml_TBL pTableName)
        {
            //Warning: La valeur de l'enum doit correspondre au nom de la table (eg: StatusCheck --> TRADESTCHECK)
            return (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pTableName.ToString() + GetTableNameStatusUser(pStatusEnum), true);
        }
        /// <summary>
        /// Retourne IDSTMATCH ou IDSTCHECK
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public static string GetColumnNameStatusUser(StatusEnum pStatusEnum)
        {
            //Warning: La valeur de l'enum doit correspondre au nom de la colone (eg: StatusCheck --> IDSTCHECK)
            return "ID" + GetTableNameStatusUser(pStatusEnum);
        }

        /// <summary>
        /// Retourne la ressource du type de statut {pStatusEnum}
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public static string LibStatusType(StatusEnum pStatusEnum)
        {
            return Ressource.GetString(pStatusEnum.ToString().Replace("Status", "lblSt"));
        }

        /// <summary>
        ///  Applique le statut d'activation <paramref name="status"/> au trade <paramref name="idT"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idT"></param>
        /// <param name="status"></param>
        /// <param name="idA"></param>
        /// <param name="dtsys"></param>
        /// <returns></returns>
        /// FI 20240618 [WI974] Add method
        public static Boolean UpdateTradeStatusActivation(string cs, int idT, Cst.StatusActivation status, int idA, DateTime dtsys)
        {

            string sqlQuery = $"Update dbo.TRADE set IDSTACTIVATION=@IDSTACTIVATION, IDASTACTIVATION=@IDA, DTSTACTIVATION=@DT where IDT=@IDT";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), status);
            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDA), idA);
            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DT), dtsys);
            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDT), idT);

            QueryParameters queryParameters = new QueryParameters(cs, sqlQuery, parameters);

            int nbRow = DataHelper.ExecuteNonQuery(cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return (nbRow > 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class CommonStatus
    {

        #region Declare Member
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stActivationSpecified;
        /// <summary>
        /// Statut Activation: MISSING , DEACTIVE, REGULAR
        /// </summary>
        [System.Xml.Serialization.XmlElement("stActivation", typeof(Status))]
        public Status stActivation;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stUsedBySpecified;
        /// <summary>
        /// Statut Utilisation: Statut pour indiquer une utilisation en cours
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Status stUsedBy;
        /// <summary>
        /// tableau de status user 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StatusCollection[] stUsers;
        #endregion Declare Member

        #region constructor
        public CommonStatus()
        {
            //
            stActivation = new Status(StatusEnum.StatusActivation);
            stUsedBy = new Status(StatusEnum.StatusUsedBy);
            //
            #region StUsers
            ArrayList lst = new ArrayList();
            foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                .Where(x => StatusTools.IsStatusUser(x)))
            {
                StatusCollection status = new StatusCollection(statusEnum);
                lst.Add(status);
            }
            stUsers = (StatusCollection[])lst.ToArray(typeof(StatusCollection));
            #endregion StUsers
        }
        #endregion constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        protected virtual void InitializeEnums(string pCS)
        {
            //FI 20120124 mise en commentaire les enums sont chargés à la connexion ou en cas de modifs
            //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(pCS);
            //
            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnumHelper
            ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
            stActivation.ExtendEnum = ListEnumsSchemes[StatusEnum.StatusActivation.ToString()];
            stUsedBy.ExtendEnum = ListEnumsSchemes[StatusEnum.StatusUsedBy.ToString()];
            */

            ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, StatusEnum.StatusActivation.ToString());
            stActivation.ExtendEnum = extendEnum ?? throw new KeyNotFoundException($"Key:{StatusEnum.StatusActivation} not found.");

            extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, StatusEnum.StatusUsedBy.ToString());
            stUsedBy.ExtendEnum = extendEnum ?? throw new KeyNotFoundException($"Key:{StatusEnum.StatusUsedBy} not found.");

            //
            for (int i = 0; i < stUsers.Length; i++)
            {
                Status[] stList = stUsers[i].Status;
                for (int j = 0; j < stList.Length; j++)
                {
                    // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnumHelper
                    //stList[j].ExtendEnum = ListEnumsSchemes[stUsers[i].StatusEnum.ToString()];
                    stList[j].ExtendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, stUsers[i].StatusEnum.ToString());
                }
            }
        }



        /// <summary>
        /// Retourne la valeur current ou new présente dans le satut {pStatusType}
        /// </summary>
        /// <param name="pStatusType"></param>
        /// <param name="pCurrentNew"></param>
        /// <returns></returns>
        public virtual string GetStatusValue(StatusEnum pStatusType, StatusCurrentNew pCurrentNew)
        {
            string ret = string.Empty;
            bool isCurrent = (pCurrentNew == StatusCurrentNew.Current);

            switch (pStatusType)
            {
                case StatusEnum.StatusActivation:
                    ret = (isCurrent ? stActivation.CurrentSt : stActivation.NewSt);
                    break;
                case StatusEnum.StatusUsedBy:
                    ret = (isCurrent ? stUsedBy.CurrentSt : stUsedBy.NewSt);
                    break;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatusType"></param>
        /// <returns></returns>
        public virtual Status GetStatus(StatusEnum pStatusType)
        {
            Status ret = null;

            switch (pStatusType)
            {
                case StatusEnum.StatusActivation:
                    ret = stActivation;
                    break;
                case StatusEnum.StatusUsedBy:
                    ret = stUsedBy;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Le User qui a saisie le Status 
        /// => Attention aujourd'hui à chaque modification on delete tout avant réinsertion => Tous les status user ont donc le même user
        /// => On s'arrête dès que l'on en trouve 1 A voir plus tard .........    
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        public virtual int GetUser(StatusEnum pStatusEnum)
        {
            int ret = 0;
            switch (pStatusEnum)
            {
                case StatusEnum.StatusActivation:
                    ret = stActivation.CurrentIdA;
                    break;
                case StatusEnum.StatusUsedBy:
                    ret = stUsedBy.CurrentIdA;
                    break;
                case StatusEnum.StatusCheck:
                case StatusEnum.StatusMatch:
                    StatusCollection statusCol = GetStUsersCollection(pStatusEnum);
                    if (null != statusCol)
                    {
                        for (int i = 0; i < statusCol.Status.Length; i++)
                        {
                            if (statusCol[i].CurrentValue == 1)
                            {
                                ret = statusCol[i].CurrentIdA;
                            }
                        }
                    }
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrLevel"></param>
        /// <param name="pActor"></param>
        /// <returns></returns>
        public string GetStepErrExplanation(ErrorLevelTuningEnum pErrLevel, string pActor)
        {

            string res;
            switch (pErrLevel)
            {
                case ErrorLevelTuningEnum.ERR_MINMATCH:
                case ErrorLevelTuningEnum.ERR_MAXMATCH:
                    res = Ressource.GetString("Msg_ErrorStMatch");
                    break;
                case ErrorLevelTuningEnum.ERR_MINCHECK:
                case ErrorLevelTuningEnum.ERR_MAXCHECK:
                    res = Ressource.GetString("Msg_ErrorStCheck");
                    break;
                case ErrorLevelTuningEnum.ERR_USERTRADE:
                case ErrorLevelTuningEnum.ERR_USERSTCHECK:
                case ErrorLevelTuningEnum.ERR_USERSTMATCH:
                    res = Ressource.GetString2("Msg_ErrorUser", pActor);
                    break;
                default:
                    res = Ressource.GetString("Err Unknown", "Err Unknown");
                    break;
            }
            return res;
        }

        /// <summary>
        /// Retourne les status de type {pStatusEnum}
        /// </summary>
        /// <param name="pStatusEnum">StatusCheck ou StatusMatch</param>
        /// <returns></returns>
        // FI 20140708 [20179] Modify
        public StatusCollection GetStUsersCollection(StatusEnum pStatusEnum)
        {
            StatusCollection ret = null;

            if (ArrFunc.IsFilled(stUsers))
            {
                ret = (from item in stUsers.Where(x => x.StatusEnum == pStatusEnum)
                       select item).First();
            }

            return ret;
        }


        /// <summary>
        /// Retourne succès si les poids des statu de type {pStatusEnum} repectent le poids (min ou max attendu) 
        /// </summary>
        /// <param name="pStatusEnum">type de statut</param>
        /// <param name="pMinMax">Type de contrôle (Min ou max)</param>
        /// <param name="pValue">la valeur de référence pour le contôle</param>
        /// <param name="pCtrlSt"></param>
        /// <returns></returns>
        public ErrorLevelTuningEnum ScanUserStatusCompatibility(StatusEnum pStatusEnum, MinMaxTuningEnum pMinMax, int pStatus, Cst.CtrlStatusEnum pCtrlSt)
        {
            if (false == StatusTools.IsStatusUser(pStatusEnum))
                throw new NotSupportedException(StrFunc.AppendFormat("{0} is not an user status", pStatusEnum));


            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;

            StatusCollection statusCollection = GetStUsersCollection(pStatusEnum);
            if (null != statusCollection)
                errLevel = statusCollection.ScanCompatibility(pMinMax, pStatus, pCtrlSt);

            return errLevel;

        }



        /// <summary>
        /// Alimentation de la table TRADESTMATCH ou TRADESTCHECK ou EVENTSTCHECK ou EVENTSTMATCH
        /// <para>Suppression des enregistrements existants et insertion des statuts checkés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pId">IdT or IdE</param>
        /// <param name="pIdA">Représente l'utilisateur</param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        /// FI 20161124 [22634] Modify
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        // EG 20190613 [24683] Use pDbTransaction
        public bool UpdateStUser(string pCS, IDbTransaction pDbTransaction, Mode pMode, int pId, int pIdA, DateTime pDtSys)
        {

            bool isOk = true;

            foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                .Where(x => StatusTools.IsStatusUser(x)))
            {
                string columnId = string.Empty;

                SQL_StUser sqlStUser = null;
                switch (pMode)
                {
                    case Mode.Trade:
                        columnId = OTCmlHelper.GetColunmID(Cst.OTCml_TBL.TRADE.ToString());
                        sqlStUser = new SQL_TradeStUser(pCS, pId, statusEnum) as SQL_StUser;
                        break;
                    case Mode.Event:
                        columnId = OTCmlHelper.GetColunmID(Cst.OTCml_TBL.EVENT.ToString());
                        sqlStUser = new SQL_EventStUser(pCS, pId, statusEnum) as SQL_StUser;
                        break;
                }
                sqlStUser.DbTransaction = pDbTransaction;


                string columnName = StatusTools.GetColumnNameStatusUser(statusEnum);
                string[] columns = new string[] { columnId, columnName, "DTEFFECT", "LONOTE", "DTINS", "IDAINS" };
                string sqlQuery = sqlStUser.GetQueryParameters(columns).QueryReplaceParameters;
                sqlStUser.DbTransaction = pDbTransaction;
                sqlStUser.LoadTable(columns);

                int nbRemove = 0;
                if (sqlStUser.IsFound)
                {
                    //Remove des anciennes valeurs
                    foreach (DataRow dataRow in sqlStUser.Dt.Rows)
                    {
                        nbRemove++;
                        dataRow.Delete();
                    }
                }


                UpdateSpecifiedtUser(statusEnum, sqlStUser.Dt, columnId, pId, pIdA, pDtSys, out int nbAdd);

                int rowsAffected = DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlQuery, sqlStUser.Dt);
                isOk = ((nbAdd + nbRemove) == rowsAffected);

                if (!isOk)
                    break;
            }

            return isOk;
        }

        /// <summary>
        ///  Insère les statuts en base de données
        /// </summary>
        /// <param name="pStatusEnum">StatusCheck ou StatusMatch</param>
        /// <param name="pDataTable">Représente TRADESTMATCH/EVENTSTMATCH ou TRADESTCHECK/EVENTSTCHECK</param>
        /// <param name="pId">Trade ou Event concerné</param>
        /// <param name="pIdA">Représente l'utilisateur</param>
        /// <param name="pDtSys"></param>
        /// <param name="opNbAdd"></param>
        /// FI 20120911 [18118] nouvelle signature 
        /// FI 20140728 [20255] Modify 
        private void UpdateSpecifiedtUser(StatusEnum pStatusEnum, DataTable pDataTable, string pColumnId, int pId, int pIdA, DateTime pDtSys, out int opNbAdd)
        {
            int nbAdd = 0;

            string columnName = StatusTools.GetColumnNameStatusUser(pStatusEnum);
            StatusCollection stUserstype = this.GetStUsersCollection(pStatusEnum);

            if ((null != stUserstype) && (null != stUserstype.Status))
            {
                for (int i = 0; i < stUserstype.Status.Length; i++)
                {
                    Status status = stUserstype.Status[i];
                    if (status.NewValue > 0)
                    {
                        nbAdd++;
                        DataRow dr = pDataTable.NewRow();

                        dr.BeginEdit();
                        dr[pColumnId] = pId;
                        dr[columnName] = status.NewSt;
                        dr["DTEFFECT"] = status.NewDtEffect ?? Convert.DBNull;
                        dr["LONOTE"] = StrFunc.IsFilled(status.NewNote) ? status.NewNote : Convert.DBNull;
                        dr["DTINS"] = pDtSys;
                        dr["IDAINS"] = pIdA;
                        dr.EndEdit();

                        pDataTable.Rows.Add(dr);
                    }
                }
            }

            opNbAdd = nbAdd;
        }



    }

    /// <summary>
    /// Statuts des trades
    /// </summary>
    // EG 20190613 [24683] Use Datatable instead of DataReader
    public class TradeStatus : CommonStatus, ICloneable
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stEnvironmentSpecified;
        /// <summary>
        /// NORMAL, SIMULATION, TEMPLATE
        /// </summary>
        [System.Xml.Serialization.XmlElement("stEnvironment", typeof(Status))]
        public Status stEnvironment;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stBusinessSpecified;
        /// <summary>
        /// ALLOCATION, INTERMEDIATION, EXECUTION, PRE-TRADE
        /// </summary>
        [System.Xml.Serialization.XmlElement("stBusiness", typeof(Status))]
        public Status stBusiness;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stPrioritySpecified;
        /// <summary>
        /// HIGH, LOW, 
        /// </summary>
        [System.Xml.Serialization.XmlElement("stPriority", typeof(Status))]
        public Status stPriority;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_TradeStSys sqlTradeStSysSource;
        #endregion  Members

        #region constructor
        public TradeStatus()
            : base()
        {
            stEnvironment = new Status(StatusEnum.StatusEnvironment);
            stBusiness = new Status(StatusEnum.StatusBusiness);
            stPriority = new Status(StatusEnum.StatusPriority);
        }
        #endregion constructor

        #region public Accesors IsActivation_XXX
        public bool IsCurrentStActivation_Regular
        {
            get { return (Cst.StatusActivation.REGULAR.ToString() == stActivation.CurrentSt); }
        }
        public bool IsCurrentStActivation_Lock
        {
            get { return (Cst.StatusActivation.LOCKED.ToString() == stActivation.CurrentSt); }
        }
        public bool IsCurrentStActivation_Deactiv
        {
            get { return (Cst.StatusActivation.DEACTIV.ToString() == stActivation.CurrentSt); }
        }
        public bool IsCurrentStActivation_Missing
        {
            get { return (Cst.StatusActivation.MISSING.ToString() == stActivation.CurrentSt); }
        }
        //
        public bool IsStActivation_Regular
        {
            get { return (Cst.StatusActivation.REGULAR.ToString() == stActivation.NewSt); }
        }
        public bool IsStActivation_Lock
        {
            get { return (Cst.StatusActivation.LOCKED.ToString() == stActivation.NewSt); }
        }
        public bool IsStActivation_Deactiv
        {
            get { return (Cst.StatusActivation.DEACTIV.ToString() == stActivation.NewSt); }
        }
        public bool IsStActivation_Missing
        {
            get { return (Cst.StatusActivation.MISSING.ToString() == stActivation.NewSt); }
        }
        #endregion public Accesors IsActivation_XXX

        #region public Accessors IsUsedBy_XXX
        public bool IsCurrentStUsedBy_Regular
        {
            get { return (Cst.StatusUsedBy.REGULAR.ToString() == stUsedBy.CurrentSt); }
        }
        public bool IsStUsedBy_Regular
        {
            get { return (Cst.StatusUsedBy.REGULAR.ToString() == stUsedBy.NewSt); }
        }
        #endregion public Accessors IsUsedBy_XXX

        #region public Accesors IsEnvironment_XXX
        public bool IsCurrentStEnvironment_Simul
        {
            get { return (Cst.StatusEnvironment.SIMUL.ToString() == stEnvironment.CurrentSt); }
        }
        public bool IsCurrentStEnvironment_Regular
        {
            get { return (Cst.StatusEnvironment.REGULAR.ToString() == stEnvironment.CurrentSt); }
        }

        public bool IsStEnvironment_Template
        {
            get { return (Cst.StatusEnvironment.TEMPLATE.ToString() == stEnvironment.NewSt); }
        }
        public bool IsStEnvironment_Simul
        {
            get { return (Cst.StatusEnvironment.SIMUL.ToString() == stEnvironment.NewSt); }
        }
        public bool IsStEnvironment_Regular
        {
            get { return (Cst.StatusEnvironment.REGULAR.ToString() == stEnvironment.NewSt); }
        }

        public bool IsStEnvironmentValid
        {
            get
            {
                bool bRet = true;
                try
                {
                    Cst.StatusEnvironment status = (Cst.StatusEnvironment)System.Enum.Parse(typeof(Cst.StatusEnvironment), stEnvironment.NewSt, true);
                }
                catch
                { bRet = false; }
                return bRet;
            }
        }
        #endregion

        #region public Accesors IsBusiness_XXX
        public bool IsCurrentStBusiness_PreTrade
        {
            get { return (Cst.StatusBusiness.PRETRADE.ToString() == stBusiness.CurrentSt); }
        }
        public bool IsCurrentStBusiness_Executed
        {
            get { return (Cst.StatusBusiness.EXECUTED.ToString() == stBusiness.CurrentSt); }
        }
        public bool IsCurrentStBusiness_Intermed
        {
            get { return (Cst.StatusBusiness.INTERMED.ToString() == stBusiness.CurrentSt); }
        }
        public bool IsCurrentStBusiness_Alloc
        {
            get { return (Cst.StatusBusiness.ALLOC.ToString() == stBusiness.CurrentSt); }
        }

        public bool IsStBusiness_PreTrade
        {
            get { return (Cst.StatusBusiness.PRETRADE.ToString() == stBusiness.NewSt); }
        }
        public bool IsStBusiness_Executed
        {
            get { return (Cst.StatusBusiness.EXECUTED.ToString() == stBusiness.NewSt); }
        }
        public bool IsStBusiness_Intermed
        {
            get { return (Cst.StatusBusiness.INTERMED.ToString() == stBusiness.NewSt); }
        }
        public bool IsStBusiness_Alloc
        {
            get { return (Cst.StatusBusiness.ALLOC.ToString() == stBusiness.NewSt); }
        }

        public bool IsStBusinessValid
        {
            get
            {
                bool bRet = true;
                try
                {
                    Cst.StatusBusiness status = (Cst.StatusBusiness)System.Enum.Parse(typeof(Cst.StatusBusiness), stEnvironment.NewSt, true);
                }
                catch
                {
                    bRet = false;
                }
                return bRet;
            }
        }
        #endregion

        #region public override GetStatusValue
        /// <summary>
        /// Retourne la valeur current ou new présente dans le satut {pStatusType}
        /// </summary>
        /// <param name="pStatusType"></param>
        /// <param name="pCurrentNew"></param>
        /// <returns></returns>
        public override string GetStatusValue(StatusEnum pStatusType, StatusCurrentNew pCurrentNew)
        {
            bool isCurrentAsked = (pCurrentNew == StatusCurrentNew.Current);

            string retValue;
            switch (pStatusType)
            {
                case StatusEnum.StatusEnvironment:
                    retValue = (isCurrentAsked ? stEnvironment.CurrentSt : stEnvironment.NewSt);
                    break;
                //20100312 PL-StatusBusiness
                case StatusEnum.StatusBusiness:
                    retValue = (isCurrentAsked ? stBusiness.CurrentSt : stBusiness.NewSt);
                    break;
                case StatusEnum.StatusPriority:
                    retValue = (isCurrentAsked ? stPriority.CurrentSt : stPriority.NewSt);
                    break;
                default:
                    retValue = base.GetStatusValue(pStatusType, pCurrentNew);
                    break;

            }
            return retValue;
        }

        #endregion
        #region public override GetStatus
        public override Status GetStatus(StatusEnum pStatusType)
        {

            Status ret;
            switch (pStatusType)
            {
                case StatusEnum.StatusEnvironment:
                    ret = stEnvironment;
                    break;
                case StatusEnum.StatusBusiness:
                    ret = stBusiness;
                    break;
                case StatusEnum.StatusPriority:
                    ret = stPriority;
                    break;
                case StatusEnum.StatusActivation:
                    ret = base.GetStatus(pStatusType);
                    break;
                default:
                    throw new NotImplementedException("statut not implemanted");
            }
            return ret;

        }
        #endregion

        #region public ResetStUserNewValueFromCurrent
        public void ResetStUserNewValueFromCurrent(StatusEnum pStatusEnum)
        {
            StatusCollection statusCollection = GetStUsersCollection(pStatusEnum);
            statusCollection.ResetNewValueFromCurrent();
        }
        #endregion
        #region public ResetNewValueFromCurrent
        public void ResetNewValueFromCurrent()
        {
            stActivation.ResetNewValueFromCurrent();
            stBusiness.ResetNewValueFromCurrent();
            stEnvironment.ResetNewValueFromCurrent();
            stPriority.ResetNewValueFromCurrent();

            ResetStUserNewValueFromCurrent(StatusEnum.StatusMatch);
            ResetStUserNewValueFromCurrent(StatusEnum.StatusCheck);
        }
        #endregion
        #region public ResetStUserNewValue
        public void ResetStUserNewValue(StatusEnum pStatusEnum)
        {
            StatusCollection statusCollection = GetStUsersCollection(pStatusEnum);
            statusCollection.ResetNewValue();
        }
        #endregion

        #region public InitStUserFromACTORROLE
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Using DataTable insteadof Datareader
        public void InitStUserFromACTORROLE(string pCS, IDbTransaction pDbTransaction, ActorRoleCollection pTradeActorRole, StatusEnum pStatusEnum)
        {
            string sql = string.Empty;
            string colSql = StatusTools.GetColumnNameStatusUser(pStatusEnum);
            Cst.OTCml_TBL tableEnum = StatusTools.GetTableNameStatusUser(pStatusEnum);

            string sqlRoot = string.Empty;
            sqlRoot += SQLCst.SELECT + "ar." + colSql + Cst.CrLf;
            sqlRoot += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar " + Cst.CrLf;
            sqlRoot += OTCmlHelper.GetSQLJoin(pCS, tableEnum, true, "ar." + colSql, "st", true) + Cst.CrLf;

            if (null != pTradeActorRole)
            {
                int[] partyList = pTradeActorRole.GetListActor(new RoleActor[] { RoleActor.PARTY });
                string sqlPartyList = DataHelper.SQLCollectionToSqlList(pCS, partyList, TypeData.TypeDataEnum.integer);

                #region COUNTERPARTY
                int[] counterPartyList = pTradeActorRole.GetListActor(new RoleActor[] { RoleActor.COUNTERPARTY });
                SQLWhere sqlWhere;
                if (ArrFunc.IsFilled(counterPartyList))
                {
                    for (int i = 0; i < counterPartyList.Length; i++)
                    {
                        sqlWhere = new SQLWhere();
                        sqlWhere.Append("ar.IDA = " + counterPartyList[i].ToString());
                        sqlWhere.Append("ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.COUNTERPARTY.ToString()));
                        sqlWhere.Append("(ar.IDA_ACTOR in (" + sqlPartyList + ")" + SQLCst.OR + "ar.IDA_ACTOR" + SQLCst.IS_NULL + ")");
                        if ((sql.Length) > 0)
                            sql += SQLCst.UNION + Cst.CrLf;
                        sql += sqlRoot + sqlWhere.ToString() + Cst.CrLf;
                    }
                }
                #endregion COUNTERPARTY

                #region ENTITY
                int[] entityList = pTradeActorRole.GetListActor(new RoleActor[] { RoleActor.ENTITY });
                if (ArrFunc.IsFilled(entityList))
                {
                    for (int i = 0; i < entityList.Length; i++)
                    {
                        sqlWhere = new SQLWhere();
                        sqlWhere.Append("ar.IDA = " + entityList[i].ToString());
                        sqlWhere.Append("ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.ENTITY.ToString()));
                        sqlWhere.Append("(ar.IDA_ACTOR in (" + sqlPartyList + ")" + SQLCst.OR + "ar.IDA_ACTOR" + SQLCst.IS_NULL + ")");
                        if ((sql.Length) > 0)
                            sql += SQLCst.UNION + Cst.CrLf;
                        sql += sqlRoot + sqlWhere.ToString() + Cst.CrLf;
                    }
                }
                #endregion ENTITY

                #region CLIENT
                int[] clientList = pTradeActorRole.GetListActor(new RoleActor[] { RoleActor.CLIENT });
                if (ArrFunc.IsFilled(clientList))
                {
                    for (int i = 0; i < clientList.Length; i++)
                    {
                        sqlWhere = new SQLWhere();
                        sqlWhere.Append("ar.IDA = " + clientList[i].ToString());
                        sqlWhere.Append("ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.CLIENT.ToString()));
                        sqlWhere.Append("(ar.IDA_ACTOR in (" + sqlPartyList + ")" + SQLCst.OR + "ar.IDA_ACTOR" + SQLCst.IS_NULL + ")");
                        if ((sql.Length) > 0)
                            sql += SQLCst.UNION + Cst.CrLf;
                        sql += sqlRoot + sqlWhere.ToString() + Cst.CrLf;
                    }
                }
                #endregion CLIENT

                #region BROKER
                int[] brokerList = pTradeActorRole.GetListActor(new RoleActor[1] { RoleActor.BROKER });
                if (ArrFunc.IsFilled(brokerList))
                {
                    for (int i = 0; i < brokerList.Length; i++)
                    {
                        //
                        sqlWhere = new SQLWhere();
                        sqlWhere.Append("ar.IDA = " + brokerList[i].ToString());
                        sqlWhere.Append("ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.BROKER.ToString()));
                        sqlWhere.Append("(ar.IDA_ACTOR in (" + sqlPartyList + ")" + SQLCst.OR + "ar.IDA_ACTOR" + SQLCst.IS_NULL + ")");
                        if ((sql.Length) > 0)
                            sql += SQLCst.UNION + Cst.CrLf;
                        sql += sqlRoot + sqlWhere.ToString() + Cst.CrLf;
                    }
                }
                #endregion BROKER

                #region TRADER
                for (int i = 0; i < pTradeActorRole.Count; i++)
                {
                    ActorRole actorRole = (ActorRole)pTradeActorRole[i];
                    if (RoleActor.TRADER == actorRole.Role)
                    {
                        sqlWhere = new SQLWhere();
                        sqlWhere.Append("ar.IDA = " + actorRole.IdA.ToString());
                        sqlWhere.Append("ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.TRADER.ToString()));
                        sqlWhere.Append("ar.IDA_ACTOR   =" + actorRole.IdA_Actor.ToString());
                        if ((sql.Length) > 0)
                            sql += SQLCst.UNION + Cst.CrLf;
                        sql += sqlRoot + sqlWhere.ToString() + Cst.CrLf;
                    }
                }
                #endregion TRADER

                if (sql.Length > 0)
                {
                    StatusCollection statusCollection = this.GetStUsersCollection(pStatusEnum);

                    //20070717 FI utilisation de ExecuteScalar pour le Cache
                    DataTable dt = DataHelper.ExecuteDataTable(pCS, pDbTransaction, sql);
                    if (null != dt)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            try
                            {
                                statusCollection[row[colSql].ToString()].NewValue = 1;
                            }
                            catch { }
                        }
                    }
                }
            }
        }
        #endregion
        #region public InitStUserFromACTIONTUNING
        public void InitStUserFromACTIONTUNING(StatusEnum pStatusEnum, DataRow pdrActionTuning)
        {
            string stUser;
            StatusCollection statusCollection = GetStUsersCollection(pStatusEnum);

            string columName = StatusTools.GetColumnNameStatusUser(pStatusEnum);
            if (pdrActionTuning[columName] != Convert.DBNull)
            {
                stUser = pdrActionTuning[columName].ToString();
                statusCollection[stUser].NewValue = 1;
            }

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataRow"></param>
        /// <param name="pIdT">Représente le trade</param>
        /// <param name="pIdA">Représente l'utilisateur</param>
        /// <param name="pDtSys"></param>
        /// <param name="pIsNewRow"></param>
        /// FI 20120911 [18118] modification de la signature de la méthode
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        public void UpdateRowTradeStSys(DataRow pDataRow, int pIdA, DateTime pDtSys, Pair<Cst.StatusUsedBy, string> pStUsedBy)
        {
            bool isNewRow = (pDataRow.RowState == DataRowState.Added);

            pDataRow.BeginEdit();

            pDataRow["IDSTENVIRONMENT"] = stEnvironment.NewSt;
            pDataRow["IDSTBUSINESS"] = stBusiness.NewSt;
            pDataRow["IDSTPRIORITY"] = stPriority.NewSt;
            pDataRow["IDSTACTIVATION"] = stActivation.NewSt;
            pDataRow["IDSTUSEDBY"] = stUsedBy.NewSt;

            if (isNewRow || GetStatus(StatusEnum.StatusEnvironment).IsModify)
            {
                pDataRow["DTSTENVIRONMENT"] = pDtSys;
                pDataRow["IDASTENVIRONMENT"] = pIdA;
            }
            if (isNewRow || GetStatus(StatusEnum.StatusBusiness).IsModify)
            {
                pDataRow["DTSTBUSINESS"] = pDtSys;
                pDataRow["IDASTBUSINESS"] = pIdA;
            }
            if (isNewRow || GetStatus(StatusEnum.StatusPriority).IsModify)
            {
                pDataRow["DTSTPRIORITY"] = pDtSys;
                pDataRow["IDASTPRIORITY"] = pIdA;
            }
            if (isNewRow || GetStatus(StatusEnum.StatusActivation).IsModify)
            {
                pDataRow["DTSTACTIVATION"] = pDtSys;
                pDataRow["IDASTACTIVATION"] = pIdA;
            }

            if (null != pStUsedBy)
            {
                pDataRow["IDSTUSEDBY"] = pStUsedBy.First.ToString();
                pDataRow["LIBSTUSEDBY"] = StrFunc.IsFilled(pStUsedBy.Second) ? pStUsedBy.Second : Convert.DBNull;
            }
            else
            {
                pDataRow["IDSTUSEDBY"] = Cst.StatusUsedBy.REGULAR.ToString();
                pDataRow["LIBSTUSEDBY"] = Convert.DBNull;
            }
            pDataRow["DTSTUSEDBY"] = pDtSys;
            pDataRow["IDASTUSEDBY"] = pIdA;
            pDataRow.EndEdit();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA">Représente l'opérateur</param>
        /// <param name="pDtsys"></param>
        /// <returns></returns>
        /// FI 20120911 [18118]modification de la signature add parameter pIdA , drop parameter pAppInstance
        /// Dans l'imporation des trades pAppIns.IdA est alimenté avec l'acteur SYSTEM
        /// et pUser est alimenté avec l'opérateur (user dans le flux de mapping)
        public bool UpdateStatus(string pCS, IDbTransaction pDbTransaction, int pIdT, int pIdA, DateTime pDtsys)
        {
            bool isRet = UpdateTradeStSys(pCS, pDbTransaction, pIdT, pIdA, pDtsys);
            if (isRet)
                isRet = UpdateStUser(pCS, pDbTransaction, Mode.Trade, pIdT, pIdA, pDtsys);

            return isRet;
        }


        /// <summary>
        /// Update d'un trade existant
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        /// FI 20120911 [18118] modification de la signature de la méthode
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private bool UpdateTradeStSys(string pCS, IDbTransaction pDbTransaction, int pIdT, int pIdA, DateTime pDtSys)
        {
            SQL_TradeCommon sqlTradeStSys = new SQL_TradeCommon(pCS, pIdT)
            {
                DbTransaction = pDbTransaction
            };
            string[] columns = new string[] {"TRADE.IDT",
            "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT",
            "IDSTBUSINESS", "DTSTBUSINESS", "IDASTBUSINESS",
            "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION",
            "IDSTPRIORITY", "DTSTPRIORITY", "IDASTPRIORITY",
            "IDASTUSEDBY", "IDSTUSEDBY", "DTSTUSEDBY", "LIBSTUSEDBY", "ROWATTRIBUT"};

            sqlTradeStSys.LoadTable(columns);
            if (sqlTradeStSys.IsFound)
                UpdateRowTradeStSys(sqlTradeStSys.FirstRow, pIdA, pDtSys, null);
            string sqlQuery = sqlTradeStSys.GetQueryParameters(columns).QueryReplaceParameters;

            int rowsAffected = DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlQuery, sqlTradeStSys.Dt);

            bool ret = rowsAffected == 1;
            return ret;
        }

        /// <summary>
        /// Retourne la liste des statuts actifs. 
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// <returns></returns>
        /// 20140710 PL [TRIM 20179]
        /// 20140805 FI [20179] Modify  
        public string GetTickedTradeStUser(StatusEnum pStatusEnum, string pSeparator)
        {
            string ret = string.Empty;

            string startSeparator = string.Empty;
            string endSeparator = string.Empty;

            if (String.IsNullOrEmpty(pSeparator))
            {
                pSeparator = ",";
            }

            if (pSeparator.Length == 1)
            {
                //ok
            }
            else if (pSeparator.Length == 2)
            {
                //Tip: Lorsque le séparateur est constitué de 2 caractères, on utilise ceux là également en début et fin de liste (ex. "}{" donnera "{UNMATCH}{XXXXX}")
                startSeparator = pSeparator.Substring(1, 1);
                endSeparator = pSeparator.Substring(0, 1);
            }
            else
                throw new ArgumentException("Argument 'pSeparator' is not valid");


            StatusCollection stUsersCol = this.GetStUsersCollection(pStatusEnum);
            if ((null != stUsersCol) && (null != stUsersCol.Status))
            {
                var statusTicked = from item in stUsersCol.Status
                                   where item.NewValue == 1
                                   select item.NewSt;
                if (statusTicked.Count() > 0)
                {
                    foreach (string status in statusTicked)
                    {
                        if (pSeparator.Length == 2)
                        {
                            ret += StrFunc.AppendFormat("{0}{1}{2}", startSeparator, status, endSeparator);
                        }
                        else if (pSeparator.Length == 1)
                        {
                            if (StrFunc.IsFilled(ret))
                                ret += pSeparator;
                            ret += status;
                        }
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20130911 Add pDbTransaction parameter
        public void Initialize(string pCS, int pIdT)
        {
            Initialize(pCS, null, pIdT);
        }
        /// <summary>
        /// Alimentation des statuts à partir des tables SQL (TRADE (EX ...STSYS), TRADESTMATCH, TRADESTCHECK)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20140728 [20255] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void Initialize(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            sqlTradeStSysSource = new SQL_TradeStSys(pCS, pIdT)
            {
                DbTransaction = pDbTransaction,
                IsAddRowVersion = true
            };

            sqlTradeStSysSource.LoadTable(new string[]{"IDT",
                "IDSTENVIRONMENT","DTSTENVIRONMENT","IDASTENVIRONMENT",
                "IDSTBUSINESS","DTSTBUSINESS","IDASTBUSINESS",
                "IDSTACTIVATION","DTSTACTIVATION","IDASTACTIVATION",
                "IDSTPRIORITY","DTSTPRIORITY","IDASTPRIORITY",
                "IDSTUSEDBY","DTSTUSEDBY","IDASTUSEDBY","LIBSTUSEDBY"});
            InitializeStatusSys();
            InitializeStUsers(pCS, pDbTransaction, pIdT);

            InitializeEnums(pCS);

            ResetNewValueFromCurrent();
        }
        /// <summary>
        /// Alimentation des statuts users à partir des tables SQL (TRADESTMATCH, TRADESTCHECK)
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        public void InitializeStUsers(string pCS, int pIdT)
        {
            InitializeStUsers(pCS, null, pIdT);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public void InitializeStUsers(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            for (int i = 0; i < stUsers.Length; i++)
                stUsers[i].InitializeFromTrade(pCS, pDbTransaction, pIdT);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool InitializeStatusSys()
        {
            //	
            bool isOk = (null != sqlTradeStSysSource) && (sqlTradeStSysSource.IsFound);
            //
            if (isOk)
            {
                //20100312 PL-StatusBusiness
                stEnvironment.CurrentSt = sqlTradeStSysSource.IdStEnvironment;
                stBusiness.CurrentSt = sqlTradeStSysSource.IdStBusiness;
                stPriority.CurrentSt = sqlTradeStSysSource.IdStPriority;
                stActivation.CurrentSt = sqlTradeStSysSource.IdStActivation;
                stUsedBy.CurrentSt = sqlTradeStSysSource.IdStUsedBy;
                //
                stEnvironment.CurrentValue = 1;
                stBusiness.CurrentValue = 1;
                stPriority.CurrentValue = 1;
                stActivation.CurrentValue = 1;
                stUsedBy.CurrentValue = 1;
                //
                stEnvironment.CurrentDtSys = sqlTradeStSysSource.DtStEnvironment;
                stBusiness.CurrentDtSys = sqlTradeStSysSource.DtStBusiness;
                stPriority.CurrentDtSys = sqlTradeStSysSource.DtStPriority;
                stActivation.CurrentDtSys = sqlTradeStSysSource.DtStActivation;
                stUsedBy.CurrentDtSys = sqlTradeStSysSource.DtStUsedBy;
                //		
                stEnvironment.CurrentIdA = sqlTradeStSysSource.IdAStEnvironment;
                stBusiness.CurrentIdA = sqlTradeStSysSource.IdAStBusiness;
                stPriority.CurrentIdA = sqlTradeStSysSource.IdAStPriority;
                stActivation.CurrentIdA = sqlTradeStSysSource.IdAStActivation;
                stUsedBy.CurrentIdA = sqlTradeStSysSource.IdAStUsedBy;
                stUsedBy.CurrentLibSt = sqlTradeStSysSource.LibStUsedBy;
            }
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        protected override void InitializeEnums(string pCS)
        {
            base.InitializeEnums(pCS);
            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnumHelper
            ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
            stEnvironment.ExtendEnum = ListEnumsSchemes[StatusEnum.StatusEnvironment.ToString()];
            stBusiness.ExtendEnum = ListEnumsSchemes[StatusEnum.StatusBusiness.ToString()];
            stPriority.ExtendEnum = ListEnumsSchemes[StatusEnum.StatusPriority.ToString()];
            */
            ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, StatusEnum.StatusEnvironment.ToString());
            stEnvironment.ExtendEnum = extendEnum ?? throw new KeyNotFoundException($"Key:{StatusEnum.StatusEnvironment} not found.");
            extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, StatusEnum.StatusBusiness.ToString());
            stBusiness.ExtendEnum = extendEnum ?? throw new KeyNotFoundException($"Key:{StatusEnum.StatusBusiness} not found.");
            extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, StatusEnum.StatusPriority.ToString());
            stPriority.ExtendEnum = extendEnum ?? throw new KeyNotFoundException($"Key:{StatusEnum.StatusPriority} not found.");
        }

        #region Membres de ICloneable
        public object Clone()
        {
            TradeStatus clone = new TradeStatus
            {
                stEnvironment = (Status)stEnvironment.Clone(),
                stBusiness = (Status)stBusiness.Clone(),
                stActivation = (Status)stActivation.Clone(),
                stPriority = (Status)stPriority.Clone()
            };

            if (ArrFunc.IsFilled(stUsers))
            {
                int count = ArrFunc.Count(stUsers);
                clone.stUsers = new StatusCollection[count];
                for (int i = 0; i < count; i++)
                    clone.stUsers[i] = (StatusCollection)stUsers[i].Clone();
            }
            return clone;
        }
        #endregion Membres de ICloneable
    }

    /// <summary>
    /// EventStatus
    /// </summary>
    public class EventStatus : CommonStatus, ICloneable
    {
        #region Members
        #endregion  Members

        #region constructor
        public EventStatus() : base() { }
        #endregion constructor

        #region public Initialize
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public void Initialize(string pCS, int pIdE)
        {
            Initialize(pCS, null, pIdE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pdbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public void Initialize(string pCS, IDbTransaction pdbTransaction, int pIdE)
        {
            SQL_Event sqlEvent = new SQL_Event(pCS, pIdE);
            if (null != pdbTransaction)
                sqlEvent.DbTransaction = pdbTransaction;

            sqlEvent.LoadTable(new string[] { "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION" });

            InitializeStatusSys(sqlEvent);

            for (int i = 0; i < stUsers.Length; i++)
                stUsers[i].InitializeFromEvent(pIdE, pCS, pdbTransaction);

            InitializeEnums(pCS);

        }
        #endregion

        #region private initializeStatusSys
        private void InitializeStatusSys(SQL_Event pSqlEvent)
        {
            if (pSqlEvent.IsFound)
            {
                stActivation.CurrentSt = pSqlEvent.IdStActivation;
                stActivation.CurrentDtSys = pSqlEvent.DtStActivation;
                stActivation.CurrentIdA = pSqlEvent.IdAStActivation;
            }
            else
            {
                stActivation.CurrentValue = 1;
                stActivation.CurrentSt = Cst.STATUSREGULAR;
                stActivation.CurrentDtSys = DateTime.MinValue;
                stActivation.CurrentIdA = 0;
                stActivation.CurrentDtEffect = null;
                stActivation.CurrentNote = null;
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        protected override void InitializeEnums(string pCS)
        {
            base.InitializeEnums(pCS);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataRow"></param>
        /// <param name="pIdE"></param>
        /// <param name="pIdA">Représente l'utilisateur</param>
        /// <param name="pDtSys"></param>
        /// <param name="pIsNewRow"></param>
        /// FI 20161124 [22634] Add
        private void UpdateRowEvent(DataRow pDataRow, int pIdE, int pIdA, DateTime pDtSys, bool pIsNewRow)
        {
            DataRow dr = pDataRow;

            pDataRow.BeginEdit();

            if (pIsNewRow)
                dr["IDE"] = pIdE;

            dr["IDSTACTIVATION"] = stActivation.NewSt;

            if (pIsNewRow || GetStatus(StatusEnum.StatusActivation).IsModify)
            {
                dr["DTSTACTIVATION"] = pDtSys;
                dr["IDASTACTIVATION"] = pIdA;
            }

            dr.EndEdit();
        }



        /// <summary>
        /// Mise à jour des tables EVENT, EVENTSTCHECK et EVENTSTMATCH 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA">Représente l'opérateur</param>
        /// <param name="pDtsys"></param>
        /// <returns></returns>
        /// FI 20161124 [22634] Add
        public bool UpdateStatus(string pCS, IDbTransaction pDbTransaction, int pIdE, int pIdA, DateTime pDtsys)
        {
            bool isRet = UpdateEvent(pCS, pDbTransaction, pIdE, pIdA, pDtsys);

            if (isRet)
                isRet = UpdateStUser(pCS, pDbTransaction, Mode.Event, pIdE, pIdA, pDtsys);

            return isRet;
        }


        /// <summary>
        /// Mise à jour de la table EVENT uniquement
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        /// FI 20161124 [22634] Add
        // EG 20180205 [23769] Upd DataHelper.ExecutedataAdapter
        public bool UpdateEvent(string pCS, IDbTransaction pDbTransaction, int pIdE, int pIdA, DateTime pDtSys)
        {

            SQL_Event sqlEvent = new SQL_Event(pCS, pIdE)
            {
                DbTransaction = pDbTransaction
            };

            string[] columns = new string[]
                                {   "IDE",
                        "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION",
                        "ROWATTRIBUT"};

            sqlEvent.LoadTable(columns);

            if (sqlEvent.IsFound)
                UpdateRowEvent(sqlEvent.FirstRow, pIdE, pIdA, pDtSys, false);

            string sqlQuery = sqlEvent.GetQueryParameters(columns).QueryReplaceParameters;

            int rowsAffected = DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlQuery, sqlEvent.Dt);

            Boolean ret = (rowsAffected == 1);
            return ret;
        }



        #region Membres de ICloneable
        public object Clone()
        {
            EventStatus clone = new EventStatus
            {
                stActivation = (Status)stActivation.Clone(),
                stUsers = (StatusCollection[])stUsers.Clone()
            };
            return clone;
        }
        #endregion Membres de ICloneable

    }

    /// <summary>
    /// StatusCollection
    /// </summary>
    public class StatusCollection : ICloneable
    {
        #region Members
        /// <summary>
        /// Liste des statuts ordonnés du poids le moins élevé au poids le plus élevé
        /// </summary>
        private Status[] _status;
        /// <summary>
        /// Type de statut  (StatusCheck ou StatusMatch)
        /// </summary>
        private readonly StatusEnum _statusEnum;
        #endregion Members

        #region constructor
        public StatusCollection(StatusEnum pStatusEnum)
        {
            _statusEnum = pStatusEnum;
        }
        #endregion constructor

        #region indexor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNewSt"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify
        public Status this[string pNewSt]
        {
            get
            {
                Status ret = null;
                if (ArrFunc.IsFilled(_status))
                {
                    for (int i = 0; i < _status.Length; i++)
                    {
                        if (_status[i].NewSt == pNewSt)
                        {
                            ret = _status[i];
                            break; // FI 20140708 [20179] add break
                        }
                    }
                }
                return ret;
            }
        }
        public Status this[int i]
        {
            get
            {
                Status ret = null;
                if (ArrFunc.IsFilled(_status))
                    ret = _status[i];
                return ret;
            }
        }
        #endregion indexor
        #region property
        public StatusEnum StatusEnum
        {
            get
            {
                return _statusEnum;
            }
        }
        public Status[] Status
        {
            get
            {
                return _status;
            }
        }
        #endregion

        /// <summary>
        /// Alimentation de la collection à partir de la table EVENTSTCHECK ou EVENTSTMATH
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        /// FI 20140728 [20255] Modify 
        /// FI 20161124 [22634] Modify
        public void InitializeFromEvent(int pIdE, string pCS, IDbTransaction pDbTransaction)
        {

            SQL_EventStUser sqlEventStUser = new SQL_EventStUser(pCS, pIdE, _statusEnum)
            {
                DbTransaction = pDbTransaction
            };

            // FI 20161124 [22634] add column DTEFFECT
            string columnId = StatusTools.GetColumnNameStatusUser(_statusEnum); // FI 20140728 [20255] add columnId 
            sqlEventStUser.LoadTable(new string[] { "IDE", columnId, "DTEFFECT", "LONOTE", "DTINS", "IDAINS" });

            _status = InitializeFromTable(sqlEventStUser);
        }

        /// <summary>
        /// Alimentation de la collection à partir de la table TRADESTCHECK ou TRADESTMATH
        /// </summary>
        /// FI 20140728 [20255] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public void InitializeFromTrade(string pCS, int pIdT)
        {
            InitializeFromTrade(pCS, null, pIdT);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public void InitializeFromTrade(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            // FI 20140728 [20255] add DTEFFECT et LONOTE
            string columnId = StatusTools.GetColumnNameStatusUser(_statusEnum);
            SQL_TradeStUser sqlTradeStUser = new SQL_TradeStUser(pCS, pIdT, _statusEnum)
            {
                DbTransaction = pDbTransaction
            };
            sqlTradeStUser.LoadTable(new string[] { "IDT", columnId, "DTEFFECT", "LONOTE", "DTINS", "IDAINS" });
            _status = InitializeFromTable(sqlTradeStUser);
        }

        /// <summary>
        /// NewValue = CurrentValue pour chaque élément de la collection
        /// </summary>
        public void ResetNewValueFromCurrent()
        {
            if (ArrFunc.IsFilled(_status))
            {
                for (int i = 0; i < _status.Length; i++)
                    _status[i].ResetNewValueFromCurrent();
            }
        }

        /// <summary>
        /// NewValue = 0 pour chaque élément de la collection 
        /// </summary>
        public void ResetNewValue()
        {
            if (ArrFunc.IsFilled(_status))
            {
                for (int i = 0; i < _status.Length; i++)
                    _status[i].ResetNewValue();
            }
        }

        /// <summary>
        /// Retourne succès si les poids des status repectent le poids (min ou max attendu) 
        /// </summary>
        /// <param name="pMinMax">Type de contrôle (Min ou max)</param>
        /// <param name="pValue">la valeur de référence pour le contôle</param>
        /// <param name="pCtrlSt"></param>
        /// <returns></returns>
        public ErrorLevelTuningEnum ScanCompatibility(MinMaxTuningEnum pMinMax, int pValue, Cst.CtrlStatusEnum pCtrlSt)
        {
            ErrorLevelTuningEnum errLevel = ErrorLevelTuningEnum.SUCCESS;

            //Lorsqu'il faut un poids minimum s'il n'existe pas de status alors Pas compatible
            bool isOk = MinMaxTuningEnum.MIN != pMinMax;

            bool isExist = false;
            if ((null != _status) && _status.Where(x => x.CurrentValue == 1).Count() > 0)
            {
                Status[] status = _status.Where(x => x.CurrentValue == 1).ToArray();
                for (int i = 0; i < status.Count(); i++)
                {
                    bool isOkItem = false;
                    //
                    switch (pMinMax)
                    {
                        case MinMaxTuningEnum.MIN:
                            if (!isExist)
                            {
                                //PL 20161110 RATP A REVOIR
                                isExist = true;
                                isOk = true;
                            }

                            //Si status coché --> ctrl de son poids par rapport au poids min attendu
                            isOkItem = (status[i].CurrentWeight >= pValue);
                            break;
                        case MinMaxTuningEnum.MAX:
                            //Si status coché --> ctrl de son poids par rapport au poids max attendu
                            isOkItem = (status[i].CurrentWeight <= pValue);
                            break;
                    }
                    //
                    if (isOkItem && (pCtrlSt == Cst.CtrlStatusEnum.ATLEASTONE))
                    {
                        isOk = true;
                        break;
                    }
                    else
                    {
                        isOk &= isOkItem;
                        //20090904 PL Bug
                        //if (false == isOk)
                        if ((!isOk) && (pCtrlSt == Cst.CtrlStatusEnum.EVERY))
                            break;
                    }
                }
            }

            if (!isOk)
            {
                switch (StatusEnum)
                {
                    case StatusEnum.StatusCheck:
                        errLevel = ((pMinMax == MinMaxTuningEnum.MIN) ? ErrorLevelTuningEnum.ERR_MINCHECK : ErrorLevelTuningEnum.ERR_MAXCHECK);
                        break;
                    case StatusEnum.StatusMatch:
                        errLevel = ((pMinMax == MinMaxTuningEnum.MIN) ? ErrorLevelTuningEnum.ERR_MINMATCH : ErrorLevelTuningEnum.ERR_MAXMATCH);
                        break;
                    default:
                        errLevel = ErrorLevelTuningEnum.ERROR;
                        break;
                }
            }

            return errLevel;

        }

        /// <summary>
        /// Alimentation de la collection en fonction des enregistrements présents dans {pSqlStStatus}
        /// <para>Les statuts sont ordonnés du poids le moins élevé au poids le plus élevé</para>
        /// </summary>
        /// <param name="pSqlStStatus"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private static Status[] InitializeFromTable(SQL_StUser pSqlStStatus)
        {
            string cs = CSTools.SetCacheOn(pSqlStStatus.CS);

            StWeightCollection stWeights;
            switch (pSqlStStatus.StatusEnum)
            {
                case StatusEnum.StatusMatch:
                    stWeights = new StMatchWeightCollection().LoadWeight(cs, pSqlStStatus.DbTransaction);
                    break;
                case StatusEnum.StatusCheck:
                    stWeights = new StCheckWeightCollection().LoadWeight(cs, pSqlStStatus.DbTransaction);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSqlStStatus.StatusEnum.ToString()));
            }

            ArrayList lst = new ArrayList();
            for (int i = 0; i < stWeights.StWeight.Length; i++)
            {
                Status statusItem = new Status(pSqlStStatus.StatusEnum)
                {
                    CurrentSt = stWeights.StWeight[i].IdSt,
                    CurrentWeight = stWeights.StWeight[i].Weight
                };

                DataRow foundRow = null;
                if ((null != pSqlStStatus) && (pSqlStStatus.IsLoaded))
                {
                    object[] findTheseVals = new object[2];
                    findTheseVals[0] = pSqlStStatus.IdParent;
                    findTheseVals[1] = stWeights.StWeight[i].IdSt;
                    foundRow = pSqlStStatus.Dt.Rows.Find(findTheseVals);
                }

                if (null != foundRow)
                {
                    statusItem.CurrentValue = 1;
                    // 20200820 [XXXXX] Dates systèmes en UTC
                    statusItem.CurrentDtSys = DateTime.SpecifyKind(Convert.ToDateTime(foundRow["DTINS"]), DateTimeKind.Utc);
                    statusItem.CurrentIdA = Convert.ToInt32(foundRow["IDAINS"]);
                    statusItem.CurrentDtEffect = null;
                    if (foundRow["DTEFFECT"] != Convert.DBNull)
                        statusItem.CurrentDtEffect = Convert.ToDateTime(foundRow["DTEFFECT"]);
                    if (foundRow["LONOTE"] != Convert.DBNull)
                        statusItem.CurrentNote = Convert.ToString(foundRow["LONOTE"]);
                }
                else
                {
                    statusItem.CurrentValue = 0;
                    statusItem.CurrentDtSys = DateTime.MinValue;
                    statusItem.CurrentIdA = 0;
                    statusItem.CurrentDtEffect = null;
                    statusItem.CurrentNote = null;
                }
                lst.Add(statusItem);
            }
            return (Status[])lst.ToArray(typeof(Status));
        }

        #region Membres de ICloneable
        public object Clone()
        {
            StatusCollection clone = new StatusCollection(_statusEnum);
            //
            if (ArrFunc.IsFilled(_status))
            {
                int count = ArrFunc.Count(_status);
                clone._status = new Status[count];
                for (int i = 0; i < count; i++)
                    clone._status[i] = (Status)_status[i].Clone();
            }
            return clone;
        }
        #endregion Membres de ICloneable
    }
}
