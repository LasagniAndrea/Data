#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public class CciProductExchangeTradedDerivative : CciProductExchangeTradedBase 
    {
        #region Members
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public ExchangeTradedDerivativeContainer ExchangeTradedDerivative { get; private set; }
        #endregion Accessors

        #region Constructors
        public CciProductExchangeTradedDerivative(CciTrade pCciTrade, IExchangeTradedDerivative pExchangeTradedDerivative, string pPrefix)
            : this(pCciTrade, pExchangeTradedDerivative, pPrefix, -1)
        {
        }
        public CciProductExchangeTradedDerivative(CciTrade pCciTrade, IExchangeTradedDerivative pExchangeTradedDerivative, string pPrefix, int pNumber)
            : base(pCciTrade, (IExchangeTradedBase)pExchangeTradedDerivative, pPrefix, pNumber)
        {
        
            //SetProduct((IProduct)pExchangeTradedDerivative);
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, ExchangeTradedDerivative.ExchangeTradedDerivative);
            base.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region Category
                        case CciEnum.Category:
                            if (ExchangeTradedDerivative.CategorySpecified)
                                data = ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(ExchangeTradedDerivative.Category.Value);
                            break;
                        #endregion Category
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            base.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {

            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    
                    switch (cciEnum)
                    {
                        #region Category
                        case CciEnum.Category:
                            CfiCodeCategoryEnum categoryEnum = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(ExchangeTradedDerivative.Category, data);
                            ExchangeTradedDerivative.Category = categoryEnum;
                            break;
                        #endregion Category
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            base.Dump_ToDocument();
        }
        #endregion IContainerCciFactory Members

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.ID);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_ETD);

            cci = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.Sym);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.DERIVATIVECONTRACT);

            cci = CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.MMY);
            if (null != cci)
            {
                if (pPage.PlaceHolder.FindControl(Cst.TXT + cci.ClientId_WithoutPrefix) is WCTextBox2 txt)
                {
                    string maturity = ExchangeTradedDerivative.TradeCaptureReport.Instrument.MaturityMonthYear;
                    if (StrFunc.IsFilled(maturity))
                    {
                        txt.Text = MaturityHelper.FormatMaturityFIX( maturity, Ccis.FmtETDMaturityInput);
                    }
                }
            }
            //
            if (StrFunc.IsFilled(pPage.ActiveElementForced))
            {
                //Si le focus est positionné sur le contrôle Contrat Spheres® affiche le panel ds lequel il se trouve
                if (pPage.ActiveElementForced == Cst.DDL + this.CciFixTradeCaptureReport.CciFixInstrument.CciClientId(CciFixInstrument.CciEnum.Sym))
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
            ExchangeTradedDerivative = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)pProduct);
            base.SetProduct(pProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pInitialString"></param>
        /// <returns></returns>
        public override string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {

            string ret = pInitialString;
            //
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                // TRD_DERIVATIVE_CONTRACT
                if (true == ret.Contains(Cst.TRD_DERIVATIVE_CONTRACT))
                {
                    string derivativeContract = Product.GetDerivativeContract();
                    ret = ret.Replace(Cst.TRD_DERIVATIVE_CONTRACT, derivativeContract);
                }
                // TRD BUSINESSDATE
                if (true == ret.Contains(Cst.BUSINESSDATE))
                {
                    // RD 20151112 [21537] Use GetBusinessDate2() instead GetBusinessDate()
                    DateTime dtBusinessDate = Product.GetBusinessDate2();
                    ret = ret.Replace(Cst.BUSINESSDATE, DtFunc.DateTimeToString(dtBusinessDate, DtFunc.FmtDateyyyyMMdd));
                }

            }
            //
            ret = base.ReplaceTradeDynamicConstantsWithValues(pCci, ret);
            //
            return ret;

        }

        /// <summary>
        /// Affiche Le panel Caractéristiques Actif
        /// </summary>
        private void ShowPanelAsset(CciPageBase pPage)
        {
            // FI 20200121 [XXXXX] Appel à ShowLinkControl
            pPage.ShowLinkControl(Cst.IMG + Prefix + "FIXML_TrdCapRpt_Instrmt_tblAssetETDDesc");
        }
        #endregion
    }
}
