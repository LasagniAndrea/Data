using System;
using System.Collections.Generic;
using System.Linq;
//
using EFS.ACommon;


namespace EFS.Common
{
    /// <summary>
    /// Représente un cache en mémoire de <see cref="IAssetETDIdent"/> 
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class MarketDataAssetETD
    {
        #region Accessors
        /// <summary>
        /// Ensemble des Assets
        /// </summary>
        public List<IAssetETDIdent> AllAsset
        {
            get;
            private set;
        }

        /// <summary>
        /// Ensemble des Assets par ContractSymbol
        /// </summary>
        public Dictionary<string, List<IAssetETDIdent>> AssetByCtrSym
        {
            get;
            private set;
        }

        /// <summary>
        /// Ensemble des Assets par ElectronicContractSymbol
        /// </summary>
        public Dictionary<string, List<IAssetETDIdent>> AssetByElecCtrSym
        {
            get; 
            private set;
        }

         /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> CtrSym
        {
            get { return AssetByCtrSym.Keys; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ElecCtrSym
        {
            get
            {
                return AssetByElecCtrSym.Keys;
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public MarketDataAssetETD()
        {
            AssetByCtrSym = new Dictionary<string, List<IAssetETDIdent>>();
            AssetByElecCtrSym = new Dictionary<string, List<IAssetETDIdent>>();
            AllAsset = new List<IAssetETDIdent>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetETD">Liste des assets</param>
        public MarketDataAssetETD(IEnumerable<IAssetETDIdent> pAssetETD) : this()
        {
            LoadAsset(pAssetETD);
        }

        #endregion

        #region Method 
        /// <summary>
        /// Chargement des assets 
        /// </summary>
        /// <param name="pAssetETD">Liste des assets</param>
        public void LoadAsset(IEnumerable<IAssetETDIdent> pAssetETD)
        {

            AllAsset = pAssetETD.ToList();


            AssetByCtrSym = (
                from asset in AllAsset.Where(x => StrFunc.IsFilled(x.ContractSymbol))
                group asset by asset.ContractSymbol
                    into assetByContract
                select new
                {
                    assetByContract.Key,
                    Value = assetByContract.ToList(),
                }).ToDictionary(e => e.Key, e => e.Value);


            AssetByElecCtrSym = (
                from asset in AllAsset.Where(x => StrFunc.IsFilled(x.ElectronicContractSymbol))
                group asset by asset.ElectronicContractSymbol
                    into assetByContract
                select new
                {
                    assetByContract.Key,
                    Value = assetByContract.ToList(),
                }).ToDictionary(e => e.Key, e => e.Value);
        }
        
        /// <summary>
        /// Indique s'il existe des assets pour le contract Symbol <paramref name="pContractSymbol"/>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        public bool IsExistContractSymbol(string pContractSymbol)
        {
            return AssetByCtrSym.ContainsKey(pContractSymbol);
        }

        /// <summary>
        /// Indique s'il existe des assets pour le ElectonicContractSymbol Symbol <paramref name="pContractElectonicSymbol"/>
        /// </summary>
        /// <param name="pContractElectonicSymbol"></param>
        /// <returns></returns>
        public bool IsExistElectonicContractSymbol(string pContractElectonicSymbol)
        {
            return AssetByElecCtrSym.ContainsKey(pContractElectonicSymbol);
        }

        /// <summary>
        /// Recherche d'un asset
        /// </summary>
        /// <param name="settings">Pilote de la recherche</param>
        /// <param name="assetRequest">Critères de recherche</param>
        /// <returns>null si asset non trouvé</returns>
        // PM 20230622 [26091][WI390] Ajout gestion de IsWithContractMultiplier et IsWithIsinCode
        // PM 20240122 [WI822] Ajout gestion pIsWithPriceCurrency
        public IAssetETDIdent GetAsset(MarketAssetETDRequestSettings settings, MarketAssetETDRequest assetRequest)
        {
            IAssetETDIdent ret = null;

            if (settings.IsWithIsinCode && settings.IsWithPriceCurrency)
            {
                ret = AllAsset.FirstOrDefault(a => (a.ISINCode == assetRequest.ISINCode) && (a.PriceCurrency == assetRequest.PriceCurrency));
            }
            else if (settings.IsWithIsinCode)
            {
                ret = AllAsset.FirstOrDefault(a => a.ISINCode == assetRequest.ISINCode);
            }
            else
            {
                List<IAssetETDIdent> assetList = new List<IAssetETDIdent>();
                bool isFound = false;
                switch (settings.ContractSymbolMode)
                {
                    case AssetETDRequestSymbolMode.ContractSymbol:
                        isFound = AssetByCtrSym.TryGetValue(assetRequest.ContractSymbol, out assetList);
                        break;
                    case AssetETDRequestSymbolMode.ElectronicContractSymbol:
                        isFound = AssetByElecCtrSym.TryGetValue(assetRequest.ElectronicContractSymbol, out assetList);
                        break;
                }

                if (isFound)
                {
                    Boolean isWithMaturityMonthYear = (settings.ContractMaturityMode == AssetETDRequestMaturityMode.MaturityMonthYear);
                    Boolean isWithMaturityDate = (settings.ContractMaturityMode == AssetETDRequestMaturityMode.MaturityDate);

                    ret = assetList.FirstOrDefault(a =>
                        ((false == settings.IsWithContractAttrib) || (a.ContractAttribute == assetRequest.ContractAttribute)) &&
                        ((false == settings.IsWithSettlementMethod) || (a.SettlementMethod == assetRequest.SettlementMethod)) &&
                        ((false == settings.IsWithContractMultiplier) || (a.ContractMultiplier == assetRequest.ContractMultiplier)) &&
                        ((false == isWithMaturityMonthYear) || (a.MaturityMonthYear == assetRequest.MaturityMonthYear)) &&
                        ((false == isWithMaturityDate) || ((a.MaturityDate.HasValue && assetRequest.MaturityDate.HasValue) && (a.MaturityDate == assetRequest.MaturityDate))) &&
                        (a.ContractType == assetRequest.ContractType) &&
                        ((a.ContractCategory == "F" && assetRequest.ContractCategory == "F") ||
                         (a.PutCall.HasValue == assetRequest.PutCall.HasValue && a.PutCall == assetRequest.PutCall &&
                          a.StrikePrice == (assetRequest.StrikePrice / (settings.IsWithStrikeDecNo && a.StrikeDecLocator.HasValue ? (int)System.Math.Pow(10, a.StrikeDecLocator.Value) : 1)) &&
                          ((false == settings.IsWithExerciseStyle) || (a.ExerciseStyle == assetRequest.ExerciseStyle))))
                          );
                }
            }
            return ret;
        }
        #endregion Methods
    }
    
}
