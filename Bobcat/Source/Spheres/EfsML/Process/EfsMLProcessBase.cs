//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;

namespace EFS.Process
{

    /// <summary>
    ///  Classe de base pour les Process business de Spheres®
    ///  <Para>Process activé suite à l'arrivé d'un message Queue</Para>
    ///  <para>Cette classe nécessite contient des objets présents sous EfsML</para>
    /// </summary>
    public abstract class EfsMLProcessBase : ProcessBase
    {
        
        #region Members
        /// <summary>
        /// Cache
        /// </summary>
        /// PM 20130104 : Déplacé de ProcessTradeBase vers TradeBase
        protected ProcessCacheContainer m_ProcessCacheContainer;
        #endregion Member
        
        #region Accessors
        /// <summary>
        /// DataSetTrade Call mode (true = Trade table only; false = all table (default))
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>Used by API slave call</remarks>
        public bool IsDataSetTrade_AllTable
        { get; set; }


        /// <summary>
        /// Remplace IsDataSetTrade_AllTable (Enumerateur de table à charger)
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>Used by API slave call</remarks>
        public EfsML.TradeTableEnum TradeTable
        { get; set; }

        

        /// <summary>
        /// Stockage en cache des différents éléments utilisables dans un process 
        /// </summary>
        public ProcessCacheContainer ProcessCacheContainer
        {
            get { return m_ProcessCacheContainer; }
            set { m_ProcessCacheContainer = value; }
        }

        /// <summary>
        /// Dictionnaire du cache des EntityMarket
        /// </summary>
        public Dictionary<int, IPosKeepingMarket> EntityMarketCache
        {
            get { return ProcessCacheContainer.EntityMarketCache; }
            set { ProcessCacheContainer.EntityMarketCache = value; }
        }

        /// <summary>
        /// Dictionnaire du cache des cotations
        /// </summary>
        /// EG 20130528 Change Key int to Pair(Cst.UnderlyingAsset, int)
        // EG 20190716 [VCL : New FixedIncome] Upd (Dictionary<KeyQuoteAdditional, Quote> instead of Quote
        public Dictionary<Pair<Cst.UnderlyingAsset, int>, Dictionary<Pair<QuotationSideEnum, DateTime>, Dictionary<KeyQuoteAdditional, Quote>>> QuoteCache
        {
            get { return ProcessCacheContainer.QuoteCache; }
            set { ProcessCacheContainer.QuoteCache = value; }
        }

        /// <summary>
        /// Dictionnaire du cache des jours ouvrés sur Date (PRECEEDING / NONE / FOLLOWING)
        /// </summary>
        public Dictionary<DateTime, Dictionary<Pair<BusinessDayConventionEnum, string>, DateTime>> BusinessDateCache
        {
            get { return ProcessCacheContainer.BusinessDateCache; }
            set { ProcessCacheContainer.BusinessDateCache = value; }
        }

        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public EfsMLProcessBase()
            : base()
        { }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public EfsMLProcessBase(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
        }
        #endregion Constructors


        #region AddLogFeeInformation
        /// <summary>
        /// Alimentation du log avec le contenu des frais
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// <param name="pPayments">Représente les frais</param>
        public void AddLogFeeInformation(DataDocumentContainer pDataDocument, IPayment[] pPayments)
        {
            foreach (IPayment payment in pPayments)
                AddLogFeeInformation(pDataDocument, payment);
        }
        /// <summary>
        /// Alimentation du log avec les frais
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// <param name="pPayments">Représente les frais</param>
        /// <param name="pDetailEnum"></param>
        /// <param name="pLevelOrder"></param>
        public void AddLogFeeInformation(DataDocumentContainer pDataDocument, IPayment[] pPayments, LogLevelDetail pDetailEnum, int pLevelOrder)
        {
            foreach (IPayment payment in pPayments)
                AddLogFeeInformation(pDataDocument, payment, pDetailEnum, pLevelOrder);
        }

        /// <summary>
        // Alimentation du log avec la ligne de frais {pPayment}
        /// <para>Niveau 0 et Alimentation si mode DetailEnum.LEVEL4 </para>
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// <param name="pPayment">Représente les frais</param>
        public void AddLogFeeInformation(DataDocumentContainer pDataDocument, IPayment pPayment)
        {
            AddLogFeeInformation(pDataDocument, pPayment, LogLevelDetail.LEVEL4,0);
        }
        /// <summary>
        /// Alimentation du log  avec le contenu d'un frais
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// <param name="pPayment">Représente un frais</param>
        /// <param name="pDetailEnum"></param>
        /// <param name="pLevelOrder"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void AddLogFeeInformation(DataDocumentContainer pDataDocument, IPayment pPayment, LogLevelDetail pDetailEnum, int pLevelOrder)
        {
            string typePayment = Cst.NotAvailable;
            if (pPayment.PaymentTypeSpecified)
                typePayment = pPayment.PaymentType.Value;
            if (pPayment.PaymentSourceSpecified)
            {
                ISpheresIdSchemeId feeMatrix = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme);
                if (null != feeMatrix)
                {
                    typePayment += " : " + feeMatrix.Value;
                    ISpheresIdSchemeId feeSchedule = pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme);
                    if (null != feeSchedule)
                        typePayment += " / " + feeSchedule.Value;
                }
            }
            Nullable<int> idA_Payer = pDataDocument.GetOTCmlId_Party(pPayment.PayerPartyReference.HRef, PartyInfoEnum.id);
            Nullable<int> idA_Receiver = pDataDocument.GetOTCmlId_Party(pPayment.ReceiverPartyReference.HRef, PartyInfoEnum.id);
            IBookId bookPayer = pDataDocument.GetBookId(pPayment.PayerPartyReference.HRef);
            IBookId bookReceiver = pDataDocument.GetBookId(pPayment.ReceiverPartyReference.HRef);


            
            Logger.Log(new LoggerData(LoggerConversionTools.DetailEnumToLogLevelEnum(pDetailEnum), new SysMsgCode(SysCodeEnum.LOG, 5021), pLevelOrder,
                new LogParam(typePayment),
                new LogParam(pPayment.PaymentDateSpecified ? DtFunc.DateTimeToStringDateISO(pPayment.PaymentDate.UnadjustedDate.DateValue) : Cst.NotAvailable),
                new LogParam(LogTools.IdentifierAndId(pPayment.PayerPartyReference.HRef, idA_Payer.Value) + " / " +
                    ((null != bookPayer) ? LogTools.IdentifierAndId(bookPayer.BookName, bookPayer.OTCmlId) : "")),
                new LogParam(LogTools.IdentifierAndId(pPayment.ReceiverPartyReference.HRef, idA_Receiver.Value) + " / " +
                    ((null != bookReceiver) ? LogTools.IdentifierAndId(bookReceiver.BookName, bookReceiver.OTCmlId) : "")),
                new LogParam(StrFunc.FmtDecimalToInvariantCulture(pPayment.PaymentAmount.Amount.DecValue) + " " + pPayment.PaymentCurrency)));
        }
        #endregion SetFeeLogInformation
    }
}
