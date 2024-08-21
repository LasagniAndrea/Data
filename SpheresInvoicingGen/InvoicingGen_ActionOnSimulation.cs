using EFS.ACommon;
using EFS.Administrative.Invoicing;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.TradeInformation;
using System;
using System.Data;
using System.Reflection;
using static EFS.TradeInformation.TradeCommonCaptureGen;

namespace EFS.Process
{
    /// <summary>
    /// Traitement d'une demande de : 
    /// - Suppression d'une facture simulée (Cst.ProcessTypeEnum.INVCANCELSIMUL)
    /// - Validation d'une facture simulée (Cst.ProcessTypeEnum.INVVALIDSIMUL)
    /// </summary>
    /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
    public class InvoicingGen_ActionOnSimulation : InvoicingGenProcessBase
    {
        private readonly Cst.ProcessTypeEnum m_ProcessType;
        private LockObject m_LockObject;
        public InvoicingGen_ActionOnSimulation(InvoicingGenProcess pInvoicingGenProcess)
            : base(pInvoicingGenProcess)
        {
            m_ProcessType = this.Queue.ProcessType;
        }

        /// <summary>
        /// Traitement du message de
        /// - Suppression d'une facture simulée (Cst.ProcessTypeEnum.INVCANCELSIMUL)
        /// - Validation d'une facture simulée (Cst.ProcessTypeEnum.INVVALIDSIMUL)
        /// </summary>
        /// <returns></returns>
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        public override Cst.ErrLevel Generate()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            int idT = ProcessBase.CurrentId;
            string identifier = ProcessBase.MQueue.Identifier;
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(ProcessBase.Cs);

