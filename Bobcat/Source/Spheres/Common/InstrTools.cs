using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.ApplicationBlocks;
using EFS.ApplicationBlocks.Data;
using EfsML.Enum;


namespace EFS.Common
{

    /// <summary>
    /// Description résumée de InstrTools.
    /// </summary>
    public sealed class InstrTools
    {
        #region constructor
        public InstrTools() { }
        #endregion

        /// <summary>
        /// Retourne la liste des groupes d'instruments auxquels appartient l'instrument
        /// <para>Retourne null si l'instrument n'appartient à aucun groupe</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        /// <param name="pRole">Réduit la liste aux seuls groupes du rôle {pRole}, valeur null autorisé</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static int[] GetGrpInstr(string pCs, int pIdI, Nullable<RoleGInstr> pRole, bool pIsUseDataDtEnabled)
        {
            return GetGrpInstr(pCs, null, pIdI, pRole, pIsUseDataDtEnabled);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetGrpInstr(string pCs, IDbTransaction pDbTransaction, int pIdI, Nullable<RoleGInstr> pRole, bool pIsUseDataDtEnabled)
        {
            int[] ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDI", DbType.Int32), pIdI);

            //PL 20111021 Add all GetSQLDataDtEnabled() and use GINSTR
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "gi.IDGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.GINSTR.ToString() + " gi" + Cst.CrLf;

            if (pRole != null)
            {
                dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEGINSTR), pRole.ToString());

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GINSTRROLE.ToString() + " gir on gir.IDGINSTR=gi.IDGINSTR" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "gir.IDROLEGINSTR=@IDROLEGINSTR" + Cst.CrLf;
                if (pIsUseDataDtEnabled)
                    sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gir") + Cst.CrLf;
            }

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRG.ToString() + " ig on ig.IDGINSTR=gi.IDGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "ig.IDI=@IDI" + Cst.CrLf;
            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "ig") + Cst.CrLf;

            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gi");

            QueryParameters qryParameters = new QueryParameters(pCs, sqlSelect.ToString(), dp);

            ArrayList al = new ArrayList();

