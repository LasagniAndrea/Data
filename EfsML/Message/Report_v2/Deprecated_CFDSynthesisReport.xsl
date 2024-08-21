<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres reports - CFD Synthesis Report                                               -->
  <!-- File    : \Report_v2\CFDSynthesisReport.xsl                                                    -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.5.5491                                                                            -->
  <!-- Date    : 20150116                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->
  <!--<xsl:import href="..\Report\AccountStatementReport.xsl"/>
  <xsl:import href="..\Report\DetailedFinancialReport.xsl"/>
  <xsl:import href="..\Report\FinancialReport.xsl"/>
  <xsl:import href="..\Report\InterestStatementReport.xsl"/>
  <xsl:import href="..\Report\InvoiceReport.xsl"/>
  <xsl:import href="..\Report\PosActionReport.xsl"/>
  <xsl:import href="..\Report\PositionReport.xsl"/>
  <xsl:import href="..\Report\PosSyntheticReport.xsl"/>
  <xsl:import href="..\Report\TransactReport.xsl"/>

  <xsl:import href="..\ISDA\FX\FxBarrierOption_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxBarrierOption_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxBarrierOption_ISDA_Business.xslt"/>

  <xsl:import href="..\ISDA\EQD\EquityOption_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\EQD\EquityOption_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\EQD\EquityOption_ISDA_PDF.xslt"/>

  <xsl:import href="..\ISDA\FX\FxBarrierOption_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxBarrierOption_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxBarrierOption_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxDigitalOption_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxDigitalOption_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxDigitalOption_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxLeg_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxLeg_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxLeg_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxOptionLeg_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxOptionLeg_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxOptionLeg_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxSwap_FULL_PDF.xslt"/>
  <xsl:import href="..\ISDA\FX\FxSwap_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\FX\FxSwap_ISDA_PDF.xslt"/>

  <xsl:import href="..\ISDA\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="..\ISDA\Shared\Shared_ISDA_PDF.xslt"/>
  <xsl:import href="..\ISDA\Shared\Shared_Variables.xslt"/>-->

  <!--<xsl:import href="ETDSynthesisReport.xsl" />-->

  <!--Business-->
  <xsl:import href="Shared\Shared_Report_v2_Business.xslt" />
  <xsl:import href="RTS\RTS_Report_v2_Business.xslt" />
  <!--Report Format-->
  <xsl:import href="Shared\Shared_Report_v2_A4Vertical.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_UK.xslt" />
  <!--Tools-->
  <xsl:import href="Shared\Shared_Report_v2_Tools.xslt" />
  <xsl:import href="RTS\RTS_Report_v2_Tools.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_Variables.xslt" />

  <!--Parameters-->
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <xsl:param name="pCurrentCulture" select="'en-GB'" />

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gReportType" select="$gcReportTypeSYNTHESIS"/>
  <!--To add Spécific Timezone-->
  <!--<xsl:variable name="gHeaderTimezone" select="$gSendBy-routingAddress/city"/>-->

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->
  <!-- .......................................................................... -->
  <!--              Main                                                          -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Main Template                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  First called template                           -->
  <!-- ................................................ -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <!-- .......................................................................... -->
      <!--              Page models                                                   -->
      <!-- .......................................................................... -->
      <fo:layout-master-set>
        <!--Define "A4Vertical" model -->
        <xsl:call-template name="A4Vertical"/>

        <!--Define "A4Vertical-Synthesis" page sequence -->
        <fo:page-sequence-master master-name="A4Vertical-Synthesis">
          <fo:repeatable-page-master-alternatives>
            <fo:conditional-page-master-reference master-reference="A4Vertical"/>
          </fo:repeatable-page-master-alternatives>
        </fo:page-sequence-master>
      </fo:layout-master-set>

      <!-- .......................................................................... -->
      <!--              Page Content                                                   -->
      <!-- .......................................................................... -->
      <fo:page-sequence master-reference="A4Vertical-Synthesis" font-family="{$gFontFamily}">
        <!-- .......................................................................... -->
        <!--              Page Header                                                   -->
        <!-- .......................................................................... -->
        <fo:static-content flow-name="A4VerticalHeader" display-align="before" >
          <xsl:call-template name="Debug_border-red"/>

          <fo:block display-align="before" linefeed-treatment="preserve">
            <xsl:call-template name="A4VerticalHeader"/>
          </fo:block>
        </fo:static-content>
        <!-- .......................................................................... -->
        <!--              Page Footer                                                   -->
        <!-- .......................................................................... -->
        <fo:static-content flow-name="A4VerticalFooter" display-align="after">
          <xsl:call-template name="Debug_border-red"/>

          <fo:block display-align="after" linefeed-treatment="preserve">
            <xsl:call-template name="A4VerticalFooter"/>
          </fo:block>
        </fo:static-content>
        <!-- .......................................................................... -->
        <!--              Page Content                                                  -->
        <!-- .......................................................................... -->
        <fo:flow flow-name="A4VerticalBody">
          <xsl:call-template name="Debug_border-red"/>
          <xsl:call-template name="UKSynthesis_Content"/>
        </fo:flow>
      </fo:page-sequence>
    </fo:root>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              UK Synthesis                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_Content                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis content                       -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_Content">

    <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
    <!--              Business Data                     -->
    <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
    <xsl:variable name="vBizRTSTradesConfirmations_Node">
      <xsl:variable name="vSubTotal" select="$gDailyTrades/subTotal[predicates/predicate/@trdTyp]"/>
      <xsl:call-template name="BizRTS_MarketSubTotal-market">
        <xsl:with-param name="pEfsmlTrade" select="$gDailyTrades/trade[@trdTyp=$vSubTotal/predicates/predicate/@trdTyp]"/>
        <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizRTSAmendmentTransfers_Node">
      <!-- POC, POT et LateTrade -->
      <xsl:call-template name="BizRTS_MarketAmendmentTransfers-market"/>
    </xsl:variable>
    <xsl:variable name="vBizRTSPurchaseSales_Node">
      <!-- UNCLEARING: Compensation générée suite à une décompensation partielle -->
      <!-- CLEARSPEC : Compensation  Spécifique -->
      <!-- CLEARBULK : Compensation globale -->
      <!-- CLEAREOD : Compensation fin de journée -->
      <!-- ENTRY : Clôture STP -->
      <!-- UPDENTRY : Clôture Fin de journée -->
      <!-- Hors Décompensation -->
      <xsl:call-template name="BizRTS_MarketPurchaseSale-market"/>
    </xsl:variable>
    <xsl:variable name="vBizRTSOpenPosition_Node">
      <xsl:call-template name="BizRTS_MarketSubTotal-market">
        <xsl:with-param name="pEfsmlTrade" select="$gPosTrades/trade"/>
        <xsl:with-param name="pSubTotal" select="$gPosTrades/subTotal"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizRTSJournalEntries_Node">
      <xsl:call-template name="Business_JournalEntries-book"/>
    </xsl:variable>
    <xsl:variable name="vBizRTSAccountSummary_Node">
      <xsl:call-template name="Business_AccountSummary-group"/>
    </xsl:variable>
    <xsl:variable name="vBizRTSTradesConfirmations" select="msxsl:node-set($vBizRTSTradesConfirmations_Node)/market"/>
    <xsl:variable name="vBizRTSAmendmentTransfers" select="msxsl:node-set($vBizRTSAmendmentTransfers_Node)/market"/>
    <xsl:variable name="vBizRTSPurchaseSales" select="msxsl:node-set($vBizRTSPurchaseSales_Node)/market"/>
    <xsl:variable name="vBizRTSOpenPosition" select="msxsl:node-set($vBizRTSOpenPosition_Node)/market"/>
    <xsl:variable name="vBizRTSJournalEntries" select="msxsl:node-set($vBizRTSJournalEntries_Node)/book"/>
    <xsl:variable name="vBizRTSAccountSummary" select="msxsl:node-set($vBizRTSAccountSummary_Node)/group"/>

    <xsl:choose>
      <xsl:when test="$gcIsBusinessDebugMode=true()">
        <tradesConfirmations>
          <xsl:copy-of select="$vBizRTSTradesConfirmations"/>
        </tradesConfirmations>
        <amendmentTransfers>
          <xsl:copy-of select="$vBizRTSAmendmentTransfers"/>
        </amendmentTransfers>
        <purchaseSale>
          <xsl:copy-of select="$vBizRTSPurchaseSales"/>
        </purchaseSale>
        <openPosition>
          <xsl:copy-of select="$vBizRTSOpenPosition"/>
        </openPosition>
        <journalEntries>
          <xsl:copy-of select="$vBizRTSJournalEntries"/>
        </journalEntries>
        <accountSummary>
          <xsl:copy-of select="$vBizRTSAccountSummary"/>
        </accountSummary>
      </xsl:when>
      <xsl:otherwise>
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Header                    -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <xsl:call-template name="ReportTitle"/>
        <xsl:call-template name="ReportSendToAddress"/>

        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Details                   -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--Space-->
        <fo:block space-before="{$gDisplaySettings-BusinessDetail/@space-before}"/>

        <fo:table table-layout="fixed"
                  table-omit-header-at-break="false"
                  display-align="center"
                  keep-together.within-page="always">

          <fo:table-column column-number="01">
            <xsl:call-template name="Debug_border-red"/>
          </fo:table-column>
          <fo:table-header>
            <fo:table-row>
              <fo:table-cell>
                <xsl:call-template name="ReportBusinessDetails"/>
                <xsl:call-template name="ReportIntroduction"/>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-header>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell>
                <!--1/ Section: TradesConfirmations / Mandatory -->
                <xsl:call-template name="UKSynthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_TRD"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizRTSTradesConfirmations"/>
                </xsl:call-template>
                <!--2/ Section: AmendmentTransfers / Optional -->
                <xsl:if test="$vBizRTSAmendmentTransfers">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizRTSAmendmentTransfers"/>
                  </xsl:call-template>
                </xsl:if>
                <!--3/ Section: PurchaseSale / Optional -->
                <xsl:if test="$vBizRTSPurchaseSales">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_PSS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizRTSPurchaseSales"/>
                  </xsl:call-template>
                </xsl:if>
                <!--4/ Section: OpenPosition / Mandatory -->
                <xsl:call-template name="UKSynthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizRTSOpenPosition"/>
                </xsl:call-template>
                <!--5/ Section: JournalEntries / Optional -->
                <xsl:if test="$vBizRTSJournalEntries">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_JNL"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizRTSJournalEntries"/>
                  </xsl:call-template>
                </xsl:if>
                <!--6/ Section: AccountSummary / Mandatory / Page break -->
                <fo:block break-after='page'/>
                <xsl:call-template name="UKSynthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ACC"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizRTSAccountSummary"/>
                </xsl:call-template>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>

        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <!--              Content Footer                    -->
        <!-- ++++++++++++++++++++++++++++++++++++++++++++++ -->
        <xsl:call-template name="ReportLastMessage"/>
        <!--Standard Legend-->
        <xsl:call-template name="A4VerticalLegend">
          <xsl:with-param name="pWithOrderType" select="true()"/>
          <xsl:with-param name="pWithTradeType" select="true()"/>
          <xsl:with-param name="pWithPriceType" select="true()"/>
        </xsl:call-template>

        <!--Report Legend-->
        <xsl:if test="$vBizRTSTradesConfirmations or $vBizRTSAmendmentTransfers or $vBizRTSPurchaseSales">
          <fo:block font-size="{$gData_font-size}"
                    padding="{$gBlockSettings_Legend/@padding}"
                    display-align="{$gData_display-align}"
                    keep-together.within-page="always"
                    space-before="{$gLegend_space-before}">

            <!--Legend_FeesExclTax-->
            <xsl:if test="$vBizRTSTradesConfirmations or $vBizRTSAmendmentTransfers">
              <xsl:call-template name="UKDisplay_Legend">
                <xsl:with-param name="pLegend" select="$gcLegend_FeesExclTax"/>
                <xsl:with-param name="pResource" select="'Report-LegendFeesExclTax'"/>
              </xsl:call-template>
            </xsl:if>

            <!--Legend_RealisedPnL-->
            <xsl:if test="$vBizRTSPurchaseSales">
              <xsl:call-template name="UKDisplay_Legend">
                <xsl:with-param name="pLegend" select="$gcLegend_RealisedPnL"/>
                <xsl:with-param name="pResource" select="'Report-Legend_RealisedPnL'"/>
              </xsl:call-template>
            </xsl:if>
          </fo:block>
        </xsl:if>

        <fo:block id="LastPage"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: TRADES CONFIRMATIONS                                 -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_TradesConfirmationsData              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display TradesConfirmations Content             -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_TradesConfirmationsData">
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="11"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='ContractValue']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>
        <xsl:variable name="vBookSubTotal" select="$gDailyTrades/subTotal[predicates/predicate/@trdTyp and @idB=$pBizBook/@OTCmlId and @idI=$pBizBook/asset/@idI]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$gDailyTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                             text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                             padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Efsml">
                    <xsl:with-param name="pDataName" select="'PosResult'"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'BuyQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'SellQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@text-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="UKDisplay_Underlyer">
                    <xsl:with-param name="pRepository-asset" select="$vRepository_Asset"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Underlying']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vEfsmlTrade/fee">
                  <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                  <xsl:for-each select="$vEfsmlTrade/fee">
                    <xsl:sort select="@paymentType"/>
                    <xsl:sort select="@ccy"/>
                    <xsl:sort select="@side=$gcDebit"/>
                    <xsl:sort select="@side=$gcCredit"/>

                    <xsl:if test="position()=1">
                      <xsl:call-template name="UKDisplay_Fee"/>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_Amount">
                <xsl:with-param name="pAmount" select="$vTrade/returnSwap/returnLeg/notional/notionalAmount/amount"/>
                <xsl:with-param name="pCcy" select="$vTrade/returnSwap/returnLeg/notional/notionalAmount/currency"/>
                <xsl:with-param name="pWithSide" select="false()"/>
              </xsl:call-template>
            </fo:table-row>
            <!--Other Fee rows-->
            <xsl:for-each select="$vEfsmlTrade/fee">
              <xsl:sort select="@paymentType"/>
              <xsl:sort select="@ccy"/>
              <xsl:sort select="@side=$gcDebit"/>
              <xsl:sort select="@side=$gcCredit"/>

              <!--Le premier a été déjà affiché sur la première ligne-->
              <xsl:if test="position()>1">
                <fo:table-row font-size="{$gData_font-size}"
                              font-weight="{$gData_font-weight}"
                              text-align="{$gData_text-align}"
                              display-align="{$gData_display-align}">

                  <fo:table-cell number-columns-spanned="11">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <xsl:call-template name="UKDisplay_Fee"/>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vSubTotal" select="$vBookSubTotal[@idAsset=$vBizAsset/@OTCmlId and @assetCategory=$vBizAsset/@assetCategory and @idI=$vBizAsset/@idI][1]"/>
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>

          <!--Subtotal row, with first Fee-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'LongQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'ShortQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="3">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                  <xsl:with-param name="pPricePattern" select="$vBizAsset/pattern/@avgPrice"/>
                  <xsl:with-param name="pIsPos" select="false()"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$vBizSubTotal/fee">
                <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                <xsl:for-each select="$vBizSubTotal/fee">
                  <xsl:sort select="@paymentType"/>
                  <xsl:sort select="@ccy"/>
                  <xsl:sort select="@side=$gcDebit"/>
                  <xsl:sort select="@side=$gcCredit"/>

                  <xsl:if test="position()=1">
                    <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
                  </xsl:if>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:variable name="vTotal_ContractValue" select="sum($vAssetTradeList/returnSwap/returnLeg/notional/notionalAmount/amount)"/>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_ContractValue" />
              <xsl:with-param name="pCcy" select="$vAssetTradeList/returnSwap/returnLeg/notional/notionalAmount/currency"/>
              <xsl:with-param name="pWithSide" select="false()"/>
            </xsl:call-template>
          </fo:table-row>
          <!--Other Subtotal Fee rows-->
          <xsl:for-each select="$vBizSubTotal/fee">
            <xsl:sort select="@paymentType"/>
            <xsl:sort select="@ccy"/>
            <xsl:sort select="@side=$gcDebit"/>
            <xsl:sort select="@side=$gcCredit"/>

            <xsl:if test="position()>1">
              <fo:table-row font-size="{$gData_font-size}"
                            font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                            text-align="{$gData_text-align}"
                            display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                            keep-with-previous="always">

                <fo:table-cell number-columns-spanned="11">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
                <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </xsl:for-each>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: AMENDMENT / TRANSFER                                 -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_AmendmentTransfersData               -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display AmendmentTransfers Content              -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_AmendmentTransfersData">
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="11"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Fee']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_FeesExclTax"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='ContractValue']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset//trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizAsset/posAction"/>
          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset//trade"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$gDailyTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <xsl:variable name="vBizPosAction" select="$vBizTrade/parent::node()[name()='trades']/parent::node()[name()='posAction']"/>
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>
            <xsl:variable name="vBizTrade2" select="$vBizPosAction/trades/trade2 | $vBizTrade/tradeSrc"/>

            <!--On considère les frais sur PosAction s'ils existent sinon on considère les frais sur le trade-->
            <xsl:variable name="vFee" select="$vPosAction/fee | $vEfsmlTrade/fee[$vPosAction=false()]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gBlockSettings_Data/@padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Type']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='Type']/data/@font-weight}"
                             text-align="{$gBlockSettings_Data/column[@name='Type']/data/@text-align}"
                             padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RTSEfsml">
                    <xsl:with-param name="pDataName" select="'ActionTyp'"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                    <xsl:with-param name="pTrade" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@text-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="UKDisplay_Underlyer">
                    <xsl:with-param name="pRepository-asset" select="$vRepository_Asset"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Underlying']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vFee">
                  <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                  <xsl:for-each select="$vFee">
                    <xsl:sort select="@paymentType"/>
                    <xsl:sort select="@ccy"/>
                    <xsl:sort select="@side=$gcDebit"/>
                    <xsl:sort select="@side=$gcCredit"/>

                    <xsl:if test="position()=1">
                      <xsl:call-template name="UKDisplay_Fee"/>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!--On considère le notional sur PosAction si le trade a subit une action, sinon on considère le notional sur le trade lui même-->
              <xsl:call-template name="UKDisplay_Amount">
                <xsl:with-param name="pAmount" select="$vPosAction/notional | $vTrade[$vPosAction=false()]/returnSwap/returnLeg/notional/notionalAmount/amount"/>
                <xsl:with-param name="pCcy" select="$vTrade/returnSwap/returnLeg/notional/notionalAmount/currency"/>
                <xsl:with-param name="pWithSide" select="false()"/>
              </xsl:call-template>
            </fo:table-row>
            <xsl:choose>
              <!--Le premier row Fee a été déjà affiché sur la première ligne-->
              <xsl:when test="count($vFee)>1">
                <!--Other Fee rows-->
                <xsl:for-each select="$vFee">
                  <xsl:sort select="@paymentType"/>
                  <xsl:sort select="@ccy"/>
                  <xsl:sort select="@side=$gcDebit"/>
                  <xsl:sort select="@side=$gcCredit"/>

                  <!--Le premier a été déjà affiché sur la première ligne-->
                  <xsl:if test="position()>1">
                    <fo:table-row font-size="{$gData_font-size}"
                                  font-weight="{$gData_font-weight}"
                                  text-align="{$gData_text-align}"
                                  display-align="{$gData_display-align}">

                      <xsl:choose>
                        <xsl:when test="$vBizTrade2 and position()=2">
                          <fo:table-cell>
                            <xsl:call-template name="Debug_border-green"/>
                          </fo:table-cell>
                          <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@font-style}"
                                         color="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@color}">
                            <fo:block>
                              <xsl:call-template name="Debug_border-green"/>
                              <xsl:call-template name="DisplayData_Format">
                                <xsl:with-param name="pData" select="$vBizTrade2/@tradeId"/>
                                <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                              </xsl:call-template>
                            </fo:block>
                          </fo:table-cell>
                          <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@font-style}"
                                         text-align="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@text-align}"
                                         color="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@color}"
                                         number-columns-spanned="8">
                            <fo:block>
                              <xsl:call-template name="Debug_border-green"/>
                              <xsl:call-template name="DisplayData_RTSEfsml">
                                <xsl:with-param name="pDataName" select="'ActionTyp2'"/>
                                <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                                <xsl:with-param name="pTrade" select="$vEfsmlTrade"/>
                                <xsl:with-param name="pTrade2" select="$gDataDocument/trade[@tradeId=$vBizTrade2/@tradeId]"/>
                              </xsl:call-template>
                            </fo:block>
                          </fo:table-cell>
                        </xsl:when>
                        <xsl:otherwise>
                          <fo:table-cell number-columns-spanned="10">
                            <xsl:call-template name="Debug_border-green"/>
                          </fo:table-cell>
                        </xsl:otherwise>
                      </xsl:choose>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                      <xsl:call-template name="UKDisplay_Fee"/>
                      <fo:table-cell number-columns-spanned="4">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                </xsl:for-each>
              </xsl:when>
              <xsl:when test="$vBizTrade2">
                <fo:table-row font-size="{$gData_font-size}"
                              font-weight="{$gData_font-weight}"
                              text-align="{$gData_text-align}"
                              display-align="{$gData_display-align}">

                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@font-style}"
                                 color="{$gBlockSettings_Data/column[@name='TrdNum']/data[@name='trade2']/@color}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vBizTrade2/@tradeId"/>
                        <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell font-style="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@font-style}"
                                 text-align="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@text-align}"
                                 color="{$gBlockSettings_Data/column[@name='Type']/data[@name='trade2']/@color}"
                                 number-columns-spanned="8">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RTSEfsml">
                        <xsl:with-param name="pDataName" select="'ActionTyp2'"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pTrade" select="$vEfsmlTrade"/>
                        <xsl:with-param name="pTrade2" select="$gDataDocument/trade[@tradeId=$vBizTrade2/@tradeId]"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:when>
            </xsl:choose>
          </xsl:for-each>
        </xsl:for-each>
        <!--Display underline-->
        <xsl:call-template name="Display_SubTotalSpace">
          <xsl:with-param name="pPosition" select="1"/>
        </xsl:call-template>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: PURCHASE & SALE                                      -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_PurchaseSaleData                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display PurchaseSale Content                    -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_PurchaseSaleData">
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="11"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                          padding="{$gBlockSettings_Data/title/@padding}"
                          display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='buy']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='sell']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}"
                             number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='RealisedPnL']/@resource"/>
              <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
                <xsl:value-of select="$gcLegend_RealisedPnL"/>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$gPosTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <!--Tri pour les deux modèles PnLOnClosing et OverallQty-->
            <xsl:sort select="current()/@closedTrdDt" data-type="text"/>
            <xsl:sort select="current()/@closedLastPx" data-type="number"/>
            <xsl:sort select="current()/@closedTradeId" data-type="text"/>
            <xsl:sort select="current()/@isClosed" data-type="text" order="descending"/>
            <!--Tri pour les deux modèles PnLOnClosing et OverallQty-->
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@text-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="UKDisplay_Underlyer">
                    <xsl:with-param name="pRepository-asset" select="$vRepository_Asset"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Underlying']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vBizTrade/fee">
                  <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                  <xsl:for-each select="$vBizTrade/fee">
                    <xsl:sort select="@paymentType"/>
                    <xsl:sort select="@ccy"/>
                    <xsl:sort select="@side=$gcDebit"/>
                    <xsl:sort select="@side=$gcCredit"/>

                    <xsl:if test="position()=1">
                      <xsl:call-template name="UKDisplay_Fee"/>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_Amount">
                <xsl:with-param name="pAmount" select="$vBizTrade/rmg"/>
              </xsl:call-template>
            </fo:table-row>
            <!--Other Fee rows-->
            <xsl:for-each select="$vBizTrade/fee">
              <xsl:sort select="@paymentType"/>
              <xsl:sort select="@ccy"/>
              <xsl:sort select="@side=$gcDebit"/>
              <xsl:sort select="@side=$gcCredit"/>

              <!--Le premier a été déjà affiché sur la première ligne-->
              <xsl:if test="position()>1">
                <fo:table-row font-size="{$gData_font-size}"
                              font-weight="{$gData_font-weight}"
                              text-align="{$gData_text-align}"
                              display-align="{$gData_display-align}">

                  <fo:table-cell number-columns-spanned="11">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <xsl:call-template name="UKDisplay_Fee"/>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>

          <!--Subtotal row-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'LongQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'ShortQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vBizSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="3">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:choose>
              <xsl:when test="$vBizSubTotal/fee">
                <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                <xsl:for-each select="$vBizSubTotal/fee">
                  <xsl:sort select="@paymentType"/>
                  <xsl:sort select="@ccy"/>
                  <xsl:sort select="@side=$gcDebit"/>
                  <xsl:sort select="@side=$gcCredit"/>

                  <xsl:if test="position()=1">
                    <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
                  </xsl:if>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="4">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:variable name="vCcy_RealisedPnL">
              <xsl:call-template name="GetAmount-ccy">
                <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vTotal_RealisedPnL">
              <xsl:call-template name="GetAmount-amt">
                <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
                <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vColor_RealisedPnL">
              <xsl:call-template name="GetAmount-color">
                <xsl:with-param name="pAmount" select="$vTotal_RealisedPnL"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vBGColor_RealisedPnL">
              <xsl:call-template name="GetAmount-background-color">
                <xsl:with-param name="pAmount" select="$vTotal_RealisedPnL"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_RealisedPnL" />
              <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
              <xsl:with-param name="pColor" select="$vColor_RealisedPnL" />
              <xsl:with-param name="pBackground-color" select="$vBGColor_RealisedPnL" />
            </xsl:call-template>
          </fo:table-row>
          <!--Other Subtotal Fee rows-->
          <xsl:if test="count($vBizSubTotal/fee) > 1">
            <xsl:for-each select="$vBizSubTotal/fee">
              <xsl:sort select="@paymentType"/>
              <xsl:sort select="@ccy"/>
              <xsl:sort select="@side=$gcDebit"/>
              <xsl:sort select="@side=$gcCredit"/>

              <xsl:if test="position()>1">
                <fo:table-row font-size="{$gData_font-size}"
                              font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                              text-align="{$gData_text-align}"
                              display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                              keep-with-previous="always">

                  <fo:table-cell number-columns-spanned="11">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>
          </xsl:if>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: OPEN POSITIONS                                       -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_OpenPosData                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OpenPosition Content                    -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_OpenPositionData">
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="11"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="19" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_Header-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header[@name='TradeDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Price']/header/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Price']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Margin']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='MarketRevaluation']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>
        <xsl:variable name="vBookSubTotal" select="$gPosTrades/subTotal[@idB=$pBizBook/@OTCmlId and @idI=$pBizBook/asset/@idI]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/*[name()=current()/@assetName and @OTCmlId=current()/@OTCmlId]/displayname" data-type="text"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$gPosTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@tradeId" data-type="text"/>

            <xsl:variable name="vBizTrade" select="current()"/>
            <!--Only one EfsmlTrade-->
            <xsl:variable name="vEfsmlTrade" select="$vAssetEfsmlTradeList[@tradeId=$vBizTrade/@tradeId][1]"/>
            <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vBizTrade/@tradeId]"/>

            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@trdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizTrade/@tradeId"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_ReturnSwap">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pRTSTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@text-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="UKDisplay_Underlyer">
                    <xsl:with-param name="pRepository-asset" select="$vRepository_Asset"/>
                    <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Underlying']"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                             padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test="$vEfsmlTrade/mgr/@factor">
                    <xsl:variable name="vFactor">
                      <xsl:call-template name="DisplayPercentage">
                        <xsl:with-param name="pFactor" select="$vEfsmlTrade/mgr/@factor"/>
                      </xsl:call-template>
                    </xsl:variable>
                    <xsl:value-of select="concat('(',$vFactor,')')"/>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_Amount">
                <xsl:with-param name="pAmount" select="$vEfsmlTrade/umg"/>
              </xsl:call-template>
            </fo:table-row>
          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vSubTotal" select="$vBookSubTotal[@idAsset=$vBizAsset/@OTCmlId and @assetCategory=$vBizAsset/@assetCategory and @idI=$vBizAsset/@idI][1]"/>
          <xsl:variable name="vBizSubTotal" select="$vBizAsset/subTotal[1]"/>

          <xsl:variable name="vCcy_MGR">
            <xsl:call-template name="GetAmount-ccy">
              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/mgr"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vTotal_MGR">
            <xsl:call-template name="GetAmount-amt">
              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/mgr"/>
              <xsl:with-param name="pCcy" select="$vCcy_MGR"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vColor_MGR">
            <xsl:call-template name="GetAmount-color">
              <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vBGColor_MGR">
            <xsl:call-template name="GetAmount-background-color">
              <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vCcy_MRV">
            <xsl:call-template name="GetAmount-ccy">
              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vTotal_MRV">
            <xsl:call-template name="GetAmount-amt">
              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
              <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vColor_MRV">
            <xsl:call-template name="GetAmount-color">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vBGColor_MRV">
            <xsl:call-template name="GetAmount-background-color">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
            </xsl:call-template>
          </xsl:variable>

          <!--Total Row-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="2">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'LongQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           background-color="{$gBlockSettings_Data/subtotal/column[@name='Qty']/data/@background-color}">
              <fo:block >
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="DisplayData_SubTotal">
                  <xsl:with-param name="pDataName" select="'ShortQty'"/>
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayConverted">
                  <xsl:with-param name="pConverted" select="$vAssetEfsmlTradeList[1]/@fmtClrPx"/>
                  <xsl:with-param name="pPattern" select="$vBizAsset/pattern/@price" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='AvgPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="3">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:call-template name="UKDisplay_SubTotal_AvgPx">
                  <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
                  <xsl:with-param name="pPricePattern" select="$vBizAsset/pattern/@avgPrice"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_MGR"/>
              <xsl:with-param name="pCcy" select="$vCcy_MGR"/>
              <xsl:with-param name="pColor" select="$vColor_MGR" />
              <xsl:with-param name="pBackground-color" select="$vBGColor_MGR" />
            </xsl:call-template>
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_MRV"/>
              <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
              <xsl:with-param name="pColor" select="$vColor_MRV" />
              <xsl:with-param name="pBackground-color" select="$vBGColor_MRV" />
            </xsl:call-template>
          </fo:table-row>
          <!--Total in countervalue Row-->
          <xsl:if test="(string-length($vCcy_MGR) >0 and $vCcy_MGR != $gReportingCcy) or (string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy)">

            <xsl:variable name="vFxRate_MGR">
              <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gReportingCcy">
                <xsl:call-template name="GetExchangeRate_Repository">
                  <xsl:with-param name="pFlowCcy" select="$vCcy_MGR" />
                  <xsl:with-param name="pExCcy" select="$gReportingCcy" />
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="vFxRate_MRV">
              <xsl:choose>
                <xsl:when test="$vCcy_MGR = $vCcy_MRV">
                  <xsl:value-of select="$vFxRate_MGR"/>
                </xsl:when>
                <xsl:when test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy">
                  <xsl:call-template name="GetExchangeRate_Repository">
                    <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                    <xsl:with-param name="pExCcy" select="$gReportingCcy" />
                  </xsl:call-template>
                </xsl:when>
              </xsl:choose>
            </xsl:variable>

            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                          keep-with-previous="always">

              <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='TotalInCV']/@text-align}"
                             padding="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                             number-columns-spanned="5">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='TotalInCV']/data/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SpotRate']/@font-size}"
                             font-weight="{$gBlockSettings_Data/subtotal/column[@name='SpotRate']/@font-weight}"
                             text-align="{$gData_Number_text-align}"
                             padding="{$gBlockSettings_Data/subtotal/@padding}"
                             padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                             number-columns-spanned="5">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gReportingCcy">
                    <xsl:call-template name="DisplayeFxRate">
                      <xsl:with-param name="pFxRate" select="$vFxRate_MGR" />
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MGR" />
                      <xsl:with-param name="pExCcy" select="$gReportingCcy" />
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gReportingCcy and 
                          string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy and $vCcy_MGR != $vCcy_MRV">
                    <xsl:value-of select="' - '"/>
                  </xsl:if>
                  <xsl:if test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy and $vCcy_MGR != $vCcy_MRV">
                    <xsl:call-template name="DisplayeFxRate">
                      <xsl:with-param name="pFxRate" select="$vFxRate_MRV" />
                      <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                      <xsl:with-param name="pExCcy" select="$gReportingCcy" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="string-length($vCcy_MGR) >0 and $vCcy_MGR != $gReportingCcy">
                  <xsl:call-template name="UKDisplay_SubTotal_Amount">
                    <xsl:with-param name="pAmount" select="$vTotal_MGR div number($vFxRate_MGR)" />
                    <xsl:with-param name="pCcy" select="$gReportingCcy"/>
                    <xsl:with-param name="pAmountPattern" select="$gReportingCcyPattern" />
                    <xsl:with-param name="pColor" select="$vColor_MGR" />
                    <xsl:with-param name="pBackground-color" select="$vBGColor_MGR" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy">
                  <xsl:call-template name="UKDisplay_SubTotal_Amount">
                    <xsl:with-param name="pAmount" select="$vTotal_MRV div number($vFxRate_MRV)" />
                    <xsl:with-param name="pCcy" select="$gReportingCcy"/>
                    <xsl:with-param name="pAmountPattern" select="$gReportingCcyPattern" />
                    <xsl:with-param name="pColor" select="$vColor_MRV" />
                    <xsl:with-param name="pBackground-color" select="$vBGColor_MRV" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
          </xsl:if>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: ACCOUNT SUMMARY                                      -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_AccountSummaryData                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Account Summary Content                 -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_AccountSummaryBody">
    <xsl:param name="pCashBalanceStreams"/>
    <xsl:param name="pExchangeCashBalanceStream"/>
    <xsl:param name="pBizGroup"/>
    <xsl:param name="pIsHideBaseCcy"/>

    <xsl:variable name="vAllCashBalanceStreams" select="$gCBTrade/cashBalanceReport/cashBalanceStream"/>

    <!--SpotRate-->
    <xsl:if test="$pIsHideBaseCcy = false() and $pBizGroup/currency">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'SpotRate'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--Empty Row-->
      <xsl:call-template name="UKSynthesis_SummaryRowEmpty">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pWithBorder" select="false()"/>
      </xsl:call-template>
    </xsl:if>
    <!--PreviousCashBalance-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'PreviousCashBalance'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/previousCashBalance"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--RealisedProfitLoss-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/other
                      | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/other
                      | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--Fee-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'Fee'"/>
      <xsl:with-param name="pAmount" select="$pBizGroup/fee/fee | $pBizGroup/fee/tax/tax"/>
      <xsl:with-param name="pExchangeAmount" select="$pBizGroup/exchangeCurrency/fee | $pBizGroup/exchangeCurrency/fee/tax"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--Funding-->
    <xsl:if test="$vAllCashBalanceStreams/funding/detail">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'Funding'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/funding"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/funding"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
    <!--Borrowing-->
    <xsl:if test="$vAllCashBalanceStreams/borrowing/detail">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'Borrowing'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/borrowing"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/borrowing"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
    <!--CashBalancePayment-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'CashBalancePayment'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashBalancePayment"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashBalancePayment"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--CashBalance-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'CashBalance'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashBalance"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pIsSubtotal" select="true()"/>
    </xsl:call-template>
    <!--Empty Row-->
    <xsl:call-template name="UKSynthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--OpenTradeEqty-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'OpenTradeEqty'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/other"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/other"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--UnsettledTransactions-->
    <xsl:if test="$vAllCashBalanceStreams/unsettledCash[@amt > 0]">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'UnsettledTransactions'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unsettledCash"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unsettledCash"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
    <!--TotalEqty-->
    <xsl:variable name="vTotalEqty_AmountName">
      <xsl:choose>
        <xsl:when test="$vAllCashBalanceStreams/unsettledCash[@amt > 0]">
          <xsl:value-of select="'TotalEqtyWithUT'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'TotalEqty'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="$vTotalEqty_AmountName"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/equityBalance"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/equityBalance"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pIsAmountMaster" select="true()"/>
    </xsl:call-template>

    <!--Empty Row-->
    <xsl:call-template name="UKSynthesis_SummaryRowEmpty">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--InitialMarginReq-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'InitialMarginReq'"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/marginRequirement"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/marginRequirement"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--SecuritiesOnDeposit-->
    <xsl:if test="$vAllCashBalanceStreams/collateralAvailable[@amt > 0]">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'SecuritiesOnDeposit'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/collateralAvailable"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/collateralAvailable"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
    <!--MarginDeficitExcess-->
    <xsl:variable name="vMarginDeficitExcess_AmountName">
      <xsl:choose>
        <xsl:when test="$pCashBalanceStreams/collateralAvailable[@amt > 0]">
          <xsl:value-of select="'MarginDeficitExcess'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'MarginDeficitExcessNoSOD'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="$vMarginDeficitExcess_AmountName"/>
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/excessDeficit"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/excessDeficit"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pIsSubtotal" select="true()"/>
    </xsl:call-template>

    <xsl:variable name="vIsDisplayFees" select="($pBizGroup/fee/fee[@amt !=0] or $pBizGroup/exchangeCurrency/fee[@amt !=0]) = true()"/>

    <xsl:variable name="vIsDisplayOthersPnL" select="$vAllCashBalanceStreams/realizedMargin/other[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOthersCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOthersOTE" select="$vAllCashBalanceStreams/unrealizedMargin/other[@amt!=0] = true()"/>

    <xsl:variable name="vIsDisplaySummaryCashFlows"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryCashFlows/text() != 'None') and
                  ($vIsDisplayOthersPnL or 
                  $vIsDisplayOthersCshSettl or 
                  $vIsDisplayOthersOTE)"/>
    <xsl:variable name="vIsDisplaySummaryFees"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryFees/text() != 'None') and $vIsDisplayFees"/>

    <!--Detail separation-->
    <xsl:if test="$vIsDisplaySummaryCashFlows or $vIsDisplaySummaryFees">
      <!--Empty Row-->
      <xsl:call-template name="UKSynthesis_SummaryRowEmpty">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pWithBorder" select="false()"/>
      </xsl:call-template>
      <!--Display underline-->
      <xsl:call-template name="UKSynthesis_SummaryRowLine">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--Empty Row-->
      <xsl:call-template name="UKSynthesis_SummaryRowEmpty">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pWithBorder" select="false()"/>
      </xsl:call-template>
    </xsl:if>
    <!--RealisedProfitLoss Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayOthersPnL or 
            $vIsDisplayOthersCshSettl)">
      <!--RealisedProfitLossDetail : title-->
      <xsl:call-template name="UKSynthesis_SummaryRowTitle">
        <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--RealisedProfitLossOthers-->
      <xsl:if test="$vIsDisplayOthersPnL">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'RealisedProfitLossOthers'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/other"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/other"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--CashSettlementOthers-->
      <xsl:if test="$vIsDisplayOthersCshSettl">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'CashSettlementOthers'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/other"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
    <!--Fee Detail-->
    <xsl:if test="$vIsDisplaySummaryFees">
      <xsl:call-template name="UKSynthesis_SummaryFeeDetail">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>