#region Using Directives
using System;
using System.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
#endregion Using Directives

namespace EFS.Common
{
	/// <summary>
	/// Classe d'appel aux Stored Procedure des SGBD/R 
	/// </summary>
    // EG 20190308 Add ACTIONREQUEST
    public sealed class SQLUP
	{
        #region Enum
        public enum IdGetId
        {
			EAR,
			EARCALC,
			EARDAY,
			EARCOMMON,
			EARNOM,
			ACCDAYBOOK,
            CORPOACTION,
            CORPOACTIONISSUE,
            CORPOEVENT,
            CORPOEVENTCONTRACT,
            CORPOEVENTASSET,
            ESR,
			EVENT, 
			//EVENTCLASS,
            MCO,
            MSO,
			MATRIXVAL_H,
            PARAMSEUREX_MATURITY,
            POSACTION,
            //POSACTIONDET,   // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
            POSREQUEST,
			PROCESS_L,
            REQUEST_L,
            SERVICE_L,
            SESSIONRESTRICT,
			TRADE, 
			TRACKER_L,
			YIELDCURVEVAL_H,
            ACTIONREQUEST,
        }
		public enum PosRetGetId
		{
			First, 
            Last
		}
        #endregion Enum

        #region Constructor
        public SQLUP(){}
        #endregion Constructor

        #region GetId()
        // Utilisation de UP_GETID pour obtenir une jeton (eg: IDT d eTRADE)
        private static Cst.ErrLevel GetId(out int opToken, string pSource, IDbTransaction pDbTransaction, IdGetId pIdGetId, PosRetGetId pPosRetGetId, int pNumberOfToken)
        {
            return GetId(out opToken, pSource, pDbTransaction, pIdGetId.ToString(), pPosRetGetId, pNumberOfToken);
        }
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        // EG 20200511 [XXXXX] Upd Utilisation d'une séquence SQLServer en lieu et place de GETID pour la table EVENT
        // EG 20200527 [XXXXX] Utilisation Séquence sur ID Table EVENT pour MSSQLServer 2016 et plus        
        // PL 20200706 [XXXXX] Utilisation Séquence sur ID Table EVENT pour MSSQLServer 2016 et plus, directement à partir de la SP UP_GETID        
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        private static Cst.ErrLevel GetId(out int opToken, string pSource, IDbTransaction pDbTransaction, string pIdGetId, PosRetGetId pPosRetGetId, int pNumberOfToken)
        {
            opToken = 0;
            string _cs = pSource;
            if (StrFunc.IsEmpty(_cs))
                _cs = pDbTransaction.Connection.ConnectionString;

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(_cs, CommandType.StoredProcedure, "pIdGetId", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdGetId.ToString());
                parameters.Add(new DataParameter(_cs, CommandType.StoredProcedure, "pNumberOfToken", DbType.Int32), pNumberOfToken);
                DataParameter paramReturnValue = new DataParameter(_cs, CommandType.StoredProcedure, "ReturnValue", DbType.Int32) { Direction = ParameterDirection.ReturnValue };
                parameters.Add(paramReturnValue, 0);

                int retUP = DataHelper.ExecuteNonQuery(pSource, pDbTransaction, CommandType.StoredProcedure, "dbo." + Cst.OTCml_StoredProcedure.UP_GETID.ToString(), parameters.GetArrayDbParameter());


            Cst.ErrLevel ret;
            //Warning: SqlServer retourne -1 mais Oracle retourne 1 (200500907 PL)
            if (0 != retUP)
            {
                ret = Cst.ErrLevel.SUCCESS;
                opToken = Convert.ToInt32(paramReturnValue.Value);
                if (PosRetGetId.First == pPosRetGetId)
                {
                    //PL 20200706 Bogue potentiel ! Il manque la prise en considération de la valeur de STEP losrque celle-ci diffère de 1
                    opToken += 1 - pNumberOfToken;
                }
            }
            else
            {
                throw new Exception("UP_GETID return 0");
            }
            return ret;
        }
        public static Cst.ErrLevel  GetId(out int opToken, string pSource, 
			IdGetId pIdGetId, PosRetGetId pPosRetGetId, int pNumberOfToken)
        {
            return GetId(out opToken, pSource, null, pIdGetId, pPosRetGetId, pNumberOfToken);
        }
        public static Cst.ErrLevel  GetId(out int opToken, string pSource, IdGetId pIdGetId)
        {
			return GetId(out opToken, pSource, pIdGetId, PosRetGetId.Last, 1);
        }
        public static Cst.ErrLevel  GetId(out int opToken, IDbTransaction pDbTransaction, 
			IdGetId pIdGetId, PosRetGetId pPosRetGetId, int pNumberOfToken)
        {
            return GetId(out opToken, null, pDbTransaction, pIdGetId.ToString(), pPosRetGetId, pNumberOfToken);
        }
        public static Cst.ErrLevel GetId(out int opToken, IDbTransaction pDbTransaction,
            string pIdGetId, PosRetGetId pPosRetGetId, int pNumberOfToken)
        {
            return GetId(out opToken, null, pDbTransaction, pIdGetId, pPosRetGetId, pNumberOfToken);
        }
        public static Cst.ErrLevel GetId(out int opToken, IDbTransaction pDbTransaction, IdGetId pIdGetId)
        {
            return GetId(out opToken, pDbTransaction, pIdGetId.ToString());
        }
        public static Cst.ErrLevel  GetId(out int opToken, IDbTransaction pDbTransaction, string pIdGetId)
        {
            return GetId(out opToken, pDbTransaction, pIdGetId, PosRetGetId.Last, 1);
        }
        public static Cst.ErrLevel GetId(out int opToken, string pSource, IdGetId pIdGetId,PosRetGetId pPosRetGetId)
        {
            return GetId(out opToken, pSource, pIdGetId,pPosRetGetId,1);
        }
        #endregion GetId()
        
