using System;
using System.Collections.Generic;
using System.Linq;

using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.Spheres.Hierarchies;
using EFS.EFSTools;
using EFS.Common;

using FpML.v44.Shared;
using EfsML.v30.MarginRequirement;
using EFS.SpheresRiskPerformance.CalculationSheet;

namespace EFS.SpheresRiskPerformance.RiskMethods
{

    /// <summary>
    /// Base class representing a generic Risk method used to compute a deposit
    /// </summary>
    public abstract class BaseMethod
    {
        /// <summary>
        /// method type
        /// </summary>
        public abstract RiskMethodType Type { get; }

        /// <summary>
        /// internal id of the clearing house using the current method
        /// </summary>
        public int IdCSS { get; internal set; }

        /// <summary>
        /// Business date
        /// </summary>
        public DateTime DtBusiness { get; internal set; }

        SerializableDictionary<Pair<int, int>, Deposit> m_Deposits =
            new SerializableDictionary<Pair<int, int>, Deposit>("ActorId_BookId", "Deposit", new PairComparer<int,int>());

        /// <summary>
        /// Deposits dictionary, research key by actor/book
        /// </summary>
        public SerializableDictionary<Pair<int,int>, Deposit> Deposits
        {
            get
            {
                return m_Deposits;
            }
        }

        /// <summary>
        /// Initialize the deposit collection
        /// </summary>
        /// <param name="pActorsRoleMarginReqOffice">MARGINREQOFFICE actors process list</param>
        /// <param name="pTiming">process timing of the evaluation as requested by the user</param>
        public void InitializeDeposits(List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice, RiskEvaluationTiming pTiming)
        {
            var depositInputDatas =
               from actor in pActorsRoleMarginReqOffice
               from attribute in actor.RoleSpecificAttributes
               where attribute is RoleMarginReqOfficeAttribute
               from rootElement in ((RoleMarginReqOfficeAttribute)attribute).RootElements
               select new
               {
                   Root = rootElement,
                   IsGrossMargining = 
                       ((RoleMarginReqOfficeAttribute)attribute).IsGrossMargining,
                   ActorsBooksInPosition =
                       ((RoleMarginReqOfficeAttribute)attribute).
                           GetPairsActorBookConstitutingPosition(rootElement.RiskElementClass, rootElement.AffectedBookId),
                   RiskElements =
                       ((RoleMarginReqOfficeAttribute)attribute).
                           GetCalculationElements(rootElement.RiskElementClass, rootElement.AffectedBookId),
                   Result =
                    ((RoleMarginReqOfficeAttribute)attribute).
                        GetPreviousTradeRisk(this.IdCSS, rootElement.AffectedBookId, pTiming),
                   // TODO 20110516 MF la collection ancestors est vide pour le moment...
                   Ancestors = new List<Pair<int,int>>()         

               };

            foreach (var inputDatas in depositInputDatas)
            {
                Deposit deposit = new Deposit(
                    inputDatas.Root, 
                    inputDatas.IsGrossMargining, 
                    inputDatas.RiskElements, 
                    inputDatas.ActorsBooksInPosition,
                    inputDatas.Result, 
                    inputDatas.Ancestors);

                this.Deposits.Add(
                    new Pair<int, int>(inputDatas.Root.ActorId, inputDatas.Root.AffectedBookId),
                    deposit);
            }

        }

        /// <summary>
        /// Main loop on all the built deposit items
        /// </summary>
        /// <returns></returns>
        public void Evaluate()
        {
            foreach (KeyValuePair<Pair<int,int>, Deposit> keyValue in this.Deposits)
            {
                EvaluateDeposit(keyValue.Key.First, keyValue.Key.Second, keyValue.Value);
            }
        }

        /// <summary>
        /// Initialize the method
        /// </summary>
        /// <returns>true if the initialization process is good</returns>
        public abstract void LoadParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache);

        /// <summary>
        /// Evaluate one deposit factor
        /// </summary>
        /// <returns></returns>
        public abstract List<Money> EvaluateRiskElement(
            int pActorId, int pBookId, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
            out ICalculationSheetMethod opCalculationMethod);

