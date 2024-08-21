using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EfsML;
using EfsML.Business;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    public partial class GetMaturityDate : PageBase
    {
        private string currentDateFormat; //Format des dates affichées
        private readonly static List<DCDatas> listDerivativeContracts = new List<DCDatas>(); //List contenant tous les DCDatas (infos sur les Deriv. Contracts)
        private readonly static List<DCDatas> listDerivativeContractsFilterByMarket = new List<DCDatas>(); //list contenant tous les DCDatas suivant le Marché sélectionné
        private readonly static List<MRDatas> listMaturityRule = new List<MRDatas>(); //List contenant tous les MRDatas (infos sur les Maturity Rules)

        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form = frmMaturityDate;
            AntiForgeryControl();
        }
        private void Page_Load(object sender, EventArgs e)
        {
            btn_Calculate.Text = "Calculate";
            btn_Validate.Text = "Submit";
            currentDateFormat = ddl_DateFormat.SelectedValue;

            if (false == IsPostBack) //Premier chargement de la page
            {
                LoadDerivativeContractsList();
                listDerivativeContractsFilterByMarket.AddRange(listDerivativeContracts);
                LoadMaturityRulesList();

                btn_Calculate.Enabled = ddl_IdMaturityRule.Enabled = ddl_MaturityMontYear.Enabled = false;

                ddl_DateFormat.Items.Add(new ListItem("dd/MM/yyyy"));
                ddl_DateFormat.Items.Add(new ListItem("MM/dd/yyyy"));
                ddl_DateFormat.Items.Add(new ListItem("yyyy/MM/dd"));
                ddl_DateFormat.Items.Add(new ListItem("yyyy/MMM/dd"));

                ControlsTools.DDLLoad_MarketETD(SessionTools.CS, ddl_Market, true, false, false, true);
            }
        }

        /// <summary>
        /// Charger listDerivativeContracts
        /// </summary>
        private void LoadDerivativeContractsList()
        {
            listDerivativeContracts.Clear();
            listDerivativeContractsFilterByMarket.Clear();

            StrBuilder sqlSelect_DC = new StrBuilder();
            sqlSelect_DC += SQLCst.SELECT + "IDDC, IDENTIFIER, IDM, IDMATURITYRULE";
            sqlSelect_DC += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT;

            DataSet dsSelect_DC = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect_DC.ToString());

            foreach (DataRow row in dsSelect_DC.Tables[0].Rows)
            {
                uint IDDC;
                string IDENTIFIER;
                uint IDM;
                uint? IDMATURITYRULE;

                IDDC = Convert.ToUInt32(row["IDDC"].ToString());
                IDENTIFIER = row["IDENTIFIER"].ToString();
                IDM = Convert.ToUInt32(row["IDM"].ToString());
                IDMATURITYRULE = row["IDMATURITYRULE"].ToString().Length > 0 ? Convert.ToUInt32(row["IDMATURITYRULE"].ToString()) : 0;

                DCDatas dc = new DCDatas
                {
                    ID = IDDC,
                    Identifier = IDENTIFIER,
                    IDM = IDM,
                    IDMaturityRule = IDMATURITYRULE
                };

                listDerivativeContracts.Add(dc);
            }
        }

        /// <summary>
        /// Charger listMaturityRule
        /// </summary>
        private void LoadMaturityRulesList()
        {
            listMaturityRule.Clear();
            ddl_IdMaturityRule.Items.Clear();
            ddl_IdMaturityRule.Items.Add(new ListItem(String.Empty, "0"));

            StrBuilder sqlSelect_MR = new StrBuilder();
            sqlSelect_MR += SQLCst.SELECT + "IDMATURITYRULE, IDENTIFIER, MMYRULE";
            sqlSelect_MR += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITYRULE;
            sqlSelect_MR += SQLCst.ORDERBY + "IDENTIFIER ASC";

            DataSet dsSelect_MR = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect_MR.ToString());

            foreach (DataRow row in dsSelect_MR.Tables[0].Rows)
            {
                uint IDMATURITYRULE;
                string IDENTIFIER;
                string MMYRULE;

                IDMATURITYRULE = Convert.ToUInt32(row["IDMATURITYRULE"].ToString());
                IDENTIFIER = row["IDENTIFIER"].ToString();
                MMYRULE = row["MMYRULE"].ToString();

                MRDatas mr = new MRDatas
                {
                    ID = IDMATURITYRULE,
                    Identifier = IDENTIFIER,
                    Frequency = MMYRULE
                };

                listMaturityRule.Add(mr);
                ddl_IdMaturityRule.Items.Add(new ListItem(mr.Identifier, mr.ID.ToString()));
            }
        }

        /// <summary>
        /// Charge listDerivativeContractsFilterByMarket avec les Deriv. Contracts du Marché sélectionné
        /// Méthode appelée lorsque ddl_Market change d'index
        /// </summary>
        protected void MarketChanged(object sender, EventArgs e)
        {
            listDerivativeContractsFilterByMarket.Clear();

            if (ddl_Market.SelectedIndex != 0)
                listDerivativeContractsFilterByMarket.AddRange(listDerivativeContracts.Where(dc => dc.IDM == Convert.ToUInt32(ddl_Market.SelectedValue)));
            else
                listDerivativeContractsFilterByMarket.AddRange(listDerivativeContracts);
        }

        /// <summary>
        /// Retourne une liste contenant les Deriv. Contracts dont l'Identifier débute par le paramètre "identifier"
        /// Nécessaire à l'autocomplete sur la TextBox txt_DcIdentifier
        /// </summary>
        [WebMethod]
        public static List<DCDatas> FetchDerivativeContractIdentifier(string identifier)
        {
            var derivcontract = listDerivativeContractsFilterByMarket.Where(dc => dc.Identifier.ToLower().StartsWith(identifier.ToLower()));
            return derivcontract.ToList();
        }

        /// <summary>
        /// Met à jour les TextBoxes et les DropDownLists avec les infos sur le Deriv. Contract sélectionné
        /// Méthode appelée lors du click sur btn_Validate
        /// </summary>
        protected void ClickOnValidateButton(object sender, EventArgs e)
        {
            DCDatas derivcontract = new DCDatas();

            try
            {
                if ((txt_IdDc.Text.Length > 0) && (UInt32.TryParse(txt_IdDc.Text, out uint iddc)))
                    derivcontract = listDerivativeContracts.Where(dc => dc.ID == iddc).First();
                else if (txt_DcIdentifier.Text.Length > 0)
                    if (ddl_Market.SelectedIndex == 0)
                        derivcontract = listDerivativeContractsFilterByMarket.Where(dc => dc.Identifier == txt_DcIdentifier.Text).First();
                    else
                        derivcontract = listDerivativeContractsFilterByMarket.Where(dc => dc.Identifier == txt_DcIdentifier.Text &&
                                                                                          dc.IDM == Convert.ToUInt32(ddl_Market.SelectedValue))
                                                                             .First();
                else
                    throw new Exception();

                ddl_Market.BorderColor = txt_DcIdentifier.BorderColor = txt_IdDc.BorderColor = System.Drawing.Color.Green;
                ddl_Market.SelectedValue = derivcontract.IDM.ToString();
                txt_DcIdentifier.Text = derivcontract.Identifier;
                txt_IdDc.Text = derivcontract.ID.ToString();
                ddl_IdMaturityRule.Enabled = true;
                RefreshMRTextBoxes(derivcontract.IDMaturityRule);
            }
            catch (Exception)
            {
                ddl_Market.BorderColor = txt_DcIdentifier.BorderColor = txt_IdDc.BorderColor = System.Drawing.Color.Red;
                ddl_Market.SelectedIndex = 0;
                txt_DcIdentifier.Text = "Deriv. Contract not found";
                txt_IdDc.Text = String.Empty;
            }
        }

        /// <summary>
        /// Rafraichissement des TextBoxes et des DropDownLists concernant les Maturity Rules et les Maturities
        /// </summary>
        /// <param name="pID">ID de la MATURITYRULE</param>
        protected void RefreshMRTextBoxes(uint? pID)
        {
            if (pID > 0)
            {
                ddl_IdMaturityRule.BorderColor = System.Drawing.Color.Green;
                ddl_IdMaturityRule.SelectedValue = txt_IdMaturityRule.Text = pID.ToString();
                txt_Frequency.Text = listMaturityRule.Where(mr => mr.ID == pID).First().Frequency;
                btn_Calculate.Enabled = true;

                /* Mise à jour de ddl_MaturityMontYear avec les MATURITY de la MATURITYRULE */
                ddl_MaturityMontYear.Items.Clear();
                StrBuilder sqlSelect_M = new StrBuilder();
                sqlSelect_M += SQLCst.SELECT + "MATURITYMONTHYEAR, MATURITYDATE";
                sqlSelect_M += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITY;
                sqlSelect_M += SQLCst.WHERE + "IDMATURITYRULE = " + pID;
                sqlSelect_M += SQLCst.ORDERBY + "MATURITYMONTHYEAR ASC";
                DataSet dsSelect_M = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect_M.ToString());
                if (dsSelect_M.Tables[0].Rows.Count > 0)
                {
                    ddl_MaturityMontYear.Enabled = true;
                    ddl_MaturityMontYear.Items.Add(String.Empty);
                    foreach (DataRow row in dsSelect_M.Tables[0].Rows)
                        ddl_MaturityMontYear.Items.Add(new ListItem(row["MATURITYMONTHYEAR"].ToString(), row["MATURITYDATE"].ToString()));
                }
                else
                {
                    ddl_MaturityMontYear.Enabled = false;
                }
            }
            else
            {
                ddl_IdMaturityRule.BorderColor = System.Drawing.Color.Red;
                ddl_IdMaturityRule.SelectedIndex = 0;
                ddl_MaturityMontYear.Items.Clear();
                txt_IdMaturityRule.Text = txt_Frequency.Text = txt_MaturityDate.Text = txt_NewMaturityMonthYear.Text = String.Empty;
                btn_Calculate.Enabled = false;
            }
        }

        /// <summary>
        /// Méthode appelée lorsque ddl_IdMaturityRule change d'index
        /// </summary>
        protected void MaturityRuleChanged(object sender, System.EventArgs e)
        {
            RefreshMRTextBoxes(Convert.ToUInt32(ddl_IdMaturityRule.SelectedValue));
        }

        /// <summary>
        /// Méthode appelée lorsque l'index de la DDL des Maturities est modifié
        /// </summary>
        protected void SearchMaturity(object sender, System.EventArgs e)
        {
            Month.Visible = true;
            txt_NewMaturityMonthYear.Text = String.Empty;

            if (ddl_MaturityMontYear.SelectedIndex != 0)
            {
                /* Calcul des Maturities avec la Maturity Date sélectionnée */
                txt_MaturityDate.Text = Convert.ToDateTime(ddl_MaturityMontYear.SelectedValue).Date.ToString(currentDateFormat);
                CalculateMaturities(ddl_MaturityMontYear.SelectedItem.ToString(), txt_Month, txt_CalcMaturityDate, txt_CalcLastTradingDate,
                                    txt_CalcDeliveryDate,
                                    txt_CalcFirstDeliveryDate, txt_CalcFirstDlvSettltDate, txt_CalcLastDeliveryDate, txt_CalcLastDlvSettltDate, 
                                    txt_CalcFirstNoticeDay, txt_CalcLastNoticeDay);
            }
            else
            {
                txt_MaturityDate.Text = String.Empty;
            }
        }

        /// <summary>
        /// Méthode appelée lors d'un click sur btn_Calculate
        /// </summary>
        protected void ClickOnCalculateButton(object sender, System.EventArgs e)
        {
            System.Text.RegularExpressions.Regex rgYYYY = new Regex(@"^([1][9]\d{2}|[2]\d{3})$");
            System.Text.RegularExpressions.Regex rgYYYYMM = new Regex(@"^([1][9]\d{2}|[2]\d{3})(0?[1-9]|1[012])$");

            ddl_MaturityMontYear.SelectedIndex = -1;
            txt_MaturityDate.Text = String.Empty;

            if (txt_NewMaturityMonthYear.Text.Length > 0)
            {
                if (rgYYYY.IsMatch(txt_NewMaturityMonthYear.Text)) //Format YYYY (de 1900 à 2999)
                {
                    Month.Visible = false;

                    foreach (char month in txt_Frequency.Text) //Pour chaque mois de la Frequency
                    {
                        string monthNumber = StrFunc.GetMonthMM(month.ToString());

                        #region Création des lignes dans la table CalculatedMaturities
                        TableRow row = new TableRow(); //Création de la ligne

                        /* Month */
                        TableCell cellMonth = new TableCell();
                        TextBox boxMonth = new TextBox();
                        row.ID = "row" + monthNumber;
                        boxMonth.ID = "txt_Month" + monthNumber;
                        cellMonth.Controls.Add(boxMonth);
                        row.Controls.Add(cellMonth);

                        /* Maturity Date */
                        TableCell cellMaturityDate = new TableCell();
                        TextBox boxCalcMaturityDate = new TextBox
                        {
                            ID = "txt_CalcMaturityDate" + monthNumber
                        };
                        cellMaturityDate.Controls.Add(boxCalcMaturityDate);
                        row.Controls.Add(cellMaturityDate);

                        /* Last Trading Date */
                        TableCell cellLastTradingDate = new TableCell();
                        TextBox boxCalcLastTradingDate = new TextBox
                        {
                            ID = "txt_CalcLastTradingDate" + monthNumber
                        };
                        cellLastTradingDate.Controls.Add(boxCalcLastTradingDate);
                        row.Controls.Add(cellLastTradingDate);

                        /* NO PERIODIC Settlt./Deliv. Date */
                        TableCell cellDeliveryDate = new TableCell();
                        TextBox boxCalcDeliveryDate = new TextBox
                        {
                            ID = "txt_CalcDeliveryDate" + monthNumber
                        };
                        cellDeliveryDate.Controls.Add(boxCalcDeliveryDate);
                        row.Controls.Add(cellDeliveryDate);

                        /* PERIODIC First Deliv. Date */
                        TableCell cellFirstDeliveryDate = new TableCell();
                        TextBox boxCalcFirstDeliveryDate = new TextBox
                        {
                            ID = "txt_CalcFirstDeliveryDate" + monthNumber
                        };
                        cellFirstDeliveryDate.Controls.Add(boxCalcFirstDeliveryDate);
                        row.Controls.Add(cellFirstDeliveryDate);

                        /* PERIODIC First Settlt. Date */
                        TableCell cellFirstDlvSettltDate = new TableCell();
                        TextBox boxCalcFirstDlvSettltDate = new TextBox
                        {
                            ID = "txt_CalcFirstDlvSettltDate" + monthNumber
                        };
                        cellFirstDlvSettltDate.Controls.Add(boxCalcFirstDlvSettltDate);
                        row.Controls.Add(cellFirstDlvSettltDate);

                        /* PERIODIC Last Deliv. Date */
                        TableCell cellLastDeliveryDate = new TableCell();
                        TextBox boxCalcLastDeliveryDate = new TextBox
                        {
                            ID = "txt_CalcLastDeliveryDate" + monthNumber
                        };
                        cellLastDeliveryDate.Controls.Add(boxCalcLastDeliveryDate);
                        row.Controls.Add(cellLastDeliveryDate);

                        /* PERIODIC Last Settlt. Date */
                        TableCell cellLastDlvSettltDate = new TableCell();
                        TextBox boxCalcLastDlvSettltDate = new TextBox
                        {
                            ID = "txt_CalcLastDlvSettltDate" + monthNumber
                        };
                        cellLastDlvSettltDate.Controls.Add(boxCalcLastDlvSettltDate);
                        row.Controls.Add(cellLastDlvSettltDate);

                        /* First Notice Day */
                        TableCell cellFirstNoticeDay = new TableCell();
                        TextBox boxCalcFirstNoticeDay = new TextBox
                        {
                            ID = "txt_FirstNoticeDay" + monthNumber
                        };
                        cellFirstNoticeDay.Controls.Add(boxCalcFirstNoticeDay);
                        row.Controls.Add(cellFirstNoticeDay);

                        /* Last Notice Day */
                        TableCell cellLastNoticeDay = new TableCell();
                        TextBox boxCalcLastNoticeDay = new TextBox
                        {
                            ID = "txt_LastNoticeDay" + monthNumber
                        };
                        cellLastNoticeDay.Controls.Add(boxCalcLastNoticeDay);
                        row.Controls.Add(cellLastNoticeDay);

                        /* ReadOnly = true */
                        boxMonth.ReadOnly = boxCalcMaturityDate.ReadOnly = boxCalcLastTradingDate.ReadOnly =
                            boxCalcDeliveryDate.ReadOnly =
                            boxCalcFirstDeliveryDate.ReadOnly = boxCalcFirstDlvSettltDate.ReadOnly =
                            boxCalcLastDeliveryDate.ReadOnly = boxCalcLastDlvSettltDate.ReadOnly = 
                            boxCalcFirstNoticeDay.ReadOnly = boxCalcLastNoticeDay.ReadOnly = true;

                        /* text-align = "center" */
                        boxMonth.Style[HtmlTextWriterStyle.TextAlign] = boxCalcMaturityDate.Style[HtmlTextWriterStyle.TextAlign] = boxCalcLastTradingDate.Style[HtmlTextWriterStyle.TextAlign] =
                        boxCalcDeliveryDate.Style[HtmlTextWriterStyle.TextAlign] =
                        boxCalcFirstDeliveryDate.Style[HtmlTextWriterStyle.TextAlign] = boxCalcFirstDlvSettltDate.Style[HtmlTextWriterStyle.TextAlign] =
                        boxCalcLastDeliveryDate.Style[HtmlTextWriterStyle.TextAlign] = boxCalcLastDlvSettltDate.Style[HtmlTextWriterStyle.TextAlign] =
                        boxCalcFirstNoticeDay.Style[HtmlTextWriterStyle.TextAlign] = boxCalcLastNoticeDay.Style[HtmlTextWriterStyle.TextAlign] = "center";

                        boxMonth.Style[HtmlTextWriterStyle.FontWeight] = "bold";
                        boxMonth.Width = Unit.Percentage(98);

                        /* Width = 96% */
                        boxCalcMaturityDate.Width = boxCalcLastTradingDate.Width =
                            boxCalcDeliveryDate.Width =
                            boxCalcFirstDeliveryDate.Width = boxCalcFirstDlvSettltDate.Width =
                            boxCalcLastDeliveryDate.Width = boxCalcLastDlvSettltDate.Width =
                            boxCalcFirstNoticeDay.Width = boxCalcLastNoticeDay.Width = Unit.Percentage(96);

                        /* BackColor = LightGray */
                        boxMonth.BackColor = boxCalcMaturityDate.BackColor = boxCalcLastTradingDate.BackColor =
                            boxCalcDeliveryDate.BackColor =
                            boxCalcFirstDeliveryDate.BackColor = boxCalcFirstDlvSettltDate.BackColor =
                            boxCalcLastDeliveryDate.BackColor = boxCalcLastDlvSettltDate.BackColor =
                            boxCalcFirstNoticeDay.BackColor = boxCalcLastNoticeDay.BackColor = System.Drawing.Color.FromArgb(229, 229, 229);

                        CalculatedMaturities.Controls.Add(row); //Ajout de la ligne dans la table
                        #endregion

                        /* Calcul des Maturities */
                        CalculateMaturities(txt_NewMaturityMonthYear.Text + monthNumber, boxMonth, boxCalcMaturityDate, boxCalcLastTradingDate,
                                            boxCalcDeliveryDate,
                                            boxCalcFirstDeliveryDate, boxCalcFirstDlvSettltDate, boxCalcLastDeliveryDate, boxCalcLastDlvSettltDate, 
                                            boxCalcFirstNoticeDay, boxCalcLastNoticeDay);
                    }
                }
                else if (rgYYYYMM.IsMatch(txt_NewMaturityMonthYear.Text)) //Format YYYYMM (de 190001 à 299912)
                {
                    /* A condition que le mois (MM) demandé soit dans la Frequency de la MR */
                    if (StrFunc.GetMaturityLetter(txt_NewMaturityMonthYear.Text.Substring(4)).IndexOfAny(txt_Frequency.Text.ToCharArray()) != -1)
                    {
                        Month.Visible = true;
                        txt_NewMaturityMonthYear.BorderColor = txt_Frequency.BorderColor = System.Drawing.Color.White;
                        CalculateMaturities(txt_NewMaturityMonthYear.Text, txt_Month, txt_CalcMaturityDate, txt_CalcLastTradingDate,
                                            txt_CalcDeliveryDate,
                                            txt_CalcFirstDeliveryDate, txt_CalcFirstDlvSettltDate, txt_CalcLastDeliveryDate, txt_CalcLastDlvSettltDate, 
                                            txt_CalcFirstNoticeDay, txt_CalcLastNoticeDay);
                    }
                    else
                    {
                        txt_NewMaturityMonthYear.BorderColor = txt_Frequency.BorderColor = System.Drawing.Color.Red;
                        txt_NewMaturityMonthYear.Text = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US").DateTimeFormat.GetMonthName(Convert.ToInt32(txt_NewMaturityMonthYear.Text.Substring(4))) +
                                                        " (" + StrFunc.GetMaturityLetter(txt_NewMaturityMonthYear.Text.Substring(4)) + ") " +
                                                        "is not in the frequency of the maturity rule.";
                    }
                }
                else
                {
                    CalculatedMaturities.Visible = false;
                    txt_NewMaturityMonthYear.Text = "Invalid date format (supported formats: YYYY or YYYYmm)";
                    txt_NewMaturityMonthYear.BorderColor = System.Drawing.Color.Red;
                }
            }
        }

        /// <summary>
        /// Calcule des Maturities
        /// </summary>
        protected void CalculateMaturities(string pMaturityMonthYear, TextBox calcMonth, TextBox calcMaturityDate, TextBox calcLastTradingDate,
            TextBox calcDeliveryDate, TextBox calcFirstDeliveryDate, TextBox calcFirstDlvSettltDate, TextBox calcLastDeliveryDate, TextBox calcLastDlvSettltDate, 
            TextBox calcFirstNoticeDay, TextBox calcLastNoticeDay)
        {
            txt_NewMaturityMonthYear.BorderColor = txt_Frequency.BorderColor = System.Drawing.Color.White;
            try
            {
                IProductBase product = Tools.GetNewProductBase();

                SQL_Market sqlMarket = new SQL_Market(SessionTools.CS, Convert.ToInt32(ddl_Market.SelectedValue));
                sqlMarket.LoadTable();

                //PL 20131112 [TRIM 19164]
                //NB: Ici on ne peut présumer de la MR à utiliser en cas d'existence de plusieurs MR
                SQL_MaturityRuleActive sql_MaturityRule = new SQL_MaturityRuleActive(SessionTools.CS, ddl_IdMaturityRule.SelectedItem.ToString(), DateTime.MinValue);

                //PL 20131107 Bidouille pour FL
                //Afin de fonctionner dans le cas d'échéances mensuelles mais de format YYYYMMDD, on override ici le paramétrage de la MR pour considérer YYYYMM
                MaturityRule maturityRule = new MaturityRule(sql_MaturityRule);
                if (maturityRule.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay)
                    maturityRule.MaturityFormatEnum = Cst.MaturityMonthYearFmtEnum.YearMonthOnly;

                CalcMaturityRuleDate calc = new CalcMaturityRuleDate(SessionTools.CS, product, (sqlMarket.Id, sqlMarket.IdBC), maturityRule);

                /* Calcul de la MATURITYDATE */
                (DateTime MaturityDateSys, DateTime MaturityDate) maturity = calc.Calc_MaturityDate(pMaturityMonthYear, out DateTime dtRolledDate);

                /* Calcul de la DELIVERYDATE ou des FIRST/LAST DELIVERYDATE */
                DateTime dtDeliveryDate = DateTime.MinValue;
                MaturityPeriodicDeliveryCharacteristics mpdc = new MaturityPeriodicDeliveryCharacteristics();
                if (DtFunc.IsDateTimeFilled(maturity.MaturityDateSys))
                {
                    /* Calcul de la DELIVERYDATE */
                    if (sql_MaturityRule.IsNoPeriodicDelivery)
                    {
                        dtDeliveryDate = calc.Calc_MaturityDeliveryDate(maturity);
                    }
                    /* Calcul des FIRST/LAST DELIVERYDATE */
                    else if (sql_MaturityRule.IsPeriodicDelivery)
                    {
                        mpdc = calc.Calc_MaturityPeriodicDeliveryDates(pMaturityMonthYear);
                    }
                }

                /* Calcul de la LASTTRADINGDATE */
                DateTime dtLastTradingDate = DateTime.MinValue;
                if (DtFunc.IsDateTimeFilled(maturity.MaturityDateSys))
                {
                    DateTime lastTradingDate = calc.Calc_MaturityLastTradingDay(maturity.MaturityDateSys, dtRolledDate);
                    if (DtFunc.IsDateTimeEmpty(dtLastTradingDate) && DtFunc.IsDateTimeFilled(mpdc.dates.dtFirstDlvSettlt))
                    {
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        //WARNING: Afin de répondre au besoin des ETD sur ENERGY (ex. PEGAS)             PL 20170221
                        //         en attendant une évolution du référentiel MR.
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        dtLastTradingDate = mpdc.dates.dtFirstDlvSettlt;
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    }
                }
                                
                /* Affichage du résultat */
                CalculatedMaturities.Visible = true;
                calcMonth.Text = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US").DateTimeFormat.GetMonthName(Convert.ToInt32(pMaturityMonthYear.Substring(4,2)));
                calcMaturityDate.Text = maturity.MaturityDate.DayOfWeek.ToString() + ' ' + maturity.MaturityDate.Date.ToString(currentDateFormat);
                if (maturity.MaturityDate != maturity.MaturityDateSys)
                {
                    calcMaturityDate.ForeColor = System.Drawing.Color.Red;
                    calcMaturityDate.Text += " !";
                    calcMaturityDate.ToolTip = "Real: " + maturity.MaturityDateSys.DayOfWeek.ToString() + " " + maturity.MaturityDateSys.Date.ToString(currentDateFormat);
                }
                calcLastTradingDate.Text = dtLastTradingDate.DayOfWeek.ToString() + ' ' + dtLastTradingDate.Date.ToString(currentDateFormat);
                calcDeliveryDate.Text = DtFunc.IsDateTimeEmpty(dtDeliveryDate) ? "n/a" : dtDeliveryDate.DayOfWeek.ToString() + ' ' + dtDeliveryDate.Date.ToString(currentDateFormat);
                mpdc.dates.dtFormat = currentDateFormat;
                calcFirstDeliveryDate.Text = mpdc.dates.FirstDelivery; 
                calcFirstDlvSettltDate.Text = mpdc.dates.FirstDlvSettlt;
                calcLastDeliveryDate.Text = mpdc.dates.LastDelivery; 
                calcLastDlvSettltDate.Text = mpdc.dates.LastDlvSettlt; 
                calcFirstNoticeDay.Text = "n/a";    //Pas encore dispo
                calcLastNoticeDay.Text = "n/a";     //Pas encore dispo 
            }
            catch
            {
                /* Maturities Incalculabes */
                CalculatedMaturities.Visible = false;
                txt_NewMaturityMonthYear.Text = "An error has occured during the calculation of maturities.";
                txt_NewMaturityMonthYear.BorderColor = System.Drawing.Color.Red;
            }
        }
    }

    /// <summary>
    /// Class contenant des informations sur un Deriv. Contract
    /// </summary>
    public class DCDatas
    {
        private uint m_ID;
        private string m_Identifier;
        private uint m_IDM;
        private uint? m_IDMaturityRule;

        public uint ID { get { return m_ID; } set { m_ID = value; } }
        public string Identifier { get { return m_Identifier; } set { m_Identifier = value; } }
        public uint IDM { get { return m_IDM; } set { m_IDM = value; } }
        public uint? IDMaturityRule { get { return m_IDMaturityRule; } set { m_IDMaturityRule = value; } }
    }


    /// <summary>
    /// Class contenant des informations sur une Maturity Rule
    /// </summary>
    public class MRDatas
    {
        private uint m_ID;
        private string m_Identifier;
        private string m_Frequency;

        public uint ID { get { return m_ID; } set { m_ID = value; } }
        public string Identifier { get { return m_Identifier; } set { m_Identifier = value; } }
        public string Frequency { get { return m_Frequency; } set { m_Frequency = value; } }
    }
}