            using (IDataReader dr = DataHelper.ExecuteDataTable(qryParameters.Cs, pDbTransaction, 
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr[0]));
            }
            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnInstrument"></param>
        /// <param name="pSqlAlias"></param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static string GetSQLJoinCriteriaInstr(string pColumnInstrument, string pSqlAlias, RoleGInstr pRole)
        {
            StrBuilder sqlRestrict = new StrBuilder(string.Empty);

            if (StrFunc.IsFilled(pColumnInstrument))
            {
                sqlRestrict += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_INSTR_PRODUCT + " v_ip" + Cst.CrLf;
                sqlRestrict += SQLCst.ON + "v_ip.IDI = " + pColumnInstrument + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].GPRODUCT is null) or ([alias].GPRODUCT = v_ip.GProduct))" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].IDP  is null) or ([alias].IDP = v_ip.IdP ))" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].IDI  is null) or ([alias].IDI = v_ip.IdI ))" + Cst.CrLf;
                sqlRestrict += SQLCst.AND + "(([alias].IDGINSTR is null) or ( [alias].IDGINSTR in (" + Cst.CrLf;
                sqlRestrict += SQLCst.SELECT + "ig.IDGINSTR" + Cst.CrLf;
                sqlRestrict += SQLCst.FROM_DBO + "INSTRG ig" + Cst.CrLf;
                sqlRestrict += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GINSTRROLE.ToString() + " gInstrRole on gInstrRole.IDGINSTR =ig.IDGINSTR" + Cst.CrLf;
                sqlRestrict += "and  gInstrRole.IDROLEGINSTR = " + DataHelper.SQLString(pRole.ToString()) + Cst.CrLf;
                sqlRestrict += SQLCst.WHERE + "ig.IDI = v_ip.IDI " + Cst.CrLf;
                sqlRestrict += "))) " + Cst.CrLf;
            }

            string ret = sqlRestrict.ToString();
            ret = ret.Replace("[alias]", pSqlAlias);
            //			
            return ret;

        }

        /// <summary>
        /// Obtient le marginingMode pour un Dealer et un instrument donné
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        /// <returns></returns>
        /// EG 20150408 [POC]
        public static Nullable<Cst.MarginingMode> GetActorMarginingMode(string pCS, int pIdI, int pIdA)
        {
            Nullable<Cst.MarginingMode> marginingMode = null;
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdA);
            dataParameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), pIdI);
            string sqlQuery = @"select MARGININGMODE from dbo.IMREQINSTRPARAM  where (IDA = @IDA) and (IDI = @IDI)";
            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                marginingMode = (Cst.MarginingMode)ReflectionTools.EnumParse(new Cst.MarginingMode(), obj.ToString());
            return marginingMode;
        }


        /// <summary>
        /// Obtient le template et le screen par défaut associé à l'instrument {pIdI}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        /// <returns></returns>
        public static StringData[] GetDefaultInstrumentGui(string pCS, int pIdI)
        {
            SearchInstrumentGUI searchInstrGui = new SearchInstrumentGUI(pIdI);
            return searchInstrGui.GetDefault(pCS, true);
        }

        /// <summary>
        /// Obtient le template et le screen par défaut associé à l'instrument {pInstrIdentifier}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pInstrIdentifier"></param>
        /// <returns></returns>
        public static StringData[] GetDefaultInstrumentGui(string pCS, string pInstrIdentifier)
        {
            SearchInstrumentGUI searchInstrGui = new SearchInstrumentGUI(pInstrIdentifier);
            return searchInstrGui.GetDefault(pCS, true);
        }

        /// <summary>
        /// Retourne la liste des Extensions d'un instrument
        /// </summary>
        public static DataRow[] GetInstrExtension(string pCs, string pOrderBy)
        {
            return GetInstrExtension(pCs, 0, 0, pOrderBy);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDI"></param>
        /// <param name="pIdP"></param>
        /// <returns></returns>
        public static DataRow[] GetInstrExtension(string pCs, int pIDI, int pIdP)
        {
            return GetInstrExtension(pCs, pIDI, pIdP, "dextd.SEQUENCENO");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDI"></param>
        /// <param name="pIdP"></param>
        /// <param name="pOrderBy"></param>
        /// <returns></returns>
        public static DataRow[] GetInstrExtension(string pCs, int pIDI, int pIdP, string pOrderBy)
        {
            try
            {
                DataParameters parametres = new DataParameters();
                string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                sqlQuery += "dext.IDDEFINEEXTEND, dext.IDENTIFIER, dext.DISPLAYNAME, dext.DESCRIPTION, dext.TYPEINSTR, dext.IDINSTR," + Cst.CrLf;
                sqlQuery += "dextd.IDDEFINEEXTENDDET, dextd.SEQUENCENO, dextd.IDENTIFIER" + SQLCst.AS + "IDENTIFIERDET, dextd.DISPLAYNAME" + SQLCst.AS + "DISPLAYNAMEDET," + Cst.CrLf;
                sqlQuery += "dextd.DESCRIPTION" + SQLCst.AS + "DESCRIPTIONDET," + Cst.CrLf;
                sqlQuery += "dextd.WEBCONTROLTYPE, dextd.STYLE, dextd.ISPOSTBACK, dextd.SCHEME, dextd.DATATYPE, dextd.ISMANDATORY," + Cst.CrLf;
                sqlQuery += "dextd.REGULAREXPRESSION, dextd.LISTRETRIEVAL, dextd.DEFAULTVALUE, dextd.DOCUMENTATION" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.DEFINEEXTEND + " dext" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DEFINEEXTENDDET + " dextd" + Cst.CrLf;
                sqlQuery += SQLCst.ON + "dextd.IDDEFINEEXTEND = dext.IDDEFINEEXTEND" + Cst.CrLf;

                sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "dext") + Cst.CrLf;

                if (pIDI > 0)
                {
                    sqlQuery += SQLCst.AND + "(( dext.TYPEINSTR" + SQLCst.IS_NULL + ")" + Cst.CrLf;
                    sqlQuery += SQLCst.OR + "( dext.TYPEINSTR = @TYPEINSTR" + SQLCst.AND + "dext.IDINSTR = @IDI )" + Cst.CrLf;

                    sqlQuery += SQLCst.OR + "( dext.TYPEINSTR = @TYPEGINSTR" + SQLCst.AND + "dext.IDINSTR in (" + Cst.CrLf;
                    sqlQuery += SQLCst.SELECT + "ig.IDGINSTR" + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + "INSTRG ig " + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "ig.IDI = @IDI" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "ig") + "))" + Cst.CrLf;

                    sqlQuery += SQLCst.OR + "( dext.TYPEINSTR = @TYPEPRODUCT" + SQLCst.AND + "dext.IDINSTR = @IDP )) " + Cst.CrLf;

                    parametres.Add(new DataParameter(pCs, "IDI", DbType.Int32), pIDI);
                    parametres.Add(new DataParameter(pCs, "IDP", DbType.Int32), pIdP);
                    parametres.Add(new DataParameter(pCs, "TYPEINSTR", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), TypeInstrEnum.Instr);
                    parametres.Add(new DataParameter(pCs, "TYPEGINSTR", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), TypeInstrEnum.GrpInstr);
                    parametres.Add(new DataParameter(pCs, "TYPEPRODUCT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), TypeInstrEnum.Product);
                }
                sqlQuery += SQLCst.ORDERBY + pOrderBy;

                DataSet dsInstrExtension = DataHelper.ExecuteDataset(pCs, CommandType.Text, sqlQuery, parametres.GetArrayDbParameter());
                DataTable dtInstrExtension = dsInstrExtension.Tables[0];
                DataRow[] drInstrExtension = dtInstrExtension.Select();

                return drInstrExtension;
            }
            catch (Exception ex)
            {
                string logMessage = StrFunc.GetProcessLogMessage("Error to load instrument extension", "[Instrument (id=" + pIDI.ToString() + ")]", ex.Message);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
            }
        }

        /// <summary>
        /// Retourne le 1er groupe d'instrument auquel est rattaché l'instrument [Warning: un instrument peut appartenir à plusieur groupe]
        /// <para>Retourne null si l'instrument n'appartient à aucun groupe</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdRoleGInstr"></param>
        /// <returns></returns>
        public static Nullable<int> GetIdGInstr(string pCS, int pIdI, RoleGInstr pIdRoleGInstr)
        {
            return GetIdGInstr(pCS, null, pIdI, pIdRoleGInstr);
        }
        /// <summary>
        /// Retourne le 1er groupe d'instrument auquel est rattaché l'instrument [Warning: un instrument peut appartenir à plusieur groupe]
        /// <para>Retourne null si l'instrument n'appartient à aucun groupe</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdRoleGInstr"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        public static Nullable<int> GetIdGInstr(string pCS, IDbTransaction pDbTransaction, int pIdI, RoleGInstr pIdRoleGInstr)
        {

            Nullable<int> ret = null;

            //PL 20111021 Add all GetSQLDataDtEnabled() and use GINSTR
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "gi.IDGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.GINSTR.ToString() + " gi" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GINSTRROLE.ToString() + " gir on gir.IDGINSTR=gi.IDGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "gir.IDROLEGINSTR=@IDROLEGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gir") + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRG.ToString() + " ig on ig.IDGINSTR=gi.IDGINSTR" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "ig.IDI=@IDI" + Cst.CrLf;
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ig") + Cst.CrLf;

            sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gi");

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), pIdI);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEGINSTR), pIdRoleGInstr.ToString());

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (null != obj)
                ret = Convert.ToInt32(obj);

            return ret;

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SearchInstrumentGUI
    {
        #region Members
        // EG 20160404 Migration vs2013
        //private string _cs;
        private readonly string _instrIdentifier;
        private readonly int _idI;
        #endregion

        #region constructor
        /// <summary>
        /// Constructor à utiliser avec l'idi de l'instrument
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        public SearchInstrumentGUI(int pIdI)
            : this(pIdI, string.Empty)
        {
        }
        /// <summary>
        /// Constructor à utiliser avec l'identifier de l'instrument
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pInstrIdentifier"></param>
        public SearchInstrumentGUI(string pInstrIdentifier)
            : this(0, pInstrIdentifier)
        {
        }

        public SearchInstrumentGUI(int pIdI, string pInstrIdentifier)
        {
            _idI = pIdI;
            _instrIdentifier = pInstrIdentifier;
        }
        #endregion

        #region public Method

        /// <summary>
        /// Retourne les écrans associés au template pTemplateIdentifier
        /// Les paramétrages ISDEFAULT=1 sont prioritaires
        /// </summary>
        /// <param name="pTemplateIdentifier"></param>
        /// <returns></returns>
        public StringData[] GetDefaultFromTemplate(string pCS, string pTemplateIdentifier)
        {
            QueryParameters queryParameters = GetQuery(pCS);
            queryParameters.Parameters.Add(new DataParameter(pCS, "TEMPLATEIDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pTemplateIdentifier);
            queryParameters.Query = queryParameters.Query + Cst.CrLf + SQLCst.WHERE + "(tr.IDENTIFIER = @TEMPLATEIDENTIFIER)" + Cst.CrLf;
            queryParameters.Query = queryParameters.Query + Cst.CrLf + SQLCst.ORDERBY + "ig.ISDEFAULT desc";
            StringData[] ret = GetResultQuery(pCS, queryParameters);
            return ret;
        }

        /// <summary>
        /// Retourne les écrans et les template par défaut
        /// </summary>
        /// <param name="pIsDefaultOnly">si true retourne uniquement les enregistrement ISDEFAULT = 1, si false retourne en priorité les enregistrements ISDEFAULT = 1 </param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public StringData[] GetDefault(string pCS, bool pIsDefaultOnly)
        {
            return GetDefault(pCS, null, pIsDefaultOnly);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public StringData[] GetDefault(string pCS, IDbTransaction pDbTransaction, bool pIsDefaultOnly)
        {
            QueryParameters queryParameters = GetQuery(pCS);
            if (pIsDefaultOnly)
                queryParameters.Query += SQLCst.WHERE + "(ig.ISDEFAULT = 1)" + Cst.CrLf;
            queryParameters.Query += Cst.CrLf + SQLCst.ORDERBY + "ig.ISDEFAULT desc" + Cst.CrLf;

            StringData[] ret = GetResultQuery(pCS, pDbTransaction, queryParameters);
            return ret;
        }

        #endregion

        #region private Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private DataParameters GetDataParameters(string pCS)
        {
            DataParameters ret = new DataParameters();

            if (_idI > 0)
                ret.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDI), _idI);
            else if (StrFunc.IsFilled(_instrIdentifier))
                ret.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER), _instrIdentifier);
            else
                throw new Exception("Parameter Instrument not defined");

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private QueryParameters GetQuery(string pCS)
        {
            DataParameters dataparameters = GetDataParameters(pCS);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "tr.IDENTIFIER as TEMPLATENAME, ig.SCREENNAME" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENTGUI.ToString() + " ig " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " instr on (instr.IDI=ig.IDI)" + Cst.CrLf;
            if (dataparameters.Contains("IDI"))
                sqlSelect += SQLCst.AND + "(instr.IDI = @IDI)" + Cst.CrLf;
            else if (dataparameters.Contains("IDENTIFIER"))
                sqlSelect += SQLCst.AND + "(instr.IDENTIFIER = @IDENTIFIER)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE.ToString() + " tr on (tr.IDT = ig.IDT_TEMPLATE)" + Cst.CrLf;

            QueryParameters queryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dataparameters);

            return queryParameters;
        }
        /// <summary>
        /// Retourne sous forme de StringData[] les colonnes de la requête
        /// <para>les éléments présents sont TEMPLATENAME et SCREENNAME</para>
        /// </summary>
        /// <param name="pQueryParameters"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private StringData[] GetResultQuery(string pCS, QueryParameters pQueryParameters)
        {
            return GetResultQuery(pCS, null, pQueryParameters);
        }
        // EG 20180205 [23769] Add dbTransaction  
        private StringData[] GetResultQuery(string pCS, IDbTransaction pDbTransaction, QueryParameters pQueryParameters)
        {
            IDataReader dr = null;
            try
            {
                StringData[] ret = null;
                ArrayList al = new ArrayList();

                dr = DataHelper.ExecuteDataTable(pCS, pDbTransaction, pQueryParameters.Query, pQueryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();

                if (dr.Read())
                {
                    al.Add(new StringData("TEMPLATENAME", TypeData.TypeDataEnum.@string, dr.GetValue(0).ToString(), string.Empty));
                    al.Add(new StringData("SCREENNAME", TypeData.TypeDataEnum.@string, dr.GetValue(1).ToString(), string.Empty));
                }

                if (ArrFunc.IsFilled(al))
                    ret = (StringData[])al.ToArray(typeof(StringData));

                return ret;
            }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
        }
        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20170908 [23409] Modify (Rename en ContractTools )
    public sealed class ContractTools
    {
        /// <summary>
        /// Retourne la liste des groupes auxquels appartient un contract
        /// </summary>
        /// <param name="pCs"></param>
        /// 
        /// <param name="pRole">Réduit la liste aux seuls groupes avec rôle {pRole}, valeur null autorisé</param>
        /// <returns></returns>
        // FI 20170908 [23409] Modify (Rename en GetGrpContract)
        // EG 20180307 [23769] Gestion dbTransaction
        public static int[] GetGrpContract(string pCs, Pair<Cst.ContractCategory, int> pContract, Nullable<RoleGContract> pRole, bool pIsUseDataDtEnabled)
        {
            return GetGrpContract(pCs, null, pContract, pRole, pIsUseDataDtEnabled);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetGrpContract(string pCs, IDbTransaction pDbTransaction, Pair<Cst.ContractCategory, int> pContract, Nullable<RoleGContract> pRole, bool pIsUseDataDtEnabled)
        {
            int[] ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDXC", DbType.Int32), pContract.Second);
            dp.Add(new DataParameter(pCs, "CONTRACTCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pContract.First);


            //PL 20111021 Add all GetSQLDataDtEnabled() and use GCONTRACT
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "gc.IDGCONTRACT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.GCONTRACT.ToString() + " gc" + Cst.CrLf;

            if (pRole != null)
            {
                dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEGCONTRACT), pRole.ToString());

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GCONTRACTROLE.ToString() + " gcr on gcr.IDGCONTRACT=gc.IDGCONTRACT" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "gcr.IDROLEGCONTRACT=@IDROLEGCONTRACT" + Cst.CrLf;
                if (pIsUseDataDtEnabled)
                    sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gcr") + Cst.CrLf;
            }

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CONTRACTG.ToString() + " cg on cg.IDGCONTRACT=gc.IDGCONTRACT" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "cg.IDXC=@IDXC and cg.CONTRACTCATEGORY=@CONTRACTCATEGORY" + Cst.CrLf;
            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "cg") + Cst.CrLf;

            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gc");

            QueryParameters qryParameters = new QueryParameters(pCs, sqlSelect.ToString(), dp);

            ArrayList al = new ArrayList();

            using (IDataReader dr = DataHelper.ExecuteDataTable(qryParameters.Cs, pDbTransaction,
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr[0]));
            }
            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));

            return ret;
        }
    }

    
    /// <summary>
    /// Tools Market
    /// </summary>
    public sealed class MarketTools
    {
        /// <summary>
        /// EUREX
        /// </summary>
        /// FI 20170928 [23452] Add
        public const string MIC_EXCHANGE_EUREX = "XEUR";
        /// <summary>
        /// EUROPEAN ENERGY EXCHANGE AG
        /// </summary>
        /// FI 20170928 [23452] Add
        public const string MIC_EXCHANGE_EUROPEAN_ENERGY_EXCHANGE = "XEEE";
        
        /// <summary>
        /// NO MARKET (E.G. UNLISTED)
        /// </summary>
        /// FI 20170928 [23452] Add
        public const string XXXX = "XXXX";

        /// <summary>
        /// OFF-EXCHANGE TRANSACTIONS - LISTED INSTRUMENTS
        /// </summary>
        /// FI 20170928 [23452] Add
        public const string XOFF = "XOFF";


        
        #region GetGrpMarket
        /// <summary>
        /// Retourne la liste des groupes de marché auxquels appartient un Marché
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIM"></param>
        /// <param name="pRole">Réduit la liste aux seuls groupes du rôle {pRole}, valeur null autorisé</param>
        /// <param name="pIsUseDataDtEnabled"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetGrpMarket(string pCs, IDbTransaction pDbTransaction, int pIDM, Nullable<RoleGMarket> pRole, bool pIsUseDataDtEnabled)
        {
            int[] ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDM", DbType.Int32), pIDM);

            //PL 20111021 Add all GetSQLDataDtEnabled() and use GMARKET
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "gm.IDGMARKET" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.GMARKET.ToString() + " gm" + Cst.CrLf;

            if (pRole != null)
            {
                //dp.Add(new DataParameter(pCs, "ROLE", DbType.AnsiString, 64), pRole.ToString()); 
                dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEGMARKET), pRole.ToString());

                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GMARKETROLE.ToString() + " gmr on gmr.IDGMARKET=gm.IDGMARKET" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "gmr.IDROLEGMARKET=@IDROLEGMARKET" + Cst.CrLf;
                if (pIsUseDataDtEnabled)
                    sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gmr") + Cst.CrLf;
            }

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKETG.ToString() + " mg on mg.IDGMARKET=gm.IDGMARKET" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "mg.IDM=@IDM" + Cst.CrLf;
            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "mg") + Cst.CrLf;

            if (pIsUseDataDtEnabled)
                sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "gm");

            QueryParameters qryParameters = new QueryParameters(pCs, sqlSelect.ToString(), dp);

            ArrayList al = new ArrayList();

            using (IDataReader dr = DataHelper.ExecuteDataTable(qryParameters.Cs, pDbTransaction,
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr[0]));
            }

            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));

            return ret;
        }
        #endregion GetGrpMarket
        #region CSSGetMarket
        /// <summary>
        /// Retourne les marchés d'une chambre
        /// <para>Retourne null s'il n'existe aucun marché sur cette chambre</para>
        /// </summary>
        /// <param name="pIdA">Représente la chambre</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] CSSGetMarket(string pCS, IDbTransaction pDbTransaction, int pIdA, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
        {
            int[] ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "IDM" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.MARKET + Cst.CrLf;
            sql += SQLCst.WHERE + "IDA=@IDA";
            //PLl 20111021 Use pScanDataDtEnabledEnum
            if (pScanDataDtEnabledEnum == SQL_Table.ScanDataDtEnabledEnum.Yes)
                sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, Cst.OTCml_TBL.MARKET);

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);

            ArrayList al = new ArrayList();

            using (IDataReader dr = DataHelper.ExecuteDataTable(qryParameters.Cs, pDbTransaction,
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr["IDM"]));
            }

            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));

            return ret;
        }
        #endregion CSSGetMarket
        #region GetMinDtBusiness
        /// <summary>
        /// Retourne la date Min parmi les dates présentes dans ENTITYMARKET pour une entité donnée et/ou une chambre donnée
        /// <para>retourne DateTime.MinValue s'il n'existe aucune ligne dans ENTITYMARKET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity">Représente l'entité</param>
        /// <param name="pIdCSS">Représente la chambre</param>
        /// <param name="pIdM">représente le marché</param>
        /// <returns></returns>
        /// FI 20130502 [] modification de la signature de la fonction (ajour des paramètres pIdCSS et pIdM)
        // EG 20180425 Analyse du code Correction [CA2202]
        public static DateTime GetMinDtBusiness(string pCS, int pIdAEntity, int pIdCSS, int pIdM)
        {
            DateTime ret = DateTime.MinValue;
            //
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), pIdAEntity);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdCSS);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), pIdM);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            // PM 20150512 [20575] Utilisation de DTENTITY à la place de DTMARKET
            //sql += "MIN(DTMARKET) as MINDTMARKET" + Cst.CrLf;
            sql += "MIN(DTENTITY) as MINDTENTITY" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITYMARKET + " em" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " m on m.IDM = em.IDM" + Cst.CrLf;
            sql += SQLCst.WHERE + "((em.IDA=@IDA_ENTITY) or (@IDA_ENTITY=0))" + Cst.CrLf;
            sql += SQLCst.AND + "((em.IDM=@IDM) or (@IDM=0))" + Cst.CrLf;
            sql += SQLCst.AND + "((m.IDA=@IDA) or (@IDA=0))";
            //
            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (!(dr["MINDTENTITY"] is DBNull))
                        ret = Convert.ToDateTime(dr["MINDTENTITY"]);
                }
            }
            return ret;
        }
        #endregion GetMinDtBusiness
        #region GetEntityMarket_MaxDtEntity
        /// <summary>
        /// Retourne l'enregistrement dans ENTITYMARKET (colonne IDEM) dont la date est la plus récente (restriction sur une entité et une liste de marché)
        /// <para>retourne 0 s'il n'existe aucune ligne dans ENTITYMARKET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity">Représente l'entité</param>
        /// <param name="pIdMarket">représente la liste des marchés</param>
        /// <returns></returns>
        /// FI 20131016 [19062] add method
        /// PM 20150529 [20575] Renommage de GetEntityMarket_MaxDtMarket en GetEntityMarket_MaxDtEntity et utilisation de DTENTITY au lieu de DTMARKET
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static Nullable<int> GetEntityMarket_MaxDtEntity(string pCS, IDbTransaction pDbTransaction, int pIdAEntity, int[] pIdMarket)
        {
            // RD 20140926 [20385] Bug sur saisie d'un trade sur nouveau couple entité/marché (table ENTITYMARKET vide)                 
            Nullable<int> ret = null;
            //
            string sqlWhereMarket = string.Empty;
            if (ArrFunc.Count(pIdMarket) > 0)
                sqlWhereMarket = DataHelper.SQLColumnIn(pCS, "em.IDM", pIdMarket, TypeData.TypeDataEnum.@int);

            DataParameters parameters = new DataParameters();
            if (pIdAEntity > 0)
                parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdAEntity);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "em.IDEM" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITYMARKET + " em" + Cst.CrLf;

            SQLWhere sqlwhere = new SQLWhere();
            if (parameters.Contains("IDA"))
                sqlwhere.Append("em.IDA=@IDA");
            if (StrFunc.IsFilled(sqlWhereMarket))
                sqlwhere.Append(sqlWhereMarket, true);
            if (StrFunc.IsFilled(sqlwhere.ToString()))
                sql += sqlwhere.ToString();
            //
            //PM 20150512 [20575] Utilisation de DTENTITY à la place de DTMARKET
            //sql += SQLCst.ORDERBY + "em.DTMARKET desc" + Cst.CrLf;
            sql += SQLCst.ORDERBY + "em.DTENTITY desc" + Cst.CrLf;
            //
            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, 
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    ret = Convert.ToInt32(dr["IDEM"]);
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'enregistrement dans ENTITYMARKET (colonne IDEM) dont la date est la plus récente
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pIdA_Custodian"></param>
        /// <returns></returns>
        /// PM 20150529 [20575] Renommage de GetEntityMarket_MaxDtMarket en GetEntityMarket_MaxDtEntity et utilisation de DTENTITY au lieu de DTMARKET
        // EG 20180425 Analyse du code Correction [CA2202]
        public static Nullable<int> GetEntityMarket_MaxDtEntity(string pCS, int pIdAEntity, int pIdA_Custodian)
        {
            // RD 20140926 [20385] Bug sur saisie d'un trade sur nouveau couple entité/marché (table ENTITYMARKET vide)                 
            Nullable<int> ret = null;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdAEntity);
            parameters.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), pIdA_Custodian);

            // PM 20150512 [20575] Utilisation de DTENTITY à la place de DTMARKET
