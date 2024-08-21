#region using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Messaging;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using EFS.Common.Acknowledgment;
using EFS.Common.Log;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
#endregion using directives

namespace EFS.Common.MQueue
{
    #region Quote
    /// <summary>
    /// 
    /// </summary>
    // EG 20190716 [VCL : New FixedIncome] Add assetmeasure, quoteunit, idc, idm, source
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_Index))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_Equity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_RateIndex))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_FxRate))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_ETDAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Quote_DebtSecurityAsset))]
    [Serializable]
    public abstract class Quote
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("action")]
        public string action;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idquote")]
        public int idQuote;
        [System.Xml.Serialization.XmlElementAttribute("idmarketenv")]
        public string idMarketEnv;
        [System.Xml.Serialization.XmlElementAttribute("idvalscenario")]
        public string idValScenario;
        [System.Xml.Serialization.XmlElementAttribute("idasset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("idasset_identifier")]
        public string idAsset_Identifier;
        [System.Xml.Serialization.XmlElementAttribute("time")]
        public DateTime time;
        [System.Xml.Serialization.XmlElementAttribute("idbc")]
        public string idBC;
        [System.Xml.Serialization.XmlElementAttribute("quoteside")]
        public string quoteSide;
        [System.Xml.Serialization.XmlElementAttribute("quotetiming")]
        public string quoteTiming;
        [System.Xml.Serialization.XmlElementAttribute("cashflowtype")]
        public string cashFlowType;
        [System.Xml.Serialization.XmlElementAttribute("assetmeasure")]
        public string assetMeasure;
        [System.Xml.Serialization.XmlElementAttribute("quoteunit")]
        public string quoteUnit;
        [System.Xml.Serialization.XmlElementAttribute("idc")]
        public string idC;
        [System.Xml.Serialization.XmlElementAttribute("idm")]
        public Nullable<int> idM;
        [System.Xml.Serialization.XmlElementAttribute("source")]
        public string source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("value")]
        public decimal value;

        [System.Xml.Serialization.XmlElementAttribute("eventClass")]
        public string eventClass = EventClassFunc.Fixing;
        // EG 20140711 (provient de Quote_ETDAsset)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isEODSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isEOD")]
        public bool isEOD;

        // EG 20170510 [23153] new (en provenance de Quote_ETDAsset)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eodComplementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eodComplement")]
        public Quote_EODComplement eodComplement;


        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient ou Définit la table de stockage des prix
        /// </summary>
        public virtual string QuoteTable
        {
            get { return "N/A"; }
            set { ;}
        }

        /// <summary>
        ///  Obtient true si ASSET_ETD, DERIVATIVEATTRIB, ou DERIVATIVECONTRACT
        /// </summary>
        public virtual bool IsQuoteTable_ETD
        {
            get { return false; }
        }

        /// <summary>
        /// Obtient le type de cotation (Bid, Mid, Ask, OfficialClose, OfficialSettlement, etc..)
        /// <para>Obtient null si non renseigné</para>
        /// </summary>
        public Nullable<QuotationSideEnum> QuoteSide
        {
            get
            {
                return QuoteType;
            }
        }

        /// <summary>
        /// Obtient le type de cotation (Bid, Mid, Ask, OfficialClose, OfficialSettlement, etc..)
        /// <para>Obtient null si non renseigné</para>
        /// </summary>
        public Nullable<QuotationSideEnum> QuoteType
        {
            get
            {
                Nullable<QuotationSideEnum> ret = null;
                if (StrFunc.IsFilled(this.quoteSide))
                {
                    ret = QuotationSideEnum.OfficialClose;
                    if (Enum.IsDefined(typeof(QuotationSideEnum), quoteSide))
                        ret = (QuotationSideEnum)Enum.Parse(typeof(QuotationSideEnum), quoteSide);
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient le timing de la cotation (close, Open , Max, etc..)
        /// <para>Obtient null si non renseigné</para>
        /// </summary>
        public Nullable<QuoteTimingEnum> QuoteTiming
        {
            get
            {
                Nullable<QuoteTimingEnum> timing = null;
                //
                if (StrFunc.IsFilled(this.quoteTiming))
                {
                    timing = QuoteTimingEnum.Close;
                    if (Enum.IsDefined(typeof(QuoteTimingEnum), quoteTiming))
                        timing = (QuoteTimingEnum)Enum.Parse(typeof(QuoteTimingEnum), quoteTiming);

                }
                return timing;
            }
        }

        public virtual Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.ExchangeTradedContract;
            }
        }
        /// <summary>
        /// Obtient la mesure de la cotation
        /// <para>Obtient null si non renseigné</para>
        /// </summary>
        // EG 20190716 [VCL : New FixedIncome] New
        public Nullable<AssetMeasureEnum> AssetMeasure
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<AssetMeasureEnum>(assetMeasure); 
            }
        }
        /// <summary>
        /// Obtient l'unité de la cotation
        /// <para>Obtient null si non renseigné</para>
        /// </summary>
        // EG 20190716 [VCL : New FixedIncome] New
        public Nullable<Cst.PriceQuoteUnits> QuoteUnit
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<Cst.PriceQuoteUnits>(quoteUnit);
            }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Recherche de la cotation
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// EG 20150515 [20513] Utilisation de VW_ASSET_DEBTSECURITY dans le cas des Bonds
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190716 [VCL : New FixedIncome] Add ASSETMEASURE, IDC, IDM parameters
        public void SetIdQuote(string pCs, IDbTransaction pDbTransaction)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDASSET", DbType.Int32), idAsset);
            parameters.Add(new DataParameter(pCs, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), StrFunc.IsFilled(idMarketEnv) ? idMarketEnv : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), StrFunc.IsFilled(idValScenario) ? idValScenario : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "CASHFLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), StrFunc.IsFilled(cashFlowType) ? cashFlowType : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), StrFunc.IsFilled(quoteSide) ? quoteSide : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), StrFunc.IsFilled(quoteTiming) ? quoteTiming : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), StrFunc.IsFilled(idBC) ? idBC : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "TIME", DbType.DateTime), time);
            parameters.Add(new DataParameter(pCs, "ASSETMEASURE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), StrFunc.IsFilled(assetMeasure) ? assetMeasure : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), StrFunc.IsFilled(idC) ? idC : Convert.DBNull);
            parameters.Add(new DataParameter(pCs, "IDM", DbType.Int32), idM ?? Convert.DBNull);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "IDQUOTE_H, IDENTIFIER" + SQLCst.FROM_DBO + QuoteTable + " quote" + Cst.CrLf;
            if (UnderlyingAsset == Cst.UnderlyingAsset.Bond)
            {
                sql += SQLCst.INNERJOIN_DBO + "VW_ASSET_DEBTSECURITY asset";
            }
            else
            {
                sql += SQLCst.INNERJOIN_DBO + QuoteTable.Replace("QUOTE_", "ASSET_").Replace("_H", String.Empty) + " asset";
            }
            sql += SQLCst.ON + "(asset.IDASSET = quote.IDASSET)" + Cst.CrLf;
            DataParameter[] arrayParameters = parameters.GetArrayParameter();
            for (int i = 0; i < arrayParameters.Length; i++)
            {
                string paramKey = arrayParameters[i].ParameterKey;

                if (i == 0)
                    sql += SQLCst.WHERE + Cst.CrLf;
                else
                    sql += SQLCst.AND + Cst.CrLf;
                //
                if (false == arrayParameters[i].IsNullValue)
                {
                    sql += "quote." + paramKey + "=@" + paramKey;
                }
                else
                {
                    sql += "quote." + paramKey + SQLCst.IS_NULL;
                    parameters.Remove(paramKey);
                }
                //
                sql += Cst.CrLf;
            }

            using (IDataReader dr = DataHelper.ExecuteReader(pCs, pDbTransaction, CommandType.Text, 
                sql.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    idQuote = Convert.ToInt32(dr["IDQUOTE_H"]);
                    idQuoteSpecified = true;
                    idAsset_Identifier = dr["IDENTIFIER"].ToString();
                }
            }
        }
        #endregion Methods
    }
    /// <summary>
    /// Représente une cotation d'une action
    /// </summary>
    [Serializable]
    public class Quote_Equity : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return Cst.OTCml_TBL.QUOTE_EQUITY_H.ToString(); }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.EquityAsset;
            }
        }
        #endregion Accessors
    }

    [Serializable]
    public class Quote_Index : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return "QUOTE_INDEX_H"; }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.Index;
            }
        }

        #endregion Accessors
    }

    [Serializable]
    public class Quote_FxRate : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return "QUOTE_FXRATE_H"; }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.FxRateAsset;
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Représente une cotation d'un asset de taux
    /// </summary>
    [Serializable]
    public class Quote_SimpleIRSwap : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return Cst.OTCml_TBL.QUOTE_SIMPLEFRA_H.ToString(); }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.SimpleIRSwap;
            }
        }

        #endregion Accessors
    }

    /// <summary>
    /// Représente une cotation d'un asset de taux
    /// </summary>
    [Serializable]
    public class Quote_RateIndex : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return Cst.OTCml_TBL.QUOTE_RATEINDEX_H.ToString(); }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.RateIndex;
            }
        }

        #endregion Accessors
    }

    /// <summary>
    /// Représente une cotation d'un titre
    /// </summary>
    [Serializable]
    public class Quote_DebtSecurityAsset : Quote
    {
        #region Accessors
        public override string QuoteTable
        {
            get { return Cst.OTCml_TBL.QUOTE_DEBTSEC_H.ToString(); }
        }
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.Bond;
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Quote_SecurityAsset : Quote
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idE_Source")]
        public int idE_Source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitCouponSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitCoupon")]
        public decimal unitCoupon;
        #endregion Members
        #region Accessors
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.EquityAsset;
            }
        }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// <para>Lecture d'une cotation ETD</para>
    /// <para>
    ///  Classe également utilisée pour activée QuotationHandling (post) lorsque
    ///  <para>- Création, Modification, Suppression d'une cotation que asset ETD</para> 
    ///  <para>- Modification contractMultiplier d'un derivative contract</para> 
    ///  <para>- Modification contractMultiplier d'une échéance</para> 
    ///  </para>
    /// </summary>
    /// FI 20170306 [22225] Modify
    /// 
    [Serializable]
    public class Quote_ETDAsset : Quote
    {
        #region Members
        /// <summary>
        ///  type de donnée  
        /// </summary>
        private string _quoteTable = "QUOTE_ETD_H";

        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        /// <summary>
        /// Représente le contract Multiplier associé avec la cotation (présent si création, modification, suppression de cotation uniquement)
        /// </summary>
        [XmlElementAttribute("contractMultiplier")]
        public decimal contractMultiplier;

        [XmlIgnoreAttribute()]
        public bool deltaSpecified;
        /// <summary>
        /// Delta de la cotation
        /// </summary>
        [XmlElementAttribute("delta")]
        public decimal delta;

        [XmlIgnoreAttribute()]
        public bool timeSpecified;



        [XmlIgnoreAttribute()]
        public bool idDCSpecified;
        /// <summary>
        /// Réprésente le derivative contrat  (présent si modification du contractMultiplier)
        /// </summary>
        [XmlElementAttribute("idDC")]
        public int idDC;

        [XmlIgnoreAttribute()]
        public bool idDC_IdentifierSpecified;
        /// <summary>
        /// Réprésente l'identifiant du derivative (présent si modification du contractMultiplier)
        /// </summary>
        [XmlElementAttribute("idDC_identifier")]
        public string idDC_Identifier;

        [XmlIgnoreAttribute()]
        public bool idDerivativeAttribSpecified;
        /// <summary>
        /// Réprésente l'échéance modifié (présent si modification du contractMultiplier sur l'échéance)
        /// </summary>
        /// PM 20151027 [20964] Ajout gestion de la modification du ContractMultiplier sur DERIVATIVEATTRIB
        [XmlElementAttribute("idDerivativeAttrib")]
        public int idDerivativeAttrib;

        [XmlIgnoreAttribute()]
        public bool isCashFlowsValSpecified;
        /// <summary>
        /// <para>Cet indicateur vaut true si</para>
        /// <para>- Modification du contractMultiplier sur une cotation</para>
        /// <para>- Suppression d'une cotation avec contractMultiplier de renseigné</para>
        /// <para>- Ajout d'une cotation avec contractMultiplier de renseigné</para>
        /// <para>- Modification du contractMultiplier sur un asset, une échéance, ou un Derivative Contract</para>
        /// </summary>
        /// FI 20170306 [22225] Mise en place du commentaire (Il faudra faire un rename de ce membre, je n'ose pas juste avant une livraison)
        [XmlElementAttribute("isCashFlowsVal")]
        public bool isCashFlowsVal;
        // EG 20170510 [23153] Remove (See Quote)
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool eodComplementSpecified;
        // EG 20170324 [22991] new informations complémentaires utiles au Calcul des cash-flows 
        //[System.Xml.Serialization.XmlElementAttribute("eodComplement")]
        //public Quote_EODComplement eodComplement;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Type de donnée
        /// </summary>
        [XmlIgnoreAttribute()]
        public override string QuoteTable
        {
            get { return _quoteTable; }
            set { _quoteTable = value; }
        }
        /// <summary>
        /// Otient true si _quoteTable vaut DERIVATIVECONTRACT ou ASSET_ETD ou DERIVATIVEATTRIB
        /// </summary>
        /// PM 20151027 [20964] Ajout DERIVATIVEATTRIB
        [XmlIgnoreAttribute()]
        public override bool IsQuoteTable_ETD
        {
            get
            {
                //return ((_quoteTable == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()) ||
                //        (_quoteTable == Cst.OTCml_TBL.ASSET_ETD.ToString()));
                return ((_quoteTable == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()) ||
                        (_quoteTable == Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString()) ||
                        (_quoteTable == Cst.OTCml_TBL.ASSET_ETD.ToString()));
            }
        }
        [XmlIgnoreAttribute()]
        public override Cst.UnderlyingAsset UnderlyingAsset
        {
            get
            {
                return Cst.UnderlyingAsset.ExchangeTradedContract;
            }
        }
        #endregion Accessors
    }

    // EG 20170324 [22991] new 
    /// <summary>
    /// Classe utilisée pour les informations complémentaires utiles au Calcul des cash-flows 
    /// appelé via le traitement EOD
    /// </summary>
    [Serializable]
    public class Quote_EODComplement
    {
        /// <summary>
        /// Entité concerné
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("entity")]
        public int idAEntity;
        /// <summary>
        /// Détermine si le book du Dealer gère la tenue de position (présent si appel par le traitement EOD)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("posKeeping")]
        public bool isPosKeeping_BookDealer;
        /// <summary>
        /// La quantity disponible veille et actions du jour est connue via la requête de sélection des trades candidats
        /// à calcul des cash-flows on la passe dans le message pour éviter une exécution supplémentaire dans EventsVal 
        /// pour alimentater la variable (m_PosQuantityPrevAndActionsDay)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("qty")]
        public decimal posQuantityPrevAndActionsDay;
    }

    /// <summary>
    /// Représente une cotation d'un asset non géré
    /// </summary>
    [Serializable] 
    public class Quote_ToDefine : Quote
    {
    }
    #endregion Quote

    #region Maturity
    public class Maturity
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("action")]
        public string action;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMaturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("idMaturity")]
        public int idMaturity;
        
        [System.Xml.Serialization.XmlElementAttribute("maturityMonthYear")]
        public string maturityMonthYear;
        
        [System.Xml.Serialization.XmlElementAttribute("idMaturityRule")]
        public int idMaturityRule;
        
        [System.Xml.Serialization.XmlElementAttribute("idMaturityRule_identifier")]
        public string idMaturityRule_identifier;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturityDate")]
        public DateTime maturityDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturityTime")]
        public string maturityTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastTradingDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastTradingDay")]
        public DateTime lastTradingDay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastTradingTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastTradingTime")]
        public string lastTradingTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryDate")]
        public DateTime deliveryDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstDeliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstDeliveryDate")]
        public DateTime firstDeliveryDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastDeliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastDeliveryDate")]
        public DateTime lastDeliveryDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstDlvSettltDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstDlvSettltDate")]
        public DateTime firstDlvSettltDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastDlvSettltDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastDlvSettltDate")]
        public DateTime lastDlvSettltDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool periodMltpDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodMltpDelivery")]
        public int periodMltpDelivery;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool periodDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodDelivery")]
        public string periodDelivery;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayTypeDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayTypeDelivery")]
        public string dayTypeDelivery;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rollConvDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("rollConvDelivery")]
        public string rollConvDelivery;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimeStartSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimeStart")]
        public string deliveryTimeStart;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimeEndSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimeEnd")]
        public string deliveryTimeEnd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimeZoneSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimeZone")]
        public string deliveryTimeZone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isApplySummerTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isApplySummerTime")]
        public bool isApplySummerTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool periodMltpDlvSettltSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodMltpDlvSettlt")]
        public int periodMltpDlvSettlt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool periodDlvSettltSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodDlvSettlt")]
        public string periodDlvSettlt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayTypeDlvSettltSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayTypeDlvSettlt")]
        public string dayTypeDlvSettlt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settltOfHolidayDlvConventionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settltOfHolidayDlvConvention")]
        public string settltOfHolidayDlvConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstNoticeDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstNoticeDay")]
        public DateTime firstNoticeDay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastNoticeDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastNoticeDay")]
        public DateTime lastNoticeDay;
        #endregion Members

        #region Methods
        // EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        public void SetIdMaturity(string pCs, IDbTransaction pDbTransaction)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDMATURITYRULE", DbType.Int32), idMaturityRule);
            parameters.Add(new DataParameter(pCs, "MATURITYMONTHYEAR", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), maturityMonthYear);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "IDMATURITY" +Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITY.ToString() + Cst.CrLf;
            sql += SQLCst.WHERE + "IDMATURITYRULE=@IDMATURITYRULE" + SQLCst.AND + "MATURITYMONTHYEAR=@MATURITYMONTHYEAR";

            object obj = DataHelper.ExecuteScalar(pCs, pDbTransaction, CommandType.Text, sql.ToString(), parameters.GetArrayDbParameter());

            if (null != obj)
            {
                idMaturity = Convert.ToInt32(obj);
                idMaturitySpecified = true;
            }
        }
        #endregion
    }
    #endregion Quote

    #region NormMsgBuildingInfo
    /// <summary>
    /// Elements de construction des messages normalisés
    /// Exemple: 
    /// . id et/ou identifier sont renseignés par un IDTASK et/ou son IDENTIFIER pour la construction d'un message I/O
    /// . des paramètres doivent être passés en complément en fonction du message à construire
    /// 
    /// </summary>
    /// EG 20140506 [19913] New Add scripts
    /// EG 20150218 New Add gProduct (for RTS = string.Empty, for ETD = Cst.ProductGProduct_FUT)
    [Serializable]
    public class NormMsgBuildingInfo
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("processType")]
        public Cst.ProcessTypeEnum processType;
        [System.Xml.Serialization.XmlElementAttribute("posRequestType")]
        public Cst.PosRequestTypeEnum posRequestType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool posRequestTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("gProduct")]
        public string gProduct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gProductSpecified;
        [System.Xml.Serialization.XmlElementAttribute("id")]
        public int id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parameters")]
        public MQueueparameters parameters;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool parametersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("scripts")]
        public MQueueScripts scripts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scriptsSpecified;
        #endregion Members
    }
    #endregion NormMsgBuildingInfo
    #region MQueueScripts
    /// <summary>
    /// Scripts SQL envoyé à NormMsqgFactory (CA only)
    /// </summary>
    /// EG 20140506 [19913] New 
    [System.Xml.Serialization.XmlRoot("scripts")]
    public class MQueueScripts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("script")]
        public MQueueScript[] script;
        #endregion Members
    }
    #endregion MQueueScripts
    #region MQueueScript
    /// <summary>
    /// Script utilisé dans la publication des CA pour exécution dans le traitement NORMMSGFACTORY et CLOSINGDAY
    /// </summary>
    /// EG 20140506 [19913] New 
    [Serializable]
    public class MQueueScript
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("docName")]
        public string docName;
        [System.Xml.Serialization.XmlElementAttribute("docType")]
        public string docType;
        [System.Xml.Serialization.XmlElementAttribute("runTime")]
        public string runTime;
        [System.Xml.Serialization.XmlElementAttribute("script")]
        public byte[] script;
        #endregion Members
    }
    #endregion MQueueScript


    #region AccountGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("account", IsNullable = false)]
    public class AccountGenMQueue : MQueueBase
    {
        #region Accessors
        #region accountDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime AccountDate
        {
            get { return GetMasterDate(); }
        }
        #endregion accountDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.ACCOUNTGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public AccountGenMQueue() { }
        public AccountGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }
    #endregion AccountGenMQueue
    #region AccrualsGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("accrualsGen", IsNullable = false)]
    public class AccrualsGenMQueue : EventsValMQueue
    {
        #region Accessors
        #region accrualDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime AccrualDate
        {
            get { return GetMasterDate(); }
        }
        #endregion accrualDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get
            {
                return Cst.ProcessTypeEnum.ACCRUALSGEN;
            }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public AccrualsGenMQueue() { }
        public AccrualsGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }
    #endregion AccrualsGenMQueue
    

    #region CashBalanceInterestMQueue
    /// <summary>
    /// 
    /// </summary>
    /// PM 20120801 [18085]
    [System.Xml.Serialization.XmlRootAttribute("cashBalanceInterest", IsNullable = false)]
    public class CashBalanceInterestMQueue : MQueueBase
    {
        #region Constants
        /// <summary>
        /// Représente le type d'intérêt (sur solde ou Deposit)
        /// </summary>
        public const string PARAM_AMOUNTTYPE = "AMOUNTTYPE";
        /// <summary>
        /// Représente la devise sur laquelle calcul les intérêts
        /// </summary>
        public const string PARAM_IDC = "IDC";
        /// <summary>
        /// Représente la periode de calcul les intérêts
        /// </summary>
        public const string PARAM_PERIOD = "PERIOD";
        /// <summary>
        /// Représente le multiplicateur de periode de calcul les intérêts
        /// </summary>
        public const string PARAM_PERIODMLTP = "PERIODMLTP";
        #endregion Constants
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.CASHINTEREST; }
        }

        /// <summary>
        ///  Obtient False puisque l'Id est soit un book, soit une partie
        /// </summary>
        public override bool IsIdT
        {
            get
            {
                return false;
            }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public CashBalanceInterestMQueue() { }
        public CashBalanceInterestMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override void SpecificParameters()
        {
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[PARAM_DATE1])))
                AddParameter(PARAM_DATE1, OTCmlHelper.GetDateSys(ConnectionString));
        }
        /// <summary>
        ///  Retourne la liste des paramètres disponible pour ce traitemement
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = new ArrayList
            {
                new MQueueparameter(PARAM_DATE1, TypeData.TypeDataEnum.date),
                new MQueueparameter(PARAM_AMOUNTTYPE, TypeData.TypeDataEnum.@string),
                new MQueueparameter(PARAM_IDC, TypeData.TypeDataEnum.@string),
                new MQueueparameter(PARAM_PERIOD, TypeData.TypeDataEnum.@string),
                new MQueueparameter(PARAM_PERIODMLTP, TypeData.TypeDataEnum.@int)
            };

            return ret;
        }
        #endregion Methods
    }
    #endregion CashBalanceInterestMQueue
    #region CollateralValMQueue
    /// <summary>
    /// Message pour déclenchement du traitement de valorisation les dépôts de garantie titre
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("posCollateralVal", IsNullable = false)]
    public class CollateralValMQueue : MQueueBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quote_DebtSecurityAsset", typeof(Quote_DebtSecurityAsset))]
        [System.Xml.Serialization.XmlElementAttribute("quote_EquityAsset", typeof(Quote_Equity))]
        public Quote quote;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient un libellé spécifique au traitement associé
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// Obtient le traitement associé au message
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.COLLATERALVAL; }
        }


        /// <summary>
        /// Obtient la quotation 
        /// <para>Obtient null si aucune cotation</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Quote Quote
        {
            get
            {
                Quote ret = null;
                if (quoteSpecified)
                    ret = quote;
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        public CollateralValMQueue() { }
        public CollateralValMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        public CollateralValMQueue(Quote pQuote, MQueueAttributes pMQueueAttributes) :
            base(pMQueueAttributes)
        {
            quote = pQuote;
            quoteSpecified = (null != pQuote);
            actionSpecified = quoteSpecified;
            if (actionSpecified)
                action = ((Quote)quote).action;
        }
        #endregion Constructors
        #region Methods

        /// <summary>
        /// Obtient false
        /// </summary>
        public override bool IsIdT
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string RetIdentifiersForMessageQueueName()
        {
            return base.RetIdentifiersForMessageQueueName();
        }
        #endregion Methods
    }
    #endregion CollateralValMQueue
    #region ConfirmationInstrGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("cnfInstrGen", IsNullable = false)]
    public class ConfirmationInstrGenMQueue : MQueueBase
    {
        #region Constantes
        public const string PARAM_ISTOEND = "ISTOEND";
        public const string PARAM_CNFTYPE = "CNFTYPE";
        #endregion Constantes
        #region Accessors

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime CiDate
        {
            get { return GetMasterDate(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.CIGEN; }
        }

        #endregion Accessors
        #region Constructors
        public ConfirmationInstrGenMQueue() { }
        public ConfirmationInstrGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_ISTOEND, TypeData.TypeDataEnum.@bool));
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        public override void SpecificParameters()
        {
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[ConfirmationInstrGenMQueue.PARAM_DATE1])))
                AddParameter(ConfirmationInstrGenMQueue.PARAM_DATE1, OTCmlHelper.GetDateBusiness(ConnectionString));
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[ConfirmationInstrGenMQueue.PARAM_ISTOEND])))
                AddParameter(ConfirmationInstrGenMQueue.PARAM_ISTOEND, true);
        }
        #endregion Methods
    }
    #endregion ConfirmationInstrGenMQueue
    #region ConfirmationMsgGenMQueue
    /// <summary>
    /// Mqueue dédié à la génération des confirmations  
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("cnfMsgGen", IsNullable = false)]
    public class ConfirmationMsgGenMQueue : MQueueBase
    {
        #region Constants
        public const string PARAM_ISMODEREGENERATE = "ISMODEREGENERATE";
        #endregion Constants
        #region Accessors
        #region IsIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IsIdT
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region msgGenDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime MsgGenDate
        {
            get { return GetMasterDate(); }
        }
        #endregion msgGenDate
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.CMGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public ConfirmationMsgGenMQueue() { }
        public ConfirmationMsgGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_IDT, TypeData.TypeDataEnum.integer));
            ret.Add(new MQueueparameter(PARAM_ISMODEREGENERATE, TypeData.TypeDataEnum.@bool));
            return ret;
        }
        #endregion
        #endregion Methods
    }
    #endregion ConfirmationMsgGenMQueue

    #region EarGenMQueue
    [System.Xml.Serialization.XmlRootAttribute("ear", IsNullable = false)]
    public class EarGenMQueue : MQueueBase
    {
        #region Constants
        public const string PARAM_CLASS = "CLASS";
        #endregion Constants
        #region Accessors
        #region earDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime EarDate
        {
            get { return GetMasterDate(); }
        }
        #endregion earDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.EARGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public EarGenMQueue() { }
        public EarGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_CLASS, TypeData.TypeDataEnum.@string));
            return ret;
        }
        #endregion GetListParameterAvailable
        #endregion Methods
    }
    #endregion EarGenMQueue
    #region EventsGenMQueue
    [System.Xml.Serialization.XmlRootAttribute("eventsGen", IsNullable = false)]
    // EG 20190613 [24683] Upd 
    public class EventsGenMQueue : MQueueBase
    {
        #region Constants
        public const string PARAM_DELEVENTS = "DELEVENTS";
        // EG 20181127 PERF Post RC (Step 3)
        public const string PARAM_NOLOCKCURRENTID = "NOLOCKCURRENTID";
        #endregion Constants
        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.EVENTSGEN; }
        }
        #endregion ProcessType
        // EG 20190613 [24683] New
        public int IdEParent_ForOffsettingPosition { get; set; }
        #endregion Accessors
        #region Constructors
        public EventsGenMQueue() { }
        public EventsGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        // EG 20181127 PERF Post RC (Step 3)
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_DELEVENTS, TypeData.TypeDataEnum.@bool));
            ret.Add(new MQueueparameter(PARAM_NOLOCKCURRENTID, TypeData.TypeDataEnum.@bool));
            return ret;
        }
        #endregion GetListParameterAvailable
        #region SpecificParameters
        // EG 20181127 PERF Post RC (Step 3)
        public override void SpecificParameters()
        {
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[EventsGenMQueue.PARAM_DELEVENTS])))
                AddParameter(EventsGenMQueue.PARAM_DELEVENTS, false);
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[EventsGenMQueue.PARAM_NOLOCKCURRENTID])))
                AddParameter(EventsGenMQueue.PARAM_NOLOCKCURRENTID, false);
        }
        #endregion SpecificParameters
        #endregion Methods
    }
    #endregion EventsGenMQueue
    #region EventsValMQueue
    // EG 20140120 Report 3.7
    // EG 20140212 Add Quote_Equity (ElementAttribute)
    [System.Xml.Serialization.XmlRootAttribute("eventsVal", IsNullable = false)]
    public class EventsValMQueue : MQueueBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quote_FxRate", typeof(Quote_FxRate))]
        [System.Xml.Serialization.XmlElementAttribute("quote_RateIndex", typeof(Quote_RateIndex))]
        [System.Xml.Serialization.XmlElementAttribute("quote_SecurityAsset", typeof(Quote_SecurityAsset))]
        [System.Xml.Serialization.XmlElementAttribute("quote_ETDAsset", typeof(Quote_ETDAsset))]
        [System.Xml.Serialization.XmlElementAttribute("quote_EquityAsset", typeof(Quote_Equity))]
        [System.Xml.Serialization.XmlElementAttribute("quote_Index", typeof(Quote_Index))]
        public Quote quote;
        #endregion Members
        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.EVENTSVAL; }
        }
        #endregion ProcessType
        #region Quote
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Quote Quote
        {
            get
            {
                if (quoteSpecified)
                    return quote;
                else
                    return null;
            }
        }
        #endregion Quote
        #endregion Accessors
        #region Constructors
        public EventsValMQueue() { }
        public EventsValMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        public EventsValMQueue(MQueueAttributes pMQueueAttributes, Quote pQuote)
            : base(pMQueueAttributes)
        {
            quote = pQuote;
            quoteSpecified = (null != pQuote);
            actionSpecified = quoteSpecified;
            if (actionSpecified)
                action = ((Quote)quote).action;
        }
        #endregion Constructors
    }
    #endregion EventsValMQueue
    #region ESRGenMQueueBase
    public abstract class ESRGenMQueueBase : MQueueBase
    {
        #region Constructors
        public ESRGenMQueueBase() { }
        public ESRGenMQueueBase(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }
    #endregion ESRGenMQueueBase
    #region ESRNetGenMQueue
    /// <summary>
    /// Process ge génération des nettings par convention ou designation
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("esrNetGenMQueue", IsNullable = false)]
    public class ESRNetGenMQueue : ESRGenMQueueBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nettingMethod")]
        public NettingMethodEnum nettingMethod;
        #endregion Members
        #region Accessors
        #region IsIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IsIdT
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.ESRNETGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public ESRNetGenMQueue() { }
        public ESRNetGenMQueue(NettingMethodEnum pNettingMethod, MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            if ((pNettingMethod != NettingMethodEnum.Convention) && (pNettingMethod != NettingMethodEnum.Designation))
                throw new ArgumentException("Invalid ", "NettingMethodEnum");
            nettingMethod = pNettingMethod;
        }
        #endregion Constructors
        #region Methods
        #region NettingMethodEntry
        private static bool NettingMethodEntry(DictionaryEntry pIdInfo)
        {
            return ("NETTINGMETHOD" == pIdInfo.Key.ToString().ToUpper());
        }
        #endregion NettingMethodEntry
        #region RetIdentifiersForMessageQueueName
        protected override string RetIdentifiersForMessageQueueName()
        {
            string nameIdentifiers = string.Empty;
            if (idSpecified)
            {
                if (NettingMethodEnum.Convention == nettingMethod)
                {
                    SQL_NetConvention sqlNetConvention = new SQL_NetConvention(ConnectionString, id);
                    if (sqlNetConvention.LoadTable(new string[] { "IDNETCONVENTION", "IDENTIFIER", "IDMASTERAGREEMENT" }))
                    {
                        nameIdentifiers += separator + sqlNetConvention.Id;	                //IDNETCONVENTION 
                        nameIdentifiers += separator + sqlNetConvention.IdMasterAgreement;  //IDMASTERAGREEMENT 
                        nameIdentifiers += separator + 0;				                    //0 
                        // 
                        identifierSpecified = true;
                        identifier = sqlNetConvention.Identifier;
                    }
                }
                if (NettingMethodEnum.Designation == nettingMethod)
                {
                    SQL_NetDesignation sqlNetDesignation = new SQL_NetDesignation(ConnectionString, id);
                    if (sqlNetDesignation.LoadTable(new string[] { "IDNETDESIGNATION", "IDENTIFIER", "IDA_1,IDA_2" }))
                    {
                        nameIdentifiers += separator + sqlNetDesignation.Id;	  //IDSTLMESSAGE 
                        nameIdentifiers += separator + sqlNetDesignation.IdA_1;   //IDA_1
                        nameIdentifiers += separator + sqlNetDesignation.IdA_2;	  //IDA_2
                        //
                        identifierSpecified = true;
                        identifier = sqlNetDesignation.Identifier;
                    }
                }
            }
            else
            {
                nameIdentifiers = base.RetIdentifiersForMessageQueueName();
            }
            return nameIdentifiers;
        }
        #endregion RetIdentifiersForMessageQueueName
        #region SpecificIdInfos
        public override void SpecificIdInfos()
        {
            nettingMethod = NettingMethodEnum.Convention;
            if (idInfoSpecified && ArrFunc.IsFilled(idInfo.idInfos))
            {
                DictionaryEntry item = Array.Find(idInfo.idInfos, NettingMethodEntry);
                if (null != item.Value)
                    nettingMethod = (NettingMethodEnum)Enum.Parse(typeof(NettingMethodEnum), item.Value.ToString(), true);
                else
                    throw new ArgumentException("DictionaryEntry is not Valid");
            }
        }
        #endregion SpecificIdInfos
        #endregion Methods
    }
    #endregion ESRNetGenMQueue
    #region ESRStandardGenMQueue
    /// <summary>
    /// Process génération des nettings standard/None (IntraTrade)
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("esrStandardGenMQueue", IsNullable = false)]
    public class ESRStandardGenMQueue : ESRGenMQueueBase
    {
        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.ESRSTDGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public ESRStandardGenMQueue() { }
        public ESRStandardGenMQueue(MQueueAttributes pQueueAttributes) : base(pQueueAttributes) { }
        #endregion Constructors
    }
    #endregion ESRStandardGenMQueue

    #region GateBCSMQueue
    [System.Xml.Serialization.XmlRootAttribute("GATEBCS", IsNullable = false)]
    public class GateBCSMQueue : MQueueBase
    {
        #region Enums
        #region BCSFlowType
        public enum BCSFlowType
        {
            None = 0,
            Series = 1,
            Contracts = 10,
            ContractTransfers = 11,
            EarlyExercises = 21,
            ExByEx = 22,
            ExerciseAtExpiry = 23,
            Assignments = 24,
            Reports = 40,
        }
        #endregion BCSFlowType
        #endregion Enums
        #region Constants
        public const string INFO_IDENTIFIER = "IDENTIFIER";
        #endregion Constants
        #region Accessors
        #region IsIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IsIdT
        #region FlowType
        public BCSFlowType FlowType
        {
            get { return BCSFlowFromName(identifier); }
        }
        #endregion FlowType
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.GATEBCS; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public GateBCSMQueue() { }
        public GateBCSMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region BCSFlowFromName
        public static BCSFlowType BCSFlowFromName(string pFlowName)
        {
            BCSFlowType flow = BCSFlowType.None;
            if (Enum.IsDefined(typeof(BCSFlowType), pFlowName))
                flow = (BCSFlowType)Enum.Parse(typeof(BCSFlowType), pFlowName, true);
            return flow;
        }
        #endregion BCSFlowFromName
        #region IsIdentifier
        private static bool IsIdentifier(DictionaryEntry pEntry)
        {
            return (INFO_IDENTIFIER == (string)pEntry.Key);
        }
        #endregion IsIdentifier
        #region RetIdentifiersForMessageQueueName
        protected override string RetIdentifiersForMessageQueueName()
        {
            string nameIdentifiers = string.Empty;
            const string filler = "0";
            if (idSpecified)
            {
                nameIdentifiers += separator + filler;	    //IDT 
                nameIdentifiers += separator + filler;	    //IDP 
                nameIdentifiers += separator + filler;	    //IDI 
            }
            else
            {
                nameIdentifiers = base.RetIdentifiersForMessageQueueName();
            }
            return nameIdentifiers;
        }
        #endregion RetIdentifiersForMessageQueueName
        #endregion Methods
    }
    #endregion GateBCSMQueue
    
    /// <summary>
    ///  Message queue qui contient les informations nécessaires à l'audit des actions utilisateurs
    /// </summary>
    /// FI 20140519 [19923] add class
    [System.Xml.Serialization.XmlRootAttribute("requestTrackMqueue", IsNullable = false)]
    public class RequestTrackMqueue : MQueueBase
    {
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.REQUESTTRACK; }
        }
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get
            {
                return false;
            }
        }

        [System.Xml.Serialization.XmlElement("requestTrack")]
        public RequestTrackDocument RequestTrack
        {
            get;
            set;
        }

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackMqueue() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueueAttributes"></param>
        public RequestTrackMqueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }

    #region InvoicingAllocationGenMQueue
    [System.Xml.Serialization.XmlRootAttribute("invoicingAllocationGen", IsNullable = false)]
    public class InvoicingAllocationGenMQueue : MQueueBase
    {
        #region Accessors
        #region InvoiceGenDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InvoiceGenDate
        {
            get { return GetMasterDate(); }
        }
        #endregion InvoiceGenDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.INVOICINGGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public InvoicingAllocationGenMQueue() { }
        public InvoicingAllocationGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        public override ArrayList GetListParameterAvailable()
        {
            return base.GetListParameterAvailable();
        }
        #endregion
        #endregion Methods
    }
    #endregion InvoicingAllocationGenMQueue
    #region InvoicingCorrectionGenMQueue
    [System.Xml.Serialization.XmlRootAttribute("invoicingCorrectionGen", IsNullable = false)]
    /// EG 20150515 [20513] TradeAdminAction devient TradeAdminActionMQueue
    public class InvoicingCorrectionGenMQueue : MQueueBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool itemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeAdminAction", typeof(TradeAdminActionMQueue))]
        public TradeAdminActionMQueue[] item;
        #endregion Members
        #region Accessors
        #region InvoiceGenDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InvoiceGenDate
        {
            get { return GetMasterDate(); }
        }
        #endregion InvoiceGenDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.INVOICINGGEN; }
        }
        #endregion ProcessType
        
        
        #region TradeActionItem
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150515 [20513] TradeCommonActionItem (TradeCommonAction) devient TradeActionItem (TradeAdminActionMQueue)
        public override TradeActionBaseMQueue[] TradeActionItem
        {
            get
            {
                if (itemSpecified)
                    return item;
                return null;
            }
        }
        #endregion TradeActionItem
        
        
        #endregion Accessors
        #region Constructors
        public InvoicingCorrectionGenMQueue() { }
        /// EG 20150515 [20513] TradeAdminAction devient TradeAdminActionMQueue
        public InvoicingCorrectionGenMQueue(MQueueAttributes pMQueueAttributes, SortedList pEvents)
            : base(pMQueueAttributes)
        {
            int i = 0;
            item = new TradeAdminActionMQueue[pEvents.Count];
            foreach (DictionaryEntry dictionaryEntry in pEvents)
            {
                string[] key = dictionaryEntry.Key.ToString().Split('_');
                int idE = Convert.ToInt32(key[0]);
                int idE_Event = Convert.ToInt32(key[1]);
                string code = key[3];
                TradeActionCode.TradeActionCodeEnum tradeActionCode =
                    (TradeActionCode.TradeActionCodeEnum)System.Enum.Parse(typeof(TradeActionCode.TradeActionCodeEnum), key[2], true);

                item[i] = new TradeAdminActionMQueue(idE, idE_Event, code, tradeActionCode)
                {
                    ActionMsgs = (ActionMsgBase[])((ArrayList)dictionaryEntry.Value).ToArray(typeof(ActionMsgBase))
                };
                i++;
            }
            itemSpecified = true;
        }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        public override ArrayList GetListParameterAvailable()
        {
            return base.GetListParameterAvailable();
        }
        #endregion
        #region InvoicingDetailEvent
        /// EG 20150515 [20513] InvoicingDetailEvent devient InvoicingDetailMsg
        public InvoicingDetailMsg InvoicingDetailEvent(int pIdE_Source)
        {
            InvoicingDetailMsg invoicingDetailEvent = null;
            if (itemSpecified)
            {
                foreach (TradeAdminActionMQueue tradeAdminAction in item)
                {
                    foreach (ActionMsgBase actionMsg in tradeAdminAction.ActionMsgs)
                    {
                        if (actionMsg.GetType().Equals(typeof(InvoicingDetailMsg)))
                        {
                            if (pIdE_Source == ((InvoicingDetailMsg)actionMsg).idE_Source)
                                invoicingDetailEvent = (InvoicingDetailMsg)actionMsg;
                        }
                    }
                }
            }
            return invoicingDetailEvent;
        }
        #endregion InvoicingDetailEvent
        #endregion Methods
    }
    #endregion InvoicingCorrectionGenMQueue
    /// <summary>
    /// Message de traitement d'une suppression de facture simulée
    /// </summary>
    /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
    [System.Xml.Serialization.XmlRootAttribute("invoicingCancelationSimulationGen", IsNullable = false)]
    public class InvoicingCancelationSimulationGenMQueue : MQueueBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.INVCANCELSIMUL; }
        }
        public InvoicingCancelationSimulationGenMQueue() { }
        public InvoicingCancelationSimulationGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
    }
    /// <summary>
    /// Message de traitement d'une validation de facture simulée
    /// </summary>
    /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
    [System.Xml.Serialization.XmlRootAttribute("invoicingValidationSimulationGen", IsNullable = false)]
    public class InvoicingValidationSimulationGenMQueue : MQueueBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.INVVALIDSIMUL; }
        }
        public InvoicingValidationSimulationGenMQueue() { }
        public InvoicingValidationSimulationGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
    }
    #region InvoicingGenMQueue
    [System.Xml.Serialization.XmlRootAttribute("invoicingGen", IsNullable = false)]
    public class InvoicingGenMQueue : MQueueBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool criteriaSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("criteria", typeof(SQL_Criteria))]
        public SQL_Criteria criteria;
        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InvoiceGenDate
        {
            get { return GetMasterDate(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.INVOICINGGEN; }
        }

        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public InvoicingGenMQueue() { }
        public InvoicingGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) 
        {
            criteria = pMQueueAttributes.criteria;
            criteriaSpecified = (null != pMQueueAttributes.criteria);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Retourne la liste des paramètres du process associé au message
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_IDT, TypeData.TypeDataEnum.@string));
            ret.Add(new MQueueparameter(PARAM_ISSIMUL, TypeData.TypeDataEnum.@bool));
            return ret;
        }
        #endregion Methods
    }
    #endregion InvoicingGenMQueue
    #region IOMQueue
    /// <summary>
    /// 
    ///  
    /// </summary>
    /// Pour l'instant IOMQueue herite de MQueueBase, Ce n'est pas forcement astucieux
    [System.Xml.Serialization.XmlRootAttribute("IO", IsNullable = false)]
    public class IOMQueue : MQueueBase
    {


        // FI 20240118 [WI814] Add 
        public const string PARAM_ISCREATEDBY_GATEWAY = "ISCREATEDBY_GATEWAY";

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.IO; }
        }

        /// <summary>
        /// Obtient true si le message est issu d'une gateway
        /// </summary>
        /// FI 20120129 [18252]
        /// FI 20240118 [WI814] Refactoring
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsGatewayMqueue
        {
            private set;
            get;
        }

        /// <summary>
        ///  Mis en place de l'attribut IsGatewayMqueue. Cette méthode est nécessaire car dans <see cref="Process.ProcessBase.InsertNewTracker"/>, <see cref="MQueueHeader.requester"/> est écrasé
        /// </summary>
        /// <remarks>L'appel à cette méthode pourra être supprimé lorsque les gateways seront en v14 ou plus</remarks>
        /// FI 20240118 [WI814] Refactoring
        public void SetIsGateway()
        {
            Boolean ret = false;
            if (parametersSpecified && (null != parameters[PARAM_ISCREATEDBY_GATEWAY]))
            {
                ret = GetBoolValueParameterById(PARAM_ISCREATEDBY_GATEWAY);
            }
            else if (header.requesterSpecified && header.requester.appNameSpecified)
            {
                string appName = header.requester.appName;
                if (StrFunc.IsFilled(appName))
                    ret = appName.ToLower().StartsWith("spheresgate");
            }

            this.IsGatewayMqueue = ret;
        }

        #endregion Accessors

        #region Constructors
        public IOMQueue() { }
        public IOMQueue(MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string RetIdentifiersForMessageQueueName()
        {
            string ret = string.Empty;
            const string filler = "0";
            if (idSpecified)
            {
                SQL_IOTask sqltask = new SQL_IOTask(ConnectionString, id);
                ret += separator + sqltask.Id;	//IDT 
                ret += separator + filler;	    //IDP 
                ret += separator + filler;	    //IDI 
                identifierSpecified = true;
                identifier = sqltask.Identifier;
            }
            else
            {
                ret = base.RetIdentifiersForMessageQueueName();
            }
            return ret;
        }


        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_ISCREATEDBY_GATEWAY, TypeData.TypeDataEnum.@bool));
            return ret;
        }


        /// <summary>
        /// Pour valoriser la colonne LODATATXT
        /// </summary>
        /// <returns></returns>
        public override String GetLogInfoDet()
        {
            string ret = string.Empty;
            if (parametersSpecified)
            {
                StringBuilder log = new StringBuilder();
                if (1 == (parameters.parameter.Length))
                    log.AppendLine(parameters.parameter[0].id + " : " + parameters.parameter[0].Value);
                else
                {
                    for (int i = 0; i < parameters.parameter.Length; i++)
                        log.AppendLine("- " + parameters.parameter[i].id + " : " + parameters.parameter[i].Value);
                }
                ret = log.ToString();
            }
            return ret;
        }
        
        
        
        
        
        #endregion Methods
    }
    #endregion IOMQueue

    #region LinearDepGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("linearDepGen", IsNullable = false)]
    public class LinearDepGenMQueue : MQueueBase
    {
        #region Accessors
        #region depreciationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime DepreciationDate
        {
            get { return GetMasterDate(); }
        }
        #endregion depreciationDate
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.LINEARDEPGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public LinearDepGenMQueue() { }
        public LinearDepGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }
    #endregion LinearDepGenMQueue

    #region MarkToMarketGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("mtm", IsNullable = false)]
    /// EG 20150317 [POC] Add isEOD and marginigMode
    public class MarkToMarketGenMQueue : MQueueBase
    {
        #region Members
        public Nullable<Cst.MarginingMode> marginingMode;
        public List<Pair<int, decimal>> crossMarginTrades;
        public bool isEOD;
        #endregion Members

        #region Accessors
        #region IsCrossMargining
        // EG 20150408 [POC] Verru à REVOIR
        // Il n'y a pas de CrossMargining si la liste  ne possède qu'un seul élément avec un ReferenceAmount à zéro.
        public bool IsCrossMargining
        {
            get
            {
                return ArrFunc.IsFilled(crossMarginTrades) && ((1 < crossMarginTrades.Count) || (1 == crossMarginTrades.Count) && (0 != crossMarginTrades[0].Second));
            }
        }
        #endregion IsCrossMargining
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region mtmDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime MtmDate
        {
            get { return GetMasterDate(); }
        }
        #endregion mtmDate
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.MTMGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        /// EG 20150317 [POC] Set isEOD and marginigMode
        public MarkToMarketGenMQueue() { }
        public MarkToMarketGenMQueue(MQueueAttributes pMQueueAttributes) : this(pMQueueAttributes, null, null, false) {  }
        public MarkToMarketGenMQueue(MQueueAttributes pMQueueAttributes, Nullable<Cst.MarginingMode> pMarginingMode, List<Pair<int, decimal>> pCrossMarginTrades, bool pIsEOD) 
        : base(pMQueueAttributes) 
        {
            isEOD = pIsEOD;
            marginingMode = pMarginingMode;
            crossMarginTrades = pCrossMarginTrades;
        }
        #endregion Constructors
    }
    #endregion MarkToMarketGenMQueue
    #region MSOGenMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("MSOGenMQueue", IsNullable = false)]
    public class MSOGenMQueue : MQueueBase
    {
        #region Constants
        public const string PARAM_DTMSO = "DTMSO";
        public const string PARAM_IDA_SENDER = "IDA_SENDER";
        #endregion Constants
        #region Accessors
        #region IdIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IdIdT
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.MSOGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public MSOGenMQueue() { }
        public MSOGenMQueue(MQueueAttributes pQueueAttributes) : base(pQueueAttributes) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_DTMSO, TypeData.TypeDataEnum.date));
            ret.Add(new MQueueparameter(PARAM_IDA_SENDER, TypeData.TypeDataEnum.integer));
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string RetIdentifiersForMessageQueueName()
        {
            string nameIdentifiers = string.Empty;
            //	
            if (idSpecified)
            {
                SQL_SettlementMessage sqlStlMsg = new SQL_SettlementMessage(ConnectionString, id);
                if (sqlStlMsg.LoadTable(new string[] { "IDSTLMESSAGE", "IDENTIFIER", "IDA_CSS" }))
                {
                    nameIdentifiers += separator + sqlStlMsg.Id;	  //IDSTLMESSAGE 
                    nameIdentifiers += separator + sqlStlMsg.IdA_Css;  //IDA_CSS 
                    nameIdentifiers += separator + 0;				  //0 
                    identifierSpecified = true;
                    identifier = sqlStlMsg.Identifier;
                }
            }
            else
                nameIdentifiers = base.RetIdentifiersForMessageQueueName();
            return nameIdentifiers;
        }
        #endregion Methods
    }
    #endregion New_MSOGenMQueue

    #region NormMsgFactoryMQueue
    /// <summary>
    /// Message de base destiné au service de construction d'un message normalisé prise en charge par un service applicatif Spheres
    /// Les données ID et IDENTIFIER correspondent à l'ID et l'IDENTIFIER externe de la demande (SCHEDULER par exemple)
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlRootAttribute("normMsgFactory", IsNullable = false)]
    public class NormMsgFactoryMQueue : MQueueBase
    {
        #region Constants parameters
        public const string PARAM_ACTOR = "ACTOR";
        public const string PARAM_BOOK = "BOOK";
        public const string PARAM_CLASS = "CLASS";
        public const string PARAM_CNFCLASS = "CNFCLASS";
        public const string PARAM_CNFTYPE = "CNFTYPE";
        public const string PARAM_CLEARINGHOUSE = "CLEARINGHOUSE";
        public const string PARAM_CSSCUSTODIAN= "CSSCUSTODIAN";
        public const string PARAM_CURRENCY = "CURRENCY";
        public const string PARAM_DTBUSINESS = "DTBUSINESS";
        public const string PARAM_DTINVOICING = "DTINVOICING";
        public const string PARAM_GPRODUCT = "GPRODUCT";

        /// <summary>
        /// Groupe de marché
        /// </summary>
        public const string PARAM_GMARKET = "GMARKET";
        /// <summary>
        /// Groupe d'acteur
        /// </summary>
        /// FI 20141230 [20616] add
        public const string PARAM_GACTOR = "GACTOR";
        /// <summary>
        /// Groupe de book
        /// </summary>
        /// FI 20141230 [20616] Add 
        public const string PARAM_GBOOK = "GBOOK";

        public const string PARAM_ISMANFEES_PRESERVED = "ISMANFEES_PRESERVED";
        // EG 20130701 Frais forcés préservés au recalcul (ITD / EOD)
        public const string PARAM_ISFORCEDFEES_PRESERVED = "ISFORCEDFEES_PRESERVED";
        // FI 20180328 [23871] Add
        public const string PARAM_FEESCALCULATIONMODE = "FEESCALCULATIONMODE";
        
        public const string PARAM_ISRESET = "ISRESET";
        public const string PARAM_MARKET = "MARKET";
        public const string PARAM_PAYER = "PAYER";
        public const string PARAM_REQUESTTYPE = "REQUESTTYPE";
        public const string PARAM_TIMING = "TIMING";
        public const string PARAM_SEND = "ISSEND";
        /// <summary>
        /// Software à l'origine de la demande de calcul
        /// </summary>
        //PM 20141216 [9700] New (Eurex Prisma for Eurosys Futures)
        public const string PARAM_SOFTWARE = "SOFTWARE";
        #endregion Constants parameters

        #region Members
        /// <summary>
        /// Informations pour la gestion des accusés de traitement
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("acknowledgment")]
        public AcknowledgmentInfo acknowledgment;
        /// <summary>
        /// Informations pour la construction du message normalisé
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("buildingInfo", typeof(NormMsgBuildingInfo))]
        public NormMsgBuildingInfo buildingInfo;
        #endregion Members

        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.NORMMSGFACTORY; }
        }
        #endregion ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get {return false;}
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public NormMsgFactoryMQueue() { }
        public NormMsgFactoryMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Retourne la liste des paramètres disponibles pour le traitement
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = new ArrayList();
            MQueueparameter mqp = new MQueueparameter
            {
                id = PARAM_ENTITY,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);
            mqp = new MQueueparameter
            {
                id = PARAM_CLEARINGHOUSE,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);
            mqp = new MQueueparameter
            {
                id = PARAM_CSSCUSTODIAN,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);
            mqp = new MQueueparameter
            {
                id = PARAM_TIMING,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);
            mqp = new MQueueparameter
            {
                id = PARAM_ISRESET,
                dataType = TypeData.TypeDataEnum.@bool
            };
            ret.Add(mqp);
            mqp = new MQueueparameter
            {
                id = PARAM_ISSIMUL,
                dataType = TypeData.TypeDataEnum.@bool
            };
            ret.Add(mqp);
            return ret;
        }
        #endregion Methods
    }
    #endregion NormMsgFactoryMQueue

    #region PosKeepingMQueue
    /// <summary>
    /// Classe abstraite utilisée par les messages de tenue de position.
    /// </summary>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosKeepingEntryMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosKeepingRequestMQueue))]
    [System.Xml.Serialization.XmlRootAttribute("posKeeping", IsNullable = false)]
    public class PosKeepingMQueue : MQueueBase
    {
        #region Constructors
        public PosKeepingMQueue() { }
        public PosKeepingMQueue(MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
        }
        #endregion Constructors
        #region methods
        public override string GetStringValueIdInfoByKey(string pKey)
        {
            string ret = null;
            DictionaryEntry? info;
            if ("CSSCUSTODIAN" == pKey)
            {
                info = GetIdInfoEntry("CSSCUSTODIAN");
                if (info.HasValue && (null != info.Value.Value))
                    ret = info.Value.Value.ToString();
                else
                {
                    info = GetIdInfoEntry("CLEARINGHOUSE");
                    if (info.HasValue && (null != info.Value.Value))
                        ret = info.Value.Value.ToString();
                    else
                    {
                        info = GetIdInfoEntry("CSS");
                        if (info.HasValue && (null != info.Value.Value))
                            ret = info.Value.Value.ToString();
                    }
                }
            }
            else
            {
                info = GetIdInfoEntry(pKey);
                if (info.HasValue && (null != info.Value.Value))
                    ret = info.Value.Value.ToString();
            }
            return ret;
        }
        #endregion methods
    }
    #endregion New_PosKeepingMQueue
    #region PosKeepingEntryMQueue
    /// <summary>
    /// Message utilisé par chaque entrée en portefeuille d'une négociation ETD
    /// items contient idT nouvellement entré
    /// Le service traitera ce message pour déterminer si la position est à (re)calculer ou non.
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("posKeepingEntry", IsNullable = false)]
    public class PosKeepingEntryMQueue : PosKeepingMQueue
    {
        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.POSKEEPENTRY; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public PosKeepingEntryMQueue() { }
        public PosKeepingEntryMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
    }
    #endregion PosKeepingEntryMQueue
    #region PosKeepingRequestMQueue
    [System.Xml.Serialization.XmlRootAttribute("posKeepingRequest", IsNullable = false)]
    public class PosKeepingRequestMQueue : PosKeepingMQueue
    {
        #region Members
        /// <summary>
        /// Représente les paramètres pour une demande via SCHEDULER
        /// Dans ce cas pas d'ID identiant un POSREQUEST, les éléments (IDEM,DTBUSINESS...)
        /// sont passés en paramètres)
        /// </summary>
        public const string PARAM_IDA_ENTITY = "IDA_ENTITY";
        public const string PARAM_IDA_CSS = "IDA_CSS";
        public const string PARAM_CLEARINGHOUSE = "CLEARINGHOUSE";
        public const string PARAM_IDA_CSSCUSTODIAN = "IDA_CSSCUSTODIAN";
        public const string PARAM_CSSCUSTODIAN = "CSSCUSTODIAN";
        public const string PARAM_DTBUSINESS = "DTBUSINESS";

        [System.Xml.Serialization.XmlElementAttribute("requestType")]
        public Cst.PosRequestTypeEnum requestType;
        #endregion Members
        #region Accessors
        #region IsIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IsIdT
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(requestType); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.POSKEEPREQUEST; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public PosKeepingRequestMQueue() { }
        public PosKeepingRequestMQueue(Cst.PosRequestTypeEnum pRequestType, MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            requestType = pRequestType;
        }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        /// <summary>
        /// Retourne la liste des paramètres du process associé au message dans le cas d'un appel
        /// via SCHEDULER = Sans IDPOSREQUEST dans le MESSAGE
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_IDA_ENTITY, TypeData.TypeDataEnum.integer));
            ret.Add(new MQueueparameter(PARAM_IDA_CSS, TypeData.TypeDataEnum.integer));
            ret.Add(new MQueueparameter(PARAM_CLEARINGHOUSE, TypeData.TypeDataEnum.@string));
            ret.Add(new MQueueparameter(PARAM_IDA_CSSCUSTODIAN, TypeData.TypeDataEnum.integer));
            ret.Add(new MQueueparameter(PARAM_CSSCUSTODIAN, TypeData.TypeDataEnum.@string));
            ret.Add(new MQueueparameter(PARAM_DTBUSINESS, TypeData.TypeDataEnum.date));
            ret.Add(new MQueueparameter(PARAM_ISSIMUL, TypeData.TypeDataEnum.boolean));
            ret.Add(new MQueueparameter(PARAM_ISKEEPHISTORY, TypeData.TypeDataEnum.boolean));
            return ret;
        }
        #endregion GetListParameterAvailable
        #endregion Methods

    }
    #endregion PosKeepingRequestMQueue

    #region QuotationHandlingMQueue
    /// <summary>
    ///  Mqueue posté à quotationHandling
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("quotationHandling", IsNullable = false)]
    public class QuotationHandlingMQueue : MQueueBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSpecified;
        /// <summary>
        /// Représente la cotation
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("quote_FxRate", typeof(Quote_FxRate))]
        [System.Xml.Serialization.XmlElementAttribute("quote_RateIndex", typeof(Quote_RateIndex))]
        [System.Xml.Serialization.XmlElementAttribute("quote_ETD", typeof(Quote_ETDAsset))]
        [System.Xml.Serialization.XmlElementAttribute("quote_debtSecurityAsset", typeof(Quote_DebtSecurityAsset))]
        [System.Xml.Serialization.XmlElementAttribute("quote_EquityAsset", typeof(Quote_Equity))]
        [System.Xml.Serialization.XmlElementAttribute("quote_Index", typeof(Quote_Index))]
        public Quote quote;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturitySpecified;
        /// <summary>
        /// Représente la maturité
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("maturity")]
        public Maturity maturity;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.QUOTHANDLING; }
        }
        
        #endregion Accessors
        
        #region Constructors
        public QuotationHandlingMQueue() { }
        public QuotationHandlingMQueue(Quote pQuote, MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            maturitySpecified = false;
            quoteSpecified = true;
            quote = pQuote;
            actionSpecified = true;
            action = ((Quote)quote).action;
        }
        public QuotationHandlingMQueue(Maturity pMaturity, MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            maturitySpecified = true;
            quoteSpecified = false;
            maturity = pMaturity;
            actionSpecified = true;
            action = maturity.action;
            id = maturity.idMaturity;
        }
        #endregion Constructors
        #region Methods
        
        #region RetIdentifiersForMessageQueueName
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        protected override string RetIdentifiersForMessageQueueName()
        {
            string nameIdentifiers = string.Empty;
            if (null != quote)
            {
                // RD 20110629
                // Gérer la modification du ContractMultiplier sur DERIVATIVECONTRACT, ASSET_ETD
                if (quote.IsQuoteTable_ETD)
                {
                    Quote_ETDAsset quote_ETDAsset = (Quote_ETDAsset)quote;

                    nameIdentifiers += "_" + quote_ETDAsset.QuoteTable.ToString();
                    if (quote_ETDAsset.idDCSpecified)
                        nameIdentifiers += "_" + quote_ETDAsset.idDC.ToString();
                    else if (quote_ETDAsset.idDerivativeAttribSpecified)
                        nameIdentifiers += "_" + quote_ETDAsset.idDerivativeAttrib.ToString(); // FI 20170306 [22225] add idDerivativeAttrib
                    else
                        nameIdentifiers += "_" + quote_ETDAsset.idAsset.ToString();

                    if (quote_ETDAsset.timeSpecified)
                        nameIdentifiers += "_" + quote_ETDAsset.time.ToString("yyyyMMddTHHmmss");

                    if (quote_ETDAsset.isCashFlowsValSpecified)
                        nameIdentifiers += "_" + quote_ETDAsset.isCashFlowsVal.ToString(); // FI 20170306 [22225] isCashFlowsVal instead of isCashFlowsValSpecified
                    else 
                        nameIdentifiers += "_false";
                }
                else
                {
                    nameIdentifiers += "_" + quote.idQuote.ToString();
                    nameIdentifiers += "_" + quote.idAsset.ToString();
                    nameIdentifiers += "_" + quote.time.ToString("yyyyMMddTHHmmss");
                }
            }
            else if (null != maturity)
            {
                nameIdentifiers += "_" + maturity.idMaturity.ToString();
                nameIdentifiers += "_" + maturity.idMaturityRule.ToString();
                nameIdentifiers += "_" + maturity.maturityMonthYear;
            }
            else
            {
                char filler = Convert.ToChar("X");
                for (int i = 0; i < 3; i++)
                    nameIdentifiers += separator + filler;
            }
            return nameIdentifiers;
        }
        #endregion RetIdentifiersForMessageQueueName
        #endregion Methods
    }
    #endregion QuotationHandlingMQueue
 
    #region ReportInstrMsgGenMQueue
    ///FI 20120327  
    /// <summary>
    /// Traitement de génération des éditions (2nd génération)
    /// <para>Avis d'opérés, Position ouvertes</para>
    /// </summary>
    /// FI 20120903 [17773] add PARAM_BOOK_IDENTIFIER, PARAM_MARKET_IDENTIFIER, PARAM_GMARKET_IDENTIFIER
    /// Ces paramètres peuvent être fournis dans le message queue manuellement
    /// Permet d'alimenter de passer les identifiers plutôt que les ids non significatif
    [System.Xml.Serialization.XmlRootAttribute("rptInstrMsgGen", IsNullable = false)]
    // EG 20160404 Migration vs2013
    public class ReportInstrMsgGenMQueue : MQueueBase
    {
        #region Constants
        /// <summary>
        /// Type d'édidition (ALLOC, POSITION, etc..)
        /// </summary>
        public const string PARAM_CNFTYPE = "CNFTYPE";
        
        /// <summary>
        /// classe d'édition (édition simple ou édition consolidée)
        /// </summary>
        public const string PARAM_CNFCLASS = "CNFCLASS";
        
        /// <summary>
        /// paramètre Book 
        /// <para>Obligatoire si édition simple</para>
        /// <para>Optionnel si édition consolidée (ds ce cas utilisé en parallèle avec GBOOK)</para>
        /// <para></para>
        /// </summary>
        public const string PARAM_BOOK  = "BOOK";

        /// <summary>
        /// paramètre GBook 
        /// <para>non pris en considération si édition simple</para>
        /// <para>Optionnel si édition consolidée (ds ce cas utilisé en parallèle avec BOOK)</para>
        /// <para></para>
        /// </summary>
        /// FI 20141230 [20616] Modify
        public const string PARAM_GBOOK = "GBOOK";

        /// <summary>
        /// paramètre ACTOR
        /// <para>non pris en considération si édition simple</para>
        /// <para>Optionnel si édition consolidée (ds ce cas utilisé en parallèle avec GACTOR)</para>
        /// <para></para>
        /// </summary>
        /// FI 20141230 [20616] Modify
        public const string PARAM_ACTOR  = "ACTOR";

        /// <summary>
        /// paramètre GACTOR
        /// <para>non pris en considération si édition simple</para>
        /// <para>Optionnel si édition consolidée (ds ce cas utilisé en parallèle avec ACTOR)</para>
        /// <para></para>
        /// </summary>
        /// FI 20141230 [20616] Modify
        public const string PARAM_GACTOR = "GACTOR";

        /// <summary>
        /// paramètre GMARKET
        /// <para>Optionnel pris en considération si édition simple ou édition consolidée (utilisé en parallèle avec GMARKET)</para>
        /// </summary>
        public const string PARAM_MARKET = "MARKET";
        
        /// <summary>
        /// paramètre MARKET
        /// <para>Optionnel pris en considération si édition simple ou édition consolidée (utilisé en parallèle avec MARKET)</para>
        /// </summary>
        public const string PARAM_GMARKET = "GMARKET";
        
        
        public const string PARAM_DTBUSINESS = "DTBUSINESS";

        #endregion Constants
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.RIMGEN; }
        }

        /// <summary>
        ///  Obtient False puisque l'Id est soit un book, soit une partie
        /// </summary>
        public override bool IsIdT
        {
            get
            {
                return false;
            }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public ReportInstrMsgGenMQueue() { }
        public ReportInstrMsgGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override void SpecificParameters()
        {
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[MQueueBase.PARAM_DATE1])))
                AddParameter(MQueueBase.PARAM_DATE1, OTCmlHelper.GetDateSys(ConnectionString));
        }
        /// <summary>
        ///  Retourne la liste des paramètres disponible pour ce traitemement
        /// </summary>
        /// <returns></returns>
        /// FI 20120829 [18048] add PARAM_DATE2 (sur les extrait de compte le traitement attent 2 dates)
        /// FI 20120903 [17773] add new parameter PARAM_ENTITY_IDENTIFIER, PARAM_BOOK_IDENTIFIER, PARAM_MARKET_IDENTIFIER, PARAM_GMARKET_IDENTIFIER
        /// FI 20141230 [20616] Modify 
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = new ArrayList
            {
                new MQueueparameter(PARAM_DATE1, TypeData.TypeDataEnum.date),
                new MQueueparameter(PARAM_ISSIMUL, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_ISOBSERVER, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_ENTITY, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_ISWITHIO, TypeData.TypeDataEnum.@bool),
                new MQueueparameter(PARAM_CNFTYPE, TypeData.TypeDataEnum.@string),
                new MQueueparameter(PARAM_CNFCLASS, TypeData.TypeDataEnum.@string),
                new MQueueparameter(PARAM_BOOK, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_MARKET, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_GMARKET, TypeData.TypeDataEnum.integer),

                // FI 20141230 [20616] add PARAM_ACTOR, PARAM_GACTOR, PARAM_GBOOK
                new MQueueparameter(PARAM_ACTOR, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_GACTOR, TypeData.TypeDataEnum.integer),
                new MQueueparameter(PARAM_GBOOK, TypeData.TypeDataEnum.integer),

                new MQueueparameter(PARAM_DATE2, TypeData.TypeDataEnum.date)
            };
            return ret;
        }
        #region RetIdentifiersForMessageQueueName
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20120920 [18137] Le nom du message queue est enrichi pour éviter les doublons
        protected override string RetIdentifiersForMessageQueueName()
        {
            string ret = string.Empty;
            const string filler = "0";

            int idB = GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_BOOK);

            ret += separator + id;	//id de l'acteur 
            ret += separator + idB;	//id du book
            ret += separator + filler;

            return ret;
        }
        #endregion RetIdentifiersForMessageQueueName
        #region RetStatusForMessageQueueName
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20120920 [18137] Le nom du message queue est enrichi pour éviter les doublons
        /// EG 20121130 Il est possible d'avoir un message sans CONFIRMATIONTYPE (équivaut à ALL)
        /// FI 20130627 [18745] add case SYNTHESIS
        protected override string RetStatusForMessageQueueName()
        {
            string ret = string.Empty;
            char filler = Convert.ToChar("X");
            string cnfTypeKey = string.Empty;

            if (false == IsMessageObserver)
            {
                Nullable<NotificationTypeEnum> cnfType = null;
                string paramCnfType = GetStringValueParameterById(ReportInstrMsgGenMQueue.PARAM_CNFTYPE);
                if (StrFunc.IsFilled(paramCnfType))
                {
                    if (false == Enum.IsDefined(typeof(NotificationTypeEnum), paramCnfType))
                        throw new Exception(StrFunc.AppendFormat("value: {0} is not implemented", paramCnfType));

                    cnfType = (NotificationTypeEnum)Enum.Parse(typeof(NotificationTypeEnum), paramCnfType);
                }
                if (cnfType.HasValue)
                {
                    switch (cnfType)
                    {
                        case NotificationTypeEnum.ALLOCATION:
                            cnfTypeKey = "ALLOC";
                            break;
                        case NotificationTypeEnum.POSACTION:
                            cnfTypeKey = "POSACT";
                            break;
                        case NotificationTypeEnum.POSITION:
                            cnfTypeKey = "POS";
                            break;
                        case NotificationTypeEnum.POSSYNTHETIC:
                            cnfTypeKey = "POSSYNT";
                            break;
                        case NotificationTypeEnum.FINANCIAL:
                            cnfTypeKey = "FIN";
                            break;
                        case NotificationTypeEnum.FINANCIALPERIODIC:
                            cnfTypeKey = "FINPER";
                            break;
                        case NotificationTypeEnum.SYNTHESIS:
                            cnfTypeKey = "SYNTHES";
                            break;

                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("cnfType {0} is not implemented", cnfType.ToString()));
                            // EG 20160404 Migration vs2013
                            //break;
                    }
                }
                else
                {
                    // EG 20121130 CONFIRMATIONTYPE possible avec NORMMSGFACTORY
                    cnfTypeKey = "ALLTYPE";
                }
            }

            ret += separator + cnfTypeKey.ToUpper().PadRight(8, filler);

            for (int i = 0; i < 2; i++)
                ret += separator + string.Empty.PadRight(8, filler);

            return ret;
        }
        #endregion RetStatusForMessageQueueName
        #endregion Methods
    }
    #endregion ReportInstrMsgGenMQueue
    #region ReportMsgGenMQueue
    /// <summary>
    /// Mqueue dédié à la génération des éditions
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("rptMsgGen", IsNullable = false)]
    public class ReportMsgGenMQueue : ConfirmationMsgGenMQueue
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }

        /// <summary>
        /// Obtient RMGEN
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.RMGEN; }
        }
        #endregion Accessors
    }
    #endregion ReportMsgGenMQueue
    #region ResponseRequestMQueue
    /// <summary>
    /// Accusé de reception de fin de traitement
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("responseRequest", IsNullable = false)]
    public class ResponseRequestMQueue : MQueueBase
    {
        #region Members
        /// <summary>
        /// Id non significatif de la demande de traitement
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idTRK_L", typeof(int))]
        public int idTRK_L;
        /// <summary>
        /// Id non significatif du log du process qui génère l'accusé de fin de traitement 
        /// <para>Pour 1 demande de traitement, il peut y avoir n enregistrements dans PROCESS_L, il s'agit là de l'id du dernier log (celui qui produit l'accusé)</para>  
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idProcess", typeof(int))]
        public int idProcess;
        /// <summary>
        /// Traitement demandé
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("requestProcess")]
        public Cst.ProcessTypeEnum requestProcess;
        /// <summary>
        /// Etat de la demandé (en théorie, seule la valeur TERMINATED est possible)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("readyState")]
        public ProcessStateTools.ReadyStateEnum readyState;
        /// <summary>
        /// Nbr de message Queue traités
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("nbMessage", typeof(int))]
        public int nbMessage;
        /// <summary>
        ///  Nb de message sans incidence
        /// </summary>
        /// FI 20201030 [25537] FI 
        [System.Xml.Serialization.XmlElementAttribute("nbNone", typeof(int))]
        public int nbNone;
        /// <summary>
        ///  Nb de message ayant terminé avec succès
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("nbSuccess", typeof(int))]
        public int nbSucces;
        /// <summary>
        ///  Nb de message ayant terminé en Warning
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("nbWarning", typeof(int))]
        public int nbWarning;
        /// <summary>
        ///  Nb de message ayant terminé en erreur
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("nbError", typeof(int))]
        public int nbError;
        #endregion Members
        #region Accessors
        #region LibRequestProcess
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string LibRequestProcess
        {
            get { return Cst.Process.GetResInvariantCulture(requestProcess); }
        }
        #endregion LibRequestProcess
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.RESPONSE; }
        }
        #endregion ProcessType
        #region RetIdentifiersForMessageQueueName
        protected override string RetIdentifiersForMessageQueueName()
        {
            return separator + header.requester.sessionId;
        }
        #endregion RetIdentifiersForMessageQueueName
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion Accessors
        #region Constructors
        public ResponseRequestMQueue() { }
        public ResponseRequestMQueue(MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            
        }
        #endregion Constructors
    }
    #endregion New_ResponseRequestMQueue
    #region RiskPerformanceMQueue
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("riskPerformance", IsNullable = false)]
    public class RiskPerformanceMQueue : MQueueBase
    {
        #region Constants
        /// <summary>
        /// Représente le paramètre "Chambre de compensation"
        /// </summary>
        public const string PARAM_IDA_CSS = "IDA_CSS";
        public const string PARAM_CLEARINGHOUSE = "CLEARINGHOUSE";
        public const string PARAM_IDA_CSSCUSTODIAN = "IDA_CSSCUSTODIAN";
        public const string PARAM_CSSCUSTODIAN = "CSSCUSTODIAN";
        /// <summary>
        /// Représente le paramètre "avec correction des trades existants"
        /// </summary>
        public const string PARAM_ISRESET = "ISRESET";
        /// <summary>
        /// Représente le paramètre (Type de traitement EndOfDay ou Intraday)
        /// </summary>
        public const string PARAM_TIMING = "TIMING";
        /// <summary>
        /// Représente le paramètre "Margin Requirement Office"
        /// </summary>
        public const string PARAM_MRO = "MRO";
        /// <summary>
        /// type de demande (cas appel Calcul de déposit via Traitement de fin de journée)
        /// </summary>
        public const string PARAM_REQUESTTYPE = "REQUESTTYPE";
        /// <summary>
        /// Id du POSREQUEST appelant (cas appel Calcul de déposit via Traitement de fin de journée)
        /// </summary>
        public const string PARAM_IDPR = "IDPR";
        /// <summary>
        /// Heures et minutes hh:mm pour le lancement Intraday d'un traitement RiskPerformance
        /// </summary>
        public const string PARAM_TIMEBUSINESS = "TIMEBUSINESS";
        /// <summary>
        /// Business date
        /// </summary>
        // EG 20131213 New
        public const string PARAM_DTBUSINESS = "DTBUSINESS";
        /// <summary>
        /// Events generation by SpheresEventGen
        /// </summary>
        public const string PARAM_ISEXTERNALGENEVENTS = "ISEXTERNALGENEVENTS";
        /// <summary>
        /// Heure de considération de l'instantané de la position pour le calcul Intra-Day
        /// </summary>
        //PM 20140212 [19493] New
        public const string PARAM_POSITIONTIME = "POSITIONTIME";

        /// <summary>
        /// Heure de considération des cours et paramètres de calcul pour le calcul Intra-Day
        /// </summary>
        //PM 20140212 [19493] New
        public const string PARAM_RISKDATATIME = "RISKDATATIME";

        /// <summary>
        /// Software à l'origine de la demande de calcul
        /// </summary>
        //PM 20141216 [9700] New (Eurex Prisma for Eurosys Futures)
        public const string PARAM_SOFTWARE = "SOFTWARE";
            
        
        /// <summary>
        /// Pilote l'execution du traitement CashBalance en fonction des derniers résultats des traitements EOD
        /// <para>Ce parmètre n'est valable uniquement sur le traitement cash Balance</para>
        /// </summary>
        //FI 20141128 [20526] Redéfinition de ce paramètre
        public const string PARAM_CTRL_EOD_MODE = "CTRL_EOD_MODE";

        /// <summary>
        /// Définition du périmètre CSS/Custodian prise en compte lorsqu'il y a contrôle des derniers résultats des traitements EOD
        /// <para>Voir paramètre CTRL_EOD</para>
        /// <para></para>
        /// </summary>
        //FI 20141128 [20526] 
        public const string PARAM_CTRL_EOD_CSSCUSTODIANLIST = "CTRL_EOD_CSSCUSTODIANLIST";


        /// <summary>
        /// Définition du statut final du traitement CashBalance lorsque les derniers résultats des traitements EOD ne sont pas en succès (ou warning)
        /// <para>Voir paramètre CTRL_EOD</para>
        /// <para></para>
        /// </summary>
        //FI 20141128 [20526] 
        public const string PARAM_CTRL_EOD_LOGSTATUS = "CTRL_EOD_LOGSTATUS";


        #endregion Constants
        #region Members
        /// <summary>
        /// Le process concerné: RISKPERFORMANCE ou bien CASHBALANCE
        /// </summary>
        private Cst.ProcessTypeEnum _processType;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Le process concerné est CASHBALANCE
        /// </summary>
        public bool IsCashBalanceProcess
        {
            get { return (ProcessType == Cst.ProcessTypeEnum.CASHBALANCE); }
        }
        /// <summary>
        /// Le process concerné est deposit (appelé également Initial Margin Requierement)
        /// </summary>
        public bool IsDepositProcess
        {
            get { return (ProcessType == Cst.ProcessTypeEnum.RISKPERFORMANCE); }
        }

        /// <summary>
        /// The id is relative to a Spheres actor
        /// </summary>
        public override bool IsIdT
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Le process concerné: RISKPERFORMANCE ou bien CASHBALANCE
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("processType")]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return _processType; }
            set { _processType = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string LibProcessType
        {
            get
            {
                string opener = GetStringValueParameterById(PARAM_REQUESTTYPE);
                if (StrFunc.IsFilled(opener))
                    return opener + "_" + ProcessType.ToString();
                else
                    return ProcessType.ToString();
            }
        }
        #endregion
        #region Methods

        /// <summary>
        /// Retourne la liste des paramètres disponibles pour le traitement
        /// </summary>
        /// <returns></returns>
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = new ArrayList();
            MQueueparameter mqp = new MQueueparameter
            {
                id = PARAM_DTBUSINESS,
                dataType = TypeData.TypeDataEnum.date
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_CLEARINGHOUSE,
                dataType = TypeData.TypeDataEnum.integer
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_CSSCUSTODIAN,
                dataType = TypeData.TypeDataEnum.integer
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_MRO,
                dataType = TypeData.TypeDataEnum.integer
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_ISSIMUL,
                dataType = TypeData.TypeDataEnum.@bool
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_TIMING,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_TIMEBUSINESS,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_ISRESET,
                dataType = TypeData.TypeDataEnum.@bool
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_REQUESTTYPE,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp);

            mqp = new MQueueparameter
            {
                id = PARAM_IDPR,
                dataType = TypeData.TypeDataEnum.integer
            };
            ret.Add(mqp);

            // EG 20131213 New
            mqp = new MQueueparameter
            {
                id = PARAM_ISEXTERNALGENEVENTS,
                dataType = TypeData.TypeDataEnum.@bool
            };
            ret.Add(mqp);


            //PM 20140212 [19493] New
            mqp = new MQueueparameter
            {
                id = PARAM_POSITIONTIME,
                dataType = TypeData.TypeDataEnum.time
            };
            ret.Add(mqp);

            //PM 20140212 [19493] New
            mqp = new MQueueparameter
            {
                id = PARAM_RISKDATATIME,
                dataType = TypeData.TypeDataEnum.time
            };
            ret.Add(mqp);

            //PM 20141216 [9700] New (Eurex Prisma for Eurosys Futures)
            mqp = new MQueueparameter
            {
                id = PARAM_SOFTWARE,
                dataType = TypeData.TypeDataEnum.@string
            };
            ret.Add(mqp); 
            
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string RetIdentifiersForMessageQueueName()
        {
            string ret = string.Empty;
            //	
            if (idSpecified)
            {
                SQL_Actor sqlActor = new SQL_Actor(ConnectionString, id);
                if (sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER" }))
                {
                    ret += separator + sqlActor.Id;	  //IDA
                    ret += separator + 0;             //0
                    ret += separator + 0;		      //0 
                    //
                    identifierSpecified = true;
                    identifier = sqlActor.Identifier;
                }
            }
            else
                ret = base.RetIdentifiersForMessageQueueName();
            //
            return ret;
        }

        /// <summary>
        /// get a span to add to a datetime object from the input parameter
        /// </summary>
        /// <param name="pParamId">input parameter identifier</param>
        /// <returns>the time span when the parameter is there and it has a good hh:mm format, 0 span otherwise</returns>
        public TimeSpan GetTimeValueFromParameter(string pParamId)
        {
            TimeSpan span = TimeSpan.Zero;

            //PM 20140214 [19493]
            //string value = GetObjectValueParameterById(pParamId) as string;

            //if (value != null)
            //{
            //    TimeSpan.TryParse(value, out span);
            //}

            object value = GetObjectValueParameterById(pParamId);

            if (value != null)
            {
                Type vType = value.GetType();
                if (vType == typeof(DateTime))
                {
                        span = ((DateTime)value).TimeOfDay;
                }
                else if (vType == typeof(string))
                {
                    TimeSpan.TryParse((string)value, out span);
                }
            }

            return span;
        }

        #endregion Methods
        #region constructor
        public RiskPerformanceMQueue()
        {
            _processType = Cst.ProcessTypeEnum.RISKPERFORMANCE;
        }
        public RiskPerformanceMQueue(Cst.ProcessTypeEnum pProcessType)
        {
            _processType = pProcessType;
        }
        public RiskPerformanceMQueue(Cst.ProcessTypeEnum pProcessType, MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            _processType = pProcessType;
        }
        #endregion
    }
    #endregion RiskPerformanceMQueue

    #region SettlementInstrGenMQueue
    /// <summary>
    /// /
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("settlementInstr", IsNullable = false)]
    public class SettlementInstrGenMQueue : MQueueBase
    {
        #region Constants
        public const string PARAM_DTSTM = "DTSTM";
        public const string PARAM_DTSTL = "DTSTL";
        public const string PARAM_ISTOEND = "ISTOEND";
        public const string PARAM_IDA_PAY = "IDA_PAY";
        public const string PARAM_IDA_REC = "IDA_REC";
        #endregion Constants
        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.SIGEN; }
        }
        #endregion ProcessType
        #endregion Accessors
        #region Constructors
        public SettlementInstrGenMQueue() { }
        public SettlementInstrGenMQueue(MQueueAttributes pMQueueAttributes) : base(pMQueueAttributes) { }
        #endregion Constructors
        #region Methods
        #region GetListParameterAvailable
        public override ArrayList GetListParameterAvailable()
        {
            ArrayList ret = base.GetListParameterAvailable();
            ret.Add(new MQueueparameter(PARAM_DTSTM, TypeData.TypeDataEnum.date));
            ret.Add(new MQueueparameter(PARAM_DTSTL, TypeData.TypeDataEnum.date));
            ret.Add(new MQueueparameter(PARAM_ISTOEND, TypeData.TypeDataEnum.@bool));
            ret.Add(new MQueueparameter(PARAM_IDA_PAY, TypeData.TypeDataEnum.integer));
            ret.Add(new MQueueparameter(PARAM_IDA_REC, TypeData.TypeDataEnum.integer));
            return ret;
        }
        #endregion
        #region SpecificParameters
        public override void SpecificParameters()
        {
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[SettlementInstrGenMQueue.PARAM_DATE1])))
                AddParameter(SettlementInstrGenMQueue.PARAM_DATE1, OTCmlHelper.GetDateBusiness(ConnectionString));
            if ((false == parametersSpecified) || (parametersSpecified && (null == parameters[SettlementInstrGenMQueue.PARAM_ISTOEND])))
                AddParameter(SettlementInstrGenMQueue.PARAM_ISTOEND, true);
        }
        #endregion SpecificParameters
        #endregion Methods
    }
    #endregion SettlementInstrGenMQueue
    #region ShellMQueue
    /// <summary>
    /// Pour l'instant ShellMQueue herite de MQueueBase
    /// Ce n'est pas forcement astucieux 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Shell", IsNullable = false)]
    public class ShellMQueue : MQueueBase
    {
        #region Accessors
        #region IsIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override bool IsIdT
        {
            get { return false; }
        }
        #endregion IsIdT
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get
            {
                return Cst.ProcessTypeEnum.SHELL;
            }
        }
        #endregion ProcessType
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #endregion Accessors
        #region Constructors
        public ShellMQueue() { }
        public ShellMQueue(MQueueAttributes pMQueueAttributes): base(pMQueueAttributes)
        {
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        protected override string RetIdentifiersForMessageQueueName()
        {

            string nameIdentifiers = string.Empty;
            const string filler = "0";
            //
            if (idSpecified)
            {
                nameIdentifiers += separator + id;	//IDT 
                nameIdentifiers += separator + filler;	    //IDP 
                nameIdentifiers += separator + filler;	    //IDI 
                identifierSpecified = true;
                identifier = "SpheresShell TODO";
            }
            else
            {
                nameIdentifiers = base.RetIdentifiersForMessageQueueName();
            }
            return nameIdentifiers;
        }
        #endregion
    }
    #endregion ShellMQueue

    #region TradeActionGenMQueue
    /// <summary>
    ///  Action sur un trade 
    /// </summary>
    /// EG 20150515 [20513] item (TradeCommonAction) devient item (TradeActionBaseMQueue)
    /// FI 20170306 [22225] Modify
    [System.Xml.Serialization.XmlRootAttribute("tradeAction", IsNullable = false)]
    public class TradeActionGenMQueue : MQueueBase
    {
        // FI 20170306 [22225] add ISMANFEES_PRESERVED, ISFORCEDFEES_PRESERVED (paramètres utilisés lors de recalcul de frais)  
        public const string PARAM_ISMANFEES_PRESERVED = "ISMANFEES_PRESERVED";
        public const string PARAM_ISFORCEDFEES_PRESERVED = "ISFORCEDFEES_PRESERVED";
        
        #region Members
        /// <summary>
        /// Détails de l'action
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tradeAction", typeof(TradeActionMQueue))]
        [System.Xml.Serialization.XmlElementAttribute("tradeAdminAction", typeof(TradeAdminActionMQueue))]
        public TradeActionBaseMQueue[] item;
        #endregion Members

        #region Accessors
        #region LibProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string LibProcessType
        {
            get { return ProcessType.ToString(); }
        }
        #endregion LibProcessType
        #region ProcessType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Cst.ProcessTypeEnum ProcessType
        {
            get { return Cst.ProcessTypeEnum.TRADEACTGEN; }
        }
        #endregion ProcessType
        
        
        #region TradeActionItem
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150515 [20513] item (TradeCommonAction) devient item (TradeActionBaseMQueue)
        public override TradeActionBaseMQueue[] TradeActionItem
        {
            get { return item; }
        }
        #endregion TradeActionItem
        
        
        #endregion Accessors
        #region Constructors
        public TradeActionGenMQueue() { }
        public TradeActionGenMQueue(MQueueAttributes pMQueueAttributes)
            : base(pMQueueAttributes)
        {
            item = new TradeActionMQueue[1] { new TradeActionMQueue(0, 0, PARAM_ISOBSERVER) };
        }
        public TradeActionGenMQueue(MQueueAttributes pMQueueAttributes, SortedList pEvents)
            : base(pMQueueAttributes)
        {
            int i = 0;
            item = new TradeActionBaseMQueue[pEvents.Count];
            foreach (DictionaryEntry ht in pEvents)
            {
                 SetItem(i, ht);
                i++;
            }
        }
        public TradeActionGenMQueue(MQueueAttributes pMQueueAttributes, RemoveTradeMsg pRemoveTradeMsg)
            : base(pMQueueAttributes)
        {
            if (IsTradeAdmin)
                item = new TradeAdminActionMQueue[1] { new TradeAdminActionMQueue(0, 0, EventCodeFunc.RemoveTrade, TradeActionCode.TradeActionCodeEnum.RemoveTrade) };
            else
                item = new TradeActionMQueue[1] { new TradeActionMQueue(0, 0, EventCodeFunc.RemoveTrade, TradeActionCode.TradeActionCodeEnum.RemoveTrade) };
            item[0].ActionMsgs = new RemoveTradeMsg[1] { pRemoveTradeMsg };
        }
        #endregion Constructors
        #region Methods
        #region SetItem
        private void SetItem(int pIndex, DictionaryEntry pEvent)
        {
            string[] key = pEvent.Key.ToString().Split('_');
            int idE = Convert.ToInt32(key[0]);
            int idE_Event = Convert.ToInt32(key[1]);
            string code = key[3];
            TradeActionCode.TradeActionCodeEnum tradeActionCode =
                (TradeActionCode.TradeActionCodeEnum)System.Enum.Parse(typeof(TradeActionCode.TradeActionCodeEnum), key[2], true);

            switch (tradeActionCode)
            {
                case TradeActionCode.TradeActionCodeEnum.Abandon:
                case TradeActionCode.TradeActionCodeEnum.Barrier:
                case TradeActionCode.TradeActionCodeEnum.CancelableProvision:
                case TradeActionCode.TradeActionCodeEnum.CashSettlement:
                case TradeActionCode.TradeActionCodeEnum.CustomerSettlementRate:
                case TradeActionCode.TradeActionCodeEnum.Exercise:
                case TradeActionCode.TradeActionCodeEnum.ExtendibleProvision:
                case TradeActionCode.TradeActionCodeEnum.MandatoryEarlyTerminationProvision:
                case TradeActionCode.TradeActionCodeEnum.OptionalEarlyTerminationProvision:
                case TradeActionCode.TradeActionCodeEnum.Payout:
                case TradeActionCode.TradeActionCodeEnum.Rebate:
                case TradeActionCode.TradeActionCodeEnum.RemoveTrade:
                case TradeActionCode.TradeActionCodeEnum.StepUpProvision:
                case TradeActionCode.TradeActionCodeEnum.Trigger:
                    item[pIndex] = new TradeActionMQueue(idE, idE_Event, code, tradeActionCode);
                    break;
                case TradeActionCode.TradeActionCodeEnum.InvoiceCorrection:
                    item[pIndex] = new TradeAdminActionMQueue(idE, idE_Event, code, tradeActionCode);
                    break;
                default:
                    item[pIndex] = new TradeActionMQueue(idE, idE_Event, code, tradeActionCode);
                    break;
            }
            item[pIndex].ActionMsgs = (ActionMsgBase[]) ((ArrayList)pEvent.Value).ToArray(typeof(ActionMsgBase));
        }
        #endregion SetItem
        #endregion Methods
    }
    #endregion
}
