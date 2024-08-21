<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common global variables for all reports                    -->
  <!-- File    : \Shared\Shared_Report_v2_Variables.xslt                                              -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.0.5738                                                                            -->
  <!-- Date    : 20150917                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [21376] Correction des noms des attributs du flux XML (élément "headerFooter":       -->
  <!-- - Remplacer "forcolor" par "forColor"                                                          -->
  <!-- - Remplacer "backcolor" par "backColor"                                                        -->
  <!-- - Remplacer "font-size" par "fontsize"                                                         -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140902                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                         Debug Tools                                            -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gcIsBusinessDebugMode" select="($pDebugMode = 'Business')"/>
  <xsl:variable name="gcIsDisplayDebugMode" select="($pDebugMode = 'Display')"/>
  <xsl:variable name="gcIsTestMode" select="false()"/>

  <!-- ============================================================================================== -->
  <!--                                         Constantes                                             -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gcReportTypeSYNTHESIS" select="'SYNTHESIS'"/>
  <xsl:variable name="gcReportTypeORDERALLOC" select="'ORDERALLOC'"/>

  <!-- Order Confirmation -->
  <xsl:variable name="gcReportSectionKey_ORD" select="'ORD'"/>
  <!-- Trade Confirmation -->
  <xsl:variable name="gcReportSectionKey_TRD" select="'TRD'"/>
  <!-- AmendmentTransfers -->
  <xsl:variable name="gcReportSectionKey_AMT" select="'AMT'"/>
  <!-- Cascading -->
  <xsl:variable name="gcReportSectionKey_CAS" select="'CAS'"/>
  <!-- Liquidations Futures -->
  <xsl:variable name="gcReportSectionKey_LIQ" select="'LIQ'"/>
  <!-- Deliveries -->
  <xsl:variable name="gcReportSectionKey_DLV" select="'DLV'"/>
  <!-- CorporateAction -->
  <xsl:variable name="gcReportSectionKey_CA" select="'CA'"/>
  <!-- Exercices -->
  <xsl:variable name="gcReportSectionKey_EXE" select="'EXE'"/>
  <!-- Assignations -->
  <xsl:variable name="gcReportSectionKey_ASS" select="'ASS'"/>
  <!-- Exercices, Assignations -->
  <xsl:variable name="gcReportSectionKey_EXA" select="'EXA'"/>
  <!-- Abandons -->
  <xsl:variable name="gcReportSectionKey_ABN" select="'ABN'"/>
  <!-- Setttled Trades -->
  <xsl:variable name="gcReportSectionKey_SET" select="'SET'"/>
  <!-- Unsetttled Trades -->
  <xsl:variable name="gcReportSectionKey_UNS" select="'UNS'"/>
  <!-- PurchaseAndSales -->
  <xsl:variable name="gcReportSectionKey_PSS" select="'PSS'"/>
  <!-- ClosedPositions -->
  <xsl:variable name="gcReportSectionKey_CLO" select="'CLO'"/>
  <!-- Open position Business -->
  <xsl:variable name="gcReportSectionKey_POS" select="'POS'"/>
  <!-- ClosedPositions -->
  <xsl:variable name="gcReportSectionKey_CST" select="'CST'"/>
  <!-- Open position settlement -->
  <xsl:variable name="gcReportSectionKey_STL" select="'STL'"/>
  <!-- Journal Entries -->
  <xsl:variable name="gcReportSectionKey_JNL" select="'JNL'"/>
  <!-- Collateral (Securities on deposit)-->
  <!-- FI 20160530 [21885] Add-->
  <xsl:variable name="gcReportSectionKey_SOD" select="'SOD'"/>
  <!-- UnderlyingStock (Underlying Stocks on deposit) -->
  <!-- FI 20160613 [22256] Add-->
  <xsl:variable name="gcReportSectionKey_UOD" select="'UOD'"/>
  <!-- Account summary -->
  <xsl:variable name="gcReportSectionKey_ACC" select="'ACC'"/>

  <xsl:variable name="gcBlockAccountFinancialSummary" select="'{AccountFinancialSummary}'"/>

  <xsl:variable name="gcPurchaseSaleOverallQty" select="'OverallQty'"/>
  <xsl:variable name="gcPurchaseSalePnLOnClosing" select="'PnLOnClosing'"/>

  <xsl:variable name="gcLegend_FeesExclTax" select="'(1)'"/>
  <xsl:variable name="gcLegend_RealisedPnL" select="'(2)'"/>

  <xsl:variable name="gcSpace"> </xsl:variable>
  <xsl:variable name="gcTab"> </xsl:variable>

  <xsl:variable name="gcWhite">white</xsl:variable>

  <xsl:variable name="gcDesignedBy">Designed by</xsl:variable>
  <xsl:variable name="gcDesignedBy2">designed by</xsl:variable>
  <xsl:variable name="gcPowerdBy">Powered by</xsl:variable>
  <xsl:variable name="gcPowerdBy2">powered by</xsl:variable>
  <xsl:variable name="gcSpheres">Spheres</xsl:variable>
  <xsl:variable name="gcCopyrightEFS">© 2023 EFS</xsl:variable>

  <!-- ============================================================================================== -->
  <!--                                         Business                                               -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gcShort" select="'Short'"/>
  <xsl:variable name="gcLong" select="'Long'"/>
  <xsl:variable name="gcFlat" select="'Flat'"/>

  <xsl:variable name="gcCredit" select="'CR'"/>
  <xsl:variable name="gcDebit" select="'DR'"/>

  <xsl:variable name="gcPrefixCcyId" select="'CURRENCY.ISO4217_ALPHA3.'"/>

  <!-- ============================================================================================== -->
  <!--                                         XML flow                                               -->
  <!-- ============================================================================================== -->
  <!--header-->
  <xsl:variable name="gHeader" select="confirmationMessage/header"/>
  <xsl:variable name="gSettings-headerFooter" select="$gHeader/reportSettings/headerFooter"/>
  <xsl:variable name="gSendBy-routingId" select="$gHeader/sendBy/routingIdsAndExplicitDetails/routingIds/routingId"/>
  <xsl:variable name="gSendBy-routingAddress" select="$gHeader/sendBy/routingIdsAndExplicitDetails/routingAddress"/>
  <xsl:variable name="gSendTo-routingAddress" select="$gHeader/sendTo/routingIdsAndExplicitDetails/routingAddress"/>
  <xsl:variable name="gSettings-section" select="$gSettings-headerFooter/sectionTitle/section"/>

  <!--dataDocument-->
  <xsl:variable name="gDataDocument" select="confirmationMessage/dataDocument"/>
  <xsl:variable name="gParty" select="$gDataDocument/party"/>
  <xsl:variable name="gTrade" select="$gDataDocument/trade"/>
  <xsl:variable name="gCBTrade" select="$gTrade[cashBalanceReport][1]"/>

  <xsl:variable name="gDailyTrades" select="confirmationMessage/trades"/>
  <xsl:variable name="gDlvTrades" select="confirmationMessage/dlvTrades"/>
  <xsl:variable name="gUnsettledTrades" select="confirmationMessage/unsettledTrades"/>
  <xsl:variable name="gSettledTrades" select="confirmationMessage/settledTrades"/>
  <xsl:variable name="gPosActions" select="confirmationMessage/posActions"/>
  <xsl:variable name="gStlPosActions" select="confirmationMessage/stlPosActions"/>
  <xsl:variable name="gPosTrades" select="confirmationMessage/posTrades"/>
  <xsl:variable name="gStlPosTrades" select="confirmationMessage/stlPosTrades"/>
  <xsl:variable name="gPosSynthetics" select="confirmationMessage/posSynthetics"/>
  <xsl:variable name="gStlPosSynthetics" select="confirmationMessage/stlPosSynthetics"/>
  <xsl:variable name="gCashPayments" select="confirmationMessage/cashPayments"/>
  <!--FI 20160530 [21885] Add gCollaterals-->
  <xsl:variable name="gCollaterals" select="confirmationMessage/collaterals"/>
  <!--FI 20160613 [22256] Add gUnderlyingStocks-->
  <xsl:variable name="gUnderlyingStocks" select="confirmationMessage/underlyingStocks"/>

  <xsl:variable name="gCommonData" select="confirmationMessage/commonData"/>

  <!--repository-->
  <xsl:variable name="gRepository" select="confirmationMessage/repository"/>

  <xsl:variable name="gEntityId" select="normalize-space($gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid'])" />
  <xsl:variable name="gReportingCcy" select="$gHeader/reportSettings/reportingCurrency"/>

  <!--Il existe des instruments marginés-->
  <xsl:variable name="gIsMarginingActivity" select="$gRepository/instr[gProduct='FUT' or isMargining='true'] = true()"/>
  <!--Il existe des trades equitySecurityTransaction-->
  <xsl:variable name="gIsEquityActivity" select="$gRepository/instr[product='equitySecurityTransaction'] = true()"/>
  <!--Il existe des trades DebtSecurityTransaction-->
  <xsl:variable name="gIsDebtSecurityActivity" select="$gRepository/instr[product='debtSecurityTransaction'] = true()"/>
  <!--Il existe des trades equitySecurityTransaction ou debtSecurityTransaction-->
  <xsl:variable name="gIsSecurityActivity" select ="$gIsDebtSecurityActivity or $gIsEquityActivity"/>
  <!--Il existe des trades commoditySpot-->
  <xsl:variable name="gIsCOMActivity" select="$gRepository/instr[product='commoditySpot'] = true()"/>

  <!--Il existe des trades Equity-->
  <xsl:variable name="gIsETDActivity" select="$gRepository/instr[product='exchangeTradedDerivative'] = true()"/>

  <!--Il existe des trades option ETD ou bien des FxOption-->
  <xsl:variable name="gIsOptionActivity" select="$gTrade[exchangeTradedDerivative/Category/text()='O' or fxSimpleOption] = true()"/>

  <!-- .......................................................................... -->
  <!--              Display Settings                                              -->
  <!-- .......................................................................... -->
  <!--Amount-->
  <xsl:variable name="gAmount-format">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/amount/@format) > 0">
        <xsl:value-of select="$gSettings-headerFooter/amount/@format"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'Default'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAmountDR-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/amount/negativeValue/@foreColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/amount/negativeValue/@foreColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'darkred'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAmountCR-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/amount/positiveValue/@foreColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/amount/positiveValue/@foreColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'green'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAmountDR-background-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/amount/negativeValue/@backColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/amount/negativeValue/@backColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'#eee0e5'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAmountCR-background-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/amount/positiveValue/@backColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/amount/positiveValue/@backColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'#e0eee0'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAmountZero-background-color">
    <xsl:if test="string-length($gAmountCR-background-color) > 0">
      <xsl:value-of select="'#e6e6e6'"/>
    </xsl:if>
  </xsl:variable>

  <!--Model-->
  <xsl:variable name="gPurchaseSaleModel" select="$gcPurchaseSaleOverallQty"/>
  <!--<xsl:variable name="gPurchaseSaleModel" select="$gcPurchaseSalePnLOnClosing"/>-->

  <!--NoActivityMsg-->
  <xsl:variable name="gNoActivityMsg-font-size">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/noActivityMsg/@fontsize) > 0">
        <xsl:value-of select="concat($gSettings-headerFooter/noActivityMsg/@fontsize,'pt')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'10pt'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gNoActivityMsg-color">
    <xsl:call-template name="Lower">
      <xsl:with-param name="source" select="$gSettings-headerFooter/noActivityMsg/@foreColor"/>
    </xsl:call-template>
  </xsl:variable>

  <!--SectionBanner-->
  <xsl:variable name="gSectionBanner-font-size">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/sectionBanner/@fontsize) > 0">
        <xsl:value-of select="concat($gSettings-headerFooter/sectionBanner/@fontsize,'pt')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'10pt'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSectionBanner-text-align">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/sectionBanner/@align) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/sectionBanner/@align"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'left'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSectionBanner-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/sectionBanner/@foreColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/sectionBanner/@foreColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'white'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSectionBanner-background-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/sectionBanner/@backColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/sectionBanner/@backColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!--<xsl:value-of select="'#27408B'"/>-->
        <xsl:value-of select="'#045FB4'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSectionBanner-style">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/sectionBanner/@style) > 0">
        <xsl:value-of select="$gSettings-headerFooter/sectionBanner/@style"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'Banner'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gSectionBanner-font-weight" select="'bold'"/>

  <!--DateBanner-->
  <xsl:variable name="gDateBanner-font-size" select="'7pt'"/>
  <xsl:variable name="gDateBanner-font-weight" select="'normal'"/>
  <xsl:variable name="gDateBanner-text-align" select="'left'"/>
  <xsl:variable name="gDateBanner-color" select="'black'"/>
  <xsl:variable name="gDateBanner-background-color" select="'#CAE1FF'"/>

  <!--AssetBanner-->
  <xsl:variable name="gAssetBanner-font-size">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/assetBanner/@fontsize) > 0">
        <xsl:value-of select="concat($gSettings-headerFooter/assetBanner/@fontsize,'pt')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'7pt'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAssetBanner-text-align">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/assetBanner/@align) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/assetBanner/@align"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'left'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAssetBanner-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/assetBanner/@foreColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/assetBanner/@foreColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'#00008B'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAssetBanner-background-color">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/assetBanner/@backColor) > 0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/assetBanner/@backColor"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'#e6e6e6'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gAssetBanner-font-weight" select="'bold'"/>

  <!--ExpiryIndicator-->
  <xsl:variable name="gExpiryIndicator-days">
    <xsl:if test="$gSettings-headerFooter and string-length($gSettings-headerFooter/expiryIndicator/@days) > 0">
      <xsl:value-of select="$gSettings-headerFooter/expiryIndicator/@days"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="gExpiryIndicator-color">
    <xsl:if test="$gSettings-headerFooter and string-length($gSettings-headerFooter/expiryIndicator/@foreColor) > 0">
      <xsl:call-template name="Lower">
        <xsl:with-param name="source" select="$gSettings-headerFooter/expiryIndicator/@foreColor"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:variable>

  <!--Timestamp-->
  <xsl:variable name="gTimestamp_Type">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/timestampType) > 0">
        <xsl:value-of select="$gSettings-headerFooter/timestampType"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'HH24:MI:SS'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gIsDisplayTimestamp" select="$gTimestamp_Type!='None'"/>

  <!--ISIN Code-->
  <xsl:variable name="gISINCode_Type">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/isinCode) > 0">
        <xsl:value-of select="$gSettings-headerFooter/isinCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'1'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!--BBG Code-->
  <xsl:variable name="gBBGCode_Type">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/bbgCode) > 0">
        <xsl:value-of select="$gSettings-headerFooter/bbgCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'2'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!--RIC Code-->
  <xsl:variable name="gRICCode_Type">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/ricCode) > 0">
        <xsl:value-of select="$gSettings-headerFooter/ricCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'3'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!--Countervalue-->
  <xsl:variable name="gIsDisplayCountervalue">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter/totalInBaseCurrency">
        <xsl:choose>
          <xsl:when test="$gSettings-headerFooter/totalInBaseCurrency/text() = 'Y'">
            <xsl:value-of select="'true'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'false'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'false'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- ============================================================================================== -->
  <!--                                         Keys                                                   -->
  <!-- ============================================================================================== -->
  <xsl:key name="kOTCmlId" match="*" use="@OTCmlId"/>
  <xsl:key name="kCcy" match="*" use="@ccy"/>

  <xsl:key name="kTradeId" match="*" use="@tradeId"/>
  <!-- FI 20151104 [21311] add clé kTrade2-TradeId-->
  <xsl:key name="kTrade2-TradeId" match="trade2" use="@tradeId"/>

  <xsl:key name="kFee-paymentType" match="fee|safekeeping" use="@paymentType"/>
  <xsl:key name="kTax-taxType" match="tax" use="@taxType"/>
  <xsl:key name="kTax-taxId" match="tax" use="@taxId"/>
  <xsl:key name="kTax-rate" match="tax" use="@rate"/>

  <xsl:key name="kInstrmt-ID" match="Instrmt" use="@ID"/>
  <xsl:key name="kRptSide-Acct" match="RptSide" use="@Acct"/>

  <xsl:key name="kSubTotal-idB" match="subTotal" use="@idB"/>
  <xsl:key name="kSubTotal-idI" match="subTotal" use="@idI"/>
  <xsl:key name="kSubTotal-idAsset" match="subTotal" use="@idAsset"/>

  <!-- RD 20190619 [23912] kTrader-traderId -->
  <xsl:key name="kTrader-traderId" match="trader" use="@traderId"/>

  <xsl:key name="kCashPayment-eventType" match="cashPayment" use="@eventType"/>

  <!-- FI 20151019 [21317] add kValDt, kProduct-idI, kAssetCategory -->
  <xsl:key name="kAssetCategory" match="*" use="@assetCategory"/>
  <xsl:key name="kValDt" match="*" use="@valDt"/>
  <xsl:key name="kProduct-idI" match="productType" use="@OTCmlId"/>

  <!-- FI 20150126 [21825] Add ketd-ContratMultiplier -->
  <xsl:key name="ketd-ContratMultiplier" match="etd" use="contractMultiplier/text()"/>


  <!-- ============================================================================================== -->
  <!--                                         Design                                                 -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gIsColorMode" select="true()"/>
  <!-- Admitted values: Times Roman, Helvetica, and Courier -->
  <xsl:variable name="gFontFamily">Helvetica</xsl:variable>

  <!-- .......................................................................... -->
  <!--              Header                                                        -->
  <!-- .......................................................................... -->
  <xsl:variable name="gHeaderText_Color">gray</xsl:variable>
  <xsl:variable name="gHeader_FontSize">16pt</xsl:variable>
  <xsl:variable name="gHeaderDetail_FontSize">7pt</xsl:variable>
  <xsl:variable name="gHeaderTextCustom_FontSize">7pt</xsl:variable>
  <xsl:variable name="gHeaderLogo_Height">15mm</xsl:variable>
  <xsl:variable name="gHeaderAdresse_Width">40mm</xsl:variable>

  <!-- Get logo of the entity -->
  <xsl:variable name="gImgLogo" select="concat('sql(select IDENTIFIER, LOLOGO from dbo.ACTOR where IDA=', $gEntityId, ')')"/>

  <!-- .......................................................................... -->
  <!--              Footer                                                        -->
  <!-- .......................................................................... -->
  <xsl:variable name="gFooterText_Color">black</xsl:variable>
  <xsl:variable name="gFooterText_FontSize">8pt</xsl:variable>
  <xsl:variable name="gFooterTextCustom_FontSize">7pt</xsl:variable>

  <!--Copyright: Powered by Spheres - © 2024 EFS-->
  <xsl:variable name="gCopyright" select="concat($gcPowerdBy,$gcSpace,$gcSpheres,$gcSpace,'-',$gcSpace,$gcCopyrightEFS)"/>

  <!-- .......................................................................... -->
  <!--              Body                                                          -->
  <!-- .......................................................................... -->
  <!--Report Title-->
  <xsl:variable name="gTitle_space-before">10mm</xsl:variable>
  <xsl:variable name="gTitle_text-align">center</xsl:variable>
  <xsl:variable name="gTitle_font-size">16pt</xsl:variable>
  <xsl:variable name="gTitle_font-weight">bold</xsl:variable>

  <!--Receiver Adress-->
  <xsl:variable name="gReceiverAdress_space-before">10mm</xsl:variable>
  <xsl:variable name="gReceiverAdress_margin-left">120mm</xsl:variable>
  <xsl:variable name="gReceiverAdress_text-align">left</xsl:variable>
  <xsl:variable name="gReceiverAdress_font-family">Courier</xsl:variable>
  <xsl:variable name="gReceiverAdress_font-size">11pt</xsl:variable>

  <xsl:variable name="gDisplaySettingsNode">
    <!--Cancel Message-->
    <block name="CancelMessage"
           msg_column-width="60mm"
           space_column-width="60mm"
           font-weight="bold"
           space-before="10mm"
           text-align="center"
           display-align="center"
           padding="2pt"
           isPreviousCreationTimestamp="{$gHeader/previousCreationTimestamp = true()}">

      <xsl:attribute name="font-size">
        <xsl:choose>
          <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/hCancelMsg/@fontsize) > 0">
            <xsl:value-of select="concat($gSettings-headerFooter/hCancelMsg/@fontsize,'pt')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'10pt'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:variable name="vColor">
        <xsl:choose>
          <xsl:when test="$gSettings-headerFooter and string-length($gSettings-headerFooter/hCancelMsg/@foreColor) > 0">
            <xsl:call-template name="Lower">
              <xsl:with-param name="source" select="$gSettings-headerFooter/hCancelMsg/@foreColor"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'darkred'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:attribute name="color">
        <xsl:value-of select="$vColor"/>
      </xsl:attribute>
      <xsl:attribute name="border">
        <xsl:value-of select="concat('1pt solid ',$vColor)"/>
      </xsl:attribute>

      <xsl:if test="$gSettings-headerFooter and string-length($gSettings-headerFooter/hCancelMsg/text()) > 0">
        <xsl:value-of select="$gSettings-headerFooter/hCancelMsg/text()"/>
      </xsl:if>
    </block>
    <!--Business details-->
    <block name="BusinessDetail" space-before="10mm" font-size="10pt" display-align="before">
      <column name="Ident" column-width="27mm">
        <data length="15" font-weight="normal" text-align="left"/>
        <data name="Date">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-ClearingDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Dates">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ClearingDates'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <column name="Ident2" column-width="17mm">
        <data length="10"/>
      </column>
      <!-- EG 20160308 Migration vs2013 ok-->
      <!--<column name="Data">-->
      <column name="Data" column-width="156mm">
        <data font-weight="bold" text-align="left"/>
        <data name="Det" font-size="8pt" font-weight="normal"/>
        <data name="To" font-weight="normal">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-To'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
    </block>
  </xsl:variable>
  <xsl:variable name="gDisplaySettings" select="msxsl:node-set($gDisplaySettingsNode)"/>
  <xsl:variable name="gDisplaySettings-CancelMessage" select="$gDisplaySettings/block[@name='CancelMessage']"/>
  <!-- FI 20150728 A VOIR avec RD le block BusinessDetail a priori n'existe plus (cas de l'éate de synthèse)  -->
  <xsl:variable name="gDisplaySettings-BusinessDetail" select="$gDisplaySettings/block[@name='BusinessDetail']"/>

  <!--Report Introduction and last message (Header and Footer Formula)-->
  <xsl:variable name="gFormula_space-before">5mm</xsl:variable>
  <xsl:variable name="gFormula_text-align">justify</xsl:variable>
  <xsl:variable name="gFormula_font-size">7pt</xsl:variable>
  <xsl:variable name="gFormula_font-weight">normal</xsl:variable>

  <!--Report Legend-->
  <xsl:variable name="gLegend_space-before">5mm</xsl:variable>
  <xsl:variable name="gLegendTable_space-after">0.5mm</xsl:variable>
  <xsl:variable name="gLegendTable_padding">1pt</xsl:variable>
  <xsl:variable name="gLegend_text-align">justify</xsl:variable>
  <xsl:variable name="gLegend_font-size">7pt</xsl:variable>
  <xsl:variable name="gLegend_font-weight">normal</xsl:variable>
  <xsl:variable name="gLegend_border">0.5pt solid black</xsl:variable>
  <xsl:variable name="gLegendLine_width">80mm</xsl:variable>
</xsl:stylesheet>
