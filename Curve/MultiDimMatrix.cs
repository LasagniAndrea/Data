#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EfsML.Curve
{
    #region MultiDimMatrix
    public class MultiDimMatrix
    {
        #region Members
        public CurveParam m_CurveParam;

        private bool m_MatrixDefSpecified;

        public MatrixDef matrixDef;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <returns></returns>
        public bool IsLoaded(string pCS, IProductBase pProductBase)
        {
            if (IsMatrixDefFound)
            {
                if (IsMatrixValFound)
                {
                    if (IsMatrixValEnabled)
                        matrixDef.matrixVal.CheckMatrixPoints(pCS, pProductBase);
                    else
                        throw new Exception(StrFunc.AppendFormat(
                            "MatrixVal name [{0}] date [{1}] is disabled",
                            m_CurveParam.IdCurveDef, DtFunc.DateTimeToString(m_CurveParam.DtCalculation, DtFunc.FmtShortDate)));

                }
                else if (IsMatrixValImported)
                {

                    throw new Exception(StrFunc.AppendFormat(
                        "MatrixVal name [{0}] date [{1}] (imported) not found",
                        m_CurveParam.IdCurveDef,
                        DtFunc.DateTimeToString(m_CurveParam.DtCalculation, DtFunc.FmtShortDate)));

                }
                else
                {
                    // EG 20160404 Migration vs2013
                    throw new Exception(StrFunc.AppendFormat(
                        "MatrixVal name [{0}] date [{1}] Market Env.: [{2}] Side:[{3}] not found",
                        m_CurveParam.IdCurveDef,
                        DtFunc.DateTimeToString(m_CurveParam.DtCalculation, DtFunc.FmtShortDate),
                        m_CurveParam.IdMarketEnvironment, m_CurveParam.CurveSide.ToString()));

                }
            }
            else
            {
                throw new Exception(StrFunc.AppendFormat("MatrixDef name [{0}] not found", m_CurveParam.IdCurveDef));
            }

            return true;
        }
        
        
        
        #region IsMatrixDefFound
        public bool IsMatrixDefFound
        {
            get { return m_MatrixDefSpecified; }
        }
        #endregion IsMatrixDefFound
        #region IsMatrixValEnabled
        public bool IsMatrixValEnabled
        {
            get { return matrixDef.IsMatrixValEnabled; }
        }
        #endregion IsMatrixValEnabled
        #region IsMatrixValFound
        public bool IsMatrixValFound
        {
            get { return matrixDef.IsMatrixValFound; }
        }
        #endregion IsMatrixValFound
        #region IsMatrixValImported
        public bool IsMatrixValImported
        {
            get { return matrixDef.IsMatrixValImported; }
        }
        #endregion IsMatrixValImported
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Instanciation of the CurveParam class and research of the matrix using these curve parameters.
        /// </summary>
        public MultiDimMatrix(string pCS, IProductBase pProductBase, string pIdMatrixDef, DateTime pDtCalculation, string pIdC)
        {
            m_CurveParam = new CurveParam(pCS, pIdMatrixDef, pDtCalculation, pIdC);
            SelectMatrix(pCS, pProductBase);
        }
        /// <summary>
        /// Recherche de la matrice à partir de son identifiant
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdMatrixDef">Identifiant de la matrice</param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pIdC"></param>
        public MultiDimMatrix(string pCS, IProductBase pProductBase, DateTime pDtCalculation, string pIdC)
            : this(pCS, pProductBase, null, null, pDtCalculation, pIdC, null ) { }
        
      
        /// <summary>
        /// Recherche de la matrice à partir des acteurs payers/receivers et de l'asset
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdA_Pay"></param>
        /// <param name="pIdB_Pay"></param>
        /// <param name="pIdA_Rec"></param>
        /// <param name="pIdB_Rec"></param>
        /// <param name="pDtCalculation"></param>
        /// <param name="pAsset"></param>
        // EG 20150706 [21021] Pair<int,Nullable<int>> for pPayer|pReceiver
        public MultiDimMatrix(string pCS, IProductBase pProductBase,
            Pair<int, Nullable<int>> pPayer, Pair<int, Nullable<int>> pReceiver, DateTime pDtCalculation, string pIdC, Pair<Cst.UnderlyingAsset, int> pAsset)
        {
            m_CurveParam = new CurveParam(pCS, pPayer, pReceiver, pDtCalculation, pIdC, pAsset);
            SelectMatrix(pCS, pProductBase);
        }




        #endregion Constructors
        #region Methods
        
        #region GetPointValue
        #region GetPointValue with Matrix Structure : Expiration / Strike
        public double GetPointValue(string pCS, IInterval pTenorExpiration, double pStrike)
        {
            return GetPointValue(pCS, MatrixInterpolationUsed(2), pTenorExpiration, pStrike);
        }
        public double GetPointValue(string pCS, DateTime pDtExpiration, double pStrike)
        {
            return GetPointValue(pCS, MatrixInterpolationUsed(2), pDtExpiration, pStrike);
        }
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, IInterval pTenorExpiration, double pStrike)
        {
            MatrixStructure[] points = new MatrixStructure[2];
            points[0] = new MatrixStructure(MatrixStructureTypeEnum.Expiration, pTenorExpiration);
            points[1] = new MatrixStructure(MatrixStructureTypeEnum.Strike, pStrike);
            return GetPointValue(pCS, pInterpolationMethod, points);
        }
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, DateTime pDtExpiration, double pStrike)
        {
            MatrixStructure[] points = new MatrixStructure[2];
            points[0] = new MatrixStructure(MatrixStructureTypeEnum.Expiration, pDtExpiration);
            points[1] = new MatrixStructure(MatrixStructureTypeEnum.Strike, pStrike);
            return GetPointValue(pCS, pInterpolationMethod, points);
        }
        #endregion GetPointValue with Matrix Structure : Expiration / Strike
        #region GetPointValue with Matrix Structure : Expiration / Strike / Term
        public double GetPointValue(string pCS, IInterval pTenorExpiration, double pStrike, IInterval pTenorTerm)
        {
            return GetPointValue(pCS, MatrixInterpolationUsed(3), pTenorExpiration, pStrike, pTenorTerm);
        }
        public double GetPointValue(string pCS, DateTime pDtExpiration, double pStrike, DateTime pDtTerm)
        {
            return GetPointValue(pCS, MatrixInterpolationUsed(3), pDtExpiration, pStrike, pDtTerm);
        }
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, IInterval pTenorExpiration, double pStrike, IInterval pTenorTerm)
        {
            MatrixStructure[] points = new MatrixStructure[3];
            points[0] = new MatrixStructure(MatrixStructureTypeEnum.Expiration, pTenorExpiration);
            points[1] = new MatrixStructure(MatrixStructureTypeEnum.Strike, pStrike);
            points[2] = new MatrixStructure(MatrixStructureTypeEnum.Term, pTenorTerm);
            return GetPointValue(pCS, pInterpolationMethod, points);
        }
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, DateTime pDtExpiration, double pStrike, DateTime pDtTerm)
        {
            MatrixStructure[] points = new MatrixStructure[3];
            points[0] = new MatrixStructure(MatrixStructureTypeEnum.Expiration, pDtExpiration);
            points[1] = new MatrixStructure(MatrixStructureTypeEnum.Strike, pStrike);
            points[2] = new MatrixStructure(MatrixStructureTypeEnum.Term, pDtTerm);
            return GetPointValue(pCS, pInterpolationMethod, points);
        }
        #endregion GetPointValue with Matrix Structure : Expiration / Strike / Term
        #region GetPointValue with Matrix Structure : Specified by user
        public double GetPointValue(string pCS, params MatrixStructure[] pPoints)
        {
            return GetPointValue(pCS, MatrixInterpolationUsed(pPoints.Length), pPoints);
        }
        /// <summary>
        /// Evaluation of a point per interpolation on the matrix
        /// </summary>
        /// <param name="pInterpolationMethod">Interpolation method used</param>
        /// <param name="pPoints">List of all Co-ordinates of the point to be evaluated</param>
        /// <returns></returns>
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, params MatrixStructure[] pPoints)
        {
            return matrixDef.matrixVal.GetPointValue(pCS, pInterpolationMethod, pPoints);
        }
        #endregion GetPointValue with Matrix Structure : Specified by user
        #endregion GetPointValue
        
        /// <summary>
        /// Request of selection in table MATRIXDEF
        /// </summary>
        /// <returns>SQL Select string</returns>
        private static string GetSelectMatrixDef(string pCS)
        {
            StrBuilder select = new StrBuilder(SQLCst.SELECT + @"mx.IDMATRIXDEF, " + Cst.CrLf);
            select += @"mx.IDMARKETENV, mx.IDVALSCENARIO, mx.DISPLAYNAME, mx.SIDE, mx.IDC, " + Cst.CrLf;
            select += @"mx.ASSETCATEGORY, mx.IDASSET, " + Cst.CrLf;
            select += @"mx.ISDEFAULT, mx.ISIMPORTED, mx.INTERPOLATIONMETH, " + Cst.CrLf;
            select += @"mx.POLYNOMIALDEGREE, mx.ISEXTRAPOLATION, mx.CASHFLOWTYPE, " + Cst.CrLf;
            select += @"mx.PERIODMLTPSPOTOFFSET, mx.PERIODSPOTOFFSET, mx.DAYTYPESPOTOFFSET, " + Cst.CrLf;
            select += @"mx.PERIODMLTPINPUTOFFSET, mx.PERIODINPUTOFFSET, mx.DAYTYPEINPUTOFFSET, " + Cst.CrLf;
            select += @"mx.PERIODMLTPENDOFFSET, mx.PERIODENDOFFSET, mx.DAYTYPEENDOFFSET" + Cst.CrLf;
            select += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATRIXDEF + " mx " + Cst.CrLf;
            select += SQLCst.WHERE + @"(mx.IDMATRIXDEF = @IDMATRIXDEF)" + Cst.CrLf;
            // Pour palier au Bug ORACLE Plusieurs querys/plusieurs paramètres
            select += SQLCst.AND + @"(@DTBASE=@DTBASE)" + Cst.CrLf;
            select += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mx").ToString();
            return select.ToString();
        }
        
        /// <summary>
        /// Request of selection in table MATRIXVAL
        /// </summary>
        /// <returns>SQL Select string</returns>
        private static string GetSelectMatrixVal()
        {
            StrBuilder select = new StrBuilder(SQLCst.SELECT + @"mx.IDMATRIXVAL_H, mx.IDMATRIXDEF, " + Cst.CrLf);
            select += @"mx.SIDE, mx.IDC, mx.DTBASE, mx.DTSPOT, mx.DTINPUTDATA, mx.DTEND, mx.DTBUILDDATE, " + Cst.CrLf;
            select += @"mx.SOURCE, mx.ISENABLED, mx.DTINS, mx.IDAINS, mx.DTUPD, mx.IDAUPD" + Cst.CrLf;
            select += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATRIXVAL_H + " mx " + Cst.CrLf;
            select += SQLCst.WHERE + @"(mx.IDMATRIXDEF=@IDMATRIXDEF)" + Cst.CrLf;
            select += SQLCst.AND + @"(mx.DTBASE=@DTBASE)";
            return select.ToString();
        }
        
        #region MatrixInterpolationUsed
        private MatrixInterpolationMethodEnum MatrixInterpolationUsed(int pDim)
        {
            MatrixInterpolationMethodEnum method;
            if (matrixDef.interpolationMethodSpecified)
                method = matrixDef.interpolationMethod;
            else
            {
                switch (pDim)
                {
                    case 1:
                        method = MatrixInterpolationMethodEnum.Linear;
                        break;
                    case 2:
                        method = MatrixInterpolationMethodEnum.BiCubicSpline;
                        break;
                    case 3:
                        method = MatrixInterpolationMethodEnum.TriLinear;
                        break;
                    default:
                        method = MatrixInterpolationMethodEnum.NearestNeighbor;
                        break;
                }
            }
            return method;
        }
        #endregion MatrixInterpolationUsed


        /// <summary>
        /// Seek of a matrix of valorization according to the parameters (CurveParam). 
        /// Loading whole of the data in the respective classes (MatrixDef, MatrixVal)
        /// </summary>
        private void SelectMatrix(string pCS, IProductBase pProductBase)
        {
            
            if (false == m_CurveParam.IdCurveDefSpecified)
                throw new Exception("No MatrixDef");


            StrBuilder SQLSelect = new StrBuilder(GetSelectMatrixDef(pCS) + SQLCst.SEPARATOR_MULTISELECT);
            SQLSelect += GetSelectMatrixVal() + SQLCst.SEPARATOR_MULTISELECT;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDMATRIXDEF", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), m_CurveParam.IdCurveDef);
            dp.Add(new DataParameter(pCS, "DTBASE", DbType.Date), m_CurveParam.DtCalculation); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters query = new QueryParameters(pCS, SQLSelect.ToString(), dp);

            DataSet dsMatrix = DataHelper.ExecuteDataset(pCS, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
            #region DataSet Initialize
            dsMatrix.DataSetName = "Matrix";
            DataTable dtMatrixDef = dsMatrix.Tables[0];
            dtMatrixDef.TableName = "MatrixDef";
            DataTable dtMatrixVal = dsMatrix.Tables[1];
            dtMatrixVal.TableName = "MatrixVal";
            #endregion DataSet Initialize

            m_MatrixDefSpecified = (null != dsMatrix) && (1 == dtMatrixDef.Rows.Count);
            if (m_MatrixDefSpecified)
            {
                #region MatrixDef filling
                DataRow rowMatrix = dtMatrixDef.Rows[0];
                matrixDef = new MatrixDef
                {
                    idMatrixDef = rowMatrix["IDMATRIXDEF"].ToString(),
                    displayName = rowMatrix["DISPLAYNAME"].ToString(),
                    idMarketEnv = rowMatrix["IDMARKETENV"].ToString(),
                    idValScenario = rowMatrix["IDVALSCENARIO"].ToString(),
                    sideSpecified = (false == Convert.IsDBNull(rowMatrix["SIDE"])),
                    idCSpecified = (false == Convert.IsDBNull(rowMatrix["IDC"])),
                    assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), rowMatrix["ASSETCATEGORY"].ToString(), true),
                    idAsset = Convert.ToInt32(rowMatrix["IDASSET"])
                };

                if (matrixDef.sideSpecified)
                    matrixDef.side = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum),
                        rowMatrix["SIDE"].ToString(), true);
                if (matrixDef.idCSpecified)
                    matrixDef.idC = rowMatrix["IDC"].ToString();


                bool isMatrixValParametersRespected = (false == matrixDef.sideSpecified) || (matrixDef.side == m_CurveParam.CurveSide);
                isMatrixValParametersRespected &= ((false == matrixDef.idCSpecified) || (matrixDef.idC == m_CurveParam.IdC));
                if (m_CurveParam.AssetSpecified)
                    isMatrixValParametersRespected &= (matrixDef.assetCategory == m_CurveParam.Asset.First);   
                #endregion MatrixDef filling

                if (isMatrixValParametersRespected)
                {
                    #region Asset informations loading
                    if (System.Enum.IsDefined(typeof(Cst.UnderlyingAsset_Rate), matrixDef.assetCategory.ToString() ))
                    {
                        Cst.UnderlyingAsset_Rate category = (Cst.UnderlyingAsset_Rate)System.Enum.Parse(typeof(Cst.UnderlyingAsset_Rate), matrixDef.assetCategory.ToString(), true);
                        SQL_AssetForCurve sql_AssetForCurve = new SQL_AssetForCurve(pCS, category, matrixDef.idAsset);
                        if (sql_AssetForCurve.IsLoaded)
                            matrixDef.BDC = sql_AssetForCurve.FpML_Enum_BdcCurve;
                    }
                    else
                    {
                        matrixDef.BDC = BusinessDayConventionEnum.FOLLOWING;  
                    }
                    #endregion Asset informations loading

                    
                    #region MatrixDef filling continue
                    matrixDef.isDefault = Convert.ToBoolean(rowMatrix["ISDEFAULT"]);
                    matrixDef.isImported = Convert.ToBoolean(rowMatrix["ISIMPORTED"]);
                    matrixDef.interpolationMethodSpecified = (false == Convert.IsDBNull(rowMatrix["INTERPOLATIONMETH"]));
                    if (matrixDef.interpolationMethodSpecified)
                        matrixDef.interpolationMethod = (MatrixInterpolationMethodEnum)System.Enum.Parse(typeof(MatrixInterpolationMethodEnum),
                            rowMatrix["INTERPOLATIONMETH"].ToString(), true);
                    matrixDef.polynomialDegreeSpecified = (false == Convert.IsDBNull(rowMatrix["POLYNOMIALDEGREE"]));
                    if (matrixDef.polynomialDegreeSpecified)
                        matrixDef.polynomialDegree = Convert.ToInt32(rowMatrix["POLYNOMIALDEGREE"]);
                    matrixDef.isExtrapolation = Convert.ToBoolean(rowMatrix["ISEXTRAPOLATION"]);
                    matrixDef.cashFlowTypeSpecified = (false == Convert.IsDBNull(rowMatrix["CASHFLOWTYPE"]));
                    if (matrixDef.cashFlowTypeSpecified)
                        matrixDef.cashFlowType = (CashFlowTypeEnum)System.Enum.Parse(typeof(CashFlowTypeEnum),
                            rowMatrix["CASHFLOWTYPE"].ToString(), true);

                    matrixDef.spotOffsetSpecified = (false == Convert.IsDBNull(rowMatrix["PERIODMLTPSPOTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["PERIODSPOTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["DAYTYPESPOTOFFSET"]));
                    if (matrixDef.spotOffsetSpecified)
                        matrixDef.spotOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowMatrix["PERIODSPOTOFFSET"].ToString()),
                            Convert.ToInt32(rowMatrix["PERIODMLTPSPOTOFFSET"]),
                            StringToEnum.DayType(rowMatrix["DAYTYPESPOTOFFSET"].ToString()));

                    matrixDef.inputOffsetSpecified = (false == Convert.IsDBNull(rowMatrix["PERIODMLTPINPUTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["PERIODINPUTOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["DAYTYPEINPUTOFFSET"]));

                    if (matrixDef.inputOffsetSpecified)
                        matrixDef.inputOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowMatrix["PERIODINPUTOFFSET"].ToString()),
                            Convert.ToInt32(rowMatrix["PERIODMLTPINPUTOFFSET"]),
                            StringToEnum.DayType(rowMatrix["DAYTYPEINPUTOFFSET"].ToString()));

                    matrixDef.endOffsetSpecified = (false == Convert.IsDBNull(rowMatrix["PERIODMLTPENDOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["PERIODENDOFFSET"])) &&
                        (false == Convert.IsDBNull(rowMatrix["DAYTYPEENDOFFSET"]));

                    if (matrixDef.endOffsetSpecified)
                        matrixDef.endOffset = pProductBase.CreateOffset(
                            StringToEnum.Period(rowMatrix["PERIODENDOFFSET"].ToString()),
                            Convert.ToInt32(rowMatrix["PERIODMLTPENDOFFSET"]),
                            StringToEnum.DayType(rowMatrix["DAYTYPEENDOFFSET"].ToString()));

                    #endregion MatrixDef filling continue
                    #region MatrixVal filling
                    matrixDef.matrixValSpecified = (1 == dtMatrixVal.Rows.Count);
                    if (matrixDef.matrixValSpecified)
                    {
                        DataRow rowMatrixVal = dtMatrixVal.Rows[0];
                        matrixDef.matrixVal = new MatrixVal
                        {
                            BDC = matrixDef.BDC,
                            idMatrixVal_H = Convert.ToInt32(rowMatrixVal["IDMATRIXVAL_H"]),
                            idMatrixDef = matrixDef.idMatrixDef,
                            sideSpecified = (false == Convert.IsDBNull(rowMatrixVal["SIDE"])),
                            idCSpecified = (false == Convert.IsDBNull(rowMatrixVal["IDC"])),
                            InterpolationMethodUsed = MatrixInterpolationMethodEnum.Linear,
                            dtBase = Convert.ToDateTime(rowMatrixVal["DTBASE"]),
                            dtSpot = Convert.ToDateTime(rowMatrixVal["DTSPOT"]),
                            dtInputData = Convert.ToDateTime(rowMatrixVal["DTINPUTDATA"]),
                            dtEnd = Convert.ToDateTime(rowMatrixVal["DTEND"]),
                            dtBuildDate = Convert.ToDateTime(rowMatrixVal["DTBUILDDATE"]),
                            isEnabled = Convert.ToBoolean(rowMatrixVal["ISENABLED"]),
                            source = rowMatrixVal["SOURCE"].ToString()
                        };

                        if (matrixDef.matrixVal.sideSpecified)
                            matrixDef.matrixVal.side = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum), rowMatrixVal["SIDE"].ToString(), true);

                        if (matrixDef.matrixVal.idCSpecified)
                        {
                            matrixDef.matrixVal.idC = rowMatrixVal["IDC"].ToString();
                            #region GetBusinessCenters and DayCountFraction for currency
                            SQL_Currency currency = new SQL_Currency(pCS, SQL_Currency.IDType.IdC, matrixDef.matrixVal.idC);
                            if (currency.IsLoaded)
                            {
                                matrixDef.matrixVal.BusinessCentersCurrency = pProductBase.CreateBusinessCenters(currency.IdBC, currency.IdBC2, currency.IdBC3);
                                matrixDef.matrixVal.DCFCurrency = currency.FpML_Enum_DayCountFraction;
                            }
                            #endregion GetBusinessCenters and DayCountFraction for currency
                        }
                        if (matrixDef.polynomialDegreeSpecified)
                            matrixDef.matrixVal.polynomialDegree = matrixDef.polynomialDegree;
                        else
                            matrixDef.matrixVal.polynomialDegree = 1;
                    }
                    #endregion MatrixVal filling
                }
                else
                    throw new Exception(StrFunc.AppendFormat(
                        "MatrixDef {0} is incorrect : Requested  [{1}/{2}] Found [{3}/{4}]",
                        m_CurveParam.IdCurveDef,
                        m_CurveParam.CurveSide.ToString(), m_CurveParam.IdC,
                        matrixDef.side.ToString(), matrixDef.idC));
            }
        }

        #endregion Methods
    }
    #endregion MultiDimMatrix
    #region MatrixDef
    public class MatrixDef
    {
        #region Members
        

        public string idMatrixDef;
        public string displayName;
        public string idMarketEnv;
        public string idValScenario;
        public bool sideSpecified;
        public QuotationSideEnum side;
        public bool idCSpecified;
        public string idC;
        public Cst.UnderlyingAsset assetCategory;
        public int idAsset;
        public bool isDefault;
        public bool isImported;
        public bool interpolationMethodSpecified;
        public MatrixInterpolationMethodEnum interpolationMethod;
        public bool polynomialDegreeSpecified;
        public int polynomialDegree;
        public bool isExtrapolation;
        public bool cashFlowTypeSpecified;
        public CashFlowTypeEnum cashFlowType;
        public bool spotOffsetSpecified;
        public IOffset spotOffset;
        public bool inputOffsetSpecified;
        public IOffset inputOffset;
        public bool endOffsetSpecified;
        public IOffset endOffset;

        public bool matrixValSpecified;
        public MatrixVal matrixVal;
        #endregion Members
        #region Accessors

        #region BusinessDayConvention
        public BusinessDayConventionEnum BDC
        {
            set;
            get;
        }
        #endregion BusinessDayConvention
        #region BusinessCentersCurrency
        //public IBusinessCenters BusinessCentersCurrency
        //{
        //    set;
        //    get;
        //}
        #endregion BusinessCentersCurrency
        #region DayCountFractionCurrency
        //public DayCountFractionEnum DCFCurrency
        //{
        //    set;
        //    get;
        //}
        #endregion BusinessCentersCurrency
        #region IsMatrixValEnabled
        public bool IsMatrixValEnabled
        {
            get { return matrixValSpecified && matrixVal.isEnabled; }
        }
        #endregion IsMatrixValEnabled
        #region IsMatrixValFound
        public bool IsMatrixValFound
        {
            get { return matrixValSpecified; }
        }
        #endregion IsMatrixValFound
        #region IsMatrixValImported
        public bool IsMatrixValImported
        {
            get { return isImported; }
        }
        #endregion IsMatrixValImported
        #endregion Accessors
        #region Constructors
        public MatrixDef() { }
        #endregion Constructors
    }
    #endregion MatrixDef
    #region MatrixVal
    public class MatrixVal
    {
        #region Members
        private IBusinessCenters m_BusinessCenters;
        private DayCountFractionEnum m_DCF;
        private MatrixInterpolationMethodEnum m_InterpolationMethodUsed;
        private BusinessDayConventionEnum m_BDC;

        public int idMatrixVal_H;
        public string idMatrixDef;
        public bool sideSpecified;
        public QuotationSideEnum side;
        public bool idCSpecified;
        public string idC;
        public DateTime dtBase;
        public DateTime dtSpot;
        public DateTime dtInputData;
        public DateTime dtEnd;
        public DateTime dtBuildDate;
        public bool isEnabled;
        public string source;
        public int polynomialDegree;
        public bool matrixPointsSpecified;
        public MatrixPoint[] matrixPoints;
        #endregion Members
        #region Accessors

        #region BusinessDayConvention
        public BusinessDayConventionEnum BDC
        {
            set { m_BDC = value; }
            get { return m_BDC; }
        }
        #endregion BusinessDayConvention
        #region BusinessCentersCurrency
        public IBusinessCenters BusinessCentersCurrency
        {
            set { m_BusinessCenters = value; }
            get { return m_BusinessCenters; }
        }
        #endregion BusinessCentersCurrency
        #region DayCountFractionCurrency
        public DayCountFractionEnum DCFCurrency
        {
            set { m_DCF = value; }
            get { return m_DCF; }
        }
        #endregion BusinessCentersCurrency
        #region InterpolationMethodUsed
        public MatrixInterpolationMethodEnum InterpolationMethodUsed
        {
            get { return m_InterpolationMethodUsed; }
            set { m_InterpolationMethodUsed = value; }
        }
        #endregion InterpolationMethodUsed

        #endregion Accessors
        #region Constructors
        public MatrixVal() { }
        #endregion Constructors
        #region Methods
        #region CheckMatrixPoints
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pMatrixDef"></param>
        public void CheckMatrixPoints(string pCS, IProductBase pProductBase)
        {
            SelectMatrixPoint(pCS, pProductBase);
            if (false == matrixPointsSpecified)
            {
                throw new Exception(StrFunc.AppendFormat(  
                    "MatrixVal name [{0}] date [{1}] hasn't any point value",idMatrixDef, DtFunc.DateTimeToString(dtBase, DtFunc.FmtShortDate)));                
            }
        }
        #endregion CheckMatrixPoints
        #region GetPointValue
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public double GetPointValue(string pCS, MatrixInterpolationMethodEnum pInterpolationMethod, params MatrixStructure[] pPoints)
        {
            double pointValue = 0;
            if (null != pPoints && (0 < pPoints.Length))
            {
                #region X_Value setting
                for (int i = 0; i < pPoints.Length; i++)
                {
                    MatrixStructure point = pPoints[i];
                    if (null != point)
                    {
                        if (false == point.x_ValueSpecified)
                        {
                            if (point.tenorSpecified)
                            {
                                point.date = Tools.ApplyAdjustedInterval(pCS, dtSpot, point.tenor, BusinessCentersCurrency, BDC, null);
                                point.dateSpecified = DtFunc.IsDateTimeFilled(point.date);
                            }
                            if (point.dateSpecified)
                            {
                                TimeSpan timeSpan = point.date - dtBase;
                                point.x_Value = Convert.ToDouble(timeSpan.Days);
                                point.x_ValueSpecified = (0 < point.x_Value);
                            }
                        }
                        if (false == point.x_ValueSpecified)
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                "Incorrect Multidimentional pricing data parameters {0} : Error X value ", point.structureType.ToString());
                    }
                }


                #endregion X_Value setting
                double[,] Xa = new Double[matrixPoints.Length, matrixPoints[0].matrixPointStructures.Length];
                double[] Ya = new Double[matrixPoints.Length];
                for (int i = 0; i < matrixPoints.Length; i++)
                {
                    MatrixPoint matrixPoint = matrixPoints[i];
                    int max = matrixPoint.matrixPointStructures.Length;
                    for (int j = 0; j < max; j++)
                        Xa[i, j] = matrixPoint.matrixPointStructures[j].matrixStructure.x_Value;
                    Ya[i] = matrixPoint.y_Value;
                }
                double[] X = new Double[pPoints.Length];
                for (int i = 0; i < pPoints.Length; i++)
                {
                    X[i] = pPoints[i].x_Value;
                }

                EFS_MxInterp_Base interpolation;
                switch (pInterpolationMethod)
                {
                    case MatrixInterpolationMethodEnum.Linear:
                    case MatrixInterpolationMethodEnum.BiLinear:
                    case MatrixInterpolationMethodEnum.TriLinear:
                        interpolation = new EFS_MxInterp_Linear(pInterpolationMethod, Xa, Ya, X);
                        pointValue = interpolation.ResultValue;
                        break;
                    case MatrixInterpolationMethodEnum.NearestNeighbor:
                        interpolation = new EFS_MxInterp_NearestNeighbor(Xa, Ya, X);
                        pointValue = interpolation.ResultValue;
                        break;
                    case MatrixInterpolationMethodEnum.BiCubicPolynomial:
                        interpolation = new EFS_MxInterp_BiCubicPolynomial(Xa, Ya, X);
                        pointValue = interpolation.ResultValue;
                        break;
                    case MatrixInterpolationMethodEnum.BiCubicSpline:
                        interpolation = new EFS_MxInterp_BiCubicSpline(Xa, Ya, X);
                        pointValue = interpolation.ResultValue;
                        break;
                }
            }
            else
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Incorrect Multidimentional pricing data : parameters are undefined");
            return pointValue;
        }
        #endregion GetPointValue

        /// <summary>
        /// select table MATRIXPT_H
        /// </summary>
        /// <returns></returns>
        private static string GetSelectMatrixPoint()
        {
            StrBuilder select = new StrBuilder(SQLCst.SELECT + @"mp.IDMATRIXVAL_H, " + Cst.CrLf);
            select += @"mp.POINTNO, mp.Y_VALUE, " + Cst.CrLf;
            select += @"mp.DTINS, mp.IDAINS, mp.DTUPD, mp.IDAUPD" + Cst.CrLf;
            select += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATRIXPT_H + " mp " + Cst.CrLf;
            select += SQLCst.WHERE + @"(mp.IDMATRIXVAL_H=@IDMATRIXVAL_H)" + Cst.CrLf;
            return select.ToString();
        }
        /// <summary>
        /// select table MATRIXPTSTRUCT_H
        /// </summary>
        /// <returns></returns>
        private static string GetSelectMatrixPointStructure()
        {
            StrBuilder select = new StrBuilder(SQLCst.SELECT + @"mp.IDMATRIXVAL_H, " + Cst.CrLf);
            select += @"mp.POINTNO, mp.STRUCTURE, " + Cst.CrLf;
            select += @"mp.PERIODMLTP, mp.PERIOD, mp.DTPOINT, mp.X_VALUE, " + Cst.CrLf;
            select += @"mp.DTINS, mp.IDAINS, mp.DTUPD, mp.IDAUPD" + Cst.CrLf;
            select += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATRIXPTSTRUCT_H + " mp " + Cst.CrLf;
            select += SQLCst.WHERE + @"(mp.IDMATRIXVAL_H=@IDMATRIXVAL_H)" + Cst.CrLf;
            return select.ToString();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        private void SelectMatrixPoint(string pCS, IProductBase pProductBase)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDMATRIXVAL_H", DbType.Int32), idMatrixVal_H);

            StrBuilder SQLSelect = new StrBuilder(GetSelectMatrixPoint() + SQLCst.SEPARATOR_MULTISELECT);
            SQLSelect += GetSelectMatrixPointStructure() + SQLCst.SEPARATOR_MULTISELECT;
            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), parameters);
            DataSet dsMatrixPoint = DataHelper.ExecuteDataset(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            #region DataSet Initialize
            dsMatrixPoint.DataSetName = "MatrixPoint";
            DataTable dtMatrixPoint = dsMatrixPoint.Tables[0];
            dtMatrixPoint.TableName = "MatrixPoint";
            DataTable dtMatrixPointStructure = dsMatrixPoint.Tables[1];
            dtMatrixPointStructure.TableName = "MatrixPointStructure";

            DataColumn[] parentColumn = new DataColumn[2] { dtMatrixPoint.Columns["IDMATRIXVAL_H"], dtMatrixPoint.Columns["POINTNO"] };
            DataColumn[] childColumn = new DataColumn[2] { dtMatrixPointStructure.Columns["IDMATRIXVAL_H"], dtMatrixPointStructure.Columns["POINTNO"] };
            DataRelation rel = new DataRelation("MatrixPoint_MatrixPointStructure", parentColumn, childColumn, false);
            dsMatrixPoint.Relations.Add(rel);
            #endregion DataSet Initialize
            #region MatrixPoint filling
            matrixPointsSpecified = (null != dsMatrixPoint) && (0 < dtMatrixPoint.Rows.Count);
            if (matrixPointsSpecified)
            {
                ArrayList aMatrixPoints = new ArrayList();
                foreach (DataRow rowMatrixPoint in dtMatrixPoint.Rows)
                {
                    MatrixPoint matrixPoint = new MatrixPoint
                    {
                        idMatrixVal_H = idMatrixVal_H,
                        pointNo = Convert.ToInt32(rowMatrixPoint["POINTNO"]),
                        y_ValueSpecified = (false == Convert.IsDBNull(rowMatrixPoint["Y_VALUE"])) && (0 != Convert.ToDouble(rowMatrixPoint["Y_VALUE"]))
                    };
                    if (matrixPoint.y_ValueSpecified)
                        matrixPoint.y_Value = Convert.ToDouble(rowMatrixPoint["Y_VALUE"]);

                    DataRow[] rowMatrixStructures = rowMatrixPoint.GetChildRows(rel);
                    matrixPoint.matrixPointStructuresSpecified = (0 < rowMatrixStructures.Length);
                    if (matrixPoint.matrixPointStructuresSpecified)
                    {
                        ArrayList aMatrixPointStructures = new ArrayList();
                        foreach (DataRow rowStructure in rowMatrixStructures)
                        {
                            MatrixStructure matrixStructure = new MatrixStructure
                            {
                                structureType = (MatrixStructureTypeEnum)
                                System.Enum.Parse(typeof(MatrixStructureTypeEnum), rowStructure["STRUCTURE"].ToString(), true),
                                tenorSpecified = (false == Convert.IsDBNull(rowStructure["PERIOD"])) &&
                                                                        (false == Convert.IsDBNull(rowStructure["PERIODMLTP"])),

                                dateSpecified = (false == Convert.IsDBNull(rowStructure["DTPOINT"])),
                                x_ValueSpecified = (false == Convert.IsDBNull(rowStructure["X_VALUE"])) &&
                                                                        (0 < Convert.ToDouble(rowStructure["X_VALUE"]))
                            };

                            if (matrixStructure.tenorSpecified)
                                matrixStructure.tenor = pProductBase.CreateInterval(
                                    StringToEnum.Period(rowStructure["PERIOD"].ToString()), Convert.ToInt32(rowStructure["PERIODMLTP"]));
                            if (matrixStructure.dateSpecified)
                                matrixStructure.date = Convert.ToDateTime(rowStructure["DTPOINT"]);
                            if (matrixStructure.x_ValueSpecified)
                                matrixStructure.x_Value = Convert.ToDouble(rowStructure["X_VALUE"]);

                            MatrixPointStructure matrixPointStructure = new MatrixPointStructure
                            {
                                idMatrixVal_H = matrixPoint.idMatrixVal_H,
                                pointNo = matrixPoint.pointNo,
                                matrixStructure = matrixStructure
                            };                            
                            aMatrixPointStructures.Add(matrixPointStructure);
                        }
                        matrixPoint.matrixPointStructures = (MatrixPointStructure[])aMatrixPointStructures.ToArray(typeof(MatrixPointStructure));
                    }
                    aMatrixPoints.Add(matrixPoint);
                }
                matrixPoints = (MatrixPoint[])aMatrixPoints.ToArray(typeof(MatrixPoint));
            }
            #endregion MatrixPoint filling
            
        }
        
        #endregion Methods
    }
    #endregion MatrixVal
    #region MatrixPoint
    public class MatrixPoint
    {
        #region Members
        public int idMatrixVal_H;
        public int pointNo;
        public bool matrixPointStructuresSpecified;
        public MatrixPointStructure[] matrixPointStructures;
        public bool y_ValueSpecified;
        public double y_Value;
        #endregion Members
        #region Constructors
        public MatrixPoint() { }
        #endregion Constructors
    }
    #endregion MatrixPoint
    #region MatrixPointStructure
    public class MatrixPointStructure
    {
        #region Members
        public int idMatrixVal_H;
        public int pointNo;
        public MatrixStructure matrixStructure;
        #endregion Members
        #region Constructors
        public MatrixPointStructure() { }
        #endregion Constructors
    }
    #endregion MatrixPointStructure
    #region MatrixStructure
    public class MatrixStructure
    {
        #region Members
        public MatrixStructureTypeEnum structureType;
        public bool dateSpecified;
        public DateTime date;
        public bool tenorSpecified;
        public IInterval tenor;
        public bool x_ValueSpecified;
        public double x_Value;
        #endregion Members
        #region Constructors
        public MatrixStructure()
        {
        }
        public MatrixStructure(MatrixStructureTypeEnum pStructureType, DateTime pDate)
        {
            structureType = pStructureType;
            dateSpecified = DtFunc.IsDateTimeFilled(pDate);
            date = pDate;
        }
        /*
        public MatrixStructure(MatrixStructureTypeEnum pStructureType, int pPeriodMultiplier, string pPeriod)
        {
            structureType  = pStructureType;
            tenor          = new Interval(pPeriod,pPeriodMultiplier);
            tenorSpecified = (null != tenor);
        }
        */
        public MatrixStructure(MatrixStructureTypeEnum pStructureType, IInterval pTenor)
        {
            structureType = pStructureType;
            tenorSpecified = (null != pTenor);
            tenor = pTenor;
        }
        public MatrixStructure(MatrixStructureTypeEnum pStructureType, double pXValue)
        {
            structureType = pStructureType;
            x_Value = pXValue;
            x_ValueSpecified = (0 < x_Value);
        }
        #endregion Constructors
    }
    #endregion MatrixStructure

}