        private List<Money> EvaluateDeposit(int pActorId, int pBookId, Deposit pDepositToEvaluate)
        {
            // 1. Deposit already evaluated, return the amounts
            if (pDepositToEvaluate.Status == DepositStatus.EVALUATED)
            {
                return pDepositToEvaluate.Amounts;
            }

            pDepositToEvaluate.Status = DepositStatus.EVALUATING;

            List<Money> amounts = null;

            ICalculationSheetMethod calculationMethod = null;

            // 2. Deposit not yet evaluate, net evaluation
            if (!pDepositToEvaluate.IsGrossMargining)
            {
                // 2.1  Get all the positions
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions =
                    PositionsExtractor.GetPositions(pDepositToEvaluate.Factors);

                // 2.2 Evaluate...

                amounts = EvaluateRiskElement(pActorId, pBookId, positions, out calculationMethod);

            }
            else
            // 2. Deposit not yet evaluate, gross evaluation
            {
                // 2.1 Evaluate each factor separately...
                foreach (RiskElement factor in pDepositToEvaluate.Factors)
                {
                    Pair<int, int> keyDeposit = new Pair<int, int>(factor.ActorId, factor.AffectedBookId);

                    // 2.2 the factor is a deposit owned by another actor? 
                    bool bGetAmountsSubDeposit = 
                        factor.ActorId != pActorId && factor.AffectedBookId != pBookId && Deposits.ContainsKey(keyDeposit);

                    List<Money> subamounts = null;

                    // 2.2.1 Yes, the factor is a deposit owned by another actor, recursive call...
                    if (bGetAmountsSubDeposit)
                    {
                        Deposit deposit = Deposits[keyDeposit];

                        if (deposit.Status == DepositStatus.NOTEVALUATED)
                        {
                            subamounts = EvaluateDeposit(keyDeposit.First, keyDeposit.Second, deposit);
                        }
                        else
                        {
                            subamounts = deposit.Amounts;
                        }
                    }
                    else
                    // 2.2.2 No, the factor is one of the risk elements of the current deposit owner, evaluate the factor...
                    {
                        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions =PositionsExtractor.GetPositions(factor);

                        subamounts = EvaluateRiskElement(factor.ActorId, factor.AffectedBookId, positions, out calculationMethod);

                        // UNDONE calculation method elements for gross risk elements ?? demande à Philippe soit Fabrice
                    }

                    // 2.3 Copy the sub amounts to the total (amounts)
                    SumAmounts(subamounts, ref amounts);
                }
            }

            // 3. The deposit is evaluated, returning the amounts

            pDepositToEvaluate.Status = DepositStatus.EVALUATED;

            pDepositToEvaluate.Amounts = amounts;

            pDepositToEvaluate.MarginCalculationMethod = calculationMethod;

            return pDepositToEvaluate.Amounts;
        }

        /// <summary>
        /// Merge two input money collections (multiple currencies) into the second one, making the sum of the amounts by currency
        /// </summary>
        /// <param name="pSourceAmounts">source money collection</param>
        /// <param name="pDestAmounts">dest money collection</param>
        public static void SumAmounts(IEnumerable<Money> pSourceAmounts, ref List<Money> pDestAmounts)
        {
            if (pDestAmounts == null)
            {
                pDestAmounts = new List<Money>();
            }

            pDestAmounts = (
                from amountsByCurrency
                    in (pSourceAmounts.Union(pDestAmounts).GroupBy(money => money.Currency))
                select
                    new Money(
                        (from amount in amountsByCurrency select amount.Amount.DecValue).Sum(),
                        amountsByCurrency.Key
                    )
               ).ToList();
        }
    }

    /// <summary>
    /// Risk method class factory
    /// </summary>
    public static class RiskMethodFactory
    {
        static Dictionary<RiskMethodType, BaseMethod> m_Methods = new Dictionary<RiskMethodType, BaseMethod>();

        /// <summary>
        /// Get all the built methods
        /// </summary>
        public static Dictionary<RiskMethodType, BaseMethod> Methods
        {
          get { return RiskMethodFactory.m_Methods; }
        }

        /// <summary>
        /// Get the method of the given type
        /// </summary>
        /// <param name="pIdCSS">clearing house, using the method to be built</param>
        /// <param name="pMethodType">type of the method we want to get back</param>
        /// <returns></returns>
        static public BaseMethod BuildMethod(int pIdCSS, DateTime pDtBusiness, RiskMethodType pMethodType)
        {
            if (!Methods.ContainsKey(pMethodType))
            {
                BaseMethod method = null;

                switch (pMethodType)
                {
                    case RiskMethodType.STANDARD:

                        method = new StandardMethod();

                        break;

                    default:

                        throw new NotSupportedException();
                }

                method.IdCSS = pIdCSS;
                method.DtBusiness = pDtBusiness;

                Methods.Add(pMethodType, method);
            }

            return Methods[pMethodType];
        }
    }


}