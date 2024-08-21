#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.GUI.Interface;
using EFS.Common.Log;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
#endregion Using Directives

namespace EfsML.Notification
{

    /// <summary>
    /// Représente les directives pour le chargement des messages de confirmation/notification
    /// </summary>
    /// FI 20170913 [23417] Modify
    public class LoadMessageSettings
    {
        #region members

        /// <summary>
        /// Représente la class de message recherché (MULTI-TRADES, MULTI-PARTIES, MONO-TRADE) 
        /// </summary>
        public NotificationClassEnum[] cnfClass;

        /// <summary>
        /// Représente le type de message recherché (MULTI-TRADES, MULTI-PARTIES, MONO-TRADE) 
        /// </summary>
        public NotificationTypeEnum[] cnfType;

        /// <summary>
        /// Représente un instrument 
        /// </summary>
        public int idI;


        // FI 20170913 [23417] idDerivativeContract de contract
        /*
        /// <summary>
        /// Représente un Derivative contract
        /// </summary>
        public int idDerivativeContract;
         */
        /// <summary>
        /// Représente le contract (derivative ou commodity)
        /// </summary>
        public Pair<Cst.ContractCategory, int> contract;


        /// <summary>
        ///  Marché
        ///  <para>0 si trade de type cash Balance</para>
        /// </summary>
        /// FI 20140813 [20275] add 
        public int idM;

        /// <summary>
        /// Représente un statut 
        /// </summary>
        public Cst.StatusBusiness idStBusiness;

        //20140710 PL [TRIM 20179]
        /// <summary>
        /// Représente un statut 
        /// </summary>
        public string idStMatch;
        /// <summary>
        /// Représente un statut 
        /// </summary>
        public string idStCheck;

        /// <summary>
        /// Représente les indicateurs pour lesquels la confirmation est activée 
        /// </summary>
        public NotificationStepLifeEnum[] stepLife;

