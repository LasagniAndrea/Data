using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.Enum;
//
using EfsML.Business;
//
using FixML.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
//

namespace EFS.SpheresRiskPerformance.EntityMarket
{
    /// <summary>
    /// Markets dictionary collection relative to a specific Spheres entity (key => market identifier).
    /// </summary>
    [XmlRoot(ElementName = "EntityMarketsDictionary")]
    public class MarketsDictionary : SerializableDictionary<string, EntityMarketWithCSS>, IXmlSerializable
    {
        string m_Entity = String.Empty;

        /// <summary>
        ///  Entity identifier connected to the markets list
        /// </summary>
        public string Entity
        {
            get { return m_Entity; }
            private set { m_Entity = value; }
        }

        int m_EntityId;

        /// <summary>
        /// Internal id of the entity
        /// </summary>
        public int EntityId
        {
            get { return m_EntityId; }
            private set { m_EntityId = value; }
        }


        SerializableDictionary<int, DateTime> m_BusinessDateByCss = new SerializableDictionary<int, DateTime>("CssId", "BusinessDate");

        /// <summary>
        /// Business date collection by Css
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public SerializableDictionary<int, DateTime> BusinessDateByCss
        {
            get { return m_BusinessDateByCss; }
            set { m_BusinessDateByCss = value; }
        }

        SerializableDictionary<int, DateTime> m_MarketDateByCss = new SerializableDictionary<int, DateTime>("CssId", "MarketDate");

        /// <summary>
        /// Market date collection by Css
        /// </summary>
        /// PM 20150505 [20575] Add MarketDateByCss
        [ReadOnly(true)]
        [Browsable(false)]
        public SerializableDictionary<int, DateTime> MarketDateByCss
        {
            get { return m_MarketDateByCss; }
            set { m_MarketDateByCss = value; }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX]
        SetErrorWarning m_SetErrorWarning;

        /// <summary>
        /// Get an empty market dictionary
        /// </summary>
        public MarketsDictionary()
            : base("MarketIdentifier", "MarketDatas")
        { }

        /// <summary>
        /// Load the list of the markets connected to the actor entity parameter
        /// </summary>
        /// <param name="pCS">the DB connection string</param>
        /// <param name="pIdAEntity">the id of the entity</param>
        /// <param name="pCssIds">collection containing all the clearing house internal Ids</param>
        public void BuildDictionary(string pCS, int pIdAEntity, object[] pCssIds)
        {
            if (pIdAEntity <= 0)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, new ArgumentException("SYS-01019", "pId"));
            }

            this.Clear();

            SQL_Actor sqlEntity = new SQL_Actor(pCS, pIdAEntity);

