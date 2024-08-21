using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;

namespace EfsML
{
    /// <summary>
    /// Classe de base pour calcul de dates (EXP, LTD, DELIVERY, etc..) à partir d'une MR 
    /// </summary>
    public class CalcMaturityRuleDateBase
    {
        protected readonly string _cs;
        protected readonly (int Id, string IdBC) _market;
        protected readonly IProductBase _product;
        protected readonly MaturityRule _maturityRule;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pMarket"></param>
        /// <param name="pMaturityRule"></param>
        public CalcMaturityRuleDateBase(string pCS, IProductBase pProduct, (int Id, string IdBC) pMarket, MaturityRule pMaturityRule)
        {
            _cs = pCS;
            _market = pMarket;
            _product = pProduct;
            _maturityRule = pMaturityRule;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IBusinessDayAdjustments GetBDAForOffset()
        {
            IBusinessDayAdjustments ret = null;

            if (StrFunc.IsFilled(_market.IdBC) || (_maturityRule.IsApplyMaturityBusinessCenterOnOffsetCalculation && StrFunc.IsFilled(_maturityRule.MaturityBusinessCenter)))
            {
                ret = _product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE,
                    _market.IdBC,
                    _maturityRule.IsApplyMaturityBusinessCenterOnOffsetCalculation ? _maturityRule.MaturityBusinessCenter : null);
            }

            return ret;
        }

        /// <summary>
        /// Retourne la date, "brute", relative à un nom d'échéance. 
        /// <example>
        /// YYYYMM   - 201302   --> 20130201 (1 février 2013)
        /// YYYYMMw9 - 201302w2 --> 20130208 (8 février 2013)
        /// YYYYMMDD - 20130215 --> 20130215 (15 février 2013)
        /// </example>
        /// </summary>
        /// <param name="maturityMonthYear"></param>
        /// <returns></returns>
        /// FI 20171030 [XXXXX] Modify
        protected DateTime GetDateFromMaturityMonthYear(string maturityMonthYear)
        {
            string strMaturityDate;

            if (_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthOnly)
            {
                //PL 20170711 Add substring() for HPC migration 4.5 to 6.0
                //Ex: 201302 --> 20130201 (1 février 2013)
                //strMaturityDate = pMaturityMonthYear + "01";
                strMaturityDate = maturityMonthYear.Substring(0, 6) + "01";
            }
            else if (_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthWeek)
            {
                //Ex: 201302w2 --> 20130208 (8 février 2013)
                int n = Convert.ToInt32(maturityMonthYear.Substring(7, 1));
                int day = DtFunc.GetDayOfWeek(n);

                strMaturityDate = maturityMonthYear.Substring(0, 6) + StrFunc.GetDayDD(day.ToString());
            }
            else if (_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay)
            {
                //Ex: 20130215 --> 20130215 (15 février 2013)
                strMaturityDate = maturityMonthYear;
            }
            else
            {
                throw new NotImplementedException($"format:{_maturityRule.MaturityFormatEnum} is not implemented");
            }

            if (false == StrFunc.IsDate(strMaturityDate, DtFunc.FmtDateyyyyMMdd, null, out DateTime dt))
                throw new InvalidProgramException($"yyyyMMdd:{strMaturityDate} is not a date");

            return dt;
        }
    }

    /// <summary>
    /// Classe pour calcul de dates (EXP, LTD, DELIVERY, etc..) à partir d'une MR 
    /// </summary>
    public class CalcMaturityRuleDate : CalcMaturityRuleDateBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="product"></param>
        /// <param name="market"></param>
        /// <param name="maturityRule"></param>
        public CalcMaturityRuleDate(string cs, IProductBase product, (int Id, string IdBC) market, MaturityRule maturityRule) :
            base(cs, product, market, maturityRule)
        {

        }

        /// <summary>
        /// Calcul de la MaturityDate de l'échéance <paramref name="maturityMonthYear"/> ainsi que sa date forcée.
        /// </summary>
        /// <param name="maturityMonthYear"></param>
        /// <param name="oDtRolledDate">Retourne la date obtenue après application de la roll Convention (alimentée si format YYYYMM ou YYYYMMwN) ou Retourne DateTime.MinValue</param>
        /// <returns></returns>
        public (DateTime MaturityDateSys, DateTime MaturityDate) Calc_MaturityDate(string maturityMonthYear, out DateTime oDtRolledDate)
        {
            oDtRolledDate = DateTime.MinValue;

            CalcMaturityDateBase calcMaturity;
            switch (_maturityRule.MaturityFormatEnum)
            {
                case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                    calcMaturity = new CalcMaturityDateLongFormat(_cs, _product, _market, _maturityRule);
                    break;
                case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                case Cst.MaturityMonthYearFmtEnum.YearMonthWeek:
                    calcMaturity = new CalcMaturityDateShortFormat(_cs, _product, _market, _maturityRule);
                    break;
                default:
                    throw new NotSupportedException($"format:{_maturityRule.MaturityFormatEnum} not supported");
            }

            DateTime dtMaturitySys = calcMaturity.CalcDate(maturityMonthYear, out oDtRolledDate);
            DateTime dtMaturity = calcMaturity.CalcDateForced(dtMaturitySys);

            return (dtMaturitySys, dtMaturity);
        }

