/*
 * SpanRisk.cs a été généré avec la commande suivante :
 * "C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\xsd" SpanRisk.xsd /c /e:spanFile /eld /nologo /n:EFS.SpheresIO.MarketData.Span /f
 * */

namespace EFS.SpheresIO.MarketData.Span
{
    using EFS.ACommon;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class d'outils de manipulation des objets issus d'un fichier Span Xml
    /// </summary>
    public static class SpanXmlFileTools
    {
        #region Methods
        /// <summary>
        /// Obtiens les données typés d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems"></param>
        /// <returns></returns>
        public static IEnumerable<T> ItemsToType<T>(object[] pItems)
        {
            return ((pItems != null) ? (pItems.Where(i => i.GetType() == typeof(T)).Cast<T>()) : (new List<T>()));
        }
        #endregion Methods
    }

    /// <summary>
    /// Extension pour les asset Non Option
    /// </summary>
    public partial class Contract
    {
        /// <summary>
        /// Indique s'il y a des trades actif sur l'asset
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInTrade = false;
    }

    /// <summary>
    /// Extension pour les Product Family
    /// </summary>
    public abstract partial class ProductFamily
    {
        /// <summary>
        /// Indique s'il y a des trades actif sur le DC
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInTrade = false;
    }


    /// <summary>
    /// Extension pour les échéances Option
    /// </summary>
    public partial class Series
    {
        /// <summary>
        /// Indique s'il y a des trades actif sur l'échéance Option
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInTrade = false;
    }

    /// <summary>
    /// Extension pour les asset Option
    /// </summary>
    public partial class Opt
    {
        /// <summary>
        /// Indique s'il y a des trades actif sur l'asset Option
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInTrade = false;
    }

    /// <summary>
    /// Extension pour les Exchange
    /// </summary>
    public partial class Exchange
    {
        #region Members
        private IEnumerable<CmbPf> m_CmbPf = null;
        private IEnumerable<OptionPF> m_OocPf = null;

        /// <summary>
        /// IDM du marché
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int Idm = 0;

        /// <summary>
        /// IDIMSPANEXCHANGE_H du marché dans les tables IMSPAN..._H
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdIMSPANEXCHANGE_H = 0;

        /// <summary>
        /// Indique s'il y a des trades actif sur le marché
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInTrade = false;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Combo Product Family
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<CmbPf> CmbPf
        {
            get
            {
                if (m_CmbPf == null)
                {
                    m_CmbPf = ProductFamily<CmbPf>();
                }
                return m_CmbPf;
            }
        }
        /// <summary>
        /// Option Combo Product Family
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<OptionPF> OocPf
        {
            get
            {
                if (m_OocPf == null)
                {
                    m_OocPf = ProductFamily<OptionPF>();
                }
                return m_OocPf;
            }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Obtiens les "Tiers" d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ProductFamily<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(Items);
        }
        #endregion Methods
    }

    /// <summary>
    /// Extension pour les Combined Commodity Definitions
    /// </summary>
    public partial class CcDef
    {
        #region Members
        private IEnumerable<Tier> m_InterTiers = null;
        private IEnumerable<Tier> m_IntraTiers = null;
        private IEnumerable<Tier> m_RateTiers = null;
        private IEnumerable<Tier> m_ScanTiers = null;
        private IEnumerable<Tier> m_SomTiers = null;
        //
        /// <summary>
        /// Indique si le Combined Commodity Definition doit être importé
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsToImport = false;
        /// <summary>
        /// IDIMSPANGRPCTR_H du Combined Commodity importé dans les tables IMSPAN..._H
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdIMSPANGRPCTR_H = 0;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Inter Tiers
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<Tier> InterTiers
        {
            get
            {
                if (m_InterTiers == null)
                {
                    IEnumerable<interTiers> tiers = ItemsTier<interTiers>();
                    m_InterTiers = ((tiers.Count() > 0) ? (tiers.FirstOrDefault().tier) : (new Tier[0]));
                }
                return m_InterTiers;
            }
        }
        /// <summary>
        /// Intra Tiers
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<Tier> IntraTiers
        {
            get
            {
                if (m_IntraTiers == null)
                {
                    IEnumerable<intraTiers> tiers = ItemsTier<intraTiers>();
                    m_IntraTiers = ((tiers.Count() > 0) ? (tiers.FirstOrDefault().tier) : (new Tier[0]));
                }
                return m_IntraTiers;
            }
        }
        /// <summary>
        /// Rate Tiers
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<Tier> RateTiers
        {
            get
            {
                if (m_RateTiers == null)
                {
                    IEnumerable<rateTiers> tiers = ItemsTier<rateTiers>();
                    m_RateTiers = ((tiers.Count() > 0) ? (tiers.FirstOrDefault().tier) : (new Tier[0]));
                }
                return m_RateTiers;
            }
        }
        /// <summary>
        /// Scan Tiers
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<Tier> ScanTiers
        {
            get
            {
                if (m_ScanTiers == null)
                {
                    IEnumerable<scanTiers> tiers = ItemsTier<scanTiers>();
                    m_ScanTiers = ((tiers.Count() > 0) ? (tiers.FirstOrDefault().tier) : (new Tier[0]));
                }
                return m_ScanTiers;
            }
        }
        /// <summary>
        /// Som Tiers
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<Tier> SomTiers
        {
            get
            {
                if (m_SomTiers == null)
                {
                    IEnumerable<somTiers> tiers = ItemsTier<somTiers>();
                    m_SomTiers = ((tiers.Count() > 0) ? (tiers.FirstOrDefault().tier) : (new Tier[0]));
                }
                return m_SomTiers;
            }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Obtiens les "Tiers" d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ItemsTier<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(Items);
        }
        #endregion Methods
    }

