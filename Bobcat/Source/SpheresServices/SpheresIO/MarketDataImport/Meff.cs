using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;  

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common.IO;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    ///  Classe de gestion des fichiers issues du marché MEFF
    /// </summary>
    internal class MarketDataImportMEFF : MarketDataImportBase
    {
        #region const
        private readonly static string[] separator = { ";" };
        private readonly static char[] charDelimiter = { '"' };
        #endregion

        #region private class
        /// <summary>
        /// Asset du Meff
        /// </summary>
        private sealed class MeffAssetCode
        {
            /// <summary>
            /// ContratGroup de l'Asset du Meff
            /// </summary>
            public string ContractGroup { get; set; }
            /// <summary>
            /// Symbol d'un Asset (ContractCode pour le Meff)
            /// </summary>
            public string AssetCode { get; set; }
        }
        /// <summary>
        /// Permet la lecture dans MeffAssetCode à partir d'un IDataReader
        /// </summary>
        private sealed class MeffAssetCodeReader : IReaderRow
        {
            /// <summary>
            /// Constrution de la requête et paramètre pour la lecture des données utilisées pour la construction des MeffAssetCode
            /// </summary>
            /// <param name="pCS"></param>
            /// <returns></returns>
            public static QueryParameters Query(string pCS)
            {
                StrBuilder query = new StrBuilder();
                query += SQLCst.SELECT + "mc.CONTRACTGROUP, mc.ASSETCODE" + Cst.CrLf;
                query += SQLCst.FROM_DBO + "IMMEFFCONTRACT_H mc" + Cst.CrLf;
                query += SQLCst.WHERE + "(mc.BUSINESSDATE = @DTFILE)" + Cst.CrLf;

                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE));

                QueryParameters qryParameters = new QueryParameters(pCS, query.ToString(), dataParameters);

                return qryParameters;
            }

            #region IReaderRow
            /// <summary>
            /// Data Reader permettant de lire les enregistrement
            /// </summary>
            public IDataReader Reader { get; set; }

            /// <summary>
            /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (MeffAssetCode)
            /// </summary>
            /// <returns>Un objet représentant l'enregistrement lu</returns>
            public object GetRowData()
            {
                MeffAssetCode ret = default;
                if (null != Reader)
                {
                    ret = new MeffAssetCode()
                    {
                        ContractGroup = Reader.GetString(0),
                        AssetCode = Reader.GetString(1)
                    };
                }
                return ret;
            }
            #endregion IReaderRow
        }

        /// <summary>
        /// Classe de convertion d'un ContractCode du MEFF qui correspond à un code d'Asset,
        /// en données élémentaires constituant la série
        /// </summary>
        private sealed class MeffSerie
        {
            #region members
            private readonly string _assetCode = null;
            private readonly MeffContract _contract = null;
            private string _putCall = null;
            public string Maturity = null;
            public decimal Strike = 0;
            public int AssetId = 0;
            public string AssetUnlCategory = null;
            public int AssetUnlId = 0;
            #endregion members

            #region accessors
            public string PutCall
            {
                get { return _putCall; }
            }
            public string Category
            {
                get { return _contract.Category; }
            }
            public string Symbol
            {
                get { return _contract.Symbol; }
            }
            public string SettltMethod
            {
                get { return _contract.SettltMethod; }
            }
            public string ExerciseStyle
            {
                get { return _contract.ExerciseStyle; }
            }
            public decimal? ContractMultiplier
            {
                get { return _contract.ContractMultiplier; }
            }
            public MeffContract Contract
            {
                get { return _contract; }
            }
            #endregion

            #region constructor
            /// <summary>
            /// Constructeur à partir du code d'un asset
            /// <para>Exemple de code d'asset:</para>
            /// <para>  CABGAM 2600U10</para>
            /// <para>  FABGH0P</para>
            /// </summary>
            /// <param name="pAssetCode">Code MEFF de l'asset</param>
            public MeffSerie(string pAssetCode)
            {
                _contract = new MeffContract();
                if (null != pAssetCode)
                {
                    _assetCode = pAssetCode.ToUpper();
                    DecodeSerie();
                }
            }
            #endregion constructor

            #region methods
            /// <summary>
            /// Décompose le code de la série en éléments la constituant
            /// </summary>
            private void DecodeSerie()
            {
                if (null != _assetCode)
                {
                    Match assetMatch;
                    Regex assetRegEx;
                    switch (_assetCode.Substring(0, 1))
                    {
                        case "F":
                            // Format: F + symbol + maturity + settlMethod + multiplier (settlMethod et multiplier optionel)
                            assetRegEx = new Regex(@"^F(?<symbol>\w+)(?<maturity>[FGHJKMNQUVXZ][0-9])(?<settlMethod>C|P)?(?<multiplier>[0-9]*)$");
                            assetMatch = assetRegEx.Match(_assetCode);
                            if (assetMatch.Success)
                            {
                                _contract.Category = "F";
                                _contract.Symbol = assetMatch.Groups["symbol"].Value;
                                string value = _contract.SettltMethod = assetMatch.Groups["settlMethod"].Value;
                                if (StrFunc.IsFilled(value))
                                {
                                    _contract.SettltMethod = value;
                                }
                                else
                                {
                                    _contract.SettltMethod = null;
                                }
                                value = assetMatch.Groups["multiplier"].Value;
                                if (StrFunc.IsFilled(value))
                                {
                                    _contract.ContractMultiplier = int.Parse(value);
                                }
                                else
                                {
                                    _contract.ContractMultiplier = null;
                                }
                            }
                            break;
                        case "C":
                        case "P":
                            // RD 20140207 [19589] Rendre le champ "exerciseStyle" optionnel et considérer la  valeur par défaut "Européen" le cas échéant, pour les raisons suivantes:
                            // - Dans les fichiers sources "CCONTRACTS.C2 et CCONTRSTAT.C2" le champ "exerciseStyle" n'existe pas pour l'Option sur indice "IBX"
                            // - Actuellement il n'y a qu'une seule Option sur indice et qu'en plus, à priori, les Options sur indice sont Européennes sur le MEFF 
                            //   (sinon il y aurait de quoi les différencier dans le code)

                            // Format: C ou P + symbol + exerciseStyle + strike + maturity + multiplier (multiplier et exerciseStyle optionel)
                            // assetRegEx = new Regex(@"^(?<putcall>[C|P])(?<symbol>\w+)(?<exerciseStyle>AM|EU) *(?<strike>[0-9,]+)(?<maturity>[FGHJKMNQUVXZ][0-9][0-9])(?<multiplier>[0-9]*)$");
                            assetRegEx = new Regex(@"^(?<putcall>C|P)(?<symbol>\w\w\w)(?<exerciseStyle>(AM|EU)?) *(?<strike>[0-9,]+)(?<maturity>[FGHJKMNQUVXZ][0-9][0-9])(?<multiplier>[0-9]*)$");
                            assetMatch = assetRegEx.Match(_assetCode);
                            if (assetMatch.Success)
                            {
                                _contract.Category = "O";
                                _putCall = (assetMatch.Groups["putcall"].Value == "C") ? "1" : "0";
                                _contract.Symbol = assetMatch.Groups["symbol"].Value;
                                _contract.ExerciseStyle = (assetMatch.Groups["exerciseStyle"].Value == "AM") ? "1" : "0";
                                string value = assetMatch.Groups["multiplier"].Value;
                                if (StrFunc.IsFilled(value))
                                {
                                    _contract.ContractMultiplier = int.Parse(value);
                                }
                                else
                                {
                                    _contract.ContractMultiplier = null;
                                }
                            }
                            break;
                    }
                }
            }
            #endregion methods
        }

        /// <summary>
        /// Class identifiant un DC sur le Meff
        /// </summary>
        private sealed class MeffContract : IComparable, IEqualityComparer<MeffContract>
        {
            #region members
            /// <summary>
            /// Symbol du DC
            /// </summary>
            public string Symbol = null;
            /// <summary>
            /// Category du DC
            /// </summary>
            public string Category = null;
            /// <summary>
            /// Settlement Method du DC
            /// </summary>
            public string SettltMethod = null;
            /// <summary>
            /// Exercise Style du DC
            /// </summary>
            public string ExerciseStyle = null;
            /// <summary>
            /// Contract Multiplier du DC
            /// </summary>
            public decimal? ContractMultiplier = null;
            #endregion members

            #region IEqualityComparer
            /// <summary>
            /// Les MeffContract sont egaux si tous leurs membres sont égaux
            /// </summary>
            /// <param name="pContractX">1er MeffContract à comparer</param>
            /// <param name="pContractY">2ème MeffContract à comparer</param>
            /// <returns>true si x Equals Y, sinon false</returns>
            public bool Equals(MeffContract pContractX, MeffContract pContractY)
            {
                return (pContractX == pContractY)
                    || ((pContractX != default(MeffContract))
                     && (pContractY != default(MeffContract))
                     && (pContractX.Symbol == pContractY.Symbol)
                     && (pContractX.Category == pContractY.Category)
                     && (pContractX.SettltMethod == pContractY.SettltMethod)
                     && (pContractX.ExerciseStyle == pContractY.ExerciseStyle)
                     && (pContractX.ContractMultiplier == pContractY.ContractMultiplier));
            }

            /// <summary>
            /// La méthode GetHashCode fournissant la même valeur pour des objets MeffContract qui sont égaux.
            /// </summary>
            /// <param name="pMaturity">Le contrat dont on veut le hash code</param>
            /// <returns>La valeur du hash code</returns>
            public int GetHashCode(MeffContract pContract)
            {
                //Vérifier si l'obet est null
                if (pContract is null) return 0;

                //Obtenir le hash code du Symbol du contrat si non null.
                int hashSymbol = pContract.Symbol == null ? 0 : pContract.Symbol.GetHashCode();

                //Obtenir le hash code de la Category du contrat si non null.
                int hashCategory = pContract.Category == null ? 0 : pContract.Category.GetHashCode();

                //Obtenir le hash code de la SettltMethod du contrat si non null.
                int hashSettltMethod = pContract.SettltMethod == null ? 0 : pContract.SettltMethod.GetHashCode();

                //Obtenir le hash code du ExerciseStyle du contrat si non null.
                int hashExerciseStyle = pContract.ExerciseStyle == null ? 0 : pContract.ExerciseStyle.GetHashCode();

                //Obtenir le hash code du Contract Multiplier du contrat si different de 0.
                int hashContractMultiplier = pContract.ContractMultiplier == 0 ? 0 : pContract.ContractMultiplier.GetHashCode();

                //Calcul du hash code pour le contrat.
                return (int)(hashSymbol ^ hashCategory ^ hashSettltMethod ^ hashExerciseStyle ^ hashContractMultiplier);
            }
            #endregion IEqualityComparer

            #region IComparable.CompareTo
            /// <summary>
            /// Compare l'instance courante à une autre
            /// </summary>
            /// <param name="pObj">Autre instance de MeffContract à comparer</param>
            /// <returns>0 si les objets sont égaux</returns>
            public int CompareTo(object pObj)
            {
                if (pObj is MeffContract contract)
                {
                    int ret = -1;
                    if (contract != default(MeffContract))
                    {
                        ret = (Symbol == contract.Symbol ? 0 : (Symbol == null ? -2 : Symbol.CompareTo(contract.Symbol)));
                        if (0 == ret)
                        {
                            ret = (Category == contract.Category ? 0 : (Category == null ? -3 : Category.CompareTo(contract.Category)));
                            if (0 == ret)
                            {
                                ret = (SettltMethod == contract.SettltMethod ? 0 : (SettltMethod == null ? -4 : SettltMethod.CompareTo(contract.SettltMethod)));
                                if (0 == ret)
                                {
                                    ret = (ExerciseStyle == contract.ExerciseStyle ? 0 : (ExerciseStyle == null ? -5 : ExerciseStyle.CompareTo(contract.ExerciseStyle)));
                                    if (0 == ret)
                                    {
                                        ret = (ContractMultiplier == contract.ContractMultiplier ? 0 : (ContractMultiplier == null ? -6 : ContractMultiplier.Value.CompareTo(contract.ContractMultiplier)));
                                    }
                                }
                            }
                        }
                    }
                    return ret;
                }
                throw new ArgumentException("object is not a MeffContract");
            }
            #endregion IComparable.CompareTo
        }

        /// <summary>
        /// Class identifiant le sous-jacent d'un DC sur le Meff
        /// </summary>
        private sealed class MeffUnderlying
        {
            public int AssetId = 0;
            public string AssetCategory = null;
            public string AssetCode = null;
        }
        #endregion private class

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportMEFF(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, true, false, AssetFindMaturityEnum.MATURITYMONTHYEAR, true, true, true, false) { }
        #endregion constructor

        #region methods
        /// <summary>
        /// Lecture de l'IDASSET de la série ETD passée en paramètres, ainsi que le couple ASSETCATEGORY/IDASSET de son sous-jacent (ATTENTION: hors sous-jacent Future).
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <param name="pSerie">Série pour laquelle identifier son IDASSET</param>
        /// <returns>True si l'asset a été trouvé, sinon false</returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private bool GetAssetETDId(string pExchangeAcronym, MeffSerie pSerie)
        {
            bool ret = false;
            QueryParameters queryParameters;
            DataParameters dp;
            if ("F" == pSerie.Category)
            {
                // PL 20220907 Ignorement du style AMERICAN/EUROPEAN de l'Option afin de considérer les sous-jacents Futures quel que soit le style de l'option négociée.
                queryParameters = QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(Cs, m_MaturityType, true, true, false, true);
                dp = queryParameters.Parameters;
                dp["SETTLTMETHOD"].Value = pSerie.SettltMethod;
            }
            else
            {
                queryParameters = QueryExistAssetOptionInTrades(Cs, m_MaturityType, false, true, true, false);
                dp = queryParameters.Parameters;
                dp["EXERCISESTYLE"].Value = pSerie.ExerciseStyle;
                dp["PUTCALL"].Value = pSerie.PutCall;
                dp["STRIKEPRICE"].Value = pSerie.Strike;
            }
            dp["DTFILE"].Value = m_dtFile;
            dp["EXCHANGEACRONYM"].Value = pExchangeAcronym;
            dp["CONTRACTSYMBOL"].Value = pSerie.Symbol;
            dp["CATEGORY"].Value = pSerie.Category;
            dp["CONTRACTMULTIPLIER"].Value = pSerie.ContractMultiplier;
            dp["MATURITYMONTHYEAR"].Value = pSerie.Maturity;

            using (IDataReader dr = DataHelper.ExecuteReader(task.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    ret = true;
                    pSerie.AssetId = IntFunc.IntValue(dr["IDASSET"].ToString());
                    pSerie.AssetUnlCategory = dr["ASSETCATEGORY"].ToString();
                    pSerie.AssetUnlId = IntFunc.IntValue(dr["IDASSET_UNL"].ToString());
                }
            }
            return ret;
        }

  

        

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des cotations et RiskData MEFF
        /// <para>Le fichier contient les enregistrements utiles, chaque enregistrement étant également enrichi en début de record avec 3 nouvelles colonnes: IDASSET,ASSETCATEGORY,SOURCE.</para>
        /// <para>WARNING: ce process n'utilise pas le parsing paramétré, il parse les données en "dur" depuis le fichier de la Clearing House. 
        /// Le parsing paramétré est destiné à l'exploitation du fichier réduit et enrichi.</para>
        /// </summary>
        /// <param name="pOutputFileName">Nom du fichier à créer en sortie</param>
        /// PL 20220505 Nouvelle version avec notamment présence des données ajoutées en début de record 
        public void Create_LightContractFile(string pOutputFileName)
        {
            // ----------------------------------------------------------------------------------------------------------------------------
            // Exemples de ligne à l'origine :
            // ----------------------------------------------------------------------------------------------------------------------------
            //"20210618";"C2";"FMIXN1";"20";"0100";;"20210716";"20210716";"";"FIE";"021";; ; "S";"202107";"ES0B00073063";"";"";"0";"";""
            //"20210618";"C2";"PIBX 9900M21";"20";"0220"; 9900; "20210618";"20210618";"FMIXM1";"FMIXM1";"021";;;"S";"202106";"";"";"";"0";"";""
            //"20210618";"C2";"CIBX10000N21";"20";"0210"; 10000; "20210716";"20210716";"FMIXN1";"FMIXN1";"021";;;"S";"202107";"ES0A02907772";"";"";"0";"";""
            //"20210618";"C2";"FIE";"21";"0240";;"20301231";"20301231";"";"";"021";;;"S";"20301231";"ES0SI0000005";"";"";"0";"";""
            // ----------------------------------------------------------------------------------------------------------------------------
            // Exemples de ligne générées :
            // ----------------------------------------------------------------------------------------------------------------------------
            //10328;"";"ETD-34696";"20210618";"C2";"FMIXN1";"20";"0100";; "20210716";"20210716";"";"FIE";"021";; ; "S";"202107";"ES0B00073063";"";"";"0";"";""
            //13024;"";"ETD-37659";"20210618";"C2";"PIBX 9900M21";"20";"0220"; 9900; "20210618";"20210618";"FMIXM1";"FMIXM1";"021";;;"S";"202106";"";"";"";"0";"";""
            //12099;"";"ETD-37661";"20210618";"C2";"CIBX10000N21";"20";"0210"; 10000; "20210716";"20210716";"FMIXN1";"FMIXN1";"021";;;"S";"202107";"ES0A02907772";"";"";"0";"";""
            //3;"Index";"UNL-52544";"20210618";"C2";"FIE";"21";"0240";;"20301231";"20301231";"";"";"021";; ; "S";"20301231";"ES0SI0000005";"";"";"0";"";""
            // ----------------------------------------------------------------------------------------------------------------------------

            try
            {
                // Dictionnaire des DC traités avec indicateur d'utilisation au sein d'au moins un trade 
                Dictionary<MeffContract, bool> dicDerivativeContract = new Dictionary<MeffContract, bool>(new MeffContract());
                // Liste des UNL pour lesquels un DC est utilisé au sein d'au moins un trade 
                List<MeffUnderlying> lstUnderlying = new List<MeffUnderlying>();
                // Liste des assets Future ajoutés dans le fichier "Light" lors du STEP 1
                List<string> lstAssetFutureWritten = new List<string>();

                #region STEP 1/2 ASSET ETD négociés
                OpenOutputFileName(pOutputFileName);
                OpenInputFileName();

                int guard = 9999999;
                int currentLineNumber = 0;
                bool isOkToWrite;
                while (++currentLineNumber < guard)
                {
                    isOkToWrite = false;
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + currentLineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED STEP 1/2");
                        break;
                    }

                    string[] splittedLine = currentLine.Split(separator, StringSplitOptions.None);
                    m_dtFile = DtFunc.ParseDate(splittedLine[0].Trim(charDelimiter), "yyyyMMdd", null);
                    string contractGroup_exchangeAcronym = splittedLine[1].Trim(charDelimiter);
                    string assetCode = splittedLine[2].Trim(charDelimiter);
                    MeffSerie serie = new MeffSerie(assetCode);
                    bool isDC = (serie.Symbol != null);
                    bool isFuture = false;
                    bool isExistDC = false;

                    if (isDC)
                    {
                        // Tenter d'obtenir d'abord l'existence du DC dans le dictionnaire des DC déjà identifiés
                        if (!dicDerivativeContract.TryGetValue(serie.Contract, out isExistDC))
                        {
                            // Si nouveau DC, on vérifie qu'il est utilisé au sein d'au moins un trade
                            isExistDC = IsExistDcInTrade(Cs, m_dtFile, contractGroup_exchangeAcronym, serie.Symbol, serie.Category, serie.SettltMethod, serie.ExerciseStyle, serie.ContractMultiplier);
                            // PM 20130925 : Si le DC n'est pas trouvé, nouvelle recherche sans le ContractMultiplier
                            if (false == isExistDC)
                            {
                                isExistDC = IsExistDcInTrade(Cs, m_dtFile, contractGroup_exchangeAcronym, serie.Symbol, serie.Category, serie.SettltMethod, serie.ExerciseStyle, (decimal?)null);
                            }
                            dicDerivativeContract.Add(serie.Contract, isExistDC);
                        }
                        if (isExistDC)
                        {
                            isFuture = ("F" == serie.Category);
                            if (!isFuture)
                            {
                                string strikeSource = splittedLine[5];
                                if (StrFunc.IsFilled(strikeSource))
                                {
                                    serie.Strike = DecFunc.DecValue(strikeSource.Replace(',', '.'));
                                }
                            }
                            serie.Maturity = splittedLine[14].Trim(charDelimiter);
                            if (GetAssetETDId(contractGroup_exchangeAcronym, serie))
                            {
                                isOkToWrite = true;

                                currentLine = serie.AssetId.ToString() + separator[0] 
                                            + charDelimiter[0] + charDelimiter[0] + separator[0]
                                            + charDelimiter[0] + "ETD-" + currentLineNumber.ToString() + charDelimiter[0] + separator[0]
                                            + currentLine;

                                // Si le sous-jacent n'est pas déjà dans la liste des sous-jacents et qu'il est identifié, on l'y rajoute 
                                string underlyingAssetCode = splittedLine[8].Trim(charDelimiter);
                                if (StrFunc.IsEmpty(underlyingAssetCode))
                                {
                                    //PL 20220504 Il est dommage de ne pas avoir d'explication sur le fait que splittedLine[8] puisse parfois être "vide"
                                    underlyingAssetCode = splittedLine[9].Trim(charDelimiter);
                                }
                                if ((false == lstUnderlying.Exists(u => u.AssetCode == underlyingAssetCode))
                                    && StrFunc.IsFilled(underlyingAssetCode)
                                    && (serie.AssetUnlId != 0))
                                {
                                    // Seuls sont ajoutés ici des UNL non Future, car les UNL Futures ne sont pas traités par getAssetETDId()

                                    // ***************************************************************************************************************
                                    // PL 20220505 WARNING: Il y a ici une faille potentielle !
                                    //                      On associe un underlyingAssetCode issu du fichier source à in IDASSET issu de Spheres,
                                    //                      sans s'assurer que la valeur de underlyingAssetCode correspond bien au symbole de l'UNL
                                    //                      paramétré dans Spheres. On considère de fait que le paramétrage dans Spheres est correct.
                                    // ***************************************************************************************************************
                                    lstUnderlying.Add(new MeffUnderlying
                                    {
                                        AssetId = serie.AssetUnlId,
                                        AssetCategory = serie.AssetUnlCategory,
                                        AssetCode = underlyingAssetCode
                                    });
                                }
                            }
                        }
                    }

                    if (isOkToWrite)
                    {
                        if (isFuture)
                        {
                            lstAssetFutureWritten.Add(assetCode);
                        }
                        //-------------------------------------------------------------------------------------------------------------------------
                        //Ecriture dans le fichier "light"
                        //-------------------------------------------------------------------------------------------------------------------------
                        IOCommonTools.StreamWriter.WriteLine(currentLine);
                        //-------------------------------------------------------------------------------------------------------------------------
                    }
                }
                CloseAllFiles();
                #endregion

                #region STEP 2/2 Tout UNL sur ETD négociés
                IOCommonTools.OpenFile(pOutputFileName, Cst.WriteMode.APPEND);
                OpenInputFileName();
                currentLineNumber = 0;
                while (++currentLineNumber < guard)
                {
                    isOkToWrite = false;                     
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + currentLineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED STEP 2/2");
                        break;
                    }

                    string[] splittedLine = currentLine.Split(separator, StringSplitOptions.None);

                    m_dtFile = DtFunc.ParseDate(splittedLine[0].Trim(charDelimiter), "yyyyMMdd", null);
                    string contractGroup_exchangeAcronym = splittedLine[1].Trim(charDelimiter);
                    string assetCode = splittedLine[2].Trim(charDelimiter);
                    MeffSerie serie = new MeffSerie(assetCode);

                    if (("O" != serie.Category)) //Future ou Sous-Jacent Equity ou Index ou ... (PL: on pourrait exclure les UNL Futures car déjà traité dans STEP 1)
                    {
                        if ("F" == serie.Category)
                        {
                            if (!lstAssetFutureWritten.Contains(assetCode))
                            {
                                // Si Future, non traité au STEP 1, tentative d'identification (Potentiellement utile pour maj du code ISIN sur un Future paramétré, mais non négocié)
                                // ---------------------------------------------------------------------------------------------------------------------------------------------------
                                // PL 20220907 *** WARNING ***
                                // Cette étape est à revoir car "getAssetETDId" ne considère que les assets négociés ! 
                                // Par conséquent, un Future qui n'aurait pas été traité lors du STEP 1 ci-dessus, ne le sera pas plus ici.  
                                // Il faut pouvoir disposer d'une méthode qui retourne l'IDASSET sans considérer la table TRADE...
                                // ---------------------------------------------------------------------------------------------------------------------------------------------------
                                serie.Maturity = splittedLine[14].Trim(charDelimiter);
                                if (GetAssetETDId(contractGroup_exchangeAcronym, serie))
                                {
                                    isOkToWrite = true;

                                    currentLine = serie.AssetId.ToString() + separator[0] 
                                                + charDelimiter[0] + charDelimiter[0] + separator[0]
                                                + charDelimiter[0] + "FUT-" + currentLineNumber.ToString() + charDelimiter[0] + separator[0]
                                                + currentLine;
                                }
                            }
                        }
                        else
                        {
                            // Vérifier s'il s'agit d'un sous-jacent identifié lors du STEP 1
                            MeffUnderlying underlyingAsset = lstUnderlying.FirstOrDefault(u => u.AssetCode == assetCode);
                            if (underlyingAsset != default(MeffUnderlying))
                            {
                                isOkToWrite = true; 
                                
                                currentLine = underlyingAsset.AssetId.ToString() + separator[0] 
                                            + charDelimiter[0] + underlyingAsset.AssetCategory + charDelimiter[0] + separator[0]
                                            + charDelimiter[0] + "UNL-" + currentLineNumber.ToString() + charDelimiter[0] + separator[0]
                                            + currentLine;
                            }
                        }
                        if (isOkToWrite)
                        {
                            //-------------------------------------------------------------------------------------------------------------------------
                            //Ecriture dans le fichier "light"
                            //-------------------------------------------------------------------------------------------------------------------------
                            IOCommonTools.StreamWriter.WriteLine(currentLine);
                            //-------------------------------------------------------------------------------------------------------------------------
                        }
                    }
                }
                #endregion
            }
            catch (Exception) { throw; }
            finally
            {
                CloseAllFiles();
            }
        }

        /// <summary>
        /// Création d'un fichier "Light" pour l'importation des cotations et RiskData MEFF
        /// <para>Le fichier contient les enregistrements strictement nécessaires en se basant sur le contenu de la table IMMEFFCONTRACT_H</para>
        /// </summary>
        /// <param name="pOutputFileName">Nom du fichier à créer en sortie</param>
        public void Create_LightRiskDataFile(string pOutputFileName)
        {
            IDataReader dr = default;
            try
            {
                DateTime fisrtDtFile = default;
                List<MeffAssetCode> meffAssetImported = default;

                OpenOutputFileName(pOutputFileName);

                OpenInputFileName();

                int guard = 9999999;
                int currentLineNumber = 0;
                while (++currentLineNumber < guard)
                {
                    bool isOkToCopy = false;
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + currentLineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED");
                        break;
                    }

                    string[] splittedLine = currentLine.Split(separator, StringSplitOptions.None);

                    m_dtFile = DtFunc.ParseDate(splittedLine[0].Trim(charDelimiter), "yyyyMMdd", null);
                    string contractGroup = splittedLine[1].Trim(charDelimiter);
                    string assetCode = splittedLine[2].Trim(charDelimiter);

                    if (1 == currentLineNumber)
                    {
                        QueryParameters queryMeffAsset = MeffAssetCodeReader.Query(task.Cs);
                        fisrtDtFile = m_dtFile;
                        queryMeffAsset.Parameters["DTFILE"].Value = m_dtFile;
                        dr = DataHelper.ExecuteReader(task.Cs, CommandType.Text, queryMeffAsset.Query.ToString(), queryMeffAsset.Parameters.GetArrayDbParameter());

                        meffAssetImported = (from meffAsset
                                            in dr.DataReaderEnumerator<MeffAssetCode, MeffAssetCodeReader>()
                                             select meffAsset).ToList();
                    }
                    if ((fisrtDtFile == m_dtFile) && (default(List<MeffAssetCode>) != meffAssetImported))
                    {
                        MeffAssetCode meffAsset = meffAssetImported.FirstOrDefault(a => (a.ContractGroup == contractGroup) && (a.AssetCode == assetCode));
                        isOkToCopy = (meffAsset != default(MeffAssetCode));

                        if (isOkToCopy)
                        {
                            //Ecriture dans le fichier "light"
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //IOTools.StreamWriter.WriteLine(currentLine);
                            IOCommonTools.StreamWriter.WriteLine(currentLine);
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (dr != default(IDataReader))
                {
                    dr.Close();
                    dr.Dispose();
                }
                CloseAllFiles();
            }
        }
        #endregion methods
    }
}