        /// <summary>
        /// Calcul du Last Trading Day à partir de la date d'échéance <paramref name="dtMaturityDateSys"/>. La date est calculée en appliquant à <paramref name="dtMaturityDateSys"/> le décalage spécifié sur la MaturityRule. Retourne DateTime.MinValue lorsque le paramétrage est absent.
        /// </summary>
        /// <param name="dtMaturityDateSys">Date d'échéance réelle</param>
        /// <param name="dtRolledDate">Date obtenue après application de la rollconvention (Doit être renseignée si rollConventionEnum est <seealso cref="RollConventionEnum.IDEXDLV"/>) </param>
        public DateTime Calc_MaturityLastTradingDay(DateTime dtMaturityDateSys, DateTime dtRolledDate)
        {
            DateTime ret = DateTime.MinValue;

            if (_maturityRule.IsExistLastTrdDayOffset)
            {
                DateTime dtStart = dtMaturityDateSys;
                if (StrFunc.IsFilled(_maturityRule.MaturityRollConv))
                {
                    // On applique la "RollConv"
                    RollConventionEnum rollConventionEnum = StringToEnum.RollConvention(_maturityRule.MaturityRollConv);
                    if (rollConventionEnum == RollConventionEnum.IDEXDLV)
                        dtStart = dtRolledDate;
                }

                CalcLTDOffset offset = new CalcLTDOffset(_cs, _product, _market, _maturityRule);
                ret = offset.Calc(dtStart, isModeReverse: false);
            }
            return ret;
        }

        /// <summary>
        /// Calcul de la Delivery Date d'une Maturity (NO PERIODIC DELIVERY)
        /// <para>- par application d'un offset (MaturityDeliveryDateOffsetMultiplier, MaturityDeliveryDateOffsetPeriod et MaturityDeliveryDateOffsetDaytype)</para>
        /// <para>- sur la base du BC spécifié sur le MARKET</para>
        /// </summary>
        /// <param name="maturity">Date d'échéance et date d'échéance éventuellement forcée</param>
        public DateTime Calc_MaturityDeliveryDate((DateTime MaturityDateSys, DateTime MaturityDate) maturity)
        {

            DateTime ret = DateTime.MinValue;

            PeriodEnum period = StringToEnum.Period(_maturityRule.MaturityDelivryDateOffsetPeriod);
            DayTypeEnum dayType = StringToEnum.DayType(_maturityRule.MaturityDelivryDateOffsetDaytype);

            IBusinessDayAdjustments bda = GetBDAForOffset();

            // RD 20191031 [25022] Use Convert.ToInt32
            // PL 20211019 [25022] Peut-être eût été t'il préférable de faire comme pour le LTD, à savoir ne pas opérer de calcul quand le paramétrage est incomplet. Plutôt que considérer ZERO quand non renseigné. 
            //IOffset offset = pProduct.CreateOffset(period, (int)pSqlMaturityRule.MaturityDelivryDateOffsetMultiplier, dayType);
            IOffset offset = _product.CreateOffset(period, Convert.ToInt32(_maturityRule.MaturityDelivryDateOffsetMultiplier), dayType);

            EFS_Offset efsOffset = new EFS_Offset(_cs, offset, maturity.MaturityDateSys, bda, null as DataDocumentContainer);
            if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
            {
                ret = efsOffset.offsetDate[0];
            }

            //WARNING: cas d'une MaturityDate forcée (see Calc_MaturityDate_WhenISSHIFTEDMATURITY)
            //         Si la MaturityDate forcée est postérieure à la DeliveryDate calculée, on recalcule cette dernière sur la base de MaturityDate forcée.
            if ((maturity.MaturityDate != maturity.MaturityDateSys) && (ret.CompareTo(maturity.MaturityDate) < 0))
            {
                efsOffset = new EFS_Offset(_cs, offset, maturity.MaturityDate, bda, null as DataDocumentContainer);
                if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                {
                    ret = efsOffset.offsetDate[0];
                }
            }

            return ret;
        }

