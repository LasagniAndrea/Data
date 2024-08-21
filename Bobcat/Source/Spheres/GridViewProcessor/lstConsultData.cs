#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Rights;
using EfsML.Business;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
#endregion using directives

namespace EFS.GridViewProcessor
{
    #region  LstConsultData
    /// <summary>
    /// Classe employée pour la gestion de la consultation multi-critères 
    /// </summary>
    public class LstConsultData
    {
        /// <summary>
        /// Représente la liste des consultations de spheres®
        /// <para>Certaines spécificités sont fonction de la consultation</para>
        /// </summary>
        public enum ConsultEnum
        {
            /// <summary>
            /// Consultation des ETD disponible depuis la saisie
            /// </summary>
            ASSET_ETD_EXPANDED,
            /// <summary>
            /// Consultation des CashBalance
            /// </summary>
            CASHBALANCE,
            /// <summary>
            /// Consultation des CashInterest
            /// </summary>
            CASHINTEREST,
            /// <summary>
            /// Consultation des CashPayment
            /// </summary>
            CASHPAYMENT,
            /// <summary>
            /// Consultation des titres de créance
            /// </summary>
            DEBTSECURITY,
            /// <summary>
            /// Consultation des cash Flows
            /// </summary>
            EVENT_STL,
            /// <summary>
            /// consultation des flux par asset (ETD Alloc)
            /// </summary>
            FLOWSBYASSET_ALLOC,
            /// <summary>
            /// consultation des flux par asset (OTC Alloc)
            /// </summary>
            FLOWSBYASSETOTC_ALLOC,
            /// <summary>
            /// consultation des flux par devise
            /// </summary>
            FLOWSBYCURRENCY_ALLOC,
            /// <summary>
            /// consultation des flux par trade (ETD Alloc)
            /// </summary>
            FLOWSBYTRADE_ALLOC,
            /// <summary>
            /// consultation des flux par trade (OTC Alloc)
            /// </summary>
            FLOWSBYTRADEOTC_ALLOC,
            /// <summary>
            /// 
            /// </summary>
            MATURITY,
            /// <summary>
            /// Consultation des éditions
            /// </summary>
            MCO_RPT,
            /// <summary>
            /// Consultation des extraits de compte
            /// </summary>
            MCO_RPT_FINPER,
            /// <summary>
            /// Consultation des actions sur positions (ETD Alloc)
            /// </summary>
            POSACTIONDET,
            /// <summary>
            /// Consultation des actions sur positions (OTC Alloc)
            /// </summary>
            POSACTIONDET_OTC,
            /// <summary>
            /// Consultation des positions détaillées (ETD Alloc)
            /// </summary>
            POSDET_ALLOC,
            /// <summary>
            /// Consultation des positions détaillées (OTC Alloc)
            /// </summary>
            POSDETOTC_ALLOC,
            /// <summary>
            /// Consultation des positions synthétiques (ETD Alloc)
            /// </summary>
            POSSYNT_ALLOC,
            /// <summary>
            /// Consultation des Deposit
            /// </summary>
            RISKPERFORMANCE,
            /// <summary>
            /// Consultation des factures
            /// </summary>
            TRADEADMIN,
            /// <summary>
            /// Consultation des trades ETD ALLOC
            /// </summary>
            TRADEFnO_ALLOC,
            /// <summary>
            /// Consultation des trades OTC ALLOC
            /// </summary>
            TRADEOTC_ALLOC,
            /// <summary>
            /// Consultation des trades ETD de type Bloc Trade
            /// </summary>
            TRADEFnO_ORDER_BLOCK,
            /// <summary>
            /// Consultation des trades OTC
            /// </summary>
            TRADEOTC,
            /// <summary>
            /// Consultation des Cash Flows (Partie/Contrepartie)
            /// </summary>
            TRADE_PARTY,
        }

        #region Private members
        private DataTable dtLstAlias;
        private DataTable dtLstConsultAliasSelected;
        private DataTable dtLstOrderBy;
        private DataTable dtLstConsultAlias;
        private bool m_isMultiTableSpecified;
        private bool m_isMultiTable;
        private string m_mainTableName;
        /// <summary>
        /// 
        /// </summary>
        private string _IdMenu;
        private string _consultXML;
        #endregion Private properties

        #region Membres
        public DataTable dtLstWhere;
        public DataTable dtLstSelectAvailable;
        public DataTable dtLstSelectedCol;
        public LstTemplateData template;
        /// <summary>
        /// Nom de la  consultation
        /// <para>Lorsque la consultation s'appuie sur un fichier XML,  le nom est consitué de REF-{typedeConsultation}{ObjectName}  </para>
        /// </summary>
        public string IdLstConsult;
        #endregion

        #region properties

        /// <summary>
        /// Obtient le titre de la consultation
        /// </summary>
        public string Title
        {
            get
            {
                string ret = string.Empty;
                if (StrFunc.IsEmpty(_IdMenu))
                {
                    //Consultation
                    //ret = "TemplateView_" + IdLstConsult;
                    ret = string.Empty;
                    //
                    if (RepositoryWeb.IsReferential(IdLstConsult))
                    {
                        ret = ReferentialShortIdConsult();
                    }
                    else
                    {
                        //PL 20100212 Add switch (Afin d'avoir des titres cohérents sur les consultations ouvertes depuis les boutons "...")
                        switch (IdLstConsult)
                        {
                            case "ASSET_ETD_EXPANDED":
                                ret = "OTC_REF_PRD_LST_INSTR_DRVCONTRACT_ATTRIB_ASSET";
                                break;
                            case "DEBTSECURITY":
                                ret = "OTC_INP_DEBTSECURITY";
                                break;
                            case "MATURITY":
                                ret = "OTC_REF_LST_MATURITYRULE_MATURITY";
                                break;
                        }
                    }
                }
                else
                {
                    //Référentiel
                    ret = _IdMenu;
                }
                //
                ret = Ressource.GetMenu_Fullname(ret, ret);
                //
                return ret;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// <para>
        /// Verification de l'existance d'une consultation  dans la table LSTCONSULT
        /// </para>
        /// <para>Si non trouvée injecte une ligne dans LSTCONSULT</para>
        /// </summary>
        /// <param name="pIdLstConsult">designe le nom de la consultation</param>
        /// <param name="pIdMenu">Désigne ID du menu appelant cette consultation</param>
        public LstConsultData(string pCS, string pIdLstConsult, string pIdMenu)
        {
            m_isMultiTable = true;
            m_mainTableName = SQLCst.TBLMAIN;
            //
            IdLstConsult = null;
            //
            _IdMenu = pIdMenu;
            //
            //verification existance du IDLSTCONSULT
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "CONSULTXML" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDLSTCONSULT=@IDLSTCONSULT";

            DataParameters dbParam = new DataParameters();
            dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), pIdLstConsult);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);

