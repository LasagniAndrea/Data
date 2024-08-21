#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Curve
{
    /// <summary>
    /// Clef pour recherche les matrices multidimentionnelles
    /// </summary>
    public class CurveParam
    {
        
        #region Accessors
        /// <summary>
        /// Obtient la date 
        /// </summary>
        public DateTime DtCalculation
        {
            private set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IdCSpecified
        {
            get { return StrFunc.IsFilled(IdC); }
        }
        /// <summary>
        /// Currency
        /// </summary>
        public string IdC
        {
            private set;
            get;
        }

        // EG 20160404 Migration vs2013
        //public bool CurveSideSpecified
        //{
        //    get { return (null != CurveSide); }
        //}
        /// <summary>
        /// Type of quotation side(optional) Ask, Bid, Mid. 
        /// </summary>		
        public QuotationSideEnum CurveSide
        {
            private set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IdMarketEnvironmentSpecified
        {
            get { return StrFunc.IsFilled(IdMarketEnvironment); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string IdMarketEnvironment
        {
            private set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AssetSpecified
        {
            get { return null != Asset; }
        }
        /// <summary>
        /// asset
        /// </summary>
        public Pair<Cst.UnderlyingAsset, int> Asset
        {
            private set;
            get;
        }

        
        public bool IdCurveDefSpecified
        {
            get { return StrFunc.IsFilled(IdCurveDef); }
        }
        /// <summary>
        /// Matrix definition identifier
        /// </summary>
        public string IdCurveDef
        {
            private set;
            get;
        }

        #endregion Accessors
        
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdCurveDef"></param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        public CurveParam(string pCS, string pIdCurveDef, DateTime pDtCalculation, string pIdC)
        {
            IdCurveDef = pIdCurveDef;

            SetParameters(pDtCalculation, pIdC, null);

            if (StrFunc.IsEmpty(IdCurveDef))
                FindIdMatrixDef(pCS); 

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="Payer"></param>
        /// <param name="Receiver"></param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        // EG 20150706 [21021] Pair<int,Nullable<int>> for pPayer|pReceiver
        public CurveParam(string pCS, Pair<int, Nullable<int>> pPayer, Pair<int, Nullable<int>> pReceiver, DateTime pDtCalculation,
                            string pIdC, Pair<Cst.UnderlyingAsset, int> pAsset)
        {
            if ((null != pPayer) && (null != pReceiver))
            {
                KeyQuote keyQuoteSide = new KeyQuote(pCS, pDtCalculation, pPayer.First, pPayer.Second, pReceiver.First, pReceiver.Second);
                IdMarketEnvironment = keyQuoteSide.IdMarketEnv;
                CurveSide = keyQuoteSide.QuoteSide.Value;
            }
            else
            {
                IdMarketEnvironment = string.Empty;
                CurveSide = QuotationSideEnum.Mid;
            }

            SetParameters(pDtCalculation, pIdC, pAsset);

            if (StrFunc.IsEmpty(IdCurveDef))
                FindIdMatrixDef(pCS);
        }


        #endregion Constructors
        #region Methods
        
        /// <summary>
        /// Alimentation de la date et de la devise 
        /// </summary>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        private void SetParameters(DateTime pDtCalculation, string pIdC, Pair<Cst.UnderlyingAsset, int> pAsset)
        {
            DtCalculation = pDtCalculation;
            Asset = pAsset;
            IdC = pIdC;
        }


        #region FindIdMatrixDef
        private void FindIdMatrixDef(string pCS)
        {
            string SQLSelect = SQLCst.SELECT + "IDMATRIXDEF" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATRIXDEF + Cst.CrLf;
            SQLSelect += SQLCst.WHERE;

            //MarketEnvironment
            SQLSelect += "(";
            if (IdMarketEnvironmentSpecified)
                SQLSelect += "IDMARKETENV = " + DataHelper.SQLString(IdMarketEnvironment) + SQLCst.OR;
            SQLSelect += "IDMARKETENV " + SQLCst.IS_NULL;
            SQLSelect += ")" + Cst.CrLf;
            SQLSelect += SQLCst.AND + Cst.CrLf;

            //IDC
            SQLSelect += "(";
            if (StrFunc.IsFilled(IdC))
                SQLSelect += "IDC = " + DataHelper.SQLString(IdC) + SQLCst.OR;
            SQLSelect += "IDC " + SQLCst.IS_NULL;
            SQLSelect += ")" + Cst.CrLf;
            SQLSelect += SQLCst.AND + Cst.CrLf;

            //CURVESIDE
            SQLSelect += "(";
            SQLSelect += "SIDE = " + DataHelper.SQLString(CurveSide.ToString()) + SQLCst.OR;
            SQLSelect += "SIDE " + SQLCst.IS_NULL;
            SQLSelect += ")" + Cst.CrLf;

            //ASSET
            if (null != Asset)
            {
                SQLSelect += SQLCst.AND + Cst.CrLf;
                SQLSelect += StrFunc.AppendFormat("(ASSETCATEGORY='{0}' and IDASSET={1})",
                                Asset.First.ToString(), Asset.Second.ToString());
            }

            SQLSelect += SQLCst.ORDERBY + "IDMARKETENV" + SQLCst.DESC + ",IDC" + SQLCst.DESC + ",SIDE" + SQLCst.DESC + ",ISDEFAULT" + SQLCst.DESC;

            Object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, SQLSelect);
            if (null != obj)
                IdCurveDef = Convert.ToString(obj);

        }
        #endregion FindIdMatrixDef

        #endregion Methods
    }

    /// <summary>
    /// Repr�sente la clef d'acc�s � une courbe de taux
    /// <para>En plus des �l�ments classiques d'acc�s aux prix, l'acc�s � une courbe de taux n�cessite une devise et la d�finition de courbe de taux</para>
    /// </summary>
    public class KeyQuoteCurve : KeyQuote
    {
        #region members
        #region public idC
        /// <summary>
        /// Repr�sente la devise
        /// </summary>
        public string idC;
        #endregion
        #region public idYieldCurveDef
        /// <summary>
        /// Repr�sente la d�finition de la courbe de taux 
        /// </summary>
        public string idYieldCurveDef;
        #endregion
        #endregion

        #region constructor
        /// <summary>
        /// Nouvelle cl� d'acc�s � une courbe de taux
        /// <para>Avec le payer et le receiver,spheres� utilise l'environnement de march� adapt� et recherche un type (Bid,Ask ou Mid)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdC">Devise</param>
        /// <param name="pIdYieldCurveDef">D�finition de la courve de taux (optionel)
        /// <para>Si non renseign�, Spheres� consid�re la d�finition de la courbe par d�faut associ� � la devise {pIdC}</para></param>
        /// <param name="pDate">Date</param>
        /// <param name="pIdA_Pay">Optionel</param>
        /// <param name="pIdB_Pay">Optionel</param>
        /// <param name="pIdA_Rec">Optionel</param>
        /// <param name="pIdB_Rec">Optionel</param>
        // EG 20150706 [21021] Nullable<int> for pIdA_Pay|pIdB_Pay|pIdA_Rec|pIdB_Rec
        public KeyQuoteCurve(string pCS, string pIdC, string pIdYieldCurveDef, DateTime pDate,
                Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec)
            : base(pCS, pDate, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec, null)
        {
            idC = pIdC;
            idYieldCurveDef = pIdYieldCurveDef;
            
            SetDefaultIdYieldCurveDef(pCS);
        }
        
        /// <summary>
        /// Nouvelle cl� d'acc�s � une courbe de taux
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdC">Devise</param>
        /// <param name="pIdYieldCurveDef">D�finition de la courve de taux (optionel)<para>Si non renseign�, Spheres� consid�re la d�finition de la courbe par d�faut associ� � la devise {pIdC}</para></param>
        /// <param name="pDate">Date</param>
        public KeyQuoteCurve(string pCS, string pIdC, string pIdYieldCurveDef, DateTime pDate)
            : base(pCS, pDate)
        {
            idC = pIdC;
            idYieldCurveDef = pIdYieldCurveDef;
         
            SetDefaultIdYieldCurveDef(pCS);
        }
        #endregion

        #region Method
        /// <summary>
        /// Alimente la propri�t� idYieldCurveDef avec la d�finition de courbe de taux par d�faut si elle est non renseign�e
        /// </summary>
        private void SetDefaultIdYieldCurveDef(string pCS)
        {
            if (StrFunc.IsEmpty(idYieldCurveDef))
                idYieldCurveDef = YieldCurveTools.GetDefaultYieldCurve(pCS, idC);
        }
        #endregion
    }

    /// <summary>
    /// Permet de d'estimer un asset de taux � une date future via la m�thode de calcul des taux Forward
    /// </summary>
    public class EFS_CalcForwardRateIndex
    {
        #region Membres
        /// <summary>
        /// Repr�sente l'asset qui doit �tre �valu�
        /// </summary>
        private readonly int _idAsset;
        /// <summary>
        /// Repr�sente les �l�ment propre � la quotation recherch�e (la date, IdMarketEnv, etc...)
        /// </summary>
        private readonly KeyQuote _keyQuote;
        /// <summary>
        /// Repr�sente la date de traitement
        /// </summary>
        private readonly DateTime _dateCalculation;
        /// <summary>
        /// Repr�sente la devise du flux dans lequel l'asset est impliqu�
        /// </summary>
        private readonly string _idC;
        //
        private YieldCurveVal _yieldCurveVal;
        private decimal _forwardRateInitial;
        private decimal _zeroCouponDtStart;
        private decimal _zeroCouponDtEnd;
        //
        private decimal _forwardRate;
        #endregion

        #region accessor
        /// <summary>
        /// Obtient la courbe de taux utilis� pour estimer le taux
        /// </summary>
        public YieldCurveVal YieldCurveVal
        {
            get { return _yieldCurveVal; }
        }
        /// <summary>
        /// Obtient le taux z�ro coupon en DtStart
        /// </summary>
        public Decimal ZeroCouponStart
        {
            get { return _zeroCouponDtStart; }
        }
        /// <summary>
        /// Obtient le taux z�ro coupon en DtEnd (=DtStart + maturit� de l'indice)
        /// </summary>
        public Decimal ZeroCouponEnd
        {
            get { return _zeroCouponDtEnd; }
        }
        /// <summary>
        /// Obtient le r�sultat de l'estimation du taux
        /// </summary>
        public Decimal ForwardRate
        {
            get { return _forwardRate; }
        }
        /// <summary>
        /// Obtient le taux Forward (taux actuariel), ce taux est ensuite converti en taux mon�taire si l'indice est de type mon�taire
        /// </summary>
        public Decimal ForwardRateInitial
        {
            get { return _forwardRateInitial; }
        }

        #endregion accessor

        #region constructor
        public EFS_CalcForwardRateIndex(KeyQuote pKeyQuote, int pIdAsset, string pIdC, DateTime pDateCalculation)
        {
            _idAsset = pIdAsset;
            _keyQuote = pKeyQuote;
            _idC = pIdC;
            _dateCalculation = pDateCalculation;

        }
        #endregion

        #region Methods
        #region public Calc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pSession"></param>
        // EG 20190716 [VCL : New FixedIncome] Add KeyQuoteAdditional
        public void Calc(string pCS, IProductBase pProductBase, AppSession pSession)
        {
            Clear();

            if (false == (_idAsset > 0))
                throw new ArgumentException(MethodInfo.GetCurrentMethod().Name
                    + ": idAsset equal 0");
            //
            // Si le taux est inconnu alors estimation => calcul d'un taux forward
            SQL_AssetRateIndex sqlRateIndex = new SQL_AssetRateIndex(pCS, SQL_AssetRateIndex.IDType.IDASSET, _idAsset);
            if (false == sqlRateIndex.IsLoaded)
                throw new Exception(StrFunc.AppendFormat("Asset Rate index id:{0} is not found", _idAsset.ToString()));
            // 
            KeyQuoteCurve keyQuoteCurve = new KeyQuoteCurve(pCS, _idC, sqlRateIndex.Asset_IdYieldCurveDef, _dateCalculation)
            {
                QuoteSide = _keyQuote.QuoteSide,
                IdMarketEnv = _keyQuote.IdMarketEnv,
                IdValScenario = _keyQuote.IdValScenario,
                KeyQuoteAdditional = _keyQuote.KeyQuoteAdditional,
                QuoteTiming = _keyQuote.QuoteTiming
            };

            IInterval interval = pProductBase.CreateInterval(StringToEnum.Period(sqlRateIndex.Asset_Period_Tenor), sqlRateIndex.Asset_PeriodMltp_Tenor);
            DateTime dtStart = _keyQuote.Time;
            //FI Spheres� Pour d�terminer la date term on applique la ternor de l'indice, sans ternir compte des jours f�ries (pas de BusinessDayConvention)
            //Cette r�gle a �t� valid�e par FI/PL et EPL
            DateTime dtEnd = Tools.ApplyInterval(dtStart, Convert.ToDateTime(null), interval);

            YieldCurve curve;
            try
            {
                curve = new YieldCurve(pCS, pProductBase, keyQuoteCurve);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Error on load YieldCurve for asset:{0} ", sqlRateIndex.Identifier), ex); }

            try
            {
                curve.IsLoaded(pCS, pProductBase, pSession);
                _yieldCurveVal = curve.YieldCurveVal;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Error on load YieldCurveVal for curve:{0} ", curve.YieldCurveDef.idYieldCurveDef), ex); }

            try
            {
                _zeroCouponDtStart = (Decimal)curve.GetPointValue(dtStart);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Error on read zero coupon rate from YieldCurveVal date:{0} ", DtFunc.DateTimeToStringISO(dtStart)), ex); }

            try
            {
                _zeroCouponDtEnd = (Decimal)curve.GetPointValue(dtEnd);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Error on read zero coupon rate from YieldCurveVal date:{0} ", DtFunc.DateTimeToStringISO(dtEnd)), ex); }

            try
            {
                _forwardRateInitial = CalcForwardRate(_zeroCouponDtStart, _zeroCouponDtEnd, _dateCalculation, dtStart, dtEnd);
                _forwardRate = _forwardRateInitial;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Error on calculation of ForwardRate"), ex); }

            //Calcul du taux mon�taire �quivalent
            if ((sqlRateIndex.Idx_RateType == Cst.RateType_regular) || (sqlRateIndex.Idx_RateType == Cst.RateType_overnight))
            {
                Nullable<DayCountFractionEnum> dcfRateIndex = null;
                if (StrFunc.IsFilled(sqlRateIndex.Idx_DayCountFractionIsda))
                    dcfRateIndex = StringToEnum.DayCountFraction(sqlRateIndex.Idx_DayCountFractionIsda);
                else if (StrFunc.IsFilled(sqlRateIndex.Idx_DayCountFraction))
                    dcfRateIndex = StringToEnum.DayCountFraction(sqlRateIndex.Idx_DayCountFraction);
                if (dcfRateIndex == null)
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,StrFunc.AppendFormat("DayCountFraction is not specified for asset:{0}", sqlRateIndex.Identifier));

                EFS_EquivalentRate equi;
                if (sqlRateIndex.Idx_RateType == Cst.RateType_regular)
                {
                    equi = new EFS_EquivalentRate(EquiRateMethodEnum.SimpleToCompound,
                           dtStart, dtEnd, _forwardRateInitial, DayCountFractionEnum.ACTACTISDA.ToString(), interval, dcfRateIndex.ToString(), null);
                }
                else if (sqlRateIndex.Idx_RateType == Cst.RateType_overnight)
                {
                    equi = new EFS_EquivalentRate(EquiRateMethodEnum.SimpleToOvernightDecapitalized,
                           dtStart, dtEnd, _forwardRateInitial, DayCountFractionEnum.ACTACTISDA.ToString(), interval, dcfRateIndex.ToString(), null);
                }
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,StrFunc.AppendFormat("Equivalente Rate is not implemented for rate type:{0}", sqlRateIndex.Idx_RateType));

                _forwardRate = Math.Round(equi.compoundRate, 9, MidpointRounding.AwayFromZero);
            }
        }
        #endregion
        #region private Clear
        /// <summary>
        /// Purge les valeurs de retour
        /// </summary>
        private void Clear()
        {
            _yieldCurveVal = null;
            _forwardRate = Decimal.Zero;
            _zeroCouponDtStart = Decimal.Zero;
            _zeroCouponDtEnd = Decimal.Zero;
        }
        #endregion
        #region private static CalcForwardRate
        private static decimal CalcForwardRate(decimal pRateStart, decimal pRateEnd, DateTime pDateCalculation, DateTime pDateStart, DateTime pDateEnd)
        {
            EFS_DayCountFraction dcfStart = new EFS_DayCountFraction(pDateCalculation, pDateStart, DayCountFractionEnum.ACTACTISDA, null);
            decimal factorStart = dcfStart.Factor;
            //
            EFS_DayCountFraction dcfEnd = new EFS_DayCountFraction(pDateCalculation, pDateEnd, DayCountFractionEnum.ACTACTISDA, null);
            decimal factorEnd = dcfEnd.Factor;
            //
            EFS_DayCountFraction dcfStartEnd = new EFS_DayCountFraction(pDateStart, pDateEnd, DayCountFractionEnum.ACTACTISDA, null);
            decimal factorStartEnd = dcfStartEnd.Factor;
            //
            double numerator = Math.Pow((double)(1 + pRateEnd), (double)factorEnd);
            double denominator = Math.Pow((double)(1 + pRateStart), (double)factorStart);
            //
            decimal ret = (Decimal)Math.Pow(numerator / denominator, 1 / (double)factorStartEnd) - 1;
            //
            return ret;
        }
        #endregion
        #endregion
    }
}