        /// <summary>
        /// Calcul des First/Last Settlement/Delivery Dates d'une Maturity (PERIODIC DELIVERY)
        /// </summary>
        /// <param name="maturityMonthYear">Représente le nom de l'échéance</param>
        public MaturityPeriodicDeliveryCharacteristics Calc_MaturityPeriodicDeliveryDates(string maturityMonthYear)
        {

            MaturityPeriodicDeliveryCharacteristics ret = new MaturityPeriodicDeliveryCharacteristics();

            // MaturityFirstDeliveryDate et MaturityLastDeliveryDate sont (actuellement) calculées à partir du mois de l'échéance 
            Cst.MaturityMonthYearFmtEnum maturityFormat = _maturityRule.MaturityFormatEnum;

            if (maturityFormat != Cst.MaturityMonthYearFmtEnum.YearMonthDay)
            {
                DateTime dtBasis = GetDateFromMaturityMonthYear(maturityMonthYear);

                string[] idbc = new string[1];

                #region First/Last Delivery Date
                RollConventionEnum rollConventionEnum = StringToEnum.RollConvention(_maturityRule.MaturityPeriodicFirstDelivryDateRollConv);
                EFS_RollConvention rollConvention = EFS_RollConvention.GetNewRollConvention(_cs, rollConventionEnum, dtBasis, default, idbc);
                ret.dates.dtFirstDelivery = rollConvention.rolledDate;
                if (rollConvention.errLevel == Cst.ErrLevel.SUCCESS)
                {
                    rollConventionEnum = StringToEnum.RollConvention(_maturityRule.MaturityPeriodicLastDelivryDateRollConv);
                    rollConvention = EFS_RollConvention.GetNewRollConvention(_cs, rollConventionEnum, dtBasis, default, idbc);
                    ret.dates.dtLastDelivery = rollConvention.rolledDate;
                }
                if (rollConvention.errLevel != Cst.ErrLevel.SUCCESS)
                    //Date invalide: ex. Roll conv. 5THFRI (5ème vendredi) sur un mois ne comportant que 4 vendredi
                    throw new Exception($"DelivryDate Invalid. Rollconvention: {rollConventionEnum}, date: {DtFunc.DateTimeToStringDateISO(dtBasis)}");

                #endregion First/Last Delivery Date

                #region First/Last Settlement Date
                PeriodEnum period = StringToEnum.Period(_maturityRule.MaturityPeriodicDlvSettltDateOffsetPeriod);
                DayTypeEnum dayType = StringToEnum.DayType(_maturityRule.MaturityPeriodicDlvSettltDateOffsetDaytype);
                IOffset offset = _product.CreateOffset(period, (int)_maturityRule.MaturityPeriodicDlvSettltDateOffsetMultiplier, dayType);

                IBusinessDayAdjustments bda = GetBDAForOffset();
                // EG 20190128 [24361] Step 2
                bda.IsSettlementOfHolidayDeliveryConvention = true;
                bda.BusinessDayConvention = StringToEnum.BusinessDayConvention(_maturityRule.MaturityPeriodicDlvSettltHolidayConv);

                EFS_Offset efsOffset = new EFS_Offset(_cs, offset, Convert.ToDateTime(ret.dates.dtFirstDelivery), bda, null as DataDocumentContainer);
                if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                {
                    ret.dates.dtFirstDlvSettlt = efsOffset.offsetDate[0];
                }
                efsOffset = new EFS_Offset(_cs, offset, Convert.ToDateTime(ret.dates.dtLastDelivery), bda, null as DataDocumentContainer);
                if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                {
                    ret.dates.dtLastDlvSettlt = efsOffset.offsetDate[0];
                }
                #endregion First/Last Settlement Date
            }
            else
            {
                //PL 20170217 TBD...
            }

            return ret;
        }

        /// <summary>
        ///  Applique à la date <paramref name="date"/> Le décalage présent sur la MaturityRule 
        ///  <para>s'il n'existe pas de décalage, la date est retournée inchangée</para>
        /// </summary>
        /// <param name="date"></param>
        /// <param name="isModeReverse">si true, le décalage est inverse</param>
        /// <returns></returns>
        /// FI 20240524 [WI942] Add Method
        public DateTime Calc_Offset(DateTime date, Boolean isModeReverse)
        {
            CalcOffset calcOffset = new CalcOffset(_cs, _product, _market, _maturityRule);
            DateTime ret = calcOffset.Calc(date, isModeReverse);
            return ret;
        }

