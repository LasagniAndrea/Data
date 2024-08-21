using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeInformation;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
//
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EFS.Process
{
    /// <summary>
    /// Gestion des frais sur un trade
    /// </summary>
    /// FI 20160907 [21831] Add
    public class TradeActionGenFeesCalculation : TradeActionGenProcessBase
    {
        #region Constructors
        /// <summary>
        /// Gestion des frais sur un trade
        /// </summary>
        /// <param name="pTradeActionGenProcess">process courant</param>
        /// <param name="pDsTrade">Dataset Trade</param>
        /// <param name="pTradeLibrary">Represente le trade</param>
        /// <param name="pTradeAction">Message Queue</param>
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenFeesCalculation(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
        {
            //CodeReturn = Valorize();
        }
        #endregion Constructors

        /// <summary>
        /// 
        /// </summary>
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction, m_DsTrade.IdT);
            m_DsEvents.Load(EventTableEnum.None, null, null);
        }

       
        
        /// <summary>
        ///  Démarge du processus 
        /// </summary>
        /// <returns></returns>
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel codeReturn;
            if (ArrFunc.IsFilled(m_TradeAction.ActionMsgs))
            {
                if (m_TradeAction.ActionMsgs[0].GetType().Equals(typeof(FeesCalculationSettingsMode1)))
                {
                    FeesCalculationSettingsMode1 feesCalculationSetting = (FeesCalculationSettingsMode1)m_TradeAction.ActionMsgs[0];
                    codeReturn = FeeCalculationAndWrite(feesCalculationSetting);
                }
                else if (m_TradeAction.ActionMsgs[0].GetType().Equals(typeof(FeesCalculationSettingsMode2)))
                {
                    FeesCalculationSettingsMode2 feesCalculationSetting = (FeesCalculationSettingsMode2)m_TradeAction.ActionMsgs[0];
                    feesCalculationSetting.trade = new KeyValuePair<int, string>(m_TradeActionGenMQueue.id, m_TradeActionGenMQueue.identifier);;
                    codeReturn = FeeCalculationAndWrite(feesCalculationSetting);
                }
                else
                {
                    throw new NotSupportedException(StrFunc.AppendFormat("type {0} is not supported", m_TradeAction.ActionMsgs[0].GetType().ToString()));
                }
            }
            else
            {
                throw new NotImplementedException("m_TradeAction.ActionMsg is empty");
            }

            return codeReturn;
        }

        /// <summary>
        ///  Recherche l'évènement OPP qui correspond au payemnt {payment}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        private static int LoadOppManualIdE(string pCS, IDbTransaction pDbTransaction, int pIdT, Pair<IPayment, IParty[]> payment)
        {
            int ret = 0;

            string select = string.Empty;
            DataParameters parameters = new DataParameters();

            if (StrFunc.IsFilled((payment.First.Id)))
            {
                select = @"select e.IDE 
from dbo.EVENT e 
inner join dbo.EVENT ep on ep.IDE=e.IDE_EVENT and ep.EVENTCODE='TRD' and ep.EVENTTYPE='DAT'
inner join dbo.EVENTFEE evFee on evFee.IDE=e.IDE and evFee.PAYMENTID=@PAYMENTID 
where e.IDT=@IDT and e.EVENTCODE = 'OPP'";

                parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
                parameters.Add(new DataParameter(pCS, "PAYMENTID", DbType.AnsiString, SQLCst.UT_LABEL_LEN), payment.First.Id);
            }
            else
            {

                IParty payer = payment.Second.Where(x => x.Id == payment.First.PayerPartyReference.HRef).FirstOrDefault();
                if (null == payer)
                    throw new Exception(StrFunc.AppendFormat("Payer :{0} not found", payment.First.PayerPartyReference.HRef));

                IParty receiver = payment.Second.Where(x => x.Id == payment.First.ReceiverPartyReference.HRef).FirstOrDefault();
                if (null == receiver)
                    throw new Exception(StrFunc.AppendFormat("Receiver :{0} not found", payment.First.ReceiverPartyReference.HRef));

                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_PAYER), payer.OTCmlId);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_RECEIVER), receiver.OTCmlId);
                parameters.Add(new DataParameter(pCS, "ISFEEINVOICING", DbType.AnsiString, SQLCst.UT_LABEL_LEN), PaymentTools.IsInvoicing(payment.First) ? "1" : "0");
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PAYMENTTYPE), payment.First.PaymentType.Value);
                parameters.Add(new DataParameter(pCS, "IDFEE", DbType.Int32), PaymentTools.GetIdFee(payment.First) ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "VALORISATION", DbType.Decimal), payment.First.PaymentAmount.Amount.DecValue);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC), payment.First.PaymentAmount.Currency);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCLASS), PaymentTools.IsInvoicing(payment.First) ? EventClassEnum.INV.ToString() : EventClassEnum.STL.ToString());
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), payment.First.PaymentDate.UnadjustedDate.DateValue);

                select = @"select e.IDE 
