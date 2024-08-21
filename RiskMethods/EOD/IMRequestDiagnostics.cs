using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.Hierarchies;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance.EOD
{
    /// <summary>
    /// class treating the diagnostic IMREQUEST table
    /// </summary>
    /// FI 20200818 [XXXXX] IMRequestDiagnostics herite de DatetimeProfiler
    public sealed class IMRequestDiagnostics : DatetimeProfiler
    {
        string m_CS;

        DbSvrType m_SvrTyp;

        int m_EntityId;

        int? m_IdPr;

        SettlSessIDEnum m_Timing;

        

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        AddCriticalException m_AddCriticalException = null;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        SetErrorWarning m_SetErrorWarning = null;

        /// <summary>
        /// 
        /// </summary>
        readonly Dictionary<int, Dictionary<Pair<int, int>, IMREQUEST>> m_MarginReqOfficeStatus =
            new Dictionary<int, Dictionary<Pair<int, int>, IMREQUEST>>();

        /// <summary>
        /// collection containing all the roots hierarchy, externally built
        /// </summary>
        /// <remarks>without this collection no id MARGINREQOFFICE parent can be found for the current diagnostic elements</remarks>
        public IEnumerable<ActorNode> Roots
        { get; set; }

        /// <summary>
        /// Service info
        /// </summary>
        [XmlIgnore]
        public AppSession AppSession
        { get; set; }

        /// <summary>
        /// Build a new diagnostics object
        /// </summary>
        public IMRequestDiagnostics() : base()
        {
        }

        /// <summary>
        /// Initialize the session parameters
        /// </summary>
        public void Initialize(string pCS, int pEntityId, SettlSessIDEnum pTiming, int? pIdPr)
        {
            m_MarginReqOfficeStatus.Clear();

            m_CS = pCS;

            m_SvrTyp = DataHelper.GetDbSvrType(m_CS);

            m_EntityId = pEntityId;

            m_Timing = pTiming;

            m_IdPr = pIdPr;

            // FI 20200818 [XXXXX] Initailisation avec la date système du SGBD 
            // Rq dans avenir proche prévoir une date UTC
            // FI 20200820 [25468] Dates systemes en UTC
            base.Start(OTCmlHelper.GetDateSysUTC(pCS));

        }


        /// <summary>
        /// Preliminary operation: building the list of the diagnostic elements for each treated MARGINREQOFFICE actor (and relative books)
        /// </summary>
        /// <param name="pCssId">related clearing house</param>
        /// <param name="pBusinessDate">business date of the executed risk evaluation process</param>
        /// <param name="pDeposits">all the generated deposits that are relative to a valid positions set</param>
        /// <param name="pPrepareHeader">true when we prepare just the header diagnostic element, false otherwise when we prepare
        /// the diagnostic elements for all treated MARGINREQOFFICE actor</param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void PrepareElements(int pCssId, DateTime pBusinessDate, IEnumerable<Deposit> pDeposits, bool pPrepareHeader)
        {

            using (IDbConnection connection = DataHelper.OpenConnection(this.m_CS))
            {
                using (IDbTransaction transaction = DataHelper.BeginTran(connection))
                {
                    DataSet dataSetToUpdate = null;

                    DataAdapter dataAdapter = null;

                    IDbCommand command = null;

                    try
                    {

                        // 1. Load existing diagnostic elements (in case of we execute times the same treatment)

                        IEnumerable<IMREQUEST> imRequestElements = LoadImRequestElements(transaction, pCssId, pBusinessDate, out dataAdapter, out dataSetToUpdate, out command, pPrepareHeader);

                        IEnumerable<RiskElement> rootElements;

                        // 2. identify the elements to evaluate: in case of we prepare the header we build a fake 0 deposit,
                        //  otherwie we extract all the root element for each valid deposit to evaluate

                        if (pPrepareHeader)
                        {
                            rootElements = new RiskElement[] { new RiskElement { ActorId = 0, AffectedBookId = 0 } };
                        }
                        else
                        {
                            rootElements = from deposit in pDeposits
                                           select deposit.Root;
                        }

                        // 3. Intersect the existing elements with the elements to evaluate, 

                        IEnumerable<IMREQUEST> sharedImRequestElements = IntersectImRequestElements(imRequestElements, rootElements);

                        // 4. found the entries to delete from the existing elements collection

                        IEnumerable<IMREQUEST> toDeleteImRequestElements = imRequestElements.Except(sharedImRequestElements);

                        // 5. found the entries to update into the existing elements collection

                        IEnumerable<IMREQUEST> toUpdateImRequestElements = imRequestElements.Except(toDeleteImRequestElements);

                        // 5. found the new entries from elements collection to evaluate, 
                        //  to add to the existing elements collection

                        IEnumerable<IMREQUEST> toAddImRequestElements = GetNewImRequestElements(imRequestElements, rootElements);

                        // 6. build the collection of the valid elements (element to update + elements to add)

                        IEnumerable<IMREQUEST> imRequestValidElements = toUpdateImRequestElements.Union(toAddImRequestElements);

                        // 7. Update the database

                        UpdateDataTable(dataSetToUpdate.Tables[0], pCssId, pBusinessDate, toUpdateImRequestElements, toAddImRequestElements, toDeleteImRequestElements);

                        // FI 20200818 [XXXXX] call SynchronizeDatabase
                        SynchronizeDatabase(dataAdapter, dataSetToUpdate);


                        DataHelper.CommitTran(transaction);

                        // 8. build the dictionary of the valid diagnostic elements (used along the evaluation process)

                        if (!pPrepareHeader)
                        {
                            BuildMarginReqOfficeStatusDictionary(pCssId, imRequestValidElements);
                        }

                    }
                    catch (Exception ex)
                    {
                        DataHelper.RollbackTran(transaction);

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(Ressource.GetString("RiskPerformance_ERRIMRequestInsertion"), ex);
                        // FI 20200623 [XXXXX] AddException
                        m_AddCriticalException.Invoke(spheresEx);

                        Logger.Log(new LoggerData(spheresEx));
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1018), 0));

                        throw spheresEx;
                    }
                    finally
                    {
                        if (dataAdapter != null)
                        {
                            dataAdapter.Dispose();
                        }

                        if (dataSetToUpdate != null)
                        {
                            dataSetToUpdate.Dispose();
                        }

                        if (command != null)
                        {
                            command.Parameters.Clear();
                            command.Dispose();
                        }
                    }

                } // disposing transaction

            } // disposing connection
        }

        /// <summary> 
        /// Validate the status of the diagnostic element for each treated MARGINREQOFFICE actor
        /// </summary>
        /// <param name="pCssId">related clearing house</param>
        /// <param name="pBusinessDate">business date of the executed risk evaluation process</param>
        /// <param name="pStatus">evaluation status to set to the diagnostic elements</param>
        /// <param name="pPrepareHeader">true when we validate just the header diagnostic element, false otherwise when we validate
        /// the diagnostic elements of all the treated MARGINREQOFFICE actor</param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void ValidateElements(int pCssId, DateTime pBusinessDate, ProcessStateTools.StatusEnum pStatus, bool pPrepareHeader)
        {
            using (IDbConnection connection = DataHelper.OpenConnection(this.m_CS))
            {
                using (IDbTransaction transaction = DataHelper.BeginTran(connection))
                {
                    DataSet dataSetToUpdate = null;
                    DataAdapter dataAdapter = null;
                    IDbCommand command = null;
                    try
                    {
                        // 1. Load the existing diagnostic elements of the database (generated or updated during the PrepareElements call)
                        LoadImRequestElements(
                            transaction, pCssId, pBusinessDate, out dataAdapter, out dataSetToUpdate, out command, pPrepareHeader);

                        // 2. Validate
                        if (pPrepareHeader)
                        {
                            // 2.1 Validate header diagnostic element
                            FinalizeImRequestHeader(dataSetToUpdate.Tables[0], pStatus);
                        }
                        else
                        {
                            // 2.2 Validate all the diagnostic element for each treated MARGINREQOFFICE actor (and relative books)
                            FinalizeImRequestElements
                                (pCssId, dataSetToUpdate.Tables[0], pStatus);
                        }
                        // FI 20200818 [XXXXX] Call SynchronizeDatabase
                        SynchronizeDatabase(dataAdapter, dataSetToUpdate);

                        DataHelper.CommitTran(transaction);
                    }
                    catch (Exception ex)
                    {
                        DataHelper.RollbackTran(transaction);

                        SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(Ressource.GetString("RiskPerformance_ERRIMRequestValidation"), ex);
                        // FI 20200623 [XXXXX] AddException
                        m_AddCriticalException(spheresEx);

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(spheresEx));
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1018), 0));

                        throw spheresEx;
                    }
                    finally
                    {
                        if (dataAdapter != null)
                        {
                            dataAdapter.Dispose();
                        }

                        if (dataSetToUpdate != null)
                        {
                            dataSetToUpdate.Dispose();
                        }

                        if (command != null)
                        {
                            command.Parameters.Clear();
                            command.Dispose();
                        }
                    }
                } // disposing transaction

            } // disposing connection

        }

        /// <summary>
        /// Set the main properties values for the internal collection of diagnostic elements 
        /// </summary>
        /// <param name="pCssId">related clearing house</param>
        /// <param name="pActorsRoleMarginReqOffice">list of all the MARGINREQOFFICE actors actually loaded</param>
        public void SetMainPropertiesNewImRequestElements
            (int pCssId, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        {

            // 1 build the actor/book collection from the internal collection of diagnostic elements

            Dictionary<Pair<int, int>, IMREQUEST> dictRequest = this.m_MarginReqOfficeStatus[pCssId];

            var actorsBooksCollection =
                from actor in pActorsRoleMarginReqOffice
                from book in actor.GetRolesTypeOf<RoleMarginReqOfficeAttribute>().First().Books
                select new
                {
                    Actor = actor,
                    BookId = book.Id
                };

            foreach (var actorBook in actorsBooksCollection)
            {

                Pair<int, int> key = new Pair<int, int> { First = actorBook.Actor.Id, Second = actorBook.BookId };

                if (dictRequest.ContainsKey(key))
                {
                    IMREQUEST element = dictRequest[key];

                    // PM 20140114 [19489] First() => FirstOrDefault() + test du default
                    ActorNode rootMarginReqOffice = (from root in Roots where root.Id == actorBook.Actor.RootId select root).FirstOrDefault();
                    if (rootMarginReqOffice != default(ActorNode))
                    {
                        List<ActorNode> ancestors = actorBook.Actor.FindAncestors(rootMarginReqOffice, obj => obj.Built);

                        ActorNodeWithSpecificRoles marginReqOfficeAncestors = (
                            from ancestor in ancestors
                            where
                                ancestor is ActorNodeWithSpecificRoles roles &&
                                roles.GetRolesTypeOf<RoleMarginReqOfficeAttribute>().Length > 0
                            select (ActorNodeWithSpecificRoles)ancestor
                            ).FirstOrDefault();

                        if (marginReqOfficeAncestors != null)
                        {
                            // 2. set the MARGINREOFFICE parent for the current diangostic element

                            element.ParentId = marginReqOfficeAncestors.Id;
                        }

                        // 3. set the evaluation type

                        element.IsGrossMargining = actorBook.Actor.GetRolesTypeOf<RoleMarginReqOfficeAttribute>().First().IsGrossMargining;
                    }
                }

            }
        }

        /// <summary>
        /// Set the stard/end time for the diagnostic element relative to the given input actor/book pair
        /// </summary>
        /// <param name="pCssId">related clearing house</param>
        /// <param name="pActorId">actor id of the target diagnostic element</param>
        /// <param name="pBookId">book id of the targer diagnostic element</param>
        /// <param name="pStart">Start time of the evaluation</param>
        /// <param name="pEnd">End time of the evaluation</param>
        public void SetStartEndTime(int pCssId, int pActorId, int pBookId, DateTime pStart, DateTime pEnd)
        {
            Pair<int, int> key = new Pair<int, int> { First = pActorId, Second = pBookId };

            if (!this.m_MarginReqOfficeStatus.ContainsKey(pCssId) || !this.m_MarginReqOfficeStatus[pCssId].ContainsKey(key))
            {
                return;
            }

            IMREQUEST element = this.m_MarginReqOfficeStatus[pCssId][key];

            element.Start = pStart;

            element.End = pEnd;
        }

        /// <summary>
        /// Set the trade Id (typeof <see cref="EfsML.v30.MarginRequirement.MarginRequirement"/>) 
        /// for the diagnostic element relative to the given input actor/book pair
        /// </summary>
        /// <remarks>the trade id is the one containing the deposit amount affecting the given input actor/book pair </remarks>
        /// <param name="pCssId">related clearing house</param>
        /// <param name="pActorId">actor id of the target diagnostic element</param>
        /// <param name="pBookId">book id of the targer diagnostic element</param>
        /// <param name="pIdTradeMarginRequirement">id of the trade DEPOSIT relative to the given input actor/book pair</param>
        public void SetTrade(int pCssId, int pActorId, int pBookId, int pIdTradeMarginRequirement)
        {
            Pair<int, int> key = new Pair<int, int> { First = pActorId, Second = pBookId };

            if (!this.m_MarginReqOfficeStatus.ContainsKey(pCssId) || !this.m_MarginReqOfficeStatus[pCssId].ContainsKey(key))
            {
                return;
            }

            IMREQUEST element = this.m_MarginReqOfficeStatus[pCssId][key];

            element.TradeId = pIdTradeMarginRequirement;
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pLog">delegate reference</param>
        public void InitDelegate(SetErrorWarning pSetErrorWarning, AddCriticalException pAddException)
        {
            m_SetErrorWarning = pSetErrorWarning;
            m_AddCriticalException = pAddException;
        }

        private IEnumerable<IMREQUEST> LoadImRequestElements
            (IDbTransaction pTransaction, int pCssId, DateTime pBusinessDate, out DataAdapter opDataAdapter,
            out DataSet opElements, out IDbCommand opCommand, bool pLoadHeader)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "IDENTITY", m_EntityId },
                { "IDA_CSSCUSTODIAN", pCssId },
                { "DTBUSINESS", pBusinessDate },
                // FI 20171025 [23533] Appel à ConvertEnumToString
                //parameters.Add("TIMING", ReflectionTools.GetCustomAttributeValue<XmlEnumAttribute>
                //    (typeof(SettlSessIDEnum), Enum.GetName(typeof(SettlSessIDEnum), m_Timing), true));
                { "TIMING", ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(m_Timing) }
            };

            opDataAdapter = DataHelper.GetDataAdapter(
                    pTransaction,
                    DataContractHelper.GetQuery(DataContractResultSets.IMREQUESTELEMENTS),
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.IMREQUESTELEMENTS, parameters),
                    out opElements, out opCommand);

            List<IMREQUEST> mroElements = DataHelper<IMREQUEST>.TransformDataSet(opElements);

            if (pLoadHeader)
            {
                mroElements = (
                    from mroElement
                        in mroElements
                    where mroElement.MroId == 0
                    select mroElement
                    ).ToList();
            }
            else
            {
                mroElements = (
                    from mroElement
                        in mroElements
                    where mroElement.MroId != 0
                    select mroElement
                    ).ToList();

            }



            return mroElements;
        }

        private static IEnumerable<IMREQUEST> IntersectImRequestElements
           (IEnumerable<IMREQUEST> pIMRequestElements, IEnumerable<RiskElement> pRootElements)
        {
            IEnumerable<IMREQUEST> normalizedMroElements =
                from mroElement in pIMRequestElements
                join rootElement
                    in pRootElements
                on new
                {
                    IdA = mroElement.MroId,
                    IdB = mroElement.MroBookId
                } equals new
                {
                    IdA = rootElement.ActorId,
                    IdB = rootElement.AffectedBookId
                }
                select mroElement;

            return normalizedMroElements;
        }

        private IEnumerable<IMREQUEST> GetNewImRequestElements(IEnumerable<IMREQUEST> imRequestElements,IEnumerable<RiskElement> rootElements)
        {
            IEnumerable<Pair<int, int>> mroElementsPair =
                from normalizedMroElement in imRequestElements
                select new Pair<int, int> { First = normalizedMroElement.MroId, Second = normalizedMroElement.MroBookId };

            IEnumerable<Pair<int, int>> actorsRoleMarginReqOfficePair =
                from rootElement in rootElements
                select new Pair<int, int> { First = rootElement.ActorId, Second = rootElement.AffectedBookId };

            IEnumerable<IMREQUEST> newImRequestElements =
                from elementToAdd in
                    actorsRoleMarginReqOfficePair.Except(mroElementsPair, new PairComparer<int, int>())
                select
                    new IMREQUEST
                    {
                        MroId = elementToAdd.First,
                        MroBookId = elementToAdd.Second,
                        IdPr = m_IdPr,
                        ParentId = 0,
                        IsGrossMargining = false

                    };

            return newImRequestElements;
        }

        /// <summary>
        /// Met à jour les valeurs de la base de données en exécutant les instructions INSERT, UPDATE ou DELETE et recharge le dataset si nécessaire
        /// </summary>
        /// <param name="dataAdapter"></param>
        /// <param name="dataSetToUpdate"></param>
        /// FI 20200818 [XXXXX] Add
        private void SynchronizeDatabase(DataAdapter dataAdapter, DataSet dataSetToUpdate)
        {
            DateTime dtSys = GetDate();

            DataTable dataTable = dataSetToUpdate.Tables[0];

            foreach (var item in dataTable.Rows.Cast<DataRow>().Where(x => x.RowState == DataRowState.Modified))
            {
                item["IDAUPD"] = this.AppSession .IdA;
                item["DTUPD"] = dtSys;
            }

            foreach (var item in dataTable.Rows.Cast<DataRow>().Where(x => x.RowState == DataRowState.Added))
            {
                item["IDAINS"] = this.AppSession.IdA;
                item["DTINS"] = dtSys;
            }

            dataAdapter.Update(dataSetToUpdate);

        }

        private void UpdateDataTable
            (DataTable pDataTable,
            int pCssId, DateTime pBusinessDate,
            IEnumerable<IMREQUEST> pToUpdateImRequestElements,
            IEnumerable<IMREQUEST> pNewImRequestElements,
            IEnumerable<IMREQUEST> pObsoleteImRequestElements)
        {

            foreach (IMREQUEST element in pToUpdateImRequestElements)
            {
                UpdateIMRequestElement(element, pDataTable);
            }

            foreach (IMREQUEST newElement in pNewImRequestElements)
            {
                CreateIMRequestElement(newElement, pCssId, pBusinessDate, pDataTable);
            }

            foreach (IMREQUEST obsoleteElement in pObsoleteImRequestElements)
            {
                DeleteIMRequestElement(obsoleteElement, pDataTable);
            }

        }

        private void CreateIMRequestElement(IMREQUEST pElement, int pCssId, DateTime pBusinessDate, DataTable pTableToUpdate)
        {
            DataRow newRow = pTableToUpdate.NewRow();

            if (pElement.MroId != 0)
            {
                newRow["IDA_MRO"] = pElement.MroId;
            }

            if (pElement.MroId != 0)
            {
                newRow["IDB_MRO"] = pElement.MroBookId;
            }

            newRow["IDA_ENTITY"] = this.m_EntityId;
            newRow["IDA_CSS"] = pCssId;
            newRow["DTBUSINESS"] = pBusinessDate;

            //newRow["IMTIMING"]  = ReflectionTools.GetCustomAttributeValue<XmlEnumAttribute>
            //    (typeof(SettlSessIDEnum), Enum.GetName(typeof(SettlSessIDEnum), this.m_Timing), true);
            newRow["IMTIMING"] = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(this.m_Timing);

            // FI 20200818 [XXXXX] Alimentation de DTSTART et DTEND avec NULL
            newRow["DTSTART"] = DBNull.Value;
            newRow["DTEND"] = DBNull.Value;
            newRow["STATUS"] = ProcessStateTools.StatusUnknown;
            newRow["ISGROSSMARGINING"] = pElement.IsGrossMargining;


            if (this.m_IdPr.HasValue)
            {
                newRow["IDPR"] = this.m_IdPr.Value;
            }

            pTableToUpdate.Rows.Add(newRow);
        }

        private DataRow GetRow(int pMroId, int pMroBookId, DataTable pTableToUpdate, bool pCheckDeleted)
        {
            DataRow resRow = null;

            switch (this.m_SvrTyp)
            {
                case DbSvrType.dbORA:

                    resRow = (
                        from row in pTableToUpdate.Rows.Cast<DataRow>()
                        where
                            ((!pCheckDeleted) || row.RowState != DataRowState.Deleted) &&
                            pMroId == (!Convert.IsDBNull(row["IDA_MRO"]) ? (long)row["IDA_MRO"] : 0)
                            && pMroBookId == (!Convert.IsDBNull(row["IDB_MRO"]) ? (long)row["IDB_MRO"] : 0)
                        select row
                        ).First();

                    break;

                case DbSvrType.dbSQL:
                default:

                    resRow = (
                        from row in pTableToUpdate.Rows.Cast<DataRow>()
                        where
                            ((!pCheckDeleted) || row.RowState != DataRowState.Deleted) &&
                            pMroId == (!Convert.IsDBNull(row["IDA_MRO"]) ? (decimal)row["IDA_MRO"] : 0)
                            && pMroBookId == (!Convert.IsDBNull(row["IDB_MRO"]) ? (decimal)row["IDB_MRO"] : 0)
                        select row
                        ).First();


                    break;
            }

            return resRow;
        }

        private void UpdateIMRequestElement(IMREQUEST pElement, DataTable pTableToUpdate)
        {
            DataRow rowToUpdate = GetRow(pElement.MroId, pElement.MroBookId, pTableToUpdate, false);

            // FI 20200818 [XXXXX] Alimentation de DTSTART et DTEND, IDT, IDPR avec NULL
            rowToUpdate["DTSTART"] = DBNull.Value;
            rowToUpdate["DTEND"] = DBNull.Value;
            rowToUpdate["IDT"] = DBNull.Value;
            rowToUpdate["STATUS"] = ProcessStateTools.StatusUnknown;
            rowToUpdate["IDPR"] = DBNull.Value;
            if (this.m_IdPr.HasValue)
                rowToUpdate["IDPR"] = this.m_IdPr.Value;
        }

        private void DeleteIMRequestElement(IMREQUEST pElement, DataTable pTableToUpdate)
        {

            DataRow rowToDelete = GetRow(pElement.MroId, pElement.MroBookId, pTableToUpdate, true);

            rowToDelete.Delete();

        }

        private void BuildMarginReqOfficeStatusDictionary
            (int pCssId,
            IEnumerable<IMREQUEST> pImRequestdElements)
        {


            m_MarginReqOfficeStatus.Add(

                pCssId,

                pImRequestdElements.

                GroupBy(group => new { ActorID = group.MroId, BookId = group.MroBookId })

                .ToDictionary(
                    groupedElements => new Pair<int, int>
                    {
                        First = groupedElements.Key.ActorID,
                        Second = groupedElements.Key.BookId
                    },

                    groupedElements => groupedElements.First(),

                    new PairComparer<int, int>()
               )
            );
        }
        private void FinalizeImRequestHeader(DataTable pTableToUpdate, ProcessStateTools.StatusEnum pStatus)
        {
            // PM 20150122 [] Ajout condition pour éviter une erreur sur First
            if ((pTableToUpdate != null) && (pTableToUpdate.Rows != null) && (pTableToUpdate.Rows.Count > 0))
            {
                DataRow rowToUpdate = (
                    from row in pTableToUpdate.Rows.Cast<DataRow>()
                    where
                        Convert.IsDBNull(row["IDA_MRO"])
                        && Convert.IsDBNull(row["IDB_MRO"])
                    select row
                    ).First();

                rowToUpdate["STATUS"] = System.Enum.GetName(typeof(ProcessStateTools.StatusEnum), pStatus);
                // FI 20200818 [XXXXX] Alimentation de DTSTART ou DTEND
                if (pStatus == ProcessStateTools.StatusEnum.PROGRESS)
                    rowToUpdate["DTSTART"] = GetDate();
                else
                    rowToUpdate["DTEND"] = GetDate();
            }
        }
        private void FinalizeImRequestElements(int pCssId, DataTable pTableToUpdate, ProcessStateTools.StatusEnum pStatus)
        {
            IEnumerable<KeyValuePair<Pair<int, int>, IMREQUEST>> elementsClearingHouse = this.m_MarginReqOfficeStatus[pCssId];

            foreach (KeyValuePair<Pair<int, int>, IMREQUEST> keyValuePair in elementsClearingHouse)
            {
                Pair<int, int> key = keyValuePair.Key;

                IMREQUEST element = keyValuePair.Value;

                DataRow rowToUpdate = GetRow(key.First, key.Second, pTableToUpdate, false);

                rowToUpdate["ISGROSSMARGINING"] = element.IsGrossMargining;

                rowToUpdate["STATUS"] = System.Enum.GetName(typeof(ProcessStateTools.StatusEnum), pStatus);

                if (element.ParentId > 0)
                {
                    rowToUpdate["IDA_MRO_IMREQUEST"] = element.ParentId;
                }

                if (element.TradeId > 0)
                {
                    rowToUpdate["IDT"] = element.TradeId;
                }

                rowToUpdate["DTSTART"] = element.Start;

                rowToUpdate["DTEND"] = element.End;
            }
        }
    }
}