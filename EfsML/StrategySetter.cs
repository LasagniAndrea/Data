using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EfsML.Business;
using EfsML.Interface;
using FpML.Interface;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;

namespace EfsML.StrategyMarker
{

    /// <summary>
    /// Class repository for StrategyTypeScheme values, issued from the Spheres ENUM table (CODE "StrategyTypeScheme")
    /// </summary>
    public class StrategyEnumRepository
    {
        /// <summary>
        /// Enumeration containing all the KNOWN strategies. 
        /// Unknown strategies are not specified in this enumeration, but they can appear in the ENUM table.
        /// </summary>
        [DataContract(Name = "Strategy")]
        public enum StrategyEnum
        {
            /// <summary>
            /// No strategy type (for trade Posting/Give-up do not identify a strategy)
            /// </summary>
            Unknown,

            [EnumMember(Value = "SPREAD")]
            Spread,
            [EnumMember(Value = "STRADDLE")]
            Straddle,
            [EnumMember(Value = "STRANGLE")]
            Strangle,
            [EnumMember(Value = "GUTS")]
            Guts,
            [EnumMember(Value = "COMBO")]
            Combo,
            [EnumMember(Value = "CALENDAR-SPREAD")]
            CalendarSpread,
            [EnumMember(Value = "DIAGONAL-CALENDAR-SPREAD")]
            DiagonalCalendarSpread,
            [EnumMember(Value = "BUTTERFLY")]
            Butterfly,
            [EnumMember(Value = "CONDOR")]
            Condor,
            [EnumMember(Value = "RATIO-SPREAD")]
            RatioSpread,
            [EnumMember(Value = "LADDER")]
            Ladder,
            [EnumMember(Value = "SPREAD-VERSUS")]
            SpreadVersus,

            /// <summary>
            /// Custom/generic strategy, identified by the "{0}Legs" label. For a strategy with 2 legs not recognized by a previous known schema
            /// it will be "2Legs".
            /// </summary>
            Custom
        }

        const string SQLREQUEST = "select VALUE, EXTVALUE from dbo.ENUM where code = 'StrategyTypeScheme'";

        // format string of the custom strategy name
        const string CUSTOMSTRATEGYFORMAT = "{0}Legs";

        // string suffix of the custom strategy name
        const string CUSTOMSTRATEGYNAME = "Legs";

        // regex to extract from its name how many legs there are in a Posting/Give-up strategy
        static readonly Regex m_GetLegs = new Regex(String.Format(@"^(\d+){0}$", CUSTOMSTRATEGYNAME));

        [DataContract(
        Name = DataHelper<EnumValue>.DATASETROWNAME,
        Namespace = DataHelper<EnumValue>.DATASETNAMESPACE)]
        private sealed class EnumValue
        {

            [DataMember(Name = "VALUE", Order = 1)]
            public string Value
            {
                get;
                set;
            }

            [DataMember(Name = "EXTVALUE", Order = 2)]
            public string ExtValue
            {
                get;
                set;
            }
        }




        readonly string m_CS;

        Dictionary<StrategyEnum, EnumValue> m_DictStrategies;

        /// <summary>
        /// Create a new instance of the repository
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        public StrategyEnumRepository(string pCS)
        {
            m_CS = pCS;
        }

        /// <summary>
        /// Build and fill the repository with the values issued from the StrategyTypeScheme database enumeration
        /// </summary>
        /// <returns></returns>
        public bool BuildDictionary()
        {
            if (String.IsNullOrEmpty(m_CS))
            {
                throw new ArgumentException("The StrategyEnum object has been not correctly created");
            }

            bool ret = false;

            using (IDbConnection connection = DataHelper.OpenConnection(m_CS))
            {

                List<EnumValue> values = DataHelper<EnumValue>.ExecuteDataSet(connection, CommandType.Text, SQLREQUEST);

                try
                {
                    StrategyEnum parsedStrategy = StrategyEnum.Unknown;

                    m_DictStrategies = (from value in values where IsKnownStrategy(value.Value, ref parsedStrategy) select value).
                        ToDictionary(elem => parsedStrategy);

                    m_DictStrategies.Add(StrategyEnum.Custom,
                        new EnumValue { Value = CUSTOMSTRATEGYFORMAT, ExtValue = null });

                    ret = true;
                }
                catch (ArgumentException)
                {
                    string lstValues = StrFunc.StringArrayList.StringArrayToStringList(values.Select(x => x.Value).ToArray());
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5177), 0, new LogParam(lstValues)));
                    ret = false;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get a value from the repository
        /// </summary>
        /// <param name="pStrategy">enum value coupled with the wanted value</param>
        /// <returns></returns>
        public string GetValue(StrategyEnum pStrategy)
        {
            string value = null;

            if (m_DictStrategies.ContainsKey(pStrategy))
            {
                value = m_DictStrategies[pStrategy].Value;
            }

            return value;
        }

        /// <summary>
        /// Check when the input strategy pStrategy is compliant with the input control value pControlValue. 
        /// The input control value can contain either a strategy name (defined in the StrategyEnum enumeration <seealso cref="StrategyEnum"/>), 
        /// as well as a strategy group code (defined in the FeeStrategyTypeEnum enumeration, 
        /// <seealso cref="EFS.ACommon.Cst.FeeStrategyTypeEnum"/>)
        /// </summary>
        /// <param name="pStrategy">input strategy name we want to compare with the control value</param>
        /// <param name="pControlValue">input control value</param>
        /// <returns>true when the strategy name and the control value matches</returns>
        public static bool IsStrategyPartOf(string pStrategy, string pControlValue)
        {
            // 0. when at least one of the input values is null, the strategy compatibility check fails suddenly
            if (pStrategy == null || pControlValue == null)
            {
                return false;
            }

            bool ok = false;

            if (pStrategy.Equals(pControlValue, StringComparison.InvariantCultureIgnoreCase))
            {
                // 1. control matches exactly the strategy type, the strategy is compliant
                ok = true;
            }
            // 2. check if the control value is filled with a strategy group, and verify if the input strategy is making part of the group
            else if ((System.Enum.IsDefined(typeof(Cst.FeeStrategyTypeEnum), pControlValue)))
            {
                Cst.FeeStrategyTypeEnum strategyGroup =
                    (Cst.FeeStrategyTypeEnum)System.Enum.Parse(typeof(Cst.FeeStrategyTypeEnum), pControlValue);

                StrategyEnum strategyType = default;

                bool isKnown = IsKnownStrategy(pStrategy, ref strategyType);

                int legs = HowManyLegs(pStrategy, strategyType, isKnown);

                switch (strategyGroup)
                {
                    case Cst.FeeStrategyTypeEnum.ALL:

                        ok = true;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALLKNOWN:

                        ok = isKnown;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALLUNKNOWN:

                        ok = !isKnown;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALL2LEGS:

                        ok = legs == 2;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALL3LEGS:

                        ok = legs == 3;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALL4LEGS:

                        ok = legs == 4;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALL5MORELEGS:

                        ok = legs > 4;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALLUNKNOWN2LEGS:

                        ok = !isKnown && legs == 2;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALLUNKNOWN3LEGS:

                        ok = !isKnown && legs == 3;
                        break;

                    case Cst.FeeStrategyTypeEnum.ALLUNKNOWN4LEGS:

                        ok = !isKnown && legs == 4;
                        break;

                    default:

                        break;


                }
            }

            return ok;
        }

        /// <summary>
        /// Get the number of legs of a strategy according with its name, or type
        /// </summary>
        /// <param name="pStrategy">strategy name</param>
        /// <param name="pStrategyType">strategy type, the value is "Unknown" 
        /// when the input strategy is not known/managed by the Spheres sytem</param>
        /// <param name="pIsKnown">pass true when the strategy is known by the Spheres sytem, along with a valid strategy type</param>
        /// <returns>the number of legs of the strategy, 0 in case the strategy is not recognized</returns>
        public static int HowManyLegs(string pStrategy, StrategyEnum pStrategyType, bool pIsKnown)
        {
            if (pStrategy == null)
            {
                return 0;
            }

            int legs = 0;

            if (!pIsKnown || pStrategyType == StrategyEnum.Custom)
            {
                // 1. if the strategy is not known by the Spheres system (Custom or Unknown) we use its name, 
                //    supposed to be expressed as "####Legs", to get the legs number
                Match match = m_GetLegs.Match(pStrategy);

                if (match.Success)
                {
                    legs = Convert.ToInt32(match.Groups[1].Value);
                }
            }
            else
            {
                // 2. When the strategy is well known by the Spheres system, we use its type to get the legs number
                switch (pStrategyType)
                {

                    case StrategyEnum.CalendarSpread:
                    case StrategyEnum.Combo:
                    case StrategyEnum.DiagonalCalendarSpread:
                    case StrategyEnum.Guts:
                    case StrategyEnum.RatioSpread:
                    case StrategyEnum.Spread:
                    case StrategyEnum.Straddle:
                    case StrategyEnum.Strangle:

                        legs = 2;
                        break;

                    case StrategyEnum.Butterfly:
                    case StrategyEnum.Ladder:
                    case StrategyEnum.SpreadVersus:

                        legs = 3;
                        break;

                    case StrategyEnum.Condor:

                        legs = 4;
                        break;

                    default:
                        break;
                }
            }

            return legs;
        }

        /// <summary>
        /// Get when the known/managed status of the input strategy 
        /// </summary>
        /// <remarks>warning: just someone of the defined strategy inside of the Spheres enumeration are known/managed, 
        /// because of that the method does not use Enum.Parse over the input value, 
        /// but it uses a switch to return a typed strategy only when the strategy is known</remarks>
        /// <param name="pStrategy">input strategy we want to check its status</param>
        /// <param name="opParsedStrategy">the output typed strategy</param>
        /// <returns>true, when the input strategy is well known/managed</returns>
        public static bool IsKnownStrategy(string pStrategy, ref StrategyEnum opParsedStrategy)
        {
            bool ret = true;

            switch (pStrategy)
            {

                case "SPREAD":

                    opParsedStrategy = StrategyEnum.Spread;
                    break;

                case "STRADDLE":

                    opParsedStrategy = StrategyEnum.Straddle;
                    break;

                case "STRANGLE":

                    opParsedStrategy = StrategyEnum.Strangle;
                    break;

                case "GUTS":

                    opParsedStrategy = StrategyEnum.Guts;
                    break;

                case "COMBO":

                    opParsedStrategy = StrategyEnum.Combo;
                    break;

                case "CALENDAR-SPREAD":

                    opParsedStrategy = StrategyEnum.CalendarSpread;
                    break;

                case "DIAGONAL-CALENDAR-SPREAD":

                    opParsedStrategy = StrategyEnum.DiagonalCalendarSpread;
                    break;

                case "BUTTERFLY":

                    opParsedStrategy = StrategyEnum.Butterfly;
                    break;

                case "CONDOR":

                    opParsedStrategy = StrategyEnum.Condor;
                    break;

                case "RATIO-SPREAD":

                    opParsedStrategy = StrategyEnum.RatioSpread;
                    break;

                case "LADDER":

                    opParsedStrategy = StrategyEnum.Ladder;
                    break;

                case "SPREAD-VERSUS":

                    opParsedStrategy = StrategyEnum.SpreadVersus;
                    break;

                default:

                    ret = false;
                    opParsedStrategy = StrategyEnum.Unknown;

                    break;

            }

            return ret;
        }

    }

