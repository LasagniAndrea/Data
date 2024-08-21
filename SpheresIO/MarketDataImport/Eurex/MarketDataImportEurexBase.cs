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
        private string m_ExchangeAcronym = default;
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
                if (default == m_ExchangeAcronym)
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
            m_QueryExistDC.Parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //m_QueryExistDCInTrade.parameters["EXCHANGEACRONYM"].Value = EUREX_EXCHANGEACRONYM;
            m_QueryExistDCInTrade.Parameters["EXCHANGEACRONYM"].Value = ExchangeAcronym;
            m_QueryExistAssetFut.Parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
            m_QueryExistAssetOpt.Parameters["EXCHANGESYMBOL"].Value = EUREX_MIC;
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
        #endregion
    }
}