    /// <summary>
    /// Extension de la class de manipulation des Tiers (Ensemble d'échéances)
    /// </summary>
    public partial class Tier
    {
        #region Members
        private IEnumerable<rate> m_Rate = null;
        private IEnumerable<scanRate> m_ScanRate = null;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Rate
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<rate> Rate
        {
            get
            {
                if (m_Rate == null)
                {
                    m_Rate = ItemsRate<rate>();
                }
                return m_Rate;
            }
        }
        /// <summary>
        /// Scan Rate
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<scanRate> ScanRate
        {
            get
            {
                if (m_ScanRate == null)
                {
                    m_ScanRate = ItemsRate<scanRate>();
                }
                return m_ScanRate;
            }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Obtiens les "Rate" d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ItemsRate<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(Items);
        }
        #endregion Methods
    }

    /// <summary>
    /// Extension de la class de manipulation des Spreads
    /// </summary>
    public partial class DSpread
    {
        #region Members
        private IEnumerable<pmpsRate> m_PmpsRate = null;
        private IEnumerable<rate> m_Rate = null;
        private decimal? m_RateValue = null;
        //
        private IEnumerable<pLeg> m_PLeg = null;
        private IEnumerable<RpLeg> m_RpLeg = null;
        private IEnumerable<tLeg> m_TLeg = null;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Pmps Rate
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<pmpsRate> PmpsRate
        {
            get
            {
                if (m_PmpsRate == null)
                {
                    m_PmpsRate = ItemsRate<pmpsRate>();
                }
                return m_PmpsRate;
            }
        }

        /// <summary>
        /// Rate
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<rate> Rate
        {
            get
            {
                if (m_Rate == null)
                {
                    m_Rate = ItemsRate<rate>();
                }
                return m_Rate;
            }
        }

        /// <summary>
        /// Rate Value
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public decimal RateValue
        {
            get
            {
                if (m_RateValue.HasValue == false)
                {
                    // PM 20230613 [26408] Correction object reference exception
                    if (Rate.Count() > 0)
                    {
                        m_RateValue = (decimal)Rate.FirstOrDefault().val * 100;
                    }
                    else
                    {
                        m_RateValue = 0;
                    }
                }
                return m_RateValue.Value;
            }
        }

        /// <summary>
        /// PLeg
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<pLeg> PLeg
        {
            get
            {
                if (m_PLeg == null)
                {
                    m_PLeg = ItemsLeg<pLeg>();
                }
                return m_PLeg;
            }
        }

        /// <summary>
        /// RpLeg
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<RpLeg> RpLeg
        {
            get
            {
                if (m_RpLeg == null)
                {
                    m_RpLeg = ItemsLeg<RpLeg>();
                }
                return m_RpLeg;
            }
        }

        /// <summary>
        /// TLeg
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<tLeg> TLeg
        {
            get
            {
                if (m_TLeg == null)
                {
                    m_TLeg = ItemsLeg<tLeg>();
                }
                return m_TLeg;
            }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Obtiens les "Rate" d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ItemsRate<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(this.Items);
        }

