using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;

using FpML.Enum;

namespace EFS.SpheresIO.MarketData
{

    /// <summary>
    /// Classe de base pour intervention sur fichiers issus des marchés
    /// </summary>
    internal abstract class MarketDataImportBase
    {
        /// <summary>
        ///  Recherche d'un asset à partir d'une échéance ou d'un date de maturité 
        /// </summary>
        /// FI 20131121 [19224] add 
        public enum AssetFindMaturityEnum
        {
            /// <summary>
            /// Echéance
            /// </summary>
            MATURITYMONTHYEAR,
            /// <summary>
            /// Date de maturité  
            /// </summary>
            MATURITYDATE
        }

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected string Cs
        {
            get { return task.Cs; }
        }
        /// <summary>
        /// Obtient le résultat du parsing de dataStyle en Cst.InputSourceDataStyle
        /// </summary>
        protected Cst.InputSourceDataStyle InputSourceDataStyle
        {
            get
            {
                Cst.InputSourceDataStyle ret = (Cst.InputSourceDataStyle)Enum.Parse(typeof(Cst.InputSourceDataStyle), this.dataStyle);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true si la tâche d'importation possède le paramètre PRICEONLY et qu'il est valorisé à True
        /// <para>Obtient false dans les autres cas</para>
        /// </summary>
        /// FI 20140617 [19911] add property
        protected Boolean IsImportPriceOnly
        {
            get
            {
                Boolean ret = false;
                if (task.IoTask.IsParametersSpecified && task.IoTask.ExistTaskParam("PRICEONLY"))
                    ret = BoolFunc.IsTrue(task.IoTask.parameters["PRICEONLY"]);
                return ret;
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// Task qui exécute l'importation
        /// </summary>
        protected Task task;
        /// <summary>
        /// URL du fichier Source
        /// </summary>
        protected string dataName;
        /// <summary>
        /// Type de fichier Source
        /// </summary>
        protected string dataStyle;
        /// <summary>
        /// Date des fichiers
        /// </summary>
        protected DateTime m_dtFile;

        /// <summary>
        /// Nbr de ligne ignorées par parsing 
        /// <para>utilisé par la méthode LoadLine</para>
        /// </summary>
        protected int nbParsingIgnoredLines;
        /// <summary>
        /// Liste des parsings de l'élément d'importation 
        /// </summary>
        protected InputParsing inputParsing;
        /// <summary>
        /// Date de début d'un traitement d'intégration
        /// </summary>
        protected DateTime dtStart;

        protected bool m_IsUseISO10383_ALPHA4;
        protected bool m_IsUseDcCategory;
        protected bool m_IsUseContractAttrib;
        protected bool m_IsUseDcOptionalSettltMethod = false;
        protected bool m_IsUseDcExerciseStyle = false;
        protected bool m_IsUseDcContractMultiplier = false;
        protected bool m_IsUseDcCategoryForUnderlying = false;
        /// <summary>
        /// Indique si le fichier contient des noms d'échéance ou des dates d'échéance
        /// </summary>
        protected AssetFindMaturityEnum m_MaturityType = AssetFindMaturityEnum.MATURITYMONTHYEAR;

        /// <summary>
        /// Requête SQL qui retourne les propriétés IDDC, ISAUTOSETTING et ISAUTOSETTINGASSET d'un DC
        /// </summary>
        protected QueryParameters m_QueryExistDC;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDASSET d'un asset Future 
        /// </summary>
        protected QueryParameters m_QueryExistAssetFut;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDASSET d'un asset Option
        /// </summary>
        protected QueryParameters m_QueryExistAssetOpt;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDA d'une chambre si un contrat a déjà été négocié sur un des marchés qui lui sont rattachés
        /// </summary>
        /// PM 20151218 Add m_QueryExistClearingInTrade
        protected QueryParameters m_QueryExistClearingInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDM d'un marché si un contrat a déjà été négocié sur celui-ci
        /// </summary>
        /// PM 20150807 Add m_QueryExistExchangeInTrade
        protected QueryParameters m_QueryExistExchangeInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDDC d'un contrat s'il est déjà négocié
        /// </summary>
        protected QueryParameters m_QueryExistDCInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDDC d'un contrat s'il est déjà négocié pour une maturité donnée
        /// </summary>
        protected QueryParameters m_QueryExistDCMaturityInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDASSET d'un Asset déjà négocié pour une maturité donnée (la maturité est exprimé sous forme de date)
        /// </summary>
        protected QueryParameters m_QueryExistAssetMaturityDateInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDASSET d'un asset sous-jacent ou la propriété IDDC d'un contract future d'un contrat si ce dernier a déjà été négocié
        /// </summary>
        protected QueryParameters m_QueryExistDCUnderlyingInTrade;
        /// <summary>
        /// Requête SQL qui retourne la propriété IDDC d'un contrat s'il peut résulter d'un contrat qui a déjà été négocié
        /// </summary>
        /// PM 20150911 [21336] New
        protected QueryParameters m_QueryExistDCFromCascading;

        protected string m_lastDC_ContractSymbol = string.Empty;
        protected string m_lastDC_Category = string.Empty;
        //PM 20150824 m_isExistPreviousDC_ContractSymbol devient inutilisé de fait du partage du code entre GetIdDcInTrade() et IsExistDcInTrade()
        //protected bool m_isExistPreviousDC_ContractSymbol = false;
        //PM 20140702 [20163][20157] Add m_lastDC_Exchange
        protected string m_lastDC_Exchange = string.Empty;
        // PM 20151218 Add m_lastExch_ExchangeClearing & m_isExistPreviousClearing
        protected string m_lastExch_ExchangeClearing = string.Empty;
        protected bool m_isExistPreviousClearing = false;
        // PM 20150807 Add m_lastExch_Exchange & m_isExistPreviousExchange
        protected string m_lastExch_Exchange = string.Empty;
        protected bool m_isExistPreviousExchange = false;
        // PM 20150824 Add 
        protected int? m_PreviousIDDC = null;

        /// <summary>
        /// Dictionnaire des contrats sujets à CA (en date J+1JO)
        /// Clé = ContractSymbol
        /// Value = Pair(IdAsset du sous-jacent, Catégory du sous-jacent)
        /// </summary>
        protected Dictionary<Pair<string,string>, Pair<int,Cst.UnderlyingAsset>> m_LstContractSubjectToCA;

        /// <summary>
        /// Requête SELECT pour la table de cotation {0}
        /// </summary>
        /// PM 20151124 [20124] Ajout (déplacée ici à partir de SpanXml.Cs)
        protected const string SelectQUOTE_XXX_HQuery =
            @"select * from dbo.{0}
               where (IDMARKETENV = @IDMARKETENV)
                 and (IDVALSCENARIO = @IDVALSCENARIO)
                 and (IDASSET = @IDASSET)
                 and (TIME = @TIME)
                 and ((IDBC = @IDBC) or ((IDBC is null) and (@IDBC is null)))
                 and (QUOTESIDE = @QUOTESIDE)
                 and ((CASHFLOWTYPE = @CASHFLOWTYPE) or ((CASHFLOWTYPE is null) and (@CASHFLOWTYPE is null)))";

        /// <summary>
        /// Environement de marché pour l'importation des cotations 
        /// </summary>
        /// PM 20151124 [20124] Ajout (déplacée ici à partir de SpanXml.Cs)
        protected string m_IdMarketEnv;
        /// <summary>
        /// Scénario d'évaluation pour l'importation des cotations 
        /// </summary>
        /// PM 20151124 [20124] Ajout (déplacée ici à partir de SpanXml.Cs)
        protected string m_IdValScenario;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pIsUseISO10383_ALPHA4">Si à True, utilisation de ISO10383_ALPHA4 pour la recherche du marché, sinon utilisation de EXCHANGESYMBOL</param>        
        public MarketDataImportBase(Task pTask, string pDataName, string pDataStyle, bool pIsUseISO10383_ALPHA4)
            : this(pTask, pDataName, pDataStyle, false, false, AssetFindMaturityEnum.MATURITYMONTHYEAR, false, false, false, pIsUseISO10383_ALPHA4) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportBase(Task pTask, string pDataName, string pDataStyle)
            : this(pTask, pDataName, pDataStyle, false, false, AssetFindMaturityEnum.MATURITYMONTHYEAR, false, false, false, false) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseContractAttrib"></param>
        /// <param name="pMaturityType">MATURITYMONTHYEAR ou MATURITYDATE</param>
        public MarketDataImportBase(Task pTask, string pDataName, string pDataStyle, bool pIsUseDcCategory, bool pIsUseContractAttrib, AssetFindMaturityEnum pMaturityType)
            : this(pTask, pDataName, pDataStyle, pIsUseDcCategory, pIsUseContractAttrib, pMaturityType, false, false, false, false) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseContractAttrib"></param>
        /// <param name="pMaturityType">MATURITYMONTHYEAR ou MATURITYDATE</param>
        /// <param name="pIsUseOptionalSettltMethod">Utilisation, si présente, de la méthode de livraison</param>
        /// <param name="pIsUseExerciseStyle">Si à True, implique pIsUseDcCategory à True</param>
        /// <param name="pIsUseContractMultiplier">Utilisation si présent du contract multiplier</param>
        /// <param name="pIsUseISO10383_ALPHA4">Si à True, utilisation de ISO10383_ALPHA4 pour la recherche du marché, sinon utilisation de EXCHANGESYMBOL</param>
        // RD 20171130 [23504] Gestion de pIsUseISO10383_ALPHA4
        public MarketDataImportBase(Task pTask, string pDataName, string pDataStyle, bool pIsUseDcCategory, bool pIsUseContractAttrib,
            AssetFindMaturityEnum pMaturityType, bool pIsUseOptionalSettltMethod, bool pIsUseExerciseStyle, bool pIsUseContractMultiplier, bool pIsUseISO10383_ALPHA4)
        {
            task = pTask;

            dataName = pDataName;
            dataStyle = pDataStyle;

            m_IsUseISO10383_ALPHA4 = pIsUseISO10383_ALPHA4;
            m_IsUseDcCategory = pIsUseDcCategory;
            m_IsUseContractAttrib = pIsUseContractAttrib;
            m_IsUseDcOptionalSettltMethod = pIsUseOptionalSettltMethod;
            m_IsUseDcExerciseStyle = pIsUseExerciseStyle;
            m_IsUseDcContractMultiplier = pIsUseContractMultiplier;
            m_MaturityType = pMaturityType;
            if (pIsUseExerciseStyle)
            {
                m_IsUseDcCategory = true;
            }

            //Clearing Query
            SetQueryExistClearingInTrade();
            //Market Query
            SetQueryExistExchangeInTrade();

            //DC Query
            SetQueryExistDC();
            SetQueryExistDCInTrade();
            SetQueryExistDCMaturityInTrades();
            SetQueryExistDCFromCascading();

            //ASSET QUERY
            SetQueryExistAsset();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Ouvre le fichier source en lecture 
        /// <para></para>
        /// </summary>
        protected void OpenInputFileName()
        {
            IOCommonTools.OpenFile(dataName, dataStyle);
        }

        /// <summary>
        /// Ouvre le fichier {pFileName} en écriture
        /// </summary>
        /// <param name="pFileName"></param>
        protected static void OpenOutputFileName(string pFileName)
        {
            // PM 20180219 [23824] IOTools => IOCommonTools
            //IOTools.OpenFile(pFileName, Cst.WriteMode.WRITE);
            IOCommonTools.OpenFile(pFileName, Cst.WriteMode.WRITE);
        }

        /// <summary>
        /// Fermer tous les flux ouverts 
        /// </summary>
        protected static void CloseAllFiles()
        {
            // PM 20180219 [23824] IOTools => IOCommonTools
            //IOTools.CloseFile();
            IOCommonTools.CloseFile();
        }

        /// <summary>
        /// Parse une ligne 
        /// <para>Alimente le compteur des lignes ignorées si la ligne est ignorée par le parsing</para>
        /// </summary>
        /// <param name="pLine">Représente la ligne en entrée</param>
        /// <param name="opRow">Résultat du parsing</param>
        protected void LoadLine(string pLine, ref IOTaskDetInOutFileRow opRow)
        {
            if (null == inputParsing)
            {
                throw new Exception("inputParsing is null, Please call method InitLoadLine");
            }

            IOCommonTools.LoadLine(task, pLine, 1, 1, inputParsing, ref opRow, ref nbParsingIgnoredLines);
        }

        /// <summary>
        /// Transco du PutCall:
        /// <para>"P" donne "0"</para>
        /// <para>"C" donne "1"</para>
        /// </summary>
        /// <param name="pPutCall"></param>
        /// <returns></returns>
        protected static string GetPutCall(string pPutCall)
        {
            return (pPutCall == "P" ? "0" : (pPutCall == "C" ? "1" : pPutCall));
        }

        /// <summary>
        /// Identifier la category:
        /// <para>"P" ou "C" donne "O"</para>
        /// <para>"F" donne "F"</para>
        /// </summary>
        /// <param name="pPutCall"></param>
        /// <returns></returns>
        protected static string GetCategory(string pPutCall)
        {
            string ret = pPutCall;

            switch (pPutCall)
            {
                case "P":
                case "C":
                    ret = "O";
                    break;
                case "F":
                    ret = "F";
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Transco du ExerciseStyle:
        /// <para>"E" ou "EU" donne "0"</para>
        /// <para>"A" ou "AM" donne "1"</para>
        /// </summary>
        /// <param name="pPutCall"></param>
        /// <returns></returns>
        protected static string GetExerciseStyle(string pExerciseStyle)
        {
            string ret = pExerciseStyle;

            switch (pExerciseStyle)
            {
                case "E":
                case "EU":
                    ret = "0";
                    break;
                case "A":
                case "AM":
                    ret = "1";
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Calcul le Strike Price en fonction de {pStrikePriceDivisor}
        /// <para>StrikePrice = {pStrikePrice} / (10 ^ {pStrikePriceDivisor})</para>
        /// </summary>
        /// <param name="pStrikePrice"></param>
        /// <param name="pStrikePriceDivisor"></param>
        /// <returns></returns>
        protected static decimal GetStrikePrice(string pStrikePrice, string pStrikePriceDivisor)
        {
            return (Convert.ToDecimal(pStrikePrice) / Convert.ToDecimal((Math.Pow(10, Convert.ToInt32(pStrikePriceDivisor)))));
        }

        /// <summary>
        /// Retourne les paramètres de la requête ExistAssetFut ou ExistAssetOpt
        /// </summary>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        protected DataParameters GetQueryExistAssetDataParameters(string pCategory)
        {
            DataParameters ret = null;

            if (pCategory == "F")
                ret = m_QueryExistAssetFut.Parameters;
            else if (pCategory == "O")
                ret = m_QueryExistAssetOpt.Parameters;

            return ret;
        }

        /// <summary>
        /// Valorise le DataParameter {pParameterName} de la requête ExistAssetFut ou ExistAssetOpt
        /// </summary>
        /// <param name="pCategory"></param>
        /// <param name="pParameterName"></param>
        /// <param name="pParameterValue"></param>
        protected void SetQueryExistAssetParameter(string pCategory, string pParameterName, object pParameterValue)
        {
            if (pCategory == "F")
                m_QueryExistAssetFut.Parameters[pParameterName].Value = pParameterValue;
            else if (pCategory == "O")
                m_QueryExistAssetOpt.Parameters[pParameterName].Value = pParameterValue;
        }

        #region Methods protected bool IsExists...
        #region protected bool IsExistClearingInTrade
        /// <summary>
        /// Vérification de l'existance d'au moins un Trade sur un marché d'une clearing
        /// </summary>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange</param>
        /// <returns></returns>
        /// PM 20151218 New
        protected bool IsExistClearingInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym)
        {
            bool exist = false;
            if (StrFunc.IsFilled(pExchangeAcronym))
            {
                if (pExchangeAcronym == m_lastExch_ExchangeClearing)
                {
                    exist = m_isExistPreviousClearing;
                }
                else
                {
                    m_QueryExistClearingInTrade.Parameters["DTFILE"].Value = pDtFile;
                    m_QueryExistClearingInTrade.Parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;

                    object obj_Exch_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, m_QueryExistClearingInTrade.Query, m_QueryExistClearingInTrade.Parameters.GetArrayDbParameter());

                    exist = (obj_Exch_Trade != null);

                    m_lastExch_ExchangeClearing = pExchangeAcronym;
                    m_isExistPreviousClearing = exist;
                }
            }
            return exist;
        }
        #endregion protected bool IsExistClearingInTrade

        #region protected bool IsExistExchangeInTrade
        /// <summary>
        /// Vérification de l'existance d'au moins un Trade sur le marché
        /// </summary>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange</param>
        /// <returns></returns>
        /// PM 20150807 New
        protected bool IsExistExchangeInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym)
        {
            bool exist = false;
            if (StrFunc.IsFilled(pExchangeAcronym))
            {
                if (pExchangeAcronym == m_lastExch_Exchange)
                {
                    exist = m_isExistPreviousExchange;
                }
                else
                {
                    m_QueryExistExchangeInTrade.Parameters["DTFILE"].Value = pDtFile;
                    m_QueryExistExchangeInTrade.Parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;

                    object obj_Exch_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, m_QueryExistExchangeInTrade.Query, m_QueryExistExchangeInTrade.Parameters.GetArrayDbParameter());

                    exist = (obj_Exch_Trade != null);

                    m_lastExch_Exchange = pExchangeAcronym;
                    m_isExistPreviousExchange = exist;
                }
            }
            return exist;
        }
        #endregion protected bool IsExistExchangeInTrade

        #region protected bool IsExistDcInTrade
        /// <summary>
        /// Vérification de l'existance du DC sur au moins un Trade
        /// </summary>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les trades sur les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <returns></returns>
        protected bool IsExistDcInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol)
        {
            return IsExistDcInTrade(pCS, pDtFile, pExchangeAcronym, pContractSymbol, null);
        }

        /// <summary>
        /// Vérification de l'existance du DC sur au moins un Trade
        /// </summary>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <returns></returns>
        protected bool IsExistDcInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory)
        {
            return IsExistDcInTrade(pCS,pDtFile, pExchangeAcronym, pContractSymbol, pCategory, null, null, null);
        }

        // PM 20150824 Partage du code avec GetIdDcInTrade
        ///// <summary>
        ///// Vérification de l'existance du DC sur au moins un Trade
        ///// </summary>
        ///// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        ///// <param name="pExchangeAcronym">Exchange du DC</param>
        ///// <param name="pContractSymbol">Symbol du DC</param>
        ///// <param name="pCategory">Category du DC</param>
        ///// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        ///// <param name="pExerciseStyle">Style d'exercise du DC</param>
        ///// <returns></returns>
        //protected bool IsExistDcInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle, decimal? pContractMultiplier)
        //{
        //    bool exist = true;

        //    if (StrFunc.IsEmpty(pExchangeAcronym) || StrFunc.IsEmpty(pContractSymbol))
        //    {
        //        exist = false;
        //    }
        //    else
        //    {
        //        //PM 20140702 [20163][20157] Add m_lastDC_Exchange
        //        if ((pExchangeAcronym == m_lastDC_Exchange)
        //            && (pContractSymbol == m_lastDC_ContractSymbol)
        //            && (!m_IsUseDcCategory || (pCategory == m_lastDC_Category))
        //            && (!m_IsUseDcExerciseStyle)
        //            && (!m_IsUseDcOptionalSettltMethod)
        //            && (!m_IsUseDcContractMultiplier))
        //        {
        //            exist = m_isExistPreviousDC_ContractSymbol;
        //        }
        //        else
        //        {
        //            m_QueryExistDCInTrade.parameters["DTFILE"].Value = pDtFile;
        //            m_QueryExistDCInTrade.parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;
        //            m_QueryExistDCInTrade.parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

        //            if (m_IsUseDcCategory)
        //            {
        //                m_QueryExistDCInTrade.parameters["CATEGORY"].Value = pCategory;
        //                m_lastDC_Category = pCategory;
        //                if (m_IsUseDcExerciseStyle)
        //                {
        //                    if (pCategory == "O")
        //                    {
        //                        m_QueryExistDCInTrade.parameters["EXERCISESTYLE"].Value = pExerciseStyle;
        //                    }
        //                    else
        //                    {
        //                        m_QueryExistDCInTrade.parameters["EXERCISESTYLE"].Value = null;
        //                    }
        //                }
        //            }
        //            if (m_IsUseDcOptionalSettltMethod)
        //            {
        //                m_QueryExistDCInTrade.parameters["SETTLTMETHOD"].Value = pOptionalSettltMethod;
        //            }
        //            if (m_IsUseDcContractMultiplier)
        //            {
        //                m_QueryExistDCInTrade.parameters["CONTRACTMULTIPLIER"].Value = pContractMultiplier;
        //            }


        //            object obj_Dc_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, m_QueryExistDCInTrade.query, m_QueryExistDCInTrade.parameters.GetArrayDbParameter());

        //            exist = (obj_Dc_Trade != null);

        //            m_lastDC_ContractSymbol = pContractSymbol;
        //            m_isExistPreviousDC_ContractSymbol = exist;
        //            //PM 20140702 [20163][20157] Add m_lastDC_Exchange
        //            m_lastDC_Exchange = pExchangeAcronym;
        //        }
        //    }
        //    return exist;
        //}
        /// <summary>
        /// Vérification de l'existance du DC sur au moins un Trade
        /// </summary>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        /// <param name="pExerciseStyle">Style d'exercise du DC</param>
        /// <returns></returns>
        /// PM 20150824 New : Partage du code avec GetIdDcInTrade
        protected bool IsExistDcInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle, decimal? pContractMultiplier)
        {
            return (null != GetIdDCInTrade(pCS, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, pOptionalSettltMethod, pExerciseStyle, pContractMultiplier));
        }
        #endregion protected bool IsExistDcInTrade

        #region protected bool IsExistDcUnderlyingInTrade
        /// <summary>
        /// Vérification de l'existance du sous-jacent d'un DC sur au moins un Trade
        /// </summary>
        /// <param name="pAssetCategory">Categorie de l'asset sous-jacent</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pSymbol">Symbol du sous-jacent du DC</param>
        /// <returns></returns>
        protected bool IsExistDcUnderlyingInTrade(string pCS, Cst.UnderlyingAsset pAssetCategory, DateTime pDtFile, string pExchangeAcronym, string pSymbol)
        {
            return IsExistDcUnderlyingInTrade(pCS, pAssetCategory, pDtFile, pExchangeAcronym, pSymbol, null, null, null);
        }

        /// <summary>
        /// Vérification de l'existance du sous-jacent d'un DC sur au moins un Trade
        /// </summary>
        /// <param name="pAssetCategory">Categorie de l'asset sous-jacent</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pSymbol">Symbol du sous-jacent du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <returns></returns>
        protected bool IsExistDcUnderlyingInTrade(string pCS, Cst.UnderlyingAsset pAssetCategory, DateTime pDtFile, string pExchangeAcronym, string pSymbol, string pCategory)
        {
            return IsExistDcUnderlyingInTrade(pCS, pAssetCategory, pDtFile, pExchangeAcronym, pSymbol, pCategory, null, null);
        }

        /// <summary>
        /// Vérification de l'existance du sous-jacent d'un DC sur au moins un Trade
        /// </summary>
        /// <param name="pAssetCategory">Categorie de l'asset sous-jacent</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pSymbol">Symbol du sous-jacent du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        /// <param name="pExerciseStyle">Style d'exercise du DC</param>
        /// <returns></returns>
        protected bool IsExistDcUnderlyingInTrade(string pCS, Cst.UnderlyingAsset pAssetCategory, DateTime pDtFile, string pExchangeAcronym, string pSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle)
        {
            bool exist;
            if (StrFunc.IsEmpty(pExchangeAcronym) || StrFunc.IsEmpty(pSymbol))
            {
                exist = false;
            }
            else
            {
                SetQueryExistDCUnderlyingInTrade(pAssetCategory);

                m_QueryExistDCUnderlyingInTrade.Parameters["DTFILE"].Value = pDtFile;
                m_QueryExistDCUnderlyingInTrade.Parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;
                m_QueryExistDCUnderlyingInTrade.Parameters["SYMBOL"].Value = pSymbol;

                if (m_IsUseDcCategoryForUnderlying)
                {
                    m_QueryExistDCUnderlyingInTrade.Parameters["CATEGORY"].Value = pCategory;
                    if (m_IsUseDcExerciseStyle)
                    {
                        if (pCategory == "O")
                        {
                            m_QueryExistDCUnderlyingInTrade.Parameters["EXERCISESTYLE"].Value = pExerciseStyle;
                        }
                        else
                        {
                            m_QueryExistDCUnderlyingInTrade.Parameters["EXERCISESTYLE"].Value = null;
                        }
                    }
                }
                if (m_IsUseDcOptionalSettltMethod)
                {
                    m_QueryExistDCUnderlyingInTrade.Parameters["SETTLTMETHOD"].Value = pOptionalSettltMethod;
                }

                object obj_Dc_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, m_QueryExistDCUnderlyingInTrade.Query, m_QueryExistDCUnderlyingInTrade.Parameters.GetArrayDbParameter());

                exist = (obj_Dc_Trade != null);
            }
            return exist;
        }
        #endregion protected bool IsExistDcUnderlyingInTrade
        #endregion Methods protected bool IsExists...

        #region Methods int? GetIdDC...
        /// <summary>
        /// Lecture de l'IDDC d'un DC s'il a existé sur au moins un Trade, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <returns></returns>
        /// PM 20150824 New
        protected int? GetIdDCInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory)
        {
            return GetIdDCInTrade(pCS, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, null, null, null);
        }

