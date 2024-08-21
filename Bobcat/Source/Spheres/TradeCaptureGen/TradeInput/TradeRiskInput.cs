#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Data;

#endregion Using Directives

namespace EFS.TradeInformation
{


    /// <summary>
    /// Classe chargée de l'alimentation du dataDocument via les ccis (contexte RISK)
    /// </summary>
    public class TradeRiskInput : TradeCommonInput
    {
        /// <summary>
        /// 
        /// </summary>
        private EventItems _marRequirementEvents;

        /// <summary>
        /// 
        /// </summary>
        public new TradeCustomCaptureInfos CustomCaptureInfos
        {
            set { base.CustomCaptureInfos = value; }
            get { return (TradeCustomCaptureInfos)base.CustomCaptureInfos; }
        }

        /// <summary>
        ///  Obtient les évènements LPC/MGR
        /// </summary>
        public EventItems MarginRequirementEvents
        {
            get { return _marRequirementEvents; }
        }

        //
        #region Constructor
        public TradeRiskInput() : base() { }
        #endregion Constructors
        //
        #region Methods

        /// <summary>
        /// Définit une nouvelle instance de SQl_TRADE
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pTradeIdentifier)
        {
            InitializeSqlTrade(pCS, pDbTransaction,  pTradeIdentifier, SQL_TableWithID.IDType.Identifier);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            m_SQLTrade = new SQL_TradeRisk(pCS, pId, pIdType)
            {
                DbTransaction = pDbTransaction
            };
        }

        /// <summary>
        /// Définit une nouvelle instance des ccis
        /// <para>
        /// Synchronize des pointeurs existants dans les CciContainers avec les éléments du dataDocument
        /// </para>
        /// </summary>
        /// <param name="pUser"></param>
        /// <param name="pSessionID"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Mofification de signature
        // EG 20180425 Analyse du code Correction [CA2214]
        public override void InitializeCustomCaptureInfos(string pCS, User pUser, string pSessionID, bool pIsGetDefaultOnInitializeCci)
        {
            this.CustomCaptureInfos = new TradeCustomCaptureInfos(pCS, this, pUser, pSessionID, pIsGetDefaultOnInitializeCci);
            this.CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        protected override bool CheckIsTradeFound(string pCS, IDbTransaction pDbTransaction,  string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
            bool isTemplate = TradeRDBMSTools.IsTradeTemplate(pCS, pDbTransaction, pId, pIdType);
            if (isTemplate)
                stEnv = Cst.StatusEnvironment.TEMPLATE;

            string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);

            SQL_TradeRisk sqlTrade = new SQL_TradeRisk(pCS, pIdType, sqlTradeId, stEnv, SQL_Table.RestrictEnum.Yes, pUser, pSessionId)
            {
                DbTransaction = pDbTransaction
            };
            return sqlTrade.IsFound;
        }

