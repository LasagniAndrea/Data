using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using EFS.ACommon;
using EfsML.Enum;
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.Common
{
    /// <summary>
    /// Représente les données majeures d'un DC ETD sous Spheres® permettant son identification depuis le monde Extérieur 
    /// </summary>
    public interface IDerivativeContractIdent
    {
        /// <summary>
        /// Contract Symbol
        /// </summary>
        string ContractSymbol { get; }
        
        /// <summary>
        /// 
        /// </summary>
        string ElectronicContractSymbol { get; }

        /// <summary>
        /// Contract Type (FLEX/STD)
        /// </summary>
        DerivativeContractTypeEnum ContractType { get; }

        /// <summary>
        /// Contract Category (F/O)
        /// </summary>
        string ContractCategory { get; }

        /// <summary>
        /// Contract Attribute (Version)
        /// </summary>
        string ContractAttribute { get; }

        /// <summary>
        /// Exercise Style 
        /// </summary>
        DerivativeExerciseStyleEnum? ExerciseStyle { get; }
        
        /// <summary>
        /// SettlementMethod
        /// </summary>
        SettlMethodEnum SettlementMethod { get; }

        /// <summary>
        /// Id interne du DC lorsque le DC est trouvé
        /// </summary>
        int IdDC { get; }

        /// <summary>
        /// Id interne du marché lorsque le DC est trouvé
        /// </summary>
        int IdM { get; }
    }

    /// <summary>
    /// Représente les données majeures d'un asset ETD sous Spheres® permettant son identification depuis le monde Extérieur
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    /// PM 20230622 [26091][WI390] Ajout de ContractMultiplier et ISINCode
    public interface IAssetETDIdent : IDerivativeContractIdent
    {
        /// <summary>
        /// MaturityMonthYear (ou Nom de l'échéance). Généralement au format  YYYYMM ou YYYYMMDD
        /// </summary>
        string MaturityMonthYear { get; }

        /// <summary>
        /// Date d'échéance (Expiry Date)
        /// </summary>
        Nullable<DateTime> MaturityDate { get; }

        /// <summary>
        /// 
        /// </summary>
        Nullable<PutOrCallEnum> PutCall { get; }

        /// <summary>
        /// 
        /// </summary>
        decimal StrikePrice { get; }

        /// <summary>
        /// 
        /// </summary>
        Nullable<int> StrikeDecLocator { get; }

        /// <summary>
        /// Contract Multiplier
        /// </summary>
        decimal ContractMultiplier { get; }

        /// <summary>
        /// Code ISIN
        /// </summary>
        string ISINCode { get; }

        /// <summary>
        /// Price Currency
        /// </summary>
        string PriceCurrency { get; }

        /// <summary>
        /// Id interne de l'asset lorsque l'asset est trouvé
        /// </summary>
        int IdAsset { get; }
    }

    /// <summary>
    /// Représente les données majeures d'un Derivative Contact sous Spheres® permettant son identification depuis le monde Extérieur
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class DerivativeContractIdent : IDerivativeContractIdent
    {
        /// <summary>
        /// Contract Symbol
        /// </summary>
        public string ContractSymbol { get; set; }

        /// <summary>
        /// Electronic Contract Symbol
        /// </summary>
        public string ElectronicContractSymbol { get; set; }

        /// <summary>
        /// Contract Type (FLEX/STD)
        /// </summary>
        public DerivativeContractTypeEnum ContractType { get; set; }

        /// <summary>
        /// Indique s'il s'agit d'un asset flexible
        /// </summary>
        public bool IsFlexible
        {
            get { return ContractType == DerivativeContractTypeEnum.FLEX; }
        }

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
        /// Exercise Style 
        /// </summary>
        public Nullable<DerivativeExerciseStyleEnum> ExerciseStyle { get; set; }

        /// <summary>
        /// Id interne du Derivative Contract
        /// </summary>
        public int IdDC { get; set; }

        /// <summary>
        /// IdM interne de l'asset lorsque l'asset est trouvé
        /// </summary>
        public int IdM { get; set; }
    }

    /// <summary>
    /// Représente les données majeures d'un asset ETD sous Spheres® permettant son identification depuis le monde Extérieur
    /// </summary>
    // FI 20220321 [XXXXX] Add
    // PM 20230622 [26091][WI390] Add ContractMultiplier & ISINCode
    // PM 20240122 [WI822] Add PriceCurrency
    public class AssetETDIdent : DerivativeContractIdent, IAssetETDIdent
    {
        /// <summary>
        /// MaturityMonthYear (ou Nom de l'échéance). Généralement au format  YYYYMM ou YYYYMMDD etc
        /// </summary>
        public string MaturityMonthYear { get; set; }

        /// <summary>
        /// Date d'échéance
        /// </summary>
        public Nullable<DateTime> MaturityDate { get; set; }

        /// <summary>
        /// Date d'échéance système
        /// </summary>
        public Nullable<PutOrCallEnum> PutCall { get; set; }

        /// <summary>
        /// Strike Price
        /// </summary>
        public decimal StrikePrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> StrikeDecLocator { get; set; }

        /// <summary>
        /// Contract Multiplier
        /// </summary>
        public decimal ContractMultiplier { get; set; }

        /// <summary>
        /// Code ISIN
        /// </summary>
        public string ISINCode { get; set; }

        /// <summary>
        /// Price Currency
        /// </summary>
        public string PriceCurrency { get; set; }

        /// <summary>
        /// Id interne de l'asset lorsque l'asset est trouvé
        /// </summary>
        public int IdAsset { get; set; }
    }
}