        /// <summary>
        /// Représente une liste restrictive de message
        /// </summary>
        public int[] idCnfMessage;
        #endregion
        //
        #region constructor
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCnfClass"></param>
        /// <param name="pCnfType"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdM"></param>
        /// <param name="pContract"></param>
        /// <param name="pIdStBusiness"></param>
        /// <param name="pIdStMatch"></param>
        /// <param name="pIdStCheck"></param>
        /// <param name="pStepLife"></param>
        /// <param name="pIdCnfMessage"></param>
        /// PL 20140710 [20179] 
        /// FI 20140813 [20179] add parameter pIdM
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public LoadMessageSettings(NotificationClassEnum[] pCnfClass, NotificationTypeEnum[] pCnfType,
                                    int pIdI, int pIdM, Pair<Cst.ContractCategory,int> pContract, Cst.StatusBusiness pIdStBusiness,
                                    string pIdStMatch, string pIdStCheck,
                                    NotificationStepLifeEnum[] pStepLife, int[] pIdCnfMessage)
        {
            cnfClass = pCnfClass;
            cnfType = pCnfType;
            idI = pIdI;
            idM = pIdM;
            // FI 20170913 [23417] 
            //idDerivativeContract = pIdDerivativeContract;
            contract = pContract; 
            idStBusiness = pIdStBusiness;
            idStMatch = pIdStMatch;
            idStCheck = pIdStCheck;
            stepLife = pStepLife;
            idCnfMessage = pIdCnfMessage;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContactOffices : OfficesBase
    {
        #region Accessor
        /// <summary>
        /// Colonne ds Book qui définit le Contact Office
        /// </summary>
        public override string BookColumn
        {
            get
            {
                return "IDA_CONTACTOFFICE";
            }
        }
        
        /// <summary>
         /// Obtient <see cref="RoleActor.CONTACTOFFICE"/>
        /// </summary>
        /// FI 20240218 [WI838] add
        public override RoleActor RoleActor
        {
            get
            {
                return RoleActor.CONTACTOFFICE;
            }
        }

        /// <summary>
        /// Obtient les types de Rôle qui doivent être exclus (à savoir  <see cref="RoleType.ACCESS"/>, <see cref="RoleType.COLLABORATION"/>, <see cref="RoleType.COLLABORATION_ALGORITHM"/>).
        /// </summary>
        /// FI 20240218 [WI838] Add
        public override RoleType[] RoleTypeExclude
        {

            get { return new RoleType[] { RoleType.ACCESS, RoleType.COLLABORATION, RoleType.COLLABORATION_ALGORITHM }; }
        }
        #endregion Accessor

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// FI 20240218 [WI838] seull ce constructeur est conservé
        public ContactOffices(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
            : base(pSource, pDbTransaction, pIdA, pIdB)
        {
        }
        #endregion Constructors
    }

    /// <summary>
    /// Classe qui gère les tables MCO et MCODET
    /// <para>Dans MCO, MCO.DTMCO représente la date d'émission du message (tient compte de l'application du décalage défini sur le message)</para>
    /// <para>Dans MCO, MCO.DTMCOFORCED représente la date de génération du MCO (équivalent à DTINS)</para>
    /// </summary>
    /// FI 20120829 [18048] gestion de la colonne MCO.DTMCO2
    public class DatasetConfirmationMessage
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private DataSet _ds;
        /// <summary>
        /// 
        /// </summary>
        private readonly bool _isModeSimulation;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la table MCO
        /// </summary>
        public DataTable DtMCO
        {
            get { return _ds.Tables["MCO"]; }
        }
        /// <summary>
        /// Obtient la table MCODET
        /// </summary>
        public DataTable DtMCODET
        {
            get { return _ds.Tables["MCODET"]; }
        }
        /// <summary>
        /// Obtient la relation entre MCO et MCODET
        /// </summary>
        public DataRelation ChildMCO_MCODET
        {
            get { return _ds.Relations["MCO_MCODET"]; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsModeSimulation"></param>
        public DatasetConfirmationMessage(bool pIsModeSimulation)
        {
            _isModeSimulation = pIsModeSimulation;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Alimente l'enregistrement MCO avec un message 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pConfBuilder"></param>
        /// <param name="pIdA"></param>
        /// FI 20160624 [22286] Modify
        /// FI 20171120 [23580] Modify
        /// AL 20240328 [WI440] New parameter logLevel
        public void UpdRowMCO(string pCS, int pIdMCO, NotificationBuilder pConfBuilder, int pIdA, LogLevelDetail logLevel)
        {

            //Suppresion de XML Declaration => Stockage base de donnée, seul UTF-16 est accepté avec ADO, autant tout supprimer
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(pConfBuilder.SerializeDocWithoutXmlns.ToString());
            XMLTools.RemoveXmlDeclaration(xmlDoc);
            //
            XmlDocument xslDoc = new XmlDocument();
            xslDoc.Load(pConfBuilder.XslFile);
            XMLTools.RemoveXmlDeclaration(xslDoc);
            //
            //Alimenation de la colonne
            DataRow[] rows = DtMCO.Select("ID=" + pIdMCO);
            //
            if (ArrFunc.IsFilled(rows))
            {
                DataRow row = rows[0];
                //
                row.BeginEdit();
                //AL 20240328 [WI440] Columns CNFMSGXML, CNFMSGXSL, LOCNFMSGTXT and DOCTYPEMSGTXT are filled only with log level 4 or higher 
                // LOCNFMSGTXT and DOCTYPEMSGTXT are filled only if the final report is not a PDF.

                bool isPdfReport = null != pConfBuilder.BinaryResult;

                row["CNFMSGXML"] = xmlDoc.InnerXml;
                row["CNFMSGXSL"] = logLevel >= LogLevelDetail.LEVEL4 ? xslDoc.InnerXml : Convert.DBNull;
                row["LOCNFMSGTXT"] = logLevel >= LogLevelDetail.LEVEL4 || !isPdfReport ? pConfBuilder.Result : Convert.DBNull;
                row["DOCTYPEMSGTXT"] = logLevel >= LogLevelDetail.LEVEL4 || !isPdfReport ? pConfBuilder.OutputXslDocTypeMIME.ToString() : Convert.DBNull;

                if (isPdfReport)
                {
                    row["LOCNFMSGBIN"] = pConfBuilder.BinaryResult;
                    row["DOCTYPEMSGBIN"] = pConfBuilder.OutputBinDocTypeMIME.ToString();
                }
                //
                if (Convert.IsDBNull(row["DTINS"]))
                {
                    // FI 20200820 [25468] Dates systemes en UTC
                    row["DTINS"] = OTCmlHelper.GetDateSysUTC(pCS);
                    row["IDAINS"] = pIdA;
                }
                else
                {
                    // FI 20200820 [25468] Dates systemes en UTC
                    row["DTUPD"] = OTCmlHelper.GetDateSysUTC(pCS);
                    row["IDAUPD"] = pIdA;
                }
                //
                row["CULTURE"] = pConfBuilder.NotificationDocument.Culture;
                row["EXTLLINK"] = Convert.DBNull;

                // FI 20171120 [23580] DTCREATE est alimentée avec une date selon l'horodatage UTC 
                // FI 20171120 [23580] Alimentation du TZ_SENDBY
                EFS_DateTimeUTC creationTimestamp = pConfBuilder.NotificationDocument.NotificationDocument.Header.CreationTimestamp;
                row["DTCREATE"] = new DtFunc().StringDateTimeISOToDateTime(creationTimestamp.utcValue);
                row["TZ_SENDBY"] = Convert.DBNull;
                if (StrFunc.IsFilled(creationTimestamp.tzdbId))
                    row["TZ_SENDBY"] = creationTimestamp.tzdbId;


                row.EndEdit();
            }
        }

        /// <summary>
        /// Ajoute une edition dans MCO
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pDate">Date du message</param>
        /// <param name="pDate2"></param>
        /// <param name="pCnfMessage">Représente le message</param>
        /// <param name="pCnfMultiPartieValue">Représente le type edition consolidée</param>
        /// <param name="pCnfChain">Représente la chaîne de confirmation</param>
        /// <param name="pIdNcs">Représente le canal de communication</param>
        /// <param name="pIdInciSendBy">Représente l'instruction retenue côté émetteur</param>
        /// <param name="pIdInciSendTo">Représente l'instruction retenue côté destinataire</param>
        /// <param name="pIdAIns"></param>
        /// FI 20120829 [18048] ajout du paramètre pDate2
        public void AddRowMCOReport(string pCS, int pIdMCO, DateTime pDate, Nullable<DateTime> pDate2,
            CnfMessage pCnfMessage, Nullable<NotificationMultiPartiesEnum> pCnfMultiPartieValue,
            ConfirmationChain pCnfChain, int pIdNcs, int pIdInciSendBy, int pIdInciSendTo, int pIdAIns)
        {
            //FI 20120829 [18048] alimentation du paramètre pdate2 
            AddRowMCO(pCS, pIdMCO, null, pDate, pDate2, pCnfMessage, pCnfMultiPartieValue, pCnfChain, pIdNcs, pIdInciSendBy, pIdInciSendTo, pIdAIns);
        }

        /// <summary>
        /// Ajoute une confirmation dans MCO
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pIdT">Id du Trade</param>
        /// <param name="pDate">Date du message</param>
        /// <param name="pCnfMessage">Représente le message</param>
        /// <param name="pCnfChain">Représente la chaîne de confirmation</param>
        /// <param name="pIdNcs">Représente le canal de communication</param>
        /// <param name="pIdInciSendBy">Représente l'instruction retenue côté émetteur</param>
        /// <param name="pIdInciSendTo">Représente l'instruction retenue côté destinataire</param>
        /// <param name="pIdAIns"></param>
        public void AddRowMCOConfirm(string pCS, int pIdMCO, int pIdT, DateTime pDate,
            CnfMessage pCnfMessage,
            ConfirmationChain pCnfChain, int pIdNcs, int pIdInciSendBy, int pIdInciSendTo, int pIdAIns)
        {
            //FI 20120829 [18048] alimentation du paramètre pdate2 avec null
            AddRowMCO(pCS, pIdMCO, pIdT, pDate, null, pCnfMessage, null, pCnfChain, pIdNcs, pIdInciSendBy, pIdInciSendTo, pIdAIns);
        }

        /// <summary>
        /// Ajoute une ligne dans MCO 
        /// <para>Les lignes ajoutées ont la proriété EXTLLINK = NEW</para>
        /// </summary>
        ///<param name="pCS"></param>
        ///<param name="pIdMCO">Id MCO</param>
        /// <param name="pIdT">Id du Trade (null si edition)</param>
        /// <param name="pDate">Date du message</param>
        /// <param name="pDate2"></param>
        /// <param name="pCnfMessage">Représente le message</param>
        /// <param name="pCnfMultiPartieValue">Représente le type de message Multi parties</param>
        /// <param name="pCnfChain">Représente la chaîne de confirmation</param>
        /// <param name="pIdNcs">Représente le canal de communication</param>
        /// <param name="pIdInciSendBy">Représente l'instruction retenue côté émetteur</param>
        /// <param name="pIdInciSendTo">Représente l'instruction retenue côté destinataire</param>
        /// <param name="pIdAIns"></param>
        /// FI 20120829 [18048] Modification de la signature, ajout de pDate2
        /// EG 20150115 [20683]
        private void AddRowMCO(string pCS, int pIdMCO, Nullable<int> pIdT, DateTime pDate, Nullable<DateTime> pDate2,
            CnfMessage pCnfMessage, Nullable<NotificationMultiPartiesEnum> pCnfMultiPartieValue,
            ConfirmationChain pCnfChain, int pIdNcs, int pIdInciSendBy, int pIdInciSendTo, int pIdAIns)
        {
            DataRow row = DtMCO.NewRow();
            //
            row["ID"] = pIdMCO;

            if (pIdT.HasValue && pIdT.Value > 0)
                row["IDT"] = pIdT.Value;

            row["DTMCO"] = pDate;
            row["DTMCOFORCED"] = OTCmlHelper.GetAnticipatedDate(pCS, pDate);

            //FI 20120829 [18048] alimentation de pDate2
            if (pDate2.HasValue)
                row["DTMCO2"] = pDate2;

            row["IDCNFMESSAGE"] = pCnfMessage.idCnfMessage;
            if (pCnfMultiPartieValue.HasValue)
                row["SCOPE"] = pCnfMultiPartieValue.Value;

            row["IDA_SENDBYPARTY"] = pCnfChain[SendEnum.SendBy].IdActor;
            row["IDA_SENDBYOFFICE"] = pCnfChain[SendEnum.SendBy].IdAContactOffice;
            if (pCnfChain[SendEnum.SendBy].IdBook > 0)
                row["IDB_SENDBYPARTY"] = pCnfChain[SendEnum.SendTo].IdBook;

            row["IDA_SENDTOPARTY"] = pCnfChain[SendEnum.SendTo].IdActor;
            row["IDA_SENDTOOFFICE"] = pCnfChain[SendEnum.SendTo].IdAContactOffice;
            if (pCnfChain[SendEnum.SendTo].IdBook > 0)
                row["IDB_SENDTOPARTY"] = pCnfChain[SendEnum.SendTo].IdBook;

            row["IDA_NCS"] = pIdNcs;

            row["IDINCI_SENDBY"] = pIdInciSendBy;
            if (pIdInciSendTo > 0)
                row["IDINCI_SENDTO"] = pIdInciSendTo;
            else
                row["IDINCI_SENDTO"] = Convert.DBNull;
            //
            row["CNFMSGXML"] = Convert.DBNull;
            row["CNFMSGXSL"] = Convert.DBNull;
            //
            row["LOCNFMSGTXT"] = Convert.DBNull;
            row["DOCTYPEMSGTXT"] = Convert.DBNull;
            row["LOCNFMSGBIN"] = Convert.DBNull;
            row["DOCTYPEMSGBIN"] = Convert.DBNull;
            //
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(pCS);
            row["IDAINS"] = pIdAIns;
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;

            /// EG 20150115 [20683]
            row["DTOBSOLETE"] = Convert.DBNull;
            row["EXTLLINK"] = "NEW";
            //
            DtMCO.Rows.Add(row);
        }


        /// <summary>
        /// Ajoute une ligne dans MCODET
        /// <para>Les lignes ajoutées ont la proriété EXTLLINK = NEW</para>
        /// </summary>
        /// <param name="pIdMCO"></param>
        /// <param name="pIdE"></param>
        /// <param name="pIdAIns"></param>
        /// EG 20150115 [20683]
        public void AddRowMCODET(string pCS, int pIdMCO, int pIdE, int pIdAIns)
        {
            DataRow row = DtMCODET.NewRow();
            //
            row["ID"] = pIdMCO;
            row["IDE"] = pIdE;
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(pCS);
            row["IDAINS"] = pIdAIns;
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;
            /// EG 20150115 [20683]
            row["DTOBSOLETE"] = Convert.DBNull;
            row["EXTLLINK"] = "NEW";
            //
            DtMCODET.Rows.Add(row);
        }

        /// <summary>
        /// Chargement du dataset pour un IdT (colonne MCO.IDT)  et une date donnée (colonne MCO.DTMCO)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// EG 20150115 [20683]
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public void LoadDs(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pDate)
        {
            string sqlWhere = @" and (mco.IDT = @IDT)";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            if (DtFunc.IsDateTimeFilled(pDate))
            {
                parameters.Add(new DataParameter(pCS, "DTMCO", DbType.Date), pDate);  // FI 20201006 [XXXXX] dbType.date
                sqlWhere += @" and (mco.DTMCO = @DTMCO)";
            }

            string sqlSelect = GetSelectMcoColumn(true) + Cst.CrLf + sqlWhere + Cst.CrLf + @"order by mco.IDMCO;" + Cst.CrLf;
            sqlSelect += GetSelectMcoDetColumn(true) + Cst.CrLf + sqlWhere + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);

            _ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            InitializeDs();
        }

        /// <summary>
        /// Charge le Dataset pour un IdMCO (colonne MCO.IDMCO)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMCO"></param>
        /// EG 20150115 [20683]
        public void LoadDs(string pCS, IDbTransaction pDbTransaction, int pIdMCO)
        {
            LoadDs(pCS, pDbTransaction, pIdMCO, true);
        }

        /// <summary>
        /// Charge le Dataset pour un IdMCO (colonne MCO.IDMCO)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pWithoutObsolete"></param>
        // RD 20161031 [22511] Add param pWithoutObsolete
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public void LoadDs(string pCS, IDbTransaction pDbTransaction, int pIdMCO, bool pWithoutObsolete)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDMCO", DbType.Int32), pIdMCO);
            string sqlSelect = GetSelectMcoColumn(pWithoutObsolete) + Cst.CrLf + (pWithoutObsolete ? " and " : "where ") + " (mco.IDMCO = @IDMCO);" + Cst.CrLf;
            sqlSelect += GetSelectMcoDetColumn(true) + Cst.CrLf + " and (mco.IDMCO = @IDMCO);" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);

            _ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            InitializeDs();
        }

        /// <summary>
        /// Alimente la colonne IDMCO sur les lignes ajoutés
        /// <para>Spheres® recupère un jeton pour chaque ligne</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsWithMCODET"></param>
        public void SetIdMCO(string pCS, IDbTransaction pDbTransaction, bool pIsWithMCODET)
        {
            DataRow[] drMCO = DtMCO.Select("EXTLLINK = 'NEW'");
            //
            if (null != drMCO)
            {
                int newId;
                //                    
                if (null != pDbTransaction)
                    SQLUP.GetId(out newId, pDbTransaction, SQLUP.IdGetId.MCO, SQLUP.PosRetGetId.First, ArrFunc.Count(drMCO));
                else
                    SQLUP.GetId(out newId, pCS, SQLUP.IdGetId.MCO, SQLUP.PosRetGetId.First, ArrFunc.Count(drMCO));
                //
                for (int i = 0; i < drMCO.Length; i++)
                {
                    if (pIsWithMCODET)
                    {
                        foreach (DataRow drMCODET in drMCO[i].GetChildRows(this.ChildMCO_MCODET))
                        {
                            drMCODET["ID"] = newId;
                            drMCODET["EXTLLINK"] = Convert.DBNull;
                        }
                    }
                    //                        
                    drMCO[i]["ID"] = newId;
                    drMCO[i]["EXTLLINK"] = Convert.DBNull;
                    newId++;
                }
            }
        }



        /// <summary>
        /// Affecte l'IDT de chaque ligne si les évènement présents dans MCODET sont issus du même trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        public void SetIDT(string pCS, string pSessionId)
        {
            DataRow[] dr = DtMCO.Select("EXTLLINK = 'NEW'");
            //
            foreach (DataRow row in dr)
            {
                if (row["IDT"] == DBNull.Value)
                {
                    int[] idE = GetIdEventsInMCODET(Convert.ToInt32(row["ID"]));
                    int[] IdT = TradeRDBMSTools.GetIdTradeFromIdEvent(pCS, pSessionId, idE);
                    if (ArrFunc.Count(IdT) == 1)
                        row["IDT"] = IdT[0];
                }
            }
        }

        /// <summary>
        /// Retourne les évènements présents dans MCODET pour un IdMCO {pIdMCO) 
        /// </summary>
        /// <param name="pIdMCO"></param>
        /// <returns></returns>
        public int[] GetIdEventsInMCODET(int pIdMCO)
        {
            int[] ret = null;
            DataRow[] rowMcoDet = DtMCODET.Select("ID = " + pIdMCO.ToString());
            if (ArrFunc.IsFilled(rowMcoDet))
                ret = Array.ConvertAll(rowMcoDet, new Converter<DataRow, Int32>(RowMCODetToIdE));
            return ret;
        }

        /// <summary>
        /// Retourne les trades inpliqués dans un MCO {pIdMCO) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO"></param>
        /// <returns></returns>
        /// FI 20120831  les IdT sont triés dans l'ordre croissant
        public int[] GetIdTInMCO(string pCS, string pSessionId, int pIdMCO)
        {
            int[] ret = null;
            DataRow[] rowMco = DtMCO.Select("ID = " + pIdMCO.ToString());
            //
            if (ArrFunc.IsFilled(rowMco))
            {
                if (rowMco[0]["IDT"] != Convert.DBNull)
                {
                    ret = new int[] { Convert.ToInt32(rowMco[0]["IDT"]) };
                }
                else
                {
                    int[] idE = GetIdEventsInMCODET(pIdMCO);
                    ret = TradeRDBMSTools.GetIdTradeFromIdEvent(pCS, pSessionId, idE);
                    if (ArrFunc.IsFilled(ret))
                        Array.Sort(ret);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le Row tel ID = {pIdMCO}
        /// </summary>
        /// <param name="pIdMCO"></param>
        /// <returns></returns>
        public DataRow GetRowIdMco(int pIdMCO)
        {
            DataRow ret = null;
            //
            DataRow[] rowMCO = DtMCO.Select("ID = " + pIdMCO.ToString());
            if (ArrFunc.IsFilled(rowMCO))
                ret = rowMCO[0];
            //
            return ret;
        }

        /// <summary>
        /// Mise à jour de la table MCO
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// EG 20150115 [20683]
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void ExecuteDataAdapterMCO(string pCS, IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectMcoColumn(false);
            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlSelect, DtMCO);
        }
        /// <summary>
        /// Mise à jour de la table MCODET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// EG 20150115 [20683]
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void ExecuteDataAdapterMCODET(string pCS, IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectMcoDetColumn(false);
            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlSelect, DtMCODET);
        }

        /// <summary>
        /// Retourne la requête de chargement de MCO
        /// </summary>
        /// <param name="pIsNoDataAdapter"></param>
        /// <returns></returns>
        /// FI 20120829 [18048] gestion de la colonne DTMCO2
        /// EG 20150115 [20683] Add DTOBSOLETE
        /// FI 20160624 [22286] Modify
        /// FI 20171120 [23580] Modify 
        private string GetSelectMcoColumn(bool pIsNoDataAdapter)
        {
            // FI 20160624 [22286] Add DTCREATE
            // FI 20171120 [23580] Add TZ_SENDBY
            string sqlSelect = @"select mco.IDMCO as ID, mco.IDT, mco.DTMCO, mco.DTMCOFORCED, mco.DTMCO2, mco.IDCNFMESSAGE,mco.IDA_NCS,
            mco.IDA_SENDBYPARTY,mco.IDA_SENDBYOFFICE,mco.IDB_SENDBYPARTY, mco.IDA_SENDTOPARTY, mco.IDA_SENDTOOFFICE, mco.IDB_SENDTOPARTY,
            mco.IDINCI_SENDBY, mco.IDINCI_SENDTO, mco.CNFMSGXML, mco.CNFMSGXSL, mco.LOCNFMSGTXT, mco.DOCTYPEMSGTXT, mco.LOCNFMSGBIN, mco.DOCTYPEMSGBIN,
            mco.DTUPD, mco.IDAUPD, mco.DTINS, mco.IDAINS, mco.EXTLLINK, mco.ROWATTRIBUT, mco.SCOPE, mco.CULTURE, mco.DTOBSOLETE, mco.DTCREATE, mco.TZ_SENDBY
            from dbo.MCO{0} mco" + Cst.CrLf;
            if (pIsNoDataAdapter)
            {
                sqlSelect += @"where (mco.DTOBSOLETE is null)" + Cst.CrLf;
            }
            return String.Format(sqlSelect, (_isModeSimulation ? "_T" : string.Empty));
        }
        /// <summary>
        /// Retourne la requête de chargement de MCODET
        /// </summary>
        /// <param name="pIsNoDataAdapter"></param>
        /// <returns></returns>
        // EG 20150115 [20683] Add DTOBSOLETE
        private string GetSelectMcoDetColumn(bool pIsNoDataAdapter)
        {
            string sqlSelect = @"select mcod.IDMCO as ID, mcod.IDE, mcod.DTUPD, mcod.IDAUPD, mcod.DTINS, mcod.IDAINS, mcod.DTOBSOLETE, mcod.EXTLLINK, mcod.ROWATTRIBUT
            from dbo.MCODET{0} mcod" + Cst.CrLf;
            if (pIsNoDataAdapter)
            {
                sqlSelect += @"inner join dbo.MCO{0} mco on (mco.IDMCO = mcod.IDMCO)
                where (mcod.DTOBSOLETE is null)" + Cst.CrLf;
            }
            return String.Format(sqlSelect, (_isModeSimulation ? "_T" : string.Empty));
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeDs()
        {
            DataTable dt = _ds.Tables[0];
            dt.TableName = "MCO";
            dt = _ds.Tables[1];
            dt.TableName = "MCODET";
            //Relations
            DataRelation rel_Mso_MsoDet = new DataRelation("MCO_MCODET", DtMCO.Columns["ID"], DtMCODET.Columns["ID"], false);
            _ds.Relations.Add(rel_Mso_MsoDet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static int RowMCODetToIdE(DataRow dr)
        {
            return Convert.ToInt32(dr["IDE"].ToString());
        }
        #endregion Methods
    }

    /// <summary>
    /// collection of CNFMESSAGE
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("CNFMESSAGES", Namespace = "", IsNullable = true)]
    public partial class CnfMessages
    {
        #region Accessors
        /// <summary>
        /// Obtient le nombre de messages
        /// </summary>
        public int Count
        {
            get
            {
                return ArrFunc.Count(cnfMessage);
            }
        }
        #endregion Accessors

        #region Members
        [System.Xml.Serialization.XmlElementAttribute("CNFMESSAGE", Order = 1)]
        public CnfMessage[] cnfMessage;
        #endregion Members

        #region Indexors
        public CnfMessage this[int pIndex]
        {
            get
            {
                return cnfMessage[pIndex];
            }
        }
        #endregion Indexors

        #region Constructors
        public CnfMessages() { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Supprime les messages qui ne sont pas compatible avec au minimum 1 ncs  
        /// </summary>
        public void RemoveMessageWithoutNcsMatching()
        {
            for (int i = ArrFunc.Count(cnfMessage) - 1; -1 < i; i--)
            {
                if (false == cnfMessage[i].IsNcsMatching())
                    ReflectionTools.RemoveItemInArray(this, "cnfMessage", i);
            }
        }
        /// <summary>
        /// Chargement des messages qui sont compatibles avec les directives
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSettings">Représente les directives</param>
        /// <param name="pIdA_SendBy"></param>
        /// <param name="pIdA_SendTo"></param>
        /// <param name="pScanDataDtEnabled"></param>
        public void Initialize(string pCS, LoadMessageSettings pSettings, int pIdA_SendBy, int pIdA_SendTo, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            CnfMessages cnfMessages = CnfMessages.LoadCnfMessage(pCS, pSettings, pIdA_SendTo, pIdA_SendBy, pScanDataDtEnabled);
            cnfMessage = cnfMessages.cnfMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSettings"></param>
        /// <param name="pIdA_SendTo"></param>
        /// <param name="pIdA_SendBy"></param>
        /// <param name="pScanDataDtEnabled"></param>
        /// <returns></returns>
        /// FI 20140813 [20275] Modify
        /// FI 20170913 [23417] Modify 
        // EG 20180307 [23769] Gestion dbTransaction
        private static CnfMessages LoadCnfMessage(string pCS, LoadMessageSettings pSettings,
                                                  int pIdA_SendTo, int pIdA_SendBy,
                                                SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {

            CnfMessages ret = null;
            //-->Param
            DataParameters dataParameters = new DataParameters();
            if (0 != pIdA_SendBy)
                dataParameters.Add(new DataParameter(pCS, "IDA_SENDBY", DbType.Int32), pIdA_SendBy);
            //
            if (0 != pIdA_SendTo)
                dataParameters.Add(new DataParameter(pCS, "IDA_SENDTO", DbType.Int32), pIdA_SendTo);
            //
            //
            //-->where
            SQLWhere sqlWhere = new SQLWhere();
            //
            //MSGTYPE
            List<NotificationClassEnum> lstNotificationClass = new List<NotificationClassEnum>(pSettings.cnfClass);
            List<String> lstNotificationClassSerialized = lstNotificationClass.ConvertAll(
                    new Converter<NotificationClassEnum, String>(ConvertNotificationClassToString));
            string sqlColMsgType = "(" + DataHelper.SQLColumnIn(pCS, "cnfm.MSGTYPE", lstNotificationClassSerialized.ToArray(), TypeData.TypeDataEnum.@string) + ")";
            sqlWhere.Append(sqlColMsgType);

            //CNFTYPE
            if (ArrFunc.IsFilled(pSettings.cnfType))
            {
                List<NotificationTypeEnum> lstNotificationType = new List<NotificationTypeEnum>(pSettings.cnfType);
                List<String> lstNotificationTypeSerialized = lstNotificationType.ConvertAll(
                        new Converter<NotificationTypeEnum, String>(ConvertNotificationTypeToString));

                string sqlColCnfType = "(" + DataHelper.SQLColumnIn(pCS, "cnfm.CNFTYPE", lstNotificationTypeSerialized.ToArray(), TypeData.TypeDataEnum.@string) + ")";
                sqlWhere.Append(sqlColCnfType);
            }
            // RD 20140723 [20173] Mise en commentaire
            // Dans le cas des Confirmations, le type de la confirmation (CNFTYPE) n'est pas connu au lancement de la génération des instructions, 
            // donc on doit prendre en compte tous les messages de confirmation quelque soit le type,
            // Contrairement aux Editions, le type de l'edition (CNFTYPE) est connu d'avance: ALLOCATION, ...
            //else
            //{
            //    sqlWhere.Append("(cnfm.CNFTYPE is null)");
            //}

            //
            //Liste de message
            if (ArrFunc.IsFilled(pSettings.idCnfMessage))
            {
                string sqlCol = "(" + DataHelper.SQLColumnIn(pCS, "cnfm.IDCNFMESSAGE", pSettings.idCnfMessage, TypeData.TypeDataEnum.@int) + ")";
                sqlWhere.Append(sqlCol);
            }

            //IDSTBUSINESS
            dataParameters.Add(new DataParameter(pCS, "IDSTBUSINESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pSettings.idStBusiness.ToString());
            sqlWhere.Append("((cnfm.IDSTBUSINESS is null) or (cnfm.IDSTBUSINESS=@IDSTBUSINESS))");

            //PL 20140710 [20179]
            //IDSTMATCH - Rq.: idStMatch contient la liste des statuts actifs encadrés d'accolades (ex. {UNMATCH}{XXXXX})
            dataParameters.Add(new DataParameter(pCS, "IDSTMATCH", DbType.AnsiString, SQLCst.UT_STATUS_LEN * 10), pSettings.idStMatch.ToString());
            sqlWhere.Append("((cnfm.IDSTMATCH is null) or (@IDSTMATCH like '%{'+cnfm.IDSTMATCH+'}%'))");
            
            //PL 20140710 [20179]
            //IDSTCHECK - Rq.: idStCheck contient la liste des statuts actifs encadrés d'accolades (ex. {UNMATCH}{XXXXX})
            dataParameters.Add(new DataParameter(pCS, "IDSTCHECK", DbType.AnsiString, SQLCst.UT_STATUS_LEN * 10), pSettings.idStCheck.ToString());
            sqlWhere.Append("((cnfm.IDSTCHECK is null) or (@IDSTCHECK like '%{'+cnfm.IDSTCHECK+'}%'))");

            //STEPLIFE                
            List<NotificationStepLifeEnum> lstStepLife = new List<NotificationStepLifeEnum>(pSettings.stepLife);
            List<String> lstStepLifeSerialized = lstStepLife.ConvertAll(
                    new Converter<NotificationStepLifeEnum, String>(ConvertStepLifeToString));
            sqlWhere.Append("(" + DataHelper.SQLColumnIn(pCS, "cnfm.STEPLIFE", lstStepLifeSerialized, TypeData.TypeDataEnum.@string) + ")");

            //INSTRUMENT
            SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCS, null, pSettings.idI, false, pScanDataDtEnabled);
            string sqlWhereInstr = sqlInstrCriteria.GetSQLRestriction(pCS, "cnfm", RoleGInstr.CNF);
            sqlWhere.Append(sqlWhereInstr, false);

            //DERIVATIVECONTRACT & IDM
            //FI 20110116 appel à SQLDerivativeContractCriteria
            //FI 20140813 [20275] use pSettings.idM
            //FI 20170913 [23417] use SQLContractCriteria
            /*
            SQLDerivativeContractCriteria sqlDerivativeContractCriteria = new SQLDerivativeContractCriteria(pCS, pSettings.idDerivativeContract, pSettings.idM, pScanDataDtEnabled);
            string sqlWhereDC = sqlDerivativeContractCriteria.GetSQLRestriction(pCS, "cnfm", RoleContractRestrict.CNF);
            */

            SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCS, null, pSettings.contract, pSettings.idM, pScanDataDtEnabled);
            string sqlWhereContract = sqlContractCriteria.GetSQLRestriction("cnfm", RoleContractRestrict.CNF);
            sqlWhere.Append(sqlWhereContract, false);

            //ENABLED
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "cnfm"));

            if (dataParameters.Contains("IDA_SENDBY"))
                sqlWhere.Append("((cnfm.IDA_SENDBY=@IDA_SENDBY) or (cnfm.IDA_SENDBY is null))");
            if (dataParameters.Contains("IDA_SENDTO"))
                sqlWhere.Append("((cnfm.IDA_SENDTO=@IDA_SENDTO) or (cnfm.IDA_SENDTO is null))");

            if (0 == sqlWhere.Length())
                sqlWhere.Append("1=1");

            //sqlSelect1
            SQLWhere sqlWhere1 = new SQLWhere(sqlWhere.ToString());
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere1.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "cnfm"));

            //Select CNFMESSAGE Attention l'ordre doit être celui des membres de CnfMessage
            StrBuilder sqlSelect1 = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sqlSelect1 += "cnfm.IDCNFMESSAGE,cnfm.IDENTIFIER,cnfm.DISPLAYNAME,cnfm.DESCRIPTION," + Cst.CrLf;
            sqlSelect1 += "cnfm.CNFTYPE,cnfm.MSGTYPE,cnfm.STEPLIFE," + Cst.CrLf;
            sqlSelect1 += DataHelper.SQLIsNull(pCS, "cnfm.PERIODMLTPMSGISSUE", "0", "PERIODMLTPMSGISSUE") + ",";
            sqlSelect1 += DataHelper.SQLIsNullChar(pCS, "cnfm.PERIODMSGISSUE", PeriodEnum.D.ToString(), "PERIODMSGISSUE") + ",";
            sqlSelect1 += DataHelper.SQLIsNullChar(pCS, "cnfm.DAYTYPEMSGISSUE", DayTypeEnum.Business.ToString(), "DAYTYPEMSGISSUE") + "," + Cst.CrLf;
            sqlSelect1 += "cnfm.IDA_SENDBY,cnfm.IDA_SENDTO," + Cst.CrLf;
            sqlSelect1 += "cnfm.XSLTFILE,cnfm.IDGPARAM," + Cst.CrLf;
            sqlSelect1 += "cnfm.ISUSECHILDEVENTS,cnfm.ISUSEEVENTSI,cnfm.ISUSENOTEPAD As ISUSENOTEPAD," + Cst.CrLf;
            sqlSelect1 += "cnfm.EXTLLINK" + Cst.CrLf;
            sqlSelect1 += SQLCst.FROM_DBO + Cst.OTCml_TBL.CNFMESSAGE.ToString() + " cnfm" + Cst.CrLf;
            sqlSelect1 += sqlWhere1.ToString();
            sqlSelect1 += SQLCst.ORDERBY + "cnfm.IDENTIFIER" + SQLCst.ASC;

            //sqlSelect2
            SQLWhere sqlWhere2 = new SQLWhere(sqlWhere1.ToString());
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere2.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "ncs"));
            //Select CNFMESSAGENCS Attention l'ordre doit être celui des membres de CnfMessageNcs
            StrBuilder sqlSelect2 = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sqlSelect2 += "ncs.IDCNFMESSAGE,ncs.IDA_NCS" + Cst.CrLf;
            sqlSelect2 += SQLCst.FROM_DBO + Cst.OTCml_TBL.CNFMESSAGENCS.ToString() + " ncs" + Cst.CrLf;
            sqlSelect2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CNFMESSAGE.ToString() + " cnfm on cnfm.IDCNFMESSAGE = ncs.IDCNFMESSAGE" + Cst.CrLf;
            sqlSelect2 += sqlWhere2.ToString();

            //sqlSelect3
            SQLWhere sqlWhere3 = new SQLWhere(sqlWhere1.ToString());
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere3.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "cnfd"));
            //
            StrBuilder sqlSelect3 = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sqlSelect3 += "cnfd.IDCNFMESSAGE,cnfd.TYPEINSTR, cnfd.IDINSTR, cnfd.DATA" + Cst.CrLf;
            sqlSelect3 += SQLCst.FROM_DBO + Cst.OTCml_TBL.CNFMESSAGEDET.ToString() + " cnfd" + Cst.CrLf;
            sqlSelect3 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CNFMESSAGE.ToString() + " cnfm on cnfm.IDCNFMESSAGE = cnfd.IDCNFMESSAGE" + Cst.CrLf;
            sqlSelect3 += sqlWhere3.ToString();

            //FI 20120426 [17703] add sqlSelect4 
            SQLWhere sqlWhere4 = new SQLWhere(sqlWhere1.ToString());
            //FI 20120731 [18048] add ligne suivante, Dans un message il n'y a plus nécessairement des évènements trigger
            sqlWhere4.Append("cnfm.EVENTCODE is not null and cnfm.EVENTCLASS is not null");
            StrBuilder sqlSelect4 = new StrBuilder(SQLCst.SELECT);
            sqlSelect4 += "cnfm.IDCNFMESSAGE, cnfm.EVENTCODE,cnfm.EVENTTYPE, cnfm.EVENTCLASS" + Cst.CrLf;
            sqlSelect4 += SQLCst.FROM_DBO + Cst.OTCml_TBL.CNFMESSAGE.ToString() + " cnfm" + Cst.CrLf;
            sqlSelect4 += sqlWhere4.ToString() + Cst.CrLf;

            string selectGlobal = sqlSelect1.ToString() + SQLCst.SEPARATOR_MULTISELECT +
                                  sqlSelect2.ToString() + SQLCst.SEPARATOR_MULTISELECT +
                                  sqlSelect3.ToString() + SQLCst.SEPARATOR_MULTISELECT +
                                  sqlSelect4.ToString();

            QueryParameters queryParameters = new QueryParameters(pCS, selectGlobal, dataParameters);

            DataSet dsResult = DataHelper.ExecuteDataset(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            dsResult.DataSetName = "CNFMESSAGES";

            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "CNFMESSAGE";

            dtTable = dsResult.Tables[1];
            dtTable.TableName = "CNFMESSAGENCS";

            dtTable = dsResult.Tables[2];
            dtTable.TableName = "CNFMESSAGEDET";

            //FI 20120426 [17703] add EVENTTRIGGER Table and EVENTTRIGGER Relation
            dtTable = dsResult.Tables[3];
            dtTable.TableName = "EVENTTRIGGER";

            DataRelation rel = new DataRelation("CNFMESSAGENCS",
                                                dsResult.Tables["CNFMESSAGE"].Columns["IDCNFMESSAGE"],
                                                dsResult.Tables["CNFMESSAGENCS"].Columns["IDCNFMESSAGE"], false)
            {
                Nested = true
            };
            dsResult.Relations.Add(rel);

            rel = new DataRelation("CNFMESSAGEDET",
                                                dsResult.Tables["CNFMESSAGE"].Columns["IDCNFMESSAGE"],
                                                dsResult.Tables["CNFMESSAGEDET"].Columns["IDCNFMESSAGE"], false)
            {
                Nested = true
            };
            dsResult.Relations.Add(rel);

            rel = new DataRelation("EVENTTRIGGER",
                                                dsResult.Tables["CNFMESSAGE"].Columns["IDCNFMESSAGE"],
                                                dsResult.Tables["EVENTTRIGGER"].Columns["IDCNFMESSAGE"], false)
            {
                Nested = true
            };
            dsResult.Relations.Add(rel);

            string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(CnfMessages), dsSerializerResult);
            ret = (CnfMessages)CacheSerializer.Deserialize(serializeInfo);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private static string ConvertStepLifeToString(NotificationStepLifeEnum pValue)
        {
            return ReflectionTools.ConvertEnumToString<NotificationStepLifeEnum>(pValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private static string ConvertNotificationClassToString(NotificationClassEnum pValue)
        {
            return ReflectionTools.ConvertEnumToString<NotificationClassEnum>(pValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private static string ConvertNotificationTypeToString(NotificationTypeEnum pValue)
        {
            return ReflectionTools.ConvertEnumToString<NotificationTypeEnum>(pValue); 
        }
        #endregion
    }

    /// <summary>
    /// Represente un message (CNFMESSAGE)
    /// </summary>
    public partial class CnfMessage
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDCNFMESSAGE")]
        public int idCnfMessage;

        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER")]
        public string identifier;

        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME")]
        public string displayName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DESCRIPTION")]
        public string description;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cnfTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CNFTYPE")]
        public string cnfType;

        [System.Xml.Serialization.XmlElementAttribute("MSGTYPE")]
        public string msgType;

        [System.Xml.Serialization.XmlElementAttribute("STEPLIFE")]
        public NotificationStepLifeEnum stepLifeEnum;

        /// <summary>
        /// Liste des évènements qui peuvent déclencher la génération du message
        /// </summary>
        // FI 20120426 [17703] possibilité de définir plusieurs évènements déclencheurs de message
        [System.Xml.Serialization.XmlElementAttribute("EVENTTRIGGER")]
        public EventTrigger[] eventTrigger;
        //
        //PL 20091022
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool periodMltpMsgIssueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PERIODMLTPMSGISSUE")]
        public int periodMltpMsgIssue;

        [System.Xml.Serialization.XmlElementAttribute("PERIODMSGISSUE")]
        public PeriodEnum periodMsgIssue;

        [System.Xml.Serialization.XmlElementAttribute("DAYTYPEMSGISSUE")]
        public DayTypeEnum dayTypeMsgIssue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Ida_SendBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_SENDBY")]
        public int ida_SendBy;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Ida_SendToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_SENDTO")]
        public int ida_SendTo;

        [System.Xml.Serialization.XmlElementAttribute("XSLTFILE")]
        public string xsltFile;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idGParamSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDGPARAM")]
        public int idGParam;

        [System.Xml.Serialization.XmlElementAttribute("ISUSECHILDEVENTS")]
        public bool isUseChildEvents;

        [System.Xml.Serialization.XmlElementAttribute("ISUSEEVENTSI")]
        public bool isUseEventSI;

        [System.Xml.Serialization.XmlElementAttribute("ISUSENOTEPAD")]
        public bool isUseNotepad;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cnfMessageNcsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CNFMESSAGENCS")]
        public CnfMessageNcs[] cnfMessageNcs;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cnfMessageDetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CNFMESSAGEDET")]
        public CnfMessageDet[] cnfMessageDet;
        #endregion

        #region Accessors
        /// <summary>
        /// Obtient true si edition simple ou edition consolidée
        /// </summary>
        public bool IsMulti
        {
            get
            {
                return ((NotificationClass == NotificationClassEnum.MULTITRADES) ||
                        (NotificationClass == NotificationClassEnum.MULTIPARTIES));
            }
        }

        /// <summary>
        /// Obtient la classe du message (Edition simple, consolidée ou confirmation)
        /// </summary>
        /// EG 20171113 Upd 
        /// EG 20180423 Analyse du code Correction [CA1065]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public NotificationClassEnum NotificationClass
        {
            get
            {
                Nullable<NotificationClassEnum> cnfClass = ReflectionTools.ConvertStringToEnumOrNullable<NotificationClassEnum>(msgType);
                if (null == cnfClass)
                    throw new InvalidOperationException(StrFunc.AppendFormat("Notification Class {0} is not defined", cnfClass.ToString()));
                return cnfClass.Value;
            }
        }

        /// <summary>
        /// Obtient le type du message
        /// </summary>
        /// EG 20180423 Analyse du code Correction [CA1065]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<NotificationTypeEnum> NotificationType
        {
            get
            {
                Nullable<NotificationTypeEnum> ret = null;
                if (StrFunc.IsFilled(cnfType))
                {
                    if (false == System.Enum.IsDefined(typeof(NotificationTypeEnum), cnfType))
                        throw new InvalidOperationException(StrFunc.AppendFormat("Notification Type {0} is not defined in NotificationTypeEnum ({1})", cnfType, NotificationTypeEnum.ORDERALLOC));
                    ret = (NotificationTypeEnum)System.Enum.Parse(typeof(NotificationTypeEnum), cnfType);
                }
                return ret;
            }
        }
        #endregion Accessors

        #region constructor
        public CnfMessage()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retourne les directives paramétrées sur le message 
        /// </summary>
        /// <returns></returns>
        public NotificationDocumentSettings GetSettings()
        {
            return new NotificationDocumentSettings(this.isUseChildEvents, this.isUseEventSI, this.isUseNotepad, this.cnfMessageDet);
        }

        /// <summary>
        /// Retourne la date de message à partir d'une date donnée
        /// <para>Cette méthode exploite l'offset éventuellement paramétré sur le message</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtStart"></param>
        /// <param name="pCnfChain"></param>
        /// <param name="IProductBase"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public DateTime GetDateToSend(string pCS, DateTime pDtStart, ConfirmationChain pCnfChain, IProductBase pProduct)
        {
            DateTime ret = pDtStart;
            //FI 20120302 Le décalage ne doit être pas être effectué sur les messages EOD
            //Ces derniers sont générés à des dates business, il ne faut pas décaler vers un jour précédent
            //Exemple
            //L'utilisateur lance une situation financière au 01/11 (En France ouvré pour le business, mais férié par ailleurs)
            //Il ne faut pas que le message soit en date du 31/01 mais bien du 01/01
            if (this.stepLifeEnum != NotificationStepLifeEnum.EOD)
            {
                IOffset offset = pProduct.CreateOffset(this.periodMsgIssue, this.periodMltpMsgIssue, this.dayTypeMsgIssue);
                ArrayList aIDBCs = new ArrayList();
                //Contact office Emetteur
                if (StrFunc.IsFilled(pCnfChain[SendEnum.SendBy].sqlContactOffice.IdBC))
                    aIDBCs.Add(pCnfChain[SendEnum.SendBy].sqlActor.IdBC);
                //
                if (pCnfChain.IsContactOfficesIdentical)
                {
                    //Acteur client si Interne
                    if (StrFunc.IsFilled(pCnfChain[SendEnum.SendBy].sqlActor.IdBC))
                        aIDBCs.Add(pCnfChain[SendEnum.SendBy].sqlActor.IdBC);
                }
                else
                {
                    //Contact office Destinataire si non interne
                    if (StrFunc.IsFilled(pCnfChain[SendEnum.SendBy].sqlContactOffice.IdBC))
                        aIDBCs.Add(pCnfChain[SendEnum.SendBy].sqlContactOffice.IdBC);
                }

                BusinessDayConventionEnum bdaConvention = BusinessDayConventionEnum.PRECEDING;
                if (this.periodMltpMsgIssue > 0)
                    bdaConvention = BusinessDayConventionEnum.FOLLOWING;
                //
                //20080718 PL Un IDBC est obligatoire en DayTypeEnum.Business, on ajoute donc N/A (en espérant que le client ne l'ait pas supprimé...)
                //            NB: On se trouve dans ce cas si aucun acteur n'a de BC de paramétré
                if (aIDBCs.Count == 0)
                    aIDBCs.Add(Cst.NotAvailable);
                //
                IBusinessDayAdjustments bda = ((IProductBase)
                        pProduct).CreateBusinessDayAdjustments(bdaConvention, (string[])aIDBCs.ToArray(typeof(string)));
                //
                EFS_Offset efs_Offset = new EFS_Offset(pCS, offset, pDtStart, bda, null as DataDocumentContainer);
                ret = efs_Offset.offsetDate[0];
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne la liste des éléments spécifiés dans les directives de tri
        /// <para>Cette liste est triée (fonction de la colonne CNFMESSAGEORDERBY.POSITION)</para>
        /// <para>Pour les messages MULTI-TRADES et MULTI-PARTIES, Retourne un tri par défaut s'il n'existe pas de paramétrage</para>
        /// <para>Retourne null s'il n'existe pas de tri</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20120830 [18048] Refactoring
        /// Le tri par défaut n'est alimenté que pour ALLOCATION,POSITION,POSSYNTHETIC,POSACTION
        /// seuls états qui utilient l'élément tradeSorting 
        /// FI 20130626 [18745] add SYNTHESIS
        public ReportSortEnum[] GetOrderBy()
        {
            ReportSortEnum[] ret = null;
            //
            ArrayList al = new ArrayList();
            //
            //Affecter un éventuel tri paramétré, TODO
            //
            //S'il n'existe pas de tri paramétré => Mise en place d'un tri par défaut
            if (ArrFunc.IsEmpty(al))
            {
                bool isOrderby_MatketAndDerivativeContract = IsMulti &&
                    (
                    (NotificationType == NotificationTypeEnum.ALLOCATION) ||
                    (NotificationType == NotificationTypeEnum.POSITION) ||
                    (NotificationType == NotificationTypeEnum.POSSYNTHETIC) ||
                    (NotificationType == NotificationTypeEnum.POSACTION) ||
                    (NotificationType == NotificationTypeEnum.SYNTHESIS)
                    );

                if (isOrderby_MatketAndDerivativeContract)
                {
                    al.Add(ReportSortEnum.MARKET);
                    al.Add(ReportSortEnum.DERIVATIVECONTRACT);
                }
            }
            //    
            if (ArrFunc.IsFilled(al))
                ret = (ReportSortEnum[])al.ToArray(typeof(ReportSortEnum));
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNotificationTypeEnum"></param>
        /// <returns></returns>
        /// FI 20190515 [23912] add Method
        public Boolean IsNotificationType(NotificationTypeEnum pNotificationTypeEnum)
        {
            Boolean ret = false;
            if (NotificationType.HasValue)
                ret = (NotificationType.Value == pNotificationTypeEnum);
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNotificationClassEnum"></param>
        /// <returns></returns>
        /// FI 20190515 [23912] add Method
        public Boolean IsNotificationClass(NotificationClassEnum pNotificationClassEnum)
        {
            return (NotificationClass == pNotificationClassEnum);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CnfMessage : ICloneable
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StringData[] xsltTransformParam;
        #endregion

        #region Méthodes
        /// <summary>
        /// Retourne les dates (DTEVENT/DTEVENTFORCED) des évènements d'un trade {pIdT} déclencheur du message, par matching des EVENTCODE,[EVENTTYPE] et EVENTCLASS.  
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20120426 [17703] prise en compte de eventTrigger 
        /// GLOP FI EVENTTRIGER : Pensez a optimiser (On pourrait éventuellement jouer uen seule requête avec des union)
        /// EG 20180426 Analyse du code Correction [CA2202]
        /// FI 20180616 [24718] La méthode reourne une Pair de Datetime (first=> DTEVENT, second=> DTEVENTFORCED)     
        public Pair<DateTime, DateTime>[] GetTriggerEventDate(string pCs, int pIdT)
        {
            List<Pair<DateTime, DateTime>> lstDate = new List<Pair<DateTime, DateTime>>();

            //FI 20120731 [18048] add ArrFunc.IsFilled(eventTrigger) => il peut y avoir plusieurs évènements déclencheur
            if (ArrFunc.IsFilled(eventTrigger))
            {
                foreach (EventTrigger eventItem in eventTrigger)
                {
                    DataParameters sqlParam = new DataParameters();
                    sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);
                    sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EVENTCODE), eventItem.eventCode);
                    if (eventItem.eventTypeSpecified)
                        sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EVENTTYPE), eventItem.eventType);
                    sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EVENTCLASS), eventItem.eventClass);

                    //FI 20110128 [17283] utilisation de DTEVENTFORCED à la place de DTSTARTUNADJ

                    StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT);
                    sqlSelect += "ec.DTEVENT,ec.DTEVENTFORCED" + Cst.CrLf;
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
                    sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on ec.IDE=e.IDE" + Cst.CrLf;
                    sqlSelect += SQLCst.WHERE + "e.IDT=@IDT";
                    sqlSelect += SQLCst.AND + "e.EVENTCODE=@EVENTCODE";
                    if (eventItem.eventTypeSpecified)
                        sqlSelect += SQLCst.AND + "e.EVENTTYPE=@EVENTTYPE";
                    sqlSelect += SQLCst.AND + "ec.EVENTCLASS=@EVENTCLASS";
                    sqlSelect += SQLCst.ORDERBY + "1";

                    QueryParameters queryparameters = new QueryParameters(pCs, sqlSelect.ToString(), sqlParam);
                    using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, queryparameters.Query, queryparameters.Parameters.GetArrayDbParameter()))
                    {
                        while (dr.Read())
                        {
                            DateTime dt = Convert.ToDateTime(dr.GetValue(0));
                            DateTime dtForced = Convert.ToDateTime(dr.GetValue(1));
                            Pair<DateTime, DateTime> item = new Pair<DateTime, DateTime>(dt, dtForced);
                            if (false == lstDate.Contains(item, new PairComparer<DateTime, DateTime>()))
                                lstDate.Add(new Pair<DateTime, DateTime>(dt, dtForced));
                        }
                    }
                }
            }

