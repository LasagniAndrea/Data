#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Drawing;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de GetQuotation.
    /// </summary>
    public partial class GetQuotationPage : PageBase
	{
        //DropDownList ddlAssetType;
        //DropDownList ddlAvailability;
        //DropDownList ddlQuoteSide;
        //DropDownList ddlQuoteTiming;
        //DropDownList ddlQuoteBasis;
        //DropDownList ddlPeriod;
        //DropDownList ddlTimeOperator;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack)
            {
                foreach(string s in Enum.GetNames(typeof(QuoteEnum)))
                {
                    ddlAssetType.Items.Add(new ListItem(s, s));
                }

                ddlAvailability.Items.Add(AvailabilityEnum.Enabled.ToString() + "/" + AvailabilityEnum.Disabled.ToString());
				ddlAvailability.Items.Add(AvailabilityEnum.Enabled.ToString());
                ddlAvailability.Items.Add(AvailabilityEnum.Disabled.ToString());

                ControlsTools.DDLLoad_MarketEnv(SessionTools.CS, ddlIdMarketEnv, true);
                ControlsTools.DDLLoad_ValScenario(SessionTools.CS, ddlIdValScenario, true);

                ControlsTools.DDLLoad_QuotationSide(SessionTools.CS, ddlQuoteSide, true);
                ControlsTools.DDLLoad_QuoteTiming(SessionTools.CS, ddlQuoteTiming, true);
                ControlsTools.DDLLoad_QuoteBasis(SessionTools.CS, ddlQuoteBasis, true);
                ControlsTools.DDLLoad_Period(SessionTools.CS, ddlPeriod, true);
                
                ddlTimeOperator.Items.Add("=");
                ddlTimeOperator.Items.Add(">");
                ddlTimeOperator.Items.Add(">=");
                ddlTimeOperator.Items.Add("<");
                ddlTimeOperator.Items.Add("<=");

                ControlsTools.DDLLoad_ENUM(ddlAssetMeasure, SessionTools.CS, true, "AssetMeasure");
                ControlsTools.DDLLoad_ENUM(ddlCashFlowType, SessionTools.CS, true, "cashflow");

                ControlsTools.DDLLoad_Currency(SessionTools.CS, ddlIDC1, true);
                ControlsTools.DDLLoad_Currency(SessionTools.CS, ddlIDC2, true);
            }
		}

        #region Code généré par le Concepteur Web Form
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
			//
			InitializeComponent();
			base.OnInit(e);

            Form = frmQuotation;
            AntiForgeryControl();

            ddlAssetType.AutoPostBack = true;
            ddlAssetType.SelectedIndexChanged += new System.EventHandler(OnSelectedAssetType);
		}
        public void OnSelectedAssetType(object sender, EventArgs e)
        {
            QuoteEnum assetType = (QuoteEnum)System.Enum.Parse(typeof(QuoteEnum), ddlAssetType.SelectedValue);

            ddlIDC1.SelectedIndex = 0;
            ddlIDC1.Enabled = (assetType == QuoteEnum.FXRATE);
            ddlIDC2.SelectedIndex = 0;
            ddlIDC2.Enabled = (assetType == QuoteEnum.FXRATE);
            ddlQuoteBasis.SelectedIndex = 0;
            ddlQuoteBasis.Enabled = (assetType == QuoteEnum.FXRATE);

            txtRateIndex.Text = string.Empty;
            txtRateIndex.Enabled = (assetType == QuoteEnum.RATEINDEX);
            txtPeriodMultiplier.Text = string.Empty;
            txtPeriodMultiplier.Enabled = (assetType == QuoteEnum.RATEINDEX);
            ddlPeriod.SelectedIndex = 0;
            ddlPeriod.Enabled = (assetType == QuoteEnum.RATEINDEX);
        }

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{    

        }
		#endregion

        // EG 20190716 [VCL : New FixedIncome] Upd
        protected void BtnSearch_Click(object sender, System.EventArgs e)
        {
            string source = SessionTools.CS;
            //2008212 PL Utilisation d'un product Fra, car on doit disposer d'un Product...
            FpML.v44.Ird.Fra product = new FpML.v44.Ird.Fra();
            SQL_Quote sql_Quote = null;
			QuoteEnum quote = (QuoteEnum)System.Enum.Parse(typeof(QuoteEnum), ddlAssetType.SelectedValue, true);
            AvailabilityEnum availability = AvailabilityEnum.NA;
            if (ddlAvailability.SelectedValue == AvailabilityEnum.Enabled.ToString())
                availability = AvailabilityEnum.Enabled ;
            else if (ddlAvailability.SelectedValue == AvailabilityEnum.Disabled.ToString())
                availability = AvailabilityEnum.Disabled; 
            
            if (txtIdQuote.Text.Length > 0)
            {
                sql_Quote = new SQL_Quote(source, quote, availability, (IProductBase)product, Convert.ToInt32(txtIdQuote.Text));
            }
            else
            {
                DateTime dtTime = DateTime.Today.AddDays(1);
                if (StrFunc.IsFilled(txtTime.Text))
                    dtTime = Convert.ToDateTime(txtTime.Text);
                KeyQuote keyQuote = new KeyQuote(source, dtTime);
                if (StrFunc.IsFilled(txtTime.Text))
                    keyQuote.TimeOperator = ddlTimeOperator.SelectedValue;
                else
                    keyQuote.TimeOperator = "<";
                if (ddlIdMarketEnv.SelectedIndex > 0)
                    keyQuote.IdMarketEnv = ddlIdMarketEnv.SelectedValue;
                if (ddlIdValScenario.SelectedIndex > 0)
                    keyQuote.IdValScenario = ddlIdValScenario.SelectedValue;
                //
                keyQuote.QuoteSide = null;
                if (StrFunc.IsFilled(ddlQuoteSide.SelectedValue))
                    keyQuote.QuoteSide = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum), ddlQuoteSide.SelectedValue);
                //    
                keyQuote.QuoteTiming = null;
                if (StrFunc.IsFilled(ddlQuoteTiming.SelectedValue))
                    keyQuote.QuoteTiming = (QuoteTimingEnum)System.Enum.Parse(typeof(QuoteTimingEnum), ddlQuoteTiming.SelectedValue);
                //
                keyQuote.KeyQuoteAdditional = new KeyQuoteAdditional
                {
                    AssetMeasure = null,
                    CashflowType = ReflectionTools.ConvertStringToEnumOrNullable<CashFlowTypeEnum>(ddlCashFlowType.SelectedValue)
                };
                if (StrFunc.IsFilled(ddlAssetMeasure.SelectedValue))
                    keyQuote.KeyQuoteAdditional.AssetMeasure = (AssetMeasureEnum)System.Enum.Parse(typeof(AssetMeasureEnum), ddlAssetMeasure.SelectedValue); 
                //    
                //    
                if (txtIdAsset.Text.Length > 0)
                    sql_Quote = new SQL_Quote(source, quote, availability, (IProductBase)product, keyQuote, Convert.ToInt32(txtIdAsset.Text));
                else if (txtAsset_Identifier.Text.Length > 0)
                    sql_Quote = new SQL_Quote(source, quote, availability, (IProductBase)product, keyQuote, txtAsset_Identifier.Text);
                else
                {
                    string assetType = ddlAssetType.SelectedValue;
                    switch ((QuoteEnum)Enum.Parse(typeof(QuoteEnum), assetType, true))
                    {
                        case QuoteEnum.FXRATE:
                            KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate();
                            string basis = ddlQuoteBasis.SelectedValue;
                            keyAssetFxRate.IdC1 = ddlIDC1.SelectedValue;
                            keyAssetFxRate.IdC2 = ddlIDC2.SelectedValue;
                            if (StrFunc.IsFilled(basis))
                            {
                                keyAssetFxRate.QuoteBasis = (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), basis, true);
                                keyAssetFxRate.QuoteBasisSpecified = true;
                            }
                            else
                                keyAssetFxRate.QuoteBasisSpecified = false;
                            sql_Quote = new SQL_Quote(source, quote, availability, (IProductBase)product, keyQuote, keyAssetFxRate);
                            break;
                        case QuoteEnum.RATEINDEX:
                            KeyAssetRateIndex keyAssetRateIndex = new KeyAssetRateIndex();
                            string period = ddlPeriod.SelectedValue;
                            keyAssetRateIndex.rateIndex_Identifier = txtRateIndex.Text;
                            if (StrFunc.IsFilled(txtPeriodMultiplier.Text))
                                keyAssetRateIndex.periodMultiplier = Convert.ToInt32(txtPeriodMultiplier.Text);
                            if (StrFunc.IsFilled(period))
                                keyAssetRateIndex.period = StringToEnum.Period(period);
                            sql_Quote = new SQL_Quote(source, quote, availability, (IProductBase)product, keyQuote, keyAssetRateIndex);
                            break;
                    }
                }
            }
			
            #region Display Result
            txtOutStatus.ForeColor = Color.White;
            txtOutQuoteSource.Text = "";
            txtOutMarket_ISO10383_ALPHA4.Text = "";
            txtOutIdMarketEnv.Text = "";
            txtOutIdValScenario.Text = "";
            txtOutAvailability.Text = "";
            txtOutQuoteSide.Text = "";
            txtOutQuoteTiming.Text = "";
            txtOutTime.Text = "";
            txtOutValue.Text = "";
            txtOutQuoteUnit.Text = "";
            txtOutAssetMeasure.Text = "";
            txtOutCashFlowType.Text = "";
            txtOutIdAsset.Text = ""; 
            txtOutIdQuote.Text = "";
			if (sql_Quote.IsLoaded && (sql_Quote.Rows.Count >= 1))
            {
                txtOutIdMarketEnv.Text = sql_Quote.IdMarketEnv;
                txtOutIdValScenario.Text = sql_Quote.IdValScenario;

                if (sql_Quote.Rows.Count == 1)
                {
                    #region Found
                    txtOutStatus.BackColor = Color.DarkGreen;
                    txtOutStatus.Text = "Quote found.";

                    txtOutQuoteSource.Text = sql_Quote.QuoteSource;
                    txtOutMarket_ISO10383_ALPHA4.Text = sql_Quote.Market_ISO10383_ALPHA4 + " (" + sql_Quote.Market_IDBC + ")";
                    txtOutAvailability.Text = BoolFunc.IsTrue(sql_Quote.GetFirstRowColumnValue("ISENABLED")) ? AvailabilityEnum.Enabled.ToString() : AvailabilityEnum.Disabled.ToString();
                    txtOutQuoteSide.Text = sql_Quote.QuoteSide;
                    txtOutQuoteTiming.Text = sql_Quote.QuoteTiming;
                    txtOutTime.Text = sql_Quote.Time.ToString();
                    txtOutValue.Text = sql_Quote.QuoteValue.ToString();
                    txtOutQuoteUnit.Text = sql_Quote.QuoteUnit;
                    txtOutAssetMeasure.Text = sql_Quote.AssetMeasure;
                    txtOutCashFlowType.Text = sql_Quote.CashFlowType;
                    txtOutIdAsset.Text = sql_Quote.IdAsset.ToString(); 
                    txtOutIdQuote.Text = sql_Quote.IdQuote.ToString();
                    #endregion
                }
                else
                {
                    #region Found Multiple
                    try
                    {
                        txtOutStatus.BackColor = Color.DarkOrange;
                        txtOutStatus.Text = "Several quotes founded!";

                        txtOutIdAsset.Text = sql_Quote.IdAssetIN.ToString();
                    }
                    catch { }
                    #endregion
                }
            }
			else
            {
                #region Not Found
				try
				{
					if (sql_Quote.IdAssetIN != 0)
					{
                        txtOutStatus.BackColor = Color.DarkRed;
                        txtOutStatus.Text = "Quote not found!";

						txtOutIdAsset.Text = sql_Quote.IdAssetIN.ToString();
					}
					else
					{
                        txtOutStatus.BackColor = Color.Red;
                        txtOutStatus.Text = "Asset not found!";
					}
                    txtOutTime.Text = sql_Quote.KeyQuoteIN.QuoteTiming.ToString();
				}
				catch{ }
                #endregion
            }
            #endregion 
        }
	}
}