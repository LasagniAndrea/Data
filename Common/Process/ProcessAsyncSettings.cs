using System;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFS.ACommon;

namespace EFS.Process
{
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION POUR GESTION ASYNCHRONE TRAITEMENT 
    // - FIN DE JOURNEE
    // - DEPOSIT
    // - CASH-BALANCE
    // - FACTURATION
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // EG 20190308 Add SafeKeepingCalculation|SafeKeepingWriting
    // EG 20190318 [24683] Upd ClosingReopeningCalculation|ClosingReopeningWriting
    // EG 20190613 [24683] Add ClosingReopeningWritingTrade
    // EG 20220324 [XXXXX] Add InvoicingWriting
    public enum ParallelProcess
    {
        AutomaticOption,
        CashFlows,
        CashBalance,
        FeesCalculation,
        FeesWriting,
        SafeKeepingCalculation,
        SafeKeepingWriting,
        LoadFlows,
        InitialMarginCalculation,
        InitialMarginWriting,
        UnderlyerDelivery,
        UTICalculation,
        ClosingReopeningCalculation,
        ClosingReopeningWriting,
        ClosingReopeningWritingTrade,
        InvoicingWriting,
    }

    // EG 20180413 [23769] Gestion customParallelConfigSource
    public sealed class ParallelTools
    {
        private static readonly string customParallelConfigSource = "\\parallelCustomSettings.config";
        /// <summary>
        /// Lecture du fichier de configuration des paramètres pour la gestion des process en mode asynchrone.
        /// 1. Lecture de la section (EndOfDay|MarginRequirement|CashBalance|Invoicing) dans le fichier Custom s'il existe
        /// 2. Si la section n'est pas trouvée ou que le fichier Custom n'est pas présent, lecture de la section dans le fichier app.Config
        /// 
        /// ATTENTION : 
        /// ---------
        /// Aucun merge n'est effectué entre les paramètres fournis par app.config et 
        /// ceux du fichier parallelCustomSettings.config
        /// </summary>
        /// <param name="pSectionName"></param>
        /// <returns></returns>
        // EG 20180412 [23769] ConfigSource for parallelism
        public static ConfigurationSection GetParallelSection(string pSectionName)
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection parallelSection = null; 

