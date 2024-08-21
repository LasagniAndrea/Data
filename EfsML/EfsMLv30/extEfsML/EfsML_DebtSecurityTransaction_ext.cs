#region using directives
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EFS.EFSTools;

using EfsML.Enum;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Shared;
using EfsML.v30.Ird;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives

namespace EfsML.v30.DebtSecurityTransaction
{
    #region DebtSecurityTransaction
    public partial class DebtSecurityTransaction : IProduct, IProductBase, IDebtSecurityTransaction
    {
        #region constructor
        public DebtSecurityTransaction()
        {
            securityAsset = new SecurityAsset(); ;
            quantity = new OrderQuantity();
            price = new OrderPrice();
            grossAmount = new Payment();
        }
        #endregion

        #region IDebtSecurityTransaction Members
        IReference IDebtSecurityTransaction.buyerPartyReference
        {
            set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.buyerPartyReference; }
        }
        IReference IDebtSecurityTransaction.sellerPartyReference
        {
            set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.sellerPartyReference; }
        }
        ISecurityAsset IDebtSecurityTransaction.securityAsset
        {
            set { this.securityAsset = (SecurityAsset)value; }
            get { return this.securityAsset; }
        }

        IOrderQuantity IDebtSecurityTransaction.quantity
        {
            set { this.quantity = (OrderQuantity)value; }
            get { return this.quantity; }
        }

        IOrderPrice IDebtSecurityTransaction.price
        {
            set { this.price = (OrderPrice)value; }
            get { return this.price; }
        }

        IPayment IDebtSecurityTransaction.grossAmount
        {
            set { this.grossAmount = (Payment)value; }
            get { return this.grossAmount; }
        }

        #endregion IDebtSecurityTransaction Members

        #region IProduct Members
        object IProduct.product { get { return this; } }
        IProductBase IProduct.productBase { get { return this; } }
        #endregion IProduct Members
    }
    #endregion DebtSecurityTransaction

    #region SecurityAsset
    public partial class SecurityAsset : ISecurityAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool issuerSpecified;
        /// <summary>
        /// Contient la partie qui représente l'émetteur, issu du datadocument debtSecurity (saisie du référentiel Titre)
        /// l'émetteur est le payer du flux d'intérêts, l'acteur SYSTEM étant le receiver
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Party issuer;
        #endregion
        //
        #region constructor
        public SecurityAsset()
        {
            this.securityId = new EFS_String();
            this.securityName = new EFS_String();
            this.securityDescription = new EFS_String();
            this.securityIssueDate = new EFS_Date();
            this.debtSecurity = new DebtSecurity.DebtSecurity();
        }
        #endregion
        //
        #region ISecurityAsset
        EFS_String ISecurityAsset.securityId
        {
            set { this.securityId = (EFS_String)value; }
            get { return this.securityId; }
        }
        //
        bool ISecurityAsset.securityNameSpecified
        {
            get { return this.securityNameSpecified; }
            set { this.securityNameSpecified = value; }
        }
        EFS_String ISecurityAsset.securityName
        {
            set { this.securityName = (EFS_String)value; }
            get { return this.securityName; }
        }
        //
        bool ISecurityAsset.securityDescriptionSpecified
        {
            get { return this.securityDescriptionSpecified; }
            set { this.securityDescriptionSpecified = value; }
        }
        EFS_String ISecurityAsset.securityDescription
        {
            set { this.securityDescription = (EFS_String)value; }
            get { return this.securityDescription; }
        }
        //
        bool ISecurityAsset.securityIssueDateSpecified
        {
            get { return this.securityIssueDateSpecified; }
            set { this.securityIssueDateSpecified = value; }
        }
        EFS_Date ISecurityAsset.securityIssueDate
        {
            set { this.securityIssueDate = (EFS_Date)value; }
            get { return this.securityIssueDate; }
        }
        //
        bool ISecurityAsset.debtSecuritySpecified
        {
            get { return this.debtSecuritySpecified; }
            set { this.debtSecuritySpecified = value; }
        }
        IDebtSecurity ISecurityAsset.debtSecurity
        {
            get { return this.debtSecurity; }
            set { this.debtSecurity = (DebtSecurity.DebtSecurity)value; }
        }
        //
        bool ISecurityAsset.issuerSpecified
        {
            get { return this.issuerSpecified; }
            set { this.issuerSpecified = value; }
        }
        IParty ISecurityAsset.issuer
        {
            get { return this.issuer; }
            set { this.issuer = (Party)value; }
        }
        IMoney ISecurityAsset.GetNominal()
        {
            Money ret =null; 
            //
            if (debtSecuritySpecified)
            {
                ret = new Money(); 
                //
                CalculationPeriodAmountBase  calculationPeriodAmount = debtSecurity.Stream[0].calculationPeriodAmount;
                if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    Notional notional = calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional;
                    ret.amount = notional.notionalStepSchedule.initialValue;
                    ret.currency = notional.notionalStepSchedule.currency;
                }
                else if (calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                {
                    KnownAmountSchedule knownAmountSchedule = calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule;
                    ret.amount = knownAmountSchedule.initialValue;
                    ret.currency = knownAmountSchedule.currency;
                }
                else if ((debtSecurity.security.faceAmountSpecified) && (debtSecurity.security.numberOfIssuedSecuritiesSpecified) && (debtSecurity.security.numberOfIssuedSecurities.DecValue > 0))
                {
                    ret.amount = new EFS_Decimal(debtSecurity.security.faceAmount.amount.DecValue / debtSecurity.security.numberOfIssuedSecurities.DecValue);
                    ret.currency = debtSecurity.security.faceAmount.currency;
                }
                else if (debtSecurity.security.currencySpecified)
                {
                    ret.amount = new EFS_Decimal(0);
                    ret.currency = debtSecurity.security.faceAmount.currency;
                }
            }
            //
            return ret;
        }
        #endregion
    }
    #endregion


    #region OrderPrice
    public partial class OrderPrice : IOrderPrice
    {
        #region constructor
        public OrderPrice()
        {
            this.priceUnits = new PriceQuoteUnits();

        }
        #endregion

        #region IOrderPrice Membres

        IScheme IOrderPrice.priceUnits
        {
            get { return this.priceUnits; }
            set { this.priceUnits = (PriceQuoteUnits)value; }
        }

        bool IOrderPrice.cleanPriceSpecified
        {
            get { return this.cleanPriceSpecified; }
            set { this.cleanPriceSpecified = value; }
        }

        EFS_Decimal IOrderPrice.cleanPrice
        {
            get { return this.cleanPrice; }
            set { this.cleanPrice = value; }
        }
        bool IOrderPrice.dirtyPriceSpecified
        {
            get { return this.dirtyPriceSpecified; }
            set { this.dirtyPriceSpecified = value; }
        }
        EFS_Decimal IOrderPrice.dirtyPrice
        {
            get { return this.dirtyPrice; }
            set { this.dirtyPrice = value; }
        }


        bool IOrderPrice.accruedInterestRateSpecified
        {
            get { return this.accruedInterestRateSpecified; }
            set { this.accruedInterestRateSpecified = value; }
        }

        EFS_Decimal IOrderPrice.accruedInterestRate
        {
            get { return this.accruedInterestRate; }
            set { this.accruedInterestRate = value; }
        }

        bool IOrderPrice.accruedInterestAmountSpecified
        {
            get { return this.accruedInterestAmountSpecified; }
            set { this.accruedInterestAmountSpecified = value; }
        }

        IMoney IOrderPrice.accruedInterestAmount
        {
            get { return this.accruedInterestAmount; }
            set { this.accruedInterestAmount = (Money)value; }
        }

        #endregion
    }
    #endregion


    #region OrderQuantity
    public partial class OrderQuantity :  IOrderQuantity
    {
        #region constructor
        public OrderQuantity()
        {
            this.notionalAmount = new Money(); 
        }
        #endregion constructor

        #region IOrderQuantity Membres
        OrderQuantityType3CodeEnum IOrderQuantity.quantity
        {
            get { return this.quantityType; }
            set { quantityType = value; }
        }
        bool IOrderQuantity.numberOfUnitsSpecified
        {
            get { return this.numberOfUnitsSpecified; }
            set { this.numberOfUnitsSpecified = value; }
        }
        EFS_Decimal IOrderQuantity.numberOfUnits
        {
            get { return this.numberOfUnits; }
            set { this.numberOfUnits = value; }
        }
        IMoney IOrderQuantity.notionalAmount
        {
            get { return this.notionalAmount; }
            set { this.notionalAmount = (Money)value; }
        }
        #endregion
    }
    #endregion OrderQuantity

}



