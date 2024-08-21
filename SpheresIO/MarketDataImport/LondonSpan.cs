using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// 
    /// </summary>
    internal class MarketDataImportLondonSPAN : MarketDataImportBase
    {
        #region Private class
        // PM 20151218 [21593] Add class LondonSPANTools
        private class LondonSPANTools
        {
            #region Members
            private readonly List<ExchangeInfo> m_ExchangeInfo = new List<ExchangeInfo>();
            #endregion Members
            #region Methods
            /// <summary>
            /// Ajoute un CombinedContract dans l'ensemble des CombinedContract présents
            /// </summary>
            /// <param name="pEchangeCode"></param>
            /// <param name="pCombinedContractCode"></param>
            public void AddCombinedContract(string pEchangeCode, string pCombinedContractCode)
            {
                ExchangeInfo.Add(m_ExchangeInfo, pEchangeCode, pCombinedContractCode);
            }
            /// <summary>
            /// Indique si des elements ont déjà été ajoutés
            /// </summary>
            /// <returns></returns>
            public bool HasElement()
            {
                return (m_ExchangeInfo.Count > 0);
            }
            /// <summary>
            /// Indique si un CombinedContract existe dans l'ensemble des CombinedContract présents
            /// </summary>
            /// <param name="pEchangeCode"></param>
            /// <param name="pCombinedContractCode"></param>
            /// <returns></returns>
            public bool ContainsCombinedContract(string pEchangeCode, string pCombinedContractCode)
            {
                bool ret = ExchangeInfo.Contains(m_ExchangeInfo, pEchangeCode, pCombinedContractCode);
                return ret;
            }
            /// <summary>
            /// Indique si la ligne de type 14 doit être importée
            /// </summary>
            /// <param name="pFileFormat"></param>
            /// <param name="pLine"></param>
            /// <returns></returns>
            public bool IsRecord14ToImport(int pFileFormat, string pLine)
            {
                bool isRecordToImport = false;
                if (StrFunc.IsFilled(pLine) && (pLine.Length > 2) && (pLine.Substring(0,2) == "14"))
                {
                    int blocSize = 0;
                    int startPos = 0;
                    int startComContractPos = 3;
                    int exchangeCodeSize = 3;
                    int combinedContractSize = 3;

                    switch (pFileFormat)
                    {
                        case 3:
                            // Position et taille des blocs Exchange Code (Position 26 Longueur 3) / Combined Contract (Position 29 Longueur 3) / ... - 
                            blocSize = 9;
                            startPos = 25;
                            break;
                        case 4:
                            // Position et taille des blocs Exchange Code (Position 26 Longueur 3) / Combined Contract (Position 29 Longueur 3) / ... - 
                            blocSize = 9;
                            startPos = 25;
                            break;
                        case 5:
                            // Position et taille des blocs Exchange Code (Position 29 Longueur 3) / Combined Contract (Position 32 Longueur 3) / ... - 
                            blocSize = 11;
                            startPos = 28;
                            break;
                        case 6:
                            // Position et taille des blocs Exchange Code (Position 29 Longueur 3) / Combined Contract (Position 32 Longueur 3) / ... - 
                            blocSize = 14;
                            startPos = 28;
                            break;
                        default:
                            isRecordToImport = true;
                            break;
                    }
                    if ((false == isRecordToImport) && (pLine.Length >= (startPos + blocSize)))
                    {
                        bool isGoOnChecking = true;
                        isRecordToImport = true;
                        pLine = pLine.Substring(startPos);
                        while (isGoOnChecking && (pLine.Length >= blocSize))
                        {
                            string spreadExchangeCode = pLine.Substring(0, exchangeCodeSize).Trim();
                            string spreadCombinedContractCode = pLine.Substring(startComContractPos, combinedContractSize).Trim();
                            isRecordToImport = ContainsCombinedContract(spreadExchangeCode, spreadCombinedContractCode);
                            // Pour l'ICE on test tous les CombinedContract, pour les autres marchés dés qu'on en trouve un on importe la ligne
                            if ((spreadExchangeCode != "I") && isRecordToImport)
                            {
                                isGoOnChecking = false;
                            }
                            pLine = pLine.Substring(blocSize);
                        }
                    }
                }
                return isRecordToImport;
            }
            #endregion Methods
        }

        // PM 20151218 [21593] Add class ExchangeInfo
        private class ExchangeInfo
        {
            #region Members
            private string m_ExchangeCode;
            private readonly List<string> m_CombinedContract = new List<string>();
            #endregion Members

            #region Methods
            /// <summary>
            /// Ajout d'un couple ExchangeCode/CombinedContractCode dans pExchangeList
            /// </summary>
            /// <param name="pExchangeList"></param>
            /// <param name="pExchangeCode"></param>
            /// <param name="pCombinedContractCode"></param>
            static public void Add(List<ExchangeInfo> pExchangeList, string pExchangeCode, string pCombinedContractCode)
            {
                if ((pExchangeList != default(List<ExchangeInfo>)) && StrFunc.IsFilled(pExchangeCode) && StrFunc.IsFilled(pCombinedContractCode))
                {
                    ExchangeInfo exchange = pExchangeList.FirstOrDefault( e => e.m_ExchangeCode == pExchangeCode);
                    if (exchange == default(ExchangeInfo))
                    {
                        exchange = new ExchangeInfo
                        {
                            m_ExchangeCode = pExchangeCode
                        };
                        pExchangeList.Add(exchange);
                    }
                    if (false == exchange.m_CombinedContract.Contains(pCombinedContractCode))
                    {
                         exchange.m_CombinedContract.Add(pCombinedContractCode);
                    }
                }
            }

            /// <summary>
            /// Indique si le couple ExchangeCode/CombinedContractCode est contenu dans pExchangeList
            /// </summary>
            /// <param name="pExchangeList"></param>
            /// <param name="pExchangeCode"></param>
            /// <param name="pCombinedContractCode"></param>
            /// <returns></returns>
            static public bool Contains(List<ExchangeInfo> pExchangeList, string pExchangeCode, string pCombinedContractCode)
            {
                bool contains = false;
                if (pExchangeList != default(List<ExchangeInfo>))
                {
                    ExchangeInfo exchInfo = pExchangeList.FirstOrDefault(e => e.m_ExchangeCode == pExchangeCode);
                    if (exchInfo != default(ExchangeInfo))
                    {
                        contains = exchInfo.m_CombinedContract.Contains(pCombinedContractCode);
                    }
                }
                return contains;
            }
            #endregion Methods
        }
        #endregion Private class

        #region Members
        /// <summary>
        /// Nombre max d'itération lors du parcourt des lignes du fichier SPAN
        /// </summary>
        /// PM 20151218 [21593] Add (en remplacement de variable locale)
        private const int m_TheoreticalPriceFileGuard = 9999999; //545663 reccords dans le fichier du 20120106

        /// <summary>
        /// Pour lister les Combined Contract avec des Contract en position
        /// </summary>
        /// PM 20151218 [21593] Add
        private readonly LondonSPANTools m_SpanElementInPos = new LondonSPANTools();
        #endregion

        #region Constructor
        public MarketDataImportLondonSPAN(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, true, false, AssetFindMaturityEnum.MATURITYMONTHYEAR)
        {
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Obtenir EXCHANGESYMBOL à partir de {pFinancialMarketCode}
        /// <para>Si {pFinancialMarketCode} = 'X' (LCP/NFP) alors considérer le marché LIFFE (EXCHANGESYMBOL='L')</para>
        /// <para>Sinon ({pFinancialMarketCode}='L' (LIFFE),{pFinancialMarketCode} = 'O' (LTOM) et {pFinancialMarketCode}='I' (ICE)) considérer EXCHANGESYMBOL={pFinancialMarketCode}</para>
        /// </summary>
        /// <param name="pFinancialMarketCode"></param>
        /// <returns></returns>
        private static string GetExchangeSymbol(string pFinancialMarketCode)
        {
            string exchangeSymbol = pFinancialMarketCode;

            //FL 20130423 : Maintenant que tous les DC sur equity du LIFFE initialement intégré sur le marché LIFFE (Marché ayant comme Exchange Symbol : ‘L’)
            //  sont dans un segment de marché du LIFFE ayant comme Exchange Symbol 'O', la transformation de l’exchange Symbol ‘O’ en ‘L’ n’est plus à faire.
            //if (pFinancialMarketCode == "O" || pFinancialMarketCode == "X")
            //    exchangeSymbol = "L";

            //FL 20140312[19711]: Suite à la gestion du segment de marché LCP - EURONEXT.LIFFE COMMODITY PRODUCT(Exchange Symbol 'X')
            //                    la transformation de l’exchange Symbol ‘X’ en ‘L’ n’est plus à faire.
            //if (pFinancialMarketCode == "X")
            //    exchangeSymbol = "L";

            return exchangeSymbol;
        }

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Market Data LIFFE
        /// </summary>
        /// <param name="pOutputFileName"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightMarketDataFile(string pOutputFileName)
        {
            #region Filtrer le fichier source
            try
            {
                #region Declare
                string dcKey = string.Empty;
                Pair<string, bool> lastDC = new Pair<string, bool>(string.Empty, false);
                string dc_EXCHANGESYMBOL = string.Empty;
                string dc_CONTRACTSYMBOL = string.Empty;
                string dc_CATEGORY = string.Empty;
                string recordCode;
                string currentLine;
                bool isToCopyOk;
                // Les trois variables suivantes, c'est pour stocker les lignes déjà lues avec recordCode inférieur à 60, 
                // avec un indicateur si la ligne est déjà recopiée ou pas
                Pair<string, bool> record30 = null;
                Pair<string, bool> record40 = null;
                Pair<string, bool> record50 = null;
                #endregion

                //Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);
                //Ouverture du fichier d'entrée LIFFE d'origine
                OpenInputFileName();

                int guard = 9999999; //545663 reccords dans le fichier du 20120106
                int lineNumber = 0;
                bool isCategoryOk = false;
                while (++lineNumber < guard)
                {
                    isToCopyOk = false;
                    //
                    if ((lineNumber % 5000) == 0)
                        System.Diagnostics.Debug.WriteLine(lineNumber);

                    //Lecture d'une ligne dans le fichier d'entrée LIFFE d'origine
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

                    // On récupère le Record Code de l’enregistrement du fichier d’entrée (Position 1 Longueur 2)
                    recordCode = currentLine.Substring(0, 2).Trim();

                    #region recordCode "10" et "20"
                    //  Si: 
                    //      - RecordCode="10", il s'agit de la ligne d'information (Header)
                    //        ou bien RecordCode="20", il s'agit de la ligne qui contient le "Financial market code (MC)", équivalent à MARKET.EXCHANGECODE
                    //      --> On recopie la ligne
                    //  Sinon:
                    //      Voir autres cas ci-dessous
                    isToCopyOk = ((recordCode == "10") || (recordCode == "20"));

                    // On récupère: Financial market code (MC) - (Position 3 Longueur 3)
                    if (recordCode == "20")
                        dc_EXCHANGESYMBOL = GetExchangeSymbol(currentLine.Substring(2, 3).Trim());

                    #endregion

                    if (isToCopyOk == false)
                    {
                        #region lignes "30", "40", "50" et "60"
                        //  Si: 
                        //      - la ligne commence par "30", "40", "50" ou "60" il s'agit des lignes contiennent des données MARKETDATA
                        //      --> On traite la ligne pour vérifier si elle est à copier ou pas
                        //  Sinon:
                        //      --> On ignore la ligne
                        if ((recordCode == "30") || (recordCode == "40") || (recordCode == "50") || (recordCode == "60"))
                        {
                            // Stocker la ligne déjà lue avec recordCode inférieur à 60, 
                            // avec un indicateur si la ligne est déjà recopiée ou pas à false
                            switch (recordCode)
                            {
                                case "30":
                                    record30 = new Pair<string, bool>(currentLine, false);
                                    record40 = null;
                                    record50 = null;
                                    break;
                                case "40":
                                    record40 = new Pair<string, bool>(currentLine, false);
                                    record50 = null;
                                    #region Check DERIVATIVECONTRACT
                                    // On récupère: Combined Family Type (CF) - (Position 6 Longueur 1)
                                    // à partire de la ligne avec recordCode="40" en cours
                                    //
                                    // Valeurs possibles: 
                                    //  F		Future
                                    //  I		Index
                                    //  S		Stock
                                    //  U		Underlying future
                                    //  O		Future/index/stock
                                    //  A		Average of forwards
                                    //  M		Monthly forward
                                    //  D		Daily forward
                                    //
                                    // On garde que les valeurs 'F' et 'O'
                                    dc_CATEGORY = record40.First.Substring(5, 1).Trim();

                                    isCategoryOk = ((dc_CATEGORY == "F") || (dc_CATEGORY == "O"));

                                    if (isCategoryOk)
                                    {
                                        // On récupère: Family Code (FC) - (Position 3 Longueur 3)
                                        // à partire de la ligne avec recordCode="40" en cours
                                        dc_CONTRACTSYMBOL = record40.First.Substring(2, 3).Trim();

                                        dcKey = dc_EXCHANGESYMBOL + "~" + dc_CONTRACTSYMBOL + "~" + dc_CATEGORY;

                                        if (dcKey != lastDC.First)
                                        {
                                            lastDC = new Pair<string, bool>(dcKey, false);

                                            // Recherche d'existence du DC dans la database
                                            m_QueryExistDC.Parameters["EXCHANGESYMBOL"].Value = dc_EXCHANGESYMBOL;
                                            m_QueryExistDC.Parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                            m_QueryExistDC.Parameters["CATEGORY"].Value = dc_CATEGORY;

                                            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistDC.Query, m_QueryExistDC.Parameters.GetArrayDbParameter()))
                                            {
                                                if (dr.Read())
                                                {
                                                    lastDC.Second = BoolFunc.IsTrue(dr["ISAUTOSETTINGASSET"]);

                                                    //  On vérifie si le DC est en ISAUTOSETTING=True (donc maj des caractéristiques du DC)                                                //
                                                    //  Si: 
                                                    //      - ISAUTOSETTING=True (maj des caractéristiques du DC), 
                                                    //      --> on recopie les lignes 30, 40
                                                    //  Sinon:
                                                    //      --> C'est en fonction de ISAUTOSETTINGASSET et de l'existence de l'Asset ou pas.
                                                    isToCopyOk = Convert.ToBoolean(dr["ISAUTOSETTING"]);
                                                }
                                                else
                                                {
                                                    // Le DC n'existe pas
                                                    //      --> on recopie les lignes 30, 40, 50 et 60
                                                    //          et on "ignorera" toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL + CATEGORY) 
                                                    isToCopyOk = true;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    break;
                                case "50":
                                    // Si la Catégory a l'une des deux valeurs 'F' ou 'O'
                                    // Si le DC est avec ISAUTOSETTINGASSET=True
                                    if (isCategoryOk && lastDC.Second)
                                        record50 = new Pair<string, bool>(currentLine, false);
                                    break;
                                case "60":
                                    // Si la Catégory a l'une des deux valeurs 'F' ou 'O'
                                    // Le DC existe dans la database
                                    // Si: 
                                    //      - ISAUTOSETTINGASSET=True (donc création des ASSET_ETD)
                                    //      --> On recopie les lignes 30, 40, 50 et 60, si l'ASSET_ETD qu'elles contiennent n'existe pas dans la database
                                    //          et on fera de même pour toutes les lignes suivantes de même key (EXCHANGESYMBOL + CONTRACTSYMBOL + CATEGORY)
                                    if (isCategoryOk && lastDC.Second)
                                    {
                                        #region Check ASSET_ETD
                                        // Recherche d'existence de l'ASSET dans la database
                                        SetQueryExistAssetParameter(dc_CATEGORY, "EXCHANGESYMBOL", dc_EXCHANGESYMBOL);
                                        SetQueryExistAssetParameter(dc_CATEGORY, "CONTRACTSYMBOL", dc_CONTRACTSYMBOL);
                                        SetQueryExistAssetParameter(dc_CATEGORY, "CATEGORY", dc_CATEGORY);
                                        // On récupère: Expiry code (EC) - (Position 3 Longueur 6)
                                        // à partire de la ligne avec recordCode="50" en cours
                                        SetQueryExistAssetParameter(dc_CATEGORY, "MATURITYMONTHYEAR", record50.First.Substring(2, 6).Trim());


                                        QueryParameters queryParameters = m_QueryExistAssetFut;

                                        if (dc_CATEGORY == "O")
                                        {
                                            // On récupère: Strike Price (SP) - (Position 3 Longueur 8)
                                            // à partire de la ligne avec recordCode="60" en cours
                                            string strikePrice = currentLine.Substring(2, 8).Trim();
                                            // On récupère: Strike Price Decimal Locator (SD) - (Position 64 Longueur 6)
                                            // à partire de la ligne avec recordCode="40" en cours
                                            string strikePriceDivisor = record40.First.Substring(63, 6).Trim();
                                            // STRIKEPRICE = SP / (10 ^ SD)
                                            m_QueryExistAssetOpt.Parameters["STRIKEPRICE"].Value = GetStrikePrice(strikePrice, strikePriceDivisor);

                                            // On récupère: Combined Family Type (CF) - (Position 11 Longueur 2)
                                            // à partire de la ligne avec recordCode="60" en cours
                                            // valeurs possibles: 'P' et 'C'
                                            m_QueryExistAssetOpt.Parameters["PUTCALL"].Value = GetPutCall(currentLine.Substring(10, 2).Trim());
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
                                    break;
                            }
                        }
                        #endregion
                    }

                    if (isToCopyOk)
                    {
                        //Ecriture des lignes dans le fichier de sortie "Light"                                
                        if (recordCode == "40") // La ligne du DC
                        {
                            // Ecrire d'abord les lignes déjà lues avec recordCode inférieur à 40, 
                            // et qui n'ont pas été encore recopiée                            
                            if ((record30 != null) && (record30.Second == false))
                            {
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(record30.First);
                                IOCommonTools.StreamWriter.WriteLine(record30.First);
                                record30.Second = true;
                            }

                            // record40 est la ligne en cours
                            record40.Second = true;
                        }

                        //Ecriture des lignes dans le fichier de sortie "Light"                                
                        if (recordCode == "60") // La ligne de l'Asset
                        {
                            // Ecrire d'abord les lignes déjà lues avec recordCode inférieur à 60, 
                            // et qui n'ont pas été encore recopiée                            
                            if ((record30 != null) && (record30.Second == false))
                            {
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(record30.First);
                                IOCommonTools.StreamWriter.WriteLine(record30.First);
                                record30.Second = true;
                            }
                            if ((record40 != null) && (record40.Second == false))
                            {
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(record40.First);
                                IOCommonTools.StreamWriter.WriteLine(record40.First);
                                record40.Second = true;
                            }
                            if ((record50 != null) && (record50.Second == false))
                            {
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(record50.First);
                                IOCommonTools.StreamWriter.WriteLine(record50.First);
                                record50.Second = true;
                            }
                        }

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
        /// Création d'un fichier "Light" pour l'importation des cotations et des paramètres de risque
        /// </summary>
        /// <param name="pOutputFileName"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightTheoreticalPriceFile(string pOutputFileName)
        {
            #region Filtrer le fichier source
            try
            {
                #region Declare
                // PM 20151218 [21593] Add fileFormatVersion, combinedContractCode, isExistCombinedContract
                int fileFormatVersion = 0;
                string combinedContractCode = string.Empty;
                bool isExistCombinedContract = false;

                //PM 20140702 [20163][20157] Add file_EXCHANGEACRONYM
                string file_EXCHANGEACRONYM = string.Empty;

                string market_EXCHANGEACRONYM = string.Empty;
                string dc_CONTRACTSYMBOL = string.Empty;
                string dc_CATEGORY = string.Empty;
                string maturity_MATURITYMONTHYEAR = string.Empty;
                string asset_PUTCALL = string.Empty;
                decimal asset_STRIKEPRICE = 0;
                
                bool isExistDerivativeContract = false;
                bool isExistMaturity = false;
                bool isExistUnderlying = false;
                // PM 20151218 [21593] recordSaved n'est plus utilisé
                //List<string> recordSaved = new List<string>(); // Permet de stocker les lignes en vues de leurs éventuelles utilisation ultérieures
                #endregion

                // PM 20151218 [21593] Faire une première passe de lecture afin de vérifier ce qu'il y a à importer.
                bool isExistPos = ReadTheoreticalPriceFileFirstPass();

                //Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);

                //Ouverture du fichier d'entrée London SPAN d'origine
                OpenInputFileName();

                // PM 20151218 [21593] Remplacement du "guard" par "m_TheoreticalPriceFileGuard"
                //int guard = 9999999; //545663 reccords dans le fichier du 20120106
                int lineNumber = 0;

                // PM 20151218 [21593] Remplacement du "guard" par "m_TheoreticalPriceFileGuard"
                //while (++lineNumber < guard)
                while (++lineNumber < m_TheoreticalPriceFileGuard)
                {
                    if ((lineNumber % 5000) == 0)
                        System.Diagnostics.Debug.WriteLine(lineNumber);

                    //Lecture d'une ligne dans le fichier d'entrée LIFFE d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();

                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + m_TheoreticalPriceFileGuard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }

                    #region Process Ligne par ligne

                    // FI 20130419 [18587] Importation LME en erreur
                    // Spheres® teste désormais que la ligne est renseignée
                    // Lorsque les fichiers sont issus d'un système Unix, OD OD OA donne niassance à 2 retours chariots et non 1 seul
                    bool isToCopyOk = currentLine.Length > 0;
                    if (isToCopyOk)
                    {
                        // On récupère le Record Code de l’enregistrement du fichier d’entrée (Position 1 Longueur 2)
                        string recordCode = currentLine.Substring(0, 2).Trim();

                        // PM 20150806 [] Ajout Record 14, 21, 36, 37
                        switch (recordCode)
                        {
                            case "10": // SPAN File Header Record
                                // On récupère la Business Date (CCYYMMDD) - (Position 6 Longueur 8)
                                m_dtFile = new DtFunc().StringyyyyMMddToDateTime(currentLine.Substring(5, 8));

                                // PM 20151218 [21593] Lecture de Format Version - (Position 4 Longueur 2)
                                fileFormatVersion = IntFunc.IntValue(currentLine.Substring(3, 2).Trim());

                                isToCopyOk = true;
                                break;
                            case "14": // Inter-contract Spread Details
                                //PM 20151218 [21593] Ne pas copier systématiquement
                                //isToCopyOk = true;
                                if (isExistPos)
                                {
                                    isToCopyOk = m_SpanElementInPos.IsRecord14ToImport(fileFormatVersion, currentLine);
                                }
                                else
                                {
                                    isToCopyOk = false;
                                }
                                break;
                            case "20": // Exchange Details
                                // On récupère: Financial market code (MC) - (Position 3 Longueur 3)
                                market_EXCHANGEACRONYM = GetExchangeSymbol(currentLine.Substring(2, 3).Trim());
                                //PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY
                                file_EXCHANGEACRONYM = market_EXCHANGEACRONYM;
                                //
                                isToCopyOk = true;
                                break;
                            case "21": // Position Split Allocation Details
                                //PM 20151218 [21593] Ne pas copier systématiquement mais uniquement s'il existe des positions
                                //isToCopyOk = true;
                                isToCopyOk = isExistPos;
                                break;
                            case "30": // Combined Contract Details
                                //PM 20151218 [21593] Copier en fonction de isExistCombinedContract = m_SpanElementInPos.ContainsCombinedContract
                                //// Sauvegarder toutes les lignes concernant les groupes combinés au cas ou elles doivent être insérées dans le fichier light
                                //recordSaved.Clear();
                                //recordSaved.Add(currentLine);
                                //isExistDerivativeContract = false;
                                //isToCopyOk = false;

                                // Lecture de: Combined Contract Code - (Position 3 Longueur 3)
                                combinedContractCode = currentLine.Substring(2, 3).Trim();
                                isExistCombinedContract = m_SpanElementInPos.ContainsCombinedContract(file_EXCHANGEACRONYM, combinedContractCode);
                                isToCopyOk = isExistCombinedContract;

                                break;
                            case "31": // Month Tier Details
                            case "32": // Leg Spread Details
                            case "33": // Prompt Date Charge Details
                            case "34": // Intercontract Tier Details
                            case "35": // Strategy Spread Details
                            case "36": // Customer Margin Ratios
                            case "37": // ???
                                //PM 20151218 [21593] Copier en fonction de isExistCombinedContract
                                //// Sauvegarder toutes les lignes concernant les groupes combinés au cas ou elles doivent être insérées dans le fichier light
                                //recordSaved.Add(currentLine);
                                //isToCopyOk = false;

                                isToCopyOk = isExistCombinedContract;
                                break;
                            case "40":  // Contract Details
                                //PM 20151218 [21593] Ajout teste sur isExistCombinedContract
                                if (isExistCombinedContract)
                                {
                                    #region Check DERIVATIVECONTRACT
                                    //PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY
                                    market_EXCHANGEACRONYM = file_EXCHANGEACRONYM;
                                    //
                                    // On récupère: Combined Family Type (CF) - (Position 6 Longueur 1)
                                    // à partire de la ligne avec recordCode="40" en cours
                                    //
                                    // Valeurs possibles: 
                                    //  F		Future
                                    //  I		Index
                                    //  S		Stock
                                    //  U		Underlying future
                                    //  O		Option on Future/index/stock
                                    //  A		Average of forwards
                                    //  M		Monthly forward
                                    //  D		Daily forward
                                    //
                                    // On garde que les valeurs 'F','O','M' et 'D'
                                    dc_CATEGORY = currentLine.Substring(5, 1).Trim();
                                    // PM/FL 20150603[20398][21088]: Gestion des DC ayant un Combined Family Type =  M -> Monthly forward ou D -> Daily forward
                                    //                               Ps. Ces DC sont considérés comme des DC de category 'Future'.
                                    if (dc_CATEGORY == "M" || dc_CATEGORY == "D")
                                    {
                                        dc_CATEGORY = "F";
                                    }

                                    // On récupère: Family Code (FC) - (Position 3 Longueur 3)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(2, 3).Trim();

                                    isExistUnderlying = false;
                                    switch (dc_CATEGORY)
                                    {
                                        case "F":
                                        case "O":
                                            // isExistDerivativeContract == true => le DC courant a été négocié ou le DC courant est un DC future ss-jacent d'un DC Option négocié
                                            isExistDerivativeContract = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, dc_CATEGORY);
                                            //PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY
                                            if ((false == isExistDerivativeContract) && ("O" == market_EXCHANGEACRONYM))
                                            {
                                                market_EXCHANGEACRONYM = "L";
                                                isExistDerivativeContract = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, dc_CATEGORY);
                                            }
                                            // FI 20211115 [XXXX] valorisation de isExistUnderlying
                                            if (dc_CATEGORY == "F")
                                            {
                                                //isExistUnderlying == true => DC future courant est ss-jacent d'un DC Option négocié
                                                isExistUnderlying = IsExistDcUnderlyingInTrade(Cs, Cst.UnderlyingAsset.Future, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, "F");
                                            }
                                            break;
                                        case "I": // Index
                                            isExistUnderlying = IsExistDcUnderlyingInTrade(Cs, Cst.UnderlyingAsset.Index, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                            isExistDerivativeContract = false;
                                            break;
                                        case "S": // Stock
                                            isExistUnderlying = IsExistDcUnderlyingInTrade(Cs, Cst.UnderlyingAsset.EquityAsset, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                            isExistDerivativeContract = false;
                                            break;
                                        case "U": // Underlying
                                        default:
                                            isExistDerivativeContract = false;
                                            break;
                                    }
                                    #endregion
                                    isToCopyOk = (isExistDerivativeContract || isExistUnderlying);
                                }
                                else
                                {
                                    isExistDerivativeContract = false;
                                    isExistUnderlying = false;
                                    isToCopyOk = false;
                                }
                                break;
                            case "50": // Contract Expiry Details
                                if (isExistDerivativeContract && (false == isExistUnderlying)) // FI 20211115 [XXXX] si le DC future courant est ss-jacent d'un DC Option négocié => la ligne est copiée. Il 'est inutile de tester que l'échéance courante est négociée)
                                {
                                    // On récupère: Expiry Date - (Position 3 Longueur 8)
                                    maturity_MATURITYMONTHYEAR = currentLine.Substring(2, 8).Trim();
                                    if (maturity_MATURITYMONTHYEAR.Substring(6) == "00")
                                        maturity_MATURITYMONTHYEAR = maturity_MATURITYMONTHYEAR.Substring(0, 6);

                                    m_QueryExistDCMaturityInTrade.Parameters["DTFILE"].Value = m_dtFile;
                                    m_QueryExistDCMaturityInTrade.Parameters["EXCHANGEACRONYM"].Value = market_EXCHANGEACRONYM;
                                    m_QueryExistDCMaturityInTrade.Parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    m_QueryExistDCMaturityInTrade.Parameters["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                    m_QueryExistDCMaturityInTrade.Parameters["CATEGORY"].Value = dc_CATEGORY;

                                    object obj_DC_Maturity_Trade = DataHelper.ExecuteScalar(Cs, CommandType.Text, m_QueryExistDCMaturityInTrade.Query, m_QueryExistDCMaturityInTrade.Parameters.GetArrayDbParameter());

                                    isExistMaturity = (obj_DC_Maturity_Trade != null);
                                }
                                else
                                {
                                    isExistMaturity = false;
                                }
                                isToCopyOk = (isExistMaturity || isExistUnderlying);
                                break;
                            case "60": // Series Details (Risk Array Record)
                                if (isExistMaturity && (dc_CATEGORY == "O"))
                                {
                                    // On récupère: Strike Price - (Position 3 Longueur 8)
                                    asset_STRIKEPRICE = Convert.ToDecimal(currentLine.Substring(2, 8).Trim());
                                    // On récupère: Contract Type - (Position 11 Longueur 2)
                                    asset_PUTCALL = GetPutCall(currentLine.Substring(10, 2).Trim());

                                    QueryParameters query = QueryTrade(Cs);
                                    DataParameters parameters = query.Parameters;

                                    parameters["DTFILE"].Value = m_dtFile;
                                    parameters["EXCHANGEACRONYM"].Value = market_EXCHANGEACRONYM;
                                    parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                    parameters["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                    parameters["CATEGORY"].Value = dc_CATEGORY;
                                    parameters["PUTCALL"].Value = asset_PUTCALL;
                                    parameters["STRIKEPRICE"].Value = asset_STRIKEPRICE;

                                    object obj = DataHelper.ExecuteScalar(Cs, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
                                    isToCopyOk = (obj != null);
                                }
                                else
                                {
                                    isToCopyOk = (isExistMaturity || isExistUnderlying);
                                }
                                break;
                            default:
                                isToCopyOk = true;
                                break;
                        }
                    }

                    if (isToCopyOk)
                    {
                        // PM 20151218 [21593] recordSaved n'est plus utilisé
                        //if ((isExistUnderlying || isExistDerivativeContract) && (recordSaved.Count > 0))
                        //{
                        //    // Ecriture de toutes les lignes de série 3 sauvegardées
                        //    foreach (string line in recordSaved)
                        //    {
                        //        IOTools.StreamWriter.WriteLine(line);
                        //    }
                        //    recordSaved.Clear();
                        //}

                        //Ecriture de la ligne dans le fichier de sortie "Light"
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StreamWriter.WriteLine(currentLine);
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                    }
                    #endregion
                }
                // PM 20151218 [21593] recordSaved n'est plus utilisé
                //recordSaved.Clear();
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
        /// Retourne la requête qui permet de vérifier l'existance d'au moins un trade à partir d'un Asset Option
        /// </summary>
        /// <returns></returns>
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryTrade(string pCS)
        {
            string query = $@"select asset.IDASSET
            from dbo.TRADE tr
            inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ma.IDMATURITYRULE)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM  = dc.IDM)
            where (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE) and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and
            (((mr.MMYFMT = '{ReflectionTools.ConvertEnumToString<Cst.MaturityMonthYearFmtEnum>(Cst.MaturityMonthYearFmtEnum.YearMonthOnly)}') and (ma.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 ))) or (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)) and
            (dc.CATEGORY = @CATEGORY) and (asset.PUTCALL = @PUTCALL) and (asset.STRIKEPRICE = (@STRIKEPRICE / power(10, isnull( dc.STRIKEDECLOCATOR, 0 ) ) ) )" + Cst.CrLf;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CATEGORY));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
            dataParameters.Add(new DataParameter(pCS, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            dataParameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal));

            QueryParameters qryParameters = new QueryParameters(pCS, query, dataParameters);
            return qryParameters;

        }

        /// <summary>
        /// Effectue une première passe de lecture afin de vérifier ce qu'il y a à importer.
        /// Construit l'ensemble des CombinedContract à importer
        /// </summary>
        /// <returns>True s'il existe des positions et donc des données à importer, sinon False</returns>
        /// PM 20151218 [21593] New
        private bool ReadTheoreticalPriceFileFirstPass()
        {
            bool retExistPos = false;
            try
            {
                // Indicateurs
                bool isContinueReadFile = true;
                bool isExistPosOnClearing = false;
                bool isExistUnderlying = false;
                bool isExistDerivativeContract = false;
                bool isExistCombinedContract = false;
                //
                // Données lues
                string exchangeCode = string.Empty;
                string combinedContractCode = string.Empty;
                string dcCategory = string.Empty;
                string dcContractSymbol = string.Empty;

                //Ouverture du fichier d'entrée London SPAN d'origine
                OpenInputFileName();
                //
                int lineNumber = 0;
                while (isContinueReadFile && ((lineNumber++) < m_TheoreticalPriceFileGuard))
                {
                    if ((lineNumber % 5000) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine(lineNumber);
                    }
                    // Lecture d'une ligne dans le fichier d'entrée d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    //
                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + m_TheoreticalPriceFileGuard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }
                    //
                    if (currentLine.Length > 0)
                    {
                        // On récupère le Record Code de l’enregistrement du fichier d’entrée (Position 1 Longueur 2)
                        string recordCode = currentLine.Substring(0, 2).Trim();

                        // PM 20150806 [] Ajout Record 14, 21, 36, 37
                        switch (recordCode)
                        {
                            case "10": // SPAN File Header Record
                                // Lecture de: Business Date (CCYYMMDD) - (Position 6 Longueur 8)
                                m_dtFile = new DtFunc().StringyyyyMMddToDateTime(currentLine.Substring(5, 8));
                                break;
                            case "14": // Intercontract Spread Details
                                break;
                            case "20": // Exchange Details
                                // Lecture de: Exchange Code - (Position 3 Longueur 3)
                                exchangeCode = currentLine.Substring(2, 3).Trim();
                                // Pour le marché ICE on test uniquement sur ce marché
                                if (exchangeCode == "I")
                                {
                                    // Test s'il existe des pos sur le marché
                                    isExistPosOnClearing = IsExistExchangeInTrade(Cs, m_dtFile, exchangeCode);
                                }
                                else
                                {
                                    // Test s'il existe des pos sur un des marchés de la chambre
                                    isExistPosOnClearing = IsExistClearingInTrade(Cs, m_dtFile, exchangeCode);
                                }

                                // On continue à lire le fichier en fonction qu'il y a des pos ou non
                                isContinueReadFile = isExistPosOnClearing;

                                //// Temporraire en attente de fin du développement
                                //nbContract = isExistPosOnClearing ? 1 : 0;
                                //isContinueReadFile = false;
                                break;
                            case "30": // Combined Contract Details
                                // Lecture de: Combined Contract Code - (Position 3 Longueur 3)
                                combinedContractCode = currentLine.Substring(2, 3).Trim();
                                isExistCombinedContract = false;
                                break;
                            case "40":  // Contract Details
                                if (isExistCombinedContract == false)
                                {
                                    #region Check DERIVATIVECONTRACT
                                    // Combined Family Type (CF) - (Position 6 Longueur 1)
                                    //
                                    // Valeurs possibles: 
                                    //  F		Future
                                    //  I		Index
                                    //  S		Stock
                                    //  U		Underlying future
                                    //  O		Option on Future/index/stock
                                    //  A		Average of forwards
                                    //  M		Monthly forward
                                    //  D		Daily forward
                                    //
                                    dcCategory = currentLine.Substring(5, 1).Trim();
                                    // Les DC ayant un Combined Family Type =  M -> Monthly forward ou D -> Daily forward
                                    // sont considérés comme des DC de category 'Future'.
                                    if ((dcCategory == "M") || (dcCategory == "D"))
                                    {
                                        dcCategory = "F";
                                    }

                                    // Family Code (FC) - (Position 3 Longueur 3)
                                    dcContractSymbol = currentLine.Substring(2, 3).Trim();

                                    isExistUnderlying = false;
                                    switch (dcCategory)
                                    {
                                        case "F":
                                        case "O":
                                            isExistDerivativeContract = IsExistDcInTrade(Cs, m_dtFile, exchangeCode, dcContractSymbol, dcCategory);
                                            // Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY
                                            if ((false == isExistDerivativeContract) && ("O" == exchangeCode))
                                            {
                                                isExistDerivativeContract = IsExistDcInTrade(Cs, m_dtFile, "L", dcContractSymbol, dcCategory);
                                            }
                                            break;
                                        case "I": // Index
                                            isExistUnderlying = IsExistDcUnderlyingInTrade(Cs, Cst.UnderlyingAsset.Index, m_dtFile, exchangeCode, dcContractSymbol);
                                            isExistDerivativeContract = false;
                                            break;
                                        case "S": // Stock
                                            isExistUnderlying = IsExistDcUnderlyingInTrade(Cs, Cst.UnderlyingAsset.EquityAsset, m_dtFile, exchangeCode, dcContractSymbol);
                                            isExistDerivativeContract = false;
                                            break;
                                        case "U": // Underlying
                                        default:
                                            isExistDerivativeContract = false;
                                            break;
                                    }
                                    #endregion
                                    if (isExistDerivativeContract || isExistUnderlying)
                                    {
                                        m_SpanElementInPos.AddCombinedContract(exchangeCode, combinedContractCode);
                                        isExistCombinedContract = true;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                retExistPos = m_SpanElementInPos.HasElement();
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer tous les fichiers
                CloseAllFiles();
            }
            return retExistPos;
        }

        #endregion
    }
}
