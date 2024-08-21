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
                            m_QueryExistDC.Parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL_New;
                            m_QueryExistDC.Parameters["CONTRACTATTRIBUTE"].Value = dc_CONTRACTATTRIBUTE;

                            using (IDataReader drDC = DataHelper.ExecuteReader(task.Cs, CommandType.Text, m_QueryExistDC.Query, m_QueryExistDC.Parameters.GetArrayDbParameter()))
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
                                m_QueryExistAssetOpt.Parameters["STRIKEPRICE"].Value = GetStrikePrice(strikePrice, strikePriceDivisor);

                                // On récupère: Product Family Type (CFT) - (Position 8 Longueur 1)
                                // valeurs possibles: 'P' et 'C'
                                m_QueryExistAssetOpt.Parameters["PUTCALL"].Value = GetPutCall(currentLine.Substring(7, 1).Trim());

                                queryParameters = m_QueryExistAssetOpt;
                            }

                            using (IDataReader drAsset = DataHelper.ExecuteReader(task.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
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
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                dataParameters_Asset_Option.Add(new DataParameter(Cs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                dataParameters_Asset_Option.Add(new DataParameter(Cs, "STRIKEPRICE", DbType.Decimal));

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
                dataParameters_Unl_Asset.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDASSET));
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
                            isToCopyOk = !IsNewMode;
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
                            //object obj = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlQuery_Dc, dataParameters_Dc.GetArrayDbParameter());
                            //isToCopyDerivativeContract = (obj != null);
                            using (IDataReader drDerivContract = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Dc, dataParameters_Dc.GetArrayDbParameter()))
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

                                    using (IDataReader drIDASSET = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Asset, dataParameters_Asset_Option.GetArrayDbParameter()))
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

                                    using (IDataReader drIDASSET = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Asset, dataParameters_Asset_Future.GetArrayDbParameter()))
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
                                        using (IDataReader drIDASSET = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Asset2, dataParameters_Asset_Future.GetArrayDbParameter()))
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

                                    DataSet dsDCInfo = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text, sqlQuery_DcInfo, dataParameters_DcInfo.GetArrayDbParameter());
                                    DataTable dtDCInfo = dsDCInfo.Tables[0];

                                    if (dtDCInfo.Rows.Count > 0)
                                    {
                                        dc_IDENTIFIER = (dtDCInfo.Rows[0]["IDENTIFIER"] == Convert.DBNull ? string.Empty : dtDCInfo.Rows[0]["IDENTIFIER"].ToString());
                                        dc_IDI = (dtDCInfo.Rows[0]["IDI"] == Convert.DBNull ? 0 : Convert.ToInt32(dtDCInfo.Rows[0]["IDI"]));
                                    }
                                    // FI 20180927 [24202] Add isAssetAlreadyExist
                                    string validationMsg = string.Empty; //Message de warning associé à la création de l'asset
                                    string infoMsg = string.Empty; //Message d'info des actions menées
                                    // 2- Création du nouvel Asset
                                    SQL_AssetETD sqlAsset = AssetTools.CreateAssetETD(Cs, (IProductBase)unknownProduct,
                                        EUREX_MIC, dc_IDENTIFIER, dc_CATEGORY, maturity_MATURITYMONTHYEAR,
                                        asset_PUTCALL, asset_STRIKEPRICE,
                                        task.Process.UserId, OTCmlHelper.GetDateSysUTC(Cs),
                                    m_dtFile, out validationMsg, out infoMsg, out bool isAssetAlreadyExist);

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
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.None, msgLog, 3, new LogParam(idasset, default, default, Cst.LoggerParameterLink.IDDATA)));
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

                                using (IDataReader asset_Unl_asset = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Unl_Asset, dataParameters_Unl_Asset.GetArrayDbParameter()))
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
                dataParameters_Dc_Referentiel.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

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
                dataParameters_Dc_Trade.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Dc_Trade.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

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
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Future.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));

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
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CATEGORY));
                dataParameters_Asset_Option.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                dataParameters_Asset_Option.Add(new DataParameter(Cs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                dataParameters_Asset_Option.Add(new DataParameter(Cs, "STRIKEPRICE", DbType.Decimal));

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
                //DataSet ds = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlQuery, dataParameters.GetArrayDbParameter());
                DataSet ds = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlQuery);

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

                            object obj_Dc_Referentiel = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlQuery_Dc_Referentiel, dataParameters_Dc_Referentiel.GetArrayDbParameter());

                            // Si c’est le cas On affecte isExistDerivativeContract à 'True' sinon On On affecte isToCopyBucketVolatility à 'False'
                            isExistDerivativeContract = (obj_Dc_Referentiel != null);

                            #endregion

                            // On vérifie si on a au moins l'existance d'au moins un trade à partir du CONTRACTSYMBOL d'un DC
                            #region Vérification de l'existence du DC sur un Trade

                            dataParameters_Dc_Trade["DTFILE"].Value = m_dtFile;
                            dataParameters_Dc_Trade["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;

                            object obj_Dc_Trade = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlQuery_Dc_Trade, dataParameters_Dc_Trade.GetArrayDbParameter());

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
                                    asset_IDASSET_Option = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlQuery_Asset_Option, dataParameters_Asset_Option.GetArrayDbParameter());

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
                                asset_IDASSET_Future = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlQuery_Asset_Future, dataParameters_Asset_Future.GetArrayDbParameter());

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
                                //        new DataParameter(Cs, "MARGIN_STYLE", DbType.String),
                                //        new DataParameter(Cs, "CONTRACTSYMBOL", DbType.String)
                                //    };

                                //Dictionary<string, object> values = new Dictionary<string, object>();
                                //values.Add("MARGIN_STYLE", contract_MARGIN_STYLE);
                                //values.Add("CONTRACTSYMBOL", dc_CONTRACTSYMBOL);

                                //int rowsupdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text,
                                //    "UPDATE dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE  where CONTRACTSYMBOL = @CONTRACTSYMBOL",
                                //    DataContractHelper.GetDbDataParameters(parameters, values));
                                //                            DataParameter[] parameters = new DataParameter[]
                                //                                {
                                //                                    new DataParameter(Cs, "MARGIN_STYLE", DbType.String),
                                //                                    new DataParameter(Cs, "CONTRACTSYMBOL", DbType.String)
                                //                                };

                                //                            Dictionary<string, object> values = new Dictionary<string, object>();
                                //                            values.Add("MARGIN_STYLE", contract_MARGIN_STYLE);
                                //                            values.Add("CONTRACTSYMBOL", dc_CONTRACTSYMBOL);
                                //                            int rowsupdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, "UPDATE dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE  where CONTRACTSYMBOL = @CONTRACTSYMBOL",
                                //DataContractHelper.GetDbDataParameters(parameters, values));
                                DataParameters dp = new DataParameters();
                                dp.Add(new DataParameter(Cs, "MARGIN_STYLE", DbType.String), contract_MARGIN_STYLE);
                                dp.Add(new DataParameter(Cs, "CONTRACTSYMBOL", DbType.String), dc_CONTRACTSYMBOL);
                                string sqlUpdate = "update dbo.PARAMSEUREX_CONTRACT set MARGIN_STYLE = @MARGIN_STYLE where CONTRACTSYMBOL = @CONTRACTSYMBOL";
                                QueryParameters qryParameters = new QueryParameters(Cs, sqlUpdate, dp);
                                int rowsupdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                                break;
                            #endregion

                            #region Record Type => (Expiry Record (E) )
                            case "E":

                                // Déversement des Datatables dans la Database et purge
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                                dt_PARAMSEUREX_MATURITY.Clear();
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                                dt_PARAMSEUREX_ASSETETD.Clear();
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
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
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                                dt_PARAMSEUREX_ASSETETD.Clear();
                                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
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
                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_MATURITY", dt_PARAMSEUREX_MATURITY);
                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_ASSETETD", dt_PARAMSEUREX_ASSETETD);
                DataHelper.ExecuteDataAdapter(Cs, "select * from dbo.PARAMSEUREX_VOLATILITY", dt_PARAMSEUREX_VOLATILITY);
            }
            #endregion
            return lineNumber - 1;
        }

        /// <summary>
        /// Parse le records S, B, VU, VN et VD à des fins de mise à jour de la table PARAMSEUREX_VOLATILITY
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdPARAMSEUREX_MATURITY"></param>
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
                        decimal Bucket_UnlPrx;
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
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(Cs);
                    dr["IDAINS"] = task.Process.UserId;
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
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSysUTC(Cs);
                    dr_filtered[0]["IDAUPD"] = task.Process.UserId;
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
            string filter = " IDASSET=" + pIdAsset.ToString();
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
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(Cs);
                    dr["IDAINS"] = task.Process.UserId;
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
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSys(Cs);
                    dr_filtered[0]["IDAUPD"] = task.Process.UserId;
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
                    SQLUP.GetId(out idPARAMSEUREX_MATURITY, Cs, SQLUP.IdGetId.PARAMSEUREX_MATURITY, SQLUP.PosRetGetId.First, 1);

                    dr.BeginEdit();
                    dr["IDPARAMSEUREX_MATURITY"] = idPARAMSEUREX_MATURITY;
                    dr["DTMARKET"] = m_dtFile;
                    // FI 20200820 [25468] dates systemes en UTC
                    dr["DTINS"] = OTCmlHelper.GetDateSysUTC(Cs);
                    dr["IDAINS"] = task.Process.UserId;
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
                    dr_filtered[0]["DTUPD"] = OTCmlHelper.GetDateSysUTC(Cs);
                    dr_filtered[0]["IDAUPD"] = task.Process.UserId;
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
            dataParameter = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE);
            dataParameter.Value = m_dtFile;
            dataParameters.Add(dataParameter);

            string sqlQuery = "delete from dbo.PARAMSEUREX_MATURITY where DTMARKET=@DTFILE";
            int nRowDeleted = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery, dataParameters.GetArrayDbParameter());

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
}
