using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Data.Common;
//
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;


namespace EFS.Common
{
 
    /// <summary>
    /// Tools pour gestion des locks dans la table EFSLOCK
    /// </summary>
    public sealed class LockTools
    {
        public const string Exclusive = "X";
        public const string Shared = "S";
        
        #region constructor
        private LockTools() { }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLockObject"></param>
        /// <param name="pSession"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static string SearchProcessLocks(string pCs, LockObject pLockObject, AppSession pSession)
        {
            return SearchProcessLocks(pCs, null, pLockObject, pSession);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public static string SearchProcessLocks(string pCs, IDbTransaction pDbTransaction, LockObject pLockObject, AppSession pSession)
        {
            string msgProcessLock = null;
            TypeLockEnum typeLocks = new TypeLockEnum();
            FieldInfo[] typeLocksFlds = typeLocks.GetType().GetFields();
            foreach (FieldInfo typeLocksFld in typeLocksFlds)
            {
                if (System.Enum.IsDefined(typeof(TypeLockEnum), typeLocksFld.Name))
                {
                    TypeLockEnum typeLockEnum = (TypeLockEnum)typeLocksFld.GetValue(typeLocks);
                    if ((pLockObject.ObjectType & typeLockEnum) == typeLockEnum)
                    {
                        Lock lockExisting = LockTools.SearchLock(pCs, pDbTransaction,
                            new LockObject(typeLockEnum, pLockObject.ObjectId, pLockObject.ObjectIdentifier, pLockObject.LockMode));
                        //
                        if (null != lockExisting)
                        {
                            if (lockExisting.Session.SessionId != pSession.SessionId)
                                msgProcessLock += lockExisting.Message() + Cst.CrLf;
                        }
                    }
                }
            }
            return msgProcessLock;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTypeLock"></param>
        /// <param name="pSession"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static string SearchProcessLocks(string pCs, TypeLockEnum pTypeLock, string pLockMode, AppSession pSession)
        {
            return SearchProcessLocks(pCs, null, pTypeLock, pLockMode, pSession);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public static string SearchProcessLocks(string pCs, IDbTransaction pDbTransaction, TypeLockEnum pTypeLock, string pLockMode, AppSession pAppInstance)
        {
            return SearchProcessLocks(pCs, pDbTransaction, new LockObject(pTypeLock, string.Empty, pLockMode), pAppInstance);
        }

        /// <summary>
        /// Retourne le lock posé dans EFSLOCK qui s'applique à l'objet {pLockObject}
        /// <para>Retourne null s'il n'y a pas de lock</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLockObject"></param>
        /// <returns></returns>
        /// FI 20130527 [18662] pLockObject.ObjectId est de type string
        // EG 20180307 [23769] Gestion dbTransaction
        public static Lock SearchLock(string pCs, LockObject pLockObject)
        {
            return SearchLock(pCs, null, pLockObject);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        public static Lock SearchLock(string pCs, IDbTransaction pDbTransaction, LockObject pLockObject)
        {
            Lock lck = null;

            if (null != pLockObject)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCs, "OBJECTNAME", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pLockObject.ObjectType.ToString());

                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "LOCKMODE, OBJECTID, ACTION, SESSIONID, HOSTNAME, APPNAME, APPVERSION," + Cst.CrLf;
                sqlSelect += "DTINS,IDAINS,DTUPD,IDAUPD," + Cst.CrLf;
                sqlSelect += "NOTE,EXTLLINK,ROWATTRIBUT" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + " lck" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(lck.OBJECTNAME = @OBJECTNAME)";
                if (StrFunc.IsFilled(pLockObject.ObjectId))
                {
                    sqlSelect += SQLCst.AND + "(lck.OBJECTID = @OBJECTID)" + Cst.CrLf;
                    parameters.Add(new DataParameter(pCs, "OBJECTID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLockObject.ObjectId);
                }

                // EG 20130523 LockMode
                //
                // Si Lock = Shared : 
                // - Plusieurs EFSLOCK Shared peuvent être posés simultanément.
                // - Une EFSLOCK Shared ne peut pas être posé s’il existe un EFSLOCK eXclusive
                // Si Lock = eXclusive : 
                // - Un seul EFSLOCK eXclusive peut être posé à un instant T
                // - Un EFSLOCK eXclusive ne peut pas être posé s’il existe au moins un EFSLOCK Shared
                if (pLockObject.LockMode.StartsWith(LockTools.Shared))
                {
                    sqlSelect += SQLCst.AND + "(lck.LOCKMODE = @LOCKMODE)" + Cst.CrLf;
                    parameters.Add(new DataParameter(pCs, "LOCKMODE", DbType.AnsiString, 4), LockTools.Exclusive);
                }

                QueryParameters queryParameters = new QueryParameters(pCs, sqlSelect.ToString(), parameters);


                string action = string.Empty;
                string extlLink = string.Empty;
                DateTime dt = DateTime.MinValue;
                LockObject _lockObject = null;
                AppSession lckSession = null;
                string rowAttribute = string.Empty;
                string note = string.Empty;
                bool isReadLock = false;
                using (IDataReader dr = DataHelper.ExecuteReader(pCs, pDbTransaction, CommandType.Text, 
                    queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                {
                    isReadLock = dr.Read();
                    if (isReadLock)
                    {
                        int idA = Convert.ToInt32(Convert.IsDBNull(dr["IDAUPD"]) ? dr["IDAINS"] : dr["IDAUPD"]);
                        // FI 20200820 [25468] dates systemes en UTC
                        dt = DateTime.SpecifyKind(Convert.ToDateTime(Convert.IsDBNull(dr["DTUPD"]) ? dr["DTINS"] : dr["DTUPD"]), DateTimeKind.Utc);
                        note = Convert.ToString(dr["NOTE"]);
                        extlLink = Convert.ToString(dr["EXTLLINK"]);
                        Convert.ToString(dr["ROWATTRIBUT"]);
                        action = Convert.ToString(dr["ACTION"]);

                        AppInstance appInstance = new AppInstance(dr["HOSTNAME"].ToString(), dr["APPNAME"].ToString(), dr["APPVERSION"].ToString());
                        lckSession = new AppSession(appInstance)
                        {
                            IdA = idA,
                            SessionId = dr["SESSIONID"].ToString()
                        };


                        // EG 20130524 LockMode
                        _lockObject = new LockObject(pLockObject.ObjectType, pLockObject.ObjectId, pLockObject.ObjectIdentifier, dr["LOCKMODE"].ToString());
                        if (StrFunc.IsEmpty(pLockObject.ObjectId))
                            _lockObject.ObjectId = Convert.ToString(dr["OBJECTID"]);
                    }
                }

                if (isReadLock)
                {
                    lck = new Lock(pCs, pDbTransaction, _lockObject, lckSession, action)
                    {
                        Dt = dt,
                        Note = note,
                        ExtLink = extlLink,
                        Rowattribute = rowAttribute,
                        LockMode = _lockObject.LockMode
                    };

                    SQL_Actor actor = new SQL_Actor(CSTools.SetCacheOn(pCs), lckSession.IdA)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (actor.IsLoaded)
                        lckSession.IdA_Identifier = actor.Identifier;
                }
            }
            return lck;
        }

        /// <summary>
        /// Retourne true s'in existe un lock posé sur l'objet {pLockObject}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLockObject"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static bool ExistLock(string pCs, IDbTransaction pDbTransaction, LockObject pLockObject)
        {
            Lock lck = null;
            
            if (pLockObject != null)
                lck = SearchLock(pCs, pDbTransaction, pLockObject);
            
            return (lck != null);
        }

        /// <summary>
        /// Suppression d'un lock 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLockObject"></param>
        /// <param name="pSession">null autorisé. Si renseigné supprime uniquement le lock posé par {pSession}</param>
        /// <returns></returns>
        /// FI 20130527 [18662] pLockObject.ObjectId est de type string
        /// PL 20140917 Refactoring and Debug on Shared mode
        // EG 20180205 [23769] Add dbTransaction  
        public static bool UnLock(string pCs, LockObject pLockObject, string pSession)
        {
            return UnLock(pCs, null, pLockObject, pSession);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool UnLock(string pCs, IDbTransaction pDbTransaction, LockObject pLockObject, string pSession)
        {
            string sqlDelete, sqlWhere;
            bool bUseSession = StrFunc.IsFilled(pSession);
            bool IsOk = true;

            if ((null != pLockObject) && StrFunc.IsFilled(pCs))
            {
                string lockMode = pLockObject.LockMode;
                bool isSharedLockMode = (lockMode == LockTools.Shared);

                sqlDelete = SQLCst.DELETE_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + Cst.CrLf;
                sqlWhere = SQLCst.WHERE + "(OBJECTNAME=@OBJECTNAME)";
                //PL 20140917 [20526] Use like on Shared mode
                if (isSharedLockMode)
                {
                    if (DataHelper.IsDbSqlServer(pCs))
                    {
                        sqlDelete = SQLCst.DELETE_TOP1_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + Cst.CrLf;
                    }
                    else if (DataHelper.IsDbOracle(pCs))
                    {
                        sqlWhere += SQLCst.AND + "(rownum=1)";
                    }
                    else
                        throw new NotImplementedException("RDBMS not implemented");

                    lockMode += "%";
                    sqlWhere += SQLCst.AND + "(LOCKMODE like @LOCKMODE)";
                }
                else
                {
                    sqlWhere += SQLCst.AND + "(LOCKMODE=@LOCKMODE)";
                }

                DataParameters parameters = new DataParameters(new DataParameter[] { });
                parameters.Add(new DataParameter(pCs, "OBJECTNAME", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pLockObject.ObjectType.ToString());
                parameters.Add(new DataParameter(pCs, "LOCKMODE", DbType.AnsiString, 4), lockMode);

                if (StrFunc.IsFilled(pLockObject.ObjectId))
                {
                    sqlWhere += SQLCst.AND + "(OBJECTID=@OBJECTID)" + Cst.CrLf;
                    parameters.Add(new DataParameter(pCs, "OBJECTID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLockObject.ObjectId);
                }
                if (bUseSession)
                {
                    parameters.Add(new DataParameter(pCs, "SESSIONID", DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), pSession);
                    sqlWhere += SQLCst.AND + "(SESSIONID=@SESSIONID)" + Cst.CrLf;
                }

                try
                {
                    DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text, sqlDelete + sqlWhere, parameters.GetArrayDbParameter());
                }
                catch
                {
                    IsOk = false;
                }
            }
            return IsOk;
        }

        /// <summary>
        /// Suppression de tous les locks posés par une session
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSessionid"></param>
        /// <returns></returns>
        public static bool UnLockSession(string pCs, string pSessionid)
        {
            bool isOk = true;
            try
            {
                DataParameter paramSession = new DataParameter(pCs, "SESSIONID", DbType.AnsiString, 128)
                {
                    Value = pSessionid
                };

                SQLWhere where = new SQLWhere("SESSIONID = @SESSIONID");

                string query = SQLCst.DELETE_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + Cst.CrLf;
                query += where.ToString();

                DataHelper.ExecuteNonQuery(pCs, CommandType.Text, query, paramSession.DbDataParameter);
            }
            catch
            {
                isOk = false;
            }
            return isOk;
        }

        /// <summary>
        /// Pose le Lock {pLock}
        /// <para>Consiste un injecter une ligne dans la table EFSLOCK</para>
        /// <para>Retourne true si le lock est correctement inséré</para>
        /// </summary>
        /// <param name="pLock"></param>
        /// <returns></returns>
        /// FI 20130527 [18662] OBJECTID est de type varchar(64)
        /// FI 20150210 [XXXXX] Modify => private Method
        // EG 20180205 [23769] Add dbTransaction  
        private static bool Lock(Lock pLock)
        {
            string cs = pLock.Cs;
            int nbRow;
            try
            {

                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(new DataParameter(cs, "OBJECTNAME", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pLock.LockObject.ObjectType.ToString());
                dataParameters.Add(new DataParameter(cs, "OBJECTID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLock.LockObject.ObjectId);
                dataParameters.Add(new DataParameter(cs, "ACTION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pLock.Action);
                dataParameters.Add(new DataParameter(cs, "LOCKMODE", DbType.AnsiString, 4), pLock.LockObject.LockMode);

                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.SESSIONID), pLock.Session.SessionId);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.HOSTNAME), pLock.Session.AppInstance.HostName);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.APPNAME), pLock.Session.AppInstance.AppName);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.APPVERSION), pLock.Session.AppInstance.AppVersion);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAINS), pLock.Session.IdA);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTINS), pLock.Dt);
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.NOTE), (StrFunc.IsFilled(pLock.Note) ? pLock.Note : Convert.DBNull));
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.EXTLLINK), (StrFunc.IsFilled(pLock.ExtLink) ? pLock.ExtLink : Convert.DBNull));
                dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ROWATTRIBUT), (StrFunc.IsFilled(pLock.Rowattribute) ? pLock.Rowattribute : Convert.DBNull));


                string lckQry = SQLCst.INSERT_INTO_DBO + Cst.EFS_TBL.EFSLOCK.ToString();
                lckQry += @" (OBJECTNAME, OBJECTID, LOCKMODE, ACTION, SESSIONID, HOSTNAME, APPNAME, APPVERSION, NOTE, IDAINS, DTINS, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;

                if (pLock.LockObject.LockMode == LockTools.Exclusive)
                {
                    // PM 20130610 Ajout FROM DUAL pour Oracle
                    lckQry += "select @OBJECTNAME, @OBJECTID, @LOCKMODE, @ACTION, @SESSIONID, @HOSTNAME, @APPNAME, @APPVERSION, @NOTE, @IDAINS, @DTINS, @EXTLLINK, @ROWATTRIBUT" + Cst.CrLf;
                    lckQry += SQLCst.FROM_DUAL + Cst.CrLf;
                    lckQry += SQLCst.UNIONALL + Cst.CrLf;
                    lckQry += SQLCst.SELECT + "OBJECTNAME, OBJECTID, @LOCKMODE, ACTION, SESSIONID, HOSTNAME, APPNAME, APPVERSION, NOTE, IDAINS, DTINS, EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
                    lckQry += SQLCst.FROM_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + Cst.CrLf;
                    lckQry += SQLCst.WHERE + "(OBJECTNAME=@OBJECTNAME) and (OBJECTID=@OBJECTID) and (LOCKMODE<>'X')" + Cst.CrLf;
                }
                else
                {
                    // PM 20130704 Remplacement du count(1) par un max du nombre suivant le 'S'
                    //lckQry += "select @OBJECTNAME, @OBJECTID, @LOCKMODE || convert(varchar, count(1) + 1), @ACTION, @SESSIONID, @HOSTNAME, @APPNAME, @APPVERSION, @NOTE, @IDAINS, @DTINS, @EXTLLINK, @ROWATTRIBUT" + Cst.CrLf;
                    //
                    lckQry += SQLCst.SELECT + "@OBJECTNAME, @OBJECTID";
                    lckQry += ", @LOCKMODE || convert(varchar, isnull(max(substring(l.LOCKMODE,2,(len(l.LOCKMODE)-1))),0) + 1)";
                    lckQry += ", @ACTION, @SESSIONID, @HOSTNAME, @APPNAME, @APPVERSION, @NOTE, @IDAINS, @DTINS, @EXTLLINK, @ROWATTRIBUT" + Cst.CrLf;
                    lckQry += SQLCst.FROM_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + " l" + Cst.CrLf;
                    lckQry += SQLCst.WHERE + "(l.OBJECTNAME=@OBJECTNAME) and (l.OBJECTID=@OBJECTID) and (l.LOCKMODE<>'X')" + Cst.CrLf;
                    // union all : lock exclusif pour interdire la pose d'un lock shared
                    lckQry += SQLCst.UNIONALL + Cst.CrLf;
                    lckQry += SQLCst.SELECT + "l.OBJECTNAME, l.OBJECTID, l.LOCKMODE, l.ACTION, l.SESSIONID, l.HOSTNAME, l.APPNAME, l.APPVERSION, l.NOTE, l.IDAINS, l.DTINS, l.EXTLLINK, l.ROWATTRIBUT" + Cst.CrLf;
                    lckQry += SQLCst.FROM_DBO + Cst.EFS_TBL.EFSLOCK.ToString() + " l" + Cst.CrLf;
                    lckQry += SQLCst.WHERE + "(l.OBJECTNAME=@OBJECTNAME) and (l.OBJECTID=@OBJECTID) and (l.LOCKMODE='X')" + Cst.CrLf;
                }

                QueryParameters qry = new QueryParameters(cs, lckQry, dataParameters);

                nbRow = DataHelper.ExecuteNonQuery(cs, pLock.DbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                if (DataHelper.IsDuplicateKeyError(cs, ex))
                {
                    // Duplicate Key => Continue
                    nbRow = 0;
                }
                else
                    throw;
            }
            return (nbRow == 1);
        }

        /// <summary>
        /// Pose un lock
        /// <para>Retourne true si le lock est posé </para>
        /// <para>Spheres® effectue 10 tentatives maximum (aucune pause entre chaque tentative)</para>
        /// </summary>
        /// <param name="pLock"></param>
        /// <param name="pLockExisting">Retourne le lock déjà existant lorsque le lock n'a pu être posé</param>
        /// <returns>Retourne true si le lock est posé, false sinon</returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static bool LockMode1(Lock pLock, out Lock pLockExisting)
        {

            bool ret = true;
            pLockExisting = null;

            int guard = 0;
            const int MAX_ATTEMPT = 10;
            while (guard < MAX_ATTEMPT)
            {
                guard++;
                ret = LockTools.Lock(pLock);

                if (ret)
                {
                    //Trade verrouillé avec succès
                    break;
                }
                else
                {
                    //déjà verrouillé ??
                    Lock lck = LockTools.SearchLock(pLock.Cs, pLock.DbTransaction, pLock.LockObject);
                    if (null != lck)
                    {
                        //déjà verrouillé --> récupère utilisateur ou process qui verrouille
                        pLockExisting = lck;
                        break;
                    }
                    else
                    {
                        //Le trade vient de se déverouiller --> nouvelle tentative
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// Pose un lock
        /// <para>Retourne true si le lock est posé</para>
        /// <para>Spheres® effectue plusieurs tentatives pendant 30 secondes (pause de 2 secondes entre chaque tentative)</para>
        /// </summary>
        /// <param name="pLock"></param>
        /// <param name="pLockExisting">Retourne le lock déjà existant lorsque le lock n'a pu être posé</param>
        /// <returns>Retourne true si le lock est posé, false sinon</returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static bool LockMode2(Lock pLock, out Lock pLockExisting)
        {

            bool ret = true;
            pLockExisting = null;

            double timeOut = 30; //delai max 

            DatetimeProfiler dtProfiler = new DatetimeProfiler(DateTime.Now);
            bool isToProcess = true;
            int nbProcess = 0;
            while (isToProcess)
            {
                nbProcess++;
                ret = LockTools.Lock(pLock);
                if (ret)
                {
                    isToProcess = false;
                }
                else
                {
                    double elapsedTime = dtProfiler.GetTimeSpan().TotalSeconds;
                    isToProcess = (elapsedTime.CompareTo(timeOut) == -1);
                    System.Threading.Thread.Sleep(2000);
                }
            }

            if (ret == false)
            {
                Lock lck = LockTools.SearchLock(pLock.Cs, pLock.DbTransaction, pLock.LockObject);
                if (null != lck)
                    pLockExisting = lck;
            }

            return ret;
        }



        #endregion Methods
    }

    

}