using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.Fix;
//
using FixML.Enum;
//
using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// La hiérarchie des acteurs avec comme racines les acteur avec le rôle CBO, vis-à-vis:
    /// <para>- soit d'eux-même</para>
    /// <para>- soit de l'Entity</para>
    /// <para>- soit de tous (NULL)</para>
    /// avec tous les enfants quelque soit le rôle
    /// </summary>
    /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
    [XmlRoot(ElementName = "entityCBOHierarchy")]
    public class CBHierarchy
    {
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public static RoleActor[] RolesCboMro = 
        {
            RoleActor.CSHBALANCEOFFICE, 
            RoleActor.MARGINREQOFFICE
        };
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public static RoleActor[] RolesClearer = 
        {
            RoleActor.CLEARER,
            RoleActor.CCLEARINGCOMPART,
            RoleActor.HCLEARINGCOMPART,
            RoleActor.MCLEARINGCOMPART,
        };

        /// <summary>
        /// IDA de l'Entity
        /// </summary>
        [XmlAttribute(AttributeName = "ida_Entity")]
        public int Ida_Entity;

        /// <summary>
        /// Identifier de l'Entity
        /// </summary>
        [XmlAttribute(AttributeName = "identifier_Entity")]
        public string Identifier_Entity;

        /// <summary>
        /// Date de compensation
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusiness")]
        public DateTime DtBusiness;

        /// <summary>
        /// Date de compensation Précédente
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusinessPrev")]
        public DateTime DtBusinessPrev;

        /// <summary>
        ///  Liste des chambres de compensation Pour lesquelles Spheres® récupère le désosit veille 
        ///  <para>Chambre pour laquelle la date de traitement est férié ou</para>
        ///  <para>Chambre pour laquelle le traitement EOD est non exécuté ou en erreur</para>
        /// </summary>
        public List<Pair<int, DateTime>> CssUsingPreviousDeposit = new List<Pair<int, DateTime>>();

        /// <summary>
        ///  Liste des CSS/Custodian pour lesquels le dernier traitement EOD est en succès\warning
        /// </summary>
        /// FI 20161021 [22152]
        public List<SpheresIdentification> cssCustodianEODValid = new List<SpheresIdentification>();
        /// <summary>
        ///  Liste des CSS/Custodian pour lesquels le dernier traitement EOD est en erreur ou non exécuté
        /// </summary>
        /// FI 20161021 [22152]
        public List<SpheresIdentification> cssCustodianEODUnValid = new List<SpheresIdentification>();


        /// <summary>
        /// Timing
        /// </summary>
        [XmlAttribute(AttributeName = "timing")]
        public SettlSessIDEnum Timing;

        List<CBActorNode> m_ChildActors = new List<CBActorNode>();
        /// <summary>
        /// La liste des acteurs de niveau 0 (racine)
        /// </summary>        
        [XmlArray(ElementName = "childActors")]
        [XmlArrayItemAttribute("actor")]
        public List<CBActorNode> ChildActors
        {
            get { return m_ChildActors; }
            set { m_ChildActors = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20200603 [XXXXX] 
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        SetErrorWarning m_SetErrorWarning = null;

        /// <summary>
        /// Table de travail qui contient la hierarchie depuis ENTITY (Résultat de récursivité)
        /// </summary>
        // EG 20150724 [21187] New
        // EG 20181119 PERF Correction post RC (Step 2)
        public Pair<string,bool> TblCBENTITY_Work { set; get; }

        /// <summary>
        /// Table de travail qui contient les acteurs CBO
        /// </summary>
        // EG 20140226 [19575][19666]
        // EG 20181119 PERF Correction post RC (Step 2)
        public Pair<string, bool> TblCBACTORCBO_Work { set; get; }

        /// <summary>
        /// Table de travail qui contient les acteurs
        /// </summary>
        // EG 20140226 [19575][19666]
        // EG 20181119 PERF Correction post RC (Step 2)
        public Pair<string, bool> TblCBACTOR_Work { set; get; }
        /// <summary>
        /// Table de travail qui contient les acteurs ne diposant pas du rôle ni "CLEARER" ni "*COMPART"
        /// </summary>
        // PL 20160524 
        // EG 20181119 PERF Correction post RC (Step 2)
        public Pair<string, bool> TblCBACTOR_NOTCLEARER_Work { set; get; }
        /// <summary>
        /// Table de travail qui contient les acteurs diposant du rôle "CLEARER" ou "*COMPART"
        /// </summary>
        // PL 20160524 
        // EG 20181119 PERF Correction post RC (Step 2)
        public Pair<string, bool> TblCBACTOR_CLEARER_Work { set; get; }

        #region QueryTradeBusiness
        // EG 20180906 PERF New (Use With instruction and Temporary based to model table)
        // EG 20181010 PERF Refactoring
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200629 [25361] PERF : Add 'FX' GProduct
        private string QueryTradeBusiness()
        {
            return @"select tr.IDT, tr.IDB_DEALER as IDB1, tr.IDB_CLEARER as IDB2
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and (ns.FUNGIBILITYMODE != 'NONE')
            where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')

            union

            select tr.IDT, ti.IDB_BUYER as IDB1, ti.IDB_SELLER as IDB2
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and (ns.FUNGIBILITYMODE = 'NONE')
            inner join dbo.VW_ALLTRADEINSTRUMENT ti on (ti.IDT = tr.IDT)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT in ('FX','OTC','SEC','FUT','COM'))
            where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR')";           
        }
        #endregion QueryTradeBusiness
        #region QueryBookTradeCandidate
        // EG 20181010 PERF New
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200629 [25361] PERF : Use union All
        private string QueryBookTradeCandidate()
        {
            return String.Format(@"select bk.IDB, count(*)
            from (
                select bk.IDB
                from dbo.{0} tr
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB1)
                where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >= @DTBUSINESS)
                union all
                select bk.IDB
                from dbo.{0} tr
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB2)
                where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >= @DTBUSINESS)
            ) bk
            group by bk.IDB", TblCBTRADE_BUSINESS_Work);
        }
        #endregion QueryBookTradeCandidate

        // EG 20181010 PERF New
        public string TblCBTRADE_BUSINESS_Work { set; get; }
        public string TblCBBOOK_Work { set; get; }

        /// <summary>
        /// Connection utilisée pour alimenter les tables temporaires
        /// </summary>
        /// EG 20140226 [19575][19666]
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public IDbConnection DbConnection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CBHierarchy() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda_Entity"></param>
        /// <param name="pIdentifier_Entity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtBusinessPrev"></param>
        /// <param name="pTiming"></param>
        public CBHierarchy(int pIda_Entity, string pIdentifier_Entity, DateTime pDtBusiness,
            DateTime pDtBusinessPrev, SettlSessIDEnum pTiming)
        {
            Ida_Entity = pIda_Entity;
            Identifier_Entity = pIdentifier_Entity;
            DtBusiness = pDtBusiness;
            DtBusinessPrev = pDtBusinessPrev;
            Timing = pTiming;
        }

        /// <summary>
        /// Create Working tables
        /// <param name="pCS"></param>
        /// <param name="pTableId">Suffixe des tables temporaires</param>
        /// </summary>
        /// EG 20140228 19575
        /// EG 20150724 [21187] New tblCBENTITY_Work
        // EG 20181010 PERF Refactoring
        private void CreateCBWorkTable(string pCS, string pTableId)
        {
            TblCBENTITY_Work = SetTableActor(pCS, "ENTITY", null, pTableId);
            TblCBACTORCBO_Work = SetTableActor(pCS, "ACTORCBO", null, pTableId);
            TblCBACTOR_Work = SetTableActor(pCS, "ACTOR", null, pTableId);
            TblCBACTOR_NOTCLEARER_Work = SetTableActor(pCS, "ACTOR", "NC", pTableId);
            TblCBACTOR_CLEARER_Work = SetTableActor(pCS, "ACTOR", "C", pTableId);
            
            // RD 20200629 [25361] PERF : Use work tables also for Oracle
            AppInstance.TraceManager.TraceVerbose(this, "Initializing Trade work Table");
            TblCBTRADE_BUSINESS_Work = SetTableTradeBook(pCS, "TRBOOK", pTableId, QueryTradeBusiness(), DtBusiness);
            
            AppInstance.TraceManager.TraceVerbose(this, "Initializing Book work Table");
            TblCBBOOK_Work = SetTableTradeBook(pCS, "BOOK", pTableId, QueryBookTradeCandidate(), DtBusiness);
        }

        #region SetTableActor
        // EG 20181010 PERF New
        // EG 20181119 PERF Correction post RC (Step 2)
        private static Pair<string, bool> SetTableActor(string pCS, string pPrefixTableModel, string pPrefixTableWork, string pTableId)
        {
            Pair<string, bool> tableWork = new Pair<string, bool>();
            if (StrFunc.IsFilled(pPrefixTableWork))
                tableWork.First = String.Format("CB{0}_{1}_{2}_W", pPrefixTableModel, pPrefixTableWork, pTableId);
            else
                tableWork.First = String.Format("CB{0}_{1}_W", pPrefixTableModel, pTableId);

            if (tableWork.First.Length > 32)
                throw new Exception(StrFunc.AppendFormat("Table Name :{0} is too long", tableWork.First));


            string tableModel = String.Format("CB{0}_MODEL", pPrefixTableModel);

            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            tableWork.Second = DataHelper.IsExistTable(pCS, tableWork.First);
            if (tableWork.Second)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = String.Format(@"truncate table dbo.{0};", tableWork.First);
                else if (DbSvrType.dbORA == serverType)
                    command = String.Format(@"delete from dbo.{0};", tableWork.First);
                else
                    throw new NotImplementedException("RDBMS not implemented");
                QueryParameters qryParameters = new QueryParameters(pCS, command, null);

                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", String.Format("Delete table {0}", tableWork.First), qryParameters.GetQueryReplaceParameters()));
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
            }
            else
            {
                DataHelper.CreateTableAsSelect(pCS, tableModel, tableWork.First, out string command);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create table", command));
            }
            return tableWork;
        }
        #endregion SetTableActor
        #region SetTableTradeBook
        // EG 20180906 PERF New (Use With instruction and Temporary based to model table)
        // EG 20181010 PERF Refactoring
        // EG 20181119 PERF Correction post RC (Step 2)
        private static string SetTableTradeBook(string pCS, string pPrefixTable, string pTableId, string pSelect, DateTime pDtBusiness)
        {
            string tableName = String.Format("CB{0}_{1}_W", pPrefixTable, pTableId);
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            bool isExistTable = DataHelper.IsExistTable(pCS, tableName);
            string sqlQuery;
            if (isExistTable)
            {
                if (DbSvrType.dbSQL == serverType)
                    sqlQuery = String.Format(@"truncate table dbo.{0}; insert into dbo.{0} {1}", tableName, pSelect);
                else if (DbSvrType.dbORA == serverType)
                    sqlQuery = String.Format(@"delete from dbo.{0}; insert into dbo.{0} {1};", tableName, pSelect);
                else
                    throw new NotImplementedException("RDBMS not implemented");
            }
            else
            {
                DataHelper.CreateTableAsSelect(pCS, String.Format("CB{0}_MODEL", pPrefixTable), tableName, out string command);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create table", command));

                sqlQuery = String.Format(@"insert into dbo.{0} {1}", tableName, pSelect);
            }

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness); // FI 20201006 [XXXXX] DbType.Date
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", String.Format("insert into {0}", tableName), qryParameters.GetQueryReplaceParameters()));
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == isExistTable)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = String.Format("create clustered index IX_{0} on dbo.{0} ({1})", tableName, (pPrefixTable == "TRBOOK") ? "IDT" : "IDB");
                else if (DbSvrType.dbORA == serverType)
                    command = String.Format("create index IX_{0} on dbo.{0} ({1})", tableName, (pPrefixTable == "TRBOOK") ? "IDT" : "IDB");
                else
                    throw new NotImplementedException("RDBMS not implemented");

                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", command));
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, command);
            }

            // RD 20200629 [25361] PERF : Add
            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, string.Format("update statistic on {0}", tableName));
                DataHelper.UpdateStatTable(pCS, tableName);
            }

            return tableName;
        }
        #endregion SetTableTradeBook

        /// <summary>
        /// Fin de la connexion et suppression des tables temporaires 
        /// </summary>
        /// 20181010 EG No drop CBWork (executed by ProcessBase at the end)
        public void Dispose()
        {
            if (null != DbConnection)
            {
                //DropCBWorkTable(pCS);

                DbConnection.Close();
                DbConnection.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        private void OpenConnection(string pCS)
        {
            DbConnection = DataHelper.OpenConnection(pCS);
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void InitDelegate(SetErrorWarning pSetErrorWarning)
        {
            this.m_SetErrorWarning = pSetErrorWarning;

        }

        /// <summary>
        /// Construire la hiérarchie des acteurs d'une manière brute:
        /// <para>En commençant par tous les acteurs qui ont le rôle CBO vis-à-vis:</para>
        /// <para>- de tous (NULL)</para>
        /// <para>- d'eux même</para> 
        /// <para>- de l'Entity en cours</para>
        /// <para>- d'un Acteur déscendant de l'Entity</para>
        /// <para>En suite, tous les acteurs enfants avec tous les rôles</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableId">Suffixe des tables temporaires</param>
        /// <returns></returns>
        /// RD 20130502 [] Utilisation des tables de travail CBACTOR et CBACTORCBO
        /// EG 20140228 [19575][19666] REFACTORING
        // EG 20180525 [23979] IRQ Processing
        public Cst.ErrLevel BuildBrutHierarchy(string pCS, ProcessBase pProcessBase)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            string tableId = pProcessBase.Session.BuildTableId();

            AppInstance.TraceManager.TraceInformation(this, "Start BuildBrutHierarchy");

            OpenConnection(pCS);

            if (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn))
            {
                AppInstance.TraceManager.TraceInformation(this, "CreateCBWorkTable");
                // EG 20140226 [19575][19666]
                CreateCBWorkTable(pCS, tableId);

                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                {
                    AppInstance.TraceManager.TraceInformation(this, "LoadCBO");
                    // 1- Charger tous les Acteurs CBO
                    codeReturn = this.LoadCBO(pCS);

                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        if (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn))
                        {
                            AppInstance.TraceManager.TraceInformation(this, "LoadCBODescendant");
                            // 2- Charger tous les Descendants des Acteurs CBO
                            this.LoadCBODescendant(pCS);
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                            (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            AppInstance.TraceManager.TraceInformation(this, "LoadRoles");
                            // 3- Charger tous les Rôles de tous les Acteurs de la structure:
                            //  -->  sauf le Rôle CSHBALANCEOFFICE qui est déjà chargé
                            this.LoadRoles(pCS);
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                            (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            AppInstance.TraceManager.TraceInformation(this, "LoadBusinessAttribute");
                            // 4- Charger les caracteristiques des Acteurs CBO et MRO
                            codeReturn = this.LoadBusinessAttribute(pCS);
                        }

                        if ((codeReturn == Cst.ErrLevel.SUCCESS) &&
                            (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            AppInstance.TraceManager.TraceInformation(this, "CheckCboMroReferential");
                            // 5- Vérification des caractéristiques des Acteurs CBO et MRO
                            codeReturn = CheckCboMroReferential(this.ChildActors);
                        }
                    }
                }
            }
            AppInstance.TraceManager.TraceInformation(this, "End BuildBrutHierarchy");
            return codeReturn;
        }

        /// <summary>
        /// Charger tous les acteurs qui ont le rôle CBO vis-à-vis:
        /// <para>- de tous (NULL)</para>
        /// <para>- d'eux même</para> 
        /// <para>- de l'Entity en cours</para>
        /// <para>- d'un Acteur déscendant de l'Entity</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// RD 20130502 [] Utilisation des tables CBACTORCBO et BOOKACTOR_R
        /// EG 20140228 [19575][19666] REFACTORING
        /// PM 20140903 [20066][20185] Gestion méthode UK (ETD & CFD)
        /// FI 20141210 [20559] Modify
        /// EG 20150724 [21187] Refactoring
        /// FI 20161021 [22152] Modify
        /// FI 20170208 [21916] Modify
        /// FI 20170208 [22151][22152] Modify
        /// PM 20180126 [CHEETAH] Refactoring
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel LoadCBO(string pCS)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // FI 20161021 [22152] call LoadEntityHierarchy
            // Construction de la query récursive et insertion dans la table temporaire : tblCBENTITY_Work
            LoadEntityHierarchy(pCS);

            #region STEP 1 : Charger les acteurs dans la table de travail CBACTORCBO_Work
            // PM 20180126 [CHEETAH] Requête totalement différente entre Oracle et SQLServer pour cause de performance
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                // PM 20180126 [CHEETAH] Refactoring pour performance
                //sqlQuery = @"select distinct ar.IDA, ar.IDA_ACTOR into " + tblCBACTORCBO_Work + Cst.CrLf;
                AppInstance.TraceManager.TraceVerbose(this, "InsertCBOSQLServer...");
                InsertCBOSQLServer(pCS);
            }
            else if (DbSvrType.dbORA == serverType)
            {
                // PM 20180126 [CHEETAH] Refactoring pour performance
                //sqlQuery = @"insert into " + tblCBACTORCBO_Work + @"(IDA, IDA_ACTOR) select distinct ar.IDA, ar.IDA_ACTOR" + Cst.CrLf;
                AppInstance.TraceManager.TraceVerbose(this, "InsertCBOOracle...");
                InsertCBOOracle(pCS);
            }
            else
            {
                throw new NotImplementedException("RDBMS not implemented");
            }
            #endregion STEP 1 : Charger les acteurs dans la table de travail CBACTORCBO_Work

            #region STEP 2 : Lectures des acteurs à partir de la table de travail CBACTORCBO_Work

            string sqlQuery = @"select a_cbo.IDA, a_cbo.IDA_ACTOR, 'CSHBALANCEOFFICE' as IDROLEACTOR, ac.IDENTIFIER, 0 as LEVEL_ACTOR
            from dbo.{0} a_cbo
            inner join dbo.ACTOR ac on (ac.IDA = a_cbo.IDA)" + Cst.CrLf;

            // Exécution
            sqlQuery = String.Format(sqlQuery, TblCBACTORCBO_Work.First);
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, null);
            DataSet dsCBO = DataHelper.ExecuteDataset(DbConnection, CommandType.Text, qryParameters.Query);

            DataTable dtCBO = dsCBO.Tables[0];
            DataRow[] drCBO = dtCBO.Select();

            if (drCBO.Length == 0)
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), this.Ida_Entity);

                sqlQuery = @"select 1
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA) and {1}
                where ({2}) and (ar.IDROLEACTOR = 'CSHBALANCEOFFICE') and ((isnull(ar.IDA_ACTOR,ar.IDA) = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))
                
                union
    
                select 1
                from dbo.ACTORROLE ar
                inner join {0} eh on (eh.IDA = ar.IDA_ACTOR)
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA) and {1}
                where ({2}) and (ar.IDROLEACTOR = 'CSHBALANCEOFFICE') and ((isnull(ar.IDA_ACTOR,ar.IDA) = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))" + Cst.CrLf;

                // Exécution
                sqlQuery = String.Format(sqlQuery, TblCBENTITY_Work.First, OTCmlHelper.GetSQLDataDtEnabled(pCS, "ac"), OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar"));
                qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (obj == null)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4000), 2,
                        new LogParam(Identifier_Entity),
                        new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                    codeReturn = Cst.ErrLevel.DATANOTFOUND;
                }
                else
                {
                    // Aucune activité constatée sur au moins un acteur CBO
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4004), 2,
                        new LogParam(Identifier_Entity),
                        new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                    codeReturn = Cst.ErrLevel.NOTHINGTODO;
                }
            }
            else
            {
                this.Add(drCBO);
            }
            #endregion STEP 2 : Lectures des acteurs à partir de la table de travail CBACTORCBO_Work
            return codeReturn;
        }

        /// <summary>
        /// Insérer les acteurs dans la table de travail CBACTORCBO_Work (pour SQLServer)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// PM 20180126 [CHEETAH] New for refactoring (code déplacé à partir de la méthode LoadCBO)
        // EG 20180906 PERF Upd (Use With instruction and Temporary based to model table)
        // EG 20181010 PERF Refacoring
        // EG 20181119 PERF Correction post RC (Step 2)
        // PM 20190909 [24826][24915] Ajout CashBalance Jour pour retraitement
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200629 [25361] PERF : Uniformiser par rapport à InsertCBOOracle
        private void InsertCBOSQLServer(string pCS)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), Ida_Entity);
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness); // FI 20201006 [XXXXX] DbType.Date
                dataParameters.Add(new DataParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS + "_PREV", DbType.Date), DtBusinessPrev); // FI 20201006 [XXXXX] DbType.Date

                string dtEnabledAc = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ac", DtBusiness);
                string dtEnabledAr = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar", DtBusiness);

                string sqlQuery = @"insert into dbo.{0}
