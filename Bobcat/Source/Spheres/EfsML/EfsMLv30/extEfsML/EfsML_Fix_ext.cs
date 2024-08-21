#region using directives
using EFS.ACommon;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Shared;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1;
using FixML.v50SP1.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
#endregion using directives

namespace EfsML.v30.Fix
{

    /// <summary>
    /// Creation d'un classe abstraite de base pour ExchangeTradedDerivative et EquitySecurityTransaction
    /// Elle contient FIXML + CFICategorieEnum (pour compatibilité ascendante car positionnée en tête de classe)
    /// </summary>
    /// EG 20150624 [21151] New Refactoring
    public abstract partial class ExchangeTradedFIXML : IProduct, IExchangeTradedBase
    {
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        #region Accessors
        // EG 20160404 Migration vs2013
        [System.Xml.Serialization.XmlIgnore]
        public virtual TradeCaptureReport_message TradeCaptureReport
        {
            get
            {
                if ((null != fixML.fixMsgTypeMessage) && fixML.fixMsgTypeMessageSpecified && fixML.fixMsgTypeMessage.IsTradeCaptureReport)
                    return (TradeCaptureReport_message)fixML.fixMsgTypeMessage;
                else
                    return null;
            }
            set
            {
                fixML.fixMsgTypeMessage = value;
                fixML.fixMsgTypeMessageSpecified = true;
            }
        }
        #endregion Accessors

        #region IExchangeTradedBase Members
        /// <summary>
        /// Obtient la clearingOrganization
        /// </summary>
        IReference IExchangeTradedBase.ClearingOrganization
        {
            get
            {
                IReference ret = null;
                //
                IFixTrdCapRptSideGrp trdCapRptSideGrp = (IFixTrdCapRptSideGrp)TradeCaptureReport.RptSide[0];
                IFixParty fixParty = RptSideTools.GetParty(trdCapRptSideGrp, PartyRoleEnum.ClearingOrganization);
                if (null != fixParty)
                    ret = fixParty.PartyId;
                return ret;
            }
        }
        IReference IExchangeTradedBase.ClearingFirm
        {
            get
            {
                IReference ret = null;
                IFixTrdCapRptSideGrp trdCapRptSideGrp = (IFixTrdCapRptSideGrp)TradeCaptureReport.RptSide[0];
                IFixParty fixParty = RptSideTools.GetParty(trdCapRptSideGrp, PartyRoleEnum.ClearingFirm);
                if (null != fixParty)
                    ret = fixParty.PartyId;
                return ret;
            }
        }
        // EG 20150624 [21151] New 
        bool IExchangeTradedBase.CategorySpecified
        {
            set { this.categorySpecified = true; }
            get { return true; }
        }
        // EG 20150624 [21151] Upd
        Nullable<CfiCodeCategoryEnum> IExchangeTradedBase.Category
        {
            set { if (value.HasValue) this.category = value.Value; }
            get { return this.category; }
        }
        IFixTradeCaptureReport IExchangeTradedBase.TradeCaptureReport
        {
            set { this.TradeCaptureReport = (TradeCaptureReport_message)value; }
            get { return this.TradeCaptureReport; }
        }

        /// <summary>
        /// Obtient le buyer
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        // EG 20150624 [21151] use ExchangeTradedContainer
        IReference IExchangeTradedBase.BuyerPartyReference
        {
            get
            {
                IReference ret = null;
                ExchangeTradedContainer container = new ExchangeTradedContainer(this);
                IFixParty buyer = container.GetBuyer();
                if (null != buyer)
                    ret = buyer.PartyId;
                return ret;
            }
        }
        /// <summary>
        /// Obtient le seller
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        // EG 20150624 [21151] use ExchangeTradedContainer
        IReference IExchangeTradedBase.SellerPartyReference
        {
            get
            {
                IReference ret = null;
                ExchangeTradedContainer container = new ExchangeTradedContainer(this);
                IFixParty seller = container.GetSeller();
                if (null != seller)
                    ret = seller.PartyId;
                return ret;
            }
        }

        public IFixInstrument CreateFixInstrument()
        {
            return new InstrumentBlock();
        }
        // EG 20150624 [21151] New 
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        void IExchangeTradedBase.InitPositionTransfer(decimal pQuantity, DateTime pDtBusiness)
        {
            TradeCaptureReport.bizDtSpecified = true;
            TradeCaptureReport.bizDt = new EFS_Date
            {
                DateValue = pDtBusiness
            };
            TradeCaptureReport.lastQtySpecified = true;
            TradeCaptureReport.lastQty = new EFS_Decimal(pQuantity);
            bool mayOverwriteSecondaryTrdType = (false == TradeCaptureReport.TrdTyp2Specified) ||
                                                (TradeCaptureReport.TrdTyp2 == SecondaryTrdTypeEnum.RegularTrade) ||
                                                (TradeCaptureReport.TrdTyp2 == SecondaryTrdTypeEnum.LateTrade);

            if (mayOverwriteSecondaryTrdType && TradeCaptureReport.TrdTypSpecified)
            {
                int _trdTypeValue = Convert.ToInt32(ReflectionTools.ConvertEnumToString<TrdTypeEnum>(TradeCaptureReport.TrdTyp));
                TradeCaptureReport.TrdTyp2Specified = (_trdTypeValue < 1000);
                TradeCaptureReport.TrdTyp2 = (SecondaryTrdTypeEnum)((int)TradeCaptureReport.TrdTyp);
            }
            TradeCaptureReport.TrdTypSpecified = true;
            TradeCaptureReport.TrdTyp = TrdTypeEnum.PortfolioTransfer;
            TradeCaptureReport.TrdSubTypSpecified = true;
            TradeCaptureReport.TrdSubTyp = TrdSubTypeEnum.InternalTransferOrAdjustment;
        }
        #endregion IExchangeTradedBase Members
    }

