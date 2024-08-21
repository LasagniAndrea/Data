using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresIO.Properties;
//
using EfsML;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.Fix;
//
using FixML.Enum;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;

namespace EFS.SpheresIO.MarketData
{

    /// <summary>
    /// Classe de base pour l'intégration des fichiers EUREX
    /// </summary>
    internal abstract class MarketDataImportEurexBase : MarketDataImportBase
    {
        protected const string EUREX_MIC = "XEUR"; // Code MIC de l'EUREX (ISO-10383)
        protected const string EUREX_EXCHANGEACRONYM = "EUR";

        #region Members
        /// <summary>
        /// Permet d'auditer le temps d'exécution 
        /// </summary>
        protected AuditTime audit = null;

        /// <summary>
        /// ExchangeAcronym des marchés pour lesquels rechercher les DerivativeContracts pour lesquels importer les paramètres de risque
        /// </summary>
        /// PM 20170531 [22834] Nouvelle donnée
        private string m_ExchangeAcronym = default(string);
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient l'Exchange Acronym si la tâche d'importation possède le paramètre EXCHANGEACRONYM et qu'il est valorisé
        /// <para>Valeur par defaut: EUREX_EXCHANGEACRONYM</para>
        /// </summary>
        /// PM 20170531 [22834] Nouvel accesseur
        protected string ExchangeAcronym
        {
            get
            {
                if (default(string) == m_ExchangeAcronym)
                {
                    if (task.IoTask.IsParametersSpecified && task.IoTask.ExistTaskParam("EXCHANGEACRONYM"))
                    {
                        m_ExchangeAcronym = task.IoTask.parameters["EXCHANGEACRONYM"];
                    }
                    if (StrFunc.IsEmpty(m_ExchangeAcronym))
                    {
                        m_ExchangeAcronym = EUREX_EXCHANGEACRONYM;
                    }
                }
                return m_ExchangeAcronym;
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pIsUseContractAttrib"></param>
        public MarketDataImportEurexBase(Task pTask, string pDataName, string pDataStyle, Boolean pIsUseContractAttrib)
            : base(pTask, pDataName, pDataStyle, false, pIsUseContractAttrib, AssetFindMaturityEnum.MATURITYMONTHYEAR)
        {
            m_QueryExistDC.parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //m_QueryExistDCInTrade.parameters["EXCHANGEACRONYM"].Value = EUREX_EXCHANGEACRONYM;
            m_QueryExistDCInTrade.parameters["EXCHANGEACRONYM"].Value = ExchangeAcronym;
            m_QueryExistAssetFut.parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
            m_QueryExistAssetOpt.parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
        }
        #endregion Constructor

        #region Methodes
        /// <summary>
        /// Démarre l'audit 
        /// </summary>
        protected void StartAudit()
        {
            audit = new AuditTime();
            audit.StartAuditTime();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSettlementType"></param>
        /// <returns></returns>
        protected static SettlMethodEnum GetSettlementMethod(string pSettlementType)
        {
            SettlMethodEnum ret = SettlMethodEnum.CashSettlement;
            //PM 20140612 [19911] [case "N": //Notional Settlement] doit être considéré comme PhysicalSettlement et non comme CashSettlement
            switch (pSettlementType)
            {
                case "C": //Cash Settlement
                case "P": //Payment-Vs-Payment
                case "T": //Cascada
                case "A": //Alternate
                    ret = SettlMethodEnum.CashSettlement;
                    break;
                case "D": //Derivative
                case "E": //Physical Settlement
                case "N": //Notional Settlement
                case "S": //Stock
                    ret = SettlMethodEnum.PhysicalSettlement;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSettlementType));
            }
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Permet l'importation des fichiers Eurex (Prisma exclu)
    /// </summary>
    internal partial class MarketDataImportEurex : MarketDataImportEurexBase
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask">Tâche IO chargée de l'importation</param>
        /// <param name="pDataName">nom du fichier</param>
        /// <param name="pDataStyle">type de fichier</param>
        public MarketDataImportEurex(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, true)
        {

        }
        #endregion Constructor

        #region Methods

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Market Data EUREX
        /// </summary>
        /// <param name="pOutputFileName"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightMarketDataFile(string pOutputFileName)
        {
            #region Filtrer le fichier source
            try
            {
                #region Declare
                string dcKey_Old = string.Empty;
                string dcKey_New = string.Empty;
                string dc_CONTRACTATTRIBUTE;
                string dc_CONTRACTSYMBOL_Old = string.Empty;
                string dc_CONTRACTSYMBOL_New = string.Empty;
                string currentLine;
                bool isAutoSetting = false;
                bool isAutoSettingAsset = false;
                bool isToCopyOk;
                Dictionary<string, bool> dicDCKey = new Dictionary<string, bool>();
                #endregion

                //Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);
                //Ouverture du fichier d'entrée EUREX d'origine
                OpenInputFileName();

                int guard = 9999999; //143086  reccord dans le fiheir Ct du 20111018
                int lineNumber = 0;
                while (++lineNumber < guard)
                {
                    if ((lineNumber % 10000) == 0)
                        System.Diagnostics.Debug.WriteLine(lineNumber);

                    //Lecture d'une ligne dans le fichier d'entrée EUREX d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //currentLine = IOTools.StreamReader.ReadLine();
                    currentLine = IOCommonTools.StreamReader.ReadLine();

                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }
                    #region Process Ligne par ligne

                    #region !StartsWith("|"): lignes d'information (Header, Footer)
                    //  Si: 
                    //      - la ligne ne commence pas par un Pipeline (|), il s'agit de lignes d'information (Header, Footer)
                    //      --> On recopie la ligne
                    //  Sinon:
                    //      Voir autres cas ci-dessous
                    isToCopyOk = !currentLine.StartsWith("|");
                    #endregion

                    if (isToCopyOk == false)
                    {
                        isAutoSetting = false;

                        // On récupère: Product Family Code (FMC) - (Position 3 Longueur 4)
                        dc_CONTRACTSYMBOL_New = currentLine.Substring(2, 4).Trim();

                        // On récupère: Version number (VN) - (Position 29 Longueur 1)

                        //FL 20130322 Le fichier source a changé de structure maintenant la donnée number (VN) est en (Position 30 Longueur 1)
                        // Ancien Code
                        // dc_CONTRACTATTRIBUTE = currentLine.Substring(28, 1).Trim();
                        dc_CONTRACTATTRIBUTE = currentLine.Substring(29, 1).Trim();

                        #region Check DERIVATIVECONTRACT
                        dcKey_New = dc_CONTRACTSYMBOL_New + "~" + dc_CONTRACTATTRIBUTE;

                        if ((dcKey_New == dcKey_Old)
                            || ((dc_CONTRACTSYMBOL_New == dc_CONTRACTSYMBOL_Old) && (dicDCKey.ContainsKey(dcKey_New))))
                        {
                            //  DC (couple CONTRACTSYMBOL + CONTRACTATTRIBUTE) 
                            //  identique à celui de la ligne immédiatement précédente ou identique à celui d'une ligne précédente.
                            //  On a donc déjà vérifié dans la table DERIVATIVECONTRACT la valeur de ISAUTOSETTINGASSET pour ce DC 
                            //  Si: 
                            //      - ISAUTOSETTINGASSET=True (donc création des ASSET_ETD)
                            //      --> On recopie la ligne, car elle contient un ASSET_ETD
                            //  Sinon: 
                            //      --> On ignore la ligne
                            if (dicDCKey.ContainsKey(dcKey_New))
                                isAutoSettingAsset = dicDCKey[dcKey_New];

                            isToCopyOk = isAutoSettingAsset;
                        }
                        else
                        {
                            dcKey_Old = dcKey_New;
                            if (dc_CONTRACTSYMBOL_New != dc_CONTRACTSYMBOL_Old)
                            {
                                dicDCKey.Clear();
                                dc_CONTRACTSYMBOL_Old = dc_CONTRACTSYMBOL_New;
                            }

                            // Recherche d'existence du DC dans la database
                            m_QueryExistDC.parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL_New;
                            m_QueryExistDC.parameters["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;

                            using (IDataReader drDC = DataHelper.ExecuteReader(task.Cs, CommandType.Text, m_QueryExistDC.query, m_QueryExistDC.parameters.GetArrayDbParameter()))
                            {
                            if (drDC.Read())
                            {
                                //  Le DC existe
                                isAutoSettingAsset = BoolFunc.IsTrue(drDC["ISAUTOSETTINGASSET"]);
                                isAutoSetting = Convert.ToBoolean(drDC["ISAUTOSETTING"]);

                                //  Si: 
                                //      - ISAUTOSETTINGASSET=True (donc création des ASSET_ETD)
                                //      --> On recopie la ligne, car elle contient un ASSET_ETD
                                //          et on fera de même pour toutes les lignes suivantes de même key (CONTRACTSYMBOL + CONTRACTATTRIBUTE)
                                isToCopyOk = isAutoSettingAsset;

                                if (!isToCopyOk)
                                {
                                    //  Le DC n'est pas en ISAUTOSETTINGASSET=True (donc pas de création des ASSET_ETD)
                                    //  On vérifie alors si le DC est malgré tout en ISAUTOSETTING=True (donc maj des caractéristiques du DC)
                                    //
                                    //  Si: 
                                    //      - ISAUTOSETTING=True (maj des caractéristiques du DC), 
                                    //      --> on recopie la ligne
                                    //          et on "ignorera" toutes les lignes suivantes de même key (CONTRACTSYMBOL + CONTRACTATTRIBUTE) 
                                    //  Sinon:
                                    //      --> on ignore la ligne
                                    //          et on "ignorera" toutes les lignes suivantes de même key (CONTRACTSYMBOL + CONTRACTATTRIBUTE) 
                                    isToCopyOk = isAutoSetting;
                                }
                            }
                            else
                            {
                                // Le DC n'existe pas
                                //      --> on recopie la ligne
                                //          et on "ignorera" toutes les lignes suivantes de même key (CONTRACTSYMBOL + CONTRACTATTRIBUTE) 
                                isAutoSettingAsset = false;
                                isToCopyOk = true;
                            }
                            }
                            dicDCKey.Add(dcKey_New, isAutoSettingAsset);
                        }
                        #endregion

                        if ((isToCopyOk == true) && (isAutoSettingAsset) && (isAutoSetting == false))
                        {
                            #region Check ASSET_ETD
                            // On récupère: Product Family Type (CFT) - (Position 8 Longueur 1)
                            // valeurs possibles: 'P','C' ou 'F'
                            string category = GetCategory(currentLine.Substring(7, 1).Trim());
                            // Recherche d'existence de l'ASSET dans la database
                            SetQueryExistAssetParameter(category, "CONTRACTSYMBOL", dc_CONTRACTSYMBOL_New);
                            SetQueryExistAssetParameter(category, "CONTRACTATTRIBUTE", dc_CONTRACTATTRIBUTE);
                            // On récupère: Expiry Date (RED) - (Position 10 Longueur 6)
                            SetQueryExistAssetParameter(category, "MATURITYMONTHYEAR", currentLine.Substring(9, 6).Trim());
                            SetQueryExistAssetParameter(category, "CATEGORY", category);

                            QueryParameters queryParameters = m_QueryExistAssetFut;
                            if (category == "O")
                            {
                                // On récupère: Strike Price (SP) - (Position 20 Longueur 6)
                                string strikePrice = currentLine.Substring(19, 6).Trim();

                                // On récupère: Decimals number current strike price (SPDN) - (Position 27 Longueur 1)

                                //FL 20130322 Le fichier source a changé de structure maintenant 
                                //  la donnée Decimals number current strike price (SPDN) est maintenant de Longueur 2
                                //  Ancien Code
                                //  string strikePriceDivisor = currentLine.Substring(26, 1).Trim();
                                string strikePriceDivisor = currentLine.Substring(26, 2).Trim();

                                // STRIKEPRICE = SP / (10 ^ SPD)
                                m_QueryExistAssetOpt.parameters["STRIKEPRICE"].Value = GetStrikePrice(strikePrice, strikePriceDivisor);

                                // On récupère: Product Family Type (CFT) - (Position 8 Longueur 1)
                                // valeurs possibles: 'P' et 'C'
                                m_QueryExistAssetOpt.parameters["PUTCALL"].Value = GetPutCall(currentLine.Substring(7, 1).Trim());

                                queryParameters = m_QueryExistAssetOpt;
                            }

                            using (IDataReader drAsset = DataHelper.ExecuteReader(task.Cs, CommandType.Text, queryParameters.query, queryParameters.parameters.GetArrayDbParameter()))
                            {
                            // L'Asset n'existe pas dans la database
                            //      --> on recopie la ligne
                            // Dans le cas où l'Assate existe, 
                            //      --> on ne recopie pas la ligne                            
                            if (drAsset.Read())
                                isToCopyOk = false;
                            else
                                isToCopyOk = true;

                            }
                            #endregion
                        }
                    }

                    if (isToCopyOk)
                    {
                        //Ecriture de la ligne dans le fichier de sortie "Light"
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StreamWriter.WriteLine(currentLine);
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                    }
                    #endregion
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer tous les fichiers
                CloseAllFiles();
                }
            #endregion
        }

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Cotations EUREX
        /// </summary>
        /// <param name="pOutputFileName"></param>
        /// EG 20140424 Test DTDISABLED sur DC / DA et ASSET => cas post CA
        /// FI 20170324 [23009] Modify
        /// FI 20170430 [23009] Modify
        /// FI 20170504 [23009] Modify
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20200305 [25077] RDBMS : Correction Erreurs (step4)
        public void Create_LightTheoreticalPriceFile(string pOutputFileName)
        {
            #region Filtrer le fichier source
            try
            {

                // FI 20170324 [23009]  lecture de la date de fichier en priorité
                ReadDtTheoreticalPriceFile();
                
                
                #region Declare

                // FI 20170324 [23009] add IsNewMode
                // FI 20170504 [23009] Mise à jour du commentaire
                // Le but est de récupérer des cours non présents dans PRISMA
                // Il s'agit 
                // - des cours des assets future et options à l'échéance 
                // - des cours EDSP des ss-jacent des assets option sur indice à échéance
                // Ce mode de fonctionnement doit être utilisé avec la tâche EUREX PRISMA RISKDATA
                // La tâche d'importation PRISMA se charge de remonter tous les prix nécessaires à travers les fichiers PRISMA
                // A terme l'importation du fichier tpn ne sera plus appelé par La tâche d'importation PRISMA (discussions en cours avec la chambre pour récupérer les  infos manquantes via les fichiers PRISMA)
                Boolean IsNewMode = m_dtFile.CompareTo(new DateTime(2017, 01, 01)) > 0;  

                // RD 20130828 [18834] 
                // Initialisation de la requête avec ses paramètres permettant de vérifier:
                // 1 - l'existance d'au moins un trade à partir du CONTRACTSYMBOL d'un DC
                // 2 - si la checkbox "Importation systematique des cotations" du DC est bien checkée

                // FI 20170324 [23009] Si IsNewMode => seuls les DC options et futures avec assets à échéance sont chargés
                // FI 20170504 [23009] Mise à jour du commentaire précédent
                #region sqlQuery_Dc

                // 1 - l'existance d'au moins un trade à partir du CONTRACTSYMBOL d'un DC
                string sqlQuery_Dc = @"select dc.ISMANDATORYIMPORTQUOTE
                from dbo.TRADE tr
                inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)" + Cst.CrLf;

                if (IsNewMode) // FI 20170324 [23009] Seuls les DCs pour lesquels il existe des assets à échéance sont considérés
                    sqlQuery_Dc += @" and (isnull(ma.MATURITYDATESYS,ma.MATURITYDATE)= @DTFILE)" + Cst.CrLf;

                sqlQuery_Dc += @"inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where ({1}) and ({2})" + Cst.CrLf;

                // 2 - si la checkbox "Importation systematique des cotations" du DC est bien checkée
                sqlQuery_Dc += @"union

                select dc.ISMANDATORYIMPORTQUOTE
                from dbo.DERIVATIVECONTRACT dc
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.ISMANDATORYIMPORTQUOTE = 1) and ({2})";

                sqlQuery_Dc = String.Format(sqlQuery_Dc, DataHelper.SQLString(EUREX_MIC), 
                    OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf; 
                
                DataParameters dataParameters_Dc = new DataParameters();
                dataParameters_Dc.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Dc.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                #endregion

                // RD 20130828 [18834] 
                // Initialisation de la requête avec ses paramètres permettant de charger des infos sur le DC
                #region sqlQuery_DcInfo
                string sqlQuery_DcInfo = SQLCst.SELECT + "dc.IDENTIFIER, dc.IDI" + Cst.CrLf;
                sqlQuery_DcInfo += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
                sqlQuery_DcInfo += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " mk" + SQLCst.ON + "mk.IDM = dc.IDM"
                                                                                       + SQLCst.AND + "mk.ISO10383_ALPHA4 =" + DataHelper.SQLString(EUREX_MIC) + Cst.CrLf;

                sqlQuery_DcInfo += SQLCst.WHERE + "dc.CONTRACTSYMBOL = @CONTRACTSYMBOL"
                                + SQLCst.AND + "dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE"
                                + SQLCst.AND + "dc.CATEGORY = @CATEGORY" + Cst.CrLf;
                sqlQuery_DcInfo += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE") + ")" + Cst.CrLf;

                DataParameters dataParameters_DcInfo = new DataParameters();
                dataParameters_DcInfo.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_DcInfo.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_DcInfo.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_DcInfo.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CATEGORY));

                #endregion

                // RD 20130828 [18834]
                // Asset de Category Future: Initialisation de la requête avec ses paramètres permettant de vérifier:
                // 1 - l'existance de l'Actif Futures dans au moins un trade dans Spheres.
                // 2 - l'existance d'un Actif Option sur Future dans au moins un trade dans Spheres, qui a comme sous jacent cet Actif Futures.
                // 3 - que le DC Futures a pour SSJ un Indice ET qu'il existe un Actif Option dans au moins un trade dans Spheres ayant pour SSJ le même que celui du Future en question, donc le même Indice.
                // 4 - si la checkbox "Importation systematique des cotations" du DC est bien checkée                
                #region sqlQuery_Asset_Future

                // FL 20130529: Modification de cette requête pour gérer le cas ou on a un Actif Options dans au moins un trade dans Spheres qui a comme sous jacent cet Actif Futures.
                // Cf (Ticket: 18706 )

                // 1 - l'existance de l'Actif Futures dans au moins un trade dans Spheres.
                // 2 - l'existance d'un Actif Option sur Future dans au moins un trade dans Spheres, qui a comme sous jacent cet Actif Futures (union).
                // RD 20130828 - En principe, l'Asset Futures existe forcement, car créé à la saisie du trade Option
                // En cas de problème, mettre un « outer join » sur les tables MATURITY, DERIVATIVEATTRIB et ASSET_ETD
                // Ainsi, l'Asset sera créé par Spheres I/O le cas échéant.
                string sqlQuery_Asset_Future = String.Format(@"select asset.IDASSET
                from dbo.ASSET_ETD asset
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY)
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where ({1}) and ({2})

                union all

                select asset_fut.IDASSET
                from dbo.ASSET_ETD asset_opt
                inner join dbo.DERIVATIVEATTRIB da_opt on (da_opt.IDDERIVATIVEATTRIB = asset_opt.IDDERIVATIVEATTRIB)
                inner join dbo.DERIVATIVECONTRACT dc_opt on (dc_opt.IDDC = da_opt.IDDC) and (dc_opt.CATEGORY = 'O') and (dc_opt.ASSETCATEGORY = 'Future')
                inner join dbo.DERIVATIVECONTRACT dc_fut on (dc_fut.IDDC = dc_opt.IDDC_UNL) and (dc_fut.CONTRACTSYMBOL = @CONTRACTSYMBOL) and 
                    (dc_fut.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc_fut.CATEGORY = @CATEGORY)
                inner join dbo.MARKET mk on (mk.IDM = dc_fut.IDM) and (mk.ISO10383_ALPHA4 = {0})
                inner join dbo.DERIVATIVEATTRIB da_fut on (da_fut.IDDC = dc_fut.IDDC)
                inner join dbo.MATURITY ma_fut on (ma_fut.IDMATURITY = da_fut.IDMATURITY) and (ma_fut.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
                inner join dbo.ASSET_ETD asset_fut on (asset_fut.IDDERIVATIVEATTRIB = da_fut.IDDERIVATIVEATTRIB)
                where ({3}) and ({4}) and ({5}) and ({6})", 
                DataHelper.SQLString(EUREX_MIC),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc_opt", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc_fut", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset_opt", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset_fut", "DTFILE")) + Cst.CrLf;

                // 3 - le DC Futures a pour SSJ un Indice ET qu'il existe un Actif Option sur Indice dans au moins un trade dans Spheres ayant pour SSJ le même que celui du Future en question, donc le même Indice.
                // RD/FL 20140527 [20009] Attention au critère qui doit être au niveau du LEFT et non pas dans la clause WHERE
                string sqlQuery_Asset_Future2 = String.Format(@"select asset_fut.IDASSET
                from dbo.ASSET_ETD asset_opt
                inner join dbo.DERIVATIVEATTRIB da_opt on (da_opt.IDDERIVATIVEATTRIB = asset_opt.IDDERIVATIVEATTRIB)
                inner join dbo.DERIVATIVECONTRACT dc_opt on (dc_opt.IDDC = da_opt.IDDC) and (dc_opt.CATEGORY = 'O') and (dc_opt.ASSETCATEGORY = 'Index')
                inner join dbo.DERIVATIVECONTRACT dc_fut on (dc_fut.IDDC = dc_opt.IDDC_UNL) and (dc_fut.CONTRACTSYMBOL = @CONTRACTSYMBOL) and 
                    (dc_fut.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc_fut.CATEGORY = @CATEGORY) and 
                    (dc_fut.ASSETCATEGORY = 'Index') and (dc_fut.IDASSET_UNL = dc_opt.IDASSET_UNL)
                inner join dbo.MARKET mk on (mk.IDM = dc_fut.IDM) and (mk.ISO10383_ALPHA4 = {0})
                left outer join dbo.MATURITY ma_fut on (ma_fut.IDMATURITYRULE = dc_fut.IDMATURITYRULE) and (ma_fut.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
                left outer join dbo.DERIVATIVEATTRIB da_fut on (da_fut.IDDC = dc_fut.IDDC) and (da_fut.IDMATURITY = ma_fut.IDMATURITY)
                left outer join dbo.ASSET_ETD asset_fut on (asset_fut.IDDERIVATIVEATTRIB = da_fut.IDDERIVATIVEATTRIB) and ({1})
                where ({2}) and ({3}) and ({4})",
                DataHelper.SQLString(EUREX_MIC),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset_fut", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc_opt", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset_opt", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc_fut", "DTFILE")) + Cst.CrLf;

                // 4 - la checkbox "Importation systematique des cotations" du DC est bien checkée                
                string sqlQuery_Asset_Future_IsMandatoryQuote = String.Format(@"union all
                select asset.IDASSET
                from dbo.DERIVATIVECONTRACT dc             
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                inner join dbo.MATURITY ma on (ma.IDMATURITYRULE = dc.IDMATURITYRULE) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC) and (da.IDMATURITY = ma.IDMATURITY)
                inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
                where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY) and ({1}) and ({2})",
                DataHelper.SQLString(EUREX_MIC),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE")) + Cst.CrLf;

                DataParameters dataParameters_Asset_Future = new DataParameters();
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                #endregion

