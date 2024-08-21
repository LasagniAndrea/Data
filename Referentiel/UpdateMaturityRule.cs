using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EfsML;
using EfsML.Business;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;

namespace EFS.Referential
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20240111 [WI793] Refactoring. Le logger n'est pas utilisé mais uniquement ProcessLog
    public class UpdateMaturityRuleProcess
    {
        /// <summary>
        ///  Plus petite DtBusiness en vigueur sur ENTITYMARKET--> dtMarket
        /// </summary>
        private readonly DateTime _dtMinMarket = DateTime.MinValue;

        /// <summary>
        /// connexion à la base de connée
        /// </summary>
        private readonly string _cs;

        /// <summary>
        /// 
        /// </summary>
        private readonly int _idA;

        /// <summary>
        /// 
        /// </summary>
        private readonly AppSession _session;

        /// <summary>
        /// 
        /// </summary>
        private readonly DateTime _dtSys;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pSession"></param>
        public UpdateMaturityRuleProcess(string cs, AppSession pSession)
        {
            _cs = cs;
            _idA = pSession.IdA;
            _session = pSession;

            _dtSys = OTCmlHelper.GetDateSysUTC(_cs);
            _dtMinMarket = MarketTools.GetMinDateMarket(CSTools.SetCacheOn(_cs));
        }

        /// <summary>
        /// Modification de la règle d'échéance dite principale sur le DC <paramref name="idDC"/>
        /// </summary>
        /// <param name="idDC">Représente le DC</param>
        /// <param name="idNewMR">Nouvelle règle d'échéance</param>
        /// FI 20220511 [XXXXX] Add
        public void UpdateMainMaturityRule(int idDC, int idNewMR)
        {
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDDC), idDC);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), idNewMR);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD), _idA);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPD), _dtSys);

            string sqlQuery = StrFunc.AppendFormat(@"UPDATE dbo.DERIVATIVECONTRACT 
set IDMATURITYRULE = @IDMATURITYRULE, 
IDAUPD = @IDAUPD, DTUPD = @DTUPD
where IDDC = @IDDC");

            QueryParameters queryParameters = new QueryParameters(_cs, sqlQuery, dp);

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(_cs))
            {
                try
                {
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                    DataHelper.CommitTran(dbTransaction, false);
                    DataHelper.queryCache.Remove("DERIVATIVECONTRACT", _cs, true);
                    DataEnabledHelper.ClearCache(_cs, Cst.OTCml_TBL.DERIVATIVECONTRACT);
                }
                catch
                {
                    if (DataHelper.IsTransactionValid(dbTransaction))
                        DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
        }


        /// <summary>
        /// Modification de la règle d'échéance additionnelle <paramref name="idOldMR"/> sur le DC <paramref name="idDC"/>
        /// </summary>
        /// <param name="idDC">Représente le DC</param>
        /// <param name="idOldMR">Règle d'échéance complémentaire remplacée</param>
        /// <param name="idNewMR">Nouvelle règle d'échéance</param>
        /// FI 20220511 [XXXXX] Add
        public void UpdateAdditionalMaturityRule(int idDC, int idOldMR, int idNewMR)
        {
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDDC), idDC);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.ID), idOldMR);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), idNewMR);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD), _idA);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPD), _dtSys);


            string sqlQuery = StrFunc.AppendFormat(@"UPDATE dbo.DRVCONTRACTMATRULE 
set IDMATURITYRULE = @IDMATURITYRULE, 
IDAUPD = @IDAUPD, DTUPD = @DTUPD
where IDDC = @IDDC and IDMATURITYRULE=@ID");

            QueryParameters queryParameters = new QueryParameters(_cs, sqlQuery, dp);

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(_cs))
            {
                try
                {
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                    DataHelper.CommitTran(dbTransaction, false);

                    Cst.OTCml_TBL[] table = new Cst.OTCml_TBL[] { Cst.OTCml_TBL.DERIVATIVECONTRACT, Cst.OTCml_TBL.DRVCONTRACTMATRULE };
                    DataHelper.queryCache.Remove(ArrFunc.Map<Cst.OTCml_TBL, string>(table, (x) => { return x.ToString(); }), _cs, true);
                    DataEnabledHelper.ClearCache(_cs, table);
                }
                catch
                {
                    if (DataHelper.IsTransactionValid(dbTransaction))
                        DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
        }



        /// <summary>
        /// Mise à jour des tables MATURITY et DERIVATIVEATTRIB suite au remplacement de la règle d'échéance <paramref name="idOldMR"/> sur le contrat dérivé <paramref name="idDC"/>
        /// <para>Insertion des  échéances (Table MATURITY) (si nécessaire)</para>
        /// <para>Maj des échéances ouvertes en vigueur (Table DERIVATIVEATTRIB)</para>
        /// </summary>
        /// <param name="idDC">Représente le DC</param>
        /// <param name="idOldMR">Règle d'échéance remplacée</param>
        /// <param name="idNewMR">Nouvelle règle d'échéance</param>
        /// FI 20190612 [XXXXX] Refactoring (la méthode ne fonctionnait pas sous Oracle®)
        /// FI 20220511 [XXXXX] Refactioring (usage de idOldMR)
        public void UpdateDerivAttrib(int idDC, int idOldMR, int idNewMR)
        {

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDDC), idDC);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULEOLD), idOldMR);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), idNewMR);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DT), _dtMinMarket);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDA), _idA);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTINS), _dtSys);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPD), _dtSys);

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(_cs))
            {
                try
                {
                    // Etape 1 : [MATURITY] Requête SQL qui copie (Insert) les maturités (MATURITYMONTHYEAR) relatives aux trades de CurrentMR et sur newMR - si non déjà existantes
                    string sqlQuery = @"insert into dbo.MATURITY (IDMATURITYRULE, MATURITYMONTHYEAR, DTENABLED, DTDISABLED, IDAINS, DTINS)
select distinct @IDMATURITYRULE, ma.MATURITYMONTHYEAR, 
ma.DTENABLED, ma.DTDISABLED,
@IDA, @DTINS
from dbo.DERIVATIVEATTRIB da
inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC and dc.IDDC = @IDDC
inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY and (ma.MATURITYDATE is null or ma.MATURITYDATE>=@DT) and (ma.IDMATURITYRULE=@IDMATURITYRULEOLD)
where not exists (select 1 from dbo.MATURITY ma_ins where ma_ins.IDMATURITYRULE = @IDMATURITYRULE and ma_ins.MATURITYMONTHYEAR = ma.MATURITYMONTHYEAR)";

                    QueryParameters qry = new QueryParameters(_cs, sqlQuery.ToString(), dp);
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    Cst.OTCml_TBL[] table = new Cst.OTCml_TBL[] { Cst.OTCml_TBL.MATURITY };
                    DataHelper.queryCache.Remove(ArrFunc.Map<Cst.OTCml_TBL, string>(table, (x) => { return x.ToString(); }), _cs, true);
                    DataEnabledHelper.ClearCache(_cs, table);
                    


                    // Etape 2 : [DERIVATIVEATTRIB] Requête SQL qui Update les Maturités (IDMATURITY) associées au DC
                    sqlQuery = @"update dbo.DERIVATIVEATTRIB
set IDMATURITY = (select ma_new.IDMATURITY
                from dbo.MATURITY ma_new
                inner join dbo.MATURITY ma on (ma.MATURITYMONTHYEAR = ma_new.MATURITYMONTHYEAR) and (ma.IDMATURITY = DERIVATIVEATTRIB.IDMATURITY)   
                where ma_new.IDMATURITYRULE = @IDMATURITYRULE),
IDAUPD = @IDA, DTUPD = @DTUPD
where exists 
(
    select 1 
    from dbo.DERIVATIVEATTRIB da
    inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.IDDC = @IDDC)    
    inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.MATURITYDATE is null or ma.MATURITYDATE>=@DT) and (ma.IDMATURITYRULE=@IDMATURITYRULEOLD)
    where ma.IDMATURITY = DERIVATIVEATTRIB.IDMATURITY and dc.IDDC = DERIVATIVEATTRIB.IDDC
)";

                    qry = new QueryParameters(_cs, sqlQuery.ToString(), dp);
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    Cst.OTCml_TBL[] tbl = new Cst.OTCml_TBL[] { Cst.OTCml_TBL.DERIVATIVEATTRIB };
                    DataHelper.queryCache.Remove(ArrFunc.Map<Cst.OTCml_TBL, string>(tbl, (x) => { return x.ToString(); }), _cs, true);
                    DataEnabledHelper.ClearCache(_cs, tbl);


                    DataHelper.CommitTran(dbTransaction, false);
                }
                catch
                {
                    if (DataHelper.IsTransactionValid(dbTransaction))
                        DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
        }

        /// <summary>
        /// Mise à jour des référentiels MATURITY 
        /// Calcul et déversement des dates dans la table MATURITY + envoi du message au service QUOTEHANDLING pour mise à jour des events associés
        /// </summary>
        /// <param name="IdNewMR"></param>
        /// <param name="processLog"></param>
        // RD 20200207 [25081] New
        public void UpdateMaturitiesAndEvents(int IdNewMR, out ProcessLogHeader processLog)
        {
            UpdateMaturitiesAndEvents(IdNewMR, out processLog, false);
        }

        /// <summary>
        /// Mise à jour des référentiels MATURITY 
        /// Calcul et déversement des dates dans la table MATURITY + envoi du message au service QUOTEHANDLING pour mise à jour des events associés
        /// </summary>
        /// <param name="pIdMR"></param>
        /// <param name="pProcessLog"></param>
        /// <param name="pIsUpdateDC"></param>
        /// RD 20200207 [25081] Modify
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        public void UpdateMaturitiesAndEvents(int pIdMR, out ProcessLogHeader pProcessLog, bool pIsUpdateDC)
        {
            bool isError = false;

            List<MQueueBase> listMQueue = new List<MQueueBase>();

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            //PL 20240229 Code déplacé avant la REGION ci-dessous, afin de ne plus engendrer d'écriture dans le tracker.
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            int idMarket = GetMarket(pIdMR);
            if (idMarket == 0)
                throw new InvalidOperationException("No Market Found"); //WARNING! Ne pas modifier ce message d'erreur, il est trappé dans le catch de OnConnectOk()
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

            #region Mise en place du tracker & Process Log + MQueueParameters pour infoTracker
            string query = "select IDENTIFIER from dbo.MATURITYRULE where IDMATURITYRULE = " + pIdMR;
            string identifierMatRule = DataHelper.ExecuteScalar(_cs, CommandType.Text, query).ToString();

            // Nouvelle instance de ProcessLog
            Tracker tracker = MaturityUpdateInitTrackerAndLog(new Pair<int, string>(pIdMR, identifierMatRule), out ProcessLog processLog);
            pProcessLog = processLog.header;

            ProcessLogAddDetail(processLog, "LOG-00800", new string[] { LogTools.IdentifierAndId(identifierMatRule, pIdMR) });
            #endregion

            string sqlSelectMaturity = @"select ma.IDMATURITY,ma.IDMATURITYRULE,ma.MATURITYMONTHYEAR,ma.MATURITYDATE,ma.MATURITYDATESYS,ma.MATURITYTIME,
ma.LASTTRADINGDAY,ma.LASTTRADINGTIME,
ma.DELIVERYDATE,ma.FIRSTDELIVERYDATE,ma.LASTDELIVERYDATE,ma.FIRSTDLVSETTLTDATE,ma.LASTDLVSETTLTDATE,
ma.PERIODMLTPDELIVERY,ma.PERIODDELIVERY,ma.DAYTYPEDELIVERY,ma.ROLLCONVDELIVERY,
ma.DELIVERYTIMESTART,ma.DELIVERYTIMEEND,ma.DELIVERYTIMEZONE,ma.ISAPPLYSUMMERTIME,
ma.PERIODMLTPDLVSETTLTOFFSET,ma.PERIODDLVSETTLTOFFSET,ma.DAYTYPEDLVSETTLTOFFSET,ma.SETTLTOFHOLIDAYDLVCONVENTION,
ma.FIRSTNOTICEDAY,ma.LASTNOTICEDAY,
ma.DTUPD,ma.IDAUPD
from dbo.MATURITY ma";

            string sqlWhereMaturity = SQLCst.WHERE + "(ma.IDMATURITYRULE=@IDMATURITYRULE)";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), pIdMR);
            if (DtFunc.IsDateTimeFilled(_dtMinMarket))
            {
                sqlWhereMaturity += SQLCst.AND + "(ma.MATURITYDATE is null or ma.MATURITYDATE>=@DT)";
                parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DT), _dtMinMarket); // FI 20201006 [XXXXX] DbType.Date
            }

            DataTable dtMaturity = DataHelper.ExecuteDataTable(_cs,  sqlSelectMaturity + sqlWhereMaturity, parameters.GetArrayDbParameter());
            dtMaturity.TableName = Cst.OTCml_TBL.MATURITY.ToString();

            #region Calcul des nouvelles dates (Expiry, LastTradingDay, Delivery, ...)
            bool isExistMaturity = (null != dtMaturity.Rows && dtMaturity.Rows.Count > 0);
            int modifiedRows, dtMaturityDateModifiedRows;
            modifiedRows = dtMaturityDateModifiedRows = 0;
            //PM 20140211 [19601] Liste pour la sauvegarde des DataRow des échéances pour lesquelles forcée l'envoie d'un message
            List<DataRow> dataRowMaturityForced = new List<DataRow>();
            if (!isExistMaturity)
            {
                ProcessLogAddDetail(processLog, "LOG-00801", new string[] { LogTools.IdentifierAndId(identifierMatRule, pIdMR) });

                
            }
            else
            {
                IProductBase product = Tools.GetNewProductBase();
                //SQL_MaturityRule sql_MaturityRule = new SQL_MaturityRule(pCS, idData);
                //PL 20131112 [TRIM 19164]
                SQL_MaturityRuleRead sql_MaturityRule = new SQL_MaturityRuleRead(_cs, pIdMR);
                SQL_Market sqlMarket = new SQL_Market(CSTools.SetCacheOn(_cs), idMarket);

                CalcMaturityRuleDate calc = new CalcMaturityRuleDate(_cs, product, (sqlMarket.Id, sqlMarket.IdBC), new MaturityRule(sql_MaturityRule));

                for (int item = 0; item < dtMaturity.Rows.Count; item++)
                {
                    #region item

                    /* Calcul de la MATURITYDATE */
                    string maturityMonthYear = dtMaturity.Rows[item]["MATURITYMONTHYEAR"].ToString();
                    (DateTime MaturityDateSys, DateTime MaturityDate) maturity = calc.Calc_MaturityDate(maturityMonthYear, out DateTime dtRolledDate);

                    // RD 20200207 [25081] Modification de toutes les dates de l'échéance dans les cas suivants:
                    // - Modification d’une MR dans le référentiel MR
                    // - Les nouvelles Maturity créées dans le cadre de la fonctionnalité de modification de la MR d'un DC
                    // - La nouvelle date calculée « Date d'expiration (règlementaire)» (colonne MATURITYDATESYS) diffère de la date existante sur la Maturity
                    bool isModifyMaturity = (!pIsUpdateDC) || (dtMaturity.Rows[item]["MATURITYDATESYS"] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item]["MATURITYDATESYS"]) != maturity.MaturityDateSys);

                    // FI 20151022 [21476] Add if 
                    // RD 20200207 [25081] Add isModify
                    // RD 20201019 [25517] Vérifier s'il y a des trades avec de mauvaises dates même s'il n'y a pas de modification des dates d'échéances
                    if (DtFunc.IsDateTimeFilled(maturity.MaturityDateSys))
                    {

                        bool isModifiedData = false;

                        if (isModifyMaturity)
                        {
                            /* Calcul de la DELIVERYDATE ou des FIRST/LAST DELIVERYDATE */
                            DateTime dtDeliveryDate = DateTime.MinValue;

                            MaturityPeriodicDeliveryCharacteristics mpdc = new MaturityPeriodicDeliveryCharacteristics();
                            if (sql_MaturityRule.IsNoPeriodicDeliveryExtend)
                            {
                                /* Calcul de la DELIVERYDATE (NO PERIODIC) */
                                dtDeliveryDate = calc.Calc_MaturityDeliveryDate(maturity);
                            }
                            else
                            {
                                /* Calcul des FIRST/LAST DELIVERYDATE (PERIODIC) */
                                mpdc = calc.Calc_MaturityPeriodicDeliveryDates(maturityMonthYear);
                            }

                            /* Calcul de la LASTTRADINGDATE */
                            DateTime dtLastTradingDate = calc.Calc_MaturityLastTradingDay(maturity.MaturityDateSys, dtRolledDate);

                            if (DtFunc.IsDateTimeEmpty(dtLastTradingDate) && DtFunc.IsDateTimeFilled(mpdc.dates.dtFirstDlvSettlt))
                            {
                                //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                //PL 20170221 WARNING: Afin de répondre au besoin des ETD sur ENERGY (ex. PEGAS, EEX) pour lesquels le prix utilisé pour le
                                //                     calcul des cash-flows de livraison est le prix observé le 1er jour de règlement.          
                                //                     On met ici le LTD égal à cette date et on peut ainsi paramétrer la lecture du prix de clôture
                                //                     en date LTD, afin de répondre au besoin. (celà en attendant une évolution du référentiel MR).
                                //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                dtLastTradingDate = mpdc.dates.dtFirstDlvSettlt;
                                //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            }

                            string colName;
                            dtMaturity.Rows[item].BeginEdit();

                            // RD 20200207 [25081] Traiter la date MATURITYDATESYS indépendamment de la date MATURITYDATE
                            colName = "MATURITYDATESYS";
                            if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != maturity.MaturityDateSys))
                            {
                                dtMaturityDateModifiedRows++;
                                isModifiedData = true;
                                dtMaturity.Rows[item][colName] = maturity.MaturityDateSys;
                            }
                            colName = "MATURITYDATE";
                            if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != maturity.MaturityDate))
                            {
                                dtMaturityDateModifiedRows++;
                                isModifiedData = true;
                                dtMaturity.Rows[item][colName] = maturity.MaturityDate;
                                //dtMaturity.Rows[item]["MATURITYDATESYS"] = dtMaturityDateSys;
                            }
                            colName = "LASTTRADINGDAY";
                            if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != dtLastTradingDate))
                            {
                                // RD 20191031 [25022] Add test on dtLastTradingDate
                                //isModifiedData = true;
                                //dtMaturity.Rows[item][colName] = dtLastTradingDate;
                                if (DtFunc.IsDateTimeFilled(dtLastTradingDate))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = dtLastTradingDate;
                                }
                                else if (dtMaturity.Rows[item][colName] != Convert.DBNull)
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = Convert.DBNull;
                                }
                            }
                            if (sql_MaturityRule.IsNoPeriodicDeliveryExtend)
                            {
                                colName = "DELIVERYDATE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != dtDeliveryDate))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = dtDeliveryDate;
                                }
                                //
                                dtMaturity.Rows[item]["FIRSTDELIVERYDATE"] = Convert.DBNull;
                                dtMaturity.Rows[item]["LASTDELIVERYDATE"] = Convert.DBNull;
                                dtMaturity.Rows[item]["FIRSTDLVSETTLTDATE"] = Convert.DBNull;
                                dtMaturity.Rows[item]["LASTDLVSETTLTDATE"] = Convert.DBNull;
                                dtMaturity.Rows[item]["PERIODMLTPDELIVERY"] = Convert.DBNull;
                                dtMaturity.Rows[item]["PERIODDELIVERY"] = Convert.DBNull;
                                dtMaturity.Rows[item]["DAYTYPEDELIVERY"] = Convert.DBNull;
                                dtMaturity.Rows[item]["ROLLCONVDELIVERY"] = Convert.DBNull;
                                dtMaturity.Rows[item]["DELIVERYTIMESTART"] = Convert.DBNull;
                                dtMaturity.Rows[item]["DELIVERYTIMEEND"] = Convert.DBNull;
                                dtMaturity.Rows[item]["DELIVERYTIMEZONE"] = Convert.DBNull;
                                dtMaturity.Rows[item]["ISAPPLYSUMMERTIME"] = false; //Column not nullable
                                dtMaturity.Rows[item]["PERIODMLTPDLVSETTLTOFFSET"] = Convert.DBNull;
                                dtMaturity.Rows[item]["PERIODDLVSETTLTOFFSET"] = Convert.DBNull;
                                dtMaturity.Rows[item]["DAYTYPEDLVSETTLTOFFSET"] = Convert.DBNull;
                                dtMaturity.Rows[item]["SETTLTOFHOLIDAYDLVCONVENTION"] = Convert.DBNull;
                            }
                            else
                            {
                                dtMaturity.Rows[item]["DELIVERYDATE"] = Convert.DBNull;
                                //
                                colName = "FIRSTDELIVERYDATE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != mpdc.dates.dtFirstDelivery))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = mpdc.dates.dtFirstDelivery;
                                }
                                colName = "LASTDELIVERYDATE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != mpdc.dates.dtLastDelivery))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = mpdc.dates.dtLastDelivery;
                                }
                                colName = "FIRSTDLVSETTLTDATE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != mpdc.dates.dtFirstDlvSettlt))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = mpdc.dates.dtFirstDlvSettlt;
                                }
                                colName = "LASTDLVSETTLTDATE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToDateTime(dtMaturity.Rows[item][colName]) != mpdc.dates.dtLastDlvSettlt))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = mpdc.dates.dtLastDlvSettlt;
                                }
                                colName = "PERIODMLTPDELIVERY";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToInt32(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateMultiplier))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateMultiplier;
                                }
                                colName = "PERIODDELIVERY";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDatePeriod))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDatePeriod;
                                }
                                colName = "DAYTYPEDELIVERY";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateDaytype))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateDaytype;
                                }
                                colName = "ROLLCONVDELIVERY";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateRollConv))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateRollConv;
                                }
                                colName = "DELIVERYTIMESTART";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateTimeStart))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateTimeStart;
                                }
                                colName = "DELIVERYTIMEEND";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateTimeEnd))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateTimeEnd;
                                }
                                colName = "DELIVERYTIMEZONE";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateTimeZone))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateTimeZone;
                                }
                                colName = "ISAPPLYSUMMERTIME";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToBoolean(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDeliveryDateApplySummerTime))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDeliveryDateApplySummerTime;
                                }
                                colName = "PERIODMLTPDLVSETTLTOFFSET";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToInt32(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetMultiplier))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetMultiplier;
                                }
                                colName = "PERIODDLVSETTLTOFFSET";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetPeriod))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetPeriod;
                                }
                                colName = "DAYTYPEDLVSETTLTOFFSET";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetDaytype))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDlvSettltDateOffsetDaytype;
                                }
                                colName = "SETTLTOFHOLIDAYDLVCONVENTION";
                                if ((dtMaturity.Rows[item][colName] == Convert.DBNull) || (Convert.ToString(dtMaturity.Rows[item][colName]) != sql_MaturityRule.MaturityPeriodicDlvSettltHolidayConv))
                                {
                                    isModifiedData = true;
                                    dtMaturity.Rows[item][colName] = sql_MaturityRule.MaturityPeriodicDlvSettltHolidayConv;
                                }
                            }
                        }

                        //PM 20140211 [19601] Vérifier s'il y a des trades renseignés avec de mauvaises dates dans le cas où il n'y a pas de modification des dates d'échéances
                        if (false == isModifiedData)
                        {
                            //PM 20140211 [19601] Requête pour la recherche de trades portant sur l'échéance à calculer et dont les dates d'échéances sont différentes.
                            //PM 20140431 Remplacement SELECT_TOP1 par COUNT_1 pour compatibilité Oracle
                            //string sqlSelectTradeMaturity = SQLCst.SELECT_TOP1 + " 1" + Cst.CrLf;
                            string sqlSelectTradeMaturity = SQLCst.SELECT + SQLCst.COUNT_1 + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE.ToString() + " t" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i" + SQLCst.ON + "(i.IDI = t.IDI)" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT.ToString() + " p" + SQLCst.ON + "(p.IDP = i.IDP)" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ASSET_ETD.ToString() + " a" + SQLCst.ON + "(a.IDASSET = t.IDASSET)" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString() + " da" + SQLCst.ON + "(da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MATURITY.ToString() + " m" + SQLCst.ON + "(m.IDMATURITY = da.IDMATURITY)" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.WHERE + "(p.FAMILY = '" + Cst.ProductFamily_LSD.ToString() + "')" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.AND + "(p.GPRODUCT = '" + Cst.ProductGProduct_FUT.ToString() + "')" + Cst.CrLf;
                            sqlSelectTradeMaturity += SQLCst.AND + "(m.IDMATURITY = @IDMATURITY)" + Cst.CrLf;
                            // EG 20210330 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
                            sqlSelectTradeMaturity += SQLCst.AND + "(t.DTOUTUNADJ != (case when m.MATURITYDATE is null then m.MATURITYDATESYS else m.MATURITYDATE end))" + Cst.CrLf;
                            DataParameters tradeMaturityParameters = new DataParameters();
                            int idMaturity = Convert.ToInt32(dtMaturity.Rows[item]["IDMATURITY"]);
                            tradeMaturityParameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITY), idMaturity);

                            QueryParameters queryTradeMaturity = new QueryParameters(_cs, sqlSelectTradeMaturity, tradeMaturityParameters);

                            //PM 20140431 Remplacement de ExecuteReader par ExecuteScalar suite au remplacement SELECT_TOP1 par COUNT_1 dans la requête pour compatibilité Oracle
                            //dr = DataHelper.ExecuteReader(pCS, CommandType.Text, queryTradeMaturity.Query, queryTradeMaturity.Parameters.GetArrayDbParameter());
                            //isModifiedData = dr.Read();
                            object countTrade = DataHelper.ExecuteScalar(_cs, CommandType.Text, queryTradeMaturity.Query, queryTradeMaturity.Parameters.GetArrayDbParameter());
                            isModifiedData = ((countTrade != null) && (Convert.ToInt32(countTrade) != 0));
                            if (isModifiedData)
                            {
                                dtMaturityDateModifiedRows += 1;
                                dataRowMaturityForced.Add(dtMaturity.Rows[item]);
                            }
                        }

                        if (isModifiedData)
                        {
                            modifiedRows++;
                            // FI 20200820 [25468] date systemes en UTC
                            dtMaturity.Rows[item]["DTUPD"] = _dtSys;
                            dtMaturity.Rows[item]["IDAUPD"] = _idA;
                            dtMaturity.Rows[item].EndEdit();
                        }
                        else
                        {
                            dtMaturity.Rows[item].CancelEdit();
                        }
                    }
                    #endregion
                }
            }
            #endregion Calcul des nouvelles dates

            if (isExistMaturity)
            {
                if (modifiedRows == 0)
                {
                    ProcessLogAddDetail(processLog, "LOG-00802", new string[] { LogTools.IdentifierAndId(identifierMatRule, pIdMR) });
                }
                else
                {
                    MQueueDataset mQueueDataset = new MQueueDataset(_cs, dtMaturity.TableName);

                    #region Constitution des objets utiles aux messages à destination de QuoteHandling
                    if (dtMaturityDateModifiedRows > 0)
                    {
                        //NB: A l'image de la maj du référentiel MATURITY, donc uniquement si la MATURITYDATE a donné lieu à modification
                        mQueueDataset.Prepare(dtMaturity);

                        // PM 20140211 [19601] Forcer l'envoie d'un message pour les échéances non modifiées mais pour lesquelles il existe des trades dont modifier les dates des événements
                        foreach (DataRow row in dataRowMaturityForced)
                        {
                            mQueueDataset.AddMaturity(row, DataRowVersion.Current, DataRowState.Modified);
                        }
                    }
                    foreach (object oMaturity in mQueueDataset.ObjDatas)
                    {
                        Maturity maturity = (Maturity)oMaturity;
                        // FI 20200820 [25468] date systemes en UTC
                        MQueueAttributes mQueueAttributes = new MQueueAttributes()
                        {
                            connectionString = _cs,
                            requester = new MQueueRequester(tracker.IdTRK_L, tracker.Request.Session, tracker.Request.DtRequest)
                        };
                        QuotationHandlingMQueue mQueue = new QuotationHandlingMQueue(maturity, mQueueAttributes);
                        listMQueue.Add(mQueue);
                    }
                    #endregion

                    #region Déversement des nouvelles dates dans la table physique MATURITY
                    int updatedRows = DataHelper.ExecuteDataAdapter(_cs, sqlSelectMaturity, dtMaturity);
                    if (modifiedRows != updatedRows)
                    {

                        isError = true;
                        ProcessLogAddDetail(processLog, "LOG-00803",
                                new string[] { LogTools.IdentifierAndId(identifierMatRule, pIdMR), modifiedRows.ToString(), updatedRows.ToString() });
                    }
                    #endregion

                    #region Postage des messages à destination de QuoteHandling
                    if (!isError)
                    {

                        ProcessLogAddDetail(processLog, "LOG-00804",
                            new string[] { LogTools.IdentifierAndId(identifierMatRule, pIdMR), listMQueue.Count.ToString() });

                        
                        //Envoi des messages Mqueue générés
                        if (ArrFunc.IsFilled(listMQueue))
                        {
                            
                            tracker.UpdatePostedMsg(listMQueue.Count, _session);

                            MQueueTaskInfo taskInfo = new MQueueTaskInfo
                            {
                                connectionString = _cs,
                                Session = _session,
                                process = Cst.ProcessTypeEnum.QUOTHANDLING,
                                mQueue = listMQueue.ToArray(),
                            };


                            int idTRK_L = tracker.IdTRK_L;
                            MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);
                        }
                    }
                    #endregion
                }
            }

            ProcessStateTools.StatusEnum statut =
                    isError ? ProcessStateTools.StatusEnum.ERROR :
                    (modifiedRows > 0 ? ProcessStateTools.StatusEnum.SUCCESS : ProcessStateTools.StatusEnum.NONE);

            processLog.SetHeaderStatus(statut);
            processLog.SQLUpdateHeader();

            //FI 20190605 [XXXXX] Mise à jour du tracker si aucune demande n'est effectué à SpheresQuotationHandling
            if (listMQueue.Count == 0)
            {
                tracker.ReadyState = ProcessStateTools.ReadyStateEnum.TERMINATED;
                tracker.Status = statut;
                tracker.Update(_session);
            }
        }

        /// <summary>
        /// Liste des DC candidats à mettre à jour 
        /// </summary>
        /// <param name="pIdNewMR">Représente le nouvelle MR dite principale injectée dans EFSSOFTWAREUPG </param>
        /// <returns></returns>
        private List<Tuple<int, int>> GetDetivativeContractToUpdate(int pIdNewMR)
        {
            List<Tuple<int, int>> lst = new List<Tuple<int, int>>();

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DT), _dtMinMarket); // FI 20201006 [XXXXX] DbType.Date
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), pIdNewMR);

            string query = @"