            // Lock du trade administratif
            m_LockObject = ProcessBase.LockElement(TypeLockEnum.TRADE, idT, identifier, true, LockTools.Exclusive);
            if (null != m_LockObject)
            {
                IDbTransaction dbTransaction = null;
                try
                {
                    dbTransaction = DataHelper.BeginTran(ProcessBase.Cs);
                    string newIdentifier = string.Empty;
                    switch (m_ProcessType)
                    {
                        case Cst.ProcessTypeEnum.INVCANCELSIMUL:

                            // 1. Suppression des événements de la facture simulée
                            TradeRDBMSTools.DeleteEvent(ProcessBase.Cs, dbTransaction, idT, Cst.ProductGProduct_ADM, ProcessBase.Session.IdA, dtSys);
                            // 2. Suppression de la facture simulée
                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(ProcessBase.Cs, "IDT", DbType.Int32), idT);
                            string sqlDelete = SQLCst.SQL_ANSI + Cst.CrLf + "delete from dbo.TRADE where (IDT = @IDT);" + Cst.CrLf;
                            QueryParameters qryParameters = new QueryParameters(ProcessBase.Cs, sqlDelete, parameters);
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            break;

                        case Cst.ProcessTypeEnum.INVVALIDSIMUL:

                            // 1. Mise à jour du statut
                            UpdateTradeAdminStEnvironment(dbTransaction, idT, dtSys);
                            // 2. Mise à jour de l'identifiant
                            newIdentifier = UpdateTradeIdentifier(dbTransaction, idT);
                            // 3. Mise à jour des liens
                            UpdateTradeLink(dbTransaction, idT, identifier, newIdentifier);
                            // 4. Mise à jour de TRADETRAIL
                            string screenName = ProcessBase.MQueue.GetStringValueParameterById("SCREENNAME");
                            TradeRDBMSTools.SaveTradeTrail(ProcessBase.Cs, dbTransaction, idT, screenName, Cst.Capture.ModeEnum.Update, dtSys,
                                ProcessBase.Session.IdA, ProcessBase.Session, ProcessBase.Tracker.IdTRK_L, ProcessBase.Tracker.IdProcess);

                            break;
                    }

                    DataHelper.CommitTran(dbTransaction);

                    switch (m_ProcessType)
                    {
                        case Cst.ProcessTypeEnum.INVCANCELSIMUL:
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5246), 0,
                                new LogParam(LogTools.IdentifierAndId(identifier, idT))));
                            Logger.Write();
                            break;
                        case Cst.ProcessTypeEnum.INVVALIDSIMUL:
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5245), 0,
                                new LogParam(LogTools.IdentifierAndId(identifier, idT)),
                                new LogParam(LogTools.IdentifierAndId(newIdentifier, idT))));
                            Logger.Write();
                            break;
                    }
                    return codeReturn;
                }
                catch (Exception)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);

                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5226), 0,
                        new LogParam(Ressource.GetString(m_ProcessType.ToString())),
                        new LogParam(LogTools.IdentifierAndId(Queue.identifier, Queue.id)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(MasterDate))));
                    throw;
                }
            }
            else
            {
                codeReturn = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5232), 0, new LogParam(LogTools.IdentifierAndId(identifier, idT))));
            }
            return codeReturn;
        }

        /// <summary>
        /// Mise à jour du statut d'environnement lors de la validation
        /// d'une facture simulée (SIMUL -> REGULAR)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id de la facture simulée à valider</param>
        /// <param name="pDateSys">Date système de validation</param>
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        private void UpdateTradeAdminStEnvironment(IDbTransaction pDbTransaction, int pIdT, DateTime pDateSys)
        {
            SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(ProcessBase.Cs, pIdT)
            {
                DbTransaction = pDbTransaction
            };
            string sqlQuery = sqlTradeStSys.GetQueryParameters(
                    new string[] { "IDT", "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT", "ROWATTRIBUT" }).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(ProcessBase.Cs, pDbTransaction, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];
            DataRow dr = dt.Rows[0];
            dr.BeginEdit();
            dr["IDSTENVIRONMENT"] = Cst.StatusEnvironment.REGULAR;
            dr["DTSTENVIRONMENT"] = pDateSys;
            dr["IDASTENVIRONMENT"] = ProcessBase.Session.IdA;
            dr.EndEdit();
            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlQuery, dt);
        }

        /// <summary>
        /// Mise à jour de l'identifiant de la facture simulée lors de 
        /// sa validation
        /// - Mise à jour dans TRADE
        /// - Mise à jour dans TRADEXML
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id de la facture simulée à valider</param>
        /// <returns></returns>
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        private string UpdateTradeIdentifier(IDbTransaction pDbTransaction, int pIdT)
        {
            int idI = ProcessBase.MQueue.GetIntValueParameterById("IDI");
            int idA_Entity = ProcessBase.MQueue.GetIntValueParameterById("IDA_ENTITY");
            DateTime dtTrade = ProcessBase.MQueue.GetDateTimeValueParameterById("DTTRADE");
            string idStPriority = ProcessBase.MQueue.GetStringValueParameterById("IDSTPRIORITY");
            string idStActivation = ProcessBase.MQueue.GetStringValueParameterById("IDSTACTIVATION");

            TradeStatus tradeStatus = new TradeStatus
            {
                stEnvironmentSpecified = true,
                stPrioritySpecified = StrFunc.IsFilled(idStPriority),
                stActivationSpecified = StrFunc.IsFilled(idStActivation),
                stEnvironment = new Status.Status()
                {
                    CurrentSt = Cst.StatusEnvironment.REGULAR.ToString()
                }
            };
            if (tradeStatus.stPrioritySpecified)
                tradeStatus.stPriority.CurrentSt = idStPriority.ToString();
            if (tradeStatus.stActivationSpecified)
                tradeStatus.stActivation.CurrentSt = idStActivation.ToString();

            SQL_Instrument sql_Instrument = new SQL_Instrument(ProcessBase.Cs, idI)
            {
                DbTransaction = pDbTransaction
            };

            TradeRDBMSTools.BuildTradeIdentifier(ProcessBase.Cs, pDbTransaction, sql_Instrument, idA_Entity, tradeStatus, dtTrade, dtTrade, 
                out string newIdentifier, out string prefix, out string suffix);
            newIdentifier = prefix + newIdentifier + suffix;

            SQL_Actor sql_Actor = new SQL_Actor(ProcessBase.Cs, idA_Entity)
            {
                DbTransaction = pDbTransaction
            };
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(ProcessBase.Cs, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(ProcessBase.Cs, "NEWIDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), newIdentifier);

            string sqlUpdate = @"update dbo.TRADE set IDENTIFIER=@NEWIDENTIFIER where (IDT = @IDT);";
            QueryParameters qryParameters = new QueryParameters(ProcessBase.Cs, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (DataHelper.IsDbSqlServer(ProcessBase.Cs))
            {
                parameters.Add(new DataParameter(ProcessBase.Cs, "XMLID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), sql_Actor.XmlId);

                string xQuery = OTCmlHelper.GetXMLNamespace_3_0(ProcessBase.Cs);
                xQuery += @"replace value of (efs:EfsML/trade/tradeHeader/partyTradeIdentifier";
                xQuery += @"[partyReference[@href=sql:variable(""@XMLID"")]]/tradeId[1]/text())[1] with sql:variable(""@NEWIDENTIFIER"")";

                sqlUpdate = $"update dbo.TRADEXML set TRADEXML.modify('{xQuery}') where (IDT = @IDT)";
            }
            else if (DataHelper.IsDbOracle(ProcessBase.Cs))
            {
                sqlUpdate = @"update dbo.TRADEXML set TRADEXML = UPDATEXML(TRADEXML,'efs:EfsML/fpml:trade/fpml:tradeHeader/fpml:partyTradeIdentifier";
                sqlUpdate += $"[fpml:partyReference[@href='{sql_Actor.XmlId}']]/fpml:tradeId[1]/text()','{newIdentifier}',";
                sqlUpdate += @"'xmlns:efs=""http://www.efs.org/2007/EFSmL-3-0"", xmlns:fpml=""http://www.fpml.org/2007/FpML-4-4""')";
                sqlUpdate += " where (IDT=@IDT)";
            }
            qryParameters = new QueryParameters(ProcessBase.Cs, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return newIdentifier;
        }

        /// <summary>
        /// Mise à jour des données d'une facture simulée lors de sa validation
        /// dans la table TRADELINK
        /// - DATA2 : Nouvel identifiant de la facture validée
        /// - DATA3 : Ancien identifiant de la facture simulée
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id de la facture simulée à valider</param>
        /// <param name="pIdentifier">Ancien IDENTIFIER (facture simulée)</param>
        /// <param name="pNewIdentifier">Nouvel IDENTIFIER (facture validée)</param>
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        private void UpdateTradeLink(IDbTransaction pDbTransaction, int pIdT, string pIdentifier, string pNewIdentifier)
        {
            SQL_TradeLink sqlTradeLink = new SQL_TradeLink(ProcessBase.Cs, pIdT)
            {
                DbTransaction = pDbTransaction
            };
            string sqlQuery = sqlTradeLink.GetQueryParameters(new string[] { "VW_TRADELINK.IDT_L", "IDT_A", "IDT_B", "DATA1", "DATA2" }).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(ProcessBase.Cs, pDbTransaction, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                string sqlData = string.Empty;
                if (pIdT == Convert.ToInt32(row["IDT_A"]))
                {
                    if ((false == Convert.IsDBNull(row["DATA1"])) && (pIdentifier == row["DATA1"].ToString()))
                        sqlData += SQLCst.SET + "DATA1=" + DataHelper.SQLString(pNewIdentifier) + Cst.CrLf;
                }
                if (pIdT == Convert.ToInt32(row["IDT_B"]))
                {
                    if ((false == Convert.IsDBNull(row["DATA2"])) && (pIdentifier == row["DATA2"].ToString()))
                    {
                        sqlData += StrFunc.IsEmpty(sqlData) ? SQLCst.SET : ",";
                        sqlData += "DATA2=" + DataHelper.SQLString(pNewIdentifier) + Cst.CrLf;
                    }
                }
                if (StrFunc.IsFilled(sqlData))
                {
                    string identification = EFS.TradeLink.TradeLinkDataIdentification.OldIdentifier.ToString();
                    sqlData += ", DATA3IDENT =" + DataHelper.SQLString(identification) + ", DATA3 = " + DataHelper.SQLString(pIdentifier);

                    string sqlUpdate = $@"update dbo.TRADELINK {Cst.CrLf} {sqlData}
                    where (IDT_L = {row["IDT_L"]}) and (IDT_A = {row["IDT_A"]}) and (IDT_B = { row["IDT_B"]})";
                    _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlUpdate);
                }
            }
        }
    }
}
