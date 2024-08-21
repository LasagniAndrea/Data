using System.Collections.Generic;
using System.Linq;
//
using EFS.SpheresRiskPerformance.CommunicationObjects;
//
using EfsML.v30.MarginRequirement;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheetRepository, contient le resultats et le détail du calcul de déposit
    /// </summary>
    /// <remarks>Cette partie de la class inclue les membres permettant de construire le détail du calcul par la méthode None</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        private MarginCalculationMethod BuildNoMarginCalculationMethod(NoMarginCalcMethCom pMethodComObj)
        {
            NoMarginCalculationMethod method = new NoMarginCalculationMethod();

            // Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(NoMarginCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                //
                method.ParametersDateSpecified = false;
                //
                if ((pMethodComObj.AssetGroup != default(List<NoMarginMethAssetGroupCom>)) && (pMethodComObj.AssetGroup.Count > 0))
                {
                    method.AssetGroupDetails = (from asset in pMethodComObj.AssetGroup
                                                select new NoMarginMethAssetGroupDetail
                                                {
                                                    IdM = asset.IdM,
                                                    MarketIdentifier = asset.MarketIdentifier,
                                                    IdI = asset.IdI,
                                                    IdContract = asset.IdContract,
                                                    ContractIdentifier = asset.ContractIdentifier,
                                                    IdAsset = asset.IdAsset,
                                                    AssetIdentifier = asset.AssetIdentifier,
                                                    AssetCategory = asset.AssetCategory,
                                                    Currency = asset.Currency,
                                                    TradesDetails = (asset.Trades != default(List<NoMarginMetTradesCom>) ? (from trd in asset.Trades
                                                                                                                        select new NoMargineMetTradesDetail
                                                                                                                        {
                                                                                                                            IdT = trd.IdT,
                                                                                                                            TradeIdentifier = trd.TradeIdentifier,
                                                                                                                            IdA_Dealer = trd.IdA_Dealer,
                                                                                                                            IdB_Dealer = trd.IdB_Dealer,
                                                                                                                            IdA_Clearer = trd.IdA_Clearer,
                                                                                                                            IdB_Clearer = trd.IdB_Clearer,
                                                                                                                            DtBusiness = trd.DtBusiness,
                                                                                                                            DtTimestamp = trd.DtTimestamp,
                                                                                                                            DtExecution = trd.DtExecution,
                                                                                                                            TzFacility = trd.TzFacility,
                                                                                                                            Side = trd.Side,
                                                                                                                            Quantity = trd.Quantity,
                                                                                                                            Price = trd.Price,
                                                                                                                        }).ToArray() : default),
                                                }).ToArray();
                }
                
            }
            return method;
        }
    }
}