from dbo.EVENT e 
inner join dbo.EVENT ep on ep.IDE=e.IDE_EVENT and ep.EVENTCODE='TRD' and ep.EVENTTYPE='DAT'
inner join dbo.EVENTFEE evFee on evFee.IDE=e.IDE and evFee.STATUS is null and evFee.IDFEE=@IDFEE and evFee.PAYMENTTYPE=@PAYMENTTYPE and evFee.ISFEEINVOICING=@ISFEEINVOICING 
inner join dbo.EVENTCLASS evclass on evclass.IDE=e.IDE and evclass.EVENTCLASS=@EVENTCLASS and evclass.DTEVENT=@DTEVENT
where e.IDT=@IDT and e.EVENTCODE = 'OPP' and e.IDA_PAY=@IDA_PAYER and e.IDA_REC=@IDA_RECEIVER and e.VALORISATION=@VALORISATION and e.UNITSYS=@IDC";

            }

            QueryParameters qryParameters = new QueryParameters(pCS, select, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToInt32(obj);

            return ret;
        }

        //PL 20200109 New signature
        //private static void LoadTradeInfo(string pCS, int pIdT, string pTradeIdentifier, out Boolean istradeInvoiced, out Boolean istradePast)
        // EG 20210322[XXXXX] Correction Alias
        private static void LoadTradeInfo(string pCS, KeyValuePair<int, string> pTradeIdentifiers, out Boolean istradeInvoiced, out Boolean istradePast)
        {
            string query = @"select distinct tr.IDT, 
case when isnull(tr_A.IDT,0)=0 then 0 else 1 end as ISINVOICED, 
case when tr.DTBUSINESS < tr.DTENTITY then 1 else 0 end as ISPAST 
from dbo.VW_TRADE_ALLOC tr
left outer join dbo.TRADELINK tlInv on  (tlInv.IDT_B = tr.IDT) and (tlInv.LINK = 'Invoice')
left outer join dbo.TRADE tr_A on  (tr_A.IDT = tlInv.IDT_A) and (tr_A.IDSTACTIVATION != 'DEACTIV') 
where tr.IDT = @IDT";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pTradeIdentifiers.Key);

            QueryParameters queryParameter = new QueryParameters(pCS, query, dp);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, queryParameter.Query, queryParameter.Parameters.GetArrayDbParameter());
            if (dt.Rows.Count == 0)
                throw new Exception(StrFunc.AppendFormat("Trade:{0} not found", LogTools.IdentifierAndId(pTradeIdentifiers)));

            istradeInvoiced = Convert.ToBoolean(dt.Rows[0]["ISINVOICED"]);
            istradePast = Convert.ToBoolean(dt.Rows[0]["ISPAST"]);
        }

        /// <summary>
        ///  Retourne True si le payment est compatible avec les directives du traitement
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="pFeesCalculationSetting"></param>
        /// <returns></returns>
        /// FI 20180424 [23871] Refactoring gestion de  pFeesCalculationSetting.partyRole
        private Boolean IsPaymentMatch(IPayment payment, IParty[] pParty, FeesCalculationSettingsMode1 pFeesCalculationSetting)
        {

            Boolean ret = false;
            if (pFeesCalculationSetting.mode == Cst.FeesCalculationMode.INV)
                ret = PaymentTools.IsInvoicing(payment);
            else if (pFeesCalculationSetting.mode == Cst.FeesCalculationMode.STL)
                ret = (false == PaymentTools.IsInvoicing(payment));
            else
                ret = true;

            // FI 20180424 [23871]
            if (pFeesCalculationSetting.partyRoleSpecified)
            {
                IParty payer = pParty.Where(x => x.Id == payment.PayerPartyReference.HRef).FirstOrDefault();
                if (null == payer)
                    throw new Exception(StrFunc.AppendFormat("Payer :{0} not found", payment.PayerPartyReference.HRef));

                IParty receiver = pParty.Where(x => x.Id == payment.ReceiverPartyReference.HRef).FirstOrDefault();
                if (null == receiver)
                    throw new Exception(StrFunc.AppendFormat("Receiver :{0} not found", payment.ReceiverPartyReference.HRef));

                ret &= DsTrade.DtTradeActor.Rows.Cast<DataRow>().Where(x =>
                    ((string)(x["FIXPARTYROLE"]) == ReflectionTools.ConvertEnumToString<FixML.v50SP1.Enum.PartyRoleEnum>(pFeesCalculationSetting.partyRole)) &&
                     (Convert.ToInt32(x["IDA"]) == payer.OTCmlId || Convert.ToInt32(x["IDA"]) == receiver.OTCmlId)).Count() > 0;
            }


            if (false == PaymentTools.IsManualOpp(payment))
            {
                if (pFeesCalculationSetting.feeSpecified)
                    ret &= (PaymentTools.GetIdFee(payment) == pFeesCalculationSetting.fee.OTCmlId);

                if (pFeesCalculationSetting.feeSheduleSpecified)
                    ret &= (PaymentTools.GetIdFeeShedule(payment) == pFeesCalculationSetting.feeShedule.OTCmlId);

                if (pFeesCalculationSetting.feeMatrixSpecified)
                    ret &= (PaymentTools.GetIdFeeMatrix(payment) == pFeesCalculationSetting.feeMatrix.OTCmlId);
            }

            return ret;
        }

        /// <summary>
        /// Recalcul de certains frais sur un trade (cf. FeesCalculationSettingsMode1)
        /// </summary>
        /// <param name="pFeesCalculationSetting">Définition du périmètre des frais à recalculer (Mode, Frais, Conditions, Barèmes, Rôles, ...)</param>
        /// <returns></returns>
        // FI 20180328 [23871] Add method
        // EG 20190114 Add detail to ProcessLog Refactoring
        // PL 20200115 [25099] Rename from FeeCalculation() to FeeCalculationAndWrite()
        // EG 20210322[XXXXX] ProcessLog null (usage tracker) 
        private Cst.ErrLevel FeeCalculationAndWrite(FeesCalculationSettingsMode1 pFeesCalculationSetting)
        {
            if (null == pFeesCalculationSetting)
                throw new ArgumentNullException("pFeesCalculationSetting is null");

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = null;

            try
            {
                string CS = m_TradeActionGenProcess.Cs;

                
                // isManualFeesPreserved= true si Conserver les frais « Hors tarification »  (Frais insérés manuellement)
                bool isManualFeesPreserved = m_TradeActionGenMQueue.GetBoolValueParameterById(TradeActionGenMQueue.PARAM_ISMANFEES_PRESERVED);
                // isForcedFeesPreserved = true si Conserver les frais « Modifiés » issus de la tarification (Frais forcés manuellement : barème, montant, …)
                bool isForcedFeesPreserved = m_TradeActionGenMQueue.GetBoolValueParameterById(TradeActionGenMQueue.PARAM_ISFORCEDFEES_PRESERVED);

                KeyValuePair<int, string> tradeIdentifiers = new KeyValuePair<int, string>(m_TradeActionGenMQueue.id, m_TradeActionGenMQueue.identifier);
                int idT = tradeIdentifiers.Key;

                // LOG-7050 => Calcul des frais
                string[] logInfo = new string[3];
                logInfo[0] = LogTools.IdentifierAndId(tradeIdentifiers);
                logInfo[1] = StrFunc.AppendFormat("{0} / {1} / {2} / {3} / {4}",
                        Ressource.GetString(ReflectionTools.GetAttribute<ResourceAttribut>(pFeesCalculationSetting.mode).Resource),
                        pFeesCalculationSetting.partyRoleSpecified ? pFeesCalculationSetting.partyRole.ToString() : Ressource.GetString("ROLE_ALL"),
                        pFeesCalculationSetting.feeSpecified ? pFeesCalculationSetting.fee.identifier : Ressource.GetString("FEE_ALL"),
                        pFeesCalculationSetting.feeSheduleSpecified ? pFeesCalculationSetting.feeShedule.identifier : Ressource.GetString("FEESCHEDULE_ALL"),
                        pFeesCalculationSetting.feeMatrixSpecified ? pFeesCalculationSetting.feeMatrix.identifier : Ressource.GetString("FEEMATRIX_ALL"));
                logInfo[2] = StrFunc.AppendFormat("{0} / {1}", isManualFeesPreserved.ToString(), isForcedFeesPreserved.ToString());
                

                
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7050), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));

                LoadTradeInfo(CS, tradeIdentifiers, out bool istradeInvoiced, out bool istradePast);

                User user = new User(m_TradeActionGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
                TradeInput tradeInput = new TradeInput();
                tradeInput.SearchAndDeserializeShortForm(CS, null, idT.ToString(), SQL_TableWithID.IDType.Id, user, m_TradeActionGenProcess.Session.SessionId);

                int idE = GetIdE_TRDDAT();
                DateTime businessDate = new ProductContainer(m_tradeLibrary.DataDocument.CurrentTrade.Product, m_tradeLibrary.DataDocument).GetBusinessDate2();

                // lstOppOrigin => Liste des frais présents sur le trade à l'origine
                List<Pair<IPayment, int>> lstOppOrigin = new List<Pair<IPayment, int>>();
                // lstOppPreserved => Liste des frais conservés
                List<Pair<IPayment, int>> lstOppPreserved = new List<Pair<IPayment, int>>();
                // lstOppImpacted => Liste des frais qui peuvent être modifiés ou supprimés
                List<Pair<IPayment, int>> lstOppImpacted = new List<Pair<IPayment, int>>();
                
                logInfo = new string[]{LogTools.IdentifierAndId(tradeIdentifiers),
                                Ressource.GetString(ReflectionTools.GetAttribute<ResourceAttribut>(pFeesCalculationSetting.mode).Resource)};

                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    // lstOppOrigin => Liste des frais présents sur le trade à l'origine
                    int i = 1;
                    lstOppOrigin = (from item in tradeInput.CurrentTrade.OtherPartyPayment
                                    select new Pair<IPayment, int>()
                                    {
                                        First = item,
                                        Second = i++
                                    }).ToList();

                    // lstOppPreserved => Liste des frais conservés
                    switch (pFeesCalculationSetting.mode)
                    {
                        case Cst.FeesCalculationMode.STL:
                            lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => PaymentTools.IsInvoicing(x.First))
                                                     select item);

                            // LOG-07051 Les frais reglés dans le cadre d'une facture sont ignorés.
                            // PM 20210121 [XXXXX] Passage du message au niveau de log None
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7051), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));

                            if (istradePast)
                            {
                                lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => (false == PaymentTools.IsInvoicing(x.First)))
                                                         select item);
                                // LOG-07052 Les frais reglés au quotidien sont ignorés. Le trade porte sur une journée de bourse clôturée
                                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7052), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                            }
                            break;
                        case Cst.FeesCalculationMode.INV:
                            lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => (false == PaymentTools.IsInvoicing(x.First)))
                                                     select item);

                            // LOG-07053 Les frais reglés au quotidien sont ignorés.
                            // PM 20210121 [XXXXX] Passage du message au niveau de log None
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7053), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));

                            if (istradeInvoiced)
                            {
                                lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => (PaymentTools.IsInvoicing(x.First)))
                                                         select item);

                                // LOG-07054 Les frais reglés dans le cadre d'une facture sont ignorés. Le trade est déjà considéré dans une facture.
                                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7054), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                            }
                            break;
                        case Cst.FeesCalculationMode.ALL:
                            if (istradePast)
                            {
                                lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => (false == PaymentTools.IsInvoicing(x.First)))
                                                         select item);

                                // LOG-07052 Les frais reglés au quotidien sont ignorés puisque que le trade porte sur une journée de bourse clôturé.
                                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7052), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                            }
                            if (istradeInvoiced)
                            {
                                lstOppPreserved.AddRange(from item in lstOppOrigin.Where(x => (PaymentTools.IsInvoicing(x.First)))
                                                         select item);

                                // LOG-07054 Les frais reglés dans le cadre d'une facture sont ignorés. Le trade est déjà considéré dans une facture.
                                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7054), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                            }
                            break;
                    }

                    // lstOppImpacted => Liste des frais qui peuvent être modifiés ou supprimés
                    lstOppImpacted.AddRange(from item in
                                                lstOppOrigin.Where(x => (false == PaymentTools.IsExistOppPayment(x.First, (from item2 in lstOppPreserved select item2.First).ToList())))
                                            select item);

                    // LOG-07055 => Tous les frais présents sont tous conservés
                    if (lstOppPreserved.Count() == lstOppOrigin.Count())
                    {
                        logInfo = new string[] { LogTools.IdentifierAndId(tradeIdentifiers) };
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7055), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                    }
                }

                // Liste des substitutions présentes sur les frais impacté du trade 
                List<FeeRequestSubstitute> lstFeeRequestSubstitute = (from item in
                                                                          lstOppImpacted.Where(x => (false == PaymentTools.IsManualOpp(x.First)) &&
                                                                              PaymentTools.GetFeeStatus(x.First).Value == SpheresSourceStatusEnum.ScheduleForced)
                                                                      select new FeeRequestSubstitute()
                                                                      {
                                                                          source = new Pair<int, int>(PaymentTools.GetIdFeeMatrix(item.First).Value, PaymentTools.GetIdFeeSheduleSys(item.First).Value),
                                                                          targetIdFeeSchedule = PaymentTools.GetIdFeeShedule(item.First).Value
                                                                      }
                                                              ).ToList();

                // Chargement des frais en fonctions des matrices/barèmes en vigeur
                LoadCurrentFees(CS, tradeInput, isForcedFeesPreserved, lstFeeRequestSubstitute);
                
                if (false == tradeInput.CurrentTrade.OtherPartyPaymentSpecified ||
                    (tradeInput.CurrentTrade.OtherPartyPayment.Where(x => IsPaymentMatch(x, tradeInput.DataDocument.Party, pFeesCalculationSetting)).Count() == 0))
                {
                    logInfo = new string[2];
                    logInfo[0] = LogTools.IdentifierAndId(tradeIdentifiers);
                    logInfo[1] = StrFunc.AppendFormat("{0} / {1} / {2} /{3}",
                            pFeesCalculationSetting.partyRoleSpecified ? pFeesCalculationSetting.partyRole.ToString() : Ressource.GetString("ROLE_ALL"),
                            pFeesCalculationSetting.feeSpecified ? pFeesCalculationSetting.fee.identifier : Ressource.GetString("FEE_ALL"),
                            pFeesCalculationSetting.feeSheduleSpecified ? pFeesCalculationSetting.feeShedule.identifier : Ressource.GetString("FEESCHEDULE_ALL"),
                            pFeesCalculationSetting.feeMatrixSpecified ? pFeesCalculationSetting.feeMatrix.identifier : Ressource.GetString("FEEMATRIX_ALL"));
                    
                    // LOG-07060 => <b>Calcul automatique à partir des barèmes et conditions</b>
                    //Aucun frais calculés sur le périmètre. S'il existe des frais sur ce prérimètre ils seront effacés.
                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7060), 0, LoggerTools.LogParamFromString(RemoveHTMLEntities(logInfo))));
                }

                List<IPayment> lstNewOpp = new List<IPayment>();
                List<IPayment> lstModOpp = new List<IPayment>();
                List<IPayment> lstRemOpp = new List<IPayment>();
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    //Suppression des ids générés via l'appel à RecalculFeeAndTax
                    foreach (IPayment payment in tradeInput.CurrentTrade.OtherPartyPayment)
                        payment.Id = string.Empty;

                    //Boucle sur les frais issus des barèmes/Conditions en vigueur
                    foreach (IPayment item in tradeInput.CurrentTrade.OtherPartyPayment)
                    {
                        if (IsPaymentMatch(item, tradeInput.DataDocument.Party, pFeesCalculationSetting)) //Spheres ® ne consirère que les frais qui sont dans le contexte
                        {
                            Pair<IPayment, int> paymentItem = lstOppImpacted.Where(k => (false == PaymentTools.IsManualOpp(k.First))).FirstOrDefault(
                                                                                         x => PaymentTools.GetIdFeeShedule(x.First) == PaymentTools.GetIdFeeShedule(item) &&
                                                                                         PaymentTools.GetIdFeeMatrix(x.First) == PaymentTools.GetIdFeeMatrix(item));
                            if (null != paymentItem)
                            {
                                //Frais existant à l'origine
                                if (PaymentTools.GetFeeStatus(paymentItem.First).HasValue && (PaymentTools.GetFeeStatus(paymentItem.First).Value == SpheresSourceStatusEnum.Forced) && isForcedFeesPreserved)
                                {
                                    lstOppPreserved.Add(paymentItem); // Frais existant sur le trade et forcé => frais conservé puisque isForcedFeesPreserved
                                }
                                else if (PaymentTools.GetFeeStatus(paymentItem.First).HasValue && (PaymentTools.GetFeeStatus(paymentItem.First).Value == SpheresSourceStatusEnum.ScheduleForced) && isForcedFeesPreserved)
                                {
                                    if (PaymentTools.IsIdenticalPayment(item, paymentItem.First))
                                        lstOppPreserved.Add(paymentItem); // Frais existant sur le trade et substitué => frais conservé si le frais reste identique avec les formules courante du barème substitué
                                    else
                                        lstModOpp.Add(item); // => Frais modifé par rapport à l'origine (montant différent par Exemple) 
                                }
                                else if (PaymentTools.IsIdenticalPayment(item, paymentItem.First))
                                {
                                    lstOppPreserved.Add(paymentItem); //Aucun Chgt => frais conservé comme à l'origine
                                }
                                else
                                {
                                    lstModOpp.Add(item); // => Frais modifé par rapport à d'origine (montant différent par Exemple) 
                                }
                            }
                            else
                            {
                                //Frais non existant à l'origine
                                lstNewOpp.Add(item);  //=> Ajout de cette ligne de frais
                            }
                        }
                    }
                }

                //Boucle sur les frais (*) du trade à l'origine 
                //(*) pouvant être modifié
                foreach (Pair<IPayment, int> item in lstOppImpacted)
                {
                    if (IsPaymentMatch(item.First, tradeInput.DataDocument.Party, pFeesCalculationSetting))
                    {
                        Boolean isDelOpp = false;
                        if (PaymentTools.IsManualOpp(item.First))
                        {
                            isDelOpp = (false == isManualFeesPreserved); // isDelOpp = true => Les frais manuel ne son pas préservés
                            if (false == isDelOpp)
                                lstOppPreserved.Add(item); // Correction recette => Ajour dans la collection lstOppPreserved
                        }
                        else
                        {
                            IPayment paymentItem = null;
                            if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                            {
                                paymentItem = tradeInput.CurrentTrade.OtherPartyPayment.FirstOrDefault(x =>
                                                                   PaymentTools.GetIdFeeShedule(x) == PaymentTools.GetIdFeeShedule(item.First) &&
                                                                   PaymentTools.GetIdFeeMatrix(x) == PaymentTools.GetIdFeeMatrix(item.First));
                            }

                            Boolean isExistInCurrentFees = (null != paymentItem);
                            isDelOpp = (false == isExistInCurrentFees);
                            /*
                            if ((false == isExistInCurrentFees) &&
                                (PaymentTools.GetFeeStatus(item.First).Value == SpheresSourceStatusEnum.Forced || PaymentTools.GetFeeStatus(item.First).Value == SpheresSourceStatusEnum.ScheduleForced) && isForcedFeesPreserved)
                            {
                                lstOppPreserved.Add(item);
                            }
                            else
                            {
                                isDelOpp = (false == isExistInCurrentFees);
                            }*/
                        }
                        if (isDelOpp)
                            lstRemOpp.Add(item.First);
                    }
                    else
                    {
                        lstOppPreserved.Add(item); //frais conservé comme à l'origine puisqu'il hors contexte
                    }
                }

                Boolean isExistChange = ((lstNewOpp.Concat(lstModOpp).Concat(lstRemOpp)).Count() > 0);
                if (false == isExistChange)
                {
                    logInfo = new string[] { LogTools.IdentifierAndId(tradeIdentifiers) };
                    // LOG-07061 => <b>Aucune modification opérée sur les frais</b>
                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7061), 0,
                        new LogParam(LogTools.IdentifierAndId(tradeIdentifiers))));
                }

                if (isExistChange)
                {
                    dbTransaction = DataHelper.BeginTran(CS);

                    //Delete EVENT
                    foreach (IPayment payment in lstRemOpp.Concat(lstModOpp))
                    {
                        if (PaymentTools.IsManualOpp(payment))
                        {
                            int idERem = LoadOppManualIdE(CS, dbTransaction, idT, new Pair<IPayment, IParty[]>(payment, tradeInput.DataDocument.Party));
                            TradeRDBMSTools.DeleteFeeEventId(CS, dbTransaction, idT, idERem);
                        }
                        else
                        {
                            TradeRDBMSTools.DeleteFeeEventAuto(CS, dbTransaction, idT, PaymentTools.GetIdFeeShedule(payment), PaymentTools.GetIdFeeMatrix(payment));
                        }
                    }

                    //Update TRADE
                    List<Pair<IPayment, string>> newOpp = new List<Pair<IPayment, string>>();
                    newOpp.AddRange(
                                    (from item
                                        in lstOppPreserved.OrderBy(x => x.Second) //l'ordre initiale est conservés
                                     select new Pair<IPayment, string>(item.First, "PreservedOpp"))
                                     .Concat(
                                     (from item
                                        in lstModOpp
                                      select new Pair<IPayment, string>(item, "ModOpp")))
                                     .Concat(
                                     (from item
                                        in lstNewOpp
                                      select new Pair<IPayment, string>(item, "NewOpp"))
                                      )
                                    );

                    IPayment[] newPayment = ((from item in newOpp
                                              select item.First)).ToArray();

                    IPayment[] ModOppAndNewOpp = ((from item in newOpp.Where(x => (x.Second == "ModOpp") || x.Second == ("NewOpp"))
                                                   select item.First)).ToArray();

                    // Mise en place des Id uniquement sur les frais modifiés ou nouveaux
                    // Il ne faut pas mettre des Id sur les frais préservés (ceux-ci conservent leur Id initial qui par ailleurs peut être renseigné ou pas)  
                    // => il ne faut pas mettre un Id sur un frais préservé sur lequel l'Id est non renseigné. Pour ce frais EVENTFEE.PAYMENTID est non renseigné.  
                    Tools.SetPaymentId(ModOppAndNewOpp, "OPP", Tools.GetMaxIndex(newPayment, "OPP") + 1);

                    // EG 20210322 ProcessLog null(usage tracker)
                    EventQuery.UpdateTradeXMLForFees(dbTransaction, idT, tradeInput.DataDocument, newPayment, OTCmlHelper.GetDateSysUTC(CS), Process.UserId,
                                    Process.Session, m_TradeActionGenProcess.Tracker.IdTRK_L, m_TradeActionGenProcess.Tracker.IdProcess);

                    if (newOpp.Where(x => x.Second != "PreservedOpp").Count() > 0)
                    {
                        //Insert EVENT pour les nouvelles lignes de frais et pour les lignes de frais modifiées
                        IPayment[] payments = (from item in newOpp.Where(x => x.Second != "PreservedOpp")
                                               select item.First).ToArray();

                        int nbEvent = 0;
                        IPayment[] modifiedPayments = EventQuery.PrepareFeeEvents(CS, tradeInput.Product.Product, tradeInput.DataDocument, idT, payments, ref nbEvent);

                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);
                        EventQuery eventQuery = new EventQuery(m_TradeActionGenProcess.Session, m_TradeActionGenProcess.ProcessType, m_TradeActionGenProcess.Tracker.IdTRK_L);
                       eventQuery.InsertFeeEvents(CS, dbTransaction, tradeInput.DataDocument, idT, businessDate, idE, modifiedPayments, newIdE);
                    }

                    DataHelper.CommitTran(dbTransaction);

                    if (newOpp.Where(x => x.Second == "PreservedOpp").Count() > 0)
                    {
                        // LOG-07065 => <b>Liste des frais conservés</b>
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7065), 0,
                            new LogParam(LogTools.IdentifierAndId(tradeIdentifiers))));

                        m_TradeActionGenProcess.AddLogFeeInformation(tradeInput.DataDocument, (from item in newOpp.Where(x => x.Second == "PreservedOpp") select item.First).ToArray(), LogLevelDetail.LEVEL3, 1);
                    }
                    //
                    if (newOpp.Where(x => x.Second == "ModOpp").Count() > 0)
                    {
                        // LOG-7066 => <b>Liste des frais modifiés</b>
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7066), 0,
                            new LogParam(LogTools.IdentifierAndId(tradeIdentifiers))));

                        m_TradeActionGenProcess.AddLogFeeInformation(tradeInput.DataDocument, (from item in newOpp.Where(x => x.Second == "ModOpp") select item.First).ToArray(), LogLevelDetail.LEVEL3, 1);
                    }
                    //
                    if (newOpp.Where(x => x.Second == "NewOpp").Count() > 0)
                    {
                        // LOG-7068 => <b>Liste des frais ajoutés</b>
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7068), 0,
                            new LogParam(LogTools.IdentifierAndId(tradeIdentifiers))));

                        m_TradeActionGenProcess.AddLogFeeInformation(tradeInput.DataDocument, (from item in newOpp.Where(x => x.Second == "NewOpp") select item.First).ToArray(), LogLevelDetail.LEVEL3, 1);
                    }

                    if (lstRemOpp.Count > 0)
                    {
                        // LOG-7068 => <b>Liste des frais ajoutés</b>
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7067), 0,
                            new LogParam(LogTools.IdentifierAndId(tradeIdentifiers))));

                        m_TradeActionGenProcess.AddLogFeeInformation(tradeInput.DataDocument, lstRemOpp.ToArray(), LogLevelDetail.LEVEL3, 1);
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
            return codeReturn;
        }

     
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pArray"></param>
        /// <returns></returns>
        private static string[] RemoveHTMLEntities(string[] pArray)
        {
            string[] ret = pArray;
            if (ArrFunc.IsFilled(pArray))
            {
                ret = (from item in pArray
                       select
                       StrFunc.IsFilled(item) ?
                       item.Replace("<", string.Empty).Replace(">", string.Empty) : item
                       ).ToArray();
            }
            return ret;
        }

        /// <summary>
        /// Alimente {tradeInput.DataDocument.currentTrade.otherPartyPayment} en fonction des conditions/barèmes en vigueur
        /// </summary>
        /// <param name="CS"></param>
        /// <param name="pTradeInput"></param>
        /// <param name="pIsForcedFeesPreserved">si true remplaceement des barèmes théoriques  par les barèmes de substitutions</param>
        /// <param name="pLstOppImpacted">Liste des frais où des </param>
        private static void LoadCurrentFees(string CS, TradeInput pTradeInput, Boolean pIsForcedFeesPreserved, List<FeeRequestSubstitute> plstFeeRequestSubstitute)
        {

            // Calcul des frais en fonction des barèmes et conditions en vigueur
            pTradeInput.CurrentTrade.OtherPartyPayment = null;
            pTradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;
            pTradeInput.RecalculFeeAndTax(CS, null);

            // Substitution des frais de Spheres® lorsqu'il existe des substitutions à l'origine  
            if (pTradeInput.CurrentTrade.OtherPartyPaymentSpecified && pIsForcedFeesPreserved && plstFeeRequestSubstitute.Count > 0)
            {

                //Sauvegarde des frais  en fonction des barèmes et conditions en vigueur
                IPayment[] otherPartyPaymentSav = (IPayment[])pTradeInput.DataDocument.CurrentTrade.OtherPartyPayment.Clone();


                ArrayList lstNewOpp = new ArrayList();
                foreach (IPayment item in otherPartyPaymentSav)
                {
                    FeeRequestSubstitute feeRequestSubstitute = (from itemSubstitute in plstFeeRequestSubstitute.Where(x => x.source.First == PaymentTools.GetIdFeeMatrix(item) 
                                                                                                                         && x.source.Second == PaymentTools.GetIdFeeShedule(item))
                                                                 select itemSubstitute).FirstOrDefault();

                    Boolean isToReplace = (null != feeRequestSubstitute);
                    if (isToReplace)
                    {
                        // ClearFee
                        pTradeInput.ClearFee(TradeInput.FeeTarget.trade, TradeInput.ClearFeeMode.All);
                        // Calc TradeInput.DataDocument.otherPartyPayment
                        FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(CS), null, pTradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade))
                        {
                            SubstituteInfo = feeRequestSubstitute
                        };
                        FeeProcessing fees = new FeeProcessing(feeRequest);
                        fees.Calc(CSTools.SetCacheOn(CS), null);
                        if (ArrFunc.IsFilled(fees.FeeResponse))
                            pTradeInput.SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse, false, false);

                        if (pTradeInput.DataDocument.OtherPartyPaymentSpecified)
                        {
                            IPayment payment = pTradeInput.DataDocument.OtherPartyPayment[0];
                            payment.Id = item.Id;

                            payment.PaymentSource.Status = SpheresSourceStatusEnum.ScheduleForced;

                            ISpheresIdSchemeId spheresId = payment.PaymentSource.SpheresId.Where(x => StrFunc.IsEmpty(x.Scheme)).FirstOrDefault();
                            if (null == spheresId)
                            {
                                ReflectionTools.AddItemInArray(payment.PaymentSource, "spheresId", 0);
                                spheresId = payment.PaymentSource.SpheresId.Last();
                            }

                            ISpheresIdSchemeId feeScheduleCurrent = item.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme);

                            ISpheresIdSchemeId feeScheduleSys = pTradeInput.DataDocument.CurrentProduct.ProductBase.CreateSpheresId(1)[0];
                            feeScheduleSys.Scheme = Cst.OTCml_RepositoryFeeScheduleSys;
                            feeScheduleSys.OTCmlId = feeScheduleCurrent.OTCmlId;
                            feeScheduleSys.Value = feeScheduleCurrent.Value;


                            spheresId.Scheme = feeScheduleSys.Scheme;
                            spheresId.OTCmlId = feeScheduleSys.OTCmlId;
                            spheresId.Value = feeScheduleSys.Value;

                            lstNewOpp.Add(payment);

                        }
                        else
                        {
                            //On rentre ici au cas ou le barème une substitution su passé (sur le trade)  n'est plus en vigueur 
                            lstNewOpp.Add(item);
                        }
                    }
                    else
                        lstNewOpp.Add(item);
                }

                pTradeInput.DataDocument.OtherPartyPaymentSpecified = (lstNewOpp.Count > 0);
                if (pTradeInput.DataDocument.CurrentTrade.OtherPartyPaymentSpecified)
                    pTradeInput.DataDocument.CurrentTrade.OtherPartyPayment = (IPayment[])lstNewOpp.ToArray(typeof(IPayment));

            }
        }
    }
}