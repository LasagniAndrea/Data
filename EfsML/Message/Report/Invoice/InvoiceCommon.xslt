<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--====================================================================================================================================-->
  <!--                                                                                                                                    -->
  <!--                                                   VARIABLES                                                                        -->
  <!--                                                                                                                                    -->
  <!--====================================================================================================================================-->
  <!--! ================================================================================ -->
  <!--!                      PRODUCT                                                     -->
  <!--! ================================================================================ -->
  <!-- Get product node value (invoice/additionalinvoice ot creditNote -->
  <xsl:variable name="varIsCreditNote">
    <xsl:call-template name="isCreditNote" />
  </xsl:variable>
  <xsl:variable name="varIsAdditionalInvoice">
    <xsl:call-template name="isAdditionalInvoice" />
  </xsl:variable>
  <xsl:variable name="varIsAmendedInvoice">
    <xsl:call-template name="isAmendedInvoice" />
  </xsl:variable>
  <xsl:variable name="varIsInvoice">
    <xsl:choose>
      <xsl:when test="$varIsAmendedInvoice = 'true'">
        <xsl:value-of select="false()" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="true()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="varProduct">
    <xsl:call-template name="GetProduct" />
  </xsl:variable>

  <!-- Details -->
  <xsl:variable name="varDetails">
    <xsl:copy-of select="//details/*" />
  </xsl:variable>

  <!--<xsl:variable name="gDataDocumentParties" select="//dataDocument/party"/>
  <xsl:variable name="gDataDocumentRepository" select="//dataDocument/repository"/>-->
  <!--<xsl:variable name="gSortingKeys" select="//invoiceTradeSorting/keys/key"/>-->


  <xsl:variable name="varSiReceiver">
    <xsl:copy-of select="//EventSi[PAYER_RECEIVER='Receiver']/*" />
  </xsl:variable>

  <xsl:variable name="tradeIdKeyName">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="'invoiceTradeIdKey'" />
    </xsl:if>
    <xsl:if test="$varIsCreditNote = 'true'">
      <xsl:value-of select="'creditNoteTradeIdKey'" />
    </xsl:if>
    <xsl:if test="$varIsAdditionalInvoice = 'true'">
      <xsl:value-of select="'additionalInvoiceTradeidKey'" />
    </xsl:if>
  </xsl:variable>
  <!--! ================================================================================ -->
  <!--!                      VARIABLES FOR SUMMARY FEES PAGE (A4 VERTICAL)               -->
  <!--! ================================================================================ -->
  <!-- A4 vertical page caracteristics -->
  <xsl:variable name="varPageA4VerticalPageHeight">29.7</xsl:variable>
  <xsl:variable name="varPageA4VerticalPageWidth">21</xsl:variable>
  <xsl:variable name="varPageA4VerticalMargin">0.5</xsl:variable>
  <!-- A4 vertical page body -->
  <xsl:variable name="varPageA4VerticalBodyMarginLeft">2</xsl:variable>
  <xsl:variable name="varPageA4VerticalBodyMarginBottom">2.1</xsl:variable>
  <xsl:variable name="varPageA4VerticalBodyMarginTop">2.8</xsl:variable>
  <!-- ====================================== -->
  <!--           Summary page header          -->
  <!-- ====================================== -->
  <!-- A4 vertical page header extent -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalHeaderExtent">28</xsl:variable>
  <!-- A4 vertical page header width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalHeaderWidth">200</xsl:variable>
  <!-- ====================================== -->
  <!--       Summary page header: logo        -->
  <!-- ====================================== -->
  <!-- A4 vertical page header logo width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalHeaderLogoWidth">30</xsl:variable>
  <!-- A4 vertical page: logo height -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalLogoHeight">15</xsl:variable>
  <!-- ================================================================== -->
  <!--    Summary page header: invoice number and period (if displayed)   -->
  <!-- ================================================================== -->
  <!-- A4 vertical page invoice text width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalInvoiceWidth">30</xsl:variable>
  <!-- A4 vertical page: invoice text font size -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalInvoiceFontSize">6pt</xsl:variable>
  <!-- A4 vertical page: invoice text margin right -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalInvoiceMarginRight">5mm</xsl:variable>
  <!-- A4 vertical page: invoice text padding top -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalInvoicePaddingTop">8mm</xsl:variable>
  <!-- Amended A4 vertical page: invoice text margin right -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageAmendedA4VerticalInvoiceMarginRight">1mm</xsl:variable>
  <!-- ====================================== -->
  <!-- Summary page header: entity's address  -->
  <!-- ====================================== -->
  <!-- A4 vertical page: address font size -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalAddressFontSize">7pt</xsl:variable>
  <!-- A4 vertical page: address font size -->
  <xsl:variable name="varPageHeaderEntityAdressColor">gray</xsl:variable>
  <!--! ================================================================================ -->
  <!--!                      VARIABLES FOR AMENDED FEES PAGE (A4 VERTICAL)               -->
  <!--! ================================================================================ -->
  <!-- A4 vertical page caracteristics -->
  <xsl:variable name="varPageAmendedA4VerticalPageHeight">29.7</xsl:variable>
  <xsl:variable name="varPageAmendedA4VerticalPageWidth">21</xsl:variable>
  <xsl:variable name="varPageAmendedA4VerticalMargin">0.5</xsl:variable>
  <!-- Amended A4 vertical page body -->
  <xsl:variable name="varPageAmendedA4VerticalBodyMarginLeft">1</xsl:variable>
  <xsl:variable name="varPageAmendedA4VerticalBodyMarginBottom">2.1</xsl:variable>
  <xsl:variable name="varPageAmendedA4VerticalBodyMarginTop">2.8</xsl:variable>
  <!-- ====================================== -->
  <!-- Summary page body: receiver address    -->
  <!-- ====================================== -->
  <xsl:variable name="varReceiverAdressWidth">18</xsl:variable>
  <xsl:variable name="varReceiverAddressPaddingTop">1</xsl:variable>
  <xsl:variable name="varReceiverAdressLeftMargin">10</xsl:variable>
  <xsl:variable name="varReceiverAdressFontSize">11</xsl:variable>
  <xsl:variable name="varReceiverAdressTextAlign">left</xsl:variable>
  <!-- ====================================== -->
  <!-- Summary page body: city and date       -->
  <!-- ====================================== -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDateTextPaddingTop">5mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDateMarginLeft">100mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDateFontSize">11pt</xsl:variable>
  <xsl:variable name="varDateTextAlign">left</xsl:variable>
  <!-- ========================================== -->
  <!-- Summary page body: invoice number and text -->
  <!-- ========================================== -->
  <xsl:variable name="varSummaryPageInvoicePaddingTop">4</xsl:variable>
  <xsl:variable name="varSummaryPageInvoiceTextAlign">left</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varSummaryPageInvoiceFontSize">12pt</xsl:variable>
  <!-- ========================================= -->
  <!--    Summary page body: invoice summary     -->
  <!-- ========================================= -->
  <xsl:variable name="varInvoiceSummaryPaddingTop">4</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryLeftMargin">1mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryRightMargin">1mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryFontSize">11pt</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryTextWidth">117</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryAmountWidth">35</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varInvoiceSummaryCurrencyWidth">15</xsl:variable>
  <xsl:variable name="varTextAlign">left</xsl:variable>
  <xsl:variable name="varAmountTextAlign">right</xsl:variable>
  <xsl:variable name="varCurrencyTextAlign">left</xsl:variable>
  <xsl:variable name="varCurrencyLeftPadding">0.06</xsl:variable>
  <!-- ========================================= -->
  <!--    Summary page body: reglement text      -->
  <!-- ========================================= -->
  <xsl:variable name="varReglementTextPaddingTop">1</xsl:variable>
  <xsl:variable name="varReglementTextLeftMargin">0</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varReglementTextFontSize">11pt</xsl:variable>
  <!-- ========================================= -->
  <!--    Summary page body: bank Conditions     -->
  <!-- ========================================= -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varBankAccountPaddingTop">1mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varBankAccountFontSize">11pt</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varBankAccountTextCellWidth">55</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varBankAccountDataCellWidth">105</xsl:variable>
  <xsl:variable name="varBankAccountDataTextAlign">left</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varBankAccountTextLeftMargin">0mm</xsl:variable>
  <!-- ====================================== -->
  <!--       A4 Vertical PAGE FOOTER         -->
  <!-- ====================================== -->
  <!-- A4 Vertical page footer -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalFooterExtent">5</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalPageFooterWidth">200</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalPageFooterTextWidth">180</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4VerticalPageFooterTextFontSize">5pt</xsl:variable>
  <xsl:variable name="varPageA4VerticalPageFooterTextAlign">left</xsl:variable>
  <!-- A4 Vertical page: address -->
  <xsl:variable name="varPageA4VerticalPageFooterAddressFontSize">7</xsl:variable>
  <xsl:variable name="varPageA4VerticalPageFooterAddressColor">gray</xsl:variable>
  <!--! =================================================================================== -->
  <!--!                      VARIABLES FOR DETAILED FEES PAGE (A4 LANDSCAPE)                -->
  <!--! =================================================================================== -->
  <!-- ========================================= -->
  <!--    Detailed fees page carateristics       -->
  <!-- ========================================= -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapePageHeight">210</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapePageWidth">297</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeMargin">5</xsl:variable>
  <xsl:variable name="varPageA4LandscapeBodyMarginLeft">0</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeBodyMarginBottom">10</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeBodyMarginTop">23</xsl:variable>
  <!-- ====================================== -->
  <!--           DETAILED FEES HEADER         -->
  <!-- ====================================== -->
  <!-- A4 landscape page: header extent -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeHeaderExtent">20</xsl:variable>
  <!-- A4 landscape page: header width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeHeaderWidth">287</xsl:variable>
  <!-- ====================================== -->
  <!--       Detailed fees header logo        -->
  <!-- ====================================== -->
  <!-- A4 landscape page: logo width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeHeaderLogoWidth">40</xsl:variable>
  <!-- A4 landscape page: header logo height -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeLogoHeight">15mm</xsl:variable>
  <!-- A4 landscape page: invoice width -->
  <!-- ====================================== -->
  <!--     Detailed fees header invoice text  -->
  <!-- ====================================== -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeInvoiceWidth">60</xsl:variable>
  <!-- A4 landscape page: invoice text font size -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeInvoiceFontSize">8pt</xsl:variable>
  <!-- A4 landscape page: invoice text margin right -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeInvoiceMarginRight">0mm</xsl:variable>
  <!-- A4 landscape page: invoice text padding top -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeInvoicePaddingTop">7mm</xsl:variable>
  <!--! =================================== -->
  <!--!         DETAILED FEES BODY          -->
  <!--! =================================== -->
  <!--! ====================== -->
  <!-- Table for detailed fees -->
  <!--! ====================== -->
  <!-- Padding for the cells in pt -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetailedFeesCellPadding">0.5mm</xsl:variable>
  <!-- Text font size -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesFontSize">7pt</xsl:variable>
  <!-- Bord of the cells -->
  <xsl:variable name="varDetFeesTableBorder">0.3pt solid black</xsl:variable>
  <!-- Columns of the table -->
  <!-- Trade identifier -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol1Width">12</xsl:variable>
  <!-- Trade date -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol2Width">15</xsl:variable>
  <!-- Expiry date -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol3Width">15</xsl:variable>
  <!-- Value date -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol4Width">15</xsl:variable>
  <!-- Trade currency -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol5Width">8</xsl:variable>
  <!-- Rate -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol6Width">16</xsl:variable>
  <!-- Buyer seller -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol7Width">5</xsl:variable>
  <!-- Trader -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol8Width">20</xsl:variable>
  <!-- Counterparty -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol9Width">45</xsl:variable>
  <!-- Duration -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol10Width">14</xsl:variable>
  <!-- ISIN code -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol11Width">21</xsl:variable>
  <!-- Fee schedule -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol12Width">24</xsl:variable>
  <!-- Fee basis -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol13Width">22</xsl:variable>
  <!-- Nominal -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol14Width">22</xsl:variable>
  <!-- Amount -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol15Width">15</xsl:variable>
  <!-- Countervalue -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableCol16Width">15</xsl:variable>
  <!-- Table width -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableTotalWidth">285mm</xsl:variable>
  <!-- Table width Credit Note/Additional Invoice-->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableTotalWidthXCreditNote">180mm</xsl:variable>
  <!-- ============================== -->
  <!-- Table header for detailed fees -->
  <!-- ============================== -->
  <xsl:variable name="varDetFeesTableHeaderBackgroundColor">#e3e3e3</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderInstrumentPadding">1mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderInstrumentFontSize">7pt</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderInstrumentHeigth">8mm</xsl:variable>
  <xsl:variable name="varDetFeesTableHeaderInstrumentTextAlign">left</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderSpaceHeight">1mm</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderTextFontSize">7pt</xsl:variable>
  <xsl:variable name="varDetFeesTableHeaderCellBorder">0.3pt solid black</xsl:variable>
  <xsl:variable name="varDetFeesTableHeaderTextAlign">center</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varDetFeesTableHeaderTextPaddingTop">1mm</xsl:variable>
  <!--! =================================== -->
  <!--!       DETAILED FEES PAGE FOOTER     -->
  <!--! =================================== -->
  <!-- A4 landscape footer -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapePageFooterWidth">287</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapePageFooterTextWidth">267</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapePageFooterTextFontSize">5pt</xsl:variable>
  <xsl:variable name="varPageA4LandscapePageFooterTextAlign">left</xsl:variable>
  <!-- A4 landscape footer: address font size -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="varPageA4LandscapeAddressFontSize">8pt</xsl:variable>
  <xsl:variable name="varPageA4LandscapePageFooterAddressColor">gray</xsl:variable>
  <!--! ================================================================================ -->
  <!--!                   END VARIABLES FOR SUMMARY FEES PAGE (A4 VERTICAL)              -->
  <!--! ================================================================================ -->
  <!--! ================================================================================ -->
  <!--!                             TECHNICAL VARIABLES                                  -->
  <!--! ================================================================================ -->
  <!-- Carriage return -->
  <xsl:variable name="varLinefeed"> </xsl:variable>
  <!-- Espace character -->
  <xsl:variable name="varEspace"> </xsl:variable>
  <!--! ================================================================================ -->
  <!--!                              VARIABLES DATA FROM XML FLUX                        -->
  <!--! ================================================================================ -->
  <!-- Get logo of the entity -->
  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select IDENTIFIER, LOLOGO from dbo.ACTOR where IDA=', //header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid'], ')')" />
  </xsl:variable>
  <!--! ================================================================================ -->
  <!--!    BEGIN REGION Variables for invoice amounts and currencies from XML            -->
  <!--! ================================================================================ -->
  <!-- GrossTurnOverAmount -->
  <xsl:variable name="varGrossTurnOverAmount">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/grossTurnOverAmount/amount" />
    </xsl:if>
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varGrossTurnOverCurrency">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/grossTurnOverAmount/currency" />
    </xsl:if>
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- RebateAmount -->
  <xsl:variable name="varRebateAmount">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/rebateAmount/amount" />
    </xsl:if>
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/rebateAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varRebateCurrency">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/rebateAmount/currency" />
    </xsl:if>
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/rebateAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- NetTurnOverAmount -->
  <xsl:variable name="varNetTurnOverAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varNetTurnOverCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverAmount/currency" />
  </xsl:variable>
  <!-- NetTurnOverIssueAmount -->
  <xsl:variable name="varNetTurnOverIssueAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverIssueAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varNetTurnOverIssueCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverIssueAmount/currency" />
  </xsl:variable>
  <!-- NetTurnOverAccountingAmount -->
  <xsl:variable name="varNetTurnOverAccountingAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverAccountingAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varNetTurnOverAccountingCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverAccountingAmount/currency" />
  </xsl:variable>
  <!-- TaxAmount -->
  <xsl:variable name="varTaxAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/amount/amount" />
  </xsl:variable>
  <xsl:variable name="varTaxCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/amount/currency" />
  </xsl:variable>
  <!-- TaxRate -->
  <xsl:variable name="varTaxRate">
    <xsl:call-template name="totalTaxRate">
      <xsl:with-param name="pListe" select="msxsl:node-set($varProduct)/tax/details/taxSource" />
    </xsl:call-template>
  </xsl:variable>
  <!-- TaxIssueAmount -->
  <xsl:variable name="varTaxIssueAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/issueAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varTaxIssueCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/issueAmount/currency" />
  </xsl:variable>
  <!-- TaxAccountingAmount -->
  <xsl:variable name="varTaxAccountingAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/accountingAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varTaxAccountingCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/tax/accountingAmount/currency" />
  </xsl:variable>
  <!-- TheoricGrossTurnOverAmount -->
  <xsl:variable name="varTheoricGrossTurnOverAmount">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varTheoricGrossTurnOverCurrency">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/grossTurnOverAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- TheoricNetTurnOverAmount -->
  <xsl:variable name="varTheoricNetTurnOverAmount">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varTheoricNetTurnOverCurrency">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- TheoricNetTurnOverAmount -->
  <xsl:variable name="varTheoricRebateAmount">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/rebateAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varTheoricRebateCurrency">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/rebateAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- TheoricNetTurnOverIssueAmount -->
  <xsl:variable name="varTheoricNetTurnOverIssueAmount">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverIssueAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varTheoricNetTurnOverIssueCurrency">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverIssueAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- TheoricNetTurnOverAccountingAmount -->
  <xsl:variable name="varTheoricNetTurnOverAccountingAmount">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverAccountingAmount/amount" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="varTheoricNetTurnOverAccountingCurrency">
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/netTurnOverAccountingAmount/currency" />
    </xsl:if>
  </xsl:variable>
  <!-- TheoricTaxAmount -->
  <xsl:variable name="varTheoricTaxAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/amount/amount" />
  </xsl:variable>
  <xsl:variable name="varTheoricTaxCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/amount/currency" />
  </xsl:variable>
  <!-- TheoricTaxIssueAmount -->
  <xsl:variable name="varTheoricTaxIssueAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/issueAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varTheoricTaxIssueCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/issueAmount/currency" />
  </xsl:variable>
  <!-- TheoricTaxAccountingAmount -->
  <xsl:variable name="varTheoricTaxAccountingAmount">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/accountingAmount/amount" />
  </xsl:variable>
  <xsl:variable name="varTheoricTaxAccountingCurrency">
    <xsl:value-of select="msxsl:node-set($varProduct)/theoricInvoiceAmount/tax/accountingAmount/currency" />
  </xsl:variable>
  <!-- Invoice date -->
  <xsl:variable name="varCreationTimestamp">
    <xsl:value-of select="//header/creationTimestamp" />
  </xsl:variable>
  <!-- Date of the invoice -->
  <xsl:variable name="varInvoiceDate">
    <xsl:value-of select="normalize-space(//invoiceDate[@id='invoiceDate'])" />
  </xsl:variable>
  <!-- Phone of the entity -->
  <xsl:variable name="varEntityPhone">
    <xsl:value-of select="normalize-space(//sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber'])" />
  </xsl:variable>
  <!-- Telex of the entity -->
  <xsl:variable name="varEntityTelex">
    <xsl:value-of select="normalize-space(//sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telexNumber'])" />
  </xsl:variable>
  <!-- Mail of the entity -->
  <xsl:variable name="varEntityMail">
    <xsl:value-of select="normalize-space(//sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email'])" />
  </xsl:variable>
  <!-- Identifier of the invoiced Actor -->
  <xsl:variable name="varInvoicedIdentifier">
    <xsl:value-of select="normalize-space(//sendTo/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid'])" />
  </xsl:variable>
  <!-- Intra TaxNumber of the invoiced party -->
  <xsl:variable name="varInvoicedTaxNumber">
    <xsl:value-of select="//dataDocument/party[@OTCmlId=$varInvoicedIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorTaxNumber']" />
  </xsl:variable>
  <!-- Invoice number -->
  <xsl:variable name="varInvoiceNumber">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:value-of select="normalize-space(//tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid'])" />
    </xsl:if>
    <xsl:if test="$varIsAmendedInvoice = 'true'">
      <xsl:value-of select="msxsl:node-set($varProduct)/initialInvoiceAmount/identifier" />
    </xsl:if>
  </xsl:variable>
  <!-- Invoice begin period -->
  <xsl:variable name="varInvoiceBeginPeriod">
    <xsl:value-of select="//dataDocument/trade/tradeHeader/tradeDate[@id='tradeDate']" />
  </xsl:variable>
  <!-- Invoice end period -->
  <xsl:variable name="varInvoiceEndPeriod">
    <xsl:value-of select="msxsl:node-set($varProduct)/paymentDate/adjustedDate" />
  </xsl:variable>
  <xsl:variable name="varEntityIdentifier">
    <xsl:value-of select="//sendBy/@href"/>
    <!--<xsl:value-of select="normalize-space(//sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier'])" />-->
  </xsl:variable>
  <!-- Economic area of the entity -->
  <xsl:variable name="varEntityEconomicArea">
    <xsl:call-template name="getEconomicArea">
      <xsl:with-param name="pEntityIdentifier" select="$varEntityIdentifier" />
    </xsl:call-template>
  </xsl:variable>
  <!-- Capital of the entity -->
  <xsl:variable name="varEntityCapital">
    <xsl:call-template name="format-money2">
      <xsl:with-param name="amount" select="//dataDocument/party[@id=$varEntityIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorCapital']" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="varEntityCapitalCurrency">
    <xsl:value-of select="//dataDocument/party[@id=$varEntityIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorIdcCapital']" />
  </xsl:variable>
  <!-- BusinessNumber of the entity -->
  <xsl:variable name="varEntityBusinessNumber">
    <xsl:value-of select="//dataDocument/party[@id=$varEntityIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorBusinessNumber']" />
  </xsl:variable>
  <!-- Intra TaxNumber of the entity -->
  <xsl:variable name="varEntityTaxNumber">
    <xsl:value-of select="//dataDocument/party[@id=$varEntityIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorTaxNumber']" />
  </xsl:variable>
  <!-- Fax of the entity -->
  <xsl:variable name="varEntityFax">
    <xsl:value-of select="normalize-space(//sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber'])" />
  </xsl:variable>
  <!-- Credit Note/ Additional Invoice number -->
  <xsl:variable name="varCreditNoteNumber">
    <xsl:value-of select="normalize-space(//tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid'])" />
  </xsl:variable>
  <!--! ================================================================================ -->
  <!--!  VARIABLES FOR CUSTOMIZING REPORT (they sould be passed as parameter of the FOP processor) -->
  <!--! ================================================================================ -->
  <xsl:variable name="varShowZeroValue" select="false()" />
  <!-- varShowZeroValue is a flag which allows to format the fees of the invoice 
	     using the trade sorting node (true), otherwise using the trade nodes themselves (false) -->
  <xsl:variable name="varUseTradeSorting" select="true()" />
  <!--! ================================================================================ -->
  <!--!                                  VARIABLES FOR DEBUG                             -->
  <!--! ================================================================================ -->
  <!-- Variable varTableBorderDebug -->
  <!-- Set the variable varTableBorderDebug for displaying tables and cells borders (for debug) -->
  <!-- 0pt solid black: no borders -->
  <!-- 1pt solid black: borders    -->
  <xsl:variable name="varTableBorderDebug">0pt solid black</xsl:variable>

  <!--==========================================================================================-->
  <!-- OTHERS VARIABLES                                                                         -->
  <!--==========================================================================================-->


  <xsl:variable name="IsDerivative">
    <xsl:choose>
      <xsl:when test="msxsl:node-set($varProduct)/invoiceDetails/invoiceTrade/instrument = 'ExchangeTradedFuture'">true</xsl:when>
      <xsl:when test="msxsl:node-set($varProduct)/invoiceDetails/invoiceTrade/instrument = 'ExchangeTradedOption'">true</xsl:when>
      <xsl:when test="normalize-space(//details/tradeDetail/product/strategy)">true</xsl:when>
      <xsl:otherwise>false</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gIsCnfWithTypeOrdColumn">
    <xsl:value-of select="$IsDerivative"/>
  </xsl:variable>

  <xsl:variable name="varPageA4LandscapeFooterExtent">
    <xsl:if test="$gIsCnfWithTypeOrdColumn = true()">0.8</xsl:if>
    <xsl:if test="$gIsCnfWithTypeOrdColumn = false()">0.5</xsl:if>
  </xsl:variable>




  <!--====================================================================================================================================-->
  <!--                                                                                                                                    -->
  <!--                                                   TEMPLATES                                                                        -->
  <!--                                                                                                                                    -->
  <!--====================================================================================================================================-->

  <!--==========================================================================================-->
  <!-- TEMPLATE : displayPageHeader                                                             -->
  <!--==========================================================================================-->
  <xsl:template name="displayPageHeader">
    <xsl:param name="pIsSummaryPage" />
    <xsl:param name="pLogo" />
    <xsl:param name="pIsDisplayInvoice" />
    <xsl:param name="pCreditNoteVisualization" />
    <xsl:variable name="varPageHeaderWidth">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalHeaderWidth" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeHeaderWidth" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderLogoWidth">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalHeaderLogoWidth" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeHeaderLogoWidth" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderInvoiceTextWidth">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalInvoiceWidth" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeInvoiceWidth" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderClientTextWidth">5</xsl:variable>
    <xsl:variable name="varPageHeaderEntityAdressWidth">
      <xsl:value-of select="concat(number ($varPageHeaderWidth) - number ($varPageHeaderLogoWidth) - number ($varPageHeaderInvoiceTextWidth),'mm')" />
    </xsl:variable>
    <xsl:variable name="varPageHeaderLogoHeight">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalLogoHeight" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeLogoHeight" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderTextFontSize">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalInvoiceFontSize" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeInvoiceFontSize" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderTextMarginRight">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:if test="$pCreditNoteVisualization='true'">
            <xsl:value-of select="$varPageAmendedA4VerticalInvoiceMarginRight" />
          </xsl:if>
          <xsl:if test="$pCreditNoteVisualization='false'">
            <xsl:value-of select="$varPageA4VerticalInvoiceMarginRight" />
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeInvoiceMarginRight" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderTextMarginLeft">
      <xsl:value-of select="$varPageA4LandscapeInvoiceMarginRight" />
    </xsl:variable>
    <xsl:variable name="varPageHeaderTextPaddingTop">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalInvoicePaddingTop" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeInvoicePaddingTop" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageHeaderEntityAdressFontSize">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalAddressFontSize" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapeAddressFontSize" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Display logo and invoice period -->
    <fo:block linefeed-treatment="preserve">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table border-bottom-style="solid" border="0.5pt" width="{$varPageHeaderWidth}mm" table-layout="fixed">
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-column border="{$varTableBorderDebug}" column-width="{$varPageHeaderLogoWidth}mm" column-number="01" />
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-column border="{$varTableBorderDebug}" column-width="{$varPageHeaderEntityAdressWidth}" column-number="02" />
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-column border="{$varTableBorderDebug}" column-width="{$varPageHeaderInvoiceTextWidth}mm" column-number="03" />
        <fo:table-body>
          <fo:table-row>
            <!-- Logo -->
            <fo:table-cell border="{$varTableBorderDebug}" text-align="left">
              <fo:block>
                <!-- EG 20160404 Migration vs2013 -->
                <fo:external-graphic src="{$varImgLogo}" height="{$varPageHeaderLogoHeight}" />
              </fo:block>
            </fo:table-cell>
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-size="{$varPageHeaderEntityAdressFontSize}" border="{$varTableBorderDebug}" text-align="center">
              <!-- EG 20160404 Migration vs2013 -->
              <fo:block padding-top="{$varPageHeaderTextPaddingTop}" color="{$varPageHeaderEntityAdressColor}">
                <fo:table>
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-column border="{$varTableBorderDebug}" column-width="{$varPageHeaderEntityAdressWidth}" column-number="01" />
                  <fo:table-body>
                    <!-- Entity's adress -->
                    <fo:table-row>
                      <fo:table-cell border="{$varTableBorderDebug}" text-align="center">
                        <fo:block>
                          <xsl:for-each select="//sendBy/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine">
                            <xsl:value-of select="." />
                            <xsl:value-of select="$varEspace" />
                          </xsl:for-each>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <!-- Phone / Telex / Fax / Mail -->
                    <fo:table-row>
                      <fo:table-cell border="{$varTableBorderDebug}" text-align="center">
                        <fo:block>
                          <!-- Phone -->
                          <xsl:if test="string-length($varEntityPhone) &gt; 0">
                            <xsl:value-of select="$varEspace" />
                            <xsl:call-template name="getSpheresTranslation">
                              <xsl:with-param name="pResourceName" select="'INV-Phone'" />
                            </xsl:call-template> :
                            <xsl:value-of select="$varEspace" /><xsl:value-of select="$varEntityPhone" />
                          </xsl:if>
                          <!-- Telex -->
                          <xsl:if test="string-length($varEntityTelex) &gt; 0">
                            -
                            <xsl:value-of select="$varEspace" />
                            <xsl:call-template name="getSpheresTranslation">
                              <xsl:with-param name="pResourceName" select="'INV-Telex'" />
                            </xsl:call-template>
                            <xsl:value-of select="$varEspace" />
                            <xsl:value-of select="$varEntityTelex" />
                          </xsl:if>
                          <!-- Fax -->
                          <xsl:if test="string-length($varEntityFax) &gt; 0">
                            -
                            <xsl:value-of select="$varEspace" />
                            <xsl:call-template name="getSpheresTranslation">
                              <xsl:with-param name="pResourceName" select="'INV-Fax'" />
                            </xsl:call-template>
                            <xsl:value-of select="$varEspace" />
                            <xsl:value-of select="$varEntityFax" />
                          </xsl:if>
                          <!-- Mail -->
                          <xsl:if test="string-length($varEntityMail) &gt; 0">
                            -
                            <xsl:value-of select="$varEspace" />
                            <xsl:call-template name="getSpheresTranslation">
                              <xsl:with-param name="pResourceName" select="'INV-Mail'" />
                            </xsl:call-template>
                            <xsl:value-of select="$varEspace" />
                            <xsl:value-of select="$varEntityMail" />
                          </xsl:if>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <!-- Static text (at present not available in the xml stream) -->
                    <fo:table-row>
                      <fo:table-cell border="{$varTableBorderDebug}" text-align="center">
                        <fo:block>
                          <!-- Economic area -->
                          <xsl:if test="string-length($varEntityEconomicArea) != 0">
                            <xsl:value-of select="$varEntityEconomicArea" />
                            <xsl:value-of select="$varEspace" />
                            -
                            <xsl:value-of select="$varEspace" />
                          </xsl:if>
                          <!-- Capital -->
                          <xsl:call-template name="getSpheresTranslation">
                            <xsl:with-param name="pResourceName" select="'INV-EntityCapital'" />
                          </xsl:call-template>
                          <!-- <xsl:value-of select="$varEspace"/>-->
                          <xsl:value-of select="$varEntityCapital" />
                          <xsl:value-of select="$varEspace" />
                          <xsl:value-of select="$varEntityCapitalCurrency" />
                          <xsl:value-of select="$varEspace" />
                          - SIRET
                          <!-- Business Number -->
                          <xsl:value-of select="$varEspace" />
                          <xsl:value-of select="$varEntityBusinessNumber" />
                          <xsl:value-of select="$varEspace" />
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <!-- Static text (at present not available in the xml stream) -->
                    <xsl:if test="$varTaxAmount&gt;0">
                      <fo:table-row>
                        <fo:table-cell border="{$varTableBorderDebug}" text-align="center">
                          <fo:block>
                            <!-- Economic area -->
                            <xsl:call-template name="getSpheresTranslation">
                              <xsl:with-param name="pResourceName" select="'INV-EntityTVANumber'" />
                            </xsl:call-template>
                            <xsl:value-of select="$varEspace" />
                            <xsl:value-of select="$varEntityTaxNumber" />
                            <xsl:value-of select="$varEspace" />
                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                    </xsl:if>
                  </fo:table-body>
                </fo:table>
              </fo:block>
            </fo:table-cell>
            <!-- Invoice (ex. Invoice INV-16 from 01/12/2008 to 31/12/2008) only if pIsDisplayInvoicePeriod=1 -->
            <xsl:if test="$pIsDisplayInvoice=1">
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell font-size="{$varPageHeaderTextFontSize}" border="{$varTableBorderDebug}" text-align="right" display-align="before" margin-right="{$varPageHeaderTextMarginRight}">
                <!-- EG 20160404 Migration vs2013 -->
                <fo:block padding-top="{$varPageHeaderTextPaddingTop}">
                  <xsl:call-template name="getHeaderInvoiceNumber">
                    <xsl:with-param name="pCreditNoteVisualization" select="$pCreditNoteVisualization" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:if>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- ======================================================================================== -->
  <!-- TEMPLATE : displayPageFooter                                                             -->
  <!-- ======================================================================================== -->
  <xsl:template name="displayPageFooter">
    <xsl:param name="pIsSummaryPage" />
    <xsl:variable name="varPageFooterWidth">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalPageFooterWidth" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapePageFooterWidth" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageFooterTextWidth">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalPageFooterTextWidth" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapePageFooterTextWidth" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageFooterTextFontSize">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalPageFooterTextFontSize" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapePageFooterTextFontSize" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageFooterTextAlign">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalPageFooterTextAlign" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapePageFooterTextAlign" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varPageFooterNumberColumnWidth">
      <xsl:value-of select="number ($varPageFooterWidth) - number ($varPageFooterTextWidth)" />
    </xsl:variable>
    <xsl:variable name="varPageFooterTextColor">
      <xsl:choose>
        <xsl:when test="$pIsSummaryPage=1">
          <xsl:value-of select="$varPageA4VerticalPageFooterAddressColor" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$varPageA4LandscapePageFooterAddressColor" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <!--//-->
    <xsl:if test="$gIsCnfWithTypeOrdColumn and $pIsSummaryPage!=1">

      <!-- EG 20160404 Migration vs2013 -->
      <fo:table border="{$gcTableBorderLegend}" width="{$gDetTrdTableTotalWidth}" font-size="{$gDetTrdDetFontSize}">
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-column column-width="13mm" column-number="01" />
        <fo:table-column column-width="18mm" column-number="02" />
        <fo:table-column column-width="23.5mm" column-number="03" />
        <fo:table-column column-width="26.5mm" column-number="04" />
        <fo:table-column column-width="24.5mm" column-number="05" />
        <fo:table-column column-width="20mm" column-number="06" />
        <fo:table-column column-width="22mm" column-number="07" />
        <fo:table-column column-width="15.5mm" column-number="08" />
        <fo:table-column column-width="23mm" column-number="09" />
        <fo:table-column column-width="14mm" column-number="10" />
        <fo:table-body>
          <fo:table-row font-weight="normal">
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell padding="{$gDetTrdCellPadding}" text-align="justify" display-align="before">
              <fo:block linefeed-treatment="preserve">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'INV-OrderTypeLong'" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'1'" />
              <xsl:with-param name="pExtValue" select="'Market'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'2'" />
              <xsl:with-param name="pExtValue" select="'Limit'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'3'" />
              <xsl:with-param name="pExtValue" select="'Stop'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'4'" />
              <xsl:with-param name="pExtValue" select="'StopLimit'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'5'" />
              <xsl:with-param name="pExtValue" select="'MarketOnClose'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'6'" />
              <xsl:with-param name="pExtValue" select="'WithOrWithout'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'7'" />
              <xsl:with-param name="pExtValue" select="'LimitOrBetter'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'8'" />
              <xsl:with-param name="pExtValue" select="'LimitWithOrWithout'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'9'" />
              <xsl:with-param name="pExtValue" select="'OnBasis'" />
            </xsl:call-template>
          </fo:table-row>
          <fo:table-row font-weight="normal">
            <xsl:call-template name="DisplayTypeOrderLegend"/>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'A'" />
              <xsl:with-param name="pExtValue" select="'OnClose'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'B'" />
              <xsl:with-param name="pExtValue" select="'LimitOnClose'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'C'" />
              <xsl:with-param name="pExtValue" select="'ForexMarket'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'D'" />
              <xsl:with-param name="pExtValue" select="'PreviouslyQuoted'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'E'" />
              <xsl:with-param name="pExtValue" select="'PreviouslyIndicated'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'F'" />
              <xsl:with-param name="pExtValue" select="'ForexLimit'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'G'" />
              <xsl:with-param name="pExtValue" select="'ForexSwap'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'H'" />
              <xsl:with-param name="pExtValue" select="'ForexPreviouslyQuoted'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'I'" />
              <xsl:with-param name="pExtValue" select="'Funari'" />
            </xsl:call-template>
          </fo:table-row>
          <fo:table-row font-weight="normal">
            <xsl:call-template name="DisplayTypeOrderLegend"/>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'J'" />
              <xsl:with-param name="pExtValue" select="'MarketIfTouched'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'K'" />
              <xsl:with-param name="pExtValue" select="'MarketWithLeftOverLimit'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'L'" />
              <xsl:with-param name="pExtValue" select="'PreviousFundValuationPoint'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'M'" />
              <xsl:with-param name="pExtValue" select="'NextFundValuationPoint'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'P'" />
              <xsl:with-param name="pExtValue" select="'Pegged'" />
            </xsl:call-template>
            <xsl:call-template name="DisplayTypeOrderLegend">
              <xsl:with-param name="pValue" select="'Q'" />
              <xsl:with-param name="pExtValue" select="'CounterOrderSelection'" />
            </xsl:call-template>
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell padding="{$gDetTrdCellPadding}" text-align="justify" display-align="before"  number-columns-spanned="3"/>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
      <!--Space-->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table border="{$varTableBorderDebug}" width="{$varPageFooterWidth}mm" font-size="{$gDetTrdDetFontSize}">
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-column column-width="{$varPageFooterWidth}mm" column-number="01" />
        <fo:table-body>
          <fo:table-row font-weight="normal" height="1mm">
            <fo:table-cell text-align="left" display-align="center">
              <fo:block />
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>

    <!-- EG 20160404 Migration vs2013 -->
    <fo:table border-top-style="solid" border="0.5pt" width="{$varPageFooterWidth}mm" table-layout="fixed">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-column column-width="{$varPageFooterTextWidth}mm" column-number="01" />
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-column column-width="{$varPageFooterNumberColumnWidth}mm" column-number="02" />
      <fo:table-body>
        <fo:table-row>
          <fo:table-cell padding="1mm" border="{$varTableBorderDebug}" color="{$varPageFooterTextColor}" text-align="{$varPageFooterTextAlign}" display-align="before">
            <!-- EG 20160404 Migration vs2013 -->
            <fo:block linefeed-treatment="preserve" font-size="{$varPageFooterTextFontSize}">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'INV-PaymentLaw'" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <!-- Page number/Total pages-->
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table-cell padding="1mm" border="{$varTableBorderDebug}" text-align="right" display-align="before" font-size="{$varPageFooterTextFontSize}">
            <fo:block>
              <fo:page-number />/<fo:page-number-citation ref-id="LastPage" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <xsl:template name="DisplayTypeOrderLegend">
    <xsl:param name="pValue" />
    <xsl:param name="pExtValue" />

    <!-- EG 20160404 Migration vs2013 -->
    <fo:table-cell padding="{$gDetTrdCellPadding}" text-align="justify" display-align="before">
      <fo:block linefeed-treatment="preserve">
        <fo:inline font-weight="bold">
          <xsl:value-of select="$pValue" />
        </fo:inline>
        <xsl:value-of select="$gcEspace" />
        <xsl:value-of select="$gcEspace" />
        <xsl:value-of select="$pExtValue" />
      </fo:block>
    </fo:table-cell>
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : setPagesCaracteristics                                                        -->
  <!--            set pages models                                                              -->
  <!--==========================================================================================-->
  <xsl:template name="SetPagesCaracteristicsSpecific">
    <xsl:param name="pHeaderExtent"/>
    <xsl:param name="pBodyMarginTop"/>
    <xsl:param name="pFooterExtent"/>
    <xsl:param name="pBodyMarginBottom"/>

    <fo:simple-page-master master-name="Amended-A4-vertical"
                           page-height="{$gPageA4VerticalPageHeight}"
                           page-width="{$gPageA4VerticalPageWidth}"
                           margin-left="{$gPageA4VerticalMarginLeft}"
                           margin-right="{$gPageA4VerticalMarginRight}"
                           margin-bottom="{$gPageA4VerticalMarginBottom}"
                           margin-top="{$gPageA4VerticalMarginTop}"
                           padding="0"
                           border-width="0mm">

      <fo:region-body region-name="Amended-A4-vertical-body"
                      margin-left="{$gPageA4VerticalBodyMargin}"
                      margin-right="{$gPageA4VerticalBodyMargin}"
                      margin-bottom="{$pBodyMarginBottom}"
                      margin-top="{$pBodyMarginTop}"
                      border="{$gcPageBorderDebug}"
                      padding="0"
                      border-width="0mm"
                      overflow="scroll">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'yellow'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-body>
      <fo:region-before region-name="Amended-A4-vertical-header"
                        extent="{$pHeaderExtent}"
                        precedence="true">
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-before>
      <fo:region-after region-name="Amended-A4-vertical-footer"
                       extent="{$pFooterExtent}"
                       display-align="after"
                       precedence="true"
                       padding="0"
                       border-width="0mm">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-after>
    </fo:simple-page-master>
    <fo:simple-page-master master-name="A4-landscape"
                           page-height="{$gPageA4LandscapePageHeight}"
                           page-width="{$gPageA4LandscapePageWidth}"
                           margin-left="{$gPageA4VerticalMarginLeft}"
                           margin-right="{$gPageA4VerticalMarginRight}"
                           margin-bottom="{$gPageA4VerticalMarginBottom}"
                           margin-top="{$gPageA4VerticalMarginTop}"
                           padding="0"
                           border-width="0mm">

      <fo:region-body region-name="A4-landscape-body"
                      margin-left="{$gPageA4VerticalBodyMargin}"
                      margin-right="{$gPageA4VerticalBodyMargin}"
                      margin-bottom="{$pBodyMarginBottom}"
                      margin-top="{$pBodyMarginTop}"
                      border="{$gcPageBorderDebug}"
                      padding="0"
                      border-width="0mm"
                      overflow="scroll">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'yellow'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-body>
      <fo:region-before region-name="A4-landscape-header"
                        extent="{$pHeaderExtent}"
                        precedence="true">
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-before>
      <fo:region-after region-name="A4-landscape-footer"
                       extent="{$pFooterExtent}"
                       display-align="after"
                       precedence="true"
                       padding="0"
                       border-width="0mm">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-after>
    </fo:simple-page-master>
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : getHeaderInvoiceNumber                                                        -->
  <!--            display invoice / credit note / additional invoice number (DETAIL PAGE)       -->
  <!--==========================================================================================-->
  <xsl:template name="getHeaderInvoiceNumber">
    <xsl:param name="pCreditNoteVisualization" />
    <fo:block>
      <!-- Invoice receiver (client) name -->
      <fo:block>
        <fo:inline font-weight="bold">
          <xsl:value-of select="//sendTo/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[position()=1]" />
        </fo:inline>
      </fo:block>
      <!-- Show only if you are building the credit note -->
      <xsl:if test="$pCreditNoteVisualization = 'true' and $varIsAmendedInvoice = 'true'">
        <xsl:if test="$varIsCreditNote = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-CreditNote'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$varIsAdditionalInvoice = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-AdditionalInvoice'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:value-of select="$varEspace" />
        <fo:inline font-weight="bold">
          <xsl:value-of select="$varCreditNoteNumber" />
        </fo:inline>
        <xsl:value-of select="$varEspace" />
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-On'" />
        </xsl:call-template>
        <xsl:value-of select="$varEspace" />
      </xsl:if>
      <!-- Show only if you are building the amended invoice -->
      <xsl:if test="$pCreditNoteVisualization = 'false' and $varIsAmendedInvoice = 'true'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-AmendedInvoice'" />
        </xsl:call-template>
        <xsl:value-of select="$varEspace" />
      </xsl:if>
      <xsl:if test="($pCreditNoteVisualization = 'true' and $varIsAmendedInvoice = 'true') or ($varIsInvoice = 'true')">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-Invoice'" />
        </xsl:call-template>
      </xsl:if>
      <xsl:value-of select="$varEspace" />
      <fo:inline font-weight="bold">
        <xsl:value-of select="$varInvoiceNumber" />
      </fo:inline>
      <fo:block>
        <xsl:call-template name="displayInvoicingPeriod" />
      </fo:block>
    </fo:block>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : getInvoiceNumber                                                              -->
  <!--            display invoice / credit note / additional invoice number (SUMMARY PAGE)      -->
  <!--==========================================================================================-->
  <xsl:template name="getInvoiceNumber">
    <xsl:param name="pIsCreditNoteVisualization" />

    <fo:block border="1pt solid black" font-size="15pt" text-align="center" background-color="#DCDCDC" >
      <!-- Show only if you are building the credit note -->
      <xsl:if test="$pIsCreditNoteVisualization and $varIsAmendedInvoice = 'true'">
        <xsl:if test="$varIsCreditNote = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-CreditNote'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$varIsAdditionalInvoice = 'true'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'INV-AdditionalInvoice'" />
          </xsl:call-template>
        </xsl:if>
        <xsl:value-of select="$varEspace" />
        <fo:inline font-weight="bold">
          <xsl:value-of select="$varCreditNoteNumber" />
        </fo:inline>
        <xsl:value-of select="$varEspace" />
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-On'" />
        </xsl:call-template>
        <xsl:value-of select="$varEspace" />
      </xsl:if>
      <!-- Show only if you are building the amended invoice -->
      <xsl:if test="$pIsCreditNoteVisualization = false() and $varIsAmendedInvoice = 'true'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-AmendedInvoice'" />
        </xsl:call-template>
        <xsl:value-of select="$varEspace" />
      </xsl:if>
      <xsl:if test="($pIsCreditNoteVisualization and $varIsAmendedInvoice = 'true') or ($varIsInvoice = 'true')">
        <xsl:choose>
          <xsl:when test="string-length($gReportHeaderFooter/hTitle/text())>0">
            <xsl:value-of select="$gReportHeaderFooter/hTitle/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Invoice'" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:value-of select="$varEspace" />
      <fo:inline font-weight="bold">
        <xsl:value-of select="$varInvoiceNumber" />
      </fo:inline>
      <xsl:value-of select="$varEspace" />
      <fo:block>
        <xsl:call-template name="displayInvoicingPeriod" />
      </fo:block>
    </fo:block>
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : isCreditNote                                                                  -->
  <!--            the confirmation message is a credit note                                     -->
  <!--==========================================================================================-->
  <xsl:template name="isCreditNote">
    <xsl:choose>
      <xsl:when test="//confirmationMessage/dataDocument/trade/creditNote">
        <xsl:value-of select="true()" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="false()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : isCreditNote                                                                  -->
  <!--            the confirmation message is an additional invoice                             -->
  <!--==========================================================================================-->
  <xsl:template name="isAdditionalInvoice">
    <xsl:choose>
      <xsl:when test="//confirmationMessage/dataDocument/trade/additionalInvoice">
        <xsl:value-of select="true()" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="false()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : isAmendedInvoice                                                              -->
  <!--            the confirmation message isn't an original invoice                            -->
  <!--==========================================================================================-->
  <xsl:template name="isAmendedInvoice">
    <xsl:choose>
      <xsl:when test="//confirmationMessage/dataDocument/trade/invoice">
        <xsl:value-of select="false()" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="true()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- =========================================================================================-->
  <!--  TEMPLATE : GetProduct                                                                   -->
  <!--             Return PRODUCT element                                                       -->
  <!-- =========================================================================================-->
  <xsl:template name="GetProduct">
    <xsl:if test="$varIsInvoice = 'true'">
      <xsl:copy-of select="/confirmationMessage/dataDocument/trade/invoice/*" />
    </xsl:if>
    <xsl:if test="$varIsCreditNote = 'true'">
      <xsl:copy-of select="/confirmationMessage/dataDocument/trade/creditNote/*" />
    </xsl:if>
    <xsl:if test="$varIsAdditionalInvoice = 'true'">
      <xsl:copy-of select="/confirmationMessage/dataDocument/trade/additionalInvoice/*" />
    </xsl:if>
  </xsl:template>

  <!-- =========================================================================================-->
  <!--  TEMPLATE : displayInvoicingPeriod                                                       -->
  <!--             Display Invoice Periods                                                      -->
  <!-- =========================================================================================-->
  <xsl:template name="displayInvoicingPeriod">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'INV-From'" />
    </xsl:call-template>
    <xsl:value-of select="$varEspace" />
    <xsl:call-template name="format-shortdate2">
      <xsl:with-param name="xsd-date-time" select="$varInvoiceBeginPeriod" />
    </xsl:call-template>
    <xsl:value-of select="$varEspace" />
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'INV-To'" />
    </xsl:call-template>
    <xsl:value-of select="$varEspace" />
    <xsl:call-template name="format-shortdate2">
      <xsl:with-param name="xsd-date-time" select="$varInvoiceEndPeriod" />
    </xsl:call-template>
  </xsl:template>

  <!-- =========================================================================================-->
  <!--  TEMPLATE : displayInvoicedTaxNumber                                                     -->
  <!--             Display Payer Tax number                                                     -->
  <!-- =========================================================================================-->
  <xsl:template name="displayInvoicedTaxNumber">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'INV-PayerTVANumber'" />
    </xsl:call-template>
    <xsl:value-of select="$varEspace" />
    <xsl:value-of select="normalize-space($varInvoicedTaxNumber)" />
  </xsl:template>

  <!-- =========================================================================================-->
  <!--  TEMPLATE : totalTaxRate                                                                 -->
  <!--             Display Sum of rate Tax                                                      -->
  <!-- =========================================================================================-->
  <xsl:template name="totalTaxRate">
    <xsl:param name="pListe" />
    <xsl:choose>
      <xsl:when test="$pListe">
        <xsl:variable name="prem" select="$pListe[1]" />
        <xsl:variable name="reste">
          <xsl:call-template name="totalTaxRate">
            <xsl:with-param name="pListe" select="$pListe[position()!=1]" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="$prem//spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate'] * 100 + $reste" />
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : displayDate                                                                   -->
  <!--            entity's town and invoice date                                                -->
  <!--==========================================================================================-->
  <xsl:template name="displayDate">
    <!-- EG 20160404 Migration vs2013 -->
    <fo:block margin-left="{$varDateMarginLeft}" padding-top="{$varDateTextPaddingTop}" font-size="{$varDateFontSize}" text-align="{$varDateTextAlign}">
      <xsl:value-of select="//routingIdsAndExplicitDetails/routingAddress/city" />,<xsl:value-of select="$varEspace" />
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'INV-The'" />
      </xsl:call-template>
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$varCreationTimestamp" />
      </xsl:call-template>
    </fo:block>
  </xsl:template>

  <!-- =========================================================================================-->
  <!-- TEMPLATE : getEconomicArea                                                               -->
  <!--            Show Economic Area from Code                                                  -->
  <!-- =========================================================================================-->
  <xsl:template name="getEconomicArea">
    <xsl:param name="pEntityIdentifier" />
    <!-- Retrieve the code area -->
    <xsl:variable name="varEntityAreaCode">
      <xsl:value-of select="//dataDocument/party[@id=$pEntityIdentifier]/partyId[@partyIdScheme='http://www.euro-finance-systems.fr/otcml/actorEconomicAreaCode']" />
    </xsl:variable>
    <xsl:choose>
      <!-- Retrieve the area by the code area -->
      <xsl:when test="//dataDocument/repository/enums[@id='ENUMS.CODE.EconomicAreaCodeEnum']/enum[@id='ENUM.VALUE.EI']/value = $varEntityAreaCode">
        <xsl:value-of select="//dataDocument/repository/enums[@id='ENUMS.CODE.EconomicAreaCodeEnum']/enum[@id='ENUM.VALUE.EI']/extvalue" />
      </xsl:when>
      <!-- otherwise display the code itself -->
      <xsl:otherwise>
        <xsl:value-of select="$varEntityAreaCode" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- =========================================================================================-->
  <!-- TEMPLATE : GetIsinCode                                                                   -->
  <!--            Get Isin code                                                                 -->
  <!-- =========================================================================================-->
  <xsl:template name="GetIsinCode">
    <xsl:param name="pISINCode" />
    <xsl:choose>
      <!-- no regex in xslt 1.0 -->
      <xsl:when test="$pISINCode = 'EN ATTENTE' or string-length($pISINCode) != 12">
        <xsl:value-of select="''" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pISINCode" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayExchangeRate                                                           -->
  <!--            display issueRate or AccountingRate value                                     -->
  <!--==========================================================================================-->
  <!--RD 20151002 [21426] Use ressource 'Report-SpotRate' instead 'INV-Rate'-->
  <xsl:template name="displayExchangeRate">
    <xsl:param name="prefix" />
    <xsl:variable name="varIsReverse">
      <xsl:if test ="$prefix='issue'">
        <xsl:value-of select="msxsl:node-set($varProduct)/issueRateIsReverse" />
      </xsl:if>
      <xsl:if test ="$prefix='accounting'">
        <xsl:value-of select="msxsl:node-set($varProduct)/accountingRateIsReverse" />
      </xsl:if>
    </xsl:variable>
    (
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'Report-SpotRate'" />
    </xsl:call-template>
    :
    <xsl:choose>
      <xsl:when test ="$varIsReverse='true'">
        <xsl:if test ="$prefix='issue'">
          <xsl:value-of select="msxsl:node-set($varProduct)/issueRateRead" />
        </xsl:if>
        <xsl:if test ="$prefix='accounting'">
          <xsl:value-of select="msxsl:node-set($varProduct)/accountingRateRead" />
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test ="$prefix='issue'">
          <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverIssueRate" />
        </xsl:if>
        <xsl:if test ="$prefix='accounting'">
          <xsl:value-of select="msxsl:node-set($varProduct)/netTurnOverAccountingRate" />
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
    )
  </xsl:template>
</xsl:stylesheet>

