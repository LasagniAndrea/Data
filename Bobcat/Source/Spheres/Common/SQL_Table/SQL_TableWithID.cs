#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Restriction;
using EFS.Status;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#endregion Using Directives

namespace EFS.Common
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class SQL_TableWithID : SQL_Table
    {
        public enum IDType
        {
            Id,
            Identifier,
            /// <summary>
            /// A utiliser sur les référentiels où il existe une autre clef unique basée sur une colonne autre que la colonne IDENTIFIER
            /// </summary>
            Identifier2,
            Displayname,
            Description,
            ExtLink,
            ExtlId,
            IsinCode,
            ISO10383_ALPHA4,//PL 20130208 New
            FIXML_SecurityExchange,//PL 20130208 New
            /// <summary>
            /// 
            /// </summary>
            /// FI 20200114 [XXXXX] Add
            SHORT_ACRONYM,
            AssetSymbol,
            RICCode,
            BBGCode,
            NSINCode,
            UNDEFINED,
            CurrencyPair, // EG 20150313 New 
        }
        #region StringForPERCENT
        /// <summary>
        /// Constante à utiliser à la place d'un % lorsque SQL_TABLE doit générer un ordre select avec une égalité  
        /// Exemple select ... where IDENTIFIER = '%'
        /// Rappel : par défaut s'il existe un identifier avec % alors SQL_TABLE génère un ordre select avec un like  
        /// select ... where IDENTIFIER like '%' ORDER by IDENTIFIER
        /// </summary>
        public const string StringForPERCENT = "$~EFS~PERCENT~$";

        #endregion

        #region private variable
        private int _id_In;
        private string _identifier_In;
        private string _displayname_In;
        private string _description_In;
        private string _extLink_In;
        #endregion private variable

        #region constructors
        /// <summary>
        /// Constructeur qui permet de passer directement un jeu de résultat. 
        /// <para>Attention les noms de colonnes du jeu de résultat doivent coincider avec ceux attendus dans les properties</para>
        /// </summary>
        /// FI 20131223 [19337] add Constructor
        public SQL_TableWithID(DataTable pDt)
            : base(pDt)
        {
        }

        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable)
            : this(pSource, pTable, IDType.UNDEFINED, string.Empty, ScanDataDtEnabledEnum.No)
        { }
        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable, int pId)
            : this(pSource, pTable, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No)
        { }
        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable, int pId, bool pIsScanDataEnabled)
            : this(pSource, pTable, IDType.Id, pId.ToString(), pIsScanDataEnabled ? ScanDataDtEnabledEnum.Yes : ScanDataDtEnabledEnum.No)
        { }
        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier)
            : this(pSource, pTable, pIdType, pIdentifier, ScanDataDtEnabledEnum.No)
        { }
        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pTable, pIdType, pIdentifier, pScanDataEnabled, DateTime.MinValue)
        { }
        public SQL_TableWithID(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, DateTime pDtRefForDataEnabled)
            : base(pSource, null, pTable.ToString(), pScanDataEnabled, pDtRefForDataEnabled)
        {
            //PL 20130604 New constructor with pDtRefForDtEnabled (Vu)
            switch (pIdType)
            {
                case IDType.Id:
                    // EG 20100402 
                    if (StrFunc.IsEmpty(pIdentifier))
                    {
                        pIdentifier = "-1";
                    }
                    Id_In = int.Parse(pIdentifier);
                    break;
                case IDType.Identifier:
                    Identifier_In = pIdentifier;
                    break;
                case IDType.Displayname:
                    Displayname_In = pIdentifier;
                    break;
                case IDType.Description:
                    Description_In = pIdentifier;
                    break;
                case IDType.ExtLink:
                    //20090617 PL Bug 
                    //Identifier_In = pIdentifier;
                    ExtLink_In = pIdentifier;
                    break;
            }

            if (this.IsUseOrderBy)
            {
                //20090724 FI SetCacheOn
                base.EfsObject = new SQL_EfsObject(CSTools.SetCacheOn(_csManager.Cs), pTable.ToString());
                //base.EfsObject.LoadTable(); 
                //20090703 PL Pour éviter un "select *"
                base.EfsObject.LoadTable(new string[] { "ISWITHSTATISTIC" });
            }
        }
        #endregion constructors

        #region public_property_get
        protected virtual bool IsUseOrderBy
        {
            get
            {
                //Critère sur la PK ou
                //Critère sur Identifier ou ExtLink sans "%" 
                //--> La query retournera 1 ou 0 record  
                return (
                    (Id_In == 0)
                    && (
                        StrFunc.ContainsIn(Identifier_In, "%")
                        || StrFunc.ContainsIn(Displayname_In, "%")
                        || StrFunc.ContainsIn(Description_In, "%")
                        || StrFunc.ContainsIn(ExtLink_In, "%")
                        )
                        );
            }
        }

        public override string Key
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }
        public virtual int Id
        {
            get
            {
                string sColumId = OTCmlHelper.GetColunmID(SQLObject);
                //
                if (StrFunc.IsFilled(sColumId))
                    return Convert.ToInt32(GetFirstRowColumnValue(sColumId));
                else
                    return 0;
            }
        }

        /// <summary>
        /// Obtient la valeur de la colonne IDENTIFIER 
        /// </summary>
        public virtual string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }
        /// <summary>
        /// Obtient la valeur de la colonne DISPLAYNAME 
        /// </summary>
        public virtual string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }
        /// <summary>
        /// Obtient la valeur de la colonne DESCRIPTION 
        /// </summary>
        public virtual string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }
        /// <summary>
        /// Obtient la valeur de la colonne EXTLLINK 
        /// </summary>
        public virtual string ExtlLink
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXTLLINK")); }
        }
        /// <summary>
        /// Obtient la valeur de la colonne EXTLLINK2 
        /// </summary>
        // FI 20140519 [19923] add ExtlLink2
        public virtual string ExtlLink2
        {
            get
            {
                string ret = string.Empty;
                if (Dt.Columns.Contains("EXTLLINK2"))
                    ret = Convert.ToString(GetFirstRowColumnValue("EXTLLINK2"));
                return ret;
            }
        }
        /// <summary>
        /// Obtient la valeur de la colonne CSSFILENAME 
        /// </summary>
        public virtual string CSSFileName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSFILENAME")); }
        }

        #endregion

        #region public_property_get_set
        public int Id_In
        {
            get { return _id_In; }
            set
            {
                if (_id_In != value)
                {
                    InitProperty(true);
                    _id_In = value;
                }
            }
        }
        public string Identifier_In
        {
            get { return _identifier_In; }
            set
            {
                if (_identifier_In != value)
                {
                    InitProperty(true);
                    _identifier_In = value;
                }
            }
        }
        public string Displayname_In
        {
            get { return _displayname_In; }
            set
            {
                if (_displayname_In != value)
                {
                    InitProperty(true);
                    _displayname_In = value;
                }
            }
        }
        public string Description_In
        {
            get { return _description_In; }
            set
            {
                if (_description_In != value)
                {
                    InitProperty(true);
                    _description_In = value;
                }
            }
        }
        public string ExtLink_In
        {
            get { return _extLink_In; }
            set
            {
                if (_extLink_In != value)
                {
                    InitProperty(true);
                    _extLink_In = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected virtual void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _id_In = 0;
                _identifier_In = null;
                _displayname_In = null;
                _description_In = null;
                _extLink_In = null;
            }
            base.InitProperty();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            this.SetSQLWhere(string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLWhere"></param>
        /// FI 20121116 [18224][18280] appel à GetWhereIDType
        protected override void SetSQLWhere(string pSQLWhere)
        {
            SQLWhere sqlWhere;
            if (StrFunc.IsFilled(pSQLWhere))
            {
                sqlWhere = new SQLWhere();
                sqlWhere.Append(pSQLWhere);
            }
            else
            {
                sqlWhere = GetWhereIDType();
            }
            InitSQLWhere(sqlWhere.ToString(), false);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLOrderBy()
        {
            string sqlColumOrderBy = string.Empty;
            string orderBy = string.Empty;
            //
            if (this.IsUseOrderBy)
            {
                if (Identifier_In != null)
                {
                    //Critère like sur Identifier --> La query peut retourner n records --> Order by sur IDENTIFIER
                    sqlColumOrderBy = SQLObject + ".IDENTIFIER";
                }
                else if (Displayname_In != null)
                {
                    sqlColumOrderBy = SQLObject + ".DISPLAYNAME";
                }
                else if (Description_In != null)
                {
                    sqlColumOrderBy = SQLObject + ".DESCRIPTION";
                }
                else if (ExtLink_In != null)
                {
                    //Critère like sur Identifier --> La query peut retourner n records --> Order by sur EXTLLINK
                    sqlColumOrderBy = SQLObject + ".EXTLLINK";
                }

                //Statistics
                if ((null != EfsObject) && EfsObject.IsWithStatistic)
                    orderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, SQLObject, sqlColumOrderBy);
                else
                    orderBy = sqlColumOrderBy;
            }
            //
            ConstructOrderBy(orderBy);

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            //Statistics: 
            string sqlStatFrom = string.Empty;
            if (IsUseOrderBy)
            {
                if ((null != EfsObject) && EfsObject.IsWithStatistic)
                    sqlStatFrom = Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(SQLObject, SQLObject);
            }
            //
            ConstructFrom(sqlStatFrom);
        }

        /// <summary>
        /// Retourne La query de chargement
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(string[] pCol)
        {
            QueryParameters ret = base.GetQueryParameters(pCol);

            if (null != ret.Parameters && ret.Parameters.Count > 0)
            {
                // FI 20180906 [24159] nouvelle écriture basé sur Linq => plus performant
                //for (int i = 0; i < ArrFunc.Count(ret.Parameters.parameter); i++)
                //{
                //    if (ret.Parameters.parameter[i].Value.GetType().Equals(typeof(String)))
                //        ret.Parameters.parameter[i].Value = ret.Parameters.parameter[i].Value.ToString().Replace(StringForPERCENT, "%"); ;
                //}

                var sparameters = from item in ret.Parameters.Parameter.Where(x => (
                  x.DbType == DbType.AnsiString || x.DbType == DbType.AnsiStringFixedLength ||
                  x.DbType == DbType.String || x.DbType == DbType.StringFixedLength) && x.Value.ToString().Contains(StringForPERCENT))
                                  select item;

                foreach (DataParameter item in sparameters)
                    item.Value = item.Value.ToString().Replace(StringForPERCENT, "%");

            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20121116 [18224][18280] add GetWhereIDType
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected virtual SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();

            if (_id_In != 0)
            {
                sqlWhere.Append(SQLObject + "." + OTCmlHelper.GetColunmID(SQLObject) + "=@ID");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.ID), _id_In);
            }
            else
            {

                string data_In = null;
                DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;

                if (_identifier_In != null)
                {
                    column_In = DataParameter.ParameterEnum.IDENTIFIER;
                    data_In = _identifier_In;
                }
                else if (_displayname_In != null)
                {
                    column_In = DataParameter.ParameterEnum.DISPLAYNAME;
                    data_In = _displayname_In;
                }
                else if (_description_In != null)
                {
                    column_In = DataParameter.ParameterEnum.DESCRIPTION;
                    data_In = _description_In;
                }
                else if (_extLink_In != null)
                {
                    column_In = DataParameter.ParameterEnum.EXTLLINK;
                    data_In = _extLink_In;
                }

                if (data_In != null)
                {
                    DataParameter dp = DataParameter.GetParameter(CS, column_In);
                    Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                    string column = SQLObject + "." + column_In.ToString();
                    if (isCaseInsensitive)
                    {
                        column = DataHelper.SQLUpper(CS, column);
                        data_In = data_In.ToUpper();
                    }

                    //Critère sur donnée String
                    if (IsUseOrderBy)
                        sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                    else
                        sqlWhere.Append(column + "=@" + dp.ParameterKey);

                    SetDataParameter(dp, data_In);
                }
            }

            return sqlWhere;
        }

        /// <summary>
        /// Retourne true si la recherche sur la donnée {pData} doit être non sensible à la casse 
        /// <para>Lecture du paramètre Collation présent dans le fichier de config</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20180906 [24159] Add Method
        protected Boolean IsUseCaseInsensitive(string pData)
        {
            Boolean ret = SystemSettings.IsCollationCI();
            if (ret)
            {
                Boolean isInteger = new Regex(@"^\d+$").IsMatch(pData);
                ret = (false == isInteger);
            }
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_TableWithIDAndInstrCriteria : SQL_TableWithID
    {
        #region constructors
        public SQL_TableWithIDAndInstrCriteria(string pSource, Cst.OTCml_TBL pTable, int pId)
            : this(pSource, pTable, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_TableWithIDAndInstrCriteria(string pSource, Cst.OTCml_TBL pTable, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pTable, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_TableWithIDAndInstrCriteria(string pSource, Cst.OTCml_TBL pTable, string pIdentifier)
            : this(pSource, pTable, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_TableWithIDAndInstrCriteria(string pSource, Cst.OTCml_TBL pTable, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pTable, SQL_TableWithID.IDType.Identifier, pIdentifier, pScanDataEnabled) { }
        public SQL_TableWithIDAndInstrCriteria(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            :
            base(pSource, pTable, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
        //
        #region properties
        public int IdP
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDP")); }
        }
        public string GProduct
        {
            get { return Convert.ToString(GetFirstRowColumnValue("GPRODUCT")); }
        }
        public int IdI
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }
        public Nullable<int> IdGInstr
        {
            get
            {
                if (GetFirstRowColumnValue("IDGINSTR") == Convert.DBNull)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("IDGINSTR"));
            }
        }
        public bool IsInfoInstrSpecified
        {
            get
            {
                return (GProductSpecified || IdPSpecified || IdGInstrSpecified || IdISpecified);
            }
        }
        public bool IdISpecified
        {
            get
            {
                return (IdI > 0);
            }
        }
        public bool IdGInstrSpecified
        {
            get
            {
                return (IntFunc.IsFilledAndNoZero(IdGInstr));
            }
        }
        public bool IdPSpecified
        {
            get
            {
                return (IdP > 0);
            }
        }
        public bool GProductSpecified
        {
            get
            {
                return (StrFunc.IsFilled(GProduct));
            }
        }
        #endregion Properties
        //
    }

    
    /// <summary>
    /// Get an Actor
    /// </summary>
    /// <returns>void</returns>
    public class SQL_Actor : SQL_TableWithID
    {
        #region Members
        #region private _withRole_In
        /// <summary>
        /// Liste de rôle nécessaire (un des rôle est nécessaire)
        /// <para>
        /// Lorsque renseignés l'acteur doit avoir l'un des rôles de _withRole_In 
        /// </para>
        /// </summary>
        private RoleActor[] _withRole_In;
        #endregion

        private bool _withInfoEntity;
        private bool _withInfoCapital;

        /// <summary>
        /// Application des restrictions vis à vis des acteurs
        /// <para>Si oui, il est nécessaire de reseigner _sessionId</para>
        /// </summary>
        private RestrictEnum _restrictActor;

        /// <summary>
        /// Représente un User (utilisé pour les restrictions uniquement)
        /// </summary>
        private readonly User _user;

        /// <summary>
        /// Représente l'identifiant de la session du User
        /// </summary>
        private readonly string _sessionId;

        /// <summary>
        /// 
        /// </summary>
        private string _xmlId;

        /// <summary>
        /// Utilisé pour restriction sur un groupe de ACTOR
        /// </summary>
        /// FI 20141230 [20616] Add
        Nullable<int> _idGActor;

        #endregion private variable

        /// <summary>
        ///  Utilisé pour restriction sur un groupe de ACTOR
        /// </summary>
        /// FI 20141230 [20616] Add
        public Nullable<int> IdGActor
        {
            get { return _idGActor; }
            set
            {
                if (_idGActor != value)
                {
                    InitProperty(false);
                    _idGActor = value;
                }
            }
        }


        #region constructor
        //Use int ident
        public SQL_Actor(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }
        public SQL_Actor(string pSource, int pId, RestrictEnum pRestrictActor, ScanDataDtEnabledEnum pScanDataEnabled, User pUser, string pSessionId)
            : this(pSource, IDType.Id, pId.ToString(), pRestrictActor, pScanDataEnabled, pUser, pSessionId) { }

        /// <summary>
        /// Use string ident
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_Actor(string pSource, string pIdentifier)
            : this(pSource, IDType.Identifier, pIdentifier, RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }

        /// <summary>
        /// Use string ident
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_Actor(string pSource, string pIdentifier, RestrictEnum pRestrictActor, ScanDataDtEnabledEnum pScanDataEnabled, User pUser, string pSessionId)
            : this(pSource, IDType.Identifier, pIdentifier, pRestrictActor, pScanDataEnabled, pUser, pSessionId) { }

        /// <summary>
        /// Chagement d'un acteur
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrictActor"></param>
        /// <param name="pScanDataEnabled"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// FI 20141107 [20441] Modification de signature
        public SQL_Actor(string pSource, IDType pIdType, string pIdentifier, RestrictEnum pRestrictActor, ScanDataDtEnabledEnum pScanDataEnabled, User pUser, string pSessionId)
            :
            base(pSource, Cst.OTCml_TBL.ACTOR, pIdType, pIdentifier, pScanDataEnabled)
        {
            _withRole_In = new RoleActor[] {};

            _restrictActor = pRestrictActor;

            _user = pUser;
            _sessionId = pSessionId;
            if (null == _user)
                _user = new User(1, null, RoleActor.SYSADMIN);
        }
        #endregion constructor

        #region public_property_get
        public string LTAddress
        {
            get
            {
                return ActorTools.GetLTAdress(Convert.ToString(GetFirstRowColumnValue("BIC")),
                    Convert.ToString(GetFirstRowColumnValue("LTCODE")));
            }
        }
        /// <summary>
        /// BIC or XXXXXXXXXXX 
        /// </summary>
        public string BIC
        {
            get
            {
                string ret = Convert.ToString(GetFirstRowColumnValue("BIC"));
                if (StrFunc.IsEmpty(ret))
                    ret = "XXXXXXXXXXX";
                return ret;
            }
        }
        /// <summary>
        /// Obtient ou définie un Id (supposé être unique et qui respecte le type ID de DTD)
        /// </summary>
        /// FI 20170928 [23452] add summary
        public string XmlId
        {
            get
            {
                if (StrFunc.IsEmpty(_xmlId))
                    _xmlId = GetXmlId();
                return _xmlId;
            }
            set
            {
                _xmlId = value;
            }
        }
        /// <summary>
        /// Obtient le BIC de l'acteur ou son identifier (si le BIC est non renseigné)
        /// </summary>
        /// <returns></returns>
        /// FI 20170928 [23452] add summary
        private string GetXmlId()
        {
            string ret = Convert.ToString(GetFirstRowColumnValue("BIC"));
            if (StrFunc.IsEmpty(ret))
                ret = Convert.ToString(GetFirstRowColumnValue("IDENTIFIER"));
            
            ret = XMLTools.GetXmlId(ret);
            
            return ret;
        }

        public bool IsEntityExist
        {
            get
            {
                if (GetFirstRowColumnValue("IDA1") == null)
                    return false;
                else
                    return true;
            }
        }

        public string IdMarketEnv
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDMARKETENV"));
            }
        }
        public string IdMarketEnv_Client
        {
            get
            {
                string ret = Convert.ToString(GetFirstRowColumnValue("IDMARKETENV_CLIENT"));
                if (StrFunc.IsEmpty(ret))
                {
                    //Aucun environnement clien --> on retourne l'environnment officiel
                    ret = IdMarketEnv;
                }
                return ret;
            }
        }

        public string IdCAccount
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDCACCOUNT"));
            }
        }

        public string IdBCAccount
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDBCACCOUNT"));
            }
        }

        public string IdCCapital
        {
            get
            {
                //PL 20120625
                //return Convert.ToString(GetFirstRowColumnValue("IDCCAPITAL"));
                return Convert.ToString(GetFirstRowColumnValue("IDC"));
            }
        }
        public decimal Capital
        {
            get
            {
                //PL 20120625
                //return Convert.ToDecimal(GetFirstRowColumnValue("CAPITAL"));
                return Convert.ToDecimal(GetFirstRowColumnValue("AMOUNT"));
            }
        }

        public string Mail
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("MAIL"));
            }
        }
        public string Address1
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ADDRESS1"));
            }
        }

        public string Address2
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ADDRESS2"));
            }
        }
        //
        // 20090420 RD : Utilisé pour enrichir les Parties dans le flux XML des confirm
        // voir: DataDocumentContainer.SetAdditionalInfoOnParty()
        //
        public string BICorNull
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("BIC"));
            }
        }
        public string BusinessNumber
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("BUSINESSNUMBER"));
            }
        }
        public string IBEI
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IBEI"));
            }
        }
        public string ISO18773PART1
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ISO18773PART1"));
            }
        }
        public string EconomicAgentCode
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ECONOMICAGENTCODE"));
            }
        }
        public string EconomicAreaCode
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ECONOMICAREACODE"));
            }
        }
        public string LinearDepPeriod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LINEARDEPPERIOD")); }
        }

        public string AccruedIntPeriod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACCRUEDINTPERIOD")); }
        }

        public string PreSettlementMethod
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("FXPRSMETHODUSED"));
            }
        }
        public string IdBC
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDBC"));
            }
        }
        public string Culture
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("CULTURE"));
            }
        }
        /// <summary>
        /// Obtient la devise de Reporting de l'acteur
        /// </summary>
        public string IdCCnf
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDC_CNF"));
            }
        }
        public string Culture_Cnf
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("CULTURE_CNF"));
            }
        }
        public string TaxNumber
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("TAXNUMBER"));
            }
        }

        public string IdCountryResidence
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRYRESIDENCE")); }
        }

        /// <summary>
        /// Obtient le code le code MIC 
        /// </summary>
        /// FI 20140206 [19564] add property
        public string ISO10383_ALPHA4
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISO10383_ALPHA4")); }
        }

        /// <summary>
        /// Obtient le code le code LEI
        /// </summary>
        /// FI 20140206 [19564] add property
        public string ISO17442
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISO17442")); }
        }

        /// <summary>
        /// Obtient le TimeZone de l'acteur
        /// </summary>
        /// EG 20170926 [22374] New
        public string TimeZone
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("TIMEZONE"));
            }
        }

        /// <summary>
        /// Obtient les types de messagerie consolidée paramétrées sur l'acteur
        /// <para>Obtient null si aucune messagerie consolidée sur l'acteur</para>
        /// </summary>
        public NotificationMultiPartiesEnum[] NotificationMultiParties
        {
            get
            {
                NotificationMultiPartiesEnum[] ret = null;

                List<NotificationMultiPartiesEnum> lst = new List<NotificationMultiPartiesEnum>();
                if (Convert.ToBoolean(GetFirstRowColumnValue("ISOWN_CNF")))
                    lst.Add(NotificationMultiPartiesEnum.OWN);
                if (Convert.ToBoolean(GetFirstRowColumnValue("ISCHILD_CNF")))
                    lst.Add(NotificationMultiPartiesEnum.CHILD);
                if (Convert.ToBoolean(GetFirstRowColumnValue("ISALL_CNF")))
                    lst.Add(NotificationMultiPartiesEnum.ALL);

                if (ArrFunc.IsFilled(lst))
                    ret = lst.ToArray();

                return ret;
            }
        }

        /// <summary>
        ///  Obtient le prénom
        /// </summary>
        /// FI 20170928 [23452] add
        public string FirstName
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("FIRSTNAME"));
            }
        }

        /// <summary>
        ///  Obtient le nom
        /// </summary>
        /// FI 20170928 [23417] add
        public string Surname
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("SURNAME"));
            }
        }

        /// <summary>
        ///  Obtient de type d'algorithme si l'actor est un algorithme
        /// </summary>
        /// FI 20170928 [23452] add
        public String AlgorithmType
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("ALGOTYPE"));
            }
        }

        /// <summary>
        ///  Obtient True si l'actor est un algorithme
        /// </summary>
        /// FI 20170928 [23452] add
        public Boolean IsAlgo
        {
            get
            {
                return StrFunc.IsFilled(AlgorithmType);
            }
        }


        public bool WithInfoEntity
        {
            get { return _withInfoEntity; }
            set
            {
                if (_withInfoEntity != value)
                {
                    InitProperty(false);
                    _withInfoEntity = value;
                }
            }
        }
        public bool WithInfoCapital
        {
            get { return _withInfoCapital; }
            set
            {
                if (_withInfoCapital != value)
                {
                    InitProperty(false);
                    _withInfoCapital = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RestrictEnum RestrictActorEnum
        {
            get { return _restrictActor; }
            set
            {
                if (_restrictActor != value)
                {
                    InitProperty(false);
                    _restrictActor = value;
                }
            }
        }
        #endregion public property

        #region public Method
        #region public SetRole
        /// <summary>
        /// Affecte la 1er liste des Rôles avec pRole
        /// </summary>
        /// <param name="pRole"></param>
        public void SetRole(RoleActor pRole)
        {
            SetRoleRange(new RoleActor[] { pRole });
        }
        #endregion
        
        #region public SetRoleWithCOUNTERPARTY
        /// <summary>
        /// Affecte la 1er liste des Rôles avec le rôle COUNTERPARTY
        /// </summary>
        public void SetRoleWithCOUNTERPARTY()
        {
            SetRole(RoleActor.COUNTERPARTY);
        }
        #endregion
        #region public ContainsRole
        /// <summary>
        /// Retourne true si la liste des rôles contient {pRole}
        /// </summary>
        /// <param name="pRole"></param>
        public bool ContainsRole(RoleActor pRole)
        {
            bool ret = false;
            if (ArrFunc.IsFilled(_withRole_In))
                ret = (_withRole_In.Where(x => x == pRole).Count() > 0);

            return ret;
        }
        #endregion
        #endregion

        #region private Method
        /// <summary>
        /// Définit la liste des Rôles avec {pRole}
        /// </summary>
        /// <param name="pRole"></param>
        public void SetRoleRange(RoleActor[] pRole)
        {
            // Function only allows roles passed in as array. All preceding roles are deleted
            InitProperty(false);

            _withRole_In = new RoleActor[pRole.Length];
            pRole.CopyTo(_withRole_In, 0);

        }

        /// <summary>
        /// Ajoute la restriction s'il existe une liste de rôle
        /// </summary>
        //FI/PL 20171130 Refactoring
        private void AddWhereACTORROLE()
        {
            if (ArrFunc.IsFilled(_withRole_In))
            {
                string predicat = StrFunc.AppendFormat(@"exists (
                select 1 from dbo.ACTORROLE 
				inner join dbo.{0} on ({0}.IDA = ACTORROLE.IDA)
				{1} and {2} and {3})",
                       this.SQLObject,
                       this.SQLWhere,
                       DataHelper.SQLColumnIn(CS, "ACTORROLE.IDROLEACTOR", _withRole_In, TypeData.TypeDataEnum.@string),
                       (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes) ? OTCmlHelper.GetSQLDataDtEnabled(CS, "ACTORROLE", DtRefForDtEnabled) : "0=0");

                ConstructWhere(predicat);
            }
        }
        #endregion private Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        /// FI 20141230 [20616] Add
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idGActor = null;
            }
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141107 [20441] Modify
        /// FI 20141230 [20616] Modify
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            // FI 20141230 [20616] Modify
            if (_idGActor.HasValue)
                AddJoinOnTableACTORG();
            string sqlFrom = string.Empty;
            if (_withInfoEntity)
                sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ENTITY, SQLJoinTypeEnum.Left, this.SQLObject + ".IDA", "ent", DataEnum.All);

            //PL 20120625 
            if (_withInfoCapital)
            {
                string leftACTORAMOUNT = SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTORAMOUNT.ToString() + " aa on (aa.IDA=" + this.SQLObject + ".IDA";
                leftACTORAMOUNT += " and aa.AMOUNTTYPE=" + DataHelper.SQLString(Cst.ActorAmountType.CAPITAL.ToString());
                leftACTORAMOUNT += " and aa.ISENABLED=1";
                leftACTORAMOUNT += " and aa.DTEFFECTIVE<=[DtCheck] and (aa.DTEXPIRATION is null or aa.DTEXPIRATION>[DtCheck]))";

                sqlFrom += leftACTORAMOUNT.Replace("[DtCheck]", DataHelper.SQLGetDateNoTime(CS));
            }

            if ((RestrictEnum.Yes == _restrictActor) && (_user.IsApplySessionRestrict()))
            {
                SessionRestrictHelper sr = new SessionRestrictHelper(_user, _sessionId, false);
                sqlFrom += sr.GetSQLActor(string.Empty, SQLObject + ".IDA");
            }

            ConstructFrom(sqlFrom);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            AddWhereACTORROLE();

            //PL 20161124 - RATP 4Eyes - MakingChecking
            //UNCOMMENT ConstructWhere(this.SQLObject + ".ISCHK=1");
        }

        /// <summary>
        /// Ajoute une jointure sur la table ACTORG
        /// </summary>
        /// FI 20141230 [20616] Add Method
        protected void AddJoinOnTableACTORG()
        {
            string sqlFrom = StrFunc.AppendFormat("inner join dbo.ACTORG ag on (ag.IDA={0}.IDA) and ag.IDGACTOR=@IDGACTOR", this.SQLObject);
            SetDataParameter(new DataParameter(CS, "IDGACTOR", DbType.Int32), _idGActor.Value.ToString());

            ConstructFrom(sqlFrom);
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public class SQL_ActorRef : SQL_TableWithID
    {
        #region members
        private SQL_Actor _sqlActor;
        #endregion

        #region constructor
        public SQL_ActorRef(string pSource, Cst.OTCml_TBL pTable, int pId)
            : base(pSource, pTable, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_ActorRef(string pSource, Cst.OTCml_TBL pTable, int pId, ScanDataDtEnabledEnum pScan)
            : base(pSource, pTable, IDType.Id, pId.ToString(), pScan) { }
        public SQL_ActorRef(string pSource, Cst.OTCml_TBL pTable, string pIdentifier)
            : base(pSource, pTable, IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_ActorRef(string pSource, Cst.OTCml_TBL pTable, string pIdentifier, ScanDataDtEnabledEnum pScan)
            : base(pSource, pTable, IDType.Identifier, pIdentifier, pScan) { }
        public SQL_ActorRef(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScan)
            : base(pSource, pTable, pIdType, pIdentifier, pScan) { }

        #endregion constructor

        #region public override _property_get
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACTOR_IDENTIFIER")); }
        }
        public override string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACTOR_DESCRIPTION")); }
        }
        public override string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACTOR_DISPLAYNAME")); }
        }
        public string BIC
        {
            get
            {
                string ret = Convert.ToString(GetFirstRowColumnValue("ACTOR_BIC"));
                if (StrFunc.IsEmpty(ret))
                {
                    ret = Convert.ToString(GetFirstRowColumnValue("ACTOR_IDENTIFIER"));
                }
                return ret;
            }
        }

        #endregion

        #region public_property_get
        /// <summary>
        /// Obtient l'alias utilisé pour la query pour la table ACTOR
        /// </summary>
        public string AliasActorTable
        {
            get { return "a"; }
        }
        // EG 20180205 [23769] Add dbTransaction  
        public SQL_Actor SqlActor
        {
            get
            {
                SQL_Actor ret = _sqlActor;
                if ((null == _sqlActor) && this.Id > 0)
                {
                    ret = new SQL_Actor(_csManager.CsSpheres, this.Id)
                    {
                        DbTransaction = DbTransaction
                    };
                }
                return ret;
            }
        }
        #endregion public_property

        #region public override Method
        protected override void SetSQLCol(string[] pCol)
        {
            if (IsQueryFindOnly(pCol))
                base.SetSQLCol(pCol);
            else
            {
                if (IsQuerySelectAllColumns(pCol))
                    pCol[0] = SQLObject + "." + "*";  // Pour Oracle Necessaire permet d'obtenir select ACTOR.*, a.IDENTIFIER  as  ACTOR_IDENTIFIER etc 
                //
                base.SetSQLCol(pCol);
                //
                AddSQLCol(AliasActorTable + ".IDENTIFIER as ACTOR_IDENTIFIER");
                AddSQLCol(AliasActorTable + ".DESCRIPTION as ACTOR_DESCRIPTION");
                AddSQLCol(AliasActorTable + ".DISPLAYNAME as ACTOR_DISPLAYNAME");
                AddSQLCol(AliasActorTable + ".BIC as ACTOR_BIC");
            }
        }

        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            string sqlFrom = OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, SQLObject.ToString() + ".IDA", AliasActorTable, DataEnum.All) + Cst.CrLf;
            ConstructFrom(sqlFrom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();

            if (Id_In != 0)
            {
                sqlWhere.Append(SQLObject + "." + OTCmlHelper.GetColunmID(SQLObject) + "=" + Id_In.ToString());
            }
            else if (Identifier_In != null)
            {
                Boolean isCaseInsensitive = IsUseCaseInsensitive(Identifier_In);

                string column = AliasActorTable + ".IDENTIFIER";
                string data_In = Identifier_In;
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                if (IsUseOrderBy)
                    // FI 20240130 [XXXXX] Add DataHelper.SQLString
                    sqlWhere.Append(column + SQLCst.LIKE + DataHelper.SQLString(data_In));
                else
                    sqlWhere.Append(column + "=" + DataHelper.SQLString(data_In));
            }
            else if (ExtLink_In != null)
            {
                Boolean isCaseInsensitive = IsUseCaseInsensitive(ExtLink_In);

                string column = AliasActorTable + ".EXTLLINK";
                string data_In = ExtLink_In;
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + DataHelper.SQLString(data_In));
                else
                    sqlWhere.Append(column + "=" + DataHelper.SQLString(data_In));
            }

            InitSQLWhere(sqlWhere.ToString(), false);
        }

        protected override void SetSQLOrderBy()
        {

            string sqlColumOrderBy = string.Empty;
            string orderBy = string.Empty;
            //
            if (this.IsUseOrderBy)
            {
                if (Identifier_In != null)
                {
                    //Critère like sur Identifier --> La query retournera n record --> Order by sur IDENTIFIER
                    sqlColumOrderBy = AliasActorTable + ".IDENTIFIER";
                }
                else if (ExtLink_In != null)
                {
                    //Critère like sur ExtlLink --> La query retournera n record --> Order by sur EXTLLINK
                    sqlColumOrderBy = SQLObject + ".EXTLLINK";
                }

                //Statistics GLop A vérifier
                if ((null != EfsObject) && EfsObject.IsWithStatistic)
                    orderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, Cst.OTCml_TBL.ACTOR.ToString(), sqlColumOrderBy);
                else
                    orderBy = sqlColumOrderBy;
            }
            //
            ConstructOrderBy(orderBy);
        }
        #endregion

        #region public LoadSQlActor
        public void LoadSQlActor()
        {
            _sqlActor = new SQL_Actor(_csManager.CsSpheres, this.Id);
            _sqlActor.LoadTable();
        }
        #endregion
    }


    /// <summary>
    /// Get an Book
    /// </summary>
    /// FI 20130208 [] add _isUseTable
    public class SQL_Book : SQL_TableWithID
    {
        #region private membres
        private int _idA_In;

        /// <summary>
        /// Prise en compte des restrictions sur ACTOR (SESSIONRESTRICT)
        /// </summary>
        private readonly RestrictEnum _restrictActor;

        /// <summary>
        /// Représente le user (utilisé pour les restrictions uniquement)
        /// </summary>
        private readonly User _user;

        /// <summary>
        /// Représente l'identifiant de la session du User
        /// </summary>
        private readonly string _sessionId;

        /// <summary>
        /// Permet de d'utiliser la table BOOK plutôt que la vue VW_BOOK_VIEWER
        /// </summary>
        private bool _isUseTable;


        /// <summary>
        /// Utilisé pour restriction sur un groupe de Book
        /// </summary>
        Nullable<int> _idGBook;


        #endregion




        public SQL_Book(string pSource, int pId)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No, 0) { }
        public SQL_Book(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, IDType.Id, pId.ToString(), pScanDataEnabled, 0) { }
        public SQL_Book(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, pIdType, pIdentifier, ScanDataDtEnabledEnum.No, 0) { }
        public SQL_Book(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, pIdType, pIdentifier, pScanDataEnabled, 0) { }
        public SQL_Book(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, int pIdA)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, pIdType, pIdentifier, pScanDataEnabled, pIdA) { }
        public SQL_Book(string pSource, Cst.OTCml_TBL pTbl, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, int pIdA)
            : this(pSource, pTbl, pIdType, pIdentifier, pScanDataEnabled, pIdA, RestrictEnum.No, null, string.Empty) { }
        public SQL_Book(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, int pIdA, RestrictEnum pRestrictActor, User pUser, string pSessionId)
            : this(pSource, Cst.OTCml_TBL.VW_BOOK_VIEWER, pIdType, pIdentifier, pScanDataEnabled, pIdA, pRestrictActor, pUser, pSessionId) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTbl"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pScanDataEnabled"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRestrictActor"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// FI 20141107 [20441] Modification de signature
        public SQL_Book(string pSource, Cst.OTCml_TBL pTbl, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, int pIdA,
            RestrictEnum pRestrictActor, User pUser, string pSessionId)
            : base(pSource, pTbl, pIdType, pIdentifier, pScanDataEnabled)
        {
            IdA_In = pIdA;
            _restrictActor = pRestrictActor;
            _sessionId = pSessionId;

            _user = pUser;
            _sessionId = pSessionId;
            if (null == _user)
                _user = new User(1, null, RoleActor.SYSADMIN);
        }

        #region public_property_get_set
        public int IdA_In
        {
            get { return _idA_In; }
            set
            {
                InitProperty(false);
                _idA_In = value;
            }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique l'usage de la table BOOK comme object principal de la requête 
        /// <para>si true: usage de BOOK</para>
        /// <para>si false: usage de VW_BOOK_VIEWER</para>
        /// </summary>
        public bool IsUseTable
        {
            get
            {
                return _isUseTable;
            }
            set
            {
                _isUseTable = value;
                //
                if (_isUseTable)
                    SQLObject = Cst.OTCml_TBL.BOOK.ToString();
                else
                    SQLObject = Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString();
            }
        }


        /// <summary>
        ///  Utilisé pour restriction sur un groupe de ACTOR
        /// </summary>
        /// FI 20141230 [20616] Add
        public Nullable<int> IdGBook
        {
            get { return _idGBook; }
            set
            {
                if (_idGBook != value)
                {
                    InitProperty(false);
                    _idGBook = value;
                }
            }
        }

        #endregion public_property_get_set

        #region public property
        public override int Id
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDB")); }
        }
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public int IdA_Entity
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_ENTITY")); }
        }
        public int IdA_ContactOffice
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_CONTACTOFFICE")); }
        }
        public int IdA_SettlementOffice
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_STLOFFICE")); }
        }
        public string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }
        public string LocalClassDerv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOCALCLASSDERV")); }
        }
        public string LocalClassNDrv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOCALCLASSNDRV")); }
        }
        public string IASClassDerv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IASCLASSDERV")); }
        }
        public string IASClassNDrv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IASCLASSNDRV")); }
        }
        public string HedgeClassDerv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("HEDGECLASSDERV")); }
        }
        public string HedgeClassNDrv
        {
            get { return Convert.ToString(GetFirstRowColumnValue("HEDGECLASSNDRV")); }
        }
        public string FxClass
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FXCLASS")); }
        }
        public string AccruedIntMethod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACCRUEDINTMETHOD")); }
        }
        public string LinearDepPeriod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LINEARDEPPERIOD")); }
        }
        public string AccruedIntPeriod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACCRUEDINTPERIOD")); }
        }
        public bool IsReceiveNcMsg
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISRECEIVENCMSG")); }
        }
        public bool IsPosKeeping
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISPOSKEEPING")); }
        }
        public string FullName
        {
            get
            {
                if (this.SQLObject == Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString())
                    return Convert.ToString(GetFirstRowColumnValue("FULLNAME"));
                else
                    return this.DisplayName;
            }
        }
        public string OptionsPosEffect
        {
            get { return Convert.ToString(GetFirstRowColumnValue("OPTPOSEFFECT")); }
        }
        public string OTCPosEffect
        {
            get { return Convert.ToString(GetFirstRowColumnValue("OTCPosEffect")); }
        }
        public string FuturesPosEffect
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FUTPOSEFFECT")); }
        }
        public Cst.CheckModeEnum VRFee
        {
            get { return GetFirstRowColumnCheckMode("VRFEE"); }
        }
        #endregion public property Id


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        /// FI 20141230 [20616] gestion de _idGBook
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idA_In = 0;
                _idGBook = null;
            }
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            if (_idA_In != 0)
            {
                string sqlWhereIda;
                if (_isUseTable)
                {
                    //Clé d'accès sur la table BOOK: IDB+IDA 
                    sqlWhereIda = "IDA=@IDA";
                    SetDataParameter(new DataParameter(CS, "IDA", DbType.Int32), _idA_In);
                }
                else
                {
                    //Clé d'accès sur la vue VW_BOOK_VIEWER: IDB+FK (FK contient l'IDA du propriétaire du Book ou d'un parent de celui-ci)
                    sqlWhereIda = "FK=@FK";
                    SetDataParameter(new DataParameter(CS, "FK", DbType.Int32), _idA_In);
                }
                ConstructWhere(sqlWhereIda);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20141107 [20441] Modify
        /// FI 20141230 [20616] Modify
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            // FI 20141230 [20616] Modify
            if (_idGBook.HasValue)
                AddJoinOnTableBOOKG();

            string sqlFrom = string.Empty;
            if ((RestrictEnum.Yes == _restrictActor) && (_user.IsApplySessionRestrict()))
            {
                SessionRestrictHelper sr = new SessionRestrictHelper(_user, _sessionId, false);
                sqlFrom += sr.GetSQLActor(string.Empty, SQLObject + ".IDA");
            }

            ConstructFrom(sqlFrom);
        }

        /// <summary>
        /// Ajoute une jointure sur la table BOOKG
        /// </summary>
        /// FI 20141230 [20616] Add Method
        protected void AddJoinOnTableBOOKG()
        {
            string sqlFrom = StrFunc.AppendFormat("inner join dbo.BOOKG bg on (bg.IDB={0}.IDB and bg.IDGBOOK=@IDGBOOK)", this.SQLObject);
            SetDataParameter(new DataParameter(CS, "IDGBOOK", DbType.Int32), _idGBook.Value.ToString());

            ConstructFrom(sqlFrom);
        }
    }

    /// <summary>
    /// Lecture de la table CSS
    /// </summary>
    public class SQL_Css : SQL_ActorRef
    {

        #region constructor
        public SQL_Css(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.CSS, pId) { }
        public SQL_Css(string pSource, int pId, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.CSS, pId, pScan) { }
        public SQL_Css(string pSource, string pIdentifier)
            : base(pSource, Cst.OTCml_TBL.CSS, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Css(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.CSS, pIdentifier, pScan) { }
        #endregion constructor

        #region public_property_get
        public string IdCountry
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRY")); }
        }
        public string Source
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SOURCE")); }
        }
        public bool IsTarget
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTARGET")); }
        }
        public string PhysicalAddr
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PHYSICALADDR")); }
        }
        public string PhysicalAddrIdent
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PHYSICALADDRIDENT")); }
        }
        public string LogicalAddr
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOGICALADDR")); }
        }
        public string LogicalAddrIdent
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOGICALADDRIDENT")); }
        }
        public string CssType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSTYPE")); }
        }
        public string SystemType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYSTEMTYPE")); }
        }
        public string SettlementType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SETTLEMENTTYPE")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }
        public bool IsMsgIssue
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISMSGISSUE")); }
        }
        public int PeriodMultiplierMsgIssue
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPMSGISSUE")); }
        }
        public string PeriodMsgIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PERIODMSGISSUE")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOTC
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISOTC")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFx
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISFX")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsCommodities
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISCOMMODITIES")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsEquities
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISEQUITIES")); }
        }

        /// <summary>
        /// Id interne de la méthode de calcul de déposit
        /// </summary>
        // PM 20160404 [22116] New
        public int? IdIMMethod
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDIMMETHOD")); }
        }

        /// <summary>
        ///  Obtient la méthode de calcul utilisé pour l'évaluation du Risque 
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //public string InitialMarginMethod
        //{
        //    get { return Convert.ToString(GetFirstRowColumnValue("INITIALMARGINMETH")); }
        //}

        /// <summary>
        ///  Obtient la méthode de calcul utilisé pour l'évaluation du Risque 
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //public Nullable<InitialMarginMethodEnum> InitialMarginMethodEnum
        //{
        //    get
        //    {
        //        Nullable<InitialMarginMethodEnum> ret = null;
        //        //
        //        string method = this.InitialMarginMethod;
        //        if (StrFunc.IsFilled(method))
        //        {
        //            if (System.Enum.IsDefined(typeof(InitialMarginMethodEnum), method))
        //                ret = (InitialMarginMethodEnum)Enum.Parse(typeof(InitialMarginMethodEnum), method);
        //        }
        //        //
        //        return ret;
        //    }
        //}
        #endregion public_property_get
    }

    #region SQL_DefineExtendDet
    public class SQL_DefineExtendDet : SQL_TableWithID
    {
        #region constructors
        public SQL_DefineExtendDet(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_DefineExtendDet(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_DefineExtendDet(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_DefineExtendDet(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.DEFINEEXTENDDET, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
        //
        #region public_property_get
        public override int Id
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDEFINEEXTENDDET")); }
        }
        public string Scheme
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SCHEME")); }
        }
        public string DefaultValue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DEFAULTVALUE")); }
        }
        //
        public int IdDefineExtend
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDEFINEEXTEND")); }
        }
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }
        public override string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }
        public override string Description
        {

            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }
        public string DataType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DATATYPE")); }
        }
        public override string ExtlLink
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("EXTLLINK")); }
        }
        #endregion public_property_get
        //
        #region public_property_get_set
        #endregion
    }
    #endregion
    #region SQL_DefineExtend
    public class SQL_DefineExtend : SQL_TableWithID
    {
        #region constructors
        public SQL_DefineExtend(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_DefineExtend(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_DefineExtend(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_DefineExtend(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.DEFINEEXTEND, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
        //
        #region public_property_get
        public override int Id
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDEFINEEXTEND")); }
        }
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }
        public override string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }
        public override string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }
        public override string ExtlLink
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("EXTLLINK")); }
        }
        #endregion public_property_get
    }
    #endregion

    /// <summary>
    /// Get a Derivative Contract
    /// </summary>
    // PL 20181001 [24212] RICCODE/BBGCODE
    public class SQL_DerivativeContract : SQL_TableWithID
    {
        #region private members
        private int _IdMarket_In = 0;
        //PL 20231222 [WI789] Newness
        private string _unlFuture_AssetCategory = string.Empty;
        #endregion private members

        #region constructor
        public SQL_DerivativeContract(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), 0, ScanDataDtEnabledEnum.No) { }
        public SQL_DerivativeContract(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), 0, pScanDataDtEnabledEnum) { }

        public SQL_DerivativeContract(string pSource, string pIdentifier, int pIdM)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, ScanDataDtEnabledEnum.No) { }
        public SQL_DerivativeContract(string pSource, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, pScanDataDtEnabledEnum) { }
        public SQL_DerivativeContract(string pSource, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, DateTime pDtRefForDtEnabled)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, pScanDataDtEnabledEnum, pDtRefForDtEnabled) { }

        public SQL_DerivativeContract(string pSource, IDType pIdType, string pIdentifier, int pIdM)
            : this(pSource, pIdType, pIdentifier, pIdM, ScanDataDtEnabledEnum.No) { }
        public SQL_DerivativeContract(string pSource, IDType pIdType, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, pIdType, pIdentifier, pIdM, pScanDataDtEnabledEnum, DateTime.MinValue) { }

        public SQL_DerivativeContract(string pSource, IDType pIdType, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, DateTime pDtRefForDtEnabled)
            : base(pSource, Cst.OTCml_TBL.DERIVATIVECONTRACT, pIdType, pIdentifier, pScanDataDtEnabledEnum, pDtRefForDtEnabled)
        {
            //PL 20130604 New constructor with pDtRefForDtEnabled (Vu)
            _IdMarket_In = pIdM;
        }
        #endregion constructor

        #region public_property_get_set
        public int IdMarket_In
        {
            get { return _IdMarket_In; }
            set
            {
                if (_IdMarket_In != value)
                {
                    InitProperty(false);
                    _IdMarket_In = value;
                }
            }
        }
        #endregion

        #region public_property_get
        public int IdI
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }

        /// <summary>
        /// Obtient la devise du nominal
        /// </summary>
        public string NominalCurrency
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC_NOMINAL")); }
        }

        /// <summary>
        /// Obtient la devise d'expression du nominal
        /// </summary>
        public decimal NominalAmount
        {
            // EG 20160404 Migration vs2013
            // RD 20101020 : Dans le cas ou DERIVATIVECONTRACT.NOMINALVALUE est NULL, on considère pour l’instant ZERO afin de ne pas bloquer la génération des événements !
            get { return Convert.ToDecimal(GetFirstRowColumnValue("NOMINALVALUE")); }
        }
        /// <summary>
        /// Obtient la devise d'expression du prix 
        /// </summary>
        public string PriceCurrency
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC_PRICE")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdMarket
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdMaturityRule
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDMATURITYRULE")); }
        }
        /// <summary>
        /// get the IDASSET_UNL field (id of the underlying asset) of the derivativecontract table
        /// </summary>
        public int IdAssetUnl
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("IDASSET_UNL"));
            }
        }
        /// <summary>
        /// get the IDDC_UNL (id of the underlying derivatice contract) field of the derivativecontract table
        /// </summary>
        public int IdDcUnl
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("IDDC_UNL"));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int InstrumentNum
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("INSTRUMENTNUM")); }
        }
        /// <summary>
        /// Obtient la base d'expression des prix de négo, clôtûre
        /// <para>
        /// Obtient 100, si la colonne INSTRUMENTDEN est non renseignée
        /// </para>
        /// </summary>
        public int InstrumentDen
        {
            get
            {
                int ret = 100;
                if (false == (GetFirstRowColumnValue("INSTRUMENTDEN") == Convert.DBNull))
                    ret = Convert.ToInt32(GetFirstRowColumnValue("INSTRUMENTDEN"));
                return ret;
            }
        }
        /// <summary>
        /// Obtient 2, sauf s'il existe une valeur sous  PRICEDECLOCATOR
        /// </summary>
        public int PriceDecLocator
        {
            get
            {
                int ret = 2;
                if (false == (GetFirstRowColumnValue("PRICEDECLOCATOR") == Convert.DBNull))
                    ret = Convert.ToInt32(GetFirstRowColumnValue("PRICEDECLOCATOR"));
                return ret;
            }
        }
        public string FutValuationMethod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FUTVALUATIONMETHOD")); }
        }
        public string SettlementMethod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SETTLTMETHOD")); }
        }
        /// <summary>
        /// Obtient le code REUTERS du contrat [DERIVATIVECONTRACT.RICCODE]
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code BLOOMBERG du contrat [DERIVATIVECONTRACT.BBGCODE]
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        public Nullable<decimal> ContractMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("CONTRACTMULTIPLIER") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("CONTRACTMULTIPLIER"));
            }
        }
        public Nullable<decimal> Factor
        {
            get
            {
                if (GetFirstRowColumnValue("FACTOR") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("FACTOR"));
            }
        }
        public Nullable<decimal> StrikeIncrement
        {
            get
            {
                if (GetFirstRowColumnValue("STRIKEINCREMENT") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("STRIKEINCREMENT"));
            }
        }
        public Nullable<decimal> StrikeMinValue
        {
            get
            {
                if (GetFirstRowColumnValue("STARTSTRIKEPXRANGE") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("STARTSTRIKEPXRANGE"));
            }
        }
        public Nullable<decimal> StrikeMaxValue
        {
            get
            {
                if (GetFirstRowColumnValue("ENDSTRIKEPXRANGE") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("ENDSTRIKEPXRANGE"));
            }
        }

        /// <summary>
        /// Obtient le nbr de decimal du strike
        /// </summary>
        /// FI 20140916 [20353] add property
        public Nullable<int> StrikeDecLocator
        {
            get
            {
                if (GetFirstRowColumnValue("STRIKEDECLOCATOR") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("STRIKEDECLOCATOR"));
            }
        }


        public string ValuationMethod
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FUTVALUATIONMETHOD")); }
        }
        public string UnderlyingAsset
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNDERLYINGASSET")); }
        }
        public string UnderlyingGroup
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNDERLYINGGROUP")); }
        }
        public string Attribute
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTATTRIBUTE")); }
        }

        /// <summary>
        /// Obtient la categorie de l'asset sous jacent
        /// </summary>
        public string AssetCategory
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ASSETCATEGORY")); }
        }
        /// <summary>
        /// Categorie de l'asset du Future sous jacent, dans le cas d'une Option sur Future
        /// </summary>
        /// PL 20231222 [WI789] Newness
        public string UnlFuture_AssetCategory
        {
            get { return _unlFuture_AssetCategory; }
            set { _unlFuture_AssetCategory = value; }
        }        
        /// <summary>
                 /// Obtient O pour option ou F pour future
                 /// </summary>
        public string Category
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CATEGORY")); }
        }

        /// <summary>
        /// Obtient Option ou Future 
        /// </summary>
        /// FI 20220613 [XXXXX] Add
        public CfiCodeCategoryEnum CfiCodeCategory
        {
            get { return ReflectionTools.ConvertStringToEnum<CfiCodeCategoryEnum>(Convert.ToString(GetFirstRowColumnValue("CATEGORY"))); }
        }


        /// <summary>
        /// Obtient le symbole du contrat
        /// </summary>
        public string ContractSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTSYMBOL")); }
        }
        public string ExerciseStyle
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXERCISESTYLE")); }
        }
        public string ExerciseRule
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXERCISERULE")); }
        }
        /// <summary>
        /// Obtient true si Spheres génère les assets automatiquement depuis la saisie
        /// </summary>
        public bool IsAutoCreateAsset
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISAUTOCREATEASSET")); }
        }
        /// <summary>
        /// Obtient true si les modifications automatique du référentiel sont autorisées
        /// </summary>
        /// PM 20160215 [21491] New
        public bool IsAutoSetting
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISAUTOSETTING")); }
        }
        /// <summary>
        /// Obtient la date de maturité
        /// </summary>
        // EG 20171115 [23509] Use GetNullableDateTimeValue
        public Nullable<DateTime> MaturityDate
        {
            get
            {
                //if (GetFirstRowColumnValue("MATURITYDATE") == null)
                //    return null;
                //else
                //    return Convert.ToDateTime(GetFirstRowColumnValue("MATURITYDATE"));
                return GetNullableDateTimeValue("MATURITYDATE");
            }
        }
        public Cst.CheckModeEnum VRStrikePxRange
        {
            get { return GetFirstRowColumnCheckMode("VRSTRIKEPXRANGE"); }
        }
        public Cst.CheckModeEnum VRStrikeIncrement
        {
            get { return GetFirstRowColumnCheckMode("VRSTRIKEINCREMENT"); }
        }
        public Cst.CheckModeEnum VRMaturityDate
        {
            get { return GetFirstRowColumnCheckMode("VRMATURITYDATE"); }
        }
        public Cst.CheckModeEnum VRLastTradingDay
        {
            get { return GetFirstRowColumnCheckMode("VRLASTTRADINGDAY"); }
        }
        public Cst.CheckModeEnum VRDelivryDate
        {
            get { return GetFirstRowColumnCheckMode("VRDELIVERYDATE"); }
        }
        /// <summary>
        /// Get the trade price display base for the current derivative contract
        /// </summary>
        /// <remarks>this value is the base used to convert the current fraction part of the trade price into the base we want the price will
        /// be shown on screen. Actually used just in the Spheres reports.</remarks>
        public int PriceNumericBase
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("PRICENUMERICBASE"));
            }
        }
        /// <summary>
        /// Get the trade price display format for the current derivative contract
        /// </summary>
        /// <remarks>
        /// this value is used to style the converted fraction part of the trade price into a specific format we want the price shown on screen. 
        /// Actually used just in the Spheres reports.
        /// </remarks>
        public Cst.PriceFormatStyle PriceFormatStyle
        {
            get
            {
                return ParsePriceFormatStyle(GetFirstRowColumnValue("PRICEFORMATSTYLE"));
            }
        }
        /// <summary>
        /// Get the asset strike price display base for the current derivative contract
        /// </summary>
        /// <remarks>
        /// this value is the base used to convert the current fraction part of the strike into the base we want the price will
        /// be shown on screen. Actually used just in the Spheres reports.
        /// </remarks>
        public int StrikeNumericBase
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("STRIKENUMERICBASE"));
            }
        }
        /// <summary>
        /// Get the strike price display format for the current derivative contract
        /// </summary>
        /// <remarks>
        /// this value is used to style the converted fraction part of the strike price into a specific format we want the strike shown on screen. 
        /// Actually used just in the Spheres reports.
        /// </remarks>
        public Cst.PriceFormatStyle StrikeFormatStyle
        {
            get
            {
                return ParsePriceFormatStyle(GetFirstRowColumnValue("STRIKEFORMATSTYLE"));
            }
        }
        /// <summary>
        /// Get the multiplier to be applied to the current trade price before performing the conversion of the fraction part
        /// </summary>
        /// <remarks>
        /// Cette valeur
        /// est nécessaire pour gérer des unités de mesure différentes entre la devise du contrat dérivatif et le format du prix 
        /// affiché à l'écran. 
        /// Ex :  on a une devise en dollar pour un certain contrat mais on veut afficher le prix en centime de dollar, 
        /// il faudra appliquer une multiplication par 100 par rapport à la valeur donnée sur la transaction, 
        /// que par contre l'on veut toujours exprimée dans l'unité de mesure habituelle de la devise. 
        /// Voir le cas du contrat Wheat  du marché CME, qui dot être affiché en 1/4 centimes de dollars.
        /// </remarks>
        public decimal PriceMultiplier
        {
            get
            {
                return Convert.ToDecimal(GetFirstRowColumnValue("PRICEMULTIPLIER"));
            }
        }
        /// <summary>
        /// Get the multiplier to be applied to the current strike before performing the conversion of the fraction part
        /// </summary>
        /// <remarks>
        /// Cette valeur
        /// est nécessaire pour gérer des unités de mesure différentes entre la devise du contrat dérivatif et le format du strike 
        /// affiché à l'écran. 
        /// Ex :  on a une devise en dollar pour un certain contrat mais on veut afficher le prix en centime de dollar, 
        /// il faudra appliquer une multiplication par 100 par rapport à la valeur donnée sur la transaction, 
        /// que par contre l'on veut toujours exprimée dans l'unité de mesure habituelle de la devise. 
        /// Voir le cas du contrat Wheat  du marché CME, qui dot être affiché en 1/4 centimes de dollars.
        /// </remarks>
        public decimal StrikeMultiplier
        {
            get
            {
                return Convert.ToDecimal(GetFirstRowColumnValue("STRIKEMULTIPLIER"));
            }
        }
        // EG 20130805 Merge
        public Nullable<int> RoundPrecWeightAveragePrice
        {
            get
            {
                Nullable<int> precision = null;
                object _precision = GetFirstRowColumnValue("ROUNDPRECWAVGPRICE");
                if (null != _precision)
                    precision = Convert.ToInt32(_precision);
                return precision;
            }
        }
        // EG 20130805 Merge
        public string RoundDirWeightAveragePrice
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ROUNDDIRWAVGPRICE")); }
        }
        public Nullable<Cst.RoundingDirectionSQL> RoundingDirectionSQLWeightAVGPrice
        {
            get
            {
                Nullable<Cst.RoundingDirectionSQL> direction = null;
                if (IsLoaded)
                {
                    if (Enum.IsDefined(typeof(Cst.RoundingDirectionSQL), RoundDirWeightAveragePrice))
                        direction = (Cst.RoundingDirectionSQL)ReflectionTools.EnumParse(new Cst.RoundingDirectionSQL(), RoundDirWeightAveragePrice);
                }
                return direction;
            }
        }

        /// <summary>
        /// Méthode de cotation des prix
        /// </summary>
        // PM 20130801 [18876] Added for Contract with Variable tick Value
        public string PriceQuoteMethod
        {
            get
            {
                string quoteMethod = "STD";
                if (GetFirstRowColumnValue("PRICEQUOTEMETHOD") != null)
                {
                    quoteMethod = Convert.ToString(GetFirstRowColumnValue("PRICEQUOTEMETHOD"));
                }
                return quoteMethod;
            }
        }


        /// <summary>
        /// Type de DC
        /// </summary>
        /// FI 20140206 [19564] add
        public DerivativeContractTypeEnum ContractTypeEnum
        {
            get
            {
                return ParseContractType(GetFirstRowColumnValue("CONTRACTTYPE"));
            }
        }

        /// <summary>
        /// Code de conversion des cours issus de fichier SPAN
        /// </summary>
        // PM 20150824 Ajout PriceAlignCode pour l'import des fichiers SPAN Xml
        public string PriceAlignCode
        {
            get
            {
                string priceAlignCode = null;
                if (GetFirstRowColumnValue("PRICEALIGNCODE") != null)
                {
                    priceAlignCode = Convert.ToString(GetFirstRowColumnValue("PRICEALIGNCODE"));
                }
                return priceAlignCode;
            }
        }

        /// <summary>
        /// Code de conversion des strike issus de fichier SPAN
        /// </summary>
        // PM 20150824 Ajout StrikeAlignCode pour l'import des fichiers SPAN Xml
        public string StrikeAlignCode
        {
            get
            {
                string strikeAlignCode = null;
                if (GetFirstRowColumnValue("STRIKEALIGNCODE") != null)
                {
                    strikeAlignCode = Convert.ToString(GetFirstRowColumnValue("STRIKEALIGNCODE"));
                }
                return strikeAlignCode;
            }
        }

        /// <summary>
        ///  Obtient le format utilisé pour générer les identifiants des DC/Asset ETD
        /// </summary>
        /// FI 20220912 [XXXXX] Add
        public string ETDIdentifierFormat
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDENTIFIERFORMAT"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140206 [19564] method is static
        private static Cst.PriceFormatStyle ParsePriceFormatStyle(object pFirstRowColumnValue)
        {
            Cst.PriceFormatStyle result = default;

            if (pFirstRowColumnValue != null && Enum.IsDefined(typeof(Cst.PriceFormatStyle), pFirstRowColumnValue))
            {
                string name = Convert.ToString(pFirstRowColumnValue);
                result = (Cst.PriceFormatStyle)Enum.Parse(typeof(Cst.PriceFormatStyle), name);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFirstRowColumnValue"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add method
        private static DerivativeContractTypeEnum ParseContractType(object pFirstRowColumnValue)
        {
            DerivativeContractTypeEnum result = DerivativeContractTypeEnum.STD;

            if (pFirstRowColumnValue != null && Enum.IsDefined(typeof(DerivativeContractTypeEnum), pFirstRowColumnValue))
            {
                string name = Convert.ToString(pFirstRowColumnValue);
                result = (DerivativeContractTypeEnum)Enum.Parse(typeof(DerivativeContractTypeEnum), name);
            }

            return result;
        }



        #endregion public_property_get

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {

            base.SetSQLWhere();
            //IDM
            if (_IdMarket_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), _IdMarket_In);
            }

        }
        #endregion
    }





    /// <summary>
    /// Get a commodity Contract
    /// </summary>
    /// FI 20170223 [22883] Add
    public class SQL_CommodityContract : SQL_TableWithID
    {
        #region private members
        private int _IdMarket_In = 0;
        //
        #endregion private members

        #region constructor
        public SQL_CommodityContract(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), 0, ScanDataDtEnabledEnum.No) { }
        public SQL_CommodityContract(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), 0, pScanDataDtEnabledEnum) { }

        public SQL_CommodityContract(string pSource, string pIdentifier, int pIdM)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, ScanDataDtEnabledEnum.No) { }
        public SQL_CommodityContract(string pSource, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, pScanDataDtEnabledEnum) { }
        public SQL_CommodityContract(string pSource, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, DateTime pDtRefForDtEnabled)
            : this(pSource, IDType.Identifier, pIdentifier, pIdM, pScanDataDtEnabledEnum, pDtRefForDtEnabled) { }

        public SQL_CommodityContract(string pSource, IDType pIdType, string pIdentifier, int pIdM)
            : this(pSource, pIdType, pIdentifier, pIdM, ScanDataDtEnabledEnum.No) { }
        public SQL_CommodityContract(string pSource, IDType pIdType, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, pIdType, pIdentifier, pIdM, pScanDataDtEnabledEnum, DateTime.MinValue) { }

        public SQL_CommodityContract(string pSource, IDType pIdType, string pIdentifier, int pIdM, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, DateTime pDtRefForDtEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_COMMODITYCONTRACT, pIdType, pIdentifier, pScanDataDtEnabledEnum, pDtRefForDtEnabled)
        {
            _IdMarket_In = pIdM;
        }
        #endregion constructor

        #region public_property_get_set
        public int IdMarket_In
        {
            get { return _IdMarket_In; }
            set
            {
                if (_IdMarket_In != value)
                {
                    InitProperty(false);
                    _IdMarket_In = value;
                }
            }
        }
        #endregion

        #region public_property_get
        /// <summary>
        /// 
        /// </summary>
        public int IdMarket
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM")); }
        }

        /// <summary>
        ///
        /// </summary>
        public int QtyScale
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("SCALEQTY")); }
        }


        /// <summary>
        /// Obtient le type de négociation pouvant être réalisé sur le Commodity Contract
        /// <para>Les valeurs possible sont "Intraday" ou "Spot"</para>
        /// </summary>
        /// FI 20170908 [23409] Add
        public string TradableType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TRADABLETYPE")); }
        }



        #endregion public_property_get

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {

            base.SetSQLWhere();
            //IDM
            if (_IdMarket_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), _IdMarket_In);
            }

        }
        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQL_Entity : SQL_ActorRef
    {
        #region constructor
        public SQL_Entity(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.ENTITY, pId) { }
        public SQL_Entity(string pSource, int pId, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.ENTITY, pId, pScan) { }
        public SQL_Entity(string pSource, string pIdentifier)
            : base(pSource, Cst.OTCml_TBL.ENTITY, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Entity(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.ENTITY, pIdentifier, pScan) { }
        #endregion constructor

        #region public_property_get
        public override string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }
        public string IdCAccount
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCACCOUNT")); }
        }
        public string IdBCAccount
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBCACCOUNT")); }
        }
        public string IdReport
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCREPORT")); }
        }

        /// <summary>
        ///  Obtient true si Spheres® doit générer des notifications vers les clients
        /// </summary>
        public bool IsSendNcMsgClient
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSENDNCMSG_CLIENT")); }
        }

        /// <summary>
        ///  Obtient true si Spheres® doit générer des notifications vers les Donneurs d'ordre Maison
        /// </summary>
        public bool IsSendNcMsgHouse
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSENDNCMSG_ENTITY")); }
        }

        /// <summary>
        ///  Obtient true si Spheres® doit générer des notifications vers les contreparties externes
        /// </summary>
        public bool IsSendNcMsgExt
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSENDNCMSG_EXT")); }
        }

        public int IdBookOpp
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDB_OPP")); }
        }
        #endregion public_property_get
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQL_Event : SQL_TableWithID
    {
        #region constructors
        public SQL_Event(string pSource, int pIdE) : base(pSource, Cst.OTCml_TBL.EVENT, IDType.Id, pIdE.ToString()) { }
        #endregion constructors

        #region public_property_get
        public int IdT
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDT")); }
        }
        public int InstrumentNo
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("INSTRUMENTNO")); }
        }
        public string InstrumentNo_ID
        {
            get { return Cst.FpML_InstrumentNo + InstrumentNo.ToString(); }
        }

        public int StreamNo
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("STREAMNO")); }
        }

        public int IdE_Event
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDE_EVENT")); }
        }

        public int IdAPayer
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_PAY")); }
        }

        public int IdBPayer
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDB_PAY")); }

        }

        public int IdAReceiver
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_REC")); }
        }

        public int IdBReceiver
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDB_REC")); }
        }

        public string EventCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTCODE")); }
        }

        public string EventType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTTYPE")); }
        }

        public string IdStCalcul
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTCALCUL")); }
        }

        public string UnitType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITTYPE")); }
        }
        public string Unit
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNIT")); }
        }

        public DateTime DtStartUnadjusted
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTARTUNADJ")); }
        }
        public DateTime DtStartAdjusted
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTARTADJ")); }
        }

        public DateTime DtEndUnadjusted
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTENDUNADJ")); }
        }
        public DateTime DtEndAdjusted
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTENDADJ")); }
        }

        // Activation
        public string IdStActivation
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTACTIVATION")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime DtStActivation
        {
            get
            {
                // FI 20200820 [25468] Dates systemes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTACTIVATION")), DateTimeKind.Utc);
            }
        }

        public int IdAStActivation
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTACTIVATION")); }
        }

        #endregion

        #region Methods
        #region GetIdBook
        public int GetIdBook(string pPayRec)
        {
            PayerReceiverEnum payerReceiver = (PayerReceiverEnum)StringToEnum.Parse(pPayRec, PayerReceiverEnum.Receiver);
            return GetIdBook(payerReceiver);
        }
        public int GetIdBook(PayerReceiverEnum pPayRec)
        {
            int ret;
            if (PayerReceiverEnum.Payer == pPayRec)
                ret = IdBPayer;
            else
                ret = IdBReceiver;
            return ret;
        }
        #endregion GetIdBook
        #region GetIdAPayRec
        public int GetIdAPayRec(string pPayRec)
        {
            PayerReceiverEnum payerReceiver = (PayerReceiverEnum)StringToEnum.Parse(pPayRec, PayerReceiverEnum.Receiver);
            return GetIdAPayRec(payerReceiver);
        }
        public int GetIdAPayRec(PayerReceiverEnum pPayRec)
        {
            int ret;
            if (PayerReceiverEnum.Payer == pPayRec)
                ret = IdAPayer;
            else
                ret = IdAReceiver;
            return ret;
        }
        #endregion GetIdAPayRec
        #region GetIdConterparty
        /// <summary>
        /// Get Receiver if pPayRec == Payer
        /// Get Payer  if pPayRec == Receiver
        /// </summary>
        /// <param name="pPayRec"></param>
        /// <returns></returns>
        public int GetIdConterparty(string pPayRec)
        {
            PayerReceiverEnum payerReceiver = (PayerReceiverEnum)StringToEnum.Parse(pPayRec, PayerReceiverEnum.Receiver);
            return GetIdConterparty(payerReceiver);

        }
        public int GetIdConterparty(PayerReceiverEnum pPayRec)
        {
            int ret;
            if (PayerReceiverEnum.Payer == pPayRec)
                ret = IdAReceiver;
            else
                ret = IdAPayer;
            return ret;
        }
        #endregion GetIdConterparty
        #endregion Methods

        #region public override ToString
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string d1 = DtFunc.DateTimeToString(DtStartAdjusted, DtFunc.FmtDateyyyyMMdd);
            string d2 = DtFunc.DateTimeToString(DtEndAdjusted, DtFunc.FmtDateyyyyMMdd);
            sb.Append($"Event[Id:{Id}, Code:{EventCode}, Type:{EventType}, Unit:{Unit}, Start Date:{d1}, End Date:{d2}]");
            return sb.ToString();
        }
        #endregion ToString
    }



    /// <summary>
    /// 
    /// </summary>
    public class SQL_Fee : SQL_TableWithID
    {
        //
        #region
        private readonly bool _isUsePaymentTypeIdentifier;
        private readonly string _paymentType_In;
        #endregion
        //
        #region Accessors
        /// <summary>
        /// Obtient le type d'événement associé (colonne EVENTTYPE)
        /// </summary>
        public string EventType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTTYPE")); }
        }
        /// <summary>
        /// Obtient le type de paiement (colonne PAYMENTTYPE)
        /// </summary>
        public string PaymentType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PAYMENTTYPE")); }
        }
        /// <summary>
        /// <para>Obtient la valeur de la colonne TAXAPPLICATION si elle est not null</para>
        /// <para>Obtient TaxApplicationEnum.Always si la colonne TAXAPPLICATION si elle est null</para>
        /// </summary>
        public string TaxApplication
        {
            get
            {
                string taxApplication = TaxApplicationEnum.Always.ToString();
                if (null != GetFirstRowColumnValue("TAXAPPLICATION"))
                    taxApplication = GetFirstRowColumnValue("TAXAPPLICATION").ToString();
                return taxApplication;
            }

        }
        /// <summary>
        /// <para>Obtient la valeur de la colonne ISDEPRECIABLE</para>
        /// </summary>
        public bool IsDepreciable
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISDEPRECIABLE")); }
        }
        /// <summary>
        /// <para>Obtient la valeur de la colonne ISIFRSACTUALRATE</para>
        /// </summary>
        public bool IsIFRSActaulRate
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISIFRSACTUALRATE")); }
        }
        /// <summary>
        /// <para>Obtient la valeur de la colonne ISINVOICING</para>
        /// </summary>
        public bool IsInvoicing
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISINVOICING")); }
        }
        #endregion Accessors

        //
        #region constructors
        public SQL_Fee(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Fee(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_Fee(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Fee(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.FEE, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            _isUsePaymentTypeIdentifier = (SQL_TableWithID.IDType.Identifier2 == pIdType);
            //
            if (_isUsePaymentTypeIdentifier)
                _paymentType_In = pIdentifier;
        }
        #endregion constructors
        //
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override void SetSQLWhere()
        {
            if (_isUsePaymentTypeIdentifier)
            {
                SQLWhere sqlWhere = new SQLWhere();

                DataParameter dp = DataParameter.GetParameter(CS, DataParameter.ParameterEnum.PAYMENTTYPE);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(_paymentType_In);

                string column = SQLObject + ".PAYMENTTYPE";
                string paymentType = _paymentType_In;
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    paymentType = _paymentType_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, paymentType);

                InitSQLWhere(sqlWhere.ToString(), false);
            }
            else
            {
                base.SetSQLWhere();
            }
        }
        

        #region protected
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret;
                if (_isUsePaymentTypeIdentifier)
                    ret = (Id_In == 0) && (StrFunc.ContainsIn(_paymentType_In, "%"));
                else
                    ret = base.IsUseOrderBy;
                return ret;
            }
        }
        #endregion

        #region protected override SetSQLOrderBy
        protected override void SetSQLOrderBy()
        {
            if (_isUsePaymentTypeIdentifier)
            {
                if (this.IsUseOrderBy)
                {
                    //Critère like sur PAYMENTTYPE --> La query peut retourner n records --> Order by sur PAYMENTTYPE
                    string sqlColumOrderBy = SQLObject + ".PAYMENTTYPE";

                    //Statistics
                    string orderBy;
                    if ((null != EfsObject) && EfsObject.IsWithStatistic)
                        orderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, SQLObject, sqlColumOrderBy);
                    else
                        orderBy = sqlColumOrderBy;
                    ConstructOrderBy(orderBy);
                }
            }
            else
            {
                base.SetSQLOrderBy();
            }

        }
        #endregion

    }

    /// <summary>
    ///  classe de base utilisée par les conditions (FeeMatrix) et les barèmes (FeeSchedule)
    /// </summary>
    /// FI 20180502 [23926] Add 
    public abstract class SQL_FeeChildBase : SQL_TableWithID
    {
        private int _idFee_In = 0;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180502 [23926] Add
        public int IdFee
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDFEE")); }
        }

        #region public_property_get_set
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180502 [23926] Add
        public int IdFeeIn
        {
            get { return _idFee_In; }
            set
            {
                if (_idFee_In != value)
                {
                    InitProperty(false);
                    _idFee_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient le displayname s'il commence par l'identifier ou "Identifier - DisplayName"
        /// </summary>
        public string DisplayName2
        {
            get
            {
                string displayName = Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME"));
                string identifier = Convert.ToString(GetFirstRowColumnValue("IDENTIFIER"));

                return displayName.Trim().StartsWith(identifier.Trim()) ? displayName.Trim() : identifier.Trim() + " - " + displayName.Trim();
            }
        }


        #endregion


        #region constructors
        public SQL_FeeChildBase(string pSource, Cst.OTCml_TBL pTable, int pId)
            : this(pSource, pTable, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_FeeChildBase(string pSource, Cst.OTCml_TBL pTable, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pTable, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_FeeChildBase(string pSource, Cst.OTCml_TBL pTable, string pIdentifier)
            : this(pSource, pTable, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_FeeChildBase(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, pTable, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
                _idFee_In = 0;
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (IdFeeIn > 0)
            {
                string sqlFrom = StrFunc.AppendFormat("inner join dbo.FEE fee on  (fee.IDFEE={0}.IDFEE) and fee.IDFEE=@IDFEE", this.SQLObject);
                SetDataParameter(new DataParameter(CS, "IDFEE", DbType.Int32), IdFeeIn);
                ConstructFrom(sqlFrom);
            }
        }

        #endregion

    }

    /// <summary>
    /// Barème
    /// </summary>
    /// FI 20180502 [23926] heritage de SQL_FeeChildBase   
    public class SQL_FeeSchedule : SQL_FeeChildBase
    {
        #region constructors
        public SQL_FeeSchedule(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_FeeSchedule(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_FeeSchedule(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_FeeSchedule(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.FEESCHEDULE, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region public_property_get
        // EG 20210713 [XXXXX] Modification Libellé de frais sur les barèmes avec tranche - FEE1NUM doublé (corrigé par FEE2NUM)
        private bool IsComplexTarification
        {
            get
            {
                bool isComplexTarification = (GetFirstRowColumnValue("FEE1NUM") is DBNull) &&
                    (GetFirstRowColumnValue("FEE2NUM") is DBNull);
                if (false == isComplexTarification)
                    isComplexTarification = (GetFirstRowColumnValue("MINNUM") is DBNull) &&
                        (GetFirstRowColumnValue("MAXNUM") is DBNull);
                if (false == isComplexTarification)
                    isComplexTarification = (GetFirstRowColumnValue("BRACKETAPPLICATION") is DBNull);
                return isComplexTarification;
            }
        }
        // EG 20210713 [XXXXX] Modification Libellé de frais sur les barèmes avec tranche
        private bool IsBracketApplication
        {
            get
            {
                return null != GetFirstRowColumnValue("BRACKETAPPLICATION");
            }
        }
        #endregion public_property_get


        /// <summary>
        ///  Obtient l'acteur si le barème est dérogatoire
        /// </summary>
        /// FI 20180502 [23926] Add
        public Nullable<int> IdA
        {
            get
            {
                object o = GetFirstRowColumnValue("IDA");
                if (o == null)
                    return null;
                else
                    return Convert.ToInt32(o);
            }
        }


        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeCurrency"></param>
        /// <returns></returns>
        // EG 20210713 [XXXXX] Modification Libellé de frais sur les barèmes avec tranche
        public string ComplexLabel(string pTradeCurrency)
        {

            string library = Convert.ToString(GetFirstRowColumnValue("SCHEDULELIBRARY"));
            _ = Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME"));
            _ = Convert.ToString(GetFirstRowColumnValue("IDENTIFIER"));

            /// FI 20180502 [23926] Affichage de la librairie
            string ret = StrFunc.AppendFormat("{0} : {1}", library, DisplayName2);

            string feeCurrency = Convert.ToString(GetFirstRowColumnValue("IDC"));
            string currency = " " + (StrFunc.IsFilled(feeCurrency) ? feeCurrency : pTradeCurrency);

            string additionalLabel = " [{0}{1}]";
            FeeFormulaEnum formula = (FeeFormulaEnum)ReflectionTools.EnumParse(new FeeFormulaEnum(), GetFirstRowColumnValue("FORMULA").ToString());
            string[] resFormula = Ressource.GetString(formula.ToString()).Split(new string[] { "f()" }, StringSplitOptions.RemoveEmptyEntries);
            string arg1 = ArrFunc.IsFilled(resFormula) ? resFormula[0].Trim() : string.Empty;
            string arg2 = Ressource.GetString("DefaultFeeTitleLabel");

            if (false == IsComplexTarification)
            {
                // EG 20210713 [XXXXX] Modification Libellé de frais sur les barèmes avec tranche
                if (IsBracketApplication)
                {
                    arg2 = " : " + String.Format(Ressource.GetString("BracketFeeTitleLabel"), GetFirstRowColumnValue("BRACKETAPPLICATION"));
                }
                else
                {
                    decimal fee1Num = Convert.ToDecimal(GetFirstRowColumnValue("FEE1NUM"));
                    decimal fee1Den = Convert.ToDecimal(GetFirstRowColumnValue("FEE1DEN"));
                    decimal feeRate = fee1Num / (fee1Den == 0 ? 1 : fee1Den);
                    string feeValue = StrFunc.FmtDecimalToInvariantCulture(feeRate);

                    switch (formula)
                    {
                        case FeeFormulaEnum.CONST:
                            break;
                        case FeeFormulaEnum.F3KO:
                        case FeeFormulaEnum.F3MO:
                        case FeeFormulaEnum.F4:
                        case FeeFormulaEnum.F4QTY:
                            arg2 = " : " + feeValue + currency;
                            break;
                        case FeeFormulaEnum.F4BPS:
                            arg2 = " : " + feeValue;
                            break;
                        case FeeFormulaEnum.F1:
                        case FeeFormulaEnum.F3KOD:
                        case FeeFormulaEnum.F3MOD:
                        case FeeFormulaEnum.F2_V1:
                        case FeeFormulaEnum.F2_V2:
                        case FeeFormulaEnum.F2_V3:
                            break;
                        case FeeFormulaEnum.F4CPS:
                            arg2 = " : " + StrFunc.FmtDecimalToInvariantCulture(feeRate * 100) + currency;
                            break;
                        case FeeFormulaEnum.F4PRM:
                        case FeeFormulaEnum.F4STK:
                            arg2 = " : " + feeValue;
                            break;
                        default:
                            break;
                    }
                }
            }

            ret += String.Format(additionalLabel, arg1, arg2);


            if (IdA.HasValue && IdA.Value > 0)
            {
                string additionalLabel2 = " [{0}]";
                SQL_Actor sqlActor = new SQL_Actor(CS, IdA.Value);
                if (sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME", "DESCRIPTION" }))
                {
                    string displayName = sqlActor.DisplayName;
                    string identifier = sqlActor.Identifier;
                    arg1 = displayName.Trim().StartsWith(identifier.Trim()) ? displayName.Trim() : identifier.Trim() + " - " + displayName.Trim();
                    ret += String.Format(additionalLabel2, arg1);
                }
            }

            return ret;
        }
        #endregion Methods
    }

    /// <summary>
    /// Conditions
    /// </summary>
    /// FI 20180502 [23926] heritage de SQL_FeeChildBase   
    public class SQL_FeeMatrix : SQL_FeeChildBase
    {

        #region constructors
        public SQL_FeeMatrix(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_FeeMatrix(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_FeeMatrix(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_FeeMatrix(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.FEEMATRIX, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ComplexLabel()
        {
            return DisplayName2; 
        }
    }

    /// <summary>
    /// VW_INSTR_PRODUCT
    /// </summary>
    // EG 20190613 [24683] Upd
    public class SQL_Instrument : SQL_TableWithID
    {
        #region Members
        RestrictEnum _restrictInstr;
        /// <summary>
        /// Représente un User (utilisé pour les restrictions uniquement)
        /// </summary>
        private readonly User _user;

        /// <summary>
        /// Représente l'identifiant de la session du User
        /// </summary>
        private readonly string _sessionId;


        #endregion

        #region constructor
        //Use Id
        public SQL_Instrument(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }
        public SQL_Instrument(string pSource, int pId, RestrictEnum pRestrictionInstr, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, User pUser, string pSessionId)
            : this(pSource, IDType.Id, pId.ToString(), pRestrictionInstr, pScanDataDtEnabledEnum, pUser, pSessionId) { }
        // Identifier
        public SQL_Instrument(string pSource, string pIdentifier)
            : this(pSource, IDType.Identifier, pIdentifier, RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }
        public SQL_Instrument(string pSource, string pIdentifier, RestrictEnum pRestrictionInstr, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, User pUser, string pSessionId)
            : this(pSource, IDType.Identifier, pIdentifier, pRestrictionInstr, pScanDataDtEnabledEnum, pUser, pSessionId) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrictionInstr"></param>
        /// <param name="pScanDataDtEnabledEnum"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        // FI 20141107 [20441] Modification de signature
        public SQL_Instrument(string pSource, IDType pIdType, string pIdentifier, RestrictEnum pRestrictionInstr, ScanDataDtEnabledEnum pScanDataDtEnabledEnum,
            User pUser, string pSessionId)
            : base(pSource, Cst.OTCml_TBL.VW_INSTR_PRODUCT, pIdType, pIdentifier, pScanDataDtEnabledEnum)
        {
            _restrictInstr = pRestrictionInstr;

            _sessionId = pSessionId;

            _user = pUser;
            _sessionId = pSessionId;
            if (null == _user)
                _user = new User(1, null, RoleActor.SYSADMIN);
        }




        #endregion constructor

        #region public property
        //INSTRUMENT
        public FungibilityModeEnum FungibilityMode
        {
            get
            {
                FungibilityModeEnum _fungibilityMode = (FungibilityModeEnum)ReflectionTools.EnumParse(new FungibilityModeEnum(),
                    Convert.ToString(GetFirstRowColumnValue("FUNGIBILITYMODE")));
                return _fungibilityMode;
            }
        }
        public bool IsFungibilityMode_OPENCLOSE
        {
            get { return this.FungibilityMode == FungibilityModeEnum.OPENCLOSE; }
        }

        /// <summary>
        ///  Obtient True si l'instrument est fongible
        /// </summary>
        /// FI 20170116 [21916] Add
        public bool IsFungible
        {
            get { return this.FungibilityMode != FungibilityModeEnum.NONE; }
        }



        public Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get
            {
                Nullable<Cst.UnderlyingAsset> _underlyingAsset = null;

                object _obj = GetFirstRowColumnValue("ASSETCATEGORY");
                if (null != _obj)
                {
                    string _temp = Convert.ToString(_obj);
                    if (Enum.IsDefined(typeof(Cst.UnderlyingAsset), _temp))
                        _underlyingAsset = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), _temp);
                }
                return _underlyingAsset;
            }
        }
        public int IdI
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }
        public int IdP
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDP")); }
        }
        public bool IsIFRS
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISIFRS")); }
        }
        public bool IsEvents
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISEVENTS")); }
        }
        // EG 20150306 POC - BERKELEY
        public bool IsFunding
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISFUNDING")); }
        }
        // EG 20150306 POC - BERKELEY
        public bool IsMargining
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISMARGINING")); }
        }
        public bool IsNoINTEvents
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISNOINTEVENTS")); }
        }
        public bool IsOpen
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISOPEN")); }
        }
        public bool IsExtend
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISEXTEND")); }
        }
        public bool IsAccounting
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISACCOUNTING")); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20240703 [WI989] Add
        public bool IsProductClassStrategy
        {

            get { return Convert.ToString(GetFirstRowColumnValue("CLASS")) == Cst.ProductClass_STRATEGY; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20240703 [WI989] Add
        public bool IsProductClassRegular
        {

            get { return Convert.ToString(GetFirstRowColumnValue("CLASS")) == Cst.ProductClass_REGULAR; }
        }


        public string CssFileName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSFILENAME")); }
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CssColor
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSFILENAME")); }
        }

        public string GProduct
        {
            get { return Convert.ToString(GetFirstRowColumnValue("GPRODUCT")); }
        }
        public string Family
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FAMILY")); }
        }
        public ProductTools.GroupProductEnum GroupProduct
        {
            get { return (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), GProduct); }
        }
        public ProductTools.FamilyEnum FamilyProduct
        {
            get { return (ProductTools.FamilyEnum)ReflectionTools.EnumParse(new ProductTools.FamilyEnum(), Family); }
        }

        /// <summary>
        /// Obient le colonne PRODUCT_IDENTIFIER
        /// </summary>
        public FungibilityModeEnum Product_FungibilityMode
        {
            get
            {
                FungibilityModeEnum _fungibilityMode = (FungibilityModeEnum)ReflectionTools.EnumParse(new FungibilityModeEnum(),
                    Convert.ToString(GetFirstRowColumnValue("PRODUCT_FUNGIBILITYMODE")));
                return _fungibilityMode;
            }
        }

        /// <summary>
        ///  Obtient True si le produi est fongible
        /// </summary>
        /// FI 20170116 [21916] Add
        public bool Product_IsFungible
        {
            get { return this.Product_FungibilityMode != FungibilityModeEnum.NONE; }
        }

        /// <summary>
        /// Obtient l'identifier du produit
        /// </summary>
        public string Product_Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PRODUCT_IDENTIFIER")); }
        }
        //
        public bool IsTradeIdByEntity
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTRADEIDBYENTITY")); }
        }
        public bool IsTradeIdByStEnv
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTRADEIDBYSTENV")); }
        }
        //20100312 PL-StatusBusiness
        public bool IsTradeIdByStBus
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTRADEIDBYSTBUS")); }
        }
        public bool IsTradeIdByDtTrade
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTRADEIDBYDTTRA")); }
        }
        public bool IsTradeIdByDtBusiness
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISTRADEIDBYDTBUS")); }
        }
        public string TradeIdFormat
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDFORMAT")); }
        }
        public int TradeIdNumberLength
        {
            get
            {
                int ret = -1;
                string data = Convert.ToString(GetFirstRowColumnValue("TRADEIDNUMLENGTH"));
                if (IntFunc.IsPositiveInteger(data))
                {
                    ret = IntFunc.IntValue(data);
                }
                return ret;
            }
        }
        public string TradeIdPrefixReg
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_REG")); }
        }
        public string TradeIdSuffixReg
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_REG")); }
        }
        public string TradeIdPrefixSim
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_SIM")); }
        }
        public string TradeIdSuffixSim
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_SIM")); }
        }
        public string TradeIdPrefixPre
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_PRE")); }
        }
        public string TradeIdSuffixPre
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_PRE")); }
        }
        public string TradeIdPrefixExe
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_EXE")); }
        }
        public string TradeIdSuffixExe
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_EXE")); }
        }
        public string TradeIdPrefixInt
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_INT")); }
        }
        public string TradeIdSuffixInt
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_INT")); }
        }
        public string TradeIdPrefixAll
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_ALL")); }
        }
        public string TradeIdSuffixAll
        {
            //20081128 PL Add string.Empty afin de tjs avoir une valeur non null
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_ALL")); }
        }
        public string TradeIdPrefixCorpoAction
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_CA")); }
        }
        public string TradeIdSuffixCorpoAction
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_CA")); }
        }
        // EG 20190613 [24683] New
        public string TradeIdPrefixClosingPosition
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_CLOPOS")); }
        }
        public string TradeIdSuffixClosingPosition
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_CLOPOS")); }
        }
        public string TradeIdPrefixReopeningPosition
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDPREFIX_REOPENPOS")); }
        }
        public string TradeIdSuffixReopeningPosition
        {
            get { return string.Empty + Convert.ToString(GetFirstRowColumnValue("TRADEIDSUFFIX_REOPENPOS")); }
        }
        #endregion public property

        #region Public property Get/set
        public RestrictEnum RestrictInstr
        {
            get { return _restrictInstr; }
            set
            {
                if (_restrictInstr != value)
                {
                    InitProperty(false);
                    _restrictInstr = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141107 [20441] Modify
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            //
            if ((RestrictEnum.Yes == _restrictInstr) && (_user.IsApplySessionRestrict()))
            {
                SessionRestrictHelper sr = new SessionRestrictHelper(_user, _sessionId, false);
                string sqlFrom = sr.GetSQLInstr(string.Empty, this.SQLObject + ".IDI");
                ConstructFrom(sqlFrom);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_Instr : SQL_TableWithID
    {
        #region constructor
        public SQL_Instr(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Instr(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum) { }
        public SQL_Instr(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Instr(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource, Cst.OTCml_TBL.INSTRUMENT, pIdType, pIdentifier, pScanDataDtEnabledEnum)
        { }
        #endregion

        #region public property
        //INSTRUMENT
        public int IdI
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }
        public int IdP
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDP")); }
        }
        public bool IsIFRS
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISIFRS")); }
        }
        #endregion public property
    }

    /// <summary>
    /// 
    /// </summary>
    /// PL 20181001 [24211] RICCODE/BBGCODE
    /// FI 20200114 [XXXXX] Possibilité d'interrogation via la colonne VW_MARKET_IDENTIFIER.SHORT_ACRONYM
    public class SQL_Market : SQL_TableWithID
    {
        #region Members
        private string _ISO10383_ALPHA4_In;
        private string _FIXML_SecurityExchange_In;
        private string _SHORT_ACRONYM_In;

        /// <summary>
        /// 
        /// </summary>
        readonly RestrictEnum _restrictMarket;

        /// <summary>
        /// Représente un User (utilisé pour les restrictions uniquement)
        /// </summary>
        private readonly User _user;

        /// <summary>
        /// Représente l'identifiant de la session du User
        /// </summary>
        private readonly string _sessionId;
        
        /// <summary>
        /// Utilisé pour restriction sur un groupe de marché
        /// </summary>
        Nullable<int> _idGMarket;

        bool _isUseView = false;

        #endregion

        #region constructors
        public SQL_Market(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }
        public SQL_Market(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), RestrictEnum.No, pScanDataEnabled, null, string.Empty) { }

        public SQL_Market(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, RestrictEnum.No, ScanDataDtEnabledEnum.No, null, string.Empty) { }
        public SQL_Market(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, RestrictEnum.No, pScanDataEnabled, null, string.Empty) { }

        public SQL_Market(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pIdType, pIdentifier, RestrictEnum.No, pScanDataEnabled, null, string.Empty) { }
        //PL 20130208 ISO-GLOP
        //public SQL_Market(string pSource, IDType pIdType, string pIdentifier, RestrictEnum pRestrictionMarket, ScanDataDtEnabledEnum pScanDataEnabled, string pSessionId, bool pIsSessionAdmin)
        //    : base(pSource, Cst.OTCml_TBL.MARKET, pIdType, pIdentifier, pScanDataEnabled)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrictionMarket"></param>
        /// <param name="pScanDataEnabled"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        // FI 20141107 [20441] Modification de signature
        // PL 20171006 [23469] MARKETTYPE deprecated, use always VW_MARKET_IDENTIFIER (isUseView = true)
        public SQL_Market(string pSource, IDType pIdType, string pIdentifier, RestrictEnum pRestrictionMarket, ScanDataDtEnabledEnum pScanDataEnabled, User pUser, string pSessionId)
            : base(pSource, Cst.OTCml_TBL.VW_MARKET_IDENTIFIER, pIdType, pIdentifier, pScanDataEnabled)
        {
            IsUseView = true; //PL 20171006 [23469] 

            _restrictMarket = pRestrictionMarket;

            _user = pUser;
            _sessionId = pSessionId;
            if (null == _user)
                _user = new User(1, null, RoleActor.SYSADMIN);


            switch (pIdType)
            {
                case IDType.ISO10383_ALPHA4:
                    _ISO10383_ALPHA4_In = pIdentifier;
                    break;
                case IDType.FIXML_SecurityExchange:
                    _FIXML_SecurityExchange_In = pIdentifier;
                    IsUseView = true;
                    break;
                case IDType.SHORT_ACRONYM:
                    _SHORT_ACRONYM_In = pIdentifier;
                    IsUseView = true;
                    break;
            }
        }
        #endregion constructors

        #region Properties
        public string ISO10383_ALPHA4_In
        {
            get { return _ISO10383_ALPHA4_In; }
            set
            {
                if (_ISO10383_ALPHA4_In != value)
                {
                    InitProperty(true);
                    _ISO10383_ALPHA4_In = value;
                }
            }
        }
        public string FIXML_SecurityExchange_In
        {
            get { return _FIXML_SecurityExchange_In; }
            set
            {
                if (_FIXML_SecurityExchange_In != value)
                {
                    InitProperty(true);
                    _FIXML_SecurityExchange_In = value;
                }
            }
        }

        public string SHORT_ACRONYM_In
        {
            get { return _SHORT_ACRONYM_In; }
            set
            {
                if (_SHORT_ACRONYM_In != value)
                {
                    InitProperty(true);
                    _SHORT_ACRONYM_In = value;
                }
            }
        }



        /// <summary>
        /// Obtient la chambre de compensation du marché [MARKET.IDA]
        /// </summary>
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        /// <summary>
        /// Obtient le pays du marché [MARKET.IDCOUNTRY]
        /// </summary>
        public string IdCountry
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRY")); }
        }
        /// <summary>
        /// Obtient le type (OPERATING/SEGMENT) du marché 
        /// </summary>
        // FI 20170928 [23452] la valeur de retour est de type Cst.MarketTypeEnum 
        // PL 20171006 [23469] Original MARKETTYPE deprecated
        public Cst.MarketTypeEnum MarketType
        {
            get { return (this.IDMOPERATING.HasValue) ? Cst.MarketTypeEnum.SEGMENT : Cst.MarketTypeEnum.OPERATING; }
        }
        /// <summary>
        /// Obtient le code du marché [MARKET.ISO10383_ALPHA4]
        /// </summary>
        public string ISO10383_ALPHA4
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISO10383_ALPHA4")); }
        }
        /// <summary>
        /// Obtient le code REUTERS du marché [MARKET.RICCODE]
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code BLOOMBERG du marché [MARKET.BBGCODE]
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        /// <summary>
        /// Obtient le FIXML_SecurityExchange
        /// <para>-si Segment sans MIC dédié : retourne ISO10383_ALPHA4+'-'+EXCHANGESYMBOL</para>
        /// <para>-sinon                     : retourne ISO10383_ALPHA4</para>
        /// </summary>
        // PL 20171006 [23469] MARKETTYPE deprecated, use VW_MARKET_IDENTIFIER view and FIXML_SecurityExchange column
        public string FIXML_SecurityExchange
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FIXML_SecurityExchange")); }
        }

        /// <summary>
        /// Identifiant système de l'éventuel Operating (ou Segment parent)
        /// </summary>
        public Nullable<int> IDMOPERATING
        {
            get 
            {
                object o = GetFirstRowColumnValue("IDMOPERATING");
                if (o == null)
                    return null;
                else
                    return  Convert.ToInt32(o); 
            }
        }
        
        /// <summary>
        /// Obtient l'Acronym du marché [MARKET.ACRONYM]
        /// </summary>
        public string Acronym
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ACRONYM")); }
        }
        /// <summary>
        /// Obtient la City du marché [MARKET.CITY]
        /// </summary>
        public string City
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CITY")); }
        }
        /// <summary>
        /// Obtient l'ExchangeSymbol du marché [MARKET.EXCHANGESYMBOL]
        /// </summary>
        public string ExchangeSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXCHANGESYMBOL")); }
        }
        /// <summary>
        /// Obtient le ShortIdentifier du marché [MARKET.SHORTIDENTIFIER]
        /// </summary>
        public string ShortIdentifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SHORTIDENTIFIER")); }
        }
        /// <summary>
        /// Obtient le BC du marché [MARKET.IDBC]
        /// </summary>
        public string IdBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC")); }
        }

        /// <summary>
        /// Obtient le short acronym du marché [MARKET.SHORTACRONYM]
        /// </summary>
        public string ShortAcronym
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SHORT_ACRONYM")); }
        }

        /// <summary>
        /// Obtient le cutOff appliqué sur les marché ISCOMMODITYSPOT
        /// </summary>
        /// FI 20161214 [21916] Add
        public string CommoditySpotCutOffTime
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CMDTYSPCUTOFFTIME")); }
        }
        /// <summary>
        /// Obtient le TimeZone du marché
        /// </summary>
        /// EG 20170926 [22374] New
        public string TimeZone
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("TIMEZONE"));
            }
        }

        /// <summary>
        /// Jointure sur un groupe de marchés
        /// </summary>
        public Nullable<int> IdGMarket
        {
            get { return _idGMarket; }
            set
            {
                if (_idGMarket != value)
                {
                    InitProperty(false);
                    _idGMarket = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique l'uasage de la vue VW_MARKET_IDENTIFIER comme object principal de la requête 
        /// <para>si true: usage de VW_MARKET_IDENTIFIER</para>
        /// <para>si false: usage de MARKET</para>
        /// </summary>
        public bool IsUseView
        {
            get { return _isUseView; }
            set
            {
                _isUseView = value;

                if (_isUseView)
                {
                    SQLObject = Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString();
                }
                else
                {
                    SQLObject = Cst.OTCml_TBL.MARKET.ToString();
                }
            }
        }

        /// <summary>
        /// Obtient un Id (supposé être unique et qui respecte le type ID de DTD)
        /// <para>Obtient FIXML_SecurityExchange </para>
        /// </summary>
        // FI 20170928 [23452] Add
        public string XmlId
        {
            get
            {
                string ret = this.FIXML_SecurityExchange;
                // L'appel  XMLTools.GetXmlId est normalement sans impact puisque les codes MICs respecte le type ID   
                ret = XMLTools.GetXmlId(ret);
                return ret;
            }
        }

        /// <summary>
        ///  Obtient le format utilisé pour générer les identifiants des DC/Asset ETD
        /// </summary>
        /// FI 20220912 [XXXXX] Add
        public string ETDIdentifierFormat
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDENTIFIERFORMAT"));
            }
        }




        #endregion Properties

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idGMarket = null;
                _ISO10383_ALPHA4_In = null;
                _FIXML_SecurityExchange_In = null;
                _SHORT_ACRONYM_In = null;
            }
            base.InitProperty(pAll);
        }

        #region SetSQLFrom
        /// FI 20141107 [20441] Modify
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (_idGMarket.HasValue)
                AddJoinOnTableMARKETG();

            if ((RestrictEnum.Yes == _restrictMarket) && (_user.IsApplySessionRestrict()))
            {
                SessionRestrictHelper sr = new SessionRestrictHelper(_user, _sessionId, false);
                string sqlFrom = sr.GetSQLMarket(string.Empty, this.SQLObject + ".IDM");
                ConstructFrom(sqlFrom);
            }
        }

        /// <summary>
        /// Ajoute une jointure sur la table MARKETG
        /// </summary>
        /// FI 20141230 [20616] Modify
        protected void AddJoinOnTableMARKETG()
        {
            string sqlFrom = StrFunc.AppendFormat("inner join dbo.MARKETG mg on  (mg.IDM={0}.IDM) and mg.IDGMARKET=@IDGMARKET", this.SQLObject);
            SetDataParameter(new DataParameter(CS, "IDGMARKET", DbType.Int32), _idGMarket.Value.ToString());

            ConstructFrom(sqlFrom);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141230 [20616] Modify
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            /// FI 20141230 [20616] la méthode AddJoinOnTableGMARKET écrit déjà les bonnes jointures
            //if (_idGMarket.HasValue)
            //{
            //    string sqlWhere = "gm.IDGMARKET=@IDGMARKET";
            //    SetDataParameter(new DataParameter(CS, "IDGMARKET", DbType.Int32), _idGMarket.Value.ToString());
            //    ConstructWhere(sqlWhere);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;

            if (ISO10383_ALPHA4_In != null)
            {
                column_In = DataParameter.ParameterEnum.ISO10383_ALPHA4;
                data_In = ISO10383_ALPHA4_In;
            }
            else if (FIXML_SecurityExchange_In != null)
            {
                column_In = DataParameter.ParameterEnum.FIXML_SecurityExchange;
                data_In = FIXML_SecurityExchange_In;
            }
            else if (SHORT_ACRONYM_In != null)
            {
                column_In = DataParameter.ParameterEnum.SHORT_ACRONYM;
                data_In = SHORT_ACRONYM_In;
            }
            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }
        // EG 20171031 [23509] New
        public string CutOffTime(string pFamily)
        {
            if (ProductTools.IsCommoditySpot(pFamily))
            {
                return Convert.ToString(GetFirstRowColumnValue("CMDTYSPCUTOFFTIME"));
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion Methods
    }

    #region SQL_MasterAgreement
    public class SQL_MasterAgreement : SQL_TableWithID
    {
        #region private variable
        private int _idA_1_In = 0;
        private int _idA_2_In = 0;
        #endregion private variable

        #region constructors
        public SQL_MasterAgreement(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_MasterAgreement(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_MasterAgreement(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_MasterAgreement(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.MASTERAGREEMENT, pIdType, pIdentifier, pIsScanDataEnabled) { }
        //
        public SQL_MasterAgreement(string pSource, int pIdA_1, int pIdA_2)
            : this(pSource, pIdA_1, pIdA_2, ScanDataDtEnabledEnum.No) { }
        public SQL_MasterAgreement(string pSource, int pIdA_1, int pIdA_2, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.MASTERAGREEMENT, SQL_TableWithID.IDType.UNDEFINED, string.Empty, pScanDataEnabled)
        {
            IdA_1_In = pIdA_1;
            IdA_2_In = pIdA_2;
        }
        #endregion constructors

        #region public_property_get
        public DateTime DtSignature
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSIGNATURE")); }
        }
        public bool IsWithCloseOut
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISWITHCLOSEOUT")); }
        }
        public string Definition
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DEFINITION")); }
        }
        public int IdA_1
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_1")); }
        }
        public int IdA_2
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_2")); }
        }
        public string AgreementType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("AGREEMENTTYPE")); }
        }
        #endregion public_property_get

        #region public_property_get_set
        public int IdA_1_In
        {
            get
            { return _idA_1_In; }
            set
            {
                if (_idA_1_In != value)
                {
                    InitProperty(true);
                    _idA_1_In = value;
                }
            }
        }
        public int IdA_2_In
        {
            get
            { return _idA_2_In; }
            set
            {
                if (_idA_2_In != value)
                {
                    InitProperty(false);//false pour ne pas faire un reset de _idA_1_In
                    _idA_2_In = value;
                }
            }
        }
        #endregion

        #region private method
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idA_1_In = 0;
                _idA_2_In = 0;
            }
            base.InitProperty();
        }
        #endregion

        #region protected override Method
        protected override void SetSQLWhere()
        {
            string sqlWhere = string.Empty;
            //
            if ((_idA_1_In != 0) && (_idA_2_In != 0))
            {
                sqlWhere = "(";
                sqlWhere += "(" + SQLObject + ".IDA_1=" + _idA_1_In + SQLCst.AND + SQLObject + ".IDA_2=" + _idA_2_In + ")";
                sqlWhere += SQLCst.OR;
                sqlWhere += "(" + SQLObject + ".IDA_2=" + _idA_1_In + SQLCst.AND + SQLObject + ".IDA_1=" + _idA_2_In + ")";
                sqlWhere += ")";
            }
            //
            base.SetSQLWhere(sqlWhere);
        }
        #endregion
    }
    #endregion

    #region SQL_Maturity
    /// <summary>
    /// Get a Maturity of a Derivative Contract
    /// </summary>
    public class SQL_Maturity : SQL_TableWithID
    {
        #region private membres
        private int _idRuleMaturityRule_In;
        #endregion

        #region constructor
        public SQL_Maturity(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No, DateTime.MinValue) { }
        public SQL_Maturity(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum, DateTime.MinValue) { }
        public SQL_Maturity(string pSource, string pIdentifier)
            : this(pSource, IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No, DateTime.MinValue) { }
        public SQL_Maturity(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Identifier, pIdentifier, pScanDataDtEnabledEnum, DateTime.MinValue) { }
        public SQL_Maturity(string pSource, string pIdentifier, DateTime pDtRefForDtEnabled)
            : this(pSource, IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.Yes, pDtRefForDtEnabled) { }
        public SQL_Maturity(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum, DateTime pDtRefForDtEnabled)
            : base(pSource, Cst.OTCml_TBL.MATURITY, pIdType, pIdentifier, pScanDataDtEnabledEnum, pDtRefForDtEnabled)
        {
            //PL 20130604 New Constructor with pDtRefForDtEnabled (Vu)
        }
        #endregion constructor

        #region public_property_get
        public DateTime MaturityDate
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("MATURITYDATE")); }
        }
        public DateTime MaturityDateSys
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("MATURITYDATESYS")); }
        }
        public DateTime DelivryDate
        {
            get
            {
                if (GetFirstRowColumnValue("DELIVERYDATE") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("DELIVERYDATE"));
            }
        }
        public DateTime FirstDelivryDate
        {
            get
            {
                if (GetFirstRowColumnValue("FIRSTDELIVERYDATE") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("FIRSTDELIVERYDATE"));
            }
        }
        public DateTime LastDelivryDate
        {
            get
            {
                if (GetFirstRowColumnValue("LASTDELIVERYDATE") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("LASTDELIVERYDATE"));
            }
        }
        public DateTime FirstDlvSettltDate
        {
            get
            {
                if (GetFirstRowColumnValue("FIRSTDLVSETTLTDATE") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("FIRSTDLVSETTLTDATE"));
            }
        }
        public DateTime LastDlvSettltDate
        {
            get
            {
                if (GetFirstRowColumnValue("LASTDLVSETTLTDATE") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("LASTDLVSETTLTDATE"));
            }
        }
        public DateTime LastTradingDay
        {
            get
            {
                if (GetFirstRowColumnValue("LASTTRADINGDAY") == null)
                    return DateTime.MinValue;
                else
                    return Convert.ToDateTime(GetFirstRowColumnValue("LASTTRADINGDAY"));
            }
        }
        #endregion public_property_get
        //
        #region public_property_get_set
        public int IdMaturityRuleIn
        {
            get { return _idRuleMaturityRule_In; }
            set
            {
                if (_idRuleMaturityRule_In != value)
                {
                    InitProperty(false);
                    _idRuleMaturityRule_In = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override void SetSQLWhere()
        {
            string sqlWhere = null;
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;

            if (Identifier_In != null)
            {
                column_In = DataParameter.ParameterEnum.MATURITYMONTHYEAR;
                data_In = Identifier_In;
            }

            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere = column + SQLCst.LIKE + "@" + dp.ParameterKey;
                else
                    sqlWhere = column + "=@" + dp.ParameterKey;

                SetDataParameter(dp, data_In);
            }

            base.SetSQLWhere(sqlWhere);

            if (IdMaturityRuleIn > 0)
            {
                ConstructWhere(SQLObject + ".IDMATURITYRULE=@IDMATURITYRULE");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDMATURITYRULE), IdMaturityRuleIn);
            }

        }

        #region public override string Identifier
        /// <summary>
        /// Obtient la colonne MATURITYMONTHYEAR
        /// </summary>
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MATURITYMONTHYEAR")); }
        }
        /// <summary>
        /// Obtient string.empty
        /// </summary>
        public override string Description
        {
            get
            {
                return string.Empty; ;
            }
        }
        /// <summary>
        /// Obtient string.empty
        /// </summary>
        public override string ExtlLink
        {
            get
            {
                return string.Empty; ;
            }
        }
        /// <summary>
        /// Obtient string.empty
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return string.Empty; ;
            }
        }

        #endregion
    }
    #endregion SQL_Maturity

    #region SQL_MaturityRule
    /// <summary>
    /// 
    /// </summary>
    public abstract class SQL_MaturityRuleBase : SQL_TableWithID
    {

        #region constructor

        /// <summary>
        /// Lecture le règle de maturité 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pDtRefForDataEnabled"></param>
        /// FI 20220511 [XXXXX] use VW_MATURITYRULE
        protected SQL_MaturityRuleBase(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled, DateTime pDtRefForDataEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_MATURITYRULE, pIdType, pIdentifier, pScanDataEnabled, pDtRefForDataEnabled)
        {
        }
        #endregion

        #region public_property_get
        public string MaturityFormat
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MMYFMT")); }
        }
        public Cst.MaturityMonthYearFmtEnum MaturityFormatEnum
        {
            get { return (Cst.MaturityMonthYearFmtEnum)System.Enum.Parse(typeof(Cst.MaturityMonthYearFmtEnum), MaturityFormat); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220511 [XXXXX] Add
        public string MaturityRule
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MATURITYRULE")); }

        }


        public string MaturityMMYRule
        {
            get
            {
                if (GetFirstRowColumnValue("MMYRULE") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("MMYRULE"));
            }
        }
        public bool IsMaturityMMYRule_Multiple
        {
            //ex.: "mFGJKNQVX|qldHMUZ" on "Monthly & Quaterly - CBOE®"
            get { return MaturityMMYRule.Contains("|"); }
        }
        /// <summary>
        /// Retourne TRUE si règle multiple contenant règle mensuelle (m)
        /// </summary>
        public bool IsMaturityMMYRule_Multiple_MonthlyInclude
        {
            get
            {
                string[] maturityRules = MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("m"))
                        return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Retourne TRUE si règle multiple contenant règle trimestrielle (q)
        /// </summary>
        public bool IsMaturityMMYRule_Multiple_QuaterlyInclude
        {
            get
            {
                string[] maturityRules = MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("q"))
                        return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Retourne TRUE si règle multiple contenant règle hebdomadaire (w)
        /// </summary>
        public bool IsMaturityMMYRule_Multiple_WeeklyInclude
        {
            get
            {
                string[] maturityRules = MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("w"))
                        return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Retourne la section MONTH (m) dans le cas d'une règle multiple
        /// <para>
        /// Ex. Si RULE="w_fri|m=FGJKNQVX|q_lbd=HMUZ", alors retourne "m=FGJKNQVX"
        /// </para>
        /// </summary>
        public string MaturityMMYRule_Multiple_Monthly
        {
            get
            {
                string[] maturityRules = MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("m"))
                        return maturityRule;
                }
                return null;
            }
        }
        /// <summary>
        /// Retourne la section QUARTER (q) dans le cas d'une règle multiple
        /// <para>
        /// Ex. Si RULE="w_fri|m=FGJKNQVX|q_lbd=HMUZ", alors retourne "q_lbd=HMUZ"
        /// </para>
        /// </summary>
        public string MaturityMMYRule_Multiple_Quaterly
        {
            get
            {
                string[] maturityRules = MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("q"))
                        return maturityRule;
                }
                return null;
            }
        }
        /// <summary>
        /// Retourne la section WEEKLY (w) dans le cas d'une règle multiple
        /// <para>
        /// Ex. Si RULE="w_fri|m=FGJKNQVX|q_lbd=HMUZ", alors retourne "w_fri"
        /// </para>
        /// </summary>
        public string MaturityMMYRule_Multiple_Weekly
        {
            get
            {
                string[] maturityRules = this.MaturityMMYRule.Split("|".ToCharArray());
                foreach (string maturityRule in maturityRules)
                {
                    if (maturityRule.StartsWith("w"))
                        return maturityRule;
                }
                return null;
            }
        }
        /// <summary>
        /// Retourne TRUE si règle journalière (d) ou règle "vide" 
        /// </summary>
        public bool IsMaturityMMYRule_Daily
        {
            get { return (this.MaturityMMYRule == "d") || String.IsNullOrEmpty(this.MaturityMMYRule); }
        }
        /// <summary>
        /// Retourne TRUE si règle hebdomadaire (w) ou (w124) ou (w1245).
        /// </summary>
        public bool IsMaturityMMYRule_Weekly
        {
            //get { return (this.MaturityMMYRule == "w"); } // FI 20220622 [XXXXX] Modification du fait de l'existence de w124, w1245, etc 
            get { return (this.MaturityMMYRule.StartsWith("w")); }
        }
        /// <summary>
        /// Retourne TRUE si règle non multiple et non journalière (d) et non hebdomadaire (w) ou (w124) ou (w1245).
        /// </summary>
        public bool IsMaturityMMYRule_Monthly
        {
            get { return (!this.IsMaturityMMYRule_Multiple) && (!this.IsMaturityMMYRule_Daily) && (!this.IsMaturityMMYRule_Weekly); }
        }

        /// <summary>
        ///  S'applique aux échéances au format YYYYMMDD. Permet de vérifier la validité de l'échéance vis à vis des règles présentes pour obtenir la date d'échéance ou la date LTD  
        /// </summary>
        /// FI 20230126 [XXXXX] Add
        public Nullable<Cst.MaturityRelativeTo> MaturityRelativeTo
        {
            get
            {
                if (GetFirstRowColumnValue("MMYRELATIVETO") == null)
                    return null;
                else
                    return ReflectionTools.ConvertStringToEnum<Cst.MaturityRelativeTo>(Convert.ToString(GetFirstRowColumnValue("MMYRELATIVETO")));
            }
        }

        public Nullable<int> MaturityOpenSimultaneously
        {
            get
            {
                if (GetFirstRowColumnValue("OPENSIMULTANEOUSLY") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("OPENSIMULTANEOUSLY"));
            }
        }

        public string MaturityRollConv
        {
            get
            {
                if (GetFirstRowColumnValue("ROLLCONVMMY") == null)
                    return string.Empty;
                else
                {
                    string ret = Convert.ToString(GetFirstRowColumnValue("ROLLCONVMMY"));
                    return ret == "ICEUCO2EMISSIONSFUT" ? RollConventionEnum.CO2EMISSIONSFUT.ToString() : ret;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20230123 [XXXXX] Add
        public RollConventionEnum? MaturityRollConvEnum
        {
            get
            {
                if (StrFunc.IsFilled(MaturityRollConv))
                    return StringToEnum.RollConvention(MaturityRollConv);
                else
                    return null;
            }
        }
        public string MaturityMonthReference
        {
            get
            {
                if (GetFirstRowColumnValue("MONTHREF") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("MONTHREF"));
            }
        }
        public string MaturityBusinessDayConv
        {
            get
            {
                if (GetFirstRowColumnValue("BDC_MDT") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("BDC_MDT"));
            }
        }
        public BusinessDayConventionEnum MaturityRollConvBusinessDayConv
        {
            get
            {
                object obj = GetFirstRowColumnValue("BDC_ROLLCONV_MDT");
                if (obj == null)
                {
                    return BusinessDayConventionEnum.NONE;
                }
                else
                {
                    return StringToEnum.BusinessDayConvention(Convert.ToString(obj), BusinessDayConventionEnum.NONE);
                }
            }
        }
        public string MaturityBusinessCenter
        {
            get
            {
                if (GetFirstRowColumnValue("IDBCMDT") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("IDBCMDT"));
            }
        }
        //FL/PL 20120704 Newness
        public bool IsApplyMaturityBusinessCenterOnOffsetCalculation
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISIDBCMDTONOFFSET")); }
        }
        public Nullable<int> MaturityOffsetMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPMDTOFFSET") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPMDTOFFSET"));
            }
        }
        public string MaturityOffsetPeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMDTOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODMDTOFFSET"));
            }
        }
        public string MaturityOffsetDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPEMDTOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPEMDTOFFSET"));
            }
        }
        public Nullable<int> MaturityLastTrdDayOffsetMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPLASTRADINGDAYOFFSET") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPLASTRADINGDAYOFFSET"));
            }
        }
        public string MaturityLastTrdDayOffsetPeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODLASTRADINGDAYOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODLASTRADINGDAYOFFSET"));
            }
        }
        public string MaturityLastTrdDayOffsetDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPELASTRADINGDAYOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPELASTRADINGDAYOFFSET"));
            }
        }
        public Nullable<int> MaturityDelivryDateOffsetMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPDELIVERYDATEOFFSET") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPDELIVERYDATEOFFSET"));
            }
        }
        public string MaturityDelivryDateOffsetPeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODDELIVERYDATEOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODDELIVERYDATEOFFSET"));
            }
        }
        public string MaturityDelivryDateOffsetDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPEDELIVERYDATEOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPEDELIVERYDATEOFFSET"));
            }
        }
        // -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // PL 20170217 - PERIODIC DELIVERY -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        public string MaturityPeriodicFirstDelivryDateRollConv
        {
            get
            {
                if (GetFirstRowColumnValue("ROLLCONVFIRSTDELIVERY") == null)
                {
#if DEBUG
                    return RollConventionEnum.DAY1.ToString();
#else
                    return null;
#endif
                }
                else
                    return Convert.ToString(GetFirstRowColumnValue("ROLLCONVFIRSTDELIVERY"));
            }
        }
        public string MaturityPeriodicLastDelivryDateRollConv
        {
            get
            {
                if (GetFirstRowColumnValue("ROLLCONVLASTDELIVERY") == null)
                {
#if DEBUG
                    return RollConventionEnum.EOM.ToString();
#else
                    return null;
#endif
                }

                else
                    return Convert.ToString(GetFirstRowColumnValue("ROLLCONVLASTDELIVERY"));
            }
        }
        public Nullable<int> MaturityPeriodicDeliveryDateMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPDELIVERY") == null)
                    return 1;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPDELIVERY"));
            }
        }
        public string MaturityPeriodicDeliveryDatePeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODDELIVERY") == null)
                    return PeriodEnum.D.ToString();
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODDELIVERY"));
            }
        }
        public string MaturityPeriodicDeliveryDateDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPEDELIVERY") == null)
                    return DayTypeEnum.Calendar.ToString();
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPEDELIVERY"));
            }
        }
        public string MaturityPeriodicDeliveryDateRollConv
        {
            get
            {
                if (GetFirstRowColumnValue("ROLLCONVDELIVERY") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("ROLLCONVDELIVERY"));
            }
        }
        public string MaturityPeriodicDeliveryDateTimeStart
        {
            get
            {
                if (GetFirstRowColumnValue("DELIVERYTIMESTART") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMESTART"));
            }
        }
        public string MaturityPeriodicDeliveryDateTimeEnd
        {
            get
            {
                if (GetFirstRowColumnValue("DELIVERYTIMEEND") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMEEND"));
            }
        }
        public string MaturityPeriodicDeliveryDateTimeZone
        {
            get
            {
                if (GetFirstRowColumnValue("DELIVERYTIMEZONE") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMEZONE"));
            }
        }
        public bool MaturityPeriodicDeliveryDateApplySummerTime
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISAPPLYSUMMERTIME")); }
        }
        public Nullable<int> MaturityPeriodicDlvSettltDateOffsetMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPDLVSETTLTOFFSET") == null)
                {
#if DEBUG
                    return -2;
#else
                    return 0;
#endif

                }
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPDLVSETTLTOFFSET"));
            }
        }
        public string MaturityPeriodicDlvSettltDateOffsetPeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODDLVSETTLTOFFSET") == null)
                    return PeriodEnum.D.ToString();
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODDLVSETTLTOFFSET"));
            }
        }
        public string MaturityPeriodicDlvSettltDateOffsetDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPEDLVSETTLTOFFSET") == null)
                    return DayTypeEnum.ExchangeBusiness.ToString();
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPEDLVSETTLTOFFSET"));
            }
        }
        public string MaturityPeriodicDlvSettltHolidayConv
        {
            get
            {
                if (GetFirstRowColumnValue("SETTLTOFHOLIDAYDLVCONVENTION") == null)
                    return "FOLLOWING";
                else
                    return Convert.ToString(GetFirstRowColumnValue("SETTLTOFHOLIDAYDLVCONVENTION"));
            }
        }
        // -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        #endregion public_property_get
        #region public override string Identifier
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }

        /// <summary>
        /// Retourne true s'il existe un paramétrage permettant d'obtenir la LTD (LastTradingDay) à partir de l'EXP (Expiry/Maturity) 
        /// </summary>
        /// <returns></returns>
        public bool IsExistLastTrdDayOffset
        {
            get
            {
                bool ret = (null != this.MaturityLastTrdDayOffsetMultiplier)
                       && StrFunc.IsFilled(this.MaturityLastTrdDayOffsetPeriod)
                       && StrFunc.IsFilled(this.MaturityLastTrdDayOffsetDaytype);
                return ret;
            }
        }
        #endregion


        /// <summary>
        ///  Obtient true s'il existe un paramétrage "livraison non périodique"
        ///  <para>
        ///  Obtient true s'il n'existe aucun paramétrage concernant les livraisons (Valeur par défaut)
        ///  </para>
        /// </summary>
        /// FI 20190719 [24695] add Property
        public Boolean IsNoPeriodicDeliveryExtend
        {
            get
            {
                bool ret = IsNoPeriodicDelivery;
                if ((!ret) && (!IsPeriodicDelivery))
                    ret = true;

                return ret;
            }
        }

        /// <summary>
        /// Retourne true s'il existe un paramétrage "livraison non périodique"
        /// </summary>
        /// <returns></returns>
        public bool IsNoPeriodicDelivery
        {
            get
            {
                bool ret = (null != this.MaturityDelivryDateOffsetMultiplier)
                           && StrFunc.IsFilled(this.MaturityDelivryDateOffsetPeriod)
                           && StrFunc.IsFilled(this.MaturityDelivryDateOffsetDaytype);
                return ret;
            }
        }

        /// <summary>
        /// Retourne true s'il existe un paramétrage "livraison périodique"
        /// </summary>
        /// <returns></returns>
        public bool IsPeriodicDelivery
        {
            get
            {
                bool ret = StrFunc.IsFilled(this.MaturityPeriodicFirstDelivryDateRollConv)
                       && StrFunc.IsFilled(this.MaturityPeriodicLastDelivryDateRollConv);
                return ret;
            }
        }

        /// <summary>
        /// Obtient le cycle 
        /// </summary>
        /// FI 20220613 [XXXXX] Add
        public string FrequencyMaturity
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FREQUENCYMATURITY")); }
        }

    }

    /// <summary>
    /// Recherche d'une règle de maturité active à une date
    /// <para>Pour obtenir la règle de maturité à partir de son Id non significatif (IDMATURITYRULE) et sans se soucier si elle est active, l'utilisation de la classe <seealso cref="SQL_MaturityRuleRead"/> est obligatoire</para>
    /// </summary>
    public class SQL_MaturityRuleActive : SQL_MaturityRuleBase
    {
        #region constructor

        /// <summary>
        /// Recherche la règle de maturité active en <paramref name="pDtRefForDataEnabled"/>
        /// <para>Attention, potentiellement Spheres® ne charge pas la règle de maturité dont l'identifiant non significatif (IDMATURITYRULE) est <paramref name="pId"/></para>
        /// <para>Spheres charge la règle de maturité active en <paramref name="pDtRefForDataEnabled"/> ayant l'identifiant identique à celui de la règle de maturité dont l'identifiant non significatif (IDMATURITYRULE) est <paramref name="pId"/></para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        /// <param name="pDtRefForDataEnabled">Lorsque non renseignée prise en considération de la date système</param>
        public SQL_MaturityRuleActive(string pSource, int pId, DateTime pDtRefForDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pDtRefForDataEnabled) { }

        /// <summary>
        /// Recherche la règle de maturité active en <paramref name="pDtRefForDataEnabled"/> et dont l'identifiant est <paramref name="pIdentifier"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pDtRefForDataEnabled">Lorsque non renseignée prise en considération de la date système</param>
        public SQL_MaturityRuleActive(string pSource, string pIdentifier, DateTime pDtRefForDataEnabled)
            : this(pSource, IDType.Identifier, pIdentifier.ToString(), pDtRefForDataEnabled) { }

        /// <summary>
        /// Lecture le règle de maturité active en <paramref name="pDtRefForDataEnabled"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pDtRefForDataEnabled"></param>
        /// FI 20220511 [XXXXX] use VW_MATURITYRULE
        private SQL_MaturityRuleActive(string pSource, IDType pIdType, string pIdentifier, DateTime pDtRefForDataEnabled)
            : base(pSource, ConvertIDType(pIdType), ConvertIdentifier(pSource, pIdType, pIdentifier), ScanDataDtEnabledEnum.Yes, pDtRefForDataEnabled) { }

        #endregion constructor

        private static IDType ConvertIDType(IDType pIdType)
        {
            //PL 20131112 [TRIM 19164]
            //Recherche systématique des MR sur la base de leur IDENTIFIER, afin d'exploiter la possibilité d'avoir plusieurs MR de même nom (avec DtEnabled/DtDisabled différents).
            if (pIdType == IDType.Id)
            {
                pIdType = IDType.Identifier;
            }
            return pIdType;
        }
        /// <summary>
        /// Lorsque {pIdentifier} est de type integer (pIdType == IDType.Id), retourne l'identifier associé à l'IDMATURITYRULE
        /// Lorsque {pIdentifier} est de type string , retourne {pIdentifier}
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        private static string ConvertIdentifier(string pSource, IDType pIdType, string pIdentifier)
        {
            //PL 20131112 [TRIM 19164]
            //Recherche systématique des MR sur la base de leur IDENTIFIER, afin d'exploiter la possibilité d'avoir plusieurs MR de même nom (avec DtEnabled/DtDisabled différents).
            if (pIdType == IDType.Id)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pSource, "IDMATURITYRULE", DbType.Int32), Convert.ToInt32(pIdentifier));

                StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                sql += "IDENTIFIER" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITYRULE.ToString() + Cst.CrLf;
                sql += SQLCst.WHERE + "IDMATURITYRULE=@IDMATURITYRULE";
                // FI 20200424 [XXXXX] Use ExecuteDataTable and CreateDataReader for cacheOn use
                using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pSource), sql.ToString(), parameters.GetArrayDbParameter()).CreateDataReader())
                {
                    if (dr.Read())
                        pIdentifier = dr["IDENTIFIER"].ToString();
                }
            }
            return pIdentifier;
        }
    }

    /// <summary>
    /// Lecture d'une règle de maturité à partir de son Id non significatif (IDMATURITYRULE). Lecture de la règle même si elle a été désactivée. 
    /// <para>Pour obtenir la règle de maturité valide à une date donnée, l'utilisation de la classe <seealso cref="SQL_MaturityRuleActive"/> est obligatoire</para>
    /// </summary>
    public class SQL_MaturityRuleRead : SQL_MaturityRuleBase
    {
        /// <summary>
        /// Lecture le règle de maturité dont ID non significatif est <paramref name="pId"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId">Représente un IDMATURITYRULE</param>
        public SQL_MaturityRuleRead(string pSource, int pId)
        : base(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No, DateTime.MinValue) { }
    }

    #endregion SQL_MaturityRule
    #region SQL_NetConvention
    public class SQL_NetConvention : SQL_TableWithIDAndInstrCriteria
    {
        #region constructors
        public SQL_NetConvention(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_NetConvention(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_NetConvention(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_NetConvention(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, pScanDataEnabled) { }
        public SQL_NetConvention(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            :
            base(pSource, Cst.OTCml_TBL.NETCONVENTION, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region Properties
        public int IdMasterAgreement
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDMASTERAGREEMENT")); }
        }
        public string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }
        #endregion Properties
    }
    #endregion

    #region SQL_NetDesignation
    public class SQL_NetDesignation : SQL_TableWithID
    {
        #region constructors
        public SQL_NetDesignation(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_NetDesignation(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_NetDesignation(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_NetDesignation(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, pScanDataEnabled) { }
        public SQL_NetDesignation(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            :
            base(pSource, Cst.OTCml_TBL.NETDESIGNATION, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region Properties
        public int IdA_1
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_1")); }
        }
        public int IdA_2
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_2")); }
        }
        public string Definition
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DEFINITION")); }
        }
        #endregion Properties
    }
    #endregion

    #region SQL_ConfirmationMessage
    public class SQL_ConfirmationMessage : SQL_TableWithID
    {
        #region constructors
        public SQL_ConfirmationMessage(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_ConfirmationMessage(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_ConfirmationMessage(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_ConfirmationMessage(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, pScanDataEnabled) { }
        public SQL_ConfirmationMessage(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.CNFMESSAGE, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region Properties
        public string CnfType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CNFTYPE")); }
        }
        public string MsgType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MSGTYPE")); }
        }
        public string IdStBusiness
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTBUSINESS")); }
        }
        //20140710 PL [TRIM 20179]
        public string IdStMatch
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTMATCH")); }
        }
        public string IdStCheck
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTCHECK")); }
        }
        public string TypeContract
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TYPECONTRACT")); }
        }
        public int IdContract
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDCONTRACT")); }
        }
        public NotificationStepLifeEnum StepLifeEnum
        {
            get { return (NotificationStepLifeEnum)Enum.Parse(typeof(NotificationStepLifeEnum), Convert.ToString(GetFirstRowColumnValue("STEPLIFE")), true); }
        }
        public string XslFile
        {
            get { return Convert.ToString(GetFirstRowColumnValue("XSLTFILE")); }
        }
        public string EventCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTCODE")); }
        }
        public string EventType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTTYPE")); }
        }
        public string EventClass
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTCLASS")); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20120731 Dans le référentiel la colonne EVENTCODE est nullable
        public Nullable<EventCodeEnum> EventCodeEnum
        {
            get
            {
                Nullable<EventCodeEnum> ret = null;
                if (StrFunc.IsFilled(EventCode))
                    ret = (EventCodeEnum)Enum.Parse(typeof(EventCodeEnum), EventCode);
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20120731 Dans le référentiel la colonne EVENTCLASS est nullable
        public Nullable<EventClassEnum> EventClassEnum
        {
            get
            {
                Nullable<EventClassEnum> ret = null;
                if (StrFunc.IsFilled(EventClass))
                    ret = (EventClassEnum)Enum.Parse(typeof(EventClassEnum), EventClass);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20120731 add eventTypeEnum
        public Nullable<EventTypeEnum> EventTypeEnum
        {
            get
            {
                Nullable<EventTypeEnum> ret = null;
                if (StrFunc.IsFilled(EventType))
                    ret = (EventTypeEnum)Enum.Parse(typeof(EventTypeEnum), EventType);
                return ret;
            }
        }



        public int PeriodMltpMsgIssue
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPMSGISSUE"));
            }
        }
        public string PeriodMsgIssue
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("PERIODMSGISSUE"));
            }
        }
        public PeriodEnum PeriodMsgEnum
        {
            get { return StringToEnum.Period(PeriodMsgIssue); }
        }
        public string DayTypeMsgIssue
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("DAYTYPEMSGISSUE"));
            }
        }
        public DayTypeEnum DayTypeMsgEnum
        {
            get { return StringToEnum.DayType(DayTypeMsgIssue); }
        }
        public bool IsUseChildEvents
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISUSECHILDEVENTS")); }
        }
        public bool IsUseEventSi
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISUSEEVENTSI")); }
        }
        public bool IsUseNotepad
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISUSENOTEPAD")); }
        }
        #endregion Properties
    }
    #endregion

    #region SQL_InvoicingRules
    public class SQL_InvoicingRules : SQL_TableWithID
    {
        #region Accessors
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public int IdB
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDB")); }
        }
        public int IdAInvoiced
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_INVOICED")); }
        }
        public int IdATrader
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_TRADER")); }
        }
        public string TraderIdentifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER1")); }
        }
        public string IdC_Fee
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC_FEE")); }
        }
        public string PaymentType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PAYMENTTYPE")); }
        }
        public string EventType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTTYPE")); }
        }
        #endregion Accessors
        #region Constructors
        public SQL_InvoicingRules(string pSource, int pIdInvoicingRules) : base(pSource, Cst.OTCml_TBL.INVOICINGRULES, pIdInvoicingRules) { }
        public SQL_InvoicingRules(string pSource, string pIdentifier) : base(pSource, Cst.OTCml_TBL.INVOICINGRULES, IDType.Identifier, pIdentifier) { }
        #endregion Constructors

        #region Methods
        #region SetSQLFrom
        protected override void SetSQLFrom()
        {
            //
            base.SetSQLFrom();
            string sqlFrom = OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_TRADER", "ac", false) + Cst.CrLf;
            ConstructFrom(sqlFrom);
        }
        #endregion SetSQLFrom
        #endregion Methods
    }
    #endregion SQL_InvoicingRules

    #region SQL_Ncs
    public class SQL_Ncs : SQL_ActorRef
    {

        #region constructor
        public SQL_Ncs(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.NCS, pId) { }
        public SQL_Ncs(string pSource, int pId, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.NCS, pId, pScan) { }
        public SQL_Ncs(string pSource, string pIdentifier)
            : base(pSource, Cst.OTCml_TBL.NCS, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Ncs(string pSource, string pIdentifier, ScanDataDtEnabledEnum pScan)
            : base(pSource, Cst.OTCml_TBL.NCS, pIdentifier, pScan) { }
        public SQL_Ncs(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            :
            base(pSource, Cst.OTCml_TBL.NCS, pIdType, pIdentifier, pIsScanDataEnabled) { }

        #endregion constructor

        #region IdIOTask
        public int IdIOTask
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDIOTASK")); }
        }
        #endregion constructor

    }
    #endregion SQL_Ncs


    /// <summary>
    /// 
    /// </summary>
    public class SQL_Product : SQL_TableWithID
    {
        public const string LIST_OF_FXPRODUCT = "fxSingleLeg;fxSwap;FxDCS;FxFixing;FxForward;FxNDF;FxSpot;";

        #region constructor
        public SQL_Product(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Product(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum) { }
        public SQL_Product(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Product(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource, Cst.OTCml_TBL.PRODUCT, pIdType, pIdentifier, pScanDataDtEnabledEnum)
        { }
        #endregion

        #region public property




        /// <summary>
        /// 
        /// </summary>
        public FungibilityModeEnum FungibilityMode
        {
            get
            {
                FungibilityModeEnum _fungibilityMode = (FungibilityModeEnum)ReflectionTools.EnumParse(new FungibilityModeEnum(),
                    Convert.ToString(GetFirstRowColumnValue("FUNGIBILITYMODE")));
                return _fungibilityMode;
            }
        }

        /// <summary>
        ///  Obtient True si le priduit est fongible
        /// </summary>
        /// FI 20170116 [21916] Add
        public bool IsFungible
        {
            get { return this.FungibilityMode != FungibilityModeEnum.NONE; }
        }

        public string CssFileName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSFILENAME")); }
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CssColor
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CSSFILENAME")); }
        }
        public bool IsDerivative
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISDERIVATIVE")); }
        }
        public string Source
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SOURCE")); }
        }
        public string Class
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CLASS")); }
        }
        public string Family
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FAMILY")); }
        }
        public string GProduct
        {
            get { return Convert.ToString(GetFirstRowColumnValue("GPRODUCT")); }
        }
        public ProductTools.GroupProductEnum GroupProduct
        {
            get { return (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), GProduct); }
        }
        public ProductTools.FamilyEnum FamilyProduct
        {
            get { return (ProductTools.FamilyEnum)ReflectionTools.EnumParse(new ProductTools.FamilyEnum(), Family); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "COMS"
        /// <para>Cela concerne les instruments commoditySpot</para>
        /// </summary>
        public bool IsCOMS
        {
            get { return (Family.Trim() == Cst.ProductFamily_COMS); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "LSD"
        /// <para>Cela concerne les instruments listés et les stratégies homogènes sur instruments listés</para>
        /// </summary>
        public bool IsLSD
        {
            get { return (Family.Trim() == Cst.ProductFamily_LSD); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "IRD"
        /// <para>Cela concerne les product: bulletPayment, capFloor, fra, swap, swaption et loanDeposit</para>
        /// </summary>
        public bool IsIRD
        {
            get { return (Family.Trim() == Cst.ProductFamily_IRD); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "BO"
        /// <para>Cela concerne le product: bondOption</para>
        /// </summary>
        // EG 20150410 [20513] BANCAPERTA
        public bool IsBO
        {
            get { return (Family.Trim() == Cst.ProductFamily_BO); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "ESE"
        /// </summary>
        public bool IsESE
        {
            get { return (Family.Trim() == Cst.ProductFamily_ESE); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "RTS"
        /// </summary>
        public bool IsRTS
        {
            get { return (Family.Trim() == Cst.ProductFamily_RTS); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.FAMILY = "FX"
        /// <para>Cela concerne les instruments Fx et les stratégies homogènes sur instruments Fx  </para>
        /// </summary>
        public bool IsFx
        {
            get { return (Family.Trim() == Cst.ProductFamily_FX); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.GPRODUCT = ADM
        /// </summary>
        public bool IsAdministrativeProduct
        {
            get { return (Cst.ProductGProduct_ADM == GProduct); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.GPRODUCT = RISK
        /// </summary>
        public bool IsRiskProduct
        {
            get { return (Cst.ProductGProduct_RISK == GProduct); }
        }
        ///// <summary>
        ///// Retourne true si PRODUCT.GPRODUCT = LSD (ETD)
        ///// </summary>
        //public bool IsExchangeTradedDerivativeProduct
        //{
        //    get { return (Cst.ProductFamily_LSD == GProduct); }
        //}
        /// <summary>
        /// Retourne true si PRODUCT.GPRODUCT = ASSET
        /// </summary>
        public bool IsAssetProduct
        {
            get { return (Cst.ProductGProduct_ASSET == GProduct); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFxAndNotOption
        {
            get
            {
                bool ret = LIST_OF_FXPRODUCT.IndexOf(Identifier + ";") >= 0;
                return ret;
            }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = bulletPayment
        /// </summary>
        public bool IsBulletPayment
        {
            get { return (Identifier.Trim().ToUpper() == "bulletPayment".ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = exchangeTradedDerivative
        /// </summary>
        public bool IsExchangeTradedDerivative
        {
            get { return (Identifier.Trim().ToUpper() == "exchangeTradedDerivative".ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = capFloor
        /// </summary>
        public bool IsCapFloor
        {
            get { return (Identifier.Trim().ToUpper() == "capFloor".ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fra
        /// </summary>
        public bool IsFra
        {
            get { return (Identifier.Trim().ToUpper() == "fra".ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = swap
        /// </summary>
        public bool IsSwap
        {
            get { return (Identifier.Trim().ToUpper() == "swap".ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = swaption
        /// </summary>
        public bool IsSwaption
        {
            get { return (Identifier.Trim().ToUpper() == "swaption".ToUpper()); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxSwap
        /// </summary>
        public bool IsFxSwap
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxSwap.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxAverageRateOption
        /// </summary>
        public bool IsFxAverageRateOption
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxAverageRateOption.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxBarrierOption
        /// </summary>
        public bool IsFxBarrierOption
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxBarrierOption.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxDigitalOption
        /// </summary>
        public bool IsFxDigitalOption
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxDigitalOption.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxSimpleOption
        /// </summary>
        public bool IsFxSimpleOption
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxSimpleOption.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = fxSingleLeg
        /// </summary>
        public bool IsFxSingleLeg
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxSingleLeg.ToUpper()); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = equitySecurityTransaction
        /// </summary>
        public bool IsEquitySecurityTransaction
        {
            get { return (Identifier.Trim().ToUpper() == "equitySecurityTransaction".ToUpper()); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = debtSecurityTransaction
        /// </summary>
        public bool IsDebtSecurityTransaction
        {
            get { return (Identifier.Trim().ToUpper() == "debtSecurityTransaction".ToUpper()); }
        }

        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = commoditySpot
        /// </summary>
        // EG 20161122 New Commodity Derivative
        public bool IsCommoditySpot
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductCommoditySpot.ToUpper()); }
        }
        /// <summary>
        /// Retourne true si PRODUCT.IDENTIFIER = commoditySwap
        /// </summary>
        // EG 20161122 New Commodity Derivative
        public bool IsCommoditySwap
        {
            get { return (Identifier.Trim().ToUpper() == Cst.ProductFxBarrierOption.ToUpper()); }
        }

        #endregion public property
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQL_RateIndex : SQL_TableWithID
    {
        public new enum IDType
        {
            Id,
            Identifier,
            ExtLink,
            IdISDA,
        }

        #region private variable
        private string _idisda_In;
        //
        private Cst.IndexSelfCompounding _withInfoSelfCompounding = Cst.IndexSelfCompounding.NONE;
        #endregion private variable

        #region constructors
        public SQL_RateIndex(string pSource, int pId)
            : this(pSource, SQL_RateIndex.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_RateIndex(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, SQL_RateIndex.IDType.Id, pId.ToString(), pScanDataDtEnabledEnum) { }
        public SQL_RateIndex(string pSource, SQL_RateIndex.IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        // PM 20120826 [18058] : remplacement de Cst.OTCml_TBL.INSTRUMENT par Cst.OTCml_TBL.RATEINDEX
        public SQL_RateIndex(string pSource, SQL_RateIndex.IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource, Cst.OTCml_TBL.RATEINDEX,
            (pIdType == SQL_RateIndex.IDType.IdISDA) ?
            SQL_TableWithID.IDType.UNDEFINED : (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), pIdType.ToString()),
            pIdentifier, pScanDataDtEnabledEnum)
        { }
        #endregion constructors

        #region public_property_get
        protected override bool IsUseOrderBy
        {
            get
            {
                //Critère sur la PK ou
                //Critère sur IDISDA sans "%" 
                //--> La query retournera 1 ou 0 record  
                return ((!base.IsUseOrderBy) && (StrFunc.ContainsIn(_idisda_In, "%")));
            }
        }
        //
        public string IdIsda
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["IDISDA"]);
                else
                    return string.Empty;
            }
        }
        public string DayCountFraction
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["DCF"]);
                else
                    return null;
            }
        }
        public DayCountFractionEnum FpML_Enum_DayCountFraction
        {
            get { return StringToEnum.DayCountFraction(DayCountFraction); }
        }
        // PM 20120826 [18058] : rename de BusinessDayConvention to CalcPeriodBusinessDayConvention
        public string CalcPeriodBusinessDayConvention
        {
            get
            {
                if (IsLoaded)
                    // PM 20120826 [18058] : replace BDC by BDC_CALCPERIOD
                    return Convert.ToString(Dt.Rows[0]["BDC_CALCPERIOD"]);
                else
                    return null;
            }
        }
        // PM 20120826 [18058] : rename de FpML_Enum_BusinessDayConvention to FpML_Enum_CalcPeriodBusinessDayConvention
        public BusinessDayConventionEnum FpML_Enum_CalcPeriodBusinessDayConvention
        {
            get { return StringToEnum.BusinessDayConvention(CalcPeriodBusinessDayConvention); }
        }
        public string DayTypeFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["DAYTYPEFIXINGOFFSET"]);
                else
                    return null;
            }
        }
        public DayTypeEnum FpML_Enum_DayTypeFixingOffset
        {
            get { return StringToEnum.DayType(DayTypeFixingOffset); }
        }
        public string PeriodFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["PERIODFIXINGOFFSET"]);
                else
                    return null;
            }
        }
        public PeriodEnum FpML_Enum_PeriodFixingOffset
        {
            get { return StringToEnum.Period(PeriodFixingOffset); }
        }
        public int PeriodMlptFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["PERIODMLTPFIXINGOFFSET"]);
                else
                    return 0;
            }
        }
        public string BusinessCenter
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["IDBC"]);
                else
                    return null;
            }
        }
        public string BusinessCenterAdditional
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["IDBCADDITIONAL"]);
                else
                    return null;
            }
        }
        public bool IsSelfCompounding
        {
            get
            {
                bool isSelfCompounding = false;
                if (IsLoaded)
                    isSelfCompounding = (Dt.Rows[0]["CALCULATIONRULE"].ToString() == Cst.CalculationRule_SELFCOMPOUNDING);
                return isSelfCompounding;
            }
        }

        #endregion
        #region public_property_get_set
        public Cst.IndexSelfCompounding WithInfoSelfCompounding
        {
            get { return _withInfoSelfCompounding; }
            set
            {
                if (_withInfoSelfCompounding != value)
                {
                    InitProperty(false);
                    _withInfoSelfCompounding = value;
                }
            }
        }


        public string IdIsda_In
        {
            get { return _idisda_In; }
            set
            {
                if (_idisda_In != value)
                {
                    InitProperty(true);
                    _idisda_In = value;
                }
            }
        }
        #endregion

        #region public override
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idisda_In = null;
            }
            base.InitProperty();
        }

        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            //
            string sqlWhereIdIsda = string.Empty;
            if (_idisda_In != null)
                sqlWhereIdIsda = DataHelper.SQLUpper(CS, "IDISDA") + DataHelper.SQLLike(_idisda_In, CompareEnum.Upper);
            //
            ConstructWhere(sqlWhereIdIsda);
        }

        protected override void SetSQLOrderBy()
        {
            base.SetSQLOrderBy();
            //
            string sqlColumOrderBy = string.Empty;
            string orderBy = string.Empty;
            //
            if (this.IsUseOrderBy)
            {
                if (_idisda_In != null)
                    sqlColumOrderBy = "IDISDA";

                //Statistics: table xxx_S
                if (EfsObject.IsWithStatistic)
                    orderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, EfsObject.ObjectName, sqlColumOrderBy);
                else
                    orderBy = sqlColumOrderBy;
            }
            ConstructOrderBy(orderBy);
        }

        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            //
            Cst.OTCml_TBL tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_CF;
            switch (WithInfoSelfCompounding)
            {
                case Cst.IndexSelfCompounding.CASHFLOW:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_CF;
                    break;
                case Cst.IndexSelfCompounding.ACCRUEDINTEREST:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_AI;
                    break;
                case Cst.IndexSelfCompounding.VALORISATION:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_V;
                    break;
            }
            //
            string sqlFrom = string.Empty;
            if (Cst.IndexSelfCompounding.NONE != WithInfoSelfCompounding)
                sqlFrom = OTCmlHelper.GetSQLJoin(CS, tblSelfCompounding, SQLJoinTypeEnum.Left, this.SQLObject + ".IDRX", "self", DataEnum.All);
            //			
            ConstructFrom(sqlFrom);
        }
        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQL_SettlementMessage : SQL_TableWithIDAndInstrCriteria
    {
        #region constructors
        public SQL_SettlementMessage(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_SettlementMessage(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_SettlementMessage(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_SettlementMessage(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            :
            base(pSource, Cst.OTCml_TBL.STLMESSAGE, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region  property
        public string Payer_Receiver
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PAYER_RECEIVER")); }
        }
        public int IdA_Css
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_CSS")); }
        }
        public int IdA_Sender
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_SENDER")); }
        }
        public int IdA_Receiver
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_RECEIVER")); }
        }
        public bool IsUseEvent
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISUSEEVENT")); }
        }
        public bool IsUseTrade
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISUSETRADE")); }
        }
        public bool IsMultiFlow
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISMULTIFLOW")); }
        }
        public bool IsSameIDC
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSAMEIDC")); }
        }

        public bool IsSameSiPayer
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSAMESIPAYER")); }
        }
        public bool IsSameSiReceiver
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSAMESIRECEIVER")); }
        }
        public int NbMinFlow
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("NBMINFLOW")); }
        }
        public int NbMaxFlow
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("NBMAXFLOW")); }
        }
        public string XsltFile
        {
            get { return Convert.ToString(GetFirstRowColumnValue("XSLTFILE")); }
        }
        #endregion  property
    }


    #region SQL_SSIdb
    public class SQL_SSIdb : SQL_TableWithID
    {
        private int _idAStlOffice_In;
        private const string Internal = ".";

        #region constructor
        public SQL_SSIdb(string pSource, int pIdIssi)
            : base(pSource, Cst.OTCml_TBL.SSIDB, IDType.Id, pIdIssi.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_SSIdb(string pSource, ScanDataDtEnabledEnum pScan, int pIdAStlOffice)
            : base(pSource, Cst.OTCml_TBL.SSIDB, IDType.Id, "0", pScan)
        {
            _idAStlOffice_In = pIdAStlOffice;
        }
        #endregion constructor
        //
        #region public_property_get_set
        public int IdA_In
        {
            get { return _idAStlOffice_In; }
            set
            {
                InitProperty(false);
                _idAStlOffice_In = value;
            }
        }
        #endregion public_property_get_set
        //
        #region public override
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
                _idAStlOffice_In = 0;
            InitProperty(pAll);
        }

        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            //
            if (_idAStlOffice_In != 0)
            {
                string sqlWhereIda = "IDA_STLOFFICE=@IDA_STLOFFICE";
                SetDataParameter(new DataParameter(CS, "IDA_STLOFFICE", DbType.Int32), _idAStlOffice_In);
                ConstructWhere(sqlWhereIda);
            }
        }
        #endregion public override
        //
        #region public Accessor
        protected override bool IsUseOrderBy
        {
            get
            {
                return (base.IsUseOrderBy || (IdA_In != 0));
            }
        }
        public override string Identifier
        {
            get { return Convert.ToString("N/A"); }
        }
        public override string DisplayName
        {
            get { return Convert.ToString("N/A"); }
        }
        public override string Key
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDENTIFIER")); }
        }
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public int PriorityRank
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PRIORITYRANK")); }
        }
        public string Url
        {
            get { return Convert.ToString(GetFirstRowColumnValue("URL")); }
        }
        public bool IsInternal
        {
            get { return (Url == Internal); }
        }
        #endregion accessor
    }
    #endregion SQL_SSIdb

    #region SQL_StUser
    /// <summary>
    ///  Classe chargée de la lecture des Tables statuts user sur EVENT ou TRADE 
    ///  <para>TRADESTMATCH, EVENTSTMATCH</para>
    ///  <para>TRADESTCHECK, EVENTSTCHECK</para>
    /// </summary>
    public abstract class SQL_StUser : SQL_TableWithID
    {
        #region Members
        protected string _columnName;
        /// <summary>
        /// StatusCheck ou StatusMatch
        /// </summary>
        protected StatusEnum _statusEnum;
        /// <summary>
        /// IDT or IDE
        /// </summary>
        protected int _idParent;
        #endregion Member

        #region constructor
        public SQL_StUser(string pSource, int pIdParent, StatusEnum pStatusEnum, Cst.OTCml_TBL pTable)
            : base(pSource, pTable, IDType.Id, "0", ScanDataDtEnabledEnum.No)
        {
            _idParent = pIdParent;
            _statusEnum = pStatusEnum;
            _columnName = StatusTools.GetColumnNameStatusUser(pStatusEnum);
        }
        #endregion constructor

        #region public property Get
        /// <summary>
        ///  Type de statut
        /// </summary>
        public StatusEnum StatusEnum
        {
            get { return _statusEnum; }
        }

        public string IdSt
        {
            get { return Convert.ToString(GetFirstRowColumnValue(_columnName)); }
        }
        /// <summary>
        /// Obtient la date DTINS
        /// </summary>
        public DateTime DtIns
        {
            get
            {
                // FI 20200820 [25468] Dates systemes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTINS")), DateTimeKind.Utc);
            }
        }
        /// <summary>
        /// Obtient l'acteur IDAINS
        /// </summary>
        public int IdAIns
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDAINS")); }
        }
        /// <summary>
        /// Obtient la date d'effet
        /// </summary>
        /// FI 20140728 [20255] add 
        // EG 20171115 [23509] Use GetNullableDateTimeValue
        public Nullable<DateTime> DtEffect
        {
            get
            {
                //Nullable<DateTime> ret = null;
                //if (GetFirstRowColumnValue("DTEFFECT") != Convert.DBNull)
                //    ret = Convert.ToDateTime(GetFirstRowColumnValue("DTEFFECT"));
                //return ret;
                return GetNullableDateTimeValue("DTEFFECT");
            }
        }

        /// <summary>
        /// Obtient la note
        /// </summary>
        /// FI 20140728 [20255] add 
        public String Note
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LONOTE")); }
        }


        /// <summary>
        /// <para>TRADESTMATCH ou TRADESTMATCH: Obtient l'id du trade (IDT)</para>
        /// <para>EVENTSTMATCH ou EVENTSTMATCH: Obtient l'id de l'evt (IDE)</para> 
        /// </summary>
        public int IdParent
        {
            get { return _idParent; }
        }

        #endregion public property Get
    }

    /// <summary>
    /// SQL_TradeStSys (Lecture via table TRADE)
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public class SQL_TradeStSys : SQL_TableWithID
    {
        #region constructor
        public SQL_TradeStSys(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.TRADE, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        #endregion constructor
        #region public property Get
        // RD 20091228 [16809] Confirmation indicators for each party
        public string IdStEnvironment
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTENVIRONMENT")); }
        }
        public DateTime DtStEnvironment
        {
            get
            {
                // FI 20200820 [25468] Dates systemes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTENVIRONMENT")), DateTimeKind.Utc);
            }
        }
        public int IdAStEnvironment
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTENVIRONMENT")); }
        }
        //20100311 PL-StatusBusiness
        public string IdStBusiness
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTBUSINESS")); }
        }
        public DateTime DtStBusiness
        {
            get {
                // FI 20200820 [25468] Dates systemes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTBUSINESS")), DateTimeKind.Utc);
            }
        }
        public int IdAStBusiness
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTBUSINESS")); }
        }
        //
        public string IdStActivation
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTACTIVATION")); }
        }
        public DateTime DtStActivation
        {
            get
            {
                // FI 20200820 [25468] Dates systèmes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTACTIVATION")), DateTimeKind.Utc);
            }
        }
        public int IdAStActivation
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTACTIVATION")); }
        }
        //
        public string IdStUsedBy
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTUSEDBY")); }
        }

        public DateTime DtStUsedBy
        {
            get
            {
                // FI 20200820 [25468] Dates systèmes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTUSEDBY")), DateTimeKind.Utc);
            }
        }

        public string LibStUsedBy
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LIBSTUSEDBY")); }
        }

        public int IdAStUsedBy
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTUSEDBY")); }
        }
        //
        public string IdStPriority
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("IDSTPRIORITY"));
                
            }
        
        }

        public DateTime DtStPriority
        {
            get
            {
                // FI 20200820 [25468] Dates systèmes en UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSTPRIORITY")), DateTimeKind.Utc);
            }
        }

        public int IdAStPriority
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTPRIORITY")); }
        }

        #endregion public property Get

        #region Methods
        // EG 20200914 [XXXXX] Correction Bug Passage CS et pDbTransaction sur l'ExecuteDataset et DataAdapter
        public Cst.ErrLevel UpdateTradeStUsedBy(IDbTransaction pDbTransaction, int pIdA, 
            Cst.StatusUsedBy pStUsedBy, string pLibStUsedBy, DateTime pDtStUsedBy)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string SQLQuery = GetQueryParameters(
                        new string[] { "IDT", "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY" }).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(CS, pDbTransaction, CommandType.Text, SQLQuery);
            DataTable dt = ds.Tables[0];
            DataRow dr = dt.Rows[0];
            dr.BeginEdit();
            dr["IDSTUSEDBY"] = pStUsedBy.ToString();
            dr["LIBSTUSEDBY"] = StrFunc.IsFilled(pLibStUsedBy) ? pLibStUsedBy : Convert.DBNull;
            dr["DTSTUSEDBY"] = pDtStUsedBy;
            dr["IDASTUSEDBY"] = pIdA;
            dr.EndEdit();
            DataHelper.ExecuteDataAdapter(CS, pDbTransaction, SQLQuery, dt);
            return ret;
        }
        #endregion Methods
    }

    /// <summary>
    ///  Lecture de la table TRADESTMATH ou de la table TRADESTCHECK
    /// </summary>
    public class SQL_TradeStUser : SQL_StUser
    {
        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdT"></param>
        /// <param name="pStatusEnum"></param>
        public SQL_TradeStUser(string pSource, int pIdT, StatusEnum pStatusEnum)
            : base(pSource, pIdT, pStatusEnum, StatusTools.GetTableNameStatusUser(pStatusEnum, Cst.OTCml_TBL.TRADE))
        {
        }
        #endregion constructor

        #region public property Get
        public string IdT
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDT")); }
        }
        #endregion public property Get

        #region override SetSQLWhere
        protected override void SetSQLWhere()
        {
            //
            base.SetSQLWhere();
            //
            if (_idParent != 0)
            {
                string sqlWhereIdT = "IDT =@IDT";
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), _idParent);
                ConstructWhere(sqlWhereIdT);
            }
        }
        #endregion override SetSQLWhere

        #region override SetPrimaryKey
        protected override void SetPrimaryKey()
        {
            if (null != Dt)
            {
                DataColumn[] columnPK = new DataColumn[2] { Dt.Columns["IDT"], Dt.Columns[_columnName] };
                Dt.PrimaryKey = columnPK;
            }
        }
        #endregion override SetPrimaryKey

    }
    /// <summary>
    ///  Lecture de la table EVENTSTMATH ou de la table EVENTSTCHECK
    /// </summary>
    public class SQL_EventStUser : SQL_StUser
    {
        #region constructor
        public SQL_EventStUser(string pSource, int pIdE, StatusEnum pStatusEnum)
            : base(pSource, pIdE, pStatusEnum, StatusTools.GetTableNameStatusUser(pStatusEnum, Cst.OTCml_TBL.EVENT))
        {
        }
        #endregion constructor

        #region override SetSQLWhere
        protected override void SetSQLWhere()
        {
            //
            base.SetSQLWhere();
            //
            if (_idParent != 0)
            {
                string sqlWhereIdE = "IDE = @IDE";
                SetDataParameter(new DataParameter(CS, "IDE", DbType.Int32), _idParent);
                ConstructWhere(sqlWhereIdE);
            }
        }
        #endregion override SetSQLWhere

        #region public property Get
        public string IdE
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDE")); }
        }
        #endregion public property Get

        #region override SetPrimaryKey
        protected override void SetPrimaryKey()
        {
            if (null != Dt)
            {
                DataColumn[] columnPK = new DataColumn[2] { Dt.Columns["IDE"], Dt.Columns[_columnName] };
                Dt.PrimaryKey = columnPK;
            }
        }
        #endregion override SetPrimaryKey
    }
    #endregion

    #region SQL_Shell
    public class SQL_Shell : SQL_TableWithID
    {
        public SQL_Shell(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Shell(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum) { }
        public SQL_Shell(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Shell(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource, Cst.OTCml_TBL.IOSHELL, pIdType, pIdentifier, pScanDataDtEnabledEnum)
        { }

        #region public property
        public string SIFileName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SIFILENAME")); }
        }
        public string SIArguments
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SIARGUMENTS")); }
        }
        public string SIWorkingDirectory
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SIWORKINGDIRECTORY")); }
        }
        public string SIStyle
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SISTYLE")); }
        }
        public string SIConnection
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SICONNECTION")); }
        }
        public int IsSynchMode
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("ISSYNCHMODE")); }
        }
        public int TimeOut
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("TIMEOUT")); }
        }
        public int ExitCodeSuccess
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("EXITCODESUCCESS")); }
        }
        public int ExitCodeWarning
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("EXITCODEWARNING")); }
        }
        public int ExitCodeError
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("EXITCODEERROR")); }
        }
        public int ExitCodeTimeOut
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("EXITCODETIMEOUT")); }
        }
        #endregion
    }
    #endregion SQL_IOShell

    public class SQL_SMTPServer : SQL_TableWithID
    {
        #region constructor
        public SQL_SMTPServer(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.SMTPSERVER, pId) { }
        public SQL_SMTPServer(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource, Cst.OTCml_TBL.SMTPSERVER, pIdType, pIdentifier, pScanDataDtEnabledEnum) { }
        #endregion constructor

        #region public_property_get
        public string OriginAdress
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ORIGINADRESS")); }
        }
        public string Host_Primary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("HOST_PRIMARY")); }
        }
        public int Port_Primary
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PORT_PRIMARY")); }
        }
        public string UserID_Primary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("USERID_PRIMARY")); }
        }
        public string PWD_Primary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PWD_PRIMARY")); }
        }
        public bool IsSSL_Primary
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSSL_PRIMARY")); }
        }
        public string Host_Secondary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("HOST_SECONDARY")); }
        }
        public int Port_Secondary
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PORT_SECONDARY")); }
        }
        public string UserID_Secondary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("USERID_SECONDARY")); }
        }
        public string PWD_Secondary
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PWD_SECONDARY")); }
        }
        public bool IsSSL_Secondary
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISSSL_SECONDARY")); }
        }
        #endregion public_property_get
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_Tax : SQL_TableWithID
    {
        #region Accessors
        public override int Id
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDTAX")); }
        }
        #endregion Accessors
        #region Constructors
        public SQL_Tax(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Tax(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_Tax(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Tax(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.TAX, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_TaxDet : SQL_TableWithID
    {
        #region Members
        private int _idTax_In;
        #endregion Members
        #region Accessors
        public override int Id
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDTAXDET")); }
        }
        public string TaxType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TAXTYPE")); }
        }
        public decimal TaxRate
        {
            get { return Convert.ToDecimal(GetFirstRowColumnValue("TAXRATE")); }
        }
        public string EventType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EVENTTYPE")); }
        }
        public int IdTax_In
        {
            get { return _idTax_In; }
            set
            {
                InitProperty(false);
                _idTax_In = value;
            }
        }
        #endregion Accessors
        #region constructors
        public SQL_TaxDet(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_TaxDet(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_TaxDet(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_TaxDet(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.TAXDET, pIdType, pIdentifier, pIsScanDataEnabled) { }

        public SQL_TaxDet(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled, int pIdTax)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled, pIdTax) { }

        public SQL_TaxDet(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled, int pIdTax)
            : base(pSource, Cst.OTCml_TBL.TAXDET, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            _idTax_In = pIdTax;
        }
        #endregion constructors
        #region public override InitProperty
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idTax_In = 0;
            }
            base.InitProperty(pAll);
        }
        #endregion public override InitProperty

    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_TradeTrail : SQL_TableWithID
    {

        public SQL_TradeTrail(string pSource, int pId)
            : base(pSource, Cst.OTCml_TBL.TRADETRAIL, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
    }

    /// <summary>
    /// Retourne un Trade 
    /// <para>L'application des restriction d'accès s'applique uniquement vis à vis de l'instrument</para>
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public class SQL_TradeCommon : SQL_TableWithID
    {
        #region private membres
        private bool _isWithTradeXML = false;
        /// <summary>
        /// Représente un User (utilisé pour les restrictions uniquement)
        /// </summary>
        private User _user;

        /// <summary>
        /// Représente l'identifiant de la session du User
        /// </summary>
        private string _sessionId;

        /// <summary>
        /// Pilote l'application des restrictons 
        /// </summary>
        private SQL_Table.RestrictEnum _restrict;

        /// <summary>
        /// Exprime un périmètre sur produit
        /// </summary>
        private readonly string _restrictProduct;

        /// <summary>
        /// 
        /// </summary>
        private Cst.StatusEnvironment _environment;



        #endregion

        #region constructor
        /// <summary>
        /// Chgt du trade {pId} sans usage des restrictions d'accès sur Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        public SQL_TradeCommon(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(),
                   Cst.StatusEnvironment.UNDEFINED, RestrictEnum.No, null, null, null) { }

        /// <summary>
        /// Chgt du trade {pId} avec usage des restrictions sur Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        /// <param name="pRestrict">Pilote les restrictions sur instruments accessibles</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        public SQL_TradeCommon(string pSource, int pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : this(pSource, IDType.Id, pId.ToString(),
                   Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, null) { }

        /// <summary>
        /// Chgt du trade {pId} avec usage des restrictions sur Instrument et avec périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        /// <param name="pRestrict">Pilote les restrictions sur instruments accessibles</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pRestrictProduct"></param>
        public SQL_TradeCommon(string pSource, int pId, RestrictEnum pRestrict, User pUser, string pSessionId, string pRestrictProduct)
            : this(pSource, IDType.Id, pId.ToString(), Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, pRestrictProduct) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions sur Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeCommon(string pSource, string pIdentifier)
            : this(pSource, IDType.Identifier, pIdentifier, Cst.StatusEnvironment.UNDEFINED, RestrictEnum.No, null, null, null) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} avec usage des restrictions sur Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrict">Pilote les restrictions sur instrument accessibles</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pRestrictProduct"></param>
        public SQL_TradeCommon(string pSource, string pIdentifier,
                                RestrictEnum pRestrict, User pUser, string pSessionId)
            : this(pSource, IDType.Identifier, pIdentifier, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, null) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} avec usage des restrictions d'accès sur Instrument, avec périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrictInstr"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSessionAdmin"></param>
        /// <param name="pIdASessionIdAEntity"></param>
        /// <param name="pRestrictProduct"></param>
        public SQL_TradeCommon(string pSource, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId, string pRestrictProduct)
            : this(pSource, IDType.Identifier, pIdentifier, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, pRestrictProduct) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} avec usage des restrictions sur Instrument, avec périmètre sur les produits possibles et environnement
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pStatusEnvironment"></param>
        /// <param name="pRestrict"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pRestrictProduct"></param>
        public SQL_TradeCommon(string pSource, IDType pIdType, string pIdentifier,
            Cst.StatusEnvironment pStatusEnvironment, RestrictEnum pRestrict, User pUser, string pSessionId, string pRestrictProduct)
            : base(pSource, Cst.OTCml_TBL.TRADE, pIdType, pIdentifier, ScanDataDtEnabledEnum.No)
        {
            //----------------------------------------------
            //PL 20121023 WARNING
            //----------------------------------------------
            if (pIdType == IDType.Id && pIdentifier == "0")
            {
                //Si ID=0 on force une recherche avec -1 afin de ne rien retourner !
                //NB: Si ID=0, la classe de base effectue une recherche "sans critère", ce qui s'avère critique concernant la table TRADE.
                this.Id_In = -1;
            }
            //----------------------------------------------

            _environment = pStatusEnvironment;
            _restrict = pRestrict;
            _restrictProduct = pRestrictProduct;

            _user = pUser;
            _sessionId = pSessionId;
            if (null == _user)
                _user = new User(1, null, RoleActor.SYSADMIN);
        }
        #endregion constructor

        #region Public property Get/set
        public bool IsWithTradeXML
        {
            get { return _isWithTradeXML; }
            set
            {
                if (_isWithTradeXML != value)
                {
                    InitProperty(false);
                    _isWithTradeXML = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RestrictEnum Restrict
        {
            get { return _restrict; }
            set
            {
                if (_restrict != value)
                {
                    InitProperty(false);
                    _restrict = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public User User
        {
            get { return _user; }
            set
            {
                InitProperty(true);
                _user = value;
                if (null == _user)
                    _user = new User(1, null, RoleActor.SYSADMIN);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Cst.StatusEnvironment Environment
        {
            get { return _environment; }
            set
            {
                if (_environment != value)
                {
                    InitProperty(true);
                    _environment = value;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }
        #endregion Public property Get/set


        /// <summary>
        /// 
        /// </summary>
        public string TradeXml
        {
            get
            {
                return Convert.ToString(GetFirstRowColumnValue("TRADEXML"));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdT
        {
            get
            {
                return Id;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdI
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdP
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDP")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdT_Template
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDT_TEMPLATE")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public EfsMLDocumentVersionEnum EfsMLVersion
        {
            get { return StringToEnum.EfsMLVersion(GetFirstRowColumnValue("EFSMLVERSION").ToString()); }
        }
        /// <summary>
        /// Obtient la date présente dans DTTRADE 
        /// </summary>
        public DateTime DtTrade
        {
            get { return (DateTime)GetFirstRowColumnValue("DTTRADE"); }
        }

        /// <summary>
        /// Obtient la date présente dans DTTIMESTAMP
        /// </summary>
        // EG 20171025 [23509] New
        // EG 20171115 [23509] Use GetNullableDateTimeValue
        public Nullable<DateTime> DtTimestamp
        {
            get 
            { 
                //return (Nullable<DateTime>)GetFirstRowColumnValue("DTTIMESTAMP"); 
                return GetNullableDateTimeValue("DTTIMESTAMP");
            }
        }
        /// <summary>
        /// Obtient la date présente dans TZFACILITY
        /// </summary>
        // EG 20171025 [23509] New
        public string TimeZoneFacility
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TZFACILITY")); }
        }
        /// <summary>
        /// Obtient la date présente dans DTORDERENTERED
        /// </summary>
        // EG 20171025 [23509] New
        // EG 20171115 [23509] Use GetNullableDateTimeValue
        public Nullable<DateTime> DtOrderEntered
        {
            get 
            {
                //return (Nullable<DateTime>)GetFirstRowColumnValue("DTORDERENTERED");
                return GetNullableDateTimeValue("DTORDERENTERED");
            }
        }
        /// <summary>
        /// Obtient la date présente dans DTEXECUTION
        /// </summary>
        // EG 20171025 [23509] New
        public Nullable<DateTime> DtExecution
        {
            get 
            { 
                //return (Nullable<DateTime>)GetFirstRowColumnValue("DTEXECUTION");
                return GetNullableDateTimeValue("DTEXECUTION");
            }
        }
        /// <summary>
        /// Obtient la date présente dans DTBUSINESS
        /// </summary>
        // EG 20171025 [23509] New
        // EG 20171115 [23509] Use GetNullableDateTimeValue
        public Nullable<DateTime> DtBusiness
        {
            get 
            { 
                //return (Nullable<DateTime>)GetFirstRowColumnValue("DTBUSINESS");
                return GetNullableDateTimeValue("DTBUSINESS");
            }
        }



        /// <summary>
        /// Obtient la date présente dans DTSYS
        /// </summary>
        public DateTime DtSYS
        {
            get
            {
                // FI 20200820 [25468] DTSYS est une date UTC
                return DateTime.SpecifyKind(Convert.ToDateTime(GetFirstRowColumnValue("DTSYS")), DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Obtient la restriction sur la table PRODUCT
        /// </summary>
        public virtual string RestrictProduct
        {
            get
            {
                return _restrictProduct;
            }
        }

        #region StSys columns
        public string IdStEnvironment
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTENVIRONMENT")); }
        }
        public DateTime DtStEnvironment
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTENVIRONMENT")); }
        }
        public int IdAStEnvironment
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTENVIRONMENT")); }
        }

        public string IdStBusiness
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTBUSINESS")); }
        }

        public Cst.StatusBusiness IdStBusinessEnum
        {
            get { return (Cst.StatusBusiness)Enum.Parse(typeof(Cst.StatusBusiness), IdStBusiness); }
        }

        public DateTime DtStBusiness
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTBUSINESS")); }
        }
        public int IdAStBusiness
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTBUSINESS")); }
        }

        public string IdStActivation
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTACTIVATION")); }
        }
        public DateTime DtStActivation
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTACTIVATION")); }
        }
        public int IdAStActivation
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTACTIVATION")); }
        }

        public string IdStUsedBy
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTUSEDBY")); }
        }
        public DateTime DtStUsedBy
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTUSEDBY")); }
        }
        public string LibStUsedBy
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LIBSTUSEDBY")); }
        }
        public int IdAStUsedBy
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTUSEDBY")); }
        }

        public string IdStPriority
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDSTPRIORITY")); }
        }
        public DateTime DtStPriority
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTSTPRIORITY")); }
        }
        public int IdAStPriority
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASTPRIORITY")); }
        }
        #endregion StSys columns

        #region TradeInstrument columns
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public Nullable<int> IdM
        {
            get {return GetNullableIntValue("IDM");}
        }
        public Nullable<int> IdM_Facility
        {
            get {return GetNullableIntValue("IDM_FACILITY");}
        }
        public Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get 
            {
                Nullable<Cst.UnderlyingAsset> ret = null;
                object colValue = GetFirstRowColumnValue("ASSETCATEGORY");
                if (null != colValue)
                    ret = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), colValue.ToString());
                return ret;
            }
        }
        public Nullable<int> IdAsset
        {
            get { return GetNullableIntValue("IDASSET"); }
        }
        public string Side
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SIDE")); }
        }
        public Nullable<decimal> Qty
        {
            get { return GetNullableDecimalValue("QTY"); }
        }
        public string UnitQty
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITQTY")); }
        }
        public Nullable<decimal> Price
        {
            get { return GetNullableDecimalValue("PRICE"); }
        }
        public string UnitPrice
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITPRICE")); }
        }
        public string TypePrice
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TYPEPRICE")); }
        }
        public Nullable<decimal> StrikePrice
        {
            get { return GetNullableDecimalValue("STRIKEPRICE"); }
        }
        public string UnitStrikePrice
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITSTRIKEPRICE")); }
        }
        public Nullable<decimal> AccrualsInterestRate
        {
            get { return GetNullableDecimalValue("ACCINTRATE"); }
        }
        public string ExecutionId
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXECUTIONID")); }
        }
        public string TrdType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TRDTYPE")); }
        }
        public string TrdSubType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TRDSUBTYPE")); }
        }
        public string SecondaryTrdType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SECONDARYTRDTYPE")); }
        }
        public string OrderId
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ORDERID")); }
        }
        public string OrderType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ORDERTYPE")); }
        }
        public Nullable<int> IdA_Dealer
        {
            get { return GetNullableIntValue("IDA_DEALER"); }
        }
        public Nullable<int> IdB_Dealer
        {
            get { return GetNullableIntValue("IDB_DEALER"); }
        }
        public Nullable<int> IdA_Clearer
        {
            get { return GetNullableIntValue("IDA_CLEARER"); }
        }
        public Nullable<int> IdB_Clearer
        {
            get { return GetNullableIntValue("IDB_CLEARER"); }
        }
        public Nullable<int> IdA_Buyer
        {
            get { return GetNullableIntValue("IDA_BUYER"); }
        }
        public Nullable<int> IdB_Buyer
        {
            get { return GetNullableIntValue("IDB_BUYER"); }
        }
        public Nullable<int> IdA_Seller
        {
            get { return GetNullableIntValue("IDA_SELLER"); }
        }
        public Nullable<int> IdB_Seller
        {
            get { return GetNullableIntValue("IDB_SELLER"); }
        }
        public Nullable<int> IdA_Risk
        {
            get { return GetNullableIntValue("IDA_RISK"); }
        }
        public Nullable<int> IdB_Risk
        {
            get { return GetNullableIntValue("IDB_RISK"); }
        }
        public Nullable<int> IdA_Entity
        {
            get { return GetNullableIntValue("IDA_ENTITY"); }
        }
        public Nullable<int> IdA_CssCustodian
        {
            get { return GetNullableIntValue("IDA_CSSCUSTODIAN"); }
        }
        public Nullable<DateTime> DtInUnadj
        {
            get { return GetNullableDateTimeValue("DTINUNADJ"); }
        }
        public Nullable<DateTime> DtInAdj
        {
            get { return GetNullableDateTimeValue("DTINADJ"); }
        }
        public Nullable<DateTime> DtOutUnadj
        {
            get { return GetNullableDateTimeValue("DTOUTUNADJ"); }
        }
        public Nullable<DateTime> DtOutAdj
        {
            get { return GetNullableDateTimeValue("DTOUTADJ"); }
        }
        public Nullable<DateTime> DtSettlement
        {
            get { return GetNullableDateTimeValue("DTSETTLT"); }
        }
        public Nullable<DateTime> DtDeliveryStart
        {
            get { return GetNullableDateTimeValue("DTDLVYSTART"); }
        }
        public Nullable<DateTime> DtDeliveryEnd
        {
            get { return GetNullableDateTimeValue("DTDLVYEND"); }
        }
        public string TimeZoneDelivery
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TZDLVY")); }
        }
        public string PositionEffect
        {
            get { return Convert.ToString(GetFirstRowColumnValue("POSITIONEFFECT")); }
        }
        public bool SubjectMarginCall
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("SUBJECTMARGINCALL")); }
        }
        public string RelatedPosId
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RELTDPOSID")); }
        }
        public string InputSource
        {
            get { return Convert.ToString(GetFirstRowColumnValue("INPUTSOURCE")); }
        }
        #endregion TradeInstrument columns

        #region Method
        /// <summary>
        /// Ajoute une jointure sur la table TRADEXML, l'alias de la jointure est "trx"
        /// </summary>
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (Left Join)
        protected void AddJoinOnTableTRADEXML()
        {
            string sqlFrom = OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.TRADEXML, false, SQLObject + ".IDT", "trx", false) + Cst.CrLf;
            ConstructFrom(sqlFrom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _sessionId = null;
                _restrict = RestrictEnum.No;
                _environment = Cst.StatusEnvironment.UNDEFINED;
                _isWithTradeXML = false;
            }
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (IsWithTradeXML)
                AddJoinOnTableTRADEXML();

            if (IsApplyRestrict() && IsApplyDefaultInstrumentRestrict())
            {
                ConstructFrom(Cst.SR_INSTR_JOIN + "(" + SQLObject + ".IDI" + ")");
            }
            //
            if (StrFunc.IsFilled(RestrictProduct))
            {
                string sqlFrom = String.Format(@"inner join dbo.INSTRUMENT ns on (ns.IDI = {0}.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and ({1})", SQLObject, RestrictProduct) + Cst.CrLf;
                ConstructFrom(sqlFrom);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            //
            if (Cst.StatusEnvironment.UNDEFINED != _environment)
            {
                string sqlWhere = SQLObject + ".IDSTENVIRONMENT=@IDSTENVIRONMENT";
                SetDataParameter(new DataParameter(CS, "IDSTENVIRONMENT", DbType.String, SQLCst.UT_STATUS_LEN), _environment);
                ConstructWhere(sqlWhere);
            }
        }

        /// <summary>
        /// Retourne true si Spheres® doit retourner uniquement les trades accessibles 
        /// <para></para>
        /// </summary>
        /// <returns></returns>
        protected bool IsApplyRestrict()
        {
            bool isOk = (_restrict == RestrictEnum.Yes);
            //Pas de restriction sur un acteur admin
            isOk &= (!_user.IsSessionSysAdmin);

            //Les templates sont accessibles par tous (TRADEINSTRUMENT n'est pas alimenté sur les templates)
            isOk &= !(Environment == Cst.StatusEnvironment.TEMPLATE);
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(string[] pCol)
        {
            QueryParameters qp = base.GetQueryParameters(pCol);
            if (IsApplyRestrict())
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(User, SessionId, true);
                qp.Query = srh.ReplaceKeyword(qp.Query);
                srh.SetParameter(CS, qp.Query, qp.Parameters);
            }
            return qp;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool IsApplyDefaultInstrumentRestrict()
        {
            return true;
        }

        public Cst.ErrLevel UpdateTradeStUsedBy(IDbTransaction pDbTransaction, int pIdA,
            Cst.StatusUsedBy pStatusUsedBy, Cst.ProcessTypeEnum pProcessType, DateTime pDtStUsedBy)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string SQLQuery = GetQueryParameters(
                        new string[] { "IDT", "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY" }).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(CS, pDbTransaction, CommandType.Text, SQLQuery);
            DataTable dt = ds.Tables[0];
            DataRow dr = dt.Rows[0];
            dr.BeginEdit();
            dr["IDSTUSEDBY"] = pStatusUsedBy.ToString();
            dr["LIBSTUSEDBY"] = pProcessType.ToString();
            dr["DTSTUSEDBY"] = pDtStUsedBy;
            dr["IDASTUSEDBY"] = pIdA;
            dr.EndEdit();
            DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, dt);
            return ret;
        }

        #endregion

    }

    /// <summary>
    /// Représente les trades de marchés dit "OTC" ou "ETD"
    /// <para></para>
    /// </summary>
    public class SQL_TradeTransaction : SQL_TradeCommon
    {
        #region accessor
        /// <summary>
        ///  Obtient restriction sur la table PRODUCT pour ne considérer que les produits différents de ADM et ASSET
        /// </summary>
        public override string RestrictProduct
        {
            get
            {
                string[] col = new string[] { Cst.ProductGProduct_ADM, Cst.ProductGProduct_ASSET, Cst.ProductGProduct_RISK };
                string ret = DataHelper.SQLColumnIn(CS, "pr.GPRODUCT", col, TypeData.TypeDataEnum.@string, true);
                return ret;
            }
        }

        /// FI 20160810 [22086] Add 
        private AddMissingTrade _addMissingTrade = AddMissingTrade.yes;

        /// <summary>
        ///  Prise en considération des trades incomplets lors de l'application de SESSIONRESTRICT
        /// </summary>
        /// FI 20160810 [22086] Add 
        public AddMissingTrade AddMissingTrade
        {
            get { return _addMissingTrade; }
            set { _addMissingTrade = value; }
        }

        #endregion

        #region constructor

        /// <summary>
        /// Chgt du trade {pId} avec usage des restrictions d'accès sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeTransaction(string pCs, int pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pId, pRestrict, pUser, pSessionId) { }


        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeTransaction(string pCs, int pIdT)
            : base(pCs, pIdT) { }


        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeTransaction(string pCs, string pIdentifier)
            : base(pCs, pIdentifier) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} avec usage des restrictions d'accès sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pRestrictInstr">Pilote l'usage des restrictions d'accès sur instrument</param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSessionAdmin"></param>
        public SQL_TradeTransaction(string pCs, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdentifier, pRestrict, pUser, pSessionId) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public SQL_TradeTransaction(string pCs, SQL_TableWithID.IDType pIdType, string pId)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, RestrictEnum.No, null, null, null) { }

        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public SQL_TradeTransaction(string pCs, SQL_TableWithID.IDType pIdType, string pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, null) { }
        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public SQL_TradeTransaction(string pCs, SQL_TableWithID.IDType pIdType, string pId,
            Cst.StatusEnvironment pStatusEnvironment, RestrictEnum pRestrict, User pUser, string pSessionId, string pRestrictProduct)
            : base(pCs, pIdType, pId, pStatusEnvironment, pRestrict, pUser, pSessionId, pRestrictProduct) { }

        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160810 [22086] Modify
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (IsApplyRestrict())
            {
                // FI 20160810 [22086] add addMissingTrade
                string from = StrFunc.AppendFormat("{0}({1}.IDT,{1},{2})", Cst.SR_TRADE_JOIN, SQLObject, AddMissingTrade.ToString());
                ConstructFrom(from);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            if (IsApplyRestrict())
                ConstructWhere(Cst.SR_TRADE_WHERE_PREDICATE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsApplyDefaultInstrumentRestrict()
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    /// Représente un trade de type titre de créance (Exemple Obligation]) 
    /// </summary>
    /// FI 20121116 [18224][18280] Refactoring gestion de _ISINCode_In
    public class SQL_TradeDebtSecurity : SQL_TradeCommon
    {
        #region membres
        /// <summary>
        /// 
        /// </summary>
        private int _idIForAssetEnv_In;
        /// <summary>
        /// Si true utilise la vue VW_TRADEDEBTSEC
        /// </summary>
        private bool _isUseView;
        /// <summary>
        /// 
        /// </summary>
        private string _ISINCode_In;
        #endregion

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public override string RestrictProduct
        {
            get
            {
                string ret = string.Empty;
                if (false == IsUseView)
                {
                    ret = "pr.GPRODUCT=" + DataHelper.SQLString(Cst.ProductGProduct_ASSET);
                    ret += SQLCst.AND + "pr.FAMILY=" + DataHelper.SQLString(Cst.ProductFamily_DSE);
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique l'uasage de la vue VW_TRADEDEBTSEC comme object principal de la requête 
        /// <para>si true: usage de VW_TRADEDEBTSEC</para>
        /// <para>si false: usage de TRADE</para>
        /// </summary>
        public bool IsUseView
        {
            get
            {
                return _isUseView;
            }
            set
            {
                _isUseView = value;
                //
                if (_isUseView)
                    SQLObject = Cst.OTCml_TBL.VW_TRADEDEBTSEC.ToString();
                else
                    SQLObject = Cst.OTCml_TBL.TRADE.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdIForAssetEnv_In
        {
            get { return _idIForAssetEnv_In; }
            set { _idIForAssetEnv_In = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ISINCode_In
        {
            get { return _ISINCode_In; }
            set
            {
                if (_ISINCode_In != value)
                {
                    InitProperty(true);
                    _ISINCode_In = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121116 [18224][18280]
        protected override bool IsUseOrderBy
        {
            get
            {
                return base.IsUseOrderBy || StrFunc.ContainsIn(ISINCode_In, "%");
            }
        }


        #endregion accessor

        #region constructor
        //Use int ident
        public SQL_TradeDebtSecurity(string pCs, int pId)
            : this(pCs, SQL_TableWithID.IDType.Id, pId.ToString() , RestrictEnum.No,null,null ) { }
        public SQL_TradeDebtSecurity(string pCs, int pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : this(pCs, SQL_TableWithID.IDType.Id, pId.ToString()  , pRestrict, pUser, pSessionId) { }
        //
        //Use string identi
        public SQL_TradeDebtSecurity(string pCs, string pIdentifier)
            : this(pCs, SQL_TableWithID.IDType.Identifier, pIdentifier, RestrictEnum.No, null, null) { }
        public SQL_TradeDebtSecurity(string pCs, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId)
            : this(pCs, SQL_TableWithID.IDType.Identifier, pIdentifier, pRestrict, pUser, pSessionId) { }
        public SQL_TradeDebtSecurity(string pCs, SQL_TableWithID.IDType pIdType,  string pId)
            : this(pCs, pIdType, pId, RestrictEnum.No, null, null) { }
        //Use string or Int ident
        public SQL_TradeDebtSecurity(string pCs, SQL_TableWithID.IDType pIdType , string pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : this(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pStatusEnvironment"></param>
        /// <param name="pRestrict"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        public SQL_TradeDebtSecurity(string pCs, IDType pIdType, string pIdentifier, Cst.StatusEnvironment pStatusEnvironment, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pIdentifier, pStatusEnvironment, pRestrict, pUser, pSessionId, string.Empty)
        {
            switch (pIdType)
            {
                case IDType.IsinCode:
                    _ISINCode_In = pIdentifier;
                    break;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            
            if (IsApplyRestrict())
                ConstructFrom(Cst.SR_TRADEDEBTSEC_JOIN + "(" + SQLObject + "." + "IDT" + ")");

            if ((_idIForAssetEnv_In > 0) || StrFunc.IsFilled(_ISINCode_In))
            {
                string sqlFrom = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEASSET.ToString() + " tasset on (tasset.IDT=" + SQLObject + ".IDT" + ")" + Cst.CrLf;
                ConstructFrom(sqlFrom);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 20120618 FI [17904] Spheres® s'appuie sur la table TRADEASSET et TRADEINSTRUMENT Mise en commeantaire du code XML 
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            if (_idIForAssetEnv_In > 0)
            {
                // ATTENTION ********************************************************************************************************************************************
                // Toute Modification de cette restiction sur ASSETENV doit être reportée sur le fichier CCIML/CustomTrade/Objects/shared.xml
                // Ce n'est pas un simple Copier/Coller, il faut:
                // - remplacer le parametre @IDI par le parametre dynamique %%ID_INSTRUMENT%%, correspondant à TRD_IDI_INSTRUMENTMASTER
                // ******************************************************************************************************************************************************

                StrBuilder sqlWhere = new StrBuilder();
                sqlWhere += @"
                            exists (
                            select 1 from 
                            dbo.ASSETENV ae 
                            where(
                                   (ae.IDI = @IDI) and 
                                   (ae.IDC is null or ae.IDC = tasset.IDC_ISSUE) and 
                                   (ae.IDCOUNTRY is null or ae.IDCOUNTRY= tasset.IDCOUNTRY_ISSUE) and
                                   (ae.IDM is null or ae.IDM = tasset.IDM) and
                                   (ae.IDI_ASSET is null or ae.IDI_ASSET=TRADE.IDI)and 
                                   (ae.SECURITYCLASS is null or ae.SECURITYCLASS = tasset.SECURITYCLASS) and 
                                   (ae.CFICODE is null or tasset.CFICODE like ae.CFICODE) and 
                                   (ae.PRODUCTTYPECODE is null or ae.PRODUCTTYPECODE = tasset.PRODUCTTYPECODE) and  
                                   (ae.FIPRODUCTTYPECODE is null or ae.FIPRODUCTTYPECODE = tasset.FIPRODUCTTYPECODE)
                                 )
                            )";


                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDI), _idIForAssetEnv_In);
                //
                string sqlWhere2 = sqlWhere.ToString();
                if (IsUseView)
                {
                    sqlWhere2 = sqlWhere2.Replace("TRADE.", Cst.OTCml_TBL.VW_TRADEDEBTSEC.ToString() + ".");
                }
                //
                ConstructWhere(sqlWhere2);
                //
                if (IsApplyRestrict())
                {
                    ConstructWhere(Cst.SR_TRADEDEBTSEC_WHERE_PREDICATE);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsApplyDefaultInstrumentRestrict()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;
            
            if (ISINCode_In != null)
            {
                data_In = ISINCode_In;
                column_In = DataParameter.ParameterEnum.ISINCODE;
            }
            
            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = "tasset." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }
            return sqlWhere;
        }

        #endregion
    }

    /// <summary>
    /// Représente un trade représentatif d'un risk (MarginRequirement)
    /// </summary>
    public class SQL_TradeRisk : SQL_TradeCommon
    {
        #region membres

        #endregion

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public override string RestrictProduct
        {
            get
            {
                return "pr.GPRODUCT=" + DataHelper.SQLString(Cst.ProductGProduct_RISK);
            }
        }
        #endregion

        #region constructor
        //Use int ident
        public SQL_TradeRisk(string pCs, int pId)
            : base(pCs, pId) { }

        public SQL_TradeRisk(string pCs, int pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pId, pRestrict, pUser, pSessionId) { }
        //
        //Use string ident
        public SQL_TradeRisk(string pCs, string pIdentifier)
            : base(pCs, pIdentifier) { }

        public SQL_TradeRisk(string pCs, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdentifier, pRestrict, pUser, pSessionId) { }
        //
        //Use string or Int ident
        public SQL_TradeRisk(string pCs, string pId, SQL_TableWithID.IDType pIdType)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, RestrictEnum.No, null, null, string.Empty) { }
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public SQL_TradeRisk(string pCs, SQL_TableWithID.IDType pIdType, string pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId,null) { }
        public SQL_TradeRisk(string pCs, IDType pIdType, string pId, Cst.StatusEnvironment pStatusEnvironment, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pId, pStatusEnvironment, pRestrict, pUser, pSessionId, null) { }

        #endregion constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160810 [22086] Modify
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            // FI 20160810 [22086] Utilisation de Cst.SR_TRADERISK_JOIN 
            if (IsApplyRestrict())
                ConstructFrom(Cst.SR_TRADERISK_JOIN + "(" + SQLObject + "." + "IDT" + ")");
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160810 [22086] Modify
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            // FI 20160810 [22086] Utilisation de Cst.SR_TRADERISK_WHERE_PREDICATE 
            if (IsApplyRestrict())
                ConstructWhere(Cst.SR_TRADERISK_WHERE_PREDICATE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsApplyDefaultInstrumentRestrict()
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    /// Représente un trades ETD allocation 
    /// <para>Une jointure est effectuée sur TRADE.IDSTBUSINESS afin de ne considérer que les allocations</para>
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
    public class SQL_TradeAllocation : SQL_TradeTransaction
    {

        #region accessor
        /// <summary>
        ///  Obtient la restriction SQL sur la table PRODUCT: pr.GPRODUCT= 'FUT'
        /// </summary>
        public override string RestrictProduct
        {
            get
            {
                return StrFunc.AppendFormat("(pr.GPRODUCT={0})", DataHelper.SQLString(Cst.ProductGProduct_FUT));
            }
        }
        #endregion

        #region constructor

        /// <summary>
        /// Chgt du trade {pId} avec usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pId"></param>
        /// <param name="pRestrict"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        public SQL_TradeAllocation(string pCs, int pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pId, pRestrict, pUser, pSessionId) { }


        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeAllocation(string pCs, int pId)
            : base(pCs, pId) { }


        /// <summary>
        /// Chgt du trade {pIdentifier} sans usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdentifier"></param>
        public SQL_TradeAllocation(string pCs, string pIdentifier)
            : base(pCs, pIdentifier) { }

        /// <summary>
        /// Chgt du trade {pIdentifier} avec usage des restrictions d'accès sur Acteur, Book et Instrument, sans périmètre sur les produits possibles
        /// </summary>
        public SQL_TradeAllocation(string pCs, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdentifier, pRestrict, pUser, pSessionId) { }

        #endregion constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            //
            string sqlWhere = SQLObject + "IDSTBUSINESS=@IDSTBUSINESS";
            SetDataParameter(new DataParameter(CS, "IDSTBUSINESS", DbType.String), Cst.StatusBusiness.ALLOC);
            ConstructWhere(sqlWhere);
        }
        #endregion
    }

    ///<summary>
    /// Représente un trade administratif (credit,invoice,additionalInvoice, etc..)
    /// </summary>
    public class SQL_TradeAdmin : SQL_TradeCommon
    {
        #region membres
        private bool _isUseView;
        #endregion

        #region properties
        /// <summary>
        /// 
        /// </summary>
        public override string RestrictProduct
        {
            get
            {
                string ret = string.Empty;
                if (false == IsUseView)
                    ret = "pr.GPRODUCT=" + DataHelper.SQLString(Cst.ProductGProduct_ADM);
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique l'uasage de la vue VW_TRADEADMIN comme object principal de la requête 
        /// <para>si true: usage de VW_TRADEADMIN</para>
        /// <para>si false: usage de TRADE</para>
        /// </summary>
        public bool IsUseView
        {
            get
            {
                return _isUseView;
            }
            set
            {
                _isUseView = value;
                //
                if (_isUseView)
                    SQLObject = Cst.OTCml_TBL.VW_TRADEADMIN.ToString();
                else
                    SQLObject = Cst.OTCml_TBL.TRADE.ToString();
            }
        }
        #endregion

        #region constructor
        //Use int ident
        public SQL_TradeAdmin(string pCs, int pId)
            : base(pCs, pId) { }
        public SQL_TradeAdmin(string pCs, int pId, RestrictEnum pRestrict, User pUSer, string pSessionId)
            : base(pCs, pId, pRestrict, pUSer, pSessionId) { }

        //
        //Use string ident
        public SQL_TradeAdmin(string pCs, string pIdentifier)
            : base(pCs, pIdentifier) { }
        public SQL_TradeAdmin(string pCs, string pIdentifier, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdentifier, pRestrict, pUser, pSessionId) { }

        //Use string or Int ident
        public SQL_TradeAdmin(string pCs, SQL_TableWithID.IDType pIdType, string pId)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, RestrictEnum.No, null, null, string.Empty) { }


        //Use string or Int ident
        public SQL_TradeAdmin(string pCs, SQL_TableWithID.IDType pIdType, string pId, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pId, Cst.StatusEnvironment.UNDEFINED, pRestrict, pUser, pSessionId, string.Empty) { }


        //Use string or Int ident
        public SQL_TradeAdmin(string pCs, SQL_TableWithID.IDType pIdType, string pId, Cst.StatusEnvironment pEnv, RestrictEnum pRestrict, User pUser, string pSessionId)
            : base(pCs, pIdType, pId, pEnv, pRestrict, pUser, pSessionId, string.Empty) { }


        #endregion constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (IsApplyRestrict())
                ConstructFrom(Cst.SR_TRADEADMIN_JOIN + "(" + SQLObject + "." + "IDT" + ")");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            if (IsApplyRestrict())
                ConstructWhere(Cst.SR_TRADEADMIN_WHERE_PREDICATE);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_Poscollateral : SQL_TableWithID
    {

        #region constructor
        public SQL_Poscollateral(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString()) { }

        public SQL_Poscollateral(string pSource, IDType pIdType, string pIdentifier)
            : base(pSource, Cst.OTCml_TBL.POSCOLLATERAL, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }

        /// <summary>
        /// 
        /// </summary>
        public override string ExtlLink2
        {
            get
            {
                return null;
            }
        }
        #endregion constructor
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_PosEquSecurity : SQL_TableWithID
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pId"></param>
        public SQL_PosEquSecurity(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString()) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        public SQL_PosEquSecurity(string pSource, IDType pIdType, string pIdentifier)
            : base(pSource, Cst.OTCml_TBL.POSEQUSECURITY, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }

        /// <summary>
        /// 
        /// </summary>
        public override string ExtlLink2
        {
            get
            {
                return null;
            }
        }
        #endregion constructor
    }

    /// <summary>
    /// Groupe de marchés
    /// </summary>
    /// FI 20141230 [20616] Add class
    // EG 20160404 Migration vs2013 Renommer (avant SQL_GMarket2)
    public class SQL_GMarket : SQL_Group
    {
        #region constructors
        public SQL_GMarket(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_GMarket(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_GMarket(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_GMarket(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.GMARKET, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

    }

    /// <summary>
    /// Groupe d'acteurs
    /// </summary>
    /// FI 20141230 [20616] Add class
    // EG 20160404 Migration vs2013
    public class SQL_GActor : SQL_Group
    {
        #region constructors
        public SQL_GActor(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_GActor(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_GActor(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_GActor(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.GACTOR, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }

    /// <summary>
    /// Groupe de books
    /// </summary>
    /// FI 20141230 [20616] Add class
    // EG 20160404 Migration vs2013
    public class SQL_GBook : SQL_Group
    {
        #region constructors
        public SQL_GBook(string pSource, int pId)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_GBook(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_GBook(string pSource, string pIdentifier)
            : this(pSource, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_GBook(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.GBOOK, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }

    /// <summary>
    /// Groupe d'acteurs, Groupe de books, ou groupe de marchés, ou groupe d'instruments, ou groupe de Derivative Contract
    /// </summary>
    /// FI 20141230 [20616] Add class
    // EG 20160404 Migration vs2013
    public class SQL_Group : SQL_GBase
    {
        #region constructors
        public SQL_Group(string pSource, Cst.OTCml_TBL pTableGrp, int pId)
            : this(pSource, pTableGrp, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_Group(string pSource, Cst.OTCml_TBL pTableGrp, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pTableGrp, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_Group(string pSource, Cst.OTCml_TBL pTableGrp, string pIdentifier)
            : this(pSource, pTableGrp, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_Group(string pSource, Cst.OTCml_TBL pTableGrp, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, pTableGrp, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            switch (pTableGrp)
            {
                case Cst.OTCml_TBL.GACTOR:
                case Cst.OTCml_TBL.GBOOK:
                case Cst.OTCml_TBL.GMARKET:
                case Cst.OTCml_TBL.GINSTR:
                case Cst.OTCml_TBL.GCONTRACT:
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Table :{0} is not implemented", pTableGrp));
            }
        }
        #endregion constructors
    }

    /// <summary>
    /// Représente un groupe de base
    /// </summary>
    /// FI 20141230 [20616] Add class
    // EG 20160404 Migration vs2013 
    public abstract class SQL_GBase : SQL_TableWithID
    {
        #region Members
        /// <summary>
        /// Utilisé pour restriction sur un rôle
        /// </summary>
        protected string _role;
        #endregion Members

        #region constructors
        public SQL_GBase(string pSource, Cst.OTCml_TBL pTableGrp, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, pTableGrp, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            switch (pTableGrp)
            {
                case Cst.OTCml_TBL.GACTOR:
                case Cst.OTCml_TBL.GBOOK:
                case Cst.OTCml_TBL.GMARKET:
                case Cst.OTCml_TBL.GINSTR:
                case Cst.OTCml_TBL.GCONTRACT:
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Table :{0} is not implemented", pTableGrp));
            }
        }
        #endregion constructors




        #region public_property_get_set
        /// <summary>
        /// Restriction sur un rôle
        /// </summary>
        // EG 20160404 Migration vs2013
        public virtual string Role
        {
            get { return _role; }
            set
            {
                if (_role != value)
                {
                    InitProperty(true);
                    _role = value;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
                _role = null;
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            if (StrFunc.IsFilled(_role))
                AddJoinOnTableGROLE();
        }
        /// <summary>
        /// Ajoute une jointure sur la table GMARKETROLE, l'alias de la jointure est "gmr"
        /// </summary>
        private void AddJoinOnTableGROLE()
        {
            string tableGRole = StrFunc.AppendFormat("{0}{1}", this.SQLObject, "ROLE");
            string columId = OTCmlHelper.GetColunmID(SQLObject);
            string sqlFrom = StrFunc.AppendFormat("inner join dbo.{0} gr on (gr.{2}={1}.{2})", tableGRole, SQLObject, columId) + Cst.CrLf;
            ConstructFrom(sqlFrom);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            string columId = StrFunc.AppendFormat("IDROLE{0}", this.SQLObject);

            if (StrFunc.IsFilled(_role))
            {
                string sqlWhere = StrFunc.AppendFormat("gr.{0}=@IDROLE", columId);
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDROLE), _role);
                ConstructWhere(sqlWhere);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Add
    public class SQL_Algorithm : SQL_Actor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdType"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pScanDataEnabled"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        public SQL_Algorithm(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, pIdType, pIdentifier, RestrictEnum.No, pScanDataEnabled, null, string.Empty)
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            string predicat = this.SQLObject + ".ALGOTYPE is not null";
            ConstructWhere(predicat);
        }


        /// <summary>
        /// Obtient le type d'algo
        /// <para>Valeurs possibles présente dans l'enum AlgorithmType</para>
        /// </summary>
        public string ALGOTYPE
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ALGOTYPE")); }
        }
    }

    public class SQL_TableTools
    {

        #region Method
        /// <summary>
        /// Retourne les infos ordinairement utilisés pour caractériser une tâche IO
        /// <para>IDIOTASK,IDENTIFIER,DISPLAYNAME,IN_OUT</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTask"></param>
        /// <returns></returns>
        /// FI 20130129 add
        public static IdInfo GetIOMQueueIdInfo(string pCS, int pIdTask)
        {
            // FI 20200916 [XXXXX] Ne jamais faire appel à un SQL_Table si l'Id vaut 0
            if (pIdTask == 0)
                throw new ArgumentException("Task id:0 doesn't exist");

            IdInfo idInfo = null;


            DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(pCS, null, pIdTask);
            if (null != sqlIOTask)
                idInfo = GetIOMQueueIdInfo(sqlIOTask);

            return idInfo;
        }
        /// <summary>
        /// Retourne les infos ordinairement utilisés pour caractériser une tâche IO
        /// <para>Les colonnes IDIOTASK,IDENTIFIER,DISPLAYNAME,IN_OUT doivent être chargées dans {sqlIOTask}</para>
        /// </summary>
        /// <param name="sqlIOTask"></param>
        /// <returns></returns>
        /// FI 20130129 add
        public static IdInfo GetIOMQueueIdInfo(DataIOTASK sqlIOTask)
        {
            if (null == sqlIOTask)
                throw new ArgumentNullException("parameter {sqlIOTask} is null");

            IdInfo idInfo = new IdInfo
            {
                id = sqlIOTask.IdIOTask,
                idInfos = new DictionaryEntry[]{
                                                new DictionaryEntry("ident", "IOTASK"),
                                                new DictionaryEntry("identifier", sqlIOTask.Identifier),
                                                new DictionaryEntry("displayName", sqlIOTask.DisplayName),
                                                new DictionaryEntry("IN_OUT", sqlIOTask.InOut)
                }
            };
            return idInfo;
        }


        /// <summary>
        /// Retourne les infos ordinairement utilisés pour caractériser une entité dans un traitement RISK
        /// <para>CashBalance,Deposit, etc...</para>
        /// <para>IDA,IDENTIFIER,DISPLAYNAME</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        /// FI 20130205 add
        public static IdInfo GetRiskMQueueIdInfo(string pCS, int pIdA)
        {
            IdInfo idInfo = null;
            SQL_Actor sqlActor = new SQL_Actor(pCS, pIdA);
            if (sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                idInfo = GetRiskMQueueIdInfo(sqlActor);
            return idInfo;
        }
        /// <summary>
        /// Retourne les infos ordinairement utilisés pour caractériser une entité dans un traitement RISK
        /// <para>CashBalance,Deposit, etc...</para>
        /// <para>IDA,IDENTIFIER,DISPLAYNAME</para>
        /// </summary>
        /// <param name="sqlIOTask"></param>
        /// <returns></returns>
        /// FI 20130205 add
        public static IdInfo GetRiskMQueueIdInfo(SQL_Actor sqlActor)
        {
            if (null == sqlActor)
                throw new ArgumentNullException("parameter {sqlActor} is null");

            IdInfo idInfo = new IdInfo
            {
                id = sqlActor.Id,
                idInfos = new DictionaryEntry[]{
                                                new DictionaryEntry("ident", "ACTOR"),
                                                new DictionaryEntry("identifier", sqlActor.Identifier),
                                                new DictionaryEntry("displayName", sqlActor.DisplayName)
            }
            };
            return idInfo;
        }


        #endregion Method

    }

}
