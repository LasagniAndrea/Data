#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using FixML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// collection of PartyTemplate
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("PARTYTEMPLATES", Namespace = "", IsNullable = true)]
    public class PartyTemplates
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("PARTYTEMPLATE", Order = 1)]
        public PartyTemplate[] partyTemplate;


        /// <summary>
        /// 
        /// </summary>
        public PartyTemplates() { }




        /// <summary>
        /// Retourne les enregistrements dans PARTYTEMPLATE pour l'acteur p<paramref name="pIdA"/>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdA">Représente la contrepatrie, doit être supérieur à 0</param>
        /// <param name="pScanDataDtEnabled"></param>
        private void Load(string pCs, int pIdA, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            PartyTemplates partyTemplates = LoadPartyTemplates(pCs, pIdA,  pScanDataDtEnabled);
            partyTemplate = partyTemplates.partyTemplate;
        }
        /// <summary>
        /// Retourne les enregistrements dans PARTYTEMPLATE pour l'acteur p<paramref name="pIdA"/>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdA">Représente la contrepatrie, doit être supérieur à 0</param>
        /// <param name="pScanDataDtEnabled"></param>
        /// <returns></returns>
        private static PartyTemplates LoadPartyTemplates(string pCs,  int pIdA, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {

            PartyTemplates ret;

            DataParameters sqlParam = new DataParameters();
            sqlParam.Add(new DataParameter(pCs, "IDA", DbType.Int32), pIdA);

            //where
            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("(pt.IDA=@IDA)");
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "pt"));

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "pt.IDPARTYTEMPLATE," + Cst.CrLf;
            sqlSelect += "pt.IDA, " + Cst.CrLf;
            sqlSelect += "pt.IDA_ENTITY," + Cst.CrLf;
            sqlSelect += "pt.IDI, pt.IDM," + Cst.CrLf;
            sqlSelect += "pt.IDB,b.IDENTIFIER as BOOKIDENTIFIER," + Cst.CrLf;
            sqlSelect += "pt.IDA_TRADER,t.IDENTIFIER as TRADERIDENTIFIER, t.DISPLAYNAME as TRADERDISPLAYNAME" + Cst.CrLf;

            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.PARTYTEMPLATE.ToString() + " pt" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDB = pt.IDB" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " t on t.IDA = pt.IDA_TRADER" + Cst.CrLf;

            sqlSelect += sqlWhere;

            QueryParameters qry = new QueryParameters(pCs, sqlSelect.ToString(), sqlParam);

            DataSet dsResult = DataHelper.ExecuteDataset(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            dsResult.DataSetName = "PARTYTEMPLATES";

            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "PARTYTEMPLATE";

            string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(PartyTemplates), dsSerializerResult);
            ret = (PartyTemplates)CacheSerializer.Deserialize(serializeInfo);

            return ret;
        }

        /// <summary>
        ///  Retourne l'enregistrement PARTYTEMPLATE prioritaire dans le contexte IDA, IDI, IDM afin de proposer un book 
        /// </summary>
        /// <remarks>l'enregistrement retourné ne possède pas forcément un book</remarks>
        /// <param name="pCS"></param>
        /// <param name="idA"></param>
        /// <param name="idI"></param>
        /// <param name="idM"></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        public static PartyTemplate FindBook(string pCS, int idA, int idI, int idM)
        {
            PartyTemplate ret = null;

            PartyTemplates partyTemplates = new PartyTemplates();
            partyTemplates.Load(pCS, idA, SQL_Table.ScanDataDtEnabledEnum.Yes);

            if (null != partyTemplates.partyTemplate)
            {
                IEnumerable<PartyTemplate> partyTemplateFind = partyTemplates.partyTemplate
                .Where(x => (
                             ((x.idISpecified && x.idI == idI) || (false == x.idISpecified)) &&
                             ((x.idMSpecified && x.idM == idM) || (false == x.idMSpecified))
                            )
                )
                .OrderByDescending(x => GetOrderByDescendingUsing_IdI_IdM(x));

                if (partyTemplateFind.Count() == 1)
                {
                    ret = partyTemplateFind.First();
                }
                else if (partyTemplateFind.Count() > 1)
                {
                    /* recherche d'un enregistrement de même priorité avec book différent => dans ce cas Spheres® ne remonte pas d'enregistrement
                     Exemple  soit 2 enregistrements qui portent sur  instr:Future, marché:<tous marchés>. 
                     si l'enregistrement 1 contient Book1 et l'enregistrement contient Book2 => Spheres ne propose pas de book
                     */
                    ret = partyTemplateFind.First();
                    if (partyTemplates.partyTemplate
                        .Where(x => x.idI == ret.idI && x.idM == ret.idM && x.idB != ret.idB).Count() > 0)
                    {
                        ret = null;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne l'enregistrement PARTYTEMPLATE prioritaire dans le contexte IDA, IDI, IDM, IDB, IdAEntity afin de proposer un trader
        /// </summary>
        /// <remarks>l'enregistrement retourné ne possède pas forcément un trader</remarks>
        /// <param name="pCS"></param>
        /// <param name="idA">Acteur Dealer</param>
        /// <param name="idI">Instruement courant</param>
        /// <param name="idM">Marché courant</param>
        /// <param name="idB">book courant </param>
        /// <param name="idAEntity">Entité de <paramref name="idB"/></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        public static PartyTemplate FindTrader(string pCS, int idA, int idI, int idM, int idB, int idAEntity)
        {
            PartyTemplate ret = null;

            PartyTemplates partyTemplates = new PartyTemplates();
            partyTemplates.Load(pCS, idA, SQL_Table.ScanDataDtEnabledEnum.Yes);

            if (null != partyTemplates.partyTemplate)
            {

                IEnumerable<PartyTemplate> partyTemplateFind = partyTemplates.partyTemplate
                     .Where(x => (
                                  ((x.idISpecified && x.idI == idI) || (false == x.idISpecified)) &&
                                  ((x.idMSpecified && x.idM == idM) || (false == x.idMSpecified)) &&
                                  ((x.idBSpecified && x.idB == idB) || (false == x.idBSpecified)) &&
                                  (x.idAEntity == idAEntity))
                      )
                     .OrderByDescending(x => GetOrderByDescendingUsing_IdI_IdM_IDB(x));

                if (partyTemplateFind.Count() == 1)
                {
                    ret = partyTemplateFind.First();
                }
                else if (partyTemplateFind.Count() > 1)
                {
                    /* recherche d'un enregistrement de même priorité avec trader différent => dans ce cas Spheres® ne remonte pas d'enregistrement */
                    ret = partyTemplateFind.First();
                    if (partyTemplates.partyTemplate
                        .Where(x => x.idI == ret.idI && x.idM == ret.idM && x.idAEntity == ret.idAEntity && x.idB == ret.idB && x.idATrader != ret.idATrader).Count() > 0)
                    {
                        ret = null;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne l'enregistrement PARTYTEMPLATE sans Trader prioritaire dans le contexte IDA, IDI, IDM, IDB, IdAEntity afin d'obtenir les sales rattachés
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="idA">Acteur Dealer</param>
        /// <param name="idI">Instruement courant</param>
        /// <param name="idM">Marché courant</param>
        /// <param name="idB">book courant </param>
        /// <param name="idAEntity">Entité de <paramref name="idB"/></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        public static PartyTemplate FindSalesNoTrader(string pCS, int idA, int idI, int idM, int idB, int idAEntity)
        {
            PartyTemplate ret = null;

            PartyTemplates partyTemplates = new PartyTemplates();
            partyTemplates.Load(pCS, idA, SQL_Table.ScanDataDtEnabledEnum.Yes);

            if (null != partyTemplates.partyTemplate)
            {

                IEnumerable<PartyTemplate> partyTemplateFind = partyTemplates.partyTemplate
                     .Where(x => (
                                  ((x.idISpecified && x.idI == idI) || (false == x.idISpecified)) &&
                                  ((x.idMSpecified && x.idM == idM) || (false == x.idMSpecified)) &&
                                  ((x.idBSpecified && x.idB == idB) || (false == x.idBSpecified)) &&
                                  (x.idAEntity == idAEntity) &&
                                  (false == x.idATraderSpecified)
                                 )
                      )
                     .OrderByDescending(x => GetOrderByDescendingUsing_IdI_IdM_IDB(x));

                if (partyTemplateFind.Count() == 1)
                {
                    ret = partyTemplateFind.First();
                }
                else if (partyTemplateFind.Count() > 1)
                {
                    /* Recherche d'un enregistrement de même priorité => dans ce cas Spheres® ne remonte pas d'enregistrement 
                     Exemple il existe 2 enregistrements tels que IDI = instrument courant, <TousMarché>, IDA_ENTITY = entité courante. Spheres® ne choisit pas plus un enregistrement que l'autre
                     Remarque ici  idATrader vaut nécessairement 0
                     */
                    ret = partyTemplateFind.First();
                    if (partyTemplates.partyTemplate
                        .Where(x => x.idI == ret.idI && x.idM == ret.idM && x.idAEntity == ret.idAEntity && x.idB == ret.idB && x.idATrader == ret.idATrader
                        && x.idPartyTemplate != ret.idPartyTemplate).Count() > 0)
                    {
                        ret = null;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        ///  Retourne l'enregistrement PARTYTEMPLATE prioritaire dans le contexte IDA, IDI, IDM, IDB, IdAEntity, idATrader afin d'obtenir les sales rattachés
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="idA"></param>
        /// <param name="idI"></param>
        /// <param name="idM"></param>
        /// <param name="idB"></param>
        /// <param name="idAEntity"></param>
        /// <param name="idATrader"></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        public static PartyTemplate FindSales(string pCS, int idA, int idI, int idM, int idB, int idAEntity, int idATrader)
        {
            PartyTemplate ret = null;

            PartyTemplates partyTemplates = new PartyTemplates();
            partyTemplates.Load(pCS, idA, SQL_Table.ScanDataDtEnabledEnum.Yes);

            if (null != partyTemplates.partyTemplate)
            {

                IEnumerable<PartyTemplate> partyTemplateFind = partyTemplates.partyTemplate
                     .Where(x => (
                                  ((x.idISpecified && x.idI == idI) || (false == x.idISpecified)) &&
                                  ((x.idMSpecified && x.idM == idM) || (false == x.idMSpecified)) &&
                                  ((x.idBSpecified && x.idB == idB) || (false == x.idBSpecified)) &&
                                  (x.idAEntity == idAEntity) &&
                                  ((x.idATraderSpecified && x.idATrader == idATrader) || (false == x.idATraderSpecified))
                                 )
                      )
                     .OrderByDescending(x => GetOrderByDescendingUsing_IdI_IdM_IDB_IDTRADER(x));


                if (partyTemplateFind.Count() == 1)
                {
                    ret = partyTemplateFind.First();
                }
                else if (partyTemplateFind.Count() > 1)
                {
                    /* Recherche d'un enregistrement de même priorité => dans ce cas Spheres® ne remonte pas d'enregistrement 
                     Exemple il existe 2 enregistrements tels que IDI = instrument courant, <TousMarché>, IDA_ENTITY = entité courante. Spheres® ne choisit pas plus un enregistrement que l'autre
                    */
                    ret = partyTemplateFind.First();
                    if (partyTemplates.partyTemplate
                        .Where(x => x.idI == ret.idI && x.idM == ret.idM && x.idAEntity == ret.idAEntity && x.idB == ret.idB && x.idATrader == ret.idATrader
                        && x.idPartyTemplate != ret.idPartyTemplate).Count() > 0)
                    {
                        ret = null;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Permet de trier PARTYTEMPLATE du plus prioritaire au moins prioritaire (prise en compte des colonnes IDI et IDM)
        /// <para>Ordre de priorité 1/Instrument,Marché; 2/Instrument,"Tous marchés", 3/"Tous instruments",Marché, 4/"Tous instruments", "Tous marchés"</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="NumberStart"></param>
        /// FI 20230927 [XXXXX][WI714] New
        private static int GetOrderByDescendingUsing_IdI_IdM(PartyTemplate x, int NumberStart = 99)
        {
            if (x.idISpecified && x.idMSpecified)
            {
                return NumberStart;
            }
            else if (x.idISpecified)
            {
                return NumberStart - 1;
            }
            else if (x.idMSpecified)
            {
                return NumberStart - 2;
            }
            else
            {
                return NumberStart - 3;
            }
        }

        /// <summary>
        /// Permet de trier PARTYTEMPLATE du plus prioritaire au moins prioritaire  (prise en compte des colonnes IDB, IDI et IDM)
        /// <para>Les paramétrages book sont prioritaires sur les paramétrages "Tous books"</para>
        /// <para>Ordre de priorité 1/Book,Instrument,Marché; 2/Book,Instrument,"Tous marchés", 3/Book,"Tous instruments",Marché, 4/Book,"Tous instruments","Tous marchés"
        /// 5/"Tous Books",Instrument,Marché; 6/"Tous Books",Instrument,"Tous marchés", 7/"Tous Books","Tous instruments",Marché, 8/"Tous Books","Tous instruments","Tous marchés"</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="NumberStart"></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        private static int GetOrderByDescendingUsing_IdI_IdM_IDB(PartyTemplate x, int NumberStart = 99)
        {

            if (x.idBSpecified)
            {
                return GetOrderByDescendingUsing_IdI_IdM(x, NumberStart);
            }
            else
            {
                return GetOrderByDescendingUsing_IdI_IdM(x, NumberStart - 10);
            }

        }

        /// <summary>
        /// Permet de trier PARTYTEMPLATE du plus prioritaire au moins prioritaire  (prise en compte des colonnes IDA_TRADER, IDB, IDI et IDM)
        /// <para>Les paramétrages Trader sont prioritaires sur les paramétrages "Tous Trader"</para>
        /// <para>Les paramétrages book sont prioritaires sur les paramétrages "Tous book"</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="NumberStart"></param>
        /// <returns></returns>
        /// FI 20230927 [XXXXX][WI714] New
        private static int GetOrderByDescendingUsing_IdI_IdM_IDB_IDTRADER(PartyTemplate x, int NumberStart = 99)
        {
            if (x.idATraderSpecified)
            {
                return GetOrderByDescendingUsing_IdI_IdM_IDB(x, NumberStart);
            }
            else
            {
                return GetOrderByDescendingUsing_IdI_IdM_IDB(x, NumberStart -20);

            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class PartyTemplate
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDPARTYTEMPLATE")]
        public int idPartyTemplate;
        
        [System.Xml.Serialization.XmlElementAttribute("IDA")]
        public int idA;
        
        [System.Xml.Serialization.XmlElementAttribute("IDA_ENTITY")]
        public int idAEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idISpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDI")]
        public int idI;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDM")]
        public int idM;
        //BOOK
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB")]
        public int idB;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOKIDENTIFIER")]
        public string bookIdentifier;
        //
        //TRADER
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idATraderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_TRADER")]
        public int idATrader;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool traderIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TRADERIDENTIFIER")]
        public string traderIdentifier;
        public bool traderDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TRADERDISPLAYNAME")]
        public string traderDisplayName;
        //
        #endregion
        #region constructor
        public PartyTemplate()
        {
        }
        #endregion
    }

    /// <summary>
    ///  Représente une instruction qui permet d'obtenir le clearer, son book et l'executing broker en fonction d'un context ETD allocation
    /// </summary>
    /// FI 20160929 [22507] Modify
    public class ClearingTemplate
    {
        #region Members
        /// <summary>
        /// Identifiant non significatif de l'instruction
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDCLEARINGTEMPLATE")]
        public int idClearingTemplate;
        //		
        /// <summary>
        /// Représente l'entité pour laquelle l'instruction s'applique
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDA_ENTITY")]
        public int idAEntity;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typePartySpecified;
        /// <summary>
        /// Représente le type de party (Actor,All,Book,GrpActor,GrpBook, None) 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("TYPEPARTY")]
        public TypePartyEnum typeParty;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPartySpecified;
        /// <summary>
        /// Représente l'identifiant non significatif rattaché au type de party
        /// </summary>
        /// FI 20160929 [22507] Add Attribut de Serialization
        [System.Xml.Serialization.XmlElementAttribute("IDPARTY")]
        public int idParty;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idRolePartySpecified;
        /// <summary>
        /// Représente le rôle de la partie 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR_PARTY")]
        public RoleActor idRoleParty;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeInstrSpecified;
        /// <summary>
        /// Représente le type d'instrument (Product,GrpInstr,Instr,None)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("TYPEINSTR")]
        public TypeInstrEnum typeInstr;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idInstrSpecified;
        /// <summary>
        /// Représente l'identifiant non significatif rattaché au type d'instrument
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDINSTR")]
        public int idInstr;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeContratSpecified;
        /// <summary>
        /// Représente le type de contrat (GrpMarket, Market,GrpContract,Contract,None)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("TYPECONTRACT")]
        public TypeContractEnum typeContrat;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idContratSpecified;
        /// <summary>
        /// Représente l'identifiant non significatif rattaché au type de contrat
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDCONTRACT")]
        public int idContrat;
        //
        //CLEARER
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAClearerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_CLEARER")]
        public int idAClearer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clearerIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CLEARERIDENTIFIER")]
        public string clearerIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clearerDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CLEARERDISPLAYNAME")]
        public string clearerDisplayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // PM 20150306 [POC] Renommage de IdBClearerSpecified en idBClearerSpecified
        //public bool IdBClearerSpecified;
        public bool idBClearerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_CLEARER")]
        public int idBClearer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookClearerIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOKCLEARERIDENTIFIER")]
        public string bookClearerIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookClearerDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOKCLEARERDISPLAYNAME")]
        public string bookClearerDisplayName;
        //
        //BROKER
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idABrokerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_BROKER")]
        public int idABroker;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool brokerIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BROKERIDENTIFIER")]
        public string brokerIdentifier;
        public bool brokerDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BROKERDISPLAYNAME")]
        public string brokerDisplayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IdBBrokerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_BROKER")]
        public int idBBroker;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookBrokerIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOKBROKERIDENTIFIER")]
        public string bookBrokerIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookBrokerDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOKBROKERDISPLAYNAME")]
        public string bookBrokerDisplayName;
        //
        #endregion
        #region constructor
        public ClearingTemplate()
        {
        }
        #endregion
    }

    /// <summary>
    /// collection of BrokerTemplates
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("CLEARINGTEMPLATES", Namespace = "", IsNullable = true)]
    public class ClearingTemplates : IComparer
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("CLEARINGTEMPLATE", Order = 1)]
        public ClearingTemplate[] clearingTemplate;
        #endregion Members

        #region Constructors
        public ClearingTemplates() { }
        #endregion

        #region Methods
        /// <summary>
        /// Alimente le membre clearingTemplate
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdA"></param>
        /// <param name="pScanDataDtEnabled"></param>
        /// PL 20140723 Change ExchangeTradedContainer to RptSideContainer
        /// FI 20140815 [XXXXX] Changement signature (suppression de pFixInstrument, utilisation de RptSideContainer exclusivement)
        public void Load(string pCs, DataDocumentContainer pDataDoc, RptSideProductContainer pRptSide, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            //ClearingTemplates clearingTemplates = LoadClearingTemplate(pCs, pDataDoc, pExchangeTraded, pFixInstrument, pScanDataDtEnabled);
            ClearingTemplates clearingTemplates = LoadClearingTemplate(pCs, pDataDoc, pRptSide, pScanDataDtEnabled);
            if (null != clearingTemplates)
            {
                clearingTemplate = clearingTemplates.clearingTemplate;
            }

            if (ArrFunc.IsFilled(clearingTemplate))
            {
                Array.Sort(clearingTemplate, this);
            }
        }

        /// <summary>
        /// Alimente les enregistrements de CLEARINGTEMPLATE qui matchent avec le document
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc">Représente la dataDocument</param>
        /// <param name="pExhangeTradedDerivative">Représente le produit</param>
        /// <param name="pScanDataDtEnabled"></param>
        /// <returns></returns>
        /// FI 20130121 [19480] Les pre-propositions sont désormais effectuées dès que le marché est connu
        /// PL 20140723 [XXXXX] Changement signature ExchangeTradedContainer to RptSideContainer
        /// FI 20140815 [XXXXX] Changement signature (suppression de pFixInstrument, utilisation de RptSideContainer exclusivement)
        /// FI 20140917 [20354] Mise en place d'un order afin de prendre en considération l'enregistrement le plus adéquat
        /// EG 20150706 [21021] Nullable<int> idDealer|idBDealer
        /// FI 20170908 [23409] Modify
        /// FI 20170913 [23417] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private static ClearingTemplates LoadClearingTemplate(string pCS, DataDocumentContainer pDataDoc,
            RptSideProductContainer pRptSide, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            ClearingTemplates ret = null;

            //Récupération du Dealer et Book  
            Nullable<int> idBDealer = null;
            Nullable<int> idDealer = null;
            int idAEntity = 0;

            IFixParty fixParty = pRptSide.GetDealer();
            if ((null != fixParty) && (null != fixParty.PartyId))
            {
                idDealer = pDataDoc.GetOTCmlId_Party(fixParty.PartyId.href);
                idBDealer = pDataDoc.GetOTCmlId_Book(fixParty.PartyId.href);
            }
            if (idBDealer.HasValue)
                idAEntity = BookTools.GetEntityBook(pCS, idBDealer);

            int idM = 0;
            pRptSide.GetMarket(pCS, null, out SQL_Market sqlMarket);
            if (sqlMarket != null)
                idM = sqlMarket.Id;

            if (idAEntity > 0)
            {
                RoleActor roleActor = RoleActor.ENTITY;
                if (idDealer.HasValue)
                {
                    ActorRoleCollection actorRoleCol = pDataDoc.GetActorRole(pCS);
                    if (actorRoleCol.IsActorRole(idDealer.Value, RoleActor.CLIENT))
                    {
                        roleActor = RoleActor.CLIENT;
                    }
                    else if (actorRoleCol.IsActorRole(idDealer.Value, RoleActor.ENTITY) && (idM > 0))
                    {
                        CaptureTools.IsActorClearingMember(pCS, idDealer.Value, sqlMarket.Id, (pScanDataDtEnabled == SQL_Table.ScanDataDtEnabledEnum.Yes), out bool isMarketMaker);
                        if (isMarketMaker)
                            roleActor = RoleActor.MARKETMAKER;
                    }
                }

                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), roleActor);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), idAEntity);

                //jointure sur Instrument
                int idI = pRptSide.ProductBase.ProductType.OTCmlId;
                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCS, null, idI, false, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                string whereInstr = sqlInstrCriteria.GetSQLRestriction2("ct", RoleGInstr.TRADING);

                // FI 20170913 [23417] Utilisation de SQLContractCriteria à la place de SQLDerivativeContractCriteria => Gestion des commoditySpot
                /*
                // FI 20140805 [XXXXX] 
                SQL_DerivativeContract sqlDerivativeContract = null;
                pRptSide.GetDerivativeContract(pCS, null, out sqlDerivativeContract);

                int idDC = 0;
                if (null != sqlDerivativeContract)
                    idDC = sqlDerivativeContract.Id;
                SQLDerivativeContractCriteria sqlDerivativeContractCriteria = new SQLDerivativeContractCriteria(pCS, idDC, idM, pScanDataDtEnabled);
                string whereDC = sqlDerivativeContractCriteria.GetSQLRestriction(pCS, "ct", RoleContractRestrict.TRADING);
                */

                Pair<Cst.ContractCategory, int> contractId = null;
                pRptSide.GetContract(pCS, null, SQL_Table.ScanDataDtEnabledEnum.No, DateTime.MinValue, out Pair<Cst.ContractCategory, SQL_TableWithID> contract);
                if (null != contract)
                    contractId = new Pair<Cst.ContractCategory, int>(contract.First, contract.Second.Id);
                SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCS, null, contractId, idM, pScanDataDtEnabled);
                string whereContract = sqlContractCriteria.GetSQLRestriction("ct", RoleContractRestrict.TRADING);

                //jointure sur {Actor,Book}
                SQLActorBookCriteria sqlActorBookCriteria = new SQLActorBookCriteria(pCS, idDealer.Value, idBDealer.Value, pScanDataDtEnabled);
                string whereParty = sqlActorBookCriteria.GetSQLRestrictionAndSignature(pCS, null, "ct", RoleActorBookRestrict.TRADING);

                //Where
                SQLWhere sqlWhere = new SQLWhere();
                sqlWhere.Append("(ct.IDA_ENTITY=@IDA)");
                sqlWhere.Append("(ct.IDROLEACTOR_PARTY=@IDROLEACTOR" + SQLCst.OR + "ct.IDROLEACTOR_PARTY is null)");
                if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                    sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "ct"));

                sqlWhere.Append(whereParty, true);
                sqlWhere.Append(whereInstr, true);
                if (StrFunc.IsFilled(whereContract))
                    sqlWhere.Append(whereContract, true);

                //Order
                StrBuilder sqlorder = new StrBuilder(SQLCst.ORDERBY);
                sqlorder += "case ct.TYPEPARTY when 'Book' then 4 when 'GrpBook' then 3 when 'Actor' then 2 when 'GrpActor' then 1 else 0 end desc, " + Cst.CrLf;
                sqlorder += "case when ct.IDPARTY is not null then 1 else 0 end desc," + Cst.CrLf;
                sqlorder += "case when ct.IDROLEACTOR_PARTY is not null then 1 else 0 end desc," + Cst.CrLf;
                // FI 20170908 [23409] Gestion des valeurs DerivativeContract et CommodityContract
                sqlorder += @"case ct.TYPECONTRACT 
                                    when 'DerivativeContract' then 4 
                                    when 'CommodityContract' then 4 
                                    when 'GrpContract' then 3 
                                    when 'Market' then 2 
                                    when 'GrpMarket' then 1 else 0 end desc, " + Cst.CrLf;
                sqlorder += "case ct.TYPEINSTR when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc" + Cst.CrLf;

                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "ct.IDCLEARINGTEMPLATE,ct.IDA_ENTITY," + Cst.CrLf;
                sqlSelect += "ct.TYPEPARTY,ct.IDPARTY," + DataHelper.SQLRTrim(pCS, "ct.IDROLEACTOR_PARTY") + "," + Cst.CrLf;
                sqlSelect += "ct.TYPEINSTR,ct.IDINSTR," + Cst.CrLf;
                sqlSelect += "ct.TYPECONTRACT,ct.IDCONTRACT," + Cst.CrLf;

                sqlSelect += "ct.IDA_CLEARER as IDA_CLEARER,c.IDENTIFIER as CLEARERIDENTIFIER, c.DISPLAYNAME as CLEARERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDB_CLEARER as IDB_CLEARER,bc.IDENTIFIER as BOOKCLEARERIDENTIFIER, bc.DISPLAYNAME as BOOKCLEARERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDA_BROKER as IDA_BROKER,br.IDENTIFIER as BROKERIDENTIFIER, br.DISPLAYNAME as BROKERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDB_BROKER as IDB_BROKER,bb.IDENTIFIER as BOOKBROKERIDENTIFIER, bb.DISPLAYNAME as BOOKBROKERDISPLAYNAME" + Cst.CrLf;

                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CLEARINGTEMPLATE.ToString() + " ct" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " c on c.IDA = ct.IDA_CLEARER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " bc on bc.IDB = ct.IDB_CLEARER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " br on br.IDA = ct.IDA_BROKER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " bb on bb.IDB = ct.IDB_BROKER" + Cst.CrLf;

                sqlSelect += sqlWhere;
                sqlSelect += sqlorder;


                QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), dp);

                DataSet dsResult = DataHelper.ExecuteDataset(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                dsResult.DataSetName = "CLEARINGTEMPLATES";

                DataTable dtTable = dsResult.Tables[0];
                dtTable.TableName = "CLEARINGTEMPLATE";

                string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ClearingTemplates), dsSerializerResult);
                ret = (ClearingTemplates)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }


        /// <summary>
        ///  Lecture de CLEARINGTEMPLATE à partir du clearer (IDA,IDB) afin de rechercher le BookDealer
        /// </summary>
        /// FI 20160929 [22507] Add 
        /// FI 20170908 [23409] Modify
        /// FI 20170913 [23417] Modify
        public void LoadModeReverse(string pCS, DataDocumentContainer pDataDoc, RptSideProductContainer pRptSide, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            IFixParty fixParty = pRptSide.GetClearerCustodian();

            Nullable<int> idBClearer = null;
            Nullable<int> idClearer = null;
            if ((null != fixParty) && (null != fixParty.PartyId))
            {
                idClearer = pDataDoc.GetOTCmlId_Party(fixParty.PartyId.href);
                idBClearer = pDataDoc.GetOTCmlId_Book(fixParty.PartyId.href);
            }

            int idAEntity = 0;
            if (idBClearer.HasValue)
                idAEntity = BookTools.GetEntityBook(pCS, idBClearer);

            if (idAEntity > 0)
            {
                int idM = 0;
                pRptSide.GetMarket(pCS, null, out SQL_Market sqlMarket);
                if (sqlMarket != null)
                    idM = sqlMarket.Id;

                //Restriction sur Instrument
                int idI = pRptSide.ProductBase.ProductType.OTCmlId;
                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCS, null, idI, false, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                string whereInstr = sqlInstrCriteria.GetSQLRestriction2("ct", RoleGInstr.TRADING);

                // FI 20170913 [23417] Utilisation de SQLContractCriteria à la place de SQLDerivativeContractCriteria => Gestion des commoditySpot
                /*
                //Restriction sur DC
                SQL_DerivativeContract sqlDerivativeContract = null;
                pRptSide.GetDerivativeContract(pCS, null, out sqlDerivativeContract);
                int idDC = 0;
                if (null != sqlDerivativeContract)
                    idDC = sqlDerivativeContract.Id;
                SQLDerivativeContractCriteria sqlDerivativeContractCriteria = new SQLDerivativeContractCriteria(pCS, idDC, idM, pScanDataDtEnabled);
                string whereDC = sqlDerivativeContractCriteria.GetSQLRestriction(pCS, "ct", RoleContractRestrict.TRADING);
                */

                Pair<Cst.ContractCategory, int> contractId = null;
                pRptSide.GetContract(pCS, null, SQL_Table.ScanDataDtEnabledEnum.No, DateTime.MinValue, out Pair<Cst.ContractCategory, SQL_TableWithID> contract);
                if (null != contract)
                    contractId = new Pair<Cst.ContractCategory, int>(contract.First, contract.Second.Id);
                SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCS, null, contractId, idM, pScanDataDtEnabled);
                string whereContract = sqlContractCriteria.GetSQLRestriction("ct", RoleContractRestrict.TRADING);


                //Restriction sur Clearer
                string whereParty = "(ct.IDA_CLEARER = @IDACLEARER and ct.IDB_CLEARER = @IDBCLEARER)";

                //Where
                SQLWhere sqlWhere = new SQLWhere();
                sqlWhere.Append("(ct.IDA_ENTITY=@IDA)");
                sqlWhere.Append("(ct.TYPEPARTY = 'Book')");
                if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                    sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "ct"));

                sqlWhere.Append(whereParty, true);
                sqlWhere.Append(whereInstr, true);
                if (StrFunc.IsFilled(whereContract))
                    sqlWhere.Append(whereContract, true);

                //Order
                StrBuilder sqlorder = new StrBuilder(SQLCst.ORDERBY);
                // FI 20170908 [23409] gestion des valeurs DerivativeContract et CommodityContract
                sqlorder += @"case ct.TYPECONTRACT 
                                    when 'DerivativeContract' then 4 
                                    when 'CommodityContract' then 4 
                                    when 'GrpContract' then 3 
                                    when 'Market' then 2 
                                    when 'GrpMarket' then 1 else 0 end desc, " + Cst.CrLf;
                sqlorder += "case ct.TYPEINSTR when 'Instr' then 2 when 'GrpInstr' then 1 else 0 end desc" + Cst.CrLf;

                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "ct.IDCLEARINGTEMPLATE,ct.IDA_ENTITY," + Cst.CrLf;
                sqlSelect += "ct.TYPEPARTY,ct.IDPARTY," + DataHelper.SQLRTrim(pCS, "ct.IDROLEACTOR_PARTY") + "," + Cst.CrLf;
                sqlSelect += "ct.TYPEINSTR,ct.IDINSTR," + Cst.CrLf;
                sqlSelect += "ct.TYPECONTRACT,ct.IDCONTRACT," + Cst.CrLf;

                sqlSelect += "ct.IDA_CLEARER as IDA_CLEARER,c.IDENTIFIER as CLEARERIDENTIFIER, c.DISPLAYNAME as CLEARERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDB_CLEARER as IDB_CLEARER,bc.IDENTIFIER as BOOKCLEARERIDENTIFIER, bc.DISPLAYNAME as BOOKCLEARERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDA_BROKER as IDA_BROKER,br.IDENTIFIER as BROKERIDENTIFIER, br.DISPLAYNAME as BROKERDISPLAYNAME," + Cst.CrLf;
                sqlSelect += "ct.IDB_BROKER as IDB_BROKER,bb.IDENTIFIER as BOOKBROKERIDENTIFIER, bb.DISPLAYNAME as BOOKBROKERDISPLAYNAME" + Cst.CrLf;

                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CLEARINGTEMPLATE.ToString() + " ct" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " c on c.IDA = ct.IDA_CLEARER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " bc on bc.IDB = ct.IDB_CLEARER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " br on br.IDA = ct.IDA_BROKER" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " bb on bb.IDB = ct.IDB_BROKER" + Cst.CrLf;

                sqlSelect += sqlWhere;
                sqlSelect += sqlorder;

                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), idAEntity);
                dp.Add(new DataParameter(pCS, "IDBCLEARER", DbType.Int32), idBClearer);
                dp.Add(new DataParameter(pCS, "IDACLEARER", DbType.Int32), idClearer);

                QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), dp);

                DataSet dsResult = DataHelper.ExecuteDataset(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                dsResult.DataSetName = "CLEARINGTEMPLATES";

                DataTable dtTable = dsResult.Tables[0];
                dtTable.TableName = "CLEARINGTEMPLATE";

                string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ClearingTemplates), dsSerializerResult);
                ClearingTemplates clearingTemplates = (ClearingTemplates)CacheSerializer.Deserialize(serializeInfo);
                clearingTemplate = clearingTemplates.clearingTemplate;
            }
        }



        #endregion

        #region IComparer Members
        /// <summary>
        /// <para>Retourne -1 si l'instruction x de type clearingTemplate est prioritaire</para>
        /// <para>Retourne 1 si l'instruction y de type clearingTemplate est prioritaire</para>
        /// Une instruction est prioritaire si son paramétrage est plus fin
        /// <para>Une instruction sur "Partie" est prioritaire par rapport à une instrution sur "Marché/Contrat"</para>
        /// <para>Une instruction sur "Marché/Contrat" est prioritaire par rapport à une instrution sur "Environnement instrumental"</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// FI 20170908 [23409] Modify
        public int Compare(object x, object y)
        {
            if ((!(x is ClearingTemplate xn)) || (!(y is ClearingTemplate yn)))
                throw new ArgumentException("object is not a ClearingTemplate");

            int ret = 0;

            if (ret == 0)
            {
                if (xn.idPartySpecified && (false == yn.idPartySpecified))
                    ret = -1;
                else if (yn.idPartySpecified && (false == xn.idPartySpecified))
                    ret = 1;
            }

            if ((ret == 0) && (xn.typePartySpecified) && (yn.typePartySpecified))
            {
                if ((xn.typeParty == TypePartyEnum.Book) && (false == (yn.typeParty == TypePartyEnum.Book)))
                    ret = -1;
                else if ((yn.typeParty == TypePartyEnum.Book) && (false == (xn.typeParty == TypePartyEnum.Book)))
                    ret = 1;
                //
                if (ret == 0)
                {
                    if ((xn.typeParty == TypePartyEnum.GrpBook) && (false == (yn.typeParty == TypePartyEnum.GrpBook)))
                        ret = -1;
                    else if ((yn.typeParty == TypePartyEnum.GrpBook) && (false == (xn.typeParty == TypePartyEnum.GrpBook)))
                        ret = 1;
                }
                //
                if (ret == 0)
                {
                    if ((xn.typeParty == TypePartyEnum.Actor) && (false == (yn.typeParty == TypePartyEnum.Actor)))
                        ret = -1;
                    else if ((yn.typeParty == TypePartyEnum.Actor) && (false == (xn.typeParty == TypePartyEnum.Actor)))
                        ret = 1;
                }
                //
                if (ret == 0)
                {
                    if ((xn.typeParty == TypePartyEnum.GrpActor) && (false == (yn.typeParty == TypePartyEnum.GrpActor)))
                        ret = -1;
                    else if ((yn.typeParty == TypePartyEnum.GrpActor) && (false == (xn.typeParty == TypePartyEnum.GrpActor)))
                        ret = 1;
                }
            }
            //
            if (ret == 0)
            {
                if (xn.idContratSpecified && (false == yn.idContratSpecified))
                    ret = -1;
                else if (yn.idContratSpecified && (false == xn.idContratSpecified))
                    ret = 1;
                //
                if ((ret == 0) && (xn.typeContratSpecified) && (yn.typeContratSpecified))
                {
                    // FI 20170908 [23409] Utilisation de TypeContractEnum.DerivativeContract
                    if ((xn.typeContrat == TypeContractEnum.DerivativeContract) && (false == (yn.typeContrat == TypeContractEnum.DerivativeContract)))
                        ret = -1;
                    else if ((yn.typeContrat == TypeContractEnum.DerivativeContract) && (false == (xn.typeContrat == TypeContractEnum.DerivativeContract)))
                        ret = 1;
                    //
                    if (ret == 0)
                    {
                        if ((xn.typeContrat == TypeContractEnum.GrpContract) && (false == (yn.typeContrat == TypeContractEnum.GrpContract)))
                            ret = -1;
                        else if ((yn.typeContrat == TypeContractEnum.GrpContract) && (false == (xn.typeContrat == TypeContractEnum.GrpContract)))
                            ret = 1;
                    }
                    //
                    if (ret == 0)
                    {
                        if ((xn.typeContrat == TypeContractEnum.Market) && (false == (yn.typeContrat == TypeContractEnum.Market)))
                            ret = -1;
                        else if ((yn.typeContrat == TypeContractEnum.Market) && (false == (xn.typeContrat == TypeContractEnum.Market)))
                            ret = 1;
                    }
                    //
                    if (ret == 0)
                    {
                        if ((xn.typeContrat == TypeContractEnum.GrpMarket) && (false == (yn.typeContrat == TypeContractEnum.GrpMarket)))
                            ret = -1;
                        else if ((yn.typeContrat == TypeContractEnum.GrpMarket) && (false == (xn.typeContrat == TypeContractEnum.GrpMarket)))
                            ret = 1;
                    }
                }
            }
            //
            if (ret == 0)
            {
                if (xn.idInstrSpecified && (false == yn.idInstrSpecified))
                    ret = -1;
                else if (yn.idInstrSpecified && (false == xn.idInstrSpecified))
                    ret = 1;
                //
                if ((ret == 0) && (xn.idInstrSpecified) && (yn.idInstrSpecified))
                {
                    if (ret == 0)
                    {
                        if ((xn.typeInstr == TypeInstrEnum.Instr) && (false == (yn.typeInstr == TypeInstrEnum.Instr)))
                            ret = -1;
                        else if ((yn.typeInstr == TypeInstrEnum.Instr) && (false == (xn.typeInstr == TypeInstrEnum.Instr)))
                            ret = 1;
                    }
                    //
                    if (ret == 0)
                    {
                        if ((xn.typeInstr == TypeInstrEnum.GrpInstr) && (false == (yn.typeInstr == TypeInstrEnum.GrpInstr)))
                            ret = -1;
                        else if ((yn.typeInstr == TypeInstrEnum.GrpInstr) && (false == (xn.typeInstr == TypeInstrEnum.GrpInstr)))
                            ret = 1;
                    }
                    //
                    if (ret == 0)
                    {
                        if ((xn.typeInstr == TypeInstrEnum.Product) && (false == (yn.typeInstr == TypeInstrEnum.Product)))
                            ret = -1;
                        else if ((yn.typeInstr == TypeInstrEnum.Product) && (false == (xn.typeInstr == TypeInstrEnum.Product)))
                            ret = 1;
                    }
                }
            }
            return ret;

        }
        #endregion IComparer Members
    }

}