        /// <summary>
        /// Obtiens les "Leg" d'un type donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ItemsLeg<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(this.Items1);
        }
        #endregion Methods
    }

    /// <summary>
    /// Extension de la class de manipulation des Spreads
    /// </summary>
    public partial class SSpread
    {
        #region Accessors
        public bool ApplyFxRisk
        {
            get { return BoolFunc.IsTrue(applyFxRisk); }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Extension pour les Clearing Organization
    /// </summary>
    public partial class ClearingOrg
    {
        #region Members
        private IEnumerable<DSpread> m_InterDSpreads = null;
        private IEnumerable<SSpread> m_InterSSpreads = null;
        private IEnumerable<DSpread> m_SuperDSpreads = null;
        private IEnumerable<SSpread> m_SuperSSpreads = null;

        /// <summary>
        /// IDIMSPAN_H de la chambre dans les tables IMSPAN..._H
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdIMSPAN_H = 0;
        #endregion Members

        #region Accessors
        /// <summary>
        /// InterDSpreads
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<DSpread> InterDSpreads
        {
            get
            {
                if (m_InterDSpreads == null)
                {
                    m_InterDSpreads = InterSpreads<DSpread>();
                }
                return m_InterDSpreads;
            }
        }
        /// <summary>
        /// InterSSpreads
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<SSpread> InterSSpreads
        {
            get
            {
                if (m_InterSSpreads == null)
                {
                    m_InterSSpreads = InterSpreads<SSpread>();
                }
                return m_InterSSpreads;
            }
        }
        /// <summary>
        /// SuperDSpreads
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<DSpread> SuperDSpreads
        {
            get
            {
                if (m_SuperDSpreads == null)
                {
                    m_SuperDSpreads = SuperSpreads<DSpread>();
                }
                return m_SuperDSpreads;
            }
        }
        /// <summary>
        /// SuperSSpreads
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IEnumerable<SSpread> SuperSSpreads
        {
            get
            {
                if (m_SuperSSpreads == null)
                {
                    m_SuperSSpreads = SuperSpreads<SSpread>();
                }
                return m_SuperSSpreads;
            }
        }
        #endregion Accessors

        #region Methods
        private IEnumerable<T> InterSpreads<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(interSpreads);
        }

        private IEnumerable<T> SuperSpreads<T>()
        {
            return SpanXmlFileTools.ItemsToType<T>(superSpreads);
        }

        #endregion Methods
    }

    /// <summary>
    /// Extension pour les Product Family Link
    /// </summary>
    public partial class PfLink : IEqualityComparer<PfLink>
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public PfLink() { }

        /// <summary>
        /// Construit un nouveau pfLink
        /// </summary>
        /// <param name="pExchAcr"></param>
        /// <param name="pContractType"></param>
        /// <param name="pPf"></param>
        public PfLink(string pExchAcr, string pContractType, ProductFamily pPf)
        {
            exch = pExchAcr;
            pfType = pContractType;
            if (pPf != default(ProductFamily))
            {
                pfId = pPf.pfId;
                pfCode = pPf.pfCode;
            }
        }
        #endregion Constructors

        #region methods IEqualityComparer
        /// <summary>
        /// Les pfLink sont égaux s'ils ont les mêmes exch, pfId, pfCode et pfType
        /// </summary>
        /// <param name="x">1er pfLink à comparer</param>
        /// <param name="y">2ème pfLink à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(PfLink x, PfLink y)
        {

            //Vérifier si les objets référencent les mêmes données
            if (ReferenceEquals(x, y)) return true;

            //Vérifier si un des objets est null
            if (x is null || y is null)
                return false;

            // Vérifier qu'il s'agit des mêmes pfLink
            return (x.exch == y.exch)
                && (x.pfId == y.pfId)
                && (x.pfCode == y.pfCode)
                && (x.pfType == y.pfType);
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets pfLink qui sont égaux.
        /// </summary>
        /// <param name="pCombinedCommodity">Le paramètre pfLink dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(PfLink pPfLink)
        {
            //Vérifier si l'obet est null
            if (pPfLink is null) return 0;

            //Obtenir le hash code de l'exch.
            int hashExch = pPfLink.exch == null ? 0 : pPfLink.exch.GetHashCode();

            //Obtenir le hash code du pfId.
            // EG 20160404 Migration vs2013
            //int hashPfId = pPfLink.pfId == null ? 0 : pPfLink.pfId.GetHashCode();
            int hashPfId = (pPfLink.pfId == 0 ? pPfLink.pfId : pPfLink.pfId.GetHashCode());

            //Obtenir le hash code du pfCode.
            int hashPfCode = pPfLink.pfCode == null ? 0 : pPfLink.pfCode.GetHashCode();

            //Obtenir le hash code du pfId.
            int hashPfType = pPfLink.pfType == null ? 0 : pPfLink.pfType.GetHashCode();
            
            //Calcul du hash code pour le CombinedCommodity.
            return (int)(hashExch ^ hashPfId ^ hashPfCode ^ hashPfType);
        }
        #endregion methods IEqualityComparer
    }

    /// <summary>
    /// Extension pour les jambes d'un spread InterContract
    /// </summary>
    public partial class RpLeg
    {
        #region Members
        private decimal? m_RateValue = null;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Rate Value
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public decimal RateValue
        {
            get
            {
                if (m_RateValue.HasValue == false)
                {
                    m_RateValue = (decimal)rate.FirstOrDefault().val * 100;
                }
                return m_RateValue.Value;
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Extension pour les jambes locales d'un spread InterClearing
    /// </summary>
    public partial class HomeLeg
    {
        #region Members
        private decimal? m_RateValue = null;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Rate Value
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public decimal RateValue
        {
            get
            {
                if (m_RateValue.HasValue == false)
                {
                    m_RateValue = (decimal)rate.FirstOrDefault().val * 100;
                }
                return m_RateValue.Value;
            }
        }
        #endregion Accessors
    }
}