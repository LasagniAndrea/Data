#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Curve
{
    #region BenchmarkRef
    public class BenchmarkRef
    {
        #region Members
        public Cst.UnderlyingAsset_Rate assetCategory;
        public int idAsset;
        public BaseRateTypeEnum baseRateType;
        #endregion Members
        //
        #region Constructors
        public BenchmarkRef() { }
        #endregion Constructors
        //
        #region Method
        /// <summary>
        /// Rtourne les caractéristiques de l'asset utlisé pour le point de BenchMark 
        /// <para>Obtient null si l'asset n'est pas trouvé</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public SQL_AssetForCurve GetSqlAsset(string pCS)
        {
            return YieldCurveTools.GetSqlAsset(pCS, (this.assetCategory), this.idAsset);
        }
        #endregion
    }
    #endregion BenchmarkRef
    #region YieldCurve
    /// <summary>
    /// Permet d'interroger une courbe de taux
    /// </summary>
    public class YieldCurve
    {
        #region Members
        /// <summary>
        /// Représente la définition de la courbe de taux
        /// </summary>
        private YieldCurveDef _yieldCurveDef;
        /// <summary>
        /// Représente la clef d'accès à une courbe de taux
        /// </summary>
        private readonly KeyQuoteCurve _keyQuoteCurve;
        /// <summary>
        /// Représente la courbe de taux
        /// </summary>
        private YieldCurveVal _yieldCurveVal;
        #endregion Members
        
        #region Accessors
        /// <summary>
        /// Obtient la définition de la courbe de taux
        /// </summary>
        public YieldCurveDef YieldCurveDef
        {
            get { return _yieldCurveDef; }
        }
        
        /// <summary>
        /// Obtient true si la courbe de taux est alimentée
        /// </summary>
        public bool YieldCurveValSpecified
        {
            get { return (null != _yieldCurveVal); }
        }
        
        /// <summary>
        /// Obtient la courbe de taux
        /// </summary>
        public YieldCurveVal YieldCurveVal
        {
            get { return _yieldCurveVal; }
        }
        #endregion

        public ProcessCacheContainer ProcessCacheContainer
        {
            get;
            set;
        }
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pKeyQuoteCurve"></param>
        public YieldCurve(string pCS, IProductBase pProduct, KeyQuoteCurve pKeyQuoteCurve)
        {
            _keyQuoteCurve = pKeyQuoteCurve;
            Initialize(pCS, pProduct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIdYieldCurveDef"></param>
        public YieldCurve(string pCS, IProductBase pProduct, DateTime pDtCalculation, string pIdC, string pIdYieldCurveDef)
        {
            _keyQuoteCurve = new KeyQuoteCurve(pCS, pIdC, pIdYieldCurveDef, pDtCalculation);
            Initialize(pCS, pProduct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIdYieldCurveDef"></param>
        /// <param name="pIdA_Pay"></param>
        /// <param name="pIdB_Pay"></param>
        /// <param name="pIdA_Rec"></param>
        /// <param name="pIdB_Rec"></param>
        // EG 20150706 [21021] Nullable<int> for  pIdA_Pay|pIdB_Pay|pIdA_Rec|pIdB_Rec
        public YieldCurve(string pCS, IProductBase pProduct,
                           DateTime pDtCalculation, string pIdC, string pIdYieldCurveDef,
                           Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec)
        {
            _keyQuoteCurve = new KeyQuoteCurve(pCS, pIdC, pIdYieldCurveDef, pDtCalculation, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec);
            Initialize(pCS, pProduct);
        }
        #endregion Constructors
        
        #region Methods
        #region public GetPointValue
        /// <summary>
        /// Retourne le taux Zero Coupon à la date {pDtPoint}
        /// <para>Utilise la méthode d'interpolation définie dans YieldCurveDef </para>
        /// </summary>
        /// <param name="pDtPoint"></param>
        /// <returns></returns>
        public double GetPointValue(DateTime pDtPoint)
        {
            return GetPointValue(YieldCurveValueEnum.zeroCouponYield, pDtPoint);
        }
        /// <summary>
        /// Retourne le taux Zero Coupon à la date {pDtPoint} en appliquant la méthode d'interpolation {pInterpolationMethod}
        /// </summary>
        /// <param name="pInterpolationMethod"></param>
        /// <param name="pDtPoint"></param>
        /// <returns></returns>
        public double GetPointValue(InterpolationMethodEnum pInterpolationMethod, DateTime pDtPoint)
        {
            return GetPointValue(pInterpolationMethod, YieldCurveValueEnum.zeroCouponYield, pDtPoint);
        }
        /// <summary>
        /// Retourne le type de point (Zero Coupon,discountFactor,..) {pYieldCurveValue} à la date {pDtPoint}
        /// <para>Utilise la méthode d'interpolation définie dans YieldCurveDef</para>
        /// <para>Utilise la méthode d'interpolation linéaire si non défini dans YieldCurveDef</para>
        /// </summary>
        /// <param name="pYieldCurveValue"></param>
        /// <param name="pDtPoint"></param>
        /// <returns></returns>
        public double GetPointValue(YieldCurveValueEnum pYieldCurveValue, DateTime pDtPoint)
        {
            InterpolationMethodEnum interpolationMethod = InterpolationMethodEnum.Linear;
            switch (pYieldCurveValue)
            {
                case YieldCurveValueEnum.zeroCouponYield:
                    if (_yieldCurveDef.interpolationMethodZCSpecified)
                        interpolationMethod = _yieldCurveDef.interpolationMethodZC;

                    break;
                case YieldCurveValueEnum.discountFactor:
                    if (_yieldCurveDef.interpolationMethodDFSpecified)
                        interpolationMethod = _yieldCurveDef.interpolationMethodDF;
                    break;
                default:
                    if (_yieldCurveDef.interpolationMethodZCSpecified)
                        interpolationMethod = _yieldCurveDef.interpolationMethodZC;
                    break;
            }
            return GetPointValue(interpolationMethod, pYieldCurveValue, pDtPoint);
        }
        /// <summary>
        /// Retourne le type de point (Zero Coupon,discountFactor,..) {pYieldCurveValue} à la date {pDtPoint} selon la méthode d'interpolation {pInterpolationMethod}
        /// </summary>
        /// <param name="pInterpolationMethod"></param>
        /// <param name="pYieldCurveValue"></param>
        /// <param name="pDtPoint"></param>
        /// <returns></returns>
        public double GetPointValue(InterpolationMethodEnum pInterpolationMethod, YieldCurveValueEnum pYieldCurveValue, DateTime pDtPoint)
        {
            int polynomialDegree = 1;
            //
            switch (pYieldCurveValue)
            {
                case YieldCurveValueEnum.zeroCouponYield:
                    if (_yieldCurveDef.polynomialDegreeZCSpecified)
                        polynomialDegree = _yieldCurveDef.polynomialDegreeZC;
                    break;
                case YieldCurveValueEnum.discountFactor:
                    if (_yieldCurveDef.polynomialDegreeDFSpecified)
                        polynomialDegree = _yieldCurveDef.polynomialDegreeDF;
                    break;
                default:
                    if (_yieldCurveDef.polynomialDegreeZCSpecified)
                        polynomialDegree = _yieldCurveDef.polynomialDegreeZC;
                    break;
            }
            //
            return _yieldCurveVal.GetPointValue(pInterpolationMethod, pYieldCurveValue, pDtPoint, polynomialDegree);
        }
        #endregion GetPointValue

        #region SetLogMessage
        // EG 20150312 New
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel SetLogMessage(string pMessage, params object[] pDatas)
        {
            string message = pMessage;
            if (null != pDatas)
                message = StrFunc.AppendFormat(message, pDatas);

            if (null == ProcessCacheContainer)
                throw new Exception(message);

            // FI 2020623 [XXXXX] SetErrorWarning
            ProcessCacheContainer.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Error, message));

            return Cst.ErrLevel.FAILURE;
        }
        #endregion SetLogMessage

        /// <summary>
        /// Retourne true si la courbe de taux est chargée
        /// <para>Si elle est n'existe pas, génère la courbe</para>
        /// </summary>
        /// <exception cref="Exception lorsque la courbe n'est pas chargée"></exception>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public bool IsLoaded(string pCS, IProductBase pProductBase, AppSession pSession)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (null == _yieldCurveDef)
                ret = SetLogMessage("YieldCurveDef name [{0}] not found", _keyQuoteCurve.idYieldCurveDef);

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                if (YieldCurveValSpecified)
                {
                    #region Imported YieldCurveVal disabled -> Error
                    if (false == YieldCurveVal.isEnabled)
                    {
                        ret = SetLogMessage("YieldCurveVal name [{0}] date [{1}] is disabled", 
                            YieldCurveVal.idYieldCurveDef, DtFunc.DateTimeToString(_keyQuoteCurve.Time, DtFunc.FmtShortDate));
                    }
                    #endregion Imported YieldCurveVal disabled -> Error
                    if (Cst.ErrLevel.SUCCESS == ret)
                        ret = _yieldCurveVal.SetYieldCurvePoints(pCS, pProductBase, _yieldCurveDef);
                }
                else
                {
                    if (_yieldCurveDef.isImported)
                    {
                        #region Imported YieldCurveVal not Found -> Error
                        ret = SetLogMessage("YieldCurveVal name [{0}] date [{1}]  (imported) not found",
                            _yieldCurveDef.idYieldCurveDef, DtFunc.DateTimeToString(_keyQuoteCurve.Time, DtFunc.FmtShortDate));
                        #endregion Imported YieldCurveVal not Found -> Error
                    }
                    else
                    {
                        #region YieldCurveVal not Found -> create it
                        ret = CreateYieldCurveVal(pCS, pProductBase);

                        if (Cst.ErrLevel.SUCCESS != ret)
                            ret = SetLogMessage("YieldCurveDef name [{0}] Date [{1}] : YieldCurveVal creation error", _yieldCurveDef.idYieldCurveDef, _keyQuoteCurve.Time.Date);

                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            IDbTransaction dbTransaction = null;
                            bool isError = false;
                            try
                            {
                                dbTransaction = DataHelper.BeginTran(pCS);
                                ret = YieldCurveVal.Insert(pCS, dbTransaction, pSession);
                                if (Cst.ErrLevel.SUCCESS != ret)
                                    isError = true;
                                else
                                    DataHelper.CommitTran(dbTransaction);
                            }
                            catch (Exception)
                            {
                                isError = true; 
                                throw;
                            }
                            finally
                            {
                                if ((null != dbTransaction) && isError)
                                    DataHelper.RollbackTran(dbTransaction);
                            }
                        }
                        #endregion YieldCurveVal not Found -> create it
                    }
                }
            }
            return (Cst.ErrLevel.SUCCESS == ret);
        }
        
        /// <summary>
        /// Génère une courbe de taux complète (génère également les points de la courbe) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pCurveParam"></param>
        // EG 20190716 [VCL : New FixedIncome] Upd KeyQuoteAdditional
        private Cst.ErrLevel CreateYieldCurveVal(string pCS, IProductBase pProductBase)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            //Chargement des points de BenchMarks
            _yieldCurveDef.LoadBenchMark(pCS, pProductBase, _keyQuoteCurve.Time);
            if (false == _yieldCurveDef.benchmarkRefsSpecified)
                ret = SetLogMessage("BenchMark points are not specified for YielCurveDef {0}", _yieldCurveDef.idYieldCurveDef);

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                //Chargement des points du modèle s'il existe un modèle de pas rattaché à la définition de la courbe de taux
                if (_yieldCurveDef.idCurvePtSchedSpecified)
                    _yieldCurveDef.LoadCurvePtSched(pCS, pProductBase);
                //
                //Création du YieldCurveVal
                #region YieldCurveVal creation
                _yieldCurveVal = new YieldCurveVal(this)
                {
                    idYieldCurveDef = _yieldCurveDef.idYieldCurveDef,

                    idMarketEnv = _keyQuoteCurve.IdMarketEnv,
                    idValScenario = _keyQuoteCurve.IdValScenario,

                    curveSideSpecified = (null != _keyQuoteCurve.QuoteSide),

                    dtBase = _keyQuoteCurve.Time,
                    dtSpot = _keyQuoteCurve.Time,
                    dtInputData = _keyQuoteCurve.Time,
                    dtEnd = _keyQuoteCurve.Time,
                    source = ReflectionTools.ConvertEnumToString<ProductTools.SourceEnum>(ProductTools.SourceEnum.EfsML),

                    cashFlowTypeSpecified = (null != _keyQuoteCurve.KeyQuoteAdditional) && _keyQuoteCurve.KeyQuoteAdditional.CashflowType.HasValue
                };
                if (_yieldCurveVal.curveSideSpecified)
                    _yieldCurveVal.curveSide = (QuotationSideEnum)_keyQuoteCurve.QuoteSide;

                if (_yieldCurveVal.cashFlowTypeSpecified)
                    _yieldCurveVal.cashFlowType = _keyQuoteCurve.KeyQuoteAdditional.CashflowType.Value.ToString();


                DateTime dtOffset;
                #region InputData date calculation
                if (_yieldCurveDef.inputOffsetSpecified)
                {
                    dtOffset = Tools.ApplyOffset(pCS, _yieldCurveVal.dtBase, _yieldCurveDef.inputOffset, _yieldCurveDef.BusinessCenters);
                    if (DtFunc.IsDateTimeFilled(dtOffset))
                        _yieldCurveVal.dtInputData = dtOffset;
                    else
                        ret = SetLogMessage("Error on ApplyOffset for dtInputData");
                }
                #endregion InputData date calculation

                #region Spot date calculation
                if ((Cst.ErrLevel.SUCCESS == ret) && _yieldCurveDef.spotOffsetSpecified)
                {
                    dtOffset = Tools.ApplyOffset(pCS, _yieldCurveVal.dtInputData, _yieldCurveDef.spotOffset, _yieldCurveDef.BusinessCenters);
                    if (DtFunc.IsDateTimeFilled(dtOffset))
                        _yieldCurveVal.dtSpot = dtOffset;
                    else
                        ret = SetLogMessage("Error on ApplyOffset for dtSpot");
                }
                #endregion Spot date calculation
                #region End date calculation
                if ((Cst.ErrLevel.SUCCESS == ret) && _yieldCurveDef.endOffsetSpecified)
                {
                    dtOffset = Tools.ApplyOffset(pCS, _yieldCurveVal.dtBase, _yieldCurveDef.endOffset, _yieldCurveDef.BusinessCenters);
                    if (DtFunc.IsDateTimeFilled(dtOffset))
                        _yieldCurveVal.dtEnd = dtOffset;
                    else
                        ret = SetLogMessage("Error on ApplyOffset for dtEnd");
                }
                #endregion End date calculation

                //Génération des points de la courbes
                if (Cst.ErrLevel.SUCCESS == ret)
                    ret = _yieldCurveVal.CreateYieldCurvePoints(pCS, pProductBase, _yieldCurveDef);

                #endregion YieldCurveVal creation
            }
            return ret;
        }
        
        /// <summary>
        /// Lecture de la table YIELDCURVEDEF pour le couple {IDYIELDCURVEDEF,IDC}
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private QueryParameters GetSelectYieldCurveDef(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"yc.IDYIELDCURVEDEF, yc.IDCURVEPTSCHED," + Cst.CrLf;
            query += @"yc.DISPLAYNAME, yc.CURVETYPE, yc.IDC," + Cst.CrLf;
            query += @"yc.ISDEFAULT, yc.ISIMPORTED, yc.COMPOUNDFREQUENCY," + Cst.CrLf;
            query += @"yc.ISFUTADJUSTMENT, yc.ISEXTRAPOLATION," + Cst.CrLf;
            query += @"yc.INTERPOLMETHODZC, yc.POLYNOMIALDEGREEZC," + Cst.CrLf;
            query += @"yc.INTERPOLMETHODDF, yc.POLYNOMIALDEGREEDF," + Cst.CrLf;
            query += @"yc.PERIODMLTPSPOTOFFSET, yc.PERIODSPOTOFFSET, yc.DAYTYPESPOTOFFSET, " + Cst.CrLf;
            query += @"yc.PERIODMLTPINPUTOFFSET, yc.PERIODINPUTOFFSET, yc.DAYTYPEINPUTOFFSET, " + Cst.CrLf;
            query += @"yc.PERIODMLTPENDOFFSET, yc.PERIODENDOFFSET, yc.DAYTYPEENDOFFSET" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.YIELDCURVEDEF + " yc " + Cst.CrLf;
            query += SQLCst.WHERE + @"(yc.IDYIELDCURVEDEF=@IDYIELDCURVEDEF)" + Cst.CrLf;
            query += SQLCst.AND + @"(@IDC=@IDC)" + Cst.CrLf;
            query += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "yc").ToString();

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEDEF", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _keyQuoteCurve.idYieldCurveDef);
            dp.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), _keyQuoteCurve.idC);

            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);

            return ret;
        }
        
        /// <summary>
        /// Retourne la query de selection d'une courbe de taux
        /// <para>Exploite la table YIELDCURVEVAL_H </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurveParam"></param>
        /// <returns></returns>
        // EG 20190716 [VCL : New FixedIncome] Upd (KeyQuoteAdditional)
        private QueryParameters GetSelectYieldCurveVal(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"yc.IDYIELDCURVEVAL_H, yc.IDYIELDCURVEDEF," + Cst.CrLf;
            query += @"yc.DTBASE, yc.DTSPOT, yc.DTINPUTDATA, yc.DTEND, yc.DTBUILDDATE," + Cst.CrLf;
            query += @"yc.IDMARKETENV, yc.IDVALSCENARIO,yc.CURVESIDE,yc.CASHFLOWTYPE," + Cst.CrLf;
            query += @"yc.SOURCE, yc.ISENABLED," + Cst.CrLf;
            query += @"yc.DTINS, yc.IDAINS, yc.DTUPD, yc.IDAUPD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.YIELDCURVEVAL_H + " yc " + Cst.CrLf;
            query += SQLCst.WHERE + @"(yc.IDYIELDCURVEDEF =@IDYIELDCURVEDEF)" + Cst.CrLf;
            query += SQLCst.AND + @"(yc.DTBASE=@DTBASE)" + Cst.CrLf;
            query += SQLCst.AND + @"(yc.IDMARKETENV=@IDMARKETENV)" + Cst.CrLf; ;
            query += SQLCst.AND + @"(yc.IDVALSCENARIO=@IDVALSCENARIO)" + Cst.CrLf;
            //
            Boolean isUseOrderbyCurveSide = (null != _keyQuoteCurve.QuoteSide);
            if (null != _keyQuoteCurve.QuoteSide)
                query += SQLCst.AND + @"((yc.CURVESIDE=@CURVESIDE) or (yc.CURVESIDE is null))";
            else
                query += SQLCst.AND + @"(yc.CURVESIDE is null)";
            query += Cst.CrLf;
            //
            Boolean isUseOrderbyCashFlowType = (null != _keyQuoteCurve.QuoteSide);
            if ((null != _keyQuoteCurve.KeyQuoteAdditional) && _keyQuoteCurve.KeyQuoteAdditional.CashflowType.HasValue)
                query += SQLCst.AND + @"((yc.CASHFLOWTYPE=@CASHFLOWTYPE) or (yc.CASHFLOWTYPE is null))";
            else
                query += SQLCst.AND + @"(yc.CASHFLOWTYPE is null)";
            query += Cst.CrLf;
            //
            StrBuilder orderby = new StrBuilder(string.Empty);
            if (isUseOrderbyCurveSide || isUseOrderbyCashFlowType)
                orderby += SQLCst.ORDERBY;
            if (isUseOrderbyCurveSide)
                orderby += DataHelper.SQLIsNullChar(pCS, "yc.CURVESIDE", "ZZ");
            if (isUseOrderbyCashFlowType)
            {
                if (isUseOrderbyCurveSide)
                    orderby += ",";
                orderby += DataHelper.SQLIsNullChar(pCS, "yc.CASHFLOWTYPE", "ZZ");
            }
            query += orderby.ToString();
            //                    
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEDEF", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _yieldCurveDef.idYieldCurveDef);
            dp.Add(new DataParameter(pCS, "DTBASE", DbType.Date), _keyQuoteCurve.Time); // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _keyQuoteCurve.IdMarketEnv);
            dp.Add(new DataParameter(pCS, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _keyQuoteCurve.IdValScenario);
            if (null != _keyQuoteCurve.QuoteSide)
                dp.Add(new DataParameter(pCS, "CURVESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), _keyQuoteCurve.QuoteSide.ToString());

            if (null != _keyQuoteCurve.KeyQuoteAdditional)
            {
                if (_keyQuoteCurve.KeyQuoteAdditional.CashflowType.HasValue)
                    dp.Add(new DataParameter(pCS, "CASHFLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), _keyQuoteCurve.KeyQuoteAdditional.CashflowType.ToString());
            }

            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);
            return ret;
        }
        
        /// <summary>
        /// Charge la définition de la courbe de taux.
        /// <para>Charge les points de BenchMark associé</para>
        /// <para>Charge les caractéristiques de la courbe (YIELDCURVEVAL_H)</para>
        /// </summary>
        private Cst.ErrLevel LoadYieldCurveDef(string pCS, IProductBase pProductBase)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            _yieldCurveDef = null;

            if (StrFunc.IsFilled(_keyQuoteCurve.idYieldCurveDef))
            {

                QueryParameters qryParameters = GetSelectYieldCurveDef(pCS);
                DataSet ds = DataHelper.ExecuteDataset(qryParameters.Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                ds.DataSetName = "dsYieldCurveDef";
                DataTable dtYieldCurveDef = ds.Tables[0];
                dtYieldCurveDef.TableName = "YieldCurveDef";


                bool yieldCurveDefFound = (null != dtYieldCurveDef) && (1 == dtYieldCurveDef.Rows.Count);
                if (false == yieldCurveDefFound)
                {
                    ret = SetLogMessage("YieldCurveDef not found : Requested [YieldCurveDef={0}/idC={1}]", _keyQuoteCurve.idYieldCurveDef, _keyQuoteCurve.idC);
                }

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region YieldCurveDef filling
                    DataRow rowYieldCurve = dtYieldCurveDef.Rows[0];
                    _yieldCurveDef = new YieldCurveDef
                    {
                        idYieldCurveDef = rowYieldCurve["IDYIELDCURVEDEF"].ToString(),

                        idCurvePtSchedSpecified = (false == Convert.IsDBNull(rowYieldCurve["IDCURVEPTSCHED"])),

                        displayName = rowYieldCurve["DISPLAYNAME"].ToString(),

                        curveType = (YieldCurveTypeEnum)System.Enum.Parse(typeof(YieldCurveTypeEnum),
                        rowYieldCurve["CURVETYPE"].ToString(), true),

                        idC = rowYieldCurve["IDC"].ToString(),
                        isDefault = Convert.ToBoolean(rowYieldCurve["ISDEFAULT"]),
                        isImported = Convert.ToBoolean(rowYieldCurve["ISIMPORTED"]),
                        compoundFrequency = (CompoundingFrequencyEnum)System.Enum.Parse(typeof(CompoundingFrequencyEnum),
                        rowYieldCurve["COMPOUNDFREQUENCY"].ToString(), true),

                        interpolationMethodZCSpecified = (false == Convert.IsDBNull(rowYieldCurve["INTERPOLMETHODZC"])),
                        polynomialDegreeZCSpecified = (false == Convert.IsDBNull(rowYieldCurve["POLYNOMIALDEGREEZC"])),
                        interpolationMethodDFSpecified = (false == Convert.IsDBNull(rowYieldCurve["INTERPOLMETHODDF"])),
                        polynomialDegreeDFSpecified = (false == Convert.IsDBNull(rowYieldCurve["POLYNOMIALDEGREEDF"])),

                        isAdjustmentFuture = Convert.ToBoolean(rowYieldCurve["ISFUTADJUSTMENT"]),

                        isExtrapolation = Convert.ToBoolean(rowYieldCurve["ISEXTRAPOLATION"]),

                        //INPUTOFFSET
                        inputOffsetSpecified = (false == Convert.IsDBNull(rowYieldCurve["PERIODMLTPINPUTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["PERIODINPUTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["DAYTYPEINPUTOFFSET"]))
                    };
                    if (_yieldCurveDef.inputOffsetSpecified)
                        _yieldCurveDef.inputOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowYieldCurve["PERIODINPUTOFFSET"].ToString()),
                            Convert.ToInt32(rowYieldCurve["PERIODMLTPINPUTOFFSET"]),
                            StringToEnum.DayType(rowYieldCurve["DAYTYPEINPUTOFFSET"].ToString()));

                    //ENDOFFSET
                    _yieldCurveDef.endOffsetSpecified = (false == Convert.IsDBNull(rowYieldCurve["PERIODMLTPENDOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["PERIODENDOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["DAYTYPEENDOFFSET"]));
                    #endregion YieldCurveDef filling continue

                    //SPOTOFFSET
                    _yieldCurveDef.spotOffsetSpecified = (false == Convert.IsDBNull(rowYieldCurve["PERIODMLTPSPOTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["PERIODSPOTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowYieldCurve["DAYTYPESPOTOFFSET"]));
                    if (_yieldCurveDef.idCurvePtSchedSpecified)
                        _yieldCurveDef.idCurvePtSched = rowYieldCurve["IDCURVEPTSCHED"].ToString();
                    if (_yieldCurveDef.interpolationMethodZCSpecified)
                        _yieldCurveDef.interpolationMethodZC = (InterpolationMethodEnum)System.Enum.Parse(typeof(InterpolationMethodEnum),
                            rowYieldCurve["INTERPOLMETHODZC"].ToString(), true);
                    if (_yieldCurveDef.polynomialDegreeZCSpecified)
                        _yieldCurveDef.polynomialDegreeZC = Convert.ToInt32(rowYieldCurve["POLYNOMIALDEGREEZC"]);
                    if (_yieldCurveDef.interpolationMethodDFSpecified)
                        _yieldCurveDef.interpolationMethodDF = (InterpolationMethodEnum)System.Enum.Parse(typeof(InterpolationMethodEnum),
                            rowYieldCurve["INTERPOLMETHODDF"].ToString(), true);
                    if (_yieldCurveDef.polynomialDegreeDFSpecified)
                        _yieldCurveDef.polynomialDegreeDF = Convert.ToInt32(rowYieldCurve["POLYNOMIALDEGREEDF"]);
                    if (_yieldCurveDef.spotOffsetSpecified)
                        _yieldCurveDef.spotOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowYieldCurve["PERIODSPOTOFFSET"].ToString()),
                            Convert.ToInt32(rowYieldCurve["PERIODMLTPSPOTOFFSET"]),
                            StringToEnum.DayType(rowYieldCurve["DAYTYPESPOTOFFSET"].ToString()));
                    if (_yieldCurveDef.endOffsetSpecified)
                        _yieldCurveDef.endOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowYieldCurve["PERIODENDOFFSET"].ToString()),
                            Convert.ToInt32(rowYieldCurve["PERIODMLTPENDOFFSET"]),
                            StringToEnum.DayType(rowYieldCurve["DAYTYPEENDOFFSET"].ToString()));

                    _yieldCurveDef.SetBusinessCentersAndDCF(pCS, pProductBase);
                }
            }
            else
            {
                ret = SetLogMessage("YieldCurve definition is not specified for currency {0}, please add a new definition curve for this currency", _keyQuoteCurve.idC);
            }
            return ret;
        }
        
        /// <summary>
        /// Charge de la courbe de taux 
        /// <para>Exploite la table YIELDCURVEVAL_H</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pCurveParam"></param>
        private void LoadYieldCurveVal(string pCS)
        {
            _yieldCurveVal = null;
            
            QueryParameters qry = GetSelectYieldCurveVal(pCS);
            
            DataSet ds = DataHelper.ExecuteDataset(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            ds.DataSetName = "dsYieldCurveVal";
            DataTable dtYieldCurveVal = ds.Tables[0];
            dtYieldCurveVal.TableName = "YieldCurveVal";
            
            #region YieldCurveVal filling
            if (1 == ArrFunc.Count(dtYieldCurveVal.Rows))
            {
                DataRow rowYieldCurveVal = dtYieldCurveVal.Rows[0];
                _yieldCurveVal = new YieldCurveVal(this)
                {
                    idYieldCurveVal_H = Convert.ToInt32(rowYieldCurveVal["IDYIELDCURVEVAL_H"]),
                    idYieldCurveDef = _yieldCurveDef.idYieldCurveDef,

                    idMarketEnv = rowYieldCurveVal["IDMARKETENV"].ToString(),
                    idValScenario = rowYieldCurveVal["IDVALSCENARIO"].ToString(),

                    curveSideSpecified = (false == Convert.IsDBNull(rowYieldCurveVal["CURVESIDE"])),
                    cashFlowTypeSpecified = (false == Convert.IsDBNull(rowYieldCurveVal["CASHFLOWTYPE"])),

                    dtBase = Convert.ToDateTime(rowYieldCurveVal["DTBASE"]),
                    dtSpot = Convert.ToDateTime(rowYieldCurveVal["DTSPOT"]),
                    dtInputData = Convert.ToDateTime(rowYieldCurveVal["DTINPUTDATA"]),
                    dtEnd = Convert.ToDateTime(rowYieldCurveVal["DTEND"]),
                    dtBuildDate = Convert.ToDateTime(rowYieldCurveVal["DTBUILDDATE"]),
                    isEnabled = Convert.ToBoolean(rowYieldCurveVal["ISENABLED"]),
                    source = rowYieldCurveVal["SOURCE"].ToString()
                };

                if (_yieldCurveVal.curveSideSpecified)
                {
                    _yieldCurveVal.curveSide = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum),
                        rowYieldCurveVal["CURVESIDE"].ToString(), true);
                }
                
                if (_yieldCurveVal.cashFlowTypeSpecified)
                    _yieldCurveVal.cashFlowType = rowYieldCurveVal["CASHFLOWTYPE"].ToString();
               
            }
            #endregion YieldCurveVal filling
        }
        
        /// <summary>
        /// => Charge la définition de la courbe de taux (table YIELDCURVEDEF)
        /// => Charge de la courbe de taux (table YIELDCURVEVAL_H) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        private void Initialize(string pCS, IProductBase pProduct)
        {
            if (Cst.ErrLevel.SUCCESS == LoadYieldCurveDef(pCS, pProduct))
                LoadYieldCurveVal(pCS);
        }
        
        #endregion Methods
    }
    #endregion YieldCurve
    #region CompareYieldCurvePoint
    /// <summary>
    /// Compare 2 points d'une courbe
    /// <para>Le point de maturité plus proche est plus petit que le point de maturité plus élévé</para>
    /// </summary>
    public class CompareYieldCurvePoint : IComparer
    {
        public int Compare(object pYieldCurvePoint1, object pYieldCurvePoint2)
        {
            YieldCurvePoint yieldCurvePoint1 = (YieldCurvePoint)pYieldCurvePoint1;
            YieldCurvePoint yieldCurvePoint2 = (YieldCurvePoint)pYieldCurvePoint2;
            return (yieldCurvePoint1.daysCurvePoint.CompareTo(yieldCurvePoint2.daysCurvePoint));
        }
    }
    #endregion CompareYieldCurvePoint
    #region CurvePtSched
    public class CurvePtSched
    {
        #region Members
        public string idCurvePtSched;
        public string displayName;
        public bool isDefault;
        public bool curvePtSchedDetsSpecified;
        public CurvePtSchedDet[] curvePtSchedDets;
        #endregion Members
        //
        #region Accessors

        #endregion Accessors
        #region Constructors
        public CurvePtSched() { }
        #endregion Constructors
    }
    #endregion CurvePtSched
    #region CurvePtSchedDet
    public class CurvePtSchedDet
    {
        #region Members
        public string idCurvePtSched;
        public IInterval tenor;
        #endregion Members
        #region Constructors
        public CurvePtSchedDet() { }
        #endregion Constructors
    }
    #endregion CurvePtSchedDet
    #region YieldCurveDef
    /// <summary>
    /// Représente une définition d'une courbe de taux
    /// <para>Contient la liste des benchmark</para> 
    /// <para>Contient la liste des points users</para> 
    /// </summary>
    public class YieldCurveDef
    {
        #region Members
        private IBusinessCenters _businessCenters;
        private DayCountFractionEnum _dayCountFraction;
        //
        public string idYieldCurveDef;
        public string idC;
        //
        public bool idCurvePtSchedSpecified;
        public string idCurvePtSched;
        //
        public string displayName;
        public YieldCurveTypeEnum curveType;
        //
        public bool isDefault;
        public bool isImported;
        public CompoundingFrequencyEnum compoundFrequency;
        //
        public bool interpolationMethodZCSpecified;
        public InterpolationMethodEnum interpolationMethodZC;
        //
        public bool interpolationMethodDFSpecified;
        public InterpolationMethodEnum interpolationMethodDF;
        //
        public bool polynomialDegreeZCSpecified;
        public int polynomialDegreeZC;
        //
        public bool polynomialDegreeDFSpecified;
        public int polynomialDegreeDF;
        //
        public bool isAdjustmentFuture;
        //
        public bool isExtrapolation;
        //
        public bool spotOffsetSpecified;
        public IOffset spotOffset;
        //
        public bool inputOffsetSpecified;
        public IOffset inputOffset;
        //
        public bool endOffsetSpecified;
        public IOffset endOffset;
        //
        public bool curvePtSchedSpecified;
        /// <summary>
        /// Représente la liste des points user
        /// </summary>
        public CurvePtSched curvePtSched;
        //
        public bool benchmarkRefsSpecified;
        /// <summary>
        /// Représente la liste des BenchMarks
        /// </summary>
        public BenchmarkRef[] benchmarkRefs;
        //
        #endregion Members
        //
        #region Accessors
        #region businessCenters
        /// <summary>
        /// Obtient les businessCenters rattachés à la définition de courbe de taux
        /// </summary>
        public IBusinessCenters BusinessCenters
        {
            get { return _businessCenters; }
        }
        #endregion businessCenters
        #region dayCountFraction
        /// <summary>
        /// Obtient la DCF rattaché à la définition de courbe de taux
        /// </summary>
        public DayCountFractionEnum DayCountFraction
        {
            get { return _dayCountFraction; }
        }
        #endregion dayCountFraction
        #endregion Accessors
        //
        #region Constructors
        public YieldCurveDef() { }
        #endregion Constructors
        //
        #region Methods
        //
        #region public LoadCurvePtSched
        /// <summary>
        /// Charge le modèle de points
        /// <para>Exploite les tables CURVEPTSCHED et CURVEPTSCHEDDET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        public void LoadCurvePtSched(string pCS, IProductBase pProductBase)
        {
            QueryParameters qryCurvePtSched = GetSelectCurvePtSched(pCS);
            QueryParameters qryCurvePtSchedDet = GetSelectCurvePtSchedDet(pCS);
            //
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += qryCurvePtSched.Query + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += qryCurvePtSchedDet.Query + SQLCst.SEPARATOR_MULTISELECT;
            DataSet dsCurvePtSched = DataHelper.ExecuteDataset(qryCurvePtSched.Cs , CommandType.Text, sqlSelect.ToString(), qryCurvePtSched.Parameters.GetArrayDbParameter());
            //
            #region DataSet Initialize
            dsCurvePtSched.DataSetName = "CurvePtSched";
            DataTable dtCurvePtSched = dsCurvePtSched.Tables[0];
            dtCurvePtSched.TableName = "CurvePtSched";
            DataTable dtCurvePtSchedDet = dsCurvePtSched.Tables[1];
            dtCurvePtSchedDet.TableName = "CurvePtSchedDet";
            #endregion DataSet Initialize
            //
            curvePtSchedSpecified = (null != dsCurvePtSched) && (1 == dtCurvePtSched.Rows.Count);
            if (curvePtSchedSpecified)
            {
                #region CurvePtSched filling
                DataRow rowCurvePtSched = dtCurvePtSched.Rows[0];
                curvePtSched = new CurvePtSched();
                curvePtSched.idCurvePtSched = rowCurvePtSched["IDCURVEPTSCHED"].ToString();
                curvePtSched.displayName = rowCurvePtSched["DISPLAYNAME"].ToString();
                curvePtSched.isDefault = Convert.ToBoolean(rowCurvePtSched["ISDEFAULT"]);
                #endregion CurvePtSched filling

                #region CurvePtSchedDet filling
                curvePtSched.curvePtSchedDetsSpecified = (0 < dtCurvePtSchedDet.Rows.Count);
                if (curvePtSched.curvePtSchedDetsSpecified)
                {
                    ArrayList aCurvePtSchedDets = new ArrayList();
                    foreach (DataRow rowCurvePtSchedDet in dtCurvePtSchedDet.Rows)
                    {
                        CurvePtSchedDet curvePtSchedDet = new CurvePtSchedDet
                        {
                            idCurvePtSched = rowCurvePtSchedDet["IDCURVEPTSCHED"].ToString(),
                            tenor = pProductBase.CreateInterval(
                            StringToEnum.Period(rowCurvePtSchedDet["PERIOD"].ToString()),
                            Convert.ToInt32(rowCurvePtSchedDet["PERIODMLTP"]))
                        };
                        aCurvePtSchedDets.Add(curvePtSchedDet);
                    }
                    curvePtSched.curvePtSchedDets = (CurvePtSchedDet[])aCurvePtSchedDets.ToArray(typeof(CurvePtSchedDet));
                }
                #endregion CurvePtSchedDet filling
            }
        }
        #endregion LoadCurvePtSched
        #region public LoadBenchMark
        /// <summary>
        /// Charge les points de BenchMark
        /// <para>Exploite la table BENCHMARKREF</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pDateCalculation"></param>
        public void LoadBenchMark(string pCS, IProductBase pProductBase, DateTime pDateCalculation)
        {
            QueryParameters qryParameters = GetSelectBenchmarkRef(pCS);
            //
            DataSet ds = DataHelper.ExecuteDataset(qryParameters.Cs , CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            ds.DataSetName = "dsBenchmarkRef";
            DataTable dtBenchmarkRef = ds.Tables[0];
            dtBenchmarkRef.TableName = "BenchmarkRef";
            //
            benchmarkRefsSpecified = (0 < dtBenchmarkRef.Rows.Count);
            if (benchmarkRefsSpecified)
            {
                ArrayList al = new ArrayList();
                foreach (DataRow rowBenchmark in dtBenchmarkRef.Rows)
                {
                    Cst.UnderlyingAsset_Rate assetCategory = (Cst.UnderlyingAsset_Rate)System.Enum.Parse(typeof(Cst.UnderlyingAsset_Rate),
                                                            rowBenchmark["ASSETCATEGORY"].ToString(), true);
                    //
                    BenchmarkRef[] benchmarkRef;
                    if (assetCategory == Cst.UnderlyingAsset_Rate.Future)
                    {
                        //Si contrat Future, Spheres® recherche les assets Futures disponibles  
                        benchmarkRef = GetAssetsFromBenchMarkFuture(pCS, rowBenchmark, pDateCalculation, pProductBase);
                    }
                    else
                    {
                        benchmarkRef = new BenchmarkRef[1] { new BenchmarkRef() };
                        benchmarkRef[0].assetCategory = assetCategory;
                        benchmarkRef[0].baseRateType = (BaseRateTypeEnum)System.Enum.Parse(typeof(BaseRateTypeEnum), rowBenchmark["BASERATETYPE"].ToString(), true);
                        benchmarkRef[0].idAsset = IntFunc.IntValue(rowBenchmark["IDASSETREF"].ToString());
                    }
                    //
                    if (ArrFunc.IsFilled(benchmarkRef))
                        al.AddRange(benchmarkRef);
                }
                benchmarkRefs = (BenchmarkRef[])al.ToArray(typeof(BenchmarkRef));
            }
        }
        #endregion
        //
        #region public SetBusinessCentersAndDCF
        /// <summary>
        ///  Détermine les business Centers et la DCF rattachés à la définition de courbe de taux
        /// <para>ces derniers sont issus de la devise de la définition de courbe de taux</para>
        /// <para>Ces données sont accessibles depuis les properties businessCenters et dayCountFraction</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        public void SetBusinessCentersAndDCF(string pCS, IProductBase pProductBase)
        {
            SQL_Currency currency = new SQL_Currency(pCS, SQL_Currency.IDType.IdC, idC);
            if (currency.IsLoaded)
            {
                _businessCenters = pProductBase.CreateBusinessCenters(currency.IdBC, currency.IdBC2, currency.IdBC3);
                _dayCountFraction = currency.FpML_Enum_DayCountFraction;
            }
        }
        #endregion
        //
        #region private GetAssetsFromBenchMarkFuture
        /// <summary>
        /// Obtient un array de BenchmarkRef de type Future à partir du paramétrage de la table BENCHMARKREF
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRowBenchmark"></param>
        /// <param name="pDateCalculation"></param>
        /// <param name="productBase"></param>
        /// <returns></returns>
        private BenchmarkRef[] GetAssetsFromBenchMarkFuture(string pCS, DataRow pRowBenchmark, DateTime pDateCalculation, IProductBase productBase)
        {
            BenchmarkRef[] ret = null;
            ArrayList al = new ArrayList();
            
            //IDASSETREF
            int idDC = IntFunc.IntValue(pRowBenchmark["IDASSETREF"].ToString());
            SQL_DerivativeContract sqlDC = new SQL_DerivativeContract(pCS, idDC, SQL_Table.ScanDataDtEnabledEnum.Yes);
            bool isLoad = sqlDC.LoadTable();
            if (false == isLoad)
            {
                sqlDC = new SQL_DerivativeContract(pCS, idDC, SQL_Table.ScanDataDtEnabledEnum.No);
                if (false == sqlDC.IsLoaded)
                    throw new Exception(StrFunc.AppendFormat("Derivative Contract (OTCmlId:{0}) not found", idDC));
            }
            
            //BASERATETYPE
            BaseRateTypeEnum baseRateType = (BaseRateTypeEnum)System.Enum.Parse(typeof(BaseRateTypeEnum), pRowBenchmark["BASERATETYPE"].ToString(), true);
            
            //FREQUENCYMATURITY
            string freqMaturity = string.Empty;
            if (pRowBenchmark["FREQUENCYMATURITY"] != Convert.DBNull)
                freqMaturity = pRowBenchmark["FREQUENCYMATURITY"].ToString();
            if (StrFunc.IsEmpty(freqMaturity))
                throw new Exception("Frequency maturity is empty, please specify a maturity for Future");
            else if ((freqMaturity != Cst.FrequencyAnnual) & (freqMaturity != Cst.FrequencyMonthly) & (freqMaturity != Cst.FrequencyQuaterly))
                throw new Exception(StrFunc.AppendFormat("Frequency maturity {0} is not implemented", freqMaturity));
            
            int periodMultiplierMin = 0;
            if (pRowBenchmark["PERIODMLTPMIN"] != Convert.DBNull)
            {
                periodMultiplierMin = IntFunc.IntValue(pRowBenchmark["PERIODMLTPMIN"].ToString());
            }
            Nullable<PeriodEnum> periodMin = null;
            if (pRowBenchmark["PERIODMIN"] != Convert.DBNull)
            {
                periodMin = (PeriodEnum)System.Enum.Parse(typeof(PeriodEnum), pRowBenchmark["PERIODMIN"].ToString(), true);
            }

            int periodMultiplierMax = 0;
            if (pRowBenchmark["PERIODMLTPMAX"] != Convert.DBNull)
            {
                periodMultiplierMin = IntFunc.IntValue(pRowBenchmark["PERIODMLTPMAX"].ToString());
            }
            Nullable<PeriodEnum> periodMax = null;
            if (pRowBenchmark["PERIODMAX"] != Convert.DBNull)
            {
                periodMin = (PeriodEnum)System.Enum.Parse(typeof(PeriodEnum), pRowBenchmark["PERIODMAX"].ToString(), true);
            }
            
            Nullable<DateTime> minDate = null;
            if ((periodMultiplierMin > 0) && (null != periodMin))
            {
                IOffset offset = productBase.CreateOffset((PeriodEnum)periodMin, periodMultiplierMin, DayTypeEnum.Calendar);
                minDate = Tools.ApplyOffset(pCS, pDateCalculation, offset, (IBusinessCenters)null);
            }
            
            Nullable<DateTime> maxDate = null;
            if ((periodMultiplierMax > 0) && (null != periodMax))
            {
                IOffset offset = productBase.CreateOffset((PeriodEnum)periodMin, periodMultiplierMax, DayTypeEnum.Calendar);
                maxDate = Tools.ApplyOffset(pCS, pDateCalculation, offset, (IBusinessCenters)null);
            }
            
            //SQL_MaturityRule sqlMaturityRule = new SQL_MaturityRule(pCS, sqlDC.IdMaturityRule, SQL_Table.ScanDataDtEnabledEnum.Yes);
            //PL 20131112 [TRIM 19164]
            SQL_MaturityRuleActive sqlMaturityRule = new SQL_MaturityRuleActive(pCS, sqlDC.IdMaturityRule,  DateTime.MinValue);
            isLoad = sqlMaturityRule.LoadTable();
            if (isLoad)
            {
                int month = 1;
                string fut_MMYRule = sqlMaturityRule.MaturityMMYRule;
                if (!String.IsNullOrEmpty(fut_MMYRule))
                {
                    string firstMonth = string.Empty;
                    if (sqlMaturityRule.IsMaturityMMYRule_Multiple)
                    {
                        string[] maturityRules = fut_MMYRule.Split("|".ToCharArray());
                        foreach (string maturityRule in maturityRules)
                        {
                            if (maturityRule.StartsWith("m"))
                            {
                                //m --> month. ex.: m=FGJKNQVX|q=HMUZ
                                int posEqual = maturityRule.IndexOf("=");
                                if (posEqual > 0)
                                {
                                    firstMonth = StrFunc.GetMonthMM(maturityRule.Substring(posEqual, 1));
                                }
                                break;
                            }
                        }
                    }
                    else if ((sqlMaturityRule.MaturityFormat == Cst.MaturityMonthYearFmtEnum.YearMonthOnly.ToString())
                        || ((!sqlMaturityRule.IsMaturityMMYRule_Daily) && (!sqlMaturityRule.IsMaturityMMYRule_Weekly)))
                    {
                        firstMonth = StrFunc.GetMonthMM(fut_MMYRule.Substring(0, 1));        
                    }
                    
                    if (firstMonth != "XX")
                    {
                        month = IntFunc.IntValue(firstMonth);
                    }
                }
               
                switch (freqMaturity)
                {
                    case Cst.FrequencyMonthly:
                        try
                        {
                            bool isNextMaturityMonthYear = new DateTime(pDateCalculation.Year, month, 01).CompareTo(pDateCalculation) > 0;
                            while (false == isNextMaturityMonthYear)
                            {
                                month++;
                                isNextMaturityMonthYear = new DateTime(pDateCalculation.Year, month, 01).CompareTo(pDateCalculation) > 0;
                            }
                            
                            //Ajout de l'échéance mensuelle en cours à la date pDateCalculation
                            month--;
                            string maturityMonthYear = pDateCalculation.Year.ToString() + StrFunc.GetMonthMM(month.ToString());
                            AddBenchmarkRef_CurrentAssetFuture(pCS, maturityMonthYear, sqlDC.Id, al, pDateCalculation, baseRateType, minDate, maxDate);
                            
                            //Ajout des échéances mensuelle postérieure à la date pDateCalculation
                            month++;
                            AddBenchmarkRef_NextAssetFuture(pCS, sqlDC.Id, al, pDateCalculation, baseRateType, pDateCalculation.Year, month, Cst.FrequencyMonthly, minDate, maxDate);
                        }
                        catch { throw; }
                        break;
                    case Cst.FrequencyQuaterly:
                        try
                        {
                            bool isNextMaturityMonthYear = new DateTime(pDateCalculation.Year, month, 01).CompareTo(pDateCalculation) > 0;
                            while (false == isNextMaturityMonthYear)
                            {
                                if (month == 1)
                                    month = 0;
                                month += 3;
                                isNextMaturityMonthYear = new DateTime(pDateCalculation.Year, month, 01).CompareTo(pDateCalculation) > 0;
                            }

                            //Ajout de l'échéance trimestrielle en cours à la date pDateCalculation
                            month -= 3;
                            string maturityMonthYear = pDateCalculation.Year.ToString() + StrFunc.GetMonthMM(month.ToString());
                            AddBenchmarkRef_CurrentAssetFuture(pCS, maturityMonthYear, sqlDC.Id, al, pDateCalculation, baseRateType, minDate, maxDate);
                            
                            //Ajout des échéances trimestrielles postérieures à la date pDateCalculation
                            month += 3;
                            AddBenchmarkRef_NextAssetFuture(pCS, sqlDC.Id, al, pDateCalculation, baseRateType, pDateCalculation.Year, month, Cst.FrequencyQuaterly, minDate, maxDate);
                        }
                        catch { throw; }
                        break;


                    case Cst.FrequencyAnnual:
                        try
                        {
                            int year = pDateCalculation.Year;
                            bool isNextMaturityMonthYear = new DateTime(year, month, 01).CompareTo(pDateCalculation) > 0;
                            while (false == isNextMaturityMonthYear)
                            {
                                year++;
                                isNextMaturityMonthYear = new DateTime(year, month, 01).CompareTo(pDateCalculation) > 0;
                            }
                            //Ajout de l'échéance anuelle en cours à la date pDateCalculation
                            year--;
                            string maturityMonthYear = year.ToString() + StrFunc.GetMonthMM(month.ToString());
                            AddBenchmarkRef_CurrentAssetFuture(pCS, maturityMonthYear, sqlDC.Id, al, pDateCalculation, baseRateType, minDate, maxDate);
                            //Ajout des échéances anuelles postérieures à la date pDateCalculation
                            year++;
                            AddBenchmarkRef_NextAssetFuture(pCS, sqlDC.Id, al, pDateCalculation, baseRateType, year, month, Cst.FrequencyAnnual, minDate, maxDate);
                        }
                        catch { throw; }
                        break;
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (BenchmarkRef[])al.ToArray(typeof(BenchmarkRef));
            //
            return ret;
        }
        #endregion
        #region private GetSelectBenchmarkRef
        /// <summary>
        /// Obtient la query SELECT sur la table BENCHMARKREF
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private QueryParameters GetSelectBenchmarkRef(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"bm.IDYIELDCURVEDEF," + Cst.CrLf;
            query += @"bm.ASSETCATEGORY, bm.IDASSETREF, bm.BASERATETYPE," + Cst.CrLf;
            query += @"bm.FREQUENCYMATURITY," + Cst.CrLf;
            query += @"bm.PERIODMLTPMIN, bm.PERIODMIN," + Cst.CrLf;
            query += @"bm.PERIODMLTPMAX, bm.PERIODMAX," + Cst.CrLf;
            query += @"bm.DTINS, bm.IDAINS, bm.DTUPD, bm.IDAUPD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.BENCHMARKREF + " bm " + Cst.CrLf;
            query += SQLCst.WHERE + @"(bm.IDYIELDCURVEDEF =@IDYIELDCURVEDEF)" + Cst.CrLf;
            query += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "bm").ToString();
            //                
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEDEF", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), idYieldCurveDef);
            //
            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);
            return ret;
        }
        #endregion GetSelectBenchmarkRef
        #region private GetSelectCurvePtSched
        /// <summary>
        /// Obtient l'instruction select qui charge la Table CURVEPTSCHED
        /// </summary>
        private QueryParameters GetSelectCurvePtSched(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"cs.IDCURVEPTSCHED, cs.DISPLAYNAME, cs.ISDEFAULT" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.CURVEPTSCHED + " cs " + Cst.CrLf;
            query += SQLCst.WHERE + @"(cs.IDCURVEPTSCHED=@IDCURVEPTSCHED)" + Cst.CrLf;
            query += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "cs");
            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDCURVEPTSCHED", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), this.idCurvePtSched);
            //
            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);
            return ret;
        }
        #endregion GetSelectCurvePtSched
        #region private GetSelectCurvePtSchedDet
        /// <summary>
        /// Obtient l'instruction select qui charge la Table CURVEPTSCHEDDET (une liste de tenor)
        /// </summary>
        private QueryParameters GetSelectCurvePtSchedDet(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"csd.IDCURVEPTSCHED, csd.PERIODMLTP, csd.PERIOD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.CURVEPTSCHEDDET + " csd " + Cst.CrLf;
            query += SQLCst.WHERE + @"(csd.IDCURVEPTSCHED=@IDCURVEPTSCHED)" + Cst.CrLf;
            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDCURVEPTSCHED", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), this.idCurvePtSched);
            //   
            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);
            return ret;
        }
        #endregion GetSelectCurvePtSchedDet
        //
        #region private static AddBenchmarkRef_CurrentAssetFuture
        /// <summary>
        /// Ajoute l'asset future en cours à la date de calculation
        /// <para>cet asset peut être de maturité mensuelle, trimestrielle ou encore annuelle</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMaturityMonthYear">Représente la maturité au format MonthYear</param>
        /// <param name="pIdDC">Représente le Derivative Contract</param>
        /// <param name="pArrayList">Représente la liste dans laquelle va être ajouté l'asset</param>
        /// <param name="pDateCalculation"></param>
        /// <param name="pBaseRate"></param>
        private static void AddBenchmarkRef_CurrentAssetFuture(string pCS, string pMaturityMonthYear, int pIdDC, ArrayList pArrayList, DateTime pDateCalculation, BaseRateTypeEnum pBaseRate, Nullable<DateTime> pMinDate, Nullable<DateTime> pMaxDate)
        {
            SQL_AssetETD sqlAsset = new SQL_AssetETD(pCS, SQL_TableWithID.IDType.UNDEFINED, string.Empty, SQL_Table.ScanDataDtEnabledEnum.No)
            {
                IdDC_In = pIdDC,
                MaturityMonthYear_In = pMaturityMonthYear
            };
            bool isFind = sqlAsset.LoadTable();
            if (isFind)
            {
                if (IsAssetFutureAvailable(sqlAsset, pDateCalculation, pMinDate, pMaxDate))
                    AddBenchmarkRefAssetFuture(pArrayList, sqlAsset, pBaseRate);
            }
        }
        #endregion
        #region private static AddBenchmarkRef_NextAssetFuture
        /// <summary>
        /// Ajoute Les asset futures de maturité postérieure à la date de calcuation
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdDC">Représente le Derivative Contract</param>
        /// <param name="pArrayList">Représente la liste dans laquelle vont être ajoutés les assets</param>
        /// <param name="pDateCalculation"></param>
        /// <param name="pBaseRate"></param>
        /// <param name="pDateStart"></param>
        /// <param name="pMonthStart"></param>
        /// <param name="pFrequence"></param>
        private static void AddBenchmarkRef_NextAssetFuture(string pCS, int pIdDC, ArrayList pArrayList, DateTime pDateCalculation, BaseRateTypeEnum pBaseRate, int pYearStart, int pMonthStart, string pFrequence, Nullable<DateTime> pMinDate, Nullable<DateTime> pMaxDate)
        {
            DateTime currentDate = new DateTime(pYearStart, pMonthStart, 01);
            bool isFind = true;
            while (isFind)
            {
                SQL_AssetETD sqlAsset = new SQL_AssetETD(pCS, SQL_TableWithID.IDType.UNDEFINED, string.Empty, SQL_Table.ScanDataDtEnabledEnum.No)
                {
                    IdDC_In = pIdDC,
                    MaturityMonthYear_In = currentDate.Year.ToString() + StrFunc.GetMonthMM(currentDate.Month.ToString())
                };

                isFind = sqlAsset.LoadTable();
                if (isFind)
                {
                    if (IsAssetFutureAvailable(sqlAsset, pDateCalculation, pMinDate, pMaxDate))
                        AddBenchmarkRefAssetFuture(pArrayList, sqlAsset, pBaseRate);
                    //
                    if (pFrequence == Cst.FrequencyMonthly)
                        currentDate = currentDate.AddMonths(1);
                    else if (pFrequence == Cst.FrequencyQuaterly)
                        currentDate = currentDate.AddMonths(3);
                    else if (pFrequence == Cst.FrequencyAnnual)
                        currentDate = currentDate.AddMonths(12);
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Frequence {0} is not implemented", pFrequence));
                }
            }
        }
        #endregion
        #region private static AddBenchmarkRefAssetFuture
        /// <summary>
        /// Ajoute dans la liste {pArrayList} un BenchmarkRef 
        /// </summary>
        /// <param name="pArrayList"></param>
        /// <param name="pSqlAsset"></param>
        /// <param name="pBaseRateType"></param>
        private static void AddBenchmarkRefAssetFuture(ArrayList pArrayList, SQL_AssetETD pSqlAsset, BaseRateTypeEnum pBaseRateType)
        {
            BenchmarkRef benchmarkRefItem = new BenchmarkRef
            {
                idAsset = pSqlAsset.Id,
                baseRateType = pBaseRateType,
                assetCategory = Cst.UnderlyingAsset_Rate.Future
            };
            pArrayList.Add(benchmarkRefItem);
        }
        #endregion
        #region private static isAssetFutureAvailable
        /// <summary>
        /// Obtient true si l'asset Future peut être ajouté aux points de BenchMark
        /// </summary>
        /// <param name="pSqlAsset"></param>
        /// <param name="pDateCalculation"></param>
        /// <param name="pMinDate"></param>
        /// <param name="pMaxDate"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private static bool IsAssetFutureAvailable(SQL_AssetETD pSqlAsset, DateTime pDateCalculation, Nullable<DateTime> pMinDate, Nullable<DateTime> pMaxDate)
        {
            try
            {
                SQL_AssetETD sqlAsset = pSqlAsset;
                //
                bool ret = sqlAsset.IsEnabled;
                ret &= (sqlAsset.Maturity_MaturityDate.CompareTo(pDateCalculation) > 0);
                //
                if (ret)
                {
                    if (DtFunc.IsDateTimeFilled(sqlAsset.FirstQuotationDay))
                        ret &= (sqlAsset.FirstQuotationDay.CompareTo(pDateCalculation) >= 0);
                }
                //
                if (ret)
                {
                    if (null != pMinDate)
                        ret &= sqlAsset.Maturity_MaturityDate.CompareTo(pMinDate) > 0;
                    if (null != pMaxDate)
                        ret &= sqlAsset.Maturity_MaturityDate.CompareTo(pMaxDate) <= 0;
                }
                //
                return ret;
            }
            catch (Exception) { throw; }
        }
        #endregion
        #endregion Methods
    }
    #endregion YieldCurveDef
    #region YieldCurveVal
    /// <summary>
    /// Représente une courbe de taux 
    /// </summary>
    public class YieldCurveVal
    {
        #region Members
        //
        public int idYieldCurveVal_H;
        public string idYieldCurveDef;
        //
        public string idMarketEnv;
        public string idValScenario;
        //
        public bool curveSideSpecified;
        public QuotationSideEnum curveSide;
        //
        public bool cashFlowTypeSpecified;
        public string cashFlowType;
        //
        public DateTime dtBase;
        public DateTime dtSpot;
        public DateTime dtInputData;
        public DateTime dtEnd;
        public DateTime dtBuildDate;
        //
        public bool isEnabled;
        public string source;
        //
        //
        private bool m_IsExistFuture;
        private int m_IndexFirstFuture;
        private int m_IndexLastFuture;
        //
        public bool yieldCurvePointsSpecified;
        /// <summary>
        /// Liste des points de la courbe
        /// </summary>
        public YieldCurvePoint[] yieldCurvePoints;
        #endregion Members
        //
        #region Accessors
        #region public Index_FirstFuture
        public int Index_FirstFuture
        {
            get { return m_IndexFirstFuture; }
            set { m_IndexFirstFuture = value; }
        }
        #endregion Index_FirstFuture
        #region public Index_LastFuture
        public int Index_LastFuture
        {
            get { return m_IndexLastFuture; }
            set { m_IndexLastFuture = value; }
        }
        #endregion Index_LastFuture

        public YieldCurve CurrentYieldCurve
        {
            get;
            set;
        }

        #endregion Accessors
        //
        #region Constructors
        public YieldCurveVal(YieldCurve pYieldCurve) 
        {
            CurrentYieldCurve = pYieldCurve;
        }
        #endregion Constructors
        //
        #region Indexors
        #region Indexors YieldCurvePoint
        /// <summary>
        /// Obtient le point tel que daysCurvePoint = {pDaysCurvePoint}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pDaysCurvePoint"></param>
        /// <returns></returns>
        public YieldCurvePoint this[double pDaysCurvePoint]
        {
            get
            {
                YieldCurvePoint ret = null;
                if (yieldCurvePointsSpecified)
                {
                    foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                    {
                        if (yieldCurvePoint.daysCurvePoint == pDaysCurvePoint)
                        {
                            ret = yieldCurvePoint;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        /// <summary>
        /// Obtient le 1er point de la courbe tel que baseRateType = {pBaseRateType}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// 
        /// <param name="pBaseRateType"></param>
        /// <returns></returns>
        public YieldCurvePoint this[BaseRateTypeEnum pBaseRateType]
        {
            get
            {
                YieldCurvePoint ret = null;
                if (yieldCurvePointsSpecified)
                {
                    foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                    {
                        if (yieldCurvePoint.baseRateType == pBaseRateType)
                        {
                            ret = yieldCurvePoint;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        /// <summary>
        /// Obtient le point de la courbe tel que baseRateType = {pBaseRateType} et dtTerm = {pDtTerm}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        public YieldCurvePoint this[BaseRateTypeEnum pBaseRateType, DateTime pDtTerm]
        {
            get
            {
                YieldCurvePoint ret = null;
                if (yieldCurvePointsSpecified)
                {
                    foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                    {
                        if ((yieldCurvePoint.baseRateType == pBaseRateType) && (yieldCurvePoint.dtTerm == pDtTerm))
                        {
                            ret = yieldCurvePoint;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion Indexors YieldCurvePoint
        #endregion Indexors
        //
        #region Methods
        #region public Add
        /// <summary>
        /// Ajoute un point dans la courbe 
        /// <para>Alimente yieldCurvePoints</para>
        /// </summary>
        /// <param name="pYieldCurvePoint"></param>
        public void Add(YieldCurvePoint pYieldCurvePoint)
        {
            ArrayList aYieldCurvePoints = new ArrayList();
            if ((yieldCurvePointsSpecified) && (0 < yieldCurvePoints.Length))
            {
                for (int i = 0; i < yieldCurvePoints.Length; i++)
                {
                    if (null != yieldCurvePoints[i])
                        aYieldCurvePoints.Add(yieldCurvePoints[i]);
                }
            }
            aYieldCurvePoints.Add(pYieldCurvePoint);
            //
            yieldCurvePoints = (YieldCurvePoint[])aYieldCurvePoints.ToArray(typeof(YieldCurvePoint));
            yieldCurvePointsSpecified = ArrFunc.IsFilled(yieldCurvePoints);
        }
        #endregion Add
        #region public CalculYieldCurvePoints
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pYieldCurveDef"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public Cst.ErrLevel CalculYieldCurvePoints(string pCS, IProductBase pProductBase, YieldCurveDef pYieldCurveDef)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            Nullable<QuotationSideEnum> _curveSide = null; ;
            if (curveSideSpecified)
                _curveSide = curveSide;
            //
            if (yieldCurvePointsSpecified)
            {
                #region InputRate for BenchMark Only
                ArrayList al = new ArrayList();
                foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                {
                    if (yieldCurvePoint.assetCategorySpecified)
                    {
                        try
                        {
                            //Recherche de la cotation de l'asset, si asset non trouvé => exception Le traitement s'arrête ici
                            if (null != CurrentYieldCurve.ProcessCacheContainer)
                            {
                                if (Cst.ErrLevel.SUCCESS != yieldCurvePoint.InputRateCalculation(CurrentYieldCurve.ProcessCacheContainer, 
                                    dtBase, idMarketEnv, idValScenario, _curveSide))
                                    ret = Cst.ErrLevel.FAILURE;
                            }
                            else
                                yieldCurvePoint.InputRateCalculation(pCS, pProductBase, dtBase, idMarketEnv, idValScenario, _curveSide);
                        }
                        catch (SpheresException2 ex)
                        {
                            ret = Cst.ErrLevel.FAILURE;
                            if ((ex.ProcessState.CodeReturn == Cst.ErrLevel.DATANOTFOUND) || 
                                (ex.ProcessState.CodeReturn == Cst.ErrLevel.QUOTENOTFOUND) ||
                                (ex.ProcessState.CodeReturn == Cst.ErrLevel.QUOTEDISABLED))
                            {
                                al.Add(ex.Message);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                if (ArrFunc.IsFilled(al))
                {
                    string msg = StrFunc.StringArrayList.StringArrayToStringList((String[])al.ToArray(typeof(string)));
                    msg = msg.Replace(StrFunc.StringArrayList.LIST_SEPARATOR.ToString(), Cst.CrLf);
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msg, 
                        new ProcessState(ProcessStateTools.StatusErrorEnum,ProcessStateTools.CodeReturnDataNotFoundEnum));
                }
                #endregion InputRatefor BenchMark Only

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region BaseRate for BenchMark Only
                    foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                    {
                        if (yieldCurvePoint.assetCategorySpecified)
                        {
                            yieldCurvePoint.BaseRateCalculation(pProductBase, this);
                        }
                    }
                    #endregion
                    #region CalcYearFraction
                    foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                    {
                        if (Cst.ErrLevel.SUCCESS != yieldCurvePoint.YearFractionCalculation(pCS, pProductBase, this))
                            ret = Cst.ErrLevel.FAILURE;
                    }
                    #endregion
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        if (m_IsExistFuture)
                        {
                            #region BEFORE FUTURE (STUB INCLUDED) BaseRate/DiscountFactor/ZeroCouponYield
                            for (int i = 0; i <= Index_FirstFuture; i++)
                            {
                                if (Cst.ErrLevel.SUCCESS != yieldCurvePoints[i].BasicCalcul(pCS, pProductBase, this, pYieldCurveDef, true))
                                    ret = Cst.ErrLevel.FAILURE;
                            }
                            #endregion BEFORE FUTURE (STUB INCLUDED) BaseRate/DiscountFactor/ZeroCouponYield
                            #region FUTURE ONLY (STUB EXCLUDED) DiscountFactor/ZeroCouponYield
                            for (int i = Index_FirstFuture + 1; i <= Index_LastFuture; i++)
                            {
                                if (yieldCurvePoints[i].assetCategorySpecified &&
                                    (Cst.UnderlyingAsset_Rate.Future == yieldCurvePoints[i].assetCategory))
                                {
                                    #region DiscountFactorInitial
                                    yieldCurvePoints[i].DiscountFactorInitialCalculation(pCS, this);
                                    #endregion DiscountFactorInitial
                                    #region DiscountFactor
                                    if (Cst.ErrLevel.SUCCESS != yieldCurvePoints[i].DiscountFactorCalculation(pCS, pProductBase, this, pYieldCurveDef))
                                        ret = Cst.ErrLevel.FAILURE;
                                    #endregion DiscountFactor
                                    #region ZeroCouponYield
                                    yieldCurvePoints[i].ZeroCouponYieldCalculation(pYieldCurveDef.compoundFrequency);
                                    #endregion ZeroCouponYield
                                }
                            }
                            #endregion FUTURE ONLY (STUB EXCLUDED) DiscountFactor/ZeroCouponYield

                            #region DURING FUTURE (FUTURE EXCLUDED) DiscountFactor/ZeroCouponYield/BaseRate
                            for (int i = Index_FirstFuture + 1; i <= Index_LastFuture; i++)
                            {
                                if (Cst.UnderlyingAsset_Rate.Future != yieldCurvePoints[i].assetCategory)
                                {
                                    if (pYieldCurveDef.isAdjustmentFuture)
                                    {
                                        #region WITH AdjustmentFuture
                                        #region DiscountFactorInitial
                                        yieldCurvePoints[i].DiscountFactorInitialCalculation(pCS, this);
                                        #endregion DiscountFactorInitial
                                        #region DiscountFactor
                                        yieldCurvePoints[i].DiscountFactorInterpolationCalculation(this);
                                        #endregion DiscountFactor
                                        #region ZeroCouponYield
                                        yieldCurvePoints[i].ZeroCouponYieldInterpolationCalculation(this);
                                        #endregion ZeroCouponYield
                                        #region ImpliedRate
                                        yieldCurvePoints[i].ImpliedRateCalculation();
                                        #endregion ImpliedRate
                                        #endregion WITH AdjustmentFuture
                                    }
                                    else
                                    {
                                        #region WITHOUT AdjustmentFuture
                                        if (Cst.ErrLevel.SUCCESS != yieldCurvePoints[i].BasicCalcul(pCS, pProductBase, this, pYieldCurveDef, 
                                            (false == yieldCurvePoints[i].assetCategorySpecified)))
                                            ret = Cst.ErrLevel.FAILURE;
                                        #endregion WITHOUT AdjustmentFuture
                                    }
                                }
                            }

                            #endregion DURING FUTURE (FUTURE EXCLUDED) DiscountFactor/ZeroCouponYield/BaseRate
                            #region AFTER FUTURE DiscountFactor/ZeroCouponYield/ImpliedRate
                            for (int i = Index_LastFuture + 1; i <= yieldCurvePoints.Length - 1; i++)
                            {
                                if (Cst.ErrLevel.SUCCESS != yieldCurvePoints[i].BasicCalcul(pCS, pProductBase, this, pYieldCurveDef, true))
                                    ret = Cst.ErrLevel.FAILURE;
                            }
                            #endregion AFTER FUTURE DiscountFactor/ZeroCouponYield/ImpliedRate
                            #region FUTURE ONLY (STUB EXCLUDED) ImpliedRate
                            for (int i = Index_FirstFuture + 1; i <= Index_LastFuture; i++)
                            {
                                if (yieldCurvePoints[i].assetCategorySpecified &&
                                    (Cst.UnderlyingAsset_Rate.Future == yieldCurvePoints[i].assetCategory))
                                {
                                    yieldCurvePoints[i].ImpliedRateInterpolationCalculation(this);
                                }
                            }
                            #endregion FUTURE ONLY (STUB EXCLUDED) ImpliedRate
                        }
                        else
                        {
                            foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
                            {
                                if (Cst.ErrLevel.SUCCESS != yieldCurvePoint.BasicCalcul(pCS, pProductBase, this, pYieldCurveDef, 
                                    (false == yieldCurvePoint.assetCategorySpecified)))
                                    ret = Cst.ErrLevel.FAILURE;
                            }
                        }
                    }
                }
            }
            return ret;
        }
        #endregion CalculYieldCurvePoints
        #region public SetYieldCurvePoints
        /// <summary>
        /// Charge les points de la courbe de taux
        /// <para>S'ils n'existent pas alors ils sont générés</para>
        /// <para>S'ils existent alors ils sont triés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pYieldCurveDef"></param>
        public Cst.ErrLevel SetYieldCurvePoints(string pCS, IProductBase pProductBase, YieldCurveDef pYieldCurveDef)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            yieldCurvePoints = null;
            LoadYieldCurvePoint(pCS, pProductBase);
            if (false == yieldCurvePointsSpecified)
                ret = CreateYieldCurvePoints(pCS, pProductBase, pYieldCurveDef);
            else
                Sort();
            return ret;
        }
        #endregion CheckYieldCurvePoints

        #region public CreateYieldCurvePoints
        /// <summary>
        /// Génère les points de la courbe de taux
        /// <para>Génère les points des BENCHMARK, les points dits obligatoires, les points user, etc...</para>
        /// <para>calcule les DF et ZC etc...</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pYieldCurveDef"></param>
        public Cst.ErrLevel CreateYieldCurvePoints(string pCS, IProductBase pProductBase, YieldCurveDef pYieldCurveDef)
        {
            #region Points based to benchmarkref
            Cst.ErrLevel ret = CreateBenchmarkYieldCurvePoints(pCS, pProductBase, pYieldCurveDef);
            #endregion Points based to benchmarkref
            //                
            #region Mandatory Points (OneDay, SystemSpot, StubFRA, StubFutures...)
            if (Cst.ErrLevel.SUCCESS == ret)
                CreateMandatoryYieldCurvePoints(pCS, pProductBase, pYieldCurveDef);
            #endregion Mandatory Points (OneDay, SystemSpot, StubFRA, StubFutures...)
            //
            #region User Points based to CurvePtSched
            if (Cst.ErrLevel.SUCCESS == ret)
                CreateUserYieldCurvePoints(pCS, pYieldCurveDef);
            #endregion User Points based to CurvePtSched
            //
            #region yieldCurvePoints sorted definitly
            if (Cst.ErrLevel.SUCCESS == ret) 
                Sort();
            #endregion yieldCurvePoints sorted definitly
            //
            #region Set index of point StubFuture and Last Future if (exist)
            if ((Cst.ErrLevel.SUCCESS == ret) && m_IsExistFuture)
                SearchIndex_Future();
            #endregion Set index of point StubFuture and Last Future if (exist)
            //
            #region Calcul values for all YieldCurve point
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = CalculYieldCurvePoints(pCS, pProductBase, pYieldCurveDef);
            #endregion Calcul values for all YieldCurve point

            return ret;
        }
        #endregion CreateYieldCurvePoints

        #region public GetPointValue
        /// <summary>
        /// Obtient le résultat d'une interplation sur la courbe de taux
        /// </summary>
        /// <param name="pInterpolationMethod"></param>
        /// <param name="pYieldCurveValue"></param>
        /// <param name="pDtPoint"></param>
        /// <param name="pPolynomialDegree"></param>
        /// <returns></returns>
        public double GetPointValue(InterpolationMethodEnum pInterpolationMethod, YieldCurveValueEnum pYieldCurveValue, DateTime pDtPoint, int pPolynomialDegree)
        {
            double pointValue = 0;
            string name = pYieldCurveValue.ToString();
            TimeSpan timeSpan = pDtPoint - dtBase;
            double daysCurvePoint = Convert.ToDouble(timeSpan.Days);
            //
            if (0 < daysCurvePoint)
            {
                YieldCurvePoint yieldCurvePoint = this[daysCurvePoint];
                if (null != yieldCurvePoint)
                {
                    pointValue = yieldCurvePoint.GetValue(name);
                }
                else
                {
                    double[] daysCurvePoints = new double[yieldCurvePoints.Length];
                    double[] calculatedValue = new double[yieldCurvePoints.Length];
                    //
                    for (int i = 0; i < yieldCurvePoints.Length; i++)
                    {
                        daysCurvePoints[i] = yieldCurvePoints[i].daysCurvePoint;
                        calculatedValue[i] = yieldCurvePoints[i].GetValue(name);
                    }
                    EFS_Interp_Base interpolation;
                    switch (pInterpolationMethod)
                    {
                        case InterpolationMethodEnum.Linear:
                        case InterpolationMethodEnum.LinearCubic:
                        case InterpolationMethodEnum.LinearQuadratic:
                        case InterpolationMethodEnum.LinearLogarithmic:
                        case InterpolationMethodEnum.LinearExponential:
                            interpolation = new EFS_Interp_Linear(pInterpolationMethod, InterpolationTypeEnum.InterpolatedExtrapolated,
                                daysCurvePoints, calculatedValue, daysCurvePoint);
                            pointValue = interpolation.ResultValue;
                            break;
                        case InterpolationMethodEnum.Polynomial:
                            interpolation = new EFS_Interp_Polynomial(daysCurvePoints, calculatedValue, daysCurvePoint, pPolynomialDegree);
                            pointValue = interpolation.ResultValue;
                            break;
                        case InterpolationMethodEnum.CubicSpline:
                            interpolation = new EFS_Interp_CubicSpline(daysCurvePoints, calculatedValue, daysCurvePoint);
                            pointValue = interpolation.ResultValue;
                            break;
                    }
                }
            }
            return pointValue;
        }
        #endregion GetPointValue

        #region public Insert
        /// <summary>
        /// Alimente la l'historique des courbes de taux avec la courbe 
        /// <para>Alimente les tables YIELDCURVEVAL_H et YIELDCURVEPOINT_H</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSession"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public Cst.ErrLevel Insert(string pCS, IDbTransaction pDbTransaction, AppSession pSession)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEVAL_H", DbType.Int32));
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEDEF", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            //
            dp.Add(new DataParameter(pCS, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            dp.Add(new DataParameter(pCS, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            dp.Add(new DataParameter(pCS, "CASHFLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            dp.Add(new DataParameter(pCS, "CURVESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            //
            dp.Add(new DataParameter(pCS, "DTBASE", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "DTSPOT", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "DTINPUTDATA", DbType.DateTime));
            dp.Add(new DataParameter(pCS, "DTEND", DbType.DateTime));
            //
            dp.Add(new DataParameter(pCS, "DTBUILDDATE", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "ISENABLED", DbType.Boolean));
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS)); 
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS));
            dp.Add(new DataParameter(pCS, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            dp.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
            //
            string sql = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.YIELDCURVEVAL_H.ToString() + @" (
						IDYIELDCURVEVAL_H, IDYIELDCURVEDEF, 
                        IDMARKETENV, IDVALSCENARIO, CASHFLOWTYPE, CURVESIDE, 
                        DTBASE, DTSPOT, DTINPUTDATA, DTEND, DTBUILDDATE,
                        ISENABLED, DTINS, IDAINS, SOURCE, EXTLLINK) 
						values (
                        @IDYIELDCURVEVAL_H, @IDYIELDCURVEDEF, 
                        @IDMARKETENV, @IDVALSCENARIO, @CASHFLOWTYPE, @CURVESIDE, 
                        @DTBASE, @DTSPOT, @DTINPUTDATA, @DTEND, @DTBUILDDATE,
                        @ISENABLED, @DTINS, @IDAINS, @SOURCE, @EXTLLINK)";
            //
            Cst.ErrLevel errLevelGetId;
            if (null != pDbTransaction)
                errLevelGetId = SQLUP.GetId(out idYieldCurveVal_H, pDbTransaction, SQLUP.IdGetId.YIELDCURVEVAL_H, SQLUP.PosRetGetId.First, 1);
            else
                errLevelGetId = SQLUP.GetId(out idYieldCurveVal_H, pCS, SQLUP.IdGetId.YIELDCURVEVAL_H, SQLUP.PosRetGetId.First, 1);
            //
            if (Cst.ErrLevel.SUCCESS != errLevelGetId)
                ret = CurrentYieldCurve.SetLogMessage("Error:{0} on Get Id", errLevelGetId.ToString());
            else
            {
                #region YieldCurveVal Parameters Set
                dp["IDYIELDCURVEVAL_H"].Value = idYieldCurveVal_H;
                dp["IDYIELDCURVEDEF"].Value = idYieldCurveDef;
                //
                dp["IDMARKETENV"].Value = this.idMarketEnv;
                dp["IDVALSCENARIO"].Value = this.idValScenario;
                dp["CURVESIDE"].Value = curveSideSpecified ? curveSide.ToString() : Convert.DBNull;
                dp["CASHFLOWTYPE"].Value = cashFlowTypeSpecified ? cashFlowType : Convert.DBNull;
                //
                dp["DTBASE"].Value = dtBase;
                dp["DTSPOT"].Value = dtSpot;
                dp["DTINPUTDATA"].Value = dtInputData;
                dp["DTEND"].Value = dtEnd;
                dp["DTBUILDDATE"].Value = OTCmlHelper.GetDateBusiness(pCS);
                //
                dp["ISENABLED"].Value = true;
                // FI 20200820 [25468] Dates systemes en UTC
                dp["DTINS"].Value = OTCmlHelper.GetDateSysUTC(pCS);
                dp["IDAINS"].Value = pSession.IdA;
                //
                //dp["SOURCE"].Value = Cst.ProductSource_OTCML;
                dp["SOURCE"].Value = ReflectionTools.ConvertEnumToString<ProductTools.SourceEnum>(ProductTools.SourceEnum.EfsML);
                dp["EXTLLINK"].Value = Convert.DBNull;
                #endregion YieldCurveVal Parameters Set
                #region YieldCurveVal Insert
                int rowAffected = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sql, dp.GetArrayDbParameter());
                #endregion YieldCurveVal Insert
                if (0 <= rowAffected)
                {
                    foreach (YieldCurvePoint point in yieldCurvePoints)
                        point.Insert(pCS, pDbTransaction, idYieldCurveVal_H, pSession);
                }
            }
            return ret;
        }
        #endregion Insert

        #region public PreviousAndNext
        public void PreviousAndNext(YieldCurvePoint pYieldCurvePointBase, out YieldCurvePoint pPrevious, out YieldCurvePoint pNext, bool pIsFutureReaded)
        {
            YieldCurvePoint next;
            YieldCurvePoint previous;
            if (m_IsExistFuture && pIsFutureReaded)
            {
                previous = PreviousFuture(pYieldCurvePointBase);
                next = NextFuture(pYieldCurvePointBase);
            }
            else
            {
                previous = PreviousOtherFuture(pYieldCurvePointBase, true);
                next = NextOtherFuture(pYieldCurvePointBase, true);
            }
            //
            if (m_IsExistFuture && ((null == previous) || (null == next)))
            {
                previous = PreviousOtherFuture(pYieldCurvePointBase, true);
                next = NextOtherFuture(pYieldCurvePointBase, true);
            }
            pPrevious = previous;
            pNext = next;
        }
        #endregion PreviousAndNext
        #region public Next
        /// <summary>
        /// Obtient le prochain point par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <param name="pUseBenchMarkOnly">Si true alors ne considère que les points de BenchMark</param>
        /// <returns></returns>
        public YieldCurvePoint Next(YieldCurvePoint pYieldCurvePointBase)
        {
            YieldCurvePoint ret = null;
            for (int i = yieldCurvePoints.Length - 1; i >= 0; i--)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                    break;
                ret = yieldCurvePoints[i];
            }
            return ret;
        }
        #endregion Next
        #region public NextOtherFuture
        /// <summary>
        /// Obtient le prochain point autre que Future par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <param name="pUseBenchMarkOnly">Si true alors ne considère que les points de BenchMark</param>
        /// <returns></returns>
        public YieldCurvePoint NextOtherFuture(YieldCurvePoint pYieldCurvePointBase, bool pUseBenchMarkOnly)
        {
            YieldCurvePoint ret = null;
            for (int i = yieldCurvePoints.Length - 1; i >= 0; i--)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                {
                    break;
                }
                else if (false == YieldCurveTools.IsFuturePoint(yieldCurvePoints[i]))
                {
                    if (pUseBenchMarkOnly)
                    {
                        if (yieldCurvePoints[i].assetCategorySpecified)
                        {
                            ret = yieldCurvePoints[i];
                        }
                    }
                    else
                    {
                        ret = yieldCurvePoints[i];
                    }
                }
            }
            return ret;
        }
        #endregion NextOtherFuture
        #region public NextFuture
        /// <summary>
        /// Obtient le prochain point Future par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <returns></returns>
        public YieldCurvePoint NextFuture(YieldCurvePoint pYieldCurvePointBase)
        {
            YieldCurvePoint ret = null;
            for (int i = yieldCurvePoints.Length - 1; i >= 0; i--)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                    break;
                else if (YieldCurveTools.IsFuturePoint(yieldCurvePoints[i]))
                    ret = yieldCurvePoints[i];
            }
            return ret;
        }
        #endregion NextFuture
        #region public Previous
        /// <summary>
        /// Obtient le précédent point par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <returns></returns>
        public YieldCurvePoint Previous(YieldCurvePoint pYieldCurvePointBase)
        {
            YieldCurvePoint ret = null;
            for (int i = 0; i < yieldCurvePoints.Length; i++)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                    break;
                ret = yieldCurvePoints[i];
            }
            return ret;
        }
        #endregion Previous
        #region public PreviousOtherFuture
        /// <summary>
        /// Obtient le précédent point autre que Future par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <param name="pUseBenchMarkOnly">Si true alors ne considère que les points de BenchMark</param>
        /// <returns></returns>
        public YieldCurvePoint PreviousOtherFuture(YieldCurvePoint pYieldCurvePointBase, bool pUseBenchMarkOnly)
        {
            YieldCurvePoint ret = null;
            for (int i = 0; i < yieldCurvePoints.Length; i++)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                {
                    break;
                }
                else if (false == YieldCurveTools.IsFuturePoint(yieldCurvePoints[i]))
                {
                    if (pUseBenchMarkOnly)
                    {
                        if (yieldCurvePoints[i].assetCategorySpecified)
                            ret = yieldCurvePoints[i];
                    }
                    else
                    {
                        ret = yieldCurvePoints[i];
                    }
                }
            }
            return ret;
        }
        #endregion PreviousOtherFuture
        #region public PreviousFuture
        /// <summary>
        /// Obtient le précédent point Future par rapport au point {pYieldCurvePointBase}
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pYieldCurvePointBase"></param>
        /// <param name="pUseBenchMarkOnly">Si true alors ne considère que les points de BenchMark</param>
        /// <returns></returns>
        public YieldCurvePoint PreviousFuture(YieldCurvePoint pYieldCurvePointBase)
        {
            YieldCurvePoint ret = null;
            for (int i = 0; i < yieldCurvePoints.Length; i++)
            {
                if (0 == yieldCurvePoints[i].CompareTo(pYieldCurvePointBase))
                    break;
                else if (YieldCurveTools.IsFuturePoint(yieldCurvePoints[i]))
                    ret = yieldCurvePoints[i];
            }
            return ret;
        }
        #endregion PreviousFuture

        #region public SumOfPreviousDiscountFactors
        /// <summary>
        /// Obtient la somme des DiscountFactors * YearFraction des points tels que
        /// <para>- le Nbre de jours calendaires entre DtTerm et DtSpot >= 1an (ie numérateur > denominateur) 
        /// [prise en considération de la base de calcul de la devise]</para>
        /// <para>- si le point n'est pas un asset Future</para>
        /// </summary>
        /// <param name="pYieldCurvePoint"></param>
        /// <returns></returns>
        public double SumOfPreviousDiscountFactors(IProductBase pProductBase, YieldCurvePoint pYieldCurvePoint)
        {
            double df = 0;

            IInterval interval = pProductBase.CreateInterval(PeriodEnum.D, 0);
            foreach (YieldCurvePoint yieldCurvePoint in yieldCurvePoints)
            {
                EFS_DayCountFraction dcf = new EFS_DayCountFraction(yieldCurvePoint.dtSpot, yieldCurvePoint.dtTerm, yieldCurvePoint.DayCountFraction, interval);
                if (dcf.IsNumberOfCalendarDaysSupEqualOneYear && (yieldCurvePoint.dtTerm < pYieldCurvePoint.dtTerm))
                {
                    if ((yieldCurvePoint.assetCategorySpecified && (Cst.UnderlyingAsset_Rate.Future != yieldCurvePoint.assetCategory)) ||
                        (false == yieldCurvePoint.assetCategorySpecified))
                    {
                        df += yieldCurvePoint.discountFactor * yieldCurvePoint.yearFraction;
                    }
                }
            }
            //Ajout du point 6 Mois
            //Lors du calcul du DiscountFactor des points >= 1An e, Le bootstrapping s'appuie le point 6M s'il existe
            if (null != this[BaseRateTypeEnum.SixtMonth])
            {
                YieldCurvePoint point = this[BaseRateTypeEnum.SixtMonth];
                df += point.discountFactor * point.yearFraction;
            }
            return df;
        }
        #endregion SumOfPreviousDiscountFactors

        #region private CreateBenchmarkYieldCurvePoints
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CreateBenchmarkYieldCurvePoints(string pCS, IProductBase pProductBase, YieldCurveDef pYieldCurveDef)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pYieldCurveDef.benchmarkRefsSpecified)
            {
                foreach (BenchmarkRef benchmarkRef in pYieldCurveDef.benchmarkRefs)
                {
                    YieldCurvePoint yieldCurvePoint = new YieldCurvePoint(this)
                    {
                        idYieldCurveVal_H = idYieldCurveVal_H
                    };

                    SQL_AssetForCurve sql_AssetForCurve = benchmarkRef.GetSqlAsset(pCS);
                    if ((null != sql_AssetForCurve) && sql_AssetForCurve.IsLoaded)
                    {
                        IOffset offset_1JO = pProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                        #region GetBusinessCenters and DayCountFraction for Asset currency
                        SQL_Currency currency = new SQL_Currency(pCS, SQL_Currency.IDType.IdC, sql_AssetForCurve.IdC);
                        if (currency.IsLoaded)
                        {
                            yieldCurvePoint.BusinessCenters = pProductBase.CreateBusinessCenters(currency.IdBC, currency.IdBC2, currency.IdBC3);
                            if (StrFunc.IsFilled(sql_AssetForCurve.DcfCurve))
                                yieldCurvePoint.DayCountFraction = sql_AssetForCurve.FpML_Enum_DcfCurve;
                        }
                        #endregion GetBusinessCenters and DayCountFraction for Asset currency

                        #region Point
                        if (StrFunc.IsEmpty(sql_AssetForCurve.PeriodCurve))
                            ret = CurrentYieldCurve.SetLogMessage("[Yield curve construction] Tenor is not specified for asset {0}", sql_AssetForCurve.Identifier);
                        else
                        {
                            yieldCurvePoint.tenorTermSpecified = true;
                            yieldCurvePoint.tenorTerm = pProductBase.CreateInterval(StringToEnum.Period(sql_AssetForCurve.PeriodCurve), sql_AssetForCurve.PeriodMltpCurve);
                        }
                        #endregion Point
                        #region DtSpot
                        switch (benchmarkRef.baseRateType)
                        {
                            case BaseRateTypeEnum.Overnight:
                                yieldCurvePoint.dtSpot = dtBase;
                                break;
                            case BaseRateTypeEnum.Tomnext:
                                yieldCurvePoint.dtSpot = Tools.ApplyOffset(pCS, dtBase, offset_1JO, yieldCurvePoint.BusinessCenters);
                                break;
                            case BaseRateTypeEnum.Future:
                                yieldCurvePoint.dtSpot = Convert.ToDateTime(sql_AssetForCurve.DtTerm);
                                break;
                            case BaseRateTypeEnum.FRA:
                                yieldCurvePoint.dtSpot = Convert.ToDateTime(sql_AssetForCurve.DtTerm);
                                break;
                            default:
                                //FI 20101118 test sur spotOffsetSpecified
                                yieldCurvePoint.dtSpot = dtBase;
                                if (pYieldCurveDef.spotOffsetSpecified)
                                    yieldCurvePoint.dtSpot = Tools.ApplyOffset(pCS, dtBase, pYieldCurveDef.spotOffset, yieldCurvePoint.BusinessCenters);
                                break;
                        }
                        yieldCurvePoint.dtSpotSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtSpot);
                        #endregion DtSpot
                        #region DtTerm
                        switch (benchmarkRef.baseRateType)
                        {
                            case BaseRateTypeEnum.Overnight:
                            case BaseRateTypeEnum.Tomnext:
                            case BaseRateTypeEnum.Spotnext:
                                yieldCurvePoint.dtTerm = Tools.ApplyOffset(pCS, yieldCurvePoint.dtSpot, offset_1JO, yieldCurvePoint.BusinessCenters);
                                break;
                            case BaseRateTypeEnum.Future:
                                Nullable<RollConventionEnum> rcCurve = null;

                                if (StrFunc.IsEmpty(sql_AssetForCurve.RollConventionCurve))
                                    ret = CurrentYieldCurve.SetLogMessage("[Yield curve construction] Roll Convention is not specified for asset {0}", sql_AssetForCurve.Identifier);
                                else
                                    rcCurve = StringToEnum.RollConvention(sql_AssetForCurve.RollConventionCurve);

                                if (yieldCurvePoint.tenorTermSpecified)
                                    yieldCurvePoint.dtTerm = Tools.ApplyInterval(yieldCurvePoint.dtSpot, Convert.ToDateTime(null), yieldCurvePoint.tenorTerm);

                                if (StrFunc.IsEmpty(sql_AssetForCurve.BdcCurve))
                                    ret = CurrentYieldCurve.SetLogMessage("[Yield curve construction] Business day convention is not specified for asset {0}", sql_AssetForCurve.Identifier);
                                else if (rcCurve.HasValue)
                                    yieldCurvePoint.dtTerm = Tools.ApplyAdjustedRollConvention(pCS, yieldCurvePoint.dtTerm, rcCurve.Value,
                                    yieldCurvePoint.BusinessCenters, sql_AssetForCurve.FpML_Enum_BdcCurve, null);
                                break;
                            default:
                                if (StrFunc.IsEmpty(sql_AssetForCurve.BdcCurve))
                                    ret = CurrentYieldCurve.SetLogMessage("[Yield curve construction] Business day convention is not specified for asset {0}", sql_AssetForCurve.Identifier);
                                else
                                    yieldCurvePoint.dtTerm = Tools.ApplyAdjustedInterval(pCS, yieldCurvePoint.dtSpot, yieldCurvePoint.tenorTerm,
                                    yieldCurvePoint.BusinessCenters, sql_AssetForCurve.FpML_Enum_BdcCurve, null);
                                break;
                        }
                        yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                        #endregion DtTerm
                        #region DaysAsset
                        TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                        yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                        yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                        #endregion DaysAsset
                        #region DaysCurvePoint
                        timeSpan = yieldCurvePoint.dtTerm - dtBase;
                        yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                        yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                        #endregion DaysCurvePoint
                        #region IdAsset
                        yieldCurvePoint.idAsset = benchmarkRef.idAsset;
                        yieldCurvePoint.idAssetSpecified = (0 < benchmarkRef.idAsset);
                        #endregion IdAsset
                        #region AssetCategory
                        yieldCurvePoint.assetCategory = benchmarkRef.assetCategory;
                        yieldCurvePoint.assetCategorySpecified = true;
                        #endregion AssetCategory
                        #region Description
                        yieldCurvePoint.description = sql_AssetForCurve.Identifier;
                        yieldCurvePoint.descriptionSpecified = true;
                        #endregion Description
                        #region BaseRateType
                        yieldCurvePoint.baseRateType = benchmarkRef.baseRateType;
                        #endregion BaseRateType
                        #region BaseRateMethod
                        switch (benchmarkRef.baseRateType)
                        {
                            case BaseRateTypeEnum.Future:
                                yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.FRAEquivalent;
                                break;
                            case BaseRateTypeEnum.Bond:
                                if (YieldCurveTypeEnum.InterBankSwapCurve == pYieldCurveDef.curveType)
                                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.MoneyMarketEquivalent;
                                else
                                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.SourceRate;
                                break;
                            default:
                                if (YieldCurveTypeEnum.InterBankSwapCurve == pYieldCurveDef.curveType)
                                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.SourceRate;
                                else
                                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.ActuarialEquivalent;
                                break;
                        }
                        #endregion BaseRateMethod

                        if (Cst.ErrLevel.SUCCESS == ret)
                            Add(yieldCurvePoint);
                    }
                }
                m_IsExistFuture = (null != this[BaseRateTypeEnum.Future]);
            }
            return ret;
        }
        #endregion CreateBenchmarkYieldCurvePoints
        #region private CreateMandatoryYieldCurvePoints
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CreateMandatoryYieldCurvePoints(string pCS, IProductBase pProductBase, YieldCurveDef pYieldCurveDef)
        {
            YieldCurvePoint yieldCurvePoint = new YieldCurvePoint(this);
            IOffset offset;
            #region Overnight
            if ((null == this[BaseRateTypeEnum.Overnight]) && pYieldCurveDef.isExtrapolation)
            {
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.Overnight;
                yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.Extrapolated;
                #endregion BaseRateType and method
                #region DtSpot
                yieldCurvePoint.dtSpot = dtBase;
                //FI 20101117 Test spotOffsetSpecified
                if (pYieldCurveDef.spotOffsetSpecified)
                    yieldCurvePoint.dtSpot = Tools.ApplyOffset(pCS, dtBase, pYieldCurveDef.spotOffset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtSpotSpecified = true;
                #endregion DtSpot
                #region DtTerm
                offset = pProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                yieldCurvePoint.dtTerm = Tools.ApplyOffset(pCS, dtBase, offset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DaysAsset
                yieldCurvePoint.daysAsset = 1;
                yieldCurvePoint.daysAssetSpecified = true;
                #endregion DaysAsset
                #region DaysCurvePoint
                yieldCurvePoint.daysCurvePoint = 1;
                yieldCurvePoint.daysCurvePointSpecified = true;
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion Overnight
            #region Tomnext
            if ((null == this[BaseRateTypeEnum.Tomnext]) && pYieldCurveDef.isExtrapolation)
            {
                offset = pProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.Tomnext;
                if ((null != this[BaseRateTypeEnum.Overnight]) && this[BaseRateTypeEnum.Overnight].assetCategorySpecified)
                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.InterpolatedExtrapolated;
                else
                    yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.Extrapolated;
                #endregion BaseRateType and method
                #region DtSpot
                yieldCurvePoint.dtSpot = Tools.ApplyOffset(pCS, dtBase, offset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtSpotSpecified = true;
                #endregion DtSpot
                #region DtTerm
                yieldCurvePoint.dtTerm = Tools.ApplyOffset(pCS, yieldCurvePoint.dtSpot, offset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DaysAsset
                TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                #endregion DaysAsset
                #region DaysCurvePoint
                timeSpan = yieldCurvePoint.dtTerm - dtBase;
                yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion Tomnext
            #region SpotNext
            if (null == this[BaseRateTypeEnum.Spotnext])
            {
                _ = pProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                yieldCurvePoint = new YieldCurvePoint(this);
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.Spotnext;
                yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.Extrapolated;
                #endregion BaseRateType and method
                #region DtSpot
                yieldCurvePoint.dtSpot = dtBase;
                if (pYieldCurveDef.spotOffsetSpecified)
                    yieldCurvePoint.dtSpot = Tools.ApplyOffset(pCS, dtBase, pYieldCurveDef.spotOffset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtSpotSpecified = true;
                #endregion DtSpot
                #region DtTerm
                offset = pProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                yieldCurvePoint.dtTerm = Tools.ApplyOffset(pCS, yieldCurvePoint.dtSpot, offset, yieldCurvePoint.BusinessCenters);
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DaysAsset
                TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                #endregion DaysAsset
                #region DaysCurvePoint
                timeSpan = yieldCurvePoint.dtTerm - dtBase;
                yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion SpotNext
            #region StubFutures
            if ((null != this[BaseRateTypeEnum.Future]) && (null == this[BaseRateTypeEnum.StubFutures]))
            {
                #region yieldCurvePoints before inserted StubFutures
                Sort();
                #endregion yieldCurvePoints before inserted StubFutures

                yieldCurvePoint = new YieldCurvePoint(this);
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.StubFutures;
                yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.Interpolated;
                #endregion BaseRateType and method
                #region DtTerm
                YieldCurvePoint yieldCurvePointBase = this[BaseRateTypeEnum.Future];
                yieldCurvePoint.dtTerm = yieldCurvePointBase.dtSpot;
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DtSpot
                yieldCurvePointBase = this[BaseRateTypeEnum.Tomnext];
                if (null == yieldCurvePointBase)
                    yieldCurvePointBase = this[BaseRateTypeEnum.Spotnext];
                yieldCurvePoint.dtSpot = yieldCurvePointBase.dtTerm;
                yieldCurvePoint.dtSpotSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtSpot);
                #endregion DtSpot
                #region DaysAsset
                TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                #endregion DaysAsset
                #region DaysCurvePoint
                timeSpan = yieldCurvePoint.dtTerm - dtBase;
                yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion StubFutures
            #region StubFRA
            if ((null != this[BaseRateTypeEnum.FRA]) && (null == this[BaseRateTypeEnum.StubFRAs]))
            {
                yieldCurvePoint = new YieldCurvePoint(this);
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.StubFRAs;
                yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.Interpolated;
                #endregion BaseRateType and method
                #region DtTerm
                YieldCurvePoint yieldCurvePointBase = this[BaseRateTypeEnum.FRA];
                yieldCurvePoint.dtTerm = yieldCurvePointBase.dtTerm;
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DtSpot
                yieldCurvePointBase = this[BaseRateTypeEnum.Tomnext];
                if (null == yieldCurvePointBase)
                    yieldCurvePointBase = this[BaseRateTypeEnum.Spotnext];
                yieldCurvePoint.dtSpot = yieldCurvePointBase.dtTerm;
                yieldCurvePoint.dtSpotSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtSpot);
                #endregion DtSpot
                #region DaysAsset
                TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                #endregion DaysAsset
                #region DaysCurvePoint
                timeSpan = yieldCurvePoint.dtTerm - dtBase;
                yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion StubFRA
            #region OneYear
            if (null == this[BaseRateTypeEnum.OneYear])
            {
                yieldCurvePoint = new YieldCurvePoint(this);
                yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                #region BaseRateType and method
                yieldCurvePoint.baseRateType = BaseRateTypeEnum.OneYear;
                yieldCurvePoint.baseRateMethod = pYieldCurveDef.isExtrapolation ? BaseRateMethodEnum.InterpolatedExtrapolated : BaseRateMethodEnum.Interpolated;
                #endregion BaseRateType and method
                #region DtSpot
                YieldCurvePoint yieldCurvePointBase = this[BaseRateTypeEnum.Tomnext];
                if (null == yieldCurvePointBase)
                    yieldCurvePointBase = this[BaseRateTypeEnum.Spotnext];
                yieldCurvePoint.dtSpot = yieldCurvePointBase.dtTerm;
                yieldCurvePoint.dtSpotSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtSpot);
                #endregion DtSpot
                #region DtTerm
                yieldCurvePoint.dtTerm = Tools.ApplyAdjustedInterval(pCS, yieldCurvePoint.dtSpot,
                    PeriodEnum.Y.ToString(), 1, yieldCurvePoint.BusinessCenters, BusinessDayConventionEnum.FOLLOWING, null);
                yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                #endregion DtTerm
                #region DaysAsset
                TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                #endregion DaysAsset
                #region DaysCurvePoint
                timeSpan = yieldCurvePoint.dtTerm - dtBase;
                yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                #endregion DaysCurvePoint
                Add(yieldCurvePoint);
            }
            #endregion OneYear
        }
        #endregion CreateMandatoryYieldCurvePoints
        #region private CreateUserYieldCurvePoints
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CreateUserYieldCurvePoints(string pCS, YieldCurveDef pYieldCurveDef)
        {
            // Il n'y a pas nécessairement une liste de pas
            // Une courbe peut uniquement être constitué de points de Benchmark 
            if (pYieldCurveDef.curvePtSchedSpecified)
            {
                CurvePtSched curvePtSched = pYieldCurveDef.curvePtSched;
                if (curvePtSched.curvePtSchedDetsSpecified)
                {
                    foreach (CurvePtSchedDet curvePtSchedDet in curvePtSched.curvePtSchedDets)
                    {
                        YieldCurvePoint yieldCurvePoint = new YieldCurvePoint(this);
                        yieldCurvePoint.idYieldCurveVal_H = idYieldCurveVal_H;
                        yieldCurvePoint.BusinessCenters = pYieldCurveDef.BusinessCenters;
                        yieldCurvePoint.DayCountFraction = pYieldCurveDef.DayCountFraction;

                        #region Point
                        yieldCurvePoint.tenorTermSpecified = true;
                        yieldCurvePoint.tenorTerm = curvePtSchedDet.tenor;
                        #endregion Point
                        #region BaseRateType and method
                        yieldCurvePoint.baseRateType = BaseRateTypeEnum.Interpolated;
                        yieldCurvePoint.baseRateMethod = BaseRateMethodEnum.InterpolatedExtrapolated;
                        #endregion BaseRateType and method
                        #region DtSpot
                        YieldCurvePoint yieldCurvePointBase = this[BaseRateTypeEnum.Tomnext];
                        if (null == yieldCurvePointBase)
                            yieldCurvePointBase = this[BaseRateTypeEnum.Spotnext];
                        yieldCurvePoint.dtSpot = yieldCurvePointBase.dtTerm;
                        yieldCurvePoint.dtSpotSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtSpot);
                        #endregion DtSpot
                        #region DtTerm
                        yieldCurvePoint.dtTerm = Tools.ApplyAdjustedInterval(pCS, yieldCurvePoint.dtSpot,
                            curvePtSchedDet.tenor, yieldCurvePoint.BusinessCenters, BusinessDayConventionEnum.FOLLOWING, null);
                        yieldCurvePoint.dtTermSpecified = DtFunc.IsDateTimeFilled(yieldCurvePoint.dtTerm);
                        #endregion DtTerm
                        #region DaysAsset
                        TimeSpan timeSpan = yieldCurvePoint.dtTerm - yieldCurvePoint.dtSpot;
                        yieldCurvePoint.daysAsset = Convert.ToDouble(timeSpan.Days);
                        yieldCurvePoint.daysAssetSpecified = (0 < yieldCurvePoint.daysAsset);
                        #endregion DaysAsset
                        #region DaysCurvePoint
                        timeSpan = yieldCurvePoint.dtTerm - dtBase;
                        yieldCurvePoint.daysCurvePoint = Convert.ToDouble(timeSpan.Days);
                        yieldCurvePoint.daysCurvePointSpecified = (0 < yieldCurvePoint.daysCurvePoint);
                        #endregion DaysCurvePoint
                        //
                        //20100722 FI Ajout du point uniquement s'il nexiste pas de un point à cette date
                        //Par exemple, Il peut y avoir un point de benchmark à cette date
                        //La clef unique de la table YIELDCURVEPOINT_H est constitutués des champs (IDYIELDCURVEVAL_H, DTTERM)
                        if (null == this[yieldCurvePoint.daysCurvePoint])
                            Add(yieldCurvePoint);
                    }
                }
            }
        }
        #endregion CreateUserYieldCurvePoints

        #region private SearchIndex_Future
        /// <summary>
        /// <para>Définit l'index du 1er point Future (c'est le point BaseRateType = StubFutures)</para>
        /// <para>Définit l'index du denier point Future (c'est un point BaseRateType = Future)</para>
        /// </summary>
        private void SearchIndex_Future()
        {
            if (yieldCurvePointsSpecified)
            {
                for (int i = 0; i < yieldCurvePoints.Length; i++)
                {
                    if (yieldCurvePoints[i].baseRateType == BaseRateTypeEnum.StubFutures)
                        Index_FirstFuture = i;
                    else if (yieldCurvePoints[i].baseRateType == BaseRateTypeEnum.Future)
                        Index_LastFuture = i;
                }
            }
        }
        #endregion Index_Future
        #region private Sort
        /// <summary>
        ///  Tri les points de la courbe
        /// <para>La définition du tri est exprimée par la class CompareYieldCurvePoint</para>
        /// </summary>
        /// <returns></returns>
        private void Sort()
        {
            Array.Sort(yieldCurvePoints, new CompareYieldCurvePoint());
        }
        #endregion Sort

        #region private LoadYieldCurvePoint
        /// <summary>
        /// Charge les différents points de la courbe de taux
        /// <para>Exploite la table YIELDCURVEPOINT_H</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        private void LoadYieldCurvePoint(string pCS, IProductBase pProductBase)
        {
            QueryParameters qryParameters = GetSelectYieldCurvePoint(pCS);
            qryParameters.Parameters["IDYIELDCURVEVAL_H"].Value = idYieldCurveVal_H;
            DataSet dsYieldCurvePoint = DataHelper.ExecuteDataset(qryParameters.Cs , CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            #region DataSet Initialize
            dsYieldCurvePoint.DataSetName = "YieldCurvePoint";
            DataTable dtYieldCurvePoint = dsYieldCurvePoint.Tables[0];
            dtYieldCurvePoint.TableName = "YieldCurvePoint";
            #endregion DataSet Initialize

            #region YieldCurvePoint filling
            yieldCurvePointsSpecified = (null != dsYieldCurvePoint) && (0 < ArrFunc.Count(dtYieldCurvePoint.Rows));

            if (yieldCurvePointsSpecified)
            {
                ArrayList aYieldCurvePoints = new ArrayList();
                foreach (DataRow rowYieldCurvePoint in dtYieldCurvePoint.Rows)
                {
                    YieldCurvePoint yieldCurvePoint = new YieldCurvePoint(this)
                    {
                        idYieldCurveVal_H = idYieldCurveVal_H,

                        assetCategorySpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["ASSETCATEGORY"])),
                        idAssetSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["IDASSET"])),
                        descriptionSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["DESCRIPTION"])),
                        tenorTermSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["PERIODMLTPTERM"])) &&
                        (false == Convert.IsDBNull(rowYieldCurvePoint["PERIODTERM"])),
                        dtTermSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["DTTERM"])),
                        //BASERATETYPE
                        baseRateType = (BaseRateTypeEnum)System.Enum.Parse(typeof(BaseRateTypeEnum),
                        rowYieldCurvePoint["BASERATETYPE"].ToString(), true),
                        //BASERATEMETHOD
                        baseRateMethod = (BaseRateMethodEnum)System.Enum.Parse(typeof(BaseRateMethodEnum),
                        rowYieldCurvePoint["BASERATEMETHOD"].ToString(), true),

                        daysCurvePointSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["DAYSCURVEPOINT"])),
                        daysAssetSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["DAYSASSET"])),

                        //INPUTRATE
                        inputRate = Convert.ToDouble(rowYieldCurvePoint["INPUTRATE"]),

                        baseRateSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["BASERATE"])),
                        zeroCouponYieldSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["ZEROCOUPONYIELD"])),
                        discountFactorSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["DISCOUNTFACTOR"])),

                        forwardYieldSpecified = (false == Convert.IsDBNull(rowYieldCurvePoint["FORWARDYIELD"]))
                    };

                    //ASSETCATEGORY
                    if (yieldCurvePoint.assetCategorySpecified)
                        yieldCurvePoint.assetCategory = (Cst.UnderlyingAsset_Rate)System.Enum.Parse(typeof(Cst.UnderlyingAsset_Rate), rowYieldCurvePoint["ASSETCATEGORY"].ToString(), true);

                    //IDASSET
                    if (yieldCurvePoint.idAssetSpecified)
                        yieldCurvePoint.idAsset = Convert.ToInt32(rowYieldCurvePoint["IDASSET"]);

                    //DESCRIPTION
                    if (yieldCurvePoint.descriptionSpecified)
                        yieldCurvePoint.description = rowYieldCurvePoint["DESCRIPTION"].ToString();

                    //TENORTERM
                    if (yieldCurvePoint.tenorTermSpecified)
                    {
                        yieldCurvePoint.tenorTerm = pProductBase.CreateInterval(
                            StringToEnum.Period(rowYieldCurvePoint["PERIODTERM"].ToString()),
                            Convert.ToInt32(rowYieldCurvePoint["PERIODMLTPTERM"]));
                    }

                    //DTTERM
                    if (yieldCurvePoint.dtTermSpecified)
                        yieldCurvePoint.dtTerm = Convert.ToDateTime(rowYieldCurvePoint["DTTERM"]);

                    if (yieldCurvePoint.daysCurvePointSpecified)
                        yieldCurvePoint.daysCurvePoint = Convert.ToDouble(rowYieldCurvePoint["DAYSCURVEPOINT"]);

                    //DAYSASSET
                    if (yieldCurvePoint.daysAssetSpecified)
                        yieldCurvePoint.daysAsset = Convert.ToDouble(rowYieldCurvePoint["DAYSASSET"]);

                    //BASERATE
                    if (yieldCurvePoint.baseRateSpecified)
                        yieldCurvePoint.baseRate = Convert.ToDouble(rowYieldCurvePoint["BASERATE"]);

                    //ZEROCOUPONYIELD
                    if (yieldCurvePoint.zeroCouponYieldSpecified)
                        yieldCurvePoint.zeroCouponYield = Convert.ToDouble(rowYieldCurvePoint["ZEROCOUPONYIELD"]);

                    //DISCOUNTFACTOR
                    if (yieldCurvePoint.discountFactorSpecified)
                        yieldCurvePoint.discountFactor = Convert.ToDouble(rowYieldCurvePoint["DISCOUNTFACTOR"]);

                    //FORWARDYIELD
                    if (yieldCurvePoint.forwardYieldSpecified)
                        yieldCurvePoint.forwardYield = Convert.ToDouble(rowYieldCurvePoint["FORWARDYIELD"]);

                    aYieldCurvePoints.Add(yieldCurvePoint);
                }
                yieldCurvePoints = (YieldCurvePoint[])aYieldCurvePoints.ToArray(typeof(YieldCurvePoint));
            }
            #endregion YieldCurvePoint filling
        }
        #endregion SelectYieldCurvePoint

        #region private static GetSelectYieldCurvePoint
        /// <summary>
        /// Obtient l'instruction select qui charge les points de la courbe [Table YIELDCURVEPOINT_H]
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private static QueryParameters GetSelectYieldCurvePoint(string pCS)
        {
            StrBuilder query = new StrBuilder(SQLCst.SELECT);
            query += @"yc.IDYIELDCURVEVAL_H," + Cst.CrLf;
            query += @"yc.ASSETCATEGORY, yc.IDASSET, yc.DESCRIPTION, " + Cst.CrLf;
            query += @"yc.PERIODMLTPTERM, yc.PERIODTERM, yc.DTTERM, " + Cst.CrLf;
            query += @"yc.BASERATETYPE, yc.BASERATEMETHOD, yc.DAYSCURVEPOINT, yc.DAYSASSET, " + Cst.CrLf;
            query += @"yc.INPUTRATE, yc.BASERATE, yc.ZEROCOUPONYIELD, yc.DISCOUNTFACTOR, yc.FORWARDYIELD, " + Cst.CrLf;
            query += @"yc.DTINS, yc.IDAINS, yc.DTUPD, yc.IDAUPD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.YIELDCURVEPOINT_H + " yc " + Cst.CrLf;
            query += SQLCst.WHERE + @"(yc.IDYIELDCURVEVAL_H=@IDYIELDCURVEVAL_H)" + Cst.CrLf;
            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEVAL_H", DbType.Int32));
            //
            QueryParameters ret = new QueryParameters(pCS, query.ToString(), dp);
            //
            return ret;
        }
        #endregion GetSelectYieldCurvePoint

        #endregion Methods
    }
    #endregion YieldCurveVal
    #region YieldCurvePoint
    /// <summary>
    /// Représente un point de la courbe de taux
    /// </summary>
    public class YieldCurvePoint : IComparable
    {
        #region Members
        /// <summary>
        /// Business centers associés au point
        /// </summary>
        private IBusinessCenters _businessCenters;
        /// <summary>
        /// DCF associé au point
        /// </summary>
        private DayCountFractionEnum _dayCountFraction;
        //
        public int idYieldCurveVal_H;
        //
        public bool assetCategorySpecified;
        public Cst.UnderlyingAsset_Rate assetCategory;
        //
        public bool idAssetSpecified;
        public int idAsset;
        //
        public bool descriptionSpecified;
        public string description;
        //
        public bool tenorTermSpecified;
        public IInterval tenorTerm;
        //
        public bool dtSpotSpecified;
        /// <summary>
        /// Représente la date Spot du point
        /// <para>Si point Overnight, représente la date de la courbe</para>
        /// <para>Si point Tomnext, représente la date de la courbe + 1JO</para>
        /// <para>Si point Spotnext, représente la date de la courbe + offset associé au Spotnext</para>
        /// <para>Si le point est un point de Benchmark Future, représente l'échéance du future</para>
        /// <para>Si le point est un point de Benchmark Fra, représente l'échéance du fra</para>
        /// <para>etc.. </para>
        /// </summary>
        public DateTime dtSpot;
        //
        public bool dtTermSpecified;
        /// <summary>
        /// Représente la date de maturité du point (jour ouvré)  
        /// <para>Prise en considération des business centers associés au point</para>
        /// </summary>
        public DateTime dtTerm;
        //
        public BaseRateTypeEnum baseRateType;
        public BaseRateMethodEnum baseRateMethod;
        //
        public bool daysCurvePointSpecified;
        /// <summary>
        /// Représente le nbr de jour entre date maturité du point et la date de la courbe
        /// </summary>
        public double daysCurvePoint;
        
        public bool daysAssetSpecified;
        /// <summary>
        /// Représente le nbr de jour entre date maturité du point et la date spot de la courbe
        /// </summary>
        public double daysAsset;
        //
        public double inputRate;
        //
        public bool baseRateSpecified;
        public double baseRate;
        //
        public bool impliedRateSpecified;
        public double impliedRate;
        //
        public bool zeroCouponYieldSpecified;
        public double zeroCouponYield;
        //
        public bool discountFactorInitialSpecified;
        public double discountFactorInitial;
        //
        public bool discountFactorSpecified;
        public double discountFactor;
        //
        public bool forwardYieldSpecified;
        public double forwardYield;
        //
        public bool yearFractionSpecified;
        public double yearFraction;
        #endregion Members
        //
        #region Accessors
        #region businessCenters
        /// <summary>
        /// Obtient ou définit une liste de businessCenters
        /// </summary>
        public IBusinessCenters BusinessCenters
        {
            set { _businessCenters = value; }
            get { return _businessCenters; }
        }
        #endregion businessCenters
        #region dayCountFraction
        /// <summary>
        /// Obtient ou définit un DCF
        /// </summary>
        public DayCountFractionEnum DayCountFraction
        {
            set { _dayCountFraction = value; }
            get { return _dayCountFraction; }
        }
        #endregion dayCountFraction

        public YieldCurve CurrentYieldCurve
        {
            get;
            set;
        }

        #endregion Accessors
        //
        #region Constructors
        // EG 20150313
        public YieldCurvePoint(YieldCurveVal pYieldCurveVal) 
        {
            CurrentYieldCurve = pYieldCurveVal.CurrentYieldCurve;
        }
        #endregion Constructors
        //
        #region Methods
        #region BaseRateCalculation
        public void BaseRateCalculation(IProductBase pProductBase, YieldCurveVal pYieldCurveVal)
        {
            baseRate = 0;
            IInterval interval = pProductBase.CreateInterval(PeriodEnum.D, 0);
            EFS_EquivalentRate equiRate;
            switch (baseRateMethod)
            {
                case BaseRateMethodEnum.SourceRate:
                    #region SourceRate
                    baseRate = inputRate;
                    break;
                #endregion SourceRate
                case BaseRateMethodEnum.ActuarialEquivalent:
                    #region ActuarialEquivalent
                    // 20071106 EG Ticket 15859
                    equiRate = new EFS_EquivalentRate(EquiRateMethodEnum.CompoundToSimple, dtSpot, dtTerm, Convert.ToDecimal(inputRate),
                        DayCountFractionEnum.ACT360.ToString(), interval, DayCountFractionEnum.ACTACTISDA.ToString(), interval);
                    baseRate = Convert.ToDouble(equiRate.simpleRate);
                    break;
                #endregion ActuarialEquivalent
                case BaseRateMethodEnum.MoneyMarketEquivalent:
                    #region MoneyMarketEquivalent
                    // 20071106 EG Ticket 15859
                    equiRate = new EFS_EquivalentRate(EquiRateMethodEnum.SimpleToCompound, dtSpot, dtTerm, Convert.ToDecimal(inputRate),
                        DayCountFractionEnum.ACTACTISDA.ToString(), interval, DayCountFractionEnum.ACT360.ToString(), interval);
                    baseRate = Convert.ToDouble(equiRate.compoundRate);
                    break;
                #endregion MoneyMarketEquivalent
                case BaseRateMethodEnum.FRAEquivalent:
                    #region FRAEquivalent
                    baseRate = (100 - inputRate) / 100;
                    break;
                #endregion FRAEquivalent
                case BaseRateMethodEnum.Interpolated:
                case BaseRateMethodEnum.InterpolatedExtrapolated:
                case BaseRateMethodEnum.Extrapolated:
                    #region Interpolated / InterpolatedExtrapolated / Extrapolated
                    InterpolationTypeEnum interpolationType = (InterpolationTypeEnum)System.Enum.Parse(typeof(InterpolationTypeEnum), baseRateMethod.ToString(), true);
                    baseRate = InterpolationFormula(interpolationType, InterpolationMethodEnum.Linear, pYieldCurveVal, "baseRate", true);
                    break;
                #endregion Interpolated / InterpolatedExtrapolated / Extrapolated
                default:
                    #region Others
                    baseRate = inputRate;
                    break;
                    #endregion Others
            }
            baseRate = System.Math.Round(baseRate, 12, MidpointRounding.AwayFromZero);
            baseRateSpecified = (baseRate != 0);
        }
        #endregion BaseRateCalculation
        //
        #region BasicCalcul
        public Cst.ErrLevel BasicCalcul(string pCS, IProductBase pProductBase, YieldCurveVal pYieldCurveVal, YieldCurveDef pYieldCurveDef, bool pIsBaseRateCalculated)
        {
            if (pIsBaseRateCalculated)
                BaseRateCalculation(pProductBase, pYieldCurveVal);

            ImpliedRateSetting();

            Cst.ErrLevel ret = DiscountFactorInitialCalculation(pCS, pYieldCurveVal);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                DiscountFactorCalculation(pCS, pProductBase, pYieldCurveVal, pYieldCurveDef);
                ZeroCouponYieldCalculation(pYieldCurveDef.compoundFrequency);
            }
            return ret;
        }
        #endregion BasicCalcul
        //
        #region YearFractionCalculation
        /// <summary>
        /// Calcul du yearFraction
        /// <para>yearFraction n'est calculé que sur les points >=1 ou le point 6M</para>
        /// </summary>
        /// <param name="pYieldCurveVal"></param>
        public Cst.ErrLevel YearFractionCalculation(string pCS, IProductBase pProductBase, YieldCurveVal pYieldCurveVal)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            yearFractionSpecified = false;

            bool isPointAfterOneYear = (dtTerm.CompareTo(pYieldCurveVal[BaseRateTypeEnum.OneYear].dtTerm) > 0);
            bool isPoint6Month = (this.baseRateType == BaseRateTypeEnum.SixtMonth);
            bool isPoint1Year = (this.baseRateType == BaseRateTypeEnum.OneYear);
            bool isFuture = YieldCurveTools.IsFuturePoint(this);

            //On considère que les points >=1 ou 6 mois s'il existe différents des points Futures 
            //On delà de 1 an les points sont normalement des points swap ou éventuellement des points interpolés s'il n'est pas un point du Benchmark
            if (isPoint6Month || isPoint1Year || (isPointAfterOneYear & (false == isFuture)))
            {
                DayCountFractionEnum dcfYearFraction = DayCountFraction;
                DateTime dtStart = DateTime.MinValue;
                if (isPoint6Month || (isPoint1Year && (null == pYieldCurveVal[BaseRateTypeEnum.SixtMonth])))
                {
                    dtStart = dtSpot;
                }
                else
                {
                    YieldCurvePoint previousPoint = pYieldCurveVal.PreviousOtherFuture(this, false);
                    if (null == previousPoint)
                        ret = CurrentYieldCurve.SetLogMessage("No previous point found from point:{0}", GetPointInformation(pCS));
                    else
                        dtStart = previousPoint.dtTerm;
                }

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    DateTime dtEnd = dtTerm;
                    IInterval interval = pProductBase.CreateInterval(PeriodEnum.D, 0);
                    EFS_DayCountFraction dcf = new EFS_DayCountFraction(dtStart, dtEnd, dcfYearFraction, interval);
                    yearFraction = Convert.ToDouble(dcf.FactorFromTotalNumberOfCalculatedDays);
                    //GLOP FI A REVOIR
                    if (isPoint1Year)
                        yearFraction = Convert.ToDouble(1.00);
                    yearFractionSpecified = true;
                }
            }
            return ret;
        }
        #endregion
        //
        #region CompareTo
        /// <summary>
        /// Obtient 0 si les DtTerm des 2 points sont identiques, -1 sinon
        /// </summary>
        /// <exception cref="ArgumentException si obj n'est pas de type YieldCurvePoint"></exception>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (false == obj.GetType().Equals(typeof(YieldCurvePoint)))
                throw new ArgumentException("YieldCurvePoint.CompareTo", "Error");
            //
            int ret = -1;
            if (dtTerm == ((YieldCurvePoint)obj).dtTerm)
                ret = 0;
            return ret;
        }
        #endregion CompareTo
        //
        #region DiscountFactorCalculation
        public Cst.ErrLevel DiscountFactorCalculation(string pCS, IProductBase pProductBase, YieldCurveVal pYieldCurveVal, YieldCurveDef pYieldCurveDef)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            double cst360 = Convert.ToDouble(360);

            
            int compoundingFrequency = CompoundingFrequencyPeriod.GetPeriod(pYieldCurveDef.compoundFrequency);
            if (CompoundingFrequencyEnum.Continuous == pYieldCurveDef.compoundFrequency)
            {
                double x = daysAsset / cst360;
                double den = Math.Exp(baseRate * x);
                discountFactor = discountFactorInitial / den;
            }
            else
            {
                bool isPointAfterOneYear = (dtTerm.CompareTo(pYieldCurveVal[BaseRateTypeEnum.OneYear].dtTerm) > 0);
                if (isPointAfterOneYear)
                {
                    double x = (baseRate / compoundingFrequency);
                    double prevDf = pYieldCurveVal.SumOfPreviousDiscountFactors(pProductBase, this);
                    discountFactor = (discountFactorInitial - (x * prevDf)) / (1 + x * yearFraction);
                }
                else
                {
                    IInterval interval = pProductBase.CreateInterval(PeriodEnum.D, 0);
                    EFS_DayCountFraction dcf = new EFS_DayCountFraction(dtSpot, dtTerm, DayCountFraction, interval);
                    //FI 20101130 (validation EPL)
                    //Il faut prendre la base de calcul associée au point, notamment pour le point 1 an (qui peut être soit un point swap, soit un point RateIndex)) 
                    if (0 == dcf.Denominator)
                    {
                        ret = CurrentYieldCurve.SetLogMessage("Error when calculating denominator, point is {0} and dayCountFraction is {1}",
                            GetPointInformation(pCS), DayCountFraction.ToString());
                    }
                    else
                    {
                        double x = daysAsset / Convert.ToDouble(dcf.Denominator);
                        discountFactor = discountFactorInitial / (1 + (baseRate * x));
                    }
                }

            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                discountFactor = Math.Round(discountFactor, 12, MidpointRounding.AwayFromZero);
                discountFactorSpecified = (discountFactor != 0);
            }
            return ret;
        }
        #endregion DiscountFactorCalculation
        #region DiscountFactorInterpolationCalculation
        public void DiscountFactorInterpolationCalculation(YieldCurveVal pYieldCurveVal)
        {
            discountFactor = InterpolationFormula(InterpolationTypeEnum.InterpolatedExtrapolated, InterpolationMethodEnum.LinearExponential, pYieldCurveVal, "discountFactor", true);
            discountFactor = System.Math.Round(discountFactor, 12, MidpointRounding.AwayFromZero);
            discountFactorSpecified = (discountFactor != 0);
        }
        #endregion DiscountFactorInterpolationCalculation
        #region DiscountFactorInitialCalculation
        public Cst.ErrLevel DiscountFactorInitialCalculation(string pCS, YieldCurveVal pYieldCurveVal)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string msgErr = StrFunc.AppendFormat(@"unable to calculate 'Discount Factor Initial' on point:{0}", GetPointInformation(pCS));

            YieldCurvePoint yieldCurvePointBase;
            switch (baseRateType)
            {
                case BaseRateTypeEnum.Overnight:
                    discountFactorInitial = 1;
                    break;
                case BaseRateTypeEnum.Tomnext:
                    yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.Overnight];
                    if (yieldCurvePointBase == null) 
                        ret = CurrentYieldCurve.SetLogMessage(msgErr);
                    else
                        discountFactorInitial = yieldCurvePointBase.discountFactor;
                    break;
                case BaseRateTypeEnum.Future:
                    yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.Future, dtSpot];
                    if (null == yieldCurvePointBase)
                        yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.StubFutures];

                    if (null == yieldCurvePointBase)
                        ret = CurrentYieldCurve.SetLogMessage(msgErr);
                    else
                        discountFactorInitial = yieldCurvePointBase.discountFactor;
                    break;
                case BaseRateTypeEnum.FRA:
                    yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.FRA, dtSpot];
                    if (null == yieldCurvePointBase)
                        yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.StubFRAs];

                    if (null == yieldCurvePointBase)
                        ret = CurrentYieldCurve.SetLogMessage(msgErr);
                    else
                        discountFactorInitial = yieldCurvePointBase.discountFactor;
                    break;
                //
                //FI 20101118 cas particulier spot 's'il n'y a pas le point Tomnext, discountFactorInitial est 1
                case BaseRateTypeEnum.Spotnext:
                    discountFactorInitial = 1;
                    yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.Tomnext];
                    if (null != yieldCurvePointBase)
                        discountFactorInitial = yieldCurvePointBase.discountFactor;
                    break;

                default:
                    yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.Tomnext];
                    if (null == yieldCurvePointBase)
                        yieldCurvePointBase = pYieldCurveVal[BaseRateTypeEnum.Spotnext];

                    if (null == yieldCurvePointBase)
                        ret = CurrentYieldCurve.SetLogMessage(msgErr);
                    else
                        discountFactorInitial = yieldCurvePointBase.discountFactor;
                    break;
            }
            discountFactorInitialSpecified = (discountFactorInitial != 0);
            return ret;
        }
        #endregion DiscountFactorInitialCalculation
        #region GetValue
        /// <summary>
        /// Obtient le valeur du membre {pName} 
        /// </summary>
        /// <param name="pName">Nom du Membre</param>
        /// <returns></returns>
        public double GetValue(string pName)
        {
            double value;
            FieldInfo fldCurvePoint = this.GetType().GetField(pName);
            if (null != fldCurvePoint)
                value = (double)fldCurvePoint.GetValue(this);
            else
            {
                CurrentYieldCurve.SetLogMessage("GetValue {0} not found into yieldCurvePoint", pName);
                value = double.NaN;
            }
            return value;
        }
        #endregion GetValue
        #region GetPointInformation
        /// <summary>
        /// Retourne un libelle pour le point
        /// <para>Utile pour générer des messages d'informations</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public string GetPointInformation(string pCS)
        {
            //FI 20101130 GetPointInformation est enrichi de baseRate
            string ret = string.Empty;
            if (descriptionSpecified)
            {
                ret = description;
            }
            else if (this.idAssetSpecified)
            {
                SQL_AssetForCurve sqlAsset = YieldCurveTools.GetSqlAsset(pCS, assetCategory, idAsset);
                if (sqlAsset.LoadTable())
                    ret = sqlAsset.Identifier;
                else
                    ret = "unknown asset";
            }
            // 
            if (baseRateSpecified)
                ret = ret + Cst.Space + "[" + baseRateType.ToString() + "]";
            //
            if (tenorTermSpecified)
                ret = ret + Cst.Space + "[" + tenorTerm.PeriodMultiplier.IntValue.ToString() + tenorTerm.Period.ToString() + "]";
            //
            return ret;
        }
        #endregion

        #region ImpliedRateCalculation
        public void ImpliedRateCalculation()
        {
            impliedRate = ((discountFactorInitial / discountFactor) - 1) * Convert.ToDouble(360) / daysAsset;
            impliedRate = System.Math.Round(impliedRate, 12, MidpointRounding.AwayFromZero);
            impliedRateSpecified = (impliedRate != 0);
            if (false == baseRateSpecified)
            {
                baseRate = impliedRate;
                baseRateSpecified = impliedRateSpecified;
            }
        }
        #endregion ImpliedRateCalculation
        #region ImpliedRateInterpolationCalculation
        public void ImpliedRateInterpolationCalculation(YieldCurveVal pYieldCurveVal)
        {
            impliedRate = InterpolationFormula(InterpolationTypeEnum.InterpolatedExtrapolated,
                InterpolationMethodEnum.Linear, pYieldCurveVal, "impliedRate", false);
            impliedRate = System.Math.Round(impliedRate, 12, MidpointRounding.AwayFromZero);
            impliedRateSpecified = (impliedRate != 0);
        }
        #endregion ImpliedRateInterpolationCalculation
        #region ImpliedRateSetting
        public void ImpliedRateSetting()
        {
            if (baseRateSpecified)
            {
                impliedRate = baseRate;
                impliedRateSpecified = baseRateSpecified;
            }
        }
        #endregion ImpliedRateSetting
        //
        #region InputRateCalculation
        /// <summary>
        /// Recherche de la cotation de l'asset associé au point et alimente inputRate avec le résultat 
        /// </summary>
        /// <exception cref="SpheresException2 lorsque une cotation est non trouvée [QUOTENOTFOUND]"></exception>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pTime"></param>
        /// <param name="pIdMarketEnv"></param>
        /// <param name="pIdValScenario"></param>
        /// <param name="pQuoteSide"></param>
        /// EG 20150312 (POC - BERKELEY] New
        // EG 20190716 [VCL : New FixedIncome] Upd ReadQuote (KeyQuoteAdditional)
        public Cst.ErrLevel InputRateCalculation(ProcessCacheContainer pProcessContainer, DateTime pTime, string pIdMarketEnv, string pIdValScenario, Nullable<QuotationSideEnum> pQuoteSide)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            inputRate = 0;
            #region KeyQuote
            KeyQuote keyQuote = new KeyQuote(pProcessContainer.CS, pTime, pIdMarketEnv, pIdValScenario, pQuoteSide, QuoteTimingEnum.Close);

            Cst.UnderlyingAsset underlyingAsset;

            #endregion KeyQuote
            switch (assetCategory)
            {
                case Cst.UnderlyingAsset_Rate.Bond:
                    underlyingAsset = Cst.UnderlyingAsset.Bond;
                    break;
                case Cst.UnderlyingAsset_Rate.Deposit:
                    underlyingAsset = Cst.UnderlyingAsset.Deposit;
                    break;
                case Cst.UnderlyingAsset_Rate.Future:
                    underlyingAsset = Cst.UnderlyingAsset.Future;
                    break;
                case Cst.UnderlyingAsset_Rate.RateIndex:
                    underlyingAsset = Cst.UnderlyingAsset.RateIndex;
                    break;
                case Cst.UnderlyingAsset_Rate.SimpleFra:
                    underlyingAsset = Cst.UnderlyingAsset.SimpleFra;
                    break;
                case Cst.UnderlyingAsset_Rate.SimpleIRSwap:
                    underlyingAsset = Cst.UnderlyingAsset.SimpleIRSwap;
                    break;
                default:
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("AssetCategory [{0}] not treated", assetCategory.ToString()));
            }

            SystemMSGInfo systemMsgInfo = null;
            Quote quote = pProcessContainer.ReadQuote(underlyingAsset, idAsset, "xxx", pTime, pQuoteSide.Value, ref systemMsgInfo);
            if ((null != systemMsgInfo))
            {
                ret = Cst.ErrLevel.FAILURE;

                // FI 2020623 [XXXXX] SetErrorWarning
                pProcessContainer.SetErrorWarning(systemMsgInfo.processState.Status);
                
                
                Logger.Log(systemMsgInfo.ToLoggerData(2));
            }
            if (null != quote && quote.valueSpecified)
                inputRate = Convert.ToDouble(quote.value);
            return ret;
        }
        #endregion InputRateCalculation

        #region InputRateCalculation
        /// <summary>
        /// Recherche de la cotation de l'asset associé au point et alimente inputRate avec le résultat 
        /// </summary>
        /// <exception cref="SpheresException2 lorsque une cotation est non trouvée [QUOTENOTFOUND]"></exception>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pTime"></param>
        /// <param name="pIdMarketEnv"></param>
        /// <param name="pIdValScenario"></param>
        /// <param name="pQuoteSide"></param>
        public void InputRateCalculation(string pCS, IProductBase pProductBase, DateTime pTime, string pIdMarketEnv, string pIdValScenario, Nullable<QuotationSideEnum> pQuoteSide)
        {
            inputRate = 0;
            KeyQuote keyQuote = new KeyQuote(pCS, pTime, pIdMarketEnv, pIdValScenario, pQuoteSide, QuoteTimingEnum.Close);

            QuoteEnum quoteType;
            switch (assetCategory)
            {
                case Cst.UnderlyingAsset_Rate.Bond:
                    //PL 20111024 quoteType = QuoteEnum.BOND;
                    quoteType = QuoteEnum.DEBTSECURITY;
                    break;
                case Cst.UnderlyingAsset_Rate.Deposit:
                    quoteType = QuoteEnum.DEPOSIT;
                    break;
                // RD 20100428 Remplacement de FUTURE par ETD
                // dans le cadre de la suppression des tables ASSET_FUTURE et QUOTE_FUTURE_H
                case Cst.UnderlyingAsset_Rate.Future:
                    quoteType = QuoteEnum.ETD;
                    break;
                case Cst.UnderlyingAsset_Rate.RateIndex:
                    quoteType = QuoteEnum.RATEINDEX;
                    break;
                case Cst.UnderlyingAsset_Rate.SimpleFra:
                    quoteType = QuoteEnum.SIMPLEFRA;
                    break;
                case Cst.UnderlyingAsset_Rate.SimpleIRSwap:
                    quoteType = QuoteEnum.SIMPLEIRS;
                    break;
                default:
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("AssetCategory [{0}] not treated", assetCategory.ToString()));
            }

            SQL_Quote quote = new SQL_Quote(pCS, quoteType, AvailabilityEnum.Enabled, pProductBase, keyQuote, idAsset);
            if (quote.IsLoaded && quote.IsEnabled)
            {
                inputRate = Convert.ToDouble(quote.QuoteValue);
            }
            else
            {
                string errMsg = "Rate not Found !";
                if (quote.IsLoaded && (false == quote.IsEnabled))
                    errMsg = "Rate not Enabled !";

                errMsg += StrFunc.AppendFormat("Time[{0}] Asset[{1}] Market Env.[{2}] Val. Scen.[{3}] Type[{4}] Timing[{5}]",
                    DtFunc.DateTimeToStringISO(keyQuote.Time),
                    description,
                    keyQuote.IdMarketEnv,
                    keyQuote.IdValScenario,
                    keyQuote.QuoteSide,
                    keyQuote.QuoteTiming);

                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errMsg, 
                    new ProcessState(ProcessStateTools.StatusErrorEnum,ProcessStateTools.CodeReturnQuoteNotFoundEnum));
            }
        }
        #endregion InputRateCalculation

        #region Insert
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public void Insert(string pCS, IDbTransaction pDbTransaction, int pIdYieldCurveVal_H, AppSession pSession)
        {
            idYieldCurveVal_H = pIdYieldCurveVal_H;
            string sql = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.YIELDCURVEPOINT_H.ToString() + @" (
						IDYIELDCURVEVAL_H, ASSETCATEGORY, IDASSET, DESCRIPTION,
                        DTSPOT, PERIODMLTPTERM, PERIODTERM, DTTERM, 
                        BASERATETYPE, BASERATEMETHOD, DAYSCURVEPOINT, DAYSASSET,
                        INPUTRATE, BASERATE, IMPLIEDRATE, ZEROCOUPONYIELD,DISCOUNTFACTOR, FORWARDYIELD,
                        DTINS, IDAINS, EXTLLINK) 
						values 
                       (@IDYIELDCURVEVAL_H, @ASSETCATEGORY, @IDASSET, @DESCRIPTION, 
                        @DTSPOT, @PERIODMLTPTERM, @PERIODTERM, @DTTERM, 
                        @BASERATETYPE, @BASERATEMETHOD, @DAYSCURVEPOINT, @DAYSASSET, 
                        @INPUTRATE, @BASERATE, @IMPLIEDRATE, @ZEROCOUPONYIELD, @DISCOUNTFACTOR, @FORWARDYIELD,
                        @DTINS, @IDAINS, @EXTLLINK)";

            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDYIELDCURVEVAL_H", DbType.Int32), idYieldCurveVal_H);
            dp.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), assetCategorySpecified ? assetCategory.ToString() : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), idAssetSpecified ? idAsset : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), descriptionSpecified ? description : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "DTSPOT", DbType.Date), dtSpotSpecified ? dtSpot : Convert.DBNull);   // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "PERIODMLTPTERM", DbType.Int32), tenorTermSpecified ? tenorTerm.PeriodMultiplier.IntValue : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "PERIODTERM", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), tenorTermSpecified ? tenorTerm.Period.ToString() : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "DTTERM", DbType.Date), dtTermSpecified ? dtTerm : Convert.DBNull);  // FI 20201006 [XXXXX] DbType.Date
            dp.Add(new DataParameter(pCS, "BASERATETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), baseRateType.ToString());
            dp.Add(new DataParameter(pCS, "BASERATEMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), baseRateMethod.ToString());
            dp.Add(new DataParameter(pCS, "DAYSCURVEPOINT", DbType.Int32), daysCurvePointSpecified ? Convert.ToInt32(daysCurvePoint) : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "DAYSASSET", DbType.Int32), daysAssetSpecified ? Convert.ToInt32(daysAsset) : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "INPUTRATE", DbType.Decimal), inputRate);
            dp.Add(new DataParameter(pCS, "BASERATE", DbType.Decimal), baseRate);
            dp.Add(new DataParameter(pCS, "IMPLIEDRATE", DbType.Decimal), impliedRateSpecified ? impliedRate : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "DISCOUNTFACTOR", DbType.Decimal), discountFactorSpecified ? discountFactor : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "ZEROCOUPONYIELD", DbType.Decimal), zeroCouponYieldSpecified ? zeroCouponYield : Convert.DBNull);
            dp.Add(new DataParameter(pCS, "FORWARDYIELD", DbType.Decimal), forwardYieldSpecified ? forwardYield : Convert.DBNull);
            // FI 20200820 [25468] dates systèmes en UTC
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(pCS));
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pSession.IdA);
            dp.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), Convert.DBNull);
            //               
            QueryParameters qry = new QueryParameters(pCS, sql, dp);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
        }
        #endregion Insert
        
        
        #region InterpolationFormula
        private double InterpolationFormula(InterpolationTypeEnum pInterpolationType, InterpolationMethodEnum pInterpolationMethod,
            YieldCurveVal pYieldCurveVal, string pName, bool pIsFutureReaded)
        {
            YieldCurvePoint ycp_A = null;
            YieldCurvePoint ycp_B = null;

            switch (pInterpolationType)
            {
                case InterpolationTypeEnum.InterpolatedExtrapolated:
                case InterpolationTypeEnum.Interpolated:
                    pYieldCurveVal.PreviousAndNext(this, out ycp_A, out ycp_B, pIsFutureReaded);
                    if ((null != ycp_A) && (null == ycp_B))
                    {
                        ycp_B = ycp_A;
                        ycp_A = pYieldCurveVal.Previous(ycp_A);
                    }
                    break;
                case InterpolationTypeEnum.Extrapolated:
                    ycp_A = pYieldCurveVal.Next(this);
                    if (null != ycp_A)
                        ycp_B = pYieldCurveVal.Next(ycp_A);
                    break;
            }
            double resultValue;
            if ((null != ycp_A) && (null != ycp_B))
            {
                EFS_Interp_Linear interpolation = new EFS_Interp_Linear(pInterpolationMethod, pInterpolationType,
                    daysCurvePoint, ycp_A.GetValue(pName), ycp_A.daysCurvePoint, ycp_B.GetValue(pName), ycp_B.daysCurvePoint);
                resultValue = interpolation.ResultValue;
            }
            else
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "InterpolationMethod {0} not applied, curve points are missing", pInterpolationMethod.ToString());

            return resultValue;
        }
        #endregion InterpolationFormula
        #region ZeroCouponYieldCalculation
        public void ZeroCouponYieldCalculation(CompoundingFrequencyEnum pCompoundingFrequencyScheme)
        {
            double cst365 = Convert.ToDouble(365);

                //FI 20101116 Ajout de ce test pour éviter les divisions par zero 
            if (discountFactorSpecified && daysCurvePointSpecified)
            {
                if (CompoundingFrequencyEnum.Continuous == pCompoundingFrequencyScheme)
                {
                    zeroCouponYield = -1 * (Math.Log(discountFactor) * (cst365 / daysCurvePoint));
                }
                else
                {
                    double x = Convert.ToDouble(1 / discountFactor);
                    double y = cst365 / daysCurvePoint;
                    zeroCouponYield = Math.Pow(x, y) - 1;
                }
            }
            zeroCouponYield = System.Math.Round(zeroCouponYield, 12, MidpointRounding.AwayFromZero);
            zeroCouponYieldSpecified = (zeroCouponYield != 0);
        }
        #endregion ZeroCouponYieldCalculation
        #region ZeroCouponYieldInterpolationCalculation
        public void ZeroCouponYieldInterpolationCalculation(YieldCurveVal pYieldCurveVal)
        {
            zeroCouponYield = InterpolationFormula(InterpolationTypeEnum.InterpolatedExtrapolated, InterpolationMethodEnum.Linear, pYieldCurveVal, "zeroCouponYield", true);
            zeroCouponYield = System.Math.Round(zeroCouponYield, 12, MidpointRounding.AwayFromZero);
            zeroCouponYieldSpecified = (zeroCouponYield != 0);
        }
        #endregion ZeroCouponYieldInterpolationCalculation
        #endregion Methods
    }
    #endregion YieldCurvePoint
    #region YieldCurveTools
    public class YieldCurveTools
    {
        #region public static GetSqlAsset
        /// <summary>
        /// Obtient l'asset présent dans VW_ASSETFORCURVE
        /// <para>Obtient null si l'asset est non trouvé</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAsset"></param>
        /// <param name="idAsset"></param>
        /// <returns></returns>
        public static SQL_AssetForCurve GetSqlAsset(string pCS, Cst.UnderlyingAsset_Rate pAsset, int pIdAsset)
        {
            SQL_AssetForCurve ret = null;
            if (pIdAsset > 0)
            {
                SQL_AssetForCurve sqlAsset = new SQL_AssetForCurve(pCS, pAsset, pIdAsset);
                if (sqlAsset.LoadTable())
                    ret = sqlAsset;
            }
            return ret;
        }
        #endregion
        //
        #region public static isFuturePoint
        /// <summary>
        /// Obtient true si le point est un benchmark de type Future ou le point StubFutures
        /// </summary>
        /// <param name="pPoint"></param>
        /// <returns></returns>
        public static bool IsFuturePoint(YieldCurvePoint pPoint)
        {
            bool ret = Cst.UnderlyingAsset_Rate.Future == pPoint.assetCategory;
            if (false == ret)
                ret = BaseRateTypeEnum.StubFutures == pPoint.baseRateType;
            return ret;
        }
        #endregion
        //
        #region public static GetDefaultYieldCurve
        /// <summary>
        /// Retourne la courbe par défaut pour une devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        public static string GetDefaultYieldCurve(string pCS, string pIdC)
        {
            string ret = string.Empty;
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC), pIdC);
            //Find default YIELDCURVEDEF
            string sqlSelect = SQLCst.SELECT + "IDYIELDCURVEDEF" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.YIELDCURVEDEF + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "ISDEFAULT = 1" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "IDC=@IDC";

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect, dp.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToString(obj);
            return ret;
        }
        #endregion
    }
    #endregion YieldCurvePoint
}
