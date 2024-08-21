#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;



using EFS.Status;
using EFS.Tuning;
using EFS.Permission;

using EFS.Book; 

using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

using FixML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CheckEventValidationLock
    public class CheckEventValidationLock : CheckTradeValidationLockBase
    {
        #region Members
        private readonly TradeCommonInput m_TradeCommonInput;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public CheckEventValidationLock(string pCs, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, Cst.Capture.ModeEnum pCaptureModeEnum, AppSession pSession)
            : base(pCs, pDbTransaction, pTradeCommonInput?.SQLInstrument, pCaptureModeEnum, pSession)
        {
            m_TradeCommonInput = pTradeCommonInput;
        }
        #endregion constructor
        //
        #region Methods
        #region ValidationLocks
        public override bool ValidationLocks()
        {
            m_CheckLock = new Hashtable();
            if (null != m_TradeCommonInput)
                CheckLockControl_Product();
            return ArrFunc.IsEmpty(m_CheckLock);

        }
        #endregion ValidationLocks

        #region CheckLockControl_Product
        private void CheckLockControl_Product()
        {
            if (null != m_TradeCommonInput)
            {
                DataDocumentContainer dataDocument = m_TradeCommonInput.DataDocument;
                ProductContainer product = dataDocument.CurrentProduct;
                if (product.IsExchangeTradedDerivative)
                {
                    IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.Product;
                    CheckValidationLock_ETD(exchangeTradedDerivative);
                }
            }
        }
        #endregion CheckLockControl_Product
        //
        #region CheckValidationLock_ETD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeTradedDerivative"></param>
        /// FI 20140328 [19793] Refactoring
        private void CheckValidationLock_ETD(IExchangeTradedDerivative pExchangeTradedDerivative)
        {
            DateTime dtClearingBusiness = DateTime.MinValue;
            if (pExchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDateSpecified)
                dtClearingBusiness = pExchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDate.DateValue;

            int idAEntity = m_TradeCommonInput.DataDocument.GetFirstEntity(CSTools.SetCacheOn(CS));
            if (idAEntity <= 0)
                throw new Exception("No entity found is dataDocument");

            m_TradeCommonInput.DataDocument.CurrentProduct.GetMarket(CSTools.SetCacheOn(CS), null, out SQL_Market sqlMarket);
            if ((sqlMarket == null))
                throw new Exception("No market found is dataDocument");

            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(CS, null, idAEntity, sqlMarket.Id, null);
            //PM 20150528 [20575] Gestion DTENTITY
            //sqlEntityMarket.LoadTable(new string[] { "IDEM,DTMARKET" });

            //DateTime dtBusiness = sqlEntityMarket.dtMarket;
            sqlEntityMarket.LoadTable(new string[] { "IDEM,DTENTITY" });

            DateTime dtBusiness = sqlEntityMarket.DtEntity;
            if (dtClearingBusiness <= dtBusiness)
            {
                LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, sqlEntityMarket.IdEM, "{IDEM}", LockTools.Shared);
                string msg = LockTools.SearchProcessLocks(CS, null, lockEntityMarket, m_Session);
                if (StrFunc.IsFilled(msg))
                {
                    msg = msg.Replace("{IDEM}", sqlEntityMarket.Entity_IDENTIFIER + " - " + sqlEntityMarket.Market_IDENTIFIER);
                    AddCheckLockError(msg);
                }
            }
        }
        #endregion CheckValidationLock_ETD
        #endregion Methods
    }
    #endregion CheckEventValidationLock
}