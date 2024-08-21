<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres reports - Synthesis Report                                                   -->
  <!-- File    : \Report_v2\ETDSynthesisReport.xsl                                                    -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140902                                                                             -->
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

  <!--<xsl:import href="CFDSynthesisReport.xsl" />-->

  <!--Business-->
  <xsl:import href="Shared\Shared_Report_v2_Business.xslt" />
  <xsl:import href="LSD\ETD_Report_v2_Business.xslt" />
  <!--Report Format-->
  <xsl:import href="Shared\Shared_Report_v2_A4Vertical.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_UK.xslt" />
  <!--Tools-->
  <xsl:import href="Shared\Shared_Report_v2_Tools.xslt" />
  <xsl:import href="Shared\Shared_Report_v2_FixmlTools.xslt" />
  <xsl:import href="LSD\ETD_Report_v2_Tools.xslt" />
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
    <xsl:variable name="vBizETDTradesConfirmations_Node">
      <xsl:variable name="vSubTotal" select="$gDailyTrades/subTotal[predicates/predicate/@trdTyp]"/>
      <xsl:call-template name="BizETD_MarketDCSubTotal-market">
        <xsl:with-param name="pEfsmlTrade" select="$gDailyTrades/trade[@trdTyp=$vSubTotal/predicates/predicate/@trdTyp]"/>
        <xsl:with-param name="pSubTotal" select="$vSubTotal"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizETDAmendmentTransfers_Node">
      <!-- POC, POT et LateTrade -->
      <xsl:call-template name="BizETD_MarketDCAmendmentTransfers-market"/>
    </xsl:variable>
    <xsl:variable name="vBizETDLiquidations_Node">
      <!-- MOF : Liquidation de future -->
      <xsl:call-template name="BizETD_MarketDCPosAction-market">
        <xsl:with-param name="pEfsmlData" select="$gPosActions/posAction[@requestType='MOF']"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizETDExeAss_Node">
      <!-- EXE, AUTOEXE, ASS et AUTOASS -->
      <xsl:call-template name="BizETD_MarketDCPosAction-market">
        <xsl:with-param name="pEfsmlData" select="$gPosActions/posAction[contains(',EXE,AUTOEXE,ASS,AUTOASS,',concat(',',@requestType,','))]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizETDAbandonments_Node">
      <!-- ABN, AUTOABN -->
      <xsl:call-template name="BizETD_MarketDCPosAction-market">
        <xsl:with-param name="pEfsmlData" select="$gPosActions/posAction[contains(',ABN,AUTOABN,',concat(',',@requestType,','))]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizETDPurchaseSales_Node">
      <!-- UNCLEARING: Compensation générée suite à une décompensation partielle -->
      <!-- CLEARSPEC : Compensation  Spécifique -->
      <!-- CLEARBULK : Compensation globale -->
      <!-- CLEAREOD : Compensation fin de journée -->
      <!-- ENTRY : Clôture STP -->
      <!-- UPDENTRY : Clôture Fin de journée -->
      <!-- Hors Décompensation -->
      <xsl:call-template name="BizETD_MarketDCPurchaseSale-market"/>
    </xsl:variable>
    <xsl:variable name="vBizETDOpenPosition_Node">
      <xsl:call-template name="BizETD_MarketDCSubTotal-market">
        <xsl:with-param name="pEfsmlTrade" select="$gPosTrades/trade"/>
        <xsl:with-param name="pSubTotal" select="$gPosTrades/subTotal"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBizJournalEntries_Node">
      <xsl:call-template name="Business_JournalEntries-book"/>
    </xsl:variable>
    <xsl:variable name="vBizAccountSummary_Node">
      <xsl:call-template name="Business_AccountSummary-group"/>
    </xsl:variable>
    <xsl:variable name="vBizETDTradesConfirmations" select="msxsl:node-set($vBizETDTradesConfirmations_Node)/market"/>
    <xsl:variable name="vBizETDAmendmentTransfers" select="msxsl:node-set($vBizETDAmendmentTransfers_Node)/market"/>
    <xsl:variable name="vBizETDLiquidations" select="msxsl:node-set($vBizETDLiquidations_Node)/market"/>
    <xsl:variable name="vBizETDExeAss" select="msxsl:node-set($vBizETDExeAss_Node)/market"/>
    <xsl:variable name="vBizETDAbandonments" select="msxsl:node-set($vBizETDAbandonments_Node)/market"/>
    <xsl:variable name="vBizETDPurchaseSales" select="msxsl:node-set($vBizETDPurchaseSales_Node)/market"/>
    <xsl:variable name="vBizETDOpenPosition" select="msxsl:node-set($vBizETDOpenPosition_Node)/market"/>
    <xsl:variable name="vBizJournalEntries" select="msxsl:node-set($vBizJournalEntries_Node)/book"/>
    <xsl:variable name="vBizAccountSummary" select="msxsl:node-set($vBizAccountSummary_Node)/group"/>

    <xsl:choose>
      <xsl:when test="$gcIsBusinessDebugMode=true()">
        <tradesConfirmations>
          <xsl:copy-of select="$vBizETDTradesConfirmations"/>
        </tradesConfirmations>
        <amendmentTransfers>
          <xsl:copy-of select="$vBizETDAmendmentTransfers"/>
        </amendmentTransfers>
        <liquidations>
          <xsl:copy-of select="$vBizETDLiquidations"/>
        </liquidations>
        <exeAss>
          <xsl:copy-of select="$vBizETDExeAss"/>
        </exeAss>
        <abandonments>
          <xsl:copy-of select="$vBizETDAbandonments"/>
        </abandonments>
        <purchaseSale>
          <xsl:copy-of select="$vBizETDPurchaseSales"/>
        </purchaseSale>
        <openPosition>
          <xsl:copy-of select="$vBizETDOpenPosition"/>
        </openPosition>
        <journalEntries>
          <xsl:copy-of select="$vBizJournalEntries"/>
        </journalEntries>
        <accountSummary>
          <xsl:copy-of select="$vBizAccountSummary"/>
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
                  <xsl:with-param name="pBusiness_Section" select="$vBizETDTradesConfirmations"/>
                </xsl:call-template>
                <!--2/ Section: AmendmentTransfers / Optional -->
                <xsl:if test="$vBizETDAmendmentTransfers">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_AMT"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDAmendmentTransfers"/>
                  </xsl:call-template>
                </xsl:if>
                <!--3/ Section: Liquidations / Optional -->
                <xsl:if test="$vBizETDLiquidations">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_LIQ"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDLiquidations"/>
                  </xsl:call-template>
                </xsl:if>
                <!--4/ Section: ExeAss / Optional -->
                <xsl:if test="$vBizETDExeAss">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_EXA"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDExeAss"/>
                  </xsl:call-template>
                </xsl:if>
                <!--5/ Section: Abandonments / Optional -->
                <xsl:if test="$vBizETDAbandonments">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ABN"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDAbandonments"/>
                  </xsl:call-template>
                </xsl:if>
                <!--6/ Section: PurchaseSale / Optional -->
                <xsl:if test="$vBizETDPurchaseSales">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_PSS"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizETDPurchaseSales"/>
                  </xsl:call-template>
                </xsl:if>
                <!--7/ Section: OpenPosition / Mandatory -->
                <xsl:call-template name="UKSynthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_POS"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizETDOpenPosition"/>
                </xsl:call-template>
                <!--8/ Section: JournalEntries / Optional -->
                <xsl:if test="$vBizJournalEntries">
                  <xsl:call-template name="UKSynthesis_Section">
                    <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_JNL"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizJournalEntries"/>
                  </xsl:call-template>
                </xsl:if>
                <!--9/ Section: Account Summary / Mandatory -->
                <fo:block break-after='page'/>
                <xsl:call-template name="UKSynthesis_Section">
                  <xsl:with-param name="pSectionKey" select="$gcReportSectionKey_ACC"/>
                  <xsl:with-param name="pBusiness_Section" select="$vBizAccountSummary"/>
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
        <xsl:if test="$vBizETDTradesConfirmations or $vBizETDAmendmentTransfers  or 
                $vBizETDLiquidations or $vBizETDExeAss or $vBizETDAbandonments or 
                $vBizETDPurchaseSales">
          <fo:block font-size="{$gData_font-size}"
                    padding="{$gBlockSettings_Legend/@padding}"
                    display-align="{$gData_display-align}"
                    keep-together.within-page="always"
                    space-before="{$gLegend_space-before}">

            <!--Legend_FeesExclTax-->
            <xsl:call-template name="UKDisplay_Legend">
              <xsl:with-param name="pLegend" select="$gcLegend_FeesExclTax"/>
              <xsl:with-param name="pResource" select="'Report-LegendFeesExclTax'"/>
            </xsl:call-template>
            <!--Legend_RealisedPnL-->
            <xsl:if test="$vBizETDLiquidations or $vBizETDExeAss or $vBizETDAbandonments or $vBizETDPurchaseSales">
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
    <xsl:param name="pBizDC"/>
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
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
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
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="3" border="{$gTable_border}" border-left-style="none" border-right-style="none">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset/trade/@tradeId]"/>
        <xsl:variable name="vBookSubTotal" select="$gDailyTrades/subTotal[predicates/predicate/@trdTyp and @idB=$pBizBook/@OTCmlId and @idI=$pBizBook/asset/@idI]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

          <xsl:variable name="vBizAsset" select="current()"/>
          <xsl:variable name="vRepository_Asset" select="$gRepository/*[name()=$vBizAsset/@assetName and @OTCmlId=$vBizAsset/@OTCmlId]"/>

          <xsl:variable name="vAssetBizTradeList" select="$vBizAsset/trade"/>
          <xsl:variable name="vAssetEfsmlTradeList" select="$gDailyTrades/trade[@tradeId=$vAssetBizTradeList/@tradeId]"/>
          <xsl:variable name="vAssetTradeList" select="$vBookTradeList[@tradeId=$vAssetBizTradeList/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizTradeList">
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
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
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'TrdTyp'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'BuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'SellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell padding="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'PC'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding="{$gData_padding}"
                                 text-align="{$gData_Number_text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'ConvertedStrike'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                        <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@strike" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
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
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vEfsmlTrade/prm"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
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
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <!--<xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>-->

                  <xsl:call-template name="GetExpiry-color">
                    <xsl:with-param name="pMaturityDate" select="$vRepository_Asset/maturityDate/text()"/>
                    <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_RepositoryEtd">
                  <xsl:with-param name="pDataName" select="'Expiry'"/>
                  <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
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
                  <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@avgPrice"/>
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
            <xsl:choose>
              <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
                <xsl:variable name="vCurrency_PRM" select="$vAssetEfsmlTradeList/prm/@ccy"/>
                <xsl:variable name="vTotal_PRM">
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/prm"/>
                    <xsl:with-param name="pCcy" select="$vCurrency_PRM"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:variable name="vColor_PRM">
                  <xsl:call-template name="GetAmount-color">
                    <xsl:with-param name="pAmount" select="$vTotal_PRM"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:variable name="vBGColor_PRM">
                  <xsl:call-template name="GetAmount-background-color">
                    <xsl:with-param name="pAmount" select="$vTotal_PRM"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:call-template name="UKDisplay_SubTotal_Amount">
                  <xsl:with-param name="pAmount" select="$vTotal_PRM" />
                  <xsl:with-param name="pCcy" select="$vCurrency_PRM"/>
                  <xsl:with-param name="pColor" select="$vColor_PRM" />
                  <xsl:with-param name="pBackground-color" select="$vBGColor_PRM" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="3">
                  <xsl:call-template name="Debug_border-green"/>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
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
    <xsl:param name="pBizDC"/>
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
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
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
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}"
                             number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name='Premium']/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="3" border="{$gTable_border}" border-left-style="none" border-right-style="none">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:variable name="vBookTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/asset//trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/asset">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

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
            <xsl:sort select="current()/@trdNum" data-type="text"/>
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
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_ETDEfsml">
                    <xsl:with-param name="pDataName" select="'ActionTyp'"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                    <xsl:with-param name="pTrade" select="$vTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell padding="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'PC'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding="{$gData_padding}"
                                 text-align="{$gData_Number_text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'ConvertedStrike'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                        <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@strike" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
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
              <xsl:choose>
                <!--On considère la prime sur PosAction si le trade a subit une action, sinon on considère la prime sur le trade lui même-->
                <xsl:when test="$pBizDC/@category = 'O' and $pBizDC/@futValuationMethod = 'EQTY'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosAction/prm | $vEfsmlTrade[$vPosAction=false()]/prm"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
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
                                <xsl:with-param name="pData" select="$vBizTrade2/@trdNum"/>
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
                              <xsl:call-template name="DisplayData_ETDEfsml">
                                <xsl:with-param name="pDataName" select="'ActionTyp2'"/>
                                <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                                <xsl:with-param name="pTrade" select="$vTrade"/>
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
                        <xsl:with-param name="pData" select="$vBizTrade2/@trdNum"/>
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
                      <xsl:call-template name="DisplayData_ETDEfsml">
                        <xsl:with-param name="pDataName" select="'ActionTyp2'"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                        <xsl:with-param name="pTrade" select="$vTrade"/>
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
  <!--              Section: LIQUIDATIONS                                         -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_LiquidationsData                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Liquidations Content                    -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_LiquidationsData">
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="10"/>
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

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
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
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
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         number-columns-spanned="2">
            <xsl:call-template name="Debug_border-green"/>
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
        <xsl:variable name="vBookETDTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/etd/posAction/trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/etd">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

          <xsl:variable name="vBizEtd" select="current()"/>
          <xsl:variable name="vRepository_etd" select="$gRepository/etd[@OTCmlId=$vBizEtd/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizEtd/posAction"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetETDTradeList" select="$vBookETDTradeList[@tradeId=$vAssetBizPosActionList/trade/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizPosActionList">
            <xsl:sort select="$vBookETDTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt" data-type="number"/>
            <xsl:sort select="$vBookETDTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx" data-type="number"/>
            <xsl:sort select="current()/trade/@trdNum" data-type="text"/>

            <xsl:variable name="vBizPosAction" select="current()"/>
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>
            <xsl:variable name="vETDTrade" select="$vAssetETDTradeList[@tradeId=$vBizPosAction/trade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBookETDTradeList[@tradeId=$vBizPosAction/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizPosAction/trade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'BuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vETDTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'SellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vETDTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizPosAction/trade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vPosAction/fee">
                  <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                  <xsl:for-each select="$vPosAction/fee">
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
                <xsl:with-param name="pAmount" select="$vPosAction/rmg"/>
                <xsl:with-param name="pCcy" select="$pBizDC/@ccy"/>
                <xsl:with-param name="pAmountPattern" select="$pBizDC/pattern/@ccy" />
              </xsl:call-template>
            </fo:table-row>
            <!--Other Fee rows-->
            <xsl:for-each select="$vPosAction/fee">
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
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>

          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizEtd/subTotal"/>

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
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <!--<xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>-->
                  <xsl:call-template name="GetExpiry-color">
                    <xsl:with-param name="pMaturityDate" select="$vRepository_etd/maturityDate/text()"/>
                    <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_RepositoryEtd">
                  <xsl:with-param name="pDataName" select="'Expiry'"/>
                  <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="3">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_Efsml">
                  <xsl:with-param name="pDataName" select="'ConvertedUnlPx'"/>
                  <xsl:with-param name="pDataEfsml" select="$vAssetPosActionList[1]"/>
                  <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@unlPrice" />
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
            <xsl:variable name="vTotal_RealisedPnL">
              <xsl:call-template name="GetAmount-amt">
                <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg"/>
                <xsl:with-param name="pCcy" select="$pBizDC/@ccy"/>
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
              <xsl:with-param name="pCcy" select="$pBizDC/@ccy"/>
              <xsl:with-param name="pAmountPattern" select="$pBizDC/pattern/@ccy" />
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
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>
          </xsl:if>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pNumber-columns" select="number('18')"/>
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Section: EXERCISES / ASSIGNMENTS / ABANDONMENTS               -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSynthesis_OptionSettlementData                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Settlement Option Content               -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_OptionSettlementData">
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Qty']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='Maturity']/@column-width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='PC']/@column-width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Strike']/@column-width}"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='Price']/@column-width}"/>
      <fo:table-column column-number="10"/>
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

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
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
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
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
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
        <xsl:variable name="vBookETDTradeList" select="$gDataDocument/trade[@tradeId=$pBizBook/etd/posAction/trade/@tradeId]"/>

        <xsl:for-each select="$pBizBook/etd">
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

          <xsl:variable name="vBizEtd" select="current()"/>
          <xsl:variable name="vRepository_etd" select="$gRepository/etd[@OTCmlId=$vBizEtd/@OTCmlId]"/>

          <xsl:variable name="vAssetBizPosActionList" select="$vBizEtd/posAction"/>
          <xsl:variable name="vAssetPosActionList" select="$gPosActions/posAction[@OTCmlId=$vAssetBizPosActionList/@OTCmlId]"/>
          <xsl:variable name="vAssetETDTradeList" select="$vBookETDTradeList[@tradeId=$vAssetBizPosActionList/trade/@tradeId]"/>

          <!--Display Details-->
          <xsl:for-each select="$vAssetBizPosActionList">
            <xsl:sort select="$vBookETDTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt" data-type="number"/>
            <xsl:sort select="$vBookETDTradeList[@tradeId=current()/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx" data-type="number"/>
            <xsl:sort select="current()/trade/@trdNum" data-type="text"/>

            <xsl:variable name="vBizPosAction" select="current()"/>
            <xsl:variable name="vPosAction" select="$vAssetPosActionList[@OTCmlId=$vBizPosAction/@OTCmlId]"/>
            <xsl:variable name="vETDTrade" select="$vAssetETDTradeList[@tradeId=$vBizPosAction/trade/@tradeId]"/>

            <!--Data row, with first Fee-->
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBookETDTradeList[@tradeId=$vBizPosAction/trade/@tradeId]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizPosAction/trade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vETDTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vETDTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vPosAction"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'PC'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'ConvertedStrike'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                    <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@strike" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizPosAction/trade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$vPosAction/fee">
                  <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
                  <xsl:for-each select="$vPosAction/fee">
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
              <xsl:choose>
                <xsl:when test="$pBizDC/@futValuationMethod = 'FUT'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosAction/rmg"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vPosAction/scu">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosAction/scu"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="number('0')"/>
                    <xsl:with-param name="pCcy" select="$vPosAction/rmg/@ccy"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
            <!--Other Fee rows-->
            <xsl:choose>
              <xsl:when test="count($vPosAction/fee) > 1">
                <xsl:for-each select="$vPosAction/fee">
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
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                      <xsl:if test="$pBizDC/@futValuationMethod = 'FUT' and $vPosAction/scu">
                        <xsl:call-template name="UKDisplay_Amount">
                          <xsl:with-param name="pAmount" select="$vPosAction/scu"/>
                        </xsl:call-template>
                      </xsl:if>
                    </fo:table-row>
                  </xsl:if>
                </xsl:for-each>
              </xsl:when>
              <xsl:when test="$pBizDC/@futValuationMethod = 'FUT' and $vPosAction/scu">
                <fo:table-row font-size="{$gData_font-size}"
                              font-weight="{$gData_font-weight}"
                              text-align="{$gData_text-align}"
                              display-align="{$gData_display-align}">

                  <fo:table-cell number-columns-spanned="15">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosAction/scu"/>
                  </xsl:call-template>
                </fo:table-row>
              </xsl:when>
            </xsl:choose>

          </xsl:for-each>

          <!--Display Subtotal-->
          <!--Only one SubTotal-->
          <xsl:variable name="vBizSubTotal" select="$vBizEtd/subTotal"/>

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
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <!--<xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>-->
                  <xsl:call-template name="GetExpiry-color">
                    <xsl:with-param name="pMaturityDate" select="$vRepository_etd/maturityDate/text()"/>
                    <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_RepositoryEtd">
                  <xsl:with-param name="pDataName" select="'Expiry'"/>
                  <xsl:with-param name="pAsset" select="$vRepository_etd"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/@font-weight}"
                           text-align="{$gData_Number_text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="3">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='UnlSettlPx']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_Efsml">
                  <xsl:with-param name="pDataName" select="'ConvertedUnlPx'"/>
                  <xsl:with-param name="pDataEfsml" select="$vAssetPosActionList[1]"/>
                  <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@unlPrice" />
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
            <xsl:variable name="vCcy_RealisedPnL">
              <xsl:choose>
                <xsl:when test="$pBizDC/@futValuationMethod = 'FUT'">
                  <xsl:call-template name="GetAmount-ccy">
                    <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg | $vAssetPosActionList/scu"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vAssetPosActionList/scu">
                  <xsl:call-template name="GetAmount-ccy">
                    <xsl:with-param name="pAmount" select="$vAssetPosActionList/scu"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GetAmount-ccy">
                    <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vTotal_RealisedPnL">
              <xsl:choose>
                <xsl:when test="$pBizDC/@futValuationMethod = 'FUT'">
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg | $vAssetPosActionList/scu"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vAssetPosActionList/scu">
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="$vAssetPosActionList/scu"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="number('0')"/>
                    <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
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
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                  <fo:table-cell number-columns-spanned="3">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:for-each>
          </xsl:if>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pNumber-columns" select="number('18')"/>
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
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
    <xsl:param name="pBizDC"/>
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
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
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
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>

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
            <xsl:sort select="current()/@closedTrdNum" data-type="text"/>
            <xsl:sort select="current()/@closedTradeId" data-type="text"/>
            <xsl:sort select="current()/@isClosed" data-type="text" order="descending"/>
            <!--Tri pour les deux modèles PnLOnClosing et OverallQty-->
            <xsl:sort select="current()/@trdDt" data-type="text"/>
            <xsl:sort select="current()/@lastPx" data-type="number"/>
            <xsl:sort select="current()/@trdNum" data-type="text"/>
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
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vBizTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell padding="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'PC'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding="{$gData_padding}"
                                 text-align="{$gData_Number_text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'ConvertedStrike'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                        <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@strike" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
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
              <xsl:choose>
                <xsl:when test="$pBizDC/@futValuationMethod = 'FUT'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vBizTrade/rmg"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vBizTrade/rmg">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="number('0')"/>
                    <xsl:with-param name="pCcy" select="$vBizTrade/rmg/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
              </xsl:choose>
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
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <!--<xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>-->

                  <xsl:call-template name="GetExpiry-color">
                    <xsl:with-param name="pMaturityDate" select="$vRepository_Asset/maturityDate/text()"/>
                    <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_RepositoryEtd">
                  <xsl:with-param name="pDataName" select="'Expiry'"/>
                  <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                </xsl:call-template>
              </fo:block>
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
              <xsl:choose>
                <xsl:when test="$pBizDC/@futValuationMethod = 'FUT'">
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="$vBizSubTotal/rmg"/>
                    <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GetAmount-amt">
                    <xsl:with-param name="pAmount" select="number('0')"/>
                    <xsl:with-param name="pCcy" select="$vCcy_RealisedPnL"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
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
  <!-- UKSynthesis_OpenPoData                           -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OpenPosition Content                    -->
  <!-- ................................................ -->
  <xsl:template name="UKSynthesis_OpenPositionData">
    <xsl:param name="pBizDC"/>
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
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Maturity']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <xsl:choose>
            <xsl:when test="$pBizDC/@category = 'O'">
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='PC']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             padding="{$gBlockSettings_Data/title/@padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Strike']/header/@text-align}"
                             display-align="{$gData_display-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='Strike']/header/@resource"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                             number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
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
            <xsl:call-template name="Debug_border-green"/>
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
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/maturityMonthYear" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/putCall" data-type="text"/>
          <xsl:sort select="$gRepository/etd[@OTCmlId=current()/@OTCmlId]/strikePrice" data-type="number"/>
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
            <xsl:sort select="current()/@trdNum" data-type="text"/>
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
                    <xsl:with-param name="pData" select="$vBizTrade/@trdNum"/>
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
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'TrdTypPos'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Type']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosBuyQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Fixml">
                    <xsl:with-param name="pDataName" select="'PosSellQty'"/>
                    <xsl:with-param name="pFixmlTrade" select="$vTrade"/>
                    <xsl:with-param name="pDataEfsml" select="$vEfsmlTrade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_RepositoryEtd">
                    <xsl:with-param name="pDataName" select="'Maturity'"/>
                    <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="$pBizDC/@category = 'O'">
                  <fo:table-cell padding="{$gData_padding}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'PC'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding="{$gData_padding}"
                                 text-align="{$gData_Number_text-align}">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                      <xsl:call-template name="DisplayData_RepositoryEtd">
                        <xsl:with-param name="pDataName" select="'ConvertedStrike'"/>
                        <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
                        <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@strike" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="2">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayConverted">
                    <xsl:with-param name="pConverted" select="$gCommonData/trade[@tradeId=$vBizTrade/@tradeId]/@fmtLastPx"/>
                    <xsl:with-param name="pPattern" select="$pBizDC/pattern/@price" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="4">
                <xsl:call-template name="Debug_border-green"/>
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

          <xsl:variable name="vCcy_MRV">
            <xsl:choose>
              <xsl:when test="$pBizDC/@futValuationMethod = 'EQTY'">
                <xsl:call-template name="GetAmount-ccy">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/nov"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetAmount-ccy">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vTotal_MRV">
            <xsl:choose>
              <xsl:when test="$pBizDC/@futValuationMethod = 'EQTY'">
                <xsl:call-template name="GetAmount-amt">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/nov"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetAmount-amt">
                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                  <xsl:with-param name="pCcy" select="$vCcy_MRV"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
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
            <fo:table-cell font-size="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-size}"
                           font-weight="{$gBlockSettings_Data/subtotal/column[@name='Expiry']/@font-weight}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayData_RepositoryEtd">
                  <xsl:with-param name="pDataName" select="'Expiry'"/>
                  <xsl:with-param name="pAsset" select="$vRepository_Asset"/>
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
                  <xsl:with-param name="pPricePattern" select="$pBizDC/pattern/@avgPrice"/>
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
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                           number-columns-spanned="4">
              <xsl:if test="$gIsColorMode">
                <xsl:attribute name="color">
                  <!--<xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/@color"/>-->
                  <xsl:call-template name="GetExpiry-color">
                    <xsl:with-param name="pMaturityDate" select="$vRepository_Asset/maturityDate/text()"/>
                    <xsl:with-param name="pDefault-color" select="$gBlockSettings_Data/subtotal/column[@name='Expiry']/@color"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:if>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data/@resource"/>
                <xsl:value-of select="$gcSpace"/>
                <xsl:call-template name="DisplayConverted">
                  <xsl:with-param name="pConverted" select="$vAssetEfsmlTradeList[1]/@fmtClrPx"/>
                  <xsl:with-param name="pPattern" select="$pBizDC/pattern/@settlPrice" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
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
          <xsl:if test="string-length($vCcy_MRV) >0 and $vCcy_MRV != $gReportingCcy">
            <xsl:variable name="vFxRate_MRV">
              <xsl:call-template name="GetExchangeRate_Repository">
                <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                <xsl:with-param name="pExCcy" select="$gReportingCcy" />
              </xsl:call-template>
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
                  <xsl:call-template name="DisplayeFxRate">
                    <xsl:with-param name="pFxRate" select="$vFxRate_MRV" />
                    <xsl:with-param name="pFlowCcy" select="$vCcy_MRV" />
                    <xsl:with-param name="pExCcy" select="$gReportingCcy" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell number-columns-spanned="3">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_SubTotal_Amount">
                <xsl:with-param name="pAmount" select="$vTotal_MRV div number($vFxRate_MRV)" />
                <xsl:with-param name="pCcy" select="$gReportingCcy"/>
                <xsl:with-param name="pAmountPattern" select="$gReportingCcyPattern" />
                <xsl:with-param name="pColor" select="$vColor_MRV" />
                <xsl:with-param name="pBackground-color" select="$vBGColor_MRV" />
              </xsl:call-template>
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
  <!-- UKSynthesis_AccountSummaryBody                   -->
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
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future
                      | $pCashBalanceStreams/realizedMargin/option[@FutValMeth='Fut']
                      | $pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future
                      | $pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='Fut']
                      | $pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
    <!--Premium-->
    <xsl:if test="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium/detail">
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'Premium'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/premium"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/premium"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
    </xsl:if>
    <!--Fee-->
    <xsl:call-template name="UKSynthesis_SummaryRowAmount">
      <xsl:with-param name="pAmountName" select="'Fee'"/>
      <xsl:with-param name="pAmount" select="$pBizGroup/fee/fee | $pBizGroup/fee/tax/tax"/>
      <xsl:with-param name="pExchangeAmount" select="$pBizGroup/exchangeCurrency/fee | $pBizGroup/exchangeCurrency/fee/tax"/>
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
    </xsl:call-template>
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
      <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future
                      | $pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='Fut']"/>
      <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future
                      | $pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='Fut']"/>
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

    <!--LiquidatingValue: Il existe des trades option ETD en position-->
    <xsl:if test="$gDataDocument/trade[@tradeId=$gPosTrades/trade/@tradeId and exchangeTradedDerivative/Category/text()='O'] = true()">
      <!--NetOptionValue-->
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'NetOptionValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--NetLiquidatingValue-->
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'NetLiquidatingValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/equityBalance
                        | $pCashBalanceStreams/liquidatingValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/equityBalance
                        | $pExchangeCashBalanceStream/liquidatingValue"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pIsAmountMaster" select="true()"/>
      </xsl:call-template>
    </xsl:if>
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

    <xsl:variable name="vIsDisplayFutPnL" select="$vAllCashBalanceStreams/realizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptPnL" select="$vAllCashBalanceStreams/realizedMargin/option[@amt!=0 and @FutValMeth='Fut'] = true()"/>
    <xsl:variable name="vIsDisplayOptCshSettl" select="$vAllCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option[@amt!=0] = true()"/>

    <xsl:variable name="vIsDisplayFutOTE" select="$vAllCashBalanceStreams/unrealizedMargin/future[@amt!=0] = true()"/>
    <xsl:variable name="vIsDisplayOptOTE" select="$vAllCashBalanceStreams/unrealizedMargin/option[@amt!=0 and @FutValMeth='Fut'] = true()"/>

    <!--LiquidatingValue: Il existe des trades option ETD en position-->
    <xsl:variable name="vIsDisplayNOV" select="$gDataDocument/trade[@tradeId=$gPosTrades/trade/@tradeId and exchangeTradedDerivative/Category/text()='O'] = true()"/>
    
    <xsl:variable name="vIsDisplaySummaryCashFlows"
                  select="(($gSettings-headerFooter=false()) or $gSettings-headerFooter/summaryCashFlows/text() != 'None') and
                  ($vIsDisplayFutPnL or $vIsDisplayOptPnL or 
                  $vIsDisplayOptCshSettl or 
                  $vIsDisplayFutOTE or $vIsDisplayOptOTE or 
                  $vIsDisplayNOV)"/>
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
            ($vIsDisplayFutPnL or $vIsDisplayOptPnL or 
            $vIsDisplayOptCshSettl)">
      <!--RealisedProfitLossDetail : title-->
      <xsl:call-template name="UKSynthesis_SummaryRowTitle">
        <xsl:with-param name="pAmountName" select="'RealisedProfitLoss'"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--RealisedProfitLossFutures-->
      <xsl:if test="$vIsDisplayFutPnL">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'RealisedProfitLossFutures'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/future"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/future"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--RealisedProfitLossOptions-->
      <xsl:if test="$vIsDisplayOptPnL">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'RealisedProfitLossOptions'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/realizedMargin/option[@FutValMeth='Fut']"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/realizedMargin/option[@FutValMeth='Fut']"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--CashSettlementOptions-->
      <xsl:if test="$vIsDisplayOptCshSettl">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'CashSettlementOptions'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/option"/>
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
    <!--OpenTradeEqty Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and 
            ($vIsDisplayFutOTE or $vIsDisplayOptOTE)">
      <!--OpenTradeEqtyDetail  : title-->
      <xsl:call-template name="UKSynthesis_SummaryRowTitle">
        <xsl:with-param name="pAmountName" select="'OpenTradeEqty'"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--OpenTradeEqtyFutures-->
      <xsl:if test="$vIsDisplayFutOTE">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'OpenTradeEqtyFutures'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/future"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/future"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
      <!--OpenTradeEqtyOptions-->
      <xsl:if test="$vIsDisplayOptOTE">
        <xsl:call-template name="UKSynthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="'OpenTradeEqtyOptions'"/>
          <xsl:with-param name="pAmount" select="$pCashBalanceStreams/unrealizedMargin/option[@FutValMeth='Fut']"/>
          <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/unrealizedMargin/option[@FutValMeth='Fut']"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
    <!--NetOptionValueDetail Detail-->
    <xsl:if test="$vIsDisplaySummaryCashFlows and $vIsDisplayNOV">
      <!--NetOptionValueDetail : title-->
      <xsl:call-template name="UKSynthesis_SummaryRowTitle">
        <xsl:with-param name="pAmountName" select="'NetOptionValue'"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      </xsl:call-template>
      <!--LongOptionValue-->
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'LongOptionValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/longOptionValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/longOptionValue"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pIsDetail" select="true()"/>
      </xsl:call-template>
      <!--ShortOptionValue-->
      <xsl:call-template name="UKSynthesis_SummaryRowAmount">
        <xsl:with-param name="pAmountName" select="'ShortOptionValue'"/>
        <xsl:with-param name="pAmount" select="$pCashBalanceStreams/liquidatingValue/shortOptionValue"/>
        <xsl:with-param name="pExchangeAmount" select="$pExchangeCashBalanceStream/liquidatingValue/shortOptionValue"/>
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pIsDetail" select="true()"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>