            if (sqlEntity.IsLoaded)
            {

                this.EntityId = sqlEntity.Id;
                this.Entity = sqlEntity.Identifier;

                BuildDictionary(pCS, pCssIds);

            }
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void BuildDictionary(string pCS, object[] pCssIds)
        {
            IDbConnection connection = null;

            try
            {
                connection = DataHelper.OpenConnection(pCS);

                object[][] valuesMatrix = 
                {
                    pCssIds
                };

                string request = DataContractHelper.GetQuery(DataContractResultSets.ENTITYMARKETWITHCSS, valuesMatrix);

                Dictionary<string, object> parameterValues = new Dictionary<string, object>
                {
                    { "IDA", this.EntityId }
                };

                List<EntityMarketWithCSS> entitymarkets =
                    DataHelper<EntityMarketWithCSS>.ExecuteDataSet(
                        connection,
                        DataContractHelper.GetType(DataContractResultSets.ENTITYMARKETWITHCSS),
                        request,
                        DataContractHelper.GetDbDataParameters(DataContractResultSets.ENTITYMARKETWITHCSS, parameterValues));

                if (!ArrFunc.IsEmpty(entitymarkets))
                {
                    foreach (EntityMarketWithCSS entitymarket in entitymarkets)
                    {

                        string key = entitymarket.Market;

                        if (!String.IsNullOrEmpty(key) && !this.ContainsKey(key))
                        {
                            this.Add(key, entitymarket);
                        }
                        else
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1020), 0,
                                new LogParam(Entity),
                                new LogParam(EntityId)));
                        }
                    }
                }
                else
                {
                    string message = String.Format(
                                Ressource.GetString("RiskPerformance_ERRMarketsCollectionEmpty"), this.Entity, this.EntityId);

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, message);
                }
            }
            finally
            {
                if (connection != null)
                    DataHelper.CloseConnection(connection);
            }
        }

        /// <summary>
        /// Get the list of the CSS internal IDs
        /// </summary>
        /// <returns>The list of the CSS</returns>
        /// <remarks>the currrent service version, called V1, will allow to treat just a css by instance process</remarks>
        public IEnumerable<int> GetCSSInternalIDs()
        {
            return (from entitymarket in this.Values select entitymarket.CssId).Distinct();
        }

        /// <summary>
        /// Get the ENTITYMARKET Date for the input clearing house
        /// </summary>
        /// <param name="pIdACss">clearing house internal id</param>
        /// <returns>the ENTITYMARKET Date for the clearing house</returns>
        public DateTime GetBusinessDate(int pIdACss)
        {
            return this.BusinessDateByCss[pIdACss];
        }

        /// <summary>
        /// Fournit la date Market courante de ENTITYMARKET pour la clearing house donnée
        /// </summary>
        /// <param name="pIdACss">L'iD interne de la Clearing House</param>
        /// <returns>La date Market courante de ENTITYMARKET pour la clearing house </returns>
        /// PM 20150506 [20575] Add method GetMarketDate
        public DateTime GetMarketDate(int pIdACss)
        {
            return this.MarketDateByCss[pIdACss];
        }
        
        
        /// <summary>
        /// get all the entity/market entries for the given clearing house
        /// </summary>
        /// <param name="pIdACss">the id of the CSS</param>
        /// <returns>an entity/market list</returns>
        public IEnumerable<EntityMarketWithCSS> GetEntityMarkets(int pIdACss)
        {
            return from entitymarket in this.Values where entitymarket.CssId == pIdACss select entitymarket;
        }

        /// <summary>
        /// Return the min Business Date in ENTITYMARKET (all clearing houses concerned)
        /// </summary>
        /// <returns></returns>
        public DateTime GetMinBusinessDate()
        {

            return (from cssDate in this.BusinessDateByCss select cssDate.Value).Min();
        }

        /// <summary>
        /// Init the log delegate method
        /// </summary>
        ///<param name="pSetErrorWarning"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void InitDelegate(SetErrorWarning pSetErrorWarning)
        {
            m_SetErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Validate the loaded date values and match their values with the passed date 
        /// </summary>
        /// <param name="pDateTimeReference">business date extracted from the posted request message</param>
        /// <param name="pMode">traitement modality: SIMULATIOn, NORMAL</param>
        /// <param name="pTiming">traitement timing: EOD, ITD</param>
        /// <returns>
        /// <list type="">
        /// <listheader>return values</listheader>
        /// <item>SUCCESS when the actual data set is good,</item>
        /// <item>MULTIDATAFOUND when wultiple dates are found for a clearing house</item>
        /// <item>DATAREJECTED when the deposit evaluation is IntraDay, 
        /// but the passed date time is EARLIER than one of the clearing house business dates, 
        /// </item>
        /// <item>DATAREJECTED when the deposit evaluation is EndOfDay 
        /// but the passed date time is DIFFERENT at least from one of the clearing house business dates</item>
        /// </list>
        /// </returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel ValidateDateValues(DateTime pDateTimeReference, RiskEvaluationMode pMode, SettlSessIDEnum pTiming)
        {
            Cst.ErrLevel errLevelDate = Cst.ErrLevel.SUCCESS;

            IEnumerable<IGrouping<int, EntityMarketWithCSS>> marketsByCss = this.GroupBy(group => group.Value.CssId, group => group.Value);

            foreach (IGrouping<int, EntityMarketWithCSS> groupByCss in marketsByCss)
            {
                DateTime[] arrayDate = (from groupCss in groupByCss select groupCss.DateBusiness).Distinct().ToArray();

                //PM 20150506 [20575] Add arrayMarketDate
                DateTime[] arrayMarketDate = (from groupCss in groupByCss select groupCss.DateMarket).Distinct().ToArray();

                //if (arrayDate.Length != 1)
                if ((arrayDate.Length != 1) || (arrayMarketDate.Length != 1))
                {
                    string cssIdentifier = groupByCss.First().CssName;

                    string message =
                        String.Format(Ressource.GetString("RiskPerformance_ERRNotUniqueOrNullBusinessDateForCss"), cssIdentifier, groupByCss.Key);

                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, message));

                    errLevelDate = Cst.ErrLevel.MULTIDATAFOUND;
                }
                else
                {
                    DateTime businessDateCss = arrayDate[0];
                    DateTime businessToAdd = DateTime.MaxValue;

                    //PM 20150506 [20575] Add MarketDate
                    DateTime marketDateCss = arrayMarketDate.Where(dtm => dtm <= businessDateCss).DefaultIfEmpty(businessDateCss).Max();
                    DateTime marketDateToAdd = DateTime.MaxValue;

                    bool forceDate = (pMode == RiskEvaluationMode.Simulation);

                    if (forceDate)
                    {
                        businessToAdd = pDateTimeReference;
                        //PM 20150506 [20575] Add MarketDate
                        marketDateToAdd = pDateTimeReference;
                    }
                    else
                    {
                        bool forceForIntraDay = (pTiming == SettlSessIDEnum.Intraday && pDateTimeReference > businessDateCss);

                        if (forceForIntraDay)
                        {
                            businessToAdd = pDateTimeReference;
                            //PM 20150506 [20575] Add MarketDate
                            marketDateToAdd = pDateTimeReference;
                        }

                        bool errorForIntraDay = (pTiming == SettlSessIDEnum.Intraday && pDateTimeReference < businessDateCss);

                        if (errorForIntraDay)
                        {
                            string cssIdentifier = groupByCss.First().CssName;

                            string message =
                                String.Format(Ressource.GetString("RiskPerformance_ERREarlierBusinessDateForIntraDay"),
                                cssIdentifier, groupByCss.Key, pTiming);

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Error, message));

                            errLevelDate = Cst.ErrLevel.DATAREJECTED;
                        }

                        bool useCssBusinessDateForIntraday = (pTiming == SettlSessIDEnum.Intraday && pDateTimeReference == businessDateCss);

                        if (useCssBusinessDateForIntraday)
                        {
                            businessToAdd = businessDateCss;
                            //PM 20150506 [20575] Add MarketDate
                            marketDateToAdd = marketDateCss;
                        }

                        bool errorForEndOfDay = (pTiming == SettlSessIDEnum.EndOfDay && pDateTimeReference != businessDateCss);

                        if (errorForEndOfDay)
                        {
                            string cssIdentifier = groupByCss.First().CssName;

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1024), 0,
                                new LogParam(cssIdentifier),
                                new LogParam(Convert.ToString(groupByCss.Key)),
                                new LogParam(System.Enum.GetName(typeof(SettlSessIDEnum), pTiming))));

                            errLevelDate = Cst.ErrLevel.DATAREJECTED;
                        }

                        bool useCssBusinessDateForEndoFDay = (pTiming == SettlSessIDEnum.EndOfDay && pDateTimeReference == businessDateCss);

                        if (useCssBusinessDateForEndoFDay)
                        {
                            businessToAdd = businessDateCss;
                            //PM 20150506 [20575] Add MarketDate
                            marketDateToAdd = marketDateCss;
                        }
                    }

                    this.BusinessDateByCss.Add(groupByCss.Key, businessToAdd);
                    //PM 20150506 [20575] Add MarketDate
                    this.MarketDateByCss.Add(groupByCss.Key, marketDateToAdd);
                }
            }

            return errLevelDate;
        }

        /// <summary>
        /// Check all the loaded markets, whether they are ready or not to evaluate the deposit at the current business date
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20161021 [22152] Refactoring
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20190717 [24752]  traitement de journée en error
        public Cst.ErrLevel EndOfDayControl(string pCS)
        {
            Cst.ErrLevel errEndOfDay = Cst.ErrLevel.SUCCESS;
            List<string> CssEODError = new List<string>();

            foreach (EntityMarketWithCSS entityMarket in this.Values)
            {
                DataSet ds = PosKeepingTools.GetQueryProcesEndOfDayInSucces(pCS, null, entityMarket.DateBusiness, entityMarket.EntityMarketId);
                if (0 == ds.Tables[0].Rows.Count)
                {
                    if (false == CssEODError.Contains(entityMarket.Css))
                    {
                        CssEODError.Add(entityMarket.Css);
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_SetErrorWarning.Invoke(ProcessStateTools.StatusWarningEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1025), 1,
                            new LogParam(entityMarket.Css),
                            new LogParam(Entity),
                            new LogParam(entityMarket.DateBusiness.ToShortDateString())));
                    }
                }
            }

            return errEndOfDay;
        }

        #region IXmlSerializable Membres

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Read the header of the dictionary collection
        /// </summary>
        /// <param name="reader">reader</param>
        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.Read();

            base.ReadXml(reader);

            reader.ReadStartElement("EntityId");
            this.EntityId = (int)reader.ReadContentAs(typeof(int), null);
            reader.ReadEndElement();

            reader.ReadStartElement("Entity");
            this.Entity = (string)reader.ReadContentAs(typeof(string), null);
            reader.ReadEndElement();
        }

        /// <summary>
        /// Write the header of the dictionary collection
        /// </summary>
        /// <param name="writer">writer</param>
        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Items");
            base.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("EntityId");
            writer.WriteValue(this.m_EntityId);
            writer.WriteEndElement();

            writer.WriteStartElement("Entity");
            writer.WriteValue(this.m_Entity);
            writer.WriteEndElement();
        }

        #endregion


    }
}