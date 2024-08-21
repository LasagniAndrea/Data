namespace EFS.SpheresIO.MarketData.T7RDF
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    using FixMLv50SP1Enum = FixML.v50SP1.Enum;
    
    using EFS.ACommon;
    using EFS.ApplicationBlocks.Data;
    using EFS.ApplicationBlocks.Data.Extension;
    using EFS.Common;
    using EFS.Common.IO;
    using EFS.Common.Log;
    using EFS.LoggerClient;
    using EFS.LoggerClient.LoggerService;
    using EFS.Process;
    //using EFS.SpheresIO.MarketData.T7RDF.v10; // Gestion de T7 Release10. il faudra remplacer ce using lors de l'implementation de la prochaine Release de T7     
    using EFS.SpheresIO.MarketData.T7RDF.v12_5; // Gestion de T7 Release12.5 il faudra remplacer ce using lors de l'implementation de la prochaine Release de T7     
    using EfsML;

    /// <summary>
    ///  Importation Fichier RDF
    /// </summary>
    internal class MarketDataImportT7RDF2 : MarketDataImportBase
    {
        #region Constants
        const int _MaxLogRow = 100;
        #endregion Constants

        #region Members
        /// <summary>
        /// Indicateur d'action en cas de fichier sans données
        /// </summary>
        private readonly Cst.IOReturnCodeEnum _RetCodeOnNoData = Cst.IOReturnCodeEnum.NA;
        /// <summary>
        /// Indicateur d'action en cas de non modification de données
        /// </summary>
        private readonly Cst.IOReturnCodeEnum _RetCodeOnNoDataModif = Cst.IOReturnCodeEnum.NA;
        /// <summary>
        /// Fichier deserializé
        /// </summary>
        private FIXML _T7RDFFixMLFile;
        /// <summary>
        /// Date business en paramètre de la tâche IO
        /// </summary>
        private readonly DateTime _BusinessDate;
        /// <summary>
        /// Date business précédente
        /// </summary>
        private DateTime _CheckBusinessDate;
        /// <summary>
        /// Indicateur de reformattage du fichier en sortie
        /// </summary>
        private readonly bool _IsFileToReformat;
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Private Class
        /// <summary>
        /// Permet la lecture des symbols des DeriativeContracts à partir d'un IDataReader
        /// </summary>
        private sealed class DerivativeContractSymbolReader : IReaderRow
        {
            #region Methods
            /// <summary>
            /// Fournit l'ensemble des symbols des DerivativeContracts d'un marché valide à une date
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pDtBusiness"></param>
            /// <param name="pIso10383_Alpha4"></param>
            /// <returns></returns>
            public static IEnumerable<string> GetContractSymbol(string pCS, DateTime pDtBusiness, string pIso10383_Alpha4)
            {
                IEnumerable<string> ret = default;

                QueryParameters qryParameters = GetQueryParameters_DerivativeContractSymbol(pCS, pDtBusiness, pIso10383_Alpha4);

                // FI 20220201 [25699] using
                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter()))
                {
                    ret = (from contractSymbol
                                         in dr.DataReaderEnumerator<string, DerivativeContractSymbolReader>()
                           select contractSymbol).ToList();
                }

                return ret;
            }

            /// <summary>
            /// Requête de selection du symbol de tous les DC ayant déja été négocié
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pDtBusiness"></param>
            /// <param name="pIso10383_Alpha4"></param>
            /// <returns></returns>
            /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
            private static QueryParameters GetQueryParameters_DerivativeContractSymbol(string pCS, DateTime pDtBusiness, string pIso10383_Alpha4)
            {
                string query = String.Format(@"select dc.CONTRACTSYMBOL
                from dbo.DERIVATIVECONTRACT dc
                inner join dbo.MARKET mk on (mk.IDM  = dc.IDM)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
                inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
                inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
                where (mk.ISO10383_ALPHA4 = @ISO10383_ALPHA4) and 
                ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTBUSINESS)) and ({0}) and ({1})

                union

                select dc_unl.CONTRACTSYMBOL
                from dbo.DERIVATIVECONTRACT dc_unl
                inner join dbo.MARKET mk_unl on (mk_unl.IDM  = dc_unl.IDM)
                inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDC = dc_unl.IDDC)
                inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
                inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDDERIVATIVEATTRIB = da_unl.IDDERIVATIVEATTRIB)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDASSET = asset_unl.IDASSET)
                inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
                inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
                where (mk_unl.ISO10383_ALPHA4 = @ISO10383_ALPHA4) and (dc_unl.CATEGORY = 'F') and 
                ((ma_unl.DELIVERYDATE is null) or (ma_unl.DELIVERYDATE >= @DTBUSINESS)) and ({2}) and ({3})",
                OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk", "DTBUSINESS"),
                OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTBUSINESS"),
                OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk_unl", "DTBUSINESS"),
                OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc_unl", "DTBUSINESS"));

                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4), pIso10383_Alpha4);

                QueryParameters qryParameters = new QueryParameters(pCS, query, dataParameters);
                return qryParameters;
            }
            #endregion Methods

            #region IReaderRow
            /// <summary>
            /// Data Reader permettant de lire les enregistrement
            /// </summary>
            public IDataReader Reader { get; set; }

            /// <summary>
            /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (Contract Symbol)
            /// </summary>
            /// <returns>Un objet représentant l'enregistrement lu</returns>
            public object GetRowData()
            {
                string ret = default;
                if (null != Reader)
                {
                    ret = Reader.GetString(0);
                }
                return ret;
            }
            #endregion IReaderRow
        }
        #endregion Private Class

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pRetCodeOnNoData"></param>
        /// <param name="pRetCodeOnNoDataModif"></param>
        public MarketDataImportT7RDF2(SpheresIO.Task pTask, string pDataName, string pDataStyle, Cst.IOReturnCodeEnum pRetCodeOnNoData, Cst.IOReturnCodeEnum pRetCodeOnNoDataModif)
            : base(pTask, pDataName, pDataStyle, true, true, AssetFindMaturityEnum.MATURITYMONTHYEAR, true, true, true, true)
        {
            _RetCodeOnNoData = pRetCodeOnNoData;
            _RetCodeOnNoDataModif = pRetCodeOnNoDataModif;
            _BusinessDate = DateTime.MaxValue;
            _IsFileToReformat = false;
            // lecture de la date en paramètre de la tâche IO
            if (pTask.IoTask.parameters.Contains("DTBUSINESS"))
            {
                DateTime.TryParse(pTask.IoTask.parameters["DTBUSINESS"], out _BusinessDate);
            }
            if (pTask.IoTask.parameters.Contains("REFORMAT"))
            {
                Boolean.TryParse(pTask.IoTask.parameters["REFORMAT"], out _IsFileToReformat);
            }
        }
        #endregion Constructor

        #region override Methods
        /// <summary>
        /// Requête de vérification de l'utilisation d'un DC uniquement à partir de son symbol
        /// </summary>
        protected override void SetQueryExistDCInTrade()
        {
            m_QueryExistDCInTrade = QueryExistDCInTrades(task.Cs, false, false, false, false, m_IsUseISO10383_ALPHA4);
        }
        #endregion override Methods

        #region Methods
        /// <summary>
        /// Méthode principale d'importation d'un fichier T7 RDF FixML
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int ImportT7RDFFile()
        {
            bool isRootElementOk = false;
            int lineNumber = 0;
            XmlSerializer serializer = new XmlSerializer(typeof(FIXML));
            //
            OpenInputFileName();
            //
            //_T7RDFFixMLFile = (FIXML)serializer.Deserialize(IOTools.StreamReader);
            _T7RDFFixMLFile = (FIXML)serializer.Deserialize(IOCommonTools.StreamReader);
            if (_T7RDFFixMLFile != default(FIXML))
            {
                if (_T7RDFFixMLFile.Items != default(object[]))
                {
                    isRootElementOk = true;
                    foreach (Batch_t batch in _T7RDFFixMLFile.Items.Where(x => x.GetType().Equals(typeof(Batch_t))))
                    {
                        lineNumber += ProcessBatch(batch);
                    }
                    //
                    if (lineNumber == 0)
                    {
                        ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
                        
                        LogLevelEnum logLevel = LogLevelEnum.None;
                        switch (_RetCodeOnNoDataModif)
                        {
                            case Cst.IOReturnCodeEnum.ERROR:
                                status = ProcessStateTools.StatusErrorEnum;
                                logLevel = LogLevelEnum.Error;
                                break;
                            case Cst.IOReturnCodeEnum.WARNING:
                                status = ProcessStateTools.StatusWarningEnum;
                                logLevel = LogLevelEnum.Warning;
                                break;
                        }
                        // FI 20200623 [XXXXX] SetErrorWarning
                        task.Process.ProcessState.SetErrorWarning(status);

                        
                        Logger.Log(new LoggerData(logLevel, new SysMsgCode(SysCodeEnum.LOG, 6032), 2));
                    }
                }
            }
            //
            if (_IsFileToReformat)
            {
                //IOTools.OpenFile(dataName + "Reformated.xml", Cst.WriteMode.WRITE);
                //serializer.Serialize(IOTools.StreamWriter, _T7RDFFixMLFile);
                IOCommonTools.OpenFile(dataName + "Reformated.xml", Cst.WriteMode.WRITE);
                serializer.Serialize(IOCommonTools.StreamWriter, _T7RDFFixMLFile);
            }
            //
            CloseAllFiles();
            //
            _T7RDFFixMLFile = default;
            //
            if (false == isRootElementOk)
            {
                
                LogLevelEnum logLevel = LogLevelEnum.None;
                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
                string messageTexte = "<b>Empty file or element FIXML or Batch not found in file.</b>";
                switch (_RetCodeOnNoData)
                {
                    case Cst.IOReturnCodeEnum.ERROR:
                        status = ProcessStateTools.StatusErrorEnum;
                        logLevel = LogLevelEnum.Error;
                        break;
                    case Cst.IOReturnCodeEnum.WARNING:
                        status = ProcessStateTools.StatusWarningEnum;
                        logLevel = LogLevelEnum.Warning;
                        break;
                }

                // FI 20200623 [XXXXX] SetErrorWarning
                task.Process.ProcessState.SetErrorWarning(status);

                
                Logger.Log(new LoggerData(logLevel, messageTexte, 2));
            }
            return lineNumber;
        }

        /// <summary>
        /// Traitement de l'élément Batch du fichier
        /// </summary>
        /// <param name="pBatch"></param>
        /// <returns></returns>
        private int ProcessBatch(Batch_t pBatch)
        {
            int dataCount = 0;
            if (pBatch != default(Batch_t))
            {
                if (pBatch.Message != default(Abstract_message_t[]))
                {
                    IEnumerable<MarketDefinition_message_t> mkDef = pBatch.Message.Where(x => x.GetType().Equals(typeof(MarketDefinition_message_t))).Cast<MarketDefinition_message_t>();
                    IEnumerable<SecurityDefinition_message_t> secdef = pBatch.Message.Where(x => x.GetType().Equals(typeof(SecurityDefinition_message_t))).Cast<SecurityDefinition_message_t>();


                    // Prendre les IDs de tous les marchés dans la business date effective correspond à la date en paramètre de la tâche IO
                    IEnumerable<string> marketIDs = mkDef.Where(m => m.EfctvBizDt == _BusinessDate).Select(m => m.MktID).Distinct().OrderBy(m => m);

                    foreach (string market in marketIDs)
                    {
                        IEnumerable<MarketDefinition_message_t> mktDef = mkDef.Where(m => m.MktID == market);
                        dataCount += ProcessMarket(mktDef, secdef, market);
                        //
                        // Log Asset with missing data
                        AddLogForMissingData(market);
                    }
                }
            }
            return dataCount;
        }

        /// <summary>
        /// Traitement d'un marché
        /// </summary>
        /// <param name="pMktDef"></param>
        /// <param name="pSecDef"></param>
        /// <param name="pMarketId">Marché à traiter</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int ProcessMarket(IEnumerable<MarketDefinition_message_t> pMktDef, IEnumerable<SecurityDefinition_message_t> pSecDef, string pMarketId)
        {
            int dataCount = 0;
            //
            // Recherche de la date business à laquelle considérer les assets valides à mettre à jour
            QueryParameters qryParameters = GetQueryParameters_CurrentBusinessDate(Cs, pMarketId);
            object objPreviousDate = DataHelper.ExecuteScalar(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (objPreviousDate != null)
            {
                _CheckBusinessDate = Convert.ToDateTime(objPreviousDate);
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("Updating valid assets at {0:d}.", _CheckBusinessDate), 3));
                //
                //Recherche des DC pouvant être impactés
                IEnumerable<string> contractSymbols = DerivativeContractSymbolReader.GetContractSymbol(task.Cs, _CheckBusinessDate, pMarketId).OrderBy(s => s);
                if (contractSymbols != default(IEnumerable<string>))
                {
                    List<Task<int>> allTaskList = new List<Task<int>>();
                    foreach (string contract in contractSymbols)
                    {
                        List<Task<int>> updTaskList = ProcessContract(pMktDef, pSecDef, pMarketId, contract);
                        allTaskList.AddRange(updTaskList);
                    }
                    try
                    {
                        // Attendre que toutes les maj des ASSET_ETD aient eu lieu
                        Task.WaitAll(allTaskList.ToArray());
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                    dataCount += allTaskList.Count;
                }
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                task.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, string.Format("<b>No business date found for market {0}.</b>", pMarketId), 2));
            }
            return dataCount;
        }

        /// <summary>
        /// Traitement d'un contrat
        /// </summary>
        /// <param name="pMktDef"></param>
        /// <param name="pSecDef"></param>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        // FI 20220201 [25699] Add contractID and t7InstrumentID
        private List<Task<int>> ProcessContract(IEnumerable<MarketDefinition_message_t> pMktDef, IEnumerable<SecurityDefinition_message_t> pSecDef, string pMarketId, string pContractSymbol)
        {
            List<Task<int>> updTaskList = new List<Task<int>>();
            int dataCount = 0;

            // Log
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Processing contract: {0}", pContractSymbol), 3));

            // Recherche l'élément MarketDefinition correspondant au DerivativeContract
            MarketDefinition_message_t marketMsg = pMktDef.FirstOrDefault(m => m.MktSeg == pContractSymbol);
            if (marketMsg != default(MarketDefinition_message_t))
            {
                string marketSegId = marketMsg.MktSegID;
                // Recherche des éléments SecurityDefinition rattachés à l'élément MarketDefinition du DC
                IEnumerable<SecurityDefinition_message_t> secDef = pSecDef.Where(s => (s.MktSegGrp != default(MarketSegmentGrp_Block_t))
                    && (s.MktSegGrp.MktSegID == marketSegId)
                    && (s.Instrmt != default(Instrument_Block_t)));
                // Prendre tous les Instrument_Block des SecurityDefinition rattachés à l'élément MarketDefinition du DC
                IEnumerable<Instrument_Block_t> instrmtBlock =
                    from sec in secDef
                    where ((sec.Instrmt.ProdCmplx == ProductComplexEnum.SimpleInstrument) || (sec.Instrmt.ProdCmplx == ProductComplexEnum.FlexibleInstrument))
                    select sec.Instrmt;
                //
                // Lecture de tous les assets ETD rattachés au DC et encore valide la veille
                QueryParameters qryParameters = GetQueryParameters_Asset(Cs, _CheckBusinessDate, pMarketId, pContractSymbol);
                DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                //
                if (dt.Rows.Count > 0)
                {
                    PutOrCallEnum putCall = PutOrCallEnum.Call;
                    foreach (DataRow dr in dt.Rows)
                    {
                        int idAsset = Convert.ToInt32(dr["IDASSET"]);
                        string attribute = Convert.ToString(dr["CONTRACTATTRIBUTE"]);
                        string category = Convert.ToString(dr["CATEGORY"]);
                        string maturityMMY = Convert.ToString(dr["MATURITYMONTHYEAR"]);
                        string settlMethod = Convert.ToString(dr["SETTLTMETHOD"]);
                        // FI 20190226 [24473] 
                        // FI 20190402 [24473]  test sur ctrMult supprimé
                        // 
                        //decimal ctrMult = Convert.ToDecimal(dr["FACTOR"]);
                        //decimal ctrMult = Convert.ToDecimal(dr["CONTRACTMULTIPLIER"]);
                        string cfiCode = Convert.ToString(dr["CFICODE"]);
                        string isinCode = Convert.ToString(dr["ISINCODE"]);

                        string contractID = Convert.ToString(dr["MARKETASSIGNEDID"]);
                        // FI 20221014 [XXXXX] lecture de dtMaturity
                        DateTime dtMaturity = DateTime.MinValue;
                        if (false == Convert.IsDBNull(dr["MATURITYDATESYS"]))
                            dtMaturity =Convert.ToDateTime(dr["MATURITYDATESYS"]);
                        // FI 20221014 [XXXXX] Add isYYYYMMDD
                        Boolean isYYYYMMDD = MaturityHelper.IsInputInFixFormat(maturityMMY, Cst.MaturityMonthYearFmtEnum.YearMonthDay);
                        List<Tuple<string, string>> lstOrderCode = new List<Tuple<string, string>>();
                        for (int i = 1; i < 2; i++)
                        {
                            if (StrFunc.IsFilled(Convert.ToString(dr[$"OTHER{i}TYPECODE"])) && StrFunc.IsFilled(Convert.ToString(dr[$"OTHER{i}CODE"])))
                                lstOrderCode.Add(new Tuple<string, string>(Convert.ToString(dr[$"OTHER{i}TYPECODE"]), Convert.ToString(dr[$"OTHER{i}CODE"])));
                        }

                        string EurexInstrmtIDValue = ReflectionTools.ConvertEnumToString<FixMLv50SP1Enum.SecurityIDSourceEnum>(FixMLv50SP1Enum.SecurityIDSourceEnum.EurexInstrmtID);
                        string t7InstrumentID = string.Empty;
                        Tuple<String, String> t7InstrumentIDTuple = lstOrderCode.Where(x => x.Item1 == EurexInstrmtIDValue).FirstOrDefault();
                        if (t7InstrumentIDTuple != default(Tuple<String, String>))
                            t7InstrumentID = t7InstrumentIDTuple.Item2;

                        Instrument_Block_t instrmt = default;
                        bool putCallSpecified = (false == Convert.IsDBNull(dr["PUTCALL"]));
                        if ((category == "O") && putCallSpecified)
                        {
                            decimal strike = Convert.ToDecimal(dr["STRIKEPRICE"]);
                            ExerciseStyleEnum exerStyle = (ExerciseStyleEnum)ReflectionTools.EnumParse(new ExerciseStyleEnum(), Convert.ToString(dr["EXERCISESTYLE"]));
                            putCall = (PutOrCallEnum)ReflectionTools.EnumParse(putCall, Convert.ToString(dr["PUTCALL"]));

                            // Recherche des informations pour un Asset Option
                            // Spheres recherche l'entrée telle que (asset_ETD.MATURITYMONTHYEAR == ContractDate (prioritaire) ou asset_ETD.MATURITYDATESYS == ContractDate) 
                            instrmt = instrmtBlock.FirstOrDefault(i => (i.OptAt == attribute)
                                    && (i.CntrctDtSpecified &&
                                        ((isYYYYMMDD && i.CntrctDt == new DtFunc().StringyyyyMMddToDateTime(maturityMMY)) || (DtFunc.IsDateTimeFilled(dtMaturity) && i.CntrctDt == dtMaturity) ))
                                    && (i.SettlMeth == settlMethod)
                                    && i.PutCallSpecified && (i.PutCall == putCall)
                                    && i.StrkPxSpecified && (i.StrkPx == strike)
                                    && (i.ExerStyle == exerStyle));

                            // Si l'étapé précédente n'a pas aboutie 2ème tentative (ancien code).
                            // Peut être utile dans le cas extrême où le format est YYYYMM et dtMaturity est inconnu  (où encore si CntrctDt ne serait pas spécifié)
                            if (default(Instrument_Block_t) == instrmt)
                            {
                                instrmt = instrmtBlock.FirstOrDefault(i => (i.OptAt == attribute)
                                       && (i.MMY == maturityMMY)
                                       && (i.SettlMeth == settlMethod)
                                       && i.PutCallSpecified && (i.PutCall == putCall)
                                       && i.StrkPxSpecified && (i.StrkPx == strike)
                                       && (i.ExerStyle == exerStyle));
                            }
                        }
                        else
                        {
                            // Recherche des informations pour un Asset Future
                            // Spheres recherche l'entrée telle que (asset_ETD.MATURITYMONTHYEAR == ContractDate (prioritaire) ou asset_ETD.MATURITYDATESYS == ContractDate) 
                            instrmt = instrmtBlock.FirstOrDefault(i => (i.SecTyp == SecurityTypeEnum.FUT)
                            && (i.CntrctDtSpecified &&
                               ((isYYYYMMDD && i.CntrctDt == new DtFunc().StringyyyyMMddToDateTime(maturityMMY)) || (DtFunc.IsDateTimeFilled(dtMaturity) && i.CntrctDt == dtMaturity)))
                            && (i.SettlMeth == settlMethod)
                            && ((i.OptAt == attribute) || (StrFunc.IsEmpty(i.OptAt) && (attribute == "0"))));

                            // Si l'étapé précédente n'a pas aboutie 2ème tentative (ancien code).
                            // Peut être utile dans le cas extrême où le format est YYYYMM et dtMaturity est inconnu  (où encore si CntrctDt ne serait pas spécifié)
                            if (default(Instrument_Block_t) == instrmt)
                            {
                                instrmt = instrmtBlock.FirstOrDefault(i => (i.SecTyp == SecurityTypeEnum.FUT)
                                && (i.MMY == maturityMMY)
                                && (i.SettlMeth == settlMethod)
                                && ((i.OptAt == attribute) || (StrFunc.IsEmpty(i.OptAt) && (attribute == "0"))));
                            }
                        }
                        //
                        if (instrmt != default(Instrument_Block_t))
                        {
                            bool isUpd = false;

                            string newCFI = string.Empty;
                            string newISIN = string.Empty;
                            string newContratID = string.Empty;
                            string newT7InstrumetID = string.Empty;

                            SecAltIDGrp_Block_t isinSource = instrmt.AID.FirstOrDefault(a => a.AltIDSrcSpecified && (a.AltIDSrc == SecurityAltIDSourceEnum.ISIN));
                            if (isinSource != default(SecAltIDGrp_Block_t))
                            {
                                if (isinCode != isinSource.AltID)
                                {
                                    isUpd = true;
                                    newISIN = isinSource.AltID;
                                }
                            }
                            if (cfiCode != instrmt.CFI)
                            {
                                isUpd = true;
                                newCFI = instrmt.CFI;
                            }

                            SecAltIDGrp_Block_t contractIDSource = instrmt.AID.FirstOrDefault(a => a.AltIDSrcSpecified && (a.AltIDSrc == SecurityAltIDSourceEnum.MarketplaceAssignedIdentifier));
                            if (contractIDSource != default(SecAltIDGrp_Block_t))
                            {
                                if (contractID != contractIDSource.AltID)
                                {
                                    isUpd = true;
                                    newContratID = contractIDSource.AltID;
                                }
                            }

                            if (t7InstrumentID != instrmt.ID)
                            {
                                isUpd = true;
                                newT7InstrumetID = instrmt.ID;
                            }

                            if (isUpd)
                            {
                                dataCount += 1;
                                // Mise à jour de ASSET_ETD en asynchrone
                                //Task<int> updAssetTask = Task<int>.Run(() => UpdateAssetETD(idAsset, newISIN, newCFI));
                                Task<int> updAssetTask = UpdateAssetETDAsync(idAsset, newISIN, newCFI, newContratID, newT7InstrumetID);
                                updTaskList.Add(updAssetTask);
                            }
                        }
                    }
                }
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("{0} assets processed.", dataCount), 3));
            }
            else
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("Contract {0} not found in file.", pContractSymbol), 3));
            }
            return updTaskList;
        }

        /// <summary>
        /// Mise à jour de IsinCode et/ou du CfiCode d'un asset ETD en asynchrone
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIsinCode"></param>
        /// <param name="pCfiCode"></param>
        /// <param name="pContractIdentier"></param>
        /// <param name="pT7InstrumentID"></param>

        /// <returns></returns>
        /// FI 20220201 [25699] Add pContractIdentier, pT7InstrumentID
        private async Task<int> UpdateAssetETDAsync(int pIdAsset, string pIsinCode, string pCfiCode, string pContractIdentier, string pT7InstrumentID)
        {
            return await Task<int>.Run(() => UpdateAssetETD(pIdAsset, pIsinCode, pCfiCode, pContractIdentier, pT7InstrumentID));
        }
        
        /// <summary>
        /// Requête de recherche de la date business à laquelle considérer les assets valides à mettre à jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIso10383_Alpha4"></param>
        /// <returns>QueryParameters de recherche de la date business à laquelle considérer les assets valides à mettre à jour</returns>
        private static QueryParameters GetQueryParameters_CurrentBusinessDate(string pCS, string pIso10383_Alpha4)
        {
            string query =
                @"select em.DTENTITY DTSTART
                    from dbo.MARKET m
                   inner join dbo.ENTITYMARKET em on (em.IDM = m.IDM)
                   where (m.ISO10383_ALPHA4 = @ISO10383_ALPHA4)";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4), pIso10383_Alpha4);

            QueryParameters qryParameters = new QueryParameters(pCS, query, dataParameters);
            return qryParameters;
        }

        /// <summary>
        /// Requête de recherche des assets valides à une date par rapport à un marché et un derivativecontract
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIso10383_Alpha4"></param>
        /// <param name="pContractSymbol"></param>
        /// <returns>QueryParameters de recherche des assets valides à une date par rapport à un marché et un derivativecontract</returns>
        private static QueryParameters GetQueryParameters_Asset(string pCS, DateTime pDtBusiness, string pIso10383_Alpha4, string pContractSymbol)
        {
            // FI 20220201 [25699] add MARKETASSIGNEDID, OTHERXTYPECODE, OTHERXCODE
            string query =
                $@"select a.IDASSET, a.CATEGORY, a.CONTRACTATTRIBUTE, a.SETTLTMETHOD, a.EXERCISESTYLE, a.MATURITYMONTHYEAR, a.MATURITYDATESYS, 
a.PUTCALL, a.STRIKEPRICE, a.CONTRACTMULTIPLIER, a.FACTOR, 
a.CFICODE, a.ISINCODE, 
a.MARKETASSIGNEDID, 
a.OTHER1TYPECODE, a.OTHER1CODE, 
a.OTHER2TYPECODE, a.OTHER2CODE 
from dbo.VW_ASSET_ETD_EXPANDED a
inner join dbo.MARKET m on (m.IDM = a.IDM)
where (m.ISO10383_ALPHA4 = @ISO10383_ALPHA4)
and (a.CONTRACTSYMBOL = @CONTRACTSYMBOL)
and ({OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", "DTBUSINESS")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4), pIso10383_Alpha4);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTSYMBOL), pContractSymbol);

            QueryParameters qryParameters = new QueryParameters(pCS, query, dataParameters);
            return qryParameters;
        }

        /// <summary>
        /// Requête de recherche des assets négociés en {pDtBusinnes} sans Code ISIN ou sans CFICode ou sans MARKETASSIGNEDID
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIso10383_Alpha4"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_AssetMissingData(string pCS, DateTime pDtBusiness, string pIso10383_Alpha4)
        {
            // FI 20220201 [25699] add MARKETASSIGNEDID
            string query = $@"select distinct m.IDENTIFIER as MARKETIDENTIFIER, a.IDENTIFIER as ASSETIDENTIFIER, a.CONTRACTIDENTIFIER, 
            a.CATEGORY, a.CONTRACTATTRIBUTE, a.MATURITYMONTHYEAR, a.PUTCALL, a.STRIKEPRICE,
            a.FACTOR, a.SETTLTMETHOD, a.EXERCISESTYLE, a.ISINCODE, a.CFICODE , a.MARKETASSIGNEDID 
            from dbo.VW_ASSET_ETD_EXPANDED a
            inner join dbo.MARKET m on (m.IDM = a.IDM)
            inner join dbo.TRADE t on (t.IDASSET = a.IDASSET) and (t.DTBUSINESS = @DTBUSINESS)
            where (m.ISO10383_ALPHA4 = @ISO10383_ALPHA4) and ((a.ISINCODE is null) or (a.CFICODE is null) or (a.MARKETASSIGNEDID is null)) and ({OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", "DTBUSINESS")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4), pIso10383_Alpha4);

            QueryParameters qryParameters = new QueryParameters(pCS, query, dataParameters);
            return qryParameters;
        }

        /// <summary>
        /// Mise à jour de ISINCODE, CFICODE, MARKETASSIGNEDID, OTHERXTYPECODE, OTHERXCODE d'un asset ETD
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIsinCode"></param>
        /// <param name="pCfiCode"></param>
        /// <param name="pContractIdentier"></param>
        /// <param name="pT7InstrumentID"></param>
        /// <returns></returns>
        /// FI 20220201 [25699] Refactoring
        private int UpdateAssetETD(int pIdAsset, string pIsinCode, string pCfiCode, string pContractIdentier, string pT7InstrumentID)
        {
            string query = @"
                update dbo.ASSET_ETD
                   set #Column#
                 where IDASSET = @IDASSET";

            string column = string.Empty;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);

            if (StrFunc.IsFilled(pIsinCode))
            {
                dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ISINCODE), pIsinCode);
                column += "ISINCODE=@ISINCODE,";
            }
            if (StrFunc.IsFilled(pCfiCode))
            {
                dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CFICODE), pCfiCode);
                column += "CFICODE=@CFICODE,";
            }
            if (StrFunc.IsFilled(pContractIdentier))
            {
                dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.MARKETASSIGNEDID), pContractIdentier);
                column += "MARKETASSIGNEDID=@MARKETASSIGNEDID,";
            }
            if (StrFunc.IsFilled(pT7InstrumentID))
            {
                /* Alimentation des colonnes OTHER1TYPECODE,OTHER1CODE ou OTHER2TYPECODE,OTHER2CODE  
                   Priorité à OTHER1TYPECODE,OTHER1CODE
                   si  OTHER1TYPECODE et OTHER2TYPECODE sont null => Maj des colonnes OTHER1TYPECODE et OTHER1CODE
                   si  OTHER1TYPECODE et OTHER2TYPECODE sont déjà renseignés avec des valeurs autres que EurexInstrmtID => pas de Maj
                   si  OTHER1TYPECODE et OTHER2TYPECODE sont déjà renseignés avec EurexInstrmtID => Maj des colonnes OTHER1CODE et OTHER2CODE
                 */
                dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), pT7InstrumentID);
                
                string EurexInstrmtIDValue = ReflectionTools.ConvertEnumToString<FixMLv50SP1Enum.SecurityIDSourceEnum>(FixMLv50SP1Enum.SecurityIDSourceEnum.EurexInstrmtID);
                column += $@"
