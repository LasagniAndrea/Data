#region Using Directives
using System;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.ApplicationBlocks.Data;

using EfsML.Enum;
using EfsML.Enum.Tools;

using FixML.Enum;
using FixML.v50SP1.Enum;

using FpML.Enum;
#endregion Using Directives


namespace EFS.Common
{

    /// <summary>
    /// Classe de base pour la lecture des assets
    /// <para>Les colonnes Market_IDENTIFIER, Market_ISO10383_ALPHA4, Market_IDBC, Market_FIXML_SecurityExchange sont systématiquement disponibles lorsque </para>
    /// </summary>
    public abstract class SQL_AssetBase : SQL_TableWithID
    {
        #region constructors
        /// <summary>
        /// Constructeur qui permet de passer directement un jeu de résultat. 
        /// <para>Attention les noms de colonnes du jeu de résultat doivent coincider avec ceux attendus dans les properties</para>
        /// </summary>
        /// FI 20131223 [19337] add Constrctor
        public SQL_AssetBase(DataTable pDt)
            : base(pDt)
        {
        }
        public SQL_AssetBase(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, pTable, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region properties
        /// <summary>
        /// Obtient l'alias utilisé pour la table MARKET
        /// </summary>
        public string AliasMarketTable
        {
            get { return "market"; }
        }

        /// <summary>
        /// Obtient l'id de l'aset
        /// </summary>
        public override int Id
        {
            get { return IdAsset; }
        }

        /// <summary>
        /// Obtient l'id de l'asset
        /// </summary>
        public virtual int IdAsset
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASSET")); }
        }
        /// <summary>
        /// Obtient la devise de l'asset
        /// </summary>
        public virtual string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }
        /// <summary>
        /// Obtient l'id du marché
        /// </summary>
        public virtual int IdM
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM")); }
        }
        /// <summary>
        /// Obtient le type (Market/Segment) du marché associé à l'asset
        /// </summary>
        public virtual string Market_MarketType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_MARKETTYPE")); }
        }
        /// <summary>
        /// Obtient l'identifier du marché associé à l'asset
        /// </summary>
        public virtual string Market_Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_IDENTIFIER")); }
        }
        /// <summary>
        /// Obtient l'ISO10383_ALPHA4 du marché associé à l'asset
        /// </summary>
        public virtual string Market_ISO10383_ALPHA4
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_ISO10383_ALPHA4")); }
        }
        /// <summary>
        /// Obtient le FIXML_SecurityExchange (ISO10383_ALPHA4 [+ '-' + EXCHANGESYMBOL dans le cas d'un Segment] du marché associé à l'asset
        /// </summary>
        public virtual string Market_FIXML_SecurityExchange 
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_FIXML_SecurityExchange")); }
        }
        /// <summary>
        /// Obtient le Business Center du marché rattaché à l'asset
        /// </summary>
        public virtual string Market_IDBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_IDBC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ClearanceSystem
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CLEARANCESYSTEM")); }
        }
        #endregion
        /// <summary>
        /// Use to display info on capture under asset textbox
        /// </summary>
        /// EG 20140526
        public virtual string SetDisplay
        {
            get 
            {
                string display = Identifier;
                if (StrFunc.IsFilled(Description))
                {
                    if (Description.StartsWith(Identifier))
                        display = Description;
                    else
                        display = Identifier + Cst.Space + "/" + Cst.Space + Description;
                }
                return display;
            }
        }
        /// <summary>
        /// AssetCategory
        /// </summary>
        /// EG 20140904 New
        public virtual Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return null; }
        }
        /// EG 20150302 New (CFD Forex)
        public virtual DateTime TimeRateSrc
        {
            get { return DateTime.MinValue; }
        }

        #region Methodes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        protected override void SetSQLCol(string[] pCol)
        {
            if (IsQueryFindOnly(pCol))
            {
                base.SetSQLCol(pCol);
            }
            else
            {
                if (IsQuerySelectAllColumns(pCol))
                {
                    pCol[0] = SQLObject + "." + "*";  // Pour Oracle Necessaire permet d'obtenir select ACTOR.*, a.IDENTIFIER  as  ACTOR_IDENTIFIER etc 
                }
                base.SetSQLCol(pCol);
                
                AddSQLCol(AliasMarketTable + ".IDENTIFIER as Market_IDENTIFIER");
                AddSQLCol(AliasMarketTable + ".ISO10383_ALPHA4 as Market_ISO10383_ALPHA4");
                AddSQLCol(AliasMarketTable + ".IDBC as Market_IDBC");

                //FI 20131223 [19337] Appel à la méthode MarketTools.BuildSQLColMarket_FIXML_SecurityExchange
                //sqlFIXML_SecurityExchange = String.Format(SQLCst.CASE_WHEN_THEN_ELSE_END,
                //     aliasMarketTable + ".MARKETTYPE=" + DataHelper.SQLString(Cst.MarketTypeEnum.SEGMENT.ToString()),
                //     aliasMarketTable + ".ISO10383_ALPHA4 || '-' || " + aliasMarketTable + ".IDENTIFIER",
                //     aliasMarketTable + ".ISO10383_ALPHA4");

                string col = MarketTools.BuildSQLColMarket_FIXML_SecurityExchange(AliasMarketTable);
                AddSQLCol(col);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            
            string sqlFrom = GetSQLJoinMarket();
            
            ConstructFrom(sqlFrom);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSQLJoinMarket()
        {
            return OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.MARKET, false, SQLObject + ".IDM", AliasMarketTable) + Cst.CrLf;
        }
        #endregion
    }

    /// <summary>
    /// Get an Asset Exchange Traded Derivative
    /// </summary>
    /// <returns>void</returns>
    /// EG 20140116 Report v3.7 
    /// PM 20140514 [19970][19259] Ajout de l'accesseur OriginalContractMultiplier
    /// PM 20141120 [20508] Ajout de l'accesseur CashFlowCalculationMethod
    /// PL 20181001 [24211] RICCODE/BBGCODE
    /// FI 20220321 [XXXXX] implementation de IAssetETDIdent
    /// PM 20240122 [WI822] Ajout IAssetETDIdent.PriceCurrency
    public class SQL_AssetETD : SQL_AssetBase, IAssetETDIdent
    {
        #region private members
        private string _AssetSymbol_In = null;
        private string _ISINCode_In = null;
        //
        private int _idM_In;
        private int _idDC_In;
        private int _idI_In;
        private Nullable<PutOrCallEnum> _putCall_In;
        private decimal _strikePrice_In;
        private string _maturityMonthYear_In;
        //
        #endregion private members

        #region constructors
        /// <summary>
        /// Constructeur qui permet de passer directement un jeu de résultat. 
        /// <para>Attention les noms de colonnes du jeu de résultat doivent coincider avec ceux attendus dans les properties</para>
        /// </summary>
        /// FI 20131223 [19337] add Constructor
        public SQL_AssetETD(DataTable pDt)
            : base(pDt)
        {
        }
        public SQL_AssetETD(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetETD(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetETD(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetETD(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            switch (pIdType)
            {
                case IDType.AssetSymbol:
                    AssetSymbol_In = pIdentifier;
                    break;
                case IDType.IsinCode:
                    ISINCode_In = pIdentifier;
                    break;
            }
        }
        #endregion constructors

        #region properties
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.ExchangeTradedContract; }
        }

        /// <summary>
        /// Obtient la chambre de compensation
        /// </summary>
        public override string ClearanceSystem
        {
            get { return base.ClearanceSystem; } 
        }
        /// <summary>
        /// Obtient le devise de l'asset, la devise de l'asset est la devide du prix (voir TRIM 17548)
        /// <para>Obtient le devise cotée de la devise du prix de l'asset</para>
        /// </summary>
        public override string IdC
        {
            get { return base.IdC; }
        }

        /// <summary>
        /// Obtient l'id du Derivative Contract rattaché à l'asset
        /// </summary>
        public int IdDerivativeContract
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDC")); }
        }

        /// <summary>
        /// Obtient l'id du Derivative Attrib rattaché à l'asset (Echéance ouverte)
        /// </summary>
        public int IdDerivativeAttrib
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDERIVATIVEATTRIB")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AssetSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ASSETSYMBOL")); }
        }

        /// <summary>
        /// Obtient le CFICODE de l'asset
        /// </summary>
        public string CFICode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CFICODE")); }
        }

        /// <summary>
        /// Obtient le AIICODE de l'asset
        /// </summary>
        public string AII
        {
            get { return Convert.ToString(GetFirstRowColumnValue("AII")); }
        }

        /// <summary>
        /// Obtient le code ISIN de l'asset
        /// </summary>
        public string ISINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISINCODE")); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200216 [25699] add
        public string MarketAssignedID
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MARKETASSIGNEDID")); }
        }


        /// <summary>
        /// Obtient le code REUTERS de l'actif [ASSET_ETD.RICCODE]
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        
        /// <summary>
        /// Obtient le code BLOOMBERG de l'actif [ASSET_ETD.BBGCODE]
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime FirstQuotationDay
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("FIRSTQUOTATIONDAY")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal StrikePrice
        {
            get { return Convert.ToDecimal(GetFirstRowColumnValue("STRIKEPRICE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PutCall
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PUTCALL")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nullable<PutOrCallEnum> PutCallEnum
        {
            get
            {
                Nullable<PutOrCallEnum> ret = null;
                if (StrFunc.IsFilled(PutCall))
                    ret = ReflectionTools.ConvertStringToEnum<PutOrCallEnum>(PutCall);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PutCall_EnglishString
        {
            get
            {
                return (null != PutCallEnum) ? PutCallEnum.ToString() : "N/A";
                //switch (PutCall)
                //{
                //    case "0":
                //        return "Put";
                //    case "1":
                //        return "Call";
                //    default:
                //        return "N/A";
                //}
            }
        }

        /// <summary>
        /// Obtient le Contract Multiplier (avec convertion en devise cotée) 
        /// </summary>
        public decimal ContractMultiplier
        {
            get { return Convert.ToDecimal(GetFirstRowColumnValue("CONTRACTMULTIPLIER")); }
        }

        /// <summary>
        /// Obtient le Contract Multiplier sans convertion en devise cotée
        /// </summary>
        //PM 20140514 [19970][19259] Ajout de l'accesseur OriginalContractMultiplier
        public decimal OriginalContractMultiplier
        {
            get { return Convert.ToDecimal(GetFirstRowColumnValue("ORIGINALCONTRACTMULTIPLIER")); }
        }

        /// <summary>
        /// Obtient le Contract Factor  
        /// </summary>
        public Nullable<decimal> Factor
        {
            get
            {
                if (GetFirstRowColumnValue("FACTOR") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("FACTOR"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int DrvContract_IdInstrument
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }

        /// <summary>
        /// Obtient l'identifier du Derivative Contrat rattaché à l'asset
        /// </summary>
        public string DrvContract_Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTIDENTIFIER")); }
        }

        /// <summary>
        /// Obtient le symbol du Derivative Contract rattaché à l'asset
        /// </summary>
        public string DrvContract_Symbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTSYMBOL")); }
        }

        /// <summary>
        /// Obtient la version du Derivative Contract rattaché à l'asset
        /// </summary>
        public string DrvContract_Attribute
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTATTRIBUTE")); }
        }

        /// <summary>
        /// Obtient la date de livraison 
        /// </summary>
        public DateTime Maturity_DeliveryDate
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DELIVERYDATE")); }
        }

        /// <summary>
        /// Obtient la dernière date de négociation de l'asset
        /// </summary>
        public DateTime Maturity_LastTradingDay
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("LASTTRADINGDAY")); }
        }

        /// <summary>
        /// Obtient le nom de l'échéance [est conforme au format défini sur la règle d'échéances] 
        /// </summary>
        public string Maturity_MaturityMonthYear
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MATURITYMONTHYEAR")); }
        }
        /// <summary>
        /// Obtient le nom de l'échéance [est conforme au format défini sur la règle d'échéances] 
        /// </summary>
        public string Maturity_MMMYY
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MATFMT_MMMYY")); }
        }
        /// <summary>
        /// Obtient la date de maturité de l'asset (date forcée)
        /// <para>isnull(ma.MATURITYDATE,ma.MATURITYDATESYS)</para>
        /// </summary>
        public DateTime Maturity_MaturityDate
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("MATURITYDATE")); }
        }
        /// <summary>
        /// Obtient la date de maturité de l'asset (date théorique)
        /// <para>isnull(ma.MATURITYDATESYS,ma.MATURITYDATE)</para>
        /// </summary>
        public DateTime Maturity_MaturityDateSys
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("MATURITYDATESYS")); }
        }
        /// <summary>
        /// Obtient le Nominal du derivative Contract rattaché à l'asset
        /// </summary>
        public decimal DrvContract_NominalValue
        {
            get { return Convert.ToDecimal(GetFirstRowColumnValue("NOMINALVALUE")); }
        }

        /// <summary>
        /// Obtient la devise du prix du derivative Contract rattaché à l'asset
        /// </summary>
        public string DrvContract_PriceCurrency
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PRICECURRENCY")); }
        }

        /// <summary>
        /// Obtient la categorie du derivative Contract rattaché à l'asset
        /// <para>Les valeurs possible sont "O" pour Option ou "F" pour Future</para>
        /// </summary>
        public string DrvContract_Category
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CATEGORY")); }
        }
        /// <summary>
        /// Obtient true si le contrat dérivé est un contrat option
        /// </summary>
        public bool DrvContract_IsOption
        {
            get
            {
                if (StrFunc.IsFilled(DrvContract_Category))
                    return DrvContract_Category.StartsWith("O");
                else
                    return false;
            }
        }
        /// <summary>
        /// Obtient true si le contrat dérivé est un contrat Future
        /// </summary>
        public bool DrvContract_IsFuture
        {
            get
            {
                if (StrFunc.IsFilled(DrvContract_Category))
                    return DrvContract_Category.StartsWith("F");
                else
                    return false;
            }
        }

        /// <summary>
        /// Obtient la categorie du sous jacent du dérivative Contract rattaché à l'asset
        /// </summary>
        public string DrvContract_AssetCategorie
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ASSETCATEGORY")); }
        }

        /// <summary>
        /// Obtient le derivative contract sous jacent du dérivative Contract rattaché à l'asset
        /// <para>Renseigné uniquement si le contrat est un contrat de type "option sur future"</para>
        /// </summary>
        public int DrvContract_IdDerivativeContractUnl
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDDC_UNL")); }
        }

        /// <summary>
        /// Obtient l'asset "future" sous jacent de l'asset 
        /// <para>Renseigné uniquement si le contrat est un derivative contrat de type "option sur future"</para>
        /// </summary>
        public int DrvAttrib_IdAssetUnl
        {
            get
            {
                return Convert.ToInt32(GetFirstRowColumnValue("DA_IDASSET"));
            }
        }

        /// <summary>
        /// Obtient l'asset non future sous jacent du dérivative Contract rattaché à l'asset
        /// <para>Renseigné uniquement si le contrat n'est pas un derivative contrat de type "option sur future"</para>
        /// </summary>
        public int DrvContract_IdAssetUnl
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDASSET_UNL")); }
        }

        /// <summary>
        /// Obtient la règle concernant l'échéance de livraison du future sous-jacent
        /// <para>Renseigné uniquement si le contrat est un derivative contrat de type "option sur future"</para>
        /// </summary>
        public string DrvContract_ExerciseRule
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXERCISERULE")); }
        }

        /// <summary>
        /// Obtient le type de contrat (STD:Standard, FLEX:Flex). Obtient DerivativeContractTypeEnum.STD si CONTRACTTYPE est non spécifié
        /// </summary>
        /// FI 20220311 [XXXXX] use DerivativeContractTypeEnum
        public DerivativeContractTypeEnum DrvContract_ContractType
        {
            get
            {
                DerivativeContractTypeEnum ret = DerivativeContractTypeEnum.STD; // default Value
                if (null != GetFirstRowColumnValue("CONTRACTTYPE"))
                    ret = ReflectionTools.ConvertStringToEnum<DerivativeContractTypeEnum>(Convert.ToString(GetFirstRowColumnValue("CONTRACTTYPE")));
                return ret;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int DrvContract_InstrumentNum
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("INSTRUMENTNUM")); }
        }
        /// <summary>
        /// Obtient la base d'expression des prix de négo, clôtûre
        /// <para>
        /// Obtient 100, si la colonne INSTRUMENTDEN est non renseignée
        /// </para>
        /// </summary>
        public int DrvContract_InstrumentDen
        {
            get
            {
                int ret = 100;
                if (false == (GetFirstRowColumnValue("INSTRUMENTDEN") == Convert.DBNull))
                    ret = Convert.ToInt32(GetFirstRowColumnValue("INSTRUMENTDEN"));
                return ret;
            }
        }

        /// <summary>
        /// Obtient la manière dont doivent être calculés les cashflows
        /// </summary>
        //PM 20141120 [20508] Ajout de l'accesseur CashFlowCalculationMethod
        public CashFlowCalculationMethodEnum CashFlowCalculationMethod
        {
            get
            {
                CashFlowCalculationMethodEnum cashFlowCalculationMethodEnum = CashFlowCalculationMethodEnum.OVERALL;
                if (false == Convert.IsDBNull(GetFirstRowColumnValue("CASHFLOWCALCMETHOD")))
                {
                    string cashFlowCalculationMethod = Convert.ToString(GetFirstRowColumnValue("CASHFLOWCALCMETHOD"));
                    cashFlowCalculationMethodEnum = (CashFlowCalculationMethodEnum)StringToEnum.Parse(cashFlowCalculationMethod, cashFlowCalculationMethodEnum);
                }
                return cashFlowCalculationMethodEnum;
            }
        }

        /// <summary>
        /// Obtient la méthode de livraison
        /// </summary>
        // PM 20180219 [23824] Ajout de l'accesseur SettlementMethod
        public SettlMethodEnum SettlementMethod
        {
            get { return ReflectionTools.ConvertStringToEnum<SettlMethodEnum>(Convert.ToString(GetFirstRowColumnValue("SETTLTMETHOD"))); }
        }

        /// <summary>
        /// Obtient le style d'exercice
        /// </summary>
        // PM 20180219 [23824] Ajout de l'accesseur ExerciseStyle
        public Nullable<DerivativeExerciseStyleEnum> ExerciseStyle
        {
            get { return ReflectionTools.ConvertStringToEnumOrNullable<DerivativeExerciseStyleEnum>(Convert.ToString(GetFirstRowColumnValue("EXERCISESTYLE"))); }
        }

        /// <summary>
        /// Obtient l'id de la méthode de calcul du déposit pour cet asset
        /// </summary>
        /// PM 20160404 [22116] Ajout IdImMethod
        public int IdImMethod
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDIMMETHOD")); }
        }

        /// <summary>
        /// Obtient le symbol électonique du Derivative Contract rattaché à l'asset (ou de son échéance ouverte)
        /// </summary>
        // PM 20190222 [24326] Ajout ElectronicContractSymbol
        public string ElectronicContractSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ELECCONTRACTSYMBOL")); }
        }

        /// <summary>
        /// Obtient le nombre de décimales du strike pour l'importation des ddonnées
        /// </summary>
        // PM 20190222 [24326] Ajout StrikeDecLocator
        public Nullable<int> StrikeDecLocator
        {
            //get { return Convert.ToInt32(GetFirstRowColumnValue("STRIKEDECLOCATOR")); }
            get
            {
                if (GetFirstRowColumnValue("STRIKEDECLOCATOR") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("STRIKEDECLOCATOR"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret = base.IsUseOrderBy;
                if (false == ret)
                {
                    ret =
                    (Id_In == 0)
                    && (
                           StrFunc.ContainsIn(ISINCode_In, "%")
                        || StrFunc.ContainsIn(AssetSymbol_In, "%")
                        );

                }
                return ret;
            }
        }
        
        /// <summary>
        /// Obtient ou définit le symbol utilisé pour la recherche de l'asset
        /// </summary>
        public string AssetSymbol_In
        {
            get { return _AssetSymbol_In; }
            set
            {
                if (_AssetSymbol_In != value)
                {
                    InitProperty(true);
                    _AssetSymbol_In = value;
                }
            }
        }
        
        /// <summary>
        /// Obtient ou définit le code ISIN utilisé pour la recherche de l'asset
        /// </summary>
        public string ISINCode_In
        {
            get { return _ISINCode_In; }
            set
            {
                if (_ISINCode_In != value)
                {
                    InitProperty(true);
                    _ISINCode_In = value;
                }
            }
        }
        
        /// <summary>
        ///  Obtient ou définit le marché utilisé pour la recherche de l'asset
        /// </summary>
        public int IdM_In
        {
            get { return _idM_In; }
            set
            {
                if (_idM_In != value)
                {
                    InitProperty(false);
                    _idM_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit le DC utilisé pour la recherche de l'asset
        /// </summary>
        public int IdDC_In
        {
            get { return _idDC_In; }
            set
            {
                if (_idDC_In != value)
                {
                    InitProperty(false);
                    _idDC_In = value;
                }
            }
        }
        
        /// <summary>
        ///  Obtient ou définit l'instrument utilisé pour la recherche de l'asset
        /// </summary>
        public int IdI_In
        {
            get { return _idI_In; }
            set
            {
                if (_idI_In != value)
                {
                    InitProperty(false);
                    _idI_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit l'attribut Put/Call utilisé pour la recherche de l'asset
        /// </summary>
        public Nullable<PutOrCallEnum> PutCall_In
        {
            get { return _putCall_In; }
            set
            {
                if (_putCall_In != value)
                {
                    InitProperty(false);
                    _putCall_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit le strike utilisé pour la recherche de l'asset
        /// </summary>
        public decimal Strike_In
        {
            get { return _strikePrice_In; }
            set
            {
                if (_strikePrice_In != value)
                {
                    InitProperty(false);
                    _strikePrice_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit l'échéance utilisé pour la recherche de l'asset
        /// </summary>
        public string MaturityMonthYear_In
        {
            get { return _maturityMonthYear_In; }
            set
            {
                if (_maturityMonthYear_In != value)
                {
                    InitProperty(false);
                    _maturityMonthYear_In = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20230615 [26398] Add 
        public int IdMaturityRule
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDMATURITYRULE")); }
        }

        #endregion properties

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _AssetSymbol_In = null;
                _ISINCode_In = null;
            }
            base.InitProperty(pAll);

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121123 refactoring 
        protected override void SetSQLWhere()
        {
            //
            base.SetSQLWhere();
            //IDM
            if (IdM_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), IdM_In);
            }
            //IDDC
            if (IdDC_In > 0)
            {
                ConstructWhere(SQLObject + ".IDDC=@IDDC");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDDC), IdDC_In);
            }
            //IDI
            if (IdI_In > 0)
            {
                ConstructWhere(SQLObject + ".IDI=@IDI");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDI), IdI_In);
            }
            //PUTCALL
            if (null != _putCall_In)
            {
                // FI 20180906 [24159] Suppression du UPPER 
                //ConstructWhere(DataHelper.SQLUpper(CS, SQLObject + "." + "PUTCALL") + "=@PUTCALL");
                ConstructWhere(SQLObject + ".PUTCALL=@PUTCALL");
                SetDataParameter(new DataParameter(CS, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),
                    ReflectionTools.ConvertEnumToString<PutOrCallEnum>(_putCall_In.Value));
            }
            //STRIKEPRICE
            if (_strikePrice_In > decimal.Zero)
            {
                ConstructWhere(SQLObject + ".STRIKEPRICE=@STRIKEPRICE");
                SetDataParameter(new DataParameter(CS, "STRIKEPRICE", DbType.Decimal), _strikePrice_In);
            }
            //MATURITYMONTHYEAR
            if (StrFunc.IsFilled(_maturityMonthYear_In))
            {
                //FI 20180906 [24159] Suppression du UPPER
                ConstructWhere(SQLObject + ".MATURITYMONTHYEAR=@MATURITYMONTHYEAR");
                //ConstructWhere(DataHelper.SQLUpper(CS, SQLObject + "." + "MATURITYMONTHYEAR") + "=@MATURITYMONTHYEAR");
                SetDataParameter(new DataParameter(CS, "MATURITYMONTHYEAR", DbType.AnsiString, 32), _maturityMonthYear_In);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20121123 add override of GetWhereIDType
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;
            //
            if (AssetSymbol_In != null)
            {
                column_In = DataParameter.ParameterEnum.ASSETSYMBOL;
                data_In = AssetSymbol_In;
            }
            else if (ISINCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.ISINCODE;
                data_In = ISINCode_In;
            }
            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }

        #endregion

        #region IAssetETDIdent
        int IAssetETDIdent.IdAsset => Id;

        int IDerivativeContractIdent.IdDC => IdDerivativeContract;

        int IDerivativeContractIdent.IdM => IdM;

        string IDerivativeContractIdent.ContractSymbol => DrvContract_Symbol;

        string IDerivativeContractIdent.ElectronicContractSymbol => ElectronicContractSymbol;

        string IDerivativeContractIdent.ContractAttribute => DrvContract_Attribute;

        string IDerivativeContractIdent.ContractCategory => DrvContract_Category;

        DerivativeContractTypeEnum IDerivativeContractIdent.ContractType => DrvContract_ContractType;

        SettlMethodEnum IDerivativeContractIdent.SettlementMethod => SettlementMethod;

        DerivativeExerciseStyleEnum? IDerivativeContractIdent.ExerciseStyle => ExerciseStyle;

        string IAssetETDIdent.MaturityMonthYear => Maturity_MaturityMonthYear;

        Nullable<DateTime> IAssetETDIdent.MaturityDate => Maturity_MaturityDateSys;

        PutOrCallEnum? IAssetETDIdent.PutCall => PutCallEnum;

        decimal IAssetETDIdent.StrikePrice => StrikePrice;

        int? IAssetETDIdent.StrikeDecLocator => StrikeDecLocator;

        string IAssetETDIdent.PriceCurrency => DrvContract_PriceCurrency;
        #endregion
    }

    /// <summary>
    /// Get an Asset Exchange Traded Fund
    /// </summary>
    /// <returns>void</returns>
    public class SQL_AssetExchangeTradedFund : SQL_AssetBase
    {
        #region private members
        private string _AssetSymbol_In = null;
        private string _ISINCode_In = null;
        private string _RICCode_In = null;
        private string _BBGCode_In = null;
        private string _NSINCode_In = null;
        private int _idM_In;
        private int _idI_In;
        #endregion private members

        #region constructors
        public SQL_AssetExchangeTradedFund(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetExchangeTradedFund(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetExchangeTradedFund(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetExchangeTradedFund(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_EXTRDFUND, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            switch (pIdType)
            {
                case IDType.AssetSymbol:
                    AssetSymbol_In = pIdentifier;
                    break;
                case IDType.IsinCode:
                    ISINCode_In = pIdentifier;
                    break;
                case IDType.RICCode:
                    RICCode_In = pIdentifier;
                    break;
                case IDType.BBGCode:
                    BBGCode_In = pIdentifier;
                    break;
                case IDType.NSINCode:
                    NSINCode_In = pIdentifier;
                    break;
            }
        }
        #endregion constructors

        #region public_property_get
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.ExchangeTradedFund; }
        }

        public string AssetSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYMBOL")); }
        }
        public string ISINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISINCODE")); }
        }
        /// <summary>
        /// Obtient le code RIC (Reuters Identification Code)
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code Bloomberg (Identifiant Bloomberg)
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        /// <summary>
        /// Obtient le code NSIN (National Securities Identification Number)
        /// See also NSINTypeCode
        /// </summary>
        public string NSINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINCODE")); }
        }
        /// <summary>
        /// Obtient le type de NSIN (National Securities Identification Number)
        /// eg. CUSIP, SEDOL, QUIK, Wertpapier, Dutch, Valoren, Sicovam, Belgian...
        /// </summary>
        public string NSINTypeCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINTYPECODE")); }
        }
        /// <summary>
        /// Obtient le code CFI (Classification of Financial Instruments)
        /// </summary>
        public string CFICode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CFICODE")); }
        }
        /// <summary>
        /// Obtient la date de lancement du fond
        /// </summary>
        public DateTime IssueDate
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTISSUE")); }
        }
        public string LocaleOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOCALEOFISSUE")); }
        }
        public string StateOrProvinceOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("STATEPROVINCEISSUE")); }
        }
        public string CountryOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRY_ISSUE")); }
        }
        public string Manager
        {
            get { return Convert.ToString(GetFirstRowColumnValue("manager_IDENTIFIER")); }
        }
        public string Administrator
        {
            get { return Convert.ToString(GetFirstRowColumnValue("administrator_IDENTIFIER")); }
        }
        public string MarketMaker
        {
            get { return Convert.ToString(GetFirstRowColumnValue("marketmaker_IDENTIFIER")); }
        }
        public string MarketMaker2
        {
            get { return Convert.ToString(GetFirstRowColumnValue("marketmaker2_IDENTIFIER")); }
        }
        public string[] MarketMakers
        {
            get 
            { 
                string marketMaker = MarketMaker;
                string marketMaker2 = MarketMaker2;
                if (String.IsNullOrEmpty(marketMaker) && String.IsNullOrEmpty(marketMaker2))
                    return null;
                else if (String.IsNullOrEmpty(marketMaker2))
                    return new string[] { MarketMaker };
                else if (String.IsNullOrEmpty(marketMaker))
                    return new string[] { MarketMaker2 };
                else
                    return new string[] { MarketMaker, MarketMaker2 };
            }
        }
        public int Asset_IDM_Related
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM_RELATED")); }
        }
        public int Asset_IDM_Options
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM_OPTIONS")); }
        }
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret = base.IsUseOrderBy;
                if (false == ret)
                {
                    ret =
                    (Id_In == 0)
                    && (
                           StrFunc.ContainsIn(ISINCode_In, "%")
                        || StrFunc.ContainsIn(AssetSymbol_In, "%")
                        || StrFunc.ContainsIn(RICCode_In, "%")
                        || StrFunc.ContainsIn(BBGCode_In, "%")
                        || StrFunc.ContainsIn(NSINCode_In, "%")
                        );

                }
                return ret;
            }
        }

        public string AssetSymbol_In
        {
            get { return _AssetSymbol_In; }
            set
            {
                if (_AssetSymbol_In != value)
                {
                    InitProperty(true);
                    _AssetSymbol_In = value;
                }
            }
        }
        public string ISINCode_In
        {
            get { return _ISINCode_In; }
            set
            {
                if (_ISINCode_In != value)
                {
                    InitProperty(true);
                    _ISINCode_In = value;
                }
            }
        }
        public string RICCode_In
        {
            get { return _RICCode_In; }
            set
            {
                if (_RICCode_In != value)
                {
                    InitProperty(true);
                    _RICCode_In = value;
                }
            }
        }
        public string BBGCode_In
        {
            get { return _BBGCode_In; }
            set
            {
                if (_BBGCode_In != value)
                {
                    InitProperty(true);
                    _BBGCode_In = value;
                }
            }
        }
        public string NSINCode_In
        {
            get { return _NSINCode_In; }
            set
            {
                if (_NSINCode_In != value)
                {
                    InitProperty(true);
                    _NSINCode_In = value;
                }
            }
        }
        public int IdM_In
        {
            get { return _idM_In; }
            set
            {
                if (_idM_In != value)
                {
                    InitProperty(false);
                    _idM_In = value;
                }
            }
        }
        public int IdI_In
        {
            get { return _idI_In; }
            set
            {
                if (_idI_In != value)
                {
                    InitProperty(false);
                    _idI_In = value;
                }
            }
        }
        #endregion

        #region Method
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _AssetSymbol_In = null;
                _ISINCode_In = null;
                _RICCode_In = null;
                _BBGCode_In = null;
                _NSINCode_In = null;
            }
            base.InitProperty(pAll);

        }

        protected override void SetSQLCol(string[] pCol)
        {
            base.SetSQLCol(pCol);
            AddSQLCol("manager.IDENTIFIER as manager_IDENTIFIER");
            AddSQLCol("administrator.IDENTIFIER as administrator_IDENTIFIER");
            AddSQLCol("marketmaker.IDENTIFIER as marketmaker_IDENTIFIER");
            AddSQLCol("marketmaker2.IDENTIFIER as marketmaker2_IDENTIFIER");
        }

        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            string sqlFrom = string.Empty;
            sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_MANAGER", "manager") + Cst.CrLf;
            sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_ADMINISTRATOR", "administrator") + Cst.CrLf;
            sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_MARKETMAKER", "marketmaker") + Cst.CrLf;
            sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_MARKETMAKER", "marketmaker2") + Cst.CrLf;

            ConstructFrom(sqlFrom);
        }

        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();
            //IDM
            if (IdM_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), IdM_In);
            }
            //IDI
            if (IdI_In > 0)
            {
                ConstructWhere(DataHelper.SQLIsNull(CS, SQLObject + ".IDI", "@IDI") + "=@IDI");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDI), IdI_In);
                // ASSETENV
                StrBuilder sqlWhere = new StrBuilder();
                sqlWhere += SQLCst.EXISTS_SELECT + Cst.OTCml_TBL.ASSETENV + " ae";
                sqlWhere += SQLCst.WHERE + "(ae.IDI = @IDI)" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDM is null)" + SQLCst.OR + "(" + SQLObject + ".IDM = ae.IDM))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDC is null)" + SQLCst.OR + "(" + SQLObject + ".IDC = ae.IDC))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDCOUNTRY is null)" + SQLCst.OR + "(" + SQLObject + ".IDCOUNTRY_ISSUE = ae.IDCOUNTRY))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.SECURITYGRP is null)" + SQLCst.OR + "(" + SQLObject + ".SECURITYGRP = ae.SECURITYGRP))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.SECURITYCLASS is null)" + SQLCst.OR + "(" + SQLObject + ".SECURITYCLASS = ae.SECURITYCLASS))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.CFICODE is null)" + SQLCst.OR + "(" + DataHelper.SQLIsNull(CS, SQLObject + ".CFICODE", "ae.CFICODE") + SQLCst.LIKE + "ae.CFICODE))" + Cst.CrLf;
                if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                    sqlWhere += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(CS, "ae", DtRefForDtEnabled) + Cst.CrLf;
                sqlWhere += ")" + Cst.CrLf;
                ConstructWhere(sqlWhere.ToString());
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;

            if (AssetSymbol_In != null)
            {
                column_In = DataParameter.ParameterEnum.SYMBOL;
                data_In = AssetSymbol_In;
            }
            else if (ISINCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.ISINCODE;
                data_In = ISINCode_In;
            }
            else if (RICCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.RICCODE;
                data_In = RICCode_In;
            }
            else if (BBGCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.BBGCODE;
                data_In = BBGCode_In;
            }
            else if (NSINCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.NSINCODE;
                data_In = NSINCode_In;
            }
            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetEquity : SQL_AssetBase
    {
        #region private members
        private string _AssetSymbol_In = null;
        private string _ISINCode_In = null;
        private string _RICCode_In = null;
        private string _BBGCode_In = null;
        private string _NSINCode_In = null;
        private int _idM_In;
        private int _idMRelated_In;
        private int _idMOption_In;
        private int _idI_In;
        #endregion private members

        #region constructors
        public SQL_AssetEquity(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetEquity(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetEquity(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetEquity(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_EQUITY, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            switch (pIdType)
            {
                case IDType.AssetSymbol:
                    AssetSymbol_In = pIdentifier;
                    break;
                case IDType.IsinCode:
                    ISINCode_In = pIdentifier;
                    break;
                case IDType.RICCode:
                    RICCode_In = pIdentifier;
                    break;
                case IDType.BBGCode:
                    BBGCode_In = pIdentifier;
                    break;
                case IDType.NSINCode:
                    NSINCode_In = pIdentifier;
                    break;
            }
        }
        #endregion constructors

        #region public_property_get
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.EquityAsset; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdInstrument
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDI")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AssetSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYMBOL")); }
        }
        /// <summary>
        /// Obtient le code ISIN de l'asset
        /// </summary>
        public string ISINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISINCODE")); }
        }
        /// <summary>
        /// Obtient le code RIC de l'asset
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code Bloomberg de l'asset
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        /// <summary>
        /// Obtient le code NSIN de l'asset
        /// (source = CUSIP, SEDOL, QUIK, Wertpapier, Dutch, Valoren, Sicovam, Belgian)
        /// </summary>
        public string NSINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINCODE")); }
        }
        /// <summary>
        /// Obtient le type de source du NSIN de l'asset 
        /// (source = CUSIP, SEDOL, QUIK, Wertpapier, Dutch, Valoren, Sicovam, Belgian)
        /// </summary>
        public string NSINTypeCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINTYPECODE")); }
        }
        /// <summary>
        /// Obtient le CFICODE de l'asset
        /// </summary>
        public string CFICode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CFICODE")); }
        }
        /// <summary>
        /// Obtient l'IDENTIFIER de l'emetteur
        /// </summary>
        public string Issuer
        {
            get { return Convert.ToString(GetFirstRowColumnValue("issuer_IDENTIFIER")); }
        }
        /// <summary>
        /// Obtient la date d'émission
        /// </summary>
        public DateTime IssueDate
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTISSUE")); }
        }
        /// <summary>
        /// Obtient la commune de l'émetteur
        /// </summary>
        public string LocaleOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("LOCALEOFISSUE")); }
        }
        /// <summary>
        /// Obtient le StateOrProvince de l'émetteur
        /// </summary>
        public string StateOrProvinceOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("STATEPROVINCEISSUE")); }
        }

        /// <summary>
        /// Obtient le
        /// </summary>
        public string CountryOfIssue
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRY_ISSUE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> StlOffsetMultiplier
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODMLTPSTLDOFFSET") == null)
                    return null;
                else
                    return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPSTLDOFFSET"));
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string StlOffsetPeriod
        {
            get
            {
                if (GetFirstRowColumnValue("PERIODSTLDOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("PERIODSTLDOFFSET"));
            }
        }
        
        /// <summary>
        ///  
        /// </summary>
        public string StlOffsetDaytype
        {
            get
            {
                if (GetFirstRowColumnValue("DAYTYPESTLDOFFSET") == null)
                    return string.Empty;
                else
                    return Convert.ToString(GetFirstRowColumnValue("DAYTYPESTLDOFFSET"));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int Asset_IDM_Related
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("rdc_IDM_RELATED")); }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Asset_IDM_Options
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("rdc_IDM_OPTIONS")); }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret = base.IsUseOrderBy;
                if (false == ret)
                {
                    ret =
                    (Id_In == 0)
                    && (
                           StrFunc.ContainsIn(ISINCode_In, "%")
                        || StrFunc.ContainsIn(AssetSymbol_In, "%")
                        || StrFunc.ContainsIn(RICCode_In, "%")
                        || StrFunc.ContainsIn(BBGCode_In, "%")
                        || StrFunc.ContainsIn(NSINCode_In, "%")
                        );

                }
                return ret;
            }
        }

        public string AssetSymbol_In
        {
            get { return _AssetSymbol_In; }
            set
            {
                if (_AssetSymbol_In != value)
                {
                    InitProperty(true);
                    _AssetSymbol_In = value;
                }
            }
        }
        public string ISINCode_In
        {
            get { return _ISINCode_In; }
            set
            {
                if (_ISINCode_In != value)
                {
                    InitProperty(true);
                    _ISINCode_In = value;
                }
            }
        }
        public string RICCode_In
        {
            get { return _RICCode_In; }
            set
            {
                if (_RICCode_In != value)
                {
                    InitProperty(true);
                    _RICCode_In = value;
                }
            }
        }
        public string BBGCode_In
        {
            get { return _BBGCode_In; }
            set
            {
                if (_BBGCode_In != value)
                {
                    InitProperty(true);
                    _BBGCode_In = value;
                }
            }
        }
        public string NSINCode_In
        {
            get { return _NSINCode_In; }
            set
            {
                if (_NSINCode_In != value)
                {
                    InitProperty(true);
                    _NSINCode_In = value;
                }
            }
        }
        public int IdM_In
        {
            get { return _idM_In; }
            set
            {
                if (_idM_In != value)
                {
                    InitProperty(false);
                    _idM_In = value;
                }
            }
        }
        public int IdMRelated_In
        {
            get { return _idMRelated_In; }
            set
            {
                if (_idMRelated_In != value)
                {
                    InitProperty(false);
                    _idMRelated_In = value;
                }
            }
        }
        public int IdMOption_In
        {
            get { return _idMOption_In; }
            set
            {
                if (_idMOption_In != value)
                {
                    InitProperty(false);
                    _idMOption_In = value;
                }
            }
        }
        
        public int IdI_In
        {
            get { return _idI_In; }
            set
            {
                if (_idI_In != value)
                {
                    InitProperty(false);
                    _idI_In = value;
                }
            }
        }
        /// <summary>
        /// Use to display info on capture under asset textbox
        /// </summary>
        /// EG 20140526
        public override string SetDisplay
        {
            get
            {
                string display = base.SetDisplay;
                if (StrFunc.IsFilled(ISINCode) && (false == Description.Contains(ISINCode)))
                    display += Cst.Space + "/" + Cst.Space + ISINCode;
                return display;
            }
        }

        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _AssetSymbol_In = null;
                _ISINCode_In = null;
                _RICCode_In = null;
                _BBGCode_In = null;
                _NSINCode_In = null;
            }
            base.InitProperty(pAll);

        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLCol(string[] pCol)
        {
            base.SetSQLCol(pCol);
            AddSQLCol("issuer.IDENTIFIER as issuer_IDENTIFIER");
            if ((IdMRelated_In > 0) || (IdMOption_In > 0))
            {
                AddSQLCol("rdc.IDM_RELATED as rdc_IDM_RELATED");
                AddSQLCol("rdc.IDM_OPTIONS as rdc_IDM_OPTIONS");
            }
        }

        /// <summary>
        /// Jointure sur l'acteur Emetteur
        /// Jointure sur le table des marchés de dérivés listés liés à cet actif
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            string sqlFrom = OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ACTOR, false, SQLObject + ".IDA_ISSUER", "issuer") + Cst.CrLf;

            if ((IdMRelated_In > 0) || (IdMOption_In > 0))
                sqlFrom += OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ASSET_EQUITY_RDCMK, SQLJoinTypeEnum.Left, this.SQLObject + ".IDASSET", "rdc", DataEnum.All);


            ConstructFrom(sqlFrom);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121123 refactoring
        protected override void SetSQLWhere()
        {
            //
            base.SetSQLWhere();
            //IDM
            if (IdM_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), IdM_In);
            }
            //IDM_RELATED
            if (IdMRelated_In > 0)
            {
                ConstructWhere("rdc.IDM_RELATED=@IDM_RELATED");
                SetDataParameter(new DataParameter(CS, "IDM_RELATED", DbType.Int32), IdMRelated_In);
            }
            //IDM_OPTION
            if (IdMOption_In > 0)
            {
                ConstructWhere("rdc.IDM_OPTIONS=@IDM_OPTIONS");
                SetDataParameter(new DataParameter(CS, "IDM_OPTIONS", DbType.Int32), IdMOption_In);
            }
            //IDI
            if (IdI_In > 0)
            {
                ConstructWhere(DataHelper.SQLIsNull(CS, SQLObject + ".IDI", "@IDI") + "=@IDI");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDI), IdI_In);
                // ASSETENV
                StrBuilder sqlWhere = new StrBuilder();
                sqlWhere += SQLCst.EXISTS_SELECT + Cst.OTCml_TBL.ASSETENV + " ae";
                sqlWhere += SQLCst.WHERE + "(ae.IDI = @IDI)" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDM is null)" + SQLCst.OR + "(" + SQLObject + ".IDM = ae.IDM))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDC is null)" + SQLCst.OR + "(" + SQLObject + ".IDC = ae.IDC))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.IDCOUNTRY is null)" + SQLCst.OR + "(" + SQLObject + ".IDCOUNTRY_ISSUE = ae.IDCOUNTRY))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.SECURITYGRP is null)" + SQLCst.OR + "(" + SQLObject + ".SECURITYGRP = ae.SECURITYGRP))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.SECURITYCLASS is null)" + SQLCst.OR + "(" + SQLObject + ".SECURITYCLASS = ae.SECURITYCLASS))" + Cst.CrLf;
                sqlWhere += SQLCst.AND + "((ae.CFICODE is null)" + SQLCst.OR + "(" + DataHelper.SQLIsNull(CS, SQLObject + ".CFICODE", "ae.CFICODE") + SQLCst.LIKE + "ae.CFICODE))" + Cst.CrLf;
                if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                    sqlWhere += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(CS, "ae", DtRefForDtEnabled) + Cst.CrLf;
                sqlWhere += ")" + Cst.CrLf;
                ConstructWhere(sqlWhere.ToString());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20121123 add override of GetWhereIDType
        protected override SQLWhere GetWhereIDType()
        {

            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;
            //
            if (AssetSymbol_In != null)
            {
                column_In = DataParameter.ParameterEnum.SYMBOL;
                data_In = AssetSymbol_In;
            }
            else if (ISINCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.ISINCODE;
                data_In = ISINCode_In;
            }
            else if (RICCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.RICCODE;
                data_In = RICCode_In;
            }
            else if (BBGCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.BBGCODE;
                data_In = BBGCode_In;
            }
            else if (NSINCode_In != null)
            {
                column_In = DataParameter.ParameterEnum.NSINCODE;
                data_In = NSINCode_In;
            }
            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// EG 20140116 Report v3.7 
    public class SQL_AssetIndex : SQL_AssetBase
    {
        #region public_property_get
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.Index; }
        }

        public string AssetSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYMBOL")); }
        }
        /// <summary>
        /// Obtient le code ISIN de l'asset
        /// </summary>
        public string ISINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISINCODE")); }
        }
        /// <summary>
        /// Obtient le code RIC de l'asset
        /// </summary>
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code Bloomberg de l'asset
        /// </summary>
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        /// <summary>
        /// Obtient le code NSIN de l'asset
        /// (source = CUSIP, SEDOL, QUIK, Wertpapier, Dutch, Valoren, Sicovam, Belgian)
        /// </summary>
        public string NSINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINCODE")); }
        }
        /// <summary>
        /// Obtient le type de source du NSIN de l'asset 
        /// (source = CUSIP, SEDOL, QUIK, Wertpapier, Dutch, Valoren, Sicovam, Belgian)
        /// </summary>
        public string NSINTypeCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("NSINTYPECODE")); }
        }
        /// <summary>
        /// Use to display info on capture under asset textbox
        /// </summary>
        /// EG 20140526
        public override string SetDisplay
        {
            get
            {
                string display = base.SetDisplay;
                if (StrFunc.IsFilled(ISINCode) && (false == Description.Contains(ISINCode)))
                    display += Cst.Space + "/" + Cst.Space + ISINCode;
                return display;
            }
        }

        #endregion
        #region constructors
        public SQL_AssetIndex(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetIndex(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetIndex(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetIndex(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_INDEX, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }



    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetDeposit : SQL_AssetBase
    {
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.Deposit; }
        }

        #region constructors
        public SQL_AssetDeposit(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetDeposit(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetDeposit(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetDeposit(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_DEPOSIT, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetMutualFund : SQL_AssetBase
    {
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.MutualFund; }
        }

        #region constructors
        public SQL_AssetMutualFund(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetMutualFund(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetMutualFund(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetMutualFund(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_MUTUALFUND, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }

    /// <summary>
    /// Get an Asset Fx Rate
    /// </summary>
    /// EG 20150302 Add ContractMultiplier (CFD Forex)
    /// FI 201500313 [XXXXX] Modify 
    public class SQL_AssetFxRate : SQL_AssetBase
    {
        #region private Members
        private string _currency1_In = null;
        private string _currency2_In = null;
        #endregion Memebers
        #region Constructors
        public SQL_AssetFxRate(string pSource, int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetFxRate(string pSource, int pId, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pIsScanDataEnabled) { }
        public SQL_AssetFxRate(string pSource, string pIdentifier)
            : this(pSource, IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetFxRate(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_FXRATE, pIdType, pIdentifier, pIsScanDataEnabled)
        {
        }
        public SQL_AssetFxRate(string pSource, string pCurrency1, string pCurrency2, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.ASSET_FXRATE, IDType.CurrencyPair, pCurrency1 + "|" + pCurrency2, pIsScanDataEnabled)
        {
            Currency1_In = pCurrency1;
            Currency2_In = pCurrency2;

        }
        #endregion Constructors

        #region public_property_get
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.FxRateAsset; }
        }
        /// <summary>
        /// Obtient l'identifier court
        /// </summary>
        /// FI 201500313 [XXXXX] Add
        public string ShortIdentifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SHORTIDENTIFIER")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string QCP_Cur1
        {
            get { return Convert.ToString(GetFirstRowColumnValue("QCP_IDC1")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string QCP_Cur2
        {
            get { return Convert.ToString(GetFirstRowColumnValue("QCP_IDC2")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string QCP_QuoteBasis
        {
            get { return Convert.ToString(GetFirstRowColumnValue("QCP_QUOTEBASIS")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public QuoteBasisEnum QCP_QuoteBasisEnum
        {
            get
            {
                string quoteBasis = QCP_QuoteBasis;
                if (Enum.IsDefined(typeof(QuoteBasisEnum), quoteBasis))
                    return (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), quoteBasis, true);
                else
                    return QuoteBasisEnum.Currency1PerCurrency2;
            }
        }
        /// EG 20150302 New (CFD Forex)
        public int ContractMultiplier
        {
            get 
            {
                int contractMultiplier = 1;
                if (null != GetFirstRowColumnValue("CONTRACTMULTIPLIER"))
                    contractMultiplier = Convert.ToInt32(GetFirstRowColumnValue("CONTRACTMULTIPLIER"));
                return contractMultiplier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PrimaryRateSrc
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PRIMARYRATESRC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PrimaryRateSrcPage
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PRIMARYRATESRCPAGE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PrimaryRateSrcHead
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PRIMARYRATESRCHEAD")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SecondaryRateSrc
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SECONDARYRATESRC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SecondaryRateSrcPage
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SECONDARYRATESRCPAGE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SecondaryRateSrcHead
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SECONDARYRATESRCHEAD")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IdBC_RateSrc
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBCRATESRC")); }
        }

        /// EG 20150302 New (CFD Forex)
        public override DateTime TimeRateSrc
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("TIMERATESRC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int PeriodMultiplierSettlementTerm
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPSETTLTTERM")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PeriodSettlementTerm
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PERIODSETTLTTERM")); }
        }
        #endregion public_property_get

        // EG 20150309 POC - BERKELEY
        public Nullable<decimal> PercentageInPoint
        {
            get
            {
                if (null == GetFirstRowColumnValue("PIP"))
                    return null;
                return Convert.ToDecimal(GetFirstRowColumnValue("PIP"));
            }
        }

        public string Currency1_In
        {
            get { return _currency1_In; }
            set
            {
                if (_currency1_In != value)
                {
                    base.InitProperty(true);
                    _currency1_In = value;
                }
            }
        }
        public string Currency2_In
        {
            get { return _currency2_In; }
            set
            {
                if (_currency2_In != value)
                {
                    base.InitProperty(true);
                    _currency2_In = value;
                }
            }
        }

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20121123 add override of GetWhereIDType
        protected override SQLWhere GetWhereIDType()
        {

            SQLWhere sqlWhere = new SQLWhere();
            if (StrFunc.IsFilled(Currency1_In) && StrFunc.IsFilled(Currency2_In))
            {
                string cur1PerCur2 = "({0}.QCP_IDC1 = @QCP_IDC1) and ({0}.QCP_IDC2 = @QCP_IDC2) and ({0}.QCP_QUOTEBASIS = @QCP_QUOTEBASIS_PER_IDC2)";
                string cur2PerCur1 = "({0}.QCP_IDC1 = @QCP_IDC2) and ({0}.QCP_IDC2 = @QCP_IDC1) and ({0}.QCP_QUOTEBASIS = @QCP_QUOTEBASIS_PER_IDC1)";
                sqlWhere.Append(String.Format("(" + cur1PerCur2 + " or " + cur2PerCur1 + ") and ({0}.ISDEFAULT = @ISDEFAULT)", SQLObject));
                SetDataParameter(new DataParameter(CS, "QCP_IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN), Currency1_In);
                SetDataParameter(new DataParameter(CS, "QCP_IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), Currency2_In);
                SetDataParameter(new DataParameter(CS, "QCP_QUOTEBASIS_PER_IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), QuoteBasisEnum.Currency1PerCurrency2.ToString());
                SetDataParameter(new DataParameter(CS, "QCP_QUOTEBASIS_PER_IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN), QuoteBasisEnum.Currency2PerCurrency1.ToString());
                SetDataParameter(new DataParameter(CS, "ISDEFAULT", DbType.Boolean), true);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }
            return sqlWhere;
        }

        #endregion Methods

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>void</returns>
    /// 
    public class SQL_AssetRateIndex : SQL_AssetBase
    {
        #region Enum
        public new enum IDType
        {
            IDASSET,
            Asset_Identifier,
            IDRX,
            RateIndex_Identifier,
            RateIndex_IDISDA
        }
        #endregion Enum

        #region members
        //
        private int _idASSET_In;
        private string _asset_identifier_In;
        private int _asset_periodmltp_In;
        private string _asset_period_In;
        //
        private int _idRX_In;
        private string _idx_identifier_In;
        private string _idx_idisda_In;
        //
        private Cst.IndexSelfCompounding _withInfoSelfCompounding = Cst.IndexSelfCompounding.NONE;
        private readonly SQL_EfsObject _tblAssetRateIndex;
        #endregion

        #region constructors
        public SQL_AssetRateIndex(string pSource, IDType pIdType, int pId)
            : this(pSource, pIdType, pId.ToString(), ScanDataDtEnabledEnum.No)
        { }
        public SQL_AssetRateIndex(string pSource, IDType pIdType, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pIdType, pId.ToString(), pScanDataEnabled)
        { }
        public SQL_AssetRateIndex(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No)
        { }
        public SQL_AssetRateIndex(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_ASSET_RATEINDEX, SQL_TableWithID.IDType.UNDEFINED, pIdentifier, pScanDataEnabled)
        {
            //
            //			base.Cs = pSource;
            //			base.SQLObject = Cst.OTCml_TBL.VW_ASSET_RATEINDEX.ToString(); 
            //			base.ScanDataDtEnabled  = pScanDataEnabled;
            //			//
            switch (pIdType)
            {
                case IDType.IDASSET:
                    try
                    {
                        IdASSET_In = int.Parse(pIdentifier);
                    }
                    catch
                    {
                        IdASSET_In = 0;
                    }
                    break;
                case IDType.Asset_Identifier:
                    Asset_Identifier_In = pIdentifier;
                    break;
                case IDType.IDRX:
                    try
                    {
                        IdRX_In = int.Parse(pIdentifier);
                    }
                    catch
                    {
                        IdRX_In = 0;
                    }
                    break;
                case IDType.RateIndex_Identifier:
                    Idx_Identifier_In = pIdentifier;
                    break;
                case IDType.RateIndex_IDISDA:
                    Idx_IdIsda_In = pIdentifier;
                    break;
            }
            //
            if (this.IsUseOrderBy)
            {
                //20090724 FI SetCacheOn
                _tblAssetRateIndex = new SQL_EfsObject(CSTools.SetCacheOn(CS), Cst.OTCml_TBL.ASSET_RATEINDEX.ToString());
                _tblAssetRateIndex.LoadTable();
            }
            //
        }
        #endregion constructors

        #region properties get
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.RateIndex; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsUseOrderBy
        {
            get
            {
                //Critère sur la PK 
                //--> La query retournera 1 ou 0 record  
                return ((IdASSET_In == 0));
            }
        }

        /// <summary>
        /// Obtient l'id de l'asset
        /// </summary>
        public override int IdAsset
        {
            get
            {

                return Convert.ToInt32(GetFirstRowColumnValue("Asset_IDASSET"));
            }
        }

        /// <summary>
        /// Obtient l'identifier de l'asset
        /// </summary>
        public override string Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_IDENTIFIER")); }
        }

        /// <summary>
        /// Obtient le nom affiché de l'asset
        /// </summary>
        public override string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_DISPLAYNAME")); }
        }

        /// <summary>
        /// Obtient la description de l'asset
        /// </summary>
        public override string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_DESCRIPTION")); }
        }

        /// <summary>
        /// Obtient la devise de l'asset
        /// </summary>
        public override string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_IDC")); }
        }

        /// <summary>
        /// Obtient la devise de l'asset
        /// </summary>
        /// FI 20140903 [20275] add property
        public override string ExtlLink
        {

            get { return Convert.ToString(GetFirstRowColumnValue("Asset_EXTLLINK")); }
        }
        
        /// <summary>
        /// Obtient l'id du marché
        /// </summary>
        public override int IdM
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("Asset_IDM")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ClearanceSystem
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_CLEARANCESYSTEM")); }
        }

        public string Asset_Period_Tenor
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_PERIOD")); }

        }

        public string Asset_PeriodMltpCalcDt
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_PERIODMLTPCALCDT")); }
        }

        public string Asset_PeriodCalcDt
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_PERIODCALCDT")); }

        }

        public string Asset_RollConvCalcDt
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Asset_ROLLCONVCALCDT")); }
        }

        public PeriodEnum FpML_Enum_Period_Tenor
        {
            get { return StringToEnum.Period(Asset_Period_Tenor); }
        }

        public int Asset_PeriodMltp_Tenor
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("Asset_PERIODMLTP")); }
        }

        public string Asset_WeeklyRollConvResetDT
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Asset_WEEKLYROLLCONVRESETDT"]);
                else
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Asset_IdYieldCurveDef
        {
            get { return Convert.ToString((GetFirstRowColumnValue("Asset_IDYIELDCURVEDEF"))); }
        }

        public bool Asset_RateIndexWithTenor
        {
            get
            {
                // EG 20160404 Migration vs2013
                // #warning 09/11/2006 PL A revoir peut-être paramétrage de EUR-EONIA-OIS-COMPOUND Car Tenor vaut 1D En attendant
                return (false == Idx_IsSelfCompounding);
                //return ( StrFunc.IsFilled (this.Asset_Period_Tenor));
            }
        }

        /// <summary>
        /// Obtient l'asset de substitution pour lire les cotations dans les historiques de prix
        /// <para>Obtient 0 s'il est non renseigné</para>
        /// </summary>
        public int Asset_IdAsset_Asset
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("Asset_IDASSET_ASSET")); }
        }
        public int IdRX
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["Idx_IDRX"]);
                else
                    return 0;
            }
        }
        public string Idx_Identifier
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_IDENTIFIER"]);
                else
                    return string.Empty;
            }
        }
        public string Idx_Displayname
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_DISPLAYNAME"]);
                else
                    return null;
            }
        }
        public string Idx_Description
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_DESCRIPTION"]);
                else
                    return null;
            }
        }
        public string Idx_ExtlLink
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_EXTLLINK"]);
                else
                    return null;
            }
        }
        public string Idx_SrcSupplierId
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_SRCSUPPLIERID"]);
                else
                    return null;
            }
        }
        public string Idx_SrcSupplierPageId
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_SRCSUPPLIERPAGEID"]);
                else
                    return null;
            }
        }
        public string Idx_SrcSupplierHeadId
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_SRCSUPPLIERHEADID"]);
                else
                    return null;
            }
        }
        public string Idx_RateType
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_RATETYPE"]);
                else
                    return null;
            }
        }
        public string Idx_CalculationRule
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_CALCULATIONRULE"]);
                else
                    return null;
            }
        }
        public string Idx_IdIsda
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_IDISDA"]);
                else
                    return string.Empty;
            }
        }
        public string Idx_IdBc
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_IDBC"]);
                else
                    return string.Empty;
            }
        }
        public string Idx_DayCountFraction
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_DCF"]);
                else
                    return null;
            }
        }
        public string Idx_DayCountFractionIsda
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_DCFISDA"]);
                else
                    return null;
            }
        }

        public DayCountFractionEnum FpML_Enum_DayCountFraction
        {
            get { return StringToEnum.DayCountFraction(Idx_DayCountFraction); }
        }
        public string Idx_BusinessDayConvention_Payment
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_BDC_PAYMENT"]);
                else
                    return null;
            }
        }
        public BusinessDayConventionEnum FpML_Enum_BusinessDayConvention_Payment
        {
            get { return StringToEnum.BusinessDayConvention(Idx_BusinessDayConvention_Payment); }
        }
        public string Idx_BusinessDayConvention_CalcPeriod
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_BDC_CALCPERIOD"]);
                else
                    return null;
            }
        }
        public BusinessDayConventionEnum FpML_Enum_BusinessDayConvention_CalcPeriod
        {
            get { return StringToEnum.BusinessDayConvention(Idx_BusinessDayConvention_CalcPeriod); }
        }
        public string Idx_DayTypeFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_DAYTYPEFIXINGOFFSET"]);
                else
                    return null;
            }
        }
        public DayTypeEnum FpML_Enum_DayTypeFixingOffset
        {
            get { return StringToEnum.DayType(Idx_DayTypeFixingOffset); }
        }
        public string Idx_PeriodFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_PERIODFIXINGOFFSET"]);
                else
                    return null;
            }
        }
        public PeriodEnum FpML_Enum_PeriodFixingOffset
        {
            get { return StringToEnum.Period(Idx_PeriodFixingOffset); }
        }
        public int Idx_PeriodMlptFixingOffset
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["Idx_PERIODMLTPFIXINGOFFSET"]);
                else
                    return 0;
            }
        }
        public string Idx_BusinessCenter
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_IDBC"]);
                else
                    return null;
            }
        }
        public string Idx_BusinessCenterAdditional
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_IDBCADDITIONAL"]);
                else
                    return null;
            }
        }

        public bool Idx_IsSelfCompounding
        {
            get
            {
                bool isSelfCompounding = false;
                if (IsLoaded)
                    isSelfCompounding = (Dt.Rows[0]["Idx_CALCULATIONRULE"].ToString() == Cst.CalculationRule_SELFCOMPOUNDING);
                return isSelfCompounding;
            }
        }
        public string Idx_RelativeToPaymentDt
        {
            get
            {

                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_RELATIVETOPAYMENTDT"]);
                else
                    return null;
            }
        }

        public string Idx_RelativeToResetDt
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["Idx_RELATIVETORESETDT"]);
                else
                    return null;
            }
        }

        public int Idx_RoundPrec
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(FirstRow["Idx_ROUNDPREC"]);
                else
                    return 6;
            }
        }
        public string Idx_RoundDir
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(FirstRow["Idx_ROUNDDIR"]);
                else
                    return Cst.RoundingDirectionSQL.N.ToString();
            }
        }
        public string Idx_IndexUnit
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(FirstRow["Idx_INDEXUNIT"]);
                else
                    return Cst.IdxUnit_NA.ToString();
            }
        }

        public int SelfAvg_RoundPrec
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(FirstRow["ROUNDPRECSELFAVG"]);
                else
                    return 6;
            }
        }
        public string SelfAvg_RoundDir
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(FirstRow["ROUNDDIRSELFAVG"]);
                else
                    return Cst.RoundingDirectionSQL.N.ToString();
            }
        }
        #endregion

        #region properties get_set
        /// <summary>
        /// 
        /// </summary>
        public Cst.IndexSelfCompounding WithInfoSelfCompounding
        {
            get { return _withInfoSelfCompounding; }
            set
            {
                if (_withInfoSelfCompounding != value)
                {
                    InitProperty(false);
                    _withInfoSelfCompounding = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int IdASSET_In
        {
            get
            { return _idASSET_In; }
            set
            {
                if (_idASSET_In != value)
                {
                    InitProperty(true);
                    _idASSET_In = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Asset_Identifier_In
        {
            get { return _asset_identifier_In; }
            set
            {
                if (_asset_identifier_In != value)
                {
                    InitProperty(true);
                    _asset_identifier_In = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Asset_PeriodMltp_In
        {
            get { return _asset_periodmltp_In; }
            set { _asset_periodmltp_In = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Asset_Period_In
        {
            get { return _asset_period_In; }
            set { _asset_period_In = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdRX_In
        {
            get
            { return _idRX_In; }
            set
            {
                if (_idRX_In != value)
                {
                    InitProperty(true);
                    _idRX_In = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Idx_Identifier_In
        {
            get { return _idx_identifier_In; }
            set
            {
                if (_idx_identifier_In != value)
                {
                    InitProperty(true);
                    _idx_identifier_In = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Idx_IdIsda_In
        {
            get { return _idx_idisda_In; }
            set
            {
                if (_idx_idisda_In != value)
                {
                    InitProperty(true);
                    _idx_idisda_In = value;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idASSET_In = 0;
                _asset_identifier_In = null;
                _asset_period_In = null;
                _asset_periodmltp_In = 0;
                _idRX_In = 0;
                _idx_identifier_In = null;
                _idx_idisda_In = null;
            }
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLCol(string[] pCol)
        {
            base.SetSQLCol(pCol);
            //
            if (Cst.IndexSelfCompounding.NONE != WithInfoSelfCompounding)
                AddSQLCol("self.*");

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            //
            if (_asset_periodmltp_In != 0)
                sqlWhere.Append("Asset_PERIODMLTP=" + _asset_periodmltp_In.ToString());
            if (_asset_period_In != null)
                sqlWhere.Append("Asset_PERIOD=" + DataHelper.SQLString(_asset_period_In));
            if (_idASSET_In != 0)
                sqlWhere.Append("Asset_IDASSET=" + _idASSET_In.ToString());
            else if (_asset_identifier_In != null)
            {
                Boolean isCaseInsensitive = IsUseCaseInsensitive(_asset_identifier_In);
                if (isCaseInsensitive)
                    sqlWhere.Append(DataHelper.SQLUpper(CS, "Asset_IDENTIFIER") + DataHelper.SQLLike(_asset_identifier_In, CompareEnum.Upper));
                else
                    sqlWhere.Append("Asset_IDENTIFIER" + DataHelper.SQLLike(_asset_identifier_In, CompareEnum.Normal));
            }
            else if (_idRX_In != 0)
            {
                sqlWhere.Append("Idx_IDRX=" + _idRX_In.ToString());
            }
            else if (_idx_identifier_In != null)
            {
                Boolean isCaseInsensitive = IsUseCaseInsensitive(_idx_identifier_In);
                if (isCaseInsensitive)
                    sqlWhere.Append(DataHelper.SQLUpper(CS, "Idx_IDENTIFIER") + DataHelper.SQLLike(_idx_identifier_In, CompareEnum.Upper));
                else
                    sqlWhere.Append("Idx_IDENTIFIER" + DataHelper.SQLLike(_idx_identifier_In, CompareEnum.Normal));
            }
            else if (_idx_idisda_In != null)
            {
                Boolean isCaseInsensitive = IsUseCaseInsensitive(_idx_idisda_In);
                if (isCaseInsensitive)
                    sqlWhere.Append(DataHelper.SQLUpper(CS, "Idx_IDISDA") + DataHelper.SQLLike(_idx_idisda_In, CompareEnum.Upper));
                else
                    sqlWhere.Append("Idx_IDISDA" + DataHelper.SQLLike(_idx_idisda_In, CompareEnum.Normal));
            }
            else
                sqlWhere.Append("1=2");

            InitSQLWhere(sqlWhere.ToString(), false);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLOrderBy()
        {

            string sqlColumOrderBy = string.Empty;
            string orderBy = string.Empty;
            //
            if (this.IsUseOrderBy)
            {
                if (_asset_identifier_In != null)
                    //Critère like sur Identifier --> La query retournera n record --> Order by sur identifier
                    sqlColumOrderBy = "Asset_IDENTIFIER";
                else if (_idRX_In != 0)
                    sqlColumOrderBy = "Asset_IDENTIFIER";
                else if (_idx_identifier_In != null)
                    sqlColumOrderBy = "Idx_IDENTIFIER";
                else if (_idx_idisda_In != null)
                    sqlColumOrderBy = "Idx_IDISDA";

                //Statistics: table xxx_S
                if (_tblAssetRateIndex.IsWithStatistic)
                    orderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, _tblAssetRateIndex.ObjectName, sqlColumOrderBy);
                else
                    orderBy = sqlColumOrderBy;
            }
            base.ConstructOrderBy(orderBy);

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {

            base.SetSQLFrom();
            //
            Cst.OTCml_TBL tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_CF;
            switch (WithInfoSelfCompounding)
            {
                case Cst.IndexSelfCompounding.CASHFLOW:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_CF;
                    break;
                case Cst.IndexSelfCompounding.ACCRUEDINTEREST:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_AI;
                    break;
                case Cst.IndexSelfCompounding.VALORISATION:
                    tblSelfCompounding = Cst.OTCml_TBL.SELFCOMPOUNDING_V;
                    break;
            }
            //
            string sqlFrom = string.Empty;
            if (Cst.IndexSelfCompounding.NONE != WithInfoSelfCompounding)
                sqlFrom = OTCmlHelper.GetSQLJoin(CS, tblSelfCompounding, SQLJoinTypeEnum.Left, this.SQLObject + ".Idx_IDRX", "self", DataEnum.All);
            //Statistics 
            if (this.IsUseOrderBy && (_tblAssetRateIndex.IsWithStatistic))
            {
                sqlFrom += Cst.CrLf + OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.ASSET_RATEINDEX, true, SQLObject + ".Asset_IDASSET", "asset", false);
                sqlFrom += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(Cst.OTCml_TBL.ASSET_RATEINDEX.ToString(), "asset");
            }
            //			
            ConstructFrom(sqlFrom);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetSQLJoinMarket()
        {
            return OTCmlHelper.GetSQLJoin(CS, Cst.OTCml_TBL.MARKET, false, SQLObject + ".Asset_IDM", AliasMarketTable) + Cst.CrLf;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetForCurve : SQL_AssetBase
    {
        #region private variable
        private Cst.UnderlyingAsset_Rate _assetCategory_In;
        #endregion private variable

        #region constructors
        public SQL_AssetForCurve(string pSource, Cst.UnderlyingAsset_Rate pAssetCategory, int pId)
            : this(pSource, pAssetCategory, SQL_TableWithID.IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetForCurve(string pSource, Cst.UnderlyingAsset_Rate pAssetCategory, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, pAssetCategory, SQL_TableWithID.IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetForCurve(string pSource, Cst.UnderlyingAsset_Rate pAssetCategory, string pIdentifier)
            : this(pSource, pAssetCategory, SQL_TableWithID.IDType.Identifier, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetForCurve(string pSource, Cst.UnderlyingAsset_Rate pAssetCategory, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_ASSET_FORCURVE, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            _assetCategory_In = pAssetCategory;
        }
        #endregion constructors

        #region public_property_get

        public int PeriodMltpCurve
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("PERIODMLTPCURVE")); }
        }
        public string PeriodCurve
        {
            get { return Convert.ToString(GetFirstRowColumnValue("PERIODCURVE")); }
        }
        public PeriodEnum FpML_Enum_PeriodCurve
        {
            get { return StringToEnum.Period(PeriodCurve); }
        }
        public string DcfCurve
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DCFCURVE")); }
        }
        public DayCountFractionEnum FpML_Enum_DcfCurve
        {
            get { return StringToEnum.DayCountFraction(DcfCurve); }
        }
        public string BdcCurve
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BDCCURVE")); }
        }
        public string RollConventionCurve
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ROLLCONVCURVE")); }
        }
        public BusinessDayConventionEnum FpML_Enum_BdcCurve
        {
            get { return StringToEnum.BusinessDayConvention(BdcCurve); }
        }

        public DateTime DtTerm
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTTERM")); }
        }
        public RollConventionEnum FpML_Enum_RollConventionCurve
        {
            get { return StringToEnum.RollConvention(RollConventionCurve); }
        }
        #endregion public_property_get

        #region public_property_get_set
        public Cst.UnderlyingAsset_Rate AssetCategory_In
        {
            get
            { return _assetCategory_In; }
            set
            {
                if (_assetCategory_In != value)
                {
                    InitProperty(true);
                    _assetCategory_In = value;
                }
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {

            if (pAll)
            {
                _assetCategory_In = Cst.UnderlyingAsset_Rate.RateIndex;
            }
            base.InitProperty(pAll);

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {

            base.SetSQLWhere();
            //
            //Ajout de la restriction sur ASSETCATEGORY
            string sqlWhere = "(" + SQLObject + ".ASSETCATEGORY=" + DataHelper.SQLString(AssetCategory_In.ToString()) + ")";
            SQLWhere += SQLCst.AND + sqlWhere;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetCash : SQL_AssetBase
    {
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.Cash; }
        }

        #region constructors
        public SQL_AssetCash(string pCS, int pId)
            : this(pCS, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetCash(string pCS, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pCS, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetCash(string pCS, IDType pIdType, string pIdentifier)
            : this(pCS, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetCash(string pCS, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pCS, Cst.OTCml_TBL.ASSET_CASH, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors

        #region properties
        /// <summary>
        /// 
        /// </summary>
        public bool IsDefaultEmerging
        {
            get { return BoolFunc.IsTrue((GetFirstRowColumnValue("ISDEFAULTEMERGING"))); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefaultNoEmerging
        {
            get { return BoolFunc.IsTrue((GetFirstRowColumnValue("ISDEFAULTNEMERGING"))); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IdYieldCurveDef
        {
            get { return Convert.ToString((GetFirstRowColumnValue("IDYIELDCURVEDEF"))); }
        }
        #endregion

        #region methods
        /// <summary>
        /// Obtient l'asset de type AssetCash en fonction de la devise {pCurrency}
        /// <para>Récupère l'asset tel que IDC = {pCurrency} </para>
        /// <para>Si ce dernier n'existe pas, retourne l'asset approprié en fonction du caractère emergent de la devise {pCurrency}</para>
        /// <para>Obtient l'asset ENABLED en date système </para>
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public static SQL_AssetCash GetAssetCash(string pCS, string pIdC)
        {
            return GetAssetCash(pCS, pIdC, OTCmlHelper.GetDateSys(pCS));
        }

        /// <summary>
        /// Obtient l'asset de type AssetCash en fonction de la devise {pCurrency}
        /// <para>Récupère l'asset tel que IDC = {pCurrency} </para>
        /// <para>Si ce dernier n'existe pas, retourne l'asset approprié en fonction du caractère emergent de la devise {pCurrency}</para>
        /// <para>Obtient l'asset ENABLED en date {pDateEnabled}</para>
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pIdC"></param>
        /// <param name="pDateEnabled"></param>
        /// <returns></returns>
        public static SQL_AssetCash GetAssetCash(string pCS, string pIdC, DateTime pDateEnabled)
        {

            SQL_AssetCash ret = null;
            //
            SQL_Currency sqlCurrency = new SQL_Currency(pCS, SQL_Currency.IDType.IdC, pIdC);
            if (false == sqlCurrency.IsFound)
                throw new Exception(StrFunc.AppendFormat("currency {0} is unknown", pIdC));
            //
            int idAsset = 0;
            //
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "IDASSET" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_CASH.ToString() + Cst.CrLf;
            //1er étape
            if (0 == idAsset)
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC), pIdC);
                SQLWhere sqlWhere = new SQLWhere("IDC = @IDC");
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, Cst.OTCml_TBL.ASSET_CASH, pDateEnabled));
                QueryParameters query = new QueryParameters(pCS, sqlSelect.ToString() + sqlWhere.ToString(), dp);
                //
                object obj = DataHelper.ExecuteScalar(query.Cs, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
                if (null != obj)
                    idAsset = Convert.ToInt32(obj);
            }
            //2ème étape
            if (0 == idAsset)
            {
                string columnName = sqlCurrency.IsEmergingMarket ? "ISDEFAULTEMERGING" : "ISDEFAULTNEMERGING";
                //
                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "TRUEVALUE", DbType.Boolean), true);
                SQLWhere sqlWhere = new SQLWhere(columnName + " = @TRUEVALUE");
                sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, Cst.OTCml_TBL.ASSET_CASH, pDateEnabled));
                QueryParameters query = new QueryParameters(pCS, sqlSelect.ToString() + sqlWhere.ToString(), dp);
                //
                object obj = DataHelper.ExecuteScalar(query.Cs, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
                if (null != obj)
                    idAsset = Convert.ToInt32(obj);
            }
            //
            if (idAsset > 0)
            {
                ret = new SQL_AssetCash(pCS, idAsset);
                ret.LoadTable();
            }
            //
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQL_AssetSimpleIRSwap : SQL_AssetBase
    {
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.SimpleIRSwap; }
        }

        #region constructors
        public SQL_AssetSimpleIRSwap(string pCS, int pId)
            : this(pCS, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetSimpleIRSwap(string pCS, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pCS, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetSimpleIRSwap(string pCS, IDType pIdType, string pIdentifier)
            : this(pCS, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        public SQL_AssetSimpleIRSwap(string pCS, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pCS, Cst.OTCml_TBL.ASSET_SIMPLEIRS, pIdType, pIdentifier, pIsScanDataEnabled) { }
        #endregion constructors
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20151019 [21317] Modify
    public class SQL_AssetDebtSecurity : SQL_AssetBase
    {
        #region accessors
        /// EG 20140904 New
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.Bond; }
        }

        /// <summary>
        /// Obtient l'id de l'asset 
        /// </summary>
        public override int IdAsset
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDT")); }
        }
        /// <summary>
        /// Obtient l'identifiant de la chambre
        /// </summary>
        public override string ClearanceSystem
        {
            get
            {
                //FI 20111103 => je pense qu'il n'y a pas de chambre
                return string.Empty;
            }
        }
        /// <summary>
        /// Use to display info on capture under asset textbox
        /// </summary>
        /// EG 20140526
        public override string SetDisplay
        {
            get
            {
                string display = base.SetDisplay;
                //if (StrFunc.IsFilled(ISINCode) && (false == Description.Contains(ISINCode)))
                //    display += Cst.Space + "/" + Cst.Space + ISINCode;
                return display;
            }
        }

        /// <summary>
        /// Obtient le code ISIN de l'asset
        /// </summary>
        /// FI 20151019 [21317] add 
        public string ISINCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISINCODE")); }
        }
        /// <summary>
        /// Obtient le code RIC de l'asset
        /// </summary>
        /// FI 20151019 [21317] add 
        public string RICCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("RICCODE")); }
        }
        /// <summary>
        /// Obtient le code Bloomberg de l'asset
        /// </summary>
        /// FI 20151019 [21317] add  
        public string BBGCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("BBGCODE")); }
        }
        /// <summary>
        /// Obtient le code SEDOL de l'asset
        /// <para>Stock Exchange Daily Official List (Use in UK,Ireland)</para>
        /// </summary>
        /// FI 20151019 [21317] add  
        public string SEDOLCode
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SEDOL")); }
        }
        
        /// <summary>
        ///  Obtient le nominal du titre 
        /// </summary>
        /// FI 20151019 [21317] Modify
        public Nullable<decimal> ParValue
        {
            get
            {
                if (GetFirstRowColumnValue("PARVALUE") == null)
                    return null;
                else
                    return Convert.ToDecimal(GetFirstRowColumnValue("PARVALUE"));
            }
        }

        /// <summary>
        ///  Obtient la devise du nominal du titre
        /// </summary>
        /// FI 20151019 [21317] Modify
        public string ParValueIdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC_PARVALUE")); }
        }
        #endregion accessors

        #region constructors
        public SQL_AssetDebtSecurity(string pCS, int pId)
            : this(pCS, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No) { }
        public SQL_AssetDebtSecurity(string pCS, int pId, ScanDataDtEnabledEnum pScanDataEnabled) :
            this(pCS, IDType.Id, pId.ToString(), pScanDataEnabled) { }
        public SQL_AssetDebtSecurity(string pCS, IDType pIdType, string pIdentifier) :
            this(pCS, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }
        // EG 20150327 [20513] BANCAPERTA Options sur titre (VW_ASSET_DEBTSECURITY remplace VW_TRADEDEBTSEC)
        public SQL_AssetDebtSecurity(string pCS, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pCS, Cst.OTCml_TBL.VW_ASSET_DEBTSECURITY, pIdType, pIdentifier, pIsScanDataEnabled) { }

        #endregion constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void SetSQLCol(string[] pCol)
        {
            base.SetSQLCol(pCol);
            if (false == IsQueryFindOnly(pCol))
            {
                AddSQLCol("ts.IDC as IDC");
                AddSQLCol(String.Format("{0}.IDM as IDM", SQLObject));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            string addfrom = StrFunc.AppendFormat(SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTREAM.ToString() + " ts on ts.IDT={0}.IDT and ts.INSTRUMENTNO=1 and ts.STREAMNO=1", SQLObject) + Cst.CrLf;
            
            ConstructFrom(addfrom);
        }
        #endregion
    }

    /// <summary>
    /// Get an Asset Commodity
    /// </summary>
    /// PM 20161216 [] New
    /// EG 20221101 [25639][WI484] Add _gasProductType_In, _electricityProductType_In et _environmentalProductType_In
    public class SQL_AssetCommodityContract : SQL_AssetCommodity
    {
        #region private members
        private int _idCC_In;
        private CommodityContractClassEnum _commodityContractClass_In;
        private GasProductTypeEnum _gasProductType_In;
        private ElectricityProductTypeEnum _electricityProductType_In;
        private EnvironmentalProductTypeEnum _environmentalProductType_In;
        #endregion private members

        #region constructors
        public SQL_AssetCommodityContract(string pSource, int pId)
            : this(pSource, pId, ScanDataDtEnabledEnum.No) { }

        public SQL_AssetCommodityContract(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataEnabled) { }

        public SQL_AssetCommodityContract(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }

        /// EG 20221101 [25639][WI484] Add _gasProductType_In, _electricityProductType_In et _environmentalProductType_In
        public SQL_AssetCommodityContract(string pSource, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.VW_ASSET_COMMODITY_EXPANDED, pIdType, pIdentifier, pIsScanDataEnabled)
        {
            CommodityContractClass_In = CommodityContractClassEnum.Undefined;
            GasProductType_In = GasProductTypeEnum.Undefined;
            ElectricityProductType_In = ElectricityProductTypeEnum.Undefined;
            EnvironmentalProductType_In = EnvironmentalProductTypeEnum.Undefined;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Obtient l'id du Commodity Contract rattaché à l'asset
        /// </summary>
        public int IdCommodityContract
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDCC")); }
        }

        /// <summary>
        /// Obtient l'identifier du Commodity Contrat rattaché à l'asset
        /// </summary>
        public string CommodityContract_Identifier
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTIDENTIFIER")); }
        }

        /// <summary>
        /// Obtient le displayname du Commodity Contrat rattaché à l'asset
        /// </summary>
        public string CommodityContract_DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTDISPLAYNAME")); }
        }

        /// <summary>
        /// Obtient le type de négociation pouvant être réalisé sur le Commodity Contract rattaché à l'asset
        /// <para>Les valeurs possible sont "Intraday" ou "Spot"</para>
        /// </summary>
        public string CommodityContract_TradableType
        {
            get { return Convert.ToString(GetFirstRowColumnValue("TRADABLETYPE")); }
        }

        /// <summary>
        /// Obtient la class de Commodity Contract rattaché à l'asset
        /// <para>Les valeurs possible sont "Coal", "Electricity", "Environmental", "Gas", "Oil"</para>
        /// </summary>
        public string CommodityContract_Class
        {
            get { return Convert.ToString(GetFirstRowColumnValue("COMMODITYCLASS")); }
        }

        /// <summary>
        /// Obtient true si le contrat est un contrat de charbon
        /// </summary>
        public bool CommodityContract_IsCoal
        {
            get
            {
                return (StrFunc.IsFilled(CommodityContract_Class) && (CommodityContract_Class == CommodityContractClassEnum.Coal.ToString()));
            }
        }

        /// <summary>
        /// Obtient true si le contrat est un contrat d'électricité
        /// </summary>
        public bool CommodityContract_IsElectricity
        {
            get
            {
                return (StrFunc.IsFilled(CommodityContract_Class) && (CommodityContract_Class == CommodityContractClassEnum.Electricity.ToString()));
            }
        }

        /// <summary>
        /// Obtient true si le contrat est un contrat sur les émissions
        /// </summary>
        public bool CommodityContract_IsEnvironmental
        {
            get
            {
                return (StrFunc.IsFilled(CommodityContract_Class) && (CommodityContract_Class == CommodityContractClassEnum.Environmental.ToString()));
            }
        }

        /// <summary>
        /// Obtient true si le contrat est un contrat de gas
        /// </summary>
        public bool CommodityContract_IsGas
        {
            get
            {
                return (StrFunc.IsFilled(CommodityContract_Class) && (CommodityContract_Class == CommodityContractClassEnum.Gas.ToString()));
            }
        }

        /// <summary>
        /// Obtient true si le contrat est un contrat de pétrol
        /// </summary>
        public bool CommodityContract_IsOil
        {
            get
            {
                return (StrFunc.IsFilled(CommodityContract_Class) && (CommodityContract_Class == CommodityContractClassEnum.Oil.ToString()));
            }
        }

        /// <summary>
        /// Obtient le type de Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_Type
        {
            get { return Convert.ToString(GetFirstRowColumnValue("COMMODITYTYPE")); }
        }

        /// <summary>
        /// Obtient le symbol de marché du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_ContractSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("CONTRACTSYMBOL")); }
        }

        /// <summary>
        /// Obtient le Exchange symbol de marché du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_ExchContractSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXCHCONTRACTSYMBOL")); }
        }

        /// <summary>
        /// Obtient le point de livraison du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_DeliveryPoint
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DELIVERYPOINT")); }
        }

        /// <summary>
        /// Obtient l'unité de volume du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_UnitOfMeasure
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITOFMEASURE")); }
        }

        /// <summary>
        ///  Obtient le volume du Commodity Contract rattaché à l'asset
        /// </summary>
        public Nullable<decimal> CommodityContract_UnitOfMeasureQuantity
        {
            get
            {
                return (GetFirstRowColumnValue("UNITOFMEASUREQTY") == null) ? (decimal?)null : Convert.ToDecimal(GetFirstRowColumnValue("UNITOFMEASUREQTY"));
            }
        }

        /// <summary>
        /// Obtient l'unité du prix du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_UnitOfPrice
        {
            get { return Convert.ToString(GetFirstRowColumnValue("UNITOFPRICE")); }
        }

        /// <summary>
        ///  Obtient le prix minimum du Commodity Contract rattaché à l'asset
        /// </summary>
        public Nullable<decimal> CommodityContract_MinimumPrice
        {
            get
            {
                return (GetFirstRowColumnValue("MINPRICE") == null) ? (decimal?)null : Convert.ToDecimal(GetFirstRowColumnValue("MINPRICE"));
            }
        }

        /// <summary>
        ///  Obtient le prix maximum du Commodity Contract rattaché à l'asset
        /// </summary>
        public Nullable<decimal> CommodityContract_MaximumPrice
        {
            get
            {
                return (GetFirstRowColumnValue("MAXPRICE") == null) ? (decimal?)null : Convert.ToDecimal(GetFirstRowColumnValue("MAXPRICE"));
            }
        }

        /// <summary>
        /// Indique si les prix négatifs sont autorisés pour le Commodity Contract rattaché à l'asset
        /// </summary>
        public bool CommodityContract_IsNegatifPriceAllowed
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISWITHNEGPRICE")); }
        }

        /// <summary>
        /// Indique si les changements d'heures été/hiver sont pris en compte pour le Commodity Contract rattaché à l'asset
        /// </summary>
        public bool CommodityContract_IsDaylightSavingTime
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISAPPLYSUMMERTIME")); }
        }

        /// <summary>
        /// Obtient la TimeZone des heures de livraison du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_TimeZone
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMEZONE")); }
        }

        /// <summary>
        /// Obtient l'heure de début de livraison du Commodity Contract journalier rattaché à l'asset
        /// </summary>
        public string CommodityContract_TimeStart
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMESTART")); }
        }

        /// <summary>
        /// Obtient l'heure de fin de livraison du Commodity Contract journalier rattaché à l'asset
        /// </summary>
        public string CommodityContract_TimeEnd
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DELIVERYTIMEEND")); }
        }

        /// <summary>
        /// Obtient le durée du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_Duration
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DURATION")); }
        }

        /// <summary>
        /// Obtient la fréquence d'application de la quantité du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_FrequencyQuantity
        {
            get { return Convert.ToString(GetFirstRowColumnValue("FREQUENCYQTY")); }
        }

        /// <summary>
        /// Obtient la quanlité livrée du Commodity Contract rattaché à l'asset
        /// </summary>
        public string CommodityContract_Quality
        {
            get { return Convert.ToString(GetFirstRowColumnValue("QUALITY")); }
        }

        /// <summary>
        ///
        /// </summary>
        public int CommodityContract_QtyScale
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("SCALEQTY")); }
        }

        /// <summary>
        /// Obtient l'id de la méthode de calcul du déposit pour cet asset
        /// </summary>
        //  PM 20200910 [25481] Ajout IdImMethod
        public int IdImMethod
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDIMMETHOD")); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret = base.IsUseOrderBy;
                if (false == ret)
                {
                    ret = (Id_In == 0) && (StrFunc.ContainsIn(AssetSymbol_In, "%"));
                }
                return ret;
            }
        }

        /// <summary>
        ///  Obtient ou définit le Commodit Contract utilisé pour la recherche de l'asset
        /// </summary>
        public int IdCC_In
        {
            get { return _idCC_In; }
            set
            {
                if (_idCC_In != value)
                {
                    InitProperty(false);
                    _idCC_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit la classe Commodit Contract utilisé pour la recherche de l'asset
        /// </summary>
        public CommodityContractClassEnum CommodityContractClass_In
        {
            get { return _commodityContractClass_In; }
            set
            {
                if (_commodityContractClass_In != value)
                {
                    InitProperty(false);
                    _commodityContractClass_In = value;
                }
            }
        }

        // EG 20221101 [25639][WI484] New
        public GasProductTypeEnum GasProductType_In
        {
            get { return _gasProductType_In; }
            set
            {
                if (_gasProductType_In != value)
                {
                    InitProperty(false);
                    _gasProductType_In = value;
                }
            }
        }
        // EG 20221101 [25639][WI484] New
        public ElectricityProductTypeEnum ElectricityProductType_In
        {
            get { return _electricityProductType_In; }
            set
            {
                if (_electricityProductType_In != value)
                {
                    InitProperty(false);
                    _electricityProductType_In = value;
                }
            }
        }
        // EG 20221101 [25639][WI484] New
        public EnvironmentalProductTypeEnum EnvironmentalProductType_In
        {
            get { return _environmentalProductType_In; }
            set
            {
                if (_environmentalProductType_In != value)
                {
                    InitProperty(false);
                    _environmentalProductType_In = value;
                }
            }
        }
        #endregion properties

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20221101 [25639][WI484] Add _gasProductType_In, _electricityProductType_In et _environmentalProductType_In
        protected override void SetSQLWhere()
        {
            base.SetSQLWhere();

            //IDCC
            if (IdCC_In > 0)
            {
                ConstructWhere(SQLObject + ".IDCC = @IDCC");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDDC), IdCC_In);
            }

            //COMMODITIYCLASS
            if (CommodityContractClass_In != CommodityContractClassEnum.Undefined)
            {
                ConstructWhere(SQLObject + ".COMMODITYCLASS=@COMMODITYCLASS");
                SetDataParameter(new DataParameter(CS, "COMMODITYCLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), CommodityContractClass_In.ToString());
            }

            //COMMODITIYTYPE
            if (GasProductType_In != GasProductTypeEnum.Undefined)
            {
                ConstructWhere(SQLObject + ".COMMODITYTYPE=@COMMODITYTYPE");
                SetDataParameter(new DataParameter(CS, "COMMODITYTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), GasProductType_In.ToString());
            }
            if (ElectricityProductType_In != ElectricityProductTypeEnum.Undefined)
            {
                ConstructWhere(SQLObject + ".COMMODITYTYPE=@COMMODITYTYPE");
                SetDataParameter(new DataParameter(CS, "COMMODITYTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ElectricityProductType_In.ToString());
            }
            if (EnvironmentalProductType_In != EnvironmentalProductTypeEnum.Undefined)
            {
                ConstructWhere(SQLObject + ".COMMODITYTYPE=@COMMODITYTYPE");
                SetDataParameter(new DataParameter(CS, "COMMODITYTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), EnvironmentalProductType_In.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;

            if (AssetSymbol_In != null)
            {
                column_In = DataParameter.ParameterEnum.ASSETSYMBOL;
                data_In = AssetSymbol_In;
            }

            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }
                
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }
        #endregion
    }

    /// <summary>
    /// Get an Asset Commodity
    /// </summary>
    /// PM 20161216 [] New
    public class SQL_AssetCommodity : SQL_AssetBase
    {
        #region private members
        private string _AssetSymbol_In = null;
        private int _idM_In;
        #endregion private members

        #region constructors
        public SQL_AssetCommodity(string pSource, int pId) : this(pSource, pId, ScanDataDtEnabledEnum.No) { }

        public SQL_AssetCommodity(string pSource, int pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : this(pSource, Cst.OTCml_TBL.ASSET_COMMODITY, IDType.Id, pId.ToString(), pScanDataEnabled) { }

        public SQL_AssetCommodity(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, Cst.OTCml_TBL.ASSET_COMMODITY, pIdType, pIdentifier, ScanDataDtEnabledEnum.No) { }

        public SQL_AssetCommodity(string pSource, Cst.OTCml_TBL pTable, IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pIsScanDataEnabled)
            : base(pSource, pTable, pIdType, pIdentifier, pIsScanDataEnabled) 
        {
            switch (pIdType)
            {
                case IDType.AssetSymbol:
                    AssetSymbol_In = pIdentifier;
                    break;
            }
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Obtient la catégorie d'asset
        /// </summary>
        public override Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get { return Cst.UnderlyingAsset.Commodity; }
        }

        /// <summary>
        /// Obtient la chambre de compensation
        /// </summary>
        public override string ClearanceSystem
        {
            get { return base.ClearanceSystem; }
        }

        /// <summary>
        /// Obtient le devise de l'asset
        /// </summary>
        public override string IdC
        {
            get { return base.IdC; }
        }

        /// <summary>
        /// Obtient le Symbol de l'asset
        /// </summary>
        public string AssetSymbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ASSETSYMBOL")); }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override bool IsUseOrderBy
        {
            get
            {
                bool ret = base.IsUseOrderBy;
                if (false == ret)
                {
                    ret = (Id_In == 0) && (StrFunc.ContainsIn(AssetSymbol_In, "%"));
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit le symbol utilisé pour la recherche de l'asset
        /// </summary>
        public string AssetSymbol_In
        {
            get { return _AssetSymbol_In; }
            set
            {
                if (_AssetSymbol_In != value)
                {
                    InitProperty(true);
                    _AssetSymbol_In = value;
                }
            }
        }

        /// <summary>
        ///  Obtient ou définit le marché utilisé pour la recherche de l'asset
        /// </summary>
        public int IdM_In
        {
            get { return _idM_In; }
            set
            {
                if (_idM_In != value)
                {
                    InitProperty(false);
                    _idM_In = value;
                }
            }
        }
        
        /// <summary>
        ///  Retourne le CommodityContrat
        /// </summary>
        /// FI 20161214 [21916] Add
        public Nullable<int> IdCC
        {
            get
            {
                Nullable<int> ret = null;
                if (false == (GetFirstRowColumnValue("IDCC") is DBNull))
                    ret = Convert.ToInt32(GetFirstRowColumnValue("IDCC"));
                return ret;
            }
        }
        #endregion properties

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAll"></param>
        protected override void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _AssetSymbol_In = null;
            }
            base.InitProperty(pAll);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            //
            base.SetSQLWhere();
            //IDM
            if (IdM_In > 0)
            {
                ConstructWhere(SQLObject + ".IDM=@IDM");
                SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDM), IdM_In);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20180906 [24159] Refactoring Upper en fonction de la collation présente dans le fichier de config
        protected override SQLWhere GetWhereIDType()
        {
            SQLWhere sqlWhere = new SQLWhere();
            string data_In = null;
            DataParameter.ParameterEnum column_In = DataParameter.ParameterEnum.NA;
            
            if (AssetSymbol_In != null)
            {
                column_In = DataParameter.ParameterEnum.ASSETSYMBOL;
                data_In = AssetSymbol_In;
            }

            if (data_In != null)
            {
                DataParameter dp = DataParameter.GetParameter(CS, column_In);
                Boolean isCaseInsensitive = IsUseCaseInsensitive(data_In);

                string column = SQLObject + "." + column_In.ToString();
                if (isCaseInsensitive)
                {
                    column = DataHelper.SQLUpper(CS, column);
                    data_In = data_In.ToUpper();
                }

                //Critère sur donnée String
                if (IsUseOrderBy)
                    sqlWhere.Append(column + SQLCst.LIKE + "@" + dp.ParameterKey);
                else
                    sqlWhere.Append(column + "=@" + dp.ParameterKey);

                SetDataParameter(dp, data_In);
            }
            else
            {
                sqlWhere = base.GetWhereIDType();
            }

            return sqlWhere;
        }
        #endregion
    }
}