            bool isOk = false;
            using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(pCS, 1, 1), CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter()))
            {
                isOk = dr.Read();
                if (isOk)
                {
                    if (false == (dr["CONSULTXML"] is DBNull))
                        _consultXML = dr["CONSULTXML"].ToString();
                }
            }

            if (false == isOk)
            {
                //Insert a new IDLSTCONSULT (Templates on referentials)
                dbParam = null;
                string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString();
                SQLInsert += @" (IDLSTCONSULT,DISPLAYNAME,DESCRIPTION) values (@IDLSTCONSULT,@DISPLAYNAME,@DESCRIPTION)";

                dbParam = new DataParameters();
                dbParam.Add(new DataParameter(pCS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdLstConsult);
                dbParam.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pIdLstConsult);
                dbParam.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), "Used by Spheres for Templates on referentials");

                qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
                isOk = (nbRow == 1);
            }

            if (isOk)
                IdLstConsult = pIdLstConsult;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Charge les données relatives à la table LSTALIAS pour la consultation concernée
        /// </summary>
        private void LoadLstAlias(string pCS)
        {
            string sqlSelect = @"select la.ALIAS, la.TABLENAME, la.DISPLAYNAME, ltd.RDBMS, ltd.COMMAND, ltd.TABLEDETXML
            from dbo.LSTALIAS la 
            inner join dbo.LSTCONSULTALIAS lca on (la.ALIAS = lca.ALIAS)
            inner join dbo.LSTCONSULT lc on (lca.IDLSTCONSULT = lc.IDLSTCONSULT)
            left outer join dbo.LSTTABLEDET ltd on (ltd.TABLENAME = la.TABLENAME)
            where " + RepositoryWeb.GetSQLClause_Consult(IdLstConsult, "lc") + Cst.CrLf;
            sqlSelect += @"order by la.ALIAS";

            dtLstAlias = DataHelper.ExecuteDataTable(pCS, sqlSelect);
        }

        /// <summary>
        /// Charge les données relatives à la table LSTCONSULTALIAS pour la consultation concernée
        /// </summary>
        private void LoadLstConsultAlias(string pCS)
        {
            string sqlSelect = @"select lca.*
            from dbo.LSTCONSULTALIAS lca 
            where " + RepositoryWeb.GetSQLClause_Consult(IdLstConsult, "lca") + Cst.CrLf;
            sqlSelect += @"order by lca.POSITION";
            dtLstConsultAlias = DataHelper.ExecuteDataTable(pCS, sqlSelect);

        }

        /// <summary>
        /// Charge les données relatives à la table LST_ALIAS pour la consultation courante
        /// NB: On ne charge que les ALIAS nécessaires à l'élaboration ultérieure de la requête
        /// </summary>
        private void LoadLstConsultAliasSelected(string pCS, bool pIsConsultWithDynamicArgs)
        {
            DataParameters dbParam = null;
            string sqlSelect = SQLCst.SELECT + "lcaj.POSITION as JOINPOSITION,lcaj.TYPEJOIN,lcaj.IDLSTJOIN,lca.POSITION as ALIASPOSITION,";

            if (pIsConsultWithDynamicArgs)
            {
                sqlSelect += DataHelper.SQLIsNull(pCS,
                                DataHelper.SQLIsNull(pCS,
                                    DataHelper.SQLIsNull(pCS,
                                        "lcaj.SQLJOINDYNAMYC",
                                        "lj.SQLJOINDYNAMYC"),
                                        "lcaj.SQLJOIN"),
                                        "lj.SQLJOIN",
                            "SQLJOIN");
            }
            else
            {
                sqlSelect += DataHelper.SQLIsNull(pCS, "lcaj.SQLJOIN", "lj.SQLJOIN", "SQLJOIN");
            }

            string SQLJoin = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lca on (lca.ALIAS=l0.ALIAS)" + Cst.CrLf;
            SQLJoin += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on (lcaj.ALIAS=lca.ALIAS)" + SQLCst.AND + " (lcaj.IDLSTCONSULT=lca.IDLSTCONSULT)" + Cst.CrLf;
            SQLJoin += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTJOIN.ToString() + " lj on (lj.IDLSTJOIN=lcaj.IDLSTJOIN)" + Cst.CrLf;
            SQLJoin += SQLCst.WHERE + RepositoryWeb.GetSQLClause_Consult(IdLstConsult, "lca");
            SQLJoin += SQLCst.AND + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "l0", out dbParam);

            StrBuilder SQLSelect = new StrBuilder(sqlSelect);
            if (IsMultiTable(pCS))
            {
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + " l0" + Cst.CrLf;
                SQLSelect += SQLJoin + Cst.CrLf;

                SQLSelect += SQLCst.UNIONALL + Cst.CrLf;

                SQLSelect += sqlSelect + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " l0" + Cst.CrLf;
                SQLSelect += SQLJoin;
                if (template.ISENABLEDLSTWHERE)
                    SQLSelect += SQLCst.AND + "(l0.ISENABLED=" + DataHelper.SQLBoolean(true) + ")" + Cst.CrLf;
                else
                    SQLSelect += SQLCst.AND + "(l0.ISMANDATORY=" + DataHelper.SQLBoolean(true) + ")" + Cst.CrLf;

                SQLSelect += SQLCst.UNIONALL + Cst.CrLf;
                SQLSelect += sqlSelect + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTORDERBY.ToString() + " l0" + Cst.CrLf;
                SQLSelect += SQLJoin;
            }
            else
            {
                //Les alias sont donc utilisés pour effectuer des restricctions (ie: EurosysWeb)
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lca" + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on (lcaj.ALIAS=lca.ALIAS)" + SQLCst.AND + "(lcaj.IDLSTCONSULT=lca.IDLSTCONSULT)" + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTJOIN.ToString() + " lj on (lj.IDLSTJOIN=lcaj.IDLSTJOIN)" + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + RepositoryWeb.GetSQLClause_Consult(IdLstConsult, "lca");
                SQLSelect += SQLCst.AND + "(lca.POSITION!=1)";
            }
            SQLSelect += Cst.CrLf + SQLCst.ORDERBY + "ALIASPOSITION,JOINPOSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            dtLstConsultAliasSelected = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            #region Purge des doublons
            //Purge en C# des éventuels doublons issus de la requête SQL (le faire en SQL compliquerait la query et celle-ci ne serait plus compatible multi-SGBD)
            int currentRow = dtLstConsultAliasSelected.Rows.Count - 1;
            DataRow drCurrent;
            string typeJoin, sqlJoin;
            bool isRemove;
            while (currentRow >= 0)
            {
                drCurrent = dtLstConsultAliasSelected.Rows[currentRow];

                isRemove = ((drCurrent["SQLJOIN"] == Convert.DBNull) || (String.IsNullOrEmpty(drCurrent["SQLJOIN"].ToString())));

                if (!isRemove)
                {
                    typeJoin = DataHelper.SQLString(drCurrent["TYPEJOIN"].ToString());
                    sqlJoin = DataHelper.SQLString(drCurrent["SQLJOIN"].ToString());

                    //Remove de jointures en tous points identiques, sur la base de TYPEJOIN et SQLJOIN
                    isRemove = (dtLstConsultAliasSelected.Select("TYPEJOIN=" + typeJoin + " and SQLJOIN=" + sqlJoin).Length > 1);

                    if (!isRemove)
                    {
                        //PL 20121023 New features: Remove de jointures identiques sur la base de l'alias de table (jointures existantes dans un ensemble de jointure) 
                        int pos_ON = sqlJoin.IndexOf(SQLCst.ON);
                        //int pos_SPACE = sqlJoin.IndexOf(Cst.Space);
                        //if ((pos_SPACE>0) && (pos_ON>pos_SPACE))
                        //{
                        //    string aliasTable = sqlJoin.Substring(pos_SPACE, pos_ON - pos_SPACE + 4);
                        //    aliasTable = DataHelper.SQLString("%" + aliasTable + "%");

                        //    isRemove = (dtLstConsultAliasSelected.Select("SQLJOIN like " + aliasTable).Length > 1);
                        //}
                        if (pos_ON > 0)
                        {
                            if (sqlJoin.IndexOf(SQLCst.ON, pos_ON + 1) == -1)
                            {
                                string join = sqlJoin.Substring(1, sqlJoin.Length - 2).Trim().Replace("''", "'");

                                #region Echappement des caractères réservés.
                                join = join.Replace("[", Cst.StringForTextBoxModePassword);//Tip
                                join = join.Replace("]", "[]]");
                                join = join.Replace(Cst.StringForTextBoxModePassword, "[[]");
                                join = join.Replace("*", "[*]");
                                join = join.Replace("%", "[%]");
                                #endregion
                                join = DataHelper.SQLString("%" + join + "%");

                                isRemove = (dtLstConsultAliasSelected.Select("SQLJOIN like " + join).Length > 1);

                                if (isRemove)
                                    System.Diagnostics.Debug.WriteLine(join);
                            }
                        }
                    }
                }

                if (isRemove)
                {
                    dtLstConsultAliasSelected.Rows.Remove(drCurrent);
                }

                currentRow--;
            }
            #endregion

            dtLstConsultAliasSelected.Select(string.Empty, "ALIASPOSITION asc,JOINPOSITION asc");
        }


        /// <summary>
        /// Retourne True lorsqu'il existe des données relatives à la table LSTSELECT pour la consultation en cours, sinon False.
        /// </summary>
        private bool ExistsLstSelect(string pCS)
        {
            bool ret = false;

            DataParameters dbParam = null;
            StrBuilder SQLSelect = new StrBuilder();
            SQLSelect += SQLCst.SELECT + "1" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + " ls" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "ls", out dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter()); ;
            ret = (null != obj);

            return ret;
        }

        #region private GetSQLClause_ConsultAndCurrentUser
        /// <summary>
        /// Obtient la clause where applicable à la consultation en cours et l'acteur en cours
        /// </summary>
        /// <returns>string: Clause where</returns>
        private string GetSQLClause_ConsultAndCurrentUser()
        {
            return RepositoryWeb.GetSQLClause_Consult(IdLstConsult, "lt") + SQLCst.AND + "(lt.IDA=" + SessionTools.User.idA + ")" + Cst.CrLf;
        }
        #endregion

        /// <summary>
        /// Scan if it's a multi-table consultation
        /// </summary>
        private void GetIsMultiTable(string pCS)
        {

            m_isMultiTableSpecified = true;
            m_isMultiTable = true;
            m_mainTableName = SQLCst.TBLMAIN;
            //
            try
            {

                if (IsConsultation(ConsultEnum.TRADE_PARTY) ||
                    IsConsultation(ConsultEnum.TRADEOTC) ||
                    IsConsultation(ConsultEnum.TRADEFnO_ORDER_BLOCK) ||
                    IsConsultation(ConsultEnum.TRADEFnO_ALLOC) ||
                    IsConsultation(ConsultEnum.TRADEADMIN) ||
                    IsConsultation(ConsultEnum.FLOWSBYTRADE_ALLOC)
                    )
                {
                    //Raccourci... pour éviter la query
                    m_isMultiTable = true;
                    m_mainTableName = Cst.OTCml_TBL.TRADE.ToString();
                }
                else if (IsConsultation(ConsultEnum.DEBTSECURITY))
                {
                    //Raccourci... pour éviter la query
                    m_isMultiTable = true;
                    m_mainTableName = Cst.OTCml_TBL.VW_TRADE_ASSET.ToString();
                }
                else
                {
                    string SQLQuery = SQLCst.SELECT_DISTINCT + "lcol.TABLENAME, lca.POSITION, lcaj.IDLSTJOIN" + Cst.CrLf;
                    SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + " lcol" + Cst.CrLf;
                    SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTABLE.ToString() + " lt on (lt.TABLENAME = lcol.TABLENAME)" + Cst.CrLf;
                    SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTALIAS.ToString() + " la on (la.TABLENAME = lt.TABLENAME)" + Cst.CrLf;
                    SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lca on (lca.ALIAS = la.ALIAS)" + Cst.CrLf;

                    SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on (lcaj.ALIAS = lca.ALIAS)";
                    SQLQuery += SQLCst.AND + "(lcaj.IDLSTCONSULT = lca.IDLSTCONSULT)" + Cst.CrLf;

                    SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString() + " lcon on (lcon.IDLSTCONSULT = lca.IDLSTCONSULT)" + Cst.CrLf;
                    SQLQuery += SQLCst.WHERE + "lcon.IDLSTCONSULT = " + DataHelper.SQLString(IdLstConsult) + Cst.CrLf;
                    SQLQuery += SQLCst.ORDERBY + "lca.POSITION";
                    //
                    DataTable dt = DataHelper.ExecuteDataTable(pCS, SQLQuery);
                    m_isMultiTable = (1 < dt.Rows.Count);
                    if (m_isMultiTable)
                        m_mainTableName = dt.Rows[0].ItemArray[2].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                m_isMultiTable = true;
                //FI 201112 Mis en place pour détecter pb
#if DEBUG
                throw new Exception("Exception gérée uniquement en Mode Debug", ex);
#endif
            }
        }

        /// <summary>
        /// Charge l'ensemble des données relatives à une consultation (alias,where,orderby...)
        /// </summary>
        /// <param name="pIsConsultWithDynamicArgs">bool: "True" si consultation avec un ou plusieurs DynamicArguments (ex. DEBTSECURITY)</param>
        public void LoadLstDatatable(string pCS, bool pIsConsultWithDynamicArgs)
        {
            LoadLstConsultAlias(pCS);
            LoadLstConsultAliasSelected(pCS, pIsConsultWithDynamicArgs);
            LoadLstAlias(pCS);
            LoadLstWhere(pCS, true, true);
            LoadLstOrderBy(pCS);
            LoadLstSelectedCol(pCS);
        }

        /// <summary>
        /// Charge un template
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdAOwner">ID du propriétaire du template</param>
        public void LoadTemplate(string pCS, string pIdLstTemplate, int pIdAOwner)
        {
            LoadTemplate(pCS, pIdLstTemplate, pIdAOwner, false);
        }
        /// <summary>
        /// Charge un template
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdAOwner">ID du propriétaire du template</param>
        /// <param name="pIsNewTemplate">Indicateur de création d'un nouveau template destiné à un usage via un filtre sur une FK.</param>
        public void LoadTemplate(string pCS, string pIdLstTemplate, int pIdAOwner, bool pIsNewTemplate)
        {
            template = new LstTemplateData();

            //PL 20150601 GUEST New feature
            if (!string.IsNullOrEmpty(pIdLstTemplate))
            {
                template.Load(pCS, IdLstConsult, pIdLstTemplate, pIdAOwner);

                #region S'il n'existe aucune colonne pour l'affichage, alors on insere par defaut les colonnes d'un Template ou de la table principale de la consultation
                if (!RepositoryWeb.IsReferential(IdLstConsult))
                {
                    if (!ExistsLstSelect(pCS))
                    {
                        bool isExistTemplate = false;

                        #region Template
                        string SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,TABLENAME,COLUMNNAME,ALIAS,POSITION,AGGREGATE) " + Cst.CrLf;
                        SQLQuery += SQLCst.SELECT + template.IDA.ToString() + "," + DataHelper.SQLString(IdLstConsult) + "," + DataHelper.SQLString(template.IDLSTTEMPLATE) + ",TABLENAME,COLUMNNAME,ALIAS,POSITION,AGGREGATE" + Cst.CrLf;
                        SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + Cst.CrLf;
                        SQLQuery += SQLCst.WHERE + "IDA=1" + SQLCst.AND + "IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult) + SQLCst.AND + "IDLSTTEMPLATE='Template'";

                        int rows = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);

                        if (rows > 0)
                        {
                            isExistTemplate = true;

                            // MF 20120430 ruptures with groupingset
                            SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTORDERBY.ToString() + "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,TABLENAME,COLUMNNAME,ALIAS,POSITION,ASCDESC,ISGROUP,ISGROUPBY,ISGROUPINGSET,GROUPINGSET) " + Cst.CrLf;
                            SQLQuery += SQLCst.SELECT + template.IDA.ToString() + "," + DataHelper.SQLString(IdLstConsult) + "," + DataHelper.SQLString(template.IDLSTTEMPLATE) + ",TABLENAME,COLUMNNAME,ALIAS,POSITION,ASCDESC,ISGROUP,ISGROUPBY,ISGROUPINGSET,GROUPINGSET" + Cst.CrLf;
                            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTORDERBY.ToString() + Cst.CrLf;
                            SQLQuery += SQLCst.WHERE + "IDA=1" + SQLCst.AND + "IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult) + SQLCst.AND + "IDLSTTEMPLATE='Template'";

                            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);
                        }
                        #endregion

                        #region Main table
                        if (!isExistTemplate)
                        {
                            SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,TABLENAME,COLUMNNAME,ALIAS,POSITION) " + Cst.CrLf;
                            SQLQuery += SQLCst.SELECT + template.IDA.ToString() + "," + DataHelper.SQLString(IdLstConsult) + "," + DataHelper.SQLString(template.IDLSTTEMPLATE) + ",lc.TABLENAME,lc.COLUMNNAME,la.ALIAS,0" + Cst.CrLf;
                            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + " lc" + Cst.CrLf;
                            SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on ((lcaj.POSITION=0)" + SQLCst.AND + "(lcaj.IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult) + "))" + Cst.CrLf;
                            SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTALIAS.ToString() + " la on ((la.TABLENAME=lc.TABLENAME)" + SQLCst.AND + "(la.ALIAS=lcaj.ALIAS))";

                            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);
                        }
                        #endregion
                    }
                }
                #endregion

                #region Insertion des critères "mandatory" (LSTCONSULTWHERE) éventuellement inexistant (LSTWHERE)
                //PL 20121126: Sur un nouveau template destiné à un usage avec une FK (ex. référentiel enfant), on n'impose plus les éventuels filtres obligatoires.
                if (!pIsNewTemplate)
                {
                    string SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTWHERE.ToString();
                    SQLQuery += "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY)" + Cst.CrLf;
                    SQLQuery += SQLCst.SELECT + template.IDA.ToString() + ",lcw.IDLSTCONSULT," + DataHelper.SQLString(template.IDLSTTEMPLATE);
                    SQLQuery += ",lcw.IDLSTCOLUMN,lcw.TABLENAME,lcw.COLUMNNAME,lcw.ALIAS,lcw.POSITION,lcw.OPERATOR,lcw.LSTVALUE,null,1,1" + Cst.CrLf;
                    SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULTWHERE.ToString() + " lcw" + Cst.CrLf;
                    SQLQuery += SQLCst.WHERE + "lcw.IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult);
                    //
                    SQLQuery += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
                    SQLQuery += SQLCst.WHERE + "lw.IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult);
                    SQLQuery += SQLCst.AND + "lw.IDLSTTEMPLATE=" + DataHelper.SQLString(template.IDLSTTEMPLATE);
                    SQLQuery += SQLCst.AND + "lw.IDA=" + DataHelper.SQLString(template.IDA.ToString());
                    SQLQuery += SQLCst.AND + "(lw.TABLENAME=lcw.TABLENAME and lw.COLUMNNAME=lcw.COLUMNNAME)";
                    SQLQuery += ")";

                    int rows = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);
                    if (rows > 0)
                    {
                        //Tip for debug.
                        rows = 0;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Retourne les données relatives à la table LSTSELECT et infos complémentaires pour la consultation et le template concernés.
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable LoadLstSelectedCol(string pCS)
        {
            return LoadLstSelectedCol(pCS, 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pResMulti_Index"></param>
        /// <returns></returns>
        public DataTable LoadLstSelectedCol(string pCS, int pResMulti_Index)
        {
            //------------------------------------------------
            // Avec Recherche de l'item correspondant dans LSTCONSULTALIAS. 
            //      Recherche de l'éventuel item "header" correspondant dans LSTCONSULTALIAS, 
            //      puis dans LSTCONSALIASJOIN en position 0 ou 1 (afin de ne pas engendrer de produit cartésien).
            DataParameters dbParam = null;
            string sqlWhere = template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "ls", out dbParam);
            string sqlSelect = @"select ls.ALIAS, lco.TABLENAME, lco.COLUMNNAME, lco.DISPLAYNAME, lco.SQLSELECT, lco.DATATYPE, lco.ISRESOURCE, lco.SCALE,
            lco.SQLORDER, lco.AGGREGATE, lco.SQLGROUPBY, lcoref.COLUMNNAME as COLUMNREF,
            case when lccd.COLUMNXML is null then lco.COLUMNXML else lccd.COLUMNXML end as COLUMNXML,
            lcaj.IDLSTJOIN, isnull(isnull(lcajheader1.IDLSTJOIN, lcajheader0.IDLSTJOIN), lcaj.IDLSTJOIN) as ALIASDISPLAYNAME,
            lob.ISGROUPBY, lob.ISGROUPINGSET, lob.GROUPINGSET
            from dbo.LSTSELECT ls
            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME = ls.TABLENAME) and (lco.COLUMNNAME = ls.COLUMNNAME)
            inner join dbo.LSTCONSALIASJOIN  lcaj on (lcaj.IDLSTCONSULT = @IDLSTCONSULT) and (lcaj.ALIAS = ls.ALIAS) and 
            (lcaj.POSITION = 
                ( 
                    select max(tmp.POSITION)
                    from dbo.LSTCONSALIASJOIN tmp 
                    where (tmp.IDLSTCONSULT = lcaj.IDLSTCONSULT) and (tmp.ALIAS = lcaj.ALIAS)
                )
            )
            inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT = @IDLSTCONSULT) and (lca.ALIAS = ls.ALIAS)
            left outer join dbo.LSTCONSULTALIAS lcaheader on (lcaheader.IDLSTCONSULT = @IDLSTCONSULT) and (lcaheader.ALIAS = lca.ALIASHEADER)
            left outer join dbo.LSTCONSALIASJOIN lcajheader1 on (lcajheader1.IDLSTCONSULT = @IDLSTCONSULT) and (lcajheader1.ALIAS = lcaheader.ALIAS) and (lcajheader1.POSITION = 1)
            left outer join dbo.LSTCONSALIASJOIN lcajheader0 on (lcajheader0.IDLSTCONSULT = @IDLSTCONSULT) and (lcajheader0.ALIAS = lcaheader.ALIAS) and (lcajheader0.POSITION = 0)
            left outer join dbo.LSTCOLUMN lcoref on (lcoref.TABLENAME = lco.TABLENAME) and (lcoref.COLUMNNAME = lco.COLUMNNAMEREF)
            left outer join dbo.LSTCONSCOLUMNDET lccd on (lccd.TABLENAME = lco.TABLENAME) and (lccd.COLUMNNAME = lco.COLUMNNAME) and (lccd.IDLSTCONSULT = @IDLSTCONSULT)
            left outer join dbo.LSTORDERBY lob on (lob.IDLSTTEMPLATE = ls.IDLSTTEMPLATE) and (lob.IDA = ls.IDA) and (lob.IDLSTCONSULT = ls.IDLSTCONSULT) and 
            (lob.ALIAS = ls.ALIAS) and (lob.TABLENAME=ls.TABLENAME and lob.COLUMNNAME=ls.COLUMNNAME)
            where " + sqlWhere + Cst.CrLf + @" order by ls.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dbParam);
            dtLstSelectedCol = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            foreach (DataRow dr in dtLstSelectedCol.Rows)
            {
                if (StrFunc.IsFilled(dr["ALIASDISPLAYNAME"].ToString()))
                {
                    dr["ALIASDISPLAYNAME"] = Ressource.GetMultiForDatagrid(IdLstConsult, dr["TABLENAME"].ToString(), "TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim(), pResMulti_Index);
                    dr["DISPLAYNAME"] = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim(), pResMulti_Index); //PL 20121024
                }
            }
            return dtLstSelectedCol;
        }


        /// <summary>
        /// Charge dans le datatable dtLstSelectAvailable (membre de la classe) les données relatives aux différentes tables LSTXXX pour la consultation concernée.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsOptGroupDDL"></param>
        public void LoadLstSelectAvailable(string pCS)
        {
            //****************************************************************************************************
            //WARNING: Query similaire dans DDLLoad_LSTCOLUMN()  *************************************************
            //****************************************************************************************************
            DataParameters dbParam = null;
            string sqlWhere = template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "ltmp", out dbParam);
            string sqlSelect = @"select 
            lco.IDLSTCOLUMN, lco.DISPLAYNAME, lco.DATATYPE, lco.TABLENAME, lco.COLUMNNAME, lco.COLUMNXML, lcref.COLUMNNAME as COLUMNREF, 
            isnull(lca.ALIASHEADER, lca.ALIAS) as ALIASHEADER, lca.POSITION, lcaj.IDLSTJOIN, lcaj.IDLSTJOIN as ALIASDISPLAYNAME, la.ALIAS,
            case when (lcw.TABLENAME = lco.TABLENAME) and (lcw.COLUMNNAME = lco.COLUMNNAME) then 1 else 0 end as ISMANDATORY
            from dbo.LSTTEMPLATE ltmp
            inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT = ltmp.IDLSTCONSULT)
            inner join dbo.LSTALIAS la on (la.ALIAS = lca.ALIAS)
            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME = la.TABLENAME)
            inner join dbo.LSTCONSALIASJOIN lcaj on (lcaj.IDLSTCONSULT = ltmp.IDLSTCONSULT) and (lcaj.ALIAS = lca.ALIAS) and (lcaj.POSITION = 
            (
                select max(lcaj2.POSITION) 
                from dbo.LSTCONSALIASJOIN lcaj2
                where (lcaj2.IDLSTCONSULT = lcaj.IDLSTCONSULT) and (lcaj2.ALIAS = lcaj.ALIAS)
            ))
            left outer join dbo.LSTCOLUMN lcref on (lcref.TABLENAME = lco.TABLENAME) and (lcref.COLUMNNAME = lco.COLUMNNAMEREF)
            left outer join dbo.LSTCONSULTWHERE lcw on (lcw.IDLSTCONSULT = ltmp.IDLSTCONSULT)
            where " + sqlWhere + Cst.CrLf + @" order by lca.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dbParam);
            dtLstSelectAvailable = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            #region Replace data by ressource
            int currentPosition = 0;
            int lastPosition = 0;
            string currentHeader = string.Empty;
            string lastHeader = string.Empty;
            foreach (DataRow dr in dtLstSelectAvailable.Rows)
            {
                if (StrFunc.IsFilled(dr["ALIASDISPLAYNAME"].ToString()))
                {
                    dr["ALIASDISPLAYNAME"] = Ressource.GetMultiForDatagrid(IdLstConsult, dr["TABLENAME"].ToString(), "TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim());
                    dr["DISPLAYNAME"] = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim());
                }
                //PL 20121022 Mise à jour de la POSITION des tables annexes d'un même ALIASHEADER
                currentPosition = Convert.ToInt32(dr["POSITION"].ToString());
                currentHeader = dr["ALIASHEADER"].ToString();
                if (lastHeader != currentHeader)
                {
                    lastPosition = currentPosition;
                    lastHeader = currentHeader;
                }
                else
                {
                    dr["POSITION"] = currentPosition;
                }
            }
            #endregion
        }

        /// <summary>
        /// Charge dans le datatable dtLstWhere les données relatives à la table LSTWHERE et données complémentaires pour la consultation et le template concernés
        /// </summary>
        public void LoadLstWhere(string pCS, bool pIsEnabledOnly, bool pIsLstSource)
        {
            DataParameters dbParam = null;
            string sqlWhere = template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out dbParam) + (pIsEnabledOnly ? @" and (lw.ISENABLED = 1)" : string.Empty) + Cst.CrLf;
            string sqlSelect = string.Empty;
            if (pIsLstSource)
            {
                sqlSelect = @"select lw.IDLSTCOLUMN, lw.TABLENAME, lw.COLUMNNAME, lw.OPERATOR, lw.LSTVALUE, lw.LSTIDVALUE, lw.ALIAS, lw.ISENABLED, lw.ISMANDATORY, 
                isnull(lca.ALIASHEADER, lca.ALIAS) as ALIASHEADER, lc.DATATYPE, lc.SQLWHERE, lc.SQLSELECT, la.TABLENAME
                from dbo.LSTWHERE lw
                inner join dbo.LSTCOLUMN lc on (lc.TABLENAME = lw.TABLENAME) and (lc.COLUMNNAME = lw.COLUMNNAME)
                inner join dbo.LSTALIAS la on (la.ALIAS = lw.ALIAS)
                inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT = lw.IDLSTCONSULT) and (lca.ALIAS = lw.ALIAS)" + Cst.CrLf;
            }
            else
            {
                sqlSelect = @"select lw.IDLSTCOLUMN, lw.TABLENAME, lw.COLUMNNAME, lw.OPERATOR, lw.LSTVALUE, lw.LSTIDVALUE, lw.ALIAS, lw.ISENABLED, lw.ISMANDATORY,
                lw.ALIAS as ALIASHEADER
                from dbo.LSTWHERE lw" + Cst.CrLf;
            }
            sqlSelect += @"where " + sqlWhere + (pIsEnabledOnly ? @" and (lw.ISENABLED = 1)" : string.Empty) + Cst.CrLf;
            sqlSelect += @"order by lw.ISMANDATORY desc, lw.POSITION" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dbParam);
            dtLstWhere = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Charge dans le datatable dtLstOrderBy (datatable d'instance dans la classe) les données relatives à la table LSTORDERBY pour la consultation et le template concernés
        /// </summary>
        public DataTable LoadLstOrderBy(string pCS)
        {
            DataParameters dbParam = null;
            //------------------------------------------------
            // Recherche de l'item correspondant dans LSTCONSULTALIAS. 
            // Recherche de l'éventuel item "header" correspondant dans LSTCONSULTALIAS, puis dans LSTCONSALIASJOIN en position 0 ou 1 (afin de ne pas engendrer de produit cartésien).

            string sqlWhere = template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lob", out dbParam);
            string sqlSelect = @"select lob.ASCDESC, lob.ALIAS, lob.ISGROUPBY, lob.ISGROUPINGSET, lob.GROUPINGSET, 
            lco.TABLENAME, lco.COLUMNNAME, lco.DISPLAYNAME, lco.AGGREGATE, lco.SQLGROUPBY, lco.SQLSELECT, lco.DATATYPE, lco.SQLORDER, 
            lcref.COLUMNNAME as COLUMNREF, lcaj.IDLSTJOIN, 
            isnull(isnull(lcajheader1.IDLSTJOIN, lcajheader0.IDLSTJOIN), lcaj.IDLSTJOIN) as ALIASDISPLAYNAME
            from dbo.LSTORDERBY lob
            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME = lob.TABLENAME) and (lco.COLUMNNAME = lob.COLUMNNAME)
            inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT = @IDLSTCONSULT) and (lca.ALIAS = lob.ALIAS)
            inner join dbo.LSTCONSALIASJOIN lcaj on(lcaj.IDLSTCONSULT = lob.IDLSTCONSULT) and (lcaj.ALIAS = lob.ALIAS) and (lcaj.POSITION = 
            (
                select  max(tmp.POSITION) 
                from dbo.LSTCONSALIASJOIN tmp 
                where (tmp.IDLSTCONSULT = lcaj.IDLSTCONSULT) and (tmp.ALIAS = lcaj.ALIAS)
            ))
            left outer join dbo.LSTCOLUMN lcref on (lcref.TABLENAME = lco.TABLENAME) and (lcref.COLUMNNAME = lco.COLUMNNAMEREF)
            left outer join dbo.LSTCONSULTALIAS  lcaheader   on (lcaheader.IDLSTCONSULT   = @IDLSTCONSULT) and (lcaheader.ALIAS   = lca.ALIASHEADER)
            left outer join dbo.LSTCONSALIASJOIN lcajheader1 on (lcajheader1.IDLSTCONSULT = @IDLSTCONSULT) and (lcajheader1.ALIAS = lcaheader.ALIAS) and (lcajheader1.POSITION = 1)
            left outer join dbo.LSTCONSALIASJOIN lcajheader0 on (lcajheader0.IDLSTCONSULT = @IDLSTCONSULT) and (lcajheader0.ALIAS = lcaheader.ALIAS) and (lcajheader0.POSITION = 0)
            where " + sqlWhere + Cst.CrLf + @"order by lob.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dbParam);
            dtLstOrderBy = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            foreach (DataRow dr in dtLstOrderBy.Rows)
            {
                if (StrFunc.IsFilled(dr["ALIASDISPLAYNAME"].ToString()))
                {
                    dr["ALIASDISPLAYNAME"] = Ressource.GetMulti("TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim());
                    dr["DISPLAYNAME"] = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim()); //PL 20121024
                }
            }
            return dtLstOrderBy;
        }

        /// <summary>
        /// Création d'une copie d'un Template avec copie childs from ...   
        /// </summary>
        /// <param name="pSourceIdLstTemplate">string: ID du template Source</param>
        /// <param name="pSourceIdA">int: ID du propriétaire du template Source</param>
        public void InsertOverWriteTemplateWithCopyChildsFrom(string pCS, string pSourceIdLstTemplate, int pSourceIdA)
        {
            string newIdLstTemplate = CreateOverwrite(pCS, template);
            LstTemplateData.CopyChilds(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, newIdLstTemplate, 0);
        }

        /// <summary>
        /// Surcharge de CreateCopyTemporaryTemplate(string pSourceIdLstTemplate,int pSourceIdA, int pForcedIda)
        /// </summary>
        /// <param name="pSourceIdLstTemplate">string: ID du template temporaire à copier</param>
        /// <returns>ID du template temporaire créé</returns>
        public string CreateCopyTemporaryTemplate(string pCS, string pSourceIdLstTemplate)
        {
            return CreateCopyTemporaryTemplate(pCS, pSourceIdLstTemplate, SessionTools.Collaborator_IDA, SessionTools.Collaborator_IDA);
        }
        /// <summary>
        /// Surcharge de CreateCopyTemporaryTemplate(string pSourceIdLstTemplate,int pSourceIdA, int pForcedIda)
        /// </summary>
        /// <param name="pSourceIdLstTemplate">string: ID du template temporaire à copier</param>
        /// <param name="pSourceIdA">int: ID du propriétaire du template temporaire à copier</param>
        /// <returns>ID du template temporaire créé</returns>
        public string CreateCopyTemporaryTemplate(string pCS, string pSourceIdLstTemplate, int pSourceIdA)
        {
            return CreateCopyTemporaryTemplate(pCS, pSourceIdLstTemplate, pSourceIdA, SessionTools.Collaborator_IDA);
        }
        /// <summary>
        /// Fonction de création d'une copie de type temporaire d'un template
        /// </summary>
        /// <param name="pSourceIdLstTemplate">string: ID du template temporaire à copier</param>
        /// <param name="pSourceIdA">int: ID du propriétaire du template temporaire à copier</param>
        /// <param name="pForcedIda">int: ID forcé pour le propriétaire du nouveau tempate temporaire (passOver l'ID du user connecté)</param>
        /// <returns>ID du template temporaire créé</returns>
        public string CreateCopyTemporaryTemplate(string pCS, string pSourceIdLstTemplate, int pSourceIdA, int pTargetIdA)
        {
            DuplicateTemplate(pCS, pSourceIdLstTemplate, pSourceIdA,
                LstTemplateData.TEMPORARYPREFIX + pSourceIdLstTemplate, pTargetIdA, false);
            //
            return LstTemplateData.TEMPORARYPREFIX + pSourceIdLstTemplate;
        }


        /// <summary>
        /// Fonction de création d'une copie d'un template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSourceIdLstTemplate"></param>
        /// <param name="pSourceIdA"></param>
        /// <param name="pTargetIdLstTemplate"></param>
        /// <param name="pTargetIdA"></param>
        /// <param name="pIsDeleteSource"></param>
        public void DuplicateTemplate(string pCS,
            string pSourceIdLstTemplate, int pSourceIdA,
            string pTargetIdLstTemplate, int pTargetIdA,
            bool pIsDeleteSource)
        {
            //on recupere le LSTTEMPLATE source pour la copie
            LstTemplateData template = new LstTemplateData();
            template.Load(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA);

            //on modifie le name source                   
            template.IDLSTTEMPLATE = pTargetIdLstTemplate;

            if (SessionTools.IsSessionGuest) //PL 20150601 GUEST New feature
            {
                //GUEST: on affecte en "dur" les droits ( see also LSTSAVE.aspx.cs - SaveData() )
                template.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
                template.RIGHTENTITY = RightsTypeEnum.REMOVE.ToString();
                template.RIGHTDEPARTMENT = RightsTypeEnum.REMOVE.ToString();
                template.RIGHTDESK = RightsTypeEnum.NONE.ToString();
            }

            //on insere dans les tables SQL 
            CreateOverwrite(pCS, template, pTargetIdA);

            //on copie les childs dans les tables SQL 
            LstTemplateData.CopyChilds(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, pTargetIdLstTemplate, pTargetIdA);
            //
            if (pIsDeleteSource)
                RepositoryWeb.Delete(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, true);
        }

        /// <summary>
        /// Obtient les infos concernant les critères spécifiés sous LSTWHERE pour une consultation
        /// </summary>
        /// FI 20140519 [19923] Modification de la signatute de la méthode add pIdAentity
        public LstWhereData[] GetInfoWhere(string pCS, int pIdAEntity)
        {
            LstWhereData[] ret = null;
            DataParameters dbParam = null;

            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "lw.OPERATOR,lw.LSTVALUE,lw.ISMANDATORY,lco.ISRESOURCE,lw.POSITION," + Cst.CrLf;
            SQLSelect += "lco.TABLENAME,lco.DISPLAYNAME,lco.DATATYPE," + Cst.CrLf;
            SQLSelect += "lcaj.IDLSTJOIN," + DataHelper.SQLIsNull(pCS, DataHelper.SQLIsNull(pCS, "lcajheader1.IDLSTJOIN", "lcajheader0.IDLSTJOIN"), "lcaj.IDLSTJOIN", "ALIASDISPLAYNAME") + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + " lco on (lco.TABLENAME=lw.TABLENAME and lco.COLUMNNAME=lw.COLUMNNAME)" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDLSTTEMPLATE=lw.IDLSTTEMPLATE and lt.IDLSTCONSULT=lw.IDLSTCONSULT and lt.IDA=lw.IDA)" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on(lcaj.IDLSTCONSULT=@IDLSTCONSULT and lcaj.ALIAS=lw.ALIAS ";
            SQLSelect += " and lcaj.POSITION = (" + SQLCst.SELECT + " max(tmp.POSITION) " + SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " tmp where (tmp.IDLSTCONSULT=lcaj.IDLSTCONSULT and tmp.ALIAS=lcaj.ALIAS)))";
            //------------------------------------------------
            // Recherche de l'item correspondant dans LSTCONSULTALIAS. 
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lca on(lca.IDLSTCONSULT=@IDLSTCONSULT and lca.ALIAS=lw.ALIAS)" + Cst.CrLf;
            // Recherche de l'éventuel item "header" correspondant dans LSTCONSULTALIAS, puis dans LSTCONSALIASJOIN en position 0 ou 1 (afin de ne pas engendrer de produit cartésien).
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lcaheader on(lcaheader.IDLSTCONSULT=@IDLSTCONSULT and lcaheader.ALIAS=lca.ALIASHEADER)" + Cst.CrLf;
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcajheader1 on(lcajheader1.IDLSTCONSULT=@IDLSTCONSULT and lcajheader1.ALIAS=lcaheader.ALIAS and lcajheader1.POSITION=1)" + Cst.CrLf;
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcajheader0 on(lcajheader0.IDLSTCONSULT=@IDLSTCONSULT and lcajheader0.ALIAS=lcaheader.ALIAS and lcajheader0.POSITION=0)" + Cst.CrLf;
            //------------------------------------------------
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out dbParam);
            SQLSelect += SQLCst.AND + "(";
            SQLSelect += "(lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + "and lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += SQLCst.OR;
            SQLSelect += "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += ")" + Cst.CrLf;
            SQLSelect += SQLCst.ORDERBY + "lw.POSITION";

            ArrayList al = new ArrayList();
            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    LstWhereData where = new LstWhereData();

                    //COLUMNNAME                    
                    string joinName = Convert.ToString(dr["ALIASDISPLAYNAME"]).Trim();
                    string columnName = Convert.ToString(dr["DISPLAYNAME"]).Trim();

                    string columnIdentifier = string.Empty;
                    if (IsMultiTable(pCS) && (joinName != MainTableName(pCS)))
                    {
                        //columnIdentifier = "[" + Ressource.GetMulti("TBL_" + joinName) + "] " + Ressource.GetMulti("COL_" + columnName);
                        string tableRessource = Ressource.GetMultiForDatagrid(IdLstConsult, dr["TABLENAME"].ToString(), "TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim(), 1);
                        string columnRessource = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim(), 1);
                        columnIdentifier = "[" + tableRessource + "] " + columnRessource;
                    }
                    else
                    {
                        //CC/PL 20151215 Use GetMultiForDatagrid instead of GetMulti
                        //columnIdentifier = Ressource.GetMulti("COL_" + columnName);
                        columnIdentifier = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + columnName, 1);
                    }
                    where.columnIdentifier = columnIdentifier;

                    //OPERATOR
                    where.@operator = Convert.ToString(dr["OPERATOR"]).ToString();

                    //LSTVALUE formatted
                    where.lstValue = FormatLstValue2(pCS, Convert.ToString(dr["LSTVALUE"]),
                                    Convert.ToString(dr["DATATYPE"]), true, Convert.ToBoolean(dr["ISRESOURCE"]), false, pIdAEntity);

                    //ISMANDATOTY
                    where.isMandatory = BoolFunc.IsTrue(Convert.ToString(dr["ISMANDATORY"]));

                    //POSTION
                    where.position = IntFunc.IntValue(Convert.ToString(dr["POSITION"]));

                    al.Add(where);
                }
            }
            if (ArrFunc.IsFilled(al))
                ret = (LstWhereData[])al.ToArray(typeof(LstWhereData));

            return ret;
        }

        /// <summary>
        /// Obtient les infos concernant les critères spécifiés sous LSTWHERE pour un referentiel
        /// </summary>
        /// FI 20140519 [19923] Modification de la signatute de la méthode add pIdAentity
        public LstWhereData[] GetInfoWhereFromReferential(string pCS, Referential pRR, int pIdAEntity)
        {
            LstWhereData[] ret = null;

            DataParameters dbParam = null;
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            //PL 20120924 Debug Use ALIAS
            //sqlSelect += "lw.IDLSTCOLUMN,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.ISMANDATORY,lw.POSITION" + Cst.CrLf;
            SQLSelect += "lw.IDLSTCOLUMN,lw.ALIAS,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.ISMANDATORY,lw.POSITION" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDLSTTEMPLATE=lw.IDLSTTEMPLATE and lt.IDLSTCONSULT=lw.IDLSTCONSULT and lt.IDA=lw.IDA)" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out dbParam) + Cst.CrLf;
            SQLSelect += SQLCst.AND + "(";
            SQLSelect += "(lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + SQLCst.AND + "lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += SQLCst.OR;
            SQLSelect += "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += ")" + Cst.CrLf;
            //PL20120329 Newness
            SQLSelect += SQLCst.AND + "((isnull(lw.IDLSTCOLUMN,-1)>=0) or (lw.COLUMNNAME is not null))";
            SQLSelect += SQLCst.ORDERBY + "lw.POSITION";
            IDataReader dr = null;
            try
            {
                ArrayList al = new ArrayList();

                QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

                while (dr.Read())
                {
                    LstWhereData where = new LstWhereData();

                    string displayCol = string.Empty;
                    int index = -1;

                    if (!(dr["IDLSTCOLUMN"] is DBNull))
                        index = Convert.ToInt32(dr["IDLSTCOLUMN"]);
                    string columnName = null;
                    if (!(dr["COLUMNNAME"] is DBNull))
                        columnName = Convert.ToString(dr["COLUMNNAME"]);
                    string previousColumnName = columnName;

                    ReferentialColumn rrc = null;
                    if (StrFunc.IsFilled(columnName))
                        rrc = pRR[columnName, Convert.ToString(dr["ALIAS"])];

                    if ((rrc == null) && (-1 < index))//Compatibilié asc: maintenu pour compatibilié ascendante (à supprimer en v2.7)
                        rrc = pRR.Column[index];
                    else
                        index = pRR.GetIndexCol(columnName);

                    if (rrc != null)//PL: Pour éviter un plantage dans le cas d'une incohérence dans les tables LST
                    {
                        if (StrFunc.IsEmpty(rrc.Ressource))
                        {
                            //Note: On est ici sur le cas d'une nième colonne "liée" (ie: Tenor)
                            int previousIndex = index;
                            while (previousIndex - 1 >= 0)
                            {
                                previousIndex--;
                                if (StrFunc.IsFilled(pRR.Column[previousIndex].Ressource))
                                {
                                    displayCol = Ressource.GetMulti(pRR.Column[previousIndex].Ressource, 1);
                                    break;
                                }
                            }
                            displayCol += "(" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                        }
                        else
                        {
                            displayCol = Ressource.GetMulti(rrc.Ressource, 1);
                            if (rrc.ExistsRelation)
                                displayCol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1, displayCol);

                            if ((index + 1 < pRR.Column.GetLength(0)) && StrFunc.IsEmpty(pRR.Column[index + 1].Ressource))
                                //Note: On est ici sur le cas de la 1ere colonne "liée" (ie: Tenor)
                                displayCol += "(" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                        }

                        //COLUMNNAME
                        where.columnIdentifier = displayCol;

                        //OPERATOR
                        //infosWhere.@operator = dr.GetValue(1).ToString();
                        where.@operator = Convert.ToString(dr["OPERATOR"]);

                        //LSTVALUE formatted
                        bool isResource = rrc.IsResourceSpecified && rrc.IsResource.IsResource;
                        if (isResource && rrc.IsResource.isCriteriaDisplaySpecified)
                            isResource &= rrc.IsResource.isCriteriaDisplay;
                        //
                        where.lstValue = FormatLstValue2(pCS, Convert.ToString(dr["LSTVALUE"]), rrc.DataType.value, true, isResource, false, pIdAEntity);

                        //ISMANDATORY
                        where.isMandatory = BoolFunc.IsTrue(Convert.ToString(dr["ISMANDATORY"]));

                        //POSITION
                        where.position = IntFunc.IntValue(Convert.ToString(dr["POSITION"]));

                        al.Add(where);
                    }
                }
                if (ArrFunc.IsFilled(al))
                    ret = (LstWhereData[])al.ToArray(typeof(LstWhereData));
            }
            catch { throw; }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRR"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public static string GetLabelReferential(Referential pRR, string pColumnName)
        {
            string displayCol = pColumnName;
            int previousIndex;
            //
            for (int index = 0; index < pRR.Column.Length; index++)
            {
                previousIndex = index;
                //
                ReferentialColumn rrc = pRR.Column[index];
                if (rrc.DataField.ToUpper() == pColumnName.ToUpper() ||
                    (StrFunc.IsFilled(rrc.IDForItemTemplateRelation) && rrc.IDForItemTemplateRelation.ToUpper() == pColumnName.ToUpper()))
                {
                    if (StrFunc.IsEmpty(rrc.Ressource))
                    {
                        //Note: On est ici sur le cas d'une nième colonne "liée" (ie: Tenor)
                        while (previousIndex - 1 >= 0)
                        {
                            previousIndex--;
                            if (StrFunc.IsFilled(pRR.Column[previousIndex].Ressource))
                            {
                                displayCol = Ressource.GetMulti(pRR.Column[previousIndex].Ressource, 1);
                                break;
                            }
                        }
                        displayCol += "(" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                    }
                    else
                    {
                        displayCol = Ressource.GetMulti(rrc.Ressource, 1);
                        if (rrc.ExistsRelation)
                            displayCol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1, displayCol);

                        if ((index + 1 < pRR.Column.GetLength(0)) && StrFunc.IsEmpty(pRR.Column[index + 1].Ressource))
                            //Note: On est ici sur le cas de la 1ere colonne "liée" (ie: Tenor)
                            displayCol += "(" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                    }
                    //
                    break;
                }
            }
            //
            return displayCol;
        }

        /// <summary>
        /// Formatage des données filtres
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pValue"></param>
        /// <param name="pDataType"></param>
        /// <param name="pIsFormatForDisplay">
        /// <para>si true  retourne la valeur stockée dans LSTWHERE formattée pour l'affichage</para>
        /// <para>si false retourne la valeur saisie au format ISO formattée pour le stockage dans LSTWHERE</para>
        /// </param>
        /// <param name="pIsResource"></param>
        /// <param name="pIsFormatForFormCriteria"></param>
        /// <returns></returns>
        /// FI 20140519 [19923] Modification signature de la fonction (la paramètre pidAEntity est nécessaire pour que cette methode n'utilise plus SessionTools
        public static string FormatLstValue2(string pCs, string pValue, string pDataType, bool pIsFormatForDisplay, bool pIsResource, bool pIsFormatForFormCriteria, int pidAEntity)
        {
            //
            string retValue = null;
            //
            //Lorsque l'utilisateur saisit blanc ou {null} 
            //Spheres® stocke NULL dans LSTWHERE affiche {null} au dessus du datagrid
            //
            //Lorsque l'utilisateur saisit null
            //Spheres® stocke 'null' dans LSTWHERE affiche null au dessus du datagrid
            //
            if (pIsFormatForDisplay && StrFunc.IsEmpty(pValue))
                retValue = "{null}";  //Affichage depuis les valeurs de LSTWHERE
            else if ((false == pIsFormatForDisplay) && ((pValue == "{null}") || StrFunc.IsEmpty(pValue)))
                retValue = null; //Stockage dans LSTWHERE
            //
            else
            {
                retValue = pValue.TrimEnd();
                //
                if ((TypeData.IsTypeDateOrDateTime(pDataType) || TypeData.IsTypeTime(pDataType)) && pValue != "null")
                {
                    #region IsTypeDateOrDateTime or IsTypeTime
                    bool isDateTimeFunction = (false == IsParseDateOk2(pDataType, retValue, pIsFormatForDisplay));
                    bool isConvertDate = !(isDateTimeFunction && pIsFormatForFormCriteria);
                    if (isConvertDate)
                    {
                        // La date est convertit au format ISO pour le stockage SQL
                        // La date est convertit au format CurrentCulture pour l'affichage
                        //
                        string defaultBusinessCenter = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");
                        DtFuncML dtFunc = new DtFuncML(pCs, defaultBusinessCenter, pidAEntity, 0, 0, null);
                        dtFunc.fourDigitReading = DtFunc.FourDigitReadingEnum.FourDigitHasYYYY;
                        //            
                        if (pIsFormatForDisplay)
                        {
                            // Formatage de la date au format CurrentCulture
                            if (TypeData.IsTypeDate(pDataType) || (TypeData.IsTypeDateTime(pDataType)))
                            {
                                if (TypeData.IsTypeDate(pDataType))
                                    retValue = dtFunc.GetDateString(pValue);
                                else
                                    retValue = dtFunc.GetDateTimeString(pValue);
                                //
                                //20090912 FI gestion de la saisie d'une année uniquement sur 2 ou 4 caractères
                                if ((pValue.Length == 4) && ((new DtFunc().StringToDateTime(retValue)).Year.ToString() == pValue))
                                    retValue = pValue;
                            }
                            else if (TypeData.IsTypeTime(pDataType))
                            {
                                retValue = dtFunc.GetShortTimeString(pValue);
                            }
                            else
                            {
                                throw new NotImplementedException("type DateTime not implemented");
                            }
                        }
                        else
                        {
                            // Formatage de la date au format ISO
                            if (TypeData.IsTypeDate(pDataType) || (TypeData.IsTypeDateTime(pDataType)))
                            {
                                if (TypeData.IsTypeDate(pDataType))
                                    retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODate);
                                else
                                    retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODateTime);
                                //
                                //20090912 FI gestion de la saisie d'une année uniquement sur 2 ou 4 caractères
                                if ((pValue.Length == 4) && ((new DtFunc().StringToDateTime(retValue)).Year.ToString() == pValue))
                                    retValue = pValue;
                            }
                            else if (TypeData.IsTypeTime(pDataType))
                            {
                                retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISOTime);
                            }
                            else
                            {
                                throw new NotImplementedException("type DateTime not implemented");
                            }
                        }
                    }
                    #endregion IsTypeDateOrDateTime
                }
                else if (pIsResource)
                {
                    #region Ressource
                    retValue = Ressource.GetString(pValue.TrimEnd(), true);
                    #endregion
                }
                else if (TypeData.IsTypeDec(pDataType) && pValue != "null")
                {
                    #region IsTypeDec
                    if (pIsFormatForDisplay)
                        retValue = StrFunc.FmtDecimalToCurrentCulture(pValue);
                    else
                        retValue = StrFunc.FmtDecimalToInvariantCulture2(pValue);
                    #endregion IsTypeDec
                }
            }
            return retValue;
        }

        /// <summary>
        /// Obtient les infos de clause order by sous forme de tableau 2D contenant n * (COL,ASCDESC)
        /// Fonction pour le cas d'une consultation (basée sur LSCONSULT et LSTxxx)
        /// </summary>
        /// <returns>ArrayList[]: tableau[n] de tableaux[3] contenant COL,OPERATOR,VALUE</returns>
        public ArrayList[] GetInfoOrderBy(string pCS)
        {
            DataParameters dbParam = null;
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "lob.ASCDESC,";
            SQLSelect += "lco.TABLENAME,lco.DISPLAYNAME," + Cst.CrLf;
            //SQLSelect += "lcaj.IDLSTJOIN as ALIASDISPLAYNAME" + Cst.CrLf;
            SQLSelect += "lcaj.IDLSTJOIN," + DataHelper.SQLIsNull(pCS, DataHelper.SQLIsNull(pCS, "lcajheader1.IDLSTJOIN", "lcajheader0.IDLSTJOIN"), "lcaj.IDLSTJOIN", "ALIASDISPLAYNAME") + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTORDERBY.ToString() + " lob" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + " lco on (lco.TABLENAME=lob.TABLENAME and lco.COLUMNNAME=lob.COLUMNNAME)" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcaj on(lcaj.IDLSTCONSULT=@IDLSTCONSULT and lcaj.ALIAS=lob.ALIAS ";
            SQLSelect += " and lcaj.POSITION = (" + SQLCst.SELECT + " max(tmp.POSITION) " + SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " tmp where (tmp.IDLSTCONSULT=lcaj.IDLSTCONSULT and tmp.ALIAS=lcaj.ALIAS)))";
            //------------------------------------------------
            // Recherche de l'item correspondant dans LSTCONSULTALIAS. 
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lca on(lca.IDLSTCONSULT=@IDLSTCONSULT and lca.ALIAS=lob.ALIAS)" + Cst.CrLf;
            // Recherche de l'éventuel item "header" correspondant dans LSTCONSULTALIAS, puis dans LSTCONSALIASJOIN en position 0 ou 1 (afin de ne pas engendrer de produit cartésien).
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTALIAS.ToString() + " lcaheader on(lcaheader.IDLSTCONSULT=@IDLSTCONSULT and lcaheader.ALIAS=lca.ALIASHEADER)" + Cst.CrLf;
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcajheader1 on(lcajheader1.IDLSTCONSULT=@IDLSTCONSULT and lcajheader1.ALIAS=lcaheader.ALIAS and lcajheader1.POSITION=1)" + Cst.CrLf;
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSALIASJOIN.ToString() + " lcajheader0 on(lcajheader0.IDLSTCONSULT=@IDLSTCONSULT and lcajheader0.ALIAS=lcaheader.ALIAS and lcajheader0.POSITION=0)" + Cst.CrLf;
            //------------------------------------------------
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lob", out dbParam);
            // RD 20111114 Pour afficher les critères de tri dans l'ordre            
            SQLSelect += SQLCst.ORDERBY + "lob.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            ArrayList[] alWhere = new ArrayList[2];
            alWhere[0] = new ArrayList();
            alWhere[1] = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    //COLUMNNAME                    
                    string joinName = dr["ALIASDISPLAYNAME"].ToString().Trim();
                    string displayName = dr["DISPLAYNAME"].ToString().Trim();

                    if (IsMultiTable(pCS) && (joinName != MainTableName(pCS)))
                    {
                        //alWhere[0].Add("[" + Ressource.GetMulti("TBL_" + joinName) + "] " + Ressource.GetMulti("COL_" + displayName));
                        string tableRessource = Ressource.GetMultiForDatagrid(IdLstConsult, dr["TABLENAME"].ToString(), "TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim(), 1);
                        string columnRessource = Ressource.GetMultiForDatagrid(IdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim(), 1);
                        alWhere[0].Add("[" + tableRessource + "] " + columnRessource);
                    }
                    else
                    {
                        alWhere[0].Add(Ressource.GetMulti("COL_" + displayName));
                    }
                    //ASCDESC
                    alWhere[1].Add(dr["ASCDESC"].ToString());
                }
            }
            catch { alWhere = null; }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            return alWhere;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public bool IsMultiTable(string pCS)
        {

            if (!m_isMultiTableSpecified)
                GetIsMultiTable(pCS);
            return (m_isMultiTable);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public string MainTableName(string pCS)
        {

            if (!m_isMultiTableSpecified)
                GetIsMultiTable(pCS);
            return (m_mainTableName);

        }

        /// <summary>
        /// Obtient une classe "Referential" créée à l'image du template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// FI 20141211 [20563] Nouvelle signature add parameter pDynamicDatas
        public Referentials GetReferentials(string pCS, string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs)
        {
            Referentials rMain = new Referentials();
            rMain.Items = new Referential[1] { new Referential() };

            if (StrFunc.IsFilled(_consultXML))
            {
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(Referentials), _consultXML);
                rMain = (Referentials)CacheSerializer.Deserialize(serializerInfo);
            }

            Referential rr = rMain.Items[0];
            rr.IsConsultation = true;

            // FI 20141211 [20563] Appel de la méthode SetDynamicArgs   
            if (null != pDynamicArgs)
                rr.SetDynamicArgs(pDynamicArgs);

            #region <Referencial>
            DataRow[] lstAliasDataRows = dtLstAlias.Select("ALIAS ='" + dtLstConsultAlias.Select("POSITION = 1")[0]["ALIAS"].ToString().Trim() + "'");

            rr.IsDynamicConsult = true;

            if (StrFunc.IsEmpty(rr.TableName))
                rr.TableName = lstAliasDataRows[0]["TABLENAME"].ToString().ToUpper();

            rr.AliasTableNameSpecified = true;
            rr.AliasTableName = dtLstConsultAlias.Select("POSITION = 1")[0]["ALIAS"].ToString().Trim();

            rr.Ressource = rr.TableName;
            rr.ColumnByRow = 0;

            if (false == rr.CreateSpecified)
            {
                rr.Create = false;
                rr.CreateSpecified = true;
            }
            if (false == rr.ModifySpecified)
            {
                rr.Modify = false;
                rr.ModifySpecified = true;
            }
            if (false == rr.RemoveSpecified)
            {
                rr.Remove = false;
                rr.RemoveSpecified = true;
            }
            if (false == rr.ImportSpecified)
            {
                rr.Import = false;
                rr.ImportSpecified = true;
            }

            rr.Export = false;
            rr.ExportSpecified = true;

            rr.Image = CSS.Main.loupe.ToString();

            rr.AliasTableNameSpecified = true;
            rr.AttachedDocSpecified = false;
            rr.ColumnByRowSpecified = true;
            rr.HelpUrlSpecified = false;
            rr.NotepadSpecified = false;
            rr.ToolBarSpecified = false;

            //FI 20111219 Appel à SetSqlCommandToReferential pour alimenter le sqlCommand source de la consultation
            if (!(lstAliasDataRows[0]["COMMAND"] is DBNull) || !(lstAliasDataRows[0]["TABLEDETXML"] is DBNull))
                SetReferentialSqlCommand(rr, pCondApp, pParam);

            //Initialisation
            rr.SQLOrderBySpecified = false;
            rr.SQLWhereSpecified = false;
            rr.SQLJoinSpecified = false;

            //OD Non géré pour l'instant (pas de table LSTGROUPBY)
            rr.SQLGroupBySpecified = false;

            SetReferentialSQLJoin(rr);

            SetReferentialColumn(rr);

            // FI 20141107 [20441] Appel à SetReferentialSQLWhere
            //SetReferentialSQLWhere(rr);
            SetReferentialSQLWhere2(rr);

            SetReferentialSQLOrder(rr);
            #endregion

            return rMain;
        }

        /// <summary>
        /// Ajoute les clauses Where d'un template à une classe referential
        /// </summary>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <param name="pIdA">int: ID du proprietaire du template</param>
        /// <param name="opReferentialsReferential">REF Referential: classe referential sur laquelle ajouter les clauses Where</param>
        public void AddLSTWHEREToReferential(string pCS, string pIdLstTemplate, int pIdA, ref Referential opReferentialsReferential)
        {
            ArrayList alObjects = new ArrayList();
            ArrayList alNewJoins = new ArrayList();
            //
            #region Initialize
            if (opReferentialsReferential.SQLWhereSpecified)
            {
                for (int index = 0; index < opReferentialsReferential.SQLWhere.Length; index++)
                {
                    alObjects.Add(((Array)opReferentialsReferential.SQLWhere).GetValue(index));
                }
            }
            //
            if (opReferentialsReferential.SQLJoinSpecified)
            {
                for (int index = 0; index < opReferentialsReferential.SQLJoin.Length; index++)
                {
                    alNewJoins.Add(opReferentialsReferential.SQLJoin[index]);
                }
            }
            #endregion
            //
            #region select LSTWHERE
            //string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN, lw.OPERATOR, lw.LSTVALUE, lw.LSTIDVALUE" + Cst.CrLf;
            //PL 20120924 Debug Use ALIAS
            //string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.LSTIDVALUE" + Cst.CrLf;
            string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN,lw.ALIAS,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.LSTIDVALUE" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDA=lw.IDA)";
            SQLSelect += SQLCst.AND + "(lt.IDLSTTEMPLATE=lw.IDLSTTEMPLATE)" + Cst.CrLf;
            SQLSelect += SQLCst.AND + "(lt.IDLSTCONSULT=lw.IDLSTCONSULT)" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("lw.IDA=" + pIdA.ToString());
            sqlWhere.Append("lw.IDLSTTEMPLATE=" + DataHelper.SQLString(pIdLstTemplate));
            sqlWhere.Append("lw.IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult));
            sqlWhere.Append("((lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + SQLCst.AND + "lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")" + SQLCst.OR + "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + "))");

            SQLSelect += sqlWhere.ToString();
            #endregion
            //
            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, SQLSelect);
            DataTable dt = ds.Tables[0];
            if (dt.Select().GetLength(0) > 0)
            {
                opReferentialsReferential.HasLstWhereClause = true;
                if (!opReferentialsReferential.SQLWhereSpecified)
                {
                    opReferentialsReferential.SQLWhere = new ReferentialSQLWhere[dt.Select().GetLength(0)];
                    opReferentialsReferential.SQLWhereSpecified = true;
                }
                int countRelation = 0;
                foreach (DataRow row in dt.Select())
                {
                    ReferentialColumn currentColumn = null;
                    //if (Convert.ToInt32(row["IDLSTCOLUMN"]) > -1)
                    if (StrFunc.IsFilled(Convert.ToString(row["COLUMNNAME"])))
                    {
                        currentColumn = opReferentialsReferential[Convert.ToString(row["COLUMNNAME"]), Convert.ToString(row["ALIAS"])];
                    }
                    else if (Convert.ToInt32(row["IDLSTCOLUMN"]) > -1)//Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                    {
                        currentColumn = opReferentialsReferential.Column[Convert.ToInt32(row["IDLSTCOLUMN"])];
                    }
                    if (currentColumn != null)
                    {
                        ReferentialSQLWhere rrSQLWhere = new ReferentialSQLWhere();
                        rrSQLWhere.AliasTableName = currentColumn.AliasTableName;
                        rrSQLWhere.AliasTableNameSpecified = true;
                        rrSQLWhere.ColumnName = currentColumn.ColumnName;
                        rrSQLWhere.ColumnNameSpecified = true;
                        rrSQLWhere.LstValue = FormatLstValue2(pCS, row["LSTVALUE"].ToString().Trim(), currentColumn.DataType.value, false, false, false, SessionTools.User.entity_IdA);
                        rrSQLWhere.LstValueSpecified = true;
                        rrSQLWhere.DataType = currentColumn.DataType;
                        rrSQLWhere.Operator = row["OPERATOR"].ToString().Trim();

                        if ((currentColumn.Relation != null)
                            && (currentColumn.Relation.Length > 0)
                            && (currentColumn.Relation[0].ColumnSelect != null)
                            && (currentColumn.Relation[0].ColumnSelect.Length > 0))
                        {
                            rrSQLWhere.AliasTableName = currentColumn.Relation[0].AliasTableName;
                            rrSQLWhere.AliasTableNameSpecified = true;
                            rrSQLWhere.ColumnName = currentColumn.Relation[0].ColumnSelect[0].ColumnName;
                            rrSQLWhere.ColumnNameSpecified = true;
                            rrSQLWhere.DataType = new ReferentialColumnDataType();
                            rrSQLWhere.DataType.value = currentColumn.Relation[0].ColumnSelect[0].DataType;
                        }
                        alObjects.Add(rrSQLWhere);
                    }
                }
                System.Type type = ((System.Array)opReferentialsReferential.SQLWhere).GetType().GetElementType();
                opReferentialsReferential.SQLWhere = (ReferentialSQLWhere[])alObjects.ToArray(type);

                if (countRelation > 0)
                {
                    opReferentialsReferential.SQLJoin = new string[alNewJoins.Count];
                    opReferentialsReferential.SQLJoinSpecified = true;
                    for (int i = 0; i < alNewJoins.Count; i++)
                        opReferentialsReferential.SQLJoin[i] = alNewJoins[i].ToString();
                }
            }
        }

        private string CreateOverwrite(string pCS, LstTemplateData pTemplate)
        {
            return CreateOverwrite(pCS, pTemplate, SessionTools.Collaborator_IDA, null, null, null);
        }
        private string CreateOverwrite(string pCS, LstTemplateData pTemplate, int pIdA)
        {
            return CreateOverwrite(pCS, pTemplate, pIdA, null, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstTemplate"></param>
        /// <param name="pValueForFilter"></param>
        /// <param name="pIndexForFilter"></param>
        /// <returns></returns>
        private string CreateOverwrite(string pCS, LstTemplateData pTemplate, string pValueForFilter, string pTableForFilter, string pColumnForFilter)
        {
            return CreateOverwrite(pCS, pTemplate, SessionTools.Collaborator_IDA, pValueForFilter, pTableForFilter, pColumnForFilter);
        }
        /// <summary>
        /// Fonction principale de création de template à l'aide de la classe LSTTEMPLATE
        /// </summary>
        /// <param name="pNewLSTTEMPLATE">LSTTEMPLATE: classe LSTTEMPLATE contenant les informations du template que l'on veut créer</param>
        /// <param name="pForcedIdA"></param>
        /// <returns>string: ID du template créé</returns>
        private string CreateOverwrite(string pCS, LstTemplateData pTemplate, int pIdA, string pValueForFilter, string pTableForFilter, string pColumnForFilter)//GLOP-DEL
        {
            //Si existe deja, on delete
            if (RepositoryWeb.ExistsTemplate(pCS, IdLstConsult, pTemplate.IDLSTTEMPLATE, pIdA))
            {
                //20091012 FI utilisation de la méthode static LstTemplate.Delete
                //Pourquoi faire un load pour delete ensuite ?
                //LstTemplate oldLSTTEMPLATE = new LstTemplate();
                //oldLSTTEMPLATE.Load(pCS, IdLstConsult, pLstTemplate.IDLSTTEMPLATE, forcedIdA);
                //oldLSTTEMPLATE.Delete(pCS, false);
                RepositoryWeb.Delete(pCS, IdLstConsult, pTemplate.IDLSTTEMPLATE, pIdA, false);
            }
            //Insert
            pTemplate.Insert(pCS, pIdA);
            //
            #region Insert into LSTWHERE
            //20070910 PL
            //Rq. Utilisé pour insérer un critère dynamique sur l'ouverture d'un référentiel en tant qu'aide à la saisie (eg. bouton "..." sur la saisie des trades)
            //20070917 PL Add du If() pour corriger un bug
            if (pValueForFilter != null)//Warning: Ne pas tester IsEmpty() mais bien "null", car la donnée "empty" est possible
            {
                string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTWHERE.ToString();
                //sqlQuery += "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,IDLSTCOLUMN,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY)" + Cst.CrLf;
                sqlQuery += "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY)" + Cst.CrLf;
                sqlQuery += " values (";
                sqlQuery += pIdA.ToString() + ",";
                sqlQuery += DataHelper.SQLString(IdLstConsult) + ",";
                sqlQuery += DataHelper.SQLString(pTemplate.IDLSTTEMPLATE) + ",";
                //20090626 PL TODO
                //if (null != pIndexForFilter) //=> on vient du référentiel
                //PL 20120529
                //if (StrFunc.IsFilled(pColumnForFilter)) //=> on vient du référentiel
                if (StrFunc.IsFilled(pTableForFilter)) //=> on vient du référentiel
                {
                    //sqlQuery += pIndexForFilter.ToString() + "," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
                    //if (pColumnForFilter == "GLOPL_TBD")//Compatibilié asc: pIndexForFilter maintenu pour compatibilié ascendante (à supprimer en v2.7)
                    //{
                    //    //sqlQuery += pIndexForFilter.ToString() + ",null,null," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
                    //}
                    //else
                    //{
                    //PL: Il faudra ici identifier les bons TABLENAME et COLUMNNAME... (TBD)
                    //PL 20120523
                    //sqlQuery += "null," + DataHelper.SQLString(SQLCst.TBLMAIN) + "," + DataHelper.SQLString(pColumnForFilter) + "," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
                    sqlQuery += "null," + DataHelper.SQLString(pTableForFilter) + "," + DataHelper.SQLString(pColumnForFilter) + "," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
                    //}
                }
                else
                {
                    // PM 20091113 : Modif de ce qui a été fait en dur afin de gérer la consultation des Assets ETD
                    // Remplacement du nom de la table, de son alias et de nom de la colonne 'identifier' par des variables
                    //string tableAlias = null; 
                    //string tableName = null;
                    string tableAlias = SQLCst.TBLMAIN;
                    string tableName = SQLCst.TBLMAIN;
                    string identifierName = "IDENTIFIER";
                    if ("*Select" == pTemplate.IDLSTTEMPLATE)
                    {
                        if ("ASSET_ETD_EXPANDED" == pTemplate.IDLSTCONSULT)
                        {
                            // FI 20180130 [23749] l'alias de la table est a_etd (avant c'était vw_a_etd) 
                            tableAlias = "a_etd";
                            tableName = "VW_ASSET_ETD_EXPANDED";
                        }
                        else if ("MATURITY" == pTemplate.IDLSTCONSULT)
                        {
                            tableAlias = "mat_etd";
                            tableName = "MATURITY";
                            identifierName = "MATURITYMONTHYEAR";
                            pColumnForFilter = "MATURITYMONTHYEAR";//GLOPL ??
                        }
                        else
                        {
                            //CC/PL 20120529 
                            //tableAlias = "vw_tasset";
                            tableAlias = "tasset";
                            tableName = "VW_TRADE_ASSET";
                        }
                    }
                    //20090626 PL TODO en dur pour l'instant...(IDLSTCOLUMN et "t")
                    // EG 20160308 Migration vs2013
                    //#warning 20090626 PL TODO
                    #region Get IDLSTCOLUMN
                    //string tmpSQLQuery = SQLCst.SELECT + "IDLSTCOLUMN" + Cst.CrLf;
                    //tmpSQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + Cst.CrLf;
                    //tmpSQLQuery += SQLCst.WHERE + "TABLENAME='" + tableName + "' and COLUMNNAME='" + identifierName + "'";
                    ////IDataReader dr = DataHelper.ExecuteReader(source, CommandType.Text, tmpSQLQuery);
                    ////if (dr.Read())
                    ////    pIndexForFilter = Convert.ToInt32(dr.GetValue(0));
                    ////dr.Close();
                    ////dr.Dispose();
                    //object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, tmpSQLQuery);
                    //if (null != obj)
                    //    pIndexForFilter = Convert.ToInt32(obj);
                    #endregion Get IDLSTCOLUMN
                    //sqlQuery += pIndexForFilter.ToString() + "," + DataHelper.SQLString(tableAlias) + ",";
                    sqlQuery += "-1," + DataHelper.SQLString(tableName) + "," + DataHelper.SQLString(identifierName) + "," + DataHelper.SQLString(tableAlias) + ",";//GLOPL??
                }
                sqlQuery += "0,";
                //FI 20111202 Mise en place de l'opérateur Contains à la place de like
                if (pValueForFilter == "%")
                    sqlQuery += DataHelper.SQLString(SQLCst.LIKE.Trim()) + ",";
                else
                    sqlQuery += DataHelper.SQLString("Contains") + ",";

                sqlQuery += "@LSTVALUE,"; //Note: Utilisation par sécurité d'un parameter car donnée saisie par le user
                sqlQuery += SQLCst.NULL + ",";
                sqlQuery += "1,";
                sqlQuery += "0)";
                //
                IDbDataParameter paramLSTVALUE = new EFSParameter(pCS, "LSTVALUE", DbType.AnsiString, SQLCst.UT_NOTE_LEN).DataParameter;
                paramLSTVALUE.Value = pValueForFilter;

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlQuery, paramLSTVALUE);
            }
            #endregion Insert into LSTWHERE

            return pTemplate.IDLSTTEMPLATE;
        }

        /// <summary>
        /// Pour l'affichage d'une seule ressource (Header) pour plusieurs colonnes.
        /// Mettre à jour les ressources de la colonne en cours et eventuellement des colonnes précédentes,
        /// si la colonne en cours est une colonne de référence pour les ressources
        /// ou bien la colonne en cours dépond d'une éventuelle colonne de référence précédente
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pCurrentColumnIndex"></param>
        /// <param name="pIsFound_ColumnRef"></param>
        private void SetColumnRessourceFromColumnRef(Referential pReferential, int pCurrentColumnIndex, ref bool pIsFound_ColumnRef)
        {

            bool isColumnRefSpecified = StrFunc.IsFilled(pReferential.Column[pCurrentColumnIndex].ColumnRef);
            int lastIndice = pCurrentColumnIndex - 1;
            //
            // La colonne d'avant est la colonne avec ressource de référence à afficher
            bool isLastColumn_ColumnRef = (lastIndice >= 0) && isColumnRefSpecified &&
                (pReferential.Column[lastIndice].AliasTableName == pReferential.Column[pCurrentColumnIndex].AliasTableName) &&
                (pReferential.Column[lastIndice].ColumnName == pReferential.Column[pCurrentColumnIndex].ColumnRef);
            //
            // La colonne d'avant a la même référence que la colonne courante
            bool isLastColumn_WithSameReference = (lastIndice >= 0) && isColumnRefSpecified &&
                (pReferential.Column[lastIndice].AliasTableName == pReferential.Column[pCurrentColumnIndex].AliasTableName) &&
                (pReferential.Column[lastIndice].ColumnRef == pReferential.Column[pCurrentColumnIndex].ColumnRef);
            //
            // La colonne courante est la colonne avec ressource de référence à afficher
            bool isColumn_ColumnRef = (lastIndice >= 0) &&
                (pReferential.Column[lastIndice].AliasTableName == pReferential.Column[pCurrentColumnIndex].AliasTableName) &&
                (pReferential.Column[lastIndice].ColumnRef == pReferential.Column[pCurrentColumnIndex].ColumnName);
            //                    
            if (isLastColumn_ColumnRef)
                pIsFound_ColumnRef = true;
            //
            // Pour mettre Ressource à vide
            // pour toutes les colonnes qui viennet "après" la colonne de ressource de référence
            if (isLastColumn_ColumnRef || (isLastColumn_WithSameReference && pIsFound_ColumnRef))
            {
                pReferential.Column[pCurrentColumnIndex].Ressource = string.Empty;
                pReferential.Column[pCurrentColumnIndex].RessourceSpecified = true;
            }
            else
            {
                pIsFound_ColumnRef = false;
            }
            //
            // Pour mettre Ressource à vide pour toutes les colonnes qui viennet "avant" la colonne de ressource de référence
            // sauf pour la première colonne: mettre la ressource de la colonne avec ressource de référence à afficher
            while (isColumn_ColumnRef)
            {
                pReferential.Column[lastIndice].Ressource = pReferential.Column[lastIndice + 1].Ressource;
                pReferential.Column[lastIndice].RessourceSpecified = true;
                pReferential.Column[lastIndice + 1].Ressource = string.Empty;
                pReferential.Column[lastIndice + 1].RessourceSpecified = true;
                lastIndice--;
                //
                isColumn_ColumnRef = (lastIndice >= 0) &&
                (pReferential.Column[lastIndice].AliasTableName == pReferential.Column[pCurrentColumnIndex].AliasTableName) &&
                (pReferential.Column[lastIndice].ColumnRef == pReferential.Column[pCurrentColumnIndex].ColumnName);
            }

        }

        /// <summary>
        /// Initilalise la command SQL de base du Referential
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// FI 20111219 Add SetSqlCommandToReferential 
        /// FI 20141211 [20563] add parameter pCondApp
        private void SetReferentialSqlCommand(Referential pReferential, string pCondApp, string[] pParam)
        {
            DataRow[] lstAliasDataRows = dtLstAlias.Select("ALIAS ='" + pReferential.AliasTableName + "'");
            DataRow lstAliasDataRow = lstAliasDataRows[0];
            //
            //Lescture de la colonne TABLEDETXML si la colonne COMMAND est non renseignée
            if (!(lstAliasDataRows[0]["TABLEDETXML"] is DBNull) & (lstAliasDataRows[0]["COMMAND"] is DBNull))
            {
                string xmlExpression = lstAliasDataRow["TABLEDETXML"].ToString();
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(ReferentialSQLSelectOrResource), xmlExpression);
                ReferentialSQLSelectOrResource sqlSelectOrResource = (ReferentialSQLSelectOrResource)CacheSerializer.Deserialize(serializerInfo);

                if (sqlSelectOrResource.SQLSelectResourceSpecified)
                {
                    pReferential.SQLSelectResourceSpecified = true;
                    pReferential.SQLSelectResource = sqlSelectOrResource.SQLSelectResource;
                }
                // si SQLSelect est renseigné alors il override ce qui est éventuellement renseigné dans SQLSelectResource
                if (sqlSelectOrResource.SQLSelectSpecified)
                {
                    pReferential.SQLSelectSpecified = true;
                    pReferential.SQLSelect = sqlSelectOrResource.SQLSelect;
                }
            }
            else
            {
                pReferential.SQLSelect = new ReferentialSQLSelect[1];
                pReferential.SQLSelect[0] = new ReferentialSQLSelect();
                pReferential.SQLSelectSpecified = true;
                pReferential.SQLSelect[0].Command = new ReferentialSQLSelectCommand[1];
                pReferential.SQLSelect[0].Command[0] = new ReferentialSQLSelectCommand();
                pReferential.SQLSelect[0].Command[0].rdbms = lstAliasDataRows[0]["RDBMS"].ToString();
                pReferential.SQLSelect[0].Command[0].Value = lstAliasDataRows[0]["COMMAND"].ToString();
            }

            //Appel à InitializeSQLSelectCommand puisque cette méthode peut interpréter les tags %%SR:
            pReferential.InitializeSQLSelectCommand(pParam, pCondApp);
        }

        /// <summary>
        /// Affecte le membre SQLWhere de Referential
        /// </summary>
        /// <param name="rr"></param>
        [Obsolete("Use SetReferentialSQLWhere2 instead of SetReferentialSQLWhere", true)]
        private void SetReferentialSQLWhere(Referential rr)
        {
            bool isAddTradeRestriction = false;

            isAddTradeRestriction = //IsConsultation(ConsultEnum.TRADEFnO_ALLOC) ||
                                    IsConsultation(ConsultEnum.TRADEFnO_ORDER_BLOCK);

            isAddTradeRestriction |= IsConsultation(ConsultEnum.TRADEOTC);
            isAddTradeRestriction |= IsConsultation(ConsultEnum.TRADE_PARTY);
            isAddTradeRestriction |= IsConsultation(ConsultEnum.EVENT_STL);
            isAddTradeRestriction |= IsConsultation(ConsultEnum.FLOWSBYTRADE_ALLOC);
            isAddTradeRestriction |= IsConsultation(ConsultEnum.POSACTIONDET);
            //
            bool isAddTradeAdminRestriction = false;
            isAddTradeAdminRestriction = IsConsultation(ConsultEnum.TRADEADMIN);

            bool isAddTradeDebtSecRestriction = false;
            isAddTradeDebtSecRestriction = IsConsultation(ConsultEnum.DEBTSECURITY);

            bool isAddAssetETDRestriction = false;
            isAddAssetETDRestriction = IsConsultation(ConsultEnum.ASSET_ETD_EXPANDED);

            //
            bool isAddRestriction = isAddTradeRestriction ||
                                    isAddTradeAdminRestriction ||
                                    isAddTradeDebtSecRestriction ||
                                    isAddAssetETDRestriction;
            //
            rr.SQLWhere = null;
            rr.SQLWhereSpecified = false;
            int nbSQLWhere = (isAddRestriction) ? 1 : 0;
            int indice = 0;

            ArrayList sqlWhereArrayList = new ArrayList();
            if (ArrFunc.IsFilled(rr.SQLWhere))
                sqlWhereArrayList = new ArrayList(rr.SQLWhere);



            nbSQLWhere += dtLstWhere.Rows.Count;
            rr.SQLWhere = new ReferentialSQLWhere[nbSQLWhere];
            foreach (DataRow row in dtLstWhere.Rows)
            {
                ReferentialSQLWhere sqlWhere = new ReferentialSQLWhere();

                //20070321 PL 
                if (template.ISENABLEDLSTWHERE || Convert.ToBoolean(row["ISMANDATORY"]))
                {
                    rr.HasLstWhereClause = true;
                    rr.SQLWhereSpecified = true;

                    sqlWhere.AliasTableName = row["ALIAS"].ToString().Trim();
                    sqlWhere.AliasTableNameSpecified = true;

                    sqlWhere.ColumnName = row["COLUMNNAME"].ToString().Trim();
                    sqlWhere.ColumnNameSpecified = StrFunc.IsFilled(rr.SQLWhere[indice].ColumnName);

                    string dataType = row["DATATYPE"].ToString().Trim();
                    sqlWhere.LstValue = FormatLstValue2(SessionTools.CS, row["LSTVALUE"].ToString().Trim(), dataType, false, false, false, SessionTools.User.entity_IdA);
                    sqlWhere.LstValueSpecified = true;
                    sqlWhere.DataType = new ReferentialColumnDataType();
                    sqlWhere.DataType.value = dataType;
                    sqlWhere.Operator = row["OPERATOR"].ToString().Trim();

                    ReferentialColumn rrc = rr[sqlWhere.ColumnName, sqlWhere.AliasTableName];
                    if (null == rrc || (false == rrc.ColumnSqlWhereSpecified) && (false == (row["SQLWHERE"] is DBNull)))
                    {
                        sqlWhere.ColumnSQLWhere = row["SQLWHERE"].ToString();
                        sqlWhere.ColumnSQLWhereSpecified = StrFunc.IsFilled(rr.SQLWhere[indice].ColumnSQLWhere);
                    }

                    // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                    if (null == rrc)
                    {
                        string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                        if (StrFunc.IsFilled(sqlSelect))
                            sqlWhere.ColumnNameOrColumnSQLSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, sqlWhere.AliasTableName);
                        else
                            sqlWhere.ColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameExpression(sqlWhere.ColumnName, sqlWhere.AliasTableName);
                        //
                        sqlWhere.ColumnNameOrColumnSQLSelectSpecified = StrFunc.IsFilled(sqlWhere.ColumnNameOrColumnSQLSelect);
                    }
                    sqlWhereArrayList.Add(sqlWhere);
                }
            }

            if (sqlWhereArrayList.Count > 0)
                rr.SQLWhere = (ReferentialSQLWhere[])sqlWhereArrayList.ToArray(typeof(ReferentialSQLWhere));

            //
            //un peu de CleanUp
            for (int i = 0; i < ArrFunc.Count(rr.SQLWhere); i++)
            {
                if (null == rr.SQLWhere[i])
                    ReflectionTools.RemoveItemInArray(rr, "SQLWhere", i);
            }
            //
            if ((!SessionTools.IsSessionSysAdmin) && isAddRestriction)
            {
                if (isAddTradeRestriction)
                {
                    #region isAddTradeRestriction
                    ReflectionTools.AddItemInArray(rr, "SQLWhere", 0);
                    rr.SQLWhere[indice] = new ReferentialSQLWhere();
                    //
                    rr.SQLWhere[indice].ConditionSystemSpecified = true;
                    rr.SQLWhere[indice].ConditionSystem = Cst.SESSIONRESTRICT;
                    //
                    rr.SQLWhere[indice].SQLJoinSpecified = true;
                    rr.SQLWhere[indice].SQLJoin = new string[1] { Cst.SR_TRADE_JOIN + "(" + rr.AliasTableName + ".IDT" + ")" };

                    rr.SQLWhere[indice].SQLWhereSpecified = true;
                    rr.SQLWhere[indice].SQLWhere = Cst.SR_TRADE_WHERE_PREDICATE;
                    #endregion isAddTradeRestriction
                }
                else if (isAddTradeAdminRestriction)
                {
                    #region isAddTradeAdmin
                    ReflectionTools.AddItemInArray(rr, "SQLWhere", 0);
                    rr.SQLWhere[indice] = new ReferentialSQLWhere();
                    //
                    rr.SQLWhere[indice].ConditionSystemSpecified = true;
                    rr.SQLWhere[indice].ConditionSystem = Cst.SESSIONRESTRICT;
                    //
                    rr.SQLWhere[indice].SQLJoinSpecified = true;
                    rr.SQLWhere[indice].SQLJoin = new string[1] { Cst.SR_TRADEADMIN_JOIN + "(" + rr.AliasTableName + ".IDT" + ")" };

                    rr.SQLWhere[indice].SQLWhereSpecified = true;
                    rr.SQLWhere[indice].SQLWhere = Cst.SR_TRADEADMIN_WHERE_PREDICATE;
                    #endregion isAddTradeAdmin
                }
                else if (isAddTradeDebtSecRestriction)
                {

                    #region isAddTradeDebtSec
                    ReflectionTools.AddItemInArray(rr, "SQLWhere", 0);
                    rr.SQLWhere[indice] = new ReferentialSQLWhere();
                    //
                    rr.SQLWhere[indice].ConditionSystemSpecified = true;
                    rr.SQLWhere[indice].ConditionSystem = Cst.SESSIONRESTRICT;
                    //
                    rr.SQLWhere[indice].SQLJoinSpecified = true;
                    rr.SQLWhere[indice].SQLJoin = new string[1] { Cst.SR_TRADEDEBTSEC_JOIN + "(" + rr.AliasTableName + ".IDT" + ")" };

                    rr.SQLWhere[indice].SQLWhereSpecified = true;
                    rr.SQLWhere[indice].SQLWhere = Cst.SR_TRADEDEBTSEC_WHERE_PREDICATE;
                    #endregion isAddTradeAdmin
                }
                else if (isAddAssetETDRestriction)
                {
                    #region isAssetEtdExpanded
                    string aliasTblTrade = rr.AliasTableName;
                    //
                    ReflectionTools.AddItemInArray(rr, "SQLWhere", 0);
                    rr.SQLWhere[indice] = new ReferentialSQLWhere();
                    //
                    rr.SQLWhere[indice].ConditionSystemSpecified = true;
                    rr.SQLWhere[indice].ConditionSystem = Cst.SESSIONRESTRICT;
                    //
                    rr.SQLWhere[indice].SQLJoinSpecified = true;
                    rr.SQLWhere[indice].SQLJoin = new string[2] 
                    { 
                        Cst.SR_INSTR_JOIN + "("+ aliasTblTrade + ".IDI"+")",
                        Cst.SR_MARKET_JOIN + "("+ aliasTblTrade + ".IDM"+")"
                    };
                    //
                    rr.SQLWhere[indice].SQLWhereSpecified = false;
                    #endregion isAssetEtdExpanded
                }
            }
            rr.SQLWhereSpecified = ArrFunc.IsFilled(rr.SQLWhere);
        }

        /// <summary>
        /// Affecte le membre SQLWhere de Referential
        /// </summary>
        /// FI 20141107 [20441] Add (Les codages en dur sur SESSIONRESTRICT on été supprimés, ils sont désormais présents dans la colonne LSTCONSULT.CONSULTXML)
        private void SetReferentialSQLWhere2(Referential rr)
        {
            ArrayList sqlWhereArrayList = new ArrayList();
            if (ArrFunc.IsFilled(rr.SQLWhere))
                sqlWhereArrayList = new ArrayList(rr.SQLWhere);

            foreach (DataRow row in dtLstWhere.Rows)
            {
                ReferentialSQLWhere sqlWhere = new ReferentialSQLWhere();

                //20070321 PL 
                if (template.ISENABLEDLSTWHERE || Convert.ToBoolean(row["ISMANDATORY"]))
                {
                    rr.HasLstWhereClause = true;
                    rr.SQLWhereSpecified = true;

                    sqlWhere.AliasTableName = row["ALIAS"].ToString().Trim();
                    sqlWhere.AliasTableNameSpecified = true;

                    sqlWhere.ColumnName = row["COLUMNNAME"].ToString().Trim();
                    sqlWhere.ColumnNameSpecified = StrFunc.IsFilled(sqlWhere.ColumnName);

                    string dataType = row["DATATYPE"].ToString().Trim();
                    sqlWhere.LstValue = FormatLstValue2(SessionTools.CS, row["LSTVALUE"].ToString().Trim(), dataType, false, false, false, SessionTools.User.entity_IdA);
                    sqlWhere.LstValueSpecified = true;
                    sqlWhere.DataType = new ReferentialColumnDataType();
                    sqlWhere.DataType.value = dataType;
                    sqlWhere.Operator = row["OPERATOR"].ToString().Trim();

                    ReferentialColumn rrc = rr[sqlWhere.ColumnName, sqlWhere.AliasTableName];
                    if (null == rrc || (false == rrc.ColumnSqlWhereSpecified) && (false == (row["SQLWHERE"] is DBNull)))
                    {
                        sqlWhere.ColumnSQLWhere = row["SQLWHERE"].ToString();
                        sqlWhere.ColumnSQLWhereSpecified = StrFunc.IsFilled(sqlWhere.ColumnSQLWhere);
                    }

                    // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                    if (null == rrc)
                    {
                        string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                        if (StrFunc.IsFilled(sqlSelect))
                            sqlWhere.ColumnNameOrColumnSQLSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, sqlWhere.AliasTableName);
                        else
                            sqlWhere.ColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameExpression(sqlWhere.ColumnName, sqlWhere.AliasTableName);
                        //
                        sqlWhere.ColumnNameOrColumnSQLSelectSpecified = StrFunc.IsFilled(sqlWhere.ColumnNameOrColumnSQLSelect);
                    }
                    sqlWhereArrayList.Add(sqlWhere);
                }
            }

            if (sqlWhereArrayList.Count > 0)
                rr.SQLWhere = (ReferentialSQLWhere[])sqlWhereArrayList.ToArray(typeof(ReferentialSQLWhere));

            //
            //un peu de CleanUp
            for (int i = 0; i < ArrFunc.Count(rr.SQLWhere); i++)
            {
                if (null == rr.SQLWhere[i])
                    ReflectionTools.RemoveItemInArray(rr, "SQLWhere", i);
            }

            rr.SQLWhereSpecified = ArrFunc.IsFilled(rr.SQLWhere);
        }

        /// <summary>
        /// Affecte le membre SQLOrderBy de Referential
        /// </summary>
        /// <param name="rr"></param>
        private void SetReferentialSQLOrder(Referential rr)
        {
            int indice = 0;
            rr.SQLOrderBy = new ReferentialSQLOrderBy[dtLstOrderBy.Rows.Count];
            //
            bool isReferentialWithGroupBy = false;
            for (int i = 0; i < ArrFunc.Count(rr.Column); i++)
            {
                if ((rr.Column[i].GroupBy != null) && rr.Column[i].GroupBy.IsGroupBy)
                    isReferentialWithGroupBy = true;
            }
            //
            foreach (DataRow row in dtLstOrderBy.Rows)
            {
                string alias = row["ALIAS"].ToString().Trim();
                string column = row["COLUMNNAME"].ToString().Trim();
                string sqlorder = row["SQLORDER"].ToString().Trim();
                string ascdesc = row["ASCDESC"].ToString().Trim();
                //
                string columnName = alias + "." + column;
                string columnAlias = alias + "_" + column;
                //
                rr.SQLOrderBy[indice] = new ReferentialSQLOrderBy();
                //
                rr.SQLOrderBy[indice].ColumnName = column;
                rr.SQLOrderBy[indice].Alias = alias;
                rr.SQLOrderBy[indice].DataType = row["DATATYPE"].ToString().Trim();
                //
                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                // Lignes Déplacées ci-dessous
                if (null == rr[column, alias])
                {
                    rr.SQLOrderBy[indice].ColumnNotInReferential = true;
                    //
                    #region GroupBy
                    rr.SQLOrderBy[indice].GroupBy = new ReferentialColumnGroupBy();
                    rr.SQLOrderBy[indice].GroupBySpecified = true;
                    rr.SQLOrderBy[indice].GroupBy.IsGroupBy = (false == (row["ISGROUPBY"] is DBNull ||
                                                                BoolFunc.IsFalse(row["ISGROUPBY"])));

                    // MF 20120430 ruptures with groupingset
                    rr.SQLOrderBy[indice].GroupBy.GroupingSet = Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET");
                    //
                    if (rr.SQLOrderBy[indice].GroupBy.IsGroupBy)
                        isReferentialWithGroupBy = true;
                    //
                    if (row["AGGREGATE"] is DBNull)
                        rr.SQLOrderBy[indice].GroupBy.AggregateSpecified = false;
                    else
                    {
                        rr.SQLOrderBy[indice].GroupBy.Aggregate = Convert.ToString(row["AGGREGATE"]);
                        rr.SQLOrderBy[indice].GroupBy.AggregateSpecified = StrFunc.IsFilled(rr.SQLOrderBy[indice].GroupBy.Aggregate);
                    }
                    //
                    if (row["SQLGROUPBY"] is DBNull)
                        rr.SQLOrderBy[indice].GroupBy.SqlGroupBySpecified = false;
                    else
                    {
                        rr.SQLOrderBy[indice].GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]);
                        rr.SQLOrderBy[indice].GroupBy.SqlGroupBySpecified = StrFunc.IsFilled(rr.SQLOrderBy[indice].GroupBy.SqlGroupBy);
                    }
                    //
                    // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                    string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                    if (StrFunc.IsFilled(sqlSelect))
                        rr.SQLOrderBy[indice].ColumnNameOrColumnSQLSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, rr.SQLOrderBy[indice].Alias);
                    else
                        rr.SQLOrderBy[indice].ColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameExpression(rr.SQLOrderBy[indice].ColumnName, rr.SQLOrderBy[indice].Alias);
                    //
                    rr.SQLOrderBy[indice].ColumnNameOrColumnSQLSelectSpecified = StrFunc.IsFilled(rr.SQLOrderBy[indice].ColumnNameOrColumnSQLSelect);
                    #endregion
                }
                //
                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                string tempColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameOrColumnSelect(rr, rr.SQLOrderBy[indice]);
                //
                rr.SQLOrderBy[indice].Value = RepositoryTools.ConstructColumnOrderBy(alias, tempColumnNameOrColumnSQLSelect, sqlorder, false) + " " + ascdesc;
                rr.SQLOrderBy[indice].ValueWithAlias = RepositoryTools.ConstructColumnOrderBy(alias, columnName, sqlorder, true) + " " + ascdesc;
                //
                indice++;
            }
            //
            rr.SQLOrderBySpecified = ArrFunc.IsFilled(rr.SQLOrderBy);
            //
            if (rr.SQLOrderBySpecified)
            {
                for (int i = 0; i < rr.SQLOrderBy.Length; i++)
                {
                    ReferentialSQLOrderBy rr_SQLOrderBy = rr.SQLOrderBy[i];
                    //
                    // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                    string columnAlias = rr_SQLOrderBy.Alias + "_" + rr_SQLOrderBy.ColumnName;
                    string columnNameOrColumnSelect = SQLReferentialData.GetColumnNameOrColumnSelect(rr, rr_SQLOrderBy);
                    //
                    string sqlSortInGroupBy = string.Empty;
                    if (isReferentialWithGroupBy)
                    {
                        sqlSortInGroupBy = SQLReferentialData.GetColumnSortInGroupBy(columnNameOrColumnSelect, columnAlias,
                        ref rr_SQLOrderBy.GroupBySort, ref rr_SQLOrderBy.GroupBySortWithAlias);
                    }
                    //
                    rr_SQLOrderBy.Value = sqlSortInGroupBy + rr_SQLOrderBy.Value;
                    rr_SQLOrderBy.ValueWithAlias = sqlSortInGroupBy + rr_SQLOrderBy.ValueWithAlias;
                }
            }
        }

        /// <summary>
        /// Affecte le membre Column de Referential
        /// </summary>
        private void SetReferentialColumn(Referential rr)
        {
            ArrayList listColumnAlias = new ArrayList();

            #region Ajout de colonne en DUR (DataKeyField, [ForeignKeyField], [OrderBy])
            bool existsDataKeyField = true;
            bool existsForeignKeyField = true;
            ReferentialColumn rrcDataKeyField = new ReferentialColumn();
            ReferentialColumn rrcForeignKeyField = new ReferentialColumn();
            ReferentialColumn rrcOrderBy = null;

            string table_DataKeyField = null;
            string table_ForeignKeyField = null;
            string table_AliasTableName = rr.AliasTableName;
            string table_OrderBy = null;
            //
            //Note: Spécificité liée à la table TRADE (pour l'ouverture du formulaire) ou aux POSITIONS Synthétiques (pour l'ouverture des positions détaillées)
            if (rr.TableName == Cst.OTCml_TBL.TRADE.ToString() ||
                rr.TableName == Cst.OTCml_TBL.VW_TRADE_ASSET.ToString())
            {
                table_DataKeyField = "IDT";
                table_ForeignKeyField = "IDI";
            }
            else if ((rr.TableName == "POSSYNT") || (rr.TableName == "POSSYNTOTC"))
            {
                table_DataKeyField = "DATAKEYFIELD";
            }
            else if ((rr.TableName == "POSDET") || (rr.TableName == "POSDETOTC"))
            {
                table_DataKeyField = "IDT";
                table_ForeignKeyField = "DATAKEYFIELD";
            }
            else if (rr.TableName == "MATURITY")
            {
                table_DataKeyField = "IDMATURITY";
                table_ForeignKeyField = "IDMATURITYRULE";
            }
            else if (rr.TableName == "VW_ASSET_ETD_EXPANDED")
            {
                table_DataKeyField = "IDASSET";
                table_ForeignKeyField = "IDI";
            }
            else if (rr.TableName == "RISKPERFORMANCE")
            {
                table_DataKeyField = "IDT";
            }
            else if (rr.TableName == "CASHBALANCE")
            {
                table_DataKeyField = "IDT";
            }
            else if (rr.TableName == "CASHINTEREST")
            {
                table_DataKeyField = "IDT";
            }
            else if (rr.TableName == "CASHPAYMENT")
            {
                table_DataKeyField = "IDT";
            }
            else if (rr.TableName == "MCO")
            {
                table_DataKeyField = "IDMCO";
            }
            //
            if (StrFunc.IsFilled(table_DataKeyField))
            {
                existsDataKeyField = false;
                existsForeignKeyField = StrFunc.IsEmpty(table_ForeignKeyField);//PL Tip
                for (int i = 0; i < dtLstSelectedCol.Rows.Count; i++)
                {
                    if ((!existsDataKeyField) && dtLstSelectedCol.Rows[i]["COLUMNNAME"].ToString() == table_DataKeyField)
                        existsDataKeyField = true;
                    if ((!existsForeignKeyField) && dtLstSelectedCol.Rows[i]["COLUMNNAME"].ToString() == table_ForeignKeyField)
                        existsForeignKeyField = true;
                    if (existsDataKeyField && existsForeignKeyField)
                        break;
                }

                #region (!existsDataKeyField)
                if (!existsDataKeyField)
                {
                    rrcDataKeyField.ColumnName = table_DataKeyField;
                    rrcDataKeyField.Ressource = table_DataKeyField;
                    rrcDataKeyField.RessourceSpecified = true;
                    rrcDataKeyField.AliasTableName = table_AliasTableName;
                    rrcDataKeyField.AliasTableNameSpecified = true;
                    //
                    if (IsConsultation(ConsultEnum.MCO_RPT) ||
                        IsConsultation(ConsultEnum.MCO_RPT_FINPER))
                    {
                        rrcDataKeyField.AliasColumnNameSpecified = true;
                        rrcDataKeyField.AliasColumnName = "IDMCO";
                    }
                    //
                    if ((rr.TableName == "POSSYNT") || (rr.TableName == "POSSYNTOTC"))
                        rrcDataKeyField.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                    else
                        rrcDataKeyField.DataType.value = TypeData.TypeDataEnum.integer.ToString();

                    rrcDataKeyField.Length = 50;
                    rrcDataKeyField.Scale = 0;
                    rrcDataKeyField.IsMandatory = false;
                    rrcDataKeyField.RegularExpression = string.Empty;
                    rrcDataKeyField.IsHide = true;
                    rrcDataKeyField.IsHideInDataGrid = true;
                    rrcDataKeyField.IsKeyField = false;
                    rrcDataKeyField.IsForeignKeyField = false;
                    rrcDataKeyField.IsIdentity.Value = false;
                    rrcDataKeyField.IsDataKeyField = true;
                    rrcDataKeyField.IsDataKeyFieldSpecified = true;
                    //
                    //FI 20111027 Aadd pavé AliasColumnNameSpecified
                    if (rrcDataKeyField.AliasColumnNameSpecified)
                    {
                        rrcDataKeyField.DataField = rrcDataKeyField.AliasColumnName;
                    }
                    else
                    {
                        rrcDataKeyField.DataField = RepositoryTools.GetNewAlias(rrcDataKeyField.AliasTableName + "_" + rrcDataKeyField.ColumnName, ref listColumnAlias);
                    }

                    rrcDataKeyField.ColspanSpecified = true;
                    rrcDataKeyField.IsForeignKeyFieldSpecified = true;
                    rrcDataKeyField.IsHideInDataGridSpecified = true;
                    rrcDataKeyField.IsHideSpecified = true;
                    rrcDataKeyField.IsIdentitySpecified = true;
                    rrcDataKeyField.IsKeyFieldSpecified = true;
                    rrcDataKeyField.IsMandatorySpecified = true;
                    rrcDataKeyField.IsOrderBySpecified = true;
                    rrcDataKeyField.IsOrderBy.Value = "false";
                    rrcDataKeyField.IsUpdatableSpecified = false;
                    rrcDataKeyField.LengthSpecified = true;
                    rrcDataKeyField.ScaleSpecified = true;
                }
                #endregion (!existsDataKeyField)

                #region (!existsForeignKeyField)
                if ((!existsForeignKeyField) && StrFunc.IsFilled(table_ForeignKeyField))
                {
                    rrcForeignKeyField.ColumnName = table_ForeignKeyField;
                    rrcForeignKeyField.Ressource = table_ForeignKeyField;
                    rrcForeignKeyField.RessourceSpecified = true;
                    rrcForeignKeyField.AliasTableName = table_AliasTableName;
                    rrcForeignKeyField.AliasTableNameSpecified = true;

                    if ((rr.TableName == "POSDET") || (rr.TableName == "POSDETOTC"))
                        rrcForeignKeyField.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                    else
                        rrcForeignKeyField.DataType.value = TypeData.TypeDataEnum.integer.ToString();

                    rrcForeignKeyField.Length = 50;
                    rrcForeignKeyField.Scale = 0;
                    rrcForeignKeyField.IsMandatory = false;
                    rrcForeignKeyField.RegularExpression = string.Empty;
                    rrcForeignKeyField.IsHide = true;
                    rrcForeignKeyField.IsHideInDataGrid = true;
                    rrcForeignKeyField.IsKeyField = false;
                    rrcForeignKeyField.IsForeignKeyField = true;
                    rrcForeignKeyField.IsIdentity.Value = false;
                    rrcForeignKeyField.IsDataKeyField = false;
                    rrcForeignKeyField.IsDataKeyFieldSpecified = true;
                    rrcForeignKeyField.DataField = RepositoryTools.GetNewAlias(rrcForeignKeyField.AliasTableName + "_" + rrcForeignKeyField.ColumnName, ref listColumnAlias);
                    rrcForeignKeyField.ColspanSpecified = true;
                    rrcForeignKeyField.IsForeignKeyFieldSpecified = true;
                    rrcForeignKeyField.IsHideInDataGridSpecified = true;
                    rrcForeignKeyField.IsHideSpecified = true;
                    rrcForeignKeyField.IsIdentitySpecified = true;
                    rrcForeignKeyField.IsKeyFieldSpecified = true;
                    rrcForeignKeyField.IsMandatorySpecified = true;
                    rrcForeignKeyField.IsOrderBySpecified = true;
                    rrcForeignKeyField.IsOrderBy.Value = "false";
                    rrcForeignKeyField.IsUpdatableSpecified = false;
                    rrcForeignKeyField.LengthSpecified = true;
                    rrcForeignKeyField.ScaleSpecified = true;
                }
                #endregion (!existsForeignKeyField)
            }
            //                
            int add = (existsDataKeyField ? 0 : 1) +
                      (existsForeignKeyField ? 0 : 1) +
                      (StrFunc.IsEmpty(table_OrderBy) ? 0 : 1);
            rr.Column = new ReferentialColumn[dtLstSelectedCol.Rows.Count + add];
            //
            int indice = 0;
            if (!existsDataKeyField)
            {
                rr.Column[indice] = rrcDataKeyField;
                indice++;
            }
            if (!existsForeignKeyField)
            {
                rr.Column[indice] = rrcForeignKeyField;
                indice++;
            }
            if (StrFunc.IsFilled(table_OrderBy))
            {
                #region rrcOrderBy
                rrcOrderBy = new ReferentialColumn();

                rrcOrderBy.ColumnName = table_OrderBy;
                rrcOrderBy.Ressource = table_OrderBy;
                rrcOrderBy.RessourceSpecified = true;
                rrcOrderBy.AliasTableName = table_AliasTableName;
                rrcOrderBy.AliasTableNameSpecified = true;
                rrcOrderBy.DataType = new ReferentialColumnDataType(); 
                rrcOrderBy.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                rrcOrderBy.Length = 64;
                rrcOrderBy.IsHide = true;
                rrcOrderBy.IsHideSpecified = true;
                rrcOrderBy.IsHideInDataGrid = true;
                rrcOrderBy.IsHideInDataGridSpecified = true;
                rrcOrderBy.DataField = RepositoryTools.GetNewAlias(rrcOrderBy.AliasTableName + "_" + rrcOrderBy.ColumnName, ref listColumnAlias);
                rrcOrderBy.IsOrderBy.Value = "true";
                rrcOrderBy.IsOrderBySpecified = true;
                #endregion
                rr.Column[indice] = rrcOrderBy;
                indice++;
            }
            #endregion Ajout de colonne en DUR


            AddReferentialSelectedColumn(rr, indice);

            AddReferentialTechnicalColumn(rr);

        }

        /// <summary>
        ///  Ajoute éventuellement des attributs IsHyperLink à la colonne {rrc} 
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="rrc"></param>
        /// <param name="pTableName"></param>
        /// FI 20111226 New Method
        /// FI 20121010 
        /// Gestion de la table SECURITY, ASSET, TRADE_ORDER, TRADEADMIN, BOOK_XXX et ACTOR_XXX
        /// Gestion du hyperlink IDASSET
        /// FI 20130307 gestion de la table ASSET_ETD_ et DERIVATIVECONTRACT
        /// CC 20130311 Gestion de la table ASSET_ETD_ESE
        /// CC 20130311 Gestion de la colonne IDENTIFIER_MISSING (table BOOK_ETD)
        /// FI 20140815 [XXXXX] Modify 
        /// FI 20150916 [XXXXX] Modify
        private void SetReferentialColumnHyperlink(Referential rr, ReferentialColumn rrc, string pTableName)
        {
            string tblName = pTableName;
            //row["TABLENAME"].ToString();
            string colName = rrc.ColumnName;
            //row["COLUMNNAME"].ToString();
            //
            //Liste des tables pour lesquelles Spheres génère des hyperlink
            bool isOk = tblName.StartsWith("ACTOR") || tblName.StartsWith("BOOK");
            isOk |= (tblName == "TRADE") || (tblName == "TRADE_LSD") || (tblName == "TRADE_ORDER") || (tblName == "TRADEADMIN");
            isOk |= (tblName == "DERIVATIVECONTRACT");
            //CC 20130311 Table ASSET obsolète
            //isOk |= (tblName == "ASSET");
            isOk |= (tblName == "ASSET_ETD_");

            isOk |= (tblName == "ASSET");

            //CC 20130311 Add
            isOk |= (tblName == "ASSET_ETD_ESE");
            isOk |= (tblName == "VW_ASSET_ETD_EXPANDED");
            isOk |= (tblName == "SECURITY");
            isOk |= (tblName == "MARKET") || (tblName == "VW_MARKET_IDENTIFIER");
            isOk |= (tblName == "CNFMESSAGE" || tblName == "TS_CNFMESSAGE");
            //CNFMESSAGE => pour Confirmations 
            //TS_CNFMESSAGE => pour Editions
            //
            if (isOk)
            {
                //Liste des colonnes pour lesquelles Spheres génère des hyperlink
                isOk = ((colName == "IDENTIFIER") || (colName == "DISPLAYNAME"));
                //CC 20130311 Table ASSET obsolète
                //if ((tblName == "VW_ASSET_ETD_EXPANDED") || (tblName == "ASSET"))
                if ((tblName == "VW_ASSET_ETD_EXPANDED"))
                    isOk |= (colName == "CONTRACTIDENTIFIER") || (colName == "CONTRACTDISPLAYNAME");
                else if ((tblName == "MARKET") || (tblName == "VW_MARKET_IDENTIFIER"))
                    isOk |= (colName.StartsWith("SHORT_ACRONYM"));
                else if ((tblName == "SECURITY"))
                    isOk |= ((colName == "SecurityId") || (colName == "SecurityName"));
                //CC 20130311 Add
                else if ((tblName.StartsWith("BOOK")))
                    isOk |= ((colName == "IDENTIFIER_MISSING"));
            }
            //
            if (isOk)
            {
                string columnData = string.Empty;
                string sqlColumnData = string.Empty;
                string hyperLinkType = string.Empty;
                GetColumnDataHyperLink(tblName, colName, out hyperLinkType, out columnData, out sqlColumnData);

                string aliasTable = rrc.AliasTableName;
                if (null == rr[columnData, aliasTable])
                {
                    AddReferentialHideColumn(rr, aliasTable, columnData, TypeData.TypeDataEnum.integer, sqlColumnData);
                    //
                    // La colonne IDASSET doit nessairement etre couplée à la colonne ASSETCATEGORY
                    if (columnData == "IDASSET")
                    {
                        if (IsConsultation(ConsultEnum.TRADEFnO_ALLOC) || IsConsultation(ConsultEnum.TRADEFnO_ORDER_BLOCK))
                        {
                            AddReferentialHideColumn(rr, aliasTable, "ASSETCATEGORY", TypeData.TypeDataEnum.@string,
                                @"case when product.GPRODUCT= 'FUT' then 'ExchangeTradedContract'
                                       when product.GPRODUCT= 'SEC' then 'EquityAsset'
	                                   else null
                                   end");
                        }
                        else if (IsConsultation(ConsultEnum.TRADEOTC))
                        {
                            AddReferentialHideColumn(rr, aliasTable, "ASSETCATEGORY", TypeData.TypeDataEnum.@string,
                            @"case when product.FAMILY= 'DSE' then 'Bond'
                                   when product.FAMILY= 'ESE' then 'EquityAsset'
	                               else null
                              end");
                        }
                        else if (IsConsultation(ConsultEnum.TRADEOTC_ALLOC) ||
                                 IsConsultation(ConsultEnum.POSDETOTC_ALLOC) ||
                                 IsConsultation(ConsultEnum.FLOWSBYTRADEOTC_ALLOC) ||
                                 IsConsultation(ConsultEnum.POSACTIONDET_OTC)
                            ) // FI 20150916 [XXXXX] add POSACTIONDET_OTC
                        {
                            //FI 20140815 [XXXXX] ce code devrait être applicable sur TRADEFnO_ALLOC,TRADEFnO_ORDER_BLOCK (a voir plus tard)
                            AddReferentialHideColumn(rr, aliasTable, "ASSETCATEGORY", TypeData.TypeDataEnum.@string, "ti_lsd.ASSETCATEGORY");
                        }
                        //CC 20151123 [21489]
                        else if (IsConsultation(ConsultEnum.FLOWSBYASSETOTC_ALLOC)
                                )
                        {
                            AddReferentialHideColumn(rr, aliasTable, "ASSETCATEGORY", TypeData.TypeDataEnum.@string, "asset.ASSETCATEGORY");
                        }
                    }
                }
                //
                rrc.IsHyperLink = new ReferentialColumnIsHyperLink();
                rrc.IsHyperLinkSpecified = true;
                //
                rrc.IsHyperLink.linktypeSpecified = true;
                rrc.IsHyperLink.linktype = "column";
                //                            
                rrc.IsHyperLink.Value = true;
                //
                //FI 20120926 [] usage of data attribute for HyperLink column
                rrc.IsHyperLink.dataSpecified = true;
                rrc.IsHyperLink.data = (rr[columnData, aliasTable].DataField).ToUpper();
                //
                rrc.IsHyperLink.typeSpecified = true;
                rrc.IsHyperLink.type = hyperLinkType;
            }
        }

        /// <summary>
        /// Création d'éléments SQLJoin au sein de Referential.
        /// </summary>
        /// <param name="rr"></param>
        private void SetReferentialSQLJoin(Referential rr)
        {
            int indice = 0;
            rr.SQLJoin = new string[dtLstConsultAliasSelected.Rows.Count];

            foreach (DataRow row in dtLstConsultAliasSelected.Rows)
            {
                //PL 20150928 Pas de suppression des "espaces" en début de commande (Utilisé pour les Table hors schéma, tel que POSDETQUOTE_%%SHORTSESSIONID%%_W)
                //string sqlJoin = row["SQLJOIN"].ToString().Trim();
                string sqlJoin = row["SQLJOIN"].ToString().TrimEnd();

                if (sqlJoin.StartsWith("(") || sqlJoin.StartsWith(" "))
                {
                    //Pas d'ajout de préfixe de propriétaire 
                    //"(": Sous-requête SQL 
                    //" ": Table hors schéma 
                    sqlJoin = SQLCst.GetSQLSyntaxForJoin(row["TYPEJOIN"].ToString()).Replace(SQLCst.DBO, " ") + sqlJoin;
                }
                else
                {
                    sqlJoin = SQLCst.GetSQLSyntaxForJoin(row["TYPEJOIN"].ToString()) + sqlJoin;
                }

                sqlJoin = @"/* Join: " + row["IDLSTJOIN"].ToString().Trim() + @" */" + Cst.CrLf + sqlJoin.Trim();

                rr.SQLJoin[indice] = sqlJoin;
                rr.SQLJoinSpecified = true;
                indice++;
            }
        }

        /// <summary>
        /// Ajout des colonnes techniques (colonne qui seront présentes dans le jeu de résultat) 
        /// </summary>
        /// <param name="rr"></param>
        private void AddReferentialTechnicalColumn(Referential rr)
        {
            // Add en dur en fonction de la consultation
            if (IsConsultation(ConsultEnum.MCO_RPT) || IsConsultation(ConsultEnum.MCO_RPT_FINPER))
            {
                ReferentialColumn rrc = null;
                #region add LOCNFMSGTXT
                rrc = new ReferentialColumn();
                rrc.AliasTableNameSpecified = true;
                rrc.AliasTableName = rr.AliasTableName;

                rrc.ColumnName = "LOCNFMSGTXT";
                rrc.AliasColumnNameSpecified = true;
                rrc.AliasColumnName = rrc.ColumnName;

                rrc.DataType = new ReferentialColumnDataType(); 
                rrc.DataType.value = TypeData.TypeDataEnum.text.ToString();
                rrc.IsResourceSpecified = true;

                rrc.IsResource = new ReferentialColumnResource(false);

                rrc.IsHideInDataGridSpecified = true;
                rrc.IsHideInDataGrid = true;
                rrc.IsHideInCriteriaSpecified = true;
                rrc.IsHideInCriteria = true;

                rrc.LengthInDataGridSpecified = true;
                rrc.LengthInDataGrid = -1;

                rr.AddColumn(rrc);
                #endregion

                #region add LOCNFMSGBIN
                rrc = new ReferentialColumn();
                rrc.AliasTableNameSpecified = true;
                rrc.AliasTableName = rr.AliasTableName;

                rrc.ColumnName = "LOCNFMSGBIN";
                rrc.AliasColumnNameSpecified = true;
                rrc.AliasColumnName = rrc.ColumnName;
                rrc.DataType = new ReferentialColumnDataType(); 
                rrc.DataType.value = TypeData.TypeDataEnum.image.ToString();

                rrc.IsResourceSpecified = true;
                rrc.IsResource = new ReferentialColumnResource(false);

                rrc.IsHideInDataGridSpecified = true;
                rrc.IsHideInDataGrid = true;
                rrc.IsHideInCriteriaSpecified = true;
                rrc.IsHideInCriteria = true;

                rrc.LengthInDataGridSpecified = true;
                rrc.LengthInDataGrid = -1;

                rr.AddColumn(rrc);
                #endregion

                #region add DOCNAME
                rrc = new ReferentialColumn();
                rrc.AliasTableNameSpecified = true;
                rrc.AliasTableName = rr.AliasTableName;

                rrc.ColumnName = "DOCNAME";
                rrc.AliasColumnNameSpecified = true;
                rrc.AliasColumnName = rrc.ColumnName;

                rrc.DataType = new ReferentialColumnDataType(); 
                rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();

                rrc.IsResourceSpecified = true;
                rrc.IsResource = new ReferentialColumnResource(false);

                rrc.IsHideInDataGridSpecified = true;
                rrc.IsHideInDataGrid = true;
                rrc.IsHideInCriteriaSpecified = true;
                rrc.IsHideInCriteria = true;

                rrc.LengthInDataGridSpecified = true;
                rrc.LengthInDataGrid = -1;

                rrc.IsVirtualColumnSpecified = true;
                rrc.IsVirtualColumn = true;

                rr.AddColumn(rrc);
                #endregion
            }
            else if (IsConsultation(ConsultEnum.TRADEADMIN))
            {
                //FI 20120104 
                //La colonne GPRODUCT est nécessaire pour que les links redirige vers tradeAdminCapturePage.aspx 
                if (null == rr["GPRODUCT", "product"])
                {
                    ReferentialColumn rrc = new ReferentialColumn();
                    rrc.AliasTableNameSpecified = true;
                    rrc.AliasTableName = "product";
                    rrc.ColumnName = "GPRODUCT";
                    rrc.DataType = new ReferentialColumnDataType(); 
                    rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();

                    rrc.IsResourceSpecified = true;
                    rrc.IsResource = new ReferentialColumnResource(false);

                    rrc.IsHideInDataGridSpecified = true;
                    rrc.IsHideInDataGrid = true;

                    rr.AddColumn(rrc);
                }
            }

            //AddReferentialTrackColumn(rr);
        }

        /// <summary>
        /// Ajoute des colonnes selectionnées
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="indice"></param>
        /// FI 20140519 [19923] Add Method
        private void AddReferentialSelectedColumn(Referential rr, int indice)
        {

            ArrayList listColumnAlias = new ArrayList();
            bool isFound_ColumnRef = false;

            foreach (DataRow row in dtLstSelectedCol.Rows)
            {
                //
                string columnXML = string.Empty;
                if (false == (row["COLUMNXML"] is DBNull))
                    columnXML = row["COLUMNXML"].ToString();

                if (StrFunc.IsFilled(columnXML))
                {
                    EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(ReferentialColumn), columnXML);
                    rr.Column[indice] = (ReferentialColumn)CacheSerializer.Deserialize(serializerInfo);
                }
                else
                    rr.Column[indice] = new ReferentialColumn();
                //
                rr.Column[indice].Ressource = row["DISPLAYNAME"].ToString();
                rr.Column[indice].RessourceSpecified = true;
                if (IsMultiTable(SessionTools.CS))
                    rr.Column[indice].Ressource = "[" + row["ALIASDISPLAYNAME"].ToString() + "] " + Cst.HTMLBreakLine + rr.Column[indice].Ressource;
                rr.Column[indice].ColumnName = row["COLUMNNAME"].ToString().Trim();

                rr.Column[indice].AliasTableNameSpecified = true;
                rr.Column[indice].AliasTableName = row["ALIAS"].ToString().Trim();

                //
                //RD 20110524 [] Affichage d'un Header commun à plusieurs colonnes
                rr.Column[indice].ColumnRef = (row["COLUMNREF"] is DBNull ? string.Empty : row["COLUMNREF"].ToString().Trim());
                SetColumnRessourceFromColumnRef(rr, indice, ref isFound_ColumnRef);
                //
                string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                if (StrFunc.IsFilled(sqlSelect))
                    rr.Column[indice].ColumnNameOrColumnSQLSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[indice].AliasTableName);
                else
                    rr.Column[indice].ColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameExpression(rr.Column[indice].ColumnName, rr.Column[indice].AliasTableName);
                rr.Column[indice].ColumnNameOrColumnSQLSelectSpecified = StrFunc.IsFilled(rr.Column[indice].ColumnNameOrColumnSQLSelect);
                //
                rr.Column[indice].DataType = new ReferentialColumnDataType(); 
                rr.Column[indice].DataType.value = row["DATATYPE"].ToString().Trim();
                //
                rr.Column[indice].LengthSpecified = true;
                rr.Column[indice].Length = 50;
                //
                rr.Column[indice].ScaleSpecified = true;
                if (row["SCALE"] is DBNull)
                    rr.Column[indice].Scale = 0;
                else
                    rr.Column[indice].Scale = Convert.ToInt32(row["SCALE"]);
                //
                rr.Column[indice].GroupBySpecified = true;
                rr.Column[indice].GroupBy = new ReferentialColumnGroupBy();
                rr.Column[indice].GroupBy.IsGroupBy = (false == (row["ISGROUPBY"] is DBNull || BoolFunc.IsFalse(row["ISGROUPBY"])));

                // MF 20120430 ruptures with groupingset
                rr.Column[indice].GroupBy.GroupingSet = Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET");
                //
                if (row["AGGREGATE"] is DBNull)
                {
                    rr.Column[indice].GroupBy.AggregateSpecified = false;
                }
                else
                {
                    rr.Column[indice].GroupBy.Aggregate = Convert.ToString(row["AGGREGATE"]);
                    rr.Column[indice].GroupBy.AggregateSpecified = StrFunc.IsFilled(rr.Column[indice].GroupBy.Aggregate);
                    rr.HasAggregateColumns = rr.HasAggregateColumns || rr.Column[indice].GroupBy.AggregateSpecified;
                }
                //
                if (row["SQLGROUPBY"] is DBNull)
                {
                    rr.Column[indice].GroupBy.SqlGroupBySpecified = false;
                }
                else
                {
                    //PL 20120426 Bug
                    //rr.Column[indice].GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]);
                    rr.Column[indice].GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]).Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[indice].AliasTableName);
                    rr.Column[indice].GroupBy.SqlGroupBySpecified = StrFunc.IsFilled(rr.Column[indice].GroupBy.SqlGroupBy);
                }
                //
                rr.Column[indice].IsMandatorySpecified = true;
                rr.Column[indice].IsMandatory = false;
                //
                rr.Column[indice].RegularExpression = "";
                //
                rr.Column[indice].IsHideSpecified = true;
                rr.Column[indice].IsHide = true;
                //
                //FI 20111026  ajout de false == rr.Column[indice].IsHideInDataGridSpecified
                //Cette donnée peut être alimentée par COLUMNXML
                if (false == rr.Column[indice].IsHideInDataGridSpecified)
                {
                    rr.Column[indice].IsHideInDataGridSpecified = true;
                    rr.Column[indice].IsHideInDataGrid = false;
                }
                //
                //FI 20111026  ajout de false == rr.Column[indice].IsHideInCriteriaSpecified
                if (false == rr.Column[indice].IsHideInCriteriaSpecified)
                {
                    rr.Column[indice].IsHideInCriteriaSpecified = true;
                    rr.Column[indice].IsHideInCriteria = false;
                }
                //
                rr.Column[indice].IsDataKeyFieldSpecified = true;
                rr.Column[indice].IsDataKeyField = false;
                if (rr.TableName == Cst.OTCml_TBL.TRADE.ToString() && rr.Column[indice].ColumnName == "IDT")
                    rr.Column[indice].IsDataKeyField = true;
                //
                rr.Column[indice].IsKeyFieldSpecified = true;
                rr.Column[indice].IsKeyField = false;
                //
                rr.Column[indice].IsForeignKeyFieldSpecified = true;
                rr.Column[indice].IsForeignKeyField = false;
                //
                rr.Column[indice].IsIdentitySpecified = true;
                rr.Column[indice].IsIdentity.Value = false;
                //
                //FI 20120412  ajout de false == rr.Column[indice].IsResourceSpecified
                //Cette donnée peut être alimentée par COLUMNXML
                if (false == rr.Column[indice].IsResourceSpecified)
                {
                    rr.Column[indice].IsResourceSpecified = true;
                    rr.Column[indice].IsResource = new ReferentialColumnResource(Convert.ToBoolean(row["ISRESOURCE"]));
                }
                //   
                //FI 20111026  ajout de rr.Column[indice].AliasColumnNameSpecified
                if (rr.Column[indice].AliasColumnNameSpecified)
                {
                    rr.Column[indice].DataField = rr.Column[indice].AliasColumnName;
                }
                else
                {
                    //20090820 RD/ Pour eviter les doublons sur les Alias                    
                    rr.Column[indice].DataField = RepositoryTools.GetNewAlias(rr.Column[indice].AliasTableName + "_" + rr.Column[indice].ColumnName, ref listColumnAlias);
                }
                //
                rr.Column[indice].ColspanSpecified = true;
                //
                rr.Column[indice].IsOrderBySpecified = true;
                rr.Column[indice].IsOrderBy.Value = "false";
                if (StrFunc.IsFilled(row["SQLORDER"].ToString()))
                {
                    rr.Column[indice].IsOrderBy.orderSpecified = true;
                    rr.Column[indice].IsOrderBy.order = row["SQLORDER"].ToString().Trim().Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[indice].AliasTableName);
                }
                //
                rr.Column[indice].IsUpdatableSpecified = false; //pourkoi ???? false
                //
                SetReferentialColumnHyperlink(rr, rr.Column[indice], row["TABLENAME"].ToString());
                //
                indice++;

            }
        }


        /// <summary>
        /// Retourne true si la consultation courante est {pConsultation}
        /// </summary>
        /// <param name="pConsultation"></param>
        /// <returns></returns>
        public bool IsConsultation(ConsultEnum pConsultation)
        {
            return (IdLstConsult == pConsultation.ToString());
        }

        /// <summary>
        /// Retourne le nom court de la consultation d'un Referentiel
        /// <para>Par Exemple pour la consultation REF-ReferentialACTOR, retourne ACTOR</para>
        /// </summary>
        public string ReferentialShortIdConsult()
        {
            string ret = IdLstConsult;

            foreach (Cst.ListType type in Enum.GetValues(typeof(Cst.ListType)))
            {
                string tmp = RepositoryWeb.PrefixForReferential + type.ToString();
                if (IdLstConsult.StartsWith(tmp))
                {
                    ret = IdLstConsult.Substring(tmp.Length);
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Ajoute une colonne technique (Hide) dans la liste des colonnes présentes dans {rr}
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pDataType"></param>
        /// <param name="sqlExpression"></param>
        /// FI 20121010 Nouvelle méthode
        private static void AddReferentialHideColumn(Referential rr,
            string pAliasTable, string pColumnName, TypeData.TypeDataEnum pDataType, string sqlExpression)
        {
            ArrayList al = new ArrayList();
            ReferentialColumn rc = new ReferentialColumn();
            rc.ColumnName = pColumnName;
            rc.AliasTableNameSpecified = true;
            rc.AliasTableName = pAliasTable;
            rc.DataField = RepositoryTools.GetNewAlias(rc.AliasTableName + "_" + rc.ColumnName, ref al);
            rc.DataType = new ReferentialColumnDataType(); 
            rc.DataType.value= pDataType.ToString();
            rc.ScaleSpecified = true;
            rc.Scale = 0;
            rc.IsHideInCriteriaSpecified = true;
            rc.IsHideInCriteria = true;
            rc.IsHideInDataGridSpecified = true;
            rc.IsHideInDataGrid = true;
            rc.IsHideSpecified = true;
            rc.IsHide = true;

            if (StrFunc.IsFilled(sqlExpression))
            {
                rc.ColumnNameOrColumnSQLSelectSpecified = true;
                rc.ColumnNameOrColumnSQLSelect = sqlExpression;
            }

            rr.AddColumn(rc);
        }

        /// <summary>
        /// Retoure les informations nécessaires pour générer un HyperLink sur la colonne {pTableName}.{pColName}
        /// </summary>
        /// <param name="pTableName">Table sur laquelle sera présent les HyperLink por toutes les lignes</param>
        /// <param name="pColName">Colonne sur laquelle sera présent les HyperLink por toutes les lignes</param>
        /// <param name="hyperLinkType">Type le HyperLink (EX IDA pour ouverture du formulaire acteur)</param>
        /// <param name="columnData">Colonne contient la donnée PK qui permet l'ouverture du formulaire</param>
        /// <param name="sqlColumnData">Expression associée à la colonne {columnData}, facultatif</param>
        /// <exception cref="NotImplementedException si la colonne {pTableName}.{pColName} est non gérée "></exception>
        /// FI 20121010 Nouvelle méthode
        /// FI 20130307 gestion de la table ASSET_ETD_ et DERIVATIVECONTRACT
        /// CC 20130311 Gestion des tables ACTOR_CLEARER et ACTOR_EXECUTOR Ticket 18218
        /// CC 20130311 Gestion de la table ASSET_ETD_ESE
        /// CC 20130311 Gestion de la colonne IDENTIFIER_MISSING (table BOOK_ETD)
        /// FI 20140815 [XXXXX] Modify
		// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void GetColumnDataHyperLink(string pTableName, string pColName,
            out  string hyperLinkType,
            out string columnData,
            out string sqlColumnData)
        {
            columnData = string.Empty;
            sqlColumnData = string.Empty;
            hyperLinkType = string.Empty;

            if (pTableName.StartsWith(Cst.OTCml_TBL.ACTOR.ToString()))
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDA";
                    hyperLinkType = "IDA";

                    //CC 20130311 Add
                    if ((pTableName == "ACTOR_CLEARER"))
                    {
                        if (IsConsultation(ConsultEnum.TRADEFnO_ALLOC))
                        {
                            sqlColumnData = @"case when b_dlr.ISPOSKEEPING=0 then a_clr.IDA else ae.IDA
                                            end";
                        }
                    }
                    else if ((pTableName == "ACTOR_EXECUTOR"))
                    {
                        if (IsConsultation(ConsultEnum.TRADEFnO_ALLOC))
                        {
                            sqlColumnData = @"case when ta_clearing.FIXPARTYROLE='21' and ta_execbkr.IDA is not null then a_exec.IDA       
	                                               when ta_clearing.FIXPARTYROLE='21' and ta_execbkr.IDA is null then ae.IDA       
	                                               when ta_clearing.FIXPARTYROLE='4'  then ae.IDA       
	                                               else null  
	                                          end";
                        }
                    }
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if (pTableName.StartsWith(Cst.OTCml_TBL.BOOK.ToString()))
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDB";
                    hyperLinkType = "IDB";
                }
                //CC 20130311 Add
                else if (pColName == "IDENTIFIER_MISSING")
                {
                    columnData = "IDB";
                    hyperLinkType = "IDB";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if (pTableName == "SECURITY")
            {
                if (pColName == "SecurityId" || pColName == "SecurityName")
                {
                    columnData = "IDASSET";
                    hyperLinkType = "IDASSET";
                    if (IsConsultation(ConsultEnum.TRADEOTC))
                    {
                        sqlColumnData = @"case   when product.FAMILY= 'DSE' then tasset.IDT
                                                 when product.FAMILY= 'ESE' then a_eqty.IDASSET
                                                 else null
                                            end";
                    }
                }
            }
            else if (pTableName == Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED.ToString())
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDASSET";
                    hyperLinkType = "IDASSET_ETD";
                }
                else if (pColName == "CONTRACTIDENTIFIER" || pColName == "CONTRACTDISPLAYNAME")
                {
                    columnData = "IDDC";
                    hyperLinkType = "IDDC";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if (pTableName == "ASSET_ETD_")
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDASSET";
                    hyperLinkType = "IDASSET_ETD";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            //CC 20130311 Add
            else if (pTableName == "ASSET_ETD_ESE")
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDASSET";
                    hyperLinkType = "IDASSET";
                    if (IsConsultation(ConsultEnum.TRADEFnO_ALLOC) || IsConsultation(ConsultEnum.TRADEFnO_ORDER_BLOCK))
                    {
                        sqlColumnData = @"case when product.GPRODUCT= 'FUT' then a_etdese.IDASSET     
	                                           when product.GPRODUCT= 'SEC' then a_eqty.IDASSET     
	                                           else null  
                                            end";
                    }
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if (pTableName == "ASSET") /// FI 20140815 [XXXXX] Add (ce code devrait être applicable à TRADEFnO_ALLOC  et TRADEFnO_ORDER_BLOCK, on verra plus tard)
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDASSET";
                    hyperLinkType = "IDASSET";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }

            else if (pTableName == "DERIVATIVECONTRACT")
            {

                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDDC";
                    hyperLinkType = "IDDC";
                }
            }
            else if (pTableName == "CONTRACT")
            {

                columnData = "IDCONTRACT";
                hyperLinkType = "IDCONTRACT";

                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    sqlColumnData = @"case when product.GPRODUCT= 'FUT' then dc.IDDC     
                                           when product.GPRODUCT= 'COM' then a_com.IDCC     
	                                       else null  
                                           end";
                }

            }
            else if ((pTableName == "TRADE") ||
                     (pTableName == "TRADE_LSD") ||
                     (pTableName == "TRADE_ORDER") ||
                     (pTableName == "TRADEADMIN"))
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDT";
                    hyperLinkType = "IDT";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if ((pTableName == "VW_MARKET_IDENTIFIER") ||
                     (pTableName == "MARKET"))
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME" || pColName.StartsWith("SHORT_ACRONYM"))
                {
                    columnData = "IDM";
                    hyperLinkType = "IDM";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }
            else if ((pTableName == "CNFMESSAGE") ||
                      (pTableName == "TS_CNFMESSAGE"))
            {
                if (pColName == "IDENTIFIER" || pColName == "DISPLAYNAME")
                {
                    columnData = "IDCNFMESSAGE";
                    hyperLinkType = "IDCNFMESSAGE";
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
            }

            else
                throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
        }

        /// <summary>
        /// Retourne true si {pValue} est parsé avec succès en date
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pValue"></param>
        /// <param name="pIsFormatForDisplay"></param>
        /// <returns></returns>
        // FI 20121106 [] new méthode 
        [Obsolete("Replaced by IsParseDateOk2", true)]
        private static bool IsParseDateOk(string pDataType, string pValue, bool pIsFormatForDisplay)
        {
            //Usage de DtFunc.IsParsableValue de manière à ne pas générer d'exception sur le mot clef TODAY ou BUSINESS qui sont largement utilisés
            bool ret = (DtFunc.IsParsableValue(pValue));
            try
            {
                if (ret)
                {
                    //Si pIsFormatForDisplay: Spheres® tente de parser ce qui existe en base (stocker au format ISO)
                    //Sinon                 : Spheres® tente de parser ce qui a été saisie pour le stocker au format ISO
                    if (TypeData.IsTypeDateTime(pDataType))
                    {
                        if (pIsFormatForDisplay)
                            DtFunc.ParseDate(pValue, DtFunc.FmtISODateTime2, null);
                        else
                            DtFunc.ParseDate(pValue, DtFunc.FmtDateTime, CultureInfo.CurrentCulture);
                    }
                    else if (TypeData.IsTypeDate(pDataType))
                    {
                        if (pIsFormatForDisplay)
                            DtFunc.ParseDate(pValue, DtFunc.FmtISODate, null);
                        else
                            DtFunc.ParseDate(pValue, DtFunc.FmtShortDate, CultureInfo.CurrentCulture);
                    }
                    else if (TypeData.IsTypeTime(pDataType))
                    {
                        if (pIsFormatForDisplay)
                            DtFunc.ParseDate(pValue, DtFunc.FmtISOTime, null);
                        else
                            DtFunc.ParseDate(pValue, DtFunc.FmtShortTime, CultureInfo.CurrentCulture);
                    }
                }
                else
                {
                    ret = false;
                }
            }
            catch (FormatException)
            {
                ret = false;
            }
            return ret;
        }
        /// <summary>
        /// Retourne true si {pValue} est parsé avec succès en date
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pValue"></param>
        /// <param name="pIsFormatForDisplay"></param>
        /// <returns></returns>
        // PL 20150722 Newness
        private static bool IsParseDateOk2(string pDataType, string pValue, bool pIsFormatForDisplay)
        {
            //Usage de DtFunc.IsParsableValue de manière à ne pas générer d'exception sur le mot clef TODAY ou BUSINESS qui sont largement utilisés
            bool ret = (DtFunc.IsParsableValue(pValue));
            if (ret)
            {
                try
                {
                    DateTime dtResult;
                    //Si pIsFormatForDisplay: Spheres® tente de parser ce qui existe en base (stocker au format ISO)
                    //Sinon                 : Spheres® tente de parser ce qui a été saisie pour le stocker au format ISO
                    if (TypeData.IsTypeDateTime(pDataType))
                    {
                        if (pIsFormatForDisplay)
                            DtFunc.ParseDate(pValue, DtFunc.FmtISODateTime2, null);
                        else
                            DtFunc.ParseDate(pValue, DtFunc.FmtDateTime, CultureInfo.CurrentCulture);
                    }
                    else if (TypeData.IsTypeDate(pDataType))
                    {
                        if (pIsFormatForDisplay)
                        {
                            ret = DateTime.TryParseExact(pValue, DtFunc.FmtISODate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtResult);
                            //DtFunc.ParseDate(pValue, DtFunc.FmtISODate, null);
                        }
                        else
                        {
                            ret = DateTime.TryParseExact(pValue, DtFunc.FmtShortDate, CultureInfo.CurrentCulture, DateTimeStyles.None, out dtResult);
                            //DtFunc.ParseDate(pValue, DtFunc.FmtShortDate, CultureInfo.CurrentCulture);
                        }
                    }
                    else if (TypeData.IsTypeTime(pDataType))
                    {
                        if (pIsFormatForDisplay)
                            DtFunc.ParseDate(pValue, DtFunc.FmtISOTime, null);
                        else
                            DtFunc.ParseDate(pValue, DtFunc.FmtShortTime, CultureInfo.CurrentCulture);
                    }
                }
                catch (FormatException)
                {
                    ret = false;
                }
            }
            return ret;
        }
        #endregion
    }
    #endregion LstConsultData

    #region LstWhereData
    /// <summary>
    /// Classe qui représente un critère de consultation
    /// </summary>
    public class LstWhereData
    {
        #region members
        #region columnIdentifier
        private string _columnIdentifier;
        /// <summary>
        /// Obtient ou définit l'identifiant de la colonne associé au critère 
        /// </summary>
        public string columnIdentifier
        {
            get { return _columnIdentifier; }
            set { _columnIdentifier = value; }
        }
        #endregion
        #region operator
        private string _operator;
        /// <summary>
        /// Obtient ou définit l'opérateur associé au critère
        /// </summary>
        public string @operator
        {
            get { return _operator; }
            set { _operator = value; }
        }
        #endregion
        #region lstValue
        private string _lstValue;
        /// <summary>
        /// Obtient ou définit la liste des valeurs associé au critère (cette liste est formattée)
        /// </summary>
        public string lstValue
        {
            get { return _lstValue; }
            set { _lstValue = value; }
        }
        #endregion
        #region isMandatory
        private bool _isMandatory;
        /// <summary>
        /// Obtient ou définit un indicateur qui précise si le critère est obligatoire ou non
        /// </summary>
        public bool isMandatory
        {
            get { return _isMandatory; }
            set { _isMandatory = value; }
        }
        #endregion
        #region position
        private int _position;
        /// <summary>
        /// Obtient ou Définit la position d'un critère parmi la liste des critères associé à un template
        /// </summary>
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
        #endregion
        //
        #region constructor
        public LstWhereData() { }
        #endregion constructor
        //
        #region Method
        public string GetDisplayOperator()
        {
            string _operator = @operator;
            switch (_operator)
            {
                case "<":
                    _operator = "less";
                    break;
                case ">":
                    _operator = "greater";
                    break;
                case "<=":
                    _operator = "lessOrEqual";
                    break;
                case ">=":
                    _operator = "greaterOrEqual";
                    break;
            }
            string ret = Ressource.GetString(_operator, @operator);
            return ret;
        }
        #endregion

    }
    #endregion LstWhereData

    #region LstTemplateData
    /// <summary>
    /// Classe image d'un template employée pour la gestion de la consultation multi-critères
    /// </summary>
    public class LstTemplateData
    {
        #region Constants
        public const string TEMPORARYPREFIX = "*";
        #endregion

        #region Members
        public string titleDisplayName;
        public string titleOwner;
        //
        public string IDLSTTEMPLATE;
        public string IDLSTCONSULT;
        public string DISPLAYNAME;
        public string DESCRIPTION;
        public string CSSFILENAME;
        public int ROWBYPAGE;
        public string RIGHTPUBLIC;
        public string RIGHTDESK;
        public string RIGHTDEPARTMENT;
        public string RIGHTENTITY;
        /// <summary>
        /// Représente le propriétaire du template
        /// </summary>
        public int IDA;
        public DateTime DTUPD;
        public int IDAUPD;
        public DateTime DTINS;
        public int IDAINS;
        public string EXTLLINK;
        public string ROWATTRIBUT;
        public string ROWVERSION;
        public bool ISENABLEDLSTWHERE;
        public string IDADisplayName;
        public string IDAUPDDisplayName;
        public string IDAINSDisplayName;
        public bool ISDEFAULT;
        public bool ISLOADONSTART;
        public int REFRESHINTERVAL;
        #endregion Structure

        #region Accessor
        /// <summary>
        /// Obtient le nom du Template sans le prefix *
        /// </summary>
        public string IDLSTTEMPLATE_WithoutPrefix
        {
            get { return RepositoryWeb.RemoveTemporaryPrefix(IDLSTTEMPLATE); }
        }
        /// <summary>
        /// Obtient le true si le Template commence par le prefix *
        /// </summary>
        public bool isTemporary
        {
            get { return RepositoryWeb.IsTemporary(IDLSTTEMPLATE); }
        }
        /// <summary>
        /// Obtient le true si le rafraîssement de page est spécifé
        /// </summary>
        public bool IsRefreshIntervalSpecified
        {
            get { return (REFRESHINTERVAL >= RepositoryWeb.MinRefreshInterval); }
        }
        #endregion

        #region Constructor(s)
        public LstTemplateData()
        {
            titleDisplayName = string.Empty;
            titleOwner = string.Empty;
            ISENABLEDLSTWHERE = true;
        }
        #endregion

        #region public Load
        /// <summary>
        /// Initialise la classe LSTTEMPLATE
        /// </summary>
        /// <param name="pIDLSTTEMPLATE">string: ID du template</param>
        /// <param name="pIDA">int: ID du propriétaire du template</param>
        /// <returns>bool: true si load OK</returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public bool Load(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
        {
            bool isOwner = (pIdA == SessionTools.Collaborator_IDA);
            bool ret = false;

            IDLSTCONSULT = pIdLstConsult;
            IDLSTTEMPLATE = pIdLstTemplate;
            IDA = pIdA;

            ISDEFAULT = false;

            DataParameters dbParam = null;
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += @" lt.IDLSTTEMPLATE, lt.IDLSTCONSULT, lt.DISPLAYNAME, lt.DESCRIPTION, lt.CSSFILENAME, 
                                lt.ROWBYPAGE, lt.RIGHTPUBLIC, lt.RIGHTDESK, lt.RIGHTDEPARTMENT, lt.RIGHTENTITY, lt.DTUPD, 
                                lt.IDAUPD, lt.DTINS, lt.IDAINS, lt.EXTLLINK, lt.ROWATTRIBUT, 
                                lt.ROWVERSION,lt.ISENABLEDLSTWHERE,lt.IDA , lt.ISLOADONSTART, lt.REFRESHINTERVAL,
                                a1.DISPLAYNAME, a2.DISPLAYNAME, a3.DISPLAYNAME " + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a1 on (a1.IDA = lt.IDA)" + Cst.CrLf;
            SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a2 on (a2.IDA = lt.IDAUPD)" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a3 on (a3.IDA = lt.IDAINS)" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, "lt", out dbParam);

            IDataReader dr = null;
            try
            {
                QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
                if (dr.Read())
                {
                    int i = -1;
                    i++; IDLSTTEMPLATE = dr.GetValue(i).ToString();
                    i++; IDLSTCONSULT = dr.GetValue(i).ToString();
                    i++; DISPLAYNAME = dr.GetValue(i).ToString();
                    i++; DESCRIPTION = dr.GetValue(i).ToString();
                    i++; CSSFILENAME = dr.GetValue(i).ToString();
                    i++; ROWBYPAGE = Convert.ToInt32((dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : "0"));
                    i++; RIGHTPUBLIC = dr.GetValue(i).ToString();
                    i++; RIGHTDESK = dr.GetValue(i).ToString();
                    i++; RIGHTDEPARTMENT = dr.GetValue(i).ToString();
                    i++; RIGHTENTITY = dr.GetValue(i).ToString();
                    i++; DTUPD = Convert.ToDateTime(dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i) : dr.GetValue(i + 2));
                    i++; IDAUPD = Convert.ToInt32((dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : dr.GetValue(i + 2).ToString()));
                    i++; DTINS = Convert.ToDateTime(dr.GetValue(i));
                    i++; IDAINS = Convert.ToInt32((dr.GetValue(i)));
                    i++; EXTLLINK = dr.GetValue(i).ToString();
                    i++; ROWATTRIBUT = dr.GetValue(i).ToString();
                    i++; ROWVERSION = dr.GetValue(i).ToString();
                    i++; ISENABLEDLSTWHERE = Convert.ToBoolean(dr.GetValue(i));
                    i++; IDA = Convert.ToInt32((dr.GetValue(i)));
                    i++; ISLOADONSTART = Convert.ToBoolean(dr.GetValue(i));
                    i++; REFRESHINTERVAL = (dr.GetValue(i) == Convert.DBNull ? 0 : Convert.ToInt32(dr.GetValue(i)));
                    i++; IDADisplayName = dr.GetValue(i).ToString();
                    i++; IDAUPDDisplayName = (dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : dr.GetValue(i + 1).ToString());
                    i++; IDAINSDisplayName = dr.GetValue(i).ToString();
                    //
                    titleDisplayName = IDLSTTEMPLATE + (IDLSTTEMPLATE == DISPLAYNAME ? string.Empty : " - " + DISPLAYNAME);
                    titleOwner = (isOwner ? string.Empty : " [" + Ressource.GetString("Owner") + ": " + IDADisplayName + "]");
                    //
                    ret = true;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            //
            if (isOwner)
            {
                string name = (isTemporary ? IDLSTTEMPLATE_WithoutPrefix : IDLSTTEMPLATE);
                SQLSelect = new StrBuilder();
                SQLSelect += SQLCst.SELECT + "1" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, name, IDA, null, out dbParam);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
                object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
                ISDEFAULT = (null != obj);
            }
            //
            return ret;

        }
        #endregion Load

        #region public Insert
        /// <summary>
        /// Fonction d'insert d'un template
        /// </summary>
        /// <param name="pForcedIdA">int: ID du propriétaire si celui si est different du current</param>
        /// <returns>(int) Cst.ErrLevel</returns>
        public void Insert(string pCS)
        {
            Insert(pCS, SessionTools.Collaborator_IDA);
        }
        public void Insert(string pCS, int pIdA)
        {
            //
            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + @" (
				    IDLSTTEMPLATE, IDLSTCONSULT, DISPLAYNAME, DESCRIPTION, CSSFILENAME,
				    ROWBYPAGE, RIGHTPUBLIC, RIGHTDESK, RIGHTDEPARTMENT, RIGHTENTITY, DTINS,
				    IDAINS, EXTLLINK, IDA, ISENABLEDLSTWHERE, ISLOADONSTART, REFRESHINTERVAL
                    ) values (  
				    @IDLSTTEMPLATE, @IDLSTCONSULT, @DISPLAYNAME, @DESCRIPTION, @CSSFILENAME,
				    @ROWBYPAGE, @RIGHTPUBLIC, @RIGHTDESK, @RIGHTDEPARTMENT, @RIGHTENTITY, " + DataHelper.SQLGetDate(pCS) + @",
				    @IDAINS, @EXTLLINK, @IDA, @ISENABLEDLSTWHERE, @ISLOADONSTART, @REFRESHINTERVAL
                    )";
            IDA = pIdA;
            IDAINS = SessionTools.Collaborator_IDA;
            //                       
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDLSTTEMPLATE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), IDLSTTEMPLATE);
            dp.Add(new DataParameter(pCS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), IDLSTCONSULT);
            dp.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), DISPLAYNAME);
            dp.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), DESCRIPTION);
            dp.Add(new DataParameter(pCS, "CSSFILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), CSSFILENAME);
            dp.Add(new DataParameter(pCS, "ROWBYPAGE", DbType.Int32), ROWBYPAGE);
            dp.Add(new DataParameter(pCS, "RIGHTPUBLIC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTPUBLIC);
            dp.Add(new DataParameter(pCS, "RIGHTDESK", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDESK);
            dp.Add(new DataParameter(pCS, "RIGHTDEPARTMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDEPARTMENT);
            dp.Add(new DataParameter(pCS, "RIGHTENTITY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTENTITY);
            dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), IDA);
            dp.Add(new DataParameter(pCS, "IDAINS", DbType.Int32), IDAINS);
            dp.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), EXTLLINK);
            dp.Add(new DataParameter(pCS, "ISENABLEDLSTWHERE", DbType.Boolean), ISENABLEDLSTWHERE);
            dp.Add(new DataParameter(pCS, "ISLOADONSTART", DbType.Boolean), ISLOADONSTART);
            dp.Add(new DataParameter(pCS, "REFRESHINTERVAL", DbType.Int32), REFRESHINTERVAL);
            //
            int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlQuery, dp.GetArrayDbParameter());
            //
            if (nbRow == 1)
                Update_LstTemplateDef(pCS, IDLSTCONSULT);
            //

        }
        #endregion
        #region public Update
        /// <summary>
        /// Fonction d'update du template
        /// </summary>
        /// <returns>(int) Cst.ErrLevel</returns>
        public bool Update(string pCS)
        {
            bool ret = false;

            IDAUPD = SessionTools.Collaborator_IDA;

            DataParameters dbParam = null;
            string SQLUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + @" set 
                                    DISPLAYNAME        = @DISPLAYNAME,
                                    DESCRIPTION        = @DESCRIPTION,
                                    CSSFILENAME        = @CSSFILENAME,
                                    ROWBYPAGE          = @ROWBYPAGE,
                                    RIGHTPUBLIC        = @RIGHTPUBLIC,
                                    RIGHTDESK          = @RIGHTDESK,
                                    RIGHTDEPARTMENT    = @RIGHTDEPARTMENT,
                                    RIGHTENTITY        = @RIGHTENTITY,
                                    DTUPD              = " + DataHelper.SQLGetDate(pCS) + @",
                                    IDAUPD             = @IDAUPD,
                                    EXTLLINK           = @EXTLLINK,
                                    ROWATTRIBUT        = @ROWATTRIBUT,
                                    ISENABLEDLSTWHERE  = @ISENABLEDLSTWHERE,
                                    ISLOADONSTART      = @ISLOADONSTART,
                                    REFRESHINTERVAL    = @REFRESHINTERVAL";
            SQLUpdate += Cst.CrLf + SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out dbParam);

            dbParam.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), DISPLAYNAME);
            dbParam.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), DESCRIPTION);
            dbParam.Add(new DataParameter(pCS, "CSSFILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), CSSFILENAME);
            dbParam.Add(new DataParameter(pCS, "ROWBYPAGE", DbType.Int32), ROWBYPAGE);
            dbParam.Add(new DataParameter(pCS, "RIGHTPUBLIC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTPUBLIC);
            dbParam.Add(new DataParameter(pCS, "RIGHTDESK", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDESK);
            dbParam.Add(new DataParameter(pCS, "RIGHTDEPARTMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDEPARTMENT);
            dbParam.Add(new DataParameter(pCS, "RIGHTENTITY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTENTITY);
            dbParam.Add(new DataParameter(pCS, "IDAUPD", DbType.Int32), IDAUPD);
            dbParam.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), EXTLLINK);
            dbParam.Add(new DataParameter(pCS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN), ROWATTRIBUT);
            dbParam.Add(new DataParameter(pCS, "ISENABLEDLSTWHERE", DbType.Boolean), ISENABLEDLSTWHERE);
            dbParam.Add(new DataParameter(pCS, "ISLOADONSTART", DbType.Boolean), ISLOADONSTART);
            dbParam.Add(new DataParameter(pCS, "REFRESHINTERVAL", DbType.Int32), REFRESHINTERVAL);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
            int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            if (nbRow == 1)
            {
                ret = true;
                Update_LstTemplateDef(pCS, IDLSTCONSULT);
            }
            return ret;
        }
        #endregion

        #region public SetIsEnabledLstWhere
        /// <summary>
        /// Mise à jour du champ ISENABLEDLSTWHERE dans la table LSTTEMPLATE
        /// </summary>
        /// <param name="pConnectionString">string: connectionString</param>
        /// <param name="pIdA">int: ID du proprietaire du template</param>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <param name="pIdLstConsult">string: ID de la consultation</param>
        /// <param name="pValue">bool: valeur à affecter à ISENABLEDLSTWHERE</param>
        /// <returns>(int) Cst.ErrLevel</returns>
        public Cst.ErrLevel SetIsEnabledLstWhere(string pCS, int pIdA, string pIdLstTemplate, string pIdLstConsult, bool pValue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            ISENABLEDLSTWHERE = pValue;

            if (RepositoryWeb.IsReferential(pIdLstTemplate))
            {
                //On delete celui deja defini
                DataParameters dbParam = null;
                StrBuilder SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam) + Cst.CrLf;

                QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

                LstTemplateData newTemplate = new LstTemplateData();
                newTemplate.IDA = pIdA;
                newTemplate.IDLSTTEMPLATE = pIdLstTemplate;
                newTemplate.IDLSTCONSULT = pIdLstConsult;
                newTemplate.DISPLAYNAME = pIdLstTemplate;
                newTemplate.DESCRIPTION = pIdLstTemplate;
                newTemplate.CSSFILENAME = string.Empty;
                newTemplate.ROWBYPAGE = 0;
                newTemplate.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
                newTemplate.RIGHTDESK = RightsTypeEnum.NONE.ToString();
                newTemplate.RIGHTDEPARTMENT = RightsTypeEnum.NONE.ToString();
                newTemplate.RIGHTENTITY = RightsTypeEnum.NONE.ToString();
                newTemplate.EXTLLINK = string.Empty;
                newTemplate.ISENABLEDLSTWHERE = ISENABLEDLSTWHERE;
                newTemplate.ISLOADONSTART = ISLOADONSTART;
                newTemplate.REFRESHINTERVAL = REFRESHINTERVAL;

                newTemplate.Insert(pCS);
            }
            else
            {
                DataParameters dbParam = null;
                StrBuilder SQLUpdate = new StrBuilder();
                SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLUpdate += @" Set ISENABLEDLSTWHERE = @ISENABLEDLSTWHERE";
                SQLUpdate += Cst.CrLf + SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam);

                dbParam.Add(new DataParameter(pCS, "ISENABLEDLSTWHERE", DbType.Boolean), ISENABLEDLSTWHERE);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

                if (nbRow == 1)
                    ret = Cst.ErrLevel.SUCCESS;
                else
                    ret = Cst.ErrLevel.ABORTED;
            }
            return ret;
        }
        #endregion SetIsEnabledLstWhere()
        #region public SetEnabledLstWhere
        /// <summary>
        /// Update LstWhere, met à jour la colonne ISENABLED du critère en position {pPosition}   
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="position"></param>
        /// <param name="pIsEnabled"></param>
        public void SetEnabledLstWhere(string pCS, int pIdA, string pIdLstTemplate, string pIdLstConsult, int pPosition, bool pIsEnabled)
        {
            DataParameters dbParam = null;
            StrBuilder SQLUpdate = new StrBuilder();
            SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTWHERE + Cst.CrLf;
            SQLUpdate += "set ISENABLED = @ISENABLED" + Cst.CrLf;
            SQLUpdate += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam);
            SQLUpdate += SQLCst.AND + "POSITION=@POSITION";

            dbParam.Add(new DataParameter(pCS, "ISENABLED", DbType.Boolean), pIsEnabled);
            dbParam.Add(new DataParameter(pCS, "POSITION", DbType.Int32), pPosition);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
            int nbRow = DataHelper.ExecuteNonQuery(qryParameters.cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
        }
        #endregion

        #region public HasUserRight
        /// <summary>
        /// Retourne true si un user a un droit particulier sur le template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUser">Représente le user</param>
        /// <param name="pTypeRight">Représente le droit</param>
        /// <returns></returns>
        public bool HasUserRight(string pCS, User pUser, RightsTypeEnum pTypeRight)
        {
            bool ret = false;
            DataRights rights = new DataRights(pCS, this.IDA);

            RightsTypeEnum rightPublic = RightsTypeEnum.NONE;
            if (Enum.IsDefined(typeof(RightsTypeEnum), RIGHTPUBLIC))
                rightPublic = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), RIGHTPUBLIC);

            RightsTypeEnum rightEntity = RightsTypeEnum.NONE;
            if (Enum.IsDefined(typeof(RightsTypeEnum), RIGHTENTITY))
                rightEntity = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), RIGHTENTITY);

            RightsTypeEnum rightDepartment = RightsTypeEnum.NONE;
            if (Enum.IsDefined(typeof(RightsTypeEnum), RIGHTDEPARTMENT))
                rightDepartment = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), RIGHTDEPARTMENT);

            RightsTypeEnum rightDesk = RightsTypeEnum.NONE;
            if (Enum.IsDefined(typeof(RightsTypeEnum), RIGHTDESK))
                rightDesk = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), RIGHTDESK);

            rights.publicRight = rightPublic;
            rights.entityRight = rightEntity;
            rights.departmentRight = rightDepartment;
            rights.deskRight = rightDesk;

            ret = rights.HasUserRight(pCS, pUser, pTypeRight);

            return ret;
        }
        #endregion

        #region IsUserOwner
        /// <summary>
        /// Obtient true si l'acteur est le propriétaire du template
        /// </summary>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        public bool IsUserOwner(int pIdA)
        {
            return (IDA == pIdA);
        }
        #endregion

        #region private Update_LstTemplateDef
        /// <summary>
        /// Defini le template current comme etant par defaut -> maj de la table LSTTEMPLATEDEF
        /// </summary>
        /// <returns>(int) Cst.ErrLevel</returns>
        private void Update_LstTemplateDef(string pCS, string pIdLstConsult)
        {
            if (ISDEFAULT)
            {
                //On delete l'eventuel ancien defaut
                DataParameters dbParam = null;
                string SQLDelete = SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + "(IDLSTCONSULT=@IDLSTCONSULT) and (IDA=@IDA)";

                dbParam = new DataParameters();
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), IDLSTCONSULT);
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

                //On insert le nouveau
                dbParam = null;
                string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + @"(IDLSTTEMPLATE,IDLSTCONSULT,IDA,DTUPD,IDAUPD,DTINS,IDAINS)";
                SQLInsert += " values (@IDLSTTEMPLATE,@IDLSTCONSULT,@IDA," + DataHelper.SQLGetDate(pCS) + ",@IDAUPD," + DataHelper.SQLGetDate(pCS) + @",@IDAINS)";

                dbParam = new DataParameters();
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), IDLSTCONSULT);
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), IDA);
                dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), IDA);
                dbParam.Add(new DataParameter(pCS, "IDLSTTEMPLATE", DbType.AnsiString, 64));
                if (RepositoryWeb.ExistsTemplate(pCS, pIdLstConsult, IDLSTTEMPLATE_WithoutPrefix, IDA))
                    dbParam["IDLSTTEMPLATE"].Value = IDLSTTEMPLATE_WithoutPrefix;
                else
                    dbParam["IDLSTTEMPLATE"].Value = IDLSTTEMPLATE;

                qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }
            else
            {
                //On delete au cas ou il serait defini par defaut
                DataParameters dbParam = null;
                StrBuilder SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                //SQLDelete += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, null, out dbParam;
                SQLDelete += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out dbParam);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }

        }
        #endregion

        #region Childs Functions
        #region CopyChilds
        /// <summary>
        /// Copie les tables 'enfants' (LSTSELECT, LSTORDERBY, LSTWHERE) d'un template sur un autre template
        /// </summary>
        /// <param name="pSourceIdLstTemplate">string: ID du template Source</param>
        /// <param name="pSourceIdA">int: ID du propriétaire du template Source</param>
        /// <param name="pDestIdLstTemplate">string: ID du template Destination</param>
        /// <param name="pForcedIdA">int: ID du propriétaire du template destinataire si celui-ci n'est pas l'user current</param>
        public static void CopyChilds(string pCS, string pIdLstConsult, string pSourceIdLstTemplate, int pSourceIdA, string pDestIdLstTemplate, int pForcedIdA)
        {
            if (pForcedIdA == 0)
                pForcedIdA = SessionTools.Collaborator_IDA;

            Cst.OTCml_TBL[] childTableName = { Cst.OTCml_TBL.LSTSELECT, Cst.OTCml_TBL.LSTORDERBY, Cst.OTCml_TBL.LSTWHERE };
            for (int i = 0; i < childTableName.Length; i++)
            {
                RepositoryWeb.DeleteChild(pCS, childTableName[i], pIdLstConsult, pDestIdLstTemplate, SessionTools.Collaborator_IDA, false);

                string tableName = childTableName[i].ToString();

                DataParameters dbParam = null;
                StrBuilder SQLInsert = new StrBuilder();
                SQLInsert += SQLCst.INSERT_INTO_DBO + tableName + "(" + AllColsForTableChild(tableName, null, null, -1) + ")" + Cst.CrLf;
                SQLInsert += "(";
                SQLInsert += SQLCst.SELECT + AllColsForTableChild(tableName, "ls", pDestIdLstTemplate, pForcedIdA) + Cst.CrLf;
                SQLInsert += SQLCst.FROM_DBO + tableName + " ls" + Cst.CrLf;
                SQLInsert += SQLCst.WHERE + RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pSourceIdLstTemplate, pSourceIdA, "ls", out dbParam);
                SQLInsert += ")";

                QueryParameters qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }
        }
        #endregion CopyChilds
        #endregion Childs Functions

        #region public GetSQLClause_PK_LSTTEMPLATE2
        /// <summary>
        /// Fonctions pour obtenir la clause where pour identifier un template par rapport à sa clé (GetSQLClause_PK_LSTTEMPLATE)
        /// </summary>
        public string GetSQLClause_PK_LSTTEMPLATE2(string pCS, string pDBAlias, out DataParameters opDataParameters)
        {
            return RepositoryWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, pDBAlias, out opDataParameters);
        }
        #endregion GetSQLClause_PK_LSTTEMPLATE

        #region public static GetSQLWhereTemporaryOrNot
        /// <summary>
        /// Fonctions qui renvoient la clause where pour identifier si un template est temporaire ou non (GetSQLWhereTemporaryOrNot)
        /// </summary>
        public static string GetSQLWhereNotTemporary()
        {
            return GetSQLWhereTemporaryOrNot(null, true);
        }
        /// <summary>
        /// Surcharge de GetSQLWhereTemporaryOrNot(string aliasTableName, bool isNot)
        /// </summary>
        /// <param name="pAliasTableName">string: alias pour prefixer les colonnes</param>
        /// <returns>string: clause where</returns>
        public static string GetSQLWhereNotTemporary(string pAliasTableName)
        {
            return GetSQLWhereTemporaryOrNot(pAliasTableName, true);
        }
        #endregion
        #region public static GetSQLWhereTemporary
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetSQLWhereTemporary()
        {
            return GetSQLWhereTemporaryOrNot(null, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAliasTableName">string: alias pour prefixer les colonnes</param>
        /// <returns>string: clause where</returns>
        public static string GetSQLWhereTemporary(string pAliasTableName)
        {
            return GetSQLWhereTemporaryOrNot(pAliasTableName, false);
        }
        #endregion

        #region public static GetSQLClause_PK_LSTTEMPLATE
        /// <summary>
        /// Obtient la clause where pour identifier un template par rapport à sa clé (IDLSTTEMPLATE et IDA)
        /// </summary>
        /// <param name="pIDLSTTEMPLATE">ID du template</param>
        /// <param name="pIDA">ID du propriétaire du template</param>
        /// <param name="pDBAlias">alias pour prefixer les colonnes</param>
        /// <returns>clause where</returns>
        [Obsolete("Use GetSQLClause_PK_LSTTEMPLATE2", true)]
        public static string GetSQLClause_PK_LSTTEMPLATE(string pIdLstConsult, string pIDLstTemplate, int pIdA, string pDbAlias)
        {
            string alias = (StrFunc.IsFilled(pDbAlias) ? pDbAlias + "." : string.Empty);

            string SQLWhere = "(";
            SQLWhere += "(" + alias + "IDLSTTEMPLATE=" + DataHelper.SQLString(pIDLstTemplate) + ")";
            //PL 20110923 Add Trim() pour alléger la lecture du SQL
            SQLWhere += SQLCst.AND + "(" + alias + "IDLSTCONSULT=" + DataHelper.SQLString(pIdLstConsult.Trim()) + ")";
            SQLWhere += SQLCst.AND + "(" + alias + "IDA=" + pIdA.ToString() + ")";
            SQLWhere += ")" + Cst.CrLf;

            return SQLWhere;
        }
        #endregion GetSQLClause_PK_LSTTEMPLATE

        #region private static AllColsForTableChild
        /// <summary>
        /// Récupération de la liste des colonnes pour une table sous la forme d'unne string utilisable dans un ordre SQL Select (ex: COL1,COL2,COL3,...)
        /// </summary>
        /// <param name="pTableName">Nom de la table: LSTSELECT|LSTWHERE|LSTORDERBY</param>
        /// <param name="pAliasTableName">Alias éventuel de la table</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <returns>Liste des colonnes</returns>
        private static string AllColsForTableChild(string pTableName, string pAliasTableName, string pIdLstTemplate, int pIdA)
        {
            string alias = (StrFunc.IsFilled(pAliasTableName) ? pAliasTableName + "." : string.Empty);

            string selectQuery = alias + "IDLSTCONSULT,";
            selectQuery += (pIdLstTemplate != null ? DataHelper.SQLString(pIdLstTemplate) : alias + "IDLSTTEMPLATE") + ", ";
            selectQuery += (pIdA >= 0 ? pIdA.ToString() : alias + "IDA") + ", ";
            selectQuery += alias + "IDLSTCOLUMN,";
            selectQuery += alias + "TABLENAME,";
            selectQuery += alias + "COLUMNNAME,";
            selectQuery += alias + "ALIAS,";
            selectQuery += alias + "POSITION";

            if (pTableName == Cst.OTCml_TBL.LSTWHERE.ToString())
            {
                selectQuery += ", ";
                selectQuery += alias + "OPERATOR,";
                selectQuery += alias + "LSTVALUE,";
                selectQuery += alias + "LSTIDVALUE,";
                selectQuery += alias + "ISENABLED,";
                selectQuery += alias + "ISMANDATORY";
            }
            if (pTableName == Cst.OTCml_TBL.LSTORDERBY.ToString())
            {
                selectQuery += ",";
                selectQuery += alias + "ASCDESC,ISGROUPBY,ISGROUPINGSET,GROUPINGSET";
            }
            return selectQuery + Cst.CrLf;
        }
        #endregion
        #region private static GetSQLWhereTemporaryOrNot
        /// <summary>
        /// Fonction qui renvoie la clause where pour identifier si un template est temporaire ou non
        /// </summary>
        /// <param name="pAliasTableName">string: alias pour prefixer les colonnes</param>
        /// <param name="isNot">bool: indique si la clause est temporaire or not temporaire</param>
        /// <returns></returns>

        private static string GetSQLWhereTemporaryOrNot(string pAliasTableName, bool pIsNot)
        {
            string alias = (StrFunc.IsEmpty(pAliasTableName) ? string.Empty : pAliasTableName + ".");
            return "(" + alias + "IDLSTTEMPLATE " + (pIsNot ? SQLCst.NOT : string.Empty) + SQLCst.LIKE + DataHelper.SQLString(TEMPORARYPREFIX + "%") + ")";
        }
        #endregion
    }
    #endregion LstTemplateData

}
