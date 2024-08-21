using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Actor;

namespace EFS.Restriction
{
    /// <summary>
    /// Classe chargée de piloter l'alimentation SQL de la table SESSIONRESTRICT pour une session 
    /// <para>La table SESSIONRESTRICT est utilisée pour réduire les jeux de résultat SQL aux seuls items autorisés (instruments, acteurs, etc..)</para>
    /// </summary>
    public class SqlSessionRestrict
    {
        #region Members



        /// <summary>
        /// Représente une valeur numérique unique par session 
        /// <para>Il y a équivalence fonctionnelle entre SESSIONID et IDSESSIONID</para>
        /// <para>_idsessionId est généré pour chaque session</para>
        /// <para>_idsessionId est généré lors de la 1er alimenation de la table SESSIONRESTRICT</para>
        /// <para>Les jointures sur SESSIONRESTRICT peuvent se faire sur la colonne SESSIONID ou la colonne IDSESSIONID pour l'instant SESSIONID est préférable puisque cette colonne fait partie d'un index</para>
        /// </summary>
        private readonly int _idSessionId;
        #endregion Members

        #region accessor
        /// <summary>
        /// Obtient l'id unique qui identifie une session 
        /// </summary>
        public int IdSessionId
        {
            get
            {
                return _idSessionId;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public AppSession Session
        {
            get;
            set;
        }

        /// <summary>
        /// Représente la connexion à la base de donnée  
        /// </summary>
        public String Cs
        {
            get;
            set;
        }


        #endregion

        #region constructor
        /// <summary>
        /// Constructor obligatoire si Ajout dans SESSIONRESTRICT
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAppSession"></param>
        /// <param name="pIdSessionId"></param>
        public SqlSessionRestrict(string pCS, AppSession pAppSession, int pIdSessionId)
            : this(pCS, pAppSession)
        {
            _idSessionId = pIdSessionId;
        }

        /// <summary>
        /// Constructor uniquement si suppression dans SESSIONRESTRICT
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAppSession"></param>
        public SqlSessionRestrict(string pCS, AppSession pAppSession)
        {
            Cs = pCS;
            Session = pAppSession;
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// Alimentation de la SESSIONRESTRICT via un insert Select
        /// </summary>
        public void SetRestrictUseSelectUnion(RestrictionBase pRestrict)
        {

            int nbItemByInsert = 1000;

            RestrictionItem[] restrict = pRestrict.GetRestrictItemEnabled();

            if (ArrFunc.IsFilled(restrict))
            {
                AuditTime auditTime = new AuditTime();
                auditTime.AddStep("Start SetRestrictUseSelectUnion on " + pRestrict.Class);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(Cs, "CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pRestrict.Class);

                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.SESSIONID), Session.SessionId);
                parameters.Add(new DataParameter(Cs, "IDSESSIONID", DbType.Int32), _idSessionId);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.HOSTNAME), Session.AppInstance.HostName);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.APPNAME), Session.AppInstance.AppName);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.APPVERSION), Session.AppInstance.AppVersion);

                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAINS), Session.IdA);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(Cs));
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAUPD), Convert.DBNull);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTUPD), Convert.DBNull);

                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.NOTE), Convert.DBNull);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.EXTLLINK), Convert.DBNull);
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ROWATTRIBUT), Convert.DBNull);

                string selectTemplate = SQLCst.SELECT + @"#ID#,@CLASS,@SESSIONID,@IDSESSIONID,@HOSTNAME,@APPNAME,@APPVERSION,@IDAINS,@DTINS,@IDAUPD,@DTUPD,@NOTE,@EXTLLINK,@ROWATTRIBUT";
                selectTemplate += DataHelper.SQLFromDual(Cs);

                StrBuilder sqlQueryIns = new StrBuilder();
                sqlQueryIns += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + Cst.CrLf;
                sqlQueryIns += @"(ID,CLASS,SESSIONID,IDSESSIONID,HOSTNAME,APPNAME,APPVERSION,IDAINS,DTINS,IDAUPD,DTUPD,NOTE,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;

                int length = restrict.Length;
                long nbInsert = Math.DivRem(length, nbItemByInsert, out long rest) + 1;

                for (int j = 0; j < (int)nbInsert; j++)
                {
                    StrBuilder sqlQuerySelelect = new StrBuilder();
                    int min = (j * nbItemByInsert);
                    int max = min + nbItemByInsert;

                    if (max > restrict.Length - 1)
                        max = min + (int)rest;

                    for (int i = min; i < max; i++)
                    {
                        StrBuilder select = new StrBuilder(selectTemplate.Replace("#ID#", restrict[i].id.ToString()) + Cst.CrLf);
                        if (i != max - 1)
                            select.Append(SQLCst.UNIONALL + Cst.CrLf);

                        sqlQuerySelelect.Append(select);
                    }
                    // FI 20140612 [20085] ajout du test sinon Spheres® génère un ordre SQL non correct
                    if (sqlQuerySelelect.Length > 0)
                    {
                        string sQuery = sqlQueryIns.ToString() + sqlQuerySelelect.ToString();

                        DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sQuery, parameters.GetArrayDbParameter());
                    }
                }

                auditTime.AddStep("End SetRestrictUseSelectUnion on " + pRestrict.Class);
                auditTime.WriteDebug();
            }

        }

        /// <summary>
        /// Alimentation de la SESSIONRESTRICT via ADO (method ExecuteDataAdapter)
        /// </summary>
        /// <param name="pRestrict"></param>
        public void SetRestrictUseAdo(RestrictionBase pRestrict)
        {
            RestrictionItem[] restrict = pRestrict.GetRestrictItemEnabled();
            if (ArrFunc.IsFilled(restrict))
            {

                //Init Parametres
                DataParameter paramClass = new DataParameter(Cs, "CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                DataParameter paramId = new DataParameter(Cs, "ID", DbType.Int32);
                //				
                DataParameter paramSessionId = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.SESSIONID);
                DataParameter paramIdSessionId = new DataParameter(Cs, "IDSESSIONID", DbType.Int32);
                DataParameter paramHostName = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.HOSTNAME);
                DataParameter paramAppName = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.APPNAME);
                DataParameter paramAppVersion = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.APPVERSION);
                //
                DataParameter paramIdaIns = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAINS);
                DataParameter paramDtIns = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTINS);
                //
                paramClass.Value = pRestrict.Class;
                //
                paramSessionId.Value = Session.SessionId;
                paramIdSessionId.Value = _idSessionId;
                paramHostName.Value = Session.AppInstance.HostName;
                paramAppName.Value = Session.AppInstance.AppName;
                paramAppVersion.Value = Session.AppInstance.AppVersion;
                paramIdaIns.Value = Session.IdA;
                // FI 20200820 [25468] Dates systemes en UTC
                paramDtIns.Value = OTCmlHelper.GetDateSysUTC(Cs); ;
                //
                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"sr.CLASS,sr.ID,sr.SESSIONID,sr.IDSESSIONID,sr.HOSTNAME,sr.APPNAME,sr.APPVERSION,sr.DTINS,sr.IDAINS" + Cst.CrLf);
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr" + Cst.CrLf;
                //
                StrBuilder sqlSelect2 = new StrBuilder(sqlSelect.ToString());
                sqlSelect2 += SQLCst.WHERE + @"(sr.SESSIONID=@SESSIONID)" + Cst.CrLf;
                sqlSelect2 += SQLCst.AND + @"(sr.CLASS=@CLASS)" + Cst.CrLf;
                //
                DataParameters parameters = new DataParameters(new DataParameter[] { paramSessionId, paramClass });
                //
                DataSet dsRestrict = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelect2.ToString(), parameters.GetArrayDbParameter());
                DataTable dtRestrict = dsRestrict.Tables[0]; // tjs 0 Ligne 
                //
                for (int i = 0; i < restrict.Length; i++)
                {
                    DataRow row = dtRestrict.NewRow();
                    paramId.Value = restrict[i].id;
                    //
                    row["CLASS"] = paramClass.Value;
                    row["ID"] = paramId.Value;
                    //
                    row["SESSIONID"] = paramSessionId.Value;
                    row["IDSESSIONID"] = paramIdSessionId.Value;
                    row["HOSTNAME"] = paramHostName.Value;
                    row["APPNAME"] = paramAppName.Value;
                    row["APPVERSION"] = paramAppVersion.Value;
                    //	
                    row["IDAINS"] = paramIdaIns.Value;
                    row["DTINS"] = paramDtIns.Value;
                    //
                    dtRestrict.Rows.Add(row);
                }
                DataHelper.ExecuteDataAdapter(Cs, sqlSelect.ToString(), dtRestrict);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void UnSetRestriction()
        {
            UnSetRestriction(string.Empty);
        }

        /// <summary>
        ///  Libere toutes les restrictions d'une classe {pClass} {(suppression des lignes existantes sous la table SESSIONRESTRICT)
        /// </summary> 
        ///<param name="pClass">si non renseigné, supprime toutes les restrictions</param>
        public void UnSetRestriction(string pClass)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "SESSIONID", DbType.AnsiString, 128), Session.SessionId);
            if (StrFunc.IsFilled(pClass))
                parameters.Add(new DataParameter(Cs, "CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pClass);

            SQLWhere where = new SQLWhere("SESSIONID = @SESSIONID");
            if (StrFunc.IsFilled(pClass))
                where.Append("CLASS = @CLASS");


            string query = SQLCst.DELETE_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + Cst.CrLf;
            query += where.ToString();

            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, query, parameters.GetArrayDbParameter());


        }

        /// <summary>
        ///  Libere toutes les Restrictions associés à une session (suppression des lignes existantes sous la table SESSIONRESTRICT)
        /// <para>
        ///  A placer ds un Finally pour purger toutes les lignes(surtout en cas d'erreur)=> voir les process (class processBase)
        /// </para>
        /// </summary> 
        /// <param name="pSource"></param>
        /// <param name="pSessionId"></param>
        /// FI 20120907 [18113] modification de la signature de la fonction 
        public static void CleanUp(string pCS, string pSessionId)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "SESSIONID", DbType.AnsiString, 128), pSessionId);
            //
            SQLWhere where = new SQLWhere("SESSIONID = @SESSIONID");
            //
            string query = SQLCst.DELETE_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + Cst.CrLf;
            query += where.ToString();
            //
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, query, parameters.GetArrayDbParameter());

        }
        #endregion
    }

}