        /// <summary>
        /// Lecture de l'IDDC d'un DC s'il a existé sur au moins un Trade, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        /// <param name="pExerciseStyle">Style d'exercise du DC</param>
        /// <param name="pContractMultiplier">Contract Multiplier du DC</param>
        /// <returns>IDDC ou null</returns>
        /// PM 20150911 [21336] New pour compatibilité
        protected int? GetIdDCInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle, decimal? pContractMultiplier)
        {
            int? idDC = GetIdDCInTrade(pCS, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, pOptionalSettltMethod, pExerciseStyle, pContractMultiplier, false);
            return idDC;
        }

        /// <summary>
        /// Lecture de l'IDDC d'un DC s'il a existé sur au moins un Trade, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">Exchange du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pIsCheckCascading">Indique s'il faut vérifier les DC issu d'un cascading</param>
        /// <returns></returns>
        /// PM 20150911 [21336] New
        protected int? GetIdDCInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, bool pIsCheckCascading)
        {
            return GetIdDCInTrade(pCS, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, null, null, null, pIsCheckCascading);
        }

        /// <summary>
        /// Lecture de l'IDDC d'un DC s'il a existé sur au moins un Trade, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym du DC</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        /// <param name="pExerciseStyle">Style d'exercise du DC</param>
        /// <param name="pContractMultiplier">Contract Multiplier du DC</param>
        /// <param name="pIsCheckCascading">Indique s'il faut vérifier les DC issu d'un cascading</param>
        /// <returns>IDDC ou null</returns>
        /// PM 20150824 New
        /// PM 20150911 [21336] Ajout paramètre pIsCheckCascading et refactoring pour partage code
        protected int? GetIdDCInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle, decimal? pContractMultiplier, bool pIsCheckCascading)
        {
            int? idDC = null;

            if (StrFunc.IsFilled(pExchangeAcronym) && StrFunc.IsFilled(pContractSymbol))
            {
                if ((pExchangeAcronym == m_lastDC_Exchange)
                    && (pContractSymbol == m_lastDC_ContractSymbol)
                    && (!m_IsUseDcCategory || (pCategory == m_lastDC_Category))
                    && (!m_IsUseDcExerciseStyle)
                    && (!m_IsUseDcOptionalSettltMethod)
                    && (!m_IsUseDcContractMultiplier))
                {
                    idDC = m_PreviousIDDC;
                }
                else
                {
                    // PM 20150911 [21336] Ajout paramètre pIsCheckCascading et refactoring pour partage code : utilisation de GetIdDC()
                    //m_QueryExistDCInTrade.parameters["DTFILE"].Value = pDtFile;
                    //m_QueryExistDCInTrade.parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;
                    //m_QueryExistDCInTrade.parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

                    //if (m_IsUseDcCategory)
                    //{
                    //    m_QueryExistDCInTrade.parameters["CATEGORY"].Value = pCategory;
                    //    m_lastDC_Category = pCategory;
                    //    if (m_IsUseDcExerciseStyle)
                    //    {
                    //        if (pCategory == "O")
                    //        {
                    //            m_QueryExistDCInTrade.parameters["EXERCISESTYLE"].Value = pExerciseStyle;
                    //        }
                    //        else
                    //        {
                    //            m_QueryExistDCInTrade.parameters["EXERCISESTYLE"].Value = null;
                    //        }
                    //    }
                    //}
                    //if (m_IsUseDcOptionalSettltMethod)
                    //{
                    //    m_QueryExistDCInTrade.parameters["SETTLTMETHOD"].Value = pOptionalSettltMethod;
                    //}
                    //if (m_IsUseDcContractMultiplier)
                    //{
                    //    m_QueryExistDCInTrade.parameters["CONTRACTMULTIPLIER"].Value = pContractMultiplier;
                    //}

                    //object obj_Dc_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, m_QueryExistDCInTrade.query, m_QueryExistDCInTrade.parameters.GetArrayDbParameter());

                    //if (obj_Dc_Trade != null)
                    //{
                    //    idDC = Convert.ToInt32(obj_Dc_Trade);
                    //}

                    idDC = GetIdDC(pCS, m_QueryExistDCInTrade, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, pOptionalSettltMethod, pExerciseStyle, pContractMultiplier);
                    if (pIsCheckCascading && (idDC == null))
                    {
                        idDC = GetIdDC(pCS, m_QueryExistDCFromCascading, pDtFile, pExchangeAcronym, pContractSymbol, pCategory, pOptionalSettltMethod, pExerciseStyle, pContractMultiplier);
                    }

                    m_lastDC_ContractSymbol = pContractSymbol;
                    m_PreviousIDDC = idDC;
                    m_lastDC_Exchange = pExchangeAcronym;
                }
            }
            return idDC;
        }

        /// <summary>
        /// Lecture de l'IDDC d'un DC un fonction de la requête d'existance reçu en paramètre, sinon return null.
        /// Attention: ici il n'y a pas de comparaison avec les éléments de la recherche précédante
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pQueryExistDC">Requête de vérification d'existance d'un DC</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pCategory">Category du DC</param>
        /// <param name="pOptionalSettltMethod">Méthode de livraison du DC</param>
        /// <param name="pExerciseStyle">Style d'exercise du DC</param>
        /// <param name="pContractMultiplier">Contract Multiplier du DC</param>
        /// <returns>IDDC ou null</returns>
        /// PM 20150911 [21336] New
        private int? GetIdDC(string pCS, QueryParameters pQueryExistDC, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol, string pCategory, string pOptionalSettltMethod, string pExerciseStyle, decimal? pContractMultiplier)
        {
            int? idDC = null;

            if (pQueryExistDC != default (QueryParameters))
            {
                pQueryExistDC.Parameters["DTFILE"].Value = pDtFile;
                pQueryExistDC.Parameters["EXCHANGEACRONYM"].Value = pExchangeAcronym;
                pQueryExistDC.Parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

                if (m_IsUseDcCategory)
                {
                    pQueryExistDC.Parameters["CATEGORY"].Value = pCategory;
                    m_lastDC_Category = pCategory;
                    if (m_IsUseDcExerciseStyle)
                    {
                        if (pCategory == "O")
                        {
                            pQueryExistDC.Parameters["EXERCISESTYLE"].Value = pExerciseStyle;
                        }
                        else
                        {
                            pQueryExistDC.Parameters["EXERCISESTYLE"].Value = null;
                        }
                    }
                }
                if (m_IsUseDcOptionalSettltMethod)
                {
                    pQueryExistDC.Parameters["SETTLTMETHOD"].Value = pOptionalSettltMethod;
                }
                if (m_IsUseDcContractMultiplier)
                {
                    pQueryExistDC.Parameters["CONTRACTMULTIPLIER"].Value = pContractMultiplier;
                }
                //
                object obj_Dc_Trade = DataHelper.ExecuteScalar(pCS, CommandType.Text, pQueryExistDC.Query, pQueryExistDC.Parameters.GetArrayDbParameter());
                if (obj_Dc_Trade != null)
                {
                    idDC = Convert.ToInt32(obj_Dc_Trade);
                }
            }
            return idDC;
        }
        #endregion Methods int? GetIdDC...

        #region Methods int? GetIdAsset...
        /// <summary>
        /// Lecture de l'IDASSET d'un Asset Futures s'il a existé au moins un Trade sur cet asset, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pMaturity">Echéance de l'asset</param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <param name="pIsCheckCascading">Indique s'il faut vérifier les Assets issus d'un cascading</param>
        /// <returns>IDASSET ou null</returns>
        protected static int? GetIdAssetFutureInTrade(string pCS, DateTime pDtFile, string pExchangeAcronym, string pContractSymbol,
            string pMaturity, AssetFindMaturityEnum pMaturityType, bool pIsCheckCascading)
        {
            int? idAsset = null;

            if (StrFunc.IsFilled(pExchangeAcronym) && StrFunc.IsFilled(pContractSymbol))
            {
                idAsset = GetIdAssetFuture(pCS, QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(pCS, pMaturityType), pDtFile, pExchangeAcronym, pContractSymbol, pMaturity, pMaturityType);
                if (pIsCheckCascading && (idAsset == null))
                {
                    // Utilisation AssetFindMaturityEnum.MATURITYMONTHYEAR pour compatibilité descendante
                    // car les marchés où il y a du cascading utilisent des noms d'échéances et pas des dates d'échéances
                    idAsset = GetIdAssetFuture(pCS, QueryExistAssetFutureFromCascading(pCS), pDtFile, pExchangeAcronym, pContractSymbol, pMaturity, AssetFindMaturityEnum.MATURITYMONTHYEAR);
                }
            }
            return idAsset;
        }
        /// <summary>
        /// Lecture de l'IDASSET d'un Asset Futures un fonction de la requête d'existance reçu en paramètre, sinon return null.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pQueryExistAssetFuture">Requête de vérification d'existance d'un Asset Futures</param>
        /// <param name="pDtFile">Date du fichier permettant de ne pas contrôler les assets précédemment échues</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym</param>
        /// <param name="pContractSymbol">Symbol du DC</param>
        /// <param name="pMaturity">Echéance de l'asset</param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <returns>IDASSET ou null</returns>
        private static int? GetIdAssetFuture(string pCS, QueryParameters pQueryExistAssetFuture, DateTime pDtFile,
            string pExchangeAcronym, string pContractSymbol, string pMaturity, AssetFindMaturityEnum pMaturityType)
        {
            int? idAsset = null;

            if (pQueryExistAssetFuture != default(QueryParameters))
            {
                QueryParameters queryParameters = pQueryExistAssetFuture;
                DataParameters dp = queryParameters.Parameters;
                dp["DTFILE"].Value = pDtFile;
                dp["EXCHANGEACRONYM"].Value = pExchangeAcronym;
                dp["CONTRACTSYMBOL"].Value = pContractSymbol;
                dp["CATEGORY"].Value = "F";
                if (pMaturityType == AssetFindMaturityEnum.MATURITYDATE)
                {
                    dp["MATURITYDATE"].Value = new DtFunc().StringyyyyMMddToDateTime(pMaturity);
                }
                else
                {
                    dp["MATURITYMONTHYEAR"].Value = pMaturity;
                }

                object idAssetFuture = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                if (idAssetFuture != null)
                {
                    idAsset = Convert.ToInt32(idAssetFuture);
                }
            }
            return idAsset;
        }
        #endregion Methods int? GetIdAsset...

        // EG 20141126 New
        // EG 20180426 Analyse du code Correction [CA2202]
        protected void LoadDcSubjectToCA(string pCS, Cst.UnderlyingAsset pAssetCategory, DateTime pDtFile)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE), pDtFile);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ASSETCATEGORY), pAssetCategory);

            string query = @"select ast.IDASSET, ast.ASSETCATEGORY, isnull(mk.EXCHANGEACRONYM,'*') as EXCHANGEACRONYM, dc.CONTRACTSYMBOL
            from VW_ASSET ast
            inner join dbo.MARKET mk on (mk.IDM  = ast.IDM)
            inner join DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = ast.IDASSET) and (dc.ASSETCATEGORY = ast.ASSETCATEGORY)
            inner join CORPOACTION ca on (ca.IDM = ast.IDM)
            inner join CORPOEVENT ce on (ce.IDCA = ca.IDCA)  and (ce.EXDATE > @DTFILE)
            where (ast.ASSETCATEGORY = @ASSETCATEGORY) and
            ((dc.DTENABLED<=@DTFILE and isnull(dc.DTDISABLED,dateadd(day,1,@DTFILE))>@DTFILE)) and 
            ((mk.DTENABLED<=@DTFILE and isnull(mk.DTDISABLED,dateadd(day,1,@DTFILE))>@DTFILE))" + Cst.CrLf;

            query += "and (" + DataHelper.GetSQLXQuery_ExtractValue(pCS, "ADJPROCEDURES", "int", "ce", "(//@spheresid)",
                @"declare namespace efs=""http://www.efs.org/2007/EFSmL-3-0"";") + " = ast.IDASSET)" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, query.ToString(), dataParameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    string _contractSymbol = dr["CONTRACTSYMBOL"].ToString();
                    string _exchangeAcronym = dr["EXCHANGEACRONYM"].ToString();
                    int _idAsset = Convert.ToInt32(dr["IDASSET"]);
                    Cst.UnderlyingAsset _underlyingAsset = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), dr["ASSETCATEGORY"].ToString());

                    if (null == m_LstContractSubjectToCA)
                        m_LstContractSubjectToCA = new Dictionary<Pair<string, string>, Pair<int, Cst.UnderlyingAsset>>();

                    Pair<string, string> _key = new Pair<string, string>(_exchangeAcronym, _contractSymbol);
                    if (false == m_LstContractSubjectToCA.ContainsKey(_key))
                        m_LstContractSubjectToCA.Add(_key, new Pair<int, Cst.UnderlyingAsset>(_idAsset, _underlyingAsset));
                }
            }
        }

        #region Methods static QueryParameters QueryExist...
        /// <summary>
        ///  Requête SQL qui retourne les propriétés IDDC, ISAUTOSETTING et ISAUTOSETTINGASSET d'un DC
        /// <para>Les paramètres de la requête sont EXCHANGESYMBOL, CONTRACTSYMBOL, CATEGORY (optionnel),  CONTRACTATTRIBUTE (optionnel)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseContractAttrib"></param>
        /// <param name="pIsUseISO10383_ALPHA4">Si à True, utilisation de ISO10383_ALPHA4 pour la recherche du marché, sinon utilisation de EXCHANGESYMBOL</param>        
        /// <returns></returns>
        // EG 20140514 [19926]
        // RD 20171130 [23504] Gestion de ISO10383_ALPHA4
        private static QueryParameters QueryExistDC(string pCS, bool pIsUseDcCategory, bool pIsUseContractAttrib, bool pIsUseISO10383_ALPHA4)
        {
            StrBuilder sql = new StrBuilder();

            sql += SQLCst.SELECT + "dc.IDDC, dc.ISAUTOSETTING, dc.ISAUTOSETTINGASSET" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " k" + SQLCst.ON + "(k.IDM=dc.IDM)";
            // RD 20171130 [23504] Utiliser soit ISO10383_ALPHA4 soit EXCHANGESYMBOL
            //sql += SQLCst.AND + "(k.EXCHANGESYMBOL = @EXCHANGESYMBOL)" + Cst.CrLf;
            if (pIsUseISO10383_ALPHA4)
                sql += SQLCst.AND + "(k.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            else
                sql += SQLCst.AND + "(k.EXCHANGESYMBOL = @EXCHANGESYMBOL)" + Cst.CrLf;

            sql += SQLCst.WHERE + "(dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)";
            if (pIsUseDcCategory)
                sql += SQLCst.AND + "(dc.CATEGORY = @CATEGORY)";
            if (pIsUseContractAttrib)
                sql += SQLCst.AND + "(dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)";

            // EG 20140514 [19926] La date RDBMS est utilisée pour la gestion ENABLED/DISABLED
            sql += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc") + ")" + Cst.CrLf;


            DataParameters parameters = new DataParameters();
            // RD 20171130 [23504] Utiliser soit ISO10383_ALPHA4 soit EXCHANGESYMBOL
            //parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGESYMBOL));
            if (pIsUseISO10383_ALPHA4)
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4));
            else
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGESYMBOL));

            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTSYMBOL));

            if (pIsUseDcCategory)
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CATEGORY));
            if (pIsUseContractAttrib)
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), parameters);
            return qryParameters;
        }

        /// <summary>
        /// Requête SQL qui retourne les propriétés IDASSET
        /// <para>Les paramètres de la requête sont EXCHANGESYMBOL, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR, PUTCALL (option),STRIKEPRICE (option) </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCategory"></param>
        /// <param name="pIsUseContractAttrib"></param>
        /// <param name="pIsUseISO10383_ALPHA4">Si à True, utilisation de ISO10383_ALPHA4 pour la recherche du marché, sinon utilisation de EXCHANGESYMBOL</param>        
        /// <returns></returns>
        // EG 20140514 [19926]
        // RD 20171130 [23504] Gestion de ISO10383_ALPHA4
        private static QueryParameters QueryExistAsset(string pCS, string pCategory, bool pIsUseContractAttrib, bool pIsUseISO10383_ALPHA4)
        {
            if (pCategory != "O" & pCategory != "F")
                throw new ArgumentException(StrFunc.AppendFormat("Category {0} is not valid", pCategory));

            // EG 20140514 [19926] La date RDBMS est utilisée pour la gestion ENABLED/DISABLED
            // FI 20220524 [XXXXX] Suppression de la jointure sur MATURITYRULE puisque inutile
            StrBuilder sql = new StrBuilder();
            sql += $@"select a.IDASSET 
from dbo.ASSET_ETD a
inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)
and ({OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc")})
and ({OTCmlHelper.GetSQLDataDtEnabled(pCS, "a")})" + Cst.CrLf;
            
            
            // RD 20171130 [23504] Utiliser soit ISO10383_ALPHA4 soit EXCHANGESYMBOL
            if (pIsUseISO10383_ALPHA4)
                sql += " and (mk.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            else
                sql += " and (mk.EXCHANGESYMBOL = @EXCHANGESYMBOL)" + Cst.CrLf;

            sql += " and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)" + Cst.CrLf;
            sql += " and (dc.CATEGORY = @CATEGORY)";
            if (pIsUseContractAttrib)
                sql += " and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)";

            if (pCategory == "O")
            {
                sql += sql + Cst.CrLf;
                sql += " and (a.PUTCALL = @PUTCALL)";
                sql += " and (a.STRIKEPRICE = @STRIKEPRICE)";
            }

            DataParameters dataParameters = new DataParameters();

            // RD 20171130 [23504] Utiliser soit ISO10383_ALPHA4 soit EXCHANGESYMBOL
            if (pIsUseISO10383_ALPHA4)
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4));
            else
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGESYMBOL));

            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            // EG 20140514 [19926] La date RDBMS est utilisée pour la gestion ENABLED/DISABLED
            //dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CATEGORY));
            if (pIsUseContractAttrib)
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));

            if (pCategory == "O")
            {
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.STRIKEPRICE));
                dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PUTCALL));
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dataParameters);

            return qryParameters;

        }

        /// <summary>
        ///  Retourne la requête qui permet vérifier si un trade a déjà été négocié sur le marché
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        /// PM 20150807 New
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistExchangeInTrades(string pCs)
        {
            string query = @"select distinct m.IDM
            from dbo.MARKET m
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = m.IDM)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET) 
            where (m.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)" + Cst.CrLf;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        ///  Retourne la requête qui permet vérifier si un trade a déjà été négocié sur les marchés d'une chambre
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        /// PM 20151218 New
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistClearingInTrades(string pCs)
        {
            string query = @"select distinct cs.IDA
            from dbo.CSS cs
            inner join dbo.MARKET m_exh on (m_exh.IDA = m_exh.IDA)
            inner join dbo.MARKET m on (m.IDA = cs.IDA)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = m.IDM)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            where (m_exh.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)" + Cst.CrLf;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet de vérifier que le DC est négocié ou que le DC est un sous jacent d'un DC négocié (Contrat option sur future)
        /// <para>Les paramètres obligatoires de la requête sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL</para>
        /// <para>Les paramètres optionels sont CATEGORY, SETTLTMETHOD, EXERCISESTYLE, CONTRACTMULTIPLIER</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseDcOptionalSettltMethod"></param>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <returns></returns>
        /// PM 20171109 [22848][23561] Ajout contrat Future sous-jacent d'un contrat Option sur Future négocié
        /// RD 20171201 [23504] Gestion de ISO10383_ALPHA4
        /// PM 20171212 [23646] Passage de method private à protected
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected static QueryParameters QueryExistDCInTrades(string pCs, bool pIsUseDcCategory, bool pIsUseDcOptionalSettltMethod, bool pIsUseDcExerciseStyle, bool pIsUseDcContractMultiplier, bool pIsUseISO10383_ALPHA4)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, pIsUseISO10383_ALPHA4 ? DataParameter.ParameterEnum.ISO10383_ALPHA4 : DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

            // PM 20171109 [22848][23561] Plus besoin du DISTINCT de fait de l'ajout de UNION plus bas
            string query = @"select dc.IDDC
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            where (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE) and ({0}) and ({1})" + Cst.CrLf;

            // RD 20171201 [23504] Add pIsUseISO10383_ALPHA4
            if (pIsUseISO10383_ALPHA4)
                query += " and (mk.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            else
                query += " and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)" + Cst.CrLf;

            if (pIsUseDcCategory)
            {
                query += " and (dc.CATEGORY = @CATEGORY)" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
                if (pIsUseDcExerciseStyle)
                {
                    query += " and ((dc.EXERCISESTYLE = @EXERCISESTYLE) or ((@CATEGORY != 'O') and (@EXERCISESTYLE is null)))" + Cst.CrLf;
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXERCISESTYLE));
                }
            }

            if (pIsUseDcOptionalSettltMethod)
            {
                query += " and ((dc.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }

            if (pIsUseDcContractMultiplier)
            {
                query += " and ((dc.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER) or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER));
            }

            query += @"union 

            select dc_unl.IDDC
            from dbo.DERIVATIVECONTRACT dc_unl
            inner join dbo.MARKET mk_unl on (mk_unl.IDM = dc_unl.IDM)
            inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDC = dc_unl.IDDC)
            inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
            inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDDERIVATIVEATTRIB = da_unl.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDASSET = asset_unl.IDASSET)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            where (dc_unl.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (ma_unl.DELIVERYDATE is null or ma_unl.DELIVERYDATE >= @DTFILE) and (dc_unl.CATEGORY = 'F') and ({2}) and ({3})" + Cst.CrLf;

            if (pIsUseISO10383_ALPHA4)
                query += @" and (mk_unl.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            else
                query += @" and (mk_unl.EXCHANGEACRONYM = @EXCHANGEACRONYM)" + Cst.CrLf;

            if (pIsUseDcCategory)
                query += @" and (dc_unl.CATEGORY = @CATEGORY)" + Cst.CrLf;

            if (pIsUseDcOptionalSettltMethod)
                query += @" and ((dc_unl.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;

            if (pIsUseDcContractMultiplier)
                query += @" and ((dc_unl.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER) or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;

            query = String.Format(query,
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_unl", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_unl", "DTFILE")) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCs, query, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet vérifier qu'un DC est déjà négocié sur une maturité particulière
        /// <para>La requête contient possède la colonne IDDC</para>
        /// <para>Les paramètres de la requête sont DTFILE,EXCHANGEACRONYM, CONTRACTSYMBOL, MATURITYMONTHYEAR, CATEGORY (optionnel)</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <returns></returns>
        /// TODO : Utiliser ISO10383_ALPHA4 à la place de EXCHANGEACRONYM ! 
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistDCMaturityInTrades(string pCs, bool pIsUseDcCategory, AssetFindMaturityEnum pMaturityType)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

            string query = @"select dc.IDDC
            from dbo.TRADE tr
            inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)" + Cst.CrLf;

            // Jointure avec MATURITYRULE lors de l'utilisation de noms d'échéances
            if (pMaturityType == AssetFindMaturityEnum.MATURITYMONTHYEAR)
                query += @"inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ma.IDMATURITYRULE)" + Cst.CrLf;

            query += @"inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            where (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE) and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)" + Cst.CrLf;

            // Gestion de pMaturityType
            switch (pMaturityType)
            {
                case AssetFindMaturityEnum.MATURITYMONTHYEAR:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                    query += @"and (((mr.MMYFMT = 0) and (ma.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 ))) or (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR))" + Cst.CrLf;
                    break;
                case AssetFindMaturityEnum.MATURITYDATE:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYDATE));
                    query += @"and isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) = @MATURITYDATE" + Cst.CrLf;
                    break;
            }

            if (pIsUseDcCategory)
            {
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
                query += @"and (dc.CATEGORY = @CATEGORY)";
            }
            query += String.Format(@"and ({0}) and ({1})", OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne  la requête qui permet vérifier qu'un asset est déjà négocié sur une maturité particulière (la maturité est représenté par une date)  
        /// <para>Les paramètres de la requête sont EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYDATE ou MATURITYMONTHYEAR, PUTCALL (option),STRIKEPRICE (option)</para>
        /// </summary>
        /// <param name="pCategory">"O" pour option, "F" pour future</param>
        /// <param name="pIsUseContractAttrib"></param>
        /// <param name="pAssetFindMaturity">MATURITYDATE ou MATURITYMONTHYEAR</param>
        /// <returns></returns>
        /// FI 20131121 [19224] add Parameter pFindMaturityType
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistAssetMaturityDateInTrades(string pCs, string pCategory, bool pIsUseContractAttrib, AssetFindMaturityEnum pFindMaturityType)
        {
            if (pCategory != "O" & pCategory != "F")
                throw new ArgumentException(StrFunc.AppendFormat("Category {0} is not valid", pCategory));

            /* Paramètres */
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ISO10383_ALPHA4));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
            
            // FI 20220524 [XXXXX] Suppression de la jointure sur MATURITYRULE puisque inutile
            string query = $@"select a.IDASSET
