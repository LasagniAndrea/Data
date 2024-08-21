using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common;
using EFS.Import;
using EFS.TradeInformation;
using EFS.GUI.CCI;
using EFS.ApplicationBlocks.Data;
using EFS.Status; 

using EfsML.Interface;
using EfsML.DynamicData;
 
using FpML.Interface;


namespace EFS.TradeInformation.Import
{
    /// <summary>
    ///  Représente les constantes utilisées lors de l'importation des trades
    /// </summary>
    /// FI 20160907 [21831]
    public sealed class TradeImportCst
    {
        //
        public const string identifier = "http://www.efs.org/otcml/tradeImport/identifier";
        public const string displayName = "http://www.efs.org/otcml/tradeImport/displayname";
        public const string description = "http://www.efs.org/otcml/tradeImport/description";
        public const string extlLink = "http://www.efs.org/otcml/tradeImport/extllink";
        //
        public const string instrumentIdentifier = "http://www.efs.org/otcml/tradeImport/instrumentIdentifier";
        public const string screen = "http://www.efs.org/otcml/tradeImport/screen";
        public const string templateIdentifier = "http://www.efs.org/otcml/tradeImport/templateIdentifier";
        //
        //PL 20130718 FeeCalculation Project
        //public const string isApplyFeeCalculation = "http://www.efs.org/otcml/tradeImport/isApplyFeeCalculation";
        public const string feeCalculation = "http://www.efs.org/otcml/tradeImport/feeCalculation";
        public const string isApplyPartyTemplate = "http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate";
        public const string isApplyClearingTemplate = "http://www.efs.org/otcml/tradeImport/isApplyClearingTemplate";

        /// <summary>
        /// Recherche du dealer à partir du clearer
        /// </summary>
        /// FI 20160929 [22507] Add isApplyReverseClearingTemplate
        public const string isApplyReverseClearingTemplate = "http://www.efs.org/otcml/tradeImport/isApplyReverseClearingTemplate";
        
        // FI 20160907 [21831] Add
        public const string updateMode = "http://www.efs.org/otcml/tradeImport/UpdateMode";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121031 [18213] permet éventuellement de débrayer les validationRules 
        public const string isApplyValidationRules = "http://www.efs.org/otcml/tradeImport/isApplyValidationRules";
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121031 [18213] permet éventuellement de débrayer la validation XSD 
        public const string isApplyValidationXSD = "http://www.efs.org/otcml/tradeImport/isApplyValidationXSD";
        /// <summary>
        /// 
        /// </summary>
        public const string isPostToEventsGen = "http://www.efs.org/otcml/tradeImport/isPostToEventsGen";
        /// <summary>
        /// 
        /// </summary>
        public const string isCopyNotePad = "http://www.efs.org/otcml/tradeImport/isCopyNotePad";
        /// <summary>
        /// 
        /// </summary>
        public const string isCopyAttachedDoc = "http://www.efs.org/otcml/tradeImport/isCopyAttachedDoc";
        //
        //public const string stBusiness = "http://www.efs.org/otcml/tradeImport/stBusiness";
        //public const string stEnvironment = "http://www.efs.org/otcml/tradeImport/stEnvironment";
        //public const string stPriority = "http://www.efs.org/otcml/tradeImport/stPriority";
        //public const string stActivation = "http://www.efs.org/otcml/tradeImport/stActivation";
        //public const string stUsedBy = "http://www.efs.org/otcml/tradeImport/stUsedBy";
    }


    /// <summary>
    /// Pilote l'importation d'un trade
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("tradeImport", IsNullable = false)]
    public class TradeImport
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settingsSpecified;
        /// <summary>
        /// Règlages de l'importation des trades
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("settings", IsNullable = false)]
        public ImportSettings settings;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeInputSpecified;

        //FI 20120226 tradeInput est de type TradeImportInput
        /// <summary>
        /// Caractéristiques du trade à importer
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tradeInput", typeof(TradeImportInput))]
        public TradeImportInput tradeInput;

        //[System.Xml.Serialization.XmlElementAttribute("tradeInput", typeof(TradeCommonInput))]
        //public TradeCommonInput tradeInput;
        //
        #endregion Members
        //
        #region constructor
        public TradeImport()
        {
        }
        #endregion
    }
    
}
