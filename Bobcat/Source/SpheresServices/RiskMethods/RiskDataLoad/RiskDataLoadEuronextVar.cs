using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.IO;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using EfsML.Enum;
//
using FpML.Enum;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Enum pour la source des données de la méthode Euronext Var
    /// </summary>
    public enum EuronextVarSector
    {
        /// <summary>
        /// Euronext Borsa Italiana Equity Derivative
        /// </summary>
        EQY,
        /// <summary>
        /// Euronext Legacy Commodity Derivative
        /// </summary>
        LGY_COM,
        /// <summary>
        /// Euronext Legacy Equity Derivative
        /// </summary>
        LGY_EQY,
    }

    /// <summary>
    /// Classe représentant la clé d'un scénario, soit une date de référence et un type (S:scaled ou U:unscaled ou C:current).
    /// </summary>
    public class RiskDataEuronextVarScenarioKey
    {
        #region Members
        private readonly string m_Type;
        private readonly DateTime m_Date;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Scenario reference date
        /// </summary>
        public DateTime ReferenceDate { get { return m_Date; } }

        /// <summary>
        /// Scenario type : current ('C') / ordinary(scaled) ('S') / stressed(unscaled) ('U')
        /// </summary>
        public string Type { get { return m_Type; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="type">Scenario type</param>
        /// <param name="date">Scenario reference date</param>
        public RiskDataEuronextVarScenarioKey(string type, DateTime date)
        {
            m_Type = type;
            m_Date = date;
        }
        #endregion
    }

    /// <summary>
    /// Classe représentant un élément de scénario
    /// </summary>
    public sealed class RiskDataEuronextVarScenario
    {
        #region Members
        private readonly RiskDataEuronextVarScenarioKey m_key;
        private readonly decimal m_Value;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Scenario key 
        /// </summary>
        public RiskDataEuronextVarScenarioKey Key { get { return m_key; } }

        /// <summary>
        /// Scenario value
        /// </summary>
        public decimal Value { get { return m_Value; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pType">Scenario type</param>
        /// <param name="pDate">Scenario reference date</param>
        /// <param name="pValue">Scenario value</param>
        public RiskDataEuronextVarScenario(string pType, DateTime pDate, decimal pValue)
        {
            m_key = new RiskDataEuronextVarScenarioKey(pType, pDate);
            m_Value = pValue;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement de scénario (Price ou Fx)</param>
        public RiskDataEuronextVarScenario(IOTaskDetInOutFileRow pParsingRow)
        {
            DateTime dt = DtFunc.ParseDate(RiskDataLoad.GetRowDataValue(pParsingRow, "ref_dt"), DtFunc.FmtISODate, null);
            string type = RiskDataLoad.GetRowDataValue(pParsingRow, "scenario");

            m_key = new RiskDataEuronextVarScenarioKey(type, dt);
            m_Value = Decimal.Parse(RiskDataLoad.GetRowDataValue(pParsingRow, "value"), System.Globalization.NumberStyles.Float);
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        public RiskDataEuronextVarScenario(string[] pFieldsName, string[] pFieldsValue)
        {
            DateTime dt = DtFunc.ParseDate(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "ref_dt"), DtFunc.FmtDateyyyyMMdd, null);
            string type = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "scenario");

            m_key = new RiskDataEuronextVarScenarioKey(type, dt);
            m_Value = Decimal.Parse(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "value"), System.Globalization.NumberStyles.Float);
        }
        #endregion Constructors
    }

    /// <summary>
    /// Classe représentant les taux de change entre 2 devises pour tous les scénarios
    /// </summary>
    public sealed class RiskDataEuronextVarSceFx
    {
        #region Members
        private DateTime m_EvaluationDate;
        private readonly string m_BaseCurrency;
        private readonly string m_CounterCurrency;
        private readonly List<RiskDataEuronextVarScenario> m_ScenarioFx;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Evaluation Date
        /// </summary>
        public DateTime EvaluationDate { get { return m_EvaluationDate; } }

        /// <summary>
        /// Base Currency
        /// </summary>
        public string BaseCurrency { get { return m_BaseCurrency; } }

        /// <summary>
        /// Counter Currency
        /// </summary>
        public string CounterCurrency { get { return m_CounterCurrency; } }

        /// <summary>
        /// Taux de change de chaque scénarios
        /// </summary>
        public List<RiskDataEuronextVarScenario> ScenarioFx { get { return m_ScenarioFx; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur. 
        /// Parse les données communes d'un même couple de devises (ne parse pas les taux des scénarios)
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement Forex File</param>
        public RiskDataEuronextVarSceFx(IOTaskDetInOutFileRow pParsingRow)
        {
            // PM 20240122 [WI822] La donnée "eval_dt" n'est plus présente dans le fichier Euronext Legacy mais présente dans le fichier Borsa Italiana
            //m_EvaluationDate = DtFunc.ParseDate(RiskDataLoad.GetRowDataValue(pParsingRow, "eval_dt"), DtFunc.FmtISODate, null);
            string eval_dt = RiskDataLoad.GetRowDataValue(pParsingRow, "eval_dt");
            if (StrFunc.IsFilled(eval_dt))
            {
                m_EvaluationDate = DtFunc.ParseDate(eval_dt, DtFunc.FmtISODate, null);
            }

            m_BaseCurrency = RiskDataLoad.GetRowDataValue(pParsingRow, "base_curcy");
            m_CounterCurrency = RiskDataLoad.GetRowDataValue(pParsingRow, "counter_curcy");
            m_ScenarioFx = new List<RiskDataEuronextVarScenario>();
        }

        /// <summary>
        /// Constructeur.
        /// Parse les données communes d'un même couple de devises (ne parse pas les taux des scénarios)
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        public RiskDataEuronextVarSceFx(string[] pFieldsName, string[] pFieldsValue)
        {
            // La donnée "eval_dt" n'est plus présente dans le fichier Euronext Legacy mais présente dans le fichier Borsa Italiana
            //m_EvaluationDate = DtFunc.ParseDate(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "eval_dt"), DtFunc.FmtDateyyyyMMdd, null);
            string eval_dt = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "eval_dt");
            if (StrFunc.IsFilled(eval_dt))
            {
                m_EvaluationDate = DtFunc.ParseDate(eval_dt, DtFunc.FmtDateyyyyMMdd, null);
            }

            m_BaseCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "base_curcy");
            m_CounterCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "counter_curcy");
            m_ScenarioFx = new List<RiskDataEuronextVarScenario>(1000);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Parse la valeur du taux du couple de devises et l'ajoute aux taux des scénarios
        /// </summary>
        /// <param name="pParsingRow"></param>
        /// <returns></returns>
        public void AddFx(IOTaskDetInOutFileRow pParsingRow)
        {
            RiskDataEuronextVarScenario fx = new RiskDataEuronextVarScenario(pParsingRow);
            m_ScenarioFx.Add(fx);

            // L'evaluation date correspond à la reference date du scenario de type 'C'
            if (fx.Key.Type == "C")
            {
                m_EvaluationDate = fx.Key.ReferenceDate;
            }
        }

        /// <summary>
        /// Parse la valeur du taux du couple de devises et l'ajoute aux taux des scénarios
        /// </summary>
        /// <param name="pFieldsName"></param>
        /// <param name="pFieldsValue"></param>
        /// <returns></returns>
        public void AddFx(string[] pFieldsName, string[] pFieldsValue)
        {
            RiskDataEuronextVarScenario fx = new RiskDataEuronextVarScenario(pFieldsName, pFieldsValue);
            m_ScenarioFx.Add(fx);

            // L'evaluation date correspond à la reference date du scenario de type 'C' 
            if (fx.Key.Type == "C")
            {
                m_EvaluationDate = fx.Key.ReferenceDate;
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant les valeurs pour chaque scénarios d'un asset
    /// </summary>
    public sealed class RiskDataEuronextVarScePrice
    {
        #region Members
        private readonly DateTime m_EvaluationDate;
        internal RiskDataEuronextVarSerie m_Serie;
        private List<RiskDataEuronextVarScenario> m_ScenarioPrice;
        private decimal m_CurrentValue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Evaluation Date
        /// </summary>
        public DateTime EvaluationDate { get { return m_EvaluationDate; } }

        /// <summary>
        /// Valeur de chaque scénarios
        /// </summary>
        public List<RiskDataEuronextVarScenario> ScenarioPrice { get { return m_ScenarioPrice; } }

        /// <summary>
        /// Charactéristiques de la série
        /// </summary>
        public RiskDataEuronextVarSerie Serie { get { return m_Serie; } }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return (m_Serie == default(RiskDataEuronextVarSerie) ? default(string) : m_Serie.PriceCurrency); } }

        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return (m_Serie == default(RiskDataEuronextVarSerie) ? 0 : m_Serie.IdAsset); } }

        /// <summary>
        /// Code Isin
        /// </summary>
        public string ISINCode { get { return (m_Serie == default(RiskDataEuronextVarSerie) ? default(string) : m_Serie.IsinCode); } }

        /// <summary>
        /// Current Value
        /// </summary>
        public Decimal CurrentValue { get { return m_CurrentValue; } set { m_CurrentValue = value; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// Parse les données communes au scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement Scenario File</param>
        public RiskDataEuronextVarScePrice(IOTaskDetInOutFileRow pParsingRow)
        {
            m_EvaluationDate = DtFunc.ParseDate(RiskDataLoad.GetRowDataValue(pParsingRow, "eval_dt"), DtFunc.FmtISODate, null);
            m_Serie = RiskDataEuronextVarSerie.ParseBorsaItalianaSerie(pParsingRow);
        }
        /// <summary>
        /// Constructeur
        /// Parse les données communes au scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <param name="pIsBorsaItalianaFile">Indicateur de fichier BorsaItaliana ou Euronext Legacy</param>
        public RiskDataEuronextVarScePrice(string[] pFieldsName, string[] pFieldsValue, bool pIsBorsaItalianaFile = true)
        {
            if (pIsBorsaItalianaFile)
            {
                m_EvaluationDate = DtFunc.ParseDate(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "eval_dt"), DtFunc.FmtDateyyyyMMdd, null);
                m_Serie = RiskDataEuronextVarSerie.ParseBorsaItalianaSerie(pFieldsName, pFieldsValue);
            }
            else
            {
                m_Serie = RiskDataEuronextVarSerie.ParseEuronextLegacyISIN(pFieldsName, pFieldsValue);
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Parse la valeur du scénario et l'ajoute aux valeurs des scénarios de l'asset
        /// </summary>
        /// <param name="pParsingRow"></param>
        /// <returns></returns>
        public RiskDataEuronextVarScenario AddScenario(IOTaskDetInOutFileRow pParsingRow)
        {
            RiskDataEuronextVarScenario scenario = new RiskDataEuronextVarScenario(pParsingRow);
            if (m_ScenarioPrice == default(List<RiskDataEuronextVarScenario>))
            {
                m_ScenarioPrice = new List<RiskDataEuronextVarScenario>(1000);
            }
            m_ScenarioPrice.Add(scenario);
            return scenario;
        }
        /// <summary>
        /// Parse la valeur du scénario et l'ajoute aux valeurs des scénarios de l'asset
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <returns></returns>
        public RiskDataEuronextVarScenario AddScenario(string[] pFieldsName, string[] pFieldsValue)
        {
            RiskDataEuronextVarScenario scenario = new RiskDataEuronextVarScenario(pFieldsName, pFieldsValue);
            if (m_ScenarioPrice == default(List<RiskDataEuronextVarScenario>))
            {
                m_ScenarioPrice = new List<RiskDataEuronextVarScenario>(1000);
            }
            m_ScenarioPrice.Add(scenario);

            // Current Value sur les enregistrementd de type 'C' pour les fichiers Euronext Legacy
            // Current Value sur les enregistrements pour lesquels ReferenceDate = EvaluationDate pour les fichiers BorsaItaliana
            if ((scenario.Key.Type == "C") || (scenario.Key.ReferenceDate == m_EvaluationDate))
            {
                m_CurrentValue = scenario.Value;
                m_Serie.CurrentValue = m_CurrentValue;
            }
            return scenario;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant les valeurs pour chaque scénarios d'un asset commodity en livraison
    /// </summary>
    public sealed class RiskDataEuronextVarSceDlyPrice
    {
        #region Members
        private string m_ISINCode;
        private string m_PriceCurrency;
        private string m_SymbolCode;
        private decimal m_Multiplier;
        private int m_HoldingPeriod;
        private decimal m_CurrentValue;
        private List<RiskDataEuronextVarScenario> m_ScenarioPrice;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Code Isin
        /// </summary>
        public string ISINCode
        {
            get { return m_ISINCode; }
            set { m_ISINCode = value; }
        }

        /// <summary>
        /// Price Currency
        /// </summary>
        public string Currency
        {
            get { return m_PriceCurrency; }
            set { m_PriceCurrency = value; }
        }

        /// <summary>
        /// Product symbol code
        /// </summary>
        public string ContractSymbol
        {
            get { return m_SymbolCode; }
            set { m_SymbolCode = value; }
        }

        /// <summary>
        /// Multiplier
        /// </summary>
        public decimal Multiplier
        {
            get { return m_Multiplier; }
            set { m_Multiplier = value; }
        }

        /// <summary>
        /// Holding Period
        /// </summary>
        public int HoldingPeriod { get { return m_HoldingPeriod; } }

        /// <summary>
        /// Current Value
        /// </summary>
        public decimal CurrentValue { get { return m_CurrentValue; } set { m_CurrentValue = value; } }

        /// <summary>
        /// Valeur de chaque scénarios
        /// </summary>
        public List<RiskDataEuronextVarScenario> ScenarioPrice { get { return m_ScenarioPrice; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// Parse les données communes au scénarios d'un même asset commodity en livraison (ne parse donc pas les valeurs des scénarios)
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        public RiskDataEuronextVarSceDlyPrice(string[] pFieldsName, string[] pFieldsValue)
        {
            m_ISINCode = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_id");
            m_PriceCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_curcy");
            m_SymbolCode = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "symbol_code");
            string mult = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mult");
            m_Multiplier = (mult != "NaN" ? DecFunc.DecValue(mult) : decimal.Zero);
            string hp = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "hppd");
            if (StrFunc.IsFilled(hp))
            {
                m_HoldingPeriod = int.Parse(hp);
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Parse la valeur du scénario et l'ajoute aux valeurs des scénarios de l'asset
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <returns></returns>
        public RiskDataEuronextVarScenario AddScenario(string[] pFieldsName, string[] pFieldsValue)
        {
            RiskDataEuronextVarScenario scenario = new RiskDataEuronextVarScenario(pFieldsName, pFieldsValue);
            if (m_ScenarioPrice == default(List<RiskDataEuronextVarScenario>))
            {
                m_ScenarioPrice = new List<RiskDataEuronextVarScenario>(1000);
            }
            m_ScenarioPrice.Add(scenario);

            // Current Value sur les enregistrementd de type 'C'
            if (scenario.Key.Type == "C")
            {
                m_CurrentValue = scenario.Value;
            }
            return scenario;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant les informations sur un asset pour les fichiers Euronext Var 
    /// </summary>
    public sealed class RiskDataEuronextVarSerie : EuronextVarSerieBase
    {
        #region Members
        #region Données du fichier
        private decimal m_Price;
        private string m_DecorrelationGroup;
        private string m_ProductGroup;
        private string m_SubPortfolio;
        #endregion Données du fichier

        private int m_IdAsset;
        private decimal m_CurrentValue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Settlement/Closing Price
        /// </summary>
        public decimal Price
        {
            get { return m_Price; }
            set { m_Price = value; }
        }

        /// <summary>
        /// Decorrelation Group
        /// </summary>
        public string DecorrelationGroup
        {
            get { return m_DecorrelationGroup; }
            set { m_DecorrelationGroup = value; }
        }

        /// <summary>
        /// Product Group
        /// </summary>
        public string ProductGroup
        {
            get { return m_ProductGroup; }
            set { m_ProductGroup = value; }
        }

        /// <summary>
        /// Sub-Portfolio
        /// </summary>
        public string SubPortfolio
        {
            get { return m_SubPortfolio; }
            set { m_SubPortfolio = value; }
        }

        /// <summary>
        /// Id de l'asset (uniquement sur les ASSET_ETD)
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } set { m_IdAsset = value; } }

        /// <summary>
        /// Current Value correspond à la valeur du sénario de risque tel que Référence date == Evaluation date dans le fichier BorsaItaliana
        /// ou au scénario de type 'C' dans les fichiers Euronext Legacy
        /// </summary>
        public decimal CurrentValue { get { return m_CurrentValue; } set { m_CurrentValue = value; } }
        #endregion Accessors

        #region Constructors
        public RiskDataEuronextVarSerie()
        { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Parse les données d'un asset à partir d'un fichier BorsaItaliana
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement contenant les informations sur l'asset</param>
        /// <returns></returns>
        public static RiskDataEuronextVarSerie ParseBorsaItalianaSerie(IOTaskDetInOutFileRow pParsingRow)
        {
            RiskDataEuronextVarSerie serie = new RiskDataEuronextVarSerie();
            serie.m_IsinCode = RiskDataLoad.GetRowDataValue(pParsingRow, "instr_id");
            serie.m_AssetType = RiskDataLoad.GetRowDataValue(pParsingRow, "asset_type");
            serie.m_OptionType = RiskDataLoad.GetRowDataValue(pParsingRow, "option_type");
            serie.m_Symbol = RiskDataLoad.GetRowDataValue(pParsingRow, "class_code");
            serie.m_UnderlyingISIN = RiskDataLoad.GetRowDataValue(pParsingRow, "und_instr_id");
            string mult = RiskDataLoad.GetRowDataValue(pParsingRow, "mult");
            serie.m_Multiplier = (mult != "NaN" ? DecFunc.DecValue(mult) : decimal.Zero);
            serie.m_StrikePrice = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "strike"));
            serie.m_PriceCurrency = RiskDataLoad.GetRowDataValue(pParsingRow, "settl_curcy");

            string matDt = RiskDataLoad.GetRowDataValue(pParsingRow, "mat_dt");
            serie.m_MaturityDate = DateTime.MinValue;
            if (DateTime.TryParseExact(matDt, DtFunc.FmtDateyyyyMMdd, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtResult))
            {
                serie.m_MaturityDate = dtResult;
            }

            // Ajout DecorrelationGroup. Dans fichier BorsaItaliana il correspond au code ISIN du sous-jacent
            serie.m_DecorrelationGroup = serie.m_UnderlyingISIN;
            serie.m_ProductGroup = "N/A";
            serie.m_SubPortfolio = "N/A";

            return serie;
        }

        /// <summary>
        /// Parse les données d'un asset à partir d'un fichier BorsaItaliana
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <returns></returns>
        public static RiskDataEuronextVarSerie ParseBorsaItalianaSerie(string[] pFieldsName, string[] pFieldsValue)
        {
            RiskDataEuronextVarSerie serie = new RiskDataEuronextVarSerie();
            serie.m_IsinCode = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_id");
            serie.m_AssetType = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "asset_type");
            serie.m_OptionType = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "option_type");
            serie.m_Symbol = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "class_code");
            serie.m_UnderlyingISIN = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "und_instr_id");
            string mult = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mult");
            serie.m_Multiplier = (mult != "NaN" ? DecFunc.DecValue(mult) : decimal.Zero);
            serie.m_StrikePrice = DecFunc.DecValue(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "strike"));
            serie.m_PriceCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "settl_curcy");

            string matDt = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mat_dt");
            serie.m_MaturityDate = DateTime.MinValue;
            if (DateTime.TryParseExact(matDt, DtFunc.FmtDateyyyyMMdd, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtResult))
            {
                serie.m_MaturityDate = dtResult;
            }

            // Ajout DecorrelationGroup. Dans fichier BorsaItaliana il correspond au code ISIN du sous-jacent
            serie.m_DecorrelationGroup = serie.m_UnderlyingISIN;
            serie.m_ProductGroup = "N/A";
            serie.m_SubPortfolio = "N/A";
            return serie;
        }

        /// <summary>
        /// Parse les données d'un asset à partir d'un fichier Euronext Legacy
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <returns></returns>
        public static RiskDataEuronextVarSerie ParseEuronextLegacySerie(string[] pFieldsName, string[] pFieldsValue, EuronextVarSector pSector)
        {
            RiskDataEuronextVarSerie serie = new RiskDataEuronextVarSerie();
            serie.m_IsinCode = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_id");
            serie.m_PriceCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_curcy");
            serie.m_Symbol = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "symbol_code");
            serie.m_AssetType = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "asset_type");
            string mult = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mult");
            serie.m_Multiplier = (mult != "NaN" ? DecFunc.DecValue(mult) : decimal.Zero);
            serie.m_SettlementType = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "settl_type");
            serie.m_OptionType = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "option_type");
            serie.m_StrikePrice = DecFunc.DecValue(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "strike"));
            serie.m_UnderlyingISIN = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "und_instr_id");
            serie.m_UnderlyingCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "und_curcy");
            serie.m_Price = DecFunc.DecValue(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "price"));

            string matDt = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mat_dt");
            serie.m_MaturityDate = DateTime.MinValue;
            if (DateTime.TryParseExact(matDt, DtFunc.FmtDateyyyyMMdd, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtResult))
            {
                serie.m_MaturityDate = dtResult;
            }

            switch (pSector)
            {
                case EuronextVarSector.LGY_EQY:
                    serie.m_DecorrelationGroup = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "und_instr_id_deco");
                    serie.m_ProductGroup = "N/A";
                    serie.m_SubPortfolio = "N/A";
                    break;
                case EuronextVarSector.LGY_COM:
                    serie.m_DecorrelationGroup = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "deco_code");
                    serie.m_ProductGroup = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "prod_group");
                    serie.m_SubPortfolio = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "sub_ptf");
                    break;
            }
            return serie;
        }

        /// <summary>
        /// Parse le code ISIN et la devise d'un asset
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        /// <returns></returns>
        public static RiskDataEuronextVarSerie ParseEuronextLegacyISIN(string[] pFieldsName, string[] pFieldsValue)
        {
            RiskDataEuronextVarSerie serie = new RiskDataEuronextVarSerie();
            serie.m_IsinCode = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_id");
            serie.m_PriceCurrency = RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "instr_curcy");
            return serie;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe des paramètres de calcul de la méthode Euronext Var Based
    /// </summary>
    public class RiskDataEuronextVarParameter
    {
        #region Members
        private decimal m_OrdinaryConfidenceLevel;
        private decimal m_StressedConfidenceLevel;
        private decimal m_DecorrelationParameter;
        private decimal m_OrdinaryWeight;
        private decimal m_StressedWeight;
        private int m_HoldingPeriod;
        private int m_SubPortfolioSeparator;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ordinary Confidence Level
        /// </summary>
        public decimal OrdinaryConfidenceLevel { get { return m_OrdinaryConfidenceLevel; } }

        /// <summary> 
        /// Stressed ConfidenceLevel
        /// </summary>
        public decimal StressedConfidenceLevel { get { return m_StressedConfidenceLevel; } }

        /// <summary>
        /// Decorrelation Parameter
        /// </summary>
        public decimal DecorrelationParameter { get { return m_DecorrelationParameter; } }

        /// <summary>
        /// Ordinary Weight
        /// </summary>
        public decimal OrdinaryWeight { get { return m_OrdinaryWeight; } }

        /// <summary>
        /// Stressed Weight
        /// </summary>
        public decimal StressedWeight { get { return m_StressedWeight; } }

        /// <summary>
        /// Holding Period
        /// </summary>
        public int HoldingPeriod { get { return m_HoldingPeriod; } }

        /// <summary> 
        /// Sub Portfolio Separator (Number of markets days between evaluation date and expiry date of the physical delivery futures)
        /// </summary>
        public int SubPortfolioSeparator { get { return m_SubPortfolioSeparator; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow"></param>
        public RiskDataEuronextVarParameter()
        {
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement parameters file</param>
        public RiskDataEuronextVarParameter(EuronextVarSector pSector, IOTaskDetInOutFileRow pParsingRow)
        {
            m_OrdinaryConfidenceLevel = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "ord_cl"));
            m_StressedConfidenceLevel = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "stress_cl"));
            m_DecorrelationParameter = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "deco"));
            m_OrdinaryWeight = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "ord_w"));
            m_StressedWeight = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "stress_w"));

            if (pSector == EuronextVarSector.LGY_COM)
            {
                m_HoldingPeriod = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "hp"));
                m_SubPortfolioSeparator = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "sub"));
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Alimentation des parametres via le fichier TXT
        /// </summary>
        /// <param name="pParsingRow"></param>
        public void ParseTxtRow(IOTaskDetInOutFileRow pParsingRow)
        {
            string key = RiskDataLoad.GetRowDataValue(pParsingRow, "key");
            decimal value = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "value"));

            switch (key)
            {
                case "ordinary_confidence_level":
                    m_OrdinaryConfidenceLevel = value;
                    break;
                case "stressed_confidence_level":
                    m_StressedConfidenceLevel = value;
                    break;
                case "decorrelation_parameter":
                    m_DecorrelationParameter = value;
                    break;
                case "ordinary_weight":
                    m_OrdinaryWeight = value;
                    break;
                case "stressed_weight":
                    m_StressedWeight = value;
                    break;
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe des paramètres de calcul de livraison de commodity de la méthode Euronext Var Based
    /// </summary>
    public class RiskDataEuronextVarParameterDelivery
    {
        #region Members
        private string m_SymbolCode;
        private string m_Currency;
        private string m_Side;
        private decimal m_ExtraPercentage;
        private decimal m_MarginPercentage;
        private decimal m_FeePercentage;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Contract code
        /// </summary>
        public string SymbolCode { get { return m_SymbolCode; } }

        /// <summary> 
        /// Devise
        /// </summary>
        public string Currency { get { return m_Currency; } }

        /// <summary>
        /// Sens 'L' (Long) ou 'S' (Short)
        /// </summary>
        public string Side { get { return m_Side; } }

        /// <summary>
        /// Extra Percentage
        /// </summary>
        public decimal ExtraPercentage { get { return m_ExtraPercentage; } }

        /// <summary>
        /// Margin Percentage
        /// </summary>
        public decimal MarginPercentage { get { return m_MarginPercentage; } }

        /// <summary>
        /// Fee Percentage
        /// </summary>
        public decimal FeePercentage { get { return m_FeePercentage; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow"></param>
        public RiskDataEuronextVarParameterDelivery()
        {
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement parameters delivery file</param>
        public RiskDataEuronextVarParameterDelivery(IOTaskDetInOutFileRow pParsingRow)
        {
            m_SymbolCode = RiskDataLoad.GetRowDataValue(pParsingRow, "symbol_code");
            m_Currency = RiskDataLoad.GetRowDataValue(pParsingRow, "instr_curcy");
            m_Side = RiskDataLoad.GetRowDataValue(pParsingRow, "pos_sign");
            m_ExtraPercentage = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "extra_pct"));
            m_MarginPercentage = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "margin_pct"));
            m_FeePercentage = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "fee_pct"));
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Classe des paramètres de calcul des market calendar de la méthode Euronext Var Based
    /// </summary>
    public class RiskDataEuronextVarMarketCalendar
    {
        #region Members
        private List<DateTime> m_MarketDate;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Market Calendar Dates
        /// </summary>
        public List<DateTime> MarketDate { get { return m_MarketDate; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow"></param>
        public RiskDataEuronextVarMarketCalendar()
        {
            m_MarketDate = new List<DateTime>();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajout d'une date dans le market calendar
        /// </summary>
        /// <param name="pFieldsName">Tableau contenant le nom des données</param>
        /// <param name="pFieldsValue">Tableau contenant les valeurs des données</param>
        public void Add(string[] pFieldsName, string[] pFieldsValue)
        {
            if ((pFieldsName != default(string[])) && (pFieldsValue != default(string[])))
            {
                if (DateTime.TryParseExact(RiskDataLoadEuronextVar.GetValue(pFieldsName, pFieldsValue, "mkt_dt"), DtFunc.FmtDateyyyyMMdd, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtResult))
                {
                    if (false == m_MarketDate.Contains(dtResult))
                    {
                        m_MarketDate.Add(dtResult);
                    }
                }
            }
        }

        /// <summary>
        /// Compte le nombre de Market Date compris entre les deux dates en paramètres
        /// </summary>
        /// <param name="pDate1"></param>
        /// <param name="pDate2"></param>
        /// <returns></returns>
        public int CountInterval( DateTime pDate1, DateTime pDate2)
        {
            int interval = -1;
            if ((m_MarketDate.Count > 0) && (pDate1 != pDate2))
            {
                if (pDate1 > pDate2)
                {
                    DateTime date = pDate1;
                    pDate1 = pDate2;
                    pDate2 = date;
                }
                interval = m_MarketDate.Where(d => ((d >= pDate1) && (d <= pDate2))).Count() - 1;
            }
            return interval;
        }
        #endregion Methods
    }

    /// <summary>
    /// Jeu de données de risque Euronext Var Based
    /// </summary>
    public sealed class RiskDataEuronextVarSet
    {
        #region Members
        #region Données importées
        internal RiskDataEuronextVarParameter m_Parameters;
        internal Dictionary<string, RiskDataEuronextVarParameterDelivery> m_ParametersDelivery;
        internal Dictionary<string, RiskDataEuronextVarSerie> m_Series;
        internal Dictionary<string, RiskDataEuronextVarScePrice> m_ScenarioPrices;
        internal Dictionary<string, RiskDataEuronextVarSceDlyPrice> m_ScenarioPricesDelivery;
        internal Dictionary<string, RiskDataEuronextVarSceFx> m_ScenarioFx;
        internal Dictionary<string, RiskDataEuronextVarSceFx> m_ScenarioFxDly;
        internal RiskDataEuronextVarMarketCalendar m_MarketCalendar;
        internal DateTime m_EvaluationDate;
        #endregion Données importées
        internal EuronextVarSector m_Sector;
        //
        private RiskDataEuronextVarSerie[] m_AssetETD;
        private RiskDataEuronextVarSerie[] m_AssetBondCash;
        private (int TypeS, int TypeU) m_ScenarioCount;
        private (int TypeS, int TypeU) m_LookBackPeriod;

        private ((decimal decValue, int intValue) TypeS, (decimal decValue, int intValue) TypeU) m_ScenarioObservationCount;

        private (int TypeS, int TypeU) m_ScenarioDeliveryCount;
        private (int TypeS, int TypeU) m_LookBackPeriodDelivery;
        private ((decimal decValue, int intValue) TypeS, (decimal decValue, int intValue) TypeU) m_ScenarioDeliveryObservationCount;
        #endregion Members

        #region Accessors
        #region Données importées
        /// <summary>
        /// Paramètres de la méthode Euronext Var Based pour les Equities Euronext & Euronext Legacy
        /// </summary>
        public RiskDataEuronextVarParameter Parameters { get { return m_Parameters; } }

        /// <summary>
        /// Paramètres de livraison de la méthode Euronext Var Based pour les Commodity Euronext Legacy
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarParameterDelivery> ParametersDelivery { get { return m_ParametersDelivery; } }

        /// <summary>
        /// Series (Key = Isin Code)
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarSerie> Series { get { return m_Series; } }

        /// <summary>
        /// Scenario Prices (Key = Isin Code)
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarScePrice> ScenarioPrices { get { return m_ScenarioPrices; } }

        /// <summary>
        /// Scenario Prices de livraison de commodity (Key = Isin Code)
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarSceDlyPrice> ScenarioPricesDelivery { get { return m_ScenarioPricesDelivery; } }

        /// <summary>
        /// Scenario Exchange Rates
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarSceFx> ScenarioFx { get { return m_ScenarioFx; } }

        /// <summary>
        /// Scenario Exchange Rates for Physical Delivery
        /// </summary>
        public Dictionary<string, RiskDataEuronextVarSceFx> ScenarioFxDelivery { get { return m_ScenarioFxDly; } }

        /// <summary>
        /// Market Calendar
        /// </summary>
        public RiskDataEuronextVarMarketCalendar MarketCalendar { get { return m_MarketCalendar; } }

        /// <summary>
        /// Evaluation Date
        /// </summary>
        public DateTime EvaluationDate { get { return m_EvaluationDate; } }
        #endregion Données importées

        /// <summary>
        /// Secteur du jeu de données : Commodity ou Equity
        /// </summary>
        public EuronextVarSector Sector { get { return m_Sector; } }

        /// <summary>
        ///  Liste des assets ETD présents dans le fichier des scénarios pour lesquels il existe des positions dans Spheres (<seealso cref="RiskDataLoad.m_DataAsset"/>)
        /// </summary>
        public RiskDataEuronextVarSerie[] AssetETD { get { return m_AssetETD; } }

        /// <summary>
        /// Liste des assets bond et cash présents dans le fichier des scénarios
        /// </summary>
        public RiskDataEuronextVarSerie[] AssetBondCash { get { return m_AssetBondCash; } }

        /// <summary>
        /// Nombre de scénarios de type ordinaire de type ("S") ou de type stressé ("U") (sans le scénario contenant la current value)
        /// <para>Cette donnée est déterminée lors de l'importation du fichier scenario.</para>
        /// </summary>
        public (int TypeS, int TypeU) ScenarioCount { get { return m_ScenarioCount; } }

        /// <summary>
        /// Nombre de Look Back Period
        /// </summary>
        public (int TypeS, int TypeU) LookBackPeriod { get { return m_LookBackPeriod; } }

        /// <summary>
        /// Nombre d'observations de type ordinaire de type ("S") ou de type stréssé ("U")
        /// </summary>
        public ((decimal decValue, int intValue) TypeS, (decimal decValue, int intValue) TypeU) ScenarioObservationCount { get { return m_ScenarioObservationCount; } }

        /// <summary>
        /// Nombre de scénarios de type ordinaire de type ("S") ou de type stressé ("U") (sans le scénario contenant la current value) pour les scénarios de livraison
        /// <para>Cette donnée est déterminée lors de l'importation du fichier scenario.</para>
        /// </summary>
        public (int TypeS, int TypeU) ScenarioDeliveryCount { get { return m_ScenarioDeliveryCount; } }

        /// <summary>
        /// Nombre de Look Back Period  pour les scénarios de livraison
        /// </summary>
        public (int TypeS, int TypeU) LookBackPeriodDelivery { get { return m_LookBackPeriodDelivery; } }

        /// <summary>
        /// Nombre d'observations de type ordinaire de type ("S") ou de type stréssé ("U") pour les scénarios de livraison
        /// </summary>
        public ((decimal decValue, int intValue) TypeS, (decimal decValue, int intValue) TypeU) ScenarioDeliveryObservationCount { get { return m_ScenarioDeliveryObservationCount; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pAssetCount">Nombre d'asset pour dimensionnement</param>
        public RiskDataEuronextVarSet(EuronextVarSector pSector, int pAssetCount)
        {
            m_Sector = pSector;

            m_Parameters = new RiskDataEuronextVarParameter();
            m_ParametersDelivery = new Dictionary<string, RiskDataEuronextVarParameterDelivery>();
            m_Series = new Dictionary<string, RiskDataEuronextVarSerie>();
            m_ScenarioPrices = new Dictionary<string, RiskDataEuronextVarScePrice>(pAssetCount);
            m_ScenarioPricesDelivery = new Dictionary<string, RiskDataEuronextVarSceDlyPrice>();
            m_ScenarioFx = new Dictionary<string, RiskDataEuronextVarSceFx>(pAssetCount) ;
            m_ScenarioFxDly = new Dictionary<string, RiskDataEuronextVarSceFx>();
            m_MarketCalendar = new RiskDataEuronextVarMarketCalendar();
            m_EvaluationDate = DateTime.MinValue;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Création des tableaux d'assets car catégories d'asset
        /// </summary>
        internal void SetAssetByCategoryFromSeries()
        {
            m_AssetETD = new RiskDataEuronextVarSerie[0];
            m_AssetBondCash = new RiskDataEuronextVarSerie[0];
            if (m_Series != default(Dictionary<string, RiskDataEuronextVarSerie>))
            {
                m_AssetETD = (m_Series.Values.Where(x => x.AssetType == "F" || x.AssetType == "O")).ToArray();
                m_AssetBondCash = (m_Series.Values.Where(x => (x.AssetType == "C") || (x.AssetType == "B"))).ToArray();
            }
        }

        /// <summary>
        /// Alimente les compteurs de type de scénario ainsi que les LookBack Periods
        /// </summary>
        internal void SetScenarioCountAndLookBackPeriod()
        {
            m_ScenarioCount = default;

            if (m_ScenarioPrices.Count > 0)
            {
                string isinCode = ScenarioPrices.Keys.First();
                if (m_Sector == EuronextVarSector.EQY)
                {
                    m_ScenarioCount.TypeS = m_ScenarioPrices[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "S") && (x.Key.ReferenceDate != m_EvaluationDate)).Count();
                    m_ScenarioCount.TypeU = m_ScenarioPrices[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "U") && (x.Key.ReferenceDate != m_EvaluationDate)).Count();
                }
                else
                {
                    m_ScenarioCount.TypeS = m_ScenarioPrices[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "S")).Count();
                    m_ScenarioCount.TypeU = m_ScenarioPrices[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "U")).Count();
                }
                // Les LookBack Periods correspondent aux nombre de scénarios puisque maintenant le scénario courant est exclu du count des scénarios
                m_LookBackPeriod = m_ScenarioCount;
            }
        }

        /// <summary>
        /// Calcul du Nombre d'observations à réaliser
        /// </summary>
        /// <returns></returns>
        internal void SetScenarioObservationCount()
        {
            m_ScenarioObservationCount = default;

            if (m_Parameters != default(RiskDataEuronextVarParameter))
            {
                decimal decTypeS = m_LookBackPeriod.TypeS * (1 - m_Parameters.OrdinaryConfidenceLevel);
                int intTypeS = Convert.ToInt32(new EFS_Round(RoundingDirectionEnum.HalfDown, 0, decTypeS).AmountRounded);
                m_ScenarioObservationCount.TypeS = (decTypeS, intTypeS);

                decimal decTypeU = m_LookBackPeriod.TypeU * (1 - m_Parameters.StressedConfidenceLevel);
                int intTypeU = Convert.ToInt32(new EFS_Round(RoundingDirectionEnum.HalfDown, 0, decTypeU).AmountRounded);
                m_ScenarioObservationCount.TypeU = (decTypeU, intTypeU);

                if (m_ScenarioObservationCount.TypeS.intValue == 0)
                {
                    m_ScenarioObservationCount.TypeS.intValue = 1;
                }
                if (m_ScenarioObservationCount.TypeU.intValue == 0)
                {
                    m_ScenarioObservationCount.TypeU.intValue = 1;
                }
            }
        }

        /// <summary>
        /// Alimente les compteurs de type de scénario ainsi que les LookBack Periods pour les scénarios de livraison
        /// </summary>
        internal void SetScenarioDeliveryCountAndLookBackPeriod()
        {
            m_ScenarioDeliveryCount = default;

            if (m_ScenarioPricesDelivery.Count > 0)
            {
                string isinCode = ScenarioPricesDelivery.Keys.First();
                m_ScenarioDeliveryCount.TypeS = m_ScenarioPricesDelivery[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "S")).Count();
                m_ScenarioDeliveryCount.TypeU = m_ScenarioPricesDelivery[isinCode].ScenarioPrice.Where(x => (x.Key.Type == "U")).Count();

                // Les LookBack Periods correspondent aux nombre de scénarios puisque maintenant le scénario courant est exclu du count des scénarios
                m_LookBackPeriodDelivery = m_ScenarioDeliveryCount;
            }
        }

        /// <summary>
        /// Calcul du Nombre d'observations à réaliser pour les scénarios de livraison
        /// </summary>
        /// <returns></returns>
        internal void SetScenarioDeliveryObservationCount()
        {
            m_ScenarioDeliveryObservationCount = default;

            if ((m_Parameters != default(RiskDataEuronextVarParameter)) && (m_LookBackPeriodDelivery != default) && (m_ScenarioPricesDelivery.Count > 0))
                {
                decimal decTypeS = m_LookBackPeriodDelivery.TypeS * (1 - m_Parameters.OrdinaryConfidenceLevel);
                int intTypeS = Convert.ToInt32(new EFS_Round(RoundingDirectionEnum.HalfDown, 0, decTypeS).AmountRounded);
                m_ScenarioDeliveryObservationCount.TypeS = (decTypeS, intTypeS);

                decimal decTypeU = m_LookBackPeriodDelivery.TypeU * (1 - m_Parameters.StressedConfidenceLevel);
                int intTypeU = Convert.ToInt32(new EFS_Round(RoundingDirectionEnum.HalfDown, 0, decTypeU).AmountRounded);
                m_ScenarioDeliveryObservationCount.TypeU = (decTypeU, intTypeU);

                if (m_ScenarioDeliveryObservationCount.TypeS.intValue == 0)
                {
                    m_ScenarioDeliveryObservationCount.TypeS.intValue = 1;
                }
                if (m_ScenarioDeliveryObservationCount.TypeU.intValue == 0)
                {
                    m_ScenarioDeliveryObservationCount.TypeU.intValue = 1;
                }
            }
        }

        /// <summary>
        /// Réduire les scénarios en ne prenant que ceux nécessaires.
        /// (L'importation des scenarios pour Euronext Legacy chargeant tous les scénarios. La restrictions étant réalisé sur les chargement des référentiels)
        /// </summary>
        internal void RestrictToUseFullScenarios()
        {
            if (m_Sector != EuronextVarSector.EQY)
            {
                // Ne prendre que les scénarios dont les séries ont été importées
                IEnumerable<(string ISINCode, RiskDataEuronextVarScePrice Scenario, RiskDataEuronextVarSerie Serie)> scenarioEtSerie =
                    from scenario in m_ScenarioPrices
                    join serie in m_Series on scenario.Key equals serie.Key
                    select (scenario.Key, scenario.Value, serie.Value);

                foreach ((string ISINCode, RiskDataEuronextVarScePrice Scenario, RiskDataEuronextVarSerie Serie) element in scenarioEtSerie)
                {
                    // Alimentation de la série du scénario avec la série importée
                    element.Serie.CurrentValue = element.Scenario.m_Serie.CurrentValue;
                    element.Scenario.m_Serie = element.Serie;
                }

                m_ScenarioPrices = scenarioEtSerie.ToDictionary(s => s.ISINCode, s => s.Scenario);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de chargement des fichiers Risk Data de Euronext Var Based Method
    /// </summary>
    public class RiskDataLoadEuronextVar : RiskDataLoad
    {
        #region Members
        /// <summary>
        /// Semaphore pour la creation des RiskDataEuronextVarSet
        /// </summary>
        private readonly static SemaphoreSlim semaphoreCreateSector = new SemaphoreSlim(1, 1);

        private readonly int m_AssetETDCount;
        //// PM 20240122 [WI822] Deprecated, remplacé par m_RiskDataSet
        //private (int TypeS, int TypeU) m_ScenarioCount;
        //private (Pair<decimal, int> TypeS, Pair<decimal, int> TypeU) m_ScenarioObservationCount;
        private RiskDataEuronextVarSerie[] m_assetETD;
        private RiskDataEuronextVarSerie[] m_assetBondCash;
        private DateTime m_EvaluationDate;

        #region Données importées
        // PM 20240122 [WI822] Deprecated, remplacé par m_RiskDataSet
        //private RiskDataEuronextVarParameter m_Parameters;
        //private Dictionary<string, RiskDataEuronextVarSerie> m_Series;
        //private Dictionary<string, RiskDataEuronextVarScePrice> m_ScenarioPrices;
        //private Dictionary<string, RiskDataEuronextVarSceFx> m_ScenarioFx;

        private Dictionary<EuronextVarSector, RiskDataEuronextVarSet> m_RiskDataSet;
        #endregion Données importées
        #endregion Members

        #region Accessors
        #region Données importées
        /// <summary>
        /// Jeux de données de risque importées
        /// </summary>
        public Dictionary<EuronextVarSector, RiskDataEuronextVarSet> RiskDataSet { get { return m_RiskDataSet; } }

        // PM 20240122 [WI822] Deprecated, remplacé par RiskDataSet
        ///// <summary>
        ///// Paramètres de la méthode Euronext Var Based pour les Equities Euronext
        ///// </summary>
        //public RiskDataEuronextVarParameter Parameters { get { return m_Parameters; } }

        ///// <summary>
        ///// Series (Key = Isin Code)
        ///// </summary>
        //public Dictionary<string, RiskDataEuronextVarSerie> Series { get { return m_Series; } }

        ///// <summary>
        ///// Scenario Prices (Key = Isin Code)
        ///// </summary>
        //public Dictionary<string, RiskDataEuronextVarScePrice> ScenarioPrices { get { return m_ScenarioPrices; } }

        ///// <summary>
        ///// Scenario Exchange Rates
        ///// </summary>
        //public Dictionary<string, RiskDataEuronextVarSceFx> ScenarioFx { get { return m_ScenarioFx; } }
        #endregion Données importées

        /// <summary>
        ///  Liste des assets ETD présents dans les fichiers des scénarios pour lesquels il existe des positions dans Spheres (<seealso cref="RiskDataLoad.m_DataAsset"/>)
        /// </summary>
        public RiskDataEuronextVarSerie[] AssetETD
        {
            get { return m_assetETD; }
        }

        /// <summary>
        /// Liste des assets bond et cash présents dans les fichiers des scénarios
        /// </summary>
        public RiskDataEuronextVarSerie[] AssetBondCash
        {
            get { return m_assetBondCash; }
        }

        /// <summary>
        ///  Obtient la date du fichier des scénarios (Evaluation date)
        /// </summary>
        public DateTime EvaluationDate { get { return m_EvaluationDate; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMethodType">Nom de la méthode de calcul pour laquelle charger les données</param>
        /// <param name="pAssetETD">Assets pour lesquels charger les données</param>
        public RiskDataLoadEuronextVar(InitialMarginMethodEnum pMethodType, IEnumerable<IAssetETDIdent> pAssetETD)
            : base(pMethodType, pAssetETD)
        {
            if (pAssetETD != default(IEnumerable<IAssetETDIdent>))
            {
                m_AssetETDCount = pAssetETD.Count();
            }
            // PM 20240122 [WI822] Deprecated, remplacé par m_RiskDataSet
            //m_Series = new Dictionary<string, RiskDataEuronextVarSerie>(m_AssetETDCount);
            //m_ScenarioPrices = new Dictionary<string, RiskDataEuronextVarScePrice>(m_AssetETDCount);
            //m_ScenarioFx = new Dictionary<string, RiskDataEuronextVarSceFx>();

            m_EvaluationDate = DateTime.MinValue;
            m_RiskDataSet = new Dictionary<EuronextVarSector, RiskDataEuronextVarSet>();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Chargement des données via le RiskDataElementLoader
        /// </summary>
        /// <param name="pRiskDataLoader">Objet de chargement du flux de données d'un element d'importation</param>
        /// <returns></returns>
        public override Cst.ErrLevel LoadFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.NOTHINGTODO;
            if (pRiskDataLoader != default(RiskDataElementLoader))
            {
                // PM 20240423 [XXXXX] Séparation de la lecture des fichiers en fonction de la méthode
                if (m_MethodType == InitialMarginMethodEnum.EURONEXT_VAR_BASED)
                {
                    switch (pRiskDataLoader.TaskInput.InputSourceDataStyle)
                    {
                        case Cst.InputSourceDataStyle.EURONEXTVARPARAMETERSTXTFILE:
                            // Equity Model Parameters File ('RF01')
                            ret = LoadParametersTxtFile(EuronextVarSector.EQY, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTVARSCENARIOSFILE:
                            // Equity Instruments Scenarios Prices File ('RF02')
                            ret = LoadScenariosBorsaItalianaFile(EuronextVarSector.EQY, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTVARFOREXFILE:
                            // Equity Forex Scenarios Values File('RF03')
                            ret = LoadForexFile(EuronextVarSector.EQY, pRiskDataLoader, false);
                            break;
                        default:
                            ret = Cst.ErrLevel.NOTHINGTODO;
                            break;
                    }
                }
                else if (m_MethodType == InitialMarginMethodEnum.EURONEXT_LEGACY_VAR)
                {
                    switch (pRiskDataLoader.TaskInput.InputSourceDataStyle)
                    {
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARPARAMETERSFILE:
                            // Equity Model Parameters File ('RF01F')
                            ret = LoadParametersFile(EuronextVarSector.LGY_EQY, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARPARAMETERSFILE:
                            // Commodity Model Parameters File ('RF01C1')
                            ret = LoadParametersFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARPARAMETERSFILE:
                            // Commodity Model Parameters for Physical Delivery File ('RF01C2')
                            ret = LoadParametersDeliveryFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARSCENARIOSFILE:
                            // Equity Instruments Scenarios Prices File ('RF02F')
                            ret = LoadScenariosEuronextLegacyFile(EuronextVarSector.LGY_EQY, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARSCENARIOSFILE:
                            // Commodity Instruments Scenarios Prices File ('RF02C1')
                            ret = LoadScenariosEuronextLegacyFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARSCENARIOSFILE:
                            // Commodity Instruments Scenarios Prices for Physical Delivery File ('RF02C2')
                            ret = LoadScenariosEuronextLegacyDeliveryFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARFOREXFILE:
                            // Equity Forex Scenarios Values File('RF03F')
                            ret = LoadForexFile(EuronextVarSector.LGY_EQY, pRiskDataLoader, false);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARFOREXFILE:
                            // Commodity Forex Scenarios Values File ('RF03C1')
                            ret = LoadForexFile(EuronextVarSector.LGY_COM, pRiskDataLoader, false);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARFOREXFILE:
                            // Commodity Forex Scenarios Values for Physical Delivery File ('RF03C2')
                            ret = LoadForexFile(EuronextVarSector.LGY_COM, pRiskDataLoader, true);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                            // Equity Instruments Prices and Referential Data File ('RF04F')
                            ret = LoadReferentialDataFile(EuronextVarSector.LGY_EQY, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                            // Commodity Instruments Prices and Referential Data File ('RF04C')
                            ret = LoadReferentialDataFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        case Cst.InputSourceDataStyle.EURONEXTLEGACYVARMARKETCALENDARFILE:
                            // Market Calendar File ('RF08C')
                            ret = LoadMarketCalendarFile(EuronextVarSector.LGY_COM, pRiskDataLoader);
                            break;
                        default:
                            ret = Cst.ErrLevel.NOTHINGTODO;
                            break;
                    }
                }
                else
                {
                    ret = Cst.ErrLevel.INCORRECTPARAMETER;
                }
            }
            else
            {
                ret = Cst.ErrLevel.INCORRECTPARAMETER;
            }
            return ret;
        }

        /// <summary>
        /// Recherche d'une valeur par son nom dans un tableau de valeur 
        /// </summary>
        /// <param name="pFieldsName">Tableau des noms des champs</param>
        /// <param name="pFieldsValue">Tableau des valeurs des champs</param>
        /// <param name="pValueName">Nom de la valeurs à lire</param>
        /// <returns></returns>
        public static string GetValue(string[] pFieldsName, string[] pFieldsValue, string pValueName)
        {
            string value;
            if ((pFieldsName != default) && (pFieldsValue != default) && (pFieldsName.Length == pFieldsValue.Length) && pFieldsName.Contains(pValueName))
            {
                int index = Array.IndexOf<string>(pFieldsName, pValueName);
                value = pFieldsValue[index];
            }
            else
            {
                value = "";
            }
            return value;
        }

        /// <summary>
        /// Mise en forme des données après import
        /// </summary>
        public void DataFormatting()
        {
            if (m_RiskDataSet != default(Dictionary<EuronextVarSector, RiskDataEuronextVarSet>))
            {
                // Construction de l'ensemble des assets tout sector confondu
                m_assetETD = (from set in m_RiskDataSet.Values
                              from asset in set.AssetETD
                              select asset).ToArray();
                m_assetBondCash = (from set in m_RiskDataSet.Values
                               from asset in set.AssetBondCash
                               select asset).ToArray();

                // Calcul du nombre d'observations de chaque sector
                foreach (RiskDataEuronextVarSet set in m_RiskDataSet.Values)
                {
                    if (set != default(RiskDataEuronextVarSet))
                    {
                        // Réduire les scénarios en ne prenant que ceux nécessaires
                        set.RestrictToUseFullScenarios();

                        // Count des scénarios et des LookBackPeriods
                        set.SetScenarioCountAndLookBackPeriod();

                        // Calcul du Nombre d'observations à réaliser
                        set.SetScenarioObservationCount();

                        // Count des scénarios de livraison et des LookBackPeriods
                        set.SetScenarioDeliveryCountAndLookBackPeriod();

                        // Calcul du Nombre d'observations à réaliser pour les scénarios de livraison
                        set.SetScenarioDeliveryObservationCount();

                    }
                }
            }
        }

        #region private Methods
        /// <summary>
        /// Chargement des données du fichier Parameters format TXT ('RF01')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadParametersTxtFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result;
            //
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                    riskDataSet.Parameters.ParseTxtRow(parsingRow);
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Parameters ('RF01C1' & 'RF01F')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadParametersFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result; ;
            //
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else if ((lineNumber != 1) && (lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart) && StrFunc.IsFilled(currentLine))
                {
                    IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                    riskDataSet.m_Parameters = new RiskDataEuronextVarParameter(pSector, parsingRow);
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Parameters Delivery ('RF01C2')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadParametersDeliveryFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result; ;
            //
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber != 1) && (lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                        RiskDataEuronextVarParameterDelivery param = new RiskDataEuronextVarParameterDelivery(parsingRow);
                        string key = $"{param.SymbolCode}/{param.Side}";

                        if (false == riskDataSet.m_ParametersDelivery.ContainsKey(key))
                        {
                            riskDataSet.m_ParametersDelivery.Add(key, param);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Scenarios en lisant le nom des données sur la première ligne du fichier
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadScenariosBorsaItalianaFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result; ;
            //
            string lastISINCode = default;
            bool isToImport = false;
            RiskDataEuronextVarScePrice currentSce = default(RiskDataEuronextVarScePrice);
            string[] fieldsName = default;

            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else if ((lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        string[] fieldsValue = currentLine.Split(',');
                        // Parser les données communes aux scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
                        RiskDataEuronextVarScePrice newSce = new RiskDataEuronextVarScePrice(fieldsName, fieldsValue);

                        if (riskDataSet.m_EvaluationDate == DateTime.MinValue)
                        {
                            riskDataSet.m_EvaluationDate = newSce.EvaluationDate;
                            m_EvaluationDate = newSce.EvaluationDate;
                        }
                        if (lastISINCode != newSce.ISINCode)
                        {
                            int IdAsset = 0;
                            // Ligne sur un nouvel asset
                            lastISINCode = newSce.ISINCode;
                            isToImport = false;

                            // Vérifier qu'il n'y a pas déjà un ensemble de scénarios sur le même asset
                            if (false == riskDataSet.ScenarioPrices.TryGetValue(lastISINCode, out RiskDataEuronextVarScePrice existSce))
                            {
                                currentSce = newSce;

                                if (((newSce.Serie.AssetType == "F") || (newSce.Serie.AssetType == "O")) || (StrFunc.IsFilled(newSce.ISINCode) && StrFunc.IsEmpty(newSce.Serie.AssetType)))
                                {
                                    // Rechercher l'IdAsset des Futures et Options
                                    IdAsset = GetIdAssetETD(newSce.Serie);
                                    currentSce.Serie.IdAsset = IdAsset;
                                }

                                if ((IdAsset != 0) || (currentSce.Serie.AssetType == "C"))
                                {
                                    // Importer tous les scénarios des sous-jacent ainsi que des Futures et Options dont l'IdAsset a été trouvé
                                    isToImport = true;

                                    // Alimenter la liste des séries
                                    riskDataSet.Series.Add(lastISINCode, currentSce.Serie);

                                    // Alimenter la liste des ensembles de scénarios par contrat
                                    riskDataSet.ScenarioPrices.Add(lastISINCode, currentSce);
                                }
                            }
                            else
                            {
                                // Utiliser l'ensemble de scénarios existant
                                currentSce = existSce;
                                isToImport = true;
                            }
                        }

                        if ((isToImport) && currentSce != default)
                        {
                            // Parser la valeur du scénario et l'ajouter aux valeurs des scénarios de l'asset
                            currentSce.AddScenario(fieldsName, fieldsValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            riskDataSet.SetAssetByCategoryFromSeries();

            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier des valeurs des Scenarios ('RF02F' & 'RF02C1')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadScenariosEuronextLegacyFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result; 
            //
            string lastISINCode = default;
            RiskDataEuronextVarScePrice currentSce = default(RiskDataEuronextVarScePrice);
            bool isToImport = false;
            string[] fieldsName = default;
            //
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else if ((lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        string[] fieldsValue = currentLine.Split(',');
                        // Parser les données communes aux scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
                        RiskDataEuronextVarScePrice newSce = new RiskDataEuronextVarScePrice(fieldsName, fieldsValue, false);

                        if (lastISINCode != newSce.ISINCode)
                        {
                            // Ligne sur un nouvel asset
                            lastISINCode = newSce.ISINCode;
                            isToImport = false;

                            // Vérifier qu'il n'y a pas déjà un ensemble de scénarios sur le même asset
                            if (false == riskDataSet.ScenarioPrices.TryGetValue(lastISINCode, out RiskDataEuronextVarScePrice existSce))
                            {
                                currentSce = newSce;

                                // Importer tous les scénarios
                                isToImport = true;

                                // Alimenter la liste des enssembles de scénarios par contrat
                                riskDataSet.ScenarioPrices.Add(lastISINCode, currentSce);
                            }
                            else
                            {
                                // Utiliser l'ensemble de scénarios existant
                                currentSce = existSce;
                                isToImport = true;
                            }
                        }

                        if ((isToImport) && currentSce != default)
                        {
                            // Parser la valeur du scénario et l'ajouter aux valeurs des scénarios de l'asset
                            RiskDataEuronextVarScenario scenarioValues = currentSce.AddScenario(fieldsName, fieldsValue);

                            if ((riskDataSet.m_EvaluationDate == DateTime.MinValue) && (scenarioValues.Key.Type == "C"))
                            {
                                riskDataSet.m_EvaluationDate = scenarioValues.Key.ReferenceDate;
                                m_EvaluationDate = scenarioValues.Key.ReferenceDate;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            return ret;
        }
        /// <summary>
        /// Chargement des données du fichier des valeurs des Scenarios pour la livraison de commodity ('RF02C2')
        /// </summary>
        /// <param name="pSector">Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadScenariosEuronextLegacyDeliveryFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result;
            //
            string lastISINCode = default;
            RiskDataEuronextVarSceDlyPrice currentSce = default(RiskDataEuronextVarSceDlyPrice);
            string[] fieldsName = default;
            //
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else if ((lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        string[] fieldsValue = currentLine.Split(',');
                        // Parser les données communes aux scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
                        RiskDataEuronextVarSceDlyPrice newSce = new RiskDataEuronextVarSceDlyPrice(fieldsName, fieldsValue);

                        if (lastISINCode != newSce.ISINCode)
                        {
                            // Ligne sur un nouvel asset
                            lastISINCode = newSce.ISINCode;

                            // Vérifier qu'il n'y a pas déjà un ensemble de scénarios sur le même asset
                            if (false == riskDataSet.ScenarioPricesDelivery.TryGetValue(lastISINCode, out RiskDataEuronextVarSceDlyPrice existSce))
                            {
                                currentSce = newSce;

                                // Alimenter la liste des ensembles de scénarios par contrat
                                riskDataSet.ScenarioPricesDelivery.Add(lastISINCode, currentSce);
                            }
                            else
                            {
                                // Utiliser l'ensemble de scénarios existant
                                currentSce = existSce;
                            }
                        }

                        if (currentSce != default(RiskDataEuronextVarSceDlyPrice))
                        {
                            // Parser la valeur du scénario et l'ajouter aux valeurs des scénarios de l'asset
                            RiskDataEuronextVarScenario scenarioValues = currentSce.AddScenario(fieldsName, fieldsValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Forex scenario values ('RF03' & 'RF03C1' & 'RF03F')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <param name="pIsForDelivery">Indique s'il s'agit des Forex pour la livraison physique de commodity</param>
        /// <returns></returns>
        private Cst.ErrLevel LoadForexFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader, bool pIsForDelivery = false)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result;
            Dictionary<string, RiskDataEuronextVarSceFx> scenarioFx = new Dictionary<string, RiskDataEuronextVarSceFx>();
            //
            string[] fieldsName = default;
            string lastKey = default;
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else if (lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart && (false == currentLine.EndsWith("NaN")))
                    {
                        string[] fieldsValue = currentLine.Split(',');

                        // Parser les données communes d'un même couple de devises (ne parse donc pas les taux des scénarios)
                        RiskDataEuronextVarSceFx fx = new RiskDataEuronextVarSceFx(fieldsName, fieldsValue);
                        string key = $"{fx.BaseCurrency}/{fx.CounterCurrency}";

                        if (lastKey != key)
                        {
                            lastKey = key;
                            scenarioFx.Add($"{key}", fx);
                        }
                        else
                        {
                            fx = scenarioFx[key];
                        }

                        // Parser la valeur du taux du couple de devises et l'ajouter aux taux des scénarios
                        fx.AddFx(fieldsName, fieldsValue);
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            if (pIsForDelivery)
            {
                riskDataSet.m_ScenarioFxDly = scenarioFx;
            }
            else
            {
                riskDataSet.m_ScenarioFx = scenarioFx;
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Instrument prices & referential data ('RF04C' & 'RF04F')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadReferentialDataFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result;
            //
            string[] fieldsName = default;
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else
                    {
                        string[] fieldsValue = currentLine.Split(',');
                        RiskDataEuronextVarSerie serie = RiskDataEuronextVarSerie.ParseEuronextLegacySerie(fieldsName, fieldsValue, pSector);
                        int IdAsset = 0;

                        if ((serie.AssetType == "F") || (serie.AssetType == "O"))
                        {
                            // Rechercher l'IdAsset des Futures et Options
                            IdAsset = GetIdAssetETD(serie);
                            serie.IdAsset = IdAsset;
                        }

                        if ((IdAsset != 0) || (serie.AssetType == "B") || (serie.AssetType == "C"))
                        {
                            // Importer tous les scénarios des sous-jacent ainsi que des Futures et Options dont l'IdAsset a été trouvé
                            // Alimenter la liste des séries
                            riskDataSet.m_Series.Add(serie.IsinCode, serie);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            riskDataSet.SetAssetByCategoryFromSeries();

            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier Market calendar ('RF08C')
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadMarketCalendarFile(EuronextVarSector pSector, RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataEuronextVarSet riskDataSet = CreateSectorRiskDataSetAsync(pSector).Result;
            //
            string[] fieldsName = default;
            int lineNumber = 0;
            int guard = 999999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                try
                {
                    if (currentLine == null)
                    {
                        break;
                    }
                    else if ((lineNumber == 1) && StrFunc.IsFilled(currentLine))
                    {
                        // Lecture du nom des données
                        fieldsName = currentLine.Split(',');
                    }
                    else
                    {
                        string[] fieldsValue = currentLine.Split(',');
                        riskDataSet.m_MarketCalendar.Add(fieldsName, fieldsValue);
                    }
                }
                catch (Exception ex)
                {
                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on importing line :" + lineNumber.ToString(), ex);
                    Logger.Log(new LoggerData(LogEx, new LogParam[] { new LogParam(currentLine) }));
                }
            }

            return ret;
        }

        #region Deprecated Methods
        ///// <summary>
        ///// Chargement des données du fichier Forex  en utilisant le Parsing I/O
        ///// </summary>
        ///// <param name="pRiskDataLoader"></param>
        ///// <returns></returns>
        //private Cst.ErrLevel LoadForexFileWithParsingIO(RiskDataElementLoader pRiskDataLoader)
        //{
        //    Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

        //    m_ScenarioFx = new Dictionary<string, RiskDataEuronextVarSceFx>();

        //    string lastKey = default;

        //    int lineNumber = 0;
        //    int guard = 999999999;
        //    while (lineNumber++ < guard)
        //    {
        //        string currentLine = pRiskDataLoader.StreamReader.ReadLine();
        //        if (currentLine == null)
        //        {
        //            break;
        //        }
        //        else if (lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart && (false == currentLine.EndsWith("NaN")))
        //        {
        //            IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
        //            // Parser les données communes d'un même couple de devises (ne parse donc pas les taux des scénarios)
        //            RiskDataEuronextVarSceFx fx = new RiskDataEuronextVarSceFx(parsingRow);
        //            string key = $"{fx.BaseCurrency}/{fx.CounterCurrency}";

        //            if (lastKey != key)
        //            {
        //                lastKey = key;
        //                m_ScenarioFx.Add($"{key}", fx);
        //            }
        //            else
        //            {
        //                fx = m_ScenarioFx[key];
        //            }
        //            // Parser la valeur du taux du couple de devises et l'ajouter aux taux des scénarios
        //            fx.AddFx(parsingRow);
        //        }
        //    }
        //    return ret;
        //}

        ///// <summary>
        ///// Chargement des données du fichier Scenarios en utilisant le Parsing I/O
        ///// </summary>
        ///// <param name="pRiskDataLoader"></param>
        ///// <returns></returns>
        //private Cst.ErrLevel LoadScenariosFileWithParsingIO(RiskDataElementLoader pRiskDataLoader)
        //{
        //    Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

        //    m_ScenarioPrices = new Dictionary<string, RiskDataEuronextVarScePrice>(m_AssetETDCount);

        //    string lastISINCode = default;
        //    bool isToImport = false;
        //    RiskDataEuronextVarScePrice currentSce = default;
        //    //
        //    int lineNumber = 0;
        //    int guard = 999999999;
        //    while (lineNumber++ < guard)
        //    {
        //        string currentLine = pRiskDataLoader.StreamReader.ReadLine();
        //        if (currentLine == null)
        //        {
        //            break;
        //        }
        //        else if (lineNumber > pRiskDataLoader.TaskInput.NbOmitRowStart)
        //        {
        //            IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
        //            // Parser les données communes aux scénarios d'un même asset (ne parse donc pas les valeurs des scénarios)
        //            RiskDataEuronextVarScePrice newSce = new RiskDataEuronextVarScePrice(parsingRow);

        //            if (m_EvaluationDate == DateTime.MinValue)
        //            {
        //                m_EvaluationDate = newSce.EvaluationDate;
        //            }
        //            if (lastISINCode != newSce.ISINCode)
        //            {
        //                int IdAsset = 0;
        //                // Ligne sur un nouvel asset
        //                lastISINCode = newSce.ISINCode;
        //                isToImport = false;

        //                // Vérifier qu'il n'y a pas déjà un ensemble de scénarios sur le même asset
        //                if (false == m_ScenarioPrices.TryGetValue(lastISINCode, out RiskDataEuronextVarScePrice existSce))
        //                {
        //                    currentSce = newSce;

        //                    if (((newSce.Serie.AssetType == "F") || (newSce.Serie.AssetType == "O")) || (StrFunc.IsFilled(newSce.ISINCode) && StrFunc.IsEmpty(newSce.Serie.AssetType)))
        //                    {
        //                        // Rechercher l'IdAsset des Futures et Options
        //                        IdAsset = GetIdAssetETD(newSce.Serie);
        //                        currentSce.Serie.IdAsset = IdAsset;
        //                    }

        //                    if ((IdAsset != 0) || (currentSce.Serie.AssetType == "C"))
        //                    {
        //                        // Importer tous les scénarios des sous-jacent ainsi que des Futures et Options dont l'IdAsset a été trouvé
        //                        isToImport = true;

        //                        // Alimenter la liste des séries
        //                        m_Series.Add(lastISINCode, currentSce.Serie);

        //                        // Alimenter la liste des ensembles de scénarios par contrat
        //                        m_ScenarioPrices.Add(lastISINCode, currentSce);
        //                    }
        //                }
        //                else
        //                {
        //                    // Utiliser l'ensemble de scénarios existant
        //                    currentSce = existSce;
        //                    isToImport = true;
        //                }
        //            }

        //            if ((isToImport) && currentSce != default)
        //            {
        //                // Parser la valeur du scénario et l'ajouter aux valeurs des scénarios de l'asset
        //                currentSce.AddScenario(parsingRow);
        //            }
        //        }
        //    }

        //    if (m_Series != default)
        //    {
        //        m_assetETD = (m_Series.Values.Where(x => x.AssetType == "F" || x.AssetType == "O")).ToArray();
        //        m_assetCash = (m_Series.Values.Where(x => x.AssetType == "C")).ToArray();
        //    }

        //    return ret;
        //}
        #endregion Deprecated Methods

        /// <summary>
        /// Rechercher l'Id Asset d'une serie
        /// </summary>
        /// <param name="pSerie">Données de la série</param>
        /// <param name="pIsWithISINOnly">Indique s'il faut uniquement rechercher l'asset via le Code ISIN</param>
        /// <returns></returns>
        private int GetIdAssetETD(RiskDataEuronextVarSerie pSerie, bool pIsWithISINOnly = false)
        {
            MarketAssetETDRequestSettings settings = EuronextVarTools.GetAssetETDRequestSettings(true, true);
            MarketAssetETDRequest request = EuronextVarTools.GetAssetETDRequest(pSerie);

            // Rechercher l'Id de l'asset via le Code Isin
            IAssetETDIdent asset = m_DataAsset.GetAsset(settings, request);

            if ((default(IAssetETDIdent) == asset) && (StrFunc.IsFilled(pSerie.ContractSymbol)) && (false == pIsWithISINOnly))
            {
                settings = EuronextVarTools.GetAssetETDRequestSettings(false);

                // Rechercher l'Id de l'asset via les caractéristiques de l'asset
                asset = m_DataAsset.GetAsset(settings, request);
            }

            return ((default(IAssetETDIdent) != asset) ? asset.IdAsset : 0);
        }

        /// <summary>
        /// Crée une jeux de données de risques s'il n'existe pas déjà
        /// </summary>
        /// <param name="pSector">Equity (EQY) ou Commodity (COM)</param>
        private async Task<RiskDataEuronextVarSet> CreateSectorRiskDataSetAsync(EuronextVarSector pSector)
        {
            RiskDataEuronextVarSet riskDataSet;
            await semaphoreCreateSector.WaitAsync();
            try
            {
                if (false == m_RiskDataSet.TryGetValue(pSector, out riskDataSet))
                {
                    riskDataSet = new RiskDataEuronextVarSet(pSector, m_AssetETDCount);
                    m_RiskDataSet.Add(pSector, riskDataSet);
                }
            }
            finally
            {
                semaphoreCreateSector.Release();
            }
            return riskDataSet;
        }
        #endregion private Methods
        #endregion Methods
    }
}