#region using directives
using System;
using System.Collections.Generic;  
using System.Collections;
using System.Data;
using System.Globalization;
using System.Linq; 

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Rights;

using EFS.SpheresIO; 

using EfsML;
using EfsML.DynamicData; 
using EfsML.Enum;
using EfsML.Business;

using Tz = EFS.TimeZone;

#endregion using directives

namespace EFS.Referential
{
    #region class LstConsult
    /// <summary>
    /// Classe employée pour la gestion de la consultation multi-critères 
    /// </summary>
    public class LstConsult
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
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200602 [25370] Add 
        public DataTable dtLstParam;
        private DataTable dtLstConsultAlias;
        private bool m_isMultiTableSpecified;
        private bool m_isMultiTable;
        private string m_mainTableName;
        /// <summary>
        /// 
        /// </summary>
        private readonly string _IdMenu;
        private readonly string _consultXML;
        #endregion Private properties

        #region Membres
        public DataTable dtLstWhere;
        public DataTable dtLstSelectAvailable;
        public DataTable dtLstSelectedCol;
        public LstTemplate template;
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
                string ret;
                if (StrFunc.IsEmpty(_IdMenu))
                {
                    //Consultation
                    ret = string.Empty;
                    if (ReferentialWeb.IsReferential(IdLstConsult))
                    {
                        ret = ReferentialShortIdConsult();
                    }
                    else
                    {
                        //PL 20100212 Add switch (Afin d'avoir des titres cohérents sur les consultations ouvertes depuis les boutons "...")
                        switch (IdLstConsult)
                        {
                            case "ASSET_ETD_EXPANDED":
                                ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_ATTRIB_ASSET";
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

                ret = Ressource.GetMenu_Fullname(ret, ret);

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
        // EG 20180426 Analyse du code Correction [CA2202]
        public LstConsult(string pCS, string pIdLstConsult, string pIdMenu)
        {
            // RD 20110704 [17501] / Coloration LST
            //m_isMultiTableSpecified = false;
            m_isMultiTable = true;
            m_mainTableName = SQLCst.TBLMAIN;

            IdLstConsult = null;

            _IdMenu = pIdMenu;

            //verification existance du IDLSTCONSULT
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "CONSULTXML" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDLSTCONSULT=@IDLSTCONSULT";

            DataParameters dbParam = new DataParameters(); 
            dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), pIdLstConsult);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);

            bool isOk = false;
            using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(pCS, 1, 1), CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
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
                string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString();
                SQLInsert += @" (IDLSTCONSULT,DISPLAYNAME,DESCRIPTION) values (@IDLSTCONSULT,@DISPLAYNAME,@DESCRIPTION)";

                dbParam = new DataParameters();
                dbParam.Add(new DataParameter(pCS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdLstConsult);
                dbParam.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pIdLstConsult);
                dbParam.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), "Used by Spheres for Templates on referentials");

                qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
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
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void LoadLstAlias(string pCS)
        {
            string sqlSelect = String.Format(@"select la.ALIAS, la.TABLENAME, la.DISPLAYNAME, ltd.RDBMS, ltd.COMMAND, ltd.TABLEDETXML
            from dbo.LSTALIAS la
            inner join dbo.LSTCONSULTALIAS lca on (lca.ALIAS=la.ALIAS)
            inner join dbo.LSTCONSULT lc on (lc.IDLSTCONSULT=lca.IDLSTCONSULT)
            left outer join dbo.LSTTABLEDET ltd on (ltd.TABLENAME=la.TABLENAME)
            where {0}
            order by la.ALIAS", ReferentialWeb.GetSQLClause_Consult(IdLstConsult, "lc")) + Cst.CrLf;

            dtLstAlias = DataHelper.ExecuteDataTable(pCS, sqlSelect);
        }

        /// <summary>
        /// Charge les données relatives à la table LSTCONSULTALIAS pour la consultation concernée
        /// </summary>
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void LoadLstConsultAlias(string pCS)
        {
            string sqlSelect = String.Format(@"select lca.*
            from dbo.LSTCONSULTALIAS lca
            where {0}
            order by lca.POSITION", ReferentialWeb.GetSQLClause_Consult(IdLstConsult, "lca")) + Cst.CrLf;

            dtLstConsultAlias = DataHelper.ExecuteDataTable(pCS, sqlSelect);
        }
        
        /// <summary>
        /// Charge les données relatives à la table LST_ALIAS pour la consultation courante
        /// NB: On ne charge que les ALIAS nécessaires à l'élaboration ultérieure de la requête
        /// </summary>
        private void LoadLstConsultAliasSelected(string pCS, bool pIsConsultWithDynamicArgs)
        {
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
            SQLJoin += SQLCst.WHERE + ReferentialWeb.GetSQLClause_Consult(IdLstConsult, "lca");
            SQLJoin += SQLCst.AND + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "l0", out DataParameters dbParam);

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
                SQLSelect += SQLCst.WHERE + ReferentialWeb.GetSQLClause_Consult(IdLstConsult, "lca");
                SQLSelect += SQLCst.AND + "(lca.POSITION!=1)";
            }
            SQLSelect += Cst.CrLf + SQLCst.ORDERBY + "ALIASPOSITION,JOINPOSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            dtLstConsultAliasSelected = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

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
                            if (sqlJoin.IndexOf(SQLCst.ON, pos_ON+1) == -1)
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
            StrBuilder SQLSelect = new StrBuilder();
            SQLSelect += SQLCst.SELECT + "1" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTSELECT.ToString() + " ls" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "ls", out DataParameters dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()); ;
            return (null != obj);
        }

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
            // FI 20200602 [25370] Alimentation de LSTParam 
            LoadLstParam(pCS);
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
            template = new LstTemplate();

            //PL 20150601 GUEST New feature
            if (!string.IsNullOrEmpty(pIdLstTemplate))
            {
                template.Load(pCS, IdLstConsult, pIdLstTemplate, pIdAOwner);

                #region S'il n'existe aucune colonne pour l'affichage, alors on insere par defaut les colonnes d'un Template ou de la table principale de la consultation
                if (!ReferentialWeb.IsReferential(IdLstConsult))
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

                    SQLQuery += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
                    SQLQuery += SQLCst.WHERE + "lw.IDLSTCONSULT=" + DataHelper.SQLString(IdLstConsult);
                    SQLQuery += SQLCst.AND + "lw.IDLSTTEMPLATE=" + DataHelper.SQLString(template.IDLSTTEMPLATE);
                    SQLQuery += SQLCst.AND + "lw.IDA=" + DataHelper.SQLString(template.IDA.ToString());
                    SQLQuery += SQLCst.AND + "(lw.TABLENAME=lcw.TABLENAME and lw.COLUMNNAME=lcw.COLUMNNAME)";
                    SQLQuery += ")";
                    _ = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);
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
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20200226 [25077] RDBMS : Correction gestion EXCLUDE et INCLUDE COLUMNNAME
        public DataTable LoadLstSelectedCol(string pCS, int pResMulti_Index)
        {
            string sqlWhere = template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "ls", out DataParameters dbParam);

            string sqlSelect = $@"select ls.ALIAS, lco.TABLENAME,lco.COLUMNNAME,lco.DISPLAYNAME,lco.COLUMNNAME,lco.COLUMNNAMEREF as COLUMNREF,
            lco.SQLSELECT,lco.DATATYPE,lco.ISRESOURCE,lco.SCALE,lco.SQLORDER,lco.AGGREGATE,lco.SQLGROUPBY,
            case when lccd.COLUMNXML is null then lco.COLUMNXML else lccd.COLUMNXML end as COLUMNXML,
            lcaj.IDLSTJOIN,isnull(isnull(lcajheader1.IDLSTJOIN,lcajheader0.IDLSTJOIN),lcaj.IDLSTJOIN) as ALIASDISPLAYNAME,
            lob.ISGROUPBY,lob.ISGROUPINGSET,lob.GROUPINGSET
            from dbo.LSTSELECT ls
            inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT=@IDLSTCONSULT and lca.ALIAS=ls.ALIAS)

            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME=ls.TABLENAME) and (lco.COLUMNNAME=ls.COLUMNNAME) and
                ((lca.EXCLUDECOLUMNNAME is null) or ((';' || lca.EXCLUDECOLUMNNAME || ';') not like ('%;' || lco.COLUMNNAME || ';%'))) and 
                ((lca.INCLUDECOLUMNNAME is null) or ((';' || lca.INCLUDECOLUMNNAME || ';') like ('%;' || lco.COLUMNNAME || ';%'))) 
            inner join dbo.LSTCONSALIASJOIN lcaj on (lcaj.IDLSTCONSULT=@IDLSTCONSULT) and (lcaj.ALIAS=ls.ALIAS) and 
                (lcaj.POSITION = (
	                select max(lcaj2.POSITION) 
	                from dbo.LSTCONSALIASJOIN lcaj2 
	                where (lcaj2.IDLSTCONSULT=lcaj.IDLSTCONSULT) and (lcaj2.ALIAS=lcaj.ALIAS)
                ))
            left outer join dbo.LSTCONSCOLUMNDET lccd on (lccd.TABLENAME=lco.TABLENAME) and (lccd.COLUMNNAME=lco.COLUMNNAME) and (lccd.IDLSTCONSULT=@IDLSTCONSULT)
            left outer join dbo.LSTORDERBY lob on (lob.IDLSTTEMPLATE=ls.IDLSTTEMPLATE) and (lob.IDA=ls.IDA) and (lob.IDLSTCONSULT=ls.IDLSTCONSULT) and 
                (lob.ALIAS=ls.ALIAS) and (lob.TABLENAME=ls.TABLENAME) and (lob.COLUMNNAME=ls.COLUMNNAME)

            /* Find optional item in header in LSTCONSULTALIAS, and in LSTCONSALIASJOIN  (position 0 or 1) */
            left outer join dbo.LSTCONSULTALIAS lcaheader on (lcaheader.IDLSTCONSULT=@IDLSTCONSULT and lcaheader.ALIAS=lca.ALIASHEADER)
            left outer join dbo.LSTCONSALIASJOIN lcajheader1 on (lcajheader1.IDLSTCONSULT=@IDLSTCONSULT and lcajheader1.ALIAS=lcaheader.ALIAS and lcajheader1.POSITION=1)
            left outer join dbo.LSTCONSALIASJOIN lcajheader0 on (lcajheader0.IDLSTCONSULT=@IDLSTCONSULT and lcajheader0.ALIAS=lcaheader.ALIAS and lcajheader0.POSITION=0)
            where {sqlWhere}
            order by ls.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dbParam);
            dtLstSelectedCol = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            
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
        /// EG 20180703 PERF (Step3 : Exclusion des colonnes en double) 
        public void LoadLstSelectAvailable(string pCS)
        {
            // FI 20201201 [XXXXX] call LoadLstSelectAvailable
            dtLstSelectAvailable = LoadLstSelectAvailable(pCS, IdLstConsult, template.IDLSTTEMPLATE, template.IDA);
        }

        /// <summary>
        ///  Retourne les colonnes disponibles pour selection 
        /// </summary>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Refactoring 
        public DataTable FilterLstSelectAvailableForSelect()
        {
            return FilterLstSelectAvailable(FilterLstSelectAvailableEnum.select);
        }

        /// <summary>
        ///  Retourne les colonnes disponibles pour critère
        /// </summary>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Refactoring 
        public DataTable FilterLstSelectAvailableForCriteria()
        {
            return FilterLstSelectAvailable(FilterLstSelectAvailableEnum.criteria);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20201201 [XXXXX] Refactoring 
        private enum FilterLstSelectAvailableEnum
        {
            /// <summary>
            /// colonnes disponibles pour les filtres
            /// </summary>
            criteria,
            /// <summary>
            /// colonnes disponibles pour selection et tri 
            /// </summary>
            select
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFilter"></param>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Refactoring 
        private DataTable FilterLstSelectAvailable(FilterLstSelectAvailableEnum pFilter)
        {
            if (null == dtLstSelectAvailable)
                throw new NullReferenceException("dtLstSelectAvailable is null. Please call LoadLstSelectAvailable");

            string predicatExclude = string.Empty;
            switch (pFilter)
            {
                case FilterLstSelectAvailableEnum.criteria:
                    predicatExclude = "<IsHideInCriteria>true";
                    break;
                case FilterLstSelectAvailableEnum.select:
                    predicatExclude = "<IsHideInDataGrid>true";
                    break;
                default:
                    throw new NotImplementedException($"{pFilter} is not implemented;");
            }

            DataTable ret = dtLstSelectAvailable.Clone();
            dtLstSelectAvailable.Rows.Cast<DataRow>().ToList().ForEach(x =>
            {
                Boolean isadd = true;
                if ((x["COLUMNXML"] != DBNull.Value) && (x["COLUMNXML"].ToString().IndexOf(predicatExclude) > 0))
                    isadd = false;
                if (isadd)
                    ret.ImportRow(x);
            });

            return ret;
        }


        /// <summary>
        /// Charge dans le datatable dtLstWhere les données relatives à la table LSTWHERE et données complémentaires pour la consultation et le template concernés
        /// </summary>
        public void LoadLstWhere(string pCS, bool pIsEnabledOnly, bool pIsJoinLstTable)
        {
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "lw.IDLSTCOLUMN,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.LSTIDVALUE,lw.ALIAS,lw.ISENABLED,lw.ISMANDATORY" + Cst.CrLf;

            if (pIsJoinLstTable)
            {
                // FI 20190327 [24603] Add COLUMNXML               
                SQLSelect += ",lc.DATATYPE,lc.SQLWHERE,lc.SQLSELECT,la.TABLENAME,lc.COLUMNXML" + Cst.CrLf; 
            }
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            if (pIsJoinLstTable)
            {
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + " lc on (lc.TABLENAME=lw.TABLENAME and lc.COLUMNNAME=lw.COLUMNNAME) " + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTALIAS.ToString() + " la on (la.ALIAS=lw.ALIAS)" + Cst.CrLf;
            }
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out DataParameters dbParam) + Cst.CrLf;
            if (pIsEnabledOnly)
                SQLSelect += SQLCst.AND + "(lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")" + Cst.CrLf;
            SQLSelect += SQLCst.ORDERBY + "lw.ISMANDATORY desc, lw.POSITION";

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            dtLstWhere = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Charge dans le datatable dtLstOrderBy (datatable d'instance dans la classe) les données relatives à la table LSTORDERBY pour la consultation et le template concernés
        /// </summary>
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20200226 [25077] RDBMS : Correction gestion EXCLUDE et INCLUDE COLUMNNAME
        public DataTable LoadLstOrderBy(string pCS)
        {
            string sqlSelect = String.Format(@"select lob.ASCDESC,lob.ALIAS,lob.ISGROUPBY,lob.ISGROUPINGSET,lob.GROUPINGSET,
            lco.TABLENAME,lco.COLUMNNAME,lco.DISPLAYNAME,lco.AGGREGATE,lco.SQLGROUPBY,lco.SQLSELECT,lco.DATATYPE,lco.SQLORDER, lco.COLUMNNAMEREF as COLUMNREF,
            lcaj.IDLSTJOIN,isnull(isnull(lcajheader1.IDLSTJOIN,lcajheader0.IDLSTJOIN),lcaj.IDLSTJOIN) as ALIASDISPLAYNAME
            from dbo.LSTORDERBY lob
            inner join dbo.LSTCONSULTALIAS lca on(lca.IDLSTCONSULT=@IDLSTCONSULT and lca.ALIAS=lob.ALIAS)
            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME=lob.TABLENAME and lco.COLUMNNAME=lob.COLUMNNAME) and
                ((lca.EXCLUDECOLUMNNAME is null) or ((';' || lca.EXCLUDECOLUMNNAME || ';') not like ('%;' || lco.COLUMNNAME || ';%'))) and 
                ((lca.INCLUDECOLUMNNAME is null) or ((';' || lca.INCLUDECOLUMNNAME || ';') like ('%;' || lco.COLUMNNAME || ';%'))) 
            inner join dbo.LSTCONSALIASJOIN lcaj on(lcaj.IDLSTCONSULT=lob.IDLSTCONSULT) and (lcaj.ALIAS=lob.ALIAS) and 
                (lcaj.POSITION = (
	                select  max(tmp.POSITION)  
	                from dbo.LSTCONSALIASJOIN tmp 
	                where (tmp.IDLSTCONSULT=lcaj.IDLSTCONSULT) and (tmp.ALIAS=lcaj.ALIAS)
                )) 
            left outer join dbo.LSTCONSULTALIAS lcaheader on(lcaheader.IDLSTCONSULT=@IDLSTCONSULT and lcaheader.ALIAS=lca.ALIASHEADER)
            left outer join dbo.LSTCONSALIASJOIN lcajheader1 on(lcajheader1.IDLSTCONSULT=@IDLSTCONSULT and lcajheader1.ALIAS=lcaheader.ALIAS and lcajheader1.POSITION=1)
            left outer join dbo.LSTCONSALIASJOIN lcajheader0 on(lcajheader0.IDLSTCONSULT=@IDLSTCONSULT and lcajheader0.ALIAS=lcaheader.ALIAS and lcajheader0.POSITION=0)
            where {0}
            order by lob.POSITION", template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lob", out DataParameters dbParam)) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dbParam);
            dtLstOrderBy = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            
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
        /// Chargement en mémoire de la table LSTPARAM
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20200602 [25370] Add 
        public DataTable LoadLstParam(string pCS)
        {
            string sqlSelect = String.Format(@"select PARAMNAME, DATATYPE, PARAMVALUE
            from dbo.LSTPARAM lstParam 
            where {0}", template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lstParam", out DataParameters dbParam)) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dbParam);
            dtLstParam = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return dtLstParam;
        }

        /// <summary>
        /// Création d'une copie d'un Template avec copie childs from ...   
        /// </summary>
        /// <param name="pSourceIdLstTemplate">ID du template Source</param>
        /// <param name="pSourceIdA">propriétaire du template Source</param>
        
        public void InsertOverWriteTemplateWithCopyChildsFrom(string pCS, string pSourceIdLstTemplate, int pSourceIdA)
        {
            // FI 20200602 [25370] Appel à ReferentialWeb.CreateOverwrite
            ReferentialWeb.CreateOverwrite(pCS, template, SessionTools.Collaborator_IDA);
            LstTemplate.CopyChilds(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, template.IDLSTTEMPLATE, SessionTools.Collaborator_IDA);
        }

        
        
        /// <summary>
        /// Création d'une copie de type temporaire d'un template
        /// </summary>
        /// <param name="pSourceIdLstTemplate">ID du template à copier</param>
        /// <param name="pSourceIdA">ID du propriétaire du template temporaire à copier</param>
        /// <param name="pTargetIdA">ID du propriétaire du nouveau template temporaire (généralement le user connecté)</param>
        /// <returns>ID du template temporaire créé</returns>
        public string CreateCopyTemporaryTemplate(string pCS, string pSourceIdLstTemplate, int pSourceIdA, int pTargetIdA)
        {
            DuplicateTemplate(pCS, pSourceIdLstTemplate, pSourceIdA,
                LstTemplate.TEMPORARYPREFIX + pSourceIdLstTemplate, pTargetIdA, false);
            
            return LstTemplate.TEMPORARYPREFIX + pSourceIdLstTemplate;
        }

        /// <summary>
        /// Création d'une copie d'un template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSourceIdLstTemplate">Identifiant du template source</param>
        /// <param name="pSourceIdA">Propriétaire du template source</param>
        /// <param name="pTargetIdLstTemplate">Identifiant du template cré</param>
        /// <param name="pTargetIdA">Propriétaire du template du template cré</param>
        /// <param name="pIsDeleteSource">si true suppression du template source </param>
        public void DuplicateTemplate(string pCS,
            string pSourceIdLstTemplate, int pSourceIdA,
            string pTargetIdLstTemplate, int pTargetIdA,
            bool pIsDeleteSource)
        {
            //Chargement du template source pour la copie
            LstTemplate lstTemplate = new LstTemplate();
            lstTemplate.Load(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA);

            //on modifie le name source                   
            lstTemplate.IDLSTTEMPLATE = pTargetIdLstTemplate;

            if (SessionTools.IsSessionGuest) //PL 20150601 GUEST New feature
            {
                //GUEST: on affecte en "dur" les droits ( see also LSTSAVE.aspx.cs - SaveData() )
                lstTemplate.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
                lstTemplate.RIGHTENTITY = RightsTypeEnum.REMOVE.ToString();
                lstTemplate.RIGHTDEPARTMENT = RightsTypeEnum.REMOVE.ToString();
                lstTemplate.RIGHTDESK = RightsTypeEnum.NONE.ToString();
            }
            
            // FI 20201210 [XXXXX] Alimentation de LoadOnStart à false si template
            if (lstTemplate.IsTemporary)
                lstTemplate.ISLOADONSTART = false;

            //on insere dans les tables SQL 
            // FI 20200602 [25370] Appel à ReferentialWeb.CreateOverwrite 
            ReferentialWeb.CreateOverwrite(pCS, lstTemplate, pTargetIdA);

            //on copie les childs dans les tables SQL 
            LstTemplate.CopyChilds(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, pTargetIdLstTemplate, pTargetIdA);
            
            if (pIsDeleteSource)
                ReferentialWeb.Delete(pCS, IdLstConsult, pSourceIdLstTemplate, pSourceIdA, true);
        }

        /// <summary>
        /// Obtient les infos concernant les critères spécifiés sous LSTWHERE pour une consultation
        /// </summary>
        /// FI 20140519 [19923] Modification de la signatute de la méthode add pIdAentity
        // EG 20180426 Analyse du code Correction [CA2202]
        /// EG 20210419 [XXXXX] Upd Nouveau paramètre - Usage du businessCenter de l'entité
        public InfosLstWhere[] GetInfoWhere(string pCS, int pIdAEntity, string pIdBCEntity)
        {
            InfosLstWhere[] ret = null;

            // FI 20190327 [24603] Add COLUMNXML
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "lw.OPERATOR,lw.LSTVALUE,lw.ISMANDATORY,lco.ISRESOURCE,lw.POSITION," + Cst.CrLf;
            SQLSelect += "lco.TABLENAME,lco.DISPLAYNAME,lco.DATATYPE,lco.COLUMNXML," + Cst.CrLf;
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
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out DataParameters dbParam);
            SQLSelect += SQLCst.AND + "(";
            SQLSelect += "(lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + "and lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += SQLCst.OR;
            SQLSelect += "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += ")" + Cst.CrLf;
            SQLSelect += SQLCst.ORDERBY + "lw.POSITION";

            ArrayList al = new ArrayList();
            
            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {

                while (dr.Read())
                {
                    InfosLstWhere infosWhere = new InfosLstWhere();

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
                    infosWhere.ColumnIdentifier = columnIdentifier;

                    // FI 20190327 [24603] Alimentation de dataType 
                    ReferentialsReferentialColumnDataType dataType = new ReferentialsReferentialColumnDataType
                    {
                        value = dr["DATATYPE"].ToString().Trim()
                    };
                    if (false == (dr["COLUMNXML"] is DBNull))
                    {
                        string columnXML = dr["COLUMNXML"].ToString();
                        EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(ReferentialsReferentialColumn), columnXML);
                        ReferentialsReferentialColumn rrcColumn = (ReferentialsReferentialColumn)CacheSerializer.Deserialize(serializerInfo);
                        if (rrcColumn.DataTypeSpecified)
                            dataType = rrcColumn.DataType;
                    }

                    //OPERATOR
                    infosWhere.Operator = Convert.ToString(dr["OPERATOR"]).ToString();

                    //LSTVALUE formatted
                    //infosWhere.lstValue = FormatLstValue2(pCS, Convert.ToString(dr["LSTVALUE"]),
                    //                Convert.ToString(dr["DATATYPE"]), true, Convert.ToBoolean(dr["ISRESOURCE"]), false, pIdAEntity);

                    infosWhere.LstValue = FormatLstValue2(pCS, Convert.ToString(dr["LSTVALUE"]), dataType, true, Convert.ToBoolean(dr["ISRESOURCE"]), false, pIdAEntity, pIdBCEntity);


                    //ISMANDATOTY
                    infosWhere.IsMandatory = BoolFunc.IsTrue(Convert.ToString(dr["ISMANDATORY"]));

                    //POSTION
                    infosWhere.Position = IntFunc.IntValue(Convert.ToString(dr["POSITION"]));

                    al.Add(infosWhere);
                }
                if (ArrFunc.IsFilled(al))
                    ret = (InfosLstWhere[])al.ToArray(typeof(InfosLstWhere));
            }
            return ret;
        }

        /// <summary>
        /// Obtient les infos concernant les critères spécifiés sous LSTWHERE pour un referentiel
        /// </summary>
        /// FI 20140519 [19923] Modification de la signatute de la méthode add pIdAentity
        // EG 20180426 Analyse du code Correction [CA2202]
        /// EG 20210419 [XXXXX] Upd Nouveau paramètre - Usage du businessCenter de l'entité
        public InfosLstWhere[] GetInfoWhereFromReferencial(string pCS, ReferentialsReferential pRR, int pIdAEntity, string pIdBCEntity)
        {
            InfosLstWhere[] ret = null;

            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "lw.IDLSTCOLUMN,lw.ALIAS,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.ISMANDATORY,lw.POSITION" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDLSTTEMPLATE=lw.IDLSTTEMPLATE and lt.IDLSTCONSULT=lw.IDLSTCONSULT and lt.IDA=lw.IDA)" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lw", out DataParameters dbParam) + Cst.CrLf;
            SQLSelect += SQLCst.AND + "(";
            SQLSelect += "(lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + SQLCst.AND + "lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += SQLCst.OR;
            SQLSelect += "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + ")";
            SQLSelect += ")" + Cst.CrLf;
            SQLSelect += SQLCst.AND + "((isnull(lw.IDLSTCOLUMN,-1)>=0) or (lw.COLUMNNAME is not null))";
            SQLSelect += SQLCst.ORDERBY + "lw.POSITION";

            ArrayList al = new ArrayList();

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    InfosLstWhere infosWhere = new InfosLstWhere();

                    string displayCol = string.Empty;
                    int index = -1;

                    if (!(dr["IDLSTCOLUMN"] is DBNull))
                        index = Convert.ToInt32(dr["IDLSTCOLUMN"]);
                    string columnName = null;
                    if (!(dr["COLUMNNAME"] is DBNull))
                        columnName = Convert.ToString(dr["COLUMNNAME"]);
                    string previousColumnName = columnName;

                    ReferentialsReferentialColumn rrc = null;
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
                            // RD 20170227 [xxxxx] Ajouter espace avant la parenthèse 
                            displayCol += " (" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                        }
                        else
                        {
                            displayCol = Ressource.GetMulti(rrc.Ressource, 1);
                            if (rrc.ExistsRelation)
                                displayCol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1, displayCol);

                            if ((index + 1 < pRR.Column.GetLength(0)) && StrFunc.IsEmpty(pRR.Column[index + 1].Ressource))
                                //Note: On est ici sur le cas de la 1ere colonne "liée" (ie: Tenor)
                                displayCol += " (" + Ressource.GetMulti(rrc.ColumnName, 1) + ")";
                        }

                        #region PL 20190611 - Affine la ressource dans le cas de ressource multiple (ex. Colonnes "BOOK Identifier / Nom affiché" et ressource BOOK_IDENTIFIER_SENDTO sur Messagerie Editions)
                        bool isExistMultiRessource = false;
                        string ovrDisplaycol = string.Empty;
                        try
                        {
                            if (StrFunc.IsEmpty(rrc.Ressource))
                            {
                                if (rrc.ExistsRelation)
                                {
                                    ovrDisplaycol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 2, 0);
                                }
                            }
                            else
                            {
                                if (rrc.ExistsRelation)
                                {
                                    isExistMultiRessource = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 0) != Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1);
                                    if (isExistMultiRessource)
                                        ovrDisplaycol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 2, 0);
                                }
                                else
                                { 
                                    isExistMultiRessource = Ressource.GetMulti(rrc.Ressource, 0) != Ressource.GetMulti(rrc.Ressource, 1);
                                    if (isExistMultiRessource)
                                        ovrDisplaycol = Ressource.GetMulti(rrc.Ressource, 2, 0);
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(ovrDisplaycol))
                                displayCol = ovrDisplaycol;
                        }
                        catch { }
                        #endregion

                        //COLUMNNAME
                        infosWhere.ColumnIdentifier = displayCol;

                        //OPERATOR
                        //infosWhere.@operator = dr.GetValue(1).ToString();
                        infosWhere.Operator = Convert.ToString(dr["OPERATOR"]);

                        //LSTVALUE formatted
                        bool isResource = rrc.IsResourceSpecified && rrc.IsResource.IsResource;
                        if (isResource && rrc.IsResource.isCriteriaDisplaySpecified)
                            isResource &= rrc.IsResource.isCriteriaDisplay;
                        //
                        infosWhere.LstValue = FormatLstValue2(pCS, Convert.ToString(dr["LSTVALUE"]), rrc.DataType, true, isResource, false, pIdAEntity, pIdBCEntity);

                        //ISMANDATORY
                        infosWhere.IsMandatory = BoolFunc.IsTrue(Convert.ToString(dr["ISMANDATORY"]));

                        //POSITION
                        infosWhere.Position = IntFunc.IntValue(Convert.ToString(dr["POSITION"]));

                        al.Add(infosWhere);
                    }
                }
                if (ArrFunc.IsFilled(al))
                    ret = (InfosLstWhere[])al.ToArray(typeof(InfosLstWhere));
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRR"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public static string GetLabelReferential(ReferentialsReferential pRR, string pColumnName)
        {
            string displayCol = pColumnName;
            int previousIndex;
            //
            for (int index = 0; index < pRR.Column.Length; index++)
            {
                previousIndex = index;
                //
                ReferentialsReferentialColumn rrc = pRR.Column[index];
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
        /// <para>si false retourne la valeur saisie au format ISO (pour  stockage dans LSTWHERE)</para>
        /// </param>
        /// <param name="pIsResource"></param>
        /// <param name="pIsFormatForFormCriteria"></param>
        /// <returns></returns>
        /// FI 20140519 [19923] Modification signature de la fonction (la paramètre pidAEntity est nécessaire pour que cette methode n'utilise plus SessionTools
        /// FI 20190327 [24603] Refactoring les données datetime acceptent des dates (ex 2019 ou 27/03/2019) 
        /// EG 20210419 [XXXXX] Upd Nouveau paramètre - Usage du businessCenter de l'entité
        public static string FormatLstValue2(string pCs, string pValue, ReferentialsReferentialColumnDataType pDataType, bool pIsFormatForDisplay, bool pIsResource, bool pIsFormatForFormCriteria, int pIdAEntity, string pIdBCEntity)
        {
            //Lorsque l'utilisateur saisit blanc ou {null} 
            //Spheres® stocke NULL dans LSTWHERE affiche {null} au dessus du datagrid
            //
            //Lorsque l'utilisateur saisit null
            //Spheres® stocke 'null' dans LSTWHERE affiche null au dessus du datagrid
            string retValue;
            if (pIsFormatForDisplay && StrFunc.IsEmpty(pValue))
                retValue = "{null}";  //Affichage depuis les valeurs de LSTWHERE
            else if ((false == pIsFormatForDisplay) && ((pValue == "{null}") || StrFunc.IsEmpty(pValue)))
                retValue = null; //Stockage dans LSTWHERE
            else
            {
                retValue = pValue.TrimEnd();
                if ((TypeData.IsTypeDateOrDateTime(pDataType.value) || TypeData.IsTypeTime(pDataType.value)) && pValue != "null")
                {
                    #region IsTypeDateOrDateTime or IsTypeTime
                    bool isDateTimeFunction = (false == DtFunc.IsParsableValue(pValue));

                    bool isConvertDate;
                    if (isDateTimeFunction && pIsFormatForFormCriteria)
                    {
                        // Dans ce cas les mots clés (ex DTBUSINESS ne sont pas converties) 
                        isConvertDate = false;
                    }
                    else
                    {
                        isConvertDate = true;
                    }

                    if (isConvertDate)
                    {
                        // La date est convertie au format ISO pour le stockage SQL
                        // La date est convertie au format CurrentCulture pour l'affichage
                        DtFuncML dtFunc = new DtFuncML(pCs, pIdBCEntity, pIdAEntity, 0, 0, null)
                        {
                            FourDigitReading = DtFunc.FourDigitReadingEnum.FourDigitHasYYYY
                        };
                        //            
                        if (pIsFormatForDisplay)
                        {
                            // Formatage de la date au format CurrentCulture
                            if (TypeData.IsTypeDate(pDataType.value) || (TypeData.IsTypeDateTime(pDataType.value)))
                            {
                                if (TypeData.IsTypeDate(pDataType.value))
                                {
                                    retValue = dtFunc.GetDateString(pValue);
                                }
                                else if (TypeData.IsTypeDateTime(pDataType.value))
                                {
                                    // FI 20190327 une donnée datetime se comporte comme une donnée date si l'utilisateur ne saisit pas d'heure
                                    if ((DateTime.TryParseExact(pValue, DtFunc.FmtISODate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                        || (isDateTimeFunction && (pValue != DtFunc.NOW)))
                                        retValue = dtFunc.GetDateString(pValue);
                                    else
                                        retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtDateLongTime);
                                }

                                //20090912 FI gestion de la saisie d'une année uniquement sur 2 ou 4 caractères
                                if ((pValue.Length == 4) && ((new DtFunc().StringToDateTime(retValue)).Year.ToString() == pValue))
                                    retValue = pValue;

                                // FI 20190327 [24603] Affichage du Fuseau horaire lorsque la donnée est de type datetime 
                                // si le fuseau horaire du stockage est différent de celui de la session => Affichage du fuseau horaire de la session
                                //                      Dans ce cas le critère date est converti dans le fuseau horaire de la colonne SQL (voir SQL_ColumnCriteria.ConvertToColTzdbid)
                                //                      pDataType.tzdbid donne le fuseau horaire de la colonne SQL
                                // sinon => Affichage du fuseau horaire de la colonne 
                                //                      Dans ce cas le critère date est n'est pas converti 
                                if (TypeData.IsTypeDateTime(pDataType.value) && pDataType.tzdbidSpecified)
                                {
                                    string tzdbIdDisplay = pDataType.tzdbid;
                                    string tzdbIdSession = SessionTools.GetCriteriaTimeZone();
                                    if (StrFunc.IsFilled(tzdbIdSession) && tzdbIdSession != pDataType.tzdbid)
                                        tzdbIdDisplay = Tz.Tools.GetShortTzdbId(tzdbIdSession);
                                    retValue = StrFunc.AppendFormat("{0} {1}", retValue, tzdbIdDisplay);
                                }
                            }
                            else if (TypeData.IsTypeTime(pDataType.value))
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
                            if (TypeData.IsTypeDate(pDataType.value) || (TypeData.IsTypeDateTime(pDataType.value)))
                            {
                                if (TypeData.IsTypeDate(pDataType.value))
                                {
                                    retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODate);
                                }
                                else if (TypeData.IsTypeDateTime(pDataType.value))
                                {
                                    // FI 20190327 une donnée datetime se comporte comme une donnée date si l'utilisateur ne saisit pas d'heure
                                    if (pIsFormatForFormCriteria && DateTime.TryParseExact(pValue, DtFunc.FmtShortDate, CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                                        retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODate);
                                    else if ((DateTime.TryParseExact(pValue, DtFunc.FmtISODate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                        || (isDateTimeFunction && (pValue != DtFunc.NOW)))
                                        retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODate);
                                    else
                                        retValue = dtFunc.GetDateTimeString(pValue, DtFunc.FmtISODateTime);
                                }
                                //
                                //20090912 FI gestion de la saisie d'une année uniquement sur 2 ou 4 caractères
                                if ((pValue.Length == 4) && ((new DtFunc().StringToDateTime(retValue)).Year.ToString() == pValue))
                                    retValue = pValue;
                            }
                            else if (TypeData.IsTypeTime(pDataType.value))
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
                else if (TypeData.IsTypeDec(pDataType.value) && pValue != "null")
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
        // EG 20180426 Analyse du code Correction [CA2202]
        public ArrayList[] GetInfoOrderBy(string pCS)
        {
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
            SQLSelect += SQLCst.WHERE + template.GetSQLClause_PK_LSTTEMPLATE2(pCS, "lob", out DataParameters dbParam);
            // RD 20111114 Pour afficher les critères de tri dans l'ordre            
            SQLSelect += SQLCst.ORDERBY + "lob.POSITION";

            ArrayList[] alWhere = new ArrayList[2];
            alWhere[0] = new ArrayList();
            alWhere[1] = new ArrayList();

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
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
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// FI 20141211 [20563] Nouvelle signature add parameter pDynamicDatas
        /// FI 20200205 [XXXXX] pDynamicArgs est de type ReferentialsReferentialStringDynamicData
        /// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        /// FI 20201201 [XXXXX] suppression de l'argument pDynamicArgs
        public Referentials GetReferentials()
        {
            Referentials rMain = new Referentials
            {
                Items = new ReferentialsReferential[1] { new ReferentialsReferential() }
            };

            if (StrFunc.IsFilled(_consultXML))
            {
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(Referentials), _consultXML);
                rMain = (Referentials)CacheSerializer.Deserialize(serializerInfo);
            }

            ReferentialsReferential rr = rMain.Items[0];
            rr.IsConsultation = true;
            rr.IdLstConsult = this.IdLstConsult;

            // FI 20201201 [XXXXX] Il n'a plus appel à SetDynamicArgs. L'alimentation des arguments est effectué uniquement dans Referential.Initialize
            //// FI 20141211 [20563] Appel de la méthode SetDynamicArgs   
            //if (null != pDynamicArgs)
            //    rr.SetDynamicArgs(pDynamicArgs);

            #region <Referencial>
            DataRow[] lstAliasDataRows = dtLstAlias.Select("ALIAS ='" + dtLstConsultAlias.Select("POSITION = 1")[0]["ALIAS"].ToString().Trim() + "'");
            

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

            rr.Image = "fas fa-search";

            rr.AliasTableNameSpecified = true;
            rr.AttachedDocSpecified = false;
            rr.ColumnByRowSpecified = true;
            rr.HelpUrlSpecified = false;
            rr.NotepadSpecified = false;
            rr.ToolBarSpecified = false;

            //FI 20111219 Appel à SetSqlCommandToReferential pour alimenter le sqlCommand source de la consultation
            if (!(lstAliasDataRows[0]["COMMAND"] is DBNull) || !(lstAliasDataRows[0]["TABLEDETXML"] is DBNull))
                SetReferentialSqlCommand(rr);

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
        /// Pour l'affichage d'une seule ressource (Header) pour plusieurs colonnes.
        /// Mettre à jour les ressources de la colonne en cours et eventuellement des colonnes précédentes,
        /// si la colonne en cours est une colonne de référence pour les ressources
        /// ou bien la colonne en cours dépond d'une éventuelle colonne de référence précédente
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pCurrentColumnIndex"></param>
        /// <param name="pIsFound_ColumnRef"></param>
        private void SetColumnRessourceFromColumnRef(ReferentialsReferential pReferential, int pCurrentColumnIndex, ref bool pIsFound_ColumnRef)
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
        /// Initilalise la command SQL de base du ReferentialsReferential
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// FI 20111219 Add SetSqlCommandToReferential 
        /// FI 20141211 [20563] add parameter pCondApp
        private void SetReferentialSqlCommand(ReferentialsReferential pReferential)
        {
            DataRow[] lstAliasDataRows = dtLstAlias.Select("ALIAS ='" + pReferential.AliasTableName + "'");
            DataRow lstAliasDataRow = lstAliasDataRows[0];
            //
            //Lescture de la colonne TABLEDETXML si la colonne COMMAND est non renseignée
            if (!(lstAliasDataRows[0]["TABLEDETXML"] is DBNull) & (lstAliasDataRows[0]["COMMAND"] is DBNull))
            {
                string xmlExpression = lstAliasDataRow["TABLEDETXML"].ToString();
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(SQLSelectOrResource), xmlExpression);
                SQLSelectOrResource sqlSelectOrResource = (SQLSelectOrResource)CacheSerializer.Deserialize(serializerInfo);
                
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
                pReferential.SQLSelect = new ReferentialsReferentialSQLSelect[1];
                pReferential.SQLSelect[0] = new ReferentialsReferentialSQLSelect();
                pReferential.SQLSelectSpecified = true;
                pReferential.SQLSelect[0].Command = new ReferentialsReferentialSQLSelectCommand[1];
                pReferential.SQLSelect[0].Command[0] = new ReferentialsReferentialSQLSelectCommand
                {
                    rdbms = lstAliasDataRows[0]["RDBMS"].ToString(),
                    Value = lstAliasDataRows[0]["COMMAND"].ToString()
                };
            }

            // FI 20201201 [XXXXX] Mise en commentaire, l'appel de cette méthode sera effctuée dans ReferentialsReferential.Initialize
            // Dans ReferentialsReferential.Initialize, il y l'exhaustivité des dynamicArg
            //Appel à InitializeSQLSelectCommand puisque cette méthode peut interpréter les tags %%SR:
            //pReferential.InitializeSQLSelectCommand(pParam, pCondApp);
        }

        /// <summary>
        /// Affecte le membre SQLWhere de ReferentialsReferential
        /// </summary>
        /// FI 20141107 [20441] Add (Les codages en dur sur SESSIONRESTRICT on été supprimés, ils sont désormais présents dans la colonne LSTCONSULT.CONSULTXML)
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private void SetReferentialSQLWhere2(ReferentialsReferential rr)
        {
            ArrayList sqlWhereArrayList = new ArrayList();
            if (ArrFunc.IsFilled(rr.SQLWhere))
                sqlWhereArrayList = new ArrayList(rr.SQLWhere);

            foreach (DataRow row in dtLstWhere.Rows)
            {
                ReferentialsReferentialSQLWhere sqlWhere = new ReferentialsReferentialSQLWhere();

                //20070321 PL 
                if (template.ISENABLEDLSTWHERE || Convert.ToBoolean(row["ISMANDATORY"]))
                {
                    rr.HasLstWhereClause = true;
                    rr.SQLWhereSpecified = true;

                    sqlWhere.AliasTableName = row["ALIAS"].ToString().Trim();
                    sqlWhere.AliasTableNameSpecified = true;

                    // FI 20201127 [XXXXX] Alimentation de TABLENAME
                    sqlWhere.TableName = row["TABLENAME"].ToString().Trim();
                    sqlWhere.TableNameSpecified = true;

                    sqlWhere.ColumnName = row["COLUMNNAME"].ToString().Trim();
                    sqlWhere.ColumnNameSpecified = StrFunc.IsFilled(sqlWhere.ColumnName);

                    // FI 20190327 [24603] sqlWhere.DataType est désormais de type ReferentialsReferentialColumnDataType
                    // Alimentation depuis COLUMNXML lorsque enseigné
                    //string dataType = row["DATATYPE"].ToString().Trim();
                    ReferentialsReferentialColumnDataType dataType = new ReferentialsReferentialColumnDataType
                    {
                        value = row["DATATYPE"].ToString().Trim()
                    };
                    if (false == (row["COLUMNXML"] is DBNull))
                    {
                        string columnXML = row["COLUMNXML"].ToString();
                        EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(ReferentialsReferentialColumn), columnXML);
                        ReferentialsReferentialColumn rrcColumn = (ReferentialsReferentialColumn)CacheSerializer.Deserialize(serializerInfo);
                        if (rrcColumn.DataTypeSpecified)
                            dataType = rrcColumn.DataType;
                    }
                    sqlWhere.DataType = dataType;

                    sqlWhere.LstValue = FormatLstValue2(SessionTools.CS, row["LSTVALUE"].ToString().Trim(), dataType,false, false, false, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter);
                    sqlWhere.LstValueSpecified = true;
                    
                    sqlWhere.Operator = row["OPERATOR"].ToString().Trim();
                    sqlWhere.OperatorSpecified = true;

                    ReferentialsReferentialColumn rrc = rr[sqlWhere.ColumnName, sqlWhere.AliasTableName];
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
                rr.SQLWhere = (ReferentialsReferentialSQLWhere[])sqlWhereArrayList.ToArray(typeof(ReferentialsReferentialSQLWhere));

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
        /// Affecte le membre SQLOrderBy de ReferentialsReferential
        /// </summary>
        /// <param name="rr"></param>
        /// FI 20170228 [22883] Modify
        private void SetReferentialSQLOrder(ReferentialsReferential rr)
        {
            int indice = 0;
            rr.SQLOrderBy = new ReferentialsReferentialSQLOrderBy[dtLstOrderBy.Rows.Count];
            
            bool isReferentialWithGroupBy = false;
            for (int i = 0; i < ArrFunc.Count(rr.Column); i++)
            {
                if ((rr.Column[i].GroupBy != null) && rr.Column[i].GroupBy.IsGroupBy)
                {
                    isReferentialWithGroupBy = true;
                    break; // FI 20170228 [22883] add
                }
            }
            
            foreach (DataRow row in dtLstOrderBy.Rows)
            {
                string alias = row["ALIAS"].ToString().Trim();
                string column = row["COLUMNNAME"].ToString().Trim();
                string sqlorder = row["SQLORDER"].ToString().Trim();
                string ascdesc = row["ASCDESC"].ToString().Trim();
                
                string columnName = alias + "." + column;
                string columnAlias = alias + "_" + column;

                rr.SQLOrderBy[indice] = new ReferentialsReferentialSQLOrderBy
                {
                    ColumnName = column,
                    Alias = alias,
                    DataType = row["DATATYPE"].ToString().Trim()
                };

                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                // Lignes Déplacées ci-dessous
                if (null == rr[column, alias])
                {
                    // FI 20170228 [22883]  Ajout d'une colonne cachée => Simplification du code . Necessaire pour retrouver les enregistrements qui constituent le groupe 
                    // ColumnNotInReferential
                    TypeData.TypeDataEnum dataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), rr.SQLOrderBy[indice].DataType);
                    
                    string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                    if (StrFunc.IsFilled(sqlSelect))
                        sqlSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, rr.SQLOrderBy[indice].Alias);
                    
                    AddReferentialHideColumn(rr, alias, column, dataType, sqlSelect);

                    ReferentialsReferentialColumn col = rr[column, alias];
                    col.GroupBy = new ReferentialsReferentialColumnGroupBy();
                    col.GroupBySpecified = true;
                    col.GroupBy.IsGroupBy = (false == (row["ISGROUPBY"] is DBNull || BoolFunc.IsFalse(row["ISGROUPBY"])));
                    col.GroupBy.GroupingSet = Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET");
                    if (row["AGGREGATE"] is DBNull)
                        col.GroupBy.AggregateSpecified = false;
                    else
                    {
                        col.GroupBy.Aggregate = Convert.ToString(row["AGGREGATE"]);
                        // FI 20230213 [26264] Modification de l'alimentation de AggregateSpecified
                        //col.GroupBy.AggregateSpecified = StrFunc.IsFilled(rr.SQLOrderBy[indice].GroupBy.Aggregate);
                        col.GroupBy.AggregateSpecified = StrFunc.IsFilled(col.GroupBy.Aggregate);

                    }
                    if (row["SQLGROUPBY"] is DBNull)
                        col.GroupBy.SqlGroupBySpecified = false;
                    else
                    {
                        col.GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]);
                        // FI 20230213 [26264]  Modification de l'alimentation de SqlGroupBySpecified
                        //col.GroupBy.SqlGroupBySpecified = StrFunc.IsFilled(rr.SQLOrderBy[indice].GroupBy.SqlGroupBy);
                        col.GroupBy.SqlGroupBySpecified = StrFunc.IsFilled(col.GroupBy.SqlGroupBy);
                    }

                    // FI 20170228 [22883] mise en commentaire des lignes suivantes Dans 3 mois si tout ok je supprimerai le code qui s'appuie sur  rr.SQLOrderBy[indice].ColumnNotInReferential = true
                    /*
                    rr.SQLOrderBy[indice].ColumnNotInReferential = true;
                    //
                    #region GroupBy
                    rr.SQLOrderBy[indice].GroupBy = new ReferentialsReferentialColumnGroupBy();
                    rr.SQLOrderBy[indice].GroupBySpecified = true;
                    rr.SQLOrderBy[indice].GroupBy.IsGroupBy = (false == (row["ISGROUPBY"] is DBNull ||
                                                                BoolFunc.IsFalse(row["ISGROUPBY"])));

                    // MF 20120430 ruptures with groupingset
                    rr.SQLOrderBy[indice].GroupBy.GroupingSet =  Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET") ;
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
                     */
                }
                
                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                string tempColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameOrColumnSelect(rr, rr.SQLOrderBy[indice]);
                rr.SQLOrderBy[indice].Value = ReferentialTools.ConstructColumnOrderBy(alias, tempColumnNameOrColumnSQLSelect, sqlorder, false) + " " + ascdesc;
                rr.SQLOrderBy[indice].ValueWithAlias = ReferentialTools.ConstructColumnOrderBy(alias, columnName, sqlorder, true) + " " + ascdesc;
                
                indice++;
            }
            
            rr.SQLOrderBySpecified = ArrFunc.IsFilled(rr.SQLOrderBy);
            
            if (rr.SQLOrderBySpecified)
            {
                for (int i = 0; i < rr.SQLOrderBy.Length; i++)
                {
                    ReferentialsReferentialSQLOrderBy rr_SQLOrderBy = rr.SQLOrderBy[i];
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
        /// Affecte le membre Column de ReferentialsReferential
        /// </summary>
        // EG 20180629 PERF Add FLOWSBYTRADE Test
        private void SetReferentialColumn(ReferentialsReferential rr)
        {
            ArrayList listColumnAlias = new ArrayList();

            #region Ajout de colonne en DUR (DataKeyField, [ForeignKeyField], [OrderBy])
            bool existsDataKeyField = true;
            bool existsForeignKeyField = true;
            ReferentialsReferentialColumn rrcDataKeyField = new ReferentialsReferentialColumn();
            ReferentialsReferentialColumn rrcForeignKeyField = new ReferentialsReferentialColumn();
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
            else if ((rr.TableName == "POSDET") ||  (rr.TableName == "POSDETOTC"))
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
            else if ((rr.TableName == "FLOWSBYTRADE") || (rr.TableName == "FLOWSBYTRADEOTC"))
            {
                table_DataKeyField = "IDT";
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
                        rrcDataKeyField.DataField = ReferentialTools.GetNewAlias(rrcDataKeyField.AliasTableName + "_" + rrcDataKeyField.ColumnName, ref listColumnAlias);
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
                    rrcForeignKeyField.DataField = ReferentialTools.GetNewAlias(rrcForeignKeyField.AliasTableName + "_" + rrcForeignKeyField.ColumnName, ref listColumnAlias);
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
            rr.Column = new ReferentialsReferentialColumn[dtLstSelectedCol.Rows.Count + add];
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
                ReferentialsReferentialColumn rrcOrderBy = new ReferentialsReferentialColumn
                {
                    ColumnName = table_OrderBy,
                    Ressource = table_OrderBy,
                    RessourceSpecified = true,
                    AliasTableName = table_AliasTableName,
                    AliasTableNameSpecified = true,
                    Length = 64,
                    IsHide = true,
                    IsHideSpecified = true,
                    IsHideInDataGrid = true,
                    IsHideInDataGridSpecified = true
                };
                rrcOrderBy.DataField = ReferentialTools.GetNewAlias(rrcOrderBy.AliasTableName + "_" + rrcOrderBy.ColumnName, ref listColumnAlias);
                rrcOrderBy.IsOrderBySpecified = true;
                rrcOrderBy.IsOrderBy.Value = "true";
                rrcOrderBy.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                #endregion
                rr.Column[indice] = rrcOrderBy;
                indice++;
            }
            #endregion Ajout de colonne en DUR


            AddReferentialSelectedColumn(rr, indice);

            AddReferentialTechnicalColumn(rr);

        }

        /// <summary>
        ///  Ajoute éventuellement des attributs IsHyperLink à la colonne {rrc}  et éventuellement des colonnes suplémentaires de type ID (Ex IDASSET, IDA, IDB, etc)
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
        /// FI 20170302 [XXXXX] Modify
        /// EG 20180629 PERF Add FLOWSBYTRADE and COMMONASSET Test
        /// EG 20180713 PERF Add POSDETOTC_ALLOC test
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetReferentialColumnHyperlink(ReferentialsReferential rr, ReferentialsReferentialColumn rrc, string pTableName)
        {
            string tblName = pTableName;
            string colName = rrc.ColumnName;
            //
            //Liste des tables pour lesquelles Spheres génère des hyperlink
            bool isOk = tblName.StartsWith("ACTOR") || tblName.StartsWith("BOOK");
            isOk |= (tblName == "TRADE") || (tblName == "TRADEADMIN");
            
            isOk |= (tblName == "DERIVATIVECONTRACT");
            isOk |= (tblName == "CONTRACT");
            
            isOk |= (tblName == "ASSET_ETD");
            isOk |= (tblName == "VW_ASSET_ETD_EXPANDED");

            isOk |= (tblName == "ASSET");
            isOk |= (tblName == "ASSET_ETD_ESE_COM");
            isOk |= (tblName == "COMMONASSET");


            isOk |= (tblName == "MARKET") || (tblName == "VW_MARKET_IDENTIFIER");
            
            isOk |= (tblName == "STLMESSAGE");
            isOk |= (tblName == "CNFMESSAGE" || tblName == "TS_CNFMESSAGE");
            //CNFMESSAGE => pour Confirmations 
            //TS_CNFMESSAGE => pour Editions
            //
            if (isOk)
            {
                //Liste des colonnes pour lesquelles Spheres génère des hyperlink
                isOk = ((colName == "IDENTIFIER") || (colName == "DISPLAYNAME"));
                if ((tblName == "VW_ASSET_ETD_EXPANDED") || (tblName == "COMMONASSET"))
                    isOk |= (colName == "CONTRACTIDENTIFIER") || (colName == "CONTRACTDISPLAYNAME");
                else if ((tblName == "MARKET") || (tblName == "VW_MARKET_IDENTIFIER"))
                    isOk |= (colName.StartsWith("SHORT_ACRONYM"));
                else if ((tblName.StartsWith("BOOK")))
                    isOk |= ((colName == "IDENTIFIER_MISSING"));
            }
            //
            if (isOk)
            {
                GetColumnDataHyperLink(tblName, colName, out string hyperLinkType, out string columnData, out string sqlColumnData);

                if (null == rr[columnData, rrc.AliasTableName])
                {
                    AddReferentialHideColumn(rr, rrc.AliasTableName, columnData, TypeData.TypeDataEnum.integer, sqlColumnData);

                    if (hyperLinkType == "IDASSET")
                    {
                        // si ajout de la colonne IDASSET ou IDCONTRACT, alors ajout de la colonne ASSETCATEGORY pour qualifier l'asset
                        string expression = "N/A";
                        if (null == rr["ASSETCATEGORY", rrc.AliasTableName])
                        {
                            if (pTableName == "ASSET" || pTableName == "COMMONASSET")
                            {
                                // ASSET => s'appuie sur la vue VW_ASSET qui possède la colonne ASSETCATEGORY
                                // COMMONASSET => ss-select qui expose la colonne ASSETCATEGORY
                                expression = null;
                            }
                            else if (pTableName == "ASSET_ETD_ESE_COM")
                            {
                                // Lecture de l'expression de la colonne ASSETCATEGORY de la LSTTABLE ASSET_ETD_ESE_COM
                                expression = GetSQLSELECT("ASSET_ETD_ESE_COM", "ASSETCATEGORY");
                            }
                            if (expression != "N/A")
                                AddReferentialHideColumn(rr, rrc.AliasTableName, "ASSETCATEGORY", TypeData.TypeDataEnum.@string, expression);
                        }
                    }
                    else if (hyperLinkType == "IDXC")
                    {
                        string expression = "N/A";
                        if (pTableName == "COMMONASSET")
                        {
                            // COMMONASSET => ss-select qui expose la colonne CONTRACTCATEGORY
                            expression = null;
                        }
                        else if (pTableName == "CONTRACT")
                        {
                            // Lecture de l'expression de la colonne CONTRACTCATEGORY de la LSTTABLE CONTRACT
                            expression = GetSQLSELECT(pTableName, "CONTRACTCATEGORY");
                        }
                        if (expression != "N/A")
                            AddReferentialHideColumn(rr, rrc.AliasTableName, "CONTRACTCATEGORY", TypeData.TypeDataEnum.@string, expression);
                    }
                }

                rrc.IsHyperLink = new ReferentialsReferentialColumnIsHyperLink();
                rrc.IsHyperLinkSpecified = true;

                rrc.IsHyperLink.linktypeSpecified = true;
                rrc.IsHyperLink.linktype = "column";

                rrc.IsHyperLink.Value = true;
                //
                //FI 20120926 [] usage of data attribute for HyperLink column
                rrc.IsHyperLink.dataSpecified = true;
                rrc.IsHyperLink.data = (rr[columnData, rrc.AliasTableName].DataField).ToUpper();

                rrc.IsHyperLink.typeSpecified = true;
                rrc.IsHyperLink.type = hyperLinkType;
            }
        }

        /// <summary>
        /// Création d'éléments SQLJoin au sein de ReferentialsReferential.
        /// </summary>
        /// <param name="rr"></param>
        // EG 20180629 PERF Replace Cst.DYNAMIC_ALIASTABLE
        private void SetReferentialSQLJoin(ReferentialsReferential rr)
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

                if (rr.AliasTableNameSpecified)
                    sqlJoin = sqlJoin.Replace(Cst.DYNAMIC_ALIASTABLE, rr.AliasTableName);
                rr.SQLJoin[indice] = sqlJoin;
                rr.SQLJoinSpecified = true;
                indice++;
            }
        }

        /// <summary>
        /// Ajout des colonnes techniques (colonne qui seront présentes dans le jeu de résultat) 
        /// </summary>
        /// <param name="rr"></param>
        /// FI 20171025 [23533] Modify
        private void AddReferentialTechnicalColumn(ReferentialsReferential rr)
        {
            // Add en dur en fonction de la consultation
            if (IsConsultation(ConsultEnum.MCO_RPT) || IsConsultation(ConsultEnum.MCO_RPT_FINPER))
            {
                #region add LOCNFMSGTXT
                ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn
                {
                    AliasTableNameSpecified = true,
                    AliasTableName = rr.AliasTableName,

                    ColumnName = "LOCNFMSGTXT",
                    AliasColumnNameSpecified = true,
                    IsHideInDataGridSpecified = true,
                    IsHideInDataGrid = true,
                    IsHideInCriteriaSpecified = true,
                    IsHideInCriteria = true,
                    LengthInDataGridSpecified = true,
                    LengthInDataGrid = -1,

                    IsResourceSpecified = true,
                    IsResource = new ReferentialsReferentialColumnResource(false)
                };
                rrc.DataType.value = TypeData.TypeDataEnum.text.ToString();
                rrc.AliasColumnName = rrc.ColumnName;
                rr.AddColumn(rrc);
                #endregion

                #region add LOCNFMSGBIN
                rrc = new ReferentialsReferentialColumn
                {
                    AliasTableNameSpecified = true,
                    AliasTableName = rr.AliasTableName,

                    ColumnName = "LOCNFMSGBIN",
                    AliasColumnNameSpecified = true,
                    IsHideInDataGridSpecified = true,
                    IsHideInDataGrid = true,
                    IsHideInCriteriaSpecified = true,
                    IsHideInCriteria = true,

                    LengthInDataGridSpecified = true,
                    LengthInDataGrid = -1,

                    IsResourceSpecified = true,
                    IsResource = new ReferentialsReferentialColumnResource(false)
                };
                rrc.DataType.value = TypeData.TypeDataEnum.image.ToString();
                rrc.AliasColumnName = rrc.ColumnName;

                rr.AddColumn(rrc);
                #endregion

                #region add DOCNAME
                rrc = new ReferentialsReferentialColumn
                {
                    AliasTableNameSpecified = true,
                    AliasTableName = rr.AliasTableName,

                    ColumnName = "DOCNAME",
                    AliasColumnNameSpecified = true,
                    IsHideInDataGridSpecified = true,
                    IsHideInDataGrid = true,
                    IsHideInCriteriaSpecified = true,
                    IsHideInCriteria = true,
                    LengthInDataGridSpecified = true,
                    LengthInDataGrid = -1,
                    IsVirtualColumnSpecified = true,
                    IsVirtualColumn = true,
                    IsResourceSpecified = true,
                    IsResource = new ReferentialsReferentialColumnResource(false)
                };

                rrc.AliasColumnName = rrc.ColumnName;
                rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                
                rr.AddColumn(rrc);
                #endregion
            }
            else if (IsConsultation(ConsultEnum.TRADEADMIN))
            {
                //FI 20120104 
                //La colonne GPRODUCT est nécessaire pour que les links redirige vers tradeAdminCapturePage.aspx 
                if (null == rr["GPRODUCT", "product"])
                {
                    ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn
                    {
                        AliasTableNameSpecified = true,
                        AliasTableName = "product",
                        ColumnName = "GPRODUCT",
                        IsResourceSpecified = true,
                        IsResource = new ReferentialsReferentialColumnResource(false),
                        IsHideInDataGridSpecified = true,
                        IsHideInDataGrid = true
                    };
                    rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();

                    rr.AddColumn(rrc);
                }
            }
            // FI 20171025 [23533] Ajout de la colonne TZFACILITY si elle n'existe pas 
            if (null != rr["DTEXECUTION"] || null != rr["DTORDERENTERED"] ||
                null != rr["TSEXECUTION"] || null != rr["TSORDERENTERED"])
            {
                string alias = null;
                if (null != rr["DTEXECUTION"])
                    alias = rr["DTEXECUTION"].AliasTableName;
                else if (null != rr["DTORDERENTERED"])
                    alias = rr["DTORDERENTERED"].AliasTableName;
                else if (null != rr["TSEXECUTION"])
                    alias = rr["TSEXECUTION"].AliasTableName;
                else if (null != rr["TSORDERENTERED"])
                    alias = rr["TSORDERENTERED"].AliasTableName;

                if (null == rr["TZFACILITY", alias])
                {
                    ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn
                    {
                        AliasTableNameSpecified = true,
                        AliasTableName = alias,
                        ColumnName = "TZFACILITY",
                        IsResourceSpecified = true,
                        IsResource = new ReferentialsReferentialColumnResource(false),
                        IsHideInDataGridSpecified = true,
                        IsHideInDataGrid = true
                    };
                    rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();

                    rr.AddColumn(rrc);

                }
            }

            // FI 20221207 [XXXXX]
            if (null != rr["DTDLVYSTART"] || null != rr["DTDLVYEND"])
            {
                string alias = null;
                if (null != rr["DTDLVYSTART"])
                    alias = rr["DTDLVYSTART"].AliasTableName;
                else if (null != rr["DTDLVYEND"])
                    alias = rr["DTDLVYEND"].AliasTableName;

                if (null == rr["TZTZDLVY", alias])
                {
                    ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn
                    {
                        AliasTableNameSpecified = true,
                        AliasTableName = alias,
                        ColumnName = "TZDLVY"
                    };
                    rrc.DataType.value = TypeData.TypeDataEnum.@string.ToString();

                    rrc.IsResourceSpecified = true;
                    rrc.IsResource = new ReferentialsReferentialColumnResource(false);

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
        /// <param name="pIndexStart"></param>
        /// FI 20140519 [19923] Add Method
        private void AddReferentialSelectedColumn(ReferentialsReferential rr, int pIndexStart)
        {
            ArrayList listColumnAlias = new ArrayList();
            bool isFound_ColumnRef = false;

            int index = pIndexStart;
            foreach (DataRow row in dtLstSelectedCol.Rows)
            {
                string columnXML = string.Empty;
                if (false == (row["COLUMNXML"] is DBNull))
                    columnXML = row["COLUMNXML"].ToString();

                if (StrFunc.IsFilled(columnXML))
                {
                    EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(ReferentialsReferentialColumn), columnXML);
                    rr.Column[index] = (ReferentialsReferentialColumn)CacheSerializer.Deserialize(serializerInfo);
                }
                else
                    rr.Column[index] = new ReferentialsReferentialColumn();

                rr.Column[index].Ressource = row["DISPLAYNAME"].ToString();
                rr.Column[index].RessourceSpecified = true;
                if (IsMultiTable(SessionTools.CS))
                    rr.Column[index].Ressource = "[" + row["ALIASDISPLAYNAME"].ToString() + "] " + Cst.HTMLBreakLine + rr.Column[index].Ressource;
                rr.Column[index].ColumnName = row["COLUMNNAME"].ToString().Trim();

                rr.Column[index].AliasTableNameSpecified = true;
                rr.Column[index].AliasTableName = row["ALIAS"].ToString().Trim();


                //RD 20110524 [] Affichage d'un Header commun à plusieurs colonnes
                rr.Column[index].ColumnRef = (row["COLUMNREF"] is DBNull ? string.Empty : row["COLUMNREF"].ToString().Trim());
                SetColumnRessourceFromColumnRef(rr, index, ref isFound_ColumnRef);

                string sqlSelect = (row["SQLSELECT"] is DBNull ? string.Empty : row["SQLSELECT"].ToString().Trim());
                if (StrFunc.IsFilled(sqlSelect))
                    rr.Column[index].ColumnNameOrColumnSQLSelect = sqlSelect.Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[index].AliasTableName);
                else
                    rr.Column[index].ColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameExpression(rr.Column[index].ColumnName, rr.Column[index].AliasTableName);
                rr.Column[index].ColumnNameOrColumnSQLSelectSpecified = StrFunc.IsFilled(rr.Column[index].ColumnNameOrColumnSQLSelect);

                // FI 20200820 [25468] si le datatype est déjà présent dans columnXML il ne faut pas l'écraser
                if (false == rr.Column[index].DataTypeSpecified)
                {
                    rr.Column[index].DataType.value = row["DATATYPE"].ToString().Trim();
                    rr.Column[index].DataTypeSpecified = true;
                }

                rr.Column[index].LengthSpecified = true;
                rr.Column[index].Length = 50;

                rr.Column[index].ScaleSpecified = true;
                if (row["SCALE"] is DBNull)
                    rr.Column[index].Scale = 0;
                else
                    rr.Column[index].Scale = Convert.ToInt32(row["SCALE"]);

                rr.Column[index].GroupBySpecified = true;
                rr.Column[index].GroupBy = new ReferentialsReferentialColumnGroupBy
                {
                    IsGroupBy = (false == (row["ISGROUPBY"] is DBNull || BoolFunc.IsFalse(row["ISGROUPBY"]))),
                    GroupingSet = Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET")
                };

                if (row["AGGREGATE"] is DBNull)
                {
                    rr.Column[index].GroupBy.AggregateSpecified = false;
                }
                else
                {
                    rr.Column[index].GroupBy.Aggregate = Convert.ToString(row["AGGREGATE"]);
                    rr.Column[index].GroupBy.AggregateSpecified = StrFunc.IsFilled(rr.Column[index].GroupBy.Aggregate);
                    rr.HasAggregateColumns = rr.HasAggregateColumns || rr.Column[index].GroupBy.AggregateSpecified;
                }

                if (row["SQLGROUPBY"] is DBNull)
                {
                    rr.Column[index].GroupBy.SqlGroupBySpecified = false;
                }
                else
                {
                    //PL 20120426 Bug
                    //rr.Column[indice].GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]);
                    rr.Column[index].GroupBy.SqlGroupBy = Convert.ToString(row["SQLGROUPBY"]).Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[index].AliasTableName);
                    rr.Column[index].GroupBy.SqlGroupBySpecified = StrFunc.IsFilled(rr.Column[index].GroupBy.SqlGroupBy);
                }


                //FI 20111026  ajout de false == rr.Column[indice].IsHideInDataGridSpecified
                //Cette donnée peut être alimentée par COLUMNXML
                if (false == rr.Column[index].IsHideInDataGridSpecified)
                {
                    rr.Column[index].IsHideInDataGridSpecified = true;
                    rr.Column[index].IsHideInDataGrid = false;
                }

                //FI 20111026  ajout de false == rr.Column[indice].IsHideInCriteriaSpecified
                if (false == rr.Column[index].IsHideInCriteriaSpecified)
                {
                    rr.Column[index].IsHideInCriteriaSpecified = true;
                    rr.Column[index].IsHideInCriteria = false;
                }

                //FI 20190830 [XXXXX] ajout de if (false == rr.Column[index].IsDataKeyFieldSpecified)
                if (false == rr.Column[index].IsDataKeyFieldSpecified)
                {
                    rr.Column[index].IsDataKeyFieldSpecified = true;
                    rr.Column[index].IsDataKeyField = false;
                    if (rr.TableName == Cst.OTCml_TBL.TRADE.ToString() && rr.Column[index].ColumnName == "IDT")
                        rr.Column[index].IsDataKeyField = true;
                }

                rr.Column[index].IsKeyFieldSpecified = true;
                rr.Column[index].IsKeyField = false;

                rr.Column[index].IsForeignKeyFieldSpecified = true;
                rr.Column[index].IsForeignKeyField = false;

                rr.Column[index].IsIdentitySpecified = true;
                rr.Column[index].IsIdentity.Value = false;
                //
                //FI 20120412  ajout de false == rr.Column[indice].IsResourceSpecified
                //Cette donnée peut être alimentée par COLUMNXML
                if (false == rr.Column[index].IsResourceSpecified)
                {
                    rr.Column[index].IsResourceSpecified = true;
                    rr.Column[index].IsResource = new ReferentialsReferentialColumnResource(Convert.ToBoolean(row["ISRESOURCE"]));
                }

                //FI 20111026  ajout de rr.Column[indice].AliasColumnNameSpecified
                if (rr.Column[index].AliasColumnNameSpecified)
                {
                    rr.Column[index].DataField = rr.Column[index].AliasColumnName;
                }
                else
                {
                    //20090820 RD/ Pour eviter les doublons sur les Alias                    
                    rr.Column[index].DataField = ReferentialTools.GetNewAlias(rr.Column[index].AliasTableName + "_" + rr.Column[index].ColumnName, ref listColumnAlias);
                }

                rr.Column[index].ColspanSpecified = true;

                rr.Column[index].IsOrderBySpecified = true;
                rr.Column[index].IsOrderBy.Value = "false";
                if (StrFunc.IsFilled(row["SQLORDER"].ToString()))
                {
                    rr.Column[index].IsOrderBy.orderSpecified = true;
                    rr.Column[index].IsOrderBy.order = row["SQLORDER"].ToString().Trim().Replace(Cst.DYNAMIC_ALIASTABLE, rr.Column[index].AliasTableName);
                }
                rr.Column[index].IsUpdatableSpecified = false; //pourkoi ???? false
                SetReferentialColumnHyperlink(rr, rr.Column[index], row["TABLENAME"].ToString());
                index++;
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
        /// <para>Par Exemple pour la consultation REF-RepositoryACTOR, retourne ACTOR</para>
        /// </summary>
        public string ReferentialShortIdConsult()
        {
            string ret = IdLstConsult;

            foreach (Cst.ListType type in Enum.GetValues(typeof(Cst.ListType)))
            {
                string tmp = ReferentialWeb.PrefixForReferential + type.ToString();
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
        private static void AddReferentialHideColumn(ReferentialsReferential rr,
            string pAliasTable, string pColumnName, TypeData.TypeDataEnum pDataType, string sqlExpression)
        {
            ArrayList al = new ArrayList();
            ReferentialsReferentialColumn rc = new ReferentialsReferentialColumn
            {
                ColumnName = pColumnName,
                AliasTableNameSpecified = true,
                AliasTableName = pAliasTable,
                ScaleSpecified = true,
                Scale = 0,
                IsHideInCriteriaSpecified = true,
                IsHideInCriteria = true,
                IsHideInDataGridSpecified = true,
                IsHideInDataGrid = true,
                IsHideSpecified = true,
                IsHide = true
            };
            rc.DataType.value = pDataType.ToString();
            rc.DataField = ReferentialTools.GetNewAlias(rc.AliasTableName + "_" + rc.ColumnName, ref al);

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
        /// FI 20170302 [XXXXX] Modify
        /// EG 20180713 PERF Add POSDETOTC_ALLOC TEST
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// FI 20201201 [XXXXX] Refactoring 
        private void GetColumnDataHyperLink(string pTableName, string pColName,
            out string hyperLinkType,
            out string columnData,
            out string sqlColumnData)
        {
            columnData = string.Empty;
            sqlColumnData = string.Empty;
            hyperLinkType = string.Empty;
            bool isOk = true;

            if (pTableName.StartsWith(Cst.OTCml_TBL.ACTOR.ToString()))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDA";
                        hyperLinkType = "IDA";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if (pTableName.StartsWith(Cst.OTCml_TBL.BOOK.ToString()))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                    case "IDENTIFIER_MISSING":
                        columnData = "IDB";
                        hyperLinkType = "IDB";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if (pTableName == Cst.OTCml_TBL.ASSET_ETD.ToString())
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDASSET";
                        hyperLinkType = "IDASSET_ETD";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if (pTableName == Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED.ToString())
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDASSET";
                        hyperLinkType = "IDASSET_ETD";
                        break;
                    case "CONTRACTIDENTIFIER":
                    case "CONTRACTDISPLAYNAME":
                        columnData = "IDDC";
                        hyperLinkType = "IDDC";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if ((pTableName == "ASSET_ETD_ESE_COM") || (pTableName == "ASSET"))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDASSET";
                        hyperLinkType = "IDASSET";
                        break;
                    default:
                        isOk = false;
                        break; throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));
                }
            }
            else if (pTableName == "COMMONASSET")
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDASSET";
                        hyperLinkType = "IDASSET";
                        break;
                    case "CONTRACTIDENTIFIER":
                    case "CONTRACTDISPLAYNAME":
                        columnData = "IDXC";
                        hyperLinkType = "IDXC";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if (pTableName == "DERIVATIVECONTRACT")
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDDC";
                        hyperLinkType = "IDDC";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if (pTableName == "CONTRACT")
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDXC";
                        hyperLinkType = "IDXC";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if ((pTableName == "TRADE") || (pTableName == "TRADEADMIN") || (pTableName == "FLOWSBYTRADE"))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDT";
                        hyperLinkType = "IDT";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if ((pTableName == "VW_MARKET_IDENTIFIER") ||
                     (pTableName == "MARKET"))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                    case "SHORT_ACRONYM":
                    case "SHORT_ACRONYM_2":
                    case "SHORT_ACRONYM_3":
                        columnData = "IDM";
                        hyperLinkType = "IDM";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if ((pTableName == "CNFMESSAGE") ||
                      (pTableName == "TS_CNFMESSAGE"))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDCNFMESSAGE";
                        hyperLinkType = "IDCNFMESSAGE";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else if ((pTableName == "STLMESSAGE"))
            {
                switch (pColName)
                {
                    case "IDENTIFIER":
                    case "DISPLAYNAME":
                        columnData = "IDSTLMESSAGE";
                        hyperLinkType = "IDSTLMESSAGE";
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            else
            {
                isOk = false;
            }

            if (false == isOk)
                throw new NotImplementedException(StrFunc.AppendFormat("Link is not implemented [Table:{0}][Col:{1}]", pTableName, pColName));

            sqlColumnData = GetSQLSELECT(pTableName, columnData);
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Add
        private static string GetSQLSELECT(string pTableName, string pColumnName)
        {
            string cs = CSTools.SetCacheOn(SessionTools.CS);

            string ret = null;
            string cmd = $"select SQLSELECT from dbo.LSTCOLUMN where TABLENAME = '{pTableName}' and COLUMNNAME='{pColumnName}'";
            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, cmd))
            {
                if (dr.Read())
                {
                    ret = (dr["SQLSELECT"] != Convert.DBNull) ? Convert.ToString(dr["SQLSELECT"]) : null;
                }
            }
            return ret;
        }

        /// <summary>
        /// Création d'une template temporaire à partir du template courant
        /// <para>si le user est proprietaire, création d'une copie temporaire (le propriétaire du template temporaire est l'utilisateur courant)</para>
        /// <para>si le user n'est pas proprietaire mais qu'il a les droits de modification, création d'une copie temporaire (le propriétaire du template temporaire est le propriétaire du template source)</para>
        /// <para>si le user n'est pas proprietaire et qu'il n'a pas les droits de modification, création d'une copie temporaire (le propriétaire du template temporaire est l'utilisateur courant)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20200602 [25370] Add 
        public Pair<string, int> CreateCopyTemporaryTemplate(string pCS)
        {
            Pair<string, int> ret;

            if (template.IsUserOwner(SessionTools.Collaborator_IDA))
            {
                // si le user est proprietaire, on créé un modele temporaire de son modele
                ret = new Pair<string, int>
                {
                    First = CreateCopyTemporaryTemplate(pCS, template.IDLSTTEMPLATE, SessionTools.Collaborator_IDA, SessionTools.Collaborator_IDA),
                    Second = SessionTools.User.IdA
                };
            }
            else
            {
                bool hasUserRightModify = template.HasUserRight(CSTools.SetCacheOn(pCS), SessionTools.User, RightsTypeEnum.MODIFY);
                if (hasUserRightModify)
                {
                    // si le user n'est pas proprietaire mais qu'il a les droits de modification, on cree un modele temporaire du modele original
                    ret = new Pair<string, int>
                    {
                        First = CreateCopyTemporaryTemplate(pCS, template.IDLSTTEMPLATE, template.IDA, template.IDA),
                        Second = template.IDA
                    };
                }
                else
                {
                    ret = new Pair<string, int>
                    {
                        // si le user n'est pas proprietaire et qu'il n'a pas les droits de modification, on cree une copie temporaire de ce modele
                        First = CreateCopyTemporaryTemplate(pCS, template.IDLSTTEMPLATE, template.IDA, SessionTools.Collaborator_IDA),
                        Second = SessionTools.User.IdA
                    };
                }
            }
            return ret;
        }

        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20200226 [25077] RDBMS : Correction gestion EXCLUDE et INCLUDE COLUMNNAME
        /// FI 20201201 [XXXXX] Add (Methode dépalcée ici)
        private static DataTable LoadLstSelectAvailable(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
        {
            #region SQLSelect
            DataParameters dbParam = new DataParameters();
            dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTTEMPLATE), pIdLstTemplate);
            dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), pIdLstConsult);
            dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            string sqlSelect = @"select lco.IDLSTCOLUMN,lco.DISPLAYNAME,lco.DATATYPE,lco.TABLENAME,lco.COLUMNNAME,lco.COLUMNNAMEREF as COLUMNREF, lco.COLUMNXML,
            lca.ALIASHEADER, lca.POSITION, lcaj.IDLSTJOIN, lcaj.IDLSTJOIN as ALIASDISPLAYNAME, la.ALIAS,
            case when lcw.IDLSTCONSULT is null then 0 else 1 end as ISMANDATORY
            from dbo.LSTTEMPLATE ltmp
            inner join dbo.LSTCONSULTALIAS lca on (lca.IDLSTCONSULT=ltmp.IDLSTCONSULT)
            inner join dbo.LSTALIAS la on (la.ALIAS=lca.ALIAS)
            inner join dbo.LSTCOLUMN lco on (lco.TABLENAME=la.TABLENAME) and 
            ((lca.EXCLUDECOLUMNNAME is null) or ((';' || lca.EXCLUDECOLUMNNAME || ';') not like ('%;' || lco.COLUMNNAME || ';%'))) and 
            ((lca.INCLUDECOLUMNNAME is null) or ((';' || lca.INCLUDECOLUMNNAME || ';') like ('%;' || lco.COLUMNNAME || ';%'))) 
            inner join dbo.LSTCONSALIASJOIN lcaj on(lcaj.IDLSTCONSULT=ltmp.IDLSTCONSULT) and  (lcaj.ALIAS=lca.ALIAS) and 
            lcaj.POSITION = (
                select max(lcaj2.POSITION) 
                from dbo.LSTCONSALIASJOIN lcaj2 
                where (lcaj2.IDLSTCONSULT=lcaj.IDLSTCONSULT) and (lcaj2.ALIAS=lcaj.ALIAS)
            )
            left outer join  dbo.LSTCONSULTWHERE lcw on (lcw.IDLSTCONSULT = ltmp.IDLSTCONSULT)  and (lcw.TABLENAME = lco.TABLENAME) and (lcw.COLUMNNAME = lco.COLUMNNAME)
            where (ltmp.IDLSTTEMPLATE = @IDLSTTEMPLATE) and (ltmp.IDLSTCONSULT = @IDLSTCONSULT) and (ltmp.IDA = @IDA)" + Cst.CrLf;

            #endregion SQLSelect

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dbParam);
            DataTable dtLstSelect = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            #region Replace data by ressource
            bool isExistDoublon = false;

            foreach (DataRow dr in dtLstSelect.Rows)
            {
                if (StrFunc.IsFilled(dr["ALIASDISPLAYNAME"].ToString()))
                {
                    dr["ALIASDISPLAYNAME"] = Ressource.GetMultiForDatagrid(pIdLstConsult, dr["TABLENAME"].ToString(), "TBL_" + dr["ALIASDISPLAYNAME"].ToString().Trim());
                    dr["DISPLAYNAME"] = Ressource.GetMultiForDatagrid(pIdLstConsult, dr["IDLSTJOIN"].ToString().Trim(), "COL_" + dr["DISPLAYNAME"].ToString().Trim()); //PL 20121024
                }

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                //EG/PL 20180628 Exclude column already exist on main table
                //PL 20181126 [24336] Code amendé !
                //                    On exclut la colonnes de même nom présentes sur les éléments "enfants", c'est à dire disposant d'un ALIASHEADER différent de ALIAS
                //                    et pour lesquels le "parent" comporte un ALIASHEADER renseigné et égal à son ALIAS.
                //                    NB: Cette évolution (astuce) permet le cas échéant de ne pas exclure des colonnes de même nom. 
                //                        Pour cela, il suffit de laisser à NULL le ALAISHEADER de la table "parent" (ex. Frais sur TRADEOTC_ALLOC)
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                if (StrFunc.IsFilled(dr["ALIASHEADER"].ToString()) && (dr["ALIASHEADER"].ToString() == dr["ALIAS"].ToString()))
                {
                    string colName = DataHelper.SQLString(dr["COLUMNNAME"].ToString());
                    string colAliasHeader = DataHelper.SQLString(dr["ALIASHEADER"].ToString());
                    DataRow[] drDoublon = dtLstSelect.Select("COLUMNNAME=" + colName + " and ALIASHEADER=" + colAliasHeader + " and ALIAS<>ALIASHEADER");
                    if (drDoublon.Length == 1) //EG/PL 20181119 Laisser " == 1 ", car cela nous sauve (initialement involontairement) la mise sur les cas de figure où il existe N éléments identiques (ex. Montants de la table EVENT)
                    {
                        isExistDoublon = true;
                        drDoublon[0]["ALIASHEADER"] = "<ToDelete>";
                    }
                }
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            }
            if (isExistDoublon)
            {
                DataRow[] drForRemove = dtLstSelect.Select("ALIASHEADER='<ToDelete>'");
                for (int i = 0; i < drForRemove.Length; i++)
                    drForRemove[i].Delete();
                dtLstSelect.AcceptChanges();
            }

            string lastHeader = string.Empty;
            foreach (DataRow dr in dtLstSelect.Rows)
            {
                if (StrFunc.IsEmpty(dr["ALIASHEADER"].ToString()))
                    dr["ALIASHEADER"] = dr["ALIAS"];

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                //-----------------------------------------------------------------------------------------------------
                //PL 20121022 Mise à jour de la POSITION des tables annexes d'un même ALIASHEADER
                int currentPosition = Convert.ToInt32(dr["POSITION"].ToString());
                string currentHeader = dr["ALIASHEADER"].ToString();
                if (lastHeader != currentHeader)
                {
                    int lastPosition = currentPosition;
                    lastHeader = currentHeader;
                }
                else
                {
                    dr["POSITION"] = currentPosition;
                }
                //-----------------------------------------------------------------------------------------------------
            }
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            #endregion

            return dtLstSelect;
        }

        #endregion

        #region class LSTTEMPLATE
        /// <summary>
        /// Classe image d'un template employée pour la gestion de la consultation multi-critères
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public class LstTemplate
        {
            #region Constants
            public const string TEMPORARYPREFIX = "*";
            #endregion
            //
            #region Members
            public string titleDisplayName;
            public string subTitle;
            public string titleOwner;
            //
            public string IDLSTTEMPLATE;
            public string IDLSTCONSULT;
            public string DISPLAYNAME;
            public string DESCRIPTION;
            public string CSSCOLOR;
            public int ROWBYPAGE;
            public string RIGHTPUBLIC;
            public string RIGHTDESK;
            public string RIGHTDEPARTMENT;
            // EG 20210505 [25700] FreezeGrid implementation 
            public string RIGHTENTITY;
            public int FREEZECOL;
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
            //
            #region Accessor
            /// <summary>
            /// Obtient le nom du Template sans le prefix *
            /// </summary>
            public string IDLSTTEMPLATE_WithoutPrefix
            {
                get { return ReferentialWeb.RemoveTemporaryPrefix(IDLSTTEMPLATE); }
            }
            /// <summary>
            /// Obtient le true si le Template commence par le prefix *
            /// </summary>
            public bool IsTemporary
            {
                get { return ReferentialWeb.IsTemporary(IDLSTTEMPLATE); }
            }
            /// <summary>
            /// Obtient le true si le rafraîssement de page est spécifé
            /// </summary>
            public bool IsRefreshIntervalSpecified
            {
                get { return (REFRESHINTERVAL >= ReferentialWeb.MinRefreshInterval); }
            }
            #endregion

            #region Constructor(s)
            public LstTemplate()
            {
                titleDisplayName = string.Empty;
                subTitle = string.Empty;
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
            // EG 20180426 Analyse du code Correction [CA2202]
            // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
            // EG 20210505 [25700] FreezeGrid implementation 
            public bool Load(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
            {
                bool isOwner = (pIdA == SessionTools.Collaborator_IDA);
                bool ret = false;

                IDLSTCONSULT = pIdLstConsult;
                IDLSTTEMPLATE = pIdLstTemplate;
                IDA = pIdA;

                ISDEFAULT = false;

                StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
                SQLSelect += @" lt.IDLSTTEMPLATE, lt.IDLSTCONSULT, lt.DISPLAYNAME, lt.DESCRIPTION, lt.CSSFILENAME, 
                                lt.ROWBYPAGE, lt.RIGHTPUBLIC, lt.RIGHTDESK, lt.RIGHTDEPARTMENT, lt.RIGHTENTITY, lt.DTUPD, 
                                lt.IDAUPD, lt.DTINS, lt.IDAINS, lt.EXTLLINK, lt.ROWATTRIBUT, 
                                lt.ROWVERSION,lt.ISENABLEDLSTWHERE,lt.IDA , lt.ISLOADONSTART, lt.REFRESHINTERVAL, lt.FREEZECOL,
                                a1.DISPLAYNAME, a2.DISPLAYNAME, a3.DISPLAYNAME " + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt" + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a1 on (a1.IDA = lt.IDA)" + Cst.CrLf;
                SQLSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a2 on (a2.IDA = lt.IDAUPD)" + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a3 on (a3.IDA = lt.IDAINS)" + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, "lt", out DataParameters dbParam);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (dr.Read())
                    {
                        int i = -1;
                        i++; IDLSTTEMPLATE = dr.GetValue(i).ToString();
                        i++; IDLSTCONSULT = dr.GetValue(i).ToString();
                        i++; DISPLAYNAME = dr.GetValue(i).ToString();
                        i++; DESCRIPTION = dr.GetValue(i).ToString();
                        i++; CSSCOLOR = dr.GetValue(i).ToString();
                        i++; ROWBYPAGE = Convert.ToInt32((dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : "0"));
                        i++; RIGHTPUBLIC = dr.GetValue(i).ToString();
                        i++; RIGHTDESK = dr.GetValue(i).ToString();
                        i++; RIGHTDEPARTMENT = dr.GetValue(i).ToString();
                        i++; RIGHTENTITY = dr.GetValue(i).ToString();
                        // FI 20200820 [25468] dates systèmes en UTC
                        i++; DTUPD = DateTime.SpecifyKind(Convert.ToDateTime(dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i) : dr.GetValue(i + 2)), DateTimeKind.Utc);
                        i++; IDAUPD = Convert.ToInt32((dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : dr.GetValue(i + 2).ToString()));
                        // FI 20200820 [25468] dates systèmes en UTC
                        i++; DTINS = DateTime.SpecifyKind(Convert.ToDateTime(dr.GetValue(i)), DateTimeKind.Utc);
                        i++; IDAINS = Convert.ToInt32((dr.GetValue(i)));
                        i++; EXTLLINK = dr.GetValue(i).ToString();
                        i++; ROWATTRIBUT = dr.GetValue(i).ToString();
                        i++; ROWVERSION = dr.GetValue(i).ToString();
                        i++; ISENABLEDLSTWHERE = Convert.ToBoolean(dr.GetValue(i));
                        i++; IDA = Convert.ToInt32((dr.GetValue(i)));
                        i++; ISLOADONSTART = Convert.ToBoolean(dr.GetValue(i));
                        i++; REFRESHINTERVAL = (dr.GetValue(i) == Convert.DBNull ? 0 : Convert.ToInt32(dr.GetValue(i)));
                        i++; FREEZECOL = Convert.ToInt32((dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : "0"));
                        i++; IDADisplayName = dr.GetValue(i).ToString();
                        i++; IDAUPDDisplayName = (dr.GetValue(i).ToString() != null && dr.GetValue(i).ToString().Length > 0 ? dr.GetValue(i).ToString() : dr.GetValue(i + 1).ToString());
                        i++; IDAINSDisplayName = dr.GetValue(i).ToString();
                        //
                        titleDisplayName = IDLSTTEMPLATE + (IDLSTTEMPLATE == DISPLAYNAME ? string.Empty : " - " + DISPLAYNAME);
                        subTitle = (IsTemporary ? IDLSTTEMPLATE_WithoutPrefix : IDLSTTEMPLATE + (IDLSTTEMPLATE == DISPLAYNAME ? string.Empty : " - " + DISPLAYNAME));
                        titleOwner = (isOwner ? string.Empty : " [" + Ressource.GetString("Owner") + ": " + IDADisplayName + "]");
                        //
                        ret = true;
                    }
                }

                if (isOwner)
                {
                    // FI 20200803 [XXXXX] (Element de travail TFS: 33 - LST et modèle par défaut)  
                    //string name = (isTemporary ? IDLSTTEMPLATE_WithoutPrefix : IDLSTTEMPLATE);
                    string name = IDLSTTEMPLATE;
                    SQLSelect = new StrBuilder();
                    SQLSelect += SQLCst.SELECT + "1" + Cst.CrLf;
                    SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                    SQLSelect += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, name, IDA, null, out dbParam);

                    qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
                    object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    ISDEFAULT = (null != obj);
                }
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
            // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
            // EG 20210505 [25700] FreezeGrid implementation 
            public void Insert(string pCS, int pIdA)
            {
                //
                string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + @" (
				    IDLSTTEMPLATE, IDLSTCONSULT, DISPLAYNAME, DESCRIPTION, CSSFILENAME,
				    ROWBYPAGE, RIGHTPUBLIC, RIGHTDESK, RIGHTDEPARTMENT, RIGHTENTITY, FREEZECOL, DTINS,
				    IDAINS, EXTLLINK, IDA, ISENABLEDLSTWHERE, ISLOADONSTART, REFRESHINTERVAL
                    ) values (  
				    @IDLSTTEMPLATE, @IDLSTCONSULT, @DISPLAYNAME, @DESCRIPTION, @CSSFILENAME,
				    @ROWBYPAGE, @RIGHTPUBLIC, @RIGHTDESK, @RIGHTDEPARTMENT, @RIGHTENTITY, @FREEZECOL, " + DataHelper.SQLGetDate(pCS) + @",
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
                dp.Add(new DataParameter(pCS, "CSSFILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), CSSCOLOR);
                dp.Add(new DataParameter(pCS, "ROWBYPAGE", DbType.Int32), ROWBYPAGE);
                dp.Add(new DataParameter(pCS, "FREEZECOL", DbType.Int32), FREEZECOL);
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
            // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
            // EG 20210505 [25700] FreezeGrid implementation 
            public bool Update(string pCS)
            {
                bool ret = false;

                IDAUPD = SessionTools.Collaborator_IDA;

                string SQLUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + @" set 
                                    DISPLAYNAME        = @DISPLAYNAME,
                                    DESCRIPTION        = @DESCRIPTION,
                                    CSSFILENAME        = @CSSFILENAME,
                                    ROWBYPAGE          = @ROWBYPAGE,
                                    RIGHTPUBLIC        = @RIGHTPUBLIC,
                                    RIGHTDESK          = @RIGHTDESK,
                                    RIGHTDEPARTMENT    = @RIGHTDEPARTMENT,
                                    RIGHTENTITY        = @RIGHTENTITY,
                                    FREEZECOL          = @FREEZECOL,
                                    DTUPD              = " + DataHelper.SQLGetDate(pCS) + @",
                                    IDAUPD             = @IDAUPD,
                                    EXTLLINK           = @EXTLLINK,
                                    ROWATTRIBUT        = @ROWATTRIBUT,
                                    ISENABLEDLSTWHERE  = @ISENABLEDLSTWHERE,
                                    ISLOADONSTART      = @ISLOADONSTART,
                                    REFRESHINTERVAL    = @REFRESHINTERVAL";
                SQLUpdate += Cst.CrLf + SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out DataParameters dbParam);

                dbParam.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), DISPLAYNAME);
                dbParam.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), DESCRIPTION);
                dbParam.Add(new DataParameter(pCS, "CSSFILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), CSSCOLOR);
                dbParam.Add(new DataParameter(pCS, "ROWBYPAGE", DbType.Int32), ROWBYPAGE);
                dbParam.Add(new DataParameter(pCS, "RIGHTPUBLIC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTPUBLIC);
                dbParam.Add(new DataParameter(pCS, "RIGHTDESK", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDESK);
                dbParam.Add(new DataParameter(pCS, "RIGHTDEPARTMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTDEPARTMENT);
                dbParam.Add(new DataParameter(pCS, "RIGHTENTITY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), RIGHTENTITY);
                dbParam.Add(new DataParameter(pCS, "FREEZECOL", DbType.Int32), FREEZECOL);
                dbParam.Add(new DataParameter(pCS, "IDAUPD", DbType.Int32), IDAUPD);
                dbParam.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), EXTLLINK);
                dbParam.Add(new DataParameter(pCS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN), ROWATTRIBUT);
                dbParam.Add(new DataParameter(pCS, "ISENABLEDLSTWHERE", DbType.Boolean), ISENABLEDLSTWHERE);
                dbParam.Add(new DataParameter(pCS, "ISLOADONSTART", DbType.Boolean), ISLOADONSTART);
                dbParam.Add(new DataParameter(pCS, "REFRESHINTERVAL", DbType.Int32), REFRESHINTERVAL);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

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
            /// Mise à jour du champ LSTTEMPLATE.ISENABLEDLSTWHERE 
            /// </summary>
            /// <param name="pCS"></param>
            /// <returns></returns>
            /// FI 20200602 [25370] Reecriture de la méthode SetIsEnabledLstWhere 
            public Cst.ErrLevel SetIsEnabledLstWhere2(string pCS)
            {
                Cst.ErrLevel ret;

                StrBuilder SQLUpdate = new StrBuilder();
                SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLUpdate += @" Set ISENABLEDLSTWHERE = @ISENABLEDLSTWHERE";
                SQLUpdate += Cst.CrLf + SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, this.IDLSTCONSULT, this.IDLSTTEMPLATE, this.IDA, null, out DataParameters dbParam);

                dbParam.Add(new DataParameter(pCS, "ISENABLEDLSTWHERE", DbType.Boolean), ISENABLEDLSTWHERE);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (nbRow == 1)
                    ret = Cst.ErrLevel.SUCCESS;
                else
                    ret = Cst.ErrLevel.ABORTED;

                return ret;
            }
            #endregion SetIsEnabledLstWhere

            #region public SetEnabledLstWhereElement
            /// <summary>
            /// Update LSTWHERE, met à jour la colonne ISENABLED du critère en position {pPosition}   
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pPosition"></param>
            /// <param name="pIsEnabled"></param>
            public void SetEnabledLstWhereElement(string pCS, int pPosition, bool pIsEnabled)
            {
                StrBuilder SQLUpdate = new StrBuilder();
                SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTWHERE + Cst.CrLf;
                SQLUpdate += "set ISENABLED = @ISENABLED" + Cst.CrLf;
                SQLUpdate += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out DataParameters dbParam);
                SQLUpdate += SQLCst.AND + "POSITION=@POSITION";

                dbParam.Add(new DataParameter(pCS, "ISENABLED", DbType.Boolean), pIsEnabled);
                dbParam.Add(new DataParameter(pCS, "POSITION", DbType.Int32), pPosition);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                _ = DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            #endregion

            /// <summary>
            /// Initialisation/Update de LSTPARAM à partir des valeurs présentes dans {pDynamicData} 
            /// <para></para>
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pDynamicArgsGUIAndDefault"></param>
            /// <param name="pIsModeInit">si true, Alimentation de LSTPARAM lorsque non alimentée (1er initialisation). Si False Update de LSTPARAM </param>
            /// FI 20200602 [25370] Add Method
            public void UpdateLstParam(string pCS, IEnumerable<Pair<ReferentialsReferentialStringDynamicData, string>> pDynamicArgsGUIAndDefault, Boolean pIsModeInit)
            {
                if (null == pDynamicArgsGUIAndDefault)
                    throw new ArgumentException("dynamicData argument is null");
                if (pDynamicArgsGUIAndDefault.Where(x => !(x.First.source.HasFlag(DynamicDataSourceEnum.GUI))).Count() > 0)
                    throw new ArgumentException("Only GUI dynamicData are expected");

                if (pDynamicArgsGUIAndDefault.Count() > 0)
                {
                    string queryFrom = StrFunc.AppendFormat("select {0} from dbo.LSTPARAM", AllColsForTableChild(Cst.OTCml_TBL.LSTPARAM.ToString(), null, null, -1));
                    String querywhere = SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out DataParameters dbParam);

                    using (DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, queryFrom + Cst.CrLf + querywhere, dbParam.GetArrayDbParameter()))
                    {
                        using (DataTable dt = ds.Tables[0])
                        {
                            Boolean isExecuteDataAdapter = false;
                            foreach (var item in pDynamicArgsGUIAndDefault)
                            {
                                ReferentialsReferentialStringDynamicData sdd = item.First;
                                if (pIsModeInit)
                                {
                                    if (dt.Select(StrFunc.AppendFormat("PARAMNAME = '{0}' ", sdd.name)).Count() == 0)
                                    {
                                        isExecuteDataAdapter = true;
                                        if (false == item.First.datatypeSpecified)
                                            throw new NotSupportedException("datatype is expected");

                                        DataRow row = dt.NewRow();
                                        row["IDLSTCONSULT"] = IDLSTCONSULT;
                                        row["IDLSTTEMPLATE"] = IDLSTTEMPLATE;
                                        row["IDA"] = IDA;
                                        row["PARAMNAME"] = sdd.name;
                                        row["DATATYPE"] = sdd.datatype;
                                        row["PARAMVALUE"] = GetValueForLSTPARAM(pCS, item);
                                        dt.Rows.Add(row);
                                    }
                                }
                                else // false == pIsModeInit
                                {
                                    isExecuteDataAdapter = true;
                                    DataRow[] rows = dt.Select(StrFunc.AppendFormat("PARAMNAME = '{0}' ", sdd.name));
                                    if (ArrFunc.IsEmpty(rows))
                                        throw new InvalidProgramException($"row {sdd.name} not found in LSTPARAM");

                                    DataRow row = rows[0];
                                    row["PARAMVALUE"] = GetValueForLSTPARAM(pCS, item);
                                }
                            }

                            if (isExecuteDataAdapter)
                            {
                                DataHelper.ExecuteDataAdapter(pCS, queryFrom, dt);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Mise à jour de l'enregistrement de LSTPARAM associé à {pDynamicData}
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pDynamicArgGUIAndDefault"></param>
            /// FI 20200602 [25370] Add Method
            public void UpdateLstParam(string pCS, Pair<ReferentialsReferentialStringDynamicData, string> pDynamicArgGUIAndDefault)
            {
                StrBuilder SQLUpdate = new StrBuilder();
                SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTPARAM + Cst.CrLf;
                SQLUpdate += "set PARAMVALUE=@PARAMVALUE" + Cst.CrLf;
                SQLUpdate += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out DataParameters dbParam);
                SQLUpdate += SQLCst.AND + "PARAMNAME=@PARAMNAME";

                dbParam.Add(new DataParameter(pCS, "PARAMNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pDynamicArgGUIAndDefault.First.name);
                dbParam.Add(new DataParameter(pCS, "PARAMVALUE", DbType.AnsiString, SQLCst.UT_NOTE_LEN), GetValueForLSTPARAM(pCS, pDynamicArgGUIAndDefault));

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }

            /// <summary>
            /// Retourne la valeur pour alimentation de LSTPARAM
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pDynamicArgsGUIAndDefault"></param>
            /// <returns></returns>
            /// FI 20200602 [25370] Add Method
            /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
            private static string GetValueForLSTPARAM(string pCS, Pair<ReferentialsReferentialStringDynamicData, string> pDynamicArgsGUIAndDefault)
            {
                if (null == pDynamicArgsGUIAndDefault)
                    throw new ArgumentNullException("pDynamicArgsGUIAndDefault argument is null");

                ReferentialsReferentialStringDynamicData dynamicData = pDynamicArgsGUIAndDefault.First;
                string defaultValue = pDynamicArgsGUIAndDefault.Second;
                
                string ret = dynamicData.GetDataValue(pCS, null); // GetDataValue peut retourner null

                /* S'il existe une valeur par défault et que la valeur du dynamicData est identique à ce default, Spheres alimente la table LSTPARAM avec le default 
                 Exemple de default => BUSINESS
                 */
                if (StrFunc.IsFilled(defaultValue))
                {
                    if (TypeData.IsTypeDateTimeOffset(dynamicData.datatype) || TypeData.IsTypeDate(dynamicData.datatype))
                    {
                        DtFuncML dtFuncML = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null);
                        string fmt = TypeData.IsTypeDateTimeOffset(dynamicData.datatype) ? DtFunc.FmtTZISOLongDateTime : DtFunc.FmtISODate;

                        string defaultValueResult = dtFuncML.GetDateTimeString(defaultValue, fmt);
                        if (ret == defaultValueResult)
                            ret = defaultValue;
                    }
                    else if (TypeData.IsTypeInt(dynamicData.datatype))
                    {
                        string defaultValueResult;
                        switch (defaultValue)
                        {
                            case "ENTTIY":
                                defaultValueResult = SessionTools.User.Entity_IdA.ToString();
                                break;
                            default:
                                defaultValueResult = defaultValue;
                                break;
                        }
                        if (IntFunc.IntValue(ret) == IntFunc.IntValue(defaultValueResult))
                            ret = defaultValue;
                    }
                    else if (TypeData.IsTypeDec(dynamicData.datatype))
                    {
                        if (DecFunc.DecValue(ret) == DecFunc.DecValue(defaultValue))
                            ret = defaultValue;
                    }
                }
                else if (TypeData.IsTypeInt(dynamicData.datatype) && (dynamicData.name == "ENTITY") && (ret == SessionTools.User.Entity_IdA.ToString()))
                {
                    // Cas particulier ou le mot clé ENTITY est utilisé même s'il n'existe de default
                    ret = "ENTITY";
                }
                return ret;
            }

            /// <summary>
            /// Mise à jour du champ LSTTEMPLATE.ISLOADONSTART 
            /// </summary>
            /// <param name="pCS"></param>
            /// <returns></returns>
            /// <param name="pCS"></param>
            /// <returns></returns>
            public Cst.ErrLevel SetIsLoadOnStart(string pCS)
            {
                Cst.ErrLevel ret;

                StrBuilder SQLUpdate = new StrBuilder();
                SQLUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLUpdate += @" Set ISLOADONSTART = @ISLOADONSTART";
                SQLUpdate += Cst.CrLf + SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, this.IDLSTCONSULT, this.IDLSTTEMPLATE, this.IDA, null, out DataParameters dbParam);

                dbParam.Add(new DataParameter(pCS, "ISLOADONSTART", DbType.Boolean), ISLOADONSTART);

                QueryParameters qryParameters = new QueryParameters(pCS, SQLUpdate.ToString(), dbParam);
                int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (nbRow == 1)
                    ret = Cst.ErrLevel.SUCCESS;
                else
                    ret = Cst.ErrLevel.ABORTED;

                return ret;
            }


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

                return rights.HasUserRight(pUser, pTypeRight);
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
                    string SQLDelete = SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                    SQLDelete += SQLCst.WHERE + "(IDLSTCONSULT=@IDLSTCONSULT) and (IDA=@IDA)";

                    //On delete l'eventuel ancien defaut
                    DataParameters dbParam = new DataParameters();
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), IDLSTCONSULT);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);

                    QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    //On insert le nouveau
                    string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + @"(IDLSTTEMPLATE,IDLSTCONSULT,IDA,DTUPD,IDAUPD,DTINS,IDAINS)";
                    SQLInsert += " values (@IDLSTTEMPLATE,@IDLSTCONSULT,@IDA,@DTUPD,@IDAUPD,@DTINS,@IDAINS)";
                    
                    // FI 20200820 [25468] Dates systèmes en UTC
                    DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);

                    dbParam = new DataParameters();
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), IDLSTCONSULT);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), IDA);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), dtSys);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), IDA);
                    dbParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), dtSys);
                    dbParam.Add(new DataParameter(pCS, "IDLSTTEMPLATE", DbType.AnsiString, 64));
                    if (ReferentialWeb.ExistsTemplate(pCS, pIdLstConsult, IDLSTTEMPLATE_WithoutPrefix, IDA))
                        dbParam["IDLSTTEMPLATE"].Value = IDLSTTEMPLATE_WithoutPrefix;
                    else
                        dbParam["IDLSTTEMPLATE"].Value = IDLSTTEMPLATE;

                    qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                    _ = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
                else
                {
                    //On delete au cas ou il serait defini par defaut
                    StrBuilder SQLDelete = new StrBuilder();
                    SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATEDEF.ToString() + Cst.CrLf;
                    //SQLDelete += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, null, out dbParam;
                    SQLDelete += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, null, out DataParameters dbParam);

                    QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }

            }
            #endregion

            #region Childs Functions
            #region CopyChilds
            /// <summary>
            /// Copie les tables 'enfants' (LSTSELECT, LSTORDERBY, LSTWHERE) d'un template sur un autre template
            /// </summary>
            /// <param name="pSourceIdLstTemplate">id du template Source</param>
            /// <param name="pSourceIdA">id du propriétaire du template Source</param>
            /// <param name="pTargetIdLstTemplate">id du template Destination</param>
            /// <param name="pTargetIdA">id du propriétaire du template Destination</param>
            public static void CopyChilds(string pCS, string pIdLstConsult, string pSourceIdLstTemplate, int pSourceIdA, string pTargetIdLstTemplate, int pTargetIdA)
            {
                Cst.OTCml_TBL[] childTableName = { Cst.OTCml_TBL.LSTSELECT, Cst.OTCml_TBL.LSTORDERBY, Cst.OTCml_TBL.LSTWHERE, Cst.OTCml_TBL.LSTPARAM };
                for (int i = 0; i < childTableName.Length; i++)
                {
                    ReferentialWeb.DeleteChild(pCS, childTableName[i], pIdLstConsult, pTargetIdLstTemplate, pTargetIdA, false);

                    string tableName = childTableName[i].ToString();

                    StrBuilder SQLInsert = new StrBuilder();
                    SQLInsert += SQLCst.INSERT_INTO_DBO + tableName + "(" + AllColsForTableChild(tableName, null, null, -1) + ")" + Cst.CrLf;
                    SQLInsert += "(";
                    SQLInsert += SQLCst.SELECT + AllColsForTableChild(tableName, "ls", pTargetIdLstTemplate, pTargetIdA) + Cst.CrLf;
                    SQLInsert += SQLCst.FROM_DBO + tableName + " ls" + Cst.CrLf;
                    SQLInsert += SQLCst.WHERE + ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pSourceIdLstTemplate, pSourceIdA, "ls", out DataParameters dbParam);
                    SQLInsert += ")";

                    QueryParameters qryParameters = new QueryParameters(pCS, SQLInsert.ToString(), dbParam);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
            }
            #endregion CopyChilds
            #endregion Childs Functions
            
            /// <summary>
            /// Fonctions pour obtenir la clause where pour identifier un template par rapport à sa clé (GetSQLClause_PK_LSTTEMPLATE)
            /// </summary>
            public string GetSQLClause_PK_LSTTEMPLATE2(string pCS, string pDBAlias, out DataParameters opDataParameters)
            {
                return ReferentialWeb.GetSQLClause_PK_LSTTEMPLATE2(pCS, IDLSTCONSULT, IDLSTTEMPLATE, IDA, pDBAlias, out opDataParameters);
            }
            
            
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
            
            
            #region private static AllColsForTableChild
            /// <summary>
            /// Récupération de la liste des colonnes pour une table sous la forme d'unne string utilisable dans un ordre SQL Select (ex: COL1,COL2,COL3,...)
            /// </summary>
            /// <param name="pTableName">Nom de la table: LSTSELECT|LSTWHERE|LSTORDERBY|LSTPARAM</param>
            /// <param name="pAliasTableName">Alias éventuel de la table</param>
            /// <param name="pIdLstTemplate">ID du template</param>
            /// <param name="pIdA">ID du propriétaire du template</param>
            /// <returns>Liste des colonnes</returns>
            /// FI 20200602 [25370] Refactoring pour LSTPRAM 
            private static string AllColsForTableChild(string pTableName, string pAliasTableName, string pIdLstTemplate, int pIdA)
            {
                string alias = (StrFunc.IsFilled(pAliasTableName) ? pAliasTableName + "." : string.Empty);

                string selectQuery = alias + "IDLSTCONSULT,";
                selectQuery += (pIdLstTemplate != null ? DataHelper.SQLString(pIdLstTemplate) : alias + "IDLSTTEMPLATE") + ", ";
                selectQuery += (pIdA >= 0 ? pIdA.ToString() : alias + "IDA") + ", ";

                if ((pTableName == Cst.OTCml_TBL.LSTSELECT.ToString()) || (pTableName == Cst.OTCml_TBL.LSTORDERBY.ToString()) || (pTableName == Cst.OTCml_TBL.LSTWHERE.ToString()))
                {
                    selectQuery += alias + "IDLSTCOLUMN,";
                    selectQuery += alias + "TABLENAME,";
                    selectQuery += alias + "COLUMNNAME,";
                    selectQuery += alias + "ALIAS,";
                    selectQuery += alias + "POSITION";
                }
                else if ((pTableName == Cst.OTCml_TBL.LSTPARAM.ToString()))
                {
                    selectQuery += alias + "PARAMNAME,";
                    selectQuery += alias + "DATATYPE,";
                    selectQuery += alias + "PARAMVALUE";
                }

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
        #endregion class LSTTEMPLATE
    }
    #endregion class LstConsult

    #region class InfosLstWhere
    /// <summary>
    /// Classe qui représente un critère de consultation
    /// </summary>
    public class InfosLstWhere
    {
        #region members
        #region columnIdentifier
        private string _columnIdentifier;
        /// <summary>
        /// Obtient ou définit l'identifiant de la colonne associé au critère 
        /// </summary>
        public string ColumnIdentifier
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
        public string Operator
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
        public string LstValue
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
        public bool IsMandatory
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
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
        #endregion
        //
        #region constructor
        public InfosLstWhere() { }
        #endregion constructor
        //
        #region Method
        public string GetDisplayOperator()
        {
            //string ret = Ressource.GetString(@operator, @operator);
            string ret = Ressource.GetString(Operator.ToLower(), Operator);
            return ret;
        }
        #endregion

    }
    #endregion
}
