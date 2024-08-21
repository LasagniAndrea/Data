using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;
using System.Diagnostics;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;

namespace EFS.SpheresIO.MarketData
{

    /// <summary>
    ///  Classe de gestion des fichiers issues du marché CBOE
    /// </summary>
    internal class MarketDataImportCBOE : MarketDataImportBase
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportCBOE(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, true, false, AssetFindMaturityEnum.MATURITYMONTHYEAR)
        {
            m_dtFile = new DtFunc().StringyyyyMMddToDateTime(Path.GetFileNameWithoutExtension(pDataName).Substring(5)); //Nom du fichier : "cboe.yyyyMMdd.txt"
        }
        #endregion constructor

        #region methods
        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des RiskData CBOE
        /// </summary>
        /// <param name="pOutputFileName"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public void Create_LightTheoreticalPriceFile(string pOutputFileName)
        {
            try
            {
                string exchangeAcronym = "XUSS";
                string iso10383 = GetISO183803FromExchangeAcronym(exchangeAcronym);
                string category = "O";
                string currentContractSymbol = string.Empty;
                string currentMaturityDate = string.Empty;
                bool isCurrentDcInTrade = false;
                bool isExistDCMaturityInTrade = false;

                OpenOutputFileName(pOutputFileName);
                OpenInputFileName();

                // RD 20131007 passage de la valeur de guard de 99 999 à 999 999
                // Les prix des contrats USO et YHOO ne sont pas intégrés à la date du 01/10/2013
                // car les lignes concernant ces deux contrats se trouvent audelà de la position 101 751 
                int guard = 9999999; // 110 188 reccord dans le fichier du 01/10/2013
                int currentLineNumber = 0;

                /* Process ligne par ligne */
                while (++currentLineNumber < guard)
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    bool isNewContractSymbol = false;
                    bool isOkToCopy = false;

                    /* Break dans le cas d'une ligne null */
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + currentLineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED");
                        break;
                    }

                    /* Vérifier existence d'un DC négocié */
                    if (currentContractSymbol != GetData(currentLine, 3))
                    {
                        isNewContractSymbol = true;
                        currentContractSymbol = GetData(currentLine, 3);
                        isCurrentDcInTrade = IsExistDcInTrade(Cs, m_dtFile, exchangeAcronym, currentContractSymbol, category);
                    }

                    if (!isNewContractSymbol && currentMaturityDate != GetData(currentLine, 13))
                        isExistDCMaturityInTrade = false;

                    /* Vérifier existence d'un DC négocié, sur une maturité particulière */
                    if (isCurrentDcInTrade && (isNewContractSymbol || false == isExistDCMaturityInTrade))
                    {
                        currentMaturityDate = GetData(currentLine, 13);
                        m_QueryExistDCMaturityInTrade.Parameters["DTFILE"].Value = m_dtFile;
                        m_QueryExistDCMaturityInTrade.Parameters["EXCHANGEACRONYM"].Value = exchangeAcronym;
                        m_QueryExistDCMaturityInTrade.Parameters["CONTRACTSYMBOL"].Value = currentContractSymbol;
                        m_QueryExistDCMaturityInTrade.Parameters["CATEGORY"].Value = category;
                        m_QueryExistDCMaturityInTrade.Parameters["MATURITYMONTHYEAR"].Value = currentMaturityDate;

                        using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistDCMaturityInTrade.Query, m_QueryExistDCMaturityInTrade.Parameters.GetArrayDbParameter()))
                        {
                            isExistDCMaturityInTrade = dr.Read();
                        }
                    }

                    /* Vérifier existence d'un Asset négocié, sur une MaturityDate particulière */
                    if (isCurrentDcInTrade && isExistDCMaturityInTrade)
                    {
                        // FI 20131121 [19224] 
                        SetQueryExistAssetMaturityDateInTrade(category, AssetFindMaturityEnum.MATURITYMONTHYEAR);
                        m_QueryExistAssetMaturityDateInTrade.Parameters["DTFILE"].Value = m_dtFile;
                        m_QueryExistAssetMaturityDateInTrade.Parameters["CONTRACTSYMBOL"].Value = currentContractSymbol;
                        m_QueryExistAssetMaturityDateInTrade.Parameters["ISO10383_ALPHA4"].Value = iso10383;
                        m_QueryExistAssetMaturityDateInTrade.Parameters["CATEGORY"].Value = category;
                        //m_QueryExistAssetMaturityDateInTrade.parameters["MATURITYDATE"].Value = DtFunc.ParseDate(currentMaturityDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        m_QueryExistAssetMaturityDateInTrade.Parameters["MATURITYMONTHYEAR"].Value = currentMaturityDate;
                        m_QueryExistAssetMaturityDateInTrade.Parameters["PUTCALL"].Value = GetPutCall(GetData(currentLine, 4));
                        m_QueryExistAssetMaturityDateInTrade.Parameters["STRIKEPRICE"].Value = decimal.Parse(GetData(currentLine, 11));

                        using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistAssetMaturityDateInTrade.Query, m_QueryExistAssetMaturityDateInTrade.Parameters.GetArrayDbParameter()))
                        {
                            isOkToCopy = dr.Read();
                        }
                    }

                    /* Ecriture dans le fichier "light" */
                    if (isOkToCopy)
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StreamWriter.WriteLine(currentLine);
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                CloseAllFiles();
            }
        }

        /// <summary>
        /// Récupère une donnée dans la ligne du fichier
        /// </summary>
        /// <param name="pLine">Ligne du fichier</param>
        /// <param name="pOrder">Ordre dans lequel le paramètre est placé</param>
        /// <returns>Donnée</returns>
        private static string GetData(string pLine, int pOrder)
        {
            /* Exemple :
             *  pLine       = "AAPL,426.21,AAPL,P,AAP,2,0,13,Apr,593.15,1020.00,0,20130420"
             *  pOrder      = "4"
             *  parameter   = "P" */
            string[] separator = new string[] { "," };
            string[] splittedLine = pLine.Split(separator, StringSplitOptions.None);
            string data = splittedLine[pOrder - 1];
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <returns></returns>
        protected override string GetISO183803FromExchangeAcronym(string pExchangeAcronym)
        {
            string ret;
            switch (pExchangeAcronym)
            {
                case "XUSS":
                    ret = pExchangeAcronym;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pExchangeAcronym));
            }

            return ret;
        }
        #endregion methods
    }

}
