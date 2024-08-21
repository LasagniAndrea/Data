#region using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Process;
using EFS.SpheresService;
using EfsML.Enum;

#endregion using directives

namespace EFS.Common.MQueue
{
    #region MQueueAttributes
    public class MQueueAttributes
    {
        #region Members
        public string connectionString;
        public int id;
        public string identifier;
        public DateTime date;
        public DateTime timestamp;
        public SQL_Criteria criteria;
        public IdInfo idInfo;
        public MQueueparameters parameters;
        public MQueueRequester requester;
        #endregion Members

        #region Constructors
        public MQueueAttributes() { }
        #endregion Constructors
        #region Methods

        /// <summary>
        /// Ajoute un paramètre date
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pDate"></param>
        public void AddParameter(string pId, DateTime pDate)
        {
            AddParameter(pId, TypeData.TypeDataEnum.date, pDate);
        }
        /// <summary>
        /// Ajoute un paramètre Bool
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pbool"></param>
        public void AddParameter(string pId, Boolean pbool)
        {
            AddParameter(pId, TypeData.TypeDataEnum.@bool, pbool);
        }
        /// <summary>
        /// Ajoute un paramètre String
        /// </summary>
        /// <param name="pId">Identifiant du paramètre</param>
        /// <param name="pString"></param>
        public void AddParameter(string pId, string pString)
        {
            AddParameter(pId, TypeData.TypeDataEnum.@string, pString);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pInt"></param>
        public void AddParameter(string pId, int pInt)
        {
            AddParameter(pId, TypeData.TypeDataEnum.@string, pInt);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pDataType"></param>
        /// <param name="pValue"></param>
        public void AddParameter(string pId, TypeData.TypeDataEnum pDataType, object pValue)
        {
            if (null == parameters)
                parameters = new MQueueparameters();
            ReflectionTools.AddItemInArray(parameters, "parameter", 0);
            int index = parameters.Count - 1;
            parameters[index].id = pId;
            parameters[index].dataType = pDataType;
            parameters[index].SetValue(pValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void AddParameter(MQueueparameter parameter)
        {
            if (null == parameters)
                parameters = new MQueueparameters();
            ReflectionTools.AddItemInArray(parameters, "parameter", 0);
            int index = parameters.Count - 1;
            parameters[index] = parameter;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcess"></param>
        /// <param name="pQueueAttributes"></param>
        /// <returns></returns>
        public void AdditionalParameters(Cst.ProcessTypeEnum pProcess, Nullable<Cst.ProcessIOType> pProcessIoType)
        {
            MQueueparameter parameter;
            switch (pProcess)
            {
                case Cst.ProcessTypeEnum.CMGEN:
                case Cst.ProcessTypeEnum.RIMGEN:
                    if (pProcessIoType.HasValue)
                    {
                        parameter = new MQueueparameter(MQueueBase.PARAM_ISWITHIO, TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(pProcessIoType.HasValue);
                        parameters.Add(parameter);
                    }
                    break;
                case Cst.ProcessTypeEnum.RMGEN:
                    parameter = new MQueueparameter(MQueueBase.PARAM_ISWITHIO, TypeData.TypeDataEnum.@bool);
                    parameter.SetValue(pProcessIoType.HasValue);
                    parameters.Add(parameter);
                    parameter = new MQueueparameter(ReportMsgGenMQueue.PARAM_ISMODEREGENERATE, TypeData.TypeDataEnum.@bool);
                    parameter.SetValue(!pProcessIoType.HasValue || (pProcessIoType.Value == Cst.ProcessIOType.ProcessAndIO));
                    parameters.Add(parameter);
                    break;
                default:
                    break;
            }
        }

        #endregion Methods
    }
    #endregion MQueueAttributes

    #region MQueueBase
    [Serializable]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AccountGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AccrualsGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CashBalanceInterestMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CollateralValMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationInstrGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationMsgGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EarGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EventsGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EventsValMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ESRNetGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ESRStandardGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GateBCSMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoicingGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoicingAllocationGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoicingCorrectionGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoicingCancelationSimulationGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvoicingValidationSimulationGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IOMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LinearDepGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MarkToMarketGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MSOGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NormMsgFactoryMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosKeepingEntryMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosKeepingRequestMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuotationHandlingMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseRequestMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReportInstrMsgGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReportMsgGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RiskPerformanceMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SettlementInstrGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ShellMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeActionGenMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackMqueue))]
    /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
    public class MQueueBase 
    {
        #region Constants
        public const int POSITION_PROCESSTYPE = 0;
        public const int POSITION_DATE = 1;
        public const int POSITION_STATUS_ENVIRONMENT = 2;
        public const int POSITION_STATUS_ACTIVATION = 3;
        public const int POSITION_STATUS_PRIORITY = 4;
        public const int POSITION_IDENTIFIERS1 = 5;
        public const int POSITION_IDENTIFIERS2 = 6;
        public const int POSITION_IDENTIFIERS3 = 7;

        public const string PARAM_ISSIMUL = "ISSIMUL";
        public const string PARAM_ISKEEPHISTORY = "ISKEEPHISTORY";
        public const string PARAM_DATE1 = "DATE1";
        public const string PARAM_DATE2 = "DATE2";
        public const string PARAM_ENTITY = "ENTITY";
        //public const string PARAM_ENTITY_IDENTIFIER = "ENTITY_IDENTIFIER"; // FI 20120903 [17773]
        public const string PARAM_IDT = "IDT";

        public const string PARAM_ISOBSERVER = "ISOBSERVER";
        public const string PARAM_ISCREATEDBY_NORMMSGFACTORY = "ISCREATEDBY_NORMMSGFACTORY";
        public const string PARAM_ISWITHIO = "ISWITHIO";
        //
        protected const string separator = "_";
        #endregion Constants

        #region Members
        [System.Xml.Serialization.XmlElementAttribute("header")]
        public MQueueHeader header;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("action")]
        public string action;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified;
        [System.Xml.Serialization.XmlElementAttribute("id")]
        public int id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idServiceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idService")]
        public string idService;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dateSpecified;
        //Use PARAM_DATE1 instead of date (date is keep only for compatibility)
        [System.Xml.Serialization.XmlElementAttribute("date")]
        public DateTime date;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dataSpecified;
        [System.Xml.Serialization.XmlElementAttribute("data")]
        public System.Xml.XmlCDataSection data;
        // IdInfo
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idInfoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idInfo")]
        public IdInfo idInfo;
        // Parameters
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool parametersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parameters")]
        public MQueueparameters parameters;
        //Version
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;

        //serviceName (PL 20180720 Newness)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string serviceName;
        #endregion
        #region Accessors

        /// <summary>
        /// Obtient l'Identifier associé au message
        /// <para>Généralement l'dentifier d'un trade</para>
        /// <para>- soit le membre identifier lorsque renseigné</para>
        /// <para>- soit l'information identifier potentiellement présent sous idInfo</para>
        /// <para>Obtient N/A s'il existe pas d'identifier</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Identifier
        {
            get
            {
                string ret = Cst.NotAvailable;
                if (identifierSpecified)
                    ret = identifier;
                else if (StrFunc.IsFilled(GetStringValueIdInfoByKey("identifier")))
                    ret = GetStringValueIdInfoByKey("identifier");
                return ret;
            }
        }

        /// <summary>
        /// Représente la ConnectionString
        /// <para>Membre privé à usage technique</para> 
        /// </summary>
        /// FI 20200120 [XXXXX] Add
        private string _ConnectionString;
        /// <summary>
        /// Obtient la ConnectionString
        /// </summary>
        /// FI 20200120 [XXXXX] Propriété de type Get uniquement et optimisation via l'usage de BuildConnectionString
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ConnectionString
        {
            get
            {
                if (StrFunc.IsEmpty(_ConnectionString))
                    _ConnectionString = BuildConnectionString();
                return _ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ConnectionStringWithoutPassword
        {
            get
            {
                CSManager csManager = new CSManager(ConnectionString);
                return csManager.GetCSWithoutPwd();
            }
        }

        /// <summary>
        ///  Obtient le process associé au message
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Cst.ProcessTypeEnum ProcessType
        {
            get
            {
                return Cst.ProcessTypeEnum.NA;
            }
            set
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string LibServiceObserver
        {
            get
            {
                if (GetBoolValueParameterById(PARAM_ISOBSERVER))
                    return " - " + Ressource.GetString("SpheresServicesObserver");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string LibProcessType
        {
            get
            {
                return Cst.ProcessTypeEnum.NA.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual bool IsIdT
        {
            get
            {
                // Par défaut on suppose que c'est un IDT
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsMasterDateSpecified
        {
            get
            {
                return DtFunc.IsDateTimeFilled(GetMasterDate());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMessageObserver
        {
            get
            {
                return GetBoolValueParameterById(MQueueBase.PARAM_ISOBSERVER);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20201013 [XXXXX] Add
        public bool IsMessageCreatedByNormMsgFactory
        {
            get
            {
                return GetBoolValueParameterById(MQueueBase.PARAM_ISCREATEDBY_NORMMSGFACTORY);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAdmin
        {
            get { return IsTradeTypeProduct(Cst.ProductGProduct_ADM); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAsset
        {
            get { return IsTradeTypeProduct(Cst.ProductGProduct_ASSET); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeRisk
        {
            get { return IsTradeTypeProduct(Cst.ProductGProduct_RISK); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdInfo"></param>
        /// <returns></returns>
        protected static bool GProductEntry(DictionaryEntry pIdInfo)
        {
            return ("GPRODUCT" == pIdInfo.Key.ToString().ToUpper());
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20150515 TradeCommonAction devient TradeActionBaseMQueue
        public virtual TradeActionBaseMQueue[] TradeActionItem
        {
            get { return null; }
        }

        #endregion Accessors
        #region Constructors
        public MQueueBase()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;
            header = new MQueueHeader();
        }
        public MQueueBase(MQueueAttributes pMQueueAttributes)
        {
            // Version EfsML
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;
            Set(pMQueueAttributes);

        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueueAttributes"></param>
        // EG 20121206 [XXXXX] Modify
        // RD 20130704 [18639] Modify
        // RD 20151016 [21337] Modify
        public void Set(MQueueAttributes pMQueueAttributes)
        {
            // Header
            // RD 20130704 [18639] Surcharger avec le type du message MQueue maitre (IOMQueue, ...)
            header = new MQueueHeader(pMQueueAttributes, this.GetType());
            // Id
            idSpecified = (0 != pMQueueAttributes.id);
            id = pMQueueAttributes.id;
            // Id
            identifierSpecified = StrFunc.IsFilled(pMQueueAttributes.identifier);
            identifier = pMQueueAttributes.identifier;
            // Date (Use PARAM_DATE1 instead of date (date is keep only for compatibility)
            dateSpecified = DtFunc.IsDateTimeFilled(pMQueueAttributes.date);
            date = pMQueueAttributes.date;
            // Parameters
            parametersSpecified = (null != pMQueueAttributes.parameters) && ArrFunc.IsFilled(pMQueueAttributes.parameters.parameter);
            //parameters = pMQueueAttributes.parameters;
            // EG 20121206
            if (parametersSpecified)
                parameters = pMQueueAttributes.parameters.Clone();
            // IdInfo
            idInfoSpecified = (null != pMQueueAttributes.idInfo) && ArrFunc.IsFilled(pMQueueAttributes.idInfo.idInfos);
            idInfo = pMQueueAttributes.idInfo;

            // RD 20151016 [21337] Code qui provient du constructeur "MQueueBase(MQueueAttributes pMQueueAttributes)" ci-dessus.
            SpecificParameters();
            SpecificIdInfos();
            // Action
            if (IsMessageObserver)
            {
                actionSpecified = true;
                action = "OBSERVER";
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGProduct"></param>
        /// <returns></returns>
        /// FI 20160524 [XXXXX] Modify
        /// FI 20160818 [XXXXX] Modify (Reparation grosse boulette introduite le 20160524)
        private bool IsTradeTypeProduct(string pGProduct)
        {
            bool ret = false;

            // FI 20160524 [XXXXX] Use GProduct() method
            string gProduct = GProduct();
            if (StrFunc.IsFilled(gProduct))
                ret = (pGProduct.ToUpper() == gProduct.ToUpper());

            return ret;
        }

        /// <summary>
        /// Retourne GPRODUCT
        /// </summary>
        /// <returns></returns>
        /// FI 20160524 [XXXXX] Add Method
        public String GProduct()
        {
            String ret = null;
            if (idInfoSpecified && ArrFunc.IsFilled(idInfo.idInfos))
            {
                DictionaryEntry item = Array.Find(idInfo.idInfos, GProductEntry);
                if (null != item.Value)
                    ret = item.Value.ToString();
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual void SpecificIdInfos()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void SpecificParameters()
        {
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune DateTime.MinValue si le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public DateTime GetDateTimeValueParameterById(string pParamId)
        {
            DateTime ret = DateTime.MinValue;
            if (parametersSpecified)
                ret = parameters.GetDateTimeValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune false si le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public Boolean GetBoolValueParameterById(string pParamId)
        {
            Boolean ret = false;
            if (parametersSpecified)
                ret = parameters.GetBoolValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune 0 sir le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public int GetIntValueParameterById(string pParamId)
        {
            int ret = 0;
            if (parametersSpecified)
                ret = parameters.GetIntValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public string GetStringValueParameterById(string pParamId)
        {
            string ret = null;
            if (parametersSpecified)
                ret = parameters.GetStringValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retourne uniquement des types de bas niveaux (Integer,Bool,Datetime,String, etc...)</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public object GetObjectValueParameterById(string pParamId)
        {
            object ret = null;
            if (parametersSpecified)
                ret = parameters.GetObjectValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne la valeur étendue du paramètre {pParamId}
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public string GetExtendValueParameterById(string pParamId)
        {
            string ret = null;
            if (parametersSpecified)
                ret = parameters.GetExtendValueParameterById(pParamId);
            return ret;
        }
        /// <summary>
        /// Retourne l'lément du dictionnaire dont la clef vaut {pKey}
        /// <para>Retourne new DictionaryEntry si {pKey} n'existe pas dans le dictionnaire</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public DictionaryEntry GetIdInfoEntry(string pKey)
        {
            DictionaryEntry ret = new DictionaryEntry();
            if (idInfoSpecified && ArrFunc.IsFilled(idInfo.idInfos))
            {
                foreach (DictionaryEntry item in idInfo.idInfos)
                {
                    if (item.Key.ToString().ToUpper() == pKey.ToUpper())
                    {
                        ret = item;
                        break;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public DateTime GetDateTimeValueIdInfoByKey(string pKey)
        {
            DateTime ret = DateTime.MinValue;
            DictionaryEntry info = GetIdInfoEntry(pKey);
            if (null != info.Value)
                ret = Convert.ToDateTime(new DtFunc().StringToDateTime(info.Value.ToString(), DtFunc.FmtISODateTime));
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public Boolean GetBoolValueIdInfoByKey(string pKey)
        {
            Boolean ret = false;
            DictionaryEntry info = GetIdInfoEntry(pKey);
            if (null != info.Value)
                ret = Convert.ToBoolean(BoolFunc.IsTrue(info.Value));
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public int GetIntValueIdInfoByKey(string pKey)
        {
            int ret = 0;
            DictionaryEntry info = GetIdInfoEntry(pKey);
            if (null != info.Value)
                ret = Convert.ToInt32(info.Value);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public virtual string GetStringValueIdInfoByKey(string pKey)
        {
            string ret = null;
            DictionaryEntry info = GetIdInfoEntry(pKey);
            if (null != info.Value)
                ret = info.Value.ToString();
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public T GetEnumValueIdInfoByKey<T>(string pKey)
        {
            T ret = default;
            if (typeof(T).IsEnum)
            {
                DictionaryEntry info = GetIdInfoEntry(pKey);
                if ((null != info.Value) && Enum.IsDefined(typeof(T), info.Value))
                    ret = (T)Enum.Parse(typeof(T), info.Value.ToString(), true);
            }
            return ret;
        }

        /// <summary>
        /// Retourne ld'identifier du message
        /// </summary>
        /// <param name="pdbTransaction">Doit être renseigné Lorsque l'Id est un IdT et que le trade a été crée dans une transaction</param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADEXML Column in TRADEXML table)
        protected virtual string RetIdentifiersForMessageQueueName()
        {
            string nameIdentifiers = string.Empty;
            const string filler = "0";

            if (idSpecified && IsIdT)
            {
                // RD 20120809 [18070] Optimisation de la compta
                // RD 20121227 [18328] - Fuite de mémoire sur IIS 
                // Inutile de mettre CacheOn, car c'est un nouveau Id à chaque fois
                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(ConnectionString, id);
                if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "IDI" }))
                {
                    SQL_Instr sqlInstr = new SQL_Instr(CSTools.SetCacheOn(ConnectionString), sqlTrade.IdI);
                    if (sqlInstr.LoadTable(new string[] { "IDI", "IDP" }))
                    {
                        nameIdentifiers += separator + sqlTrade.IdT;
                        nameIdentifiers += separator + sqlInstr.IdP;
                        nameIdentifiers += separator + sqlInstr.IdI;
                    }
                    identifierSpecified = true;
                    identifier = sqlTrade.Identifier;
                }
            }
            //20081029 Ticket 16368, lorsqu'il existe un Id il rentre dans la composition du nom de message  
            else if (idSpecified)
            {
                nameIdentifiers += separator + id;  //id Autre 

                for (int i = 0; i < 2; i++)
                    nameIdentifiers += separator + filler;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                    nameIdentifiers += separator + filler;
            }

            return nameIdentifiers;
        }
        /// <summary>
        /// Retourne les statuts chargés d'alimenter le nom du message
        /// </summary>
        /// <returns></returns>
        protected virtual string RetStatusForMessageQueueName()
        {
            string nameStatus = string.Empty;
            char filler = Convert.ToChar("X");

            if (idSpecified && IsIdT)
            {
                SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(ConnectionString, id);
                // RD 20120809 [18070] Optimisation de la compta
                if (sqlTradeStSys.LoadTable(new string[] { "IDSTENVIRONMENT", "IDSTACTIVATION", "IDSTPRIORITY" }))
                {
                    nameStatus += separator + sqlTradeStSys.IdStEnvironment.PadRight(8, filler);
                    nameStatus += separator + sqlTradeStSys.IdStActivation.PadRight(8, filler);
                    nameStatus += separator + sqlTradeStSys.IdStPriority.PadRight(8, filler);
                }
            }
            else
            {
                if (StrFunc.IsFilled(action))
                    nameStatus += separator + action.ToUpper().PadRight(8, filler);
                else
                    nameStatus += separator + string.Empty.PadRight(8, filler);

                for (int i = 0; i < 2; i++)
                    nameStatus += separator + string.Empty.PadRight(8, filler);
            }

            return nameStatus;
        }
        /// <summary>
        /// Retourne la date pour le process associé au message
        /// <para>Retrourne le paramètre DATE1</para>
        /// </summary>
        /// <returns></returns>
        public DateTime GetMasterDate()
        {
            DateTime ret = DateTime.MinValue;
            if (DtFunc.IsDateTimeFilled(GetDateTimeValueParameterById(MQueueBase.PARAM_DATE1)))
                ret = GetDateTimeValueParameterById(MQueueBase.PARAM_DATE1);
            else if (dateSpecified)
                ret = date;
            return ret;
        }
        /// <summary>
        /// Retourne la liste des paramètres du process associé au message
        /// </summary>
        /// <returns></returns>
        public virtual ArrayList GetListParameterAvailable()
        {
            ArrayList ret = new ArrayList
            {
                new MQueueparameter(PARAM_ISCREATEDBY_NORMMSGFACTORY, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_DATE1, TypeData.TypeDataEnum.date),
                new MQueueparameter(PARAM_DATE2, TypeData.TypeDataEnum.date),
                new MQueueparameter(PARAM_ISSIMUL, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_ISOBSERVER, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_ENTITY, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_ISWITHIO, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_ISKEEPHISTORY, TypeData.TypeDataEnum.@bool)
            };
            return ret;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <para>NB: Doit être en phase avec SetMessageQueueName</para>
        /// <returns></returns>
        public static string GetRegularExpressionPattern()
        {
            string regex = string.Empty;

            regex += @"^";
            regex += @"\w{15}";      // PROCESSTYPE sur 15 caractères
            regex += @"_\d{19}";     // DATE
            regex += @"(_\w{8}){3}"; // STATUS_ENVIRONMENT, STATUS_ACTIVATION,STATUS_PRIORITY
            regex += @"(_\w+){3}";   // IDENTIFIERS1,IDENTIFIERS2,IDENTIFIERS3
            //regex += @"(_\d{2})?";   // COMPTEUR (ie si Lock ou unmatch) 
            regex += @"(_\d{19})?";  // DATE2 (ie si Lock ou unmatch) 
            regex += @"$";

            return regex;
        }

        /// <summary>
        /// Alimente le header.messageQueueName
        /// <para>NB: Doit être en phase avec GetRegularExpressionPattern</para>
        /// </summary>
        /// <param name="pIsSetWithNewTimestamp">Stipule de recaluler le nom sur la base d'un nouveau Timestamp</param>
        /// <returns></returns>
        public bool SetMessageQueueName(bool pIsSetWithNewTimestamp)
        {
            StringBuilder sb = new StringBuilder();
            char filler = Convert.ToChar("X");
            const string format = "yyyyMMddHHmmssfffff";
            string timestamp = DateTime.Now.ToString(format);

            if (pIsSetWithNewTimestamp)
            {
                //NB: "If" mis en place pour éviter une potentielle fuite mémoire observée sous WS2008R2/64bits (PL 20130104)
                string old_timestamp = header.messageQueueName.Substring(16, format.Length);
                if (timestamp == old_timestamp)
                {
                    Thread.Sleep(1);
                    timestamp = DateTime.Now.ToString(format);
                }
                header.messageQueueName = header.messageQueueName.Replace(separator + old_timestamp + separator, separator + timestamp + separator);
            }
            else
            {
                string nameIdentifiers = RetIdentifiersForMessageQueueName();
                string nameStatus = RetStatusForMessageQueueName();

                sb.AppendFormat("{0}" + separator + "{1}{2}{3}",
                        ProcessType.ToString().PadRight(15, filler),//0
                        timestamp,                              	//1
                        nameStatus,									//2
                        nameIdentifiers 							//3		
                        );
                header.messageQueueName = sb.ToString();
                //header.messageQueueNameSpecified = StrFunc.IsFilled(header.messageQueueName);
                header.messageQueueNameSpecified = true;
            }
            //PL 20130103 Add pIsSetWithNewTimestamp and boolean return value 
            bool ret;
            if (ProcessType == Cst.ProcessTypeEnum.RESPONSE)
            {
                ret = true;
            }
            else
            {
                Regex regularEx = new Regex(GetRegularExpressionPattern());
                //if (false == regularEx.IsMatch(header.messageQueueName))
                //    header.messageQueueName = "IdentifierErrNoMatchWithPattern";
                ret = regularEx.IsMatch(header.messageQueueName);
            }

            return ret;
        }

        /// <summary>
        /// Retourne true si la paramètre {pParameterId} est un paramètre valide pour le process associé au message
        /// </summary>
        /// <param name="pParameterId"></param>
        /// <returns></returns>
        public bool IsExistParameter(string pParameterId)
        {
            bool ret = false;
            ArrayList al = this.GetListParameterAvailable();
            for (int j = 0; j < ArrFunc.Count(al); j++)
            {
                if (((MQueueparameter)al[j]).id == pParameterId)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        //
        #region AddParameter
        /// <summary>
        /// Ajoute un paramètre date
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pDate"></param>
        public void AddParameter(string pId, DateTime pDate)
        {
            AddParameter(pId, TypeData.TypeDataEnum.date, pDate);
        }
        /// <summary>
        /// Ajoute un paramètre Bool
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pbool"></param>
        public void AddParameter(string pId, Boolean pbool)
        {
            AddParameter(pId, TypeData.TypeDataEnum.@bool, pbool);
        }
        /// <summary>
        /// Ajoute un paramètre String
        /// </summary>
        /// <param name="pId">Identifiant du paramètre</param>
        /// <param name="pString"></param>
        public void AddParameter(string pId, string pString)
        {
            AddParameter(pId, TypeData.TypeDataEnum.@string, pString);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pInt"></param>
        /// FI 20120903  
        public void AddParameter(string pId, int pInt)
        {
            //FI 20120903 => usage de TypeData.TypeDataEnum.@int
            //AddParameter(pId, TypeData.TypeDataEnum.@string, pInt);
            //
            AddParameter(pId, TypeData.TypeDataEnum.@int, pInt);
        }
        public void AddParameter(string pId, TypeData.TypeDataEnum pDataType, object pValue)
        {
            if (null == parameters)
                parameters = new MQueueparameters();
            //
            ReflectionTools.AddItemInArray(parameters, "parameter", 0);
            //
            int index = parameters.Count - 1;
            parameters[index].id = pId;
            parameters[index].dataType = pDataType;
            parameters[index].SetValue(pValue);
            //
            parametersSpecified = true;

        }
        public void AddParameter(MQueueparameter parameter)
        {
            if (null == parameters)
                parameters = new MQueueparameters();
            //
            ReflectionTools.AddItemInArray(parameters, "parameter", 0);
            //
            int index = parameters.Count - 1;
            parameters[index] = parameter;
            //
            parametersSpecified = true;

        }
        #endregion

        
        public virtual string GetLogInfoDet()
        {
            return null;

        }

        /// <summary>
        ///  Construit la connectionString
        /// </summary>
        /// <returns></returns>
        /// FI 20200120 [XXXXX] Add Method
        private string BuildConnectionString()
        {
            string ret = header.ConnectionString;

            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //PL 20180720 Newness (See also CompleteConnectionString() on Web Application)
            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            if (ret.IndexOf("Initial Catalog=") >= 0)
            {
                //PL 20190107 Use csManager 
                CSManager csManager = new CSManager(ret);
                ret = csManager.Cs;

                //Source is a MS SQLServer connection string (NB: Oracle don't support the key "Application Name". See also CheckOracleConnection(...) in DataAccess.cs)
                int posAppName = ret.IndexOf("Application Name");
                if (posAppName < 0)
                {
                    if (!string.IsNullOrEmpty(this.serviceName))
                        ret = "Application Name=" + Software.NameMajorMinorType + " - " + this.serviceName + ";" + ret;
                    else
                        ret = "Application Name=" + Software.NameMajorMinorType + " - N/A;" + ret; //Cas de figure théoriquement improbable.
                }
                else
                {
                    int posSemiColon = ret.IndexOf(";", posAppName);
                    if (posSemiColon < 0)
                    {
                        if (!string.IsNullOrEmpty(this.serviceName))
                            ret = "Application Name=" + Software.NameMajorMinorType + " - " + this.serviceName + ";" + ret.Substring(0, posAppName);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.serviceName))
                            ret = "Application Name=" + Software.NameMajorMinorType + " - " + this.serviceName + ";" + ret.Substring(0, posAppName) + ret.Substring(posSemiColon + 1);
                    }
                }
                ret = csManager.GetSpheresInfoCs(true) + ret;
            }
            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            return ret;
        }
        #endregion Methods
    }
    #endregion MQueueBase

    #region MQueueConnectionString
    /// <summary>
    /// Représente la connectionString à la base de données
    /// </summary>
    public class MQueueConnectionString
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cryptSpecified;
        /// <summary>
        ///  Indicateur  connectionString cryptée ou non 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool crypt;
        /// <summary>
        /// représente la connectionString (peut être cryptée ou non cryptée)
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient true si la propriété la connectionString (propriété value) est cryptée
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsCrypt
        {
            //Si non renseigné, alors la donnée est cryptée.
            get { return (!cryptSpecified) || (crypt); }
        }
        #endregion Accessors
    }
    #endregion MQueueConnectionString
    #region MQueuedata
    // EG 20180423 Analyse du code Correction [CA1405]
    [System.Xml.Serialization.XmlRoot("data")]
    [ComVisible(false)]
    public class MQueuedata : XmlCDataSection
    {
        #region constructors
        public MQueuedata(string pData, XmlDocument pDoc) : base(pData, pDoc) { }
        public MQueuedata(string pData) : base(pData, null) { }
        public MQueuedata() : base(null, null) { }
        #endregion constructors
    }
    #endregion MQueuedata

    #region MQueueHeader
    /// <summary>
    /// Represente le header dans un message Mqueue
    /// </summary>
    public class MQueueHeader
    {
        #region Public Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool messageQueueNameSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("messageQueueName")]
        public string messageQueueName;

        /// <summary>
        ///  Obtient ou définit la connectionString
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("connectionString")]
        public MQueueConnectionString connectionString;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool creationTimestampSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("creationTimestamp")]
        public DateTime creationTimestamp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool requesterSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("requester")]
        public MQueueRequester requester;
        #endregion Public Members

        #region Accessors
        /// <summary>
        ///  représente la connectionString
        /// <para>Membre privé à usage technique</para> 
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// Obtient ou définit la ConnectionString
        /// </summary>
        /// FI 20200120 [XXXXX] refactoring (Il n'y a plus les éventuels Decrypt/Encrypt)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ConnectionString
        {
            set
            {
                _connectionString = value;
            }
            get
            {
                // En cas de deSerialization _connectionString est non renseigné. 
                //Dans ce cas lecture de connectionString.Value et application d'un éventuel Decrypt 
                if (StrFunc.IsEmpty(_connectionString))
                {
                    if (connectionString.IsCrypt)
                        _connectionString = Cryptography.Decrypt(connectionString.Value);
                    else
                        _connectionString = connectionString.Value;
                }
                return _connectionString;
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public MQueueHeader()
        {
            connectionString = new MQueueConnectionString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueueAttributes"></param>
        /// <param name="pMQueueType">Type du message MQueue incluant le header</param>
        /// RD 20130704 [18639] Surcharger avec le type du message MQueue maitre (IOMQueue, ...)
        public MQueueHeader(MQueueAttributes pMQueueAttributes, Type pMQueueType)
            : this()// FI 20201001 [XXXXX] Appel a this pour instancier  connectionString
        {
            // ConnectionString
            SetConnectionStringWithTimeout(pMQueueAttributes.connectionString, pMQueueType);

            // Timestamp
            creationTimestampSpecified = DtFunc.IsDateTimeFilled(pMQueueAttributes.timestamp);
            if (creationTimestampSpecified)
                creationTimestamp = pMQueueAttributes.timestamp;

            // Requester
            requesterSpecified = (null != pMQueueAttributes.requester);
            if (requesterSpecified)
                requester = pMQueueAttributes.requester;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Application d'un timeout en défaut en fonction du type de Messsage Queue
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pMQueueType"></param>
        /// RD 20130704 [18639] Surcharger avec le type du message MQueue maitre (IOMQueue, ...)
        protected void SetConnectionStringWithTimeout(string pConnectionString, Type pMQueueType)
        {
            //Recherche, dans le fichier web.config, d'une valeur de Timeout spécifique pour le service concerné (ex. InvoicingGen_Timeout = 180)
            // RD 20130704 [18639] utilisation du type du message MQueue maitre (IOMQueue, ...)
            //int timeout = (int)SystemSettings.GetAppSettings(this.GetType().Name.Replace("MQueue", "_Timeout"), typeof(Int32), -1);
            int timeout = (int)SystemSettings.GetAppSettings(pMQueueType.Name.Replace("MQueue", "_Timeout"), typeof(Int32), -1);

            bool isExistSpecificTimeout = (timeout > -1);

            if (!isExistSpecificTimeout)//Aucune valeur de Timeout spécifique n'est définie dans le fichier web.config
            {
                #region Récupération d'un éventuel Timeout présent dans la CS
                const string SQLSERVER_CS_TIMEOUT = "Connection Timeout=";
                int start = pConnectionString.IndexOf(SQLSERVER_CS_TIMEOUT);
                if (start >= 0)
                {
                    isExistSpecificTimeout = true;

                    string temp = pConnectionString.Substring(start).Replace(SQLSERVER_CS_TIMEOUT, string.Empty);
                    int semicolon = temp.IndexOf(";");
                    if (semicolon > 0)
                        temp = temp.Substring(0, semicolon);

                    timeout = IntFunc.IntValue(temp);
                }
                #endregion

                if (isExistSpecificTimeout)
                {
                    if (timeout > 0)
                        //Default for InvoicingGen service or all services
                        // RD 20130704 [18639] utilisation du type du message MQueue maitre (IOMQueue, ...)
                        //timeout = Math.Max(timeout, (this.GetType() == typeof(InvoicingGenMQueue)) ? 180 : 60);
                        timeout = Math.Max(timeout, (pMQueueType == typeof(InvoicingGenMQueue)) ? 180 : 60);
                }
                else
                {
                    //Default for InvoicingGen service or all services
                    // RD 20130704 [18639] utilisation du type du message MQueue maitre (IOMQueue, ...)
                    //timeout = (this.GetType() == typeof(InvoicingGenMQueue)) ? 180 : 60;
                    timeout = (pMQueueType == typeof(InvoicingGenMQueue)) ? 180 : 60;
                }
            }
            ConnectionString = CSTools.SetTimeOut(pConnectionString, timeout);
        }
        /// <summary>
        ///  Génère l'élément connectionString.Value (doit être effectué avant la serialisation du message Mqueue)
        ///  <para>La connectionString est cryptée ou non en fonction de l'attribut crypt</para>
        /// </summary>
        // FI 20201001 [XXXXX] Add Method
        public void BuildConnectionStringValue()
        {
            if (connectionString.IsCrypt)
                connectionString.Value = Cryptography.Encrypt(ConnectionString);
            else
                connectionString.Value = ConnectionString;
        }
        #endregion Methods
    }
    #endregion MQueueHeader

    #region MQueueparameter
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRoot("parameter")]
    public class MQueueparameter
    {
        #region Members
        /// <summary>
        /// Id du paramètre
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("id")]
        public string id;
        /// <summary>
        /// Identifiant du paramètre
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nameSpecified;
        [System.Xml.Serialization.XmlAttribute("name")]
        public string name;
        /// <summary>
        /// Identifiant long du paramètre
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;
        [System.Xml.Serialization.XmlAttribute("displayname")]
        public string displayName;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("direction")]
        public string direction;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("datatype")]
        public TypeData.TypeDataEnum dataType;
        /// <summary>
        /// Valeur string du paramètre (valeur au format ISO)
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "anyURI")]
        public string Value;
        /// <summary>
        /// Valeur string du paramètre (valeur au format ISO) = IDENTIFIER / DISPLAYNAME / DESCRIPTION lié à VALUE etc...
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExValueSpecified;
        [System.Xml.Serialization.XmlAttribute("exvalue")]
        public string ExValue;
        /// <summary>
        /// 
        /// </summary>
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReturnTypeSpecified;
        private Cst.ReturnSPParamTypeEnum _returnType;

        /// <summary>
        /// Non sérialisée : utilisée lors de l'alimentation avec des paramètres de tâches I/O
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isMandatory;

        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("returntype")]         //GP 20070130
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public Cst.ReturnSPParamTypeEnum ReturnType
        {
            set
            {
                _returnType = value;
                ReturnTypeSpecified = (Cst.ReturnSPParamTypeEnum.NA != _returnType);
            }
            get
            {
                return _returnType;
            }

        }
        #endregion Accessors
        #region Constructors
        public MQueueparameter() { }
        public MQueueparameter(string pId, TypeData.TypeDataEnum pTypedata)
            : this(pId, string.Empty, string.Empty, pTypedata, Cst.ReturnSPParamTypeEnum.NA) { }
        public MQueueparameter(string pId, string pName, string pDisplayName, TypeData.TypeDataEnum pTypedata)
            : this(pId, pName, pDisplayName, pTypedata, Cst.ReturnSPParamTypeEnum.NA) { }
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public MQueueparameter(string pId, string pName, string pDisplayName, TypeData.TypeDataEnum pTypedata, Cst.ReturnSPParamTypeEnum pReturnType)
        {
            id = pId;
            nameSpecified = StrFunc.IsFilled(pName);
            name = pName;
            displayNameSpecified = StrFunc.IsFilled(pDisplayName);
            displayName = pDisplayName;
            dataType = pTypedata;
            //
            ReturnTypeSpecified = (pReturnType != Cst.ReturnSPParamTypeEnum.NA);
            ReturnType = pReturnType; //GP 20070130
            ExValueSpecified = false;
        }
        #endregion Constructors
        #region Methods

        /// <summary>
        /// Pour clonner l'instance
        /// </summary>
        /// <returns>Une nouvelle instance à l'identique</returns>
        public MQueueparameter Clone()
        {
            // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
            MQueueparameter newMQueueparameter = new MQueueparameter
            {
                id = this.id,
                nameSpecified = this.nameSpecified,
                name = this.name,
                displayNameSpecified = this.displayNameSpecified,
                displayName = this.displayName,
                direction = this.direction,
                dataType = this.dataType,
                Value = this.Value,
                // EG 20140522 Add ExValueSpecified
                ExValueSpecified = this.ExValueSpecified,
                ExValue = this.ExValue,
                ReturnTypeSpecified = this.ReturnTypeSpecified,
                ReturnType = this.ReturnType
            };

            return newMQueueparameter;
        }

        /// <summary>
        ///  Alimente Value avec une donnée de type date
        /// </summary>
        /// <param name="pDate"></param>
        public void SetValue(DateTime pDate)
        {
            switch (dataType)
            {
                case TypeData.TypeDataEnum.datetime:
                    // FI 20200819 [XXXXX] Usage deu format FmtTZISODateTime2 si date UTC
                    if (pDate.Kind == DateTimeKind.Utc)
                        Value = DtFunc.DateTimeToString(pDate, DtFunc.FmtTZISODateTime2);
                    else
                        Value = DtFunc.DateTimeToStringISO(pDate);
                    break;
                case TypeData.TypeDataEnum.date:
                    Value = DtFunc.DateTimeToString(pDate, DtFunc.FmtISODate);
                    break;
                case TypeData.TypeDataEnum.time:
                    Value = DtFunc.DateTimeToString(pDate, DtFunc.FmtISOTime);
                    break;
            }
        }
        /// <summary>
        ///  Alimente Value avec une donnée de type bool
        /// </summary>
        /// <param name="pbool"></param>
        public void SetValue(Boolean pBool)
        {
            Value = ObjFunc.FmtToISo(pBool, dataType);
        }
        /// <summary>
        ///  Alimente Value avec une donnée de type string
        /// </summary>
        /// <param name="pString"></param>
        public void SetValue(string pString)
        {
            Value = ObjFunc.FmtToISo(pString, dataType);
        }
        /// <summary>
        ///  Alimente Ex avec une donnée de type string
        /// </summary>
        /// <param name="pString"></param>
        public void SetValue(object pObj, string pString)
        {
            Value = ObjFunc.FmtToISo(pObj, dataType);
            ExValue = ObjFunc.FmtToISo(pString, TypeData.TypeDataEnum.@string);
            ExValueSpecified = StrFunc.IsFilled(ExValue);
        }
        /// <summary>
        ///  Alimente Value avec une donnée de type integer
        /// </summary>
        /// <param name="pInt"></param>
        public void SetValue(int pInt)
        {
            Value = ObjFunc.FmtToISo(pInt, dataType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObj"></param>
        public void SetValue(Object pObj)
        {
            Value = ObjFunc.FmtToISo(pObj, dataType);
        }

        #region GetObjectValue
        public object GetObjectValue()
        {
            object ret;
            switch (dataType)
            {
                case TypeData.TypeDataEnum.@bool:
                case TypeData.TypeDataEnum.boolean:
                    ret = BoolFunc.IsTrue(Value);
                    break;
                case TypeData.TypeDataEnum.datetime:
                    ret = new DtFunc().StringToDateTime(Value, DtFunc.FmtISODateTime);
                    break;
                case TypeData.TypeDataEnum.date:
                    ret = new DtFunc().StringToDateTime(Value, DtFunc.FmtISODate);
                    break;
                case TypeData.TypeDataEnum.time:
                    ret = new DtFunc().StringToDateTime(Value, DtFunc.FmtISOTime);
                    break;
                case TypeData.TypeDataEnum.@decimal:
                    ret = DecFunc.DecValueFromInvariantCulture(Value);
                    break;
                case TypeData.TypeDataEnum.@string:
                case TypeData.TypeDataEnum.text:
                    ret = Value;
                    break;
                case TypeData.TypeDataEnum.integer:
                case TypeData.TypeDataEnum.@int:
                    ret = IntFunc.IntValue(Value);
                    break;
                default:
                    throw new NotImplementedException("TypeDataEnum.unknown not available");
            }
            return ret;
        }
        #endregion GetObjectValue

        #endregion Methods
    }
    #endregion MQueueparameter
    #region MQueueparameters
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRoot("parameters")]
    public class MQueueparameters
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("parameter")]
        public MQueueparameter[] parameter;
        #endregion Members
        #region Accessors
        public int Count
        {
            get { return ArrFunc.Count(parameter); }
        }
        #endregion Accessors
        #region Indexors
        public MQueueparameter this[int pIndex]
        {
            get
            {
                return parameter[pIndex];
            }
            set
            {
                parameter[pIndex] = value;
            }
        }
        public MQueueparameter this[string pId]
        {
            get
            {
                MQueueparameter ret = null;
                for (int i = 0; i < Count; i++)
                {
                    if (pId == parameter[i].id)
                    {
                        ret = parameter[i];
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion Indexors
        #region Constructors
        public MQueueparameters() { }
        #endregion Constructors
        #region Methods
        #region Add
        public void Add(MQueueparameter pMQueueparameter)
        {
            Add(new MQueueparameter[] { pMQueueparameter });
        }
        public void Add(MQueueparameter[] pMQueueparameter)
        {
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(parameter); i++)
                al.Add(parameter[i]);
            for (int i = 0; i < ArrFunc.Count(pMQueueparameter); i++)
                al.Add(pMQueueparameter[i]);
            parameter = (MQueueparameter[])al.ToArray(typeof(MQueueparameter));
        }
        #endregion Add
        #region Remove
        public void Remove(string pId)
        {
            ArrayList al = new ArrayList();
            foreach (MQueueparameter item in parameter)
            {
                if (item.id != pId)
                    al.Add(item);
            }
            parameter = (MQueueparameter[])al.ToArray(typeof(MQueueparameter));
        }
        #endregion Remove
        #region Clone
        /// <summary>
        /// Pour clonner l'instance
        /// </summary>
        /// <returns>Une nouvelle instance à l'identique</returns>
        public MQueueparameters Clone()
        {
            MQueueparameters newMQueueparameters = new MQueueparameters();
            ArrayList newListParameter = new ArrayList();

            if (ArrFunc.IsFilled(parameter))
            {
                foreach (MQueueparameter mQueueparameter in parameter)
                    newListParameter.Add(mQueueparameter.Clone());
            }

            newMQueueparameters.parameter = (MQueueparameter[])newListParameter.ToArray(typeof(MQueueparameter));

            return newMQueueparameters;
        }
        #endregion Clone
        #region GetDateTimeValueParameterById
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune DateTime.MinValue si le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public DateTime GetDateTimeValueParameterById(string pParamId)
        {
            DateTime ret = DateTime.MinValue;
            if (null != GetObjectValueParameterById(pParamId))
                ret = Convert.ToDateTime(GetObjectValueParameterById(pParamId));
            return ret;
        }
        #endregion GetDateTimeValueParameterById
        #region GetBoolValueParameterById
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune false si le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public Boolean GetBoolValueParameterById(string pParamId)
        {
            Boolean ret = false;
            if (null != GetObjectValueParameterById(pParamId))
                ret = Convert.ToBoolean(GetObjectValueParameterById(pParamId));
            return ret;
        }
        #endregion GetBoolValueParameterById
        #region GetIntValueParameterById
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// <para>Retoune 0 sir le paramètre est non renseigné</para>
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public int GetIntValueParameterById(string pParamId)
        {
            int ret = 0;
            if (null != GetObjectValueParameterById(pParamId))
                ret = Convert.ToInt32(GetObjectValueParameterById(pParamId));
            return ret;
        }
        #endregion GetIntValueParameterById
        #region GetStringValueParameterById
        /// <summary>
        /// Retourne la valeur du paramètre {pParamId}
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public string GetStringValueParameterById(string pParamId)
        {
            string ret = null;
            if (null != GetObjectValueParameterById(pParamId))
                ret = Convert.ToString(GetObjectValueParameterById(pParamId));
            return ret;
        }
        #endregion GetStringValueParameterById
        #region GetObjectValueParameterById
        public object GetObjectValueParameterById(string pParamId)
        {
            object ret = null;
            MQueueparameter mQueueParameter = null;
            //
            if (ArrFunc.IsFilled(parameter))
            {
                for (int i = 0; i < parameter.Length; i++)
                {
                    if (parameter[i].id == pParamId)
                    {
                        mQueueParameter = parameter[i];
                        break;
                    }
                }
            }

            if (null != mQueueParameter)
            {
                ret = mQueueParameter.GetObjectValue();
            }
            return ret;
        }
        #endregion GetObjectValueParameterById
        #region GetExtendValueParameterById
        public string GetExtendValueParameterById(string pParamId)
        {
            string ret = null;
            if (ArrFunc.IsFilled(parameter))
            {
                for (int i = 0; i < parameter.Length; i++)
                {
                    if (parameter[i].id == pParamId)
                    {
                        ret = parameter[i].ExValue;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion GetStringExtendValueParameterById
        #endregion Methods
    }
    #endregion MQueueparameters

    #region Entity
    /// <summary>
    /// 
    /// </summary>
    //PL 20101125 Utilisation ic de l'objet Party de FpML
    //            WARNING: Attention l'obet est ici dupliqué car non accessible ! (A finaliser, pour le partager et évité la duplication)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class Entity
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("entityId", Order = 1)]
        public EntityId[] entityId;
        [System.Xml.Serialization.XmlElementAttribute("entityName", Order = 2)]
        public string partyName;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
        #region Constructors
        public Entity()
        {
            entityId = new EntityId[1] { new EntityId() };
        }
        #endregion Constructors    }
    }
    #endregion Entity
    #region EntityId
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class EntityId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string entityIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion EntityId

    #region MQueueRequester
    /// <summary>
    /// 
    /// </summary>
    /// FI 20120129 add appName (Permet de connaître l'application à l'origine du MQueueRequester
    [System.Xml.Serialization.XmlRoot("requester")]
    public class MQueueRequester
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idASpecified;
        [System.Xml.Serialization.XmlAttribute("id")]
        public int idA;

        [System.Xml.Serialization.XmlElement("date")]
        public DateTime date;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool appNameSpecified;
        [System.Xml.Serialization.XmlElement("appName")]
        public string appName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sessionIdSpecified;
        [System.Xml.Serialization.XmlElement("sessionId")]
        public string sessionId;

        [System.Xml.Serialization.XmlElement("hostName")]
        public string hostName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]

        public bool entitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("entity")]
        public Entity entity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTRKSpecified;
        [System.Xml.Serialization.XmlAttribute("idTRK")]
        public int idTRK;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPROCESSSpecified;
        [System.Xml.Serialization.XmlAttribute("idPROCESS")]
        public int idPROCESS;


        #endregion Members
        #region Constructors
        public MQueueRequester() { }
        public MQueueRequester(AppSession pSession, DateTime dtRequest) : this(0, pSession, dtRequest) { }
        public MQueueRequester(int pIdTRK, AppSession pSession, DateTime dtRequest)
        {
            idTRK = pIdTRK;
            idTRKSpecified = (pIdTRK > 0);

            idA = pSession.IdA;
            idASpecified = (idA > 0);

            appName = pSession.AppInstance.AppName;
            appNameSpecified = StrFunc.IsFilled(appName);

            sessionId = pSession.SessionId;
            sessionIdSpecified = StrFunc.IsFilled(sessionId);

            hostName = pSession.AppInstance.HostName;
            date = dtRequest;

            if (pSession.IdA_Entity > 0)
            {
                entity = new Entity
                {
                    otcmlId = pSession.IdA_Entity.ToString(),
                    partyName = null
                };
                entity.entityId[0].entityIdScheme = Cst.OTCml_ActorIdentifierScheme;
                entity.entityId[0].Value = pSession.IdA_Identifier_Entity;
            }
            entitySpecified = (entity != null);

        }
        #endregion Constructors
    }
    #endregion New_MQueueRequester

}
