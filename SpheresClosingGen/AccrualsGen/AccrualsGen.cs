#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML;
using EfsML.Business;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.Accruals
{
    #region public class AccrualsBook
    /// <summary>
    /// Représente les Types de gestion des amortissements associé à un book 
    /// </summary>
    public class AccrualsBook
    {
        #region Members
        /// <summary>
        /// Représente le Book
        /// </summary>
        public int idB;
        /// <summary>
        /// Représente le type de gestion des amortissements pour les flux fixes paramétré sur le book
        /// </summary>
        public string bookLinearDepPeriod;
        /// <summary>
        /// Représente le type de gestion des amortissements pour intérêts réglés en début de période paramétré sur le book
        /// </summary>
        public string bookAccruedIntPeriod;
        /// <summary>
        /// Représente l'entité de associée au book
        /// </summary>
        public int idAEntity;
        /// <summary>
        /// Représente le type de gestion des amortissements pour les flux fixes paramétré sur l'entité
        /// </summary>
        public string entityLinearDepPeriod;
        /// <summary>
        /// Représente le type de gestion des amortissements pour intérêts réglés en début de période paramétré sur l'entité
        /// </summary>
        public string entityAccruedIntPeriod;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// Représente le type de gestion des amortissements pour les flux fixes
        /// </summary>
        public string LinearDepPeriod
        {
            get { return (StrFunc.IsEmpty(bookLinearDepPeriod) ? entityLinearDepPeriod : bookLinearDepPeriod); }
        }
        /// <summary>
        /// Représente le type de gestion des amortissements pour intérêts réglés en début de période
        /// </summary>
        public string AccruedIntPeriod
        {
            get { return (StrFunc.IsEmpty(bookAccruedIntPeriod) ? entityAccruedIntPeriod : bookAccruedIntPeriod); }
        }
        #endregion
        //
        #region Constructors
        public AccrualsBook(int pIdB, string pBookLinearDepPeriod, string pBookAccruedIntPeriod, int pIdAEntity, string pEntityLinearDepPeriod, string pEntityAccruedIntPeriod)
        {
            idB = pIdB;
            bookLinearDepPeriod = pBookLinearDepPeriod;
            bookAccruedIntPeriod = pBookAccruedIntPeriod;
            idAEntity = pIdAEntity;
            entityLinearDepPeriod = pEntityLinearDepPeriod;
            entityAccruedIntPeriod = pEntityAccruedIntPeriod;
        }
        #endregion Constructors
    }
    #endregion

    #region public AccrualsBase
    public class AccrualsBase : ProcessTradeBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected DataSetTrade m_DsTrade;

        /// <summary>
        /// 
        /// </summary>
        protected EFS_TradeLibrary m_TradeLibrary;

        /// <summary>
        /// Liste des books, chaque item contient un  AccrualsBook
        /// </summary>
        public Hashtable m_Books;
        #endregion Members
        #region Constructor
        public AccrualsBase(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance)
        {
        }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// Alimente m_Books à partir du trade
        /// </summary>
        // EG 20150706 [21021] Nullable<int> for idB
        protected void SetAccrualsBookList()
        {
            m_Books = new Hashtable();
            foreach (IParty party in m_TradeLibrary.Party)
            {
                Nullable<int> idB = m_TradeLibrary.DataDocument.GetOTCmlId_Book(party.Id);
                if (idB.HasValue)
                {
                    SQL_Book book = new SQL_Book(Cs, idB.Value);
                    bool isEntityOk = true;
                    //
                    if (book.IsLoaded)
                    {
                        if ((StrFunc.IsFilled(book.LinearDepPeriod)) && (StrFunc.IsFilled(book.AccruedIntPeriod)))
                        {
                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, book.LinearDepPeriod, book.AccruedIntPeriod, book.IdA_Entity, string.Empty, string.Empty));
                        }
                        else
                        {
                            #region Get Entity LinearDepPeriod
                            if (0 < book.IdA_Entity)
                            {
                                SQL_Actor actor = new SQL_Actor(Cs, book.IdA_Entity)
                                {
                                    WithInfoEntity = true
                                };
                                if (actor.IsLoaded)
                                {
                                    if (StrFunc.IsFilled(book.LinearDepPeriod))
                                    {
                                        if (StrFunc.IsFilled(actor.AccruedIntPeriod))
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, book.LinearDepPeriod, string.Empty, book.IdA_Entity, string.Empty, actor.AccruedIntPeriod));
                                        else
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, book.LinearDepPeriod, string.Empty, book.IdA_Entity, string.Empty, string.Empty));
                                    }
                                    else if (StrFunc.IsFilled(book.AccruedIntPeriod))
                                    {
                                        if (StrFunc.IsFilled(actor.LinearDepPeriod))
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, string.Empty, book.AccruedIntPeriod, book.IdA_Entity, actor.LinearDepPeriod, string.Empty));
                                        else
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, string.Empty, book.AccruedIntPeriod, book.IdA_Entity, string.Empty, string.Empty));
                                    }
                                    else
                                    {
                                        if ((StrFunc.IsFilled(actor.LinearDepPeriod)) && (StrFunc.IsFilled(actor.AccruedIntPeriod)))
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, string.Empty, string.Empty, book.IdA_Entity, actor.LinearDepPeriod, actor.AccruedIntPeriod));
                                        else if (StrFunc.IsFilled(actor.LinearDepPeriod))
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, string.Empty, string.Empty, book.IdA_Entity, actor.LinearDepPeriod, string.Empty));
                                        else if (StrFunc.IsFilled(actor.AccruedIntPeriod))
                                            m_Books.Add(idB.Value.ToString(), new AccrualsBook(idB.Value, string.Empty, string.Empty, book.IdA_Entity, string.Empty, actor.AccruedIntPeriod));
                                    }
                                }
                                else
                                    isEntityOk = false;
                            }
                            else
                                isEntityOk = false;
                            #endregion
                        }

                        if (false == isEntityOk)
                        {
                            if (StrFunc.IsFilled(book.LinearDepPeriod))
                                m_Books.Add(idB.ToString(), new AccrualsBook(idB.Value, book.LinearDepPeriod, string.Empty, book.IdA_Entity, string.Empty, string.Empty));
                            else if (StrFunc.IsFilled(book.AccruedIntPeriod))
                                m_Books.Add(idB.ToString(), new AccrualsBook(idB.Value, string.Empty, book.AccruedIntPeriod, book.IdA_Entity, string.Empty, string.Empty));
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdbPayer"></param>
        /// <param name="pIdbReceiver"></param>
        /// <returns></returns>
        public bool CheckLinearDepPeriod(string pIdbPayer, string pIdbReceiver)
        {
            AccrualsBook bookPayer = (AccrualsBook)m_Books[pIdbPayer];
            AccrualsBook bookReceiver = (AccrualsBook)m_Books[pIdbReceiver];
            //
            if (null != bookPayer && null != bookReceiver)
            {
                if ((StrFunc.IsFilled(bookPayer.LinearDepPeriod) &&
                    StrFunc.IsFilled(bookReceiver.LinearDepPeriod) &&
                    bookPayer.LinearDepPeriod != bookReceiver.LinearDepPeriod))
                    return true;
                else if ((StrFunc.IsFilled(bookPayer.LinearDepPeriod)) &&
                    ((StrFunc.IsEmpty(bookReceiver.LinearDepPeriod)) ||
                    bookPayer.LinearDepPeriod == bookReceiver.LinearDepPeriod))
                    return (bookPayer.LinearDepPeriod == Cst.LinearDepreciation_Remaining);
                else if (StrFunc.IsFilled(bookReceiver.LinearDepPeriod) && StrFunc.IsEmpty(bookPayer.LinearDepPeriod))
                    return (bookReceiver.LinearDepPeriod == Cst.LinearDepreciation_Remaining);
            }
            else if (null != bookPayer)
                return (bookPayer.LinearDepPeriod == Cst.LinearDepreciation_Remaining);
            else if (null != bookReceiver)
                return (bookReceiver.LinearDepPeriod == Cst.LinearDepreciation_Remaining);
            //
            return false;
        }
        
       /// <summary>
       /// 
       /// </summary>
       /// <param name="pIdbPayer"></param>
       /// <param name="pIdbReceiver"></param>
       /// <returns></returns>
        public bool CheckAccruedIntPeriod(string pIdbPayer, string pIdbReceiver)
        {
            AccrualsBook bookPayer = (AccrualsBook)m_Books[pIdbPayer];
            AccrualsBook bookReceiver = (AccrualsBook)m_Books[pIdbReceiver];
            //
            if (null != bookPayer && null != bookReceiver)
            {
                if ((StrFunc.IsFilled(bookPayer.AccruedIntPeriod) &&
                    StrFunc.IsFilled(bookReceiver.AccruedIntPeriod) &&
                    bookPayer.AccruedIntPeriod != bookReceiver.AccruedIntPeriod))
                    return true;
                else if ((StrFunc.IsFilled(bookPayer.AccruedIntPeriod)) &&
                    ((StrFunc.IsEmpty(bookReceiver.AccruedIntPeriod)) ||
                    bookPayer.AccruedIntPeriod == bookReceiver.AccruedIntPeriod))
                    return (bookPayer.AccruedIntPeriod == Cst.AccruedIntPeriod_Remaining);
                else if (StrFunc.IsFilled(bookReceiver.AccruedIntPeriod) && StrFunc.IsEmpty(bookPayer.AccruedIntPeriod))
                    return (bookReceiver.AccruedIntPeriod == Cst.AccruedIntPeriod_Remaining);
            }
            else if (null != bookPayer)
                return (bookPayer.AccruedIntPeriod == Cst.AccruedIntPeriod_Remaining);
            else if (null != bookReceiver)
                return (bookReceiver.AccruedIntPeriod == Cst.AccruedIntPeriod_Remaining);
            return false;
        }
        #endregion Methods
    }
    #endregion AccrualsGenProcessBase

    #region AccrualsGenProcess
    public class AccrualsGenProcess : AccrualsBase
    {
        #region Constructor
        public AccrualsGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance) { }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 710), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
            }

            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            m_DsTrade = new DataSetTrade(Cs, CurrentId);
            m_TradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
            SetAccrualsBookList();
            return ProcessExecuteSimpleProduct(m_TradeLibrary.Product);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180502 Analyse du code Correction [CA2214]
        protected override Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            AccrualsGenProcessBase accrualsGenProcessBase = null;
            if (pProduct.ProductBase.IsIRD)
                accrualsGenProcessBase = new AccrualsGenProcessIRD(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsFx)
                accrualsGenProcessBase = new AccrualsGenProcessFX(this, m_DsTrade, m_TradeLibrary, pProduct);

            if (null != accrualsGenProcessBase)
            {
                accrualsGenProcessBase.InitializeDataSetEvent();
                codeReturn = accrualsGenProcessBase.Valorize();
            }
            return codeReturn;
        }
        #endregion Methods
    }
    #endregion AccrualsGenProcess
}
