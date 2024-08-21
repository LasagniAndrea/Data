using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EfsML.CorporateActions;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.PosRequest;
using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EfsML.ClosingReopeningPositions
{
    // EG 20190308 New
    public sealed partial class ARQTools
    {
        #region ActionRequestReadyStateEnum
        /// <summary>
        /// Etat de publication d'une action
        /// </summary>
        [Serializable()]
        [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public enum ActionRequestReadyStateEnum
        {
            /// <summary>Dépréciée</summary>
            DEPRECATED,
            /// <summary>Regular</summary>
            REGULAR,
            /// <summary>Reservée</summary>
            RESERVED,
            /// <summary>A compléter</summary>
            TOCOMPLETE,
        }
        #endregion ActionRequestReadyStateEnum

        #region ARQWhereMode
        /// <summary>
        /// Type de recherche utilisée pour lecture d'une ARQ dans les tables 
        /// </summary>
        public enum ARQWhereMode
        {
            EFFECTIVEDATE,
            ID,
            IDENTIFIER,
        }
        #endregion ARQWhereMode

        #region ARQReadyState
        public static Nullable<ActionRequestReadyStateEnum> ReadyState(string pValue)
        {
            Nullable<ActionRequestReadyStateEnum> _readyState = null;
            if (System.Enum.IsDefined(typeof(ActionRequestReadyStateEnum), pValue))
                _readyState = (ActionRequestReadyStateEnum)System.Enum.Parse(typeof(ActionRequestReadyStateEnum), pValue, false);
            return _readyState;
        }
        #endregion ARQReadyState

        #region PriceUsed
        /// <summary>
        /// Classe du prix utilisé pour le trade de Fermeture ou réouverture de position
        /// </summary>
        // EG 20190318 New ClosingReopening Step3
        // EG 20190613 [24683] Upd
        public class PriceUsed
        {
            #region Members
            [XmlAttribute(AttributeName = "type")]
            public TransferPriceEnum @type;
            [XmlIgnoreAttribute]
            public bool dtPriceSpecified;
            [System.Xml.Serialization.XmlElementAttribute("date", typeof(EFS_DateTime))]
            public EFS_DateTime dtPrice;
            [XmlIgnoreAttribute]
            public bool priceSpecified;
            [System.Xml.Serialization.XmlElementAttribute("price", typeof(EFS_Decimal))]
            public EFS_Decimal price;
            [XmlAttribute(AttributeName = "codeReturn")]
            public Cst.ErrLevel codeReturn;
            #endregion Members
            #region Constructors
            public PriceUsed()
            {
                codeReturn = Cst.ErrLevel.INITIALIZE;
            }
            public PriceUsed(TransferPriceEnum pType, Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, Cst.ErrLevel pCodeReturn)
            {
                @type = pType;
                priceSpecified = pPrice.HasValue;
                if (priceSpecified)
                {
                    price = new EFS_Decimal
                    {
                        DecValue = pPrice.Value
                    };
                }
                dtPriceSpecified = pDtPrice.HasValue;
                if (dtPriceSpecified)
                {
                    dtPrice = new EFS_DateTime
                    {
                        DateTimeValue = pDtPrice.Value
                    };
                }
                codeReturn = pCodeReturn;
            }
            #endregion Constructors
        }
        #endregion PriceUsed
        #region TradeContextEnum
        /// <summary>
        /// Liste des éléments de trades pour matchage avec contexte
        /// </summary>
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
        public enum TradeContextEnum
        {
            #region Members
            IdA_Entity,
            // EG 20240520 [WI930] Add
            IdA_CssCustodian,
            IdB_Dealer,
            IdB_Clearer,
            IdA_Dealer,
            IdA_Clearer,
            IdGrpBook_Dealer,
            IdGrpBook_Clearer,
            IdGrpActor_Dealer,
            IdGrpActor_Clearer,
            IdDerivativeContract,
            IdCommodityContract,
            IsinUnderlyerContract,
            IdGrpContract,
            IdM,
            IdGrpMarket,
            IdI,
            IdGrpInstr,
            GProduct,
            None,
            #endregion Members
        }
        #endregion TradeContextEnum
        #region TradeContextWeight
        /// <summary>
        /// Poids des contextes de merge
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
        public sealed class TradeContextWeight : Attribute
        {
            #region Accessors
            public TradeContextEnum Name { get; set; }
            public int Weight { get; set; }
            #endregion Accessors
        }
        #endregion TradeContextWeight
        #region TradeContext
        /// <summary>
        /// Poids d'un contexte TRADEARQ : Du plus fort au plus faible
        /// IDA_ENTITY, IDA_CSSCUSTODIAN, BOOK DEALER, BOOK CLEARER, ACTOR_DEALER, ACTOR_CLEARER, GRPBOOK_DEALER, GRPBOOK_CLEARER, GRPACTOR_DEALER, GRPACTOR_CLEARER
        /// CONTRACT, GRPCONTRACT, MARKET, GRPMARKET, INSTR, GRPINSTR, GPRODUCT
        /// </summary>
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Add idA_CssCustodian
        public class TradeContext
        {
            #region Members
            [TradeContextWeight(Weight = 19, Name = TradeContextEnum.IdA_Entity)]
            public int idA_Entity;
            [TradeContextWeight(Weight = 18, Name = TradeContextEnum.IdA_CssCustodian)]
            public int idA_CssCustodian;
            [TradeContextWeight(Weight = 17, Name = TradeContextEnum.IdB_Dealer)]
            public int idB_Dealer;
            [TradeContextWeight(Weight = 16, Name = TradeContextEnum.IdB_Clearer)]
            public int idB_Clearer;
            [TradeContextWeight(Weight = 15, Name = TradeContextEnum.IdA_Dealer)]
            public int idA_Dealer;
            [TradeContextWeight(Weight = 14, Name = ARQTools.TradeContextEnum.IdA_Clearer)]
            public int idA_Clearer;
            [TradeContextWeight(Weight = 13, Name = TradeContextEnum.IdGrpBook_Dealer)]
            public Nullable<int> idGrpBook_Dealer;
            [TradeContextWeight(Weight = 12, Name = TradeContextEnum.IdGrpBook_Clearer)]
            public Nullable<int> idGrpBook_Clearer;
            [TradeContextWeight(Weight = 11, Name = TradeContextEnum.IdGrpActor_Dealer)]
            public Nullable<int> idGrpActor_Dealer;
            [TradeContextWeight(Weight = 10, Name = TradeContextEnum.IdGrpActor_Clearer)]
            public Nullable<int> idGrpActor_Clearer;
            [TradeContextWeight(Weight = 9, Name = TradeContextEnum.IdDerivativeContract)]
            public Nullable<int> idDerivativeContract;
            [TradeContextWeight(Weight = 8, Name = TradeContextEnum.IdCommodityContract)]
            public Nullable<int> idCommodityContract;
            [TradeContextWeight(Weight = 7, Name = TradeContextEnum.IsinUnderlyerContract)]
            public Nullable<int> isinUnderlyerContract;
            [TradeContextWeight(Weight = 6, Name = TradeContextEnum.IdGrpContract)]
            public Nullable<int> idGrpContract;
            [TradeContextWeight(Weight = 5, Name = TradeContextEnum.IdM)]
            public int idM;
            [TradeContextWeight(Weight = 4, Name = TradeContextEnum.IdGrpMarket)]
            public Nullable<int> idGrpMarket;
            [TradeContextWeight(Weight = 3, Name = TradeContextEnum.IdI)]
            public int idI;
            [TradeContextWeight(Weight = 2, Name = TradeContextEnum.IdGrpInstr)]
            public Nullable<int> idGrpInstr;
            [TradeContextWeight(Weight = 1, Name = TradeContextEnum.GProduct)]
            public string gProduct;
            #endregion Members
            #region Constructors
            // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
            public TradeContext(int pIdA_Entity, DataRow pRowTrade)
            {
                // Entity
                idA_Entity = pIdA_Entity;

                // EG 20240520 [WI930] New
                if (false == Convert.IsDBNull(pRowTrade["IDA_CSSCUSTODIAN"]))
                    idA_CssCustodian = Convert.ToInt32(pRowTrade["IDA_CSSCUSTODIAN"]);

                // Dealer
                if (false == Convert.IsDBNull(pRowTrade["IDA_DEALER"]))
                    idA_Dealer = Convert.ToInt32(pRowTrade["IDA_DEALER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDGACTOR_DEALER"]))
                    idGrpActor_Dealer = Convert.ToInt32(pRowTrade["IDGACTOR_DEALER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDB_DEALER"]))
                    idB_Dealer = Convert.ToInt32(pRowTrade["IDB_DEALER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDGBOOK_DEALER"]))
                    idGrpBook_Dealer = Convert.ToInt32(pRowTrade["IDGBOOK_DEALER"]);

                // Clearer
                if (false == Convert.IsDBNull(pRowTrade["IDA_CLEARER"]))
                    idA_Clearer = Convert.ToInt32(pRowTrade["IDA_CLEARER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDGACTOR_CLEARER"]))
                    idGrpActor_Clearer = Convert.ToInt32(pRowTrade["IDGACTOR_CLEARER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDB_CLEARER"]))
                    idB_Clearer = Convert.ToInt32(pRowTrade["IDB_CLEARER"]);
                if (false == Convert.IsDBNull(pRowTrade["IDGBOOK_CLEARER"]))
                    idGrpBook_Clearer = Convert.ToInt32(pRowTrade["IDGBOOK_CLEARER"]);

                // Instrument
                if (false == Convert.IsDBNull(pRowTrade["IDI"]))
                    idI = Convert.ToInt32(pRowTrade["IDI"]);
                // Groupe Instrument
                if (false == Convert.IsDBNull(pRowTrade["IDGINSTR"]))
                    idGrpInstr = Convert.ToInt32(pRowTrade["IDGINSTR"]);
                // gProduct
                if (false == Convert.IsDBNull(pRowTrade["GPRODUCT"]))
                    gProduct = pRowTrade["GPRODUCT"].ToString();

                // Market
                if (false == Convert.IsDBNull(pRowTrade["IDM"]))
                    idM = Convert.ToInt32(pRowTrade["IDM"].ToString());
                // Groupe Market
                if (false == Convert.IsDBNull(pRowTrade["IDGMARKET"]))
                    idGrpMarket = Convert.ToInt32(pRowTrade["IDGMARKET"]);

                // Contract
                if (pRowTrade.Table.Columns.Contains("IDDC") && (false == Convert.IsDBNull(pRowTrade["IDDC"])))
                    idDerivativeContract = Convert.ToInt32(pRowTrade["IDDC"]);
                if (pRowTrade.Table.Columns.Contains("IDCC") && (false == Convert.IsDBNull(pRowTrade["IDCC"])))
                    idCommodityContract = Convert.ToInt32(pRowTrade["IDCC"]);
                if (pRowTrade.Table.Columns.Contains("IDGCONTRACT") && (false == Convert.IsDBNull(pRowTrade["IDGCONTRACT"])))
                    idGrpContract = Convert.ToInt32(pRowTrade["IDGCONTRACT"]);
                if (pRowTrade.Table.Columns.Contains("IDASSET_UNL") && (false == Convert.IsDBNull(pRowTrade["IDASSET_UNL"])))
                    isinUnderlyerContract = Convert.ToInt32(pRowTrade["IDASSET_UNL"]);
            }
            #endregion Constructors
        }
        #endregion TradeContext

        #region TradeContextComparer
        /// <summary>
        /// Comparer de l'application des poids de contexte (via ResultMatching)
        /// </summary>
        public class TradeContextComparer : IComparer<ResultActionRequest>
        {
            public int Compare(ResultActionRequest pResultAction1, ResultActionRequest pResultAction2)
            {
                return pResultAction1.ResultMatching.CompareTo(pResultAction1.ResultMatching);
            }
        }
        #endregion TradeContextComparer

        #region ExActor
        /// <summary>
        /// Acteurs de réouverture de position
        /// Recherche de la correspondance en fonctions des instructions du contexte
        /// Lecture dans table EXTLID et recherche du book de correspondance :
        /// - par ACTEUR ou par BOOK : le 1er BOOK trouvé est sélectionné
        /// </summary>
        // EG 20190613 [24683] Upd
        [XmlType(TypeName = "exActor")]
        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
        public class ExActor
        {
            #region Members
            [XmlAttribute(AttributeName = "idA")]
            public int IdA;
            [XmlIgnoreAttribute]
            public bool IdA_IdentifierSpecified { set; get; }
            [XmlAttribute(AttributeName = "idA_Identifier")]
            public string IdA_Identifier;
            [XmlIgnoreAttribute]
            public bool IdBSpecified { set; get; }
            [XmlAttribute(AttributeName = "idB")]
            public int IdB;
            [XmlIgnoreAttribute]
            public bool IdB_IdentifierSpecified { set; get; }
            [XmlAttribute(AttributeName = "idB_Identifier")]
            public string IdB_Identifier;
            [XmlAttribute(AttributeName = "codeReturn")]
            public Cst.ErrLevel codeReturn;
            #endregion Members
            #region Constructors
            public ExActor() 
            {
                codeReturn = Cst.ErrLevel.INITIALIZE;
            }
            #endregion Constructors
            #region Methods
            #region SetActorForReopening
            // EG 20240520 [WI930] Add ACTOR|BOOK identifiers for Log
            public void SetActorForReopening(EfsMLProcessBase pProcess, LinkActorInstructions pActorInstructions, int pCumIdA, Nullable<int> pCumIdB)
            {
                DataParameters parameters = new DataParameters(new DataParameter[] { });
                parameters.Add(new DataParameter(pProcess.Cs, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), pActorInstructions.table);
                parameters.Add(new DataParameter(pProcess.Cs, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pActorInstructions.column);
                
                string sqlSelect = string.Empty;

                switch (pActorInstructions.table)
                {
                    case "ACTOR":
                        parameters.Add(new DataParameter(pProcess.Cs, "ID", DbType.Int32), pCumIdA);
                        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
                        // Column mapping for CSS
                        if (pActorInstructions.isCssCustodian)
                        {
                            sqlSelect = @"select ac.IDA, ac.IDENTIFIER as ACTOR_IDENTIFIER
                            from dbo.EXTLID ex
                            inner join dbo.ACTOR ac on (ac.IDENTIFIER = ex.VALUE)
                            inner join dbo.CSS css on (css.IDA = ac.IDA)" + Cst.CrLf;
                        }
                        else
                        {
                            sqlSelect = @"select bk.IDA, bk.IDB, ac.IDENTIFIER as ACTOR_IDENTIFIER
                            from dbo.EXTLID ex
                            inner join dbo.ACTOR ac on (ac.IDENTIFIER = ex.VALUE)
                            inner join dbo.BOOK bk on (bk.IDA = ac.IDA)" + Cst.CrLf;
                        }
                        break;
                    case "BOOK":
                        parameters.Add(new DataParameter(pProcess.Cs, "ID", DbType.Int32), pCumIdB);
                        sqlSelect = @"select bk.IDA, bk.IDB, ac.IDENTIFIER as ACTOR_IDENTIFIER, bk.IDENTIFIER as BOOK_IDENTIFIER
                        from dbo.EXTLID ex
                        inner join dbo.BOOK bk on (bk.IDENTIFIER = ex.VALUE)
                        inner join dbo.ACTOR ac on (ac.IDA = bk.IDA)" + Cst.CrLf;
                        break;
                }
                sqlSelect += @"where (ex.TABLENAME = @TABLENAME) and (ex.IDENTIFIER = @IDENTIFIER) and (ex.ID = @ID)";

                QueryParameters qryParameters = new QueryParameters(pProcess.Cs, sqlSelect, parameters);
                DataTable dt = DataHelper.ExecuteDataTable(pProcess.Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                codeReturn = (null != dt) && (0 < dt.Rows.Count) ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.DATANOTFOUND;
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    IdA = Convert.ToInt32(dt.Rows[0]["IDA"]);
                    IdA_IdentifierSpecified = dt.Columns.Contains("ACTOR_IDENTIFIER");
                    if (IdA_IdentifierSpecified)
                        IdA_Identifier = dt.Rows[0]["ACTOR_IDENTIFIER"].ToString();
                    IdBSpecified = dt.Columns.Contains("IDB");
                    if (IdBSpecified)
                        IdB = Convert.ToInt32(dt.Rows[0]["IDB"]);
                    IdB_IdentifierSpecified = dt.Columns.Contains("BOOK_IDENTIFIER");
                    if (IdB_IdentifierSpecified)
                        IdB_Identifier = dt.Rows[0]["BOOK_IDENTIFIER"].ToString();
                }
            }
            #endregion SetActorForReopening
            #endregion Methods
        }
        #endregion ExActor

        #region TradeKeyComparer
        /// <summary>
        /// Comparer de trades par TradeKey
        /// </summary>
        public class TradeKeyComparer : IEqualityComparer<TradeKey>
        {
            #region IEqualityComparer
            /// <summary>
            /// Les TradeKey sont égaux s'ils ont les même:
            /// DEALER  (ACTEUR/BOOK), CLEARER (ACTEUR/BOOK), IDCONTRAT (IDDC/IDCC), IDASSET, SIDE 
            /// </summary>
            /// <param name="x">1er TradeKey à comparer</param>
            /// <param name="y">2ème TradeKey à comparer</param>
            /// <returns>true si x Equals Y, sinon false</returns>
            public bool Equals(TradeKey pTradeKey1, TradeKey pTradeKey2)
            {

                //Vérifier si les objets référencent les même données
                if (Object.ReferenceEquals(pTradeKey1, pTradeKey2)) return true;

                //Vérifier si un des objets est null
                if (pTradeKey1 is null || pTradeKey2 is null)
                    return false;

                // Vérifier qu'il s'agit des même TradeInfo
                // EG 20190318 Add IDEM, Remove Side ClosingReopening Step3
                return (pTradeKey1.IdEM == pTradeKey2.IdEM) &&
                       (pTradeKey1.IdA_Dealer == pTradeKey2.IdA_Dealer) &&
                       (pTradeKey1.IdB_Dealer == pTradeKey2.IdB_Dealer) &&
                       (pTradeKey1.IdA_Clearer == pTradeKey2.IdA_Clearer) &&
                       (pTradeKey1.IdB_Clearer == pTradeKey2.IdB_Clearer) &&
                       (pTradeKey1.IdDC == pTradeKey2.IdDC) &&
                       (pTradeKey1.IdCC == pTradeKey2.IdCC) &&
                       (pTradeKey1.IdAsset == pTradeKey2.IdAsset);
            }

            /// <summary>
            /// La méthode GetHashCode fournissant la même valeur pour des objets Tradeinfo qui sont égaux.
            /// </summary>
            /// <param name="pCombinedCommodity">Le paramètre TradeInfo dont on veut le hash code</param>
            /// <returns>La valeur du hash code</returns>
            // EG 20190318 Add IDEM, Remove Side ClosingReopening Step3
            public int GetHashCode(TradeKey pTradeKey)
            {
                //Vérifier si l'obet est null
                if (pTradeKey is null) return 0;

                int hashIdEM = pTradeKey.IdEM.GetHashCode();
                int hashIdA_Dealer = pTradeKey.IdA_Dealer.GetHashCode();
                int hashIdB_Dealer = pTradeKey.IdB_Dealer.GetHashCode();
                int hashIdA_Clearer = pTradeKey.IdA_Clearer.GetHashCode();
                int hashIdB_Clearer = pTradeKey.IdB_Clearer.GetHashCode();
                int hashIdDC = pTradeKey.IdDC.GetHashCode();
                int hashIdCC = pTradeKey.IdCC.GetHashCode();
                int hashIdAsset = pTradeKey.IdAsset.GetHashCode();

                //Calcul du hash code pour le TradeInfo.
                return (int)(hashIdEM ^ hashIdA_Dealer ^ hashIdB_Dealer ^ hashIdA_Clearer ^ hashIdB_Clearer ^ hashIdDC ^ hashIdCC ^ hashIdAsset);
            }
            #endregion IEqualityComparer

        }
        #endregion TradeInfoComparer

        #region TradeKey
        /// <summary>
        /// Elements constituants la clé identique hors contexte
        /// </summary>
        [XmlType(TypeName = "tradeKey")]
        // EG 20190318 Refactoring ClosingReopening Step3
        // EG 20190613 [24683] Upd
        // EG 20231214 [WI725] Closing/Reopening : Add Category
        // EG 20240520 [WI930] Add IdACss_Custodian|exCss_Custodian|CssCustodianIdentifier
        public class TradeKey
        {
            #region Members
            [XmlAttribute(AttributeName = "idEM")]
            public int IdEM { set; get; }
            [XmlAttribute(AttributeName = "idA_Dealer")]
            public int IdA_Dealer { set; get; }
            [XmlAttribute(AttributeName = "idB_Dealer")]
            public int IdB_Dealer { set; get; }
            [XmlAttribute(AttributeName = "idA_Clearer")]
            public int IdA_Clearer { set; get; }
            [XmlAttribute(AttributeName = "idB_Clearer")]
            public int IdB_Clearer { set; get; }
            [XmlIgnoreAttribute]
            public bool IdA_CssCustodianSpecified { set; get; }
            [XmlAttribute(AttributeName = "idA_CssCustodian")]
            public int IdA_CssCustodian { set; get; }
            [XmlAttribute(AttributeName = "idI")]
            public int IdI { set; get; }
            [XmlIgnoreAttribute]
            public int IdP { set; get; }
            [XmlIgnoreAttribute]
            public bool IdDCSpecified { set; get; }
            [XmlAttribute(AttributeName = "idDC")]
            public int IdDC { set; get; }
            [XmlIgnoreAttribute]
            public bool IdCCSpecified { set; get; }
            [XmlAttribute(AttributeName = "idCC")]
            public int IdCC { set; get; }
            [XmlAttribute(AttributeName = "idAsset")]
            public int IdAsset { set; get; }
            [XmlIgnoreAttribute]
            public DateTime DtOut { set; get; }
            [XmlAttribute(AttributeName = "assetCategory")]
            public Cst.UnderlyingAsset AssetCategory { set; get; }
            [XmlIgnoreAttribute]
            public bool FutValuationMethodSpecified { set; get; }
            [XmlAttribute("futValuationMethod")]
            public FuturesValuationMethodEnum FutValuationMethod { set; get; }
            [XmlIgnoreAttribute]
            public bool CategorySpecified { set; get; }
            [XmlAttribute("category")]
            public CfiCodeCategoryEnum Category { set; get; }
            [XmlIgnoreAttribute]
            public ClosingReopeningAction actionReference;
            [XmlIgnoreAttribute]
            public bool priceUsedForClosingSpecified;
            [XmlElement("priceUsedForClosing")]
            public PriceUsed priceUsedForClosing;
            [XmlIgnoreAttribute]
            public bool priceUsedForReopeningSpecified;
            [XmlElement("priceUsedForReopening")]
            public PriceUsed priceUsedForReopening;
            [XmlIgnoreAttribute]
            public List<SystemMSGInfo> errMsgPrice;

            // Stockage du dealer de réouverture en fonction du contexte
            [XmlIgnoreAttribute]
            public bool exDealerSpecified;
            [XmlElement("exDealer")]
            public ExActor exDealer;

            // Stockage du clearer de réouverture en fonction du contexte
            [XmlIgnoreAttribute]
            public bool exClearerSpecified;
            [XmlElement("exClearer")]
            public ExActor exClearer;

            // Stockage de la chambre de compensation sur la réouverture en fonction du contexte
            [XmlIgnoreAttribute]
            public bool exCssCustodianSpecified;
            [XmlElement("exCssCustodian")]
            public ExActor exCssCustodian;

            // Status de résultat du traitement de calcul de fermeture|réouverture
            [XmlIgnoreAttribute]
            public bool statusCalculationSpecified;
            [XmlElement("calculation")]
            public ProcessState statusCalculation;

            // Status de résultat du traitement d'écriture de fermeture|réouverture
            [XmlIgnoreAttribute]
            public bool statusWritingSpecified;
            [XmlElement("writing")]
            public ProcessState statusWriting;

            // Données de travail
            [XmlIgnoreAttribute]
            public string Dealer_Identifier { set; get; }
            [XmlIgnoreAttribute]
            public string BookDealer_Identifier { set; get; }
            [XmlIgnoreAttribute]
            public string Clearer_Identifier { set; get; }
            [XmlIgnoreAttribute]
            public string BookClearer_Identifier { set; get; }
            [XmlIgnoreAttribute]
            public string CssCustodian_Identifier { set; get; }
            [XmlIgnoreAttribute]
            public SQL_Instrument sqlInstrument;
            [XmlIgnoreAttribute]
            public SQL_Product sqlProduct;
            [XmlIgnoreAttribute]
            public IPosKeepingData posKeepingData;
            #endregion Members
            #region Accessors
            #region DisplayInfo
            [XmlIgnoreAttribute]
            public string DisplayInfo
            {
                get
                {
                    string ret = " Asset : " + Asset.identifier;
                    ret += " - Dealer : " + Dealer_Identifier + "(" + BookDealer_Identifier + ")";
                    ret += " - Clearer : " + Clearer_Identifier + "(" + BookClearer_Identifier + ")";
                    return ret;
                }
            }
            #endregion DisplayInfo

            [XmlIgnoreAttribute]
            // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
            public (int id, string identifier, Cst.UnderlyingAsset underlyer) AssetInfo
            {
                get { return (IdAsset, Asset.identifier, Asset.UnderlyingAsset.Value); }
            }
            [XmlIgnoreAttribute]
            public PosKeepingAsset Asset
            {
                get { return posKeepingData.Asset; }
            }
            [XmlIgnoreAttribute]
            public bool IsTimingStartOfDay
            {
                get
                {
                    return (actionReference.timing == SettlSessIDEnum.StartOfDay);
                }
            }
            [XmlIgnoreAttribute]
            public bool IsTimingEndOfDay
            {
                get
                {
                    return (actionReference.timing == SettlSessIDEnum.EndOfDay);
                }
            }
            [XmlIgnoreAttribute]
            // EG 20240520 [WI930] New
            public bool IsTimingEndOfDayPlusStartOfDay
            {
                get
                {
                    return (actionReference.timing == SettlSessIDEnum.EndOfDayPlusStartOfDay);
                }
            }
            // EG 20231214 [WI725] Closing/Reopening : New
            public bool IsFuturesStyleMarkToMarket
            {
                get { return FuturesValuationMethodEnum.FuturesStyleMarkToMarket == FutValuationMethod; }
            }
            /// <summary>
            /// 
            /// </summary>
            // EG 20231214 [WI725] Closing/Reopening : New
            public bool IsPremiumStyle
            {
                get { return FuturesValuationMethodEnum.PremiumStyle == FutValuationMethod; }
            }
            // EG 20231214 [WI725] Closing/Reopening : New
            public bool IsOption
            {
                get { return FuturesValuationMethodEnum.PremiumStyle == FutValuationMethod; }
            }
            #endregion Accessors
            #region Constructors
            public TradeKey() { }
            // EG 20190613 [24683] Upd
            // EG 20240520 [WI930] Add IdaCssCustodian|CssCustodian_Identifier
            public TradeKey(TradeKey pTradeKey)
                : this(pTradeKey.IdEM, pTradeKey.IdA_Dealer, pTradeKey.IdB_Dealer, pTradeKey.IdA_Clearer, pTradeKey.IdB_Clearer,
                pTradeKey.Dealer_Identifier, pTradeKey.BookDealer_Identifier, pTradeKey.Clearer_Identifier, pTradeKey.BookClearer_Identifier,
                pTradeKey.IdA_CssCustodian, pTradeKey.CssCustodian_Identifier,
                pTradeKey.IdI, pTradeKey.IdP, pTradeKey.IdDC, pTradeKey.IdCC, pTradeKey.AssetCategory, pTradeKey.IdAsset, 
                (pTradeKey.FutValuationMethodSpecified?pTradeKey.FutValuationMethod.ToString():string.Empty))
            {
            }
            // EG 20190613 [24683] Upd
            // EG 20240520 [WI930] Add IdaCssCustodian|CssCustodian_Identifier
            public TradeKey(int pIdEM, int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer, 
                string pDealer_Identifier, string pBookDealer_Identifier, string pClearer_Identifier, string pBookClearer_Identifier,
                int pIdA_CssCustodian, string pCssCustodian_Identifier,
                int pIdI, int pIdP, int pIdDC, int pIdCC, Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pFutValuationMethod)
            {
                IdEM = pIdEM;
                IdI = pIdI;
                IdP = pIdP;
                IdA_Dealer = pIdA_Dealer;
                IdB_Dealer = pIdB_Dealer;
                IdA_Clearer = pIdA_Clearer;
                IdB_Clearer = pIdB_Clearer;
                IdA_CssCustodianSpecified = (0 < pIdA_CssCustodian);
                IdA_CssCustodian = pIdA_CssCustodian;

                Dealer_Identifier = pDealer_Identifier;
                BookDealer_Identifier = pBookDealer_Identifier;
                Clearer_Identifier = pClearer_Identifier;
                BookClearer_Identifier = pBookClearer_Identifier;
                CssCustodian_Identifier = pCssCustodian_Identifier;

                IdDCSpecified = (0 < pIdDC);
                IdDC = pIdDC;
                IdCCSpecified = (0 < pIdCC);
                IdCC = pIdCC;
                AssetCategory = pAssetCategory;
                IdAsset = pIdAsset;
                FutValuationMethodSpecified = StrFunc.IsFilled(pFutValuationMethod);
                if (FutValuationMethodSpecified)
                    FutValuationMethod = ReflectionTools.ConvertStringToEnum<FuturesValuationMethodEnum>(pFutValuationMethod);
            }
            // EG 20190318 Upd ClosingReopening Step3
            // EG 20190613 [24683] Upd
            // EG 20240520 [WI930] Add IdaCssCustodian|CssCustodian_Identifier
            public TradeKey(DataRow pRowTrade)
            {
                IdEM = Convert.ToInt32(pRowTrade["IDEM"]);
                IdI = Convert.ToInt32(pRowTrade["IDI"]);
                IdP = Convert.ToInt32(pRowTrade["IDP"]);
                IdA_Dealer = Convert.ToInt32(pRowTrade["IDA_DEALER"]);
                IdB_Dealer = Convert.ToInt32(pRowTrade["IDB_DEALER"]);
                IdA_Clearer = Convert.ToInt32(pRowTrade["IDA_CLEARER"]);
                IdB_Clearer = Convert.ToInt32(pRowTrade["IDB_CLEARER"]);
                IdA_CssCustodianSpecified = pRowTrade.Table.Columns.Contains("IDA_CSSCUSTODIAN") && (false == Convert.IsDBNull(pRowTrade["IDA_CSSCUSTODIAN"]));
                if (IdA_CssCustodianSpecified)
                {
                    IdA_CssCustodian = Convert.ToInt32(pRowTrade["IDA_CSSCUSTODIAN"]);
                    IdA_CssCustodianSpecified = (0 < IdA_CssCustodian);
                    CssCustodian_Identifier = pRowTrade["CSSCUSTODIAN_IDENTIFIER"].ToString();
                }
                IdDCSpecified = pRowTrade.Table.Columns.Contains("IDDC") && (false == Convert.IsDBNull(pRowTrade["IDDC"]));
                if (IdDCSpecified)
                {
                    IdDC = Convert.ToInt32(pRowTrade["IDDC"]);
                    IdDCSpecified = (0 < IdDC);
                }

                IdCCSpecified = pRowTrade.Table.Columns.Contains("IDCC") && (false == Convert.IsDBNull(pRowTrade["IDCC"]));
                if (IdCCSpecified)
                {
                    IdCC = Convert.ToInt32(pRowTrade["IDCC"]);
                    IdCCSpecified = (0 < IdCC);
                }
                AssetCategory = ReflectionTools.ConvertStringToEnum<Cst.UnderlyingAsset>(pRowTrade["ASSETCATEGORY"].ToString());
                IdAsset = Convert.ToInt32(pRowTrade["IDASSET"]);

                Dealer_Identifier = pRowTrade["DEALER_IDENTIFIER"].ToString();
                BookDealer_Identifier = pRowTrade["BKDEALER_IDENTIFIER"].ToString();
                Clearer_Identifier = pRowTrade["CLEARER_IDENTIFIER"].ToString();
                BookClearer_Identifier = pRowTrade["BKCLEARER_IDENTIFIER"].ToString();

                DtOut = DateTime.MinValue;
                if (pRowTrade.Table.Columns.Contains("DTOUT"))
                    DtOut = Convert.ToDateTime(pRowTrade["DTOUT"]);

                FutValuationMethodSpecified = (pRowTrade.Table.Columns.Contains("FUTVALUATIONMETHOD") && (false == Convert.IsDBNull(pRowTrade["FUTVALUATIONMETHOD"])));
                if (FutValuationMethodSpecified)
                    FutValuationMethod = ReflectionTools.ConvertStringToEnum<FuturesValuationMethodEnum>(pRowTrade["FUTVALUATIONMETHOD"].ToString());

                // EG 20231214 [WI725] Closing/Reopening : New
                CategorySpecified = (pRowTrade.Table.Columns.Contains("CATEGORY") && (false == Convert.IsDBNull(pRowTrade["CATEGORY"])));
                if (CategorySpecified)
                    Category = ReflectionTools.ConvertStringToEnum<CfiCodeCategoryEnum>(pRowTrade["CATEGORY"].ToString());
            }
            #endregion Constructors
            #region Methods
            #region SetPriceUsedForClosingReopening
            /// <summary>
            /// Recherche et stockage du prix utilisé pour le fermeture et réouverture de position
            /// </summary>
            // EG 20190318 New ClosingReopening Step3
            // EG 20190716 [VCL : New FixedIncome] Upd GetQuoteLock
            // EG 20240520 [WI930] Add StatusUnSuccess (if CssCustodianSpecifed)
            public ProcessState SetPriceUsedForClosingReopening(EfsMLProcessBase pProcess)
            {
                ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
                ProcessStateTools.StatusEnum statusUnsuccess = actionReference.Reopening.cssCustodianSpecified ? ProcessStateTools.StatusEnum.ERROR : ProcessStateTools.StatusWarningEnum;
                priceUsedForClosing = SetPriceUsedForClosingReopening(pProcess, actionReference.Closing);
                priceUsedForClosingSpecified = (null != priceUsedForClosing);
                if (priceUsedForClosingSpecified && (Cst.ErrLevel.SUCCESS != priceUsedForClosing.codeReturn))
                    processState.SetErrorWarning(statusUnsuccess, priceUsedForClosing.codeReturn);

                if (actionReference.ReopeningSpecified)
                {
                    priceUsedForReopening = SetPriceUsedForClosingReopening(pProcess, actionReference.Reopening);
                    priceUsedForReopeningSpecified = (null != priceUsedForReopening);
                    if (priceUsedForReopeningSpecified && (Cst.ErrLevel.SUCCESS != priceUsedForReopening.codeReturn))
                        processState.SetErrorWarning(statusUnsuccess, priceUsedForReopening.codeReturn);
                }
                return processState;
            }
            /// <summary>
            /// Recherche et stockage du prix utilisé pour le fermeture et ou réouverture de position
            /// </summary>
            // EG 20190613 [24683] Upd
            // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (FairValuePrice)
            // EG 20240520 [WI930] Add Test IsTimingEndOfDayPlusStartOfDay
            private PriceUsed SetPriceUsedForClosingReopening(EfsMLProcessBase pProcess, PositionInstructions pPositionInstructions)
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                Nullable<decimal> price = null;
                TransferPriceEnum @type = pPositionInstructions.Price.OtherPrice;
                bool isTimingEndOfDay = IsTimingEndOfDay || (IsTimingEndOfDayPlusStartOfDay && (pPositionInstructions is ClosingInstructions));
                if (FutValuationMethodSpecified && (FutValuationMethod == FuturesValuationMethodEnum.PremiumStyle))
                    @type = pPositionInstructions.Price.EqtyPrice;
                else if (FutValuationMethodSpecified && (FutValuationMethod == FuturesValuationMethodEnum.FuturesStyleMarkToMarket))
                    @type = pPositionInstructions.Price.FutPrice;

                SystemMSGInfo errMsgInfo = null;
                Nullable<DateTime> dtPrice = null;
                IPosKeepingMarket market = pProcess.ProcessCacheContainer.GetEntityMarketLock(IdEM);
                QuotationSideEnum quotationSide = QuotationSideEnum.OfficialClose;
                switch (type)
                {
                    case TransferPriceEnum.DayBeforePrice:
                        dtPrice = isTimingEndOfDay ? market.DtMarketPrev : market.DtMarket;
                        break;
                    case TransferPriceEnum.DayPrice:
                        dtPrice = isTimingEndOfDay ? market.DtMarket : market.DtMarketNext;
                        break;
                    case TransferPriceEnum.FairValueDayBeforePrice:
                        quotationSide = QuotationSideEnum.FairValue;
                        dtPrice = isTimingEndOfDay ? market.DtMarketPrev : market.DtMarket;
                        break;
                    case TransferPriceEnum.FairValueDayPrice:
                        quotationSide = QuotationSideEnum.FairValue;
                        dtPrice = isTimingEndOfDay ? market.DtMarket : market.DtMarketNext;
                        break;
                    case TransferPriceEnum.TradingPrice:
                        break;
                    case TransferPriceEnum.Zero:
                        price = 0;
                        break;
                }

                if (dtPrice.HasValue)
                {
                    Quote quote = pProcess.ProcessCacheContainer.GetQuoteLock(IdAsset, dtPrice.Value, null, quotationSide, AssetCategory, new KeyQuoteAdditional(), ref errMsgInfo);
                    if ((null != quote) && quote.valueSpecified)
                    {
                        dtPrice = quote.time;
                        price = quote.value;
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                    }
                }
                PriceUsed priceUsed = new PriceUsed(@type, price, dtPrice, codeReturn);

                // Gestion du log et du retour de traitement en fonction :
                // 1. Cotation non trouvée
                // 2. RequestType
                // 3. Mode 

                if (null != errMsgInfo)
                {
                    // FI 20200623 [XXXXX] Call SetErrorWarning
                    pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    
                    errMsgInfo.processState.Status = ProcessStateTools.StatusErrorEnum;
                    
                    
                    Logger.Log(errMsgInfo.ToLoggerData(3));
                }
                return priceUsed;
            }
            #endregion SetPriceUsedForClosingReopening

            #region SetActorForReopening
            /// <summary>
            /// Recherche et stockage des acteurs utilisés pour la réouverture de position
            /// </summary>
            // EG 20190613 [24683] New
            // EG 20240520 [WI930] Add StatusUnSuccess (if CssCustodianSpecifed)
            public ProcessState SetActorForReopening(EfsMLProcessBase pProcess)
            {
                ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
                if (actionReference.ReopeningSpecified)
                {
                    ReopeningInstructions reopening = actionReference.Reopening;
                    ProcessStateTools.StatusEnum statusUnsuccess = reopening.cssCustodianSpecified ? ProcessStateTools.StatusEnum.ERROR : ProcessStateTools.StatusWarningEnum;
                    LogLevelEnum logLevelUnsuccess = reopening.cssCustodianSpecified ? LogLevelEnum.Error : LogLevelEnum.Warning;

                    if (reopening.dealerSpecified)
                    {
                        exDealer = new ExActor();
                        exDealer.SetActorForReopening(pProcess, reopening.dealer, IdA_Dealer, IdB_Dealer);
                        exDealerSpecified = true;
                        if (Cst.ErrLevel.SUCCESS != exDealer.codeReturn)
                        {
                            processState.SetErrorWarning(statusUnsuccess, Cst.ErrLevel.CLOSINGREOPENINGREJECTED);

                            
                            Logger.Log(new LoggerData(logLevelUnsuccess, new SysMsgCode(SysCodeEnum.SYS, 8850), 3,
                                new LogParam(Ressource.GetString("SelectDealerTransfer")),
                                new LogParam(LogTools.IdentifierAndId(Dealer_Identifier, IdA_Dealer)),
                                new LogParam(LogTools.IdentifierAndId(BookDealer_Identifier, IdB_Dealer))));
                        }
                    }

                    if (reopening.clearerSpecified)
                    {
                        exClearer = new ExActor();
                        exClearer.SetActorForReopening(pProcess, reopening.clearer, IdA_Clearer, IdB_Clearer);
                        exClearerSpecified = true;
                        if (Cst.ErrLevel.SUCCESS != exClearer.codeReturn)
                        {
                            processState.SetErrorWarning(statusUnsuccess, Cst.ErrLevel.CLOSINGREOPENINGREJECTED);

                            
                            Logger.Log(new LoggerData(logLevelUnsuccess, new SysMsgCode(SysCodeEnum.SYS, 8850), 3,
                                new LogParam(Ressource.GetString("SelectClearerOrCustodianTransfer")),
                                new LogParam(LogTools.IdentifierAndId(Clearer_Identifier, IdA_Clearer)),
                                new LogParam(LogTools.IdentifierAndId(BookClearer_Identifier, IdB_Clearer))));
                        }
                    }
                    // EG 20240520 [WI930] New
                    if (reopening.cssCustodianSpecified)
                    {
                        exCssCustodian = new ExActor();
                        exCssCustodian.SetActorForReopening(pProcess, reopening.cssCustodian, IdA_CssCustodian, null);
                        exCssCustodianSpecified = true;
                        if (Cst.ErrLevel.SUCCESS != exCssCustodian.codeReturn)
                        {
                            processState.SetErrorWarning(statusUnsuccess, Cst.ErrLevel.CLOSINGREOPENINGREJECTED);


                            Logger.Log(new LoggerData(logLevelUnsuccess, new SysMsgCode(SysCodeEnum.SYS, 8850), 3,
                                new LogParam(Ressource.GetString("SelectCssCustodianTransfer")),
                                new LogParam(LogTools.IdentifierAndId(CssCustodian_Identifier, IdA_CssCustodian)), new LogParam("-")));
                        }

                    }
                }
                return processState;
            }
            #endregion SetActorForReopening

            #region SetProductInstrument
            /// <summary>
            /// Chargement PRODUCT|INSTRUMENT
            /// </summary>
            // EG 20190613 [24683] New
            public bool SetProductInstrument(string pCS)
            {
                sqlInstrument = new SQL_Instrument(pCS, IdI);
                bool ret = sqlInstrument.IsLoaded;
                if (ret)
                {
                    sqlProduct = new SQL_Product(pCS, sqlInstrument.IdP);
                    ret = sqlProduct.IsLoaded;
                }
                return ret;
            }
            #endregion SetProductInstrument
            #endregion Methods
        }
        #endregion TradeKey
        #region OverrideTradeCandidate
        /// <summary>
        /// Données overridées d'un trade candidat utilisée
        /// pour la création du trade de Fermeture et de réouverture
        /// </summary>
        // EG 20190318 New ClosingReopening Step3
        // EG 20190613 [24683] Upd
        [XmlTypeAttribute("overrideTrade", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public class OverrideTradeCandidate
        {
            [XmlIgnoreAttribute()]
            public string identifierSource;
            [XmlAttribute(AttributeName = "idTSource")]
            public int idTSource;
            [XmlAttribute(AttributeName = "qty")]
            public decimal qty;
            [XmlAttribute(AttributeName = "side")]
            public string side;
            [XmlAttribute(AttributeName = "reverseSide")]
            public bool reverseSide;
            [XmlAttribute(AttributeName = "price")]
            public decimal price;
            [XmlAttribute(AttributeName = "isWriting")]
            public bool isWriting;

            // Trade de fermeuture
            [XmlIgnoreAttribute()]
            public bool idTClosingSpecified;
            [XmlAttribute(AttributeName = "idTClosing")]
            public int idTClosing;
            [XmlIgnoreAttribute()]
            public bool identifierClosingSpecified;
            [XmlAttribute(AttributeName = "identifierClosing")]
            public string identifierClosing;

            [XmlIgnoreAttribute()]
            public bool idEParent_ForOffsettingPositionSpecified;
            [XmlAttribute(AttributeName = "idEParent_OCP")]
            public int idEParent_ForOffsettingPosition;

            // Trade de réouverture
            [XmlIgnoreAttribute()]
            public bool idTReopeningSpecified;
            [XmlAttribute(AttributeName = "idTReopening")]
            public int idTReopening;
            [XmlIgnoreAttribute()]
            public bool identifierReopeningSpecified;
            [XmlAttribute(AttributeName = "identifierReopening")]
            public string identifierReopening;

            [XmlIgnoreAttribute()]
            // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
            public (int id, string identifier) ClosingTradeInfo
            {
                get { return (idTClosing, identifierClosing); }
            }
            [XmlIgnoreAttribute()]
            // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
            public (int id, string identifier) ReopeningTradeInfo
            {
                get { return (idTReopening, identifierReopening); }
            }
            [XmlIgnoreAttribute()]
            public List<TradeCandidate> linkedTrades;
        }
        #endregion OverrideTradeCandidate
        #region TradeCandidate
        /// <summary>
        /// Données d'un trade candidat
        /// </summary>
        // EG 20190318 Upd ClosingReopening Step3
        // EG 20190613 [24683] Upd
        [XmlTypeAttribute("trade", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public class TradeCandidate
        {
            #region Accessors
            [XmlIgnoreAttribute()]
            public TradeKey posKeys;
            [XmlAttribute(AttributeName = "idT")]
            public int idT;
            [XmlAttribute(AttributeName = "qty")]
            public decimal qty;
            [XmlAttribute(AttributeName = "side")]
            [XmlIgnoreAttribute()]
            public DateTime dtExecution;
            public string side;
            [XmlAttribute(AttributeName = "price")]
            public decimal price;
            [XmlIgnoreAttribute()]
            public string identifier;
            [XmlAttribute(AttributeName = "idE_Event")]
            public int idE_Event;

            [XmlIgnoreAttribute()]
            public bool overrideClosingSpecified;
            [XmlElementAttribute("overrideClosing")]
            public OverrideTradeCandidate overrideClosing;
            [XmlIgnoreAttribute()]
            public bool overrideReopeningSpecified;
            [XmlElementAttribute("overrideReopening")]
            public OverrideTradeCandidate overrideReopening;

            [XmlIgnoreAttribute()]
            public TradeContext context;

            [XmlIgnoreAttribute()]
            // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
            public (int id, string identifier) TradeInfo
            {
                get { return (idT, identifier); }
            }
            #endregion Accessors
            #region Constructors
            public TradeCandidate():base()
            {
            }
            public TradeCandidate(DataRow pRowTrade, TradeContext pContext)
            {
                posKeys = new TradeKey(pRowTrade);
                context = pContext;
                idT = Convert.ToInt32(pRowTrade["IDT"]);
                identifier = pRowTrade["IDENTIFIER"].ToString();
                qty = Convert.ToDecimal(pRowTrade["QTY"]);
                side = pRowTrade["SIDE"].ToString();
                price = Convert.ToDecimal(pRowTrade["PRICE"]);
                idE_Event = Convert.ToInt32(pRowTrade["IDE_EVENT"]);
                dtExecution = Convert.ToDateTime(pRowTrade["DTEXECUTION"]);
            }
            public TradeCandidate(TradeCandidate pTradeCandidate)
            {
                posKeys = new TradeKey(pTradeCandidate.posKeys);
                context = pTradeCandidate.context;
                idT = pTradeCandidate.idT;
                identifier = pTradeCandidate.identifier;
                qty = pTradeCandidate.qty;
                dtExecution = pTradeCandidate.dtExecution;
            }
            #endregion Constructors
        }
        #endregion TradeCandidate
    }


    #region ActionRequestQuery
    /// <summary>
    /// Classe de base des Queries et paramètres des actions sur position (insert, update, select, delete)
    /// </summary>
    // EG 20190308 New
    // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
    public partial class ActionRequestQuery
    {
        #region Members
        protected DataParameter paramID;
        protected DataParameter paramIdentifier;
        protected DataParameter paramDisplayName;
        protected DataParameter paramDescription;
        protected DataParameter paramReadyState;
        protected DataParameter paramRequestType;
        protected DataParameter paramTiming;
        protected DataParameter paramEffectiveDate;
        protected DataParameter paramEffectiveEndDate;
        protected DataParameter paramDtIns;
        protected DataParameter paramIdAIns;
        protected DataParameter paramDtUpd;
        protected DataParameter paramIdAUpd;
        protected DataParameter paramExtlLink;
        protected DataParameter paramRowAttribut;
        protected string _CS;
        #endregion Members
        #region Constructor
        public ActionRequestQuery(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Ajout des paramètres commun pour l'insertion et la mise à jour
        /// </summary>
        /// <param name="pParameters"></param>
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramIdentifier);
            pParameters.Add(paramDisplayName);
            pParameters.Add(paramDescription);
            pParameters.Add(paramReadyState);
            pParameters.Add(paramRequestType);
            pParameters.Add(paramTiming);
            pParameters.Add(paramEffectiveDate);
            pParameters.Add(paramEffectiveEndDate);
        }
        #endregion AddParametersInsert
        #region AddParametersInsert
        /// <summary>
        /// Ajout des commun paramètres pour l'insertion
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersInsert(DataParameters pParameters)
        {
            AddParameters(pParameters);
            pParameters.Add(paramDtIns);
            pParameters.Add(paramIdAIns);
            pParameters.Add(paramExtlLink);
            pParameters.Add(paramRowAttribut);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Ajout des paramètres commun pour la mise à jour
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
            pParameters.Add(paramDtUpd);
            pParameters.Add(paramIdAUpd);
        }
        #endregion AddParametersUpdate
        #region AddParametersUpdateStatus
        /// <summary>
        /// Ajout des paramètres pour la mise à jour du status d'intégration
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersUpdateStatus(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramReadyState);
            //pParameters.Add(paramEmbeddedState);
            pParameters.Add(paramDtUpd);
            pParameters.Add(paramIdAUpd);
        }
        #endregion AddParametersUpdateStatus
        #region GetQueryUpdateStatus
        public virtual QueryParameters GetQueryUpdateStatus()
        {
            return null;
        }
        #endregion GetQueryUpdateStatus

        #region GetQueryExist
        public virtual QueryParameters GetQueryExist(ARQTools.ARQWhereMode pWhereMode)
        {
            return null;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        public virtual QueryParameters GetQueryDelete()
        {
            return null;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        public virtual QueryParameters GetQuerySelect(ARQTools.ARQWhereMode pWhereMode)
        {
            return null;
        }
        #endregion GetQuerySelect
        #region GetQuerySelectCandidate
        public virtual QueryParameters GetQuerySelectCandidate()
        {
            return null;
        }
        #endregion GetQuerySelectCandidate

        #region GetQueryInsert
        public virtual QueryParameters GetQueryInsert()
        {
            return null;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        public virtual QueryParameters GetQueryUpdate()
        {
            return null;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres communs
        /// </summary>
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        private void InitParameter()
        {
            paramID = new DataParameter(_CS, "IDARQ", DbType.Int32);
            paramIdentifier = new DataParameter(_CS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
            paramDisplayName = new DataParameter(_CS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN);
            paramDescription = new DataParameter(_CS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN);
            paramReadyState = new DataParameter(_CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramRequestType = new DataParameter(_CS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramTiming = new DataParameter(_CS, "TIMING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);
            paramEffectiveEndDate = new DataParameter(_CS, "EFFECTIVEENDDATE", DbType.Date);
            paramDtIns = DataParameter.GetParameter(_CS,DataParameter.ParameterEnum.DTINS); // FI 20201006 [XXXXX] Call GetdataParameter
            paramIdAIns = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.IDAINS); // FI 20201006 [XXXXX] Call GetdataParameter
            paramDtUpd = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTUPD); // FI 20201006 [XXXXX] Call GetdataParameter
            paramIdAUpd = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.IDAUPD); // FI 20201006 [XXXXX] Call GetdataParameter
            paramExtlLink = new DataParameter(_CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            paramRowAttribut = new DataParameter(_CS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
        }
        #endregion

        #region SetParameters
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        private void SetParameters(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            pParameters["IDARQ"].Value = pClosingReopeningAction.IdARQ;
            pParameters["IDENTIFIER"].Value = pClosingReopeningAction.identifier;
            pParameters["DISPLAYNAME"].Value = pClosingReopeningAction.displaynameSpecified ? pClosingReopeningAction.displayname : Convert.DBNull;
            pParameters["DESCRIPTION"].Value = pClosingReopeningAction.descriptionSpecified ? pClosingReopeningAction.description : Convert.DBNull;
            pParameters["REQUESTTYPE"].Value = ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pClosingReopeningAction.requestType);
            pParameters["TIMING"].Value = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pClosingReopeningAction.timing);
            pParameters["EFFECTIVEDATE"].Value = pClosingReopeningAction.effectiveDate;
            pParameters["READYSTATE"].Value = ReflectionTools.ConvertEnumToString<ARQTools.ActionRequestReadyStateEnum>(pClosingReopeningAction.readystate);
            pParameters["EFFECTIVEENDDATE"].Value = pClosingReopeningAction.effectiveEndDateSpecified ? pClosingReopeningAction.effectiveEndDate: Convert.DBNull;
        }
        #endregion SetParameters
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres communs INSERTION (Intégrées et publiées)
        /// </summary>
        /// <param name="pCorporateAction">Corporate action</param>
        /// <param name="pParameters">Paramètres</param>
        public virtual void SetParametersInsert(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            SetParameters(pClosingReopeningAction, pParameters);

            pParameters["IDAINS"].Value = pClosingReopeningAction.IdA;
            // FI 20200820 [25468] Dates systemes en UTC
            pParameters["DTINS"].Value = OTCmlHelper.GetDateSysUTC(_CS);
            pParameters["EXTLLINK"].Value = string.Empty;
            pParameters["ROWATTRIBUT"].Value = Cst.RowAttribut_System;
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres communs MISE A JOUR (Intégrées et publiées)
        /// </summary>
        /// <param name="pCorporateAction">Corporate action</param>
        /// <param name="pParameters">Paramètres</param>
        public virtual void SetParametersUpdate(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            SetParameters(pClosingReopeningAction, pParameters);

            pParameters["IDAUPD"].Value = pClosingReopeningAction.IdA;
            // FI 20200820 [25468] Dates systemes en UTC
            pParameters["DTUPD"].Value = OTCmlHelper.GetDateSysUTC(_CS);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion ActionRequestQuery

    #region ClosingReopeningRequestQuery
    /// <summary>
    /// Classe des Queries et paramètres des Corporate actions PUBLIEES (insert, update, select, delete)
    /// </summary>
    // EG 20190308 New
    // EG 20190613 [24683] Upd paramIsSumClosingAMount_C
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
    // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
    public class ClosingReopeningRequestQuery : ActionRequestQuery
    {
        #region Members
        private DataParameter paramTypeInstr;
        private DataParameter paramIdInstr;
        private DataParameter paramTypeContract;
        private DataParameter paramIdContract;

        protected DataParameter paramIdA_Entity;

        private DataParameter paramTypeDealer_C;
        private DataParameter paramIdDealer_C;
        private DataParameter paramTypeClearer_C;
        private DataParameter paramIdClearer_C;
        private DataParameter paramTypeCssCustodian_C;
        private DataParameter paramIdCssCustodian_C;
        private DataParameter paramMode_C;
        private DataParameter paramIsSumClosingAMount_C;
        private DataParameter paramIsDelisting_C;
        private DataParameter paramEQTYPrice_C;
        private DataParameter paramFUTPrice_C;
        private DataParameter paramOtherPrice_C;
        private DataParameter paramFeeAction_C;

        private DataParameter paramTypeDealer_O;
        private DataParameter paramIdDealer_O;
        private DataParameter paramTypeClearer_O;
        private DataParameter paramIdClearer_O;
        private DataParameter paramTypeCssCustodian_O;
        private DataParameter paramIdCssCustodian_O;
        private DataParameter paramMode_O;
        private DataParameter paramEQTYPrice_O;
        private DataParameter paramFUTPrice_O;
        private DataParameter paramOtherPrice_O;
        private DataParameter paramFeeAction_O;

        private DataParameter paramBuildInfo;
        private DataParameter paramARQFilter;
        #endregion Members
        #region Constructor
        public ClosingReopeningRequestQuery(string pCS) : base(pCS)
        {
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        // EG 20190613 [24683] Upd paramIsSumClosingAMount_C
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramARQFilter);

            pParameters.Add(paramTypeInstr);
            pParameters.Add(paramIdInstr);
            pParameters.Add(paramTypeContract);
            pParameters.Add(paramIdContract);

            pParameters.Add(paramIdA_Entity);            

            pParameters.Add(paramTypeDealer_C);
            pParameters.Add(paramIdDealer_C);
            pParameters.Add(paramTypeClearer_C);
            pParameters.Add(paramIdClearer_C);
            pParameters.Add(paramTypeCssCustodian_C);
            pParameters.Add(paramIdCssCustodian_C);
            pParameters.Add(paramMode_C);
            pParameters.Add(paramIsSumClosingAMount_C);
            pParameters.Add(paramIsDelisting_C);
            pParameters.Add(paramEQTYPrice_C);
            pParameters.Add(paramFUTPrice_C);
            pParameters.Add(paramOtherPrice_C);
            pParameters.Add(paramFeeAction_C);

            pParameters.Add(paramTypeDealer_O);
            pParameters.Add(paramIdDealer_O);
            pParameters.Add(paramTypeClearer_O);
            pParameters.Add(paramIdClearer_O);
            pParameters.Add(paramTypeCssCustodian_O);
            pParameters.Add(paramIdCssCustodian_O);
            pParameters.Add(paramMode_O);
            pParameters.Add(paramEQTYPrice_O);
            pParameters.Add(paramFUTPrice_O);
            pParameters.Add(paramOtherPrice_O);
            pParameters.Add(paramFeeAction_O);

            pParameters.Add(paramBuildInfo);
        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersInsert(DataParameters pParameters)
        {
            base.AddParametersInsert(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersUpdate(DataParameters pParameters)
        {
            base.AddParametersUpdate(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryExist
        public override QueryParameters GetQueryExist(ARQTools.ARQWhereMode pWhereMode)
        {
            DataParameters parameters = new DataParameters();
            string sqlQuery = @"select arq.IDARQ
            from dbo.ACTIONREQUEST arq" + Cst.CrLf;

            switch (pWhereMode)
            {
                case ARQTools.ARQWhereMode.ID:
                    parameters.Add(paramID);
                    sqlQuery += SQLCst.WHERE + "(arq.IDARQ = @IDARQ)" + Cst.CrLf;
                    break;
                case ARQTools.ARQWhereMode.IDENTIFIER:
                    parameters.Add(paramIdentifier);
                    sqlQuery += @"where (arq.IDENTIFIER = @IDENTIFIER)" + Cst.CrLf;
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        public override QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            string sqlQuery = @"delete from ACTIONREQUEST where (IDARQ = @IDARQ)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        // EG 20190613 [24683] Upd ISSUMCLOSINGAMT_C
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        public override QueryParameters GetQuerySelect(ARQTools.ARQWhereMode pWhereMode)
        {
            DataParameters parameters = new DataParameters(); // arq.EMBEDDEDSTATE,
            string sqlQuery = @"select arq.IDARQ, arq.IDENTIFIER, arq.DISPLAYNAME, arq.DESCRIPTION, 
            arq.REQUESTTYPE, arq.TIMING, arq.EFFECTIVEDATE, arq.EFFECTIVEENDDATE, arq.READYSTATE, arq.ARQFILTER,
            arq.TYPEINSTR, arq.IDINSTR, arq.TYPECONTRACT, arq.IDCONTRACT, arq.IDA_ENTITY,
            arq.TYPEDEALER_C, arq.IDDEALER_C, arq.TYPECLEARER_C, arq.IDCLEARER_C, arq.TYPECSSCUSTODIAN_C, arq.IDCSSCUSTODIAN_C,
            arq.MODE_C, arq.ISSUMCLOSINGAMT_C, arq.ISDELISTING_C, arq.EQTYPRICE_C, arq.FUTPRICE_C, arq.OTHERPRICE_C, arq.FEEACTION_C,
            arq.TYPEDEALER_O, arq.IDDEALER_O, arq.TYPECLEARER_O, arq.IDCLEARER_O, arq.TYPECSSCUSTODIAN_O, arq.IDCSSCUSTODIAN_O,
            arq.MODE_O, arq.EQTYPRICE_O, arq.FUTPRICE_O, arq.OTHERPRICE_O, arq.FEEACTION_O,
            arq.BUILDINFO,
            arq.DTINS, arq.IDAINS, arq.DTUPD, arq.IDAUPD, arq.EXTLLINK, arq.ROWATTRIBUT
            from dbo.ACTIONREQUEST arq" + Cst.CrLf;
            switch (pWhereMode)
            {
                case ARQTools.ARQWhereMode.ID:
                    parameters.Add(paramID);
                    sqlQuery += @"where (arq.IDARQ = @IDARQ)" + Cst.CrLf;
                    break;
                case ARQTools.ARQWhereMode.IDENTIFIER:
                    parameters.Add(paramIdentifier);
                    sqlQuery += @"where (arq.IDENTIFIER = @IDENTIFIER)" + Cst.CrLf;
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'une Fermeture/Réouverture de position
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd ISSUMCLOSINGAMT_C
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        public override QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            string sqlQuery = @"Insert into dbo.ACTIONREQUEST
            (IDARQ, IDENTIFIER, DISPLAYNAME, DESCRIPTION, REQUESTTYPE, TIMING, EFFECTIVEDATE, EFFECTIVEENDDATE, READYSTATE, ARQFILTER,
            TYPEINSTR, IDINSTR, TYPECONTRACT, IDCONTRACT, IDA_ENTITY,
            TYPEDEALER_C, IDDEALER_C, TYPECLEARER_C, IDCLEARER_C, TYPECSSCUSTODIAN_C, IDCSSCUSTODIAN_C, MODE_C, EQTYPRICE_C, FUTPRICE_C, OTHERPRICE_C, FEEACTION_C, ISSUMCLOSINGAMT_C, ISDELISTING_C,
            TYPEDEALER_O, IDDEALER_O, TYPECLEARER_O, IDCLEARER_O, TYPECSSCUSTODIAN_O, IDCSSCUSTODIAN_O, MODE_O, EQTYPRICE_O, FUTPRICE_O, OTHERPRICE_O, FEEACTION_O,
            BUILDINFO,
            DTINS, IDAINS, EXTLLINK, ROWATTRIBUT)
            values
            (@IDARQ, @IDENTIFIER, @DISPLAYNAME, @DESCRIPTION, @REQUESTTYPE, @TIMING, @EFFECTIVEDATE, @EFFECTIVEENDDATE, @READYSTATE, @ARQFILTER,
            @TYPEINSTR, @IDINSTR, @TYPECONTRACT, @IDCONTRACT, @IDA_ENTITY,
            @TYPEDEALER_C, @IDDEALER_C, @TYPECLEARER_C, @IDCLEARER_C, @TYPECSSCUSTODIAN_C, @IDCSSCUSTODIAN_C, @MODE_C, @EQTYPRICE_C, @FUTPRICE_C, @OTHERPRICE_C, @FEEACTION_C, @ISSUMCLOSINGAMT_C, @ISDELISTING_C,
            @TYPEDEALER_O, @IDDEALER_O, @TYPECLEARER_O, @IDCLEARER_O, @TYPECSSCUSTODIAN_O, @IDCSSCUSTODIAN_O, @MODE_O, @EQTYPRICE_O, @FUTPRICE_O, @OTHERPRICE_O, @FEEACTION_O,
            @BUILDINFO,
            @DTINS, @IDAINS, @EXTLLINK, @ROWATTRIBUT)";

            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdateStatus
        /// <summary>
        /// Requête de mise à jour du STATUS D'INTEGRATION d'une Fermeture/Réouverture de position
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryUpdateStatus()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdateStatus(parameters);
            string sqlQuery = @"update dbo.ACTIONREQUEST
            set READYSTATE = @READYSTATE, DTUPD = @DTUPD, IDAUPD = @IDAUPD
            where (IDARQ = @IDARQ)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQueryUpdateStatus
        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'une Fermeture/Réouverture de position
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd ISSUMCLOSINGAMT_C
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        public override QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters); 
            string sqlQuery = @"update ACTIONREQUEST
            set IDENTIFIER = @IDENTIFIER, DISPLAYNAME = @DISPLAYNAME, DESCRIPTION = @DESCRIPTION, 
            REQUESTTYPE = @REQUESTTYPE, TIMING = @TIMING, EFFECTIVEDATE = @EFFECTIVEDATE, EFFECTIVEENDDATE = @EFFECTIVEENDDATE, READYSTATE = @READYSTATE, ARQFILTER = @ARQFILTER, 
            TYPEINSTR = @TYPEINSTR, IDINSTR = @IDINSTR, TYPECONTRACT = @TYPECONTRACT, IDCONTRACT = @IDCONTRACT,
            IDA_ENTITY = @IDA_ENTITY, 
            TYPEDEALER_C = @TYPEDEALER_C, IDDEALER_C = @IDDEALER_C, TYPECLEARER_C = @TYPECLEARER_C, IDCLEARER_C = @IDCLEARER_C, TYPECSSCUSTODIAN_C = @TYPECSSCUSTODIAN_C, IDCSSCUSTODIAN_C = @IDCSSCUSTODIAN_C,
            MODE_C = @MODE_C, EQTYPRICE_C = @EQTYPRICE_C, FUTPRICE_C = @FUTPRICE_C, OTHERPRICE_C = @OTHERPRICE_C, FEEACTION_C = @FEEACTION_C, 
            ISSUMCLOSINGAMT_C = @ISSUMCLOSINGAMT_C, ISDELISTING_C = @ISDELISTING_C,
            TYPEDEALER_O = @TYPEDEALER_O, IDDEALER_O = @IDDEALER_O, TYPECLEARER_O = @TYPECLEARER_O, IDCLEARER_O = @IDCLEARER_O, TYPECSSCUSTODIAN_O = @TYPECSSCUSTODIAN_O, IDCSSCUSTODIAN_O = @IDCSSCUSTODIAN_O,
            MODE_O = @MODE_O, EQTYPRICE_O = @EQTYPRICE_O, FUTPRICE_O = @FUTPRICE_O, OTHERPRICE_O = @OTHERPRICE_O, FEEACTION_O = @FEEACTION_O,
            BUILDINFO = @BUILDINFO,
            DTUPD = @DTUPD, IDAUPD = @IDAUPD
            where (IDARQ = @IDARQ)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        // EG 20190613 [24683] Upd ISSUMCLOSINGAMT_C
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        private void InitParameter()
        {
            paramARQFilter = new DataParameter(_CS, "ARQFILTER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
            //paramEmbeddedState = new DataParameter(_CS, "EMBEDDEDSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramTypeInstr = new DataParameter(_CS, "TYPEINSTR", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdInstr = new DataParameter(_CS, "IDINSTR", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramTypeContract = new DataParameter(_CS, "TYPECONTRACT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdContract = new DataParameter(_CS, "IDCONTRACT", DbType.Int32);

            paramIdA_Entity = new DataParameter(_CS, "IDA_ENTITY", DbType.Int32);

            paramTypeDealer_C = new DataParameter(_CS, "TYPEDEALER_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdDealer_C = new DataParameter(_CS, "IDDEALER_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramTypeClearer_C = new DataParameter(_CS, "TYPECLEARER_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdClearer_C = new DataParameter(_CS, "IDCLEARER_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramTypeCssCustodian_C = new DataParameter(_CS, "TYPECSSCUSTODIAN_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdCssCustodian_C = new DataParameter(_CS, "IDCSSCUSTODIAN_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramMode_C = new DataParameter(_CS, "MODE_C", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramIsSumClosingAMount_C = new DataParameter(_CS, "ISSUMCLOSINGAMT_C", DbType.Boolean);
            paramIsDelisting_C = new DataParameter(_CS, "ISDELISTING_C", DbType.Boolean);
            paramEQTYPrice_C = new DataParameter(_CS, "EQTYPRICE_C", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramFUTPrice_C = new DataParameter(_CS, "FUTPRICE_C", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramOtherPrice_C = new DataParameter(_CS, "OTHERPRICE_C", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramFeeAction_C = new DataParameter(_CS, "FEEACTION_C", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);

            paramTypeDealer_O = new DataParameter(_CS, "TYPEDEALER_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdDealer_O = new DataParameter(_CS, "IDDEALER_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramTypeClearer_O = new DataParameter(_CS, "TYPECLEARER_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdClearer_O = new DataParameter(_CS, "IDCLEARER_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramTypeCssCustodian_O = new DataParameter(_CS, "TYPECSSCUSTODIAN_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramIdCssCustodian_O = new DataParameter(_CS, "IDCSSCUSTODIAN_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramMode_O = new DataParameter(_CS, "MODE_O", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramEQTYPrice_O = new DataParameter(_CS, "EQTYPRICE_O", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramFUTPrice_O = new DataParameter(_CS, "FUTPRICE_O", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramOtherPrice_O = new DataParameter(_CS, "OTHERPRICE_O", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramFeeAction_O = new DataParameter(_CS, "FEEACTION_O", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);

            paramBuildInfo = new DataParameter(_CS, "BUILDINFO", DbType.Xml);
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        private void SetParameters(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            // ArqFilter
            SetArqFilterParameters(pClosingReopeningAction, pParameters);
            // Environnement instrumental
            SetEnvironmentParameters(pClosingReopeningAction, pParameters);
            // Entity
            SetEntityParameters(pClosingReopeningAction, pParameters);
            // Closing
            SetCommonPositionParameters(pClosingReopeningAction.Closing, pParameters);
            // Reopening
            SetCommonPositionParameters(pClosingReopeningAction.Reopening, pParameters);

            SetBuildInfoParameter(pClosingReopeningAction, pParameters);
        }
        #endregion SetParameters

        #region SetArqFilterParameters
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        private void SetArqFilterParameters(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            pParameters["ARQFILTER"].Value = (pClosingReopeningAction.ArqFilterSpecified && pClosingReopeningAction.ArqFilter.identifierSpecified ? 
                pClosingReopeningAction.ArqFilter.identifier : Convert.DBNull);
        }
        #endregion SetArqFilterParameters

        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private void SetBuildInfoParameter(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            EFS_SerializeInfoBase serializeInfo =
                new EFS_SerializeInfoBase(pClosingReopeningAction.normMsgFactoryMQueue.GetType(), pClosingReopeningAction.normMsgFactoryMQueue);
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UnicodeEncoding());
            pParameters["BUILDINFO"].Value = sb.ToString();
        }
        #region SetEnvironmentParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        private void SetEnvironmentParameters(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            // Environnement instrumental
            pParameters["TYPEINSTR"].Value = (pClosingReopeningAction.EnvironmentSpecified && pClosingReopeningAction.Environment.instrSpecified) ? pClosingReopeningAction.Environment.instr.type : Convert.DBNull;
            pParameters["IDINSTR"].Value = (pClosingReopeningAction.EnvironmentSpecified && pClosingReopeningAction.Environment.instrSpecified) ? pClosingReopeningAction.Environment.instr.value : Convert.DBNull;
            pParameters["TYPECONTRACT"].Value = (pClosingReopeningAction.EnvironmentSpecified && pClosingReopeningAction.Environment.contractSpecified) ? pClosingReopeningAction.Environment.contract.type : Convert.DBNull;
            pParameters["IDCONTRACT"].Value = (pClosingReopeningAction.EnvironmentSpecified && pClosingReopeningAction.Environment.contractSpecified) ? pClosingReopeningAction.Environment.contract.value : Convert.DBNull;
        }
        #endregion SetEnvironmentParameters
        #region SetEntityParameters
        private void SetEntityParameters(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            pParameters["IDA_ENTITY"].Value = (pClosingReopeningAction.EntitySpecified ? pClosingReopeningAction.Entity : Convert.DBNull);
        }
        #endregion SetEntityParameters
        #region SetCommonPositionParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        // EG 20190613 [24683] Upd ISSUMCLOSINGAMT_C
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Add CssCustodian parameters for Closing|Reopening
        private void SetCommonPositionParameters(PositionInstructions pPositionInstructions, DataParameters pParameters)
        {
            string suffix = "_" + (pPositionInstructions is ClosingInstructions ? "C" : "O");

            if (null != pPositionInstructions)
            {
                // Common
                pParameters["EQTYPRICE" + suffix].Value = ReflectionTools.ConvertEnumToString<TransferPriceEnum>(pPositionInstructions.Price.EqtyPrice);
                pParameters["FUTPRICE" + suffix].Value = ReflectionTools.ConvertEnumToString<TransferPriceEnum>(pPositionInstructions.Price.FutPrice);
                pParameters["OTHERPRICE" + suffix].Value = ReflectionTools.ConvertEnumToString<TransferPriceEnum>(pPositionInstructions.Price.OtherPrice);
                pParameters["FEEACTION" + suffix].Value = pPositionInstructions.FeeActionSpecified ? pPositionInstructions.FeeAction.value : Convert.DBNull;

                pParameters["MODE" + suffix].Value = pPositionInstructions.Mode.ToString();

                if (pPositionInstructions is ClosingInstructions)
                {
                    ClosingInstructions closing = pPositionInstructions as ClosingInstructions;
                    pParameters["ISSUMCLOSINGAMT" + suffix].Value = closing.isSumClosingAmountSpecified && closing.isSumClosingAmount;
                    pParameters["TYPEDEALER" + suffix].Value = closing.dealerSpecified ? closing.dealer.type: Convert.DBNull;
                    pParameters["IDDEALER" + suffix].Value = closing.dealerSpecified ? closing.dealer.value : Convert.DBNull;
                    pParameters["TYPECLEARER" + suffix].Value = closing.clearerSpecified ? closing.clearer.type : Convert.DBNull;
                    pParameters["IDCLEARER" + suffix].Value = closing.clearerSpecified ? closing.clearer.value : Convert.DBNull;
                    pParameters["TYPECSSCUSTODIAN" + suffix].Value = closing.cssCustodianSpecified ? closing.cssCustodian.type : Convert.DBNull;
                    pParameters["IDCSSCUSTODIAN" + suffix].Value = closing.cssCustodianSpecified ? closing.cssCustodian.value : Convert.DBNull;
                    pParameters["ISDELISTING" + suffix].Value = closing.isDelistingSpecified && closing.isDelisting;
                }
                else if (pPositionInstructions is ReopeningInstructions)
                {
                    ReopeningInstructions reopening = pPositionInstructions as ReopeningInstructions;
                    pParameters["TYPEDEALER" + suffix].Value = reopening.dealerSpecified ? reopening.dealer.table : Convert.DBNull;
                    pParameters["IDDEALER" + suffix].Value = reopening.dealerSpecified ? reopening.dealer.column : Convert.DBNull;
                    pParameters["TYPECLEARER" + suffix].Value = reopening.clearerSpecified ? reopening.clearer.table : Convert.DBNull;
                    pParameters["IDCLEARER" + suffix].Value = reopening.clearerSpecified ? reopening.clearer.column : Convert.DBNull;
                    pParameters["TYPECSSCUSTODIAN" + suffix].Value = reopening.cssCustodianSpecified ? reopening.cssCustodian.table : Convert.DBNull;
                    pParameters["IDCSSCUSTODIAN" + suffix].Value = reopening.cssCustodianSpecified ? reopening.cssCustodian.column : Convert.DBNull;
                }
            }
            else
            {
                pParameters["EQTYPRICE" + suffix].Value = Convert.DBNull;
                pParameters["FUTPRICE" + suffix].Value = Convert.DBNull;
                pParameters["OTHERPRICE" + suffix].Value = Convert.DBNull;
                pParameters["FEEACTION" + suffix].Value = Convert.DBNull;
                pParameters["MODE" + suffix].Value = Convert.DBNull;
                pParameters["TYPEDEALER" + suffix].Value = Convert.DBNull;
                pParameters["IDDEALER" + suffix].Value = Convert.DBNull;
                pParameters["TYPECLEARER" + suffix].Value = Convert.DBNull;
                pParameters["IDCLEARER" + suffix].Value = Convert.DBNull;
                pParameters["TYPECSSCUSTODIAN" + suffix].Value = Convert.DBNull;
                pParameters["IDCSSCUSTODIAN" + suffix].Value = Convert.DBNull;
                pParameters["FEEACTION" + suffix].Value = Convert.DBNull;
                
                if (null != pParameters["ISSUMCLOSINGAMT" + suffix])
                    pParameters["ISSUMCLOSINGAMT" + suffix].Value = Convert.DBNull;
                if (null != pParameters["ISDELISTING" + suffix])
                    pParameters["ISDELISTING" + suffix].Value = Convert.DBNull;
            }

        }
        #endregion SetCommonPositionParameters
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public override void SetParametersInsert(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            base.SetParametersInsert(pClosingReopeningAction, pParameters);
            SetParameters(pClosingReopeningAction, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public override void SetParametersUpdate(ClosingReopeningAction pClosingReopeningAction, DataParameters pParameters)
        {
            base.SetParametersUpdate(pClosingReopeningAction, pParameters);
            SetParameters(pClosingReopeningAction, pParameters);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion ClosingReopeningRequestQuery

    #region ARQInfo
    [Serializable]
    // EG 20190308 New
    public class ARQInfo
    {
        #region Members
        private Cst.Capture.ModeEnum m_Mode;
        private Nullable<int> m_Id;
        private int m_IdA;
        #endregion Members
        #region Accessors
        #region Mode
        public Cst.Capture.ModeEnum Mode
        {
            set { m_Mode = value; }
            get { return m_Mode; }
        }
        #endregion Mode
        #region Id
        public Nullable<int> Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
        #endregion IdSource
        #region IdA
        public int IdA
        {
            get { return m_IdA; }
            set { m_IdA = value; }
        }
        #endregion IdA
        #endregion Accessors
        #region Constructors
        public ARQInfo(int pIdA)
        {
            m_IdA = pIdA;
            m_Mode = Cst.Capture.ModeEnum.New;
            m_Id = null;
        }
        public ARQInfo(int pIdA, string pPKV)
            : this(pIdA)
        {
            if (StrFunc.IsFilled(pPKV))
            {
                m_Mode = Cst.Capture.ModeEnum.Update;
                m_Id = Convert.ToInt32(pPKV);
            }
        }
        #endregion Constructors
    }
    #endregion ARQInfo

    #region ActionRequest
    /// <summary>
    /// Contient les caractéristiques générales d'une fermeture/réouverture de position
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ClosingReopeningAction))]
    // EG 20190308 New
    // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
    public abstract class ActionRequest : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("requestType", Order = 1)]
        public Cst.PosRequestTypeEnum requestType;
        [XmlElementAttribute("timing", Order = 2)]
        public SettlSessIDEnum timing;
        [XmlElementAttribute("effectiveDate", Order = 3)]
        public DateTime effectiveDate;
        [XmlIgnore()]
        public bool effectiveEndDateSpecified;
        [XmlElementAttribute("effectiveEndDate", Order = 4)]
        public DateTime effectiveEndDate;
        [XmlElementAttribute("readystate", Order = 5)]
        public ARQTools.ActionRequestReadyStateEnum readystate;

        // Variables de travail (non sérialisées)
        [XmlIgnoreAttribute()]
        protected int _idA;
        [XmlIgnoreAttribute()]
        protected string _CS;
        #endregion Members

        #region Accessors
        #region IdARQ
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdARQ
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #endregion IdARQ
        #region IdA
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdA
        {
            get { return _idA; }
            set { _idA = value; }
        }
        #endregion IdA
        #endregion Accessors
        #region Constructors
        public ActionRequest() { }
        public ActionRequest(string pCS)
        {
            _CS = pCS;
        }
        #endregion Constructors
        #region Methods
        #region Delete
        public Cst.ErrLevel Delete()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            IDbTransaction dbTransaction = null;
            try
            {
                ActionRequestQuery arqQuery = new ClosingReopeningRequestQuery(_CS);
                if (null != arqQuery)
                {
                    dbTransaction = DataHelper.BeginTran(_CS);
                    QueryParameters qry = arqQuery.GetQueryDelete();
                    qry.Parameters["IDARQ"].Value = IdARQ;
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    DataHelper.CommitTran(dbTransaction);
                    ret = Cst.ErrLevel.SUCCESS;
                }
                else
                    ret = Cst.ErrLevel.FAILURE;
                return ret;
            }
            catch (Exception)
            {
                ret = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                if (ret == Cst.ErrLevel.FAILURE)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
        }
        #endregion Delete
        #region Exist
        public bool Exist(ARQInfo pARQInfo, ARQTools.ARQWhereMode pWhereMode)
        {
            bool isExist = false;
            ActionRequestQuery arqQuery = new ClosingReopeningRequestQuery(_CS);
            QueryParameters qryParameters = arqQuery.GetQueryExist(pWhereMode);
            DataParameters parameters = qryParameters.Parameters;
            SetWhereParameters(parameters, pARQInfo, pWhereMode);
            object obj = DataHelper.ExecuteScalar(_CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                isExist = BoolFunc.IsTrue(obj);
            return isExist;
        }
        #endregion Exist

        #region Load
        public Cst.ErrLevel Load(ARQInfo pARQInfo, ARQTools.ARQWhereMode pWhereMode)
        {
            ActionRequestQuery arqQuery = new ClosingReopeningRequestQuery(_CS);
            if (null != arqQuery)
            {
                IdA = pARQInfo.IdA;

                QueryParameters qryParameters = arqQuery.GetQuerySelect(pWhereMode);
                DataParameters parameters = qryParameters.Parameters;
                SetWhereParameters(parameters, pARQInfo, pWhereMode);
                DataSet ds = DataHelper.ExecuteDataset(_CS, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter());
                if (null != ds)
                {
                    if (1 == ds.Tables[0].Rows.Count)
                        SetActionRequest(ds.Tables[0].Rows[0]);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Load
        #region Write
        public Cst.ErrLevel Write(ARQInfo pARQInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            switch (pARQInfo.Mode)
            {
                case Cst.Capture.ModeEnum.New:
                    ret = Insert(pARQInfo);
                    break;
                case Cst.Capture.ModeEnum.Update:
                    ret = Update(pARQInfo);
                    break;
            }
            return ret;
        }
        #endregion Write
        #region Insert
        public virtual Cst.ErrLevel Insert(ARQInfo pARQInfo)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Insert
        #region Update
        public virtual Cst.ErrLevel Update(ARQInfo pARQInfo)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Update

        #region SetWhereParameters
        private void SetWhereParameters(DataParameters pParameters, ARQInfo pARQInfo, ARQTools.ARQWhereMode pWhereMode)
        {
            switch (pWhereMode)
            {
                case ARQTools.ARQWhereMode.ID:
                    pParameters["IDARQ"].Value = pARQInfo.Id;
                    break;
                case ARQTools.ARQWhereMode.IDENTIFIER:
                    pParameters["IDENTIFIER"].Value = identifier;
                    break;
            }
        }
        #endregion SetWhereParameters

        #region SetActionRequest
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        public virtual void SetActionRequest(DataRow pRow)
        {
            spheresid = pRow["IDARQ"].ToString();

            identifierSpecified = (false == Convert.IsDBNull(pRow["IDENTIFIER"]));
            if (identifierSpecified)
                identifier = pRow["IDENTIFIER"].ToString();
            displaynameSpecified = (false == Convert.IsDBNull(pRow["DISPLAYNAME"]));
            if (displaynameSpecified)
                displayname = pRow["DISPLAYNAME"].ToString();
            descriptionSpecified = (false == Convert.IsDBNull(pRow["DESCRIPTION"]));
            if (descriptionSpecified)
                description = pRow["DESCRIPTION"].ToString();

            requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(pRow["REQUESTTYPE"].ToString());
            timing = ReflectionTools.ConvertStringToEnum<SettlSessIDEnum>(pRow["TIMING"].ToString());
            effectiveDate = Convert.ToDateTime(pRow["EFFECTIVEDATE"]);
            readystate = ARQTools.ReadyState(pRow["READYSTATE"].ToString()).Value;

            effectiveEndDateSpecified = (false == Convert.IsDBNull(pRow["EFFECTIVEENDDATE"]));
            if (effectiveEndDateSpecified)
                effectiveEndDate = Convert.ToDateTime(pRow["EFFECTIVEENDDATE"]);

        }
        #endregion SetActionRequestEmbedded
        #endregion Methods
    }
    #endregion ActionRequest

    #region ActionFilter
    [Serializable()]
    // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
    public class ActionFilter : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("dtEnabled", Order = 1)]
        public DateTime dtEnabled;
        [XmlIgnoreAttribute()]
        public bool dtDisabledSpecified;
        [XmlElementAttribute("dtDisabled", Order = 2)]
        public DateTime dtDisabled;
        [XmlIgnoreAttribute()]
        public bool qrySpecified;
        //[XmlElementAttribute("qry", Order = 3)]
        [XmlIgnoreAttribute()]
        public string qry;

        #endregion Members
        #region Constructors
        public ActionFilter() { }
        #endregion Constructors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = new MQueueparameter("FILTERNAME", TypeData.TypeDataEnum.@string);
            parameter.SetValue(identifier);
            parameter.displayNameSpecified = displaynameSpecified;
            parameter.displayName = displayname;
            parameters.Add(parameter);
            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters

        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <returns></returns>
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = new List<string>();
            MQueueparameter _parameter = pMQueueParameters["FILTERNAME"];
            if (null != _parameter)
            {
                identifierSpecified = true;
                identifier = _parameter.Value;
                displaynameSpecified = _parameter.displayNameSpecified;
                if (displaynameSpecified)
                    displayname = _parameter.displayName;
            }
            return _unavailableParameters;
        }
    }
    #endregion ActionFilter
    #region EnvironmentInstructions
    [Serializable()]
    // EG 20190308 New
    public class EnvironmentInstructions
    {
        #region Members
        [XmlIgnore()]
        public bool instrSpecified;
        [XmlElement("typeInstr", Order = 1)]
        public TypeInstrument instr;
        [XmlIgnore()]
        public bool contractSpecified;
        [XmlElement("typeContract", Order = 2)]
        public TypeContract contract;
        #endregion Members
        #region Constructors
        public EnvironmentInstructions() { }
        #endregion Constructors
        #region Accessors
        public bool IsSpecified
        {
            get { return instrSpecified || contractSpecified; }
        }
        #endregion Accessors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter;

            // Instrument (GrpProduct|Product|Instr|GrpInstr)
            // Dans le cas d'une intégration via NormMsgFactory
            // seules les valeurs GrpProduct|Product ont un sens et seront exploitées
            if (instrSpecified)
            {
                parameter = new MQueueparameter("INSTRTYPE", instr.identifier, null, TypeData.TypeDataEnum.@string);
                parameter.SetValue(instr.type.ToString());
                parameters.Add(parameter);
            }

            // Contract (CommodityContract|DerivativeContract|GrpContract|GrpMarket|Market|IsinUnderlyerContract)
            // Dans le cas d'une intégration via NormMsgFactory
            // seules les valeurs CommodityContract|DerivativeContract|Market|IsinUnderlyerContract ont un sens et seront exploitées
            // l'attribut "identifier" sera décomposé par NormMsgFactory (Ex : ACRONYM + ISIN, etc)
            if (this.contractSpecified)
            {
                parameter = new MQueueparameter("CONTRACTTYPE", contract.identifier, null, TypeData.TypeDataEnum.@string);
                parameter.SetValue(contract.type);
                parameters.Add(parameter);
            }
            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters

        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <returns></returns>
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20230906 [WI701] Closing/reopening module : End Of Day and Closing Day Process(Gestion Marchés sur IsinUnderlyingContract)
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = new List<string>();
            MQueueparameter _parameter = null;
            QueryParameters qryParameters = null;
            DataParameters dataParameters = new DataParameters();
            string sqlQuery = string.Empty;
            object obj = null;

            // Paramètres Product/Instr Type & Value
            _parameter = pMQueueParameters["INSTRTYPE"];
            Nullable<TypeInstrEnum> typeInstr;
            SQL_TableWithID _sqlTable = null;
            if (null != _parameter)
            {
                typeInstr = ReflectionTools.ConvertStringToEnumOrNullable<TypeInstrEnum>(_parameter.Value);
                if (typeInstr.HasValue)
                {
                    instr = new TypeInstrument
                    {
                        type = typeInstr.Value
                    };

                    switch (typeInstr.Value)
                    {
                        case TypeInstrEnum.GrpInstr:
                            // Recherche de l'ID du groupe sur la base de son identifier (Normalement INUSITE car peut ne pas exister)
                            dataParameters.Add(new DataParameter(pCS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGINSTR_LEN), "CLOSINGREOPENING");
                            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN), _parameter.name);
                            sqlQuery = @"select g.IDGINSTR 
                            from dbo.GINSTR g
                            inner join dbo.GINSTRROLE gr on (gr.IDGINSTR = g.IDGINSTR) and (gr.IDROLEGINSTR = @ROLE)
                            where (g.IDENTIFIER = @IDENTIFIER)";
                            qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                            obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (null != obj)
                                instr.value = obj.ToString();
                            break;
                        case TypeInstrEnum.GrpProduct:
                            // Alimentation Recherche de l'ID du groupe sur la base de son identifiant (Normalement INUSITE car peut ne pas exister)
                            Nullable<ProductTools.GroupProductEnum> gProduct = ReflectionTools.ConvertStringToEnumOrNullable<ProductTools.GroupProductEnum>(_parameter.Value);
                            if (gProduct.HasValue && ProductTools.IsProductTrading(gProduct.Value))
                                instr.value = _parameter.Value;
                            break;
                        case TypeInstrEnum.Instr:
                            // Recherche de l'ID de l'instrument sur la base de son identifier (Normalement INUSITE car peut ne pas exister)
                            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN), _parameter.name);
                            sqlQuery = @"select i.IDI 
                            from dbo.INSTRUMENT i
                            inner join dbo.PRODUCT p on (p.IDP = i.IDP)
                            where (i.IDENTIFIER = @IDENTIFIER) and (p.GPRODUCT not in ('ADM','ASSET','RISK'))";
                            qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                            obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (null != obj)
                            {
                                instr.value = obj.ToString();
                                instr.identifier = _parameter.name;
                            }
                            break;
                        case TypeInstrEnum.Product:
                            // Recherche de l'ID du produit sur la base de son identifier
                            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN), _parameter.name);
                            sqlQuery = @"select p.IDP 
                            from dbo.PRODUCT p
                            where (p.IDENTIFIER = @IDENTIFIER) and (p.GPRODUCT not in ('ADM','ASSET','RISK'))";
                            qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                            obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (null != obj)
                            {
                                instr.value = obj.ToString();
                                instr.identifier = _parameter.name;
                            }
                            break;
                    }
                    instrSpecified = StrFunc.IsFilled(instr.value);
                    // Non géré ou incorrect
                    if (false == instrSpecified)
                        _unavailableParameters.Add("INSTRTYPE");
                }
            }

            // Paramètres Market/Contract Type & Value
            Nullable<TypeContractEnum> typeContract;
            _parameter = pMQueueParameters["CONTRACTTYPE"];
            if (null != _parameter)
            {
                typeContract = ReflectionTools.ConvertStringToEnumOrNullable<TypeContractEnum>(_parameter.Value);
                if (typeContract.HasValue)
                {
                    contract = new TypeContract
                    {
                        type = typeContract.Value,
                        identifier = _parameter.name
                    };

                    Pair<string, int> _parameterSplit;
                    Pair<string, int> _parameterSplit2;
                    switch (typeContract.Value)
                    {
                        case TypeContractEnum.CommodityContract:
                            // Recherche de l'ID du CommodityContract sur la base de son identifier et du marché
                            _parameterSplit = SplitParameter(pCS, "-", _parameter.name);
                            if (null != _parameterSplit)
                            {
                                _sqlTable = new SQL_CommodityContract(pCS, _parameterSplit.First, _parameterSplit.Second);
                                if (_sqlTable.IsLoaded)
                                    contract.value = _sqlTable.Id;
                            }
                            break;
                        case TypeContractEnum.DerivativeContract:
                            _parameterSplit = SplitParameter(pCS, "-", _parameter.name);
                            if (null != _parameterSplit)
                            {
                                // Recherche de l'ID du DerivativeContract sur la base de son identifier et du marché
                                _sqlTable = new SQL_DerivativeContract(pCS, _parameterSplit.First, _parameterSplit.Second);
                                if (_sqlTable.IsLoaded)
                                    contract.value = _sqlTable.Id;
                                else
                                {
                                    // Si non trouvé décomposition de l'identier en élements simples
                                    // Symbol|Type|Category|SettlementMethod|[ExerciseStyle]|[Attribute]
                                    // et recherche sur la base de ces éléments
                                    List<DictionaryEntry> lstData = SplitDerivativeContractIdentifer(_parameterSplit.First);
                                    sqlQuery = @"select dc.IDDC 
                                    from dbo.DERIVATIVECONTRACT dc
                                    where";
                                    lstData.ForEach(item => {sqlQuery += $" (dc.{item.Key} = '{item.Value}') and ";});
                                    sqlQuery += $@" (dc.IDM = {_parameterSplit.Second})";
                                    obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlQuery, null);
                                    if (null != obj)
                                        contract.value = Convert.ToInt32(obj);
                                }
                            }
                            break;
                        case TypeContractEnum.GrpContract:
                        case TypeContractEnum.GrpMarket:
                            // Recherche de l'ID du groupe sur la base de son identifier (Normalement INUSITE car peut ne pas exister)
                            string elt = typeContract.Value.ToString().ToUpper().Replace("GRP", "G");
                            dataParameters.Add(new DataParameter(pCS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGINSTR_LEN), "CLOSINGREOPENING");
                            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN), _parameter.name);
                            sqlQuery = $@"select g.ID{elt}
                            from dbo.{elt} g
                            inner join dbo.{elt}ROLE gr on (gr.ID{elt} = g.ID{elt}) and (gr.IDROLE{elt} = @ROLE)
                            where (g.IDENTIFIER = @IDENTIFIER)";
                            qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                            obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (null != obj)
                                contract.value = Convert.ToInt32(obj);
                            break;
                        case TypeContractEnum.IsinUnderlyerContract:
                            // Recherche de l'ID du Sous-jacent sur la base de son code ISIN et du marché
                            _parameterSplit = SplitParameter(pCS, "-", _parameter.name);
                            _parameterSplit2 = SplitParameter(pCS, "/", _parameterSplit.First);
                            sqlQuery = $@"select ast.IDASSET as ID 
                            from dbo.VW_ASSET ast
                            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = ast.IDASSET)
                            where dc.IDM = {_parameterSplit.Second} and ast.IDM = {_parameterSplit2.Second} and ast.ISINCODE = '{_parameterSplit2.First}'";
                            obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlQuery, null);
                            if (null != obj)
                                contract.value = Convert.ToInt32(obj); ;
                            break;
                        case TypeContractEnum.Market:
                            // Recherche de l'ID du marché sur la base de son FIXML_SecurityExchange
                            _sqlTable = new SQL_Market(pCS, _parameter.name);
                            if (_sqlTable.IsLoaded)
                                contract.value = _sqlTable.Id;
                            break;
                    }

                    contractSpecified = (0 < contract.value);
                    // Non géré ou incorrect
                    if (false == contractSpecified)
                        _unavailableParameters.Add("CONTRACTTYPE");
                }
            }
            return _unavailableParameters;
        }

        /// <summary>
        /// Split de la valeur du paramètre MARKET pour récupérer
        /// son FIXML_SecurityExchange
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pParameterValue"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public Pair<string, int> SplitParameter(string pCS, string pSeparator, string pParameterValue)
        {
            Pair<string, int> ret = null;

            string[] _value = pParameterValue.Split(pSeparator.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            if (ArrFunc.IsFilled(_value))
            {
                SQL_Market sqlMarket = new SQL_Market(pCS, SQL_TableWithID.IDType.FIXML_SecurityExchange, _value[0].Trim(), SQL_Table.ScanDataDtEnabledEnum.Yes);
                sqlMarket.LoadTable(new string[] { "IDM, IDENTIFIER, FIXML_SecurityExchange" });
                if (sqlMarket.IsLoaded)
                    ret = new Pair<string, int>(_value[1].Trim(), sqlMarket.Id);
            }
            return ret;
        }

        /// <summary>
        /// Split de la valeur du paramètre IDCONTRACT (IDENTIFIER) pour récupérer les données élémentaires
        /// Symbol|Type|Category|SettlementMethod|[ExerciseStyle]|[Attribute]
        /// </summary>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private List<DictionaryEntry> SplitDerivativeContractIdentifer(string pIdentifier)
        {
            // Symbol + ' ' + Type(S | F | L) + Category(F | O) + ' ' + SM(CS | PS)[+' ' + ES(AM | EU)][+' ' + Attribute]
            List<DictionaryEntry> lstData = new List<DictionaryEntry>();

            string[] _value = pIdentifier.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (ArrFunc.IsFilled(_value) && (2 < _value.Length))
            {
                // Contract Symbol
                string contractSymbol = _value[0];
                lstData.Add(new DictionaryEntry("CONTRACTSYMBOL", contractSymbol));

                // Contract Type
                DerivativeContractTypeEnum derivativeContractType = DerivativeContractTypeEnum.STD;
                string ct = _value[1].Substring(0, 1);
                if (ct == "F")
                    derivativeContractType = DerivativeContractTypeEnum.FLEX;
                else if (ct == "L")
                    derivativeContractType = DerivativeContractTypeEnum.LEAPS;
                lstData.Add(new DictionaryEntry("CONTRACTTYPE", derivativeContractType.ToString()));

                // Category
                string category = _value[1].Substring(1, 1);
                EfsML.Enum.CfiCodeCategoryEnum cfiCodeCategory = (category=="F"?CfiCodeCategoryEnum.Future: CfiCodeCategoryEnum.Option);
                lstData.Add(new DictionaryEntry("CATEGORY", category));

                // Settlement method
                string settltMethod = _value[2].Substring(0, 1);
                lstData.Add(new DictionaryEntry("SETTLTMETHOD", settltMethod));

                // Exercise Style
                DerivativeExerciseStyleEnum derivativeExerciseStyle = DerivativeExerciseStyleEnum.American;
                if (cfiCodeCategory == CfiCodeCategoryEnum.Option)
                {
                    switch (_value[3])
                    {
                        case "AM":
                            derivativeExerciseStyle = DerivativeExerciseStyleEnum.American;
                            break;
                        case "EU":
                            derivativeExerciseStyle = DerivativeExerciseStyleEnum.European;
                            break;
                    }
                    lstData.Add(new DictionaryEntry("EXERCISESTYLE", ReflectionTools.ConvertEnumToString<DerivativeExerciseStyleEnum>(derivativeExerciseStyle)));

                    if (5 == _value.Length)
                        lstData.Add(new DictionaryEntry("CONTRACTATTRIBUTE", _value[4]));
                }
                else if (4 == _value.Length)
                    lstData.Add(new DictionaryEntry("CONTRACTATTRIBUTE", _value[3]));
            }
            return lstData;
        }
    }
    #endregion EnvironmentInstructions
    #region TypeInstrument
    [Serializable()]
    // EG 20190308 New
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public class TypeInstrument
    {
        #region Members
        [XmlText()]
        public TypeInstrEnum type;
        [XmlAttribute("id")]
        public string @value;
        [XmlAttribute("identifier")]
        public string @identifier;
        #endregion Members
        #region Constructors
        public TypeInstrument() { }
        #endregion Constructors
    }
    #endregion TypeInstrument
    #region TypeContract
    [Serializable()]
    // EG 20190308 New
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public class TypeContract
    {
        #region Members
        [XmlText()]
        public TypeContractEnum type;
        [XmlAttribute("id")]
        public int @value;
        [XmlAttribute("identifier")]
        public string @identifier;
        [XmlAttribute("idM")]
        public int @idM;
        #endregion Members
        #region Constructors
        public TypeContract() { }
        #endregion Constructors
    }
    #endregion ContractInstructions

    #region ActorInstructions
    [Serializable()]
    // EG 20190308 New
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public class ActorInstructions
    {
        #region Members
        [XmlText()]
        public TypePartyEnum type;
        [XmlAttribute("id")]
        public int @value;
        [XmlAttribute("identifier")]
        public string @identifier;
        #endregion Members
        #region Constructors
        public ActorInstructions() { }
        #endregion Constructors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pTypeActor"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public MQueueparameter[] GetNormMsgFactoryParameters(string pTypeActor)
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = new MQueueparameter($"{pTypeActor}_C", identifier, null, TypeData.TypeDataEnum.@string);
            parameter.SetValue(type.ToString());
            parameters.Add(parameter);

            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters

        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <param name="pTypeActor"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters, string pTypeActor)
        {
            List<string> _unavailableParameters = new List<string>();

            // Type Dealer/Clearer for Closing
            MQueueparameter _parameter = pMQueueParameters[$"{pTypeActor}_C"];
            if (null != _parameter)
            {
                Nullable<TypePartyEnum> _type = ReflectionTools.ConvertStringToEnumOrNullable<TypePartyEnum>(_parameter.Value);
                if (_type.HasValue)
                {
                    type = _type.Value;
                    identifier = _parameter.name;
                    SQL_TableWithID _sqlTable;
                    SQL_Group _sqlGroup;
                    switch (type)
                    {
                        case TypePartyEnum.Actor:
                            _sqlTable = new SQL_Actor(pCS, _parameter.name);
                            if (_sqlTable.IsLoaded)
                                value = _sqlTable.Id;
                            break;
                        case TypePartyEnum.GrpActor:
                            _sqlGroup = new SQL_GActor(pCS, _parameter.name);
                            if (_sqlGroup.IsLoaded)
                                value = _sqlGroup.Id;
                            break;
                        case TypePartyEnum.Book:
                            _sqlTable = new SQL_Book(pCS, SQL_TableWithID.IDType.Identifier, _parameter.name);
                            if (_sqlTable.IsLoaded)
                                value = _sqlTable.Id;
                            break;
                        case TypePartyEnum.GrpBook:
                            _sqlGroup = new SQL_GBook(pCS, _parameter.name);
                            if (_sqlGroup.IsLoaded)
                                value = _sqlGroup.Id;
                            break;
                    }

                    // Non géré ou incorrect
                    if (0 == value)
                        _unavailableParameters.Add($"{ pTypeActor}_C");
                }
                else
                    _unavailableParameters.Add($"{pTypeActor}_C");
            }
            return _unavailableParameters;
        }
    }
    #endregion ActorInstructions
    #region LinkActorInstructions
    [SerializableAttribute()]
    // EG 20190308 New
    // EG 20240520 [WI930] Add isCssCustodian
    public class LinkActorInstructions
    {
        #region Members
        [XmlAttribute("table")]
        public string table;
        [XmlText()]
        public string column;
        [XmlAttribute("isCssCustodian")]
        public bool isCssCustodian;
        #endregion Members
        #region Constructors
        public LinkActorInstructions() { }
        #endregion Constructors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pTypeActor"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public MQueueparameter[] GetNormMsgFactoryParameters(string pTypeActor)
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = new MQueueparameter($"{pTypeActor}_O", table, null, TypeData.TypeDataEnum.@string);
            parameter.SetValue(column);
            parameters.Add(parameter);

            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pMQueueParameters"></param>
        /// <param name="pTypeActor"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters, string pTypeActor)
        {
            List<string> _unavailableParameters = new List<string>();

            // Type Dealer/Clearer/Csscustodian for Reopening
            MQueueparameter _parameter = pMQueueParameters[$"{pTypeActor}_O"];
            if (null != _parameter)
            {
                column = _parameter.Value;
                if (_parameter.nameSpecified)
                    table = _parameter.name;
                else
                    _unavailableParameters.Add($"{pTypeActor}_C");
            }
            return _unavailableParameters;
        }
    }
    #endregion LinkActorInstructions
    #region FeeActionInstructions
    [SerializableAttribute()]
    public class FeeActionInstructions
    {
        #region Members
        [XmlText()]
        public string @identifier;
        [XmlAttribute("id")]
        public int @value;
        #endregion Members
        #region Constructors
        public FeeActionInstructions() { }
        #endregion Constructors
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters, string pSuffix)
        {
            List<string> _unavailableParameters = new List<string>();
            DataParameters dataParameters = new DataParameters();
            MQueueparameter _parameter = pMQueueParameters[$"FEEACTION_{pSuffix}"];
            if (null != _parameter)
            {
                identifier = _parameter.Value;
                dataParameters.Add(new DataParameter(pCS, "IDMENU", DbType.AnsiStringFixedLength, SQLCst.UT_IDENTIFIER_LEN), identifier);
                string sqlQuery = @"select IDPERMISSION
                from dbo.VW_ALL_VW_PERMIS_MENU
                where (IDMENU = @IDMENU) and (PERMISSION = 'Create')";
                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
                object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    value = Convert.ToInt32(obj);
                else
                    _unavailableParameters.Add($"FEEACTION_{pSuffix}");
            }
            return _unavailableParameters;
        }

    }
    #endregion FeeActionInstructions
    #region PriceInstructions
    [SerializableAttribute()]
    // EG 20190308 New
    // EG 20200318 [24683] Upd Add XMLComment
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public class PriceInstructions
    {
        #region Members
        [XmlAnyElement("eqtyPriceXmlComment")]
        public XmlComment EqtyPriceXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlAttribute("eqty")]
        [XmlComment("- for PremiumStyle : FairValueBeforePrice|FairValuePrice|DayBeforePrice|DayPrice|TradingPrice|Zero")]
        public TransferPriceEnum EqtyPrice { set; get; }

        [XmlAnyElement("futPriceXmlComment")]
        public XmlComment FutPriceXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlAttribute("fut")]
        [XmlComment("- for FutureStyleMarkToMarket : FairValueBeforePrice|FairValuePrice|DayBeforePrice|DayPrice|TradingPrice|Zero")]
        public TransferPriceEnum FutPrice { set; get; }

        [XmlAnyElement("otherPriceXmlComment")]
        public XmlComment OtherPriceXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlAttribute("other")]
        [XmlComment("- for Others : FairValueBeforePrice|FairValuePrice|DayBeforePrice|DayPrice|TradingPrice|Zero")]
        public TransferPriceEnum OtherPrice { set; get; }
        #endregion Members
        #region Constructors
        public PriceInstructions() { }
        #endregion Constructors
    }
    #endregion PriceInstructions
    #region ClosingInstructions
    /// <summary>
    /// Instructions de fermeture pour un contexte donné
    /// </summary>
    // EG 20190308 New
    // EG 20190318 Upd ClosingReopening Step3
    // EG 20190613 [24683] Upd isSumClosingAmount
    [SerializableAttribute()]
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    // EG 20240520 [WI930] Add cssCustodian
    public class ClosingInstructions : PositionInstructions
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool dealerSpecified;
        [XmlElementAttribute("dealer", Order = 1)]
        public ActorInstructions dealer;
        [XmlIgnoreAttribute()]
        public bool clearerSpecified;
        [XmlElementAttribute("clearer", Order = 2)]
        public ActorInstructions clearer;
        [XmlIgnoreAttribute()]
        public bool cssCustodianSpecified;
        [XmlElementAttribute("cssCustodian", Order = 3)]
        public ActorInstructions cssCustodian;
        [XmlIgnoreAttribute()]
        public bool isSumClosingAmountSpecified;
        [XmlElementAttribute("isSumClosingAmt", Order = 4)]
        public bool isSumClosingAmount;
        [XmlIgnoreAttribute()]
        public bool isDelistingSpecified;
        [XmlElementAttribute("isDelisting", Order = 5)]
        public bool isDelisting;
        #endregion Members
        #region Constructors
        public ClosingInstructions() { }
        #endregion Constructors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <returns></returns>
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Set cssCustodian parameter
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters
            {
                parameter = base.GetNormMsgFactoryParameters("C")
            };

            MQueueparameter parameter;
            if (isSumClosingAmountSpecified)
            {
                parameter = new MQueueparameter($"ISSUMCLOSINGAMT_C", TypeData.TypeDataEnum.@bool);
                parameter.SetValue(isSumClosingAmount);
                parameters.Add(parameter);
            }
            if (isDelistingSpecified)
            {
                parameter = new MQueueparameter($"ISDELISTING_C", TypeData.TypeDataEnum.@bool);
                parameter.SetValue(isDelisting);
                parameters.Add(parameter);
            }


            if (dealerSpecified)
                parameters.Add(dealer.GetNormMsgFactoryParameters("DEALER"));

            if (clearerSpecified)
                parameters.Add(clearer.GetNormMsgFactoryParameters("CLEARER"));

            if (cssCustodianSpecified)
                parameters.Add(cssCustodian.GetNormMsgFactoryParameters("CSSCUSTODIAN"));

            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Set cssCustodian parameter
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = base.SetNormMsgFactoryParameters(pCS, pMQueueParameters, "C");
            dealerSpecified = (null != pMQueueParameters["DEALER_C"]);
            if (dealerSpecified)
            {
                dealer = new ActorInstructions();
                _unavailableParameters.AddRange(dealer.SetNormMsgFactoryParameters(pCS, pMQueueParameters, "DEALER"));
            }
            clearerSpecified = (null != pMQueueParameters["CLEARER_C"]);
            if (clearerSpecified)
            {
                clearer = new ActorInstructions();
                _unavailableParameters.AddRange(clearer.SetNormMsgFactoryParameters(pCS, pMQueueParameters, "CLEARER"));
            }
            cssCustodianSpecified = (null != pMQueueParameters["CSSCUSTODIAN_C"]);
            if (cssCustodianSpecified)
            {
                cssCustodian = new ActorInstructions();
                _unavailableParameters.AddRange(cssCustodian.SetNormMsgFactoryParameters(pCS, pMQueueParameters, "CSSCUSTODIAN"));
            }
            MQueueparameter _parameter = pMQueueParameters[$"ISSUMCLOSINGAMT_C"];
            isSumClosingAmountSpecified = (null != _parameter);
            if (isSumClosingAmountSpecified)
                isSumClosingAmount= Convert.ToBoolean(_parameter.Value);

            _parameter = pMQueueParameters[$"ISDELISTING_C"];
            isDelistingSpecified = (null != _parameter);
            if (isDelistingSpecified)
                isDelisting = Convert.ToBoolean(_parameter.Value);

            return _unavailableParameters;
        }
    }
    #endregion ClosingInstructions
    #region ReopeningInstructions
    /// <summary>
    /// Instructions de réouverture pour un contexte donné
    /// </summary>
    [SerializableAttribute()]
    // EG 20190308 New
    // EG 20240520 [WI930] Add cssCustodian 
    public class ReopeningInstructions : PositionInstructions
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool dealerSpecified;
        [XmlElementAttribute("dealer", Order = 1)]
        public LinkActorInstructions dealer;
        [XmlIgnoreAttribute()]
        public bool clearerSpecified;
        [XmlElementAttribute("clearer", Order = 2)]
        public LinkActorInstructions clearer;
        [XmlIgnoreAttribute()]
        public bool cssCustodianSpecified;
        [XmlElementAttribute("cssCustodian", Order = 3)]
        public LinkActorInstructions cssCustodian;
        #endregion Members
        #region Constructors
        public ReopeningInstructions() { }
        #endregion Constructors

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Get cssCustodian parameter
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters
            {
                parameter = base.GetNormMsgFactoryParameters("O")
            };

            if (dealerSpecified)
                parameters.Add(dealer.GetNormMsgFactoryParameters("DEALER"));

            if (clearerSpecified)
                parameters.Add(clearer.GetNormMsgFactoryParameters("CLEARER"));

            if (cssCustodianSpecified)
                parameters.Add(cssCustodian.GetNormMsgFactoryParameters("CSSCUSTODIAN"));

            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters

        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Set cssCustodian parameter
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = base.SetNormMsgFactoryParameters(pCS, pMQueueParameters, "O");
            dealerSpecified = (null != pMQueueParameters["DEALER_O"]);
            if (dealerSpecified)
            {
                dealer = new LinkActorInstructions();
                _unavailableParameters.AddRange(dealer.SetNormMsgFactoryParameters(pMQueueParameters, "DEALER"));
            }
            clearerSpecified = (null != pMQueueParameters["CLEARER_O"]);
            if (clearerSpecified)
            {
                clearer = new LinkActorInstructions();
                _unavailableParameters.AddRange(clearer.SetNormMsgFactoryParameters(pMQueueParameters, "CLEARER"));
            }
            cssCustodianSpecified = (null != pMQueueParameters["CSSCUSTODIAN_O"]);
            if (cssCustodianSpecified)
            {
                cssCustodian = new LinkActorInstructions();
                cssCustodian.isCssCustodian = true;
                _unavailableParameters.AddRange(cssCustodian.SetNormMsgFactoryParameters(pMQueueParameters, "CSSCUSTODIAN"));
            }
            return _unavailableParameters;
        }
    }
    #endregion ReopeningInstructions
    #region PositionInstructions
    /// <summary>
    /// Instructions communes de fermeture et réouverture pour un contexte donné
    /// </summary>
    [SerializableAttribute()]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ClosingInstructions))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReopeningInstructions))]
    // EG 20190308 New
    // EG 20190318 Upd ClosingReopening Step3
    // EG 20190613 [24683] Upd
    // EG 20200318 [24683] Upd Add XMLComment
    /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    public abstract class PositionInstructions
    {
        #region Members
        [XmlAnyElement("modeXmlComment", Order = 1)]
        public XmlComment ModeXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlComment("Closing/Reopening mode : ReverseTrade/Trade | ReverseLongShortPosition/LongShortPosition | ReverseSyntheticPosition/SyntheticPosition")]
        [XmlElementAttribute("mode", Order = 2)]
        public TransferModeEnum Mode { get; set; }

        [XmlAnyElement("priceXmlComment", Order = 3)]
        public XmlComment PriceXmlComment { get { return GetType().GetXmlComment(); } set { } }
        
        [XmlElementAttribute("price", Order = 4)]
        [XmlComment("Price used for PremiumStyle|FutureStyleMarkToMarket|Others")]
        public PriceInstructions Price { get; set; }

        [XmlIgnoreAttribute()]
        public bool FeeActionSpecified { get; set; }

        [XmlAnyElement("feeActionXmlComment", Order = 5)]
        public XmlComment FeeActionXmlComment { get { return (FeeActionSpecified? GetType().GetXmlComment():null); } set { } }

        [XmlElementAttribute("feeAction", Order = 6)]
        [XmlComment("Fees application")]
        public FeeActionInstructions FeeAction { get; set; }
        #endregion Members

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation des paramètres du message NormMsgFactuory avec les élements de la classe
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public virtual MQueueparameter[] GetNormMsgFactoryParameters(string pSuffix)
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = new MQueueparameter($"MODE_{pSuffix}", TypeData.TypeDataEnum.@string);
            parameter.SetValue(Mode.ToString());
            parameters.Add(parameter);

            parameter = new MQueueparameter($"PREMIUMSTYLEPRICE_{pSuffix}", TypeData.TypeDataEnum.@string);
            parameter.SetValue(Price.EqtyPrice.ToString());
            parameters.Add(parameter);

            parameter = new MQueueparameter($"FUTURESTYLEMTMPRICE_{pSuffix}", TypeData.TypeDataEnum.@string);
            parameter.SetValue(Price.FutPrice.ToString());
            parameters.Add(parameter);

            parameter = new MQueueparameter($"OTHERPRICE_{pSuffix}", TypeData.TypeDataEnum.@string);
            parameter.SetValue(Price.OtherPrice.ToString());
            parameters.Add(parameter);

            if (FeeActionSpecified)
            {
                parameter = new MQueueparameter($"FEEACTION_{pSuffix}", TypeData.TypeDataEnum.@string);
                parameter.SetValue(FeeAction.identifier);
                parameters.Add(parameter);
            }
            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters, string pSuffix)
        {
            List<string> _unavailableParameters = new List<string>();
            Nullable<TransferModeEnum> _mode = null;
            Nullable<TransferPriceEnum> _price = null;
            // Mode
            MQueueparameter _parameter = pMQueueParameters[$"MODE_{pSuffix}"];
            if (null != _parameter)
                _mode = ReflectionTools.ConvertStringToEnumOrNullable<TransferModeEnum>(_parameter.Value);

            if (_mode.HasValue)
                Mode = _mode.Value;
            else
                _unavailableParameters.Add($"MODE_{pSuffix}");

            // Price
            Price = new PriceInstructions();
            _parameter = pMQueueParameters[$"PREMIUMSTYLEPRICE_{pSuffix}"];
            if (null != _parameter)
                _price = ReflectionTools.ConvertStringToEnumOrNullable<TransferPriceEnum>(_parameter.Value);

            if (_price.HasValue)
                Price.EqtyPrice = _price.Value;
            else
                _unavailableParameters.Add($"PREMIUMSTYLEPRICE_{pSuffix}");

            _price = null;
            _parameter = pMQueueParameters[$"FUTURESTYLEMTMPRICE_{pSuffix}"];
            if (null != _parameter)
                _price = ReflectionTools.ConvertStringToEnumOrNullable<TransferPriceEnum>(_parameter.Value);

            if (_price.HasValue)
                Price.FutPrice = _price.Value;
            else
                _unavailableParameters.Add($"FUTURESTYLEMTMPRICE_{pSuffix}");

            _price = null;
            _parameter = pMQueueParameters[$"OTHERPRICE_{pSuffix}"];
            if (null != _parameter)
                _price = ReflectionTools.ConvertStringToEnumOrNullable<TransferPriceEnum>(_parameter.Value);

            if (_price.HasValue)
                Price.OtherPrice = _price.Value;
            else
                _unavailableParameters.Add($"OTHERPRICE_{pSuffix}");

            // FeeAction
            FeeActionSpecified = (null != pMQueueParameters[$"FEEACTION_{pSuffix}"]);
            if (FeeActionSpecified)
            {
                FeeAction = new FeeActionInstructions();
                _unavailableParameters.AddRange(FeeAction.SetNormMsgFactoryParameters(pCS, pMQueueParameters, pSuffix));
            }

            return _unavailableParameters;
        }

    }
    #endregion PositionInstructions


    #region ResultActionRequest
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("results", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20190308 New
    // EG 20190318 Upd ClosingReopening Step3
    public class ResultActionRequest
    {
        #region Members
        [XmlAttribute("matching")]
        public double ResultMatching { get; set; }
        [XmlAttribute("totalTradesInPosition")]
        public int TotalTradesInPosition { get; set; }
        [XmlAttribute("totalTradesCandidates")]
        public int TotalTradesCandidates { get; set; }
        

        // IDT des trades potentiellement candidats à CLOSING/REOPENING
        [XmlIgnoreAttribute()]
        public List<int> LstIdTCandidate { get; set; }
        // Trades potentiellement candidats à CLOSING/REOPENING regroupés par Clé unique (voir classe TradeKey)
        // KEY   = TradeKey 
        // VALUE = List<ARQTools.TradeCandidate> = Liste des trades de même clé 
        [XmlElementAttribute("positions", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        public SerializableDictionary<ARQTools.TradeKey, List<ARQTools.TradeCandidate>> Positions { get; set; }
        #endregion Members
        #region Constructors
        public ResultActionRequest() 
        {
            LstIdTCandidate = new List<int>();
            Positions = new SerializableDictionary<ARQTools.TradeKey, List<ARQTools.TradeCandidate>>("position", "key", "trades", "http://www.efs.org/2007/EFSmL-3-0");
        }
        #endregion Constructors
    }
    #endregion ResultActionRequest

    #region ClosingReopeningAction
    /// <summary>
    /// Contient les caractéristiques générales d'une fermeture/réouverture de position
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("closingReopeningAction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20190308 New
    // EG 20190613 [24683] Upd
    // EG 20200318 [24683] Upd Add XMLComment
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
    // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
    public class ClosingReopeningAction : ActionRequest
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool EnvironmentSpecified { get; set; }

        [XmlAnyElement("environmentXmlComment", Order = 1)]
        public XmlComment EnvironmentXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("environment", Order = 2)]
        [XmlComment("Instrumental environment parameters")]
        public EnvironmentInstructions Environment { get; set; }

        [XmlIgnoreAttribute()]
        public bool EntitySpecified { get; set; }

        [XmlAnyElement("entityXmlComment", Order = 3)]
        public XmlComment EntityXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("entity", Order = 4)]
        [XmlComment("Entity concerned")]
        public int Entity { get; set; }

        [XmlAnyElement("closingXmlComment", Order = 5)]
        public XmlComment ClosingXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("closing", Order = 6)]
        [XmlComment("Position closing instruction parameters")]
        public ClosingInstructions Closing { get; set; }

        [XmlIgnoreAttribute()]
        public bool ReopeningSpecified { get; set; }

        [XmlAnyElement("reopeningXmlComment", Order = 7)]
        public XmlComment ReopeningXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("reopening", Order = 8)]
        [XmlComment("Position reopening instruction parameters")]
        public ReopeningInstructions Reopening { get; set; }

        [XmlIgnoreAttribute()]
        public bool ResultsSpecified { get; set; }

        [XmlAnyElement("resultsXmlComment", Order = 9)]
        public XmlComment ResultsXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("results", Order = 10)]
        [XmlComment("Results report")]
        public ResultActionRequest Results { get; set; }

        // Variables de travail (non sérialisées)
        [XmlIgnoreAttribute()]
        public NormMsgFactoryMQueue normMsgFactoryMQueue;

        [XmlIgnoreAttribute()]
        public bool ArqFilterSpecified { get; set; }

        [XmlAnyElement("arqFilterXmlComment", Order = 11)]
        public XmlComment ArqFilterXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlElementAttribute("arqFilter", Order = 12)]
        [XmlComment("Additional filter for trades candidates")]
        public ActionFilter ArqFilter { get; set; }

        #endregion Members

        #region Accessors
        #region DisplayMode
        public string DisplayMode
        {
            get
            {
                string ret = Closing.Mode.ToString();
                if (ReopeningSpecified)
                    ret += " / " + Reopening.Mode.ToString();
                return ret;
            }
        }
        public string DisplayNbPositionAndTrades
        {
            get
            {
                string ret = Results.Positions.Count + " / ";
                ret += (from position in Results.Positions
                         select position.Value.Count).Sum().ToString();
                return ret;
            }
        }
        #endregion DisplayMode
        #region TypeContract
        [XmlIgnoreAttribute()]
        public bool IsCommodityContract
        {
            get
            {
                return IsTypeContract(TypeContractEnum.CommodityContract);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsDerivativeContract
        {
            get
            {
                return IsTypeContract(TypeContractEnum.DerivativeContract);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsGrpContract
        {
            get
            {
                return IsTypeContract(TypeContractEnum.GrpContract);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsGrpMarket
        {
            get
            {
                return IsTypeContract(TypeContractEnum.GrpMarket);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsMarket
        {
            get
            {
                return IsTypeContract(TypeContractEnum.Market);
            }
        }
        [XmlIgnoreAttribute()]
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public bool IsIsinUnderlyerContract
        {
            get
            {
                return IsTypeContract(TypeContractEnum.IsinUnderlyerContract);
            }
        }
        #endregion TypeContract
        #region TypeInstr
        [XmlIgnoreAttribute()]
        public bool IsInstr
        {
            get
            {
                return IsTypeInstr(TypeInstrEnum.Instr);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsGrpInstr
        {
            get
            {
                return IsTypeInstr(TypeInstrEnum.GrpInstr);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsGrpProduct
        {
            get
            {
                return IsTypeInstr(TypeInstrEnum.GrpProduct);
            }
        }
        #endregion TypeInstr
        #region TypeDealer
        [XmlIgnoreAttribute()]
        public bool IsDealerGrpActor
        {
            get
            {
                return Closing.dealerSpecified && (TypePartyEnum.GrpActor == Closing.dealer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsDealerGrpBook
        {
            get
            {
                return Closing.dealerSpecified && (TypePartyEnum.GrpBook == Closing.dealer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsDealerActor
        {
            get
            {
                return Closing.dealerSpecified && (TypePartyEnum.Actor == Closing.dealer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsDealerBook
        {
            get
            {
                return Closing.dealerSpecified && (TypePartyEnum.Book == Closing.dealer.type);
            }
        }
        #endregion TypeDealer
        #region TypeClearer
        [XmlIgnoreAttribute()]
        public bool IsClearerGrpActor
        {
            get
            {
                return Closing.clearerSpecified && (TypePartyEnum.GrpActor == Closing.clearer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsClearerGrpBook
        {
            get
            {
                return Closing.clearerSpecified && (TypePartyEnum.GrpBook == Closing.clearer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsClearerActor
        {
            get
            {
                return Closing.clearerSpecified &&  (TypePartyEnum.Actor == Closing.clearer.type);
            }
        }
        [XmlIgnoreAttribute()]
        public bool IsClearerBook
        {
            get
            {
                return Closing.clearerSpecified && (TypePartyEnum.Book == Closing.clearer.type);
            }
        }
        #endregion TypeClearer
        #region TypeCssCustodian
        [XmlIgnoreAttribute()]
        // EG 20240520 [WI930] New
        public bool IsCssCustodianGrpActor
        {
            get
            {
                return Closing.cssCustodianSpecified && (TypePartyEnum.GrpActor == Closing.cssCustodian.type);
            }
        }
        [XmlIgnoreAttribute()]
        // EG 20240520 [WI930] New
        public bool IsCssCustodianActor
        {
            get
            {
                return Closing.clearerSpecified && (TypePartyEnum.Actor == Closing.cssCustodian.type);
            }
        }
        #endregion TypeCssCustodian
        #region HasDataToProcess
        /// <summary>
        /// Il existe des trades à traiter sur ce contexte
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool HasDataToProcess
        {
            get
            {
                return (0 < Results.ResultMatching) && (null != Results.Positions) && (0 < Results.Positions.Count);
            }
        }
        #endregion HasDataToProcess
        #region Accessors
        #region ResultMatchingName
        public string ResultMatchingName
        {
            get { return (ResultsSpecified && (Results.ResultMatching > 0)) ? "MATCH" : "NOMATCH";}
        }
        #endregion ResultMatchingName
        #region RequestTypeName
        public string RequestTypeName
        {
            get { return ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(requestType);}
        }
        #endregion RequestTypeName
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
        public bool IsRequestTypeAndTimingAvailable
        {
            get { return (IsRequestTypeAvailable && IsTimingAvailable) || IsRequestTypeAndTimingCombined; }
        }
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public bool IsRequestTypeAvailable
        {
            get { return (requestType == Cst.PosRequestTypeEnum.ClosingPosition) || (requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition); }
        }
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
        public bool IsTimingAvailable
        {
            get { return (timing == SettlSessIDEnum.StartOfDay) || (timing == SettlSessIDEnum.EndOfDay);}
        }
        // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
        public bool IsRequestTypeAndTimingCombined
        {
            get { return (requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition) && (timing == SettlSessIDEnum.EndOfDayPlusStartOfDay); }
        }

        #endregion Accessors

        #endregion Accessors
        #region Constructors
        public ClosingReopeningAction() { }
        public ClosingReopeningAction(string pCS):base(pCS)
        {
        }
        #endregion Constructors
        #region Methods
        #region ConstructNormMsgFactoryMessage
        /// <summary>
        /// Création d'un message de Type NORMMSGFACTORY avec Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE
        /// destiné à l'intégration d'un Clôture/réouverture de position
        /// </summary>
        /// EG 20140518 [19913]
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public Cst.ErrLevel ConstructNormMsgFactoryMessage()
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = _CS,
                id = IdARQ,
                identifier = identifier
            };

            normMsgFactoryMQueue = new NormMsgFactoryMQueue(mQueueAttributes)
            {
                buildingInfo = new NormMsgBuildingInfo()
                {
                    id = IdARQ,
                    idSpecified = true,
                    identifierSpecified = true,
                    identifier = identifier,
                    processType = Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE
                }
            };

            normMsgFactoryMQueue.buildingInfo.parametersSpecified = true;
            normMsgFactoryMQueue.buildingInfo.parameters = new MQueueparameters();
            normMsgFactoryMQueue.buildingInfo.parameters.Add(GetNormMsgFactoryParameters());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructNormMsgFactoryMessage
        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Création des paramètres du message NORMMSGFACTORY contenant l'ensemble des caractéristiques des
        /// instruction de Closing/Reopening
        /// </summary>
        /// <returns>Array de paramètres</returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = new MQueueparameter("REQUESTTYPE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(requestType));
            parameters.Add(parameter);

            parameter = new MQueueparameter("TIMING", TypeData.TypeDataEnum.@string);
            parameter.SetValue(ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(timing));
            parameters.Add(parameter);

            parameter = new MQueueparameter("EFFECTIVEDATE", TypeData.TypeDataEnum.@date);
            parameter.SetValue(effectiveDate);
            parameters.Add(parameter);

            if (effectiveEndDateSpecified)
            {
                parameter = new MQueueparameter("EFFECTIVEENDDATE", TypeData.TypeDataEnum.@date);
                parameter.SetValue(effectiveEndDate);
                parameters.Add(parameter);
            }

            parameter = new MQueueparameter("READYSTATE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(readystate.ToString());
            parameters.Add(parameter);

            parameter = new MQueueparameter("IDENTIFICATION", TypeData.TypeDataEnum.@string);
            parameter.SetValue(identifier);
            parameter.displayNameSpecified = displaynameSpecified;
            parameter.displayName = displayname;
            parameters.Add(parameter);

            if (descriptionSpecified)
            {
                parameter = new MQueueparameter("DOCUMENTATION", TypeData.TypeDataEnum.@string);
                parameter.SetValue(description);
                parameters.Add(parameter);
            }

            // Environnement
            if (EnvironmentSpecified)
                parameters.Add(Environment.GetNormMsgFactoryParameters());

            // Closing instructions
            parameters.Add(Closing.GetNormMsgFactoryParameters());

            // Reopening instructions
            if (ReopeningSpecified)
                parameters.Add(Reopening.GetNormMsgFactoryParameters());

            // Filter instructions
            if (ArqFilterSpecified)
                parameters.Add(ArqFilter.GetNormMsgFactoryParameters());
            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        /// <summary>
        /// Alimentation de la classe sur la base des paramètres du message NormMsgFactory
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMQueueParameters"></param>
        /// <returns></returns>
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        public List<string> SetNormMsgFactoryParameters(string pCS, MQueueparameters pMQueueParameters)
        {
            List<string> _unavailableParameters = new List<string>();

            // RequestTYpe (ClosingPosition|ClosingReopeningPosition)
            MQueueparameter _parameter = pMQueueParameters["REQUESTTYPE"];
            if (null != _parameter)
                requestType = ReflectionTools.ConvertStringToEnumOrDefault<Cst.PosRequestTypeEnum>(_parameter.Value);

            // Timing (StartOfDay|EndOfDay)
            _parameter = pMQueueParameters["TIMING"];
            if (null != _parameter)
                timing = ReflectionTools.ConvertStringToEnumOrDefault<FixML.Enum.SettlSessIDEnum>(_parameter.Value);

            _parameter = pMQueueParameters["READYSTATE"];
            if (null != _parameter)
                readystate = ARQTools.ReadyState(_parameter.Value).Value;
            else
                readystate = ARQTools.ActionRequestReadyStateEnum.REGULAR;

            // Date d'effet
            _parameter = pMQueueParameters["EFFECTIVEDATE"];
            if (null != _parameter)
                effectiveDate = new EFS_Date(_parameter.Value).DateValue;
            else
                _unavailableParameters.Add("EFFECTIVEDATE");

            _parameter = pMQueueParameters["EFFECTIVEENDDATE"];
            if (null != _parameter)
                effectiveEndDate = new EFS_Date(_parameter.Value).DateValue;

            // Identification (Identifier and Displayname)
            _parameter = pMQueueParameters["IDENTIFICATION"];
            if (null != _parameter)
            {
                identifierSpecified = true;
                identifier = _parameter.Value;
                displaynameSpecified = _parameter.displayNameSpecified;
                if (displaynameSpecified)
                    displayname = _parameter.displayName;
            }


            // Documentation
            _parameter = pMQueueParameters["DOCUMENTATION"];
            if (null != _parameter)
            {
                descriptionSpecified = true;
                description = _parameter.Value;
            }

            if (IsRequestTypeAndTimingAvailable)
            {
                // Product/Instr Type & Market/Contract type
                Environment = new EnvironmentInstructions();
                _unavailableParameters.AddRange(Environment.SetNormMsgFactoryParameters(pCS, pMQueueParameters));
                EnvironmentSpecified = Environment.contractSpecified || Environment.instrSpecified;

                // Closing
                Closing = new ClosingInstructions();
                _unavailableParameters.AddRange(Closing.SetNormMsgFactoryParameters(pCS, pMQueueParameters));

                // Reopening
                ReopeningSpecified = (requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition);
                if (ReopeningSpecified)
                {
                    Reopening = new ReopeningInstructions();
                    _unavailableParameters.AddRange(Reopening.SetNormMsgFactoryParameters(pCS, pMQueueParameters));
                }
            }
            else
            {
                if (false == IsRequestTypeAvailable)
                    _unavailableParameters.Add("REQUESTTYPE");
                if (false == IsTimingAvailable)
                    _unavailableParameters.Add("TIMING");
            }

            // ARQFilter
            _parameter = pMQueueParameters["FILTERNAME"];
            if (null != _parameter)
            {
                ArqFilter = new ActionFilter();
                _unavailableParameters.AddRange(ArqFilter.SetNormMsgFactoryParameters(pMQueueParameters));
            }

            return _unavailableParameters;
        }



        #region SetActionRequest
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        public override void SetActionRequest(DataRow pRow)
        {
            base.SetActionRequest(pRow);
            SetBuildInfo(pRow);
            SetArqFilterInstructions(pRow);
            SetEnvironmentInstructions(pRow);
            SetEntityInstructions(pRow);
            SetClosingInstructions(pRow);
            SetReopeningInstructions(pRow);
        }
        #endregion SetActionRequest
        #region SetArqFilterInstructions
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        protected void SetArqFilterInstructions(DataRow pRow)
        {
            ArqFilterSpecified = (false == Convert.IsDBNull(pRow["ARQFILTER"]));
            if (ArqFilterSpecified)
            {
                ArqFilter = new ActionFilter
                {
                    identifierSpecified = true,
                    identifier = pRow["ARQFILTER"].ToString(),
                    // Use in SpheresClosingGen services only
                    displaynameSpecified = pRow.Table.Columns.Contains("DN_FILTER") && StrFunc.IsFilled(pRow["DN_FILTER"].ToString()),
                    descriptionSpecified = pRow.Table.Columns.Contains("DESC_FILTER") && StrFunc.IsFilled(pRow["DESC_FILTER"].ToString()),
                    qrySpecified = pRow.Table.Columns.Contains("QRY_FILTER") && StrFunc.IsFilled(pRow["QRY_FILTER"].ToString()),
                    dtDisabledSpecified = pRow.Table.Columns.Contains("DTDISABLED_FILTER") && StrFunc.IsFilled(pRow["DTDISABLED_FILTER"].ToString())
                };
                if (ArqFilter.displaynameSpecified)
                    ArqFilter.displayname = pRow["DN_FILTER"].ToString();
                if (ArqFilter.descriptionSpecified)
                    ArqFilter.description = pRow["DESC_FILTER"].ToString();
                if (ArqFilter.qrySpecified)
                    ArqFilter.qry = pRow["QRY_FILTER"].ToString();
                if (pRow.Table.Columns.Contains("DTENABLED_FILTER"))
                    ArqFilter.dtEnabled = Convert.ToDateTime(pRow["DTENABLED_FILTER"]);
                if (ArqFilter.dtDisabledSpecified)
                    ArqFilter.dtDisabled = Convert.ToDateTime(pRow["DTDISABLED_FILTER"]);
            }
        }
        #endregion SetArqFilterInstructions

        #region SetEnvironmentInstructions
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected void SetEnvironmentInstructions(DataRow pRow)
        {
            Environment = new EnvironmentInstructions
            {
                instrSpecified = (false == Convert.IsDBNull(pRow["TYPEINSTR"])),
                contractSpecified = (false == Convert.IsDBNull(pRow["TYPECONTRACT"]))
            };
            if (Environment.instrSpecified)
            {
                Environment.instr = new TypeInstrument
                {
                    type = ReflectionTools.ConvertStringToEnum<TypeInstrEnum>(pRow["TYPEINSTR"].ToString())
                };
                if (false == Convert.IsDBNull(pRow["IDINSTR"]))
                    Environment.instr.value = pRow["IDINSTR"].ToString();
            }
            if (Environment.contractSpecified)
            {
                Environment.contract = new TypeContract
                {
                    type = ReflectionTools.ConvertStringToEnum<TypeContractEnum>(pRow["TYPECONTRACT"].ToString())
                };
                if (false == Convert.IsDBNull(pRow["IDCONTRACT"]))
                    Environment.contract.value = Convert.ToInt32(pRow["IDCONTRACT"].ToString());
            }
            EnvironmentSpecified = Environment.IsSpecified;
        }
        #endregion SetInstrumentalInstructions
        #region SetEntityInstructions
        protected void SetEntityInstructions(DataRow pRow)
        {
            EntitySpecified = (false == Convert.IsDBNull(pRow["IDA_ENTITY"]));
            if (EntitySpecified)
                Entity = Convert.ToInt32(pRow["IDA_ENTITY"]);
        }
        #endregion SetEntityInstructions
        #region SetClosingInstructions
        // EG 20190318 Upd ClosingReopening Step3
        // EG 20190613 [24683] Upd isSumClosingAmount
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected void SetClosingInstructions(DataRow pRow)
        {
            EntitySpecified = (false == Convert.IsDBNull(pRow["IDA_ENTITY"]));
            if (EntitySpecified)
                Entity = Convert.ToInt32(pRow["IDA_ENTITY"]);

            Closing = new ClosingInstructions();

            SetCommonPositionInstructions(pRow, Closing);

            Closing.dealerSpecified = (false == Convert.IsDBNull(pRow["TYPEDEALER_C"]));
            if (Closing.dealerSpecified)
            {
                Closing.dealer = new ActorInstructions
                {
                    type = ReflectionTools.ConvertStringToEnum<TypePartyEnum>(pRow["TYPEDEALER_C"].ToString())
                };
                if (false == Convert.IsDBNull(pRow["IDDEALER_C"]))
                    Closing.dealer.value = Convert.ToInt32(pRow["IDDEALER_C"]);
            }
            Closing.clearerSpecified = (false == Convert.IsDBNull(pRow["TYPECLEARER_C"]));
            if (Closing.clearerSpecified)
            {
                Closing.clearer = new ActorInstructions
                {
                    type = ReflectionTools.ConvertStringToEnum<TypePartyEnum>(pRow["TYPECLEARER_C"].ToString())
                };
                if (false == Convert.IsDBNull(pRow["IDCLEARER_C"]))
                    Closing.clearer.value = Convert.ToInt32(pRow["IDCLEARER_C"]);
            }
            // EG 20240520 [WI930] Set CssCustodian
            Closing.cssCustodianSpecified = (false == Convert.IsDBNull(pRow["TYPECSSCUSTODIAN_C"]));
            if (Closing.cssCustodianSpecified)
            {
                Closing.cssCustodian = new ActorInstructions
                {
                    type = ReflectionTools.ConvertStringToEnum<TypePartyEnum>(pRow["TYPECSSCUSTODIAN_C"].ToString())
                };
                if (false == Convert.IsDBNull(pRow["IDCSSCUSTODIAN_C"]))
                    Closing.cssCustodian.value = Convert.ToInt32(pRow["IDCSSCUSTODIAN_C"]);
            }
            Closing.isSumClosingAmountSpecified = (false == Convert.IsDBNull(pRow["ISSUMCLOSINGAMT_C"]));
            if (Closing.isSumClosingAmountSpecified)
                Closing.isSumClosingAmount = Convert.ToBoolean(pRow["ISSUMCLOSINGAMT_C"]);

            Closing.isDelistingSpecified = (false == Convert.IsDBNull(pRow["ISDELISTING_C"]));
            if (Closing.isDelistingSpecified)
                Closing.isDelisting = Convert.ToBoolean(pRow["ISDELISTING_C"]);
        }
        #endregion SetClosingInstructions
        #region SetReopeningInstructions
        // EG 20190318 Upd ClosingReopening Step3
        protected void SetReopeningInstructions(DataRow pRow)
        {
            ReopeningSpecified = (requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition);
            if (ReopeningSpecified)
            {
                Reopening = new ReopeningInstructions();

                SetCommonPositionInstructions(pRow, Reopening);

                Reopening.dealerSpecified = (false == Convert.IsDBNull(pRow["TYPEDEALER_O"]));
                if (Reopening.dealerSpecified)
                {
                    Reopening.dealer = new LinkActorInstructions
                    {
                        table = pRow["TYPEDEALER_O"].ToString()
                    };
                    if (false == Convert.IsDBNull(pRow["IDDEALER_O"]))
                        Reopening.dealer.column = pRow["IDDEALER_O"].ToString();
                }
                Reopening.clearerSpecified = (false == Convert.IsDBNull(pRow["TYPECLEARER_O"]));
                if (Reopening.clearerSpecified)
                {
                    Reopening.clearer = new LinkActorInstructions
                    {
                        table = pRow["TYPECLEARER_O"].ToString()
                    };
                    if (false == Convert.IsDBNull(pRow["IDCLEARER_O"]))
                        Reopening.clearer.column = pRow["IDCLEARER_O"].ToString();
                }
                // EG 20240520 [WI930] Set CssCustodian
                Reopening.cssCustodianSpecified = (false == Convert.IsDBNull(pRow["TYPECSSCUSTODIAN_O"]));
                if (Reopening.cssCustodianSpecified)
                {
                    Reopening.cssCustodian = new LinkActorInstructions
                    {
                        table = pRow["TYPECSSCUSTODIAN_O"].ToString(),
                        isCssCustodian = true
                    };
                    if (false == Convert.IsDBNull(pRow["IDCSSCUSTODIAN_O"]))
                        Reopening.cssCustodian.column = pRow["IDCSSCUSTODIAN_O"].ToString();
                }
            }
        }
        // EG 20190318 Upd ClosingReopening Step3
        // EG 20190613 [24683] Upd feeAction
        // EG 20231030 [WI725] New Closing/Reopening : Set feeAction.identifier
        protected void SetCommonPositionInstructions(DataRow pRow, PositionInstructions pPosition)
        {
            string suffix = (pPosition is ClosingInstructions) ? "_C" : "_O";
            pPosition.Mode = ReflectionTools.ConvertStringToEnum<TransferModeEnum>(pRow["MODE" + suffix].ToString());
            pPosition.Price = new PriceInstructions
            {
                EqtyPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pRow["EQTYPRICE" + suffix].ToString()),
                FutPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pRow["FUTPRICE" + suffix].ToString()),
                OtherPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pRow["OTHERPRICE" + suffix].ToString())
            };

            pPosition.FeeActionSpecified = (false == Convert.IsDBNull(pRow["FEEACTION" + suffix]));
            if (pPosition.FeeActionSpecified)
                pPosition.FeeAction = new FeeActionInstructions() {
                    value = Convert.ToInt32(pRow["FEEACTION" + suffix]),
                    identifier = normMsgFactoryMQueue.buildingInfo.parameters["FEEACTION" + suffix].Value,
                };
        }

        #endregion SetActionRequestEmbedded
        #region SetEntityInstructions
        protected void SetBuildInfo(DataRow pRow)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), pRow["BUILDINFO"].ToString());
            normMsgFactoryMQueue = (NormMsgFactoryMQueue)CacheSerializer.Deserialize(serializeInfo);
        }
        #endregion SetEntityInstructions

        #region Insert
        public override Cst.ErrLevel Insert(ARQInfo pARQInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            ActionRequestQuery arqQry = new ClosingReopeningRequestQuery(_CS);
            SQLUP.IdGetId idGetId = SQLUP.IdGetId.ACTIONREQUEST;
            IDbTransaction dbTransaction = null;
            bool isOk = true;
            try
            {
                dbTransaction = DataHelper.BeginTran(_CS);
                QueryParameters qryParameters = arqQry.GetQueryInsert();
                SQLUP.GetId(out int id, dbTransaction, idGetId);
                IdARQ = id;
                IdA = pARQInfo.IdA;
                arqQry.SetParametersInsert(this, qryParameters.Parameters);
                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                DataHelper.CommitTran(dbTransaction);
                ret = Cst.ErrLevel.SUCCESS;
            }
            catch (Exception) { isOk = false; throw; }
            finally
            {
                if (false == isOk)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
            return ret;
        }
        #endregion Insert
        #region Update
        public override Cst.ErrLevel Update(ARQInfo pARQInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            ActionRequestQuery arqQry = new ClosingReopeningRequestQuery(_CS);
            IDbTransaction dbTransaction = null;
            bool isOk = true;
            try
            {
                dbTransaction = DataHelper.BeginTran(_CS);
                QueryParameters qryParameters = arqQry.GetQueryUpdate();
                arqQry.SetParametersUpdate(this, qryParameters.Parameters);
                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                DataHelper.CommitTran(dbTransaction);
                ret = Cst.ErrLevel.SUCCESS;
            }
            catch (Exception) { isOk = false; throw; }
            finally
            {
                if (false == isOk)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
            return ret;
        }
        #endregion Update


        #region IsTypeContract
        public bool IsTypeContract(TypeContractEnum pCompare)
        {
            return EnvironmentSpecified && Environment.contractSpecified && (Environment.contract.type == pCompare);
        }
        #endregion IsTypeContract
        #region IsTypeInstr
        public bool IsTypeInstr(TypeInstrEnum pCompare)
        {
            return EnvironmentSpecified && Environment.instrSpecified && (Environment.instr.type == pCompare);
        }
        #endregion IsTypeInstr
        #endregion Methods

        #region IsSyntheticAction
        public bool IsSyntheticAction
        {
            get 
            {
                return (Closing.Mode != TransferModeEnum.ReverseTrade) || (ReopeningSpecified && (Reopening.Mode != TransferModeEnum.Trade));
            }
        }
        #endregion IsSyntheticAction
        #region IsMatch
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public MatchingEnum IsMatch(ARQTools.TradeContextEnum pContextEnum, ARQTools.TradeContext pTradeContext)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            switch (pContextEnum)
            {
                case ARQTools.TradeContextEnum.IdA_Entity:
                    #region Entity
                    if (EntitySpecified)
                        match = (Entity == pTradeContext.idA_Entity) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Entity
                    break;
                case ARQTools.TradeContextEnum.IdA_Dealer:
                    #region Dealer Actor
                    if (IsDealerActor)
                        match = (Closing.dealer.value == pTradeContext.idA_Dealer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Dealer Actor
                    break;
                case ARQTools.TradeContextEnum.IdB_Dealer:
                    #region Dealer Book
                    if (IsDealerBook)
                        match = (Closing.dealer.value == pTradeContext.idB_Dealer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Dealer Book
                    break;
                case ARQTools.TradeContextEnum.IdGrpActor_Dealer:
                    #region Dealer Groupe Actor
                    if (IsDealerGrpActor)
                        match = (Closing.dealer.value == pTradeContext.idGrpActor_Dealer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Dealer Groupe Actor
                    break;
                case ARQTools.TradeContextEnum.IdGrpBook_Dealer:
                    #region Dealer Groupe Book
                    if (IsDealerGrpBook)
                        match = (Closing.dealer.value == pTradeContext.idGrpBook_Dealer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Dealer Groupe Book
                    break;
                case ARQTools.TradeContextEnum.IdA_Clearer:
                    #region Clearer Actor
                    if (IsClearerActor)
                        match = (Closing.clearer.value == pTradeContext.idA_Clearer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Clearer Actor
                    break;
                case ARQTools.TradeContextEnum.IdB_Clearer:
                    #region Clearer Book
                    if (IsClearerBook)
                        match = (Closing.clearer.value == pTradeContext.idB_Clearer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Clearer Book
                    break;
                case ARQTools.TradeContextEnum.IdGrpActor_Clearer:
                    #region Clearer Groupe Actor
                    if (IsClearerGrpActor)
                        match = (Closing.clearer.value == pTradeContext.idGrpActor_Clearer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Clearer Groupe Actor
                    break;
                case ARQTools.TradeContextEnum.IdGrpBook_Clearer:
                    #region Clearer Groupe Book
                    if (IsClearerGrpBook)
                        match = (Closing.clearer.value == pTradeContext.idGrpBook_Clearer) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Clearer Groupe Book
                    break;
                case ARQTools.TradeContextEnum.GProduct:
                    #region Groupe Product
                    if (IsGrpProduct)
                        match = (Environment.instr.value == pTradeContext.gProduct) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Product
                    break;
                case ARQTools.TradeContextEnum.IdGrpInstr:
                    #region Groupe Instrument
                    if (IsGrpInstr)
                        match = (pTradeContext.idGrpInstr.HasValue && (Convert.ToInt32(Environment.instr.value) == pTradeContext.idGrpInstr.Value)) ? 
                            MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Instrument
                    break;
                case ARQTools.TradeContextEnum.IdI:
                    #region Instrument
                    if (IsInstr)
                        match = (Convert.ToInt32(Environment.instr.value) == pTradeContext.idI) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Instrument
                    break;
                case ARQTools.TradeContextEnum.IdGrpMarket:
                    #region Groupe Market
                    if (IsGrpMarket)
                        match = (pTradeContext.idGrpMarket.HasValue && (Environment.contract.value == pTradeContext.idGrpMarket.Value)) ? 
                        MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Market
                    break;
                case ARQTools.TradeContextEnum.IdM:
                    #region Market
                    if (IsMarket)
                        match = (Environment.contract.value == pTradeContext.idM) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Market
                    break;
                case ARQTools.TradeContextEnum.IdGrpContract:
                    #region Groupe Contract
                    if (IsGrpContract)
                        match = (pTradeContext.idGrpContract.HasValue && (Environment.contract.value == pTradeContext.idGrpContract.Value)) ? 
                            MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Contract
                    break;
                case ARQTools.TradeContextEnum.IdDerivativeContract:
                    #region DerivativeContract
                    if (IsDerivativeContract)
                        match = (Environment.contract.value == pTradeContext.idDerivativeContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion DerivativeContract
                    break;
                case ARQTools.TradeContextEnum.IdCommodityContract:
                    #region CommodityContract
                    if (IsCommodityContract)
                        match = (Environment.contract.value == pTradeContext.idCommodityContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion CommodityContract
                    break;
                // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
                case ARQTools.TradeContextEnum.IsinUnderlyerContract:
                    if (IsIsinUnderlyerContract)
                        match = (Environment.contract.value == pTradeContext.isinUnderlyerContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    break;
            }
            return match;
        }
        #endregion IsMatch
        #region Match
        public MatchingEnum Match(ARQTools.TradeContextEnum pContext, int pWeight, ARQTools.TradeContext pTradeContext)
        {
            MatchingEnum match = IsMatch(pContext, pTradeContext);
            if (MatchingEnum.HiMatch == match)
                Results.ResultMatching += Math.Pow(Convert.ToDouble(2), Convert.ToDouble(pWeight));
            else if (MatchingEnum.UnMatch == match)
                Results.ResultMatching = 0;
            return match;
        }
        #endregion Match
        #region AssembleTrade
        /// <summary>
        /// Assemblage des trades candidats entre eux en fonction de leur clé de 
        /// Clé de merge = IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDASSET, ASSETCATEGORY 
        /// </summary>
        /// <param name="pDicTradeCandidate">Dictionnaire des trades candidats à merge (pour l'ensemble des contextes)</param>
        /// <param name="pLstTradeKeyGroup">Liste des "clé unique de merge" rencontrées dans les trade candidats</param>
        // EG 20190318 Upd ClosingReopening Step3
        public void AssembleTrade(Dictionary<int, ARQTools.TradeCandidate> pDicTradeCandidate, List<IGrouping<ARQTools.TradeKey, ARQTools.TradeKey>> pLstTradeKeyGroup)
        {
            Results.LstIdTCandidate.ForEach(idTCandidate =>
            {
                ARQTools.TradeCandidate tradeCandidate = pDicTradeCandidate[idTCandidate];
                // Pair (idt, identifier)
                //Pair<int, string> _idT = new Pair<int, string>(trade, tradeCandidate.TradeIdentifier);
                pLstTradeKeyGroup.ForEach(tradeKeyGroup =>
                {
                    if (new ARQTools.TradeKeyComparer().Equals(tradeCandidate.posKeys, tradeKeyGroup.Key))
                    {
                        if (null == Results.Positions)
                        {
                            Results.Positions =
                                new SerializableDictionary<ARQTools.TradeKey, List<ARQTools.TradeCandidate>>("position", "key", "trades", "http://www.efs.org/2007/EFSmL-3-0");
                        }
                        if (false == Results.Positions.ContainsKey(tradeKeyGroup.Key))
                        {
                            List<ARQTools.TradeCandidate> _lst = new List<ARQTools.TradeCandidate>
                            {
                                tradeCandidate
                            };
                            Results.Positions.Add(tradeKeyGroup.Key, _lst);
                        }
                        else
                        {
                            Results.Positions[tradeKeyGroup.Key].Add(tradeCandidate);
                        }
                    }
                });
            });
        }
        #endregion AssembleTrade

        #region WeightingPlus
        /// <summary>
        /// Contrôle si existence d'un filtre et application de celui-ci sur 
        /// le trade est candidat avant de calculer le poid du périmètre 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeContext"></param>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        public void WeightingPlus(string pCS, DateTime pDtBusiness, int pIdT, ARQTools.TradeContext pTradeContext)
        {
            bool _isMatch = ApplyFilterBeforeWeighting(pCS, pDtBusiness, pIdT);
            if (_isMatch)
                Weighting(pTradeContext);
        }
        #endregion WeightingPlus
        #region Weighting
        public void Weighting(ARQTools.TradeContext pTradeContext)
        {
            Results.ResultMatching = 1;
            int _weight = 0;
            ARQTools.TradeContextEnum _name = ARQTools.TradeContextEnum.None;
            List<FieldInfo> flds = pTradeContext.GetType().GetFields().ToList();
            bool _isMatch = true;
            flds.ForEach(fld =>
            {
                if (_isMatch)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(ARQTools.TradeContextWeight), false);
                    if (0 != attributes.GetLength(0))
                    {
                        _weight = ((ARQTools.TradeContextWeight)attributes[0]).Weight;
                        _name = ((ARQTools.TradeContextWeight)attributes[0]).Name;
                        if (MatchingEnum.UnMatch == Match(_name, _weight, pTradeContext))
                            _isMatch = false;
                    }
                }
            });
        }
        #endregion Weighting

        #region ApplyFilterBeforeWeighting
        /// <summary>
        /// Application du filtre sur le trade potentiel candidat
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        public bool ApplyFilterBeforeWeighting(string pCS, DateTime pDtBusiness, int pIdT)
        {
            bool isTradeOk = true;
            if (ArqFilterSpecified)
            {
                // Le filtre est actif
                if ((pDtBusiness.CompareTo(ArqFilter.dtEnabled) >= 0) && ((false == ArqFilter.dtDisabledSpecified) || (pDtBusiness.CompareTo(ArqFilter.dtDisabled) < 0)))
                {
                    isTradeOk = false;
                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                    parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDtBusiness);
                    string sqlSelect = $@"select tr.IDT 
                    from TRADE tr
                    {ArqFilter.qry}
                    where (tr.IDT = @IDT)";
                    QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                    object obj = DataHelper.ExecuteScalar(_CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    if (null != obj)
                        isTradeOk = BoolFunc.IsTrue(obj);
                }
            }
            return isTradeOk; 
        }
        #endregion ApplyFilterBeforeWeighting
    }
    #endregion ClosingReopeningAction
}