        /// <summary>
        /// Classe de base pour calcul d'une date d'échéance (EXP) à partir d'une MR et d'un nom d'échéance
        /// </summary>
        private class CalcMaturityDateBase : CalcMaturityRuleDateBase
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cs"></param>
            /// <param name="product"></param>
            /// <param name="market"></param>
            /// <param name="maturityRule"></param>
            public CalcMaturityDateBase(string cs, IProductBase product, (int Id, string IdBC) market, MaturityRule maturityRule) :
                base(cs, product, market, maturityRule)
            {

            }

            /// <summary>
            /// Retourne la date d'échéance à partir du nom d'échéance <paramref name="maturityMonthYear"/>
            /// </summary>
            /// <param name="maturityMonthYear">Représente le nom d'échéance</param>
            /// <param name="dtRolledDate">différent DateTime.MinValue si Format YYYYMMDD uniquement</param>
            public virtual DateTime CalcDate(string maturityMonthYear, out DateTime dtRolledDate)
            {
                throw new InvalidOperationException("CalcDate is not overrided");
            }

            /// <summary>
            /// Spécificité introduite pour XChanging: Ajout de +1 JO Bourse/Clearing sur la base du BC du MARKET
            /// <para>NB: cette fonctionnalité est introduite en STANDARD, afin que tout client Spheres® puisse en bénéficier, et que l'offset puisse être si besoin amendé.</para>
            /// </summary>
            /// <param name="maturiyDateSys">date d'échéance système</param>
            /// 
            public DateTime CalcDateForced(DateTime maturiyDateSys)
            {
                DateTime ret = maturiyDateSys;

                if (DtFunc.IsDateTimeFilled(ret))
                {
                    const string @TRUE = "TRUE";
                    const string ISSHIFTEDMATURITY = "ISSHIFTEDMATURITY";

                    string sqlQuery = SQLCst.SELECT + "1" + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EXTLID.ToString() + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(Cst.OTCml_TBL.MARKET.ToString());
                    sqlQuery += " and ID=@ID and IDENTIFIER=" + DataHelper.SQLString(ISSHIFTEDMATURITY) + " and VALUE=" + DataHelper.SQLString(@TRUE);
                    sqlQuery += " and IDAINS>0"; //Tip: pour, si nécessaire, pouvoir aisément débrayer le processus.

                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(_cs, "ID", DbType.Int32), _market.Id);
                    object obj = DataHelper.ExecuteScalar(_cs, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
                    if (obj != null)
                    {
                        //Ajout de +1 JO Bourse/Clearing sur la base du BC du MARKET
                        IOffset offset = _product.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
                        IBusinessDayAdjustments bda = _product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE, _market.IdBC);
                        EFS_Offset efsOffset = new EFS_Offset(_cs, offset, ret, bda, null as DataDocumentContainer);
                        if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                        {
                            ret = efsOffset.offsetDate[0];
                        }
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Classe pour calcul d'une date d'échéance à partir d'une MR au format YYYYMMDD et d'un nom d'échéance
        /// </summary>
        private class CalcMaturityDateLongFormat : CalcMaturityDateBase
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="cs"></param>
            /// <param name="product"></param>
            /// <param name="market"></param>
            /// <param name="maturityRule"></param>
            public CalcMaturityDateLongFormat(string cs, IProductBase product, (int Id, string IdBC) market, MaturityRule maturityRule)
                : base(cs, product, market, maturityRule)
            {
                if (!(_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay))
                    throw new InvalidOperationException($"Format : {maturityRule.MaturityFormatEnum} not valid");
            }

            /// <summary>
            /// Retourne la date d'échéance à partir du nom d'échéance <paramref name="maturityMonthYear"/>
            /// </summary>
            /// <param name="maturityMonthYear">nom de l'échéance</param>
            /// <param name="dtRolledDate">retourne DateTime.MinValue</param>
            public override DateTime CalcDate(string maturityMonthYear, out DateTime dtRolledDate)
            {
                dtRolledDate = DateTime.MinValue;
                Cst.MaturityRelativeTo maturityRelativeTo = _maturityRule.MaturityRelativeTo ?? Cst.MaturityRelativeTo.EXP;
                DateTime ret;
                switch (maturityRelativeTo)
                {
                    case Cst.MaturityRelativeTo.EXP:
                        // maturityMonthYear est relatif à une date d'échéance => convertion maturityMonthYear en date pour obtenir la date d'échéance
                        ret = GetDateFromMaturityMonthYear(maturityMonthYear);
                        break;
                    case Cst.MaturityRelativeTo.LTD:
                        // maturityMonthYear est relatif à une date LTD => convertion maturityMonthYear en date pour obtenir la LTD et application de l’offset paramétré en sens inverse
                        CalcLTDOffset offset = new CalcLTDOffset(_cs, _product, _market, _maturityRule);
                        ret = offset.Calc(GetDateFromMaturityMonthYear(maturityMonthYear), isModeReverse: true);
                        break;
                    default:
                        throw new NotSupportedException($"MaturityRelativeTo: {_maturityRule.MaturityRelativeTo.Value} is not supported");
                }

                return ret;
            }
        }

        /// <summary>
        /// Classe pour calcul d'une date d'échéance à partir d'une MR au format court (YYYYMM ou YYYYMMwN) et d'un nom d'échéance
        /// </summary>
        private class CalcMaturityDateShortFormat : CalcMaturityDateBase
        {
            public CalcMaturityDateShortFormat(string cs, IProductBase product, (int Id, string IdBC) market, MaturityRule maturityRule)
                : base(cs, product, market, maturityRule)
            {

                if (!((_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthOnly) ||
                      (_maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthWeek)))
                    throw new InvalidOperationException($"Format : {maturityRule.MaturityFormatEnum} not valid");
            }

            /// <summary>
            /// Retourne la date d'échéance à partir du nom d'échéance <paramref name="maturityMonthYear"/>
            /// </summary>
            /// <param name="maturityMonthYear">nom de l'échéance</param>
            /// <param name="dtRolledDate">Obtient la RolledDate</param>
            public override DateTime CalcDate(string maturityMonthYear, out DateTime dtRolledDate)
            {
                #region Descriptif de l'algorithme de calcul des Dates
                //      +Fabriquer la date d'échéance en appliquant le mois de référence et la "RollConv"
                //          - Si "RollConv" non spécifié alors Error
                //      + Appliquer le premier ajustement (Nouveauté 2013)
                //          - Si une nouvelle date d'échéance est bien calculée à l'étape précédente
                //          - Si un ajustement est spécifié pour la date d'échéance (MaturityRollConvBusinessDayConv)
                //          - En utilisant le BC spécifié sur MARKET 
                //      + Appliquer le décalage 
                //          - Si une nouvelle date d'échéance est bien calculée à l'étape précédente
                //          - Si un décalage est spécifié pour la date d'échéance (MaturityOffsetMultiplier, MaturityOffsetPeriod et MaturityOffsetDaytype)
                //          - En utilisant le BC spécifié sur MARKET 
                //      + Appliquer l'ajustement
                //          - Si une nouvelle date d'échéance est bien calculée aux étapes précédentes (en sachant que le décalage n’est pas obligatoire)
                //          - Si un ajustement est spécifié pour la date d'échéance (MaturityBusinessDayConv)

                //          - Un premier ajustement est appliqué en utilisant le BC de Market (s’il est spécifié), en incluant les jours fériés de type ScheduledTradingDay (//PL 20131121 Previously "ExchangeBusiness")
                //              * si aucun décalage n'est spécifié 
                //              * ou bien le décalage spécifié est de type Calendaire
                //          - Un deuxième ajustement est appliqué en utilisant le BC Additionnel de MaturityRule (s’il est spécifié), en incluant les jours fériés de type Business (donc Banking ou CurrencyBusiness).

                //          - On boucle sur l'application du premier ajustement (sans les deux conditions), ensuite le deuxième, jusqu'à trouver une date ouvrée sur les deux calendriers.
                #endregion

                dtRolledDate = DateTime.MinValue;

                DateTime ret = GetDateFromMaturityMonthYear(maturityMonthYear);

                #region 1- Calcul dtMaturityDate : Application de la "RollConv"
                if (!String.IsNullOrEmpty(_maturityRule.MaturityMonthReference))
                    ret = MaturityRuleHelper.ApplyMonthReference(ret, _maturityRule.MaturityMonthReference);

                RollConventionEnum rollConventionEnum = RollConventionEnum.NONE;
                if (StrFunc.IsFilled(_maturityRule.MaturityRollConv))
                {
                    // On applique la "RollConv"
                    rollConventionEnum = StringToEnum.RollConvention(_maturityRule.MaturityRollConv);

                    // MF 20120511 Ticket 17776
                    //EFS_RollConvention rollConvention = new EFS_RollConvention(rollConventionEnum, dtMaturityDate);
                    //EFS_RollConvention rollConvention = EFS_RollConvention.GetNewRollConvention(pCS, rollConventionEnum, dtMaturityDate, default, 2, pMarketBusinessCenter);
                    //FL/PL 20120704 Newness
                    List<string> idbc = new List<string> { _market.IdBC };
                    if (_maturityRule.IsApplyMaturityBusinessCenterOnOffsetCalculation && StrFunc.IsFilled(_maturityRule.MaturityBusinessCenter))
                        idbc.Add(_maturityRule.MaturityBusinessCenter);

                    EFS_RollConvention rollConvention = EFS_RollConvention.GetNewRollConvention(_cs, rollConventionEnum, ret, default, idbc.ToArray());
                    dtRolledDate = rollConvention.rolledDate;
                    ret = dtRolledDate;
                }
                else
                    ret = DateTime.MinValue;

                //PL 20130215 Newness BDC_ROLLCONV_MDT
                if (DtFunc.IsDateTimeFilled(ret) &&
                    (_maturityRule.MaturityRollConvBusinessDayConv != BusinessDayConventionEnum.NONE) && (!String.IsNullOrEmpty(_market.IdBC)))
                {
                    IBusinessDayAdjustments bda = _product.CreateBusinessDayAdjustments(_maturityRule.MaturityRollConvBusinessDayConv, _market.IdBC);
                    //PL 20131121
                    //EFS_AdjustableDate adjDate = new EFS_AdjustableDate(pCS, dtMaturityDate, marketBDAdj, DayTypeEnum.ExchangeBusiness);
                    EFS_AdjustableDate adjDate = new EFS_AdjustableDate(_cs, ret, bda, DayTypeEnum.ScheduledTradingDay, null);
                    if (DtFunc.IsDateTimeFilled(adjDate.adjustedDate.DateValue) &&
                        ret.CompareTo(adjDate.adjustedDate.DateValue) != 0)
                    {
                        ret = adjDate.adjustedDate.DateValue;
                    }
                }
                #endregion

                // 2- Calcul dtMaturityDate : Application du décalage
                // Si une nouvelle date d'échéance est bien calculée à l'étape précédente
                bool isToOffsetMaturityDate = (DtFunc.IsDateTimeFilled(ret) &&
                    ((_maturityRule.MaturityOffsetMultiplier.HasValue) &&
                    StrFunc.IsFilled(_maturityRule.MaturityOffsetPeriod) &&
                    StrFunc.IsFilled(_maturityRule.MaturityOffsetDaytype)));

                if (isToOffsetMaturityDate)
                {
                    /// FI 20240524 [WI942] Utilisation de CalcOffset
                    CalcOffset calcOffset = new CalcOffset(_cs, _product, _market, _maturityRule);
                    ret = calcOffset.Calc(ret, false);
                }

                // On applique l'ajustement:
                // 1- Si une nouvelle date d'échéance est bien calculée aux étapes précédentes (le décalage n’est pas obligatoire)
                // 2- Si un ajustement est spécifié pour la date d'échéance(BusinessDayConv)

                if (DtFunc.IsDateTimeFilled(ret) && StrFunc.IsFilled(_maturityRule.MaturityBusinessDayConv))
                {
                    #region 3- Calcul dtMaturityDate : Application de l'ajustement
                    BusinessDayConventionEnum maturityRuleBDC = BusinessDayConventionEnum.NONE;
                    if (StrFunc.IsFilled(_maturityRule.MaturityBusinessDayConv))
                    {
                        maturityRuleBDC = (BusinessDayConventionEnum)System.Enum.Parse(typeof(BusinessDayConventionEnum), _maturityRule.MaturityBusinessDayConv);
                    }

                    IBusinessDayAdjustments maturityRuleBDAdj = null;
                    //FL/PL 20120704
                    //if (StrFunc.IsFilled(pMaturityRule.MaturityBusinessCenter))
                    if ((!_maturityRule.IsApplyMaturityBusinessCenterOnOffsetCalculation) && StrFunc.IsFilled(_maturityRule.MaturityBusinessCenter))
                    {
                        maturityRuleBDAdj = _product.CreateBusinessDayAdjustments(maturityRuleBDC, _maturityRule.MaturityBusinessCenter);
                    }

                    IBusinessDayAdjustments marketBDAdj = null;
                    if (StrFunc.IsFilled(_market.IdBC))
                    {
                        marketBDAdj = _product.CreateBusinessDayAdjustments(maturityRuleBDC, _market.IdBC);
                    }

                    // Le but est de trouver une Date ouvrée sur les deux calendriers 
                    // - Si un BC est spécifié sur MARKET : avec les jours fériés de type ScheduledTradingDay (//PL 20131121 Previously "ExchangeBusiness")
                    // - Si un BC est spécifié sur MATURITYRULE : avec les jours fériés de type Business (donc Banking ou CurrencyBusiness)
                    // 
                    // - Un premier ajustement est appliqué en utilisant le BC de Market
                    //   + si aucun décalage n'est spécifié 
                    //   + ou bien le décalage spécifié est de type Calendaire
                    //
                    // - Un deuxième ajustement est appliqué en utilisant le BC de MaturityRule
                    //
                    // - On boucle sur l'application du premier ajustement, ensuite le deuxième jusqu'à trouver une date ouvrée sur les deux.

                    bool isToAdjusteByMarketBDAdj = ((marketBDAdj != null) && ((isToOffsetMaturityDate == false) || (_maturityRule.MaturityOffsetDaytype == DayTypeEnum.Calendar.ToString())));
                    bool isToAdjusteByMaturityRuleBDAdj = (maturityRuleBDAdj != null);

                    while (isToAdjusteByMarketBDAdj || isToAdjusteByMaturityRuleBDAdj)
                    {
                        if (isToAdjusteByMarketBDAdj)
                        {
                            //PL 20131121
                            //EFS_AdjustableDate efsAdj = new EFS_AdjustableDate(pCS, dtMaturityDate, marketBDAdj, DayTypeEnum.ExchangeBusiness);
                            EFS_AdjustableDate efsAdj = new EFS_AdjustableDate(_cs, ret, marketBDAdj, DayTypeEnum.ScheduledTradingDay, null);
                            if (DtFunc.IsDateTimeFilled(efsAdj.adjustedDate.DateValue) &&
                                ret.CompareTo(efsAdj.adjustedDate.DateValue) != 0)
                            {
                                ret = efsAdj.adjustedDate.DateValue;
                                isToAdjusteByMaturityRuleBDAdj = (maturityRuleBDAdj != null);
                            }
                            isToAdjusteByMarketBDAdj = false;
                        }

                        if (isToAdjusteByMaturityRuleBDAdj)
                        {
                            EFS_AdjustableDate efsAdj = new EFS_AdjustableDate(_cs, ret, maturityRuleBDAdj, DayTypeEnum.Business, null);
                            if (DtFunc.IsDateTimeFilled(efsAdj.adjustedDate.DateValue) &&
                                ret.CompareTo(efsAdj.adjustedDate.DateValue) != 0)
                            {
                                ret = efsAdj.adjustedDate.DateValue;
                                isToAdjusteByMarketBDAdj = (marketBDAdj != null);
                            }
                            isToAdjusteByMaturityRuleBDAdj = false;
                        }
                    }
                    #endregion
                }

                return ret;
            }
        }


        /// <summary>
        /// Gestion de l'application de l'offset associé à la LTD
        /// </summary>
        private class CalcLTDOffset : CalcMaturityRuleDateBase
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pProduct"></param>
            /// <param name="pMarket"></param>
            /// <param name="pMaturityRule"></param>
            public CalcLTDOffset(string pCS, IProductBase pProduct, (int Id, string IdBC) pMarket, MaturityRule pMaturityRule) :
                base(pCS, pProduct, pMarket, pMaturityRule)
            {

            }

            /// <summary>
            ///  Applique l'offset LDT à la date <paramref name="dtStart"/>
            /// </summary>
            /// <param name="dtStart"></param>
            /// <param name="isModeReverse">si true Application de l'offset en sens inverse</param>
            /// <returns></returns>
            public DateTime Calc(DateTime dtStart, Boolean isModeReverse)
            {
                DateTime ret = DateTime.MinValue;
                if (_maturityRule.IsExistLastTrdDayOffset)
                {
                    IBusinessDayAdjustments bda = GetBDAForOffset();

                    int multipler = _maturityRule.MaturityLastTrdDayOffsetMultiplier.Value;
                    if (isModeReverse)
                        multipler *= -1;

                    PeriodEnum period = StringToEnum.Period(_maturityRule.MaturityLastTrdDayOffsetPeriod);
                    DayTypeEnum dayType = StringToEnum.DayType(_maturityRule.MaturityLastTrdDayOffsetDaytype);
                    IOffset offset = _product.CreateOffset(period, multipler, dayType);

                    EFS_Offset efsOffset = new EFS_Offset(_cs, offset, dtStart, bda, null as DataDocumentContainer);

                    if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                    {
                        ret = efsOffset.offsetDate[0];
                    }
                }
                return ret;
            }

        }


