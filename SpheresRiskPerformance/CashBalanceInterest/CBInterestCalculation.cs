using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CashBalanceInterest
{
    /// <summary>
    /// Règle de calcul des intérêts enrichie
    /// </summary>
    internal partial class InterestRule
    {
        #region members
        private DateTime m_MinDate; /* Date business de début de recherche des flux */
        private IBusinessDayAdjustments m_BDA = default;
        private SQL_RateIndex sqlInterestRateIndex = default;
        #endregion
        #region accessors
        public DayCountFractionEnum DayCountFractionEnum
        {
            get { return (DayCountFractionEnum)StringToEnum.Parse(this.DayCountFraction, DayCountFractionEnum.ACT360); }
        }
        public InterestAmountTypeEnum AmountTypeEnum
        {
            get { return (InterestAmountTypeEnum)StringToEnum.Parse(this.AmountType, InterestAmountTypeEnum.CashBalance); }
        }
        public PeriodEnum PeriodEnum
        {
            get { return (PeriodEnum)StringToEnum.Parse(this.Period, PeriodEnum.D); }
        }
        public DateTime MinDate
        {
            get { return m_MinDate; }
            set { m_MinDate = value; }
        }
        public IBusinessDayAdjustments BDA
        {
            get { return m_BDA; }
            set { m_BDA = value; }
        }
        public SQL_RateIndex SqlInterestRateIndex
        {
            get
            {
                if ((this.IdAsset == default) || (this.IdRateIndex == default))
                {
                    sqlInterestRateIndex = default;
                }
                return sqlInterestRateIndex;
            }
        }
        #endregion
        #region methods
        public InterestRule ShallowCopy()
        {
            return (InterestRule)this.MemberwiseClone();
        }
        /// <summary>
        /// Lecture de l'indice
        /// </summary>
        /// <param name="pCS"></param>
        public void SetSqlRateIndex(string pCS)
        {
            if ((this.IdAsset != default) && (this.IdRateIndex != default))
            {
                sqlInterestRateIndex = new SQL_RateIndex(pCS, (int)this.IdRateIndex, SQL_Table.ScanDataDtEnabledEnum.Yes);
                sqlInterestRateIndex.LoadTable();
            }
            else
            {
                sqlInterestRateIndex = default;
            }
        }

        #endregion
    }

    /// <summary>
    /// Flux enrichie
    /// </summary>
    internal partial class InterestFlow
    {
        #region members
        public DateTime ValueDate = DateTime.MinValue;
        public InterestRule InterestRule = default;
        // PM 20130819 [18582] Ajouté pour la gestion du seuil minimum
        public decimal FinalAmount = 0;
        #endregion
        #region methods
        public InterestFlow ShallowCopy()
        {
            return (InterestFlow)this.MemberwiseClone();
        }
        public DateTime STLDate
        {
            get { return (DateSTL == default ? DateTime.MinValue : (DateTime) DateSTL); }
            set { DateSTL = value; }
        }
        #endregion
    }
    
    /// <summary>
    /// Calcul de la période de calcul des intérêts et de tous les flux de cette période sur lesquels portent les intérêts
    /// </summary>
    internal class CashInterestCalculation
    {
        #region constants
        private const int ValueDateOffset = 1;
        #endregion
        #region members
        private readonly EfsML.v30.CashBalanceInterest.CashBalanceInterest m_CBIProduct = null; /* Product : à des fins technique */
        private readonly IOffset m_OffsetMinDate = null;
        // PM 20150206 [20778] Add m_OffsetZero
        private readonly IOffset m_OffsetZero = null; 
        private readonly IOffset m_OffsetStartPeriod = null;
        private readonly IOffset m_OffsetPlusOneDay = null;
        private readonly IOffset m_OffsetValueDate = null;
        private readonly IBusinessDayAdjustments m_BDACaldendar = null;

        private readonly InterestProcessParameters m_ProcessParameter = null; /* Paramétres de la tâche */
        private DateTime m_MinDate; /* Date business de début de recherche des flux */
        private readonly DateTime m_StartPeriodDate; /* Date calendaire de début de période de calcul des intérêts*/

        private List<InterestRule> m_InterestRules = null;   /* Régles d'intérêts paramétrées ([Déposit: 1 élément][Cash Balance: 2 éléments])*/
        private List<InterestFlow> m_InterestFlows = null; /* Flux associés */
        private List<InterestFlow> m_PaymentFlows = null; /* Payment */
        private List<CashBalanceInterestTradeInfo> m_TradeInfo = null;
        private readonly string m_CS = null;
        private readonly ProcessBase m_Process = null;
        #endregion
        #region accessors
        internal IProduct Product
        {
            get { return (IProduct)m_CBIProduct; }
        }
        internal DateTime StartPeriodDate
        {
            get { return m_StartPeriodDate; }
        }
        #endregion
        #region constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public CashInterestCalculation(ProcessBase pProcess, string pCS, InterestProcessParameters pProcessParameter)
        {
            // Copier les paramètres
            m_Process = pProcess;
            m_CS = pCS;
            m_ProcessParameter = pProcessParameter;
            // Créer les autres membres
            m_CBIProduct = new EfsML.v30.CashBalanceInterest.CashBalanceInterest();
            m_OffsetStartPeriod = Product.ProductBase.CreateOffset(m_ProcessParameter.PeriodEnum, -1 * m_ProcessParameter.PeriodMultiplier, DayTypeEnum.Calendar);
            m_OffsetPlusOneDay = Product.ProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Calendar);
            m_BDACaldendar = Product.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE);
            #region m_StartPeriodDate
            // Recherche du premier jour de la période de calcul
            IOffset m_OffsetPlusOnePeriod = Product.ProductBase.CreateOffset(m_ProcessParameter.PeriodEnum, 1, DayTypeEnum.Calendar);
            m_StartPeriodDate = Tools.ApplyOffset(m_CS, m_ProcessParameter.ProcessDate, m_OffsetStartPeriod, m_BDACaldendar, null);
            m_StartPeriodDate = Tools.ApplyOffset(m_CS, m_StartPeriodDate, m_OffsetPlusOnePeriod, m_BDACaldendar, null);
            if (m_ProcessParameter.PeriodEnum != PeriodEnum.D)
            {
                EFS_RollConvention rollConvention = default;
                switch (m_ProcessParameter.PeriodEnum)
                {
                    case PeriodEnum.W:
                        if (m_StartPeriodDate.DayOfWeek != DayOfWeek.Monday)
                        {
                            rollConvention = EFS_RollConvention.GetNewRollConvention(RollConventionEnum.MON, m_StartPeriodDate.AddDays(-7), default);
                        }
                        break;
                    case PeriodEnum.M:
                        rollConvention = EFS_RollConvention.GetNewRollConvention(RollConventionEnum.DAY1, m_StartPeriodDate, default);
                        break;
                    case PeriodEnum.Y:
                        rollConvention = EFS_RollConvention.GetNewRollConvention(RollConventionEnum.FOY, m_StartPeriodDate, default);
                        break;
                }
                if (rollConvention != default)
                {
                    m_StartPeriodDate = rollConvention.rolledDate;
                }
            }
            #endregion m_StartPeriodDate
            m_OffsetMinDate = Product.ProductBase.CreateOffset(PeriodEnum.D, -1 * ValueDateOffset, DayTypeEnum.Business);
            m_OffsetValueDate = Product.ProductBase.CreateOffset(PeriodEnum.D, ValueDateOffset, DayTypeEnum.Business);
            // PM 20150206 [20778] Add m_OffsetZero
            m_OffsetZero = Product.ProductBase.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Business);
        }
        #endregion
        #region methods
        /// <summary>
        /// Chargement des règles d'intérêts et des flux associés
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        internal Cst.ErrLevel LoadData()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
            using (IDbConnection connection = DataHelper.OpenConnection(m_CS))
            {
                // Chargement des règles d'intérêts
                dbParametersValue.Add(DataParameter.ParameterEnum.IDA.ToString(), m_ProcessParameter.IdA_Cbo);
                dbParametersValue.Add(DataParameter.ParameterEnum.IDC.ToString(), m_ProcessParameter.Currency);
                dbParametersValue.Add(DataParameter.ParameterEnum.AMOUNTTYPE.ToString(), m_ProcessParameter.AmountTypeEnum.ToString());
                dbParametersValue.Add(DataParameter.ParameterEnum.PERIOD.ToString(), m_ProcessParameter.PeriodEnum.ToString());
                dbParametersValue.Add(DataParameter.ParameterEnum.PERIODMLTP.ToString(), m_ProcessParameter.PeriodMultiplier);
                m_InterestRules = DataContractLoad<InterestRule>.LoadData(connection, dbParametersValue, DataContractResultSets.INTERESTRULE_CBI);
                dbParametersValue.Clear();
                ExtendInterestRules(m_InterestRules);

                if (m_ProcessParameter.AmountTypeEnum == InterestAmountTypeEnum.CashBalance)
                {
                    if (m_InterestRules.Count == 1)
                    {
                        InterestRule firstRule = m_InterestRules.First();
                        InterestRule newRule = firstRule.ShallowCopy();
                        newRule.AmountType = firstRule.AmountTypeEnum == InterestAmountTypeEnum.CreditCashBalance ? InterestAmountTypeEnum.DebitCashBalance.ToString() : InterestAmountTypeEnum.CreditCashBalance.ToString();
                        newRule.Fixedrate = 0;
                        newRule.IdAsset = default(int);
                        newRule.Multiplier = 0;
                        newRule.Spread = 0;
                        m_InterestRules.Add(newRule);
                    }
                    if (m_InterestRules.Count != 2)
                    {
                        codeReturn = Cst.ErrLevel.DATANOTFOUND;
                    }
                }
                else if (m_InterestRules.Count != 1)
                {
                    codeReturn = Cst.ErrLevel.DATANOTFOUND;
                }

                if (codeReturn != Cst.ErrLevel.SUCCESS)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4501), 0,
                        new LogParam(m_ProcessParameter.ProcessDateISO),
                        new LogParam(m_ProcessParameter.Identifier_Cbo),
                        new LogParam(m_ProcessParameter.Currency),
                        new LogParam(m_ProcessParameter.AmountType)));
                }
                else
                {
                    // Calcul de la date minimum
                    m_MinDate = CalcMinDate();

                    // Chargement des flux
                    DataContractResultSets flowType;
                    dbParametersValue.Add(DataParameter.ParameterEnum.PRODUCT.ToString(), Cst.ProductCashBalance);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDA.ToString(), m_ProcessParameter.IdA_Cbo);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDC.ToString(), m_ProcessParameter.Currency);
                    if (InterestAmountTypeEnum.CashCoveredInitialMargin == m_ProcessParameter.AmountTypeEnum)
                    {
                        if (m_InterestRules.First().IsUseAvailableCash)
                            dbParametersValue.Add(DataParameter.ParameterEnum.EVENTTYPE.ToString(), EventTypeEnum.CSU.ToString());
                        else
                            dbParametersValue.Add(DataParameter.ParameterEnum.EVENTTYPE.ToString(), EventTypeEnum.UMR.ToString());

                        flowType = DataContractResultSets.FLOWCASHCOVERED_CBI;
                    }
                    else
                    {
                        dbParametersValue.Add(DataParameter.ParameterEnum.EVENTTYPE.ToString(), EventTypeEnum.CSB.ToString());
                        flowType = DataContractResultSets.FLOWCASHBALANCE_CBI;
                    }
                    dbParametersValue.Add(DataParameter.ParameterEnum.DTSTART.ToString(), m_MinDate);
                    dbParametersValue.Add(DataParameter.ParameterEnum.DTEND.ToString(), m_ProcessParameter.ProcessDate);

                    m_InterestFlows = DataContractLoad<InterestFlow>.LoadData(connection, dbParametersValue, flowType);
                    dbParametersValue.Clear();
                    ExtendInterestFlows(m_InterestFlows);

                    // Si Flux = CashBalance alors chargement des CashPayments
                    if (DataContractResultSets.FLOWCASHBALANCE_CBI == flowType)
                    {
                        InterestFlow firstFlow = m_InterestFlows.FirstOrDefault();
                        bool isClearer = (firstFlow != default(InterestFlow)) && firstFlow.IsClearer;
                        flowType = DataContractResultSets.FLOWCASHPAYMENT_CBI;

                        List<int> actors = new List<int>
                        {
                            m_ProcessParameter.IdA_Cbo
                        };
                        actors = actors.Union(
                            (from flow in m_InterestFlows
                             group flow by flow.IdA into actorFlow
                             select actorFlow.Key).ToList()).ToList();

                        m_PaymentFlows = new List<InterestFlow>();
                        foreach (int ida in actors)
                        {
                            dbParametersValue.Add(DataParameter.ParameterEnum.PRODUCT.ToString(), Cst.ProductCashPayment);
                            dbParametersValue.Add(DataParameter.ParameterEnum.IDA.ToString(), ida);
                            dbParametersValue.Add(DataParameter.ParameterEnum.IDC.ToString(), m_ProcessParameter.Currency);
                            // PM 20150206 [20778] Prendre tous les types de Payment
                            //dbParametersValue.Add(DataParameter.ParameterEnum.EVENTTYPE.ToString(), EventTypeEnum.CSH.ToString());
                            dbParametersValue.Add(DataParameter.ParameterEnum.DTSTART.ToString(), m_StartPeriodDate);
                            dbParametersValue.Add(DataParameter.ParameterEnum.DTEND.ToString(), m_ProcessParameter.ProcessDate);
                            dbParametersValue.Add(DataParameter.ParameterEnum.ISXXX.ToString(), isClearer);

                            m_PaymentFlows = m_PaymentFlows.Concat(DataContractLoad<InterestFlow>.LoadData(connection, dbParametersValue, flowType)).ToList();
                            dbParametersValue.Clear();
                        }
                        ExtendInterestFlows(m_PaymentFlows);
                    }
                    else
                    {
                        m_PaymentFlows = new List<InterestFlow>();
                    }
                    // Vérifier qu'il existe au moins 1 flux
                    if ((m_InterestFlows.Count == 0) && (m_PaymentFlows.Count == 0))
                    {
                        codeReturn = Cst.ErrLevel.DATANOTFOUND;

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4502), 0,
                            new LogParam(m_ProcessParameter.ProcessDateISO),
                            new LogParam(m_ProcessParameter.Identifier_Cbo),
                            new LogParam(m_ProcessParameter.Currency),
                            new LogParam(m_ProcessParameter.AmountType)));
                    }
                }
            }
            return codeReturn;
        }
        /// <summary>
        /// Ajout des données étendues aux InterestRule
        /// </summary>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private List<InterestRule> ExtendInterestRules(List<InterestRule> pRules)
        {
            foreach (InterestRule rule in pRules)
            {
                rule.BDA = Product.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, rule.BusinessCenter);
                // PM 20150206 [20778] Utilisation de m_OffsetZero pour revenir au 1er jour ouvré précédant avant d'effectuer l'offset pour prendre la 1ère date de solde 
                //rule.MinDate = Tools.ApplyOffset(m_CS, StartPeriodDate, m_OffsetMinDate, rule.BDA);
                rule.MinDate = Tools.ApplyOffset(m_CS, StartPeriodDate, m_OffsetZero, rule.BDA, null);
                rule.MinDate = Tools.ApplyOffset(m_CS, rule.MinDate, m_OffsetMinDate, rule.BDA, null);
            }
            return pRules;
        }
        /// <summary>
        /// Ajout des données étendues aux InterestFlow
        /// </summary>
        /// <param name="pFlows">Liste des flux dont étendre les données</param>
        /// <returns>Liste des flux avec données étendues</returns>
        private List<InterestFlow> ExtendInterestFlows(List<InterestFlow> pFlows)
        {
            // PM 20130819 [18582] Ajout paramètre pIsApplyOffset pour la gestion du seuil minimum
            return ExtendInterestFlows(pFlows, true);
        }
        /// <summary>
        /// Ajout des données étendues aux InterestFlow
        /// </summary>
        /// <param name="pFlows">Liste des flux dont étendre les données</param>
        /// <param name="pIsApplyOffset">True pour appliquer l'offset de date valeur</param>
        /// <returns>Liste des flux avec données étendues</returns>
        // PM 20130819 [18582] Ajouté pour la gestion du seuil minimum
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private List<InterestFlow> ExtendInterestFlows(List<InterestFlow> pFlows, bool pIsApplyOffset)
        {
            foreach (InterestFlow flow in pFlows)
            {
                flow.InterestRule = (from rule in m_InterestRules
                                     where ((rule.AmountTypeEnum == m_ProcessParameter.AmountTypeEnum) ||
                                            ((rule.AmountTypeEnum == InterestAmountTypeEnum.CreditCashBalance) && flow.FlowAmount >= 0) ||
                                            ((rule.AmountTypeEnum == InterestAmountTypeEnum.DebitCashBalance) && flow.FlowAmount < 0)
                                           )
                                     select rule).FirstOrDefault();
                if (flow.InterestRule == default)
                {
                    flow.InterestRule = m_InterestRules.First();
                }
                if (pIsApplyOffset)
                {
                    flow.ValueDate = Tools.ApplyOffset(m_CS, flow.DateBusiness, m_OffsetValueDate, flow.InterestRule.BDA, null);
                }
            }
            return pFlows;
        }
        /// <summary>
        /// Calcul de la date de bourse minimum des flux à rechercher sur la période
        /// </summary>
        private DateTime CalcMinDate()
        {
            return m_InterestRules.Min(c => c.MinDate);
        }
        /// <summary>
        /// Ajoute les flux de début de période à 0 si ceux-ci sont inexistant
        /// </summary>
        private void AddFirstFlow()
        {
            // PM 20150206 [20778] Modification de la clé de regoupement & refactoring de l'ajout du 1er élément de la période lorsqu'il n'existe pas
            //IEnumerable<InterestFlow> minDateFlow = from flow in m_InterestFlows
            //                                        group flow by new {
            //                                            IdA = flow.IdA,
            //                                            IdB = flow.IdB,
            //                                            IdAEntity = flow.IdA_Entity,
            //                                            IdBEntity = flow.IdB_Entity,
            //                                            flow.Currency,
            //                                            flow.InterestRule } into groupedFlow
            //                                        where groupedFlow.Min(f => f.ValueDate) > m_StartPeriodDate
            //                                        select new InterestFlow
            //                                        {
            //                                            IdA = groupedFlow.Key.IdA,
            //                                            IdB = groupedFlow.Key.IdB,
            //                                            IdA_Entity = groupedFlow.Key.IdAEntity,
            //                                            IdB_Entity = groupedFlow.Key.IdBEntity,
            //                                            Currency = groupedFlow.Key.Currency,
            //                                            InterestRule = groupedFlow.Key.InterestRule,
            //                                            FlowAmount = 0,
            //                                            DateBusiness = m_StartPeriodDate,
            //                                            ValueDate = m_StartPeriodDate
            //                                        };

            //m_InterestFlows = m_InterestFlows.Concat(minDateFlow).ToList();

            var groupedInterestFlows =
                from flow in m_InterestFlows
                group flow by new
                {
                    flow.IdA,
                    flow.IdB,
                    flow.IdA_Entity,
                    flow.IdB_Entity,
                    flow.Currency
                } into groupedFlow
                select groupedFlow;

            foreach (var groupedFlow in groupedInterestFlows)
            {
                DateTime minValueDate = groupedFlow.Min(f => f.ValueDate);
                if (minValueDate > m_StartPeriodDate)
                {
                    InterestFlow newFlow = new InterestFlow
                    {
                        IdA = groupedFlow.Key.IdA,
                        IdB = groupedFlow.Key.IdB,
                        IdA_Entity = groupedFlow.Key.IdA_Entity,
                        IdB_Entity = groupedFlow.Key.IdB_Entity,
                        Currency = groupedFlow.Key.Currency,
                        InterestRule = groupedFlow.FirstOrDefault(f => f.ValueDate == minValueDate).InterestRule,
                        FlowAmount = 0,
                        DateBusiness = m_StartPeriodDate,
                        ValueDate = m_StartPeriodDate
                    };
                    m_InterestFlows.Add(newFlow);
                }
            }
        }
        /// <summary>
        /// Ajoute les flux des jours manquants
        /// </summary>
        private void FillMissingFlow()
        {
            if ((m_InterestFlows != null) && (m_InterestFlows.Count > 0))
            {
                // Trie des flux
                IEnumerable<InterestFlow> orderedFlow = from flow in m_InterestFlows
                                                        orderby flow.IdA, flow.IdB, flow.IdA_Entity, flow.IdB_Entity, flow.Currency, flow.DateBusiness, flow.ValueDate
                                                        select flow;
                InterestFlow previousFlow = null;
                InterestFlow newFlow = null;
                // Compléter les intervals de flux manquant à partir de la plus petite date valeur
                foreach (InterestFlow currentFlow in orderedFlow)
                {
                    if (previousFlow != null)
                    {
                        DateTime currentDate = previousFlow.ValueDate.AddDays(1);
                        if ((previousFlow.IdA == currentFlow.IdA) &&
                            (previousFlow.IdB == currentFlow.IdB) &&
                            (previousFlow.IdA_Entity == currentFlow.IdA_Entity) &&
                            (previousFlow.IdB_Entity == currentFlow.IdB_Entity) &&
                            (previousFlow.Currency == currentFlow.Currency))
                        {
                            while (currentFlow.ValueDate > currentDate)
                            {
                                newFlow = previousFlow.ShallowCopy();
                                newFlow.ValueDate = currentDate;
                                m_InterestFlows.Add(newFlow);

                                currentDate = currentDate.AddDays(1);
                            }
                        }
                        else
                        {
                            // Compléter jusque la date de fin de période
                            while (currentDate <= m_ProcessParameter.ProcessDate)
                            {
                                newFlow = previousFlow.ShallowCopy();
                                newFlow.ValueDate = currentDate;
                                m_InterestFlows.Add(newFlow);

                                currentDate = currentDate.AddDays(1);
                            }
                        }
                    }
                    previousFlow = currentFlow;
                }
                if (previousFlow.ValueDate < m_ProcessParameter.ProcessDate)
                {
                    DateTime currentDate = previousFlow.ValueDate.AddDays(1);
                    // Compléter jusque la date de fin de période pour le dernier flux
                    while (currentDate <= m_ProcessParameter.ProcessDate)
                    {
                        newFlow = previousFlow.ShallowCopy();
                        newFlow.ValueDate = currentDate;
                        m_InterestFlows.Add(newFlow);

                        currentDate = currentDate.AddDays(1);
                    }
                }
                // PM 20151109 Ajout tri des flux
                m_InterestFlows = (
                    from fl in m_InterestFlows
                    orderby fl.IdA, fl.IdB, fl.IdA_Entity, fl.IdB_Entity, fl.Currency, fl.DateBusiness, fl.ValueDate
                    select fl).ToList();
            }
        }
        /// <summary>
        /// Rectifie les montants de cash balance en fonction des montants de cash payment
        /// </summary>
        private void ApplyPaymentToFlow()
        {
            // Cela ne s'applique pas aux Cash Covered Initial Margin, mais uniquement aux Cash Balance
            if (InterestAmountTypeEnum.CashBalance == m_ProcessParameter.AmountTypeEnum)
            {
                // S'il y a des cash payment
                if ((m_PaymentFlows != null) && (m_PaymentFlows.Count > 0))
                {
                    foreach(InterestFlow pf in m_PaymentFlows)
                    {
                        //PM 20150319 [POC] Ne rien faire si la date de valeur calculé correspond à la date STL du payment
                        if (pf.STLDate != pf.ValueDate)
                        {
                            DateTime applyFromDate; // Date à partir de  laquelle appliquer le payment
                            DateTime applyToDate; // Date jusque laquelle appliquer le payment
                            decimal paymentAmount = 0; // Montant du payment à appliquer

                            // Si la date de valeur est antérieur à la date de prise en compte
                            //  => Retrancher le versement à partir de la date de valeur jusque la date de prise en compte exclue
                            if (pf.STLDate > pf.ValueDate)
                            {
                                applyFromDate = pf.ValueDate;
                                applyToDate = pf.STLDate;
                                //PM 20150319 [POC] Payment à retrancher et non à ajouter
                                //paymentAmount = pf.FlowAmount;
                                paymentAmount = -1 * pf.FlowAmount;
                            }
                            // Si la date de valeur est postérieur à la date de prise en compte
                            //  => Ajouter le versement à partir de la date de prise en compte jusque la date de valeur exclue
                            else
                            {
                                applyFromDate = pf.STLDate;
                                applyToDate = pf.ValueDate;
                                //PM 20150319 [POC] Payment à ajouter et non à retrancher
                                //paymentAmount = -1 * pf.FlowAmount;
                                paymentAmount = pf.FlowAmount;
                            }
                            // Modifier les cash balance pour la période de 'applyFromDate' inclu à 'applyToDate' exclu
                            // PM 20150205 [20778] Ne pas tenir compte du book de l'entity + refactoring
                            //m_InterestFlows.FindAll(f => (f.IdA == pf.IdA)
                            //    && (f.IdB == pf.IdB)
                            //    && (f.IdA_Entity == pf.IdA_Entity)
                            //    && (f.IdB_Entity == pf.IdB_Entity)
                            //    && (f.Currency == pf.Currency)
                            //    && (f.ValueDate >= applyFromDate)
                            //    && (f.ValueDate < applyToDate)).ForEach(f => f.FlowAmount += paymentAmount);
                            List<InterestFlow> flowToUpdate = m_InterestFlows.FindAll(f => (f.IdA == pf.IdA)
                                && (f.IdB == pf.IdB)
                                && (f.IdA_Entity == pf.IdA_Entity)
                                && (f.Currency == pf.Currency)
                                && (f.ValueDate >= applyFromDate)
                                && (f.ValueDate < applyToDate));

                            flowToUpdate.ForEach(f => f.FlowAmount += paymentAmount);
                        }
                    }
                    // PM 20130819 [18582] Mise à jour des données étendues des flux après modification du montant
                    ExtendInterestFlows(m_InterestFlows, false);
                }
            }
        }

        /// <summary>
        /// Application du seuil minimum si défini à tous les montants
        /// </summary>
        // PM 20130819 [18582] Gestion du seuil minimum
        private void ApplyThresholdToFlow()
        {
            foreach (InterestFlow flow in m_InterestFlows)
            {
                if (flow.InterestRule.MinimumThreshold.HasValue && (flow.InterestRule.MinimumThreshold.Value > 0))
                {
                    if (flow.FlowAmount < 0)
                    {
                        flow.FinalAmount = System.Math.Min(0, flow.FlowAmount + flow.InterestRule.MinimumThreshold.Value);
                    }
                    else
                    {
                        flow.FinalAmount = System.Math.Max(0, flow.FlowAmount - flow.InterestRule.MinimumThreshold.Value);
                    }
                }
                else
                {
                    flow.FinalAmount = flow.FlowAmount;
                }
            }
        }

        /// <summary>
        /// Garder uniquement les flux des jours non férié sur le business center
        /// </summary>
        /// PM 20151109 [REC 50] New
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void RemoveFlowOfHoliday()
        {
            m_InterestFlows = (
                from flow in m_InterestFlows
                where (flow.DateBusiness == Tools.ApplyOffset(m_CS, flow.DateBusiness, m_OffsetZero, flow.InterestRule.BDA, null))
                select flow).ToList();
        }

        /// <summary>
        /// Calcul les flux pour chaque jour de la période demandée
        /// </summary>
        internal void CalcAllFlows()
        {
            // PM 20151109 [REC 50] Ne pas prendre les flux des jours fériés sur le business center
            RemoveFlowOfHoliday();
            //
            AddFirstFlow();
            FillMissingFlow();
            ApplyPaymentToFlow();
            // PM 20130819 [18582] Gestion du seuil minimum
            ApplyThresholdToFlow();
        }
        /// <summary>
        /// Fournit la liste de toutes les informations nécéssaire à la création des "trades" cash interest
        /// (Un trade par book)
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public List<CashBalanceInterestTradeInfo> GetTradeInfo()
        {
            m_TradeInfo = ( from flow in m_InterestFlows
                            where (flow.ValueDate >= m_StartPeriodDate) && (flow.ValueDate <= m_ProcessParameter.ProcessDate)
                            group flow by new {
                                flow.IdA,
                                flow.IdB,
                                flow.IdA_Entity,
                                flow.IdB_Entity } into groupedFlow
                            select new CashBalanceInterestTradeInfo(groupedFlow.Key.IdA,
                                groupedFlow.Key.IdB,
                                groupedFlow.Key.IdA_Entity,
                                groupedFlow.Key.IdB_Entity,
                                m_ProcessParameter.AmountTypeEnum,
                                // PM 20130819 [18582] Remplacement de FlowAmount par FinalAmout
                                groupedFlow.Select( f => new CashInterestAmount( f.DateBusiness, f.ValueDate, f.FinalAmount, f.Currency) ).OrderBy(f => f.ValueDate).ToList()
                                )
                          ).ToList();

            foreach (CashBalanceInterestTradeInfo ti in m_TradeInfo)
            {
                ti.TradeDate = m_ProcessParameter.ProcessDate;
                ti.EffectiveDate = m_StartPeriodDate;
                ti.TerminationDate = Tools.ApplyOffset(m_CS, m_ProcessParameter.ProcessDate, m_OffsetPlusOneDay, m_BDACaldendar, null);
                ti.InterestRules = m_InterestRules;
                ti.SetSqlActorBook(m_CS);
                ti.UnAdjustedEffectiveDate = ti.Amounts.First(a => a.ValueDate == ti.EffectiveDate).BusinessDate;
                ti.UnAdjustedTerminationDate = m_ProcessParameter.ProcessDate;
            }
            return (m_TradeInfo);
        }
        #endregion
    }

    /// <summary>
    /// Paramètres de la tâche
    /// </summary>
    internal class InterestProcessParameters
    {
        #region members
        private string m_AmountType = null;
        private InterestAmountTypeEnum m_AmountTypeEnum = InterestAmountTypeEnum.CashBalance;
        private string m_Period = null;
        private PeriodEnum m_PeriodEnum = PeriodEnum.D;

        internal DateTime ProcessDate;
        internal string Currency = null;
        internal int IdA_Cbo = 0;
        internal string Identifier_Cbo = null;
        internal int PeriodMultiplier = 1;
        #endregion
        #region accessors
        internal string ProcessDateISO
        {
            get { return DtFunc.DateTimeToStringDateISO(ProcessDate); }
        }
        internal string AmountType
        {
            get { return m_AmountType; }
            set
            {
                m_AmountType = value;
                m_AmountTypeEnum = (InterestAmountTypeEnum)StringToEnum.Parse(m_AmountType,InterestAmountTypeEnum.CashBalance);
            }
        }
        internal InterestAmountTypeEnum AmountTypeEnum
        {
            get { return m_AmountTypeEnum; }
            set
            {
                m_AmountTypeEnum = value;
                m_AmountType = m_AmountTypeEnum.ToString();
            }
        }
        internal string Period
        {
            get { return m_Period; }
            set
            {
                m_Period = value;
                m_PeriodEnum = (PeriodEnum)StringToEnum.Parse(m_Period,PeriodEnum.D);
            }
        }
        internal PeriodEnum PeriodEnum
        {
            get { return m_PeriodEnum; }
            set
            {
                m_PeriodEnum = value;
                m_Period = m_PeriodEnum.ToString();
            }
        }
        #endregion
        #region constructor
        public InterestProcessParameters()
        {
            AmountTypeEnum = InterestAmountTypeEnum.CashBalance;
            PeriodEnum = PeriodEnum.D;
        }
        #endregion
    }

    /// <summary>
    /// Montant unitaire associé à sa date de valeur
    /// </summary>
    internal class CashInterestAmount : Money
    {
        #region members
        private readonly DateTime m_ValueDate;
        private readonly DateTime m_BusinessDate;
        #endregion members
        #region accessors
        public Money MoneyAmount
        {
            get { return (Money)this; }
        }
        public DateTime ValueDate
        {
            get { return m_ValueDate; }
        }
        public DateTime BusinessDate
        {
            get { return m_BusinessDate; }
        }
        #endregion accessors
        #region constructors
        public CashInterestAmount(DateTime pBusinessDate, DateTime pValueDate, Decimal pAmount, string pCur)
            : base(pAmount, pCur)
        {
            m_BusinessDate = pBusinessDate;
            m_ValueDate = pValueDate;
        }
        #endregion constructors
    }

    /// <summary>
    /// Information nécéssaire à la création d'un "trade" cash interest
    /// </summary>
    internal class CashBalanceInterestTradeInfo
    {
        #region members
        public int IdA_Interest = 0;
        public int IdB_Interest = 0;
        public int IdA_Entity = 0;
        public int IdB_Entity = 0;
        public DateTime TradeDate;
        public DateTime EffectiveDate;
        public DateTime TerminationDate;
        public DateTime UnAdjustedEffectiveDate;
        public DateTime UnAdjustedTerminationDate;
        public InterestAmountTypeEnum InterestTypeEnum;
        // PM 20130819 [18582] Gestion du seuil minimum
        public List<CashInterestAmount> Amounts = null;

        private List<InterestRule> m_InterestRules = null;
        private InterestRule m_CashCoveredInitialMarginRule = null;
        private InterestRule m_CreditCashBalanceRule = null;
        private InterestRule m_DebitCashBalanceRule = null;
        private SQL_Actor sqlEntity;
        private SQL_Book sqlEntityBook;
        private SQL_Actor sqlInterestActor;
        private SQL_Book sqlInterestBook;
        #endregion members
        #region accessors
        public SQL_Actor SqlEntity
        {
            get { return sqlEntity; }
        }
        public SQL_Book SqlEntityBook
        {
            get { return sqlEntityBook; }
        }
        public SQL_Actor SqlInterestActor
        {
            get { return sqlInterestActor; }
        }
        public SQL_Book SqlInterestBook
        {
            get { return sqlInterestBook; }
        }
        public List<InterestRule> InterestRules
        {
            get { return m_InterestRules; }
            set
            {
                m_InterestRules = value;
                if (null != m_InterestRules)
                {
                    m_CashCoveredInitialMarginRule = m_InterestRules.FirstOrDefault(r => r.AmountTypeEnum == InterestAmountTypeEnum.CashCoveredInitialMargin);
                    m_CreditCashBalanceRule = m_InterestRules.FirstOrDefault(r => r.AmountTypeEnum == InterestAmountTypeEnum.CreditCashBalance);
                    m_DebitCashBalanceRule = m_InterestRules.FirstOrDefault(r => r.AmountTypeEnum == InterestAmountTypeEnum.DebitCashBalance);
                }
            }
        }
        public InterestRule CashCoveredInitialMarginRule
        {
            get { return m_CashCoveredInitialMarginRule; }
        }
        public InterestRule CreditCashBalanceRule
        {
            get { return m_CreditCashBalanceRule; }
        }
        public InterestRule DebitCashBalanceRule
        {
            get { return m_DebitCashBalanceRule; }
        }
        #endregion accessors
        #region constructors
        public CashBalanceInterestTradeInfo() { }
        public CashBalanceInterestTradeInfo(int pIdA, int pIdB, int pIdAEntity, int pIdBEntity, InterestAmountTypeEnum pInterestTypeEnum, List<CashInterestAmount> pAmount)
        {
            IdA_Interest = pIdA;
            IdB_Interest = pIdB;
            IdA_Entity = pIdAEntity;
            IdB_Entity = pIdBEntity;
            InterestTypeEnum = pInterestTypeEnum;
            Amounts = pAmount;
        }
        #endregion constructors
        #region methods
        /// <summary>
        /// Alimente les SQL_TABLE associés aux éléments Ida, Idb, Ida_Entity
        /// </summary>
        public void SetSqlActorBook(string pCS)
        {
            sqlInterestActor = new SQL_Actor(pCS, IdA_Interest);
            sqlInterestActor.LoadTable();

            if (IdB_Interest != 0)
            {
                sqlInterestBook = new SQL_Book(pCS, IdB_Interest);
                sqlInterestBook.LoadTable();
            }
            else
            {
                sqlInterestBook = default;
            }

            sqlEntity = new SQL_Actor(pCS, IdA_Entity);
            sqlEntity.LoadTable();

            if (IdB_Entity != 0)
            {
                sqlEntityBook = new SQL_Book(pCS, IdB_Entity);
                sqlEntityBook.LoadTable();
            }
            else
            {
                sqlEntityBook = default;
            }
        }
        /// <summary>
        /// Obtient les règles de calcul en fonction du type de montant
        /// </summary>
        /// <param name="pAmounTypeEnum">Type de montant dont on veux les règles de calcul</param>
        /// <returns>Règles de calcul du montant demandées</returns>
        public InterestRule GetInterestRule(InterestAmountTypeEnum pAmounTypeEnum)
        {
            InterestRule rule = default;
            switch (pAmounTypeEnum)
            {
                case InterestAmountTypeEnum.CashCoveredInitialMargin:
                    rule = m_CashCoveredInitialMarginRule;
                    break;
                case InterestAmountTypeEnum.CreditCashBalance:
                    rule = m_CreditCashBalanceRule;
                    break;
                case InterestAmountTypeEnum.DebitCashBalance:
                    rule = m_DebitCashBalanceRule;
                    break;
            }
            return rule;
        }
        #endregion methods
    }
}
