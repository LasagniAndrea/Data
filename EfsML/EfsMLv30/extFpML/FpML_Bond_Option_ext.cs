#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using EfsML.v30.Security.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.BondOption
{
    #region BondOption
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class BondOption : IProduct, IDeclarativeProvision, IDebtSecurityOption
	{
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:BondOption";
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_BondOption efs_BondOption;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_BondOptionPremium efs_BondOptionPremium;
        #endregion Members


        #region Accessors
        #region DebtSecurity
        public DebtSecurity DebtSecurity
        {
            get
            {
                DebtSecurity debtSecurity = null;
                if (securityAssetSpecified)
                {
                    if ((null != securityAsset) && securityAsset.debtSecuritySpecified)
                        debtSecurity = securityAsset.debtSecurity;
                }
                return debtSecurity;
            }
        }
        #endregion DebtSecurity
        // EG 20150608 [21091] New
        private bool ParValueSpecified
        {
            get
            {
                bool parValueSpecified = false;
                if (this.underlyerBondSpecified)
                    parValueSpecified = this.underlyerBond.parValueSpecified;
                else if (this.underlyerConvertibleBondSpecified)
                    parValueSpecified = this.underlyerConvertibleBond.parValueSpecified;
                return parValueSpecified;
            }
        }
        // EG 20150608 [21091] New
        private EFS_Decimal ParValue
        {
            get
            {
                EFS_Decimal parValue = null;
                if (ParValueSpecified)
                {
                    if (this.underlyerBondSpecified)
                        parValue = this.underlyerBond.parValue;
                    else if (this.underlyerConvertibleBondSpecified)
                        parValue = this.underlyerConvertibleBond.parValue;
                }
                return parValue;
            }
        }
        #endregion Accessors

		#region Constructors
        /// EG 20150422 [20513] BANCAPERTA New 
		public BondOption()
		{
            underlyerBond = new Bond();
            underlyerConvertibleBond = new ConvertibleBond();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified {get {return false;}}
		ICancelableProvision IDeclarativeProvision.CancelableProvision {get { return null;}}
		bool IDeclarativeProvision.ExtendibleProvisionSpecified {get { return false;}}
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision {get { return null;}}
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified {get { return false;}}
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision {get { return null;}}
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision {get { return null;} }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members

        #region IDebtSecurityOption Members
        IReference IDebtSecurityOption.IssuerPartyReference
        {
            get
            {
                IReference reference = null;
                DebtSecurity debtSecurity = DebtSecurity;
                if (null != debtSecurity)
                {

                    if ((null != debtSecurity) && ArrFunc.IsFilled(debtSecurity.Stream))
                        reference = DebtSecurity.Stream[0].payerPartyReference;
                }
                return reference;
            }
        }
        bool IDebtSecurityOption.SecurityAssetSpecified
        {
            set { this.securityAssetSpecified = value; }
            get { return this.securityAssetSpecified; }
        }
        ISecurityAsset IDebtSecurityOption.SecurityAsset
        {
            set { this.securityAsset = (SecurityAsset)value; }
            get { return this.securityAsset; }
        }
        IDebtSecurity IDebtSecurityOption.DebtSecurity
        {
            get { return DebtSecurity; }
        }
        int IDebtSecurityOption.SecurityAssetOTCmlId
        {
            get
            {
                int OTCmlId = 0;
                if (null != securityAsset)
                    OTCmlId = securityAsset.OTCmlId;
                return OTCmlId;
            }
        }
        EFS_Asset IDebtSecurityOption.Efs_Asset(string pCS)
        {
            EFS_Asset _efs_Asset = null;
            int _id = ((IDebtSecurityOption)this).SecurityAssetOTCmlId;
            if (0 < _id)
            {
                SQL_AssetDebtSecurity sql_AssetDebtSecurity = new SQL_AssetDebtSecurity(CSTools.SetCacheOn(pCS), _id);
                if (sql_AssetDebtSecurity.IsLoaded)
                {
                    _efs_Asset = new EFS_Asset
                    {
                        idAsset = sql_AssetDebtSecurity.Id,
                        description = sql_AssetDebtSecurity.Description,
                        IdMarket = sql_AssetDebtSecurity.IdM,
                        IdMarketFIXML_SecurityExchange = sql_AssetDebtSecurity.Market_FIXML_SecurityExchange,
                        IdMarketIdentifier = sql_AssetDebtSecurity.Market_Identifier,
                        IdMarketISO10383_ALPHA4 = sql_AssetDebtSecurity.Market_ISO10383_ALPHA4,
                        assetCategory = sql_AssetDebtSecurity.AssetCategory.Value
                    };

                    string schemeId = EnumTools.GenerateScheme(pCS, Cst.OTCmL_SecurityIdSourceScheme, "SecurityIDSourceEnum", "ISIN", true);
                    DebtSecurity _debtSecurity = DebtSecurity;
                    if ((null != _debtSecurity.security) && (null != _debtSecurity.security.instrumentId))
                    {
                        IScheme scheme = Tools.GetScheme((IScheme[])_debtSecurity.security.instrumentId, schemeId);
                        if (null != scheme)
                            _efs_Asset.isinCode = scheme.Value;
                    }
                }
            }
            return _efs_Asset;
        }
        /// EG 20150422 [20513] BANCAPERTA New 
        EFS_BondOption IDebtSecurityOption.Efs_BondOption
        {
            set { this.efs_BondOption = value; }
            get { return this.efs_BondOption; }
        }
        /// EG 20150422 [20513] BANCAPERTA New 
        EFS_BondOptionPremium IDebtSecurityOption.Efs_BondOptionPremium
        {
            set { this.efs_BondOptionPremium = value; }
            get { return this.efs_BondOptionPremium; }
        }
        #endregion IDebtSecurityOption Members


        #region IBondOption Membres

        IBondOptionStrike IBondOption.Strike
        {
            set { this.strike = (BondOptionStrike)value; }
            get { return this.strike; }
        }
        /// EG 20150422 [20513] BANCAPERTA Mod 
        bool IBondOption.BondSpecified
        {
            set { this.underlyerBondSpecified = value; }
            get { return this.underlyerBondSpecified; }
        }
        /// EG 20150422 [20513] BANCAPERTA Mod 
        IBond IBondOption.Bond
        {
            set { this.underlyerBond = (Bond)value; }
            get { return this.underlyerBond; }
        }

        bool IBondOption.ConvertibleBondSpecified
        {
            set { this.underlyerConvertibleBondSpecified = value; }
            get { return this.underlyerConvertibleBondSpecified; }
        }

        IConvertibleBond IBondOption.ConvertibleBond
        {
            set { this.underlyerConvertibleBond = (ConvertibleBond)value; }
            get { return this.underlyerConvertibleBond; }
        }
        // EG 20150608 [21091] New
        bool IBondOption.ParValueSpecified
        {
            get {return this.ParValueSpecified; }
        }
        // EG 20150608 [21091] New
        EFS_Decimal IBondOption.ParValue
        {
            get {return this.ParValue;}
        }
        #endregion IBondOption membres
    }
	#endregion BondOption
    #region BondOptionStrike
    public partial class BondOptionStrike : IBondOptionStrike
    {
        #region Constructors
        public BondOptionStrike()
        {
            typeStrikePrice = new OptionStrike();
            typeStrikeReferenceSwapCurve = new ReferenceSwapCurve();
        }
        #endregion Constructors
        #region IBondOptionStrike Membres
        bool IBondOptionStrike.ReferenceSwapCurveSpecified
        {
            set { this.typeStrikeReferenceSwapCurveSpecified = value; }
            get { return this.typeStrikeReferenceSwapCurveSpecified; }
        }
        IReferenceSwapCurve IBondOptionStrike.ReferenceSwapCurve
        {
            set { this.typeStrikeReferenceSwapCurve = (ReferenceSwapCurve)value; }
            get { return this.typeStrikeReferenceSwapCurve; }
        }
        bool IBondOptionStrike.PriceSpecified
        {
            set { this.typeStrikePriceSpecified = value; }
            get { return this.typeStrikePriceSpecified; }
        }
        IOptionStrike IBondOptionStrike.Price
        {
            set { this.typeStrikePrice = (OptionStrike)value; }
            get { return this.typeStrikePrice; }
        }
        #endregion
    }
    #endregion BondOptionStrike

    #region MakeWholeAmount
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class MakeWholeAmount : IMakeWholeAmount
    {
		#region Constructors
        /// EG 20150422 [20513] BANCAPERTA New 
        public MakeWholeAmount()
		{
            this.earlyCallDate = new IdentifiedDate();
            this.floatingRateIndex = new FloatingRateIndex();
            this.indexTenor = new Interval();
            this.interpolationMethod = new InterpolationMethod();
            this.spread = new EFS_Decimal();
		}
		#endregion Constructors

        #region IMakeWholeAmount Membres
        bool IMakeWholeAmount.InterpolationMethodSpecified
        {
            set { this.interpolationMethodSpecified = value; }
            get { return this.interpolationMethodSpecified; }
        }
        IScheme IMakeWholeAmount.InterpolationMethod
        {
            set { this.interpolationMethod = (InterpolationMethod) value; }
            get { return this.interpolationMethod; }
        }
        IAdjustedDate IMakeWholeAmount.EarlyCallDate
        {
            set { this.earlyCallDate = (IdentifiedDate)value; }
            get { return this.earlyCallDate; }
        }
        #endregion IMakeWholeAmount Membres
    }
    #endregion MakeWholeAmount

    #region ReferenceSwapCurve
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class ReferenceSwapCurve : IReferenceSwapCurve
    {
		#region Constructors
        /// EG 20150422 [20513] BANCAPERTA New 
        public ReferenceSwapCurve()
		{
            swapUnwindValue = new SwapCurveValuation();
            makeWholeAmount = new MakeWholeAmount();
		}
		#endregion Constructors

        #region IReferenceSwapCurve Membres
        ISwapCurveValuation IReferenceSwapCurve.SwapUnwindValue
        {
            set { this.swapUnwindValue = (SwapCurveValuation)value; }
            get { return this.swapUnwindValue; }
        }
        bool IReferenceSwapCurve.MakeWholeAmountSpecified
        {
            set { this.makeWholeAmountSpecified = value; }
            get { return this.makeWholeAmountSpecified; }
        }
        IMakeWholeAmount IReferenceSwapCurve.MakeWholeAmount
        {
            set { this.makeWholeAmount = (MakeWholeAmount)value; }
            get { return this.makeWholeAmount; }
        }
        #endregion
    }
    #endregion ReferenceSwapCurve
    #region SwapCurveValuation
    /// EG 20150422 [20513] BANCAPERTA New 
    public partial class SwapCurveValuation : ISwapCurveValuation
    {
		#region Constructors
        /// EG 20150422 [20513] BANCAPERTA New 
        public SwapCurveValuation()
		{
            this.floatingRateIndex = new FloatingRateIndex();
            this.indexTenor = new Interval();
            this.spread = new EFS_Decimal();
		}
		#endregion Constructors

        #region ISwapCurveValuation Membres
        IFloatingRateIndex ISwapCurveValuation.FloatingRateIndex
        {
            set { this.floatingRateIndex = (FloatingRateIndex)value; }
            get { return this.floatingRateIndex; }
        }
        bool ISwapCurveValuation.IndexTenorSpecified
        {
            set { this.indexTenorSpecified = value; }
            get { return this.indexTenorSpecified; }
        }
        IInterval ISwapCurveValuation.IndexTenor
        {
            set { this.indexTenor = (Interval)value; }
            get { return this.indexTenor; }
        }
        EFS_Decimal ISwapCurveValuation.Spread
        {
            set { this.spread = value; }
            get { return this.spread; }
        }
        bool ISwapCurveValuation.SideSpecified
        {
            set { this.sideSpecified = value; }
            get { return this.sideSpecified; }
        }
        QuotationSideEnum ISwapCurveValuation.Side
        {
            set { this.side = value; }
            get { return this.side; }
        }
        #endregion ISwapCurveValuation membres
    }
    #endregion SwapCurveValuation

}
