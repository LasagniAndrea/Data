#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Interface;
using FixML.Interface;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public class CciProductEquitySecurityTransaction : CciProductExchangeTradedBase 
    {
        #region Members

        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public EquitySecurityTransactionContainer EquitySecurityTransaction
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public CciPayment CciGrossAmount
        {
            get;
            private set;
        }

        #endregion Accessors

        #region Constructors
        public CciProductEquitySecurityTransaction(CciTrade pCciTrade, IEquitySecurityTransaction pEquitySecurityTransaction, string pPrefix)
            : this(pCciTrade, pEquitySecurityTransaction, pPrefix, -1)
        {
        }
        public CciProductEquitySecurityTransaction(CciTrade pCciTrade, IEquitySecurityTransaction pEquitySecurityTransaction, string pPrefix, int pNumber)
            : base(pCciTrade, (IExchangeTradedBase)pEquitySecurityTransaction, pPrefix, pNumber)
        {
            
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// EG 20150306 [POC-BERKELEY] : Add marginRatio
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, EquitySecurityTransaction.EquitySecurityTransaction);
            base.Initialize_FromCci();

            CciGrossAmount.Initialize_FromCci();

            if (null == EquitySecurityTransaction.MarginRatio)
                EquitySecurityTransaction.MarginRatio = EquitySecurityTransaction.EquitySecurityTransaction.CreateMarginRatio;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            CciGrossAmount.AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150316 [XXXXX] Modify
        public override void Initialize_FromDocument()
        {
            base.Initialize_FromDocument();

            // FI 20150316 [XXXXX] call InitCciGrossAmountDefaultDateSettings
            InitCciGrossAmountDefaultDateSettings();

            CciGrossAmount.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();
            CciGrossAmount.Dump_ToDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20141105 [20466] Modify
        /// EG 20150306 [POC-BERKELEY] : Refactoring
        /// FI 20150316 [XXXXX] Modify
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);

            CciGrossAmount.ProcessInitialize(pCci);


            if (CciFixTradeCaptureReport.IsCci(CciFixTradeCaptureReport.CciEnum.LastPx, pCci) ||
                CciFixTradeCaptureReport.IsCci(CciFixTradeCaptureReport.CciEnum.LastQty, pCci))
            {
                IFixTradeCaptureReport tradeCaptureReport = EquitySecurityTransaction.TradeCaptureReport;
                if (tradeCaptureReport.LastPxSpecified && tradeCaptureReport.LastQtySpecified)
                {
                    CustomCaptureInfo cciAsset = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.ID);
                    if ((null != cciAsset) && (null != cciAsset.Sql_Table))
                    {
                        SQL_AssetEquity sql_Asset = (SQL_AssetEquity)cciAsset.Sql_Table;
                        string idC = sql_Asset.IdC;

                        decimal price = tradeCaptureReport.LastPx.DecValue;
                        decimal qty = tradeCaptureReport.LastQty.DecValue;

                        // FI 20141105 [20466] convertion du prix dans la devise cotée (cas du GBX)
                        Pair<Nullable<decimal>, string> priceGrossAmount = Tools.ConvertToQuotedCurrency(CciTrade.CSCacheOn, null, new Pair<Nullable<decimal>, string>(price, idC));

                        //FI 20120705 [17991] importation des equityTransaction (Test existence CciPayment.CciEnum.amount) 
                        //FI 20120705 [17996] alimentation de la devise
                        IMoney grosAmount = Product.ProductBase.CreateMoney(qty * priceGrossAmount.First.Value, priceGrossAmount.Second);
                        if (CcisBase.Contains(CciGrossAmount.CciClientId(CciPayment.CciEnum.amount)))
                        {
                            CciGrossAmount.Cci(CciPayment.CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(grosAmount.Amount.DecValue);
                            CciGrossAmount.Cci(CciPayment.CciEnum.currency).NewValue = grosAmount.Currency;
                        }
                        else
                        {
                            CciGrossAmount.Money.Amount = (EFS_Decimal)grosAmount.Amount.Clone();
                            CciGrossAmount.Money.Currency = grosAmount.Currency;
                        }
                    }
                }
            }
            else if (CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.ID, pCci))
            {
                //FI 20150316 [XXXXX] call InitCciGrossAmountDefaultDateSettings
                InitCciGrossAmountDefaultDateSettings();
            }

            //FI 20120705 [17991] add preproposition du payment date avec BizDt
            //FI 20150316 [XXXXX] Init de la date si saisie de l'asset ou de la date de clearing
            //FI 20190520 [XXXXX] Utilisation de IsCCiReferenceForInitPaymentDate
            if (IsCCiReferenceForInitPaymentDate(pCci) ||
               (CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.ID, pCci)))
            {
                //CciGrossAmount.clientIdDefaultDate = cciTrade.cciMarket[0].CciClientId(CciMarketParty.CciEnum.clearedDate);  

                // FI 20191021 [XXXXX] Modification de l'initilisation de _cciGrossAmount. clientIdDefaultDate
                // puisqu'en duplication de trade la date de référence n'est potentiellement pas saisie et dans ce cas _cciGrossAmount.clientIdDefaultDate n'était pas valorisé
                //if (IsCCiReferenceForInitPaymentDate(pCci))
                if (StrFunc.IsEmpty(CciGrossAmount.ClientIdDefaultDate))
                {
                    CustomCaptureInfo cciDate = CciTrade.cciProduct.GetCCiReferenceForPaymentDate();
                    if (null != cciDate)
                        CciGrossAmount.ClientIdDefaultDate = cciDate.ClientId_WithoutPrefix;
                }

                CciGrossAmount.PaymentDateInitialize(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = base.IsClientId_PayerOrReceiver(pCci);
            
            if (!isOk)
                isOk = CciGrossAmount.IsClientId_PayerOrReceiver(pCci);
            
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            CciGrossAmount.CleanUp();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            CciGrossAmount.SetDisplay(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            CciGrossAmount.Initialize_Document();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            CciGrossAmount.RefreshCciEnabled();
        }

        #endregion IContainerCciFactory Members

        #region IContainerCciPayerReceiver members
        /// <summary>
        /// Synchronisation des payers/receivers par rapport aux parties
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
            CciGrossAmount.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver


        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = base.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled); 
            #region buttons of cciGrossAmount
            #region buttons settlementInfo
            if (!isOk)
            {
                isOk = CciGrossAmount.IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Element = "settlementInformation";
                    pCo.Object = "grossAmount";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = CciGrossAmount.IsSettlementInfoSpecified;
                    pIsEnabled = CciGrossAmount.IsSettlementInstructionSpecified;
                }
            }
            #endregion buttons settlementInfo
            #endregion
            //
            return isOk;
        }
        #endregion Membres de IContainerCciGetInfoButton

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {

            CustomCaptureInfo cci = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.ID);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_EQUITY);


            if (StrFunc.IsFilled(pPage.ActiveElementForced))
            {
                //Si le focus est positionné sur le contrôle Contrat Spheres® affiche le panel ds lequel il se trouve
                if (pPage.ActiveElementForced == Cst.TXT + this.CciFixTradeCaptureReport.CciFixInstrument.CciClientId(CciFixInstrument.CciEnum.ID))
                {
                    ShowPanelAsset(pPage);
                }
            }

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion
        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {
            EquitySecurityTransaction = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)pProduct, CciTradeCommon.TradeCommonInput.DataDocument);
            base.SetProduct(pProduct);

            CciGrossAmount = new CciPayment(CciTrade, -1, EquitySecurityTransaction.GrossAmount, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_grossAmount,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
            {
                ProcessQueueCciDate = CustomCaptureInfosBase.ProcessQueueEnum.High
            };
        }

        /// <summary>
        /// Affiche Le panel Caractéristiques Actif
        /// </summary>
        private static void ShowPanelAsset(CciPageBase pPage)
        {
            pPage.ShowLinkControl(Cst.IMG + "equitySecurityTransaction_FIXML_TrdCapRpt_tblAssetEquityDesc");
        }
        
        /// <summary>
        /// Initialisation d'un offset sur l'objet cciGrossAmount de manière à pré-proposé une date de rglt qui tient compte du délai de règlement livraison du titre	
        /// </summary>
        /// FI 20150316 [XXXXX] Add 
        private void InitCciGrossAmountDefaultDateSettings()
        {
            CciGrossAmount.DefaultDateSettings = null;
            
            CustomCaptureInfo cciAsset = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.ID);
            if ((null != cciAsset) && (null != cciAsset.Sql_Table))
            {
                SQL_AssetEquity sql_Asset = (SQL_AssetEquity)cciAsset.Sql_Table;

                IOffset offset = null;
                if (sql_Asset.StlOffsetMultiplier.HasValue && StrFunc.IsFilled(sql_Asset.StlOffsetPeriod) && StrFunc.IsFilled(sql_Asset.StlOffsetDaytype))
                {
                    FpML.Enum.PeriodEnum period = (FpML.Enum.PeriodEnum)Enum.Parse(typeof(FpML.Enum.PeriodEnum), sql_Asset.StlOffsetPeriod, true);
                    FpML.Enum.DayTypeEnum dayType = (FpML.Enum.DayTypeEnum)Enum.Parse(typeof(FpML.Enum.DayTypeEnum), sql_Asset.StlOffsetDaytype, true);
                    offset = CciTrade.DataDocument.CurrentProduct.ProductBase.CreateOffset(
                          period, sql_Asset.StlOffsetMultiplier.Value, dayType);
                }

                IBusinessCenters businessCenters = null;
                if (StrFunc.IsFilled(sql_Asset.Market_IDBC))
                    businessCenters = CciTrade.DataDocument.CurrentProduct.ProductBase.CreateBusinessCenters(sql_Asset.Market_IDBC);

                if ((null != offset) && (null != businessCenters))
                    CciGrossAmount.DefaultDateSettings = new Pair<IOffset, IBusinessCenters>(offset, businessCenters);
            }
        }


        #endregion
    }
}