    #region ExchangeTradedDerivative
    // EG 20150624 [21151] Upd
    public partial class ExchangeTradedDerivative : IProduct, IExchangeTradedDerivative
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeTradedDerivative efs_ExchangeTradedDerivative;
        #endregion Members
        //
        #region Accessors
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlIgnore]
        //public TradeCaptureReport_message tradeCaptureReport
        //{
        //    get
        //    {
        //        if ((null != fixML.fixMsgTypeMessage) && fixML.fixMsgTypeMessageSpecified && fixML.fixMsgTypeMessage.IsTradeCaptureReport)
        //            return (TradeCaptureReport_message)fixML.fixMsgTypeMessage;
        //        else
        //            return null;
        //    }
        //    set
        //    {
        //        fixML.fixMsgTypeMessage = value;
        //        fixML.fixMsgTypeMessageSpecified = true;
        //    }
        //}
        #endregion Accessors
        //
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170122 [23734] Modify
        public ExchangeTradedDerivative()
        {
            fixML = new FIXML();
            // FI 20170122 [23734] il y a tjs une categorie (Option ou future)
            categorySpecified = true; 
        }
        #endregion Constructor
        //
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        //
        #region IExchangeTradedDerivative Members
        EFS_ExchangeTradedDerivative IExchangeTradedDerivative.Efs_ExchangeTradedDerivative
        {
            set { efs_ExchangeTradedDerivative = value; }
            get { return efs_ExchangeTradedDerivative; }
        }
        #endregion IExchangeTradedDerivative Members
    }
    #endregion ExchangeTradedDerivative

    #region EquitySecurityTransaction
    /// EG 20150306 [POC-BERKELEY] : Add marginRatio and Accessors (OpenTerminationDate|InitialMarginPayerPartyReference|InitialMarginReceiverPartyReference)
    public partial class EquitySecurityTransaction : IProduct, IProductBase, IEquitySecurityTransaction
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EquitySecurityTransaction efs_EquitySecurityTransaction;
        #endregion Members
        #region Accessors
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return efs_EquitySecurityTransaction.ClearingBusinessDate; }
        }
        #endregion ClearingBusinessDate
        #region OpenTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate OpenTerminationDate
        {
            get { return efs_EquitySecurityTransaction.OpenTerminationDate; }
        }
        #endregion OpenTerminationDate
        #region InitialMarginPayerPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                return efs_EquitySecurityTransaction.InitialMarginPayerPartyReference;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                return efs_EquitySecurityTransaction.InitialMarginReceiverPartyReference;
            }
        }
        #endregion InitialMarginReceiverPartyReference

        #region TradeCaptureReport
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public TradeCaptureReport_message tradeCaptureReport
        //{
        //    get
        //    {
        //        if ((null != fixML.fixMsgTypeMessage) && fixML.fixMsgTypeMessageSpecified && fixML.fixMsgTypeMessage.IsTradeCaptureReport)
        //            return (TradeCaptureReport_message)fixML.fixMsgTypeMessage;
        //        else
        //            return null;
        //    }
        //    set
        //    {
        //        fixML.fixMsgTypeMessage = value;
        //        fixML.fixMsgTypeMessageSpecified = true;
        //    }
        //}
        #endregion TradeCaptureReport
        #endregion Accessors
        #region Constructor
        public EquitySecurityTransaction()
        {
            fixML = new FIXML();
        }
        #endregion Constructor
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members


        #region IEquitySecurityTransaction Members
        // EG 20150624 [21151] New
        bool IExchangeTradedBase.CategorySpecified
        {
            set { ; }
            get { return false; }
        }

        EFS_EquitySecurityTransaction IEquitySecurityTransaction.Efs_EquitySecurityTransaction
        {
            set { efs_EquitySecurityTransaction = value; }
            get { return efs_EquitySecurityTransaction; }
        }
        IMarginRatio IEquitySecurityTransaction.CreateMarginRatio
        {
            get
            {
                return new MarginRatio();
            }
        }
        IPayment IEquitySecurityTransaction.GrossAmount
        {
            set { this.grossAmount = (Payment)value; }
            get { return this.grossAmount; }
        }

        /// <summary>
        /// 
        /// </summary>
        Nullable<CfiCodeCategoryEnum> IExchangeTradedBase.Category
        {
            set { ; }
            get { return null; }
        }
        IMarginRatio IEquitySecurityTransaction.MarginRatio
        {
            set { marginRatio = (MarginRatio) value; }
            get { return marginRatio; }
        }
        bool IEquitySecurityTransaction.MarginRatioSpecified
        {
            set { marginRatioSpecified = value; }
            get { return marginRatioSpecified; }
        }
    

        #endregion IEquitySecurityTransaction Members
    }
    #endregion EquitySecurityTransaction

}