        /// <summary>
        /// Gestion de l'application de l'offset 
        /// </summary>
        /// FI 20240524 [WI942]
        private class CalcOffset : CalcMaturityRuleDateBase
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pProduct"></param>
            /// <param name="pMarket"></param>
            /// <param name="pMaturityRule"></param>
            public CalcOffset(string pCS, IProductBase pProduct, (int Id, string IdBC) pMarket, MaturityRule pMaturityRule) :
                base(pCS, pProduct, pMarket, pMaturityRule)
            {

            }

            /// <summary>
            /// Application du décalage présent sur la MR
            /// <para>Si un décalage est spécifié pour la date d'échéance(OffsetMultiplier, OffsetPeriod et OffsetDaytype) (Utilisation du BC spécifié sur MARKET)</para>
            /// </summary>
            /// <param name="date"></param>
            /// <param name="isModeReverse">si true, le décalage est inverse</param>
            /// <returns></returns>
            public DateTime Calc(DateTime date, Boolean isModeReverse)
            {
                DateTime ret = date;

                bool isToOffsetMaturityDate = (DtFunc.IsDateTimeFilled(ret) &&
                        ((_maturityRule.MaturityOffsetMultiplier.HasValue) &&
                        StrFunc.IsFilled(_maturityRule.MaturityOffsetPeriod) &&
                        StrFunc.IsFilled(_maturityRule.MaturityOffsetDaytype)));

                if (isToOffsetMaturityDate)
                {

                    int multipler = _maturityRule.MaturityOffsetMultiplier.Value;
                    if (isModeReverse)
                        multipler *= -1;

                    IOffset offset = _product.CreateOffset(StringToEnum.Period(_maturityRule.MaturityOffsetPeriod), multipler, StringToEnum.DayType(_maturityRule.MaturityOffsetDaytype));

                    IBusinessDayAdjustments bda = GetBDAForOffset();

                    // RD 20110419 [17414]
                    // Pour appliquer en cas de décalage BUSINESS, un ajustement PRECEDING ou FOLLOWING en fonction du signe de l’offset, avant d’appliquer l’offset.
                    //PL 20130215 Newness BDC_ROLLCONV_MDT (see above)
                    //EFS_Offset efsOffset = new EFS_Offset(pCS, offset, Convert.ToDateTime(dtMaturityDate), pMarketBDAOffset, true);
                    EFS_Offset efsOffset = new EFS_Offset(_cs, offset, ret, bda, null as DataDocumentContainer);

                    if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                    {
                        ret = efsOffset.offsetDate[0];

                        //FL/PL 20120703 Newness: Ré-application de la RollConvention si décalage M ou Y, et si RollConvention SECONDTHU, THIRDTHU, ...
                        if ((offset.Period == PeriodEnum.M) || (offset.Period == PeriodEnum.Y))
                        {
                            RollConventionEnum rollConventionEnum = RollConventionEnum.NONE;
                            if (StrFunc.IsFilled(_maturityRule.MaturityRollConv))
                                rollConventionEnum = StringToEnum.RollConvention(_maturityRule.MaturityRollConv);

                            //PL 20160225 F&Oml Add xxxMON
                            if (rollConventionEnum == RollConventionEnum.EOM
                                || rollConventionEnum == RollConventionEnum.FIRSTMON || rollConventionEnum == RollConventionEnum.SECONDMON
                                || rollConventionEnum == RollConventionEnum.THIRDMON || rollConventionEnum == RollConventionEnum.FOURTHMON
                                || rollConventionEnum == RollConventionEnum.FIFTHMON
                                || rollConventionEnum == RollConventionEnum.SECONDTHU || rollConventionEnum == RollConventionEnum.THIRDTHU
                                || rollConventionEnum == RollConventionEnum.FOURTHTHU
                                || rollConventionEnum == RollConventionEnum.FIRSTFRI || rollConventionEnum == RollConventionEnum.SECONDFRI
                                || rollConventionEnum == RollConventionEnum.THIRDFRI || rollConventionEnum == RollConventionEnum.FOURTHFRI
                                || rollConventionEnum == RollConventionEnum.FIFTHFRI)
                            {
                                // MF 20120511 Ticket 17776
                                //EFS_RollConvention rollConvention = new EFS_RollConvention(rollConventionEnum, dtMaturityDate);
                                EFS_RollConvention rollConvention = EFS_RollConvention.GetNewRollConvention(_cs, rollConventionEnum, ret, default, new string[] { _market.IdBC });

                                ret = rollConvention.rolledDate;
                            }
                        }
                    }
                }
                return ret;
            }
        }
    }
}