            Pair<DateTime, DateTime>[] ret = null;
            if (ArrFunc.IsFilled(lstDate))
                ret = lstDate.ToArray();

            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements déclencheurs du message associés au trade {pIdT}  
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDate">Représente la date du message</param>
        /// <param name="pIdT">Représente l'Id du trade</param>
        /// <returns></returns>
        /// EG 20180426 Analyse du code Correction [CA2202]
        public int[] GetTriggerEvent(string pCS, DateTime pDate, int pIdT)
        {
            List<int> lstIdE = new List<int>();
            //
            //FI 20120731 [18048] add ArrFunc.IsFilled(eventTrigger) => il n'y a plus nécessairement d'évènement déclencheur
            if (ArrFunc.IsFilled(eventTrigger))
            {
                foreach (EventTrigger eventItem in eventTrigger)
                {
                    DataParameters sqlParam = new DataParameters();

                    sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), pDate);
                    sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);

                    sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCODE), eventItem.eventCode);
                    if (eventItem.eventTypeSpecified)
                        sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTTYPE), eventItem.eventType);
                    sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCLASS), eventItem.eventClass);

                    StrBuilder sqlSelect = new StrBuilder();
                    sqlSelect +=
                    @"select ec.IDE
                    from dbo.EVENT e 
                    inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE
                    where e.IDT=@IDT 
                    and e.EVENTCODE=@EVENTCODE";
                    if (eventItem.eventTypeSpecified)
                        sqlSelect += " and e.EVENTTYPE=@EVENTTYPE";
                    sqlSelect += " and ec.EVENTCLASS=@EVENTCLASS";

                    //FI 20110128 [17283] utilisation de DTEVENTFORCED à la place de DTSTARTUNADJ
                    //PL 20110826 Use stepLifeEnum value
                    if (this.stepLifeEnum == NotificationStepLifeEnum.EOD)
                        sqlSelect += " and ec.DTEVENT=@DTEVENT";
                    else
                        sqlSelect += " and ec.DTEVENTFORCED=@DTEVENT";

                    QueryParameters queryparameters = new QueryParameters(pCS, sqlSelect.ToString(), sqlParam);

                    using (IDataReader dr = DataHelper.ExecuteReader(queryparameters.Cs, CommandType.Text, queryparameters.Query, queryparameters.Parameters.GetArrayDbParameter()))
                    {
                        while (dr.Read())
                        {
                            int idE = Convert.ToInt32(dr.GetValue(0));
                            if (false == lstIdE.Contains(idE))
                                lstIdE.Add(idE);
                        }
                    }
                }
            }
            //
            int[] ret = null;
            if (ArrFunc.IsFilled(lstIdE))
                ret = lstIdE.ToArray();
            //
            return ret;
        }

        /// <summary>
        /// Retourne les systèmes de messagerie (NCS) compatibles avec le message
        /// <para>Retourne null s'il en existe pas</para>
        /// <para>NCS: Notification Confirmation System</para>
        /// </summary>
        /// <returns></returns>
        public int[] GetIdNcsCompatible()
        {
            int[] ret = null;
            ArrayList al = new ArrayList();
            //
            if (cnfMessageNcsSpecified)
            {
                for (int i = 0; i < ArrFunc.Count(cnfMessageNcs); i++)
                    al.Add(cnfMessageNcs[i].idNcs);
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));
            //
            return ret;
        }

        /// <summary>
        /// Retourne true si le message est compatible avec le NCS {pIdNcs}
        /// </summary>
        /// <param name="pIdNcs"></param>
        /// <returns></returns>
        public bool IsMatchedWithNcs(int pIdNcs)
        {
            bool ret = false;
            if (cnfMessageNcsSpecified)
            {
                for (int i = 0; i < ArrFunc.Count(cnfMessageNcs); i++)
                {
                    if (cnfMessageNcs[i].idNcs == pIdNcs)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// Retourne true si le message est compatible avec au minimum 1 NCS 
        /// </summary>
        /// <returns></returns>
        public bool IsNcsMatching()
        {
            bool ret = false;
            int[] idNcs = GetIdNcsCompatible();
            if (ArrFunc.IsFilled(idNcs))
            {
                for (int i = 0; i < ArrFunc.Count(idNcs); i++)
                {
                    if (idNcs[i] > 0)
                        ret = true;
                    if (ret)
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Charge les paramètres XSL disponibles sous PARAMG
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        public void LoadXsltTransformParam(string pCS, int pIdT)
        {
            xsltTransformParam = null;
            if (idGParamSpecified)
                xsltTransformParam = ConfirmationTools.LoadParamG(CSTools.SetCacheOn(pCS), idGParam, pIdT);
        }

        #endregion

        #region ICloneable Membres
        public object Clone()
        {
            return ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CnfMessageDet
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDCNFMESSAGE")]
        public int idCnfMessage;
        [System.Xml.Serialization.XmlElementAttribute("TYPEINSTR")]
        public TypeInstrEnum typeInstr;
        [System.Xml.Serialization.XmlElementAttribute("IDINSTR")]
        public int idInstr;
        [System.Xml.Serialization.XmlElementAttribute("DATA")]
        public InvoicingTradeDetailEnum data;
        #endregion Members
        //
        #region Constructors
        public CnfMessageDet()
        {
        }
        #endregion Constructors
        //
        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Represente un couple (CNFMESSAGE,NCS)
    /// </summary>
    public partial class CnfMessageNcs
    {
        #region public Members
        [System.Xml.Serialization.XmlElementAttribute("IDCNFMESSAGE")]
        public int idCnfMessage;
        [System.Xml.Serialization.XmlElementAttribute("IDA_NCS")]
        public int idNcs;
        #endregion

        #region constructor
        public CnfMessageNcs()
        {
        }
        public CnfMessageNcs(int pIdCnfMessage, int pIdNcs)
        {
            idCnfMessage = pIdCnfMessage;
            idNcs = pIdNcs;
        }
        #endregion
    }

    /// <summary>
    ///  represente 1 message + 1 ncs
    /// </summary>
    public partial class CnfMessageNcs
    {
        #region private Members
        private SQL_ConfirmationMessage _sqlCnfMessage;
        private SQL_Ncs _sqlNcs;
        private bool _isSqlTableLoaded;
        #endregion
        //
        #region accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_ConfirmationMessage SqlCnfMessage
        {
            get
            {
                return _sqlCnfMessage;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_Ncs SqlNcs
        {
            get { return _sqlNcs; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsSqlTableLoaded
        {
            get { return _isSqlTableLoaded; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CnfMessageIdentifier
        {
            get
            {
                string ret = string.Empty;
                if (null != SqlCnfMessage)
                    ret = SqlCnfMessage.Identifier;
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NcsIdentifier
        {
            get
            {
                string ret = string.Empty;
                if (null != SqlNcs)
                    ret = SqlNcs.Identifier;
                return ret;
            }
        }
        #endregion

        /// <summary>
        /// Charge sqlTrable de CnfMessage et Ncs 
        /// </summary>
        /// <param name="pCs"></param>
        public void LoadSqlTable(string pCs)
        {
            _isSqlTableLoaded = true;
            //
            _sqlCnfMessage = new SQL_ConfirmationMessage(pCs, idCnfMessage);
            _sqlCnfMessage.LoadTable();
            //
            _sqlNcs = new SQL_Ncs(pCs, idNcs);
            _sqlNcs.LoadTable();
        }

    }

    /// <summary>
    /// Represente un message à envoyer
    /// <para>
    /// dateInfo représente les dates de message
    /// </para>
    /// <para>
    /// ncsInciChain représente les NCS chargés de transporter le message et pour chacun d'eux les instructions de chaque côté de la chaîne
    /// </para>
    /// </summary>
    public class CnfMessageToSend : CnfMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public enum SetNcsInciChainErrLevel
        {
            Succes,
            /// <summary>
            /// Aucune instruction de reception
            /// <para> </para>
            /// </summary>
            NotFound_CI_on_SendTo,
            /// <summary>
            /// Aucune instruction en émission
            /// </summary>
            NotFound_CI_on_SendBy,
            /// <summary>
            /// Aucune instruction par défaut en émission vers les clients
            /// </summary>
            NotFound_CI_on_SendBy_for_DefaultToClient,
            /// <summary>
            /// Aucune instruction par défaut en émission vers les donneurs d'ordre maison
            /// </summary>
            NotFound_CI_on_SendBy_for_DefaultToEntity,
            /// <summary>
            /// Aucune instruction par défaut en émission vers les contreparties externes
            /// </summary>
            NotFound_CI_on_SendBy_for_DefaultToExternalCtr,
            /// <summary>
            /// 
            /// </summary>
            Undefined
        }

        

        #region accessor
        /// <summary>
        /// Represente les dates d'envoi (Date de l'évènement déclencheur, date d'envoi du message, date d'envoi forced du message)
        /// </summary>
        public NotificationSendDateInfo[] DateInfo
        {
            get;
            set;
        }
        /// <summary>
        ///Represente les systèmes de messageries (NCS) chargés de transporter le message et pour chacun d'eux les instructions de chaque côté de la chaîne
        /// </summary>
        public NcsInciChain[] NcsInciChain
        {
            get;
            set;
        }
        #endregion

        #region constructor
        public CnfMessageToSend()
            : base() { }
        public CnfMessageToSend(CnfMessage pCnfMessage)
        {
            //
            this.idCnfMessage = pCnfMessage.idCnfMessage;
            this.identifier = pCnfMessage.identifier;
            this.displayName = pCnfMessage.displayName;
            //
            this.descriptionSpecified = pCnfMessage.descriptionSpecified;
            this.description = pCnfMessage.description;
            //
            this.cnfTypeSpecified = pCnfMessage.cnfTypeSpecified;
            this.cnfType = pCnfMessage.cnfType;
            this.msgType = pCnfMessage.msgType;
            this.stepLifeEnum = pCnfMessage.stepLifeEnum;
            //
            this.eventTrigger = pCnfMessage.eventTrigger;
            //
            //PL 20091022
            //this.periodMltpMsgIssueSpecified = pCnfMessage.periodMltpMsgIssueSpecified;
            this.periodMltpMsgIssue = pCnfMessage.periodMltpMsgIssue;
            this.periodMsgIssue = pCnfMessage.periodMsgIssue;
            this.dayTypeMsgIssue = pCnfMessage.dayTypeMsgIssue;
            //
            this.Ida_SendBySpecified = pCnfMessage.Ida_SendBySpecified;
            this.ida_SendBy = pCnfMessage.ida_SendBy;
            //
            this.Ida_SendToSpecified = pCnfMessage.Ida_SendToSpecified;
            this.ida_SendTo = pCnfMessage.ida_SendTo;
            //
            this.xsltFile = pCnfMessage.xsltFile;
            //
            this.idGParamSpecified = pCnfMessage.idGParamSpecified;
            this.idGParam = pCnfMessage.idGParam;
            //
            this.isUseChildEvents = pCnfMessage.isUseChildEvents;
            this.isUseEventSI = pCnfMessage.isUseEventSI;
            this.isUseNotepad = pCnfMessage.isUseNotepad;
            //
            this.extlLinkSpecified = pCnfMessage.extlLinkSpecified;
            this.extlLink = pCnfMessage.extlLink;
            //
            this.cnfMessageNcsSpecified = pCnfMessage.cnfMessageNcsSpecified;
            this.cnfMessageNcs = pCnfMessage.cnfMessageNcs;
            //
            this.xsltTransformParam = pCnfMessage.xsltTransformParam;
        }
        #endregion

        #region method
        /// <summary>
        /// Charge des instructions de chaque côté de la chaîne de confirmation et alimente ncsInciChain (voir définition) 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pConfirmationChain"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pStBusiness"></param>
        /// <param name="pStMatch"></param>
        /// <param name="pStCheck"></param>
        /// <returns></returns>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// EG 20160404 Migration vs2013  
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public SetNcsInciChainErrLevel SetNcsInciChain(string pCs, ConfirmationChain pConfirmationChain, int pIdM,
                int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory,int> pContract, string pStBusiness, string pStMatch, string pStCheck)
        {
            SetNcsInciChainErrLevel ret = SetNcsInciChainErrLevel.Undefined;
            
            bool isSearch_CI_on_SendBy = false;
            NotificationSendToClass sendToType = pConfirmationChain.SendTo(pCs, NotificationClass);
            NotificationSendToClass recipientType = sendToType;
            switch (sendToType)
            {
                case NotificationSendToClass.Client:
                    // EG 20160404 Migration vs2013 
                    //recipientType = NotificationRecipientTypeEnum.CLIENT;
                    isSearch_CI_on_SendBy = pConfirmationChain.IsContactOfficesIdentical;
                    break;
                case NotificationSendToClass.Entity:
                    // EG 20160404 Migration vs2013 
                    //recipientType = NotificationRecipientTypeEnum.ENTITY;
                    isSearch_CI_on_SendBy = pConfirmationChain.IsContactOfficesIdentical;
                    break;
                case NotificationSendToClass.External:
                    // EG 20160404 Migration vs2013 
                    //recipientType = NotificationRecipientTypeEnum.EXTERNAL_COUNTERPARTY;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("sendToType:{0} is not implemented", sendToType.ToString()));
            }

            ArrayList al = new ArrayList();

            //Recherche des C.I. côté SendTo par ordre de priorité. 
            //Arrêt dès que le sendBy possède au moins une instruction qui matche
            if (!isSearch_CI_on_SendBy)
            {
                #region isSendToExternal or isSendTo_Client with specific C.I.
                CnfMessageNcsInciss cnfMsgNcsIncissSendTo = new CnfMessageNcsInciss();
                //PL 20140710 [20179]
                cnfMsgNcsIncissSendTo.Load(pCs, (CnfMessage)this, SendReceiveEnum.Receive,
                    pConfirmationChain[SendEnum.SendTo].IdAContactOffice,
                    pConfirmationChain[SendEnum.SendTo].IdATrader,
                    pConfirmationChain[SendEnum.SendTo].IdBook, pIdM,
                    pIdI, pIdIUnderlyer, pIdC, pContract, pStBusiness, pStMatch, pStCheck, null, false);


                CnfMessageNcsIncisBest cnfMsgNcsBestSendTo = cnfMsgNcsIncissSendTo.GetCnfMessageNcsIncisBest();
                //
                if (cnfMsgNcsBestSendTo.Count == 0)
                {
                    //Aucune C.I. sur le SendTo --> Tentative de recherche de C.I. depuis le SendBy 
                    ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendTo;
                    isSearch_CI_on_SendBy = true;
                }
                else
                {
                    bool isToSend = false;

                    //Recherche par ordre ordre de priorité côté émetteur
                    int[] prioritySendTo = cnfMsgNcsBestSendTo.GetDistinctPriorityRankInOrder();
                    for (int i = 0; i < ArrFunc.Count(prioritySendTo); i++)
                    {
                        CnfMessageNcsInci[] cnfMsgNcsInciSendTo = cnfMsgNcsBestSendTo.GetCnfMessageNcsInciOnPriority(prioritySendTo[i]);
                        for (int k = 0; k < ArrFunc.Count(cnfMsgNcsInciSendTo); k++)
                        {
                            if (cnfMsgNcsInciSendTo[k].Inci.isToReceive)
                            {
                                isToSend = true;
                                // isToSend = true:
                                // - Il existe au moins 1 instruction de reception, cela veut dire que le destinataire veut recevoir ce message
                                // - Si l'émetteur n'est pas en mesure d'émettre ce message (aucune instruction quel que soit le ncs) => Alors Erreur

                                // Recherche d'existance côté sendBy, d'une instruction qui match
                                CnfMessageNcsIncis cnfMsgNcsIncisSendBy = new CnfMessageNcsIncis(cnfMsgNcsInciSendTo[k].CnfMessageNcs, SendReceiveEnum.Send,
                                    pConfirmationChain[SendEnum.SendBy].IdATrader, pConfirmationChain[SendEnum.SendBy].IdBook, recipientType);
                                //20140710 PL [TRIM 20179]
                                cnfMsgNcsIncisSendBy.LoadIncis(pCs, pConfirmationChain[SendEnum.SendBy].IdAContactOffice, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                                                                pStBusiness, pStMatch, pStCheck, false);
                                bool isOkSendBy = (cnfMsgNcsIncisSendBy.Incis.Count > 0);
                                if (isOkSendBy)
                                    isOkSendBy = cnfMsgNcsIncisSendBy.Incis[0].isToSend;
                                if (isOkSendBy)
                                {
                                    InciChain inciChain = new InciChain();
                                    NotificationConfirmationSystem ncs = ConfirmationTools.LoadNotificationConfirmationSystem(pCs, SQL_TableWithID.IDType.Id, cnfMsgNcsInciSendTo[k].CnfMessageNcs.idNcs.ToString());
                                    inciChain[SendEnum.SendTo] = cnfMsgNcsInciSendTo[k].Inci;
                                    inciChain[SendEnum.SendBy] = cnfMsgNcsIncisSendBy.Incis[0];

                                    al.Add(new NcsInciChain(ncs, inciChain));
                                }
                            }
                        }
                        //Dans l'ordre des priorités, si au moins une instruction receptrice est satisfaite, on arrête là.
                        if (ArrFunc.IsFilled(al))
                            break;
                    }

                    if (isToSend && ArrFunc.IsEmpty(al))
                        ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy;
                    else
                        ret = SetNcsInciChainErrLevel.Succes;
                }
                #endregion
            }
            
            //Recherche de C.I. depuis le SendBy
            if (isSearch_CI_on_SendBy)
            {
                CnfMessageNcsInciss cnfMsgNcsIncissSendBy = new CnfMessageNcsInciss();
                //PL 20140710 [20179]
                cnfMsgNcsIncissSendBy.Load(pCs, (CnfMessage)this, SendReceiveEnum.Send,
                    pConfirmationChain[SendEnum.SendBy].IdAContactOffice,
                    pConfirmationChain[SendEnum.SendBy].IdATrader,
                    pConfirmationChain[SendEnum.SendBy].IdBook,
                    pIdM, pIdI, pIdIUnderlyer, pIdC, pContract, pStBusiness, pStMatch, pStCheck, recipientType, true);

                CnfMessageNcsIncisBest cnfMsgNcsBestSendBy = cnfMsgNcsIncissSendBy.GetCnfMessageNcsIncisBest();

                if (cnfMsgNcsBestSendBy.Count == 0)
                {
                    // EG 20160404 Migration vs2013 
                    //if (recipientType == NotificationRecipientTypeEnum.CLIENT)
                    //    ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToClient;
                    //else if (recipientType == NotificationRecipientTypeEnum.ENTITY)
                    //    ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToEntity;
                    //else if (recipientType == NotificationRecipientTypeEnum.EXTERNAL_COUNTERPARTY)
                    //    ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToExternalCtr;
                    if (recipientType == NotificationSendToClass.Client)
                        ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToClient;
                    else if (recipientType == NotificationSendToClass.Entity)
                        ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToEntity;
                    else if (recipientType == NotificationSendToClass.External)
                        ret = SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToExternalCtr;
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("recipientType:{0} is not implemented", recipientType.ToString()));
                }
                else
                {
                    //Recherche par ordre ordre de priorité côté sender
                    int[] prioritySendBy = cnfMsgNcsBestSendBy.GetDistinctPriorityRankInOrder();
                    CnfMessageNcsInci[] cnfMsgNcsInciSendBy = cnfMsgNcsBestSendBy.GetCnfMessageNcsInciOnPriority(prioritySendBy[0]);
                    for (int i = 0; i < ArrFunc.Count(cnfMsgNcsInciSendBy); i++)
                    {
                        NotificationConfirmationSystem ncs = ConfirmationTools.LoadNotificationConfirmationSystem(pCs, SQL_TableWithID.IDType.Id, cnfMsgNcsInciSendBy[i].CnfMessageNcs.idNcs.ToString());
                        InciChain inciChain = new InciChain();
                        inciChain[SendEnum.SendBy] = cnfMsgNcsInciSendBy[i].Inci;
                        inciChain[SendEnum.SendTo] = new Inci();
                        //
                        if (inciChain[SendEnum.SendBy].isToSend)
                            al.Add(new NcsInciChain(ncs, inciChain));
                    }

                    ret = SetNcsInciChainErrLevel.Succes;
                }
            }

            if (ArrFunc.IsFilled(al))
                NcsInciChain = (NcsInciChain[])al.ToArray(typeof(NcsInciChain));

            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Représnte un couple de INCI 
    /// </summary>
    public class InciChain
    {
        /// <summary>
        /// 
        /// </summary>
        public Inci[] inci;

        /// <summary>
        /// Retourne l'instruction en Emission ou l'instruction en récepion
        /// </summary>
        /// <param name="sendSide"></param>
        /// <returns></returns>
        public Inci this[SendEnum sendSide]
        {
            get
            {
                if (SendEnum.SendBy == sendSide)
                    return inci[0];
                else
                    return inci[1];
            }
            set
            {
                if (SendEnum.SendBy == sendSide)
                    inci[0] = value;
                else
                    inci[1] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public InciChain()
        {
            inci = new Inci[2] { new Inci(), new Inci() };
        }
    }

    /// <summary>
    ///Represente un Ncs et les instructions associées (côté SendBy et côté SendTo) 
    /// </summary>
    public class NcsInciChain
    {
        #region members
        /// <summary>
        /// Représente le canal de communication (NCS)
        /// </summary>
        public NotificationConfirmationSystem ncs;
        /// <summary>
        /// Représente la chaîne INCI (1 côté émission et 1 autre côté réception)
        /// </summary>
        public InciChain inciChain;
        #endregion

        #region constructor
        public NcsInciChain(NotificationConfirmationSystem pNcs, InciChain pInciChain)
        {
            ncs = pNcs;
            inciChain = pInciChain;
        }
        #endregion
    }

    /// <summary>
    /// Collection of Inci
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("INCIS", Namespace = "", IsNullable = true)]
    public class Incis
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("INCI", Order = 1)]
        public Inci[] inci;
        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return ArrFunc.Count(inci);
            }
        }

        #endregion

        #region Indexors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIndex"></param>
        /// <returns></returns>
        public Inci this[int pIndex]
        {
            get
            {
                return inci[pIndex];
            }
        }
        #endregion Indexors

        #region constructor
        public Incis()
        {
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// Chargement des instructions en reception pour le couple [Messaga,NCS]
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCnfMessageNcs">Représente le couple [Messaga,NCS]</param>
        /// <param name="pIdAContactOffice">Représente le Contact Office</param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pTradeStBusiness"></param>
        /// <param name="pScanDataDtEnabled"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void LoadInciCollectionReceive(string pCs, CnfMessageNcs pCnfMessageNcs,
            int pIdAContactOffice, int pIdATrader, int pIdB, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pTradeStBusiness, string pTradeStMatch, string pTradeStCheck, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            // FI 20170913 [23417] paramètre pContract
            LoadInciCollection(pCs, pCnfMessageNcs, SendReceiveEnum.Receive,
                pIdAContactOffice, pIdATrader, pIdB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                pTradeStBusiness, pTradeStMatch, pTradeStCheck, null, false, pScanDataDtEnabled);
        }
        /// <summary>
        /// Chargement des instructions en émission pour le couple [Messaga,NCS]
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCnfMessageNcs">Représente le couple [Messaga,NCS]</param>
        /// <param name="pIdAContactOffice">Représente le Contact Office</param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pTradeStBusiness"></param>
        /// <param name="pRecipientType">Type de Contrepartie destinataire</param>
        /// <param name="pIsSendDefault">True pour cosnidérer les instruction par défaut</param>
        /// <param name="pScanDataDtEnabled"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// EG 20160404 Migration vs2013 
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void LoadInciCollectionSend(string pCs, CnfMessageNcs pCnfMessageNcs,
            int pIdAContactOffice, int pIdATrader, int pIdB, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pTradeStBusiness, string pTradeStMatch, string pTradeStCheck, NotificationSendToClass pRecipientType, bool pIsSendDefault, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            // FI 20170913 [23417] passage du paramètre pContract
            LoadInciCollection(pCs, pCnfMessageNcs, SendReceiveEnum.Send,
                pIdAContactOffice, pIdATrader, pIdB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                pTradeStBusiness, pTradeStMatch, pTradeStCheck, pRecipientType, pIsSendDefault, pScanDataDtEnabled);
        }
        /// <summary>
        /// Chargement des instructions 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCnfMessageNcs"></param>
        /// <param name="pSideEnum">Réception/Emission</param>
        /// <param name="pIdAContactOffice"></param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pContract"></param>
        /// <param name="pIDDC"></param>
        /// <param name="pTradeStBusiness"></param>
        /// <param name="pTradeStMatch"></param>
        /// <param name="pTradeStCheck"></param>
        /// <param name="pRecipientType">Type de Contrepartie destinataire
        /// <para>Ne doit être renseigné que si Recherche des instruction côté Emission</para>
        /// </param>
        /// <param name="pIsSendDefault">True pour cosnidérer les instruction par défaut
        ///  <para>Ne doit être renseigné que si Recherche des instruction côté Emission</para>
        /// </param>
        /// <param name="pScanDataDtEnabled"></param>
        /// 20140710 PL [TRIM 20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// EG 20160404 Migration vs2013 
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        private void LoadInciCollection(string pCs, CnfMessageNcs pCnfMessageNcs, SendReceiveEnum pSideEnum,
            int pIdAContactOffice, int pIdATrader, int pIdB, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pTradeStBusiness, string pTradeStMatch, string pTradeStCheck,
            Nullable<NotificationSendToClass> pRecipientType, bool pIsSendDefault, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {

            // RD 20100902 [] 
            // Pour qu'un critère d'une instruction match avec un le même critère du message:
            //  1- Soit il y'a égalité entre les deux valeurs
            //  2- Soit le critère sur l'instruction est NULL
            // En résumé, si la valeur d'un crière est spécifiée sur une instruction, il faudrait avoir la même valeur sur le critère du message
            if (false == pCnfMessageNcs.IsSqlTableLoaded)
                pCnfMessageNcs.LoadSqlTable(pCs);

            DataParameters sqlParameters = new DataParameters();
            
            //MESSAGE
            sqlParameters.Add(new DataParameter(pCs, "IDCNFMESSAGE", DbType.Int32), pCnfMessageNcs.idCnfMessage);
            sqlParameters.Add(new DataParameter(pCs, "STEPLIFE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pCnfMessageNcs.SqlCnfMessage.StepLifeEnum.ToString());
            sqlParameters.Add(new DataParameter(pCs, "MSGTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pCnfMessageNcs.SqlCnfMessage.MsgType);
            sqlParameters.Add(new DataParameter(pCs, "CNFTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pCnfMessageNcs.SqlCnfMessage.CnfType);
            
            //NCS
            sqlParameters.Add(new DataParameter(pCs, "IDA_NCS", DbType.Int32), pCnfMessageNcs.SqlNcs.Id);
            
            //STBUSINESS
            // RD 20100823 [] pour gérer le critère Statut Business sur les Inci
            if (StrFunc.IsFilled(pTradeStBusiness))
            {
                sqlParameters.Add(new DataParameter(pCs, "IDSTBUSINESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pTradeStBusiness);
            }
            
            //20140710 PL [TRIM 20179] 
            if (StrFunc.IsFilled(pTradeStMatch))
            {
                sqlParameters.Add(new DataParameter(pCs, "IDSTMATCH", DbType.AnsiString, SQLCst.UT_STATUS_LEN * 10), pTradeStMatch);
            }
            if (StrFunc.IsFilled(pTradeStCheck))
            {
                sqlParameters.Add(new DataParameter(pCs, "IDSTCHECK", DbType.AnsiString, SQLCst.UT_STATUS_LEN * 10), pTradeStCheck);
            }
            
            sqlParameters.Add(new DataParameter(pCs, "SIDE", DbType.AnsiString), pSideEnum.ToString());
            sqlParameters.Add(new DataParameter(pCs, "IDA_CONTACTOFFICE", DbType.Int32), pIdAContactOffice);
            sqlParameters.Add(new DataParameter(pCs, "IDA_TRADER", DbType.Int32), pIdATrader);
            sqlParameters.Add(new DataParameter(pCs, "IDB", DbType.Int32), pIdB);
            sqlParameters.Add(new DataParameter(pCs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), pIdC);
            
            if (SendReceiveEnum.Send == pSideEnum)
            {
                sqlParameters.Add(new DataParameter(pCs, "RECIPIENTTYPE", DbType.AnsiString));
                sqlParameters["RECIPIENTTYPE"].Value = pRecipientType.ToString();
                if (pIsSendDefault)
                {
                    // EG 20160404 Migration vs2013 
                    //if (NotificationRecipientTypeEnum.CLIENT == pRecipientType)
                    //    sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_CLIENT", DbType.Boolean), true);
                    //else if (NotificationRecipientTypeEnum.ENTITY == pRecipientType)
                    //    sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_ENTITY", DbType.Boolean), true);
                    //else if (NotificationRecipientTypeEnum.EXTERNAL_COUNTERPARTY == pRecipientType)
                    //    sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_EXTCTR", DbType.Boolean), true);
                    if (NotificationSendToClass.Client == pRecipientType)
                        sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_CLIENT", DbType.Boolean), true);
                    else if (NotificationSendToClass.Entity == pRecipientType)
                        sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_ENTITY", DbType.Boolean), true);
                    else if (NotificationSendToClass.External == pRecipientType)
                        sqlParameters.Add(new DataParameter(pCs, "SENDDEFAULT_EXTCTR", DbType.Boolean), true);
                }
            }
            
            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("(inci.IDA_CONTACTOFFICE=@IDA_CONTACTOFFICE)");
            sqlWhere.Append("(inci.IDCNFMESSAGE=@IDCNFMESSAGE or inci.IDCNFMESSAGE is null)");
            sqlWhere.Append("(inci.STEPLIFE=@STEPLIFE or inci.STEPLIFE is null)");
            sqlWhere.Append("(inci.MSGTYPE=@MSGTYPE or inci.MSGTYPE is null)");
            sqlWhere.Append("(inci.IDA_NCS=@IDA_NCS or inci.IDA_NCS is null)");
            sqlWhere.Append("(inci.SIDE=@SIDE)");
            sqlWhere.Append("(inci.IDA_TRADER=@IDA_TRADER or inci.IDA_TRADER is null)");
            sqlWhere.Append("(inci.IDB=@IDB or inci.IDB is null)");
            sqlWhere.Append("(inci.IDC=@IDC or inci.IDC is null)");
            if (sqlParameters.Contains("IDSTBUSINESS"))
                sqlWhere.Append("(inci.IDSTBUSINESS=@IDSTBUSINESS or inci.IDSTBUSINESS is null)");
            else
                sqlWhere.Append("inci.IDSTBUSINESS is null");
            
            //20140710 PL [TRIM 20179] - Rq.: @IDSTMATCH contient la liste des statuts actifs encadrés d'accolades (ex. {UNMATCH}{XXXXX})
            if (sqlParameters.Contains("IDSTMATCH"))
                sqlWhere.Append("((@IDSTMATCH like '%{'+inci.IDSTMATCH+'}%') or (inci.IDSTMATCH is null))");
            else
                sqlWhere.Append("inci.IDSTMATCH is null");
            if (sqlParameters.Contains("IDSTCHECK"))
                sqlWhere.Append("((@IDSTCHECK like '%{'+inci.IDSTCHECK+'}%') or (inci.IDSTCHECK is null))");
            else
                sqlWhere.Append("inci.IDSTCHECK is null");
            
            if (sqlParameters.Contains("CNFTYPE"))
                sqlWhere.Append("(inci.CNFTYPE=@CNFTYPE or inci.CNFTYPE is null)");
            else
                sqlWhere.Append("inci.CNFTYPE is null");

            // EG 20160404 Migration vs2013 
            //if (sqlParameters.Contains("RECIPIENTTYPE"))
            //    sqlWhere.Append("(inci.RECIPIENTTYPE=@RECIPIENTTYPE or inci.RECIPIENTTYPE = 'ALL')");
            if (sqlParameters.Contains("RECIPIENTTYPE"))
                sqlWhere.Append("(inci.RECIPIENTTYPE=@RECIPIENTTYPE or inci.RECIPIENTTYPE is null)");
            if (sqlParameters.Contains("SENDDEFAULT_CLIENT"))
                sqlWhere.Append("(inci.SENDDEFAULT_CLIENT=@SENDDEFAULT_CLIENT)");
            if (sqlParameters.Contains("SENDDEFAULT_ENTITY"))
                sqlWhere.Append("(inci.SENDDEFAULT_ENTITY=@SENDDEFAULT_ENTITY)");
            if (sqlParameters.Contains("SENDDEFAULT_EXTCTR"))
                sqlWhere.Append("(inci.SENDDEFAULT_EXTCTR=@SENDDEFAULT_EXTCTR)");

            //FI 20100325 appel à SQLInstrCriteria à la place de InstrTools.GetSQLCriteriaInstr2
            //IDI
            SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCs, null, pIdI, false, pScanDataDtEnabled);
            string instrWhere = sqlInstrCriteria.GetSQLRestriction2("inci", RoleGInstr.CNF);
            sqlWhere.Append(instrWhere, true);
            
            //IDI_UNDERLYER
            sqlInstrCriteria = new SQLInstrCriteria(pCs, null, pIdIUnderlyer, true, pScanDataDtEnabled);
            instrWhere = sqlInstrCriteria.GetSQLRestriction2("inci", RoleGInstr.CNF);
            sqlWhere.Append(instrWhere, true);
            

            /* FI 20170913 [23417] Utilisation de SQLContractCriteria
            string dcWhere = string.Empty;
            SQLDerivativeContractCriteria sqlDC = new SQLDerivativeContractCriteria(pCs, pIDDC, pIdM, pScanDataDtEnabled);
            dcWhere = sqlDC.GetSQLRestriction(pCs, "inci", RoleContractRestrict.CNF);
            sqlWhere.Append(dcWhere, true);
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "inci"));
             */

            // FI 20170913 [23417]  Utilisation de SQLContractCriteria
            SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCs, null, pContract, pIdM, pScanDataDtEnabled);
            string contractWhere =  sqlContractCriteria.GetSQLRestriction( "inci", RoleContractRestrict.CNF);
            sqlWhere.Append(contractWhere, true);
            
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "inci"));
            
            //Select INCI
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "inci.IDINCI,inci.IDA_CONTACTOFFICE,inci.PRIORITYRANK," + Cst.CrLf;
            sqlSelect += "inci.SIDE,inci.RECIPIENTTYPE,inci.IDSTBUSINESS," + Cst.CrLf;
            //20140710 PL [TRIM 20179]
            sqlSelect += "inci.IDSTMATCH,inci.IDSTCHECK," + Cst.CrLf;
            sqlSelect += "inci.IDCNFMESSAGE,inci.CNFTYPE,inci.MSGTYPE," + Cst.CrLf;
            sqlSelect += "inci.STEPLIFE," + Cst.CrLf;
            sqlSelect += "inci.IDA_NCS," + Cst.CrLf;
            sqlSelect += "inci.IDA_TRADER,inci.IDB," + Cst.CrLf;
            sqlSelect += "inci.ISTOSEND,inci.SENDDEFAULT_CLIENT,inci.SENDDEFAULT_EXTCTR,inci.ISTORECEIVE," + Cst.CrLf;
            sqlSelect += "inci.IDC, inci.GPRODUCT, inci.IDINSTR, inci.IDINSTR_UNL," + Cst.CrLf;
            sqlSelect += DataHelper.SQLIsNullChar(pCs, "inci.TYPECONTRACT", TypeContractEnum.None.ToString(), "TYPECONTRACT") + ", inci.IDCONTRACT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INCI.ToString() + " inci" + Cst.CrLf;
            sqlSelect += sqlWhere.ToString();
            
            //Select INCIITEM
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "item"));
            StrBuilder sqlSelect2 = new StrBuilder(SQLCst.SELECT);
            sqlSelect2 += "item.IDINCI,item.IDA,item.ACTORTYPE,item.ISTO,item.ISCC,item.ISBCC,item.ADDRESSIDENT" + Cst.CrLf;
            sqlSelect2 += SQLCst.FROM_DBO + Cst.OTCml_TBL.INCIITEM.ToString() + " item" + Cst.CrLf;
            sqlSelect2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INCI.ToString() + " inci on inci.IDINCI=item.IDINCI" + Cst.CrLf;
            sqlSelect2 += sqlWhere.ToString();
            
            string sqlSelectGlobal = sqlSelect.ToString() + SQLCst.SEPARATOR_MULTISELECT + sqlSelect2.ToString();
            QueryParameters queryParameters = new QueryParameters(pCs, sqlSelectGlobal, sqlParameters);
            
            DataSet dsResult = DataHelper.ExecuteDataset(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            dsResult.DataSetName = "INCIS";
            
            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "INCI";
            
            dtTable = dsResult.Tables[1];
            dtTable.TableName = "INCIITEM";

            DataRelation rel = new DataRelation("INCIITEM",
                                                dsResult.Tables["INCI"].Columns["IDINCI"],
                                                dsResult.Tables["INCIITEM"].Columns["IDINCI"], false)
            {
                Nested = true
            };
            dsResult.Relations.Add(rel);
            //
            string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();
            //
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(Incis), dsSerializerResult);
            Incis inciCol = (Incis)CacheSerializer.Deserialize(serializeInfo);
            if (null != inciCol && (0 < ((Incis)inciCol).Count))
                inci = inciCol.inci;

        }
        #endregion
    }

    /// <summary>
    /// Représente 1 instruction de confirmation [avec les destinataires associés (=pInciItem)]
    /// </summary>
    public class Inci
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDINCI", Order = 1)]
        public int idInci;
        //
        [System.Xml.Serialization.XmlElementAttribute("IDA_CONTACTOFFICE", Order = 2)]
        public int idA_contactOffice;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priorityRankSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIORITYRANK", Order = 3)]
        public int priorityRank;
        //
        [System.Xml.Serialization.XmlElementAttribute("SIDE", Order = 4)]
        public SendReceiveEnum sideEnum;
        // EG 20160404 Migration vs2013 
        [System.Xml.Serialization.XmlElementAttribute("RECIPIENTTYPE", Order = 5)]
        //public NotificationRecipientTypeEnum recipientType;
        public NotificationSendToClass recipientType;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStBusinessSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTBUSINESS", Order = 6)]
        public string idStBusiness;
        //
        //20140710 PL [TRIM 20179]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStMatchSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTMATCH", Order = 7)]
        public string idStMatch;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStCheckSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTCHECK", Order = 8)]
        public string idStCheck;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCnfMessageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDCNFMESSAGE", Order = 9)]
        public int idCnfMessage;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool confirmationTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CNFTYPE", Order = 10)]
        public string confirmationType;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool msgTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("MSGTYPE", Order = 11)]
        public string msgType;
        //
        [System.Xml.Serialization.XmlElementAttribute("STEPLIFE", Order = 12)]
        public Nullable<NotificationStepLifeEnum> stepLifeEnum;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_ncsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_NCS", Order = 13)]
        public int idA_ncs;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_traderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_TRADER", Order = 14)]
        public int idA_trader;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB", Order = 15)]
        public int idB;
        //
        [System.Xml.Serialization.XmlElementAttribute("ISTOSEND", Order = 16)]
        public bool isToSend;
        //
        [System.Xml.Serialization.XmlElementAttribute("SENDDEFAULT_CLIENT", Order = 17)]
        public bool isToSendDefaultClient;
        //
        [System.Xml.Serialization.XmlElementAttribute("SENDDEFAULT_EXTCTR", Order = 18)]
        public bool isToSendDefaultExternalCounterparty;
        //
        [System.Xml.Serialization.XmlElementAttribute("ISTORECEIVE", Order = 19)]
        public bool isToReceive;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC", Order = 20)]
        public string idC;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gProductSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GPRODUCT", Order = 21)]
        public string gProduct;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idInstrSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDINSTR", Order = 22)]
        public int idInstr;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idInstrUnlSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDINSTR_UNL", Order = 23)]
        public string idInstrUnl;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeContractSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TYPECONTRACT", Order = 24)]
        public string typeContract;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idContractSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDCONTRACT", Order = 25)]
        public int idContract;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool inciItemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("INCIITEM", Order = 26)]
        public InciItem[] inciItem;
        #endregion Members

        #region property
        /// <summary>
        /// 
        /// </summary>
        public bool IsInfoInstrSpecified
        {
            get
            {
                return (idInstrSpecified || idInstrUnlSpecified || gProductSpecified);
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        public void AddItemInInciItem(int pIda)
        {
            if (pIda > 0)
            {
                //
                ReflectionTools.AddItemInArray(this, "inciItem", 0);
                //
                if (ArrFunc.IsFilled(inciItem))
                {
                    InciItem item = inciItem[ArrFunc.Count(inciItem) - 1];
                    //
                    item.idA = pIda;
                    item.idASpecified = true;
                    item.isTo = true;
                    item.isCC = false;
                    item.isBCC = false;
                    item.addressIdent = string.Empty;
                }
                //
                inciItemSpecified = ArrFunc.IsFilled(inciItem);
                //
            }
        }
        #endregion
    }

    /// <summary>
    /// Represente des destinataires
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("INCIITEMS", Namespace = "", IsNullable = true)]
    public class InciItems
    {
        #region Members
        /// <summary>
        /// Represente un array de destinataires
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("INCIITEM", Order = 1)]
        public InciItem[] inciItem;
        #endregion

        #region constructor
        public InciItems()
        {
        }
        #endregion

        #region Methodes

        /// <summary>
        /// Charge les destinataires associés à une instruction de confirmation (alimente le membre pInciItem)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdInci"></param>
        /// <param name="pScanDataDtEnabledEnum"></param>
        public void Initialize(string pCs, int pIdInci, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
        {
            InciItems inciItems = LoadInciItems(pCs, pIdInci, pScanDataDtEnabledEnum);
            this.inciItem = inciItems.inciItem;
        }
        /// <summary>
        /// Charge les destinataires associés à une facture (alimente le membre pInciItem)
        /// </summary>
        public void Initialize(string pCs, IInvoicingScope pScope, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
        {
            InciItems inciItems = LoadInciItems(pCs, pScope, pScanDataDtEnabledEnum);
            this.inciItem = inciItems.inciItem;
        }

        /// <summary>
        /// Retourne les destinataires associé à une instruction de confirmation
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdInci"></param>
        /// <param name="pScanDataDtEnabled"></param>
        /// <returns></returns>
        public static InciItems LoadInciItems(string pCs, int pIdInci, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDINCI", DbType.Int32), pIdInci);
            //
            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("item.IDINCI=@IDINCI");
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "item"));

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "item.IDINCI,item.IDA,item.ACTORTYPE," + Cst.CrLf;
            sqlSelect += "item.ISTO,item.ISCC,item.ISBCC,item.ADDRESSIDENT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INCIITEM.ToString() + " item" + Cst.CrLf;
            sqlSelect += sqlWhere.ToString();

            QueryParameters queryParameters = new QueryParameters(pCs, sqlSelect.ToString(), parameters);
            InciItems ret = LoadFromQuery(queryParameters);
            return ret;

        }

        /// <summary>
        /// Retourne le destinataire d'une facture  
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pInvoice"></param>
        /// <returns></returns>
        public static InciItems LoadInciItems(string pCs, IInvoicingScope pScope, SQL_Table.ScanDataDtEnabledEnum pScanDataDtEnabled)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDINVOICINGRULES", DbType.Int32), pScope.OTCmlId);

            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("item.IDINVOICINGRULES=@IDINVOICINGRULES");
            if (SQL_Table.ScanDataDtEnabledEnum.Yes == pScanDataDtEnabled)
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCs, "item"));

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "item.IDINVOICINGRULES as IDINCI,item.IDA_INVOICED as IDA,null as ACTORTYPE," + Cst.CrLf;
            sqlSelect += "1 as ISTO,0 as ISCC,0 as ISBCC,item.ADDRESSIDENT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INVOICINGRULES.ToString() + " item" + Cst.CrLf;
            sqlSelect += sqlWhere.ToString();

            QueryParameters queryParameters = new QueryParameters(pCs, sqlSelect.ToString(), parameters);
            InciItems ret = LoadFromQuery(queryParameters);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInci"></param>
        /// <param name="pIsNotAddExistingActor"></param>
        public void Add(InciItem pInci, bool pIsNotAddExistingActor)
        {
            Add(new InciItem[] { pInci }, pIsNotAddExistingActor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInci"></param>
        /// <param name="pIsNotAddExistingActor"></param>
        public void Add(InciItem[] pInci, bool pIsNotAddExistingActor)
        {
            ArrayList al = GetList();
            //
            for (int i = 0; i < ArrFunc.Count(pInci); i++)
            {
                if (false == this.Contains(pInci[i]) && pIsNotAddExistingActor)
                    al.Add(pInci[i]);
            }
            //
            inciItem = (InciItem[])al.ToArray(typeof(InciItem));
        }

        /// <summary>
        /// Retourne true si l'acteur destinataire {pInciItem} existe dans la liste des destinataires (element pInciItem)   
        /// </summary>
        /// <param name="pInciItem"></param>
        /// <returns></returns>
        public bool Contains(InciItem pInciItem)
        {
            bool ret = false;
            for (int i = 0; i < ArrFunc.Count(inciItem); i++)
            {
                if (0 == inciItem[i].CompareActorTo(pInciItem))
                {
                    ret = true;
                    break;
                }
            }
            return ret;

        }

        /// <summary>
        /// Pour chaque destinataire paramétré, recherche de l'acteur lorsque le paramétrage est de type dynamique 
        /// </summary>
        /// <param name="pCnfChainItem"></param>
        /// <param name="pDataDocumentContainer">Représente un trade (valeur null possible)</param>
        public void SearchIdaActorFromActorType(ConfirmationChainItem pCnfChainItem, DataDocumentContainer pDataDocumentContainer)
        {
            for (int i = 0; i < ArrFunc.Count(inciItem); i++)
                inciItem[i].SearchIdaActorFromActorType(pCnfChainItem, pDataDocumentContainer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ArrayList GetList()
        {
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(inciItem); i++)
                al.Add(inciItem[i]);
            //
            return al;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueryParameters"></param>
        /// <returns></returns>
        private static InciItems LoadFromQuery(QueryParameters pQueryParameters)
        {
            DataSet dsResult = DataHelper.ExecuteDataset(pQueryParameters.Cs, CommandType.Text, pQueryParameters.Query, pQueryParameters.Parameters.GetArrayDbParameter());
            dsResult.DataSetName = "INCIITEMS";

            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "INCIITEM";

            string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(InciItems), dsSerializerResult);
            InciItems ret = (InciItems)CacheSerializer.Deserialize(serializeInfo);
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Represente un destinataire
    /// </summary>
    public class InciItem
    {

        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDINCI", Order = 1)]
        public int idInci;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idASpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA", Order = 2)]
        public int idA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ACTORTYPE", Order = 3)]
        public string actorType;
        //
        [System.Xml.Serialization.XmlElementAttribute("ISTO", Order = 4)]
        public bool isTo;
        //
        [System.Xml.Serialization.XmlElementAttribute("ISCC", Order = 5)]
        public bool isCC;
        //
        [System.Xml.Serialization.XmlElementAttribute("ISBCC", Order = 6)]
        public bool isBCC;
        //
        [System.Xml.Serialization.XmlElementAttribute("ADDRESSIDENT", Order = 7)]
        public string addressIdent;
        #endregion Members

        #region Accessor
        /// <summary>
        /// 
        /// </summary>
        public ActorTypeEnum ActorTypeEnum
        {
            get
            {
                ActorTypeEnum ret = ActorTypeEnum.NA;
                if (StrFunc.IsFilled(this.actorType))
                    ret = (ActorTypeEnum)System.Enum.Parse(typeof(ActorTypeEnum), actorType, true);
                return ret;
            }
        }
        #endregion Accessor

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public InciItem()
        {
        }
        /// <summary>
        /// Constructeur où l'acteur {pIdA} est défini comme destinataire principale
        /// </summary>
        /// <param name="pIdInci"></param>
        /// <param name="pIdA"></param>
        public InciItem(int pIdInci, int pIdA)
        {
            idInci = pIdInci;
            idA = pIdA;
            isTo = true;
            addressIdent = string.Empty;
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// Alimente l'acteur (membre idA) en fonction du paramétrage de ConfirmationChainItem
        /// </summary>
        /// <param name="pCfChainItem"></param>
        /// <param name="pDataDocument">Représente un trade (valeur null possible)</param>
        public void SearchIdaActorFromActorType(ConfirmationChainItem pCfChainItem, DataDocumentContainer pDataDocument)
        {
            if ((false == idASpecified) && (ActorTypeEnum.NA != ActorTypeEnum))
            {
                switch (ActorTypeEnum)
                {
                    case ActorTypeEnum.actor:
                        //this.idA = this.idA; 
                        break;
                    case ActorTypeEnum.party:
                        this.idA = pCfChainItem.IdActor;
                        break;
                    case ActorTypeEnum.bookOwner:
                        if (pCfChainItem.IdBook > 0)
                            this.idA = pCfChainItem.sqlBook.IdA;
                        break;
                    case ActorTypeEnum.trader:
                        // RD 20100823 [] Dans le cas des messages Multi-Trades et Multi-Parties, on considère le premier Trade                            
                        // FI 20120403 [] Dans la messagerie Multi-Parties, multi-Trades , pDataDocument n'est pas renseigné
                        // Dans ce dernier cas si le destinataire est matérialisé par le mot clef Trader, Spheres® ne cherche pas le destinataire
                        if (null != pDataDocument)
                        {
                            IParty party = pDataDocument.GetParty(pCfChainItem.sqlActor.Id.ToString(), PartyInfoEnum.OTCmlId);
                            if (null != party)
                            {
                                ITrader[] trader = pDataDocument.GetPartyTrader(party.Id);
                                if (ArrFunc.IsFilled(trader))
                                    this.idA = trader[0].OTCmlId;
                            }
                        }
                        break;
                    case ActorTypeEnum.NA:
                        throw new NotImplementedException("ActorTypeEnum not NotImplemented");
                }
                this.idASpecified = (this.idA > 0);
            }
        }

        /// <summary>
        /// Retourne 0 si le destinataire {pObj} est identique au destinataire présent
        /// <para>Compare les attributs (idA,isTo,isBCC,isCC)</para>
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        /// FI 20150206 [20776] Modify
        public int CompareActorTo(InciItem pObj)
        {
            int ret = 0;

            if (ret != -1)
            {
                if (pObj.idA != this.idA)
                    ret = -1;
            }
            // FI 20150206 [20776] Ajout contrôle sur addressIdent
            if (ret != -1)
            {
                if (pObj.addressIdent != this.addressIdent)
                    ret = -1;
            }
            if (ret != -1)
            {
                if (pObj.isTo != this.isTo)
                    ret = -1;
            }
            if (ret != -1)
            {
                if (pObj.isBCC != this.isBCC)
                    ret = -1;
            }
            if (ret != -1)
            {
                if (pObj.isCC != this.isCC)
                    ret = -1;
            }
            return ret;
        }
        #endregion
    }

    /// <summary> 
    /// Représente une liste de CnfMessageIncis
    /// </summary>
    public class CnfMessageNcsInciss
    {
        #region Members
        /// <summary>
        /// Un message, un Ncs, et une liste de inci
        /// </summary>
        private CnfMessageNcsIncis2[] _cnfMessageNcsIncis;
        #endregion

        #region Accessor
        /// <summary>
        /// Obtient la liste sous forme de array
        /// </summary>
        public CnfMessageNcsIncis2[] CnfMessageNcsIncis
        {
            get { return _cnfMessageNcsIncis; }
        }
        #endregion

        #region constructor
        public CnfMessageNcsInciss()
        {
        }
        #endregion

        #region Method
        /// <summary>
        /// Chargement des instructions qui matchent pour un message donné [+ paramètres Contact office, Trader, Sens, etc...]
        /// <para>
        /// Chargement d'une liste d'instruction par NCS compatible avec le message
        /// </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pSideEnum"></param>
        /// <param name="pIdAContactOffice"></param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pStBusiness"></param>
        /// <param name="pStMatch"></param>
        /// <param name="pStCheck"></param>
        /// <param name="pRecipientType"></param>
        /// <param name="pIsSendDefault"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// EG 20160404 Migration vs2013  
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void Load(string pCs, CnfMessage pCnfMessage, SendReceiveEnum pSideEnum, int pIdAContactOffice,
            int pIdATrader, int pIdB, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pStBusiness, string pStMatch, string pStCheck,
            Nullable<NotificationSendToClass> pRecipientType, bool pIsSendDefault)
        {
            int[] idNcs = pCnfMessage.GetIdNcsCompatible();

            // FI 20170913 [23417] paramètre pContract
            Load(pCs, pCnfMessage, pSideEnum, pIdAContactOffice, pIdATrader, pIdB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                pStBusiness, pStMatch, pStCheck, idNcs, pRecipientType, pIsSendDefault);
        }

                
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pSideEnum"></param>
        /// <param name="pIdAContactOffice"></param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pStBusiness"></param>
        /// <param name="pStMatch"></param>
        /// <param name="pStCheck"></param>
        /// <param name="pIdNcs"></param>
        /// <param name="pRecipientType"></param>
        /// <param name="pIsSendDefault"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// EG 20160404 Migration vs2013  
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void Load(string pCs, CnfMessage pCnfMessage, SendReceiveEnum pSideEnum, int pIdAContactOffice,
            int pIdATrader, int pIdB, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pStBusiness, string pStMatch, string pStCheck,
            int[] pIdNcs, Nullable<NotificationSendToClass> pRecipientType, bool pIsSendDefault)
        {

            //Pour chaque NCS compatible, selection des instructions compatibles 
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(pIdNcs); i++)
            {
                CnfMessageNcs cnfMessageNcs = new CnfMessageNcs(pCnfMessage.idCnfMessage, pIdNcs[i]);
                //FI 20120323 use CnfMessageNcsIncis2
                //CnfMessageNcsIncis cnfMessageNcsIncis = new CnfMessageNcsIncis(cnfMessageNcs, pSideEnum, pIdATrader, pIdB, pRecipientType);
                CnfMessageNcsIncis2 cnfMessageNcsIncis = new CnfMessageNcsIncis2(cnfMessageNcs, pSideEnum, pRecipientType);

                // FI 20170913 [23417] paramètre pContract
                //20140710 PL [TRIM 20179]
                cnfMessageNcsIncis.LoadIncis(pCs, pIdAContactOffice, pIdB, pIdATrader, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                    pStBusiness, pStMatch, pStCheck, pIsSendDefault);
                al.Add(cnfMessageNcsIncis);
            }

            if (ArrFunc.IsFilled(al))
                _cnfMessageNcsIncis = (CnfMessageNcsIncis2[])al.ToArray(typeof(CnfMessageNcsIncis2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CnfMessageNcsIncisBest GetCnfMessageNcsIncisBest()
        {
            CnfMessageNcsIncisBest ret = new CnfMessageNcsIncisBest();
            //
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(_cnfMessageNcsIncis); i++)
            {
                if (_cnfMessageNcsIncis[i].Incis.Count > 0)
                {
                    CnfMessageNcsInci cnfMessageNcsInci = new CnfMessageNcsInci(_cnfMessageNcsIncis[i].CnfMessageNcs, _cnfMessageNcsIncis[i].Incis[0]);
                    al.Add(cnfMessageNcsInci);
                }
            }
            //
            if (ArrFunc.IsFilled(al))
            {
                ret.CnfMessageInci = (CnfMessageNcsInci[])al.ToArray(typeof(CnfMessageNcsInci));
                ret.Sort();
            }
            //
            return ret;
        }

        #endregion
    }

    /// <summary>
    /// Represente un couple (CNFMESSAGE,NCS), et les INCIs qui matchent en fonction d'éléments supplémentaires [le sens (en emission ou en reception),  le type de destinatire, le trader etc....]
    /// Les INCIs qui matchent sont chargés puis ordonnées par la méthode LoadIncis
    /// </summary>
    public class CnfMessageNcsIncis : IComparer
    {
        #region Members
        private readonly CnfMessageNcs _cnfMessageNcs;
        private readonly SendReceiveEnum _sendReceiveEnum;
        private readonly NotificationSendToClass _recipientType;  //Valable uniquement lorsque _sendReceiveEnum = Send = Type de destinataire
        private readonly int _idATrader;
        private readonly int _idB;
        //
        private Incis _incis;
        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public Incis Incis
        {
            get { return _incis; }
            set { _incis = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CnfMessageNcs CnfMessageNcs
        {
            get { return _cnfMessageNcs; }
        }
        /// <summary>
        /// 
        /// </summary>
        public SendReceiveEnum SendReceiveEnum
        {
            get
            {
                return _sendReceiveEnum;
            }
        }
        #endregion

        #region constructor
        // EG 20160404 Migration vs2013  
        //public CnfMessageNcsIncis(CnfMessageNcs pCnfMessageNcs, SendReceiveEnum pSideEnum, int pIdATrader, int pIdB, NotificationRecipientTypeEnum pRecipientType)
        public CnfMessageNcsIncis(CnfMessageNcs pCnfMessageNcs, SendReceiveEnum pSideEnum, int pIdATrader, int pIdB, NotificationSendToClass pRecipientType)
        {
            _cnfMessageNcs = pCnfMessageNcs;
            _sendReceiveEnum = pSideEnum;
            _idATrader = pIdATrader;
            _idB = pIdB;
            //
            _recipientType = pRecipientType;
            //
            _incis = new Incis();

        }
        #endregion
        
        /// <summary>
        /// Chargement et tri des INCIs qui matchent pour le couple (CNFMESSAGE,NCS)
        /// Les INCIs qui matchent sont fonction du contexte (Contact office, Sens (Emission, Reception), Instrument...) 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdAContactOffice"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pStBusiness"></param>
        /// <param name="pStMatch"></param>
        /// <param name="pStCheck"></param>
        /// <param name="pIsDefault"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] add parameter pIdM
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void LoadIncis(string pCs, int pIdAContactOffice, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, Pair<Cst.ContractCategory, int> pContract,
            string pStBusiness, string pStMatch, string pStCheck, bool pIsDefault)
        {
            _cnfMessageNcs.LoadSqlTable(pCs);

            //Chargement des INCIs
            _incis = new Incis();
            if (_sendReceiveEnum == SendReceiveEnum.Receive)
            {
                // FI 20170913 [23417] passage du paramètre pContract
                _incis.LoadInciCollectionReceive(pCs, _cnfMessageNcs, pIdAContactOffice, _idATrader, _idB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                    pStBusiness, pStMatch, pStCheck, SQL_Table.ScanDataDtEnabledEnum.Yes);
            }
            else
            {
                // FI 20170913 [23417] passage du paramètre pContract
                _incis.LoadInciCollectionSend(pCs, _cnfMessageNcs, pIdAContactOffice, _idATrader, _idB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                    pStBusiness, pStMatch, pStCheck, _recipientType, pIsDefault, SQL_Table.ScanDataDtEnabledEnum.Yes);
            }

            //Tri
            Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Sort()
        {
            bool isOk = (0 < _incis.Count);
            if (isOk)
                Array.Sort(Incis.inci, this);
            return isOk;
        }

        #region IComparer Members
        /// <summary>
        /// Permet de trier les INCIs (les instructions de confirmation) 
        ///
        /// Exemples :
        /// une instruction qui porte sur le message est prioritaire par rapport à une instruction qui porte sur le ncs  
        /// une instruction qui porte et sur le message et sur le ncs, est  prioritaire par rapport à une instruction qui porte sur l'un des deux
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            //
            //si ret = -1, inciX < inciX,  inciX est prioritaire prioritaire
            //si ret =  1, inciY < inciX,  inciY est prioritaire prioritaire
            //

            int ret = 0;

            if ((x is Inci inciX) && (y is Inci inciY) && (null != _cnfMessageNcs))
            {

                #region IDCNFMESSAGE
                int idCnfMessage = _cnfMessageNcs.SqlCnfMessage.Id;
                //
                if (((inciX.idCnfMessageSpecified) && (inciX.idCnfMessage == idCnfMessage))
                    &&
                    ((!inciY.idCnfMessageSpecified) || ((inciY.idCnfMessageSpecified) && inciY.idCnfMessage != idCnfMessage))
                    )
                    ret = -1;
                else if
                    (((inciY.idCnfMessageSpecified) && (inciY.idCnfMessage == idCnfMessage))
                    &&
                    ((!inciX.idCnfMessageSpecified) || ((inciX.idCnfMessageSpecified) && inciX.idCnfMessage != idCnfMessage))
                    )
                    ret = 1;
                #endregion IDST

                #region MSGTYPE
                if (ret == 0)
                {
                    string msgType = _cnfMessageNcs.SqlCnfMessage.MsgType;
                    //
                    if (((inciX.msgTypeSpecified) && (inciX.msgType == msgType))
                        &&
                        ((!inciY.msgTypeSpecified) || ((inciY.msgTypeSpecified) && inciY.msgType != msgType))
                        )
                        ret = -1;
                    else if
                        (((inciY.msgTypeSpecified) && (inciY.msgType == msgType))
                        &&
                        ((!inciX.msgTypeSpecified) || ((inciX.msgTypeSpecified) && inciX.msgType != msgType))
                        )
                        ret = 1;
                }
                #endregion MSGTYPE

                #region IDSTBUSINESS
                if (ret == 0)
                {
                    string idStBusiness = _cnfMessageNcs.SqlCnfMessage.IdStBusiness;
                    //
                    if (((inciX.idStBusinessSpecified) && (inciX.idStBusiness == idStBusiness))
                        &&
                        ((!inciY.idStBusinessSpecified) || ((inciY.idStBusinessSpecified) && inciY.idStBusiness != idStBusiness))
                        )
                        ret = -1;
                    else if
                        (((inciY.idStBusinessSpecified) && (inciY.idStBusiness == idStBusiness))
                        &&
                        ((!inciX.idStBusinessSpecified) || ((inciX.idStBusinessSpecified) && inciX.idStBusiness != idStBusiness))
                        )
                        ret = 1;
                }
                #endregion IDSTBUSINESS

                //20140710 PL [TRIM 20179]
                #region IDSTMATCH
                if (ret == 0)
                {
                    string idStMatch = _cnfMessageNcs.SqlCnfMessage.IdStMatch;
                    //
                    if (((inciX.idStMatchSpecified) && (inciX.idStMatch == idStMatch))
                        &&
                        ((!inciY.idStMatchSpecified) || ((inciY.idStMatchSpecified) && inciY.idStBusiness != idStMatch))
                        )
                        ret = -1;
                    else if
                        (((inciY.idStMatchSpecified) && (inciY.idStMatch == idStMatch))
                        &&
                        ((!inciX.idStMatchSpecified) || ((inciX.idStMatchSpecified) && inciX.idStMatch != idStMatch))
                        )
                        ret = 1;
                }
                #endregion IDSTMATCH

                //20140710 PL [TRIM 20179]
                #region IDSTCHECK
                if (ret == 0)
                {
                    string idStCHECK = _cnfMessageNcs.SqlCnfMessage.IdStCheck;
                    //
                    if (((inciX.idStCheckSpecified) && (inciX.idStCheck == idStCHECK))
                        &&
                        ((!inciY.idStCheckSpecified) || ((inciY.idStCheckSpecified) && inciY.idStCheck != idStCHECK))
                        )
                        ret = -1;
                    else if
                        (((inciY.idStCheckSpecified) && (inciY.idStCheck == idStCHECK))
                        &&
                        ((!inciX.idStCheckSpecified) || ((inciX.idStCheckSpecified) && inciX.idStCheck != idStCHECK))
                        )
                        ret = 1;
                }
                #endregion IDSTCHECK

                #region BOOK
                if (ret == 0)
                {
                    int idB = _idB;
                    //
                    if (((inciX.idBSpecified) && (inciX.idB == idB))
                        &&
                        ((!inciY.idBSpecified) || ((inciY.idBSpecified) && inciY.idB != idB))
                        )
                        ret = -1;
                    else if
                        (((inciY.idBSpecified) && (inciY.idB == idB))
                        &&
                        ((!inciX.idBSpecified) || ((inciX.idBSpecified) && inciX.idB != idB))
                        )
                        ret = 1;
                }
                #endregion BOOK

                #region TRADER
                if (ret == 0)
                {
                    int idATrader = _idATrader;
                    //
                    if (((inciX.idA_traderSpecified) && (inciX.idA_trader == idATrader))
                        &&
                        ((!inciY.idA_traderSpecified) || ((inciY.idA_traderSpecified) && inciY.idA_trader != idATrader))
                        )
                        ret = -1;
                    else if
                        (((inciY.idA_traderSpecified) && (inciY.idA_trader == idATrader))
                        &&
                        ((!inciX.idA_traderSpecified) || ((inciX.idA_traderSpecified) && inciX.idA_trader != idATrader))
                        )
                        ret = 1;
                }
                #endregion TRADER

                #region STEPLIFE
                if (ret == 0)
                {
                    NotificationStepLifeEnum stepLifeEnum = _cnfMessageNcs.SqlCnfMessage.StepLifeEnum;
                    //Priorite aux Issis de même book 
                    if (((inciX.stepLifeEnum.HasValue) && (inciX.stepLifeEnum.Value == stepLifeEnum))
                        &&
                        (((false == inciY.stepLifeEnum.HasValue)) || ((inciY.stepLifeEnum.HasValue) && inciY.stepLifeEnum.Value != stepLifeEnum))
                        )
                        ret = -1;
                    else if (((inciY.stepLifeEnum.HasValue) && (inciY.stepLifeEnum.Value == stepLifeEnum))
                        &&
                        (((false == inciX.stepLifeEnum.HasValue)) || ((inciX.stepLifeEnum.HasValue) && inciX.stepLifeEnum.Value != stepLifeEnum))
                        )
                        ret = 1;

                }
                #endregion STEPLIFE

                #region CNFTYPE
                if (ret == 0)
                {
                    string cnfType = _cnfMessageNcs.SqlCnfMessage.CnfType;

                    if (((inciX.confirmationTypeSpecified) && (inciX.confirmationType == cnfType))
                    &&
                    ((!inciY.confirmationTypeSpecified) || ((inciY.confirmationTypeSpecified) && inciY.confirmationType != cnfType))
                    )
                        ret = -1;
                    else if
                    (((inciY.confirmationTypeSpecified) && (inciY.confirmationType == cnfType))
                    &&
                    ((!inciX.confirmationTypeSpecified) || ((inciX.confirmationTypeSpecified) && inciX.confirmationType != cnfType))
                    )
                        ret = 1;
                }
                #endregion CNFTYPE

                #region IDNCS
                if (ret == 0)
                {
                    int idA_Ncs = _cnfMessageNcs.SqlNcs.Id;
                    //                        
                    if (((inciX.idA_ncsSpecified) && (inciX.idA_ncs == idA_Ncs))
                        &&
                        ((!inciY.idA_ncsSpecified) || ((inciY.idA_ncsSpecified) && inciY.idA_ncs != idA_Ncs))
                        )
                        ret = -1;
                    else if
                        (((inciY.idA_ncsSpecified) && (inciY.idA_ncs == idA_Ncs))
                        &&
                        ((!inciX.idA_ncsSpecified) || ((inciX.idA_ncsSpecified) && inciX.idA_ncs != idA_Ncs))
                        )
                        ret = 1;
                }
                #endregion IDNCS

                #region IDC
                if (ret == 0)
                {
                    if (inciX.idCSpecified && (false == inciY.idCSpecified))
                        ret = -1;
                    else if (inciY.idCSpecified && (false == inciX.idCSpecified))
                        ret = 1;
                }
                #endregion IDC

                #region INSTR_UNL
                if (ret == 0)
                {
                    if (inciX.idInstrUnlSpecified && (false == inciY.idInstrUnlSpecified))
                        ret = -1;
                    else if (inciY.idInstrUnlSpecified && (false == inciX.idInstrUnlSpecified))
                        ret = 1;
                }
                #endregion INSTR_UNL

                #region INSTR
                if (ret == 0)
                {
                    if (inciX.idInstrSpecified && (false == inciY.idInstrSpecified))
                        ret = -1;
                    else if (inciY.idInstrSpecified && (false == inciX.idInstrSpecified))
                        ret = 1;
                }
                #endregion INSTR

                #region GPRODUCT
                if (ret == 0)
                {
                    if (inciX.gProductSpecified && (false == inciY.gProductSpecified))
                        ret = -1;
                    else if (inciY.gProductSpecified && (false == inciX.gProductSpecified))
                        ret = 1;
                }
                #endregion GPRODUCT

                #region TYPECONTRACT / IDCONTRACT
                if (ret == 0)
                {
                    if ((inciX.typeContractSpecified && inciX.idContractSpecified) &&
                        (false == (inciY.typeContractSpecified && inciY.idContractSpecified)))
                        ret = -1;
                    else if ((inciY.typeContractSpecified && inciY.idContractSpecified) &&
                        (false == (inciX.typeContractSpecified && inciX.idContractSpecified)))
                        ret = 1;
                }
                #endregion TYPECONTRACT

                #region SIDE
                if (ret == 0)
                {
                    SendReceiveEnum sendReceiveEnum = _sendReceiveEnum;
                    //Priorite aux Issis de même book 
                    if (((SendReceiveEnum.None != inciX.sideEnum) && (inciX.sideEnum == sendReceiveEnum))
                        &&
                        (((SendReceiveEnum.None == inciY.sideEnum)) || ((SendReceiveEnum.None != inciY.sideEnum) && inciY.sideEnum != sendReceiveEnum))
                        )
                        ret = -1;
                    else if (((SendReceiveEnum.None != inciY.sideEnum) && (inciY.sideEnum == sendReceiveEnum))
                        &&
                        (((SendReceiveEnum.None == inciX.sideEnum)) || ((SendReceiveEnum.None != inciX.sideEnum) && inciX.sideEnum != sendReceiveEnum))
                        )
                        ret = 1;
                }
                #endregion SIDE

                #region PRIORITYRANK
                //En cas d'égalité priorite en fonction de priorityRank
                if (ret == 0)
                {
                    if (inciX.priorityRank < inciY.priorityRank)
                        ret = -1;
                    else if (inciX.priorityRank > inciY.priorityRank)
                        ret = 1;
                }
                #endregion PRIORITYRANK

            }
            else
                throw new ArgumentException("object is not a INCI");

            return ret;

        }
        #endregion IComparer Members
    }

    /// <summary>
    /// Represente un couple (CNFMESSAGE,NCS), et les  instructions qui matchent en fonction d'éléments supplémentaires (le sens (en emission ou en reception),  le type de destinatire, le trader etc....]
    /// <para>
    /// Les instructions qui matchent sont chargés puis ordonnées par la méthode LoadIncis
    /// </para>
    /// <remarks>Remplace la CnfMessageNcsIncis si HPC se plaint de INCI non trouvées</remarks>
    /// </summary>
    public class CnfMessageNcsIncis2 : IComparer
    {
        #region Members
        /// <summary>
        /// Couple CNFMESSAGE,CNF
        /// </summary>
        private readonly CnfMessageNcs _cnfMessageNcs;
        /// <summary>
        /// 
        /// </summary>
        private readonly SendReceiveEnum _sendReceiveEnum;
        /// <summary>
        /// Liste des INCI qui matchent avec le couple CNFMESSGAE,CNF
        /// </summary>
        private Incis _incis;
        /// <summary>
        /// Type de destinatiare 
        /// <para>Doit être renseigné lorsque côté émission</para>
        /// </summary>
        // EG 20160404 Migration vs2013  
        //private Nullable<NotificationRecipientTypeEnum> _recipientType;
        private readonly Nullable<NotificationSendToClass> _recipientType;

        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public Incis Incis
        {
            get { return _incis; }
            set { _incis = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CnfMessageNcs CnfMessageNcs
        {
            get { return _cnfMessageNcs; }
        }
        /// <summary>
        /// 
        /// </summary>
        public SendReceiveEnum SendReceiveEnum
        {
            get
            {
                return _sendReceiveEnum;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCnfMessageNcs"></param>
        /// <param name="pSideEnum"></param>
        /// <param name="pRecipientType"></param>
        // EG 20160404 Migration vs2013  
        //public CnfMessageNcsIncis2(CnfMessageNcs pCnfMessageNcs, SendReceiveEnum pSideEnum, Nullable<NotificationRecipientTypeEnum> pRecipientType)
        public CnfMessageNcsIncis2(CnfMessageNcs pCnfMessageNcs, SendReceiveEnum pSideEnum, Nullable<NotificationSendToClass> pRecipientType)
        {
            _cnfMessageNcs = pCnfMessageNcs;
            _sendReceiveEnum = pSideEnum;
            _recipientType = pRecipientType;
            _incis = new Incis();
        }
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdAContactOffice"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdATrader"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pContract"></param>
        /// <param name="pStBusiness"></param>
        /// <param name="pTradeStMatch"></param>
        /// <param name="pTradeStCheck"></param>
        /// <param name="pIsDefault"></param>
        /// PL 20140710 [20179]
        /// FI 20140808 [20275] Add pIdM parameter
        /// FI 20170913 [23417] Modify (chgt de signature) pIdDc devient pContract
        public void LoadIncis(string pCs, int pIdAContactOffice, int pIdB, int pIdATrader, int pIdM, int pIdI, int pIdIUnderlyer, string pIdC, 
            Pair<Cst.ContractCategory, int> pContract,
            string pStBusiness, string pTradeStMatch, string pTradeStCheck, bool pIsDefault)
        {

            _cnfMessageNcs.LoadSqlTable(pCs);

            //Chargement des INCIs
            _incis = new Incis();
            if (_sendReceiveEnum == SendReceiveEnum.Receive)
            {
                _incis.LoadInciCollectionReceive(pCs, _cnfMessageNcs, pIdAContactOffice, pIdATrader, pIdB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                    pStBusiness, pTradeStMatch, pTradeStCheck, SQL_Table.ScanDataDtEnabledEnum.Yes);
            }
            else
            {
                _incis.LoadInciCollectionSend(pCs, _cnfMessageNcs, pIdAContactOffice, pIdATrader, pIdB, pIdM, pIdI, pIdIUnderlyer, pIdC, pContract,
                    pStBusiness, pTradeStMatch, pTradeStCheck, _recipientType.Value, pIsDefault, SQL_Table.ScanDataDtEnabledEnum.Yes);
            }

            //Tri
            Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Sort()
        {
            bool isOk = (0 < _incis.Count);
            if (isOk)
                Array.Sort(Incis.inci, this);
            return isOk;
        }

        #region IComparer Members
        /// <summary>
        /// Permet de trier les INCIs (les instructions de confirmation) 
        ///
        /// Exemples :
        /// une instruction qui porte sur le message est prioritaire par rapport à une instruction qui porte sur le ncs  
        /// une instruction qui porte et sur le message et sur le ncs, est  prioritaire par rapport à une instruction qui porte sur l'un des deux
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            //
            //si ret = -1, inciX < inciX,  inciX est prioritaire prioritaire
            //si ret =  1, inciY < inciX,  inciY est prioritaire prioritaire
            //

            int ret = 0;

            if ((x is Inci inciX) && (y is Inci inciY))
            {
                #region IDCNFMESSAGE
                if (ret == 0)
                {
                    if (inciX.idCnfMessageSpecified && !inciY.idCnfMessageSpecified)
                        ret = -1;
                    else if (inciY.idCnfMessageSpecified && !inciX.idCnfMessageSpecified)
                        ret = 1;
                }
                #endregion

                #region MSGTYPE
                if (ret == 0)
                {
                    if ((inciX.msgTypeSpecified) && (!inciY.msgTypeSpecified))
                        ret = -1;
                    else if
                        ((inciY.msgTypeSpecified) && (!inciX.msgTypeSpecified))
                        ret = 1;
                }
                #endregion MSGTYPE

                #region IDSTBUSINESS
                if (ret == 0)
                {
                    if ((inciX.idStBusinessSpecified) && (!inciY.idStBusinessSpecified))
                        ret = -1;
                    else if
                        ((inciY.idStBusinessSpecified) && (!inciX.idStBusinessSpecified))
                        ret = 1;
                }
                #endregion IDSTBUSINESS

                //20140710 PL [TRIM 20179]
                #region IDSTMATCH
                if (ret == 0)
                {
                    if ((inciX.idStMatchSpecified) && (!inciY.idStMatchSpecified))
                        ret = -1;
                    else if
                        ((inciY.idStMatchSpecified) && (!inciX.idStMatchSpecified))
                        ret = 1;
                }
                #endregion IDSTMATCH

                #region IDSTCHECK
                if (ret == 0)
                {
                    if ((inciX.idStCheckSpecified) && (!inciY.idStCheckSpecified))
                        ret = -1;
                    else if
                        ((inciY.idStCheckSpecified) && (!inciX.idStCheckSpecified))
                        ret = 1;
                }
                #endregion IDSTCHECK

                #region BOOK
                if (ret == 0)
                {
                    if ((inciX.idBSpecified) && (!inciY.idBSpecified))
                        ret = -1;
                    else if
                        ((inciY.idBSpecified) && (!inciX.idBSpecified))
                        ret = 1;
                }
                #endregion BOOK

                #region TRADER
                if (ret == 0)
                {
                    if ((inciX.idA_traderSpecified) && (!inciY.idA_traderSpecified))
                        ret = -1;
                    else if ((inciY.idA_traderSpecified) && (!inciX.idA_traderSpecified))
                        ret = 1;
                }
                #endregion TRADER

                #region STEPLIFE
                if (ret == 0)
                {
                    if ((inciX.stepLifeEnum.HasValue) && (!inciY.stepLifeEnum.HasValue))
                        ret = -1;
                    else if ((inciY.stepLifeEnum.HasValue) && (!inciX.stepLifeEnum.HasValue))
                        ret = 1;
                }
                #endregion STEPLIFE

                #region CNFTYPE
                if (ret == 0)
                {
                    if ((inciX.confirmationTypeSpecified) && (!inciY.confirmationTypeSpecified))
                        ret = -1;
                    else if
                    ((inciY.confirmationTypeSpecified) && (!inciX.confirmationTypeSpecified))
                        ret = 1;
                }
                #endregion CNFTYPE

                #region IDNCS
                if (ret == 0)
                {
                    if ((inciX.idA_ncsSpecified) && (!inciY.idA_ncsSpecified))
                        ret = -1;
                    else if ((inciY.idA_ncsSpecified) && (!inciX.idA_ncsSpecified))
                        ret = 1;
                }
                #endregion IDNCS

                #region IDC
                if (ret == 0)
                {
                    if (inciX.idCSpecified && (false == inciY.idCSpecified))
                        ret = -1;
                    else if (inciY.idCSpecified && (false == inciX.idCSpecified))
                        ret = 1;
                }
                #endregion IDC

                #region INSTR_UNL
                if (ret == 0)
                {
                    if (inciX.idInstrUnlSpecified && (false == inciY.idInstrUnlSpecified))
                        ret = -1;
                    else if (inciY.idInstrUnlSpecified && (false == inciX.idInstrUnlSpecified))
                        ret = 1;
                }
                #endregion INSTR_UNL

                #region INSTR
                if (ret == 0)
                {
                    if (inciX.idInstrSpecified && (false == inciY.idInstrSpecified))
                        ret = -1;
                    else if (inciY.idInstrSpecified && (false == inciX.idInstrSpecified))
                        ret = 1;
                }
                #endregion INSTR

                #region GPRODUCT
                if (ret == 0)
                {
                    if (inciX.gProductSpecified && (false == inciY.gProductSpecified))
                        ret = -1;
                    else if (inciY.gProductSpecified && (false == inciX.gProductSpecified))
                        ret = 1;
                }
                #endregion GPRODUCT

                #region TYPECONTRACT / IDCONTRACT
                if (ret == 0)
                {
                    if ((inciX.typeContractSpecified && inciX.idContractSpecified) &&
                        (false == (inciY.typeContractSpecified && inciY.idContractSpecified)))
                        ret = -1;
                    else if ((inciY.typeContractSpecified && inciY.idContractSpecified) &&
                        (false == (inciX.typeContractSpecified && inciX.idContractSpecified)))
                        ret = 1;
                }
                #endregion TYPECONTRACT

                #region SIDE
                if (ret == 0)
                {
                    if ((SendReceiveEnum.None != inciX.sideEnum) && (SendReceiveEnum.None == inciY.sideEnum))
                        ret = -1;
                    else if ((SendReceiveEnum.None != inciY.sideEnum) && (SendReceiveEnum.None == inciX.sideEnum))
                        ret = 1;
                }
                #endregion SIDE

                #region PRIORITYRANK
                //En cas d'égalité priorite en fonction de priorityRank
                if (ret == 0)
                {
                    if (inciX.priorityRank < inciY.priorityRank)
                        ret = -1;
                    else if (inciX.priorityRank > inciY.priorityRank)
                        ret = 1;
                }
                #endregion PRIORITYRANK

            }
            else
                throw new ArgumentException("object is not a INCI");

            return ret;

        }
        #endregion IComparer Members
    }

    /// <summary>
    /// une collection de CnfMessageNcsInci
    /// chaque item est constitué d'un couple (CNFMESSAGE,NCS) et de l'instruction la plus appropriée
    /// Le but de cette classe est d'ordonner les item par ordre de priorité (priorityRank)
    /// Exemple d'instruction de reception: 
    /// Le message "A" doit être envoyé via le NCS "NCS1" avec une priorité 0 
    /// Le message "A" doit être envoyé via le NCS "NCS2" avec une priorité 0
    /// Le message "A" doit être envoyé via le NCS "NCS3" avec une priorité 1
    /// 
    /// Spheres doit envoyé le message via NCS1 et NCS2 si l'emetteur en est capable 
    /// sinon
    /// Spheres doit envoyé le message via NCS3
    /// </summary>
    public class CnfMessageNcsIncisBest : IComparer
    {
        #region Members
        CnfMessageNcsInci[] _cnfMessageInci;
        #endregion

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return ArrFunc.Count(_cnfMessageInci);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CnfMessageNcsInci[] CnfMessageInci
        {
            get
            {
                return _cnfMessageInci;
            }
            set
            {
                _cnfMessageInci = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MinPriorityRank
        {
            get
            {
                int ret = 0;
                if (ArrFunc.IsFilled(_cnfMessageInci))
                {
                    this.Sort();
                    ret = _cnfMessageInci[0].PriorityRank;
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MaxPriorityRank
        {
            get
            {
                int ret = 0;
                if (ArrFunc.IsFilled(_cnfMessageInci))
                {
                    this.Sort();
                    ret = _cnfMessageInci[ArrFunc.Count(_cnfMessageInci) - 1].PriorityRank;
                }
                return ret;
            }
        }

        #endregion Accessors

        #region Indexors
        public CnfMessageNcsInci this[int pIndex]
        {
            get
            {
                return _cnfMessageInci[pIndex];
            }
        }
        #endregion Indexors

        #region Methods

        /// <summary>
        ///  Recherche l'instruction valable pur le Ncs Passé en paramètre
        /// </summary>
        /// <param name="pIdNcs"></param>
        /// <returns></returns>
        public CnfMessageNcsInci GetCnfMessageNcsInciCompatibleWithNcs(int pIdNcs)
        {

            CnfMessageNcsInci ret = null;
            if (ArrFunc.IsFilled(_cnfMessageInci))
            {
                for (int i = 0; i < ArrFunc.Count(_cnfMessageInci); i++)
                {
                    if (_cnfMessageInci[i].CnfMessageNcs.idNcs == pIdNcs)
                    {
                        ret = _cnfMessageInci[i];
                        break;
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int[] GetDistinctPriorityRankInOrder()
        {
            int[] ret = null;
            ArrayList al = new ArrayList();
            if (ArrFunc.IsFilled(CnfMessageInci))
            {
                //
                Sort();
                int lastValue = MinPriorityRank;
                al.Add(lastValue);
                //
                for (int i = 0; i < ArrFunc.Count(CnfMessageInci); i++)
                {
                    if (CnfMessageInci[i].PriorityRank != lastValue)
                    {
                        al.Add(CnfMessageInci[i].PriorityRank);
                        lastValue = CnfMessageInci[i].PriorityRank;
                    }
                }
            }
            if (ArrFunc.IsFilled(al))
            {
                ret = (int[])al.ToArray(typeof(int));
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Sort()
        {
            bool isOk = (0 < Count);
            if (isOk)
                Array.Sort(_cnfMessageInci, this);
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPriority"></param>
        /// <returns></returns>
        public CnfMessageNcsInci[] GetCnfMessageNcsInciOnPriority(int pPriority)
        {
            ArrayList al = new ArrayList();
            CnfMessageNcsInci[] ret = null;
            for (int i = 0; i < ArrFunc.Count(CnfMessageInci); i++)
            {
                if (CnfMessageInci[i].PriorityRank == pPriority)
                    al.Add(CnfMessageInci[i]);
            }
            if (ArrFunc.IsFilled(al))
                ret = (CnfMessageNcsInci[])al.ToArray(typeof(CnfMessageNcsInci));
            //
            return ret;
        }

        #endregion Methods

        #region IComparer Members
        public int Compare(object x, object y)
        {
            //
            //si ret = -1, inciX < inciX,  issiX est prioritaire prioritaire
            //si ret =  1, inciY < inciX,  inciY est prioritaire prioritaire
            //

            int ret = 0;

            if ((x is CnfMessageNcsInci xn) && (y is CnfMessageNcsInci yn))
            {
                #region PRIORITYRANK
                //En cas d'égalité priorite en fonction de priorityRank
                if (ret == 0)
                {
                    if (xn.PriorityRank < yn.PriorityRank)
                        ret = -1;
                    else if (xn.PriorityRank > yn.PriorityRank)
                        ret = 1;
                }
                #endregion PRIORITYRANK
            }
            else
                throw new ArgumentException("object is not a CnfMessageNcsIncisBest");

            return ret;

        }
        #endregion IComparer Members
    }

    /// <summary>
    /// Représente une couple (CNFMESSAGE,NCS) avec son instruction la plus appropriée
    /// </summary>
    public class CnfMessageNcsInci
    {
        #region members
        private readonly CnfMessageNcs _cnfMessageNcs;
        private readonly Inci _inci;
        #endregion

        #region Accessor
        public Inci Inci
        {
            get { return _inci; }
        }
        public int IdInci
        {
            get { return _inci.idInci; }
        }
        public int PriorityRank
        {
            get { return _inci.priorityRank; }
        }
        public CnfMessageNcs CnfMessageNcs
        {
            get
            {
                return _cnfMessageNcs;
            }
        }
        #endregion Accessor

        #region constructor
        public CnfMessageNcsInci()
        {
        }
        public CnfMessageNcsInci(CnfMessageNcs pCnfMessageNcs, Inci pInci)
        {
            _cnfMessageNcs = pCnfMessageNcs;
            _inci = pInci;
        }
        #endregion
    }

    /// <summary>
    /// Classe chargé de générer les Routing utilisés pour constituer les membres sendBy,sendTo,copyTo d'un ConfirmationMessageDocument
    /// </summary>
    /// <remarks>
    /// 20090217 FI [16420] Ajout de l'instrument dans le constructor afin de considérer les restrictions par instrument appliquées aux adresses complémentaires
    /// Attention si plusieurs adresses complémentaires s'applique au périmètre instrumental on remonte la 1er aléatoirement
    /// </remarks> 
    public class NotificationRoutingActorsBuilder : RoutingActorsBuilder
    {
        #region Members
        protected string addressIdent;
        protected int idI;
        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public override bool IsAddAddress
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool IsAddPhone
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool IsAddWeb
        {
            get { return true; }
        }
        #endregion

        #region constructor

        /// <summary>
        /// Chargement des informations générale (Addresse, Email, Téléphone)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAddressIdent">Typde d'adresse complémentaire</param>
        /// <param name="pIdI">instrument (pour appliquer une restriction sur le périmètre instrumenttal)</param>
        /// <param name="pRoutingCreateElement"></param>
        /// FI 20190515 [23912] add constructor
        public NotificationRoutingActorsBuilder(IRoutingCreateElement pRoutingCreateElement)
            : this(string.Empty, 0, pRoutingCreateElement)
        {
        }

        /// <summary>
        /// Chargement des informations générale (Addresse, Email, Téléphone) associé à une adresse compémentaire
        /// </summary>
        /// <param name="pAddressIdent">Typde d'adresse complémentaire</param>
        /// <param name="pIdI">instrument (pour appliquer une restriction sur le périmètre instrumental)</param>
        /// <param name="pRoutingCreateElement"></param>
        public NotificationRoutingActorsBuilder(string pAddressIdent, int pIdI, IRoutingCreateElement pRoutingCreateElement)
            : base(pRoutingCreateElement)
        {
            addressIdent = pAddressIdent;
            idI = pIdI;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <returns></returns>
        public override IRouting GetRouting(int pIda)
        {
            return base.GetRouting(pIda);
        }

        /// <summary>
        /// Chargement des informations générale (Addresse, Email, Téléphone) pour une liste d'acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pListIdA"></param>
        public override void Load(string pCS, int[] pListIdA)
        {
            if (StrFunc.IsFilled(addressIdent))
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "ADDRESSIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), addressIdent);
                //
                StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                sql += "a.IDA,a.IDENTIFIER,a.BIC,a.LTCODE,null as CSSMEMBERIDENT,a.DISPLAYNAME,a.DESCRIPTION," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESS1 else a.ADDRESS1 end as ADDRESS1," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESS2 else a.ADDRESS2 end as ADDRESS2," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESS3 else a.ADDRESS3 end as ADDRESS3," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESS4 else a.ADDRESS4 end as ADDRESS4," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESSPOSTALCODE else a.ADDRESSPOSTALCODE end as ADDRESSPOSTALCODE," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESSCITY else a.ADDRESSCITY end as ADDRESSCITY," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESSSTATE else a.ADDRESSSTATE end as ADDRESSSTATE," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.ADDRESSCOUNTRY else a.ADDRESSCOUNTRY end as ADDRESSCOUNTRY," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.TELEPHONENUMBER else a.TELEPHONENUMBER end as TELEPHONENUMBER," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.MOBILEPHONENUMBER else a.MOBILEPHONENUMBER end as MOBILEPHONENUMBER," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.FAXNUMBER else a.FAXNUMBER end as FAXNUMBER," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.TELEXNUMBER else a.TELEXNUMBER end as TELEXNUMBER," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.MAIL else a.MAIL end as MAIL," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.WEB else a.WEB end as WEB," + Cst.CrLf;
                sql += "case when ac.IDA is not null then " + DataHelper.SQLIsNull(pCS, "ac.CULTURE_CNF", "ac.CULTURE") + " else " + DataHelper.SQLIsNull(pCS, "a.CULTURE_CNF", "a.CULTURE") + " end as CULTURE," + Cst.CrLf;
                sql += "case when ac.IDA is not null then ac.IDC_CNF else a.IDC_CNF end as IDC_CNF" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                sql += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ADDRESSCOMPL.ToString() + " ac on ac.IDA=a.IDA and ac.ADDRESSIDENT=@ADDRESSIDENT" + Cst.CrLf;
                //FI 20100325 appel à SQLInstrCriteria à la place de InstrTools.GetSQLCriteriaInstr
                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCS, null, idI, false, true, SQL_Table.ScanDataDtEnabledEnum.Yes);
                sql += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction("ac", RoleGInstr.CNF);

                sql += SQLCst.WHERE + "(" + DataHelper.SQLColumnIn(pCS, "a.IDA", pListIdA, TypeData.TypeDataEnum.integer) + ")";
                if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                    sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a").ToString();

                DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sql.ToString(), parameters.GetArrayDbParameter());
                dt = ds.Tables[0];
            }
            else
            {
                base.Load(pCS, pListIdA);
            }
        }
        #endregion
    }

    /// <summary>
    /// Représente les directives utilisées pour bâtir le message
    /// </summary>
    public class NotificationDocumentSettings
    {
        #region Members
        /// <summary>
        /// <para>Necessite la mise à jour des évènements dans le flux</para>
        /// Avec détail sur l'évènement déclencheur
        /// </summary>
        public bool IsUseChildEvents
        {
            get;
            set;
        }
        /// <summary>
        /// Nécessite la mise à disposition des instructions de règlement dans le flux XML	
        /// </summary>
        public bool IsUseEventSi
        {
            get;
            set;
        }
        /// <summary>
        /// Nécessite la mise à disposition du bloc-notes du trade dans le flux XML	
        /// </summary>
        public bool IsUseNotepad
        {
            get;
            set;
        }
        public CnfMessageDet[] CnfMessageDet
        {
            get;
            set;
        }
        #endregion Members

        #region Constructors
        public NotificationDocumentSettings()
        { }
        public NotificationDocumentSettings(bool pIsUseChildEvents, bool pIsUseEventSi, bool pIsUseNotepad, CnfMessageDet[] pCnfMessageDet)
        {
            IsUseChildEvents = pIsUseChildEvents;
            IsUseEventSi = pIsUseEventSi;
            IsUseNotepad = pIsUseNotepad;
            CnfMessageDet = pCnfMessageDet;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdP"></param>
        /// <param name="pIdGInstr"></param>
        /// <param name="pIdI"></param>
        /// <returns></returns>
        public bool IsMessageDetRequested(int pIdP, int pIdGInstr, int pIdI)
        {
            bool isFound = false;
            if (ArrFunc.IsFilled(CnfMessageDet))
            {
                InvoicingTradeDetailEnum pDetailEnum = new InvoicingTradeDetailEnum();
                FieldInfo[] flds = pDetailEnum.GetType().GetFields();
                foreach (FieldInfo fld in flds)
                {
                    if (System.Enum.IsDefined(typeof(InvoicingTradeDetailEnum), fld.Name))
                    {
                        InvoicingTradeDetailEnum enumValue = (InvoicingTradeDetailEnum)System.Enum.Parse(typeof(InvoicingTradeDetailEnum), fld.Name);
                        isFound = IsMessageDetRequested(pIdP, pIdGInstr, pIdI, enumValue);
                        if (isFound)
                            break;
                    }
                }
            }
            return isFound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdP"></param>
        /// <param name="pIdGInstr"></param>
        /// <param name="pIdI"></param>
        /// <param name="pDetailEnum"></param>
        /// <returns></returns>
        public bool IsMessageDetRequested(int pIdP, int pIdGInstr, int pIdI, InvoicingTradeDetailEnum pDetailEnum)
        {
            bool isFound = false;
            if (ArrFunc.IsFilled(CnfMessageDet))
            {
                isFound = IsMessageDetRequested(TypeInstrEnum.Instr, pIdI, pDetailEnum);
                if (false == isFound)
                    isFound = IsMessageDetRequested(TypeInstrEnum.GrpInstr, pIdGInstr, pDetailEnum);
                if (false == isFound)
                    isFound = IsMessageDetRequested(TypeInstrEnum.Product, pIdP, pDetailEnum);
            }
            return isFound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTypeInstr"></param>
        /// <param name="pId"></param>
        /// <param name="pDetailEnum"></param>
        /// <returns></returns>
        private bool IsMessageDetRequested(TypeInstrEnum pTypeInstr, int pId, InvoicingTradeDetailEnum pDetailEnum)
        {
            bool isFound = false;
            foreach (CnfMessageDet item in CnfMessageDet)
            {
                if ((pTypeInstr == item.typeInstr) &&
                    (pDetailEnum == item.data) &&
                    (pId == item.idInstr))
                {
                    isFound = true;
                    break;
                }
            }
            return isFound;
        }
        #endregion Methods

    }

    /// <summary>
    /// Représente une chaîne de confirmation
    /// <para>une chaîne de confirmation contient 2 items :  1 pour côté Emission, 1 côte Reception</para>
    /// </summary>
    /// <remarks>
    /// Attention il faut veiller à ce que la class ConfirmationChainProcess reste en phase avec ConfirmationChain (voir solution des services)
    /// voir constructor de la classe ConfirmationChainProcess
    /// </remarks>
    public class ConfirmationChain
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly ConfirmationChainItem[] _confirmationChainItem;
        /// <summary>
        /// 
        /// </summary>
        private bool _isSendTo_Broker;
        // EG 20160404 Migration vs2013  
        //private bool _isSendTo_Clearer;
        /// <summary>
        /// 
        /// </summary>
        private bool _isSendBy_ActorEntity;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient true si le contact office est identique de chaque coté de la chaîne de confirmation
        /// <para>
        /// En générale lorsque l'envoi de message s'effectue vers un client ou un compte maison et que ce dernier ne possède pas de contact office spécifique 
        /// </para>
        /// </summary>
        public bool IsContactOfficesIdentical
        {
            get
            {
                bool ret = false;
                if ((null != _confirmationChainItem[0].sqlContactOffice) && (null != _confirmationChainItem[1].sqlContactOffice))
                    ret = (_confirmationChainItem[0].sqlContactOffice.Id == _confirmationChainItem[1].sqlContactOffice.Id);
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique que le Contact Office de la chaîne de reception est un BROKER
        /// <para>
        ///   Ce cas se produit sur les opération en "names give-up" lorsque le destinataire
        ///   <para>
        ///   - est une contrepartie externe 
        ///   </para>
        ///   <para>
        ///   - passe par un broker (Broker rattaché à la partie externe) lui même contact Office 
        ///   </para>
        /// </para>
        /// </summary>
        public bool IsSendTo_Broker
        {
            get { return _isSendTo_Broker; }
            set { _isSendTo_Broker = value; }
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique true si l'acteur de la demi-chaîne émettrice est l'entité
        /// <para>
        /// ex.:  Cas d'un trade réalisé entre "Contrepartie externe" et un "Client". 
        /// La "Contrepartie externe" reçevra un message envoyé par le Contact Office de l'Entité du Client.
        /// <para>
        /// Demi-chaîne émettrice: l'acteur est une Entité, le Book est nécessairement null, le C.O. est celui de l'Entité
        /// </para>
        /// </para>
        /// </summary>
        public bool IsSendByActor_Entity
        {
            get { return _isSendBy_ActorEntity; }
            set { _isSendBy_ActorEntity = value; }
        }

        /// <summary>
        /// Obtient true si la chaîne de confirmation est valide
        /// </summary>
        public bool IsValid
        {
            get { return StrFunc.IsEmpty(CheckConfirmationChain()); }
        }

        /// <summary>
        /// Obtient true s'il existe un contact office dans l'item [SendEnum.SendTo]
        /// <para>Il n'existe pas de contact office lorsque l'acteur ne veut pas recevoir de messagerie</para>
        /// </summary>
        public bool IsExistSendToContactOffice
        {
            get { return ((null != this[SendEnum.SendTo].sqlContactOffice) && (this[SendEnum.SendTo].sqlContactOffice.IsLoaded)); }
        }

        /// <summary>
        /// Obtient l'dentifier du Contact Office du côté Emetteur
        /// </summary>
        public string SendByContactOffice
        {
            get { return GetContactOfficeIdentifier(SendEnum.SendBy); }
        }

        /// <summary>
        /// Obtient l'dentifier du Contact Office du côté Destinataire
        /// </summary>
        public string SendToContactOffice
        {
            get { return GetContactOfficeIdentifier(SendEnum.SendTo); }
        }
        #endregion Accessors

        #region Indexors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendSide"></param>
        /// <returns></returns>
        public ConfirmationChainItem this[SendEnum sendSide]
        {
            get
            {
                if (SendEnum.SendBy == sendSide)
                    return _confirmationChainItem[0];
                else
                    return _confirmationChainItem[1];
            }
        }
        #endregion Indexors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ConfirmationChain()
        {
            _confirmationChainItem = new ConfirmationChainItem[2] { new ConfirmationChainItem(), new ConfirmationChainItem() };
        }
        #endregion

        #region Methodes

        /// <summary>
        /// Retourne les messages compatibles avec la chaîne de confirmation
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSettings">paramètres pour le chargement</param>
        /// <returns></returns>
        public CnfMessages LoadCnfMessage(string pCs, LoadMessageSettings pSettings)
        {
            CnfMessages ret = new CnfMessages();
            ret.Initialize(pCs, pSettings, this[SendEnum.SendBy].IdAContactOffice,
                                            this[SendEnum.SendTo].IdAContactOffice,
                                            SQL_Table.ScanDataDtEnabledEnum.Yes);
            return ret;
        }

        /// <summary>
        /// Retourne un message d'erreur si la chaîne de confirmation n'est pas correcte
        /// </summary>
        /// <returns></returns>
        public string CheckConfirmationChain()
        {

            string ret = string.Empty;
            //
            if (null == this[SendEnum.SendBy].sqlActor || (false == this[SendEnum.SendBy].sqlActor.IsLoaded))
                ret += " sendBy Actor is unknown";
            if (null == this[SendEnum.SendBy].sqlContactOffice || (false == this[SendEnum.SendBy].sqlContactOffice.IsLoaded))
                ret += " sendBy Contact Office is unknown";
            if (null == this[SendEnum.SendTo].sqlActor || (false == this[SendEnum.SendTo].sqlActor.IsLoaded))
                ret += " sendTo Actor is unknown";
            if (null == this[SendEnum.SendTo].sqlContactOffice || (false == this[SendEnum.SendTo].sqlContactOffice.IsLoaded))
                ret += " sendTo Contact Office is unknown";
            //
            return ret;

        }

        /// <summary>
        /// Retourne l'identifier de l'acteur contact office côté SendBy ou côté SendTo
        /// </summary>
        /// <param name="pSend"></param>
        /// <returns></returns>
        public string GetContactOfficeIdentifier(SendEnum pSend)
        {
            string ret = string.Empty;
            //
            if (null != this[pSend].sqlContactOffice)
                ret = this[pSend].sqlContactOffice.Identifier;
            //
            return ret;
        }

        /// <summary>
        /// Retourne l'OtcmlId de l'acteur destinataire par défaut
        /// <para>Si chaîne de confirmation client retourne le client, sinon retourne le contact office</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150106 [XXXXX] Modify
        public int GetDefaultSendToIda()
        {
            int ret;
            //FI 20120417 [17752]
            if (IsContactOfficesIdentical)
            {
                // FI 20150106 [XXXXX] Test la présence d'un book car si édition consolidée il n'y a pas de book dans la chaine de confirmation
                // Dans ce cas Spheres® considère idActor
                if (null != this[SendEnum.SendTo].sqlBook)
                {
                    //L'acteur proprétaire du book est l'unique destinataire du message
                    ret = this[SendEnum.SendTo].sqlBook.IdA;
                }
                else
                {
                    ret = this[SendEnum.SendTo].IdActor;
                }
            }
            else
            {
                //s'il n'existe pas de destinataires spécifiés sur les instructions
                //l'acteur contact office est le destinataire par défaut 
                ret = this[SendEnum.SendTo].IdAContactOffice;
            }
            return ret;
        }

        /// <summary>
        /// Retourne le language par défaut de la chaîne de confirmation
        /// <para>c'est l'acteur destinataire qui oriente le language utilisé</para>
        /// </summary>
        public string GetDefaultLanquage(string pCS)
        {
            string ret = string.Empty;

            int ida = GetDefaultSendToIda();
            SQL_Actor sqlActor = new SQL_Actor(pCS, ida);
            sqlActor.LoadTable(new string[] { "CULTURE", "CULTURE_CNF" });
            if ((sqlActor.IsLoaded))
            {
                if (StrFunc.IsFilled(sqlActor.Culture_Cnf))
                    ret = sqlActor.Culture_Cnf;
                else if (StrFunc.IsFilled(sqlActor.Culture))
                    ret = sqlActor.Culture;
            }
            return ret;
        }

        /// <summary>
        /// Retourne true s'il n'existe pas de contre-indication (au niveau Book, Entity...) concernant l'envoi et la réception de message de Notifications/Confirmations.
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pNotificationClass">Type de messagerie (Edition simple, consolidée ou confirmation)</param>
        /// <param name="opMessageLog">Message de sortie en cas de contre-indication</param>
        /// <returns></returns>
        /// FI 20120418 [17752] Refactoting
        public bool IsGenerateMessage(string pCs, NotificationClassEnum pNotificationClass, out string opMessageLog)
        {
            bool ret = true;
            opMessageLog = string.Empty;

            #region -- SendTo (Destinataire) ------------------------------------------------------------------------------------------------------
            if (null != this[SendEnum.SendTo].sqlBook)
            {
                // Contrôle du book du destinataire pour savoir s'il est paramétré pour recevoir des Notifications/Confirmations.
                SQL_Book sqlBook = this[SendEnum.SendTo].sqlBook;
                ret = (sqlBook.IsReceiveNcMsg);
                if (false == ret)
                    opMessageLog = Ressource.GetString2("Msg_CnfProcess_BookNoMessageToReceive", sqlBook.Identifier);
            }
            #endregion

            #region -- SendBy (Emetteur) ----------------------------------------------------------------------------------------------------------
            //PL 20111128 Ajout du test sur: ret=true
            if (ret)
            {
                int idAEntity;
                if (IsSendByActor_Entity)
                {
                    // Cas où l'emission est pilotée par l'entité, celle-ci se trouve alors dans sqlActor
                    // ex. 
                    //     - Emission d'une confirmation à un Client depuis un Trade Client vs Entity
                    //     - Emission d'une confirmation à une Contrepartie externe depuis un Trade Client vs Contrepartie externe
                    idAEntity = this[SendEnum.SendBy].sqlActor.Id;
                }
                else
                {
                    // Cas où l'emission est pilotée par la partie elle même, on obtient alors son entité depuis sqlBook
                    // ex. 
                    //     - Emission d'une confirmation à une Contrepartie externe depuis un Trade Entity vs Contrepartie externe
                    //     - Emission d'une confirmation à une Entity depuis un Trade Entity E1 vs Entity E2
                    idAEntity = this[SendEnum.SendBy].sqlBook.IdA_Entity;
                }
                if (idAEntity == 0)
                    throw new Exception("Entity not Found");

                SQL_Entity sqlEntity = new SQL_Entity(pCs, idAEntity);
                bool isEntityParameterFound = sqlEntity.LoadTable(new string[] { "ISSENDNCMSG_CLIENT", "ISSENDNCMSG_ENTITY", "ISSENDNCMSG_EXT" });

                if (isEntityParameterFound)
                {
                    NotificationSendToClass sendToClass = this.SendTo(pCs, pNotificationClass);
                    switch (sendToClass)
                    {
                        case NotificationSendToClass.Client:
                            ret = sqlEntity.IsSendNcMsgClient;
                            if (false == ret)
                                opMessageLog = Ressource.GetString2("Msg_CnfProcess_EntityNoMessageToSendToClient", sqlEntity.Identifier);
                            break;
                        case NotificationSendToClass.Entity:
                            ret = sqlEntity.IsSendNcMsgHouse;
                            if (false == ret)
                                opMessageLog = Ressource.GetString2("Msg_CnfProcess_EntityNoMessageToSendToEntity", sqlEntity.Identifier);
                            break;
                        case NotificationSendToClass.External:
                            ret = sqlEntity.IsSendNcMsgExt;
                            if (false == ret)
                                opMessageLog = Ressource.GetString2("Msg_CnfProcess_EntityNoMessageToSendToExternalCounterparty", sqlEntity.Identifier);
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", sendToClass.ToString()));
                    }
                }
            }
            #endregion

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public string GetDisplay(string pCS)
        {
            string ret = string.Empty;

            ret += "SendBy:";
            if (null != this[SendEnum.SendBy].sqlContactOffice && this[SendEnum.SendBy].sqlContactOffice.IsLoaded)
                ret += " [Contact Office: " + this[SendEnum.SendBy].sqlContactOffice.Identifier + "]";
            else
                ret += " [Contact Office: <Unknown>]";
            //20080721 PL Lignes ajoutées
            if (IsSendTo_Client(pCS) || IsSendTo_Broker)
            {
                if (null != this[SendEnum.SendBy].sqlActor && this[SendEnum.SendBy].sqlActor.IsLoaded)
                    ret += " [Entity: " + this[SendEnum.SendBy].sqlActor.Identifier + "]";
                if (null != this[SendEnum.SendTo].sqlBook && this[SendEnum.SendTo].sqlBook.IsLoaded)
                    ret += " [Entity of book: " + this[SendEnum.SendTo].sqlBook.Identifier + "]";
            }
            else
            {
                if (null != this[SendEnum.SendBy].sqlActor && this[SendEnum.SendBy].sqlActor.IsLoaded)
                    ret += " [Party: " + this[SendEnum.SendBy].sqlActor.Identifier + "]";
                if (null != this[SendEnum.SendBy].sqlBook && this[SendEnum.SendBy].sqlBook.IsLoaded)
                    ret += " [Book: " + this[SendEnum.SendBy].sqlBook.Identifier + "]";
            }
            //
            ret += Cst.CrLf;
            //
            ret += "SendTo:";
            if (null != this[SendEnum.SendTo].sqlContactOffice && this[SendEnum.SendTo].sqlContactOffice.IsLoaded)
                ret += " [Contact Office: " + this[SendEnum.SendTo].sqlContactOffice.Identifier + "]";
            else
                ret += " [Contact Office: Unknown]";
            //20080721 PL Lignes ajoutées
            if (IsSendTo_Broker)
            {
                if (null != this[SendEnum.SendTo].sqlActor && this[SendEnum.SendTo].sqlActor.IsLoaded)
                    ret += " [Broker of party: " + this[SendEnum.SendTo].sqlActor.Identifier + "]";
            }
            else
            {
                if (null != this[SendEnum.SendTo].sqlActor && this[SendEnum.SendTo].sqlActor.IsLoaded)
                    ret += " [Party: " + this[SendEnum.SendTo].sqlActor.Identifier + "]";
                if (null != this[SendEnum.SendTo].sqlBook && this[SendEnum.SendTo].sqlBook.IsLoaded)
                    ret += " [Book: " + this[SendEnum.SendTo].sqlBook.Identifier + "]";
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si le destinataire est un client 
        /// </summary>
        /// FI 20120528 Cette méthode est appelée sur la messagerie de type multi-parties
        /// Je branche un test sur (null != this[SendEnum.SendTo].sqlBook)
        public bool IsSendTo_Client(string pCS)
        {
            bool ret = false;
            if (IsSendByActor_Entity)
            {
                if (null != this[SendEnum.SendTo].sqlBook)
                {
                    ret = BookTools.IsBookClient(pCS, this[SendEnum.SendTo].sqlActor.Id,
                                                        this[SendEnum.SendTo].sqlBook.Id,
                                                        this[SendEnum.SendBy].sqlActor.Id);
                }
                else
                {
                    ret = ActorTools.IsActorClient(pCS, this[SendEnum.SendTo].sqlActor.Id);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si le destinataire est un Dealer Maison
        /// <para>Sur les confirmations et les éditions simples, le destinataire est maison si le book est géré par l'entité émettrice</para>
        /// <para>Sur les editions consolidées, le destinataire est (nécessairement) maison s'il n'est pas CLIENT</para>
        /// </summary>
        public bool IsSendTo_Entity(string pCS, NotificationClassEnum pNotificationClass)
        {
            bool ret = false;

            if (IsSendByActor_Entity)
            {
                switch (pNotificationClass)
                {
                    case NotificationClassEnum.MONOTRADE:
                    case NotificationClassEnum.MULTITRADES:
                        if ((null != this[SendEnum.SendTo].sqlActor) && (null != this[SendEnum.SendTo].sqlBook))
                        {
                            int idAEntity = this[SendEnum.SendBy].sqlActor.Id;
                            ret = BookTools.IsCounterPartyHouse(pCS, this[SendEnum.SendTo].sqlActor.Id, this[SendEnum.SendTo].sqlBook.Id, idAEntity);
                        }
                        break;
                    case NotificationClassEnum.MULTIPARTIES:
                        ret = (false == IsSendTo_Client(pCS));
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("NotificationClass: {0}", pNotificationClass.ToString()));
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le type de destinataire 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pNotificationClass">Type de messagerie (Edition simple, consolidée ou confirmation)</param>
        /// <returns></returns>
        public NotificationSendToClass SendTo(string pCS, NotificationClassEnum pNotificationClass)
        {
            NotificationSendToClass ret;
            if (IsSendTo_Client(pCS))
                ret = NotificationSendToClass.Client;
            else if (IsSendTo_Entity(pCS, pNotificationClass))
                ret = NotificationSendToClass.Entity;
            else
                ret = NotificationSendToClass.External;

            return ret;
        }

        #endregion
    }

    /// <summary>
    /// Représente le triplet Party, Book, Contact Office impliqué dans un item d'une chaîne de confirmation
    /// </summary>
    public class ConfirmationChainItem
    {
        #region Members
        public SQL_Actor sqlActor;
        public SQL_Book sqlBook;
        public SQL_Actor sqlContactOffice;
        public SQL_Actor sqlTrader;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public int IdActor
        {
            get
            {
                int ret = 0;
                if ((null != sqlActor) && (sqlActor.IsLoaded))
                    ret = sqlActor.Id;
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdBook
        {
            get
            {
                int ret = 0;
                if ((null != sqlBook) && (sqlBook.IsLoaded))
                    ret = sqlBook.Id;
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdAContactOffice
        {
            get
            {
                int ret = 0;
                if ((null != sqlContactOffice) && (sqlContactOffice.IsLoaded))
                    ret = sqlContactOffice.Id;
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdATrader
        {
            get
            {
                int ret = 0;
                if ((null != sqlTrader) && (sqlTrader.IsLoaded))
                    ret = sqlTrader.Id;
                return ret;
            }
        }
        #endregion

        #region Constructors
        public ConfirmationChainItem()
        {
        }
        #endregion Constructors

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdA"></param>
        public void LoadActor(string pCs, int pIdA)
        {
            sqlActor = null;
            if (0 < pIdA)
            {
                sqlActor = new SQL_Actor(pCs, pIdA);
                sqlActor.LoadTable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdB"></param>
        // EG 20150706 [21021] Nullable<int> pIdB
        public void LoadBook(string pCs, Nullable<int> pIdB)
        {
            sqlBook = null;
            if (pIdB.HasValue)
            {
                sqlBook = new SQL_Book(pCs, pIdB.Value);
                sqlBook.LoadTable();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdAContactOffice"></param>
        public void LoadContactOffice(string pCs, int pIdAContactOffice)
        {
            sqlContactOffice = null;
            if (0 < pIdAContactOffice)
            {
                sqlContactOffice = new SQL_Actor(pCs, pIdAContactOffice);
                sqlContactOffice.LoadTable();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdATrader"></param>
        public void LoadTrader(string pCs, int pIdATrader)
        {
            sqlTrader = null;
            if (0 < pIdATrader)
            {
                sqlTrader = new SQL_Actor(pCs, pIdATrader);
                sqlTrader.LoadTable();
            }
        }

        #endregion
    }

    /// <summary>
    /// Représente un NCS
    /// </summary>
    public class NotificationConfirmationSystem
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        public int idNcs;
        /// <summary>
        /// 
        /// </summary>
        public string identifier;
        /// <summary>
        /// 
        /// </summary>
        public string extlLink;
        /// <summary>
        /// 
        /// </summary>
        public int idIoTask;
        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public NotificationConfirmationSystem()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdNcs"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pExtlLink"></param>
        public NotificationConfirmationSystem(int pIdNcs, string pIdentifier, string pExtlLink)
        {
            idNcs = pIdNcs;
            identifier = pIdentifier;
            extlLink = pExtlLink;
        }
        #endregion constructor
    }

    /// <summary>
    ///  Dates d'émission d'une notification
    /// </summary>
    public class NotificationSendDateInfo
    {
        #region Members
        /// <summary>
        /// Représente la date de l'événement déclencheur du message
        /// </summary>
        public DateTime dateEvent;
        /// <summary>
        /// Représente la date de génération de message 
        /// <para>Cette date est obtenue après application de l'offset présent sur le message</para>
        /// </summary>
        public DateTime dateToSend;
        /// <summary>
        /// Représente la dateForcée de génération de message 
        /// <para>Elle est supérieures si le message est généré en retard</para>
        /// </summary>
        public DateTime dateToSendForced;
        #endregion

        #region constructor
        public NotificationSendDateInfo(DateTime pDateEvent, DateTime pDateToSend, DateTime pDateToSendForced)
        {
            dateEvent = pDateEvent;
            dateToSend = pDateToSend;
            dateToSendForced = pDateToSendForced;
        }
        #endregion
    }

    /// <summary>
    /// Repésente les codes des évènements déclencheur de messagerie
    /// </summary>
    /// FI 20120731 eventType devient de type EventTypeEnum
    public class EventTrigger
    {
        [System.Xml.Serialization.XmlElementAttribute("EVENTCODE")]
        public EventCodeEnum eventCode;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EVENTTYPE")]
        public EventTypeEnum eventType;
        //
        [System.Xml.Serialization.XmlElementAttribute("EVENTCLASS")]
        public EventClassEnum eventClass;

        public EventTrigger()
        {
        }

        public EventTrigger(EventCodeEnum pEventCode, EventClassEnum pEventClass, bool pEventTypeSpecified = false)
        {
            eventCode = pEventCode;
            eventTypeSpecified = pEventTypeSpecified;
            eventClass = pEventClass;
        }
}

    /// <summary>
    /// Représente une action sur position sur un trade avec frais
    /// </summary>
    /// FI 20150825 [21287] Add classe
    public partial class PosActionTradeFee : IPosactionTradeFee
    {
        /// <summary>
        /// Identitiiant non significatif de l'action
        /// </summary>
        public int idPosActionDet;

        /// <summary>
        /// Identitiiant non significatif du trade
        /// </summary>
        public int idT;

        /// <summary>
        /// 
        /// </summary>
        public bool feeSpecified;

        /// <summary>
        /// représente les frais rattachés à l'action
        /// </summary>
        public ReportFee[] fee;

        #region IPosactionTradeFee Membres

        int IPosactionTradeFee.IdPosActionDet
        {
            get
            {
                return this.idPosActionDet;
            }
            set
            {
                this.idPosActionDet = value;
            }
        }

        int ITradeFee.IdT
        {
            get
            {
                return this.idT;
            }
            set
            {
                this.idT = value;
            }
        }

        bool ITradeFee.FeeSpecified
        {
            get
            {
                return this.feeSpecified;
            }
            set
            {
                this.feeSpecified = value;
            }
        }

        ReportFee[] ITradeFee.Fee
        {
            get
            {
                return this.fee;
            }
            set
            {
                fee = value;
            }
        }

        #endregion

    }

}