select distinct ar.IDA, ar.IDA_ACTOR
from dbo.ACTORROLE ar
inner join dbo.ACTOR ac on (ac.IDA=ar.IDA) and {2}
inner join dbo.BOOKACTOR_R bar on (bar.IDA_ACTOR = ar.IDA)
{3} and ((isnull(ar.IDA_ACTOR,ar.IDA) = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))

union

select distinct ar.IDA, ar.IDA_ACTOR  
from dbo.ACTORROLE ar   
inner join dbo.{1} eh on (eh.IDA = ar.IDA_ACTOR)
inner join dbo.ACTOR ac on (ac.IDA=ar.IDA) and {2}
inner join dbo.BOOKACTOR_R bar on (bar.IDA_ACTOR = ar.IDA)   
{3}" + Cst.CrLf;
                
                string sqlSubQuery = String.Format(@"inner join 
(
    /* Cash-Flows du jour sur négociations fongibles et non fongibles */

	select TOT, IDB 
	from dbo.{0}

    union 

    /* Deposit du jour */

	select count(*) as TOT, tr_mr.IDB_RISK as IDB           
	from dbo.TRADE tr_mr           
	inner join dbo.INSTRUMENT ns on (ns.IDI = tr_mr.IDI)           
	inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'marginRequirement')           
	where (tr_mr.DTBUSINESS = @DTBUSINESS)   
	group by tr_mr.IDB_RISK

    union 

    /* CashPayment du jour */

	select count(*) as TOT, bk.IDB           
	from dbo.TRADE tr_cp           
	inner join dbo.INSTRUMENT ns on (ns.IDI = tr_cp.IDI)              
	inner join dbo.BOOK bk on (bk.IDB = tr_cp.IDB_SELLER) or (bk.IDB = tr_cp.IDB_BUYER)                  
	inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'cashPayment')           
	where (tr_cp.DTBUSINESS = @DTBUSINESS)   
	group by bk.IDB

    union 

    /* Collateral valide du jour */

	select count(*) as TOT, bk.IDB           
	from dbo.POSCOLLATERAL pcol              
	inner join dbo.BOOK bk on (bk.IDB = pcol.IDB_PAY) or (bk.IDB = pcol.IDB_REC)                  
	where (pcol.IDSTACTIVATION = 'REGULAR') and (pcol.DTBUSINESS <= @DTBUSINESS) and 
	(case pcol.DURATION when 'Overnight' then pcol.DTBUSINESS when 'Term' then pcol.DTTERMINATION else @DTBUSINESS end >= @DTBUSINESS)   
	group by bk.IDB

    union 

    /* CashBalance veille et jour */

	select count(*) as TOT, tr_cb.IDB_RISK as IDB
	from dbo.TRADE tr_cb
	inner join dbo.INSTRUMENT ns on (ns.IDI = tr_cb.IDI)           
	inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'cashBalance')           
	where (tr_cb.DTBUSINESS = @DTBUSINESS_PREV)
       or (tr_cb.DTBUSINESS = @DTBUSINESS)
	group by tr_cb.IDB_RISK 

) pos on (pos.IDB = bar.IDB)

where ({1}) and (ar.IDROLEACTOR = 'CSHBALANCEOFFICE')", TblCBBOOK_Work, dtEnabledAr);                

                sqlQuery = String.Format(sqlQuery,
                        TblCBACTORCBO_Work.First, TblCBENTITY_Work.First,
                        dtEnabledAc,
                        sqlSubQuery);
                
                
                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", String.Format("insert into {0}",TblCBACTORCBO_Work.First) , qryParameters.GetQueryReplaceParameters()));
                
                DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());
            }
        }

        /// <summary>
        /// Insérer les acteurs dans la table de travail CBACTORCBO_Work (pour Oracle)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// PM 20180126 [CHEETAH] New for refactoring
        // EG 20181010 PERF Refactoring
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200629 [25361] PERF : Use tblCBBOOK_Work
        private void InsertCBOOracle(string pCS)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), Ida_Entity);
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);
                dataParameters.Add(new DataParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS + "_PREV", DbType.Date), DtBusinessPrev);

                string dtEnabledAc = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ac", DtBusiness);
                string dtEnabledAr = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar", DtBusiness);

                string sqlQuery = @"insert into dbo.{0} (IDA, IDA_ACTOR)
with bar as 
(
    select bar.IDA_ACTOR
    from dbo.BOOKACTOR_R bar
    {4}
)
select ar.IDA, ar.IDA_ACTOR
from dbo.ACTORROLE ar
inner join dbo.ACTOR ac on (ac.IDA = ar.IDA)
inner join bar on (bar.IDA_ACTOR = ar.IDA)
where (ar.IDROLEACTOR = 'CSHBALANCEOFFICE') and ((ar.IDA_ACTOR is null or ar.IDA_ACTOR = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))
and ({2}) and ({3})

union

