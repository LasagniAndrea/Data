using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EFS.Common;
using EFS.SpheresRiskPerformance.CommunicationObjects;

using EfsML.Business;

using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the "TIMS IDEM" Risk method, as used by the CCG italian clearing house (IDEM derivative market)
    /// ASYNCHRONE METHODS
    /// </summary>
    public sealed partial class TimsIdemMethod
    {

        #region EvaluateRiskElementContractByTask
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Contract
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pContractComObj">Contrat</param>
        /// <param name="pPositionActionActorBook">position NET nécessaire au calcul du Déposit de la position des positions action</param>
        /// <param name="pSpreadEvalParams">Spread margin parameters</param>
        /// <param name="pPremiumEvalParams">Premium margin parameters</param>
        /// <param name="pAdditionalEvalParams">Additional margin parameters</param>
        /// <param name="pMTMEvalParams">MTM margin parameters</param>
        /// <param name="pIsClassGroupToMerge"></param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementContractByTask(int pActorId, int pBookId, TimsIdemContractParameterCommunicationObject pContractComObj,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams,
            IEnumerable<PremiumEvaluationParameters> pPremiumEvalParams,
            IEnumerable<AdditionalEvaluationParameters> pAdditionalEvalParams,
            IEnumerable<MtmEvaluationParameters> pMTMEvalParams,
            bool pIsClassGroupToMerge)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Contract: {2})", pActorId, pBookId, pContractComObj.Contract);
            string wait = "EvaluateRiskElementContract {0} Wait   : {1} " + key;
            string release = "EvaluateRiskElementContract {0} Release: {1} " + key;

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementContract {0}", key));

            List<Task> lstTasks = new List<Task>();

            // Calcul du Futures Spread Margin par Contract
            if (false == pIsClassGroupToMerge)
            {
                Task spreadTask = Task.Run(() =>
                    {
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "SPREAD", SemaphoreDeposit.CurrentCount));
                        pContractComObj.Spread = GetSpreadMargin(pContractComObj.SpreadPositions, pSpreadEvalParams);
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "SPREAD", SemaphoreDeposit.CurrentCount));
                    });
                lstTasks.Add(spreadTask);
            }

            // Calcul du Premium Margin par Contract
            Task premiumTask = Task.Run(() => 
                    {
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "PREMIUM", SemaphoreDeposit.CurrentCount));
                        pContractComObj.Premium = GetPremiumMargin(pContractComObj.Positions, pPremiumEvalParams);
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "PREMIUM", SemaphoreDeposit.CurrentCount));
                    });
            lstTasks.Add(premiumTask);

            // Calcul de l'Additional Margin par Contract
            // Calcul des montants Additional, une fois sans Offset, une fois avec Offset
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> positionActionContract =
                from position in pPositionActionActorBook
                where position.First.derivativeContractSymbol == pContractComObj.Contract
                select position;

            Task additionnalTask = Task.Run(() => 
                    {
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "ADDITIONAL", SemaphoreDeposit.CurrentCount));
                        pContractComObj.Additional = GetAdditionalMargin(pContractComObj.Positions, positionActionContract, pAdditionalEvalParams, null);
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "ADDITIONAL", SemaphoreDeposit.CurrentCount));
                    });
            lstTasks.Add(additionnalTask);
            Task additionnalWithOffsetTask = Task.Run(() => 
                    {
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "ADDITIONALOFFSET", SemaphoreDeposit.CurrentCount));
                        pContractComObj.AdditionalWithOffset = GetAdditionalMargin(pContractComObj.Positions, positionActionContract, pAdditionalEvalParams, pContractComObj.Offset);
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "ADDITIONALOFFSET", SemaphoreDeposit.CurrentCount));
                    });
            lstTasks.Add(additionnalWithOffsetTask);

            // Calcul du Mark to Market Margin for unsettled stock futures contracts par Contract
            Task mtmTask = Task.Run(() =>
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "MTM", SemaphoreDeposit.CurrentCount));
                    pContractComObj.Mtm = GetMtmMargin(pContractComObj.Positions, positionActionContract, pMTMEvalParams);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "MTM", SemaphoreDeposit.CurrentCount));
                });
            lstTasks.Add(mtmTask);

            try
            {
                Task.WaitAll(lstTasks.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementContract {0}", key));
            }
        }
        #endregion EvaluateRiskElementContractByTask
        #region EvaluateRiskElementClassByTask
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Classe
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pClassComObj">Classe</param>
        /// <param name="pPositionActionActorBook">position NET nécessaire au calcul du Déposit de la position des positions action</param>
        /// <param name="pSpreadEvalParams">Spread margin parameters</param>
        /// <param name="pMinimumEvalParams">Deposit Minimum parameters</param>
        /// <param name="pIsClassGroupToMerge">Indicateur de regroupement des positions Future</param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementClassByTask(int pActorId, int pBookId, TimsIdemClassParameterCommunicationObject pClassComObj,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams, IEnumerable<MinimumEvaluationParameters> pMinimumEvalParams, bool pIsClassGroupToMerge)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Class: {2})", pActorId, pBookId, pClassComObj.Class);
            string start    = "START EvaluateRiskElementClass {0} " + key;
            string stop = "STOP  EvaluateRiskElementClass {0} " + key;

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementClass {0}", key));

            List<Task> lstTasks = new List<Task>();

            // Calcul du Futures Spread Margin par Class
            Task spreadTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(start, "SPREAD"));

                    if (pIsClassGroupToMerge)
                        pClassComObj.Spread = GetSpreadMargin(pClassComObj.SpreadPositions, pSpreadEvalParams);
                    else
                        pClassComObj.Spread = AggregateMargin(
                            from parameter in pClassComObj.Parameters
                            select ((TimsIdemContractParameterCommunicationObject)parameter).Spread);

                    AppInstance.TraceManager.TraceVerbose(this, String.Format(stop, "SPREAD"));
                });
            lstTasks.Add(spreadTask);

            // Cumul des Mark to Market Margin for unsettled stock futures contracts par Class
            Task mtmTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(start, "MTM"));
                    pClassComObj.Mtm = AggregateMargin(
                        from parameter in pClassComObj.Parameters
                        select ((TimsIdemContractParameterCommunicationObject)parameter).Mtm);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(stop, "MTM"));
                });
            lstTasks.Add(mtmTask);

            // Cumul des Premium Margin par Class
            Task premiumTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(start, "PREMIUM"));
                    pClassComObj.Premium = AggregateMargin(
                        from parameter in pClassComObj.Parameters
                        select ((TimsIdemContractParameterCommunicationObject)parameter).Premium);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(stop, "PREMIUM"));
                });
            lstTasks.Add(premiumTask);

            // Cumul des Additional Margin par Class
            // Sommer uniquement les matrices et pas les montants
            // Gestion offset sur la somme des riskarrays des contracts de la class
            IEnumerable<TimsDecomposableParameterCommunicationObject> additional =
                from parameter in pClassComObj.Parameters
                select ((TimsIdemContractParameterCommunicationObject)parameter).Additional;

            IEnumerable<TimsDecomposableParameterCommunicationObject> additionalWithOffset =
                from parameter in pClassComObj.Parameters
                select ((TimsIdemContractParameterCommunicationObject)parameter).AdditionalWithOffset;

            Task additionalTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(start, "ADDITIONAL"));
                    pClassComObj.Additional = AggregateClassAdditionalMargin(additional, additionalWithOffset);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(stop, "ADDITIONAL"));
                });

            lstTasks.Add(additionalTask);

            try
            {
                Task.WaitAll(lstTasks.ToArray());

                AppInstance.TraceManager.TraceVerbose(this, String.Format(start, "MINIMUM"));

                // Calcul du Deposit Minimum par Class
                IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> positionActionClass =
                        from position in pPositionActionActorBook
                        join contractComObj in pClassComObj.Parameters
                        on position.First.derivativeContractSymbol
                        equals ((TimsIdemContractParameterCommunicationObject)contractComObj).Contract
                        select position;

                pClassComObj.Minimum = GetMinimumMargin2(
                    pClassComObj.Positions, positionActionClass, pMinimumEvalParams,
                    pClassComObj.Premium?.MarginAmount);

                AppInstance.TraceManager.TraceVerbose(this, String.Format(stop, "MINIMUM"));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementClass {0}", key));
            }
        }
        #endregion EvaluateRiskElementClassByTask
        #region EvaluateRiskElementProductByTask
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Produit
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pProductComObj">Produit</param>
        /// <param name="pAmounts">Liste des montants calculés</param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementProductByTask(int pActorId, int pBookId, TimsIdemProductParameterCommunicationObject pProductComObj, ref List<Money> pAmounts)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Product: {2})", pActorId, pBookId, pProductComObj.Product);
            string wait = "EvaluateRiskElementProduct {0} Wait   : {1} " + key;
            string release = "EvaluateRiskElementProduct {0} Release: {1} " + key;

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementProduct {0}", key));

            List<Task> lstTasks = new List<Task>();

            // Cumul des Spread Margin par Product
            Task spreadTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "SPREAD", SemaphoreDeposit.CurrentCount));
                    pProductComObj.Spread = AggregateMargin(
                        from parameter in pProductComObj.Parameters
                        select ((TimsIdemClassParameterCommunicationObject)parameter).Spread);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "SPREAD", SemaphoreDeposit.CurrentCount));
                });

            lstTasks.Add(spreadTask);

            // Cumul des Mark to Market Margin for unsettled stock futures contracts par Product
            Task mtmTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "MTM", SemaphoreDeposit.CurrentCount));
                    pProductComObj.Mtm = AggregateMargin(
                        from parameter in pProductComObj.Parameters
                        select ((TimsIdemClassParameterCommunicationObject)parameter).Mtm);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "MTM", SemaphoreDeposit.CurrentCount));
                });

            lstTasks.Add(mtmTask);

            // Cumul des Premium Margin par Product
            Task premiumTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "PREMIUM", SemaphoreDeposit.CurrentCount));
                    pProductComObj.Premium = AggregateMargin(
                        from parameter in pProductComObj.Parameters
                        select ((TimsIdemClassParameterCommunicationObject)parameter).Premium);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "PREMIUM", SemaphoreDeposit.CurrentCount));
                });

            lstTasks.Add(premiumTask);

            // Cumul des Additional Margin par Product
            Task additionalTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "ADDITIONAL", SemaphoreDeposit.CurrentCount));
                    pProductComObj.Additional = AggregateAdditionalMargin(
                        from parameter in pProductComObj.Parameters
                        select ((TimsIdemClassParameterCommunicationObject)parameter).Additional);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "ADDITIONAL", SemaphoreDeposit.CurrentCount));
                });
            lstTasks.Add(additionalTask);

            // Cumul des Deposit Minimum par Product
            Task minimumTask = Task.Run(() => 
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "MINIMUM", SemaphoreDeposit.CurrentCount));
                    pProductComObj.Minimum = AggregateMargin(
                        from parameter in pProductComObj.Parameters
                        select ((TimsIdemClassParameterCommunicationObject)parameter).Minimum);
                    AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "MINIMUM", SemaphoreDeposit.CurrentCount));
                });

            lstTasks.Add(minimumTask);

            // Calcul du deposit final par Product
            try
            {
                Task.WaitAll(lstTasks.ToArray());

                AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, "MARGINAMOUNT", SemaphoreDeposit.CurrentCount));

                pProductComObj.MarginAmount = GetProductMarginAmount(
                        pProductComObj.Mtm?.MarginAmount,
                        pProductComObj.Spread?.MarginAmount,
                        pProductComObj.Premium?.MarginAmount,
                        pProductComObj.Additional?.MarginAmount,
                        pProductComObj.Minimum?.MarginAmount);

                SumAmounts(new Money[] { (Money)pProductComObj.MarginAmount }, ref pAmounts);

                AppInstance.TraceManager.TraceVerbose(this, String.Format(release, "MARGINAMOUNT", SemaphoreDeposit.CurrentCount));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementProduct {0}", key));
            }
        }
        #endregion EvaluateRiskElementProductByTask

        /// <summary>
        /// Identique à GetMinimumMargin (mais utilisé en mode asynchrone)
        /// </summary>
        // EG 20180205 [23769] New
        private TimsDecomposableParameterCommunicationObject GetMinimumMargin2
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pClassPositionAction,
            IEnumerable<MinimumEvaluationParameters> pMinimumEvalParameters, IMoney pPremiumAmount)
        {

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Start GetMinimumMargin {0}", "GetMinimumMarginFactors"));
            IEnumerable<TimsFactorCommunicationObject> factors =
                GetMinimumMarginFactors2(pClassPositions, pClassPositionAction, pMinimumEvalParameters);
            AppInstance.TraceManager.TraceVerbose(this, String.Format("End GetMinimumMargin {0}", "GetMinimumMarginFactors"));

            IEnumerable<TimsFactorCommunicationObject> factorsFuture =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Future))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.FutureMoff))
                select factor;

            IEnumerable<TimsFactorCommunicationObject> factorsAction = 
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.PositionAction))
                select factor;

            IEnumerable<TimsFactorCommunicationObject> factorsCall =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Call))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.ExeAssCall))
                select factor;

            IEnumerable<TimsFactorCommunicationObject> factorsPut =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Put))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.ExeAssPut))
                select factor;

            // Abs( Sum( Montant Minimum Future ) ) + Abs( Sum( Montant Minimum Action ) )
            IEnumerable<IMoney> factorAmounts = (
                from factor in factorsFuture
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney)new Money(RoundAmount(System.Math.Abs(factorCur.Sum(f => f.MarginAmount.Amount.DecValue))), factorCur.Key)
                ).Concat(
                from factor in factorsAction
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney)new Money(RoundAmount(System.Math.Abs(factorCur.Sum(f => f.MarginAmount.Amount.DecValue))), factorCur.Key)
                );

            // Abs( Sum( Montant Minimum Call ) ) + Abs( Sum( Montant Minimum Put ) )
            IEnumerable<IMoney> factorOptionAmounts = (
                from factor in factorsCall
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney)new Money(RoundAmount(System.Math.Abs(factorCur.Sum(f => f.MarginAmount.Amount.DecValue))), factorCur.Key)
                ).Concat(
                from factor in factorsPut
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney)new Money(RoundAmount(System.Math.Abs(factorCur.Sum(f => f.MarginAmount.Amount.DecValue))), factorCur.Key)
                );

            bool bUsePremiumAmount = false;

            // using premium amount when the premium amount is a credit and the minimum amount for option is greater than the premium amount
            if (pPremiumAmount == null || pPremiumAmount.Amount.DecValue <= 0)
            {
                decimal optionMinAmounts = factorOptionAmounts.Sum(a => a.Amount.DecValue);

                if ((pPremiumAmount == null && optionMinAmounts > 0)
                   || (pPremiumAmount != null && optionMinAmounts > System.Math.Abs(pPremiumAmount.Amount.DecValue)))
                {
                    bUsePremiumAmount = true;
                }
            }

            if (bUsePremiumAmount)
            {
                if (pPremiumAmount != null)
                {
                    factorAmounts = factorAmounts.Concat(new IMoney[] { new Money(System.Math.Abs(pPremiumAmount.Amount.DecValue), pPremiumAmount.Currency) });
                }
            }
            else
            {
                factorAmounts = factorAmounts.Concat(factorOptionAmounts);
            }

            IEnumerable<Money> amounts = SumAmounts(from amount in factorAmounts select (Money)amount, null);

            TimsDecomposableParameterCommunicationObject minimumComObj = new TimsDecomposableParameterCommunicationObject
            {
                Factors = factors
            };
            // One currency only is attended for a minimum margin (taking the first amount)
            // PM 20130528 Vérification qu'il y ait au moins un montant
            if (amounts.Count() != 0)
            {
                minimumComObj.MarginAmount = amounts.First();
            }
            //else
            //{
            //    minimumComObj.MarginAmount = new Money();
            //}
            return minimumComObj;
        }

        /// <summary>
        /// Identique à GetMinimumMarginFactors (mais utilisé en mode asynchrone)
        /// </summary>
        // EG 20180205 [23769] New
        private IEnumerable<TimsFactorCommunicationObject> GetMinimumMarginFactors2(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pClassPositionAction,
            IEnumerable<MinimumEvaluationParameters> pMinimumEvalParameters)
        {

            var posCategoryAndMinimum = (
                from position in pClassPositions
                join data in pMinimumEvalParameters on position.First.idAsset equals data.AssetID
                select new
                {
                    EvaluationData = data,
                    Position = position
                }).GroupBy(key => new { key.EvaluationData.Type, key.EvaluationData.MinimumRate });

            var posActionCategoryAndMinimum = (
                from position in pClassPositionAction
                join data in pMinimumEvalParameters on position.First.idAsset equals data.AssetID
                where data.Type == RiskMethodQtyType.PositionAction
                select new
                {
                    EvaluationData = data,
                    Position = position
                }).GroupBy(key => new { key.EvaluationData.Type, key.EvaluationData.MinimumRate });

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Start GetMinimumMarginFactors {0}", "Set factor"));

            RiskMethodQtyType[] method = new RiskMethodQtyType[] { RiskMethodQtyType.Future, RiskMethodQtyType.Call, RiskMethodQtyType.Put };

            HashSet<TimsFactorCommunicationObject> hsFactor = new HashSet<TimsFactorCommunicationObject>(
                from positionsByCategory in posCategoryAndMinimum
                select new TimsFactorCommunicationObject
                {
                    Identifier = System.Enum.GetName(typeof(RiskMethodQtyType), positionsByCategory.Key.Type),

                    MinimumRate = positionsByCategory.Key.MinimumRate,

                    Quantity =

                        method.Contains(positionsByCategory.Key.Type)

                        ?

                        (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetSellFIXmlSide() select elem.Position.Second.Quantity).Sum()
                         -
                        (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetBuyFIXmlSide() select elem.Position.Second.Quantity).Sum()
                        :
                        // the input position grouped by series have an exe/ass quantity value already provided by sign (ass positive, exe negative)
                        (from elem in positionsByCategory select elem.Position.Second.ExeAssQuantity).Sum(),

                    MarginAmount = new Money(0, (from elem in positionsByCategory select elem.EvaluationData.Currency).First())
                });

            hsFactor.UnionWith(
                    from positionsByCategory in posActionCategoryAndMinimum
                    select new TimsFactorCommunicationObject
                    {
                        Identifier = (from elem in positionsByCategory select elem.EvaluationData.CrossMargin).First() ?
                            System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.PositionAction) : "NoCrossMargin",

                        MinimumRate = positionsByCategory.Key.MinimumRate,

                        Quantity =

                            (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetSellFIXmlSide() select elem.Position.Second.Quantity).Sum()
                             -
                            (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetBuyFIXmlSide() select elem.Position.Second.Quantity).Sum(),

                        MarginAmount = new Money(0, (from elem in positionsByCategory select elem.EvaluationData.Currency).First())
                    });

            foreach(TimsFactorCommunicationObject f in hsFactor)
            {
                f.MarginAmount.Amount.DecValue = (int)f.Quantity * (decimal)f.MinimumRate;
            }

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Stop GetMinimumMarginFactors {0}", "Set factor"));

            return hsFactor;
        }

    }
}