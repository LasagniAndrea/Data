using System;
using System.Collections;
using System.Data;
using System.Globalization;
//
using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    #region public class DataParameter
    /// <summary>
    /// Classe de Gestion d'un DbDataParameter
    /// </summary>
    /// EG [XXXXX][WI417] DataParameter des colonnes d'audit datetime2/timestamp(7)
    [System.Xml.Serialization.XmlRoot(ElementName = "DataParameter", IsNullable = true)]
    public class DataParameter : IDbDataParameter, IDataParameter, ICloneable
    {
        #region Public ParameterEnum
        //PL 20220720 Add DTEXECUTION et DTTRADE
        public enum ParameterEnum
        {
            AMOUNTTYPE,
            APPBROWSER,
            APPNAME,
            APPVERSION,
            ASSETCATEGORY,
            ASSETSYMBOL,
            //
            BBGCODE,
            BIC,
            BUYER_SELLER,
            //
            CATEGORY,
            // PM 20171212 [23646] Add CFICODE
            CFICODE,
            CONTRACTATTRIBUTE,
            CONTRACTSYMBOL,
            //
            CONTRACTMULTIPLIER,
            //
            DISPLAYNAME,
            DESCRIPTION,
            DERIVATIVECONTRACT,
            //
            EXCHANGEACRONYM,
            EXCHANGESYMBOL,
            EXERCISESTYLE,
            EXTLLINK,
            //
            FIXML_SecurityExchange,
            //
            GPRODUCT,
            //
            HOSTNAME,
            HOSTNAMEINS,
            HOSTNAMEUPD,
            //
            [DataParameterAttribute(DbType.Int32)]
            ID,
            [DataParameterAttribute(DbType.Int32)]
            IDA,
            [DataParameterAttribute(DbType.Int32)]
            IDASSET,
            [DataParameterAttribute(DbType.Int32)]
            IDA_ENTITY,
            [DataParameterAttribute(DbType.Int32)]
            IDA_CSS,
            [DataParameterAttribute(DbType.Int32)]
            IDA_CUSTODIAN,
            [DataParameterAttribute(DbType.Int32)]
            IDA_CSSCUSTODIAN,
            [DataParameterAttribute(DbType.Int32)]
            IDA_PAYER,
            [DataParameterAttribute(DbType.Int32)]
            IDA_RECEIVER,
            [DataParameterAttribute(DbType.Int32)]
            IDAINS,
            [DataParameterAttribute(DbType.Int32)]
            IDAUPD,
            [DataParameterAttribute(DbType.Int32)]
            IDB,
            IDBC,
            [DataParameterAttribute(DbType.Int32)]
            IDB_ENTITY,
            IDC,
            IDCOUNTRY,
            [DataParameterAttribute(DbType.Int32)]
            IDDC,
            [DataParameterAttribute(DbType.Int32)]
            IDDERIVATIVEATTRIB,
            [DataParameterAttribute(DbType.Int32)]
            IDE,
            IDENTIFIER,
            [DataParameterAttribute(DbType.Int32)]
            IDI,
            IDLSTCONSULT,
            IDLSTTEMPLATE,
            [DataParameterAttribute(DbType.Int32)]
            IDM,
            [DataParameterAttribute(DbType.Int32)]
            IDMATURITY,
            [DataParameterAttribute(DbType.Int32)]
            IDMATURITYRULE,
            [DataParameterAttribute(DbType.Int32)]
            IDMATURITYRULEOLD,
            [DataParameterAttribute(DbType.Int32)]
            IDP,
            [DataParameterAttribute(DbType.Int32)]
            IDPR,
            [DataParameterAttribute(DbType.Int32)]
            IDPROCESS_L,
            IDROLE,
            IDROLEACTOR,
            IDROLEGACTOR,
            IDROLEGBOOK,
            IDROLEGCONTRACT,
            IDROLEGINSTR,
            IDROLEGMARKET,
            IDSESSIONINS,
            IDSESSIONUPD,

            [DataParameterAttribute(DbType.Int32)]
            IDT,
            ISINCODE,
            ISO10383_ALPHA4,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTBUSINESS,
            /// <summary>
            /// 
            /// </summary>
            /// Datetime2 devient OracleDbType.TimeStamp sous Oracle®
            [DataParameterAttribute(DbType.DateTime2)]
            DTEXECUTION,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTPOS,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DT,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou  where) si la colonne DTFILE est de type date   
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTFILE,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime (SQL Server) et oracleDbtype.timestamp (Oracle)
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou  where) si la colonne DTINS est de type date   
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_INS2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTINS,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime (SQL Server) et oracleDbtype.timestamp (Oracle)
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou  where) si la colonne DTUPD est de type date   
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DTUPD2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTUPD,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime (SQL Server) et oracleDbtype.date (Oracle)
            ///  <para>Doit être utilisé tant que les colonnes DTINS et DTUPD restent de type DATE (Devra être supprimé après)</para>
            /// </summary>
            /// FI 20201019 [XXXXX] Add
            //[DataParameterAttribute(DbType.Date, "DTINS")]
            //DTINSDATE,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime (SQL Server) et oracleDbtype.date (Oracle)
            ///  <para>Doit être utilisé tant que les colonnes DTINS et DTUPD restent de type DATE (Devra être supprimé après)</para>
            /// </summary>
            //[DataParameterAttribute(DbType.Date, "DTUPD")]
            //DTUPDDATE,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime2 (SQL Server) et oracleDbtype.timestamp (Oracle)
            ///  <para>Doit être utilisé sur les colonnes DTINS et DTUPD  de type DATETIME2 (Exemple TRACKER_L)</para>
            /// </summary>
            /// FI 20201028 [XXXXX] Add
            [DataParameterAttribute(DbType.DateTime2, "DTINS")]
            DTINSDATETIME2,
            /// <summary>
            ///  Permet d'obtenir un paramètre de type sqlDbType.datetime (SQL Server) et oracleDbtype.date (Oracle)
            ///  <para>Doit être utilisé sur les colonnes DTINS et DTUPD  de type DATETIME2 (Exemple TRACKER_L)</para>
            /// </summary>
            /// FI 20201028 [XXXXX] Add
            [DataParameterAttribute(DbType.DateTime2, "DTUPD")]
            DTUPDDATETIME2,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date 
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DTSYS2 sous SQLServer® 
            /// FI 20200820 [25468] Add DTSYS
            [DataParameterAttribute(DbType.DateTime2)]
            DTSYS,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTENABLED,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTDISABLED,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTEVENT ,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTEVENTFORCED,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            DTTRADE,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTART,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTEND,
            /// <summary>
            /// 
            /// </summary>
            /// Datetime2 devient OracleDbType.TimeStamp sous Oracle®
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTPROCESS,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTACTIVATION,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTBUSINESS,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date   
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTENVIRONMENT,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTPRIORITY,
            /// <summary>
            ///  Attention sous Oracle. Ce paramètre de type dbType.datetime ne doit pas être utilisé dans une restriction (inner join ou where) si la colonne est de type date   
            /// </summary>
            /// Il faudrait passer ce paramètre en datetime2 Cela permettrait d'avoir une meilleure précision sous SQLserver® (Equivalent au TIMESTAMP sous Oracle®) 
            /// A faire qd les colonnes associées seront converties en TIMESTAMP sous Oracle® et UT_DATETIME2 sous SQLServer® 
            [DataParameterAttribute(DbType.DateTime2)]
            DTSTUSEDBY,
            //
            EVENTCODE,
            EARCODE,
            EVENTTYPE,
            EVENTCLASS,
            //
            // RD 20121003 / FI 20121003 [18166] Bug import des trades HPC
            //MARKET,
            // FI 20220201 [25699] Add
            [DataParameterAttribute(DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN)]
            MARKETASSIGNEDID,

            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            MATURITYDATE,
            /// <summary>
            /// 
            /// </summary>
            /// Date devient SqlDbType.Datetime sous SqlServer®
            [DataParameterAttribute(DbType.Date)]
            MATURITYDATESYS, 
            /// <summary>
            /// 
            /// </summary>
            MATURITYMONTHYEAR,
            [DataParameterAttribute(DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN)]
            MMYFMT,
            //
            NSINCODE,
            NOTE,
            PAYMENTTYPE,
            PROCESS,
            PERIOD,
            [DataParameterAttribute(DbType.Int32)]
            PERIODMLTP,
            //
            PRODUCT,
            PUTCALL,
            //
            RICCODE,
            ROWATTRIBUT,
            /// <summary>
            /// 
            /// </summary>
            /// FI 20200114 [XXXXX] Add
            SHORT_ACRONYM,
            SESSIONID,
            SETTLTMETHOD,
            SR_SESSIONID,
            [DataParameterAttribute(DbType.Int32)]
            SR_IDA_ENTITY,
            STATUS,
            STRIKEPRICE,
            [DataParameterAttribute(DbType.Int32)]
            STREAMNO,
            SYMBOL,
            //
            TIMING,
            LINKCODE,
            ISXXX,
            //
            NA
        }
        #endregion
        
        #region Members
        /// <summary>
        /// Représente DbDataParameter géré
        /// </summary>
        private IDbDataParameter _dbDataParameter;
        /// <summary>
        /// Représente le nom proposé pour le paramètre
        /// </summary>
        private string _namePrompt;
        /// <summary>
        /// Représente la taille proposé pour le paramètre
        /// </summary>
        private int _sizePrompt;
        /// <summary>
        /// Représente le DataType proposé pour le paramètre
        /// </summary>
        private DbType _dbTypePrompt;
        /// <summary>
        /// Représente la valeur proposé pour le paramètre
        /// </summary>
        private string _valuePrompt;
        /// <summary>
        /// Représente le type de server associé au paramètre 
        /// </summary>
        private DbSvrType _serverType;
        /// <summary>
        ///  
        /// </summary>
        private readonly Boolean isMPDSpheres = true;    
        #endregion Members

        #region Accesseur(s)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// <summary>
        /// Obtient ou définit le DbDataParameter
        /// </summary>
        public IDbDataParameter DbDataParameter
        {
            set { _dbDataParameter = value; }
            get { return _dbDataParameter; }
        }
        /// <summary>
        /// Obtient ou définit le type de server
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("ServerType")]
        public DbSvrType ServerType
        {
            set { _serverType = value; }
            get { return _serverType; }
        }
        /// <summary>
        /// Obtient ou définit le nom proposé pour le paramètre, le nom du paramètre aura en plus le prefix spécifique au moteur (Exemple ":" pour Oracle, "@" pour SqlServer)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("Name")]
        public string NamePrompt
        {
            set { _namePrompt = value; }
            get { return _namePrompt; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SizePromptSpecified;
        /// <summary>
        /// Obtient ou définit la taille proposé pour le paramètre
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("Size")]
        public int SizePrompt
        {
            set { _sizePrompt = value; }
            get { return _sizePrompt; }
        }
        /// <summary>
        /// Obtient ou définit le datatype proposé pour le paramètre
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("DbType")]
        public DbType DbTypePrompt
        {
            set { _dbTypePrompt = value; }
            get { return _dbTypePrompt; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValuePromptSpecified;
        /// <summary>
        /// Obtient ou définit la valeur proposé pour le paramètre
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public string ValuePrompt
        {
            set { _valuePrompt = value; }
            get { return _valuePrompt; }
        }
        //
        /// <summary>
        /// Obtient l'identifiant du paramètre (= Nom sans préfix) 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ParameterKey
        {
            get
            {
                return ParameterName.Replace(DataHelper.GetVarPrefix(_serverType), string.Empty); ;
            }
        }
        //    
        #region Membres de IDataParameter
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DbType DbType
        {
            set { _dbDataParameter.DbType = value; }
            get { return _dbDataParameter.DbType; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ParameterDirection Direction
        {
            set { _dbDataParameter.Direction = value; }
            get { return _dbDataParameter.Direction; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNullable
        {
            get { return _dbDataParameter.IsNullable; }
        }
        /// <summary>
        /// Attention: 
        /// <para>Le nom du Parameter contient un préfixe</para>
        /// <para>- ':' pour Oracle</para>
        /// <para>- '@' pour SqlServer</para>
        /// <para>Faire attention à la substitution du nom du Parameter</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ParameterName
        {
            set { _dbDataParameter.ParameterName = value; }
            get { return _dbDataParameter.ParameterName; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SourceColumn
        {
            set { _dbDataParameter.SourceColumn = value; }
            get { return _dbDataParameter.SourceColumn; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataRowVersion SourceVersion
        {
            set { _dbDataParameter.SourceVersion = value; }
            get { return _dbDataParameter.SourceVersion; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object Value
        {
            set { _dbDataParameter.Value = DataHelper.GetDBData(value); }
            get { return _dbDataParameter.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNullValue
        {
            get
            {
                return Convert.IsDBNull(Value);
            }
        }
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public byte Precision
        {
            set { _dbDataParameter.Precision = value; }
            get { return _dbDataParameter.Precision; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public byte Scale
        {
            set { _dbDataParameter.Scale = value; }
            get { return _dbDataParameter.Scale; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int Size
        {
            set { _dbDataParameter.Size = value; }
            get { return _dbDataParameter.Size; }
        }
        #endregion Membres de IDataParameter
        //
        #endregion Accesseur(s)
        
        #region Constructeur(s)
        #region connectionString
        public DataParameter(string connectionString, string parameterName, DbType dbType)
            : this(connectionString, CommandType.Text, parameterName, dbType, 0) { }
        public DataParameter(string connectionString, string parameterName, DbType dbType, int size)
            : this(connectionString, CommandType.Text, parameterName, dbType, size) { }
        public DataParameter(string connectionString, CommandType pCommandType, string parameterName, DbType dbType)
            : this(connectionString, pCommandType, parameterName, dbType, 0) { }
        public DataParameter(string connectionString, CommandType pCommandType, string parameterName, DbType dbType, int size)
            :
            this(DataHelper.GetDbSvrType(connectionString), pCommandType, parameterName, dbType, size) { }
        #endregion
        

        public DataParameter() { }
        public DataParameter(DbSvrType pServerType, CommandType pCommandType, string pParameterName, DbType pDbType, int pSize)
        {
            _serverType = pServerType;
            //
            NamePrompt = pParameterName;
            DbTypePrompt = pDbType;
            SizePrompt = pSize;
            //
            InitializeFromPrompt(pCommandType);
        }
        #endregion Constructeur(s)
        //
        #region Membres de ICloneable
        //PL 20131029 Bug on Oracle
        public object Clone()
        {
            bool isSpecialDbType = false;
            DataParameter clone = new DataParameter
            {
                //Warning: Dans le cas d'une donnée XML, en Oracle, seul DbTypePrompt contient ce type (PL Pourquoi, je ne sais pas ?)
                DbDataParameter = DataHelper.GetDAL(_serverType).NewParameter(this.DbTypePrompt, ref isSpecialDbType),
                ServerType = this.ServerType,
                Direction = this.Direction,
                ParameterName = this.ParameterName,
                SourceColumn = this.SourceColumn,
                SourceVersion = this.SourceVersion,
                Value = this.Value,
                Precision = this.Precision,
                Scale = this.Scale,
                Size = this.Size,
            };
            if (!isSpecialDbType)
                clone.DbType = this.DbType;
            return clone;
        }
        #endregion Membres de ICloneable
        //
        #region public InitializeFromPrompt
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommandType"></param>
        /// FI 20170116 [21916] Modify
        /// FI 20201006 [XXXXX] Private method
        private void InitializeFromPrompt(CommandType pCommandType)
        {
            bool isSpecialDbType = false;

            // FI 20201006 [XXXXX] Dans le MPD SQLSERVER de Spheres il n'y a pas de colonne DATE (UT_DATETIME et UTDATETIME2)
            if ((_serverType == DbSvrType.dbSQL && isMPDSpheres && _dbTypePrompt == DbType.Date))
                _dbTypePrompt = DbType.DateTime;

            _dbDataParameter = DataHelper.GetDAL(ServerType).NewParameter(pCommandType, ref _namePrompt, ref _dbTypePrompt, ref isSpecialDbType);

            ParameterName = NamePrompt;

            if (!isSpecialDbType)
            {
                DbType = DbTypePrompt;
            }

            if (SizePrompt != 0)
            {
                Size = SizePrompt;
            }


            if (ValuePrompt != null)
            {
                switch (DbType)
                {
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                        Value = ValuePrompt;
                        break;
                    case DbType.Int32:
                    case DbType.Int16:
                        Value = IntFunc.IntValue(ValuePrompt);
                        break;
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                        Value = new DtFunc().StringDateISOToDateTime(ValuePrompt);
                        break;
                    case DbType.Boolean:
                        Value = BoolFunc.IsTrue(ValuePrompt);
                        break;
                    case DbType.DateTimeOffset:
                        // FI 20170116 [21916] Ajout d'une exception
                        // Il faudra ajouter ici la conversion d'une string vers un DateTimeOffset qd cela sera nécessaire
                        throw new NotImplementedException(DbType.ToString() + " is not implemented");
                    default:
                        throw new NotImplementedException(DbType.ToString() + " is not implemented");
                }
            }
        }
        #endregion
        #region public InitializeOracleCursor
        public void InitializeOracleCursor(string pName, ParameterDirection pDirection)
        {
            //Warning: N'existe que pour Oracle
            // 20090210 RD 16480 (DAL) 
            _dbDataParameter = DataHelper.GetDAL(DbSvrType.dbORA).NewCursorParameter(pName);
            //----------------    
            //// 20060626 RD ODP.NET
            ////_dbDataParameter = new OracleParameter(pName, OracleType.Cursor);//OracleClient_FRAMEWORK
            //_dbDataParameter = new OracleParameter(pName, OracleDbType.RefCursor);//OracleClient_ORACLE
            //----------------    
            _dbDataParameter.Direction = pDirection;
        }
        #endregion public InitializeOracleCursor
        #region public GetSqlServerDeclaration
        public string GetSqlServerDeclaration()
        {
            
                string ret = ParameterKey;
                ret = DataHelper.GetVarPrefix(DbSvrType.dbSQL) + ret + Cst.Space + DataHelper.DbTypeToSqlDbType(DbType).ToString();
                if (Size > 0)
                    ret += "(" + Size.ToString() + ")";
                return ret;
            
        }
        #endregion
        //
        #region public static GetParameter
        /// <summary>
        /// Retourne un DataParameter à partir d'une ParameterEnum
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pParameterEnum"></param>
        /// <returns></returns>
        public static DataParameter GetParameter(string pCs, ParameterEnum pParameterEnum)
        {

            DataParameter ret;
            string parameterName = pParameterEnum.ToString();

            /******************************************************************************
             * Ne plus ajouter de nouvelles entrées dans le swith case 
             * => Il est préférable de déclaré un DataParameterAttribute
            ********************************************************************************/

            switch (pParameterEnum)
            {
                case ParameterEnum.CONTRACTMULTIPLIER:
                case ParameterEnum.STRIKEPRICE:
                    ret = new DataParameter(pCs, parameterName, DbType.Decimal);
                    break;
                case ParameterEnum.BIC:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, 11);
                    break;
                case ParameterEnum.IDC:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_CURR_LEN);
                    break;
                case ParameterEnum.IDENTIFIER:
                case ParameterEnum.DERIVATIVECONTRACT:
                case ParameterEnum.PAYMENTTYPE:
                case ParameterEnum.PRODUCT:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
                    break;
                case ParameterEnum.ISINCODE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ISINCODE_LEN);
                    break;
                // PM 20171212 [23646] Add CFICODE
                case ParameterEnum.CFICODE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_CFICODE_LEN);
                    break;
                case ParameterEnum.DISPLAYNAME:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN);
                    break;
                case ParameterEnum.DESCRIPTION:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN);
                    break;
                case ParameterEnum.EARCODE:
                case ParameterEnum.EVENTCODE:
                case ParameterEnum.EVENTTYPE:
                case ParameterEnum.EVENTCLASS:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_EVENT_LEN);
                    break;
                case ParameterEnum.EXTLLINK:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
                    break;
                case ParameterEnum.APPVERSION:
                case ParameterEnum.APPBROWSER:
                case ParameterEnum.APPNAME:
                case ParameterEnum.IDSESSIONINS:
                case ParameterEnum.IDSESSIONUPD:
                case ParameterEnum.PROCESS:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
                    break;
                case ParameterEnum.ASSETSYMBOL:
                case ParameterEnum.BBGCODE:
                case ParameterEnum.BUYER_SELLER:
                case ParameterEnum.GPRODUCT:
                case ParameterEnum.NSINCODE:
                case ParameterEnum.RICCODE:
                case ParameterEnum.SYMBOL:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
                    break;
                case ParameterEnum.IDBC:
                case ParameterEnum.AMOUNTTYPE:
                case ParameterEnum.ASSETCATEGORY:
                case ParameterEnum.CATEGORY:
                case ParameterEnum.EXCHANGESYMBOL:
                case ParameterEnum.PERIOD:
                case ParameterEnum.TIMING:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                    break;
                case ParameterEnum.CONTRACTATTRIBUTE:
                case ParameterEnum.CONTRACTSYMBOL:
                case ParameterEnum.LINKCODE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_LINKCODE_LEN);
                    break;
                case ParameterEnum.ISXXX:
                    ret = new DataParameter(pCs, parameterName, DbType.Boolean);
                    break;
                case ParameterEnum.HOSTNAME:
                case ParameterEnum.HOSTNAMEINS:
                case ParameterEnum.HOSTNAMEUPD:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_HOST_LEN);
                    break;
                case ParameterEnum.NOTE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_NOTE_LEN);
                    break;
                case ParameterEnum.ROWATTRIBUT:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
                    break;
                case ParameterEnum.SESSIONID:
                case ParameterEnum.SR_SESSIONID:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_SESSIONID_LEN);
                    break;
                case ParameterEnum.STATUS:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_STATUS_LEN);
                    break;
                case ParameterEnum.EXERCISESTYLE:
                case ParameterEnum.SETTLTMETHOD:
                case ParameterEnum.PUTCALL:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN);
                    break;
                case ParameterEnum.EXCHANGEACRONYM:
                case ParameterEnum.ISO10383_ALPHA4:
                    // RD 20121003 / FI 20121003 [18166] Bug import des trades HPC
                    // Une colonne TMP_VTGENEVE_TRADEIMPORT.MARKET, avec une taille différente de SQLCst.UT_MARKET_LEN, est utilisée
                    //case ParameterEnum.MARKET:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_MARKET_LEN);
                    break;
                case ParameterEnum.FIXML_SecurityExchange:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_MARKET_LEN + 1 + SQLCst.UT_ENUM_MANDATORY_LEN);
                    break;
                case ParameterEnum.SHORT_ACRONYM:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                    break;
                case ParameterEnum.MATURITYMONTHYEAR:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_MATURITY_LEN);
                    break;
                case ParameterEnum.IDLSTCONSULT:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN);
                    break;
                case ParameterEnum.IDLSTTEMPLATE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
                    break;
                case ParameterEnum.IDROLE:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEACTOR_LEN);
                    break;
                case ParameterEnum.IDROLEACTOR:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEACTOR_LEN);
                    break;
                case ParameterEnum.IDROLEGCONTRACT:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGCONTRACT_LEN);
                    break;
                case ParameterEnum.IDROLEGINSTR:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGINSTR_LEN);
                    break;
                case ParameterEnum.IDROLEGMARKET:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGMARKET_LEN);
                    break;
                case ParameterEnum.IDROLEGACTOR:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGACTOR_LEN);
                    break;
                case ParameterEnum.IDROLEGBOOK:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGBOOK_LEN);
                    break;
                case ParameterEnum.IDCOUNTRY:
                    ret = new DataParameter(pCs, parameterName, DbType.AnsiString, SQLCst.UT_COUNTRY_LEN);
                    break;

                default:
                    // FI 20220128 [XXXXX] Call GetParameterFromAttrib
                    ret = GetParameterFromAttrib(pCs, pParameterEnum);
                    if (ret == default(DataParameter))
                        throw (new ArgumentException("No match parameterName", parameterName));
                    break;
            }
            return ret;

        }
        #endregion public static GetParameter

        /// <summary>
        /// Retourne un DataParameter à partir des éventuels attributs spécifiés sur l'enum
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pParameterEnum"></param>
        /// <returns></returns>
        /// 20220128 [XXXXX] Add
        private static DataParameter GetParameterFromAttrib(string pCs, ParameterEnum pParameterEnum)
        {
            DataParameter ret = null;

            DataParameterAttribute attribut = ReflectionTools.GetAttribute<DataParameterAttribute>(pParameterEnum);
            if (null != attribut)
            {
                string name = StrFunc.IsFilled(attribut.Name) ? attribut.Name : pParameterEnum.ToString();
                ret = new DataParameter(pCs, name, attribut.DbType);
                if (attribut.Size.HasValue)
                    ret.Size = attribut.Size.Value;
            }
            return ret;
        }
    }
    #endregion class DataParameter

    #region public class DataParameters
    /// <summary>
    /// Représente une collection de DataParameter
    /// </summary>
    [System.Xml.Serialization.XmlRoot(ElementName = "DataParameters", IsNullable = true)]
    public class DataParameters : ICloneable
    {
        #region Members
        private readonly Hashtable htDataParameter;
        #endregion
        //
        #region accessor
        [System.Xml.Serialization.XmlElementAttribute("DataParameter")]
        public DataParameter[] Parameter
        {
            get
            {
                return GetArrayParameter();
            }
            set
            {
                this.Clear();
                if (ArrFunc.IsFilled(value))
                {
                    for (int i = 0; i < value.Length; i++)
                        Add(value[i]);
                }
            }
        }
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int Count
        {
            get
            {
                int ret = 0;
                if (null != htDataParameter)
                    ret = htDataParameter.Count;
                return ret;
            }
        }
        #endregion accesor
        //
        #region constructor
        public DataParameters()
        {
            htDataParameter = new Hashtable();
        }
        public DataParameters(DataParameter[] parameters)
        {
            htDataParameter = new Hashtable();
            for (int i = 0; i < ArrFunc.Count(parameters); i++)
                Add(parameters[i]);
        }
        #endregion constructor
        //
        #region public Indexeurs
        public DataParameter this[string pParameterKey]
        {
            get
            {
                return (DataParameter)htDataParameter[pParameterKey];
            }
        }
        #endregion Indexeurs
        //
        #region public Add
        public void Add(DataParameter pDataParameter)
        {
            string parameterKey = pDataParameter.ParameterKey;
            htDataParameter.Add(parameterKey, pDataParameter);
        }
        public void Add(DataParameter pDataParameter, object pvalue)
        {
            string parameterKey = pDataParameter.ParameterKey;
            Add(pDataParameter);
            this[parameterKey].Value = pvalue;
        }
        #endregion public Add
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (null != htDataParameter)
                htDataParameter.Clear();
        }
        public void SetAllDBNull()
        {
            if (null != htDataParameter)
            {
                IEnumerator listEnum = htDataParameter.Values.GetEnumerator();
                while (listEnum.MoveNext())
                    ((DataParameter)listEnum.Current).DbDataParameter.Value = Convert.DBNull;
            }
        }
        
        
        /// <summary>
        /// Retourne true s'il existe déjà le paramètre nommé {pParameterKey}
        /// </summary>
        /// <param name="pParameterKey"></param>
        /// <returns></returns>
        public bool Contains(string pParameterKey)
        {
            return htDataParameter.ContainsKey(pParameterKey);
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter[] GetArrayDbParameter()
        {
            IDbDataParameter[] ret = null;
            //
            if (ArrFunc.IsFilled(htDataParameter))
            {
                //
                ArrayList aParameters = new ArrayList();
                IEnumerator listEnum = htDataParameter.Values.GetEnumerator();
                while (listEnum.MoveNext())
                    aParameters.Add(((DataParameter)listEnum.Current).DbDataParameter);
                //
                if (ArrFunc.IsFilled(aParameters))
                    ret = (IDbDataParameter[])aParameters.ToArray(aParameters[0].GetType());
            }
            return ret;
        }
        
        
        /// <summary>
        /// Retourne les valeurs des paramètres séparés par des ;
        /// </summary>
        /// <returns></returns>
        public string GetDataParameter()
        {
            string ret = string.Empty;
            //
            if (ArrFunc.IsFilled(htDataParameter))
            {
                //
                ArrayList aParameters = new ArrayList();
                IEnumerator listEnum = htDataParameter.Values.GetEnumerator();
                while (listEnum.MoveNext())
                    aParameters.Add(((DataParameter)listEnum.Current).DbDataParameter.Value);
                //
                if (ArrFunc.IsFilled(aParameters))
                    ret = ArrFunc.GetStringList(aParameters, ";");
            }
            //
            return ret;
        }
        
        
        /// <summary>
        /// Retourne tous les paramètres vers un nouveau array de type DataParameter
        /// <para>Retourne null s'il n'existe aucun paramètre ds la collection</para>
        /// </summary>
        /// <returns></returns>
        public DataParameter[] GetArrayParameter()
        {
            DataParameter[] ret = null;
            if (ArrFunc.IsFilled(htDataParameter))
            {
                //
                ArrayList aParameters = new ArrayList();
                IEnumerator listEnum = htDataParameter.Values.GetEnumerator();
                while (listEnum.MoveNext())
                    aParameters.Add(((DataParameter)listEnum.Current));
                //
                if (ArrFunc.IsFilled(aParameters))
                    ret = (DataParameter[])aParameters.ToArray(aParameters[0].GetType());
            }
            return ret;
        }
        
        
        /// <summary>
        /// Supprime le paramètre
        /// </summary>
        /// <param name="pParameterKey"></param>
        public void Remove(string pParameterKey)
        {
            htDataParameter.Remove(pParameterKey);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSqlServerParamList()
        {

            string ret = string.Empty;
            ArrayList al = new ArrayList();
            if (this.Count > 0)
            {
                DataParameter[] dataParameter = this.GetArrayParameter();
                for (int i = 0; i < dataParameter.Length; i++)
                    al.Add(DataHelper.GetVarPrefix(DbSvrType.dbSQL) + dataParameter[i].ParameterKey);
            }
            string[] sqlServerParamName = (string[])al.ToArray(typeof(string));
            //
            if (ArrFunc.IsFilled(sqlServerParamName))
            {
                ret = StrFunc.StringArrayList.StringArrayToStringList(sqlServerParamName, false);
                ret = ret.Replace(StrFunc.StringArrayList.LIST_SEPARATOR, ',');
            }
            //
            return ret;

        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            DataParameters ret = null;
            //
            if (ArrFunc.IsFilled(htDataParameter))
            {
                ret = new DataParameters();
                IEnumerator listEnum = htDataParameter.Values.GetEnumerator();
                while (listEnum.MoveNext())
                    ret.Add((DataParameter)(((DataParameter)listEnum.Current).Clone()));
            }
            return ret;
        }
        
        /// <summary>
        /// Copie les paramètres courants vers un DataParameters cible
        /// <para>Pour chaque paramètre, la copie n'est effectuée que si le paramètre n'existe pas dans la cible</para>
        /// </summary>
        /// <param name="parameters"></param>
        public void CopyTo(DataParameters pParameters)
        {
            DataParameter[] parameter = GetArrayParameter();
            if (ArrFunc.IsFilled(parameter))
            {
                foreach (DataParameter item in parameter)
                {
                    if (false == pParameters.Contains(item.ParameterKey))
                        pParameters.Add(item);
                }
            }
        }
        

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// FI 20220128 [XXXXX] Add
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DataParameterAttribute : Attribute
    {
        public DataParameterAttribute(DbType dbtype)
        {
            DbType = dbtype;
        }
        public DataParameterAttribute(DbType dbtype, int size) : this(dbtype)
        {
            Size = size;
        }
        public DataParameterAttribute(DbType dbtype, string name) : this(dbtype)
        {
            Name = name;
        }
        public DataParameterAttribute(DbType dbtype, string name, int size) : this(dbtype)
        {
            Name = name;
            Size = size;
        }

        public String Name { get; set; }

        public DbType DbType { get; set; }

        public Nullable<int> Size { get; set; }
    }

}
