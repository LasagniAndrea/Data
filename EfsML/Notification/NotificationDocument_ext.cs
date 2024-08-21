using FpML.Interface;
using System.Collections.Generic;


namespace EfsML.Notification
{

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationId
    {
        #region Constructors
        public NotificationId() { }
        public NotificationId(string pIdScheme, string pValue)
        {
            notificationIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationConfirmationSystemIds
    {

        #region Constructors
        public NotificationConfirmationSystemIds() { }
        public NotificationConfirmationSystemIds(NotificationConfirmationSystemId[] pNcsId)
        {
            this.notificationConfirmationSystemId = pNcsId;
        }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationConfirmationSystemId
    {
        #region Constructors
        public NotificationConfirmationSystemId() { }
        public NotificationConfirmationSystemId(string pIdScheme, string pValue)
        {
            notificationConfirmationSystemIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PositionSubTotal
    {
        #region constructor
        public PositionSubTotal()
        {
            builder = new ConvertedPrices();
            @long = new AveragePrice();
            @short = new AveragePrice();
        }
        #endregion
    }

    /// <summary>
    /// Données complémentaires sur les Trades et les assets
    /// </summary>
    public partial class CommonData
    {
        #region constructor
        public CommonData()
        {
            trade = new List<CommonTradeAlloc>();
            positionUTI = new List<PositionUTIId>();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CommonTrade
    {

        public CommonTrade()
        { }
    }

    /// <summary>
    /// Données complémentaires sur les Trades
    /// </summary>
    public partial class CommonTradeAlloc
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CommonTradeAlloc()
            : base()
        {
            uti = new UTIAlloc();
            positionUtiRef = new PositionUTIReference();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PosSynthetics
    {

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public PosSynthetics()
        {
            posSynthetic = new List<PosSynthetic>();
        }
        #endregion constructor
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PosSynthetic
    {
        #region constructor
        public PosSynthetic()
        {
            posKey = new PositionKey();
            positionUtiRef = new PositionUTIReference();
            fda = new List<ReportAmountSideFundingAmount>();
            bwa = new List<ReportAmountSideBorrowingAmount>();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PosTrade
    {
        #region constructor
        public PosTrade()
        {
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150427 [20987] Modify
    public partial class PosTrades
    {
        #region constructor
        public PosTrades()
        {
            subTotal = new List<PositionSubTotal>();
            trade = new List<PosTrade>();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150825 [21287] Modify (Implementation de IPosactionTradeFee)
    public partial class PosAction : IPosactionTradeFee
    {
        #region constructor
        public PosAction()
        {
            trades = new PosActionTrades();
        }
        #endregion

        #region IPosactionTradeFee Membres

        int IPosactionTradeFee.IdPosActionDet
        {
            get
            {
                return this.IdPosActionDet;
            }
            set
            {
                this.IdPosActionDet = value;
            }
        }

        int ITradeFee.IdT
        {
            get
            {
                return this.trades.trade.OTCmlId;
            }
            set
            {
                this.trades.trade.OTCmlId = value;
            }
        }



        bool ITradeFee.FeeSpecified
        {
            get
            {
                return this.feeSpecified;
            }
            set
            {
                this.feeSpecified = value;
            }
        }

        ReportFee[] ITradeFee.Fee
        {
            get
            {
                return this.fee;
            }
            set
            {
                fee = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PosActions
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150427 [20987] Modify
        public PosActions()
        {
            this.posAction = new List<PosAction>();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PosActionTrades
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public PosActionTrades()
        {
            trade = new PosActionTrade();
            trade2 = new PosActionTrade();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TradeReport
    {
        #region constructor
        public TradeReport()
            : base()
        {
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150427 [20987] Modify
    /// FI 20150623 [21149] Rename
    public partial class TradesReport
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150427 [20987] Modify
        public TradesReport()
        {
            trade = new List<TradeReport>();
            subTotal = new List<PositionSubTotal>();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20140731 [20179] add class
    public partial class TradeCciMatch
    {
        #region constructor
        public TradeCciMatch()
            : base()
        {
        }
        #endregion constructor
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20140731 [20179] add class
    public partial class CciMatch
    {
        #region constructor
        public CciMatch()
        {
            label = new List<CciLabel>();
        }
        #endregion constructor
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20141218 [20574] Add Class
    public partial class PositionUTIId
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public PositionUTIId()
        {
            this.uti = new UTIAlloc();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PositionUTIReference
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public PositionUTIReference()
        {
            href = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashPayments
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150427 [20987] Modify
        public CashPayments()
        {
            this.cashPayment = new List<CashPayment>();
        }
        #endregion cosntructor
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170217 [22862] Add
    public partial class DlvTrades
    {
        #region Members
        public DlvTrades ()
        {
            this.trade = new List<DlvTrade>(); 
        }
        #endregion
    }


    /// <summary>
    ///    
    /// </summary>
    /// FI 20170217 [22862] Add
    public partial class DlvTrade
    {
        #region Members
        public DlvTrade() : base()
        {
            this.phyQty = new PhysicalDelivery();
        }
        #endregion
    }


}
