using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;

using EfsML.Business;
using EfsML.Enum;

using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OmxShortRiskArray
    {
        #region Members
        /// <summary>
        /// Représente un point
        /// </summary>
        public int Point
        { get; set; }

        /// <summary>
        /// Représente le cours spot du sous-jacent (ex OXMS30)
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        public long Spot
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public long VolDown
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public long VolMarket
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public long VolUp
        { get; set; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Construction pour constitué un point 
        /// <para>volDown, volMarket, volUp sont renseignés à zéro</para>
        /// </summary>
        /// <param name="pPoint"></param>
        /// <param name="pSpot"></param>
        public OmxShortRiskArray(int pPoint, int pSpot)
        {
            this.Point = pPoint;
            this.Spot = pSpot;
            this.VolDown = this.VolMarket = this.VolUp = 0;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// retourne la valeur minimum entre volDown, volMarket, volUp
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        public long GetVolMinValue()
        {
            long ret = System.Math.Min(this.VolDown, this.VolMarket);
            ret = System.Math.Min(ret, this.VolUp);
            return ret;
        }
        #endregion Methods
    }

    /// <summary>
    /// Data container for the OMX Risk Array parameters
    /// <para>Représente un enregistrement ds vector File</para>
    /// <para>The component int the vector file are multiplied by 100 because they are internally treated as integers</para>
    /// <para>In practice we have txo decimals int the calculations</para>
    /// <para>voir doc MargMeth_G_Nordic.pdf  chapitre 3.5 (dispo  trim 31657) </para>
    /// </summary>
    [DataContract(Name = DataHelper<OmxRiskArray>.DATASETROWNAME,
        Namespace = DataHelper<OmxRiskArray>.DATASETNAMESPACE)]
    internal sealed class OmxRiskArray
    {
        #region Members
        /// <summary>
        /// Asset ETD
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset
        { get; set; }

        /// <summary>
        /// Représente un point de la matrice 
        /// <para>Valeurs comprises entre 0 et 31</para>
        /// </summary>
        [DataMember(Name = "POINT", Order = 2)]
        public int Point
        { get; set; }

        /// <summary>
        /// Cours spot du ss-jacent 
        /// </summary>
        [DataMember(Name = "SPOT", Order = 3)]
        public int Spot
        { get; set; }

        /// <summary>
        /// Held Low volatility (Bid Low vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "HELDLOW", Order = 4)]
        public int HeldLow
        { get; set; }

        /// <summary>
        /// Written Low volatility (Ask Low vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "WRITTENLOW", Order = 5)]
        public int WrittenLow
        { get; set; }

        /// <summary>
        /// Held Middle volatility (Bid Mid vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "HELDMIDDLE", Order = 6)]
        public int HeldMiddle
        { get; set; }

        /// <summary>
        /// Written Middle volatility (Ask Mid vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "WRITTENMIDDLE", Order = 7)]
        public int WrittenMiddle
        { get; set; }

        /// <summary>
        /// Held High volatility (Bid High vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "HELDHIGH", Order = 8)]
        public int HeldHigh
        { get; set; }

        /// <summary>
        /// Written High volatility (Ask High vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        [DataMember(Name = "WRITTENHIGH", Order = 9)]
        public int WrittenHigh
        { get; set; }
        #endregion Members

        #region Methods
        /// <summary>
        /// Permet d'obtenir les valeurs de risque sous la forme d'un tableau de int
        /// </summary>
        public int[] ToArray()
        {
            int[] array = new int[6];

            array[0] = HeldLow;
            array[1] = WrittenLow;
            array[2] = HeldMiddle;
            array[3] = WrittenMiddle;
            array[4] = HeldHigh;
            array[5] = WrittenHigh;

            return array;
        }

        /// <summary>
        /// Permet d'obtenir les valeurs de risque sous la forme d'un dictionnaire
        /// </summary>
        /// <returns>Un dictionnaire de valeur de risque</returns>
        public Dictionary<int, decimal> ToDictionary()
        {
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>
            {
                { 1, HeldLow },
                { 2, WrittenLow },
                { 3, HeldMiddle },
                { 4, WrittenMiddle },
                { 5, HeldHigh },
                { 6, WrittenHigh }
            };

            return dic;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class representing the OMX Nordic Risk method
    /// </summary>
    public sealed class RiskMethodOMXRCaR : BaseMethod
    {
        #region Members
        /// <summary>
        /// Liste des matrices par asset
        /// <para>Chaque matrice possède 30 enregistrements par assets</para>
        /// </summary>
        private IEnumerable<OmxRiskArray> _riskArrayParameters = null;

        /// <summary>
        /// Liste des assets
        /// </summary>
        private IEnumerable<AssetExpandedParameter> _assetExpandedParameters = null;
        #endregion Members

        #region Override base accessors
        /// <summary>
        /// Returns the OMX_NORDIC type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.OMX_RCAR; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150511 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.IMOMXVCTFILE_H vct
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = vct.IDASSET)
                     where (vct.DTBUSINESS = @DTBUSINESS)";
                return query;
            }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// No public builder, use the factory method inside the base class
        /// </summary>
        internal RiskMethodOMXRCaR()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region Override base methods
        /// <summary>
        /// Initialize the Custom method, load the parameters for all the contracts in position
        /// </summary>
        /// <param name="pAssetETDCache">collection containing all the assets in position</param>
        /// <param name="pCS">connection string</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            // PM 20150511 [20575] Ajout gestion dtMarket 
            //DateTime dtBusiness = this.DtBusiness.Date;
            DateTime dtBusiness = GetRiskParametersDate(pCS);
            bool isLoadParametersFromfile = (m_IdIOTaskRiskData != 0);
            //
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {

                _assetExpandedParameters = LoadParametersAssetExpanded(connection);

                // PM 20190222 [24326] Add file reader
                if (false == isLoadParametersFromfile)
                {
                    Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
                    {
                        { "DTBUSINESS", dtBusiness }
                    };

                    _riskArrayParameters = LoadParametersMethod<OmxRiskArray>.LoadParameters(connection, dbParametersValue, DataContractResultSets.VCTFILE_OMXMETHOD);
                }
            }
            //
            // PM 20190222 [24326] Add file reader
            if (isLoadParametersFromfile)
            {
                LoadSpecificParametersFromFile(dtBusiness, pAssetETDCache);
            }
        }

        /// <summary>
        /// Reset the Assets and Parameters collections that contributed to evaluate the "OMX Nordic Risk method" risk amount
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            _riskArrayParameters = null;
        }

        /// <summary>
        /// Evaluate a deposit item, according with the parameters of the OMX Nordic method
        /// </summary>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">the positions to evaluate the partial amount for the current deposit item</param>
        /// <param name="opMethodComObj">output value containing all the datas to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository.BuildCustomMarginCalculationMethod"/>) 
        /// in order to build a margin calculation node (type of <see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>the partial amount for the current deposit item</returns>
        /// PM 20160404 [22116] Devient public
        /// FI 20160613 [22256] Modify 
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate (=>  RiskData pRiskDataToEvaluate)
        //public override List<Money> EvaluateRiskElementSpecific(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj)
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = new List<Money>();
            //
            // Creation de l'objet de communication du détail du calcul
            OMXCalcMethCom methodComObj = new OMXCalcMethCom
            {
                // PM 20200910 [25481] Suppression de MarginMethodType remplacé par CalculationMethodType
                //methodComObj.MarginMethodType = this.Type;
                MarginMethodType = this.Type
            };
            opMethodComObj = methodComObj;
            //PM 20150511 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;
            //
            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();
                //
                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;
                    // PM 20190318 [24601] Correction gestion plusieurs devises
                    List<OMXUnderlyingSymbolCom> lstAllSymbol = new List<OMXUnderlyingSymbolCom>();

                    // Group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)
                    positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                    // Ne garder que les positions dont la quantité est différente de 0
                    positions =
                        from pos in positions
                        where pos.Second.Quantity != 0
                        select pos;

                    //GLOP il faudra brancher la reduction de position
                    //positions = positions.ToArray();

                    List<string> lstCurrency = GetDistinctCurrency(positions);
                    foreach (string currency in lstCurrency)
                    {
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        long initialMarginCurrency = 0;

                        // Liste des positions sans vector file
                        List<Pair<PosRiskMarginKey, RiskMarginPosition>> lstAllMissingPos = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();

                        // Liste des symbols
                        // Dans la méthode OMX Nordic, il faut agréger les matrices des assets dont le sous-jacent est identique 
                        List<OMXUnderlyingSymbolCom> lstSymbol = GetDistinctSymbol(positions, currency);
                        // PM 20190318 [24601] Correction gestion plusieurs devises
                        lstAllSymbol.AddRange(lstSymbol);
                        foreach (OMXUnderlyingSymbolCom symbol in lstSymbol)
                        {
                            //liste des positions par symbol
                            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> posSymbol = symbol.Positions;

                            // Constitution d'une matrice vide 
                            List<OmxShortRiskArray> lstShortRiskArray = GetNewShortRiskArray(symbol.Symbol);

                            // Liste des positions du Symbol sans vector
                            List<Pair<PosRiskMarginKey, RiskMarginPosition>> lstMissingPos = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();

                            //
                            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in posSymbol)
                            {
                                bool isbuyer = SideTools.IsFIXmlBuyer(pos.First.Side);
                                bool isSeller = (!isbuyer);

                                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                // EG 20170127 Qty Long To Decimal
                                decimal qty = pos.Second.Quantity;
                                AssetExpandedParameter asset = GetAsset(pos.First.idAsset);
                                int contractMuliplier = Convert.ToInt32(asset.ContractMultiplier);
                                CfiCodeCategoryEnum category = ReflectionTools.ConvertStringToEnum<CfiCodeCategoryEnum>(asset.Category);

                                List<OmxRiskArray> lstRiskArray = GetRiskArray(pos.First.idAsset)
                                    .OrderBy(orderby => orderby.Point).ToList();

                                if (lstRiskArray.Count == 0)
                                {
                                    symbol.Missing = true;
                                    // Ajout des positions à la liste des positions sans Vector
                                    lstMissingPos.Add(pos);
                                }

                                if (lstRiskArray.Count > 0)
                                {
                                    //Spheres® rentre ici s'il existe une matrice pour l'asset ETD 
                                    //(table IMOMXVCTFILE_H alimentée pour cette asset à la date de traitement
                                    for (int i = 0; i < ArrFunc.Count(lstShortRiskArray); i++)
                                    {
                                        //Rq 
                                        //il faut multiplier par (-1) les valeurs de la matrice sur les ventes des futures
                                        //voir doc MargMeth_G_Nordic.pdf chapiter 4.2.3 page 47 (dispo  trim 31657) 
                                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                        // EG 20170127 Qty Long To Decimal
                                        decimal qtySigned = qty;
                                        if ((isSeller) && (category == CfiCodeCategoryEnum.Future))
                                            qtySigned = -1 * qty;

                                        //Rq la règle implémentée ici est valable uniquement sur les futures, les options ne sont pas gérées
                                        //Il faudra revoir le calcul lorsque l'on prendra en considération les options
                                        //En attendant si un jour on se retrouve ici avec un asset option, Spheres® applique la formule
                                        //Risque(i) = qty * stRiskArray[i]* contractMuliplier/100. 
                                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                        long factor = (long)qtySigned * contractMuliplier / 100;

                                        if (isbuyer)
                                        {
                                            // EG 20170127 Qty Long To Decimal
                                            lstShortRiskArray[i].VolDown += factor * lstRiskArray[i].HeldLow;
                                            lstShortRiskArray[i].VolMarket += factor * lstRiskArray[i].HeldMiddle;
                                            lstShortRiskArray[i].VolUp += factor * lstRiskArray[i].HeldHigh;
                                        }
                                        else
                                        {
                                            lstShortRiskArray[i].VolDown += factor * lstRiskArray[i].WrittenLow;
                                            lstShortRiskArray[i].VolMarket += factor * lstRiskArray[i].WrittenMiddle;
                                            lstShortRiskArray[i].VolUp += factor * lstRiskArray[i].WrittenHigh;
                                        }
                                    }
                                }
                            }
                            if (lstMissingPos.Count != 0)
                            {
                                // Retirer les positions sans Vector des positions de l'UnderlyingSymbol courant
                                symbol.Positions = symbol.Positions.Except(lstMissingPos);
                                lstAllMissingPos.AddRange(lstMissingPos);
                            }

                            //initialMarginSymbol représente le risque le risque engendrée par toutes les positions Future/Option sur le même ss-jacent
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            long initialMarginSymbol = MinValue(lstShortRiskArray);
                            initialMarginSymbol = System.Math.Min(0, initialMarginSymbol);
                            symbol.MarginAmount = new Money(System.Math.Abs(initialMarginSymbol), currency);
                            // 
                            initialMarginCurrency += initialMarginSymbol;
                        }

                        if (lstAllMissingPos.Count != 0)
                        {
                            lstSymbol.Add(
                                new OMXUnderlyingSymbolCom
                                {
                                    Symbol = Cst.NotFound,
                                    Positions = lstAllMissingPos
                                });
                        }
                        // PM 20190318 [24601] Correction gestion plusieurs devises
                        //methodComObj.Parameters = lstSymbol.ToArray();
                        //// FI 20160613 [22256] Alimentation de UnderlyingStock (pas de réduction de position)
                        //methodComObj.UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();

                        //Dans Spheres les montants sont positifs
                        riskAmounts.Add(new Money(System.Math.Abs(initialMarginCurrency), currency));
                    }
                    // PM 20190318 [24601] Correction gestion plusieurs devises
                    methodComObj.Parameters = lstAllSymbol.ToArray();
                    methodComObj.UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
                }
            }
            if (riskAmounts.Count == 0)
            {
                // Si aucun montant, créer un montant à zéro
                if (StrFunc.IsEmpty(this.m_CssCurrency))
                {
                    // Si aucune devise de renseignée, utiliser l'euro
                    riskAmounts.Add(new Money(0, "SEK"));
                }
                else
                {
                    riskAmounts.Add(new Money(0, this.m_CssCurrency));
                }
            }
            return riskAmounts;
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset"></param>
        /// <returns>a null collection</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            //IEnumerable<CoverageSortParameters> covParam =
            //from position in pGroupedPositionsByIdAsset
            //join asset in m_AssetExpandedParameters on position.First.idAsset equals (int)asset.AssetId
            //join quote in m_AssetQuoteParameters on asset.AssetId equals quote.AssetId
            //where (quote.AssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract)
            //select
            //new CoverageSortParameters
            //{
            //    AssetId = position.First.idAsset,
            //    ContractId = (int)asset.ContractId,
            //    MaturityYearMonth = decimal.Parse(asset.MaturityYearMonth),
            //    Multiplier = asset.ContractMultiplier,
            //    Quote = quote.Quote,
            //    StrikePrice = asset.StrikePrice,
            //    Type = RiskMethodExtensions.GetTypeFromCategoryPutCall(asset.CategoryEnum, asset.PutOrCall),
            //};
            //return covParam;
            return null;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <returns></returns>
        protected override decimal RoundAmount(decimal pAmount)
        {
            return base.RoundAmount(pAmount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pPrecision"></param>
        /// <returns></returns>
        protected override decimal RoundAmount(decimal pAmount, int pPrecision)
        {
            return base.RoundAmount(pAmount, pPrecision);
        }
        #endregion Override base methods

        #region Methods
        /// <summary>
        /// Chargement des paramètres de calcul à partir des fichiers
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pAssetETDCache"></param>
        // PM 20190222 [24326] Add file reader
        private void LoadSpecificParametersFromFile(DateTime pDtBusiness, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Logger.Write();
                        
            if (pAssetETDCache != default(Dictionary<int, SQL_AssetETD>))
            {
                // Objet qui contiendra les paramètres de calcul lus lors de l'import
                RiskDataLoadNOMX fileCFMData = new RiskDataLoadNOMX(this.Type, pAssetETDCache.Values);

                // Lancement de l'import
                RiskDataImportTask import = new RiskDataImportTask(ProcessInfo.Process, IdIOTaskRiskData);
                import.ProcessTask(pDtBusiness, fileCFMData);

                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1078), 1));

                // Déverser les données dans les classes de calcul
                DumpFileParametersToData(fileCFMData);
            }
        }

        /// <summary>
        /// Déverse les données provenant des fichiers de paramètres dans les classes de paramètres utilisées par le calcul du déposit
        /// </summary>
        /// <param name="pFileData"></param>
        // PM 20190222 [24326] Add file reader
        private void DumpFileParametersToData(RiskDataLoadNOMX pFileData)
        {
            if (pFileData != default(RiskDataLoadNOMX))
            {
                _riskArrayParameters =
                    from mv in pFileData.MarginVector
                    select new OmxRiskArray
                    {
                        IdAsset = mv.IdAsset,
                        Point = mv.Point,
                        Spot = mv.Spot,
                        HeldLow = mv.HeldLow,
                        WrittenLow = mv.WrittenLow,
                        HeldMiddle = mv.HeldMiddle,
                        WrittenMiddle = mv.WrittenMiddle,
                        HeldHigh = mv.HeldHigh,
                        WrittenHigh = mv.WrittenHigh,
                    };
            }
        }

        /// <summary>
        /// Retourne une matrice vide (tous les montants valeurs sont à zéro) avec 31 points 
        /// <para></para>
        /// </summary>
        /// <param name="pSymbol"></param>
        /// <returns></returns>
        private List<OmxShortRiskArray> GetNewShortRiskArray(string pSymbol)
        {
            List<OmxShortRiskArray> ret;
            List<OmxRiskArray> riskArray = GetRiskArray(pSymbol);
            // EG 20160404 Migration vs2013
            //if (true)
            //{
            // lstPoint => au final contient la liste des couples (point,spot) 
            // Toutes les matrices issues des assets associés au symbol {pSymbol} possèdent les mêmes valeurs pour les couples (point,spot) 
            var lstPoint =
                from item in riskArray
                select new
                {
                    item.Point,
                    item.Spot
                };
            lstPoint = lstPoint.Distinct();
            //
            //ret contient une matrice vide constituée de 31 points
            ret = (from item in lstPoint select (new OmxShortRiskArray(item.Point, item.Spot))).ToList();
            //}
            //else
            //{
            //     ret = (
            //        from item in riskArray
            //        group item by new
            //        {
            //            Point = item.point,
            //            Spot = item.Spot
            //        }
            //            into point
            //            select new OmxShortRiskArray(point.Key.Point, point.Key.Spot)
            //        ).ToList();
            //}
            return ret;
        }

        /// <summary>
        /// Retourne les matrices de tous les assets dont le contrat derivé a pour symbol {pSymbol)
        /// <para>Dans le cadre de l'OMX, le symbol du contrat derivé a pour valeur le sous-jacent</para>
        /// <para>Exemple les contrats option et future sur indice OMXS30 ont pour symbol OMXS30</para>
        /// </summary>
        /// <param name="pSymbol"></param>
        /// <returns></returns>
        private List<OmxRiskArray> GetRiskArray(string pSymbol)
        {
            List<OmxRiskArray> ret = (
                from riskArray in _riskArrayParameters
                join asset in _assetExpandedParameters on riskArray.IdAsset equals asset.AssetId
                where (asset.ContractSymbol == pSymbol)
                select riskArray
                ).ToList();
            //
            return ret;
        }

        /// <summary>
        /// Retourne la matrice associée à l'asset {pIdAsset}
        /// </summary>
        /// <param name="pIdAsset">Représente un asset ETD (Option ou Future)</param>
        /// <returns></returns>
        private List<OmxRiskArray> GetRiskArray(int pIdAsset)
        {
            List<OmxRiskArray> ret = (
                from riskArray in _riskArrayParameters
                join asset in _assetExpandedParameters on riskArray.IdAsset equals asset.AssetId
                where (asset.AssetId == pIdAsset)
                select riskArray
                ).ToList();
            //
            return ret;
        }

        /// <summary>
        /// Retourne les différents symbols des contrats dérivés en positions
        /// <para>Les symbols représente le sous jacent</para>
        /// <para>Exemple les futures et option sur indice OMXS30 ont tous pour symbol la valeur OMXS30</para>
        /// </summary>
        /// <param name="positions">Représente les positions</param>
        /// <param name="pCurrency">Filtre sur devise</param>
        /// <returns></returns>
        private List<OMXUnderlyingSymbolCom> GetDistinctSymbol(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions, string pCurrency)
        {
            // liste des symbols
            // Dans la méthode OMX Nordic, il faut agréger les matrices des assets dont le sous-jacent est identique 
            // Il faut aggréger les assets Futures et options
            // Le symbol porte le nom du sous jacent 
            List<OMXUnderlyingSymbolCom> symbol =
                (from pos in positions
                 join asset in _assetExpandedParameters on pos.First.idAsset equals asset.AssetId
                 where asset.Currency == pCurrency
                 group pos by asset.ContractSymbol
                 into posSymbol
                 select new OMXUnderlyingSymbolCom
                    {
                        Symbol = posSymbol.Key,
                        Positions = posSymbol,
                    }).ToList();
            //
            return symbol;
        }

        /// <summary>
        /// Retourne les différents devises des contrats dérivés en positions
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        private List<String> GetDistinctCurrency(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions)
        {
            // liste des symbols
            // Dans la méthode OMX Nordic, il faut agréger les matrices des assets dont le sous-jacent est identique 
            // Il faut aggréger les assets Futures et options
            // Le symbol porte le nom du sous jacent 
            List<string> ret =
                (from pos in positions
                 join asset in _assetExpandedParameters on pos.First.idAsset equals asset.AssetId
                 select asset.Currency).Distinct().ToList();
            //
            return ret;
        }

        /// <summary>
        /// Retourne toutes les positions dont le symbol des contrats dérivés des assets est {pSymbol}
        /// </summary>
        /// <param name="positions">Représente toutes les positions pour tous les DC confondus</param>
        /// <param name="pSymbol">Représentre la valeur du filtre</param>
        /// <returns></returns>
        private IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> GetPositionsSymbol(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions, string pSymbol)
        {
            //liste des positions par symbol
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> posSymbol =
                from pos in positions
                join asset in _assetExpandedParameters on pos.First.idAsset equals asset.AssetId
                where asset.ContractSymbol == pSymbol
                select pos;
            //
            return posSymbol;
        }

        /// <summary>
        /// Retourne la category Future/Option du derivative Contract associé à l'asset
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <returns></returns>
        private AssetExpandedParameter GetAsset(int pIdAsset)
        {
            IEnumerable<AssetExpandedParameter> lstAsset =
                from assetExpandedParameters in _assetExpandedParameters
                where assetExpandedParameters.AssetId == pIdAsset
                select assetExpandedParameters;
            //
            if (lstAsset.Count() == 0)
            {
                throw new InvalidOperationException(StrFunc.AppendFormat("asset (id:{0}) not found", pIdAsset));
            }
            //
            AssetExpandedParameter asset = lstAsset.First();
            //
            return asset;
        }

        /// <summary>
        /// Retourne la valeur minimum présente dans la matrice {pLstShortRiskArray}
        /// </summary>
        /// <param name="pLstShortRiskArray"></param>
        /// <returns></returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        private static long MinValue(List<OmxShortRiskArray> pLstShortRiskArray)
        {
            long ret = 0;
            foreach (OmxShortRiskArray item in pLstShortRiskArray)
            {
                ret = System.Math.Min(ret, item.GetVolMinValue());
            }
            return ret;
        }
        #endregion Methods
    }
}
