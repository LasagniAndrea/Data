#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.IO;

#endregion using directives

namespace EfsML.Settlement
{
    #region AccountAt
    public class AccountAt
    {
        #region Members
        public int idA;
        public int idAAt;
        public string accountNumber;
        #endregion Members
        #region Accessors
        public bool IsValid
        {
            get { return ((0 < idA) && (0 < idAAt) && (StrFunc.IsFilled(accountNumber))); }
        }
        #endregion Accessors
        #region Constructors
        public AccountAt(int pIdA, int pIdAAt, string pAccountNumber)
        {
            idA = pIdA;
            idAAt = pIdAAt;
            accountNumber = pAccountNumber;
        }
        #endregion Constructors
    }
    #endregion AccountAt
    #region AccountInternal
    public class AccountInternal
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDB", Order = 1)]
        public int idB;
        [System.Xml.Serialization.XmlElementAttribute("IDA", Order = 2)]
        public int idA;
        [System.Xml.Serialization.XmlElementAttribute("IDC", Order = 3)]
        public string idC;
        [System.Xml.Serialization.XmlElementAttribute("ACCOUNTTYPE", Order = 4)]
        public string accountType;
        [System.Xml.Serialization.XmlElementAttribute("ISPPALACCOUNT", Order = 5)]
        public bool IsPPalAccount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ACCOUNTNUMBER", Order = 6)]
        public string cAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNumberIdentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ACCOUNTNUMBERIDENT", Order = 7)]
        public string cAccountNumberIdent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ACCOUNTNAME", Order = 8)]
        public string cAccountName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK", Order = 9)]
        public string extLink;
        #endregion Members
    }
    #endregion AccountInternal
    #region AccountInternals
    [System.Xml.Serialization.XmlRootAttribute("ACCOUNTINTERNALS", Namespace = "", IsNullable = true)]
    public class AccountInternals
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("ACCOUNTINTERNAL", Order = 1)]
        public AccountInternal[] accountInternal;
        #endregion Members
        #region Accessors
        public int Count
        {
            get
            {
                return ArrFunc.Count(accountInternal) ;
            }
        }
        #endregion Accessors
        #region Indexors
        public AccountInternal this[int pIndex]
        {
            get
            {
                return accountInternal[pIndex];
            }
        }
        #endregion Indexors
        #region Constructors
        public AccountInternals() { }
        #endregion Constructors
        #region Methods
        #region Initialize
        /// <summary>
        /// Chargement des Comptes 
        /// </summary>
        /// <remarks>
        /// 20090608 FI Add paramètre pIsSecAccount 
        /// Permet de charger les comptes titre ou espèces
        /// </remarks>
        /// <param name="pCs"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsSecAccount"></param>
        public void Initialize(string pCs, int pIdB, int pIdA, string pIdC, bool  pIsSecAccount)
        {
            accountInternal = null;
            AccountInternals col = AccountInternals.Load(pCs, pIdB, pIdA, pIdC, pIsSecAccount);
            if (null != col  && (col.Count > 0))
                accountInternal = col.accountInternal;
        }
        #endregion Initialize
        #region Load
        /// 20090608 FI Add paramètre pIsSecAccount 
        public static AccountInternals Load(string pCs, int pIdB, int pIdA, string pIdC, bool pIsSecAccount)
        {
            TextWriter writer = null;
            try
            {
                QueryParameters qryParameters = null;
                DataSet dsResult = null;
                AccountInternals ret = null;
                //
                #region A
                qryParameters = GetQuery(pCs, pIdB, pIdA, pIdC, pIsSecAccount,false);
                dsResult = DataHelper.ExecuteDataset(pCs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                #endregion A
                //
                #region B
                if (dsResult.Tables[0].Rows.Count == 0)
                {
                    qryParameters = GetQuery(pCs, pIdB, pIdA, pIdC, pIsSecAccount, true);
                    dsResult = DataHelper.ExecuteDataset(pCs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
                #endregion B
                //
                if (dsResult.Tables[0].Rows.Count>0)
                {
                    dsResult.DataSetName = "ACCOUNTINTERNALS";
                    DataTable dtTable = dsResult.Tables[0];
                    dtTable.TableName = "ACCOUNTINTERNAL";
                    //
                    string serializerResult = new DatasetSerializer(dsResult).Serialize();
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(AccountInternals), serializerResult);
                    ret = (AccountInternals)CacheSerializer.Deserialize(serializeInfo);
                }
                return ret;
            }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
        }
        #endregion Load
        #region GetQuery
        /// <summary>
        /// retourne la query de selection des comptes internes titre ou espèces
        /// <remarks>
        /// 20090608 FI [16603] Gestion du paramètre pIsSecAccount 
        /// </remarks>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsSecAccount"></param>
        /// <param name="pIsFindOnChid"></param>
        /// <returns></returns>
        private static QueryParameters GetQuery(string pCs, int pIdB, int pIdA, string pIdC, bool pIsSecAccount, bool pIsFindOnChid)
        {
            ///
            //Parametre pFindFindOnChid est né suite à la modification suivante 
            //20071212 Pl Si aucun, alors recherche sur un teneur (acteur) enfant de l'entité
            DataParameters parameters = new DataParameters();

            //select
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "ai.IDB,ai.IDA,ai.IDC,ai.ACCOUNTTYPE,ai.ISPPALACCOUNT,";
            sqlSelect += "ai.ACCOUNTNUMBER,ai.ACCOUNTNUMBERIDENT,ai.ACCOUNTNAME,ai.EXTLLINK" + Cst.CrLf;
            //From
            StrBuilder sqlFrom = new StrBuilder(SQLCst.FROM_DBO);
            sqlFrom += Cst.OTCml_TBL.ACCOUNTINTERNAL.ToString() + " ai" + Cst.CrLf;
            if (pIsFindOnChid)
            {
                sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on (ar.IDA=ai.IDA)";
                //sqlFrom += SQLCst.AND + "(ar.IDROLEACTOR=" + DataHelper.SQLString(RoleActor.xxxxxxxxxx) + ")" + Cst.CrLf;
                sqlFrom += SQLCst.AND + "(ar.IDA_ACTOR=@IDA)" + Cst.CrLf;
            }
            //Where
            StrBuilder sqlWhere = new StrBuilder(SQLCst.WHERE);
            sqlWhere += "ai.IDB=@IDB" + SQLCst.AND + "ai.IDA=@IDA" + Cst.CrLf;
            parameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDA), pIdA);
            parameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDB), pIdB);
            //
            if (StrFunc.IsFilled(pIdC))
            {
                parameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), pIdC);
                sqlWhere += SQLCst.AND + "((ai.IDC is null) or (ai.IDC=@IDC))" + Cst.CrLf;
            }
            else
            {
                sqlWhere += SQLCst.AND + "(ai.IDC is null)" + Cst.CrLf;
            }
            //
            if (false == pIsSecAccount) //compte Cash uniquement
                sqlWhere += SQLCst.AND + "((ai.CASHACCOUNTTYPE is not null) or ((ai.CASHACCOUNTTYPE is null) and (ai.SECACCOUNTTYPE is null)))" + Cst.CrLf;
            else
                sqlWhere += SQLCst.AND + "((ai.SECACCOUNTTYPE is not null))" + Cst.CrLf;
            //
            //Order by
            StrBuilder sqlOrder = new StrBuilder(SQLCst.ORDERBY);
            if (StrFunc.IsFilled(pIdC))
            {
                sqlOrder += DataHelper.SQLIsNull(pCs, "ai.IDC", DataHelper.SQLString("ZZZ"));
                sqlOrder += ",";
            }
            sqlOrder += "ISPPALACCOUNT";
            //
            QueryParameters ret = new QueryParameters(pCs, sqlSelect.ToString() + sqlFrom.ToString() + sqlWhere.ToString() + sqlOrder.ToString(), parameters);
            //
            return ret;
        }
        #endregion
        #endregion Methods
    }
    #endregion AccountInternals

    #region Issi
    public class Issi
    {
        [System.Xml.Serialization.XmlElementAttribute("IDISSI", Order = 1)]
        public int idIssi;
        //		
        [System.Xml.Serialization.XmlElementAttribute("IDA_STLOFFICE", Order = 2)]
        public int idA_stlOffice;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SOURCECODE", Order = 3)]
        public string sourceCode;
        //
        [System.Xml.Serialization.XmlElementAttribute("CASHSECURITIES", Order = 4)]
        public string cashSecurities;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priorityRankSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIORITYRANK", Order = 5)]
        public int priorityRank;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SIDE", Order = 6)]
        public string side;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC", Order = 7)]
        public string idC;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool countrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("COUNTRY", Order = 8)]
        public string country;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quotePlaceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEPLACE", Order = 9)]
        public string quotePlace;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAConterpartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_COUNTERPARTY", Order = 10)]
        public int idAConterparty;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB", Order = 11)]
        public int idB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cfiCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CFICODE", Order = 12)]
        public string cfiCode;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool productTypeCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRODUCTTYPECODE", Order = 13)]
        public string productTypeCode;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fiProductTypeCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FIPRODUCTTYPECODE", Order = 14)]
        public string fiProductTypeCode;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isinCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ISINCODE", Order = 15)]
        public string isinCode;
        //
    }
    #endregion Issi
    #region Issis
    [System.Xml.Serialization.XmlRootAttribute("ISSIS", Namespace = "", IsNullable = true)]
    public partial class Issis
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("ISSI", Order = 1)]
        public Issi[] issi;
        #endregion
    }
    #endregion Issis

    #region IssisContext
    /// <summary>
    /// Class qui représente une liste de issi qui matchent avec un context donné
    /// cette class effectue le tri pour retenir la meilleure issi
    /// </summary>
    public class IssisContext : IComparer
    {

        #region Members
        private readonly SQL_Event _sqlEvent;
        private readonly DataDocumentContainer _dataDocument;
        private readonly ProductContainer _productContainer;
        private readonly PayerReceiverEnum _payRec;
        //
        private readonly string _idC;
        private readonly string _country;
        // EG 20160404 Migration vs2013
        private readonly ISsiCriteria _issiCriteria = null;
        //spécifique aux ISSI securities
        readonly SecurityAssetContainer _securityAsset;
        readonly string _codeIsin; 
        //
        private Issi[] _issi;
        #endregion

        #region Accessors
        #region issi
        /// <summary>
        /// Liste des ISSI retenues en fonction du context
        /// <para>
        /// liste alimentée par la méthode LoadIssiCollection
        /// </para>
        /// </summary>
        public Issi[] Issi
        {
            get { return _issi; }
        }
        #endregion
        #endregion
        //
        #region Constructors
        public IssisContext(SQL_Event pSqlEvent, PayerReceiverEnum pPayerReceiver, ISsiCriteria pSsiCriteria, DataDocumentContainer pDataDocument)
        {
            //
            _sqlEvent = pSqlEvent;
            _dataDocument = pDataDocument;
            //
            _productContainer = (ProductContainer) _dataDocument.CurrentProduct.GetProduct(_sqlEvent.InstrumentNo_ID);
            if (null == _productContainer) //cas des OPP car non rattachés à un produit
                _productContainer = pDataDocument.CurrentProduct;
            //
            _payRec = pPayerReceiver;
            //Par défaut on retient les SI spécifié dans le SsiCriteria
            if ((null != pSsiCriteria) && pSsiCriteria.CountrySpecified)
                _country = pSsiCriteria.Country.Value;
            //
            if (_productContainer.IsDebtSecurityTransaction || _productContainer.IsRepo || _productContainer.IsBuyAndSellBack)
            {
                //
                _securityAsset = new SecurityAssetContainer(_productContainer.GetSecurityAssetPrincipal());
                //
                if (null != _securityAsset)
                {
                    _codeIsin = GetcodeIsin(pSqlEvent.CS);
                    //
                    //Pour les opérations sur titres on considère la devise du titre
                    //ceci pour les évènements matière comme pour les évènements espèces
                    _idC = _securityAsset.GetIdC(_productContainer.Product.ProductBase);  
                    //
                    if (StrFunc.IsEmpty(_country))
                        _country = _securityAsset.GetCountry();
                }
            }
            //par défaut on considère la devise du flux
            //Dans un contexte titre, la devise sera renseignée avec la devise du titre
            if (StrFunc.IsEmpty(_idC))
                _idC = _sqlEvent.Unit;
        }
        #endregion Constructors
        //
        #region Methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCssCriteria"></param>
        /// <param name="pIdAStlOffice"></param>
        /// <param name="pCashSecuritiesEnum"></param>
        /// <param name="pStlDtEventForced"></param>
        /// RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
        // EG 20180307 [23769] Gestion dbTransaction
        public void LoadIssiCollection(string pCs, ICssCriteria pCssCriteria, int pIdAStlOffice, CashSecuritiesEnum pCashSecuritiesEnum, DateTime pStlDtEventForced)
        {
            
                string cs = pCs;
                _issi = null;
                //
                DataParameters dataParameters = new DataParameters();
                //
                int idCounterparty = _sqlEvent.GetIdConterparty(_payRec);
                int idBook = _sqlEvent.GetIdBook(_payRec);
                int idI = 0;
                //
                //
                //IProduct instr = (IProduct)ReflectionTools.GetObjectById(_dataDocument.currentTrade,
                //    Cst.FpML_InstrumentNo + _sqlEvent.InstrumentNo.ToString());
                //if ((null != instr) && (null != instr.productBase.productType) && (0 != instr.productBase.productType.OTCmlId))
                //    idI = instr.productBase.productType.OTCmlId;
                //
                if ((null != _productContainer.Product.ProductBase.ProductType) && (0 != _productContainer.Product.ProductBase.ProductType.OTCmlId))
                    idI = _productContainer.Product.ProductBase.ProductType.OTCmlId;
                //
                // Attention l'ordre des champs doit respecter l'order des membres de ISSI
                string sql = SQLCst.SELECT + "issi.IDISSI,issi.IDA_STLOFFICE,issi.SOURCECODE,issi.CASHSECURITIES,issi.PRIORITYRANK," + Cst.CrLf;
                sql += "issi.SIDE,issi.IDC,issi.IDCOUNTRY,issi.QUOTEPLACE,issi.IDA_COUNTERPARTY,issi.IDB," + Cst.CrLf;
                sql += "issi.CFICODE,issi.PRODUCTTYPECODE,issi.FIPRODUCTTYPECODE,issi.ISINCODE" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ISSI.ToString() + " issi" + Cst.CrLf;
                //
                if (null != pCssCriteria)
                {
                    ICssCriteria cssCriteria = pCssCriteria;
                    if (cssCriteria.CssSpecified || cssCriteria.CssInfoSpecified)
                    {
                        dataParameters.Add(new DataParameter(cs, "CSS", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEACTOR_LEN), RoleActorSSI.CSS.ToString());
                        sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ISSIITEM.ToString() + " cssItem on cssItem.IDISSI=issi.IDISSI and cssItem.IDROLEACTOR=@CSS" + Cst.CrLf;
                        if (cssCriteria.CssSpecified)
                        {
                            ICss css = cssCriteria.Css;
                            dataParameters.Add(new DataParameter(cs, "CSSITEMIDA", DbType.Int32), css.OTCmlId);
                            sql += " and cssItem.IDA = @CSSITEMIDA";
                        }
                        else if (cssCriteria.CssInfoSpecified)
                        {
                            ICssInfo cssInfo = cssCriteria.CssInfo;
                            if (cssInfo.CountrySpecified)
                            {
                                dataParameters.Add(new DataParameter(cs, "IDCOUNTRY", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), cssInfo.Country.Value);
                                sql += " and css.IDCOUNTRY= @IDCOUNTRY" + Cst.CrLf;
                            }
                            if (cssInfo.PaymentTypeSpecified)
                            {
                                dataParameters.Add(new DataParameter(cs, "PAYMENTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), cssInfo.PaymentType.Value);
                                sql += " and css.PAYMENTTYPE= @PAYMENTTYPE" + Cst.CrLf;
                            }
                            if (cssInfo.SettlementTypeSpecified)
                            {
                                dataParameters.Add(new DataParameter(cs, "SETTLEMENTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), cssInfo.SettlementType.Value);
                                sql += " and css.SETTLEMENTTYPE= @SETTLEMENTTYPE" + Cst.CrLf;
                            }
                            if (cssInfo.SystemTypeSpecified)
                            {
                                dataParameters.Add(new DataParameter(cs, "SYSTEMTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), cssInfo.SystemType.Value);
                                sql += " and css.SYSTEMTYPE= @SYSTEMTYPE" + Cst.CrLf;
                            }
                            if (cssInfo.TypeSpecified)
                            {
                                dataParameters.Add(new DataParameter(cs, "CSSTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), cssInfo.Type.Value);
                                sql += " and css.CSSTYPE= @CSSTYPE" + Cst.CrLf;
                            }
                        }
                    }
                }
                //
                dataParameters.Add(new DataParameter(cs, "IDA_STLOFFICE", DbType.Int32), pIdAStlOffice);
                dataParameters.Add(new DataParameter(cs, "CASHSECURITIES", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pCashSecuritiesEnum.ToString());
                dataParameters.Add(new DataParameter(cs, "SIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), _payRec.ToString());
                dataParameters.Add(new DataParameter(cs, "IDA_CTR", DbType.Int32), idCounterparty);
                dataParameters.Add(new DataParameter(cs, "IDB", DbType.Int32), idBook);
                dataParameters.Add(new DataParameter(cs, "IDC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), _idC);
                //
                sql += SQLCst.WHERE + "issi.IDA_STLOFFICE=@IDA_STLOFFICE" + Cst.CrLf;
                sql += SQLCst.AND + "(issi.CASHSECURITIES=@CASHSECURITIES)" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.SIDE is null) or (issi.SIDE=@SIDE))" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.IDA_COUNTERPARTY is null) or (issi.IDA_COUNTERPARTY=@IDA_CTR))" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.IDB  is null) or (issi.IDB=@IDB))" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.IDC  is null) or (issi.IDC=@IDC))" + Cst.CrLf;
                // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
                sql += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(cs, "issi", pStlDtEventForced) + ")" + Cst.CrLf;
                //
                if (null != _issiCriteria)
                {
                    ISsiCriteria issiCriteria = _issiCriteria;
                    if (issiCriteria.CountrySpecified)
                    {
                        dataParameters.Add(new DataParameter(cs, "ISSSI_COUNTRY", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), issiCriteria.Country.Value);
                        sql += SQLCst.AND + "((issi.IDCOUNTRY is null) or (issi.IDCOUNTRY=@ISSSI_COUNTRY))" + Cst.CrLf;
                    }
                }
                dataParameters.Add(new DataParameter(cs, "ISINCODE", DbType.AnsiString, 16), GetcodeIsin(cs));
                dataParameters.Add(new DataParameter(cs, "PRODUCTTYPECODE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), GetProductTypeCode());
                dataParameters.Add(new DataParameter(cs, "FIPRODUCTTYPECODE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), GetfinancialInstrumentProductTypeCode());
                sql += SQLCst.AND + "((issi.ISINCODE is null) or (issi.ISINCODE=@ISINCODE))" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.PRODUCTTYPECODE is null) or (issi.PRODUCTTYPECODE=@PRODUCTTYPECODE))" + Cst.CrLf;
                sql += SQLCst.AND + "((issi.FIPRODUCTTYPECODE is null) or (issi.FIPRODUCTTYPECODE=@FIPRODUCTTYPECODE))" + Cst.CrLf;

                //IDI
                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(cs, null, idI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                string sqlWhereInstr = sqlInstrCriteria.GetSQLRestriction2("issi", RoleGInstr.STL);
                sql += SQLCst.AND + sqlWhereInstr;
                //IDI_UNL
                int idI_Unl = (null == _productContainer.GetUnderlyingAssetIdI()) ? 0 : (int)_productContainer.GetUnderlyingAssetIdI();
                sqlInstrCriteria = new SQLInstrCriteria(cs, null, idI_Unl, true, SQL_Table.ScanDataDtEnabledEnum.Yes);
                sqlWhereInstr = sqlInstrCriteria.GetSQLRestriction2("issi", RoleGInstr.STL);
                sql += SQLCst.AND + sqlWhereInstr;

                QueryParameters queryParameters = new QueryParameters(cs, sql, dataParameters);

                DataSet dsResult = DataHelper.ExecuteDataset(cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()); ;
                dsResult.DataSetName = "ISSIS";
                DataTable dtTable = dsResult.Tables[0];
                dtTable.TableName = "ISSI";
                //
                string serializerResult = new DatasetSerializer(dsResult).Serialize();
                //
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(Issis), serializerResult);
                Issis issis = (Issis)CacheSerializer.Deserialize(serializeInfo);
                _issi = issis.issi;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Sort()
        {

            bool isOk = (null != _sqlEvent) && (0 < ArrFunc.Count(_issi));
            if (isOk)
                Array.Sort(_issi, this);
            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IClassification GetClassificaton()
        {
            IClassification ret = null;
            //20090630 PL Bug
            if ((_securityAsset != null) && (_securityAsset.SecurityAsset!=null) 
                && _securityAsset.SecurityAsset.DebtSecuritySpecified)
            {
                if ((_securityAsset.SecurityAsset.DebtSecurity != null) && (_securityAsset.SecurityAsset.DebtSecurity.Security!=null) 
                    && _securityAsset.SecurityAsset.DebtSecurity.Security.ClassificationSpecified)
                    ret  = _securityAsset.SecurityAsset.DebtSecurity.Security.Classification;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCfiCode()
        {
            string ret = string.Empty;
            IClassification classification = GetClassificaton();
            if (null != classification)
            {
                if (classification.CfiCodeSpecified)
                    ret = classification.CfiCode.Value;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetProductTypeCode()
        {

            string ret = string.Empty;
            IClassification classification = GetClassificaton();
            if (null != classification)
            {
                if (classification.ProductTypeCodeSpecified)
                    ret = classification.ProductTypeCode.ToString();
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetfinancialInstrumentProductTypeCode()
        {
            string ret = string.Empty;
            IClassification classification = GetClassificaton();
            if (null != classification)
            {
                if (classification.FinancialInstrumentProductTypeCodeSpecified)
                    ret = classification.FinancialInstrumentProductTypeCode.ToString();
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        private string GetcodeIsin(string pCs)
        {
            string ret = string.Empty;
            if (null != _securityAsset)
                ret = _securityAsset.GetCodeIsin(pCs);
            return ret;
        }
        #endregion Methods

        #region IComparer Members
        public int Compare(object x, object y)
        {
            //
            //si ret = -1, issiX < issiY,  issiX est prioritaire prioritaire
            //si ret =  1, issiY < issiX,  issiY est prioritaire prioritaire
            int ret = 0;

            if ((x is Issi issiX) && (y is Issi issiY) && (null != _sqlEvent && _sqlEvent.IsLoaded))
            {
                #region ISIN
                if (ret == 0)
                {
                    if (StrFunc.IsFilled(_codeIsin))
                    {
                        //Priorite aux Issis de même code ISIN
                        //Priorite aux Issis de même book 
                        if (((issiX.isinCodeSpecified) && (issiX.isinCode == _codeIsin)) &&
                            ((!issiY.isinCodeSpecified) || ((issiY.isinCodeSpecified) && issiY.isinCode != _codeIsin))  //No isin Or different than _codeisin
                            )
                            ret = -1;
                        else if
                            (((issiY.isinCodeSpecified) && (issiY.isinCode == _codeIsin)) &&
                            ((!issiX.isinCodeSpecified) || ((issiX.isinCodeSpecified) && issiX.isinCode != _codeIsin)) //No isin Or different than _codeisin
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans code ISIN 
                        // Priorite aux issi sans book 
                        if (!issiX.isinCodeSpecified && issiY.isinCodeSpecified)
                            ret = -1;
                        else if (issiX.isinCodeSpecified && !issiY.isinCodeSpecified)
                            ret = 1;
                    }
                }
                #endregion ISIN
                #region BOOK
                if (ret == 0)
                {
                    int idBook = _sqlEvent.GetIdBook(_payRec);
                    if (idBook > 0)
                    {
                        //Priorite aux Issis de même book 
                        if (((issiX.idBSpecified) && (issiX.idB == idBook))
                            &&
                            ((!issiY.idBSpecified) || ((issiY.idBSpecified) && issiY.idB != idBook))  //No Book Or different than idBook
                            )
                            ret = -1;
                        else if
                            (((issiY.idBSpecified) && (issiY.idB == idBook))
                            &&
                            ((!issiX.idBSpecified) || ((issiX.idBSpecified) && issiX.idB != idBook)) //No Book Or different than idBook
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans book 
                        if (!issiX.idBSpecified && issiY.idBSpecified)
                            ret = -1;
                        else if (issiX.idBSpecified && !issiY.idBSpecified)
                            ret = 1;
                    }
                }
                #endregion BOOK
                #region IDCOUNTERPARTY
                if (ret == 0)
                {
                    int idAConterparty = _sqlEvent.GetIdConterparty(_payRec);
                    //Priorite aux Issis de même counterparty 
                    if (((issiX.idAConterpartySpecified) && (issiX.idAConterparty == idAConterparty))
                        &&
                        ((!issiY.idAConterpartySpecified) || ((issiY.idAConterpartySpecified) && issiY.idAConterparty != idAConterparty))  //No Counterparty Or different than idAConterparty
                        )
                        ret = -1;
                    else if
                        (((issiY.idAConterpartySpecified) && (issiY.idAConterparty == idAConterparty))
                        &&
                        ((!issiX.idAConterpartySpecified) || ((issiX.idAConterpartySpecified) && issiX.idAConterparty != idAConterparty)) //No Counterparty Or different than idAConterparty
                        )
                        ret = 1;
                }
                #endregion IDCOUNTERPARTY
                #region COUNTRY
                if (ret == 0)
                {
                    if (StrFunc.IsFilled(_country))
                    {
                        //Priorite aux Issis de même book 
                        if (((issiX.countrySpecified) && (issiX.country == _country))
                            &&
                            ((!issiY.countrySpecified) || ((issiY.countrySpecified) && issiY.country != _country))  //No Book Or different than idBook
                            )
                            ret = -1;
                        else if
                            (((issiY.countrySpecified) && (issiY.country == _country))
                            &&
                            ((!issiX.countrySpecified) || ((issiX.countrySpecified) && issiX.country != _country)) //No Book Or different than idBook
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans pays (= tous pays) 
                        if (!issiX.countrySpecified && issiY.countrySpecified)
                            ret = -1;
                        else if (issiX.countrySpecified && !issiY.countrySpecified)
                            ret = 1;
                    }
                }
                #endregion COUNTRY
                #region IDC
                if (ret == 0)
                {
                    string idC = _idC;
                    //Priorite aux Issis de même devise 
                    if (((issiX.idCSpecified) && (issiX.idC == idC))
                        &&
                        ((!issiY.idCSpecified) || ((issiY.idCSpecified) && issiY.idC != idC))  //No Idc Or different than IDC
                        )
                        ret = -1;
                    else if
                        (((issiY.idCSpecified) && (issiY.idC == idC))
                        &&
                        ((!issiX.idCSpecified) || ((issiX.idCSpecified) && issiX.idC != idC)) //No Idc Or different than IDC
                        )
                        ret = 1;
                }
                #endregion IDC

                #region CFICODE
                if (ret == 0)
                {
                    string cfiCode = GetCfiCode();
                    if (StrFunc.IsFilled(cfiCode))
                    {
                        //Priorite aux Issis de même code ISIN
                        if (((issiX.cfiCodeSpecified) && (issiX.cfiCode == cfiCode))
                            &&
                            ((!issiY.cfiCodeSpecified) || ((issiY.cfiCodeSpecified) && issiY.cfiCode != cfiCode))  //No CFICODE Or different than cfiCode
                            )
                            ret = -1;
                        else if
                            (((issiY.cfiCodeSpecified) && (issiY.cfiCode == cfiCode))
                            &&
                            ((!issiX.cfiCodeSpecified) || ((issiX.cfiCodeSpecified) && issiX.cfiCode != cfiCode)) //No CFICODE Or different than cfiCode
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans CFICODE
                        if (!issiX.cfiCodeSpecified && issiY.cfiCodeSpecified)
                            ret = -1;
                        else if (issiX.cfiCodeSpecified && !issiY.cfiCodeSpecified)
                            ret = 1;
                    }
                }
                #endregion CFICODE
                #region PRODUCTYPECODE
                if (ret == 0)
                {
                    string productTypeCode = GetProductTypeCode();
                    if (StrFunc.IsFilled(productTypeCode))
                    {
                        //Priorite aux Issis de même code productTypeCode
                        if (((issiX.productTypeCodeSpecified) && (issiX.productTypeCode == productTypeCode))
                            &&
                            ((!issiY.productTypeCodeSpecified) || ((issiY.productTypeCodeSpecified) && issiY.productTypeCode != productTypeCode))  //No productTypeCode Or different than productTypeCode
                            )
                            ret = -1;
                        else if
                            (((issiY.productTypeCodeSpecified) && (issiY.productTypeCode == productTypeCode))
                            &&
                            ((!issiX.productTypeCodeSpecified) || ((issiX.productTypeCodeSpecified) && issiX.productTypeCode != productTypeCode)) //No productTypeCode Or different than productTypeCode
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans productTypeCode
                        if (!issiX.productTypeCodeSpecified && issiY.productTypeCodeSpecified)
                            ret = -1;
                        else if (issiX.productTypeCodeSpecified && !issiY.productTypeCodeSpecified)
                            ret = 1;
                    }
                }
                #endregion PRODUCTYPECODE
                #region FIPRODUCTYPECODE
                if (ret == 0)
                {
                    string fiProductTypeCode = GetfinancialInstrumentProductTypeCode();
                    if (StrFunc.IsFilled(fiProductTypeCode))
                    {
                        //Priorite aux Issis de même code productTypeCode
                        if (((issiX.fiProductTypeCodeSpecified) && (issiX.fiProductTypeCode == fiProductTypeCode))
                            &&
                            ((!issiY.fiProductTypeCodeSpecified) || ((issiY.fiProductTypeCodeSpecified) && issiY.fiProductTypeCode != fiProductTypeCode))  //No fiProductTypeCode Or different than fiProductTypeCode
                            )
                            ret = -1;
                        else if
                            (((issiY.fiProductTypeCodeSpecified) && (issiY.fiProductTypeCode == fiProductTypeCode))
                            &&
                            ((!issiX.fiProductTypeCodeSpecified) || ((issiX.fiProductTypeCodeSpecified) && issiX.fiProductTypeCode != fiProductTypeCode)) //No fiProductTypeCode Or different than fiProductTypeCode
                            )
                            ret = 1;
                    }
                    else
                    {
                        // Priorite aux issi sans productTypeCode
                        if (!issiX.fiProductTypeCodeSpecified && issiY.fiProductTypeCodeSpecified)
                            ret = -1;
                        else if (issiX.fiProductTypeCodeSpecified && !issiY.fiProductTypeCodeSpecified)
                            ret = 1;
                    }
                }
                #endregion FIPRODUCTYPECODE


                #region SIDE
                if (ret == 0)
                {
                    string side = _payRec.ToString();
                    //Priorite aux Issis de même book 
                    if (((issiX.sideSpecified) && (issiX.side == side))
                        &&
                        ((!issiY.sideSpecified) || ((issiY.sideSpecified) && issiY.side != side))  //No Book Or different than idBook
                        )
                        ret = -1;
                    else if
                        (((issiY.sideSpecified) && (issiY.side == side))
                        &&
                        ((!issiX.sideSpecified) || ((issiX.sideSpecified) && issiX.side != side)) //No Book Or different than idBook
                        )
                        ret = 1;
                }
                #endregion SIDE

                #region PRIORITYRANK
                //En cas d'égalité priorite en fonction de priorityRank
                if (ret == 0)
                {
                    if (issiX.priorityRank < issiY.priorityRank)
                        ret = -1;
                    else if (issiX.priorityRank > issiY.priorityRank)
                        ret = 1;
                }
                #endregion PRIORITYRANK
            }
            else
                throw new ArgumentException("object is not a ISSI");

            return ret;
        }
        #endregion IComparer Members
    }
    #endregion IssisContext

    #region IssiItem
    public partial class IssiItem 
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDISSI", Order = 1)]
        public int idIssi;
        [System.Xml.Serialization.XmlElementAttribute("IDA", Order = 2)]
        public int idA;
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR", Order = 3)]
        public EFS.Actor.RoleActorSSI idRoleActor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sequenceNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SEQUENCENO", Order = 4)]
        public int sequenceNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool chainPartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("CHAINPARTY", Order = 5)]
        public string chainParty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPartyRoleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PARTYROLE", Order = 6)]
        public int idPartyRole;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CACCOUNTNUMBER", Order = 7)]
        public string cAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNumberIdentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CACCOUNTNUMBERIDENT", Order = 8)]
        public string cAccountNumberIdent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cAccountNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CACCOUNTNAME", Order = 9)]
        public string cAccountName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sAccountNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SACCOUNTNUMBER", Order = 10)]
        public string sAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sAccountNumberIdentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SACCOUNTNUMBERIDENT", Order = 11)]
        public string sAccountNumberIdent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sAccountNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SACCOUNTNAME", Order = 12)]
        public string sAccountName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK", Order = 13)]
        public string extLink;
        #endregion Members
    }
    #endregion IssiItem
    #region IssiItems
    [System.Xml.Serialization.XmlRootAttribute("ISSIITEMS", Namespace = "", IsNullable = true)]
    public partial class IssiItems
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("ISSIITEM", Order = 1)]
        public IssiItem[] issiItem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idIssi;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idCss;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string source;
        #endregion Members
    }
    #endregion

    #region SettlementRoutingActorsBuilder
    public class SettlementRoutingActorsBuilder : RoutingActorsBuilder
    {
        #region Members
        /// <summary>
        /// Rpésente une chambre de compensation
        /// </summary>
        protected int idCss;
        protected SQL_Css sqlCss;


        #endregion Members

        #region Accessors
        public bool ExistCss
        {
            get
            {
                return (idCss != 0);
            }
        }
        public override bool IsAddAddress
        {
            get
            {
                return true ;
            }
        }
        public override bool IsAddPhone
        {
            get
            {
                return false;
            }
        }
        public override bool IsAddWeb
        {
            get
            {
                return false;
            }
        }
        

        #endregion Accessors
        
        #region constructor
        public SettlementRoutingActorsBuilder(string pCS, int pIdCss, IRoutingCreateElement pRoutingCreateElement)
            : base(pRoutingCreateElement)
        {
            idCss = pIdCss;
            if (ExistCss)
            {
                sqlCss = new SQL_Css(pCS, pIdCss);
                sqlCss.LoadTable(new string[1] { "*" });
            }
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pListIdA"></param>
        public override void Load(string pCS, int[] pListIdA)
        {

            if (ExistCss)
            {
                // ACTOR ( INTERMEDIARE, ACCOUNTOWNER, Etc ....)
                StrBuilder sql = new StrBuilder(string.Empty);
                sql += SQLCst.SELECT + "a.IDA,a.IDENTIFIER,a.BIC,a.LTCODE,a.DISPLAYNAME,cssm.CSSMEMBERIDENT,a.DESCRIPTION," + Cst.CrLf;
                sql += "a.ADDRESS1,a.ADDRESS2,a.ADDRESS3,a.ADDRESS4," + Cst.CrLf;
                sql += "a.ADDRESSPOSTALCODE,a.ADDRESSCITY,a.ADDRESSSTATE,a.ADDRESSCOUNTRY" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
                sql += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.CSMID.ToString() + " cssm on cssm.IDA=a.IDA" + Cst.CrLf;
                if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                    sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "cssm").ToString() + Cst.CrLf;
                sql += SQLCst.AND + "cssm.IDA_CSS = " + idCss.ToString() + Cst.CrLf;

                sql += SQLCst.WHERE + "(" + DataHelper.SQLColumnIn(pCS, "a.IDA", pListIdA, TypeData.TypeDataEnum.integer) + ")";
                if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                    sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a").ToString();

                DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sql.ToString());
                dt = ds.Tables[0];
            }
            else
            {
                base.Load(pCS, pListIdA);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIda"></param>
        /// <returns></returns>
        public override IRoutingIdsAndExplicitDetails GetRoutingIdsAndExplicitDetails( int pIda)
        {
            IRoutingIdsAndExplicitDetails ret = base.GetRoutingIdsAndExplicitDetails( pIda);
            if (IsActorCss(pIda))
            {
                if (sqlCss.IsLoaded)
                {
                    IRoutingId rId;
                    if (StrFunc.IsFilled(sqlCss.PhysicalAddr))
                    {
                        string scheme = Cst.OTCml_CssPhysicalAddressScheme;
                        if (StrFunc.IsFilled(sqlCss.PhysicalAddrIdent))
                            scheme += "_" + sqlCss.PhysicalAddrIdent;
                        rId = routingCreateElement.CreateRoutingId();
                        rId.Value = sqlCss.PhysicalAddr;
                        rId.RoutingIdCodeScheme = scheme;

                        ReflectionTools.AddItemInArray(ret.RoutingIds[0], "routingId", 0);
                        ret.RoutingIds[0].RoutingId[ArrFunc.Count(ret.RoutingIds[0].RoutingId) - 1] = rId;
                    }
                    if (StrFunc.IsFilled(sqlCss.LogicalAddr))
                    {
                        string scheme = Cst.OTCml_CssLogicalAddressScheme;
                        if (StrFunc.IsFilled(sqlCss.LogicalAddrIdent))
                            scheme += "_" + sqlCss.LogicalAddrIdent;
                        rId = routingCreateElement.CreateRoutingId();
                        rId.Value = sqlCss.LogicalAddr;
                        rId.RoutingIdCodeScheme = scheme;

                        ReflectionTools.AddItemInArray(ret.RoutingIds[0], "routingId", 0);
                        ret.RoutingIds[0].RoutingId[ArrFunc.Count(ret.RoutingIds[0].RoutingId) - 1] = rId;
                    }
                }
            }
            return ret;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        protected bool IsActorCss(int pIdA)
        {
            return (pIdA == idCss);
        }
        #endregion

    }
    #endregion

    #region SettlementCst
    public sealed class SettlementCst
    {
        public const string SSILocalDatabase = ".";
        public const string CssBOOK = "BOOK";
        public SettlementCst()
        { }
    }
    #endregion SettlementCst
    #region SettlementOffices
    public class SettlementOffices : OfficesBase
    {
        #region Accessor
        public override string BookColumn
        {
            get
            {
                return "IDA_STLOFFICE";
            }
        }
        /// <summary>
        /// Obtient <see cref="RoleActor.SETTLTOFFICE"/>
        /// </summary>
        /// FI 20240218 [WI838] Add
        public override RoleActor RoleActor
        {
            get
            {
                return RoleActor.SETTLTOFFICE;
            }
        }


        /// <summary>
        /// Obtient les types de Rôle qui doivent être exclus (à savoir <see cref="RoleType.ACCESS"/>, <see cref="RoleType.COLLABORATION"/>, <see cref="RoleType.COLLABORATION_ALGORITHM"/>).
        /// </summary>
        /// FI 20240218 [WI838] Add
        public override RoleType[] RoleTypeExclude
        {

            get { return new RoleType[] { RoleType.ACCESS, RoleType.COLLABORATION, RoleType.COLLABORATION_ALGORITHM }; }
        }

        #endregion Accessor
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// FI 20240218 [WI838] Seul ce constructeur est conservé
        public SettlementOffices(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
            : base(pSource, pDbTransaction, pIdA, pIdB) {}
        #endregion Constructors
    }
    #endregion SettlementOffices

    #region SsiDbs
    /// <summary>
    /// Load Info of SsiDbs
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("SSIDBS", Namespace = "", IsNullable = true)]
    public partial class SsiDbs
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("SSIDB", Order = 1)]
        public SsiDb[] ssidb;
        #endregion Members

        #region Constructors
        public SsiDbs() { }
        #endregion Constructors
    }
    #endregion
    #region SsiDb
    public partial class SsiDb
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDSSIDB", Order = 1)]
        public int idssiDb;
        //		
        [System.Xml.Serialization.XmlElementAttribute("IDA_STLOFFICE", Order = 2)]
        public int idAStlOffice;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DESCRIPTION", Order = 3)]
        public string description;
        //
        [System.Xml.Serialization.XmlElementAttribute("DBTYPE", Order = 4)]
        public string dbType;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idACustodianSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_CUSTODIAN", Order = 5)]
        public int idACustodian;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priorityRankSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIORITYRANK", Order = 6)]
        public int priorityRank;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool codeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CODE", Order = 7)]
        public EfsML.Enum.SettlementStandingInstructionDatabase1CodeEnum code;
        //
        [System.Xml.Serialization.XmlElementAttribute("URL", Order = 8)]
        public string url;
        //
        [System.Xml.Serialization.XmlElementAttribute("IDSSIFORMATREQUEST", Order = 9)]
        public string idformatRequest;
        //
        [System.Xml.Serialization.XmlElementAttribute("IDSSIFORMATANSWER", Order = 10)]
        public string idformatAnswer;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK", Order = 11)]
        public string extLink;
        #endregion Members
    }
    #endregion SsiDb

    

}