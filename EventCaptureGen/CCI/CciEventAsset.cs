#region Using Directives
using System;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 

using EFS.GUI;
using EFS.GUI.CCI; 
using EFS.GUI.Interface;



using EfsML;
using EfsML.DynamicData;
using EfsML.Enum;

using FpML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{
	#region CciEventAsset
    public class CciEventAsset : IContainerCciFactory, IContainerCci
	{
		#region Enums
        #region CciEnum
        public enum CciEnum
		{
			idAsset,
			assetType,
			fixingTime_hourMinuteTime,
			fixingTime_businessCenter,
            idMarketEnv,
            idValScenario,
			quote_side,
			quote_timing,
			src_primaryRateSrc,
			src_primaryRateSrcPage,
			src_primaryRateSrcHead,
            clearanceSystem,
            idM,
            idC,
            weight,
            unitWeight,
            unitTypeWeight,
            isinCode,
            assetSymbol,
            identifier,
            displayName,
            contractSymbol,
            category,
            putCallValue,
            strikePrice,
            contractMultiplier,
            maturityDate,
            deliveryDate,
            nominalValue,
            maturityDateSys,
            assetCategory,
			unknown,

        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        public EventCustomCaptureInfos ccis;
        private EventAsset eventAsset;
        private readonly string prefix;
        private readonly int number;
        #endregion Members
		#region Accessors
        #region AssetCategory
        public Cst.UnderlyingAsset AssetCategory
        {
            get { return (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), eventAsset.assetCategory); }
        }
        #endregion AssetCategory
        #region CS
        public string CS
		{
			get {return ccis.CS;}
		}
		#endregion CS
		
		#region EventAsset
		public EventAsset EventAsset
		{
			set {eventAsset = (EventAsset) value;}
			get {return eventAsset;}	
		}
		#endregion EventAsset
		#region ExistNumber
		private bool ExistNumber 
		{
			get {return (0 < number);}
		}
		#endregion ExistNumber
        #region IsFxRate
        public bool IsFxRate
        {
            get { return Cst.ProductFamily_FX == ccis.EventInput.CurrentEvent.family; }
        }
        #endregion IsFxRate
		#region IsETD
        public bool IsETD
		{
			get {return Cst.ProductFamily_LSD == ccis.EventInput.CurrentEvent.family;}   
		}
		#endregion IsFxRate
        #region IsRateIndex
        public bool IsRateIndex
        {
            get { return Cst.ProductFamily_IRD == ccis.EventInput.CurrentEvent.family; }
        }
        #endregion IsRateIndex
        #region IsExchangeTradedDerivative
        public bool IsExchangeTradedDerivative
        {
            get { return Cst.ProductFamily_LSD == ccis.EventInput.CurrentEvent.family; }
        }
        #endregion IsExchangeTradedDerivative
        #region Number
        private string Number
		{
			get
			{ 
				string ret =  string.Empty; 
				if (ExistNumber)
					ret = number.ToString();
				return ret;				
			}
		}
		#endregion Number
		#endregion Accessors
		#region Constructor
		public CciEventAsset(CciEvent pCciEvent,int pNumber,EventAsset pEventAsset,string pPrefix)
		{
			number	   = pNumber;
			prefix	   = pPrefix + this.Number  + CustomObject.KEY_SEPARATOR ;
			ccis       = pCciEvent.ccis;  	
			eventAsset = pEventAsset;
		}
		#endregion Constructors
        #region Methods
        #region Clear
        public void Clear()
        {
            ccis.Set(CciClientId(CciEnum.assetCategory), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.assetSymbol), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.assetType), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.category), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.contractSymbol), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.deliveryDate), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.displayName), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idAsset), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idC), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.identifier), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idM), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idMarketEnv), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idValScenario), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.clearanceSystem), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.isinCode), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.fixingTime_hourMinuteTime), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.fixingTime_businessCenter), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.maturityDate), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.maturityDateSys), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.nominalValue), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.putCallValue), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.quote_side), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.quote_timing), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrc), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrcPage), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrcHead), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.strikePrice), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.unitTypeWeight), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.unitWeight), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.weight), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.contractMultiplier), "NewValue", string.Empty);
        }
        #endregion Clear
        #region  SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            ccis.Set(CciClientId(CciEnum.assetType), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.fixingTime_hourMinuteTime), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.fixingTime_businessCenter), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.quote_side), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.quote_timing), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrc), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrcPage), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.src_primaryRateSrcHead), "IsEnabled", pIsEnabled);
            Cci(CciEnum.idAsset).IsEnabled = true;
        }
        #endregion SetEnabled
        #endregion Methods
		#region Interface Methods
		#region IContainerCciFactory members
		#region AddCciSystem
		public void AddCciSystem(){}
		#endregion AddCciSystem
		#region Initialize_FromCci
		public void Initialize_FromCci()
		{
            CciTools.CreateInstance(this, eventAsset);

            if (eventAsset.maturityDateSpecified)
            {
                if (null == eventAsset.maturityDate)
                    eventAsset.maturityDate = new EFS_Date();
            }
		}
		#endregion Initialize_FromCci
		#region Initialize_FromDocument
		public void Initialize_FromDocument()
		{ 
			Type tCciEnum = typeof(CciEnum);
			foreach (CciEnum cciEnum in Enum.GetValues(tCciEnum))
			{
                CustomCaptureInfo cci = Cci(cciEnum); 
				if (cci != null)
				{
					#region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
					#endregion Reset variables

                    switch (cciEnum)
					{
						case CciEnum.idAsset:
							#region IdAsset
							try
							{
								if (eventAsset.idAsset > 0)
								{
                                    SQL_AssetBase sql_Asset = null;
                                    switch (AssetCategory)
                                    {
                                        case Cst.UnderlyingAsset.Bond:
                                            sql_Asset = new SQL_AssetDebtSecurity(CS, eventAsset.idAsset);
                                            break;
                                        case Cst.UnderlyingAsset.EquityAsset:
                                            sql_Asset = new SQL_AssetEquity(CS, eventAsset.idAsset);
                                            break;
                                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                                            sql_Asset = new SQL_AssetETD(CS, eventAsset.idAsset);
                                            break;
                                        case Cst.UnderlyingAsset.FxRateAsset:
                                            sql_Asset = new SQL_AssetFxRate(CS, eventAsset.idAsset);
                                            break;
                                        case Cst.UnderlyingAsset.Index:
                                            sql_Asset = new SQL_AssetIndex(CS, eventAsset.idAsset);
                                            break;
                                        case Cst.UnderlyingAsset.RateIndex:
                                            sql_Asset = new SQL_AssetRateIndex(CS, SQL_AssetRateIndex.IDType.IDASSET, eventAsset.idAsset);
                                            break;
                                        default:
                                            break;
                                    }
                                    if ((null != sql_Asset) && sql_Asset.IsLoaded)
                                    {
                                        cci.Sql_Table = sql_Asset;
                                        data = sql_Asset.Identifier;
                                    }
								}
							}
							catch
							{
								cci.Sql_Table = null;
								data = string.Empty;
							}
							break;
							#endregion IdAsset
						case CciEnum.assetType:
                            #region AssetType
                            if (eventAsset.assetTypeSpecified)
                                data = eventAsset.assetType;
                            break;
                            #endregion AssetType
                        case CciEnum.assetCategory:
                            #region AssetCategory
                            if (eventAsset.assetCategorySpecified)
                                data = eventAsset.assetCategory;
                            break;
                            #endregion AssetCategory
                        case CciEnum.fixingTime_hourMinuteTime:
							#region Time
							if (eventAsset.timeSpecified)
								data = eventAsset.time.Value; 
							break;
							#endregion Time
						case CciEnum.fixingTime_businessCenter:
							#region IdBC
							data = eventAsset.idBC; 
							break;
							#endregion IdBC
						case CciEnum.src_primaryRateSrc:
							#region PrimaryRateSrc
							if (eventAsset.primaryRateSrcSpecified)
								data = eventAsset.primaryRateSrc; 
							break;
							#endregion PrimaryRateSrc
						case CciEnum.src_primaryRateSrcPage:
							#region PrimaryRateSrcPage
							if (eventAsset.primaryRateSrcPageSpecified)
								data = eventAsset.primaryRateSrcPage; 
							break;
							#endregion PrimaryRateSrcPage
						case CciEnum.src_primaryRateSrcHead:
							#region PrimaryRateSrcHead
							if (eventAsset.primaryRateSrcHeadSpecified)
								data = eventAsset.primaryRateSrcHead; 
							break;
							#endregion PrimaryRateSrcHead
                        case CciEnum.clearanceSystem:
                            #region ClearanceSystem
                            if (eventAsset.clearanceSystemSpecified)
                                data = eventAsset.clearanceSystem;
                            break;
                            #endregion ClearanceSystem
                        case CciEnum.idMarketEnv:
                            #region IdMarketEnv
                            if (eventAsset.idMarketEnvSpecified)
                                data = eventAsset.idMarketEnv;
                            break;
                            #endregion IdMarketEnv
                        case CciEnum.idValScenario:
                            #region IdValScenario
                            if (eventAsset.idValScenarioSpecified)
                                data = eventAsset.idValScenario;
                            break;
                            #endregion IdValScenario
                        case CciEnum.quote_side:
							#region QuoteSide
							if (eventAsset.quoteSideSpecified)
								data = eventAsset.quoteSide; 
							break;
							#endregion QuoteSide
						case CciEnum.quote_timing:
							#region QuoteTiming
							if (eventAsset.quoteTimingSpecified)
								data = eventAsset.quoteTiming; 
							break;
							#endregion QuoteTiming
                        case CciEnum.idM:
                            #region Market
                            if (eventAsset.idMSpecified)
                            {
                                SQL_Market sql_Market = new SQL_Market(CS,eventAsset.idM.IntValue);
                                if (sql_Market.IsLoaded)
                                {
                                    cci.Sql_Table = sql_Market;
                                    data = sql_Market.Identifier;
                                }
                            }
                            break;
                            #endregion Market
                        case CciEnum.idC:
                            #region IdC
                            if (eventAsset.idCSpecified)
                                data = eventAsset.idC;
                            break;
                            #endregion IdC
                        case CciEnum.isinCode:
                            #region isinCode
                            if (eventAsset.isinCodeSpecified)
                                data = eventAsset.isinCode;
                            break;
                            #endregion isinCode
                        case CciEnum.assetSymbol:
                            #region assetSymbol
                            if (eventAsset.assetSymbolSpecified)
                                data = eventAsset.assetSymbol;
                            break;
                            #endregion assetSymbol
                        case CciEnum.identifier:
                            #region identifier
                            if (eventAsset.identifierSpecified)
                                data = eventAsset.identifier;
                            break;
                            #endregion identifier
                        case CciEnum.displayName:
                            #region displayName
                            if (eventAsset.displayNameSpecified)
                                data = eventAsset.displayName;
                            break;
                            #endregion displayName
                        case CciEnum.contractSymbol:
                            #region contractSymbol
                            if (eventAsset.contractSymbolSpecified)
                                data = eventAsset.contractSymbol;
                            break;
                            #endregion contractSymbol
                        case CciEnum.category:
                            #region category
                            if (eventAsset.categorySpecified)
                                data = eventAsset.category;
                            break;
                            #endregion category
                        case CciEnum.putCallValue:
                            #region putOrCall
                            if (eventAsset.putOrCallSpecified)
                                data = eventAsset.putOrCall;
                            break;
                            #endregion putOrCall
                        case CciEnum.strikePrice:
                            #region strikePrice
                            if (eventAsset.strikePriceSpecified)
                                data = eventAsset.strikePrice.Value;
                            break;
                            #endregion strikePrice
                        case CciEnum.contractMultiplier:
                            #region contractMultiplier
                            if (eventAsset.contractMultiplierSpecified)
                                data = eventAsset.contractMultiplier.Value;
                            break;
                            #endregion contractMultiplier
                        case CciEnum.maturityDate:
                            #region maturityDate
                            if (eventAsset.maturityDateSpecified)
                                data = eventAsset.maturityDate.Value;
                            break;
                            #endregion maturityDate
                        case CciEnum.maturityDateSys:
                            #region MaturityDateSys
                            if (eventAsset.maturityDateSysSpecified)
                                data = eventAsset.maturityDateSys.Value;
                            break;
                            #endregion MaturityDateSys
                        case CciEnum.deliveryDate:
                            #region deliveryDate
                            if (eventAsset.deliveryDateSpecified)
                                data = eventAsset.deliveryDate.Value;
                            break;
                            #endregion deliveryDate
                        case CciEnum.nominalValue:
                            #region nominalValue
                            if (eventAsset.nominalValueSpecified)
                                data = eventAsset.nominalValue.Value;
                            break;
                            #endregion nominalValue
                        case CciEnum.weight:
                            #region Weight
                            if (eventAsset.weightSpecified)
                                data = eventAsset.weight.Value;
                            break;
                            #endregion Weight
                        case CciEnum.unitWeight:
                            #region UnitWeight
                            if (eventAsset.unitWeightSpecified)
                                data = eventAsset.unitWeight;
                            break;
                            #endregion UnitWeight
                        case CciEnum.unitTypeWeight:
                            #region UnitTypeWeight
                            if (eventAsset.unitTypeWeightSpecified)
                                data = eventAsset.unitTypeWeight;
                            break;
                            #endregion UnitTypeWeight
                        default:
							isSetting = false;
							break;
					}
					if (isSetting)
						ccis.InitializeCci(cci,sql_Table,data);
				}
			}
			if (false == Cci(CciEnum.idAsset).IsMandatory)
				SetEnabled(Cci(CciEnum.idAsset).IsFilledValue);
			
		}
		#endregion Initialize_FromDocument
		#region Dump_ToDocument
		public void Dump_ToDocument()
		{ 
			Type tCciEnum = typeof(CciEnum);
            foreach (CciEnum keyEnum in Enum.GetValues(tCciEnum))
			{
                CustomCaptureInfo cci = Cci(keyEnum); 
				if ((cci != null) && (cci.HasChanged))
				{
					#region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
					#endregion

					switch (keyEnum)
					{
						case CciEnum.idAsset:
							#region IdAsset
							if (StrFunc.IsEmpty(cci.NewValue) && (false == Cci(CciEnum.idAsset).IsMandatory) )  
							{
								eventAsset.idAsset = 0;
								Clear(); 
							}
							else
							{
								string dataToFind   = data.Replace(" ", "%") + "%";
                                SQL_AssetBase sql_Asset = null;
                                switch (AssetCategory)
                                {
                                    case Cst.UnderlyingAsset.Bond:
                                        sql_Asset = new SQL_AssetDebtSecurity(CS, SQL_TableWithID.IDType.Identifier, dataToFind);
                                        break;
                                    case Cst.UnderlyingAsset.EquityAsset:
                                        sql_Asset = new SQL_AssetEquity(CS, SQL_TableWithID.IDType.Identifier, dataToFind);
                                        break;
                                    case Cst.UnderlyingAsset.ExchangeTradedContract:
                                        sql_Asset = new SQL_AssetETD(CS, SQL_TableWithID.IDType.Identifier, dataToFind);
                                        break;
                                    case Cst.UnderlyingAsset.FxRateAsset:
                                        sql_Asset = new SQL_AssetFxRate(CS, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                        break;
                                    case Cst.UnderlyingAsset.Index:
                                        sql_Asset = new SQL_AssetIndex(CS, SQL_TableWithID.IDType.Identifier, dataToFind);
                                        break;
                                    case Cst.UnderlyingAsset.RateIndex:
                                        sql_Asset = new SQL_AssetRateIndex(CS, SQL_AssetRateIndex.IDType.Asset_Identifier, dataToFind);
                                        break;
                                    default:
                                        break;
                                }
                                if ((null != sql_Asset) && sql_Asset.IsLoaded)
                                {
                                    cci.NewValue = sql_Asset.Identifier;
                                    cci.Sql_Table = sql_Asset;
                                    eventAsset.idAsset = sql_Asset.Id;
                                    eventAsset.idM = new EFS_Integer(sql_Asset.IdM);
                                    eventAsset.displayName = sql_Asset.DisplayName;
                                    eventAsset.identifier = sql_Asset.Identifier;

                                    switch (AssetCategory)
                                    {
                                        case Cst.UnderlyingAsset.Bond:

                                            SQL_AssetDebtSecurity _assetDebtSecurity = sql_Asset as SQL_AssetDebtSecurity;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.Bond.ToString();
                                            eventAsset.assetType = QuoteEnum.DEBTSECURITY.ToString();
                                            eventAsset.clearanceSystem = _assetDebtSecurity.ClearanceSystem;
                                            eventAsset.idBC = _assetDebtSecurity.Market_IDBC;
                                            eventAsset.idC = _assetDebtSecurity.IdC;
                                            eventAsset.idM = new EFS_Integer(_assetDebtSecurity.IdM);
                                            break;

                                        case Cst.UnderlyingAsset.EquityAsset:

                                            SQL_AssetEquity _assetEquity = sql_Asset as SQL_AssetEquity;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.EquityAsset.ToString();
                                            eventAsset.assetType = QuoteEnum.EQUITY.ToString();
                                            eventAsset.assetSymbol = _assetEquity.AssetSymbol;
                                            eventAsset.clearanceSystem = _assetEquity.ClearanceSystem;
                                            eventAsset.idBC = _assetEquity.Market_IDBC; 
                                            eventAsset.idC = _assetEquity.IdC;
                                            eventAsset.idM = new EFS_Integer(_assetEquity.IdM);
                                            eventAsset.isinCode = _assetEquity.ISINCode;
                                            break;

                                        case Cst.UnderlyingAsset.ExchangeTradedContract:

                                            SQL_AssetETD _assetETD = sql_Asset as SQL_AssetETD;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract.ToString();
                                            eventAsset.assetType = QuoteEnum.ETD.ToString();
                                            eventAsset.isinCode = _assetETD.ISINCode;
                                            eventAsset.assetSymbol = _assetETD.AssetSymbol;
                                            eventAsset.contractSymbol = _assetETD.DrvContract_Symbol;
                                            eventAsset.category = _assetETD.DrvContract_Category;
                                            eventAsset.putOrCall = _assetETD.PutCall;
                                            eventAsset.strikePrice = new EFS_Decimal(_assetETD.StrikePrice);
                                            eventAsset.contractMultiplier = new EFS_Decimal(_assetETD.ContractMultiplier);
                                            eventAsset.maturityDate = new EFS_Date
                                            {
                                                DateValue = _assetETD.Maturity_MaturityDate
                                            };
                                            eventAsset.maturityDateSys = new EFS_Date
                                            {
                                                DateValue = _assetETD.Maturity_MaturityDateSys
                                            };
                                            eventAsset.deliveryDate = new EFS_Date
                                            {
                                                DateValue = _assetETD.Maturity_DeliveryDate
                                            };
                                            eventAsset.nominalValue = new EFS_Decimal(_assetETD.DrvContract_NominalValue);
                                            break;

                                        case Cst.UnderlyingAsset.FxRateAsset:

                                            SQL_AssetFxRate _assetFxRate = sql_Asset as SQL_AssetFxRate;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.FxRateAsset.ToString();
                                            eventAsset.assetType = QuoteEnum.FXRATE.ToString();
                                            if (false == eventAsset.timeSpecified)
                                                eventAsset.time = new EFS_Time();
                                            eventAsset.time.TimeValue = _assetFxRate.TimeRateSrc;
                                            eventAsset.idBC = _assetFxRate.IdBC_RateSrc;
                                            eventAsset.primaryRateSrc = _assetFxRate.PrimaryRateSrc;
                                            eventAsset.primaryRateSrcPage = _assetFxRate.PrimaryRateSrcPage;
                                            eventAsset.primaryRateSrcHead = _assetFxRate.PrimaryRateSrcHead;
                                            break;

                                        case Cst.UnderlyingAsset.Index:

                                            SQL_AssetIndex _assetIndex = sql_Asset as SQL_AssetIndex;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.Index.ToString();
                                            eventAsset.assetType = QuoteEnum.INDEX.ToString();
                                            eventAsset.assetSymbol = _assetIndex.AssetSymbol;
                                            eventAsset.clearanceSystem = _assetIndex.ClearanceSystem;
                                            eventAsset.idBC = _assetIndex.Market_IDBC;
                                            eventAsset.idC = _assetIndex.IdC;
                                            eventAsset.idM = new EFS_Integer(_assetIndex.IdM);
                                            eventAsset.isinCode = _assetIndex.ISINCode;
                                            break;

                                        case Cst.UnderlyingAsset.RateIndex:

                                            SQL_AssetRateIndex _assetRateIndex = sql_Asset as SQL_AssetRateIndex;
                                            eventAsset.assetCategory = Cst.UnderlyingAsset.RateIndex.ToString();
                                            eventAsset.assetType = QuoteEnum.RATEINDEX.ToString();
                                            eventAsset.time = null;
                                            eventAsset.idBC = _assetRateIndex.FirstRow["Idx_IDBC"].ToString();
                                            if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERID"]))
                                                eventAsset.primaryRateSrc = null;
                                            else
                                                eventAsset.primaryRateSrc = _assetRateIndex.FirstRow["Idx_SRCSUPPLIERID"].ToString();
                                            if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERPAGEID"]))
                                                eventAsset.primaryRateSrcPage = null;
                                            else
                                                eventAsset.primaryRateSrcPage = _assetRateIndex.FirstRow["Idx_SRCSUPPLIERPAGEID"].ToString();
                                            if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERHEADID"]))
                                                eventAsset.primaryRateSrcHead = null;
                                            else
                                                eventAsset.primaryRateSrcHead = _assetRateIndex.FirstRow["Idx_SRCSUPPLIERHEADID"].ToString();
                                            break;

                                        default:
                                            break;
                                    }
                                }

                                eventAsset.assetCategorySpecified = StrFunc.IsFilled(eventAsset.assetCategory);
                                eventAsset.assetSymbolSpecified = StrFunc.IsFilled(eventAsset.assetSymbol);
                                eventAsset.assetTypeSpecified = StrFunc.IsFilled(eventAsset.assetType);
                                eventAsset.categorySpecified = StrFunc.IsFilled(eventAsset.category);
                                eventAsset.clearanceSystemSpecified = StrFunc.IsFilled(eventAsset.clearanceSystem);
                                eventAsset.contractMultiplierSpecified = (null != eventAsset.contractMultiplier) && (0 < eventAsset.contractMultiplier.DecValue);
                                eventAsset.contractSymbolSpecified = StrFunc.IsFilled(eventAsset.contractSymbol);
                                eventAsset.deliveryDateSpecified = (DtFunc.IsDateTimeFilled(eventAsset.deliveryDate.DateValue));
                                eventAsset.displayNameSpecified = StrFunc.IsFilled(eventAsset.displayName);
                                eventAsset.idBCSpecified = StrFunc.IsFilled(eventAsset.idBC);
                                eventAsset.idCSpecified = StrFunc.IsFilled(eventAsset.idC);
                                eventAsset.identifierSpecified = StrFunc.IsFilled(eventAsset.identifier);
                                eventAsset.idMSpecified = (null != eventAsset.idM) &&  (0 < eventAsset.idM.IntValue);
                                eventAsset.idMarketEnvSpecified = StrFunc.IsFilled(eventAsset.idMarketEnv);
                                eventAsset.idValScenarioSpecified = StrFunc.IsFilled(eventAsset.idValScenario);
                                eventAsset.isinCodeSpecified = StrFunc.IsFilled(eventAsset.isinCode);
                                eventAsset.maturityDateSpecified = (DtFunc.IsDateTimeFilled(eventAsset.maturityDate.DateValue));
                                eventAsset.maturityDateSysSpecified = (DtFunc.IsDateTimeFilled(eventAsset.maturityDateSys.DateValue));
                                eventAsset.nominalValueSpecified = (null != eventAsset.nominalValue) && (0 < eventAsset.nominalValue.DecValue);
                                eventAsset.primaryRateSrcSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrc);
                                eventAsset.primaryRateSrcHeadSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrcHead);
                                eventAsset.primaryRateSrcPageSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrcPage);
                                eventAsset.putOrCallSpecified = StrFunc.IsFilled(eventAsset.putOrCall);
                                eventAsset.quoteSideSpecified = StrFunc.IsFilled(eventAsset.quoteSide);
                                eventAsset.quoteTimingSpecified = StrFunc.IsFilled(eventAsset.quoteTiming);
                                eventAsset.strikePriceSpecified = (null != eventAsset.strikePrice) && (0 < eventAsset.strikePrice.DecValue);
                                eventAsset.timeSpecified = (null != eventAsset.time);	   
                                eventAsset.unitTypeWeightSpecified = StrFunc.IsFilled(eventAsset.unitTypeWeight);
                                eventAsset.unitWeightSpecified = StrFunc.IsFilled(eventAsset.unitWeight);
                                eventAsset.weightSpecified = (null != eventAsset.weight) && (0 < eventAsset.weight.DecValue);
							}
							eventAsset.idAssetSpecified = (0 < eventAsset.idAsset);
							processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
							break;
							#endregion IdAsset
                        case CciEnum.quote_side:
							#region QuoteSide
							eventAsset.quoteSide          = data;  
							eventAsset.quoteSideSpecified = StrFunc.IsFilled(eventAsset.quoteSide);
							break;
							#endregion QuoteSide
						case CciEnum.quote_timing:
							#region QuoteTiming
							eventAsset.quoteTiming          = data;  
							eventAsset.quoteTimingSpecified = StrFunc.IsFilled(eventAsset.quoteTiming);
							break;
							#endregion QuoteTiming
						case CciEnum.fixingTime_hourMinuteTime:
							#region Time
							eventAsset.time.Value    = data;  
							eventAsset.timeSpecified = StrFunc.IsFilled(eventAsset.time.Value);
							break;
							#endregion Time
						case CciEnum.src_primaryRateSrc:
                            #region primaryRateSrc
							eventAsset.primaryRateSrc          = data;  
							eventAsset.primaryRateSrcSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrc);
							break;
                            #endregion primaryRateSrc
						case CciEnum.src_primaryRateSrcPage:
                            #region primaryRateSrcPage
                            eventAsset.primaryRateSrcPage          = data;  
							eventAsset.primaryRateSrcPageSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrcPage);
							break;
                            #endregion primaryRateSrcPage
                        case CciEnum.src_primaryRateSrcHead:
                            #region primaryRateSrcHead
                            eventAsset.primaryRateSrcHead          = data;  
							eventAsset.primaryRateSrcHeadSpecified = StrFunc.IsFilled(eventAsset.primaryRateSrcHead);
                            break;
                            #endregion primaryRateSrcHead
                        case CciEnum.idMarketEnv:
                            #region idMarketEnv
                            eventAsset.idMarketEnvSpecified = StrFunc.IsFilled(data);
                            eventAsset.idMarketEnv = data;
                            break;
                            #endregion idMarketEnv
                        case CciEnum.idValScenario:
                            #region idValScenario
                            eventAsset.idValScenarioSpecified = StrFunc.IsFilled(data);
                            eventAsset.idValScenario = data;
                            break;
                            #endregion idValScenario
                        default:
							isSetting = false;
							break;
					}
					if (isSetting)
						ccis.Finalize(cci.ClientId_WithoutPrefix , processQueue );
				}
				if (false == Cci(CciEnum.idAsset).IsMandatory)
					SetEnabled(Cci(CciEnum.idAsset).IsFilledValue);
			}
		}

		#endregion Dump_ToDocument
		#region IsClientId_PayerOrReceiver
		public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
		{
			return false;
		}
		#endregion IsClientId_PayerOrReceiver
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region ProcessInitialize
		public void ProcessInitialize(CustomCaptureInfo pCci)
		{
			if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
			{
				string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix)  ;
				CciEnum key = CciEnum.unknown;
				if (System.Enum.IsDefined(typeof(CciEnum),clientId_Key))
					key = (CciEnum) System.Enum.Parse(typeof(CciEnum ), clientId_Key);
				//
				switch(key)
				{
					case CciEnum.idAsset:
						#region IdAsset
						if ((null != pCci.Sql_Table) && pCci.Sql_Table.IsLoaded)
						{
                            switch (AssetCategory)
                            {
                                case Cst.UnderlyingAsset.Bond:
                                    _ = pCci.Sql_Table as SQL_AssetDebtSecurity;
                                    break;

                                case Cst.UnderlyingAsset.EquityAsset:
                                    _ = pCci.Sql_Table as SQL_AssetEquity;
                                    break;

                                case Cst.UnderlyingAsset.ExchangeTradedContract:
                                    _ = pCci.Sql_Table as SQL_AssetETD;
                                    break;

                                case Cst.UnderlyingAsset.FxRateAsset:

                                    SQL_AssetFxRate _assetFxRate = pCci.Sql_Table as SQL_AssetFxRate;
							        ccis.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime.ToString()),DtFunc.DateTimeToString(_assetFxRate.TimeRateSrc, DtFunc.FmtISOTime));
							        ccis.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter.ToString()),_assetFxRate.IdBC_RateSrc);
							        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrc.ToString()),_assetFxRate.PrimaryRateSrc);
							        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcPage.ToString()),_assetFxRate.PrimaryRateSrcPage);
							        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcHead.ToString()),_assetFxRate.PrimaryRateSrcHead);
                                    break;

                                case Cst.UnderlyingAsset.Index:
                                    _ = pCci.Sql_Table as SQL_AssetIndex;
                                    break;

                                case Cst.UnderlyingAsset.RateIndex:

                                    SQL_AssetRateIndex _assetRateIndex = pCci.Sql_Table as SQL_AssetRateIndex;
								    ccis.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime.ToString()),null);
								    ccis.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter.ToString()),_assetRateIndex.FirstRow["Idx_IDBC"].ToString());
                                    if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERID"]))
								        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrc.ToString()),null);
                                    else
                                        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrc.ToString()), _assetRateIndex.FirstRow["Idx_SRCSUPPLIERHEADID"].ToString());

                                    if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERHEADID"]))
                                        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcHead.ToString()), null);
                                    else
                                        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcHead.ToString()), _assetRateIndex.FirstRow["Idx_SRCSUPPLIERID"].ToString());

                                    if (Convert.IsDBNull(_assetRateIndex.FirstRow["Idx_SRCSUPPLIERPAGEID"]))
                                        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcPage.ToString()), null);
                                    else
                                        ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcPage.ToString()), _assetRateIndex.FirstRow["Idx_SRCSUPPLIERPAGEID"].ToString());
                                    break;

                                default:
                                    break;
                            }
						}
						else
						{
							ccis.SetNewValue(CciClientId(CciEnum.idAsset.ToString()),string.Empty);
							ccis.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime.ToString()),string.Empty);
							ccis.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter.ToString()),string.Empty);
							ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrc.ToString()),string.Empty);
							ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcPage.ToString()),string.Empty);
							ccis.SetNewValue(CciClientId(CciEnum.src_primaryRateSrcHead.ToString()),string.Empty);
						}
						break;
						#endregion IdAsset
					default:
                        #region Default
						break;
						#endregion Default
				}
			}	
        }
        #endregion ProcessInitialize
        #region CleanUp
        public void CleanUp()
		{
		}
		#endregion
		#region GetDisplay
		public void SetDisplay(CustomCaptureInfo pCci)
		{
		}
		#endregion
		#region RefreshCciEnabled
		public void RefreshCciEnabled()
		{
			
		}
		#endregion
		#region RemoveLastItemInArray
		public void RemoveLastItemInArray(string _)
		{
		}
		#endregion RemoveLastItemInArray
		#region Initialize_Document
		public void Initialize_Document()
		{
		}
		#endregion Initialize_Document
		#endregion IContainerCciFactory Members
		#region IContainerCci Members
		#region CciClientId
		public string  CciClientId(CciEnum  pEnumValue) 
		{
			return CciClientId(pEnumValue.ToString()); 
		}
		public string CciClientId(string pClientId_Key)
		{
			return prefix + pClientId_Key  ;
		}
		#endregion 
		#region Cci
		public CustomCaptureInfo Cci(CciEnum pEnum  )
		{
			return Cci(pEnum.ToString()); 
		}
		public CustomCaptureInfo Cci(string pClientId_Key)
		{
			return ccis[CciClientId(pClientId_Key)];
		}

		#endregion Cci
		#region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(prefix));
        }
		#endregion 
		#region CciContainerKey
		public string CciContainerKey(string pClientId_WithoutPrefix)
		{
			return pClientId_WithoutPrefix.Substring(prefix.Length);
		}
		#endregion 
		#region IsCci
		public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci )
		{
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix)  ; 
		}
		#endregion

        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (IsCci(CciEnum.idAsset, pCci))
            {
                bool isOk = true;
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;

                if (eventAsset.assetCategorySpecified)
                {
                    switch (AssetCategory)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            pCo.Referential = "ASSET_EQUITY";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_EQUITY";
                            break;
                        case Cst.UnderlyingAsset.Index:
                            pCo.Referential = "ASSET_INDEX";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_INDEX";
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                            pCo.Referential = "ASSET_ETD";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_ASSET_ETD";
                            break;
                        case Cst.UnderlyingAsset.FxRateAsset:
                            pCo.Referential = "ASSET_FXRATE";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_FXRATE";
                            break;
                        case Cst.UnderlyingAsset.RateIndex:
                            pCo.Referential = "ASSET_RATEINDEX";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_RATEINDEX";
                            break;
                        case Cst.UnderlyingAsset.Bond:
                            isOk = false;
                            //pCo.Referential = "ASSET_RATEINDEX";
                            //pCo.Title = "OTC_INP_DEBTSECURITY";
                            break;
                        default:
                            isOk = false;
                            break;
                    }

                    if (isOk)
                    {
                        pCo.DynamicArgument = null;
                        pCo.ClientId = pCci.ClientId_WithoutPrefix;
                        pCo.SqlColumn = "IDENTIFIER";
                        pCo.Fk = null;
                    }

                }
            }
        }
        #endregion SetButtonReferential
		#endregion IContainerCci Members
		#endregion Interface Methods
	}
	#endregion CciEventAsset
}