select ar.IDA, ar.IDA_ACTOR
from dbo.ACTORROLE ar
inner join dbo.ACTOR ac on (ac.IDA=ar.IDA)
inner join bar on (bar.IDA_ACTOR = ar.IDA)
inner join dbo.{1} eh on (eh.IDA = ar.IDA_ACTOR)
where (ar.IDROLEACTOR = 'CSHBALANCEOFFICE')
and ({2}) and ({3})";

                string sqlSubQuery = String.Format(@"inner join 
    (
        /* Cash-Flows du jour sur négociations fongibles ET NON fongibles */

        select bk.IDB
        from dbo.{0} bk

        union all

        /* Deposit du jour ET CashBalance veille */

                        select tr.IDB_RISK as IDB
                        from dbo.TRADE tr
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        where (tr.DTBUSINESS = @DTBUSINESS) and (pr.IDENTIFIER = 'marginRequirement')
                        group by tr.IDB_RISK

        union all

                        select tr.IDB_RISK as IDB
                        from dbo.TRADE tr
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        where (tr.DTBUSINESS = @DTBUSINESS_PREV) and (pr.IDENTIFIER = 'cashBalance')
                        group by tr.IDB_RISK

        union all

        /* CashPayment du jour */

                        select bk.IDB
                        from dbo.TRADE tr_cp
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr_cp.IDI)
                        inner join dbo.BOOK bk on (bk.IDB = tr_cp.IDB_BUYER) or (bk.IDB = tr_cp.IDB_SELLER) 
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'cashPayment')
                        where (tr_cp.DTBUSINESS = @DTBUSINESS)
                        group by bk.IDB

        union all

        /* Collateral valide du jour */

        select bk.IDB
        from dbo.POSCOLLATERAL pcol
        inner join dbo.BOOK bk on (bk.IDB = pcol.IDB_PAY) or (bk.IDB = pcol.IDB_REC)
        where (pcol.IDSTACTIVATION = 'REGULAR') and (pcol.DTBUSINESS <= @DTBUSINESS) and 
        (case pcol.DURATION when 'Overnight' then pcol.DTBUSINESS when 'Term' then pcol.DTTERMINATION else @DTBUSINESS end >= @DTBUSINESS)
        group by bk.IDB
    ) pos on (pos.IDB = bar.IDB)", TblCBBOOK_Work);
                
                sqlQuery = String.Format(sqlQuery,
                    TblCBACTORCBO_Work.First, TblCBENTITY_Work.First,
                    dtEnabledAc, dtEnabledAr,
                    sqlSubQuery);

                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", String.Format("insert into {0}", TblCBACTORCBO_Work.First), qryParameters.GetQueryReplaceParameters()));
                DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (false == TblCBACTORCBO_Work.Second)
                {
                    string command;
                    if (DbSvrType.dbSQL == serverType)
                        command = $"create clustered index IX_{TblCBACTORCBO_Work.First} on dbo.{TblCBACTORCBO_Work.First} (IDA)";
                    else if (DbSvrType.dbORA == serverType)
                        command = $"create index IX_{TblCBACTORCBO_Work.First} on dbo.{TblCBACTORCBO_Work.First} (IDA)";
                    else
                        throw new NotImplementedException("RDBMS not implemented");

                    qryParameters = new QueryParameters(pCS, command, null);
                    AppInstance.TraceManager.TraceVerbose(null, $"create index IX ON ida - SQL:{TblCBACTORCBO_Work.First}");
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
                }

                if (DbSvrType.dbORA == serverType)
                {
                    AppInstance.TraceManager.TraceVerbose(null, $"update statistic on {TblCBACTORCBO_Work.First}");
                    DataHelper.UpdateStatTable(pCS, TblCBACTORCBO_Work.First);
                }
            }
        }
        // EG 20181119 UNUSED for the moment NO DELETE
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private void InsertCBOOracle_V12(string pCS)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), Ida_Entity);
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);  // FI 20201006 [XXXXX] DbType.Date
                dataParameters.Add(new DataParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS + "_PREV", DbType.Date), DtBusinessPrev); // FI 20201006 [XXXXX] DbType.Date

                #region Query

                string dtEnabledAc = @"(ac.DTENABLED <= @DTBUSINESS) and ((ac.DTDISABLED is null) or (@DTBUSINESS < ac.DTDISABLED))";
                string dtEnabledAr = @"(ar.DTENABLED <= @DTBUSINESS) and ((ar.DTDISABLED is null) or (@DTBUSINESS < ar.DTDISABLED))";

                string sqlQuery = @"insert into dbo.{0} (IDA, IDA_ACTOR)
                with bar as (
                    select bar.IDA_ACTOR
                    from dbo.BOOKACTOR_R bar
                    inner join 
                    (
                        /* Cash-Flows du jour sur négociations fongibles ET NON fongibles */

                        select bk.IDB
                        from dbo.TRADE tr
                        inner join dbo.EVENTCLASS ec on (ec.IDT = tr.IDT) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >= @DTBUSINESS)
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER) or (bk.IDB = tr.IDB_CLEARER)
                        where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
                        (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (ns.FUNGIBILITYMODE != 'NONE') 
                        group by bk.IDB

                        union all

                        select bk.IDB
                        from dbo.TRADE tr
                        inner join dbo.EVENTCLASS ec on (ec.IDT = tr.IDT) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >= @DTBUSINESS)
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
                        where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
                        (tr.IDSTACTIVATION = 'REGULAR') and (ns.FUNGIBILITYMODE  = 'NONE') and (pr.GPRODUCT in ('OTC','SEC','FUT','COM'))
                        group by bk.IDB

                        union all

                        /* Deposit du jour ET CashBalance veille */

                        select tr.IDB_RISK as IDB
                        from dbo.TRADE tr
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        where (tr.DTBUSINESS = @DTBUSINESS) and (pr.IDENTIFIER = 'marginRequirement')
                        group by tr.IDB_RISK

                        union all

                        select tr.IDB_RISK as IDB
                        from dbo.TRADE tr
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                        where (tr.DTBUSINESS = @DTBUSINESS_PREV) and (pr.IDENTIFIER = 'cashBalance')
                        group by tr.IDB_RISK

                        union all

                        /* CashPayment du jour */

                        select bk.IDB
                        from dbo.TRADE tr_cp
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr_cp.IDI)
                        inner join dbo.BOOK bk on (bk.IDB = tr_cp.IDB_BUYER) or (bk.IDB = tr_cp.IDB_SELLER) 
                        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'cashPayment')
                        where (tr_cp.DTBUSINESS = @DTBUSINESS)
                        group by bk.IDB

                        union all

                        /* Collateral valide du jour */

                        select bk.IDB
                        from dbo.POSCOLLATERAL pcol
                        inner join dbo.BOOK bk on (bk.IDB = pcol.IDB_PAY) or (bk.IDB = pcol.IDB_REC)
                        where (pcol.IDSTACTIVATION = 'REGULAR') and (pcol.DTBUSINESS <= @DTBUSINESS) and 
                        (case pcol.DURATION when 'Overnight' then pcol.DTBUSINESS when 'Term' then pcol.DTTERMINATION else @DTBUSINESS end >= @DTBUSINESS)
                        group by bk.IDB
                    ) pos on (pos.IDB = bar.IDB)
                )

                select ar.IDA, ar.IDA_ACTOR
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA)
                inner join bar on (bar.IDA_ACTOR = ar.IDA)
                where (ar.IDROLEACTOR = 'CSHBALANCEOFFICE') and ((ar.IDA_ACTOR is null or ar.IDA_ACTOR = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))
                and ({2}) and ({3})

                union

                select ar.IDA, ar.IDA_ACTOR
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA=ar.IDA)
                inner join bar on (bar.IDA_ACTOR = ar.IDA)
                inner join dbo.{1} eh on (eh.IDA = ar.IDA_ACTOR)
                where (ar.IDROLEACTOR = 'CSHBALANCEOFFICE')
                and ({2}) and ({3})";


                sqlQuery = String.Format(sqlQuery, TblCBACTORCBO_Work.First, TblCBENTITY_Work.First, dtEnabledAc, dtEnabledAr);
                #endregion Query

                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (false == TblCBACTORCBO_Work.Second)
                {
                    if (DbSvrType.dbSQL == serverType)
                        DataHelper.ExecuteNonQuery(pCS, CommandType.Text, $"create clustered index IX_{TblCBACTORCBO_Work.First} on dbo.{TblCBACTORCBO_Work.First} (IDA)");
                    else
                        DataHelper.ExecuteNonQuery(pCS, CommandType.Text, $"create index IX_{TblCBACTORCBO_Work.First} on dbo.{TblCBACTORCBO_Work.First} (IDA)");
                }

            }
        }

        /// <summary>
        /// Charger tous les acteurs descendants dans la table CBACTORCBO (Temporary table)
        /// </summary>
        /// <param name="pCS"></param>
        /// RD 20130502 [] Utilisation de la table CBACTORCBO
        /// EG 20140228 [19575][19666] REFACTORING
        /// FI 20141210 [20559] Modify
        // EG 20181010 PERF Refactoring
        // EG 20181119 PERF Correction post RC (Step 2)
        private void LoadCBODescendant(string pCS)
        {
            // 1- Charger les acteurs dans la table CBACTOR                        
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.CSHBALANCEOFFICE);
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);

            string dtEnabledAr = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar");
            string dtEnabledAc = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ac");


            string sqlQuery;
            if (DbSvrType.dbSQL == serverType)
            {
                #region SQLServer Recursivity (using clause WITH)
                sqlQuery = String.Format(@"with ActorHierarchy (IDA_ACTOR, IDA, IDROLEACTOR, LEVEL_ACTOR) as 
                (
                    select ar.IDA_ACTOR, ar.IDA, ar.IDROLEACTOR, 0 as LEVEL_ACTOR
                    from dbo.ACTORROLE ar
                    inner join dbo.{0} a_cbo on (a_cbo.IDA = ar.IDA)
                    where (ar.IDROLEACTOR = @IDROLEACTOR)

                    union all 

                    select ar.IDA_ACTOR, ar.IDA, ar.IDROLEACTOR, ah.LEVEL_ACTOR + 1 as LEVEL_ACTOR
                    from dbo.ACTORROLE ar
                    inner join ActorHierarchy ah on (ah.IDA = ar.IDA_ACTOR) and (ah.IDA <> ar.IDA) and (ah.LEVEL_ACTOR < 10)
                    where ({1})
                )
                insert into dbo.{2}
                select distinct ah.IDA, ah.IDA_ACTOR, ah.IDROLEACTOR, ah.LEVEL_ACTOR
                from ActorHierarchy ah
                inner join dbo.ACTOR ac on (ac.IDA = ah.IDA) and ({3});",
                TblCBACTORCBO_Work.First, dtEnabledAr, TblCBACTOR_Work.First, dtEnabledAc);
                #endregion
            }
            else if (DbSvrType.dbORA == serverType)
            {
                #region Oracle Recurisivity (using clause START WITH ... CONNECT BY)
                // Ne considérer que les Acteurs CBO existants dans la table CBACTORCBO : (ar.IDROLEACTOR=@IDROLEACTOR and a_cbo.IDA is not null)
                // Eviter des boucles infinies de l'acteur sur soit-même : (prior ar.IDA <> ar.IDA)
                // Eviter des boucles infinies entre différents acteurs : (level <= 10)
                sqlQuery = String.Format(@"insert into dbo.{0}
                (IDA, IDA_ACTOR, IDROLEACTOR, LEVEL_ACTOR)
                select distinct ar.IDA, ar.IDA_ACTOR, ar.IDROLEACTOR, LEVEL - 1 as LEVEL_ACTOR
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA) and ({1})
                left outer join dbo.{2} a_cbo on (a_cbo.IDA = ar.IDA)
                where {3}
                start with (ar.IDROLEACTOR = @IDROLEACTOR and a_cbo.IDA is not null)
                connect by nocycle (prior ar.IDA = ar.IDA_ACTOR) and (prior ar.IDA <> ar.IDA) and (level <= 10)" + Cst.CrLf,
                TblCBACTOR_Work.First, dtEnabledAc, TblCBACTORCBO_Work.First, dtEnabledAr);
                #endregion
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", String.Format("insert into {0}", TblCBACTOR_Work.First), qryParameters.GetQueryReplaceParameters()));
            DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == TblCBACTOR_Work.Second)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = $"create clustered index IX_{TblCBACTOR_Work.First} on dbo.{TblCBACTOR_Work.First} (IDA)";
                else if (DbSvrType.dbORA == serverType)
                    command = $"create index IX_{TblCBACTOR_Work.First} on dbo.{TblCBACTOR_Work.First} (IDA)";
                else
                    throw new NotImplementedException("RDBMS not implemented");

                qryParameters = new QueryParameters(pCS, command, null);
                AppInstance.TraceManager.TraceVerbose(null, $"create index: IX_{TblCBACTOR_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, $"update statistic on {TblCBACTOR_Work.First}");
                DataHelper.UpdateStatTable(pCS, TblCBACTOR_Work.First);
            }



            // 1bis- Charger les acteurs NON CLEARER/NON COMPART dans la table CBACTOR... 
            sqlQuery = String.Format(@"insert into dbo.{0} (IDA, LEVEL_ACTOR)
            select cba.IDA, -99
            from dbo.{1} cba
            {2}
            select distinct cba.IDA, -99
            from dbo.{1} cba
            inner join dbo.ACTORROLE ar on (ar.IDA = cba.IDA) and ({3})
            order by IDA",
            TblCBACTOR_NOTCLEARER_Work.First, TblCBACTOR_Work.First, DataHelper.SQLGetExcept(pCS),
            DataHelper.SQLColumnIn(pCS, "ar.IDROLEACTOR", CBHierarchy.RolesClearer, TypeData.TypeDataEnum.@string, false, true));

            qryParameters = new QueryParameters(pCS, sqlQuery, null);
            AppInstance.TraceManager.TraceVerbose(null, $"Name: insert into {TblCBACTOR_NOTCLEARER_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
            DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.Query);

            if (false == TblCBACTOR_NOTCLEARER_Work.Second)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = $"create clustered index IX_{TblCBACTOR_NOTCLEARER_Work.First} on dbo.{TblCBACTOR_NOTCLEARER_Work.First} (IDA)";
                else if (DbSvrType.dbORA == serverType)
                    command = $"create index IX_{TblCBACTOR_NOTCLEARER_Work.First} on dbo.{TblCBACTOR_NOTCLEARER_Work.First} (IDA)";
                else
                    throw new NotImplementedException("RDBMS not implemented");

                qryParameters = new QueryParameters(pCS, command, null);
                AppInstance.TraceManager.TraceVerbose(null, $"Name: create index IX_{TblCBACTOR_NOTCLEARER_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, $"update statistic on {TblCBACTOR_NOTCLEARER_Work.First}" );
                DataHelper.UpdateStatTable(pCS, TblCBACTOR_NOTCLEARER_Work.First);
            }

            // 1ter- Charger les acteurs CLEARER/COMPART dans la table CBACTOR...
            sqlQuery = String.Format(@"insert into dbo.{0} (IDA, LEVEL_ACTOR)
            select distinct cba.IDA, -99
            from dbo.{1} cba
            inner join dbo.ACTORROLE ar on (ar.IDA = cba.IDA) and ({2})
            order by IDA",
            TblCBACTOR_CLEARER_Work.First, TblCBACTOR_Work.First,
            DataHelper.SQLColumnIn(pCS, "ar.IDROLEACTOR", CBHierarchy.RolesClearer, TypeData.TypeDataEnum.@string, false, true));

            qryParameters = new QueryParameters(pCS, sqlQuery, null);
            AppInstance.TraceManager.TraceVerbose(null, $"Name: insert into {TblCBACTOR_CLEARER_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
            DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.Query);

            if (false == TblCBACTOR_CLEARER_Work.Second)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = $"create clustered index IX_{TblCBACTOR_CLEARER_Work.First} on dbo.{TblCBACTOR_CLEARER_Work.First} (IDA)";
                else if (DbSvrType.dbORA == serverType)
                    command = $"create index IX_{TblCBACTOR_CLEARER_Work.First} on dbo.{TblCBACTOR_CLEARER_Work.First} (IDA)";
                else
                    throw new NotImplementedException("RDBMS not implemented");

                qryParameters = new QueryParameters(pCS, command, null);
                AppInstance.TraceManager.TraceVerbose(null, $"Name: create index IX_{TblCBACTOR_CLEARER_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, $"update statistic on {TblCBACTOR_CLEARER_Work.First}");
                DataHelper.UpdateStatTable(pCS, TblCBACTOR_CLEARER_Work.First);
            }


            // 2- Charger les acteurs à partir de la table CBACTOR
            sqlQuery = String.Format(@"select distinct a_cb.IDA, a_cb.IDA_ACTOR, a_cb.IDROLEACTOR, a.IDENTIFIER, a_cb.LEVEL_ACTOR
            from dbo.{0} a_cb
            inner join dbo.ACTOR a on (a.IDA = a_cb.IDA)
            order by a_cb.LEVEL_ACTOR, a_cb.IDA_ACTOR, a_cb.IDA", TblCBACTOR_Work.First);
            qryParameters = new QueryParameters(pCS, sqlQuery, null);
            DataSet dsCBO = DataHelper.ExecuteDataset(DbConnection, CommandType.Text, qryParameters.Query);

            DataTable dtCBO = dsCBO.Tables[0];
            DataRow[] drCBO = dtCBO.Select();
            //
            if (drCBO.Length > 0)
                this.Add(drCBO);
        }

        /// <summary>
        /// Charger tous les rôles de tous les acteurs dans la table CBACTOR
        /// Sauf le rôle CBO
        /// </summary>
        /// <param name="pCS"></param>
        /// RD 20130502 [] Utilisation de la table CBACTOR
        // EG 20142028 [19575][19666] REFACTORING
        // EG 20181119 PERF Correction post RC (Step 2)
        private void LoadRoles(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.MARGINREQOFFICE);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), this.Ida_Entity);
            DataParameter dataParameter = new DataParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR + "_" + RoleActor.CSHBALANCEOFFICE, DbType.AnsiStringFixedLength, SQLCst.UT_ROLEACTOR_LEN);
            dataParameters.Add(dataParameter, RoleActor.CSHBALANCEOFFICE);

            string sqlQuery = @"
            /* 1- Tous les rôles excepté MRO et CBO */

            select ar.IDA, ar.IDA_ACTOR, ar.IDROLEACTOR
            from dbo.ACTORROLE ar
            inner join dbo.{0} a_cb on (a_cb.IDA = ar.IDA)
            where ({1}) and (ar.IDROLEACTOR != @IDROLEACTOR_CSHBALANCEOFFICE) and (ar.IDROLEACTOR != @IDROLEACTOR)
            
            union all

            /* 2- Les rôle MRO uniquement */

            select result.IDA, result.IDA_ACTOR, result.IDROLEACTOR
            from
            (
                select ar.IDA, ar.IDA_ACTOR, ar.IDROLEACTOR
                from dbo.ACTORROLE ar
                inner join dbo.{0} a_cb on (a_cb.IDA = ar.IDA)
                where ({1}) and (ar.IDROLEACTOR = @IDROLEACTOR) and ((isnull(ar.IDA_ACTOR,ar.IDA) = ar.IDA) or (ar.IDA_ACTOR = @IDA_ENTITY))

                union

                select ar.IDA, ar.IDA_ACTOR, ar.IDROLEACTOR
                from dbo.ACTORROLE ar
                inner join dbo.{0} a_cb on (a_cb.IDA = ar.IDA)
                inner join dbo.{2} eh on (eh.IDA = ar.IDA_ACTOR)
                where ({1}) and (ar.IDROLEACTOR = @IDROLEACTOR)
            ) result " + Cst.CrLf;

            sqlQuery = String.Format(sqlQuery, TblCBACTOR_Work.First, OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar"), TblCBENTITY_Work.First);


            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), dataParameters);
            DataSet dsCBO = DataHelper.ExecuteDataset(DbConnection, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            DataTable dtCBO = dsCBO.Tables[0];
            DataRow[] drCBO = dtCBO.Select();
            //
            if (drCBO.Length > 0)
            {
                foreach (DataRow rowCBO in drCBO)
                {
                    int ida = Convert.ToInt32(rowCBO["IDA"]);
                    int ida_actor = (Convert.IsDBNull(rowCBO["IDA_ACTOR"]) ? 0 : Convert.ToInt32(rowCBO["IDA_ACTOR"]));

                    string idRoleActor = rowCBO["IDROLEACTOR"].ToString();
                    RoleActor roleActor = RoleActor.EXTERNAL;

                    if (System.Enum.IsDefined(typeof(RoleActor), idRoleActor.Trim()))
                        roleActor = (RoleActor)System.Enum.Parse(typeof(RoleActor), idRoleActor, true);

                    // Ajouter le rôle aux acteurs concernés de la hiérarchie
                    this.Add(new ActorRole(ida, roleActor, ida_actor));
                }
            }
        }

        /// <summary>
        /// Chargement des caractéritiques Business des Acteurs MRO et CBO
        /// <para>Book du MRO, s'il existe</para>
        /// <para>Book du CBO, s'il existe</para>
        /// <para>CBSCOPE</para>
        /// <para>isUseAvailableCash</para>
        /// <para>exchangeType</para>
        /// <para>exchangeIDC, s'il existe</para>
        /// <para>cashAndCollateralLocalization</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// RD 20130502 [] Utilisation de la table CBACTOR
        /// EG 20130924 [18993] Suppression Paramètres en trop IDA_ENTITY + Gestion paramètre (ORACLE) sur MULTISELECT
        /// EG 20142028 [19575][19666] REFACTORING
        /// PM 20140901 [20066][20185] Gestion méthode UK
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel LoadBusinessAttribute(string pCS)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.MARGINREQOFFICE);            //
            dataParameters.Add(new DataParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR + "_" + RoleActor.CSHBALANCEOFFICE,
                DbType.AnsiStringFixedLength, SQLCst.UT_ROLEACTOR_LEN), RoleActor.CSHBALANCEOFFICE);

            StrBuilder sqlQuery = new StrBuilder();
            // PM 20180130 [CHEETAH] Ajout distinct
            //sqlQuery += SQLCst.SELECT + "a_cb.IDA," + Cst.CrLf;
            sqlQuery += SQLCst.SELECT_DISTINCT + "a_cb.IDA," + Cst.CrLf;
            sqlQuery += "mro.IDB" + SQLCst.AS + "IDB_MRO, bMRO.IDENTIFIER" + SQLCst.AS + "IDENTIFIERB_MRO, mro.IMSCOPE," + Cst.CrLf;
            sqlQuery += "cbo.IDB" + SQLCst.AS + "IDB_CBO, bCBO.IDENTIFIER" + SQLCst.AS + "IDENTIFIERB_CBO," + Cst.CrLf;
            sqlQuery += "cbo.CBSCOPE, cbo.ISUSEAVAILABLECASH, cbo.EXCHANGEIDC, cbo.CASHANDCOLLATERAL, cbo.ISMANAGEMENTBALANCE, cbo.MGCCALCMETHOD," + Cst.CrLf;
            sqlQuery += "cbo.CBCALCMETHOD, cbo.CBIDC," + Cst.CrLf;
            sqlQuery += "(case when mro.IDA is null then 0 else 1 end)" + SQLCst.AS + "IS_MRO," + Cst.CrLf;
            sqlQuery += "(case when cbo.IDA is null then 0 else 1 end)" + SQLCst.AS + "IS_CBO" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + TblCBACTOR_Work.First + " a_cb" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.RISKMARGIN, SQLJoinTypeEnum.Left, "a_cb.IDA", "mro", DataEnum.EnabledOnly);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.CASHBALANCE, SQLJoinTypeEnum.Left, "a_cb.IDA", "cbo", DataEnum.EnabledOnly);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.BOOK, SQLJoinTypeEnum.Left, "mro.IDB", "bMRO", DataEnum.EnabledOnly);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.BOOK, SQLJoinTypeEnum.Left, "cbo.IDB", "bCBO", DataEnum.EnabledOnly);
            sqlQuery += SQLCst.WHERE + @"(a_cb.IDA in 
            (select ar.IDA 
            from dbo.ACTORROLE ar 
            where (ar.IDROLEACTOR = @IDROLEACTOR_CSHBALANCEOFFICE) 
            or (ar.IDROLEACTOR = @IDROLEACTOR)))" + Cst.CrLf;

            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;

            sqlQuery += SQLCst.SELECT + "cp.IDA_CBO, cp.PRIORITYRANK, cp.COLLATERALCATEGORY, cp.ASSETCATEGORY, cp.IDASSET" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.COLLATERALPRIORITY + " cp" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + TblCBACTOR_Work.First + " a_cb on (a_cb.IDA = cp.IDA_CBO)" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "cp") + Cst.CrLf;
            sqlQuery += SQLCst.AND + @"(a_cb.IDA in 
            (select ar.IDA 
            from dbo.ACTORROLE ar 
            where (ar.IDROLEACTOR = @IDROLEACTOR_CSHBALANCEOFFICE) and (@IDROLEACTOR=@IDROLEACTOR)))" + Cst.CrLf;

            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;

            sqlQuery += SQLCst.SELECT + "ccp.IDA_CBO, ccp.PRIORITYRANK, ccp.IDC" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MGCCURPRIORITY + " ccp" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + TblCBACTOR_Work.First + " a_cb on (a_cb.IDA = ccp.IDA_CBO)" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ccp") + Cst.CrLf;
            sqlQuery += SQLCst.AND + @"(a_cb.IDA in 
            (select ar.IDA 
            from dbo.ACTORROLE ar 
            where (ar.IDROLEACTOR = @IDROLEACTOR_CSHBALANCEOFFICE) and (@IDROLEACTOR=@IDROLEACTOR)))" + Cst.CrLf;

            //FI 20120904 use queryParameters 
            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), dataParameters);
            DataSet dsBusinessAttrib = DataHelper.ExecuteDataset(DbConnection, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            DataTable dtMroCboAttrib = dsBusinessAttrib.Tables[0];
            DataTable dtCboPriority = dsBusinessAttrib.Tables[1];
            DataTable dtCboCurrencyPriority = dsBusinessAttrib.Tables[2];

            DataRelation relCboPriority = new DataRelation("CboPriority", dtMroCboAttrib.Columns["IDA"], dtCboPriority.Columns["IDA_CBO"], false);
            dsBusinessAttrib.Relations.Add(relCboPriority);

            relCboPriority = new DataRelation("CboCurrencyPriority", dtMroCboAttrib.Columns["IDA"], dtCboCurrencyPriority.Columns["IDA_CBO"], false);
            dsBusinessAttrib.Relations.Add(relCboPriority);

            DataRow[] drMroCboAttrib = dtMroCboAttrib.Select();

            if (drMroCboAttrib.Length > 0)
            {
                int idB_MRO = 0;
                string identifierB_MRO = string.Empty;
                string scope_MRO = string.Empty;
                GlobalElementaryEnum scopeEnum_MRO = GlobalElementaryEnum.Elementary;
                //
                int idB_CBO = 0;
                string identifierB_CBO = string.Empty;
                string scope_CBO = string.Empty;
                GlobalElementaryEnum scopeEnum_CBO = GlobalElementaryEnum.Elementary;
                bool isUseAvailableCash = false;
                string exchangeIDC = string.Empty;
                CBExchangeTypeEnum exchangeTypeEnum = CBExchangeTypeEnum.FCU;
                string cashAndCollateral = string.Empty;
                CashAndCollateralLocalizationEnum cashAndCollateralLocalization = CashAndCollateralLocalizationEnum.CBO;
                string mgcCalcMethod = string.Empty;
                MarginCallCalculationMethodEnum marginCallCalculationMethod = MarginCallCalculationMethodEnum.MGCCTRVAL;
                bool isManagementBalance = false;
                string cbCalcMethod = string.Empty;
                CashBalanceCalculationMethodEnum cashBalanceCalculationMethod = CashBalanceCalculationMethodEnum.CSBDEFAULT;
                string cashBalanceIDC = string.Empty;
                //
                foreach (DataRow rowMroCbo in drMroCboAttrib)
                {
                    int idA = Convert.ToInt32(rowMroCbo["IDA"]);
                    //
                    bool is_MRO = Convert.ToBoolean(rowMroCbo["IS_MRO"]);
                    bool is_CBO = Convert.ToBoolean(rowMroCbo["IS_CBO"]);
                    //
                    if (is_MRO)
                    {
                        idB_MRO = (Convert.IsDBNull(rowMroCbo["IDB_MRO"]) ? 0 : Convert.ToInt32(rowMroCbo["IDB_MRO"]));
                        identifierB_MRO = (Convert.IsDBNull(rowMroCbo["IDENTIFIERB_MRO"]) ? string.Empty :
                            rowMroCbo["IDENTIFIERB_MRO"].ToString());
                        //
                        scope_MRO = rowMroCbo["IMSCOPE"].ToString();
                        //
                        // TODO: Créer un message d'erreur dans le cas de parametrage incohérent
                        //if (false == Enum.IsDefined(typeof(GlobalElementaryEnum), scope_MRO.Trim()))
                        //{
                        //    base.CodeReturn = Cst.ErrLevel.FAILURE;
                        //    continue;
                        //}
                        //
                        scopeEnum_MRO = (GlobalElementaryEnum)System.Enum.Parse(typeof(GlobalElementaryEnum), scope_MRO, true);
                    }
                    //
                    if (is_CBO)
                    {
                        idB_CBO = (Convert.IsDBNull(rowMroCbo["IDB_CBO"]) ? 0 : Convert.ToInt32(rowMroCbo["IDB_CBO"]));
                        identifierB_CBO = (Convert.IsDBNull(rowMroCbo["IDENTIFIERB_CBO"]) ? string.Empty :
                            rowMroCbo["IDENTIFIERB_CBO"].ToString());
                        //
                        scope_CBO = rowMroCbo["CBSCOPE"].ToString();
                        //
                        // TODO: Créer un message d'erreur dans le cas de parametrage incohérent
                        //if (false == Enum.IsDefined(typeof(GlobalElementaryEnum), scope_CBO.Trim()))
                        //{
                        //    base.CodeReturn = Cst.ErrLevel.FAILURE;
                        //    //
                        //    continue;
                        //}
                        //
                        scopeEnum_CBO = (GlobalElementaryEnum)System.Enum.Parse(typeof(GlobalElementaryEnum), scope_CBO, true);
                        //
                        isUseAvailableCash = (!Convert.IsDBNull(rowMroCbo["ISUSEAVAILABLECASH"]) && Convert.ToBoolean(rowMroCbo["ISUSEAVAILABLECASH"]));
                        //
                        exchangeIDC = (Convert.IsDBNull(rowMroCbo["EXCHANGEIDC"]) ? string.Empty : rowMroCbo["EXCHANGEIDC"].ToString());
                        exchangeTypeEnum = CBExchangeTypeEnum.FCU;
                        if (StrFunc.IsFilled(exchangeIDC))
                            exchangeTypeEnum = CBExchangeTypeEnum.ACU_BUSINESSDATE;
                        //
                        cashAndCollateral = (Convert.IsDBNull(rowMroCbo["CASHANDCOLLATERAL"]) ? string.Empty :
                            rowMroCbo["CASHANDCOLLATERAL"].ToString());
                        //
                        // TODO: Créer un message d'erreur dans le cas de parametrage incohérent
                        //if (false == Enum.IsDefined(typeof(CashAndCollateralLocalizationEnum), cashAndCollateral.Trim()))
                        //{
                        //    base.CodeReturn = Cst.ErrLevel.FAILURE;
                        //    //
                        //    continue;
                        //}
                        //
                        cashAndCollateralLocalization =
                            (CashAndCollateralLocalizationEnum)System.Enum.Parse(typeof(CashAndCollateralLocalizationEnum), cashAndCollateral, true);

                        mgcCalcMethod = rowMroCbo["MGCCALCMETHOD"].ToString();

                        marginCallCalculationMethod =
                            (MarginCallCalculationMethodEnum)System.Enum.Parse(typeof(MarginCallCalculationMethodEnum), mgcCalcMethod, true);

                        cbCalcMethod = rowMroCbo["CBCALCMETHOD"].ToString();

                        cashBalanceCalculationMethod = (CashBalanceCalculationMethodEnum)System.Enum.Parse(typeof(CashBalanceCalculationMethodEnum), cbCalcMethod, true);

                        cashBalanceIDC = (Convert.IsDBNull(rowMroCbo["CBIDC"]) ? string.Empty : rowMroCbo["CBIDC"].ToString());

                        isManagementBalance = (!Convert.IsDBNull(rowMroCbo["ISMANAGEMENTBALANCE"]) && Convert.ToBoolean(rowMroCbo["ISMANAGEMENTBALANCE"]));
                    }
                    //
                    CBBusinessAttribute businessAttribute =
                        new CBBusinessAttribute(idA, is_MRO, idB_MRO, identifierB_MRO, scopeEnum_MRO,
                         is_CBO, idB_CBO, identifierB_CBO, scopeEnum_CBO, isUseAvailableCash, exchangeTypeEnum,
                         marginCallCalculationMethod, exchangeIDC, cashAndCollateralLocalization, isManagementBalance,
                         cashBalanceCalculationMethod, cashBalanceIDC);
                    //
                    this.Add(businessAttribute);
                    //
                    if ((idB_CBO == 0) &&
                        ((scopeEnum_CBO == EfsML.Enum.GlobalElementaryEnum.Global) ||
                        (scopeEnum_CBO == EfsML.Enum.GlobalElementaryEnum.Full)))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                        //CB_BusinessAttrib_NoBookForGlobalCBO
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4002), 2,
                            new LogParam(FindActor(idA).Identifier + "[id:" + idA.ToString() + "]"),
                            new LogParam(Identifier_Entity),
                            new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                    //
                    if (is_CBO)
                    {
                        // 1- Priorités d'utilisation des Actifs
                        DataRow[] drCboPriority = rowMroCbo.GetChildRows(dsBusinessAttrib.Relations["CboPriority"]);

                        foreach (DataRow rowCboPriority in drCboPriority)
                        {
                            int priority = Convert.ToInt32(rowCboPriority["PRIORITYRANK"]);
                            string collatCat = rowCboPriority["COLLATERALCATEGORY"].ToString();
                            //
                            // TODO: Créer un message d'erreur dans le cas de parametrage incohérent
                            //if (false == Enum.IsDefined(typeof(CollateralCategoryEnum), collatCat.Trim()))
                            //{
                            //    base.CodeReturn = Cst.ErrLevel.FAILURE;
                            //    continue;
                            //}
                            //
                            CollateralCategoryEnum collatCategory = (CollateralCategoryEnum)System.Enum.Parse(typeof(CollateralCategoryEnum), collatCat, true);
                            string assetCategory = (Convert.IsDBNull(rowCboPriority["ASSETCATEGORY"]) ? string.Empty :
                                rowCboPriority["ASSETCATEGORY"].ToString());
                            //
                            int idAsset = (Convert.IsDBNull(rowCboPriority["IDASSET"]) ? 0 : Convert.ToInt32(rowCboPriority["IDASSET"]));
                            //
                            CBCollateralPriority collateralPriority = new CBCollateralPriority(idA, priority, collatCategory, assetCategory, idAsset);
                            this.Add(collateralPriority);
                        }

                        // 2- Priorités de couverture des devises
                        DataRow[] drCboCurrencyPriority = rowMroCbo.GetChildRows(dsBusinessAttrib.Relations["CboCurrencyPriority"]);

                        foreach (DataRow rowCboPriority in drCboCurrencyPriority)
                        {
                            int priority = Convert.ToInt32(rowCboPriority["PRIORITYRANK"]);
                            string currency = rowCboPriority["IDC"].ToString();

                            CBCollateralCurrencyPriority collateralCurrencyPriority = new CBCollateralCurrencyPriority(idA, priority, currency);
                            this.Add(collateralCurrencyPriority);
                        }
                    }
                }
            }

            return codeReturn;
        }

        /// <summary>
        /// Renvoi la clause Where à inclure dans toute requête pour le chargement d'un Rôle donné vis-à-vis:
        /// <para>- de tous (NULL)</para>
        /// <para>- de l'acteur lui même</para> 
        /// <para>- de l'Entity en cours</para>
        /// <para>- d'un Acteur déscendant de l'Entity</para>
        /// <para>La requête principale doit avoir:</para>
        /// <para>- "ar" comme alias de la table ACTORROLE</para>
        /// <para>- un Parameter @IDROLEACTOR pour le rôle en question</para>
        /// <para>- un Parameter @IDA_ENTITY pour l'Entité concernée</para>
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20131114 [19186] NEW
        // FI 20141210 [20559] Modify (juste une réécriture sans chgt de comportement mais meilleure lecture du code)
        private static string GetSqlWhere_RoleActor(string pCS)
        {

            // FI 20141210 [20559] 
            // Restriction pour obtenir un acteur avec un rôle 
            //- vis à vis de lui même ou
            //- vis à vis de l'entité ou
            //- vis à vis de d'un acteur de la hierarchie des acteurs lié à l'entité
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar") + Cst.CrLf;
            sqlQuery += "and (ar.IDROLEACTOR=@IDROLEACTOR)" + Cst.CrLf;
            sqlQuery += @"and (  
                        (isnull(ar.IDA_ACTOR,ar.IDA)=ar.IDA)
                        or (ar.IDA_ACTOR=@IDA_ENTITY) 
                        or (ar.IDA_ACTOR in(##QueryEntityHierarchy))
                        )";

            string ret = sqlQuery.ToString();


            StrBuilder sqlQuery2 = new StrBuilder();
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                #region SQLServer Recursivity (using clause WITH)
                sqlQuery2 += SQLCst.SELECT + "eh.IDA" + Cst.CrLf;
                sqlQuery2 += SQLCst.X_FROM + "EntityHierarchy eh" + Cst.CrLf;
                #endregion
            }
            else if (DbSvrType.dbORA == serverType)
            {
                #region Oracle Recurisivity (using clause START WITH ... CONNECT BY)
                sqlQuery2 += SQLCst.SELECT + "ar.IDA" + Cst.CrLf;
                sqlQuery2 += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar" + Cst.CrLf;
                sqlQuery2 += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ar.IDA", "a", DataEnum.EnabledOnly);
                sqlQuery2 += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar") + Cst.CrLf;
                sqlQuery2 += SQLCst.START_WITH + "(ar.IDA_ACTOR=@IDA_ENTITY)" + Cst.CrLf;
                sqlQuery2 += SQLCst.CONNECT_BY_NOCYCLE + "(" + SQLCst.PRIOR + "ar.IDA = ar.IDA_ACTOR)" + Cst.CrLf;
                sqlQuery2 += SQLCst.AND + "(" + SQLCst.PRIOR + "ar.IDA != ar.IDA)" + Cst.CrLf;
                // Pour la sécurité, eviter des boucles infinies entre différents acteurs 
                sqlQuery2 += SQLCst.AND + "(LEVEL <= 10 )" + Cst.CrLf;
                #endregion
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            ret = ret.Replace("##QueryEntityHierarchy", sqlQuery2.ToString());

            return ret;
        }

        /// <summary>
        /// Chargement de la hierarchie des acteurs enfants de l'entité (Alimentation de la table tblCBENTITY_Work)
        /// </summary>
        /// <param name="pCS"></param>
        /// EG 20150724 [21187] Refactoring
        /// FI 20161021 [22152] Refactoring
        // EG 20181010 PERF Refactoring
        private void LoadEntityHierarchy(string pCS)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(DbConnection);
            string sqlQuery;
            if (DbSvrType.dbSQL == serverType)
            {
                #region SQLServer Recursivity (using clause WITH)
                sqlQuery = @"with EntityHierarchy (IDA, LEVEL_ACTOR) as (
                select ar.IDA, 0 as LEVEL_ACTOR
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA) and {0}
                where (ar.IDA_ACTOR = @IDA_ENTITY) and {1}
                
                union all
    	        
                select ar.IDA, eh.LEVEL_ACTOR + 1 as LEVEL_ACTOR   
	            from dbo.ACTORROLE ar   
	            inner join dbo.ACTOR ac on  (ac.IDA = ar.IDA) and {0}
	            inner join EntityHierarchy eh on (ar.IDA_ACTOR = eh.IDA) and (ar.IDA != eh.IDA) and (eh.LEVEL_ACTOR < 10)   
	            where {1}
                )
                insert into {2} select IDA, LEVEL_ACTOR from EntityHierarchy;" + Cst.CrLf;
                #endregion SQLServer Recursivity (using clause WITH)
            }
            else if (DbSvrType.dbORA == serverType)
            {
                #region Oracle Recurisivity (using clause START WITH ... CONNECT BY)
                sqlQuery = @"insert into dbo.{2} (IDA, LEVEL_ACTOR)
                select ar.IDA, LEVEL as LEVEL_ACTOR
                from dbo.ACTORROLE ar
                inner join dbo.ACTOR ac on (ac.IDA = ar.IDA) and {0}
                where {1}
                start with (ar.IDA_ACTOR = @IDA_ENTITY)
                connect by nocycle (prior ar.IDA = ar.IDA_ACTOR) and (prior ar.IDA != ar.IDA) and (LEVEL <= 10)" + Cst.CrLf;
                #endregion Oracle Recurisivity (using clause START WITH ... CONNECT BY)
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            string sqlQueryHierarchy = String.Format(sqlQuery, OTCmlHelper.GetSQLDataDtEnabled(pCS, "ac"), OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar"), TblCBENTITY_Work.First);

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), this.Ida_Entity);
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQueryHierarchy, dataParameters);

            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", $"insert into {TblCBENTITY_Work.First}", qryParameters.GetQueryReplaceParameters()));
            DataHelper.ExecuteNonQuery(DbConnection, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());

            if (false == TblCBENTITY_Work.Second)
            {
                string command;
                if (DbSvrType.dbSQL == serverType)
                    command = $"create clustered index IX_{TblCBENTITY_Work.First} on dbo.{TblCBENTITY_Work.First} (IDA)";
                else if (DbSvrType.dbORA == serverType)
                    command = $"create index IX_{TblCBENTITY_Work.First} on dbo.{TblCBENTITY_Work.First} (IDA)";
                else
                    throw new NotImplementedException("RDBMS not implemented");

                qryParameters = new QueryParameters(pCS, command, null);
                AppInstance.TraceManager.TraceVerbose(null, $"create index IX_{TblCBENTITY_Work.First} - SQL:{qryParameters.GetQueryReplaceParameters()}");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query);
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, $"update statistic on {TblCBENTITY_Work.First}");
                DataHelper.UpdateStatTable(pCS, TblCBENTITY_Work.First);
            }
        }

        /// <summary>
        /// Vérification si les caratéristiques du CBO/MRO sont bien parametrées:
        /// <para>CBO: référentiel "Garanties/Soldes"</para>
        /// <para>MRO: référentiel "Risque"</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel CheckCboMroReferential(List<CBActorNode> pActors)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel codeReturnActor = Cst.ErrLevel.SUCCESS;
            //
            foreach (CBActorNode actor in pActors)
            {
                // Acteur CBO
                if (actor.IsExistRole(RoleActor.CSHBALANCEOFFICE) && actor.IsNotCBO)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4001), 2,
                        new LogParam(actor.Identifier + " [id:" + actor.Ida.ToString() + "]"),
                        new LogParam(Identifier_Entity),
                        new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                    codeReturnActor = Cst.ErrLevel.FAILURE;
                }

                // Acteur MRO
                if (actor.IsExistRole(RoleActor.MARGINREQOFFICE) && actor.IsNotMRO)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4003), 2,
                        new LogParam(actor.Identifier + " [id:" + actor.Ida.ToString() + "]"),
                        new LogParam(Identifier_Entity),
                        new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                    codeReturnActor = Cst.ErrLevel.FAILURE;
                }

                if (codeReturnActor != Cst.ErrLevel.SUCCESS)
                    codeReturn = codeReturnActor;

                // Acteurs enfants 
                codeReturnActor = CheckCboMroReferential(actor.ChildActors);

                if (codeReturnActor != Cst.ErrLevel.SUCCESS)
                    codeReturn = codeReturnActor;
            }
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDrActors"></param>
        /// EG 20240125 [XXXXX] Correction Bug Code analysis sur idRoleActor
        private void Add(DataRow[] pDrActors)
        {
            foreach (DataRow rowActor in pDrActors)
            {
                int ida = Convert.ToInt32(rowActor["IDA"]);
                string identifier = rowActor["IDENTIFIER"].ToString();
                int ida_actor = (Convert.IsDBNull(rowActor["IDA_ACTOR"]) ? 0 : Convert.ToInt32(rowActor["IDA_ACTOR"]));
                string idRoleActor = rowActor["IDROLEACTOR"].ToString();
                RoleActor roleActor = RoleActor.EXTERNAL;

                if (System.Enum.IsDefined(typeof(RoleActor), idRoleActor.Trim()))
                    roleActor = (RoleActor)System.Enum.Parse(typeof(RoleActor), idRoleActor, true);

                int level = Convert.ToInt32(rowActor["LEVEL_ACTOR"]);

                CBActorNode node = new CBActorNode(ida, identifier, level, roleActor, ida_actor);

                // Ajouter l'acteur à la hiérarchie
                this.Add(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        public void Add(ActorRole pRole)
        {
            Add(pRole, this.ChildActors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <param name="pActors"></param>
        public void Add(ActorRole pRole, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pRole.IdA == actor.Ida)
                    actor.Add(pRole);
                //
                Add(pRole, actor.ChildActors);
            }
        }

        /// <summary>
        /// Ajoute un noeud Acteur à la hiérarchie
        /// </summary>
        /// <param name="pActor">L'Acteur à rajouter</param>
        /// <returns></returns>
        public CBActorNode Add(CBActorNode pActor)
        {
            return Add(pActor, 0, this.ChildActors, 0);
        }
        /// <summary>
        /// Ajoute un noeud Acteur à la hiérarchie
        /// </summary>
        /// <param name="pActor">L'Acteur à rajouter</param>
        /// <param name="pIda_AncestorTreeNode">L'Acteur parent du noeud de l'arbre en cours de traitement</param>
        /// <param name="pActors_TreeNode">La liste des Acteurs du noeud de l'arbre en cours de traitement</param>
        /// <param name="pLevel_TreeNode">Le niveau de l'arbre en cours de traitement</param>
        /// <returns></returns>
        public CBActorNode Add(CBActorNode pActor, int pIda_AncestorTreeNode, List<CBActorNode> pActors_TreeNode,
            int pLevel_TreeNode)
        {
            CBActorNode retNode = null;
            //
            if (pActor.Level == pLevel_TreeNode)
            {
                if (pIda_AncestorTreeNode == 0 || (pActor.Roles.First().IdA_Actor == pIda_AncestorTreeNode))
                {
                    // L'Acteur existe au même niveau
                    retNode = this.Find(pActor);
                    //
                    if (retNode != null)
                    {
                        // Plusieurs rôles vis-à-vis du même acteur
                        if (pIda_AncestorTreeNode == 0 || retNode.IsExistRole(pIda_AncestorTreeNode))
                        {
                            foreach (ActorRole role in pActor.Roles)
                                retNode.Roles.Add(role);
                        }
                        else
                        {
                            // Plusieurs rôles vis-à-vis de plusieurs acteurs
                            pActors_TreeNode.Add(retNode);
                            retNode = pActors_TreeNode.Find(match => match.CompareTo(retNode) == 0);
                        }
                    }
                    else
                    {
                        // L'Acteur existe sur le niveau 0
                        int ida_root = this.GetRoot(pIda_AncestorTreeNode);
                        retNode = this.ChildActors.Find(actor => actor.Ida == pActor.Ida && actor.Ida != ida_root);
                        //
                        if (retNode != null)
                        {
                            pActors_TreeNode.Add(retNode);
                            this.ChildActors.Remove(retNode);
                            retNode.UpdateLevel(pActor.Level);
                        }
                        else
                        {
                            pActors_TreeNode.Add(pActor);
                            retNode = pActors_TreeNode.Find(match => match.CompareTo(pActor) == 0);
                        }
                    }
                }
            }
            else if (pActor.Level > pLevel_TreeNode)
            {
                foreach (CBActorNode actor in pActors_TreeNode)
                {
                    // Le rôle est vis-à-vis de l'acteur soit même
                    if ((pActor.Ida == actor.Ida) && (pActor.Roles.First().IdA_Actor == actor.Ida))
                    {
                        retNode = actor;
                        //
                        foreach (ActorRole role in pActor.Roles)
                            retNode.Roles.Add(role);
                    }
                    else
                        retNode = Add(pActor, actor.Ida, actor.ChildActors, pLevel_TreeNode + 1);
                    //
                    if (null != retNode)
                        break;
                }
            }
            //
            return retNode;
        }
        public void Add(CBBookLeaf pBook)
        {
            bool amountAdded = false;
            Add(pBook, this.ChildActors, ref amountAdded);
        }
        public static void Add(CBBookLeaf pBook, List<CBActorNode> pActors, ref bool pAmountAdded)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pBook.Ida == actor.Ida)
                    actor.Add(pBook, ref pAmountAdded);
                else
                    Add(pBook, actor.ChildActors, ref pAmountAdded);
            }
        }
        public void Add(CBBookTrade pTrade)
        {
            Add(pTrade, this.ChildActors);
        }
        private static void Add(CBBookTrade pTrade, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pTrade.IDA == actor.Ida)
                    actor.Add(pTrade);
                else
                    Add(pTrade, actor.ChildActors);
            }
        }
        public void Add(CBBusinessAttribute pAttribute)
        {
            Add(pAttribute, this.ChildActors);
        }
        private static void Add(CBBusinessAttribute pAttribute, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if ((actor.Ida == pAttribute.IDA) && (actor.BusinessAttribute == null))
                    actor.BusinessAttribute = pAttribute;
                //
                Add(pAttribute, actor.ChildActors);
            }
        }

        public void Add(CBCollateralPriority pPriority)
        {
            Add(pPriority, this.ChildActors);
        }
        private static void Add(CBCollateralPriority pPriority, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (actor.IsCBO && (actor.Ida == pPriority.Ida_CBO))
                    actor.BusinessAttribute.CollateralPriority.Add(pPriority);
                //
                Add(pPriority, actor.ChildActors);
            }
        }

        public void Add(CBCollateralCurrencyPriority pPriority)
        {
            Add(pPriority, this.ChildActors);
        }
        private static void Add(CBCollateralCurrencyPriority pPriority, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (actor.IsCBO && (actor.Ida == pPriority.Ida_CBO))
                    actor.BusinessAttribute.CollateralCurrencyPriority.Add(pPriority);
                //
                Add(pPriority, actor.ChildActors);
            }
        }

        public void Add(CBTradeInfo pTradeInfo)
        {
            Add(pTradeInfo, this.ChildActors);
        }
        private static void Add(CBTradeInfo pTradeInfo, List<CBActorNode> pActors)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pTradeInfo.Ida == actor.Ida)
                    actor.Add(pTradeInfo);
                //
                Add(pTradeInfo, actor.ChildActors);
            }
        }

        /// <summary>
        /// Retourne l'Acteur racine de l'arbre dont dépond l'Acteur {pIDA}
        /// </summary>
        /// <param name="pIDA"></param>
        /// <returns></returns>
        public int GetRoot(int pIDA)
        {
            foreach (CBActorNode root in this.ChildActors)
            {
                if (IsRootOf(root, pIDA))
                    return root.Ida;
            }
            //
            return 0;
        }

        /// <summary>
        /// Indique si Le noeud {pRoot} est la racine de l'arbre dont dépond l'Acteur {pIDA}
        /// </summary>
        /// <param name="pRoot"></param>
        /// <param name="pIDA"></param>
        /// <returns></returns>
        public bool IsRootOf(CBActorNode pRoot, int pIDA)
        {
            foreach (CBActorNode child in pRoot.ChildActors)
            {
                if (child.Ida == pIDA)
                    return true;
                //
                return IsRootOf(child, pIDA);
            }
            //
            return false;
        }

        public CBActorNode Find(CBActorNode pActor)
        {
            return Find(pActor, this.ChildActors, 0);
        }
        public CBActorNode Find(CBActorNode pActor, List<CBActorNode> pActors, int pLevelActors)
        {
            CBActorNode retNode = null;
            //
            if (pActor.Level == pLevelActors)
                retNode = pActors.Find(match => match.CompareTo(pActor) == 0);
            else if (pActor.Level > pLevelActors)
            {
                foreach (CBActorNode actor in pActors)
                {
                    retNode = Find(pActor, actor.ChildActors, pLevelActors + 1);
                    //
                    if (null != retNode)
                        break;
                }
            }
            //
            return retNode;
        }
        /// <summary>
        /// Retourne la liste de tous les IDA des acteurs de la hiérarchie
        /// </summary>
        /// <returns></returns>
        public List<int> FindAllActorID()
        {
            return FindAllActorID(null);
        }
        /// <summary>
        /// Retourne la liste de tous les IDA des acteurs de la hiérarchie avec le Rôle {pRole}
        /// </summary>
        /// <returns></returns>
        public List<int> FindAllActorID(RoleActor pRole)
        {
            return FindAllActorID(new RoleActor[] { pRole });
        }
        /// <summary>
        /// Retourne la liste de tous les IDA des acteurs de la hiérarchie avec l'un des Rôles {pRoles}
        /// </summary>
        /// <returns></returns>
        public List<int> FindAllActorID(RoleActor[] pRoles)
        {
            List<int> ret = new List<int>();
            FindAllActorID(this.ChildActors, pRoles, ref ret);
            //
            return ret;
        }
        /// <summary>
        /// Alimente {pList} avec la liste de tous les acteurs contenus dans la liste {pActors} et de tous leurs acteurs descendants avec le Rôle {pRole}
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pRoles"></param>
        /// <param name="pList"></param>
        private void FindAllActorID(List<CBActorNode> pActors, RoleActor[] pRoles, ref List<int> pList)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pList.Contains(actor.Ida) == false)
                {
                    if ((pRoles == null) || (actor.IsExistRole(pRoles)))
                        pList.Add(actor.Ida);
                }
                //
                FindAllActorID(actor.ChildActors, pRoles, ref pList);
            }
        }
        /// <summary>
        /// Retourne la liste de tous les acteurs de la hiérarchie avec le Rôle {pRole}
        /// </summary>
        /// <returns></returns>
        public CBActorNode FindActor(int pIda)
        {
            return FindActor(this.ChildActors, pIda);
        }
        public CBActorNode FindActor(List<CBActorNode> pActors, int pIda)
        {
            CBActorNode retActor = pActors.Find(match => match.Ida == pIda);
            //
            if (retActor != null)
                return retActor;
            //
            foreach (CBActorNode actor in pActors)
            {
                retActor = FindActor(actor.ChildActors, pIda);
                //
                if (retActor != null)
                    return retActor;
            }
            //
            return null;
        }

        /// <summary>
        /// Retourne la liste de tous les acteurs de la hiérarchie avec le Rôle {pRole}
        /// </summary>
        /// <returns></returns>
        public List<CBActorNode> FindActor(RoleActor pRole)
        {
            return FindActor(new RoleActor[] { pRole });
        }
        /// <summary>
        /// Retourne la liste de tous les acteurs de la hiérarchie avec l'un des Rôles {pRoles}
        /// </summary>
        /// <returns></returns>
        public List<CBActorNode> FindActor(RoleActor[] pRoles)
        {
            List<CBActorNode> retListe = new List<CBActorNode>();
            FindActor(this.ChildActors, pRoles, ref retListe);
            //
            return retListe;
        }
        /// <summary>
        /// Alimente {pList} avec la liste de tous les acteurs contenus dans la liste {pActors} et de tous leurs acteurs descendants avec le Rôle {pRole}
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pRoles"></param>
        /// <param name="pList"></param>
        private void FindActor(List<CBActorNode> pActors, RoleActor[] pRoles, ref List<CBActorNode> pList)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pList.Contains(actor) == false)
                {
                    if ((pRoles == null) || (actor.IsExistRole(pRoles)))
                        pList.Add(actor);
                }
                //
                FindActor(actor.ChildActors, pRoles, ref pList);
            }
        }
        /// <summary>
        /// Retourne la liste de tous les IDA des acteurs avec le rôle MarginRequirementOffice de la hiérarchie
        /// <para></para>
        /// </summary>
        /// <returns></returns>
        public List<int> FindMROActor()
        {
            List<int> ret = new List<int>();
            FindMROActor(this.ChildActors, ref ret);
            //
            return ret;
        }
        /// <summary>
        /// Alimente {pList} avec la liste de tous les acteurs contenus dans la liste {pActors} et de tous leurs acteurs descendants avec le rôle MarginRequirementOffice
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pList"></param>
        private void FindMROActor(List<CBActorNode> pActors, ref List<int> pList)
        {
            foreach (CBActorNode actor in pActors)
            {
                if (pList.Contains(actor.Ida) == false)
                    pList.Add(actor.Ida);
                //
                FindMROActor(actor.ChildActors, ref pList);
            }
        }

        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie avec le role {pRole}
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public bool IsExistRole(RoleActor pRole)
        {
            return IsExistRole(this.ChildActors, null, pRole);
        }

        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie avec au moins un role de la liste {pRoles}
        /// </summary>
        /// <param name="pRoles"></param>
        /// <returns></returns>
        public bool IsExistRole(RoleActor[] pRoles)
        {
            return IsExistRole(this.ChildActors, pRoles, RoleActor.NONE);
        }

        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie:
        /// <para>- avec le role {pAddRole}</para>
        /// <para>- et avec au moins un role de la liste {pRoles}</para>
        /// </summary>
        /// <param name="pRoles"></param>
        /// <param name="pAddRole"></param>
        /// <returns></returns>
        public bool IsExistRole(RoleActor[] pRoles, RoleActor pAddRole)
        {
            return IsExistRole(this.ChildActors, pRoles, pAddRole);
        }

        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie {pActors}:
        /// <para>- avec le role {pAddRole}</para>
        /// <para>- et avec au moins un role de la liste {pRoles}</para>
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pRoles"></param>
        /// <param name="pAddRole"></param>
        /// <returns></returns>
        private bool IsExistRole(List<CBActorNode> pActors, RoleActor[] pRoles, RoleActor pAddRole)
        {
            bool ret = false;

            bool isRoles = ArrFunc.IsFilled(pRoles);
            bool isAddRole = (pAddRole != RoleActor.NONE);

            foreach (CBActorNode actor in pActors)
            {
                if (isAddRole)
                    ret = actor.IsExistRole(pAddRole);

                if (isRoles && ((isAddRole == false) || ret))
                    ret = actor.IsExistRole(pRoles);

                if (ret)
                    break;

                ret = IsExistRole(actor.ChildActors, pRoles, pAddRole);

                if (ret)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Filtrer les acteurs de la hiérarchie en gardant que les acteurs avec les rôles: 
        /// <para>- CashBalanceOffice</para>
        /// <para>- MarginRequirementOffice</para>
        /// En sachant que la racine est obligatoirement un acteur avec le rôle: CashBalanceOffice
        /// </summary>
        public void FilterCBOMROActor()
        {
            foreach (CBActorNode actor in this.ChildActors)
                actor.ChildActors = FilterCBOMROActor(actor, actor, null);
        }
        /// <summary>
        /// Filtrer les acteurs descendants de l'acteur {pActorToFilter} en gardant que les acteurs avec les rôles: 
        /// <para>- CashBalanceOffice</para>
        /// <para>- MarginRequirementOffice</para>
        /// </summary>
        /// <param name="pActorToFilter">La racine du sous-arbre à filtrer</param>
        /// <param name="pAncestorFiltredActor">L'acteur ancêtre de {pActorToFilter}, auquel seront ratachés les Books des descendants (CBO et/ou MRO) de {pActorToFilter} si {pActorToFilter} est à supprimer du sous-arbre</param>
        /// <param name="pNewAncestorFiltredActorChilds">La nouvelle collection d'acteurs avec le rôle (CBO et/ou MRO), à laquelle seront ajoutés les descendants (CBO et/ou MRO) de {pActorToFilter} si {pActorToFilter} est à supprimer du sous-arbre</param>
        /// <returns></returns>
        private List<CBActorNode> FilterCBOMROActor(CBActorNode pActorToFilter,
            CBActorNode pAncestorFiltredActor, List<CBActorNode> pNewAncestorFiltredActorChilds)
        {
            List<CBActorNode> filtredChildActors = pNewAncestorFiltredActorChilds;
            //
            if (null == filtredChildActors)
                filtredChildActors = new List<CBActorNode>();
            //
            foreach (CBActorNode actor in pActorToFilter.ChildActors)
            {
                // 1- Si l'acteur est un CBO/MRO
                if (actor.IsCBO || actor.IsMRO)
                {
                    // 1-a- Rajouter l'acteur dans la collection d'Acteurs MRO/CBO
                    CBActorNode retNode = filtredChildActors.Find(match => match.Ida == actor.Ida);
                    //
                    if (retNode != null)
                    {
                        // S'il existe alors rajouter juste la liste des Rôles
                        foreach (ActorRole role in actor.Roles)
                            retNode.Roles.Add(role);
                    }
                    else
                        // Sinon rajouter l'acteur
                        filtredChildActors.Add(actor);

                    // 1-b- Filtrer ses descendants avec le rôle MRO/CBO
                    actor.ChildActors = FilterCBOMROActor(actor, actor, null);
                }
                else
                {
                    // 2- Sinon il est court-circuité:
                    // 2-a- On remonte ses Books vers son Parent
                    foreach (CBBookLeaf book in actor.Books)
                        pAncestorFiltredActor.Add(book);
                    //
                    // 2-b- On remonte tous ses fils vers son Parent
                    FilterCBOMROActor(actor, pAncestorFiltredActor, filtredChildActors);
                }
            }
            //
            return filtredChildActors;
        }

        /// <summary>
        /// Pour indiquer qu'un flux est complétement chargé à partir de la DB
        /// <para>Car pendant le chargement de la DB, si on charge deux flux équivalents, leurs montants seront cumulés</para>
        /// <para>Voir la méthode CBDetFlow.Add(Money pCurrencyAmount)</para>
        /// </summary>
        /// <param name="pAmountType"></param>
        public void SetFlowLoaded(FlowTypeEnum pAmountType)
        {
            foreach (CBActorNode actor in this.ChildActors)
                SetFlowLoaded(actor, pAmountType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pActor"></param>
        /// <param name="pFlowType"></param>
        private void SetFlowLoaded(CBActorNode pActor, FlowTypeEnum pFlowType)
        {
            //            
            foreach (CBActorNode actor in pActor.ChildActors)
                SetFlowLoaded(actor, pFlowType);
            //
            // [TODO RD: Use actor.Books instead of actor.Amounts, after removing member actor.Amounts]
            List<CBDetFlow> flows =
                (from book in pActor.Books
                 from flow in book.Flows
                 where flow.Type == pFlowType
                 select flow).ToList();
            //
            foreach (CBDetFlow flow in flows)
                flow.IsLoaded = true;
        }

        /// <summary>
        /// Tous les flux visibles par cet Actor, à travers tous ces descendants
        /// </summary>
        /// <param name="pActor"></param>
        /// <param name="pMainActor"></param>
        public void GetScopeFlows(CBActorNode pActor, CBActorNode pMainActor)
        {
            List<CBDetFlow> flows = null;
            List<CBFlowTradesSource> tradesSource = null;
            //
            if (null == pMainActor)
                pMainActor = pActor;
            //
            foreach (CBActorNode actor in pActor.ChildActors)
            {
                // Pour un MRO, tous les flux se trouvent au niveau du Noeud (Acteur) lui même
                if (pMainActor.IsMRO)
                {
                    // [TODO RD: Use actor.Books instead of actor.Amounts, after removing member actor.Amounts]
                    flows =
                        (from flow in actor.Flows
                         where (flow.Type == FlowTypeEnum.CashFlows) || (flow.Type == FlowTypeEnum.OtherFlows)
                         select flow).ToList();
                    //
                    // PM 20170208 Add FlowTypeEnum.TradeFlows
                    // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                    tradesSource =
                        (from tradeSource in actor.TradesSource
                         where tradeSource.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                         || tradeSource.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                         || tradeSource.FlowTypes.Contains(FlowTypeEnum.TradeFlows)
                         || tradeSource.FlowTypes.Contains(FlowTypeEnum.AllocNotFungibleFlows)
                         select tradeSource).ToList();
                    //
                    pMainActor.Add(flows);
                    pMainActor.Add(tradesSource);
                    //
                    // Pour un CBO, tous les flux se trouvent au niveau des Noeuds enfants (Premier niveau)
                    if (actor.IsMRO == false)
                        GetScopeFlows(actor, pMainActor);
                }
                else
                {
                    if (actor.IsMRO == false) //???
                    {
                        // [TODO RD: Use actor.Books instead of actor.Amounts, after removing member actor.Amounts]
                        // PM 20170208 Add FlowTypeEnum.TradeFlows
                        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                        flows =
                            (from flow in actor.Flows
                             where (flow.Type == FlowTypeEnum.CashFlows)
                             || (flow.Type == FlowTypeEnum.OtherFlows)
                             || (flow.Type == FlowTypeEnum.TradeFlows)
                             || (flow.Type == FlowTypeEnum.AllocNotFungibleFlows)
                             select flow).ToList();
                        //
                        // PM 20170208 Add FlowTypeEnum.TradeFlows
                        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                        tradesSource =
                            (from tradeSource in actor.TradesSource
                             where tradeSource.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                             || tradeSource.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                             || tradeSource.FlowTypes.Contains(FlowTypeEnum.TradeFlows)
                             || tradeSource.FlowTypes.Contains(FlowTypeEnum.AllocNotFungibleFlows)
                             select tradeSource).ToList();
                        //
                        pMainActor.Add(flows);
                        pMainActor.Add(tradesSource);
                    }
                }
            }
        }

        /// <summary>
        /// Cumul les Cash Flows sur tous les Noeuds de la hiérarchie
        /// </summary>
        public void CumulChildFlows(FlowTypeEnum pAmountType)
        {
            foreach (CBActorNode actor in this.ChildActors)
                CumulChildFlows(actor, pAmountType);
        }
        /// <summary>
        /// Cumul les Flux sur le noeud {pActor} et tous ses noueds descendants.
        /// </summary>
        /// <param name="pActor">Le noeud pour lequel les Cash Flows seront cumulés</param>
        /// <param name="pAmountType">Type de montant</param>
        private void CumulChildFlows(CBActorNode pActor, FlowTypeEnum pAmountType)
        {
            foreach (CBActorNode actor in pActor.ChildActors)
                CumulChildFlows(actor, pAmountType);
            //
            // Le Déposit ne concerne que les Acteurs MRO
            if ((pAmountType != FlowTypeEnum.Deposit) || (pActor.IsMRO))
            {
                // [TODO RD: Use actor.Books instead of actor.Amounts, after removing member actor.Amounts]
                List<CBDetFlow> flows =
                    (from book in pActor.Books
                     from flow in book.Flows
                     where (flow.Type == pAmountType)
                     select flow).ToList();

                List<CBFlowTradesSource> tradesSource =
                    (from book in pActor.Books
                     from source in book.TradesSource
                     where (source.FlowTypes.Contains(pAmountType))
                     select source).ToList();
                //
                pActor.Add(flows);
                pActor.Add(tradesSource);
            }
            //
            // PM 20170208 Add FlowTypeEnum.TradeFlows
            // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
            if ((pAmountType == FlowTypeEnum.CashFlows)
                || (pAmountType == FlowTypeEnum.OtherFlows)
                || (pAmountType == FlowTypeEnum.TradeFlows)
                || (pAmountType == FlowTypeEnum.AllocNotFungibleFlows))
            {
                GetScopeFlows(pActor, null);
            }
        }

        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie {pActors}
        /// utilisant la méthode de calcul du cash balance {pCBMethod}
        /// </summary>
        /// <param name="pCBMethod"></param>
        /// <returns></returns>
        public bool IsExistActorWithCBMethod(CashBalanceCalculationMethodEnum pCBMethod)
        {
            bool ret = IsExistActorWithCBMethod(ChildActors, pCBMethod);
            return ret;
        }
        /// <summary>
        /// Retourne true s'il existe au moins un acteur dans la hiéararchie {pActors}
        /// utilisant la méthode de calcul du cash balance {pCBMethod}
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pCBMethod"></param>
        /// <returns></returns>
        public static bool IsExistActorWithCBMethod(IEnumerable<CBActorNode> pActors, CashBalanceCalculationMethodEnum pCBMethod)
        {
            bool ret = false;
            if ((null != pActors) && (pActors.Count() > 0))
            {
                ret = pActors.Any(a => (a.BusinessAttribute != null) && (a.BusinessAttribute.CbCalcMethod == pCBMethod));
                if (false == ret)
                {
                    // Ensemble des acteurs enfant de tous les acteur de {pActors}
                    IEnumerable<CBActorNode> actorsChild =
                        from actor in pActors
                        where (actor.ChildActors != null)
                        from child in actor.ChildActors
                        select child;
                    //
                    ret = IsExistActorWithCBMethod(actorsChild, pCBMethod);
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la restriction SQL qui permet de selectionner des flux, trades, etc. en rapport avec les CSS\CUSTODIAN valides ou non valides
        ///  <para>un CSS\CUSTODIAN est dit "valide" si le traitement de fin de journée est en succès ou warning</para>
        ///  <para>un CSS\CUSTODIAN est dit "non valide" si le traitement de fin de journée est en erreur ou non encore exécuté</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAliasTradeInstrument">pAlias de la table ou vue contenant la colonne IDA_CSSCUSTODIAN (généralement le table TRADEINSTRUMENT)</param>
        /// <param name="pIsValid"></param>
        /// <param name="pOperator">or ou and ou string.Empty</param>
        /// <returns></returns>
        /// FI 20161021 [22152] Add
        /// FI 20170208 [21916] Modify (Methode Public, add  pOperator)
        /// FI 20170208 [22151][22152] Obsolete => Méthode non utilisée
        /// FI 20170208 [22151][22152] Obsolete => Méthode non utilisée
        public string SQLRestrictCssCustodianValidOrUnValid(string pCS, string pAliasTradeInstrument, bool pIsValid, string pOperator)
        {
            if (false == ((pOperator == SQLCst.AND) || (pOperator == SQLCst.OR) || StrFunc.IsEmpty(pOperator)))
                throw new ArgumentException("pOperator value (:{0})  is unvalid", pOperator);

            string ret = string.Empty;

            Boolean isExist = pIsValid ? cssCustodianEODValid.Count > 0 : cssCustodianEODUnValid.Count > 0;

            if (isExist)
            {
                ICollection col = pIsValid ? cssCustodianEODValid.Select(x => x.OTCmlId).ToArray() : cssCustodianEODUnValid.Select(x => x.OTCmlId).ToArray();
                //ret = StrFunc.AppendFormat(" and ({0}.IDA_CSSCUSTODIAN {1})", pAliasTradeInstrument, DataHelper.SQLColumnIn(pCS, "{0}.IDA_CSSCUSTODIAN", col, TypeData.TypeDataEnum.integer));
                ret = StrFunc.AppendFormat(" {0} ({1})", pOperator, DataHelper.SQLColumnIn(pCS, StrFunc.AppendFormat("{0}.IDA_CSSCUSTODIAN", pAliasTradeInstrument), col, TypeData.TypeDataEnum.integer));
            }
            else
            {
                ret = StrFunc.AppendFormat(" {0} (1=0)", pOperator);
            }

            return ret;
        }
    }

    /// <summary>
    /// Représente un Noeud Actor dans la structure arborescente
    /// Le même Actor peut exister sur plusieurs niveaux
    /// </summary>
    [XmlRoot(ElementName = "actor")]
    public class CBActorNode : IComparable
    {
        /// <summary>
        /// Id de l'acteur
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int Ida;

        /// <summary>
        /// Identifier de l'acteur
        /// </summary>
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;

        /// <summary>
        /// Niveau auquel l'acteur se situe dans l'arbre
        /// NB: le même acteur peut se retrouver à plusieurs niveaux
        /// </summary>
        [XmlAttribute(AttributeName = "level")]
        public int Level;

        List<ActorRole> m_Roles;
        /// <summary>
        /// Les rôles de l'acteur
        /// </summary>
        [XmlArray(ElementName = "roles")]
        [XmlArrayItemAttribute("role")]
        public List<ActorRole> Roles
        {
            get { return m_Roles; }
            set { m_Roles = value; }
        }

        /// <summary>
        /// Las caractéristiques d'un Acteur CBO ou MRO
        /// </summary>
        [XmlElement(ElementName = "businessAttribute")]
        public CBBusinessAttribute BusinessAttribute;

        /// <summary>
        /// 
        /// </summary>
        List<CBActorNode> m_ChildActors;
        /// <summary>
        /// Les acteurs enfants
        /// </summary>        
        [XmlArray(ElementName = "childActors")]
        [XmlArrayItemAttribute("actor")]
        public List<CBActorNode> ChildActors
        {
            get { return m_ChildActors; }
            set { m_ChildActors = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        List<CBBookLeaf> m_Books;
        /// <summary>
        /// Les books ratachés à l'acteur        
        /// </summary>        
        [XmlArray(ElementName = "books")]
        [XmlArrayItemAttribute("book")]
        public List<CBBookLeaf> Books
        {
            get { return m_Books; }
            set { m_Books = value; }
        }

        List<CBDetFlow> m_Flows;
        List<CBTradeInfo> m_Trades;
        List<CBFlowTradesSource> m_TradesSource;

        [XmlIgnore]
        public bool FlowsSpecified;
        /// <summary>
        /// La liste des flux cumulés à partir des montants existants sur les Books enfants
        /// </summary>        
        [XmlArray(ElementName = "flows")]
        [XmlArrayItemAttribute("flow")]
        public List<CBDetFlow> Flows
        {
            get { return m_Flows; }
            set { m_Flows = value; }
        }

        /// <summary>
        /// Collection des différents trades sources des différents flux
        /// </summary>
        [XmlArray(ElementName = "tradesSource")]
        [XmlArrayItemAttribute("trade")]
        public List<CBFlowTradesSource> TradesSource
        {
            get { return m_TradesSource; }
            set { m_TradesSource = value; }
        }

        /// <summary>
        /// La liste des Trades Cash-Balance générés pour cet Acteur
        /// </summary>           
        [XmlArray(ElementName = "trades")]
        [XmlArrayItemAttribute("trade")]
        public List<CBTradeInfo> Trades
        {
            get { return m_Trades; }
            set { m_Trades = value; }
        }

        /// <summary>
        /// Obtient true si l'acteur a le rôle CLEARER ou CLEARINGCOMPART
        /// </summary>
        [XmlIgnore]
        public bool IsClearer
        {
            get
            {
                return (IsExistRole(new RoleActor[] { RoleActor.CLEARER, RoleActor.CCLEARINGCOMPART, RoleActor.HCLEARINGCOMPART, RoleActor.MCLEARINGCOMPART }));
            }
        }

        /// <summary>
        /// Actor CBO
        /// </summary>
        [XmlIgnore]
        public bool IsCBO
        {
            get
            {
                return ((BusinessAttribute != null) && BusinessAttribute.IsCBO);
            }
        }
        /// <summary>
        /// Actor non CBO
        /// </summary>
        [XmlIgnore]
        public bool IsNotCBO
        {
            get
            {
                return (IsCBO == false);
            }
        }
        /// <summary>
        /// Actor CBO avec IMSCOPE = GLOBAL ou FULL et un Book spécifié
        /// </summary>
        [XmlIgnore]
        public bool IsCBOGlobalScopWithBook
        {
            get
            {
                return ((IsCBOGlobalScope && BusinessAttribute.IdB_CBO > 0));
            }
        }
        /// <summary>
        /// Actor CBO avec IMSCOPE = GLOBAL ou FULL
        /// </summary>
        [XmlIgnore]
        public bool IsCBOGlobalScope
        {
            get
            {
                return (IsCBO &&
                    ((BusinessAttribute.Scope_CBO == GlobalElementaryEnum.Global) ||
                    (BusinessAttribute.Scope_CBO == GlobalElementaryEnum.Full)));
            }
        }
        /// <summary>
        /// Actor CBO avec IMSCOPE != GLOBAL et IMSCOPE != FULL
        /// </summary>
        [XmlIgnore]
        public bool IsCBONotGlobalScope
        {
            get
            {
                return (IsCBO &&
                    ((BusinessAttribute.Scope_CBO != GlobalElementaryEnum.Global) &&
                    (BusinessAttribute.Scope_CBO != GlobalElementaryEnum.Full)));
            }
        }

        /// <summary>
        /// Actor CBO avec IMSCOPE != GLOBAL et IMSCOPE != FULL 
        /// <para>ou bien</para>
        /// Actor CBO avec IMSCOPE = GLOBAL ou FULL et le Book NON spécifié
        /// </summary>
        [XmlIgnore]
        public bool IsCBONotGlobalScopOrWithoutBook
        {
            get
            {
                return (IsCBONotGlobalScope || (IsCBOGlobalScope && BusinessAttribute.IdB_CBO == 0));
            }
        }
        /// <summary>
        /// Actor CBO avec IMSCOPE = Elementary ou Full
        /// </summary>
        [XmlIgnore]
        public bool IsCBOElementaryScope
        {
            get
            {
                return (IsCBO &&
                    ((BusinessAttribute.Scope_CBO == GlobalElementaryEnum.Elementary) ||
                    (BusinessAttribute.Scope_CBO == GlobalElementaryEnum.Full)));
            }
        }

        /// <summary>
        /// Actor CBO avec IMSCOPE != Elementary et IMSCOPE != Full
        /// </summary>
        [XmlIgnore]
        public bool IsCBONotElementaryScope
        {
            get
            {
                return (IsCBO &&
                    ((BusinessAttribute.Scope_CBO != GlobalElementaryEnum.Elementary) &&
                    (BusinessAttribute.Scope_CBO != GlobalElementaryEnum.Full)));
            }
        }

        /// <summary>
        /// Actor CBO avec CashAndCollateral=CBO
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithCashCollatCBO
        {
            get
            {
                return ((IsCBO && BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.CBO));
            }
        }
        /// <summary>
        /// Actor CBO avec CashAndCollateral=CBOChild
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithCashCollatCBOChild
        {
            get
            {
                return ((IsCBO && BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.CBOChild));
            }
        }
        /// <summary>
        /// Actor CBO avec CashAndCollateral=MROChild
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithCashCollatMROChild
        {
            get
            {
                return ((IsCBO && BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.MROChild));
            }
        }

        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCOLLATCUR (Déposit et couverture en devise)
        /// <para>(équivaut au Calcul MonoDevise dans Eurosys)</para>
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithMGCCollatCur
        {
            get
            {
                return ((IsCBO && BusinessAttribute.MgcCalcMethod == MarginCallCalculationMethodEnum.MGCCOLLATCUR));
            }
        }

        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCTRVAL (Déposit et couverture en contrevaleur)
        /// <para>(équivaut au Calcul MultiDevise dans Eurosys)</para>
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithMGCCTRVal
        {
            get
            {
                return ((IsCBO && BusinessAttribute.MgcCalcMethod == MarginCallCalculationMethodEnum.MGCCTRVAL));
            }
        }

        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCOLLATCTRVAL ((Déposit en devise et couverture en contrevaleur)
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithMGCCollatCTRVal
        {
            get
            {
                return ((IsCBO && BusinessAttribute.MgcCalcMethod == MarginCallCalculationMethodEnum.MGCCOLLATCTRVAL));
            }
        }

        /// <summary>
        /// Actor CBO avec ExchangeType=FCU
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithFCU
        {
            get
            {
                return ((IsCBO && BusinessAttribute.ExchangeType == CBExchangeTypeEnum.FCU));
            }
        }

        /// <summary>
        /// Actor CBO avec ExchangeType=ACU_BUSINESSDATE
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithACU_BUSINESSDATE
        {
            get
            {
                return ((IsCBO && BusinessAttribute.ExchangeType == CBExchangeTypeEnum.ACU_BUSINESSDATE));
            }
        }

        /// <summary>
        /// Actor CBO avec IsUseAvailableCash=True
        /// </summary>
        [XmlIgnore]
        public bool IsCBOWithUseAvailableCash
        {
            get
            {
                return ((IsCBO && BusinessAttribute.IsUseAvailableCash));
            }
        }
        /// <summary>
        /// Actor MRO
        /// </summary>
        [XmlIgnore]
        public bool IsMRO
        {
            get
            {
                return (BusinessAttribute != null && BusinessAttribute.IsMRO);
            }
        }
        /// <summary>
        /// Actor non MRO
        /// </summary>
        [XmlIgnore]
        public bool IsNotMRO
        {
            get
            {
                return (IsMRO == false);
            }
        }
        /// <summary>
        /// Actor MRO avec IMSCOPE = GLOBAL ou FULL et un Book spécifié
        /// </summary>
        [XmlIgnore]
        public bool IsMROGlobalScopWithBook
        {
            get
            {
                return ((IsMROGlobalScope && BusinessAttribute.IdB_MRO > 0));
            }
        }
        /// <summary>
        /// Actor MRO avec IMSCOPE != GLOBAL et IMSCOPE != FULL 
        /// <para>ou bien</para>
        /// Actor MRO avec IMSCOPE = GLOBAL ou FULL et le Book NON spécifié
        /// </summary>
        [XmlIgnore]
        public bool IsMRONotGlobalScopOrWithoutBook
        {
            get
            {
                return (IsMRONotGlobalScope || (IsMROGlobalScope && BusinessAttribute.IdB_MRO == 0));
            }
        }
        /// <summary>
        /// Actor MRO avec IMSCOPE = GLOBAL ou FULL
        /// </summary>
        [XmlIgnore]
        public bool IsMROGlobalScope
        {
            get
            {
                return (IsMRO &&
                    ((BusinessAttribute.Scope_MRO == EfsML.Enum.GlobalElementaryEnum.Global) ||
                    (BusinessAttribute.Scope_MRO == EfsML.Enum.GlobalElementaryEnum.Full)));
            }
        }
        /// <summary>
        /// Actor MRO avec IMSCOPE != GLOBAL et IMSCOPE != FULL
        /// </summary>
        [XmlIgnore]
        public bool IsMRONotGlobalScope
        {
            get
            {
                return (IsMRO &&
                    ((BusinessAttribute.Scope_MRO != EfsML.Enum.GlobalElementaryEnum.Global) &&
                    (BusinessAttribute.Scope_MRO != EfsML.Enum.GlobalElementaryEnum.Full)));
            }
        }

        /// <summary>
        /// Actor MRO avec IMSCOPE = Elementary ou Full
        /// </summary>
        [XmlIgnore]
        public bool IsMROElementaryScope
        {
            get
            {
                return (IsMRO &&
                    ((BusinessAttribute.Scope_MRO == EfsML.Enum.GlobalElementaryEnum.Elementary) ||
                    (BusinessAttribute.Scope_MRO == EfsML.Enum.GlobalElementaryEnum.Full)));
            }
        }
        /// <summary>
        /// Actor MRO avec IMSCOPE != Elementary et IMSCOPE != Full
        /// </summary>
        [XmlIgnore]
        public bool IsMRONotElementaryScope
        {
            get
            {
                return (IsMRO &&
                    ((BusinessAttribute.Scope_MRO != EfsML.Enum.GlobalElementaryEnum.Elementary) &&
                    (BusinessAttribute.Scope_MRO != EfsML.Enum.GlobalElementaryEnum.Full)));
            }
        }
        
        /// <summary>
        ///  Retourne les flux avec status = {pStatus}
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        /// FI 20170208 [22151][22152] Add
        public IEnumerable<CBDetFlow> GetFlows(StatusEnum pStatus)
        {
            return this.Flows.Where(x => x.Status == pStatus);
        }


        /// <summary>
        /// 
        /// </summary>
        public CBActorNode()
            : base()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pLevel"></param>
        /// <param name="pIdRoleActor"></param>
        /// <param name="pIdA_Actor"></param>
        public CBActorNode(int pIdA, string pIdentifier, int pLevel, RoleActor pIdRoleActor, int pIdA_Actor)
        {
            Ida = pIdA;
            Identifier = pIdentifier;
            Level = pLevel;
            //
            m_Roles = new List<ActorRole>();
            m_Books = new List<CBBookLeaf>();
            m_Flows = new List<CBDetFlow>();
            m_TradesSource = new List<CBFlowTradesSource>();
            m_Trades = new List<CBTradeInfo>();
            m_ChildActors = new List<CBActorNode>();
            //
            Add(new ActorRole(pIdA, pIdRoleActor, pIdA_Actor));
        }

        /// <summary>
        /// Mise à jour du niveau
        /// </summary>
        /// <param name="pLevel"></param>
        public void UpdateLevel(int pLevel)
        {
            this.Level = pLevel;
            //
            foreach (CBActorNode child in this.ChildActors)
                child.UpdateLevel(pLevel + 1);
        }

        /// <summary>
        /// Comparer avec un objet {pObj} , l'égalité est contatée si:
        /// <para>- Même Ida</para>
        /// <para>- Même Niveau</para>
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            if (pObj is CBActorNode nodeActor)
            {
                int ret = 0;
                //
                if (nodeActor.Ida != Ida)
                    ret = -1;
                else if (nodeActor.Level != Level)
                    ret = -1;
                //
                return ret;
            }
            throw new ArgumentException("object is not a CBActorNode");
        }

        /// <summary>
        /// Retourne un objet ActorRole existant dans la collection des rôles
        /// </summary>
        /// <param name="pActorRole"></param>
        /// <returns></returns>
        public ActorRole this[ActorRole pActorRole]
        {
            get { return this.Roles.Find(match => match.CompareTo(pActorRole) == 0); }
        }

        /// <summary>
        /// Ajoute un Rôle s'il n'existe pas dans la collection des rôles
        /// </summary>
        /// <param name="pActorRole"></param>
        public ActorRole Add(ActorRole pActorRole)
        {
            ActorRole retActorRole = pActorRole;
            //
            if (IsExistRole(pActorRole))
                retActorRole = this[pActorRole];
            else
                this.Roles.Add(pActorRole);
            //
            return retActorRole;
        }
        /// <summary>
        /// Ajoute un Book {pBook} à la collection des Books:
        /// <para>- Si le Book n'existe pas, alors il est directement ajouté à la collection des Books</para>
        /// <para>- Sinon la liste des Montants sera rajouté au Book existant</para>        
        /// </summary>
        /// <param name="pBook"></param>
        public void Add(CBBookLeaf pBook)
        {
            bool amountAdded = false;
            Add(pBook, ref amountAdded);
        }
        /// <summary>
        /// Ajoute un Book {pBook} à la collection des Books:
        /// <para>- Si le Book n'existe pas, alors il est directement ajouté à la collection des Books</para>
        /// <para>- Sinon, si les montants ne sont pas déjà rajoutés {pAmountAdded} alors:</para>
        /// <para>la liste des Montants sera rajoutée au Book existant</para>
        /// <para>NB: Un Book est unique dans la structure, donc on ne rajoute pas deux fois le même montant au même Book</para>
        /// </summary>
        /// <param name="pBook"></param>
        /// <param name="pAmountAdded"></param>
        public void Add(CBBookLeaf pBook, ref bool pAmountAdded)
        {
            CBBookLeaf book = this.Books.Find(match => match.CompareTo(pBook) == 0);
            //
            if (null == book)
                this.Books.Add(pBook);
            else
            {
                if (pAmountAdded == false)
                {
                    foreach (CBDetFlow flow in pBook.Flows)
                        book.Add(flow);
                    //
                    pAmountAdded = true;
                }
            }
        }
        /// <summary>
        /// Ajoute un Trade {pTrade} au Book concerné de la collection des Books
        /// </summary>
        /// <param name="pTrade"></param>
        public void Add(CBBookTrade pTrade)
        {
            CBBookLeaf book = this.Books.Find(match => match.CompareTo(pTrade.IDB, pTrade.IDA) == 0);
            if (null != book)
                book.Add(pTrade);
        }
        /// <summary>
        /// Ajoute un Trade {pTradeInfo} à la collection des Trades
        /// </summary>
        /// <param name="pTradeInfo"></param>
        public void Add(CBTradeInfo pTradeInfo)
        {
            this.Trades.Add(pTradeInfo);
        }

        /// <summary>
        /// Ajoute un flux {pFlow} à la collection des flux:
        /// <para>- Si le Type de flux n'existe pas, alors il est directement ajouter à la collection des flux</para>
        /// <para>- Sinon la liste des CurrencyAmount sera rajouté au flux existant</para>
        /// </summary>
        /// <param name="pFlow"></param>
        public void Add(CBDetFlow pFlow)
        {
            CBDetFlow flow = this.Flows.Find(match => match.CompareTo(pFlow) == 0);
            //
            if (null == flow)
            {
                this.Flows.Add(pFlow);
            }
            else
            {
                foreach (Money money in pFlow.CurrencyAmount)
                    flow.Add(money);
            }
        }
        /// <summary>
        /// Ajoute une collection de flux {pFlows} à la collection des flux:
        /// </summary>
        /// <param name="pFlows"></param>
        public void Add(List<CBDetFlow> pFlows)
        {
            foreach (CBDetFlow flow in pFlows)
                Add(flow);
        }

        /// <summary>
        /// Ajoute une collection de trades sources {pTradesSource} à la collection existantes de trades sources.
        /// </summary>
        /// <param name="pTradesSource"></param>
        public void Add(List<CBFlowTradesSource> pTradesSource)
        {
            foreach (CBFlowTradesSource source in pTradesSource)
                this.TradesSource.Add(source);
        }

        /// <summary>
        /// True si le rôle {pActorRole} existe
        /// </summary>
        /// <param name="pActorRole"></param>
        /// <returns></returns>
        public bool IsExistRole(ActorRole pActorRole)
        {
            bool ret = false;
            //
            if (this.Roles.Find(
                match => match.CompareTo(pActorRole) == 0) != null)
            {
                ret = true;
            }
            //
            return ret;
        }
        /// <summary>
        /// True si le rôle {pRole} existe
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public bool IsExistRole(RoleActor pRole)
        {
            bool ret = false;
            //
            if (this.Roles.Find(
                match => match.Role == pRole) != null)
            {
                ret = true;
            }
            //
            return ret;
        }
        /// <summary>
        /// True si les rôles {pRoles} existent
        /// </summary>
        /// <param name="pRoles"></param>
        /// <returns></returns>
        public bool IsExistRole(RoleActor[] pRoles)
        {
            bool ret = false;
            //
            foreach (RoleActor role in pRoles)
            {
                ret = IsExistRole(role);
                //
                if (ret == true)
                    break;
            }
            //
            return ret;
        }
        /// <summary>
        /// True si un rôle existe vis-à-vis de l'acteur {pIDA_ACTOR} 
        /// </summary>
        /// <param name="pIDA_ACTOR"></param>
        /// <returns></returns>
        public bool IsExistRole(int pIDA_ACTOR)
        {
            bool ret = false;
            //
            if (this.Roles.Find(
                match => match.IdA_Actor == pIDA_ACTOR) != null)
            {
                ret = true;
            }
            //
            return ret;
        }
    }

    /// <summary>
    /// Représente une Feuille Book dans la structure arborescente, 
    /// elle porte les différents montants chargés à partir de la BD, pour un couple Acteur/Book
    /// Un Book est unique dans la structure quelque soit le niveau
    /// </summary>
    [XmlRoot(ElementName = "Book")]
    public class CBBookLeaf : IComparable
    {
        /// <summary>
        /// C'est selon la nature du flux (donc du source) sur lequel apparait le Book:
        /// <para>- exchangeTradedDerivative: C'est le propriétaire du Book</para>
        /// <para>- marginRequirement: C'est le MRO</para>
        /// <para>- cashBalance: C'est le CBO</para>
        /// <para>- bulletPayment: C'est le MRO/CBO qui paye/reçoit le montant du mouvement espèces</para>
        /// Pour le cas des Dépôts de garantie, c'est le Déposant (pour un Dealer) ou le Dépositaire (pour un Clearer)
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int Ida;

        /// <summary>
        /// Le Book payeur ou receveur des différents montants
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int Idb;

        /// <summary>
        /// L'identifier du Book payeur ou receveur des différents montants
        /// </summary>
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;

        /// <summary>
        /// Collection des différents flux
        /// </summary>
        [XmlArray(ElementName = "flows")]
        [XmlArrayItemAttribute("flow")]
        public List<CBDetFlow> Flows;

        /// <summary>
        /// Collection des différents m_Trades sources des différents flux sur lesquels apparait le Book
        /// </summary>
        [XmlArray(ElementName = "tradesSource")]
        [XmlArrayItemAttribute("trade")]
        public List<CBFlowTradesSource> TradesSource;

        /// <summary>
        /// 
        /// </summary>
        public CBBookLeaf() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdaOwner"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier"></param>
        public CBBookLeaf(int pIdaOwner, int pIdb, string pIdentifier)
        {
            Ida = pIdaOwner;
            Idb = pIdb;
            Identifier = pIdentifier;

            List<FlowTypeEnum> dailyFlow = new List<FlowTypeEnum>
            {
                FlowTypeEnum.CashFlows,
                FlowTypeEnum.OtherFlows
            };

            List<FlowTypeEnum> depositFlow = new List<FlowTypeEnum>
            {
                FlowTypeEnum.Deposit
            };

            List<FlowTypeEnum> paymentFlow = new List<FlowTypeEnum>
            {
                FlowTypeEnum.Payment,
                //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                FlowTypeEnum.SettlementPayment
            };

            TradesSource = new List<CBFlowTradesSource>
            {
                new CBFlowTradesSource(Idb, Ida, dailyFlow),
                new CBFlowTradesSource(Idb, Ida, depositFlow),
                new CBFlowTradesSource(Idb, Ida, paymentFlow)
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdaOwner"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pFlow"></param>
        public CBBookLeaf(int pIdaOwner, int pIdb, string pIdentifier, CBDetFlow pFlow)
            : this(pIdaOwner, pIdb, pIdentifier)
        {
            Flows = new List<CBDetFlow>
            {
                pFlow
            };
        }

        /// <summary>
        /// Comparer avec un objet {pObj} , l'égalité est constatée si:
        /// <para>- Même Book</para>
        /// <para>- Même Acteur</para>
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            if (pObj is CBBookLeaf leafBook)
                return CompareTo(leafBook.Idb, leafBook.Ida);

            throw new ArgumentException("object is not a CBBookLeaf");
        }
        /// <summary>
        /// Comparer avec le couple ({pIdb},{pIdaOwner}), l'égalité est constatée si:
        /// <para>- Même Book</para>
        /// <para>- Même Acteur</para>
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIdaOwner"></param>
        /// <returns></returns>
        public int CompareTo(int pIdb, int pIdaOwner)
        {
            int ret = 0;
            //
            if (pIdb != Idb)
                ret = -1;
            else if (pIdaOwner != Ida)
                ret = -1;
            //
            return ret;
        }

        /// <summary>
        /// Ajoute un flux {pFlow} à la collection des flux:
        /// <para>- Si le Type de flux n'existe pas, alors il est directement ajouter à la collection des flux</para>
        /// <para>- Sinon la liste des CurrencyAmount sera rajouté au flux existant</para>
        /// </summary>
        /// <param name="pFlow"></param>
        public void Add(CBDetFlow pFlow)
        {
            CBDetFlow flow = this.Flows.Find(match => match.CompareTo(pFlow) == 0);
            //
            if (null == flow)
                this.Flows.Add(pFlow);
            else
            {
                foreach (Money money in pFlow.CurrencyAmount)
                    flow.Add(money);
            }
        }

        /// <summary>
        /// Ajoute un source {pTrade}, s'il n'existe pas, à la collection des trades sources (m_TradesSource)
        /// </summary>
        /// <param name="pTrade"></param>
        /// FI 20170208 [22151][22152] Modify
        /// FI 20170316 [22950] Modify
        public void Add(CBBookTrade pTrade)
        {
            // FI 20170208 [22151][22152] Modify
            //CBFlowTradesSource source = this.TradesSource.Find(match => (match.FlowTypes.Contains(pTrade.FlowType)));
            //if ((source != null) && (source.Trades.Exists(trade => trade.First == pTrade.IDT) == false))
            //    source.Trades.Add(new Pair<int, string>(pTrade.IDT, pTrade.Identifier));
            // FI 20170316 [22950] Alimentation de dtBusiness
            CBFlowTradesSource source = this.TradesSource.Find(match => (match.FlowTypes.Contains(pTrade.FlowType)));
            if ((source != null) && (source.Trades.Exists(x => x.IdT == pTrade.IDT) == false))
                source.Trades.Add(new CBTrade(pTrade.IDT, pTrade.Identifier, pTrade.Status, pTrade.DtBusiness));
        }
    }

    /// <summary>
    /// Le role d'un acteur
    /// </summary>
    [XmlRoot(ElementName = "Role")]
    public class CBActorRole : IComparable
    {
        /// <summary>
        /// Acteur 
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IdA;

        /// <summary>
        /// Rôle de l'acteur via à vis de l'acteur parent
        /// </summary>
        [XmlAttribute(AttributeName = "role")]
        public RoleActor Role;

        /// <summary>
        /// L'acteur parent (Lorsque le rôle est attribué vis à vis d'un acteur parent)
        /// </summary>
        [XmlAttribute(AttributeName = "idaActor")]
        public int IdA_Actor;

        public CBActorRole() { }
        public CBActorRole(int pIdA, RoleActor pRole, int pIdA_Actor)
        {
            // m_ida est Role de m_IdA_Actor  
            // - Ex m_ida est CLIENT de m_IdA_Actor
            // - Ex m_ida est ENTITY de m_IdA_Actor						
            //
            IdA = pIdA;
            Role = pRole;
            IdA_Actor = pIdA_Actor;
        }

        public CBActorRole(int pIdA, RoleActor pRole)
            : this(pIdA, pRole, 0) { }
        /// <summary>
        /// Comparer avec un objet {pObj} , l'égalité est contatée si:
        /// <para>- Même Acteur</para>
        /// <para>- Même Rôle</para>
        /// <para>- Même Acteur parent</para>
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            if (pObj is CBActorRole actorRole)
            {
                int ret = 0;
                if (actorRole.IdA != IdA)
                    ret = -1;
                else if (actorRole.Role != Role)
                    ret = -1;
                else if (actorRole.IdA_Actor != IdA_Actor)
                    ret = -1;
                //
                return ret;
            }
            throw new ArgumentException("object is not a CBActorRole");
        }
    }

    /// <summary>
    /// Classe portant les détails de chaque source duquel sont issus des flux manipulés dans le Cash-Balance. 
    /// Il peut s'agir des m_Trades suivants:
    /// <para>- exchangeTradedDerivative: Allocation ETD, pour les flux de Cash-Flows</para>
    /// <para>- marginRequirement: Encours de déposit</para>
    /// <para>- bulletPayment: Cash Payment</para>
    /// </summary>
    /// FI 20170208 [22151][22152] Modify
    /// FI 20170316 [22950] Modify 
    [XmlRoot(ElementName = "FlowTrade")]
    public class CBBookTrade : IComparable
    {
        int m_IDB;
        int m_IDA;
        int m_IDT;
        string m_Identifier;
        FlowTypeEnum m_FlowType;
        // FI 20170208 [22151][22152] add m_status
        StatusEnum m_status;
        // FI 20170316 [22950] add
        DateTime m_dtBusiness;

        /// <summary>
        /// Book apparaissant sur le source
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int IDB
        {
            get { return m_IDB; }
            set { m_IDB = value; }
        }

        /// <summary>
        /// C'est selon le cas:
        /// <para>- exchangeTradedDerivative: C'est le propriétaire du Book</para>
        /// <para>- marginRequirement: C'est le MRO</para>
        /// <para>- bulletPayment: C'est le MRO/CBO qui paye/reçoit le montant du mouvement espèces</para>
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IDA
        {
            get { return m_IDA; }
            set { m_IDA = value; }
        }

        /// <summary>
        /// L'Id du Trade
        /// </summary>
        [XmlAttribute(AttributeName = "idt")]
        public int IDT
        {
            get { return m_IDT; }
            set { m_IDT = value; }
        }

        /// <summary>
        /// L'Id du Trade
        /// </summary>
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier
        {
            get { return m_Identifier; }
            set { m_Identifier = value; }
        }

        /// <summary>
        /// Le type de flux
        /// </summary>
        [XmlAttribute(AttributeName = "flow")]
        public FlowTypeEnum FlowType
        {
            get { return m_FlowType; }
            set { m_FlowType = value; }
        }

        /// <summary>
        /// status 
        /// </summary>
        /// FI 20170208 [22151][22152] Add
        [XmlAttribute(AttributeName = "status")]
        public StatusEnum Status
        {
            get { return m_status; }
            set { m_status = value; }
        }


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public bool DtBusinessSpecified;
        /// <summary>
        /// dtBusiness
        /// </summary>
        /// FI 20170316 [22950] add
        [XmlAttribute(AttributeName = "dtBusiness")]
        public DateTime DtBusiness
        {
            get { return m_dtBusiness; }
            set { m_dtBusiness = value; }
        }



        /// <summary>
        /// Constructor par défaut (nécessaire à la serialization)
        /// </summary>
        public CBBookTrade() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pIdt"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pFlowType"></param>
        /// <param name="pStatus"></param>
        /// <param name="pDtBusiness"></param>
        /// FI 20170316 [22950] Modify 
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public CBBookTrade(int pIdb, int pIda, int pIdt, string pIdentifier, FlowTypeEnum pFlowType, StatusEnum pStatus, DateTime pDtBusiness)
        {
            m_IDB = pIdb;
            m_IDA = pIda;
            m_IDT = pIdt;
            m_Identifier = pIdentifier;
            m_FlowType = pFlowType;
            m_status = pStatus;
            // FI 20170316 [22950] Alimentation de dtBusiness
            DtBusinessSpecified = DtFunc.IsDateTimeFilled(pDtBusiness);  
            m_dtBusiness = pDtBusiness;
        }

        /// <summary>
        /// Comparer le Trade courant à {pObj}, selon les critères suivants:
        /// <para>IDT</para>
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBBookTrade courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        public virtual int CompareTo(object pObj)
        {
            if (pObj is CBBookTrade trade)
            {
                int ret = 0;
                //
                if (trade.IDT != IDT)
                    ret = -1;
                else if (trade.IDB != IDB)
                    ret = -1;
                else if (trade.IDA != IDA)
                    ret = -1;
                //
                return ret;
            }
            //
            throw new ArgumentException("object is not a CBTradeFlow");
        }
    }

    /// <summary>
    /// Les attributs Business de l'acteur, qui peut être un MRO ou bien un CBO
    /// </summary>
    /// PM 20140901 [20066][20185] Add CashBalanceMethod et CashBalanceIDC
    /// PM 20140901 [20066][20185] Modify constructor
    [XmlRoot(ElementName = "Attributes")]
    public class CBBusinessAttribute
    {
        /// <summary>
        /// Ida Acteur MRO ou bien un CBO
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IDA;

        /// <summary>
        /// True s'il s'agit d'un acteur avec le rôle Cash Balance Office
        /// </summary>
        [XmlAttribute(AttributeName = "isCBO")]
        public bool IsCBO;

        /// <summary>
        /// Idb du Book spécifié sur l'Acteur CBO
        /// </summary>
        [XmlAttribute(AttributeName = "idbCBO")]
        public int IdB_CBO;

        /// <summary>
        /// Identifier du Book spécifié sur l'Acteur CBO
        /// </summary>
        [XmlAttribute(AttributeName = "identifierBookCBO")]
        public string IdentifierB_CBO;

        /// <summary>
        /// Niveau de calcul de Cash-Flows et de Soldes sur l'Acteur CBO
        /// </summary>
        [XmlAttribute(AttributeName = "scopeCBO")]
        public GlobalElementaryEnum Scope_CBO;

        /// <summary>
        /// True pour utiliser les espèces disponibles en couverture
        /// </summary>
        [XmlAttribute(AttributeName = "isUseAvailableCash")]
        public bool IsUseAvailableCash;

        /// <summary>
        /// Type de calcul des Appels/Restitutions de Déposits:
        /// <para>FlowCurrency: en devise d'origine du flux</para>
        /// <para>ACU_BUSINESSDATE: en devise de contrevaleur, avec utilisation du fixing de change publié en dates de compensation.</para>
        /// </summary>
        [XmlAttribute(AttributeName = "exchangeType")]
        public CBExchangeTypeEnum ExchangeType;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "mgcCalcMethod")]
        public MarginCallCalculationMethodEnum MgcCalcMethod;

        /// <summary>
        /// Devise à utiliser dans le cas ou la méthode de calcul des Appels/Restitutions est:
        /// <para>- MGCCTRVAL (Déposit et couverture en contrevaleur): utilisée comme devise de contrevaleur</para>
        /// <para>- MGCCOLLATCTRVAL (Déposit en devise et couverture en contrevaleur): utilisée comme devise pivot pour le calcul des contresvaleurs</para>
        /// </summary>
        [XmlAttribute(AttributeName = "exchangeIDC")]
        public string ExchangeIDC;

        /// <summary>
        /// La provenance des éléments de couverture à prendre en compte
        /// </summary>
        [XmlAttribute(AttributeName = "cashAndCollateralLocalization")]
        public CashAndCollateralLocalizationEnum CashAndCollateral;

        /// <summary>
        /// Méthode de calcul du cash balance
        /// </summary>
        [XmlAttribute(AttributeName = "cbCalcMethod")]
        public CashBalanceCalculationMethodEnum CbCalcMethod;

        /// <summary>
        /// Devise de contrevaleur à utiliser dans le cas d'un calcul du cash balance par la méthode 2 (Méthode CSBUK)
        /// </summary>
        [XmlAttribute(AttributeName = "cbIDC")]
        public string CbIDC;

        /// <summary>
        /// <para>True: gestion des soldes, chaque solde précédent sera pris en compte lors du calcul d'un nouveau solde.</para>
        /// False:  pas de gestion des soldes, chaque calcul d'un nouveau solde considèrera un solde précédent à zéro.
        /// </summary>
        [XmlAttribute(AttributeName = "isManagementBalance")]
        public bool IsManagementBalance;

        /// <summary>
        /// True s'il s'agit d'un acteur avec le rôle Margin Office (MRO)
        /// </summary>
        [XmlAttribute(AttributeName = "isMRO")]
        public bool IsMRO;

        /// <summary>
        /// Idb du Book spécifié sur l'Acteur MRO
        /// </summary>
        [XmlAttribute(AttributeName = "idbMRO")]
        public int IdB_MRO;

        /// <summary>
        /// Identifier du Book spécifié sur l'Acteur MRO
        /// </summary>
        [XmlAttribute(AttributeName = "identifierBookMRO")]
        public string IdentifierB_MRO;

        /// <summary>
        /// Niveau de calcul de de montants de déposit sur l'Acteur MRO
        /// </summary>
        [XmlAttribute(AttributeName = "scopeMRO")]
        public GlobalElementaryEnum Scope_MRO;

        /// <summary>
        /// Priorités d'utilisation des Espèces, Titres et Actions lors du calcul de la couverture des appels de déposits.
        /// </summary>
        [XmlArray(ElementName = "collateralPriorities")]
        [XmlArrayItemAttribute("collateralPriority")]
        public List<CBCollateralPriority> CollateralPriority;

        /// <summary>
        /// Priorités de couverture des devises lors du calcul de la couverture des appels de déposits.
        /// </summary>
        [XmlArray(ElementName = "collateralCurrencyPriorities")]
        [XmlArrayItemAttribute("collateralCurrencyPriority")]
        public List<CBCollateralCurrencyPriority> CollateralCurrencyPriority;

        public CBBusinessAttribute() { }
        public CBBusinessAttribute(int pIDA, bool pIsMRO, int pIdB_MRO, string pIdentifierB_MRO,
            GlobalElementaryEnum pScope_MRO, bool pIsCBO, int pIdB_CBO, string pIdentifierB_CBO,
            GlobalElementaryEnum pScope_CBO, bool pIsUseAvailableCash, CBExchangeTypeEnum pExchangeType,
            MarginCallCalculationMethodEnum pMgcCalcMethod, string pExchangeIDC, CashAndCollateralLocalizationEnum pCashAndCollateral,
            bool pIsManagementBalance, CashBalanceCalculationMethodEnum pCbCalcMethod, string pCbIDC)
        {
            IDA = pIDA;
            //
            IsMRO = pIsMRO;
            IdB_MRO = pIdB_MRO;
            IdentifierB_MRO = pIdentifierB_MRO;
            Scope_MRO = pScope_MRO;
            //
            IsCBO = pIsCBO;
            IdB_CBO = pIdB_CBO;
            IdentifierB_CBO = pIdentifierB_CBO;
            Scope_CBO = pScope_CBO;
            IsUseAvailableCash = pIsUseAvailableCash;
            ExchangeType = pExchangeType;
            MgcCalcMethod = pMgcCalcMethod;
            ExchangeIDC = pExchangeIDC;
            CashAndCollateral = pCashAndCollateral;
            //
            CbCalcMethod = pCbCalcMethod;
            CbIDC = pCbIDC;
            if (CbCalcMethod == CashBalanceCalculationMethodEnum.CSBUK)
            {
                // Pour les cash balances en méthode UK, les margin requierement et collateral sont forcement tous les deux en devise initiale
                MgcCalcMethod = MarginCallCalculationMethodEnum.MGCCOLLATCUR;
            }
            //
            IsManagementBalance = pIsManagementBalance;
            //
            if (IsCBO)
            {
                CollateralPriority = new List<CBCollateralPriority>();
                CollateralCurrencyPriority = new List<CBCollateralCurrencyPriority>();
            }
        }
    }
    
    /// <summary>
    /// Représente un trade 
    /// </summary>
    /// FI 20170208 [22151][22152] Add
    /// FI 20170316 [22950] Modify
    public class CBTrade
    {
        /// <summary>
        /// IdT du trade
        /// </summary>
        [XmlAttribute(AttributeName = "idT")]
        public int IdT;

        /// <summary>
        /// Identifier du trade
        /// </summary>
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;

        /// <summary>
        /// status
        /// </summary>
        [XmlAttribute(AttributeName = "status")]
        public StatusEnum Status;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtBusinessSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170316 [22950] Add
        [XmlAttribute(AttributeName = "dtBusiness")]
        public DateTime dtBusiness;


        /// <summary>
        /// 
        /// </summary>
        public CBTrade()
        { }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pStatus"></param>
        /// <param name="pDtBusiness"></param>
        /// FI 20170316 [22950] Modify
        public CBTrade(int pIdT, string pIdentifier, StatusEnum pStatus, DateTime pDtBusiness)
        {
            IdT = pIdT;
            Identifier = pIdentifier;
            Status = pStatus;
            // FI 20170316 [22950] Alimentation de dtBusiness
            dtBusinessSpecified = DtFunc.IsDateTimeFilled(pDtBusiness);
            dtBusiness = pDtBusiness;
        }
    }


}