    /// <summary>
    /// Common reference fields set
    /// </summary>
    /// <remarks>
    /// Any enumeration item is related to the entry in the ENUMS table having CODE = 'OrderReferenceEnum'
    /// </remarks>
    [DataContract(Name = "COMMONID")]
    public enum OrderReferenceEnum
    {
        /// <summary>
        /// FIX Tag 37 (@OrdID)
        /// </summary>
        [EnumMember(Value = "OrderID")]
        OrderID,
        /// <summary>
        /// FIX Tag 198 (@OrdID2)
        /// </summary>
        [EnumMember(Value = "SecondaryOrderID")]
        SecondaryOrderID,
        /// <summary>
        /// FIX Tag 11 (@ClOrdID)
        /// </summary>
        [EnumMember(Value = "ClOrderID")]
        ClOrderID,
        /// <summary>
        /// FIX Tag 526 (@ClOrdID2)
        /// </summary>
        [EnumMember(Value = "SecondaryClOrderID")]
        SecondaryClOrderID,
        /// <summary>
        /// Spheres® Front Reference
        /// </summary>
        [EnumMember(Value = "FrontId")]
        FrontId,
        /// <summary>
        /// Spheres® Folder reference
        /// </summary>
        [EnumMember(Value = "FolderId")]
        FolderId,
        /// <summary>
        /// Spheres® External reference
        /// </summary>
        [EnumMember(Value = "ExtlLink")]
        ExtlLink,
    }

    /// <summary>
    /// Generic repository
    /// </summary>
    internal abstract class GenericStrategyRepository
    {
        /// <summary>
        /// Returns the main SQL request which loads the repository
        /// </summary>
        protected abstract string SQLREQUEST { get; }

        /// <summary>
        /// get the initialization status of the object
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        /// Get/Set the DB Parameters used in the SQLREQUEST string
        /// </summary>
        /// FI 20210727 [XXXXX] Mise en commentaire car non utilisé
        /// protected DataParameter[] Parameters { get; set; }

        /// <summary>
        /// Connection string
        /// </summary>
        protected string CS { get; set; }

        /// <summary>
        /// Current business date
        /// </summary>
        protected DateTime DtBusiness { get; set; }

        /// <summary>
        /// Current market internal id
        /// </summary>
        protected int IDM { get; set; }

        /// <summary>
        /// Current entity internal id
        /// </summary>
        protected int IDA_ENTITY { get; set; }

        /// <summary>
        /// Create a new instance of the repository
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        /// <param name="pDtBusiness">current business date, NOT empty</param>
        /// <param name="pIDM">market id, major than 0</param>
        /// <param name="pIDA_ENTITY">entity id, major than 0</param>
        public GenericStrategyRepository(string pCS, DateTime pDtBusiness, int pIDM, int pIDA_ENTITY)
        {
            this.CS = pCS;

            this.DtBusiness = pDtBusiness;

            IDM = pIDM;

            IDA_ENTITY = pIDA_ENTITY;
        }

        /// <summary>
        /// Init the repository
        /// </summary>
        /// <returns>true when the repository is well initialized, false otherwise</returns>
        /// <remarks>call it before any other class methods</remarks>
        public bool Init()
        {
            if (String.IsNullOrEmpty(CS) || IDM <= 0 || IDA_ENTITY <= 0 || DtBusiness <= DateTime.MinValue)
            {
                throw new ArgumentException("The StrategyIdentificationRulesRepository object has been not correctly created");
            }

            // FI 20210727 [XXXXX] Mise en commentaire car non utilisé
            //Parameters = new DataParameter[]
            //    {
            //        new DataParameter(CS, "IDA_ENTITY", DbType.Int32),
            //        new DataParameter(CS, "IDM", DbType.Int32),
            //        new DataParameter(CS, "DTBUSINESS", DbType.Date),
            //    };

            return (Initialized = true);
        }
    }

    // 20120612 MF Ticket 17786
    /// <summary>
    /// Containing all the identification rules loaded for the current strategy identification process
    /// </summary>
    internal class StrategyIdentificationRulesRepository : GenericStrategyRepository
    {
        /// <summary>
        /// Get all the identification rules for the current entity/market couple (this module is running in the EOD process)
        /// </summary>
        /// <remarks>
        /// The first trick si filtering all the rules by contract, contract group, market and market group and returns just those ones
        /// related to the current market (NB: this module is running in the EOD process).
        /// The second trick is getting back a line filled with the internal instrument indentifier for each rule by product, product
        /// group, instrument, instrument group ; IOW for each product/product group/instrument group rule
        /// a line will be returned  for each instrument making part of the item.
        /// The third trick is similar to the second one but related to the derivative contracts. For each contract/contract group rule
        /// a line will be returned  for each derivative contact making part of the item.
        /// </remarks>
        /// FI 20170908 [23409] Modify
        /// FI 20170919 [23409] Modify
        protected override string SQLREQUEST
        {
            get
            {

                // FI 20170908 [23409] Gestion de la valeur DerivativeContract pour la colonne TYPECONTRACT
                return @"select 

                stg.DISPLAYNAME, 
                stg.COMMONID,
                stg.TYPEINSTR,
                stg.IDINSTR,
                stg.TYPECONTRACT,
                stg.IDCONTRACT,
                isnull(dc.IDDC,cg.IDXC) as IDDC,
                isnull(i.IDI, ig.IDI) as IDI,
                stg.DTENABLED,
                stg.DTDISABLED

                from dbo.STGIDENTRULES stg

                /* Retrieve all the instructions related to contracts */

                left outer join dbo.DERIVATIVECONTRACT dc on
                (dc.IDDC = stg.IDCONTRACT and stg.TYPECONTRACT = 'DerivativeContract' and dc.IDM = @IDM)

                /* Retrieve all the instructions related to groups of contracts */

                left outer join dbo.CONTRACTG cg on
                (cg.IDGCONTRACT = stg.IDCONTRACT and stg.TYPECONTRACT = 'GrpContract')

                left outer join dbo.DERIVATIVECONTRACT dcXContractGroup on (dcXContractGroup.IDDC = cg.IDXC and cg.CONTRACTCATEGORY = 'DerivativeContract'  and dcXContractGroup.IDM = @IDM)

                /* Retrieve all the instructions related to groups of markets */

                left outer join dbo.MARKETG mg on
                (mg.IDGMARKET = stg.IDCONTRACT and stg.TYPECONTRACT = 'GrpMarket' and  mg.IDM = @IDM)

                /* Retrieve all the instructions related to instruments or products */

                left outer join dbo.INSTRUMENT i on
                (i.IDI = stg.IDINSTR and stg.TYPEINSTR = 'Instr') or (i.IDP = stg.IDINSTR and stg.TYPEINSTR = 'Product')

                left outer join dbo.PRODUCT pXInstr on (pXInstr.IDP = i.IDP and pXInstr.GPRODUCT = 'FUT')

                /* Retrieve all the instructions related to groups of instruments */

                left outer join dbo.INSTRG ig on (ig.IDGINSTR = stg.IDINSTR and stg.TYPEINSTR = 'GrpInstr')

                left outer join dbo.PRODUCT pXInstr2 on (pXInstr2.IDP = i.IDP and i.IDI = ig.IDI  and pXInstr2.GPRODUCT = 'FUT')

                where

                stg.IDA = @IDA_ENTITY

                and

                /* TYPECONTRACT conditions block (with market filter) */
                (
                     (stg.TYPECONTRACT is null)  
                  or (stg.TYPECONTRACT = 'None')   
                  or (stg.TYPECONTRACT = 'Market' and stg.IDCONTRACT = @IDM) 
                  or (stg.TYPECONTRACT in ('Market', 'DerivativeContract', 'GrpMarket', 'GrpContract') and stg.IDCONTRACT is null)
                  or (stg.TYPECONTRACT = 'DerivativeContract' and stg.IDCONTRACT is not null and dc.IDDC is not null)
                  or (stg.TYPECONTRACT = 'GrpMarket' and stg.IDCONTRACT is not null and mg.IDM is not null)
                  or (stg.TYPECONTRACT = 'GrpContract' and stg.IDCONTRACT is not null and cg.IDXC is not null)
                )
                
                and 

                /*  TYPEINSTR conditions block */
                (
                     (stg.TYPEINSTR is null)
                  or (stg.TYPEINSTR = 'None')    
                  or (stg.TYPEINSTR in ('Product', 'Instr', 'GrpProduct', 'GrpInstr') and stg.IDINSTR is null) 
                  or (stg.TYPEINSTR = 'Product' and stg.IDINSTR is not null and i.IDP is not null)
                  or (stg.TYPEINSTR = 'Instr' and stg.IDINSTR is not null and i.IDI is not null)
                  or (stg.TYPEINSTR = 'GrpInstr' and stg.IDINSTR is not null and ig.IDI is not null)
                )";
            }
        }

        /// <summary>
        /// Get all the loaded rules 
        /// </summary>
        public List<StrategyIdentificationRules> Rules { get; private set; }

        /// <summary>
        /// Get all the identification rules grouped by the common reference field
        /// </summary>
        public IEnumerable<StrategyIdentificationRulesGroup> RulesConditions { get; private set; }

        /// <summary>
        /// Create a new instance of the repository
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        /// <param name="pDtBusiness">current business date, NOT empty</param>
        /// <param name="pIDM">market id, major than 0</param>
        /// <param name="pIDA_ENTITY">entity id, major than 0</param>
        public StrategyIdentificationRulesRepository(string pCS, DateTime pDtBusiness, int pIDM, int pIDA_ENTITY) :
            base(pCS, pDtBusiness, pIDM, pIDA_ENTITY)
        { }

        /// <summary>
        /// Load and aggregate by the common reference (CommonId) all the strategy identification rules for the given entity/market/business date
        /// </summary>
        public void LoadAndAggregate()
        {
            LoadRules();

            AggregateRules();
        }

        /// <summary>
        /// Load all the strategy identification rules for the given entity/market/business date
        /// </summary>
        private void LoadRules()
        {
            if (!Initialized || DtBusiness <= DateTime.MinValue || IDM <= 0 || IDA_ENTITY <= 0)
            {
                throw new ArgumentException("The StrategyIdentificationRulesRepository object has been not correctly created nor initialized");
            }

            // EG 20160404 Migration vs2013
            //Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
            //dbParameterValues.Add("IDA_ENTITY", this.IDA_ENTITY);
            //dbParameterValues.Add("IDM", this.IDM);
            //dbParameterValues.Add("DTBUSINESS", this.DtBusiness.Date);
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDA_ENTITY", DbType.Int32), this.IDA_ENTITY);
            dp.Add(new DataParameter(CS, "IDM", DbType.Int32), this.IDM);
            dp.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), this.DtBusiness.Date);

            using (IDbConnection connection = DataHelper.OpenConnection(CS))
            {
                string sql = DataHelper<StrategyIdentificationRules>.XQueryTransform(connection, CommandType.Text, SQLREQUEST);
                sql = DataHelper<StrategyIdentificationRules>.IsNullTransform(connection, CommandType.Text, sql);
                // EG 20160404 Migration vs2013
                //Rules = DataHelper<StrategyIdentificationRules>.ExecuteDataSet(connection, CommandType.Text, sql, DataContractHelper.GetDbDataParameters(this.Parameters, dbParameterValues));
                QueryParameters qryParameters = new QueryParameters(CS, sql, dp);
                Rules = DataHelper<StrategyIdentificationRules>.ExecuteDataSet(connection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }

        }

