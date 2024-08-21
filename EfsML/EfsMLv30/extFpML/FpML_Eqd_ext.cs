#region using directives
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Eq.Shared;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.Eqd
{
    #region BrokerEquityOption
    public partial class BrokerEquityOption : IProduct,IDeclarativeProvision
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion BrokerEquityOption

	#region EquityAmericanExercise
	public partial class EquityAmericanExercise : IEquityAmericanExercise
    {
        #region IEquityId Members
        string IExerciseId.Id
        {
			set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion IEquityId Members

        #region IEquityExercise Members
        IAdjustableOrRelativeDate IEquityExercise.ExpirationDate
        {
            set { this.expirationDate = (AdjustableOrRelativeDate)value; }
            get { return this.expirationDate; }
        }
        TimeTypeEnum IEquityExercise.EquityExpirationTimeType
        {
            set { this.equityExpirationTimeType = value; }
            get { return this.equityExpirationTimeType; }
        }
        bool IEquityExercise.EquityExpirationTimeSpecified
        {
            set { this.equityExpirationTimeSpecified = value; }
            get { return this.equityExpirationTimeSpecified; }
        }
        IBusinessCenterTime IEquityExercise.EquityExpirationTime
        {
            set { this.equityExpirationTime = (BusinessCenterTime)value; }
            get { return this.equityExpirationTime; }
        }
        #endregion IEquityExercise Members

        #region ISharedAmericanExercise Members
        IAdjustableOrRelativeDate ISharedAmericanExercise.CommencementDate
        {
            set { this.commencementDate = (AdjustableOrRelativeDate)value; }
            get { return this.commencementDate; }
        }
        IAdjustableOrRelativeDate ISharedAmericanExercise.ExpirationDate
        {
            set { this.expirationDate = (AdjustableOrRelativeDate)value; }
            get { return this.expirationDate; }
        }
        bool ISharedAmericanExercise.LatestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime ISharedAmericanExercise.LatestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        #endregion

        #region IEquitySharedAmericanExercise Members
        bool IEquitySharedAmericanExercise.LatestExerciseTimeTypeSpecified
		{
			set { this.latestExerciseTimeTypeSpecified = value; }
			get { return this.latestExerciseTimeTypeSpecified; }
		}
        TimeTypeEnum IEquitySharedAmericanExercise.LatestExerciseTimeType
		{
			set { this.latestExerciseTimeType = value; }
			get { return this.latestExerciseTimeType; }
        }
        #endregion

        #region IEquityAmericanExercise
        bool IEquityAmericanExercise.EquityMultipleExerciseSpecified
		{
			set { this.equityMultipleExerciseSpecified = value; }
			get { return this.equityMultipleExerciseSpecified; }
		}
		IEquityMultipleExercise IEquityAmericanExercise.EquityMultipleExercise
		{
			set { this.equityMultipleExercise = (EquityMultipleExercise)value; }
			get { return this.equityMultipleExercise; }
		}
		#endregion IEquityAmericanExercise Members
        
    }
	#endregion EquityAmericanExercise
	#region EquityBermudanExercise
    public partial class EquityBermudaExercise : IEquityBermudaExercise
    {

        #region IEquityId Members
        string IExerciseId.Id
        {
			set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion IEquityId Members

        #region IEquityExercise Members
        IAdjustableOrRelativeDate IEquityExercise.ExpirationDate
        {
            set { this.expirationDate = (AdjustableOrRelativeDate)value; }
            get { return this.expirationDate; }
        }
        TimeTypeEnum IEquityExercise.EquityExpirationTimeType
        {
            set { this.equityExpirationTimeType = value; }
            get { return this.equityExpirationTimeType; }
        }
        bool IEquityExercise.EquityExpirationTimeSpecified
        {
            set { this.equityExpirationTimeSpecified = value; }
            get { return this.equityExpirationTimeSpecified; }
        }
        IBusinessCenterTime IEquityExercise.EquityExpirationTime
        {
            set { this.equityExpirationTime = (BusinessCenterTime)value; }
            get { return this.equityExpirationTime; }
        }
        #endregion IEquityExercise Members

        #region ISharedAmericanExercise Members
        IAdjustableOrRelativeDate ISharedAmericanExercise.CommencementDate
        {
            set { this.commencementDate = (AdjustableOrRelativeDate)value; }
            get { return this.commencementDate; }
        }
        IAdjustableOrRelativeDate ISharedAmericanExercise.ExpirationDate
        {
            set { this.expirationDate = (AdjustableOrRelativeDate)value; }
            get { return this.expirationDate; }
        }
        bool ISharedAmericanExercise.LatestExerciseTimeSpecified
        {
            set { this.latestExerciseTimeSpecified = value; }
            get { return this.latestExerciseTimeSpecified; }
        }
        IBusinessCenterTime ISharedAmericanExercise.LatestExerciseTime
        {
            set { this.latestExerciseTime = (BusinessCenterTime)value; }
            get { return this.latestExerciseTime; }
        }
        #endregion

        #region IEquitySharedAmericanExercise Members
        bool IEquitySharedAmericanExercise.LatestExerciseTimeTypeSpecified
        {
            set { this.latestExerciseTimeTypeSpecified = value; }
            get { return this.latestExerciseTimeTypeSpecified; }
        }
        TimeTypeEnum IEquitySharedAmericanExercise.LatestExerciseTimeType
        {
            set { this.latestExerciseTimeType = value; }
            get { return this.latestExerciseTimeType; }
        }
        #endregion

        #region IEquityBermudaExercise Members
        IDateList IEquityBermudaExercise.BermudaExerciseDates
        {
            set { this.bermudaExerciseDates = (DateList)value; }
            get { return this.bermudaExerciseDates; }
        }
        bool IEquityBermudaExercise.EquityMultipleExerciseSpecified
        {
            set { this.equityMultipleExerciseSpecified = value; }
            get { return this.equityMultipleExerciseSpecified; }
        }
        IEquityMultipleExercise IEquityBermudaExercise.EquityMultipleExercise
        {
            set { this.equityMultipleExercise = (EquityMultipleExercise)value; }
            get { return this.equityMultipleExercise; }
        }
        #endregion IEquityBermudaExercise Members

    }
	#endregion EquityBermudanExercise
	#region EquityDerivativeBase
	public partial class EquityDerivativeBase : IProduct, IEquityDerivativeBase,IDeclarativeProvision
	{
		#region Constructors
        public EquityDerivativeBase()
        {
            equityEffectiveDate = new EFS_Date();
            equityExercise = new EquityExerciseValuationSettlement();
        }
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IEquityDerivativeBase Members
		IReference IEquityDerivativeBase.BuyerPartyReference
		{
			set {this.buyerPartyReference = (PartyOrTradeSideReference)value;}
			get {return this.buyerPartyReference;}
		}
		IReference IEquityDerivativeBase.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		OptionTypeEnum IEquityDerivativeBase.OptionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IEquityDerivativeBase.EquityEffectiveDateSpecified
		{
			set { this.equityEffectiveDateSpecified = value; }
			get { return this.equityEffectiveDateSpecified; }
		}
		EFS_Date IEquityDerivativeBase.EquityEffectiveDate
		{
			set { this.equityEffectiveDate = value; }
			get { return this.equityEffectiveDate; }
		}
		IUnderlyer IEquityDerivativeBase.Underlyer
		{
			set { this.underlyer = (Underlyer)value; }
			get { return this.underlyer; }
		}
		bool IEquityDerivativeBase.NotionalSpecified
		{
			set { this.notionalSpecified = value; }
			get { return this.notionalSpecified; }
		}
		IMoney IEquityDerivativeBase.Notional
		{
			set { this.notional =(Money) value; }
			get { return this.notional; }
		}
		IEquityExerciseValuationSettlement IEquityDerivativeBase.EquityExercise
		{
			set { this.equityExercise = (EquityExerciseValuationSettlement)value; }
			get { return this.equityExercise; }
		}
		bool IEquityDerivativeBase.FeatureSpecified
		{
			set { this.featureSpecified = value; }
			get { return this.featureSpecified; }
		}
		IOptionFeatures IEquityDerivativeBase.Feature
		{
			set { this.feature = (OptionFeatures) value; }
			get { return this.feature; }
		}
		bool IEquityDerivativeBase.FxFeatureSpecified
		{
			set { this.fxFeatureSpecified = value; }
			get { return this.fxFeatureSpecified; }
		}
		IFxFeature IEquityDerivativeBase.FxFeature
		{
			set { this.fxFeature =(FxFeature) value; }
			get { return this.fxFeature; }
		}
		bool IEquityDerivativeBase.StrategyFeatureSpecified
		{
			set { this.strategyFeatureSpecified = value; }
			get { return this.strategyFeatureSpecified; }
		}
		IStrategyFeature IEquityDerivativeBase.StrategyFeature
		{
			set { this.strategyFeature = (StrategyFeature)value; }
			get { return this.strategyFeature; }
		}
        IOptionFeatures IEquityDerivativeBase.CreateOptionFeatures
        {
            get { return new OptionFeatures(); }
        }
        IFxFeature IEquityDerivativeBase.CreateFxFeature
        {
            get { return new FxFeature(); }
        }
        #endregion IEquityDerivativeBase Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquityDerivativeBase
	#region EquityDerivativeLongFormBase
	public partial class EquityDerivativeLongFormBase : IProduct, IEquityDerivativeLongFormBase,IDeclarativeProvision
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

		#region IEquityDerivativeLongFormBase Members
		bool IEquityDerivativeLongFormBase.DividendConditionsSpecified
		{
			set {dividendConditionsSpecified = value; }
			get {return this.dividendConditionsSpecified; }
		}
		IDividendConditions IEquityDerivativeLongFormBase.DividendConditions
		{
			set { dividendConditions = (DividendConditions)value; }
			get { return this.dividendConditions; }
		}
		MethodOfAdjustmentEnum IEquityDerivativeLongFormBase.MethodOfAdjustment
		{
			set { methodOfAdjustment = value; }
			get { return this.methodOfAdjustment; }
		}
		IExtraordinaryEvents IEquityDerivativeLongFormBase.ExtraordinaryEvents
		{
			set { extraordinaryEvents = (ExtraordinaryEvents)value; }
			get { return this.extraordinaryEvents; }
		}
		#endregion IEquityDerivativeLongFormBase Members
		#region IEquityDerivativeBase Members
		IReference IEquityDerivativeBase.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IEquityDerivativeBase.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		OptionTypeEnum IEquityDerivativeBase.OptionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IEquityDerivativeBase.EquityEffectiveDateSpecified
		{
			set { this.equityEffectiveDateSpecified = value; }
			get { return this.equityEffectiveDateSpecified; }
		}
		EFS_Date IEquityDerivativeBase.EquityEffectiveDate
		{
			set { this.equityEffectiveDate = value; }
			get { return this.equityEffectiveDate; }
		}
		IUnderlyer IEquityDerivativeBase.Underlyer
		{
			set { this.underlyer = (Underlyer)value; }
			get { return this.underlyer; }
		}
		bool IEquityDerivativeBase.NotionalSpecified
		{
			set { this.notionalSpecified = value; }
			get { return this.notionalSpecified; }
		}
		IMoney IEquityDerivativeBase.Notional
		{
			set { this.notional = (Money)value; }
			get { return this.notional; }
		}
		IEquityExerciseValuationSettlement IEquityDerivativeBase.EquityExercise
		{
			set { this.equityExercise = (EquityExerciseValuationSettlement)value; }
			get { return this.equityExercise; }
		}
		bool IEquityDerivativeBase.FeatureSpecified
		{
			set { this.featureSpecified = value; }
			get { return this.featureSpecified; }
		}
		IOptionFeatures IEquityDerivativeBase.Feature
		{
			set { this.feature = (OptionFeatures)value; }
			get { return this.feature; }
		}
		bool IEquityDerivativeBase.FxFeatureSpecified
		{
			set { this.fxFeatureSpecified = value; }
			get { return this.fxFeatureSpecified; }
		}
		IFxFeature IEquityDerivativeBase.FxFeature
		{
			set { this.fxFeature = (FxFeature)value; }
			get { return this.fxFeature; }
		}
		bool IEquityDerivativeBase.StrategyFeatureSpecified
		{
			set { this.strategyFeatureSpecified = value; }
			get { return this.strategyFeatureSpecified; }
		}
		IStrategyFeature IEquityDerivativeBase.StrategyFeature
		{
			set { this.strategyFeature = (StrategyFeature)value; }
			get { return this.strategyFeature; }
		}
		#endregion IEquityDerivativeBase Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquityDerivativeLongFormBase
	#region EquityDerivativeShortFormBase
	public partial class EquityDerivativeShortFormBase : IProduct,IDeclarativeProvision
	{
		#region Constructors
		public EquityDerivativeShortFormBase()
		{
			spotPrice = new EFS_Decimal();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquityDerivativeShortFormBase
	#region EquityEuropeanExercise
	public partial class EquityEuropeanExercise : IEquityEuropeanExercise
    {
        #region IEquityExercise Members
        IAdjustableOrRelativeDate IEquityExercise.ExpirationDate
		{
			set { this.expirationDate = (AdjustableOrRelativeDate)value; }
			get { return this.expirationDate; }
		}
        TimeTypeEnum IEquityExercise.EquityExpirationTimeType
		{
			set { this.equityExpirationTimeType = value; }
			get { return this.equityExpirationTimeType; }
		}
        bool IEquityExercise.EquityExpirationTimeSpecified
		{
			set { this.equityExpirationTimeSpecified = value; }
			get { return this.equityExpirationTimeSpecified; }
		}
        IBusinessCenterTime IEquityExercise.EquityExpirationTime
		{
			set { this.equityExpirationTime = (BusinessCenterTime)value; }
			get { return this.equityExpirationTime; }
        }
        #endregion IEquityExercise Members
        #region IExerciseId Members
        string IExerciseId.Id
		{
			set { this.Id = value; }
			get { return this.Id; }
		}
		#endregion IEquityExercise Members
	}
	#endregion EquityEuropeanExercise
	#region EquityExerciseValuationSettlement
	public partial class EquityExerciseValuationSettlement : IEquityExerciseValuationSettlement
	{
		#region Constructors
        public EquityExerciseValuationSettlement()
        {
            equityExerciseAmerican = new EquityAmericanExercise();
            equityExerciseBermuda = new EquityBermudaExercise();
            equityExerciseEuropean = new EquityEuropeanExercise();
            exerciseMethodAutomaticExercise = new EFS_Boolean();
            exerciseMethodPrePayment = new PrePayment();
            exerciseMethodMakeWholeProvisions = new MakeWholeProvisions();
            settlementCurrency = new Currency();
        }
		#endregion Constructors

		#region IEquityExerciseValuationSettlement Members
		bool IEquityExerciseValuationSettlement.EquityExerciseAmericanSpecified
		{
			set { this.equityExerciseAmericanSpecified = value; }
			get { return this.equityExerciseAmericanSpecified; }
		}
		IEquityAmericanExercise IEquityExerciseValuationSettlement.EquityExerciseAmerican
		{
			set { this.equityExerciseAmerican = (EquityAmericanExercise)value; }
			get { return this.equityExerciseAmerican; }
		}
		bool IEquityExerciseValuationSettlement.EquityExerciseBermudaSpecified
		{
			set { this.equityExerciseBermudaSpecified = value; }
			get { return this.equityExerciseBermudaSpecified; }
		}
		IEquityBermudaExercise IEquityExerciseValuationSettlement.EquityExerciseBermuda
		{
			set { this.equityExerciseBermuda = (EquityBermudaExercise)value; }
			get { return this.equityExerciseBermuda; }
		}
		bool IEquityExerciseValuationSettlement.EquityExerciseEuropeanSpecified
		{
			set { this.equityExerciseEuropeanSpecified = value; }
			get { return this.equityExerciseEuropeanSpecified; }
		}
		IEquityEuropeanExercise IEquityExerciseValuationSettlement.EquityExerciseEuropean
		{
			set { this.equityExerciseEuropean = (EquityEuropeanExercise)value; }
			get { return this.equityExerciseEuropean; }
		}
		bool IEquityExerciseValuationSettlement.AutomaticExerciseSpecified
		{
			set { this.exerciseMethodAutomaticExerciseSpecified = value; }
			get { return this.exerciseMethodAutomaticExerciseSpecified; }
		}
		bool IEquityExerciseValuationSettlement.AutomaticExercise
		{
			set { this.exerciseMethodAutomaticExercise = new EFS_Boolean(value); }
			get { return this.exerciseMethodAutomaticExercise.BoolValue; }
		}
		bool IEquityExerciseValuationSettlement.PrePaymentSpecified
		{
			set { this.exerciseMethodPrePaymentSpecified = value; }
			get { return this.exerciseMethodPrePaymentSpecified; }
		}
		IPrePayment IEquityExerciseValuationSettlement.PrePayment
		{
			set { this.exerciseMethodPrePayment = (PrePayment)value; }
			get { return this.exerciseMethodPrePayment; }
		}
		bool IEquityExerciseValuationSettlement.MakeWholeProvisionsSpecified
		{
			set { this.exerciseMethodMakeWholeProvisionsSpecified = value; }
			get { return this.exerciseMethodMakeWholeProvisionsSpecified; }
		}
		IMakeWholeProvisions IEquityExerciseValuationSettlement.MakeWholeProvisions
		{
			set { this.exerciseMethodMakeWholeProvisions = (MakeWholeProvisions)value; }
			get { return this.exerciseMethodMakeWholeProvisions; }
		}
		IEquityValuation IEquityExerciseValuationSettlement.EquityValuation
		{
			set { this.equityValuation = (EquityValuation)value; }
			get { return this.equityValuation; }
		}
		bool IEquityExerciseValuationSettlement.SettlementDateSpecified
		{
			set { this.settlementDateSpecified = value; }
			get { return this.settlementDateSpecified; }
		}
		IAdjustableOrRelativeDate IEquityExerciseValuationSettlement.SettlementDate
		{
			set { this.settlementDate = (AdjustableOrRelativeDate)value; }
			get { return this.settlementDate; }
		}
		ICurrency IEquityExerciseValuationSettlement.SettlementCurrency
		{
			set { this.settlementCurrency = (Currency)value; }
			get { return this.settlementCurrency; }
		}
		bool IEquityExerciseValuationSettlement.SettlementPriceSourceSpecified
		{
			set { this.settlementPriceSourceSpecified = value; }
			get { return this.settlementPriceSourceSpecified; }
		}
		IScheme IEquityExerciseValuationSettlement.SettlementPriceSource
		{
			set { this.settlementPriceSource = (SettlementPriceSource)value; }
			get { return this.settlementPriceSource; }
		}
		SettlementTypeEnum IEquityExerciseValuationSettlement.SettlementType
		{
			set { this.settlementType = value; }
			get { return this.settlementType; }
		}
		bool IEquityExerciseValuationSettlement.ElectionDateSpecified
		{
			set { this.settlementMethodElectionDateSpecified = value; }
			get { return this.settlementMethodElectionDateSpecified; }
		}
		IAdjustableOrRelativeDate IEquityExerciseValuationSettlement.ElectionDate
		{
			set { this.settlementMethodElectionDate = (AdjustableOrRelativeDate)value; }
			get { return this.settlementMethodElectionDate; }
		}
		bool IEquityExerciseValuationSettlement.ElectingPartyReferenceSpecified
		{
			set { this.settlementMethodElectingPartyReferenceSpecified = value; }
			get { return this.settlementMethodElectingPartyReferenceSpecified; }
		}
		IReference IEquityExerciseValuationSettlement.ElectingPartyReference
		{
			set { this.settlementMethodElectingPartyReference = (PartyReference)value; }
			get { return this.settlementMethodElectingPartyReference; }
		}
		ExerciseStyleEnum IEquityExerciseValuationSettlement.GetStyle
		{
			get
			{
				if (this.equityExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.equityExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;
				return ExerciseStyleEnum.European;
			}
		}
		#endregion IEquityExerciseValuationSettlement Members

	}
	#endregion EquityExerciseValuationSettlement
	#region EquityForward
	public partial class EquityForward : IProduct,IDeclarativeProvision
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquityForward
	#region EquityMultipleExercise
	public partial class EquityMultipleExercise : IEquityMultipleExercise
	{
		#region Constructors
		public EquityMultipleExercise()
		{
			integralMultipleExercise = new EFS_Decimal();
		}
		#endregion Constructors

		#region IEquityMultipleExercise Members
		bool IEquityMultipleExercise.IntegralMultipleExerciseSpecified
		{
			set { this.integralMultipleExerciseSpecified = value; }
			get { return this.integralMultipleExerciseSpecified; }
		}
		EFS_Decimal IEquityMultipleExercise.IntegralMultipleExercise
		{
			set { this.integralMultipleExercise = value; }
			get { return this.integralMultipleExercise; }
		}
		EFS_Decimal IEquityMultipleExercise.MinimumNumberOfOptions
		{
			set { this.minimumNumberOfOptions = value; }
			get { return this.minimumNumberOfOptions; }
		}
		EFS_Decimal IEquityMultipleExercise.MaximumNumberOfOptions
		{
			set { this.maximumNumberOfOptions = value; }
			get { return this.maximumNumberOfOptions; }
		}
		#endregion IEquityMultipleExercise Members
	}
	#endregion EquityMultipleExercise
	#region EquityOption
    /// EG 20150422 [20513] BANCAPERTA New  INbOptionsAndNotionalBase
    public partial class EquityOption : IProduct, IEquityOption, IDeclarativeProvision, INbOptionsAndNotionalBase 
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_EquityOption efs_EquityOption;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_EquityOptionPremium efs_EquityOptionPremium;
		#endregion Members

        #region IProduct Members
        object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

        #region IEquityOption Members
        IEquityStrike IEquityOption.Strike
		{
			set { this.strike = (EquityStrike)value;}
			get { return this.strike;}
		}
		bool IEquityOption.SpotPriceSpecified
		{
			set { this.spotPriceSpecified = value; }
			get { return this.spotPriceSpecified; }
		}
		EFS_Decimal IEquityOption.SpotPrice
		{
			set { this.spotPrice = value; }
			get { return this.spotPrice; }
		}
		bool IEquityOption.NumberOfOptionsSpecified
		{
			set { this.numberOfOptionsSpecified = value; }
			get { return this.numberOfOptionsSpecified; }
		}
		EFS_Decimal IEquityOption.NumberOfOptions
		{
			set { this.numberOfOptions = value; }
			get { return this.numberOfOptions; }
		}
		EFS_Decimal IEquityOption.OptionEntitlement
		{
			set { this.optionEntitlement = value; }
			get { return this.optionEntitlement; }
		}
		IEquityPremium IEquityOption.EquityPremium
		{
			set { this.equityPremium = (EquityPremium)value; }
			get { return this.equityPremium; }
		}
		EFS_EquityOption IEquityOption.Efs_EquityOption
		{
			set { this.efs_EquityOption = value; }
			get { return this.efs_EquityOption; }
		}
		EFS_EquityOptionPremium IEquityOption.Efs_EquityOptionPremium
		{
			set { this.efs_EquityOptionPremium = value; }
			get { return this.efs_EquityOptionPremium; }
		}
		#endregion IEquityOption Members
		#region IEquityDerivativeLongFormBase Members
		bool IEquityDerivativeLongFormBase.DividendConditionsSpecified
		{
			set { this.dividendConditionsSpecified = value; }
			get { return this.dividendConditionsSpecified; }
		}
		IDividendConditions IEquityDerivativeLongFormBase.DividendConditions
		{
			set { this.dividendConditions = (DividendConditions)value; }
			get { return this.dividendConditions; }
		}
		MethodOfAdjustmentEnum IEquityDerivativeLongFormBase.MethodOfAdjustment
		{
			set { this.methodOfAdjustment = value; }
			get { return this.methodOfAdjustment; }
		}
		IExtraordinaryEvents IEquityDerivativeLongFormBase.ExtraordinaryEvents
		{
			set { this.extraordinaryEvents = (ExtraordinaryEvents)value; }
			get { return this.extraordinaryEvents; }
		}
		#endregion IEquityDerivativeLongFormBase Members
		#region IEquityDerivativeBase Members
		IReference IEquityDerivativeBase.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference) value; }
			get { return this.buyerPartyReference; }
		}
		IReference IEquityDerivativeBase.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		OptionTypeEnum IEquityDerivativeBase.OptionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IEquityDerivativeBase.EquityEffectiveDateSpecified
		{
			set { this.equityEffectiveDateSpecified = value; }
			get { return this.equityEffectiveDateSpecified; }
		}
		EFS_Date IEquityDerivativeBase.EquityEffectiveDate
		{
			set { this.equityEffectiveDate = value; }
			get { return this.equityEffectiveDate; }
		}
		IUnderlyer IEquityDerivativeBase.Underlyer
		{
			set { this.underlyer = (Underlyer)value; }
			get { return this.underlyer; }
		}
		bool IEquityDerivativeBase.NotionalSpecified
		{
			set { this.notionalSpecified = value; }
			get { return this.notionalSpecified; }
		}
		IMoney IEquityDerivativeBase.Notional
		{
			set { this.notional = (Money)value; }
			get { return this.notional; }
		}
		IEquityExerciseValuationSettlement IEquityDerivativeBase.EquityExercise
		{
			set { this.equityExercise = (EquityExerciseValuationSettlement)value; }
			get { return this.equityExercise; }
		}
		bool IEquityDerivativeBase.FeatureSpecified
		{
			set { this.featureSpecified = value; }
			get { return this.featureSpecified; }
		}
		IOptionFeatures IEquityDerivativeBase.Feature
		{
			set { this.feature = (OptionFeatures)value; }
			get { return this.feature; }
		}
		bool IEquityDerivativeBase.FxFeatureSpecified
		{
			set { this.fxFeatureSpecified = value; }
			get { return this.fxFeatureSpecified; }
		}
		IFxFeature IEquityDerivativeBase.FxFeature
		{
			set { this.fxFeature = (FxFeature)value; }
			get { return this.fxFeature; }
		}
		bool IEquityDerivativeBase.StrategyFeatureSpecified
		{
			set { this.strategyFeatureSpecified = value; }
			get { return this.strategyFeatureSpecified; }
		}
		IStrategyFeature IEquityDerivativeBase.StrategyFeature
		{
			set { this.strategyFeature = (StrategyFeature)value; }
			get { return this.strategyFeature; }
		}
		#endregion IEquityDerivativeBase Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
        #region INbOfOptionsAndNotional Membres
        bool INbOptionsAndNotionalBase.NumberOfOptionsSpecified
        {
            get { return numberOfOptionsSpecified; }
        }
        EFS_Decimal INbOptionsAndNotionalBase.NumberOfOptions
        {
            get { return numberOfOptions; }
        }
        EFS_Decimal INbOptionsAndNotionalBase.OptionEntitlement
        {
            get { return optionEntitlement; }
        }
        bool INbOptionsAndNotionalBase.NotionalSpecified
        {
            get { return notionalSpecified; }
        }
        IMoney INbOptionsAndNotionalBase.Notional
        {
            get { return notional; }
        }
        #endregion INbOptionsAndNotionalBase Membres
    }
	#endregion EquityOption
	#region EquityOptionTransactionSupplement
	public partial class EquityOptionTransactionSupplement : IProduct,IDeclarativeProvision
	{
		#region Constructors
		public EquityOptionTransactionSupplement()
		{
			unitMultiplier = new EFS_Decimal();
			unitOptionEntitlement = new EFS_Decimal();
			unitNone = new Empty();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion EquityOptionTransactionSupplement

	#region PrePayment
	public partial class PrePayment : IPrePayment
	{
		#region IPrePayment Members
		IReference IPrePayment.PayerPartyReference
		{
			set { this.payerPartyReference = (PartyOrAccountReference)value; }
			get { return this.payerPartyReference; }
		}
		IReference IPrePayment.ReceiverPartyReference
		{
			set { this.receiverPartyReference = (PartyOrAccountReference)value; }
			get { return this.receiverPartyReference; }
		}
		EFS_Boolean IPrePayment.PrePayment
		{
			set { this.prePayment = value; }
			get { return this.prePayment; }
		}
		IMoney IPrePayment.PrePaymentAmount
		{
			set { this.prePaymentAmount = (Money)value; }
			get { return this.prePaymentAmount; }
		}
		IAdjustableDate IPrePayment.PrePaymentDate
		{
			set { this.prePaymentDate = (AdjustableDate)value; }
			get { return this.prePaymentDate; }
		}
		#endregion IPrePayment Members
	}
	#endregion PrePayment
}
