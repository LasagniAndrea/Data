#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Business;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CheckTradeValidationLock
    public class CheckTradeValidationLock : CheckTradeValidationLockBase
    {
        #region Members
        private readonly TradeInput m_Input;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public CheckTradeValidationLock(string pCS, IDbTransaction pDbTransaction, TradeInput pTradeInput, Cst.Capture.ModeEnum pCaptureModeEnum, AppSession pSession)
            : base(pCS, pDbTransaction, pTradeInput.SQLInstrument, pCaptureModeEnum, pSession)
        {
            m_Input = pTradeInput;
        }
        #endregion constructor
        //
        #region Methods
        #region ValidationLocks
        public override bool ValidationLocks()
        {
            m_CheckLock = new Hashtable();
            CheckLockControl_Product();
            return ArrFunc.IsEmpty(m_CheckLock);
        }
        #endregion ValidationLocks

        #region CheckLockControl_Product
        private void CheckLockControl_Product()
        {
            // FI 20191210 [XXXXX] L'apple à CheckValidationLock_ETD est remplacé par CheckValidationLock_Alloc (Plus généraliste)
            /*
            DataDocumentContainer dataDocument = m_Input.DataDocument;
            ProductContainer product = dataDocument.currentProduct;
            if (product.isExchangeTradedDerivative)
            {
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.product;
                CheckValidationLock_ETD(exchangeTradedDerivative);
            }
            */

            if (Tools.IsUseEntityMarket(CSTools.SetCacheOn(CS), DbTransaction, m_Input.Product) && m_Input.IsAllocation)
                CheckValidationLock_Alloc();
        }

        #endregion CheckLockControl_Product

        /// <summary>
        /// Contôle si un traitement de journée, ou autre...  (vérification sur TypeLockEnum.ENTITYMARKET) est pas en cours. 
        /// </summary>
        /// FI 20191210 [XXXXX] Add Method (inspirée de CheckValidationLock_ETD)
        private void CheckValidationLock_Alloc()
        {
            RptSideProductContainer rptSide = m_Input.DataDocument.CurrentProduct.RptSide();
            if (null == rptSide)
                throw new NullReferenceException("rptSideContainer is null");

            DateTime dtClearingBusiness = rptSide.GetBusinessDate();

            int idAEntity = m_Input.DataDocument.GetFirstEntity(CSTools.SetCacheOn(CS), DbTransaction);
            if (idAEntity <= 0)
                throw new Exception("No entity found is dataDocument");

            rptSide.GetMarket(CSTools.SetCacheOn(CS), DbTransaction, out SQL_Market sqlMarket);
            if ((sqlMarket == null))
                throw new Exception("No market found is dataDocument");

            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(CS, DbTransaction, idAEntity, sqlMarket.Id, rptSide.IdA_Custodian)
            {
                DbTransaction = DbTransaction
            };

            sqlEntityMarket.LoadTable(new string[] { "IDEM, DTENTITY" });
            DateTime dtBusiness = sqlEntityMarket.DtEntity;
            if (dtClearingBusiness <= dtBusiness)
            {
                // EG 20130610 [18671] Replace LockMode.Exclusive par LockMode.Shared
                LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, sqlEntityMarket.IdEM, "{IDEM}", LockTools.Shared);
                string msg = LockTools.SearchProcessLocks(CS, DbTransaction, lockEntityMarket, m_Session);
                if (StrFunc.IsFilled(msg))
                {
                    msg = msg.Replace("{IDEM}", sqlEntityMarket.Entity_IDENTIFIER + " - " + sqlEntityMarket.Market_IDENTIFIER);
                    AddCheckLockError(msg);
                }
            }
        }

        #endregion Methods
    }
    #endregion CheckTradeValidationLock
}