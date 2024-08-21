#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.v30.AssetDef;
using EfsML.v30.Doc;
using EfsML.v30.Shared;
using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.v44.Shared;
using System;
using System.Reflection;
using System.Xml.Serialization;
#endregion using directives


namespace EfsML.v30.CashBalance
{
    /// <summary>
    /// Représente les CashFlows
    /// </summary>
    /// FI 20110824 [CashBalance] add CashFlows
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashFlows : CashPosition
    {
        /// <summary>
        /// Représente les éléments qui constitue les cash Flows en date de Cash Balance 
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "constituent", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("constituent", Order = 1)]
        public CashFlowsConstituent constituent;

        /// <summary>
        /// Do not used
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Constituent")]
        public bool FillBalise2;
    }

    /// <summary>
    /// Représente les éléments qui constituent un CashFlows
    /// </summary>
    /// FI 20110824 [CashBalance] add CashFlowsConstituent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashFlowsConstituent : ItemGUI
    {
        /// <summary>
        /// Représente les VMG
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Variation margin", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("variationMargin", Order = 1)]
        public ContractSimplePayment variationMargin;

        /// <summary>
        /// Représente les primes
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Variation margin")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("premium", Order = 2)]
        public ContractSimplePayment premium;

        /// <summary>
        /// Représente les cashSettlement
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 3)]
        //PM 20150319 [POC] Change type of "cashSettlement" from ContractSimplePayment to ContractSimplePaymentConstituent
        //public ContractSimplePayment cashSettlement;
        public ContractSimplePaymentConstituent cashSettlement;

        /// <summary>
        /// Représente les frais par type / par date ??? (Good Question?)
        /// </summary>
        //FI 20120719 [18009] fee est de type DerivativeContractPayment
        //PM 20140829 [20066][20185] Change type of "fee" from DerivativeContractPayment to DetailedContractPayment
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fee", IsClonable = false, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1, IsCopyPasteItem = false)]
        [System.Xml.Serialization.XmlElementAttribute("fee", Order = 4)]
        public DetailedContractPayment[] fee;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool safekeepingSpecified;

        /// <summary>
        /// Représente les frais Safekeeping
        /// </summary>
        /// PM 20150709 [21103] Add safekeeping
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Safekeeping")]
        //[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Safekeeping", IsClonable = false, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 0, IsCopyPasteItem = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Safekeeping", IsChild = true)]
        [System.Xml.Serialization.XmlElementAttribute("safekeeping", Order = 5)]
        public DetailedContractPayment[] safekeeping;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equalisationPaymentSpecified;

        /// <summary>
        /// Représente les Equalisation Payments
        /// </summary>
        /// PM 20170911 [23408] Add equalisationPayment
        [System.Xml.Serialization.XmlElementAttribute("equalisationPayment", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Equalisation Payment")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public ContractSimplePayment equalisationPayment;
    }

    /// <summary>
    /// Représente le cash disponible
    /// </summary>
    /// FI 20110824 [CashBalance] add CashAvailable
    /// PM 20140918 [20066][20185] constituent devient optionel
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashAvailable : ExchangeCashPosition
    {
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool constituentSpecified;

        /// <summary>
        /// Représente les éléments qui constituent le cash disponible
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "constituent", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("constituent", Order = 1)]
        public CashAvailableConstituent constituent;

        /// <summary>
        /// Do not used
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "constituent")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise2;
    }

    /// <summary>
    /// Représente les collateral disponible
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CollateralAvailable : ExchangeCashPosition
    {
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool constituentSpecified;

        /// <summary>
        /// Représente les éléments qui constituent le collateral disponible
        /// <para>Renseigné uniquement si Déposit en devise et couverture en contrevaleur</para>
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "constituent")]
        [System.Xml.Serialization.XmlElementAttribute("constituent", Order = 1)]
        public CollateralAvailableConstituent constituent;

        ///// <summary>
        ///// Do not used
        ///// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "constituent")]
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool FillBalise2;
    }

    /// <summary>
    /// Représente les éléments d'un collateral disponible pour le cash balance
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CollateralAvailableConstituent : ItemGUI
    {
        /// <summary>
        /// Représente les collatéraux en devise d'origine, après abattment
        /// </summary>
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral", IsChild = true)]
        [System.Xml.Serialization.XmlElementAttribute("collateral", Order = 1)]
        public Money[] collateral;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralAlreadyUsedSpecified;

        /// <summary>
        /// Représente les collatéraux déjà utilisés dans les devise précédentes
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral already used", IsChild = true)]
        [System.Xml.Serialization.XmlElementAttribute("collateralAlreadyUsed", Order = 2)]
        public Money[] collateralAlreadyUsed;
    }


    /// <summary>
    /// Représente les éléments d'un cash disponible pour le cash balance
    /// </summary>
    /// FI 20110824 [CashBalance] add CashAvailableConstituent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CashAvailableConstituent : ItemGUI
    {
        /// <summary>
        /// Représente les versements et retraits en espèce
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash balance payment", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalancePayment", Order = 1)]
        //PM 20140829 [20066][20185] Change type of "cashBalancePayment" from CashPosition to CashBalancePayment for cashBalancePayment
        //public CashPosition cashBalancePayment;
        public CashBalancePayment cashBalancePayment;

        /// <summary>
        /// Représente le solde précédent
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous balance payment")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous cash balance", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("previousCashBalance", Order = 2)]
        public CashPosition previousCashBalance;

        /// <summary>
        /// Représente les cash Flows en date du CashBalance
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous cash balance")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash flows", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashFlows", Order = 3)]
        public CashFlows cashFlows;

        /// <summary>
        /// Do not used
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash flows")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Represente un stream en devise d'origine
    /// </summary>
    /// FI 20110824 [CashBalance] add CashBalanceStream
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalanceStream : ItemGUI
    {
        /// <summary>
        /// Représente la devise du stream
        /// </summary>
        [ControlGUI(Name = "currency")]
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        public Currency currency;

        /// <summary>
        /// Représente le risque dans la devise du stream (somme des risques toutes chambres confondues)
        /// <para>La contrevaleur du risque doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        /// FI 20120719 [18009] marginRequirement est de type CssExchangeCashPosition
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 2)]
        public CssExchangeCashPosition marginRequirement;

        /// <summary>
        /// Représente le cash disponible 
        /// <para>La contrevaleur du cash doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 3)]
        public CashAvailable cashAvailable;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashUsedSpecified;

        /// <summary>
        /// Représente le cash utilisé pour couvrir le déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé en  devise comptable</para>
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "cash used")]
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 4)]
        public CashPosition cashUsed;


        /// <summary>
        /// Représente le détail des collateraaux disponibles 
        /// <para>Methode "DEFAULT" : le haircut n'est pas appliqué (puisqu'il peut être spécifique à chaque chambre de compensation)</para>
        /// <para>Methode "UK" : le haircut est appliqué (nécessairement unique)</para>
        /// <para>Généralement la somme des montants = collateralAvailable  ((Vrai si méthode "UK") (Vrai si méthode "DEFAULT" et "Déposit et couverture en devise"))</para>
        /// </summary>
        /// FI 20160530 [21885] Add 
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "cashUsed")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)] // FI 20160530 [21885] GLOP (prévoir l'affichage de cet élément)
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "collateral")]
        [XmlArray(ElementName = "collaterals", Order = 5)]
        [XmlArrayItemAttribute("collateral")]
        public PosCollateral[] collateral;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralSpecified;


        /// <summary>
        /// Représente le collateral disponible pour couvrir le déposit
        /// <para>La contrevaleur du collateral doit être alimentée si appels/restitutions de deposit en devise comptable</para>
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 6)]
        public CollateralAvailable collateralAvailable;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralUsedSpecified;


        /// <summary>
        /// Représente le collateral utilisé pour couvrir le déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé en  devise comptable</para>
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral used")]
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 7)]
        public CashPosition collateralUsed;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool uncoveredMarginRequirementSpecified;

        /// <summary>
        /// Représente le défaut de garantie (garanties utilisées - espèces utilisées)
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé dans la devise d'origine</para>
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Uncovered margin requirement")]
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 8)]
        public CashPosition uncoveredMarginRequirement;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginCallSpecified;

        /// <summary>
        /// Représente l'appel/restitution de déposit
        /// <para>Doit être alimenté uniquement lorsque l'appel/restition est calculé dans la devise d'origine</para>
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin call")]
        [System.Xml.Serialization.XmlElementAttribute("marginCall", Order = 9)]
        public SimplePayment marginCall;

        /// <summary>
        /// Représente le nouveau solde 
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash balance", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", Order = 10)]
        public CashPosition cashBalance;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousMarginConstituentSpecified;

        /// <summary>
        /// 
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash balance")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous margin constituent", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("previousMarginConstituent", Order = 11)]
        public PreviousMarginConstituent previousMarginConstituent;

        /// <summary>
        /// ??????????
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool realizedMarginSpecified;

        /// <summary>
        /// Realized Margin
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous margin constituent")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Realized Margin", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", Order = 12)]
        public MarginConstituent realizedMargin;

        /// <summary>
        /// ??????????
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unrealizedMarginSpecified;

        /// <summary>
        /// Unrealized Margin
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Realized Margin")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unrealized Margin", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", Order = 13)]
        public MarginConstituent unrealizedMargin;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool liquidatingValueSpecified;

        /// <summary>
        /// Liquidating Value
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unrealized Margin")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Liquidating Value", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("liquidatingValue", Order = 14)]
        public OptionLiquidatingValue liquidatingValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketValueSpecified;

        /// <summary>
        /// Market Value
        /// </summary>
        /// PM 20150616 [21124] add marketValue
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Liquidating Value")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Market Value", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("marketValue", Order = 15)]
        public DetailedCashPosition marketValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fundingSpecified;

        /// <summary>
        /// Funding
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Market Value")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Funding", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("funding", Order = 16)]
        public DetailedCashPosition funding;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool borrowingSpecified;

        /// <summary>
        /// Borrowing
        /// </summary>
        /// PM 20150323 [POC] Add borrowing
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Funding")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Borrowing", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("borrowing", Order = 17)]
        public DetailedCashPosition borrowing;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unsettledCashSpecified;

        /// <summary>
        /// Unsettled Cash
        /// </summary>
        /// PM 20150318 [POC] Add unsettledCash
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Borrowing")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unsettled Cash", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("unsettledCash", Order = 18)]
        public DetailedCashPosition unsettledCash;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardCashPaymentSpecified;

        /// <summary>
        /// Forward Cash Payment
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unsettled Cash")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward Cash Payment", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", Order = 19)]
        public CashBalancePayment forwardCashPayment;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceSpecified;

        /// <summary>
        /// Equity Balance
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward Cash Payment")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", Order = 20)]
        public CashPosition equityBalance;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceWithForwardCashSpecified;

        /// <summary>
        /// Equity Balance With Forward Cash
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance With Forward Cash", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", Order = 21)]
        public CashPosition equityBalanceWithForwardCash;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalAccountValueSpecified;

        /// <summary>
        /// Total Account Value
        /// </summary>
        /// PM 20150616 [21124] add totalAccountValue
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance With Forward Cash")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Total Account Value", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", Order = 22)]
        public CashPosition totalAccountValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitSpecified;

        /// <summary>
        /// Excess Deficit
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Total Account Value")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", Order = 23)]
        public CashPosition excessDeficit;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitWithForwardCashSpecified;

        /// <summary>
        /// Excess Deficit With Forward Cash
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit With Forward Cash", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", Order = 24)]
        public CashPosition excessDeficitWithForwardCash;

        /// <summary>
        /// Do not used
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit With Forward Cash")]
        public bool FillBalise;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
    }

    /// <summary>
    /// Reprente le stream en devise comptable
    /// </summary>
    /// FI 20110824 [CashBalance] add ExchangeCashBalanceStream
    ///PM 20140911 [20066][20185] Add cashBalance, realizedMargin, unrealizedMargin, liquidatingValue, funding, forwardCashPayment, equityBalance, equityBalanceWithForwardCash, excessDeficit, excessDeficitWithForward
    ///PM 20140918 [20066][20185] Change type of cashAvailable from CashPosition to CashAvailable
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeCashBalanceStream : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(Name = "currency")]
        public Currency currency;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginRequirementSpecified;

        /// <summary>
        /// Représente le déposit en devise comptable
        /// </summary>
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Margin requirement")]
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 2)]
        public CashPosition marginRequirement;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashAvailableSpecified;

        /// <summary>
        /// Représente le cash disponible en devise comptable
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash available")]
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 3)]
        //PM 20140918 [20066][20185] Change type of cashAvailable from CashPosition to CashAvailable
        //public CashPosition cashAvailable;
        public CashAvailable cashAvailable;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashUsedSpecified;

        /// <summary>
        /// Représente le cash utilisé pour convrir le rique en devise comptable
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash used", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash used")]
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 4)]
        public CashPosition cashUsed;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralAvailableSpecified;

        /// <summary>
        /// Représente le collateral pour convrir le rique en devise comptable
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash used")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Collateral available")]
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 5)]
        public CashPosition collateralAvailable;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collateralUsedSpecified;

        /// <summary>
        /// Représente le collateral utilisé pour convrir le rique en devise comptable
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral used", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Collateral used")]
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 6)]
        public CashPosition collateralUsed;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool uncoveredMarginRequirementSpecified;

        /// <summary>
        /// Représente le défaut de garantie (garanties utilisées - espèces utilisées)
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral used")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Uncovered margin requirement")]
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 7)]
        public CashPosition uncoveredMarginRequirement;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marginCallSpecified;

        /// <summary>
        /// Représente l'appel/restitution de déposit en en devise comptable
        /// </summary>
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin call", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Margin call")]
        [System.Xml.Serialization.XmlElementAttribute("marginCall", Order = 8)]
        public SimplePayment marginCall;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashBalanceSpecified;

        /// <summary>
        /// Représente le nouveau solde 
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin call")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash balance", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash balance")]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", Order = 9)]
        public CashPosition cashBalance;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousMarginConstituentSpecified;

        /// <summary>
        /// ?????????
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash balance")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous margin constituent", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Previous margin constituent")]
        [System.Xml.Serialization.XmlElementAttribute("previousMarginConstituent", Order = 10)]
        public PreviousMarginConstituent previousMarginConstituent;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool realizedMarginSpecified;

        /// <summary>
        /// Realized Margin
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Previous margin constituent")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Realized Margin", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Realized Margin")]
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", Order = 11)]
        //PM 20140911 [20066][20185] Add realizedMargin
        public MarginConstituent realizedMargin;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unrealizedMarginSpecified;

        /// <summary>
        /// Unrealized Margin
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Realized Margin")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unrealized Margin", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Unrealized Margin")]
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", Order = 12)]
        //PM 20140911 [20066][20185] Add unrealizedMargin
        public MarginConstituent unrealizedMargin;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool liquidatingValueSpecified;

        /// <summary>
        /// Liquidating Value
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unrealized Margin")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Liquidating Value", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Liquidating Value")]
        [System.Xml.Serialization.XmlElementAttribute("liquidatingValue", Order = 13)]
        //PM 20140911 [20066][20185] Add liquidatingValue
        public OptionLiquidatingValue liquidatingValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketValueSpecified;

        /// <summary>
        /// Market Value
        /// </summary>
        /// PM 20150616 [21124] add marketValue
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Liquidating Value")]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Market Value", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Market Value")]
        [System.Xml.Serialization.XmlElementAttribute("marketValue", Order = 14)]
        public DetailedCashPosition marketValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fundingSpecified;

        /// <summary>
        /// Funding
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Market Value")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Funding", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Funding")]
        [System.Xml.Serialization.XmlElementAttribute("funding", Order = 15)]
        public DetailedCashPosition funding;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool borrowingSpecified;

        /// <summary>
        /// Borrowing
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Funding")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Borrowing", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Borrowing")]
        [System.Xml.Serialization.XmlElementAttribute("borrowing", Order = 16)]
        //PM 20150323 [POC] Add borrowing
        public DetailedCashPosition borrowing;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unsettledCashSpecified;

        /// <summary>
        /// Unsettled Cash
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Borrowing")]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unsettled Cash", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Unsettled Cash")]
        [System.Xml.Serialization.XmlElementAttribute("unsettledCash", Order = 17)]
        //PM 20150319 [POC] Add unsettledCash
        public DetailedCashPosition unsettledCash;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardCashPaymentSpecified;

        /// <summary>
        /// Forward Cash Payment
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unsettled Cash")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward Cash Payment", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Forward Cash Payment")]
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", Order = 18)]
        public CashBalancePayment forwardCashPayment;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceSpecified;

        /// <summary>
        /// Equity Balance
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward Cash Payment")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Equity Balance")]
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", Order = 19)]
        public CashPosition equityBalance;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool equityBalanceWithForwardCashSpecified;

        /// <summary>
        /// Equity Balance With Forward Cash
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance With Forward Cash", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Equity Balance with Forward Cash")]
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", Order = 20)]
        public CashPosition equityBalanceWithForwardCash;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalAccountValueSpecified;

        /// <summary>
        /// Total Account Value
        /// </summary>
        /// PM 20150616 [21124] add totalAccountValue
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Equity Balance With Forward Cash")]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Total Account Value", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Total Account Value")]
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", Order = 21)]
        public CashPosition totalAccountValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitSpecified;

        /// <summary>
        /// Excess Deficit
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Total Account Value")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Excess Deficit")]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", Order = 22)]
        //PM 20140911 [20066][20185] Add excessDeficit
        public CashPosition excessDeficit;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool excessDeficitWithForwardCashSpecified;

        /// <summary>
        /// Excess Deficit With Forward Cash
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit")]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit With Forward Cash", IsVisible = false, IsCopyPaste = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Excess Deficit with Forward Cash")]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", Order = 23)]
        //PM 20140911 [20066][20185] Add excessDeficitWithForwardCash
        public CashPosition excessDeficitWithForwardCash;

        /// <summary>
        /// Do not used
        /// </summary>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Excess Deficit With Forward Cash")]
        //public bool FillBalise;

        [System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
    }

    /// <summary>
    /// Cash Balance(modeled as an FpML:Product)
    /// </summary>
    /// FI 20110824 [XXXXX] Add
    /// FI 20161027 [22151] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("cashBalance", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Cash Balance")]
    public partial class CashBalance : Product
    {
        #region Members
        /// <summary>
        /// Représente l'acteur sur lequel s'applique le CashBalance
        /// <para>Cet acteur n'a pas nécessairement le rôle CashBalanceOffice</para>
        /// </summary>
        [ControlGUI(Name = "Cash Balance Office", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceOfficePartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        public PartyReference cashBalanceOfficePartyReference;

        /// <summary>
        /// Représente l'entité
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("entityPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [ControlGUI(Name = "Entity", LineFeed = MethodsGUI.LineFeedEnum.None)]
        public PartyReference entityPartyReference;

        /// <summary>
        /// Intraday ou EndOfDay 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("timing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 3)]
        [ControlGUI(Name = "Timing", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public SettlSessIDEnum timing;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeCashBalanceStreamSpecified;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exchange cash balance stream", Color = MethodsGUI.ColorEnum.Green)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeCashBalanceStream", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 4)]
        public ExchangeCashBalanceStream exchangeCashBalanceStream;

        /// <summary>
        /// 
        /// </summary>
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash balance stream", IsClonable = false, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 1, IsCopyPasteItem = false, Color = MethodsGUI.ColorEnum.Green)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceStream", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 5)]
        public CashBalanceStream[] cashBalanceStream;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxRateSpecified;

        /// <summary>
        /// Represente un array de taux de change 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Fx rate")]
        [System.Xml.Serialization.XmlElementAttribute("fxRate", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 6)]
        public IdentifiedFxRate[] fxRate;

        /// <summary>
        /// Represente les paramétrages en vigueur sur l'acteur CashBalanceOffice 
        /// <para>Pratique pour l'audit</para>
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Settings")]
        [System.Xml.Serialization.XmlElementAttribute("settings", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        public CashBalanceSettings settings;


        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool endOfDayStatusSpecified;

        /// <summary>
        ///  Etats des traitements de fin de journée 
        /// </summary>
        /// FI 20161027 [22151] Modify
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "EndOfDayStatus")]
        [System.Xml.Serialization.XmlElementAttribute("endOfDayStatus", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 8)]
        public EndOfDayStatus endOfDayStatus;

        #endregion Members
    }

    /// <summary>
    /// Represente ??????
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PreviousMarginConstituent : ItemGUI
    {
        /// <summary>
        /// ?????????
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", Order = 1)]
        public CashPosition marginRequirement;

        /// <summary>
        /// ?????????
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin requirement")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", Order = 2)]
        public CashPosition cashAvailable;


        /// <summary>
        /// ?????????
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash available")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash used", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", Order = 3)]
        public CashPosition cashUsed;

        /// <summary>
        /// ?????????
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash used")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", Order = 4)]
        public CashPosition collateralAvailable;

        /// <summary>
        /// ?????????
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral available")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral used", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", Order = 5)]
        public CashPosition collateralUsed;


        /// <summary>
        /// ?????????
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collateral used")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Uncovered margin requirement", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", Order = 6)]
        public CashPosition uncoveredMarginRequirement;


        /// <summary>
        /// ?????????
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Uncovered margin requirement")]
        public bool FillBalise;

    }

    /// <summary>
    /// Représente les directives spécifiées sur l'acteur CashBalanceOffice 
    /// <para>Table CASHBALANCE</para>
    /// </summary>
    /// PM 20140829 [20066][20185] Add cashBalanceMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalanceSettings : ItemGUI
    {
        /// <summary>
        /// Représente le CashBalanceOffice
        /// </summary>
        [ControlGUI(Name = "Cash Balance Office")]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceOfficePartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        public PartyReference cashBalanceOfficePartyReference;

        /// <summary>
        /// Représente le périmètre des cashBalances
        /// </summary>
        [ControlGUI(Name = "Scope", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("scope", Order = 2)]
        public GlobalElementaryEnum scope;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeCurrencySpecified;

        /// <summary>
        /// Représente la devise de contrevaleur pour le calcul des appels/restitutions de déposits lorsque la méthode de calcul est 
        /// <para>
        /// "Déposit et couverture en contrevaleur (MGCCTRVAL)"  ou "Déposit en devise et couverture en contrevaleur (MGCCOLLATCTRVAL)"
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("exchangeCurrency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency exchangeCurrency;

        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Use available cash", Level = MethodsGUI.LevelEnum.Intermediary, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("useAvailableCash", Order = 4)]
        public EFS_Boolean useAvailableCash;

        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Cash And Collateral", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("cashAndCollateral", Order = 5)]
        public CashAndCollateralLocalizationEnum cashAndCollateral;

        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Management Balance", Level = MethodsGUI.LevelEnum.Intermediary, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("managementBalance", Order = 6)]
        public EFS_Boolean managementBalance;

        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Margin Call Calculation Method", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("marginCallCalculationMethod", Order = 7)]
        public MarginCallCalculationMethodEnum marginCallCalculationMethod;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashBalanceMethodSpecified;

        /// <summary>
        /// Methode de calcul du cash balance
        /// </summary>
        [ControlGUI(Name = "Cash Balance Calculation Method", Level = MethodsGUI.LevelEnum.End, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceMethod", Order = 8)]
        public CashBalanceCalculationMethodEnum cashBalanceMethod;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashBalanceCurrencySpecified;

        /// <summary>
        /// Représente la devise de contrevaleur du cash balance (Devise obligatoire lorsque la méthode de calcul est "Méthode 2 (UK)"
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Cash Balance currency", Level = MethodsGUI.LevelEnum.End, Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceCurrency", Order = 9)]
        public Currency cashBalanceCurrency;
    }

    /// <summary>
    /// Represente ??????
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class MarginConstituent : ItemGUI
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool globalAmountSpecified;

        /// <summary>
        /// Montant global
        /// </summary>
        /// PM 20140829 [20066][20185] Add globalAmount
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "GlobalAmount", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("globalAmount", Order = 1)]
        public CashPosition globalAmount;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool futureSpecified;

        /// <summary>
        /// Montant pour les ETD Future
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "GlobalAmount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Future", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("future", Order = 2)]
        public CashPosition future;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool optionSpecified;

        /// <summary>
        /// Montant pour les ETD Option
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Future")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("option", Order = 3)]
        //PM 20140829 [20066][20185] Change type of "option" from CashPosition to OptionMarginConstituent[]
        //public CashPosition option;
        public OptionMarginConstituent[] option;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otherSpecified;

        /// <summary>
        /// Montant autre que ETD
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("other", Order = 4)]
        //PM 20150319 [POC] Add other
        //PM 20150616 [21124] Change type of "option" from CashPosition to OTCMarginConstituent[]
        //public CashPosition other;
        public AssetMarginConstituent[] other;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// Détail des montants par contrat
        /// </summary>
        ///PM 20140829 [20066][20185] Add "detail"
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other")]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 5)]
        public ContractAmount[] detail;

        /// <summary>
        /// 
        /// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Représente un montant et un sens (Crédit ou Débit)
    /// </summary>
    /// FI 20141208 [20549] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AmountSide : ItemGUI
    {

        /// FI 20141208 [XXXXX] add property
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean AmtSideSpecified;

        /// <summary>
        /// Sens vis à vis du CBO
        /// <para>CR le CBO reçoit, DR le CBO paye</para>
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side", Width = 40)]
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "AmtSide")]
        public CrDrEnum AmtSide;

        /// <summary>
        /// Valeur du montant
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "value")]
        [System.Xml.Serialization.XmlAttributeAttribute("Amt")]
        public Decimal Amt
        {
            set { amt = new EFS_Decimal(value); }
            get { return (null != amt) ? amt.DecValue : Convert.ToDecimal(null); }
        }
        /// <summary>
        /// Valeur du montant
        /// </summary>
        protected EFS_Decimal amt;

        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Représente un montant vis à vis d'une Chambre de compensation
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssAmount : AmountSide
    {
        /// <summary>
        /// 
        /// </summary>
        //PM 20150402 [POC] cssHref devient optionel
        [XmlIgnoreAttribute()]
        public bool cssHrefSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string cssHref;
    }


    /// <summary>
    /// Représente un montant vis à vis d'un contrat dérivé ou d'un asset
    /// <para>○ jusqu'à la version 4.2 de Spheres®, seuls sym et exch sont alimentés (seuls des Derivative Contract sont gérés)</para>
    /// <para>○ A partir de la version 4.2, otcmlId et assetCategory sont en plus alimentés (gestion de tout type d'assset)</para>
    /// </summary>
    /// Rename class DerivativeContractAmount to ContractAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractAmount : AmountSide, IEFS_Array
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlIgnoreAttribute()]
        public bool otcmlIdSpecified;

        /// <summary>
        /// Id non significatif de l'asset ou du DC
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        /// <summary>
        /// si ExchangeTradedDerivative alors c'est une DC
        /// </summary>
        [ControlGUI(Name = "Asset Category")]
        [XmlAttributeAttribute("AssetCategory")]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// Symbole du DC ou identifier de l'asset
        /// </summary>
        private EFS_String m_Sym;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified;

        /// <summary>
        /// Représente le symbol du contrat dérivé ou de l'asset
        /// </summary>
        [ControlGUI(Name = "Symbol")]
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { m_Sym = new EFS_String(value); }
            get { return m_Sym?.Value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private EFS_String m_Exch;

        /// <summary>
        /// 
        /// </summary>
        ///PM 20140829 [20066][20185] Change from required to optional for attribute Exch
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExchSpecified;

        /// <summary>
        /// Représente le marché du contract dérivé ou de l'asset
        /// </summary>
        [ControlGUI(Name = "Exchange")]
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Exch
        {
            set { m_Exch = new EFS_String(value); }
            get { return m_Exch?.Value; }
        }

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }


    /// <summary>
    /// Représente un montant et un montant de taxe vis à vis d'un contrat dérivé ou d'un asset
    /// </summary>
    ///PM 20140829 [20066][20185] Rename DerivativeContractAmountAndTax to ContractAmountAndTax
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractAmountAndTax : ContractAmount
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 1)]
        public AmountSide tax;
    }

    /// <summary>
    /// ExchangeCashPosition avec détail par Chambre de compensation
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssExchangeCashPosition : ExchangeCashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// détail des soldes par chambre de compensation (le détail est en devise du solde)
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public CssAmount[] detail;

        /// <summary>
        /// Do not Use
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail")]
        public bool FillBalise;
    }

    /// <summary>
    /// SimplePayment avec détail par Contrat Dérivé
    /// </summary>
    ///PM 20140829 [20066][20185] Rename DerivativeContractSimplePayment to ContractSimplePayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractSimplePayment : SimplePayment
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// 
        /// </summary>
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmount[] detail;
    }

    /// <summary>
    /// Payment avec détail par derivative Contract ou asset
    /// <para>Le détail s'applique sur le montant HT et les taxes</para>
    /// </summary>
    ///PM 20140829 [20066][20185] Rename DerivativeContractPayment to DetailedContractPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedContractPayment : Payment
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// détail par DC ou asset des montants HT 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmountAndTax[] detail;
    }

    /// <summary>
    /// Constituant d'un montant portant sur des options
    /// </summary>
    ///PM 20140829 [20066][20185] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OptionMarginConstituent : CashPosition, IEFS_Array
    {
        /// <summary>
        /// Méthode de calcule de la prime
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Valuation Method", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlAttributeAttribute("FutValMeth")]
        public FuturesValuationMethodEnum valuationMethod;

        /// <summary>
        /// 
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Valuation Method")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }

    /// <summary>
    /// Constituant d'un montant portant sur des contrats OTC
    /// </summary>
    /// PM 20150616 [21124] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetMarginConstituent : CashPosition, IEFS_Array
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlIgnoreAttribute()]
        public bool assetCategorySpecified;

        /// <summary>
        /// Catégorie d'asset
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Asset Category", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlAttributeAttribute("AssetCategory")]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// 
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Asset Category")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }

    /// <summary>
    /// Détail d'un montant de valeur liquidative options
    /// </summary>
    ///PM 20140829 [20066][20185] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OptionLiquidatingValue : CashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool longOptionValueSpecified;

        /// <summary>
        /// Valeur liquidative pour la position long
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Long Option Value", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("longOptionValue", Order = 1)]
        public CashPosition longOptionValue;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool shortOptionValueSpecified;

        /// <summary>
        /// Valeur liquidative pour la position short
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Long Option Value")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Short Option Value", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("shortOptionValue", Order = 2)]
        public CashPosition shortOptionValue;

        /// <summary>
        /// Do not Use
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Short Option Value")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Détail d'un montant de cash payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashBalancePayment : CashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashDepositSpecified;
        /// <summary>
        /// Dépôts
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Deposit", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashDeposit", Order = 1)]
        public DetailedCashPayment cashDeposit;
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashWithdrawalSpecified;
        /// <summary>
        /// Retraits
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Deposit")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Withdrawal", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("cashWithdrawal", Order = 2)]
        public DetailedCashPayment cashWithdrawal;

        /// <summary>
        /// Do not Use
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Withdrawal")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Détail d'un montant de cash payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedCashPayment : CashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// Detail
        /// </summary>
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Detail", IsClonable = true, IsChild = true, IsCopyPasteItem = true, MinItem = 0)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public CashPaymentDetail[] detail;
    }

    /// <summary>
    /// Montant pour un type de payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashPaymentDetail : AmountSide
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentTypeSpecified;
        /// <summary>
        /// Type de Payment
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("paymentType", Order = 1)]
        public PaymentType paymentType;
    }

    /// <summary>
    /// Montant détaillé par contrat
    /// </summary>
    /// PM 20140829 [20066][20185] New
    /// PM 20150616 [21124] Add optional element dateDetail
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedCashPosition : CashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dateDetailSpecified;
        /// <summary>
        /// Détails par date
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("dateDetail", Order = 1)]
        public DetailedDateAmount[] dateDetail;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;
        /// <summary>
        /// Détails du montant
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 2)]
        public ContractAmount[] detail;

        /// <summary>
        /// Do not Use
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
    }

    /// <summary>
    /// Constituant d'un payment
    /// </summary>
    //PM 20150319 [POC] Add ContractSimplePaymentConstituent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractSimplePaymentConstituent : ContractSimplePayment
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool optionSpecified;

        /// <summary>
        /// Montant pour les ETD Option
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("option", Order = 3)]
        public OptionMarginConstituent[] option;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otherSpecified;

        /// <summary>
        /// Montant autre que ETD
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Other", IsVisible = false, IsCopyPaste = false)]
        [System.Xml.Serialization.XmlElementAttribute("other", Order = 4)]
        public CashPosition other;

        /// <summary>
        /// 
        /// </summary>
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBaliseContractSimplePaymentConstituent;
    }

    /// <summary>
    /// Montant détaillé par date
    /// </summary>
    /// PM 20150616 [21124] New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DetailedDateAmount : AmountSide
    {
        /// <summary>
        /// Date valeur
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlAttributeAttribute("ValDt", DataType = "date")]
        public DateTime ValueDate;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailSpecified;

        /// <summary>
        /// Détails du montant
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlElementAttribute("detail", Order = 1)]
        public ContractAmount[] detail;
    }

    /// <summary>
    ///  
    /// </summary>
    /// FI 20160530 [21885] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRoot(ElementName = "collaterals")]
    public partial class PosCollaterals
    {
        [System.Xml.Serialization.XmlElementAttribute("collateral")]
        public PosCollateral[] collateral;
    }


    /// <summary>
    ///  Représent un dépôt de garanties 
    /// </summary>
    /// FI 20160530 [21885] Add 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PosCollateral : ItemGUI
    {
        /// <summary>
        ///  Id non significatif du dépôt de garantie
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /* otcmlId pertmet d'accéder à l'acteur comme au book, je laisse bookId uniquement parce que déjà suffisamment significatif 
        /// <summary>
        /// Représente l'acteur déposant (côté ENTITY) ou le dépositaire (côté CLEARER)
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Actor Id")]
        [System.Xml.Serialization.XmlElementAttribute("actorId")]
        public ActorId actorId;
        */

        /// <summary>
        /// Représente le book déposant (côté ENTITY) ou le dépositaire (côté CLEARER)
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Book Id")]
        [System.Xml.Serialization.XmlElementAttribute("bookId")]
        public BookId bookId;

        /// <summary>
        /// Représente l'asset  
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "asset")]
        [System.Xml.Serialization.XmlElementAttribute("asset")]
        public ContractAsset asset;

        /// <summary>
        /// Représente la valorisation du dépôt de garantie
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "valuation")]
        [System.Xml.Serialization.XmlElementAttribute("valuation")]
        public PosCollateralValuation valuation;

        /// <summary>
        /// Représentes les haircuts appliqués par chambre
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "haircut")]
        [System.Xml.Serialization.XmlElementAttribute("haircut")]
        public CssValue[] haircut;


        /// <summary>
        /// 
        /// </summary>
        public PosCollateral()
        {
            haircut = new CssValue[] { };
            valuation = new PosCollateralValuation();
            bookId = new BookId()
            {
                bookIdScheme = string.Empty
            };
            /*actorId = new ActorId()
            {
                actorIdScheme = string.Empty
            };*/

            asset = new ContractAsset();
        }
    }



    /// <summary>
    ///  Valeur d'une position déposée en garantie 
    /// </summary>
    /// FI [21885] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PosCollateralValuation : AmountSide
    {

        /// <summary>
        ///  Id non significatif de la valorisation (IDPOSCOLLATERALVAL)
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified;

        // EG 20170127 Qty Long To Decimal
        protected EFS_Decimal _qty;
        /// <summary>
        /// Qty en position (lorsque l'asset est de type titre (Action, titre de créance, etc...) 
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Qty")]
        [System.Xml.Serialization.XmlAttributeAttribute("Qty")]
        // EG 20170127 Qty Long To Decimal
        public decimal Qty
        {
            set { _qty = new EFS_Decimal(value); }
            get { return (null != _qty) ? _qty.DecValue : Convert.ToDecimal(null); }
        }
    }

    /// <summary>
    ///  Représente une donnée décimale spécifique à une chambre de compensation
    /// </summary>
    /// FI 20160530 [21885] Add 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssValue : ItemGUI
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool cssHrefSpecified;

        /// <summary>
        /// Représente une référence vers une partie qui représente une chambre de compenstion
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "cssHref", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string cssHref;


        /// <summary>
        /// Valeur du montant
        /// </summary>
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "value")]
        [System.Xml.Serialization.XmlAttributeAttribute("value")]
        public Decimal Value
        {
            set { _value = new EFS_Decimal(value); }
            get { return (null != _value) ? _value.DecValue : Convert.ToDecimal(null); }
        }
        /// <summary>
        /// Valeur du montant
        /// </summary>
        protected EFS_Decimal _value;
    }

    /// <summary>
    /// Représente un Asset ou un DerivativeContract
    /// <para>Similitude avec ContractAmount</para>
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ContractAsset : IEFS_Array
    {
        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlIgnoreAttribute()]
        public bool otcmlIdSpecified;

        /// <summary>
        /// Id non significatif de l'asset ou du DC
        /// <para>Lorsque non renseigné représente nécessaire un Derivative Contract</para>
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        /// <summary>
        /// si ExchangeTradedDerivative alors c'est une DC
        /// </summary>
        [ControlGUI(Name = "Asset Category")]
        [XmlAttributeAttribute("AssetCategory")]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// Symbole du DC ou identifier de l'asset
        /// </summary>
        private EFS_String m_Sym;

        /// <summary>
        /// 
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SymSpecified;

        /// <summary>
        /// Représente le symbol du contrat dérivé ou de l'asset
        /// </summary>
        [ControlGUI(Name = "Symbol")]
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sym
        {
            set { m_Sym = new EFS_String(value); }
            get { return m_Sym?.Value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private EFS_String m_Exch;

        /// <summary>
        /// 
        /// </summary>
        ///PM 20140829 [20066][20185] Change from required to optional for attribute Exch
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExchSpecified;

        /// <summary>
        /// Représente le marché du contract dérivé ou de l'asset
        /// </summary>
        [ControlGUI(Name = "Exchange")]
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Exch
        {
            set { m_Exch = new EFS_String(value); }
            get { return m_Exch?.Value; }
        }

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }


    /// <summary>
    ///  Etats des traitements de fin de journée 
    ///  <para>La liste des marchés/cssCustodian est pilotée par le référentiel "Marchés en activité"</para>
    ///  <para></para>
    /// </summary>
    /// FI 20161027 [22151] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EndOfDayStatus : ItemGUI
    {
        /// <summary>
        ///  Etats des traitements de fin de journée par cssCustodian 
        /// </summary>
        [ControlGUI(Name = "CssCustodain Status")]
        [System.Xml.Serialization.XmlElement()]
        public CssCustodianStatus[] cssCustodianStatus;
    }



    /// <summary>
    ///  Rerésente l'état du process EOD vis à vis d'un marché
    /// </summary>
    /// FI 20161027 [22151] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeStatus : ItemGUI
    {

        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId
        
        /// <summary>
        /// 
        /// </summary>
        private EFS_String m_Exch;


        /// <summary>
        /// Id non significatif du marché
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        ///  Représente le marché
        /// </summary>
        [ControlGUI(Name = "Exchange")]
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "Exch")]
        public string Exch
        {
            set { m_Exch = new EFS_String(value); }
            get { return m_Exch?.Value; }
        }


        /// <summary>
        ///  Status du du traitement EndOFDay pour le marché 
        ///  <para>Performed : EOD Traité => les flux en rapport avec le marché sont considérés par le traitement CashBalance</para>
        ///  <para>Unperformed : EOD non Traité ou en erreur => les flux en rapport avec le marché ne sont pas considérés par le traitement CashBalance</para>
        /// </summary>
        [ControlGUI(Name = "Status")]
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PerformedSatusEnum status;

    }

    /// <summary>
    ///  Rerésente l'état du process EOD vis à vis d'une css\Custodian
    /// </summary>
    /// FI 20161027 [22151] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CssCustodianStatus : ItemGUI
    {

        /// <summary>
        ///  Représente le custodian ou la chambre
        ///  <para>pas de serialization</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int idACssCustodian;

        /// <summary>
        /// Représente une chambre de compensation ou un custodian 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "cssCustodianHref", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string cssCustodianHref;

        /// <summary>
        ///  Status du traitement de fin de journée pour CSS\Custodian
        ///  <para>Performed : EOD Traité => les flux en rapport avec css\Custodian sont considérés par le traitement CashBalance</para>
        ///  <para>Unperformed : EOD non Traité ou en erreur => les flux en rapport avec css\Custodian ne sont pas considérés par le traitement CashBalance</para> 
        /// </summary>
        [ControlGUI(Name = "Status")]
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "status")]
        public PerformedSatusEnum status;

        /// <summary>
        ///  Status du traitement de fin de journée pour chaque marché
        ///  <para>Les status de chaque marché est identique au statut du CSS/Custodian</para>
        /// </summary>
        [ControlGUI(Name = "Exchange Status")]
        [System.Xml.Serialization.XmlElement()]
        public ExchangeStatus[] exchStatus;

    }


}
