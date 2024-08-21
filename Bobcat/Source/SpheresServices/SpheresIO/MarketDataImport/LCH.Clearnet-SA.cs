using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;

using EfsML;
using EfsML.v30.Fix;

using FpML.Interface;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// 
    /// </summary>
    internal class MarketDataImportLCHClearnetSA : MarketDataImportBase
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle">Type de fichier</param>
        public MarketDataImportLCHClearnetSA(Task pTask, string pDataName, string pDataStyle) :
            base(pTask, pDataName, pDataStyle) { }
        #endregion Constructor

        #region Methods

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Repository CLEARNET
        /// </summary>
        /// <param name="pOutputFileName"></param>
        /// FI 20131205 [19275] Mise en place d'un compteur qui déterminer le nombre de valeur différentes de contractMultiplier pour un même symbole
        /// BD 20130306 "Tri" du fichier pour mettre les DC Options à la fin et les Futures au début
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightMarketDataFile(string pOutputFileName)
        {
            try
            {
                #region Declare
                string dcKey = string.Empty;
                string contractSymbol = string.Empty;
                string exchangeSymbol = string.Empty;
                string contractMultiplier = string.Empty;

                string category = string.Empty;
                List<string> lsDCOption = new List<string>(); //List qui contiendra tous les DC Options
                List<string> lsDCFuture = new List<string>(); //List qui contiendra tous les DC Futures
                List<string> lsAllLine = new List<string>(); //List qui contiendra tous les DC triés (Futures puis Options)
                string currentLine;
                string firstLine = string.Empty; //Première ligne du fichier (ex: "00000PFILEDD01DER0642013030419173520130305")
                string lastLine = string.Empty; //Dernière ligne du fichier (ex: "99999PFILEDDDER000000000041741")
                bool isAutoSetting = false;
                bool isAutoSettingAsset = false;
                bool isToCopyOk;
                Dictionary<string, Pair<bool, List<string>>> dicDCKey = new Dictionary<string, Pair<bool, List<string>>>();


                #endregion

                //Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);
                //Ouverture du fichier d'entrée CLEARNET d'origine
                OpenInputFileName();

                int guard = 9999999; //39985 reccords dans le fichier du 20120106
                int lineNumber = 0;
                while (++lineNumber < guard)
                {
                    if ((lineNumber % 5000) == 0)
                        System.Diagnostics.Debug.WriteLine(lineNumber);

                    //Lecture d'une ligne dans le fichier d'entrée CLEARNET d'origine
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

                    #region !StartsWith("1"): lignes d'information (Header, Footer)
                    //  Si: 
                    //      - la ligne ne commence pas par un "1", il s'agit de lignes d'information (Header, Footer)
                    //      --> On recopie la ligne
                    //  Sinon:
                    //      Voir autres cas ci-dessous
                    isToCopyOk = !currentLine.StartsWith("1");
                    #endregion

                    if (isToCopyOk == false)
                    {
                        #region Check DERIVATIVECONTRACT
                        isAutoSetting = false;

                        exchangeSymbol = GetExchangeSymbol(currentLine);
                        contractSymbol = GetContractSymbol(currentLine);
                        category = GetContractCategory(currentLine);
                        contractMultiplier = GetContractMultiplier(currentLine);

                        dcKey = BuildKey(exchangeSymbol, contractSymbol);

                        if (dicDCKey.ContainsKey(dcKey))
                        {
                            //  DC (couple EXCHANGESYMBOL + CONTRACTSYMBOL) 
                            //  identique à celui d'une ligne précédente.
                            //  On a donc déjà vérifié dans la table DERIVATIVECONTRACT la valeur de ISAUTOSETTINGASSET pour ce DC 
                            //  Si: 
                            //      - ISAUTOSETTINGASSET=True (donc création des ASSET_ETD)
                            //      --> On recopie la ligne, si l'ASSET_ETD qu'elle contient n'existe pas dans la database
                            //  Sinon: 
                            //      --> On ignore la ligne
                            isAutoSettingAsset = dicDCKey[dcKey].First;
                            isToCopyOk = isAutoSettingAsset;

                            if (false == (dicDCKey[dcKey].Second.Contains(contractMultiplier)))
                                dicDCKey[dcKey].Second.Add(contractMultiplier);
                        }
                        else
                        {
                            // Recherche d'existence du DC dans la database
                            m_QueryExistDC.Parameters["EXCHANGESYMBOL"].Value = exchangeSymbol;
                            m_QueryExistDC.Parameters["CONTRACTSYMBOL"].Value = contractSymbol;

                            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistDC.Query, m_QueryExistDC.Parameters.GetArrayDbParameter()))
                            {
                                if (dr.Read())
                                {
                                    //  Le DC existe dans la database
                                    //  Si: 
                                    //      - ISAUTOSETTINGASSET=True (donc création des ASSET_ETD)
                                    //      --> On recopie la ligne, si l'ASSET_ETD qu'elle contient n'existe pas dans la database
                                    //          et on fera de même pour toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL)
                                    isAutoSettingAsset = BoolFunc.IsTrue(dr["ISAUTOSETTINGASSET"]);
                                    isAutoSetting = Convert.ToBoolean(dr["ISAUTOSETTING"]);

                                    isToCopyOk = isAutoSettingAsset;

                                    if (isToCopyOk == false)
                                    {
                                        //  Le DC n'est pas en ISAUTOSETTINGASSET=True (donc pas de création des ASSET_ETD)
                                        //  On vérifie alors si le DC est malgré tout en ISAUTOSETTING=True (donc maj des caractéristiques du DC)
                                        //
                                        //  Si: 
                                        //      - ISAUTOSETTING=True (maj des caractéristiques du DC), 
                                        //      --> on recopie la ligne
                                        //          et on "ignorera" toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL) 
                                        //  Sinon:
                                        //      --> on ignore la ligne
                                        //          et on "ignorera" toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL) 
                                        isToCopyOk = isAutoSetting;
                                    }
                                }
                                else
                                {
                                    // Le DC n'existe pas
                                    //      --> on recopie la ligne
                                    //          et on "ignorera" toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL) 
                                    isAutoSettingAsset = false;
                                    isToCopyOk = true;
                                }
                                isToCopyOk = true; //BD&FL 20130314 On insère la ligne même si le DC existe déjà
                            }

                            dicDCKey.Add(dcKey, new Pair<bool, List<string>>(isAutoSettingAsset, new List<string>(new string[] { contractMultiplier })));
                        }
                        #endregion

                        if ((isToCopyOk == true) && (isAutoSettingAsset) && (isAutoSetting == false))
                        {
                            #region Check ASSET_ETD

                            // Recherche d'existence de l'ASSET dans la database
                            SetQueryExistAssetParameter(category, "EXCHANGESYMBOL", exchangeSymbol);
                            SetQueryExistAssetParameter(category, "CONTRACTSYMBOL", contractSymbol);
                            // On récupère: Expiry code for the derivative product (EC) - (Position 146 Longueur 8)
                            SetQueryExistAssetParameter(category, "MATURITYMONTHYEAR", currentLine.Substring(145, 8).Trim());
                            SetQueryExistAssetParameter(category, "CATEGORY", category);

                            QueryParameters queryParameters = m_QueryExistAssetFut;
                            if (category == "O")
                            {
                                // On récupère: Strike price for a derivative product (SP) - (Position 336 Longueur 18)
                                string strikePrice = currentLine.Substring(335, 18).Trim();
                                // On récupère: Strike Price Divisor (SPD) - (Position 335 Longueur 1)
                                string strikePriceDivisor = currentLine.Substring(334, 1).Trim();
                                // STRIKEPRICE = SP / (10 ^ SPD)
                                m_QueryExistAssetOpt.Parameters["STRIKEPRICE"].Value = GetStrikePrice(strikePrice, strikePriceDivisor);

                                // On récupère: Option sign (OS) - (Position 216 Longueur 1)
                                // valeurs possibles: 'P' et 'C'
                                m_QueryExistAssetOpt.Parameters["PUTCALL"].Value = GetPutCall(currentLine.Substring(215, 1).Trim());
                                queryParameters = m_QueryExistAssetOpt;
                            }

                            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                            {
                                // L'Asset n'existe pas dans la database
                                //      --> on recopie la ligne
                                // Dans le cas où l'Assate existe, 
                                //      --> on ne recopie pas la ligne                            
                                if (dr.Read())
                                    isToCopyOk = false;
                                else
                                    isToCopyOk = true;
                            }
                            #endregion
                        }
                    }

                    if (isToCopyOk)
                    {
                        if (IsFirstLine(currentLine))
                        {
                            firstLine = currentLine;
                        }
                        else if (IsLastLine(currentLine))
                        {
                            lastLine = currentLine;
                        }
                        else
                        {
                            if (category.Equals("F"))
                            {
                                //Ajout du DC courant dans la liste des Futures : il s'agit d'un Future
                                lsDCFuture.Add(currentLine);
                            }
                            else if (category.Equals("O"))
                            {
                                //Ajout du DC courant dans la liste des Options : il s'agit d'un Option
                                lsDCOption.Add(currentLine);
                            }
                        }
                    }
                    #endregion
                }
                //Ajout dans l'ordre des lignes dans lsAllLine
                lsAllLine.Add(firstLine);
                lsAllLine.AddRange(lsDCFuture);
                lsAllLine.AddRange(lsDCOption);
                lsAllLine.Add(lastLine);

                foreach (string line in lsAllLine)
                {
                    string lineNew = ReplaceContractMultiplier(line, dicDCKey);
                    //Ligne par ligne, écriture dans le StreamWriter
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //IOTools.StreamWriter.WriteLine(lineNew);
                    IOCommonTools.StreamWriter.WriteLine(lineNew);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer tous les fichiers
                CloseAllFiles();
            }

        }

        /// <summary>
        /// Récupère: Financial market code (MC) - (Position 7 Longueur 3)
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static string GetExchangeSymbol(string pLine)
        {
            return pLine.Substring(6, 3).Trim();
        }

        /// <summary>
        /// Récupère: Mnemonic code for the family (FMC) - (Position 61 Longueur 5)
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static string GetContractSymbol(string pLine)
        {
            return pLine.Substring(60, 5).Trim();
        }

        /// <summary>
        /// Récupère: Type of derivative product family - (Position 101 Longueur 1)
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static string GetContractCategory(string pLine)
        {
            return pLine.Substring(100, 1).Trim();
        }

        /// <summary>
        /// Récupère: Valuation coefficient for positions on a derivative instrument - (Position 600 Longueur 18)
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static string GetContractMultiplier(string pLine)
        {
            return pLine.Substring(600, 18).Trim();
        }
        /// <summary>
        /// Remplace éventuellement dans la {pLine} la zone "Valuation coefficient for positions on a derivative instrument"
        /// <para>Le remplacement s'effectue lorsque pour un même symbole plusieurs contractMuktiplier existent</para>
        /// <para>LA valeur de remplacement est zéro</para> 
        /// </summary>
        /// <param name="pLine">Ligne source du fichier</param>
        /// <param name="pDicKey"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method 
        private static string ReplaceContractMultiplier(string pLine, Dictionary<string, Pair<bool, List<string>>> pDicKey)
        {
            string ret = pLine;

            if ((false == IsFirstLine(pLine)) & (false == IsLastLine(pLine)))
            {
                string exchangeSymbol = GetExchangeSymbol(pLine);
                string contractSymbol = GetContractSymbol(pLine);
                string key = BuildKey(exchangeSymbol, contractSymbol);

                if (pDicKey[key].Second.Count > 1)
                    ret = ret.Remove(600, 18).Insert(600, "0".PadLeft(18, '0'));
            }
            return ret;
        }
        /// <summary>
        /// Génère une clé unique pour le couple ({pExchangeSymbol},{pContractSymbol})
        /// </summary>
        /// <param name="pExchangeSymbol"></param>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static string BuildKey(string pExchangeSymbol, string pContractSymbol)
        {
            return pExchangeSymbol + "~" + pContractSymbol;
        }

        /// <summary>
        ///  Retourne true si {pLine} est la 1er ligne du fichier
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static Boolean IsFirstLine(string pLine)
        {
            return pLine.StartsWith("00000");
        }

        /// <summary>
        ///  Retourne true si {pLine} est la dernière ligne du fichier
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// FI 20131205 [19275] add Method
        private static Boolean IsLastLine(string pLine)
        {
            return pLine.StartsWith("99999");
        }

        #endregion
    }
}
