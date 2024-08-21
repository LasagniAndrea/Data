using System;
using System.Data;
using System.Collections;
//
using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EfsML;
using EfsML.Enum;
using EFS.Actor;
using EFS.Book;

// RD 20120809 [18070] Optimisation de la compta
namespace EFS.Common
{

    /// <summary>
    /// 
    /// </summary>
    public enum RoleContractRestrict
    {
        CNF, 
        FEE,
        //FUNDING,
        POSKEEPING,
        TRADING,
    }

    //
    /// <summary>
    /// 
    /// </summary>
    public enum RoleActorBookRestrict
    {
        /// <summary>
        /// 
        /// </summary>
        TRADING,
        /// <summary>
        /// 
        /// </summary>
        FEE,
        /// <summary>
        /// 
        /// </summary>
        COLLATERAL,
    }


    /// <summary>
    /// Classe qui permet de générer les restrictions SQL sur des tables qui contiennent des colonne qui se rapporttent à l'instrument
    /// <para>Table avec les colonnes GPRODUCT/IDP/IDGINSTR/IDI</para>
    /// <para>Table avec les colonnes GPRODUCT/TYPEINSTR/IDINSTR ou TYPEINSTR/IDINSTR ou TYPEINSTR_UNL/IDINSTR_UNL</para>
    /// </summary>
    // EG 20180307 [23769] Gestion dbTransaction
    public class SQLInstrCriteria
    {
        const string S4 = "    ";
        const string S6 = "      ";
        
        #region Members
        /// <summary>
        /// Représente la ConnectionString
        /// </summary>
        private readonly string _cs;
        private readonly IDbTransaction _dbTransaction;
        /// <summary>
        /// Représente l'instrument
        /// <para>La valeur null est possible</para>
        /// </summary>
        private readonly SQL_Instr _sqlInstr;
        /// <summary>
        /// Représente le product auquel est rattaché l'instrument
        /// <para>La valeur null est possible</para>
        /// </summary>
        private readonly SQL_Product _sqlProduct;
        /// <summary>
        /// 
        /// </summary>
        private readonly bool _isContextUNL;
        /// <summary>
        /// 
        /// </summary>
        private readonly bool _isUseGPRODUCT;
        /// <summary>
        /// 
        /// </summary>
        private readonly SQL_Table.ScanDataDtEnabledEnum _scanDataDtEnabled;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        private bool IsUseGPRODUCT
        {
            get { return ((false == _isContextUNL) && _isUseGPRODUCT); }
        }
        #endregion

        #region Constructor
        /// <param name="pCS"></param>
        /// <param name="pIdI">Id de l'instrument. La valeur 0 est autorisée</param>
        /// <param name="pIsContextUNL">Indicateur d'exploitation de colonnes relatives à l'Underlying (ex. TYPEINSTR_UNL)</param>
        /// <param name="pScanDtEnabled"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public SQLInstrCriteria(string pCS, IDbTransaction pDbTransaction, int pIdI, bool pIsContextUNL, SQL_Table.ScanDataDtEnabledEnum pScanDtEnabled)
            : this(pCS, pDbTransaction, pIdI, pIsContextUNL, true, pScanDtEnabled)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdI">Id de l'instrument. La valeur 0 est autorisée</param>
        /// <param name="pIsContextUNL">Indicateur d'exploitation de colonnes relatives à l'Underlying (ex. TYPEINSTR_UNL)</param>
        /// <param name="pIsUseGPRODUCT"></param>
        /// <param name="pScanDtEnabled"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public SQLInstrCriteria(string pCS, IDbTransaction pDbTransaction, int pIdI, bool pIsContextUNL, bool pIsUseGPRODUCT, SQL_Table.ScanDataDtEnabledEnum pScanDtEnabled)
        {
            _cs = pCS;
            _dbTransaction = pDbTransaction;
            _scanDataDtEnabled = pScanDtEnabled;
            _sqlInstr = null;
            _sqlProduct = null;
            _isContextUNL = pIsContextUNL;
            _isUseGPRODUCT = pIsUseGPRODUCT;
            //
            if (pIdI != 0)
            {
                SQL_Instr sqlInstr = new SQL_Instr(_cs, pIdI)
                {
                    DbTransaction = _dbTransaction
                };
                sqlInstr.LoadTable(new string[] { "IDI", "IDP" });
                if (sqlInstr.IsLoaded)
                {
                    _sqlInstr = sqlInstr;

                    SQL_Product sqlProduct = new SQL_Product(_cs, _sqlInstr.IdP)
                    {
                        DbTransaction = _dbTransaction
                    };
                    sqlProduct.LoadTable(new string[] { "GPRODUCT" });
                    if (sqlProduct.IsLoaded)
                    {
                        _sqlProduct = sqlProduct;
                    }
                }
            }
        }
        #endregion