        #region GetActor()
        // Utilisation de UP_GETACTOR pour obtenir un acteur parent d'un acteur en fonction des rôles
        public static int GetActor(string pSource, string pIdaActor, string pRole, int pAskParentLevel, ref int opIDA, ref bool opHasRole, ref bool opHasRoleSelf, ref int opParentLevel)
        {
            return GetActor(pSource, pIdaActor, pRole, pAskParentLevel, DateTime.MinValue, ref opIDA, ref opHasRole, ref opHasRoleSelf, ref opParentLevel);
        }
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        public static int GetActor(string pSource, string pIdaActor, string pRole, int pAskParentLevel, DateTime pComparisonDate, ref int opIDA, ref bool opHasRole, ref bool opHasRoleSelf, ref int opParentLevel)
        {
            opIDA = 0;

            int ret;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pActorIDA.ToString(), DbType.Int32), pIdaActor);
                if (pComparisonDate != DateTime.MinValue)
                    parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pComparisonDate.ToString(), DbType.DateTime), pComparisonDate);
                else
                    parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pComparisonDate.ToString(), DbType.DateTime), DBNull.Value);
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pAskParentLevel.ToString(), DbType.Int32), pAskParentLevel);
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pIdRoleActor.ToString(), DbType.AnsiString, 16), pRole);

                DataParameter paramOutIDA = new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.opActorIDA.ToString(), DbType.Int32) { Direction = ParameterDirection.Output };
                parameters.Add(paramOutIDA);
                DataParameter paramOutHasRole = new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.opHasRole.ToString(), DbType.Boolean) { Direction = ParameterDirection.Output };
                parameters.Add(paramOutHasRole);
                DataParameter paramOutHasRoleSelf = new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.opHasRoleSelf.ToString(), DbType.Boolean) { Direction = ParameterDirection.Output };
                parameters.Add(paramOutHasRoleSelf);
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.pGETVERSION_1_0_0.ToString(), DbType.AnsiString, 10));
                DataParameter paramOutParentLevel = new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.opParentLevel.ToString(), DbType.Int32) { Direction = ParameterDirection.Output };
                parameters.Add(paramOutParentLevel, 0);
                DataParameter paramReturnValue = new DataParameter(pSource, CommandType.StoredProcedure, Cst.OTCml_StoredProcedure_Params.ReturnValue.ToString(), DbType.Int32) { Direction = ParameterDirection.ReturnValue };
                parameters.Add(paramReturnValue, 0);

                //20091026 PL Ajout du owner dbo
                if (0 != DataHelper.ExecuteNonQuery(pSource, CommandType.StoredProcedure, "dbo." + Cst.OTCml_StoredProcedure.UP_GETACTOR.ToString(), parameters.GetArrayDbParameter()))
                {
                    ret = (int)paramReturnValue.Value;
                    if (!(paramOutIDA.Value is DBNull))
                    {
                        opIDA = Convert.ToInt32(paramOutIDA.Value);
                        opHasRole = Convert.ToBoolean(paramOutHasRole.Value);
                        opHasRoleSelf = Convert.ToBoolean(paramOutHasRoleSelf.Value);
                        opParentLevel = Convert.ToInt32(paramOutParentLevel.Value);
                    }

                    ret = (int)Cst.ErrLevel.SUCCESS;
                }
                else
                {
                    throw new Exception("UP_GETACTOR(" + pIdaActor + ", " + pRole + ", " + pAskParentLevel + ")");
                }
            }
            catch
            {
                ret = (int)Cst.ErrLevel.FAILURE;
                //throw new SystemException("UP_GetActor(" + pIdaActor + ", "  + ", " + pRole + ")");
            }
            return ret;
        }
        #endregion GetActor()

        #region RunUP()
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        public static Cst.ErrLevel RunUP(out int opReturnValue, string pSource, string pUP, string pIdLstConsult, string pIdLstTemplate, int pIdA)
		{
            opReturnValue = 0;

            Cst.ErrLevel ret;
            try
            {
                DataParameter paramReturnValue = new DataParameter(pSource, CommandType.StoredProcedure, "ReturnValue", DbType.Int32) { Direction = ParameterDirection.ReturnValue };
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, "pIdA", DbType.Int32), pIdA);
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, "pIdLstTemplate", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdLstTemplate);
                parameters.Add(new DataParameter(pSource, CommandType.StoredProcedure, "pIdLstConsult", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdLstConsult);
                parameters.Add(paramReturnValue as DataParameter, 0);

                int retUP = DataHelper.ExecuteNonQuery(pSource, CommandType.StoredProcedure, pUP, parameters.GetArrayDbParameter());

                //Warning: SqlServer retourne -1 mais Oracle retourne 1 
                if (0 != retUP)
                {
                    ret = Cst.ErrLevel.SUCCESS;
                    opReturnValue = Convert.ToInt32(paramReturnValue.Value);
                }
                else
                {
                    ret = Cst.ErrLevel.FAILURE;
                    throw new SystemException(pUP + "()");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: [" + pUP + "()] [" + e.Message + "]");
                ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
		}
        #endregion RunUP()

        #region GetIdBySequence
        // EG 20200511 [XXXX] New Utilisation d'une séquence SQLServer en lieu et place de GETID pour la table EVENT
        public static Cst.ErrLevel GetIdBySequence(out int opToken, string pCS, IDbTransaction pDbTransaction, string pSequenceName)
        {
            return GetIdBySequence(out opToken, pCS, pDbTransaction, pSequenceName, PosRetGetId.First, 1);
        }
        public static Cst.ErrLevel GetIdBySequence(out int opToken, string pCS, IDbTransaction pDbTransaction, string pSequenceName, PosRetGetId pPosRetGetId, int pRangeSize)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "sequence_name", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pSequenceName);
            parameters.Add(new DataParameter(pCS, "range_size", DbType.Int64), pRangeSize);
            parameters.Add(new DataParameter(pCS, "range_first_value", DbType.Object));
            parameters.Add(new DataParameter(pCS, "range_last_value", DbType.Object));

            parameters["range_first_value"].Direction = ParameterDirection.Output;
            parameters["range_last_value"].Direction = ParameterDirection.Output;

            int retUP = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.StoredProcedure, "sys.sp_sequence_get_range", parameters.GetArrayDbParameter());
            Cst.ErrLevel ret;
            if (0 != retUP)
            {
                // SUCCESS
                ret = Cst.ErrLevel.SUCCESS;
                opToken = Convert.ToInt32(parameters[(PosRetGetId.First == pPosRetGetId) ? "range_first_value" : "range_last_value"].Value);
            }
            else
            {
                throw new Exception("GetIdBySequence Error REturn 0");
            }
            return ret;
        }

        // EG 20200527 [XXXXX] Test GetRange for Oracle 
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        public static Cst.ErrLevel GetORAIdBySequence(out int opToken, string pCS, IDbTransaction pDbTransaction, PosRetGetId pPosRetGetId, int pRangeSize)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, CommandType.StoredProcedure, "pRangeSize", DbType.Int32), pRangeSize);
            DataParameter paramReturnValue = new DataParameter(pCS, CommandType.StoredProcedure, "ReturnValue", DbType.Int32) { Direction = ParameterDirection.ReturnValue };
            parameters.Add(paramReturnValue, 0);
            int retUP = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.StoredProcedure, SQLCst.DBO.Trim() + "UP_GETNEXTRANGE", parameters.GetArrayDbParameter());

            Cst.ErrLevel ret;
            if (0 != retUP)
            {
                // SUCCESS
                ret = Cst.ErrLevel.SUCCESS;
                opToken = Convert.ToInt32(paramReturnValue.Value);
                if (PosRetGetId.First == pPosRetGetId)
                    opToken += 1 - pRangeSize;
            }
            else
            {
                throw new Exception("GetIdBySequence Error Return 0");
            }
            return ret;
        }
        #endregion GetIdBySequence
    }
}


