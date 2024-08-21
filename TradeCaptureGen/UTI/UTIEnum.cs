
using System;

namespace EFS.TradeInformation
{
    /// <summary>
    ///  type de UTI 
    /// </summary>
    public enum UTIType
    {
        /// <summary>
        /// UTI (Unique Trade Identifier)
        /// </summary>
        UTI,
        /// <summary>
        /// UTI (Unique Trade Identifier) for Positions 
        /// </summary>
        PUTI,
    }

    /// <summary>
    /// Type de code (avec présence ou non de l'émetteur)
    /// </summary>
    /// FI 20140206 [19564] add Enum
    public enum UTIBuildMode
    {
        /// <summary>
        /// Le code généré est le résultat de la concaténation d'un code issuer et d'un code trade
        /// </summary>
        withIssuer,
        /// <summary>
        /// Le code généré est le code trade
        /// </summary>
        withoutIssuer
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20240425 [26593] UTI/PUTI REFIT Add Class
    public class UTIRuleAttribute : Attribute
    {
        /// <summary>
        /// Is a REFIT algorithme
        /// </summary>
        public Boolean IsREFIT;
        /// <summary>
        /// Is a CCP algorithme
        /// </summary>
        public Boolean IsCPP;
        /// <summary>
        /// Is a Spheres algorithme
        /// </summary>
        public Boolean IsSpheres;
        /// <summary>
        /// Retourne true si règle "Level 2 Validation".
        /// <para>NB: "Level 2 Validation" interdit notamment l'usage du carcatère 'space' dans un UTI</para>
        /// </summary>
        public Boolean IsLevel2;
    }



    /// <summary>
    /// Règle de calcul de l'UTI
    /// </summary>
    // FI 20140206 [19564] add Enum
    // PL 20151029 add L2
    public enum UTIRule
    {
        /// <summary>
        /// Règle propre à Spheres
        /// </summary>
        [UTIRule(IsREFIT = false, IsCPP = false, IsSpheres = true)]
        SPHERES,
        /// <summary>
        /// Règle propre à Spheres (Level2)
        /// </summary>
        [UTIRule(IsREFIT = false, IsCPP = false, IsSpheres = true, IsLevel2 = true)]
        SPHERES_L2,
        /// <summary>
        /// Règle propre à Spheres (Refit)
        /// </summary>
        [UTIRule(IsREFIT = true)]
        SPHERES_REFIT,

        /// <summary>
        /// Règle propre à la chambre de compensation
        /// </summary>
        [UTIRule(IsREFIT = false, IsCPP = true)]
        CCP,
        /// <summary>
        /// Règle propre à la chambre de compensation (Level2)
        /// </summary>
        [UTIRule(IsREFIT = false, IsCPP = true, IsLevel2 = true)]
        CCP_L2,
        /// <summary>
        /// Règle propre à la chambre de compensation (Refit)
        /// </summary>
        [UTIRule(IsREFIT = true, IsCPP = true)]
        CCP_REFIT, //FI 20240425 [26593] UTI/PUTI REFIT
        
        /// <summary>
        /// Règle propre à la chambre de compensation CCeG
        /// </summary>
        CCEG,
        /// <summary>
        /// Règle propre à la chambre de compensation CCeG (Level2)
        /// </summary>
        [UTIRule(IsLevel2 = true)]
        CCEG_L2,
        /// <summary>
        /// Règle propre à la chambre de compensation CCeG (Refit)
        /// </summary>
        [UTIRule(IsREFIT =true)]
        CCEG_REFIT, //FI 20240425 [26593] UTI/PUTI REFIT
        
        /// <summary>
        /// Règle propre à la chambre de compensation EUREX
        /// </summary>
        EUREX,
        /// <summary>
        /// Règle propre à la chambre de compensation EUREX (Level2)
        /// </summary>
        [UTIRule(IsLevel2 = true)]
        EUREX_L2,
        /// <summary>
        /// Règle propre à la chambre de compensation EUREX (Level2 C7)
        /// </summary>
        [UTIRule(IsLevel2 = true)]
        EUREX_L2_C7_3, //PL 20160426 [22107] Newness EUREX C7 3.0 Release
        /// <summary>
        /// Règle propre à la chambre de compensation EUREX (Refit)
        /// </summary>
        [UTIRule(IsREFIT = true)]
        EUREX_REFIT, //FI 20240425 [26593] UTI/PUTI REFIT
        
        /// <summary>
        /// Règle propre à la chambre de compensation LCH.Clearnet-SA
        /// </summary>
        LCHCLEARNETSA,
        /// <summary>
        /// Règle propre à la chambre de compensation LCH.Clearnet-SA (Level2)
        /// </summary>
        [UTIRule(IsLevel2 = true)]
        LCHCLEARNETSA_L2,
        /// <summary>
        /// Règle propre à la chambre de compensation LCH.Clearnet-SA (Level2)
        /// </summary>
        [UTIRule(IsREFIT = true)]
        LCHCLEARNETSA_REFIT, //FI 20240425 [26593] UTI/PUTI REFIT

        /// <summary>
        /// Règle propre à la chambre de compensation EURONEXT CLEARING (Refit)
        /// </summary>
        /// FI 20240704 [WI987] Add
        [UTIRule(IsREFIT = true)]
        EURONEXTCLEARING_REFIT, 
        
        /// <summary>
        /// pas de calcul de le l'UTI/PUTI
        /// </summary>
        /// FI 20140623 [20125] Gestion de la valeur None
        NONE,
    }

    /// <summary>
    ///  Identification d'un émetteur de UTI
    /// </summary>
    /// FI 20140206 [19564] add Enum 
    public enum UTIIssuerIdent
    {
        /// <summary>
        /// Namespace connu
        /// </summary>
        ISSUER,
        /// <summary>
        /// Identification via un code MIC
        /// </summary>
        MIC,
        /// <summary>
        /// Identification via un l'identifiant non significatif d'un acteur dans Spheres
        /// </summary>
        IDA,
        /// <summary>
        /// Identification via un code BIC
        /// </summary>
        BIC,
        /// <summary>
        /// Identification via un code LEI
        /// </summary>
        LEI,
    }
}