            // Get the custom configuration file.
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = Path.GetDirectoryName(config.FilePath) + customParallelConfigSource
                };
                Configuration customConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                ConfigurationSectionGroup parallelGroup = customConfig.SectionGroups["parallelSettings"];
                if (null != parallelGroup)
                    parallelSection = parallelGroup.Sections[pSectionName];
            }
            catch { }

            if (null == parallelSection)
                parallelSection = config.SectionGroups["parallelSettings"].Sections[pSectionName];


            return parallelSection;
        }
    }

    #region ParallelElementCollection
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public abstract class ParallelElementCollection : ConfigurationElementCollection
    {
        #region Members
        // Entité
        [ConfigurationProperty("entity", IsRequired = false)]
        public string Entity
        {
            get { return this["entity"].ToString(); }
            set { this["entity"] = value; }
        }
        // Exclude
        [ConfigurationProperty("exclude", IsRequired = false)]
        public bool Exclude
        {
            get { return (bool) this["exclude"]; }
            set { this["exclude"] = value; }
        }
        #endregion Members
        #region Accessors
        public virtual bool IsDefault
        {
            get { return false; }
        }
        #endregion Accessors
        #region Indexers
        public ParallelElement this[int pIndex]
        {
            get { return (ParallelElement)base.BaseGet(pIndex); }
            set
            {
                if (base.BaseGet(pIndex) != null)
                {
                    base.BaseRemoveAt(pIndex);
                }
                base.BaseAdd(pIndex, value);
            }
        }

        public ParallelElement this[ParallelProcess pName]
        {
            get { return (ParallelElement)BaseGet(pName); }
        }

        #endregion Indexers
        #region Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new ParallelElement();
        }

        protected override object GetElementKey(ConfigurationElement pElement)
        {
            return (pElement as ParallelElement).Name;
        }
        #endregion Methods
    }
    #endregion ParallelElementCollection
    #region ParallelElement
    /// <summary>
    /// Elements de configuation asynchrone d'une étape d'un traitement EOD / Deposit / Cash-Balance / Invoicing
    /// </summary>
    [ComVisible(false)]
    // EG 20181127 PERF Post RC (Step 3)
    public class ParallelElement : ConfigurationElement
    {
        #region Members
        // Phase du traitement
        [ConfigurationProperty("name", IsRequired = true)]
        public ParallelProcess Name { get { return (ParallelProcess)base["name"]; } }

        [ConfigurationProperty("heapSize", DefaultValue = 1)]
        public int HeapSize { get { return (int)base["heapSize"]; } }

        [ConfigurationProperty("maxThreshold", DefaultValue = 1)]
        public int MaxThreshold { get { return (int)base["maxThreshold"]; } }

        [ConfigurationProperty("maxThresholdEvents")]
        public Nullable<int> MaxThresholdEvents { get { return (Nullable<int>)base["maxThresholdEvents"]; } }

        [ConfigurationProperty("slaveCallEvents", DefaultValue = true)]
        public bool IsSlaveCallEvents { get { return (bool)base["slaveCallEvents"]; } }

        [ConfigurationProperty("heapSizeEvents")]
        public Nullable<int> HeapSizeEvents { get { return (Nullable<int>)base["heapSizeEvents"]; } }

        #endregion Members
    }
    #endregion ParallelElement


    //────────────────────────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION POUR GESTION ASYNCHRONE TRAITEMENT DE FIN DE JOURNEE
    //────────────────────────────────────────────────────────────────────────────────────────────────
    #region ParallelEndOfDaySection
    /// <summary>
    /// Section de configuration du traitement de fin de journée en mode ASYNCHRONE
    /// </summary>
    [ComVisible(false)]  
    public class ParallelEndOfDaySection : ConfigurationSection
    {
        #region Members
        [ConfigurationProperty("defaultSettings")]
        [ConfigurationCollection(typeof(EndOfDayMappingCollection))]
        public EndOfDayMappingCollection DefaultSettings
        {
            get { return ((EndOfDayMappingCollection)(base["defaultSettings"])); }
        }

        [ConfigurationProperty("settings")]
        [ConfigurationCollection(typeof(EndOfDayCollection), AddItemName = "endOfDay")]
        public EndOfDayCollection Settings
        {
            get { return ((EndOfDayCollection)(base["settings"])); }
        }
        #endregion Members
        #region Methods
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public ParallelElementCollection GetParallelSettings(string pEntity, string pCssCustodian, string pMarket)
        {
            ParallelElementCollection match = null;
            if (null != Settings)
                match = Settings.Match(pEntity, pCssCustodian, pMarket);
            if ((null != match) && (0 == match.Count))
                match = DefaultSettings;
            return match;
        }
        #endregion Methods
    }
    #endregion ParallelEndOfDaySection
    #region EndOfDayCollection
    [ComVisible(false)] 
    public class EndOfDayCollection : ConfigurationElementCollection
    {
        #region Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndOfDayMappingCollection();
        }

        protected override object GetElementKey(ConfigurationElement pElement)
        {
            EndOfDayMappingCollection element = pElement as EndOfDayMappingCollection;
            return String.Format("{0}|{1}|{2}", element.Entity, element.CssCustodian, element.Market);
        }
        #region Match
        public EndOfDayMappingCollection Match(string pEntity, string pCssCustodian, string pMarket)
        {
            EndOfDayMappingCollection match = null;

            IEnumerable<EndOfDayMappingCollection> mappings =
                (from EndOfDayMappingCollection element in this
                 where (String.IsNullOrEmpty(element.Entity) || (element.Entity == pEntity)) &&
                       (String.IsNullOrEmpty(element.CssCustodian) || (element.CssCustodian == pCssCustodian)) &&
                       (String.IsNullOrEmpty(element.Market) || (element.Market == pMarket))
                 select element);

            if (null != mappings)
            {
                int level = 0;
                while ((level < 8))
                {
                    switch (level)
                    {
                        case 0:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.CssCustodian) && StrFunc.IsFilled(eod.Market));
                            break;
                        case 1:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.CssCustodian));
                            break;
                        case 2:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.Market));
                            break;
                        case 3:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity));
                            break;
                        case 4:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.CssCustodian) && StrFunc.IsFilled(eod.Market));
                            break;
                        case 5:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.CssCustodian));
                            break;
                        case 6:
                            match = mappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Market));
                            break;
                        case 7:
                            match = mappings.FirstOrDefault();
                            break;
                    }

                    if (null != match)
                    {
                        #region Contrôle exclusion
                        if (MatchExclude(mappings, level))
                            match = null;
                        break;
                        #endregion Contrôle exclusion
                    }
                    level++;
                }
            }
            return match;
        }
        #endregion Match
        #region MatchExclude
        private bool MatchExclude(IEnumerable<EndOfDayMappingCollection> pMappings, int pLevel)
        {
            bool isOk = true;
            int level = pLevel;
            while (isOk && (level < 8))
            {
                switch (level)
                {
                    case 0:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.CssCustodian) && StrFunc.IsFilled(eod.Market) && eod.Exclude));
                        break;
                    case 1:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.CssCustodian) && eod.Exclude));
                        break;
                    case 2:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.Market) && eod.Exclude));
                        break;
                    case 3:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && eod.Exclude));
                        break;
                    case 4:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.CssCustodian) && StrFunc.IsFilled(eod.Market) && eod.Exclude));
                        break;
                    case 5:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.CssCustodian) && eod.Exclude));
                        break;
                    case 6:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Market) && eod.Exclude));
                        break;
                    case 7:
                        isOk = (null == pMappings.FirstOrDefault(eod => eod.Exclude));
                        break;
                }
                level++;
            }
            return (false == isOk);
        }
        #endregion MatchExclude
        #endregion Methods
    }
    #endregion EndOfDayCollection
    #region EndOfDayMappingCollection
    [ComVisible(false)] 
    public class EndOfDayMappingCollection : ParallelElementCollection
    {
        #region Members
        [ConfigurationProperty("cssCustodian", IsRequired = false)]
        public string CssCustodian
        {
            get { return this["cssCustodian"].ToString(); }
            set { this["cssCustodian"] = value; }
        }

        [ConfigurationProperty("market", IsRequired = false)]
        public string Market
        {
            get { return this["market"].ToString(); }
            set { this["market"] = value; }
        }
        #endregion Members
        #region Accessors
        public override bool IsDefault
        {
            get { return String.IsNullOrEmpty(Entity) && String.IsNullOrEmpty(CssCustodian) && String.IsNullOrEmpty(Market); }
        }
        #endregion Accessors
    }
    #endregion EndOfDayMappingCollection

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION POUR GESTION ASYNCHRONE TRAITEMENT DU CALCUL DU DEPOSIT
    //────────────────────────────────────────────────────────────────────────────────────────────────
    #region ParallelMarginRequirementSection
    /// <summary>
    /// Section de configuration du traitement de calcul des déposits en mode ASYNCHRONE
    /// </summary>
    [ComVisible(false)] 
    public class ParallelMarginRequirementSection : ConfigurationSection
    {
        #region Members
        [ConfigurationProperty("defaultSettings")]
        [ConfigurationCollection(typeof(MarginRequirementMappingCollection))]
        public MarginRequirementMappingCollection DefaultSettings
        {
            get { return ((MarginRequirementMappingCollection)(base["defaultSettings"])); }
        }

        [ConfigurationProperty("settings")]
        [ConfigurationCollection(typeof(MarginRequirementCollection), AddItemName = "marginRequirement")]
        public MarginRequirementCollection Settings
        {
            get { return ((MarginRequirementCollection)(base["settings"])); }
        }
        #endregion Members
        #region Methods
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public ParallelElementCollection GetParallelSettings(string pEntity, string pCssCustodian)
        {
            ParallelElementCollection match = null;
            if (null != Settings)
                match = Settings.Match(pEntity, pCssCustodian);
            if ((null != match) && (0 == match.Count))
                match = DefaultSettings;
            return match;
        }
        #endregion Methods
    }

    #endregion ParallelMarginRequirementSection
    #region MarginRequirementCollection
    [ComVisible(false)] 
    public class MarginRequirementCollection : ConfigurationElementCollection
    {
        #region Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new MarginRequirementMappingCollection();
        }

        protected override object GetElementKey(ConfigurationElement pElement)
        {
            MarginRequirementMappingCollection element = pElement as MarginRequirementMappingCollection;
            return String.Format("{0}|{1}", element.Entity, element.CssCustodian);
        }
        #region Match
        public MarginRequirementMappingCollection Match(string pEntity, string pCssCustodian)
        {
            MarginRequirementMappingCollection match = null;

            IEnumerable<MarginRequirementMappingCollection> mappings =
                (from MarginRequirementMappingCollection element in this
                 where (String.IsNullOrEmpty(element.Entity) || (element.Entity == pEntity)) &&
                       (String.IsNullOrEmpty(element.CssCustodian) || (element.CssCustodian == pCssCustodian))
                 select element);

            if (null != mappings)
            {
                int level = 0;
                while ((level < 4))
                {
                    switch (level)
                    {
                        case 0:
                            match = mappings.FirstOrDefault(mr => StrFunc.IsFilled(mr.Entity) && StrFunc.IsFilled(mr.CssCustodian));
                            break;
                        case 1:
                            match = mappings.FirstOrDefault(mr => StrFunc.IsFilled(mr.Entity));
                            break;
                        case 2:
                            match = mappings.FirstOrDefault(mr => StrFunc.IsFilled(mr.CssCustodian));
                            break;
                        case 3:
                            match = mappings.FirstOrDefault();
                            break;
                    }

                    if (null != match)
                    {
                        #region Contrôle exclusion
                        if (MatchExclude(mappings, level))
                            match = null;
                        break;
                        #endregion Contrôle exclusion
                    }
                    level++;
                }
            }
            return match;
        }
        #endregion Match
        #region MatchExclude
        private bool MatchExclude(IEnumerable<MarginRequirementMappingCollection> pMappings, int pLevel)
        {
            bool isOk = true;
            int level = pLevel;
            while (isOk && (level < 4))
            {
                switch (level)
                {
                    case 0:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && StrFunc.IsFilled(eod.CssCustodian) && eod.Exclude));
                        break;
                    case 1:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.Entity) && eod.Exclude));
                        break;
                    case 2:
                        isOk = (null == pMappings.FirstOrDefault(eod => StrFunc.IsFilled(eod.CssCustodian) && eod.Exclude));
                        break;
                    case 3:
                        isOk = (null == pMappings.FirstOrDefault(eod => eod.Exclude));
                        break;
                }
                level++;
            }
            return (false == isOk);
        }
        #endregion MatchExclude
        #endregion Methods
    }
    #endregion MarginRequirementCollection
    #region MarginRequirementMappingCollection
    [ComVisible(false)]
    public class MarginRequirementMappingCollection : ParallelElementCollection
    {
        #region Members
        [ConfigurationProperty("cssCustodian", IsRequired = false)]
        public string CssCustodian
        {
            get { return this["cssCustodian"].ToString(); }
            set { this["cssCustodian"] = value; }
        }
        #endregion Members
        #region Accessors
        public override bool IsDefault
        {
            get { return String.IsNullOrEmpty(Entity) && String.IsNullOrEmpty(CssCustodian); }
        }
        #endregion Accessors
    }
    #endregion MarginRequirementMappingCollection


    //────────────────────────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION POUR GESTION ASYNCHRONE TRAITEMENT DU CALCUL DES CASH-BALANCE
    //────────────────────────────────────────────────────────────────────────────────────────────────
    #region ParallelCashBalanceSection
    /// <summary>
    /// Section de configuration du traitement de calcul des Cash-Balance en mode ASYNCHRONE
    /// </summary>
    [ComVisible(false)]
    public class ParallelCashBalanceSection : ConfigurationSection
    {
        #region Members
        [ConfigurationProperty("defaultSettings")]
        [ConfigurationCollection(typeof(CashBalanceMappingCollection))]
        public CashBalanceMappingCollection DefaultSettings
        {
            get { return ((CashBalanceMappingCollection)(base["defaultSettings"])); }
        }

        [ConfigurationProperty("settings")]
        [ConfigurationCollection(typeof(CashBalanceCollection), AddItemName = "cashBalance")]
        public CashBalanceCollection Settings
        {
            get { return ((CashBalanceCollection)(base["settings"])); }
        }
        #endregion Members
        #region Methods
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public ParallelElementCollection GetParallelSettings(string pEntity)
        {
            ParallelElementCollection match = null;
            if (null != Settings)
                match = Settings.Match(pEntity);
            if ((null != match) && (0 == match.Count))
                match = DefaultSettings;
            return match;
        }
        #endregion Methods
    }
    #endregion ParallelCashBalanceSection
    #region CashBalanceCollection
    [ComVisible(false)]
    public class CashBalanceCollection : ConfigurationElementCollection
    {
        #region Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new CashBalanceMappingCollection();
        }

        protected override object GetElementKey(ConfigurationElement pElement)
        {
            return String.Format("{0}", (pElement as CashBalanceMappingCollection).Entity);
        }
        #region Match
        public CashBalanceMappingCollection Match(string pEntity)
        {
            CashBalanceMappingCollection match = null;

            IEnumerable<CashBalanceMappingCollection> mappings =
                (from CashBalanceMappingCollection element in this
                 where String.IsNullOrEmpty(element.Entity) || (element.Entity == pEntity)
                 select element).ToList();

            if (null != mappings)
            {
                // Contrôle Matching Entity
                match = mappings.FirstOrDefault(cb => StrFunc.IsFilled(cb.Entity));
                if ((null != match) && match.Exclude)
                    match = null;
                else if (null == match)
                    match = mappings.FirstOrDefault(cb => String.IsNullOrEmpty(cb.Entity));
            }
            return match;
        }
        #endregion Match
        #endregion Methods
    }
    #endregion CashBalanceCollection
    #region CashBalanceMappingCollection
    [ComVisible(false)]
    public class CashBalanceMappingCollection : ParallelElementCollection
    {
        #region Accessors
        public override bool IsDefault
        {
            get { return String.IsNullOrEmpty(Entity); }
        }
        #endregion Accessors
    }
    #endregion CashBalanceMappingCollection

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION POUR GESTION ASYNCHRONE TRAITEMENT DU FACTURATION
    //────────────────────────────────────────────────────────────────────────────────────────────────
    #region ParallelInvoicingSection
    /// <summary>
    /// Section de configuration du traitement de la facturation en mode ASYNCHRONE
    /// </summary>
    /// EG 20220324 [XXXXX] New
    [ComVisible(false)]
    public class ParallelInvoicingSection : ConfigurationSection
    {
        #region Members
        [ConfigurationProperty("defaultSettings")]
        [ConfigurationCollection(typeof(InvoicingMappingCollection))]
        public InvoicingMappingCollection DefaultSettings
        {
            get { return ((InvoicingMappingCollection)(base["defaultSettings"])); }
        }

        [ConfigurationProperty("settings")]
        [ConfigurationCollection(typeof(CashBalanceCollection), AddItemName = "invoicing")]
        public InvoicingCollection Settings
        {
            get { return ((InvoicingCollection)(base["settings"])); }
        }
        #endregion Members
        #region Methods
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public ParallelElementCollection GetParallelSettings(string pEntity)
        {
            ParallelElementCollection match = null;
            if (null != Settings)
                match = Settings.Match(pEntity);
            if ((null != match) && (0 == match.Count))
                match = DefaultSettings;
            return match;
        }
        #endregion Methods
    }
    #endregion ParallelCashBalanceSection
    #region CashBalanceCollection
    [ComVisible(false)]
    /// EG 20220324 [XXXXX] New
    public class InvoicingCollection : ConfigurationElementCollection
    {
        #region Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new InvoicingMappingCollection();
        }

        protected override object GetElementKey(ConfigurationElement pElement)
        {
            return String.Format("{0}", (pElement as InvoicingMappingCollection).Entity);
        }
        #region Match
        public InvoicingMappingCollection Match(string pEntity)
        {
            InvoicingMappingCollection match = null;

            IEnumerable<InvoicingMappingCollection> mappings =
                (from InvoicingMappingCollection element in this
                 where String.IsNullOrEmpty(element.Entity) || (element.Entity == pEntity)
                 select element).ToList();

            if (null != mappings)
            {
                // Contrôle Matching Entity
                match = mappings.FirstOrDefault(inv => StrFunc.IsFilled(inv.Entity));
                if ((null != match) && match.Exclude)
                    match = null;
                else if (null == match)
                    match = mappings.FirstOrDefault(inv => String.IsNullOrEmpty(inv.Entity));
            }
            return match;
        }
        #endregion Match
        #endregion Methods
    }
    #endregion InvoicingCollection
    #region InvoicingMappingCollection
    [ComVisible(false)]
    /// EG 20220324 [XXXXX] New
    public class InvoicingMappingCollection : ParallelElementCollection
    {
        #region Accessors
        public override bool IsDefault
        {
            get { return String.IsNullOrEmpty(Entity); }
        }
        #endregion Accessors
    }
    #endregion InvoicingMappingCollection

}
