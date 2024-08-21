using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Enum;
using FixML.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// class treating the diagnostic CBREQUEST table
    /// </summary>
    /// FI 20200731 [XXXXX] Reecriture de CBRequestDiagnostics
    public sealed class CBRequestDiagnostics2 : DatetimeProfiler
    {
        private DataTable dtCBRequest;
        private string queryForAdapter;
        /// <summary>
        ///  user (for IDAINS et IDAUPD)
        /// </summary>
        readonly int idA;

        public string Identifier_Entity
        { private set; get; }
        public DateTime DtBusiness
        { private set; get; }
        public int? IdPr
        { private set; get; }
        public SettlSessIDEnum Timing
        { private set; get; }
        public int Ida_Entity
        { private set; get; }

        /// <summary>
        /// Build a new diagnostics object
        /// <para>CBREQUEST</para>
        /// </summary>
        public CBRequestDiagnostics2(string pCs, int pIdA, int pIda_Entity, string pIdentifier_Entity, DateTime pDtBusiness, SettlSessIDEnum pTiming, int? pIdPr) :
            base(OTCmlHelper.GetDateSysUTC(pCs))
        {
            idA = pIdA;
            Ida_Entity = pIda_Entity;
            Identifier_Entity = pIdentifier_Entity;
            DtBusiness = pDtBusiness;
            Timing = pTiming;
            IdPr = pIdPr;

            InitQueryForAdapter();
        }


        /// <summary>
        /// Alimentation de la table CBREQUEST:
        /// <para>- création d'un une ligne chapeau si elle n'existe pas avec le statut "NA"</para>
        /// <para>- Initialisation de toutes les lignes existantes de la table avec le statut "NA"</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction">valeur null autorisée. Alimentation de la table en dehors d'une transaction</param>
        /// FI 20200729 [XXXXX] Add pCS et pDbTransaction
        public void StartCBRequest(string pCS, IDbTransaction pDbTransaction)
        {
            try
            {
                LoadCBRequest(pCS, pDbTransaction);

                DataRow headerRow = GetRowHeader();
                if (null == headerRow)
                {
                    headerRow = dtCBRequest.NewRow();
                    InitializeCBRequestNewRow(headerRow);
                    dtCBRequest.Rows.Add(headerRow);
                }
                else
                {
                    InitializeCBRequestUpdRow(headerRow);
                }

                headerRow["DTSTART"] = GetDate();
                headerRow["STATUS"] = ProcessStateTools.StatusProgressEnum.ToString();

                IEnumerable<DataRow> noHeaderRows =
                    from row in dtCBRequest.Rows.Cast<DataRow>()
                    where ObjFunc.IsNotDBNull(row["IDA_CBO"]) && ObjFunc.IsNotDBNull(row["IDB_CBO"])
                    select row;

                foreach (DataRow row in noHeaderRows)
                    InitializeCBRequestUpdRow(row);

                // Appel à SynchronizeDatabase pour recharger le dataset lorsque la colonne header est ajoutée
                SynchronizeDatabase(pCS, pDbTransaction);

            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04061",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        /// Met à jour le statut de la ligne chapeau en fonction des lignes détails et met à jour la base de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction">valeur null autorisée. Dans ce cas la mise à jour s'effectue en dehors d'une transaction</param>
        /// FI 20200729 [XXXXX] Add pCS et pDbTransaction
        public void EndCBRequest(string pCS, IDbTransaction pDbTransaction)
        {
            try
            {
                DataRow headerRow = GetRowHeader();
                if (null == headerRow)
                    throw new NullReferenceException("headerRow is null");

                IEnumerable<DataRow> otherRowsNotSuccess =
                    from row in dtCBRequest.Rows.Cast<DataRow>()
                    where ObjFunc.IsNotDBNull(row["IDA_CBO"]) && ObjFunc.IsNotDBNull(row["IDB_CBO"])
                    && (false == ProcessStateTools.IsStatusSuccess(row["STATUS"].ToString()))
                    select row;

                EndCBRequestRowHeader((otherRowsNotSuccess.Count() > 0) ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusSuccessEnum);

                SynchronizeDatabase(pCS, pDbTransaction);
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04062",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        /// Met à jour le statut de la ligne chapeau et met à jour la base de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pStatus"></param>
        public void EndCBRequest(string pCS, IDbTransaction pDbTransaction, ProcessStateTools.StatusEnum pStatus)
        {
            try
            {
                if (null == dtCBRequest)
                    throw new NullReferenceException("dataset dsCBRequest is null");

                EndCBRequestRowHeader(pStatus);

                SynchronizeDatabase(pCS, pDbTransaction);

            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04062",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        /// Met à jour la table CBRequest par rapport à la liste des couples Actor/Book {pPartyTradeInfos} et met à jour la base de données:
        /// <para>- Update des lignes existantes dans la table CBRequest et existantes dans la liste {pPartyTradeInfos}</para>
        /// <para>- Insert des lignes NON existantes dans la table CBRequest et existantes dans la liste {pPartyTradeInfos}</para>
        /// <para>- Delete des lignes existantes dans la table CBRequest et NON existantes dans la liste {pPartyTradeInfos}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction">valeur null autorisée. Dans ce cas la mise à jour s'effectue en dehors d'une transaction</param>
        /// <param name="pPartyTradeInfos"></param>
        /// FI 20200729 [XXXXX] Add pCS et pDbTransaction
        public void UpdateCBRequestPartyTradeInfo(string pCS, IDbTransaction pDbTransaction, List<CBPartyTradeInfo> pPartyTradeInfos)
        {
            try
            {
                List<DataRow> rowsToDelete = new List<DataRow>();

                foreach (DataRow row in dtCBRequest.Rows)
                {
                    if (ObjFunc.IsNotDBNull(row["IDA_CBO"]))
                    {
                        CBPartyTradeInfo partyTradeInfo = pPartyTradeInfos.Find(
                            tradeInfo => (ObjFunc.IsNotDBNull(row["IDA_CBO"]))
                                    && tradeInfo.Ida == Convert.ToInt32(row["IDA_CBO"])
                                    && tradeInfo.Idb == Convert.ToInt32(row["IDB_CBO"]));

                        if (partyTradeInfo != null)
                        {
                            // Row to Update
                            InitializeCBRequestUpdRow(row);
                            row["EXCHANGETYPE"] = System.Enum.GetName(typeof(CBExchangeTypeEnum), partyTradeInfo.ActorCBO.BusinessAttribute.ExchangeType);
                            row["MGCCALCMETHOD"] = System.Enum.GetName(typeof(MarginCallCalculationMethodEnum), partyTradeInfo.ActorCBO.BusinessAttribute.MgcCalcMethod);
                        }
                        else
                            rowsToDelete.Add(row);// Row to Delete
                    }
                }

                foreach (DataRow row in rowsToDelete)
                    row.Delete();

                foreach (CBPartyTradeInfo partyTradeInfo in pPartyTradeInfos)
                {
                    DataRow[] rows = dtCBRequest.Select(
                        "IDA_CBO = " + partyTradeInfo.Ida.ToString() + " and IDB_CBO = " + partyTradeInfo.Idb.ToString());

                    if (ArrFunc.IsEmpty(rows))
                    {
                        // Row to Insert
                        DataRow newRow = dtCBRequest.NewRow();
                        InitializeCBRequestNewRow(newRow);
                        newRow["IDA_CBO"] = partyTradeInfo.Ida;
                        newRow["IDB_CBO"] = partyTradeInfo.Idb;
                        newRow["EXCHANGETYPE"] = System.Enum.GetName(typeof(CBExchangeTypeEnum), partyTradeInfo.ActorCBO.BusinessAttribute.ExchangeType);
                        newRow["MGCCALCMETHOD"] = System.Enum.GetName(typeof(MarginCallCalculationMethodEnum), partyTradeInfo.ActorCBO.BusinessAttribute.MgcCalcMethod);
                        dtCBRequest.Rows.Add(newRow);
                    }
                }

                 SynchronizeDatabase(pCS, pDbTransaction);
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04063",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        /// Met à jour le datarow associé au couple pPartyTradeInfo
        /// </summary>
        /// <param name="pPartyTradeInfo"></param>
        /// <param name="pStatus"></param>
        /// <param name="pIdT"></param>
        /// EG 20180205 [23769] Add dbTransaction  
        /// FI 20200729 [XXXXX] Add pCS et pDbTransaction
        public void UpdateCBRequestPartyTradeInfoRow(CBPartyTradeInfo pPartyTradeInfo, ProcessStateTools.StatusEnum pStatus, int pIdT)
        {
            try
            {
                lock (dtCBRequest.Rows)
                {
                    DataRow row =
                    (from DataRow item in dtCBRequest.Rows
                     where ObjFunc.IsNotDBNull(item["IDA_CBO"])
                     && Convert.ToInt32(item["IDA_CBO"]) == pPartyTradeInfo.Ida
                     && ObjFunc.IsNotDBNull(item["IDB_CBO"])
                     && Convert.ToInt32(item["IDB_CBO"]) == pPartyTradeInfo.Idb
                     select item).First();

                    if (null == row)
                        throw new Exception("row is null");

                    row["STATUS"] = System.Enum.GetName(typeof(ProcessStateTools.StatusEnum), pStatus);

                    if (pIdT > 0)
                        row["IDT"] = pIdT;

                    if (pStatus == ProcessStateTools.StatusEnum.PROGRESS)
                        row["DTSTART"] = GetDate();
                    else
                        row["DTEND"] = GetDate();
                }
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-04062",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        /// Met à jour les valeurs de la base de données en exécutant les instructions INSERT, UPDATE ou DELETE et recharge le dataset si nécessaire
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        public void SynchronizeDatabase(string pCS, IDbTransaction pDbTransaction)
        {
            if (null == dtCBRequest)
                throw new NullReferenceException("dtCBRequest is null");

            DateTime dtSys = GetDate();

            foreach (var item in dtCBRequest.Rows.Cast<DataRow>().Where(x => x.RowState == DataRowState.Modified))
            {
                item["IDAUPD"] = this.idA;
                item["DTUPD"] = dtSys;
            }

            foreach (var item in dtCBRequest.Rows.Cast<DataRow>().Where(x => x.RowState == DataRowState.Added))
            {
                item["IDAINS"] = this.idA;
                item["DTINS"] = dtSys;
            }

            bool isLoadCbRequest = (dtCBRequest.Rows.Cast<DataRow>().Where(x => x.RowState == DataRowState.Added).Count() > 0);

            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, queryForAdapter, dtCBRequest);
            if (isLoadCbRequest)
            {
                // Chargement nécessairede manière à alimenter les colonnes IDCBREQUEST pour chaque ligne insérée
                LoadCBRequest(pCS, pDbTransaction);
            }
        }

        /// <summary>
        /// Charge les enregistrements présents dans CBREQUEST
        /// <para>pour la même entité, la même date et le même timing </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction">valeur null autorisée. chgt en dehors d'une transaction</param>
        private void LoadCBRequest(string pCS, IDbTransaction pDbTransaction)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), Ida_Entity);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness); // FI 20201006 [XXXXX] DbType.Date

            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.TIMING), Timing);

            StrBuilder sqlWhere = new StrBuilder();
            sqlWhere += SQLCst.WHERE + "(cbr.IDA_ENTITY=@IDA_ENTITY)" + Cst.CrLf;
            sqlWhere += SQLCst.AND + "(cbr.DTBUSINESS=@DTBUSINESS)" + Cst.CrLf;
            sqlWhere += SQLCst.AND + "(cbr.CBTIMING=@TIMING)" + Cst.CrLf;

            DataSet dsCbRequest = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text,
                queryForAdapter + Cst.CrLf + sqlWhere.ToString(), dataParameters.GetArrayDbParameter());

            dtCBRequest = dsCbRequest.Tables[0];
        }

        /// <summary>
        /// Initialise une nouvelle ligne. Le statut est positionné à "N/A". 
        /// </summary>
        /// <param name="pRow"></param>
        private void InitializeCBRequestNewRow(DataRow pRow)
        {

            pRow["IDA_ENTITY"] = Ida_Entity;
            pRow["DTBUSINESS"] = DtBusiness;
            pRow["CBTIMING"] = Timing;

            if (this.IdPr.HasValue)
                pRow["IDPR"] = this.IdPr.Value;
            else
                pRow["IDPR"] = DBNull.Value;

            pRow["STATUS"] = ProcessStateTools.StatusUnknown;

            pRow["EXCHANGETYPE"] = DBNull.Value;
            pRow["MGCCALCMETHOD"] = DBNull.Value;
            pRow["IDA_CBO_CBREQUEST"] = DBNull.Value;
            pRow["IDT"] = DBNull.Value;
            pRow["DTSTART"] = DBNull.Value;
            pRow["DTEND"] = DBNull.Value;
            pRow["EXTLLINK"] = DBNull.Value;
            pRow["ROWATTRIBUT"] = DBNull.Value;
        }



        /// <summary>
        /// Initialise une ligne déjà existante. Le statut est positionné à "N/A"
        /// </summary>
        /// <param name="pRow"></param>
        private void InitializeCBRequestUpdRow(DataRow pRow)
        {
            pRow["STATUS"] = System.Enum.GetName(typeof(ProcessStateTools.StatusEnum), ProcessStateTools.StatusUnknownEnum);

            if (this.IdPr.HasValue)
                pRow["IDPR"] = this.IdPr.Value;
            else
                pRow["IDPR"] = DBNull.Value;

            pRow["EXCHANGETYPE"] = null;
            pRow["MGCCALCMETHOD"] = null;
            pRow["IDA_CBO_CBREQUEST"] = DBNull.Value;
            pRow["IDT"] = DBNull.Value;
            pRow["DTSTART"] = DBNull.Value;
            pRow["DTEND"] = DBNull.Value;
            pRow["EXTLLINK"] = DBNull.Value;
            pRow["ROWATTRIBUT"] = DBNull.Value;
        }

        /// <summary>
        /// Met à jour le statut de la ligne chapeau avec le statut {pStatus}
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20200729 [XXXXX] Add pCS et pDbTransaction
        private void EndCBRequestRowHeader(ProcessStateTools.StatusEnum pStatus)
        {
            try
            {
                if (null == dtCBRequest)
                    throw new NullReferenceException("dataset dsCBRequest is null");

                DataRow row = GetRowHeader();
                if (null == row)
                    throw new NullReferenceException("headerRow is null");

                row["STATUS"] = System.Enum.GetName(typeof(ProcessStateTools.StatusEnum), pStatus);
                row["DTEND"] = GetDate();

            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04062",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE), ex);
            }
        }

        /// <summary>
        ///  Retourne la ligne Header (IDA_CBO is null && IDB_CBO is null)
        ///  <para>Retourne null si la ligne n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20200729 [XXXXX] Add
        private DataRow GetRowHeader()
        {
            DataRow headerRow =
                    (from row in dtCBRequest.Rows.Cast<DataRow>()
                     where Convert.IsDBNull(row["IDA_CBO"]) && Convert.IsDBNull(row["IDB_CBO"])
                     select row
                    ).FirstOrDefault();

            return headerRow;
        }
        /// <summary>
        /// Alimentation de queryForAdapter
        /// </summary>
        private void InitQueryForAdapter()
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDCBREQUEST, IDPR, IDA_ENTITY, IDA_CBO, IDB_CBO," + Cst.CrLf;
            sqlQuery += "EXCHANGETYPE, MGCCALCMETHOD, IDA_CBO_CBREQUEST, DTBUSINESS, CBTIMING, STATUS, IDT," + Cst.CrLf;
            sqlQuery += "DTSTART, DTEND, DTUPD, IDAUPD, DTINS, IDAINS, EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST + " cbr" + Cst.CrLf;
            queryForAdapter = sqlQuery.ToString();
        }
    }
}