from dbo.ASSET_ETD a
inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
inner join dbo.TRADE tr on (tr.IDASSET = a.IDASSET)
where (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE) 
and (mk.ISO10383_ALPHA4 = @ISO10383_ALPHA4) 
and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CATEGORY = @CATEGORY) 
and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")}) and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})" + Cst.CrLf;

            if (pIsUseContractAttrib)
            {
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
                query += @" and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)" + Cst.CrLf;
            }

            switch (pFindMaturityType)
            {
                case AssetFindMaturityEnum.MATURITYDATE:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYDATE));
                    query += @" and (isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) = @MATURITYDATE)" + Cst.CrLf;
                    break;

                case AssetFindMaturityEnum.MATURITYMONTHYEAR:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                    query += @" and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)" + Cst.CrLf;
                    break;

                default:
                    throw new Exception(StrFunc.AppendFormat("AssetFindMaturityEnum : {0} is not implemented", pFindMaturityType));
            }

            if (pCategory == "O")
            {
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.PUTCALL));
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.STRIKEPRICE));
                query += @" and (a.PUTCALL = @PUTCALL) and (a.STRIKEPRICE = (@STRIKEPRICE / power(10, isnull( dc.STRIKEDECLOCATOR, 0 ) ) ))" + Cst.CrLf;
            }

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset Future a déjà été négocié ou qu'il est Sous-Jacent d'un asset Option déjà négocié.
        /// <para>Les paramètres obligatoires sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR|MATURITYDATE</para>
        /// <para>Les paramètres optionnels sont SETTLTMETHOD, CONTRACTMULTIPLIER, CONTRACTATTRIBUTE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <returns></returns>
        /// PL 20220907 New signature (Rename)
        protected static QueryParameters QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(string pCs, AssetFindMaturityEnum pMaturityType)
        {
            return QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(pCs, pMaturityType, false, false, false, false);
        }
        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset Future a déjà été négocié ou qu'il est Sous-Jacent d'un asset Option déjà négocié.
        /// <para>Les paramètres obligatoires sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR|MATURITYDATE</para>
        /// <para>Les paramètres optionnels sont SETTLTMETHOD, CONTRACTMULTIPLIER, CONTRACTATTRIBUTE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <param name="pIsUseDcSettltMethod"></param>
        /// <param name="pIsUseDcContractMultiplier"></param>
        /// <param name="pIsUseDcContractAttrib"></param>
        /// <returns></returns>
        /// PL 20220907 New signature (Rename)
        protected static QueryParameters QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(string pCs, AssetFindMaturityEnum pMaturityType,
                                         bool pIsUseDcSettltMethod, bool pIsUseDcContractMultiplier, bool pIsUseDcContractAttrib)
        {
            return QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(pCs, pMaturityType, pIsUseDcSettltMethod, pIsUseDcContractMultiplier, pIsUseDcContractAttrib, false);
        }
        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset Future a déjà été négocié ou qu'il est Sous-Jacent d'un asset Option déjà négocié.
        /// <para>Les paramètres obligatoires sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR|MATURITYDATE</para>
        /// <para>Les paramètres optionnels sont SETTLTMETHOD, CONTRACTMULTIPLIER, CONTRACTATTRIBUTE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <param name="pIsUseDcSettltMethod"></param>
        /// <param name="pIsUseDcContractMultiplier"></param>
        /// <param name="pIsUseDcContractAttrib"></param>
        /// <param name="pIsIgnoreExerciseStyle"></param>
        /// <returns></returns>
        /// TODO : Utiliser ISO10383_ALPHA4 à la place de EXCHANGEACRONYM !
        /// FI 20131024 [17861] Add pIsUseContractAttrib
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        /// PL 20220907 New override (Add pIsIgnoreExerciseStyle for MEFF/BMECLEARING)
        protected static QueryParameters QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(string pCs, AssetFindMaturityEnum pMaturityType, 
                                         bool pIsUseDcSettltMethod, bool pIsUseDcContractMultiplier, bool pIsUseDcContractAttrib, bool pIsIgnoreExerciseStyle)
        {
            // Parameters
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));

            // Query
            // Asset Future négocié
            string query = @"select asset.IDASSET, dc.IDASSET_UNL, dc.ASSETCATEGORY
            from dbo.ASSET_ETD asset
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)" + Cst.CrLf;

            // Jointure avec MATURITYRULE dans le cas d'utilisation de noms d'échéances
            if (pMaturityType == AssetFindMaturityEnum.MATURITYMONTHYEAR)
                query += @"inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ma.IDMATURITYRULE)" + Cst.CrLf;

            query += @"where (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CATEGORY = @CATEGORY) and 
            (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE) and ({0}) and ({1})" + Cst.CrLf;

            // Gestion pMaturityType
            switch (pMaturityType)
            {
                case AssetFindMaturityEnum.MATURITYMONTHYEAR:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                    query += @" and (((mr.MMYFMT = 0) and (ma.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 ))) or (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR))" + Cst.CrLf;
                    break;
                case AssetFindMaturityEnum.MATURITYDATE:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYDATE));
                    query += @" and (isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) = @MATURITYDATE)" + Cst.CrLf;
                    break;
            }

            if (pIsUseDcSettltMethod)
            {
                query += @" and ((dc.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }
            if (pIsUseDcContractMultiplier)
            {
                query += @" and (((case when asset.CONTRACTMULTIPLIER is null then ( 
                case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end ) else asset.CONTRACTMULTIPLIER end) = @CONTRACTMULTIPLIER)
                or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER));
            }
            if (pIsUseDcContractAttrib)
            {
                query += @" and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)";
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
            }

            // Asset Future sous-jacent d'un Asset Option sur Future négocié
            query += @"union
            select da.IDASSET, dc_unl.IDASSET_UNL, dc_unl.ASSETCATEGORY
            from dbo.DERIVATIVEATTRIB da
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDASSET = da.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDERIVATIVEATTRIB = asset_unl.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc_unl on (dc_unl.IDDC = da_unl.IDDC)
            inner join dbo.MARKET mk_unl on (mk_unl.IDM = dc.IDM)" + Cst.CrLf;

            // Jointure avec MATURITYRULE dans le cas d'utilisation de noms d'échéances
            if (pMaturityType == AssetFindMaturityEnum.MATURITYMONTHYEAR)
                query += @"inner join dbo.MATURITYRULE mr_unl on (mr_unl.IDMATURITYRULE = ma_unl.IDMATURITYRULE)" + Cst.CrLf;

            query += @"where (mk_unl.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (dc_unl.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc_unl.CATEGORY = @CATEGORY) 
            and (ma_unl.DELIVERYDATE is null or ma_unl.DELIVERYDATE >= @DTFILE) and (dc.ASSETCATEGORY = 'Future')" + Cst.CrLf;

            #region ExerciseStyle
            // PL 20220907 Implémentation de la possibilité d'ignorer le style AMERICAN/EUROPEAN de l'Option (Utile aux RiskData du MEFF/BMECLEARING)
            if (pIsIgnoreExerciseStyle)
            {
                query += SQLCst.SQL_ANSI_COMMENT_BEGIN + Cst.CrLf;
                query += @"No restriction to American Options or European Options at maturity" + Cst.CrLf;
            }
            query += @"and ((dc.EXERCISESTYLE='1') or (ma.MATURITYDATE = @DTFILE))" + Cst.CrLf;
            if (pIsIgnoreExerciseStyle)
            {
                query += SQLCst.SQL_ANSI_COMMENT_END + Cst.CrLf;
            }
            #endregion ExerciseStyle

            query += @"and({2}) and({3}) and({4})" + Cst.CrLf;

            // Gestion pMaturityType
            switch (pMaturityType)
            {
                case AssetFindMaturityEnum.MATURITYMONTHYEAR:
                    query += @" and (((mr_unl.MMYFMT = 0) and (ma_unl.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 ))) or (ma_unl.MATURITYMONTHYEAR = @MATURITYMONTHYEAR))" + Cst.CrLf;
                    break;
                case AssetFindMaturityEnum.MATURITYDATE:
                    query += @" and (isnull(ma_unl.MATURITYDATESYS, ma_unl.MATURITYDATE) = @MATURITYDATE)" + Cst.CrLf;
                    break;
            }

            if (pIsUseDcSettltMethod)
                query += @" and ((dc_unl.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;

            if (pIsUseDcContractMultiplier)
                query += @" and ((dc_unl.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER) or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;

            if (pIsUseDcContractAttrib)
                query += @" and (dc_unl.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)" + Cst.CrLf;

            query = String.Format(query, 
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_unl", "DTFILE"),
                OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_unl", "DTFILE")) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset Option est déjà négocié 
        /// <para>Les paramètres de la requête sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR, PUTCALL, STRIKEPRICE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <returns></returns>
        /// TODO : Utiliser ISO10383_ALPHA4 à la place de EXCHANGEACRONYM ! 
        protected static QueryParameters QueryExistAssetOptionInTrades(string pCs, AssetFindMaturityEnum pMaturityType)
        {
            return QueryExistAssetOptionInTrades(pCs, pMaturityType, false, false, false, false);
        }

        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset "Option" a déjà été négocié. 
        /// <para>Les paramètres de la requête sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR|MATURITYDATE, PUTCALL, STRIKEPRICE</para>
        /// <para>et optionnellement SETTLTMETHOD, EXERCISESTYLE, CONTRACTMULTIPLIER, CONTRACTATTRIBUTE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMaturityType">Type de l'échéance: nom ou date</param>
        /// <param name="pIsUseDcSettltMethod"></param>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <param name="pIsUseDcContractMultiplier"></param>
        /// <param name="pIsUseDcContractAttrib"></param>
        /// <returns></returns>
        /// FI 20131023 [17861] GLOP il manque la notion de version de contract
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected static QueryParameters QueryExistAssetOptionInTrades(string pCs, AssetFindMaturityEnum pMaturityType, 
            bool pIsUseDcSettltMethod, bool pIsUseDcExerciseStyle, bool pIsUseDcContractMultiplier, bool pIsUseDcContractAttrib)
        {
            // Parameters
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.PUTCALL));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.STRIKEPRICE));

            // Query
            string query = @"select asset.IDASSET, dc.IDASSET_UNL, dc.ASSETCATEGORY
            from dbo.TRADE tr
            inner join dbo.ASSET_ETD asset on (asset.IDASSET = tr.IDASSET) and (asset.PUTCALL = @PUTCALL) and (asset.STRIKEPRICE = @STRIKEPRICE)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.DELIVERYDATE is null or ma.DELIVERYDATE >= @DTFILE)" + Cst.CrLf;
            // Gestion pMaturityType
            switch (pMaturityType)
            {
                case AssetFindMaturityEnum.MATURITYMONTHYEAR:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
                    query += @"inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ma.IDMATURITYRULE) and 
                    (((mr.MMYFMT = 0) and (ma.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 ))) or (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR))" + Cst.CrLf;
                    break;
                case AssetFindMaturityEnum.MATURITYDATE:
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYDATE));
                    query += SQLCst.AND + DataHelper.SQLIsNull(pCs, "ma.MATURITYDATESYS", "ma.MATURITYDATE") + " = @MATURITYDATE" + Cst.CrLf;
                    break;
            }

            query += @"inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CATEGORY = @CATEGORY)" + Cst.CrLf;

            if (pIsUseDcExerciseStyle)
            {
                query += @" and ((dc.EXERCISESTYLE = @EXERCISESTYLE) or ((@CATEGORY != 'O') and (@EXERCISESTYLE is null)))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXERCISESTYLE));
            }
            if (pIsUseDcSettltMethod)
            {
                query += @" and ((dc.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }
            if (pIsUseDcContractMultiplier)
            {
                query += @" and (((case when asset.CONTRACTMULTIPLIER is null then ( 
                case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end ) else asset.CONTRACTMULTIPLIER end) = @CONTRACTMULTIPLIER)
                or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER));
            }
            if (pIsUseDcContractAttrib)
            {
                query += @" and (dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
            }

            query += @"inner join dbo.MARKET mk on (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM) and (mk.IDM = dc.IDM)
            where ({0}) and ({1})" + Cst.CrLf;


            query = String.Format(query, OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet d'obtenir le sous-jacent (Equity, Index, DC,...) d'un DC déjà négocié 
        /// <para>Les paramètres de la requête sont DTFILE, EXCHANGEACRONYM, SYMBOL</para>
        /// <para>et optionnellement CATEGORY, SETTLTMETHOD, EXERCISESTYLE</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pAssetCategory">Categorie de l'asset sous-jacent</param>
        /// <param name="pIsUseDcCategory">true si usage du paramètre CATEGORY</param>
        /// <param name="pIsUseDcOptionalSettltMethod">true si usage du paramètre SETTLTMETHOD</param>
        /// <param name="pIsUseDcExerciseStyle">true si usage du paramètre EXERCISESTYLE</param>
        /// <returns></returns>
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        /// FI 20211115 [XXXXX] Refactoring afin de gérer Cst.UnderlyingAsset.Future
        private static QueryParameters QueryExistDCUnderlyingInTrades(string pCs, Cst.UnderlyingAsset pAssetCategory, bool pIsUseDcCategory, bool pIsUseDcOptionalSettltMethod, bool pIsUseDcExerciseStyle)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SYMBOL));

            Cst.OTCml_TBL tableUnl;
            string colId;
            string joinDC;
            switch (pAssetCategory)
            {
                case Cst.UnderlyingAsset.Future:
                    tableUnl = Cst.OTCml_TBL.DERIVATIVECONTRACT;
                    colId = "unl.IDDC";
                    joinDC = $"inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC_UNL = unl.IDDC) and (dc.ASSETCATEGORY = '{pAssetCategory}')";
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedContract:
                    throw new NotSupportedException($"{pAssetCategory} is not supported.");

                default:
                    tableUnl = AssetTools.ConvertUnderlyingAssetToTBL(pAssetCategory);
                    colId = "unl.IDASSET";
                    joinDC = $"inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = unl.IDASSET) and (dc.ASSETCATEGORY = '{pAssetCategory}')";
                    break;
            }

            string query = $@"select {colId}
            from dbo.{tableUnl} unl
            {joinDC}
            inner join dbo.MARKET mk on (mk.IDM  = dc.IDM)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)" + Cst.CrLf;

            switch (pAssetCategory)
            {
                case Cst.UnderlyingAsset.Future:
                    query += @"where (unl.CONTRACTSYMBOL = @SYMBOL)" + Cst.CrLf;
                    break;
                case Cst.UnderlyingAsset.EquityAsset:
                    // PM 20131224 [19404] Ajout traitement particulier pour sous-jacent Equity : jointure avec ASSET_EQUITY_RDCMK
                    query += @"inner join dbo.ASSET_EQUITY_RDCMK aeqrdc on (aeqrdc.IDASSET = unl.IDASSET)" + Cst.CrLf;
                    query += @"where (aeqrdc.SYMBOL = @SYMBOL) and (aeqrdc.IDM_RELATED = mk.IDM)" + Cst.CrLf;
                    break;
                default:
                    query += @"where (unl.SYMBOL = @SYMBOL)" + Cst.CrLf;
                    break;
            }

            query += $@" and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM) and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE)) and
            ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "unl", "DTFILE")}) and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")}) and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})" + Cst.CrLf;

            if (pIsUseDcCategory)
            {
                query += @" and (dc.CATEGORY = @CATEGORY)" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
                if (pIsUseDcExerciseStyle)
                {
                    query += @" and ((dc.EXERCISESTYLE = @EXERCISESTYLE) or ((@CATEGORY != 'O') and (@EXERCISESTYLE is null)))" + Cst.CrLf;
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXERCISESTYLE));
                }
            }

            if (pIsUseDcOptionalSettltMethod)
            {
                query += @" and (dc.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet vérifier que le DC peut être issu du cascading d'un DC qui a déjà été négocié
        /// <para>Les paramètres obligatoires de la requête sont DTFILE, EXCHANGEACRONYM, CONTRACTSYMBOL</para>
        /// <para>Les paramètres optionels sont CATEGORY, SETTLTMETHOD, EXERCISESTYLE, CONTRACTMULTIPLIER</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseDcOptionalSettltMethod"></param>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <returns></returns>
        /// PM 20150911 [21336] New
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistDCFromCascading(string pCs, bool pIsUseDcCategory, bool pIsUseDcOptionalSettltMethod, bool pIsUseDcExerciseStyle, bool pIsUseDcContractMultiplier)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

            string query = @"select distinct dc_casc.IDDC
            from dbo.CONTRACTCASCADING cc
            inner join dbo.DERIVATIVECONTRACT dc_casc on (dc_casc.IDDC  = cc.IDDC_CASC)
            inner join dbo.MARKET mk_casc on (mk_casc.IDM  = dc_casc.IDM)
            inner join dbo.DERIVATIVEATTRIB da_casc on (da_casc.IDDC = dc_casc.IDDC)
            inner join dbo.MATURITY ma_casc on (ma_casc.IDMATURITY = da_casc.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC  = cc.IDDC)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            where (dc_casc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (mk_casc.EXCHANGEACRONYM = @EXCHANGEACRONYM) and
            (ma_casc.DELIVERYDATE is null or ma_casc.DELIVERYDATE >= @DTFILE) and 
            ((isnull(ma.MATURITYDATE,ma.MATURITYDATESYS) is null) or (isnull(ma.MATURITYDATE,ma.MATURITYDATESYS) = @DTFILE)) and ({0}) and ({1})" + Cst.CrLf;

            if (pIsUseDcCategory)
            {
                query += @" and (dc_casc.CATEGORY = @CATEGORY)" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
                if (pIsUseDcExerciseStyle)
                {
                    query += @" and ((dc_casc.EXERCISESTYLE = @EXERCISESTYLE) or ((@CATEGORY != 'O') and (@EXERCISESTYLE is null)))" + Cst.CrLf;
                    dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXERCISESTYLE));
                }
            }
            if (pIsUseDcOptionalSettltMethod)
            {
                query += @" and ((dc_casc.SETTLTMETHOD = @SETTLTMETHOD) or (@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }
            if (pIsUseDcContractMultiplier)
            {
                query += @" and ((dc_casc.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER) or (@CONTRACTMULTIPLIER is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER));
            }

            query = String.Format(query, OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_casc", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_casc", "DTFILE")) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Retourne la requête qui permet vérifier qu'un Asset peut être issu du cascading d'un Asset qui a déjà été négocié
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        /// PM 20150911 [21336] New
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static QueryParameters QueryExistAssetFutureFromCascading(string pCs)
        {
            // Parameters
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EXCHANGEACRONYM));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.CATEGORY));
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));

            // Query
            // Asset Future négocié
            string query = @"select distinct asset_casc.IDASSET, dc_casc.IDASSET_UNL, dc_casc.ASSETCATEGORY
            from dbo.CONTRACTCASCADING cc
            inner join dbo.DERIVATIVECONTRACT dc_casc on (dc_casc.IDDC  = cc.IDDC_CASC)
            inner join dbo.MARKET mk_casc on (mk_casc.IDM  = dc_casc.IDM)
            inner join dbo.DERIVATIVEATTRIB da_casc on (da_casc.IDDC = dc_casc.IDDC)
            inner join dbo.MATURITY ma_casc on (ma_casc.IDMATURITY = da_casc.IDMATURITY)
            inner join dbo.ASSET_ETD asset_casc on (asset_casc.IDDERIVATIVEATTRIB = da_casc.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC  = cc.IDDC)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.TRADE tr on (tr.IDASSET = asset.IDASSET)
            where (dc_casc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (mk_casc.EXCHANGEACRONYM = @EXCHANGEACRONYM) and 
            (ma_casc.DELIVERYDATE is null or ma_casc.DELIVERYDATE >= @DTFILE) and 
            ((isnull(ma.MATURITYDATE,ma.MATURITYDATESYS) is null) or (isnull(ma.MATURITYDATE,ma.MATURITYDATESYS) = @DTFILE)) and 
            (ma_casc.MATURITYMONTHYEAR = @MATURITYMONTHYEAR) and 
            ((substring(ma.MATURITYMONTHYEAR,1,4) + isnull(cc.ISYEARINCREMENT,0)*1) = substring(ma_casc.MATURITYMONTHYEAR,1,4)) and
            (substring(ma.MATURITYMONTHYEAR,5,2) = cc.MATURITYMONTH) and 
            (substring(ma_casc.MATURITYMONTHYEAR,5,2) = cc.CASCMATURITYMONTH) and ({0}) and ({1})" + Cst.CrLf;

            query = String.Format(query, OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_casc", "DTFILE"), OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_casc", "DTFILE")) + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(pCs, query, dataParameters);
            return qryParameters;
        }
        #endregion Methods static QueryParameters QueryExist...

        #region Methods protected virtual void SetQueryExist...
        /// <summary>
        /// Affecte la requête de recherche d'un DC
        /// </summary>
        protected virtual void SetQueryExistDC()
        {
            // RD 20171130 [23504] Gestion de m_IsUseISO10383_ALPHA4
            m_QueryExistDC = QueryExistDC(task.Cs, m_IsUseDcCategory, m_IsUseContractAttrib, m_IsUseISO10383_ALPHA4);
        }

        /// <summary>
        /// Affecte la requête de recherche d'une Clearing sur laquel un DC a déjà été négocié
        /// </summary>
        /// PM 20151218 New
        protected virtual void SetQueryExistClearingInTrade()
        {
            m_QueryExistClearingInTrade = QueryExistClearingInTrades(task.Cs);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un Exchange sur lequel un DC a déjà été négocié
        /// </summary>
        /// PM 20150807 New
        protected virtual void SetQueryExistExchangeInTrade()
        {
            m_QueryExistExchangeInTrade = QueryExistExchangeInTrades(task.Cs);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un DC (permet de vérifier que le DC est négocié ou que le DC est un sous jacent d'un DC négocié (Contrat option sur future)) 
        /// </summary>
        protected virtual void SetQueryExistDCInTrade()
        {
            // RD 20171201 [23504] Gestion de m_IsUseISO10383_ALPHA4
            m_QueryExistDCInTrade = QueryExistDCInTrades(task.Cs, m_IsUseDcCategory, m_IsUseDcOptionalSettltMethod, m_IsUseDcExerciseStyle, m_IsUseDcContractMultiplier, m_IsUseISO10383_ALPHA4);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un DC déjà négocié sur une maturité particulière
        /// </summary>
        protected virtual void SetQueryExistDCMaturityInTrades()
        {
            m_QueryExistDCMaturityInTrade = QueryExistDCMaturityInTrades(task.Cs, m_IsUseDcCategory, m_MaturityType);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un Asset Future et d'un asset Option
        /// </summary>
        protected virtual void SetQueryExistAsset()
        {
            // RD 20171130 [23504] Gestion de m_IsUseISO10383_ALPHA4
            m_QueryExistAssetFut = QueryExistAsset(task.Cs, "F", m_IsUseContractAttrib, m_IsUseISO10383_ALPHA4);
            m_QueryExistAssetOpt = QueryExistAsset(task.Cs, "O", m_IsUseContractAttrib, m_IsUseISO10383_ALPHA4);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un Asset déjà négocié sur une maturité particulière
        /// <para>Les paramètres de la requête sont EXCHANGEACRONYM, CONTRACTSYMBOL, CATEGORY, MATURITYDATE ou MATURITYMONTHYEAR, PUTCALL (option),STRIKEPRICE (option)</para>
        /// </summary>
        /// <param name="pCategory">Category</param>
        /// <param name="pAssetFindMaturity">MATURITYDATE ou MATURITYMONTHYEAR</param>
        /// FI 20131121 [19224] add Parameter pFindMaturityType
        protected virtual void SetQueryExistAssetMaturityDateInTrade(string pCategory, AssetFindMaturityEnum pFindMaturityType)
        {
            m_QueryExistAssetMaturityDateInTrade = QueryExistAssetMaturityDateInTrades(Cs, pCategory, m_IsUseContractAttrib, pFindMaturityType);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un sous-jacent d'un DC déjà négocié
        /// </summary>
        /// <param name="pAssetCategory">Categorie de l'asset sous-jacent</param>
        protected virtual void SetQueryExistDCUnderlyingInTrade(Cst.UnderlyingAsset pAssetCategory)
        {
            m_QueryExistDCUnderlyingInTrade = QueryExistDCUnderlyingInTrades(task.Cs, pAssetCategory, m_IsUseDcCategoryForUnderlying, m_IsUseDcOptionalSettltMethod, m_IsUseDcExerciseStyle);
        }

        /// <summary>
        /// Affecte la requête de recherche d'un DC pouvant être issu d'un DC qui a déjà été négocié
        /// </summary>
        /// PM 20150911 [21336] New
        protected virtual void SetQueryExistDCFromCascading()
        {
            m_QueryExistDCFromCascading = QueryExistDCFromCascading(task.Cs, m_IsUseDcCategory, m_IsUseDcOptionalSettltMethod, m_IsUseDcExerciseStyle, m_IsUseDcContractMultiplier);
        }
        #endregion Methods protected virtual void SetQueryExist...

        /// <summary>
        /// Retourne le code ISO du marché en fonction de son acronym au sein de la chambre
        /// </summary>
        /// <returns></returns>
        protected virtual string GetISO183803FromExchangeAcronym(string pExchangeAcronym)
        {
            throw new Exception("Method is not override");
        }

        /// <summary>
        ///  Initialisation nécessaire en cas d'importation directe d'un fichier
        /// </summary>
        /// <param name="pInputParsing"></param>
        /// FI 20131025 [17861] Add Method
        protected void InitDirectImport(InputParsing pInputParsing)
        {
            inputParsing = pInputParsing;
            nbParsingIgnoredLines = 0;
            // FI 20200820 [25468] dates systemes en UTC
            dtStart = OTCmlHelper.GetDateSysUTC(Cs);
        }

        /// <summary>
        /// Alimente les colonnes DTINS et IDAINS du datarow {pRow}
        /// <para>DTINS est alimenté avec dtStart</para>
        /// <para>IDAINS est alimenté avec task.UserId</para>
        /// </summary>
        /// <param name="pRow"></param>
        /// FI 20131108 [17861] Add Method
        protected void SetDataRowIns(DataRow pRow)
        {
            if (DtFunc.IsDateTimeEmpty(dtStart))
                throw new InvalidOperationException("dtStart is empty");

            pRow["DTINS"] = dtStart;
            pRow["IDAINS"] = task.Process.UserId;
        }

        /// <summary>
        /// Alimente les colonnes DTUPD et IDAUPD du datarow {pRow}
        /// <para>DTUPD est alimenté avec dtStart</para>
        /// <para>IDAUPD est alimenté avec task.UserId</para>
        /// </summary>
        /// <param name="pRow"></param>
        /// FI 20131108 [17861] Add Method
        protected void SetDataRowUpd(DataRow pRow)
        {
            if (DtFunc.IsDateTimeEmpty(dtStart))
                throw new InvalidOperationException("dtStart is empty");

            pRow["DTUPD"] = dtStart;
            pRow["IDAUPD"] = task.Process.UserId;
        }

        /// <summary>
        /// Retourne la description définie en tant que attribut sur l'enum Cst.InputSourceDataStyle
        /// </summary>
        /// <returns></returns>
        /// FI 20131108 [17861] Add Method
        protected string GetFileDesciption()
        {
            string ret = string.Empty;
            Cst.InputSourceDataStyle inputSource = new Cst.InputSourceDataStyle();
            FieldInfo[] flds = inputSource.GetType().GetFields();

            foreach (FieldInfo fld in flds)
            {
                if (fld.Name == InputSourceDataStyle.ToString())
                {
                    Cst.InputSourceDataStyleAttribute[] attrs = (Cst.InputSourceDataStyleAttribute[])
                        fld.GetCustomAttributes(typeof(Cst.InputSourceDataStyleAttribute), true);

                    if (0 < attrs.Length)
                    {
                        ret = attrs[0].Description;
                    }
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table {pQuoteTable}.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuoteTable"></param>
        /// <param name="pIdMarketEnv"></param>
        /// <param name="pIdValScenario"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] Ajout (déplacée ici à partir de SpanXml.cs)
        /// FI 20181226 [24308] Add pQuoteSide
        protected static QueryParameters GetQueryParameters_QUOTE_XXX_H(string pCS, Cst.OTCml_TBL pQuoteTable, string pIdMarketEnv, string pIdValScenario, DateTime pDate, QuotationSideEnum pQuoteSide)
        {
            string noData = null;
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdMarketEnv);
            dataParameters.Add(new DataParameter(pCS, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdValScenario);
            dataParameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32));
            dataParameters.Add(new DataParameter(pCS, "TIME", DbType.DateTime), pDate);
            dataParameters.Add(new DataParameter(pCS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), noData);
            // FI 20181226 [24308] 
            //dataParameters.Add(new DataParameter(pCS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), QuotationSideEnum.OfficialClose.ToString());
            dataParameters.Add(new DataParameter(pCS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pQuoteSide.ToString());
            dataParameters.Add(new DataParameter(pCS, "CASHFLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), noData);

            string selectQuoteQuery = string.Format(SelectQUOTE_XXX_HQuery, pQuoteTable.ToString());
            QueryParameters qryParameters = new QueryParameters(pCS, selectQuoteQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du CONTRACTMULTIPLIER de la table ASSET_ETD
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pIdAUpd"></param>
        /// <param name="pDtUpd"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        /// PM 20160215 [21491] Déplacée ici à partir de MarketDataImportEurexPrisma
        // PM 20240122 [WI822] Rename
        protected static QueryParameters GetQueryUpdASSET_ETDMultiplier(string pCS, int pIdAsset, decimal pContractMultiplier, int pIdAUpd, DateTime pDtUpd)
        {
            string sql = @"update dbo.ASSET_ETD 
                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (IDASSET=@IDASSET)
                                and ((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
                                and exists (select 1 from dbo.DERIVATIVECONTRACT dc
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
                                             where (dc.ISAUTOSETTING = 1)
                                               and (da.IDDERIVATIVEATTRIB = ASSET_ETD.IDDERIVATIVEATTRIB))";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdAUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), pDtUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(pCS, sql, dp);

            return qry;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du Code ISIN de la table ASSET_ETD
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pCodeISIN"></param>
        /// <param name="pIdAUpd"></param>
        /// <param name="pDtUpd"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] New
        protected static QueryParameters GetQueryUpdASSET_ETDCodeISIN(string pCS, int pIdAsset, string pCodeISIN, int pIdAUpd, DateTime pDtUpd)
        {
            string sql = @"update dbo.ASSET_ETD 
                              set ISINCODE = @ISINCODE,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (IDASSET=@IDASSET)
                                and ((ISINCODE is null) or (ISINCODE != @ISINCODE))
                                and exists (select 1 from dbo.DERIVATIVECONTRACT dc
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
                                             where (dc.ISAUTOSETTING = 1)
                                               and (da.IDDERIVATIVEATTRIB = ASSET_ETD.IDDERIVATIVEATTRIB))";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISINCODE), pCodeISIN);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdAUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), pDtUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(pCS, sql, dp);

            return qry;
        }

        /// <summary>
        /// Initialise les membres m_IdMarketEnv & m_IdValScenario
        /// </summary>
        /// PM 20151124 [20124] Ajout (déplacée ici à partir de SpanXml.cs)
        protected void InitDefaultMembers()
        {
            // Lecture de IDMARKETENV
            string query = @"
            select me.IDMARKETENV
              from dbo.MARKETENV me
             where (me.ISDEFAULT = 1)";
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(Cs, "me", "DTFILE") + ")" + Cst.CrLf;
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE), m_dtFile);
            //
            QueryParameters queryParameters = new QueryParameters(Cs, query, dataParameters);
            object result = DataHelper.ExecuteScalar(CSTools.SetCacheOn(Cs), CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            //
            m_IdMarketEnv = (result != null ? Convert.ToString(result) : null);
            //
            // Lecture de IDVALSCENARIO
            query = @"
            select v.IDVALSCENARIO, 1 as colorder
              from dbo.VALSCENARIO v
             where (v.ISDEFAULT = 1)
               and (v.IDMARKETENV = @IDMARKETENV)";
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(Cs, "v", "DTFILE") + ")" + Cst.CrLf;
            query += @"
            union all
            select v.IDVALSCENARIO, 2 as colorder
              from dbo.VALSCENARIO v
             where (v.ISDEFAULT = 1) and (v.IDMARKETENV is null)";
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(Cs, "v", "DTFILE") + ")" + Cst.CrLf;
            query += "order by colorder asc";
            //
            dataParameters.Add(new DataParameter(Cs, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), m_IdMarketEnv);
            //
            queryParameters = new QueryParameters(Cs, query, dataParameters);
            result = DataHelper.ExecuteScalar(CSTools.SetCacheOn(Cs), CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            //
            m_IdValScenario = (result != null ? Convert.ToString(result) : null);
        }

        /// <summary>
        /// Lecture de l'IDM d'un marché grâce à son Exchange Acronym
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] Ajout
        protected int GetIdmFromExchangeAcronym(string pExchangeAcronym)
        {
            // Lecture de l'IDM du marché
            string query = @"
            select mk.IDM
              from dbo.MARKET mk
             where (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)";
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(Cs, "mk", "DTFILE") + ")" + Cst.CrLf;
            
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(Cs, "EXCHANGEACRONYM", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pExchangeAcronym);
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTFILE), m_dtFile);
            
            QueryParameters queryParameters = new QueryParameters(Cs, query, dataParameters);
            object idmObj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(Cs), CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            int ret = (idmObj != null) ? Convert.ToInt32(idmObj) : 0;
            
            return ret;
        }
        
        /// <summary>
        /// Indique si une quotation est modifiable ou non par rapport à sa source
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuoteSource"></param>
        /// <returns></returns>
        /// PM 20160219 [21924] New
        protected static bool IsQuoteOverridable( string pCS, string pQuoteSource )
        {
            bool isOverridable = true;
            if (StrFunc.IsFilled(pQuoteSource))
            {
                string query = "select e.CUSTOMVALUE from dbo.ENUM e where (e.CODE = 'InformationProvider') and (e.VALUE = '" + pQuoteSource + "')";
                object customValue = DataHelper.ExecuteScalar(pCS, CommandType.Text, query);
                if (customValue != null)
                {
                    string quoteSourceCustomValue = Convert.ToString(customValue);
                    isOverridable = (String.IsNullOrEmpty(quoteSourceCustomValue) || !(quoteSourceCustomValue.IndexOf("NOTOVERRIDABLE") >= 0));
                }
            }
            return isOverridable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        // PM 20240122 [WI822] Déplacée à partir de MarketDataImportEurexPrismaSTLPrices
        protected void OnQuoteSourceNotOverridable(object source, QuoteEventArgs e)
        {
            ArrayList message = new ArrayList();
            string msgDet = " (source:<b>" + e.DataRow["SOURCE"] + "</b>, rule:NOTOVERRIDABLE)";
            message.Insert(0, msgDet);
            message.Insert(0, "LOG-06073");
            ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        // PM 20240122 [WI822] Déplacée à partir de MarketDataImportEurexPrismaSTLPrices
        protected void OnQuoteUpdated(object source, QuoteEventArgs e)
        {
            DataRow row = e.DataRow;

            string table = $"<b>{e.DataRow.Table.TableName}</b>";
            string idAsset = $"<b>{e.DataRow["IDASSET"]}</b> ";
            string quoteSide = $"<b>{e.DataRow["QUOTESIDE"]}</b>";
            string time = $"<b>{DtFunc.DateTimeToString(Convert.ToDateTime(e.DataRow["TIME"]), DtFunc.FmtISODateTime)}</b>";
            string quoteTiming = $"<b>{e.DataRow["QUOTETIMING"]}</b>";
            string value = $"<b>{StrFunc.FmtDecimalToCurrentCulture(Convert.ToDecimal(e.DataRow["VALUE"]))}</b>";

            string info = string.Empty;

            if (row.RowState == DataRowState.Added)
            {
                info = $"Quotation Added => Table: {table}, IdAsset: {idAsset}, QuoteSide: {quoteSide}, Time: {time}, QuoteTiming: {quoteTiming}, Value: {value}";
            }
            else if (row.RowState == DataRowState.Modified)
            {
                info = $"Quotation Modified => IdQuote: <b>{e.DataRow["IDQUOTE_H"]}</b>, Table: {table}, IdAsset: {idAsset}, QuoteSide: {quoteSide}, Time: {time}, QuoteTiming: {quoteTiming}, Value: {value}";
            }
            Logger.Log(new LoggerData(LogLevelEnum.Debug, info, 3));
        }

        /// <summary>
        /// Sollicitation de SpheresQuotationHandling
        /// </summary>
        /// <param name="pSource">Représente un ASSET_ETD ou un DC</param>
        /// <param name="pContractMultiplier"></param>
        // PM 20240122 [WI822] Déplacée à partir de MarketDataImportEurexPrismaSTLPrices et modification des paramètres
        protected void SendQuotationHandlingContractMultiplierModified((Cst.OTCml_TBL Table, int Id) pSource, decimal pContractMultiplier)
        {
            Quote_ETDAsset quoteETD;
            switch (pSource.Table)
            {
                case Cst.OTCml_TBL.DERIVATIVECONTRACT:
                    // Quote Handling pour le contract
                    quoteETD = new Quote_ETDAsset
                    {
                        QuoteTable = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(),
                        idDC = pSource.Id,
                        idDCSpecified = true
                    };
                    break;
                case Cst.OTCml_TBL.ASSET_ETD:
                    // Quote Handling pour l'asset
                    quoteETD = new Quote_ETDAsset
                    {
                        QuoteTable = Cst.OTCml_TBL.ASSET_ETD.ToString(),
                        idAsset = pSource.Id
                    };
                    break;
                default:
                    throw new NotSupportedException($"Table: {pSource.Table} not supported");
            }

            quoteETD.action = DataRowState.Modified.ToString();
            quoteETD.contractMultiplier = pContractMultiplier;
            quoteETD.contractMultiplierSpecified = true;
            quoteETD.timeSpecified = true;
            quoteETD.time = m_dtFile;
            quoteETD.isCashFlowsVal = true;
            quoteETD.isCashFlowsValSpecified = true;

            MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = Cs };

            QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);

            IOTools.SendMQueue(task, qHMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
        }

        #endregion
    }
}
