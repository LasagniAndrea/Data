using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;    
using EfsML.v30.CashBalance;
using EfsML.v30.AssetDef;
using EfsML.Notification; 
using FpML.v44.Shared;
using FixML.Enum;

//FI 20130701 [18745]
//Ce fichier contient toutes les classes nécessaires à la serialization d'un CashBalance dans le module de messagerie
//Le flux obtnus est plus légé, plus lisible de manière à facilité l'exploitation XSL    
//Chaque évolution dans le CashBalance qui devra être inclue dans un report devra faire l'objet d'une évolution dans les classes suivantes

namespace EfsML.v30.Notification
{

    /// <summary>
    /// Représent un cashBalance
    /// <para>Cette classe est serializée dans le cadre de la messagerie de manière à restituer un flux plus simple et plus léger que le CashBalance présent dans le trade</para>
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("cashBalanceReport", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CashBalanceReport : Product
    {

        /// <summary>
        /// Représente le Book
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("Acct", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string Acct;

        [System.Xml.Serialization.XmlAttributeAttribute("BizDt", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string BizDt;

        [System.Xml.Serialization.XmlElementAttribute("cashBalanceOfficePartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        public PartyReference cashBalanceOfficePartyReference;

        /// <summary>
        /// Représente l'entité
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("entityPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        public PartyReference entityPartyReference;

        /// <summary>
        /// Intraday ou EndOfDay 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("timing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        public SettlSessIDEnum timing;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceStream", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        public CashBalanceStreamReport[] cashBalanceStream;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeCashBalanceStreamSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("exchangeCashBalanceStream", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        public ExchangeCashBalanceStreamReport exchangeCashBalanceStream;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxRateSpecified;

        /// <summary>
        /// Represente un array de taux de change 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fxRate", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        public IdentifiedFxRate[] fxRate;

        /// <summary>
        /// Represente les paramétrages en vigueur sur l'acteur CashBalanceOffice 
        /// <para>Pratique pour l'audit</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("settings", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        public CashBalanceSettings settings;


        /// <summary>
        /// 
        /// </summary>
        /// FI 20161027 [22151] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool endOfDayStatusSpecified;

        /// <summary>
        ///  Etats des process EndOfDay 
        /// </summary>
        /// FI 20161027 [22151] Add
        [System.Xml.Serialization.XmlElementAttribute("endOfDayStatus", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 8)]
        public EndOfDayStatus endOfDayStatus;

        #region Constructor
        public CashBalanceReport()
        {
            this.cashBalanceOfficePartyReference = new PartyReference();
            this.entityPartyReference = new PartyReference();
            this.timing = SettlSessIDEnum.EndOfDay;
            this.cashBalanceStream = new CashBalanceStreamReport[] { new CashBalanceStreamReport() };
            this.exchangeCashBalanceStream = new ExchangeCashBalanceStreamReport();
            this.settings = new CashBalanceSettings();
            this.fxRate = new IdentifiedFxRate[] { new IdentifiedFxRate() };
            this.endOfDayStatus = new EndOfDayStatus();  
        }
        #endregion

    }

    /// <summary>
    /// Represente un stream en devise d'origine
    /// </summary>
    /// FI 20140910 [20275] Modify 
    /// FI 20150320 [XXPOC] Modify
    /// FI 20150323 [XXPOC] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalanceStreamReport
    {
        /// <summary>
        /// Représente la devise du stream
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("ccy")]
        public string currency;

        /// <summary>
        /// Représente le risque dans la devise du stream (somme des risques toutes chambres confondues)
        /// <para>La contrevaleur du risque doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 2)]
        public CssExchangeCashPositionReport marginRequirement;

        /// <summary>
        /// Représente le cash disponible 
        /// <para>La contrevaleur du cash doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 3)]
        public CashAvailableReport cashAvailable;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashUsedSpecified;

        /// <summary>
        /// Représente le cash utilisé pour couvrir le déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé en  devise comptable</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 4)]
        public CashPositionReport cashUsed;

        /// <summary>
        /// Représente le collateral utilisé pour couvrir le déposit
        /// <para>La contrevaleur du collateral doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 5)]
        public ExchangeCashPositionReport collateralAvailable;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralUsedSpecified;

        /// <summary>
        /// Représente le collateral utilisé pour couvrir le déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé en  devise comptable</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 6)]
        public CashPositionReport collateralUsed;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool uncoveredMarginRequirementSpecified;

        /// <summary>
        /// Représente le défaut de garantie (garanties utilisées - espèces utilisées)
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé dans la devise d'origine</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 7)]
        public CashPositionReport uncoveredMarginRequirement;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginCallSpecified;

        /// <summary>
        /// Représente l'appel/restitution de déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé dans la devise d'origine</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("marginCall", Order = 8)]
        public SimplePaymentReport marginCall;

        /// <summary>
        /// Représente le nouveau solde 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", Order = 9)]
        public CashPositionReport cashBalance;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousMarginConstituentSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("previousMarginConstituent", Order = 10)]
        public PreviousMarginConstituentReport previousMarginConstituent;

        /// <summary>
        /// ??????????
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool realizedMarginSpecified;

        /// <summary>
        /// ??????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", Order = 11)]
        public MarginConstituentReport realizedMargin;

        /// <summary>
        /// ??????????
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unrealizedMarginSpecified;

        /// <summary>
        /// ??????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", Order = 12)]
        public MarginConstituentReport unrealizedMargin;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool liquidatingValueSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] liquidatingValue est de type OptionLiquidatingValueReport
        [System.Xml.Serialization.XmlElementAttribute("liquidatingValue", Order = 13)]
        public OptionLiquidatingValueReport liquidatingValue;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketValueSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150623 [21149] Add
        [System.Xml.Serialization.XmlElementAttribute("marketValue", Order = 14)]
        public DetailedCashPositionReport marketValue;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fundingSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add funding
        [System.Xml.Serialization.XmlElementAttribute("funding", Order = 15)]
        public DetailedCashPositionReport funding;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool borrowingSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150323 [XXPOC] add borrowing
        [System.Xml.Serialization.XmlElementAttribute("borrowing", Order = 16)]
        public DetailedCashPositionReport borrowing;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unsettledCashSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150320 [XXPOC] add unsettledCash
        [System.Xml.Serialization.XmlElementAttribute("unsettledCash", Order = 17)]
        public DetailedCashPositionReport unsettledCash;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardCashPaymentSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add forwardCashPayment
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", Order = 18)]
        public CashBalancePaymentReport forwardCashPayment;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add equityBalance
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", Order = 19)]
        public CashPositionReport equityBalance;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceWithForwardCashSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add equityBalanceWithForwardCash
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", Order = 20)]
        public CashPositionReport equityBalanceWithForwardCash;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalAccountValueSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150623 [21149] Add
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", Order = 21)]
        public CashPositionReport totalAccountValue;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add excessDeficit
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", Order = 22)]
        public CashPositionReport excessDeficit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitWithForwardCashSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140910 [20275] add excessDeficitWithForwardCash
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", Order = 23)]
        public CashPositionReport excessDeficitWithForwardCash;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CashBalanceStreamReport()
        {
            marginRequirement = new CssExchangeCashPositionReport();
            cashAvailable = new CashAvailableReport();
            cashUsed = new CashPositionReport();
            collateralAvailable = new ExchangeCashPositionReport();
            collateralUsed = new CashPositionReport();
            uncoveredMarginRequirement = new CashPositionReport();
            marginCall = new SimplePaymentReport();
            cashBalance = new CashPositionReport();
            previousMarginConstituent = new PreviousMarginConstituentReport();
            realizedMargin = new MarginConstituentReport();
            unrealizedMargin = new MarginConstituentReport();
            liquidatingValue = new OptionLiquidatingValueReport();
            funding = new DetailedCashPositionReport();
            borrowing = new DetailedCashPositionReport();
            unsettledCash = new DetailedCashPositionReport();
            forwardCashPayment = new CashBalancePaymentReport();
            equityBalance = new CashPositionReport();
            equityBalanceWithForwardCash = new CashPositionReport();
            excessDeficit = new CashPositionReport();
            excessDeficitWithForwardCash = new CashPositionReport();
        }
        #endregion constructor
    }

    /// <summary>
    /// Représente un solde (un solde de trésorie), un montant
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashPositionReport : ReportAmountSide
    {
        #region constructor
        public CashPositionReport() : base() { }
        #endregion constructor
    }

    /// <summary>
    /// Représente un solde (un solde de trésorie), un montant
    /// <para>Ce solde peut être exprimé en devise de contrevaleur</para>
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeCashPositionReport : CashPositionReport
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeAmountSpecified;
        /// <summary>
        ///  Représente le solde en devise de contrevaleur
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("exchangeAmount", Order = 1)]
        public ReportAmountSide exchangeAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeFxRateReferenceSpecified;

        /// <summary>
        /// Taux de change 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("exchangeFxRateReference", Order = 2)]
        public FxRateReference[] exchangeFxRateReference;

        #region constructor
        public ExchangeCashPositionReport()
            : base()
        {
            this.exchangeAmount = new ReportAmountSide();
            this.exchangeFxRateReferenceSpecified = false;
            this.exchangeFxRateReference = new FxRateReference[] { };
        }
        #endregion
    }

    /// <summary>
    /// ExchangeCashPosition avec détail par Chambre de compensation
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssExchangeCashPositionReport : ExchangeCashPositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// détail des soldes par chambre de compensation (le détail est en devise du solde)
        /// <para>si la chambre n'est pas renseigné, le détail représente un montant "all custodian"</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public CssAmountReport[] detail;

        #region constructor
        public CssExchangeCashPositionReport()
            : base()
        {
            detail = new CssAmountReport[] { new CssAmountReport() };
            detailSpecified = false;
        }
        #endregion Method

    }

    /// <summary>
    ///  Montant par chambre de compensation 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssAmountReport : ReportAmountSide
    {

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150402 [POC] cssHref devient optionel
        [XmlIgnoreAttribute()]
        public bool cssHrefSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string cssHref;

        #region Constructor
        public CssAmountReport()
            : base()
        {
        }
        #endregion
    }

    /// <summary>
    /// Représente le cash disponible
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashAvailableReport : ExchangeCashPositionReport
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool constituentSpecified;

        /// <summary>
        /// Représente les éléments qui constituent le cash disponible
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("constituent", Order = 1)]
        public CashAvailableConstituentReport constituent;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CashAvailableReport()
            : base()
        {
            constituent = new CashAvailableConstituentReport();
        }
        #endregion
    }

    /// <summary>
    /// Représente les éléments d'un cash disponible pour le cash balance
    /// </summary>
    /// FI 20140909 [20275] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CashAvailableConstituentReport
    {
        /// <summary>
        /// Représente les versements et retraits en espèce
        /// </summary>
        /// FI 20140909 [20275] est de type CashBalancePaymentReport
        [System.Xml.Serialization.XmlElementAttribute("cashBalancePayment", Order = 1)]
        public CashBalancePaymentReport cashBalancePayment;

        /// <summary>
        /// Représente le solde précédent
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("previousCashBalance", Order = 2)]
        public CashPositionReport previousCashBalance;

        /// <summary>
        /// Représente les cash Flows 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashFlows", Order = 3)]
        public CashFlowsReport cashFlows;

        #region constructor
        public CashAvailableConstituentReport()
        {
            cashBalancePayment = new CashBalancePaymentReport();
            previousCashBalance = new CashPositionReport();
            cashFlows = new CashFlowsReport();
        }
        #endregion
    }

    /// <summary>
    /// Représente les CashFlows
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashFlowsReport : CashPositionReport
    {
        /// <summary>
        /// Représente les éléments qui constitue les cash Flows en date de Cash Balance 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("constituent", Order = 1)]
        public CashFlowsConstituentReport constituent;

        #region constructor
        public CashFlowsReport()
            : base()
        {
            constituent = new CashFlowsConstituentReport();
        }
        #endregion
    }

    /// <summary>
    /// Représente les éléments qui constituent un CashFlows
    /// </summary>
    /// FI 20150320 [XXPOC] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashFlowsConstituentReport
    {
        /// <summary>
        /// Représente les VMG
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("variationMargin", Order = 1)]
        public ContractSimplePaymentReport variationMargin;

        /// <summary>
        /// Représente les primes
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("premium", Order = 2)]
        public ContractSimplePaymentReport premium;

        /// <summary>
        /// Représente les cashSettlement
        /// </summary>
        /// FI 20150320 [XXPOC] cashSettlement est de type  ContractSimplePaymentConstituentReport
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 3)]
        public ContractSimplePaymentConstituentReport cashSettlement;

        /// <summary>
        /// Représente les frais 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fee", Order = 4)]
        public ContractPaymentReport[] fee;

        /// <summary>
        /// Représente les frais de garde
        /// </summary>
        /// FI 20150709 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("safekeeping", Order = 5)]
        public ContractPaymentReport[] safekeeping;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equalisationPaymentSpecified;

        /// <summary>
        /// Représente les Equalisation Payments
        /// </summary>
        /// PM 20170911 [23408] Add equalisationPayment
        [System.Xml.Serialization.XmlElementAttribute("equalisationPayment", Order = 6)]
        public ContractSimplePaymentReport equalisationPayment;

        #region constructor
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// FI 20151019 [21317] Modify
        public CashFlowsConstituentReport()
        {
            variationMargin = new ContractSimplePaymentReport();
            premium = new ContractSimplePaymentReport();
            cashSettlement = new ContractSimplePaymentConstituentReport();
            fee = new ContractPaymentReport[] { };
            // FI 20151019 [21317] add safekeeping
            safekeeping = new ContractPaymentReport[] { };
            // PM 20170911 [23408] Add equalisationPaymentSpecified
            equalisationPaymentSpecified = false;
        }
        #endregion

    }

    /// <summary>
    /// SimplePayment avec détail par Contrat Dérivé
    /// </summary>
    /// FI 20140909 [20275] Rename DerivativeContractSimplePaymentReport to ContractSimplePaymentReport
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractSimplePaymentReport : SimplePaymentReport
    {

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmountReport[] detail;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ContractSimplePaymentReport()
            : base()
        {
            this.detailSpecified = false;
            this.detail = new ContractAmountReport[] { new ContractAmountReport() };
        }
        #endregion

    }

    /// <summary>
    /// Représente un montant vis à vis d'un contrat dérivé (DerivativeContract) ou d'un asset
    /// </summary>
    /// FI 20140909 [20275] Rename DerivativeContractAmountReport to ContractAmountReport
    /// FI 20140909 [20275] Modify (add idAsset, assetCategory)
    /// FI 20150622 [21124] Modify (add ValueDate)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractAmountReport : ReportAmountSide
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool ValueDateSpecified;

        /// <summary>
        /// Date valeur
        /// </summary>
        [XmlAttributeAttribute("valDt", DataType = "date")]
        public DateTime ValueDate;


        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool idAssetSpecified;

        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttributeAttribute("assetCategory")]
        public Cst.UnderlyingAsset assetCategory;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified;
        /// <summary>
        /// Représente le contrat dérivé (Symbole)
        /// </summary>
        [XmlAttributeAttribute("sym")]
        public String sym;


        [XmlIgnoreAttribute()]
        public bool exchSpecified;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttributeAttribute("exch")]
        public String exch;

        #region constructor
        public ContractAmountReport()
            : base()
        {
        }
        #endregion

    }

    /// <summary>
    /// Represente ??????
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PreviousMarginConstituentReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 1)]
        public CashPositionReport marginRequirement;

        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 2)]
        public CashPositionReport cashAvailable;


        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 3)]
        public CashPositionReport cashUsed;

        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 4)]
        public CashPositionReport collateralAvailable;

        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 5)]
        public CashPositionReport collateralUsed;


        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 6)]
        public CashPositionReport uncoveredMarginRequirement;

        #region constructor
        public PreviousMarginConstituentReport()
        {
            marginRequirement = new CashPositionReport();
            cashAvailable = new CashPositionReport();
            cashUsed = new CashPositionReport();
            collateralAvailable = new CashPositionReport();
            collateralUsed = new CashPositionReport();
            uncoveredMarginRequirement = new CashPositionReport();
        }
        #endregion


    }

    /// <summary>
    /// Represente ??????
    /// </summary>
    /// FI 20150320 [XXPOC] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class MarginConstituentReport
    {


        [XmlIgnoreAttribute()]
        public bool globalAmountSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("globalAmount", Order = 1)]
        public CashPositionReport globalAmount;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool futureSpecified;

        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("future", Order = 2)]
        public CashPositionReport future;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool optionSpecified;

        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("option", Order = 3)]
        public OptionMarginConstituentReport[] option;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otherSpecified;

        /// <summary>
        /// "other" for non ETD amount
        /// </summary>
        /// FI 20150320 [XXPOC] Add
        [System.Xml.Serialization.XmlElementAttribute("other", Order = 4)]
        public AssetMarginConstituentReport[] other;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// Détail des montants par contrat
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 5)]
        public ContractAmountReport[] detail;

        #region constructor
        public MarginConstituentReport()
        {
            globalAmount = new CashPositionReport();
            future = new CashPositionReport();
            option = new OptionMarginConstituentReport[] { };
            other = new AssetMarginConstituentReport[] { };
            detail = new ContractAmountReport[] { };
        }
        #endregion
    }

    /// <summary>
    /// Reprente le stream en devise comptable
    /// </summary>
    /// FI 20150320 [XXPOC] Modify
    /// FI 20150323 [XXPOC] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeCashBalanceStreamReport
    {
        /// <summary>
        /// Représente la devise du stream
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("ccy")]
        public string currency;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginRequirementSpecified;
        /// <summary>
        /// Représente le déposit en devise comptable
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 2)]
        public CashPositionReport marginRequirement;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashAvailableSpecified;
        /// <summary>
        /// Représente le cash disponible en devise comptable
        /// </summary>
        /// FI 20140922 [20275] cashAvailable est désormais de type CashAvailableReport
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 3)]
        public CashAvailableReport cashAvailable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashUsedSpecified;
        /// <summary>
        /// Représente le cash utilisé pour convrir le rique en devise comptable
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 4)]
        public CashPositionReport cashUsed;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralAvailableSpecified;
        /// <summary>
        /// Représente le collateral pour convrir le rique en devise comptable
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 5)]
        public CashPositionReport collateralAvailable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralUsedSpecified;
        /// <summary>
        /// Représente le collateral utilisé pour convrir le rique en devise comptable
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 6)]
        public CashPositionReport collateralUsed;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool uncoveredMarginRequirementSpecified;
        /// <summary>
        /// Représente le défaut de garantie (garanties utilisées - espèces utilisées)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 7)]
        public CashPositionReport uncoveredMarginRequirement;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginCallSpecified;
        /// <summary>
        /// Représente l'appel/restitution de déposit en en devise comptable
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("marginCall", Order = 8)]
        public SimplePaymentReport marginCall;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashBalanceSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", Order = 9)]
        public CashPositionReport cashBalance;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousMarginConstituentSpecified;
        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("previousMarginConstituent", Order = 10)]
        public PreviousMarginConstituentReport previousMarginConstituent;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool realizedMarginSpecified;
        /// <summary>
        /// Realized Margin
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", Order = 11)]
        public MarginConstituentReport realizedMargin;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unrealizedMarginSpecified;
        /// <summary>
        /// Unrealized Margin
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", Order = 12)]
        public MarginConstituentReport unrealizedMargin;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool liquidatingValueSpecified;
        /// <summary>
        /// Liquidating Value
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("liquidatingValue", Order = 13)]
        public OptionLiquidatingValueReport liquidatingValue;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketValueSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150623 [21149] Add
        [System.Xml.Serialization.XmlElementAttribute("marketValue", Order = 14)]
        public DetailedCashPositionReport marketValue;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fundingSpecified;
        /// <summary>
        /// Funding
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("funding", Order = 15)]
        public DetailedCashPositionReport funding;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool borrowingSpecified;
        /// <summary>
        /// unsettledCash
        /// </summary>
        /// FI 20150320 [XXPOC] Add
        [System.Xml.Serialization.XmlElementAttribute("borrowing", Order = 16)]
        public DetailedCashPositionReport borrowing;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unsettledCashSpecified;
        /// <summary>
        /// unsettledCash
        /// </summary>
        /// FI 20150320 [XXPOC] Add
        [System.Xml.Serialization.XmlElementAttribute("unsettledCash", Order = 17)]
        public DetailedCashPositionReport unsettledCash;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardCashPaymentSpecified;
        /// <summary>
        /// Forward Cash Payment
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", Order = 18)]
        public CashBalancePaymentReport forwardCashPayment;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceSpecified;
        /// <summary>
        /// Equity Balance
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", Order = 19)]
        public CashPositionReport equityBalance;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceWithForwardCashSpecified;
        /// <summary>
        /// Equity Balance With Forward Cash
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", Order = 20)]
        public CashPositionReport equityBalanceWithForwardCash;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalAccountValueSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150623 [21149] Add
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", Order = 21)]
        public CashPositionReport totalAccountValue;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitSpecified;
        /// <summary>
        /// Excess Deficit
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", Order = 22)]
        public CashPositionReport excessDeficit;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitWithForwardCashSpecified;
        /// <summary>
        /// Excess Deficit With Forward Cash
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", Order = 23)]
        public CashPositionReport excessDeficitWithForwardCash;


        #region constructor
        public ExchangeCashBalanceStreamReport()
        {
            marginRequirement = new CashPositionReport();
            cashAvailable = new CashAvailableReport();
            cashUsed = new CashPositionReport();
            collateralAvailable = new CashPositionReport();
            collateralUsed = new CashPositionReport();
            uncoveredMarginRequirement = new CashPositionReport();
            marginCall = new SimplePaymentReport();
            previousMarginConstituent = new PreviousMarginConstituentReport();
            realizedMargin = new MarginConstituentReport();
            unrealizedMargin = new MarginConstituentReport();
            liquidatingValue = new OptionLiquidatingValueReport();
            funding = new DetailedCashPositionReport();
            borrowing = new DetailedCashPositionReport();
            unsettledCash = new DetailedCashPositionReport();
            forwardCashPayment = new CashBalancePaymentReport();
            equityBalance = new CashPositionReport();
            equityBalanceWithForwardCash = new CashPositionReport();
            excessDeficit = new CashPositionReport();
        }
        #endregion constructor
    }

    /// <summary>
    /// Payment avec détail par derivative Contract ou asset
    /// <para>Le détail s'applique sur le montant HT et les taxes</para>
    /// </summary>
    /// FI 20140909 [20275] Rename DerivativeContractPaymentReport to ContractPaymentReport 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractPaymentReport : ReportFee
    {
        /// <summary>
        ///  date de payment au formmat ISO
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "paymentDate")]
        public string paymentDate;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// détail par DC des montants HT 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmountAndTaxReport[] detail;

    }

    /// <summary>
    /// Représente un montant et un montant de taxe vis à vis d'un contrat dérivé (DerivativeContract) ou un asset 
    /// </summary>
    /// FI 20140909 [20275] Rename DerivativeContractAmountAndTaxReport to ContractAmountAndTaxReport 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractAmountAndTaxReport : ContractAmountReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 1)]
        public ReportAmountSide tax;
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SimplePaymentReport : ReportAmountSide
    {

        /// <summary>
        ///  date de payment au formmat ISO
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "paymentDate")]
        public string paymentDate;
    }

    /// <summary>
    /// Cash payment et son détail
    /// </summary>
    /// FI 20140909 [20275] add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalancePaymentReport : CashPositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashDepositSpecified;
        /// <summary>
        /// Dépôts
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashDeposit", Order = 1)]
        public DetailedCashPaymentReport cashDeposit;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashWithdrawalSpecified;
        /// <summary>
        /// Retraits
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashWithdrawal", Order = 2)]
        public DetailedCashPaymentReport cashWithdrawal;

    }

    /// <summary>
    /// Montant de cash payment avec son détail par payment Type
    /// </summary>
    /// FI 20140909 [20275] add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedCashPaymentReport : CashPositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// Détail par payment Type
        /// </summary>
        [XmlElementAttribute("detail", Order = 1)]
        public CashPaymentDetailReport[] detail;
    }

    /// <summary>
    /// Montant pour un type de payment
    /// </summary>
    /// FI 20150128 [20275] Modify class (Herite de ReportPayment)
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashPaymentDetailReport : ReportPayment
    {

    }

    /// <summary>
    /// Constituant d'un montant portant sur des options
    /// </summary>
    /// FI 20140910 [20275] add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OptionMarginConstituentReport : CashPositionReport
    {
        /// <summary>
        /// Méthode de calcule de la prime
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("FutValMeth")]
        public FixML.v50SP1.Enum.FuturesValuationMethodEnum valuationMethod;
    }

    /// <summary>
    /// Montant détaillé par contrat ou asset
    /// </summary>
    /// FI 20140910 [20275] Add class
    /// FI 20150622 [21124] Mod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedCashPositionReport : CashPositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;
        /// <summary>
        /// Détails du montant
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmountReport[] detail;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dateDetailSpecified;
        /// <summary>
        /// Détails du montant par date
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("dateDetail", Order = 2)]
        public DetailedDateAmountReport[] dateDetail;

    }

    /// <summary>
    /// Détail d'un montant de valeur liquidative options
    /// </summary>
    /// FI 20140910 [20275] add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OptionLiquidatingValueReport : CashPositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool longOptionValueSpecified;

        /// <summary>
        /// Valeur liquidative pour la position long
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("longOptionValue", Order = 1)]
        public CashPositionReport longOptionValue;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool shortOptionValueSpecified;

        /// <summary>
        /// Valeur liquidative pour la position short
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("shortOptionValue", Order = 2)]
        public CashPositionReport shortOptionValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150320 [XXPOC] add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractSimplePaymentConstituentReport : ContractSimplePaymentReport
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool optionSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("option", Order = 1)]
        public OptionMarginConstituentReport[] option;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otherSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("other", Order = 2)]
        public CashPositionReport other;

    }



    /// <summary>
    /// Montant détaillé par date
    /// </summary>
    /// FI 20150622 [21124] New classe
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedDateAmountReport : ReportAmountSide
    {
        /// <summary>
        /// Date valeur
        /// </summary>
        [XmlAttributeAttribute("valDt", DataType = "date")]
        public DateTime valueDate;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;
        /// <summary>
        /// Détails du montant
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmountReport[] detail;
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetMarginConstituentReport : CashPositionReport
    {

        /// <summary>
        /// 
        /// </summary>
        public bool assetCategorySpecified;

        /// <summary>
        /// Catégorie d'asset
        /// </summary>
        public Cst.UnderlyingAsset assetCategory;

    }
}