        #region Method
        // EG 20180307 [23769] Gestion dbTransaction
        public string GetSQLRestriction(string pAlias, RoleGInstr pRole)
        {
            return GetSQLRestriction(_cs, pAlias, pRole);
        }
        /// <summary>
        /// Retourne le code SQL qui permet de réduire le jeu de résultat aux seuls éléments valides de la table (représenté par l'alias {pAlias}) vis à vis de l'instrument  
        /// <para>Le code SQL peut s'appliquer à un where ou à un inner</para>
        /// <para>Cette methode s'applique uniquement aux tables qui contiennent les colonnes GPRODUCT/IDP/IDGINSTR/IDI</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSqlAlias">Alias de la table sur laquelle s'applique la restriction</param>
        /// <param name="pRole">Rôle du groupe d'instrument</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public string GetSQLRestriction(string pCs, string pAlias, RoleGInstr pRole)
        {
            StrBuilder sqlRestrict = new StrBuilder(string.Empty);

            if (null != _sqlInstr)
            {
                //20090526 PL Refactoring
                int[] grpInstr = InstrTools.GetGrpInstr(pCs, _dbTransaction, _sqlInstr.IdI, pRole, (SQL_Table.ScanDataDtEnabledEnum.Yes == _scanDataDtEnabled));
                //
                sqlRestrict += "(([alias].GPRODUCT is null) or ([alias].GPRODUCT=" + DataHelper.SQLString(_sqlProduct.GProduct) + "))" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].IDP  is null) or ([alias].IDP=" + _sqlInstr.IdP.ToString() + "))" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].IDI  is null) or ([alias].IDI=" + _sqlInstr.IdI.ToString() + "))" + Cst.CrLf;
                if (ArrFunc.IsFilled(grpInstr))
                    sqlRestrict += SQLCst.AND + "(([alias].IDGINSTR is null) or (" + DataHelper.SQLColumnIn(pCs, "[alias].IDGINSTR", grpInstr, TypeData.TypeDataEnum.integer) + "))" + Cst.CrLf;
                else
                    sqlRestrict += SQLCst.AND + "([alias].IDGINSTR is null)" + Cst.CrLf;
            }
            else
            {
                sqlRestrict += "([alias].GPRODUCT is null)" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "([alias].IDP      is null)" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "([alias].IDI      is null)" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "([alias].IDGINSTR is null)" + Cst.CrLf;
            }

            string ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pAlias);

            ret = "(" + ret + ")";

            return ret;
        }

        /// <summary>
        /// Retourne le code SQL qui permet de réduire le jeu de résultat aux seuls éléments valides de la table (représenté par l'alias {pAlias}) vis à vis de l'instrument  
        /// <para>Le code SQL peut s'appliquer à un where ou à un inner</para>
        /// <para>Cette Methode s'applique uniquement aux tables qui contiennent les colonnes GPRODUCT/TYPEINSTR/IDINSTR ou TYPEINSTR/IDINSTR ou TYPEINSTR_UNL/IDINSTR_UNL</para>
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public string GetSQLRestriction2(string pSqlAlias, Nullable<RoleGInstr> pRole)
        {
            string ret;
            StrBuilder sqlRestrict = new StrBuilder();
            
            string columnTypeInstr = "TYPEINSTR";
            if (_isContextUNL)
                columnTypeInstr += "_UNL";
            
            string columnIdInstr = "IDINSTR";
            if (_isContextUNL)
                columnIdInstr += "_UNL";
            
            if (null != _sqlInstr)
            {
                int[] grpInstr = InstrTools.GetGrpInstr(_cs, _dbTransaction, _sqlInstr.IdI, pRole, (SQL_Table.ScanDataDtEnabledEnum.Yes == _scanDataDtEnabled));

                if (IsUseGPRODUCT)
                {
                    sqlRestrict += S4 + "( ([alias].GPRODUCT is null) or ([alias].GPRODUCT=" + DataHelper.SQLString(_sqlProduct.GProduct) + ") )" + Cst.CrLf;
                    sqlRestrict += S4 + SQLCst.AND + Cst.CrLf;
                }
                
                sqlRestrict += S4 + "(" +Cst.CrLf;
                
                sqlRestrict += S6 + "([alias].[TYPEINSTR] is null)" + Cst.CrLf;
                sqlRestrict += S6 + "or ([alias].[TYPEINSTR]=" + DataHelper.SQLString(TypeInstrEnum.None.ToString()) + ")" + Cst.CrLf;
                sqlRestrict += S6 + "or ( ([alias].[TYPEINSTR]=" + DataHelper.SQLString(TypeInstrEnum.Product.ToString()) + ") and ([alias].[IDINSTR]=" + _sqlInstr.IdP.ToString() + ") )" + Cst.CrLf;
                sqlRestrict += S6 + "or ( ([alias].[TYPEINSTR]=" + DataHelper.SQLString(TypeInstrEnum.Instr.ToString()) + ") and ([alias].[IDINSTR]=" + _sqlInstr.IdI.ToString() + ") )" + Cst.CrLf;

                if (ArrFunc.IsFilled(grpInstr))
                {
                    sqlRestrict += S6 + "or ( ([alias].[TYPEINSTR]=" + DataHelper.SQLString(TypeInstrEnum.GrpInstr.ToString()) + ") and (" + DataHelper.SQLColumnIn(_cs, "[alias].[IDINSTR]", grpInstr, TypeData.TypeDataEnum.integer) + ") )" + Cst.CrLf;
                }

                sqlRestrict += S4 + ")" + Cst.CrLf;
            }
            else
            {
                if (IsUseGPRODUCT)
                {
                    sqlRestrict += S4 + "([alias].GPRODUCT is null)" + Cst.CrLf;
                    sqlRestrict += S4 + SQLCst.AND + Cst.CrLf;
                }

                sqlRestrict += S4 + "( ([alias].[TYPEINSTR] is null) or ([alias].[TYPEINSTR]=" + DataHelper.SQLString(TypeInstrEnum.None.ToString()) + ") )" + Cst.CrLf;
            }
            
            ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pSqlAlias);
            ret = ret.Replace("[TYPEINSTR]", columnTypeInstr);
            ret = ret.Replace("[IDINSTR]", columnIdInstr);
            
            OTCmlHelper.SQLAddSignature(ref ret, "GetSQLRestriction2");

            return ret;
        }
        #endregion
    }

    
    #region SQLMarketCriteria
    /// <summary>
    /// Classe qui permet de générer les restrictions SQL 
    /// <para>lorsque la table contient les colonnes TYPEMARKET/IDMARKET</para>
    /// </summary>
    public class SQLMarketCriteria
    {
        #region Members
        /// <summary>
        /// Représente le marché (null si le contract est disabled)
        /// </summary>
        private readonly SQL_Market _sqlMarket;
        private readonly SQL_Table.ScanDataDtEnabledEnum _scanDataDtEnabled;
        #endregion
        //
        #region Constructor
        public SQLMarketCriteria(string pCS, int pIdM, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {

            _sqlMarket = null;
            _scanDataDtEnabled = pScanDataDtEnabled;
            //
            if (pIdM != 0)
            {
                SQL_Market sqlMarket = new SQL_Market(pCS, pIdM, pScanDataDtEnabled);
                sqlMarket.LoadTable(new string[] { "IDM" });
                if (sqlMarket.IsLoaded)
                    _sqlMarket = sqlMarket;
            }

        }
        #endregion

        #region Method
        /// <summary>
        /// Retourne le code SQL qui permet de réduire le jeu de résultat aux seuls éléments valides de la table (représenté par l'alias {pSqlTableAlias}) vis à vis du market
        /// <para>Le code SQL peut s'appliquer à un where ou à un inner</para>
        /// <para>Cette Methode s'applique uniquement aux tables qui contiennent les colonnes TYPEMARKET/IDMARKET</para>
        /// </summary>
        /// FI 20170908 [23409] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        public string GetSQLRestriction(string pCs, IDbTransaction pDbTransaction, string pSqlTableAlias, Nullable<RoleGMarket> pRole)
        {
            StrBuilder sqlRestrict = new StrBuilder(string.Empty);

            if (null != _sqlMarket)
            {
                sqlRestrict += "([alias].TYPEMARKET is null)" + Cst.CrLf;
                sqlRestrict += SQLCst.OR + Cst.CrLf;
                sqlRestrict += "([alias].TYPEMARKET=" + DataHelper.SQLString(TypeMarketEnum.None.ToString()) + ")" + Cst.CrLf;
                sqlRestrict += SQLCst.OR + Cst.CrLf;
                // FI 20240219 [WI851] Add isnull
                sqlRestrict += "(([alias].TYPEMARKET=" + DataHelper.SQLString(TypeMarketEnum.Market.ToString()) + ") and (isnull([alias].IDMARKET,0)=" + _sqlMarket.Id.ToString() + "))" + Cst.CrLf;

                int[] grpMarket = MarketTools.GetGrpMarket(pCs, pDbTransaction,_sqlMarket.Id, pRole, (_scanDataDtEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes));
                if (ArrFunc.IsFilled(grpMarket))
                {
                    sqlRestrict += SQLCst.OR + Cst.CrLf;
                    sqlRestrict += "(([alias].TYPEMARKET=" + DataHelper.SQLString(TypeMarketEnum.GrpMarket.ToString()) + ") and (" + DataHelper.SQLColumnIn(pCs, "[alias].IDMARKET", grpMarket, TypeData.TypeDataEnum.integer) + "))" + Cst.CrLf;
                }
            }
            else
            {
                // FI 20170908 [23409] Use TypeMarketEnum.None à la place de TypeInstrEnum.None
                sqlRestrict += "(([alias].TYPEMARKET is null) or ([alias].TYPEMARKET=" + DataHelper.SQLString(TypeMarketEnum.None.ToString()) + "))" + Cst.CrLf;
            }
            //
            string ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pSqlTableAlias);
            //	
            ret = "(" + ret + ")";
            //
            return ret;

        }
        #endregion

    }
    #endregion SQLMarketCriteria

    /// <summary>
    /// Classe qui permet de générer les restrictions SQL 
    /// <para>lorsque la table contient les colonnes [xxx]TYPEPARTY[xxx]/[xxx]IDPARTY[xxx]</para>
    /// </summary>
    public class SQLActorBookCriteria
    {
        const string S4 = "    ";

        #region Members
        /// <summary>
        /// Représente l'acteur, null si inconnu (exemple s'il est disabled)  
        /// </summary>
        private readonly SQL_Actor _sqlActor;
        /// <summary>
        /// Représente le book, null si inconnu (exemple s'il est disabled)  
        /// </summary>
        private readonly SQL_Book _sqlBook;
        /// <summary>
        /// Représente la liste des rôles de l'acteur 
        /// </summary>
        private RoleActor[] _roleActor;
        /// <summary>
        /// 
        /// </summary>
        private readonly SQL_Table.ScanDataDtEnabledEnum _scanDataEnabled;
        /// <summary>
        /// Représente le nom de colonne TYPEPARTY
        /// </summary>
        private string _columnTYPEPARTY;
        /// <summary>
        /// Représente le nom de colonne IDPARTY
        /// </summary>
        private string _columnIDPARTY;
        /// <summary>
        /// Représente le nom de colonne IDROLE
        /// </summary>
        private string _columnIDROLE;
        #endregion

        #region Accessor
        /// <summary>
        /// Obtient ou définit la colonnes TYPEPARTY
        /// </summary>
        public string ColumnTYPEPARTY
        {
            get { return _columnTYPEPARTY; }
            set { _columnTYPEPARTY = value; }
        }

        /// <summary>
        /// Obtient ou définit la colonnes IDPARTY
        /// </summary>
        public string ColumnIDPARTY
        {
            get { return _columnIDPARTY; }
            set { _columnIDPARTY = value; }
        }

        /// <summary>
        /// Obtient ou définit la colonnes IDROLE
        /// </summary>
        public string ColumnIDROLE
        {
            get { return _columnIDROLE; }
            set { _columnIDROLE = value; }
        }

        /// <summary>
        /// Obtient ou définit la liste des rôles de l'acteur 
        /// </summary>
        public RoleActor[] RoleActor
        {
            get { return _roleActor; }
            set { _roleActor = (RoleActor[])value; }
        }
        #endregion

        #region Constructor
        // EG 20180205 [23769] Add dbTransaction  
        public SQLActorBookCriteria(string pCS, int pIdA, int pIdB, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
            : this(pCS, null as IDbTransaction, pIdA, pIdB, pScanDataDtEnabled) { }
        // EG 20180205 [23769] Add dbTransaction  
        public SQLActorBookCriteria(string pCS, IDbTransaction pDbTransaction, int pIdA, int pIdB, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            _columnTYPEPARTY = "TYPEPARTY";
            _columnIDPARTY = "IDPARTY";
            _columnIDROLE = "IDROLE";
            _sqlActor = null;
            _sqlBook = null;
            _scanDataEnabled = pScanDataDtEnabled;
            //
            if (pIdA != 0)
            {
                SQL_Actor sqlActor = new SQL_Actor(pCS, pIdA, SQL_Table.RestrictEnum.No, _scanDataEnabled, null, string.Empty)
                {
                    DbTransaction = pDbTransaction
                };
                sqlActor.LoadTable(new string[] { "IDA" });
                if (sqlActor.IsLoaded)
                    _sqlActor = sqlActor;
            }
            //
            if (pIdB > 0)
            {
                SQL_Book sqlBook = new SQL_Book(pCS, pIdB, _scanDataEnabled)
                {
                    DbTransaction = pDbTransaction
                };

                sqlBook.LoadTable(new string[] { "IDB" });
                if (sqlBook.IsLoaded)
                    _sqlBook = sqlBook;
            }
        }
        #endregion

        #region Method
        // EG 20180205 [23769] Add dbTransaction  
        public string GetSQLRestrictionOnMandatory(string pCs, string pSqlTableAlias, Nullable<RoleActorBookRestrict> pRole)
        {
            return GetSQLRestrictionOnMandatory(pCs, null, pSqlTableAlias, pRole);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public string GetSQLRestrictionOnMandatory(string pCs, IDbTransaction pDbTransaction, string pSqlTableAlias, Nullable<RoleActorBookRestrict> pRole)
        {
            string ret = GetSQLRestriction(pCs, pDbTransaction, pSqlTableAlias, pRole);
            //PL 20120903 Bidouille à perfectionner ...
            if ((_sqlActor != null) || (_sqlBook != null))
            {
                ret = ret.Replace(S4+ "(" + pSqlTableAlias + "." + ColumnTYPEPARTY + " is null)" + Cst.CrLf, string.Empty);
                ret = ret.Replace(S4 + "or (" + pSqlTableAlias + "." + ColumnTYPEPARTY + "=" + DataHelper.SQLString(TypePartyEnum.None.ToString()) + ")" + Cst.CrLf, string.Empty);
                int posFirstOR = ret.IndexOf("or ");
                ret = ret.Remove(posFirstOR, "or ".Length);
            }
            OTCmlHelper.SQLAddSignature(ref ret, "GetSQLRestriction"); 
            return ret;
        }
        // EG 20180205 [23769] Add dbTransaction  
        public string GetSQLRestrictionOnMandatoryB(string pCs, IDbTransaction pDbTransaction, string pSqlTableAlias, Nullable<RoleActorBookRestrict> pRole)
        {
            string ret = GetSQLRestriction(pCs, pDbTransaction, pSqlTableAlias, pRole);
            //PL 20120903 Bidouille à perfectionner ...
            if ((_sqlActor != null) || (_sqlBook != null))
            {
                ret = ret.Replace(S4 + "(" + pSqlTableAlias + "." + ColumnTYPEPARTY + " is null)" + Cst.CrLf, string.Empty);
                ret = ret.Replace(S4 + "or (" + pSqlTableAlias + "." + ColumnTYPEPARTY + "=" + DataHelper.SQLString(TypePartyEnum.None.ToString()) + ")" + Cst.CrLf, string.Empty);
                ret = ret.Replace(S4 + "or (" + pSqlTableAlias + "." + ColumnTYPEPARTY + "=" + DataHelper.SQLString(TypePartyEnum.All.ToString()) + ")" + Cst.CrLf, string.Empty);
                int posFirstOR = ret.IndexOf("or ");
                ret = ret.Remove(posFirstOR, "or ".Length);
            }
            OTCmlHelper.SQLAddSignature(ref ret, "GetSQLRestriction");
            return ret;
        }
        /// <summary>
        /// Retourne le code SQL qui permet de réduire le jeu de résultat aux seuls éléments valides de la table (représenté par l'alias {pSqlTableAlias}) vis à vis du market
        /// <para>Le code SQL peut s'appliquer à un where ou à un inner</para>
        /// <para>Cette Methode s'applique uniquement aux tables qui contiennent les colonnes TYPEPARTY/IDPARTY</para>
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        public string GetSQLRestrictionAndSignature(string pCs, IDbTransaction pDbTransaction, string pSqlTableAlias, Nullable<RoleActorBookRestrict> pRole)
        {
            string ret = GetSQLRestriction(pCs, pDbTransaction, pSqlTableAlias, pRole);
            OTCmlHelper.SQLAddSignature(ref ret, "GetSQLRestriction");
            return ret;
        }
        // EG 20180205 [23769] Add dbTransaction  
        private string GetSQLRestriction(string pCs, IDbTransaction pDbTransaction, string pSqlTableAlias, Nullable<RoleActorBookRestrict> pRole)
        {
            string ret;
            StrBuilder sqlRestrict = new StrBuilder();

            if ((null != _sqlActor) || (null != _sqlBook))
            {
                sqlRestrict += S4 + "([alias].[TYPEPARTY] is null)" + Cst.CrLf;
                sqlRestrict += S4 + "or ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.None.ToString()) + ")" + Cst.CrLf;
                sqlRestrict += S4 + "or ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.All.ToString()) + ")" + Cst.CrLf;

                #region ACTOR
                if (_sqlActor != null)
                {
                    sqlRestrict += S4 + "or ( " + "([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.Actor.ToString()) + ") and ([alias].[IDPARTY]=" + _sqlActor.Id.ToString() + ")";
                    if (ArrFunc.IsFilled(_roleActor))
                    {
                        sqlRestrict += " and ( (" + DataHelper.SQLColumnIn(pCs, "[alias].[IDROLE]", _roleActor, TypeData.TypeDataEnum.@string) + ") or ([alias].[IDROLE] is null) )";
                    }
                    sqlRestrict += " )" + Cst.CrLf;

                    Nullable<RoleGActor> roleGActor = null;
                    if (null != pRole)
                    {
                        roleGActor = (RoleGActor)Enum.Parse(typeof(RoleGActor), pRole.ToString());
                    }
                    int[] grpActor = ActorTools.GetGrpActor(pCs, pDbTransaction,
                        _sqlActor.Id, roleGActor.ToString(), (_scanDataEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes));
                    if (ArrFunc.IsFilled(grpActor))
                    {
                        sqlRestrict += S4 + "or ( ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.GrpActor.ToString()) + ") and (" + DataHelper.SQLColumnIn(pCs, "[alias].[IDPARTY]", grpActor, TypeData.TypeDataEnum.integer) + ") )" + Cst.CrLf;
                    }
                }
                #endregion 
                #region BOOK
                if (_sqlBook != null)
                {
                    sqlRestrict += S4 + "or ( ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.Book.ToString()) + ") and ([alias].[IDPARTY]=" + _sqlBook.Id.ToString() + ") )" + Cst.CrLf;
                    
                    Nullable<RoleGBook> roleGBook = null;
                    if (null != pRole)
                    {
                        roleGBook = (RoleGBook)Enum.Parse(typeof(RoleGBook), pRole.ToString());
                    }
                    int[] grpBook = BookTools.GetGrpBook(pCs, pDbTransaction, 
                        _sqlBook.Id, roleGBook, (_scanDataEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes));
                    if (ArrFunc.IsFilled(grpBook))
                    {
                        sqlRestrict += S4 + "or ( ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.GrpBook.ToString()) + ") and (" + DataHelper.SQLColumnIn(pCs, "[alias].[IDPARTY]", grpBook, TypeData.TypeDataEnum.integer) + ") )" + Cst.CrLf;
                    }
                }
                #endregion
            }
            else
            {
                sqlRestrict += S4 + "( ([alias].[TYPEPARTY] is null) or ([alias].[TYPEPARTY]=" + DataHelper.SQLString(TypePartyEnum.None.ToString()) + ") )" + Cst.CrLf;
            }
            
            ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pSqlTableAlias);
            ret = ret.Replace("[TYPEPARTY]", ColumnTYPEPARTY);
            ret = ret.Replace("[IDPARTY]", ColumnIDPARTY);
            ret = ret.Replace("[IDROLE]", ColumnIDROLE);

            return ret;
        }
        #endregion
    }



    /// <summary>
    /// Classe qui permet de générer les restrictions SQL lorsque la table contient les colonnes TYPECONTRACT/IDCONTRACT
    /// </summary>
    /// FI 20170908 [23409] Add (Replace of SQLDerivativeContractCriteria) 
    // EG 20180307 [23769] Gestion dbTransaction
    public class SQLContractCriteria
    {
        const string S4 = "    ";
        const string S6 = "      ";
        const string S8 = "        ";
        const string S10 = "         ";
        const string S12 = "           ";

        #region Members
        /// <summary>
        /// Représente la ConnectionString
        /// </summary>
        private readonly string _cs;
        private readonly IDbTransaction _dbTransaction;
        /// <summary>
        /// Représente le Contract (pourrait être null si le contract est disabled) ou s'il n'existe pas de contract
        /// <para>DerivativeContract ou CommodityContract</para>
        /// </summary>
        private Pair<Cst.ContractCategory, SQL_TableWithID> _sqlContract;
        /// <summary>
        /// Représente le Market Généralement utilisé dans le cas où l'actif ne s'appuie pas sur un Contract, par exemple sur un ReturnSwap.)
        /// </summary>
        private SQL_Market _sqlMarket;
        /// <summary>
        /// 
        /// </summary>
        private readonly SQL_Table.ScanDataDtEnabledEnum _scanDataDtEnabled;

        #endregion

        #region Accessor
        /// <summary>
        /// Obtient ou définit l'indicateur qui active l'utilisation des colonnes  TYPECONTRACTEXCEPT,IDCONTRACTEXCEPT
        /// <para>Valeurs attendues : True si référentiel avec colonnes TYPECONTRACTEXCEPT,IDCONTRACTEXCEPT, False sinon   </para>
        /// </summary>
        public bool IsUseColumnExcept
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pContract">Type de contract avec son Id. La valeur null est acceptée.</param>
        /// <param name="pIdM">Id du Market. La valeur 0 est acceptée.</param>
        /// <param name="pScanDataDtEnabled"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public SQLContractCriteria(string pCS, IDbTransaction pDbTransaction, Pair<Cst.ContractCategory, int> pContract, int pIdM, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            _cs = pCS;
            _dbTransaction = pDbTransaction;
            IsUseColumnExcept = false;
            _sqlContract = null;
            _sqlMarket = null;
            _scanDataDtEnabled = pScanDataDtEnabled;

            if (null != pContract)
            {
                InitializeFromContract(pContract);
            }
            else if (pIdM != 0)
            {
                InitializeFromMarket(pIdM);
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Alimente _sqlContract
        /// </summary>
        /// <param name="pContract"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        private void InitializeFromContract(Pair<Cst.ContractCategory, int> pContract)
        {
            if (pContract == null)
                throw new ArgumentNullException("Argument pContract: null Value is not allowed");
            if (pContract.Second == 0)
                throw new ArgumentException("Argument pContract: 0 Value is not allowed");

            switch (pContract.First)
            {
                case Cst.ContractCategory.CommodityContract:
                    SQL_CommodityContract sqlCommodityContract = new SQL_CommodityContract(_cs, pContract.Second, _scanDataDtEnabled)
                    {
                        DbTransaction = _dbTransaction
                    };
                    sqlCommodityContract.LoadTable(new string[] { "IDCC", "IDM" });
                    if (sqlCommodityContract.IsLoaded)
                        _sqlContract = new Pair<Cst.ContractCategory, SQL_TableWithID>(pContract.First, sqlCommodityContract);
                    break;
                case Cst.ContractCategory.DerivativeContract:
                    SQL_DerivativeContract sqlDerivativeContract = new SQL_DerivativeContract(_cs, pContract.Second, _scanDataDtEnabled)
                    {
                        DbTransaction = _dbTransaction
                    };
                    sqlDerivativeContract.LoadTable(new string[] { "IDDC", "IDM" });
                    if (sqlDerivativeContract.IsLoaded)
                        _sqlContract = new Pair<Cst.ContractCategory, SQL_TableWithID>(pContract.First, sqlDerivativeContract);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Value : {0} is not implemeted", pContract.First.ToString()));
            }
        }

        /// <summary>
        /// Alimente _sqlMarket
        /// </summary>
        /// <param name="pIdM"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        private void InitializeFromMarket(int pIdM)
        {
            if (pIdM == 0)
                throw new ArgumentException("Argument pIdM: Value 0 is not allowed");

            SQL_Market sqlMarket = new SQL_Market(_cs, pIdM, _scanDataDtEnabled)
            {
                DbTransaction = _dbTransaction
            };
            sqlMarket.LoadTable(new string[] { "IDM" });
            if (sqlMarket.IsLoaded)
                _sqlMarket = sqlMarket;
        }

        /// <summary>
        /// Retourne le code SQL qui permet de réduire le jeu de résultat aux seuls éléments valides de la table (représenté par l'alias {pAlias}) vis à vis du derivativeContract
        /// <para>Le code SQL peut s'appliquer à un where ou à un inner</para>
        /// <para>Cette methode s'applique uniquement aux tables qui contiennent les colonnes TYPECONTRACT/IDCONTRACT</para>
        /// </summary>
        public string GetSQLRestriction(string pSqlAlias, Nullable<RoleContractRestrict> pRole)
        {
            string ret;
            StrBuilder sqlRestrict = new StrBuilder();

            //20140721 Newness _sqlMarket
            if ((null != _sqlContract) || (null != _sqlMarket))
            {
                sqlRestrict += GetSQLRestrict(false, pRole);
                if (IsUseColumnExcept)
                {
                    sqlRestrict += S4 + SQLCst.AND + Cst.CrLf;
                    sqlRestrict += GetSQLRestrict(true, pRole);
                }
            }
            else
            {
                sqlRestrict += S4 + "( ([alias].TYPECONTRACT is null) or ([alias].TYPECONTRACT=" + DataHelper.SQLString(TypeInstrEnum.None.ToString()) + ") )" + Cst.CrLf;
            }

            ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pSqlAlias);

            OTCmlHelper.SQLAddSignature(ref ret, "GetSQLRestriction");

            return ret;
        }

        /// <summary>
        /// Retourne la restriction SQL qui permet d'inclure ou d'exclure du jeu de résultat le DerivativeContract 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIsExclude">Si true, requête d'exclusion</param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        private string GetSQLRestrict(bool pIsExclude, Nullable<RoleContractRestrict> pRole)
        {

            if ((null == _sqlContract) && (null == _sqlMarket))
                throw new NotSupportedException("_sqlContract and _sqlMarket are null"); 
            
            
            string ret;
            string Sx = S8;

            int idM = 0;
            if (null != _sqlContract)
            {
                if (null != _sqlContract.Second.GetFirstRowColumnValue("IDM"))
                    idM = Convert.ToInt32(_sqlContract.Second.GetFirstRowColumnValue("IDM"));
            }
            else if (null != _sqlMarket)
            {
                idM = _sqlMarket.Id;
            }


            StrBuilder sqlRestrict = new StrBuilder();

            sqlRestrict += S6 + "(" + Cst.CrLf;
            sqlRestrict += S8 + "([alias].[TYPECONTRACT] is null)" + Cst.CrLf;
            sqlRestrict += S8 + "or ([alias].[TYPECONTRACT]=" + DataHelper.SQLString(TypeContractEnum.None.ToString()) + ")" + Cst.CrLf;

            sqlRestrict += S8 + "or";
            if (pIsExclude)
            {
                sqlRestrict += SQLCst.NOT + Cst.CrLf;
                sqlRestrict += S10 + "(" + Cst.CrLf;
                Sx = S12;
                sqlRestrict += Sx;
            }
            // FI 20240219 [WI851] Add isnull
            sqlRestrict += "( ([alias].[TYPECONTRACT]=" + DataHelper.SQLString(TypeContractEnum.Market.ToString()) + ") and (isnull([alias].[IDCONTRACT],0)=" + idM.ToString() + ") )" + Cst.CrLf;

            Nullable<RoleGMarket> roleGMarket = null;
            if (null != pRole)
                roleGMarket = (RoleGMarket)Enum.Parse(typeof(RoleGMarket), pRole.ToString());


            int[] grpMarket = MarketTools.GetGrpMarket(_cs, _dbTransaction, idM, roleGMarket, _scanDataDtEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes);
            if (ArrFunc.IsFilled(grpMarket))
            {
                sqlRestrict += Sx + "or ( ([alias].[TYPECONTRACT]=" + DataHelper.SQLString(TypeContractEnum.GrpMarket.ToString()) + ") and (" + DataHelper.SQLColumnIn(_cs, "[alias].[IDCONTRACT]", grpMarket, TypeData.TypeDataEnum.integer) + ") )" + Cst.CrLf;
            }

            //Rq.: Lorsque _sqlMarket l'appel est opéré sur la base d'un contract.
            if (null != _sqlContract)
            {
                Nullable<RoleGContract> roleGContrat = null;
                int[] grpContract = null;
                if (null != pRole)
                    roleGContrat = (RoleGContract)Enum.Parse(typeof(RoleGContract), pRole.ToString());
                if (null != _sqlContract)
                {
                    grpContract = ContractTools.GetGrpContract(_cs, _dbTransaction,
                        new Pair<Cst.ContractCategory, int>(_sqlContract.First, _sqlContract.Second.Id),
                        roleGContrat, (_scanDataDtEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes));
                }

                if (ArrFunc.IsFilled(grpContract))
                {
                    sqlRestrict += Sx + "or ( ([alias].[TYPECONTRACT]=" + DataHelper.SQLString(TypeContractEnum.GrpContract.ToString()) + ") and (" + DataHelper.SQLColumnIn(_cs, "[alias].[IDCONTRACT]", grpContract, TypeData.TypeDataEnum.integer) + ") )" + Cst.CrLf;
                }


                TypeContractEnum typeContract;
                switch (_sqlContract.First)
                {
                    case Cst.ContractCategory.DerivativeContract:
                        typeContract = TypeContractEnum.DerivativeContract;
                        break;
                    case Cst.ContractCategory.CommodityContract:
                        typeContract = TypeContractEnum.CommodityContract;
                        break;
                    default:
                        throw new NotSupportedException(StrFunc.AppendFormat("typeContract value {0} is not implemented", _sqlContract.First));
                }
                // FI 20240219 [WI851] Add isnull
                sqlRestrict += Sx + "or ( ([alias].[TYPECONTRACT]=" + DataHelper.SQLString(typeContract.ToString()) + ") and (isnull([alias].[IDCONTRACT],0)=" + _sqlContract.Second.Id.ToString() + ") )" + Cst.CrLf;
            }

            if (pIsExclude)
            {
                sqlRestrict += S10 + ")" + Cst.CrLf;
            }

            sqlRestrict += S6 + ")" + Cst.CrLf;

            ret = sqlRestrict.ToString();

            if (pIsExclude)
            {
                ret = ret.Replace("[TYPECONTRACT]", "TYPECONTRACTEXCEPT");
                ret = ret.Replace("[IDCONTRACT]", "IDCONTRACTEXCEPT");
            }
            else
            {
                ret = ret.Replace("[TYPECONTRACT]", "TYPECONTRACT");
                ret = ret.Replace("[IDCONTRACT]", "IDCONTRACT");
            }

            ret = S4 + "(" + Cst.CrLf + ret + S4 + ")" + Cst.CrLf;

            return ret;
        }
        #endregion
    }


}