select distinct dc.IDDC, ma.IDMATURITYRULE as IDMATURITYRULE_MAIN
from dbo.DERIVATIVEATTRIB da
inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC and dc.IDMATURITYRULE = @IDMATURITYRULE
inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY and (ma.MATURITYDATE is null or ma.MATURITYDATE>=@DT)
left outer join dbo.DRVCONTRACTMATRULE maAdd on  maAdd.IDDC = dc.IDDC and maAdd.IDMATURITYRULE = ma.IDMATURITYRULE
where ma.IDMATURITYRULE != @IDMATURITYRULE  and ma.IDMATURITYRULE != isnull(maAdd.IDMATURITYRULE,0)";

            QueryParameters sqlQuery = new QueryParameters(_cs, query, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(_cs, CommandType.Text, sqlQuery.Query, sqlQuery.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                    lst.Add(new Tuple<int, int>(Convert.ToInt32(dr["IDDC"]), Convert.ToInt32(dr["IDMATURITYRULE_MAIN"])));
            }

            return lst;
        }

        /// <summary>
        /// Initialisation du tracker + Process Log
        /// </summary>
        /// <param name="pProcessLog"></param>
        /// <returns></returns>
        private Tracker MaturityUpdateInitTrackerAndLog(Pair<int, string> pIdNewMaturityRule, out ProcessLog pProcessLog)
        {
            Cst.ProcessTypeEnum processType = Cst.ProcessTypeEnum.MATURITYRULEUPDATE;
            IdMenu.Menu menu = IdMenu.Menu.DerivativeContractUpdateMaturityRule;

            MQueueparameters mqParameters = new MQueueparameters();
            MQueueparameter parameter;
            parameter = new MQueueparameter("IDDATA", TypeData.TypeDataEnum.@int);
            parameter.SetValue(pIdNewMaturityRule.First);
            mqParameters.Add(parameter);
            parameter = new MQueueparameter("IDDATAIDENT", TypeData.TypeDataEnum.@string);
            parameter.SetValue(Cst.OTCml_TBL.MATURITYRULE.ToString());
            mqParameters.Add(parameter);
            parameter = new MQueueparameter("IDDATAIDENTIFIER", TypeData.TypeDataEnum.@string);
            parameter.SetValue(pIdNewMaturityRule.Second);
            mqParameters.Add(parameter);
            parameter = new MQueueparameter("DATA1", TypeData.TypeDataEnum.@string);
            parameter.SetValue(pIdNewMaturityRule.Second);
            mqParameters.Add(parameter);
            parameter = new MQueueparameter("DATA2", TypeData.TypeDataEnum.@string);
            parameter.SetValue("Modified");
            mqParameters.Add(parameter);


            TrackerAttributes TrackerAttrib = new TrackerAttributes()
            {
                process = processType,
                gProduct = Cst.ProductGProduct_FUT,
                caller = IdMenu.GetIdMenu(menu),
                info = TrackerAttributes.BuildInfo(processType, mqParameters)
            };

            Tracker tracker = new Tracker(_cs)
            {
                ProcessRequested = processType,
                ReadyState = ProcessStateTools.ReadyStateEnum.ACTIVE,
                Status = ProcessStateTools.StatusEnum.PROGRESS,
                Group = TrackerAttrib.BuildTrackerGroup(),
                IdData = TrackerAttrib.BuildTrackerIdData(),
                Data = TrackerAttrib.BuildTrackerData()
            };

            tracker.Ack.SetAckWebSessionSchedule();


            tracker.Insert(_session, 0);

            pProcessLog = new ProcessLog(_cs, processType, _session);
            pProcessLog.header.Info.message = Ressource.GetString(processType.ToString());
            pProcessLog.header.IdTRK_L = tracker.IdTRK_L;
            pProcessLog.header.Info.status = ProcessStateTools.StatusEnum.PROGRESS.ToString();
            // PM 20200102 [XXXXX] New Log
            //pProcessLog.header.Info.code = "TRK";
            //pProcessLog.header.Info.number = 400;
            pProcessLog.header.Info.SetSysMsgCode(new SysMsgCode(SysCodeEnum.TRK, 400));
            pProcessLog.SQLWriteHeader();

            return tracker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessLog"></param>
        /// <param name="pMsg"></param>
        /// <param name="pArg"></param>
        private static void ProcessLogAddDetail(ProcessLog pProcessLog, string pMsg, string[] pArg)
        {
            if (null != pProcessLog)
            {
                List<String> lstInfo = new List<string>() { pMsg };
                if (ArrFunc.IsFilled(pArg))
                    lstInfo.AddRange(pArg);

                pProcessLog.AddDetail(lstInfo.ToArray(), LogLevelEnum.Info);
                pProcessLog.SQLWriteDetail();
            }
        }

        /// <summary>
        /// Lecture du Marché du premier DC (de préférence enabled) qui référence la MR <paramref name="idMR"/>
        /// </summary>
        /// <param name="idMR"></param>
        /// <returns></returns>
        private int GetMarket(int idMR)
        {
            int idMarket = 0;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(_cs, "DT", DbType.Date), _dtMinMarket);  // FI 20201006 [XXXXX] DbType.Date
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDMATURITYRULE), idMR);

            string sqlDC = $@"select '1' as Sort, IDM
from dbo.VW_DRVCONTRACTMATRULE dcMR
inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = dcMR.IDDC  
where dcMR.IDMATURITYRULE = @IDMATURITYRULE and {OTCmlHelper.GetSQLDataDtEnabled(_cs, "dc", _dtMinMarket)}
union all
select '2' as Sort, IDM
from dbo.VW_DRVCONTRACTMATRULE dcMR
inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = dcMR.IDDC
where dcMR.IDMATURITYRULE = @IDMATURITYRULE
order by 1";

            QueryParameters sqlQuery = new QueryParameters(_cs, sqlDC, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(_cs, CommandType.Text, sqlQuery.Query, sqlQuery.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    idMarket = Convert.ToInt32(dr["IDM"]);
                }
            }

            return idMarket;
        }
    }
}
