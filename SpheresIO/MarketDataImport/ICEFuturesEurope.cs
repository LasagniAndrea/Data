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
    internal class MarketDataImportICEFuturesEurope : MarketDataImportBase
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle">Type de fichier</param>
        public MarketDataImportICEFuturesEurope(Task pTask, string pDataName, string pDataStyle) :
            base(pTask, pDataName, pDataStyle, true) { }
        #endregion Constructor

        #region Methods

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Repository ICE Futures Europe
        /// </summary>
        /// <param name="pOutputFileName"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightMarketDataFile(string pOutputFileName)
        {
            try
            {
                #region Declare
                string dcKey = string.Empty;

                string contractSymbol = string.Empty;
                string exchangeCode = string.Empty;                
                string category = string.Empty;
                string maturityMonthYear = string.Empty;
                string sBusinessDate = string.Empty;
                DateTime businessDate = DateTime.MinValue;
            
                string currentLine;
                bool isToCopyOk;
                Dictionary<string, bool> dicDCKey = new Dictionary<string, bool>();
                #endregion

                //Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);
                //Ouverture du fichier d'entrée ICE Futures Europe d'origine
                OpenInputFileName();

                int guard = 9999999; //540000 reccords dans le fichier du 20171121
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

                    #region lineNumber == 1: ligne entête (Header)
                    //  Si: 
                    //      - il s'agit de la première ligne (Header)
                    //      --> On recopie la ligne
                    //  Sinon:
                    //      Voir autres cas ci-dessous
                    isToCopyOk = (lineNumber == 1);
                    #endregion

                    if (isToCopyOk == false)
                    {
                        #region Check DERIVATIVECONTRACT                        

                        /* Exemple :
                         *  currentLine = 21-NOV-2017,"WTB","IFEU","FUT","","","20180600","455654","GB00H1RZYK67","ICE/FUT 20180521 IFEU:WTB","FCECSX"
                        */
                        string[] separator = new string[] { "," };
                        char[] delimeter = new char[] { '"' };
                        string[] splittedLine = currentLine.Split(separator, StringSplitOptions.None);

                        contractSymbol = splittedLine[1].Trim(delimeter);   // Order 2 - COMMODITY_CODE (Contract Symbol for the derivative product)
                        exchangeCode = splittedLine[2].Trim(delimeter);     // Order 3 - EXCHANGE_CODE (Financial market code)
                        
                        dcKey = BuildKey(exchangeCode, contractSymbol);

                        if (dicDCKey.ContainsKey(dcKey))
                        {
                            //  DC (couple EXCHANGESYMBOL + CONTRACTSYMBOL) 
                            //  identique à celui d'une ligne précédente.
                            //  On a donc déjà vérifié l'existance du DC dans la table DERIVATIVECONTRACT                            
                            isToCopyOk = dicDCKey[dcKey];
                        }
                        else
                        {
                            sBusinessDate = splittedLine[0].Trim(delimeter);    // Order 1 - BUSINESS_DATE (Business Date)
                            if (sBusinessDate.Length == 9)
                                businessDate = DtFunc.ParseDate(sBusinessDate, "dd-MMM-yy", null);
                            else
                                businessDate = DtFunc.ParseDate(sBusinessDate, "dd-MMM-yyyy", null);

                            // Recherche d'existence du DC dans la database
                            m_QueryExistDCInTrade.Parameters["ISO10383_ALPHA4"].Value = exchangeCode;
                            m_QueryExistDCInTrade.Parameters["CONTRACTSYMBOL"].Value = contractSymbol;
                            m_QueryExistDCInTrade.Parameters["DTFILE"].Value = businessDate;

                            using (IDataReader drDC = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistDCInTrade.Query, m_QueryExistDCInTrade.Parameters.GetArrayDbParameter()))
                            {
                                //  isToCopyOk = true  : Le DC existe dans la database
                                //  isToCopyOk = false : Le DC n'existe pas, on recopie la ligne et on "ignorera" toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL)
                                isToCopyOk = drDC.Read();
                            }
                            dicDCKey.Add(dcKey, isToCopyOk);
                        }
                        #endregion

                        if (isToCopyOk)
                        {
                            #region Check ASSET_ETD
                            
                            category = splittedLine[3].Trim(delimeter).Substring(0, 1);     // Order 4 - SECURITY_TYPE (Category for the derivative product)
                            maturityMonthYear = splittedLine[6].Trim(delimeter);            // Order 7 - CONTRACT_PERIOD (Expiry code for the derivative product)
                            if (maturityMonthYear.Substring(6, 2) == "00")
                                maturityMonthYear = maturityMonthYear.Substring(0, 6);      // Pour les Monthly, on enlève le jour "00"

                            // Recherche d'existence de l'ASSET dans la database
                            SetQueryExistAssetParameter(category, "ISO10383_ALPHA4", exchangeCode);
                            SetQueryExistAssetParameter(category, "CONTRACTSYMBOL", contractSymbol);
                            SetQueryExistAssetParameter(category, "MATURITYMONTHYEAR", maturityMonthYear);
                            SetQueryExistAssetParameter(category, "CATEGORY", category);

                            QueryParameters queryParameters = m_QueryExistAssetFut;
                            if (category == "O")
                            {
                                m_QueryExistAssetOpt.Parameters["PUTCALL"].Value = GetPutCall(splittedLine[4].Trim(delimeter)); // Order 5 - OPTION_TYPE (Option Type for the derivative product)
                                string strikePrice = splittedLine[5].Trim(delimeter);                                           // Order 6 - STRIKE (Strike price for the derivative product)
                                m_QueryExistAssetOpt.Parameters["STRIKEPRICE"].Value = Convert.ToDecimal(strikePrice);
                                queryParameters = m_QueryExistAssetOpt;
                            }

                            using (IDataReader drAsset = DataHelper.ExecuteReader(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                            {
                                // L'Asset n'existe pas dans la database
                                //      --> on ne recopie pas la ligne 
                                // Dans le cas où l'Asset existe, 
                                //      --> on recopie la ligne                           
                                isToCopyOk = drAsset.Read();
                            }
                            #endregion
                        }
                    }

                    if (isToCopyOk)
                    {
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
        #endregion
    }
}