OTHER1TYPECODE = case when ((OTHER1TYPECODE is null and isnull(OTHER2TYPECODE,'N/A') != '{EurexInstrmtIDValue}')) then '{EurexInstrmtIDValue}'
				 else OTHER1TYPECODE
				 end, 
OTHER1CODE = case when ((OTHER1TYPECODE is null and isnull(OTHER2TYPECODE,'N/A') != '{EurexInstrmtIDValue}') or (OTHER1TYPECODE='{EurexInstrmtIDValue}')) then @IDENTIFIER
				else OTHER1CODE
				end,
OTHER2TYPECODE = case when (((OTHER2TYPECODE is null and isnull(OTHER1TYPECODE,'N/A') != '{EurexInstrmtIDValue}')) and (not(OTHER1TYPECODE is null and OTHER2TYPECODE is null))) then '{EurexInstrmtIDValue}'
				 else OTHER2TYPECODE
				 end, 
OTHER2CODE = case when (((OTHER2TYPECODE is null and isnull(OTHER1TYPECODE,'N/A') != '{EurexInstrmtIDValue}') or (OTHER2TYPECODE='{EurexInstrmtIDValue}')) and (not(OTHER1TYPECODE is null and OTHER2TYPECODE is null))) then @IDENTIFIER
			else OTHER2CODE
			end,";
            }

            if (StrFunc.IsEmpty(column))
                throw new InvalidOperationException("column is empty");

            column = StrFunc.Before(column, ",", OccurenceEnum.Last);


            query = query.Replace("#Column#", column);

            QueryParameters qryParameters = new QueryParameters(Cs, query, dataParameters);
            return DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

        }

        /// <summary>
        /// Recherche et log des assets négociés en date business et qui n'ont pas de code ISIN ou de code CFI ou de ContractID
        /// </summary>
        /// <param name="pMarketId"></param>
        /// FI 20220201 [25699] Refactoring
        private void AddLogForMissingData(string pMarketId)
        {

            // Lecture de tous les assets ETD rattachés au DC et encore valide la veille
            QueryParameters qryParameters = GetQueryParameters_AssetMissingData(Cs, _CheckBusinessDate, pMarketId);
            DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            Dictionary<string, LogAssetInfoMissing> dicLog = new Dictionary<string, LogAssetInfoMissing>
                {
                    { "CFICODE", new LogAssetInfoMissing() { ColumnDesc = "CFI Code" } },
                    { "ISINCODE", new LogAssetInfoMissing() { ColumnDesc = "ISIN Code" } },
                    { "MARKETASSIGNEDID", new LogAssetInfoMissing() { ColumnDesc = "ContractID" } }
                };

            if (dt.Rows.Count > 0)
            {
                DataRow[] rows = dt.Rows.Cast<DataRow>().ToArray();
                string marketIdentifier = rows.Select(x => x["MARKETIDENTIFIER"].ToString()).First();

                foreach (string Key in dicLog.Keys)
                {
                    DataRow[] rowsMissing = rows.Where(x => x[Key] == Convert.DBNull).ToArray();
                    dicLog[Key].NbMissing = rowsMissing.Count();
                    dicLog[Key].Message = ArrFunc.Map<DataRow, string>(rowsMissing.Take(_MaxLogRow).ToArray(), (x) =>
                    {
                        string assetIdentifier = Convert.ToString(x["ASSETIDENTIFIER"]);
                        return $"{Cst.CrLf}- {assetIdentifier}";
                    });

                    if (dicLog[Key].NbMissing > 0)
                    {
                        string messageTitle = $"Market: <b>{marketIdentifier}</b>, <b>{dicLog[Key].ColumnDesc}</b> is missing on <b>{dicLog[Key].NbMissing}</b> ETD assets traded on <b>{DtFunc.DateTimeToString(_CheckBusinessDate, DtFunc.FmtShortDate)}</b>. You can check and optionally set them manually through the View of ETD <b>Assets</b>.";
                        StringBuilder message = new StringBuilder(messageTitle);
                        if (dicLog[Key].NbMissing >= _MaxLogRow)
                        {
                            message.Append($"{Cst.CrLf}Below the first {_MaxLogRow} assets without {dicLog[Key].ColumnDesc} (Asset identifier):");
                        }
                        else
                        {
                            message.Append($"{Cst.CrLf}Below the {dicLog[Key].NbMissing} assets without {dicLog[Key].ColumnDesc} (Asset identifier):");
                        }
                        message.Append(dicLog[Key].Message.Aggregate((a, b) => $"{a}{b}"));

                        
                        Logger.Log(new LoggerData(LogLevelEnum.None, message.ToString(), 2));
                    }
                }
            }
        }
        #endregion Methods
    }
    /// <summary>
    /// 
    /// </summary>
    /// FI 20220201 [XXXXX]
    internal class LogAssetInfoMissing
    {
        public string ColumnDesc;
        public string[] Message;
        public int NbMissing;
    }

}
