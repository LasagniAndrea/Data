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

using EfsML.Enum;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// 
    /// </summary>
    internal class MarketDataImportSPAN : MarketDataImportBase
    {
        #region Declare Constante
        private const int lenght_Record_81_Standard_Format = 80;
        private const int lenght_Record_82_Standard_Format = 80;

        private const int lenght_Record_81_Expanded_Format = 123;
        private const int lenght_Record_82_Expanded_Format = 171;
        private const int lenght_Record_83_Expanded_Format = 135;
        private const int lenght_Record_84_Expanded_Format = 192;

        private const int lenght_Record_81_Paris_Expanded_Format = 132;
        private const int lenght_Record_82_Paris_Expanded_Format = 132;
        private const int lenght_Record_83_Paris_Expanded_Format = 132;
        #endregion Declare Constante

        #region enum
        /// <summary>
        /// Type de format de fichier SPAN
        /// </summary>
        public enum SpanFileFormatEnum
        {
            StandardUnpacked,
            StandardPacked,
            ExpandedUnpacked,
            ParisExpanded,
            NA
        }
        #endregion enum

        #region private class
        private class CombinedCommodity : IEqualityComparer<CombinedCommodity>
        {
            #region members
            private readonly string m_Exchange;
            private readonly string m_CombinedCode;
            #endregion members

            #region accessors
            public string Exchange { get { return m_Exchange; } }
            public string CombinedCode { get { return m_CombinedCode; } }
            #endregion accessors

            #region constructor
            public CombinedCommodity(string pExchange, string pCombinedCode)
            {
                m_Exchange = pExchange;
                m_CombinedCode = pCombinedCode;
            }
            #endregion constructor

            #region methods IEqualityComparer
            /// <summary>
            /// Les CombinedCommodity sont égaux s'ils ont le même Exchange et le même code
            /// </summary>
            /// <param name="x">1er v à comparer</param>
            /// <param name="y">2ème CombinedCommodity à comparer</param>
            /// <returns>true si x Equals Y, sinon false</returns>
            public bool Equals(CombinedCommodity x, CombinedCommodity y)
            {

                //Vérifier si les objets référencent les même données
                if (Object.ReferenceEquals(x, y)) return true;

                //Vérifier si un des objets est null
                if (x is null || y is null)
                    return false;

                // Vérifier qu'il s'agit des même CombinedCommodity
                return (x.m_Exchange == y.m_Exchange)
                    && (x.m_CombinedCode == y.m_CombinedCode);
            }

            /// <summary>
            /// La méthode GetHashCode fournissant la même valeur pour des objets CombinedCommodity qui sont égaux.
            /// </summary>
            /// <param name="pCombinedCommodity">Le paramètre CombinedCommodity dont on veut le hash code</param>
            /// <returns>La valeur du hash code</returns>
            public int GetHashCode(CombinedCommodity pCombinedCommodity)
            {
                //Vérifier si l'obet est null
                if (pCombinedCommodity is null) return 0;

                //Obtenir le hash code de l'Exchange.
                int hashExchange = pCombinedCommodity.m_Exchange == null ? 0 : pCombinedCommodity.m_Exchange.GetHashCode();

                //Obtenir le hash code du Combined Code si non null.
                int hashCombinedCode = pCombinedCommodity.m_CombinedCode == null ? 0 : pCombinedCommodity.m_CombinedCode.GetHashCode();

                //Calcul du hash code pour le CombinedCommodity.
                return (int)(hashExchange ^ hashCombinedCode);
            }
            #endregion methods IEqualityComparer
        }
        /// <summary>
        /// Class définissant un contrat SPAN (commodity)
        /// </summary>
        private class Commodity : IEqualityComparer<Commodity>
        {
            #region members
            private readonly string m_ExchangeAcronym;
            private readonly string m_ContractSymbol;
            private readonly string m_Category;
            //
            private int m_InstrumentDen = 0;
            private int m_StrikeDecLocator = 0;
            private SpanStrikeConversionEnum? m_StrikeConversionEnum = null;
            #endregion members

            #region accessors
            public string ExchangeAcronym { get { return m_ExchangeAcronym; } }
            public string ContractSymbol { get { return m_ContractSymbol; } }
            public string Category { get { return m_Category; } }
            //
            public int InstrumentDen { get { return m_InstrumentDen; } }
            public int StrikeDecLocator { get { return m_StrikeDecLocator; } }
            public SpanStrikeConversionEnum? StrikeConversionEnum { get { return m_StrikeConversionEnum; } }
            #endregion accessors

            #region constructor
            /// <summary>
            ///  Constructeur tenant compte uniquement de l'exchange et du contract
            /// </summary>
            /// <param name="pExchangeAcronym"></param>
            /// <param name="pContractSymbol"></param>
            /// PM 20170929 [23472] New
            public Commodity(string pExchangeAcronym, string pContractSymbol) : this (pExchangeAcronym, pContractSymbol, null){}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pExchangeAcronym"></param>
            /// <param name="pContractSymbol"></param>
            /// <param name="pCategory"></param>
            public Commodity(string pExchangeAcronym, string pContractSymbol, string pCategory)
            {
                m_ExchangeAcronym = pExchangeAcronym;
                m_ContractSymbol = pContractSymbol;
                m_Category = pCategory;
            }
            #endregion constructor

            #region methods
            /// <summary>
            /// Lecture des informations de conversion de strike du DC et affectation au Commodity
            /// </summary>
            /// <param name="pDr"></param>
            /// <returns>True si les informations ont pu être affectés, sinon false</returns>
            public bool SetContractInformation(IDataReader pDr)
            {
                bool jobDone = false;
                if ((null != pDr) && (pDr.Read()))
                {
                    m_InstrumentDen = Convert.IsDBNull(pDr["INSTRUMENTDEN"]) ? 0 : Convert.ToInt32(pDr["INSTRUMENTDEN"]);
                    m_StrikeDecLocator = Convert.IsDBNull(pDr["STRIKEDECLOCATOR"]) ? 0 : Convert.ToInt32(pDr["STRIKEDECLOCATOR"]); 
                    string strikeSPANCode = Convert.ToString(pDr["STRIKESPANCODE"]);
                    if (StrFunc.IsFilled(strikeSPANCode))
                    {
                        m_StrikeConversionEnum = (SpanStrikeConversionEnum)Enum.Parse(typeof(SpanStrikeConversionEnum), strikeSPANCode);
                    }
                    else
                    {
                        m_StrikeConversionEnum = null;
                    }
                    jobDone = true;
                }
                return jobDone;
            }
            #endregion methods

            #region methods IEqualityComparer
            /// <summary>
            /// Les Commodity sont égaux s'ils ont les même données
            /// </summary>
            /// <param name="x">1er Commodity à comparer</param>
            /// <param name="y">2ème Commodity à comparer</param>
            /// <returns>true si x Equals Y, sinon false</returns>
            public bool Equals(Commodity x, Commodity y)
            {

                //Vérifier si les objets référencent les même données
                if (ReferenceEquals(x, y)) return true;

                //Vérifier si un des objets est null
                if (x is null || y is null)
                    return false;

                // Vérifier qu'il s'agit des même Commodity
                return ((x.m_ExchangeAcronym == y.m_ExchangeAcronym)
                    && (x.m_ContractSymbol == y.m_ContractSymbol)
                    && (x.m_Category == y.m_Category));
            }

            /// <summary>
            /// La méthode GetHashCode fournissant la même valeur pour des objets Commodity qui sont égaux.
            /// </summary>
            /// <param name="pCommodity">Le paramètre Commodity dont on veut le hash code</param>
            /// <returns>La valeur du hash code</returns>
            public int GetHashCode(Commodity pCommodity)
            {
                //Vérifier si l'obet est null
                if (pCommodity is null) return 0;

                //Obtenir le hash code de l'Exchange.
                int hashExchange = pCommodity.m_ExchangeAcronym == null ? 0 : pCommodity.m_ExchangeAcronym.GetHashCode();

                //Obtenir le hash code du Contract si non null.
                int hashCommodity = pCommodity.m_ContractSymbol == null ? 0 : pCommodity.m_ContractSymbol.GetHashCode();

                //Obtenir le hash code de la Category si non null.
                int hashCategory = pCommodity.m_Category == null ? 0 : pCommodity.m_Category.GetHashCode();

                //Calcul du hash code pour le Commodity.
                return (int)(hashExchange ^ hashCommodity ^ hashCategory);
            }
            #endregion methods IEqualityComparer
        }
        #endregion

        #region members
        /// <summary>
        /// Ensemble des groupes de contrats destinataires de spreads de regroupement
        /// </summary>
        readonly List<CombinedCommodity> m_TargetCombinedCodeList = default;
        /// <summary>
        /// Ensemble des contrats appartenant à des groupes de contrats destinataires de spreads de regroupement
        /// </summary>
        /// PM 20170929 [23472] Ajout m_TargetContractList
        readonly List<Commodity> m_TargetContractList = default;
        /// <summary>
        /// Permet de stocker les informations sur les DC
        /// </summary>
        readonly List<Commodity> m_ContractInformation = default;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pMaturityType">MATURITYMONTHYEAR ou MATURITYDATE</param>
        public MarketDataImportSPAN(Task pTask, string pDataName, string pDataStyle, AssetFindMaturityEnum pMaturityType)
            : base(pTask, pDataName, pDataStyle, false, false, pMaturityType)
        {
            m_TargetCombinedCodeList = new List<CombinedCommodity>();
            // PM 20170929 [23472] Ajout m_TargetContractList
            m_TargetContractList = new List<Commodity>();
            m_ContractInformation = new List<Commodity>();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Lecture des groupes de contrats qui sont des groupes de contrats cibles des spreads inter-commodity par la méthode "04"
        /// </summary>
        private void ReadTargetCombinedCode()
        {
            try
            {
                SpanFileFormatEnum fileFormat = SpanFileFormatEnum.NA;
                bool isRecord6Found = false;

                OpenInputFileName();

                bool isKeepOnReading = true;
                int guard = 9999999;
                int lineNumber = 0;
                while (isKeepOnReading && (++lineNumber < guard))
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + lineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED");
                        break;
                    }
                    string recordType = currentLine.Substring(0, 1);
                    switch (recordType)
                    {
                        #region Record ID "0" - Ce type de Record existe uniquement sur les fichiers SPAN au format 'Expanded Unpacked' et 'Paris Expanded'
                        case "0":
                            // On récupère FileFormat - (Position 36 Longueur 2 )
                            // Valeurs Possible : U2 for Expanded Unpacked, UP for Paris Expanded Format
                            switch (currentLine.Substring(35, 2))
                            {
                                case "U2":
                                    fileFormat = SpanFileFormatEnum.ExpandedUnpacked;
                                    break;
                                case "UP":
                                    fileFormat = SpanFileFormatEnum.ParisExpanded;
                                    // Pas de spread inter-commidty en méthode 4 pour ce type de fichier
                                    isKeepOnReading = false;
                                    break;
                                default:
                                    fileFormat = SpanFileFormatEnum.NA;
                                    break;
                            }
                            break;
                        #endregion
                        #region Record ID "1"
                        case "1":
                            // Si le premier record du fichier est de type "1" ce fichier est un fichier SPAN au format 'Standard'
                            if (lineNumber == 1)
                            {
                                // On récupère la format de fichier standard - (Position 22 Longueur 1)
                                if (currentLine.Substring(21, 1) == "P")
                                {
                                    fileFormat = SpanFileFormatEnum.StandardPacked;
                                }
                                else
                                {
                                    fileFormat = SpanFileFormatEnum.StandardUnpacked;
                                }
                            }
                            break;
                        #endregion
                        #region Record ID "6"
                        case "6":
                            string spreadMethod = null;
                            string targetExchange = null;
                            string targetContractGroup = null;
                            isRecord6Found = true;

                            if (fileFormat == SpanFileFormatEnum.ExpandedUnpacked)
                            {
                                // FL BD 20130718 Correction d'un bug lors d’un test d'intégration du fichier SPAN  
                                // fourni par la chambre JSCC ( fichie SPAN au format ExpandedUnpacked(PA2)) incluant 
                                // désormais le marché OSE (voir ticket 33357).
                                // Dans ce fichier les record de Type 6 on une longueur ne dépassant pas 53 caractères. 
                                // Exemple de record : 6 JGB00010433400OSE JGBL  0010000AOSE JGBM  0012900B
                                if (currentLine.Length > 53)
                                {
                                    spreadMethod = currentLine.Substring(88, 2);
                                    targetExchange = currentLine.Substring(90, 3);
                                    targetContractGroup = currentLine.Substring(94, 6);
                                }
                            }
                            else if ((fileFormat == SpanFileFormatEnum.StandardPacked) || (fileFormat == SpanFileFormatEnum.StandardUnpacked))
                            {
                                spreadMethod = currentLine.Substring(78, 2);
                                targetExchange = currentLine.Substring(43, 2);
                                targetContractGroup = currentLine.Substring(45, 3);
                            }

                            if (spreadMethod == "04")
                            {
                                if (m_TargetCombinedCodeList != default(List<CombinedCommodity>))
                                {
                                    CombinedCommodity targetCombinedCode = new CombinedCommodity(targetExchange.Trim(), targetContractGroup.Trim());
                                    if (false == m_TargetCombinedCodeList.Contains(targetCombinedCode, targetCombinedCode))
                                    {
                                        m_TargetCombinedCodeList.Add(targetCombinedCode);
                                    }
                                }
                            }
                            break;
                        #endregion
                        #region Record ID autre
                        default:
                            if (isRecord6Found)
                            {
                                // Les enregistrements de type 6 ont été dépassés
                                isKeepOnReading = false;
                            }
                            break;

                        #endregion
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer les fichiers
                CloseAllFiles();
            }

        }

        /// <summary>
        /// Lecture des contrats appartenant aux groupes de contrats qui sont des groupes de contrats cibles des spreads inter-commodity par la méthode "04"
        /// </summary>
        /// PM 20170929 [23472] New
        private void ReadTargetContract()
        {
            try
            {
                SpanFileFormatEnum fileFormat = SpanFileFormatEnum.NA;
                string market = string.Empty;
                string combinedCode = string.Empty;
                bool isRecord6Found = false;

                OpenInputFileName();

                bool isKeepOnReading = true;
                int guard = 9999999;
                int lineNumber = 0;
                while (isKeepOnReading && (++lineNumber < guard))
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + lineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED");
                        break;
                    }
                    string recordType = currentLine.Substring(0, 1);
                    switch (recordType)
                    {
                        #region Record ID "0" - Ce type de Record existe uniquement sur les fichiers SPAN au format 'Expanded Unpacked' et 'Paris Expanded'
                        case "0":
                            // On récupère FileFormat - (Position 36 Longueur 2 )
                            // Valeurs Possible : U2 for Expanded Unpacked, UP for Paris Expanded Format
                            switch (currentLine.Substring(35, 2))
                            {
                                case "U2":
                                    fileFormat = SpanFileFormatEnum.ExpandedUnpacked;
                                    break;
                                case "UP":
                                    fileFormat = SpanFileFormatEnum.ParisExpanded;
                                    // Pas de spread inter-commidty en méthode 4 pour ce type de fichier
                                    isKeepOnReading = false;
                                    break;
                                default:
                                    fileFormat = SpanFileFormatEnum.NA;
                                    break;
                            }
                            break;
                        #endregion
                        #region Record ID "1"
                        case "1":
                            // Si le premier record du fichier est de type "1" ce fichier est un fichier SPAN au format 'Standard'
                            if (lineNumber == 1)
                            {
                                // On récupère la format de fichier standard - (Position 22 Longueur 1)
                                if (currentLine.Substring(21, 1) == "P")
                                {
                                    fileFormat = SpanFileFormatEnum.StandardPacked;
                                }
                                else
                                {
                                    fileFormat = SpanFileFormatEnum.StandardUnpacked;
                                }
                            }
                            break;
                        #endregion
                        #region Record ID "2"
                        case "2":
                            int dcPos = 0; // Position de départ de chaque DC
                            int dcLen = 0; // Longueur de chaque DC
                            int dcDecalPos = 0; // Décalage entre chaque DC
                            int dcMax = 0; // Position maximum de lecture d'un DC

                            bool isTargetCurrentCombinedCode = false;
                            switch (fileFormat)
                            {
                                #region A partir d'un fichier SPAN au format Standard
                                case SpanFileFormatEnum.StandardPacked:
                                case SpanFileFormatEnum.StandardUnpacked:
                                    // market_EXCHANGEACRONYM est celui du Record 1

                                    // On récupère le Combined Commodity Code - (Position 2 Longueur 3 )
                                    combinedCode = currentLine.Substring(1, 3).Trim();

                                    dcPos = 4; // Position de départ de chaque DC (5)
                                    dcLen = 2; // Longueur de chaque DC (2)
                                    dcDecalPos = 3; // Décalage entre chaque DC
                                    dcMax = 65;
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Expanded Unpacked
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Combined Commodity Code - (Position 7 Longueur 6 )
                                    combinedCode = currentLine.Substring(6, 6).Trim();

                                    dcPos = 22; // Position de départ de chaque DC (23)
                                    dcLen = 10; // Longueur de chaque DC (10)
                                    dcDecalPos = 16; // Décalage entre chaque DC
                                    dcMax = currentLine.Length;
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Paris Expanded
                                case SpanFileFormatEnum.ParisExpanded:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Combined Commodity Code - (Position 7 Longueur 6)
                                    combinedCode = currentLine.Substring(6, 6).Trim();

                                    dcPos = 23; // Position de départ de chaque DC (24)
                                    dcLen = 12; // Longueur de chaque DC (12)
                                    dcDecalPos = 33; // Décalage entre chaque DC
                                    dcMax = currentLine.Length;
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            isTargetCurrentCombinedCode = m_TargetCombinedCodeList.Exists(c => (c.Exchange == market) && (c.CombinedCode == combinedCode));
                            if (isTargetCurrentCombinedCode && (m_TargetContractList != default(List<Commodity>)))
                            {
                                // Construire l'ensemble des DC du CombinedCommodity
                                while ((dcPos + dcLen) <= dcMax)
                                {
                                    string dc_CONTRACTSYMBOL = currentLine.Substring(dcPos, dcLen).Trim();
                                    Commodity targetContract = new Commodity(market.Trim(), dc_CONTRACTSYMBOL.Trim());
                                    if (false == m_TargetContractList.Contains(targetContract, targetContract))
                                    {
                                        m_TargetContractList.Add(targetContract);
                                    }
                                    dcPos += dcDecalPos;
                                }
                            }
                            break;
                        #endregion
                        #region Record ID "6"
                        case "6":
                            isRecord6Found = true;
                            break;
                        #endregion
                        #region Record ID autre
                        default:
                            if (isRecord6Found)
                            {
                                // Les enregistrements de type 6 ont été dépassés
                                isKeepOnReading = false;
                            }
                            break;
                        #endregion
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer les fichiers
                CloseAllFiles();
            }
        }

        /// <summary>
        /// Retourne un DataReader contenant les informations concernant un DC sur l'interprétation des strike lu dans un fichier SPAN
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        private IDataReader GetDerivativeContractInfo(string pExchangeAcronym, string pContractSymbol, string pCategory)
        {
            string sqlQuery = SQLCst.SELECT + "dc.INSTRUMENTDEN, dc.STRIKEDECLOCATOR, dc.STRIKESPANCODE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " m" + SQLCst.ON + "(m.IDM = dc.IDM)" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)";
            sqlQuery += SQLCst.AND + "(dc.CATEGORY = @CATEGORY)";
            sqlQuery += SQLCst.AND + "(m.EXCHANGEACRONYM = @EXCHANGEACRONYM)";

            DataParameter dataParameter;
            DataParameters dataParameters = new DataParameters();

            dataParameter = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.EXCHANGEACRONYM);
            dataParameters.Add(dataParameter);

            dataParameter = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL);
            dataParameters.Add(dataParameter);

            dataParameter = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CATEGORY);
            dataParameters.Add(dataParameter);

            dataParameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;
            dataParameters["CONTRACTSYMBOL"].Value = pContractSymbol;
            dataParameters["CATEGORY"].Value = pCategory;

            QueryParameters qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);
            IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return (dr);
        }

        /// <summary>
        /// Converti un strike du format SPAN vers le format Spheres
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCategory"></param>
        /// <param name="pStrike"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private decimal ConvertStrikePrice(string pExchangeAcronym, string pContractSymbol, string pCategory, decimal pStrike)
        {
            decimal convertedStrike = pStrike;
            Commodity contract = new Commodity(pExchangeAcronym, pContractSymbol, pCategory);
            //
            if (false == m_ContractInformation.Contains(contract, contract))
            {
                using (IDataReader dr = GetDerivativeContractInfo(pExchangeAcronym, pContractSymbol, pCategory))
                {
                    if (contract.SetContractInformation(dr))
                    {
                        m_ContractInformation.Add(contract);
                    }
                }
            }
            else
            {
                contract = m_ContractInformation.Find(a => a.Equals(a,contract));
            }
            //
            if (contract != default(Commodity))
            {
                // Gestion du nombre de décimal
                decimal strikeDiviseur = Convert.ToDecimal(Math.Pow(10, contract.StrikeDecLocator));
                convertedStrike /= strikeDiviseur;
                // Gestion de la Base
                if (contract.InstrumentDen >= 10000)
                {
                    convertedStrike = convertedStrike * contract.InstrumentDen / 100;
                }
                // Gestion format particulier de la partie décimal
                if ((contract.StrikeConversionEnum.HasValue) && (System.Math.Floor(convertedStrike) != convertedStrike))
                {
                    string strStrike = convertedStrike.ToString().Trim().TrimEnd(new char[] { '0' });
                    string lastdigit = strStrike.Substring(strStrike.Length - 1, 1);

                    switch (contract.StrikeConversionEnum)
                    {
                        case SpanStrikeConversionEnum.Quarters:
                            // Cas de l'option 5-Year U.S. Treasury Note (25) et des Options EuroDollar
                            if ((lastdigit == "2") || (lastdigit == "7"))
                            {
                                strStrike += "5";
                            }
                            break;
                        case SpanStrikeConversionEnum.Eighths:
                            // Cas de l'option 2-Year U.S. Treasury Note (26)
                            switch (lastdigit)
                            {
                                case "1":
                                case "6":
                                    strStrike += "25";
                                    break;
                                case "2":
                                case "7":
                                    strStrike += "5";
                                    break;
                                case "3":
                                case "8":
                                    strStrike += "75";
                                    break;
                            }
                            break;
                    }
                    convertedStrike = decimal.Parse(strStrike);
                }
            }
            return convertedStrike;
        }

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des Cotations et des paramètres Risk SPAN
        /// </summary>
        /// <param name="pOutputFileName"></param>
        public void Create_LightTheoreticalPriceFile(string pOutputFileName)
        {
            #region Filtrer le fichier source
            try
            {
                #region Declare
                #region Initialisation de la requête avec ses paramètres permettant d'avoir l'asset du Sous-jacent  ainsi que le nom de la table de cotation associée.
                string sqlQuery_Unl_Asset = SQLCst.SELECT + "main.IDASSET source,";
                sqlQuery_Unl_Asset += SQLCst.CASE + "main.ASSETCATEGORY"
                                        + SQLCst.CASE_WHEN + "'Future'" + SQLCst.CASE_THEN + "main.DA_IDASSET"
                                        + SQLCst.CASE_ELSE + "main.IDASSET_UNL" + SQLCst.CASE_END + "as unl_IDASSET,";
                sqlQuery_Unl_Asset += SQLCst.CASE + "main.ASSETCATEGORY"
                                        + SQLCst.CASE_WHEN + "'Future'" + SQLCst.CASE_THEN + "'QUOTE_ETD_H'"
                                        + SQLCst.CASE_WHEN + "'Bond'" + SQLCst.CASE_THEN + "'QUOTE_DEBTSEC_H'"
                                        + SQLCst.CASE_WHEN + "'Commodity'" + SQLCst.CASE_THEN + "'QUOTE_COMMODITY_H'"
                                        + SQLCst.CASE_WHEN + "'EquityAsset'" + SQLCst.CASE_THEN + "'QUOTE_EQUITY_H'"
                                        + SQLCst.CASE_WHEN + "'FxRateIndex'" + SQLCst.CASE_THEN + "'QUOTE_FXRATE_H'"
                                        + SQLCst.CASE_WHEN + "'Index'" + SQLCst.CASE_THEN + "'QUOTE_INDEX_H'"
                                        + SQLCst.CASE_WHEN + "'RateIndex'" + SQLCst.CASE_THEN + "'QUOTE_RATEINDEX_H'"
                                        + SQLCst.CASE_ELSE + "null" + SQLCst.CASE_END + "as TBL_UNLQUOTE";
                sqlQuery_Unl_Asset += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " main" + Cst.CrLf;
                sqlQuery_Unl_Asset += SQLCst.WHERE + "main.IDASSET = @IDASSET" + Cst.CrLf;

                DataParameter dataParameter_Unl_Asset;
                DataParameters dataParameters_Unl_Asset = new DataParameters();

                dataParameter_Unl_Asset = DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDASSET);
                dataParameters_Unl_Asset.Add(dataParameter_Unl_Asset);
                #endregion

                string Record_Type1 = string.Empty;
                string Record_Type2 = string.Empty;
                string previousRecord_Type1 = string.Empty;
                string currentLine = string.Empty;
                string currentLine_81828384_Plus = string.Empty;

                SpanFileFormatEnum FileFormat = SpanFileFormatEnum.NA;

                string ProductTypeCode = string.Empty;
                string ProductType = string.Empty;

                bool isToCopyOk = false;
                bool isToCopyOk_Record_81828384 = false;
                bool isExistCombinedCode = false;
                bool isExistDerivativeContract = false;
                bool isExistDerivativeContractMaturity = false;

                string market_EXCHANGEACRONYM = string.Empty;
                string dc_CONTRACTSYMBOL = string.Empty;
                string Exist_ContractSymbolMaturity = string.Empty;
                string dc_CATEGORY = string.Empty;
                string asset_PUTCALL = string.Empty;
                string maturity_MATURITYMONTHYEAR = string.Empty;
                string unlMaturity_MATURITYMONTHYEAR = string.Empty;
                string combinedCode = string.Empty;

                string tablename_Unl_Quote = string.Empty;

                decimal asset_STRIKEPRICE = 0;
                decimal StrikeDiviseur = 1;

                int idasset = 0;
                int idasset_unl = 0;

                List<string> listOfAllContract = new List<string>();
                List<string> listOfContractCombined = new List<string>();   // Liste de tous les contrats d'un CombinedCommodity
                List<string> savedLinesRecord2 = new List<string>();        // Liste de tous les Record type 2 d'un CombinedCommodity
                string previousCombinedCode = string.Empty;
                bool isOkToWriteRecords2 = false;
                string strIntraSpreadPriority = string.Empty;       // Gestion du Spread Priority des records C
                int readIntraSpreadPriority = 0;
                int currentIntraSpreadPriority = 0;
                string typeCPreviousCombinedCode = string.Empty;
                string typeCCombinedCode = string.Empty;
                //PM 20161130 [22636] Gestion du Spread Priority des records 6
                string strInterSpreadPriority = string.Empty;
                int readInterSpreadPriority = 0;
                int currentInterSpreadPriority = 0;
                #endregion

                // Parcours préalable du fichier pour rechercher les groupes de contrats cibles de spreads inter-commodity de méthode "04"
                ReadTargetCombinedCode();

                // Parcours préalable du fichier pour rechercher les contrats appartenant aux groupes de contrats cibles de spreads inter-commodity de méthode "04"
                // PM 20170929 [23472] Ajout
                ReadTargetContract();

                #region Création et ouverture du fichier de sortie "Light"
                OpenOutputFileName(pOutputFileName);
                #endregion

                #region Ouverture du fichier SPAN
                OpenInputFileName();
                #endregion

                int guard = 9999999; //426641 record span cme 20111018
                int lineNumber = 0;

                #region Process Ligne par ligne

                while (++lineNumber < guard)
                {
                    if ((lineNumber % 10000) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine(lineNumber);
                    }

                    //Lecture d'une ligne dans le fichier d'entrée SPAN d'origine
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //currentLine = IOTools.StreamReader.ReadLine();
                    currentLine = IOCommonTools.StreamReader.ReadLine();
                    // RD 20160425 [22035] La ligne doit avoir au moins 2 caractères (voir Record_Type2 ci-dessous)
                    if (currentLine != null && currentLine.Length < 2)
                    {
                        //NB: La fin de ligne est du type 0D0D0A
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //currentLine = IOTools.StreamReader.ReadLine();
                        currentLine = IOCommonTools.StreamReader.ReadLine();
                    }

                    if (currentLine == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Line number: " + lineNumber.ToString());
                        System.Diagnostics.Debug.WriteLine("Guard: " + guard.ToString());
                        System.Diagnostics.Debug.WriteLine("ENDED");
                        break;
                    }

                    // Passage de la longueur du record type de 2 à 2 Record type différent (un de longueur 1 et un de longueur 2 pour le traitement des records 8)
                    // On récupère le Record Type de l’enregistrement du fichier d’entrée (Position 1 Longueur 1 ou 2)
                    Record_Type1 = currentLine.Substring(0, 1);
                    Record_Type2 = currentLine.Substring(0, 2);

                    switch (Record_Type1)
                    {
                        #region Record ID "0" - Ce type de Record existe uniquement sur les fichiers SPAN au format 'Expanded Unpacked' et 'Paris Expanded'
                        case "0":
                            // On récupère la Business Date (CCYYMMDD) - (Position 9 Longueur 8)
                            m_dtFile = new DtFunc().StringyyyyMMddToDateTime(currentLine.Substring(8, 8));

                            // On récupère FileFormat - (Position 36 Longueur 2 )
                            // Valeurs Possible : U2 for Expanded Unpacked, UP for Paris Expanded Format
                            switch (currentLine.Substring(35, 2))
                            {
                                case "U2":
                                    FileFormat = SpanFileFormatEnum.ExpandedUnpacked;
                                    break;

                                case "UP":
                                    FileFormat = SpanFileFormatEnum.ParisExpanded;
                                    break;

                                default:
                                    FileFormat = SpanFileFormatEnum.NA;
                                    break;
                            }

                            isToCopyOk = true;
                            break;

                        #endregion

                        #region Record ID "1"
                        case "1":
                            #region Record ID "1" - Si le premier record du fichier est de type "1" ce fichier est un fichier SPAN au format 'Standard'
                            if (lineNumber == 1)
                            {
                                // On récupère l' Exchange Acronym (Code) - (Position 2 Longueur 2)
                                market_EXCHANGEACRONYM = currentLine.Substring(1, 2).Trim();

                                // On récupère la Business Date (YYMMDD) - (Position 4 Longueur 6)
                                m_dtFile = new DtFunc().StringyyyyMMddToDateTime("20" + currentLine.Substring(3, 6));

                                // On récupère la format de fichier standard - (Position 22 Longueur 1)
                                if (currentLine.Substring(21, 1) == "P")
                                {
                                    FileFormat = SpanFileFormatEnum.StandardPacked;
                                }
                                else
                                {
                                    FileFormat = SpanFileFormatEnum.StandardUnpacked;
                                }
                            }
                            #endregion
                            isToCopyOk = true;
                            break;
                        #endregion

                        #region Record ID "2"
                        case "2":
                            int dcPos = 0; // Position de départ de chaque DC
                            int dcLen = 0; // Longueur de chaque DC
                            int dcDecalPos = 0; // Décalage entre chaque DC
                            int dcMax = 0; // Position maximum de lecture d'un DC

                            bool isExistCurrentCombinedCode = false;
                            switch (FileFormat)
                            {
                                #region A partir d'un fichier SPAN au format Standard
                                case SpanFileFormatEnum.StandardPacked:
                                case SpanFileFormatEnum.StandardUnpacked:
                                    // market_EXCHANGEACRONYM est celui du Record 1

                                    // On récupère le Combined Commodity Code - (Position 2 Longueur 3 )
                                    combinedCode = currentLine.Substring(1, 3).Trim();

                                    dcPos = 4; // Position de départ de chaque DC (5)
                                    dcLen = 2; // Longueur de chaque DC (2)
                                    dcDecalPos = 3; // Décalage entre chaque DC
                                    dcMax = 65;
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Expanded Unpacked
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Combined Commodity Code - (Position 7 Longueur 6 )
                                    combinedCode = currentLine.Substring(6, 6).Trim();

                                    dcPos = 22; // Position de départ de chaque DC (23)
                                    dcLen = 10; // Longueur de chaque DC (10)
                                    dcDecalPos = 16; // Décalage entre chaque DC
                                    dcMax = currentLine.Length;
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Paris Expanded
                                case SpanFileFormatEnum.ParisExpanded:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Combined Commodity Code - (Position 7 Longueur 6)
                                    combinedCode = currentLine.Substring(6, 6).Trim();

                                    dcPos = 23; // Position de départ de chaque DC (24)
                                    dcLen = 12; // Longueur de chaque DC (12)
                                    dcDecalPos = 33; // Décalage entre chaque DC
                                    dcMax = currentLine.Length;
                                    break;
                                #endregion
                                default:
                                    isExistCurrentCombinedCode = true;
                                    break;
                            }
                            //
                            if (previousCombinedCode != combinedCode)
                            {
                                // Nouveau CombinedCode : tout ré-initialiser
                                previousCombinedCode = combinedCode;
                                savedLinesRecord2 = new List<string>();
                                isExistCombinedCode = false;
                                listOfContractCombined = new List<string>();
                            }
                            savedLinesRecord2.Add(currentLine);
                            //
                            // Construire la liste de tous les DC du CombinedCommodity
                            // et vérifier l'existance du DC du CombinedCommodity sur au moins un Trade
                            isExistDerivativeContract = false;
                            while ((dcPos + dcLen) <= dcMax)
                            {
                                dc_CONTRACTSYMBOL = currentLine.Substring(dcPos, dcLen).Trim();
                                listOfContractCombined.Add(dc_CONTRACTSYMBOL);
                                if (isExistDerivativeContract == false)
                                {
                                    isExistDerivativeContract = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                }
                                dcPos += dcDecalPos;
                            }
                            isExistCurrentCombinedCode = isExistDerivativeContract;

                            if (isExistCurrentCombinedCode == false)
                            {
                                // Vérifier si le CombinedCode est la cible d'un spread inter-commodity
                                // Si oui insérer les données dans le fichier
                                isExistCurrentCombinedCode = m_TargetCombinedCodeList.Exists(c => (c.Exchange == market_EXCHANGEACRONYM) && (c.CombinedCode == combinedCode));
                            }
                            //
                            isExistCombinedCode |= isExistCurrentCombinedCode;
                            //
                            isOkToWriteRecords2 = isExistCombinedCode;
                            isToCopyOk = false; // L'écriture se fera juste avant qu'un prochain record de type différent soit écrit
                            break;
                        #endregion

                        #region Record ID "C"
                        case "C":
                            // Gestion du Spread Priority des records C
                            //=> Ajout un fin de record le SpreadPriority calculé sur 5 digits

                            // Si le CombinedCode lu sur le record de type "2" existe
                            isToCopyOk = isExistCombinedCode;
                            if (isToCopyOk)
                            {
                                switch (FileFormat)
                                {
                                    case SpanFileFormatEnum.StandardPacked:
                                    case SpanFileFormatEnum.StandardUnpacked:
                                        // On récupère le Combined Commodity Code - (Position 2 Longueur 3)
                                        typeCCombinedCode = currentLine.Substring(1, 3).Trim();
                                        // On récupère le Spread Priority - (Position 7 Longueur 2)
                                        strIntraSpreadPriority = currentLine.Substring(6, 2).Trim();
                                        // Complète la ligne avec des blancs pour remplir les champs optionels
                                        currentLine = currentLine.PadRight(73, ' ');
                                        currentLine = currentLine.Substring(0, 73);
                                        break;
                                    case SpanFileFormatEnum.ExpandedUnpacked:
                                    case SpanFileFormatEnum.ParisExpanded:
                                        // On récupère le Combined Commodity Code - (Position 3 Longueur 6)
                                        typeCCombinedCode = currentLine.Substring(2, 6).Trim();
                                        // On récupère le Spread Priority - (Position 11 Longueur 2)
                                        strIntraSpreadPriority = currentLine.Substring(10, 2).Trim();
                                        // Complète la ligne avec des blancs pour remplir les champs optionels
                                        currentLine = currentLine.PadRight(77, ' ');
                                        currentLine = currentLine.Substring(0, 77);
                                        break;
                                }
                                if (typeCPreviousCombinedCode != typeCCombinedCode)
                                {
                                    currentIntraSpreadPriority = 0;
                                    typeCPreviousCombinedCode = typeCCombinedCode;
                                }
                                if (int.TryParse(strIntraSpreadPriority, out readIntraSpreadPriority))
                                {
                                    currentIntraSpreadPriority = readIntraSpreadPriority + ((int)((currentIntraSpreadPriority + 1) / 100)) * 100;
                                    //Ajout du SpreadPriority calculé un fin de record sur 5 digits
                                    currentLine += currentIntraSpreadPriority.ToString("00000");
                                }
                                else
                                {
                                    currentLine += strIntraSpreadPriority.PadLeft(5, '0');
                                }
                            }
                            break;
                        #endregion

                        #region Record ID "S", "3", "E", "4"
                        case "S":
                        case "3":
                        case "E":
                        case "4":
                            // Ajout gestion record "S", "3", "E", "C", "4" conjointement au record "2"
                            string lineCombinedCode = string.Empty;
                            switch (FileFormat)
                            {
                                #region A partir d'un fichier SPAN au format Standard
                                case SpanFileFormatEnum.StandardPacked:
                                case SpanFileFormatEnum.StandardUnpacked:
                                    if (Record_Type1 == "E")
                                    {
                                        // On récupère le Combined Commodity Code - (Position 3 Longueur 6)
                                        lineCombinedCode = currentLine.Substring(2, 6).Trim();
                                    }
                                    else
                                    {
                                        // On récupère le Combined Commodity Code - (Position 2 Longueur 3)
                                        lineCombinedCode = currentLine.Substring(1, 3).Trim();
                                    }
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Expanded Unpacked ou Paris Expanded
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                case SpanFileFormatEnum.ParisExpanded:
                                    // On récupère le Combined Commodity Code - (Position 3 Longueur 6)
                                    lineCombinedCode = currentLine.Substring(2, 6).Trim();
                                    break;
                                #endregion
                            }

                            if (lineCombinedCode == combinedCode)
                            {
                                isToCopyOk = isExistCombinedCode;
                            }
                            else
                            {
                                isToCopyOk = true;
                            }
                            break;
                        #endregion

                        #region Record ID "B", "P", "V", "Z"
                        case "B":
                        case "P":
                        case "V":
                        case "Z":
                            #region Initialisation des variables market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL
                            switch (FileFormat)
                            {
                                #region A partir d'un fichier SPAN au format Standard
                                case SpanFileFormatEnum.StandardPacked:
                                case SpanFileFormatEnum.StandardUnpacked:
                                    if (Record_Type1 == "Z")
                                    {
                                        // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                        market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                        // On récupère le Commodity (Product) Code - (Position 6 Longueur 10)
                                        dc_CONTRACTSYMBOL = currentLine.Substring(5, 10).Trim();
                                    }
                                    else
                                    {
                                        // On récupère l' Exchange Acronym - (Position 2 Longueur 2)
                                        market_EXCHANGEACRONYM = currentLine.Substring(1, 2).Trim();

                                        // On récupère le Commodity (Product) Code - (Position 4 Longueur 2)
                                        dc_CONTRACTSYMBOL = currentLine.Substring(3, 2).Trim();
                                    }
                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Expanded Unpacked
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Commodity (Product) Code - (Position 6 Longueur 10)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(5, 10).Trim();

                                    break;
                                #endregion
                                #region A partir d'un fichier SPAN au format Paris Expanded
                                case SpanFileFormatEnum.ParisExpanded:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Commodity (Product) Code - (Position 6 Longueur 12)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(5, 12).Trim();
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            #endregion

                            // PM 20170929 [23472] Modification des conditions d'insertion dans le fichier avec prise en compte de m_TargetContractList
                            // Pas de record V ou Z pour ParisExpanded
                            if (((Record_Type1 == "V") || (Record_Type1 == "Z")) && (FileFormat == SpanFileFormatEnum.ParisExpanded))
                            {
                                isToCopyOk = false;
                            }
                            else
                            {
                                // Vérifier si le Contract appartient à un CombinedCode cible d'un spread inter-commodity
                                // Si oui insérer les données dans le fichier
                                isToCopyOk = m_TargetContractList.Exists(c => (c.ExchangeAcronym == market_EXCHANGEACRONYM) && (c.ContractSymbol == dc_CONTRACTSYMBOL));
                                if (false == isToCopyOk)
                                {
                                    if (Record_Type1 == "B")
                                    {
                                        switch (FileFormat)
                                        {
                                            #region A partir d'un fichier SPAN au format Standard
                                            case SpanFileFormatEnum.StandardPacked:
                                            case SpanFileFormatEnum.StandardUnpacked:
                                                ProductTypeCode = string.Empty;
                                                break;
                                            #endregion
                                            #region A partir d'un fichier SPAN au format Expanded Unpacked
                                            case SpanFileFormatEnum.ExpandedUnpacked:
                                                // On récupère le Product Type Code - (Position 16 Longueur 3)
                                                ProductTypeCode = currentLine.Substring(15, 3).Trim();
                                                break;
                                            #endregion
                                            #region A partir d'un fichier SPAN au format Paris Expanded
                                            case SpanFileFormatEnum.ParisExpanded:
                                                // On récupère le Product Type Code - (Position 18 Longueur 5)
                                                ProductTypeCode = currentLine.Substring(17, 5).Trim();
                                                break;
                                            #endregion
                                        }
                                        //
                                        // Vérification de l'existance du DC uniquement pour les options
                                        if ((ProductTypeCode == "OOP") || (ProductTypeCode == "OOF") || (ProductTypeCode == "OOF") || (ProductTypeCode == "OOF"))
                                        {
                                            // Vérification de l'existance du DC sur au moins un Trade
                                            isToCopyOk = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                        }
                                        else
                                        {
                                            isToCopyOk = isExistCombinedCode;
                                        }
                                    }
                                    else
                                    {
                                        // PM 20170929 [23472] le cas "record V ou Z pour ParisExpanded" est remonté en début de bloc
                                        //// Pas de record V ou Z pour ParisExpanded 
                                        //if (((Record_Type1 == "V") || (Record_Type1 == "Z")) && (FileFormat == SpanFileFormatEnum.ParisExpanded))
                                        //{
                                        //    isToCopyOk = false;
                                        //}
                                        //else
                                        //{
                                            // Vérification de l'existance du DC sur au moins un Trade
                                            //isToCopyOk = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                        //}
                                        isToCopyOk = IsExistDcInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region Record ID "6"
                        case "6":
                            //PM 20161130 [22636] Gestion du Spread Priority des records 6
                            //=> Ajout un fin de record le SpreadPriority calculé sur 6 digits
                            switch (FileFormat)
                            {
                                case SpanFileFormatEnum.StandardPacked:
                                case SpanFileFormatEnum.StandardUnpacked:
                                    // On récupère le Spread Priority - (Position 5 Longueur 2)
                                    strInterSpreadPriority = currentLine.Substring(4, 2).Trim();
                                    // Complète la ligne avec des blancs pour remplir les champs optionels
                                    currentLine = currentLine.PadRight(80, ' ');
                                    currentLine = currentLine.Substring(0, 80);
                                    break;
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                    // On récupère le Spread Priority - (Position 6 Longueur 4)
                                    strInterSpreadPriority = currentLine.Substring(5, 4).Trim();
                                    // Complète la ligne avec des blancs pour remplir les champs optionels
                                    currentLine = currentLine.PadRight(151, ' ');
                                    currentLine = currentLine.Substring(0, 151);
                                    break;
                                case SpanFileFormatEnum.ParisExpanded:
                                    // On récupère le Spread Priority - (Position 8 Longueur 4)
                                    strInterSpreadPriority = currentLine.Substring(7, 4).Trim();
                                    // Complète la ligne avec des blancs pour remplir les champs optionels
                                    currentLine = currentLine.PadRight(95, ' ');
                                    currentLine = currentLine.Substring(0, 95);
                                    break;
                            }
                            if (int.TryParse(strInterSpreadPriority, out readInterSpreadPriority))
                            {
                                currentInterSpreadPriority = readInterSpreadPriority + (((int)((currentInterSpreadPriority + 1) / 10000)) * 10000);
                                //Ajout du SpreadPriority calculé un fin de record sur 6 digits
                                currentLine += currentInterSpreadPriority.ToString("000000");
                            }
                            else
                            {
                                currentLine += strIntraSpreadPriority.PadLeft(6, '0');
                            }
                            break;
                        #endregion Record ID "6"

                        #region Record ID "8"
                        case "8":
                            #region Initialisation des variables market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, ProductType, dc_CATEGORY, asset_PUTCALL, asset_STRIKEPRICE, maturity_MATURITYMONTHYEAR
                            switch (FileFormat)
                            {
                                #region A partir d'un fichier SPAN au format 'STANDARD'
                                case SpanFileFormatEnum.StandardUnpacked:
                                case SpanFileFormatEnum.StandardPacked:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 2)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 2);

                                    // On récupère le Commodity (Product) Code - (Position 5 Longueur 2)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(4, 2).Trim();

                                    // On récupère le ProductType(PrdTyp) - (Position 7 Longueur 1)
                                    // Valeurs Possible : Contract Type(Future/Put/Call) Flag: blank for future, C for Calls, or P for Puts
                                    ProductType = currentLine.Substring(6, 1);

                                    switch (ProductType)
                                    {
                                        case " ":
                                            dc_CATEGORY = "F";
                                            asset_PUTCALL = string.Empty;
                                            asset_STRIKEPRICE = 0;

                                            // On récupère le FuturesContractMonth(FutMth) - (Position 8 Longueur 4)
                                            // Futures Contract Month as YYMM
                                            maturity_MATURITYMONTHYEAR = "20" + currentLine.Substring(7, 4);
                                            break;

                                        case "C":
                                        case "P":
                                            dc_CATEGORY = "O";

                                            asset_PUTCALL = GetPutCall(ProductType);

                                            // On récupère le OptionContractMonth(OptMth) - (Position 12 Longueur 4)
                                            // Option Contract Month as YYMM
                                            maturity_MATURITYMONTHYEAR = "20" + currentLine.Substring(11, 4);

                                            asset_STRIKEPRICE = Convert.ToDecimal(currentLine.Substring(15, 6));
                                            asset_STRIKEPRICE = ConvertStrikePrice(market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, dc_CATEGORY, asset_STRIKEPRICE);
                                            break;

                                        default:
                                            dc_CATEGORY = string.Empty;
                                            break;
                                    }
                                    break;
                                #endregion

                                #region A partir d'un fichier SPAN au format 'Expanded Unpacked'
                                case SpanFileFormatEnum.ExpandedUnpacked:
                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Commodity (Product) Code - (Position 6 Longueur 10)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(5, 10).Trim();

                                    // On récupère le ProductTypeCode(PrdTyp) - (Position 26 Longueur 3)
                                    // Valeurs Possible : PHY for Physical, FUT for Future, CMB for Combination, OOP for Option on Physical, OOF for Option on Future, OOC for Option on Combination
                                    ProductTypeCode = currentLine.Substring(25, 3);
                                    switch (ProductTypeCode)
                                    {
                                        case "FUT":

                                            dc_CATEGORY = "F";
                                            asset_PUTCALL = string.Empty;
                                            asset_STRIKEPRICE = 0;

                                            // On récupère le FuturesContractMonth(FutMth) - (Position 30 Longueur 6)
                                            // Futures Contract Month as CCYYMM
                                            maturity_MATURITYMONTHYEAR = currentLine.Substring(29, 6);

                                            // On récupère le FuturesContractDayorWeek Code(FutDayWk) - (Position 36 Longueur 2)
                                            if (currentLine.Substring(35, 2).Trim().Length > 0)
                                            {
                                                maturity_MATURITYMONTHYEAR += currentLine.Substring(35, 2);
                                            }
                                            break;

                                        case "OOP":
                                        case "OOF":
                                        case "OOC":
                                        case "OOS": // BD & FL 20130422 Gestion des ProductTypeCode "OOS"

                                            dc_CATEGORY = "O";

                                            // On récupère le OptionRightCode(OptTyp) - (Position 29 Longueur 1)
                                            // Valeurs Possible : for an option only: P for Put or C for Call
                                            asset_PUTCALL = GetPutCall(currentLine.Substring(28, 1));

                                            // On récupère le OptionContractMonth(OptMth) - (Position 39 Longueur 6)
                                            // Option Contract Month as CCYYMM
                                            maturity_MATURITYMONTHYEAR = currentLine.Substring(38, 6);

                                            // On récupère le OptionContractDayorWeekCode(OptDayWk) - (Position 45 Longueur 2)
                                            // Option Contract Day or Week Code For standard monthly options, this field will contain zeros or blanks. 
                                            // For other options, this field will typically contain "W1", "W2", etc. 
                                            // for weekly options expiring in week 1 of the month, week 2 of the month, etc. 
                                            //  or a two-digit day of the month, for flex options or other options for which the exact expiration day is specified.
                                            if (currentLine.Substring(44, 2).Trim().Length > 0)
                                            {
                                                maturity_MATURITYMONTHYEAR += currentLine.Substring(44, 2);
                                            }

                                            asset_STRIKEPRICE = Convert.ToDecimal(currentLine.Substring(47, 7));
                                            asset_STRIKEPRICE = ConvertStrikePrice(market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, dc_CATEGORY, asset_STRIKEPRICE);
                                            break;

                                        default:
                                            dc_CATEGORY = string.Empty;
                                            break;
                                    }
                                    break;
                                #endregion

                                #region A partir d'un fichier SPAN au format 'Paris Expanded Format'
                                case SpanFileFormatEnum.ParisExpanded:

                                    // On récupère l' Exchange Acronym - (Position 3 Longueur 3)
                                    market_EXCHANGEACRONYM = currentLine.Substring(2, 3).Trim();

                                    // On récupère le Commodity (Product) Code - (Position 6 Longueur 12)
                                    dc_CONTRACTSYMBOL = currentLine.Substring(5, 12).Trim();

                                    // On récupère le ProductTypeCode(PrdTyp) - (Position 30 Longueur 5)
                                    // Valeurs Possible : PHY for Physical, FUT for Future, CMB for Combination, OOP for Option on Physical, OOF for Option on Future, OOC for Option on Combination

                                    ProductTypeCode = currentLine.Substring(29, 5).Trim();
                                    switch (ProductTypeCode)
                                    {
                                        case "FUT":

                                            dc_CATEGORY = "F";
                                            asset_PUTCALL = string.Empty;
                                            asset_STRIKEPRICE = 0;

                                            // On récupère le FuturesContractMonth(FutMth) - (Position 36 Longueur 6)
                                            // Futures Contract Month as CCYYMM
                                            maturity_MATURITYMONTHYEAR = currentLine.Substring(35, 6);

                                            // On récupère le FuturesContractDayorWeek Code(FutDayWk) - (Position 42 Longueur 2)
                                            if (currentLine.Substring(41, 2).Trim().Length > 0)
                                            {
                                                maturity_MATURITYMONTHYEAR += currentLine.Substring(41, 2);
                                            }
                                            break;

                                        case "OOP":
                                        case "OOF":
                                        case "OOC":
                                        case "OOS": // BD & FL 20130422 Gestion des ProductTypeCode "OOS"

                                            dc_CATEGORY = "O";

                                            // On récupère le OptionRightCode(OptTyp) - (Position 35 Longueur 1)
                                            // Valeurs Possible : for an option only: P for Put or C for Call
                                            asset_PUTCALL = GetPutCall(currentLine.Substring(34, 1));

                                            // On récupère le OptionContractMonth(OptMth) - (Position 45 Longueur 6)
                                            // Option Contract Month as CCYYMM
                                            maturity_MATURITYMONTHYEAR = currentLine.Substring(44, 6);

                                            // On récupère le OptionContractDayorWeekCode(OptDayWk) - (Position 51 Longueur 2)
                                            // Option Contract Day or Week Code For standard monthly options, this field will contain zeros or blanks. 
                                            // For other options, this field will typically contain "W1", "W2", etc. 
                                            // for weekly options expiring in week 1 of the month, week 2 of the month, etc. 
                                            //  or a two-digit day of the month, for flex options or other options for which the exact expiration day is specified.
                                            if (currentLine.Substring(50, 2).Trim().Length > 0)
                                            {
                                                maturity_MATURITYMONTHYEAR += currentLine.Substring(50, 2);
                                            }

                                            // On récupère le OptionStrikePrice(Strk)- (Position 54 Longueur 14)
                                            // On récupère le StrikeDecimalLocator(StrkNbDec)- (Position 68 Longueur 1)
                                            StrikeDiviseur = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(currentLine.Substring(67, 1))));
                                            asset_STRIKEPRICE = Convert.ToDecimal(currentLine.Substring(53, 14)) / StrikeDiviseur;
                                            break;

                                        default:
                                            dc_CATEGORY = string.Empty;
                                            break;
                                    }
                                    break;
                                #endregion

                                default:
                                    break;
                            }
                            #endregion
                            switch (Record_Type2)
                            {
                                #region Record ID "81", "83"
                                case "81":
                                case "83":


                                    #region Pour Un Stock/Index
                                    // BD & FL 20130422 On copie les lignes correspondant aux Stock
                                    bool isStockOrIndex = false;
                                    // Ne pas vérifier que le symbol des indices fini par ".IND"
                                    if ((ProductTypeCode == "STOCK") || (ProductTypeCode == "PHY"))
                                    {
                                        isStockOrIndex = listOfAllContract.Contains(dc_CONTRACTSYMBOL);
                                    }
                                    #endregion Pour Un Stock

                                    // Vérification de l'existance du DC sur au moins un Trade
                                    isExistDerivativeContract = IsExistDcInTrade(Cs,m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL);

                                    #region Vérification de l'existance de l'Asset et dans le cas ou il existe rajout de données supplémentaires
                                    // les données supplémentaires sont les suivantes :
                                    //      - Identifiant de l'asset 
                                    //      - Identifiant de l'asset de son sous jacent
                                    //      - Nom de la table de cotation du sous jacent

                                    if (currentLine_81828384_Plus.Length == 0 && isExistDerivativeContract)
                                    {
                                        #region Recherche et ajout de l'identifiant de l'asset dans la variable currentLine_818283_Plus

                                        #region Pour Un Asset Option

                                        if (dc_CATEGORY == "O")
                                        {

                                            #region Vérification de l'existance du DC + Maturité sur au moins un Trade

                                            if (dc_CONTRACTSYMBOL + maturity_MATURITYMONTHYEAR != Exist_ContractSymbolMaturity)
                                            {
                                                m_QueryExistDCMaturityInTrade.Parameters["DTFILE"].Value = m_dtFile;
                                                m_QueryExistDCMaturityInTrade.Parameters["EXCHANGEACRONYM"].Value = market_EXCHANGEACRONYM;
                                                m_QueryExistDCMaturityInTrade.Parameters["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                                if (m_MaturityType == AssetFindMaturityEnum.MATURITYDATE)
                                                {
                                                    m_QueryExistDCMaturityInTrade.Parameters["MATURITYDATE"].Value = new DtFunc().StringyyyyMMddToDateTime(maturity_MATURITYMONTHYEAR);
                                                }
                                                else
                                                {
                                                    m_QueryExistDCMaturityInTrade.Parameters["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                                }

                                                object obj_Dc_Maturity_Trade = DataHelper.ExecuteScalar(Cs, CommandType.Text, m_QueryExistDCMaturityInTrade.Query, m_QueryExistDCMaturityInTrade.Parameters.GetArrayDbParameter());

                                                isExistDerivativeContractMaturity = (obj_Dc_Maturity_Trade != null);

                                                Exist_ContractSymbolMaturity = dc_CONTRACTSYMBOL + maturity_MATURITYMONTHYEAR;
                                            }

                                            #endregion

                                            // Si il y a au moins un trade existant sur ce DC + Maturité ( on effectue une recherche par Actif (Optimisation de traitement) )
                                            if (isExistDerivativeContractMaturity)
                                            {
                                                QueryParameters queryParameters = QueryExistAssetOptionInTrades(Cs, m_MaturityType);
                                                DataParameters dp = queryParameters.Parameters;
                                                // On vérifie si on a au moins un trade existant avec cet Actif (ProductID + N° De version + échéance+ prix d’exercice + Call/Put)
                                                dp["DTFILE"].Value = m_dtFile;
                                                dp["EXCHANGEACRONYM"].Value = market_EXCHANGEACRONYM;
                                                dp["CONTRACTSYMBOL"].Value = dc_CONTRACTSYMBOL;
                                                dp["CATEGORY"].Value = dc_CATEGORY;
                                                if (m_MaturityType == AssetFindMaturityEnum.MATURITYDATE)
                                                {
                                                    dp["MATURITYDATE"].Value = new DtFunc().StringyyyyMMddToDateTime(maturity_MATURITYMONTHYEAR);
                                                }
                                                else
                                                {
                                                    dp["MATURITYMONTHYEAR"].Value = maturity_MATURITYMONTHYEAR;
                                                }
                                                dp["PUTCALL"].Value = asset_PUTCALL;
                                                dp["STRIKEPRICE"].Value = asset_STRIKEPRICE;

                                                object asset_IDASSET_Option = DataHelper.ExecuteScalar(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                                                // Si c’est le cas On flag un booléen de type isToCopyBucketVolatility à la	valeur ‘VRAI’ 
                                                // Sinon On isToCopyBucketVolatility à la valeur ‘FALSE’
                                                if (asset_IDASSET_Option == null)
                                                {
                                                    isToCopyOk_Record_81828384 = false;
                                                    idasset = 0;
                                                }
                                                else
                                                {
                                                    isToCopyOk_Record_81828384 = true;
                                                    idasset = Convert.ToInt32(asset_IDASSET_Option);
                                                    currentLine_81828384_Plus = idasset.ToString().PadLeft(10);
                                                }
                                            }
                                            else
                                            {
                                                isToCopyOk_Record_81828384 = false;
                                                idasset = 0;
                                            }
                                        }
                                        #endregion

                                        #region Pour un Asset Future
                                        else if (dc_CATEGORY == "F")
                                        {
                                            // On vérifie si l'Actif existe dans le référentiel Spheres (Actif (ProductID + N° De version + échéance ))
                                            // Remarque Importante: Attention la règle est différente que celle des Options ou on vérifie si on a au moins un trade existant avec cet Actif
                                            int? idAssetFuture = GetIdAssetFutureInTrade(Cs, m_dtFile, market_EXCHANGEACRONYM, dc_CONTRACTSYMBOL, maturity_MATURITYMONTHYEAR, m_MaturityType, false);
                                            if (idAssetFuture == null)
                                            {
                                                isToCopyOk_Record_81828384 = false;
                                                idasset = 0;
                                            }
                                            else
                                            {
                                                isToCopyOk_Record_81828384 = true;
                                                idasset = idAssetFuture.Value;
                                                currentLine_81828384_Plus = idasset.ToString().PadLeft(10);
                                            }

                                        }
                                        #endregion

                                        #endregion Recherche et ajout de l'identifiant de l'asset dans la variable currentLine_818283_Plus

                                        #region Recherche et ajout l'identifiant de l'asset de son sous jacent + nom de la table de cotation du sous jacent dans la variable currentLine_818283_Plus

                                        if (isToCopyOk_Record_81828384)
                                        {
                                            // Initialisation de la variable currentLine_818283_Plus qui contient l'asset du sous jacent et le nom de la table de cotation du sous jacent
                                            // Remarque : On ne recherche l'asset du sous jacent et le nom de la table de cotation du sous jacent que lor d'une nouvelle échéance.
                                            dataParameters_Unl_Asset["IDASSET"].Value = idasset;

                                            using (IDataReader asset_Unl_asset = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlQuery_Unl_Asset, dataParameters_Unl_Asset.GetArrayDbParameter()))
                                            {
                                                if (asset_Unl_asset.Read())
                                                {
                                                    if (asset_Unl_asset["unl_IDASSET"] == Convert.DBNull)
                                                    {
                                                        currentLine_81828384_Plus += "         0";
                                                    }
                                                    else
                                                    {
                                                        idasset_unl = Convert.ToInt32(asset_Unl_asset["unl_IDASSET"]);
                                                        currentLine_81828384_Plus += idasset_unl.ToString().PadLeft(10);
                                                    }

                                                    if (asset_Unl_asset["TBL_UNLQUOTE"] == Convert.DBNull)
                                                    {
                                                        currentLine_81828384_Plus += "              Null";
                                                    }
                                                    else
                                                    {
                                                        tablename_Unl_Quote = Convert.ToString(asset_Unl_asset["TBL_UNLQUOTE"]);
                                                        currentLine_81828384_Plus += tablename_Unl_Quote.ToString().PadLeft(18);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    if (isExistDerivativeContract)
                                    {
                                        isToCopyOk = isToCopyOk_Record_81828384;
                                    }
                                    else
                                    {
                                        // BD & FL 20130422 On copie les lignes correspondant aux Stock
                                        isToCopyOk = isStockOrIndex;
                                    }
                                    break;

                                #endregion

                                #region Record ID "82", "84"
                                case "82":
                                case "84":
                                    #region Pour Un Stock
                                    // BD & FL 20130422 On copie les lignes correspondant aux Stock
                                    isStockOrIndex = false;
                                    dc_CONTRACTSYMBOL = currentLine.Substring(5, 12).Trim();
                                    // Ne pas vérifier la fin du symbol, mais le type de contrat
                                    if ((ProductTypeCode == "STOCK") || (ProductTypeCode == "PHY"))
                                    {
                                        isStockOrIndex = listOfAllContract.Contains(dc_CONTRACTSYMBOL);
                                    }
                                    #endregion Pour Un Stock
                                    if (isExistDerivativeContract)
                                    {
                                        isToCopyOk = isToCopyOk_Record_81828384;
                                    }
                                    else
                                    {
                                        // BD & FL 20130422 On copie les lignes correspondant aux Stock
                                        isToCopyOk = isStockOrIndex;
                                    }
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region Record ID autre
                        default:
                            //Ecriture de la ligne dans le fichier de sortie "Light"
                            isToCopyOk = true;
                            break;

                        #endregion
                    }

                    #region Ecriture de la ligne dans le fichier de sortie "Light"
                    if (isToCopyOk)
                    {
                        if (isOkToWriteRecords2)
                        {
                            if (listOfContractCombined != default(List<string>))
                            {
                                listOfAllContract.AddRange(listOfContractCombined.Distinct());
                            }
                            foreach (string line in savedLinesRecord2)
                            {
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.StreamWriter.WriteLine(line);
                                IOCommonTools.StreamWriter.WriteLine(line);
                            }
                            isOkToWriteRecords2 = false;
                        }

                        switch (Record_Type2)
                        {
                            #region Record ID "81", "82", "83", "84" - On rajoute dans ce type de record les informations : asset, asset du sous jacent et le nom de la table de cotation du sous jacent
                            case "81":
                                switch (FileFormat)
                                {
                                    case SpanFileFormatEnum.StandardUnpacked:
                                    case SpanFileFormatEnum.StandardPacked:
                                        currentLine = currentLine.PadRight(lenght_Record_81_Standard_Format, ' ');
                                        break;

                                    case SpanFileFormatEnum.ExpandedUnpacked:
                                        currentLine = currentLine.PadRight(lenght_Record_81_Expanded_Format, ' ');
                                        break;

                                    case SpanFileFormatEnum.ParisExpanded:
                                        currentLine = currentLine.PadRight(lenght_Record_81_Paris_Expanded_Format, ' ');
                                        break;

                                    default:
                                        break;
                                }

                                if (currentLine_81828384_Plus.Length > 0)
                                {
                                    currentLine += currentLine_81828384_Plus;
                                }
                                break;

                            case "82":
                                switch (FileFormat)
                                {
                                    case SpanFileFormatEnum.StandardUnpacked:
                                    case SpanFileFormatEnum.StandardPacked:
                                        currentLine = currentLine.PadRight(lenght_Record_82_Standard_Format, ' ');
                                        break;

                                    case SpanFileFormatEnum.ExpandedUnpacked:
                                        currentLine = currentLine.PadRight(lenght_Record_82_Expanded_Format, ' ');
                                        break;

                                    case SpanFileFormatEnum.ParisExpanded:
                                        currentLine = currentLine.PadRight(lenght_Record_82_Paris_Expanded_Format, ' ');
                                        break;

                                    default:
                                        break;
                                }

                                if (currentLine_81828384_Plus.Length > 0)
                                {
                                    currentLine += currentLine_81828384_Plus;

                                    if (FileFormat != SpanFileFormatEnum.ParisExpanded)
                                    {
                                        currentLine_81828384_Plus = string.Empty;
                                    }
                                }
                                break;

                            case "83":
                                switch (FileFormat)
                                {
                                    case SpanFileFormatEnum.ExpandedUnpacked:
                                        currentLine = currentLine.PadRight(lenght_Record_83_Expanded_Format, ' ');
                                        break;

                                    case SpanFileFormatEnum.ParisExpanded:
                                        currentLine = currentLine.PadRight(lenght_Record_83_Paris_Expanded_Format, ' ');
                                        break;

                                    default:
                                        break;
                                }
                                if (currentLine_81828384_Plus.Length > 0)
                                {
                                    currentLine += currentLine_81828384_Plus;

                                    if (FileFormat == SpanFileFormatEnum.ParisExpanded)
                                    {
                                        currentLine_81828384_Plus = string.Empty;
                                    }
                                }
                                break;

                            case "84":
                                switch (FileFormat)
                                {
                                    case SpanFileFormatEnum.ExpandedUnpacked:
                                        currentLine = currentLine.PadRight(lenght_Record_84_Expanded_Format, ' ');
                                        break;

                                    default:
                                        break;
                                }
                                if (currentLine_81828384_Plus.Length > 0)
                                {
                                    currentLine += currentLine_81828384_Plus;

                                    if (FileFormat == SpanFileFormatEnum.ExpandedUnpacked)
                                    {
                                        currentLine_81828384_Plus = string.Empty;
                                    }
                                }
                                break;

                            #endregion

                            #region Record ID  autre que  "81", "82", "83", "84"
                            default:
                                // mise a null currentLine_818283_Plus 
                                // pour refaire une recherche des informations : asset, asset du sous jacent et le nom de la table de cotation du sous jacent
                                // dans la prochaine boucle
                                currentLine_81828384_Plus = string.Empty;
                                break;
                            #endregion

                        }

                        //Ecriture de la ligne dans le fichier de sortie "Light"
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StreamWriter.WriteLine(currentLine);
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                    }
                    #endregion

                    previousRecord_Type1 = Record_Type1;
                }
                #endregion
            }
            catch (Exception) { throw; }
            finally
            {
                // Fermer tous les fichiers
                CloseAllFiles();
            }
            #endregion
        }
        #endregion
    }
}
