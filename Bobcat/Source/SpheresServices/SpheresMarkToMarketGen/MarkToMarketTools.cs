namespace EFS.Process.MarkToMarket_Deprecated
{
    #region class PricingValues
    // EG 20160404 Migration vs2013
    //[Obsolete("This class is transfered in EfsML_BusinessTools_Shared2.cs (EFSML project)", true)] 
    //public class PricingValues
    //{
    //    #region Members
    //    #region Input Values
    //    private decimal              m_UnderlyingPrice  = 0m;
    //    private decimal              m_Strike           = 0m;
    //    private decimal              m_RiskFreeInterest = 0m;
    //    private decimal              m_DividendYield    = 0m;
    //    private decimal              m_TimeToExpiration = 0m;
    //    private decimal              m_Volatility       = 0m;
    //    private EFS_DayCountFraction m_DayCountFraction;
    //    #endregion Input Values
    //    #region Result Values
    //    #region Same for Call and Put
    //    private decimal m_gamma = 0m;
    //    private decimal m_speed = 0m;
    //    private decimal m_vega  = 0m;
    //    private decimal m_volga = 0m;
    //    private decimal m_vanna = 0m;
    //    private decimal m_color = 0m;
    //    #endregion Same for Call and Put
    //    #region Call
    //    private decimal m_callPrice = 0m;
    //    private decimal m_callDelta = 0m;
    //    private decimal m_callTheta = 0m;
    //    private decimal m_callRho1  = 0m;
    //    private decimal m_callRho2  = 0m;
    //    private decimal m_callCharm = 0m;
    //    #endregion Call
    //    #region Put
    //    private decimal m_putPrice = 0m;
    //    private decimal m_putDelta = 0m;
    //    private decimal m_putTheta = 0m;
    //    private decimal m_putRho1  = 0m;
    //    private decimal m_putRho2  = 0m;
    //    private decimal m_putCharm = 0m;
    //    #endregion Put

    //    #region FxForward
    //    private decimal              m_SpotRate;
    //    private decimal              m_ForwardPrice;

    //    private string               m_Currency1;
    //    private decimal              m_InterestRate1;

    //    private string               m_Currency2;
    //    private EFS_DayCountFraction m_DayCountFraction2;
    //    private decimal              m_TimeToExpiration2;
    //    private decimal              m_InterestRate2;
    //    #endregion FxForward

    //    #endregion Result Values

    //    #region DBParameters
    //    private DataParameters m_ParamEventPricing;
    //    private DataParameters m_ParamEventPricing2;

    //    private IDbDataParameter m_ParamIde;

    //    private IDbDataParameter m_ParamDcf;
    //    private IDbDataParameter m_ParamDcfNum;
    //    private IDbDataParameter m_ParamDcfDen;
    //    private IDbDataParameter m_ParamTotalOfYear;
    //    private IDbDataParameter m_ParamTotalOfDay;

    //    private IDbDataParameter m_ParamIdC1;
    //    private IDbDataParameter m_ParamIdC2;
    //    private IDbDataParameter m_ParamStrike;
    //    private IDbDataParameter m_ParamVolatility;
    //    private IDbDataParameter m_ParamTimeToExpiration;
    //    private IDbDataParameter m_ParamUnderlyingPrice;
    //    private IDbDataParameter m_ParamDividendYield;
    //    private IDbDataParameter m_ParamRiskFreeInterest;
    //    private IDbDataParameter m_ParamExchangeRate;
    //    private IDbDataParameter m_ParamInterestRate1;
    //    private IDbDataParameter m_ParamInterestRate2;
    //    private IDbDataParameter m_ParamCallPrice;
    //    private IDbDataParameter m_ParamCallDelta;
    //    private IDbDataParameter m_ParamCallRho1;
    //    private IDbDataParameter m_ParamCallRho2;
    //    private IDbDataParameter m_ParamCallTheta;
    //    private IDbDataParameter m_ParamCallCharm;
    //    private IDbDataParameter m_ParamPutPrice;
    //    private IDbDataParameter m_ParamPutDelta;
    //    private IDbDataParameter m_ParamPutRho1;
    //    private IDbDataParameter m_ParamPutRho2;
    //    private IDbDataParameter m_ParamPutTheta;
    //    private IDbDataParameter m_ParamPutCharm;
    //    private IDbDataParameter m_ParamGamma;
    //    private IDbDataParameter m_ParamVega;
    //    private IDbDataParameter m_ParamColor;
    //    private IDbDataParameter m_ParamSpeed;
    //    private IDbDataParameter m_ParamVanna;
    //    private IDbDataParameter m_ParamVolga; 

    //    private IDbDataParameter m_ParamDcf2;
    //    private IDbDataParameter m_ParamDcfNum2;
    //    private IDbDataParameter m_ParamDcfDen2;
    //    private IDbDataParameter m_ParamTotalOfYear2;
    //    private IDbDataParameter m_ParamTotalOfDay2;
    //    private IDbDataParameter m_ParamTimeToExpiration2;
    //    private IDbDataParameter m_ParamSpotRate;
    //    #endregion DBParameters
    //    #endregion Members

    //    #region Accessors
    //    public EFS_DayCountFraction DayCountFraction
    //    {
    //        get { return m_DayCountFraction; }
    //        set { m_DayCountFraction=value; }
    //    }
    //    #region For FX Options only
    //    public decimal ExchangeRate
    //    {
    //        get { return UnderlyingPrice; }
    //    }
    //    public decimal DomesticInterest
    //    {
    //        get { return RiskFreeInterest; }
    //    }
    //    public decimal ForeignInterest
    //    {
    //        get { return DividendYield; }
    //    }
    //    #endregion For FX Options only
    //    #region For not FX Options only
    //    public decimal RiskFreeInterest
    //    {
    //        get { return m_RiskFreeInterest; }
    //    }
    //    public decimal DividendYield
    //    {
    //        get { return m_DividendYield; }
    //    }
    //    public decimal UnderlyingPrice
    //    {
    //        get { return m_UnderlyingPrice; }
    //    }
    //    #endregion For not FX Options only
    //    #region For All Options
    //    public decimal Strike
    //    {
    //        get { return m_Strike; }
    //    }
    //    public decimal TimeToExpiration
    //    {
    //        get { return m_TimeToExpiration; }
    //        set { m_TimeToExpiration = value; }
    //    }
    //    public decimal Volatility
    //    {
    //        get { return m_Volatility; }
    //    }
    //    #endregion For All Options

    //    #region Call and Put identicals values
    //    public decimal Gamma
    //    {
    //        get { return m_gamma; }
    //        set { m_gamma = value; }
    //    }
    //    public decimal Speed
    //    {
    //        get { return m_speed; }
    //        set { m_speed = value; }
    //    }
    //    public decimal Vega
    //    {
    //        get { return m_vega; }
    //        set { m_vega = value; }
    //    }
    //    public decimal Volga
    //    {
    //        get { return m_volga; }
    //        set { m_volga = value; }
    //    }
    //    public decimal Vanna
    //    {
    //        get { return m_vanna; }
    //        set { m_vanna = value; }
    //    }
    //    public decimal Color
    //    {
    //        get { return m_color; }
    //        set { m_color = value; }
    //    }
    //    #endregion Call and Put identicals values
    //    #region Call values
    //    public decimal CallPrice
    //    {
    //        get { return m_callPrice; }
    //        set { m_callPrice = value; }
    //    }
    //    public decimal CallDelta
    //    {
    //        get { return m_callDelta; }
    //        set { m_callDelta = value; }
    //    }
    //    public decimal CallTheta
    //    {
    //        get { return m_callTheta; }
    //        set { m_callTheta = value; }
    //    }
    //    public decimal CallRho1
    //    {
    //        get { return m_callRho1; }
    //        set { m_callRho1 = value; }
    //    }
    //    public decimal CallRho2
    //    {
    //        get { return m_callRho2; }
    //        set { m_callRho2 = value; }
    //    }
    //    public decimal CallCharm
    //    {
    //        get { return m_callCharm; }
    //        set { m_callCharm = value; }
    //    }
    //    public decimal CallGamma
    //    {
    //        get { return Gamma; }
    //        set { Gamma = value; }
    //    }
    //    public decimal CallSpeed
    //    {
    //        get { return Speed; }
    //        set { Speed = value; }
    //    }
    //    public decimal CallVega
    //    {
    //        get { return Vega; }
    //        set { Vega = value; }
    //    }
    //    public decimal CallVolga
    //    {
    //        get { return Volga; }
    //        set { Volga = value; }
    //    }
    //    public decimal CallVanna
    //    {
    //        get { return Vanna; }
    //        set { Vanna = value; }
    //    }
    //    public decimal CallColor
    //    {
    //        get { return Color; }
    //        set { Color = value; }
    //    }
    //    #region For FX Options only
    //    public decimal CallRhoCurrency1
    //    {
    //        get { return m_callRho1; }
    //        set { m_callRho1 = value; }
    //    }
    //    public decimal CallRhoCurrency2
    //    {
    //        get { return m_callRho2; }
    //        set { m_callRho2 = value; }
    //    }
    //    #endregion For FX Options only
    //    #endregion Call values
    //    #region Put values
    //    public decimal PutPrice
    //    {
    //        get { return m_putPrice; }
    //        set { m_putPrice = value; }
    //    }
    //    public decimal PutDelta
    //    {
    //        get { return m_putDelta; }
    //        set { m_putDelta = value; }
    //    }
    //    public decimal PutTheta
    //    {
    //        get { return m_putTheta; }
    //        set { m_putTheta = value; }
    //    }
    //    public decimal PutRho1
    //    {
    //        get { return m_putRho1; }
    //        set { m_putRho1 = value; }
    //    }
    //    public decimal PutRho2
    //    {
    //        get { return m_putRho2; }
    //        set { m_putRho2 = value; }
    //    }
    //    public decimal PutCharm
    //    {
    //        get { return m_putCharm; }
    //        set { m_putCharm = value; }
    //    }
    //    public decimal PutGamma
    //    {
    //        get { return Gamma; }
    //        set { Gamma = value; }
    //    }
    //    public decimal PutSpeed
    //    {
    //        get { return Speed; }
    //        set { Speed = value; }
    //    }
    //    public decimal PutVega
    //    {
    //        get { return Vega; }
    //        set { Vega = value; }
    //    }
    //    public decimal PutVolga
    //    {
    //        get { return Volga; }
    //        set { Volga = value; }
    //    }
    //    public decimal PutVanna
    //    {
    //        get { return Vanna; }
    //        set { Vanna = value; }
    //    }
    //    public decimal PutColor
    //    {
    //        get { return Color; }
    //        set { Color = value; }
    //    }
    //    #region For FX Options only
    //    public decimal PutRhoCurrency1
    //    {
    //        get { return m_putRho1; }
    //        set { m_putRho1 = value; }
    //    }
    //    public decimal PutRhoCurrency2
    //    {
    //        get { return m_putRho2; }
    //        set { m_putRho2 = value; }
    //    }
    //    #endregion For FX Options only
    //    #endregion Put values

    //    #region For FX forward only

    //    public EFS_DayCountFraction DayCountFraction2
    //    {
    //        get { return m_DayCountFraction2; }
    //        set { m_DayCountFraction2 = value; }
    //    }
    //    public decimal ForwardPrice
    //    {
    //        get { return m_ForwardPrice; }
    //        set { m_ForwardPrice = value; }
    //    }
    //    public decimal TimeToExpiration2
    //    {
    //        get { return m_TimeToExpiration2; }
    //        set { m_TimeToExpiration2 = value; }
    //    }
    //    public string Currency1
    //    {
    //        get { return m_Currency1; }
    //        set { m_Currency1 = value; }
    //    }
    //    public string Currency2
    //    {
    //        get { return m_Currency2; }
    //        set { m_Currency2 = value; }
    //    }
    //    public decimal InterestRate1
    //    {
    //        get { return m_InterestRate1; }
    //        set { m_InterestRate1 = value; }
    //    }
    //    public decimal InterestRate2
    //    {
    //        get { return m_InterestRate2; }
    //        set { m_InterestRate2 = value; }
    //    }
    //    public decimal SpotRate
    //    {
    //        get { return m_SpotRate; }
    //        set { m_SpotRate = value; }
    //    }
    //    #endregion For FX forward only

    //    #endregion Accessors

    //    #region Constructors
    //    public PricingValues()
    //    {
    //        m_UnderlyingPrice  = 0m;
    //        m_Strike           = 0m;
    //        m_RiskFreeInterest = 0m;
    //        m_DividendYield    = 0m;
    //        m_TimeToExpiration = 0m;
    //        m_Volatility       = 0m;
    //    }
    //    public PricingValues( decimal pExchangeRate, decimal pStrike, decimal pDomesticInterest, decimal pForeignInterest, EFS_DayCountFraction pDayCountFraction, decimal pVolatility)
    //    {
    //        m_UnderlyingPrice  = pExchangeRate;
    //        m_Strike           = pStrike;
    //        m_RiskFreeInterest = pDomesticInterest;
    //        m_DividendYield    = pForeignInterest;
    //        m_Volatility       = pVolatility;
    //        m_DayCountFraction = pDayCountFraction;
    //        m_TimeToExpiration = m_DayCountFraction.Factor;
    //    }


    //    public PricingValues(decimal pSpotRate,string pIdC1,string pIdC2)
    //    {
    //        m_SpotRate          = pSpotRate;
    //        m_Currency1         = pIdC1;
    //        m_Currency2         = pIdC2;
    //    }

    //    public PricingValues(decimal pSpotRate,decimal pForwardRate,
    //                         string pIdC1,EFS_DayCountFraction pDayCountFraction,decimal pInterestRate1,
    //                         string pIdC2,EFS_DayCountFraction pDayCountFraction2,decimal pInterestRate2)
    //    {
    //        m_SpotRate          = pSpotRate;
    //        m_ForwardPrice      = pForwardRate;

    //        m_Currency1         = pIdC1;
    //        m_DayCountFraction  = pDayCountFraction;
    //        m_TimeToExpiration  = m_DayCountFraction.Factor;
    //        m_InterestRate1     = pInterestRate1;

    //        m_Currency2         = pIdC2;
    //        m_InterestRate2     = pInterestRate2;
    //        m_DayCountFraction2 = pDayCountFraction2;
    //        m_TimeToExpiration = m_DayCountFraction2.Factor;
    //    }
    //    #endregion Constructors

    //    #region Method
    //    #region InitParameters
    //    private void InitParameters(IDbTransaction pDbTransaction)
    //    {
    //        m_ParamIde	            = new EFSParameter( pDbTransaction, "IDE",              DbType.Int32   ).DataParameter;

    //        m_ParamIdC1              = new EFSParameter( pDbTransaction, "IDC1",             DbType.AnsiString, SQLCst.UT_CURR_LEN).DataParameter;
    //        m_ParamIdC2              = new EFSParameter( pDbTransaction, "IDC2",             DbType.AnsiString, SQLCst.UT_CURR_LEN).DataParameter;
    //        m_ParamDcf               = new EFSParameter( pDbTransaction, "DCF",              DbType.AnsiString,SQLCst.UT_ENUM_OPTIONAL_LEN).DataParameter;
    //        m_ParamDcfNum            = new EFSParameter( pDbTransaction, "DCFNUM",           DbType.Int32).DataParameter;
    //        m_ParamDcfDen            = new EFSParameter( pDbTransaction, "DCFDEN",           DbType.Int32).DataParameter;
    //        m_ParamTotalOfYear       = new EFSParameter( pDbTransaction, "TOTALOFYEAR",      DbType.Int32).DataParameter;
    //        m_ParamTotalOfDay        = new EFSParameter( pDbTransaction, "TOTALOFDAY",       DbType.Int32).DataParameter;
    //        m_ParamTimeToExpiration  = new EFSParameter( pDbTransaction, "TIMETOEXPIRATION", DbType.Decimal ).DataParameter;

    //        m_ParamInterestRate1     = new EFSParameter( pDbTransaction, "INTERESTRATE1",    DbType.Decimal ).DataParameter;
    //        m_ParamInterestRate2     = new EFSParameter( pDbTransaction, "INTERESTRATE2",    DbType.Decimal ).DataParameter;

    //        m_ParamDcf2              = new EFSParameter( pDbTransaction, "DCF2",              DbType.AnsiString,SQLCst.UT_ENUM_OPTIONAL_LEN).DataParameter;
    //        m_ParamDcfNum2           = new EFSParameter( pDbTransaction, "DCFNUM2",           DbType.Int32).DataParameter;
    //        m_ParamDcfDen2           = new EFSParameter( pDbTransaction, "DCFDEN2",           DbType.Int32).DataParameter;
    //        m_ParamTotalOfYear2      = new EFSParameter( pDbTransaction, "TOTALOFYEAR2",      DbType.Int32).DataParameter;
    //        m_ParamTotalOfDay2       = new EFSParameter( pDbTransaction, "TOTALOFDAY2",       DbType.Int32).DataParameter;
    //        m_ParamTimeToExpiration2 = new EFSParameter( pDbTransaction, "TIMETOEXPIRATION2", DbType.Decimal ).DataParameter;
    //        m_ParamSpotRate          = new EFSParameter( pDbTransaction, "SPOTRATE",          DbType.Decimal).DataParameter;

    //        m_ParamStrike            = new EFSParameter(pDbTransaction, "STRIKE", DbType.Decimal).DataParameter;
    //        m_ParamVolatility        = new EFSParameter( pDbTransaction, "VOLATILITY",       DbType.Decimal ).DataParameter;
    //        m_ParamUnderlyingPrice   = new EFSParameter( pDbTransaction, "UNDERLYINGPRICE",  DbType.Decimal ).DataParameter;
    //        m_ParamDividendYield     = new EFSParameter( pDbTransaction, "DIVIDENDYIELD",    DbType.Decimal ).DataParameter;
    //        m_ParamRiskFreeInterest  = new EFSParameter( pDbTransaction, "RISKFREEINTEREST", DbType.Decimal ).DataParameter;
    //        m_ParamExchangeRate      = new EFSParameter( pDbTransaction, "EXCHANGERATE",     DbType.Decimal ).DataParameter;
    //        m_ParamCallPrice         = new EFSParameter( pDbTransaction, "CALLPRICE",        DbType.Decimal ).DataParameter;
    //        m_ParamCallDelta         = new EFSParameter( pDbTransaction, "CALLDELTA",        DbType.Decimal ).DataParameter;
    //        m_ParamCallRho1          = new EFSParameter( pDbTransaction, "CALLRHO1",         DbType.Decimal ).DataParameter;
    //        m_ParamCallRho2          = new EFSParameter( pDbTransaction, "CALLRHO2",         DbType.Decimal ).DataParameter;
    //        m_ParamCallTheta         = new EFSParameter( pDbTransaction, "CALLTHETA",        DbType.Decimal ).DataParameter;
    //        m_ParamCallCharm         = new EFSParameter( pDbTransaction, "CALLCHARM",        DbType.Decimal ).DataParameter;
    //        m_ParamPutPrice          = new EFSParameter( pDbTransaction, "PUTPRICE",         DbType.Decimal ).DataParameter;
    //        m_ParamPutDelta          = new EFSParameter( pDbTransaction, "PUTDELTA",         DbType.Decimal ).DataParameter;
    //        m_ParamPutRho1           = new EFSParameter( pDbTransaction, "PUTRHO1",          DbType.Decimal ).DataParameter;
    //        m_ParamPutRho2           = new EFSParameter( pDbTransaction, "PUTRHO2",          DbType.Decimal ).DataParameter;
    //        m_ParamPutTheta          = new EFSParameter( pDbTransaction, "PUTTHETA",         DbType.Decimal ).DataParameter;
    //        m_ParamPutCharm          = new EFSParameter( pDbTransaction, "PUTCHARM",         DbType.Decimal ).DataParameter;
    //        m_ParamGamma             = new EFSParameter( pDbTransaction, "GAMMA",            DbType.Decimal ).DataParameter;
    //        m_ParamVega              = new EFSParameter( pDbTransaction, "VEGA",             DbType.Decimal ).DataParameter;
    //        m_ParamColor             = new EFSParameter( pDbTransaction, "COLOR",            DbType.Decimal ).DataParameter;
    //        m_ParamSpeed             = new EFSParameter( pDbTransaction, "SPEED",            DbType.Decimal ).DataParameter;
    //        m_ParamVanna             = new EFSParameter( pDbTransaction, "VANNA",            DbType.Decimal ).DataParameter;
    //        m_ParamVolga             = new EFSParameter( pDbTransaction, "VOLGA",            DbType.Decimal ).DataParameter; 

    //        m_ParamIdC1.Value              = Convert.DBNull;
    //        m_ParamIdC2.Value              = Convert.DBNull;
    //        m_ParamDcf.Value               = Convert.DBNull;
    //        m_ParamDcfNum.Value            = Convert.DBNull;
    //        m_ParamDcfDen.Value            = Convert.DBNull;
    //        m_ParamTotalOfYear.Value       = Convert.DBNull;
    //        m_ParamTotalOfDay.Value        = Convert.DBNull;
    //        m_ParamTimeToExpiration.Value  = Convert.DBNull;

    //        m_ParamInterestRate1.Value     = Convert.DBNull;
    //        m_ParamInterestRate2.Value     = Convert.DBNull;

    //        m_ParamDcf2.Value              = Convert.DBNull;
    //        m_ParamDcfNum2.Value           = Convert.DBNull;
    //        m_ParamDcfDen2.Value           = Convert.DBNull;
    //        m_ParamTotalOfYear2.Value      = Convert.DBNull;
    //        m_ParamTotalOfDay2.Value       = Convert.DBNull;
    //        m_ParamTimeToExpiration2.Value = Convert.DBNull;
    //        m_ParamSpotRate.Value          = Convert.DBNull;

    //        m_ParamStrike.Value            = Convert.DBNull;
    //        m_ParamVolatility.Value        = Convert.DBNull;
    //        m_ParamUnderlyingPrice.Value   = Convert.DBNull;
    //        m_ParamDividendYield.Value     = Convert.DBNull;
    //        m_ParamRiskFreeInterest.Value  = Convert.DBNull;
    //        m_ParamExchangeRate.Value      = Convert.DBNull;
    //        m_ParamCallPrice.Value         = Convert.DBNull;
    //        m_ParamCallDelta.Value         = Convert.DBNull;
    //        m_ParamCallRho1.Value          = Convert.DBNull;
    //        m_ParamCallRho2.Value          = Convert.DBNull;
    //        m_ParamCallTheta.Value         = Convert.DBNull;
    //        m_ParamCallCharm.Value         = Convert.DBNull;
    //        m_ParamPutPrice.Value          = Convert.DBNull;
    //        m_ParamPutDelta.Value          = Convert.DBNull;
    //        m_ParamPutRho1.Value           = Convert.DBNull;
    //        m_ParamPutRho2.Value           = Convert.DBNull;
    //        m_ParamPutTheta.Value          = Convert.DBNull;
    //        m_ParamPutCharm.Value          = Convert.DBNull;
    //        m_ParamGamma.Value             = Convert.DBNull;
    //        m_ParamVega.Value              = Convert.DBNull;
    //        m_ParamColor.Value             = Convert.DBNull;
    //        m_ParamSpeed.Value             = Convert.DBNull;
    //        m_ParamVanna.Value             = Convert.DBNull;
    //        m_ParamVolga.Value             = Convert.DBNull;
    //    }
    //    #endregion InitParameters
    //    #region UpdateEventPricing

    //    public void UpdateEventPricing(IDbTransaction pDbTransaction, int pIdE, IProduct pProduct)
    //    {
    //        int count = 0;
    //        string SQLSelect = SQLCst.SELECT + SQLCst.COUNT_ALL + SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTPRICING + SQLCst.WHERE + "IDE = " + DataHelper.SQLString(Convert.ToString(pIdE));

    //        object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, SQLSelect);
    //        if (null != obj)
    //            count = Convert.ToInt32(obj);

    //        InitParameters(pDbTransaction);
    //        m_ParamIde.Value = pIdE;
    //        if (StrFunc.IsFilled(this.Currency1))
    //            m_ParamIdC1.Value = this.Currency1;
    //        if (StrFunc.IsFilled(this.Currency2))
    //            m_ParamIdC2.Value = this.Currency2;
    //        if (null != this.DayCountFraction)
    //        {
    //            m_ParamDcf.Value = this.DayCountFraction.DayCountFraction_FpML;
    //            m_ParamDcfNum.Value = this.DayCountFraction.Numerator;
    //            m_ParamDcfDen.Value = this.DayCountFraction.Denominator;
    //            m_ParamTotalOfYear.Value = this.DayCountFraction.NumberOfCalendarYears;
    //            m_ParamTotalOfDay.Value = this.DayCountFraction.TotalNumberOfCalculatedDays;
    //            m_ParamTimeToExpiration.Value = this.DayCountFraction.Factor;
    //        }
    //        if (pProduct.productBase.IsFxOption)
    //        {
    //            #region FxOption
    //            m_ParamStrike.Value = this.Strike;
    //            m_ParamVolatility.Value = this.Volatility;
    //            if (pProduct.productBase.IsFxOptionLeg)
    //            {
    //                m_ParamExchangeRate.Value = this.ExchangeRate;
    //                m_ParamInterestRate1.Value = this.DomesticInterest;
    //                m_ParamInterestRate2.Value = this.ForeignInterest;
    //            }
    //            else
    //            {
    //                m_ParamUnderlyingPrice.Value = this.UnderlyingPrice;
    //                m_ParamDividendYield.Value = this.DividendYield;
    //                m_ParamRiskFreeInterest.Value = this.RiskFreeInterest;
    //            }

    //            #region Call values
    //            if (0 != this.CallPrice)
    //                m_ParamCallPrice.Value = this.CallPrice;
    //            if (0 != this.CallDelta)
    //                m_ParamCallDelta.Value = this.CallDelta;
    //            if (0 != this.CallRho1)
    //                m_ParamCallRho1.Value = this.CallRho1;
    //            if (0 != this.CallRho2)
    //                m_ParamCallRho2.Value = this.CallRho2;
    //            if (0 != this.CallTheta)
    //                m_ParamCallTheta.Value = this.CallTheta;
    //            if (0 != this.CallCharm)
    //                m_ParamCallCharm.Value = this.CallCharm;
    //            #endregion Call values
    //            #region Put values
    //            if (0 != this.PutPrice)
    //                m_ParamPutPrice.Value = this.PutPrice;
    //            if (0 != this.PutDelta)
    //                m_ParamPutDelta.Value = this.PutDelta;
    //            if (0 != this.PutRho1)
    //                m_ParamPutRho1.Value = this.PutRho1;
    //            if (0 != this.PutRho2)
    //                m_ParamPutRho2.Value = this.PutRho2;
    //            if (0 != this.PutTheta)
    //                m_ParamPutTheta.Value = this.PutTheta;
    //            if (0 != this.PutCharm)
    //                m_ParamPutCharm.Value = this.PutCharm;
    //            #endregion Put values
    //            #region Others values
    //            if (0 != this.Gamma)
    //                m_ParamGamma.Value = this.Gamma;
    //            if (0 != this.Vega)
    //                m_ParamVega.Value = this.Vega;
    //            if (0 != this.Color)
    //                m_ParamColor.Value = this.Color;
    //            if (0 != this.Speed)
    //                m_ParamSpeed.Value = this.Speed;
    //            if (0 != this.Vanna)
    //                m_ParamVanna.Value = this.Vanna;
    //            if (0 != this.Volga)
    //                m_ParamVolga.Value = this.Volga;
    //            #endregion Others values

    //            #endregion FxOption
    //        }
    //        else if (pProduct.productBase.IsFxLeg)
    //        {
    //            #region FxLeg
    //            if (null != this.DayCountFraction2)
    //            {
    //                m_ParamDcf2.Value = this.DayCountFraction2.DayCountFraction_FpML;
    //                m_ParamDcfNum2.Value = this.DayCountFraction2.Numerator;
    //                m_ParamDcfDen2.Value = this.DayCountFraction2.Denominator;
    //                m_ParamTotalOfYear2.Value = this.DayCountFraction2.NumberOfCalendarYears;
    //                m_ParamTotalOfDay2.Value = this.DayCountFraction2.TotalNumberOfCalculatedDays;
    //                m_ParamTimeToExpiration2.Value = this.DayCountFraction2.Factor;
    //            }

    //            if (0 != this.ForwardPrice)
    //                m_ParamExchangeRate.Value = this.ForwardPrice;
    //            if (0 != this.InterestRate1)
    //                m_ParamInterestRate1.Value = this.InterestRate1;
    //            if (0 != this.InterestRate2)
    //                m_ParamInterestRate2.Value = this.InterestRate2;
    //            if (0 != this.SpotRate)
    //                m_ParamSpotRate.Value = this.SpotRate;
    //            #endregion FxLeg
    //        }

    //        string SQLQuery = string.Empty;
    //        if (0 == count)
    //        {
    //            #region Insert
    //            SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EVENTPRICING + Cst.CrLf;
    //            SQLQuery += "(IDE, DCF, DCFNUM, DCFDEN, TOTALOFYEAR, TOTALOFDAY, IDC1, IDC2, ";
    //            SQLQuery += " STRIKE, VOLATILITY, TIMETOEXPIRATION, UNDERLYINGPRICE, DIVIDENDYIELD, RISKFREEINTEREST, EXCHANGERATE, INTERESTRATE1, INTERESTRATE2,";
    //            SQLQuery += " CALLPRICE, CALLDELTA, CALLRHO1, CALLRHO2, CALLTHETA, CALLCHARM, PUTPRICE, PUTDELTA, PUTRHO1, PUTRHO2, PUTTHETA, PUTCHARM, GAMMA, VEGA, COLOR, SPEED, VANNA, VOLGA,";
    //            SQLQuery += " DCF2, DCFNUM2, DCFDEN2, TOTALOFYEAR2, TOTALOFDAY2,TIMETOEXPIRATION2, SPOTRATE )" + Cst.CrLf;
    //            SQLQuery += "VALUES (@IDE, @DCF, @DCFNUM, @DCFDEN, @TOTALOFYEAR, @TOTALOFDAY, @IDC1, @IDC2, ";
    //            SQLQuery += " @STRIKE, @VOLATILITY, @TIMETOEXPIRATION, @UNDERLYINGPRICE, @DIVIDENDYIELD, @RISKFREEINTEREST, @EXCHANGERATE, @INTERESTRATE1, @INTERESTRATE2,";
    //            SQLQuery += " @CALLPRICE, @CALLDELTA, @CALLRHO1, @CALLRHO2, @CALLTHETA, @CALLCHARM, @PUTPRICE, @PUTDELTA, @PUTRHO1, @PUTRHO2, @PUTTHETA, @PUTCHARM, @GAMMA, @VEGA, @COLOR, @SPEED, @VANNA, @VOLGA,";
    //            SQLQuery += " @DCF2, @DCFNUM2, @DCFDEN2, @TOTALOFYEAR2, @TOTALOFDAY2, @TIMETOEXPIRATION2, @SPOTRATE )";
    //            #endregion Insert
    //        }
    //        else
    //        {
    //            #region Update
    //            SQLQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.EVENTPRICING + Cst.CrLf;
    //            SQLQuery += " set DCF = @DCF, DCFNUM = @DCFNUM, DCFDEN = @DCFDEN, TOTALOFYEAR = @TOTALOFYEAR, TOTALOFDAY = @TOTALOFDAY, " + Cst.CrLf;
    //            SQLQuery += "     IDC1 = @IDC1, IDC2 = @IDC2, STRIKE = @STRIKE, VOLATILITY = @VOLATILITY, TIMETOEXPIRATION = @TIMETOEXPIRATION, " + Cst.CrLf;
    //            SQLQuery += "     UNDERLYINGPRICE = @UNDERLYINGPRICE, DIVIDENDYIELD = @DIVIDENDYIELD, RISKFREEINTEREST = @RISKFREEINTEREST, " + Cst.CrLf;
    //            SQLQuery += "     EXCHANGERATE = @EXCHANGERATE, INTERESTRATE1 = @INTERESTRATE1, INTERESTRATE2 = @INTERESTRATE2, " + Cst.CrLf;
    //            SQLQuery += "     CALLPRICE = @CALLPRICE, CALLDELTA = @CALLDELTA, CALLRHO1 = @CALLRHO1, CALLRHO2 = @CALLRHO2, CALLTHETA = @CALLTHETA, CALLCHARM = @CALLCHARM, " + Cst.CrLf;
    //            SQLQuery += "     PUTPRICE = @PUTPRICE, PUTDELTA = @PUTDELTA, PUTRHO1 = @PUTRHO1, PUTRHO2 = @PUTRHO2, PUTTHETA = @PUTTHETA, PUTCHARM = @PUTCHARM, " + Cst.CrLf;
    //            SQLQuery += "     GAMMA = @GAMMA, VEGA = @VEGA, COLOR = @COLOR, SPEED = @SPEED, VANNA = @VANNA, VOLGA = @VOLGA, " + Cst.CrLf;
    //            SQLQuery += "     DCF2 = @DCF2, DCFNUM2 = @DCFNUM2, DCFDEN2 = @DCFDEN2, TOTALOFYEAR2 = @TOTALOFYEAR2, TOTALOFDAY2 = @TOTALOFDAY2, " + Cst.CrLf;
    //            SQLQuery += "     TIMETOEXPIRATION2 = @TIMETOEXPIRATION2, SPOTRATE = @SPOTRATE " + Cst.CrLf;

    //            SQLQuery += SQLCst.WHERE + "IDE = @IDE";
    //            #endregion Update
    //        }

    //        DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, SQLQuery,
    //            m_ParamIde, m_ParamDcf, m_ParamDcfNum, m_ParamDcfDen, m_ParamTotalOfYear, m_ParamTotalOfDay, m_ParamIdC1, m_ParamIdC2,
    //            m_ParamStrike, m_ParamVolatility, m_ParamTimeToExpiration, m_ParamUnderlyingPrice, m_ParamDividendYield, m_ParamRiskFreeInterest,
    //            m_ParamExchangeRate, m_ParamInterestRate1, m_ParamInterestRate2,
    //            m_ParamCallPrice, m_ParamCallDelta, m_ParamCallRho1, m_ParamCallRho2, m_ParamCallTheta, m_ParamCallCharm,
    //            m_ParamPutPrice, m_ParamPutDelta, m_ParamPutRho1, m_ParamPutRho2, m_ParamPutTheta, m_ParamPutCharm,
    //            m_ParamGamma, m_ParamVega, m_ParamColor, m_ParamSpeed, m_ParamVanna, m_ParamVolga,
    //            m_ParamDcf2, m_ParamDcfNum2, m_ParamDcfDen2, m_ParamTotalOfYear2, m_ParamTotalOfDay2, m_ParamTimeToExpiration2, m_ParamSpotRate
    //            );
    //    }
    //    #endregion WriteEventPricing
    //    #endregion Method
    //}
    #endregion class PricingValues

    #region class MarkToMarketTools
    // EG 20160404 Migration vs2013
    //[Obsolete("This class is transfered in EfsML_BusinessTools_Shared2.cs (EFSML project)", true)] 
    //public class MarkToMarketTools
    //{
    //    #region Constructor
    //    public MarkToMarketTools()
    //    {
    //    }
    //    #endregion Constructor
    //    #region Method
    //    #region decimal functions
    //    private static decimal Abs( decimal pX )
    //    {
    //        return Convert.ToDecimal( Math.Abs( Convert.ToDouble( pX ) ) );
    //    }
    //    private static decimal Exp( decimal pX )
    //    {
    //        return Convert.ToDecimal( Math.Exp( Convert.ToDouble( pX ) ) );
    //    }
    //    private static decimal Log( decimal pX )
    //    {
    //        return Convert.ToDecimal( Math.Log( Convert.ToDouble( pX ) ) );
    //    }
    //    private static decimal Max( decimal pX, decimal pY )
    //    {
    //        return (pX < pY ? pY : pX );
    //    }
    //    private static decimal Pow( decimal pX, decimal pY )
    //    {
    //        return Convert.ToDecimal( Math.Pow( Convert.ToDouble( pX ), Convert.ToDouble( pY ) ) );
    //    }
    //    private static decimal Sqrt( decimal pX )
    //    {
    //        return Convert.ToDecimal( Math.Sqrt( Convert.ToDouble( pX ) ) );
    //    }
    //    #endregion decimal functions

    //    #region StandardNormal
    //    public static decimal StandardNormal( decimal pX )
    //    {
    //        const decimal p  = 0.2316419m;
    //        const decimal c1 = 2.506628m;
    //        const decimal c2 = 0.319381530m;
    //        const decimal c3 = -0.356563782m;
    //        const decimal c4 = 1.781477937m;
    //        const decimal c5 = -1.821255978m;
    //        const decimal c6 = 1.330274429m;
    //        decimal w  = 1m;
    //        if ( pX < 0 ) w = -1m;
    //        decimal y = 1m / ( 1m + p * w * pX );
    //        return ( 0.5m + w * ( 0.5m - ( Exp( -pX * pX / 2 ) / c1 ) * ( y * ( c2 + y * ( c3 + y * ( c4 + y * ( c5 + y * c6 ) ) ) ) ) ) );
    //    }
    //    #endregion StandardNormal

    //    #region StandardNormalProbability
    //    public static decimal StandardNormalProbability( decimal pX )
    //    {
    //        return ( 1 / Sqrt( 2 * ( (decimal)Math.PI ) ) * Exp( -0.5m * Pow( pX, 2 ) ) );
    //    }
    //    #endregion StandardNormalProbability

    //    #region PricingValues Black76
    //    // see : http://www.riskglossary.com/articles/black_1976.htm
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="pUnderlyingPrice"></param> The current underlying forward price
    //    /// <param name="pStrike"></param> The strike price
    //    /// <param name="pRiskFreeInterest"></param> The continuously compounded risk free interest rate
    //    /// <param name="pDividendYield"></param> The dividend yield
    //    /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
    //    /// <param name="pVolatility"></param> The implied volatility for the underlying
    //    /// <returns></returns>
    //    public static PricingValues Black76( decimal pUnderlyingPrice, decimal pStrike, decimal pRiskFreeInterest, decimal pDividendYield, EFS_DayCountFraction pDayCountFraction, decimal pVolatility )
    //    {
    //        PricingValues pricingValues = null;
    //        pricingValues    = new PricingValues( pUnderlyingPrice, pStrike, pRiskFreeInterest, pDividendYield, pDayCountFraction, pVolatility );
    //        decimal sqrtTime = Sqrt( pricingValues.TimeToExpiration );
    //        decimal dt = pricingValues.Volatility * Sqrt( pricingValues.TimeToExpiration );
    //        #region d1 & d2
    //        decimal d1 = ( Log( pricingValues.UnderlyingPrice / pricingValues.Strike ) +
    //                        ( pricingValues.RiskFreeInterest - pricingValues.DividendYield + 
    //                          Pow( pricingValues.Volatility, 2 ) / 2 ) * pricingValues.TimeToExpiration ) / dt;
    //        decimal d2 = d1 - dt;
    //        #endregion d1 & d2
    //        #region StandardNormal of d1 & d2
    //        decimal phiD1 = StandardNormal( d1 );
    //        decimal phiD2 = StandardNormal( d2 );
    //        #endregion StandardNormal of d1 & d2

    //        #region Delta
    //        pricingValues.CallDelta = phiD1 * Exp( - pricingValues.DividendYield * pricingValues.TimeToExpiration );
    //        pricingValues.PutDelta  = pricingValues.CallDelta - 1;
    //        #endregion Delta

    //        #region Gamma
    //        pricingValues.Gamma      = pricingValues.CallDelta / ( pricingValues.UnderlyingPrice * dt );
    //        #endregion Gamma

    //        #region Vega
    //        decimal underlyingPricePerCallDelta = pricingValues.UnderlyingPrice * pricingValues.CallDelta;
    //        pricingValues.Vega      = underlyingPricePerCallDelta * sqrtTime;
    //        #endregion Vega

    //        return pricingValues;
    //    }
    //    #endregion static PricingValues Black76

    //    #region PricingValues GarmanAndKolhagen
    //    //  see : http://www.riskglossary.com/link/garman_kohlhagen_1983.htm
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="pExchRate"></param> The current exchange rate in domestic currency per unit of foreign currency
    //    /// <param name="pStrike"></param> The strike exchange rate
    //    /// <param name="pDomesticInterest"></param> The continuously compounded domestic risk free interest rate
    //    /// <param name="pForeignInterest"></param> The continuously compounded foreign risk free interest rate
    //    /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
    //    /// <param name="pVolatility"></param> The implied volatility for the underlying exchange rate
    //    /// <returns></returns>
    //    /// 
    //    public static PricingValues GarmanAndKolhagen( decimal pExchRate, decimal pStrike, decimal pDomesticInterest, decimal pForeignInterest, EFS_DayCountFraction pDayCountFraction, decimal pVolatility )
    //    {
    //        PricingValues pricingValues = null;
    //        pricingValues    = new PricingValues( pExchRate, pStrike, pDomesticInterest, pForeignInterest, pDayCountFraction, pVolatility );
    //        decimal sqrtTime = Sqrt( pricingValues.TimeToExpiration );
    //        decimal dt       = pricingValues.Volatility * sqrtTime;
    //        #region d1 & d2
    //        decimal d1       = ( Log(pricingValues.ExchangeRate / pricingValues.Strike ) + 
    //                             ( pricingValues.DomesticInterest - pricingValues.ForeignInterest + 
    //                               Pow( pricingValues.Volatility, 2 ) / 2 ) * pricingValues.TimeToExpiration ) / dt;

    //        decimal d2       = d1 - dt;
    //        #endregion d1 & d2

    //        #region StandardNormal of d1 & d2 (and opposites)
    //        decimal phiD1    = StandardNormal( d1 );
    //        decimal phiD2    = StandardNormal( d2 );
    //        decimal phiOppD1 = StandardNormal( - d1 );
    //        decimal phiOppD2 = StandardNormal( - d2 );
    //        #endregion StandardNormal of d1 & d2 (and opposites)

    //        decimal expOppForeignInterestRateT  = Exp( - pricingValues.ForeignInterest * pricingValues.TimeToExpiration );
    //        decimal expOppDomesticInterestRateT = Exp( - pricingValues.DomesticInterest * pricingValues.TimeToExpiration);

    //        decimal SNPD1 = StandardNormalProbability( d1 );

    //        #region Call Price
    //        decimal c1              = pricingValues.ExchangeRate * expOppForeignInterestRateT  * phiD1;
    //        decimal c2              = pricingValues.Strike       * expOppDomesticInterestRateT * phiD2;
    //        pricingValues.CallPrice = c1 - c2;
    //        #endregion Call Price
    //        #region Put Price
    //        decimal p1             = pricingValues.ExchangeRate * expOppForeignInterestRateT  * phiOppD1;
    //        decimal p2             = pricingValues.Strike       * expOppDomesticInterestRateT * phiOppD2;
    //        pricingValues.PutPrice = p2 - p1;
    //        #endregion Put Price
    //        #region Greeks
    //        #region Delta
    //        pricingValues.CallDelta = expOppForeignInterestRateT * phiD1;
    //        pricingValues.PutDelta  = expOppForeignInterestRateT * ( phiD1 - 1 );
    //        #endregion Delta
    //        #region Gamma
    //        pricingValues.Gamma     = ( SNPD1 * expOppForeignInterestRateT ) / ( pricingValues.ExchangeRate * dt );
    //        #endregion Gamma
    //        #region Vega
    //        pricingValues.Vega      = pricingValues.ExchangeRate * expOppForeignInterestRateT  * SNPD1 * sqrtTime;
    //        #endregion Vega
    //        #region Theta
    //        decimal thetaPart1      = - ( ( pricingValues.ExchangeRate * expOppForeignInterestRateT * SNPD1 * pricingValues.Volatility ) / ( 2 * sqrtTime ) );
    //        pricingValues.CallTheta = thetaPart1 + ( pricingValues.ForeignInterest * c1 ) - ( pricingValues.DomesticInterest * c2 );
    //        pricingValues.PutTheta  = thetaPart1 - ( pricingValues.ForeignInterest * p1 ) + ( pricingValues.DomesticInterest * p2 );
    //        #endregion Theta
    //        #region Rho
    //        #region Rho1
    //        pricingValues.CallRhoCurrency1 = pricingValues.TimeToExpiration * c2;
    //        pricingValues.PutRhoCurrency1  = - pricingValues.TimeToExpiration * p2;
    //        #endregion Rho1
    //        #region Rho2
    //        pricingValues.CallRhoCurrency2 = - pricingValues.TimeToExpiration * c1;
    //        pricingValues.PutRhoCurrency2  = pricingValues.TimeToExpiration * p1;
    //        #endregion Rho2
    //        #endregion Rho
    //        #endregion Greeks

    //        return pricingValues;
    //    }
    //    #endregion static PricingValues GarmanAndKolhagen

    //    #region PricingValues BinomialTrees
    //    // see : http://www.global-derivatives.com/options/american-options.php
    //    //       http://www.global-derivatives.com/options/european-options.php
    //    //       http://en.wikipedia.org/wiki/Binomial_options_pricing_model
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="pExchRate"></param> The current exchange rate in domestic currency per unit of foreign currency
    //    /// <param name="pStrike"></param> The strike exchange rate
    //    /// <param name="pRiskFreeInterest"></param> The continuously compounded risk free interest rate ( or the continuously compounded domestic risk free interest rate for FX options)
    //    /// <param name="pDividendYield"></param> The dividend yield ( or the continuously compounded foreign risk free interest rate for FX Options)
    //    /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
    //    /// <param name="pVolatility"></param> The implied volatility for the underlying exchange rate
    //    /// <param name="pSteps"></param> The number of level of the binomial tree
    //    /// <returns></returns>
    //    public static PricingValues BinomialTrees( ExerciseStyleEnum pExeStyle, decimal pExchRate, decimal pStrike, decimal pRiskFreeInterest, decimal pDividendYield, EFS_DayCountFraction pDayCountFraction, decimal pVolatility, int pSteps )
    //    {
    //        PricingValues pricingValues = null;
    //        pricingValues = new PricingValues( pExchRate, pStrike, pRiskFreeInterest, pDividendYield, pDayCountFraction, pVolatility );

    //        #region Call Price
    //        pricingValues.CallPrice = calcBinomial( ExerciseStyleEnum.American, OptionTypeEnum.Call,
    //            pricingValues.UnderlyingPrice, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        #endregion Call Price
    //        #region Put Price
    //        pricingValues.PutPrice = calcBinomial( ExerciseStyleEnum.American, OptionTypeEnum.Put,
    //            pricingValues.UnderlyingPrice, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        #endregion Put Price
    //        #region Greeks
    //        decimal value1 = 0m;
    //        decimal value2 = 0m;
    //        decimal value3 = 0m;
    //        #region Delta
    //        decimal du = 0.001m * pricingValues.UnderlyingPrice;
    //        decimal u1 = pricingValues.UnderlyingPrice + du;
    //        decimal u2 = pricingValues.UnderlyingPrice - du;
    //        decimal u3 = 0m;

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, u1, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Call, u2, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.CallDelta = ( value1 - value2 ) / ( 2 * du );

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Put, u1, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Put, u2, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.PutDelta = ( value1 - value2 ) / ( 2 * du );
    //        #endregion Delta
    //        #region Gamma
    //        du = 0.1m * pricingValues.UnderlyingPrice;
    //        u1 = pricingValues.UnderlyingPrice + du;
    //        u3 = pricingValues.UnderlyingPrice - du;
    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, u1, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = pricingValues.CallPrice;
    //        value3 = calcBinomial( pExeStyle, OptionTypeEnum.Call, u3, pricingValues.Strike, pricingValues.RiskFreeInterest,
    //            pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.Gamma = ( value1 - ( 2 * value2 ) + value3 ) / Pow( du, 2 );
    //        #endregion Gamma
    //        #region Vega
    //        decimal dv = 0.001m * pricingValues.Volatility;
    //        decimal v1 = pricingValues.Volatility + dv;
    //        decimal v2 = pricingValues.Volatility - dv;
    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, pricingValues.TimeToExpiration, v1, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, pricingValues.TimeToExpiration, v2, pSteps );
    //        pricingValues.Vega = ( value1 - value2 ) / ( 2 * dv );
    //        #endregion Vega
    //        #region Theta
    //        decimal dt = 0.001m * pricingValues.TimeToExpiration;
    //        decimal t1 = pricingValues.TimeToExpiration + dt;
    //        decimal t2 = pricingValues.TimeToExpiration - dt;

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, t1, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, t2, pricingValues.Volatility, pSteps );
    //        pricingValues.CallTheta = ( value2 - value1 ) / ( 2 * dt );

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, t1, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	pricingValues.DividendYield, t2, pricingValues.Volatility, pSteps );
    //        pricingValues.PutTheta = ( value2 - value1 ) / ( 2 * dt );
    //        #endregion Theta
    //        #region Rho1
    //        decimal dr = 0.1m * pricingValues.RiskFreeInterest;
    //        decimal r1 = pricingValues.RiskFreeInterest + dr;
    //        decimal r2 = pricingValues.RiskFreeInterest - dr;

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            r1,	pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            r2,	pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.CallRho1 = ( value1 - value2 ) / ( 2 * dr );

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            r1,	pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            r2,	pricingValues.DividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.PutRho1 = ( value1 - value2 ) / ( 2 * dr );
    //        #endregion Rho1
    //        #region Rho2
    //        decimal dd = 0.1m * pricingValues.DividendYield;
    //        decimal d1 = pricingValues.DividendYield + dd;
    //        decimal d2 = pricingValues.DividendYield - dd;

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	d1, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	d2, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.CallRho2 = ( value1 - value2 ) / ( 2 * dd );

    //        value1 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	d1, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        value2 = calcBinomial( pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice, pricingValues.Strike,
    //            pricingValues.RiskFreeInterest,	d2, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps );
    //        pricingValues.PutRho2 = ( value1 - value2 ) / ( 2 * dd );
    //        #endregion Rho2
    //        #endregion Greeks

    //        return pricingValues;
    //    }
    //    #endregion static PricingValues AmericainBinomialTrees

    //    #region calcBinomial
    //    // see : http://www.global-derivatives.com/options/american-options.php
    //    //       http://www.global-derivatives.com/options/european-options.php
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="pExeStyle"></param>
    //    /// <param name="pOptionType"></param>
    //    /// <param name="pUnderlyingPrice"></param> The current underlying forward price
    //    /// <param name="pStrike"></param> The strike price
    //    /// <param name="pRiskFreeInterest"></param> The continuously compounded risk free interest rate
    //    /// <param name="pDividendYield"></param> The dividend yield
    //    /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
    //    /// <param name="pVolatility"></param> The implied volatility for the underlying
    //    /// <param name="pSteps"></param> The number of level of the binomial tree
    //    /// <returns></returns>
    //    private static decimal calcBinomial( ExerciseStyleEnum pExeStyle, OptionTypeEnum pOptionType, decimal pUnderlyingPrice, decimal pStrike, decimal pRiskFreeInterest, decimal pDividendYield, decimal pTimeToExpiration, decimal pVolatility, int pSteps )
    //    {
    //        decimal binomialValue;
    //        decimal dt = pTimeToExpiration / pSteps;
    //        decimal upMovement = Exp( pVolatility * Sqrt( dt ) );
    //        decimal downMovement = 1 / upMovement;
    //        decimal increaseProbability = ( Exp( ( pRiskFreeInterest - pDividendYield ) * dt ) - downMovement ) / ( upMovement - downMovement );
    //        decimal decreaseProbability = 1 - increaseProbability;
    //        decimal expOppInterestDt = Exp( - pRiskFreeInterest * dt );
    //        decimal[] nodes = new decimal[ pSteps + 1 ];
    //        decimal spotPrice = 0m;
    //        int sign = 1;

    //        if ( OptionTypeEnum.Put == pOptionType )
    //        {
    //            sign = -1;
    //        }

    //        // Compute each final node
    //        for ( int i = 0; i <= pSteps; i += 1 )
    //        {
    //            spotPrice = pUnderlyingPrice * Pow( upMovement, i ) * Pow( downMovement, pSteps - i );
    //            nodes[i] = Max( 0, sign * ( spotPrice - pStrike ) );
    //        }

    //        if ( ExerciseStyleEnum.European == pExeStyle )
    //        {
    //            // Compute all nodes
    //            for ( int tt = pSteps - 1; tt >= 0; tt -= 1 )
    //            {
    //                for ( int i = 0; i <= tt; i += 1 )
    //                {
    //                    nodes[i] = ( increaseProbability * nodes[i + 1] + decreaseProbability * nodes[i] ) * expOppInterestDt;
    //                }
    //            }
    //        }
    //        else if ( ExerciseStyleEnum.American == pExeStyle )
    //        {
    //            // Compute all nodes
    //            for ( int tt = pSteps - 1; tt >= 0; tt -= 1 )
    //            {
    //                for ( int i = 0; i <= tt; i += 1 )
    //                {
    //                    nodes[i] = Max( ( sign * ( pUnderlyingPrice * Pow( upMovement, i ) * Pow( downMovement, Abs( i - tt ) ) - pStrike ) ), ( increaseProbability * nodes[i + 1] + decreaseProbability * nodes[i] ) * expOppInterestDt );
    //                }
    //            }
    //        }
    //        //
    //        binomialValue = nodes[0];
    //        //
    //        return binomialValue;
    //    }
    //    #endregion static decimal calcBinomial
    //    #endregion Method
    //}
    #endregion class MarkToMarketTools
}
