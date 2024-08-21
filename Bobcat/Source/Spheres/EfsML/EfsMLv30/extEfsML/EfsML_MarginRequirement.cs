#region using directives
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Shared;
using System;
#endregion using directives

namespace EfsML.v30.MarginRequirement
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20160613 [22256] Modify
    public partial class MarginRequirement : IProduct, IMarginRequirement
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SimplePayment[] efs_SimplePayment;
        #endregion Members
        //
        #region accessor
        
        #endregion
        //
        #region Constructor
        public MarginRequirement()
        {
            this.timing = SettlSessIDEnum.None;
            this.clearingOrganizationPartyReference = new PartyReference();
            this.marginRequirementOfficePartyReference = new PartyReference();
            this.entityPartyReference = new PartyReference();
            this.payment = new SimplePayment[] { new SimplePayment() };
            this.efs_SimplePayment = null;
            this.initialMarginMethodSpecified = false;
            this.isGrossMargin = new EFS_Boolean(false);
        }
        #endregion
        //
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        //
        #region IMarginRequirement Membres
        bool IMarginRequirement.IsGrossMarginSpecified
        {
            get { return this.isGrossMarginSpecified; }
            set { isGrossMarginSpecified = value; }
        }
        EFS_Boolean IMarginRequirement.IsGrossMargin
        {
            get { return this.isGrossMargin; }
            set { isGrossMargin = value; }
        }
        bool IMarginRequirement.InitialMarginMethodSpecified
        { 
            get { return this.initialMarginMethodSpecified; }
            set { initialMarginMethodSpecified = value; }
        }
        // PM 20160404 [22116] initialMarginMethod devient un array
        InitialMarginMethodEnum[] IMarginRequirement.InitialMarginMethod
        {
            get { return this.InitialMarginMethod; }
            set { InitialMarginMethod = value; }
        }
        IReference IMarginRequirement.ClearingOrganizationPartyReference
        {
            get { return (IReference)this.clearingOrganizationPartyReference; }
            set { clearingOrganizationPartyReference = (PartyReference)value; }
        }
        IReference IMarginRequirement.MarginRequirementOfficePartyReference
        {
            get { return (IReference)this.marginRequirementOfficePartyReference ; }
            set { marginRequirementOfficePartyReference = (PartyReference)value; }
        }
        IReference IMarginRequirement.EntityPartyReference 
        {
            get { return (IReference)this.entityPartyReference; }
            set { entityPartyReference = (PartyReference)value; }
        }
        SettlSessIDEnum IMarginRequirement.Timing
        {
            get { return this.timing; }
            set { timing = (SettlSessIDEnum)value; }
        }
        ISimplePayment[] IMarginRequirement.Payment
        {
            get { return this.payment; }
            set { payment = (SimplePayment[])value; }
        }
        EFS_SimplePayment[] IMarginRequirement.Efs_SimplePayment
        {
            get { return this.efs_SimplePayment; }
            set { efs_SimplePayment = (EFS_SimplePayment[])value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        bool IMarginRequirement.UnderlyingStockSpecified
        {
            get { return this.underlyingStockSpecified; }
            set { underlyingStockSpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        UnderlyingStock[] IMarginRequirement.UnderlyingStock
        {
            get { return this.underlyingStock; }
            set { underlyingStock = (UnderlyingStock[] )value; }
        }


        #endregion  IMarginRequirement Members

        /// <summary>
        /// Retourne timing
        /// <para>Utilisé par les évènements</para>
        /// </summary>
        /// <returns></returns>
        public SettlSessIDEnum GetTiming()
        {
            return timing;
        }


    }



    /// <summary>
    /// Actions déposées et utilisées pour réduire les positions ETD Short Future et Short Call Future
    /// </summary>
    public partial class UnderlyingStock : ISpheresId
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public UnderlyingStock()
        {
            equity = new EquityAsset();
        }

        
        #region ISpheresId Members
        string ISpheresId.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion ISpheresId Members

    }

}