                // RD 20130828 [18834] 
                // Asset de Category Option: Initialisation de la requête avec ses paramètres permettant de vérifier:
                // 1 - l'existance d'au moins un trade dans Spheres à partir d'un Asset Option
                // 2 - si la checkbox "Importation systematique des cotations" du DC est bien checkée                
                #region sqlQuery_Asset_Option
                // 1 - l'existence d'au moins un trade dans Spheres à partir d'un Asset Option
                string sqlQuery_Asset_Option = String.Format(@"select asset.IDASSET, asset_other.IDASSET as IDASSET_OTHER, tr.IDT
                from dbo.DERIVATIVECONTRACT dc             
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)
                left outer join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB) and (asset.PUTCALL = @PUTCALL) and (asset.STRIKEPRICE = @STRIKEPRICE)
                and ({1})
                left outer join dbo.ASSET_ETD asset_other on (asset_other.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB) and (asset_other.PUTCALL = @PUTCALL)
                and ({2})
                left outer join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET) and (asset_other.PUTCALL = @PUTCALL)
                where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY) and ({3})",
                DataHelper.SQLString(EUREX_MIC),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset_other", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf;

                // 2 - la checkbox "Importation systematique des cotations" du DC est bien checkée                
                string sqlQuery_Asset_Option_IsMandatoryQuote = String.Format(@"union
                select asset.IDASSET, null as IDASSET_OTHER, null as IDT
                from dbo.DERIVATIVECONTRACT dc             
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                inner join dbo.MATURITY ma on (ma.IDMATURITYRULE = dc.IDMATURITYRULE) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC) and (da.IDMATURITY = ma.IDMATURITY)
                inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB) and (asset.PUTCALL = @PUTCALL) and (asset.STRIKEPRICE = @STRIKEPRICE)
                where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY) and ({1}) and ({2})",
                DataHelper.SQLString(EUREX_MIC),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE")) + Cst.CrLf;

                DataParameters dataParameters_Asset_Option = new DataParameters();
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                dataParameters_Asset_Option.Add(new DataParameter(cs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                dataParameters_Asset_Option.Add(new DataParameter(cs, "STRIKEPRICE", DbType.Decimal));

                #endregion

                // Initialisation de la requête avec ses paramètres permettant d'avoir l'asset du Sous jacent ainsi que le nom de la table de cotation associée.
                #region sqlQuery_Unl_Asset
                string sqlQuery_Unl_Asset = @"select main.IDASSET as source, main.ASSETCATEGORY, main.SETTLTMETHOD, main.MATURITYDATE, main.MATURITYDATESYS,
                case main.ASSETCATEGORY when 'Future' then main.DA_IDASSET else main.IDASSET_UNL end as unl_IDASSET,
                case main.ASSETCATEGORY when 'Future' then 'QUOTE_ETD_H'
                                        when 'Bond' then 'QUOTE_DEBTSEC_H'
                                        when 'Commodity' then 'QUOTE_COMMODITY_H'
                                        when 'EquityAsset' then 'QUOTE_EQUITY_H'
                                        when 'FxRateIndex' then 'QUOTE_FXRATE_H'
                                        when 'Index' then 'QUOTE_INDEX_H'
                                        when 'RateIndex' then 'QUOTE_RATEINDEX_H'
                                        else null end as TBL_UNLQUOTE
                from dbo.VW_ASSET_ETD_EXPANDED main
                where (main.IDASSET = @IDASSET)" + Cst.CrLf;

                DataParameters dataParameters_Unl_Asset = new DataParameters();
                dataParameters_Unl_Asset.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET));
                #endregion

                string Record_Type = string.Empty;
                string dc_CONTRACTSYMBOL = string.Empty;
                string dc_CONTRACTATTRIBUTE = string.Empty;
                string dc_CATEGORY = string.Empty;
                bool dc_IsMandatoryQuoteImport = false;
                string asset_PUTCALL = string.Empty;
                string maturity_MATURITYMONTHYEAR = string.Empty;
                string currentLine = string.Empty;
                string currentLine_MarginClass = string.Empty;
                string currentLine_Product = string.Empty;
                string currentLine_Expiry = string.Empty;
                string currentLine_SeriePlus = string.Empty;

                // true si  asset de type Option sur indice en Cash settlement  
                bool currentLine_IsCSIndexOption = false;

                // true si asset à l'échéance  
                // FI 20170504 [23009] Add
                bool currentLine_IsExpiryDate = false;
                
                // true si asset de type Option sur indice à l'échéance  
                bool currentLine_IsExpiryDateIndexOption = false;
                // true si asset de type future sur indice à l'échéance  
                // FI 20170403 [23009] add currentLine_IsExpiryDateIndexFuture 
                bool currentLine_IsExpiryDateIndexFuture = false;


                string tablename_Unl_Quote = string.Empty;

                decimal asset_STRIKEPRICE = 0;
                decimal StrikeDivisor = 1;

                int idasset = 0;
                int idasset_other = 0;
                int idasset_unl = 0;

                bool isToCopyOk = false;
                bool isToCopyDerivativeContract = false;
                bool isToCopySerie = false;
                bool isToBufferSerie = false;
                ArrayList alBufferSerie = new ArrayList();
                string CSIndexOptionEDSP = string.Empty; //EDSP d'une Option sur Indice en Cash-Settlement

                
                //// FI 20170324 [23009]  add IsSerieAdded (retourne true dès qu'un enregistrement série est ajouté pour une échéance donnée)
                //Boolean isSerieAdded =false;

                #endregion


                OpenOutputFileName(pOutputFileName);

                // FI 20170324 [23009]  Mise en commentaire l'appel est déjà effectué
                //ReadDtTheoreticalPriceFile();

                OpenInputFileName();


                int guard = 99999999;
                int lineNumber = 0;
                while (++lineNumber < guard)
                {
                    if ((lineNumber % 10000) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine(lineNumber);
                    }

                    //Lecture d'une ligne dans le fichier d'entrée EUREX d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //currentLine = IOTools.StreamReader.ReadLine();
                    currentLine = IOCommonTools.StreamReader.ReadLine();

                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }

                    #region Process Ligne par ligne

                    // Lecture du Type de l’enregistrement du fichier d’entrée (Position 1 Longueur 1)
                    Record_Type = currentLine.Substring(0, 1);

                    switch (Record_Type)
                    {
                        #region Record Type => Margin class Record (M)
                        case "M":
                            //Ecriture systématique de la ligne dans le fichier "Light" de sortie.
                            // FI 20170324 [23009] Alimentation decurrentLine_MarginClass
                            currentLine_MarginClass = currentLine;
                            // FI 20170324 [23009] si IsNewMode la copie des lignes M n'est pas systématique
                            isToCopyOk = IsNewMode ? false : true;
                            break;
                        #endregion

                        #region Record Type => Product Record (P)
                        case "P":
                            // WARNING: Jamais d'écriture, à ce stade, de cette ligne "Product Record (P)" dans le fichier "Light" de sortie. 
                            //          Cette écriture s'effectue dans le cadre des "Expiry Record (E)".
                            isToCopyOk = false;

                            currentLine_Product = currentLine;

                            // Lecture du Product ID (Position 2 Longueur 4) 
                            dc_CONTRACTSYMBOL = currentLine.Substring(1, 4).Trim();

                            // Lecture du nombre de décimale du Strike (Position 23 Longueur 1) 
                            StrikeDivisor = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(currentLine.Substring(22, 1))));

                            // Vérification de l'existance d'au moins un trade à partir du CONTRACTSYMBOL du DC
                            dataParameters_Dc["DTFILE"].Value = m_dtFile;
                            dataParameters_Dc["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;

                            // RD 20130828 [18834] Gestion de la check "Importation systématique des cotations" et création du nouvel asset
                            //object obj = DataHelper.ExecuteScalar(cs, CommandType.Text, sqlQuery_Dc, dataParameters_Dc.GetArrayDbParameter());
                            //isToCopyDerivativeContract = (obj != null);
                            using (IDataReader drDerivContract = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery_Dc, dataParameters_Dc.GetArrayDbParameter()))
                            {
                            if (drDerivContract.Read())
                            {
                                isToCopyDerivativeContract = true;
                                dc_IsMandatoryQuoteImport = false; 
                                // FI 20170324 [23009]  si IsNewMode (dc_IsMandatoryQuoteImport == false)
                                if (false == IsNewMode)
                                    dc_IsMandatoryQuoteImport = BoolFunc.IsTrue(drDerivContract["ISMANDATORYIMPORTQUOTE"]);
                            }
                            else
                            {
                                isToCopyDerivativeContract = false;
                                dc_IsMandatoryQuoteImport = false;
                            }
                            }
                            break;
                        #endregion

                        #region Record Type => Expiry Record (E)
                        case "E":
                            // WARNING: Jamais d'écriture, à ce stade, de cette ligne "Expiry Record (E)" dans le fichier "Light" de sortie. 
                            //          Cette écriture s'effectue dans le cadre des "Serie Record (S)" et ce uniquement quand il existe au moins trade sur la série.
                            //          Par contre c'est ici que l'on écrit le "Product Record (P)" lu précédemment, dans le fichier "Light" de sortie.
                            isToCopyOk = false;

                            alBufferSerie.Clear();
                            CSIndexOptionEDSP = string.Empty;

                            currentLine_Expiry = currentLine;
                            currentLine_SeriePlus = string.Empty;
                            currentLine_IsCSIndexOption = false;

                            // FI 20170504 [23009] add currentLine_IsExpiryDate
                            currentLine_IsExpiryDate = false;
                            currentLine_IsExpiryDateIndexOption = false;
                            // FI 20170430 [23009] 
                            currentLine_IsExpiryDateIndexFuture = false;  
                            //isSerieAdded = false; // FI 20170324 [23009] 


                            // Lecture du type d'option Call/Put (Position 2 Longueur 1)
                            switch (currentLine.Substring(1, 1).Trim())
                            {
                                case "P":
                                    asset_PUTCALL = "0";
                                    dc_CATEGORY = "O";
                                    break;

                                case "C":
                                    asset_PUTCALL = "1";
                                    dc_CATEGORY = "O";
                                    break;

                                default:
                                    asset_PUTCALL = null;
                                    dc_CATEGORY = "F";
                                    //NB: Ecriture systématique concernant les Futures, qu'il existe ou non un trade portant sur le DC
                                    //isToCopyDerivativeContract = true;
                                    // FI 20170324 [23009] => Pas d'écriture systématique des contrats Futures en IsNewMode (Seuls les assets à échéance sont considérés) 
                                    if (false == IsNewMode)
                                        isToCopyDerivativeContract = true;
                                    break;
                            }

                            // Lecture de l’échéance (Position 3 Longueur 4) NB: Maturité au format 'AAMM' on rajoute le siecle 
                            maturity_MATURITYMONTHYEAR = "20" + currentLine.Substring(2, 4).Trim();

                            if (isToCopyDerivativeContract && (currentLine_Product.Length > 0))
                            {
                                // FI 20170324 [23009] si IsNewMode alimentation de la ligne MarginClass
                                if ((IsNewMode) && currentLine_MarginClass.Length > 0)
                                {
                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    //IOTools.StreamWriter.WriteLine(currentLine_MarginClass);
                                    IOCommonTools.StreamWriter.WriteLine(currentLine_MarginClass);
                                    currentLine_MarginClass = string.Empty; 
                                }
                                
                                //Ecriture dans le fichier "Light" de sortie du "Product Record (P)" lu précédemment.
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(currentLine_Product);
                                IOCommonTools.StreamWriter.WriteLine(currentLine_Product);
                                currentLine_Product = string.Empty; // Reset de currentLine_Product afin de ne générer qu'un seul "Product Record (P)"
                            }

                            break;
                        #endregion

                        #region Record Type => (Series Record (S) )
                        case "S":
                            // Lecture du Strike (Position 2 Longueur 6) [EXER-PRC-RMTHED  PIC 9(6)]
                            asset_STRIKEPRICE = Convert.ToDecimal(currentLine.Substring(1, 6)) / StrikeDivisor;

                            // Lecture du  N° de version de la série (Position 8 Longueur 1) [SERI-VERS-NO-RMTHED  PIC 9(1)]
                            dc_CONTRACTATTRIBUTE = currentLine.Substring(7, 1).Trim();

                            // Vérification de l'existance d'au moins un trade à partir de l'ASSET (ProductID + N° De version + Maturity + Strike + Put/Call)
                            #region Vérification de l'existence de l'ASSET
                            isToCopySerie = false;
                            idasset = 0;
                            idasset_other = 0;
                            if (isToCopyDerivativeContract)
                            {
                                if (dc_CATEGORY == "O")
                                {
                                    #region OPTION
                                    dataParameters_Asset_Option["DTFILE"].Value = m_dtFile;
                                    dataParameters_Asset_Option["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    dataParameters_Asset_Option["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;
                                    dataParameters_Asset_Option["CATEGORY"].Value = dc_CATEGORY;
                                    dataParameters_Asset_Option["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                    dataParameters_Asset_Option["PUTCALL"].Value = asset_PUTCALL;
                                    dataParameters_Asset_Option["STRIKEPRICE"].Value = asset_STRIKEPRICE;

                                    string sqlQuery_Asset = sqlQuery_Asset_Option;
                                    if (dc_IsMandatoryQuoteImport)
                                        sqlQuery_Asset += sqlQuery_Asset_Option_IsMandatoryQuote;

                                    using (IDataReader drIDASSET = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery_Asset, dataParameters_Asset_Option.GetArrayDbParameter()))
                                    {
                                    if (drIDASSET.Read())
                                    {
                                        isToCopySerie = (drIDASSET["IDT"] != Convert.DBNull); // Existence d'un trade sur cette option 
                                        if (drIDASSET["IDASSET"] != Convert.DBNull)
                                        {
                                            idasset = Convert.ToInt32(drIDASSET["IDASSET"]); // Asset existant dans le référentiel
                                        }
                                        else if (drIDASSET["IDASSET_OTHER"] != Convert.DBNull)
                                        {
                                            idasset_other = Convert.ToInt32(drIDASSET["IDASSET_OTHER"]); // utilisation d'un Asset sur un autre Strike
                                        }
                                    }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region FUTURE
                                    dataParameters_Asset_Future["DTFILE"].Value = m_dtFile;
                                    dataParameters_Asset_Future["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    dataParameters_Asset_Future["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;
                                    dataParameters_Asset_Future["CATEGORY"].Value = dc_CATEGORY;
                                    dataParameters_Asset_Future["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;

                                    string sqlQuery_Asset = sqlQuery_Asset_Future;

                                    string sqlQuery_Asset2 = sqlQuery_Asset_Future2;
                                    if (dc_IsMandatoryQuoteImport)
                                        sqlQuery_Asset2 += sqlQuery_Asset_Future_IsMandatoryQuote;

                                    using (IDataReader drIDASSET = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery_Asset, dataParameters_Asset_Future.GetArrayDbParameter()))
                                    {
                                        isToCopySerie = drIDASSET.Read();
                                        if (isToCopySerie)
                                    {
                                        if (drIDASSET["IDASSET"] != Convert.DBNull)
                                            idasset = Convert.ToInt32(drIDASSET["IDASSET"]); // Asset existant dans le référentiel
                                    }
                                    }

                                    if ((false == isToCopySerie) && (false == IsNewMode))
                                    {
                                        using (IDataReader drIDASSET = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery_Asset2, dataParameters_Asset_Future.GetArrayDbParameter()))
                                        {
                                            isToCopySerie = drIDASSET.Read();
                                            if (isToCopySerie)
                                            {
                                                if (drIDASSET["IDASSET"] != Convert.DBNull)
                                                    idasset = Convert.ToInt32(drIDASSET["IDASSET"]); // Asset existant dans le référentiel
                                            }
                                        }
                                    }
                                    #endregion
                                }

                                // RD 20130828 [18834] Gestion de la check "Importation systématique des cotations" et création du nouvel asset
                                if (dc_IsMandatoryQuoteImport)
                                    isToCopySerie = true;

                                if (isToCopySerie && (idasset == 0))
                                {
                                    #region Création d'un nouvel l'Asset
                                    string dc_IDENTIFIER = string.Empty;
                                    int dc_IDI = 0;
                                    ExchangeTradedDerivative unknownProduct = new ExchangeTradedDerivative();
                                    

                                    // 1- Chargement des informations nécessaires pour la création du nouvel Asset
                                    dataParameters_DcInfo["DTFILE"].Value = m_dtFile;
                                    dataParameters_DcInfo["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    dataParameters_DcInfo["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;
                                    dataParameters_DcInfo["CATEGORY"].Value = dc_CATEGORY;

                                    DataSet dsDCInfo = DataHelper.ExecuteDataset(CSTools.SetCacheOn(cs), CommandType.Text, sqlQuery_DcInfo, dataParameters_DcInfo.GetArrayDbParameter());
                                    DataTable dtDCInfo = dsDCInfo.Tables[0];

                                    if (dtDCInfo.Rows.Count > 0)
                                    {
                                        dc_IDENTIFIER = (dtDCInfo.Rows[0]["IDENTIFIER"] == Convert.DBNull ? string.Empty : dtDCInfo.Rows[0]["IDENTIFIER"].ToString());
                                        dc_IDI = (dtDCInfo.Rows[0]["IDI"] == Convert.DBNull ? 0 : Convert.ToInt32(dtDCInfo.Rows[0]["IDI"]));
                                    }
                                    // FI 20180927 [24202] Add isAssetAlreadyExist
                                    string validationMsg = string.Empty; //Message de warning associé à la création de l'asset
                                    string infoMsg = string.Empty; //Message d'info des actions menées
                                    Boolean isAssetAlreadyExist = false;
                                    // 2- Création du nouvel Asset
                                    SQL_AssetETD sqlAsset = AssetTools.CreateAssetETD(cs, (IProductBase)unknownProduct,
                                        EUREX_MIC, dc_IDI,
                                        dc_IDENTIFIER, dc_CATEGORY, maturity_MATURITYMONTHYEAR,
                                        asset_PUTCALL, asset_STRIKEPRICE,
                                        task.process.UserId, m_dtFile, out validationMsg, out infoMsg, out isAssetAlreadyExist);

                                    idasset = sqlAsset.Id;
                                    if (false == isAssetAlreadyExist)
                                    {
                                        string msgLog = Ressource.GetString("Msg_NewUnderlying");
                                        msgLog = msgLog.Replace("{0}", sqlAsset.Identifier);

                                        if (StrFunc.IsFilled(infoMsg))
                                            msgLog += Cst.CrLf2 + infoMsg;
                                        //
                                        if (StrFunc.IsFilled(validationMsg))
                                        {
                                            //"Msg_ETD_ValidationRuleWarning" Attention, certaines règles de validation ne sont pas respectées:
                                            msgLog += Cst.CrLf2 + Ressource.GetString("Msg_ETD_ValidationRuleWarning");
                                            msgLog += Cst.CrLf + validationMsg;
                                        }

                                        ProcessLogInfo _logInfo = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, idasset, string.Empty, new string[] { msgLog });
                                        _logInfo.levelOrder = 3;
                                        task.process.ProcessLogAddDetail2(_logInfo);

                                        // PM 20200102 [XXXXX] New Log
                                        Logger.Log(new LoggerData(LogLevelEnum.None, msgLog, 3, new LogParam(idasset, default(string), default(string), Cst.LoggerParameterLink.IDDATA)));
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            #region Ajout dans le record de la serie de: Identifiant de l'asset + Identifiant de l'asset sous-jacent + Tablename de stockage des cotations du sous-jacent
                            // Initialisation de: 
                            // - la variable currentLine_SeriePlus qui contient l'asset du sous jacent et le nom de la table de cotation du sous jacent.
                            // - la variable currentLine_IsCSIndexOption
                            // - la variable currentLine_IsExpiryDateIndexOption
                            // Rq: On ne recherche l'Asset du sous-jacent et le Tablename de stockage des cotations du sous-jacent que lors d'une nouvelle échéance.

                            //PL 20130619 Recherche effectuée même si aucun trade n'est négocié sur cet asset quand il s'agit d'une option. 
                            //            En effet, dans le cas d'une Option sur Indice, en Cash-Settlment, cet asset est peut-être le seul à disposer d'un EDSP.
                            //if (isToCopySerie & (currentLine_SeriePlus.Length == 0))
                            if (((idasset > 0) || (idasset_other > 0)) && (currentLine_SeriePlus.Length == 0))
                            {
                                #region Obtention des caractéristiques de l'Asset sous-jacent.
                                dataParameters_Unl_Asset["IDASSET"].Value = (idasset > 0 ? idasset : idasset_other);

                                using (IDataReader asset_Unl_asset = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery_Unl_Asset, dataParameters_Unl_Asset.GetArrayDbParameter()))
                                {
                                if (asset_Unl_asset.Read())
                                {
                                    // 10 caractères pour l'asset
                                    if (asset_Unl_asset["unl_IDASSET"] == Convert.DBNull)
                                    {
                                        currentLine_SeriePlus = "         0";
                                    }
                                    else
                                    {
                                        idasset_unl = Convert.ToInt32(asset_Unl_asset["unl_IDASSET"]);
                                        currentLine_SeriePlus = idasset_unl.ToString().PadLeft(10);
                                    }
                                    // 18 caractères pour la table
                                    if (asset_Unl_asset["TBL_UNLQUOTE"] == Convert.DBNull)
                                    {
                                        currentLine_SeriePlus += "              Null"; //18 caractères
                                    }
                                    else
                                    {
                                        tablename_Unl_Quote = Convert.ToString(asset_Unl_asset["TBL_UNLQUOTE"]);
                                        currentLine_SeriePlus += tablename_Unl_Quote.ToString().PadLeft(18);
                                    }

                                    /* FI 20170504 [23009] Mise en commentaire  (nouvelle écriture voir plus bas)
                                    if ((asset_Unl_asset["ASSETCATEGORY"] != Convert.DBNull) && (asset_Unl_asset["SETTLTMETHOD"] != Convert.DBNull))
                                    {
                                        if (dc_CATEGORY == "O")
                                        {
                                            // RD 20130828 [18834] Bug: Ce traitement concerne uniquement les Options (comme le nom des variables l'indique: currentLine_IsCSIndexOption, currentLine_IsExpiryDateIndexOption)
                                            currentLine_IsCSIndexOption = (asset_Unl_asset["ASSETCATEGORY"].ToString() == Cst.UnderlyingAsset_ETD.Index.ToString())
                                                && (asset_Unl_asset["SETTLTMETHOD"].ToString() == SettlMethodEnum.CashSettlement.ToString().Substring(0, 1));

                                            if (currentLine_IsCSIndexOption && (asset_Unl_asset["MATURITYDATE"] != Convert.DBNull))
                                            {
                                                //FL/PL 20131223 
                                                //currentLine_IsExpiryDateIndexOption = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATE"]) == m_dtFile);
                                                if (asset_Unl_asset["MATURITYDATESYS"] != Convert.DBNull)
                                                {
                                                    currentLine_IsExpiryDateIndexOption = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATESYS"]) == m_dtFile);
                                                }
                                                else
                                                {
                                                    currentLine_IsExpiryDateIndexOption = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATE"]) == m_dtFile);
                                                }
                                            }
                                        }
                                        else if (dc_CATEGORY == "F") 
                                        {
                                            // FI 20170403 [23009] Add  future => Code non optimisé j'en suis conscient
                                            // développement fait ainsi pour le pas casser l'existant et parce que l'importation du fichier tpn devrait mourrir prochainement
                                            if (asset_Unl_asset["ASSETCATEGORY"].ToString() == Cst.UnderlyingAsset_ETD.Index.ToString()
                                                && (asset_Unl_asset["SETTLTMETHOD"].ToString() == SettlMethodEnum.CashSettlement.ToString().Substring(0, 1))
                                                && (asset_Unl_asset["MATURITYDATE"] != Convert.DBNull))
                                            {

                                                if (asset_Unl_asset["MATURITYDATESYS"] != Convert.DBNull)
                                                {
                                                    currentLine_IsExpiryDateIndexFuture = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATESYS"]) == m_dtFile);
                                                }
                                                else
                                                {
                                                    currentLine_IsExpiryDateIndexFuture = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATE"]) == m_dtFile);
                                                }
                                            }
                                        }
                                    }
                                    */
                                    
                                    // FI 20170504 [23009] (Nouvelle écriture)
                                    if (asset_Unl_asset["MATURITYDATE"] != Convert.DBNull)
                                    {
                                        if (asset_Unl_asset["MATURITYDATESYS"] != Convert.DBNull)
                                        {
                                            currentLine_IsExpiryDate = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATESYS"]) == m_dtFile);
                                        }
                                        else
                                        {
                                            currentLine_IsExpiryDate = (Convert.ToDateTime(asset_Unl_asset["MATURITYDATE"]) == m_dtFile);
                                        }
                                    }

                                    if ((asset_Unl_asset["ASSETCATEGORY"] != Convert.DBNull) && (asset_Unl_asset["SETTLTMETHOD"] != Convert.DBNull))
                                    {
                                        if (dc_CATEGORY == "O")
                                        {
                                            currentLine_IsCSIndexOption = (asset_Unl_asset["ASSETCATEGORY"].ToString() == Cst.UnderlyingAsset_ETD.Index.ToString())
                                                    && (asset_Unl_asset["SETTLTMETHOD"].ToString() == SettlMethodEnum.CashSettlement.ToString().Substring(0, 1));

                                            currentLine_IsExpiryDateIndexOption = currentLine_IsCSIndexOption && currentLine_IsExpiryDate;
                                        }
                                        else if (dc_CATEGORY == "F")
                                        {
                                            currentLine_IsExpiryDateIndexFuture = currentLine_IsExpiryDate;
                                        }
                                    }

                                }
                                }
                                #endregion
                            }

                            
                            if (IsNewMode)
                            {
                                /* FI 20170504 [23009] Mise en commentaire
                                // FI 20170324 [23009]si IsNewMode seuls les assets options sur indice à échéance peuvent sont conservés (pas tous les assets,  le but étant juste de récupérer le cours du ss-jacent)
                                if (dc_CATEGORY == "O")
                                    isToCopySerie &= currentLine_IsExpiryDateIndexOption;
                                else if (dc_CATEGORY == "F")
                                    isToCopySerie &= currentLine_IsExpiryDateIndexFuture; */
                                /* FI 20170504 [23009] seuls les assets à échéance sont conservés (futures ou options) */
                                isToCopySerie &= currentLine_IsExpiryDate;
                            }

                            currentLine += idasset.ToString().PadLeft(10);
                            currentLine += currentLine_SeriePlus;

                            //FL/PL 20130530 
                            #region Cas particulier des Options sur Indice, en Cash-Settlement. A l'échéance: EDSP = STRIKE +/- OPTIONPRICE
                            if (currentLine_IsCSIndexOption)
                            {
                                #region Documentation: Extrait de EUREX - DataFiles Description - Extract - FPTHED_en.pdf
                                //***
                                //* RECORD TYPE = "S" (Series Record)
                                //***
                                //05 RMTHED-SERIES-REC REDEFINES RMTHED-CLS-REC.
                                //10 REC-TYP-COD-RMTHED PIC X.
                                //10 EXER-PRC-RMTHED PIC 9(6).
                                //10 SERI-VERS-NO-RMTHED PIC 9(1).
                                //10 UND-REF-PRC-RMTHED PIC X(10).
                                //10 SERI-REF-PRC-RMTHED PIC X(10).
                                //10 SECU-TRD-UNT-NO-RMTHED PIC Z(3)9.9(4).
                                //10 SERI-STS-COD-RMTHED PIC X(1).
                                //10 VOL-RMTHED PIC Z(2)9.9(2).
                                #endregion
                                // Si échéance, on écrase la valeur UND-REF-PRC-RMTHED par EXER-PRC-RMTHED +/- SERI-REF-PRC-RMTHED
                                // Sinon, on écrase la valeur de UND-REF-PRC-RMTHED PIC par zéro et le XSL associé ne traite pas les valeurs zéro
                                isToBufferSerie = false;
                                string newValue = "0000000.00";
                                if (currentLine_IsExpiryDateIndexOption)
                                {
                                    if (StrFunc.IsEmpty(CSIndexOptionEDSP))
                                    {
                                        //Model:    X9999991XXXXXXXXXXxxxxxxxxxxZ333.4444xZ22.22
                                        //Sample:   S0010000   2817.78   1817.66   1.0000E  0.01
                                        //Position: 01234567890123456789012345678901234567890123
                                        //                    1         2         3         4

                                        // Lecture du prix (Position 19 Longueur 10) [SERI-REF-PRC-RMTHED  PIC X(10)]
                                        decimal seri_ref_prc = DecFunc.DecValue(currentLine.Substring(18, 10));
                                        if (Convert.ToInt32(seri_ref_prc * 100) <= 1)
                                        {
                                            //WARNING: Nombre de prix sont dans le fichier avec une valeur "0.01". 
                                            //         Concernant ces prix, a priori non significatif, on leur substituera le premier prix significatif trouvé sur un strike de la même échéance.
                                            isToBufferSerie = true;
                                            newValue = "{********}";
                                        }
                                        else
                                        {
                                            //PL 20130619 Manage asset_PUTCALL
                                            if (asset_PUTCALL == "0")
                                            {
                                                CSIndexOptionEDSP = StrFunc.FmtDecimalToInvariantCulture(asset_STRIKEPRICE - seri_ref_prc); //Put
                                            }
                                            else
                                            {
                                                CSIndexOptionEDSP = StrFunc.FmtDecimalToInvariantCulture(asset_STRIKEPRICE + seri_ref_prc); //Call
                                            }
                                            newValue = CSIndexOptionEDSP;

                                            if (alBufferSerie.Count > 0)
                                            {
                                                // Mise à jour des lignes bufférisées et écriture de celles-ci dans le fichier "Light" de sortie. 
                                                for (int item = 0; item < ArrFunc.Count(alBufferSerie); item++)
                                                {
                                                    alBufferSerie[item] = alBufferSerie[item].ToString().Remove(8, 10).Insert(8, newValue.PadLeft(10));
                                                    /* FI 20170504 [23009] Mise en commentaire (car toutes les series à l'échéance sont conservées)
                                                     
                                                    // FI 20170324 [23009]  si IsNewMode => écrirure d'une seule ligne (le but est juste de récupérer le prix du ss-jacent)
                                                    Boolean isWriteLine = true;
                                                    if (IsNewMode)
                                                        isWriteLine = (isSerieAdded == false);
                                                    
                                                    if (isWriteLine)
                                                    {
                                                        IOTools.StreamWriter.WriteLine(alBufferSerie[item]);
                                                        isSerieAdded = true;
                                                    }

                                                    if (IsNewMode && item == 0) //si IsNewMode on s'arrête dès la 1er ligne (le but étant de récupérer le cous du ss-jacent)
                                                        break;
                                                      */

                                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                                    //IOTools.StreamWriter.WriteLine(alBufferSerie[item]);
                                                    IOCommonTools.StreamWriter.WriteLine(alBufferSerie[item]);
                                                }
                                                alBufferSerie.Clear();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        newValue = CSIndexOptionEDSP;
                                    }
                                }
                                currentLine = currentLine.Remove(8, 10).Insert(8, newValue.PadLeft(10));

                                if (isToCopySerie && isToBufferSerie)
                                {
                                    alBufferSerie.Add(currentLine);
                                }
                            }
                            #endregion

                            #endregion

                            if (isToCopySerie && (currentLine_Expiry.Length > 0))
                            {
                                //Ecriture dans le fichier "Light" de sortie du "Expiry Record (E)" lu précédemment.
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(currentLine_Expiry);
                                IOCommonTools.StreamWriter.WriteLine(currentLine_Expiry);
                                currentLine_Expiry = string.Empty; // Reset de currentLine_Expiry  afin de ne générer qu'un seul "Expiry Record (E)"
                            }

                            isToCopyOk = isToCopySerie && (!isToBufferSerie);

                            break;
                        #endregion

                        #region Record Type => End Of File: (*EOF*)
                        case "*":
                            //Ecriture systématique de la ligne dans le fichier "Light" de sortie.
                            isToCopyOk = true;
                            break;
                        #endregion

                        #region Record Type => Array Record (A), Bucket Record (B), Volatility Record (V)
                        default:
                            //WARNING: Jamais d'écriture de ces lignes dans le fichier "Light" de sortie.
                            isToCopyOk = false;
                            break;
                        #endregion
                    }

                    if (isToCopyOk)
                    {
                        /* FI 20170504 [23009] Mise en commentaire (car toutes les series à l'échéance sont conservées en sortie)
                        // FI 20170324 [23009]  si IsNewMode => écriture d'une seule ligne (le but est juste de récupérer le prix du ss-jacent)
                        Boolean isWriteLine = true;
                        if (Record_Type == "S" && IsNewMode)
                            isWriteLine = (isSerieAdded == false);

                        if (isWriteLine)
                        {
                            //Ecriture de la ligne dans le fichier "Light" de sortie.
                            IOTools.StreamWriter.WriteLine(currentLine);
                            if (Record_Type == "S")
                                isSerieAdded = true;
                        }
                        */

                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StreamWriter.WriteLine(currentLine);
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                    }
                    #endregion
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermeture des fichiers
                CloseAllFiles();
            }
            #endregion
        }

        /// <summary>
        /// Importation (directe) des Risk Array EUREX (Method RBM)
        /// </summary>
        /// <param name="pInputParsing"></param>
        /// <param name="opNbParsingIgnoredLines"></param>
        /// EG 20140424 Test DTDISABLED sur DC / DA et ASSET => cas post CA
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        public int Input_TheoreticalPriceFile_RiskArray(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            inputParsing = pInputParsing;
            nbParsingIgnoredLines = 0;

            int lineNumber = 0;
            #region Filtrer le fichier source et le traiter

            DataTable dt_PARAMSEUREX_ARRAY = new DataTable();
            dt_PARAMSEUREX_ARRAY.Columns.Add("RTC", System.Type.GetType("System.String"));
            dt_PARAMSEUREX_ARRAY.Columns.Add("RskArrIdx", System.Type.GetType("System.Int32"));
            dt_PARAMSEUREX_ARRAY.Columns.Add("QtCmp", System.Type.GetType("System.String"));
            dt_PARAMSEUREX_ARRAY.Columns.Add("QtUnlInd", System.Type.GetType("System.String"));
            DataTable dt_PARAMSEUREX_MATURITY = null;
            DataTable dt_PARAMSEUREX_ASSETETD = null;
            DataTable dt_PARAMSEUREX_VOLATILITY = null;

            try
            {
                #region Declare

                #region Initialisation de la requête avec ses paramètres permettant de vérifier l'existance d'un DC dans dans le référentiel Spheres
                string sqlQuery_Dc_Referentiel = String.Format(@"select1 
                from dbo.DERIVATIVECONTRACT dc
                inner join dbo.MARKET mk on (mk.ISO10383_ALPHA4 = {0}) and (mk.IDM = dc.IDM)
                where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and ({1})",
                DataHelper.SQLString(EUREX_MIC), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf;

                DataParameters dataParameters_Dc_Referentiel = new DataParameters();
                dataParameters_Dc_Referentiel.Add(DataParameter.GetParameter(task.Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Dc_Referentiel.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

                #endregion

                #region Initialisation de la requête avec ses paramètres permettant de vérifier l'existance d'au moins un trade à partir du CONTRACTSYMBOL d'un DC
                string sqlQuery_Dc_Trade = String.Format(@"select 1
                from dbo.TRADE tr
                inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where ({1}) and ({2})", DataHelper.SQLString(EUREX_MIC), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf; 

                DataParameters dataParameters_Dc_Trade = new DataParameters();
                dataParameters_Dc_Trade.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Dc_Trade.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

                #endregion

                #region Asset de Category Future: Initialisation de la requête avec ses paramètres permettant de vérifier l'existance de l'Actif Futures dans le référentiel Spheres
                // Remarque : Cette requête par la même occasion nous l’identifiant de la l’actif
                string sqlQuery_Asset_Future = String.Format(@"select asset.IDASSET
                FROM dbo.TRADE tr
                inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR) and ((ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY)
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where ({1}) and ({2})", DataHelper.SQLString(EUREX_MIC), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf;

                DataParameters dataParameters_Asset_Future = new DataParameters();
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));

                #endregion

                #region Asset de Category Option: Initialisation de la requête avec ses paramètres permettant de vérifier l'existance d'au moins un trade à partir d'un Asset Option
                // Remarque : Cette requête par la même occasion nous l’identifiant de la l’actif
                string sqlQuery_Asset_Option = String.Format(@"select asset.IDASSET
                FROM dbo.TRADE tr
                inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET) and (asset.PUTCALL = @PUTCALL) and (asset.STRIKEPRICE = @STRIKEPRICE)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR) and ((ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE) and (dc.CATEGORY = @CATEGORY)
                inner join dbo.MARKET mk on (mk.IDM = dc.IDM) and (mk.ISO10383_ALPHA4 = {0})
                where ({1}) and ({2})", DataHelper.SQLString(EUREX_MIC), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "asset", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(task.Cs, "dc", "DTFILE")) + Cst.CrLf;

                DataParameters dataParameters_Asset_Option = new DataParameters();
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                dataParameters_Asset_Option.Add(new DataParameter(cs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                dataParameters_Asset_Option.Add(new DataParameter(cs, "STRIKEPRICE", DbType.Decimal));

                #endregion

                string Record_Type = string.Empty;
                string Record_Type_volatility = string.Empty;

                string dc_CONTRACTSYMBOL = string.Empty;
                string dc_CONTRACTATTRIBUTE = string.Empty;
                string dc_CATEGORY = string.Empty;
                // 20120719 MF Ticket 18030 - update PARAMSEUREX_CONTRACT is missing since the XSL exploitation has been translated in C#
                string contract_MARGIN_STYLE = string.Empty;
                string asset_PUTCALL = string.Empty;

                string maturity_MATURITYMONTHYEAR = string.Empty;

                string currentLine = string.Empty;

                string currentLine_Array = string.Empty;
                string currentLine_Product = string.Empty;
                string currentLine_Expiry = string.Empty;
                string currentLine_Serie = string.Empty;
                string currentLine_Bucket = string.Empty;
                string currentLine_volatility_Down = string.Empty;
                string currentLine_volatility_Neutral = string.Empty;
                string currentLine_volatility_Up = string.Empty;

                decimal asset_STRIKEPRICE = 0;
                decimal strikeDiviseur = 1;

                bool isToParseOk = false;
                bool isToParseDerivativeContract = false;
                bool isToParseSerieBucketVolatility = false;
                bool isExistDerivativeContract = false;

                int idasset = 0;

                #endregion

                ReadDtTheoreticalPriceFile();

                OpenInputFileName();

                Delete_TheoreticalPriceFile_RiskArray();

                #region Chargement des Datatables
                //sqlQuery = "select * from dbo.PARAMSEUREX_MATURITY where DTMARKET=@DTFILE";
                string sqlQuery = "select * from dbo.PARAMSEUREX_MATURITY where 1=2";
                sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
                sqlQuery += "select * from dbo.PARAMSEUREX_ASSETETD where 1=2";
                sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
                sqlQuery += "select * from dbo.PARAMSEUREX_VOLATILITY where 1=2";
                //DataSet ds = DataHelper.ExecuteDataset(cs, CommandType.Text, sqlQuery, dataParameters.GetArrayDbParameter());
                DataSet ds = DataHelper.ExecuteDataset(cs, CommandType.Text, sqlQuery);

                dt_PARAMSEUREX_MATURITY = ds.Tables[0];
                dt_PARAMSEUREX_MATURITY.TableName = "PARAMSEUREX_MATURITY";
                dt_PARAMSEUREX_ASSETETD = ds.Tables[1];
                dt_PARAMSEUREX_ASSETETD.TableName = "PARAMSEUREX_ASSETETD";
                dt_PARAMSEUREX_VOLATILITY = ds.Tables[2];
                dt_PARAMSEUREX_VOLATILITY.TableName = "PARAMSEUREX_VOLATILITY";

                int last_IdPARAMSEUREX_MATURITY = 0;
                #endregion

                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    if ((lineNumber % 10000) == 0)
                        System.Diagnostics.Debug.WriteLine(lineNumber);

                    //Lecture d'une ligne dans le fichier d'entrée EUREX d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //currentLine = IOTools.StreamReader.ReadLine();
                    currentLine = IOCommonTools.StreamReader.ReadLine();

                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }

                    #region Process Ligne par ligne

                    // Récupération du "Record Type" de l’enregistrement (Position 1 Longueur 1)
                    Record_Type = currentLine.Substring(0, 1);
                    switch (Record_Type)
                    {
                        #region Record Type => (Margin class Record (M) )
                        case "M":
                            //Reset du datatable ARRAY
                            dt_PARAMSEUREX_ARRAY.Rows.Clear();
                            isToParseOk = false;
                            break;
                        #endregion

                        #region Record Type => (Array Record (A) )
                        case "A":
                            currentLine_Array = currentLine;
                            isToParseOk = true;
                            break;
                        #endregion

                        #region Record Type => (Product Record (P) )
                        case "P":
                            // On récupère le Product ID (Position 2 Longueur 4) que l’on mémorise
                            dc_CONTRACTSYMBOL = currentLine.Substring(1, 4).Trim();

                            // 20120719 MF Ticket 18030 - update PARAMSEUREX_CONTRACT is missing 
                            //                         since the XSL exploitation has been translated in C#
                            contract_MARGIN_STYLE = currentLine.Substring(21, 1).Trim();

                            // On récupère le nombre de décimale du Strike (Position 23 Longueur 1) que l’on mémorise
                            strikeDiviseur = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(currentLine.Substring(22, 1))));

                            currentLine_Product = currentLine;

                            #region Vérification de l'existence du DC dans le référentiel spheres

                            dataParameters_Dc_Referentiel["DTFILE"].Value = m_dtFile;
                            dataParameters_Dc_Referentiel["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;

                            object obj_Dc_Referentiel = DataHelper.ExecuteScalar(cs, CommandType.Text, sqlQuery_Dc_Referentiel, dataParameters_Dc_Referentiel.GetArrayDbParameter());

                            // Si c’est le cas On affecte isExistDerivativeContract à 'True' sinon On On affecte isToCopyBucketVolatility à 'False'
                            isExistDerivativeContract = (obj_Dc_Referentiel != null);

                            #endregion

                            // On vérifie si on a au moins l'existance d'au moins un trade à partir du CONTRACTSYMBOL d'un DC
                            #region Vérification de l'existence du DC sur un Trade

                            dataParameters_Dc_Trade["DTFILE"].Value = m_dtFile;
                            dataParameters_Dc_Trade["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;

                            object obj_Dc_Trade = DataHelper.ExecuteScalar(cs, CommandType.Text, sqlQuery_Dc_Trade, dataParameters_Dc_Trade.GetArrayDbParameter());

                            // Si c’est le cas On affecte isToParseDerivativeContract à 'True' sinon On On affecte isToCopyBucketVolatility à 'False'
                            isToParseDerivativeContract = (obj_Dc_Trade != null);

                            #endregion

                            //// Pas d'Exploitation directe de la ligne. Celle-ci se fera ou pas dans le record de type Echeance car on doit gérer 
                            //// deux cas distinct en fonction de la categorie ( F => Future, O => Option) qui ne figure que dans Record Type => (Expiry Record (E) )
                            // 20120719 MF Ticket 18030 - Enabling straight exploitation of the P line
                            //isToParseOk = false;
                            isToParseOk = isToParseDerivativeContract;
                            break;
                        #endregion

                        #region Record Type => (Expiry Record (E) )
                        case "E":

                            // On récupère la catégorie Call/Put  (Position 2 Longueur 1) que l’on mémorise
                            switch (currentLine.Substring(1, 1).Trim())
                            {
                                case "P":
                                    asset_PUTCALL = "0";
                                    dc_CATEGORY = "O";
                                    break;

                                case "C":
                                    asset_PUTCALL = "1";
                                    dc_CATEGORY = "O";
                                    break;

                                default:
                                    asset_PUTCALL = null;
                                    dc_CATEGORY = "F";
                                    isToParseDerivativeContract = true;
                                    break;
                            }

                            // On récupère l’échéance (Position 3 Longueur 4) que l’on mémorise
                            // maturité au format 'AAMM' on rajoute le siecle avant qui est '20'
                            maturity_MATURITYMONTHYEAR = "20" + currentLine.Substring(2, 4).Trim();

                            currentLine_Expiry = currentLine;

                            //Ecriture ou pas de la ligne dans le fichier de sortie "Light"
                            isToParseOk = isToParseDerivativeContract;

                            break;

                        #endregion

                        #region Record Type => (Series Record (S) )
                        case "S":

                            // On récupère le prix d’exercice (Position 2 Longueur 6) que l’on mémorise
                            asset_STRIKEPRICE = DecFunc.DecValue(currentLine.Substring(1, 6)) / strikeDiviseur;
                            //Convert.ToDecimal(currentLine.Substring(1, 6)) / strikeDiviseur;

                            // On récupère le N° De version de la serie (Position 8 Longueur 1) que l’on mémorise
                            dc_CONTRACTATTRIBUTE = currentLine.Substring(7, 1).Trim();


                            #region Vérification de l'existence de l'ASSET

                            // Pour une Option
                            // On vérifie si on a au moins un trade existant avec cet Actif (ProductID + N° De version + échéance+ prix d’exercice + Call/Put)
                            if (dc_CATEGORY == "O")
                            {
                                if (isToParseDerivativeContract)
                                {
                                    dataParameters_Asset_Option["DTFILE"].Value = m_dtFile;
                                    dataParameters_Asset_Option["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    dataParameters_Asset_Option["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;
                                    dataParameters_Asset_Option["CATEGORY"].Value = dc_CATEGORY;
                                    dataParameters_Asset_Option["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                    dataParameters_Asset_Option["PUTCALL"].Value = asset_PUTCALL;
                                    dataParameters_Asset_Option["STRIKEPRICE"].Value = asset_STRIKEPRICE;

                                    object asset_IDASSET_Option;
                                    asset_IDASSET_Option = DataHelper.ExecuteScalar(cs, CommandType.Text, sqlQuery_Asset_Option, dataParameters_Asset_Option.GetArrayDbParameter());

                                    // Si c’est le cas On flag un booléen de type isToCopyBucketVolatility à la	valeur ‘VRAI’ 
                                    // Sinon On isToCopyBucketVolatility à la valeur ‘FALSE’
                                    if (asset_IDASSET_Option == null)
                                    {
                                        isToParseSerieBucketVolatility = false;
                                        idasset = 0;
                                    }
                                    else
                                    {
                                        isToParseSerieBucketVolatility = true;
                                        idasset = Convert.ToInt32(asset_IDASSET_Option);
                                    }
                                }
                                else
                                {
                                    isToParseSerieBucketVolatility = false;
                                    idasset = 0;
                                }
                            }
                            else
                            {
                                // Pour un Future
                                // On vérifie si l'Actif existe dans le référentiel Spheres (Actif (ProductID + N° De version + échéance ))
                                // Remarque Importante: Attention la règle est différente que celle des Options ou on vérifie si on a au moins un trade existant avec cet Actif
                                dataParameters_Asset_Future["DTFILE"].Value = m_dtFile;
                                dataParameters_Asset_Future["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                dataParameters_Asset_Future["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;
                                dataParameters_Asset_Future["CATEGORY"].Value = dc_CATEGORY;
                                dataParameters_Asset_Future["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;

                                object asset_IDASSET_Future;
                                asset_IDASSET_Future = DataHelper.ExecuteScalar(cs, CommandType.Text, sqlQuery_Asset_Future, dataParameters_Asset_Future.GetArrayDbParameter());

                                // Si c’est le cas On flag un booléen de type isToCopyBucketVolatility à la	valeur ‘VRAI’ 
                                // Sinon On isToCopyBucketVolatility à la valeur ‘FALSE’
                                // Dans le cadre des DC Futures on recopi toujours les record Bucket et Volatility si et seulement si l'actif existe
                                if (asset_IDASSET_Future == null)
                                {
                                    isToParseSerieBucketVolatility = false;
                                    idasset = 0;
                                }
                                else
                                {
                                    isToParseSerieBucketVolatility = true;
                                    idasset = Convert.ToInt32(asset_IDASSET_Future);
                                }

                            }

                            #endregion

                            // Si le Recopy_Bucket_Volatility à la valeur ‘VRAI’ on recopie ce type de record dans le fichier de sortie
                            isToParseOk = isToParseSerieBucketVolatility;

                            currentLine_Serie = currentLine;

                            break;
                        #endregion

                        #region Record Type => (Bucket Record (B) )
                        case "B":
                            // Si le Recopy_Bucket_Volatility à la valeur ‘VRAI’ on recopie ce type de record dans le fichier de sortie
                            // Test - isToParseOk = isToCopySerieBucketVolatility;

                            currentLine_Bucket = currentLine;

                            currentLine_volatility_Down = string.Empty;
                            currentLine_volatility_Neutral = string.Empty;
                            currentLine_volatility_Up = string.Empty;

                            isToParseOk = false;

                            break;
                        #endregion

                        #region Record Type => (Volatility Record (V) )
                        case "V":
                            // Si le Recopy_Bucket_Volatility à la valeur ‘VRAI’ on recopie ce type de record dans le fichier de sortie
                            // Test - isToParseOk = isToCopySerieBucketVolatility;

                            // ------  Test Optimisation ----------------------
                            Record_Type_volatility = currentLine.Substring(0, 2);
                            if (isToParseSerieBucketVolatility)
                            {
                                switch (Record_Type_volatility)
                                {
                                    case "VU":
                                        currentLine_volatility_Up = currentLine;
                                        isToParseOk = false;
                                        break;

                                    case "VN":
                                        currentLine_volatility_Neutral = currentLine;
                                        if (dc_CATEGORY == "F")
                                        {
                                            isToParseOk = isToParseSerieBucketVolatility;
                                        }
                                        else
                                        {
                                            isToParseOk = false;
                                        }
                                        break;

                                    case "VD":
                                        currentLine_volatility_Down = currentLine;
                                        if (dc_CATEGORY == "O")
                                        {
                                            isToParseOk = isToParseSerieBucketVolatility;
                                        }
                                        else
                                        {
                                            isToParseOk = false;
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                isToParseOk = false;
                            }
                            break;
                        #endregion
                    }

                    if (isToParseOk)
                    {
                        #region Exploitation des données
                        switch (Record_Type)
                        {
                            #region Record Type => (Array Record (A) )
                            case "A":
                                // Parse des records P(Product) et E(Expiry), à des fins de mise à jour de la table PARAMSEUREX_MATURITY
                                ParsePARAMSEUREX_ARRAY(currentLine_Array, dt_PARAMSEUREX_ARRAY);
                                break;
                            #endregion

                            #region Record Type => (Array Record (P) )
                            // 20120719 MF Ticket 18030
                            case "P":
                                // EG 20160404 Migration vs2013
                                //DataParameter[] parameters = new DataParameter[]
                                //    {
                                //        new DataParameter(cs, "MARGIN_STYLE", DbType.String),
                                //        new DataParameter(cs, "CONTRACTSYMBOL", DbType.String)
                                //    };

                                //Dictionary<string, object> values = new Dictionary<string, object>();
                                //values.Add("MARGIN_STYLE", contract_MARGIN_STYLE);
                                //values.Add("CONTRACTSYMBOL", dc_CONTRACTSYMBOL);

                                //int rowsupdated = DataHelper.ExecuteNonQuery(cs, CommandType.Text,
                                //    "UPDATE dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE  where CONTRACTSYMBOL = @CONTRACTSYMBOL",
                                //    DataContractHelper.GetDbDataParameters(parameters, values));
                                //                            DataParameter[] parameters = new DataParameter[]
                                //                                {
                                //                                    new DataParameter(cs, "MARGIN_STYLE", DbType.String),
                                //                                    new DataParameter(cs, "CONTRACTSYMBOL", DbType.String)
                                //                                };

                                //                            Dictionary<string, object> values = new Dictionary<string, object>();
                                //                            values.Add("MARGIN_STYLE", contract_MARGIN_STYLE);
                                //                            values.Add("CONTRACTSYMBOL", dc_CONTRACTSYMBOL);
                                //                            int rowsupdated = DataHelper.ExecuteNonQuery(cs, CommandType.Text, "UPDATE dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE  where CONTRACTSYMBOL = @CONTRACTSYMBOL",
                                //DataContractHelper.GetDbDataParameters(parameters, values));
                                DataParameters dp = new DataParameters();
                                dp.Add(new DataParameter(cs, "MARGIN_STYLE", DbType.String), contract_MARGIN_STYLE);
                                dp.Add(new DataParameter(cs, "CONTRACTSYMBOL", DbType.String), dc_CONTRACTSYMBOL);
                                string sqlUpdate = "update dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE where CONTRACTSYMBOL = @CONTRACTSYMBOL";
                                QueryParameters qryParameters = new QueryParameters(cs, sqlUpdate, dp);
                                int rowsupdated = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

                                break;
                            #endregion

                            #region Record Type => (Expiry Record (E) )
                            case "E":

                                // Déversement des Datatables dans la Database et purge
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                                dt_PARAMSEUREX_MATURITY.Clear();
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                                dt_PARAMSEUREX_ASSETETD.Clear();
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
                                dt_PARAMSEUREX_VOLATILITY.Clear();

                                if (isExistDerivativeContract)
                                {
                                    // Parse des records P(Product) et E(Expiry), à des fins de mise à jour de la table PARAMSEUREX_MATURITY
                                    last_IdPARAMSEUREX_MATURITY = ParsePARAMSEUREX_MATURITY(currentLine_Product, currentLine_Expiry, dt_PARAMSEUREX_MATURITY);
                                }
                                break;
                            #endregion

                            #region Record Type => (Series Record (S) )
                            case "S":

                                // Déversement des Datatables dans la Database et purge (Pas de purge de dt_PARAMSEUREX_MATURITY)
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                                dt_PARAMSEUREX_ASSETETD.Clear();
                                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
                                dt_PARAMSEUREX_VOLATILITY.Clear();

                                if (isExistDerivativeContract)
                                {
                                    // Parse le records S(Series), à des fins de mise à jour de la table PARAMSEUREX_ASSETETD
                                    ParsePARAMSEUREX_ASSETETD(idasset, last_IdPARAMSEUREX_MATURITY, currentLine_Serie, dt_PARAMSEUREX_ASSETETD);
                                }
                                break;
                            #endregion

                            #region Record Type => (Volatility Record (V) )
                            case "V":
                                if (isExistDerivativeContract)
                                {
                                    // Parse les records S(Series), B(Bucket), VU(Volatility UP), VN(Volatility NEUTRAL) et VD(Volatility DOWN) à des fins de mise à jour de la table PARAMSEUREX_VOLATILITY
                                    ParsePARAMSEUREX_VOLATILITY(idasset, last_IdPARAMSEUREX_MATURITY, dc_CATEGORY,
                                    currentLine_Bucket, currentLine_volatility_Up, currentLine_volatility_Neutral, currentLine_volatility_Down, dt_PARAMSEUREX_VOLATILITY,
                                    dt_PARAMSEUREX_ARRAY, asset_STRIKEPRICE);
                                }
                                break;
                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception) { throw; }
            finally
            {
                opNbParsingIgnoredLines = nbParsingIgnoredLines;
                // Fermeture du fichier
                CloseAllFiles();
                // Déversement des Datatables dans la Database
                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                DataHelper.ExecuteDataAdapter(cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
            }
            #endregion
            return lineNumber - 1;
        }

        /// <summary>
        /// Parse le records S, B, VU, VN et VD à des fins de mise à jour de la table PARAMSEUREX_VOLATILITY
        /// </summary>
        /// <param name="IdAsset"></param>
        /// <param name="IdPARAMSEUREX_MATURITY"></param>
        /// <param name="pLine_Serie"></param>
        /// <param name="pLine_Bucket"></param>
        /// <param name="pLine_Volatility_Up"></param>
        /// <param name="pLine_Volatility_Neutral"></param>
        /// <param name="pLine_Volatility_Down"></param>
        /// <param name="pDataTable"></param>
        private void ParsePARAMSEUREX_VOLATILITY(int pIdAsset, int pIdPARAMSEUREX_MATURITY, string pCategory,
            string pLine_Bucket, string pLine_Volatility_Up, string pLine_Volatility_Neutral, string pLine_Volatility_Down, DataTable pDataTable,
                    DataTable pDataTable_Array, decimal pStrike)
        {

            IOTaskDetInOutFileRow row_Bucket = null;
            IOTaskDetInOutFileRow row_Volatility_Up = null;
            IOTaskDetInOutFileRow row_Volatility_Neutral = null;
            IOTaskDetInOutFileRow row_Volatility_Down = null;

            string filter = "IDASSET=" + pIdAsset.ToString();
            filter += " and IDPARAMSEUREX_MATURITY=" + pIdPARAMSEUREX_MATURITY.ToString();

            if (pCategory == "O")
            {
                LoadLine(pLine_Bucket, ref row_Bucket);
                LoadLine(pLine_Volatility_Up, ref row_Volatility_Up);
                LoadLine(pLine_Volatility_Neutral, ref row_Volatility_Neutral);
                LoadLine(pLine_Volatility_Down, ref row_Volatility_Down);
            }
            else
            {
                LoadLine(pLine_Bucket, ref row_Bucket);
                LoadLine(pLine_Volatility_Neutral, ref row_Volatility_Neutral);
            }


            DataRow dr = pDataTable.NewRow();
            dr.BeginEdit();
            Decimal Bucket_UnlPrx = 0;

            #region row_Bucket
            foreach (IOTaskDetInOutFileRowData data in row_Bucket.data)
            {
                switch (data.name)
                {
                    case "RskArrIdx":
                        dr["RISKARRAY_INDEX"] = data.value;
                        filter += " and RISKARRAY_INDEX=" + DataHelper.SQLString(data.value);

                        #region Array
                        DataRow[] dr_Array_filtered = pDataTable_Array.Select("RskArrIdx=" + DataHelper.SQLString(data.value));
                        if (dr_Array_filtered.Length == 1)
                        {
                            dr["QUOTE_UNL_INDICATOR"] = dr_Array_filtered[0]["QtUnlInd"];
                            dr["QUOTE_ETD_UNL_COMPARE"] = dr_Array_filtered[0]["QtCmp"];
                        }
                        else
                        {
                            //TBD ERROR
                        }
                        #endregion

                        break;

                    case "UnlPrx":
                        //PL/FL 20120120 Use DecFunc.DecValue() instead of Decimal.TryParse()
                        //bool success = Decimal.TryParse(data.value, out Bucket_UnlPrx);
                        bool success = StrFunc.IsFilled(data.value);
                        if (success)
                        {
                            Bucket_UnlPrx = DecFunc.DecValue(data.value);
                        }
                        else
                        {
                            // 0 ne pas une valeur pour le prix du sj, en cas d'erreur on force la valeur 0
                            Bucket_UnlPrx = 0;
                        }
                        dr["RISKVALUE_EXEASS"] = Bucket_UnlPrx - pStrike;
                        break;

                    default:
                        break;
                }
            }
            #endregion

            #region row_Volatility_Neutral
            foreach (IOTaskDetInOutFileRowData data in row_Volatility_Neutral.data)
            {
                switch (data.name)
                {
                    case "AdjVol":
                        dr["NTRLVOLATILITY"] = DecFunc.DecValue(data.value);
                        break;

                    case "TheVal":
                        dr["NTRLTHEORETICAL_VALUE"] = DecFunc.DecValue(data.value);
                        break;

                    case "SrtOptAdj":
                        dr["NTRLSHORTOPTADJUSTMENT"] = DecFunc.DecValue(data.value);
                        break;

                    default:
                        break;
                }
            }
            #endregion

            if (pCategory == "O")
            {
                #region row_Volatility_Up
                foreach (IOTaskDetInOutFileRowData data in row_Volatility_Up.data)
                {
                    switch (data.name)
                    {
                        case "AdjVol":
                            dr["UPVOLATILITY"] = DecFunc.DecValue(data.value);
                            break;

                        case "TheVal":
                            dr["UPTHEORETICAL_VALUE"] = DecFunc.DecValue(data.value);
                            break;

                        case "SrtOptAdj":
                            dr["UPSHORTOPTADJUSTMENT"] = DecFunc.DecValue(data.value);
                            break;

                        default:
                            break;
                    }
                }
                #endregion
                #region row_Volatility_Down
                foreach (IOTaskDetInOutFileRowData data in row_Volatility_Down.data)
                {
                    switch (data.name)
                    {
                        case "AdjVol":
                            dr["DOWNVOLATILITY"] = DecFunc.DecValue(data.value);
                            break;

                        case "TheVal":
                            dr["DOWNTHEORETICAL_VALUE"] = DecFunc.DecValue(data.value);
                            break;

                        case "SrtOptAdj":
                            dr["DOWNSHORTOPTADJUSTMENT"] = DecFunc.DecValue(data.value);
                            break;

                        default:
                            break;
                    }
                }
                #endregion
            }

            dr.EndEdit();

            DataRow[] dr_filtered = pDataTable.Select(filter);
            switch (dr_filtered.Length)
            {
                case 0:
                    //Nouvelle ligne --> INS
                    dr.BeginEdit();
                    dr["IDPARAMSEUREX_MATURITY"] = pIdPARAMSEUREX_MATURITY;
                    dr["IDASSET"] = pIdAsset;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(cs);
                    dr["IDAINS"] = task.process.UserId;
                    dr.EndEdit();

                    pDataTable.Rows.Add(dr);
                    break;

                case 1:
                    //Ligne déjà existante --> UPD
                    dr["IDPARAMSEUREX_MATURITY"] = dr_filtered[0]["IDPARAMSEUREX_MATURITY"];
                    dr["IDASSET"] = dr_filtered[0]["IDASSET"];
                    dr["DTINS"] = dr_filtered[0]["DTINS"];
                    dr["IDAINS"] = dr_filtered[0]["IDAINS"];

                    object[] clone = (object[])dr.ItemArray.Clone();

                    dr_filtered[0].BeginEdit();
                    dr_filtered[0].ItemArray = clone;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSysUTC(cs);
                    dr_filtered[0]["IDAUPD"] = task.process.UserId; 
                    dr_filtered[0].EndEdit();
                    break;

                default:
                    //Error
                    //TBD
                    break;
            }
        }

        /// <summary>
        /// Parse le records S, à des fins de mise à jour de la table PARAMSEUREX_ASSETETD
        /// </summary>
        /// <param name="IdAsset"></param>
        /// <param name="IdPARAMSEUREX_MATURITY"></param>
        /// <param name="pLine_Serie"></param>
        /// <param name="pDataTable"></param>
        private void ParsePARAMSEUREX_ASSETETD(int pIdAsset, int pIdPARAMSEUREX_MATURITY, string pLine_Serie, DataTable pDataTable)
        {
            IOTaskDetInOutFileRow row_Serie = null;

            string filter = null;
            filter = " IDASSET=" + pIdAsset.ToString();
            filter += " and IDPARAMSEUREX_MATURITY=" + pIdPARAMSEUREX_MATURITY.ToString();

            LoadLine(pLine_Serie, ref row_Serie);
            DataRow dr = pDataTable.NewRow();
            dr.BeginEdit();

            #region row_Serie
            foreach (IOTaskDetInOutFileRowData data in row_Serie.data)
            {
                switch (data.name)
                {
                    case "Price":
                        dr["VALUE_QUOTE_ASSETETD"] = DecFunc.DecValue(data.value);
                        break;
                    case "UndPrice":
                        dr["VALUE_QUOTE_UNL"] = DecFunc.DecValue(data.value);
                        break;
                    case "TrdUnt":
                        dr["TRADE_UNIT"] = DecFunc.DecValue(data.value);
                        break;
                    case "Vol":
                        dr["VOLATILITY"] = DecFunc.DecValue(data.value);
                        break;
                    default:
                        break;
                }
            }


            #endregion

            dr.EndEdit();

            DataRow[] dr_filtered = pDataTable.Select(filter);
            switch (dr_filtered.Length)
            {
                case 0:
                    //Nouvelle ligne --> INS
                    dr.BeginEdit();
                    dr["IDPARAMSEUREX_MATURITY"] = pIdPARAMSEUREX_MATURITY;
                    dr["IDASSET"] = pIdAsset;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(cs);
                    dr["IDAINS"] = task.process.UserId;
                    dr.EndEdit();

                    pDataTable.Rows.Add(dr);
                    break;
                case 1:
                    //Ligne déjà existante --> UPD
                    dr["IDPARAMSEUREX_MATURITY"] = dr_filtered[0]["IDPARAMSEUREX_MATURITY"];
                    dr["IDASSET"] = dr_filtered[0]["IDASSET"];
                    dr["DTINS"] = dr_filtered[0]["DTINS"];
                    dr["IDAINS"] = dr_filtered[0]["IDAINS"];

                    object[] clone = (object[])dr.ItemArray.Clone();

                    dr_filtered[0].BeginEdit();
                    dr_filtered[0].ItemArray = clone;
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSys(cs);
                    dr_filtered[0]["IDAUPD"] = task.process.UserId;
                    dr_filtered[0].EndEdit();
                    break;
                default:
                    //Error
                    //TBD
                    break;
            }

        }

        /// <summary>
        /// Parse des records P et E, à des fins de mise à jour de la table PARAMSEUREX_MATURITY
        /// </summary>
        /// <param name="pLine_Product"></param>
        /// <param name="pLine_Expiry"></param>
        /// <param name="pDataTable"></param>
        private int ParsePARAMSEUREX_MATURITY(string pLine_Product, string pLine_Expiry, DataTable pDataTable)
        {

            int idPARAMSEUREX_MATURITY = 0;
            IOTaskDetInOutFileRow row_Product = null;
            IOTaskDetInOutFileRow row_Expiry = null;
            string filter = null;

            LoadLine(pLine_Product, ref row_Product);
            LoadLine(pLine_Expiry, ref row_Expiry);
            //if (isOk)
            //    isOk = !GetPostParsingRowStatus(row_Product);
            //if (isOk)
            //    isOk = !GetPostParsingRowStatus(row_Expiry);


            DataRow dr = pDataTable.NewRow();
            dr.BeginEdit();

            #region row_Product
            foreach (IOTaskDetInOutFileRowData data in row_Product.data)
            {
                switch (data.name)
                {
                    case "PrdCd":
                        dr["CONTRACTSYMBOL"] = data.value;
                        filter = "CONTRACTSYMBOL=" + DataHelper.SQLString(data.value);
                        break;
                    default:
                        break;
                }
            }
            #endregion
            #region row_Expiry
            foreach (IOTaskDetInOutFileRowData data in row_Expiry.data)
            {
                switch (data.name)
                {
                    case "SerExp":
                        dr["MATURITYYEARMONTH"] = "20" + data.value;
                        filter += " and MATURITYYEARMONTH=" + DataHelper.SQLString("20" + data.value);
                        break;
                    case "PrdTyp":
                        switch (data.value)
                        {
                            case "P":
                            case "C":
                                dr["PUTCALL"] = (data.value == "P" ? "0" : "1");
                                filter += " and PUTCALL=" + DataHelper.SQLString(data.value == "P" ? "0" : "1");
                                break;
                            default:
                                dr["PUTCALL"] = Convert.DBNull;
                                filter += " and PUTCALL is null";
                                break;
                        }
                        break;
                    case "ItrRat":
                        dr["THEORETICAL_INTEREST_RATE"] = DecFunc.DecValue(data.value);
                        break;
                    case "YldRat":
                        dr["THEORETICAL_YIELD_RATE"] = DecFunc.DecValue(data.value);
                        break;
                }
            }
            #endregion

            dr.EndEdit();

            DataRow[] dr_filtered = pDataTable.Select(filter);
            switch (dr_filtered.Length)
            {
                case 0:
                    //Nouvelle ligne --> INS
                    SQLUP.GetId(out idPARAMSEUREX_MATURITY, cs, SQLUP.IdGetId.PARAMSEUREX_MATURITY, SQLUP.PosRetGetId.First, 1);

                    dr.BeginEdit();
                    dr["IDPARAMSEUREX_MATURITY"] = idPARAMSEUREX_MATURITY;
                    dr["DTMARKET"] = m_dtFile;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(cs);
                    dr["IDAINS"] = task.process.UserId;
                    dr.EndEdit();

                    pDataTable.Rows.Add(dr);
                    break;
                case 1:
                    //Ligne déjà existante --> UPD
                    idPARAMSEUREX_MATURITY = Convert.ToInt32(dr_filtered[0]["IDPARAMSEUREX_MATURITY"]);
                    dr["IDPARAMSEUREX_MATURITY"] = dr_filtered[0]["IDPARAMSEUREX_MATURITY"];
                    dr["DTMARKET"] = dr_filtered[0]["DTMARKET"];
                    dr["DTINS"] = dr_filtered[0]["DTINS"];
                    dr["IDAINS"] = dr_filtered[0]["IDAINS"];

                    object[] clone = (object[])dr.ItemArray.Clone();

                    dr_filtered[0].BeginEdit();
                    dr_filtered[0].ItemArray = clone;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSysUTC(cs);
                    dr_filtered[0]["IDAUPD"] = task.process.UserId;
                    dr_filtered[0].EndEdit();
                    break;
                default:
                    //Error
                    //TBD
                    break;
            }

            return idPARAMSEUREX_MATURITY;
        }

        /// <summary>
        /// Parse des records A, à des fins de mise à jour du datatable PARAMSEUREX_ARRAY
        /// </summary>
        /// <param name="pLine_Array"></param>
        /// <param name="pDataTable"></param>
        private void ParsePARAMSEUREX_ARRAY(string pLine_Array, DataTable pDataTable)
        {

            IOTaskDetInOutFileRow row_Array = null;
            LoadLine(pLine_Array, ref row_Array);

            DataRow dr = pDataTable.NewRow();
            dr.BeginEdit();

            #region row_Array
            foreach (IOTaskDetInOutFileRowData data in row_Array.data)
                dr[data.name] = data.value;
            #endregion

            dr.EndEdit();

            pDataTable.Rows.Add(dr);

        }

        /// <summary>
        /// Supprime les données présentes en DTFILE
        /// </summary>
        /// <returns></returns>
        private int Delete_TheoreticalPriceFile_RiskArray()
        {

            DataParameter dataParameter;
            DataParameters dataParameters = new DataParameters();
            dataParameter = DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTFILE);
            dataParameter.Value = m_dtFile;
            dataParameters.Add(dataParameter);

            string sqlQuery = "delete from dbo.PARAMSEUREX_MATURITY where DTMARKET=@DTFILE";
            int nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlQuery, dataParameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Récupère la date sur la dernière ligne du fichier 
        /// </summary>
        private void ReadDtTheoreticalPriceFile()
        {
            string lastline = FileTools.GetLastLine(dataName);
            m_dtFile = new DtFunc().StringyyyyMMddToDateTime(lastline.Substring(15, 8));
        }

        #endregion
    }

    /// <summary>
    /// Permet l'importation des fichiers Eurex PRISMA
    /// </summary>
    internal class MarketDataImportEurexPrisma : MarketDataImportEurexBase
    {
        #region members
        /// <summary>
        /// Identifiant non significatif d'une date de traitement (voir table IMPRISMA_H)
        /// </summary>
        private int idIMPRISMA_H;
        // PM 20151216 [21662] Ajout IsEurosysSoftware
        protected bool IsEurosysSoftware = false; // Indique si l'import est à destination d'Eurosys Futures ou non
        // PM 20180208 [CHEETAH] Ajout pour gestion asynchrone
        private static SemaphoreSlim m_PrismaSemaphore;
        // PM 20180208 [CHEETAH] Gestion asynchrone
        private static bool m_IsAsync;
        //
        // PM 20180219 [23824] Ajout
        private PrismaDataAsset m_PrismaAssetToImport;
        #endregion members

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        public MarketDataImportEurexPrisma(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, false)
        {
            // PM 20151216 [21662] Ajout gestion IsEurosysSoftware
            if ((pTask != default(Task)) && pTask.IoTask.IsParametersSpecified && pTask.IoTask.ExistTaskParam("SOFTWARE"))
            {
                string software = pTask.IoTask.parameters["SOFTWARE"];
                IsEurosysSoftware = (software == Software.SOFTWARE_EurosysFutures);
            }
            // PM 20180208 [CHEETAH] Ajout pour gestion asynchrone
            m_IsAsync = (EFS.SpheresIO.Properties.Settings.Default.AsyncLevel > 0);
            if (m_IsAsync)
            {
                m_PrismaSemaphore = new SemaphoreSlim(EFS.SpheresIO.Properties.Settings.Default.AsyncLevel);
            }
            // PM 20180219 [23824] Ajout
            m_PrismaAssetToImport = new PrismaDataAsset();
        }
        #endregion Constructor

        #region Methods
        // PM 20150804 [21236] Ajout type delegué pour correction mauvais format de date suite à la modif pour le ticket [21224]
        private delegate DateTime StringToDateTime(string pStringDate);

        /// <summary>
        ///  Importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>

        /// <summary>
        ///  Importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// FI 20140618 [19911] Corrections diverses pour gérer les flex
        /// FI 20151123 [21576] modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputTheoreticalPriceFile(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                // PM 20180208 [CHEETAH] Liste des tâches lancées en asynchrone
                List<System.Threading.Tasks.Task> executeTask = new List<System.Threading.Tasks.Task>();

                StartPrismaImport(pInputParsing);

                PrismaInputFileConfig config = InitTheoreticalPriceFileConfig();
                // Delete dans 2 cas de figure
                // 1er cas : le fichier se termine par 0001_dddd
                // Ceci afin de gérer le cas où Eurex génère 2 fichiers  (le 1er se termine par 0001_0002 et le 2nd par 0001_0002)
                // => Il faudra importer manuellement le 2nd fichier, il n'y aura pas de delete qui supprimerait alors les infos du 1er fichier
                // 2nd cas : le fichier se termine par 9999_9999
                // Ceci afin de gérer le cas où spheres concatène les n fichiers présents (voir ticket 19081)
                bool isDelete = false;
                string extention = Path.GetExtension(dataName);
                // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                //Regex regEx = new Regex(@"0001_\d{4}" + extention + "$", RegexOptions.IgnoreCase);
                // FI 20151123 [21576] Modify
                //if ((regEx.IsMatch(dataName)) || dataName.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention)))
                Regex regEx = new Regex(@"(0001_\d{4})|(01_\d{2})" + extention + "$", RegexOptions.IgnoreCase);
                if ((regEx.IsMatch(dataName))
                    || dataName.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention))
                    || dataName.EndsWith(StrFunc.AppendFormat("99_99{0}", extention)))
                {
                    isDelete = true;
                }

                if (isDelete)
                {
                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat("Removal of existing values ..."));

                    // PM 20200102 [XXXXX] New Log
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));
                    
                    Delete_TheoreticalPriceFile();
                }

                Boolean isProductOk = false;
                Boolean isSerieOk = false;
                Boolean isProductIDInTrade = false;
                int idAsset = 0;
                ArrayList alProductOk = new ArrayList();

                StringBuilder sqlSP = new StringBuilder();
                StringBuilder sqlCE = new StringBuilder();
                StringBuilder sqlVAR = new StringBuilder();

                DataRow dataRowProduct = null;
                DataRow dataRowSerie = null;
                DataRow dataRowLGS = null;
                DataRow dataRowFxRate = null;
                DataRow dataRowLGSOfSerie = null;
                DataRow dataRowRMSOfLGS = null;
                DataRow dataRowRMSOfLGSOfSerie = null;

                IOTaskDetInOutFileRow parsingRowProduct = null;
                IOTaskDetInOutFileRow parsingRowExpiry = null;
                IOTaskDetInOutFileRow parsingRowSerie = null;
                IOTaskDetInOutFileRow parsingRowNeutralScenario = null;
                IOTaskDetInOutFileRow parsingRowLGS = null;
                IOTaskDetInOutFileRow parsingRowRMS = null;
                IOTaskDetInOutFileRow parsingRowLH = null;
                IOTaskDetInOutFileRow parsingRowFxSet = null;

                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat("Add values ..."));

                // PM 20200102 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                        {
                            // Lorsqu'il existe plusieurs fichiers chez Eurex, seul le dernier se termine par *EOF*
                            // => Ce cas permet de gérer les fichiers où il n'existe pas de *EOF*
                            // PM 20180208 [CHEETAH] Gestion asynchrone
                            if (m_IsAsync)
                            {
                                executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, sqlSP, sqlCE, sqlVAR));
                            }
                            else
                            {
                                ExecuteQueryTheoriticalPriceFile(cs, sqlSP, sqlCE, sqlVAR);
                            }
                            sqlSP = new StringBuilder();
                            sqlCE = new StringBuilder();
                            sqlVAR = new StringBuilder(); 
                            break;
                        }

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "P"://Product
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté
                                // PM 20180208 [CHEETAH] Gestion asynchrone
                                if (m_IsAsync)
                                {
                                    executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, sqlSP, sqlCE, sqlVAR));
                                }
                                else
                                {
                                    ExecuteQueryTheoriticalPriceFile(cs, sqlSP, sqlCE, sqlVAR);
                                }
                                sqlSP = new StringBuilder();
                                sqlCE = new StringBuilder();
                                sqlVAR = new StringBuilder();

                                parsingRowProduct = null;
                                dataRowProduct = null;
                                LoadLine(currentLine, ref parsingRowProduct);

                                string productID = parsingRowProduct.GetRowDataValue(cs, "PRODUCT ID");
                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);
                                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                        StrFunc.AppendFormat("Add parameters values for product:{0}", productID));

                                    // PM 20200102 [XXXXX] New Log
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for product:{0}", productID), 3));

                                    if (config.isSearchProduct)
                                    {
                                        isProductIDInTrade = IsExistDcInTrade(productID);
                                    }
                                }
                                break;

                            case "E"://Expiry
                                parsingRowExpiry = null;
                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Toujours parser les Expiry
                                //if (isProductOk)
                                //{
                                    LoadLine(currentLine, ref parsingRowExpiry);
                                //}
                                break;

                            case "S"://Serie
                                parsingRowSerie = null;
                                isSerieOk = false;
                                idAsset = 0;

                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Vérifier le Product Flex
                                LoadLine(currentLine, ref parsingRowSerie);
                                if (false == isProductOk)
                                {
                                    Boolean isFlexible = BoolFunc.IsTrue(parsingRowSerie.GetRowDataValue(cs, "Flex Series Flag"));
                                    if (isFlexible)
                                    {
                                        string flexProductID = parsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
                                        isProductOk = IsProductToImport(config, flexProductID, alProductOk);
                                        if (isProductOk)
                                        {
                                            if (config.isSearchProduct)
                                            {
                                                // Vérifier qu'il existe des trade sur le contrat Flex 
                                                isProductOk = IsExistDcInTrade(flexProductID);
                                            }
                                            if (isProductOk)
                                            {
                                                alProductOk.Add(flexProductID);
                                                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                                    StrFunc.AppendFormat("Add parameters values for flexible product:{0}", flexProductID));

                                                // PM 20200102 [XXXXX] New Log
                                                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for flexible product:{0}", flexProductID), 3));
                                            }
                                        }
                                    }
                                }

                                if (isProductOk)
                                {
                                    // PM 20150804 [21236] Pb d'un Flex sans trade sur le contrat standard
                                    //LoadLine(currentLine, ref parsingRowSerie);

                                    if (config.isSearchProduct)
                                    {
                                        Boolean isStandard = BoolFunc.IsFalse(parsingRowSerie.GetRowDataValue(cs, "Flex Series Flag"));
                                        if (isStandard)
                                        {
                                            // pour une série sur un produit standard, Spheres® vérifie que la série a été négociée uniquement si le contrat a déjà été négocié
                                            isProductOk = isProductIDInTrade;
                                        }
                                    }
                                    if (isProductOk)
                                    {
                                        // PM 20180219 [23824] Modification de IsSerieToImport 
                                        //isSerieOk = IsSerieToImport(config, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);
                                        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                        //isSerieOk = IsSerieToImport(config, false, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);
                                        MarketDataAssetToImport assetData;
                                        isSerieOk = IsSerieToImport(config, false, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out assetData);
                                        idAsset = assetData.IdAsset;
                                    }
                                }
                                break;
                            case "N": //NEUTRAL SCENARIO
                                // Alimentation de la table IMPRISMA_P,IMPRISMA_E,IMPRISA_S

                                parsingRowNeutralScenario = null;
                                dataRowSerie = null;
                                dataRowLGS = null; // Release 2 : les lignes LGS arrivent après la Ligne N, cette initialisation est donc ok 
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowNeutralScenario);

                                    dataRowProduct = this.LoadRowProduct(parsingRowProduct);
                                    int idProduct = Convert.ToInt32(dataRowProduct["IDIMPRISMAP_H"]);
                                    string productId = dataRowProduct["PRODUCTID"].ToString();

                                    DataRow rowExpiry = this.LoadRowProductExpiration(idProduct, productId, parsingRowExpiry);
                                    int idExpiry = Convert.ToInt32(rowExpiry["IDIMPRISMAE_H"]);

                                    dataRowSerie = LoadRowSerie(idExpiry, idAsset, parsingRowSerie, parsingRowNeutralScenario);

                                    //Release 1, une serie est associée à un et un seul Liquidation Group Split.
                                    //Ici Spheres® ajoute le Liquidation Group Split s'il n'existe pas
                                    string liquidationGroupSplit = parsingRowSerie.GetRowDataValue(cs, "Liquidation Group Split");
                                    if (StrFunc.IsFilled(liquidationGroupSplit))
                                    {
                                        int idLiquidationGroup = Convert.ToInt32(dataRowProduct["IDIMPRISMALG_H"]);
                                        dataRowLGS = LoadRowLiquidationGroupSplit(idLiquidationGroup, liquidationGroupSplit, null);
                                    }
                                }
                                break;
                            case "LGS": //LGS existe uniquement en release 2
                                parsingRowLGS = null;
                                dataRowLGS = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowLGS);
                                    int idLiquidationGroup = Convert.ToInt32(dataRowProduct["IDIMPRISMALG_H"]);
                                    dataRowLGS = LoadRowLiquidationGroupSplit(idLiquidationGroup, parsingRowLGS, null);
                                }
                                break;
                            case "RMS":
                                parsingRowRMS = null;
                                dataRowRMSOfLGS = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowRMS);
                                    DataRow rowRMS = LoadRowRiskMeasuresSet(parsingRowRMS);

                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLGS["IDIMPRISMALGS_H"]);
                                    int idRiskMeasureSet = Convert.ToInt32(rowRMS["IDIMPRISMARMS_H"]);
                                    dataRowRMSOfLGS = LoadRiskMeasuresSetOfLiquidationGroupSplit(idLiquidationGroupSplit, idRiskMeasureSet, parsingRowRMS);
                                }
                                break;
                            case "LH": //LIQUIDATION HORIZON 
                                parsingRowLH = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowLH);
                                }
                                break;
                            case "FX": //FX SET
                                parsingRowFxSet = null;
                                dataRowFxRate = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowFxSet);
                                    dataRowFxRate = LoadRowFX(parsingRowFxSet);
                                }
                                break;
                            case "SP": //SCENARIO PRICES
                                dataRowLGSOfSerie = null;
                                dataRowRMSOfLGSOfSerie = null;
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsingRowScenariosPrices = null;
                                    LoadLine(currentLine, ref parsingRowScenariosPrices);

                                    //Liquidation Group Split of serie
                                    int idSerie = Convert.ToInt32(dataRowSerie["IDIMPRISMAS_H"]);
                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLGS["IDIMPRISMALGS_H"]);
                                    dataRowLGSOfSerie = LoadRowLiquidationGroupSplitOfSerie(idSerie, idLiquidationGroupSplit, parsingRowLH, parsingRowLGS);

                                    //RSM of LGS of Serie
                                    int idRowRMSOfLGS = Convert.ToInt32(dataRowRMSOfLGS["IDIMPRISMARMSLGS_H"]);
                                    int IdFxRate = Convert.ToInt32(dataRowFxRate["IDIMPRISMAFX_H"]);
                                    // PM 20161019 [22174] Prisma 5.0 : Ajout pParsingRowLH
                                    //dataRowRMSOfLGSOfSerie = LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(idSerie, idRowRMSOfLGS, IdFxRate);
                                    dataRowRMSOfLGSOfSerie = LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(idSerie, idRowRMSOfLGS, IdFxRate, parsingRowLH);

                                    //Senarios Prices
                                    // PM 20161019 [22174] Prisma 5.0 : Ne plus prendre le liquidationHorizon dans "dataRowLGSOfSerie" mais dans "dataRowRMSOfLGSOfSerie"
                                    //int liquidationHorizon = Convert.ToInt32(dataRowLGSOfSerie["LH"]);
                                    int liquidationHorizon = Convert.ToInt32(dataRowRMSOfLGSOfSerie["LH"]);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    // PM 20161019 [22174] Prisma 5.0 : Ajout de Neutral Price pour les valeurs de SP qui ne sont pas renseignées
                                    decimal neutralPrice = Convert.ToDecimal(dataRowSerie["NPRICE"]);

                                    if (sqlSP.Length > 0)
                                    {
                                        sqlSP.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    }
                                    // PM 20161019 [22174] Prisma 5.0 : Ajout de neutralPrice
                                    //sqlSP.Append(BuildSelectSPValues2(cs, idRMSOfLGSOfSerie, parsingRowScenariosPrices, liquidationHorizon));
                                    sqlSP.Append(BuildSelectSPValues2(cs, idRMSOfLGSOfSerie, parsingRowScenariosPrices, liquidationHorizon, neutralPrice));

                                    if (sqlSP.Length >= config.sqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, sqlSP, null, null));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(cs, sqlSP, null, null);
                                        }
                                        sqlSP = new StringBuilder();
                                    }
                                }
                                break;
                            case "CE": //COMPRESSION ERROR
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsiongRowCompressionError = null;
                                    LoadLine(currentLine, ref parsiongRowCompressionError);

                                    int liquidationHorizon = Convert.ToInt32(dataRowLGSOfSerie["LH"]);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    if (sqlCE.Length > 0)
                                        sqlCE.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    sqlCE.Append(BuildSelectCEValues(cs, idRMSOfLGSOfSerie, parsiongRowCompressionError, liquidationHorizon));

                                    if (sqlCE.Length >= config.sqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, null, sqlCE, null));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(cs, null, sqlCE, null);
                                        }
                                        sqlCE = new StringBuilder();
                                    }
                                }
                                break;
                            case "IVAR": //INSTRUMENT VAR
                            case "AIVAR": //ADDITIONAL INSTRUMENT VAR
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsiongRowVar = null;
                                    LoadLine(currentLine, ref parsiongRowVar);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    if (sqlVAR.Length > 0)
                                        sqlVAR.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    sqlVAR.Append(BuildSelectVaRValues(cs, idRMSOfLGSOfSerie,  parsiongRowVar));

                                    if (sqlVAR.Length >= config.sqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, null, null, sqlVAR));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(cs, null, null, sqlVAR);
                                        }
                                        sqlVAR = new StringBuilder();
                                    }
                                }
                                break;
                            case "*EOF*":
                                if (m_IsAsync)
                                {
                                    // PM 20180208 [CHEETAH] Gestion asynchrone
                                    executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(cs, sqlSP, sqlCE, sqlVAR));
                                }
                                else
                                {
                                    ExecuteQueryTheoriticalPriceFile(cs, sqlSP, sqlCE, sqlVAR);
                                }
                                //PM 20140612 [19911] Ajout mise à null des variables de données SQL à insérer pour ne pas tenter des les insérer de nouveau lorsque currentline vaut null
                                sqlSP = null;
                                sqlCE = null;
                                sqlVAR = null; 
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                // PM 20180208 [CHEETAH] Gestion asynchrone : attente fin des tâches
                if (m_IsAsync)
                {
                    try
                    {
                        System.Threading.Tasks.Task.WaitAll(executeTask.Where(t => t != default(System.Threading.Tasks.Task)).ToArray());
                        executeTask = default(List<System.Threading.Tasks.Task>);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                }

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier Risk Measure Configuration ou Importation du fichier Risk Measure Aggregation
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        public int InputRiskMeasure(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);

                DataRow dataRowLiquidationGroup = null;
                DataRow dataRowLiquidationGroupSplit = null;

                IOTaskDetInOutFileRow parsiongRowLiquidationGroup = null;
                IOTaskDetInOutFileRow parsiongRowLiquidationGroupSplit = null;

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                            break;

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "LG"://Liquidation Group
                                dataRowLiquidationGroup = null;
                                parsiongRowLiquidationGroup = null;
                                LoadLine(currentLine, ref parsiongRowLiquidationGroup);
                                dataRowLiquidationGroup = LoadRowLiquidationGroup(parsiongRowLiquidationGroup);
                                break;

                            case "LGS": //LIQUIDATION GROUP SPLIT (présent uniquement en release 2)
                                parsiongRowLiquidationGroupSplit = null;
                                LoadLine(currentLine, ref parsiongRowLiquidationGroupSplit);
                                dataRowLiquidationGroupSplit = LoadRowLiquidationGroupSplit(
                                                    Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]), parsiongRowLiquidationGroupSplit, null);
                                break;

                            case "RM"://RISK MEASURE (présent uniquement sur le fichier Risk Measure Aggregation)
                                IOTaskDetInOutFileRow parsingRowRiskMeasures = null;
                                LoadLine(currentLine, ref parsingRowRiskMeasures);
                                if (parsiongRowLiquidationGroupSplit == null) // se produit sur un fichier Release 1 (pas de record LGS)
                                {
                                    //Release 1, les RM sont associés au Liquidation Group 
                                    //Release 2, les RM sont associés au Liquidation Group Split
                                    //Release 1,
                                    //Il est nécessaire de connaitre au préalable les Liquidation Group Split déclarés ds Theoretical prices and instrument uniquement sur les record de type S
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);
                                    //FI 20140225 [17861] Utilisation de la méthode GetLGSOfLG                                    
                                    //string identifierLiquidationGroup = dataRowLiquidationGroup["IDENTIFIER"].ToString() ;
                                    //Il peut ne pas y avoir de Liquidation Group Split lorsque Spheres intègre partiellement le fichier Theoretical prices and instrument (cas par défaut)
                                    //List<Pair<Int32, string>> liquidationGroupSplit = CheckLGSOfLG(idLiquidationGroup, identifierLiquidationGroup);
                                    List<Pair<Int32, string>> liquidationGroupSplit = GetLGSOfLG(cs, idLiquidationGroup); 
                                    for (int i = 0; i < ArrFunc.Count(liquidationGroupSplit); i++)
                                        LoadRowLiquidationGroupSplit(idLiquidationGroup, liquidationGroupSplit[i].Second, parsingRowRiskMeasures);
                                }
                                else
                                {
                                    //Release 2
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);
                                    LoadRowLiquidationGroupSplit(idLiquidationGroup, parsiongRowLiquidationGroupSplit, parsingRowRiskMeasures);
                                }
                                break;

                            case "RMS": //RISK MEASURE SET
                                IOTaskDetInOutFileRow parsingRowRiskMeasureSet = null;
                                LoadLine(currentLine, ref parsingRowRiskMeasureSet);

                                DataRow rowRMS = LoadRowRiskMeasuresSet(parsingRowRiskMeasureSet);
                                int idRiskMeasureSet = Convert.ToInt32(rowRMS["IDIMPRISMARMS_H"]);
                                if (parsiongRowLiquidationGroupSplit == null) // se produit sur un fichier Release 1 (pas de record LGS)
                                {
                                    //Release 1, les RMS sont associés au Liquidation Group 
                                    //Release 2, les RMS sont associés au Liquidation Group Split
                                    //
                                    //Release 1,
                                    //Il est nécessaire de connaitre au préalable les Liquidation Group Split déclarés ds Theoretical prices and instrument uniquement sur les record de type S
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);
                                    
                                    //FI 20140225 [17861] Utilisation de la méthode GetLGSOfLG
                                    //Il peut ne pas y avoir de Liquidation Group Split lorsque Spheres intègre partiellement le fichier Theoretical prices and instrument (cas par défaut)
                                    //string identifierLiquidationGroup = dataRowLiquidationGroup["IDENTIFIER"].ToString();
                                    //List<Pair<Int32, string>> liquidationGroupSplit = CheckLGSOfLG(idLiquidationGroup, identifierLiquidationGroup);
                                    List<Pair<Int32, string>> liquidationGroupSplit = GetLGSOfLG(cs, idLiquidationGroup); 
                                    for (int i = 0; i < ArrFunc.Count(liquidationGroupSplit); i++)
                                        LoadRiskMeasuresSetOfLiquidationGroupSplit(liquidationGroupSplit[i].First, idRiskMeasureSet, parsingRowRiskMeasureSet);
                                }
                                else
                                {
                                    //Release 2
                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLiquidationGroupSplit["IDIMPRISMALGS_H"]);
                                    LoadRiskMeasuresSetOfLiquidationGroupSplit(idLiquidationGroupSplit, idRiskMeasureSet, parsingRowRiskMeasureSet);
                                }
                                break;

                            case "*EOF*":
                                break;

                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport(); 
            }
        }
        
        /// <summary>
        ///  Importation du fichier Liquidity Factors Configuration
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputLiquidityFactors(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);

                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat("Removal of existing values ..."));

                // PM 20200102 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_LiquidityFactorsFile();

                StringBuilder sql = new StringBuilder();

                string lastLiquidityClass = string.Empty;
                DataRow dataRowLiquidityClass = null;

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                            break;

                        #region Process Ligne par ligne
                        string value = GetFirstElement(currentLine);
                        if (value != "*EOF*")
                        {
                            IOTaskDetInOutFileRow parsingRow = null;
                            this.LoadLine(currentLine, ref  parsingRow);

                            string liquidityClass = parsingRow.GetRowDataValue(cs, "Liquidity Class");
                            if (lastLiquidityClass != liquidityClass)
                            {
                                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                    StrFunc.AppendFormat("Add values for liquidity class:{0}", liquidityClass));

                                // PM 20200102 [XXXXX] New Log
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add values for liquidity class:{0}", liquidityClass), 3));

                                dataRowLiquidityClass = LoadRowLiquidityClass(liquidityClass);
                                lastLiquidityClass = liquidityClass;
                            }

                            int idLiquidityClass = Convert.ToInt32(dataRowLiquidityClass["IDIMPRISMALIQCLASS_H"]);

                            if (sql.Length > 0)
                                sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                            sql.Append(BuildSelectLiquidityFactors(cs, idLiquidityClass, parsingRow));
                        }
                        else
                        {
                            ExecuteQueryLiquidityFactors(cs, sql);
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier Market Capacities Configuration
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// FI 20140617 [19911] utilisation de la variable isElementTheoInstExecuted pour s'appuyer sur l'importation du fichier TheoInst
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputMarketCapacities(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitMarketCapacitiesFileConfig();

                // PM 20170524 [22834][23078] Recherche uniquement de "RISKDATA - TheoInst OI-serie"
                //Boolean isElementTheoInstExecuted = task.ExistElement("EUREX - RISKDATA - TheoInst OI-serie");
                Boolean isElementTheoInstExecuted = task.ExistElementContains("RISKDATA - TheoInst OI-serie");

                // Récupération de la liste des assets importés via l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
                List<String> lstProductId = new List<String>();
                if (isElementTheoInstExecuted)
                    lstProductId = LoadProductIdfromPrismaProduct();

                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat("Removal of existing values ..."));

                // PM 20200102 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_MarketCapacitiesFile();

                string lastProductId = string.Empty;
                Boolean isProductOk = false;
                ArrayList alProductOk = new ArrayList();
                StringBuilder sql = new StringBuilder();

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                            break;

                        #region Process Ligne par ligne
                        string value = GetFirstElement(currentLine);
                        if (value != "*EOF*")
                        {
                            IOTaskDetInOutFileRow parsingRow = null;
                            this.LoadLine(currentLine, ref  parsingRow);

                            // Insertion des données à chaque nouveau produit
                            string productID = parsingRow.GetRowDataValue(cs, "Product ID");
                            if ((productID != lastProductId))
                            {
                                lastProductId = productID;

                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isElementTheoInstExecuted && isProductOk)
                                    isProductOk = lstProductId.Contains(productID);
                                
                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);
                                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                        StrFunc.AppendFormat("Add values for product:{0}", productID));

                                    // PM 20200102 [XXXXX] New Log
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add values for product:{0}", productID), 3));
                                }
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté 
                                if (sql.Length > 0)
                                {
                                    ExecuteQueryMarketCapacities(cs, sql);
                                    sql = new StringBuilder();
                                }
                            }
                            if (isProductOk)
                            {
                                if (sql.Length > 0)
                                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                                sql.Append(BuildSelectMarketCapacities(cs, idIMPRISMA_H, parsingRow));
                                if (sql.Length >= config.sqlLimitSize)
                                {
                                    ExecuteQueryMarketCapacities(cs, sql);
                                    sql = new StringBuilder();
                                }
                            }
                        }
                        else
                        {
                            if (sql.Length > 0)
                            {
                                ExecuteQueryMarketCapacities(cs, sql);
                            }
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier foreign Exchange rates
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputFxExchangeRates(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                // PM 20180208 [CHEETAH] Liste des tâches lancées en asynchrone
                List<System.Threading.Tasks.Task> executeTask = new List<System.Threading.Tasks.Task>();

                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitFxExchangeRateFileConfig();

                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat("Removal of existing values ..."));

                // PM 20200102 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_FxExchangeRatesFile();

                IOTaskDetInOutFileRow parsingRowFxSet = null;
                IOTaskDetInOutFileRow parsingRowCurrencyPair = null;

                DataRow dataRowFxSet = null;
                DataRow dataRowFxPair = null;
                StringBuilder sql = new StringBuilder();

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                            break;

                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "FX": //FX SET
                                parsingRowFxSet = null;
                                dataRowFxSet = null;
                                LoadLine(currentLine, ref parsingRowFxSet);
                                dataRowFxSet = LoadRowFX(parsingRowFxSet);
                                break;

                            case "P"://Currency Pair
                                parsingRowCurrencyPair = null;
                                LoadLine(currentLine, ref parsingRowCurrencyPair);
                                break;

                            case "C"://Currency Exchange Rate
                                IOTaskDetInOutFileRow parsingRowCurrentExchangeRate = null;
                                LoadLine(currentLine, ref parsingRowCurrentExchangeRate);

                                int idPrismaFX = Convert.ToInt32(dataRowFxSet["IDIMPRISMAFX_H"]);
                                string fxSetIdentifier = Convert.ToString(dataRowFxSet["IDENTIFIER"]);
                                dataRowFxPair = LoadRowFxPair(idPrismaFX, fxSetIdentifier, parsingRowCurrencyPair, parsingRowCurrentExchangeRate);
                                break;

                            case "RMS"://Risk Measure Set and Exchange Rate Scenarios
                                IOTaskDetInOutFileRow parsingRowCurrentRiskMeasureSet = null;
                                LoadLine(currentLine, ref parsingRowCurrentRiskMeasureSet);

                                string RMSIdentifier = parsingRowCurrentRiskMeasureSet.GetRowDataValue(cs, "Risk Measure Set");
                                DataRow dataRowRMS = LoadRowRiskMeasuresSet(parsingRowCurrentRiskMeasureSet);
                                int idRMS = Convert.ToInt32(dataRowRMS["IDIMPRISMARMS_H"]);
                                int idFxPair = Convert.ToInt32(dataRowFxPair["IDIMPRISMAFXPAIR_H"]);

                                if (sql.Length > 0)
                                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                                sql.Append(BuildSelectFXRatesValues(cs, idFxPair, idRMS, parsingRowCurrentRiskMeasureSet));
                                if (sql.Length >= config.sqlLimitSize)
                                {
                                    if (m_IsAsync)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        executeTask.Add(ExecuteQueryFxRateAsync(cs, sql));
                                    }
                                    else
                                    {
                                        ExecuteQueryFxRate(cs, sql);
                                    }
                                    sql = new StringBuilder();
                                }
                                break;

                            case "*EOF*":
                                if (m_IsAsync)
                                {
                                    // PM 20180208 [CHEETAH] Gestion asynchrone
                                    executeTask.Add(ExecuteQueryFxRateAsync(cs, sql));
                                }
                                else
                                {
                                    ExecuteQueryFxRate(cs, sql);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                // PM 20180208 [CHEETAH] Gestion asynchrone : attente fin des tâches
                if (m_IsAsync)
                {
                    try
                    {
                        System.Threading.Tasks.Task.WaitAll(executeTask.Where(t => t != default(System.Threading.Tasks.Task)).ToArray());
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                }

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier settlement prices
        ///  <para>alimentation de la table IMPRISMAS_H et alimentation des tables de cotations</para>
        ///  <para>cette importation doit se dérouler nécessairement après l'importation du fichier Theoretical prices and instrument</para>
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// FI 20140617 [19911] add method 
        /// En vue d'une prochaine évolution cette méthode gère le mode isImportPriceOnly cependant pour l'instant si isImportPriceOnly=true spheres® n'appelle pas cette méthode
        /// FI 20170306 [22225] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputSettlementPriceFile(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitSettlementPriceFileConfig();
                //L'élément "EUREX - RISKDATA - TheoInst OI-serie" pourrait éventuellement ne pas exister si un jour Spheres® est utilisé uniquement pour récupérer les cotations
                //Si l'élément existe, Spheres® considère qu'il est nécessairemenent exécuté avant l'importation du fichier settlement prices
                // PM 20170524 [22834][23078] Recherche uniquement de "RISKDATA - TheoInst OI-serie"
                //Boolean isElementTheoInstExecuted = task.ExistElement("EUREX - RISKDATA - TheoInst OI-serie");
                Boolean isElementTheoInstExecuted = task.ExistElementContains("RISKDATA - TheoInst OI-serie");

                if (false == isImportPriceOnly)
                {
                    // l'élément d'importation du fichier TheoInst doit déjà être exécutée (puisque cette fonction effectue des update de la table IMPRISMAS_H)
                    if (false == isElementTheoInstExecuted)
                    {
                        // PM 20170524 [22834][23078] Modif du message
                        //throw new InvalidOperationException(StrFunc.AppendFormat("the execution of element(id:{0])is required", "EUREX - RISKDATA - TheoInst OI-serie"));
                        throw new InvalidOperationException("The execution of element: \"* - RISKDATA - TheoInst OI-serie\" is required");
                    }
                }

                // Récupération de la liste des assets importés via l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
                List<int> lstIdAsset = new List<int>();
                List<String> lstProductId = new List<String>();
                // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                //if (isElementTheoInstExecuted && (false == isImportPriceOnly))
                //{
                //    lstIdAsset = LoadSpheresAssetFromPrismaSerie();
                //    lstProductId = LoadProductIdfromPrismaProduct();
                //}
                if (isImportPriceOnly)
                {
                    m_PrismaAssetToImport.LoadAsset(cs, m_dtFile);
                    lstProductId = m_PrismaAssetToImport.GetListOfContractSymbol();
                    lstIdAsset = m_PrismaAssetToImport.GetListOfIdAsset();
                }
                else if (isElementTheoInstExecuted)
                {
                    lstIdAsset = LoadSpheresAssetFromPrismaSerie();
                    lstProductId = LoadProductIdfromPrismaProduct();
                }

                Boolean isProductOk = false;
                Boolean isExpiryOk = false;
                Boolean isProductFlexOk = false; // PM 20151214 [21643] Add isProductFlexOk
                ArrayList alProductOk = new ArrayList();
                Boolean isProductIDInTrade = false;

                StringBuilder sqlUpd = new StringBuilder();

                IOTaskDetInOutFileRow parsingRowProduct = null;
                IOTaskDetInOutFileRow parsingRowExpiry = null;

                // PM 20151124 [20124] Initialise les membres m_IdMarketEnv & m_IdValScenario
                InitDefaultMembers();

                // Lecture de l'IDM du marché pour l'import des cotations
                // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
                //int idm = GetIdmFromExchangeAcronym(EUREX_EXCHANGEACRONYM);
                int idm = GetIdmFromExchangeAcronym(ExchangeAcronym);
                string currency = string.Empty;
                decimal productTickSize = 0;
                decimal productTickValue = 0;
                string productID = null;
                List<Pair<string, int>> updatedDC = new List<Pair<string, int>>();
                bool isNewExpiry = false;
                //
                //FI 20181126 [24308] Alimentation du paramètre pQuoteSide
                QueryParameters qryParamQuoteETD = GetQueryParameters_QUOTE_XXX_H(cs, Cst.OTCml_TBL.QUOTE_ETD_H, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialClose);
                //
                // PM 20151209 [21653] Lecture des DC sans Contract Multiplier
                QueryParameters queryDCMutliplier = GetQuerySelectDERIVATIVECONTRACT(task.Cs, idm, m_dtFile);
                DataTable dtDC = DataHelper.ExecuteDataTable(task.Cs, queryDCMutliplier.query.ToString(), queryDCMutliplier.parameters.GetArrayDbParameter());
                List<DataRow> dataRowDCWithoutCM = dtDC.AsEnumerable().ToList(); // 
                List<Pair<string, string>> dcWithoutCM = dataRowDCWithoutCM.Select(dr => new Pair<string, string>(dr.Field<string>("CONTRACTSYMBOL"), dr.Field<string>("CONTRACTTYPE"))).ToList();
                bool isDCToUpdated = false;
                bool isDCFlexToUpdated = false;
                bool isCheckForFlex = dcWithoutCM.Any(dc => dc.Second == DerivativeContractTypeEnum.FLEX.ToString()); // Existe-t-il des Flex sans Contract Multiplier
                //
                // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                bool isExpiryDate = false;
                bool isUnlPriceForIndexOptionToDo = true;
                bool isUnlPriceForIndexOptionDone = false;
                MarketDataAssetToImport assetDataForIndexOption = default(MarketDataAssetToImport);
                //
                //task.process.ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.EXPANDED, 3, StrFunc.AppendFormat("Add values ..."));
                task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3),
                                        StrFunc.AppendFormat("Add values ..."));

                // PM 20200102 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Add values ...", 3));

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                        {
                            // Lorsqu'il existe plusieurs fichiers chez Eurex, seul le dernier se termine par *EOF*
                            // => Ce cas permet de gérer les fichiers où il n'existe pas de *EOF*
                            if ((null != sqlUpd) && sqlUpd.Length > 0)
                            {
                                DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpd.ToString());
                                sqlUpd = new StringBuilder();
                            }
                            break;
                        }

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            #region Product
                            case "P"://Product
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté 
                                if ((null != sqlUpd) && sqlUpd.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpd.ToString());
                                    sqlUpd = new StringBuilder();
                                }
                                parsingRowProduct = null;

                                LoadLine(currentLine, ref parsingRowProduct);

                                // PM 20151124 [20124] Devise pour l'import des cotations
                                currency = parsingRowProduct.GetRowDataValue(cs, "Currency");
                                // PM 20151124 [20124] Tick Size et Tick Value pour l'import du Contract Multiplier
                                productTickSize = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(cs, "Tick Size"));
                                productTickValue = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(cs, "Tick Value"));

                                // PM 20151124 [20124] Garder le productID pour l'utiliser au niveau de la série
                                //string productID = parsingRowProduct.GetRowDataValue(cs, "PRODUCT ID");
                                productID = parsingRowProduct.GetRowDataValue(cs, "PRODUCT ID");

                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);
                                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                        StrFunc.AppendFormat("Add parameters values for product:{0}", productID));

                                    // PM 20200102 [XXXXX] New Log
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for product:{0}", productID), 3));

                                    // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                                    //if (isImportPriceOnly)
                                    //{
                                    //    isProductIDInTrade = IsExistDcInTrade(productID);
                                    //}
                                    //else
                                    //{
                                    //    isProductOk = lstProductId.Contains(productID);
                                    //}
                                        isProductOk = lstProductId.Contains(productID);
                                    }
                                // PM 20151209 [21653] Si le DC ne doit pas est importé, si son CM est absent alors le mettre à jour quand même
                                isDCToUpdated = false;
                                isDCFlexToUpdated = false;
                                if ((isProductOk == false) && (isImportPriceOnly == false))
                                {
                                    isDCToUpdated = dcWithoutCM.Any(dc => dc.First == productID);
                                }
                                break;
                            #endregion Product

                            #region Expiry
                            case "E"://Expiry
                                parsingRowExpiry = null;
                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                isExpiryOk = false;
                                // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                isExpiryDate = false;
                                isUnlPriceForIndexOptionToDo = true;
                                isUnlPriceForIndexOptionDone = false;
                                assetDataForIndexOption = default(MarketDataAssetToImport);
                                //
                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                //if (isProductOk)
                                if (isProductOk || isImportPriceOnly)
                                {
                                    // RD 20150404 [23043] Ici, on est dans le cas où le DC avec des échéances dans le fichier Theo
                                    isExpiryOk = true;
                                    LoadLine(currentLine, ref parsingRowExpiry);
                                    // PM 20151124 [20124] Nouvelle maturity dont importer le prix du sous-jacent
                                    isNewExpiry = true;
                                }
                                else 
                                {
                                    // RD 20150404 [23043] Ici, on est dans le cas où le DC sans échéances dans le fichier Theo, il faut donc vérifier si Maturity avec date d'échéance à la date du fichier
                                    bool tmpIsProductOk = IsProductToImport(config, productID, alProductOk);
                                    IOTaskDetInOutFileRow tmpParsingRowExpiry = null;

                                    if (tmpIsProductOk)
                                    {
                                        LoadLine(currentLine, ref tmpParsingRowExpiry);

                                        string year = tmpParsingRowExpiry.GetRowDataValue(cs, "Expiration Year");
                                        string month = tmpParsingRowExpiry.GetRowDataValue(cs, "Expiration Month");
                                        string day = tmpParsingRowExpiry.GetRowDataValue(cs, "Expiration Day");

                                        year = "20" + (StrFunc.IsEmpty(year) ? "00" : year);
                                        month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');
                                        day = StrFunc.IsEmpty(day) ? "00" : day.PadLeft(2, '0');
                                        
                                        string maturityDate = year + month + day;
                                        DateTime dtMaturityDate = new DtFunc().StringyyyyMMddToDateTime(maturityDate);
                                        // Le jour de l'échéance
                                        tmpIsProductOk = (dtMaturityDate.CompareTo(m_dtFile) == 0);
                                    }

                                    if (tmpIsProductOk)
                                    {
                                        isExpiryOk = true;
                                        parsingRowExpiry = tmpParsingRowExpiry;
                                        isNewExpiry = true;
                                    }
                                }
                                break;
                            #endregion Expiry

                            #region Serie
                            case "S"://Serie
                                IOTaskDetInOutFileRow parsingRowSerie = null;

                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Vérifier le Product Flex
                                // PM 20151124 [20124] Toujours lire ligne serie du fichier et si contrat flexible lorsque isImportPriceOnly || isProductOk
                                //if (isImportPriceOnly)
                                Boolean isFlexible = false;
                                string flexProductID = null;
                                // PM 20151209 [21653] Ajout lecture de la série pour contract flex (ou standard) sans CM
                                //if (isImportPriceOnly || isProductOk)
                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                //if (isImportPriceOnly || isProductOk || isDCToUpdated || isCheckForFlex)
                                if (isImportPriceOnly || isProductOk || isDCToUpdated || isCheckForFlex || isExpiryOk)
                                {
                                    LoadLine(currentLine, ref parsingRowSerie);
                                    isFlexible = BoolFunc.IsTrue(parsingRowSerie.GetRowDataValue(cs, "Flex Series Flag"));
                                    if (isImportPriceOnly)
                                    {
                                        if (isFlexible)
                                        {
                                            flexProductID = parsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
                                            isProductFlexOk = IsProductToImport(config, flexProductID, alProductOk);
                                            if (isProductFlexOk)
                                            {
                                                if (config.isSearchProduct)
                                                {
                                                    // Vérifier qu'il existe des trade sur le contrat Flex 
                                                    // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                                                    //isProductFlexOk = IsExistDcInTrade(flexProductID);
                                                    isProductFlexOk = lstProductId.Contains(flexProductID);
                                                }
                                                if (isProductFlexOk)
                                                {
                                                    alProductOk.Add(flexProductID);
                                                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                                        StrFunc.AppendFormat("Add parameters values for flexible product:{0}", flexProductID));

                                                    // PM 20200102 [XXXXX] New Log
                                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for flexible product:{0}", flexProductID), 3));
                                                }
                                            }
                                            // PM 20151209 [21653] Lorsque le contract ne doit pas être importé, vérification si le contract flex est sans CM pour le mettre à jour
                                            if (isProductFlexOk == false)
                                            {
                                                isDCFlexToUpdated = dcWithoutCM.Any(dc => (dc.First == flexProductID) && (dc.Second == DerivativeContractTypeEnum.FLEX.ToString()));
                                            }
                                        }
                                        // PM 20151124 [20124] Refactoring du test plus bas
                                        else
                                        {
                                            // pour une série sur un produit standard, Spheres® vérifie que la série a été négociée uniquement si le contrat a déjà été négocié
                                            isProductOk = isProductIDInTrade;
                                        }
                                    }
                                }

                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                //if (isProductOk || isProductFlexOk)
                                if (isProductOk || isProductFlexOk || isExpiryOk)
                                {
                                    // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                    // PM 20151124 [20124] Devenu inutile car toujours fait plus haut
                                    //if (parsingRowSerie == null)
                                    //{
                                    //    LoadLine(currentLine, ref parsingRowSerie);
                                    //}

                                    // PM 20151124 [20124] Refactoring : remonté plus haut
                                    //if (isImportPriceOnly)
                                    //{
                                    //    Boolean isStandard = BoolFunc.IsFalse(parsingRowSerie.GetRowDataValue(cs, "Flex Series Flag"));
                                    //    if (isStandard)
                                    //    {
                                    //        // pour une série sur un produit standard, Spheres® vérifie que la série a été négociée uniquement si le contrat a déjà été négocié
                                    //        isProductOk = isProductIDInTrade;
                                    //    }
                                    //}
                                    // PM 20151124 [20124] Test devenu inutile
                                    //if (isProductOk)
                                    //{
                                    //int idAsset = 0;
                                    // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                                    //Boolean isSerieOk = IsSerieToImport(config, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);

                                    // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                    //Boolean isSerieOk = IsSerieToImport(config, isImportPriceOnly, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);
                                    MarketDataAssetToImport assetData;
                                    Boolean isSerieOk = IsSerieToImport(config, isImportPriceOnly, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out assetData);
                                    int idAsset = assetData.IdAsset;

                                    // Vérifier si l'on doit importer le cours de l'indice d'une option sur indice
                                    if (isUnlPriceForIndexOptionToDo && (false == isUnlPriceForIndexOptionDone) && (assetDataForIndexOption == default(MarketDataAssetToImport)))
                                    {
                                        if ((assetData.ContractCategory == "F") || (assetData.SettlementMethod != SettlMethodEnum.CashSettlement))
                                        {
                                            isUnlPriceForIndexOptionToDo = false;
                                        }
                                        else
                                        {
                                            if (idAsset != 0)
                                            {
                                                // Le cours de l'asset est à importer, vérifier s'il s'agit d'une option sur indice
                                                if ((assetData.ContractCategory == "O")
                                                    && (assetData.AssetSousJacentCategory == Cst.UnderlyingAsset.Index.ToString())
                                                    && (assetData.SettlementMethod == SettlMethodEnum.CashSettlement)
                                                    && (assetData.MaturityDate != default(DateTime))
                                                    && (assetData.IdAssetUnl > 0))
                                                {
                                                    assetDataForIndexOption = assetData;
                                                    if (assetData.MaturityDateSys != default(DateTime))
                                                    {
                                                        isExpiryDate = (assetData.MaturityDateSys == m_dtFile);
                                                    }
                                                    else
                                                    {
                                                        isExpiryDate = (assetData.MaturityDate == m_dtFile);
                                                    }
                                                    // Si ce n'est pas une échéance ne rien faire
                                                    isUnlPriceForIndexOptionDone = (false == isExpiryDate);
                                                }
                                                else
                                                {
                                                    isUnlPriceForIndexOptionToDo = false;
                                                }
                                            }
                                            else
                                            {
                                                // Lorsque l'asset n'est pas à importer, recherche d'un asset Option sur Indice en CashSettlement sur le même Contrat et Echeance
                                                assetDataForIndexOption = m_PrismaAssetToImport.GetAssetDataForIndexOption(assetData);
                                                if ((assetDataForIndexOption != default(MarketDataAssetToImport))
                                                    && (assetDataForIndexOption.MaturityDate != default(DateTime))
                                                    && (assetDataForIndexOption.IdAssetUnl > 0))
                                                {
                                                    if (assetDataForIndexOption.MaturityDateSys != default(DateTime))
                                                    {
                                                        isExpiryDate = (assetDataForIndexOption.MaturityDateSys == m_dtFile);
                                                    }
                                                    else
                                                    {
                                                        isExpiryDate = (assetDataForIndexOption.MaturityDate == m_dtFile);
                                                    }
                                                    // Si ce n'est pas une échéance ne rien faire
                                                    isUnlPriceForIndexOptionDone = (false == isExpiryDate);
                                                }
                                                else
                                                {
                                                    isUnlPriceForIndexOptionToDo = false;
                                                }
                                            }
                                        }
                                    }

                                    // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                    //if (isSerieOk)
                                    if ((isSerieOk) || (isExpiryDate && (false == isUnlPriceForIndexOptionDone)))
                                    {
                                        // PM 20181001 [XXXXX] Import file STLCF : vérifier que les cours est présent
                                        //decimal stlPrice = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(cs, "Settlement Price"));
                                        string stlPriceValue = parsingRowSerie.GetRowDataValue(cs, "Settlement Price");
                                        if (StrFunc.IsFilled(stlPriceValue))
                                        {
                                            decimal stlPrice = DecFunc.DecValueFromInvariantCulture(stlPriceValue);

                                            // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                            if (isExpiryDate && (false == isUnlPriceForIndexOptionDone))
                                            {
                                                //WARNING: De nombreux prix sont dans le fichier avec une valeur "0.01" ou vide. 
                                                //         Ces prix, a priori non significatif, son ignoré.
                                                if (stlPrice * 100 > 1)
                                                {
                                                    decimal unlStlPrice;
                                                    // FI 20190204 [24504] Il faut utiliser assetData 
                                                    //if (assetDataForIndexOption.PutCallAsFixValue == "0")
                                                    if (assetData.PutCallAsFixValue == "0")
                                                    {
                                                        // Put
                                                        unlStlPrice = assetData.StrikePrice - stlPrice;
                                                    }
                                                    else
                                                    {
                                                        // Call
                                                        unlStlPrice = assetData.StrikePrice + stlPrice;
                                                    }
                                                    // Mise à jour du cours OfficialSettlement des Options sur Indice
                                                    Cst.OTCml_TBL quoteTable = AssetTools.ConvertQuoteEnumToQuoteTbl(QuoteEnum.INDEX);
                                                    QueryParameters qryParamQuote = GetQueryParameters_QUOTE_XXX_H(cs, quoteTable, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialSettlement);
                                                    InsertUpdate_QUOTE_XXX_H(qryParamQuote, assetDataForIndexOption.IdAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialSettlement, unlStlPrice, currency);
                                                    isUnlPriceForIndexOptionDone = true;
                                                }
                                            }

                                            if (isSerieOk)
                                            {
                                            // PM 20151124 [20124] Series Version Number et Trading Unit pour mise à jour du Contract Multiplier
                                            int serieVerionNumber = IntFunc.IntValue(parsingRowSerie.GetRowDataValue(cs, "Series Version Number"));
                                            decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(cs, "Trading Unit"));

                                            // RD 20150404 / FI 20150404 si (pConfig.isSearchSerie = false) alors Idasset est à 0
                                            if (idAsset == 0)
                                                throw new NotImplementedException(StrFunc.AppendFormat("Record type {0}, asset not found", Record_Type));

                                            // PM 20151124 [20124] Lecture des données de l'asset
                                            SQL_AssetETD sqlAsset = new SQL_AssetETD(cs, idAsset);
                                            sqlAsset.LoadTable(new string[] { sqlAsset.SQLObject + "." + "IDENTIFIER", "IDDC", "ASSETCATEGORY", "IDASSET_UNL" });

                                            if (false == isImportPriceOnly)
                                            {
                                                if (lstIdAsset.Contains(idAsset))
                                                {
                                                    DataRow dataRowProduct = this.LoadRowProduct(parsingRowProduct);
                                                    int idProduct = Convert.ToInt32(dataRowProduct["IDIMPRISMAP_H"]);
                                                    string productId = dataRowProduct["PRODUCTID"].ToString();

                                                    DataRow rowExpiry = this.LoadRowProductExpiration(idProduct, productId, parsingRowExpiry);
                                                    int idExpiry = Convert.ToInt32(rowExpiry["IDIMPRISMAE_H"]);

                                                    decimal pvPrice = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(cs, "PV Reference Price"));
                                                    Nullable<decimal> unlPriceOffset = null;
                                                    string sUnlPriceOffset = parsingRowSerie.GetRowDataValue(cs, "Underlying price offset");
                                                    if (StrFunc.IsFilled(sUnlPriceOffset))
                                                    {
                                                        unlPriceOffset = DecFunc.DecValueFromInvariantCulture(sUnlPriceOffset);
                                                    }

                                                    QueryParameters qryUpdatePrice = GetQueryUpdPriceOfSeries(cs, idAsset, idExpiry, pvPrice, stlPrice, unlPriceOffset);

                                                    if (sqlUpd.Length > 0)
                                                    {
                                                        sqlUpd.Append(Cst.CrLf);
                                                    }
                                                    sqlUpd.Append(qryUpdatePrice.GetQueryReplaceParameters(false));
                                                    if (sqlUpd.Length >= config.sqlLimitSize)
                                                    {
                                                        DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpd.ToString());
                                                        sqlUpd = new StringBuilder();
                                                    }
                                                }
                                                else
                                                {
                                                    // PM 20151124 [20124] sqlAsset defini plus haut
                                                    //SQL_AssetETD sqlAsset = new SQL_AssetETD(cs, idAsset);
                                                    //sqlAsset.LoadTable(new string[] { sqlAsset.SQLObject + "." + "IDENTIFIER" });
                                                    task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), 
                                                        StrFunc.AppendFormat("Asset (identifier:{0})(id:{1}) does not exist in file TheoInst", sqlAsset.Identifier, idAsset));

                                                    // PM 20200102 [XXXXX] New Log
                                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Asset (identifier:{0})(id:{1}) does not exist in file TheoInst", sqlAsset.Identifier, idAsset), 3));
                                                }
                                            }

                                            // PM 20151216 [21662] Ne rien faire quand il s'agit de l'importation Eurosys
                                            if (false == IsEurosysSoftware)
                                            {
                                                // PM 20161019 [22174] Prisma 5.0 : Ajout du Delta
                                                //// PM 20151124 [20124] Insertion/Mise à jour des cotations
                                                //InsertUpdate_QUOTE_XXX_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency);
                                                string sDelta = parsingRowSerie.GetRowDataValue(cs, "Delta");
                                                if (StrFunc.IsFilled(sDelta))
                                                {
                                                    decimal delta = DecFunc.DecValueFromInvariantCulture(sDelta);
                                                    InsertUpdate_QUOTE_ETD_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency, delta);
                                                }
                                                else
                                                {
                                                    // PM 20151124 [20124] Insertion/Mise à jour des cotations
                                                    InsertUpdate_QUOTE_XXX_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency);
                                                }

                                                // PM 20151124 [20124] Insertion/Mise à jour des cotations des sous-jacents
                                                if (isNewExpiry)
                                                {
                                                    // PM 20161019 [22174] Prisma 5.0 : "Underlying close Price" peu ne pas être renseigné
                                                    //decimal unlClosePrice = DecFunc.DecValueFromInvariantCulture(parsingRowExpiry.GetRowDataValue(cs, "Underlying close Price"));
                                                    string sUnlClosePrice = parsingRowExpiry.GetRowDataValue(cs, "Underlying close Price");
                                                    if (StrFunc.IsFilled(sUnlClosePrice))
                                                    {
                                                        decimal unlClosePrice = DecFunc.DecValueFromInvariantCulture(sUnlClosePrice);
                                                        InsertUpdateUnderliyngPrice(sqlAsset, unlClosePrice, currency);
                                                    }
                                                    isNewExpiry = false;
                                                }

                                                // PM 20151124 [20124] Mise à jour du Contract Mutliplier
                                                if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                                {
                                                    string contractSymbol = isFlexible ? flexProductID : productID;
                                                    bool isContractUpdated = updatedDC.Exists(p => p.First == contractSymbol && p.Second == serieVerionNumber);
                                                    //
                                                    if ((isContractUpdated == false) || (serieVerionNumber > 0))
                                                    {
                                                        decimal contractMultiplier = productTickValue / productTickSize * tradingUnit;
                                                        int nbrowsContract = 0;
                                                        int nbrowsAsset = 0;
                                                        Quote_ETDAsset quoteETD = null;

                                                        // Mise à jour du Contract Mutliplier sur le DC
                                                        if (isContractUpdated == false)
                                                        {
                                                            QueryParameters qryUpdateDC;
                                                            if (serieVerionNumber > 0)
                                                            {
                                                                qryUpdateDC = GetQueryUpdDERIVATIVECONTRACTLight(idAsset, productTickSize);
                                                            }
                                                            else
                                                            {
                                                                qryUpdateDC = GetQueryUpdDERIVATIVECONTRACT(idAsset, contractMultiplier, productTickValue * tradingUnit, productTickSize);
                                                            }
                                                            nbrowsContract = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryUpdateDC.query, qryUpdateDC.parameters.GetArrayDbParameter());
                                                            updatedDC.Add(new Pair<string, int>(contractSymbol, serieVerionNumber));
                                                        }

                                                        // Mise à jour du Contract Mutliplier sur l'asset
                                                        if (serieVerionNumber > 0)
                                                        {
                                                            // PM 20160215 [21491] Déplacement de la méthode dans la classe de base
                                                            //QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETD(idAsset, contractMultiplier);
                                                            QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETD(cs, idAsset, contractMultiplier, task.process.UserId, dtStart);
                                                            nbrowsAsset = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryUpdateAsset.query, qryUpdateAsset.parameters.GetArrayDbParameter());
                                                        }

                                                        // Test si le CM du DC a été modifié
                                                        if ((nbrowsContract > 0) && (serieVerionNumber == 0))
                                                        {
                                                            // Quote Handling pour le contract
                                                            quoteETD = new Quote_ETDAsset();
                                                            quoteETD.QuoteTable = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                                                            quoteETD.idDC = sqlAsset.IdDerivativeContract;
                                                            quoteETD.idDCSpecified = true;
                                                        }
                                                        // Test si l'asset a été modifié
                                                        else if (nbrowsAsset > 0)
                                                        {
                                                            // Quote Handling pour l'asset
                                                            quoteETD = new Quote_ETDAsset();
                                                            quoteETD.QuoteTable = Cst.OTCml_TBL.ASSET_ETD.ToString();
                                                            quoteETD.idAsset = idAsset;
                                                        }
                                                        // Test si besoin d'envoyer à QuotationHandling
                                                        if (quoteETD != null)
                                                        {
                                                            quoteETD.action = DataRowState.Modified.ToString();
                                                            quoteETD.contractMultiplier = contractMultiplier;
                                                            quoteETD.contractMultiplierSpecified = true;
                                                            quoteETD.timeSpecified = true; //FI 20170306 [22225] add
                                                            quoteETD.time = m_dtFile;
                                                            quoteETD.isCashFlowsVal = true;
                                                            quoteETD.isCashFlowsValSpecified = true;
                                                            //
                                                            MQueueAttributes mQueueAttributes = new MQueueAttributes(cs);
                                                            QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);
                                                            //
                                                            IOTools.SendMQueue(task, qHMQueue, Cst.ServiceEnum.SpheresQuotationHandling, Cst.ProcessTypeEnum.QUOTHANDLING);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                }
                                else if (isDCToUpdated || isDCFlexToUpdated) // PM 20151209 [21653] Mise à jour des DC sans CM
                                {
                                    int serieVerionNumber = IntFunc.IntValue(parsingRowSerie.GetRowDataValue(cs, "Series Version Number"));
                                    if (serieVerionNumber == 0)
                                    {
                                        decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(cs, "Trading Unit"));
                                        if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                        {
                                            string contractSymbol = isFlexible ? flexProductID : productID;
                                            bool isContractUpdated = updatedDC.Exists(p => p.First == contractSymbol && p.Second == serieVerionNumber);
                                            if (isContractUpdated == false)
                                            {
                                                DataRow dcToUptade = dataRowDCWithoutCM.FirstOrDefault(dr => dr.Field<string>("CONTRACTSYMBOL") == contractSymbol);
                                                if (dcToUptade != default(DataRow))
                                                {
                                                    decimal contractMultiplier = productTickValue / productTickSize * tradingUnit;
                                                    dcToUptade["CONTRACTMULTIPLIER"] = contractMultiplier;
                                                    dcToUptade["MINPRICEINCRAMOUNT"] = productTickValue * tradingUnit;
                                                    dcToUptade["MINPRICEINCR"] = productTickSize;
                                                    dcToUptade["IDAUPD"] = task.process.UserId;
                                                    dcToUptade["DTUPD"] = dtStart;
                                                    updatedDC.Add(new Pair<string, int>(contractSymbol, serieVerionNumber));
                                                }
                                            }
                                            if (isFlexible)
                                            {
                                                isDCFlexToUpdated = false;
                                            }
                                            else
                                            {
                                                isDCToUpdated = false;
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion Serie

                            case "*EOF*":
                                if ((null != sqlUpd) && sqlUpd.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpd.ToString());
                                    sqlUpd = new StringBuilder();
                                }
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }
                // PM 20151209 [21653] Mise à jour des DC sans Contract Multiplier
                DataHelper.ExecuteDataAdapter(cs, queryDCMutliplier.GetQueryReplaceParameters(), dtDC);

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitTheoreticalPriceFileConfig()
        {
            PrismaInputFileConfig config = null;

            string productList = GetAppSetting("InputTheoreticalPriceFile", "productList");
            string nbProduct = GetAppSetting("InputTheoreticalPriceFile", "nbProduct");

            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputTheoreticalPriceFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.isSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string searchSerie = GetAppSetting("InputTheoreticalPriceFile", "isSearchSerie");
            if (StrFunc.IsFilled(searchSerie))
            {
                config.isSearchSerie = BoolFunc.IsTrue(searchSerie);
            }

            string sqlLimitSize = GetAppSetting("InputTheoreticalPriceFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.sqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Market capacities
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitMarketCapacitiesFileConfig()
        {
            PrismaInputFileConfig config = null;

            string productList = GetAppSetting("InputMarketCapacitiesFile", "productList");
            string nbProduct = GetAppSetting("InputMarketCapacitiesFile", "nbProduct");

            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputMarketCapacitiesFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.isSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string sqlLimitSize = GetAppSetting("InputMarketCapacitiesFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.sqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier foreign Exchange rate
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitFxExchangeRateFileConfig()
        {
            PrismaInputFileConfig config = new PrismaInputFileConfig();

            string sqlLimitSize = GetAppSetting("InputFxExchangeRateFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.sqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Settlement prices
        /// </summary>
        /// <returns></returns>
        ///  FI 20140617 [19911] add methode
        private PrismaInputFileConfig InitSettlementPriceFileConfig()
        {
            PrismaInputFileConfig config = null;

            string productList = GetAppSetting("InputSettlementPriceFile", "productList");
            string nbProduct = GetAppSetting("InputSettlementPriceFile", "nbProduct");

            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputSettlementPriceFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.isSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string searchSerie = GetAppSetting("InputSettlementPriceFile", "isSearchSerie");
            if (StrFunc.IsFilled(searchSerie))
            {
                config.isSearchSerie = BoolFunc.IsTrue(searchSerie);
            }

            string sqlLimitSize = GetAppSetting("InputSettlementPriceFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.sqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Retourne le la clé présente dans le fichier de config
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private static string GetAppSetting(string prefix, string key)
        {
            string ret = SystemSettings.GetAppSettings(prefix + "_" + key);
            if (StrFunc.IsEmpty(ret))
                ret = SystemSettings.GetAppSettings("IMPORTPRISMA_");
            return ret;
        }

        /// <summary>
        ///  Retourne true si le produit est à importer
        ///  <para>Lecture du fichier de configuration</para>
        /// </summary>
        /// <param name="pConfig">configuration de l'importation</param>
        /// <param name="pProductID">Symbol du produit à importer</param>
        /// <param name="pAlProduct">Liste des produits déjà importés</param>
        /// <returns></returns>
        /// FI 20140618 [19911] Modification de comportement, cette méthode ne teste plus l'existence de trade négocié sur le produit 
        private Boolean IsProductToImport(PrismaInputFileConfig pConfig, string pProductID, ArrayList pAlProduct)
        {
            bool ret = true;
            if ((pConfig.nbProductMax > 0) || (pConfig.restrictProduct.Count > 0))
            {
                if (pConfig.restrictProduct.Count > 0)
                    ret = pConfig.restrictProduct.Contains(pProductID);

                if (ret && pConfig.nbProductMax > 0)
                    ret = (ArrFunc.Count(pAlProduct) < pConfig.nbProductMax);
            }
            //
            // FI 20140618 [19911] Mise en commentaire
            //if (ret == true && pConfig.isSearchProduct)
            //{
            //    // Pour des raisons de perfs => Spheres® ne teste pas la présence du DC dans le référentiel mais uniqement dans les trades
            //    //ret = IsExistDerivativeContract(pProductID);
            //    //if (ret)
            //    ret = IsExistDcInTrade(pProductID);
            //}

            return ret;
        }

        /// <summary>
        ///  Retourne true si le serie est à importer
        /// </summary>
        /// <param name="config">configuration de l'importation</param>
        /// <param name="pIsImportPriceOnly">Indicateur d'import de cours uniquement</param>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne produit</param>
        /// <param name="pParsingRowSerie">Représente le parsing de la ligne serie</param>
        /// <param name="pParsingRowExpiration">Représente le parsing de la ligne échéance</param>
        /// <param name="pAssetData"></param> 
        /// <returns></returns>
        // PM 20180219 [23824] Ajout de la gestion de pIsImportPriceOnly
        //private Boolean IsSerieToImport(PrismaInputFileConfig pConfig, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out int pIdAsset)
        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
        //private Boolean IsSerieToImport(PrismaInputFileConfig pConfig, bool pIsImportPriceOnly, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out int pIdAsset)
        private Boolean IsSerieToImport(PrismaInputFileConfig pConfig, bool pIsImportPriceOnly, IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out MarketDataAssetToImport pAssetData)
        {
            bool ret = true;
            int idAsset = 0;

            if (pConfig.isSearchSerie)
            {
                if (pIsImportPriceOnly)
                {
                    idAsset = GetIdAssetETDForPriceOnly(pParsingRowProduct, pParsingRowSerie, pParsingRowExpiration, out pAssetData);
                }
                else
                {
                    idAsset = GetIdAssetETD(pParsingRowProduct, pParsingRowSerie, pParsingRowExpiration, out pAssetData);
                }
                ret = (idAsset > 0);
            }
            else
            {
                pAssetData = new MarketDataAssetToImport();
                pAssetData.IdAsset = 0;
            }
            return ret;
        }

        /// <summary>
        /// Retourne les caractères présents entre le début de la ligne et le 1er ";" 
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        private static string GetFirstElement(string pLine)
        {
            int index = pLine.IndexOf(";");
            if (false == index > 0)
                throw new Exception("Missing ';' char");

            string ret = pLine.Substring(0, index);
            return ret;
        }

        /// <summary>
        /// Récupération du "Record Type" de la ligne dans le fichier
        /// </summary>
        /// <param name="pLine">Représnte une ligne du fichier</param>
        /// <returns></returns>
        private static string GetRecordType(string pLine)
        {
            return GetFirstElement(pLine);
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier theoritical prices and instrument configuration 
        /// </summary>
        private int Delete_TheoreticalPriceFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"delete from dbo.IMPRISMAP_H where (IDIMPRISMA_H = @ID)";

            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier liquidity factors
        /// </summary>
        private int Delete_LiquidityFactorsFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"
delete from dbo.IMPRISMALIQFACT_H 
where IDIMPRISMALIQCLASS_H 
in 
(
    select IDIMPRISMALIQCLASS_H from dbo.IMPRISMALIQCLASS_H
    where (IDIMPRISMA_H=@ID)
)";

            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier market capacities
        /// </summary>
        private int Delete_MarketCapacitiesFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"delete from dbo.IMPRISMAMKTCAPA_H where IDIMPRISMA_H=@ID";

            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier Foreign Exchange Rates
        /// </summary>
        private int Delete_FxExchangeRatesFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

// PM 20141022 Supression d'une clause "IN"
//            string sqlQuery = @"
//delete from dbo.IMPRISMAFXRMS_H 
// where IDIMPRISMAFXPAIR_H in
//     ( select IDIMPRISMAFXPAIR_H from dbo.IMPRISMAFXPAIR_H
//        where IDIMPRISMAFX_H in
//            ( select IDIMPRISMAFX_H from dbo.IMPRISMAFX_H 
//               where (IDIMPRISMA_H = @ID)
//            )
//     )";
            string sqlQuery = @"
delete from dbo.IMPRISMAFXRMS_H 
 where IDIMPRISMAFXPAIR_H in
     ( select fxpair.IDIMPRISMAFXPAIR_H from dbo.IMPRISMAFXPAIR_H fxpair
        inner join dbo.IMPRISMAFX_H fx on fx.IDIMPRISMAFX_H = fxpair.IDIMPRISMAFX_H
        where (fx.IDIMPRISMA_H = @ID)
     )";
            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery, dataParameters);
            int nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            sqlQuery = @"
delete from dbo.IMPRISMAFXPAIR_H
 where IDIMPRISMAFX_H in
     ( select IDIMPRISMAFX_H from dbo.IMPRISMAFX_H 
        where (IDIMPRISMA_H = @ID)
     )";

            qryParameters = new QueryParameters(cs, sqlQuery, dataParameters);
            nRowDeleted = DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Retourne true si le symbol existe dans Spheres®
        /// <para>Rq: Sous Spheres® plusieurs Contrat derivé peuvent exister pour un même symbol (dans ce cas, la version sera différente (DERIVATIVECONTRACT.CONTRACTATTRIBUTE)</para>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        /// PM 20170531 [22834] Méthode non utilisée
        //private Boolean IsExistDerivativeContract(string pContractSymbol)
        //{
        //    m_QueryExistDC.parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

        //    object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(cs), CommandType.Text, m_QueryExistDC.query, m_QueryExistDC.parameters.GetArrayDbParameter());
        //    bool ret = (obj != null);

        //    return ret;
        //}

        /// <summary>
        /// Retourne true si le symbol est négocié dans Spheres®
        /// <para>Rq: Sous Spheres® plusieurs Contrat derivé peuvent exister pour un même symbol (dans ce cas, la version sera différente (DERIVATIVECONTRACT.CONTRACTATTRIBUTE)</para>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        // PM 20141022 [9700] Eurex Prisma for Eurosys Futures : La méthode devient "virtual"
        protected virtual Boolean IsExistDcInTrade(string pContractSymbol)
        {
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //return base.IsExistDcInTrade(CSTools.SetCacheOn(cs), m_dtFile, EUREX_EXCHANGEACRONYM, pContractSymbol);
            return base.IsExistDcInTrade(CSTools.SetCacheOn(cs), m_dtFile, ExchangeAcronym, pContractSymbol);
        }

        /// <summary>
        /// Alimente la date du fichier (date de traitement)
        /// <para>Récupère la date sur la dernière ligne du fichier</para>
        /// </summary>
        /// FI 20150727 [21224] Modify 
        private void SetDateFile()
        {
            try
            {
                IOTaskDetInOutFileRow parsingRow = null;
                
                string lastline = FileTools.GetLastLine(dataName);
                LoadLine(lastline, ref parsingRow);

                // PM 20150804 [21236] Ajout dtFunc et stringToDateTime pour correction mauvais format de date suite à la modif pour le ticket [21224]
                DtFunc dtFunc = new DtFunc();
                StringToDateTime stringToDateTime;

                string date = string.Empty;
                if (null == parsingRow) // la ligne n'est pas systématiquement parsée (cas du fichier  LIQUIDITY FACTORS par exemple)
                {
                    if (lastline.StartsWith("*EOF*"))
                    {
                        string[] lineValues = lastline.Split(';');
                        date = lineValues[3];
                        // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                        stringToDateTime = dtFunc.StringyyyyMMddToDateTime;
                    }
                    else
                    {
                        throw new Exception("Last line doesn't start with *EOF*");
                    }
                }
                else
                {
                    date = parsingRow.GetRowDataValue(cs, "Current business day");
                    // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                    stringToDateTime = dtFunc.StringDateISOToDateTime;
                }

                // FI 20150727 [21224] garde fou Exception avec message d'erreur clair lorsque la date est vide 
                if (StrFunc.IsEmpty(date))
                {
                    throw new Exception("Current business day is empty.");
                }

                // PM 20150804 [21236] Correction mauvais format de date suite à la modif pour le ticket [21224]
                //m_dtFile = new DtFunc().StringyyyyMMddToDateTime(date);
                m_dtFile = stringToDateTime(date);
            }
            catch (Exception e)
            {
                throw new Exception("Error while initializing the current business day.", e);
            }
        }

        /// <summary>
        ///  Retourne l'enregistrement dans IMPRISMA_H qui correspond à la date de traitement
        ///  <para>Alimente au passage le membre idIMPRISMA_H</para>
        ///  <para>Alimente la table s'il n'existe aucun enregistrement en date de traitement</para>
        /// </summary>
        private DataRow LoadRowIMPRISMA_H()
        {
            string queryDataAdapter = null;

            // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
            //DataTable dt = GetDataTable_IMPRISMA_H(cs, m_dtFile, out queryDataAdapter);
            DataTable dt = GetDataTable_IMPRISMA_H(cs, m_dtFile, ExchangeAcronym, out queryDataAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["DTBUSINESS"] = m_dtFile;
                // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
                dr["EXCHANGEACRONYM"] = ExchangeAcronym;

                SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryDataAdapter, dt);
                // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
                //dt = GetDataTable_IMPRISMA_H(cs, m_dtFile, out queryDataAdapter);
                dt = GetDataTable_IMPRISMA_H(cs, m_dtFile, ExchangeAcronym, out queryDataAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMA_H is not loaded for date:{0}", DtFunc.DateTimeToStringISO(m_dtFile)));
            }

            this.idIMPRISMA_H = Convert.ToInt32(dt.Rows[0]["IDIMPRISMA_H"]);

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate">Date de traitement</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym du fichier en cours d'importation</param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
        //private static DataTable GetDataTable_IMPRISMA_H(string pCS, DateTime pDate, out string pQuery)
        private static DataTable GetDataTable_IMPRISMA_H(string pCS, DateTime pDate, string pExchangeAcronym, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE), pDate);
            // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGEACRONYM), pExchangeAcronym);

            //string sqlQuery = "select * from dbo.IMPRISMA_H where DTBUSINESS = @DTFILE";
            string sqlQuery = "select * from dbo.IMPRISMA_H where (DTBUSINESS = @DTFILE) and (EXCHANGEACRONYM = @EXCHANGEACRONYM)";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAP_H qui correspond au produit {pRowProduct}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAP_H s'il n'existe pas</para>
        /// <para>Alimente la table IMPRISMALG_H et IMPRISMALIQCLASS_H si nécessaire</para>
        /// </summary>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne Product du produit</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowProduct(IOTaskDetInOutFileRow pParsingRowProduct)
        {
            string productId = pParsingRowProduct.GetRowDataValue(cs, "Product ID");
            string queryAdapter = null;
            DataTable dt = GetTableProduct(CSTools.SetCacheOn(cs,1,null), productId, idIMPRISMA_H, out queryAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();

                dr["IDIMPRISMA_H"] = idIMPRISMA_H;

                dr["PRODUCTID"] = productId;
                dr["TICKSIZE"] = DecFunc.DecValueFromInvariantCulture(pParsingRowProduct.GetRowDataValue(cs, "Tick Size"));
                dr["TICKVALUE"] = DecFunc.DecValueFromInvariantCulture(pParsingRowProduct.GetRowDataValue(cs, "Tick Value"));
                dr["IDC"] = pParsingRowProduct.GetRowDataValue(cs, "Currency");

                string liquidityClass = pParsingRowProduct.GetRowDataValue(cs, "Liquidity Class");
                DataRow drLiquidityClass = LoadRowLiquidityClass(liquidityClass);
                dr["IDIMPRISMALIQCLASS_H"] = drLiquidityClass["IDIMPRISMALIQCLASS_H"];

                string liquidationGroup = pParsingRowProduct.GetRowDataValue(cs, "Liquidation Group");
                DataRow drLiquidationGroup = LoadRowLiquidationGroup(liquidationGroup);
                dr["IDIMPRISMALG_H"] = drLiquidationGroup["IDIMPRISMALG_H"];

                dr["MARGINSTYLE"] = pParsingRowProduct.GetRowDataValue(cs, "Margin Style");
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableProduct(CSTools.SetCacheOn(cs, 1, null), productId, idIMPRISMA_H, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAP_H is not loaded for Product:{0}", productId));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductId"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableProduct(string pCS, string pProductId, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pProductId);

            string sqlQuery = "select * from dbo.IMPRISMAP_H where IDIMPRISMA_H=@IDIMPRISMA_H and PRODUCTID=@PRODUCTID";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALG_H qui correspond au groupe de liquidation {pLiquidationGroup}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALG_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pLiquidationGroup">représente l'identifier du groupe de liquidation</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroup(string pLiquidationGroup)
        {
            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow();
            row.data = new IOTaskDetInOutFileRowData[1] { new IOTaskDetInOutFileRowData() };
            row.data[0].name = "Liquidation Group";
            row.data[0].value = pLiquidationGroup;
            row.data[0].datatype = TypeData.TypeDataEnum.@string.ToString();
            row.data[0].datatypeSpecified = true;
            return LoadRowLiquidationGroup(row);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALG_H qui correspond au groupe de liquidation {pRowLiquidationGroup}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALG_H s'il n'existe pas</para>
        /// <para>Mise à jour lorsque le fichier est "Eurex Risk Measure configuration, "</para>
        /// </summary>
        /// <param name="pRowLiquidationGroup">Représente le parsing d'une ligne de type LiquidationGroup</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowLiquidationGroup(IOTaskDetInOutFileRow pRowLiquidationGroup)
        {
            string liquidationGroup = pRowLiquidationGroup.GetRowDataValue(cs, "Liquidation Group");

            string queryDataAdapter = null;
            DataTable dt = GetTableLiquidationGroup(CSTools.SetCacheOn(cs,1,null), liquidationGroup, idIMPRISMA_H, out queryDataAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidationGroup;
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        dr["CURRENCYTYPEFLAG"] = pRowLiquidationGroup.GetRowDataValue(cs, "Currency Type Flag");
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        dr["CURRENCYTYPEFLAG"] = "P"; // P Comme PRODUCT (valeur par défaut sur la table)
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryDataAdapter, dt);
                dt = GetTableLiquidationGroup(CSTools.SetCacheOn(cs, 1, null), liquidationGroup, idIMPRISMA_H, out queryDataAdapter);
            }
            else if (dt.Rows.Count == 1)
            {
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        DataRow dr = dt.Rows[0];
                        dr["CURRENCYTYPEFLAG"] = pRowLiquidationGroup.GetRowDataValue(cs, "Currency Type Flag");
                        //SetDataRowUpd(dr);

                        DataHelper.ExecuteDataAdapter(cs, queryDataAdapter, dt);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //NOTHING TO DO
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
            }
            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMLAG_H is not loaded for Liquidation Group:{0}", liquidationGroup));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Retourne un DataTable
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLiquidationGroup"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroup(string pCS, string pLiquidationGroup, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidationGroup);

            string sqlQuery = "select * from dbo.IMPRISMALG_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALGS_H qui correspond à un groupe de liquidation Split {pLiquidationGroupSplit}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup">identifiant non significatif dun groupe de liquidation</param>
        /// <param name="pLiquidationGroupSplit">identifiant d'une groupe de liquidation Split</param>
        /// <param name="pParsingRowRiskMethod">Représente le parsing d'une ligne de type RM (risk Method), ne doit être valorisé uniquement sur le fichier Risk Measure Aggregation</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroupSplit(int pIdLiquidationGroup, string pLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowRiskMethod)
        {
            IOTaskDetInOutFileRow parsingRow = new IOTaskDetInOutFileRow();
            parsingRow.data = new IOTaskDetInOutFileRowData[1] { new IOTaskDetInOutFileRowData() };
            parsingRow.data[0].name = "Liquidation Group Split";
            parsingRow.data[0].value = pLiquidationGroupSplit;
            parsingRow.data[0].datatype = TypeData.TypeDataEnum.@string.ToString();
            parsingRow.data[0].datatypeSpecified = true;

            return LoadRowLiquidationGroupSplit(pIdLiquidationGroup, parsingRow, pParsingRowRiskMethod);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALGS_H qui correspond au liquidation Group Split {pLiquidationGroupSplit}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup">Représente le parsing d'une ligne de type LG (Liquidation Group)</param>
        /// <param name="pParsingRowLiquidationGroupSplit">Représente le parsing d'une ligne de type LGS (Release 2 uniquement)</param>
        /// <param name="pParsingRowRiskMethod">Représente le parsing d'une ligne de type RM (risk Method), ne doit être valorisé uniquement sur le fichier Risk Measure Aggregation</param>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowLiquidationGroupSplit(int pIdLiquidationGroup, IOTaskDetInOutFileRow pParsingRowLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowRiskMethod)
        {
            switch (inputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    //PM 20140608 [19911] Ne pas générer d'exeption car sur la ligne LGS en release 2 pParsingRowRiskMethod == null
                    //if (pParsingRowRiskMethod == null)
                    //    throw new ArgumentException(StrFunc.AppendFormat("Arg :{0} is null", "pRowRiskMethod"));
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    //NOTHING TO DO
                    break;
                default:
                    throw new Exception(StrFunc.AppendFormat("File :{0} is not implemented", inputSourceDataStyle.ToString()));
            }

            string liquidationGroupSplit = pParsingRowLiquidationGroupSplit.GetRowDataValue(cs, "Liquidation Group Split");
            string queryDataAdapter = null;
            DataTable dt = GetTableLiquidationGroupSplit(CSTools.SetCacheOn(cs,1,null), pIdLiquidationGroup, liquidationGroupSplit, out queryDataAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidationGroupSplit;
                dr["IDIMPRISMALG_H"] = pIdLiquidationGroup;
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //PM 20140608 [19911] Uniquement lorsque pParsingRowRiskMethod != null
                        if (pParsingRowRiskMethod != null)
                        {
                            dr["RISKMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(cs, "Risk Method ID");
                            dr["AGGREGATIONMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(cs, "Aggregation Method");
                        }
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        dr["RISKMETHOD"] = Convert.DBNull;
                        dr["AGGREGATIONMETHOD"] = Convert.DBNull;
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
                //FI 20140617 [19911] 
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryDataAdapter, dt);
                dt = GetTableLiquidationGroupSplit(CSTools.SetCacheOn(cs, 1, null), pIdLiquidationGroup, liquidationGroupSplit, out queryDataAdapter);
            }
            else if (dt.Rows.Count == 1)
            {
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //PM 20140608 [19911] Uniquement lorsque pParsingRowRiskMethod != null
                        if (pParsingRowRiskMethod != null)
                        {
                            DataRow dr = dt.Rows[0];
                            //PM 20140608 [19911] Add RISKMETHOD
                            dr["RISKMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(cs, "Risk Method ID");
                            dr["AGGREGATIONMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(cs, "Aggregation Method");
                            //SetDataRowUpd(dr);

                            DataHelper.ExecuteDataAdapter(cs, queryDataAdapter, dt);
                            DataHelper.queryCache.Remove("IMPRISMALGS_H", cs);
                        }
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        //NOTHING TODO
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMLALGS_H is not loaded for Liquidation Group Split:{0}", liquidationGroupSplit));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Lecture de la table IMPRISMALGS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLiquidationGroup"></param>
        /// <param name="pLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroupSplit(string pCS, int pIdLiquidationGroup, string pLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALG_H", DbType.Int32), pIdLiquidationGroup);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMALGS_H where IDIMPRISMALG_H=@IDIMPRISMALG_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALIQCLASS_H qui correspond à la classe de liquidation  {pLiquidityClass}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAPLIQCLASS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pLiquidityClass">Représente le parsing d'une ligne de type LiquidationClass</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidityClass(string pLiquidityClass)
        {
            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow();
            row.data = new IOTaskDetInOutFileRowData[1] { new IOTaskDetInOutFileRowData() };
            row.data[0].name = "Liquidity Class";
            row.data[0].value = pLiquidityClass;
            row.data[0].datatype = TypeData.TypeDataEnum.@string.ToString();
            row.data[0].datatypeSpecified = true;
            return LoadRowLiquidityClass(row);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALIQCLASS_H qui correspond à la classe de liquidation  {pLiquidityClass}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALIQCLASS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowLiquidityClass">Représente une classe de liquidation </param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidityClass(IOTaskDetInOutFileRow pParsingRowLiquidityClass)
        {
            string liquidityClass = pParsingRowLiquidityClass.GetRowDataValue(cs, "Liquidity Class");

            string queryAdapter = null;
            DataTable dt = GetTableLiquidityClass(CSTools.SetCacheOn(cs, 1, null), liquidityClass, idIMPRISMA_H, out queryAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidityClass;
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableLiquidityClass(CSTools.SetCacheOn(cs, 1, null), liquidityClass, idIMPRISMA_H, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMALIQCLASS_H is not loaded for {0}", liquidityClass));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLiquidityClass"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidityClass(string pCS, string pLiquidityClass, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidityClass);

            string sqlQuery = "select * from dbo.IMPRISMALIQCLASS_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'Id non significatif de la serie dans Spheres®
        /// <para>Retourne 0 si la serie n'existe pas ds Spheres® ou si la série n'a jamais été négociée</para>
        /// </summary>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne produit</param>
        /// <param name="pParsingRowSerie">Représente le parsing de la ligne serie</param>
        /// <param name="pParsingRowExpiration">Représente le parsing de la ligne échéance</param>
        /// <param name="pAssetData">Données parsées de l'asset</param>
        /// <returns></returns>
        /// FI 20140618 [19911] lecture de l'élément Flex Product ID
        // PM 20141022 [9700] Eurex Prisma for Eurosys Futures : La méthode devient "protected virtual"
        // EG 20180426 Analyse du code Correction [CA2202]
        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
        //protected virtual int GetIdAssetETD(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration)
        protected virtual int GetIdAssetETD(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out MarketDataAssetToImport pAssetData)
        {
            int ret = 0;
            pAssetData = new MarketDataAssetToImport();

            //Row Product
            pAssetData.ContractSymbol = pParsingRowProduct.GetRowDataValue(cs, "Product ID");

            //Row Expiration
            //PM 20140612 [19911] En Release 2.0, l'échéance n'est pas forcement la date d'échéance : utilisation de Contract Year/Month au lieu de Expiration Year/Month
            //string year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            //string month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            string year = pParsingRowExpiration.GetRowDataValue(cs, "Contract Year");
            string month = pParsingRowExpiration.GetRowDataValue(cs, "Contract Month");
            string day = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Day");

            //PM 20150902 [21236] Contract Year/Month n'est pas toujours renseigné (notamment pour les flex), dans ce cas utiliser Expiration Year/Month
            if (StrFunc.IsEmpty(year))
            {
                year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            }
            if (StrFunc.IsEmpty(month))
            {
                month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            }

            //Row Serie
            
            string callPut = pParsingRowSerie.GetRowDataValue(cs, "Call Put Flag");
            string exerciseStyle = pParsingRowSerie.GetRowDataValue(cs, "Series exercise style flag");
            string strikePrice = pParsingRowSerie.GetRowDataValue(cs, "Exercise Price");
            string seriesVersion = pParsingRowSerie.GetRowDataValue(cs, "Series Version Number");
            string settlementType = pParsingRowSerie.GetRowDataValue(cs, "Settlement Type");
            string flex = pParsingRowSerie.GetRowDataValue(cs, "Flex Series Flag");

            //contractSymbol spécifique si flex
            Boolean isFlex = BoolFunc.IsTrue(flex);
            if (isFlex)
            {
                pAssetData.ContractSymbol = pParsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
                pAssetData.ContractType = "FLEX";
            }
            else
            {
                pAssetData.ContractType = "STD";
            }
            
            year = "20" + (StrFunc.IsEmpty(year) ? "00" : year);
            month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');
            day = StrFunc.IsEmpty(day) ? "00" : day.PadLeft(2, '0');
            callPut = StrFunc.IsFilled(callPut) ? callPut : "F";

            //Boolean isUseContractAttrib = (seriesVersion != "0");
            Boolean isUseContractAttrib = true;
            
            //
            pAssetData.MaturityMonthYear = year + month;
            if (isFlex)
            {
                pAssetData.MaturityMonthYear += day;
            }
            //
            pAssetData.SettlementMethod = GetSettlementMethod(settlementType);
            pAssetData.ContractAttribute = seriesVersion;
            pAssetData.ContractCategory = GetCategory(callPut);
            //
            QueryParameters queryParameters = null;
            DataParameters dp = null;
            if (pAssetData.ContractCategory == "O")
            {
                bool isUseDcSettltMethod = true;
                bool isUseDcExerciseStyle = true;
                bool isUseDcContractMultiplier = false;

                pAssetData.ExerciseStyle = GetExerciseStyle(exerciseStyle);
                pAssetData.PutCallAsFixValue = GetPutCall(callPut);
                pAssetData.StrikePrice = DecFunc.DecValueFromInvariantCulture(strikePrice);

                queryParameters = QueryExistAssetOptionInTrades(cs, m_MaturityType, isUseDcSettltMethod, isUseDcExerciseStyle, isUseDcContractMultiplier, isUseContractAttrib);
                dp = queryParameters.parameters;

                dp["EXERCISESTYLE"].Value = pAssetData.ExerciseStyle;
                dp["PUTCALL"].Value = pAssetData.PutCallAsFixValue;
                dp["STRIKEPRICE"].Value = pAssetData.StrikePrice;
                dp["SETTLTMETHOD"].Value = ReflectionTools.ConvertEnumToString<SettlMethodEnum>(pAssetData.SettlementMethod);
            }
            else if (pAssetData.ContractCategory == "F")
            {
                bool isUseDcSettltMethod = true;
                bool isUseDcContractMultiplier = false;

                queryParameters = QueryExistAssetFutureInTrades(cs, m_MaturityType, isUseDcSettltMethod, isUseDcContractMultiplier, isUseContractAttrib);
                dp = queryParameters.parameters;
                dp["SETTLTMETHOD"].Value = ReflectionTools.ConvertEnumToString<SettlMethodEnum>(pAssetData.SettlementMethod);
            }
            dp["DTFILE"].Value = m_dtFile;
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //dp["EXCHANGEACRONYM"].Value = EUREX_EXCHANGEACRONYM;
            dp["EXCHANGEACRONYM"].Value = ExchangeAcronym;
            dp["CONTRACTSYMBOL"].Value = pAssetData.ContractSymbol;
            dp["CATEGORY"].Value = pAssetData.ContractCategory;
            dp["MATURITYMONTHYEAR"].Value = pAssetData.MaturityMonthYear;

            if (isUseContractAttrib)
            {
                dp["CONTRACTATTRIBUTE"].Value = pAssetData.ContractAttribute;
            }

            // PM 20180219 [23824] Utilisation de ExecuteReader à la place de ExecuteDataTable car pas besoin de cache (sinon OutOfMemory)
            //using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(cs), queryParameters.query, queryParameters.parameters.GetArrayDbParameter()).CreateDataReader())
            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, queryParameters.query, queryParameters.parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    ret = IntFunc.IntValue(dr["IDASSET"].ToString());
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAE_H qui correspond à l'expiration {pRowExpiration} associée au produit {pProductId}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAE_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdProduct">id non significatif du produit</param>
        /// <param name="pProductId">symbol du produit</param>
        /// <param name="pParsingRowExpiration">Représente le parsing d'une ligne de type E</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowProductExpiration(int pIdProduct, string pProductId, IOTaskDetInOutFileRow pParsingRowExpiration)
        {
            //PM 20140612 [19911] En Release 2.0, l'échéance n'est pas forcement la date d'échéance : utilisation de Contract Year/Month au lieu de Expiration Year/Month
            //string year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            //string month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            string year = pParsingRowExpiration.GetRowDataValue(cs, "Contract Year");
            string month = pParsingRowExpiration.GetRowDataValue(cs, "Contract Month");
            string day = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Day");
            string daysToExpiry = pParsingRowExpiration.GetRowDataValue(cs, "Days to Expiry");

            // PM 20161019 [22174] Prisma 5.0 : Attention Contract Year et Contract Month ne sont pas forcement alimentés pour les contrat Flex
            if (StrFunc.IsEmpty(year))
            {
                year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            }
            if (StrFunc.IsEmpty(month))
            {
                month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            }

            int iYear = IntFunc.IntValue(year);
            int iMonth = IntFunc.IntValue(month);
            int iDay = IntFunc.IntValue(day);
            int iDaysToExpiry = IntFunc.IntValue(daysToExpiry);

            string queyAdapter = null;
            DataTable dt = GetTableExpiration(CSTools.SetCacheOn(cs,1,null), pIdProduct, iYear, iMonth, iDay, out queyAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAP_H"] = pIdProduct;
                dr["YEAR"] = iYear;
                dr["MONTH"] = iMonth;
                dr["DAY"] = iDay;
                dr["DAYTOEXPIRY"] = iDaysToExpiry;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queyAdapter, dt);
                dt = GetTableExpiration(CSTools.SetCacheOn(cs, 1, null), pIdProduct, iYear, iMonth, iDay, out queyAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAE_H is not loaded for Product:{0} Year:{1} Month:{2} Day:{3}", pProductId, year, month, day));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdProduct"></param>
        /// <param name="pYear"></param>
        /// <param name="pMonth"></param>
        /// <param name="pDay"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableExpiration(string pCS, int pIdProduct, int pYear, int pMonth, int pDay, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAP_H", DbType.Int32), pIdProduct);
            dataParameters.Add(new DataParameter(pCS, "YEAR", DbType.Int32), pYear);
            dataParameters.Add(new DataParameter(pCS, "MONTH", DbType.Int32), pMonth);
            dataParameters.Add(new DataParameter(pCS, "DAY", DbType.Int32), pDay);

            string sqlQuery = "select * from dbo.IMPRISMAE_H where IDIMPRISMAP_H=@IDIMPRISMAP_H and YEAR=@YEAR and MONTH=@MONTH and DAY=@DAY";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAS_H qui correspond à une serie
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdExpiration">Id non significatif de l'échéance de la série</param>
        /// <param name="pIdAsset">Id non significatif de l'asset dans Spheres</param>
        /// <param name="pParsingRowSerie">Représente le parsing d'une ligne de type S</param>
        /// <param name="pParsingRowNeutralScenario">Représente le parsing d'une ligne de type N (contient le prix de l'asset)</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowSerie(int pIdExpiration, int pIdAsset, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowNeutralScenario)
        {
            // ROW SERIE
            string callPut = pParsingRowSerie.GetRowDataValue(cs, "Call Put Flag");
            string strike = pParsingRowSerie.GetRowDataValue(cs, "Exercise Price");
            string version = pParsingRowSerie.GetRowDataValue(cs, "Series Version Number");
            string timeToExpiryBucketID = pParsingRowSerie.GetRowDataValue(cs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowSerie.GetRowDataValue(cs, "Moneyness Bucket ID");
            string riskBucket = pParsingRowSerie.GetRowDataValue(cs, "Risk Bucket");
            string status = pParsingRowSerie.GetRowDataValue(cs, "Series Status");
            string tradingUnit = pParsingRowSerie.GetRowDataValue(cs, "Trading Unit");
            string vega = pParsingRowSerie.GetRowDataValue(cs, "Option Vega");
            string volatility = pParsingRowSerie.GetRowDataValue(cs, "Implied Volatility");
            string interestRate = pParsingRowSerie.GetRowDataValue(cs, "Interest Rate");
            string flexProductId = pParsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
            string settlementType = pParsingRowSerie.GetRowDataValue(cs, "Settlement Type");
            string seriesExerciseStyleFlag = pParsingRowSerie.GetRowDataValue(cs, "Series Exercise Style Flag");
            string flexSeriesFlag = pParsingRowSerie.GetRowDataValue(cs, "Flex Series Flag");

            Boolean isFlex = BoolFunc.IsTrue(flexSeriesFlag);
            decimal dStrike = Decimal.Zero;
            if (StrFunc.IsFilled(callPut))
                dStrike = DecFunc.DecValueFromInvariantCulture(strike);
            decimal dTradingUnit = DecFunc.DecValueFromInvariantCulture(tradingUnit);

            decimal dVega = DecFunc.DecValueFromInvariantCulture(vega);
            decimal dVolatility = DecFunc.DecValueFromInvariantCulture(volatility);
            decimal dInterestRate = DecFunc.DecValueFromInvariantCulture(interestRate);

            // ROW NEUTRAL SCENARIO
            string price = pParsingRowNeutralScenario.GetRowDataValue(cs, "Theorical price for neutral scenario");
            decimal dPrice = DecFunc.DecValueFromInvariantCulture(price);

            string queryAdapter = null;
            DataTable dt = GetTableSeries(CSTools.SetCacheOn(cs,1,null), pIdExpiration, callPut, dStrike, version, settlementType, seriesExerciseStyleFlag, isFlex, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAE_H"] = pIdExpiration;
                dr["PUTCALL"] = StrFunc.IsFilled(callPut) ? callPut : Convert.DBNull;
                dr["STRIKEPRICE"] = StrFunc.IsFilled(callPut) ? dStrike : Convert.DBNull;
                dr["VERSION"] = version;
                dr["IDASSET"] = pIdAsset;
                dr["ASSETCATEGORY"] = Cst.UnderlyingAsset.ExchangeTradedContract.ToString();
                dr["NPRICE"] = dPrice;
                dr["TTEBUCKETID"] = StrFunc.IsFilled(timeToExpiryBucketID) ? timeToExpiryBucketID : Convert.DBNull;
                dr["MONEYNESSBUCKETID"] = StrFunc.IsFilled(moneynessBucketID) ? moneynessBucketID : Convert.DBNull;
                dr["RISKBUCKET"] = StrFunc.IsFilled(riskBucket) ? riskBucket : Convert.DBNull;
                dr["STATUS"] = status;
                dr["TU"] = dTradingUnit;
                dr["DELTA"] = Convert.DBNull;
                dr["VEGA"] = dVega;
                dr["VOLATILITY"] = dVolatility;
                dr["INTERESTRATE"] = dInterestRate;
                dr["FLEXPRODUCTID"] = StrFunc.IsFilled(flexProductId) ? flexProductId : Convert.DBNull;
                dr["SETTLTMETHOD"] = settlementType;
                dr["EXERCISESTYLE"] = StrFunc.IsFilled(seriesExerciseStyleFlag) ? seriesExerciseStyleFlag : Convert.DBNull;
                //dr["ISFLEX"] = isFlex;
                dr["ISFLEX"] = OTCmlHelper.GetADONetBoolValue(cs, isFlex.ToString());  //OTCmlHelper.GetADONetBoolValue for oracle  
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableSeries(CSTools.SetCacheOn(cs, 1, null), pIdExpiration, callPut, dStrike, version, settlementType, seriesExerciseStyleFlag, isFlex, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAS_H is not loaded for asset (IdAsset):{0}", pIdAsset));
            }

            return dt.Rows[0];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdExpiration"></param>
        /// <param name="pPutCall"></param>
        /// <param name="pStrikePrice"></param>
        /// <param name="pVersion"></param>
        /// <param name="pSettlementType"></param>
        /// <param name="pExerciseStyle"></param>
        /// <param name="pIsFlex"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableSeries(string pCS, int pIdExpiration, string pPutCall, Decimal pStrikePrice, String pVersion, string pSettlementType, string pExerciseStyle, bool pIsFlex, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAE_H", DbType.Int32), pIdExpiration);
            dataParameters.Add(new DataParameter(pCS, "VERSION", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pVersion);
            dataParameters.Add(new DataParameter(pCS, "ISFLEX", DbType.Boolean), pIsFlex);
            dataParameters.Add(new DataParameter(pCS, "SETTLTMETHOD", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pSettlementType);

            if (StrFunc.IsFilled(pPutCall))
            {
                dataParameters.Add(new DataParameter(pCS, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pPutCall);
                dataParameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal), pStrikePrice);
                dataParameters.Add(new DataParameter(pCS, "EXERCISESTYLE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pExerciseStyle);
            }

            string sqlQuery = @"
select * from dbo.IMPRISMAS_H 
where (IDIMPRISMAE_H=@IDIMPRISMAE_H )
and (VERSION=@VERSION) and (ISFLEX=@ISFLEX) and (SETTLTMETHOD=@SETTLTMETHOD)
and (PUTCALL=@PUTCALL) and (STRIKEPRICE=@STRIKEPRICE) and (EXERCISESTYLE=@EXERCISESTYLE)";
            if (StrFunc.IsEmpty(pPutCall))
            {
                sqlQuery = sqlQuery.Replace("PUTCALL=@PUTCALL", "PUTCALL is null");
                sqlQuery = sqlQuery.Replace("STRIKEPRICE=@STRIKEPRICE", "STRIKEPRICE is null");
                sqlQuery = sqlQuery.Replace("EXERCISESTYLE=@EXERCISESTYLE", "EXERCISESTYLE is null");
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne un enregistrement dans IMPRISMALGSS_H qui correspond au LGS of serie
        /// <para>Alimente IMPRISMALGSS_H si l'enregistrement n'existe pas</para>
        /// </summary>
        /// <param name="pIdSerie">id non significatif d'une serie</param>
        /// <param name="pIdLiquidationGroupSplit">id non significatif d'un Liquidation Group Split</param>
        /// <param name="pParsingRowLH">Représente le parsing d'une ligne type LH</param>
        /// <param name="pParsingRowLiquidationGroupSplit">Représente le parsing d'une ligne type LGS (nécessairement null si Release 1)</param>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroupSplitOfSerie(int pIdSerie, int pIdLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowLH, IOTaskDetInOutFileRow pParsingRowLiquidationGroupSplit)
        {
            string liquidationHorizon = pParsingRowLH.GetRowDataValue(cs, "Liquidation Horizon");
            int iLiquidationHorizon = Convert.ToInt32(liquidationHorizon);

            string defaultLGSIndicator = "Y";
            if (null != pParsingRowLiquidationGroupSplit)
                defaultLGSIndicator = pParsingRowLiquidationGroupSplit.GetRowDataValue(cs, "Default LGS Indicator");

            string queryAdapter = null;
            DataTable dt = GetTableLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(cs, 1, null), pIdSerie, pIdLiquidationGroupSplit, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAS_H"] = pIdSerie;
                dr["IDIMPRISMALGS_H"] = pIdLiquidationGroupSplit;
                dr["LH"] = iLiquidationHorizon;
                //dr["ISDEFAULT"] = BoolFunc.IsTrue(defaultLGSIndicator) 
                dr["ISDEFAULT"] = OTCmlHelper.GetADONetBoolValue(cs, defaultLGSIndicator);  //OTCmlHelper.GetADONetBoolValue for oracle  
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(cs, 1, null), pIdSerie, pIdLiquidationGroupSplit, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMALGSS_H is not loaded for serie (IdSerie):{0} and liquidation group Split:{1}", pIdSerie, pIdLiquidationGroupSplit));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdSerie"></param>
        /// <param name="pIdLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroupSplitOfSerie(string pCS, int pIdSerie, int pIdLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAS_H", DbType.Int32), pIdSerie);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALGS_H", DbType.Int32), pIdLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMALGSS_H where IDIMPRISMAS_H=@IDIMPRISMAS_H and IDIMPRISMALGS_H=@IDIMPRISMALGS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMARMS_H qui correspond au Risk Measure Set {pRowRMS}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMARMS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowRiskMeasuresSet(IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string identifier = string.Empty;
            switch (inputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                    identifier = pParsingRowRMS.GetRowDataValue(cs, "Risk Measure Set");
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    identifier = pParsingRowRMS.GetRowDataValue(cs, "Risk Measure Set ID");
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dataStyle :{0} is not implemented", dataStyle));
            }

            string queryAdapter = null;
            DataTable dt = GetTableRiskMeasuresSet(CSTools.SetCacheOn(cs,1,null), identifier, idIMPRISMA_H, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                dr["IDENTIFIER"] = identifier;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSet(CSTools.SetCacheOn(cs, 1, null), identifier, idIMPRISMA_H, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMS_H is not loaded for identifier:{0}", identifier));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSet(string pCS, string pIdentifier, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);

            string sqlQuery = "select * from dbo.IMPRISMARMS_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        ///  Retourne l'enregsitrement dans IMPRISMARMSLGS_H qui correspond au RMS {pIdRiskMeasureSet} rattaché au LGS {pIdLiquidationGroupSplit}
        ///  <para>Alimente IMPRISMARMSLGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroupSplit">Id non significatif d'un LGS</param>
        /// <param name="pIdRiskMeasureSet">Id non significatif d'un RMS (Risk Measure Set)</param>
        /// <param name="pParsingRowRMS">Parsing d'une ligne de type RMS</param>
        /// <returns></returns>
        private DataRow LoadRiskMeasuresSetOfLiquidationGroupSplit(int pIdLiquidationGroupSplit, int pIdRiskMeasureSet, IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string queryAdapter = null;
            DataTable dt = GetTableRiskMeasuresSetOfLiquidationGroupSplit(CSTools.SetCacheOn(cs,1,null), pIdLiquidationGroupSplit, pIdRiskMeasureSet, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMALGS_H"] = pIdLiquidationGroupSplit;
                dr["IDIMPRISMARMS_H"] = pIdRiskMeasureSet;
                dr["ISUSEROBUSTNESS"] = OTCmlHelper.GetADONetBoolValue(cs, "false"); // Valeur par défaut de la table , nécessaire pour Oracle 
                dr["ISCORRELATIONBREAK"] = OTCmlHelper.GetADONetBoolValue(cs, "false"); // Valeur par défaut de la table , nécessaire pour Oracle 
                dr["ISLIQUIDCOMPONENT"] = OTCmlHelper.GetADONetBoolValue(cs, "false"); // Valeur par défaut de la table, nécessaire pour Oracle 
                SetDataRowRMSLGS(cs, dr, pParsingRowRMS, inputSourceDataStyle);
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSetOfLiquidationGroupSplit(CSTools.SetCacheOn(cs, 1, null), pIdLiquidationGroupSplit, pIdRiskMeasureSet, out queryAdapter);
            }
            else if (Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE != inputSourceDataStyle)
            {
                DataRow dr = dt.Rows[0];
                SetDataRowRMSLGS(cs, dr, pParsingRowRMS, inputSourceDataStyle);
                //SetDataRowUpd(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                DataHelper.queryCache.Remove("IMPRISMARMSLGS_H", cs);   
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMSLGS_H is not loaded for LGS:{0} and RMS:{0}", pIdLiquidationGroupSplit, pIdRiskMeasureSet));
            }

            return dt.Rows[0];
        }

        /// <summary>
        ///  Aliemnte le datarow {pDataRow} avec les informations présentes dans une ligne RMS
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pDataRow">DataRow mis à jour</param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <param name="pDataStyle">Type de fichier source</param>
        private static void SetDataRowRMSLGS(string cs, DataRow pDataRow, IOTaskDetInOutFileRow pParsingRowRMS, Cst.InputSourceDataStyle pDataStyle)
        {
            switch (pDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    string historicalStessed = pParsingRowRMS.GetRowDataValue(cs, "Historical / Stressed");
                    string riskMeasure = pParsingRowRMS.GetRowDataValue(cs, "Risk Measure");
                    string anchorConfidenceLevel = pParsingRowRMS.GetRowDataValue(cs, "Anchor Confidence Level");
                    string robustness = pParsingRowRMS.GetRowDataValue(cs, "Robustness");
                    string scallingFactor = pParsingRowRMS.GetRowDataValue(cs, "Scaling Factor");
                    string correlationBreakFlag = pParsingRowRMS.GetRowDataValue(cs, "Correlation Break Flag");
                    string movingSubWindow = pParsingRowRMS.GetRowDataValue(cs, "Moving Sub-Window");
                    string confidenceLevelCorrelationBreak = pParsingRowRMS.GetRowDataValue(cs, "Confidence Level Correlation Break");
                    string cap = pParsingRowRMS.GetRowDataValue(cs, "Cap");
                    string floor = pParsingRowRMS.GetRowDataValue(cs, "Floor");
                    string multiplier = pParsingRowRMS.GetRowDataValue(cs, "Multiplier");
                    string liquidityRiskComponent = pParsingRowRMS.GetRowDataValue(cs, "Liquidity Risk Component");
                    string confidenceLevelDiversificationFactor = pParsingRowRMS.GetRowDataValue(cs, "Confidence Level Diversification Factor");
                    string alphaFloor = pParsingRowRMS.GetRowDataValue(cs, "Alpha Floor");
                    // PM 20161019 [22174] Prisma 5.0 : Ajout nbWorstScenario
                    string nbWorstScenario = pParsingRowRMS.GetRowDataValue(cs, "NoWorstScenarios");

                    pDataRow["HISTORICALSTRESSED"] = historicalStessed;
                    pDataRow["RISKMEASURE"] = riskMeasure;
                    pDataRow["CONFIDENCELEVEL"] = Convert.ToDecimal(anchorConfidenceLevel);
                    //Robustness
                    //pDataRow["ISUSEROBUSTNESS"] = BoolFunc.IsTrue(robustness) 
                    pDataRow["ISUSEROBUSTNESS"] = OTCmlHelper.GetADONetBoolValue(cs, robustness); //OTCmlHelper.GetADONetBoolValue for oracle  
                    pDataRow["SCALINGFACTOR"] = Convert.ToDecimal(scallingFactor);
                    // Correlation Break
                    //pDataRow["ISCORRELATIONBREAK"] = BoolFunc.IsTrue(correlationBreakFlag);
                    pDataRow["ISCORRELATIONBREAK"] = OTCmlHelper.GetADONetBoolValue(cs, correlationBreakFlag); //OTCmlHelper.GetADONetBoolValue for oracle  
                    if (StrFunc.IsFilled(movingSubWindow))
                    {
                        pDataRow["CBSUBWINDOW"] = Convert.ToInt32(movingSubWindow);
                    }
                    if (StrFunc.IsFilled(confidenceLevelCorrelationBreak))
                    {
                        pDataRow["CBCONFIDENCELEVEL"] = Convert.ToDecimal(confidenceLevelCorrelationBreak);
                    }
                    if (StrFunc.IsFilled(cap))
                    {
                        pDataRow["CBCAP"] = Convert.ToDecimal(cap);
                    }
                    if (StrFunc.IsFilled(floor))
                    {
                        pDataRow["CBFLOOR"] = Convert.ToDecimal(floor);
                    }
                    if (StrFunc.IsFilled(multiplier))
                    {
                        pDataRow["CBMULTIPLIER"] = Convert.ToDecimal(multiplier);
                    }
                    // Liquidity Risk
                    //pDataRow["ISLIQUIDCOMPONENT"] = BoolFunc.IsTrue(liquidityRiskComponent);
                    pDataRow["ISLIQUIDCOMPONENT"] = OTCmlHelper.GetADONetBoolValue(cs, liquidityRiskComponent); //OTCmlHelper.GetADONetBoolValue for oracle  
                    if (StrFunc.IsFilled(confidenceLevelDiversificationFactor))
                    {
                        pDataRow["DFACONFIDENCELEVEL"] = Convert.ToDecimal(confidenceLevelDiversificationFactor);
                    }
                    if (StrFunc.IsFilled(alphaFloor))
                    {
                        pDataRow["DFAFLOOR"] = Convert.ToDecimal(alphaFloor);
                    }
                    // PM 20161019 [22174] Prisma 5.0 : Ajout nbWorstScenario
                    if (StrFunc.IsFilled(nbWorstScenario))
                    {
                        pDataRow["NBWORSTSCENARIO"] = Convert.ToInt32(nbWorstScenario);
                    }
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    string weightingFactor = pParsingRowRMS.GetRowDataValue(cs, "Weighting Factor");
                    string aggregationMethod = pParsingRowRMS.GetRowDataValue(cs, "Aggregation Method");

                    pDataRow["WEIGHTINGFACTOR"] = Convert.ToDecimal(weightingFactor);
                    pDataRow["AGGREGATIONMETHOD"] = Convert.ToString(aggregationMethod);
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    // NOTHING TO DO
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Data Style:{0} is not implemented", pDataStyle));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLiquidationGroupSplit"></param>
        /// <param name="pIdRiskMeasureSet"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSetOfLiquidationGroupSplit(string pCS, int pIdLiquidationGroupSplit, int pIdRiskMeasureSet, out string pQuery)
        {

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALGS_H", DbType.Int32), pIdLiquidationGroupSplit);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMARMS_H", DbType.Int32), pIdRiskMeasureSet);

            string sqlQuery = "select * from dbo.IMPRISMARMSLGS_H where IDIMPRISMALGS_H=@IDIMPRISMALGS_H and IDIMPRISMARMS_H=@IDIMPRISMARMS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAFX_H qui correspond au FX Set {pRowFX}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAFX_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowFX">Représente le parsing d'une ligne de type FX</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowFX(IOTaskDetInOutFileRow pParsingRowFX)
        {
            string identifier = pParsingRowFX.GetRowDataValue(cs, "FX Set");

            string queryAdapter = null;
            DataTable dt = GetTableFX(CSTools.SetCacheOn(cs,1,null), identifier, idIMPRISMA_H, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                dr["IDENTIFIER"] = identifier;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableFX(CSTools.SetCacheOn(cs, 1, null), identifier, idIMPRISMA_H, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAFX_H is not loaded for identifier:{0}", identifier));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Lecture de la table IMPRISMAFX_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableFX(string pCS, string pIdentifier, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);

            string sqlQuery = "select * from dbo.IMPRISMAFX_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAFXPAIR_H qui correspond au FX Set {pRowFX} et au currencyPair
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAFXPAIR_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdPrismaFx_H">Identifiant non significatif du Fx Set</param>
        /// <param name="pFxSetIdentifier">Identifiant significatif du Fx Set</param>
        /// <param name="pParsingRowFxPair">Représente le parsing d'une ligne de type P (Currency Pair)</param>
        /// <param name="pParsingRowCurrentExchangeRate">Représente le parsing d'une ligne de type C (Current Exchange Rate)</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowFxPair(int pIdPrismaFx_H, string pFxSetIdentifier, IOTaskDetInOutFileRow pParsingRowFxPair, IOTaskDetInOutFileRow pParsingRowCurrentExchangeRate)
        {
            string currencyPair = pParsingRowFxPair.GetRowDataValue(cs, "Currency pair");
            string currentExchangeRate = pParsingRowCurrentExchangeRate.GetRowDataValue(cs, "Current Exchange Rate");

            decimal dCurrentExchangeRate = DecFunc.DecValueFromInvariantCulture(currentExchangeRate);

            string queryAdapter = null;
            DataTable dt = GetTableFxPair(cs, pIdPrismaFx_H, currencyPair, out queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAFX_H"] = pIdPrismaFx_H;
                dr["CURRENCYPAIR"] = currencyPair;
                dr["EXCHANGERATE"] = dCurrentExchangeRate;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableFxPair(cs, pIdPrismaFx_H, currencyPair, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAFXPAIR_H is not loaded for Fx Set:{0], Currency pair:{1}", currencyPair));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMPRISMAFX_H"></param>
        /// <param name="pCurrencyPair"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableFxPair(string pCS, int pIdIMPRISMAFX_H, string pCurrencyPair, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAFX_H", DbType.Int32), pIdIMPRISMAFX_H);
            dataParameters.Add(new DataParameter(pCS, "CURRENCYPAIR", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pCurrencyPair);

            string sqlQuery = "select * from dbo.IMPRISMAFXPAIR_H where IDIMPRISMAFX_H=@IDIMPRISMAFX_H and CURRENCYPAIR=@CURRENCYPAIR";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregsitrement dans IMPRISMARMSLGSS_H (RMS of LGS of Serie)
        /// <para>Alimente la table IMPRISMARMSLGSS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdSerie">Représente une serie</param>
        /// <param name="pIdRiskMeasureSetOfLiquidationGroupSplit"></param>
        /// <param name="pIdFxRate">Représente les cours de change appliqués au couple serie/RMSofLGS</param>
        /// <param name="pParsingRowLH">Représente le parsing d'une ligne type LH</param>
        /// <returns></returns>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout pParsingRowLH
        private DataRow LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(int pIdSerie, int pIdRiskMeasureSetOfLiquidationGroupSplit, int pIdFxRate, IOTaskDetInOutFileRow pParsingRowLH)
        {
            string queryAdapter = null;
            DataTable dt = GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(cs,1,null) , pIdSerie, pIdRiskMeasureSetOfLiquidationGroupSplit, out queryAdapter);
            // PM 20161019 [22174] Prisma 5.0 : Ajout Liquidation Horizon
            //if (dt.Rows.Count == 0)
            if ((0 ==dt.Rows.Count) && (default(IOTaskDetInOutFileRow) != pParsingRowLH))
                {
                string liquidationHorizon = pParsingRowLH.GetRowDataValue(cs, "Liquidation Horizon");
                int iLiquidationHorizon = Convert.ToInt32(liquidationHorizon);

                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAS_H"] = pIdSerie;
                dr["IDIMPRISMARMSLGS_H"] = pIdRiskMeasureSetOfLiquidationGroupSplit;
                dr["IDIMPRISMAFX_H"] = pIdFxRate;
                dr["LH"] = iLiquidationHorizon;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(cs, 1, null), pIdSerie, pIdRiskMeasureSetOfLiquidationGroupSplit, out queryAdapter);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMSLGSS_H is not loaded for identifier"));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdSerie"></param>
        /// <param name="pIdRiskMeasureSetOfLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(string pCS, int pIdSerie, int pIdRiskMeasureSetOfLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAS_H", DbType.Int32), pIdSerie);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMARMSLGS_H", DbType.Int32), pIdRiskMeasureSetOfLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMARMSLGSS_H where IDIMPRISMAS_H=@IDIMPRISMAS_H and IDIMPRISMARMSLGS_H=@IDIMPRISMARMSLGS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        ///  Insertion des prix des n scenarios d'une serie pour un RMS
        /// </summary>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="pLH">Liquidation Horizon</param>
        private void AddSPValues(IOTaskDetInOutFileRow pParsingRowSP, int pIdRMSLGSS, int pLH)
        {
            string sql = BuildInsertSPValues(cs, pIdRMSLGSS, pParsingRowSP, pLH);
            DataHelper.ExecuteNonQuery(cs, CommandType.Text, sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParsingRowSP"></param>
        /// <param name="pIdRMSLGSS"></param>
        /// <param name="pLH"></param>
        /// FI 20140430 [XXXXX] New methode
        private void AddSPValues2(IOTaskDetInOutFileRow pParsingRowSP, int pIdRMSLGSS, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(cs, "Scenarios Prices").Split(';');

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(cs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                    DataHelper.ExecuteNonQuery(cs, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                i += pLH;
            }
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertSPValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StrBuilder finalQuery = new StrBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                    finalQuery += qry.GetQueryReplaceParameters(false);
                i += pLH;
            }

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectSPValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StringBuilder finalQuery = new StringBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQuerySelectSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                {
                    if (finalQuery.Length > 0)
                        finalQuery.Append(SQLCst.UNIONALL + Cst.CrLf);
                    finalQuery.Append(qry.GetQueryReplaceParameters(false));
                }
                i += pLH;
            }

            finalQuery.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf);
            finalQuery.Append(";");

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <param name="pNeutralPrice">Neutral Price</param>
        /// <returns></returns>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout de pNeutralPrice
        private static string BuildSelectSPValues2(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH, decimal pNeutralPrice)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StringBuilder finalQuery = new StringBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQuerySelectSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        // PM 20161019 [22174] Prisma 5.0 : Prendre le Neutral Price lorsque la valeur de SP n'est pas renseignée
                        //qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]); 
                        if (StrFunc.IsEmpty(spValues[i + j]))
                        {
                            qry.parameters[col].Value = pNeutralPrice;
                        }
                        else
                        {
                            qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                        }
                    }
                }
                if (isAddQuery)
                {
                    if (finalQuery.Length > 0)
                    {
                        finalQuery.Append(SQLCst.UNIONALL + Cst.CrLf);
                    }
                    finalQuery.Append(qry.GetQueryReplaceParameters(false));
                }
                i += pLH;
            }

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private QueryParameters BuildInsertSPValues2(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            QueryParameters ret = null;

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                {
                    ret = qry;
                }
                i += pLH;
            }

            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMASPS_H (prix des series pour les différents scenarios) 
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="pScenarionNb">Numéro de scenario</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertSPValues(string pCs, int pIdRMSLGSS, int pScenarionNb)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            for (int i = 1; i <= 5; i++)
            {
                dp.Add(new DataParameter(pCs, "PRICE" + i.ToString(), DbType.Decimal), Convert.DBNull);
            }
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), pScenarionNb);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @SCENARIONO,@PRICE1,@PRICE2,@PRICE3,@PRICE4,@PRICE5);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMASPS_H (prix des series pour les différents scenarios) 
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="pScenarionNb">Numéro de scenario</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectSPValues(string pCs, int pIdRMSLGSS, int pScenarionNb)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            for (int i = 1; i <= 5; i++)
            {
                dp.Add(new DataParameter(pCs, "PRICE" + i.ToString(), DbType.Decimal), Convert.DBNull);
            }
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), pScenarionNb);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMARMSLGSS_H,@SCENARIONO,@PRICE1,@PRICE2,@PRICE3,@PRICE4,@PRICE5" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);   

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Insertion des compressions Error associé à une devise
        /// </summary>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="pLH">Liquidation Horizon</param>
        private void AddCEValues(IOTaskDetInOutFileRow pParsingRowCE, int pIdRMSLGSS, int pLH)
        {
            string sql = BuildInsertCEValues(cs, pIdRMSLGSS, pParsingRowCE, pLH);
            DataHelper.ExecuteNonQuery(cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertCEValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowCE, int pLH)
        {
            string[] spValues = pParsingRowCE.GetRowDataValue(pCs, "Compression Error/Currency").Split(';');
            string idC = spValues[ArrFunc.Count(spValues) - 1];

            QueryParameters qry = GetQueryInsertCEValues(pCs, pIdRMSLGSS);
            for (int j = 0; j < pLH; j++)
            {
                string col = "CE" + (j + 1).ToString();
                qry.parameters[col].Value = Convert.ToDecimal(spValues[j]);
            }
            qry.parameters["IDC"].Value = idC;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectCEValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowCE, int pLH)
        {
            string[] spValues = pParsingRowCE.GetRowDataValue(pCs, "Compression Error/Currency").Split(';');
            string idC = spValues[ArrFunc.Count(spValues) - 1];

            QueryParameters qry = GetQuerySelectCEValues(pCs, pIdRMSLGSS);
            for (int j = 0; j < pLH; j++)
            {
                string col = "CE" + (j + 1).ToString();
                qry.parameters[col].Value = Convert.ToDecimal(spValues[j]);
            }
            qry.parameters["IDC"].Value = idC;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMACES_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertCEValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            for (int i = 1; i <= 5; i++)
                dp.Add(new DataParameter(pCs, "CE" + i.ToString(), DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMACES_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, IDC, CE1, CE2, CE3, CE4, CE5)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @IDC,@CE1,@CE2,@CE3,@CE4,@CE5);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMACES_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectCEValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            for (int i = 1; i <= 5; i++)
                dp.Add(new DataParameter(pCs, "CE" + i.ToString(), DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMARMSLGSS_H,@IDC,@CE1,@CE2,@CE3,@CE4,@CE5" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);   

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// </summary>
        /// <param name="pParsingRowVar">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        private void AddVaRValues(IOTaskDetInOutFileRow pParsingRowVar, int pIdRMSLGSS)
        {
            string sql = BuildInsertVaRValues(cs, pIdRMSLGSS, pParsingRowVar);
            DataHelper.ExecuteNonQuery(cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowVaR">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertVaRValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowVaR)
        {
            string IVAR = pParsingRowVaR.GetRowDataValue(pCs, "IVAR");
            string AIVAR = pParsingRowVaR.GetRowDataValue(pCs, "AIVAR");
            string varType = StrFunc.IsFilled(IVAR) ? IVAR : AIVAR;

            string idC = pParsingRowVaR.GetRowDataValue(pCs, "Currency");
            string longShortIndicator = pParsingRowVaR.GetRowDataValue(pCs, "Long Short Indicator");

            string amount = string.Empty;
            if (varType == "IVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Instrument VaR");
            else if (varType == "AIVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Additional Instrument VaR");
            decimal dAmount = DecFunc.DecValueFromInvariantCulture(amount);

            QueryParameters qry = GetQueryInsertVaRValues(pCs, pIdRMSLGSS);
            qry.parameters["IDC"].Value = idC;
            qry.parameters["SHORTLONG"].Value = longShortIndicator;
            qry.parameters["VARTYPE"].Value = varType;
            qry.parameters["VARAMOUNT"].Value = dAmount;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowVaR">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectVaRValues(string pCs, int pIdRMSLGSS,  IOTaskDetInOutFileRow pParsingRowVaR)
        {
            string IVAR = pParsingRowVaR.GetRowDataValue(pCs, "IVAR");
            string AIVAR = pParsingRowVaR.GetRowDataValue(pCs, "AIVAR");
            string varType = StrFunc.IsFilled(IVAR) ? IVAR : AIVAR;

            string idC = pParsingRowVaR.GetRowDataValue(pCs, "Currency");
            string longShortIndicator = pParsingRowVaR.GetRowDataValue(pCs, "Long Short Indicator");

            string amount = string.Empty;
            if (varType == "IVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Instrument VaR");
            else if (varType == "AIVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Additional Instrument VaR");
            decimal dAmount = DecFunc.DecValueFromInvariantCulture(amount);

            QueryParameters qry = GetQuerySelectVaRValues(pCs, pIdRMSLGSS);
            qry.parameters["IDC"].Value = idC;
            qry.parameters["SHORTLONG"].Value = longShortIndicator;
            qry.parameters["VARTYPE"].Value = varType;
            qry.parameters["VARAMOUNT"].Value = dAmount;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAVARS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertVaRValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARAMOUNT", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SHORTLONG", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAVARS_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, IDC, SHORTLONG, VARTYPE, VARAMOUNT)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @IDC,@SHORTLONG,@VARTYPE,@VARAMOUNT);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAVARS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectVaRValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARAMOUNT", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SHORTLONG", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT) + Cst.CrLf;
            sql += "@IDIMPRISMARMSLGSS_H,@IDC,@SHORTLONG,@VARTYPE,@VARAMOUNT" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);   

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMALIQFACT_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de la classe de Liquidation</param>
        /// <param name="pParsingRowLiquidityFactor">Représente le parsing d'une ligne du fichier Liquidity Factors Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildInsertLiquidityFactors(string pCs, int pIdLiquidityClass, IOTaskDetInOutFileRow pParsingRowLiquidityFactor)
        {
            string minPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Minimum Percentage Threshold");
            string maxPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Maximum Percentage Threshold");
            string liquidityFactorMinThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Minimum Threshold");
            string liquidityFactorMaxThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Maximum Threshold");

            QueryParameters qry = GetQueryInsertLiquidityFactorsValues(pCs, pIdLiquidityClass);
            qry.parameters["PCTMINTHRESHOLD"].Value = Convert.ToDecimal(minPercentThreshold);
            if (StrFunc.IsFilled(maxPercentThreshold))
            {
                qry.parameters["PCTMAXTHRESHOLD"].Value = Convert.ToDecimal(maxPercentThreshold);
            }
            qry.parameters["MINTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMinThreshold);
            if (StrFunc.IsFilled(liquidityFactorMaxThreshold))
            {
                qry.parameters["MAXTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMaxThreshold);
            }

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMALIQFACT_H
        ///  <para>Requête de type select </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de la classe de Liquidation</param>
        /// <param name="pParsingRowLiquidityFactor">Représente le parsing d'une ligne du fichier Liquidity Factors Configuration</param>
        /// <returns></returns>
        private static string BuildSelectLiquidityFactors(string pCs, int pIdLiquidityClass, IOTaskDetInOutFileRow pParsingRowLiquidityFactor)
        {
            string minPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Minimum Percentage Threshold");
            string maxPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Maximum Percentage Threshold");
            string liquidityFactorMinThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Minimum Threshold");
            string liquidityFactorMaxThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Maximum Threshold");

            QueryParameters qry = GetQuerySelectLiquidityFactorsValues(pCs, pIdLiquidityClass);
            qry.parameters["PCTMINTHRESHOLD"].Value = Convert.ToDecimal(minPercentThreshold);
            if (StrFunc.IsFilled(maxPercentThreshold))
            {
                qry.parameters["PCTMAXTHRESHOLD"].Value = Convert.ToDecimal(maxPercentThreshold);
            }
            qry.parameters["MINTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMinThreshold);
            if (StrFunc.IsFilled(liquidityFactorMaxThreshold))
            {
                qry.parameters["MAXTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMaxThreshold);
            }

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table IMPRISMALIQFACT_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdLiquidityClass"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertLiquidityFactorsValues(string pCs, int pIdLiquidityClass)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMALIQCLASS_H", DbType.Int32), pIdLiquidityClass);
            dp.Add(new DataParameter(pCs, "PCTMINTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PCTMAXTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MINTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MAXTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H") + Cst.CrLf;
            sql += "(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMALIQCLASS_H, @PCTMINTHRESHOLD, @PCTMAXTHRESHOLD, @MINTHRESHOLDFACTOR, @MAXTHRESHOLDFACTOR);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table IMPRISMALIQFACT_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdLiquidityClass"></param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectLiquidityFactorsValues(string pCs, int pIdLiquidityClass)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMALIQCLASS_H", DbType.Int32), pIdLiquidityClass);
            dp.Add(new DataParameter(pCs, "PCTMINTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PCTMAXTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MINTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MAXTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);

            //StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H") + Cst.CrLf;
            //sql += "(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf;
            //sql += "Values(@IDIMPRISMALIQCLASS_H, @PCTMINTHRESHOLD, @PCTMAXTHRESHOLD, @MINTHRESHOLDFACTOR, @MAXTHRESHOLDFACTOR, @DTINS, @IDAINS);" + Cst.CrLf;

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMALIQCLASS_H,@PCTMINTHRESHOLD,@PCTMAXTHRESHOLD,@MINTHRESHOLDFACTOR,@MAXTHRESHOLDFACTOR" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);    
            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="pParsingRowMarketCapacities">Représente le parsing d'une ligne du fichier Market capacities Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildInsertMarketCapacities(string pCs, int pIdPrismaH, IOTaskDetInOutFileRow pParsingRowMarketCapacities)
        {
            string productLine = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product Line");
            string productId = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product ID");
            string unlIsin = pParsingRowMarketCapacities.GetRowDataValue(pCs, "U/L_ISIN");
            string putCallFlag = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Put Call Flag");
            string timeToExpiryBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Moneyness Bucket ID");
            string marketCapacity = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Market Capacity");
            string liquidityPremium = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Liquidity Premium");

            QueryParameters qry = GetQueryInsertMarketCapacitiesValues(pCs, pIdPrismaH);
            qry.parameters["PRODUCTLINE"].Value = productLine;
            qry.parameters["PRODUCTID"].Value = productId;
            qry.parameters["ISINCODEUNL"].Value = unlIsin;
            if (StrFunc.IsFilled(putCallFlag))
                qry.parameters["PUTCALL"].Value = putCallFlag;
            qry.parameters["TTEBUCKETID"].Value = timeToExpiryBucketID;
            if (StrFunc.IsFilled(moneynessBucketID))
                qry.parameters["MONEYNESSBUCKETID"].Value = moneynessBucketID;
            qry.parameters["MARKETCAPACITY"].Value = Convert.ToDecimal(marketCapacity);
            qry.parameters["LIQUIDITYPREMIUM"].Value = Convert.ToDecimal(liquidityPremium);

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="pParsingRowMarketCapacities">Représente le parsing d'une ligne du fichier Market capacities Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildSelectMarketCapacities(string pCs, int pIdPrismaH, IOTaskDetInOutFileRow pParsingRowMarketCapacities)
        {
            string productLine = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product Line");
            string productId = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product ID");
            string unlIsin = pParsingRowMarketCapacities.GetRowDataValue(pCs, "U/L_ISIN");
            string putCallFlag = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Put Call Flag");
            string timeToExpiryBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Moneyness Bucket ID");
            string marketCapacity = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Market Capacity");
            string liquidityPremium = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Liquidity Premium");

            QueryParameters qry = GetQuerySelectMarketCapacitiesValues(pCs, pIdPrismaH);
            qry.parameters["PRODUCTLINE"].Value = productLine;
            qry.parameters["PRODUCTID"].Value = productId;
            qry.parameters["ISINCODEUNL"].Value = unlIsin;
            if (StrFunc.IsFilled(putCallFlag))
                qry.parameters["PUTCALL"].Value = putCallFlag;
            qry.parameters["TTEBUCKETID"].Value = timeToExpiryBucketID;
            if (StrFunc.IsFilled(moneynessBucketID))
                qry.parameters["MONEYNESSBUCKETID"].Value = moneynessBucketID;
            qry.parameters["MARKETCAPACITY"].Value = Convert.ToDecimal(marketCapacity);
            qry.parameters["LIQUIDITYPREMIUM"].Value = Convert.ToDecimal(liquidityPremium);

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table de la table IMPRISMAMKTCAPA_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertMarketCapacitiesValues(string pCs, int pIdPrismaH)
        {
            DataParameters dp = new DataParameters();

            dp.Add(new DataParameter(pCs, "IDIMPRISMA_H", DbType.Int32), pIdPrismaH);
            dp.Add(new DataParameter(pCs, "PRODUCTLINE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "ISINCODEUNL", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "TTEBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MONEYNESSBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MARKETCAPACITY", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "LIQUIDITYPREMIUM", DbType.Decimal), Convert.DBNull);
            
            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAMKTCAPA_H") + Cst.CrLf;
            sql += "(IDIMPRISMA_H, PRODUCTLINE, PRODUCTID, ISINCODEUNL, PUTCALL, TTEBUCKETID, MONEYNESSBUCKETID, MARKETCAPACITY, LIQUIDITYPREMIUM)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMA_H, @PRODUCTLINE, @PRODUCTID, @ISINCODEUNL, @PUTCALL, @TTEBUCKETID, @MONEYNESSBUCKETID, @MARKETCAPACITY, @LIQUIDITYPREMIUM);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table de la table IMPRISMAMKTCAPA_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectMarketCapacitiesValues(string pCs, int pIdPrismaH)
        {
            DataParameters dp = new DataParameters();

            dp.Add(new DataParameter(pCs, "IDIMPRISMA_H", DbType.Int32), pIdPrismaH);
            dp.Add(new DataParameter(pCs, "PRODUCTLINE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "ISINCODEUNL", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "TTEBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MONEYNESSBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MARKETCAPACITY", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "LIQUIDITYPREMIUM", DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT); 
            sql += "@IDIMPRISMA_H,@PRODUCTLINE,@PRODUCTID,@ISINCODEUNL,@PUTCALL,@TTEBUCKETID,@MONEYNESSBUCKETID,@MARKETCAPACITY,@LIQUIDITYPREMIUM" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);  
            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Insertion des FX Rates 
        /// </summary>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        private void AddFXRatesValues(IOTaskDetInOutFileRow pParsingRowRMS, int pIdFxPair, int pIdRMS)
        {
            string sql = BuildInsertFXRatesValues(cs, pIdFxPair, pIdRMS, pParsingRowRMS);
            DataHelper.ExecuteNonQuery(cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS du fichier Foreign Exchante rates configuration</param>
        /// <returns></returns>
        private static string BuildInsertFXRatesValues(string pCs, int pIdFxPair, int pIdRMS, IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string[] values = pParsingRowRMS.GetRowDataValue(pCs, "Exchange Rate for scenarios").Split(';');

            StrBuilder sql = new StrBuilder();
            string ret = string.Empty;

            for (int i = 0; i < ArrFunc.Count(values); i++)
            {
                QueryParameters qry = GetQueryInsertFXRatesValues(pCs, pIdFxPair, pIdRMS);
                qry.parameters["VALUE"].Value = DecFunc.DecValueFromInvariantCulture(values[i]);
                qry.parameters["SCENARIONO"].Value = i + 1;

                sql += qry.GetQueryReplaceParameters(false);
            }
            ret = sql.ToString();

            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS du fichier Foreign Exchante rates configuration</param>
        /// <returns></returns>
        private static string BuildSelectFXRatesValues(string pCs, int pIdFxPair, int pIdRMS,IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string[] values = pParsingRowRMS.GetRowDataValue(pCs, "Exchange Rate for scenarios").Split(';');

            StrBuilder sql = new StrBuilder();
            string ret = string.Empty;

            for (int i = 0; i < ArrFunc.Count(values); i++)
            {
                QueryParameters qry = GetQuerySelectFXRatesValues(pCs, pIdFxPair, pIdRMS);
                qry.parameters["VALUE"].Value = DecFunc.DecValueFromInvariantCulture(values[i]);
                qry.parameters["SCENARIONO"].Value = i + 1;
                if (sql.Length > 0)
                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                sql += qry.GetQueryReplaceParameters(false);
            }
            ret = sql.ToString();

            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAFXRMS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">Id RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertFXRatesValues(string pCs, int pIdFxPair, int pIdRMS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMAFXPAIR_H", DbType.Int32), pIdFxPair);
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMS_H", DbType.Int32), pIdRMS);
            dp.Add(new DataParameter(pCs, "VALUE", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), Convert.DBNull);
            
            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAFXRMS_H") + Cst.CrLf;
            sql += "(IDIMPRISMAFXPAIR_H, IDIMPRISMARMS_H, SCENARIONO, VALUE)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMAFXPAIR_H, @IDIMPRISMARMS_H, @SCENARIONO,@VALUE);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAFXRMS_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">Id RMS</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectFXRatesValues(string pCs, int pIdFxPair, int pIdRMS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMAFXPAIR_H", DbType.Int32), pIdFxPair);
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMS_H", DbType.Int32), pIdRMS);
            dp.Add(new DataParameter(pCs, "VALUE", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMAFXPAIR_H,@IDIMPRISMARMS_H,@SCENARIONO,@VALUE" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);   

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Initialisation à effectuer lors de l'importation des fichiers PRISMA
        /// <para>Démarrage de l'audit, Ouverture du fichier, Récupération de la date de fichier, Chargement de la table IMPRISMA_H</para>
        /// </summary>
        /// <param name="pInputParsing"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void StartPrismaImport(InputParsing pInputParsing)
        {
            string fileDescription = GetFileDesciption();
            task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat(@"Start importation:{0}", fileDescription));

            // PM 20200102 [XXXXX] New Log
            Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", fileDescription), 3));

            StartAudit();

            InitDirectImport(pInputParsing);

            SetDateFile();

            LoadRowIMPRISMA_H();

            OpenInputFileName();
        }

        /// <summary>
        /// Fin d'importation d'un fichier PRISMA
        /// <para>Fermeture des fichiers, Purge du cache SQL</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void EndPrismaImport()
        {
            CloseAllFiles();
            task.process.ProcessLogAddDetail2(new ProcessLogParameters(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.LEVEL3, 3), StrFunc.AppendFormat(@"End importation"));

            // PM 20200102 [XXXXX] New Log
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));

            DataHelper.queryCache.Remove("IMPRISMA_H", task.Cs, true);
        }

        /// <summary>
        ///  Rerourne les groupes de liquidation split d'un groupe de liquidation
        /// </summary>
        /// <param name="pIdLiquidationGroup">Id non significatif du groupe le liquidation</param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private static List<Pair<Int32, string>> GetLGSOfLG(string pCS, int pIdLiquidationGroup)
        {
            List<Pair<Int32, string>> ret = new List<Pair<int, string>>();

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALG_H", DbType.Int32), pIdLiquidationGroup);

            string sqlQuery = @"select lgs.IDIMPRISMALGS_H, lgs.IDENTIFIER 
  from dbo.IMPRISMALGS_H lgs
            inner join dbo.IMPRISMALG_H lg on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H) and (lg.IDIMPRISMALG_H = @IDIMPRISMALG_H)";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    int idLGS = Convert.ToInt32(dr.GetValue(0));
                    string identifier = Convert.ToString(dr.GetValue(1));
                    ret.Add(new Pair<Int32, string>(idLGS, identifier));
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne les groupes de liquidation split d'un groupe de liquidation
        ///  <para>Cette méthode ne doit être utilisée que lors de l'importation des fichier de la Release 1</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup"></param>
        /// <param name="pIdentifierLiquidationGroup"></param>
        /// <exception cref="Exception s'il n'existe pas aucun groupe de liquidation split"></exception>
        /// <returns></returns>
        private List<Pair<Int32, string>> CheckLGSOfLG(int pIdLiquidationGroup, string pIdentifierLiquidationGroup)
        {
            List<Pair<Int32, string>> ret = GetLGSOfLG(cs, pIdLiquidationGroup);
            if (ArrFunc.IsEmpty(ret))
            {
                string file1 = "Theoretical prices and instrument";
                string file2 = string.Empty;
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        file2 = "Risk Measure Configutation";
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        file2 = "Risk Measure Aggregation Configutation";
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("File :{0} is not implemented", inputSourceDataStyle));
                }
                throw new Exception(StrFunc.AppendFormat("No Liquidation Group Split Found for Liquidation Group {0}(id:{1}), please load file :{2} before file :{3}", pIdentifierLiquidationGroup, pIdLiquidationGroup.ToString(), file1, file2));
            }
            return ret;
        }

        /// <summary>
        /// Methode utilisée tester la méthode BuildInsertSPValues
        /// </summary>
        public static void TestBuildInsertSPValues()
        {
            string cs = @"Data Source=SVR-MSS-2\SVRMSS2;Initial Catalog=RD_PRISMA;Persist Security Info=False;User Id=sa;Password=efs98*;Workstation Id=127.0.0.1;Packet Size=4096";
            cs = @"Data Source=DB02V112;User ID=RD_SPHERES_TST260;Password=efs";

            int id = 3;

            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow();
            row.data = new IOTaskDetInOutFileRowData[2];
            row.data[0] = new IOTaskDetInOutFileRowData();
            row.data[0].name = "toto";
            row.data[0].value = "TOTO";

            row.data[1] = new IOTaskDetInOutFileRowData();
            row.data[1].name = "Scenarios Prices";
            row.data[1].value = "1;2;3;4;5;6;7;8;9;10";
            int LH = 4;

            string SQL = BuildInsertSPValues(cs, id, row, LH);

            System.Diagnostics.Debug.WriteLine(SQL);
        }

        /// <summary>
        /// Ecriture dans System.Diagnostics.Debug
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="currentLine"></param>
        /// <param name="guard"></param>
        private void AuditLine(int lineNumber, string currentLine, int guard)
        {
            if ((lineNumber % 10000) == 0)
            {
                System.Diagnostics.Debug.WriteLine(lineNumber);
                audit.AddStep("Line number: " + lineNumber.ToString());
            }

            if (currentLine == null)
            {
                System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                System.Diagnostics.Debug.WriteLine("ENDED");
                audit.AddStep("ENDED");
                audit.WriteDebug();
            }
        }

        /// <summary>
        ///  Insertion des données dans tables SQL IMPRISMASPS_H, IMPRISMACES_H, IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sqlSP"></param>
        /// <param name="sqlCE"></param>
        /// <param name="sqlVAR"></param>
        private static void ExecuteQueryTheoriticalPriceFile(string pCS, StringBuilder sqlSP, StringBuilder sqlCE, StringBuilder sqlVAR)
        {
            if ((null != sqlSP) && sqlSP.Length > 0)
            {
                sqlSP.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf);
                sqlSP.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlSP.ToString());
            }

            if ((null != sqlCE) && sqlCE.Length > 0)
            {
                sqlCE.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMACES_H(IDIMPRISMARMSLGSS_H, IDC, CE1, CE2, CE3, CE4, CE5)");
                sqlCE.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlCE.ToString());
            }

            if ((null != sqlVAR) && sqlVAR.Length > 0)
            {
                sqlVAR.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAVARS_H(IDIMPRISMARMSLGSS_H, IDC, SHORTLONG, VARTYPE, VARAMOUNT)");
                sqlVAR.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlVAR.ToString());
            }
        }

        /// <summary>
        /// Insertion des données dans tables SQL IMPRISMASPS_H, IMPRISMACES_H, IMPRISMAVARS_H en Asynchrone
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlSP"></param>
        /// <param name="pSqlCE"></param>
        /// <param name="pSqlVAR"></param>
        /// <returns></returns>
        // PM 20180208 [CHEETAH] New
        private static async System.Threading.Tasks.Task ExecuteQueryTheoriticalPriceFileAsync(string pCS, StringBuilder pSqlSP, StringBuilder pSqlCE, StringBuilder pSqlVAR)
        {
            if (((null != pSqlSP) && pSqlSP.Length > 0) || ((null != pSqlCE) && pSqlCE.Length > 0) || ((null != pSqlVAR) && pSqlVAR.Length > 0))
            {
                try
                {
                    m_PrismaSemaphore.Wait();
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        ExecuteQueryTheoriticalPriceFile(pCS, pSqlSP, pSqlCE, pSqlVAR);
                    });
                }
                catch { throw; }
                finally
                {
                    m_PrismaSemaphore.Release();
                }
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sqlMC"></param>
        private static void ExecuteQueryMarketCapacities(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAMKTCAPA_H(IDIMPRISMA_H, PRODUCTLINE, PRODUCTID, ISINCODEUNL, PUTCALL, TTEBUCKETID, MONEYNESSBUCKETID, MARKETCAPACITY, LIQUIDITYPREMIUM)");
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMALIQFACT_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sql"></param>
        private static void ExecuteQueryLiquidityFactors(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf);
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }
        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAFXRMS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sql"></param>
        private static void ExecuteQueryFxRate(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAFXRMS_H(IDIMPRISMAFXPAIR_H, IDIMPRISMARMS_H, SCENARIONO, VALUE)");
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAFXRMS_H en Asynchrone
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSql"></param>
        // PM 20180208 [CHEETAH] New
        private static async System.Threading.Tasks.Task ExecuteQueryFxRateAsync(string pCS, StringBuilder pSql)
        {
            if ((null != pSql) && pSql.Length > 0)
            {
                try
                {
                    m_PrismaSemaphore.Wait();
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        ExecuteQueryFxRate(pCS, pSql);
                    });
                }
                catch { throw; }
                finally
                {
                    m_PrismaSemaphore.Release();
                }
            }
        }

        /// <summary>
        ///  Retourne les assets présents dans la table IMPRISMAS_H
        ///  <para>Pour rapperl cette table est alimentée uniquement par l'importation du fichier Theoretical prices and instrument configuration</para>
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <returns></returns>
        /// FI 20140617 [19911] add method 
        // EG 20180426 Analyse du code Correction [CA2202]
        private List<int> LoadSpheresAssetFromPrismaSerie()
        {
            List<int> ret = new List<int>(); ;

            string query = @"select IDASSET from IMPRISMAS_H s
inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
inner join dbo.IMPRISMA_H main on (main.IDIMPRISMA_H = p.IDIMPRISMA_H) and (main.IDIMPRISMA_H = @IDIMPRISMA_H)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "IDIMPRISMA_H", DbType.Int32), idIMPRISMA_H);

            QueryParameters qry = new QueryParameters(cs, query, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    ret.Add(Convert.ToInt32(dr[0]));
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les PRODUCTID présents dans la table IMPRISMA_P
        /// </summary>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private List<string> LoadProductIdfromPrismaProduct()
        {
            List<string> ret = new List<string>(); ;

            string query = @"select PRODUCTID 
from dbo.IMPRISMAP_H p
inner join dbo.IMPRISMA_H main on (main.IDIMPRISMA_H = p.IDIMPRISMA_H) and (main.IDIMPRISMA_H = @IDIMPRISMA_H)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "IDIMPRISMA_H", DbType.Int32), idIMPRISMA_H);

            QueryParameters qry = new QueryParameters(cs, query, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    ret.Add(Convert.ToString(dr[0]));
                }
            }
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idAsset"></param>
        /// <param name="idExpiry"></param>
        /// <param name="pvPrice"></param>
        /// <param name="stlPrice"></param>
        /// <param name="unlPriceOffset"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryUpdPriceOfSeries(string cs, int idAsset, int idExpiry, Decimal pvPrice, Decimal stlPrice, Nullable<Decimal> unlPriceOffset)
        {
            string sql = @"update dbo.IMPRISMAS_H 
set PVPRICE = @PVPRICE, STLPRICE = @STLPRICE, UNLPRICEOFFSET = @UNLPRICEOFFSET
where (IDIMPRISMAE_H=@IDEXPIRY) and (IDASSET=@IDASSET);";
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "PVPRICE", DbType.Decimal), pvPrice);
            dp.Add(new DataParameter(cs, "STLPRICE", DbType.Decimal), stlPrice);
            dp.Add(new DataParameter(cs, "UNLPRICEOFFSET", DbType.Decimal), unlPriceOffset.HasValue ? unlPriceOffset.Value : Convert.DBNull);
            dp.Add(new DataParameter(cs, "IDEXPIRY", DbType.Int32), idExpiry);
            dp.Add(new DataParameter(cs, "IDASSET", DbType.Int32), idAsset);
            QueryParameters qry = new QueryParameters(cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du CONTRACTMULTIPLIER de la table ASSET_ETD
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        /// PM 20160215 [21491] Déplacement de la méthode dans la classe de base
//        private QueryParameters GetQueryUpdASSET_ETD(int pIdAsset, decimal pContractMultiplier)
//        {
//            string sql = @"update dbo.ASSET_ETD 
//                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
//                                  IDAUPD = @IDAUPD,
//                                  DTUPD = @DTUPD
//                              where (IDASSET=@IDASSET)
//                                and ((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
//                                and exists (select 1 from dbo.DERIVATIVECONTRACT dc
//                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
//                                             where (dc.ISAUTOSETTING = 1)
//                                               and (da.IDDERIVATIVEATTRIB = ASSET_ETD.IDDERIVATIVEATTRIB))";

//            DataParameters dp = new DataParameters();
//            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
//            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAUPD), task.UserId);
//            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTUPD), dtStart);
//            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
//            QueryParameters qry = new QueryParameters(cs, sql, dp);

//            return qry;
//        }
        /// <summary>
        /// Construction du QueryParameters pour la mise à jour des CONTRACTMULTIPLIER, MINPRICEINCR et MINPRICEINCRAMOUNT de la table DERIVATIVECONTRACT
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pMinPriceAmount"></param>
        /// <param name="pMinPriceIncr"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        private QueryParameters GetQueryUpdDERIVATIVECONTRACT(int pIdAsset, decimal pContractMultiplier, decimal pMinPriceAmount, decimal pMinPriceIncr)
        {
            string sql = @"update dbo.DERIVATIVECONTRACT 
                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
                                  MINPRICEINCRAMOUNT = @MINPRICEINCRAMOUNT,
                                  MINPRICEINCR = @MINPRICEINCR,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (ISAUTOSETTING = 1)
                                and (((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
                                  or ((MINPRICEINCR is null) or (MINPRICEINCR != @MINPRICEINCR))
                                  or ((MINPRICEINCRAMOUNT is null) or (MINPRICEINCRAMOUNT != @MINPRICEINCRAMOUNT)))
                                and exists (select 1 from dbo.ASSET_ETD a
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                                             where (a.IDASSET=@IDASSET)
                                               and (da.IDDC = DERIVATIVECONTRACT.IDDC))";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
            dp.Add(new DataParameter(cs, "MINPRICEINCRAMOUNT", DbType.Decimal), pMinPriceAmount);
            dp.Add(new DataParameter(cs, "MINPRICEINCR", DbType.Decimal), pMinPriceIncr);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAUPD), task.process.UserId);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTUPD), dtStart);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du MINPRICEINCR de la table DERIVATIVECONTRACT
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pMinPriceIncr"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        private QueryParameters GetQueryUpdDERIVATIVECONTRACTLight(int pIdAsset, decimal pMinPriceIncr )
        {
            string sql = @"update dbo.DERIVATIVECONTRACT 
                              set MINPRICEINCR = @MINPRICEINCR,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (ISAUTOSETTING = 1)
                                and ((MINPRICEINCR is null) or (MINPRICEINCR != @MINPRICEINCR))
                                and exists (select 1 from dbo.ASSET_ETD a
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                                             where (a.IDASSET=@IDASSET)
                                               and (da.IDDC = DERIVATIVECONTRACT.IDDC))";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "MINPRICEINCR", DbType.Decimal), pMinPriceIncr);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAUPD), task.process.UserId);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTUPD), dtStart);
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Insertion ou mise à jour d'une cotation dans une table QUOTE_XXX_H
        /// </summary>
        /// <param name="pQryParameters">QueryParameters correspondant à un select * sur une table QUOTE_XXX_H</param>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// PM 20151124 [20124] New
        private void InsertUpdate_QUOTE_XXX_H(QueryParameters pQryParameters, int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal pPrice, string pCurrency)
        {
            if (pIdAsset > 0)
            {
                try
                {
                    pQryParameters.parameters["IDASSET"].Value = pIdAsset;
                    DataTable dt = DataHelper.ExecuteDataTable(cs, pQryParameters.query, pQryParameters.parameters.GetArrayDbParameter());
                    DataRow dr;
                    bool setValues = false;
                    if (dt.Rows.Count == 0)
                    {
                        dr = dt.NewRow();
                        SetDataRowIns(dr);
                        dt.Rows.Add(dr);
                        setValues = true;
                    }
                    else
                    {
                        dr = dt.Rows[0];
                        // PM 20160219 [21924] Ajout vérification de ENUM.CUSTOMVALUE
                        string quoteSource = dr["SOURCE"].ToString();
                        if (!IsQuoteOverridable(cs, quoteSource))
                        {
                            ArrayList message = new ArrayList();
                            string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                            message.Insert(0, msgDet);
                            message.Insert(0, "LOG-06073");
                            ProcessMapping.LogLogInfo(task, task.SetErrorWarning, null, message);
                        }
                        else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                            || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                            || (Convert.IsDBNull(dr["IDBC"]) == false)
                            || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                            || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                            || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                            || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                            || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                            || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                            || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                            || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                            || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime))
                            || ((Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice)))
                        {
                            SetDataRowUpd(dr);
                            setValues = true;
                        }
                    }
                    //
                    if (setValues)
                    {
                        //
                        dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                        dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                        dr["IDASSET"] = pIdAsset;
                        dr["IDBC"] = DBNull.Value; //Non présent
                        dr["IDC"] = pCurrency;
                        if (pIdM > 0)
                        {
                            dr["IDM"] = pIdM;
                        }
                        dr["IDMARKETENV"] = m_IdMarketEnv;
                        dr["IDVALSCENARIO"] = m_IdValScenario;
                        dr["ISENABLED"] = true;
                        dr["QUOTESIDE"] = pQuoteSide.ToString();
                        dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
                        dr["QUOTEUNIT"] = "Price";
                        dr["SOURCE"] = "ClearingOrganization";
                        dr["SPREADVALUE"] = 0;
                        dr["TIME"] = pQuoteTime;
                        dr["VALUE"] = pPrice;
                        //
                        DataHelper.ExecuteDataAdapter(cs, pQryParameters.GetQueryReplaceParameters(), dt);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert QUOTE_ETD_H for Asset Id: {0}", pIdAsset), e);
                }
            }
        }

        /// <summary>
        /// Insertion ou mise à jour d'une cotation dans la table QUOTE_ETD_H, avec valeur du Delta
        /// </summary>
        /// <param name="pQryParameters">QueryParameters correspondant à un select * sur une table QUOTE_ETD_H</param>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDelta"></param>
        /// PM 20161019 [22174] Prisma 5.0 : New
        private void InsertUpdate_QUOTE_ETD_H(QueryParameters pQryParameters, int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal pPrice, string pCurrency, decimal pDelta)
        {
            if (pIdAsset > 0)
            {
                try
                {
                    pQryParameters.parameters["IDASSET"].Value = pIdAsset;
                    DataTable dt = DataHelper.ExecuteDataTable(cs, pQryParameters.query, pQryParameters.parameters.GetArrayDbParameter());
                    DataRow dr;
                    bool setValues = false;
                    if (dt.Rows.Count == 0)
                    {
                        dr = dt.NewRow();
                        SetDataRowIns(dr);
                        dt.Rows.Add(dr);
                        setValues = true;
                    }
                    else
                    {
                        dr = dt.Rows[0];
                        // PM 20160219 [21924] Ajout vérification de ENUM.CUSTOMVALUE
                        string quoteSource = dr["SOURCE"].ToString();
                        if (!IsQuoteOverridable(cs, quoteSource))
                        {
                            ArrayList message = new ArrayList();
                            string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                            message.Insert(0, msgDet);
                            message.Insert(0, "LOG-06073");
                            ProcessMapping.LogLogInfo(task, task.SetErrorWarning, null, message);
                        }
                        else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                            || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                            || (Convert.IsDBNull(dr["IDBC"]) == false)
                            || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                            || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                            || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                            || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                            || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                            || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                            || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                            || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                            || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime))
                            || ((Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice))
                            || ((Convert.IsDBNull(dr["DELTA"]) == true) || ((Convert.IsDBNull(dr["DELTA"]) == false) && (Convert.ToDecimal(dr["DELTA"]) != pDelta))))
                        {
                            SetDataRowUpd(dr);
                            setValues = true;
                        }
                    }
                    //
                    if (setValues)
                    {
                        //
                        dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                        dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                        dr["IDASSET"] = pIdAsset;
                        dr["IDBC"] = DBNull.Value; //Non présent
                        dr["IDC"] = pCurrency;
                        if (pIdM > 0)
                        {
                            dr["IDM"] = pIdM;
                        }
                        dr["IDMARKETENV"] = m_IdMarketEnv;
                        dr["IDVALSCENARIO"] = m_IdValScenario;
                        dr["ISENABLED"] = true;
                        dr["QUOTESIDE"] = pQuoteSide.ToString();
                        dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
                        dr["QUOTEUNIT"] = "Price";
                        dr["SOURCE"] = "ClearingOrganization";
                        dr["SPREADVALUE"] = 0;
                        dr["TIME"] = pQuoteTime;
                        dr["VALUE"] = pPrice;
                        dr["DELTA"] = pDelta;
                        //
                        DataHelper.ExecuteDataAdapter(cs, pQryParameters.GetQueryReplaceParameters(), dt);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert QUOTE_ETD_H for Asset Id: {0}", pIdAsset), e);
                }
            }
        }

        /// <summary>
        /// Mise à jour ou insertion d'une cotation
        /// </summary>
        /// <param name="pSqlAsset"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// PM 20151124 [20124] New
        private void InsertUpdateUnderliyngPrice(SQL_AssetETD pSqlAsset, decimal pPrice, string pCurrency)
        {
            // PM 20161019 [22174] Prisma 5.0 : Ajout vérification que pSqlAsset.DrvContract_AssetCategorie est renseigné
            //if ((pSqlAsset != null) && (pSqlAsset.IsLoaded))
            if ((pSqlAsset != null) && (pSqlAsset.IsLoaded) && StrFunc.IsFilled(pSqlAsset.DrvContract_AssetCategorie))
            {
                Cst.UnderlyingAsset underlyingAssetCategory = Cst.ConvertToUnderlyingAsset(pSqlAsset.DrvContract_AssetCategorie);
                int idAssetUnl = (underlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ? pSqlAsset.DrvAttrib_IdAssetUnl : pSqlAsset.DrvContract_IdAssetUnl;
                if (idAssetUnl > 0)
                {
                    QuoteEnum quoteCategory = AssetTools.ConvertUnderlyingAssetToQuoteEnum(underlyingAssetCategory);
                    Cst.OTCml_TBL quoteTable = AssetTools.ConvertQuoteEnumToQuoteTbl(quoteCategory);
                    //FI 20181126 [24308] Add Alimentation du paramètre pQuoteSide
                    QueryParameters qryParamQuote = GetQueryParameters_QUOTE_XXX_H(cs, quoteTable, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialClose);
                    //
                    InsertUpdate_QUOTE_XXX_H(qryParamQuote, idAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialClose, pPrice, pCurrency);
                }
            }
        }

        /// <summary>
        /// Selection des DC dont le CM est null en vue de sa mise à jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdM"></param>
        /// <param name="pDtFile"></param>
        /// <returns></returns>
        /// PM 20151209 [21653] New
        private static QueryParameters GetQuerySelectDERIVATIVECONTRACT(string pCS, int pIdM, DateTime pDtFile)
        {
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + "dc.IDDC, dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CONTRACTTYPE, dc.CONTRACTMULTIPLIER, dc.MINPRICEINCRAMOUNT, dc.MINPRICEINCR, dc.IDAUPD, dc.DTUPD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
            query += SQLCst.WHERE + "(ISAUTOSETTING = 1)" + Cst.CrLf;
            query += SQLCst.AND + "(dc.IDM = @IDM)" + Cst.CrLf;
            query += SQLCst.AND + "(dc.CONTRACTMULTIPLIER is null)" + Cst.CrLf;
            query += SQLCst.AND + "((dc.CONTRACTATTRIBUTE is null) or (dc.CONTRACTATTRIBUTE = '0'))" + Cst.CrLf;
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTFILE") + ")" + Cst.CrLf;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), pIdM);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE), pDtFile);

            QueryParameters qryParameters = new QueryParameters(pCS, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne l'Id non significatif de la serie dans Spheres®
        /// <para>Retourne 0 si la serie n'existe pas ds Spheres® ou si la série n'a jamais été négociée</para>
        /// </summary>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne produit</param>
        /// <param name="pParsingRowSerie">Représente le parsing de la ligne serie</param>
        /// <param name="pParsingRowExpiration">Représente le parsing de la ligne échéance</param>
        /// <returns></returns>
        // PM 20180219 [23824] New
        //private int GetIdAssetETDForPriceOnly(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration)
        private int GetIdAssetETDForPriceOnly(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out MarketDataAssetToImport pAssetData)
        {
            int ret = 0;
            MarketDataAssetToImport assetData = new MarketDataAssetToImport();

            //Row Product
            assetData.ContractSymbol = pParsingRowProduct.GetRowDataValue(cs, "Product ID");

            //Row Expiration
            string year = pParsingRowExpiration.GetRowDataValue(cs, "Contract Year");
            string month = pParsingRowExpiration.GetRowDataValue(cs, "Contract Month");
            string day = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Day");

            //Contract Year/Month n'est pas toujours renseigné (notamment pour les flex), dans ce cas utiliser Expiration Year/Month
            if (StrFunc.IsEmpty(year))
            {
                year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            }
            if (StrFunc.IsEmpty(month))
            {
                month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            }

            //Row Serie
            string callPut = pParsingRowSerie.GetRowDataValue(cs, "Call Put Flag");
            string exerciseStyle = pParsingRowSerie.GetRowDataValue(cs, "Series exercise style flag");
            string strikePrice = pParsingRowSerie.GetRowDataValue(cs, "Exercise Price");
            string seriesVersion = pParsingRowSerie.GetRowDataValue(cs, "Series Version Number");
            string settlementType = pParsingRowSerie.GetRowDataValue(cs, "Settlement Type");
            string flex = pParsingRowSerie.GetRowDataValue(cs, "Flex Series Flag");

            // contractSymbol spécifique si flex
            Boolean isFlex = BoolFunc.IsTrue(flex);
            if (isFlex)
            {
                assetData.ContractSymbol = pParsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
                assetData.ContractType = "FLEX";
            }
            else
            {
                assetData.ContractType = "STD";
            }

            //
            year = "20" + (StrFunc.IsEmpty(year) ? "00" : year);
            month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');
            day = StrFunc.IsEmpty(day) ? "00" : day.PadLeft(2, '0');
            callPut = StrFunc.IsFilled(callPut) ? callPut : "F";

            //
            assetData.MaturityMonthYear = year + month;
            if (isFlex)
            {
                assetData.MaturityMonthYear += day;
            }
            //
            assetData.SettlementMethod = GetSettlementMethod(settlementType);
            assetData.ContractAttribute = seriesVersion;
            assetData.ContractCategory = GetCategory(callPut);
            //
            if (assetData.ContractCategory == "O")
            {
                assetData.ExerciseStyle = GetExerciseStyle(exerciseStyle);
                assetData.PutCallAsFixValue = GetPutCall(callPut);
                assetData.StrikePrice = DecFunc.DecValueFromInvariantCulture(strikePrice);
            }
            //
            pAssetData = assetData;
            //
            ret = m_PrismaAssetToImport.GetAssetId(assetData);
            //
            return ret;
        }
        #endregion Methods
    }

    /// <summary>
    /// Configuration de l'importation des fichiers PRISMA
    /// </summary>
    internal class PrismaInputFileConfig
    {
        /// <summary>
        /// Taille Max par défaut des requêtes SQL
        /// </summary>
//        private const int defaultSQLLimit = 60000;
        private const int defaultSQLLimit = 60000;

        /// <summary>
        /// Obtient le nombre max de produit à importer
        /// <para>L'importation s'arrête dès que ce nombre est atteint</para>
        /// </summary>
        public int nbProductMax
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la liste restrictive des produits à importer
        /// <para>L'importation s'arrête dès que les produits de la liste sont importés</para>
        /// </summary>
        public ArrayList restrictProduct
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définie la directive qui active la recherche du produit dans Spheres®
        /// <para>Si true seuls les produits négociés sont importés</para>
        /// </summary>
        public Boolean isSearchProduct
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définie la directive qui active la recherche de la serie dans Spheres®
        /// <para>Si true seuls les séries négociés sont importés</para>
        /// </summary>
        public Boolean isSearchSerie
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définit la taille max des ordres SQL.Dès qu'un ordre SQL dépasse cette limite il est exécuté 
        /// <para>Définir -1 pour exécuter les requêtes 1 à 1</para>
        /// </summary>
        public int sqlLimitSize
        {
            get;
            set;
        }

        #region constructor
        /// <summary>
        /// Configuration par Défaut
        /// <para>L'importation de charge uniquement les produits et séries existants dans Spheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        public PrismaInputFileConfig()
        {
            isSearchProduct = true;
            isSearchSerie = true;
            restrictProduct = new ArrayList();
            nbProductMax = -1;
            sqlLimitSize = defaultSQLLimit;
        }

        /// <summary>
        /// Importation des produits de la liste
        /// <para>Les produits sont importés uniquement s'ils existent dans Speheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        /// <param name="pRestrictProduct"></param>
        public PrismaInputFileConfig(ICollection pRestrictProduct)
            : this()
        {
            restrictProduct = new ArrayList(pRestrictProduct);
            nbProductMax = ArrFunc.Count(pRestrictProduct);
        }

        /// <summary>
        /// Importation des {pNbProduct} premiers product du fichier
        /// <para>Les produits sont importés uniquement s'ils existent dans Speheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        /// <param name="pNbProduct"></param>
        public PrismaInputFileConfig(int pNbProduct)
            : this()
        {
            nbProductMax = pNbProduct;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20141022 [9700] Eurex Prisma for Eurosys Futures : New class
    internal class EurosysMarketDataImportEurexPrisma : MarketDataImportEurexPrisma
    {
        #region members
        private EurosysMarketData m_EurosysTools;
        #endregion
        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        public EurosysMarketDataImportEurexPrisma(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle)
        {
            string eurosysSchemaPrefix = Settings.Default.EurosysSchemaPrefix;
            m_EurosysTools = new EurosysMarketData(CSTools.SetCacheOn(pTask.Cs), eurosysSchemaPrefix);
        }
        #endregion
        #region methods
        /// <summary>
        /// Retourne true si le symbol est négocié dans Eurosys
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        protected override Boolean IsExistDcInTrade(string pContractSymbol)
        {
            return m_EurosysTools.IsExistDcInTrade(m_dtFile, EUREX_MIC, pContractSymbol);
        }

        /// <summary>
        /// Retourne l'Id non significatif de la serie dans Eurosys
        /// <para>Retourne 0 si la serie n'existe pas dans Eurosys ou si la série n'a jamais été négociée</para>
        /// </summary>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne produit</param>
        /// <param name="pParsingRowSerie">Représente le parsing de la ligne serie</param>
        /// <param name="pParsingRowExpiration">Représente le parsing de la ligne échéance</param>
        /// <param name="pAssetData">Données parsées de l'asset</param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
        //protected override int GetIdAssetETD(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration)
        protected override int GetIdAssetETD(IOTaskDetInOutFileRow pParsingRowProduct, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowExpiration, out MarketDataAssetToImport pAssetData)
        {
            int ret = 0;
            pAssetData = new MarketDataAssetToImport();

            //Row Product
            pAssetData.ContractSymbol = pParsingRowProduct.GetRowDataValue(cs, "Product ID");

            //Row Expiration
            string year = pParsingRowExpiration.GetRowDataValue(cs, "Contract Year");
            string month = pParsingRowExpiration.GetRowDataValue(cs, "Contract Month");
            string day = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Day");

            //PM 20150902 [21236] Contract Year/Month n'est pas toujours renseigné (notamment pour les flex), dans ce cas utiliser Expiration Year/Month
            if (StrFunc.IsEmpty(year))
            {
                year = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Year");
            }
            if (StrFunc.IsEmpty(month))
            {
                month = pParsingRowExpiration.GetRowDataValue(cs, "Expiration Month");
            }
            
            //Row Serie
            string callPut = pParsingRowSerie.GetRowDataValue(cs, "Call Put Flag");
            string exerciseStyle = pParsingRowSerie.GetRowDataValue(cs, "Series exercise style flag");
            string strikePrice = pParsingRowSerie.GetRowDataValue(cs, "Exercise Price");
            string seriesVersion = pParsingRowSerie.GetRowDataValue(cs, "Series Version Number");
            string settlementType = pParsingRowSerie.GetRowDataValue(cs, "Settlement Type");
            string flex = pParsingRowSerie.GetRowDataValue(cs, "Flex Series Flag");

            // contractSymbol spécifique si flex
            bool isFlex = BoolFunc.IsTrue(flex);
            if (isFlex)
            {
                pAssetData.ContractSymbol = pParsingRowSerie.GetRowDataValue(cs, "Flex Product ID");
                pAssetData.ContractType = "FLEX";
            }
            else
            {
                pAssetData.ContractType = "STD";
            }

            //
            year = "20" + (StrFunc.IsEmpty(year) ? "00" : year);
            month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');
            day = StrFunc.IsEmpty(day) ? "00" : day.PadLeft(2, '0');
            //
            pAssetData.MaturityMonthYear = year + month;
            if (isFlex)
            {
                pAssetData.MaturityMonthYear += day;
            }
            //
            pAssetData.SettlementMethod = GetSettlementMethod(settlementType);
            pAssetData.ContractAttribute = seriesVersion;
            pAssetData.ContractCategory = GetCategory(callPut);
            //
            //
            callPut = StrFunc.IsFilled(callPut) ? callPut : "F";
            //
            bool isUseDcExerciseStyle = true;
            bool isUseDcSettltMethod = true;
            bool isUseContractAttrib = true;
            QueryParameters queryParameters = m_EurosysTools.QueryExistAssetInTrades(isUseDcExerciseStyle,isUseDcSettltMethod,isUseContractAttrib);
            DataParameters dp = queryParameters.parameters;
            dp["DTFILE"].Value = m_dtFile;
            dp["ISO10383_ALPHA4"].Value = EUREX_MIC;
            dp["CONTRACTSYMBOL"].Value = pAssetData.ContractSymbol;
            dp["CATEGORY"].Value = pAssetData.ContractCategory;
            dp["MATURITYMONTHYEAR"].Value = pAssetData.MaturityMonthYear;
            dp["SETTLTMETHOD"].Value = ReflectionTools.ConvertEnumToString<SettlMethodEnum>(pAssetData.SettlementMethod);
            dp["CONTRACTATTRIBUTE"].Value = pAssetData.ContractAttribute;
            if (pAssetData.ContractCategory == "O")
            {
                pAssetData.ExerciseStyle = GetExerciseStyle(exerciseStyle);
                pAssetData.PutCallAsFixValue = GetPutCall(callPut);
                pAssetData.StrikePrice = DecFunc.DecValueFromInvariantCulture(strikePrice);
                //
                dp["EXERCISESTYLE"].Value = pAssetData.ExerciseStyle;
                dp["PUTCALL"].Value = pAssetData.PutCallAsFixValue;
                dp["STRIKEPRICE"].Value = pAssetData.StrikePrice;
            }
            else
            {
                dp["EXERCISESTYLE"].Value = null;
                dp["PUTCALL"].Value = null;
                dp["STRIKEPRICE"].Value = null;
            }

            using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(cs), queryParameters.query, queryParameters.parameters.GetArrayDbParameter()).CreateDataReader())
            {
                if (dr.Read())
                    ret = IntFunc.IntValue(dr["IDASSET"].ToString());
                }
            return ret;
        }
        #endregion
    }
    
    /// <summary>
    /// Classe contenant l'ensemble des assets dont les cours sont requis
    /// </summary>
    // PM 20180219 [23824] New
    internal class PrismaDataAsset
    {
        #region Members
        /// <summary>
        /// Ensemble des Assets par ContractSymbol
        /// </summary>
        private Dictionary<string, List<MarketDataAssetToImport>> m_Asset; // Key: ContractSymbol
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constucteur
        /// </summary>
        public PrismaDataAsset()
        {
            m_Asset = new Dictionary<string, List<MarketDataAssetToImport>>();
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Lire les assets dont importer les cours
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDt"></param>
        public void LoadAsset(string pCs, DateTime pDt)
        {
            IDataReader dr = default(IDataReader);
            try
            {
                QueryParameters queryAsset = MarketDataAssetToImportReader.Query(pCs);
                queryAsset.parameters["DTFILE"].Value = pDt;
                //
                dr = DataHelper.ExecuteReader(pCs, CommandType.Text, queryAsset.query.ToString(), queryAsset.parameters.GetArrayDbParameter());
                //
                IEnumerable<MarketDataAssetToImport> assetToImport = 
                    from asset
                        in dr.DataReaderEnumerator<MarketDataAssetToImport, MarketDataAssetToImportReader>()
                    select asset;
                //
                m_Asset = (
                    from asset in assetToImport
                    group asset by asset.ContractSymbol
                    into assetByContract
                    select new
                    {
                        Key = assetByContract.Key,
                        Value = assetByContract.ToList(),
                    }).ToDictionary(e => e.Key, e => e.Value);
            }
            catch (Exception) { throw; }
            finally
            {
                if (dr != default(IDataReader))
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// Indique s'il existe des asset pour le contract Symbol <paramref name="pContractSymbol"/>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        public bool IsExistContract(string pContractSymbol)
        {
            return m_Asset.ContainsKey(pContractSymbol);
        }

        /// <summary>
        /// Recherche de l'IdAsset en fonction des caractéristiques de celui-ci
        /// </summary>
        /// <param name="pAssetInfo">Caractèristique de l'asset dont rechercher l'Id</param>
        /// <returns>IdAsset si trouvé, 0 si non trouvé</returns>
        public int GetAssetId(MarketDataAssetToImport pAssetInfo)
        {
            int assetId = 0;
            if (pAssetInfo != default(MarketDataAssetToImport))
            {
                List<MarketDataAssetToImport> assetList;
                pAssetInfo.IdAsset = 0;
                if (m_Asset.TryGetValue(pAssetInfo.ContractSymbol, out assetList))
                {
                    MarketDataAssetToImport asset = assetList.FirstOrDefault(a =>
                        a.ContractSymbol == pAssetInfo.ContractSymbol &&
                        a.ContractAttribute == pAssetInfo.ContractAttribute &&
                        a.SettlementMethod == pAssetInfo.SettlementMethod &&
                        a.MaturityMonthYear == pAssetInfo.MaturityMonthYear &&
                        ((StrFunc.IsFilled(a.ContractType) ? a.ContractType : "STD") == (StrFunc.IsFilled(pAssetInfo.ContractType) ? pAssetInfo.ContractType : "STD")) &&
                        ((a.ContractCategory == "F" && pAssetInfo.ContractCategory == "F") ||
                         (a.PutCallAsFixValue == pAssetInfo.PutCallAsFixValue &&
                          a.StrikePrice == pAssetInfo.StrikePrice &&
                          a.ExerciseStyle == pAssetInfo.ExerciseStyle))
                    );
                    if (asset != default(MarketDataAssetToImport))
                    {
                        assetId = asset.IdAsset;
                        pAssetInfo.IdAsset = asset.IdAsset;
                        // PM 20181211 [24389][24383] Ajout pour Gestion du cours OfficialSettlement sur les Options sur Indice
                        pAssetInfo.IdAssetUnl = asset.IdAssetUnl;
                        pAssetInfo.MaturityDate = asset.MaturityDate;
                        pAssetInfo.MaturityDateSys = asset.MaturityDateSys;
                        pAssetInfo.AssetSousJacentCategory = asset.AssetSousJacentCategory;
                    }
                }
            }
            return assetId;
        }
        
        /// <summary>
        /// Obtient la liste de ContractSymbol
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfContractSymbol()
        {
            return m_Asset.Keys.ToList();
        }
        /// <summary>
        /// Obtient la liste de IdAsset
        /// </summary>
        /// <returns></returns>
        public List<int> GetListOfIdAsset()
        {
            List<int> idAssetList = 
                (from assetList in m_Asset.Values
                 from asset in assetList
                 select asset.IdAsset).ToList();
            //
            return idAssetList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetInfo"></param>
        /// <returns></returns>
        /// PM 20181211 [24389][24383] Ajout pour Gestion du cours OfficialSettlement sur les Options sur Indice
        public MarketDataAssetToImport GetAssetDataForIndexOption(MarketDataAssetToImport pAssetInfo)
        {
            MarketDataAssetToImport asset = default(MarketDataAssetToImport);
            if (pAssetInfo != default(MarketDataAssetToImport))
            {
                if ((pAssetInfo.ContractCategory == "O") && (pAssetInfo.SettlementMethod == SettlMethodEnum.CashSettlement))
                {
                    List<MarketDataAssetToImport> assetList;
                    if (m_Asset.TryGetValue(pAssetInfo.ContractSymbol, out assetList))
                    {
                        string assetCategory = Cst.UnderlyingAsset.Index.ToString();
                        //
                        asset = assetList.FirstOrDefault(a =>
                            a.ContractSymbol == pAssetInfo.ContractSymbol &&
                            a.ContractAttribute == pAssetInfo.ContractAttribute &&
                            a.SettlementMethod == pAssetInfo.SettlementMethod &&
                            a.MaturityMonthYear == pAssetInfo.MaturityMonthYear &&
                            a.ContractCategory == "O" &&
                                // FI 20190204 [24504] Mise en commentaire de PutCallAsFixValue
                                //a.PutCallAsFixValue == pAssetInfo.PutCallAsFixValue &&
                            a.AssetSousJacentCategory == assetCategory
                        );
                    }
                }
            }
            return asset;
        }

        #endregion Methods
    }

    /// <summary>
    /// Classe des données d'un asset
    /// </summary>
    // PM 20180219 [23824] New
    // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice : Ajout IdAssetUnl, MaturityDate, MaturityDateSys
    internal class MarketDataAssetToImport
    {
        #region Accessors
        /// <summary>
        /// Id interne du contract
        /// </summary>
        public int IdDC { get; set; }
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        public int IdAsset { get; set; }
        /// <summary>
        /// Category de l'asset sous-acent
        /// </summary>
        public string AssetSousJacentCategory { get; set; }
        /// <summary>
        /// Contract Symbol
        /// </summary>
        public string ContractSymbol { get; set; }
        /// <summary>
        /// Contract Type (FLEX/STD)
        /// </summary>
        public string ContractType { get; set; }
        /// <summary>
        /// Indique s'il s'agit d'un asset flexible
        /// </summary>
        public bool IsFlexible
        {
            get { return ContractType == "FLEX"; }
        }
        /// <summary>
        /// Contract Attribute (Version)
        /// </summary>
        public string ContractAttribute { get; set; }
        /// <summary>
        /// Contract Category (F/O)
        /// </summary>
        public string ContractCategory { get; set; }
        /// <summary>
        /// Settlement Method
        /// </summary>
        public SettlMethodEnum SettlementMethod { get; set; }
        /// <summary>
        /// Maturity Month Year
        /// </summary>
        public string MaturityMonthYear { get; set; }
        /// <summary>
        /// Format de Maturity Month Year
        /// </summary>
        public string MaturityMonthYearFormat { get; set; }
        /// <summary>
        /// Put Call as Fix Value (0 = Put / 1 = Call) (ou null)
        /// </summary>
        public string PutCallAsFixValue { get; set; }
        /// <summary>
        /// Strike Price
        /// </summary>
        public decimal StrikePrice { get; set; }
        /// <summary>
        /// Exercise Style (ou null)
        /// </summary>
        public string ExerciseStyle { get; set; }
        /// <summary>
        /// Id interne de l'asset sous-jacent
        /// </summary>
        public int IdAssetUnl { get; set; }
        /// <summary>
        /// Date d'échéance
        /// </summary>
        public DateTime MaturityDate { get; set; }
        /// <summary>
        /// Date d'échéance système
        /// </summary>
        public DateTime MaturityDateSys { get; set; }
        #endregion Accessors
    }

    /// <summary>
    /// Classe de lecture des assets dont les données sont requises
    /// </summary>
    // PM 20180219 [23824] New
    internal class MarketDataAssetToImportReader : IReaderRow
    {
        #region Methods
        /// <summary>
        /// Requête de selection des assets
        /// </summary>
        /// <returns></returns>
        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice : Ajout IDASSET_UNL, MATURITYDATESYS, MATURITYDATE
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        public static QueryParameters Query(string pCS)
        {
            string sqlQuery = @"select dc.IDDC, asset.IDASSET,
            dc.ASSETCATEGORY, dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDASSET_UNL,
            mr.MMYFMT, ma.MATURITYMONTHYEAR, ma.MATURITYDATESYS, ma.MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE
            from dbo.ASSET_ETD asset
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.MATURITYRULE mr on mr.IDMATURITYRULE = ma.IDMATURITYRULE
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            where (mk.EXCHANGEACRONYM = 'EUR') and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE)) ";
            //
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTFILE") + ")" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk", "DTFILE") + ")" + Cst.CrLf;
            //
            sqlQuery += @" union 
            select dc_unl.IDDC, asset_unl.IDASSET,
            dc_unl.ASSETCATEGORY, dc_unl.CONTRACTSYMBOL, dc_unl.CONTRACTATTRIBUTE, dc_unl.CATEGORY, dc_unl.CONTRACTTYPE, dc_unl.EXERCISESTYLE, dc_unl.SETTLTMETHOD, dc_unl.IDASSET_UNL,
            mr_unl.MMYFMT, ma_unl.MATURITYMONTHYEAR, ma_unl.MATURITYDATESYS, ma_unl.MATURITYDATE,
            asset_unl.PUTCALL, asset_unl.STRIKEPRICE
            from dbo.DERIVATIVEATTRIB da
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDASSET = da.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDERIVATIVEATTRIB = asset_unl.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
            inner join dbo.MATURITYRULE mr_unl on mr_unl.IDMATURITYRULE = ma_unl.IDMATURITYRULE
            inner join dbo.DERIVATIVECONTRACT dc_unl on (dc_unl.IDDC = da_unl.IDDC)
            inner join dbo.MARKET mk_unl on (mk_unl.IDM = dc.IDM)
            where (mk_unl.EXCHANGEACRONYM = 'EUR') and (ma_unl.DELIVERYDATE is null or ma_unl.DELIVERYDATE >= @DTFILE) ";
            //
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTFILE") + ")" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc_unl", "DTFILE") + ")" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk_unl", "DTFILE") + ")" + Cst.CrLf;
            //
            sqlQuery += @" union 
            select dc.IDDC, asset.IDASSET,
            dc.ASSETCATEGORY, dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDASSET_UNL,
            mr.MMYFMT, ma.MATURITYMONTHYEAR, ma.MATURITYDATESYS, ma.MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE
            from dbo.ASSET_ETD asset
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.MATURITYRULE mr on mr.IDMATURITYRULE = ma.IDMATURITYRULE
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            where (mk.EXCHANGEACRONYM = 'EUR')
            and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE))
            and (dc.ISMANDATORYIMPORTQUOTE = 1)
            ";
            //
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTFILE") + ")" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk", "DTFILE") + ")" + Cst.CrLf;
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE));
            //
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            //
            return qryParameters;
        }
        #endregion Methods

        #region IReaderRow
        /// <summary>
        /// Data Reader permettant de lire les enregistrements
        /// </summary>
        public IDataReader Reader { get; set; }
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'un objet
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public object GetRowData()
        {
            MarketDataAssetToImport ret;
            if (null != Reader)
            {
                ret = new MarketDataAssetToImport()
                {
                    IdDC = Convert.ToInt32(Reader["IDDC"]),
                    IdAsset = Convert.ToInt32(Reader["IDASSET"]),
                    AssetSousJacentCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString()),
                    ContractSymbol = Reader["CONTRACTSYMBOL"].ToString(),
                    ContractAttribute = (Convert.IsDBNull(Reader["CONTRACTATTRIBUTE"]) ? "0" : Reader["CONTRACTATTRIBUTE"].ToString()),
                    ContractCategory = Reader["CATEGORY"].ToString(),
                    ContractType = (Convert.IsDBNull(Reader["CONTRACTTYPE"]) ? "STD" : Reader["CONTRACTTYPE"].ToString()),
                    ExerciseStyle = (Convert.IsDBNull(Reader["EXERCISESTYLE"]) ? null : Reader["EXERCISESTYLE"].ToString()),
                    SettlementMethod = ReflectionTools.ConvertStringToEnumOrDefault<SettlMethodEnum>((Convert.IsDBNull(Reader["SETTLTMETHOD"]) ? null : Reader["SETTLTMETHOD"].ToString()), SettlMethodEnum.CashSettlement),
                    MaturityMonthYearFormat = Reader["MMYFMT"].ToString(),
                    MaturityMonthYear = Reader["MATURITYMONTHYEAR"].ToString(),
                    PutCallAsFixValue = (Convert.IsDBNull(Reader["PUTCALL"]) ? null : Reader["PUTCALL"].ToString()),
                    StrikePrice = (Convert.IsDBNull(Reader["STRIKEPRICE"]) ? 0 : Convert.ToDecimal(Reader["STRIKEPRICE"])),
                    // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice : Ajout IDASSET_UNL, MATURITYDATESYS, MATURITYDATE
                    IdAssetUnl = (Convert.IsDBNull(Reader["IDASSET_UNL"]) ? 0 : Convert.ToInt32(Reader["IDASSET_UNL"])),
                    MaturityDate = (Convert.IsDBNull(Reader["MATURITYDATE"]) ? default(DateTime) : Convert.ToDateTime(Reader["MATURITYDATE"])),
                    MaturityDateSys = (Convert.IsDBNull(Reader["MATURITYDATESYS"]) ? default(DateTime) : Convert.ToDateTime(Reader["MATURITYDATESYS"])),
                };
            }
            else
            {
                ret = default(MarketDataAssetToImport);
            }
            return ret;
        }
        #endregion IReaderRow
    }
}