//            string sqlQuery = @"select em.IDEM
//            from dbo.ENTITYMARKET em
//            where (em.IDA = @IDA) and (em.IDA_CUSTODIAN = @IDA_CUSTODIAN)
//            order by em.DTMARKET desc" + Cst.CrLf;
            string sqlQuery = @"select em.IDEM
            from dbo.ENTITYMARKET em
            where (em.IDA = @IDA) and (em.IDA_CUSTODIAN = @IDA_CUSTODIAN)
            order by em.DTENTITY desc" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text,
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    ret = Convert.ToInt32(dr["IDEM"]);
            }
            return ret;
        }
        #endregion GetEntityMarket_MaxDtEntity
        #region IsEntityMarketFilled
        /// <summary>
        /// Retourne 1 si ENTITYMARKET est renseigné
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20130430 
        public static bool IsEntityMarketFilled(string pCS)
        {
            string query = @"Select 1 from dbo.ENTITYMARKET";

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, query);
            bool ret = (null != obj);

            return ret;
        }
        #endregion IsEntityMarketFilled
        #region IsEntityMarketForCSSFilled
        /// <summary>
        /// Retourne 1 si ENTITYMARKET est renseigné pour une chambre donnée
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20130705 New
        public static bool IsEntityMarketForCSSFilled(string pCS, int pIdCSS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDCSS", DbType.Int32), pIdCSS);

            string query = @"select 1 from dbo.ENTITYMARKET em inner join dbo.MARKET mk on (mk.IDM = em.IDM) and (mk.IDA = @IDCSS)";
            QueryParameters qryParameters = new QueryParameters(pCS, query, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            bool ret = (null != obj);
            return ret;
        }
        #endregion IsEntityMarketForCSSFilled
        #region IsCurrentDateBusiness
        /// <summary>
        /// Retourne true si la date {pDate} est une date courante d'une entité sur l'un des marchés d'une chambre
        /// <para>
        /// Il existe au moins un enregistrement dans ENTITYMARKET tel que :
        /// <para>
        /// - DTENTITY = {pDate} et 
        /// </para>
        /// <para>
        /// - IDM in (liste des marchés de la chambre) 
        /// </para>
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdACSS">Représente la chambre</param>
        /// <returns></returns>
        /// FI 20130429 [] add Method
        /// PM 20150513 [20575] Gestion DTENTITY à la place de DTMARKET
        // EG 20180307 [23769] Gestion dbTransaction
        public static bool IsCurrentDateBusiness(string pCS, DateTime pDate, int pIdACSS)
        {
            if (DateTime.MinValue == pDate)
                throw new ArgumentException("Min Value is not valid for Argument pDate");

            int[] market = CSSGetMarket(pCS, null, pIdACSS, SQL_Table.ScanDataDtEnabledEnum.Yes);

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);

            string query = @"Select 1 from dbo.ENTITYMARKET where DTENTITY = @DT" + SQLCst.AND;
            query += DataHelper.SQLColumnIn(pCS, "IDM", market, TypeData.TypeDataEnum.@int);

            QueryParameters queryParameters = new QueryParameters(pCS, query, parameters);

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            bool ret = (obj != null);

            return ret;
        }
        #endregion IsCurrentDateBusiness
        #region IsCurrentDateMarket
        /// <summary>
        /// Retourne true si la date {pDate} est une date courante sur l'un des marchés d'une chambre
        /// <para>
        /// Il existe au moins un enregistrement dans ENTITYMARKET tel que :
        /// <para>
        /// - DTMARKET= {pDate} et 
        /// </para>
        /// <para>
        /// - IDM in (liste des marchés de la chambre) 
        /// </para>
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdACSS">Représente la chambre</param>
        /// <returns></returns>
        /// FI 20130429 [] add Method
        /// PM 20150513 [20575] Renommage de IsCurrentDateBusiness en IsCurrentDateMarket
        // EG 20180307 [23769] Gestion dbTransaction
        public static bool IsCurrentDateMarket(string pCS, DateTime pDate, int pIdACSS)
        {
            if (DateTime.MinValue == pDate)
                throw new ArgumentException("Min Value is not valid for Argument pDate");

            int[] market = CSSGetMarket(pCS, null, pIdACSS, SQL_Table.ScanDataDtEnabledEnum.Yes);

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);

            string query = @"Select 1 from dbo.ENTITYMARKET where DTMARKET = @DT" + SQLCst.AND;
            query += DataHelper.SQLColumnIn(pCS, "IDM", market, TypeData.TypeDataEnum.@int);

            QueryParameters queryParameters = new QueryParameters(pCS, query, parameters);

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            bool ret = (obj != null);

            return ret;
        }
        #endregion IsCurrentDateMarket
        #region LoadEntityMarketActivity
        /// <summary>
        /// Chargement de VW_ENTITYMARKET_ACTIVITY pour une entity donnée et/ou une date donnée
        /// <para>Les colonnes disponible dans le DataRow sont :</para>
        /// <para>IDEM, DTMARKETPREV, DTMARKET, DTMARKETNEXT,IDM, IDENTIFIER, IDA_CSSCUSTODIAN, IDENTIFIER_CSSCUSTODIAN,</para>
        /// <para>DTENTITYPREV, DTENTITY, DTENTITYNEXT</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIda_Entity">Représente l'entité</param>
        /// <param name="pdtBusiness">Représente la date busness</param>
        /// <returns></returns>
        /// FI 20130603 [SCHEDULING] Add method (méthode écrite dans le traitement CashBalance à l'origine)
        /// FI 20131203 [19290] Correction
        /// FI 20141126 [20526] Modify and Rename  
        /// PM 20150507 [20575] Add em.DTENTITYPREV, em.DTENTITY, em.DTENTITYNEXT, IDBCENTITY
        public static DataRow[] LoadEntityMarketActivity(string pCS, Nullable<int> pIda_Entity, Nullable<DateTime> pdtBusiness)
        {
            DataParameters dataParameters = new DataParameters();
            if (pIda_Entity.HasValue)
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), pIda_Entity);
            if (pdtBusiness.HasValue)
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pdtBusiness);

            //FI 20130429 le select retourne l'identifier du marché et l'identifier de la chambre
            StrBuilder sqlQuery = new StrBuilder();
            // PM 20150507 [20575] Add em.DTENTITYPREV, em.DTENTITY, em.DTENTITYNEXT, IDBCENTITY
            sqlQuery += @"select em.IDEM, em.DTMARKETPREV, em.DTMARKET, em.DTMARKETNEXT, m.IDM as IDM, m.IDENTIFIER,
            a.IDA as IDA_CSSCUSTODIAN, a.IDENTIFIER as IDENTIFIER_CSSCUSTODIAN, em.DTENTITYPREV, em.DTENTITY, em.DTENTITYNEXT, " + DataHelper.SQLIsNull(pCS, "e.IDBCACCOUNT", "m.IDBC", "IDBCENTITY") + Cst.CrLf;
            sqlQuery += "from dbo.VW_ENTITYMARKET_ACTIVITY em" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "em.IDA_CSSCUSTODIAN", "a", DataEnum.EnabledOnly);
            //PM 20150511 [20575] Ajout jointure sur ENTITY
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ENTITY, SQLJoinTypeEnum.Inner, "em.IDA", "e", DataEnum.All);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.MARKET, SQLJoinTypeEnum.Inner, "em.IDM", "m", DataEnum.EnabledOnly);

            //FI 20141128 [XXXXX] suppression de la clause m.ISTRADEDDERIVATIVE = 1
            //sqlQuery += SQLCst.AND + "(m.ISTRADEDDERIVATIVE = 1)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(m.ISENABLED = 1)" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere(SQLWhere.StartEnum.StartWithWhere);
            if (pIda_Entity.HasValue)
                sqlWhere.Append("(em.IDA= @IDA_ENTITY)");

            //FI 20131203 [19162] ce n'est pas un else if qu'il faut mais un if 
            //else if (pdtBusiness.HasValue)
            if (pdtBusiness.HasValue)
            {
                //PM 20150507 [20575] Gestion DtEntity
                //sqlWhere.Append("(em.DTMARKET= @DT)");
                sqlWhere.Append("(em.DTENTITY= @DT)");
            }
            sqlQuery += sqlWhere.ToString();

            DataSet dsEntityMarket = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlQuery.ToString(),
                                                        dataParameters.GetArrayDbParameter());

            DataTable dtEntityMarket = dsEntityMarket.Tables[0];
            DataRow[] drEntityMarket = dtEntityMarket.Select();

            return drEntityMarket;
        }
        #endregion LoadEntityMarketActivity
        
        #region BuildSQLColMarket_FIXML_SecurityExchange
        /// <summary>
        /// Retourne une expressions de colonne SQL dont l'alias est Market_FIXML_SecurityExchange 
        /// </summary>
        /// <param name="pAlias"></param>
        /// <returns></returns>
        // FI 20131223 [19337] add Method 
        // PL 20171006 [23469] MARKETTYPE deprecated
        public static string BuildSQLColMarket_FIXML_SecurityExchange(string pAlias)
        {
            string ret = String.Format(SQLCst.CASE_WHEN_THEN_ELSE_END,
                     //pAlias + ".MARKETTYPE=" + DataHelper.SQLString(Cst.MarketTypeEnum.SEGMENT.ToString()),

                     //NB: Test si SEGMENT (IDMOPERATING is not null) et si MIC du parent (en principe l'Operating) est identique 
                     //    ex. IDEX, AGREX sur CCeG et XHEX sur Eurex
                     "(" + pAlias + ".IDMOPERATING is not null) and exists (select 1 from dbo.MARKET operating where operating.IDM=" + pAlias + ".IDMOPERATING and operating.ISO10383_ALPHA4=" + pAlias + ".ISO10383_ALPHA4)",
                     pAlias + ".ISO10383_ALPHA4 || '-' || " + pAlias + ".EXCHANGESYMBOL",
                     pAlias + ".ISO10383_ALPHA4");

            ret += " as Market_FIXML_SecurityExchange";
            return ret;
        }
        #endregion BuildSQLColMarket_FIXML_SecurityExchange
        #region GetCSSMemberCode
        /// <summary>
        ///  Retourne le code d'un membre d'une chambre de compensation
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pActor">Représente le membre (scheme,value)</param>
        /// <param name="pActorCSS">Représente la chambre (scheme,value)</param>
        /// <param name="pCssMemberIdent">Représente la nature de la codification</param>
        /// <returns></returns>
        /// FI 20140206 [19564] add Method 
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static string GetCSSMemberCode(string pCS, IDbTransaction pDbTransaction, Pair<string, string> pActor, Pair<string, string> pActorCSS, string pCssMemberIdent)
        {
            string ret = string.Empty;

            int idA = ActorTools.GetIdAFromScheme(pCS, pDbTransaction,  pActor.Second, pActor.First);
            int idACSS = ActorTools.GetIdAFromScheme(pCS, pDbTransaction,  pActorCSS.Second, pActorCSS.First);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "csm.CSSMEMBERCODE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CSMID.ToString() + " csm" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_css on a_css.IDA=csm.IDA_CSS";
            sqlSelect += SQLCst.AND + "a_css.IDA=@IDA_CSS" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "csm.IDA=@IDA";
            sqlSelect += SQLCst.AND + "csm.CSSMEMBERIDENT=@CSSMEMBERIDENT";
            sqlSelect += SQLCst.AND + "csm.CSSMEMBERCODE" + SQLCst.IS_NOT_NULL;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_CSS), idACSS);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), idA);
            parameters.Add(new DataParameter(pCS, "CSSMEMBERIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pCssMemberIdent);

            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteDataTable(qry.Cs, pDbTransaction,  
                qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                if (dr.Read())
                    ret = Convert.ToString(dr["CSSMEMBERCODE"]);
            }
            return ret;
        }
        #endregion GetCSSMemberCode
        #region GetClearingCompartCode
        /// <summary>
        ///  Retourne le code d'un compartiment d'une chambre de compensation
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pActor">Représente le compartiment (scheme,value)</param>
        /// <param name="pActorCSS">Représente la chambre (scheme,value)</param>
        /// <returns></returns>
        /// FI 20140916 [20353] Add Method
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        public static string GetClearingCompartCode(string pCS, IDbTransaction pDbTransaction, Pair<string, string> pActor, Pair<string, string> pActorCSS)
        {
            string ret = string.Empty;

            int idA = ActorTools.GetIdAFromScheme(pCS, pDbTransaction,  pActor.Second, pActor.First);
            int idACSS = ActorTools.GetIdAFromScheme(pCS, pDbTransaction,  pActorCSS.Second, pActorCSS.First);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "cc.COMPARTMENTCODE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CLEARINGCOMPART.ToString() + " cc" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "cc.IDA=@IDA and cc.IDA_CSS=@IDA_CSS";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_CSS), idACSS);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), idA);

            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteDataTable(qry.Cs, pDbTransaction,  
                qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                if (dr.Read())
                    ret = Convert.ToString(dr["COMPARTMENTCODE"]);
            }
            return ret;
        }
        #endregion GetClearingCompartCode

        #region GetMinDateMarket
        /// <summary>
        /// Lecture de la plus petite DtBusiness en vigueur sur ENTITYMARKET
        /// </summary>   
        public static DateTime GetMinDateMarket(string pCS)
        {
            DateTime dtMarket = DateTime.MinValue;
            string sql = SQLCst.SELECT + "min(DTMARKET) as DTMARKET" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITYMARKET.ToString() + " em" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a on a.IDA=em.IDA" + Cst.CrLf;
            sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a");
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sql))
            {
                if (dr.Read())
                {
                    if (dr["DTMARKET"] != Convert.DBNull)
                    {
                        dtMarket = Convert.ToDateTime(dr["DTMARKET"]);
                    }
                }
            }

            if (dtMarket == DateTime.MinValue)
                return DateTime.Today;

            return dtMarket;
        }

        #endregion GetMinDateMarket

    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Add 
    public sealed class CountryTools
    {

        /// <summary>
        /// LOCAL (Internal use) used by XOFF,XXXX,BILT Markets
        /// </summary>
        /// FI 20180129 [XXXXX] Add
        public const string COUNTRY_ZZ = "ZZ";
        
        
        /// <summary>
        /// Union de pays 
        /// </summary>
        public enum CountryUnion
        {
            /// <summary>
            /// EEA (European Economic Area)
            /// </summary>
            EEA,
            /// <summary>
            /// EFTA (European Free Trade Association)
            /// </summary>
            EFTA,
            /// <summary>
            /// EMU (European monetary union)
            /// </summary>
            XE,
            /// <summary>
            /// OECD (Organisation for Economic Co-operation and Development)
            /// </summary>
            XO,
            /// <summary>
            /// EU (European Union)
            /// </summary>
            XU,
        }

        /// <summary>
        ///  Retourne les pays appartenant à l'union {pUnion} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUnion"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public static IEnumerable<String> GetCountryOfUnion(String pCS, CountryUnion pUnion)
        {
            return GetCountryOf(pCS, pUnion.ToString());
        }

        /// <summary>
        ///  Retourne les pays appartement à l'union {pUnion} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdCountry">Représente une union (Exemple: EEA (European Economic Area))</param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public static IEnumerable<String> GetCountryOf(string pCS, string pIdCountry)
        {
            List<String> ret = new List<string>();

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDCOUNTRY), pIdCountry);

            string query = @"select IDCOUNTRY from dbo.COUNTRYOF 
                                    where IDCOUNTRY_COUNTRY = @IDCOUNTRY";

            QueryParameters qry = new QueryParameters(pCS, query, dp);
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                    ret.Add(dr["IDCOUNTRY"].ToString());
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
            return ret;
        }
    }


}