        /// <summary>
        /// Charge les évènement EventCode: LPC/LPI et EventType: MGR
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        public void LoadEventMarginRequirement(string pCS, IDbTransaction pDbTransaction)
        {
            IMarginRequirement mrgRequirement = (IMarginRequirement)Product.Product;
            //                    
            _marRequirementEvents = null;
            //
            string eventCode = string.Empty;
            if (mrgRequirement.Timing == SettlSessIDEnum.EndOfDay)
                eventCode = EventCodeFunc.LinkedProductClosing;
            else if (mrgRequirement.Timing == SettlSessIDEnum.Intraday)
                eventCode = EventCodeFunc.LinkedProductIntraday;
            else
                throw new NotImplementedException(StrFunc.AppendFormat("timing {0} is not implemented", eventCode));
            //
            int idT = Identification.OTCmlId;
            int[] ide = EventRDBMSTools.GetEvents(pCS, pDbTransaction, idT, eventCode, EventTypeFunc.MarginRequirement, 1, 1);
            //
            if (ArrFunc.IsFilled(ide))
            {
                DataSetEvent dsEvent = new DataSetEvent(pCS);
                dsEvent.Load(pDbTransaction, idT, ide, new DataSetEventLoadSettings(DataSetEventLoadEnum.Event));
                _marRequirementEvents = dsEvent.GetEventItems();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _marRequirementEvents = null;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMode"></param>
        public override void InitializeForAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            base.InitializeForAction(pCS, pMode);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetDefaultValue(string pCS, IDbTransaction pDbTransaction)
        {

            if (this.Product.ProductBase.IsBulletPayment)
            {
                if ((SQLProduct.Identifier == Cst.ProductCollateral))
                {
                    IBulletPayment bullet = (IBulletPayment)this.Product.Product;
                    bool isSetDefault = (false == bullet.Payment.PaymentDateSpecified);
                    if (false == isSetDefault)
                        isSetDefault = StrFunc.IsEmpty(bullet.Payment.PaymentDate.UnadjustedDate.Value);
                    //
                    if (isSetDefault)
                    {
                        ITradeExtend extend = DataDocument.Extends.GetSpheresIdFromScheme2("Collateral-EFFECTIVE");
                        if (null != extend)
                            bullet.Payment.PaymentDate.UnadjustedDate.Value = extend.Value;
                    }
                }
            }

            base.SetDefaultValue(pCS, pDbTransaction);
        }

        #endregion


    }


    /// <summary>
    /// 
    /// </summary>
    public class TradeRiskInputGUI : TradeCommonInputGUI
    {
        #region Accessors

        /// <summary>
        ///  Obtient l'identifiant du regroupement des produits 
        /// </summary>
        /// FI 20140930 [XXXXX] Modify 
        public override Cst.SQLCookieGrpElement GrpElement
        {
            get
            {
                Cst.SQLCookieGrpElement ret = Cst.SQLCookieGrpElement.SelRiskProduct;

                //FI 20140930 [XXXXX] GrpElement est fonction du menu
                Nullable<EFS.ACommon.IdMenu.Menu> menu = EFS.ACommon.IdMenu.ConvertToMenu(IdMenu);
                switch (menu)
                {
                    case EFS.ACommon.IdMenu.Menu.InputTradeRisk_InitialMargin:
                        ret = Cst.SQLCookieGrpElement.SelRiskProductMargin;
                        break;
                    case EFS.ACommon.IdMenu.Menu.InputTradeRisk_CashBalance:
                        ret = Cst.SQLCookieGrpElement.SelRiskProductCashBalance;
                        break;
                    case EFS.ACommon.IdMenu.Menu.InputTradeRisk_CashPayment:
                        ret = Cst.SQLCookieGrpElement.SelRiskProductCashPayment;
                        break;
                    case EFS.ACommon.IdMenu.Menu.InputTradeRisk_CashInterest: 
                        ret = Cst.SQLCookieGrpElement.SelRiskProductCashInterest;
                        break;
                }
                
                return ret;
            }
        }

        /// <summary>
        ///  Retoune la restriction sur la table PRODUCT
        /// </summary>
        /// <param name="pSqlAlias">alias de la table PRODUCT</param>
        /// FI 20140930 [XXXXX]
        /// FI 20140930 [XXXXX] Modify 
        public override string GetSQLRestrictProduct(string pSqlAlias)
        {
            StrBuilder ret = new StrBuilder(StrFunc.AppendFormat("{0}.GPRODUCT='{1}'", pSqlAlias, Cst.ProductGProduct_RISK));

            // FI 20140930 [XXXXX] add restriction supplémentaire en fonction de GrpElement
            switch (GrpElement)
            {
                case Cst.SQLCookieGrpElement.SelRiskProduct:
                    break;
                case Cst.SQLCookieGrpElement.SelRiskProductMargin:
                    ret += SQLCst.AND +  StrFunc.AppendFormat("{0}.FAMILY='{1}'", pSqlAlias, Cst.ProductFamily_MARGIN);
                    break;
                case Cst.SQLCookieGrpElement.SelRiskProductCashBalance:
                    ret += SQLCst.AND + StrFunc.AppendFormat( "{0}.FAMILY='{1}'", pSqlAlias, Cst.ProductFamily_CASHBALANCE);
                    break;
                case Cst.SQLCookieGrpElement.SelRiskProductCashPayment:
                    ret += SQLCst.AND + StrFunc.AppendFormat("{0}.FAMILY='{1}'", pSqlAlias, Cst.ProductFamily_CASHPAYMENT);
                    break;
                case Cst.SQLCookieGrpElement.SelRiskProductCashInterest:
                    ret += SQLCst.AND + StrFunc.AppendFormat("{0}.FAMILY='{1}'", pSqlAlias, Cst.ProductFamily_CASHINTEREST);
                    break;
            }

            return ret.ToString() ;
        }
        
        /// <summary>
        /// Obtient "Risk"
        /// </summary>
        public override string MainRessource
        {
            get
            {
                return "Risk";
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pUser"></param>
        /// <param name="pXMLFilePath"></param>
        public TradeRiskInputGUI(string pIdMenu, User pUser, string pXMLFilePath)
            : base(pIdMenu, pUser, pXMLFilePath)
        {
        }
        #endregion Constructors
    }

}