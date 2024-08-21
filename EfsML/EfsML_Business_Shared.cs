#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;

using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EfsML.Business
{
    // EG 20231130 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

    #region CurrencyCashInfo
    public class CurrencyCashInfo
    {
        #region Members
        private readonly string m_Currency;
        private int m_Factor;
        private int m_RoundPrec;
        private string m_RoundDir;
        #endregion Members
        #region Accessors
        #region Currency
        public string Currency
        {
            get { return m_Currency; }
        }
        #endregion Currency
        #region Factor
        public int Factor
        {
            get { return m_Factor; }
            set { m_Factor = value; }
        }
        #endregion Factor
        #region RoundPrec
        public int RoundPrec
        {
            get { return m_RoundPrec; }
            set { m_RoundPrec = value; }
        }
        #endregion RoundPrec
        #region RoundDir
        public string RoundDir
        {
            get { return m_RoundDir; }
            set { m_RoundDir = value; }
        }
        #endregion RoundDir
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Add dbTransaction  
        public CurrencyCashInfo(string pCS, string pCurrency) : this(pCS, null, pCurrency) { }
        // EG 20180205 [23769] Add dbTransaction  
        public CurrencyCashInfo(string pCS, IDbTransaction pDbTransaction, string pCurrency)
        {
            SQL_Currency sqlCurrency = new SQL_Currency(CSTools.SetCacheOn(pCS), SQL_Currency.IDType.Iso4217, pCurrency)
            {
                DbTransaction = pDbTransaction
            };
            if (sqlCurrency.IsLoaded)
            {
                m_Currency = sqlCurrency.IdC;
                m_Factor = sqlCurrency.Factor;
                m_RoundPrec = sqlCurrency.RoundPrec;
                m_RoundDir = sqlCurrency.RoundDir;
            }
        }
        public CurrencyCashInfo(int pFactor, int pRoundPrec, string pRoundDir)
        {
            m_Factor = pFactor;
            m_RoundPrec = pRoundPrec;
            m_RoundDir = pRoundDir;
        }
        #endregion Constructors
    }
    #endregion CurrencyCashInfo

    #region DtFuncML
    /// <summary>
    /// 
    /// </summary>
    /// EG 20140711 Gestion du CUSTODIAN
    public partial class DtFuncML : DtFunc
    {
        //
        #region Members
        private readonly IProductBase _productBase;
        private readonly string _cs;
        /// <summary>
        /// BusinessCenter untilisé pour les décalages Business (ex TODAY-4B)
        /// </summary>
        private readonly string _businessCenter;
        /// <summary>
        /// Représente une entité (utilisé pour interpréter le mot clef BUSINESS)
        /// </summary>
        private readonly int _idAEntity;
        /// <summary>
        /// Représente un marché (utilisé pour interpréter le mot clef BUSINESS)
        /// </summary>
        private readonly int _idMarket;

        /// <summary>
        /// Représente une chambre (utilisé pour interpréter le mot clef BUSINESS)
        /// </summary>
        /// FI 2130502 [] ajout de _idCss
        private readonly int _idCss;

        /// <summary>
        /// Représente un custodian (utilisé pour interpréter le mot clef BUSINESS)
        /// </summary>
        /// EG 20140711 Add _idCustodian
        private readonly Nullable<int> _idCustodian;

        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pBusinessCenter"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pIdMarket"></param>
        /// <param name="pIdCss"></param>
        /// FI 2130502 [] ajout de pIdCss
        //public DtFuncML(string pCs, string pBusinessCenter, int pIdAEntity, int pIdMarket, int pIdCss) :
        //    this(pCs, (IProductBase)new FpML.v42.Shared.Product(), pBusinessCenter, pIdAEntity, pIdMarket, pIdCss, null) { }
        // EG 20231127 [WI752] Exclusion de FpML 4.2
        public DtFuncML(string pCs, string pBusinessCenter, int pIdAEntity, int pIdMarket, int pIdCss, Nullable<int> pIdCustodian) :
            this(pCs, (IProductBase)new FpML.v44.Shared.Product(), pBusinessCenter, pIdAEntity, pIdMarket, pIdCss, pIdCustodian) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProduct"></param>
        /// <param name="pBusinessCenter"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pIdMarket"></param>
        /// <param name="pIdCss"></param>
        /// FI 2130502 [] ajout de pIdCss
        private DtFuncML(string pCs, IProductBase pProduct, string pBusinessCenter, int pIdAEntity, int pIdMarket, int pIdCss, Nullable<int> pIdCustodian)
        {
            _cs = pCs;
            _productBase = pProduct;
            _businessCenter = pBusinessCenter;
            _idAEntity = pIdAEntity;
            _idMarket = pIdMarket;
            _idCss = pIdCss;
            _idCustodian = pIdCustodian;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Décalage de +/- n jours business
        /// </summary>
        /// <param name="pDtSource"></param>
        /// <param name="pDays"></param>
        /// <param name="pBusinessCenter"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected DateTime ApplyDaysOffset(DateTime pDtSource, int pDays, string pBusinessCenter)
        {
            DateTime dtRet = Tools.ApplyOffset(_cs, _productBase, pDtSource, pDays, DayTypeEnum.Business, pBusinessCenter, null);
            return dtRet;
        }

        /// <summary>
        /// Interprète une expression String en date (voir le ticket 16079)
        /// <para>Gestion des décalages business selon le business center courant </para>
        /// <para>En cas d'évolution, il faut nécessaire de tester la non dégradation via la méthode DtFuncML.test.Run()</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        protected override DateTime GetDateFunction(string pDateValue)
        {

            string dateValue = pDateValue.ToUpper();
            DateTime dtRet;
            if (base.IsDateFunction(dateValue))
            {
                //ici si Exemple dateValue =>TODAY-3M ou -4M ou -5 etc
                //Résultat equivalent à l'appel de la fonction de base
                dtRet = base.GetDateFunction(dateValue);
            }
            else
            {
                DateTime dtRef = DateTime.MinValue;
                Regex regex = new Regex(@"[\-\+]\d+");
                MatchCollection matchcol = regex.Matches(dateValue);
                if (matchcol.Count == 2)
                {
                    //ici si Exemple  dateValue =>TODAY-3M-4FRPA ou TODAY-3M-4B ou TODAY-3M-4C
                    //dtRef = Resultat de TODAY-3M
                    dtRef = base.GetDateFunction(dateValue.Substring(0, matchcol[1].Index));
                    //PL 20161116 RATP
                    //---------------------------------------------------------------------------------------------------------------
                    //BEFORE
                    //---------------------------------------------------------------------------------------------------------------
                    //dateValue = Resultat précédent au format ddMMyy-4FRPA ou -4B ou -4C   
                    //dateValue = DtFunc.DateTimeToString(dtRef, "ddMMyy") + dateValue.Substring(matchcol[1].Index);
                    //---------------------------------------------------------------------------------------------------------------
                    //AFTER (pour corriger le fait que +20Y sur la base d'une date 1996 ne donne 2116)
                    //---------------------------------------------------------------------------------------------------------------
                    //dateValue = Resultat précédent au format ddMMyyyy-4FRPA ou -4B ou -4C   
                    dateValue = DtFunc.DateTimeToString(dtRef, "ddMMyyyy") + dateValue.Substring(matchcol[1].Index);
                }
                //
                if (DtFunc.IsDateTimeEmpty(dtRef))
                {
                    //ici si Exemple  dateValue =>TODAY-4FRPA 
                    dtRef = GetDateReference(dateValue);
                }
                //
                dtRet = dtRef;
                if (DtFunc.IsDateTimeFilled(dtRef))
                {
                    regex = new Regex(@"[\-\+]\d+");
                    if (regex.IsMatch(dateValue.ToUpper()))
                    {
                        Int32 days = IntFunc.IntValue(regex.Match(dateValue.ToUpper()).Value);
                        //search businessCenter
                        string businessCenter = string.Empty;
                        regex = new Regex(@"[^0-9]+$");
                        if (regex.IsMatch(dateValue.ToUpper()))
                        {
                            businessCenter = regex.Match(dateValue.ToUpper()).Value;
                            //
                            switch (businessCenter)
                            {
                                case "B":
                                    if (businessCenter == "B")
                                        businessCenter = _businessCenter;
                                    break;
                                case "C":
                                    businessCenter = string.Empty;
                                    break;
                            }
                        }
                        //
                        if (StrFunc.IsFilled(businessCenter))
                            dtRet = ApplyDaysOffset(dtRet, days, businessCenter);
                        else
                            dtRet = dtRet.AddDays(days);
                    }
                }
            }
            return dtRet;

        }

        /// <summary>
        /// Retourne true lorsque la pDateValue respecte la regex RegexDateRelative
        /// <para>Signifie que l'expression {pDateValue} peut être convertie en date</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        protected override bool IsDateFunction(string pDateValue)
        {

            bool isOk = false;
            //
            if (StrFunc.IsFilled(pDateValue))
                isOk = EFSRegex.IsMatch(pDateValue.ToUpper(), EFSRegex.TypeRegex.RegexDateRelativeOffset);
            //
            return isOk;

        }

        /// <summary>
        /// Retourne la date de référence par interprétation de mots clefs ou d'une date partiellement renseignée  
        /// </summary>
        /// <param name="pDateValue">String à interpréter</param>
        /// <returns></returns>
        /// FI 20130502 [] gestion de la notion de chambre dans l'interprétation de DTBUSINESS
        /// EG 20140711 Gestion du CUSTODIAN
        protected override DateTime GetDateReference(string pDateValue)
        {
            DateTime dtRef = DateTime.MinValue;
            //	
            Regex regex;
            //
            #region  IsBUSINESS
            regex = new Regex(@"^(" + BUSINESS + ")", RegexOptions.IgnoreCase);
            bool isOk = regex.IsMatch(pDateValue.ToUpper());
            if (isOk)
            {
                if ((_idMarket > 0) && (_idAEntity > 0))
                    dtRef = OTCmlHelper.GetDateBusiness(_cs, _idAEntity, _idMarket, _idCustodian);
                else if ((_idCss > 0) && (_idAEntity > 0))
                    dtRef = MarketTools.GetMinDtBusiness(_cs, _idAEntity, _idCss, 0);
                else if (_idMarket > 0)
                    dtRef = MarketTools.GetMinDtBusiness(_cs, 0, 0, _idMarket);
                else if (_idCss > 0)
                    dtRef = MarketTools.GetMinDtBusiness(_cs, 0, _idCss, 0);
                else if (_idAEntity > 0)
                    dtRef = MarketTools.GetMinDtBusiness(_cs, _idAEntity, 0, 0);
                //
                if (dtRef == DateTime.MinValue)
                {
                    //PL 20111125  
                    //dtRef = base.GetDateFunction(DtFunc.TODAY);
                    dtRef = MarketTools.GetMinDtBusiness(_cs, 0, 0, 0);
                }
                //FI 20120529 S'il n'y a aucune ligne dans ENTITYMARKET alors Spheres® interprete DTBUSINESS comme TODAY
                if (dtRef == DateTime.MinValue)
                {
                    //Spheres® rentre ici et remplace BUSINESS par TODAY
                    pDateValue = pDateValue.Replace(BUSINESS, TODAY);
                    isOk = false;
                }
            }
            #endregion
            //
            if (false == isOk)
                dtRef = base.GetDateReference(pDateValue);
            //
            return dtRef;
        }

        /// <summary>
        /// Retourne la date offsetée {pDate} au format UTC (l'offset donne le décalage par rapport au temps universel)
        /// </summary>
        /// <param name="pDate">Représente une date supposée être sans heure</param>
        /// <param name="pTime">Représente une heure et la localisation</param>
        /// <returns></returns>
        // FI 20170718 [23326] Modify 
        // EG 20171025 [23509] Upd
        public static DateTimeOffset CalcDeliveryDateTimeOffset(DateTime pDate, IPrevailingTime pTime)
        {
            if (null == pTime)
                throw new ArgumentNullException("Argument pTime is null");

            DateTime dt = pDate.AddTicks(pTime.HourMinuteTime.TimeValue.TimeOfDay.Ticks);
            TimeZoneInfo timeZoneInfo = Tz.Tools.GetTimeZoneInfoFromTzdbId(pTime.Location.Value);
            return new DateTimeOffset(dt, timeZoneInfo.GetUtcOffset(dt));
        }
        // EG 20171025 [23509] New
        public static DateTime CalcDeliveryDateTimeUTC(DateTime pDate, IPrevailingTime pTime)
        {
            return CalcDeliveryDateTimeOffset(pDate, pTime).UtcDateTime;
        }



        #endregion Methods
    }

    public partial class DtFuncML
    {
#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public new static class Test
        {
            /// <summary>
            ///  Plusieurs exemples d'usage de GetDateFunction 
            /// <para>Tests en relation avec les explications du ticket 16079</para>
            /// </summary>
            /// <param name="cs"></param>
            public static void Run(string cs)
            {
                string defaultBusinessCenter = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");
                DtFuncML dtFunc = new DtFuncML( cs , defaultBusinessCenter, 0, 0, 0, null);

                //-7D
                _ = dtFunc.IsDateFunction("-7D");
                _ = dtFunc.GetDateFunction("-7D");

                //The Last day of the month
                _ = dtFunc.IsDateFunction("EOM");
                _ = dtFunc.GetDateFunction("EOM");

                //The Last day of the year
                _ = dtFunc.IsDateFunction("EOY");
                _ = dtFunc.GetDateFunction("EOY");

                //Date of the day less 1 calendar day
                _ = dtFunc.IsDateFunction("TODAY-1");
                _ = dtFunc.GetDateFunction("TODAY-1");

                //Date of the day less 2 months
                _ = dtFunc.IsDateFunction("TODAY-2M");
                _ = dtFunc.GetDateFunction("TODAY-2M");

                //Date of the day less 2 years 
                _ = dtFunc.IsDateFunction("TODAY-2Y");
                _ = dtFunc.GetDateFunction("TODAY-2Y");

                //The 22nd of the current month more 2 months
                _ = dtFunc.IsDateFunction("22+2M");
                _ = dtFunc.GetDateFunction("22+2M");

                //Date of the day less 2 calendar days
                _ = dtFunc.IsDateFunction("TODAY-2C");
                _ = dtFunc.GetDateFunction("TODAY-2C");

                //Date of the Business day less 2 working days on the Business Center by default** (ex. FRPA)
                _ = dtFunc.IsDateFunction("BUSINESS-2B");
                _ = dtFunc.GetDateFunction("BUSINESS-2B");


                //Date of the day less 2 working days on the Business Center FRPA (Paris)
                _ = dtFunc.IsDateFunction("TODAY-2FRPA");
                _ = dtFunc.GetDateFunction("TODAY-2FRPA");

                //Date of the day more 2 working days on the Business Center GBLO (London)
                _ = dtFunc.IsDateFunction("TODAY+2GBLO");
                _ = dtFunc.GetDateFunction("TODAY+2GBLO");

                //The 22nd of the current month more 2 working days on the Business Center GBLO(London)
                _ = dtFunc.IsDateFunction("22+2GBLO");
                _ = dtFunc.GetDateFunction("22+2GBLO");

                //Date of the day less 1 month less 2 calendar days
                _ = dtFunc.IsDateFunction("TODAY-1M-2C");
                _ = dtFunc.GetDateFunction("TODAY-1M-2C");

                //The 22nd of the current month more 1 year more 2 working days on the Business Center GBLO (London)
                _ = dtFunc.IsDateFunction("22+1Y+2GBLO");
                _ = dtFunc.GetDateFunction("22+1Y+2GBLO");

                /* tests pour fenêtre  de consultation des évènements */
                //Date of the Business day less 7 days
                _ = dtFunc.IsDateFunction("BUSINESS-7D");
                _ = dtFunc.GetDateFunction("BUSINESS-7D");

                //Date of the Business day less 3 months
                _ = dtFunc.IsDateFunction("BUSINESS-3M");
                _ = dtFunc.GetDateFunction("BUSINESS-3M");

            }
        }
#endif
    }


    #endregion DtFuncML

    #region EFS_AccountingEventClass
    /// <revision>
    ///     <version>1.1.5</version><date>20070412</date><author>EG</author>
    ///     <EurosysSupport>N° xxxxx</EurosysSupport>
    ///     <comment>
    ///     Lecture paramètres des constructions des eventClass comptables.
    ///     Calcul eventClass comptables candidats.
    ///		</comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0.1</version><date>20071130</date><author>EG</author>
    ///     <comment>Ticket 16003
    ///     CalculEventClassCandidate
    ///     </comment>
    ///     <version>1.2.1.0</version><date>20080306</date><author>FI</author>
    ///     <comment>Ticket 16124
    ///     m_Product est maintenant de type IProductBase
    ///     </comment>
    /// </revision>
    public class EFS_AccountingEventClass
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        private readonly int m_IdE;
        private readonly int m_IdI;
        private readonly IProductBase m_ProductBase;
        private readonly DateTime m_DtDemand;
        //private DateTime m_DtToDay;
        private DataTable m_DtAccountingTuningRule;
        private EventClass[] m_EventClassCandidate;
        #endregion Members
        #region Accessors
        #region EventClassCandidate
        public EventClass[] EventClassCandidate
        {
            get { return m_EventClassCandidate; }
        }
        #endregion EventClassCandidate
        #region IsExistAccountingTuningRule
        public bool IsExistAccountingTuningRule
        {
            get { return (null != m_DtAccountingTuningRule) && (0 < m_DtAccountingTuningRule.Rows.Count); }
        }
        #endregion IsExistAccountingTuningRule
        #region IsEventClassCandidate
        public bool IsEventClassCandidate
        {
            get { return ArrFunc.IsFilled(m_EventClassCandidate); }
        }
        #endregion IsEventClassCandidate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AccountingEventClass(string pConnectionString, IProductBase pProduct, int pIdI, DateTime pDtDemand, int pIdE, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            m_ProductBase = pProduct;
            m_IdE = pIdE;
            m_IdI = pIdI;
            m_DtDemand = pDtDemand;
            //20080901 PL WARNING: Suppression du test à l'égard de la date jour pour la génération des EventClass comptable (ex. CLN, CLA, ...)
            //m_DtToDay = OTCmlHelper.GetRDBMSDateSys(m_Cs);
            //if (0 <= DateTime.Compare(m_DtToDay, m_DtDemand))
            //{
            LoadAccountingTuningRule();
            if (IsExistAccountingTuningRule)
            {
                CalculEventClassCandidate2();
            }
            //}
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20231127 [WI752] Exclusion de FpML 4.2
        public EFS_AccountingEventClass(string pConnectionString, int pIdI, DateTime pDtDemand, int pIdE, DataDocumentContainer pDataDocument)
            : this(pConnectionString, (IProductBase)new FpML.v44.Shared.Product(), pIdI, pDtDemand, pIdE, pDataDocument) { }
        #endregion Constructors
        #region Methods
        #region AddRowAccountingEventClass
        // RD 20150330 [20847] Modify
        // EG 20150716 [21175] Test pDbTransaction
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        // EG 20231127 [WI752] Exclusion de FpML 4.2
        public static void AddRowAccountingEventClass(string pCs, IDbTransaction pDbTransaction, int pIdE, int pIdI, DateTime pDate, DataDocumentContainer pDataDocument)
        {

            DataParameters sqlParam = new DataParameters();
            sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDE), pIdE);
            
            // RD 20150330 [20847] Supprimer l'alias IDPARENT de la colonne IDE
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + @"ec.IDEC as ID, ec.IDE," + Cst.CrLf;
            sqlSelect += "ec.EVENTCLASS, ec.DTEVENT, ec.DTEVENTFORCED, ec.ISPAYMENT,ec.NETMETHOD,ec.IDNETCONVENTION,ec.IDNETDESIGNATION,ec.EXTLLINK" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec " + Cst.CrLf;
            //				
            SQLWhere sqlWhere = new SQLWhere("ec.IDE=@IDE");
            //			
            string sql = sqlSelect.ToString() + sqlWhere.ToString();
            //CSManager csManager = new CSManager(pCs);

            // EG 20150716 [21175] 
            DataTable dtEventClass = DataHelper.ExecuteDataTable(pCs, pDbTransaction, sql, sqlParam.GetArrayDbParameter());

            if (null != dtEventClass)
            {
                AddRowAccountingEventClass(pCs, (IProductBase)new FpML.v44.Shared.Product(), pIdE, pIdI, pDate, dtEventClass, pDataDocument);
                // EG 20150716 [21175]
                DataHelper.ExecuteDataAdapter(pCs, pDbTransaction, sqlSelect.ToString(), dtEventClass);
            }
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static void AddRowAccountingEventClass(string pCs, IProductBase pProduct, int pIdE, int pIdI, DateTime pDate, DataTable pDataEventClass, DataDocumentContainer pDataDocument)
        {

            EFS_AccountingEventClass accountingEventClass = new EFS_AccountingEventClass(pCs, pProduct, pIdI, pDate, pIdE, pDataDocument);
            //
            if (accountingEventClass.IsEventClassCandidate)
            {
                foreach (EventClass eventClassCandidate in accountingEventClass.EventClassCandidate)
                {
                    DataRow rowEventClassAccounting = pDataEventClass.NewRow();
                    rowEventClassAccounting.BeginEdit();
                    rowEventClassAccounting["IDE"] = eventClassCandidate.idE;
                    rowEventClassAccounting["EVENTCLASS"] = eventClassCandidate.code;
                    rowEventClassAccounting["DTEVENT"] = eventClassCandidate.dtEvent.DateValue;
                    rowEventClassAccounting["DTEVENTFORCED"] = eventClassCandidate.dtEventForced.DateValue;
                    rowEventClassAccounting["ISPAYMENT"] = false;
                    rowEventClassAccounting.EndEdit();
                    pDataEventClass.Rows.Add(rowEventClassAccounting);
                }
            }

        }
        #endregion AddRowAccountingEventClass
        #region AdjustedDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private DateTime AdjustedDate(DateTime pUnadjustedDate, string pIdBC, BusinessDayConventionEnum pBDC)
        {

            IBusinessDayAdjustments bda = m_ProductBase.CreateBusinessDayAdjustments(pBDC, pIdBC);
            //Suppression de l'heure pour le contrôle Holiday
            TimeSpan tsUnadjustedDate = pUnadjustedDate.TimeOfDay;
            DateTime dtUnadjustedDate = pUnadjustedDate.Date;
            EFS_AdjustableDate efs_AdjustableDate = new EFS_AdjustableDate(m_Cs, dtUnadjustedDate, bda, m_DataDocument);
            DateTime dtAdjustedDate = efs_AdjustableDate.adjustedDate.DateValue;
            //Rajout de l'heure supprimer pour le contrôle Holiday
            dtAdjustedDate = dtAdjustedDate.Add(tsUnadjustedDate);
            return dtAdjustedDate;

        }

        #endregion AdjustedDate
        #region CalculEventClassCandidate

        //PL 20130904 New method using GetEndDateOfLastPeriod() method  
        private void CalculEventClassCandidate2()
        {
            ArrayList aEventClass = new ArrayList();

            foreach (DataRow row in m_DtAccountingTuningRule.Rows)
            {
                int periodMultiplier = Convert.ToInt32(row["PERIODMLTP"]);
                PeriodEnum period = StringToEnum.Period(row["PERIOD"].ToString());
                RollConventionEnum rollConvention = StringToEnum.RollConvention(row["ROLLCONV"].ToString());

                DateTime dtLastPeriod = DateTime.MinValue;
                if (Tools.GetEndDateOfLastPeriod(periodMultiplier, period, rollConvention, m_DtDemand, ref dtLastPeriod))
                {
                    BusinessDayConventionEnum bdc = StringToEnum.BusinessDayConvention(row["BDC"].ToString());
                    string idBC = row["IDBC"].ToString();
                    dtLastPeriod = AdjustedDate(dtLastPeriod, idBC, bdc);
                    if (0 == DateTime.Compare(dtLastPeriod, m_DtDemand))
                    {
                        EventClass eventClass = new EventClass
                        {
                            idE = m_IdE,
                            code = row["EVENTCLASS"].ToString(),
                            dtEvent = new EFS_Date()
                            {
                                DateValue = m_DtDemand
                            },
                            dtEventSpecified = DtFunc.IsDateTimeFilled(m_DtDemand),
                            dtEventForced = new EFS_Date
                            {
                                DateValue = OTCmlHelper.GetAnticipatedDate(m_Cs, m_DtDemand)
                            },
                            dtEventForcedSpecified = DtFunc.IsDateTimeFilled(m_DtDemand)
                        };
                        eventClass.codeSpecified = StrFunc.IsFilled(eventClass.code);
                        aEventClass.Add(eventClass);
                    }
                }

                if (0 < aEventClass.Count)
                {
                    m_EventClassCandidate = (EventClass[])aEventClass.ToArray(typeof(EventClass));
                }
            }
        }
        #endregion CalculEventClassCandidate
        #region LoadAccountingTuningRule
        // EG 20180307 [23769] Gestion dbTransaction
        private void LoadAccountingTuningRule()
        {

            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
            sqlQuery += "ar.PERIODMLTP, ar.PERIOD, ar.ROLLCONV, ar.BDC, ar.IDBC, ar.EVENTCLASS" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCOUNTING + " ar" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, "ar") + Cst.CrLf;
            //FI 20100325 appel à SQLInstrCriteria à la place de InstrTools.GetSQLCriteriaInstr
            //sqlQuery += InstrTools.GetSQLCriteriaInstr(m_Cs, m_IdI, "ar", RoleGInstr.TUNING);
            SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(m_Cs, null, m_IdI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
            sqlQuery += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction(m_Cs, "ar", RoleGInstr.TUNING);
            sqlQuery += SQLCst.ORDERBY + "ar.IDACCOUNTING" + SQLCst.ASC;
            //
            DataSet ds = DataHelper.ExecuteDataset(m_Cs, CommandType.Text, sqlQuery);
            m_DtAccountingTuningRule = ds.Tables[0];

        }
        #endregion LoadAccountingTuningRule
        #endregion Methods
    }
    #endregion EFS_AccountingEventClass

    #region EFS_AdjustableDate
    /// <summary>
    /// Classe de calcul qui permet d'obtenir un date ajustée à partir d'une date non ajustée
    /// </summary>
    // EG 20180411 [23769] Set & Use dbTransaction 
    public class EFS_AdjustableDate : ICloneable, IEFS_AdjustableDate
    {
        #region Members
        private readonly string m_Cs;
        private readonly IDbTransaction m_DbTransaction;
        private readonly DataDocumentContainer m_DataDocument;
        private IAdjustableDate m_adjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adjustableDateSpecified;
        //
        public IAdjustedDate adjustedDate;
        //
        public DayTypeEnum dayType;
        #endregion Members
        //
        #region Accessors
        #region adjustableDate
        public IAdjustableDate AdjustableDate
        {
            get { return m_adjustableDate; }
            set
            {
                m_adjustableDate = value;
                adjustableDateSpecified = (Convert.ToDateTime(null) != m_adjustableDate.UnadjustedDate.DateValue) &&
                                          (null != m_adjustableDate.DateAdjustments);
                Calc();
            }
        }
        #endregion adjustableDate
        #region AdjustedEventDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEventDate
        {
            get
            {
                EFS_Date adjustedEventDate = new EFS_Date
                {
                    DateValue = adjustedDate.DateValue
                };
                return adjustedEventDate;
            }
        }
        #endregion AdjustedEventDate
        #region UnadjustedEventDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEventDate
        {
            get
            {
                EFS_Date unadjustedEventDate = new EFS_Date
                {
                    DateValue = m_adjustableDate.UnadjustedDate.DateValue
                };
                return unadjustedEventDate;
            }
        }
        #endregion UnadjustedEventDate
        #region EventDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EventDate
        {
            get
            {
                return new EFS_EventDate(this);

            }
        }
        #endregion EventDate
        #endregion Accessors
        //
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustableDate(string pCS, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument;
        }
        public EFS_AdjustableDate(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;
            m_DataDocument = pDataDocument;
        }
        /// <summary>
        ///     
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAdjustableDate">Représente la date non ajustée et les ajustements</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustableDate(string pCS, IAdjustableDate pAdjustableDate, DataDocumentContainer pDataDocument)
            : this(pCS, pAdjustableDate, DayTypeEnum.Business, pDataDocument) { }
        // EG 20180530 [23980] Add pDbTransaction parameter
        public EFS_AdjustableDate(string pCS, IDbTransaction pDbTransaction, IAdjustableDate pAdjustableDate, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pAdjustableDate, DayTypeEnum.Business, pDataDocument) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAdjustableDate">Représente la date non ajustée et les ajustements</param>
        /// <param name="pDayType">Type de jours fériés</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustableDate(string pCS, IAdjustableDate pAdjustableDate, DayTypeEnum pDayType, DataDocumentContainer pDataDocument)
            : this(pCS, null, pAdjustableDate, pDayType, pDataDocument){}
        // EG 20180530 [23980] Add pDbTransaction parameter
        public EFS_AdjustableDate(string pCS, IDbTransaction pDbTransaction, IAdjustableDate pAdjustableDate, DayTypeEnum pDayType, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pDataDocument)
        {
            dayType = pDayType;
            AdjustableDate = (IAdjustableDate)pAdjustableDate.Clone();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUnadjustedDate">Date non ajustée</param>
        /// <param name="pBusinessDayAdjustments">Ajustements</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustableDate(string pCS, DateTime pUnadjustedDate, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pCS, pUnadjustedDate, pBusinessDayAdjustments, DayTypeEnum.Business, pDataDocument) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUnadjustedDate">Date non ajustée</param>
        /// <param name="pBusinessDayAdjustments">Ajustements</param>
        /// <param name="pDayType">Type de jours fériés</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustableDate(string pCS, DateTime pUnadjustedDate, IBusinessDayAdjustments pBusinessDayAdjustments, DayTypeEnum pDayType, DataDocumentContainer pDataDocument)
            : this(pCS, pDataDocument)
        {
            dayType = pDayType;
            AdjustableDate = pBusinessDayAdjustments.CreateAdjustableDate(pUnadjustedDate);
        }
        public EFS_AdjustableDate(string pCS, IDbTransaction pDbTransaction, DateTime pUnadjustedDate, IBusinessDayAdjustments pBusinessDayAdjustments, DayTypeEnum pDayType, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pDataDocument)
        {
            dayType = pDayType;
            AdjustableDate = pBusinessDayAdjustments.CreateAdjustableDate(pUnadjustedDate);
        }
        //dbTransaction, 
        #endregion Constructors
        //
        #region Methods
        #region Calc
        /// <summary>
        /// An update of adjustableDate property triggers the automatic calculation of adjustedDate
        /// </summary>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel Calc()
        {
            DateTime adjustedDate = Convert.ToDateTime(null);
            Cst.ErrLevel ret;
            if (adjustableDateSpecified)
            {
                ret = Cst.ErrLevel.SUCCESS;
                this.adjustedDate = AdjustableDate.GetAdjustedDate();
                adjustedDate = this.adjustedDate.DateValue;
                if ((null != AdjustableDate.DateAdjustments) &&
                    (BusinessDayConventionEnum.NONE != AdjustableDate.DateAdjustments.BusinessDayConvention) &&
                    (BusinessDayConventionEnum.NotApplicable != AdjustableDate.DateAdjustments.BusinessDayConvention))
                {
                    EFS_BusinessCenters efs_BusinessCenters = new EFS_BusinessCenters(m_Cs, m_DbTransaction, AdjustableDate.DateAdjustments, m_DataDocument);
                    int guard = 0;
                    DateTime unadjustedDate = adjustedDate;
                    bool isMODReverse = false;
                    while (efs_BusinessCenters.IsHoliday(adjustedDate, dayType))
                    {
                        guard++;
                        #region BusinessDayConvention
                        switch (AdjustableDate.DateAdjustments.BusinessDayConvention)
                        {
                            case BusinessDayConventionEnum.FOLLOWING:
                                adjustedDate = adjustedDate.AddDays(1);
                                break;
                            case BusinessDayConventionEnum.PRECEDING:
                                adjustedDate = adjustedDate.AddDays(-1);
                                break;
                            case BusinessDayConventionEnum.MODFOLLOWING:
                            case BusinessDayConventionEnum.FRN:
                                if (isMODReverse)
                                    adjustedDate = adjustedDate.AddDays(-1);
                                else
                                {
                                    adjustedDate = adjustedDate.AddDays(1);
                                    if (unadjustedDate.Month != adjustedDate.Month)
                                    {
                                        isMODReverse = true;
                                        adjustedDate = unadjustedDate.AddDays(-1);
                                    }
                                }
                                break;
                            case BusinessDayConventionEnum.MODPRECEDING:
                                if (isMODReverse)
                                    adjustedDate = adjustedDate.AddDays(1);
                                else
                                {
                                    adjustedDate = adjustedDate.AddDays(-1);
                                    if (unadjustedDate.Month != adjustedDate.Month)
                                    {
                                        isMODReverse = true;
                                        adjustedDate = unadjustedDate.AddDays(1);
                                    }
                                }
                                break;
                            default:
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Invalid BusinessDayConvention [{0}]",
                                    AdjustableDate.DateAdjustments.BusinessDayConvention.ToString());
                        }
                        #endregion BusinessDayConvention

                        if (guard == 999)
                            throw new Exception("ERR: BusinessCenter.IsHoliday Loop");
                    }
                }
            }
            else
            {
                ret = Cst.ErrLevel.ABORTED;
            }

            if (ret == Cst.ErrLevel.SUCCESS)
            {
                this.adjustedDate.DateValue = adjustedDate;
            }

            return ret;
        }
        #endregion Calc
        #endregion Methods
        //
        #region ICloneable Members
        #region ICloneable
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            EFS_AdjustableDate clone = new EFS_AdjustableDate(this.m_Cs, this.m_DataDocument)
            {
                m_adjustableDate = (IAdjustableDate)this.m_adjustableDate.Clone(),
                adjustableDateSpecified = this.adjustableDateSpecified
            };
            clone.Calc();
            return clone;
        }
        #endregion ICloneable
        #endregion ICloneable Members
        //
        #region IEFS_AdjustableDate Members
        bool IEFS_AdjustableDate.AdjustableDateSpecified { get { return this.adjustableDateSpecified; } }
        DateTime IEFS_AdjustableDate.AdjustedDate { get { return this.adjustedDate.DateValue; } }
        DateTime IEFS_AdjustableDate.UnadjustedDate { get { return this.AdjustableDate.UnadjustedDate.DateValue; } }
        #endregion IEFS_AdjustableDate Members
    }
    #endregion EFS_AdjustableDate
    #region EFS_AdjustableDates
    public class EFS_AdjustableDates : ICloneable
    {
        #region Members
        public EFS_AdjustableDate[] adjustableDates;
        #endregion Members
        #region Accessors
        #region AdjustedEventDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public static EFS_Date AdjustedEventDate
        {
            get
            {

                return new EFS_Date();

            }
        }
        #endregion AdjustedEventDate
        #region EventDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public static EFS_EventDate EventDate
        {
            get
            {

                return new EFS_EventDate();

            }
        }
        #endregion EventDate
        #endregion Accessors
        #region Constructors
        public EFS_AdjustableDates() { }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_AdjustableDates clone = new EFS_AdjustableDates
            {
                adjustableDates = (EFS_AdjustableDate[])this.adjustableDates.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion EFS_AdjustableDates
    #region EFS_AdjustablePeriod
    public class EFS_AdjustablePeriod : ICloneable
    {
        #region Members
        public EFS_AdjustableDate adjustableDate1;
        public EFS_AdjustableDate adjustableDate2;
        #endregion Members
        #region Accessors
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {
                if (null != adjustableDate1)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = adjustableDate1.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;
            }
        }
        #endregion AdjustedStartPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                if (null != adjustableDate1)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion UnadjustedStartPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                if (null != adjustableDate1)
                    return new EFS_EventDate(adjustableDate1);
                else
                    return null;

            }
        }
        #endregion StartPeriod
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                if (null != adjustableDate2)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = adjustableDate2.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion AdjustedEndPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                if (null != adjustableDate2)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion UnadjustedEndPeriod
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                if (null != adjustableDate2)
                    return new EFS_EventDate(adjustableDate2);
                else
                    return null;

            }
        }
        #endregion EndPeriod
        #endregion Accessors
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_AdjustablePeriod clone = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)this.adjustableDate1.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)this.adjustableDate2.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable
    }
    #endregion EFS_AdjustablePeriod
    #region EFS_AdjustedSettlement
    // EG 20091230 Replace EFS_PresSettlement
    // EG 20180307 [23769] Gestion dbTransaction
    public class EFS_AdjustedSettlement
    {
        #region Members
        protected string m_Cs;
        protected IDbTransaction m_DbTransaction;
        protected DataDocumentContainer m_DataDocument;
        protected PreSettlementDateMethodDeterminationEnum m_Method;
        protected DateTime m_SettlementDate;
        protected string m_CU1;
        protected string m_CU2;
        protected IOffset m_Offset;
        protected DateTime m_OffsetSettlementDate;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pSettlementDate.DateValue, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pSettlementDate.DateValue, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pSettlementDate.DateValue, pCU1, pCU2, pOffset, pMethod, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pSettlementDate, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pSettlementDate, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AdjustedSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;
            m_DataDocument = pDataDocument;
            m_SettlementDate = pSettlementDate;
            m_CU1 = pCU1;
            m_CU2 = pCU2;
            m_Offset = pOffset;
            m_Method = PreSettlementDateMethodDeterminationEnum.BothCurrencies;
            if (System.Enum.IsDefined(typeof(PreSettlementDateMethodDeterminationEnum), pMethod))
                m_Method = (PreSettlementDateMethodDeterminationEnum)System.Enum.Parse(typeof(PreSettlementDateMethodDeterminationEnum), pMethod);
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public void Calc()
        {
            IBusinessCenters businessCenters;
            EFS_Offset offset;
            switch (m_Method)
            {
                case PreSettlementDateMethodDeterminationEnum.BothCurrencies:
                case PreSettlementDateMethodDeterminationEnum.BothCurrenciesAndUSD:
                    businessCenters = m_Offset.GetBusinessCentersCurrency(m_Cs, m_DbTransaction, m_CU1, m_CU2);
                    offset = new EFS_Offset(m_Cs, m_Offset, m_SettlementDate, Convert.ToDateTime(null), businessCenters, m_DataDocument);
                    m_OffsetSettlementDate = offset.offsetDate[0];
                    if (PreSettlementDateMethodDeterminationEnum.BothCurrenciesAndUSD == m_Method)
                        m_OffsetSettlementDate = GetOpenDay(m_OffsetSettlementDate, "USD");
                    break;
                case PreSettlementDateMethodDeterminationEnum.OneByOneCurrency:
                case PreSettlementDateMethodDeterminationEnum.OneByOneCurrencyAndUSD:
                    if (("USD" != m_CU1) && ("USD" != m_CU2))
                    {
                        businessCenters = m_Offset.GetBusinessCentersCurrency(m_Cs, m_DbTransaction, m_CU1);
                        offset = new EFS_Offset(m_Cs, m_Offset, m_SettlementDate, Convert.ToDateTime(null), businessCenters, m_DataDocument);
                        m_OffsetSettlementDate = GetOpenDay(offset.offsetDate[0], m_CU2);
                        if (PreSettlementDateMethodDeterminationEnum.OneByOneCurrencyAndUSD == m_Method)
                            m_OffsetSettlementDate = GetOpenDay(m_OffsetSettlementDate, "USD");
                    }
                    else
                    {
                        string m_WorkCurrency = ("USD" == m_CU1) ? m_CU2 : m_CU1;
                        businessCenters = m_Offset.GetBusinessCentersCurrency(m_Cs, m_DbTransaction, m_WorkCurrency);
                        offset = new EFS_Offset(m_Cs, m_Offset, m_SettlementDate, Convert.ToDateTime(null), businessCenters, m_DataDocument);
                        m_WorkCurrency = ("USD" == m_CU1) ? m_CU1 : m_CU2;
                        m_OffsetSettlementDate = GetOpenDay(offset.offsetDate[0], m_WorkCurrency);
                    }
                    break;
            }

        }
        #endregion Calc
        #region GetOpenDay
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        private DateTime GetOpenDay(DateTime pDate, string pCurrency)
        {
            int multiplier = (0 < m_Offset.PeriodMultiplier.IntValue) ? 1 : -1;
            DateTime dtTemp = pDate;
            EFS_BusinessCenters businessCenters = new EFS_BusinessCenters(m_Cs, m_Offset.GetBusinessCentersCurrency(m_Cs, null, pCurrency), m_DataDocument);
            while (true)
            {
                if (false == businessCenters.IsHoliday(dtTemp, m_Offset.DayType))
                    break;
                dtTemp = dtTemp.AddDays(multiplier);
            }
            return dtTemp;
        }
        #endregion GetOpenDay
        #endregion Methods
    }
    #endregion EFS_AdjustedSettlement
    #region EFS_AsianFeatures
    public class EFS_AsianFeatures
    {
        #region Members
        public AveragingInOutEnum averagingInOut;
        public bool averagingPeriodInSpecified;
        public EFS_EquityAveragingPeriod averagingPeriodIn;
        public bool averagingPeriodOutSpecified;
        public EFS_EquityAveragingPeriod averagingPeriodOut;
        public bool isLookBackMethod;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AsianFeatures(string pConnectionString, IAsian pAsian, EFS_Underlyer pUnderlyer, IBusinessDayAdjustments pBusinessDayAdjustments, 
            DataDocumentContainer pDataDocument)
        {
            averagingInOut = pAsian.AveragingInOut;
            #region AveragingPeriodIn
            averagingPeriodInSpecified = pAsian.AveragingPeriodInSpecified;
            if (pAsian.AveragingPeriodInSpecified)
                averagingPeriodIn = new EFS_EquityAveragingPeriod(pConnectionString, EventTypeFunc.AveragingIn,
                    pUnderlyer, pAsian.AveragingPeriodIn, pBusinessDayAdjustments, pAsian.LookBackMethodSpecified, pDataDocument);
            #endregion AveragingPeriodIn
            #region AveragingPeriodOut
            averagingPeriodOutSpecified = pAsian.AveragingPeriodOutSpecified;
            if (averagingPeriodOutSpecified)
                averagingPeriodOut = new EFS_EquityAveragingPeriod(pConnectionString, EventTypeFunc.AveragingOut,
                    pUnderlyer, pAsian.AveragingPeriodOut, pBusinessDayAdjustments, pAsian.LookBackMethodSpecified, pDataDocument);
            #endregion AveragingPeriodOut
        }
        #endregion Constructors
    }
    #endregion EFS_AsianFeatures
    #region EFS_Asset
    // EG 20140120 Report 3.7
    // EG 20140702 Upd Nullable<QuoteEnum>
    // EG 20140702 Add AssetCategory
    public class EFS_Asset
    {
        #region Members
        public int idAsset;
        public Nullable<Cst.UnderlyingAsset> assetCategory;
        #region Fx/RateIndex
        public DateTime time;
        public string idBC;
        public string quoteTiming;
        public string quoteSide;
        public string primaryRateSrc;
        public string primaryRateSrcPage;
        public string primaryRateSrcHead;
        #endregion Fx/RateIndex
        #region Equity
        public string description;
        public string clearanceSystem;
        public int IdMarket;
        public string IdMarketIdentifier;
        public string IdMarketISO10383_ALPHA4;
        public string IdMarketFIXML_SecurityExchange;
        public string idC;
        public EFS_Decimal weight;
        public string unitWeight;
        public UnitTypeEnum unitTypeWeight;
        #endregion Equity
        // EG 20140120 Report 3.7
        #region ExchangeTradedContract / Index / EquityAsset
        public string assetSymbol;
        public string isinCode;
        #endregion ExchangeTradedContract / Index / EquityAsset
        #region ExchangeTradedContract
        public string contractIdentifier;
        public string contractDisplayName;
        public string contractSymbol;
        public CfiCodeCategoryEnum category;
        public FixML.Enum.PutOrCallEnum putOrCall;
        public EFS_Decimal strikePrice;
        public EFS_Decimal contractMultiplier;
        public DateTime maturityDate;
        public DateTime maturityDateSys;
        public DateTime deliveryDate;
        public DateTime lastTradingDay;
        public decimal nominalValue;
        // EG 20140325 [19766]
        public int instrumentNum;
        public int instrumentDen;
        #endregion ExchangeTradedContract
        #region CommodityContract
        public string commodityClass;
        public string commodityType;
        public string deliveryPoint;
        public string tradableType;
        public string duration;
        public string deliveryTimezone;
        public string commodityQuality;
        #endregion CommodityContract
        #endregion Members
        #region Accessors
        // EG 20140905 New Replace member assetType
        public Nullable<QuoteEnum> AssetType
        {
            get
            {
                Nullable<QuoteEnum> assetType = null;
                if (assetCategory.HasValue)
                    assetType = AssetTools.ConvertUnderlyingAssetToQuoteEnum(assetCategory.Value);
                return assetType;
            }
        }
        #endregion Accessors
    }
    #endregion EFS_Asset

    #region EFS_BarrierFeatures
    public class EFS_BarrierFeatures
    {
        #region Members
        public bool barrierCapSpecified;
        public EFS_TriggerEvent barrierCap;
        public bool barrierFloorSpecified;
        public EFS_TriggerEvent barrierFloor;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BarrierFeatures(string pConnectionString, IBarrier pBarrier, EFS_Underlyer pUnderlyer, EFS_OptionStrikeBase pStrike,
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            #region BarrierCap
            barrierCapSpecified = pBarrier.BarrierCapSpecified;
            if (barrierCapSpecified)
                barrierCap = new EFS_TriggerEvent(pConnectionString, EventTypeFunc.Cap, pUnderlyer, pBarrier.BarrierCap, pStrike, 
                    pBusinessDayAdjustments, pDataDocument);
            #endregion BarrierCap
            #region BarrierFloor
            barrierFloorSpecified = pBarrier.BarrierFloorSpecified;
            if (barrierFloorSpecified)
                barrierFloor = new EFS_TriggerEvent(pConnectionString, EventTypeFunc.Floor, pUnderlyer, pBarrier.BarrierFloor, pStrike, 
                    pBusinessDayAdjustments, pDataDocument);
            #endregion BarrierFloor
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_Barrier
    #region EFS_BasisRate
    public class EFS_BasisRate
    {
        #region Members
        public string currency1;
        public string currency2;
        public QuoteBasisEnum quoteBasis;
        public EFS_Decimal rate;
        #endregion Members
    }
    #endregion EFS_BasisRate
    #region EFS_BasisRates
    public class EFS_BasisRates
    {
        #region Members
        public EFS_BasisRate nativeBasisRate;
        public EFS_StrikeBasisRate callPerPutBasisRate;
        public EFS_StrikeBasisRate putPerCallBasisRate;
        public EFS_StrikeBasisRate settlementBasisRate;
        #endregion Members
    }
    #endregion EFS_BasisRates
    #region EFS_Basket
    // EG 20140702 Upd 
    public class EFS_Basket : EFS_Underlyer
    {
        #region Members
        public bool basketConstituentSpecified;
        public EFS_BasketConstituent[] basketConstituent;
        #endregion Members
        #region Accessors
        #endregion Accessors

        #region Constructors
        public EFS_Basket(string pCS, OptionTypeEnum pOptionType, IReference pBuyerPartyReference, IReference pSellerPartyReference, IBasket pBasket)
            : base(pCS, EventCodeFunc.Basket, EventCodeFunc.BasketValuationDate)
        {
            SetBuyerSellerPartyReference(pOptionType, pBuyerPartyReference, pSellerPartyReference);
            SetOpenUnits((IOpenUnits)pBasket);
            SetBasketConstituent(pBasket);
        }
        public EFS_Basket(string pCS, IReference pPayerPartyReference, IReference pReceiverPartyReference, IBasket pBasket)
            : base(pCS, EventCodeFunc.Basket, EventCodeFunc.BasketValuationDate)
        {
            SetPayerReceiverPartyReference(pPayerPartyReference, pReceiverPartyReference);
            SetOpenUnits((IOpenUnits)pBasket);
            SetBasketConstituent(pBasket);
        }
        #endregion Constructors
        #region SetBasketConstituent
        private void SetBasketConstituent(IBasket pBasket)
        {
            basketConstituentSpecified = ArrFunc.IsFilled(pBasket.BasketConstituent);
            if (basketConstituentSpecified)
            {
                ArrayList aBasketConstituent = new ArrayList();
                foreach (IBasketConstituent item in pBasket.BasketConstituent)
                {
                    aBasketConstituent.Add(new EFS_BasketConstituent(cs, payerPartyReference, receiverPartyReference, item));
                }
                if (0 < aBasketConstituent.Count)
                    basketConstituent = (EFS_BasketConstituent[])aBasketConstituent.ToArray(typeof(EFS_BasketConstituent));
            }
        }
        #endregion SetBasketConstituent
    }
    #endregion EFS_Basket
    #region EFS_BasketConstituent
    // EG 20140702 Upd 
    public class EFS_BasketConstituent : EFS_Underlyer
    {
        #region Accessors
        #region UnitTypeUnderlyerConstituent
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnitTypeUnderlyerConstituent
        {
            get
            {
                return unitType.ToString();

            }
        }
        #endregion UnitTypeUnderlyerConstituent
        #region UnitUnderlyerConstituent
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnitUnderlyerConstituent
        {
            get
            {
                return unit;

            }
        }
        #endregion UnitUnderlyerConstituent
        #region UnitValueUnderlyerConstituent
        public EFS_Decimal UnitValueUnderlyerConstituent(EFS_Decimal pTotalUnderlyer)
        {

            if ((UnitTypeEnum.Qty == unitType) || (UnitTypeEnum.Percentage == unitType))
                return new EFS_Decimal(unitValue.DecValue * pTotalUnderlyer.DecValue);
            else
                return unitValue;

        }
        #endregion UnitValueUnderlyerConstituent
        #endregion Accessors
        #region Constructors
        public EFS_BasketConstituent(string pCS, IReference pPayerPartyReference, IReference pReceiverPartyReference, IBasketConstituent pBasketConstituent)
            : base(pCS, EventCodeFunc.UnderlyerBasketConstituent, EventCodeFunc.UnderlyerValuationDate)
        {
            SetPayerReceiverPartyReference(pPayerPartyReference, pReceiverPartyReference);
            SetOpenUnits(pBasketConstituent);
        }
        #endregion Constructors
    }
    #endregion EFS_BasketConstituent
    #region EFS_BusinessCenters
    // EG 20180411 [23769] Set & Use dbTransaction 
    public class EFS_BusinessCenters : ICloneable
    {
        #region Members
        protected string m_Cs;
        protected IDbTransaction m_DbTransaction;
        protected DataDocumentContainer m_DataDocument;
        protected IBusinessCenters m_businessCenters;
        public bool businessCentersSpecified;
        protected ArrayList aHolidayWeekly = new ArrayList();
        protected ArrayList aHolidayMonthly = new ArrayList();
        protected ArrayList aHolidayYearly = new ArrayList();
        protected ArrayList aHolidayMisc = new ArrayList();
        protected ArrayList aHolidayCalculated = new ArrayList();
        protected string listOfBusinessCenters = string.Empty;

        #endregion Members
        #region Accessors

        /// <summary>
        /// Obtient ou définit les business Centers
        /// </summary>
        public IBusinessCenters BusinessCenters
        {
            get { return m_businessCenters; }
            set
            {
                m_businessCenters = value;
                businessCentersSpecified = (null != m_businessCenters) &&
                                           (ArrFunc.IsFilled(m_businessCenters.BusinessCenter));
            }
        }

        /// <summary>
        /// Obtient ou définit un indicateur afin de charger les descriptions des mots clés
        /// </summary>
        public Boolean IsLoadDescription
        {
            get;
            set;
        }




        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BusinessCenters(string pCS) : this(pCS, null) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BusinessCenters(string pCS, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument; 
        }
        public EFS_BusinessCenters(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;
            m_DataDocument = pDataDocument; 
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        public EFS_BusinessCenters(string pCS, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pCS, null, pBusinessDayAdjustments, pDataDocument)
        { }
        public EFS_BusinessCenters(string pCS, IDbTransaction pDbTransaction, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pDataDocument)
        {
            //businessCentersSpecified = false;
            if (pBusinessDayAdjustments.BusinessCentersDefineSpecified)
            {
                BusinessCenters = (IBusinessCenters)pBusinessDayAdjustments.BusinessCentersDefine;
            }
            else if (pBusinessDayAdjustments.BusinessCentersReferenceSpecified)
            {
                if (null != m_DataDocument)
                {
                    object _obj = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, pBusinessDayAdjustments.BusinessCentersReferenceValue);
                if (null != _obj)
                {
                    if (_obj is IBusinessCenters centers)
                        BusinessCenters = centers;
                    else if (_obj is IBusinessCenter)
                    {
                        IBusinessCenter _bc = _obj as IBusinessCenter;
                            BusinessCenters = m_DataDocument.CurrentTrade.Product.ProductBase.CreateBusinessCenters(_bc.Value);
                    }
                }

                }
                businessCentersSpecified = (null != BusinessCenters);
            }

            LoadHoliday();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pBusinessCenters">Null est une valeur autorisée</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190613 [24683] Use pDbTransaction
        public EFS_BusinessCenters(string pCS, IBusinessCenters pBusinessCenters, DataDocumentContainer pDataDocument)
            : this(pCS, null, pBusinessCenters, pDataDocument)
        {

        }
        public EFS_BusinessCenters(string pCS, IDbTransaction pDbTransaction, IBusinessCenters pBusinessCenters, DataDocumentContainer pDataDocument)
            : this(pCS, pDbTransaction, pDataDocument)
        {
            if (null != pBusinessCenters)
                BusinessCenters = pBusinessCenters;
            businessCentersSpecified = (pBusinessCenters != null);

            LoadHoliday();
        }
        #endregion Constructors

        #region Methods



        /// <summary>
        /// Charge les HOLIDAYWEEKLY
        /// </summary>
        /// FI 20130502 [18636] 
        // EG 20180411 [23769] Set & Use dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use Datatable instead of DataReader
        private void LoadHolidayWeekly()
        {
            aHolidayWeekly.Clear();
            if (businessCentersSpecified)
            {
                string SQLSelectHolidayWeekly = GetSQLSelectForHolidayTable(m_Cs, Cst.OTCml_TBL.HOLIDAYWEEKLY, IsLoadDescription, listOfBusinessCenters);
                DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, SQLSelectHolidayWeekly);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //PL 20131121
                        EFS_HolidayWeekly holidayWeekly = new EFS_HolidayWeekly(row["DDD"].ToString(),
                            Convert.ToBoolean(row["ISBANKINGBUSINESS"]),
                            Convert.ToBoolean(row["ISCOMMODITYBUSINESS"]),
                            Convert.ToBoolean(row["ISCURRENCYBUSINESS"]),
                            Convert.ToBoolean(row["ISEXCHANGEBUSINESS"]),
                            Convert.ToBoolean(row["ISSCHEDULEDTRADINGDAY"]));

                        // FI 20130502 [18636] add 
                        if (IsLoadDescription)
                            holidayWeekly.Description = Convert.ToString(row["DESCRIPTION"]);

                        aHolidayWeekly.Add(holidayWeekly);
                    }
                }
            }
        }

        /// <summary>
        /// Charge les HOLIDAYMONTHLY
        /// </summary>
        /// FI 20130502 [18636] 
        // EG 20180411 [23769] Set & Use dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190603 [24683] Use Datatable instead of DataReader
        private void LoadHolidayMonthly()
        {
            aHolidayMonthly.Clear();
            if (businessCentersSpecified)
            {
                string SQLSelectHolidayMonthly = GetSQLSelectForHolidayTable(m_Cs, Cst.OTCml_TBL.HOLIDAYMONTHLY, IsLoadDescription, listOfBusinessCenters);
                DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, SQLSelectHolidayMonthly);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //PL 20131121
                        EFS_HolidayMonthly holidayMonthly = new EFS_HolidayMonthly(row["POSITIONDAY"], row["DDD"], row["MM"], row["MARKDAY"],
                            Convert.ToBoolean(row["ISBANKINGBUSINESS"]),
                            Convert.ToBoolean(row["ISCOMMODITYBUSINESS"]),
                            Convert.ToBoolean(row["ISCURRENCYBUSINESS"]),
                            Convert.ToBoolean(row["ISEXCHANGEBUSINESS"]),
                            Convert.ToBoolean(row["ISSCHEDULEDTRADINGDAY"]));

                        // FI 20130502 [18636] add 
                        if (IsLoadDescription)
                            holidayMonthly.Description = Convert.ToString(row["DESCRIPTION"]);

                        aHolidayMonthly.Add(holidayMonthly);
                    }
                }
            }
        }


        /// <summary>
        /// Charge les HOLIDAYYEARLY
        /// </summary>
        /// FI 20130502 [18636] 
        // EG 20180411 [23769] Set & Use dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use Datatable instead of DataReader
        private void LoadHolidayYearly()
        {
            aHolidayYearly.Clear();
            if (businessCentersSpecified)
            {
                string SQLSelectHolidayYearly = GetSQLSelectForHolidayTable(m_Cs, Cst.OTCml_TBL.HOLIDAYYEARLY, IsLoadDescription, listOfBusinessCenters);
                DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, SQLSelectHolidayYearly);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //PL 20131121
                        EFS_HolidayYearly holidayYearly = new EFS_HolidayYearly(Convert.ToInt32(row["DD"]), Convert.ToInt32(row["MM"]),
                            //Convert.ToBoolean(dr["ISHOLIDAYWEEKLYSLIDE"]),
                            Convert.ToString(row["METHODOFADJUSTMENT"]),
                            Convert.ToBoolean(row["ISBANKINGBUSINESS"]),
                            Convert.ToBoolean(row["ISCOMMODITYBUSINESS"]),
                            Convert.ToBoolean(row["ISCURRENCYBUSINESS"]),
                            Convert.ToBoolean(row["ISEXCHANGEBUSINESS"]),
                            Convert.ToBoolean(row["ISSCHEDULEDTRADINGDAY"]));

                        // FI 20130502 [18636] add 
                        if (IsLoadDescription)
                            holidayYearly.Description = Convert.ToString(row["DESCRIPTION"]);

                        aHolidayYearly.Add(holidayYearly);
                    }
                }
            }
        }

        /// <summary>
        /// Charge les HOLIDAYMISC
        /// </summary>
        /// FI 20130502 [18636] 
        // EG 20180411 [23769] Set & Use dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use Datatable instead of DataReader
        private void LoadHolidayMisc()
        {
            aHolidayMisc.Clear();
            if (businessCentersSpecified)
            {
                string SQLSelectHolidayMisc = GetSQLSelectForHolidayTable(m_Cs, Cst.OTCml_TBL.HOLIDAYMISC, IsLoadDescription, listOfBusinessCenters);
                DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, SQLSelectHolidayMisc);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //PL 20131121
                        EFS_HolidayMisc holidayMisc = new EFS_HolidayMisc(Convert.ToDateTime(row["DTMISC"]),
                            Convert.ToBoolean(row["ISHOLIDAY"]),
                            Convert.ToBoolean(row["ISBANKINGBUSINESS"]),
                            Convert.ToBoolean(row["ISCOMMODITYBUSINESS"]),
                            Convert.ToBoolean(row["ISCURRENCYBUSINESS"]),
                            Convert.ToBoolean(row["ISEXCHANGEBUSINESS"]),
                            Convert.ToBoolean(row["ISSCHEDULEDTRADINGDAY"]));

                        // FI 20130502 [18636] add 
                        if (IsLoadDescription)
                            holidayMisc.Description = Convert.ToString(row["DESCRIPTION"]);

                        aHolidayMisc.Add(holidayMisc);
                    }
                }
            }
        }

        /// <summary>
        /// Charge les HOLIDAYCALCULATED
        /// </summary>
        /// FI 20130502 [18636] 
        // EG 20180411 [23769] Set & Use dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190603 [24683] Use Datatable instead of DataReader
        private void LoadHolidayCalculated()
        {
            aHolidayCalculated.Clear();
            if (businessCentersSpecified)
            {
                string SQLSelectHolidayCalculated = GetSQLSelectForHolidayTable(m_Cs, Cst.OTCml_TBL.HOLIDAYCALCULATED, IsLoadDescription, listOfBusinessCenters);
                DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, SQLSelectHolidayCalculated);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //PL 20131121
                        EFS_HolidayCalculated holidayCalculated = new EFS_HolidayCalculated(row["HOLIDAYNAME"].ToString(),
                            Convert.ToBoolean(row["ISBANKINGBUSINESS"]),
                            Convert.ToBoolean(row["ISCOMMODITYBUSINESS"]),
                            Convert.ToBoolean(row["ISCURRENCYBUSINESS"]),
                            Convert.ToBoolean(row["ISEXCHANGEBUSINESS"]),
                            Convert.ToBoolean(row["ISSCHEDULEDTRADINGDAY"]));

                        // FI 20130502 [18636] add 
                        if (IsLoadDescription)
                            holidayCalculated.Description = Convert.ToString(row["DESCRIPTION"]);

                        aHolidayCalculated.Add(holidayCalculated);
                    }
                }
            }
        }

        #region LoadHoliday
        /// <summary>
        /// 
        /// </summary>
        /// FI 20130502 [18636] method public
        public void LoadHoliday()
        {
            listOfBusinessCenters = GetListOfBusinessCenters();
            this.LoadHoliday2();
        }
        /// <summary>
        /// Charge toutes les dates fériée (Weekly,Monthly,Yearly, etc...)
        /// </summary>
        protected void LoadHoliday2()
        {
            LoadHolidayWeekly();
            LoadHolidayMonthly();
            LoadHolidayYearly();
            LoadHolidayMisc();
            LoadHolidayCalculated();
        }
        #endregion

        /// <summary>
        /// Retourne un ordre commande SQL Select sur la table BC en Inner sur la table concernée. 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pIsLoadDescription"></param>
        /// <returns></returns>
        /// 20130502 [18636] Static method, add parameter pIsLoadDescription
        private static string GetSQLSelectForHolidayTable(String pCS, Cst.OTCml_TBL pTable, bool pIsLoadDescription, string pListOfBusinessCenters)
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT);
            string aliasTbl = "th";
            switch (pTable)
            {
                case Cst.OTCml_TBL.HOLIDAYCALCULATED:
                    sqlSelect += aliasTbl + ".HOLIDAYNAME, ";
                    break;
                case Cst.OTCml_TBL.HOLIDAYMISC:
                    sqlSelect += aliasTbl + ".DTMISC, ";
                    sqlSelect += aliasTbl + ".ISHOLIDAY, ";
                    break;
                case Cst.OTCml_TBL.HOLIDAYMONTHLY:
                    sqlSelect += aliasTbl + ".POSITIONDAY, ";
                    sqlSelect += aliasTbl + ".DDD, ";
                    sqlSelect += aliasTbl + ".MM, ";
                    sqlSelect += aliasTbl + ".MARKDAY, ";
                    break;
                case Cst.OTCml_TBL.HOLIDAYWEEKLY:
                    sqlSelect += aliasTbl + ".DDD, ";
                    break;
                case Cst.OTCml_TBL.HOLIDAYYEARLY:
                    sqlSelect += aliasTbl + ".DD, ";
                    sqlSelect += aliasTbl + ".MM, ";
                    sqlSelect += aliasTbl + ".METHODOFADJUSTMENT, ";
                    break;
            }
            //PL 20131121
            sqlSelect += aliasTbl + ".ISBUSINESS as ISBANKINGBUSINESS, ";
            sqlSelect += aliasTbl + ".ISCOMMODITYBUSINESS, ";
            sqlSelect += aliasTbl + ".ISCURRENCYBUSINESS, ";
            sqlSelect += aliasTbl + ".ISEXCHANGEBUSINESS, ";
            sqlSelect += aliasTbl + ".ISSCHEDULEDTRADINGDAY";

            if (pIsLoadDescription)
            {
                sqlSelect += ",";
                sqlSelect += aliasTbl + ".DESCRIPTION";
            }

            sqlSelect += Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.BUSINESSCENTER.ToString() + " bc" + Cst.CrLf;
            //sqlSelect += OTCmlHelper.GetSQLJoin(m_Cs, pTable, true, "bc.IDBC", aliasTbl, true);
            sqlSelect += OTCmlHelper.GetSQLJoin(pCS, pTable, SQLJoinTypeEnum.Inner, "bc.IDBC", aliasTbl, DataEnum.EnabledOnly);
            sqlSelect += SQLCst.WHERE + "bc.IDBC in (" + pListOfBusinessCenters + ")";

            return sqlSelect.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetListOfBusinessCenters()
        {
            string list = string.Empty;

            businessCentersSpecified = (null != BusinessCenters) && ArrFunc.IsFilled(BusinessCenters.BusinessCenter);

            if (businessCentersSpecified)
            {
                foreach (IBusinessCenter businessCenter in BusinessCenters.BusinessCenter)
                    list += DataHelper.SQLString(businessCenter.Value) + ",";

                list = list.Substring(0, list.Length - 1);
            }
            return list;
        }

        /// <summary>
        /// retourne true si HOLIDAYWEEKLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] Add method
        public bool IsHolidayWeekly(DateTime pDate, DayTypeEnum pDayType)
        {
            return IsHolidayWeekly(pDate, pDayType, out _);
        }
        /// <summary>
        /// retourne true si HOLIDAYWEEKLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="pHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] public method, modification de la signature
        public bool IsHolidayWeekly(DateTime pDate, DayTypeEnum pDayType, out EFS_HolidayWeekly pHoliday)
        {
            bool isHoliday = false;
            pHoliday = null;

            string dayOfWeek = pDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
            foreach (EFS_HolidayWeekly hw in aHolidayWeekly)
            {
                if (hw.DDD == dayOfWeek)
                {
                    isHoliday = ((EFS_HolidayBase)hw).IsDayType(pDayType);
                    pHoliday = hw;
                    break;
                }
            }
            return isHoliday;
        }

        /// <summary>
        /// retourne true si HOLIDAYMONTHLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] add Method
        public bool IsHolidayMonthly(DateTime pDate, DayTypeEnum pDayType)
        {
            return IsHolidayMonthly(pDate, pDayType, out _);
        }
        /// <summary>
        /// retourne true si HOLIDAYMONTHLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="pHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] public method, modification de la signature
        public bool IsHolidayMonthly(DateTime pDate, DayTypeEnum pDayType, out EFS_HolidayMonthly pHoliday)
        {
            bool isHoliday = false;
            pHoliday = null;

            foreach (EFS_HolidayMonthly hm in aHolidayMonthly)
            {
                if ((hm.MM == pDate.Month) && (GetDayByPositionDay(hm, pDate.Year) == pDate.Day))
                {
                    isHoliday = ((EFS_HolidayBase)hm).IsDayType(pDayType);
                    pHoliday = hm;
                    break;
                }
            }
            return isHoliday;
        }

        /// <summary>
        /// retourne true si HOLIDAYYEARLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="pDtExceptWhereNearestHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] Add method
        public bool IsHolidayYearly(DateTime pDate, DayTypeEnum pDayType, DateTime pDtExceptWhereNearestHoliday)
        {
            return IsHolidayYearly(pDate, pDayType, pDtExceptWhereNearestHoliday, out _);
        }
        /// <summary>
        /// retourne true si HOLIDAYYEARLY
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="pDtExceptWhereNearestHoliday"></param>
        /// <param name="pHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] method is public, modification de la signature
        public bool IsHolidayYearly(DateTime pDate, DayTypeEnum pDayType, DateTime pDtExceptWhereNearestHoliday, out EFS_HolidayYearly pHoliday)
        {
            EFS_HolidayYearly hy_ = null;
            bool isHoliday = false;
            bool isOk = false;
            bool isExistDtExcept = DtFunc.IsDateTimeFilled(pDtExceptWhereNearestHoliday);

            pHoliday = null;


            foreach (EFS_HolidayYearly hy in aHolidayYearly)
            {
                hy_ = hy;
                //Le jour fériée correspond exactement à la date testée
                if ((hy.DD == pDate.Day) && (hy.MM == pDate.Month) && ((!isExistDtExcept) || ((hy.DD != pDtExceptWhereNearestHoliday.Day) || (hy.MM != pDtExceptWhereNearestHoliday.Month))))
                {
                    //WARNING: PL 20120109
                    //         Dans le cas très particulier où isExistDtExcept=true, on ne considère pas le jour lui même afin de ne pas tomber dans une boucle infinie.
                    isOk = true;
                    break;
                }
                //Il s'agit d'un jour férié glissant
                //else if (hy.IsHolidayWeeklySlide)
                else if (hy.MethodOfAdjustment != Cst.HolidayMethodOfAdjustment.NONE)
                {
                    if ((hy.MethodOfAdjustment == Cst.HolidayMethodOfAdjustment.ADJ_TO_NEXTDAY_IF_HOLIDAY) && isExistDtExcept)
                    {
                        //WARNING: PL 20120109
                        //         Dans le cas très particulier où isExistDtExcept=true, on n'effectue aucun contrôle afin de ne pas tomber dans une boucle infinie.
                        //         Cela interdit donc d'utiliser ADJ_TO_NEARESTDAY_IF_HOLIDAY pour 2 jours contigus.
                        isOk = false;
                    }
                    else
                    {
                        int yearToCheck = pDate.Year;
                        if ((hy.MM == 12) && (pDate.Month == 1))
                            yearToCheck = pDate.Year - 1; //Contrôle vis à vis de l'année précédente

                        //Contrôle si ce jour férié glisse et si oui, si le nouveau jour férié correspond à la date testée 
                        if (WeeklySlide(hy, yearToCheck, pDayType) &&
                            (hy.SlidedDD == pDate.Day) && (hy.SlidedMM == pDate.Month) && (hy.SlidedYYYY == pDate.Year))
                        {
                            isOk = true;
                            break;
                        }
                    }
                }
            }

            if (isOk)
            {
                isHoliday = ((EFS_HolidayBase)hy_).IsDayType(pDayType);
                if (isHoliday)
                    pHoliday = hy_;
            }

            return isHoliday;
        }

        /// <summary>
        /// retourne true si HOLIDAYMSIC
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="opIsHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] Add method,
        public bool HolidayMisc(DateTime pDate, DayTypeEnum pDayType)
        {
            return HolidayMisc(pDate, pDayType, out _);
        }
        /// <summary>
        /// retourne true si HOLIDAYMSIC
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="opIsHoliday"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] public method,
        /// FI 20130502 [18636] modification de la signature
        /// PL 20131104 [19127] bug
        public bool HolidayMisc(DateTime pDate, DayTypeEnum pDayType, out EFS_HolidayMisc pHoliday)
        {
            bool isFind = false;
            pHoliday = null;

            foreach (EFS_HolidayMisc hm in aHolidayMisc)
            {
                //Before 20131104
                //if (pDate == hm.DTMisc)
                //{
                //    isFind = true;
                //    if (((EFS_HolidayBase)hm).IsDayType(pDayType))
                //        pHoliday = hm;
                //    break;
                //}

                //After 20131104
                if ((pDate == hm.DTMisc) && ((EFS_HolidayBase)hm).IsDayType(pDayType))
                {
                    isFind = true;
                    pHoliday = hm;
                    break;
                }
            }

            return isFind;
        }

        /// <summary>
        /// retourne true si HOLIDAYCALCULATED
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <returns></returns>
        /// FI 20130502 [18636] add method
        public bool IsHolidayCalculated(DateTime pDate, DayTypeEnum pDayType)
        {
            return IsHolidayCalculated(pDate, pDayType, out _);
        }
        /// <summary>
        /// retourne true si HOLIDAYCALCULATED
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="holiday"></param>
        /// <returns></returns>
        /// RD 20110712 [17514]
        /// Utilisation des deux méthodes communes :
        /// - GetCalculatedHolidayDat()
        /// - GetHolidayDateOrSlidedIfSunday()
        /// FI 20130502 [18636] public method, modification de la signature
        public bool IsHolidayCalculated(DateTime pDate, DayTypeEnum pDayType, out EFS_HolidayCalculated holiday)
        {
            bool isHoliday = false;
            holiday = null;

            foreach (EFS_HolidayCalculated hc in aHolidayCalculated)
            {
                if (((EFS_HolidayBase)hc).IsDayType(pDayType))
                {
                    Cst.HolidayName myEnum = (Cst.HolidayName)System.Enum.Parse(typeof(Cst.HolidayName), hc.HolidayName, true);
                    DateTime holidayDate = GetCalculatedHolidayDate(myEnum, pDate.Year);
                    isHoliday = (pDate.Day == holidayDate.Day && pDate.Month == holidayDate.Month);
                    if (isHoliday)
                    {
                        holiday = hc;
                        break;
                    }
                }
            }

            return isHoliday;
        }



        #region IsHoliday
        public bool IsHoliday(DateTime pDate)
        {
            return IsHoliday(pDate, DayTypeEnum.Business);
        }
        public bool IsHoliday(DateTime pDate, DayTypeEnum pDayType)
        {
            return IsHoliday(pDate, pDayType, DateTime.MinValue);
        }
        private bool IsHoliday(DateTime pDate, DayTypeEnum pDayType, DateTime pDtExceptWhereNearestHoliday)
        {
            bool isHoliday;

            //HolidayMisc contient des jours fériés ou non fériés exceptionnels
            if (HolidayMisc(pDate, pDayType, out EFS_HolidayMisc holidayMisc))
            {
                isHoliday = holidayMisc.IsHoliday;
            }
            else
            {
                isHoliday = IsHolidayWeekly(pDate, pDayType);
                if (!isHoliday)
                    isHoliday = IsHolidayMonthly(pDate, pDayType);
                if (!isHoliday)
                    isHoliday = IsHolidayYearly(pDate, pDayType, pDtExceptWhereNearestHoliday);
                if (!isHoliday)
                    isHoliday = IsHolidayCalculated(pDate, pDayType);
            }

            return isHoliday;
        }
        #endregion IsHoliday

        #region WeeklySlide
        /// <summary>
        /// Retourne true si le jour est un jour glissant qui glisse. 
        /// <para>Et valorise avec le nouveau jour férié "glissé", les accesseurs: "Slided..."</para>
        /// </summary>
        /// <param name="phy"></param>
        /// <param name="pCurrentDateYear"></param>
        /// <param name="pDayType"></param>
        /// <returns></returns>
        private bool WeeklySlide(EFS_HolidayYearly pHy, int pCurrentDateYear, DayTypeEnum pDayType)
        {
            bool ret = false;
            int guard = 0;

            pHy.SlidedDD = 0;
            pHy.SlidedMM = 0;
            pHy.SlidedYYYY = 0;

            DateTime dtHoliday = new DateTime(pCurrentDateYear, pHy.MM, pHy.DD);

            //if (pHy.IsHolidayWeeklySlide)
            switch (pHy.MethodOfAdjustment)
            {
                case Cst.HolidayMethodOfAdjustment.ADJ_TO_MONDAY_IF_SUNDAY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base du Dimanche
                    //----------------------------------------------------------------
                    //Si le jour en question est un Dimanche
                    //--> On calcule donc un nouveau jour férié par ajout de 1 jour calendaire (pour obtenir donc un Lundi).
                    //NB: Il s'agit ici de la méthode communément utilisée pour les décalages JPTO
                    if (dtHoliday.DayOfWeek == DayOfWeek.Sunday)
                    {
                        ret = true;
                        dtHoliday = dtHoliday.AddDays(1);
                    }
                    break;
                case Cst.HolidayMethodOfAdjustment.ADJ_TO_FRIDAY_IF_SATURDAY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base du Samedi
                    //----------------------------------------------------------------
                    //Si le jour en question est un Samedi
                    //--> On calcule donc un nouveau jour férié par retranchement de 1 jour calendaire (pour obtenir donc un Vendredi).
                    //NB: Méthode inutilisée à ce jour
                    if (dtHoliday.DayOfWeek == DayOfWeek.Saturday)
                    {
                        ret = true;
                        dtHoliday = dtHoliday.AddDays(-1);
                    }
                    break;

                case Cst.HolidayMethodOfAdjustment.ADD_TWO_DAY_IF_HOLIDAYWEEKLY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base des jours fériés hebdomadaires
                    //----------------------------------------------------------------
                    //Si le jour en question est "également" férié hebdomadaire
                    //--> On calcule donc un nouveau jour férié par ajout de 2 jours calendaires.
                    //NB: Il s'agit ici de la méthode utilisée pour les décalages Christmas/Boxing day GBLO
                    if (IsHolidayWeekly(dtHoliday, pDayType))
                    {
                        ret = true;
                        dtHoliday = dtHoliday.AddDays(2);
                    }
                    break;

                case Cst.HolidayMethodOfAdjustment.ADJ_TO_NEXTDAY_IF_HOLIDAY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base des jours fériés
                    //----------------------------------------------------------------
                    //Si le jour en question est "également" férié 
                    //--> On calcule donc un nouveau jour férié "non férié".
                    DateTime dtCurrent = dtHoliday;
                    while ((++guard < 99) && IsHoliday(dtHoliday, pDayType, dtCurrent))
                    {
                        ret = true;
                        dtHoliday = dtHoliday.AddDays(1);
                    }
                    break;
                case Cst.HolidayMethodOfAdjustment.ADJ_TO_NEXTDAY_IF_HOLIDAYWEEKLY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base des jours fériés hebdomadaires
                    //----------------------------------------------------------------
                    //Si le jour en question est "également" férié hebdomadaire 
                    //--> On calcule donc un nouveau jour férié "non férié hebdomadaire".
                    while ((++guard < 99) && IsHolidayWeekly(dtHoliday, pDayType))
                    {
                        ret = true;
                        dtHoliday = dtHoliday.AddDays(1);
                    }
                    break;

                case Cst.HolidayMethodOfAdjustment.ADJ_TO_NEARESTDAY_IF_HOLIDAYWEEKLY:
                    //----------------------------------------------------------------
                    //Jour férié "glissant" sur la base des jours fériés hebdomadaires
                    //----------------------------------------------------------------
                    //Si le jour en question est "également" férié hebdomadaire 
                    //--> On calcule donc un nouveau jour férié "non férié hebdomadaire".
                    if (IsHolidayWeekly(dtHoliday, pDayType))
                    {
                        ret = true;
                        DateTime dtPrev = dtHoliday;
                        DateTime dtNext = dtHoliday;
                        while (++guard < 99)
                        {
                            dtPrev = dtPrev.AddDays(-1);
                            dtNext = dtNext.AddDays(1);
                            if (!IsHolidayWeekly(dtPrev, pDayType))
                            {
                                dtHoliday = dtPrev;
                                break;
                            }
                            else if (!IsHolidayWeekly(dtNext, pDayType))
                            {
                                dtHoliday = dtNext;
                                break;
                            }
                        }
                    }
                    break;
            }

            if (ret)
            {
                pHy.SlidedDD = dtHoliday.Day;
                pHy.SlidedMM = dtHoliday.Month;
                pHy.SlidedYYYY = dtHoliday.Year;
            }
            return ret;
        }
        #endregion

        #region GetDayByPositionDay
        /// <summary>
        /// Retourne la position du jour dans le mois 
        /// </summary>
        /// <param name="pHolidayMonthly"></param>
        /// <param name="pCurrentDateYear"></param>
        /// <returns></returns>
        private static int GetDayByPositionDay(EFS_HolidayMonthly pHolidayMonthly, int pCurrentDateYear)
        {
            return GetDayByPositionDay(pHolidayMonthly.MM, pHolidayMonthly.PositionDay, pHolidayMonthly.DDD, pHolidayMonthly.MarkDay, pCurrentDateYear);
        }
        /// <summary>
        /// Retourne la position du jour dans le mois 
        /// </summary>
        /// <param name="pMM"></param>
        /// <param name="pPositionDay"></param>
        /// <param name="pDDD"></param>
        /// <param name="pMarkDay"></param>
        /// <param name="pCurrentDateYear"></param>
        /// <returns></returns>
        private static int GetDayByPositionDay(int pMM, string pPositionDay, string pDDD, int? pMarkDay, int pCurrentDateYear)
        {
            int correspondingDay = -1;
            int markDay;
            Cst.PositionDay positionDay;

            if (System.Enum.IsDefined(typeof(Cst.PositionDay), pPositionDay))
            {
                positionDay = (Cst.PositionDay)System.Enum.Parse(typeof(Cst.PositionDay), pPositionDay, true);
                //---------------------------------------------------------------
                //Création d'une date au premier jour du mois ou au jour spécifié
                //---------------------------------------------------------------
                markDay = 1;
                if (pMarkDay != null)
                    markDay = Convert.ToInt32(pMarkDay);
                //---------------------------------------------------------------
                int position = 0;
                int sens = 1;

                switch (positionDay)
                {
                    case Cst.PositionDay.FIRST:
                        position = 1;
                        break;
                    case Cst.PositionDay.SECOND:
                        position = 2;
                        break;
                    case Cst.PositionDay.THIRD:
                        position = 3;
                        break;
                    case Cst.PositionDay.FOURTH:
                        position = 4;
                        break;
                    case Cst.PositionDay.LAST:
                        position = 1;
                        sens = -1;
                        //-----------------------------------------------------------------------
                        //Création d'une date au dernier jour du mois ou au jour le jour spécifié
                        //-----------------------------------------------------------------------
                        markDay = DateTime.DaysInMonth(pCurrentDateYear, pMM);
                        if (pMarkDay != null)
                            markDay = Convert.ToInt32(pMarkDay);
                        //-----------------------------------------------------------------------
                        break;
                }

                DateTime currentDate = new DateTime(pCurrentDateYear, pMM, markDay);
                string dayOfWeek = currentDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                int currentPosition = 0;
                while (true)
                {
                    if (dayOfWeek == pDDD)
                    {
                        currentPosition++;
                        if (currentPosition == position)
                        {
                            // N°15457
                            correspondingDay = currentDate.Day;
                            break;
                        }
                    }
                    currentDate = currentDate.AddDays(sens);
                    dayOfWeek = currentDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                    if (currentDate.Month != pMM)
                    {
                        correspondingDay = -1;//an error occurs
                        break;
                    }
                }
            }
            return correspondingDay;
        }
        #endregion

        #region GetMidSummerDay
        private static DateTime GetMidSummerDay(int pCurrentDateYear)
        {
            /*
             In modern Sweden, Midsummer's Eve and Midsummer's Day (Midsommarafton and Midsommardagen) were formerly celebrated on 23 June and 24 June, 
             but since 1953 the celebration has been moved to the Friday and Saturday between 19 June and 26 June with the main celebrations taking place on Friday. 
             It is one of the most important holidays of the year in Sweden, and probably the most uniquely Swedish in the way it is celebrated.
            */
                DateTime dtMidSummerDay = new DateTime(pCurrentDateYear, 6, 20);

            // EG 20130704 dtMidSummerDay = dtMidSummerDay.AddDays(1);
            while (dtMidSummerDay.DayOfWeek != DayOfWeek.Saturday)
            {
                dtMidSummerDay = dtMidSummerDay.AddDays(1);
            }
            return dtMidSummerDay;
        }
        #endregion

        #region GetEasterDay
        private static DateTime GetEasterDay(int pCurrentDateYear)
        {
            /*  http://www.recreomath.qc.ca/dict_paques_d.htm
                T. H. O’Beirne, en s’inspirant des travaux de Gauss, a donné cette formule qui s'applique aussi aux années 1900 à 2099.
                Soit m l’année, on fait les calculs suivants :
                1. On soustrait 1900 de m : c’est la valeur de n.
                2. On divise n par 19 : le reste est la valeur de a.
                3. On divise (7a + 1) par 19 : la partie entière du quotient est b.
                4. On divise (11a - b + 4) par 29 : le reste est c.
                5. On divise n par 4 : la partie entière du quotient est d.
                6. On divise (n - c + d + 31) par 7 : le reste est e.
                La date de Pâques est le (25 - c - e) avril si le résultat est positif. S’il est négatif, le mois est mars. Le quantième est la somme de 31 et du résultat. Par exemple, si le résultat est -7, le quantième est 31 + -7 = 24.
            */
            DateTime dtEasterDay;

            int n = pCurrentDateYear - 1900;
            int a = n % 19;
            int b = (int)((7 * a + 1) / 19);
            int c = (11 * a - b + 4) % 29;
            int d = (int)(n / 4);
            int e = (n - c + d + 31) % 7;

            int dd;
            int mm;
            dd = (25 - c - e);
            if (dd > 0)
                mm = 4; //Avril
            else
            {
                dd += 31;
                mm = 3; //Mars
            }

            dtEasterDay = new DateTime(pCurrentDateYear, mm, dd);

            return dtEasterDay;
        }
        #endregion GetEasterDay
        
        #region GetCalculatedHolidayDate
        /// <summary>
        /// Retourne la date du jour férié spécifié {pHolidayName}, pour l'année spécifiée {pCurrentDateYear}
        /// </summary>
        /// <param name="pHolidayName"></param>
        /// <param name="pYear"></param>
        /// <returns></returns>
        private DateTime GetCalculatedHolidayDate(Cst.HolidayName pHolidayName, int pYear)
        {
            DateTime holidayDate = DateTime.MinValue;

            switch (pHolidayName)
            {
                case Cst.HolidayName.EASTER:
                    holidayDate = GetEasterDay(pYear);
                    break;
                case Cst.HolidayName.EASTERMONDAY:
                    holidayDate = GetEasterDay(pYear).AddDays(1);
                    break;
                case Cst.HolidayName.ASCENSION:
                    //Ascension = Pâques + 39 jours
                    holidayDate = GetEasterDay(pYear).AddDays(39);
                    break;
                case Cst.HolidayName.PENTECOST:
                    //Pentecôte = Pâques + 49 jours
                    holidayDate = GetEasterDay(pYear).AddDays(49);
                    break;
                case Cst.HolidayName.PENTECOSTMONDAY:
                    holidayDate = GetEasterDay(pYear).AddDays(50);
                    break;
                case Cst.HolidayName.CORPUSCHRISTI:
                    holidayDate = GetEasterDay(pYear).AddDays(60);
                    break;
                case Cst.HolidayName.GOODFRIDAY:
                    //Good Friday = Vendredi précédant Pâques (Vendredi Saint)
                    holidayDate = GetEasterDay(pYear).AddDays(-2);
                    break;
                case Cst.HolidayName.MIDSUMMEREVE:
                    //MidSummer's Eve = Vendredi du couple Vendredi/Samedi situé entre le 19 et 26 juin (Midsommarafton)
                    holidayDate = GetMidSummerDay(pYear).AddDays(-1);
                    break;
                case Cst.HolidayName.MIDSUMMERDAY:
                    //MidSummer's Day = Samedi du couple Vendredi/Samedi situé entre le 19 et 26 juin (Midsommardagen)
                    holidayDate = GetMidSummerDay(pYear);
                    break;
                //case Cst.HolidayName.KODOMO:
                //    // 5 MAI  reporté au 6 si dimanche (jour des enfants)
                //    holidayDate = GetDateSlidedIfSunday(pHolidayName, pYear, 5, 5);
                //    break;
                //case Cst.HolidayName.SHUBUN:
                //    // 23 SEPTEMBRE  reporté au 24 si dimanche (équinoxe d’automne)
                //    holidayDate = GetDateSlidedIfSunday(pHolidayName, pYear, 9, 23);
                //    break;
                //case Cst.HolidayName.BUNKA:
                //    // 3 NOVEMBRE    reporté au 4 si dimanche  (journée nationale de la culture)
                //    holidayDate = GetDateSlidedIfSunday(pHolidayName, pYear, 11, 3);
                //    break;
                //case Cst.HolidayName.KEIRO:
                //    // 15 SEPTEMBRE  reporté au 16 si dimanche et reporté au lundi suivant si mardi, mercredi ou jeudi (journée des personnes âgées)
                //    holidayDate = GetKEIRODay(pHolidayName, pYear);
                //    break;
            }
            return holidayDate;
        }
        #endregion

        // RD 20110921 / Gestion d'une nième valeur(2), représentant l’année concernée
        /// <summary>
        /// Retourne la prochaine date correspondant à un jour férié. 
        /// <para>WARNING: Réservé à un usage spécifique depuis l'automate de paramétrage !</para>
        /// </summary>
        /// <param name="pData">String "spécifique" constituée de plusieurs données concaténées dans un ordre bien particulier.</param>
        /// <returns></returns>
        /// FI 20190509 [24661] Add parameter pDateRef
        public DateTime GetDate(string pData, Nullable<DateTime> pDateRef)
        {
            int guard = 0;


            // FI 20190509 [24661] Gestion d'une date de reference
            DateTime currentDate = pDateRef ?? OTCmlHelper.GetDateBusiness(m_Cs);

            string[] arrayElement = pData.Split(';');
            Cst.OTCml_TBL tableName = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), arrayElement[0]);
            string idBC = arrayElement[1];
            //PL 20131121
            bool isBusiness = BoolFunc.IsTrue(arrayElement[2]);
            bool isCurrencyBusiness = BoolFunc.IsTrue(arrayElement[3]);
            bool isCommodityBusiness = BoolFunc.IsTrue(arrayElement[4]);
            bool isExchangeBusiness = BoolFunc.IsTrue(arrayElement[5]);
            bool isScheduledTradingDay = BoolFunc.IsTrue(arrayElement[6]);
            int nextElement = 7;
            //
            this.listOfBusinessCenters = DataHelper.SQLString(idBC);
            this.businessCentersSpecified = true;
            this.LoadHoliday2();

            //PL 20131121
            DayTypeEnum dayType;
            if (isScheduledTradingDay)
                dayType = DayTypeEnum.ScheduledTradingDay;
            else if (isExchangeBusiness)
                dayType = DayTypeEnum.ExchangeBusiness;
            else if (isCurrencyBusiness)
                dayType = DayTypeEnum.CurrencyBusiness;
            else if (isCommodityBusiness)
                dayType = DayTypeEnum.CommodityBusiness;
            else
                dayType = DayTypeEnum.Business;

            int? MarkDay = null;

            string DD;
            string DDD;
            string MM;
            string YYYY;

            bool isWithYear;

            DateTime returnDate = DateTime.MinValue;
            switch (tableName)
            {
                case Cst.OTCml_TBL.HOLIDAYWEEKLY:
                    #region HOLIDAYWEEKLY
                    DDD = arrayElement[nextElement];
                    //
                    returnDate = currentDate;
                    guard = 0;
                    while ((DDD != returnDate.DayOfWeek.ToString().Substring(0, 3).ToUpper()) && (guard < 999))
                    {
                        guard++;
                        returnDate = returnDate.AddDays(1);
                    }
                    //
                    if (guard < 999)
                    {
                        guard = 0;

                        while (HolidayMisc(returnDate, dayType, out EFS_HolidayMisc holidayMisc) && (guard < 999))
                        {
                            bool isHolidayMisc = holidayMisc.IsHoliday;
                            if (!isHolidayMisc)
                            {
                                //Il s'agit d'un jour exceptionnelement "non férié"
                                returnDate = returnDate.AddDays(7);
                            }
                            else
                            {
                                break;
                            }
                            //
                            guard++;
                        }
                    }
                    #endregion
                    break;

                case Cst.OTCml_TBL.HOLIDAYMONTHLY:
                    #region HOLIDAYMONTHLY
                    string posDay = arrayElement[nextElement];
                    DDD = arrayElement[nextElement + 1];
                    MM = arrayElement[nextElement + 2];
                    if (StrFunc.IsFilled(arrayElement[nextElement + 3]))
                        MarkDay = Convert.ToInt32(arrayElement[nextElement + 3]);

                    isWithYear = (ArrFunc.Count(arrayElement) > (nextElement + 4));
                    YYYY = (isWithYear ? arrayElement[nextElement + 4] : currentDate.Year.ToString());

                    DD = GetDayByPositionDay(Convert.ToInt32(MM), posDay, DDD, MarkDay, Convert.ToInt32(YYYY)).ToString();

                    if (DD.Length == 1)
                        DD = "0" + DD;
                    if (MM.Length == 1)
                        MM = "0" + MM;

                    returnDate = DtFunc.ParseDate(YYYY + MM + DD, DtFunc.FmtDateyyyyMMdd, null);

                    // Si l'annéee est passée en argument, on retourne le jour férié de l'année passée en argument
                    // Sinon, si le jour férié trouvé est dans le passé par rapport à la date courante, on cherche le jour férié de l'année prochaine
                    if ((isWithYear == false) && (returnDate.CompareTo(currentDate) < 0))
                    {
                        DD = GetDayByPositionDay(Convert.ToInt32(MM), posDay, DDD, MarkDay, returnDate.Year + 1).ToString();

                        if (DD.Length == 1)
                            DD = "0" + DD;

                        returnDate = DtFunc.ParseDate((returnDate.Year + 1).ToString() + MM + DD, DtFunc.FmtDateyyyyMMdd, null);
                    }
                    //

                    guard = 0;
                    EFS_HolidayMisc holidayMisc2;
                    while (HolidayMisc(returnDate, dayType, out holidayMisc2) && (guard < 999))
                    {
                        if (!holidayMisc2.IsHoliday)
                        {
                            if (isWithYear)
                            {
                                // Si l'annéee est passée en argument et Il s'agit d'un jour exceptionnelement "non férié", on retourne null
                                returnDate = DateTime.MinValue;
                                break;
                            }
                            else
                            {
                                //Il s'agit d'un jour exceptionnelement "non férié", on cherche le jour férié de l'année prochaine
                                DD = GetDayByPositionDay(Convert.ToInt32(MM), posDay, DDD, MarkDay, returnDate.Year + 1).ToString();

                                if (DD.Length == 1)
                                    DD = "0" + DD;

                                returnDate = DtFunc.ParseDate((returnDate.Year + 1).ToString() + MM + DD, DtFunc.FmtDateyyyyMMdd, null);
                            }
                        }
                        else
                        {
                            break;
                        }

                        guard++;
                    }
                    #endregion
                    break;

                case Cst.OTCml_TBL.HOLIDAYYEARLY:
                    #region HOLIDAYYEARLY
                    DD = arrayElement[nextElement];
                    MM = arrayElement[nextElement + 1];
                    if (DD.Length == 1)
                        DD = "0" + DD;
                    if (MM.Length == 1)
                        MM = "0" + MM;

                    isWithYear = (ArrFunc.Count(arrayElement) > (nextElement + 3));
                    YYYY = (isWithYear ? arrayElement[nextElement + 3] : currentDate.Year.ToString());

                    // PL 20240322 [WI511] Refactoring - Add Main While(isContinue) and addYear
                    guard = 0;
                    int addYear = 0;
                    bool isContinue = true;
                    while (isContinue && (guard < 999))
                    {
                        isContinue = false;
                        if (guard > 0)
                        {
                            // PL 20240322 [WI511] 
                            // We are here on an Nth attempt, therefore on a new calculation for the following year.
                            addYear++;
                        }
                        returnDate = DtFunc.ParseDate(YYYY + MM + DD, DtFunc.FmtDateyyyyMMdd, null).AddYears(addYear);

                        // AL 20240321 [WI511] 
                        // If the date is associated with an adjustment method, I calculate the adjusted date.
                        if (arrayElement[nextElement + 2] != Cst.HolidayMethodOfAdjustment.NONE.ToString())
                        {
                            EFS_HolidayYearly hy = new EFS_HolidayYearly(returnDate.Day, returnDate.Month, arrayElement[nextElement + 2],
                                isBusiness, isCommodityBusiness, isCurrencyBusiness, isExchangeBusiness, isScheduledTradingDay);

                            if (WeeklySlide(hy, returnDate.Year, dayType))
                            {
                                returnDate = new DateTime(hy.SlidedYYYY, hy.SlidedMM, hy.SlidedDD);
                            }
                        }

                        // Si l'année est passée en argument, on retourne le jour férié de l'année passée en argument. Peu importe qu'il soit déjà passé.
                        // Sinon, si le jour férié trouvé est un jour passé, par rapport à la date courante, on cherche alors le jour férié sur l'année à venir.
                        if ((isWithYear == false) && (returnDate.CompareTo(currentDate) < 0))
                        {
                            isContinue = true;
                        }
                        else
                        {
                            //Last step : we check if this public holiday is not exceptionally non-public holiday.
                            while (HolidayMisc(returnDate, dayType, out EFS_HolidayMisc holidayMisc) && (guard < 999))
                            {
                                if (!holidayMisc.IsHoliday)
                                {
                                    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    // Il s'agit d'un jour, exceptionnellement, "non férié"
                                    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    if (isWithYear)
                                    {
                                        // Si l'annéee est passée en argument, on retourne "null", pour indiquer qu'exceptionnellement, ce jour n'est pas un jour férié.
                                        returnDate = DateTime.MinValue;
                                        break;
                                    }
                                    else
                                    {
                                        // Sinon, on cherche alors le jour férié sur l'année à venir
                                        isContinue = true;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                guard++;
                            }
                        }

                        guard++;
                    }
                    #endregion
                    break;

                case Cst.OTCml_TBL.HOLIDAYCALCULATED:
                    #region HOLIDAYCALCULATED
                    DDD = arrayElement[nextElement];

                    isWithYear = (ArrFunc.Count(arrayElement) > (nextElement + 1));
                    YYYY = (isWithYear ? arrayElement[nextElement + 1] : currentDate.Year.ToString());

                    Cst.HolidayName holidayName = (Cst.HolidayName)System.Enum.Parse(typeof(Cst.HolidayName), DDD, true);
                    returnDate = GetCalculatedHolidayDate(holidayName, Convert.ToInt32(YYYY));

                    // Si l'annéee est passée en argument, on retourne le jour férié de l'année passée en argument
                    // Sinon, si le jour férié trouvé est dans le passé par rapport à la date courante, on cherche le jour férié de l'année prochaine
                    if ((isWithYear == false) && (returnDate.CompareTo(currentDate) < 0))
                        returnDate = GetCalculatedHolidayDate(holidayName, returnDate.Year + 1);
                    guard = 0;

                    EFS_HolidayMisc holidayMisc4;
                    while (HolidayMisc(returnDate, dayType, out holidayMisc4) && (guard < 999))
                    {
                        if (!holidayMisc4.IsHoliday)
                        {
                            if (isWithYear)
                            {
                                // Si l'annéee est passée en argument et s'il s'agit d'un jour exceptionnelement "non férié", on retourne null
                                returnDate = DateTime.MinValue;
                                break;
                            }
                            else
                            {
                                // S'il s'agit d'un jour exceptionnelement "non férié", on cherche alors le jour férié de l'année suivante
                                returnDate = GetCalculatedHolidayDate(holidayName, returnDate.Year + 1);
                            }
                        }
                        else
                        {
                            break;
                        }
                        guard++;
                    }
                    #endregion
                    break;

                case Cst.OTCml_TBL.HOLIDAYMISC:
                    #region HOLIDAYMISC
                    returnDate = DtFunc.ParseDate(arrayElement[nextElement], DtFunc.FmtDateyyyyMMdd, null);
                    if (returnDate.CompareTo(currentDate) < 0)
                        returnDate = DateTime.MinValue;
                    #endregion
                    break;

                default:
                    returnDate = DateTime.MinValue;
                    break;
            }
            if (guard == 999)
                returnDate = DateTime.MinValue;

            return returnDate;
        }

        /// <summary>
        ///  Retourne le type de jour férié 
        ///  <para>Retourn null si le jour n'est pas férié</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDayType"></param>
        /// <param name="pHoliday"></param>
        /// <returns></returns>
        public Nullable<Cst.HolidayType> GetHolidayType(DateTime pDate, DayTypeEnum pDayType, out EFS_HolidayBase pHoliday)
        {
            Nullable<Cst.HolidayType> holidayType = null;
            pHoliday = null;

            if (HolidayMisc(pDate, DayTypeEnum.ExchangeBusiness, out EFS_HolidayMisc holidayMisc))
            {
                if (holidayMisc.IsHoliday)
                {
                    holidayType = Cst.HolidayType.HOLIDAYMISC;
                    pHoliday = holidayMisc;
                }
            }
            else
            {
                if (IsHolidayWeekly(pDate, pDayType, out EFS_HolidayWeekly holidayWeekly))
                {
                    holidayType = Cst.HolidayType.HOLIDAYWEEKLY;
                    pHoliday = holidayWeekly;
                }
                else if (IsHolidayMonthly(pDate, pDayType, out EFS_HolidayMonthly holidayMonthly))
                {
                    holidayType = Cst.HolidayType.HOLIDAYMONTHLY;
                    pHoliday = holidayMonthly;
                }
                else if (IsHolidayYearly(pDate, pDayType, DateTime.MinValue, out EFS_HolidayYearly holidayYearly))
                {
                    holidayType = Cst.HolidayType.HOLIDAYYEARLY;
                    pHoliday = holidayYearly;
                }
                else if (IsHolidayCalculated(pDate, pDayType, out EFS_HolidayCalculated holidayCalculated))
                {
                    holidayType = Cst.HolidayType.HOLIDAYYEARLY;
                    pHoliday = holidayCalculated;
                }
            }

            return holidayType;
        }


        #endregion Methods

        #region ICloneable Members
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            EFS_BusinessCenters clone = new EFS_BusinessCenters(m_Cs, m_DataDocument)
            {
                businessCentersSpecified = this.businessCentersSpecified,
                m_businessCenters = (IBusinessCenters)this.m_businessCenters.Clone()
            };
            clone.LoadHoliday();
            return clone;
        }
        #endregion ICloneable Members
    }
    #endregion EFS_BusinessCenters

    #region EFS_Cash
    // EG 20091202 Add roundPrec/roundDir (members & accessors)
    public class EFS_Cash
    {
        #region Members
        private string cs;
        private IDbTransaction dbTransaction;
        private decimal amountRounded;
        private decimal exchangeAmount;
        private decimal exchangeAmountRounded;
        private int roundPrec;
        private string roundDir;
        private bool m_IsReverse;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public decimal AmountRounded
        {
            get { return amountRounded; }
            set { amountRounded = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public decimal ExchangeAmount
        {
            get { return exchangeAmount; }
            set { exchangeAmount = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExchangeAmountRounded
        {
            get { return exchangeAmountRounded; }
            set { exchangeAmountRounded = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AmountRoundDir
        {
            get { return roundDir; }
            set { roundDir = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int AmountRoundPrec
        {
            get { return roundPrec; }
            set { roundPrec = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsExchangeRateIsReverse
        {
            get { return m_IsReverse; }
        }


        #endregion Accessors

        #region Constructor
        /// <summary>
        ///  CashRound 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, decimal pAmount, string pCurrency)
            : this(pConnectionString, null as IDbTransaction, pAmount, pCurrency) { }
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, IDbTransaction pDbTransaction, decimal pAmount, string pCurrency)
        {
            Initialize(pConnectionString, pDbTransaction);
            CashRound(pAmount, pCurrency, ref amountRounded);
        }
        //CashRound With CurrencyCashInfo instead Currency
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, decimal pAmount, CurrencyCashInfo pCurrencyCashInfo) :
            this(pConnectionString, null, pAmount, pCurrencyCashInfo){}
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, IDbTransaction pDbTransaction, decimal pAmount, CurrencyCashInfo pCurrencyCashInfo)
        {
            Initialize(pConnectionString, pDbTransaction);
            CashRound(pAmount, pCurrencyCashInfo, ref amountRounded);
        }
        /// <summary>
        /// Calcul de contrevaleur
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        /// <param name="pAmount"></param>
        /// <param name="pExchangeRate"></param>
        /// <param name="pQuotedBasis"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, string pCurrency1, string pCurrency2, decimal pAmount, Nullable<decimal> pExchangeRate, QuoteBasisEnum pQuotedBasis)
            :this(pConnectionString, null as IDbTransaction, pCurrency1, pCurrency2, pAmount, pExchangeRate, pQuotedBasis){}
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, IDbTransaction pDbTransaction, string pCurrency1, string pCurrency2, decimal pAmount, Nullable<decimal> pExchangeRate, QuoteBasisEnum pQuotedBasis)
        {
            Initialize(pConnectionString, pDbTransaction);
            CashConvert(pCurrency1, pCurrency2, pAmount, pExchangeRate, pQuotedBasis);
        }
        /// <summary>
        /// CashConvert With CurrencyCashInfo instead Currency
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pCurrencyCashInfo1"></param>
        /// <param name="pCurrencyCashInfo2"></param>
        /// <param name="pAmount"></param>
        /// <param name="pExchangeRate"></param>
        /// <param name="pQuotedBasis"></param>
        public EFS_Cash(string pConnectionString, CurrencyCashInfo pCurrencyCashInfo1, CurrencyCashInfo pCurrencyCashInfo2,
            decimal pAmount, Nullable<decimal> pExchangeRate, QuoteBasisEnum pQuotedBasis)
        {
            Initialize(pConnectionString);
            CashConvert(pCurrencyCashInfo1, pCurrencyCashInfo2, pAmount, pExchangeRate, pQuotedBasis);
        }
        /// <summary>
        /// CashConvert with IMoney
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAmount"></param>
        /// <param name="pExchangeRate"></param>
        /// <param name="pQuotedCurrencyPair"></param>
        // RD 20170119 [22797] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, IMoney pAmount, Nullable<decimal> pExchangeRate, IQuotedCurrencyPair pQuotedCurrencyPair) :
            this(pConnectionString, null, pAmount, pExchangeRate, pQuotedCurrencyPair){}
        // EG 20180205 [23769] Add dbTransaction  
        public EFS_Cash(string pConnectionString, IDbTransaction pDbTransaction, IMoney pAmount, Nullable<decimal> pExchangeRate, IQuotedCurrencyPair pQuotedCurrencyPair)
        {
            Initialize(pConnectionString, pDbTransaction);
            string currency1 = pQuotedCurrencyPair.Currency1;
            string currency2 = pQuotedCurrencyPair.Currency2;
            QuoteBasisEnum quoteBasis = pQuotedCurrencyPair.QuoteBasis;

            // RD 20170119 [22797] 
            // Le paramètre "pCurrency1" de la méthode "CashConvert" doit correspondre à la devise du montant à convertir "pAmount.currency" 
            // Si ce n'est pas le cas, il faut l'aligner, en réajustant quoteBasis
            //if (((QuoteBasisEnum.Currency1PerCurrency2 == quoteBasis) && (currency2 == pAmount.currency)) ||
            //    ((QuoteBasisEnum.Currency2PerCurrency1 == quoteBasis) && (currency1 == pAmount.currency)))
            //{
            //    currency2 = currency1;
            //    currency1 = pAmount.currency;
            //    quoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
            //}
            if (currency1 != pAmount.Currency)
            {
                currency2 = currency1;
                currency1 = pAmount.Currency;
                if (QuoteBasisEnum.Currency1PerCurrency2 == quoteBasis)
                    quoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else if (QuoteBasisEnum.Currency2PerCurrency1 == quoteBasis)
                    quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
            }
            CashConvert(currency1, currency2, pAmount.Amount.DecValue, pExchangeRate, quoteBasis);
        }
        //
        #endregion Constructor

        #region Methods

        /// <summary>
        /// Calcul of Contrevalue 
        /// </summary>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        /// <param name="pAmount1"></param>
        /// <param name="pExchangeRate"></param>
        /// <param name="pQuotedBasis"></param>
        /// <returns></returns>
        /// EG 20150317 [POC] Test (pExchangeRate.HasValue)
        private Cst.ErrLevel CashConvert(CurrencyCashInfo pCurrencyCashInfo, CurrencyCashInfo pExchangeCurrencyCashInfo,
            decimal pAmount1, Nullable<decimal> pExchangeRate, QuoteBasisEnum pQuotedBasis)
        {
            Cst.ErrLevel retValue = Cst.ErrLevel.FAILURE;
            //
            try
            {
                if ((null != pCurrencyCashInfo) && (null != pExchangeCurrencyCashInfo))
                {
                    if (pExchangeRate.HasValue)
                    {
                        // Calculate Amount
                        if (QuoteBasisEnum.Currency1PerCurrency2 == pQuotedBasis)
                        {
                            m_IsReverse = true;
                            ExchangeAmount = pAmount1 * ((decimal)1 / pExchangeRate.Value) * ((decimal)pExchangeCurrencyCashInfo.Factor / (decimal)pCurrencyCashInfo.Factor);
                        }
                        else if (QuoteBasisEnum.Currency2PerCurrency1 == pQuotedBasis)
                        {
                            ExchangeAmount = pAmount1 * pExchangeRate.Value * ((decimal)pExchangeCurrencyCashInfo.Factor / (decimal)pCurrencyCashInfo.Factor);
                        }
                    }
                    retValue = CashRound(ExchangeAmount, pExchangeCurrencyCashInfo.RoundPrec, pExchangeCurrencyCashInfo.RoundDir, ref exchangeAmountRounded);
                }
            }
            catch
            {
                ExchangeAmount = Decimal.Zero;
                ExchangeAmountRounded = Decimal.Zero;
            }
            //
            return retValue;
        }
        /// <summary>
        /// Calcul of Contrevalue 
        /// </summary>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        /// <param name="pAmount1"></param>
        /// <param name="pExchangeRate"></param>
        /// <param name="pQuotedBasis"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private Cst.ErrLevel CashConvert(string pCurrency1, string pCurrency2, decimal pAmount1, Nullable<decimal> pExchangeRate, QuoteBasisEnum pQuotedBasis)
        {

            // Get Currency Info from DB
            CurrencyCashInfo currencyCashInfo = new CurrencyCashInfo(cs, dbTransaction, pCurrency1);
            CurrencyCashInfo exchangeCurrencyCashInfo = new CurrencyCashInfo(cs, dbTransaction, pCurrency2);

            Cst.ErrLevel retValue = CashConvert(currencyCashInfo, exchangeCurrencyCashInfo, pAmount1, pExchangeRate, pQuotedBasis);

            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pCurrencyCashInfo"></param>
        /// <param name="opAmount"></param>
        /// <returns></returns>
        private Cst.ErrLevel CashRound(decimal pAmount, CurrencyCashInfo pCurrencyCashInfo, ref decimal opAmount)
        {
            Cst.ErrLevel retValue = Cst.ErrLevel.FAILURE;
            //
            try
            {
                if (null != pCurrencyCashInfo)
                    retValue = CashRound(pAmount, pCurrencyCashInfo.RoundPrec, pCurrencyCashInfo.RoundDir, ref opAmount);
            }
            catch { opAmount = Decimal.Zero; }
            //
            return retValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="opAmount"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        private Cst.ErrLevel CashRound(decimal pAmount, string pCurrency, ref decimal opAmount)
        {
            Cst.ErrLevel retValue = Cst.ErrLevel.FAILURE;
            //
            try
            {
                CurrencyCashInfo currencyCashInfo = new CurrencyCashInfo(cs, dbTransaction, pCurrency);
                if (null != currencyCashInfo)
                    retValue = CashRound(pAmount, currencyCashInfo.RoundPrec, currencyCashInfo.RoundDir, ref opAmount);
            }
            catch { opAmount = Decimal.Zero; }
            //
            return retValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pRoundPrec"></param>
        /// <param name="pRoundDir"></param>
        /// <param name="opAmount"></param>
        /// <returns></returns>
        private Cst.ErrLevel CashRound(decimal pAmount, int pRoundPrec, string pRoundDir, ref decimal opAmount)
        {
            Cst.ErrLevel retValue = Cst.ErrLevel.FAILURE;
            //
            try
            {
                EFS_Round round = new EFS_Round(pRoundDir, pRoundPrec, pAmount);
                roundDir = pRoundDir;
                roundPrec = pRoundPrec;
                opAmount = round.AmountRounded;
                retValue = Cst.ErrLevel.SUCCESS;
            }
            catch { opAmount = Decimal.Zero; }
            //
            return retValue;
        }

        #region Initialize
        private void Initialize(string pConnectionString)
        {
            cs = pConnectionString;
            amountRounded = Decimal.Zero;
            exchangeAmount = Decimal.Zero;
            exchangeAmountRounded = Decimal.Zero;
            m_IsReverse = false;

        }
        // EG 20180205 [23769] Add dbTransaction  
        private void Initialize(string pConnectionString, IDbTransaction pDbTransaction)
        {
            Initialize(pConnectionString);
            dbTransaction = pDbTransaction; 
        }
        #endregion Initialize
        #endregion Methods
    }
    #endregion EFS_Cash

    #region EFS_CashPosition
    /// <summary>
    /// 
    /// </summary>
    public class EFS_CashPosition
    {
        #region members
        private readonly CashPosition _cashPosition;
        private readonly DataDocumentContainer _dataDoc;
        #endregion

        public EFS_CashPosition(CashPosition pCashPosition, DataDocumentContainer pDataDoc)
        {
            _cashPosition = pCashPosition;
            _dataDoc = pDataDoc;
        }

        /// <summary>
        /// Retourne la date
        /// <para>La date ajustée est identique à la date non ajustée (la date est nécessairement ajustée sur un CashPosition )</para>
        /// </summary>
        /// <returns></returns>
        public EFS_EventDate GetEventDate()
        {
            EFS_EventDate ret = null;

            if (_cashPosition.dateDefineSpecified)
            {
                ret = new EFS_EventDate(_cashPosition.dateDefine.DateValue, _cashPosition.dateDefine.DateValue);
            }
            else if (_cashPosition.dateReferenceSpecified)
            {
                IAdjustedDate adjustedDate = (IAdjustedDate)_dataDoc.GetObjectById(_cashPosition.dateReference.href);
                ret = new EFS_EventDate(adjustedDate.DateValue, adjustedDate.DateValue);
            }
            return ret;
        }
    }
    #endregion EFS_CashPosition

    #region EFS_Commission
    public class EFS_Commission
    {
        #region Members
        public CommissionDenominationEnum denomination;
        public IMoney fee;
        #endregion Members
        #region Constructors
        public EFS_Commission(IReturnLegValuationPrice pPrice, IMoney pNotional)
        {
            Calc(pPrice, pNotional);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc(IReturnLegValuationPrice pPrice, IMoney pNotional)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            ICommission commission = pPrice.Commission;
            denomination = commission.CommissionDenomination;
            switch (denomination)
            {
                case CommissionDenominationEnum.BPS:
                    fee = commission.CreateMoney(pNotional.Amount.DecValue / commission.Amount.DecValue, pNotional.Currency); // Conversion si Currency différente
                    break;
                case CommissionDenominationEnum.CentsPerShare:
                    break;
                case CommissionDenominationEnum.FixedAmount:
                    fee = commission.CreateMoney(commission.Amount.DecValue, commission.Currency);
                    break;
                case CommissionDenominationEnum.Percentage:
                    if (pPrice.GrossPriceSpecified)
                        fee = commission.CreateMoney(pPrice.GrossPrice.Amount.DecValue * commission.Amount.DecValue, pPrice.GrossPrice.Currency.Value); // Conversion si Currency différente
                    break;
            }
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_Commission

    #region EFS_DayCountFraction
    #region revision
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Constructor with Interval Parameter 
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0</version><date>20071108</date><author>EG</author>
    ///     <comment>Ticket 15859   
    ///     Changed RollConventionEnum by period type (=DAY1 for Month, Trimestrial and Year, =NONE the others)
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0</version><date>2008024</date><author>PL</author>
    ///     <comment>Ticket 16064
    ///     </comment>
    /// </revision>
    #endregion revision
    public class EFS_DayCountFraction : IEFS_DayCountFraction
    {
        #region Members
        //Note: Utiliser uniquement dans le contexte des FEES (20080604 PL)
        protected bool m_IsCalculateAlwaysNumberOfCalendarYears;// = false;
        //
        protected DateTime m_DateStart;
        protected DateTime m_DateEnd;
        protected Nullable<DateTime> m_DateEndCoupon;
        // EG 20191209 New
        protected Nullable<StubEnum> m_Stub;
        protected DayCountFractionEnum m_DayCountFraction;
        //protected int m_PeriodMultiplier;
        //protected string m_Period;
        /// <summary>
        /// Nbr d'année(s) pleine(s)
        /// </summary>
        public int NumberOfCalendarYears;
        /// <summary>
        /// Obtient le Nbr de jour (hors années pleines) si base ACTACTAFB ou base ACTACTISDA ou si IsCalculateAlwaysNumberOfCalendarYears (voir cette propriété)
        /// <para>Obtient 0 sinon</para>
        /// </summary>
        public int Numerator;
        public decimal Denominator;
        protected IInterval m_IntervalFrequency;

        // EG 20150828 New
        protected DateTime m_DateEffective;
        protected DateTime m_DateTermination;

        #endregion Members
        
        #region Accessors
        #region IsCalculateAlwaysNumberOfCalendarYears
        public bool IsCalculateAlwaysNumberOfCalendarYears
        {
            set { m_IsCalculateAlwaysNumberOfCalendarYears = value; }
            get { return m_IsCalculateAlwaysNumberOfCalendarYears; }
        }
        #endregion IsCalculateAlwaysNumberOfCalendarYears

        #region IsNumberOfCalendarDaysSupEqualOneYear
        /// <summary>
        /// 
        /// </summary>
        public bool IsNumberOfCalendarDaysSupEqualOneYear
        {
            get
            {
                //int nbDays = TotalNumberOfCalendarDays;
                if (DayCountFractionEnum.DCF11 == m_DayCountFraction)
                    return false;
                else
                    return (Numerator >= Denominator) || (0 < NumberOfCalendarYears);
            }
        }
        #endregion IsNumberOfCalendarDaysSupEqualOneYear

        #region DayCountFraction
        public string DayCountFraction
        {
            get { return m_DayCountFraction.ToString(); }
        }
        #endregion DayCountFraction

        #region DayCountFraction_FpML
        public string DayCountFraction_FpML
        {
            get
            {
                FieldInfo fld = m_DayCountFraction.GetType().GetField(m_DayCountFraction.ToString());
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (0 != attributes.GetLength(0))
                    return ((XmlEnumAttribute)attributes[0]).Name;
                else
                    return DayCountFraction;
            }
            set
            {
                FieldInfo[] flds = m_DayCountFraction.GetType().GetFields();
                foreach (FieldInfo fld in flds)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                    if ((0 != attributes.GetLength(0)) && (value == ((XmlEnumAttribute)attributes[0]).Name))
                    {
                        m_DayCountFraction = (DayCountFractionEnum)fld.GetValue(m_DayCountFraction);
                        break;
                    }
                }
            }
        }
        #endregion DayCountFraction_FpML

        #region IntervalFrequency
        public IInterval IntervalFrequency
        {
            get { return m_IntervalFrequency; }
        }
        #endregion IntervalFrequency

        #region TotalNumberOfCalendarDays
        /// <summary>
        /// Obtient le nbr de jour calendaires indépendamment du DCF   
        /// <para>m_DateEnd - m_DateStart</para>
        /// </summary>
        public int TotalNumberOfCalendarDays
        {
            get
            {
                TimeSpan timeSpan = (m_DateEnd - m_DateStart);
                return timeSpan.Days;
            }
        }
        #endregion TotalNumberOfCalendarDays

        #region TotalNumberOfCalculatedDays
        /// <summary>
        /// Obtient le nbr de jours Total (y compris les années pleines) en fonction du DCF
        /// </summary>
        // EG 20191113 [FIXED INCOME] Pas de calcul Numerateur/Dénominateur/NbDays sur FI perpétuel
        // EG 20191209 Upd GetNumerator (Add m_Stub parameter)
        public int TotalNumberOfCalculatedDays
        {
            get
            {
                int numerator = 0;
                if ((m_DateEnd.Year != 9999) && (Cst.ErrLevel.SUCCESS == GetNumerator(m_DateStart, m_DateEnd, m_Stub.Value, ref numerator)))
                    return numerator;
                else
                    return 0;
            }
        }
        #endregion TotalNumberOfCalculatedDays

        #region Factor
        public decimal Factor
        {
            get
            {
                if (0 != Denominator)
                {
                    //20091002 PL See also EFS_CalculAmount()
                    //return NumberOfCalendarYears + (Numerator / Denominator);
                    return Convert.ToDecimal(NumberOfCalendarYears) + (Convert.ToDecimal(Numerator) / Denominator);
                }
                return 0;
            }
        }
        //20091002 PL Newness
        public decimal OppositeFactor
        {
            get
            {
                if (0 != Factor)
                    return 1 / Factor;
                return 1;
            }
        }
        //20091002 PL Newness
        public string FactorToString
        {
            get
            {
                string ret = string.Empty;
                if (0 != NumberOfCalendarYears)
                {
                    ret = NumberOfCalendarYears.ToString() + @"+";
                }
                if (decimal.Truncate(Denominator) == Denominator)
                    ret += @"/" + decimal.Truncate(Denominator).ToString();
                else
                    ret += @"/" + StrFunc.FmtDecimalToInvariantCulture(Denominator);
                return ret;
            }
        }
        //20091002 PL Newness
        // EG 20191113 [FIXED INCOME] Pas de calcul Numerateur/Dénominateur/NbDays sur FI perpétuel
        public decimal FactorFromTotalNumberOfCalculatedDays
        {
            get
            {
                decimal factor = 0;
                if (0 != Denominator)
                    factor = TotalNumberOfCalculatedDays / Denominator;
                return factor;
            }
        }
        #endregion

        // EG 20150828 New
        #region PeriodCount
        protected int PeriodCount
        {
            get
            {
                int periodCount = 1;
                //PL 20110624 Autorise m_IntervalFrequency nullable for Tools.CalculateInterest()
                if ((m_IntervalFrequency != null) && (m_IntervalFrequency.PeriodMultiplier.IntValue != 0))
                {
                    RollConventionEnum roll = RollConventionEnum.NONE;
                    if ((PeriodEnum.M == m_IntervalFrequency.Period) || (PeriodEnum.T == m_IntervalFrequency.Period) || (PeriodEnum.Y == m_IntervalFrequency.Period))
                        roll = RollConventionEnum.DAY1;
                    EFS_IntervalPeriods intervalPeriods = new EFS_IntervalPeriods(m_IntervalFrequency, roll, new DateTime(2003, 01, 01), new DateTime(2004, 01, 01));
                    periodCount = intervalPeriods.Count;
                }
                return periodCount;
            }
        }
        #endregion PeriodCount
        #endregion Accessors

        #region Constructors
        // 20080124 PL Ticket 16064 (pDateEndCoupon for Accrual Interest on ACT/ACT.SMA)
        public EFS_DayCountFraction() { }
        public EFS_DayCountFraction(DateTime pDateStart, DateTime pDateEnd, string pDayCountFraction, IInterval pIntervalFrequency)
            : this(pDateStart, pDateEnd, pDayCountFraction, pIntervalFrequency, null) { }
        // EG 20190415 [Migration BANCAPERTA]
        // EG 20190607 DCF => Call ReflectionTools.ConvertStringToEnumOrDefault
        // EG 20191209 Add m_Stub parameter
        public EFS_DayCountFraction(DateTime pDateStart, DateTime pDateEnd, string pDayCountFraction, IInterval pIntervalFrequency, Nullable<DateTime> pDateEndCoupon)
            : this(pDateStart, pDateEnd, ReflectionTools.ConvertStringToEnumOrDefault(pDayCountFraction, DayCountFractionEnum.DCF30360), pIntervalFrequency, pDateEndCoupon) { }
        public EFS_DayCountFraction(DateTime pDateStart, DateTime pDateEnd, DayCountFractionEnum pDayCountFraction, IInterval pIntervalFrequency)
            : this(pDateStart, pDateEnd, pDayCountFraction, pIntervalFrequency, null) { }
        public EFS_DayCountFraction(DateTime pDateStart, DateTime pDateEnd, DayCountFractionEnum pDayCountFraction, IInterval pIntervalFrequency, Nullable<DateTime> pDateEndCoupon)
            : this(pDateStart, pDateEnd, pDayCountFraction, pIntervalFrequency, pDateEndCoupon, StubEnum.None) { }
        // EG 20191209 Add m_Stub parameter
        public EFS_DayCountFraction(DateTime pDateStart, DateTime pDateEnd, DayCountFractionEnum pDayCountFraction, IInterval pIntervalFrequency,
            Nullable<DateTime> pDateEndCoupon, Nullable<StubEnum> pStub = StubEnum.None)
        {
            m_DateStart = pDateStart;
            m_DateEnd = pDateEnd;
            // EG 20150828 Nullable
            if (pDateEndCoupon.HasValue)
                m_DateEndCoupon = pDateEndCoupon.Value;
            m_DayCountFraction = pDayCountFraction;
            m_IntervalFrequency = pIntervalFrequency;
            m_Stub = pStub;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// An update of adjustableDate property triggers the automatic calculation of adjustedDate
        /// </summary>
        // EG 20191113 [FIXED INCOME] Pas de calcul Numerateur/Dénominateur/NbDays sur FI perpétuel
        // EG 20191209 Add m_Stub parameter (GetNumerator/GetDenominator)
        public void Calc()
        {
            Check();
            //
            int numerator = 0;
            decimal denominator = 0M;
            DateTime dtModifiedEnd = m_DateEnd;
            NumberOfCalendarYears = 0;
            if (m_DateEnd.Year != 9999)
            {
                //20080227 PL Décompte du nombre d'années pleines uniquement pour les DCF ACTACTAFB & ACTACTISDA
                if ((m_DayCountFraction == DayCountFractionEnum.ACTACTAFB)
                    || (m_DayCountFraction == DayCountFractionEnum.ACTACTISDA)
                    || m_IsCalculateAlwaysNumberOfCalendarYears)
                {
                    dtModifiedEnd = GetModifiedEndDate(m_DateStart, m_DateEnd, ref NumberOfCalendarYears);
                }

                if (Cst.ErrLevel.SUCCESS == GetNumerator(m_DateStart, dtModifiedEnd, m_Stub.Value, ref numerator))
                    Numerator = numerator;

                if (Cst.ErrLevel.SUCCESS == GetDenominator(m_DateStart, dtModifiedEnd, m_DateEndCoupon, m_Stub.Value, ref denominator))
                    Denominator = denominator;
            }

        }
        #endregion Calc
        #region GetModifiedEndDate
        protected static DateTime GetModifiedEndDate(DateTime pDateStart, DateTime pDateEnd, ref int pNumberOfCalendarYears)
        {
            int numberOfCalendarYears = 0;
            DateTime tmpEndDate = new DateTime(pDateEnd.Year, pDateEnd.Month, pDateEnd.Day);
            // Uses the default calendar of the InvariantCulture.
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            while (pDateStart < calendar.AddYears(tmpEndDate, -1))
            {
                numberOfCalendarYears++;
                tmpEndDate = calendar.AddYears(tmpEndDate, -1);
            }
            //
            pNumberOfCalendarYears = numberOfCalendarYears;
            //
            return tmpEndDate;
        }
        #endregion GetModifiedEndDate
        #region GetDayDiff
        protected static int GetDayDiff(DateTime pDateStart, DateTime pDateEnd)
        {
            return (pDateEnd.Day - pDateStart.Day);
        }
        #endregion GetDayDiff
        #region GetMonthDiff
        protected static int GetMonthDiff(DateTime pDateStart, DateTime pDateEnd)
        {
            return (pDateEnd.Month - pDateStart.Month);
        }
        #endregion GetMonthDiff
        #region GetYearDiff
        protected static int GetYearDiff(DateTime pDateStart, DateTime pDateEnd)
        {
            return (pDateEnd.Year - pDateStart.Year);
        }
        #endregion GetYearDiff
        #region GetDenominator
        // EG 20150828 Nullable<DateTime> for pDateEndCoupon
        // EG 20121204 [XXXXX] Correction calcul Denominateur sur méthode ACTACTICMA|ACTACTISMA
        // EG 20191209 Gestion Stub (Base ISMA/ICMA)
        protected Cst.ErrLevel GetDenominator(DateTime pDateStart, DateTime pDateEnd, Nullable<DateTime> pDateEndCoupon, StubEnum pStub, ref decimal opDenominator)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            TimeSpan timeSpan;

            switch (m_DayCountFraction)
            {
                case DayCountFractionEnum.DCF11:
                    opDenominator = 1;
                    break;
                case DayCountFractionEnum.DCF30360:
                case DayCountFractionEnum.DCF30EPLUS360:
                case DayCountFractionEnum.DCF30E360:
                case DayCountFractionEnum.ACT360:
                case DayCountFractionEnum.DCF30E360ISDA:
                    opDenominator = 360;
                    break;
                case DayCountFractionEnum.ACT365FIXED:
                    opDenominator = 365;
                    break;
                case DayCountFractionEnum.ACTACTISMA:
                case DayCountFractionEnum.ACTACTICMA:
                    try
                    {
                        // 20080124 PL Ticket 16064 Note: pDateEndCoupon n'est alimentée que dans le cadre du Réescompte (Accrual interests)
                        // EG 20150828 Nullable pDateEndCoupon
                        timeSpan = (pDateEndCoupon ?? pDateEnd) - pDateStart;
                        int numDays = timeSpan.Days;
                        int periodCount = PeriodCount;

                        // EG/PL 20150828 Gestion des stubs ("None|ShortInitial|LongInitial|ShortFinal|LongFinal)" . 
                        #region EG/PL 20150828 
                        // The accrual factor is (1/Freq) * Adjustment
                        // -------------------------------------------
                        // where Freq is the number of coupon per year and Adjustment depends of the type of stub period.
                        // 
                        // 1. None 
                        //    ----
                        //    The Adjustment is 1. 
                        //    The second expression reduces to 1 and the coupon is 1/Freq.
                        //
                        // 2. ShortInitial (Short at start)
                        //    -----------------------------
                        //    The Adjustment is computed as a ratio. 
                        //    The numerator is the number of days in the period. 
                        //    The denominator is the number of days between the standardised start date,
                        //    computed as the coupon end date minus the number of month corresponding to the frequency (i.e. 12/Freq), and the end date.
                        //
                        // 3. LongInitial (Long at start)
                        //    ---------------------------
                        //    Two standardised start dates are computed as the coupon end date minus one time and two times the number of month corresponding to the frequency. 
                        //    The numerator is the number of days between the start date and the first standardised start date 
                        //    and 
                        //    the denominator is the number of days between the first and second standardised start date. 
                        //    The adjustment is the ratio of the numerator by the denominator plus 1.
                        //
                        // 3. ShortFinal (Short at end)
                        //    -------------------------
                        //    Short at end The Adjustment is computed as a ratio. 
                        //    The numerator is the number of days in the period. 
                        //    The denominator is the number of days between the start date and the standardised end date, 
                        //    computed as the coupon start date plus the number of month corresponding to the frequency (i.e. 12/Freq).
                        //
                        // 4. LongFinal (Long at end)
                        //    -----------------------
                        //    Long at end Two standardised end dates are computed as the coupon start date plus one time and two times the number of month corresponding to the frequency. 
                        //    The numerator is the number of days between the end date and the first standardised end date 
                        //    and 
                        //    the denominator is the number of days between the second and first standardised end date. 
                        //    The Adjustment is the ratio of the numerator by the denominator plus 1.        
                        if (pDateEndCoupon.HasValue)
                        {
                            DateTime dtStandardised = DateTime.MinValue;
                            DateTime dtStandardised2 = DateTime.MinValue;

                            switch (pStub)
                            {
                                case StubEnum.None:
                                case StubEnum.Initial:
                                    dtStandardised = pDateEndCoupon.Value.AddMonths(-12 / periodCount);
                                    if (dtStandardised <= pDateStart)
                                    {
                                        // Short Initial Stub or None Stub (dtStandardised = pDateStart)
                                        opDenominator = ((TimeSpan)(pDateEndCoupon - dtStandardised)).Days * periodCount;
                                    }
                                    else if (dtStandardised > pDateStart)
                                    {
                                        // Long Initial Stub
                                        dtStandardised2 = dtStandardised.AddMonths(-12 / periodCount);
                                        opDenominator = (((TimeSpan)(dtStandardised - pDateStart)).Days / (((TimeSpan)(dtStandardised2 - dtStandardised)).Days + 1)) * periodCount;
                                    }
                                    break;
                                case StubEnum.Final:
                                    dtStandardised = pDateStart.AddMonths(12 / periodCount);
                                    if (dtStandardised >= pDateEndCoupon.Value)
                                    {
                                        // Short Final Stub or None Stub (dtStandardised = pDateEndCoupon)
                                        opDenominator = ((TimeSpan)(pDateStart - dtStandardised)).Days * periodCount;
                                    }
                                    else if (dtStandardised < pDateEndCoupon.Value)
                                    {
                                        // Long Final Stub
                                        dtStandardised2 = dtStandardised.AddMonths(12 / periodCount);
                                        opDenominator = (((TimeSpan)(dtStandardised - pDateEndCoupon.Value)).Days / (((TimeSpan)(dtStandardised2 - dtStandardised)).Days + 1)) * periodCount;
                                    }
                                    break;
                            }
                        }
                        else
                            opDenominator = numDays * periodCount;
                        #endregion EG/PL 20150828
                        if (opDenominator <= 0)
                            ret = Cst.ErrLevel.ABORTED;
                    }
                    catch { ret = Cst.ErrLevel.FAILURE; }
                    break;
                case DayCountFractionEnum.ACTACTISDA:
                    opDenominator = GetDaysWithSplitYear(pDateStart, pDateEnd);
                    break;
                case DayCountFractionEnum.ACTACTAFB:
                    // EG 20150828 Mod 
                    opDenominator = GetDaysInYear(pDateStart, pDateEndCoupon ?? pDateEnd);
                    break;
                case DayCountFractionEnum.ACT365L:
                    //20090529 PL A étudier: A priori ACT365L est identique à ACTACTAFB ()
                    Calendar calendar = CultureInfo.InvariantCulture.Calendar;
                    opDenominator = calendar.IsLeapYear(pDateEnd.Year) ? 366 : 365;
                    break;
                case DayCountFractionEnum.BUS252:
                    opDenominator = 252;
                    break;
                default:
                    ret = Cst.ErrLevel.UNDEFINED;
                    break;
            }
            return ret;
        }
        #endregion GetDenominator
        #region GetNumerator
        // EG 20191209 Gestion Stub (Base ISMA/ICMA)
        protected Cst.ErrLevel GetNumerator(DateTime pDateStart, DateTime pDateEnd, StubEnum pStub, ref int opNumerator)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            DateTime tmpDateStart = new DateTime(pDateStart.Year, pDateStart.Month, pDateStart.Day);
            DateTime tmpDateEnd = new DateTime(pDateEnd.Year, pDateEnd.Month, pDateEnd.Day);

            //20080227 CC Ticket 16097 Add ACTACTICMA & DCF30E360ISDA & BUS252
            switch (m_DayCountFraction)
            {
                case DayCountFractionEnum.DCF11:
                    opNumerator = 1;
                    break;
                case DayCountFractionEnum.DCF30E360ISDA:
                    DateTime dtTemp;
                    int d1 = tmpDateStart.Day;
                    int m1 = tmpDateStart.Month;
                    int y1 = tmpDateStart.Year;
                    int d2 = tmpDateEnd.Day;
                    int m2 = tmpDateEnd.Month;
                    int y2 = tmpDateEnd.Year;
                    
                    dtTemp = tmpDateStart.AddDays(1);
                    if (dtTemp.Month != m1)
                    {
                        d1 = 30;
                    }
                    dtTemp = tmpDateEnd.AddDays(1);
                    if (dtTemp.Month != m2)
                    {
                        //TODO 20080227 PL (on a besoin de la termination date...)
                        d2 = 30;
                    }
                    opNumerator = (d2 - d1) + (30 * (m2 - m1)) + (360 * (y2 - y1));
                    break;
                case DayCountFractionEnum.DCF30EPLUS360:
                case DayCountFractionEnum.DCF30360:
                case DayCountFractionEnum.DCF30E360:
                    bool isStartDayIs30 = (30 == tmpDateStart.Day);
                    bool isStartDayIs31 = (31 == tmpDateStart.Day);
                    bool isEndDayIs31 = (31 == tmpDateEnd.Day);

                    if (31 == tmpDateStart.Day)
                    {
                        tmpDateStart = tmpDateStart.AddDays(-1);
                    }
                    //DayCountFractionEnum.DCF30360
                    if (DayCountFractionEnum.DCF30360 == m_DayCountFraction)
                    {
                        if ((isStartDayIs30 || isStartDayIs31) && isEndDayIs31)
                            tmpDateEnd = tmpDateEnd.AddDays(-1);
                    }
                    else if (DayCountFractionEnum.DCF30E360 == m_DayCountFraction)
                    {
                        //DayCountFractionEnum.DCF30E360
                        if (isEndDayIs31)
                            tmpDateEnd = tmpDateEnd.AddDays(-1);
                    }
                    else if (DayCountFractionEnum.DCF30EPLUS360 == m_DayCountFraction)
                    {
                        //DayCountFractionEnum.DCF30EPLUS360
                        // Sets day = 1 and m + 1
                        if (isEndDayIs31)
                            tmpDateEnd = tmpDateEnd.AddDays(1);
                    }
                    //
                    opNumerator = GetDayDiff(tmpDateStart, tmpDateEnd) +
                        (30 * GetMonthDiff(tmpDateStart, tmpDateEnd)) +
                        (360 * GetYearDiff(tmpDateStart, tmpDateEnd));
                    break;
                case DayCountFractionEnum.ACT360:
                case DayCountFractionEnum.ACT365FIXED:
                case DayCountFractionEnum.ACTACTISDA:
                case DayCountFractionEnum.ACTACTAFB:
                case DayCountFractionEnum.ACT365L:
                    TimeSpan timeSpan = pDateEnd - pDateStart;
                    // //EG 20140724 FDA-Funding: Daily period with pDateStart=pDateEnd=BizDt
                    // //opNumerator = timeSpan.Days;
                    // opNumerator = Math.Max(timeSpan.Days, 1);
                    //PL 20150706 Restore initial code
                    //            NB: Dans EvenstValRTS.cs, Filip utilise AddDays(1) sur la date fin des événements de FDA, afin de tjs considérer pour ces événements le dernier jour inclus.
                    //                La restauration ici du code initial va solutionner le pb sur le Réescompte d'une période de 1 jour rencontré chez BA.
                    opNumerator = timeSpan.Days;
                    break;
                case DayCountFractionEnum.ACTACTISMA:
                case DayCountFractionEnum.ACTACTICMA:
                    int periodCount = PeriodCount;
                    DateTime dtStandardised;
                    DateTime dtStandardised2;
                    switch (pStub)
                    {
                        case StubEnum.None:
                            opNumerator = ((TimeSpan)(pDateEnd - pDateStart)).Days;
                            break;
                        case StubEnum.Initial:
                            dtStandardised = pDateEnd.AddMonths(-12 / periodCount);
                            if (dtStandardised <= pDateStart)
                            {
                                // Short Initial Stub or None Stub
                                opNumerator = ((TimeSpan)(pDateEnd - pDateStart)).Days;
                            }
                            else if (dtStandardised > pDateStart)
                            {
                                // Long Initial Stub
                                dtStandardised2 = dtStandardised.AddMonths(-12 / periodCount);
                                opNumerator = ((TimeSpan)(dtStandardised - pDateStart)).Days + ((TimeSpan)(dtStandardised - dtStandardised2)).Days;
                            }
                            break;
                        case StubEnum.Final:
                            dtStandardised = pDateStart.AddMonths(12 / periodCount);
                            if (dtStandardised >= pDateEnd)
                            {
                                // Short Final Stub 
                                opNumerator = ((TimeSpan)(pDateEnd - pDateStart)).Days;
                            }
                            else if (dtStandardised < pDateEnd)
                            {
                                // Long Final Stub
                                dtStandardised2 = dtStandardised.AddMonths(12 / periodCount);
                                opNumerator = ((TimeSpan)(pDateEnd - dtStandardised)).Days + ((TimeSpan)(dtStandardised2 - dtStandardised)).Days;
                            }
                            break;
                    }
                    break;
                case DayCountFractionEnum.BUS252:
                    //TODO 20080227 PL (on a besoin des BCs...)
                    opNumerator = 0;
                    ret = Cst.ErrLevel.DATANOTFOUND;
                    break;
                default:
                    ret = Cst.ErrLevel.UNDEFINED;
                    break;
            }
            return ret;
        }
        #endregion GetNumerator
        #region GetDaysWithSplitYear
        protected decimal GetDaysWithSplitYear(DateTime pStartDate, DateTime pEndDate)
        {
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            TimeSpan timeSpan;
            decimal retValue;
            if (calendar.IsLeapYear(pStartDate.Year) ^ calendar.IsLeapYear(pEndDate.Year))
            {
                bool isLeapYear;
                int nbDays;
                DateTime dateEndOfYear = new DateTime(pStartDate.Year, 12, 31);
                DateTime dateStartOfYear = new DateTime(pEndDate.Year, 1, 1);
                isLeapYear = calendar.IsLeapYear(pStartDate.Year);
                timeSpan = dateEndOfYear - pStartDate;
                //20071114 PL Ticket:15950 Il manquait un jour... (le 31/12)
                nbDays = timeSpan.Days + 1;
                decimal percDaysYearStart = (isLeapYear ? (decimal)nbDays / 366 : (decimal)nbDays / 365);

                isLeapYear = calendar.IsLeapYear(pEndDate.Year);
                timeSpan = pEndDate - dateStartOfYear;
                nbDays = timeSpan.Days;
                decimal percDaysYearEnd = (isLeapYear ? (decimal)nbDays / 366 : (decimal)nbDays / 365);
                retValue = Numerator / (percDaysYearStart + percDaysYearEnd);
            }
            else
            {
                retValue = (calendar.IsLeapYear(pStartDate.Year) || calendar.IsLeapYear(pEndDate.Year)) ? (decimal)366 : (decimal)365;
            }
            return retValue;
        }
        #endregion GetDaysWithSplitYear
        #region GetDaysInYear
        private int GetDaysInYear(DateTime pStartDate, DateTime pEndDate)
        {
            int retDaysInYear = 365;
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            //Si Date Debut se trouve sur une année bissextile 
            //Si Date Debut est inférieure ou égale au 29/02 et si DateFin strictement supérieure au 29/02, alors 366
            if (calendar.IsLeapYear(pStartDate.Year))
            {
                DateTime leapYearDate = new DateTime(pStartDate.Year, 2, 29);
                if ((pStartDate <= leapYearDate) && (pEndDate > leapYearDate))
                    retDaysInYear = 366;
            }
            //Si Date Fin se trouve sur une année bissextile 
            //Si Date Debut est inférieure ou égale au 29/02 et si DateFin strictement supérieure au 29/02, alors 366
            if (calendar.IsLeapYear(pEndDate.Year))
            {
                DateTime leapYearDate = new DateTime(pEndDate.Year, 2, 29);
                if ((pStartDate <= leapYearDate) && (pEndDate > leapYearDate))
                    retDaysInYear = 366;
            }
            return retDaysInYear;
        }
        #endregion GetDaysInYear
        #region IsExistLeapYear
        /// <summary>
        /// 20090529 PL Deprecated
        /// </summary>
        /// <param name="pStartDate"></param>
        /// <param name="pEndDate"></param>
        /// <returns></returns>
        protected static bool IsExistLeapYear_DEPRECATED(DateTime pStartDate, DateTime pEndDate)
        {


            bool isOk = false;
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            if (pStartDate.Year == pEndDate.Year)
            {
                if (calendar.IsLeapYear(pStartDate.Year))
                {
                    DateTime leapYearDate = new DateTime(pStartDate.Year, 2, 29);
                    isOk = ((pStartDate <= leapYearDate) && (pEndDate > leapYearDate));
                }
            }
            else
            {
                isOk = ((calendar.IsLeapYear(pStartDate.Year)) && (pStartDate <= new DateTime(pStartDate.Year, 2, 29))) ||
                    ((calendar.IsLeapYear(pEndDate.Year)) && (pEndDate > new DateTime(pEndDate.Year, 2, 29)));
            }
            return isOk;

        }
        #endregion GetNumberOfLeapYears
        #region private Check
        /// <summary>
        /// Vérifie la cohérence des données en entrée
        /// </summary>
        /// <exception cref="SpheresException2 si une incohérence est constatée"></exception>
        private void Check()
        {

            switch (m_DayCountFraction)
            {
                case DayCountFractionEnum.ACTACTISMA:
                case DayCountFractionEnum.ACTACTICMA:
                    if (null == m_IntervalFrequency)
                    {
                        throw new Exception(StrFunc.AppendFormat("Interval is not specified for DayCountFraction:{0}", m_DayCountFraction.ToString()));
                    }
                    break;
                default:
                    break;
            }

        }
        #endregion
        #endregion Methods

        #region IEFS_DayCountFraction Members
        string IEFS_DayCountFraction.DayCountFractionFpML { get { return this.DayCountFraction_FpML; } }
        int IEFS_DayCountFraction.Numerator { get { return this.Numerator; } }
        decimal IEFS_DayCountFraction.Denominator { get { return this.Denominator; } }
        int IEFS_DayCountFraction.NumberOfCalendarYears { get { return this.NumberOfCalendarYears; } }
        int IEFS_DayCountFraction.TotalNumberOfCalculatedDays { get { return this.TotalNumberOfCalculatedDays; } }
        #endregion IEFS_DayCountFraction Members
    }
    #endregion EFS_DayCountFraction
    #region EFS_DefaultParty
    /// <summary>
    /// 
    /// </summary>
    public class EFS_DefaultParty
    {
        #region Members
        /// <summary>
        /// Représente le BIC ou L'identifier de l'acteur (si pas de BIC)
        /// </summary>
        public string id;
        /// <summary>
        /// Représente l'idA de l'acteur
        /// </summary>
        public int OTCmlId;
        /// <summary>
        /// Représente l'identifier de la partie
        /// </summary>
        public string partyId;
        /// <summary>
        /// Représente le displayName de la partie
        /// </summary>
        public string partyName;
        #endregion Members

        #region Constructors
        public EFS_DefaultParty() { }
        public EFS_DefaultParty(string pId, int pOTCmlId, string pIdentifier, string pDisplayName)
        {
            id = pId;
            OTCmlId = pOTCmlId;
            partyId = pIdentifier;
            partyName = pDisplayName;
        }
        #endregion Constructors
    }
    #endregion EFS_DefaultParty
    #region EFS_DeliveryPeriod
    /// <summary>
    /// Periode de livraison / Paiement pour un DC
    /// </summary>
    // EG 20171025 [23509] Add timeZone 
    public class EFS_DeliveryPeriod
    {
        #region Members
        public EFS_Period period;
        public DateTimeOffset deliveryDateStart;
        public DateTimeOffset deliveryDateEnd;
        public string timeZone;
        public DateTime settlementDate;
        public long deliveryHours;
        #endregion Members

        #region Accessors
        #region StartPeriod
        public DateTime StartPeriod
        {
            get { return period.date1; }
        }
        #endregion DeliveryStartDate
        #region EndPeriod
        public DateTime EndPeriod
        {
            get { return period.date2; }
        }
        #endregion EndPeriod
        #endregion Accessors
        #region Constructors
        // EG 20171025 [23509] Upd
        public EFS_DeliveryPeriod(EFS_Period pPeriod, DateTimeOffset pDeliveryDateStart, DateTimeOffset pDeliveryDateEnd, string pTimeZone, DateTime pSettlementDate, bool pIsApplySummertime)
        {
            period = pPeriod;
            deliveryDateStart = pDeliveryDateStart;
            deliveryDateEnd = pDeliveryDateEnd;
            timeZone = pTimeZone;
            settlementDate = pSettlementDate;

            TimeSpan diff;
            // Si prise en compte heure été <=> hiver alors différentiel en datetime UTC (23/25 heures)
            // sinon différentiel en datetime Local (24 heures)
            if (pIsApplySummertime)
                diff = deliveryDateEnd.UtcDateTime - deliveryDateStart.UtcDateTime;
            else
                diff = deliveryDateEnd.LocalDateTime - deliveryDateStart.LocalDateTime;
            deliveryHours = Convert.ToInt64(diff.TotalHours);
        }
        #endregion Constructors
    }
    #endregion EFS_DeliveryPeriod

    #region EFS_EquivalentRate
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Parameter(s) IntervalFrequency to constructor(s)
    ///     Used by EFS_DayCountFraction in EFS_CalculationPeriod
    ///     </comment>
    /// </revision>
    public class EFS_EquivalentRate
    {
        #region Members
        protected EquiRateMethodEnum m_EquiRateMethod;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected decimal m_SourceRate;
        protected IRounding m_Rounding;
        protected string m_SourceDayCountFraction;
        protected IInterval m_SourceIntervalFrequency;
        protected EFS_DayCountFraction m_SourceDcf;
        protected string m_TargetDayCountFraction;
        protected IInterval m_TargetIntervalFrequency;
        protected EFS_DayCountFraction m_TargetDcf;

        public decimal compoundRate;
        public decimal simpleRate;
        #endregion Members
        #region Constructors
        // 20071106 EG Ticket 15859
        public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod, DateTime pStartDate, DateTime pEndDate,
            decimal pSourceRate, string pSourceDayCountFraction, IInterval pIntervalFrequency)
            : this(pEquiRateMethod, pStartDate, pEndDate, pSourceRate, null, pSourceDayCountFraction, pIntervalFrequency) { }
        // 20071106 EG Ticket 15859
        public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod, DateTime pStartDate, DateTime pEndDate,
            decimal pSourceRate, string pSourceDayCountFraction, IInterval pSourceIntervalFrequency, string pTargetDayCountFraction, IInterval pTargetIntervalFrequency)
            : this(pEquiRateMethod, pStartDate, pEndDate, pSourceRate, null,
            pSourceDayCountFraction, pSourceIntervalFrequency, pTargetDayCountFraction, pTargetIntervalFrequency) { }
        // 20071106 EG Ticket 15859
        public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod, DateTime pStartDate, DateTime pEndDate,
            decimal pSourceRate, IRounding pRounding, string pSourceDayCountFraction, IInterval pIntervalFrequency)
            : this(pEquiRateMethod, pStartDate, pEndDate, pSourceRate, pRounding,
            pSourceDayCountFraction, pIntervalFrequency, pSourceDayCountFraction, pIntervalFrequency) { }
        // 20071106 EG Ticket 15859
        public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod, DateTime pStartDate, DateTime pEndDate,
            decimal pSourceRate, IRounding pRounding,
            string pSourceDayCountFraction, IInterval pSourceIntervalFrequency,
            string pTargetDayCountFraction, IInterval pTargetIntervalFrequency)
        {
            m_EquiRateMethod = pEquiRateMethod;
            m_StartDate = pStartDate;
            m_EndDate = pEndDate;
            m_SourceRate = pSourceRate;
            m_Rounding = pRounding;
            m_SourceDayCountFraction = pSourceDayCountFraction;
            m_SourceIntervalFrequency = pSourceIntervalFrequency;
            m_TargetDayCountFraction = pTargetDayCountFraction;
            m_TargetIntervalFrequency = pTargetIntervalFrequency;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public void Calc()
        {

            #region Rounding SourceRate
            if (null != m_Rounding)
            {
                EFS_Round round = new EFS_Round(m_Rounding, m_SourceRate);
                m_SourceRate = round.AmountRounded;
            }
            #endregion Rounding SourceRate

            #region SourceDayCountFraction
            // 20071108 Eg Ticket 15859
            m_SourceDcf = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_SourceDayCountFraction, m_SourceIntervalFrequency);
            // 20071108 Eg Ticket 15859
            m_TargetDcf = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_TargetDayCountFraction, m_TargetIntervalFrequency);
            #endregion SourceDayCountFraction

            switch (m_EquiRateMethod)
            {
                case EquiRateMethodEnum.CompoundToSimple:
                    EquivalentSimpleRateFromCompoundRate();
                    break;
                case EquiRateMethodEnum.SimpleToCompound:
                    EquivalentCompoundRateFromSimpleRate();
                    break;
                case EquiRateMethodEnum.SimpleToOvernightDecapitalized:
                    EquivalentOvernightDecapitailzedRateFromSimpleRate();
                    break;
            }

        }
        #endregion Calc
        #region EquivalentCompoundRateFromSimpleRate
        /// <summary>
        /// Calcul un taux monétaire (Application de la formule des intérêts simple) à partir d'un taux actuariel
        /// </summary>
        /// <returns></returns>
        private void EquivalentCompoundRateFromSimpleRate()
        {

            simpleRate = m_SourceRate;
            decimal x = (1 + simpleRate);
            decimal y = m_SourceDcf.Factor;
            decimal z = m_TargetDcf.OppositeFactor;
            compoundRate = Convert.ToDecimal(System.Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y))) - 1;
            compoundRate *= z;

        }
        #endregion EquivalentCompoundRateFromSimpleRate
        #region EquivalentSimpleRateFromCompoundRate
        /// <summary>
        /// calcul un taux actuariel (Application de la formule des intérêts composé) à partir d'un taux monétaire
        /// </summary>
        /// <returns></returns>
        private void EquivalentSimpleRateFromCompoundRate()
        {

            compoundRate = m_SourceRate;
            decimal x = 1 + (compoundRate * m_SourceDcf.Factor);
            decimal y = m_TargetDcf.OppositeFactor;
            simpleRate = Convert.ToDecimal(System.Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y))) - 1;

        }
        #endregion EquivalentSimpleRateFromCompoundRate

        #region EquivalentSimpleRateFromCompoundRate
        /// <summary>
        /// Calcul un taux overnight decapitalisé (Application de la formule des intérêts simple) à partir d'un taux actuariel 
        /// </summary>
        /// <returns></returns>
        private void EquivalentOvernightDecapitailzedRateFromSimpleRate()
        {

            simpleRate = m_SourceRate;
            decimal x = (1 + simpleRate);
            decimal y = 1 / m_SourceDcf.Denominator;
            decimal z = m_TargetDcf.Denominator;
            compoundRate = Convert.ToDecimal(System.Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y))) - 1;
            compoundRate *= z;

        }
        #endregion EquivalentSimpleRateFromCompoundRate
        #endregion Methods
    }
    #endregion EFS_EquivalentRate
    #region EFS_ExchangeRate
    public class EFS_ExchangeRate : IEFS_ExchangeRate
    {
        #region Members
        public IExchangeRate exchangeRate;
        public bool referenceCurrencySpecified;
        public string referenceCurrency;
        public bool notionalAmountSpecified;
        public EFS_Decimal notionalAmount;
        #endregion Members
        #region Constructors
        public EFS_ExchangeRate(IExchangeRate pExchangeRate) : this(pExchangeRate, null) { }
        public EFS_ExchangeRate(IExchangeRate pExchangeRate, IMoney pAmount)
        {
            exchangeRate = pExchangeRate;
            if (null != pAmount)
            {
                referenceCurrencySpecified = StrFunc.IsFilled(pAmount.Currency);
                if (referenceCurrencySpecified)
                    referenceCurrency = pAmount.Currency;

                notionalAmountSpecified = (0 < pAmount.Amount.DecValue);
                if (notionalAmountSpecified)
                    notionalAmount = pAmount.Amount;
            }
        }
        #endregion Constructors

        #region IEFS_ExchangeRate Members
        IExchangeRate IEFS_ExchangeRate.ExchangeRate { get { return this.exchangeRate; } }
        bool IEFS_ExchangeRate.ReferenceCurrencySpecified { get { return this.referenceCurrencySpecified; } }
        string IEFS_ExchangeRate.ReferenceCurrency { get { return this.referenceCurrency; } }
        bool IEFS_ExchangeRate.NotionalAmountSpecified { get { return this.notionalAmountSpecified; } }
        EFS_Decimal IEFS_ExchangeRate.NotionalAmount { get { return this.notionalAmount; } }
        #endregion IEFS_ExchangeRate Members
    }
    #endregion EFS_ExchangeRate
    #region EFS_ExerciseDatesEvent
    public class EFS_ExerciseDatesEvent
    {
        #region Members
        public ExerciseStyleEnum exerciseStyle;
        public string exerciseClass;
        public EFS_Date startExerciseDate;
        public EFS_Date endExerciseDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool relevantUnderlyingDateSpecified;
        public EFS_Date relevantUnderlyingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashSettlementValuationDateSpecified;
        public EFS_Date cashSettlementValuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashSettlementPaymentDateSpecified;
        public EFS_Date cashSettlementPaymentDate;
        #endregion Members
        #region Accessors
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {
                return startExerciseDate;

            }
        }
        #endregion AdjustedStartPeriod
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20170220 [22842] New
        public EFS_Date AdjustedEndPeriod
        {
            get
            {
                return endExerciseDate;
            }
        }
        #endregion AdjustedEndPeriod
        #region AdjustedRelevantUnderlyingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedRelevantUnderlyingDate
        {
            get
            {
                return relevantUnderlyingDate;

            }
        }
        #endregion AdjustedRelevantUnderlyingDate
        #region AdjustedCashSettlementValuationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedCashSettlementValuationDate
        {
            get
            {
                return cashSettlementValuationDate;

            }
        }
        #endregion AdjustedCashSettlementValuationDate
        #region AdjustedCashSettlementPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedCashSettlementPaymentDate
        {
            get
            {
                return cashSettlementPaymentDate;

            }
        }
        #endregion AdjustedCashSettlementPaymentDate
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                if (null != startExerciseDate)
                    return new EFS_EventDate(startExerciseDate.DateValue, startExerciseDate.DateValue);
                else
                    return null;

            }
        }
        #endregion StartPeriod
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                if (null != endExerciseDate)
                    return new EFS_EventDate(endExerciseDate.DateValue, endExerciseDate.DateValue);
                else
                    return null;

            }
        }
        #endregion EndPeriod
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                string eventType = string.Empty;
                switch (exerciseStyle)
                {
                    case ExerciseStyleEnum.American:
                        eventType = EventTypeFunc.American;
                        break;
                    case ExerciseStyleEnum.Bermuda:
                        eventType = EventTypeFunc.Bermuda;
                        break;
                    case ExerciseStyleEnum.European:
                        eventType = EventTypeFunc.European;
                        break;
                }
                return eventType;
            }
        }
        #endregion EventType
        #endregion Accessors
    }
    #endregion EFS_ExerciseDatesEvent

    #region EFS_ExtendBarrierFeatures
    public class EFS_ExtendBarrierFeatures
    {
        #region Members
        public bool barrierCapSpecified;
        public EFS_ExtendedTriggerEvent barrierCap;
        public bool barrierFloorSpecified;
        public EFS_ExtendedTriggerEvent barrierFloor;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExtendBarrierFeatures(string pCS, IExtendedBarrier pBarrier, EFS_Underlyer pUnderlyer,
            EFS_OptionStrikeBase pStrike, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            #region BarrierCap
            barrierCapSpecified = pBarrier.BarrierCapSpecified;
            if (barrierCapSpecified)
                barrierCap = new EFS_ExtendedTriggerEvent(pCS, EventTypeFunc.Cap, pUnderlyer, pBarrier.BarrierCap, pStrike, pBusinessDayAdjustments, pDataDocument);
            #endregion BarrierCap
            #region BarrierFloor
            barrierFloorSpecified = pBarrier.BarrierFloorSpecified;
            if (barrierFloorSpecified)
                barrierFloor = new EFS_ExtendedTriggerEvent(pCS, EventTypeFunc.Floor, pUnderlyer, pBarrier.BarrierFloor, pStrike, pBusinessDayAdjustments, pDataDocument);
            #endregion BarrierFloor
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_ExtendBarrierFeatures

    #region EFS_HolidayBase
    /// <summary>
    /// Classe de base pour la représentation d'un jour férié
    /// </summary>
    public abstract class EFS_HolidayBase
    {
        #region Members
        public bool IsBanking;
        public bool IsCommodityBusiness;
        public bool IsCurrencyBusiness;
        public bool IsExchangeBusiness;
        public bool IsScheduledTradingDay;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20130502 [18636] add Description
        public string Description;
        #endregion Members

        #region Constructors
        //PL 20131121
        public EFS_HolidayBase(bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
        {
            IsBanking = pIsBanking;
            IsCommodityBusiness = pIsCommodityBusiness;
            IsCurrencyBusiness = pIsCurrencyBusiness;
            IsExchangeBusiness = pIsExchangeBusiness;
            IsScheduledTradingDay = pIsScheduledTradingDay;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDayType"></param>
        /// <returns></returns>
        public bool IsDayType(DayTypeEnum pDayType)
        {
            bool isHoliday;
            switch (pDayType)
            {
                case DayTypeEnum.Business:
                    // Business Days :
                    // ISDA 2000 section 1.4 définit explicitement comme Business Days, en cas d'ajustement, les jours 
                    // où les banques ET les marchés des changes sont ouverts (par opposition avec les Banking Days où
                    // seules les banques sont ouvertes).
                    isHoliday = (IsBanking || IsCurrencyBusiness);
                    break;
                case DayTypeEnum.CommodityBusiness: //PL 20131121
                    isHoliday = IsCommodityBusiness;
                    break;
                case DayTypeEnum.CurrencyBusiness:
                    isHoliday = IsCurrencyBusiness;
                    break;
                case DayTypeEnum.ExchangeBusiness:
                    isHoliday = IsExchangeBusiness;
                    break;
                case DayTypeEnum.ScheduledTradingDay:
                    //PL 20131121
                    //isHoliday = IsExchangeBusiness;
                    isHoliday = IsScheduledTradingDay;
                    break;
                case DayTypeEnum.Calendar:
                default:
                    isHoliday = false;
                    break;
            }
            return isHoliday;
        }
        #endregion Methods
    }
    #endregion EFS_HolidayBase
    #region EFS_HolidayCalculated
    public class EFS_HolidayCalculated : EFS_HolidayBase
    {
        #region Members
        public string HolidayName;
        #endregion Members
        #region Constructors
        //PL 20131121
        public EFS_HolidayCalculated(string pHolidayName, bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
            : base(pIsBanking, pIsCommodityBusiness, pIsCurrencyBusiness, pIsExchangeBusiness, pIsScheduledTradingDay)
        {
            HolidayName = pHolidayName;
        }
        #endregion Constructors
    }
    #endregion EFS_HolidayCalculated
    #region EFS_HolidayMonthly
    public class EFS_HolidayMonthly : EFS_HolidayBase
    {
        #region Members
        public string PositionDay;
        public string DDD;
        public int MM;
        public int? MarkDay;
        #endregion Members
        #region Constructors
        //PL 20131121
        public EFS_HolidayMonthly(object pPositionDay, object pDDD, object pMM, object pMarkDay, bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
            : base(pIsBanking, pIsCommodityBusiness, pIsCurrencyBusiness, pIsExchangeBusiness, pIsScheduledTradingDay)
        {
            PositionDay = pPositionDay.ToString();
            DDD = pDDD.ToString();
            MM = Convert.ToInt32(pMM);
            if (pMarkDay != Convert.DBNull)
                MarkDay = Convert.ToInt32(pMarkDay);
        }
        #endregion Constructors
    }
    #endregion EFS_HolidayMonthly
    #region EFS_HolidayMisc
    public class EFS_HolidayMisc : EFS_HolidayBase
    {
        #region Members
        public DateTime DTMisc;
        public bool IsHoliday;
        #endregion Members
        #region Constructors
        //PL 20131121
        public EFS_HolidayMisc(DateTime pDTMisc, bool pIsHoliday, bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
            : base(pIsBanking, pIsCommodityBusiness, pIsCurrencyBusiness, pIsExchangeBusiness, pIsScheduledTradingDay)
        {
            DTMisc = pDTMisc;
            IsHoliday = pIsHoliday;
        }
        #endregion Constructors
    }
    #endregion EFS_HolidayMisc
    #region EFS_HolidayWeekly
    public class EFS_HolidayWeekly : EFS_HolidayBase
    {
        #region Members
        public string DDD;
        #endregion Members
        #region Constructors
        //PL 20131121
        public EFS_HolidayWeekly(string pDDD, bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
            : base(pIsBanking, pIsCommodityBusiness, pIsCurrencyBusiness, pIsExchangeBusiness, pIsScheduledTradingDay)
        {
            DDD = pDDD;
        }
        #endregion Constructors
    }
    #endregion EFS_HolidayWeekly
    #region EFS_HolidayYearly
    public class EFS_HolidayYearly : EFS_HolidayBase
    {
        #region Members
        public int DD;
        public int MM;
        //public bool IsHolidayWeeklySlide;
        public Cst.HolidayMethodOfAdjustment MethodOfAdjustment;
        public int SlidedDD;
        public int SlidedMM;
        public int SlidedYYYY;
        #endregion Members
        #region Constructors
        //public EFS_HolidayYearly(int pDD, int pMM, bool pIsHolidayWeeklySlide, bool pIsBanking, bool pIsCurrencyBusiness, bool pIsExchangeBusiness)
        //PL 20131121
        public EFS_HolidayYearly(int pDD, int pMM, string pMethodOfAdjustment, bool pIsBanking, bool pIsCommodityBusiness, bool pIsCurrencyBusiness, bool pIsExchangeBusiness, bool pIsScheduledTradingDay)
            : base(pIsBanking, pIsCommodityBusiness, pIsCurrencyBusiness, pIsExchangeBusiness, pIsScheduledTradingDay)
        {
            DD = pDD;
            MM = pMM;
            //IsHolidayWeeklySlide = pIsHolidayWeeklySlide;
            if (System.Enum.IsDefined(typeof(Cst.HolidayMethodOfAdjustment), pMethodOfAdjustment))
                MethodOfAdjustment = (Cst.HolidayMethodOfAdjustment)System.Enum.Parse(typeof(Cst.HolidayMethodOfAdjustment), pMethodOfAdjustment, true);
            else
                MethodOfAdjustment = Cst.HolidayMethodOfAdjustment.NONE;
        }
        #endregion Constructors
    }
    #endregion EFS_HolidayYearly

    #region EFS_Interval
    public class EFS_Interval
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected IInterval interval;
        /// <summary>
        /// 
        /// </summary>
        protected DateTime startDate;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected bool startDateSpecified;
        /// <summary>
        /// 
        /// </summary>
        protected DateTime terminationDate;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected bool terminationDateSpecified;
        /// <summary>
        /// 
        /// </summary>
        public DateTime offsetDate;
        #endregion Members

        #region Constructors
        public EFS_Interval() { }
        public EFS_Interval(IInterval pInterval, DateTime pStartDate, DateTime pTerminationDate)
        {
            this.interval = pInterval;
            this.startDate = pStartDate;
            this.startDateSpecified = (pStartDate != Convert.ToDateTime(null));
            this.terminationDate = pTerminationDate;
            this.terminationDateSpecified = (pTerminationDate != Convert.ToDateTime(null));
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        protected virtual Cst.ErrLevel Calc()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            if (this.startDateSpecified)
                ret = GetDateByInterval(this.startDate);
            return ret;
        }
        #endregion Calc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        protected Cst.ErrLevel GetDateByInterval(DateTime pDate)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            DateTime offsetDate = pDate;
            int periodMultiplier = this.interval.PeriodMultiplier.IntValue;
            switch (this.interval.Period)
            {
                case PeriodEnum.T:
                    if (terminationDateSpecified)
                        offsetDate = terminationDate;
                    else
                        ret = Cst.ErrLevel.ABORTED;
                    break;
                case PeriodEnum.D:
                    offsetDate = pDate.AddDays(periodMultiplier);
                    break;
                case PeriodEnum.W:
                    offsetDate = pDate.AddDays(periodMultiplier * 7);
                    break;
                case PeriodEnum.M:
                    offsetDate = pDate.AddMonths(periodMultiplier);
                    break;
                case PeriodEnum.Y:
                    offsetDate = pDate.AddYears(periodMultiplier);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Period [{0}] not implemented", this.interval.Period.ToString()));
            }

            if (ret != Cst.ErrLevel.SUCCESS)
                this.offsetDate = Convert.ToDateTime(null);
            else
                this.offsetDate = offsetDate;

            return ret;
        }

        #endregion Methods
    }
    #endregion EFS_IntervalBase
    #region EFS_IntervalPeriods
    /// <summary>
    /// CalcFrequency() : Apply an interval between two dates and load a collection of periods (Output: EFS_Period[] periodDates).
    /// Calc()          : Apply an offset to a date (Output: DateTime offsetDate).
    /// </summary>
    public class EFS_IntervalPeriods : EFS_Interval
    {
        #region Members
        public EFS_Period[] PeriodDates;
        private readonly RollConventionEnum rollConvention;
        private Cst.ErrLevel m_ErrLevel;
        #endregion Members
        #region Constructors
        public EFS_IntervalPeriods(IInterval pInterval, RollConventionEnum pRollConvention, DateTime pStartDate, DateTime pEndDate)
        {
            this.interval = pInterval;
            this.rollConvention = pRollConvention;
            this.startDate = pStartDate;
            this.startDateSpecified = (pStartDate != Convert.ToDateTime(null));
            this.terminationDate = pEndDate;
            this.terminationDateSpecified = (pEndDate != Convert.ToDateTime(null));
            Calc();
        }
        #endregion Constructors
        #region Accessors
        public int Count
        {
            get
            {

                if (null != PeriodDates)
                    return PeriodDates.Length;
                else
                    return 0;

            }
        }
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion Accessors
        #region Methods
        #region Calc
        protected override Cst.ErrLevel Calc()
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;

            #region Validation Rules
            if (startDateSpecified && terminationDateSpecified)
            {
                m_ErrLevel = Cst.ErrLevel.SUCCESS;
                if (((0 < interval.PeriodMultiplier.IntValue) || (PeriodEnum.T == interval.Period)) && (terminationDate < startDate))
                    m_ErrLevel = Cst.ErrLevel.ABORTED;
                else if (0 > interval.PeriodMultiplier.IntValue && (terminationDate > startDate))
                    m_ErrLevel = Cst.ErrLevel.ABORTED;
                else if ((PeriodEnum.T != interval.Period) && (terminationDate == startDate))
                    m_ErrLevel = Cst.ErrLevel.ABORTED;
            }
            else
            {
                m_ErrLevel = Cst.ErrLevel.ABORTED;
            }
            #endregion Validation Rules

            ArrayList aPeriodDates = new ArrayList();
            if (m_ErrLevel == Cst.ErrLevel.SUCCESS)
            {
                if (PeriodEnum.T == interval.Period)
                {
                    aPeriodDates.Add(new EFS_Period(startDate, terminationDate));
                }
                else
                {
                    DateTime date2 = startDate;
                    int guard = 0;
                    while (true)
                    {
                        DateTime date1 = date2;
                        if (Cst.ErrLevel.SUCCESS == GetDateByInterval(date1))
                        {
                            date2 = this.offsetDate;

                            // MF 20120511 Ticket 17776
                            //EFS_RollConvention efs_rollConvention = new EFS_RollConvention(rollConvention, date2);
                            EFS_RollConvention efs_rollConvention = EFS_RollConvention.GetNewRollConvention(rollConvention, date2, default);
                            date2 = efs_rollConvention.rolledDate;
                            if (DtFunc.IsDateTimeEmpty(date2))
                            {
                                m_ErrLevel = Cst.ErrLevel.ABORTED;
                                break;
                            }

                            if (((0 < interval.PeriodMultiplier.IntValue) && (date2 <= terminationDate)) ||
                                ((0 > interval.PeriodMultiplier.IntValue) && (date2 >= terminationDate)))
                            {
                                aPeriodDates.Add(new EFS_Period(date1, date2));
                                if (date2 == terminationDate)
                                    break;
                                else
                                    continue;
                            }
                            else if (this.offsetDate > terminationDate)
                            {
                                break;
                            }
                            else
                            {
                                m_ErrLevel = Cst.ErrLevel.ABORTED;
                                break;
                            }
                        }
                        guard++;
                        if (guard == 9999)
                        {
                            m_ErrLevel = Cst.ErrLevel.ABORTED;
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Loop PeriodDates");
                        }
                    }
                }
            }
            if (m_ErrLevel == Cst.ErrLevel.SUCCESS)
            {
                if ((m_ErrLevel == Cst.ErrLevel.SUCCESS) && (aPeriodDates.Count > 0))
                {
                    this.PeriodDates = (EFS_Period[])aPeriodDates.ToArray(aPeriodDates[0].GetType());
                }
            }

            return m_ErrLevel;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_IntervalPeriods

    #region EFS_KnockFeatures
    public class EFS_KnockFeatures
    {
        #region Members
        public bool knockInSpecified;
        public EFS_TriggerEvent knockIn;
        public bool knockOutSpecified;
        public EFS_TriggerEvent knockOut;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_KnockFeatures(string pConnectionString, IKnock pKnock, EFS_Underlyer pUnderlyer, EFS_OptionStrikeBase pStrike,
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            #region KnockIn
            knockInSpecified = pKnock.KnockInSpecified;
            if (knockInSpecified)
                knockIn = new EFS_TriggerEvent(pConnectionString, EventTypeFunc.KnockIn, pUnderlyer, pKnock.KnockIn, pStrike, pBusinessDayAdjustments, pDataDocument);
            #endregion KnockIn
            #region KnockOut
            knockOutSpecified = pKnock.KnockOutSpecified;
            if (knockOutSpecified)
                knockOut = new EFS_TriggerEvent(pConnectionString, EventTypeFunc.KnockOut, pUnderlyer, pKnock.KnockOut, pStrike, pBusinessDayAdjustments, pDataDocument);
            #endregion KnockOut
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_KnockFeatures



    #region EFS_Offset
    public class EFS_Offset
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        private readonly bool m_IsAdjustDateBeforeApplyOffset;
        //
        public IOffset offset;
        public DateTime[] startDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool startDateSpecified;
        public DateTime terminationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool terminationDateSpecified;
        public EFS_BusinessCenters businessCenters;
        public IBusinessDayAdjustments businessDayAdjustments;
        public DateTime[] offsetDate;
        #endregion Members
        #region Accessors
        #region IsBusinessDayConventionSpecified
        private bool IsBusinessDayConventionSpecified
        {
            get
            {
                return ((null != this.businessDayAdjustments) &&
                        (BusinessDayConventionEnum.NotApplicable != businessDayAdjustments.BusinessDayConvention) &&
                        (BusinessDayConventionEnum.NONE != businessDayAdjustments.BusinessDayConvention));
            }
        }
        #endregion IsBusinessDayConventionSpecified
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument;
            if (null != pOffset)
                this.offset = pOffset;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DateTime[] pStartDate, DateTime pTerminationDate, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            :this(pCS, pOffset, pDataDocument)
        {
            if (null != this.offset)
            {
                this.startDate = (DateTime[])pStartDate.Clone();
                this.startDateSpecified = (pStartDate.Length > 0);
                this.terminationDate = pTerminationDate;
                this.terminationDateSpecified = (pTerminationDate != Convert.ToDateTime(null));
                this.businessCenters = new EFS_BusinessCenters(m_Cs, pBusinessDayAdjustments, m_DataDocument);
                this.businessDayAdjustments = pBusinessDayAdjustments;
                this.offsetDate = new DateTime[pStartDate.Length];
                Calc();
            }
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DateTime pStartDate, DateTime pTerminationDate, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pCS, pOffset, pDataDocument)
        {
            if (null != this.offset)
            {
                this.startDate = new DateTime[1] { pStartDate };
                this.startDateSpecified = (pStartDate != Convert.ToDateTime(null));
                this.terminationDate = pTerminationDate;
                this.terminationDateSpecified = (pTerminationDate != Convert.ToDateTime(null));
                this.businessCenters = new EFS_BusinessCenters(m_Cs, pBusinessDayAdjustments, m_DataDocument);
                this.businessDayAdjustments = pBusinessDayAdjustments;
                this.offsetDate = new DateTime[1];
                Calc();
            }
        }
        // RD 20110419 [17414]
        // Pour appliquer en cas de décalage BUSINESS, un ajustement PRCEDING ou FOLLOWING en fonction du signe de l’offset, avant d’appliquer l’offset.
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DateTime pStartDate, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pCS, pOffset, pStartDate, pBusinessDayAdjustments, false, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DateTime pStartDate, IBusinessDayAdjustments pBusinessDayAdjustments, bool pIsAdjustDateBeforeApplyOffset, DataDocumentContainer pDataDocument)
            : this(pCS, pOffset, pDataDocument)
        {
            m_IsAdjustDateBeforeApplyOffset = pIsAdjustDateBeforeApplyOffset;

            if (null != this.offset)
            {
                this.startDate = new DateTime[1] { pStartDate };
                this.startDateSpecified = (pStartDate != Convert.ToDateTime(null));
                //this.terminationDateSpecified = false;
                this.businessCenters = new EFS_BusinessCenters(m_Cs, pBusinessDayAdjustments, m_DataDocument);
                this.businessDayAdjustments = pBusinessDayAdjustments;
                this.offsetDate = new DateTime[1];
                Calc();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pOffset"></param>
        /// <param name="pStartDate"></param>
        /// <param name="pTerminationDate"></param>
        /// <param name="pBusinessCenters">Null est une valeur autorisée</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Offset(string pCS, IOffset pOffset, DateTime pStartDate, DateTime pTerminationDate, IBusinessCenters pBusinessCenters, DataDocumentContainer pDataDocument)
            : this(pCS, pOffset, pDataDocument)
        {
            if (null != this.offset)
            {
                this.startDate = new DateTime[1] { pStartDate };
                this.startDateSpecified = (pStartDate != Convert.ToDateTime(null));
                this.terminationDate = pTerminationDate;
                this.terminationDateSpecified = (pTerminationDate != Convert.ToDateTime(null));
                this.businessCenters = new EFS_BusinessCenters(m_Cs, pBusinessCenters, pDataDocument);
                this.offsetDate = new DateTime[1];
                Calc();
            }
        }
        #endregion Constructors

        #region Methods
        #region Calc
        // EG 20190115 [24361] Use isSettlementOfHolidayDeliveryConvention (Migration financial settlement for BoM Products)
        public Cst.ErrLevel Calc()
        {

            #region local Variables
            int multiplier = this.offset.PeriodMultiplier.IntValue;

            Cst.ErrLevel ret;
            #endregion local Variables
            #region Checking
            if (this.offset.DayTypeSpecified)
            {
                if ((DayTypeEnum.Calendar != this.offset.DayType) &&
                    ((PeriodEnum.D != this.offset.Period) ||
                    ((false == this.businessCenters.businessCentersSpecified) && this.IsBusinessDayConventionSpecified)))
                    ret = Cst.ErrLevel.ABORTED;
                else
                    ret = Cst.ErrLevel.SUCCESS;
            }
            else
            {
                this.offset.DayTypeSpecified = true;
                this.offset.DayType = DayTypeEnum.Business;
                ret = Cst.ErrLevel.SUCCESS;
            }
            #endregion Checking
            #region Process
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                #region periodMultiplier = 0
                if (multiplier == 0)
                {
                    for (int i = 0; i < this.startDate.Length; i++)
                    {
                        if ((DayTypeEnum.Calendar == this.offset.DayType) ||
                            (false == this.businessCenters.IsHoliday(this.startDate[i], this.offset.DayType)))
                        {
                            this.offsetDate[i] = this.startDate[i];
                        }
                        else
                        {
                            // EG 20190115 [24361]
                            if ((null != this.businessDayAdjustments) && this.businessDayAdjustments.IsSettlementOfHolidayDeliveryConvention)
                            {
                                // On cherche le prochain jour ouvré précédent|suivant (sur la base du businessDayConvention présent sur this.businessDayAdjustments)
                                this.offsetDate[i] = this.SubCalc(this.startDate[i], this.businessDayAdjustments.BusinessDayConvention == BusinessDayConventionEnum.FOLLOWING ? 1 : -1);
                            }
                            else
                            {
                                // On cherche le prochain jour ouvré précédent (ex.: fixing date d'un Reset de Stream flottant)
                                this.offsetDate[i] = this.SubCalc(this.startDate[i], -1);
                            }
                        }
                    }
                }
                #endregion
                #region period = Term
                else if (PeriodEnum.T == this.offset.Period)
                {
                    if (this.terminationDateSpecified)
                        for (int i = 0; i < this.startDate.Length; i++)
                            this.offsetDate[i] = this.terminationDate;
                    else
                        ret = Cst.ErrLevel.ABORTED;
                }
                #endregion period = Term
                #region dayType = Calendar
                else if (DayTypeEnum.Calendar == this.offset.DayType)
                {
                    for (int i = 0; i < this.startDate.Length; i++)
                    {
                        EFS_Interval interval = new EFS_Interval(this.offset.GetInterval(multiplier, this.offset.Period), this.startDate[i], this.terminationDate.Date);
                        this.offsetDate[i] = interval.offsetDate;
                    }
                }
                #endregion dayType = Calendar
                #region other
                else
                {
                    if (startDateSpecified)
                    {
                        for (int i = 0; i < this.startDate.Length; i++)
                        {
                            // RD 20110419 [17414]
                            // Pour appliquer en cas de décalage BUSINESS, un ajustement PRCEDING ou FOLLOWING en fonction du signe de l’offset, avant d’appliquer l’offset.
                            if (m_IsAdjustDateBeforeApplyOffset &&
                                this.businessCenters.IsHoliday(this.startDate[i], this.offset.DayType))
                            {
                                //On cherche le prochain jour ouvré en fonction du signe de l'offset
                                this.startDate[i] = this.SubCalc(this.startDate[i], (multiplier > 0 ? +1 : -1));
                            }
                            //
                            this.offsetDate[i] = this.SubCalc(this.startDate[i], multiplier);
                        }
                    }
                }
                #endregion other
            }
            #endregion Process
            return ret;

        }
        #endregion Calc
        #region SubCalc
        private DateTime SubCalc(DateTime pDate, int pMultiplier)
        {
            int guard = 0;

            // Note: pMultiplier est toujours différent de Zero
            DateTime retDate;
            while (true)
            {
                guard++;
                if (guard == 999)
                {
                    string sdate = DtFunc.DateTimeToStringDateISO(pDate);
                    throw new Exception(StrFunc.AppendFormat("Infinite Loop detected [Method Arguments:{0} - {1}]", sdate, pMultiplier.ToString()));
                }

                if (pMultiplier == 0)
                {
                    //On a décalé ici du nombre de jour souhaité
                    retDate = pDate;
                    break;
                }
                else
                {
                    EFS_Interval interval = new EFS_Interval(offset.GetInterval(Math.Sign(pMultiplier), PeriodEnum.D), pDate, terminationDate);
                    // EG 20100416 interval.offsetDate.Date replace interval.offsetDate
                    if (!this.businessCenters.IsHoliday(interval.offsetDate.Date, this.offset.DayType))
                    {
                        //On est ici sur un jour non férié --> on décrémente le nombre de jour de décalage
                        pMultiplier -= Math.Sign(pMultiplier);
                    }
                    pDate = interval.offsetDate;
                }
            }
            return retDate;
        }
        #endregion SubCalc
        #endregion Methods
    }
    #endregion EFS_Offset
    #region EFS_OptionFeaturesDates
    public class EFS_OptionFeaturesDates
    {
        #region Members
        protected string cs;
        protected DataDocumentContainer m_DataDocument;
        protected IBusinessDayAdjustments bda;
        protected EFS_Underlyer underlyer;
        protected bool schedulesSpecified;
        protected IAveragingSchedule[] schedules;
        protected bool dateTimeListSpecified;
        protected IDateTimeList dateTimeList;
        protected bool dateListSpecified;
        protected IDateList dateList;

        public EFS_ValuationDates[] valuationDates;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionFeaturesDates(string pConnectionString, EFS_Underlyer pUnderlyer,
            IBusinessDayAdjustments pBusinessDayAdjustments, bool pAveragingSchedulesSpecified, IAveragingSchedule[] pAveragingSchedules, DataDocumentContainer pDataDocument)
        {
            cs = pConnectionString;
            m_DataDocument = pDataDocument;
            bda = pBusinessDayAdjustments;
            schedulesSpecified = pAveragingSchedulesSpecified;
            schedules = pAveragingSchedules;
            underlyer = pUnderlyer;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionFeaturesDates(string pConnectionString, EFS_Underlyer pUnderlyer,
            bool pAveragingSchedulesSpecified, IAveragingSchedule[] pAveragingSchedules,
            bool pDateTimeListSpecified, IDateTimeList pDateTimeList, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pUnderlyer, pBusinessDayAdjustments, pAveragingSchedulesSpecified, pAveragingSchedules, pDataDocument)
        {
            dateTimeListSpecified = pDateTimeListSpecified;
            dateTimeList = pDateTimeList;
            Calc();
        }
        public EFS_OptionFeaturesDates(string pConnectionString, EFS_Underlyer pUnderlyer,
            bool pAveragingSchedulesSpecified, IAveragingSchedule[] pAveragingSchedules,
            bool pDateListSpecified, IDateList pDateList, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pUnderlyer, pBusinessDayAdjustments, pAveragingSchedulesSpecified, pAveragingSchedules, pDataDocument)
        {
            dateListSpecified = pDateListSpecified;
            dateList = pDateList;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            List<EFS_ValuationDates> lstValuationDates = new List<EFS_ValuationDates>();
            if (schedulesSpecified)
            {

                foreach (IAveragingSchedule schedule in schedules)
                {
                    ret = CalcSchedule(schedule, lstValuationDates);
                }
            }
            if (dateTimeListSpecified)
                ret = CalcDateTimeList(lstValuationDates);
            if (dateListSpecified)
                ret = CalcDateList(lstValuationDates);

            if ((Cst.ErrLevel.SUCCESS == ret) && (0 < lstValuationDates.Count))
            {
                valuationDates = lstValuationDates.ToArray();
            }

            return ret;
        }
        #endregion Calc
        #region CalcDateList
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcDateList(List<EFS_ValuationDates> pLstValuationDates)
        {
            for (int i = 0; i < dateList.Date.Length; i++)
            {
                EFS_AdjustableDate valuationDate = new EFS_AdjustableDate(cs, dateList[i], bda, DayTypeEnum.ScheduledTradingDay, m_DataDocument);
                pLstValuationDates.Add(UnderlyerValuationDate(valuationDate));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalcDateList
        #region CalcDateTimeList
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcDateTimeList(List<EFS_ValuationDates> pLstValuationDates)
        {
            foreach (EFS_DateTimeArray dt in dateTimeList.DateTime)
            {
                EFS_AdjustableDate valuationDate = new EFS_AdjustableDate(cs, dt.DateTimeValue, bda, DayTypeEnum.ScheduledTradingDay, m_DataDocument);
                pLstValuationDates.Add(UnderlyerValuationDate(valuationDate));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalcDateTimeList
        #region CalcSchedule
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcSchedule(IAveragingSchedule pSchedule, List<EFS_ValuationDates> pLstValuationDates)
        {
            DateTime startDate = pSchedule.StartDate.DateValue;
            DateTime endDate = pSchedule.EndDate.DateValue;
            DayTypeEnum dayType = DayTypeEnum.ScheduledTradingDay;
            IResetFrequency observationFrequency = pSchedule.ResetFrequency;
            if (FrequencyTypeEnum.Day == pSchedule.FrequencyType)
                dayType = DayTypeEnum.Calendar;

            #region StartDate
            EFS_AdjustableDate valuationDate = new EFS_AdjustableDate(cs, startDate, bda, dayType, m_DataDocument);
            pLstValuationDates.Add(UnderlyerValuationDate(valuationDate));
            #endregion StartDate

            DateTime observationDate = startDate;
            int guard = 0;
            bool isContinue = true;
            Cst.ErrLevel ret;
            while (isContinue)
            {
                observationDate = Tools.ApplyInterval(observationDate, endDate, observationFrequency.Interval);

                if (pSchedule.WeekNumberSpecified && observationFrequency.WeeklyRollConventionSpecified)
                {
                    observationDate = Tools.GetRankDayOfWeek(observationDate, pSchedule.WeekNumber.IntValue, observationFrequency.WeeklyRollConvention);
                }
                else if (observationFrequency.WeeklyRollConventionSpecified)
                    observationDate = Tools.ApplyWeeklyRollConvention(observationDate, observationFrequency.WeeklyRollConvention);

                isContinue = (observationDate < endDate);
                if (isContinue)
                {
                    valuationDate = new EFS_AdjustableDate(cs, observationDate, bda, dayType, m_DataDocument);
                    pLstValuationDates.Add(UnderlyerValuationDate(valuationDate));
                }

                guard++;
                if (guard == 999)
                {
                    //Loop parapet
                    string msgException = "Observation Dates exception:" + Cst.CrLf;
                    msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                    msgException += "Please, verify dates, averaging schedule parameters, and business day adjustment on the trade";
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                }
            }
            #region EndDate
            valuationDate = new EFS_AdjustableDate(cs, endDate, bda, dayType, m_DataDocument);
            pLstValuationDates.Add(UnderlyerValuationDate(valuationDate));
            #endregion EndDate
            ret = Cst.ErrLevel.SUCCESS;

            return ret;
        }
        #endregion CalcSchedule
        #region UnderlyerValuationDate
        private EFS_ValuationDates UnderlyerValuationDate(EFS_AdjustableDate pValuationDate)
        {
            if (underlyer is EFS_SingleUnderlyer underlyer1)
                return new EFS_SingleUnderlyerValuationDates(cs, underlyer1, pValuationDate);
            else if (underlyer is EFS_Basket basket)
                return new EFS_BasketValuationDates(cs, basket, pValuationDate);
            else if (underlyer is EFS_BasketConstituent constituent)
                return new EFS_BasketConstituentValuationDates(cs, constituent, pValuationDate);
            else if (underlyer is EFS_BondUnderlyer underlyer2)
                return new EFS_BondUnderlyerValuationDates(cs, underlyer2, pValuationDate);
            else
                return null;
        }
        #endregion UnderlyerValuationDate
        #endregion Methods
    }
    #endregion EFS_OptionFeaturesDates
    #region EFS_OriginalPayment
    public class EFS_OriginalPayment
    {
        #region Members
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_EventDate settlementDate;
        public IMoney paymentAmount;
        public EFS_EventDate expirationDate;
        // EG/PM 20150108 New 
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        // EG 20150708 [21103] New Periodes (par défaut = paymentDate, mais forcées sur SKP)
        public Pair<DateTime,DateTime> dtEventStartPeriod;
        public Pair<DateTime, DateTime> dtEventEndPeriod;

        #endregion Members
        #region Accessors
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get
            {
                return paymentAmount.Amount;

            }
        }
        #endregion Amount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                return paymentAmount.Currency;

            }
        }
        #endregion Currency
        #region ExpirationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpirationDate
        {
            get
            {
                return expirationDate;

            }
        }
        #endregion ExpirationDate
        #region PaymentAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PaymentAmount
        {
            get
            {
                return this.paymentAmount.Amount;

            }
        }
        #endregion PaymentAmount
        #region PaymentCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PaymentCurrency
        {
            get
            {
                return this.paymentAmount.Currency;

            }
        }
        #endregion PaymentCurrency
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(paymentDateAdjustment);

            }
        }
        #endregion PaymentDate
        #region SettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate SettlementDate
        {
            get
            {
                return settlementDate;

            }
        }
        #endregion SettlementDate

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
        {
            get {return payerPartyReference.HRef; }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
        {
            get {return receiverPartyReference.HRef; }
        }
        #endregion ReceiverPartyReference

        #endregion Accessors
        #region Constructors
        // EG 20180307 [23769]
        public EFS_OriginalPayment(IMoney pPayment)
        {
            paymentAmount = pPayment;
        }
        // EG 20180307 [23769]
        public EFS_OriginalPayment(IMoney pPayment, EFS_FxOptionPremium pFxOptionPremium)
        {
            paymentAmount = pPayment;
            settlementDate = pFxOptionPremium.SettlementDate;
            expirationDate = pFxOptionPremium.ExpirationDate;
        }
        // EG 20180307 [23769]
        public EFS_OriginalPayment(IMoney pPayment, EFS_Payment pEFS_Payment)
        {
            paymentAmount = pPayment;
            paymentDateAdjustment = pEFS_Payment.paymentDateAdjustment;
            expirationDate = pEFS_Payment.expirationDate;
            // EG/PM 20150108 New 
            payerPartyReference = pEFS_Payment.payerPartyReference;
            receiverPartyReference = pEFS_Payment.receiverPartyReference;
            // EG 20150708 [21103] New
            dtEventStartPeriod = pEFS_Payment.dtEventStartPeriod;
            dtEventEndPeriod = pEFS_Payment.dtEventEndPeriod;
        }
        #endregion Constructors
    }
    #endregion EFS_OriginalPayment

    #region EFS_Payment
    /// <revision>
    ///     <version>1.1.8</version><date>20070823</date><author>EG</author>
    ///     <comment>
    ///     Apply Rules to determine the date of Pre-Settlement events
    ///     </comment>
    /// </revision>
    /// 20090708 EG Intégration du payment d'origine
    // EG 20180307 [23769] Gestion dbTransaction
    public class EFS_Payment
    {
        #region Members
        private readonly string m_Cs;
        private readonly IDbTransaction m_DbTransaction;
        private readonly DataDocumentContainer m_DataDocument;

        private Cst.ErrLevel m_ErrLevel;
        public EFS_AdjustableDate paymentDateAdjustment;
        public IMoney paymentAmount;
        public bool exchangeRateSpecified;
        public EFS_ExchangeRate exchangeRate;
        public bool paymentQuoteSpecified;
        public EFS_PaymentQuote paymentQuote;
        public EFS_EventDate expirationDate;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;
        //
        public bool paymentSourceSpecified;
        public EFS_PaymentSource paymentSource;
        //
        public bool originalPaymentSpecified;
        public EFS_OriginalPayment originalPayment;
        // EG 20100506 Ticket 16978
        public bool taxSpecified;
        public EFS_Tax[] tax;
        // EG/PM 20150108 Add PartyReference
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        // EG 20150708 [21103] New
        public Pair<DateTime,DateTime> dtEventStartPeriod;
        public Pair<DateTime, DateTime> dtEventEndPeriod;
        #endregion Members
        #region Accessors
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get
            {
                return paymentAmount.Amount;

            }
        }
        #endregion Amount
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                EFS_Date retDt = null;
                if ((null != paymentDateAdjustment) && ((false == paymentSourceSpecified) || (false == paymentSource.isFeeInvoicing)))
                {
                    //Note: Si paymentSource.isFeeInvoicing = true, alors il s'agit d'un paiement destiné à être encaissé 
                    //      au sein d'une facture globale, on ne génère donc pas de date afin de ne pas générer d'eventclass 
                    //      See also: Invoicing_AdjustedPaymentDate
                    retDt = new EFS_Date
                    {
                        DateValue = paymentDateAdjustment.adjustedDate.DateValue
                    };
                }
                return retDt;

            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlement.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                return paymentAmount.Currency;

            }
        }
        #endregion Currency
        #region ExchangeRate
        public EFS_ExchangeRate ExchangeRate
        {
            get { return exchangeRate; }
        }
        #endregion ExchangeRate
        #region ExpirationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpirationDate
        {
            get
            {
                return expirationDate;

            }
        }
        #endregion ExpirationDate
        #region Invoicing_AdjustedPaymentDate
        //2080703 PL Newness
        // 20081030 EG Modif
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date Invoicing_AdjustedPaymentDate
        {
            get
            {
                EFS_Date retDt = null;
                if ((null != paymentDateAdjustment) && paymentSourceSpecified && paymentSource.isFeeInvoicing)
                {
                    //Note: Si paymentSource.isFeeInvoicing=false, alors il ne s'agit pas d'un paiement destiné à être encaissé 
                    //      au sein d'une facture globale, on ne génère donc pas de date afin de ne pas générer d'eventclass 
                    //      See also: AdjustedPaymentDate
                    retDt = new EFS_Date
                    {
                        DateValue = paymentDateAdjustment.adjustedDate.DateValue
                    };
                }
                return retDt;

            }
        }
        #endregion Invoicing_AdjustedPaymentDate
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(paymentDateAdjustment);

            }
        }
        #endregion PaymentDate
        #region PaymentQuote
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentQuote PaymentQuote
        {
            get
            {
                return paymentQuote;

            }
        }
        #endregion PaymentQuote
        #region PaymentSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentSource PaymentSource
        {
            get
            {
                return paymentSource;

            }
        }
        #endregion PaymentSource
        #region UnAdjustedPaymentDate
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {

                if (null != paymentDateAdjustment && paymentDateAdjustment.adjustableDateSpecified)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion UnAdjustedPaymentDate
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
        {
            get{return payerPartyReference.HRef; }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
        {
            get {return receiverPartyReference.HRef; }
        }
        #endregion ReceiverPartyReference

        #region PaymentType
        public string PaymentType(string pEventCode)
        {

            if (paymentSourceSpecified)
            {
                return paymentSource.eventType;
            }
            else
            {
                if (EventCodeFunc.IsAdditionalPayment(pEventCode))
                    return EventTypeFunc.Fee;
                else if (EventCodeFunc.IsOtherPartyPayment(pEventCode))
                    return EventTypeFunc.Brokerage;
                else
                    return EventTypeFunc.CashSettlement.ToString();
            }

        }
        #endregion PaymentType

        #endregion Accessors
        #region Constructors
        //public EFS_Payment(string pCS)
        //{
        //    m_Cs = pCS;
        //    m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        //}
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_Payment(string pCS, IDbTransaction pDbTransaction, IPayment pPayment, DataDocumentContainer pDataDocument) :
            this(pCS, pDbTransaction, pPayment, Convert.ToDateTime(null), null, pDataDocument)
        {
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_Payment(string pCS, IDbTransaction pDbTransaction, IPayment pPayment, IProduct pProduct, DataDocumentContainer pDataDocument) :
            this(pCS, pDbTransaction, pPayment, Convert.ToDateTime(null), pProduct, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20211005 [XXXXX] DebentureSecurityTransaction - Correction alimentation de la classe de travail : efs_DebtSecurityTransactionStream
        public EFS_Payment(string pCS, IDbTransaction pDbTransaction, IPayment pPayment, DateTime pValueDate, IProduct pProduct, DataDocumentContainer pDataDocument)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;
            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;
            m_DataDocument = pDataDocument;
            payerPartyReference = pPayment.PayerPartyReference;
            receiverPartyReference = pPayment.ReceiverPartyReference;
            Calc(pPayment, pValueDate);

            if (pProduct is IDebtSecurityTransaction)
            {
                IDebtSecurityTransaction dst = pProduct as IDebtSecurityTransaction;
                if (null == dst.Efs_DebtSecurityTransactionStream)
                {
                    DebtSecurityTransactionContainer container = new DebtSecurityTransactionContainer(pCS, pDbTransaction, dst, pDataDocument);
                    container.DebtSecurityTransaction.SetStreams(pCS, pDataDocument, Cst.StatusBusiness.ALLOC);
                }
            }
            expirationDate = CalcExpirationDate(pProduct);

            if (originalPaymentSpecified)
                originalPayment = new EFS_OriginalPayment(pPayment.PaymentAmount, this);

            CalcTax(pPayment);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPayment"></param>
        /// <param name="pValueDate">Doit être renseigné si paymentDate ou adjustedPaymentDate ne sont pas renseigné</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT) 
        // EG 20180530 [23980] m_DbTransaction to EFS_AdjustableDate and EFS_Cash
        // EG 20230505 [XXXXX] [WI617] paymentDateAdjustment optional => controls for Trade template
        private void Calc(IPayment pPayment, DateTime pValueDate)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;
            #region paymentDate adjustement
            if (pPayment.PaymentDateSpecified)
            {
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DbTransaction, pPayment.PaymentDate, m_DataDocument);
            }
            else if (pPayment.AdjustedPaymentDateSpecified)
            {
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DbTransaction, m_DataDocument)
                {
                    adjustedDate = pPayment.CreateAdjustedDate(pPayment.AdjustedPaymentDate)
                };
            }
            else if (DtFunc.IsDateTimeFilled(pValueDate))
            {
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DbTransaction, m_DataDocument)
                {
                    adjustedDate = pPayment.CreateAdjustedDate(pValueDate)
                };
            }
            #endregion paymentDate adjustement
            //
            #region payment settlement
            paymentQuoteSpecified = pPayment.PaymentQuoteSpecified;
            if (paymentQuoteSpecified)
            {
                paymentQuote = new EFS_PaymentQuote
                {
                    paymentRelativeTo = pPayment.PaymentQuote.PaymentRelativeTo.HRef,
                    percentageRate = new EFS_Decimal(pPayment.PaymentQuote.PercentageRate.DecValue)
                };
                object notionalReference = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, paymentQuote.paymentRelativeTo);
                if (null != notionalReference)
                {
                    IMoney notionalAmount = pPayment.GetNotionalAmountReference(notionalReference);
                    if (null != notionalAmount)
                    {
                        paymentQuote.notionalAmount = new EFS_Decimal(notionalAmount.Amount.DecValue);
                        paymentQuote.currency = notionalAmount.Currency;
                    }
                }
            }
            //
            originalPaymentSpecified = pPayment.CustomerSettlementPaymentSpecified;
            if (originalPaymentSpecified)
            {
                ICustomerSettlementPayment settlementPayment = pPayment.CustomerSettlementPayment;
                decimal sourcePaymentAmount = pPayment.PaymentAmount.Amount.DecValue;
                exchangeRateSpecified = (null != settlementPayment.Rate);
                if (exchangeRateSpecified)
                    exchangeRate = new EFS_ExchangeRate(settlementPayment.Rate);

                if ((false == settlementPayment.AmountSpecified) || (0 == settlementPayment.Amount.DecValue))
                {
                    string currencyAmount = pPayment.PaymentAmount.Currency;
                    if (paymentQuoteSpecified)
                    {
                        currencyAmount = paymentQuote.currency;
                        if (0 == sourcePaymentAmount)
                            sourcePaymentAmount = paymentQuote.percentageRate.DecValue * paymentQuote.notionalAmount.DecValue;
                    }

                    if (exchangeRateSpecified)
                    {
                        decimal rate = exchangeRate.exchangeRate.Rate.DecValue;
                        IQuotedCurrencyPair qcp = exchangeRate.exchangeRate.QuotedCurrencyPair;
                        IMoney moneyAmount = pPayment.CreateMoney(sourcePaymentAmount, currencyAmount);
                        EFS_Cash cash = new EFS_Cash(m_Cs, m_DbTransaction, moneyAmount, rate, qcp);
                        paymentAmount = pPayment.CreateMoney(cash.ExchangeAmountRounded, settlementPayment.Currency);
                    }
                }
                else
                {
                    paymentAmount = pPayment.CreateMoney(settlementPayment.Amount.DecValue, settlementPayment.Currency);
                }
            }
            else
            {
                paymentAmount = pPayment.PaymentAmount;
            }
            #endregion payment settlement
            // 20070823 EG Ticket : 15643
            #region preSettlementPaymentDate
            // 20150623 [21149] Test TradeLibrary is null, Cas d'un appel hors génération des événements, hors Services spheres (par exemple valorisation DTSETTLT dans TRADEINSTRUMENT)
            if (null != m_DataDocument)
            {
                EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, m_DbTransaction, pPayment.PaymentAmount.Currency,
                    pPayment.PayerPartyReference.HRef, pPayment.ReceiverPartyReference.HRef, m_DataDocument, paymentAmount.DefaultOffsetPreSettlement);
                preSettlementSpecified = (preSettlementInfo.IsUsePreSettlement);
                if (preSettlementSpecified)
                    preSettlement = new EFS_PreSettlement(m_Cs, m_DbTransaction, paymentDateAdjustment.adjustedDate.DateValue, pPayment.PaymentAmount.Currency, preSettlementInfo.OffsetPreSettlement, m_DataDocument);
            }
            #endregion preSettlementPaymentDate
            //
            #region PaymentSource
            paymentSourceSpecified = pPayment.PaymentSourceSpecified;
            if (paymentSourceSpecified)
                paymentSource = new EFS_PaymentSource(pPayment);
            #endregion PaymentSource

            // EG 20230505 [XXXXX] [WI617]
            if (null != paymentDateAdjustment)
            {
                dtEventStartPeriod = new Pair<DateTime, DateTime>(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
                dtEventEndPeriod = new Pair<DateTime, DateTime>(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
            }
            m_ErrLevel = Cst.ErrLevel.SUCCESS;

        }
        #endregion Calc
        #region CalcExpirationDate
        // CAP/FLOOR : premium & additionnalPayment
        // SWAP      : premium & additionnalPayment
        // SWAPTION  : premium
        // ALL       : otherPartyPayment
        private EFS_EventDate CalcExpirationDate(IProduct pProduct)
        {
            EFS_EventDate endDate = null;
            if (null != pProduct)
            {
                #region ExpirationDate
                if (pProduct.ProductBase.IsStrategy)
                {
                    //foreach (IProduct product in pProduct.ProductsStrategy)
                    foreach (IProduct product in ((IStrategy)pProduct).SubProduct)
                    {
                        EFS_EventDate dt = CalcExpirationDate(product);
                        //PL 20100702 PL/FL Correction de bug
                        if (null == endDate)
                        {
                            //1er subproduct
                            if (null != dt)
                                endDate = dt;
                        }
                        else if ((null != dt) && (dt.unadjustedDate.DateValue > endDate.unadjustedDate.DateValue))
                        {
                            //Next subproduct avec Date supérieure 
                            endDate = dt;
                        }
                    }
                }
                else if (pProduct.ProductBase.IsBulletPayment)
                {
                    endDate = new EFS_EventDate();
                    endDate.adjustedDate.DateValue = ((IBulletPayment)pProduct).Payment.PaymentDate.UnadjustedDate.DateValue;
                    endDate.unadjustedDate.DateValue = ((IBulletPayment)pProduct).Payment.PaymentDate.UnadjustedDate.DateValue;
                }
                else if (pProduct.ProductBase.IsFra)
                {
                    endDate = new EFS_EventDate
                    {
                        adjustedDate = ((IFra)pProduct).AdjustedTerminationDate,
                        unadjustedDate = ((IFra)pProduct).AdjustedTerminationDate
                    };
                }
                else if (pProduct.ProductBase.IsFxTermDeposit)
                {
                    endDate = new EFS_EventDate
                    {
                        adjustedDate = ((ITermDeposit)pProduct).MaturityDate,
                        unadjustedDate = ((ITermDeposit)pProduct).MaturityDate
                    };
                }
                else if (pProduct.ProductBase.IsCapFloor)
                    endDate = ((ICapFloor)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsLoanDeposit)
                    endDate = ((ILoanDeposit)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsSwap)
                    endDate = ((ISwap)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsDebtSecurityTransaction)
                    endDate = ((IDebtSecurityTransaction)pProduct).MaxTerminationDate;
                // EG 20101013 Add Test On IsSaleAndRepurchaseAgreement & IsBuyAndSellBack
                else if (pProduct.ProductBase.IsSaleAndRepurchaseAgreement)
                    endDate = ((ISaleAndRepurchaseAgreement)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsBuyAndSellBack)
                    endDate = ((IBuyAndSellBack)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsRepo)
                    endDate = ((IRepo)pProduct).MaxTerminationDate;
                else if (pProduct.ProductBase.IsSwaption)
                    endDate = ((ISwaption)pProduct).Efs_SwaptionDates.EndPeriod;
                else if (pProduct.ProductBase.IsFxDigitalOption)
                    endDate = ((IFxDigitalOption)pProduct).ExpiryDate;
                else if (pProduct.ProductBase.IsFxOptionLeg)
                    endDate = ((IFxOptionLeg)pProduct).Efs_FxSimpleOption.ExpiryDate;
                else if (pProduct.ProductBase.IsFxAverageRateOption)
                    endDate = ((IFxAverageRateOption)pProduct).Efs_FxAverageRateOption.ExpiryDate;
                else if (pProduct.ProductBase.IsFxBarrierOption)
                    endDate = ((IFxBarrierOption)pProduct).Efs_FxBarrierOption.ExpiryDate;
                else if ((pProduct.ProductBase.IsFxLeg) || (pProduct.ProductBase.IsFxSwap))
                {
                    IFxLeg fxleg = null;
                    //
                    if (pProduct.ProductBase.IsFxSwap)
                    {
                        FxSwapContainer fxSwap = new FxSwapContainer((IFxSwap)pProduct);
                        fxleg = fxSwap.GetLastLeg();
                    }
                    else if (pProduct.ProductBase.IsFxLeg)
                    {
                        fxleg = (IFxLeg)pProduct;
                    }
                    //
                    EFS_FxLeg efs_FxLeg = fxleg.Efs_FxLeg;
                    if (efs_FxLeg.deliverableLegSpecified)
                        endDate = efs_FxLeg.deliverableLeg.ValueDate;
                    else if (efs_FxLeg.nonDeliverableLegSpecified)
                        endDate = efs_FxLeg.nonDeliverableLeg.ValueDate;
                }
                else if (pProduct.ProductBase.IsExchangeTradedDerivative)
                {
                    //PL 20100701 PL/FL Add
                    //PL 20200107 [25099] GLOPXXX See EG Add test on efs_ExchangeTradedDerivative
                    if (((IExchangeTradedDerivative)pProduct).Efs_ExchangeTradedDerivative != null)
                        endDate = ((IExchangeTradedDerivative)pProduct).Efs_ExchangeTradedDerivative.MaturityDate;
                }
                else if (pProduct.ProductBase.IsCashBalance)
                {
                    endDate = new EFS_EventDate(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
                }
                else if (pProduct.ProductBase.IsEquitySecurityTransaction)
                {
                    endDate = new EFS_EventDate(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
                }
                else if (pProduct.ProductBase.IsReturnSwap)
                {
                    endDate = new EFS_EventDate(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
                }
                else if (pProduct.ProductBase.IsBondOption)
                {
                    endDate = ((IDebtSecurityOption)pProduct).Efs_BondOption.ExpiryDate;
                }
                else if (pProduct.ProductBase.IsCommoditySpot)
                {
                    endDate = new EFS_EventDate(PaymentDate.unadjustedDate.DateValue, PaymentDate.adjustedDate.DateValue);
                }
                else
                {
                    //PL 20100700 FI Add
                    throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not managed, please contact EFS", pProduct.GetType().FullName));
                }
                #endregion ExpirationDate
            }
            return endDate;
        }
        #endregion CalcExpirationDate
        #region CalcTax
        private void CalcTax(IPayment pPayment)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;

            bool isTaxMustBeGenerated = pPayment.TaxSpecified && paymentSourceSpecified;
            if (isTaxMustBeGenerated)
            {
                #region if Invoicing = true then event with no tax (Invoicing calculation)
                if (Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeInvoicingScheme))
                    isTaxMustBeGenerated = BoolFunc.IsFalse(pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value);
                #endregion if Invoicing = true then event with no tax (Invoicing calculation)
            }
            taxSpecified = isTaxMustBeGenerated;
            #region Tax
            if (taxSpecified)
            {
                ArrayList aTax = new ArrayList();
                foreach (ITax iTax in pPayment.Tax)
                {
                    foreach (ITaxSchedule iTaxDetail in iTax.TaxDetail)
                    {
                        aTax.Add(new EFS_Tax(this, iTax.TaxSource, iTaxDetail));
                    }
                }
                if (0 < aTax.Count)
                    tax = (EFS_Tax[])aTax.ToArray(typeof(EFS_Tax));
            }
            #endregion Tax
            m_ErrLevel = Cst.ErrLevel.SUCCESS;

        }
        #endregion CalcTax
        #region SetEventDatePeriods
        /// <summary>
        /// Alimentation de la période de la ligne de paiement
        /// </summary>
        // EG 20150708 [21103] New
        public void SetEventDatePeriods(DateTime pUnadjustedStartPeriod, DateTime pAdjustedStartPeriod, DateTime pUnadjustedEndPeriod, DateTime pAdjustedEndPeriod)
        {
            dtEventStartPeriod = new Pair<DateTime, DateTime>(pUnadjustedStartPeriod, pAdjustedStartPeriod);
            dtEventEndPeriod = new Pair<DateTime, DateTime>(pUnadjustedEndPeriod, pAdjustedEndPeriod);
            if (originalPaymentSpecified)
            {
                originalPayment.dtEventStartPeriod = new Pair<DateTime, DateTime>(pUnadjustedStartPeriod, pAdjustedStartPeriod);
                originalPayment.dtEventEndPeriod = new Pair<DateTime, DateTime>(pUnadjustedEndPeriod, pAdjustedEndPeriod);
            }
            if (taxSpecified)
            {
                foreach (EFS_Tax _tax in tax)
                {
                    _tax.dtEventStartPeriod = new Pair<DateTime, DateTime>(pUnadjustedStartPeriod, pAdjustedStartPeriod);
                    _tax.dtEventEndPeriod = new Pair<DateTime, DateTime>(pUnadjustedEndPeriod, pAdjustedEndPeriod);
                }
            }
        }
        #endregion SetEventDatePeriods
        #endregion Methods
    }
    #endregion EFS_Payment
    #region EFS_PaymentQuote
    public class EFS_PaymentQuote
    {
        public EFS_Decimal percentageRate;
        public EFS_Decimal notionalAmount;
        public string currency;
        public string paymentRelativeTo;

        public EFS_PaymentQuote() { }
    }
    #endregion EFS_PaymentQuote
    #region EFS_PaymentSource
    public class EFS_PaymentSource
    {
        #region Members
        private Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;

        // FI 20180328 [23871] Add paymentId
        public bool paymentIdSpecified;
        public string paymentId; 
        public bool statusSpecified;
        public SpheresSourceStatusEnum status;
        public bool idFeeMatrixSpecified;
        public int idFeeMatrix;
        public bool idFeeSpecified;
        public int idFee;
        public bool idFeeScheduleSpecified;
        public int idFeeSchedule;
        //PL 20191210 [25099]
        public bool feeScheduleIdentifierSpecified;
        public string feeScheduleIdentifier;
        public bool bracket1Specified;
        public string bracket1;
        public bool bracket2Specified;
        public string bracket2;
        //PL 20191210 [25099] 
        public bool feeScopeSpecified;
        public Cst.FeeScopeEnum feeScope;
        public bool eventTypeSpecified;
        public string eventType;
        public bool paymentTypeSpecified;
        public string paymentType;
        public bool formulaSpecified;
        public string formula;
        public bool formulaValue1Specified;
        public string formulaValue1;
        public bool formulaValue2Specified;
        public string formulaValue2;
        public bool formulaValueBracketSpecified;
        public string formulaValueBracket;
        public bool formulaDCFSpecified;
        public string formulaDCF;
        public bool formulaMinSpecified;
        public string formulaMin;
        public bool formulaMaxSpecified;
        public string formulaMax;
        public bool isFeeInvoicing;
        public bool feePaymentFrequencySpecified;
        public string feePaymentFrequency;
        //PL 20141023 //PL/EG/CC 20141121 string to decimal
        public bool assessmentBasisValue1Specified;
        //public string assessmentBasisValue1;
        public decimal assessmentBasisValue1;
        public bool assessmentBasisValue2Specified;
        //public string assessmentBasisValue2;
        public decimal assessmentBasisValue2;
        //EG 20130911 [18076] Add 
        public bool assessmentBasisDetSpecified;
        public string assessmentBasisDet;
        #endregion Members
        #region Accessors
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion Accessors
        #region Constructors
        public EFS_PaymentSource(IPayment pPayment)
        {
            Calc(pPayment);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        private void Calc(IPayment pPayment)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;

            if (pPayment.PaymentSourceSpecified)
            {
                #region Status
                statusSpecified = pPayment.PaymentSource.StatusSpecified;
                status = pPayment.PaymentSource.Status;
                #endregion Status
                #region IdFeeMatrix
                idFeeMatrixSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeMatrixScheme);
                if (idFeeMatrixSpecified)
                    idFeeMatrix = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId;
                #endregion IdFeeMatrix
                #region IdFee
                idFeeSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeScheme);
                if (idFeeSpecified)
                    idFee = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId;
                #endregion IdFee
                #region IdFeeSchedule
                idFeeScheduleSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeScheduleScheme);
                if (idFeeScheduleSpecified)
                    idFeeSchedule = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId;
                feeScheduleIdentifierSpecified = idFeeScheduleSpecified;
                if (feeScheduleIdentifierSpecified)
                    feeScheduleIdentifier = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).Value;
                #endregion IdFeeSchedule
                #region Bracket1
                bracket1Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedBracket1Scheme);
                if (bracket1Specified)
                    bracket1 = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedBracket1Scheme).Value;
                #endregion Bracket1
                #region Bracket2
                bracket2Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedBracket2Scheme);
                if (bracket2Specified)
                    bracket2 = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedBracket2Scheme).Value;
                #endregion Bracket2
                #region FeeScope
                //PL 20191210 [25099] 
                feeScopeSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedScopeScheme);
                if (feeScopeSpecified)
                {
                    string strFeeScope = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedScopeScheme).Value;
                    if (System.Enum.IsDefined(typeof(Cst.FeeScopeEnum), strFeeScope))
                    {
                        feeScope = (Cst.FeeScopeEnum)System.Enum.Parse(typeof(Cst.FeeScopeEnum), strFeeScope);
                    }
                    else
                    {
                        feeScopeSpecified = false;
                    }
                }
                #endregion FeeScope
                #region EventType
                eventTypeSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeEventTypeScheme);
                if (eventTypeSpecified)
                    eventType = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeEventTypeScheme).Value;
                #endregion EventType

                #region Formula
                formulaSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaScheme);
                if (formulaSpecified)
                    formula = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaScheme).Value;
                #endregion Formula
                #region FormulaValue1
                formulaValue1Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme);
                if (formulaValue1Specified)
                    formulaValue1 = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme).Value;
                #endregion FormulaValue1
                #region FormulaValue2
                //PL 20141017
                formulaValue2Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme);
                if (formulaValue2Specified)
                    formulaValue2 = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme).Value;
                #endregion FormulaValue2

                #region FormulaValueBracket
                formulaValueBracketSpecified = Tools.IsLikePaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket");
                if (formulaValueBracketSpecified)
                {
                    ArrayList aBracketValue = new ArrayList();
                    ISpheresIdSchemeId[] schemes = pPayment.PaymentSource.GetSpheresIdLikeScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + "_Bracket");
                    foreach (ISpheresIdSchemeId scheme in schemes)
                    {
                        aBracketValue.Add("[" + scheme.Scheme.Substring(scheme.Scheme.LastIndexOf("_") + 1) + "]Value1:" + scheme.Value);
                    }

                    schemes = pPayment.PaymentSource.GetSpheresIdLikeScheme(Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme + "_Bracket");
                    // FL 20100423 Add Test null (avec PL)
                    if (schemes != null)
                    {
                        int i = 1;
                        foreach (ISpheresIdSchemeId scheme in schemes)
                        {
                            aBracketValue.Insert(i, "Value2: " + scheme.Value);
                            i += 2;
                        }
                    }
                    // FL 20100423 Init to string.empty (avec PL)
                    formulaValueBracket = string.Empty;
                    foreach (object bracketValue in aBracketValue)
                    {
                        formulaValueBracket += bracketValue.ToString();
                    }
                }
                #endregion FormulaValueBracket
                #region FormulaDCF
                formulaDCFSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaDCFScheme);
                if (formulaDCFSpecified)
                    formulaDCF = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaDCFScheme).Value;
                #endregion FormulaDCF
                #region FormulaMin
                formulaMinSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaMinScheme);
                if (formulaMinSpecified)
                    formulaMin = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaMinScheme).Value;
                #endregion FormulaMin
                #region FormulaMax
                formulaMaxSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedFormulaMaxScheme);
                if (formulaMaxSpecified)
                    formulaMax = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaMaxScheme).Value;
                #endregion FormulaMax
                #region IsFeeInvoicing
                if (Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeInvoicingScheme))
                    isFeeInvoicing = BoolFunc.IsTrue(pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme).Value);
                #endregion IsFeeInvoicing
                #region FeePaymentFrequency
                feePaymentFrequencySpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeePaymentFrequencyScheme);
                if (feePaymentFrequencySpecified)
                    feePaymentFrequency = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeePaymentFrequencyScheme).Value;
                #endregion FeePaymentFrequency

                #region AssessmentBasisValue1/AssessmentBasisValue2
                //PL 20141017 
                assessmentBasisValue1Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme);
                if (assessmentBasisValue1Specified)
                {
                    //PL/EG/CC 20141121 string to decimal
                    //assessmentBasisValue1 = pPayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme).Value;
                    assessmentBasisValue1 = DecFunc.DecValueFromInvariantCulture(pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme).Value);
                }
                assessmentBasisValue2Specified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme);
                if (assessmentBasisValue2Specified)
                {
                    //assessmentBasisValue2 = pPayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme).Value;
                    assessmentBasisValue2 = DecFunc.DecValueFromInvariantCulture(pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme).Value);
                }
                #endregion AssessmentBasisValue1/AssessmentBasisValue2

                #region PaymentType
                paymentTypeSpecified = pPayment.PaymentTypeSpecified;
                if (paymentTypeSpecified)
                    paymentType = pPayment.PaymentType.Value;
                #endregion PaymentType

                #region paymentId
                // FI 20180328 [23871]
                paymentIdSpecified = StrFunc.IsFilled(pPayment.Id);
                if (paymentIdSpecified)
                    paymentId = pPayment.Id;
                #endregion paymentId
                
                //EG 20130911 [18076] Set assessmentBasisDet
                #region PeriodCharacteristics
                assessmentBasisDet = string.Empty;
                assessmentBasisDetSpecified = Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeSchedPeriodCharacteristicsScheme);
                if (assessmentBasisDetSpecified)
                {
                    ISpheresIdSchemeId scheme = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedPeriodCharacteristicsScheme);
                    assessmentBasisDet = scheme.Value;
                }
                #endregion PeriodCharacteristics
            }
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_PaymentSource
    #region EFS_Period
    public class EFS_Period
    {
        #region Members
        public DateTime date1;
        public DateTime date2;
        #endregion Members
        #region Constructors
        public EFS_Period() { }
        public EFS_Period(DateTime pDate1, DateTime pDate2)
        {
            date1 = pDate1;
            date2 = pDate2;
        }
        #endregion Constructors
    }
    #endregion EFS_Period
    #region EFS_PreSettlement
    // EG 20090105 Replace the old version EFS_PreSettlement
    public class EFS_PreSettlement : EFS_AdjustedSettlement, ICloneable
    {
        #region Accessors
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if (DtFunc.IsDateTimeFilled(m_OffsetSettlementDate))
                {
                    string dt = DtFunc.DateTimeToString(m_OffsetSettlementDate, DtFunc.FmtISODate);
                    return new EFS_Date(dt);
                }
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, EFS_Date pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU2, pOffset, pMethod, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU1, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU2, pOffset, PreSettlementDateMethodDeterminationEnum.BothCurrencies.ToString(), pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_PreSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pSettlementDate, string pCU1, string pCU2, IOffset pOffset, string pMethod, DataDocumentContainer pDataDocument)
            : base(pCS, pDbTransaction, pSettlementDate, pCU1, pCU2, pOffset, pMethod, pDataDocument) { }
        #endregion Constructors
        #region ICloneable
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            EFS_PreSettlement clone = new EFS_PreSettlement(this.m_Cs, m_DbTransaction, this.m_SettlementDate, this.m_CU1, this.m_CU2, this.m_Offset, this.m_Method.ToString(), m_DataDocument);
            return clone;
        }
        #endregion ICloneable
    }
    #endregion EFS_PreSettlement

    #region EFS_RollConvention
    public class EFS_RollConvention
    {
        #region Members
        //PL 20130131 New member ErrLevel
        public Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
        public RollConventionEnum rollConvention;
        public DateTime sourceDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceDateSpecified;
        public DateTime effectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool effectiveDateSpecified;
        /// <summary>
        /// Connection string, used only for CO2EMISSIONSFUT,CBOTAGRIOPT and EUREXFIXEDINCOMEOPT roll convention
        /// </summary>
        public string CS;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CSSpecified;
        /// <summary>
        /// Business center identifier, used for CO2EMISSIONSFUT,CBOTAGRIOPT and EUREXFIXEDINCOMEOPT roll convention
        /// </summary>
        public string[] IdBC = null;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IdBCSpecified;

        public DateTime rolledDate;
        #endregion Members
        #region Constructors

        /// <summary>
        /// Get a new roll convention object cusotmized according with the passed roll convention type.
        /// </summary>
        /// <param name="pCS">Connection string, used only by the CO2EMISSIONSFUT,CBOTAGRIOPT or EUREXFIXEDINCOMEOPT rolling convention, in this case must be NOT null nor empty</param>
        /// <param name="pRollConvention">Roll convention</param>
        /// <param name="pSourceDate">Reference date, used by the rolling procedure, MUST be a valid date value</param>
        /// <param name="pEffectiveDate">Effective date, used only by the FRN Rolling convention
        /// Used only by the CBOTAGRIOPT or EUREXFIXEDINCOMEOPT rolling convention</param>
        /// <param name="pIdBC">Business center, if null the date will be computed considering no business centers. 
        /// Used only by the CO2EMISSIONSFUT,CBOTAGRIOPT or EUREXFIXEDINCOMEOPT rolling convention</param>
        /// <returns>A new roll convention object</returns>
        /// <exception cref="ArgumentException">when pSourceDate (reference date) is not a valid date, 
        /// or pRollConvention equals to CBOTAGRIOPT/EUREXFIXEDINCOMEOPT and pCS (connection string) is null or empty</exception>
        //public static EFS_RollConvention GetNewRollConvention(string pCS, RollConventionEnum pRollConvention, DateTime pSourceDate, DateTime pEffectiveDate, string pIdBC)
        public static EFS_RollConvention GetNewRollConvention(string pCS, RollConventionEnum pRollConvention, DateTime pSourceDate, DateTime pEffectiveDate, string[] pIdBC)
        {
            if (pSourceDate <= DateTime.MinValue || pSourceDate >= DateTime.MaxValue)
            {
                throw new ArgumentException("Source date must be a valid date value", "pSourceDate");
            }

            EFS_RollConvention instance;
            switch (pRollConvention)
            {
                //case RollConventionEnum.ICEUCO2EMISSIONSFUT:
                case RollConventionEnum.CO2EMISSIONSFUT:
                case RollConventionEnum.CBOTAGRIOPT:
                case RollConventionEnum.EUREXFIXEDINCOMEOPT:
                    if (String.IsNullOrEmpty(pCS))
                    {
                        string errMsg = String.Format("Connection string is mandatory on {0} rolling convention", pRollConvention.ToString());
                        throw new ArgumentException(errMsg, "pCS");
                    }

                    instance = new EFS_RollConvention(pCS, pRollConvention, pSourceDate, pIdBC);
                    break;

                default:
                    instance = new EFS_RollConvention(pRollConvention, pSourceDate, pEffectiveDate);
                    break;
            }

            return instance;
        }

        public static EFS_RollConvention GetNewRollConvention(RollConventionEnum pRollConvention, DateTime pSourceDate, DateTime pEffectiveDate)
        {
            return GetNewRollConvention(null, pRollConvention, pSourceDate, pEffectiveDate, null);
        }

        private EFS_RollConvention(RollConventionEnum pRollConvention, DateTime pSourceDate, DateTime pEffectiveDate)
        {
            this.rollConvention = pRollConvention;
            this.sourceDate = pSourceDate;
            this.sourceDateSpecified = (pSourceDate != Convert.ToDateTime(null));
            this.effectiveDate = pEffectiveDate;
            this.effectiveDateSpecified = (pEffectiveDate != Convert.ToDateTime(null));
            Calc();
        }
        /// <summary>
        /// Specific roll convention (eg. "CBOT Agricultural Options")
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pRollConvention">Roll Convention</param>
        /// <param name="pSourceDate">Starting date to compute the last Friday which precedes by at least two business days the last business day of the month</param>
        /// <param name="pIdBC">Business center, if null the date will be computed considering no business centers</param>
        private EFS_RollConvention(string pCS, RollConventionEnum pRollConvention, DateTime pSourceDate, string[] pIdBC)
        {
            this.CS = pCS;
            this.CSSpecified = true;

            this.rollConvention = pRollConvention;

            this.sourceDate = pSourceDate;
            this.sourceDateSpecified = (pSourceDate != Convert.ToDateTime(null));

            //if (!String.IsNullOrEmpty(pIdBC))
            if (pIdBC != null && pIdBC.Length > 0)
            {
                this.IdBC = pIdBC;
                this.IdBCSpecified = true;
            }

            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void Calc()
        {
            this.errLevel = Cst.ErrLevel.UNDEFINED;

            DateTime rolledDate = Convert.ToDateTime(null);
            if (this.sourceDateSpecified)
            {
                this.errLevel = Cst.ErrLevel.SUCCESS;

                rolledDate = this.sourceDate;
                int lastDayRolledDate = DateTime.DaysInMonth(rolledDate.Year, rolledDate.Month);

                switch (this.rollConvention)
                {
                    case RollConventionEnum.NONE:
                        break;
                    case RollConventionEnum.MON:
                    case RollConventionEnum.TUE:
                    case RollConventionEnum.WED:
                    case RollConventionEnum.THU:
                    case RollConventionEnum.FRI:
                    case RollConventionEnum.SAT:
                    case RollConventionEnum.SUN:
                        //On se regarde si le jour de la date correspond à la RollConv sinon on avance jusqu'à trouver ce jour, sans toutefois changer de mois.
                        this.errLevel = Cst.ErrLevel.DATAUNMATCH;
                        string firtsThreeCharOfDay = this.rollConvention.ToString();
                        for (int i = 0; i < 7; i++)
                        {
                            if (rolledDate.DayOfWeek.ToString().ToUpper().StartsWith(firtsThreeCharOfDay))
                            {
                                //Ctrl si le jour correspond à la RollConv
                                this.errLevel = Cst.ErrLevel.SUCCESS;
                                break;
                            }
                            else if (rolledDate.Day < lastDayRolledDate)
                            {
                                //Ajout d'un jour tant que l'on reste sur le même mois
                                rolledDate = rolledDate.AddDays(1);
                            }
                        }
                        break;
                    case RollConventionEnum.EOM:
                        rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, lastDayRolledDate);
                        break;
                    case RollConventionEnum.FOY:
                        //First Of Year
                        rolledDate = new DateTime(rolledDate.Year, 1, 1);
                        break;
                    case RollConventionEnum.FOQ:
                        //First Of Quaterly
                        //Ex.: Sur une une échéance Jun(M) on retourne le 1er Avril
                        rolledDate = new DateTime(rolledDate.Year, rolledDate.Month - 2, 1);
                        break;
                    case RollConventionEnum.FRN:
                        if (this.effectiveDateSpecified)
                        {
                            int day = this.effectiveDate.Day;
                            if ((day == DateTime.DaysInMonth(this.effectiveDate.Year, this.effectiveDate.Month)) ||
                                (day > DateTime.DaysInMonth(rolledDate.Year, rolledDate.Month)))
                                rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, lastDayRolledDate);
                            else
                                rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, day);
                        }
                        break;
                    case RollConventionEnum.IMM:
                        rolledDate = Tools.GetRankDayOfWeek(rolledDate, 3, DayOfWeek.Wednesday);
                        break;
                    case RollConventionEnum.IMMCAD:
                        rolledDate = Tools.GetRankDayOfWeek(rolledDate, 3, DayOfWeek.Wednesday); // Error
                        break;
                    case RollConventionEnum.TBILL:
                        rolledDate = rolledDate.AddDays(DayOfWeek.Monday - rolledDate.DayOfWeek);
                        break;
                    case RollConventionEnum.SFE:
                        rolledDate = Tools.GetRankDayOfWeek(rolledDate, 2, DayOfWeek.Friday);
                        break;
                    case RollConventionEnum.FIRSTMON://PL 20160225 F&Oml
                    case RollConventionEnum.SECONDMON://PL 20160225 F&Oml
                    case RollConventionEnum.THIRDMON://PL 20100325 F&Oml
                    case RollConventionEnum.FOURTHMON://PL 20160225 F&Oml
                    case RollConventionEnum.FIFTHMON://PL 20160225 F&Oml
                    case RollConventionEnum.FIRSTTUE:
                    case RollConventionEnum.SECONDTUE:
                    case RollConventionEnum.THIRDTUE:
                    case RollConventionEnum.FOURTHTUE:
                    case RollConventionEnum.FIFTHTUE:
                    case RollConventionEnum.FIRSTWED:
                    case RollConventionEnum.SECONDWED:
                    case RollConventionEnum.THIRDWED:
                    case RollConventionEnum.FOURTHWED:
                    case RollConventionEnum.FIFTHWED:
                    case RollConventionEnum.FIRSTTHU:
                    case RollConventionEnum.SECONDTHU://RD 20110606 [17474]
                    case RollConventionEnum.THIRDTHU://RD 20110606 [17474]
                    case RollConventionEnum.FOURTHTHU://RD 20110606 [17474]
                    case RollConventionEnum.FIFTHTHU:
                    case RollConventionEnum.FIRSTFRI://PL 20130123 F&Oml
                    case RollConventionEnum.SECONDFRI://PL 20130123 F&Oml
                    case RollConventionEnum.THIRDFRI://PL 20100325 F&Oml
                    case RollConventionEnum.FOURTHFRI://PL 20130123 F&Oml
                    case RollConventionEnum.FIFTHFRI://PL 20130123 F&Oml
                    case RollConventionEnum.SECONDLASTMON:
                    case RollConventionEnum.SECONDLASTTUE:
                    case RollConventionEnum.SECONDLASTWED:
                    case RollConventionEnum.SECONDLASTTHU:
                    case RollConventionEnum.SECONDLASTFRI:
                        // FI 20240523 [26625][WI797] Refactoring pour gérer SECONDLASTFRI (le code prévoir de gérer n'importe quelle valeur si besoin , exemple THIRDLASTWED ) 
                        XmlEnumAttribute att = ReflectionTools.GetAttribute<XmlEnumAttribute>(rollConvention);
                        if (null == att)
                            throw new InvalidOperationException($"rollConvention: {rollConvention}. XmlEnumAttribute not specified");

                        int currentMonth = rolledDate.Month;
                        int rank = Convert.ToInt32(att.Name.Substring(0, 1)); //1,2,3,4,5
                        DayOfWeekEnum dayOfWeekEnum;
                        if (att.Name.Contains("LAST"))
                        {
                            //MON => Monday , TUE => Tuesday, WED => Wednesday, THU => Thursday, FRI => Friday
                            dayOfWeekEnum = ReflectionTools.ConvertStringToEnum<DayOfWeekEnum>(att.Name.Substring( att.Name.IndexOf("LAST")+ "LAST".Length, 3));
                            rolledDate = Tools.GetRankLastDayOfWeek(rolledDate, rank, (DayOfWeek)System.Enum.Parse(typeof(DayOfWeek), dayOfWeekEnum.ToString()));
                        }
                        else
                        {
                            //MON => Monday , TUE => Tuesday, WED => Wednesday, THU => Thursday, FRI => Friday
                            dayOfWeekEnum = ReflectionTools.ConvertStringToEnum<DayOfWeekEnum>(att.Name.Substring(3, 3)); //MON => Monday , TUE => Tuesday, WED => Wednesday, THU => Thursday, FRI => Friday
                            rolledDate = Tools.GetRankDayOfWeek(rolledDate, rank, (DayOfWeek)System.Enum.Parse(typeof(DayOfWeek), dayOfWeekEnum.ToString()));
                        }

                        if (rolledDate.Month != currentMonth)
                        {
                            //Changement de mois: le mois courant ne comporte pas de 5ème lundi, mardi etc. (NB: En théorie c'est GetRankDayOfWeek() qui devrait tester cela, mais ce n'est pas le cas...)
                            this.errLevel = Cst.ErrLevel.DATAUNMATCH;
                        }
                        break;
                    case RollConventionEnum.LASTMON: //LP 20240522 [WI933]
                    case RollConventionEnum.LASTTUE: //LP 20240522 [WI933]
                    case RollConventionEnum.LASTWED: //LP 20240522 [WI933]
                    case RollConventionEnum.LASTTHU: //LP 20240522 [WI933]
                    case RollConventionEnum.LASTFRI: //LP 20240522 [WI933]
                        XmlEnumAttribute attr = ReflectionTools.GetAttribute<XmlEnumAttribute>(rollConvention);
                        if (null == attr)
                            throw new InvalidOperationException($"rollConvention: {rollConvention}. XmlEnumAttribute not specified");

                        DayOfWeekEnum dayOfWeek = ReflectionTools.ConvertStringToEnum<DayOfWeekEnum>(attr.Name.Substring(4, 3));

                        rolledDate = Tools.GetLastDayOfWeekOfMonth(rolledDate, (DayOfWeek)System.Enum.Parse(typeof(DayOfWeek), dayOfWeek.ToString()));
                        break;
                    //case RollConventionEnum.ICEUCO2EMISSIONSFUT:
                    case RollConventionEnum.CO2EMISSIONSFUT:
                        #region Initialize
                        IProductBase product = Tools.GetNewProductBase();
                        IBusinessDayAdjustments bdaOffset = product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE, this.IdBC);
                        EFS_BusinessCenters businessCenters = new EFS_BusinessCenters(this.CS, bdaOffset, null);
                        #endregion

                        //Last Monday of the contract month 
                        DateTime computedDate = Tools.GetLastDayOfWeekOfMonth(rolledDate, DayOfWeek.Monday);

                        bool isValidMonday = false;
                        do
                        {
                            //Check if this Monday is a Non-Business Day
                            if (!businessCenters.IsHoliday(computedDate, DayTypeEnum.ExchangeBusiness))
                            {
                                //Check if there is a Non-Business Day in the 4 days following the Monday
                                int addDays = 4;
                                _ = product.CreateExchangeTradedDerivative();
                                IOffset offset = product.CreateOffset(PeriodEnum.D, addDays, DayTypeEnum.ExchangeBusiness);
                                EFS_Offset efsOffset = new EFS_Offset(this.CS, offset, computedDate, bdaOffset, false, null);
                                if (ArrFunc.IsFilled(efsOffset.offsetDate))
                                {
                                    //OK si l'ajout de 4 jours calendaires est identique à l'ajout de 4 jours ouvrés
                                    isValidMonday = (efsOffset.offsetDate[0] == computedDate.AddDays(addDays));
                                }
                            }

                            //The Monday is a Non-Business Day or there is a Non-Business Day in the 4 days following the Monday
                            if (!isValidMonday)
                            {
                                computedDate = computedDate.AddDays(-7);
                            }

                            #region Error
                            //On considèrera (subjectivement) être en erreur s'il y a changement de mois ! 
                            if ((!isValidMonday) && (computedDate.Month != rolledDate.Month))
                            {
                                computedDate = DateTime.MinValue;
                                break;
                            }
                            #endregion
                        }
                        while (!isValidMonday);

                        rolledDate = computedDate;
                        break;
                    case RollConventionEnum.CBOTAGRIOPT:
                        //Utilisation de "2" jours ouvrés (en dur pour cette Roll Convention: CBOTAGRIOPT)
                        rolledDate = Tools.GetLastFridayWithBusinessDayOffset(this.CS, rolledDate, 2, DayTypeEnum.Business, this.IdBC, true);
                        break;
                    case RollConventionEnum.EUREXFIXEDINCOMEOPT:
                        //Utilisation de "2" jours ouvrés (en dur pour cette Roll Convention: EUREXFIXEDINCOMEOPT)
                        //PL 20131121
                        //rolledDate = Tools.GetLastFridayWithBusinessDayOffset(this.CS, rolledDate, 2, this.IdBC, true);
                        rolledDate = Tools.GetLastFridayWithBusinessDayOffset(this.CS, rolledDate, 2, DayTypeEnum.ScheduledTradingDay, this.IdBC, true);
                        break;
                    case RollConventionEnum.IDEXDLV:
                        //EOM sauf au mois de décembre où on retourne le 1er Janvier de l'année suivante (en dur pour cette Roll Convention: IDEXDLV)
                        if (rolledDate.Month == 12)
                        {
                            rolledDate = new DateTime(rolledDate.Year + 1, 1, 1);
                        }
                        else
                        {
                            rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, lastDayRolledDate);
                        }
                        break;
                    case RollConventionEnum.EOMMINUS1EXCEPTMON:
                        //PL 20160226 Newness
                        rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, lastDayRolledDate);
                        if (rolledDate.DayOfWeek != DayOfWeek.Monday)
                            rolledDate = rolledDate.AddDays(-1);
                        break;
                    case RollConventionEnum.WEDNEAR15:
                        rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, 15);
                        if (rolledDate.DayOfWeek != DayOfWeek.Wednesday)
                            rolledDate = rolledDate.AddDays((int)DayOfWeek.Wednesday - (int)rolledDate.DayOfWeek);
                        break;
                    default: // DAY1..DAY31
                        int rollDay = StringToEnum.DayRollConvention(this.rollConvention);
                        rolledDate = new DateTime(rolledDate.Year, rolledDate.Month, System.Math.Min(rollDay, lastDayRolledDate));
                        break;
                }
            }
            if (this.errLevel == Cst.ErrLevel.SUCCESS)
            {
                this.rolledDate = rolledDate;
            }
            else
            {
                this.rolledDate = Convert.ToDateTime(null);
            }
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_RollConvention
    #region EFS_Round
    public class EFS_Round
    {
        #region Members
        private Cst.RoundingDirectionSQL _roundDirSQL;
        private RoundingDirectionEnum _roundDirFpML;
        private int _roundPrec;
        private decimal _amount;
        private decimal _amountRounded;
        public bool IsAbs = true;
        #endregion Members
        #region Constructors
        // RoundDirSQL
        public EFS_Round() : this(Cst.RoundingDirectionSQL.N, 0, 0) { }
        public EFS_Round(string pRoundDirSQL, int pRoundPrec) : this(pRoundDirSQL, pRoundPrec, 0) { }
        public EFS_Round(string pRoundDirSQL, int pRoundPrec, decimal pAmount)
            : this((Cst.RoundingDirectionSQL)System.Enum.Parse(typeof(Cst.RoundingDirectionSQL), pRoundDirSQL), pRoundPrec, pAmount) { }
        public EFS_Round(Cst.RoundingDirectionSQL pRoundDirSQL, int pRoundPrec) : this(pRoundDirSQL, pRoundPrec, 0) { }
        public EFS_Round(Cst.RoundingDirectionSQL pRoundDirSQL, int pRoundPrec, decimal pAmount)
        {
            _amount = pAmount;
            _roundPrec = pRoundPrec;
            RoundDirSQL = pRoundDirSQL;
        }
        // RoundDirFpML
        public EFS_Round(RoundingDirectionEnum pRoundDirFpML, int pRoundPrec) : this(pRoundDirFpML, pRoundPrec, 0) { }
        public EFS_Round(RoundingDirectionEnum pRoundDirFpML, int pRoundPrec, decimal pAmount)
        {
            _amount = pAmount;
            _roundPrec = pRoundPrec;
            RoundDirFpML = pRoundDirFpML;
        }
        // Rounding (FpML)
        public EFS_Round(IRounding pRounding) : this(pRounding, 0) { }
        public EFS_Round(IRounding pRounding, decimal pAmount)
        {
            _amount = pAmount;
            _roundPrec = pRounding.Precision;
            RoundDirFpML = pRounding.RoundingDirection;
        }
        // Currency
        public EFS_Round(string pSource, string pIso4217) : this(pSource, pIso4217, 0) { }
        public EFS_Round(string pSource, string pIso4217, decimal pAmount)
        {
            _amount = pAmount;
            SQL_Currency sql_Currency = new SQL_Currency(pSource, SQL_Currency.IDType.Iso4217, pIso4217);
            _roundPrec = sql_Currency.RoundPrec;
            RoundDirSQL = sql_Currency.RoundingDirectionSQL;
        }
        #endregion Constructors
        #region Accessors
        #region Amount
        public decimal Amount
        {
            set
            {

                _amount = value;

            }
            get
            {

                return _amount;

            }
        }
        #endregion Amount
        #region AmountRounded
        public decimal AmountRounded
        {
            get
            {

                Rounded();
                return _amountRounded;

            }
        }
        #endregion AmountRounded
        #region Debug
        public string Debug
        {
            get
            {

                string tmp = "RoundPrec: " + RoundPrec.ToString() + " ";
                tmp += "RoundDir: " + RoundDirSQL.ToString() + " (" + RoundDirFpML.ToString() + ") ";
                tmp += "Amount: " + Amount.ToString() + " ";
                tmp += "AmountRounded: " + AmountRounded.ToString() + " ";
                tmp += "IsRounded: " + IsRounded.ToString();
                return tmp;

            }
        }
        #endregion Debug
        #region IsRounded
        public bool IsRounded
        {
            get
            {

                return (0 != Decimal.Compare(Amount, AmountRounded));

            }
        }
        #endregion IsRounded
        #region RoundDirSQL
        public Cst.RoundingDirectionSQL RoundDirSQL
        {
            set
            {

                _roundDirSQL = value;
                _roundDirFpML = (RoundingDirectionEnum)((int)value);

            }
            get
            {

                return _roundDirSQL;

            }
        }
        #endregion RoundDirSQL
        #region RoundDirFpML
        public RoundingDirectionEnum RoundDirFpML
        {
            set
            {

                _roundDirFpML = value;
                RoundDirSQL = (Cst.RoundingDirectionSQL)((int)value);

            }
            get
            {

                return _roundDirFpML;

            }
        }
        #endregion RoundDirFpML
        #region RoundPrec
        public int RoundPrec
        {
            set
            {

                _roundPrec = value;

            }
            get
            {

                return _roundPrec;

            }
        }
        #endregion RoundPrec
        #endregion Accessors
        #region Methods
        #region Rounded
        // EG 20130315 Ajout du paramètre MidpointRounding.AwayFromZero pour être en phase avec Arrondi Nearest de type FpML.
        // Exemples:
        // Decimal.round(3.45, 1, MidpointRounding.AwayFromZero) = 3.5
        // Decimal.round(3.45, 1, MidpointRounding.ToEven) = 3.4
        // Decimal.round(3.55, 1, MidpointRounding.AwayFromZero) = 3.6
        // Decimal.round(3.55, 1, MidpointRounding.ToEven) = 3.6
        private void Rounded()
        {

            _amountRounded = 0;
            //
            decimal pow;
            decimal tmpAmount = Amount;
            bool IsNegate = (-1 == System.Math.Sign(Amount));
            if (IsAbs && IsNegate)
                tmpAmount = Decimal.Negate(Amount);
            //
            switch (RoundDirFpML)
            {
                case RoundingDirectionEnum.Down:
                    pow = (decimal)System.Math.Pow(10, RoundPrec);
                    _amountRounded = Decimal.Truncate(tmpAmount * pow) / pow;
                    break;
                case RoundingDirectionEnum.Up:
                    pow = (decimal)System.Math.Pow(10, RoundPrec);
                    decimal poweredValue = tmpAmount * pow;
                    decimal downValue = Decimal.Truncate(poweredValue);
                    if (downValue - poweredValue == 0)
                        _amountRounded = downValue / pow;
                    else
                        _amountRounded = (downValue + 1) / pow;
                    break;
                case RoundingDirectionEnum.Nearest:
                    _amountRounded = Decimal.Round(tmpAmount, RoundPrec, MidpointRounding.AwayFromZero);
                    break;
                case RoundingDirectionEnum.HalfDown:

                    _amountRounded = HalfDown(tmpAmount, RoundPrec);

                    break;
                default:
                    _amountRounded = tmpAmount;
                    break;
            }
            //
            if (IsNegate)
                _amountRounded = Decimal.Negate(_amountRounded);

        }

        /// <summary>
        /// Rounding mode to round towards "nearest neighbor" unless both neighbors are equidistant, in which case round down. 
        /// Behaves as ceiling if the discarded fraction is > 0.5; otherwise, behaves as floor. 
        /// </summary>
        /// <param name="pTmpAmount">the amount to be rounded</param>
        /// <param name="pRoundPrec">the rounding precision</param>
        /// <returns>the rounded amount at the giben precision</returns>
        /// <exception cref="ArgumentException">
        /// throws an eception when the given amount is negative or when the given precision is lesser than 0
        /// </exception>
        private decimal HalfDown(decimal pTmpAmount, int pRoundPrec)
        {
            if (pTmpAmount < 0)
            {
                throw new ArgumentException("Trying to round a negative amount, only positive amounts are accepted", "pTmpAmount");
            }

            if (pRoundPrec < 0)
            {
                throw new ArgumentException("Trying to round with a precision lesser than 0", "pTmpAmount");
            }

            decimal precisionFactor = (decimal)System.Math.Pow(10, pRoundPrec);
            decimal pTmpAmountExp = precisionFactor * pTmpAmount;
            decimal decimalPart = pTmpAmountExp - System.Math.Floor(pTmpAmountExp);

            if (decimalPart > 0)
            {
                if (decimalPart <= (decimal)0.5)
                {
                    pTmpAmountExp = System.Math.Floor(pTmpAmountExp);
                }
                else if (decimalPart > (decimal)0.5)
                {
                    pTmpAmountExp = System.Math.Ceiling(pTmpAmountExp);
                }

                pTmpAmount = pTmpAmountExp / precisionFactor;
            }

            return pTmpAmount;
        }

        #endregion Rounded
        #endregion Methods
    }
    #endregion EFS_Round

    #region EFS_SettlementInfoEntity
    /// <summary>
    ///     Gives infos about the entity of a couple of party use by Pre-Settlement/UsanceDelay calculation dates 
    ///     <para>
    ///     . Accounting currency
    ///     </para>
    ///     <para>
    ///     . PreSettlement method
    ///     </para>
    /// </summary>
    // EG 20150706 [21021] Nullable<int> (m_IdB_Payer|m_IdB_Receiver) and Remove (m_IdB_PayerSpecified|m_IdB_ReceiverSpecified)
    // EG 20180307 [23769] Gestion dbTransaction
    public class EFS_SettlementInfoEntity
    {
        #region Members
        private readonly string m_Cs;
        private readonly IDbTransaction m_DbTransaction;
        /// <summary>
        /// Représente la devise du flux
        /// </summary>
        private readonly string m_Currency;
        /// <summary>
        /// Représente le book du payer
        /// </summary>
        private readonly int? m_IdB_Payer;

        /// <summary>
        /// Représente le book du receiver
        /// </summary>
        private readonly int? m_IdB_Receiver;
        /// <summary>
        /// Représente l'entity
        /// </summary>
        private int m_IdA_Entity;
        /// <summary>
        /// Représente de devise de compta de l'entity
        /// </summary>
        private string m_IdCAccount_Entity;
        /// <summary>
        /// Drapeau qui indique si le flux de pré-settlement est a nécessaire ou si le delai d'usance est à appliquer
        /// </summary>
        private bool m_IsUsePreSettlement;
        /// <summary>
        /// Représente la méthode pour obtenir la date du flux de pré-settlement
        /// </summary>
        private string m_PreSettlementMethod;



        /// <summary>
        /// Offset pour calculer la date du flux de pré-settlement ou la date après application du delai d'usance
        /// </summary>
        private readonly IOffset m_Offset;
        private bool m_OffsetSpecified;

        #endregion Members
        #region Accessors
        #region Currency
        public string Currency
        {
            get { return m_Currency; }
        }
        #endregion Currency
        #region IsUsePreSettlement
        /// <summary>
        /// Obtient true si le flux de pre-settlement est nécessaire ou si l'aplication d'un delai d'usance est nécessaire
        /// </summary>
        public bool IsUsePreSettlement
        {
            get { return m_IsUsePreSettlement; }
        }
        #endregion IsUsePreSettlement

        #region OffsetPreSettlement
        /// <summary>
        /// Obtient l'offset (periodMultiplier est nécessairement négatif)
        /// </summary>
        // EG/CC 20100506: Ticket 16982
        // l'offset est déjà signé à l'appel de cet accessor (verrue temporaire Math.Abs)
        public IOffset OffsetPreSettlement
        {
            get
            {
                IOffset offset = Offset;
                if (null != offset)
                {
                    offset.PeriodMultiplier.IntValue = Math.Abs(offset.PeriodMultiplier.IntValue);
                    offset.PeriodMultiplier.IntValue *= -1;
                }
                return offset;
            }
        }
        #endregion OffsetPreSettlement
        #region Offset
        /// <summary>
        /// Obtient l'offset
        /// </summary>
        public IOffset Offset
        {
            get { return m_OffsetSpecified ? m_Offset : null; }
        }
        #endregion Offset
        #region OffsetUsanceDelay
        /// <summary>
        /// Obtient Offset (identique à la property Offset)
        /// </summary>
        public IOffset OffsetUsanceDelay
        {
            get { return Offset; }
        }
        #endregion OffsetUsanceDelay
        #region PreSettlementMethod
        public string PreSettlementMethod
        {
            get { return m_PreSettlementMethod; }
        }
        #endregion PreSettlementMethod
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pIdB_Payer"></param>
        /// <param name="pIdB_Receiver"></param>
        /// <param name="pDefaultOffset"></param>
        // EG 20150706 [21021] Nullable<int> for pIdB_Payer|pIdB_Receiver
        // EG 20150706 [21021] Remove pIdA_Payer|pIdA_Receiver
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, string pCurrency, Nullable<int> pIdB_Payer, Nullable<int> pIdB_Receiver, IOffset pDefaultOffset) :
            this(pCS, null, pCurrency, pIdB_Payer, pIdB_Receiver, pDefaultOffset) { }

        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, IDbTransaction pDbTransaction, string pCurrency, Nullable<int> pIdB_Payer, Nullable<int> pIdB_Receiver, IOffset pDefaultOffset)
        {

            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;

            m_Currency = pCurrency;

            m_IdB_Payer = pIdB_Payer;
            m_IdB_Receiver = pIdB_Receiver;

            m_Offset = pDefaultOffset;
            m_OffsetSpecified = (null != m_Offset);

            SearchEntity();

            SetIsWithSettlement();

            if (StrFunc.IsFilled(m_Currency))
                SetOffset();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPayer"></param>
        /// <param name="pReceiver"></param>
        /// <param name="pDocument"></param>
        /// <param name="pDefaultOffset"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, string pPayer, string pReceiver, DataDocumentContainer pDocument, IOffset pDefaultOffset)
            : this(pCS, null, string.Empty, pPayer, pReceiver, pDocument, pDefaultOffset) { }
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, IDbTransaction pDbTransaction, string pPayer, string pReceiver, DataDocumentContainer pDocument, IOffset pDefaultOffset)
            : this(pCS, pDbTransaction, string.Empty, pPayer, pReceiver, pDocument, pDefaultOffset) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pPayer"></param>
        /// <param name="pReceiver"></param>
        /// <param name="pDocument"></param>
        /// <param name="pDefaultOffset"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, string pCurrency, string pPayer, string pReceiver, DataDocumentContainer pDocument, IOffset pDefaultOffset) :
            this(pCS, null, pCurrency, pPayer, pReceiver, pDocument, pDefaultOffset){}
        // EG 20180307 [23769] Gestion dbTransaction
        public EFS_SettlementInfoEntity(string pCS, IDbTransaction pDbTransaction, string pCurrency, string pPayer, string pReceiver, DataDocumentContainer pDocument, IOffset pDefaultOffset)
        {

            m_Cs = pCS;
            m_DbTransaction = pDbTransaction;

            m_Currency = pCurrency;

            #region Payer
            //IParty payer = pDocument.GetParty(pPayer);
            //if (null != payer)
            //    m_IdB_Payer = pDocument.GetOTCmlId_Book(payer.id);
            // EG 20150706 [21021] 
            m_IdB_Payer = pDocument.GetOTCmlId_Book(pPayer);
            //m_IdB_PayerSpecified = (m_IdB_Payer.HasValue);
            #endregion Payer

            #region Receiver
            //IParty receiver = pDocument.GetParty(pReceiver);
            //if (null != receiver)
            //    m_IdB_Receiver = pDocument.GetOTCmlId_Book(receiver.id);
            // EG 20150706 [21021] 
            m_IdB_Receiver = pDocument.GetOTCmlId_Book(pReceiver);
            //m_IdB_ReceiverSpecified = (m_IdB_Receiver.HasValue);
            #endregion Receiver

            m_Offset = pDefaultOffset;
            m_OffsetSpecified = (null != m_Offset);

            SearchEntity();

            SetIsWithSettlement();

            if (StrFunc.IsFilled(m_Currency))
                SetOffset();

        }
        #endregion Constructors
        #region Methods
        #region SearchEntity
        /// <summary>
        /// Recherche l'entité du payer, et ensuite de receiver (si le payer n'est pas rattaché à une entité)
        /// </summary>
        // EG 20150706 [21021] 
        private void SearchEntity()
        {
            if (m_IdB_Payer.HasValue)
                GetInfoEntity(m_IdB_Payer.Value);
            if (m_IdB_Receiver.HasValue && (m_IdA_Entity <= 0))
                GetInfoEntity(m_IdB_Receiver.Value);

        }
        #endregion SearchEntity
        #region GetInfoEntity
        /// <summary>
        /// Recherche l'entité, sa devise de compta et sa méthode de calcul de la date de presettlement
        /// </summary>
        /// <param name="pIdB"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20200423 [XXXXX] Mise en cache 
        private void GetInfoEntity(int pIdB)
        {

            SQL_Book book = new SQL_Book(CSTools.SetCacheOn(m_Cs), pIdB)
            {
                DbTransaction = m_DbTransaction
            };
            if (book.IsLoaded)
            {
                if (0 < book.IdA_Entity)
                {
                    SQL_Actor actor = new SQL_Actor(CSTools.SetCacheOn(m_Cs), book.IdA_Entity)
                    {
                        DbTransaction = m_DbTransaction,
                        WithInfoEntity = true
                    };
                    if (actor.IsLoaded)
                    {
                        m_IdA_Entity = book.IdA_Entity;
                        m_IdCAccount_Entity = actor.IdCAccount;
                        m_PreSettlementMethod = actor.PreSettlementMethod;
                    }
                }
            }

        }
        #endregion GetInfoEntity
        #region SetOffset
        /// <summary>
        /// Recherche l'asset Change entre la devise du flux et la devise de compta
        /// <para>Alimente m_offset avec le décalage paramétré sur l'asset de change</para>
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        private void SetOffset()
        {

            if (m_IsUsePreSettlement)
            {
                KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
                {
                    IdC1 = m_Currency,
                    IdC2 = m_IdCAccount_Entity,
                    QuoteBasisSpecified = false
                };
                int idAsset = keyAssetFxRate.GetIdAsset(m_Cs, m_DbTransaction);
                if (idAsset > 0)
                    SetOffset(idAsset);
            }

        }
        /// <summary>
        /// Recherche l'asset Change entre  2 devises
        /// <para>Alimente m_offset avec le décalage paramétré sur l'asset de change</para>
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public void SetOffset(string pIdC1, string pIdC2)
        {

            KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
            {
                IdC1 = pIdC1,
                IdC2 = pIdC2,
                QuoteBasisSpecified = false
            };
            int idAsset = keyAssetFxRate.GetIdAsset(m_Cs, m_DbTransaction);
            if (idAsset > 0)
                SetOffset(idAsset);

        }
        // EG 20180307 [23769] Gestion dbTransaction
        public void SetOffset(int pIdAsset)
        {
            SQL_AssetFxRate asset = new SQL_AssetFxRate(m_Cs, pIdAsset, SQL_Table.ScanDataDtEnabledEnum.Yes)
            {
                DbTransaction = m_DbTransaction
            };
            if (asset.IsLoaded)
            {
                m_Offset.DayType = DayTypeEnum.Business;
                m_Offset.DayTypeSpecified = true;
                m_Offset.Period = StringToEnum.Period(asset.PeriodSettlementTerm);
                m_Offset.PeriodMultiplier.IntValue = System.Math.Abs(asset.PeriodMultiplierSettlementTerm); // *-1;
            }
            m_OffsetSpecified = (null != m_Offset);

        }
        #endregion SetOffset
        #region SetIsWithSettlement
        /// <summary>
        /// Applique les règles de Spheres® qui indique s'il le flux de pre-settlement est à calculer ou si le delai d'usance est à appliquer
        /// <para>
        /// Affecte le membre m_IsWithPreSettlement 
        /// </para>
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190603 [24683] Use Datatable instead of DataReader
        // EG 20200423 [XXXXX] Mise en cache 
        private void SetIsWithSettlement()
        {
            bool isPreSettlementOnAllFlows = false;
            string sqlQuery = @"select ISPRSONALLFLOWS from dbo.EFSSOFTWARE where IDEFSSOFTWARE = " + DataHelper.SQLString(Software.Name);
            object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(m_Cs), m_DbTransaction, CommandType.Text, sqlQuery);
            if (null != obj)
                isPreSettlementOnAllFlows = Convert.ToBoolean(obj);

            m_IsUsePreSettlement = isPreSettlementOnAllFlows;

            if (false == m_IsUsePreSettlement)
            {
                if (StrFunc.IsFilled(m_Currency))
                {
                    ArrayList m_CurrencyIn = new ArrayList();
                    sqlQuery = @"select IDC from dbo.CURRENCY where IDCQUOTED = 'EUR'";
                    DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(m_Cs), m_DbTransaction, sqlQuery);
                    if (null != dt)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            m_CurrencyIn.Add(row["IDC"].ToString());
                        }
                    }
                    m_IsUsePreSettlement = (false == m_CurrencyIn.Contains(m_Currency) && (m_Currency != m_IdCAccount_Entity));
                }
            }
        }
        #endregion SetIsWithSettlement
        #endregion Methods
    }
    #endregion EFS_SettlementInfoEntity
    #region EFS_SerializeInfo
    public class EFS_SerializeInfo : EFS_SerializeInfoBase
    {
        #region Accessors
        #region PathEfsMLSchemas
        private string PathEfsMLSchemas
        {
            get
            {
                string version = string.Empty;
                switch (((IEfsDocument)m_Document).EfsMLversion)
                {
                    case EfsMLDocumentVersionEnum.Version20:
                        version += "EfsMLv20";
                        break;
                    case EfsMLDocumentVersionEnum.Version30:
                        version += "EfsMLv30";
                        break;
                }
                return version;
            }
        }
        #endregion PathEfsMLSchemas
        #region PathFpMLSchemas
        private string PathFpMLSchemas
        {
            get
            {
                string version = string.Empty;
                switch (((IDocument)m_Document).Version)
                {
                    case DocumentVersionEnum.Version42:
                        version += "FpML42";
                        break;
                    case DocumentVersionEnum.Version44:
                        version += "FpML44";
                        break;
                }
                return version;
            }
        }
        #endregion PathFpMLSchemas
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDocument"></param>
        /// <param name="pPath">Répertoire des fichiers xsd</param>
        public EFS_SerializeInfo(object pDocument, string pPath)
            : this(pDocument)
        {
            DeclareSchemas(pPath);
        }
        public EFS_SerializeInfo(object pDocument)
            : base(pDocument)
        {
            m_Type = Tools.GetTypeDocument(m_Document);
            SetInfoBase();
        }
        public EFS_SerializeInfo(string pXMLDocument)
            : base(Tools.GetTypeXmlDocument(pXMLDocument), pXMLDocument)
        {
            SetInfoBase();
        }
        #endregion Constructors

        #region Methods
        #region SetInfoBase
        private void SetInfoBase()
        {
            m_NameSpace = Tools.GetNameSpaceDocument(m_Type);
            m_Source = Tools.GetSourceNameDocument(m_Type);
            if ((null != m_Document) && Tools.IsTypeOrInterfaceOf(m_Type, InterfaceEnum.IEfsDocument))
            {
                m_IsXMLTrade = true;
                m_Version = ((IEfsDocument)m_Document).EfsMLversion;
            }
        }
        #endregion SetInfoBase
        #region DeclareSchemas
        public void DeclareSchemas(string pPath)
        {
            ArrayList aSchemas = new ArrayList();
            if (Tools.IsTypeOrInterfaceOf(m_Type, InterfaceEnum.IEfsDocument))
            {
                #region EfsML XSD
                FieldInfo fld = ((IEfsDocument)m_Document).EfsMLversion.GetType().GetField(((IEfsDocument)m_Document).EfsMLversion.ToString());
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (0 != attributes.GetLength(0))
                    aSchemas.Add(pPath + PathEfsMLSchemas + @"\EfsML-" + ((XmlEnumAttribute)attributes[0]).Name + ".xsd");
                #endregion EfsML XSD
            }
            else if (Tools.IsTypeOrInterfaceOf(m_Type, InterfaceEnum.IDocument))
            {
                #region FpML XSD
                FieldInfo fld = ((IDocument)m_Document).Version.GetType().GetField(((IDocument)m_Document).Version.ToString());
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (0 != attributes.GetLength(0))
                    aSchemas.Add(pPath + PathFpMLSchemas + @"\fpml-main-" + ((XmlEnumAttribute)attributes[0]).Name + ".xsd");
                #endregion FpML XSD
            }
            if (0 < aSchemas.Count)
            {
                #region FixML XSD
                if (DocumentVersionEnum.Version42 == ((IDocument)m_Document).Version)
                    aSchemas.Add(pPath + @"FIXML44\fixml-order-impl-4-4.xsd");
                else if (DocumentVersionEnum.Version44 == ((IDocument)m_Document).Version)
                    aSchemas.Add(pPath + @"FIXML50SP1\fixml-tradecapture-impl-5-0-SP1.xsd");
                #endregion FixML XSD
                m_Schemas = (string[])aSchemas.ToArray(typeof(string));
            }
        }
        #endregion DeclareSchemas
        #endregion Methods
    }
    #endregion EFS_SerializeInfo
    #region EFS_SimplePayment
    /// <summary>
    /// Classe de travail associée à un ISimplePayment
    /// </summary>
    public class EFS_SimplePayment
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private EFS_AdjustableDate _paymentDateAdjustment;
        /// <summary>
        /// 
        /// </summary>
        private ISimplePayment _simplePayment;
        /// <summary>
        /// 
        /// </summary>
        private EFS_EventDate _expirationDate;
        /// <summary>
        /// 
        /// </summary>
        private EFS_PreSettlement _preSettlement;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string PayerPartyReference
        {
            get
            {
                return _simplePayment.PayerPartyReference.HRef;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ReceiverPartyReference
        {
            get
            {
                return _simplePayment.ReceiverPartyReference.HRef;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EFS_Decimal Amount
        {
            get
            {
                return _simplePayment.PaymentAmount.Amount;
            }
        }

        /// <summary>
        /// Obtient la devise
        /// </summary>
        public string Currency
        {
            get
            {
                return _simplePayment.PaymentAmount.Currency;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EFS_EventDate ExpirationDate
        {
            get
            {
                return _expirationDate;
            }
        }

        /// <summary>
        /// Obtient les dates (ajustée et non ajustée)
        /// </summary>
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(_paymentDateAdjustment);
            }
        }

        /// <summary>
        /// Obtient la date non ajustée
        /// </summary>
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {
                EFS_Date ret = new EFS_Date
                {
                    DateValue = _paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return ret;
            }
        }

        /// <summary>
        /// Obtient la date ajustée
        /// </summary>
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                EFS_Date ret = new EFS_Date
                {
                    DateValue = _paymentDateAdjustment.adjustedDate.DateValue
                };
                return ret;
            }
        }

        /// <summary>
        /// Obtient la date ajustée de PreSettlement
        /// <para>Obtient null si Spheres® ne détecte pas la nécessite d'un PreSettlement</para>
        /// </summary>
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {
                EFS_Date ret = null;
                if (null != _preSettlement)
                    ret = _preSettlement.AdjustedPreSettlementDate;
                return ret;
            }
        }
        #endregion Accessors

        #region Constructors
        public EFS_SimplePayment(string pCS, DataDocumentContainer pDoc, ISimplePayment pPayment)
        {
            Calc(pCS, pDoc, pPayment);
        }
        #endregion Constructors
        //
        #region Methods
        /// <summary>
        /// Calcule les members
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pPayment"></param>
        public void Calc(string pCs, DataDocumentContainer pDoc, ISimplePayment pPayment)
        {
            _simplePayment = pPayment;
            //
            #region paymentDate adjustement
            if (pPayment.PaymentDate.AdjustableDateSpecified)
            {
                _paymentDateAdjustment = new EFS_AdjustableDate(pCs, _simplePayment.PaymentDate.AdjustableDate, pDoc);
            }
            else if (pPayment.PaymentDate.RelativeDateSpecified)
            {
                string href = pPayment.PaymentDate.RelativeDate.DateRelativeToValue;
                object dateReference = pDoc.GetObjectById(href);
                if (null == dateReference)
                    throw new NullReferenceException(StrFunc.AppendFormat("Id:{0} doesn't exist in DataDocument", href));
                //
                throw new NotImplementedException("paymentDate.relativeDate is not implemented");
            }
            #endregion paymentDate adjustement
            //
            #region preSettlementPaymentDate
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(pCs, Currency,
                PayerPartyReference, ReceiverPartyReference, pDoc,
                _simplePayment.PaymentAmount.DefaultOffsetPreSettlement);
            //                   
            if (preSettlementInfo.IsUsePreSettlement)
                _preSettlement = new EFS_PreSettlement(pCs,  null,_paymentDateAdjustment.adjustedDate.DateValue, Currency, preSettlementInfo.OffsetPreSettlement, pDoc);
            #endregion preSettlementPaymentDate
            //
            CalcExpirationDate(pDoc.CurrentProduct);
        }
        /// <summary>
        /// Calcule ExpirationDate
        /// </summary>
        /// <param name="pProduct"></param>
        private void CalcExpirationDate(ProductContainer pMainProduct)
        {
            if (pMainProduct.IsMarginRequirement)
            {
                _expirationDate = PaymentDate;
            }
            else if (pMainProduct.IsCashBalance)
            {
                _expirationDate = PaymentDate;
            }
            else
            {
                //throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not managed, please contact EFS", pMainProduct.GetType().FullName));
            }
        }
        #endregion Methods
    }
    #endregion EFS_SimplePayment
    #region EFS_SingleUnderlyer
    public class EFS_SingleUnderlyer : EFS_Underlyer
    {
        #region Constructors
        public EFS_SingleUnderlyer(string pCS, OptionTypeEnum pOptionType, IReference pBuyerPartyReference, IReference pSellerPartyReference, ISingleUnderlyer pSingleUnderlyer)
            : base(pCS, EventCodeFunc.SingleUnderlyer, EventCodeFunc.UnderlyerValuationDate)
        {
            SetBuyerSellerPartyReference(pOptionType, pBuyerPartyReference, pSellerPartyReference);
            SetUnderlyingAsset(pCS, pSingleUnderlyer.UnderlyingAsset);
            SetOpenUnits((IOpenUnits)pSingleUnderlyer);
        }
        public EFS_SingleUnderlyer(string pCS, IReference pPayerPartyReference, IReference pReceiverPartyReference, ISingleUnderlyer pSingleUnderlyer)
            : base(pCS, EventCodeFunc.SingleUnderlyer, EventCodeFunc.UnderlyerValuationDate)
        {
            SetPayerReceiverPartyReference(pPayerPartyReference, pReceiverPartyReference);
            SetUnderlyingAsset(pCS, pSingleUnderlyer.UnderlyingAsset);
            SetOpenUnits((IOpenUnits)pSingleUnderlyer);
        }
        #endregion Constructors
    }
    #endregion EFS_SingleUnderlyer
    #region EFS_Step
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     </comment>
    /// </revision>
    public class EFS_Step : IEFS_Step
    {
        #region Members
        private readonly EFS_AdjustableDate m_AdjustableDate;
        private readonly EFS_Decimal m_StepValue;
        #endregion Members
        #region Accessors
        #region AdjustedDate
        public DateTime AdjustedDate
        {
            get { return m_AdjustableDate.adjustedDate.DateValue; }
        }
        #endregion AdjustedDate
        #region UnAdjustedDate
        public DateTime UnAdjustedDate
        {
            get { return m_AdjustableDate.AdjustableDate.UnadjustedDate.DateValue; }
        }
        #endregion UnAdjustedDate
        #region StepValue
        public decimal StepValue
        {
            get { return m_StepValue.DecValue; }
        }
        #endregion StepValue
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Step(string pConnectionString, DateTime pUnAjustedStepDate, decimal pStepValue, IBusinessDayAdjustments pBusinessDayAdjustments, 
            DataDocumentContainer pDataDocument)
        {
            m_AdjustableDate = new EFS_AdjustableDate(pConnectionString, pUnAjustedStepDate, pBusinessDayAdjustments, pDataDocument);
            m_StepValue = new EFS_Decimal(pStepValue);
        }
        #endregion Constructors

        #region IEFS_Step Members
        DateTime IEFS_Step.AdjustedDate { get { return this.AdjustedDate; } }
        decimal IEFS_Step.StepValue { get { return this.StepValue; } }
        #endregion IEFS_Step Members
    }
    #endregion EFS_Step
    #region EFS_StrikeBasisRate
    public class EFS_StrikeBasisRate
    {
        #region Members
        public string callCurrency;
        public string putCurrency;
        public StrikeQuoteBasisEnum strikeQuoteBasis;
        public EFS_Decimal rate;
        #endregion Members
    }
    #endregion EFS_StrikeBasisRate

    #region EFS_Tax
    public class EFS_Tax
    {
        #region Members
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_EventDate expirationDate;
        public EFS_Date adjustedPaymentDate;
        public EFS_Date adjustedPreSettlementDate;
        public EFS_TaxSource taxSource;
        public bool taxAmountSpecified;
        public ITripleInvoiceAmounts taxAmount;
        // EG/PM 20150108 New 
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        // EG 20150708 [21103] New
        public Pair<DateTime, DateTime> dtEventStartPeriod;
        public Pair<DateTime, DateTime> dtEventEndPeriod;
        #endregion Members
        #region Accessors
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20150318 (POC] isTaxCollected : (AdjustedPaymentDate == null si isTaxCollected = FALSE)
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                EFS_Date retDt = null;
                if (taxSource.isTaxCollected)
                    retDt = new EFS_Date(paymentDateAdjustment.adjustedDate.Value);
                return retDt;
            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20150318 (POC] isTaxCollected : (AdjustedPreSettlementDate == null si isTaxCollected = FALSE)
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {
                EFS_Date retDt = null;
                if (taxSource.isTaxCollected)
                    retDt = adjustedPreSettlementDate;
                return retDt;
            }
        }
        #endregion AdjustedPreSettlementDate
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get
            {
                return taxAmount.Amount.Amount;

            }
        }
        #endregion Amount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                return taxAmount.Amount.Currency;

            }
        }
        #endregion Currency
        #region IssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal IssueAmount
        {
            get
            {
                if (taxAmount.IssueAmountSpecified)
                    return taxAmount.IssueAmount.Amount;
                return null;

            }
        }
        #endregion IssueAmount
        #region IssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string IssueCurrency
        {
            get
            {
                if (taxAmount.IssueAmountSpecified)
                    return taxAmount.IssueAmount.Currency;
                return null;

            }
        }
        #endregion IssueCurrency
        #region AccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal AccountingAmount
        {
            get
            {

                if (taxAmount.AccountingAmountSpecified)
                    return taxAmount.AccountingAmount.Amount;
                return null;

            }
        }
        #endregion AccountingAmount
        #region AccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string AccountingCurrency
        {
            get
            {

                if (taxAmount.AccountingAmountSpecified)
                    return taxAmount.AccountingAmount.Currency;
                return null;

            }
        }
        #endregion AccountingCurrency
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                return this.taxSource.eventType;

            }
        }
        #endregion EventType
        #region ExpirationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpirationDate
        {
            get
            {
                return expirationDate;

            }
        }
        #endregion ExpirationDate
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(paymentDateAdjustment);

            }
        }
        #endregion PaymentDate
        #region TaxAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal TaxAmount
        {
            get
            {
                return this.taxAmount.Amount.Amount;

            }
        }
        #endregion TaxAmount
        #region TaxCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string TaxCurrency
        {
            get
            {
                return this.taxAmount.Amount.Currency;

            }
        }
        #endregion TaxCurrency
        #region TaxIssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal TaxIssueAmount
        {
            get
            {
                if (this.taxAmount.IssueAmountSpecified)
                    return this.taxAmount.IssueAmount.Amount;
                return null;

            }
        }
        #endregion TaxIssueAmount
        #region TaxIssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string TaxIssueCurrency
        {
            get
            {
                if (this.taxAmount.IssueAmountSpecified)
                    return this.taxAmount.IssueAmount.Currency;
                return null;

            }
        }
        #endregion TaxIssueCurrency
        #region TaxAccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal TaxAccountingAmount
        {
            get
            {

                if (this.taxAmount.AccountingAmountSpecified)
                    return this.taxAmount.AccountingAmount.Amount;
                return null;

            }
        }
        #endregion TaxAccountingAmount
        #region TaxAccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string TaxAccountingCurrency
        {
            get
            {

                if (this.taxAmount.AccountingAmountSpecified)
                    return this.taxAmount.AccountingAmount.Currency;
                return null;

            }
        }
        #endregion TaxAccountingCurrency
        #region TaxSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_TaxSource TaxSource
        {
            get
            {
                return taxSource;

            }
        }
        #endregion TaxSource

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
        {
            get {return payerPartyReference.HRef;}
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG/PM 20150108 New 
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
        {
            get {return receiverPartyReference.HRef;}
        }
        #endregion ReceiverPartyReference

        #endregion Accessors
        #region Constructors
        public EFS_Tax(EFS_Payment pEFS_Payment, ISpheresSource pTaxSource, ITaxSchedule pTaxSchedule)
        {
            payerPartyReference = pEFS_Payment.payerPartyReference;
            receiverPartyReference = pEFS_Payment.receiverPartyReference;
            paymentDateAdjustment = pEFS_Payment.paymentDateAdjustment;
            expirationDate = pEFS_Payment.expirationDate;
            taxSource = new EFS_TaxSource(pTaxSource, pTaxSchedule.TaxSource);
            taxAmountSpecified = pTaxSchedule.TaxAmountSpecified;
            if (taxAmountSpecified)
                taxAmount = pTaxSchedule.TaxAmount;

            if (pEFS_Payment.preSettlementSpecified)
                adjustedPreSettlementDate = pEFS_Payment.preSettlement.AdjustedPreSettlementDate;

            // EG 20150708 [21103] New
            dtEventStartPeriod = pEFS_Payment.dtEventStartPeriod;
            dtEventEndPeriod = pEFS_Payment.dtEventEndPeriod;


        }
        #endregion Constructors
    }
    #endregion EFS_Tax
    #region EFS_TaxSource
    // EG 20150318 [POC] new isTaxCollected
    public class EFS_TaxSource : ICloneable
    {
        #region Members
        public bool idTaxSpecified;
        public int idTax;
        public bool idTaxDetSpecified;
        public int idTaxDet;
        public bool taxTypeSpecified;
        public string taxType;
        public bool taxRateSpecified;
        public decimal taxRate;
        public bool taxCountrySpecified;
        public string taxCountry;
        public string eventType;
        public bool isTaxCollected;
        #endregion Members
        #region Constructors
        public EFS_TaxSource()
        {
        }
        public EFS_TaxSource(ISpheresSource pTaxSource, ISpheresSource pTaxSourceDetail)
            : this(pTaxSourceDetail)
        {

            #region idTax
            ISpheresIdSchemeId spheresIdScheme = pTaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxScheme);
            idTaxSpecified = (null != spheresIdScheme);
            if (idTaxSpecified)
                idTax = pTaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxScheme).OTCmlId;
            #endregion idTax
        }
        public EFS_TaxSource(ISpheresSource pTaxSourceDetail)
        {
            #region idTaxDet
            ISpheresIdSchemeId spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailScheme);
            idTaxDetSpecified = (null != spheresIdScheme);
            if (idTaxDetSpecified)
                idTaxDet = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailScheme).OTCmlId;
            #endregion idTaxDet
            #region taxType
            spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailTypeScheme);
            taxTypeSpecified = (null != spheresIdScheme);
            if (taxTypeSpecified)
                taxType = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailTypeScheme).Value;
            #endregion taxType
            #region taxRate
            spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme);
            taxRateSpecified = (null != spheresIdScheme);
            if (taxRateSpecified)
                taxRate = DecFunc.DecValueFromInvariantCulture(pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme).Value);
            #endregion taxRate
            #region taxCountry
            spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailCountryScheme);
            taxCountrySpecified = (null != spheresIdScheme);
            if (taxCountrySpecified)
                taxCountry = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailCountryScheme).Value;
            #endregion taxCountry
            #region EventType
            spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme);
            if (null != spheresIdScheme)
                eventType = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme).Value;
            #endregion EventType
            #region IsTaxCollected
            // EG 20150318 [POC]
            // CC/PL 20151105 Add else and isTaxCollected = true; (Compatibilité Asc.)
            spheresIdScheme = pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailCollected);
            if (null != spheresIdScheme)
                isTaxCollected = Convert.ToBoolean(pTaxSourceDetail.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailCollected).Value);
            else
                isTaxCollected = true;
            #endregion IsTaxCollected
        }
        #endregion Constructors
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_TaxSource clone = new EFS_TaxSource
            {
                idTaxSpecified = idTaxSpecified,
                idTax = idTax,
                idTaxDetSpecified = idTaxDetSpecified,
                idTaxDet = idTaxDet,
                taxTypeSpecified = taxTypeSpecified,
                taxType = taxType,
                taxRateSpecified = taxRateSpecified,
                taxRate = taxRate,
                taxCountrySpecified = taxCountrySpecified,
                taxCountry = taxCountry,
                eventType = eventType
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion EFS_TaxSource

    #region EFS_TradeLibrary
    public class EFS_TradeLibrary
    {

        /// <summary>
        /// Attaching delegate method
        /// </summary>
        AddAttachedDoc m_Attach = null;
        int m_Attach_IdProcess;
        int m_Attach_IdA;


        #region Members
        /// <summary>
        /// 
        /// </summary>
        private DataDocumentContainer _dataDocument;
        /// <summary>
        /// Document XML
        /// </summary>
        private XmlDocument _xmlDocument;
        
        /// <summary>
        /// Matrice des événements
        /// </summary>
        private bool _isDisposed;
        #endregion Members

        #region Accessors


        /// <summary>
        /// 
        /// </summary>
        /// FI 20180919 [23976] Public SET
        public EFS_EventMatrix EventMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public DataDocumentContainer DataDocument
        {
            get { return _dataDocument; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ITrade CurrentTrade
        {
            get { return _dataDocument.CurrentTrade; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IParty[] Party
        {
            get { return _dataDocument.Party; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IProduct Product
        {
            get { return CurrentTrade.Product; }
        }

        /// <summary>
        /// 
        /// </summary>
        public XmlDocument XmlDocument
        {
            get { return _xmlDocument; }
        }
        #endregion Accessors

        #region Constructor
        public EFS_TradeLibrary(){}
        /// <summary>
        /// Constructeur avec chargement du DataDocument
        /// </summary>
        /// <param name="pIdT">IdT du trade à charger</param>
        // EG 20180205 [23769] Upd EFS_TradeLibray constructor (not use EFS_CURRENT)  
        public EFS_TradeLibrary(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            LoadDocument(pCS, pDbTransaction, pIdT);
        }
        #endregion Constructor

        #region Destructors
        ~EFS_TradeLibrary() { Dispose(false); }
        #endregion Destructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsDisposing"></param>
        protected void Dispose(bool pIsDisposing)
        {
            if (false == _isDisposed)
            {
                try
                {
                    if (pIsDisposing)
                    {
                        _xmlDocument = null;
                        _dataDocument = null;
                        EventMatrix = null;
                    }
                }
                finally { _isDisposed = true; }
            }
        }

        /// <summary>
        /// Retourne la Query de selection de la colonne TRADEXML de la table TRADE
        /// </summary>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20180530 [23980] Add pDbTransaction parameter 
        // RD 20210304 Add "trx."
        protected virtual QueryParameters GetQueryTradeXML(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            return TradeRDBMSTools.GetQueryParametersTrade(pCS, pDbTransaction, pIdT, new string[] { "trx.TRADEXML" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlpath">Path où se trouve les ficheirs xslt</param>
        /// <param name="pXslFileName">Nom du fichier maître,Il ne faut pas renseigner l'extension du fichier</param>
        /// <returns></returns>
        // EG 20231127 [WI755] Implementation Return Swap : CFD vs RTS
        public void EventsMatrixConstruction(string pCS, string pXmlpath, string pXslFileName)
        {
            string xslFile = StrFunc.AppendFormat(@"{0}\{1}.xslt", pXmlpath, pXslFileName);
            if (false == File.Exists(xslFile))
                throw new Exception(StrFunc.AppendFormat("File[{0}] doesn't exist", xslFile));

            #region Simple Product or Strategy
            string xmlText = "<EventMatrix>" + Environment.NewLine;
            xmlText += GetXmlEventMatrixText(pCS, Product) + Environment.NewLine;
            xmlText += "</EventMatrix>";
            #endregion Simple Product or Strategy
            //
            StringBuilder sb = new StringBuilder();
            sb.Append(xmlText);
            //
            Hashtable param = new Hashtable
            {
                { "pMasterProduct", Product.GetType().Name }
            };

            string retTransform = XSLTTools.TransformXml(sb, xslFile, param, null);
            if (null != m_Attach)
            {
                StringBuilder sbTmp = new StringBuilder(retTransform);
                sbTmp.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                Encoding fileEncoding = Encoding.UTF8;
                byte[] content = fileEncoding.GetBytes(sbTmp.ToString());
                m_Attach.Invoke(pCS, m_Attach_IdProcess, m_Attach_IdA, content, "EVENTMATRIX", Cst.TypeMIME.Text.Xml);
            }
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(EFS_EventMatrix), retTransform);
            EventMatrix = (EFS_EventMatrix)CacheSerializer.Deserialize(serializeInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        /// EG 20231127 [WI755] Implementation Return Swap : Bidouille pour charge ContractForDifference à la place de ReturnSwap (Test sur Instrument Fongible)
        /// FI 20240213 [WI848] EventsGen : Exception on Strategy Product 
        private string GetXmlEventMatrixText(string pCS, IProduct pProduct)
        {
            IProductBase productBase = (IProductBase)pProduct;
            string xmlText = string.Empty;

            if (productBase.IsStrategy)
            {
                xmlText += $@"<item name=""{productBase.ProductName}"">" + Environment.NewLine;
                foreach (IProduct subProductIem in ((IStrategy)pProduct).SubProduct)
                {
                    if (subProductIem.ProductBase.IsStrategy)
                    {
                        xmlText += $@"<item name=""{subProductIem.ProductBase.ProductName}"">" + Environment.NewLine;
                        xmlText += GetXmlEventMatrixText(pCS, subProductIem);
                        xmlText += @"</item>" + Environment.NewLine;
                    }
                    else
                        xmlText += $@"<item name=""{subProductIem.ProductBase.ProductName}""/>" + Environment.NewLine;
                }
                xmlText += @"</item>" + Environment.NewLine;
            }
            else if (productBase.IsReturnSwap && Tools.IsUseEntityMarket(pCS, DataDocument.CurrentProduct))
                xmlText += $@"<item name=""ContractForDifference""/>";
            else
                xmlText += $@"<item name=""{productBase.ProductName}""/>";
            return xmlText;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadXmlDocument()
        {
            EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(DataDocument);
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
            //
            _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdT"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        // EG 20180530 [23980] Use pDbTransaction on GetQueryTradeXML
        private void LoadDocument(string pCs, IDbTransaction pDbTransaction, int pIdT)
        {
            QueryParameters queryParameters = GetQueryTradeXML(pCs, pDbTransaction, pIdT);

            object obj = DataHelper.ExecuteScalar(pCs, pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
            {
                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo((string)obj);
                _dataDocument = new DataDocumentContainer((IDataDocument)CacheSerializer.Deserialize(serializeInfo));
            }
        }
        #endregion Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAttach"></param>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdA"></param>
        public void InitLogAttachDelegates(AddAttachedDoc pAttach, int pIdProcess, int pIdA)
        {
            this.m_Attach = pAttach;
            this.m_Attach_IdProcess = pIdProcess;
            this.m_Attach_IdA = pIdA;
        }

    }
    #endregion EFS_TradeLibrary
    #region EFS_ExtendedTriggerEvent
    public class EFS_ExtendedTriggerEvent : EFS_TriggerEvent
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExtendedTriggerEvent(string pCS, string pTriggerType, EFS_Underlyer pUnderlyer, IExtendedTriggerEvent pTriggerEvent,
            EFS_OptionStrikeBase pStrike, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : base(pCS, pTriggerType, pUnderlyer, (ITriggerEvent)pTriggerEvent, pStrike, pBusinessDayAdjustments, pDataDocument) { }
        #endregion Constructors
    }
    #endregion EFS_ExtendedTriggerEvent
    #region EFS_TriggerEvent
    public class EFS_TriggerEvent : EFS_OptionFeaturesDates
    {
        #region Members
        public EFS_OptionStrikeBase strike;
        protected string triggerType;
        public EFS_Decimal triggerPrice;
        #endregion Members
        #region Accessors
        #region TriggerType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string TriggerType
        {
            get
            {
                string type = string.Empty;

                decimal knockPrice = triggerPrice.DecValue;
                decimal _price = 0;
                if (strike.spotPriceSpecified)
                    _price = strike.spotPrice.DecValue;
                else if (strike.strikePriceSpecified)
                    _price = strike.strikePrice.DecValue;

                if (EventTypeFunc.IsCap(triggerType))
                    type = EventTypeFunc.UpTouch;
                else if (EventTypeFunc.IsFloor(triggerType))
                    type = EventTypeFunc.DownTouch;
                else if (EventTypeFunc.IsKnockIn(triggerType))
                {
                    if (_price < knockPrice)
                        type = EventTypeFunc.UpIn;
                    else
                        type = EventTypeFunc.DownIn;
                }
                else if (EventTypeFunc.IsKnockOut(triggerType))
                {
                    if (_price < knockPrice)
                        type = EventTypeFunc.UpOut;
                    else
                        type = EventTypeFunc.DownOut;
                }
                return type;
            }
        }
        #endregion TriggerType
        #region TriggerEvent
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_TriggerEvent TriggerEvent
        {
            get { return this; }
        }
        #endregion TriggerEvent
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_TriggerEvent(string pConnectionString, string pTriggerType, EFS_Underlyer pUnderlyer, ITriggerEvent pTriggerEvent,
            EFS_OptionStrikeBase pStrike, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pUnderlyer, pTriggerEvent.ScheduleSpecified,
            pTriggerEvent.Schedule, pTriggerEvent.TriggerDatesSpecified, pTriggerEvent.TriggerDates, pBusinessDayAdjustments, pDataDocument)
        {
            triggerType = pTriggerType;
            strike = pStrike;
            #region TriggerPrice
            if (pTriggerEvent.Trigger.LevelSpecified)
                triggerPrice = new EFS_Decimal(pTriggerEvent.Trigger.Level.DecValue);
            else if (pTriggerEvent.Trigger.LevelPercentageSpecified)
            {
                if (strike.strikePriceSpecified)
                    triggerPrice = new EFS_Decimal(pTriggerEvent.Trigger.LevelPercentage.DecValue * strike.strikePrice.DecValue);
                else if (strike.strikePercentageSpecified)
                    triggerPrice = new EFS_Decimal(pTriggerEvent.Trigger.LevelPercentage.DecValue * strike.strikePercentage.DecValue);
            }
            #endregion TriggerPrice
        }
        #endregion Constructors
    }
    #endregion EFS_TriggerEvent

    #region EFS_Underlyer
    public abstract class EFS_Underlyer : EFS_UnderlyingAsset
    {
        #region Members
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public EFS_Decimal unitValue;
        public UnitTypeEnum unitType;
        public string unit;
        #endregion Members
        #region Accessors
        #region OpenUnits
        public EFS_Decimal OpenUnits
        {
            get { return new EFS_Decimal(unitValue.DecValue); }
        }
        #endregion OpenUnits
        #region OpenUnitsType
        public string OpenUnitsType
        {
            get { return unitType.ToString(); }
        }
        #endregion OpenUnitsType
        #region Unit
        public string Unit
        {
            get { return unit; }
        }
        #endregion Unit
        #region UnitType
        public string UnitType
        {
            get { return unitType.ToString(); }
        }
        #endregion UnitType
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference

        #endregion Accessors
        #region Constructors
        public EFS_Underlyer(string pCS, string pUnderlyerEventCode, string pValuationEventCode)
            : base(pCS, pUnderlyerEventCode, pValuationEventCode) { }
        #endregion Constructors
        #region Methods
        #region SetOpenUnits
        protected virtual void SetOpenUnits<T>(T pSource)
        {
            if (pSource is IBond || pSource is IConvertibleBond || pSource is ISecurityAsset)
            {
                unitValue = new EFS_Decimal(1);
                unitType = UnitTypeEnum.Qty;
                UpdateUnderlyingAsset(unitValue, unitType, unit);
            }
            else if (pSource is IOpenUnits)
            {
                IOpenUnits _openUnits = pSource as IOpenUnits;
                unitValue = new EFS_Decimal(_openUnits.OpenUnitsSpecified ? _openUnits.OpenUnits.DecValue : 1);
                unitType = UnitTypeEnum.Qty;
                UpdateUnderlyingAsset(unitValue, unitType, unit);
            }
            else if (pSource is IBasketConstituent)
            {
                IBasketConstituent _basketConstituent = pSource as IBasketConstituent;
                if (_basketConstituent.ConstituentWeightSpecified)
                {
                    if (_basketConstituent.ConstituentWeightBasketAmountSpecified)
                    {
                        #region Amount
                        unitValue = new EFS_Decimal(_basketConstituent.ConstituentWeightBasketAmountValue);
                        unitType = UnitTypeEnum.Currency;
                        unit = _basketConstituent.ConstituentWeightBasketAmountCurrency;
                        #endregion Amount
                    }
                    else if (_basketConstituent.ConstituentWeightBasketPercentageSpecified)
                    {
                        #region Percentage
                        unitValue = new EFS_Decimal(_basketConstituent.ConstituentWeightBasketPercentage);
                        unitType = UnitTypeEnum.Percentage;
                        #endregion Percentage
                    }
                    else if (_basketConstituent.ConstituentWeightOpenUnitsSpecified)
                    {
                        #region Weight
                        unitValue = new EFS_Decimal(_basketConstituent.ConstituentWeightOpenUnits);
                        unitType = UnitTypeEnum.Qty;
                        #endregion Weight
                    }
                    UpdateUnderlyingAsset(unitValue, unitType, unit);
                }
            }
        }
        #endregion SetOpenUnits
        #region SetBuyerSellerPartyReference
        protected void SetBuyerSellerPartyReference(OptionTypeEnum pOptionType, IReference pBuyerPartyReference, IReference pSellerPartyReference)
        {
            #region Payer/Receiver PartyReference
            if (OptionTypeEnum.Call == pOptionType)
            {
                payerPartyReference = pSellerPartyReference;
                receiverPartyReference = pBuyerPartyReference;
            }
            else if (OptionTypeEnum.Put == pOptionType)
            {
                payerPartyReference = pBuyerPartyReference;
                receiverPartyReference = pSellerPartyReference;
            }
            #endregion Payer/Receiver PartyReference
        }
        #endregion SetBuyerSellerPartyReference
        #region SetPayerReceiverPartyReference
        protected void SetPayerReceiverPartyReference(IReference pPayerPartyReference, IReference pReceiverPartyReference)
        {
            payerPartyReference = pPayerPartyReference;
            receiverPartyReference = pReceiverPartyReference;
        }
        #endregion SetPayerReceiverPartyReference
        #endregion Methods
    }
    #endregion EFS_Underlyer
    #region EFS_UnderlyingAsset
    public abstract class EFS_UnderlyingAsset
    {
        #region Members
        protected string cs;
        public int idAsset;
        readonly string underlyerEventCode;
        string underlyerEventType;
        readonly string valuationEventCode;
        string valuationEventType;
        public EFS_Asset asset;
        #endregion Members
        #region Accessors
        #region UnderlyerEventCode
        public string UnderlyerEventCode { get { return underlyerEventCode; } }
        #endregion UnderlyerEventCode
        #region ValuationEventCode
        public string ValuationEventCode { get { return valuationEventCode; } }
        #endregion ValuationEventCode

        #region UnderlyerEventType
        public string UnderlyerEventType { get { return underlyerEventType; } }
        #endregion UnderlyerEventType
        #region ValuationEventType
        public string ValuationEventType { get { return valuationEventType; } }
        #endregion ValuationEventType

        #region UnderlyingAsset
        public object UnderlyingAsset
        {
            get { return asset; }
        }
        #endregion UnderlyingAsset
        #endregion Accessors
        #region Constructors
        public EFS_UnderlyingAsset(string pCS, string pUnderlyerEventCode, string pValuationEventCode)
        {
            cs = pCS;
            underlyerEventCode = pUnderlyerEventCode;
            valuationEventCode = pValuationEventCode;
        }
        #endregion Constructors
        #region Methods
        #region SetUnderlyingAsset
        // EG 20140904 Add AssetCategory
        public void SetUnderlyingAsset<T>(string pCs, T pSource)
        {
            IUnderlyingAsset _underlyingAsset = null;
            asset = new EFS_Asset();
            if (pSource is ISecurityAsset)
            {
                ISecurityAsset _securityAsset = pSource as ISecurityAsset;
                idAsset = _securityAsset.OTCmlId;
                asset.idAsset = idAsset;
                if (_securityAsset.DebtSecuritySpecified)
                    _underlyingAsset = _securityAsset.DebtSecurity.Security as IUnderlyingAsset;
            }
            else if (pSource is IUnderlyingAsset)
            {
                _underlyingAsset = pSource as IUnderlyingAsset;
                idAsset = _underlyingAsset.OTCmlId;
                asset.idAsset = idAsset;
            }

            if (null != _underlyingAsset)
            {
                asset.assetCategory = _underlyingAsset.UnderlyerAssetCategory;
                underlyerEventType = _underlyingAsset.UnderlyerEventType;
                valuationEventType = underlyerEventType;

                if (_underlyingAsset.ExchangeIdSpecified)
                {
                    asset.IdMarketFIXML_SecurityExchange = _underlyingAsset.ExchangeId.Value;

                    SQL_Market sqlMarket = new SQL_Market(pCs, SQL_TableWithID.IDType.FIXML_SecurityExchange, asset.IdMarketFIXML_SecurityExchange, SQL_Table.ScanDataDtEnabledEnum.No);
                    sqlMarket.LoadTable(new string[] { "IDM", "IDENTIFIER", "ISO10383_ALPHA4" });
                    if (sqlMarket.IsLoaded)
                    {
                        asset.IdMarket = sqlMarket.Id;
                        asset.IdMarketIdentifier = sqlMarket.Identifier;
                        asset.IdMarketISO10383_ALPHA4 = sqlMarket.ISO10383_ALPHA4;
                    }
                }

                if (_underlyingAsset.ClearanceSystemSpecified)
                {
                    asset.clearanceSystem = _underlyingAsset.ClearanceSystem.Value;
                }

                if (_underlyingAsset.CurrencySpecified)
                {
                    asset.idC = _underlyingAsset.Currency.Value;
                }
            }
        }
        #endregion SetUnderlyingAsset
        #region UpdateUnderlyingAsset
        protected void UpdateUnderlyingAsset(EFS_Decimal pUnitValue, UnitTypeEnum pUnitType, string pUnit)
        {
            asset.weight = pUnitValue;
            asset.unitTypeWeight = pUnitType;
            asset.unitWeight = pUnit;
        }
        #endregion UpdateUnderlyingAsset
        #endregion Methods
    }
    #endregion EFS_UnderlyingAsset

    #region EFS_AutomaticExerciseBase
    public abstract class EFS_AutomaticExerciseBase
    {
        #region Members
        protected string cs;
        protected DataDocumentContainer m_DataDocument;
        public ExerciseStyleEnum exerciseStyle;
        public EFS_Date expiryDate;
        public bool settlementDateSpecified;
        public EFS_AdjustableDate settlementDate;
        public Nullable<SettlementTypeEnum> settlementType;
        public bool settlementCurrencySpecified;
        public string settlementCurrency;
        #endregion Members
        #region Accessors
        #region AdjustedAutomaticExerciseDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedAutomaticExerciseDate
        {
            get { return expiryDate; }
        }
        #endregion AdjustedAutomaticExerciseDate
        #region AdjustedSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedSettlementDate
        {
            get
            {
                EFS_Date adjustedSettlementDate = new EFS_Date();
                if (settlementDateSpecified)
                    adjustedSettlementDate.DateValue = settlementDate.adjustedDate.DateValue;
                else
                    adjustedSettlementDate.DateValue = expiryDate.DateValue;
                return adjustedSettlementDate;
            }
        }
        #endregion AdjustedSettlementDate
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get { return new EFS_EventDate(expiryDate.DateValue, expiryDate.DateValue); }
        }
        #endregion EndPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get { return new EFS_EventDate(expiryDate.DateValue, expiryDate.DateValue); }
        }
        #endregion StartPeriod
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                string eventType = string.Empty;
                switch (exerciseStyle)
                {
                    case ExerciseStyleEnum.American:
                        eventType = EventTypeFunc.American;
                        break;
                    case ExerciseStyleEnum.Bermuda:
                        eventType = EventTypeFunc.Bermuda;
                        break;
                    case ExerciseStyleEnum.European:
                        eventType = EventTypeFunc.European;
                        break;
                }
                return eventType;
            }
        }
        #endregion EventType
        #region SettlementType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SettlementType
        {
            get
            {
                string type = string.Empty;
                if (settlementType.HasValue)
                {
                    switch (settlementType.Value)
                    {
                        case SettlementTypeEnum.Cash:
                            type = EventTypeFunc.CashSettlement;
                            break;
                        case SettlementTypeEnum.Election:
                            type = EventTypeFunc.ElectionSettlement;
                            break;
                        case SettlementTypeEnum.Physical:
                            type = EventTypeFunc.PhysicalSettlement;
                            break;
                    }
                }
                return type;
            }
        }
        #endregion SettlementType
        #endregion Accessors
        #region Constructors
        public EFS_AutomaticExerciseBase() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_AutomaticExerciseBase(string pConnectionString, EFS_Date pExpiryDate, DataDocumentContainer pDataDocument)
        {
            cs = pConnectionString;
            m_DataDocument = pDataDocument; 
            expiryDate = pExpiryDate;
        }
        #endregion Constructors
    }
    #endregion EFS_AutomaticExerciseBase

    #region EFS_MarketScheduleTradingDayBase
    public abstract class EFS_MarketScheduleTradingDayBase
    {
        #region Members
        private readonly string m_Cs;
        private readonly IProductBase m_Product;
        public DayTypeEnum dayType;
        public IBusinessDayAdjustments defaultBda;
        protected List<IExchangeTraded> LstExchangeTraded { set; get; }
        public List<string> lstIdBC;
        #endregion Members
        #region Constructors
        public EFS_MarketScheduleTradingDayBase()
        {
        }
        public EFS_MarketScheduleTradingDayBase(string pCS, IProductBase pProduct)
        {
            m_Cs = pCS;
            m_Product = pProduct;
            dayType = DayTypeEnum.ScheduledTradingDay;
        }
        #endregion Constructors
        #region Methods
        protected void Initialize()
        {
            Initialize(BusinessDayConventionEnum.FOLLOWING);
        }
        protected void Initialize(BusinessDayConventionEnum pBDC)
        {
            lstIdBC = MarketTools2.GetListBusinessCenters(m_Cs, LstExchangeTraded);
            if (0 < lstIdBC.Count)
            defaultBda = m_Product.CreateBusinessDayAdjustments(pBDC, lstIdBC.ToArray());
        }
        public IBusinessDayAdjustments GetBusinessDayAdjustments(BusinessDayConventionEnum pBDC)
        {
            IBusinessDayAdjustments bda = null;
            if (0 < lstIdBC.Count)
                bda = m_Product.CreateBusinessDayAdjustments(pBDC, lstIdBC.ToArray());
            return bda;
        }
        #endregion Methods
    }
    #endregion EFS_MarketScheduleTradingDayBase

    #region EFS_OptionBase
    public abstract class EFS_OptionBase
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument; 

        protected ITradeDate tradeDate;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public OptionTypeEnum optionType;
        public EFS_Decimal numberOfOptions;
        public EFS_Decimal optionEntitlement;
        public bool faceAmountSpecified;
        public EFS_Decimal faceAmount;
        public EFS_Date effectiveDate;
        public EFS_Date expiryDate;
        public EFS_OptionNotionalBase notional;
        public EFS_Underlyer underlyer;
        public EFS_OptionStrikeBase strike;
        public bool isWithFeatures;
        public bool automaticExerciseSpecified;
        public EFS_AutomaticExerciseBase automaticExercise;
        public EFS_MarketScheduleTradingDayBase marketScheduleTradingDay;
        public bool asianSpecified;
        public EFS_AsianFeatures asian;
        public bool barrierSpecified;
        public EFS_BarrierFeatures barrier;
        public bool knockSpecified;
        public EFS_KnockFeatures knock;
        public bool multipleBarrierSpecified;
        public EFS_ExtendBarrierFeatures[] multipleBarrier;
        #endregion Members
        #region Accessors
        #region AdjustedExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedExpiryDate
        {
            get { return new EFS_Date(expiryDate.Value); }
        }
        #endregion AdjustedExpiryDate
        
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BuyerPartyReference
        {
            get {return buyerPartyReference.HRef;}
        }
        #endregion BuyerPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SellerPartyReference
        {
            get {return sellerPartyReference.HRef;}
        }
        #endregion SellerPartyReference
        
        #region EventCode
        public virtual string EventCode
        {
            get
            {
                return null;
            }
        }
        #endregion EventCode
        #region EventType
        public virtual string EventType
        {
            get { return isWithFeatures ? EventTypeFunc.Features : EventTypeFunc.Vanilla; }
        }
        #endregion EventType

        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EffectiveDate
        {
            get { return new EFS_EventDate(effectiveDate.DateValue, effectiveDate.DateValue); }
        }
        #endregion EffectiveDate
        #region ExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpiryDate
        {
            get { return new EFS_EventDate(expiryDate.DateValue, expiryDate.DateValue); }
        }
        #endregion ExpiryDate

        #region NotionalPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IReference NotionalPayerPartyReference
        {
            get
            {

                IReference partyReference = null;
                if (OptionTypeEnum.Call == optionType)
                    partyReference = buyerPartyReference;
                else if (OptionTypeEnum.Put == optionType)
                    partyReference = sellerPartyReference;
                return partyReference;

            }
        }
        #endregion NotionalPayerPartyReference
        #region NotionalReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IReference NotionalReceiverPartyReference
        {
            get
            {

                IReference partyReference = null;
                if (OptionTypeEnum.Call == optionType)
                    partyReference = sellerPartyReference;
                else if (OptionTypeEnum.Put == optionType)
                    partyReference = buyerPartyReference;
                return partyReference;

            }
        }
        #endregion NotionalReceiverPartyReference

        #region NumberOptions
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal NumberOptions
        {
            get
            {
                return numberOfOptions;

            }
        }
        #endregion NumberOptions
        #region NumberOptionsUnitType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NumberOptionsUnitType
        {
            get
            {
                return UnitTypeEnum.Qty.ToString();
            }
        }
        #endregion NumberOptionsUnitType

        #region UnderlyerType
        public virtual string UnderlyerType
        {
            get
            {
                return null;
            }
        }
        #endregion UnderlyerType

        #region OptionEntitlement
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal OptionEntitlement
        {
            get {return optionEntitlement;}
        }
        #endregion OptionEntitlement
        #region OptionEntitlementUnitType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string OptionEntitlementUnitType
        {
            get {return UnitTypeEnum.Qty.ToString();}
        }
        #endregion OptionEntitlementUnitType
        #region OptionEntitlementUnit
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string OptionEntitlementUnit
        {
            get {return null;}
        }
        #endregion OptionEntitlementUnit

        #region UnitValueUnderlyerPerOption
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal UnitValueUnderlyerPerOption
        {
            get
            {
                return optionEntitlement;

            }
        }
        #endregion UnitValueUnderlyerPerOption

        #region UnitTypeNumberOptions
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnitTypeNumberOptions
        {
            get
            {
                return UnitTypeEnum.Qty.ToString();
            }
        }
        #endregion UnitTypeNumberOptions
        #region UnitTypeUnderlyerPerOption
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnitTypeUnderlyerPerOption
        {
            get
            {
                return UnitTypeEnum.Qty.ToString();

            }
        }
        #endregion UnitTypeUnderlyerPerOption

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionBase(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionBase(string pConnectionString, IReference pBuyerPartyReference, IReference pSellerPartyReference, OptionTypeEnum pOptionType, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            tradeDate = m_DataDocument.TradeHeader.TradeDate;
            buyerPartyReference = pBuyerPartyReference;
            sellerPartyReference = pSellerPartyReference;
            optionType = pOptionType;
        }
        #endregion Constructors
        #region Methods
        #region SetFeatures
        public void SetFeatures<T>(T pSource)
        {
            SetAsianFeature(pSource);
            SetBarrierFeature(pSource);
            SetKnockFeature(pSource);
            SetMultipleBarriersFeatures(pSource);
        }
        #endregion SetFeatures
        #region SetAsianFeature
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetAsianFeature<T>(T pSource)
        {
            IAsian _asian = null;
            if (pSource is IOptionFeatures)
            {
                IOptionFeatures features = pSource as IOptionFeatures;
                asianSpecified = features.AsianSpecified;
                _asian = features.Asian;
            }
            else if (pSource is IOptionFeature)
            {
                IOptionFeature feature = pSource as IOptionFeature;
                asianSpecified = feature.AsianSpecified;
                _asian = feature.Asian;
            }
            if (asianSpecified)
                asian = new EFS_AsianFeatures(m_Cs, _asian, underlyer, marketScheduleTradingDay.defaultBda, m_DataDocument);
        }
        #endregion SetAsianFeature
        #region SetBarrierFeature
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetBarrierFeature<T>(T pSource)
        {
            IBarrier _barrier = null;
            if (pSource is IOptionFeatures)
            {
                IOptionFeatures features = pSource as IOptionFeatures;
                barrierSpecified = features.BarrierSpecified;
                _barrier = features.Barrier;
            }
            else if (pSource is IOptionFeature)
            {
                IOptionFeature feature = pSource as IOptionFeature;
                barrierSpecified = feature.BarrierSpecified;
                _barrier = feature.Barrier;
            }
            if (barrierSpecified)
                barrier = new EFS_BarrierFeatures(m_Cs, _barrier, underlyer, strike, marketScheduleTradingDay.defaultBda, m_DataDocument);
        }
        #endregion SetBarrierFeature
        #region SetKnockFeature
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetKnockFeature<T>(T pSource)
        {
            IKnock _knock = null;
            if (pSource is IOptionFeatures)
            {
                IOptionFeatures features = pSource as IOptionFeatures;
                knockSpecified = features.KnockSpecified;
                _knock = features.Knock;
            }
            else if (pSource is IOptionFeature)
            {
                IOptionFeature feature = pSource as IOptionFeature;
                knockSpecified = feature.KnockSpecified;
                _knock = feature.Knock;
            }
            if (knockSpecified)
                knock = new EFS_KnockFeatures(m_Cs, _knock, underlyer, strike, marketScheduleTradingDay.defaultBda, m_DataDocument);
        }
        #endregion SetKnockFeature
        #region SetMultipleBarriersFeatures
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetMultipleBarriersFeatures<T>(T pSource)
        {
            if (pSource is IOptionFeatures)
            {
                IOptionFeatures features = pSource as IOptionFeatures;
                multipleBarrierSpecified = features.MultipleBarrierSpecified;
                if (multipleBarrierSpecified)
                {
                    List<EFS_ExtendBarrierFeatures> _lstExtendedBarrier = new List<EFS_ExtendBarrierFeatures>();
                    foreach (IExtendedBarrier _extendedBarrier in features.MultipleBarrier)
                    {
                        _lstExtendedBarrier.Add(new EFS_ExtendBarrierFeatures(m_Cs, _extendedBarrier, underlyer, strike, marketScheduleTradingDay.defaultBda, m_DataDocument));
                    }
                    multipleBarrier = _lstExtendedBarrier.ToArray();
                }
            }
        }
        #endregion SetMultipleBarriersFeatures
        #endregion Methods
    }
    #endregion EFS_OptionBase

    #region EFS_OptionExercise
    public abstract class EFS_OptionExercise
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        public ExerciseStyleEnum exerciseStyle;
        public EFS_AdjustableDate commencementDate;
        public EFS_AdjustableDate expirationDate;
        public Nullable<SettlementTypeEnum> settlementType;
        public EFS_OptionExerciseDates[] exerciseDates;
        public EFS_MarketScheduleTradingDayBase marketScheduleTradingDay;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionExercise(string pCS, ExerciseStyleEnum pExerciseStyle, SettlementTypeEnum pSettlementType, bool pSettlementTypeSpecified, 
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument;
            exerciseStyle = pExerciseStyle;
            if (pSettlementTypeSpecified)
                settlementType = pSettlementType;
            marketScheduleTradingDay = pMarketScheduleTradingDay;
        }
        #endregion Constructors
    }
    #endregion EFS_OptionExercise
    #region EFS_OptionExerciseDates
    public abstract class EFS_OptionExerciseDates
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        public string eventType;
        public ExerciseStyleEnum exerciseStyle;
        public Nullable<SettlementTypeEnum> settlementType;
        public EFS_AdjustableDate commencementDate;
        public EFS_AdjustableDate expiryDate;
        public EFS_AdjustableDate exerciseDate;
        public EFS_ValuationDates[] valuationDates;
        public EFS_MarketScheduleTradingDayBase marketScheduleTradingDay;
        #endregion Members
        #region Accessors
        #region AdjustedExerciseDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedExerciseDate
        {
            get
            {
                EFS_Date adjustedExerciseDate = new EFS_Date
                {
                    DateValue = exerciseDate.adjustedDate.DateValue
                };
                return adjustedExerciseDate;
            }
        }
        #endregion AdjustedExerciseDate
        #region CommencementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate CommencementDate
        {
            get { return new EFS_EventDate(commencementDate.AdjustableDate.UnadjustedDate.DateValue, commencementDate.adjustedDate.DateValue); }
        }
        #endregion CommencementDate
        #region ExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpiryDate
        {
            get { return new EFS_EventDate(expiryDate.AdjustableDate.UnadjustedDate.DateValue, expiryDate.adjustedDate.DateValue); }
        }
        #endregion ExpiryDate

        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                string eventType = string.Empty;
                switch (exerciseStyle)
                {
                    case ExerciseStyleEnum.American:
                        eventType = EventTypeFunc.American;
                        break;
                    case ExerciseStyleEnum.Bermuda:
                        eventType = EventTypeFunc.Bermuda;
                        break;
                    case ExerciseStyleEnum.European:
                        eventType = EventTypeFunc.European;
                        break;
                }
                return eventType;
            }
        }
        #endregion EventType

        #region SettlementType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SettlementType
        {
            get 
            {
                string _settlementType = string.Empty;
                if (settlementType.HasValue)
                {
                    switch (settlementType)
                    {
                        case SettlementTypeEnum.Cash:
                            _settlementType = EventClassFunc.CashSettlement;
                            break;
                        case SettlementTypeEnum.Election:
                            _settlementType = EventClassFunc.ElectionSettlement;
                            break;
                        case SettlementTypeEnum.Physical:
                            _settlementType = EventClassFunc.PhysicalSettlement;
                            break;
                    }

                }
                return _settlementType; 
            }
        }
        #endregion SettlementType

        #endregion Accessors
        #region Constructors
        public EFS_OptionExerciseDates() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionExerciseDates(string pCS, ExerciseStyleEnum pExerciseStyle, SettlementTypeEnum pSettlementType,
            EFS_AdjustableDate pCommencementDate, EFS_AdjustableDate pExpiryDate, EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, 
            DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument;
            exerciseStyle = pExerciseStyle;
            settlementType = pSettlementType;
            marketScheduleTradingDay = pMarketScheduleTradingDay;
            SetPeriodExerciseDates(pCommencementDate, pExpiryDate);
        }
        #endregion Constructors
        #region Methods
        #region CalculExerciseValuationDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
        protected void CalculExerciseValuationDates<T>(EFS_Underlyer pUnderlyer, T pSource)
        {
            if (null != commencementDate)
            {
                List<EFS_ValuationDates> lstValuationDates = new List<EFS_ValuationDates>();
                IBusinessDayAdjustments bda = marketScheduleTradingDay.GetBusinessDayAdjustments(commencementDate.AdjustableDate.DateAdjustments.BusinessDayConvention);
                EFS_AdjustableDate adjustable_ValuationDate;
                if (null != pSource)
                {
                    #region Bermuda exercise
                    if (pSource is IDateList)
                    {
                        IDateList exerciseDates = pSource as IDateList;
                        for (int i = 0; i < exerciseDates.Date.Length; i++)
                        {
                            adjustable_ValuationDate = new EFS_AdjustableDate(m_Cs, exerciseDates[i], bda, marketScheduleTradingDay.dayType, m_DataDocument);
                            lstValuationDates.Add(UnderlyerValuationDate(pUnderlyer, adjustable_ValuationDate));
                        }
                    }
                    else if (pSource is EFS_AdjustableDates)
                    {
                        EFS_AdjustableDates exerciseDates = pSource as EFS_AdjustableDates;
                        for (int i = 0; i < exerciseDates.adjustableDates.Length; i++)
                        {
                            lstValuationDates.Add(UnderlyerValuationDate(pUnderlyer, exerciseDates.adjustableDates[i]));
                        }
                    }
                    #endregion Bermuda exercise
                }
                else
                {
                    #region American/European exercise
                    lstValuationDates.Add(UnderlyerValuationDate(pUnderlyer, commencementDate));
                    DateTime dtValuationDate = commencementDate.adjustedDate.DateValue;
                    DateTime dtExpiryDate = expiryDate.adjustedDate.DateValue;
                    int guard = 0;
                    while (dtValuationDate < dtExpiryDate)
                    {
                        guard++;
                        if (guard == 9999)
                        {
                            //Loop parapet
                            string msgException = "Underlyer Valuation exercise Dates exception:" + Cst.CrLf;
                            msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                            msgException += "Please, verify commencement and expiry dates, business day adjustment on the trade";
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                        }
                        adjustable_ValuationDate = new EFS_AdjustableDate(m_Cs, dtValuationDate.AddDays(1), bda, marketScheduleTradingDay.dayType, m_DataDocument);
                        lstValuationDates.Add(UnderlyerValuationDate(pUnderlyer, adjustable_ValuationDate));
                        dtValuationDate = adjustable_ValuationDate.adjustedDate.DateValue;
                    }
                    #endregion American/European exercise
                }
                if (0 < lstValuationDates.Count)
                    valuationDates = lstValuationDates.ToArray();
            }
        }
        #endregion CalculExerciseValuationDates
        #region SetPeriodExerciseDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
        protected void SetPeriodExerciseDates(EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod)
        {
            IBusinessDayAdjustments bda;
            if (DtFunc.IsDateTimeFilled(pStartPeriod.AdjustableDate.UnadjustedDate.DateValue))
            {
                bda = marketScheduleTradingDay.GetBusinessDayAdjustments(pStartPeriod.AdjustableDate.DateAdjustments.BusinessDayConvention);
                if (null != bda)
                    commencementDate = new EFS_AdjustableDate(m_Cs, pStartPeriod.AdjustableDate.UnadjustedDate.DateValue, bda, marketScheduleTradingDay.dayType, m_DataDocument);
            }
            if (DtFunc.IsDateTimeFilled(pEndPeriod.AdjustableDate.UnadjustedDate.DateValue))
            {
                bda = marketScheduleTradingDay.GetBusinessDayAdjustments(pEndPeriod.AdjustableDate.DateAdjustments.BusinessDayConvention);
                if (null != bda)
                {
                    expiryDate = new EFS_AdjustableDate(m_Cs, pEndPeriod.AdjustableDate.UnadjustedDate.DateValue, bda, marketScheduleTradingDay.dayType, m_DataDocument);
                    exerciseDate = new EFS_AdjustableDate(m_Cs, pEndPeriod.AdjustableDate.UnadjustedDate.DateValue, bda, marketScheduleTradingDay.dayType, m_DataDocument);
                }
            }
        }
        #endregion SetPeriodExerciseDates
        #region UnderlyerValuationDate
        protected EFS_ValuationDates UnderlyerValuationDate(EFS_Underlyer pUnderlyer, EFS_AdjustableDate pValuationDates)
        {
            if (pUnderlyer is EFS_SingleUnderlyer underlyer)
                return new EFS_SingleUnderlyerValuationDates(m_Cs, underlyer, pValuationDates);
            else if (pUnderlyer is EFS_Basket basket)
                return new EFS_BasketValuationDates(m_Cs, basket, pValuationDates);
            else if (pUnderlyer is EFS_BasketConstituent constituent)
                return new EFS_BasketConstituentValuationDates(m_Cs, constituent, pValuationDates);
            else if (pUnderlyer is EFS_BondUnderlyer underlyer1)
                return new EFS_BondUnderlyerValuationDates(m_Cs, underlyer1, pValuationDates);
            else
                return null;
        }
        #endregion UnderlyerValuationDate

        #endregion Methods
    }
    #endregion EFS_OptionExerciseDates

    #region EFS_OptionNotionalBase
    public abstract class EFS_OptionNotionalBase
    {
        #region Members
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney notionalAmount;
        #endregion Members
        #region Accessors
        #region NotionalAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal NotionalAmount
        {
            get
            {
                return this.notionalAmount.Amount;

            }
        }
        #endregion NotionalAmount
        #region NotionalAmountCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NotionalAmountCurrency
        {
            get
            {
                return this.notionalAmount.Currency;

            }
        }
        #endregion NotionalAmountCurrency
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        public EFS_OptionNotionalBase(IReference pPayerPartyReference, IReference pReceiverPartyReference, IMoney pNotionalAmount)
        {
            payerPartyReference = pPayerPartyReference;
            receiverPartyReference = pReceiverPartyReference;
            notionalAmount = pNotionalAmount;
        }
        #endregion Constructors
    }
    #endregion EFS_OptionNotionalBase
    #region EFS_OptionPremiumBase
    public abstract class EFS_OptionPremiumBase
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public bool premiumTypeSpecified;
        public PremiumTypeEnum premiumType;
        public IMoney premiumAmount;
        public bool settlementDateSpecified;
        public EFS_AdjustableDate settlementDate;
        public EFS_Date expiryDate;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;
        #endregion Members
        #region Accessors
        #region ExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpiryDate
        {
            get { return new EFS_EventDate(expiryDate.DateValue, expiryDate.DateValue); }
        }
        #endregion ExpiryDate

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region PremiumAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PremiumAmount
        {
            get
            {
                return this.premiumAmount.Amount;

            }
        }
        #endregion PremiumAmount
        #region PremiumAmountCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PremiumAmountCurrency
        {
            get
            {
                return this.premiumAmount.Currency;

            }
        }
        #endregion PremiumAmountCurrency

        #region SettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate SettlementDate
        {
            get {return new EFS_EventDate(settlementDate.UnadjustedEventDate.DateValue, settlementDate.adjustedDate.DateValue);}
        }
        #endregion SettlementDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlement.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #region AdjustedSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedSettlementDate
        {
            get {return settlementDate.AdjustedEventDate;}
        }
        #endregion AdjustedSettlementDate

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionPremiumBase(string pCS, IProductBase pProduct, INbOptionsAndNotionalBase pNbOptionsAndNotional, IPremiumBase pPremium, DataDocumentContainer pDataDocument)
        {
            m_Cs = pCS;
            m_DataDocument = pDataDocument;
            payerPartyReference = pPremium.PayerPartyReference;
            receiverPartyReference = pPremium.ReceiverPartyReference;

            if (pPremium.PaymentAmountSpecified)
                premiumAmount = pPremium.PaymentAmount;
            else if (pPremium.PricePerOptionSpecified && pNbOptionsAndNotional.NumberOfOptionsSpecified)
            {
                decimal result = pPremium.PricePerOption.Amount.DecValue * pNbOptionsAndNotional.NumberOfOptions.DecValue;
                premiumAmount = pProduct.CreateMoney(result, pPremium.PricePerOption.Currency);
            }
            else if (pPremium.PercentageOfNotionalSpecified && pNbOptionsAndNotional.NotionalSpecified)
            {
                decimal result = pNbOptionsAndNotional.Notional.Amount.DecValue * pPremium.PercentageOfNotional.DecValue;
                premiumAmount = pProduct.CreateMoney(result, pNbOptionsAndNotional.Notional.Currency);
            }
        }
        #endregion Constructors
        #region Methods
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected void SetPreSettlement()
        {
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, premiumAmount.Currency,
                payerPartyReference.HRef, receiverPartyReference.HRef, m_DataDocument, ((IMoney)premiumAmount).DefaultOffsetPreSettlement);
            preSettlementSpecified = (preSettlementInfo.IsUsePreSettlement);
            if (preSettlementSpecified)
                preSettlement = new EFS_PreSettlement(m_Cs, null, settlementDate.adjustedDate.DateValue, premiumAmount.Currency, preSettlementInfo.OffsetPreSettlement, m_DataDocument);
        }
        #endregion Methods
    }
    #endregion EFS_OptionPremiumBase
    #region EFS_OptionStrikeBase
    public abstract class EFS_OptionStrikeBase
    {
        #region Members
        public string cs;
        public DataDocumentContainer m_DataDocument;
        public bool strikePriceSpecified;
        public EFS_Decimal strikePrice;
        public bool spotPriceSpecified;
        public EFS_Decimal spotPrice;
        public bool strikeAmountSpecified;
        public IMoney strikeAmount;
        public bool strikePercentageSpecified;
        public EFS_Decimal strikePercentage;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public EFS_OptionStrikeBase()
        {
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_OptionStrikeBase(string pCS, DataDocumentContainer pDataDocument)
        {
            cs = pCS;
            m_DataDocument = pDataDocument;
        }
        #endregion Constructors
    }
    #endregion EFS_OptionStrikeBase

    #region EFS_ValuationDates
    public abstract class EFS_ValuationDates
    {
        #region Members
        protected string cs;
        public EFS_AdjustableDate valuationDate;
        public EFS_Underlyer underlyer;
        #endregion Members
        #region Accessors
        #region AdjustedValuationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedValuationDate
        {
            get
            {
                EFS_Date adjustedValuationDate = new EFS_Date
                {
                    DateValue = valuationDate.adjustedDate.DateValue
                };
                return adjustedValuationDate;
            }
        }
        #endregion AdjustedValuationDate
        #region UnderlyingAsset
        public object UnderlyingAsset
        {
            get { return underlyer.UnderlyingAsset; }
        }
        #endregion UnderlyingAsset
        #region ValuationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValuationDate
        {
            get { return new EFS_EventDate(valuationDate.AdjustableDate.UnadjustedDate.DateValue, valuationDate.adjustedDate.DateValue); }
        }
        #endregion ValuationDate
        #region UnderlyerEventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnderlyerEventType
        {
            get { return underlyer.UnderlyerEventType; }
        }
        #endregion UnderlyerEventType
        #endregion Accessors
        #region Constructors
        public EFS_ValuationDates(string pConnectionString, EFS_Underlyer pUnderlyer, EFS_AdjustableDate pValuationDate)
        {
            cs = pConnectionString;
            valuationDate = pValuationDate;
            underlyer = pUnderlyer;
        }
        #endregion Constructors
    }
    #endregion EFS_ValuationDates
    #region EFS_BasketValuationDates
    public class EFS_BasketValuationDates : EFS_ValuationDates
    {
        #region Members
        public bool basketConstituentValuationDatesSpecified;
        public EFS_BasketConstituentValuationDates[] basketConstituentValuationDates;
        #endregion Members
        #region Constructors
        public EFS_BasketValuationDates(string pConnectionString, EFS_Basket pBasket, EFS_AdjustableDate pValuationDate)
            : base(pConnectionString, pBasket, pValuationDate)
        {
            basketConstituentValuationDatesSpecified = pBasket.basketConstituentSpecified;
            if (basketConstituentValuationDatesSpecified)
            {
                ArrayList aBasketConstituentValuationDates = new ArrayList();
                foreach (EFS_BasketConstituent constituent in pBasket.basketConstituent)
                {
                    aBasketConstituentValuationDates.Add(
                        new EFS_BasketConstituentValuationDates(pConnectionString, constituent, pValuationDate));
                }

                if (0 < aBasketConstituentValuationDates.Count)
                {
                    basketConstituentValuationDates =
                        (EFS_BasketConstituentValuationDates[])aBasketConstituentValuationDates.ToArray(typeof(EFS_BasketConstituentValuationDates));
                }
            }
        }
        #endregion Constructors
    }
    #endregion EFS_BasketValuationDates
    #region EFS_BasketConstituentValuationDates
    public class EFS_BasketConstituentValuationDates : EFS_ValuationDates
    {
        #region Constructors
        public EFS_BasketConstituentValuationDates(string pConnectionString, EFS_Underlyer pUnderlyer, EFS_AdjustableDate pValuationDate)
            : base(pConnectionString, pUnderlyer, pValuationDate) { }
        #endregion Constructors
    }
    #endregion EFS_BasketConstituentValuationDates
    #region EFS_SingleUnderlyerValuationDates
    public class EFS_SingleUnderlyerValuationDates : EFS_ValuationDates
    {
        #region Constructors
        public EFS_SingleUnderlyerValuationDates(string pConnectionString, EFS_SingleUnderlyer pSingleUnderlyer, EFS_AdjustableDate pValuationDate)
            : base(pConnectionString, pSingleUnderlyer, pValuationDate)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_SingleUnderlyerValuationDates
}
