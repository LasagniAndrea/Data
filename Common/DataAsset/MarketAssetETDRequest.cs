using System;
//
using EfsML.Enum;
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.Common
{
    /// <summary>
    /// Critère de recherche d'un asset ETD
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public enum AssetETDRequestSymbolMode
    {
        /// <summary>
        /// Recherche via le symbol
        /// </summary>
        ContractSymbol,
        /// <summary>
        /// Recherche via le Electronic symbol
        /// </summary>
        ElectronicContractSymbol,
    }

    /// <summary>
    /// Mode de recherche de l'échéance
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public enum AssetETDRequestMaturityMode
    {
        /// <summary>
        /// Recherche via la date d'échéance
        /// </summary>
        MaturityDate,
        /// <summary>
        /// Recherche via le nom d'échéance (généralement YYYYMM ou YYYYMMDD)
        /// </summary>
        MaturityMonthYear
    }

    /// <summary>
    /// Pilotage de la recherche d'un asset dans un <seealso cref="MarketDataAssetETD"/>
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    /// PM 20230622 [26091][WI390] Ajout de IsWithContractMultiplier et IsWithIsinCode
    /// PM 20240122 [WI822] Add IsWithPriceCurrency
    public struct MarketAssetETDRequestSettings
    {
        /// <summary>
        /// Recherche sur le symbol ou sur le symbol electronique
        /// </summary>
        public AssetETDRequestSymbolMode ContractSymbolMode;
        /// <summary>
        /// Recherche sur le nom de l'échéance ou sur la date d'échéance
        /// </summary>
        public AssetETDRequestMaturityMode ContractMaturityMode;
        /// <summary>
        /// Usage de ContractAttrib oui/non
        /// </summary>
        public bool IsWithContractAttrib;
        /// <summary>
        /// Usage de  settlement Method oui/non
        /// </summary>
        public bool IsWithSettlementMethod;
        /// <summary>
        /// Usage de l'exercice style oui/non
        /// </summary>
        public bool IsWithExerciseStyle;
        /// <summary>
        /// Indicateur d'utilisation du nombre de décimal du strike sur le DC oui/non
        /// </summary>
        public bool IsWithStrikeDecNo;
        /// <summary>
        /// Indicateur d'utilisation du contract multiplier oui/non
        /// </summary>
        public bool IsWithContractMultiplier;
        /// <summary>
        /// Indicateur de recherche via l'Isin Code
        /// </summary>
        public bool IsWithIsinCode;
        /// <summary>
        /// Indicateur de recherche via la devise
        /// </summary>
        public bool IsWithPriceCurrency;
    }

    /// <summary>
    /// Restrictions appliquées lors de la recherche dans un <seealso cref="MarketDataAssetETD"/> 
    /// <para>Les champs sont renseignés en fonction de <seealso cref="MarketAssetETDRequestSettings"/></para>
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class MarketAssetETDRequest
    {
        /// <summary>
        /// Contract Symbol
        /// </summary>
        public string ContractSymbol { get; set; }
        /// <summary>
        /// Electronic Contract Symbol
        /// </summary>
        /// PM 20190222 [24326] Ajout ElectronicContractSymbol
        public string ElectronicContractSymbol { get; set; }
        /// <summary>
        /// Code ISIN
        /// </summary>
        /// PM 20230622 [26091][WI390] Ajout ISINCode
        public string ISINCode { get; set; }
        /// <summary>
        /// Contract Type (FLEX/STD)
        /// </summary>
        public DerivativeContractTypeEnum ContractType { get; set; }
        /// <summary>
        /// Contract Attribute (Version)
        /// </summary>
        public string ContractAttribute { get; set; }
        /// <summary>
        /// Contract Category (F/O)
        /// </summary>
        public string ContractCategory { get; set; }
        /// <summary>
        /// Settlement Method
        /// </summary>
        public SettlMethodEnum SettlementMethod { get; set; }
        /// <summary>
        /// MaturityMonthYear ou nom de l'échéance (géréralement YYYYMM ou YYYYMMDD)
        /// </summary>
        public string MaturityMonthYear { get; set; }
        /// <summary>
        /// Maturity Date
        /// </summary>
        public Nullable<DateTime> MaturityDate { get; set; }
        /// <summary>
        /// Put Call (ou null)
        /// </summary>
        public Nullable<PutOrCallEnum> PutCall { get; set; }
        /// <summary>
        /// Strike Price
        /// </summary>
        public decimal StrikePrice { get; set; }
        /// <summary>
        /// Exercise Style (ou null)
        /// </summary>
        public Nullable<DerivativeExerciseStyleEnum> ExerciseStyle { get; set; }
        /// <summary>
        /// Frequency Maturity (Fréquence des échéances)
        /// </summary>
        /// PM 20220701 [XXXXX] Ajout FrequencyMaturity
        public string FrequencyMaturity { get; set; }
        /// RD 20230403 [26332] Ajout FrequencyMaturityEnum
        public Nullable<ContractFrequencyEnum> FrequencyMaturityEnum { get; set; }
        /// <summary>
        /// Contract Multiplier
        /// </summary>
        /// PM 20230622 [26091][WI390] Ajout ContractMultiplier
        public decimal ContractMultiplier { get; set; }
        /// <summary>
        /// Price Currency
        /// </summary>
        // PM 20240122 [WI822] Add PriceCurrency
        public string PriceCurrency { get; set; }
    }
}
