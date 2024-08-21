#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Fix;
using FixML.Enum;
using FixML.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20190822 [XXXXX] Herite de ContainerCciBase
    public class CciFixInstrument : ContainerCciBase, IContainerCciFactory, IContainerCciGetInfoButton
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        /// FI 20131126 [19271] add Matdt
        /// EG 20171113 [23509] Add FacilityHasChanged
        public enum CciEnum
        {
            /// <summary>
            /// Représente le marché
            /// </summary>
            /// FI 20200116 [25141] Add CciColumnValue attribute
            [CciColumnValue(Column = "SHORT_ACRONYM", IOColumn = "FIXML_SecurityExchange")]
            [CciGroupAttribute(name = "FacilityHaschanged")]
            Exch,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [CciGroupAttribute(name = "SetCciID")] 
            ID,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [CciGroupAttribute(name = "SetCciID")]
            MMY,
            [CciGroupAttribute(name = "SetCciID")]
            PutCall,
            [CciGroup(name = "SetCciID")]
            StrkPx,
            [CciGroupAttribute(name = "SetCciID")]
            [CciGroupAttribute(name = "FacilityHaschanged")]
            Sym,
            [CciGroup(name = "AssetCode")]
            ISINCode,
            [CciGroup(name = "AssetCode")]
            RICCode,
            [CciGroup(name = "AssetCode")]
            BBGCode,
            [CciGroup(name = "AssetCode")]
            NSINCode,
            [CciGroup(name = "AssetCode")]
            CFICode,
            Issuer,
            IssueDate,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            ExchangeSymbol,
            MatDt,
        }
        #endregion Enums

        #region Members
        /// <summary>
        /// pointeur pour accéder aux éléments du trade
        /// </summary>
        private readonly CciTradeBase _cciTrade;

        /// <summary>
        /// pointeur pour accéder aux éléments communs des products exchangeTradedDerivative / equitySecurityTransaction
        /// <para>Ce produit n'est pas nécessairement le master product (cas des strategies)</para>
        /// </summary>
        private readonly CciProductExchangeTradedBase _cciExchangeTraded;

        /// <summary>
        /// 
        /// </summary>
        private readonly IFixInstrument _fixInstrument;
        /// <summary>
        /// True si ETD product
        /// </summary>
        private readonly bool _isETD;
        /// <summary>
        /// True si Equity security transaction
        /// </summary>
        private readonly bool _isEST;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtiens la collection ccis
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// Représente le trade
        /// </summary>
        public CciTradeBase CciTrade
        {
            get { return _cciTrade; }
        }
        /// <summary>
        /// Obtient le cci Marcché
        /// </summary>
        public CustomCaptureInfo CciExch
        {
            get { return Cci(CciEnum.Exch); }
        }

        /// <summary>
        /// Retourne la colonne SQL utilisée pour alimenter la proriété .NewValue du cci marché
        /// </summary>
        /// FI 20200116 [25141] Add
        public string CciExchColumn
        {
            get
            {
                return CciTools.GetColumn<CciEnum>(CciEnum.Exch, CcisBase.IsModeIO);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciMMY
        {
            get { return Cci(CciEnum.MMY); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciPutCall
        {
            get { return Cci(CciEnum.PutCall); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciStrkPx
        {
            get { return Cci(CciEnum.StrkPx); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciSym
        {
            get { return Cci(CciEnum.Sym); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciISINCode
        {
            get { return Cci(CciEnum.ISINCode); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciRICCode
        {
            get { return Cci(CciEnum.RICCode); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciBBGCode
        {
            get { return Cci(CciEnum.BBGCode); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciNSINCode
        {
            get { return Cci(CciEnum.NSINCode); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciCFICode
        {
            get { return Cci(CciEnum.CFICode); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciIssuer
        {
            get { return Cci(CciEnum.Issuer); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciIssueDate
        {
            get { return Cci(CciEnum.IssueDate); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20131126 [19271]  add cciMaturityDate
        public CustomCaptureInfo CciMaturityDate
        {
            get { return Cci(CciEnum.MatDt); }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfo CciExchangeSymbol
        {
            get { return Cci(CciEnum.ExchangeSymbol); }
        }


        /// <summary>
        /// 
        /// </summary>
        public IFixInstrument FixInstrument
        {
            get { return _fixInstrument; }
        }

        #endregion Accessors

        #region Constructors
        public CciFixInstrument(CciTradeBase pCciTrade, CciProductExchangeTradedBase pCciExchangeTraded, string pPrefix, IFixInstrument pFixInstrument)
            : base(pPrefix, pCciTrade.Ccis)

        {
            _cciTrade = pCciTrade;
            _cciExchangeTraded = pCciExchangeTraded;

            _isETD = pCciExchangeTraded.GetType().Equals(typeof(CciProductExchangeTradedDerivative));
            _isEST = pCciExchangeTraded.GetType().Equals(typeof(CciProductEquitySecurityTransaction));

            _fixInstrument = pFixInstrument;

        }
        #endregion Constructors

        #region Interfaces

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170430 [23009] Modify
        public void AddCciSystem()
        {
            // FI 20170430 [23009] Nouveau cci system (permet de ne pas ajouter ce cci dans le flux post mapping lors d'une importation de trade)
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.ID), true, TypeData.TypeDataEnum.@string); // Obligatoire
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
        }
        

        /// <summary>
        /// 
        /// </summary>
        // FI 20141001 [XXXXX] Modify
        // PL 20171006 [23469] MARKETTYPE deprecated
        public void Initialize_Document()
        {
            // RD 20110225
            // C'est pour charger le dérnier Marché utilisé, uniquement si on est en mode Création
            if (StrFunc.IsEmpty(FixInstrument.SecurityExchange) && (CcisBase.CaptureMode == Cst.Capture.ModeEnum.New))
            {
                string defaultMarket = string.Empty;

                if (Ccis.TradeCommonInput.IsDefaultSpecified(CommonInput.DefaultEnum.market))
                {
                    defaultMarket = (string)Ccis.TradeCommonInput.GetDefault(CommonInput.DefaultEnum.market);
                }

                if (StrFunc.IsFilled(defaultMarket))
                {
                    // FI 20200116 [25141] Appel CciTools.GetColumn pour obtenir la colonne qui alimente le cci
                    SQL_TableWithID.IDType IDTypeSearch = CciTools.ParseColumn(CciTools.GetColumn<CciEnum>(CciEnum.Exch, CcisBase.IsModeIO));

                    SQL_Market sqlMarket = new SQL_Market(CciTrade.CSCacheOn, IDTypeSearch, defaultMarket,
                        SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, CcisBase.User, CcisBase.SessionId)
                    {
                        IsUseView = true
                    };

                    //FI 20141001 [XXXXX] Ajout de la colonne EXCHANGESYMBOL
                    //PL 20171006 [23469] MARKETTYPE deprecated, add IDMOPERATING
                    if (sqlMarket.LoadTable(new string[] { "IDMOPERATING", "FIXML_SecurityExchange", "EXCHANGESYMBOL", "IDENTIFIER", "ISO10383_ALPHA4", "SHORT_ACRONYM" }))
                    {
                        //PL 20130208 ISO
                        //FixInstrument.SecurityExchange = sqlMarket.Identifier;
                        FixInstrument.SecurityExchange = sqlMarket.FIXML_SecurityExchange;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fixInstrument);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromDocument()
        {
            CustomCaptureInfo cciID = Cci(CciEnum.ID);


            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion

                    switch (cciEnum)
                    {
                        #region Security Exchange
                        case CciEnum.Exch:
                            data = FixInstrument.SecurityExchange;
                            //PL 20130208 ISO
                            //sql_Table = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.No,true);
                            sql_Table = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, null, _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
                            // FI 20200116 [XXXXX] Utilisation CciExchColumn 
                            if (null != sql_Table)
                                data = ((sql_Table as SQL_Market).GetFirstRowColumnValue(CciExchColumn)) as string;
                            break;
                        #endregion Security Exchange

                        #region Security ID
                        case CciEnum.ID:
                            sql_Table = null;
                            data = string.Empty;
                            int idAsset = IntFunc.IntValue(FixInstrument.SecurityId);
                            if (idAsset > 0)
                            {
                                SQL_AssetBase sql_Asset = null;
                                if (_isETD)
                                    sql_Asset = new SQL_AssetETD(CciTrade.CSCacheOn, idAsset);
                                else if (_isEST)
                                    sql_Asset = new SQL_AssetEquity(CciTrade.CSCacheOn, idAsset);

                                if (sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1))
                                {
                                    sql_Table = sql_Asset;
                                    data = sql_Asset.Identifier;
                                }
                            }
                            break;
                        #endregion Security ID
                        #region Maturity Month Year
                        case CciEnum.MMY:
                            data = FixInstrument.MaturityMonthYear;
                            break;
                        #endregion Maturity Month Year
                        #region Put Or Call
                        case CciEnum.PutCall:
                            if (FixInstrument.PutOrCallSpecified)
                                data = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(FixInstrument.PutOrCall);
                            break;
                        #endregion Put Or Call
                        #region Strike Price
                        case CciEnum.StrkPx:
                            if (true == FixInstrument.StrikePriceSpecified)
                                data = FixInstrument.StrikePrice.Value;
                            break;
                        #endregion Strike Price
                        #region Contract Symbol
                        case CciEnum.Sym:
                            data = FixInstrument.Symbol;
                            // FI 20121004 [18172]
                            sql_Table = ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(CciTrade.CSCacheOn, null, _fixInstrument);
                            // FI 20221206 [XXXXX] data remplacé par l'dentifier du DC Spheres®
                            if (sql_Table != null)
                                data = (sql_Table as SQL_DerivativeContract).Identifier;
                            break;
                        #endregion Contract Symbol
                        #region ISINCode
                        case CciEnum.ISINCode:
                            data = FixInstrument.ISINCode;
                            // FI 20200122 [25175] Lecture de l'information sur l'asset 
                            if ((null != cciID) && (null != cciID.Sql_Table))
                            {
                                FixInstrument.ISINCode = cciID.Sql_Table.GetFirstRowColumnValue("ISINCODE") as string;
                                data = FixInstrument.ISINCode;
                            }
                            break;
                        #endregion ISINCode
                        #region RICCode
                        case CciEnum.RICCode:
                            data = FixInstrument.RICCode;
                            // FI 20200122 [25175] Lecture de l'information sur l'asset
                            if ((null != cciID) && (null != cciID.Sql_Table))
                            {
                                FixInstrument.RICCode = cciID.Sql_Table.GetFirstRowColumnValue("RICCODE") as string;
                                data = FixInstrument.RICCode;
                            }
                            break;
                        #endregion RICCode
                        #region BBGCode
                        case CciEnum.BBGCode:
                            data = FixInstrument.BBGCode;
                            // FI 20200122 [25175] Lecture de l'information sur l'asset
                            if ((null != cciID) && (null != cciID.Sql_Table))
                            {
                                FixInstrument.BBGCode = cciID.Sql_Table.GetFirstRowColumnValue("BBGCODE") as string;
                                data = FixInstrument.BBGCode;
                            }
                            break;
                        #endregion BBGCode
                        #region NSINCode
                        case CciEnum.NSINCode:
                            if ((null != cciID) && (null != cciID.Sql_Table))
                                FixInstrument.NSINTypeCode = ((SQL_AssetEquity)cciID.Sql_Table).NSINTypeCode;
                            data = FixInstrument.NSINCode;
                            break;
                        #endregion BBGCode
                        #region CFICode
                        case CciEnum.CFICode:
                            data = FixInstrument.CFICode;
                            // FI 20200122 [25175] Lecture de l'information sur l'asset
                            if ((null != cciID) && (null != cciID.Sql_Table))
                            {
                                FixInstrument.CFICode = cciID.Sql_Table.GetFirstRowColumnValue("CFICODE") as string;
                                data = FixInstrument.CFICode;
                            }
                            break;
                        #endregion CFICode
                        #region Issuer
                        case CciEnum.Issuer:
                            if (FixInstrument.IssuerSpecified)
                                data = FixInstrument.Issuer;
                            break;
                        #endregion Issuer
                        case CciEnum.IssueDate:
                            if (FixInstrument.IssueDateSpecified)
                                data = FixInstrument.IssueDate.Value;
                            break;
                        case CciEnum.MatDt: // FI 20131126 [19271] 
                            if (FixInstrument.MaturityDateSpecified)
                                data = FixInstrument.MaturityDate.Value;
                            break;
                        #region  Asset Exchange Symbol
                        case CciEnum.ExchangeSymbol:
                            data = FixInstrument.ExchangeSymbol;
                            break;
                        #endregion Asset Exchange Symbol
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            // FI 20220929 [XXXXX] Add
            //initilisation 
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode)
                    && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode))
                    && (null != cciID))
            {
                SetCciID();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    switch (key)
                    {
                        case CciEnum.Exch:
                            //lastValue est valorisé à * pour provoquer le dump sur le CciEnum.ID (l'asset sera alors non valide)
                            //FI 20120716 [18012] Cci(CciEnum.ID) n'est pas nécessairement présent (C'est le cas sur le _cciProductGlogal d'une satagety)
                            //Cci(CciEnum.ID).LastValue = "*";
                            CcisBase.Set(CciClientId(CciEnum.ID), "lastValue", "*");
                            break;

                        case CciEnum.ID:
                            if ((null != pCci.Sql_Table) || (pCci.IsEmptyValue && pCci.IsLastInputByUser))
                                InitializeFromSecurityID();
                            break;
                    }
                }

                SearchAsset(pCci);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20161214 [21916] Modify 
        public void SetDisplay(CustomCaptureInfo pCci)
        {

            if (IsCci(CciEnum.ID, pCci))
            {
                if (null != pCci.Sql_Table)
                {
                    string display = string.Empty;
                    if (_isETD)
                    {
                        SQL_AssetETD SqlAsset = (SQL_AssetETD)pCci.Sql_Table;                        
                        display = SqlAsset.DisplayName;
                        display += "  -  ";
                        bool isOption = StrFunc.IsFilled(SqlAsset.PutCall);
                        if (isOption)
                            display += SqlAsset.PutCall_EnglishString;
                        if (StrFunc.IsFilled(SqlAsset.Maturity_MaturityMonthYear))
                        {
                            display += " " + SqlAsset.Maturity_MaturityMonthYear;
                            if (DtFunc.IsDateTimeFilled(SqlAsset.Maturity_MaturityDate))
                                display += " (" + DtFunc.DateTimeToString(SqlAsset.Maturity_MaturityDate, DtFunc.FmtShortDate) + ")";
                        }
                        if (isOption)
                        {
                            // FI 20190520 [XXXXX] Usage .ToString(NumberFormatInfo.InvariantInfo) pour ne pas perder des decimales
                            // AL 20240327 [WI593]
                            //display += " " + StrFunc.FmtDecimalToCurrentCulture(SqlAsset.StrikePrice.ToString(NumberFormatInfo.InvariantInfo));

                            // By default the trailing 0 of the StrikePrice are removed
                            // e.g. 
                            // 1.12300000 => 1.123
                            // 1.10000000 => 1.1
                            // 1.00000000 => 1.
                            string strVal = SqlAsset.StrikePrice.ToString(NumberFormatInfo.InvariantInfo).TrimEnd('0');

                            // If the Contract Symbol is present, read the StrikeIcrement
                            if (null != Cci(CciEnum.Sym) && Cci(CciEnum.Sym).Sql_Table != null)
                            {
                                SQL_DerivativeContract SqlContract = (SQL_DerivativeContract)Cci(CciEnum.Sym).Sql_Table;
                                if (SqlContract.StrikeIncrement != null) {
                                    //If the StrikeIncrement has a value, the minimum number of decimal digits is the number of significant decimal digits of the StrikeIncrement
                                    decimal stkInc = SqlContract.StrikeIncrement.Value;
                                    string strIncVal = stkInc.ToString(NumberFormatInfo.InvariantInfo).TrimEnd('0');
                                    string[] splittedVal = strIncVal.Split(NumberFormatInfo.InvariantInfo.NumberDecimalSeparator.ToCharArray());
                                    int minPrecision = splittedVal.Count() > 1 ? splittedVal[1].Length : 0;
                                    //Using the FmtDecimalToInvariantCulture, the StrikePrice is formatted using minPrecision or at least numberOfDecimalMin decimal digits (defined in FmtDecimal method)
                                    strVal = StrFunc.FmtDecimalToInvariantCulture(SqlAsset.StrikePrice, minPrecision);
                                }
                            }
                            //Using the FmtDecimalToCurrentCulture, the StrikePrice is displayed using at least numberOfDecimalMin decimal digits, defined in FmtDecimal method
                            display += " " + StrFunc.FmtDecimalToCurrentCulture(strVal);

                        }
                        // FI 20200122 [25175] Suppression de l'ISIN puisque désormais afficher dans le panneau caractéristiques de l'asset
                        //if (StrFunc.IsFilled(SqlAsset.ISINCode))
                        //    display += "  -  ISIN:" + SqlAsset.ISINCode;
                    }
                    else if (_isEST)
                    {
                        SQL_AssetEquity SqlAsset = (SQL_AssetEquity)pCci.Sql_Table;
                        display = SqlAsset.DisplayName;
                    }

                    pCci.Display = display;
                }
                else if (pCci.IsNewValueKeyword)
                {
                    GetMsgWithOutKeyword(pCci.NewValue, out string msg);
                    pCci.Display = "{" + msg + "}";
                }
            }
            else if (IsCci(CciEnum.MMY, pCci))
            {
                if (null != pCci.Sql_Table)
                {
                    SQL_Maturity sqlMaturity = (SQL_Maturity)pCci.Sql_Table;
                    pCci.Display = DtFunc.DateTimeToStringDateISO(sqlMaturity.MaturityDate);
                }
            }
            else if (IsCci(CciEnum.Exch, pCci))
            {
                if (null != pCci.Sql_Table)
                    CciTools.SetMarKetDisplay(pCci); // FI 20161214 [21916] call SetMarKetDisplay
            }
            else if (IsCci(CciEnum.NSINCode, pCci))
            {
                string NSINType = string.Empty;
                if (StrFunc.IsFilled(FixInstrument.NSINCode))
                    NSINType = "(" + FixInstrument.NSINTypeCodeText + ")";
                pCci.Display = NSINType;
            }
            else if (IsCci(CciEnum.Issuer, pCci))
            {
                string address = string.Empty;
                if (FixInstrument.CountryOfIssueSpecified)
                    address = FixInstrument.CountryOfIssue;
                if (FixInstrument.StateOrProvinceOfIssueSpecified)
                {
                    if (StrFunc.IsFilled(address))
                        address += " - ";
                    address += FixInstrument.StateOrProvinceOfIssue;
                }
                if (FixInstrument.LocaleOfIssueSpecified)
                {
                    if (StrFunc.IsFilled(address))
                        address += " - ";
                    address += FixInstrument.LocaleOfIssue;
                }
                if (StrFunc.IsFilled(address))
                    address = "(" + address + ")";
                pCci.Display = address;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public void Dump_ToDocument()
        {
            Boolean isSetCciID = false;
            List<CciEnum> lstcci = CciTools.GetCciEnum<CciEnum>("SetCciID").ToList();
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region Security Exchange
                        case CciEnum.Exch:
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            // FI 20130703 [18798] Alimentation du marché avec la donnée renseignée
                            // Il est possible de renseigné un marché incorrect lorsque le trade est en statut d'activation MISSING (uniquement dans le cadre de l'import)
                            FixInstrument.SecurityExchange = data;
                            if (StrFunc.IsFilled(data))
                            {

                                // FI 20200116 [25141] Appel CciTools.GetColumn pour obtenir la colonne qui alimente le cci
                                SQL_TableWithID.IDType IDTypeSearch = CciTools.ParseColumn(CciTools.GetColumn<CciEnum>(CciEnum.Exch, CcisBase.IsModeIO));

                                // FI 20200116 [25141] Appel à la méthode CciTools.IsMarketValid
                                CciTools.IsMarketValid(CciTrade.CSCacheOn, data, IDTypeSearch,
                                    out SQL_Market sqlMarket, out bool isLoaded, out bool isFound, CcisBase.User, CcisBase.SessionId);

                                if (isFound)
                                {
                                    cci.Sql_Table = sqlMarket;
                                    // FI 20200116 [XXXXX] Utilisation de CciExchColumn
                                    cci.NewValue = (sqlMarket.GetFirstRowColumnValue(CciExchColumn)) as string;

                                    //FixInstrument.SecurityExchange = sqlMarket.Identifier;
                                    FixInstrument.SecurityExchange = sqlMarket.FIXML_SecurityExchange;

                                    //Ajout de la d'une party dans le DataDocument
                                    int idClearingHouse = sqlMarket.IdA;
                                    if (idClearingHouse > 0)
                                    {
                                        SQL_Actor sqlClearingHouse = new SQL_Actor(CciTrade.CSCacheOn, idClearingHouse);
                                        if (sqlClearingHouse.IsLoaded)
                                            _cciTrade.DataDocument.AddParty(sqlClearingHouse);
                                    }
                                    //
                                    // RD 20110225
                                    // Sauvegarde du dernier marché utilisé
                                    if (StrFunc.IsFilled(data))
                                        Ccis.TradeCommonInput.SetDefault(CommonInput.DefaultEnum.market, cci.NewValue);
                                }
                                else
                                {
                                    if (isLoaded)
                                        cci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotUnique"), data);
                                    else
                                        cci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotFound"), data);
                                }
                            }

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Security Exchange
                        #region Security ID
                        case CciEnum.ID:
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            if (cci.IsFilledValue)
                            {
                                if ((false == cci.IsNewValueKeyword) ||
                                    (false == _cciExchangeTraded.ExchangeTradedContainer.IsAssetInfoFilled()))
                                {
                                    if (_isETD)
                                        DumpFixInstrument_ToDocument_ETD(cci, data);
                                    else if (_isEST)
                                        DumpFixInstrument_ToDocument_Equity(cci, data);
                                }
                            }
                            else if (cci.IsEmptyValue && cci.IsLastInputByUser)
                            {
                                //on supprime toutes les infos de l'asset lorsque l'utilisateur efface volontairement ID 
                                ClearFixInstrument(false);
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Security ID
                        #region Maturity Month Year
                        case CciEnum.MMY:
                            DumpMaturityMonthYear_ToDocument(cci, data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;

                            break;
                        #endregion Maturity Month Year
                        #region Put Or Call
                        case CciEnum.PutCall:
                            FixInstrument.PutOrCallSpecified = cci.IsFilledValue;
                            if (FixInstrument.PutOrCallSpecified)
                            {
                                PutOrCallEnum putOrCallEnum = (PutOrCallEnum)ReflectionTools.EnumParse(FixInstrument.PutOrCall, data);
                                FixInstrument.PutOrCall = putOrCallEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Put Or Call
                        #region Strike Price
                        case CciEnum.StrkPx:
                            FixInstrument.StrikePriceSpecified = cci.IsFilledValue;
                            if (FixInstrument.StrikePriceSpecified)
                                FixInstrument.StrikePrice.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Strike Price
                        #region Contract Symbol
                        case CciEnum.Sym:
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            // FI 20130702 [18798] Alimentation de symbol avec la donnée renseignée
                            // Il est possible de renseigné un DC incorrect lorsque le trade est en statut d'activation MISSING (uniquement dans le cadre de l'import)
                            FixInstrument.Symbol = data;
                            if (StrFunc.IsFilled(data))
                            {
                                //FI 20100429 use LoadSqlMarketFromFixInstrument
                                int idM = 0;
                                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, null, this.FixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
                                if (null != sqlMarket)
                                    idM = sqlMarket.Id;
                                //
                                SQL_DerivativeContract derivativeContract = new SQL_DerivativeContract(CciTrade.CSCacheOn, data, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                if (derivativeContract.IsLoaded)
                                {
                                    cci.Sql_Table = derivativeContract;
                                    FixInstrument.Symbol = derivativeContract.Identifier;
                                }
                                else
                                {
                                    // FI 20130702 [18798] utilisation de la méthode BuildCciErrMsg 
                                    cci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_DerivativeContractNotFound"), data);
                                }
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Contract Symbol
                        #region ISINCode
                        case CciEnum.ISINCode:
                            FixInstrument.ISINCode = data;
                            break;
                        #endregion ISINCode
                        #region RICCode
                        case CciEnum.RICCode:
                            FixInstrument.RICCode = data;
                            break;
                        #endregion RICCode
                        #region BBGCode
                        case CciEnum.BBGCode:
                            FixInstrument.BBGCode = data;
                            break;
                        #endregion BBGCode
                        #region CFICode
                        case CciEnum.CFICode:
                            FixInstrument.CFICode = data;
                            break;
                        #endregion CFICode
                        #region Issuer
                        case CciEnum.Issuer:
                            FixInstrument.IssuerSpecified = StrFunc.IsFilled(data);
                            FixInstrument.Issuer = data;
                            break;
                        #endregion Issuer
                        #region IssueDate
                        case CciEnum.IssueDate:
                            FixInstrument.IssueDate = new EFS_Date();
                            FixInstrument.IssueDateSpecified = StrFunc.IsFilled(data);
                            if (FixInstrument.IssueDateSpecified)
                                FixInstrument.IssueDate = new EFS_Date(data);
                            break;
                        #endregion IssueDate
                        #region  Asset Exchange Symbol
                        case CciEnum.ExchangeSymbol:
                            FixInstrument.ExchangeSymbol = data;
                            break;
                        #endregion Asset Exchange Symbol
                        // FI 20131126 [19271] 
                        case CciEnum.MatDt:
                            FixInstrument.MaturityDate = new EFS_Date();
                            FixInstrument.MaturityDateSpecified = StrFunc.IsFilled(data);
                            if (FixInstrument.MaturityDateSpecified)
                                FixInstrument.MaturityDate = new EFS_Date(data);
                            break;

                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                    {
                        if (false == isSetCciID)
                            isSetCciID = lstcci.Contains(cciEnum);
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }

                }
            }
            // FI 20220929 [XXXXX] call SetCciID only if isSetCciID == true
            //initilisation 
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) 
                    && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode))
                    && isSetCciID
                    && (null != Cci(CciEnum.ID)))
            {
                SetCciID();
            }
        }

        #endregion IContainerCciFactory Members

        #region ICciGetInfoButton Members
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

            //PL 20100107 Debug:Test if cciPutCall or cciStrkPx is null
            if (IsCci(CciEnum.ID, pCci))
            {
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;

                if (_isETD)
                {
                    pCo.Consultation = "ASSET_ETD_EXPANDED";
                    pCo.SqlColumn = "IDENTIFIER";
                    
                    StringDynamicData IdI = new StringDynamicData(TypeData.TypeDataEnum.@int.ToString(), "IDI", _cciExchangeTraded.Product.IdI.ToString());
                    StringDynamicData market = new StringDynamicData(TypeData.TypeDataEnum.@int.ToString(), "IDM", (null != CciExch.Sql_Table) ? ((SQL_Market)CciExch.Sql_Table).Id.ToString() : string.Empty);
                    StringDynamicData contract = new StringDynamicData(TypeData.TypeDataEnum.@int.ToString(), "IDDC", (null != CciSym.Sql_Table) ? ((SQL_DerivativeContract)CciSym.Sql_Table).Id.ToString() : "0");

                    StringDynamicData maturity = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "MATURITY", CciMMY.NewValue);
                    StringDynamicData putcall = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "PUTCALL", CciPutCall?.NewValue);
                    StringDynamicData strike = new StringDynamicData(TypeData.TypeDataEnum.@decimal.ToString(), "STRIKEPRICE", CciStrkPx?.NewValue);
                    StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "TRADEDATE", CciTrade.DataDocument.TradeHeader.TradeDate.Value)
                    {
                        dataformat = DtFunc.FmtISODate
                    };
                    
                    pCo.DynamicArgument = new string[7] { IdI.Serialize(),  market.Serialize(), contract.Serialize(), maturity.Serialize(), putcall.Serialize(), strike.Serialize(), dt.Serialize()  };
                }
                else if (_isEST)
                {
                    pCo.DynamicArgument = null;
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = "ASSET_EQUITY";
                    pCo.SqlColumn = "IDENTIFIER";
                    pCo.Condition = "TRADE_INPUT";
                    pCo.Fk = null;
                    pCo.Title = "OTC_REF_DATA_UNDERASSET_EQUITY";
                    // FI 20200116 [25141] Utilisation de cciExch.Sql_Table puisque  cciExch.NewValue ne contient plus nécessairement FIXML_SecurityExchange
                    StringDynamicData market = new StringDynamicData(TypeData.TypeDataEnum.text.ToString(), "MARKET", (null != CciExch.Sql_Table) ? ((SQL_Market)CciExch.Sql_Table).Identifier : string.Empty);
                    StringDynamicData IdI = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI", _cciExchangeTraded.Product.IdI.ToString());
                    if (StrFunc.IsFilled(CciExch.NewValue))
                    {
                        pCo.Condition = "TRADE_INPUT";
                        pCo.DynamicArgument = new string[2] { market.Serialize(), IdI.Serialize() };
                    }
                    else
                    {
                        pCo.Condition = "TRADE_INPUT2";
                        pCo.DynamicArgument = new string[1] { IdI.Serialize() };
                    }
                }
            }
            else if (IsCci(CciEnum.MMY, pCci))
            {
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Consultation = "MATURITY";
                pCo.SqlColumn = "MATURITYMONTHYEAR";

                StringDynamicData contract = new StringDynamicData(TypeData.TypeDataEnum.@int.ToString(), "IDDC",
                    (null != CciSym.Sql_Table) ? ((SQL_DerivativeContract)CciSym.Sql_Table).Id.ToString() : "0");

                EFS_Date bizDt = ((ExchangeTradedDerivative)_cciExchangeTraded.Product.Product).TradeCaptureReport.bizDt;
                StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "TRADEDATE", bizDt.Value)
                {
                    dataformat = DtFunc.FmtISODate
                };

                pCo.DynamicArgument = new string[2] { contract.Serialize(), dt.Serialize() };
            }
            else if (IsCci(CciEnum.Exch, pCci))
            {
                // FI 2020016 [25141] Mise à jour de Param et de condition en fonction du facility saisi
                string prefix;
                if (_isEST)
                    prefix = Cst.ProductFamily_ESE;
                else if (_isETD)
                    prefix = "ETD";
                else
                    throw new NotSupportedException("Prefix invalid");

                CustomCaptureInfo cciFacility = CciTrade.CciFacilityParty.Cci(CciMarketParty.CciEnum.identifier);
                if (null != cciFacility.Sql_Table)
                {
                    pCo.Param = new string[] { ((SQL_Market)cciFacility.Sql_Table).XmlId };
                    pCo.Condition = prefix + "_MARKET_FACILITY";
                }
            }
            else if (IsCci(CciEnum.Sym, pCci))
            {
                // FI 20200123 [XXXXX] use by autocomplte and btn3pts
                EFS_Date bizDt = ((ExchangeTradedDerivative)_cciExchangeTraded.Product.Product).TradeCaptureReport.bizDt;
                StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "BUSINESSDATE", bizDt.Value)
                {
                    dataformat = DtFunc.FmtISODate
                };

                int idI = _cciExchangeTraded.Product.IdI;
                StringDynamicData IdI = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI", idI.ToString());

                StringDynamicData market = new StringDynamicData(TypeData.TypeDataEnum.text.ToString(), "MARKET", (null != CciExch.Sql_Table) ? ((SQL_Market)CciExch.Sql_Table).Identifier : string.Empty);

                pCo.DynamicArgument = new string[3] { market.Serialize(), dt.Serialize(), IdI.Serialize() };
            }
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;

        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //
            return isOk;


        }
        #endregion SetButtonZoom
        #endregion ICciGetInfoButton Members
        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsClearMarket"></param>
        /// FI 20131126 [19271] add MaturityDate
        private void ClearFixInstrument(bool pIsClearMarket)
        {

            FixInstrument.SetSecurityID(null,null);

            //Product
            FixInstrument.FixProductSpecified = false;
            //Exchange
            if (pIsClearMarket)
            {
                FixInstrument.SecurityExchange = null;
            }
            //Contract
            FixInstrument.Symbol = null;
            //Maturity
            FixInstrument.MaturityMonthYear = null;
            //StrikePrice
            FixInstrument.StrikePriceSpecified = false;
            FixInstrument.StrikePrice.DecValue = 0;
            //PutOrCall
            FixInstrument.PutOrCallSpecified = false;
            FixInstrument.PutOrCall = PutOrCallEnum.Call;
            //Attribute
            FixInstrument.OptAttribute = string.Empty;
            //ISINCode
            FixInstrument.ISINCode = string.Empty;
            //RICCode
            FixInstrument.RICCode = string.Empty;
            //BBGCode
            FixInstrument.BBGCode = string.Empty;
            //NSINCode
            FixInstrument.NSINCode = string.Empty;
            // CFICode
            FixInstrument.CFICode = string.Empty;
            // Issuer
            FixInstrument.Issuer = string.Empty;
            FixInstrument.IssuerSpecified = false;
            // IssueDate
            FixInstrument.IssueDate = new EFS_Date();
            FixInstrument.IssueDateSpecified = false;

            FixInstrument.MaturityDate = new EFS_Date();
            FixInstrument.MaturityDateSpecified = false;

            // CountryOfIssue
            FixInstrument.CountryOfIssue = string.Empty;
            FixInstrument.CountryOfIssueSpecified = false;
            // StateOrProvinceOfIssue
            FixInstrument.StateOrProvinceOfIssue = string.Empty;
            FixInstrument.StateOrProvinceOfIssueSpecified = false;
            // LocaleOfIssue
            FixInstrument.LocaleOfIssue = string.Empty;
            FixInstrument.LocaleOfIssueSpecified = false;
            //Asset Exchange Symbol
            FixInstrument.ExchangeSymbol = string.Empty;
        }

        /// <summary>
        /// Initialisation des ccis à partir des éléments de FixInstrument
        /// </summary>
        /// FI 20131126 [19271] add 
        private void InitializeFromSecurityID()
        {

            //Exchange
            // FI 20200115 [XXXXX] Prise en considération de CciExchColumn
            string cciExchNewValue = FixInstrument.SecurityExchange;
            if (StrFunc.IsFilled(cciExchNewValue) && CciExchColumn != "FIXML_SecurityExchange")
            {
                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(CciTrade.CSCacheOn, null, FixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
                if (null != sqlMarket)
                    cciExchNewValue = sqlMarket.GetFirstRowColumnValue(CciExchColumn) as string;
            }

            CcisBase.SetNewValue(CciClientId(CciEnum.Exch), cciExchNewValue);

            //Contract
            CcisBase.SetNewValue(CciClientId(CciEnum.Sym), FixInstrument.Symbol);
            //Maturity
            CcisBase.SetNewValue(CciClientId(CciEnum.MMY), FixInstrument.MaturityMonthYear);
            //
            if (_cciExchangeTraded.ExchangeTradedContainer.Category == CfiCodeCategoryEnum.Option)
            {
                string data = string.Empty;
                //Strike
                if (FixInstrument.StrikePriceSpecified)
                    data = FixInstrument.StrikePrice.Value;
                CcisBase.SetNewValue(CciClientId(CciEnum.StrkPx), data);
                //PutCall
                data = string.Empty;
                if (FixInstrument.PutOrCallSpecified)
                    data = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(FixInstrument.PutOrCall);
                CcisBase.SetNewValue(CciClientId(CciEnum.PutCall), data);
            }
            //ISINCode
            CcisBase.SetNewValue(CciClientId(CciEnum.ISINCode), FixInstrument.ISINCode);
            //RICCode
            CcisBase.SetNewValue(CciClientId(CciEnum.RICCode), FixInstrument.RICCode);
            //BBGCode
            CcisBase.SetNewValue(CciClientId(CciEnum.BBGCode), FixInstrument.BBGCode);
            //NSINCode
            CcisBase.SetNewValue(CciClientId(CciEnum.NSINCode), FixInstrument.NSINCode);
            //Asset Exchange Symbol
            CcisBase.SetNewValue(CciClientId(CciEnum.ExchangeSymbol), FixInstrument.ExchangeSymbol);
            // CFICode
            CcisBase.SetNewValue(CciClientId(CciEnum.CFICode), FixInstrument.CFICode);
            // Issuer
            CcisBase.SetNewValue(CciClientId(CciEnum.Issuer), FixInstrument.Issuer);
            // IssueDate
            CcisBase.SetNewValue(CciClientId(CciEnum.IssueDate), FixInstrument.IssueDate.Value);
            // MaturityDate
            CcisBase.SetNewValue(CciClientId(CciEnum.MatDt), FixInstrument.MaturityDate.Value);
        }

        /// <summary>
        /// Dump a Fix Instrument into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFixInstrument_ToDocument_ETD(CustomCaptureInfo pCci, string pData)
        {
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;

            SQL_AssetETD sqlAssetETD = null;

            if (StrFunc.IsFilled(pData))
            {
                int idI = _cciExchangeTraded.Product.IdI;
                //
                int idM = 0;
                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, null, _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlMarket)
                    idM = sqlMarket.Id;
                //
                // FI 20121004 [18172] usage de la méthode ExchangeTradedDerivativeTools.LoadSqlDerivativeContract
                int idDC = 0;
                SQL_DerivativeContract sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(_cciTrade.CSCacheOn, _fixInstrument.SecurityExchange, _fixInstrument.Symbol, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlDerivativeContract)
                    idDC = sqlDerivativeContract.Id;
                //
                decimal strikePrice = decimal.Zero;
                if (_fixInstrument.StrikePriceSpecified)
                    strikePrice = _fixInstrument.StrikePrice.DecValue;
                //
                Nullable<PutOrCallEnum> putCall = null;
                if (_fixInstrument.PutOrCallSpecified)
                    putCall = _fixInstrument.PutOrCall;
                //
                string maturityMonthYear = _fixInstrument.MaturityMonthYear;
                //
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string searchAsset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_assetETD", typeof(string), IDTypeSearch.ToString());
                string[] aSearchAsset = searchAsset.Split(";".ToCharArray());
                int searchCount = aSearchAsset.Length;
                for (int j = 0; j < searchCount; j += 1)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearchAsset[j], true); }
                    catch { continue; }

                    for (int i = 0; i < 3; i++)
                    {
                        //string cs = CSTools.SetTimeOut(CciTrade.CSCacheOn, 60);
                        string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //
                        //FI 20120523 Spheres® effectue un 1er passage où il ne considère que l'identifiant saisi, le marché et l'instrument   
                        //Cela permet de gérer le cas où idDC,putCall,strikePrice, maturityMonthYear sont déjà renseignés avec les infos d'un asset précédent
                        //Il ne faut pas tenir compte des éléments présents dans idDC,putCall,strikePrice, maturityMonthYear qui correspondent aux valeurs de l'asset précédent
                        sqlAssetETD = new SQL_AssetETD(CciTrade.CSCacheOn, IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            IdM_In = idM,
                            IdI_In = idI,
                            MaxRows = 2
                        };
                        isLoaded = sqlAssetETD.IsLoaded;
                        isFound = isLoaded && (1 == sqlAssetETD.RowsCount);

                        // FI 20121123 add isLoaded
                        //if (false == isFound)
                        if ((false == isFound) && isLoaded)
                        {
                            // nouvelle recherche plus fine 
                            sqlAssetETD = new SQL_AssetETD(CciTrade.CSCacheOn, IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                            {
                                IdI_In = idI,
                                IdM_In = idM,
                                IdDC_In = idDC,
                                PutCall_In = putCall,
                                Strike_In = strikePrice,
                                MaturityMonthYear_In = maturityMonthYear,
                                //
                                MaxRows = 2 //NB: Afin de retourner au max 2 lignes (s'ignifiant qu'il y en a plus d'une)
                            };
                            //	
                            isLoaded = sqlAssetETD.IsLoaded;
                            isFound = isLoaded && (1 == sqlAssetETD.RowsCount);
                        }
                        //
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
            }
            //
            if (isFound)
            {
                #region isFound
                pCci.NewValue = sqlAssetETD.Identifier;
                pCci.Sql_Table = sqlAssetETD;
                pCci.ErrorMsg = string.Empty;
                #endregion isFound
            }
            else
            {
                if (pCci.IsFilled)
                {
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetETDNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetETDNotFound");
                }
            }
            //
            if (null != pCci.Sql_Table)
            {
                SQL_AssetETD sql_AssetETD = (SQL_AssetETD)pCci.Sql_Table;
                ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset(CciTrade.CSCacheOn, null, sql_AssetETD,
                    _cciExchangeTraded.ExchangeTradedContainer.Category.Value, FixInstrument, _cciTrade.DataDocument);
            }
        }
        /// <summary>
        /// Dump a Fix Instrument into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFixInstrument_ToDocument_Equity(CustomCaptureInfo pCci, string pData)
        {
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;

            SQL_AssetEquity sqlAsset = null;

            if (StrFunc.IsFilled(pData))
            {
                int idI = _cciExchangeTraded.Product.IdI;
                //
                int idM = 0;
                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, null, _fixInstrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != sqlMarket)
                    idM = sqlMarket.Id;
                //
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string searchAsset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_equitySecurity", typeof(string), IDTypeSearch.ToString());
                string[] aSearchAsset = searchAsset.Split(";".ToCharArray());
                int searchCount = aSearchAsset.Length;
                for (int j = 0; j < searchCount; j += 1)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearchAsset[j], true); }
                    catch { continue; }

                    for (int i = 0; i < 3; i++)
                    {
                        //string cs = CSTools.SetTimeOut(CciTrade.CSCacheOn, 60);
                        string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";
                        //     
                        sqlAsset = new SQL_AssetEquity(_cciTrade.CSCacheOn, IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            IdM_In = idM,
                            IdI_In = idI,
                            MaxRows = 2 //NB: Afin de retourner au max 2 lignes (s'ignifiant qu'il y en a plus d'une)
                        };

                        isLoaded = sqlAsset.IsLoaded;
                        isFound = isLoaded && (1 == sqlAsset.RowsCount);

                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
            }
            //
            if (isFound)
            {
                #region isFound
                pCci.NewValue = sqlAsset.Identifier;
                pCci.Sql_Table = sqlAsset;
                pCci.ErrorMsg = string.Empty;
                #endregion isFound
            }
            else
            {
                //	
                if (pCci.IsFilled)
                {
                    ClearFixInstrument(false);
                    InitializeFromSecurityID();
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetEquityNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetEquityNotFound");
                }
            }
            //
            if (null != pCci.Sql_Table)
            {
                SQL_AssetEquity sql_AssetEquity = (SQL_AssetEquity)pCci.Sql_Table;
                EquitySecurityTransactionTools.SetFixInstrumentFromEquityAsset(CciTrade.CSCacheOn, sql_AssetEquity,
                    _cciExchangeTraded.CciFixTradeCaptureReport.TradeCaptureReport.Instrument,
                    _cciExchangeTraded.CciFixTradeCaptureReport.TradeCaptureReport, _cciTrade.DataDocument);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        // FI 20100402 [16922] Interprétation des échéances abrégées
        private void DumpMaturityMonthYear_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            CustomCaptureInfo cci = pCci;
            cci.Sql_Table = null;

            
             new FixInstrumentContainer(_fixInstrument).SetMaturityMonthYear(CciTrade.CSCacheOn, Ccis.FmtETDMaturityInput, pData);

            /* FI 20220601 [XXXXX] Mise en commentaire
            if (StrFunc.IsFilled(FixInstrument.FixInstrument.MaturityMonthYear))
            {
                //PL 20181026 Remove SetTimeOut() afin de ne pas créer (inutilement) un nouveau POOL ADO.NET avec une nouvelle CS 
                //string cs = CSTools.SetTimeOut(CciTrade.CSCacheOn, 60);
                string cs = CciTrade.CSCacheOn;
                SQL_Maturity sqlMaturity = new SQL_Maturity(cs, pData, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (null != Cci(CciEnum.Sym).Sql_Table)
                {
                    sqlMaturity.idMaturityRuleIn = ((SQL_DerivativeContract)(Cci(CciEnum.Sym).Sql_Table)).IdMaturityRule;
                }

                if (sqlMaturity.IsLoaded)
                {
                    cci.Sql_Table = sqlMaturity;
                    FixInstrument.FixInstrument.MaturityMonthYear = sqlMaturity.Identifier;
                }
            }
            */
            //Spheres met à jour newValue parce puisque de la donnée saisie a peut être interprétée (ex Z15 pour 201512)
            //Dans ce cas on veut que le cci soit en phase avec la valeur présente dans FixInstrument.MaturityMonthYear
            pCci.NewValue = _fixInstrument.MaturityMonthYear;
        }

        /// <summary>
        /// Affecte newValue des ccis gérés par ce CciContainer avec string.Empty
        /// </summary>
        public void Clear()
        {
            //isLastInputByUser = true afin de bien supprimer toutes les infos de l'asset
            //Voir le Dump associé à CciEnum.ID
            Cci(CciEnum.ID).IsLastInputByUser = true;
            //
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pFixInstrument"></param>
        /// <param name="pMsg"></param>
        private static void GetMsgWithOutKeyword(string pValue, out string pMsg)
        {
            //
            bool isWarning = pValue.StartsWith(CustomCaptureInfo.KeywordWarning);
            //
            pValue = pValue.Replace(CustomCaptureInfo.KeywordAlert, string.Empty);
            pValue = pValue.Replace(CustomCaptureInfo.KeywordWarning, string.Empty);
            pValue = pValue.Replace(CustomCaptureInfo.Keyword, string.Empty);
            //   
            if (isWarning)
                pMsg = Ressource.GetString($"Msg_ETD_{pValue}Warning");
            else
                pMsg = Ressource.GetString($"Msg_ETD_{pValue}");
        }
        /// <summary>
        /// Initialisation du CCI CciEnum.ID lorsqu'il est non renseigné.
        /// </summary>
        private void SetCciID()
        {
            if (StrFunc.IsEmpty(FixInstrument.SecurityId))
            {
                if (_isETD)
                {
                    IFixTradeCaptureReport tradeCaptureReport = _cciExchangeTraded.ExchangeTradedContainer.TradeCaptureReport;

                    AssetETDBuilder chkNewAsset = new AssetETDBuilder(CciTrade.CSCacheOn, null, tradeCaptureReport.Instrument,  
                                        _cciExchangeTraded.Product.ProductBase,  tradeCaptureReport.ClearingBusinessDate.DateValue);
                    
                    bool isValid = chkNewAsset.CheckCharacteristicsBeforeCreateAssetETD2(_cciExchangeTraded.ExchangeTradedContainer.Category.Value,  out AssetETDBuilder.CheckMessageEnum VRMsgEnum);
                    if (isValid)
                    {
                        if (AssetETDBuilder.CheckMessageEnum.None != VRMsgEnum)
                        {
                            Cci(CciEnum.ID).NewValue = CustomCaptureInfo.KeywordWarning + VRMsgEnum.ToString();
                        }
                        else
                        {
                            Cci(CciEnum.ID).NewValue = CustomCaptureInfo.Keyword + AssetETDBuilder.CheckMessageEnum.NewAsset.ToString();
                        }
                        Cci(CciEnum.ID).ErrorMsg = string.Empty;
                        //Ce Cci est alimenté avec un mot clef afin d'y afficher un message
                        //Il ne faut donc pas que Spheres® pense que c'est une donnée saisie par l'utilisateur car cela provoquerait la reinitialisation des frais existants
                        //=> donc lastValue = newValue
                        Cci(CciEnum.ID).LastValue = Cci(CciEnum.ID).NewValue;
                    }
                    else if ((AssetETDBuilder.CheckMessageEnum.CharacteristicMissing != VRMsgEnum))
                    {
                        Cci(CciEnum.ID).NewValue = CustomCaptureInfo.KeywordAlert + VRMsgEnum.ToString();
                        Cci(CciEnum.ID).ErrorMsg = string.Empty;
                        //Ce Cci es alimenté avec un mot clef afin d'y afficher un message
                        //Il ne faut donc pas que Spheres® pense que c'est une donnée saisie par l'utilisateur car cela provoquerait la reinitialisation des frais existants
                        //=> donc lastValue = newValue
                        Cci(CciEnum.ID).LastValue = Cci(CciEnum.ID).NewValue;
                    }
                }
            }
        }

        /// <summary>
        /// Recherche de l'asset suite à une modification de la donnée dans {pCci}
        /// </summary>
        /// FI 20130402 [] New method
        private void SearchAsset(CustomCaptureInfo pCci)
        {
            //Recherche de l'asset en fonction des éléments de la série, dès qu'un asset est trouvé il est chargé
            //bool isLoadAsset = (null == Cci(CciEnum.ID).Sql_Table);
            int idI = _cciExchangeTraded.Product.IdI;
            bool isLoadAsset = Cst.Capture.IsModeInput(CcisBase.CaptureMode);
            // RD 20100706 / en mode Importation, ne charger l'Asset que si toutes les infos sont saisies
            if (CcisBase.IsModeIO)
                isLoadAsset &= _cciExchangeTraded.ExchangeTradedContainer.IsAssetInfoFilled();

            if (isLoadAsset)
            {
                if (_isETD)
                {
                    isLoadAsset &= (IsCci(CciEnum.Sym, pCci) || IsCci(CciEnum.PutCall, pCci) || IsCci(CciEnum.StrkPx, pCci) || IsCci(CciEnum.MMY, pCci));
                    isLoadAsset &= pCci.IsLastInputByUser;

                    // RD 20100706 / en mode Importation, ne charger l'Asset que si toutes les Cci ne comporte pas d'erreur
                    if (isLoadAsset)
                    {
                        CustomCaptureInfo cciExch = Cci(CciEnum.Exch);
                        CustomCaptureInfo cciSym = Cci(CciEnum.Sym);
                        CustomCaptureInfo cciPutCall = Cci(CciEnum.PutCall);
                        CustomCaptureInfo cciStrkPx = Cci(CciEnum.StrkPx);
                        CustomCaptureInfo cciMMY = Cci(CciEnum.MMY);

                        if (null != cciExch)
                            isLoadAsset &= (false == cciExch.HasError);
                        if (null != cciSym)
                            isLoadAsset &= (false == cciSym.HasError);
                        if (null != cciPutCall)
                            isLoadAsset &= (false == cciPutCall.HasError);
                        if (null != cciStrkPx)
                            isLoadAsset &= (false == cciStrkPx.HasError);
                        if (null != cciMMY)
                            isLoadAsset &= (false == cciMMY.HasError);
                    }
                }
                else
                {
                    isLoadAsset &= IsCci(CciEnum.Exch, pCci);
                    if (isLoadAsset)
                    {
                        CustomCaptureInfo cciExch = Cci(CciEnum.Exch);
                        if (null != cciExch)
                            isLoadAsset &= (false == cciExch.HasError);
                    }
                }
            }

            if (isLoadAsset)
            {
                IExchangeTradedBase exchangeTraded = (IExchangeTradedBase)_cciExchangeTraded.Product.Product;
                SQL_AssetBase sqlAsset = null;
                if (_isETD)
                {
                    //PL 20130515 Add dtBusiness
                    sqlAsset = AssetTools.LoadAssetETD(CciTrade.CSCacheOn, null, idI, exchangeTraded.Category.Value, _fixInstrument,
                        ((IExchangeTradedDerivative)exchangeTraded).TradeCaptureReport.ClearingBusinessDate.DateValue);
                }
                else if (_isEST)
                {
                    sqlAsset = AssetTools.LoadAssetEquity(null, CciTrade.CSCacheOn, idI, _fixInstrument);
                }

                if (null != sqlAsset)
                {
                    //Pour forcer le dump de l'asset, on vide lastValue 
                    //exemple l'asset est déjà renseigné, je positionne contrat à blanc, si l'asset est à nouveau remonté après le select de LoadAssetETD alors il faut initialiser toutes les zones de la série 
                    Cci(CciEnum.ID).LastValue = string.Empty;
                    Cci(CciEnum.ID).NewValue = sqlAsset.Identifier;
                }
                else
                {
                    //RD 20100323 /on supprime l'idAsset lorsque l'Asset n'est pas trouvé                             
                    FixInstrument.SetSecurityID(null, null);
                    Cci(CciEnum.ID).NewValue = string.Empty;
                }
            }
        }

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New 
        public void ResetCciFacilityHasChanged()
        {
            List<CciEnum> lst = Enum.GetValues(typeof(CciEnum)).Cast<CciEnum>().ToList();
            lst.ForEach(item =>
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200122 [25175] Add Method
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            string keyScreen = string.Empty;
            string keySetting = string.Empty;
            if (_isETD)
            {
                keyScreen = "ETD";
                keySetting = "assetETD";
            }
            else if (_isEST)
            {
                keyScreen = "Equity";
                keySetting = "equitySecurity";
            }

            string id = StrFunc.AppendFormat("{0}{1}tblAsset{2}DescH", CciPageDesign.PrefixTableHID, Prefix, keyScreen);
            if (StrFunc.IsFilled(id))
            {
                if (pPage.FindControl(id) is WCTogglePanel pnl)
                {
                    List<CciEnum> lst = CciTools.GetCciEnum<CciEnum>("AssetCode").ToList();

                    string assetDisplay = (string)SystemSettings.GetAppSettings(StrFunc.AppendFormat("Spheres_TradeDisplay_{0}", keySetting),
                                                                    "ISIN;CFI;RIC;BBG"); /*ISIN;CFI;RIC;BBG order d'affichage par défaut dans le header*/

                    if (StrFunc.IsFilled(assetDisplay))
                    {
                        Type iFixInstrument = _fixInstrument.GetType().GetInterface("IFixInstrument");
                        if (null == iFixInstrument)
                            throw new NullReferenceException(StrFunc.AppendFormat("type:{0}, IFixInstrument interface expected ", iFixInstrument.ToString()));

                        foreach (string item in assetDisplay.Split(';'))
                        {
                            string code = item;
                            if (false == code.EndsWith("CODE"))
                                code += "CODE";

                            bool isOk = Enum.TryParse(code, true, out CciEnum cciCodeEnum) && lst.Contains(cciCodeEnum);
                            if (isOk)
                            {
                                PropertyInfo propertyInfo = iFixInstrument.GetProperty(cciCodeEnum.ToString());
                                if (null != propertyInfo)
                                {
                                    string propertyValue = propertyInfo.GetValue(_fixInstrument) as String;
                                    string data = StrFunc.IsFilled(propertyValue) ? propertyValue : string.Empty;
                                    string style = (string)SystemSettings.GetAppSettings(StrFunc.AppendFormat("Spheres_TradeDisplay_assetStyle_{0}", item), string.Empty);
                                    TradeCustomCaptureInfos.SetLinkInfoInTogglePanel(pnl, cciCodeEnum.ToString(), data, style);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
