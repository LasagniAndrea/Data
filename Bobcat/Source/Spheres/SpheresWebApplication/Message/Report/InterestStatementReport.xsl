<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
Summary : Spheres report - Cash Interest Report 
          XSL for generation PDF document of Cash Interest Report
          
File    : InterestStatementReport.xsl
================================================================================================================
Version : v5.1.6078                                           
Date    : 20160822
Author  : RD
Comment : [22410] 
          - Correction sur intérêts avec taux negatif
================================================================================================================
Version : v4.5.5791                                           
Date    : 20151109
Author  : RD
Comment : [21186] 
          - Correction sur intérêts débiteurs et créditeurs
          - Affichage de tous les décimales sur les taux 
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
================================================================================================================
Version : v2.0.0.0 (Spheres 3.7.0.0)
Date    : 20130820 
Author  : RD & PM
Comment : [18582] Ajout du seuil de déclenchement
================================================================================================================
Version : v1.0.0.0 (Spheres 2.7.0.0)
Date    : 20121001
Author  : RD
Comment : First version
================================================================================================================  
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared/Shared_Report_Main.xslt" />

  <!-- ================================================== -->
  <!--        Parameters                                  -->
  <!-- ================================================== -->
  <xsl:param name="pCurrentCulture" select="'fr-FR'" />

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <xsl:variable name="gReportType" select="$gcReportTypeINTSTAT"/>

  <xsl:variable name="gDataDocumentTrades" select="//dataDocument/trade"/>
  <xsl:variable name="gDataDocumentRepository" select="//dataDocument/repository"/>
  <xsl:variable name="gBusinessBooks">
    <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade/BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>
  </xsl:variable>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ================================================== -->
  <!--        Global Variables : Display                  -->
  <!-- ================================================== -->
  <!-- ===== Display settings of all Blocks===== -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name="gDetTrdTableTotalWidth">197mm</xsl:variable>

  <!-- Report model, containing the columns/rows definition -->
  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="CBIBlock" padding-top="{$gGroupPaddingTop}">
      <!-- EG 20160404 Migration vs2013 -->
      <title sort="1" empty-row="1mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-weight="bold">
        <column number="01" name="TS-CurrencyTitle" colspan="9">
          <data font-size="10"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
        <column number="02" name="TS-FxRateTitle" colspan="4">
          <data text-align="right" display-align="center" font-size="7"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
      </title>
      <title sort="2" withColumnHdr="{true()}" withSubColumn="{false()}" font-size="7">
        <column number="01" name="TS-RateTitle" colspan="4">
          <header ressource=" " background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="02" name="TS-DebitBalanceTitle" colspan="3">
          <header ressource="TS-DebitBalance" font-weight="bold" text-align="center"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" text-align="center" border-bottom-style="solid"/>
        </column>
        <column number="03" name="TS-CreditBalanceTitle" colspan="3">
          <header ressource="TS-CreditBalance" font-weight="bold" text-align="center"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"/>
          <data font-weight="bold" text-align="center" border-bottom-style="solid"/>
        </column>
      </title>
      <!-- EG 20160404 Migration vs2013 -->
      <title sort="3" empty-row="1mm" withSubColumn="{false()}" font-size="7">
        <column number="01" name="TS-ThresholdTitle" colspan="4">
          <data font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="02" name="TS-DrThresholdTitle" colspan="3">
          <data text-align="right" decPattern="%%PAT:currency.PARENT%%"
                font-weight="bold" border-bottom-style="solid"/>
        </column>
        <column number="03" name="TS-CrThresholdTitle" colspan="3">
          <data text-align="right" decPattern="%%PAT:currency.PARENT%%"
                font-weight="bold" border-bottom-style="solid"/>
        </column>
      </title>

      <column number="01" name="TS-ValueDate" width="18mm">
        <header font-weight="bold" text-align="center"/>
        <data font-weight="bold" text-align="center"
              border-bottom-style="solid"/>
      </column>
      <column number="02" name="Report-CashBalance" width="33mm">
        <header ressource="%%RES:Report-CashBalance%%" colspan="2" font-weight="bold" text-align="center"/>
        <data text-align="right" decPattern="%%PAT:currency.PARENT%%"
              border-right-style="none" border-bottom-style="solid"/>
      </column>
      <column number="03" name="Report-CashBalanceDrCr" width="3.5mm">
        <header colspan="-1"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"
              border-left-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="04" name="TS-Rate" width="33mm">
        <header font-weight="bold" text-align="center">
          <inline>
            <xsl:value-of select="concat($gcEspace, '(%)')"/>
          </inline>
        </header>
        <data text-align="right"
              border-bottom-style="solid"/>
      </column>
      <column number="05" name="TS-DrInterestCurrency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="06" name="TS-DrInterest" width="24.5mm">
        <header colspan="3" font-weight="bold"/>
        <data text-align="right" decPattern="%%PAT:currency.PARENT%%"
              border-left-style="none"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="07" name="TS-DrInterestDrCr" width="3.5mm">
        <header colspan="-1"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"
              border-left-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="08" name="TS-CrInterestCurrency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="09" name="TS-CrInterest" width="24.5mm">
        <header colspan="3" font-weight="bold"/>
        <data text-align="right" decPattern="%%PAT:currency.PARENT%%"
              border-left-style="none"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="10" name="TS-CrInterestDrCr" width="3.5mm">
        <header colspan="-1"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"
              border-left-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="11" name="TS-ExCurrency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="12" name="TS-ExInterest" width="26mm">
        <header ressource="CSHBAL-CounterValueTitle"
                text-align="center" colspan="3" font-weight="bold">
          <inline>
            <xsl:value-of select="concat($gcEspace, $gReportingCurrency)"/>
          </inline>
        </header>
        <data text-align="right" decPattern="%%PAT:RepCurrency%%"
              border-left-style="none"
              border-right-style="none"
              border-bottom-style="solid"/>
      </column>
      <column number="13" name="TS-ExInterestDrCr" width="3.5mm">
        <header colspan="-1"/>
        <data display-align="center" font-size="{$gDetTrdDetFontSize}"
              border-left-style="none"
              border-bottom-style="solid"/>
      </column>
    </group>
  </xsl:variable>
  <!-- ===== SubTotals settings of principal Block (ETDBlock) ===== -->
  <xsl:variable name="gColSubTotalColDisplaySettings">
    <group name="CBIBlock">
      <row sort="1">
        <subtotal sort="1" name="TS-SubTotInterest" columns-spanned="4" text-align="left" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="solid"/>
        <subtotal sort="2" name="TS-SubTotDrIntCurrency" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="none"/>
        <subtotal sort="3" name="TS-SubTotDrIntAmount" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="4" name="TS-SubTotDrIntAmountDrCr" display-align="center" text-align="left" font-size="5pt" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="5" name="TS-SubTotCrIntCurrency" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="none"/>
        <subtotal sort="6" name="TS-SubTotCrIntAmount" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="7" name="TS-SubTotCrIntAmountDrCr" display-align="center" text-align="left" font-size="5pt" font-weight="bold"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="8" name="TS-Filler" columns-spanned="3"
                  background-color="{$gDetTrdHdrCellBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="solid"/>
      </row>
      <row sort="2">
        <subtotal sort="1" name="TS-SubTotDeltaInterest" columns-spanned="4" text-align="left" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="solid"/>
        <subtotal sort="2" name="TS-SubTotDeltaDrIntCurrency" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="none"/>
        <subtotal sort="3" name="TS-SubTotDeltaDrIntAmount" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="4" name="TS-SubTotDeltaDrIntDrCr" display-align="center" text-align="left" font-size="5pt" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="5" name="TS-SubTotDeltaCrIntCurrency" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="none"/>
        <subtotal sort="6" name="TS-SubTotDeltaCrIntAmount" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="7" name="TS-SubTotDeltaCrIntDrCr" display-align="center" text-align="left" font-size="5pt" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
        <subtotal sort="8" name="TS-SubTotDeltaIntExCurrency" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="solid"
                  border-right-style="none"/>
        <subtotal sort="9" name="TS-SubTotDeltaIntExAmount" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="none"/>
        <subtotal sort="10" name="TS-SubTotDeltaIntExDrCr" display-align="center" text-align="left" font-size="5pt" font-weight="bold"
                  background-color="{$gDetTrdTotalBackgroundColor}"
                  border-bottom-style="solid"
                  border-top-style="solid"
                  border-left-style="none"
                  border-right-style="solid"/>
      </row>
    </group>
  </xsl:variable>
  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->
  <!--Affichage du trade dans le Header du report-->
  <xsl:template name="GetTradeHeader"/>
  <!-- SetPagesCaracteristicsSpecific template -->
  <xsl:template name="SetPagesCaracteristicsSpecific"/>
  <!--DisplayReceiverDetailSpecific template-->
  <xsl:template name="DisplayReceiverDetailSpecific">
    <xsl:param name="pBook" />
    <xsl:call-template name="DisplayReceiverDetailCommon">
      <xsl:with-param name="pBook" select="$pBook" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/">
    <!-- Le template main est commun à tous les reports, il est dans: Shared/Shared_Report_Main.xslt -->
    <xsl:call-template name="Main" />
  </xsl:template>

  <!-- ================================================== -->
  <!--        Business Template                           -->
  <!-- ================================================== -->
  <!--Trade templates-->
  <xsl:template match="trade" mode="business">
    <xsl:variable name="vTradeId" select="string(./tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid' and string-length(text())>0 and text()!='0']/text())"/>
    <xsl:variable name="vIDB" select="number(./tradeHeader/partyTradeIdentifier/bookId[@bookIdScheme='http://www.euro-finance-systems.fr/otcml/bookid']/@OTCmlId)"/>
    <xsl:variable name="vBook" select="string($gDataDocumentRepository/book[@OTCmlId=$vIDB]/identifier/text())"/>

    <xsl:variable name="vStartDate" select="string(./cashBalanceInterest/cashBalanceInterestStream[1]/calculationPeriodDates/effectiveDate/unadjustedDate/text())"/>
    <xsl:variable name="vCurrency" select="string(./cashBalanceInterest/cashBalanceInterestStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency/text())"/>
    <xsl:variable name="vCurrencyFxRate">
      <xsl:call-template name="FxRate">
        <xsl:with-param name="pFlowCurrency" select="$vCurrency" />
        <xsl:with-param name="pExCurrency" select="$gReportingCurrency" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vPrefixedCurrencyId" select="concat($gPrefixCurrencyId, $vCurrency)"/>
    <xsl:variable name="vCurrencyRepository" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]"/>
    <xsl:variable name="vCurrencyPattern">
      <xsl:call-template name="GetDecPattern">
        <xsl:with-param name="pRoundDir" select="$vCurrencyRepository/rounddir"/>
        <xsl:with-param name="pRoundPrec" select="$vCurrencyRepository/roundprec"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vPrefixedRepCurrencyId" select="concat($gPrefixCurrencyId, $gReportingCurrency)"/>
    <xsl:variable name="vReptCurrencyRepository" select="$gDataDocumentRepository/currency[@id=$vPrefixedRepCurrencyId]"/>
    <xsl:variable name="vReptCurrencyPattern">
      <xsl:call-template name="GetDecPattern">
        <xsl:with-param name="pRoundDir" select="$vReptCurrencyRepository/rounddir"/>
        <xsl:with-param name="pRoundPrec" select="$vReptCurrencyRepository/roundprec"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vDebitInterestEvent" select="$gEvents//Event[EVENTCODE='CIS' and (EVENTTYPE='FLO' or EVENTTYPE='FIX') and IDB_PAY=$vIDB]/Event[EVENTCODE='TER' and EVENTTYPE='INT']"/>
    <xsl:variable name="vCreditInterestEvent" select="$gEvents//Event[EVENTCODE='CIS' and (EVENTTYPE='FLO' or EVENTTYPE='FIX') and IDB_REC=$vIDB]/Event[EVENTCODE='TER' and EVENTTYPE='INT']"/>
    <xsl:variable name="vIsCashCoveredInitialMargin" select="./cashBalanceInterest/cashBalanceInterestStream[@id='CashCoveredInitialMargin'] = true()"/>

    <trade id="{$vTradeId}"
           startDate="{$vStartDate}"
           endDate="{string(./tradeHeader/tradeDate/text())}"
           isCashCoveredInitialMargin="{$vIsCashCoveredInitialMargin}">
      
      <!--Book-->
      <BOOK data="{$vBook}"/>
      <currency currency="{$vCurrency}" desc="{$vCurrencyRepository/description}" pattern="{$vCurrencyPattern}"/>
      <exCurrency currency="{$gReportingCurrency}" pattern="{$vReptCurrencyPattern}" fxRate="{$vCurrencyFxRate}"/>

      <!-- RD 20130926 [18582] Gérer CashCoveredInitialMargin-->
      <xsl:variable name="vDebitStream" select="./cashBalanceInterest/cashBalanceInterestStream[@id='DebitCashBalance']"/>
      <xsl:variable name="vCreditStream" select="./cashBalanceInterest/cashBalanceInterestStream[@id='CreditCashBalance' or @id='CashCoveredInitialMargin']"/>

      <xsl:variable name="vDebitCalculation" select="$vDebitStream/calculationPeriodAmount/calculation"/>
      <xsl:variable name="vCreditCalculation" select="$vCreditStream/calculationPeriodAmount/calculation"/>

      <!--Debit used interest rate-->
      <xsl:call-template name="GetUsedInterestRate">
        <xsl:with-param name="pRateCalculation" select="$vDebitCalculation"/>
        <xsl:with-param name="pDrCr" select="$gcDebit"/>
      </xsl:call-template>
      <!--Credit used interest rate-->
      <xsl:call-template name="GetUsedInterestRate">
        <xsl:with-param name="pRateCalculation" select="$vCreditCalculation"/>
        <xsl:with-param name="pDrCr" select="$gcCredit"/>
      </xsl:call-template>

      <xsl:if test="$vDebitStream/threshold">
        <threshold notionalDrCr="{$gcDebit}" amount="{$vDebitStream/threshold/amount/text()}"/>
      </xsl:if>

      <xsl:if test="$vCreditStream/threshold">
        <threshold notionalDrCr="{$gcCredit}" amount="{$vCreditStream/threshold/amount/text()}"/>
      </xsl:if>

      <interest notionalDrCr="{$gcDebit}" currency="{$vCurrency}" amount="{$vDebitInterestEvent/VALORISATION}">
        <xsl:attribute name="drCr">
          <xsl:choose>
            <xsl:when test="$vDebitInterestEvent/IDB_PAY = $vIDB">
              <xsl:value-of select="$gcDebit"/>
            </xsl:when>
            <xsl:when test="$vDebitInterestEvent/IDB_REC = $vIDB">
              <xsl:value-of select="$gcCredit"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </interest>
      <interest notionalDrCr="{$gcCredit}" currency="{$vCurrency}" amount="{$vCreditInterestEvent/VALORISATION}">
        <xsl:attribute name="drCr">
          <xsl:choose>
            <xsl:when test="$vCreditInterestEvent/IDB_PAY = $vIDB">
              <xsl:value-of select="$gcDebit"/>
            </xsl:when>
            <xsl:when test="$vCreditInterestEvent/IDB_REC = $vIDB">
              <xsl:value-of select="$gcCredit"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </interest>

      <xsl:variable name="vDebitNotionalStepSchedule" select="$vDebitCalculation/notionalSchedule/notionalStepSchedule"/>
      <xsl:variable name="vCreditNotionalStepSchedule" select="$vCreditCalculation/notionalSchedule/notionalStepSchedule"/>

      <xsl:variable name="vStepsNode">
        <!--<steps>-->
        <!--le premier jour-->
        <xsl:call-template name="GetStep">
          <xsl:with-param name="pStepDate" select="$vStartDate"/>
          <xsl:with-param name="pStepDebitNotional" select="number($vDebitNotionalStepSchedule/initialValue/text())"/>
          <xsl:with-param name="pStepCreditNotional" select="number($vCreditNotionalStepSchedule/initialValue/text())"/>
          <xsl:with-param name="pDebitInterestEvent" select="$vDebitInterestEvent"/>
          <xsl:with-param name="pCreditInterestEvent" select="$vCreditInterestEvent"/>
          <xsl:with-param name="pDebitCalculation" select="$vDebitCalculation"/>
          <xsl:with-param name="pCreditCalculation" select="$vCreditCalculation"/>
          <xsl:with-param name="pIDB" select="$vIDB"/>
          <xsl:with-param name="pCurrencyFxRate" select="$vCurrencyFxRate"/>
        </xsl:call-template>

        <xsl:for-each select="./cashBalanceInterest/cashBalanceInterestStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">

          <xsl:call-template name="GetStep">
            <xsl:with-param name="pStepDate" select="current()/stepDate"/>
            <xsl:with-param name="pStepDebitNotional" select="number($vDebitNotionalStepSchedule/step[stepDate/text() = current()/stepDate/text()]/stepValue/text())"/>
            <xsl:with-param name="pStepCreditNotional" select="number($vCreditNotionalStepSchedule/step[stepDate/text() = current()/stepDate/text()]/stepValue/text())"/>
            <xsl:with-param name="pDebitInterestEvent" select="$vDebitInterestEvent"/>
            <xsl:with-param name="pCreditInterestEvent" select="$vCreditInterestEvent"/>
            <xsl:with-param name="pDebitCalculation" select="$vDebitCalculation"/>
            <xsl:with-param name="pCreditCalculation" select="$vCreditCalculation"/>
            <xsl:with-param name="pIDB" select="$vIDB"/>
            <xsl:with-param name="pCurrencyFxRate" select="$vCurrencyFxRate"/>
          </xsl:call-template>
        </xsl:for-each>
        <!--</steps>-->
      </xsl:variable>

      <xsl:variable name="vSteps" select="msxsl:node-set($vStepsNode)/step"/>
      <xsl:variable name="vRatePattern">
        <xsl:call-template name="GetPricesPattern">
          <xsl:with-param name="pPrices" select="$vSteps/@effectiveRate"/>
        </xsl:call-template>
      </xsl:variable>

      <steps ratePattern="{$vRatePattern}">
        <xsl:copy-of select="$vSteps"/>
      </steps>
    </trade>
  </xsl:template>
  <!--GetStep-->
  <xsl:template name="GetStep">
    <xsl:param name="pStepDate"/>
    <xsl:param name="pStepDebitNotional"/>
    <xsl:param name="pStepCreditNotional"/>
    <xsl:param name="pDebitInterestEvent"/>
    <xsl:param name="pCreditInterestEvent"/>
    <xsl:param name="pDebitCalculation"/>
    <xsl:param name="pCreditCalculation"/>
    <xsl:param name="pIDB"/>
    <xsl:param name="pCurrencyFxRate"/>

    <xsl:variable name="vStepNotional">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:value-of select="$pStepDebitNotional"/>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:value-of select="$pStepCreditNotional"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vStepNotionalDrCr">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:value-of select="$gcDebit"/>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:value-of select="$gcCredit"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vStepDebitInterestEvent" select="$pDebitInterestEvent/Event[EVENTCODE='PER' and (EVENTTYPE='FLO' or EVENTTYPE='FIX') and (EventClass[EVENTCLASS='GRP' and substring(DTEVENT,1,10)=$pStepDate])]"/>
    <xsl:variable name="vStepCreditInterestEvent" select="$pCreditInterestEvent/Event[EVENTCODE='PER' and (EVENTTYPE='FLO' or EVENTTYPE='FIX') and (EventClass[EVENTCLASS='GRP' and substring(DTEVENT,1,10)=$pStepDate])]"/>

    <xsl:variable name="vStepInterestRate">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:choose>
            <xsl:when test="$vStepDebitInterestEvent/EVENTTYPE/text()='FLO'">
              <xsl:value-of select="$vStepDebitInterestEvent/Event[EVENTCODE='RES' and EVENTTYPE='FLO']/VALORISATION"/>
            </xsl:when>
            <xsl:when test="$vStepDebitInterestEvent/EVENTTYPE/text()='FIX'">
              <xsl:value-of select="$pDebitCalculation/fixedRateSchedule/initialValue/text()"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:choose>
            <xsl:when test="$vStepCreditInterestEvent/EVENTTYPE/text()='FLO'">
              <xsl:value-of select="$vStepCreditInterestEvent/Event[EVENTCODE='RES' and EVENTTYPE='FLO']/VALORISATION"/>
            </xsl:when>
            <xsl:when test="$vStepCreditInterestEvent/EVENTTYPE/text()='FIX'">
              <xsl:value-of select="$pCreditCalculation/fixedRateSchedule/initialValue/text()"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vStepMultiplier">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:if test="$pDebitCalculation/floatingRateCalculation">
            <xsl:value-of select="$pDebitCalculation/floatingRateCalculation/floatingRateMultiplierSchedule/initialValue/text()"/>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:if test="$pCreditCalculation/floatingRateCalculation">
            <xsl:value-of select="$pCreditCalculation/floatingRateCalculation/floatingRateMultiplierSchedule/initialValue/text()"/>
          </xsl:if>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vStepSpread">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:if test="$pDebitCalculation/floatingRateCalculation">
            <xsl:value-of select="$pDebitCalculation/floatingRateCalculation/spreadSchedule/initialValue/text()"/>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:if test="$pCreditCalculation/floatingRateCalculation">
            <xsl:value-of select="$pCreditCalculation/floatingRateCalculation/spreadSchedule/initialValue/text()"/>
          </xsl:if>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vStepInterestEffectiveRate">
      <xsl:if test="string-length($vStepInterestRate) >0">
        <!-- RD 20131223 [18582] Affichage du taux net. Exemple: 2.91 au lieu de 3.21 - 0.30-->
        <xsl:variable name="vRate" select="number($vStepInterestRate * 100)"/>
        <xsl:variable name="vRateMultiplier">
          <xsl:choose>
            <xsl:when test="string-length($vStepMultiplier) > 0">
              <xsl:value-of select="$vRate * number($vStepMultiplier)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vRate"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <!--<xsl:variable name="vRateSpread">
          <xsl:choose>
            <xsl:when test="string-length($vStepSpread) > 0">
              <xsl:value-of select="number($vRateMultiplier + ($vStepSpread * 100))"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vRateMultiplier"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="$vRateSpread"/>-->
        <xsl:choose>
          <xsl:when test="string-length($vStepSpread) > 0 and number($vStepSpread) !=0">
            <xsl:value-of select="number($vRateMultiplier) + (number($vStepSpread) * 100)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="number($vRateMultiplier)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <!-- RD 20130926 [18582] Taux d'intérêt négatif-->
    <xsl:variable name="vStepInterest">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:choose>
            <xsl:when test="0 > number($vStepInterestEffectiveRate) and
                $vStepDebitInterestEvent/EVENTTYPE/text()='FLO' and $pDebitCalculation/floatingRateCalculation/negativeInterestRateTreatment/text()='ZeroInterestRateMethod'">
              <xsl:value-of select="number('0')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number($vStepDebitInterestEvent/VALORISATION)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:choose>
            <xsl:when test="0 > number($vStepInterestEffectiveRate) and
                $vStepCreditInterestEvent/EVENTTYPE/text()='FLO' and $pCreditCalculation/floatingRateCalculation/negativeInterestRateTreatment/text()='ZeroInterestRateMethod'">
              <xsl:value-of select="number('0')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number($vStepCreditInterestEvent/VALORISATION)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vStepInterestDrCr">
      <xsl:choose>
        <xsl:when test="$pStepDebitNotional > 0">
          <xsl:call-template name="GetInterestDrCr">
            <xsl:with-param name="pStepInterestEvent" select="$vStepDebitInterestEvent"/>
            <xsl:with-param name="pIDB" select="$pIDB"/>
            <xsl:with-param name="pStepInterestEffectiveRate" select="$vStepInterestEffectiveRate"/>
            <xsl:with-param name="pNegativeInterestRateTreatment" select="$pDebitCalculation/floatingRateCalculation/negativeInterestRateTreatment/text()"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pStepCreditNotional > 0">
          <xsl:call-template name="GetInterestDrCr">
            <xsl:with-param name="pStepInterestEvent" select="$vStepCreditInterestEvent"/>
            <xsl:with-param name="pIDB" select="$pIDB"/>
            <xsl:with-param name="pStepInterestEffectiveRate" select="$vStepInterestEffectiveRate"/>
            <xsl:with-param name="pNegativeInterestRateTreatment" select="$pCreditCalculation/floatingRateCalculation/negativeInterestRateTreatment/text()"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vStepExInterest">
      <xsl:if test="string-length($pCurrencyFxRate) > 0 and number($pCurrencyFxRate) >= 0 and number($vStepInterest) >= 0">
        <xsl:call-template name="ExchangeAmount">
          <xsl:with-param name="pAmount" select="$vStepInterest" />
          <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <step date="{$pStepDate}"
          notional="{$vStepNotional}" notionalDrCr="{$vStepNotionalDrCr}"
          rate="{$vStepInterestRate}" multiplier="{$vStepMultiplier}" spread="{$vStepSpread}" effectiveRate="{$vStepInterestEffectiveRate}"
          interest="{$vStepInterest}" interestDrCr="{$vStepInterestDrCr}" exInterest="{$vStepExInterest}"/>
  </xsl:template>

  <!--GetInterestDrCr-->
  <xsl:template name="GetInterestDrCr">
    <xsl:param name="pStepInterestEvent"/>
    <xsl:param name="pIDB"/>
    <xsl:param name="pStepInterestEffectiveRate"/>
    <xsl:param name="pNegativeInterestRateTreatment"/>

    <xsl:choose>
      <xsl:when test="0 > number($pStepInterestEffectiveRate) and
                $pStepInterestEvent/EVENTTYPE/text()='FLO' and $pNegativeInterestRateTreatment='ZeroInterestRateMethod'">
        <xsl:value-of select="$gcDebit"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$pStepInterestEvent/IDB_PAY = $pIDB">
            <xsl:value-of select="$gcDebit"/>
          </xsl:when>
          <xsl:when test="$pStepInterestEvent/IDB_REC = $pIDB">
            <xsl:value-of select="$gcCredit"/>
          </xsl:when>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--GetUsedInterestRate-->
  <xsl:template name="GetUsedInterestRate">
    <xsl:param name="pRateCalculation"/>
    <xsl:param name="pDrCr"/>

    <xsl:if test="$pRateCalculation">
      <xsl:choose>
        <xsl:when test="$pRateCalculation/floatingRateCalculation">
          <usedInterestRate notionalDrCr="{$pDrCr}"
                            index="{$pRateCalculation/floatingRateCalculation/floatingRateIndex/text()}"
                            multiplier="{$pRateCalculation/floatingRateCalculation/floatingRateMultiplierSchedule/initialValue/text()}"
                            spread="{$pRateCalculation/floatingRateCalculation/spreadSchedule/initialValue/text()}"/>
        </xsl:when>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="$pRateCalculation/fixedRateSchedule">
          <usedInterestRate notionalDrCr="{$pDrCr}"
                            rate="{$pRateCalculation/fixedRateSchedule/initialValue/text()}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!--CreateGroups template-->
  <xsl:template name="CreateGroups">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />

    <xsl:variable name="vBookTrades">
      <xsl:choose>
        <xsl:when test="$pBookTrades">
          <xsl:copy-of select="$pBookTrades"/>
        </xsl:when>
        <xsl:when test="$gIsMonoBook = 'true'">
          <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade[BOOK/@data=$pBook]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <groups>
      <xsl:variable name="vBookTrade" select="msxsl:node-set($vBookTrades)/trade[position() = 1]" />

      <!--CBIBlockGroup-->
      <xsl:variable name="vCBIBlock" select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBIBlock']"/>

      <xsl:if test="$vCBIBlock">
        <group sort="{$vCBIBlock/@sort}" name="{$vCBIBlock/@name}"
               currencyPattern="{$vBookTrade/currency/@pattern}"
               exCurrencyPattern="{$vBookTrade/exCurrency/@pattern}">
          <trade href="{$vBookTrade/@id}">
            <sum>
              <xsl:copy-of select="$vBookTrade/interest"/>
                   
              <xsl:variable name="vDiffInterest">
                <xsl:choose>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0 and string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:call-template name="abs">
                      <xsl:with-param name="n" select="number($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount - $vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount)" />
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0">
                    <xsl:call-template name="abs">
                      <xsl:with-param name="n" select="number($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount)" />
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:call-template name="abs">
                      <xsl:with-param name="n" select="number($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount)" />
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="number('0')"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vDiffInterestNotionalDrCr">
                <xsl:choose>
                  <xsl:when test="number($vDiffInterest) = 0"/>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0 and string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:choose>
                      <xsl:when test="$vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount > $vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount">
                        <xsl:value-of select="$gcDebit"/>
                      </xsl:when>
                      <xsl:when test="$vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount > $vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount">
                        <xsl:value-of select="$gcCredit"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$gcCredit"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0">
                    <xsl:value-of select="$gcDebit"/>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:value-of select="$gcCredit"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gcCredit"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vDiffInterestDrCr">
                <xsl:choose>
                  <xsl:when test="number($vDiffInterest) = 0"/>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0 and string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:choose>
                      <xsl:when test="$vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount > $vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount">
                        <xsl:value-of select="$vBookTrade/interest[@notionalDrCr=$gcDebit]/@drCr"/>
                      </xsl:when>
                      <xsl:when test="$vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount > $vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount">
                        <xsl:value-of select="$vBookTrade/interest[@notionalDrCr=$gcCredit]/@drCr"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$gcCredit"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcDebit]/@amount) > 0">
                    <xsl:value-of select="$gcDebit"/>
                  </xsl:when>
                  <xsl:when test="string-length($vBookTrade/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
                    <xsl:value-of select="$gcCredit"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gcCredit"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              
              <xsl:variable name="vExDiffInterest">
                <xsl:if test="string-length($vBookTrade/exCurrency/@fxRate) > 0">
                  <xsl:call-template name="ExchangeAmount">
                    <xsl:with-param name="pAmount" select="$vDiffInterest" />
                    <xsl:with-param name="pFxRate" select="$vBookTrade/exCurrency/@fxRate" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:variable>

              <deltaInterest notionalDrCr="{$vDiffInterestNotionalDrCr}" 
                             currency="{$vBookTrade/currency/@currency}" amount="{$vDiffInterest}" drCr="{$vDiffInterestDrCr}"
                             exCurrency="{$vBookTrade/exCurrency/@currency}" exAmount="{$vExDiffInterest}"/>
            </sum>
          </trade>
        </group>
      </xsl:if>
    </groups>
  </xsl:template>

  <!-- ================================================== -->
  <!--        Display Template                            -->
  <!-- ================================================== -->
  <!--DisplayPageBody template-->
  <xsl:template name="DisplayPageBody">
    <xsl:param name="pBook" />
    <xsl:param name="pBookPosition" />
    <xsl:param name="pCountBook" />
    <xsl:param name="pSetLastPage" select="true()" />
    <xsl:param name="pGroups" />
    <xsl:param name="pIsDisplayStarLegend" />
    <xsl:param name="pIsHFormulaMandatory" select="true()" />

    <xsl:call-template name="DisplayPageBodyCommon">
      <xsl:with-param name="pBook" select="$pBook" />
      <xsl:with-param name="pBookPosition" select="$pBookPosition" />
      <xsl:with-param name="pCountBook" select="$pCountBook" />
      <xsl:with-param name="pSetLastPage" select="$pSetLastPage" />
      <xsl:with-param name="pGroups" select="$pGroups" />
      <xsl:with-param name="pIsDisplayStarLegend" select="$pIsDisplayStarLegend"/>
      <xsl:with-param name="pIsHFormulaMandatory" select="$pIsHFormulaMandatory"/>
    </xsl:call-template>
  </xsl:template>

  <!-- DisplaySummaryPageBody template -->
  <xsl:template name="DisplaySummaryPageBody"/>

  <!--DisplayPageBodyStart template-->
  <xsl:template name="DisplayPageBodyStart">
    <xsl:param name="pBook" />

    <xsl:call-template name="DisplayPageBodyStartCommon">
      <xsl:with-param name="pBook" select="$pBook"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:template match="trade" mode="display">
    <xsl:param name="pGroupColumnsToDisplay"/>

    <xsl:variable name="vCurrentTrade" select="current()"/>
    <xsl:variable name="vCurrentBusinessTrade" select="msxsl:node-set($gBusinessTrades)/trade[@id=$vCurrentTrade/@href]"/>
    <xsl:variable name="vCurrentDataDisplaySettings">
      <settings>
        <xsl:copy-of select="$vCurrentBusinessTrade/currency"/>
      </settings>
    </xsl:variable>

    <!-- ===== Display Trades details===== -->
    <xsl:apply-templates select="$vCurrentBusinessTrade/steps/step" mode="display">
      <xsl:sort select="@date" data-type="text"/>
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay"/>
    </xsl:apply-templates>
    <!-- ===== Display SubTotals===== -->
    <xsl:for-each select="msxsl:node-set($gColSubTotalColDisplaySettings)/group[@name='CBIBlock']/row">
      <xsl:sort select="@sort" data-type="number"/>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row font-size="{$gDetTrdFontSize}"
                    font-weight="normal"
                    text-align="right"
                    keep-with-previous="always">

        <xsl:for-each select="subtotal">
          <xsl:sort select="@sort" data-type="number"/>

          <xsl:call-template name="CreateEmptyCellsForDetailedTrades">
            <xsl:with-param name="pnNumberOfCells" select="@empty-columns" />
          </xsl:call-template>

          <fo:table-cell border="{$gDetTrdCellBorder}">

            <xsl:if test="string-length(@rows-spanned)>0">
              <xsl:attribute name="number-rows-spanned">
                <xsl:value-of select="number(@rows-spanned)" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="string-length(@columns-spanned)>0">
              <xsl:attribute name="number-columns-spanned">
                <xsl:value-of select="number(@columns-spanned)" />
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="string-length(@background-color)>0">
              <xsl:attribute name="background-color">
                <xsl:value-of select="@background-color" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="string-length(@border-bottom-style)>0">
              <xsl:attribute name="border-bottom-style">
                <xsl:value-of select="@border-bottom-style" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="string-length(@border-top-style)>0">
              <xsl:attribute name="border-top-style">
                <xsl:value-of select="@border-top-style" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="string-length(@border-left-style)>0">
              <xsl:attribute name="border-left-style">
                <xsl:value-of select="@border-left-style" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="string-length(@border-right-style)>0">
              <xsl:attribute name="border-right-style">
                <xsl:value-of select="@border-right-style" />
              </xsl:attribute>
            </xsl:if>


            <xsl:variable name="vSubTotalData">
              <xsl:call-template name="GetSubTotalData">
                <xsl:with-param name="pRowData" select="$vCurrentTrade"/>
                <xsl:with-param name="pSubTotalName" select="./@name"/>
                <xsl:with-param name="pDataDisplaySettings" select="$vCurrentDataDisplaySettings"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:choose>
              <xsl:when test="string-length($vSubTotalData) > 0">

                <xsl:attribute name="font-weight">
                  <xsl:choose>
                    <xsl:when test="string-length(@font-weight)>0">
                      <xsl:value-of select="@font-weight" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'bold'" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
                <xsl:attribute name="padding">
                  <xsl:value-of select="$gDetTrdCellPadding" />
                </xsl:attribute>
                <xsl:attribute name="text-align">
                  <xsl:choose>
                    <xsl:when test="string-length(@text-align)>0">
                      <xsl:value-of select="@text-align" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'right'" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>

                <xsl:if test="string-length(@font-size)>0">
                  <xsl:attribute name="font-size">
                    <xsl:value-of select="@font-size" />
                  </xsl:attribute>
                </xsl:if>
                <xsl:if test="string-length(@display-align)>0">
                  <xsl:attribute name="display-align">
                    <xsl:value-of select="@display-align" />
                  </xsl:attribute>
                </xsl:if>

                <fo:block>
                  <xsl:value-of select="$vSubTotalData"/>
                </fo:block>
              </xsl:when>
              <xsl:otherwise>
                <fo:block>
                  <xsl:value-of select="$gcEspace"/>
                </fo:block>
              </xsl:otherwise>
            </xsl:choose>

          </fo:table-cell>
        </xsl:for-each>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="step" mode="display">
    <xsl:param name="pGroupColumnsToDisplay"/>

    <!-- EG 20160404 Migration vs2013 -->
    <fo:table-row font-size="{$gDetTrdFontSize}" font-weight="normal">
      <xsl:apply-templates select="$pGroupColumnsToDisplay[string-length(@master-column)=0]" mode="display-data">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pRowData" select="current()" />
        <xsl:with-param name="pParentRowData" select="current()/.." />
      </xsl:apply-templates>
    </fo:table-row>
  </xsl:template>

  <!--DisplayBookAdditionalDetail template-->
  <xsl:template name="DisplayBookAdditionalDetail">
    <fo:table-row font-weight="normal">
      <fo:table-cell margin-left="{$gReceiverAdressLeftMargin}" border="{$gcTableBorderDebug}" text-align="{$gReceiverBookTextAlign}">
        <fo:block>
          <xsl:choose>
            <xsl:when test="string-length($gReportHeaderFooter/hDtBusinessLabel/text())>0">
              <xsl:value-of select="$gReportHeaderFooter/hDtBusinessLabel/text()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'TS-PeriodFrom'" />
              </xsl:call-template>
              <xsl:value-of select ="string(' ')"/>
            </xsl:otherwise>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell margin-left="{$gReceiverAdressLeftMargin}" border="{$gcTableBorderDebug}" text-align="{$gReceiverBookTextAlign}">
        <fo:block font-weight="bold">
          <xsl:variable name="vFirstTrade" select="msxsl:node-set($gBusinessTrades)/trade[position() = 1]"/>
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="$vFirstTrade/@startDate" />
          </xsl:call-template>
          <fo:inline font-weight="normal">
            <xsl:value-of select ="string(' ')"/>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-To'" />
            </xsl:call-template>
            <xsl:value-of select ="string(' ')"/>
          </fo:inline>
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="$vFirstTrade/@endDate" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <xsl:template match="column" mode="display-getcolumn">
    <xsl:param name="pGroup"/>

    <xsl:call-template name="GetColumnToDisplay">
      <xsl:with-param name="pGroup" select="$pGroup" />
    </xsl:call-template>
  </xsl:template>

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!--GetColumnDataToDisplay-->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />
    <xsl:param name="pIsGetDetail" select="false()" />
    <xsl:param name="pParentRowData" />

    <xsl:variable name="vColumnName" select="@name"/>
    <xsl:variable name="vBusinessTrade" select="msxsl:node-set($gBusinessTrades)/trade[@id=$pRowData/trade/@href]" />

    <xsl:choose>
      <!-- ===== CurrencyTitle ===== -->
      <xsl:when test="$vColumnName='TS-CurrencyTitle'">
        <xsl:choose>
          <xsl:when test="string-length($vBusinessTrade/currency/@desc) > 0">
            <xsl:value-of select="$vBusinessTrade/currency/@desc" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$vBusinessTrade/currency/@currency" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FxRateTitle ===== -->
      <xsl:when test="$vColumnName='TS-FxRateTitle'">
        <xsl:variable name="vFxRate">
          <xsl:if test="$vBusinessTrade/exCurrency/@fxRate">
            <xsl:value-of select="$vBusinessTrade/exCurrency/@fxRate"/>
          </xsl:if>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="string-length($vFxRate) = 0 or $vBusinessTrade/currency/@currency = $vBusinessTrade/exCurrency/@currency">
            <xsl:value-of select="''"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-FxRateTitle'" />
            </xsl:call-template>
            <xsl:value-of select="concat($gcEspace,$vBusinessTrade/exCurrency/@currency,'/',$vBusinessTrade/currency/@currency,':',$gcEspace)" />
            <xsl:choose>
              <xsl:when test="number($vFxRate) = number(-1)">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="number($vFxRate)" />
                  <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== RateTitle ===== -->
      <xsl:when test="$vColumnName='TS-RateTitle'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="concat($vColumnName,'')" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== DrInterestTitle ===== -->
      <xsl:when test="$vColumnName='TS-DebitBalanceTitle'">
        <xsl:call-template name="DisplayInterestRate">
          <xsl:with-param name="pInterestRate" select="$vBusinessTrade/usedInterestRate[@notionalDrCr=$gcDebit]" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== CrInterestTitle ===== -->
      <xsl:when test="$vColumnName='TS-CreditBalanceTitle'">
        <xsl:call-template name="DisplayInterestRate">
          <xsl:with-param name="pInterestRate" select="$vBusinessTrade/usedInterestRate[@notionalDrCr=$gcCredit]" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== ThresholdTitle ===== -->
      <xsl:when test="$vColumnName='TS-ThresholdTitle'">
        <xsl:if test="$vBusinessTrade/threshold">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="concat($vColumnName,'')" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== DrThresholdTitle ===== -->
      <xsl:when test="$vColumnName='TS-DrThresholdTitle'">
        <xsl:value-of select="$vBusinessTrade/threshold[@notionalDrCr=$gcDebit]/@amount" />
      </xsl:when>
      <!-- ===== CrThresholdTitle ===== -->
      <xsl:when test="$vColumnName='TS-CrThresholdTitle'">
        <xsl:value-of select="$vBusinessTrade/threshold[@notionalDrCr=$gcCredit]/@amount" />
      </xsl:when>
      <!-- ===== ValueDate ===== -->
      <xsl:when test="$vColumnName='TS-ValueDate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-shortdate_ddMMMyy">
              <xsl:with-param name="year" select="substring($pRowData/@date , 1, 4)" />
              <xsl:with-param name="month" select="substring($pRowData/@date , 6, 2)" />
              <xsl:with-param name="day" select="substring($pRowData/@date , 9, 2)" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CashBalance ===== -->
      <xsl:when test="$vColumnName='Report-CashBalance'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="string-length($pRowData/@notional) > 0">
                <xsl:value-of select="$pRowData/@notional"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CashBalance DrCr===== -->
      <xsl:when test="$vColumnName='Report-CashBalanceDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/@notionalDrCr" />
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Rate ===== -->
      <xsl:when test="$vColumnName='TS-Rate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@effectiveRate) >0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="number($pRowData/@effectiveRate)" />
                <xsl:with-param name="pAmountPattern" select="$pParentRowData/@ratePattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DrInterest ===== -->
      <xsl:when test="$vColumnName='TS-DrInterest'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@interest) >0 and $pRowData/@notionalDrCr=$gcDebit">
              <xsl:value-of select="$pRowData/@interest"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DrInterestDrCr ===== -->
      <xsl:when test="$vColumnName='TS-DrInterestDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@interest) >0 and number($pRowData/@interest) >0 and $pRowData/@notionalDrCr=$gcDebit">
              <xsl:value-of select="$pRowData/@interestDrCr"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CrInterest ===== -->
      <xsl:when test="$vColumnName='TS-CrInterest'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@interest) >0 and $pRowData/@notionalDrCr=$gcCredit">
              <xsl:value-of select="$pRowData/@interest"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== CrInterestDrCr ===== -->
      <xsl:when test="$vColumnName='TS-CrInterestDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@interest) >0 and number($pRowData/@interest) >0 and $pRowData/@notionalDrCr=$gcCredit">
              <xsl:value-of select="$pRowData/@interestDrCr"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== ExInterest ===== -->
      <xsl:when test="$vColumnName='TS-ExInterest'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@exInterest) >0">
              <xsl:value-of select="$pRowData/@exInterest"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== ExInterest DrCr===== -->
      <xsl:when test="$vColumnName='TS-ExInterestDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/@exInterest) >0 and number($pRowData/@exInterest) >0">
              <xsl:value-of select="$pRowData/@interestDrCr" />
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DisplayInterestRate">
    <xsl:param name="pInterestRate"/>

    <xsl:choose>
      <xsl:when test="$pInterestRate/@rate">
        <xsl:if test="string-length($pInterestRate/@rate) >0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="concat(number($pInterestRate/@rate * 100),'')" />
            <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
          </xsl:call-template>
          <xsl:value-of select="' %'"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pInterestRate/@index"/>
        <xsl:if test="string-length($pInterestRate/@multiplier) > 0">
          <xsl:value-of select="' * '"/>
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="number($pInterestRate/@multiplier)" />
            <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="string-length($pInterestRate/@spread) > 0 and number($pInterestRate/@spread) !=0">
          <xsl:choose>
            <xsl:when test="number($pInterestRate/@spread) >0">
              <xsl:value-of select="' + '"/>
            </xsl:when>
            <xsl:when test="0 > number($pInterestRate/@spread)">
              <xsl:value-of select="' - '"/>
            </xsl:when>
          </xsl:choose>
          <xsl:variable name="vAbsSpread">
            <xsl:call-template name="abs">
              <xsl:with-param name="n" select="number($pInterestRate/@spread * 100)" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$vAbsSpread" />
            <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
          </xsl:call-template>
          <xsl:value-of select="' %'"/>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--GetCommonSerieSubTotalData template-->
  <xsl:template name="GetSubTotalData">
    <xsl:param name="pRowData" />
    <xsl:param name="pSubTotalName"/>
    <xsl:param name="pDataDisplaySettings" />

    <xsl:variable name="vDataDisplaySettings" select="msxsl:node-set($pDataDisplaySettings)/settings"/>
    <xsl:variable name="vDecPattern">
      <xsl:choose>
        <xsl:when test="string-length($vDataDisplaySettings/currency/@pattern) > 0">
          <xsl:value-of select="$vDataDisplaySettings/currency/@pattern"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$number2DecPattern"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <!-- ===== SubTotInterest ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotInterest'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="concat($pSubTotalName,'')" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== SubTotDrIntCurrency ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDrIntCurrency'">
        <xsl:value-of select="$pRowData/sum/interest[@notionalDrCr=$gcDebit]/@currency" />
      </xsl:when>
      <!-- ===== SubTotDrIntAmount ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDrIntAmount'">
        <xsl:choose>
          <xsl:when test="string-length($pRowData/sum/interest[@notionalDrCr=$gcDebit]/@amount) > 0">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="number($pRowData/sum/interest[@notionalDrCr=$gcDebit]/@amount)" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="number('0')" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SubTotDrIntAmountDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDrIntAmountDrCr'">
        <xsl:if test="string-length($pRowData/sum/interest[@notionalDrCr=$gcDebit]/@amount) > 0 and number($pRowData/sum/interest[@notionalDrCr=$gcDebit]/@amount) >0">
          <xsl:value-of select="$pRowData/sum/interest[@notionalDrCr=$gcDebit]/@drCr"/>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotCrIntCurrency ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotCrIntCurrency'">
        <xsl:value-of select="$pRowData/sum/interest[@notionalDrCr=$gcCredit]/@currency" />
      </xsl:when>
      <!-- ===== SubTotCrIntAmount ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotCrIntAmount'">
        <xsl:choose>
          <xsl:when test="string-length($pRowData/sum/interest[@notionalDrCr=$gcCredit]/@amount) > 0">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="number($pRowData/sum/interest[@notionalDrCr=$gcCredit]/@amount)" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="number('0')" />
              <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SubTotCrIntAmountDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotCrIntAmountDrCr'">
        <xsl:if test="string-length($pRowData/sum/interest[@notionalDrCr=$gcCredit]/@amount) > 0 and number($pRowData/sum/interest[@notionalDrCr=$gcCredit]/@amount) >0">
          <xsl:value-of select="$pRowData/sum/interest[@notionalDrCr=$gcCredit]/@drCr"/>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaInterest ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaInterest'">
        <xsl:choose>
          <xsl:when test="string-length($pRowData/sum/deltaInterest/@drCr) = 0 or $pRowData/sum/deltaInterest/@drCr=$gcDebit">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-TotDrInterest'" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pRowData/sum/deltaInterest/@drCr=$gcCredit">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-TotCrInterest'" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SubTotDeltaDrIntCurrency ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaDrIntCurrency'">
        <xsl:if test="string-length($pRowData/sum/deltaInterest/@notionalDrCr) = 0 or $pRowData/sum/deltaInterest/@notionalDrCr=$gcDebit">
          <xsl:value-of select="$pRowData/sum/deltaInterest/@currency" />
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaDrIntAmount ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaDrIntAmount'">
        <xsl:if test="string-length($pRowData/sum/deltaInterest/@notionalDrCr) = 0 or $pRowData/sum/deltaInterest/@notionalDrCr=$gcDebit">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="number($pRowData/sum/deltaInterest/@amount)" />
            <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaDrIntDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaDrIntDrCr'">
        <xsl:if test="(string-length($pRowData/sum/deltaInterest/@notionalDrCr) > 0 or $pRowData/sum/deltaInterest/@notionalDrCr=$gcDebit) 
                and number($pRowData/sum/deltaInterest/@amount) >0">
          <xsl:value-of select="$pRowData/sum/deltaInterest/@drCr"/>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaCrIntCurrency ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaCrIntCurrency'">
        <xsl:if test="$pRowData/sum/deltaInterest/@notionalDrCr=$gcCredit">
          <xsl:value-of select="$pRowData/sum/deltaInterest/@currency" />
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaCrIntAmount ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaCrIntAmount'">
        <xsl:if test="$pRowData/sum/deltaInterest/@notionalDrCr=$gcCredit">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="number($pRowData/sum/deltaInterest/@amount)" />
            <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaCrIntDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaCrIntDrCr'">
        <xsl:if test="$pRowData/sum/deltaInterest/@notionalDrCr=$gcCredit and number($pRowData/sum/deltaInterest/@amount) >0">
          <xsl:value-of select="$pRowData/sum/deltaInterest/@drCr"/>
        </xsl:if>
      </xsl:when>
      <!-- ===== SubTotDeltaIntExCurrency ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaIntExCurrency'">
        <xsl:value-of select="$pRowData/sum/deltaInterest/@exCurrency" />
      </xsl:when>
      <!-- ===== SubTotDeltaIntExAmount ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaIntExAmount'">
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="number($pRowData/sum/deltaInterest/@exAmount)" />
          <xsl:with-param name="pAmountPattern" select="$vDecPattern" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== SubTotDeltaIntExDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-SubTotDeltaIntExDrCr' and number($pRowData/sum/deltaInterest/@exAmount) >0">
        <xsl:value-of select="$pRowData/sum/deltaInterest/@drCr" />
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--GetColumnIsAltData-->
  <xsl:template name="GetColumnIsAltData"/>
  <!-- GetSpecificDataFontWeight -->
  <xsl:template name="GetSpecificDataFontWeight"/>
  <!-- ===== GetContractDataToDisplay ===== -->
  <xsl:template name="GetContractDataToDisplay"/>

  <xsl:template name="GetColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vValue">
      <xsl:call-template name="GetSpecificColumnDynamicData">
        <xsl:with-param name="pValue" select="$pValue"/>
        <xsl:with-param name="pRowData" select="$pRowData"/>
        <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="GetComonColumnDynamicData">
      <xsl:with-param name="pValue" select="$vValue"/>
      <xsl:with-param name="pRowData" select="$pRowData"/>
      <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
    </xsl:call-template>
  </xsl:template>

  <!-- GetSpecificColumnDynamicData -->
  <xsl:template name="GetSpecificColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vColumnName" select="@name"/>
    <xsl:variable name="vBusinessTrade" select="msxsl:node-set($gBusinessTrades)/trade[@id=$pParentRowData/trade/@href]" />

    <xsl:choose>
      <!-- ===== Flow Title ===== -->
      <xsl:when test="contains($pValue,'%%RES:Report-CashBalance%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:choose>
            <xsl:when test="$vBusinessTrade/@isCashCoveredInitialMargin = 'true'">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-CashCollateral'" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-CashBalance'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%RES:Report-CashBalance%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
