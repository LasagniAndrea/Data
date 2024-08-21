using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFS.ACommon;

using FpML.Interface;
using EfsML.Enum;
using FixML.Enum;
using EFS.GUI.Interface;


namespace EfsML.Notification
{


    /// <summary>
    /// 
    /// </summary>
    /// FI 20160530 [21885] Modify
    /// FI 20160613 [22256] Modify
    public partial class HeaderFooter
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("hLFirstPg")]
        public string hLFirstPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hLFirstPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hLFirstPgCustom")]
        public string hLFirstPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("hCFirstPg")]
        public string hCFirstPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hCFirstPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hCFirstPgCustom")]
        public string hCFirstPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("hRFirstPg")]
        public string hRFirstPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hRFirstPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hRFirstPgCustom")]
        public string hRFirstPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("hLAllPg")]
        public string hLAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hLAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hLAllPgCustom")]
        public string hLAllPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("hCAllPg")]
        public string hCAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hCAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hCAllPgCustom")]
        public string hCAllPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("hRAllPg")]
        public string hRAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hRAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hRAllPgCustom")]
        public string hRAllPGCustom;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hColorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hColor")]
        public string hColor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hRuleSizeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hRuleSize")]
        public int hRuleSize;
        [System.Xml.Serialization.XmlElementAttribute("hRuleColor")]
        public string hRuleColor;
        [System.Xml.Serialization.XmlElementAttribute("hTitle")]
        public string hTitle;
        [System.Xml.Serialization.XmlElementAttribute("hTitlePosition")]
        public string hTitlePosition;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hBookIDLabelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hBookIdLabel")]
        public string hBookIDLabel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hBookOwnerIDLabelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hBookOwnerIdLabel")]
        public string hBookOwnerIDLabel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hDTBusinessLabelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hDtBusinessLabel")]
        public string hDTBusinessLabel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("hFormula")]
        public string hFormula;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean hCancelMsgSpecified;
        /// <summary>
        /// Customization du message "Annule et remplace"
        /// </summary>
        /// FI 20160624 [22286] add 
        [System.Xml.Serialization.XmlElementAttribute("hCancelMsg")]
        public MsgStyle hCancelMsg;
        [System.Xml.Serialization.XmlElementAttribute("fLLastPg")]
        public string fLLastPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fLLastPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fLLastPgCustom")]
        public string fLLastPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("fCLastPg")]
        public string fCLastPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fCLastPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fCLastPgCustom")]
        public string fCLastPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("fRLastPg")]
        public string fRLastPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fRLastPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fRLastPgCustom")]
        public string fRLastPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("fLAllPg")]
        public string fLAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fLAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fLAllPgCustom")]
        public string fLAllPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("fCAllPg")]
        public string fCAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fCAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fCAllPgCustom")]
        public string fCAllPGCustom;
        [System.Xml.Serialization.XmlElementAttribute("fRAllPg")]
        public string fRAllPG;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fRAllPGCustomSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fRAllPgCustom")]
        public string fRAllPGCustom;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fRuleSizeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fRuleSize")]
        public int fRuleSize;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fRuleColorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fRuleColor")]
        public string fRuleColor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fFormula")]
        public string fFormula;
        [System.Xml.Serialization.XmlElementAttribute("fLegend")]
        public string fLegend;
        [System.Xml.Serialization.XmlElementAttribute("sort")]
        public string sort;
        [System.Xml.Serialization.XmlElementAttribute("summaryStrategies")]
        public string summaryStrategies;
        [System.Xml.Serialization.XmlElementAttribute("summaryCashFlows")]
        public string summaryCashFlows;
        [System.Xml.Serialization.XmlElementAttribute("summaryFees")]
        public string summaryFees;
        /// <summary>
        /// voir enum cst.UTISummary
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("summaryUTI")]
        public string summaryUTI;

        /// <summary>
        /// 
        /// </summary>
        // RD 20151102 [21309] tradeNumberIdent is mandatory
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public Boolean tradeNumberIdentSpecified;

        /// <summary>
        /// Transaction Identifier used in report (TradeId, ExecId...)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tradeNumberIdent")]
        public Cst.TradeIdentificationEnum tradeNumberIdent;

        /// <summary>
        /// Affichage de l'horodatage
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("timestampType")]
        public Cst.TimestampType timestampStyle;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean amountSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141209 [XXXXX] Add 
        [System.Xml.Serialization.XmlElementAttribute("amount")]
        public AmountStyle amount;


        /// <summary>
        /// Affichage des totaux en contrevaleur dans la section positions ouvertes
        /// </summary>
        /// FI 20141209 [XXXXX] Add 
        [System.Xml.Serialization.XmlElementAttribute("totalInBaseCurrency")]
        public YesNoEnum totalInBaseCurrency;


        /// <summary>
        ///  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean noActivityMsgSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141209 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("noActivityMsg")]
        public StyleAttributes noActivityMsg;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sectionBannerSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141209 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("sectionBanner")]
        public SectionBannerStyle sectionBanner;



        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sectionTitleSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150128 [20742] Add
        [System.Xml.Serialization.XmlElementAttribute("sectionTitle")]
        public SectionTitles sectionTitle;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean assetBannerSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141209 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("assetBanner")]
        public AssetBannerStyle assetBanner;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean expiryIndicatorSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20141209 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("expiryIndicator")]
        public ExpiryIndicator expiryIndicator;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean journalEntrySpecified;

        /// <summary>
        /// Description des formules retenues pour l'alimentation des labels dans la section journalEntry
        /// </summary>
        /// FI 20150327 [20275] Add
        [System.Xml.Serialization.XmlArray(ElementName = "journalEntries")]
        [System.Xml.Serialization.XmlArrayItemAttribute("journalEntry")]
        public List<LabelDescription> journalEntry;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean collateralSpecified;

        /// <summary>
        /// Description de la formule retenue pour l'alimentation des labels dans la section collateral
        /// </summary>
        /// FI 20160530 [21885] Add 
        [System.Xml.Serialization.XmlElementAttribute("collateral")]
        public LabelDescription collateral;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean underlyingStockSpecified;

        /// <summary>
        /// Description de la formule retenue pour l'alimentation des labels dans la section underlyingStocks (positions Stocks utilisées pou réduire les positions ETD Short Call et Short Option)
        /// </summary>
        /// FI 20160613 [22256] add
        [System.Xml.Serialization.XmlElementAttribute("underlyingStock")]
        public LabelDescription underlyingStock;

        #endregion Members

    }

    /// <summary>
    /// Style des données de type Montant
    /// </summary>
    public partial class AmountStyle
    {
        /// <summary>
        /// voir Cst.AmountFormat
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "format")]
        public string format;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool positiveValueSpecified;
        /// <summary>
        /// Définition du style associé aux données de type montant lorsque les valeurs sont positives
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(ElementName = "positiveValue")]
        public StyleAttributes positiveValue;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool negativeValueSpecified;
        /// <summary>
        /// Définition du style associé aux données de type montant lorsque les valeurs sont négatives
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(ElementName = "negativeValue")]
        public StyleAttributes negativeValue;

        /// <summary>
        /// 
        /// </summary>
        public AmountStyle()
        {
            positiveValue = new StyleAttributes();
            negativeValue = new StyleAttributes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // RD 20150917 [21376] Modify
        public Boolean IsFilled()
        {
            // Au moins un élément renseigné
            //return StrFunc.IsFilled(format) && negativeValue.IsFilled() || positiveValue.IsFilled();
            return StrFunc.IsFilled(format) || negativeValue.IsFilled() || positiveValue.IsFilled();
        }

    }

    /// <summary>
    /// align, fontsize, foreColor, backColorn,
    /// </summary>
    /// FI 20141209 [XXXXX] Add
    public partial class StyleAttributes
    {
        /// <summary>
        ///  Left, Center, Right
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("align")]
        public string align;

        /// <summary>
        /// Taille de police
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fontsize")]
        public string fontsize;

        /// <summary>
        /// Couleur de 1er plan 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("foreColor")]
        public string foreColor;

        /// <summary>
        /// Couleur d'arrière-plan
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("backColor")]
        public string backColor;

        /// <summary>
        /// Retourne true si un des attributs de style est renseigné
        /// </summary>
        /// <returns></returns>
        public Boolean IsFilled()
        {
            return StrFunc.IsFilled(align) || StrFunc.IsFilled(fontsize) || StrFunc.IsFilled(foreColor) || StrFunc.IsFilled(foreColor);
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150128 [20742] Add
    public partial class SectionTitles
    {
        /// <summary>
        /// Titres des saction
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("section")]
        public List<SectionTitle> section;

        public SectionTitles()
        {
            section = new List<SectionTitle>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150128 [20742] Add
    public partial class SectionTitle
    {
        [System.Xml.Serialization.XmlAttributeAttribute("key")]
        public string key;

        [System.Xml.Serialization.XmlText()]
        public string title;

    }

    /// <summary>
    /// Définition d'un label dynamique 
    /// </summary>
    /// FI 20150128 [20742] Add
    public partial class LabelDescription
    {
        /// <summary>
        ///  "FDA" ou "***" 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("key")]
        public string key;

        /// <summary>
        /// La description est constituée de tag qui seront interprétés par Spheres
        /// <para>Exemple de description si FDA:{ASSET}/{QTY} at {RATEVALUE}% ({DAYCOUNT})</para>
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string description;
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20141209 [XXXXX] Add
    public partial class SectionBannerStyle : StyleAttributes
    {
        /// <summary>
        /// None, Banner, Tab
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("style")]
        public string style;

        /// <summary>
        /// Retourne true si un des attributs de style est renseigné
        /// </summary>
        public new Boolean IsFilled()
        {
            return StrFunc.IsFilled(style) || base.IsFilled();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20141209 [XXXXX] Add
    public partial class AssetBannerStyle : StyleAttributes
    {
        /// <summary>
        /// Contract_Model1, Contract_Model2, Contract_Model3
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("style")]
        public string style;

        /// <summary>
        /// Retourne true si un des attributs de style est renseigné
        /// </summary>
        public new Boolean IsFilled()
        {
            return StrFunc.IsFilled(style) || base.IsFilled();
        }

    }

    /// <summary>
    ///  Indicateur d'échéance 
    /// </summary>
    /// FI 20141209 [XXXXX] Add
    public partial class ExpiryIndicator : StyleAttributes
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean daysSpecified;

        /// <summary>
        /// Nbre de jour avant échéance, destiné à être utilisé pour afficher un warning sur les trades en positions dont l’échéance est à moins de ce N jours
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("days")]
        public int days;

        /// <summary>
        /// Retourne true si un des attributs de style est renseigné
        /// </summary>
        public new Boolean IsFilled()
        {
            return daysSpecified || base.IsFilled();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20160624 [22286] add 
    public partial class MsgStyle : StyleAttributes
    {
        /// <summary>
        /// Message 
        /// <para></para>
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string msg;

        /// <summary>
        /// Retourne true si un des attributs de style est renseigné ou si un message est renseigné
        /// </summary>
        public new Boolean IsFilled()
        {
            return StrFunc.IsFilled(msg) || base.IsFilled();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ReportSettings
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportCurrencySpecified;
        /// <summary>
        /// Représente la devise du message (Code ISO4217_ALPHA3)
        /// <para>Les contrevaleurs seront en cette devise</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingCurrency", Order = 1)]
        public string reportCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportFeeDisplaySpecified;
        /// <summary>
        /// Pilote la présentation des frais
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingFeeDisplay", Order = 2)]
        public string reportFeeDisplay;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool headerFooterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("headerFooter", Order = 3)]
        public HeaderFooter headerFooter;
        #endregion Members
    }
}