namespace EFS.ApplicationBlocks.Data
{

#if DEBUG
    /// <summary>
    /// 
    /// </summary>
    /// FI 20131213 [19337] Add class DeadLockGen. Cette classe existe uniquement en mode DEBUG.
    public class DeadLockGen
    {
        readonly string _cs;
        public DeadLockGen(string pCS)
        {
            _cs = pCS;
        }

        /// <summary>
        /// Génère un DeadLock
        /// </summary>
        public void Generate()
        {
            //
            System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(Method2);
            System.Threading.Thread thread = new System.Threading.Thread(threadStart);
            thread.Start();
            //
            Method1();
        }

        private void Method1()
        {

            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(_cs);
                System.Threading.Thread.Sleep(1000);
                Cst.ErrLevel errLevelb = EFS.Common.SQLUP.GetId(out int id, dbTransaction, "TESTDEADLOCK_B");
                System.Threading.Thread.Sleep(2000);
                Cst.ErrLevel errLevela = EFS.Common.SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_A");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                DataHelper.RollbackTran(dbTransaction);
            }
        }

        private void Method2()
        {
            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(_cs);

                Cst.ErrLevel errLevela = EFS.Common.SQLUP.GetId(out int id, dbTransaction, "TESTDEADLOCK_A");
                System.Threading.Thread.Sleep(2000);
                Cst.ErrLevel errLevelb = EFS.Common.SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_B");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                DataHelper.RollbackTran(dbTransaction);
            }
        }

        /// <summary>
        /// Generère par reflexion une System.Data.SqlClient.SqlException qui représente un deadlock 
        /// <para>Permet de simuler l'exception obtenue sans avoir à générer un vrai deadlock</para>
        /// </summary>
        /// FI 20210730 [XXXXX] Add
        public static void GenerateException()
        {
            // Message obtenu via select * from sys.messages where message_id = 1205 and language_id = 1033
            throw SqlExceptionCreator.Create("Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", 1205);
        }
    }
#endif
}