        /// <summary>
        /// Aggregate the loaded rules by the common reference field (CommonId)
        /// </summary>
        private void AggregateRules()
        {
            if (!Initialized || this.Rules == null)
            {
                throw new ArgumentException("The Rules has been not loaded yet, call LoadRules");
            }

            // 1. Group rules by common reference (CommonId)
            IEnumerable<IGrouping<OrderReferenceEnum, StrategyIdentificationRules>> groupedRules
                = this.Rules.GroupBy(rule => rule.CommonId);

            RulesConditions =
                from groupedRule in groupedRules
                select new StrategyIdentificationRulesGroup
                {
                    CommonId = groupedRule.Key,

                    // 1.1 Excluding the desactivated elements
                    // TODO 20120612 MF maybe get a message when the rule is desactivated ?
                    Rules = groupedRule.Where(rule => DataContractHelper.GetDataContractElementEnabled(rule, this.DtBusiness.Date)),
                };
        }
    }

    /// <summary>
    /// Class representing a strategy identification rule
    /// </summary>
    /// <remarks>
    /// Related SQL request : StrategyIdentificationRulesRepository.SQLREQUEST 
    /// <seealso cref="StrategyIdentificationRulesRepository.SQLREQUEST"/>.
    /// </remarks>
    [DataContract(
        Name = DataHelper<StrategyIdentificationRules>.DATASETROWNAME,
        Namespace = DataHelper<StrategyIdentificationRules>.DATASETNAMESPACE)]
    internal class StrategyIdentificationRules : IDataContractEnabled
    {
        /// <summary>
        /// Display name of the identification rule
        /// </summary>
        [DataMember(Name = "DISPLAYNAME", Order = 1)]
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Common reference field of the identification rule, 
        /// pointing at the field will be used to aggregate Posting/Give-up  trades  and build strategy
        /// </summary>
        [DataMember(Name = "COMMONID", Order = 2)]
        public OrderReferenceEnum CommonId
        {
            get;
            set;
        }

        /// <summary>
        /// Instrument type
        /// </summary>
        /// <value>
        /// "Instr" => the related IdInstr points at an instrument; "Product" => the related IdInstr points at a product ...
        /// </value>
        [DataMember(Name = "TYPEINSTR", Order = 3)]
        public string TypeInstr
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the data whose type is related to the current TypeInstr instrument type 
        /// </summary>
        [DataMember(Name = "IDINSTR", Order = 4)]
        public int IdInstr
        {
            get;
            set;
        }

        /// <summary>
        /// Contract type element 
        /// </summary>
        /// <value>
        /// "Market" => the related IdContract points at a market; "Contract" => the related IdContract points at a derivative contract ...
        /// </value>
        [DataMember(Name = "TYPECONTRACT", Order = 5)]
        public string TypeContract
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the data whose type is related to the current TypeContract contract type element
        /// </summary>
        [DataMember(Name = "IDCONTRACT", Order = 6)]
        public int IdContract
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the derivative contract, this value is valid when TypeContract is equals to Market or Markets Group
        /// </summary>
        [DataMember(Name = "IDDC", Order = 7)]
        public int IdDC
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the intrument, this value is ever valid but when TypeInstr is different than null or None
        /// </summary>
        [DataMember(Name = "IDI", Order = 8)]
        public int IdI
        {
            get;
            set;
        }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Activation date of the identification rule
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 9)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Desactivation date of the identification rule
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 10)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Container of the identification rules associated to a specific common reference
    /// </summary>
    internal class StrategyIdentificationRulesGroup
    {
        /// <summary>
        /// Get/Set the common reference field
        /// </summary>
        public OrderReferenceEnum CommonId { get; set; }

        /// <summary>
        /// Get/Set the rule set associated with the current CommonId
        /// </summary>
        public IEnumerable<StrategyIdentificationRules> Rules { get; set; }

        /// <summary>
        /// Get when the SQL commands - related to the current rule set - have been evaluated, 
        /// if not this rule may not be used for decorate the main trade request, in this case please call EvaluateCommonId on this instance 
        /// to proceed to the SQL evaluation.
        /// </summary>
        public bool Evaluated { get; private set; }

        /// <summary>
        /// Get the SQL field of the common reference
        /// </summary>
        public string SQLCommonId
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the SQL where condition related to the current common reference
        /// </summary>
        public string SQLWhereCommonId
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the sql additional joint block related to the current common reference
        /// </summary>
        public string SQLJointCommonId
        {
            get;
            private set;
        }

        /// <summary>
        /// Evaluate the SQL fields related to this rules-set, these SQL commands are used to load the trades Posting/GiveUp according with 
        /// the chosen reference value (OrderId, ExtlLink, etc...) 
        /// </summary>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void EvaluateSQLCommonId()
        {
            Evaluated = true;

            SQLJointCommonId = String.Empty;

            switch (this.CommonId)
            {
                case OrderReferenceEnum.ClOrderID:

                    SQLCommonId =
                        "trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@ClOrdID)[1]','UT_ENUM_OPTIONAL')";
                    SQLWhereCommonId =
                        "and trx.TRADEXML.exist('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@ClOrdID)') = 1";

                    break;

                case OrderReferenceEnum.ExtlLink:

                    SQLCommonId = "t.EXTLLINK";
                    SQLWhereCommonId = "and (t.EXTLLINK is not null)";

                    break;

                case OrderReferenceEnum.FolderId:

                    SQLCommonId = "lid.LINKID";
                    SQLWhereCommonId = "and (lid.LINKID is not null)";
                    SQLJointCommonId =
                        String.Format(@"left outer join dbo.LINKID lid on lid.IDT = t.IDT and lid.IDA = t.IDA_DEALER and lid.linkidscheme = '{0}'",
                        Cst.OTCml_FolderIdScheme);

                    break;

                case OrderReferenceEnum.FrontId:

                    SQLCommonId = "tid.TRADEID";
                    SQLWhereCommonId = "and (tid.TRADEID is not null)";
                    SQLJointCommonId =
                        String.Format(@"left outer join dbo.TRADEID tid on tid.IDT = t.IDT and tid.IDA = t.IDA_DEALER and tid.tradeidscheme = '{0}'",
                        Cst.OTCml_FrontTradeIdScheme);

                    break;

                case OrderReferenceEnum.OrderID:
                    // FI 20210805 [XXXXX] alias t
                    SQLCommonId = "t.ORDERID";
                    SQLWhereCommonId = "and (t.ORDERID is not null)";

                    break;

                case OrderReferenceEnum.SecondaryClOrderID:

                    SQLCommonId =
                        "trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@ClOrdID2)[1]','UT_ENUM_OPTIONAL')";
                    SQLWhereCommonId =
                        "and trx.TRADEXML.exist('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@ClOrdID2)') = 1";

                    break;

                case OrderReferenceEnum.SecondaryOrderID:

                    SQLCommonId =
                       "trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@OrdID2)[1]','UT_ENUM_OPTIONAL')";
                    SQLWhereCommonId =
                        "and trx.TRADEXML.exist('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:RptSide/@OrdID2)') = 1";

                    break;

                default:

                    Evaluated = false;
                    break;
            }
        }

        /// <summary>
        /// Check if the input target is compliant with at least one of the current rules
        /// </summary>
        /// <param name="pTargetTrade">input target, NOT null</param>
        /// <returns>true when the input target is compliant with at least one of the rules. In this case the trade will be processed,
        /// otherwise the trade will be excluded from the strategy identification process</returns>
        public bool IsTradeAllowed(TradeXStrategy pTargetTrade)
        {
            if (pTargetTrade == null || this.Rules == null)
            {
                throw new ArgumentNullException
                    ("The target trade or the internal rules reference are null");
            }

            bool allowed = false;

            foreach (StrategyIdentificationRules rule in this.Rules)
            {
                bool allowedInstr = false;

                bool allowedContract = false;

                switch (rule.TypeInstr)
                {
                    case "GrpProduct":
                    case "Product":
                    case "GrpInstr":
                    case "Instr":

                        allowedInstr = rule.IdI == pTargetTrade.IdI;

                        break;

                    case "None":

                        allowedInstr = false;

                        break;

                    default:

                        allowedInstr = true;

                        break;
                }

                switch (rule.TypeContract)
                {
                    case "GrpMarket":
                    case "Market":

                        // 20120613 MF Actually the market and the market group rules are treated when the rules are loaded. 
                        //             Actually we load only the strategy identification instructions related to the market currently 
                        //             treated by the EOD process. Then we force "true"

                        allowedContract = true;

                        break;

                    case "GrpContract":
                    case "Contract":

                        allowedContract = rule.IdDC == pTargetTrade.IDDC;

                        break;

                    case "None":


                        allowedContract = false;

                        break;

                    default:

                        allowedContract = true;

                        break;
                }

                if (rule.TypeInstr != "None" && rule.TypeContract != "None")
                {
                    // The instrumental environment rule and the market / product environment rule are joint in logical AND ever (exception for 
                    // the special value "None")
                    allowed = allowedInstr && allowedContract;
                }
                else
                {
                    // None is the only case where the two conditions instrumental environment and market / product are joint in a logical OR
                    allowed = allowedInstr || allowedContract;
                }

                // At the first rule the trade is compliant with, we break the loop and we return the OK status of the trade
                if (allowed) break;
            }

            return allowed;
        }
    }

    /// <summary>
    /// Class representing a trade including an Order Id
    /// </summary>
    /// <remarks>
    /// A trade provided with an Order ID could be linked to a strategy.
    /// Related SQL request : TradeXStrategyRepository.SQLREQUEST <seealso cref="TradeXStrategyRepository.SQLREQUEST"/>.
    /// </remarks>
    [DataContract(
        Name = DataHelper<TradeXStrategy>.DATASETROWNAME,
        Namespace = DataHelper<TradeXStrategy>.DATASETNAMESPACE)]
    internal class TradeXStrategy
    {

        /// <summary>
        /// internal id of the trade
        /// </summary>
        [DataMember(Name = "IDT", Order = 1)]
        public int IDT
        {
            get;
            set;
        }

        /// <summary>
        /// Trade identifier
        /// </summary>
        [DataMember(Name = "TRADEIDENTIFIER", Order = 2)]
        public string TradeIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Trade quantity
        /// </summary>
        [DataMember(Name = "QTY", Order = 3)]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Trade Order Id. It is the strategy identifier; Any trade sharing this Id will be grouped in the same strategy
        /// </summary>
        [DataMember(Name = "ORDID", Order = 4)]
        public string OrdId
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the book
        /// </summary>
        [DataMember(Name = "IDB", Order = 5)]
        public int IDB
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of the book where the trade is registered
        /// </summary>
        [DataMember(Name = "BOOKIDENTIFIER", Order = 6)]
        public string BookIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Side of the trade dealer
        /// </summary>
        /// <value>Buyer|Seller</value>
        [DataMember(Name = "SIDE", Order = 7)]
        public string Side
        {
            get;
            set;
        }

        /// <summary>
        /// internal id of the market
        /// </summary>
        [DataMember(Name = "IDM", Order = 8)]
        public int IDM
        {
            get;
            set;
        }

        /// <summary>
        /// market ISO acronym  
        /// </summary>
        [DataMember(Name = "MARKET", Order = 9)]
        public string Market
        {
            get;
            set;
        }

        /// <summary>
        /// Asset contract category
        /// </summary>
        /// <value>O:Option|F:Future</value>
        [DataMember(Name = "CATEGORY", Order = 10)]
        public string Category
        {
            get;
            set;
        }

        /// <summary>
        /// internal id of the asset derivative contract
        /// </summary>
        [DataMember(Name = "IDDC", Order = 11)]
        public int IDDC
        {
            get;
            set;
        }

        [DataMember(Name = "CONTRACTIDENTIFIER", Order = 12)]
        public string ContractIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Maturity code, formatted like as YYYYMM
        /// </summary>
        [DataMember(Name = "MATURITY", Order = 13)]
        public string Maturity
        {
            get;
            set;
        }

        /// <summary>
        /// For Option asset, put or call indicator
        /// </summary>
        /// <value>0:Put|1:Call, for Futures asset it will be 0</value>
        [DataMember(Name = "PUTCALL", Order = 14)]
        public int PutCall
        {
            get;
            set;
        }

        /// <summary>
        /// Trade asset strike price
        /// </summary>
        [DataMember(Name = "STRIKEPRICE", Order = 15)]
        public decimal StrikePrice
        {
            get;
            set;
        }

        /// <summary>
        /// Asset category/family
        /// </summary>
        /// <value>Future|Option</value>
        [DataMember(Name = "ASSETCATEGORY", Order = 16)]
        public string AssetCategory
        {
            get;
            set;
        }

        /// <summary>
        /// internal id of the trade asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 18)]
        public int IdAsset
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the instrument
        /// </summary>
        [DataMember(Name = "IDI", Order = 19)]
        public int IdI
        {
            get;
            set;
        }

        /// <summary>
        /// When not null the trade is part of a strategy instrument
        /// </summary>
        [DataMember(Name = "MLEGRPTTYP", Order = 20)]
        public string MLegRptTyp
        {
            get;
            set;
        }

        /// <summary>
        /// Trade leg number
        /// </summary>
        [DataMember(Name = "LEGNO", Order = 21)]
        public int LegNo
        {
            get;
            set;
        }

        /// <summary>
        /// Trade strategy type
        /// </summary>
        [DataMember(Name = "SECSUBTYP", Order = 22)]
        public string SecSubTyp
        {
            get;
            set;
        }

        /// <summary>
        /// Trade "to update" indicator. 
        /// True when this trade making part of a strategy should be changed. 
        /// Trade member could be updated: MLegRptTyp, LegNo, SecSubTyp.
        /// </summary>
        public bool ToUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// Update indicator related to the MLegRptTyp member. 
        /// True when the trade is entering a strategy.
        /// </summary>
        public bool UpdateMLegRptTyp
        {
            get;
            set;
        }

        /// <summary>
        /// Update indicator related to the LegNo member. 
        /// True when the trade is entering a strategy or it changes leg
        /// </summary>
        public bool UpdateLegNo
        {
            get;
            set;
        }

        /// <summary>
        /// Update indicator related to the SecSubTyp member. 
        /// True when the trade is entering a strategy or it changes type of strategy.
        /// </summary>
        public bool UpdateSecSubTyp
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Leg of a strategy
    /// </summary>
    internal class StrategyLeg
    {
        /// <summary>
        /// Trades collection related to the leg
        /// </summary>
        public TradeXStrategy[] Trades { get; set; }

        /// <summary>
        /// Asset id of the asset related to the leg
        /// </summary>
        public int IdAsset
        {
            get;
            set;
        }

        /// <summary>
        /// Contract identifier of the asset related to the leg
        /// </summary>
        public string Category
        {
            get;
            set;
        }

        /// <summary>
        /// maturity code of the asset related to the leg
        /// </summary>
        public string Maturity
        {
            get;
            set;
        }

        /// <summary>
        /// PutCall code of the asset related to the leg. Null for futures
        /// </summary>
        public int PutCall
        {
            get;
            set;
        }

        /// <summary>
        /// Strike price code of the asset related to the leg. 0 futures
        /// </summary>
        public decimal StrikePrice
        {
            get;
            set;
        }

        /// <summary>
        /// Signed net quantity on the leg. negative for short net quantity, positive otherwise
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Leg number of the leg
        /// </summary>
        public int LegNo
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Strategy
    /// </summary>
    internal class Strategy
    {
        /// <summary>
        /// Trades collection related to the strategy (all legs included)
        /// </summary>
        public IEnumerable<TradeXStrategy> Trades { get; set; }

        /// <summary>
        /// Legs collection related to the strategy
        /// </summary>
        public StrategyLeg[] Legs { get; set; }

        /// <summary>
        /// Order id identifying all the trades composing the strategy
        /// </summary>
        public string OrdId
        {
            get;
            set;
        }

        /// <summary>
        /// Flag identifying when the object has been recognized as a strategy.
        /// </summary>
        /// <value>True when the numbers of leg is major than 1, false otherwise</value>
        public bool IsRealStrategy
        {
            get;
            set;
        }

        /// <summary>
        /// Strategy code
        /// </summary>
        /// <value>"2" when IsRealStrategy is true, null otherwise</value>
        public string MLegRptTyp
        {
            get;
            set;
        }

        /// <summary>
        /// Strategy name
        /// </summary>
        /// <value>value issued from the StrategyEnumRepository when IsRealStrategy is true, null otherwise</value>
        public string SecSubTyp
        {
            get;
            set;
        }

        /// <summary>
        /// Set the strategy datas: SecSubTyp and the LegNo member for all the legs making part of the strategy
        /// </summary>
        /// <param name="pRep"></param>
        internal void SetStrategyDatas(StrategyEnumRepository pRep)
        {
            if (!IsRealStrategy || Trades == null || Legs == null || Legs.Length == 0)
            {
                throw new ArgumentException("The AllocsByOrderId object has been not correctly initialized");
            }

            SetSecSubTyp(pRep);

            SetLegNo();

        }

        private void SetLegNo()
        {
            for (int leg = 0; leg < Legs.Length; leg++)
            {
                Legs[leg].LegNo = leg + 1;
            }
        }

        private void SetSecSubTyp(StrategyEnumRepository pRep)
        {
            SecSubTyp = String.Format(pRep.GetValue(StrategyEnumRepository.StrategyEnum.Custom), Legs.Length);

            bool isOption = IsAnyLegOption();

            bool isQuantityOnLegsCompliant =
                Legs.GroupBy(elem => Math.Abs(elem.Quantity)).Count() == 1
                // maybe Butterfly 
                || (Legs.Count() == 3
                        && Legs.GroupBy(elem => Math.Abs(elem.Quantity)).Count() == 2)
                // maybe spread ratio
                || (Legs.Count() == 2
                        && Legs.GroupBy(elem => Math.Abs(elem.Quantity)).Count() == 2);

            if (isQuantityOnLegsCompliant && isOption)
            {
                switch (Legs.Length)
                {
                    case 2:

                        SetSecSubTypBy2Legs(pRep);

                        break;

                    case 3:

                        SetSecSubTypBy3Legs(pRep);

                        break;

                    case 4:

                        SetSecSubTypBy4Legs(pRep);

                        break;

                    default:

                        break;
                }
            }

        }

        // MF 20120925 - Ticket 18148 - private to public in order to use this method outside the class
        /// <summary>
        /// Verify all the trades being on option contracts
        /// </summary>
        /// <returns>True when all the trades on all the legs are option contracts</returns>
        public bool IsAnyLegOption()
        {
            bool isOption = false;

            IEnumerable<IGrouping<string, StrategyLeg>> groupedLegsByCat = Legs.GroupBy(elem => elem.Category);

            if (groupedLegsByCat.Count() == 1 && groupedLegsByCat.First().Key == "O")
            {
                isOption = true;
            }
            return isOption;
        }

        private void SetSecSubTypBy4Legs(StrategyEnumRepository pRep)
        {
            int howManyStrike = this.Legs.GroupBy(elem => elem.StrikePrice).Count();

            if (this.Legs.GroupBy(elem => elem.Maturity).Count() == 1
                && this.Legs.GroupBy(elem => elem.PutCall).Count() == 1
                && howManyStrike >= 3)
            {
                StrategyLeg legMaxStrike = this.Legs.Aggregate((curr, next) => next != null &&
                    curr.StrikePrice > next.StrikePrice ? curr : next);

                StrategyLeg legMinStrike = this.Legs.Aggregate((curr, next) => next != null &&
                    curr.StrikePrice < next.StrikePrice ? curr : next);

                StrategyLeg[] legIntStrikes = this.Legs.Except(new StrategyLeg[] { legMaxStrike, legMinStrike }).ToArray();

                if (Math.Sign(legMaxStrike.Quantity) == Math.Sign(legMinStrike.Quantity)
                    && Math.Sign(legIntStrikes[0].Quantity) == Math.Sign(legIntStrikes[1].Quantity)
                    && Math.Sign(legMaxStrike.Quantity) != Math.Sign(legIntStrikes[0].Quantity))
                {
                    SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Condor);
                }

            }

        }

        private void SetSecSubTypBy3Legs(StrategyEnumRepository pRep)
        {
            int howManyStrike = this.Legs.GroupBy(elem => elem.StrikePrice).Count();

            // 1. Same maturity
            if (this.Legs.GroupBy(elem => elem.Maturity).Count() == 1)
            {
                // 1.1. Same put or call indicator and 3 different strikes
                if (this.Legs.GroupBy(elem => elem.PutCall).Count() == 1 && howManyStrike == 3)
                {
                    StrategyLeg legMaxStrike = this.Legs.Aggregate((curr, next) => next != null &&
                        curr.StrikePrice > next.StrikePrice ? curr : next);

                    StrategyLeg legMinStrike = this.Legs.Aggregate((curr, next) => next != null &&
                        curr.StrikePrice < next.StrikePrice ? curr : next);

                    StrategyLeg legIntStrike = this.Legs.Except(new StrategyLeg[] { legMaxStrike, legMinStrike }).First();

                    // 1.1.1. same side for the legs with max and min strikes, and double opposite direction for the intermediary leg 
                    if
                        (
                            Math.Sign(legMaxStrike.Quantity) == Math.Sign(legMinStrike.Quantity)
                            && Math.Sign(legMaxStrike.Quantity) != Math.Sign(legIntStrike.Quantity)
                            && (2 * Math.Abs(legMaxStrike.Quantity)) == Math.Abs(legIntStrike.Quantity)
                        )
                    {
                        SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Butterfly);
                    }
                    // 1.1.2. 
                    // same side for the legs with max and intermediary strikes, opposite direction for the minimum strike leg 
                    // or
                    // same side for the legs with min and intermediary strikes, opposite direction for the maximal strike leg 
                    else if
                        (
                            (
                                Math.Sign(legMaxStrike.Quantity) == Math.Sign(legIntStrike.Quantity)
                                && Math.Sign(legMaxStrike.Quantity) != Math.Sign(legMinStrike.Quantity)
                            )
                            ||
                            (
                                Math.Sign(legMinStrike.Quantity) == Math.Sign(legIntStrike.Quantity)
                                && Math.Sign(legMinStrike.Quantity) != Math.Sign(legMaxStrike.Quantity)
                            )
                        )
                    {
                        SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Ladder);
                    }
                }
                // 1.2. Different put or call indicator (2 puts and 1 call or viceversa)
                else if (this.Legs.GroupBy(elem => elem.PutCall).Count() == 2)
                {

                    int howManyPutCallAndSides =
                        this.Legs.GroupBy(elem => new { elem.PutCall, Dir = Math.Sign(elem.Quantity) }).Count();

                    // 1.2.1. the double put or call assets have different sides, then we must have 3 groups, one group per leg
                    if (howManyPutCallAndSides == 3)
                    {
                        SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.SpreadVersus);
                    }
                }
            }
        }

        private void SetSecSubTypBy2Legs(StrategyEnumRepository pRep)
        {
            // 1. Same maturity 
            if (this.Legs.GroupBy(elem => elem.Maturity).Count() == 1)
            {
                // 1.1 Just one option type (two calls or two puts)
                if (this.Legs.GroupBy(elem => elem.PutCall).Count() == 1)
                {
                    // The two legs have same maturities, same put or call indicators, the strike prices must be different.
                    // There is one max and one min strike prices.

                    StrategyLeg legMaxStrike = this.Legs.Aggregate((curr, next) => next != null &&
                                curr.StrikePrice > next.StrikePrice ? curr : next);

                    StrategyLeg legMinStrike = this.Legs.Aggregate((curr, next) => next != null &&
                                curr.StrikePrice < next.StrikePrice ? curr : next);

                    // 1.1.1 Different leg sides
                    if (Math.Sign(legMaxStrike.Quantity) != Math.Sign(legMinStrike.Quantity))
                    {
                        // 1.1.1.1 same quantity on any leg
                        if (Math.Abs(legMaxStrike.Quantity) == Math.Abs(legMinStrike.Quantity))
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Spread);
                        }
                        // 1.1.1.2 two Puts, and min strike = 2 * max strike
                        else if (legMaxStrike.PutCall == 0
                            && ((2 * Math.Abs(legMaxStrike.Quantity)) == Math.Abs(legMinStrike.Quantity)))
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.RatioSpread);
                        }
                        // 1.1.1.3 two Calls, and max strike = 2 * min strike
                        else if (legMaxStrike.PutCall == 1
                            && ((2 * Math.Abs(legMinStrike.Quantity)) == Math.Abs(legMaxStrike.Quantity)))
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.RatioSpread);
                        }
                    }

                }
                // 1.2 more than one option type (one call and one put) AND same quantity
                else if (Legs.GroupBy(elem => Math.Abs(elem.Quantity)).Count() == 1)
                {
                    StrategyLeg legPut = this.Legs.Where(elem => elem.PutCall == 0).First();

                    StrategyLeg legCall = this.Legs.Where(elem => elem.PutCall == 1).First();

                    // 1.2.1 Same strike
                    if (this.Legs.GroupBy(elem => elem.StrikePrice).Count() == 1)
                    {
                        // 1.2.1.1 same side
                        if (Math.Sign(legPut.Quantity) == Math.Sign(legCall.Quantity))
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Straddle);
                        }
                    }
                    // 1.2.2 different strikes
                    else
                    {
                        // 1.2.2.1 Same side, short direction
                        if (legPut.Quantity < 0 && legCall.Quantity < 0)
                        {
                            if (legPut.StrikePrice < legCall.StrikePrice)
                            {
                                SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Strangle);
                            }

                            if (legPut.StrikePrice > legCall.StrikePrice)
                            {
                                SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Guts);
                            }
                        }
                        // // 1.2.2.2 Different sides
                        else if (Math.Sign(legPut.Quantity) != Math.Sign(legCall.Quantity))
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.Combo);
                        }

                    }
                }
            }
            // 2. Different maturities
            else
            {
                // 2.1 Just one option type (two calls or two puts)
                if (this.Legs.GroupBy(elem => elem.PutCall).Count() == 1)
                {
                    StrategyLeg leg1 = this.Legs[0];

                    StrategyLeg leg2 = this.Legs[1];

                    // 2.1.1 Different sides
                    if (Math.Sign(leg1.Quantity) != Math.Sign(leg2.Quantity))
                    {
                        // 2.1.1.1 Same strike
                        if (this.Legs.GroupBy(elem => elem.StrikePrice).Count() == 1)
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.CalendarSpread);
                        }
                        // 2.1.1.2 Different strikes
                        else
                        {
                            SecSubTyp = pRep.GetValue(StrategyEnumRepository.StrategyEnum.DiagonalCalendarSpread);
                        }
                    }
                }
            }

        }

        internal void SetElementsToUpdate()
        {
            if (!IsRealStrategy || Trades == null || Legs == null || Legs.Length == 0)
            {
                throw new ArgumentException("The AllocsByOrderId object has been not correctly initialized");
            }

            for (int leg = 0; leg < Legs.Length; leg++)
            {
                StrategyLeg legObj = Legs[leg];

                for (int tradeIdx = 0; tradeIdx < legObj.Trades.Length; tradeIdx++)
                {
                    TradeXStrategy trade = legObj.Trades[tradeIdx];

                    bool notAStrategy = trade.MLegRptTyp == null || trade.MLegRptTyp != this.MLegRptTyp;

                    bool differentType = trade.SecSubTyp == null || trade.SecSubTyp != this.SecSubTyp;

                    bool differentLeg = trade.LegNo == 0 || trade.LegNo != legObj.LegNo;

                    trade.ToUpdate = notAStrategy || differentType || differentLeg;

                    if (trade.ToUpdate)
                    {
                        if (notAStrategy)
                        {
                            trade.MLegRptTyp = this.MLegRptTyp;
                            trade.UpdateMLegRptTyp = true;
                        }

                        if (differentType)
                        {
                            trade.SecSubTyp = this.SecSubTyp;
                            trade.UpdateSecSubTyp = true;
                        }

                        if (differentLeg)
                        {
                            trade.LegNo = legObj.LegNo;
                            trade.UpdateLegNo = true;
                        }
                    }

                }

            }
        }
    }

    /// <summary>
    /// Repository of the trades (for a given entity/market/business date) including an Order Id
    /// </summary>
    internal class TradeXStrategyRepository : GenericStrategyRepository
    {

        /// <summary>
        /// Get all the loaded trades 
        /// </summary>
        public List<TradeXStrategy> Trades { get; private set; }

        /// <summary>
        /// Get all the trades Posting/GiveUp marke as strategy but not being strategy anymore
        /// </summary>
        public List<TradeXStrategy> ExStrategies { get; private set; }

        /// <summary>
        /// Get all the built strategies related to the Trades collection
        /// </summary>
        /// <remarks>
        /// The returned request MUST be submitted to some customizations:
        /// <para>{0} will be replaced with the current reference value passed 
        /// as input by the processed rule group (<seealso cref="StrategyIdentificationRulesGroup"/>)</para>
        /// <para>{1} will be replaced with the current reference WHERE condition passed 
        /// as input by the processed rule group (<seealso cref="StrategyIdentificationRulesGroup"/>)</para>
        /// <para>{2} will be replaced with the additional joint block passed 
        /// as input by the processed rule group (<seealso cref="StrategyIdentificationRulesGroup"/>). Any
        /// additional joint block is placed after the TRADE/TRADEACTOR/BOOK joints, then any other next table can not be exploited 
        /// in the additional joint block.
        /// </para>
        /// </remarks>
        public Strategy[] Strategies { get; private set; }
        /// EG 20150306 TRDTYPE (string)
        /// FI 20180131 [XXXXX] Modify
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override string SQLREQUEST
        {
            get
            {
                //// FI 20180131 [XXXXX] 27 enclosed by '
                //return @"select t.IDT, t.IDENTIFIER as TRADEIDENTIFIER, ti.QTY, 
                //{0} /*t.ORDERID*/ as ORDID,
                //b.IDB, b.IDENTIFIER as BOOKIDENTIFIER,
                //ta.BUYER_SELLER SIDE,
                //m.IDM as IDM, m.SHORT_ACRONYM as MARKETSHORT_ACRONYM,
                //v_etd.CATEGORY as CATEGORY, v_etd.IDDC, v_etd.CONTRACTIDENTIFIER as CONTRACTIDENTIFIER, v_etd.MATFMT_MMMYY as MATURITY, 
                //v_etd.PUTCALL, v_etd.STRIKEPRICE, v_etd.ASSETCATEGORY, v_etd.DISPLAYNAME as ASSETDISPLAYNAME, v_etd.IDASSET,
                //ti.IDI,
                //trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@MLegRptTyp)[1]','UT_ENUM_OPTIONAL') as MLEGRPTTYP,
                //trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg/@LegNo)[1]','UT_ENUM_OPTIONAL') as LEGNO,
                //trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg/fixml:Leg/@SecSubTyp)[1]','UT_ENUM_OPTIONAL') as SECSUBTYP
                //from dbo.TRADE t
                //inner join dbo.TRADE trx on (trx.IDT = t.IDT)
                //inner join dbo.TRADEACTOR ta on ta.IDT = t.IDT and ta.FIXPARTYROLE = '27'
                //inner join dbo.BOOK b on b.IDB = ta.IDB  and b.IDA_ENTITY = @IDA_ENTITY
                //{2}
                //inner join dbo.VW_ALLTRADEINSTRUMENT ti on (ti.IDT = t.IDT) {1} /*and ti.ORDERID is not null*/
                //inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = ti.IDM) and (m.IDM = @IDM)
                //inner join dbo.VW_ASSET_ETD_EXPANDED v_etd on (v_etd.IDASSET = ti.IDASSET)
                //where (t.DTBUSINESS = @DTBUSINESS) and (t.IDSTBUSINESS = 'ALLOC') and (ti.TRDTYPE not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + ")";


                // FI 20210805 [XXXX] Reécriture de la requête
                return @"select t.IDT, t.IDENTIFIER as TRADEIDENTIFIER, t.QTY, 
                {0} /*t.ORDERID*/ as ORDID,
                b.IDB, b.IDENTIFIER as BOOKIDENTIFIER,
                case  t.SIDE
                    when '1' then 'Buyer'
                    when '2' then 'Seller'
                end as SIDE,                      
                m.IDM as IDM, m.SHORT_ACRONYM as MARKETSHORT_ACRONYM,
                v_etd.CATEGORY as CATEGORY, v_etd.IDDC, v_etd.CONTRACTIDENTIFIER as CONTRACTIDENTIFIER, v_etd.MATFMT_MMMYY as MATURITY, 
                v_etd.PUTCALL, v_etd.STRIKEPRICE, v_etd.ASSETCATEGORY, v_etd.DISPLAYNAME as ASSETDISPLAYNAME, v_etd.IDASSET,
                t.IDI,
                trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@MLegRptTyp)[1]','UT_ENUM_OPTIONAL') as MLEGRPTTYP,
                trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg/@LegNo)[1]','UT_ENUM_OPTIONAL') as LEGNO,
                trx.TRADEXML.value('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg/fixml:Leg/@SecSubTyp)[1]','UT_ENUM_OPTIONAL') as SECSUBTYP
                from dbo.TRADE t
                inner join dbo.TRADEXML trx on (trx.IDT = t.IDT)
                inner join dbo.BOOK b on b.IDB = t.IDA_DEALER
                {2}
                inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = t.IDM) and (m.IDM = @IDM)
                inner join dbo.VW_ASSET_ETD_EXPANDED v_etd on (v_etd.IDASSET = t.IDASSET)
                where (t.IDA_ENTITY=@IDA_ENTITY) and (t.DTBUSINESS = @DTBUSINESS) and (t.IDSTBUSINESS = 'ALLOC') 
                and (t.TRDTYPE not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + ")) {1} /*and t.ORDERID is not null*/";
            }
        }

        IEnumerable<StrategyIdentificationRulesGroup> RulesConditions { get; set; }

        /// <summary>
        /// Create a new instance of the repository
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        /// <param name="pDtBusiness">current business date, NOT empty</param>
        /// <param name="pIDM">market id, major than 0</param>
        /// <param name="pIDA_ENTITY">entity id, major than 0</param>
        public TradeXStrategyRepository
            (string pCS, DateTime pDtBusiness, int pIDM, int pIDA_ENTITY, IEnumerable<StrategyIdentificationRulesGroup> pRulesConditions)
            : base(pCS, pDtBusiness, pIDM, pIDA_ENTITY)
        {
            this.RulesConditions = pRulesConditions;
        }

        /// <summary>
        /// Load all the trades for the given entity/market/business date including an OrderId. 
        /// If the trades contain some strategy informations then these informations will be loaded too. 
        /// The loaded trades must be compliant with the instruction rule set given for allo strategies 
        /// (<seealso cref="TradeXStrategyRepository.RulesConditions"/>). 
        /// </summary>
        /// <remarks>After this call the Trades and the ExStrategies collections will be available.</remarks>
        // EG 20160404 Migration vs2013
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void LoadTrades()
        {
            if (!Initialized || DtBusiness <= DateTime.MinValue || IDM <= 0 || IDA_ENTITY <= 0)
            {
                throw new ArgumentException("The TradeXStrategyRepository object has been not correctly created nor initialized");
            }

            // When no conditions at all are found, no trades will be loaded
            if (this.RulesConditions == null || this.RulesConditions.Count() <= 0)
            {
                Trades = new List<TradeXStrategy>();
                ExStrategies = new List<TradeXStrategy>();
                return;
            }

            // EG 20160404 Migration vs2013
            //Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
            //dbParameterValues.Add("IDA_ENTITY", this.IDA_ENTITY);
            //dbParameterValues.Add("IDM", this.IDM);
            //dbParameterValues.Add("DTBUSINESS", this.DtBusiness.Date);
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDA_ENTITY", DbType.Int32), this.IDA_ENTITY);
            dp.Add(new DataParameter(CS, "IDM", DbType.Int32), this.IDM);
            dp.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), this.DtBusiness.Date);

            using (IDbConnection connection = DataHelper.OpenConnection(CS))
            {
                // 20120612 MF Ticket 17886
                // Load the trades taking care of all the loaded conditions (grouped by the common reference)...
                // Remarks: this process can load a trade twice than once, it depends of how many common reference a trade got validated, 
                //          and how many instructions apply on this trade. 
                foreach (StrategyIdentificationRulesGroup ruleCondition in this.RulesConditions)
                {
                    if (!ruleCondition.Evaluated)
                    {
                        ruleCondition.EvaluateSQLCommonId();
                    }

                    string sql = String.Format(SQLREQUEST, ruleCondition.SQLCommonId, ruleCondition.SQLWhereCommonId, ruleCondition.SQLJointCommonId);

                    sql = DataHelper<TradeXStrategy>.XQueryTransform(connection, CommandType.Text, sql);

                    // EG 20160404 Migration vs2013
                    //List<TradeXStrategy> tempTrades = DataHelper<TradeXStrategy>.ExecuteDataSet(
                    //    connection, CommandType.Text, sql, DataContractHelper.GetDbDataParameters(this.Parameters, dbParameterValues));
                    QueryParameters qryParameters = new QueryParameters(CS, sql, dp);
                    List<TradeXStrategy> tempTrades = DataHelper<TradeXStrategy>.ExecuteDataSet(
                        connection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    // 20120612 MF Ticket 17886 
                    tempTrades = tempTrades
                        // filter the trades that are not compliant with the given instruction rule set
                        .Where(trade => ruleCondition.IsTradeAllowed(trade))
                        // returning one trade per IDT 
                        // (in case of the main SQL request will be enriched and tome trades will be returned more than once)
                        .Distinct((trade1, trade2) => trade1.IDT == trade2.IDT).ToList()
                        .ToList();

                    // add the trade collection charged for the current rule to the main trade collection
                    Trades = Trades == null ? tempTrades : Trades.Union(tempTrades).ToList();
                }

                // 20120615 MF Ticket 17882 reprise
                // Retrieve all the trades Posting/GiveUp already marked as Strategy but taht will not process, because of they
                // no have no more orders id, either the rule/instruction associate to them has been deleted/desactivated 
                // EG 20160404 Migration vs2013
                //LoadExStrategies(dbParameterValues, connection);
                LoadExStrategies(connection, dp);
            }

        }

        // 20120615 MF Ticket 17882 reprise
        /// <summary>
        /// Retrieve all the trades Posting/GiveUp already marked as Strategy but taht will not process, because of they
        /// no have no more orders id, either the rule/instruction associate to them has been deleted/desactivated 
        /// </summary>
        /// <param name="pDbParameterValues">current db parameters</param>
        /// <param name="pConnection">current connection</param>
        // EG 20160404 Migration vs2013
        //private void LoadExStrategies(Dictionary<string, object> pDbParameterValues, IDbConnection pConnection)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void LoadExStrategies(IDbConnection pConnection, DataParameters pDataParameters)
        {
            string sql = String.Format(SQLREQUEST,
                DataHelper.SQLString(Cst.None),
                "and trx.TRADEXML.exist('(/efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt[@MLegRptTyp=\"2\"])') = 1",
                String.Empty);

            sql = DataHelper<TradeXStrategy>.XQueryTransform(pConnection, CommandType.Text, sql);
            // EG 20160404 Migration vs2013
            QueryParameters qryParameters = new QueryParameters(CS, sql, pDataParameters);
            // 1. load all the trades posting/GiveUp already marked as strategy, without no care for their order id
            //ExStrategies = DataHelper<TradeXStrategy>.ExecuteDataSet(
            //        pConnection, CommandType.Text, sql, DataContractHelper.GetDbDataParameters(this.Parameters, pDbParameterValues));
            ExStrategies = DataHelper<TradeXStrategy>.ExecuteDataSet(
                    pConnection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            // 2. filter the previous element excludinf all the ones will be processed, in other words excluding all the trades already
            //    inserted in the main Trades collection used to identify strategies
            ExStrategies = ExStrategies.Except(Trades, (elem1, elem2) => elem1.IDT == elem2.IDT).ToList();
        }

        /// <summary>
        /// Build the strategies collection on the loaded Trades collection. After this the Strategies collection will be available
        /// </summary>
        public void BuildStrategies()
        {
            if (!Initialized || Trades == null)
            {
                throw new ArgumentException("The AllocStrategyCollection object has been not initialized not the BuildAllocs has been performed");
            }

            Strategies = GroupByOrderId(this.Trades);

            foreach (Strategy strategy in Strategies)
            {
                GroupByAsset(strategy);
            }
        }

        private Strategy[] GroupByOrderId(IEnumerable<TradeXStrategy> pTrades)
        {

            IEnumerable<Strategy> strategies =
              from groupedAllocs in pTrades.GroupBy(elem => elem.OrdId)
              select new Strategy
              {
                  OrdId = groupedAllocs.Key,

                  Trades = groupedAllocs.Select(elem => elem),

              };

            return strategies.ToArray();

        }

        private void GroupByAsset(Strategy pStrategy)
        {

            StrategyLeg[] legs =

              (from groupedTrades in pStrategy.Trades.GroupBy(
                   elem => new { elem.Category, elem.Maturity, elem.StrikePrice, elem.PutCall, elem.IdAsset })
               select new StrategyLeg
               {
                   Category = groupedTrades.Key.Category,

                   Maturity = groupedTrades.Key.Maturity,

                   StrikePrice = groupedTrades.Key.StrikePrice,

                   PutCall = groupedTrades.Key.PutCall,

                   IdAsset = groupedTrades.Key.IdAsset,

                   Trades = groupedTrades.Select(elem => elem).ToArray(),

                   Quantity = groupedTrades.Sum(elem => (elem.Side == "Seller") ? elem.Quantity : -1 * elem.Quantity)

               }

               )

               .OrderByDescending(elem => elem.PutCall)
               .OrderBy(elem => elem.StrikePrice)
               .OrderBy(elem => elem.Maturity)
               // first Option O
               .OrderByDescending(elem => elem.Category)

               .ToArray();

            pStrategy.Legs = legs;

        }
    }

    // 20120619 Ticket 17908
    /// <summary>
    /// A class containing all the additional data related to a single trade, in order to get all the infos needed by the
    /// fees calculation process
    /// </summary>
    [DataContract(
        Name = DataHelper<TradeXFees>.DATASETROWNAME,
        Namespace = DataHelper<TradeXFees>.DATASETNAMESPACE)]
    internal class TradeXFees
    {
        /// <summary>
        /// Internal id of the trade linked to the retrieven additional datas
        /// </summary>
        [DataMember(Name = "IDT", Order = 1)]
        public int IDT
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the first event connected to the current trade
        /// </summary>
        [DataMember(Name = "IDE_EVENT", Order = 2)]
        public int IDEvent
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of an actor participating to the current trade
        /// </summary>
        [DataMember(Name = "IDA", Order = 3)]
        public int IdA
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of an actor participating to the current trade
        /// </summary>
        [DataMember(Name = "ACTOR_IDENTIFIER", Order = 4)]
        public string ActorIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the book of the current trade actor. It is the book where the trade is registered.
        /// </summary>
        [DataMember(Name = "IDB", Order = 5)]
        public int IdB
        {
            get;
            set;
        }

        /// <summary>
        /// Role identifier of the current trade actor. 
        /// </summary>
        [DataMember(Name = "IDROLEACTOR", Order = 6)]
        public string IdRoleActor
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of the book of the current trade actor. It is the book where the trade is registered.
        /// </summary>
        [DataMember(Name = "BOOK_IDENTIFIER", Order = 7)]
        public string BookIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Warning level of the current book
        /// </summary>
        [DataMember(Name = "VRFEE", Order = 8)]
        public Cst.CheckModeEnum VrFee
        {
            get;
            set;
        }

    }

    // 20120619 Ticket 17908
    /// <summary>
    /// Reporitory containing all the additional datas (first id event and trade actors list) in order to get all the infos needed by the
    /// fees calculation process
    /// </summary>
    internal class TradeXFeesRepository : GenericStrategyRepository
    {
        /// <summary>
        /// Get all the additional datas (first id event and trade actors list) in order to get all the infos needed by the 
        /// fees calculation process which will follow.
        /// </summary>
        public List<TradeXFees> Trades { get; private set; }

        /// <summary>
        /// Get an sql request to load all of the trades Posting/GiveUp for a specific market/entity/business date.
        /// For each trade we return its own internal id and the internal id of the first event related to him.
        /// For each returned trade we have a line for each trade actor participating to the trade. 
        /// </summary>
        // EG 20131119 On prend l'événement TRD pour récupérer son IDE et plus (FED/AED/EED)
        /// EG 20150306 TRDTYPE (string)
        /// FI 20180131 [23755] Modify => Spheres only consider books where fees are expected (ie VRFEE Warning or Error)
        ///                            => for further details see GetQueryCandidatesToFeesCalculation 
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override string SQLREQUEST
        {
            get
            {
                //PL 20170304
                return @"select 
                        t.IDT, 
                        ev.IDE as IDE_EVENT, 
                        ta.IDA, 
                        ac.IDENTIFIER as ACTOR_IDENTIFIER,
                        ta.IDB, 
                        ta.IDROLEACTOR, 
                        b.IDENTIFIER as BOOK_IDENTIFIER, 
                        b.VRFEE
                        from dbo.VW_TRADE_FUNGIBLE_LIGHT_ETD t
                        inner join dbo.EVENT ev  on (ev.IDT = t.IDT) and (ev.EVENTCODE = 'TRD')
                        inner join dbo.TRADEACTOR ta on (ta.IDT = t.IDT)
                        inner join dbo.BOOK b on (b.IDB = ta.IDB) and (isnull(b.VRFEE,'None') != 'None')
                        inner join dbo.ACTOR ac on (ac.IDA = ta.IDA)
                        where (t.DTBUSINESS = @DTBUSINESS and t.IDA_ENTITY = @IDA_ENTITY and t.IDM = @IDM) and (t.TRDTYPE not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + @"))";
            }
        }

        /// <summary>
        /// Create a new instance of the repository
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        /// <param name="pDtBusiness">current business date, NOT empty</param>
        /// <param name="pIDM">market id, major than 0</param>
        /// <param name="pIDA_ENTITY">entity id, major than 0</param>
        public TradeXFeesRepository(string pCS, DateTime pDtBusiness, int pIDM, int pIDA_ENTITY) :
            base(pCS, pDtBusiness, pIDM, pIDA_ENTITY)
        { }

        /// <summary>
        /// Load the additional datas
        /// </summary>
        // EG 20160404 Migration vs2013
        public void LoadTrades()
        {
            if (!Initialized || DtBusiness <= DateTime.MinValue || IDM <= 0 || IDA_ENTITY <= 0)
            {
                throw new ArgumentException("The TradeXStrategyRepository object has been not correctly created nor initialized");
            }

            // EG 20160404 Migration vs2013
            //Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
            //dbParameterValues.Add("IDA_ENTITY", this.IDA_ENTITY);
            //dbParameterValues.Add("IDM", this.IDM);
            //dbParameterValues.Add("DTBUSINESS", this.DtBusiness.Date);
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDA_ENTITY", DbType.Int32), this.IDA_ENTITY);
            dp.Add(new DataParameter(CS, "IDM", DbType.Int32), this.IDM);
            dp.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), this.DtBusiness.Date);

            using (IDbConnection connection = DataHelper.OpenConnection(CS))
            {
                // EG 20160404 Migration vs2013
                //Trades = DataHelper<TradeXFees>.ExecuteDataSet(
                //    connection, CommandType.Text, SQLREQUEST, DataContractHelper.GetDbDataParameters(this.Parameters, dbParameterValues));
                QueryParameters qryParameters = new QueryParameters(CS, SQLREQUEST, dp);
                Trades = DataHelper<TradeXFees>.ExecuteDataSet(connection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
        }
    }

    /// <summary>
    /// Helper class to add strategy modes/attributes to the physical Xml flow of a Spheres trade (TRADEXML column of the TRADE table). 
    /// </summary>
    internal class TrdLegFixMLBuilder
    {
        XmlDocument XmlTrade { get; set; }

        TradeXStrategy Trade { get; set; }

        XmlNamespaceManager NsMgr { get; set; }

        /// <summary>
        /// Create a new instance of the helper class
        /// </summary>
        /// <param name="pXmlTrade">Xml flow for a single trade</param>
        /// <param name="pTradeXStrategy">related trade element</param>
        public TrdLegFixMLBuilder(XmlDocument pXmlTrade, TradeXStrategy pTradeXStrategy)
        {
            XmlTrade = pXmlTrade;

            Trade = pTradeXStrategy;

            NsMgr = new XmlNamespaceManager(pXmlTrade.NameTable);

            NsMgr.AddNamespace("efs", Cst.EFSmL_Namespace_3_0);
            NsMgr.AddNamespace("fixml", Cst.FixML_Namespace_5_0_SP1);
        }

        /// <summary>
        /// Add the missing strategy datas, or delete them in case the strategy does not exist anymore
        /// </summary>
        public void BuildTrdLeg()
        {

            if (Trade == null || XmlTrade == null)
            {
                throw new ArgumentException("The TrdLegFixMLBuilder has not been well created");
            }

            // 1. Update the FixML:MLegRptTyp attribut
            if (Trade.UpdateMLegRptTyp)
            {
                // 20120611 MF Ticket 17882
                // Verify that the strategy information are valid (not null)
                if (Trade.MLegRptTyp != null)
                {
                    XmlAttribute attrLegRptTyp = XmlTrade.CreateAttribute("", "MLegRptTyp", "");
                    attrLegRptTyp.Value = Trade.MLegRptTyp;

                    XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt", NsMgr).
                        Attributes.Append(attrLegRptTyp);
                }
                // 20120611 MF Ticket 17882
                else
                {
                    // 1.1 The MLegRptTyp attribute has to be deleted, because of it is not a strategy anymore

                    XmlNode nodeTrdCaptRpt = XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt", NsMgr);
                    XmlAttribute attrMLegRptTyp = nodeTrdCaptRpt.Attributes["MLegRptTyp"];

                    if (attrMLegRptTyp != null)
                    {
                        nodeTrdCaptRpt.Attributes.Remove(attrMLegRptTyp);
                    }
                }
            }

            // 2. Update the FixML:TrdLeg node (attributes SecSubTyp and LegNo)
            if (Trade.UpdateSecSubTyp || Trade.UpdateLegNo)
            {

                XmlNode nodeTrdLeg = XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg", NsMgr);
                XmlNode nodeLeg;
                if (nodeTrdLeg == null)
                {
                    // 2.1 if the node not exist (new strategy) we crete the path TrdLeg/Leg with empty attributes

                    XmlNode nodeInstr = XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt/fixml:Instrmt", NsMgr);

                    nodeTrdLeg = XmlTrade.CreateElement("fixml", "TrdLeg", Cst.FixML_Namespace_5_0_SP1);

                    XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt", NsMgr).InsertAfter(nodeTrdLeg, nodeInstr);

                    nodeLeg = XmlTrade.CreateElement("fixml", "Leg", Cst.FixML_Namespace_5_0_SP1);
                    nodeTrdLeg.AppendChild(nodeLeg);

                    XmlAttribute attrSecTyp = XmlTrade.CreateAttribute("", "SecTyp", "");
                    attrSecTyp.Value = "MLEG";
                    nodeLeg.Attributes.Append(attrSecTyp);
                }
                else
                {
                    nodeLeg = XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg/fixml:Leg", NsMgr);
                }

                // 20120611 MF Ticket 17882
                // Verify that the strategy information are valid (not null)
                if (Trade.LegNo != 0 && Trade.SecSubTyp != null)
                {
                    // 2.2 fill the attribute values with the related strategy datas

                    XmlAttribute attrLegNo = XmlTrade.CreateAttribute("", "LegNo", "");
                    attrLegNo.Value = Trade.LegNo.ToString();
                    nodeTrdLeg.Attributes.Append(attrLegNo);

                    XmlAttribute attrSecSubTyp = XmlTrade.CreateAttribute("", "SecSubTyp", "");
                    attrSecSubTyp.Value = Trade.SecSubTyp;
                    nodeLeg.Attributes.Append(attrSecSubTyp);
                }
                else
                // 20120611 MF Ticket 17882
                {
                    // 2.3 delete all the TrdLeg Nodes, because of the strategy does not exist any more

                    XmlNodeList nodesTrdLeg = XmlTrade.SelectNodes("//fixml:FIXML/fixml:TrdCaptRpt/fixml:TrdLeg", NsMgr);

                    if (nodesTrdLeg != null)
                    {
                        XmlNode father = XmlTrade.SelectSingleNode("//fixml:FIXML/fixml:TrdCaptRpt", NsMgr);

                        foreach (XmlNode node in nodesTrdLeg)
                        {
                            father.RemoveChild(node);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Main class for the strategy identification process
    /// </summary>
    // EG 20190114 Add detail to ProcessLog Refactoring
    public class StrategyMarker
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        SetErrorWarning SetErrorWarning { get; set; }

        

        string CS { get; set; }

        IPosRequest PosRequest { get; set; }

        StrategyEnumRepository StrategiesDictionary { get; set; }

        // 20120612 MF Ticket 17886
        StrategyIdentificationRulesRepository RulesRepository { get; set; }

        TradeXStrategyRepository TradesRepository { get; set; }

        TradeXFeesRepository TradesXFeesRepository { get; set; }

        DataParameter[] ReadParameters { get; set; }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        const string READSQL = "select TRADEXML from dbo.TRADEXML where IDT = @IDT";

        const string WRITESQL = "update dbo.TRADEXML set TRADEXML = @TRADEXML where IDT = @IDT";

        DataParameter[] WriteParameters { get; set; }

        /// <summary>
        /// get the initialization status of the object
        /// </summary>
        public bool Initialized { get; private set; }

        // 20120619 Ticket 17908
        /// <summary>
        /// Get the trades Posting/GiveUp which have been updated in order to set specific strategy informations
        /// </summary>
        IEnumerable<TradeXStrategy> UpdatedTrades { get; set; }

        private bool m_StrategySerializationWithCarriageReturn = true;

        /// <summary>
        /// Get/Set the serialization mode of the enriched (with strategy infos) XML trade. 
        /// When set to true the XML trade will keep the carriage returns.
        /// </summary>
        /// <value>true by default</value>
        public bool StrategySerializationWithCarriageReturn
        {
            get { return m_StrategySerializationWithCarriageReturn; }
            set { m_StrategySerializationWithCarriageReturn = value; }
        }

        /// <summary>
        /// Create a new strategy marker
        /// </summary>
        /// <param name="pCS">connection string, NOT null</param>
        /// <param name="pPosRequest">position request for the current EOD process, NOT null</param>
        /// <param name="pSetErrorWarning"></param>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20200623 [XXXXX] Add SetErrorWarning
        public StrategyMarker(string pCS, IPosRequest pPosRequest, SetErrorWarning pSetErrorWarning)
        {
            this.CS = pCS;

            this.PosRequest = pPosRequest;

            this.SetErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Init the strategy marker instance. Load the trades potentially linked to a strategy (taking care of all instruction rules), 
        /// and initialize the strategy enumeration with the official Spheres strategy labels.
        /// </summary>
        /// <returns>true when the intialization process succeed, false otherwise</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public bool Init()
        {

            if (String.IsNullOrEmpty(CS))
            {
                throw new ArgumentException("The StrategySetter object has been not correctly created");
            }

            if (PosRequest.DtBusiness <= DateTime.MinValue || PosRequest.IdM <= 0 || PosRequest.IdA_Entity <= 0)
            {
                throw new ArgumentException("Cannot loaded the allocations, the StrategySetter object has been not correctly created");
            }

            Initialized = false;

            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5022), 4,
                new LogParam(LogTools.IdentifierAndId(PosRequest.Identifiers.Entity, PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(PosRequest.Identifiers.CssCustodian, PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(PosRequest.Identifiers.Market, PosRequest.IdM)),
                new LogParam(PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(PosRequest.DtBusiness))));


            // 1. Init strategy collection

            StrategiesDictionary = new StrategyEnumRepository(CS);

            bool ok = StrategiesDictionary.BuildDictionary();

            if (!ok)
            {
                // FI 20200623 [XXXXX] call SetErrorWarning
                SetErrorWarning(ProcessStateTools.StatusErrorEnum);
            }

            // 2. Load/Aggregate the strategy identification rules

            RulesRepository =
                new StrategyIdentificationRulesRepository(CS, PosRequest.DtBusiness, PosRequest.IdM, PosRequest.IdA_Entity);

            ok = RulesRepository.Init();

            if (ok)
            {
                RulesRepository.LoadAndAggregate();
            }

            // 20120612 MF Ticket 17886
            // 3. Load trades allocations

            TradesRepository =
                new TradeXStrategyRepository(CS, PosRequest.DtBusiness, PosRequest.IdM, PosRequest.IdA_Entity, RulesRepository.RulesConditions);

            ok = TradesRepository.Init();

            if (ok)
            {
                TradesRepository.LoadTrades();
            }

            // 20120619 MF Ticket 17908
            // 4. Load additional datas to build the TradeFeesCalculation objects to send to the EOD_FeesGen method

            TradesXFeesRepository =
                new TradeXFeesRepository(this.CS, PosRequest.DtBusiness, PosRequest.IdM, PosRequest.IdA_Entity);

            ok = TradesXFeesRepository.Init();

            if (ok)
            {
                // FI 20210728 [XXXXX] Application d'un tryMultiple sur l'appel à TradesXFeesRepository.LoadTrades()  
                //TradesXFeesRepository.LoadTrades();

                TryMultiple tryMultiple = new TryMultiple(CS, "TradesXFeesRepositoryLoadTrades", $"TradesXFeesRepositoryLoadTrades")
                {
                    SetErrorWarning = SetErrorWarning,
                    IsModeTransactional = false,
                    ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                };
                tryMultiple.Exec(delegate { this.TradesXFeesRepository.LoadTrades(); });
            }

            // 5. Init parameter for trade update

            ReadParameters = new DataParameter[]
                {
                    new DataParameter(CS, "IDT", DbType.Int32),
                };

            WriteParameters = new DataParameter[]
                {
                    new DataParameter(CS, "IDT", DbType.Int32),
                    new DataParameter(CS, "TRADEXML", DbType.Xml),
                };

            Initialized = ok;

            return Initialized;
        }

        /// <summary>
        /// When the current instance has been well initialized, build the strategy collection using a TradeXStrategyRepository object
        /// </summary>
        public void ComposeStrategies()
        {
            if (!Initialized)
            {
                throw new ArgumentException("The StrategySetter is not well initialized");
            }

            TradesRepository.BuildStrategies();

            foreach (Strategy strategy in TradesRepository.Strategies)
            {
                // MF 20120926 - Ticket 18148 - add criteria (IsAnyLegOption) over Legs to be an option instrument to block strategy identification on Futures
                if (strategy.Legs.Count() > 1 && strategy.IsAnyLegOption())
                {
                    strategy.IsRealStrategy = true;

                    strategy.MLegRptTyp = "2";
                }
            }

        }

        /// <summary>
        /// When the strategy collection has been correctly built, mark all the built the strategy objects with the related strategy datas
        /// </summary>
        public void StrategyIdentification()
        {
            if (!Initialized || this.TradesRepository.Strategies == null)
            {
                throw new ArgumentException("The StrategySetter does not have performed the BuildStrategies procedure, or it is not initialized");
            }

            IEnumerable<Strategy> validStrategies =
                from strategy in TradesRepository.Strategies where strategy.IsRealStrategy select strategy;

            foreach (Strategy validStrategy in validStrategies)
            {
                validStrategy.SetStrategyDatas(this.StrategiesDictionary);
            }
        }

        /// <summary>
        /// For each identified strategy (or trade ex-strategy), 
        /// verify if the related trades lack of some strategy datas, in case set these ones to be updated 
        /// </summary>
        public void SetAllocsToUpdate()
        {
            if (!Initialized
                || this.TradesRepository.Strategies == null || this.TradesRepository.Trades == null || this.TradesRepository.ExStrategies == null)
            {
                throw new ArgumentException("The StrategySetter does not have performed the BuildStrategies procedure, or it is not initialized");
            }

            IEnumerable<Strategy> validStrategies =
                from strategy in TradesRepository.Strategies
                where
                    strategy.IsRealStrategy && strategy.SecSubTyp != null
                select strategy;

            // 1. Finding the trade to update in the processed trades set
            foreach (Strategy validStrategy in validStrategies)
            {
                validStrategy.SetElementsToUpdate();
            }

            // 20120611 MF Ticket 17882 - trades that lost its status of strategy
            // 2. Finding all the trades to reset in the processed trades set (TradesRepository.Trades) 
            //    as well as in the not processed trades set (TradesRepository.ExStrategies)
            SetTradesToReset();

        }

        /// <summary>
        /// Finding all the trades to reset in the processed trades set (TradesRepository.Trades) 
        /// as well as in the not processed trades set (TradesRepository.ExStrategies)
        /// </summary>
        private void SetTradesToReset()
        {
            // 1. Get all the trades in the processed set (TradesRepository.Trades) which making part of a valid strategy
            IEnumerable<TradeXStrategy> tradesOfStrategies =
                from strategy in TradesRepository.Strategies
                where
                    strategy.IsRealStrategy && strategy.SecSubTyp != null
                from trade in strategy.Trades
                select trade;

            // 2. Extract all the trades that are NOT making part of a strategy from the processed trades set (TradesRepository.Trades) 
            IEnumerable<TradeXStrategy> tradesToReset = TradesRepository.Trades.Except(tradesOfStrategies);

            // 3. Sum the preceeding set wih the not processed trades set (TradesRepository.ExStrategies)
            tradesToReset = tradesToReset.Union(this.TradesRepository.ExStrategies);

            // 4. Choose only the elements already evaluated as a strategy (during another EOD process). 
            //    All the trades Posting/GiveUp in the TradesRepository.ExStrategies repository will match this condition EVER.
            tradesToReset = tradesToReset.Where(trade => !String.IsNullOrEmpty(trade.MLegRptTyp) && !trade.ToUpdate);

            // 5. Finally set these trades to be updated, in other words to be reset, and reset strategy datas
            foreach (TradeXStrategy trade in tradesToReset)
            {
                trade.ToUpdate = true;

                trade.MLegRptTyp = null;
                trade.UpdateMLegRptTyp = true;

                trade.SecSubTyp = null;
                trade.UpdateSecSubTyp = true;

                trade.UpdateLegNo = true;
                trade.LegNo = 0;
            }
        }

        /// <summary>
        /// Update all the trades to be updated, filling the related Xml flow with the needed FixMl nodes and attributes, using the
        /// TrdLegFixMLBuilder helper class
        /// </summary>
        /// <returns>The trades Posting/GiveUp updated</returns>
        // EG 20160404 Migration vs2013
        public void UpdateAllocs()
        {
            if (!Initialized || this.TradesRepository.Strategies == null)
            {
                throw new ArgumentException("The StrategySetter does not have performed the BuildStrategies procedure, or it is not initialized");
            }

            // 1. get all the trades flagges as to update
            IEnumerable<TradeXStrategy> tradesToUpdate =
                // 20120615 MF Ticket 17882 reprise (union with ExStrategies)
                from alloc in TradesRepository.Trades.Union(TradesRepository.ExStrategies)
                where
                    alloc.ToUpdate
                select alloc;

            // 2. define the document container used as handle to manipulate any TRADEXML element to update
            XmlDocument xmlTrade = new XmlDocument();

            // EG 20160404 Migration vs2013
            //Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
            DataParameters dp = new DataParameters();

            // 3. for each trade to update..
            foreach (TradeXStrategy trade in tradesToUpdate)
            {
                // EG 20160404 Migration vs2013
                //dbParameterValues.Add("IDT", trade.IDT);
                dp.Add(new DataParameter(CS, "IDT", DbType.Int32), trade.IDT);


                // 3.1 read the TRADEXML
                // EG 20160404 Migration vs2013
                //DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, READSQL,
                //               DataContractHelper.GetDbDataParameters(this.ReadParameters, dbParameterValues));
                QueryParameters qryParameters = new QueryParameters(CS, READSQL, dp);
                DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                xmlTrade.LoadXml((string)ds.Tables[0].Rows[0]["TRADEXML"]);

                // 3.2 M§odifiy the node accordinf with the current strategy attribute on the trade and the flag set
                TrdLegFixMLBuilder builder = new TrdLegFixMLBuilder(xmlTrade, trade);
                builder.BuildTrdLeg();

                // TODO MF 20120502 : the IDataDocument has been introduced just to produce one XML UTF-16 encoded as well as to keep 
                //  .. carriage returns.
                //  .. The previous XML modification "BuildTrdLeg", which inserts strategy datas, is born like 
                //  .. as a basic XPath operation in order to get 
                //  .. better performances and to avoid IDataDocument serialization/deserialization. Using IDataDocument afterwards,
                //  .. this original proposition gets lost. 
                //  .. Then Use IDataDocument directly for XML modifications and get rid of any XPath operation
                EFS_SerializeInfo tradeDeserializerInfo = new EFS_SerializeInfo(xmlTrade.OuterXml);
                IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(tradeDeserializerInfo);
                EFS_SerializeInfo tradeSerializerInfo = new EFS_SerializeInfo(dataDoc);
                string xmlTradeWithCarriageReturn = CacheSerializer.Serialize(tradeSerializerInfo).ToString();

                // 3.3 Reload the modified trade content to have a specific format UTF-16 and carriage returns
                xmlTrade.LoadXml(xmlTradeWithCarriageReturn);

                string queryUpdate = WRITESQL;

                int inserted;

                // 3.4 Write the modified XML into the database
                switch (DataHelper.GetDbSvrType(this.CS))
                {
                    case DbSvrType.dbORA:
                        {
                            if (this.StrategySerializationWithCarriageReturn)
                            {
                                // Default query inserted afterwards the PL specification saying to preserve carriage returns in the trade xml.
                                // we need to use the StringBuilder.

                                // EG 20160404 Migration vs2013
                                //dbParameterValues.Add("TRADEXML", xmlTradeWithCarriageReturn);
                                //inserted = DataHelper.ExecuteNonQuery(this.CS,CommandType.Text,queryUpdate, DataContractHelper.GetDbDataParameters(this.WriteParameters, dbParameterValues));
                                dp.Add(new DataParameter(CS, "TRADEXML", DbType.Xml), xmlTradeWithCarriageReturn);
                                qryParameters = new QueryParameters(CS, queryUpdate, dp);
                                inserted = DataHelper.ExecuteNonQuery(this.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            }
                            else
                            {
                                // Emegency query will be necessary in case the xml will be any longer than 32KB
                                // EG 20160404 Migration vs2013
                                //inserted = DataHelper.ExecuteNonQueryXmlForOracle(DbSvrType.dbORA,this.CS,queryUpdate,xmlTrade,"TRADEXML",DataContractHelper.GetDbDataParameters(this.WriteParameters, dbParameterValues));
                                qryParameters = new QueryParameters(CS, queryUpdate, dp);
                                inserted = DataHelper.ExecuteNonQueryXmlForOracle(DbSvrType.dbORA, this.CS, qryParameters.Query, xmlTrade, "TRADEXML", qryParameters.Parameters.GetArrayDbParameter());
                            }
                        }

                        break;

                    case DbSvrType.dbSQL:
                    default:
                        {
                            // EG 20160404 Migration vs2013
                            //dbParameterValues.Add("TRADEXML", xmlTradeWithCarriageReturn);
                            //inserted = DataHelper.ExecuteNonQuery(this.CS, CommandType.Text, queryUpdate, DataContractHelper.GetDbDataParameters(this.WriteParameters, dbParameterValues));
                            dp.Add(new DataParameter(CS, "TRADEXML", DbType.Xml), xmlTradeWithCarriageReturn);
                            qryParameters = new QueryParameters(CS, queryUpdate, dp);
                            inserted = DataHelper.ExecuteNonQuery(this.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }

                        break;
                }

                // EG 20160404 Migration vs2013
                //dbParameterValues.Clear();
                dp.Clear();
            }

            // 20120619 Ticket 17908
            UpdatedTrades = tradesToUpdate;

        }

        // 20120619 Ticket 17908
        /// <summary>
        /// Convert all the updated trade object to objects type of <see cref="EfsML.Business.TradeFeesCalculation"/>. These objects will
        /// be passed to the fees calculation method of the End OF Day process, for each transformed trade we will compute a brand new fees set.
        /// </summary>
        /// <returns>A list of objects type of <see cref="EfsML.Business.TradeFeesCalculation"/></returns>
        public IEnumerable<TradeFeesCalculation> GetTradeFeesCalculationObjects()
        {
            if (!Initialized || this.TradesRepository.Strategies == null || TradesXFeesRepository.Trades == null || UpdatedTrades == null)
            {
                throw new ArgumentException("The StrategySetter does not have performed the UpdateAllocs procedure, or it is not well initialized");
            }

            IEnumerable<TradeFeesCalculation> tradesXFeesCalculation =
                from tradeXFees in this.TradesXFeesRepository.Trades
                    // 1. Group the additional datas useful to make the conversion into  objects type of EfsML.Business.TradeFeesCalculation
                    //    by trade and related first event.
                group tradeXFees by new { tradeXFees.IDT, IdE = tradeXFees.IDEvent }
                    into groupedTradeXFees
                // 2. Join the grouped additional datas to the set of updated trades in the current strategy identification process
                //    in order to get a EfsML.Business.TradeFeesCalculation object just when the trade has been changed.
                join tradeUpdated in this.UpdatedTrades on groupedTradeXFees.Key.IDT equals tradeUpdated.IDT
                select
                // 3. Build the fees calculation objects
                new TradeFeesCalculation
                {
                    idT = groupedTradeXFees.Key.IDT,

                    idE_Event = groupedTradeXFees.Key.IdE,

                    identifier = tradeUpdated.TradeIdentifier,

                    qty = tradeUpdated.Quantity,

                    actorFee =
                    // 3.1 Build the actor list concerned by the fee 
                    //    (one item will be added for each TRADEACTOR item found for the current trade).
                    (from tradeXFee in groupedTradeXFees
                     select
new ActorFeesCalculation
                         {
                             actorIdentifier = tradeXFee.ActorIdentifier,

                             bookIdentifier = tradeXFee.BookIdentifier,

                             idA = tradeXFee.IdA,

                             idB = tradeXFee.IdB,

                             idRoleActor = tradeXFee.IdRoleActor,

                             vrFee = tradeXFee.VrFee,
                         })
                         .ToDictionary(elem => elem.idB)
                };

            return tradesXFeesCalculation;
        }
    }

    /// <summary>
    /// Container class for all method extentions of the strategy marker namespace
    /// </summary>
    internal static class StrategyMarkerExtentions
    {
        /// <summary>
        /// Get all the elements of the first enumeration not existing in the second enumeration, 
        /// the equality check is done according with the provided comparer
        /// </summary>
        /// <typeparam name="T">type of the enumeration items</typeparam>
        /// <param name="pEnumerable">first enumeration</param>
        /// <param name="pExcept">second enumeration, containing the element to exclude</param>
        /// <param name="pComparer">the comparer expression defining the equality check</param>
        /// <returns>all the elements of the first enumeration not existing in the second enumeration</returns>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> pEnumerable, IEnumerable<T> pExcept, Func<T, T, bool> pComparer)
        {
            return pEnumerable.Except(pExcept, new SpheresLambdaComparer<T>(pComparer));
        }

        /// <summary>
        /// Get all the unique elements in the current enumeration, the equality check is done according with the provided comparer
        /// </summary>
        /// <typeparam name="T">type of the enumeration items</typeparam>
        /// <param name="pEnumerable">enumeration where the distinct operation is applied</param>
        /// <param name="pComparer">the comparer expression defining the equality check</param>
        /// <returns>all the unique elements</returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> pEnumerable, Func<T, T, bool> pComparer)
        {
            return pEnumerable.Distinct(new SpheresLambdaComparer<T>(pComparer));
        }
    }

}
